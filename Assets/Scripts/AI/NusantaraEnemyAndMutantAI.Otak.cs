using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // 06 §5-6: rumus persepsi & keputusan Sekte sebagai fungsi murni (diuji NusantaraSelfTest tanpa scene).
    public partial class NusantaraEnemyAndMutantAI
    {
        public const float KerucutPandang = 110f, LajuSadarDasar = 40f, TurunSadar = 8f, JarakAcuanSadar = 10f;
        public const float AmbangPenasaran = 30f, AmbangWaspada = 60f, AmbangTerlibat = 100f;
        public const float FaktorJongkok = 0.6f, FaktorSemak = 0.35f, FaktorMalam = 0.5f, FaktorCahaya = 1.5f, FaktorBadai = 0.7f;
        public const float TakutLari = 70f, HpLari = 0.3f, PeluangLari = 0.6f, HpTerluka = 0.15f, PeluangTerluka = 0.5f;
        public const float HpMenyerah = 0.25f, TakutMenyerah = 60f, RadiusTemanMenyerah = 15f, PeluangGertak = 0.4f, RasioGertak = 0.6f;
        public const float TurunTakut = 5f, AmbangSerangAgresi = 40f, AmbangGertakAgresi = 60f;
        public const float TakutTemanMati = 15f, TakutParry = 20f, TakutApi = 10f;

        // Faktor terlihat 06 §5 (0 = tidak terlihat).
        public static float FaktorTerlihat(bool dalamKerucut, bool terhalang, bool jongkok, bool diSemak, bool malam, bool bawaCahaya, bool badai)
        {
            if (!dalamKerucut || terhalang) return 0f;
            float f = 1f;
            if (jongkok) f *= diSemak ? FaktorSemak : FaktorJongkok;
            if (malam) f *= bawaCahaya ? FaktorCahaya : FaktorMalam;
            if (badai) f *= FaktorBadai;
            return f;
        }

        // Laju naik kesadaran per detik: faktor x (1 / max(1, jarak/10)) x 40.
        public static float LajuSadar(float faktor, float jarak) { return faktor * (1f / Mathf.Max(1f, jarak / JarakAcuanSadar)) * LajuSadarDasar; }

        public static float LangkahKesadaran(float sadar, float faktor, float jarak, float jarakLihat, float dt)
        {
            if (faktor > 0f && jarak <= jarakLihat) return Mathf.Min(AmbangTerlibat, sadar + LajuSadar(faktor, jarak) * dt);
            return Mathf.Max(0f, sadar - TurunSadar * dt);
        }

        public static bool DalamKerucut(Vector3 maju, Vector3 keTarget) { keTarget.y = 0f; maju.y = 0f; return keTarget.sqrMagnitude < 0.01f || Vector3.Angle(maju, keTarget) <= KerucutPandang * 0.5f; }

        public static bool HarusMenyerah(float hpRasio, float takut, bool adaTemanDekat) { return hpRasio < HpMenyerah && takut > TakutMenyerah && !adaTemanDekat; }

        // acak 0..1 dari pemanggil (deterministik di self-test).
        public static bool HarusLari(float takut, float hpRasio, float acak) { return takut >= TakutLari || (hpRasio < HpLari && acak < PeluangLari); }

        public static bool HarusTerluka(float hpRasio, float acak) { return hpRasio < HpTerluka && acak < PeluangTerluka; }

        public static bool PilihGertak(float takut, float agresi, float acak) { return takut > agresi * RasioGertak && acak < PeluangGertak; }

        // Saat kesadaran penuh: Isyarat (pemimpin), Mengintai (agresi rendah & belum diserang), Gertak, atau Serang.
        public static KeadaanSekte PutuskanTerlibat(EnemyArchetype jenis, bool sudahIsyarat, bool diserang, float agresi, float takut, float acak)
        {
            if (jenis == EnemyArchetype.PemimpinPatroli && !sudahIsyarat) return KeadaanSekte.Isyarat;
            if (!diserang && agresi < AmbangSerangAgresi) return agresi > AmbangGertakAgresi * 0.5f && PilihGertak(takut, agresi, acak) ? KeadaanSekte.Gertak : KeadaanSekte.Mengintai;
            return PilihGertak(takut, agresi, acak) ? KeadaanSekte.Gertak : KeadaanSekte.Serang;
        }

        // Reaksi kena (06 §3): durasi goyah. Ringan 0,25; berat 0,7; Abdi/Wanita HP < 40% kena berat = jatuh 1,2.
        public static float LamaReaksi(EnemyArchetype jenis, bool berat, float hpRasio)
        {
            if (!berat) return 0.25f;
            if ((jenis == EnemyArchetype.AbdiSekte || jenis == EnemyArchetype.AbdiWanita) && hpRasio < 0.4f) return 1.2f;
            return 0.7f;
        }
    }
}
