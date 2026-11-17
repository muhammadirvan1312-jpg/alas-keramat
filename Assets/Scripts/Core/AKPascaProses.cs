using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AlasKeramat.Core
{
    // Pasca-proses sinematik mengikuti jam & cuaca: siang hangat-kontras, Maghrib merah-oranye, malam biru dingin & gelap, badai kelabu.
    public class AKPascaProses : MonoBehaviour
    {
        public const float Interval = 0.25f, Lerp = 2f;

        [SerializeField] Volume volume;

        ColorAdjustments _ca;
        WhiteBalance _wb;
        Vignette _vig;
        Bloom _bloom;
        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _atmos;
        float _t;
        Vector4 _target, _skrg; // x eksposur, y saturasi, z suhu WB, w vignette

        public void Pasang(Volume v) { volume = v; }

        void Awake()
        {
            GameServices.Register(this);
            if (volume == null) volume = GetComponent<Volume>();
            if (volume == null || volume.sharedProfile == null) { Debug.LogError("[AK][AKPascaProses] Volume/profil tidak ada, pasca-proses statis."); enabled = false; return; }
            var p = volume.profile; // salinan runtime: aset profil tidak berubah saat Play di Editor
            p.TryGet(out _ca); p.TryGet(out _wb); p.TryGet(out _vig); p.TryGet(out _bloom);
            _skrg = Target(TimeOfDayClock.JamMulai, WeatherState.Cerah);
        }

        // x eksposur (EV), y saturasi, z suhu WB, w intensitas vignette.
        public static Vector4 Target(float jam, WeatherState cuaca)
        {
            Vector4 siang = new Vector4(0.15f, 12f, 4f, 0.22f);
            Vector4 maghrib = new Vector4(0.0f, 18f, 28f, 0.30f);
            Vector4 malam = new Vector4(-0.45f, -18f, -22f, 0.38f);
            Vector4 fajar = new Vector4(0.05f, 4f, 14f, 0.28f);
            Vector4 v;
            if (jam >= 7f && jam < 16.5f) v = siang;
            else if (jam >= 16.5f && jam < 18f) v = Vector4.Lerp(siang, maghrib, (jam - 16.5f) / 1.5f);
            else if (jam >= 18f && jam < 19.5f) v = Vector4.Lerp(maghrib, malam, (jam - 18f) / 1.5f);
            else if (jam >= 4.5f && jam < 6f) v = Vector4.Lerp(malam, fajar, (jam - 4.5f) / 1.5f);
            else if (jam >= 6f && jam < 7f) v = Vector4.Lerp(fajar, siang, jam - 6f);
            else v = malam;
            if (cuaca == WeatherState.HujanBadai) { v.x -= 0.25f; v.y -= 22f; v.z -= 8f; v.w += 0.06f; }
            else if (cuaca == WeatherState.Gerimis || cuaca == WeatherState.Berawan) { v.x -= 0.08f; v.y -= 8f; }
            else if (cuaca == WeatherState.KabutDarah) { v.y += 10f; v.z += 35f; v.w += 0.1f; }
            return v;
        }

        void Update()
        {
            _t += Time.deltaTime;
            if (_t >= Interval)
            {
                _t = 0f;
                if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
                if (_atmos == null) _atmos = GameServices.Get<URPAtmosphereAndWeather>();
                _target = Target(_jam != null ? _jam.Jam : TimeOfDayClock.JamMulai, _atmos != null ? _atmos.Cuaca : WeatherState.Cerah);
            }
            _skrg = Vector4.Lerp(_skrg, _target, Time.deltaTime * Lerp);
            if (_ca != null) { _ca.postExposure.value = _skrg.x; _ca.saturation.value = _skrg.y; }
            if (_wb != null) _wb.temperature.value = _skrg.z;
            if (_vig != null) _vig.intensity.value = _skrg.w;
        }
    }
}
