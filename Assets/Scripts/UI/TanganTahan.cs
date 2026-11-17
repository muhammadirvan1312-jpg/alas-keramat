using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.EventSystems;
namespace AlasKeramat.UI
{
    public class TanganTahan : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        InteraksiAndi _aksi;
        public void OnPointerDown(PointerEventData e) { _aksi = GameServices.Get<InteraksiAndi>(); if (_aksi != null) _aksi.MulaiTahan(); }
        public void OnPointerUp(PointerEventData e) { Batal(); }
        public void OnPointerExit(PointerEventData e) { Batal(); }
        void OnDisable() { Batal(); }
        void Batal() { if (_aksi != null) _aksi.LepasTahan(); }
    }
}
