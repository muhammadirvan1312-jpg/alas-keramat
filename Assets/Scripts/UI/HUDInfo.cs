using AlasKeramat.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    // Baris info HUD (09 §2.1): "HARI 1 | 08:00 (PAGI) | Cerah" + nama zona saat masuk zona baru. Teks diperbarui hanya saat berubah (H10).
    public class HUDInfo : MonoBehaviour
    {
        public const float LamaZona = 4f, PudarZona = 1f, JarakCekZona = 0.5f;

        [SerializeField] Text labelInfo;
        [SerializeField] Text labelZona;

        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _atmos;
        int _menitTampil = -1;
        WeatherState _cuacaTampil = (WeatherState)(-1);
        string _zona = "";
        float _tZona = LamaZona + PudarZona, _tCek;

        public void Pasang(Text info, Text zona) { labelInfo = info; labelZona = zona; }

        public static string LabelWaktu(float jam)
        {
            if (jam >= 4f && jam < 6f) return "FAJAR";
            if (jam >= 6f && jam < 11f) return "PAGI";
            if (jam >= 11f && jam < 15f) return "SIANG";
            if (jam >= 15f && jam < 18f) return "SORE";
            if (jam >= 18f && jam < 19f) return "MAGHRIB";
            if (jam >= 19f) return "MALAM";
            return "DINI HARI";
        }

        public static string NamaCuaca(WeatherState w)
        {
            switch (w)
            {
                case WeatherState.Cerah: return "Cerah";
                case WeatherState.Berawan: return "Berawan";
                case WeatherState.Gerimis: return "Gerimis";
                case WeatherState.HujanBadai: return "Hujan Badai";
                case WeatherState.KabutPagi: return "Kabut Pagi";
                case WeatherState.KabutAsap: return "Kabut Asap";
                case WeatherState.KabutDarah: return "Kabut Darah";
                default: return "";
            }
        }

        void Update()
        {
            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            if (_atmos == null) _atmos = GameServices.Get<URPAtmosphereAndWeather>();
            if (_jam != null && labelInfo != null)
            {
                int menit = _jam.Hari * 1440 + Mathf.FloorToInt(_jam.Jam * 60f);
                var cuaca = _atmos != null ? _atmos.Cuaca : WeatherState.Cerah;
                if (menit != _menitTampil || cuaca != _cuacaTampil)
                {
                    _menitTampil = menit; _cuacaTampil = cuaca;
                    int j = Mathf.FloorToInt(_jam.Jam), m = Mathf.FloorToInt((_jam.Jam - j) * 60f);
                    labelInfo.text = "HARI " + _jam.Hari + "  |  " + j.ToString("00") + ":" + m.ToString("00") + " (" + LabelWaktu(_jam.Jam) + ")  |  " + NamaCuaca(cuaca);
                }
            }
            CekZona();
        }

        void CekZona()
        {
            if (labelZona == null) return;
            _tCek += Time.deltaTime;
            var cam = Camera.main;
            if (_tCek >= JarakCekZona && cam != null)
            {
                _tCek = 0f;
                string z = WorldTerrainGenerator.NamaZonaTerdekat(cam.transform.position);
                if (z != _zona) { _zona = z; labelZona.text = z; _tZona = 0f; }
            }
            _tZona += Time.deltaTime;
            float a = _tZona < LamaZona ? 1f : 1f - Mathf.Clamp01((_tZona - LamaZona) / PudarZona);
            var c = labelZona.color;
            if (!Mathf.Approximately(c.a, a)) { c.a = a; labelZona.color = c; }
        }
    }
}
