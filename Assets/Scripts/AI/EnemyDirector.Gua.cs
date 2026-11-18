using AlasKeramat.Core;
using UnityEngine;
namespace AlasKeramat.AI
{
    // T10: pemunculan di titik persis (gua bawah tanah, siege), Malam Darah tambahan sesudah Paku 1/2 (04 §5), abu massal (04 §7.6).
    public partial class EnemyDirector
    {
        public static int MalamDarahPaksa = -1;
        public static bool TanpaMalamDarah; // Ending B: sandbox tanpa Malam Darah & hantu (04 §7.9)

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetGua() { MalamDarahPaksa = -1; TanpaMalamDarah = false; }

        // Malam berikutnya setelah sekarang (dipakai saat Paku 1/2 didapat).
        public static int MalamBerikutnya(int hari, float jam) { return JamMalam(jam) ? MalamKe(hari, jam) + 1 : hari; }

        // Sekte di titik persis, langsung memburu tujuan (raid/siege/alarm). Mengabaikan batas aktif bila paksa.
        public NusantaraEnemyAndMutantAI SpawnDi(EnemyArchetype j, Vector3 pos, Vector3 tujuan, bool pemimpin)
        {
            var ai = SpawnLangsung(j, pos);
            if (ai == null) return null;
            ai.MulaiSerbu(tujuan, pemimpin, false);
            _raid.Add(ai);
            return ai;
        }

        // Hantu di titik persis tanpa sampel tinggi permukaan (gua). idx: 0 Pocong, 1 Kuntilanak, 2 Banaspati.
        public HantuAI PanggilHantuDi(int idx, Vector3 pos) { return BuatHantu(idx, pos); }

        // Semua musuh & hantu dalam radius larut jadi abu (damage Gaib).
        public int AbukanSemua(Vector3 pusat, float radius)
        {
            int n = 0;
            var info = new DamageInfo { amount = 99999f, type = DamageType.Gaib, direction = Vector3.up, source = gameObject };
            for (int i = NusantaraEnemyAndMutantAI.Semua.Count - 1; i >= 0; i--)
            {
                var a = NusantaraEnemyAndMutantAI.Semua[i];
                if (a == null || !a.IsAlive || (a.transform.position - pusat).sqrMagnitude > radius * radius) continue;
                info.point = a.transform.position; a.TakeDamage(info); n++;
            }
            for (int i = HantuAI.Semua.Count - 1; i >= 0; i--)
            {
                var h = HantuAI.Semua[i];
                if (h == null || !h.IsAlive || (h.transform.position - pusat).sqrMagnitude > radius * radius) continue;
                info.point = h.transform.position; h.TakeDamage(info); n++;
            }
            return n;
        }
    }
}
