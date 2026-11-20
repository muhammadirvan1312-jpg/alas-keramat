using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // 06 §4: statistik final tiap archetype (nilai integer GDD). Hantu tidak mati tetapi "diusir".
    public static class StatMusuh
    {
        public struct DataMusuh
        {
            public EnemyArchetype jenis;
            public float hp, jalan, lari, lihatSiang, lihatMalam, dengar, damage, damageBerat;
            public bool hantu;
            public ItemID[] drop; public float[] peluang; public int[] jumlah;
        }

        public const int JumlahInti = 13; // ID 0-12 (06 §4); 13-19 menyusul (17 §5)

        static DataMusuh D(EnemyArchetype j, float hp, float jalan, float lari, float siang, float malam, float dengar, float dmg, float berat, bool hantu,
            ItemID[] drop = null, float[] peluang = null, int[] jumlah = null)
        {
            return new DataMusuh { jenis = j, hp = hp, jalan = jalan, lari = lari, lihatSiang = siang, lihatMalam = malam, dengar = dengar, damage = dmg, damageBerat = berat, hantu = hantu,
                drop = drop ?? new ItemID[0], peluang = peluang ?? new float[0], jumlah = jumlah ?? new int[0] };
        }

        static readonly DataMusuh[] _tabel =
        {
            D(EnemyArchetype.AbdiSekte, 80f, 1.6f, 5.4f, 35f, 18f, 20f, 12f, 16f, false, new[] { ItemID.KainBatik, ItemID.Damar, ItemID.AnakPanah }, new[] { 0.3f, 0.2f, 0.2f }, new[] { 1, 1, 2 }),
            D(EnemyArchetype.AbdiWanita, 60f, 1.8f, 6.2f, 38f, 20f, 22f, 14f, 18f, false, new[] { ItemID.TaliIjuk }, new[] { 0.3f }, new[] { 1 }),
            D(EnemyArchetype.AbdiLumpur, 70f, 1.4f, 5.0f, 20f, 15f, 25f, 9f, 18f, false, new[] { ItemID.Batu }, new[] { 1f }, new[] { 2 }),
            D(EnemyArchetype.AbdiButo, 300f, 1.2f, 3.6f, 30f, 15f, 18f, 30f, 30f, false, new[] { ItemID.Tengkorak, ItemID.LogamBekas }, new[] { 0.4f, 0.3f }, new[] { 1, 1 }),
            D(EnemyArchetype.PemimpinPatroli, 120f, 1.6f, 5.6f, 40f, 20f, 25f, 14f, 14f, false, new[] { ItemID.KainBatik, ItemID.MesiuHitam }, new[] { 1f, 0.3f }, new[] { 1, 1 }),
            D(EnemyArchetype.KiRogo, 400f, 1.4f, 1.4f, 0f, 0f, 0f, 20f, 20f, false),
            D(EnemyArchetype.PocongHutan, 120f, 2.8f, 2.8f, 30f, 30f, 15f, 15f, 15f, true, new[] { ItemID.KainKafan, ItemID.Kemenyan }, new[] { 0.5f, 0.25f }, new[] { 1, 1 }),
            D(EnemyArchetype.PocongGua, 100f, 2.8f, 2.8f, 25f, 25f, 15f, 15f, 15f, true, new[] { ItemID.KainKafan }, new[] { 0.5f }, new[] { 1 }),
            D(EnemyArchetype.Kuntilanak, 220f, 4.0f, 12f, 50f, 50f, 40f, 18f, 18f, true),
            D(EnemyArchetype.Banaspati, 50f, 6.0f, 6.0f, 40f, 40f, 0f, 10f, 10f, true, new[] { ItemID.Arang }, new[] { 1f }, new[] { 2 }),
            D(EnemyArchetype.Genderuwo, 1500f, 2.0f, 9.0f, 0f, 0f, 0f, 35f, 40f, true),
            D(EnemyArchetype.Jenglot, 15f, 5.5f, 5.5f, 15f, 15f, 10f, 4f, 4f, true),
            D(EnemyArchetype.ManusiaAkar, 400f, 1.5f, 3.0f, 25f, 25f, 15f, 12f, 24f, false, new[] { ItemID.Tulang }, new[] { 1f }, new[] { 4 }),
            D(EnemyArchetype.Kuyang, 90f, 5.0f, 7.5f, 35f, 35f, 20f, 14f, 14f, true, new[] { ItemID.PotonganTubuh, ItemID.Kemenyan }, new[] { 0.6f, 0.35f }, new[] { 1, 1 }),
            D(EnemyArchetype.Tuyul, 999f, 4.2f, 6.5f, 24f, 24f, 18f, 0f, 0f, true),
            D(EnemyArchetype.GenderuwoHutan, 280f, 2.4f, 6.8f, 35f, 30f, 25f, 24f, 30f, true, new[] { ItemID.Tulang, ItemID.DagingBusuk }, new[] { 0.7f, 0.5f }, new[] { 2, 1 }),
            D(EnemyArchetype.MutanGembung, 180f, 1.6f, 3.8f, 25f, 20f, 15f, 18f, 24f, false),
            D(EnemyArchetype.MutanJemari, 150f, 2.2f, 5.4f, 28f, 22f, 20f, 16f, 20f, false),
            D(EnemyArchetype.Penyerupa, 60f, 1.8f, 4.5f, 30f, 30f, 15f, 10f, 10f, true),
            D(EnemyArchetype.HantuAir, 110f, 2.4f, 4.8f, 28f, 28f, 18f, 16f, 16f, true, new[] { ItemID.DaunSirih, ItemID.Tulang }, new[] { 0.5f, 0.4f }, new[] { 2, 1 }),
        };

        public static DataMusuh Ambil(EnemyArchetype j)
        {
            int i = (int)j;
            if (i >= 0 && i < _tabel.Length) return _tabel[i];
            Debug.LogWarning("[AK][StatMusuh] archetype " + j + " belum punya statistik (17 §5), memakai AbdiSekte");
            return _tabel[0];
        }

        public static bool Ada(EnemyArchetype j) { int i = (int)j; return i >= 0 && i < _tabel.Length && _tabel[i].jenis == j; }

        public static bool Sekte(EnemyArchetype j) { return j <= EnemyArchetype.PemimpinPatroli; }

        // Jarak pandang efektif (06 §5): malam memakai nilai malam kecuali Andi membawa cahaya (60 m).
        public static float JarakLihat(DataMusuh d, bool malam, bool andiBawaCahaya)
        {
            if (!malam) return d.lihatSiang;
            return andiBawaCahaya ? Mathf.Max(60f, d.lihatSiang) : d.lihatMalam;
        }
    }
}
