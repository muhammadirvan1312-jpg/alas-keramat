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
        public static Vector4 TargetGua { get { return new Vector4(1.28f,-2f,-3f,0.20f); } }
        public static Vector4 Target(float jam, WeatherState cuaca)
        {
            Vector4 siang = new Vector4(0.28f, 2f, 3f, 0.16f);
            Vector4 maghrib = new Vector4(0.24f, 6f, 18f, 0.22f);
            Vector4 malam = new Vector4(0.24f, -8f, -14f, 0.22f);
            Vector4 fajar = new Vector4(0.20f, 3f, 12f, 0.22f);
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

        public static void TerapkanFoto(VolumeProfile profile,float jam,WeatherState cuaca,bool bawah)
        {
            if(profile==null)return;var v=bawah?TargetGua:Target(jam,cuaca);
            ColorAdjustments ca;WhiteBalance wb;Vignette vig;
            if(profile.TryGet(out ca)) { ca.postExposure.value=v.x;ca.saturation.value=v.y; }
            if(profile.TryGet(out wb))wb.temperature.value=v.z;
            if(profile.TryGet(out vig))vig.intensity.value=v.w;
        }
        void Update()
        {
            _t += Time.deltaTime;
            if (_t >= Interval)
            {
                _t = 0f;
                if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
                if (_atmos == null) _atmos = GameServices.Get<URPAtmosphereAndWeather>();
                _target = URPAtmosphereAndWeather.DalamRuangan ? TargetGua : Target(_jam != null ? _jam.Jam : TimeOfDayClock.JamMulai, _atmos != null ? _atmos.Cuaca : WeatherState.Cerah);
            }
            _skrg = Vector4.Lerp(_skrg, _target, Time.deltaTime * Lerp);
            if (_ca != null) { _ca.postExposure.value = _skrg.x; _ca.saturation.value = _skrg.y; }
            if (_wb != null) _wb.temperature.value = _skrg.z;
            if (_vig != null) _vig.intensity.value = _skrg.w;
        }
    }
}
