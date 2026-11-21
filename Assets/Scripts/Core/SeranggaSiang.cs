using UnityEngine;

namespace AlasKeramat.Core
{
    // Kupu-kupu tropis siang hari (desa/padang) & capung tepi sungai/rawa: zero-GC, mati otomatis saat hujan atau malam.
    public class SeranggaSiang : MonoBehaviour
    {
        public const float JarakAktif = 44f;

        [System.Serializable]
        public struct Serangga
        {
            public Transform badan, sayapL, sayapR;
            public Vector3 pusat;
            public float radius, laju, fase;
            public bool capung;
        }

        [SerializeField] Serangga[] daftar = new Serangga[0];
        Transform _cam;
        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _cuaca;

        public int Jumlah { get { return daftar != null ? daftar.Length : 0; } }
        public void Pasang(Serangga[] d) { daftar = d ?? new Serangga[0]; }

        public static bool AktifSiang(float jam, WeatherState cuaca)
        {
            if (jam < 6.5f || jam >= 17.0f) return false;
            return cuaca == WeatherState.Cerah || cuaca == WeatherState.Berawan || cuaca == WeatherState.KabutPagi;
        }

        void Update()
        {
            if (daftar == null || daftar.Length == 0 || JedaDunia.Dijeda) return;
            if (_cam == null) { var c = Camera.main; if (c != null) _cam = c.transform; }
            if (_cam == null) return;
            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            if (_cuaca == null) _cuaca = GameServices.Get<URPAtmosphereAndWeather>();
            bool boleh = AktifSiang(_jam != null ? _jam.Jam : 10f, _cuaca != null ? _cuaca.Cuaca : WeatherState.Cerah);
            Vector3 cp = _cam.position;
            float t = Time.time;
            float maksSq = JarakAktif * JarakAktif;

            for (int i = 0; i < daftar.Length; i++)
            {
                var s = daftar[i];
                if (s.badan == null) continue;
                Vector3 d = s.pusat - cp;
                bool hidup = boleh && (d.x * d.x + d.z * d.z <= maksSq);
                if (s.badan.gameObject.activeSelf != hidup) s.badan.gameObject.SetActive(hidup);
                if (!hidup) continue;

                float a = t * s.laju + s.fase;
                float r = s.radius * (s.capung ? 1f : (0.72f + 0.28f * Mathf.Sin(a * 0.7f)));
                float yOff = s.capung ? 0.14f * Mathf.Sin(a * 2.6f) : 0.45f * Mathf.Sin(a * 1.9f);
                Vector3 pos = new Vector3(s.pusat.x + Mathf.Cos(a) * r, s.pusat.y + yOff, s.pusat.z + Mathf.Sin(a * (s.capung ? 1f : 1.3f)) * r);
                Vector3 dir = pos - s.badan.position; dir.y = 0f;
                if (dir.sqrMagnitude > 1e-6f) s.badan.rotation = Quaternion.LookRotation(dir);
                s.badan.position = pos;

                float kepak = Mathf.Sin(t * (s.capung ? 36f : 14f) + s.fase) * (s.capung ? 24f : 52f);
                if (s.sayapL != null) s.sayapL.localRotation = Quaternion.Euler(0f, 0f, kepak);
                if (s.sayapR != null) s.sayapR.localRotation = Quaternion.Euler(0f, 0f, -kepak);
            }
        }
    }
}
