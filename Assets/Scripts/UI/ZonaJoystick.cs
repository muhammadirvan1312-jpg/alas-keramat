using UnityEngine;
using UnityEngine.EventSystems;

namespace AlasKeramat.UI
{
    // Joystick mengambang (09 §2.3): muncul di titik sentuh pada zona kiri-bawah. Input lewat event pointer uGUI (H3).
    public class ZonaJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerExitHandler, ICancelHandler, IInitializePotentialDragHandler
    {
        public const float RadiusGerak = 110f, AlphaDiam = 0.35f, AlphaAktif = 0.8f;
        public const float AmbangSprint = 0.78f, SudutSprint = 50f, TahanSprint = 0.10f;

        [SerializeField] RectTransform basis;
        [SerializeField] RectTransform kenop;
        [SerializeField] UnityEngine.UI.Image cincin;
        [SerializeField] UnityEngine.UI.Text labelLari;

        public Vector2 Nilai { get; private set; }
        public bool Lari { get; private set; }

        int _jari = int.MinValue;
        Vector2 _asal, _posisiDiam;
        float _tahan;
        RectTransform _rt;

        public void Pasang(RectTransform b, RectTransform k, UnityEngine.UI.Image c, UnityEngine.UI.Text lari)
        {
            basis = b; kenop = k; cincin = c; labelLari = lari;
        }

        void Awake()
        {
            _rt = (RectTransform)transform;
            if (basis != null) _posisiDiam = basis.anchoredPosition;
            Lepas();
        }

        public void OnInitializePotentialDrag(PointerEventData e) { e.useDragThreshold = false; }
        void OnDisable() { ResetSentuh(); }
        void OnApplicationFocus(bool fokus) { if (!fokus) ResetSentuh(); }
        public void OnPointerDown(PointerEventData e)
        {
            if (_jari != int.MinValue || PengunciInput.Terblokir) return;
            _jari = e.pointerId;
            Vector2 lokal;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, e.position, e.pressEventCamera, out lokal))
                _asal = basis != null ? BatasBasis(lokal, _rt.rect, basis.rect.width * 0.5f) : lokal;
            if (basis != null) basis.anchoredPosition = _asal;
            Warna(AlphaAktif);
            OnDrag(e);
        }

        public void OnDrag(PointerEventData e)
        {
            if (e.pointerId != _jari) return;
            Vector2 lokal;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, e.position, e.pressEventCamera, out lokal)) return;
            Vector2 selisih = lokal - _asal;
            float jarak = selisih.magnitude;
            if (jarak > RadiusGerak * 1.15f)
            {
                Vector2 targetAsal = lokal - (selisih / jarak) * RadiusGerak;
                _asal = basis != null ? BatasBasis(targetAsal, _rt.rect, basis.rect.width * 0.5f) : targetAsal;
                if (basis != null) basis.anchoredPosition = _asal;
                selisih = lokal - _asal;
                jarak = selisih.magnitude;
            }
            Vector2 d = Vector2.ClampMagnitude(selisih, RadiusGerak);
            if (kenop != null) kenop.anchoredPosition = d;
            float m = Mathf.Clamp01(d.magnitude / RadiusGerak);
            if (m < 0.06f) Nilai = Vector2.zero;
            else
            {
                float r = Mathf.Lerp(0.24f, 1f, Mathf.InverseLerp(0.06f, 0.94f, m));
                Nilai = (d / d.magnitude) * r;
            }
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (e.pointerId != _jari) return;
            Lepas();
        }

        public void OnPointerExit(PointerEventData e) { }
        public void OnCancel(BaseEventData e) { Lepas(); }

        public void ResetSentuh() { Lepas(); }

        void Lepas()
        {
            _jari = int.MinValue; Nilai = Vector2.zero; Lari = false; _tahan = 0f;
            if (kenop != null) kenop.anchoredPosition = Vector2.zero;
            if (basis != null) basis.anchoredPosition = _posisiDiam;
            Warna(AlphaDiam);
            if (labelLari != null) labelLari.enabled = false;
        }

        public static bool PosisiSprint(Vector2 nilai)
        {
            return nilai.magnitude >= AmbangSprint && Vector2.Angle(Vector2.up, nilai) <= SudutSprint;
        }

        public static Vector2 BatasBasis(Vector2 titik, Rect zona, float radius)
        {
            float rx = Mathf.Min(radius, zona.width * 0.5f), ry = Mathf.Min(radius, zona.height * 0.5f);
            return new Vector2(Mathf.Clamp(titik.x, zona.xMin + rx, zona.xMax - rx), Mathf.Clamp(titik.y, zona.yMin + ry, zona.yMax - ry));
        }

        void Update()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            if (_jari != int.MinValue && Input.touchSupported && Input.touchCount == 0) { Lepas(); return; }
#endif
            bool pos = _jari != int.MinValue && (Lari ? (Nilai.magnitude >= 0.72f && Vector2.Angle(Vector2.up, Nilai) <= 55f) : PosisiSprint(Nilai));
            _tahan = pos ? _tahan + Time.unscaledDeltaTime : 0f;
            bool lari = Lari ? pos : _tahan >= TahanSprint;
            if (lari != Lari)
            {
                Lari = lari;
                if (cincin != null) cincin.color = lari ? new Color(1f, 0.6f, 0.235f, AlphaAktif) : new Color(1f, 1f, 1f, AlphaAktif);
                if (labelLari != null) labelLari.enabled = lari;
            }
        }

        void Warna(float a)
        {
            if (cincin != null) cincin.color = new Color(1f, 1f, 1f, a);
            var img = kenop != null ? kenop.GetComponent<UnityEngine.UI.Image>() : null;
            if (img != null) img.color = new Color(1f, 1f, 1f, a);
        }
    }
}
