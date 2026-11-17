using System;
using System.Reflection;
using UnityEngine;

namespace AlasKeramat.Core
{
    // Builder memasang satu komponen. Cuaca T2b dibaca tanpa ketergantungan
    // compile-time pada partial Cuaca yang belum tersedia di main T2a.
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(100)]
    public sealed class AKAngin : MonoBehaviour
    {
        const float Cerah = 0.18f, Berawan = 0.42f, Gerimis = 0.60f;
        const float Badai = 1.15f, Kabut = 0.12f, Asap = 0.28f;
        const float TransisiDetik = 8f, ResponsHembusan = 2f;
        const float FrekuensiHembusan = 0.12f, PengaliHembusan = 0.55f;
        const float SudutDasar = 35f, SudutHembusan = 12f;
        const float FrekuensiArah = 0.035f, DeltaMaks = 0.1f;

        static readonly int ArahId = Shader.PropertyToID("_AK_AnginArah");
        static readonly int KuatId = Shader.PropertyToID("_AK_AnginKuat");
        static readonly int WaktuId = Shader.PropertyToID("_AK_AnginWaktu");
        static AKAngin _pemilik;

        URPAtmosphereAndWeather _atmosfer;
        Func<URPAtmosphereAndWeather, WeatherState> _bacaCuaca;
        WeatherState _cuaca = WeatherState.Cerah;
        float _dasar = Cerah, _kuat = Cerah, _waktu;
        bool _gagalBaca;

        public float Kekuatan { get { return _kuat; } }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatis()
        {
            _pemilik = null;
            Shader.SetGlobalVector(ArahId, Vector4.zero);
            Shader.SetGlobalFloat(KuatId, 0f);
            Shader.SetGlobalFloat(WaktuId, 0f);
        }

        void OnEnable()
        {
            if (_pemilik != null && _pemilik != this)
            {
                Debug.LogError("[AK][AKAngin] Pengendali angin ganda dinonaktifkan.");
                enabled = false;
                return;
            }
            _pemilik = this;
            GameServices.Register(this);
            GameEvents.OnWeatherChanged += TerimaCuaca;
            // Pertahankan fase ketika komponen dinonaktifkan lalu diaktifkan.
            KirimGlobal();
        }

        void Start()
        {
            _atmosfer = GameServices.Get<URPAtmosphereAndWeather>();
            try
            {
                PropertyInfo p = typeof(URPAtmosphereAndWeather).GetProperty("Cuaca");
                if (p != null && p.PropertyType == typeof(WeatherState))
                {
                    MethodInfo getter = p.GetGetMethod();
                    if (getter != null)
                        _bacaCuaca = (Func<URPAtmosphereAndWeather, WeatherState>)Delegate.CreateDelegate(
                            typeof(Func<URPAtmosphereAndWeather, WeatherState>), getter);
                }
                if (_bacaCuaca == null)
                    Debug.LogWarning("[AK][AKAngin] Properti Cuaca belum tersedia; memakai event cuaca dan nilai awal Cerah.");
            }
            catch (Exception e)
            {
                Debug.LogError("[AK][AKAngin] Pembaca cuaca gagal disiapkan: " + e.Message);
            }
        }

        // Dapat dipanggil integrator saat memasang atmosfer setelah Start.
        public void Pasang(URPAtmosphereAndWeather atmosfer, WeatherState cuacaAwal)
        {
            _atmosfer = atmosfer;
            _gagalBaca = false;
            TerimaCuaca(cuacaAwal);
        }

        void TerimaCuaca(WeatherState cuaca) { _cuaca = cuaca; }

        static float Target(WeatherState cuaca)
        {
            switch (cuaca)
            {
                case WeatherState.Berawan: return Berawan;
                case WeatherState.Gerimis: return Gerimis;
                case WeatherState.HujanBadai: return Badai;
                case WeatherState.KabutPagi: return Kabut;
                case WeatherState.KabutAsap: return Asap;
                case WeatherState.KabutDarah: return Berawan;
                default: return Cerah;
            }
        }

        void Update()
        {
            if (_pemilik != this) return;
            // Delegate ter-cache: tanpa reflection, boxing, atau pencarian scene per frame.
            if (_atmosfer != null && _bacaCuaca != null && !_gagalBaca)
            {
                try { _cuaca = _bacaCuaca(_atmosfer); }
                catch (Exception e)
                {
                    _gagalBaca = true;
                    Debug.LogError("[AK][AKAngin] Pembacaan cuaca gagal; memakai event: " + e.Message);
                }
            }
            float dt = Mathf.Clamp(Time.deltaTime, 0f, DeltaMaks);
            _waktu += dt;
            _dasar = Mathf.Lerp(_dasar, Target(_cuaca), 1f - Mathf.Exp(-dt / TransisiDetik));
            float noise = Mathf.PerlinNoise(_waktu * FrekuensiHembusan, SudutDasar);
            float hembusan = Mathf.SmoothStep(0f, 1f, noise);
            float badai01 = Mathf.InverseLerp(Berawan, Badai, _dasar);
            float target = _dasar * (1f + hembusan * PengaliHembusan * badai01);
            _kuat = Mathf.Lerp(_kuat, target, 1f - Mathf.Exp(-dt * ResponsHembusan));
            KirimGlobal();
        }

        void KirimGlobal()
        {
            float sudut = (SudutDasar + Mathf.Sin(_waktu * FrekuensiArah) * SudutHembusan) * Mathf.Deg2Rad;
            Shader.SetGlobalVector(ArahId, new Vector4(Mathf.Cos(sudut), 0f, Mathf.Sin(sudut), 0f));
            Shader.SetGlobalFloat(KuatId, _kuat);
            Shader.SetGlobalFloat(WaktuId, _waktu);
        }

        void OnDisable()
        {
            GameEvents.OnWeatherChanged -= TerimaCuaca;
            if (_pemilik != this) return;
            _pemilik = null;
            Shader.SetGlobalFloat(KuatId, 0f);
        }
    }
}
