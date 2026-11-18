using System.Collections;
using System.Collections.Generic;
using AlasKeramat.AI;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.UI;
namespace AlasKeramat.Core
{
    // Tiga kompleks bawah tanah (03 §10, pusat v5 20 §2.3): pindah dengan fade 1,2 s, Mode Dalam Ruangan, maks 4 lampu aktif,
    // napas 60 s dengan Rebreather, bendera gerbang tersimpan, kejadian terskrip (Pocong jatuh, penjaga, alarm bunker).
    public partial class WorldCaveAndSaveManager : MonoBehaviour, ISaveable
    {
        public const float LamaFade = 1.2f, JarakPandangGua = 80f, KabutGua = 0.035f, TickDetik = 0.5f, NapasRebreather = 60f;
        public const int MaksLampu = 4, GaliPerlu = 4;
        public const int B_Palka = 1, B_PintuKartu = 2, B_Rintangan = 4, B_Tali = 8, B_AkarK4 = 16, B_Alarm = 32, B_PocongJatuh = 64,
            B_PenjagaR4 = 128, B_Penyergap = 256, B_MalamDarah1 = 512, B_MalamDarah2 = 1024;
        public static readonly Vector3[] Pusat = { Vector3.zero, new Vector3(-600f, -80f, -600f), new Vector3(0f, -80f, -600f), new Vector3(600f, -80f, -600f) };
        public static readonly Color AmbienGua = new Color(0.11f, 0.14f, 0.17f);

        [SerializeField] Transform[] kompleks = new Transform[4];
        [SerializeField] Image layarHitam;

        public UndergroundId Kini { get; private set; }
        public int Bendera { get; private set; }
        public int Gali { get; private set; }
        public int Versi { get; private set; }
        public bool SedangPindah { get; private set; }
        public string SaveKey { get { return "gua"; } }

        readonly List<Light>[] _lampu = new List<Light>[4];
        readonly List<Light> _urut = new List<Light>();
        AndiFPSController _andi;
        Camera _kamera;
        float _jauhAsli = -1f, _t;
        CameraClearFlags _bersihAsli;
        Color _latarAsli;

        public void Pasang(Transform[] k, Image hitam) { kompleks = k; layarHitam = hitam; }
        public bool Ada(int b) { return (Bendera & b) != 0; }
        public void Set(int b) { if ((Bendera & b) == b) return; Bendera |= b; Versi++; }
        public void TambahGali() { Gali = Mathf.Min(GaliPerlu, Gali + 1); if (Gali >= GaliPerlu) Set(B_Palka); Versi++; }

        void Awake()
        {
            GameServices.Register(this);
            for (int i = 0; i < 4; i++) _lampu[i] = new List<Light>();
            for (int i = 1; i < 4 && kompleks != null && i < kompleks.Length; i++)
                if (kompleks[i] != null) _lampu[i].AddRange(kompleks[i].GetComponentsInChildren<Light>(true));
            if (layarHitam != null) { layarHitam.raycastTarget = false; Warna(0f); }
        }
        void OnEnable() { GameEvents.OnLoreFound += Lore; GameEvents.OnItemChanged += Item; }
        void OnDisable() { GameEvents.OnLoreFound -= Lore; GameEvents.OnItemChanged -= Item; }

        public static UndergroundId KompleksDi(Vector3 p)
        {
            if (!WorldTerrainGenerator.DiBawahTanah(p.y)) return UndergroundId.None;
            int terbaik = 1; float d = float.MaxValue;
            for (int i = 1; i < Pusat.Length; i++) { float m = (new Vector2(p.x - Pusat[i].x, p.z - Pusat[i].z)).sqrMagnitude; if (m < d) { d = m; terbaik = i; } }
            return (UndergroundId)terbaik;
        }
        public static Vector3 Lokal(UndergroundId u, Vector3 lokal) { return Pusat[(int)u] + lokal; }

        // ---------- Pindah (fade 1,2 s + teleport) ----------
        public void Pindah(Vector3 tujuan, float yaw)
        {
            if (SedangPindah) return;
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            if (_andi == null) { Debug.LogError("[AK][Gua] Andi tidak ditemukan, pindah dibatalkan"); return; }
            StartCoroutine(RutinPindah(tujuan, yaw));
        }

        IEnumerator RutinPindah(Vector3 tujuan, float yaw)
        {
            SedangPindah = true;
            float setengah = LamaFade * 0.5f;
            for (float t = 0f; t < setengah; t += Time.unscaledDeltaTime) { Warna(t / setengah); yield return null; }
            Warna(1f);
            _andi.Teleport(tujuan);
            _andi.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            Segarkan(true);
            yield return null;
            for (float t = 0f; t < setengah; t += Time.unscaledDeltaTime) { Warna(1f - t / setengah); yield return null; }
            Warna(0f);
            SedangPindah = false;
        }

        void Warna(float a) { if (layarHitam != null) { layarHitam.color = new Color(0f, 0f, 0f, a); layarHitam.enabled = a > 0.001f; } }

        // ---------- Mode Dalam Ruangan ----------
        void Segarkan(bool paksa)
        {
            if (_andi == null) return;
            var baru = KompleksDi(_andi.transform.position);
            if (baru == Kini && !paksa) return;
            var lama = Kini;
            Kini = baru;
            if (baru == lama) return;
            if (_kamera == null && _andi.Kamera != null) _kamera = _andi.Kamera.GetComponent<Camera>();
            bool dalam = baru != UndergroundId.None;
            URPAtmosphereAndWeather.DalamRuangan = dalam;
            if (_kamera != null)
            {
                if (_jauhAsli < 0f) { _jauhAsli = _kamera.farClipPlane; _bersihAsli = _kamera.clearFlags; _latarAsli = _kamera.backgroundColor; }
                _kamera.farClipPlane = dalam ? JarakPandangGua : _jauhAsli;
                _kamera.clearFlags = dalam ? CameraClearFlags.SolidColor : _bersihAsli;
                _kamera.backgroundColor = dalam ? Color.black : _latarAsli;
            }
            for (int i = 1; i < 4; i++) if (i != (int)baru) for (int k = 0; k < _lampu[i].Count; k++) if (_lampu[i][k] != null) _lampu[i][k].enabled = false;
            if (dalam) GameEvents.RaiseEnterUnderground(baru); else GameEvents.RaiseExitUnderground();
            if (lama == UndergroundId.UG2_Bunker && !dalam && Punya(ItemID.PakuKeramat2))
            {
                var q = GameServices.Get<QuestManager>(); if (q != null) q.TandaiKeluarBunker();
            }
        }

        void Update()
        {
            _t += Time.deltaTime;
            if (_t < TickDetik) return;
            _t = 0f;
            if (_andi == null) { _andi = GameServices.Get<AndiFPSController>(); if (_andi == null) return; }
            if (!SedangPindah) Segarkan(false);
            _andi.NapasMaks = Punya(ItemID.AlatSelamRebreather) ? NapasRebreather : AndiFPSController.NapasNormal;
            if (Kini != UndergroundId.None) { AturLampu(); Skrip(); }
        }

        // Hanya 4 lampu titik terdekat aktif (03 §10).
        void AturLampu()
        {
            var daftar = _lampu[(int)Kini];
            if (daftar.Count == 0) return;
            Vector3 a = _andi.transform.position;
            _urut.Clear(); _urut.AddRange(daftar);
            _urut.Sort((x, y) => (x.transform.position - a).sqrMagnitude.CompareTo((y.transform.position - a).sqrMagnitude));
            for (int i = 0; i < _urut.Count; i++) if (_urut[i] != null) _urut[i].enabled = i < MaksLampu;
        }

        // Khusus inspeksi foto Editor, juga bekerja sebelum Awake.
        public void TerapkanLampuFoto(Vector3 posisi)
        {
            var u=KompleksDi(posisi);
            for(int i=1;i<4;i++)
            {
                if(kompleks==null || i>=kompleks.Length || kompleks[i]==null)continue;
                var daftar=kompleks[i].GetComponentsInChildren<Light>(true);
                System.Array.Sort(daftar,(a,b)=>(a.transform.position-posisi).sqrMagnitude.CompareTo((b.transform.position-posisi).sqrMagnitude));
                for(int k=0;k<daftar.Length;k++)daftar[k].enabled=i==(int)u && k<MaksLampu;
            }
        }

        bool Punya(ItemID id) { var inv = GameServices.Get<TikarPandan3DInventory>(); return inv != null && inv.Punya(id); }

        [System.Serializable] class DataGua { public int bendera, gali, malamDarah; }
        public string CaptureJson() { return JsonUtility.ToJson(new DataGua { bendera = Bendera, gali = Gali, malamDarah = EnemyDirector.MalamDarahPaksa }); }
        public void RestoreJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            var d = JsonUtility.FromJson<DataGua>(json);
            if (d == null) return;
            Bendera = d.bendera; Gali = d.gali; EnemyDirector.MalamDarahPaksa = d.malamDarah; Versi++;
            Segarkan(true);
        }
    }
}
