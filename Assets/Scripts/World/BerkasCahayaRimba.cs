using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.World
{
    // Berkas cahaya matahari menembus kanopi (God-Rays) & lapisan kabut lembah rendah bergulung.
    // Ringan di HP RAM 4 GB: update tiap 0,12 s, culling jarak, nol alokasi GC (MaterialPropertyBlock).
    public class BerkasCahayaRimba : MonoBehaviour
    {
        public const float JarakBerkas = 125f, JarakKabut = 165f;

        [SerializeField] MeshRenderer[] berkas = new MeshRenderer[0];
        [SerializeField] MeshRenderer[] kabutLembah = new MeshRenderer[0];
        [SerializeField] Vector3[] asalKabut = new Vector3[0];

        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _cuaca;
        AndiFPSController _andi;
        MaterialPropertyBlock _mpbBerkas, _mpbKabut;
        float _t;

        public int JumlahBerkas { get { return berkas != null ? berkas.Length : 0; } }
        public int JumlahKabut { get { return kabutLembah != null ? kabutLembah.Length : 0; } }

        public void Pasang(MeshRenderer[] b, MeshRenderer[] k, Vector3[] posKabut)
        {
            berkas = b ?? new MeshRenderer[0];
            kabutLembah = k ?? new MeshRenderer[0];
            asalKabut = posKabut ?? new Vector3[0];
        }

        public static float IntensitasBerkas(float jam, WeatherState cuaca)
        {
            if (jam < 6.0f || jam > 18.1f) return 0f;
            if (cuaca == WeatherState.HujanBadai || cuaca == WeatherState.KabutDarah) return 0f;
            float faktorCuaca = cuaca == WeatherState.Cerah ? 1f : (cuaca == WeatherState.KabutPagi ? 1.15f : 0.42f);
            // Paling dramatis saat fajar emas (06:30-09:30) dan sore hangat (15:15-17:45)
            float pagi = Mathf.Clamp01(1f - Mathf.Abs(jam - 8.0f) / 2.2f);
            float sore = Mathf.Clamp01(1f - Mathf.Abs(jam - 16.5f) / 1.8f);
            float siang = (jam >= 9.5f && jam <= 15.2f) ? 0.36f : 0f;
            return Mathf.Clamp01(Mathf.Max(Mathf.Max(pagi, sore), siang) * faktorCuaca);
        }

        public static float IntensitasKabutLembah(float jam, WeatherState cuaca)
        {
            float dasar = (jam < 7.8f || jam >= 17.2f) ? 0.62f : 0.20f;
            if (cuaca == WeatherState.KabutPagi || cuaca == WeatherState.Gerimis) dasar += 0.28f;
            else if (cuaca == WeatherState.HujanBadai || cuaca == WeatherState.KabutAsap) dasar += 0.34f;
            return Mathf.Clamp01(dasar);
        }

        void Awake()
        {
            _mpbBerkas = new MaterialPropertyBlock();
            _mpbKabut = new MaterialPropertyBlock();
        }

        void Update()
        {
            if (JedaDunia.Dijeda) return;
            _t += Time.deltaTime;
            if (_t < 0.12f) return;
            _t = 0f;

            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            if (_cuaca == null) _cuaca = GameServices.Get<URPAtmosphereAndWeather>();
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            if (_andi == null) return;

            bool bawah = URPAtmosphereAndWeather.DalamRuangan;
            float jam = _jam != null ? _jam.Jam : 9f;
            var w = _cuaca != null ? _cuaca.Cuaca : WeatherState.Cerah;
            Vector3 pAndi = _andi.transform.position;
            float now = Time.time;

            float iBerkas = bawah ? 0f : IntensitasBerkas(jam, w);
            Color colBerkas = jam > 15f
                ? new Color(1.0f, 0.76f, 0.42f, iBerkas * 0.11f)
                : new Color(1.0f, 0.94f, 0.72f, iBerkas * 0.095f);
            _mpbBerkas.SetColor("_BaseColor", colBerkas);

            float jB2 = JarakBerkas * JarakBerkas;
            float sudutX = Mathf.Lerp(26f, -28f, Mathf.Clamp01((jam - 6f) / 12f));
            for (int i = 0; i < berkas.Length; i++)
            {
                var r = berkas[i];
                if (r == null) continue;
                bool aktif = iBerkas > 0.02f && (r.transform.position - pAndi).sqrMagnitude <= jB2;
                if (r.enabled != aktif) r.enabled = aktif;
                if (!aktif) continue;
                r.transform.rotation = Quaternion.Euler(sudutX, 24f + (i % 5) * 8f, 12f);
                r.SetPropertyBlock(_mpbBerkas);
            }

            float iKabut = bawah ? 0f : IntensitasKabutLembah(jam, w);
            Color colKabut = w == WeatherState.KabutDarah
                ? new Color(0.55f, 0.16f, 0.14f, iKabut * 0.18f)
                : (jam >= 17.5f && jam < 19.2f
                    ? new Color(0.86f, 0.58f, 0.42f, iKabut * 0.16f)
                    : new Color(0.82f, 0.88f, 0.86f, iKabut * 0.16f));
            _mpbKabut.SetColor("_BaseColor", colKabut);

            float jK2 = JarakKabut * JarakKabut;
            for (int i = 0; i < kabutLembah.Length; i++)
            {
                var r = kabutLembah[i];
                if (r == null) continue;
                bool aktif = iKabut > 0.02f && i < asalKabut.Length && (asalKabut[i] - pAndi).sqrMagnitude <= jK2;
                if (r.enabled != aktif) r.enabled = aktif;
                if (!aktif) continue;
                float driftX = Mathf.Sin(now * 0.22f + i * 1.13f) * 3.8f;
                float driftZ = Mathf.Cos(now * 0.18f + i * 0.87f) * 3.2f;
                r.transform.position = asalKabut[i] + new Vector3(driftX, Mathf.Sin(now * 0.4f + i) * 0.25f, driftZ);
                r.SetPropertyBlock(_mpbKabut);
            }
        }
    }
}
