using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    // Tombol TEBAS (09 §2.5, 06 §1): ketuk = tebasan ringan, tahan >= 0,35 s lalu lepas = tebasan berat; senjata jarak: tahan = tarik/bidik, lepas = tembak/lempar.
    // Label berganti otomatis: TEBAS / BUNUH SENYAP / TARIK / LEMPAR / TEMBAK (H3: event pointer uGUI).
    public class TombolTahan : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IInitializePotentialDragHandler
    {
        public const float AmbangBerat = 0.35f, JedaLabel = 0.2f;
        public static bool Berat(float lama) { return lama >= AmbangBerat; }

        int _jari=int.MinValue;
        Text _label;
        float _tLabel;
        string _isi;

        CombatAndLeftHandController C() { return GameServices.Get<CombatAndLeftHandController>(); }
        public void OnPointerDown(PointerEventData e) { if(_jari!=int.MinValue || Time.timeScale<=0f)return;_jari=e.pointerId;var c = C(); if (c != null) c.TekanTebas(); }
        public void OnPointerUp(PointerEventData e) { if(e.pointerId!=_jari)return;_jari=int.MinValue;var c = C(); if (c != null && Time.timeScale>0f) c.LepasTebas(); }
        public void ResetSentuh() { _jari=int.MinValue;var c=C();if(c!=null)c.BatalkanTebas(); }
        void OnDisable() { ResetSentuh(); }
        void OnApplicationFocus(bool fokus) { if(!fokus)ResetSentuh(); }

        public void OnInitializePotentialDrag(PointerEventData e) { e.useDragThreshold=false; }
        public void OnDrag(PointerEventData e) { if(e.pointerId!=_jari)return;var tc=GameServices.Get<TouchControls>();if(tc!=null)tc.GeserDariTombol(e.delta); }

        void Update()
        {
            _tLabel -= Time.unscaledDeltaTime;
            if (_tLabel > 0f) return;
            _tLabel = JedaLabel;
            if (_label == null) _label = GetComponentInChildren<Text>();
            var c = C();
            if (_label == null || c == null) return;
            string s = c.LabelTebas();
            if (s != _isi) { _isi = s; _label.text = s; _label.fontSize = s.Length > 8 ? 22 : 30; }
        }
    }
}
