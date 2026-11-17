using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    // Tombol TEBAS (09 §2.5, 06 §1): ketuk = tebasan ringan, tahan >= 0,35 s lalu lepas = tebasan berat; senjata jarak: tahan = tarik/bidik, lepas = tembak/lempar.
    // Label berganti otomatis: TEBAS / BUNUH SENYAP / TARIK / LEMPAR / TEMBAK (H3: event pointer uGUI).
    public class TombolTahan : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public const float AmbangBerat = 0.35f, JedaLabel = 0.2f;
        public static bool Berat(float lama) { return lama >= AmbangBerat; }

        Text _label;
        float _tLabel;
        string _isi;

        CombatAndLeftHandController C() { return GameServices.Get<CombatAndLeftHandController>(); }
        public void OnPointerDown(PointerEventData e) { var c = C(); if (c != null) c.TekanTebas(); }
        public void OnPointerUp(PointerEventData e) { var c = C(); if (c != null) c.LepasTebas(); }

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
