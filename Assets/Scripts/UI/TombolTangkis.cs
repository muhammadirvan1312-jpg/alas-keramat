using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AlasKeramat.UI
{
    // Tombol TANGKIS (06 §1): tekan = parry, tahan >= 0,4 s = blok, ketuk ganda = simpan/keluarkan senjata (H3: event pointer uGUI).
    public class TombolTangkis : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        int _jari=int.MinValue;
        CombatAndLeftHandController C() { return GameServices.Get<CombatAndLeftHandController>(); }
        public void OnPointerDown(PointerEventData e) { if(_jari!=int.MinValue || PengunciInput.Terblokir)return; _jari=e.pointerId; var c = C(); if (c != null) c.TangkisTekan(); }
        public void OnPointerUp(PointerEventData e) { if(e.pointerId==_jari)Batal(); }
        void OnDisable() { Batal(); }
        void OnApplicationFocus(bool fokus) { if(!fokus)Batal(); }
        void Batal() { _jari=int.MinValue;var c=C();if(c!=null)c.TangkisLepas(); }
    }
}
