using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.World
{
    // Penanda cakrawala hidup jarak jauh (terlihat hingga 400 m dari puncak bukit/menara):
    // 8 pendaran api malam & kepulan asap penanda pemukiman/landmark yang merespons jam & cuaca.
    public class SinyalCakrawalaPulau : MonoBehaviour
    {
        [SerializeField] Light[] pendarApi = new Light[0];
        [SerializeField] ParticleSystem[] kepulanAsap = new ParticleSystem[0];

        float[] _int0;
        Transform _kamera;
        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _cuaca;
        bool _siap;

        public int JumlahSinyal
        {
            get { return (pendarApi != null ? pendarApi.Length : 0) + (kepulanAsap != null ? kepulanAsap.Length : 0); }
        }

        public void Pasang(Light[] api, ParticleSystem[] asap)
        {
            pendarApi = api ?? new Light[0];
            kepulanAsap = asap ?? new ParticleSystem[0];
        }

        public static float KekuatanPendarCakrawala(float jam)
        {
            if (jam >= 18.0f || jam <= 5.5f) return 1f;
            if (jam >= 16.8f) return Mathf.InverseLerp(16.8f, 18.0f, jam);
            if (jam <= 6.8f) return 1f - Mathf.InverseLerp(5.5f, 6.8f, jam);
            return 0f;
        }

        void Awake()
        {
            _int0 = new float[pendarApi.Length];
            for (int i = 0; i < pendarApi.Length; i++)
                if (pendarApi[i] != null) _int0[i] = pendarApi[i].intensity;
            _siap = true;
        }

        void Update()
        {
            if (!_siap || !Application.isPlaying) return;
            if (_kamera == null) { var c = Camera.main; if (c != null) _kamera = c.transform; }
            if (_kamera == null) return;
            Vector3 cPos = _kamera.position;
            if (WorldTerrainGenerator.DiBawahTanah(cPos.y)) return;

            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            if (_cuaca == null) _cuaca = GameServices.Get<URPAtmosphereAndWeather>();
            float jam = _jam != null ? _jam.Jam : 10f;
            WeatherState cuaca = _cuaca != null ? _cuaca.Cuaca : WeatherState.Cerah;
            float pendar = KekuatanPendarCakrawala(jam);
            bool badai = cuaca == WeatherState.HujanBadai;
            float t = Time.time;

            for (int i = 0; i < pendarApi.Length; i++)
            {
                var l = pendarApi[i];
                if (l == null) continue;
                bool aktif = pendar > 0.05f && (l.transform.position - cPos).sqrMagnitude < 240f * 240f;
                if (l.enabled != aktif) l.enabled = aktif;
                if (aktif)
                {
                    float kedip = 0.85f + 0.15f * Mathf.Sin(t * 5.8f + i * 1.9f);
                    l.intensity = _int0[i] * pendar * kedip;
                }
            }

            for (int i = 0; i < kepulanAsap.Length; i++)
            {
                var ps = kepulanAsap[i];
                if (ps == null) continue;
                bool dekat = !badai && (ps.transform.position - cPos).sqrMagnitude < 320f * 320f;
                var em = ps.emission;
                if (em.enabled != dekat) em.enabled = dekat;
            }
        }
    }
}
