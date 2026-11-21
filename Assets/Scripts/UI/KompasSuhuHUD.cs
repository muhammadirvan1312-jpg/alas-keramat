using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    // Strip kompas kardinal, suhu lingkungan (C) & nama bioma aktif di bagian atas HUD (setara jam taktis SOTF).
    public class KompasSuhuHUD : MonoBehaviour
    {
        public const float Interval = 0.25f;
        static readonly string[] Arah = { "U", "TL", "T", "TG", "S", "BD", "B", "BL" };

        [SerializeField] Text teksInfo;
        float _t;
        int _lastKey = int.MinValue;
        Transform _cam;
        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _cuaca;
        AndiFPSController _andi;

        public void Pasang(Text t) { teksInfo = t; }

        public static string ArahKardinal(float sudutY)
        {
            int i = Mathf.RoundToInt(Mathf.Repeat(sudutY, 360f) / 45f) & 7;
            return Arah[i];
        }

        // Suhu lingkungan (derajat C): siang tropis ~31 C, malam ~21 C, hujan/gua ~16 C, dekat api +9 C.
        public static float HitungSuhuLingkungan(float jam, WeatherState cuaca, bool bawahTanah, bool dekatApi)
        {
            float dasar;
            if (bawahTanah) dasar = 17f;
            else
            {
                float siang = Mathf.Clamp01(Mathf.Sin(Mathf.Clamp01((jam - 5.5f) / 13f) * Mathf.PI));
                dasar = Mathf.Lerp(21f, 31f, siang);
                if (cuaca == WeatherState.HujanBadai) dasar -= 5.5f;
                else if (cuaca == WeatherState.Gerimis) dasar -= 3.0f;
                else if (cuaca == WeatherState.KabutPagi) dasar -= 2.0f;
            }
            if (dekatApi) dasar += 9f;
            return dasar;
        }

        public static string NamaBioma(Biome b)
        {
            if (b == WorldTerrainGenerator.RawaBakau) return "Rawa Bakau";
            if (b == WorldTerrainGenerator.PabrikGula) return "Pabrik Gula";
            if (b == WorldTerrainGenerator.GunungKapur) return "Gunung Kapur";
            if (b == WorldTerrainGenerator.LadangTebu) return "Ladang Tebu";
            if (b == WorldTerrainGenerator.AlasWingit) return "Alas Wingit";
            switch (b)
            {
                case Biome.Pantai: return "Pantai";
                case Biome.KawahBeringin: return "Kawah Beringin";
                case Biome.TebingKapur: return "Tebing Kapur";
                case Biome.SawitTerbakar: return "Sawit Terbakar";
                case Biome.Desa: return "Desa";
                case Biome.HutanBambu: return "Hutan Bambu";
                case Biome.TepiSungai: return "Tepi Sungai";
                case Biome.HutanJatiSelatan: return "Hutan Jati";
                case Biome.BawahTanah: return "Bawah Tanah";
                default: return "Hutan Rimba";
            }
        }

        void Update()
        {
            if (teksInfo == null) return;
            _t += Time.deltaTime;
            if (_t < Interval) return;
            _t = 0f;
            if (_cam == null) { var c = Camera.main; if (c != null) _cam = c.transform; }
            if (_cam == null) return;
            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            if (_cuaca == null) _cuaca = GameServices.Get<URPAtmosphereAndWeather>();
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();

            Vector3 p = _andi != null ? _andi.transform.position : _cam.position;
            bool bawah = WorldTerrainGenerator.DiBawahTanah(p.y);
            bool dekatApi = false;
            var apis = ApiUnggun.Semua;
            for (int i = 0; i < apis.Count; i++)
            {
                var a = apis[i];
                if (a != null && a.Menyala && (a.transform.position - p).sqrMagnitude < 49f) { dekatApi = true; break; }
            }
            float jamVal = _jam != null ? _jam.Jam : 9f;
            var w = _cuaca != null ? _cuaca.Cuaca : WeatherState.Cerah;
            int suhu = Mathf.RoundToInt(HitungSuhuLingkungan(jamVal, w, bawah, dekatApi));
            int arahIdx = Mathf.RoundToInt(Mathf.Repeat(_cam.eulerAngles.y, 360f) / 45f) & 7;
            Biome bio = bawah ? Biome.BawahTanah : WorldTerrainGenerator.GetBiome(p.x, p.z);
            int key = arahIdx + (suhu + 50) * 16 + ((int)bio) * 2048 + (dekatApi ? 65536 : 0);
            if (key == _lastKey) return;
            _lastKey = key;
            teksInfo.text = Arah[arahIdx] + "  |  " + NamaBioma(bio) + "  |  " + suhu + " C" + (dekatApi ? " [API]" : "");
        }
    }
}
