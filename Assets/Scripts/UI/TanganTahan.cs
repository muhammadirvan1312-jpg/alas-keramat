using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.EventSystems;
namespace AlasKeramat.UI
{
    public class TanganTahan : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        int _jari=int.MinValue;
        InteraksiAndi _aksi;
        public void OnPointerDown(PointerEventData e) { if(_jari!=int.MinValue || PengunciInput.Terblokir)return; _jari=e.pointerId; _aksi = GameServices.Get<InteraksiAndi>(); if (_aksi != null) _aksi.MulaiTahan(); }
        public void OnPointerUp(PointerEventData e) { if(e.pointerId==_jari)Batal(); }
        public void OnPointerExit(PointerEventData e) { if(e.pointerId==_jari)Batal(); }
        void OnDisable() { Batal(); }
        void OnApplicationFocus(bool fokus) { if(!fokus)Batal(); }
        void Batal() { _jari=int.MinValue; if (_aksi != null) _aksi.LepasTahan(); }
    }
}
