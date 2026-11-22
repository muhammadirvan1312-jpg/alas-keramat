using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Titik ambil flora/material tambahan (pool FloraSegarDirector).
    // WAJIB di file sendiri: komponen yang dipasang ke scene harus bernama sama dengan file-nya. Bila tidak, Unity tidak
    // membuat MonoScript, datanya tetap tersimpan di scene, dan player Android membaca level0 "corrupted / Position out of
    // bounds" lalu crash setelah logo Unity (terbukti di Samsung A16 & redroid ARM64).
    public class ForagingExtraNode : MonoBehaviour, IInteractable
    {
        public int Indeks = -1;
        public FloraSegarDirector.JenisFlora Tipe;
        public float HoldDuration { get { return 0f; } }
        public string GetPromptText(GameObject actor) { return "AMBIL " + ItemDatabase.Get(FloraSegarDirector.Item(Tipe)).displayName.ToUpperInvariant(); }
        public bool CanInteract(GameObject actor) { return Indeks >= 0; }

        public void Interact(GameObject actor)
        {
            var inv = GameServices.Get<TikarPandan3DInventory>();
            var dir = GameServices.Get<FloraSegarDirector>();
            if (inv == null || dir == null || Indeks < 0) return;
            var id = FloraSegarDirector.Item(Tipe);
            int qty = (Tipe == FloraSegarDirector.JenisFlora.Pisang || Tipe == FloraSegarDirector.JenisFlora.Ranting) ? 2 : 1;
            int m = inv.Tambah(id, qty);
            if (m == 0) { GameEvents.RaiseToast(ItemDatabase.Get(id).displayName + ": tas penuh"); return; }
            GameEvents.RaiseToast("+" + m + " " + ItemDatabase.Get(id).displayName);
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.PlayItemPickup();
            dir.Ambil(Indeks);
        }
    }
}
