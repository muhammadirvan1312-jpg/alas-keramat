using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.Building
{
    // 07 §1-2: pohon interaktif (proxy collider di atas instance vegetasi). Takikan 4 kuadran dunia U/T/S/B;
    // Σ takikan >= HP -> rebah menjauhi kuadran terdalam. Bambu, pisang, aren, mahoni, beringin punya perilaku khusus.
    public class DirectionalTree : MonoBehaviour
    {
        public const int Utara = 0, Timur = 1, Selatan = 2, Barat = 3;
        public const float TinggiTakik = 0.9f, PeluangDamar = 0.25f;
        static readonly Vector3[] Sumbu = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };

        public VegetationManager.InfoPohon Info;
        public readonly float[] CutDepth = new float[4];
        readonly Transform[] _visualTakik = new Transform[4];
        int _pukulan, _seratAren, _damar, _batangBambu = -1;

        public float HP { get { return HPTebang(Info.jenis); } }
        public float TotalTakik { get { return CutDepth[0] + CutDepth[1] + CutDepth[2] + CutDepth[3]; } }

        public static float KedalamanTakik01(float cutKuadran, float hpTotal)
        {
            if (cutKuadran <= 0f || hpTotal <= 0f) return 0f;
            return Mathf.Clamp01(cutKuadran / Mathf.Max(1f, hpTotal * 0.45f));
        }

        void PerbaruiVisualTakik(int q)
        {
            if (q < 0 || q >= 4) return;
            float d01 = KedalamanTakik01(CutDepth[q], HP);
            if (d01 <= 0f) return;
            if (_visualTakik[q] == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(go.GetComponent<Collider>());
                go.name = "Takik_" + q;
                go.transform.SetParent(transform, false);
                var mr = go.GetComponent<MeshRenderer>();
                mr.sharedMaterial = SafeMaterialFactory.GetCached("M_TakikKayuDalam", new Color(0.78f, 0.58f, 0.34f), 0f, 0.18f);
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _visualTakik[q] = go.transform;
            }
            var t = _visualTakik[q];
            t.gameObject.SetActive(true);
            Vector3 arah = Sumbu[q];
            float rBatang = 0.24f * Mathf.Max(0.75f, Info.skala);
            t.localPosition = Vector3.up * TinggiTakik + arah * (rBatang * (1f - d01 * 0.38f));
            t.localRotation = Quaternion.LookRotation(arah, Vector3.up) * Quaternion.Euler(18f, 0f, 0f);
            t.localScale = new Vector3(Mathf.Lerp(0.14f, 0.34f, d01), Mathf.Lerp(0.07f, 0.16f, d01), Mathf.Lerp(0.08f, 0.24f, d01));
        }

        void BersihkanVisualTakik()
        {
            for (int i = 0; i < 4; i++)
                if (_visualTakik[i] != null) _visualTakik[i].gameObject.SetActive(false);
        }

        void OnDisable() { BersihkanVisualTakik(); }

        public static float HPTebang(ProcPohon.Jenis j)
        {
            switch (j)
            {
                case ProcPohon.Jenis.Jati: return 120f;
                case ProcPohon.Jenis.Mahoni: return 140f;
                case ProcPohon.Jenis.Kelapa: return 70f;
                case ProcPohon.Jenis.Aren: return 90f;
                case ProcPohon.Jenis.SawitHangus: return 50f;
                default: return 0f;
            }
        }

        public static int JumlahKayu(ProcPohon.Jenis j)
        {
            switch (j)
            {
                case ProcPohon.Jenis.Jati: case ProcPohon.Jenis.Mahoni: return 4;
                case ProcPohon.Jenis.Aren: return 3;
                case ProcPohon.Jenis.Kelapa: case ProcPohon.Jenis.SawitHangus: return 2;
                default: return 0;
            }
        }

        public static bool BisaDitebang(ProcPohon.Jenis j) { return HPTebang(j) > 0f; }
        public static bool Interaktif(ProcPohon.Jenis j) { return BisaDitebang(j) || j == ProcPohon.Jenis.BambuRumpun || j == ProcPohon.Jenis.Pisang || j == ProcPohon.Jenis.BeringinKecil; }

        public static int Kuadran(Vector3 arahKeAndi)
        {
            int best = 0; float bd = -2f;
            arahKeAndi.y = 0f; arahKeAndi.Normalize();
            for (int i = 0; i < 4; i++) { float d = Vector3.Dot(arahKeAndi, Sumbu[i]); if (d > bd) { bd = d; best = i; } }
            return best;
        }

        // Arah rebah = berlawanan dengan kuadran takikan terdalam; seri -> menjauhi Andi.
        public static Vector3 ArahRebah(float[] cut, Vector3 arahKeAndi)
        {
            int best = 0; bool seri = false;
            for (int i = 1; i < 4; i++)
            {
                if (cut[i] > cut[best] + 0.001f) { best = i; seri = false; }
                else if (Mathf.Abs(cut[i] - cut[best]) <= 0.001f) seri = true;
            }
            if (seri) { var d = -arahKeAndi; d.y = 0f; return d.sqrMagnitude > 0.0001f ? d.normalized : Vector3.forward; }
            return -Sumbu[best];
        }

        // Dipanggil CombatAndLeftHandController. Mengembalikan true bila pukulan mengenai sesuatu yang bermakna.
        public bool Pukul(Vector3 posisiAndi, float damageTebang)
        {
            var inv = GameServices.Get<TikarPandan3DInventory>();
            var synth = GameServices.Get<ProceduralAudioSynth>();
            Vector3 titik = Info.posisi + Vector3.up * TinggiTakik;
            if (synth != null) synth.PlayWoodChop(titik);
            _pukulan++;
            switch (Info.jenis)
            {
                case ProcPohon.Jenis.BeringinKecil:
                    var v = GameServices.Get<Player.AndiVitalsAndSanity>();
                    if (v != null) v.UbahSanity(-5f);
                    if (synth != null) synth.PlayKuntilanakLaugh(titik + Vector3.up * 6f);
                    GameEvents.RaiseToast("Beringin itu tidak boleh dilukai...");
                    return true;
                case ProcPohon.Jenis.Pisang:
                    Beri(inv, ItemID.DaunPisang, 3); Beri(inv, ItemID.Pisang, Random.Range(1, 3));
                    Hilang();
                    return true;
                case ProcPohon.Jenis.BambuRumpun:
                    if (_batangBambu < 0) _batangBambu = Random.Range(8, 13);
                    if (_pukulan % 2 == 0) { Beri(inv, ItemID.Bambu, 1); _batangBambu--; if (_batangBambu <= 0) Hilang(); }
                    return true;
            }
            if (!BisaDitebang(Info.jenis) || damageTebang <= 0f) return false;
            if (Info.jenis == ProcPohon.Jenis.Aren && _seratAren < 3) { _seratAren++; Beri(inv, ItemID.SeratIjuk, 1); }
            if (Info.jenis == ProcPohon.Jenis.Mahoni && _damar < 2 && Random.value < PeluangDamar) { _damar++; Beri(inv, ItemID.Damar, 1); }
            Vector3 keAndi = posisiAndi - Info.posisi;
            int q = Kuadran(keAndi);
            CutDepth[q] += damageTebang;
            PerbaruiVisualTakik(q);
            if (TotalTakik >= HP) Rebah(ArahRebah(CutDepth, keAndi));
            return true;
        }

        static void Beri(TikarPandan3DInventory inv, ItemID id, int n)
        {
            if (inv == null) return;
            int masuk = inv.Tambah(id, n);
            GameEvents.RaiseToast(masuk > 0 ? "+" + masuk + " " + ItemDatabase.Get(id).displayName : ItemDatabase.Get(id).displayName + ": tas penuh");
        }

        void Hilang()
        {
            BersihkanVisualTakik();
            var vm = GameServices.Get<VegetationManager>();
            if (vm != null) vm.Sembunyikan(Info.id);
            var pi = GameServices.Get<PohonInteraktif>();
            if (pi != null) pi.Lepas(this);
        }

        void Rebah(Vector3 arah)
        {
            var vm = GameServices.Get<VegetationManager>();
            var j = vm != null ? vm.Jenis(Info.jenisVeg) : null;
            Hilang();
            if (j != null) PohonRebah.Buat(Info, j, arah);
        }
    }
}
