using AlasKeramat.AI;
using AlasKeramat.Building;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.Core
{
    // 16 §5 (T9): kejadian dunia dinamis - tiap pagi 06:00 diundi maks 2 (bobot, syarat) + terskrip:
    // W01 pesawat pencari (Hari 4 08:00 + 10%), W05 peti terdampar (20%), W06 jenazah ekspedisi (tiap 3 hari, maks 5),
    // W11 hujan meteor (malam Hari 7 cerah), W12 kunang-kunang (8%, malam cerah dekat sungai), W13 penyu bertelur (Hari >= 4).
    // W02-W04, W07-W10 (badai merobohkan pohon, kebakaran, banjir, prosesi, Sekte vs mutan, babi, monyet) menyusul fase poles.
    public class WorldEventScheduler : MonoBehaviour, ISaveable
    {
        public const int MaksPerPagi = 2, MaksJenazah = 5;
        public const float PeluangPesawat = 0.1f, PeluangPeti = 0.2f, PeluangKunang = 0.08f, SanityMeteor = 15f, SanityKunang = 10f, JendelaSuar = 30f;
        public static readonly string[] NamaEkspedisi = { "Rendra W.", "Sari Puspa", "Bayu A.", "dr. Hendra", "Yosef K." };

        TimeOfDayClock _jam;
        AndiFPSController _andi;
        int _hariUndi = -1, _hariPesawat = -1, _hariMeteor = -1, _hariKunang = -1, _jenazah, _hariJenazah = -10, _hariPenyu = -1;
        float _jamPesawat = -1f, _tPesawat;
        bool _penyuBesok;

        public string SaveKey { get { return "peristiwa_dunia"; } }
        public int JumlahJenazah { get { return _jenazah; } }

        void Awake() { GameServices.Register(this); }

        // Undian pagi (fungsi murni): bit 1 = pesawat, 2 = peti, 4 = jenazah, 8 = penyu. Maks 2 kejadian.
        public static int Undi(int hari, int hariJenazahLalu, int jenazah, System.Random rng)
        {
            int hasil = 0, n = 0;
            if (hari == 4 || rng.NextDouble() < PeluangPesawat) { hasil |= 1; n++; }
            if (n < MaksPerPagi && hari - hariJenazahLalu >= 3 && jenazah < MaksJenazah) { hasil |= 4; n++; }
            if (n < MaksPerPagi && rng.NextDouble() < PeluangPeti) { hasil |= 2; n++; }
            if (n < MaksPerPagi && hari >= 4 && rng.NextDouble() < 0.3) { hasil |= 8; n++; }
            return hasil;
        }

        void Update()
        {
            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            if (_jam == null || _andi == null) return;
            try { Tick(); } catch (System.Exception e) { Debug.LogError("[AK][WorldEventScheduler] galat: " + e.Message); enabled = false; }
        }

        void Tick()
        {
            float jam = _jam.Jam; int hari = _jam.Hari;
            if (jam >= 6f && jam < 7f && _hariUndi != hari)
            {
                _hariUndi = hari;
                if (_penyuBesok) { _penyuBesok = false; GameEvents.RaiseSubtitle("(Tukik-tukik kecil merayap ke laut. Kehidupan tetap berjalan.)"); }
                int u = Undi(hari, _hariJenazah, _jenazah, new System.Random(hari * 7919 + 17));
                if ((u & 1) != 0) { _jamPesawat = 8f + Random.value * 3f; _hariPesawat = hari; }
                if ((u & 2) != 0) Peti();
                if ((u & 4) != 0) Jenazah(hari);
                if ((u & 8) != 0 && _hariPenyu != hari) { _hariPenyu = hari; _penyuBesok = true; }
            }
            if (_hariPesawat == hari && _jamPesawat > 0f && jam >= _jamPesawat) { _jamPesawat = -1f; Pesawat(); }
            if (_tPesawat > 0f)
            {
                _tPesawat -= Time.deltaTime;
                bool suar = false;
                foreach (var api in ApiUnggun.Semua) if (api != null && api.Menyala && Vector3.Distance(api.transform.position, _andi.transform.position) < 40f) suar = true;
                if (suar) { _tPesawat = 0f; GameEvents.RaiseSubtitle("(Pesawat itu berbelok sedikit... lalu pergi. Mereka mencari, tapi kabut menutupi semuanya.)"); }
            }
            bool cerah = GameServices.Get<URPAtmosphereAndWeather>() is URPAtmosphereAndWeather c && c.Cuaca == WeatherState.Cerah;
            if (hari == 7 && jam >= 21f && jam < 22f && _hariMeteor != hari && cerah)
            {
                _hariMeteor = hari;
                var v = GameServices.Get<AndiVitalsAndSanity>(); if (v != null) v.UbahSanity(SanityMeteor);
                GameEvents.RaiseSubtitle("(Bintang jatuh, satu demi satu. Untuk sesaat, hutan terasa damai.)");
            }
            if (jam >= 20f && jam < 21f && _hariKunang != hari && cerah)
            {
                _hariKunang = hari;
                var a = _andi.transform.position;
                if (JarakKeSungai(a.x, a.z) < 60f && new System.Random(hari * 31).NextDouble() < PeluangKunang)
                {
                    var v = GameServices.Get<AndiVitalsAndSanity>(); if (v != null) v.UbahSanity(SanityKunang);
                    GameEvents.RaiseSubtitle("(Ribuan kunang-kunang menyala di atas sungai.)");
                }
            }
        }

        void Pesawat()
        {
            var audio = GameServices.Get<ProceduralAudioSynth>();
            if (audio != null) audio.Play(ProceduralAudioSynth.KlipPesawat, _andi.transform.position + new Vector3(0f, 250f, 0f), AudioCategory.Efek, 1f, 1f, 4, 800f);
            GameEvents.RaiseSubtitle("(Suara pesawat kecil di atas kabut! Nyalakan api atau tembakkan suar!)");
            _tPesawat = JendelaSuar;
        }

        // Titik pantai: dari pusat ke arah acak, cari sel pertama di bawah 0,8 m.
        public static Vector3 TitikPantai(float sudut)
        {
            Vector3 arah = new Vector3(Mathf.Cos(sudut), 0f, Mathf.Sin(sudut));
            for (float r = 200f; r < WorldTerrainGenerator.BatasDunia; r += 8f)
            {
                Vector3 p = arah * r;
                float h = WorldTerrainGenerator.SampleHeight(p.x, p.z);
                if (h < 0.8f && h > -0.3f) return new Vector3(p.x, Mathf.Max(h, 0f), p.z);
            }
            return arah * (WorldTerrainGenerator.BatasDunia * 0.8f);
        }

        void Peti()
        {
            var p = TitikPantai(Random.value * Mathf.PI * 2f);
            var isi = new[] { ItemID.Baterai, ItemID.PerbanKain, ItemID.RansumDarurat, ItemID.PelurSuar };
            for (int i = 0; i < 2; i++) JarahanTanah.Jatuhkan(p + new Vector3(i * 0.6f, 0.4f, 0f), isi[Random.Range(0, isi.Length)], 1);
            GameEvents.RaiseToast("Sesuatu terdampar di pantai");
        }

        void Jenazah(int hari)
        {
            _hariJenazah = hari;
            string nama = NamaEkspedisi[_jenazah % NamaEkspedisi.Length];
            _jenazah++;
            var a = _andi.transform.position;
            Vector3 p = a;
            for (float r = 40f; r < 400f; r += 20f)
            {
                float s = Random.value * Mathf.PI * 2f;
                float x = a.x + Mathf.Cos(s) * r, z = a.z + Mathf.Sin(s) * r;
                if (JarakKeSungai(x, z) < 12f && WorldTerrainGenerator.SampleHeight(x, z) > 0.2f) { p = new Vector3(x, WorldTerrainGenerator.SampleHeight(x, z), z); break; }
            }
            JarahanTanah.Jatuhkan(p + Vector3.up * 0.4f, ItemID.RansumDarurat, 1);
            JarahanTanah.Jatuhkan(p + new Vector3(0.7f, 0.4f, 0f), ItemID.Baterai, 1);
            GameEvents.RaiseSubtitle("(Di tepi sungai: jenazah anggota ekspedisi. Tanda pengenalnya: " + nama + ".)");
        }

        public static float JarakKeSungai(float x, float z)
        {
            float terbaik = 9999f, wl, w, dalam;
            foreach (var s in WorldTerrainGenerator.SemuaSungai) terbaik = Mathf.Min(terbaik, WorldTerrainGenerator.JarakSungai(s, x, z, out wl, out w, out dalam));
            return terbaik;
        }

        [System.Serializable] struct DataDunia { public int jenazah, hariJenazah, hariMeteor; public bool penyuBesok; }
        public string CaptureJson() { return JsonUtility.ToJson(new DataDunia { jenazah = _jenazah, hariJenazah = _hariJenazah, hariMeteor = _hariMeteor, penyuBesok = _penyuBesok }); }
        public void RestoreJson(string json) { if (string.IsNullOrEmpty(json)) return; var d = JsonUtility.FromJson<DataDunia>(json); _jenazah = d.jenazah; _hariJenazah = d.hariJenazah; _hariMeteor = d.hariMeteor; _penyuBesok = d.penyuBesok; }
    }
}
