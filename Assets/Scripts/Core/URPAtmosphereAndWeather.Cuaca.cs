using UnityEngine;

namespace AlasKeramat.Core
{
    // Cuaca (docs/gdd/05 §2): 7 keadaan, peluang per musim, transisi 40 s, hujan, kilat & guruh.
    public partial class URPAtmosphereAndWeather
    {
        public const float JamCekCuaca = 3f, DetikTransisi = 40f;
        const int HariPerMusim = 6;

        [SerializeField] Material materialHujan;
        [SerializeField] Light kilat;

        WeatherState _cuaca = WeatherState.Cerah, _cuacaLama = WeatherState.Cerah;
        float _transisi = 1f, _jamCekBerikut = 11f, _kilatBerikut = 10f, _kilatSisa, _guruhTunda = -1f;
        ParticleSystem _hujan;
        Dsp.Rng _rng = new Dsp.Rng(20261117u);

        public WeatherState Cuaca { get { return _cuaca; } }
        public static Season MusimHari(int hari) { return ((hari - 1) / HariPerMusim) % 2 == 0 ? Season.Kemarau : Season.Penghujan; }

        public void PasangCuaca(Material hujan, Light lampuKilat) { materialHujan = hujan; kilat = lampuKilat; }

        // Langsung (tanpa transisi) bila instan = true. Dipakai debug & self-test.
        public void SetCuaca(WeatherState w, bool instan)
        {
            if (w == _cuaca && _transisi >= 1f) return;
            _cuacaLama = instan ? w : _cuaca;
            _cuaca = w;
            _transisi = instan ? 1f : 0f;
            GameEvents.RaiseWeatherChanged(w);
            if (instan && _jam != null) Terapkan(_jam.Jam);
        }

        void UpdateCuaca()
        {
            if (_jam == null) return;
            if (_transisi < 1f) _transisi = Mathf.Min(1f, _transisi + Time.deltaTime / DetikTransisi * Mathf.Max(1f, _jam.Pengali * 0.1f));
            float jamAbs = (_jam.Hari - 1) * 24f + _jam.Jam;
            if (jamAbs >= _jamCekBerikut)
            {
                _jamCekBerikut = jamAbs + JamCekCuaca;
                SetCuaca(PilihCuaca(_jam.Hari, _jam.Jam), false);
            }
            PerbaruiHujan();
            PerbaruiKilat();
        }

        WeatherState PilihCuaca(int hari, float jam)
        {
            if (hari == 1 && jam < 18f) return WeatherState.Cerah;
            if (hari == 1) return WeatherState.Berawan;
            bool kemarau = MusimHari(hari) == Season.Kemarau;
            float p = _rng.Range(0f, 1f);
            WeatherState tujuan;
            if (kemarau) tujuan = p < 0.55f ? WeatherState.Cerah : p < 0.80f ? WeatherState.Berawan : p < 0.90f ? WeatherState.Gerimis : p < 0.95f ? WeatherState.HujanBadai : WeatherState.KabutAsap;
            else tujuan = p < 0.20f ? WeatherState.Cerah : p < 0.45f ? WeatherState.Berawan : p < 0.70f ? WeatherState.Gerimis : WeatherState.HujanBadai;
            // Transisi bertahap: Cerah tidak langsung ke Badai.
            if ((_cuaca == WeatherState.Cerah && tujuan == WeatherState.HujanBadai) || (_cuaca == WeatherState.HujanBadai && tujuan == WeatherState.Cerah)) tujuan = WeatherState.Berawan;
            return tujuan;
        }

        static void ParamCuaca(WeatherState w, out float cahaya, out float kabut, out float hujan, out Color tintKabut)
        {
            tintKabut = Color.white;
            switch (w)
            {
                case WeatherState.Berawan: cahaya = 0.75f; kabut = 0.0003f; hujan = 0f; break;
                case WeatherState.Gerimis: cahaya = 0.55f; kabut = 0.0012f; hujan = 0.3f; tintKabut = ProcTex.Hex("#B8C0C4"); break;
                case WeatherState.HujanBadai: cahaya = 0.3f; kabut = 0.0030f; hujan = 1f; tintKabut = ProcTex.Hex("#8A949A"); break;
                case WeatherState.KabutPagi: cahaya = 0.7f; kabut = 0.0040f; hujan = 0f; tintKabut = ProcTex.Hex("#D8DCDC"); break;
                case WeatherState.KabutAsap: cahaya = 0.6f; kabut = 0.0035f; hujan = 0f; tintKabut = ProcTex.Hex("#9A7A55"); break;
                case WeatherState.KabutDarah: cahaya = 0.5f; kabut = 0.0035f; hujan = 0f; tintKabut = ProcTex.Hex("#8A1A14"); break;
                default: cahaya = 1f; kabut = 0f; hujan = 0f; break;
            }
        }

        // Dipanggil di akhir Terapkan(): menimpa nilai jam dengan pengaruh cuaca (dicampur sesuai transisi).
        void TerapkanCuaca()
        {
            float c0, k0, h0, c1, k1, h1; Color t0, t1;
            ParamCuaca(_cuacaLama, out c0, out k0, out h0, out t0);
            ParamCuaca(_cuaca, out c1, out k1, out h1, out t1);
            float a = Mathf.SmoothStep(0f, 1f, _transisi);
            float cahaya = Mathf.Lerp(c0, c1, a), kabut = Mathf.Lerp(k0, k1, a);
            if (sun != null) sun.intensity *= cahaya;
            RenderSettings.fogDensity += kabut;
            Color tint = Color.Lerp(t0, t1, a);
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, RenderSettings.fogColor * tint * 1.2f, kabut > 0f ? 0.7f : 0f);
            if (skybox != null) SafeMaterialFactory.SetF(skybox, "_Exposure", skybox.GetFloat("_Exposure") * Mathf.Lerp(1f, 0.55f, 1f - cahaya));
        }

        float IntensitasHujan()
        {
            float c, k, h0, h1; Color t;
            ParamCuaca(_cuacaLama, out c, out k, out h0, out t);
            ParamCuaca(_cuaca, out c, out k, out h1, out t);
            return Mathf.Lerp(h0, h1, Mathf.SmoothStep(0f, 1f, _transisi));
        }

        void PerbaruiHujan()
        {
            float i = IntensitasHujan();
            if (i <= 0.01f) { if (_hujan != null && _hujan.isPlaying) _hujan.Stop(); return; }
            if (_hujan == null) BuatHujan();
            if (_hujan == null) return;
            var cam = Camera.main;
            if (cam != null) _hujan.transform.position = cam.transform.position + Vector3.up * 12f;
            var em = _hujan.emission; em.rateOverTime = 4500f * i;
            if (!_hujan.isPlaying) _hujan.Play();
        }

        void BuatHujan()
        {
            if (materialHujan == null) { Debug.LogWarning("[AK][Cuaca] Material hujan belum dipasang Builder; hujan visual dimatikan."); materialHujan = null; return; }
            var go = new GameObject("Hujan");
            go.transform.SetParent(transform, false);
            _hujan = go.AddComponent<ParticleSystem>();
            _hujan.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = _hujan.main;
            main.loop = true; main.startLifetime = 1.1f; main.startSpeed = 22f; main.startSize = 0.03f;
            main.maxParticles = 9000; main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new Color(0.75f, 0.8f, 0.85f, 0.45f);
            var shape = _hujan.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(40f, 1f, 40f);
            shape.rotation = new Vector3(90f, 0f, 0f);
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Stretch; r.velocityScale = 0.05f; r.lengthScale = 1f;
            r.sharedMaterial = materialHujan;
        }

        void PerbaruiKilat()
        {
            if (kilat == null) return;
            if (_kilatSisa > 0f)
            {
                _kilatSisa -= Time.deltaTime;
                kilat.intensity = _kilatSisa > 0.08f ? 3.5f : (_kilatSisa > 0.05f ? 0.2f : 2.4f);
                kilat.enabled = _kilatSisa > 0f;
            }
            if (_guruhTunda > 0f)
            {
                _guruhTunda -= Time.deltaTime;
                if (_guruhTunda <= 0f)
                {
                    var synth = GameServices.Get<ProceduralAudioSynth>();
                    if (synth != null) synth.Play(ProceduralAudioSynth.KlipGuruh, null, AudioCategory.Ambien, _rng.Range(0.6f, 1f), _rng.Range(0.85f, 1.1f), 1);
                }
            }
            if (_cuaca != WeatherState.HujanBadai || _transisi < 0.8f) return;
            _kilatBerikut -= Time.deltaTime;
            if (_kilatBerikut > 0f) return;
            _kilatBerikut = _rng.Range(8f, 25f);
            _kilatSisa = 0.18f;
            kilat.transform.rotation = Quaternion.Euler(_rng.Range(30f, 70f), _rng.Range(0f, 360f), 0f);
            float jarakKm = _rng.Range(0.3f, 3f);
            _guruhTunda = jarakKm * 1000f / 340f;
        }
    }
}
