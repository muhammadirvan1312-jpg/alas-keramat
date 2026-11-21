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
        ChromaticAberration _chr;
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
            p.TryGet(out _ca); p.TryGet(out _wb); p.TryGet(out _vig); p.TryGet(out _bloom); p.TryGet(out _chr);
            _skrg = Target(TimeOfDayClock.JamMulai, WeatherState.Cerah);
        }

        AlasKeramat.Player.AndiVitalsAndSanity _vital;
        AlasKeramat.Player.AndiFPSController _andi;
        float _kritis, _selam01, _hipo01;
        static readonly Color WarnaVigDasar = new Color(0.03f, 0.02f, 0.015f);
        public const float AmbangKritis = 0.35f;
        // 0 saat HP >= 35%, naik ke 1 saat HP mendekati 0.
        public static float Kritis01(float hp01) { return Mathf.Clamp01(1f - hp01 / AmbangKritis); }
        public static float FaktorHipotermia01(float suhuTubuh)
        {
            return Mathf.Clamp01((AlasKeramat.Player.AndiVitalsAndSanity.SuhuKedinginan - suhuTubuh) / 1.5f);
        }
        public static Vector4 EfekLingkunganTubuh(Vector4 v, float selam01, float hipo01)
        {
            float s = Mathf.Clamp01(selam01);
            float h = Mathf.Clamp01(hipo01);
            if (s > 0f) { v.x -= 0.35f * s; v.y -= 18f * s; v.z -= 26f * s; v.w = Mathf.Min(0.62f, v.w + 0.18f * s); }
            if (h > 0f) { v.x -= 0.12f * h; v.y -= 32f * h; v.z -= 28f * h; v.w = Mathf.Min(0.65f, v.w + 0.20f * h); }
            return v;
        }
        public static Color WarnaFilterTubuh(float selam01, float hipo01)
        {
            Color c = Color.white;
            float s = Mathf.Clamp01(selam01);
            float h = Mathf.Clamp01(hipo01);
            if (h > 0f) c = Color.Lerp(c, new Color(0.80f, 0.90f, 1.0f), h * 0.75f);
            if (s > 0f) c = Color.Lerp(c, new Color(0.54f, 0.85f, 0.93f), s * 0.85f);
            return c;
        }
        public static float HitungAberasiKromatik(float kritis01, float kewarasan01)
        {
            float tidakWaras = Mathf.Clamp01(1f - kewarasan01 / 0.45f);
            return Mathf.Clamp(0.03f + kritis01 * 0.38f + tidakWaras * 0.28f, 0f, 0.75f);
        }
        // HP kritis: warna memudar, tepi layar menggelap dan berdenyut pelan seperti detak jantung.
        public static Vector4 EfekKritis(Vector4 v, float k, float waktu)
        {
            if (k <= 0f) return v;
            float denyut = 0.5f + 0.5f * Mathf.Sin(waktu * 5.2f);
            v.y -= 55f * k; v.w = Mathf.Min(0.6f, v.w + k * (0.16f + 0.08f * denyut)); v.x -= 0.15f * k;
            return v;
        }

        // x eksposur (EV), y saturasi, z suhu WB, w intensitas vignette.
        public static Vector4 TargetGua { get { return new Vector4(1.28f,-2f,-3f,0.20f); } }

        // Pewarnaan halus per bioma/cuaca (menggantikan LUT 3D tanpa shader/fitur kustom).
        public static Color FilterBioma(Biome b, WeatherState w, float jam)
        {
            Color dasar = Color.white;
            if (b == Biome.BawahTanah) dasar = new Color(0.86f, 0.96f, 0.98f);
            else if (b == WorldTerrainGenerator.RawaBakau) dasar = new Color(0.88f, 0.95f, 0.86f);
            else if (jam >= 17.2f && jam < 19.0f) dasar = new Color(1.0f, 0.93f, 0.86f);
            else if (jam >= 4.5f && jam < 7.0f) dasar = new Color(0.92f, 0.95f, 1.0f);
            if (w == WeatherState.HujanBadai) dasar = Color.Lerp(dasar, new Color(0.84f, 0.90f, 0.98f), 0.55f);
            else if (w == WeatherState.KabutDarah) dasar = Color.Lerp(dasar, new Color(1.0f, 0.72f, 0.68f), 0.5f);
            return dasar;
        }

        Color _filter = Color.white, _targetFilter = Color.white;
        public static Vector4 Target(float jam, WeatherState cuaca)
        {
            Vector4 siang = new Vector4(0.32f, 3f, 3f, 0.16f);
            Vector4 maghrib = new Vector4(0.24f, 6f, 18f, 0.22f);
            Vector4 malam = new Vector4(0.30f, -8f, -12f, 0.22f);
            Vector4 fajar = new Vector4(0.24f, 3f, 12f, 0.22f);
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
            v.x = Mathf.Max(-1.2f, v.x);
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
                if (_andi == null) _andi = GameServices.Get<AlasKeramat.Player.AndiFPSController>();
                float jv = _jam != null ? _jam.Jam : TimeOfDayClock.JamMulai;
                var wv = _atmos != null ? _atmos.Cuaca : WeatherState.Cerah;
                _target = URPAtmosphereAndWeather.DalamRuangan ? TargetGua : Target(jv, wv);
                Biome bio = URPAtmosphereAndWeather.DalamRuangan ? Biome.BawahTanah : (_andi != null ? WorldTerrainGenerator.GetBiome(_andi.transform.position.x, _andi.transform.position.z) : Biome.HutanCampuran);
                _targetFilter = FilterBioma(bio, wv, jv);
            }
            _skrg = Vector4.Lerp(_skrg, _target, Time.deltaTime * Lerp);
            _filter = Color.Lerp(_filter, _targetFilter, Time.deltaTime * Lerp);
            if (_vital == null) _vital = GameServices.Get<AlasKeramat.Player.AndiVitalsAndSanity>();
            if (_andi == null) _andi = GameServices.Get<AlasKeramat.Player.AndiFPSController>();
            float hp = _vital != null ? _vital.Health / AlasKeramat.Player.AndiVitalsAndSanity.Maks : 1f;
            _kritis = Mathf.MoveTowards(_kritis, Kritis01(hp), Time.deltaTime * 0.8f);
            _selam01 = Mathf.MoveTowards(_selam01, (_andi != null && _andi.Menyelam) ? 1f : 0f, Time.deltaTime * 3.5f);
            _hipo01 = Mathf.MoveTowards(_hipo01, _vital != null ? FaktorHipotermia01(_vital.SuhuTubuh) : 0f, Time.deltaTime * 1.2f);
            var o = EfekLingkunganTubuh(EfekKritis(_skrg, _kritis, Time.time), _selam01, _hipo01);
            if (_ca != null) { _ca.postExposure.value = o.x; _ca.saturation.value = o.y; _ca.colorFilter.value = _filter * WarnaFilterTubuh(_selam01, _hipo01); }
            if (_wb != null) _wb.temperature.value = o.z;
            if (_vig != null)
            {
                _vig.intensity.value = o.w;
                Color vc = Color.Lerp(WarnaVigDasar, new Color(0.18f, 0.34f, 0.52f), _hipo01 * 0.85f);
                _vig.color.value = Color.Lerp(vc, new Color(0.02f, 0.16f, 0.24f), _selam01 * 0.9f);
            }
            if (_chr != null)
            {
                float san01 = _vital != null ? _vital.Sanity / AlasKeramat.Player.AndiVitalsAndSanity.Maks : 1f;
                _chr.intensity.value = Mathf.Clamp(HitungAberasiKromatik(_kritis, san01) + _hipo01 * 0.18f + _selam01 * 0.08f, 0f, 0.78f);
            }
        }
    }
}
