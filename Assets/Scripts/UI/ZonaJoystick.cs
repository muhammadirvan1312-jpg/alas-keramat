using UnityEngine;
using UnityEngine.EventSystems;

namespace AlasKeramat.UI
{
    // Joystick mengambang (09 §2.3): muncul di titik sentuh pada zona kiri-bawah. Input lewat event pointer uGUI (H3).
    public class ZonaJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IInitializePotentialDragHandler
    {
        public const float RadiusGerak = 110f, AlphaDiam = 0.35f, AlphaAktif = 0.8f;
        public const float AmbangSprint = 0.82f, SudutSprint = 42f, TahanSprint = 0.15f;

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
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, e.position, e.pressEventCamera, out lokal)) _asal = lokal;
            if (basis != null) basis.anchoredPosition = BatasBasis(_asal, _rt.rect, basis.rect.width * 0.5f);
            Warna(AlphaAktif);
            OnDrag(e);
        }

        public void OnDrag(PointerEventData e)
        {
            if (e.pointerId != _jari) return;
            Vector2 lokal;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, e.position, e.pressEventCamera, out lokal)) return;
            Vector2 d = Vector2.ClampMagnitude(lokal - _asal, RadiusGerak);
            if (kenop != null) kenop.anchoredPosition = d;
            Nilai = d / RadiusGerak;
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (e.pointerId != _jari) return;
            Lepas();
        }

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
