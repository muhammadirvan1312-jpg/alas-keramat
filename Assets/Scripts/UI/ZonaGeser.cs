using UnityEngine;
using UnityEngine.EventSystems;

namespace AlasKeramat.UI
{
    // Area geser kamera (09 §2.4): jari pertama di area kanan memutar kamera. Delta dalam piksel layar.
    public class ZonaGeser : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        int _jari = int.MinValue;
        Vector2 _akumulasi;

        public Vector2 AmbilDelta() { var d = _akumulasi; _akumulasi = Vector2.zero; return d; }

        public void OnPointerDown(PointerEventData e) { if (_jari == int.MinValue) _jari = e.pointerId; }
        public void OnDrag(PointerEventData e) { if (e.pointerId == _jari) _akumulasi += e.delta; }
        public void OnPointerUp(PointerEventData e) { if (e.pointerId == _jari) _jari = int.MinValue; }
    }
}
