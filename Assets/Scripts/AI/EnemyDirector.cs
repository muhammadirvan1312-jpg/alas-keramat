using System.Collections.Generic;
using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.AI
{
    // 06 §7 & §9 (T7a): regu Sekte siang (Desa 4-6 + pemimpin, Sawit 3-4 + Abdi Lumpur), pengintai 1-2 mulai Hari 1 15:00,
    // antrian spawn (maks 12 musuh aktif), despawn > 150 m & tak terlihat 10 s, token penyerang (Normal 2 / Sulit 3),
    // Kemarahan Sekte 0-100 (mulai 10). Malam (hantu, raid, Malam Darah, totem): EnemyDirector.Malam.cs.
    public partial class EnemyDirector : MonoBehaviour, ISaveable
    {
        public const int MaksAktif = 12, TokenNormal = 2, TokenSulit = 3;
        public const float JarakAktifRegu = 230f, JarakDespawn = 150f, LamaTakTerlihat = 10f, JarakLepasRegu = 320f;
        public const float JamPengintai = 15f, JamPengintaiAkhir = 18f, JarakMunculPengintai = 95f, RadiusIsyarat = 120f, TambahIsyarat = 40f;
        public const float KemarahanAwal = 10f, PerHari = 3f, PerBunuh = 2f, PerAmpuni = -4f, PerHariDamai = -2f, RadiusTakutTeman = 20f;
        public static readonly Vector3 PusatDesa = new Vector3(540f, 0f, 196f), TitikRitualDesa = new Vector3(540f, 0f, 200f), PusatSawit = new Vector3(-520f, 0f, 240f);

        public static Difficulty Kesulitan = Difficulty.Normal;
        public static GoreLevel TingkatGore { get { return GoreSystem.Tingkat; } } // H16: sumber tunggal di GoreSystem

        [SerializeField] GameObject[] prefab = new GameObject[5]; // indeks = EnemyArchetype 0-4

        public class Regu
        {
            public string nama; public Vector3 pusat; public EnemyArchetype[] anggota; public Vector3[][] rute; public bool[] matiHariIni;
            public readonly List<NusantaraEnemyAndMutantAI> hidup = new List<NusantaraEnemyAndMutantAI>();
            public bool aktif; public bool ritual;
        }

        // Antrian spawn murni (diuji self-test): berapa yang boleh muncul sekarang tanpa melewati MaksAktif.
        public class AntrianSpawn
        {
            public int Menunggu { get; private set; }
            public void Tambah(int n) { Menunggu += Mathf.Max(0, n); }
            public int Ambil(int aktif) { int n = Mathf.Clamp(MaksAktif - aktif, 0, Menunggu); Menunggu -= n; return n; }
        }

        struct PermintaanSpawn { public EnemyArchetype jenis; public Vector3 pos; public Regu regu; public int slot; public bool pengintai; }

        readonly List<Regu> _regu = new List<Regu>();
        readonly Queue<PermintaanSpawn> _antri = new Queue<PermintaanSpawn>();
        readonly List<NusantaraEnemyAndMutantAI> _token = new List<NusantaraEnemyAndMutantAI>();
        readonly List<NusantaraEnemyAndMutantAI> _menungguToken = new List<NusantaraEnemyAndMutantAI>();
        readonly Dictionary<NusantaraEnemyAndMutantAI, float> _takTerlihat = new Dictionary<NusantaraEnemyAndMutantAI, float>();
        readonly List<NusantaraEnemyAndMutantAI> _buang = new List<NusantaraEnemyAndMutantAI>();

        AndiFPSController _andi;
        TimeOfDayClock _jam;
        float _tCek;
        int _hariPengintai = -1, _membunuhHariIni;

        public float Kemarahan { get; private set; } = KemarahanAwal;
        public int SekteDibunuh { get; private set; }
        public int SekteDiampuni { get; private set; }
        public int Aktif { get { int n = 0; for (int i = 0; i < NusantaraEnemyAndMutantAI.Semua.Count; i++) if (NusantaraEnemyAndMutantAI.Semua[i].IsAlive) n++; for (int i = 0; i < HantuAI.Semua.Count; i++) if (HantuAI.Semua[i].IsAlive) n++; return n; } }
        public IList<Regu> DaftarRegu { get { return _regu; } }
        public string SaveKey { get { return "musuh"; } }

        public static int MaksToken() { return Kesulitan == Difficulty.Sulit ? TokenSulit : TokenNormal; }

        public void Pasang(GameObject[] p) { prefab = p; }

        void Awake()
        {
            GameServices.Register(this);
            BuatRegu();
        }

        void OnEnable() { GameEvents.OnDayChanged += HariBaru; }
        void OnDisable() { GameEvents.OnDayChanged -= HariBaru; }

        void BuatRegu()
        {
            _regu.Clear();
            var rng = new System.Random(7177);
            var desa = new List<EnemyArchetype> { EnemyArchetype.PemimpinPatroli };
            int nDesa = 4 + rng.Next(3);
            for (int i = 0; i < nDesa; i++) desa.Add(i % 3 == 2 ? EnemyArchetype.AbdiWanita : EnemyArchetype.AbdiSekte);
            _regu.Add(BuatSatuRegu("Desa", PusatDesa, desa.ToArray(), 55f, rng, true));
            var sawit = new List<EnemyArchetype> { EnemyArchetype.AbdiLumpur };
            int nSawit = 3 + rng.Next(2);
            for (int i = 1; i < nSawit; i++) sawit.Add(EnemyArchetype.AbdiSekte);
            _regu.Add(BuatSatuRegu("Sawit", PusatSawit, sawit.ToArray(), 70f, rng, false));
        }

        static Regu BuatSatuRegu(string nama, Vector3 pusat, EnemyArchetype[] anggota, float radius, System.Random rng, bool ritual)
        {
            var r = new Regu { nama = nama, pusat = pusat, anggota = anggota, rute = new Vector3[anggota.Length][], matiHariIni = new bool[anggota.Length], ritual = ritual };
            for (int a = 0; a < anggota.Length; a++)
            {
                int n = 4 + rng.Next(3);
                float mulai = (float)rng.NextDouble() * 360f, rr = radius * (0.35f + 0.65f * (float)rng.NextDouble());
                var titik = new Vector3[n];
                for (int i = 0; i < n; i++)
                {
                    float s = (mulai + i * 360f / n) * Mathf.Deg2Rad;
                    float x = pusat.x + Mathf.Cos(s) * rr, z = pusat.z + Mathf.Sin(s) * rr;
                    titik[i] = new Vector3(x, WorldTerrainGenerator.SampleHeight(x, z), z);
                }
                r.rute[a] = titik;
            }
            return r;
        }

        void Update()
        {
            try { Tick(Time.deltaTime); }
            catch (System.Exception e) { Debug.LogError("[AK][EnemyDirector] galat: " + e.Message); enabled = false; }
        }

        void Tick(float dt)
        {
            if (_andi == null) { _andi = GameServices.Get<AndiFPSController>(); if (_andi == null) return; }
            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            _tCek -= dt;
            if (_tCek <= 0f)
            {
                _tCek = 1f;
                CekRegu();
                CekPengintai();
                TickMalam();
                TickJadwal();
                Despawn(1f);
            }
            ProsesAntrian();
            for (int i = _menungguToken.Count - 1; i >= 0; i--) if (_menungguToken[i] == null || !_menungguToken[i].IsAlive || _menungguToken[i].Keadaan != NusantaraEnemyAndMutantAI.KeadaanSekte.Serang) _menungguToken.RemoveAt(i);
        }

        void CekRegu()
        {
            Vector3 a = _andi.transform.position;
            for (int r = 0; r < _regu.Count; r++)
            {
                var g = _regu[r];
                float d = Datar(a - g.pusat).magnitude;
                if (!g.aktif && d < JarakAktifRegu)
                {
                    g.aktif = true;
                    for (int s = 0; s < g.anggota.Length; s++)
                        if (!g.matiHariIni[s]) _antri.Enqueue(new PermintaanSpawn { jenis = g.anggota[s], pos = g.rute[s][0], regu = g, slot = s });
                }
                else if (g.aktif && d > JarakLepasRegu)
                {
                    for (int h = g.hidup.Count - 1; h >= 0; h--) if (g.hidup[h] != null) { _takTerlihat.Remove(g.hidup[h]); Destroy(g.hidup[h].gameObject); }
                    g.hidup.Clear();
                    g.aktif = false;
                }
            }
        }

        void CekPengintai()
        {
            if (_jam == null || _jam.Hari < 1 || _hariPengintai == _jam.Hari) return;
            if (_jam.Jam < JamPengintai || _jam.Jam >= JamPengintaiAkhir) return;
            _hariPengintai = _jam.Hari;
            int n = Kemarahan >= 20f ? 2 : 1;
            for (int i = 0; i < n; i++)
            {
                Vector3 p;
                if (!TitikTersembunyi(_andi.transform.position, _andi.transform.forward, JarakMunculPengintai, i * 37f, out p)) continue;
                _antri.Enqueue(new PermintaanSpawn { jenis = EnemyArchetype.AbdiSekte, pos = p, pengintai = true, slot = -1 });
            }
        }

        // Titik di belakang Andi (> 100 derajat dari arah pandang) yang bisa dilalui grid.
        public static bool TitikTersembunyi(Vector3 andi, Vector3 maju, float jarak, float geser, out Vector3 hasil)
        {
            var pf = GameServices.Get<GridPathfinder>();
            float dasar = Mathf.Atan2(maju.x, maju.z) * Mathf.Rad2Deg + 180f;
            for (int k = 0; k < 8; k++)
            {
                float s = (dasar + geser + (k % 2 == 0 ? 1 : -1) * k * 12f) * Mathf.Deg2Rad;
                float x = andi.x + Mathf.Sin(s) * jarak, z = andi.z + Mathf.Cos(s) * jarak;
                if (Mathf.Abs(x) > WorldTerrainGenerator.BatasDunia - 10f || Mathf.Abs(z) > WorldTerrainGenerator.BatasDunia - 10f) continue;
                int ix, iz; SurfaceGrid.KeSel(new Vector3(x, 0f, z), out ix, out iz);
                if (pf != null && pf.Grid != null && pf.Grid.Ambil(ix, iz) != SurfaceGrid.Darat) continue;
                float h = WorldTerrainGenerator.SampleHeight(x, z);
                if (h < 0.5f) continue;
                hasil = new Vector3(x, h, z);
                return true;
            }
            hasil = Vector3.zero;
            return false;
        }

        void ProsesAntrian()
        {
            int boleh = MaksAktif - Aktif;
            while (boleh > 0 && _antri.Count > 0)
            {
                var p = _antri.Dequeue();
                if (Spawn(p)) boleh--;
            }
        }

        bool Spawn(PermintaanSpawn p)
        {
            int i = (int)p.jenis;
            if (prefab == null || i < 0 || i >= prefab.Length || prefab[i] == null) { Debug.LogWarning("[AK][EnemyDirector] prefab " + p.jenis + " tidak ada"); return false; }
            var go = Instantiate(prefab[i], p.pos + Vector3.up * 0.1f, Quaternion.Euler(0f, Random.value * 360f, 0f), transform);
            go.name = (p.pengintai ? "Pengintai_" : (p.regu != null ? p.regu.nama + "_" : "")) + p.jenis;
            var ai = go.GetComponent<NusantaraEnemyAndMutantAI>();
            if (ai == null) { Destroy(go); return false; }
            ai.Pengintai = p.pengintai;
            if (p.regu != null)
            {
                ai.Asal = p.regu;
                ai.Rute = p.regu.rute[p.slot];
                if (p.regu.ritual) ai.TitikRitual = new Vector3(TitikRitualDesa.x, WorldTerrainGenerator.SampleHeight(TitikRitualDesa.x, TitikRitualDesa.z), TitikRitualDesa.z);
                p.regu.hidup.Add(ai);
            }
            _takTerlihat[ai] = 0f;
            return true;
        }

        void Despawn(float dt)
        {
            _buang.Clear();
            var cam = _andi.Kamera;
            foreach (var kv in _takTerlihat)
            {
                var ai = kv.Key;
                if (ai == null) { _buang.Add(ai); continue; }
                if (!ai.gameObject.activeSelf) { _buang.Add(ai); continue; }
                if (ai.Asal != null && ai.Asal.aktif) continue; // anggota regu dilepas bersama regunya (CekRegu)
                Vector3 ke = ai.transform.position - _andi.transform.position;
                bool jauh = Datar(ke).magnitude > JarakDespawn;
                bool terlihat = cam != null && Vector3.Angle(cam.forward, ke) < 60f;
                if (jauh && !terlihat) { if (kv.Value + dt >= LamaTakTerlihat) _buang.Add(ai); }
            }
            for (int i = 0; i < _buang.Count; i++)
            {
                var ai = _buang[i];
                _takTerlihat.Remove(ai);
                if (ai == null) continue;
                if (ai.Asal != null) ai.Asal.hidup.Remove(ai);
                Destroy(ai.gameObject);
            }
            // Perbarui pencacah setelah daftar buang (hindari mengubah kamus saat iterasi).
            _buang.Clear();
            foreach (var kv in _takTerlihat) _buang.Add(kv.Key);
            for (int i = 0; i < _buang.Count; i++)
            {
                var ai = _buang[i];
                Vector3 ke = ai.transform.position - _andi.transform.position;
                bool tersembunyi = Datar(ke).magnitude > JarakDespawn && (cam == null || Vector3.Angle(cam.forward, ke) >= 60f);
                _takTerlihat[ai] = tersembunyi ? _takTerlihat[ai] + dt : 0f;
            }
        }

        // ---------------- Token & komunikasi ----------------
        public bool MintaToken(NusantaraEnemyAndMutantAI ai)
        {
            for (int i = _token.Count - 1; i >= 0; i--) if (_token[i] == null || !_token[i].IsAlive || _token[i].Keadaan != NusantaraEnemyAndMutantAI.KeadaanSekte.Serang) _token.RemoveAt(i);
            if (_token.Contains(ai)) return true;
            if (_token.Count < MaksToken()) { _token.Add(ai); _menungguToken.Remove(ai); return true; }
            if (!_menungguToken.Contains(ai)) _menungguToken.Add(ai);
            return false;
        }

        public void LepasToken(NusantaraEnemyAndMutantAI ai) { _token.Remove(ai); }
        public bool AdaYangMenunggu(NusantaraEnemyAndMutantAI ai) { return _menungguToken.Count > 0 && !_menungguToken.Contains(ai); }
        public int JumlahToken { get { return _token.Count; } }

        public void Isyarat(Vector3 asal, Vector3 posAndi)
        {
            var s = NusantaraEnemyAndMutantAI.Semua;
            for (int i = 0; i < s.Count; i++)
                if (s[i].IsAlive && StatMusuh.Sekte(s[i].Jenis) && Vector3.Distance(s[i].transform.position, asal) <= RadiusIsyarat) s[i].Waspadai(posAndi, TambahIsyarat);
            MeterSuara.Pancar(asal, RadiusIsyarat, false);
        }

        public bool AdaTemanDekat(NusantaraEnemyAndMutantAI ai, float r)
        {
            var s = NusantaraEnemyAndMutantAI.Semua;
            for (int i = 0; i < s.Count; i++)
                if (s[i] != ai && s[i].IsAlive && s[i].Keadaan != NusantaraEnemyAndMutantAI.KeadaanSekte.Menyerah && Vector3.Distance(s[i].transform.position, ai.transform.position) <= r) return true;
            return false;
        }

        public void LaporMati(NusantaraEnemyAndMutantAI ai)
        {
            _token.Remove(ai);
            if (StatMusuh.Sekte(ai.Jenis)) { SekteDibunuh++; _membunuhHariIni++; UbahKemarahan(PerBunuh); }
            if (ai.Asal != null)
            {
                int slot = System.Array.IndexOf(ai.Asal.rute, ai.Rute);
                if (slot >= 0) ai.Asal.matiHariIni[slot] = true;
                ai.Asal.hidup.Remove(ai);
            }
            var s = NusantaraEnemyAndMutantAI.Semua;
            for (int i = 0; i < s.Count; i++)
                if (s[i] != ai && s[i].IsAlive && Vector3.Distance(s[i].transform.position, ai.transform.position) <= RadiusTakutTeman) s[i].Takut += NusantaraEnemyAndMutantAI.TakutTemanMati;
        }

        public void LaporAmpuni(NusantaraEnemyAndMutantAI ai) { SekteDiampuni++; UbahKemarahan(PerAmpuni); }

        public void UbahKemarahan(float d) { Kemarahan = Mathf.Clamp(Kemarahan + d * (d > 0f && Kesulitan == Difficulty.Sulit ? 1.5f : 1f), 0f, 100f); }
        public void SetKemarahan(float v) { Kemarahan = Mathf.Clamp(v, 0f, 100f); }

        void HariBaru(int hari)
        {
            UbahKemarahan(PerHari);
            if (_membunuhHariIni == 0 && hari > 1) UbahKemarahan(PerHariDamai);
            _membunuhHariIni = 0;
            for (int r = 0; r < _regu.Count; r++) System.Array.Clear(_regu[r].matiHariIni, 0, _regu[r].matiHariIni.Length);
        }

        static Vector3 Datar(Vector3 v) { v.y = 0f; return v; }

        [System.Serializable] struct DataDirektur { public float kemarahan; public int dibunuh, diampuni, hantuDiusir, totem; public bool kuntiTerikat; }
        public string CaptureJson() { return JsonUtility.ToJson(new DataDirektur { kemarahan = Kemarahan, dibunuh = SekteDibunuh, diampuni = SekteDiampuni, hantuDiusir = HantuDiusir, totem = TotemMask, kuntiTerikat = KuntiTerikat }); }
        public void RestoreJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            var d = JsonUtility.FromJson<DataDirektur>(json);
            Kemarahan = Mathf.Clamp(d.kemarahan, 0f, 100f); SekteDibunuh = d.dibunuh; SekteDiampuni = d.diampuni; HantuDiusir = d.hantuDiusir; TotemMask = d.totem; KuntiTerikat = d.kuntiTerikat;
        }
    }
}
