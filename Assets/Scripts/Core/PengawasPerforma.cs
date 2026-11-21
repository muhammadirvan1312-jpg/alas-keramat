using AlasKeramat.Building;
using AlasKeramat.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AlasKeramat.Core
{
    // Penjaga frame-rate adaptif (zero-alloc) di Android: otomatis turunkan skala render & kepadatan bila FPS jatuh berkepanjangan.
    public class PengawasPerforma : MonoBehaviour
    {
        public const int JendelaTurun = 2, JendelaNaik = 4; // histeresis: turun setelah 6 s buruk, naik setelah 12 s baik (cegah naik-turun)
        float _waktu, _skala = 1f;
        int _frame, _buruk, _baik;
        VegetationManager _veg;
        SerasahHutan _serasah;
        MenuScreens _menu;

        void Update()
        {
            if (!Application.isPlaying || JedaDunia.Dijeda) return;
            _frame++;
            _waktu += Time.unscaledDeltaTime;
            if (_waktu < 3f) return;
            float fps = _frame / _waktu;
            _frame = 0; _waktu = 0f;

            if (_menu == null) _menu = GameServices.Get<MenuScreens>();
            int target = _menu != null ? _menu.TargetFps : 60;
            float ambangTurun = target >= 55 ? 45f : 26f;
            float baru = _skala;
            bool buruk = fps < ambangTurun, baik = fps >= target * 0.9f;
            _buruk = buruk ? _buruk + 1 : 0; _baik = baik ? _baik + 1 : 0;
            if (_buruk >= JendelaTurun) { baru = Mathf.Max(0.70f, _skala - 0.1f); _buruk = 0; }
            else if (_baik >= JendelaNaik && _skala < 1f) { baru = Mathf.Min(1f, _skala + 0.05f); _baik = 0; }
            if (Mathf.Approximately(baru, _skala)) return;
            _skala = baru;
            Terapkan();
        }

        void Terapkan()
        {
            var dp = _menu != null ? MenuScreens.AmbilPreset(_menu.Preset) : MenuScreens.AmbilPreset(PresetGrafis.Sedang);
            if (_veg == null) _veg = GameServices.Get<VegetationManager>();
            if (_veg != null) { _veg.PengaliDetail = dp.detailVeg * _skala; _veg.PengaliJarak = dp.pengaliVeg * Mathf.Lerp(0.8f, 1f, (_skala - 0.7f) / 0.3f); }
            if (_serasah == null) _serasah = GameServices.Get<SerasahHutan>();
            if (_serasah != null) _serasah.Kepadatan = dp.rumput * _skala;
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp != null && urp.name.EndsWith("_RuntimeClone"))
                urp.renderScale = Mathf.Clamp(dp.renderScale * _skala, 0.65f, 1f);
        }
    }
}
