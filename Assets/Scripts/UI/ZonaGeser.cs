using UnityEngine;
using UnityEngine.EventSystems;

namespace AlasKeramat.UI
{
    // Area geser kamera (09 §2.4): jari pertama di area kamera memutar view. Delta dalam piksel layar.
    public class ZonaGeser : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, ICancelHandler, IInitializePotentialDragHandler
    {
        int _jari = int.MinValue;
        Vector2 _akumulasi;

        public void ResetSentuh() { _jari = int.MinValue; _akumulasi = Vector2.zero; }
        public Vector2 AmbilDelta() { var d = _akumulasi; _akumulasi = Vector2.zero; return d; }

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
