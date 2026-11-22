using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Kehidupan sosial alami untuk companion Kang Deden & Ningsih (terinspirasi Kelvin & Virginia SOTF):
    // - Kang Deden menyapa saat Andi kembali, mengingatkan badai/malam/luka & memberi semangat di dekat api.
    // - Ningsih membawa hadiah herbal/buah segar (DaunSirih, KunyitObat, JaheHutan, KelapaMuda) ke tas Andi
    //   dan memberi peringatan halus saat Maghrib atau bahaya malam mendekat.
    public class InteraksiSosialTeman : MonoBehaviour
    {
        public enum PeranTeman { KangDeden = 0, Ningsih = 1 }
        public const float JarakSapa = 8.5f, JedaSapa = 75f, JedaHadiah = 140f;

        [SerializeField] PeranTeman peran = PeranTeman.KangDeden;

        Transform _kamera;
        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _cuaca;
        float _sapaBerikut = 12f, _hadiahBerikut = 45f;
        int _indeksPesan;

        public PeranTeman PeranAktif { get { return peran; } }

        public void Pasang(PeranTeman p) { peran = p; }

        public static ItemID PilihHadiahNingsih(int urutan)
        {
            int m = Mathf.Abs(urutan) % 4;
            if (m == 0) return ItemID.DaunSirih;
            if (m == 1) return ItemID.KunyitObat;
            if (m == 2) return ItemID.JaheHutan;
            return ItemID.KelapaMuda;
        }

        public static string PilihSapaan(PeranTeman p, float jam, WeatherState cuaca, int urutan)
        {
            bool malam = jam >= 18.0f || jam <= 5.5f;
            bool hujan = cuaca == WeatherState.Gerimis || cuaca == WeatherState.HujanBadai;
            if (p == PeranTeman.KangDeden)
            {
                if (hujan) return "Kang Deden: Badai makin kencang, Mas Andi! Merapat ke api unggun dulu!";
                if (malam) return "Kang Deden: Gelap begini sekte sering patroli. Senter jangan lupa dinyalakan.";
                int k = Mathf.Abs(urutan) % 3;
                if (k == 0) return "Kang Deden: Syukur Mas Andi balik selamat! Kuali jamu & peti dekat heli masih aman.";
                if (k == 1) return "Kang Deden: Tadi saya lihat burung terbang dari arah Desa Mati. Hati-hati kalau ke sana.";
                return "Kang Deden: Kalau butuh kayu atau batu, beri aba-aba saja ya Mas.";
            }
            if (hujan) return "Ningsih: Hujan deras membawa kabut dari Kawah Beringin... tetap di dekatku, Andi.";
            if (malam) return "Ningsih: Dengar suara kentongan jauh itu? Penjaga rimba sudah bangun malam ini.";
            int n = Mathf.Abs(urutan) % 3;
            if (n == 0) return "Ningsih: Aku menemukan tanaman obat di tepi rimba... ambillah untuk bekalmu.";
            if (n == 1) return "Ningsih: Altar Beringin terasa lebih tenang saat dupa dinyalakan.";
            return "Ningsih: Pulau ini mengingat setiap langkah kita. Jangan berjalan sendirian terlalu jauh.";
        }

        void Update()
        {
            if (!Application.isPlaying) return;
            if (_kamera == null) { var c = Camera.main; if (c != null) _kamera = c.transform; }
            if (_kamera == null) return;

            Vector3 d = _kamera.position - transform.position;
            if (d.x * d.x + d.z * d.z > JarakSapa * JarakSapa) return;

            float t = Time.time;
            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            if (_cuaca == null) _cuaca = GameServices.Get<URPAtmosphereAndWeather>();
            float jam = _jam != null ? _jam.Jam : 10f;
            WeatherState cuaca = _cuaca != null ? _cuaca.Cuaca : WeatherState.Cerah;

            if (peran == PeranTeman.Ningsih && t >= _hadiahBerikut)
            {
                _hadiahBerikut = t + JedaHadiah;
                _sapaBerikut = t + JedaSapa * 0.6f;
                ItemID hadiah = PilihHadiahNingsih(_indeksPesan++);
                var inv = GameServices.Get<TikarPandan3DInventory>();
                if (inv != null) inv.Tambah(hadiah, 1);
                var vital = GameServices.Get<AndiVitalsAndSanity>();
                if (vital != null) vital.UbahSanity(8f);
                GameEvents.RaiseToast("Ningsih memberimu " + hadiah + " segar (+8 Sanity)!");
                return;
            }

            if (t >= _sapaBerikut)
            {
                _sapaBerikut = t + JedaSapa;
                string pesan = PilihSapaan(peran, jam, cuaca, _indeksPesan++);
                var vital = GameServices.Get<AndiVitalsAndSanity>();
                if (vital != null) vital.UbahSanity(4f);
                GameEvents.RaiseToast(pesan);
            }
        }
    }
}
