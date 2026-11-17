using UnityEngine;

namespace AlasKeramat.Core
{
    // 02 §9 & 03 §8: hewan berkerangka quadruped 17 sendi (Pelvis/Spine/Chest/Neck/Head + 4 kaki x Upper/Lower/Foot), mesh loft satu SkinnedMesh.
    public static class ResepHewan
    {
        public enum JenisHewan { Rusa, Kancil, BabiHutan, AyamHutan, Biawak }
        public const int Pelvis = 0, Spine = 1, Chest = 2, Neck = 3, Head = 4, LFU = 5, LFL = 6, LFF = 7, RFU = 8, RFL = 9, RFF = 10, LBU = 11, LBL = 12, LBF = 13, RBU = 14, RBL = 15, RBF = 16, Jumlah = 17;
        public static readonly string[] Nama = { "Pelvis", "Spine", "Chest", "Neck", "Head", "LF_Upper", "LF_Lower", "LF_Foot", "RF_Upper", "RF_Lower", "RF_Foot", "LB_Upper", "LB_Lower", "LB_Foot", "RB_Upper", "RB_Lower", "RB_Foot" };
        static readonly int[] Induk = { -1, 0, 1, 2, 3, 2, 5, 6, 2, 8, 9, 0, 11, 12, 0, 14, 15 };

        // Ukuran: panjang badan, tinggi kaki, lebar, panjang leher, kepala, warna badan, warna perut/kaki, warna aksen.
        public struct Ukuran { public float panjang, kaki, lebar, leher, kepala; public string badan, bawah, aksen; public float kecepatan, deteksi; public bool agresif; }

        public static Ukuran Ambil(JenisHewan j)
        {
            switch (j)
            {
                case JenisHewan.Rusa: return new Ukuran { panjang = 1.1f, kaki = 0.78f, lebar = 0.3f, leher = 0.45f, kepala = 0.22f, badan = "#8A6440", bawah = "#D8C8A8", aksen = "#3A2A1A", kecepatan = 9f, deteksi = 22f };
                case JenisHewan.Kancil: return new Ukuran { panjang = 0.5f, kaki = 0.32f, lebar = 0.14f, leher = 0.16f, kepala = 0.1f, badan = "#7A5634", bawah = "#E0D4BC", aksen = "#2A1E12", kecepatan = 8f, deteksi = 14f };
                case JenisHewan.BabiHutan: return new Ukuran { panjang = 0.95f, kaki = 0.42f, lebar = 0.36f, leher = 0.12f, kepala = 0.3f, badan = "#3A2E26", bawah = "#4A3E34", aksen = "#E8E0C8", kecepatan = 7f, deteksi = 14f, agresif = true };
                case JenisHewan.AyamHutan: return new Ukuran { panjang = 0.28f, kaki = 0.2f, lebar = 0.13f, leher = 0.14f, kepala = 0.06f, badan = "#B8461E", bawah = "#2A3A2A", aksen = "#D8C020", kecepatan = 6f, deteksi = 12f };
                default: return new Ukuran { panjang = 1.1f, kaki = 0.14f, lebar = 0.2f, leher = 0.12f, kepala = 0.1f, badan = "#4A4A36", bawah = "#6A6A4A", aksen = "#2A2A1E", kecepatan = 4f, deteksi = 8f };
            }
        }

        public static Color[] Warna(JenisHewan j)
        {
            var u = Ambil(j);
            return new[] { ProcTex.Hex(u.badan), ProcTex.Hex(u.bawah), ProcTex.Hex(u.aksen), ProcTex.Hex("#101010") };
        }

        public static Transform[] BuatKerangka(Transform root, JenisHewan j)
        {
            var u = Ambil(j);
            float h = u.kaki, L = u.panjang * 0.5f, w = u.lebar * 0.45f;
            var pos = new Vector3[Jumlah];
            pos[Pelvis] = new Vector3(0f, h, -L); pos[Spine] = new Vector3(0f, h + 0.02f, 0f); pos[Chest] = new Vector3(0f, h + 0.03f, L);
            pos[Neck] = pos[Chest] + new Vector3(0f, 0.05f, 0.05f);
            pos[Head] = pos[Neck] + (j == JenisHewan.Biawak ? new Vector3(0f, 0f, u.leher) : new Vector3(0f, u.leher * 0.85f, u.leher * 0.5f));
            for (int k = 0; k < 4; k++)
            {
                bool depan = k < 2; float sx = (k % 2 == 0) ? -w : w;
                float z = depan ? L * 0.85f : -L * 0.85f;
                int b = LFU + k * 3;
                pos[b] = new Vector3(sx, h, z); pos[b + 1] = new Vector3(sx, h * 0.5f, z + (depan ? 0.02f : -0.03f)); pos[b + 2] = new Vector3(sx, 0.03f, z);
            }
            var t = new Transform[Jumlah];
            for (int i = 0; i < Jumlah; i++)
            {
                t[i] = new GameObject(Nama[i]).transform;
                t[i].SetParent(Induk[i] < 0 ? root : t[Induk[i]], false);
                t[i].position = root.TransformPoint(pos[i]);
            }
            return t;
        }

        static Vector3 S(float p, float r) { return new Vector3(p, r, r); }

        public static Mesh BangunMesh(JenisHewan j, Transform root, Transform[] t)
        {
            var u = Ambil(j);
            var b = new CharacterMeshBuilder();
            System.Func<int, Vector3> P = i => root.InverseTransformPoint(t[i].position);
            float r = u.lebar * 0.5f;
            // Badan: pantat -> dada (elips sedikit tinggi), perut warna bawah sebagai loft kedua lebih kecil di bawah.
            b.Loft(P(Pelvis) + Vector3.back * r * 0.6f, P(Chest) + Vector3.forward * r * 0.5f, new[] { new Vector3(0f, r * 0.7f, r * 0.8f), new Vector3(0.3f, r, r * 1.1f), new Vector3(0.7f, r, r * 1.15f), new Vector3(1f, r * 0.75f, r * 0.85f) }, Spine, 0);
            b.Loft(P(Spine) + Vector3.down * r * 0.55f + Vector3.back * u.panjang * 0.3f, P(Spine) + Vector3.down * r * 0.55f + Vector3.forward * u.panjang * 0.3f, new[] { S(0f, r * 0.6f), S(1f, r * 0.6f) }, Spine, 1);
            b.Loft(P(Neck), P(Head), new[] { S(0f, r * 0.55f), S(1f, r * 0.4f) }, Neck, 0);
            Vector3 arahKepala = (P(Head) - P(Neck)).normalized;
            Vector3 moncong = j == JenisHewan.AyamHutan || j == JenisHewan.Biawak ? Vector3.forward : (Vector3.forward * 0.8f + Vector3.down * 0.4f).normalized;
            b.Loft(P(Head) - arahKepala * 0.02f, P(Head) + moncong * u.kepala, new[] { S(0f, r * 0.5f), S(0.6f, r * 0.38f), S(1f, r * 0.2f) }, Head, j == JenisHewan.AyamHutan ? 2 : 0);
            for (int s = 0; s < 2; s++)
            {
                float sx = s == 0 ? -1f : 1f;
                b.Bola(P(Head) + new Vector3(sx * r * 0.42f, r * 0.15f, u.kepala * 0.3f), Vector3.one * Mathf.Max(0.008f, r * 0.09f), Head, 3, 6, 4);
                if (j == JenisHewan.Rusa) b.Kerucut(P(Head) + new Vector3(sx * 0.05f, 0.08f, 0f), P(Head) + new Vector3(sx * 0.18f, 0.42f, -0.05f), 0.02f, Head, 2, 5);
                if (j == JenisHewan.BabiHutan) b.Kerucut(P(Head) + new Vector3(sx * 0.05f, -0.06f, u.kepala * 0.8f), P(Head) + new Vector3(sx * 0.08f, 0.02f, u.kepala * 0.95f), 0.012f, Head, 2, 5);
            }
            for (int k = 0; k < 4; k++)
            {
                int bb = LFU + k * 3;
                bool sayap = j == JenisHewan.AyamHutan && k < 2;
                if (sayap) { b.Bola(P(bb) + Vector3.down * r * 0.3f, new Vector3(0.02f, r * 0.6f, u.panjang * 0.45f), bb, 2, 8, 6); continue; }
                float rk = Mathf.Max(0.012f, r * (j == JenisHewan.BabiHutan ? 0.32f : 0.22f));
                b.Loft(P(bb), P(bb + 1), new[] { S(0f, rk * 1.3f), S(1f, rk) }, bb, 0);
                b.Loft(P(bb + 1), P(bb + 2), new[] { S(0f, rk), S(1f, rk * 0.7f) }, bb + 1, 1);
                b.Kotak(P(bb + 2) + Vector3.forward * rk * 0.4f, new Vector3(rk * 1.6f, 0.04f, rk * 2.2f), Quaternion.identity, bb + 2, 3);
            }
            if (j == JenisHewan.Biawak || j == JenisHewan.AyamHutan)
                b.Kerucut(P(Pelvis) + Vector3.back * r * 0.5f, P(Pelvis) + (j == JenisHewan.Biawak ? Vector3.back * u.panjang : new Vector3(0f, 0.18f, -0.15f)), r * (j == JenisHewan.Biawak ? 0.6f : 0.8f), Pelvis, j == JenisHewan.AyamHutan ? 2 : 0, 6);
            else b.Kerucut(P(Pelvis) + Vector3.back * r * 0.6f, P(Pelvis) + new Vector3(0f, -0.08f, -r * 0.6f - 0.12f), 0.025f, Pelvis, 0, 5);
            return b.Selesai("M_Hewan_" + j, root, t);
        }
    }
}
