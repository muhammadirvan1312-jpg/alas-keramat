using System.Collections.Generic;
using AlasKeramat.Building;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // T7b (06 §7-9, 04 §3): hantu malam (Pocong Hutan 2 / Malam Darah 4, Kuntilanak di Beringin, Banaspati di Sawit tanpa hujan),
    // Malam Darah (Hari 3 lalu tiap 4 malam: kabut darah, hantu x2, raid terbesar), raid Sekte ke kemah 21:00-02:00 sesuai Kemarahan,
    // Totem Topeng x6 (+5 Kemarahan bila dihancurkan). Siang 05:00: semua hantu larut.
    public partial class EnemyDirector
    {
        public const int PocongNormal = 2, PocongMaks = 4, JedaSpawnHantu = 20, HariMalamDarahPertama = 3, SelangMalamDarah = 4;
        public const float JarakPocongMin = 60f, JarakPocongMaks = 120f, RadiusKuntilanak = 80f, JarakSawitBanaspati = 260f;
        public const float JamRaidMulai = 21f, JamRaidAkhir = 26f, JarakMunculRaid = 90f, PerTotem = 5f, LamaRaguObor = 30f;
        public static readonly Vector3 PusatBeringin = Vector3.zero;

        [SerializeField] GameObject[] prefabHantu = new GameObject[3]; // Pocong Hutan, Kuntilanak, Banaspati
        [SerializeField] GameObject[] prefabHantuTambahan = new GameObject[5]; // Kuyang, Tuyul, GenderuwoHutan, HantuAir, Penyerupa
        [SerializeField] GameObject prefabJarahan;
        [SerializeField] GameObject prefabBambu;

        public static bool KuntiTerikat; // 13 §4.1: dipaku -> tidak muncul lagi
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void ResetKunti() { KuntiTerikat = false; }
        public GameObject PrefabJarahan { get { return prefabJarahan; } }
        public GameObject PrefabBambu { get { return prefabBambu; } }
        public bool MalamDarah { get; private set; }
        public int HantuDiusir { get; private set; }
        public bool RaidBerjalan { get { return _raid.Count > 0; } }
        public Vector3 Kemah { get; private set; }

        readonly List<HantuAI> _hantu = new List<HantuAI>();
        readonly List<NusantaraEnemyAndMutantAI> _raid = new List<NusantaraEnemyAndMutantAI>();
        float _tSpawnHantu, _jamRaid = -1f;
        int _malamRaidDiputuskan = -1, _malamKuntiDiusir = -1, _totemHancur;
        bool _kuntiMalamIni;

        public void PasangMalam(GameObject[] hantu, GameObject jarahan, GameObject bambu) { prefabHantu = hantu; prefabJarahan = jarahan; prefabBambu = bambu; }
        public void PasangMalamTambahan(GameObject[] tambahan) { prefabHantuTambahan = tambahan; }

        // ---------- Rumus murni (self-test) ----------
        public static bool IsMalamDarah(int malam) { return malam >= HariMalamDarahPertama && (malam - HariMalamDarahPertama) % SelangMalamDarah == 0; }
        public static int MalamKe(int hari, float jam) { return jam < 5f ? hari - 1 : hari; }
        public static bool JamMalam(float jam) { return jam >= 19f || jam < 5f; }
        public static float PeluangRaid(float kemarahan) { return Mathf.Clamp(kemarahan / 100f + 0.2f, 0f, 0.95f); }

        // Komposisi raid 06 §7 (acak 0..1 menentukan jumlah dalam rentang).
        public static EnemyArchetype[] KomposisiRaid(float agresi, float acak)
        {
            var d = new List<EnemyArchetype>();
            if (agresi < 20f) return d.ToArray();
            int n = agresi < 40f ? 2 + (acak < 0.5f ? 0 : 1) : agresi < 60f ? 4 + (acak < 0.5f ? 0 : 1) : agresi < 80f ? 6 + (acak < 0.5f ? 0 : 1) : 8;
            for (int i = 0; i < n; i++)
            {
                var a = EnemyArchetype.AbdiSekte;
                if (agresi >= 40f && i % 3 == 1) a = EnemyArchetype.AbdiWanita;
                if (agresi >= 60f && i == 0) a = EnemyArchetype.PemimpinPatroli;
                if (agresi >= 60f && i == n - 1) a = EnemyArchetype.AbdiLumpur;
                if (agresi >= 80f && i == n - 2) a = EnemyArchetype.AbdiButo;
                d.Add(a);
            }
            return d.ToArray();
        }

        public static int TargetPocong(bool malamDarah) { return malamDarah ? PocongMaks : PocongNormal; }

        public static bool BiomaHutan(Biome b)
        {
            return b == Biome.HutanCampuran || b == Biome.HutanJatiSelatan || b == Biome.HutanBambu || b == Biome.TepiSungai || b == WorldTerrainGenerator.AlasWingit;
        }

        // ---------- Tick malam (dipanggil tiap 1 s dari Tick) ----------
        void TickMalam()
        {
            if (_jam == null) return;
            float jam = _jam.Jam;
            int malam = MalamKe(_jam.Hari, jam);
            bool gelap = JamMalam(jam);
            bool md = gelap && (IsMalamDarah(malam) || malam == MalamDarahPaksa) && !TanpaMalamDarah;
            if (md != MalamDarah)
            {
                MalamDarah = md;
                var atm = GameServices.Get<URPAtmosphereAndWeather>();
                if (md) { GameEvents.RaiseMalamDarahStart(); if (atm != null) atm.SetCuaca(WeatherState.KabutDarah, false); GameEvents.RaiseToast("Bulan memerah. Malam Darah."); }
                else { GameEvents.RaiseMalamDarahEnd(); if (atm != null && atm.Cuaca == WeatherState.KabutDarah) atm.SetCuaca(WeatherState.Cerah, false); }
            }
            for (int i = _hantu.Count - 1; i >= 0; i--) if (_hantu[i] == null || !_hantu[i].gameObject.activeSelf) _hantu.RemoveAt(i);
            if (!gelap)
            {
                for (int i = 0; i < _hantu.Count; i++) _hantu[i].Larutkan(); // sisa malam larut pukul 05:00
                _kuntiMalamIni = false;
                return;
            }
            _tSpawnHantu -= 1f;
            if (_tSpawnHantu <= 0f) { _tSpawnHantu = JedaSpawnHantu; SpawnHantu(malam); }
            TickRaid(jam, malam);
        }

        int HitungHantu(EnemyArchetype j) { int n = 0; for (int i = 0; i < _hantu.Count; i++) if (_hantu[i].IsAlive && (_hantu[i].Jenis == j || (j == EnemyArchetype.PocongHutan && _hantu[i].Jenis == EnemyArchetype.PocongGua))) n++; return n; }

        void SpawnHantu(int malam)
        {
            if (Aktif >= MaksAktif || TanpaMalamDarah || WorldTerrainGenerator.DiBawahTanah(_andi.transform.position.y)) return;
            Vector3 a = _andi.transform.position;
            if (HitungHantu(EnemyArchetype.PocongHutan) < TargetPocong(MalamDarah) && BiomaHutan(WorldTerrainGenerator.GetBiome(a.x, a.z)))
            {
                Vector3 p;
                if (TitikTersembunyi(a, _andi.transform.forward, Random.Range(JarakPocongMin, JarakPocongMaks), Random.Range(-40f, 40f), out p)) BuatHantu(0, p);
            }
            if (!KuntiTerikat && !_kuntiMalamIni && _malamKuntiDiusir != malam && HitungHantu(EnemyArchetype.Kuntilanak) == 0)
            {
                Vector3 sarang = PusatBeringin;
                if (MalamDarah) sarang = Kemah != Vector3.zero ? Kemah + new Vector3(12f, 0f, 12f) : a + new Vector3(20f, 0f, 20f);
                if (MalamDarah || Datar(a - PusatBeringin).magnitude < RadiusKuntilanak * 2.5f)
                {
                    var k = BuatHantu(1, new Vector3(sarang.x + 6f, WorldTerrainGenerator.SampleHeight(sarang.x + 6f, sarang.z) + 6f, sarang.z));
                    if (k != null) { k.Sarang = k.transform.position; _kuntiMalamIni = true; }
                }
            }
            var cuaca = GameServices.Get<URPAtmosphereAndWeather>();
            bool hujan = cuaca != null && (cuaca.Cuaca == WeatherState.Gerimis || cuaca.Cuaca == WeatherState.HujanBadai);
            if (!hujan && Datar(a - PusatSawit).magnitude < JarakSawitBanaspati && HitungHantu(EnemyArchetype.Banaspati) < (MalamDarah ? 2 : 1))
            {
                float s = Random.value * Mathf.PI * 2f;
                float x = PusatSawit.x + Mathf.Cos(s) * 60f, z = PusatSawit.z + Mathf.Sin(s) * 60f;
                BuatHantu(2, new Vector3(x, WorldTerrainGenerator.SampleHeight(x, z) + 1.4f, z));
            }
            Biome bi = WorldTerrainGenerator.GetBiome(a.x, a.z);
            if (prefabHantuTambahan != null && prefabHantuTambahan.Length >= 4)
            {
                Vector3 p;
                if ((bi == Biome.Desa || BiomaHutan(bi)) && HitungHantu(EnemyArchetype.Kuyang) < 1 && TitikTersembunyi(a, _andi.transform.forward, 55f, Random.Range(-50f, 50f), out p))
                    BuatHantuPref(prefabHantuTambahan[0], p + Vector3.up * 2.2f);
                if (HitungHantu(EnemyArchetype.Tuyul) < 1 && !AlasKeramat.Building.FungsiBangunan.MenangkalTuyul(a) && TitikTersembunyi(a, _andi.transform.forward, 28f, Random.Range(-60f, 60f), out p))
                    BuatHantuPref(prefabHantuTambahan[1], p);
                if ((bi == WorldTerrainGenerator.AlasWingit || bi == Biome.HutanCampuran) && HitungHantu(EnemyArchetype.GenderuwoHutan) < 1 && TitikTersembunyi(a, _andi.transform.forward, 65f, Random.Range(-45f, 45f), out p))
                    BuatHantuPref(prefabHantuTambahan[2], p);
                if ((bi == Biome.TepiSungai || bi == Biome.Pantai) && HitungHantu(EnemyArchetype.HantuAir) < 1 && TitikTersembunyi(a, _andi.transform.forward, 42f, Random.Range(-45f, 45f), out p))
                    BuatHantuPref(prefabHantuTambahan[3], p);
            }
        }

        HantuAI BuatHantu(int idx, Vector3 pos)
        {
            if (prefabHantu == null || idx >= prefabHantu.Length) return null;
            return BuatHantuPref(prefabHantu[idx], pos);
        }

        HantuAI BuatHantuPref(GameObject pref, Vector3 pos)
        {
            if (pref == null) return null;
            var go = Instantiate(pref, pos, Quaternion.Euler(0f, Random.value * 360f, 0f), transform);
            var h = go.GetComponent<HantuAI>();
            if (h == null) { Destroy(go); return null; }
            h.Sarang = pos;
            _hantu.Add(h);
            return h;
        }

        // T9: dipanggil TerrorDirector (E11/E12/E30): hantu muncul di titik tertentu (0 Pocong, 1 Kuntilanak, 2 Banaspati).
        public HantuAI PanggilHantu(int idx, Vector3 pos)
        {
            if (Aktif >= MaksAktif) return null;
            pos.y = WorldTerrainGenerator.SampleHeight(pos.x, pos.z) + (idx == 0 ? 0f : idx == 1 ? 2.4f : 1.4f);
            return BuatHantu(idx, pos);
        }

        public void LaporHantuDiusir(HantuAI h)
        {
            HantuDiusir++;
            if (h.Jenis == EnemyArchetype.Kuntilanak && _jam != null) _malamKuntiDiusir = MalamKe(_jam.Hari, _jam.Jam);
        }

        public int JumlahHantuAktif { get { int n = 0; for (int i = 0; i < _hantu.Count; i++) if (_hantu[i] != null && _hantu[i].IsAlive) n++; return n; } }

        // ---------- Raid ----------
        void TickRaid(float jam, int malam)
        {
            for (int i = _raid.Count - 1; i >= 0; i--) if (_raid[i] == null || !_raid[i].IsAlive) _raid.RemoveAt(i);
            if (_malamRaidDiputuskan != malam)
            {
                _malamRaidDiputuskan = malam;
                _jamRaid = -1f;
                if (malam >= 2)
                {
                    bool paksa = malam == 2 && Kemarahan < 20f;
                    if (paksa || Random.value < PeluangRaid(Kemarahan)) _jamRaid = Random.Range(JamRaidMulai, JamRaidAkhir);
                }
            }
            if (_jamRaid < 0f) return;
            float jamLinier = jam < 5f ? jam + 24f : jam;
            if (jamLinier < _jamRaid) return;
            _jamRaid = -1f;
            MulaiRaid(malam);
        }

        public void MulaiRaid(int malam)
        {
            Kemah = CariKemah();
            float agresi = Mathf.Max(Kemarahan, malam == 2 ? 20f : 0f);
            var komposisi = KomposisiRaid(agresi, Random.value);
            if (komposisi.Length == 0) return;
            Vector3 maju = _andi.transform.forward;
            Vector3 asal;
            if (!TitikTersembunyi(Kemah, maju, JarakMunculRaid, 0f, out asal)) return;
            bool raguObor = ZonaPenangkal.AdaObor(Kemah, ZonaPenangkal.RadiusRaidObor);
            for (int i = 0; i < komposisi.Length; i++)
            {
                Vector3 p = asal + new Vector3((i % 3 - 1) * 2.5f, 0f, (i / 3) * 2.5f);
                p.y = WorldTerrainGenerator.SampleHeight(p.x, p.z);
                var ai = SpawnLangsung(komposisi[i], p);
                if (ai == null) continue;
                ai.MulaiSerbu(Kemah, i == 0, raguObor);
                _raid.Add(ai);
            }
            if (MalamDarah) { int n = Random.Range(2, 5); for (int i = 0; i < n; i++) BuatHantu(0, asal + new Vector3(i * 3f, 0f, -4f)); }
            GameEvents.RaiseSubtitle("Ada langkah banyak orang di tepi hutan...");
        }

        NusantaraEnemyAndMutantAI SpawnLangsung(EnemyArchetype j, Vector3 p)
        {
            int i = (int)j;
            if (prefab == null || i >= prefab.Length || prefab[i] == null) return null;
            var go = Instantiate(prefab[i], p + Vector3.up * 0.1f, Quaternion.identity, transform);
            go.name = "Raid_" + j;
            var ai = go.GetComponent<NusantaraEnemyAndMutantAI>();
            if (ai != null) _takTerlihat[ai] = 0f;
            return ai;
        }

        // Kemah = Api Unggun terdekat dari Andi (<= 200 m); tanpa itu, posisi Andi.
        Vector3 CariKemah()
        {
            Vector3 a = _andi.transform.position, terbaik = a;
            float jarak = 200f;
            for (int i = 0; i < ApiUnggun.Semua.Count; i++)
            {
                var api = ApiUnggun.Semua[i];
                if (api == null) continue;
                float d = Datar(api.transform.position - a).magnitude;
                if (d < jarak) { jarak = d; terbaik = api.transform.position; }
            }
            return terbaik;
        }

        // ---------- Totem Topeng ----------
        public void LaporTotem(TotemTopeng t)
        {
            _totemHancur |= 1 << t.Indeks;
            UbahKemarahan(PerTotem);
            GameEvents.RaiseToast("Totem Topeng hancur. Sekte makin marah.");
        }

        public bool TotemHancur(int i) { return (_totemHancur & (1 << i)) != 0; }
        public int JumlahTotemHancur { get { int n = 0; for (int i = 0; i < 6; i++) if (TotemHancur(i)) n++; return n; } }
        int TotemMask { get { return _totemHancur; } set { _totemHancur = value; foreach (var t in TotemTopeng.Semua) if (t != null) t.Sinkron(TotemHancur(t.Indeks)); } }
    }
}
