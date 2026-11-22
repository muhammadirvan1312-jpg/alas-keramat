using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.Building
{
    // 05 §5: Panci di atas api unggun menyala (<= 2 m): 1 Botol Air Bersih + 2 bahan -> 40 s -> 2 Sup Hutan (+ botol kosong kembali).
    public class PanciMasak : MonoBehaviour, IInteractable
    {
        public const float LamaSup = 40f, JarakApi = 2.2f;
        public static readonly ItemID[] Bahan = { ItemID.DagingMentah, ItemID.DagingMatang, ItemID.IkanMentah, ItemID.IkanMatang, ItemID.Jamur, ItemID.Pisang, ItemID.JaheHutan, ItemID.TelurAyamHutan };
        float _t = -1f;
        Transform _kuahTrans;
        MeshRenderer _kuahMr;
        ParticleSystem _uapPanci;

        public static float ProgresMasak01(float waktu)
        {
            return waktu < 0f ? 0f : Mathf.Clamp01(waktu / LamaSup);
        }

        [System.Serializable] class IsiPanciT6 { public float waktu; }
        public string CaptureIsi() { return JsonUtility.ToJson(new IsiPanciT6 { waktu=_t }); }
        public void RestoreIsi(string json) { var d=JsonUtility.FromJson<IsiPanciT6>(json); if(d!=null) _t=d.waktu; }
        public float HoldDuration { get { return 0f; } }

        bool AdaApi() { return ApiUnggun.AdaApiDekat(transform.position, JarakApi); }

        void PastikanVisualMasak()
        {
            if (_kuahTrans != null) return;
            var kuah = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(kuah.GetComponent<Collider>());
            kuah.name = "KuahSup";
            kuah.transform.SetParent(transform, false);
            kuah.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            kuah.transform.localScale = new Vector3(0.32f, 0.015f, 0.32f);
            _kuahMr = kuah.GetComponent<MeshRenderer>();
            _kuahMr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _kuahTrans = kuah.transform;

            var uapGo = new GameObject("UapPanci");
            uapGo.transform.SetParent(transform, false);
            uapGo.transform.localPosition = new Vector3(0f, 0.28f, 0f);
            _uapPanci = uapGo.AddComponent<ParticleSystem>();
            var m = _uapPanci.main;
            m.loop = true; m.startLifetime = 1.3f; m.startSpeed = 0.45f; m.startSize = 0.14f;
            m.maxParticles = 18; m.gravityModifier = -0.08f; m.simulationSpace = ParticleSystemSimulationSpace.World;
            m.startColor = new Color(0.92f, 0.90f, 0.85f, 0.32f);
            var em = _uapPanci.emission; em.rateOverTime = 6f;
            var sh = _uapPanci.shape; sh.shapeType = ParticleSystemShapeType.Circle; sh.radius = 0.12f;
            var pr = uapGo.GetComponent<ParticleSystemRenderer>();
            pr.sharedMaterial = SafeMaterialFactory.CreatePBR("M_UapPanci", new Color(0.92f, 0.90f, 0.86f, 0.30f), 0f, 0.1f, null, null, null, SurfaceMode.Transparent);
            pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        void Update()
        {
            bool api = AdaApi();
            if (_t >= 0f && _t < LamaSup && api) _t += Time.deltaTime;
            bool adaIsi = _t >= 0f;
            if (adaIsi) PastikanVisualMasak();
            if (_kuahTrans != null)
            {
                if (_kuahTrans.gameObject.activeSelf != adaIsi) _kuahTrans.gameObject.SetActive(adaIsi);
                if (adaIsi && _kuahMr != null)
                {
                    float p = ProgresMasak01(_t);
                    Color w = Color.Lerp(new Color(0.38f, 0.46f, 0.42f), new Color(0.76f, 0.54f, 0.22f), p);
                    _kuahMr.sharedMaterial = SafeMaterialFactory.GetCached("M_KuahSup_" + Mathf.RoundToInt(p * 10f), w, 0f, 0.78f);
                }
            }
            if (_uapPanci != null)
            {
                bool ngepul = adaIsi && api;
                if (ngepul && !_uapPanci.isPlaying) _uapPanci.Play();
                else if (!ngepul && _uapPanci.isPlaying) _uapPanci.Stop();
            }
        }

        public static int HitungBahan(TikarPandan3DInventory inv) { int n = 0; foreach (var b in Bahan) n += inv.Jumlah(b); return n; }

        public string GetPromptText(GameObject actor)
        {
            if (_t >= LamaSup) return "AMBIL SUP HUTAN";
            if (_t >= 0f) return AdaApi() ? "" : "PANCI BUTUH API";
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv == null || !AdaApi()) return "";
            return inv.Punya(ItemID.BotolAirBersih) && HitungBahan(inv) >= 2 ? "MASAK SUP" : "";
        }

        public bool CanInteract(GameObject actor) { string p = GetPromptText(actor); return p.Length > 0 && p != "PANCI BUTUH API"; }

        public void Interact(GameObject actor)
        {
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv == null) return;
            if (_t >= LamaSup) { if (inv.Tambah(ItemID.SupHutan, 2) > 0) { inv.Tambah(ItemID.BotolKosong, 1); _t = -1f; GameEvents.RaiseToast("+2 Sup Hutan"); } return; }
            if (!inv.Kurangi(ItemID.BotolAirBersih, 1)) return;
            int ambil = 0;
            foreach (var b in Bahan) while (ambil < 2 && inv.Kurangi(b, 1)) ambil++;
            _t = 0f;
            GameEvents.RaiseToast("Sup mulai dimasak");
        }
    }
}
