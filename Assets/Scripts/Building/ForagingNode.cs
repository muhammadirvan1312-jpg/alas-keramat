using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Titik pungut (03 §7): batu, sirih, kunyit, jahe, jamur, jamur beracun, sarang telur. Proxy dari ForagingDirector.
    public class ForagingNode : MonoBehaviour, IInteractable
    {
        public int Indeks = -1;
        public ForagingDirector.JenisPungut Tipe;
        public float HoldDuration { get { return 0f; } }
        public string GetPromptText(GameObject actor) { return "AMBIL " + ItemDatabase.Get(ForagingDirector.Item(Tipe)).displayName.ToUpperInvariant(); }
        public bool CanInteract(GameObject actor) { return Indeks >= 0; }

        public void Interact(GameObject actor)
        {
            var inv = GameServices.Get<TikarPandan3DInventory>();
            var dir = GameServices.Get<ForagingDirector>();
            if (inv == null || dir == null || Indeks < 0) return;
            var id = ForagingDirector.Item(Tipe);
            int n = Tipe == ForagingDirector.JenisPungut.Sarang ? 2 : 1;
            int m = inv.Tambah(id, n);
            if (m == 0) { GameEvents.RaiseToast(ItemDatabase.Get(id).displayName + ": tas penuh"); return; }
            GameEvents.RaiseToast("+" + m + " " + ItemDatabase.Get(id).displayName);
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.PlayItemPickup();
            dir.Ambil(Indeks);
        }
    }
}
