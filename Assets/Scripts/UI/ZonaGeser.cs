using UnityEngine;
using UnityEngine.EventSystems;

namespace AlasKeramat.UI
{
    // Area geser kamera (09 §2.4): jari pertama di area kamera memutar view. Delta dalam piksel layar.
    public class ZonaGeser : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, ICancelHandler, IInitializePotentialDragHandler
    {
        public const float AmbangAksel = 14f, MaksAksel = 1.65f, FaktorHalus = 0.62f;

        int _jari = int.MinValue;
        Vector2 _akumulasi, _halus;

        public static Vector2 HitungKurvaGeser(Vector2 delta)
        {
            float m = delta.magnitude;
            if (m < 0.05f) return Vector2.zero;
            float aksel = m <= AmbangAksel ? 1f : Mathf.Lerp(1f, MaksAksel, Mathf.Clamp01((m - AmbangAksel) / 42f));
            return delta * aksel;
        }

        public void ResetSentuh() { _jari = int.MinValue; _akumulasi = Vector2.zero; _halus = Vector2.zero; }
        public Vector2 AmbilDelta()
        {
            Vector2 target = HitungKurvaGeser(_akumulasi);
            _akumulasi = Vector2.zero;
            _halus = Vector2.Lerp(_halus, target, FaktorHalus);
            if (_halus.sqrMagnitude < 0.002f) _halus = Vector2.zero;
            return _halus;
        }

        public void OnInitializePotentialDrag(PointerEventData e) { e.useDragThreshold = false; }
        void OnDisable() { ResetSentuh(); }
        void OnApplicationFocus(bool fokus) { if (!fokus) ResetSentuh(); }
        public void OnPointerDown(PointerEventData e) { if (_jari == int.MinValue) _jari = e.pointerId; }
        public void OnDrag(PointerEventData e) { if (e.pointerId == _jari) _akumulasi += e.delta; }
        public void OnPointerUp(PointerEventData e) { if (e.pointerId == _jari) _jari = int.MinValue; }
        public void OnCancel(BaseEventData e) { ResetSentuh(); }
        void Update()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            if (_jari != int.MinValue && Input.touchSupported && Input.touchCount == 0) _jari = int.MinValue;
#endif
        }
    }
}
