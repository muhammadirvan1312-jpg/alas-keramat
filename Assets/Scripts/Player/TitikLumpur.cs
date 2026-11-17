using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Player
{
    // 17 §2.4: genangan lumpur di rawa - "BALUR LUMPUR" (tahan 1 s) = deteksi visual x0,7 selama 10 menit; luntur saat hujan/berenang.
    public class TitikLumpur : MonoBehaviour, IInteractable
    {
        public float HoldDuration { get { return 1f; } }
        public string GetPromptText(GameObject actor) { return "BALUR LUMPUR"; }
        public bool CanInteract(GameObject actor) { var r = GameServices.Get<ReaksiAndi>(); return r != null && r.SisaLumpur < ReaksiAndi.LamaLumpur * 0.9f; }
        public void Interact(GameObject actor) { var r = GameServices.Get<ReaksiAndi>(); if (r != null) r.BalurLumpur(); }
    }
}
