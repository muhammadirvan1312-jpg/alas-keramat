using AlasKeramat.Core;

namespace AlasKeramat.Player
{
    // Kontrak vital Andi yang dipanggil interaksi (minum di sungai, makan item). Diimplementasikan AndiVitalsAndSanity (T3b/GPT TUGAS 03).
    public interface IVitalAndi
    {
        void Minum(float hidrasi, bool airMentah);
        bool Konsumsi(ItemID id);
    }
}
