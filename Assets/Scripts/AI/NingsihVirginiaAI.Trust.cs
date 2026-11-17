using UnityEngine;

namespace AlasKeramat.AI
{
    // 08 §3.2-3.3: Trust 0-100 dalam tiga tahap + tabel perubahan (fungsi murni untuk self-test).
    public partial class NingsihVirginiaAI
    {
        public enum KejadianTrust { BunuhSekteDiDepan, TidurDiSaung, DipukulAndi, SerangYangMenyerah, TakBertemuSehari, LoreDelapanDiApi, BeriMakanan, TerimaHadiah, WasiatAyah }
        public const float BatasTahap2 = 31f, BatasTahap3 = 70f, BatasPistol = 90f;
        public const float NaikTahap1 = 2f, MaksPerjumpaan1 = 15f, NaikTahap2 = 8f, MaksPerjumpaan2 = 20f, TurunLari = 5f, TurunBidik = 15f;

        public static int Tahap(float trust) { return trust >= BatasTahap3 ? 3 : trust >= BatasTahap2 ? 2 : 1; }

        public static float Delta(KejadianTrust k)
        {
            switch (k)
            {
                case KejadianTrust.BunuhSekteDiDepan: return 5f;
                case KejadianTrust.TidurDiSaung: return 3f;
                case KejadianTrust.DipukulAndi: return -40f;
                case KejadianTrust.SerangYangMenyerah: return -5f;
                case KejadianTrust.TakBertemuSehari: return -2f;
                case KejadianTrust.LoreDelapanDiApi: return 10f;
                case KejadianTrust.BeriMakanan: return 10f;
                case KejadianTrust.TerimaHadiah: return 2f;
                case KejadianTrust.WasiatAyah: return 10f;
            }
            return 0f;
        }

        public static float Terapkan(float trust, KejadianTrust k) { return Mathf.Clamp(trust + Delta(k), 0f, 100f); }

        // Kenaikan bertahap per perjumpaan dengan batas (tahap 1: +2/s maks +15; tahap 2: +8/s maks +20).
        public static float NaikPerjumpaan(float trust, float sudahDidapat, float perDetik, float maks, float dt, out float didapatBaru)
        {
            float tambah = Mathf.Min(perDetik * dt, Mathf.Max(0f, maks - sudahDidapat));
            didapatBaru = sudahDidapat + tambah;
            return Mathf.Clamp(trust + tambah, 0f, 100f);
        }

        // Syarat sikap tenang Andi: senjata disimpan, jongkok, diam, menatap ke arahnya.
        public static bool AndiTenang(bool senjataDisimpan, bool jongkok, float laju, float sudutTatap) { return senjataDisimpan && jongkok && laju < 0.3f && sudutTatap < 25f; }
    }
}
