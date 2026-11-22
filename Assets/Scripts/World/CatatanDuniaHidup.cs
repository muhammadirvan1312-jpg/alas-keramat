using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.World
{
    // Catatan lapangan & prasasti cerita lingkungan di 12 reruntuhan utama pulau:
    // membaca catatan memberi narasi lokal, memulihkan +6 Sanity & memberi +12 XP KeahlianRakit (sekali per catatan).
    public class CatatanDuniaHidup : MonoBehaviour, IInteractable
    {
        [SerializeField] string judul = "Catatan Lapangan";
        [SerializeField] string isiRingkas = "Tinta memudar oleh lembap hutan.";

        bool _sudahDibaca;

        public string Judul { get { return judul; } }
        public bool SudahDibaca { get { return _sudahDibaca; } }
        public float HoldDuration { get { return 0f; } }

        public void Pasang(string j, string isi)
        {
            judul = string.IsNullOrEmpty(j) ? "Catatan" : j;
            isiRingkas = string.IsNullOrEmpty(isi) ? "Catatan ekspedisi." : isi;
        }

        public static string FormatPrompt(string j, bool dibaca)
        {
            return (dibaca ? "BACA ULANG: " : "BACA CATATAN: ") + j;
        }

        public string GetPromptText(GameObject actor)
        {
            return FormatPrompt(judul, _sudahDibaca);
        }

        public bool CanInteract(GameObject actor) { return true; }

        public void Interact(GameObject actor)
        {
            if (!_sudahDibaca)
            {
                _sudahDibaca = true;
                var vital = GameServices.Get<AndiVitalsAndSanity>();
                if (vital != null) vital.UbahSanity(6f);
                var lvl = GameServices.Get<LevelSurvivalAndi>();
                if (lvl != null) lvl.TambahXP(LevelSurvivalAndi.Atribut.KeahlianRakit, 12f);
            }
            GameEvents.RaiseToast("[" + judul + "] " + isiRingkas);
        }
    }
}
