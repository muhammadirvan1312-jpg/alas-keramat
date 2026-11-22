using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.World
{
    // Menghidupkan atmosfer desa, pasar, altar & pantai: jendela rumah menyala hangat saat senja/malam,
    // jemuran batik & lentera pasar bergoyang tertiup angin, perahu tambat mengayun di ombak, & bara sesajen berkedip.
    public class SuasanaHidupDesaDanPantai : MonoBehaviour
    {
        [SerializeField] Renderer[] jendelaMalam = new Renderer[0];
        [SerializeField] Light[] lenteraMalam = new Light[0];
        [SerializeField] Transform[] kainAngin = new Transform[0];
        [SerializeField] Transform[] perahuOmbak = new Transform[0];

        Vector3[] _posPerahu0;
        Quaternion[] _rotKain0;
        Quaternion[] _rotPerahu0;
        float[] _intensitasLentera0;
        MaterialPropertyBlock _mpb;
        Transform _kamera;
        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _cuaca;
        bool _siap;

        public int JumlahElemenHidup
        {
            get
            {
                return (jendelaMalam != null ? jendelaMalam.Length : 0)
                    + (lenteraMalam != null ? lenteraMalam.Length : 0)
                    + (kainAngin != null ? kainAngin.Length : 0)
                    + (perahuOmbak != null ? perahuOmbak.Length : 0);
            }
        }

        public void Pasang(Renderer[] jendela, Light[] lentera, Transform[] kain, Transform[] perahu)
        {
            jendelaMalam = jendela ?? new Renderer[0];
            lenteraMalam = lentera ?? new Light[0];
            kainAngin = kain ?? new Transform[0];
            perahuOmbak = perahu ?? new Transform[0];
        }

        public static float FaktorLampuMalam(float jam, WeatherState cuaca)
        {
            float malam = 0f;
            if (jam >= 17.2f) malam = Mathf.Clamp01((jam - 17.2f) / 1.1f);
            else if (jam <= 6.2f) malam = Mathf.Clamp01((6.2f - jam) / 1.1f);
            float badai = cuaca == WeatherState.HujanBadai ? 0.65f : cuaca == WeatherState.HujanLebat ? 0.35f : 0f;
            return Mathf.Clamp01(Mathf.Max(malam, badai));
        }

        void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            _rotKain0 = new Quaternion[kainAngin.Length];
            for (int i = 0; i < kainAngin.Length; i++)
                if (kainAngin[i] != null) _rotKain0[i] = kainAngin[i].localRotation;

            _posPerahu0 = new Vector3[perahuOmbak.Length];
            _rotPerahu0 = new Quaternion[perahuOmbak.Length];
            for (int i = 0; i < perahuOmbak.Length; i++)
                if (perahuOmbak[i] != null) { _posPerahu0[i] = perahuOmbak[i].position; _rotPerahu0[i] = perahuOmbak[i].rotation; }

            _intensitasLentera0 = new float[lenteraMalam.Length];
            for (int i = 0; i < lenteraMalam.Length; i++)
                if (lenteraMalam[i] != null) _intensitasLentera0[i] = lenteraMalam[i].intensity;

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
            float lampu = FaktorLampuMalam(jam, cuaca);
            float angin = cuaca == WeatherState.HujanBadai ? 2.4f : cuaca == WeatherState.HujanLebat ? 1.7f : 1.0f;
            float t = Time.time;

            // Jendela rumah & lentera desa menyala hangat keemasan saat senja/malam.
            for (int i = 0; i < jendelaMalam.Length; i++)
            {
                var r = jendelaMalam[i];
                if (r == null) continue;
                bool dekat = (r.transform.position - cPos).sqrMagnitude < 120f * 120f;
                if (!dekat) continue;
                float kedip = 0.90f + 0.10f * Mathf.Sin(t * 5.2f + i * 1.7f);
                float p = lampu * kedip;
                Color em = new Color(2.6f * p, 1.45f * p, 0.42f * p, 1f);
                _mpb.SetColor("_EmissionColor", em);
                r.SetPropertyBlock(_mpb);
            }

            for (int i = 0; i < lenteraMalam.Length; i++)
            {
                var l = lenteraMalam[i];
                if (l == null) continue;
                bool dekat = lampu > 0.05f && (l.transform.position - cPos).sqrMagnitude < 75f * 75f;
                if (l.enabled != dekat) l.enabled = dekat;
                if (dekat)
                {
                    float kedip = 0.86f + 0.14f * Mathf.Sin(t * 6.4f + i * 2.1f);
                    l.intensity = _intensitasLentera0[i] * lampu * kedip;
                }
            }

            // Jemuran kain batik & lentera gantung pasar bergoyang mengikuti angin pulau.
            for (int i = 0; i < kainAngin.Length; i++)
            {
                var tr = kainAngin[i];
                if (tr == null) continue;
                if ((tr.position - cPos).sqrMagnitude > 85f * 85f) continue;
                float ayun = Mathf.Sin(t * (1.8f * angin) + i * 1.3f) * (7.5f * angin);
                float putar = Mathf.Cos(t * (1.2f * angin) + i * 0.9f) * (3.5f * angin);
                tr.localRotation = _rotKain0[i] * Quaternion.Euler(ayun, putar, ayun * 0.45f);
            }

            // Perahu nelayan tambat mengayun lembut mengikuti ombak pantai.
            for (int i = 0; i < perahuOmbak.Length; i++)
            {
                var tr = perahuOmbak[i];
                if (tr == null) continue;
                if ((tr.position - cPos).sqrMagnitude > 110f * 110f) continue;
                float bob = Mathf.Sin(t * (1.4f * angin) + i * 1.9f) * (0.14f * angin);
                float pitch = Mathf.Sin(t * (1.15f * angin) + i * 1.3f) * (3.2f * angin);
                float roll = Mathf.Cos(t * (1.55f * angin) + i * 2.1f) * (4.5f * angin);
                tr.position = _posPerahu0[i] + new Vector3(0f, bob, 0f);
                tr.rotation = _rotPerahu0[i] * Quaternion.Euler(pitch, 0f, roll);
            }
        }
    }
}
