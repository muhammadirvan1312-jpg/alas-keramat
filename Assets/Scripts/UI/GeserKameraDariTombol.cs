using AlasKeramat.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AlasKeramat.UI
{
    // Memungkinkan geseran ibu jari yang dimulai di atas tombol aksi kanan tetap memutar kamera pemain dengan mulus.
    public class GeserKameraDariTombol : MonoBehaviour, IDragHandler, IInitializePotentialDragHandler
    {
        public void OnInitializePotentialDrag(PointerEventData e) { e.useDragThreshold = false; }

        public void OnDrag(PointerEventData e)
        {
            var tc = GameServices.Get<TouchControls>();
            if (tc != null) tc.GeserDariTombol(e.delta);
        }
    }
}
