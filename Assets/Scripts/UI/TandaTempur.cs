using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    // 06 §1 & §3: tanda kena putih 0,15 s di crosshair saat tebasan mengenai musuh; vignette merah 0,3 s saat Andi terluka.
    public class TandaTempur : MonoBehaviour
    {
        public const float LamaKena = 0.15f, LamaMerah = 0.3f, AlphaMerahMaks = 0.55f;

        [SerializeField] Image tandaKena;
        [SerializeField] Image vignette;
        [SerializeField] Image ikonDarah;
        [SerializeField] RectTransform cincinSuara;
        ReaksiAndi _reaksi;
        float _tKena, _tMerah, _kuatMerah;
        bool _berat;

        public void Pasang(Image kena, Image merah) { tandaKena = kena; vignette = merah; }
        public void PasangIkonDarah(Image ikon) { ikonDarah = ikon; }
        public void PasangCincinSuara(RectTransform r) { cincinSuara = r; }

        void KenaJarak() { Kena(false); }
        void OnEnable() { CombatAndLeftHandController.OnKena += Kena; CombatAndLeftHandController.OnKenaJarak += KenaJarak; GameEvents.OnPlayerDamaged += Terluka; Set(tandaKena, 0f); Set(vignette, 0f); }
        void OnDisable() { CombatAndLeftHandController.OnKenaJarak -= KenaJarak; CombatAndLeftHandController.OnKena -= Kena; GameEvents.OnPlayerDamaged -= Terluka; }

        void Kena(bool berat) { _tKena = LamaKena; _berat = berat; }
        void Terluka(DamageInfo d) { _tMerah = LamaMerah; _kuatMerah = Mathf.Clamp01(0.4f + d.amount / 40f); }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (cincinSuara != null)
            {
                if (_reaksi == null) _reaksi = GameServices.Get<ReaksiAndi>();
                float r = _reaksi != null ? _reaksi.RadiusSuara : 0f;
                float target = 40f + r * 5f; // meter suara di sekitar crosshair (17 §2.4)
                float now = Mathf.Lerp(cincinSuara.sizeDelta.x, target, dt * 6f);
                cincinSuara.sizeDelta = new Vector2(now, now);
            }
            if (ikonDarah != null)
            {
                if (_reaksi == null) _reaksi = GameServices.Get<ReaksiAndi>();
                bool darah = _reaksi != null && _reaksi.Berdarah;
                if (ikonDarah.enabled != darah) ikonDarah.enabled = darah;
                if (darah) { var c = ikonDarah.color; c.a = 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 6f); ikonDarah.color = c; } // berdenyut (14 §4)
            }
            if (_tKena > 0f || (tandaKena != null && tandaKena.color.a > 0f))
            {
                _tKena = Mathf.Max(0f, _tKena - dt);
                Set(tandaKena, _tKena / LamaKena);
                if (tandaKena != null) tandaKena.rectTransform.localScale = Vector3.one * (_berat ? 1.35f : 1f);
            }
            if (_tMerah > 0f || (vignette != null && vignette.color.a > 0f))
            {
                _tMerah = Mathf.Max(0f, _tMerah - dt);
                Set(vignette, _tMerah / LamaMerah * AlphaMerahMaks * _kuatMerah);
            }
        }

        static void Set(Image img, float a)
        {
            if (img == null) return;
            var c = img.color; c.a = a; img.color = c;
            if (img.enabled != a > 0.001f) img.enabled = a > 0.001f;
        }
    }
}
