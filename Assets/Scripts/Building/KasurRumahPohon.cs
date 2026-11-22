using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Kasur tikar di rumah pohon: tidur 6 jam memulihkan HP/Stamina/Sanity, menyembuhkan luka & simpan otomatis.
    public class KasurRumahPohon : MonoBehaviour, IInteractable
    {
        public float HoldDuration { get { return 0.6f; } }
        public string GetPromptText(GameObject actor) { return "TIDUR & SIMPAN (6 JAM)"; }
        public bool CanInteract(GameObject actor) { return true; }

        public void Interact(GameObject actor)
        {
            var vital = GameServices.Get<AndiVitalsAndSanity>();
            if (vital != null) vital.Pulihkan(6f, true);
            var save = GameServices.Get<SaveSystem>();
            if (save != null) save.Simpan();
            GameEvents.RaiseToast("Istirahat aman di rumah pohon. Kondisi pulih & progres tersimpan.");
        }
    }
}
