using System.Collections;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;
namespace AlasKeramat.Core
{
    // Jangkar zipline Jurang Kapur (03 Zona 6, 04 §4): TEMBAK TALI dengan Zipline Gun (1 Tali Zipline), lalu MELUNCUR antar-jangkar.
    public class JangkarZipline : MonoBehaviour, IInteractable
    {
        public const float Laju = 12f, Gantung = 1.7f, LendutTali = 1.15f;
        public const int SegmenTali = 12;
        [SerializeField] JangkarZipline pasangan;

        public static Vector3 HitungTitikTaliZipline(Vector3 a, Vector3 b, float t01, float lendut)
        {
            float u = Mathf.Clamp01(t01);
            return Vector3.Lerp(a, b, u) - Vector3.up * (4f * u * (1f - u) * lendut);
        }
        [SerializeField] bool utama;
        [SerializeField] LineRenderer tali;
        int _versi = -1;
        static bool _meluncur;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetLuncur() { _meluncur = false; }

        public void Pasang(JangkarZipline lawan, bool jangkarUtama, LineRenderer garis) { pasangan = lawan; utama = jangkarUtama; tali = garis; }
        public Vector3 Titik { get { return transform.position; } }
        public float HoldDuration { get { return 0.3f; } }
        WorldCaveAndSaveManager M { get { return GameServices.Get<WorldCaveAndSaveManager>(); } }
        bool AdaTali { get { var m = M; return m != null && m.Ada(WorldCaveAndSaveManager.B_Tali); } }

        public string GetPromptText(GameObject actor)
        {
            if (_meluncur || pasangan == null) return "";
            if (AdaTali) return "MELUNCUR";
            return utama ? "TEMBAK TALI" : "";
        }
        public bool CanInteract(GameObject actor) { return !_meluncur && pasangan != null && (AdaTali || utama); }
        public void Interact(GameObject actor)
        {
            var m = M; if (m == null || pasangan == null) return;
            if (!AdaTali)
            {
                var inv = GameServices.Get<TikarPandan3DInventory>();
                if (inv == null || !inv.Punya(ItemID.ZiplineGun)) { GameEvents.RaiseToast("Butuh Zipline Gun untuk menembakkan tali ke seberang."); return; }
                if (!inv.Kurangi(ItemID.TaliZipline, 1)) { GameEvents.RaiseToast("Butuh Tali Zipline."); return; }
                m.Set(WorldCaveAndSaveManager.B_Tali);
                GameEvents.RaiseToast("Tali terpasang ke seberang jurang.");
                return;
            }
            var andi = GameServices.Get<AndiFPSController>();
            if (andi != null) StartCoroutine(Luncur(andi));
        }

        IEnumerator Luncur(AndiFPSController andi)
        {
            _meluncur = true;
            Vector3 a = Titik - Vector3.up * Gantung, b = pasangan.Titik - Vector3.up * Gantung;
            float lama = Mathf.Max(0.5f, Vector3.Distance(a, b) / Laju);
            var percikGo = new GameObject("PercikanZipline");
            var ps = percikGo.AddComponent<ParticleSystem>();
            var pm = ps.main; pm.loop = true; pm.startLifetime = 0.28f; pm.startSpeed = 2.6f; pm.startSize = 0.04f;
            pm.maxParticles = 24; pm.gravityModifier = 1.2f; pm.simulationSpace = ParticleSystemSimulationSpace.World;
            pm.startColor = new Color(1f, 0.78f, 0.28f);
            var pem = ps.emission; pem.rateOverTime = 20f;
            var psh = ps.shape; psh.shapeType = ParticleSystemShapeType.Sphere; psh.radius = 0.08f;
            var ppr = percikGo.GetComponent<ParticleSystemRenderer>();
            ppr.sharedMaterial = SafeMaterialFactory.CreatePBR("M_PercikanZipline", new Color(1f, 0.8f, 0.3f), 0f, 0.8f, null, null, new Color(3.2f, 1.8f, 0.4f));
            ppr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            andi.DudukDi(a);
            for (float t = 0f; t < lama; t += Time.deltaTime)
            {
                float u = Mathf.SmoothStep(0f, 1f, t / lama);
                Vector3 pos = HitungTitikTaliZipline(a, b, u, LendutTali);
                andi.Teleport(pos);
                percikGo.transform.position = pos + Vector3.up * Gantung;
                yield return null;
            }
            Destroy(percikGo);
            Vector3 maju = b - a; maju.y = 0f;
            andi.Berdiri();
            andi.Teleport(b + (maju.sqrMagnitude > 0.01f ? maju.normalized * 1.5f : Vector3.zero));
            andi.GuncangKamera(0.12f);
            _meluncur = false;
        }

        void Update()
        {
            var m = M; if (m == null || m.Versi == _versi) return;
            _versi = m.Versi;
            if (tali != null && pasangan != null)
            {
                tali.enabled = AdaTali;
                tali.positionCount = SegmenTali;
                for (int i = 0; i < SegmenTali; i++)
                    tali.SetPosition(i, HitungTitikTaliZipline(Titik, pasangan.Titik, i / (float)(SegmenTali - 1), LendutTali));
            }
        }
    }
}
