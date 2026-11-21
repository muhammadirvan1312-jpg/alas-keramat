using UnityEngine;

namespace AlasKeramat.Core
{
    // 02 §9 & 03 §8: hewan berkerangka quadruped 17 sendi (Pelvis/Spine/Chest/Neck/Head + 4 kaki x Upper/Lower/Foot), mesh loft satu SkinnedMesh.
    public static class ResepHewan
    {
        public enum JenisHewan { Rusa, Kancil, BabiHutan, AyamHutan, Biawak, Harimau, Buaya, Tawon, UlarPiton, UlarKobra, Monyet, Anjing, Kucing }
        public const int Pelvis = 0, Spine = 1, Chest = 2, Neck = 3, Head = 4, LFU = 5, LFL = 6, LFF = 7, RFU = 8, RFL = 9, RFF = 10, LBU = 11, LBL = 12, LBF = 13, RBU = 14, RBL = 15, RBF = 16, Jumlah = 17;
        public static readonly string[] Nama = { "Pelvis", "Spine", "Chest", "Neck", "Head", "LF_Upper", "LF_Lower", "LF_Foot", "RF_Upper", "RF_Lower", "RF_Foot", "LB_Upper", "LB_Lower", "LB_Foot", "RB_Upper", "RB_Lower", "RB_Foot" };
        static readonly int[] Induk = { -1, 0, 1, 2, 3, 2, 5, 6, 2, 8, 9, 0, 11, 12, 0, 14, 15 };

        // Ukuran: panjang badan, tinggi kaki, lebar, panjang leher, kepala, warna badan, warna perut/kaki, warna aksen.
        public struct Ukuran
        {
            public float panjang, kaki, lebar, leher, kepala;
            public string badan, bawah, aksen;
            public float kecepatan, deteksi;
            public bool agresif, pemangsa, beracun, terbang, ular, amfibi;
            public float damage, jangkau, jeda;
        }

        public static Ukuran Ambil(JenisHewan j)
        {
            switch (j)
            {
                case JenisHewan.Rusa: return new Ukuran { panjang = 1.1f, kaki = 0.78f, lebar = 0.3f, leher = 0.45f, kepala = 0.22f, badan = "#8A6440", bawah = "#D8C8A8", aksen = "#3A2A1A", kecepatan = 9f, deteksi = 22f };
                case JenisHewan.Kancil: return new Ukuran { panjang = 0.5f, kaki = 0.32f, lebar = 0.14f, leher = 0.16f, kepala = 0.1f, badan = "#7A5634", bawah = "#E0D4BC", aksen = "#2A1E12", kecepatan = 8f, deteksi = 14f };
                case JenisHewan.BabiHutan: return new Ukuran { panjang = 0.95f, kaki = 0.42f, lebar = 0.36f, leher = 0.12f, kepala = 0.3f, badan = "#3A2E26", bawah = "#4A3E34", aksen = "#E8E0C8", kecepatan = 7f, deteksi = 14f, agresif = true, damage = 12f, jangkau = 1.4f, jeda = 1.5f };
                case JenisHewan.AyamHutan: return new Ukuran { panjang = 0.28f, kaki = 0.2f, lebar = 0.13f, leher = 0.14f, kepala = 0.06f, badan = "#B8461E", bawah = "#2A3A2A", aksen = "#D8C020", kecepatan = 6f, deteksi = 12f };
                case JenisHewan.Harimau: return new Ukuran { panjang = 1.45f, kaki = 0.68f, lebar = 0.42f, leher = 0.28f, kepala = 0.34f, badan = "#D96B1A", bawah = "#F0E4CE", aksen = "#181512", kecepatan = 9.5f, deteksi = 20f, agresif = true, pemangsa = true, damage = 24f, jangkau = 1.9f, jeda = 1.2f };
                case JenisHewan.Buaya: return new Ukuran { panjang = 2.1f, kaki = 0.16f, lebar = 0.46f, leher = 0.22f, kepala = 0.52f, badan = "#3A4A2E", bawah = "#8E8A6A", aksen = "#222B1A", kecepatan = 6.2f, deteksi = 14f, agresif = true, pemangsa = true, amfibi = true, damage = 28f, jangkau = 2.1f, jeda = 1.5f };
                case JenisHewan.Tawon: return new Ukuran { panjang = 0.34f, kaki = 1.45f, lebar = 0.20f, leher = 0.06f, kepala = 0.12f, badan = "#E6B41E", bawah = "#161412", aksen = "#D8DCE0", kecepatan = 8.4f, deteksi = 13f, agresif = true, pemangsa = true, terbang = true, damage = 9f, jangkau = 1.5f, jeda = 0.75f };
                case JenisHewan.UlarPiton: return new Ukuran { panjang = 2.6f, kaki = 0.08f, lebar = 0.20f, leher = 0.45f, kepala = 0.20f, badan = "#5B4B2A", bawah = "#D6CCA8", aksen = "#C69F4B", kecepatan = 5.4f, deteksi = 11f, agresif = true, pemangsa = true, ular = true, damage = 19f, jangkau = 1.8f, jeda = 1.4f };
                case JenisHewan.UlarKobra: return new Ukuran { panjang = 1.75f, kaki = 0.07f, lebar = 0.14f, leher = 0.42f, kepala = 0.16f, badan = "#242420", bawah = "#D8C87A", aksen = "#B83220", kecepatan = 6.2f, deteksi = 12f, agresif = true, pemangsa = true, ular = true, beracun = true, damage = 14f, jangkau = 1.75f, jeda = 1.15f };
                case JenisHewan.Monyet: return new Ukuran { panjang = 0.56f, kaki = 0.40f, lebar = 0.22f, leher = 0.12f, kepala = 0.17f, badan = "#6E5B47", bawah = "#C4AE94", aksen = "#2B241E", kecepatan = 7.8f, deteksi = 13f, agresif = true, damage = 10f, jangkau = 1.4f, jeda = 1.1f };
                case JenisHewan.Anjing: return new Ukuran { panjang = 0.85f, kaki = 0.48f, lebar = 0.25f, leher = 0.22f, kepala = 0.24f, badan = "#8C5630", bawah = "#D4C0A2", aksen = "#241C16", kecepatan = 8.6f, deteksi = 17f, agresif = true, pemangsa = true, damage = 15f, jangkau = 1.6f, jeda = 1.05f };
                case JenisHewan.Kucing: return new Ukuran { panjang = 0.54f, kaki = 0.28f, lebar = 0.17f, leher = 0.12f, kepala = 0.15f, badan = "#9A7B52", bawah = "#E4DCCE", aksen = "#2B241C", kecepatan = 8.2f, deteksi = 11f, agresif = true, damage = 8f, jangkau = 1.3f, jeda = 0.9f };
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
            pos[Head] = pos[Neck] + ((j == JenisHewan.Biawak || j == JenisHewan.Buaya) ? new Vector3(0f, 0f, u.leher) : new Vector3(0f, u.leher * 0.85f, u.leher * 0.5f));
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
            if (u.ular) { BangunUlar(b, j, u, P); return b.Selesai("M_Hewan_" + j, root, t); }
            if (u.terbang) { BangunTawon(b, P); return b.Selesai("M_Hewan_" + j, root, t); }
            float r = u.lebar * 0.5f;
            // Badan: pantat -> dada (elips sedikit tinggi), perut warna bawah sebagai loft kedua lebih kecil di bawah.
            b.Loft(P(Pelvis) + Vector3.back * r * 0.6f, P(Chest) + Vector3.forward * r * 0.5f, new[] { new Vector3(0f, r * 0.7f, r * 0.8f), new Vector3(0.3f, r, r * 1.1f), new Vector3(0.7f, r, r * 1.15f), new Vector3(1f, r * 0.75f, r * 0.85f) }, Spine, 0);
            b.Loft(P(Spine) + Vector3.down * r * 0.55f + Vector3.back * u.panjang * 0.3f, P(Spine) + Vector3.down * r * 0.55f + Vector3.forward * u.panjang * 0.3f, new[] { S(0f, r * 0.6f), S(1f, r * 0.6f) }, Spine, 1);
            if (j == JenisHewan.Harimau || j == JenisHewan.Kucing)
            {
                int belang = j == JenisHewan.Harimau ? 6 : 4;
                for (int st = 0; st < belang; st++)
                {
                    float zOff = Mathf.Lerp(-u.panjang * 0.34f, u.panjang * 0.34f, (st + 0.5f) / belang);
                    b.Kotak(P(Spine) + new Vector3(0f, r * 0.35f, zOff), new Vector3(r * 1.92f, r * 1.15f, 0.045f), Quaternion.identity, Spine, 2);
                }
            }
            if (j == JenisHewan.Buaya)
            {
                for (int sc = 0; sc < 6; sc++)
                {
                    float zOff = Mathf.Lerp(-u.panjang * 0.38f, u.panjang * 0.35f, sc / 5f);
                    b.Kotak(P(Spine) + new Vector3(0f, r * 0.88f, zOff), new Vector3(r * 0.55f, 0.07f, 0.14f), Quaternion.identity, Spine, 2);
                }
            }
            b.Loft(P(Neck), P(Head), new[] { S(0f, r * 0.55f), S(1f, r * 0.4f) }, Neck, 0);
            Vector3 arahKepala = (P(Head) - P(Neck)).normalized;
            Vector3 moncong = (j == JenisHewan.AyamHutan || j == JenisHewan.Biawak || j == JenisHewan.Buaya) ? Vector3.forward : (Vector3.forward * 0.8f + Vector3.down * 0.4f).normalized;
            b.Loft(P(Head) - arahKepala * 0.02f, P(Head) + moncong * u.kepala, new[] { S(0f, r * 0.5f), S(0.6f, r * 0.38f), S(1f, r * (j == JenisHewan.Buaya ? 0.32f : 0.2f)) }, Head, j == JenisHewan.AyamHutan ? 2 : 0);
            for (int s = 0; s < 2; s++)
            {
                float sx = s == 0 ? -1f : 1f;
                b.Bola(P(Head) + new Vector3(sx * r * 0.42f, r * 0.15f, u.kepala * 0.3f), Vector3.one * Mathf.Max(0.008f, r * 0.09f), Head, 3, 6, 4);
                if (j == JenisHewan.Rusa) b.Kerucut(P(Head) + new Vector3(sx * 0.05f, 0.08f, 0f), P(Head) + new Vector3(sx * 0.18f, 0.42f, -0.05f), 0.02f, Head, 2, 5);
                if (j == JenisHewan.BabiHutan || j == JenisHewan.Harimau) b.Kerucut(P(Head) + new Vector3(sx * 0.05f, -0.06f, u.kepala * 0.8f), P(Head) + new Vector3(sx * 0.08f, 0.02f, u.kepala * 0.95f), 0.012f, Head, j == JenisHewan.Harimau ? 1 : 2, 5);
                if (j == JenisHewan.Harimau || j == JenisHewan.Kucing || j == JenisHewan.Anjing) b.Kerucut(P(Head) + new Vector3(sx * r * 0.36f, r * 0.38f, -0.02f), P(Head) + new Vector3(sx * r * 0.48f, r * 0.72f, 0.02f), r * 0.14f, Head, j == JenisHewan.Anjing ? 0 : 2, 5);
                if (j == JenisHewan.Monyet) b.Bola(P(Head) + new Vector3(sx * r * 0.56f, r * 0.1f, 0f), new Vector3(0.03f, r * 0.22f, r * 0.2f), Head, 1, 5, 4);
            }
            for (int k = 0; k < 4; k++)
            {
                int bb = LFU + k * 3;
                bool sayap = j == JenisHewan.AyamHutan && k < 2;
                if (sayap) { b.Bola(P(bb) + Vector3.down * r * 0.3f, new Vector3(0.02f, r * 0.6f, u.panjang * 0.45f), bb, 2, 8, 6); continue; }
                float rk = Mathf.Max(0.012f, r * (j == JenisHewan.BabiHutan || j == JenisHewan.Harimau ? 0.32f : 0.22f));
                b.Loft(P(bb), P(bb + 1), new[] { S(0f, rk * 1.3f), S(1f, rk) }, bb, 0);
                b.Loft(P(bb + 1), P(bb + 2), new[] { S(0f, rk), S(1f, rk * 0.7f) }, bb + 1, 1);
                b.Kotak(P(bb + 2) + Vector3.forward * rk * 0.4f, new Vector3(rk * 1.6f, 0.04f, rk * 2.2f), Quaternion.identity, bb + 2, 3);
            }
            if (j == JenisHewan.Biawak || j == JenisHewan.AyamHutan || j == JenisHewan.Buaya)
                b.Kerucut(P(Pelvis) + Vector3.back * r * 0.5f, P(Pelvis) + (j == JenisHewan.AyamHutan ? new Vector3(0f, 0.18f, -0.15f) : Vector3.back * (j == JenisHewan.Buaya ? u.panjang * 0.9f : u.panjang)), r * (j == JenisHewan.AyamHutan ? 0.8f : 0.62f), Pelvis, j == JenisHewan.AyamHutan ? 2 : 0, 6);
            else if (j == JenisHewan.Harimau || j == JenisHewan.Monyet || j == JenisHewan.Kucing || j == JenisHewan.Anjing)
                b.Kerucut(P(Pelvis) + Vector3.back * r * 0.5f, P(Pelvis) + new Vector3(0f, (j == JenisHewan.Anjing || j == JenisHewan.Kucing) ? 0.24f : -0.08f, -r * 0.6f - (j == JenisHewan.Monyet || j == JenisHewan.Harimau ? 0.55f : 0.3f)), r * 0.24f, Pelvis, 2, 6);
            else b.Kerucut(P(Pelvis) + Vector3.back * r * 0.6f, P(Pelvis) + new Vector3(0f, -0.08f, -r * 0.6f - 0.12f), 0.025f, Pelvis, 0, 5);
            return b.Selesai("M_Hewan_" + j, root, t);
        }

        static void BangunUlar(CharacterMeshBuilder b, JenisHewan j, Ukuran u, System.Func<int, Vector3> P)
        {
            float r = u.lebar * 0.5f;
            b.Kerucut(P(Pelvis), P(Pelvis) + new Vector3(0.08f, -0.01f, -u.panjang * 0.45f), r * 0.72f, Pelvis, 0, 8);
            b.Loft(P(Pelvis), P(Spine), new[] { S(0f, r * 0.8f), S(1f, r * 1.0f) }, Pelvis, 0);
            b.Loft(P(Spine), P(Chest), new[] { S(0f, r * 1.0f), S(1f, r * 0.92f) }, Spine, 0);
            b.Loft(P(Spine) + Vector3.down * r * 0.45f, P(Chest) + Vector3.down * r * 0.45f, new[] { S(0f, r * 0.55f), S(1f, r * 0.5f) }, Spine, 1);
            b.Loft(P(Chest), P(Neck), new[] { S(0f, r * 0.9f), S(1f, r * 0.78f) }, Chest, 0);
            b.Loft(P(Neck), P(Head), new[] { S(0f, r * 0.78f), S(1f, r * 0.66f) }, Neck, 0);
            if (j == JenisHewan.UlarKobra)
            {
                b.Kotak(P(Neck) + Vector3.down * 0.04f, new Vector3(r * 3.6f, r * 2.2f, 0.045f), Quaternion.Euler(20f, 0f, 0f), Neck, 0);
                b.Kotak(P(Neck) + new Vector3(0f, -0.02f, -0.026f), new Vector3(r * 2.1f, r * 1.3f, 0.015f), Quaternion.Euler(20f, 0f, 0f), Neck, 1);
                b.Kerucut(P(Head) + Vector3.forward * u.kepala * 0.9f, P(Head) + Vector3.forward * (u.kepala * 1.45f), 0.01f, Head, 2, 4);
            }
            else
            {
                for (int k = 0; k < 5; k++)
                {
                    Vector3 p = Vector3.Lerp(P(Pelvis), P(Chest), k / 4f) + Vector3.up * (r * 0.65f);
                    b.Bola(p, new Vector3(r * 1.3f, r * 0.6f, 0.14f), Spine, 2, 6, 4);
                }
            }
            b.Bola(P(Head) + Vector3.forward * (u.kepala * 0.45f), new Vector3(r * 1.3f, r * 0.85f, u.kepala * 1.2f), Head, 0, 8, 6);
            b.Bola(P(Head) + new Vector3(-r * 0.45f, r * 0.25f, u.kepala * 0.55f), Vector3.one * 0.018f, Head, 3, 5, 4);
            b.Bola(P(Head) + new Vector3(r * 0.45f, r * 0.25f, u.kepala * 0.55f), Vector3.one * 0.018f, Head, 3, 5, 4);
        }

        static void BangunTawon(CharacterMeshBuilder b, System.Func<int, Vector3> P)
        {
            Vector3 c = P(Spine);
            Vector3[] offs = { Vector3.zero, new Vector3(-0.22f, 0.14f, 0.18f), new Vector3(0.24f, 0.10f, -0.15f), new Vector3(-0.16f, -0.12f, -0.22f), new Vector3(0.18f, 0.22f, 0.16f) };
            for (int i = 0; i < offs.Length; i++)
            {
                Vector3 q = c + offs[i];
                int bone = i == 0 ? Spine : i == 1 ? Chest : i == 2 ? Neck : i == 3 ? Pelvis : Head;
                b.Bola(q, new Vector3(0.06f, 0.06f, 0.11f), bone, 0, 6, 5);
                b.Kotak(q + Vector3.back * 0.02f, new Vector3(0.064f, 0.064f, 0.028f), Quaternion.identity, bone, 1);
                b.Kotak(q + new Vector3(-0.07f, 0.03f, 0f), new Vector3(0.12f, 0.008f, 0.05f), Quaternion.Euler(0f, 10f, 18f), LFU, 2);
                b.Kotak(q + new Vector3(0.07f, 0.03f, 0f), new Vector3(0.12f, 0.008f, 0.05f), Quaternion.Euler(0f, -10f, -18f), RFU, 2);
            }
        }
    }
}
