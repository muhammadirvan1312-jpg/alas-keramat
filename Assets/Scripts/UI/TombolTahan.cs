using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AlasKeramat.UI
{
    // Tombol TEBAS (09 §2.5): ketuk = tebasan ringan, tahan >= 0,35 s lalu lepas = tebasan berat (H3: event pointer uGUI).
    public class TombolTahan : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public const float AmbangBerat = 0.35f;
        float _mulai = -1f;
        public static bool Berat(float lama) { return lama >= AmbangBerat; }
        public void OnPointerDown(PointerEventData e) { _mulai = Time.unscaledTime; }
        public void OnPointerUp(PointerEventData e)
        {
            if (_mulai < 0f) return;
            float lama = Time.unscaledTime - _mulai; _mulai = -1f;
            var c = GameServices.Get<CombatAndLeftHandController>();
            if (c != null) c.Tebas(Berat(lama));
        }
    }
}
