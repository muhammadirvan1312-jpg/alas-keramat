using UnityEngine;

namespace AlasKeramat.Core
{
    // Resep 12 humanoid (02 §2-7): proporsi, warna atlas, gaya rambut, aksesori khas. Hewan menyusul di T5.
    public static class ResepKarakter
    {
        public enum JenisKarakter { Andi, Deden, Ningsih, KiRogo, AbdiSekte, AbdiWanita, AbdiLumpur, AbdiButo, PemimpinPatroli, Pocong, Kuntilanak, Genderuwo }
        public enum Bagian { Semua, BadanSaja, KepalaLengan }
        public const int Kulit = 0, Atasan = 1, LenganAtas = 2, LenganBawah = 3, Celana = 4, Sepatu = 5, Rambut = 6, PutihMata = 7, Iris = 8, Bibir = 9, Aks1 = 10, Aks2 = 11, Gelap = 12, Aks3 = 13;

        public struct DataKarakter { public float tinggi, lebar, perut, dada, lengan; public string[] warna; }

        public static DataKarakter Ambil(JenisKarakter j)
        {
            switch (j)
            {
                case JenisKarakter.Andi: return D(1.75f, 1f, 0.92f, 1.05f, 1f, "#B98560", "#556B3A", "#556B3A", "#B98560", "#5E5442", "#3E3023", "#151210", "#EDE8E0", "#3B2416", "#8E5A4A", "#1A1A1A", "#9E9E9E", "#2A1A12", "#4A4033");
                case JenisKarakter.Deden: return D(1.68f, 1.12f, 1.15f, 1.08f, 1.1f, "#7E5236", "#C8641E", "#7A3A2A", "#7E5236", "#4A4A3A", "#D8B12A", "#3A3530", "#E8E2D8", "#2A1A10", "#6E4636", "#1A1410", "#E8E0D0", "#5A2A1A", "#D8D8C8");
                case JenisKarakter.Ningsih: return D(1.58f, 0.88f, 0.9f, 0.95f, 0.88f, "#D2A584", "#E6DFCF", "#E6DFCF", "#D2A584", "#5E3F22", "#3A2A1C", "#0E0C0B", "#F2EEE6", "#2A1810", "#A0625A", "#F4F1E0", "#6FE0C0", "#2A1A12", "#8A6A3A");
                case JenisKarakter.KiRogo: return D(1.70f, 0.92f, 0.9f, 0.95f, 0.9f, "#7E5E46", "#141414", "#141414", "#7E5E46", "#3A2A1A", "#2A2018", "#BFB8B0", "#D8D0C0", "#5A5048", "#5E3E36", "#2A1F14", "#E8DCC0", "#1A1210", "#7A1A10");
                case JenisKarakter.AbdiSekte: return D(1.72f, 1f, 1f, 1f, 1f, "#8A6448", "#B8AE96", "#B8AE96", "#8A6448", "#9E9480", "#4A3A2A", "#1A1612", "#E0DAD0", "#2A1A10", "#6E4A3A", "#6B4A2E", "#8A7A5A", "#2A1A12", "#5A4A36");
                case JenisKarakter.AbdiWanita: return D(1.60f, 0.9f, 0.95f, 0.95f, 0.9f, "#9A7254", "#A89E86", "#A89E86", "#9A7254", "#8E8470", "#3A2E22", "#120E0C", "#E0DAD0", "#2A1A10", "#7A4A3A", "#5A3E26", "#7A6A4A", "#2A1A12", "#5A4A36");
                case JenisKarakter.AbdiLumpur: return D(1.74f, 1.05f, 1.05f, 1f, 1.05f, "#5A4634", "#4A3E30", "#4A3E30", "#5A4634", "#3E342A", "#2E261E", "#1A1612", "#C8C0B0", "#2A1A10", "#4A3A2E", "#3E2E1E", "#6A5A40", "#1A120C", "#3A3024");
                case JenisKarakter.AbdiButo: return D(2.05f, 1.35f, 1.25f, 1.25f, 1.3f, "#7A5640", "#8E8470", "#8E8470", "#7A5640", "#5E5444", "#3A2E22", "#1A1612", "#D8D0C0", "#5A1A10", "#5E3A2E", "#4A3420", "#6E2A1A", "#2A1A12", "#4A3A2A");
                case JenisKarakter.PemimpinPatroli: return D(1.78f, 1.02f, 1f, 1.05f, 1.02f, "#866046", "#5A2A20", "#5A2A20", "#866046", "#2A2420", "#2A2018", "#1A1612", "#E0DAD0", "#2A1A10", "#6E4A3A", "#7A5A2E", "#C8A050", "#2A1A12", "#3A1A12");
                case JenisKarakter.Pocong: return D(1.72f, 1f, 1.05f, 1f, 1f, "#C8C2B4", "#E2DCCC", "#E2DCCC", "#E2DCCC", "#E2DCCC", "#E2DCCC", "#E2DCCC", "#D0D0C8", "#D0D0C8", "#3A3A30", "#C8C0A8", "#B8B098", "#24221C", "#8A8070");
                case JenisKarakter.Kuntilanak: return D(1.66f, 0.85f, 0.88f, 0.9f, 0.85f, "#C8C4B6", "#DCD6C8", "#DCD6C8", "#C8C4B6", "#DCD6C8", "#C8C4B6", "#0A0908", "#E8E8E0", "#E8E8E0", "#5A3A3A", "#8A1A1A", "#B8B0A0", "#1A1010", "#6A5A50");
                default: return D(2.3f, 1.45f, 1.3f, 1.35f, 1.4f, "#2A2018", "#2A2018", "#2A2018", "#2A2018", "#2A2018", "#1E1812", "#1A140E", "#3A1A10", "#D02818", "#3A2018", "#3A2A1E", "#4A3424", "#0E0A08", "#5A4030");
            }
        }

        static DataKarakter D(float t, float l, float p, float d, float lg, params string[] w) { return new DataKarakter { tinggi = t, lebar = l, perut = p, dada = d, lengan = lg, warna = w }; }

        public static Color[] WarnaAtlas(JenisKarakter j)
        {
            var w = Ambil(j).warna;
            var c = new Color[w.Length];
            for (int i = 0; i < w.Length; i++) c[i] = ProcTex.Hex(w[i]);
            return c;
        }

        static Vector3 S3(float p, float rx, float rz) { return new Vector3(p, rx, rz); }

        // Membangun mesh pada kerangka t (sudah dibuat dengan Kerangka.Buat). root = transform akar karakter.
        public static Mesh BangunMesh(JenisKarakter j, Bagian bagian, Transform root, Transform[] t)
        {
            var d = Ambil(j);
            var b = new CharacterMeshBuilder();
            float L = d.lebar;
            System.Func<int, Vector3> P = i => root.InverseTransformPoint(t[i].position);
            bool badan = bagian != Bagian.KepalaLengan, kepala = bagian != Bagian.BadanSaja;
            bool pocong = j == JenisKarakter.Pocong, kunti = j == JenisKarakter.Kuntilanak, topeng = j >= JenisKarakter.AbdiSekte && j <= JenisKarakter.PemimpinPatroli;

            if (badan)
            {
                if (pocong)
                {
                    // Kain kafan membungkus dari kaki sampai leher (lengan di dalam).
                    b.Loft(new Vector3(0f, 0.03f, 0f), P(Kerangka.Neck), new[] { S3(0f, 0.12f * L, 0.10f * L), S3(0.25f, 0.15f * L, 0.12f * L), S3(0.55f, 0.19f * L, 0.13f * L), S3(0.85f, 0.2f * L, 0.13f * L), S3(1f, 0.08f, 0.07f) }, Kerangka.Spine, Atasan);
                    b.Kerucut(P(Kerangka.Pelvis) + new Vector3(0f, -0.93f, 0f), P(Kerangka.Pelvis) + new Vector3(0f, -1.0f, 0f), 0.06f, Kerangka.Pelvis, Aks1);
                }
                else
                {
                    b.Loft(P(Kerangka.Pelvis) + Vector3.down * 0.08f, P(Kerangka.Spine), new[] { S3(0f, 0.15f * L * d.perut, 0.10f * L), S3(0.5f, 0.17f * L * d.perut, 0.12f * L * d.perut), S3(1f, 0.15f * L * d.perut, 0.11f * L * d.perut) }, Kerangka.Pelvis, kunti ? Atasan : Celana);
                    b.Loft(P(Kerangka.Spine), P(Kerangka.Chest), new[] { S3(0f, 0.15f * L * d.perut, 0.11f * L * d.perut), S3(1f, 0.155f * L * d.dada, 0.105f * L) }, Kerangka.Spine, Atasan, false);
                    b.Loft(P(Kerangka.Chest), P(Kerangka.Neck), new[] { S3(0f, 0.155f * L * d.dada, 0.105f * L), S3(0.6f, 0.18f * L * d.dada, 0.115f * L), S3(1f, 0.1f * L, 0.08f * L) }, Kerangka.Chest, Atasan);
                    if (kunti)
                    {
                        b.Loft(P(Kerangka.Pelvis), new Vector3(0f, 0.02f, 0f), new[] { S3(0f, 0.15f * L, 0.11f * L), S3(0.6f, 0.21f * L, 0.17f * L), S3(1f, 0.25f * L, 0.21f * L) }, Kerangka.Pelvis, Atasan);
                    }
                    else
                    {
                        for (int s = 0; s < 2; s++)
                        {
                            int paha = s == 0 ? Kerangka.LThigh : Kerangka.RThigh, betis = paha + 2, kaki = paha + 4;
                            bool botTinggi = j == JenisKarakter.Deden;
                            b.Loft(P(paha), P(betis), new[] { S3(0f, 0.085f * L, 0.085f * L), S3(0.4f, 0.08f * L, 0.08f * L), S3(1f, 0.056f * L, 0.056f * L) }, paha, Celana);
                            b.Loft(P(betis), P(kaki), new[] { S3(0f, 0.056f * L, 0.056f * L), S3(0.35f, 0.06f * L, 0.066f * L), S3(1f, 0.037f * L, 0.037f * L) }, betis, botTinggi ? Sepatu : Celana);
                            b.Kotak(P(kaki) + new Vector3(0f, -0.04f, 0.05f), new Vector3(0.095f * L, 0.08f, 0.25f * L), Quaternion.identity, kaki, Sepatu);
                        }
                    }
                    if (j == JenisKarakter.Deden)
                    {
                        // Rompi hi-vis + 2 strip reflektif.
                        b.Loft(P(Kerangka.Spine) + Vector3.down * 0.06f, P(Kerangka.Neck) + Vector3.down * 0.05f, new[] { S3(0f, 0.175f * L * d.perut, 0.125f * L * d.perut), S3(0.7f, 0.19f * L, 0.125f * L), S3(1f, 0.13f * L, 0.095f * L) }, Kerangka.Chest, Atasan, false);
                        b.Loft(P(Kerangka.Spine) + Vector3.up * 0.05f, P(Kerangka.Spine) + Vector3.up * 0.08f, new[] { S3(0f, 0.18f * L * d.perut, 0.13f * L * d.perut), S3(1f, 0.18f * L * d.perut, 0.13f * L * d.perut) }, Kerangka.Spine, Aks3, false);
                        b.Loft(P(Kerangka.Chest) + Vector3.up * 0.06f, P(Kerangka.Chest) + Vector3.up * 0.09f, new[] { S3(0f, 0.19f * L, 0.128f * L), S3(1f, 0.19f * L, 0.128f * L) }, Kerangka.Chest, Aks3, false);
                    }
                    if (j == JenisKarakter.Andi) b.Kotak(P(Kerangka.Chest) + new Vector3(0f, 0f, -0.15f), new Vector3(0.24f, 0.3f, 0.1f), Quaternion.identity, Kerangka.Chest, Aks3);
                }
            }

            if (kepala)
            {
                if (!pocong)
                {
                    for (int s = 0; s < 2; s++)
                    {
                        int atas = s == 0 ? Kerangka.LUpper : Kerangka.RUpper, bawah = atas + 2, tangan = atas + 4;
                        float lg = d.lengan * L;
                        b.Loft(P(atas), P(bawah), new[] { S3(0f, 0.056f * lg, 0.056f * lg), S3(0.3f, 0.051f * lg, 0.053f * lg), S3(1f, 0.041f * lg, 0.041f * lg) }, atas, LenganAtas);
                        b.Loft(P(bawah), P(tangan), new[] { S3(0f, 0.043f * lg, 0.043f * lg), S3(0.4f, 0.04f * lg, 0.038f * lg), S3(1f, 0.028f * lg, 0.024f * lg) }, bawah, LenganBawah);
                        b.Kotak(P(tangan) + new Vector3(0f, -0.055f, 0.01f), new Vector3(0.035f * lg, 0.1f * lg, 0.08f * lg), Quaternion.identity, tangan, Kulit);
                        if (j == JenisKarakter.Andi && s == 0) b.Loft(P(tangan) + Vector3.up * 0.03f, P(tangan) + Vector3.up * 0.04f, new[] { S3(0f, 0.032f, 0.032f), S3(1f, 0.032f, 0.032f) }, bawah, Aks2, true, 8);
                    }
                    b.Loft(P(Kerangka.Neck), P(Kerangka.Head), new[] { S3(0f, 0.055f * L, 0.055f * L), S3(1f, 0.05f * L, 0.05f * L) }, Kerangka.Neck, j == JenisKarakter.Andi ? Aks1 : Kulit, false);
                }
                Vector3 pusat = P(Kerangka.Head) + new Vector3(0f, 0.085f, 0.01f) * (d.tinggi / 1.75f);
                float sk = d.tinggi / 1.75f;
                b.Bola(pusat, new Vector3(0.078f, 0.105f, 0.095f) * sk, Kerangka.Head, pocong ? Atasan : Kulit, 16, 12);
                b.Kerucut(pusat + new Vector3(0f, -0.005f, 0.088f) * sk, pusat + new Vector3(0f, -0.025f, 0.112f) * sk, 0.014f * sk, Kerangka.Head, pocong ? Atasan : Kulit, 5);
                if (pocong)
                {
                    b.Bola(pusat + new Vector3(0f, -0.01f, 0.07f) * sk, new Vector3(0.055f, 0.07f, 0.03f) * sk, Kerangka.Head, Gelap, 10, 6);
                    b.Kerucut(pusat + new Vector3(0f, 0.1f, 0f) * sk, pusat + new Vector3(0f, 0.17f, -0.01f) * sk, 0.035f, Kerangka.Head, Aks1);
                }
                else
                {
                    for (int s = 0; s < 2; s++)
                    {
                        int mata = s == 0 ? Kerangka.LEye : Kerangka.REye, alis = mata + 2;
                        b.Bola(P(mata), Vector3.one * 0.0125f * sk, mata, PutihMata, 8, 6);
                        b.Bola(P(mata) + new Vector3(0f, 0f, 0.0115f) * sk, new Vector3(0.0068f, 0.0068f, 0.002f) * sk, mata, Iris, 8, 4);
                        b.Kotak(P(alis), new Vector3(0.03f, 0.006f, 0.008f) * sk, Quaternion.identity, alis, Rambut);
                    }
                    b.Kotak(pusat + new Vector3(0f, -0.045f, 0.088f) * sk, new Vector3(0.034f, 0.005f, 0.006f) * sk, Quaternion.identity, Kerangka.Jaw, Bibir);
                    if (topeng) b.Bola(pusat + new Vector3(0f, -0.005f, 0.07f) * sk, new Vector3(0.075f, 0.1f, 0.04f) * sk, Kerangka.Head, Aks1, 12, 8);
                    if (j == JenisKarakter.Andi) b.Bola(pusat + new Vector3(-0.036f, -0.014f, 0.081f) * sk, Vector3.one * 0.0045f, Kerangka.Head, Gelap, 6, 4);
                    BuatRambut(b, j, pusat, sk);
                }
            }
            return b.Selesai("M_Karakter_" + j + (bagian == Bagian.Semua ? "" : "_" + bagian), root, t);
        }

        static void BuatRambut(CharacterMeshBuilder b, JenisKarakter j, Vector3 c, float sk)
        {
            int h = Kerangka.Head;
            switch (j)
            {
                case JenisKarakter.Ningsih:
                    b.Bola(c + new Vector3(0f, 0.035f, -0.01f) * sk, new Vector3(0.083f, 0.085f, 0.09f) * sk, h, Rambut, 12, 8);
                    b.Bola(c + new Vector3(0f, -0.02f, -0.1f) * sk, new Vector3(0.05f, 0.04f, 0.035f) * sk, h, Rambut, 10, 6); // sanggul
                    b.Bola(c + new Vector3(0.05f, 0.0f, -0.095f) * sk, Vector3.one * 0.012f, h, Aks1, 6, 4);                // bunga kantil
                    b.Bola(c + new Vector3(0f, 0.07f, 0.075f) * sk, new Vector3(0.012f, 0.012f, 0.004f), h, Aks2, 6, 4);   // segel
                    break;
                case JenisKarakter.Kuntilanak:
                    b.Bola(c + new Vector3(0f, 0.03f, -0.005f) * sk, new Vector3(0.085f, 0.09f, 0.095f) * sk, h, Rambut, 12, 8);
                    b.Loft(c + new Vector3(0f, 0.05f, -0.06f) * sk, c + new Vector3(0f, -0.75f, -0.1f) * sk, new[] { S3(0f, 0.09f, 0.05f), S3(0.5f, 0.12f, 0.05f), S3(1f, 0.1f, 0.03f) }, h, Rambut);
                    b.Loft(c + new Vector3(0f, 0.06f, 0.07f) * sk, c + new Vector3(0f, -0.35f, 0.1f) * sk, new[] { S3(0f, 0.07f, 0.02f), S3(1f, 0.08f, 0.02f) }, h, Rambut); // menutup wajah
                    break;
                case JenisKarakter.KiRogo:
                    b.Bola(c + new Vector3(0f, 0.05f, -0.005f) * sk, new Vector3(0.085f, 0.07f, 0.095f) * sk, h, Aks1, 12, 6); // blangkon
                    b.Bola(c + new Vector3(0f, 0.0f, -0.1f) * sk, new Vector3(0.03f, 0.03f, 0.025f), h, Aks1, 6, 4);
                    break;
                case JenisKarakter.Deden:
                    b.Bola(c + new Vector3(0f, 0.035f, -0.005f) * sk, new Vector3(0.081f, 0.08f, 0.092f) * sk, h, Rambut, 12, 6);
                    b.Loft(c + new Vector3(0f, 0.045f, 0f) * sk, c + new Vector3(0f, 0.065f, 0f) * sk, new[] { S3(0f, 0.086f * sk, 0.098f * sk), S3(1f, 0.084f * sk, 0.096f * sk) }, h, Aks1, false, 12); // iket
                    b.Loft(c + new Vector3(0f, -0.025f, 0f) * sk, c + new Vector3(0f, 0.005f, 0f) * sk, new[] { S3(0f, 0.088f * sk, 0.06f * sk), S3(1f, 0.088f * sk, 0.06f * sk) }, h, Aks2, false, 12); // perban telinga
                    break;
                case JenisKarakter.Genderuwo:
                    b.Bola(c + new Vector3(0f, 0.02f, -0.01f) * sk, new Vector3(0.09f, 0.1f, 0.1f) * sk, h, Rambut, 12, 8);
                    break;
                default:
                    b.Bola(c + new Vector3(0f, 0.04f, -0.008f) * sk, new Vector3(0.082f, 0.075f, 0.092f) * sk, h, Rambut, 12, 6);
                    if (j == JenisKarakter.Andi)
                        for (int i = 0; i < 12; i++)
                        {
                            float a = i / 12f * Mathf.PI * 2f;
                            Vector3 basis = c + new Vector3(Mathf.Cos(a) * 0.05f, 0.09f, Mathf.Sin(a) * 0.05f) * sk;
                            b.Kerucut(basis, basis + new Vector3(0.025f + Mathf.Cos(a) * 0.02f, 0.03f, Mathf.Sin(a) * 0.02f + 0.012f), 0.022f, h, Rambut, 5);
                        }
                    break;
            }
        }
    }
}
