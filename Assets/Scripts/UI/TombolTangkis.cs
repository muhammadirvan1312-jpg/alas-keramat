using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AlasKeramat.UI
{
    // Tombol TANGKIS (06 §1): tekan = parry, tahan >= 0,4 s = blok, ketuk ganda = simpan/keluarkan senjata (H3: event pointer uGUI).
    public class TombolTangkis : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        CombatAndLeftHandController C() { return GameServices.Get<CombatAndLeftHandController>(); }
        public void OnPointerDown(PointerEventData e) { var c = C(); if (c != null) c.TangkisTekan(); }
        public void OnPointerUp(PointerEventData e) { var c = C(); if (c != null) c.TangkisLepas(); }
    }
}
