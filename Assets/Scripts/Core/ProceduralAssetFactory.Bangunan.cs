using System.Collections.Generic;
using UnityEngine;

namespace AlasKeramat.Core
{
    // Mesh landmark prosedural (T2c): rumah panggung, heli rusak, mercusuar, cerobong & gedung pabrik, mulut gua, altar.
    // Setiap mesh punya beberapa submesh; urutan material dicatat di komentar tiap fungsi.
    public static class ProcBangunan
    {
        sealed class B
        {
            readonly List<Vector3> v = new List<Vector3>(); readonly List<Vector3> n = new List<Vector3>(); readonly List<Vector2> uv = new List<Vector2>();
            readonly List<List<int>> sub = new List<List<int>>();
            public B(int nSub) { for (int i = 0; i < nSub; i++) sub.Add(new List<int>()); }

            public void Quad(int s, Vector3 a, Vector3 b, Vector3 c, Vector3 d, float skalaUV)
            {
                Vector3 nn = Vector3.Cross(b - a, d - a).normalized;
                int i = v.Count;
                v.Add(a); v.Add(b); v.Add(c); v.Add(d);
                for (int k = 0; k < 4; k++) n.Add(nn);
                float w = Vector3.Distance(a, b) / skalaUV, h = Vector3.Distance(a, d) / skalaUV;
                uv.Add(new Vector2(0, 0)); uv.Add(new Vector2(w, 0)); uv.Add(new Vector2(w, h)); uv.Add(new Vector2(0, h));
                sub[s].Add(i); sub[s].Add(i + 3); sub[s].Add(i + 2); sub[s].Add(i); sub[s].Add(i + 2); sub[s].Add(i + 1);
            }

            // Kotak berpusat c, ukuran sz, rotasi rot.
            public void Kotak(int s, Vector3 c, Vector3 sz, Quaternion rot, float skalaUV = 2f)
            {
                Vector3 h = sz * 0.5f;
                Vector3[] p =
                {
                    c + rot * new Vector3(-h.x, -h.y, -h.z), c + rot * new Vector3(h.x, -h.y, -h.z), c + rot * new Vector3(h.x, h.y, -h.z), c + rot * new Vector3(-h.x, h.y, -h.z),
                    c + rot * new Vector3(-h.x, -h.y, h.z), c + rot * new Vector3(h.x, -h.y, h.z), c + rot * new Vector3(h.x, h.y, h.z), c + rot * new Vector3(-h.x, h.y, h.z)
                };
                Quad(s, p[0], p[1], p[2], p[3], skalaUV); Quad(s, p[5], p[4], p[7], p[6], skalaUV);
                Quad(s, p[4], p[0], p[3], p[7], skalaUV); Quad(s, p[1], p[5], p[6], p[2], skalaUV);
                Quad(s, p[3], p[2], p[6], p[7], skalaUV); Quad(s, p[4], p[5], p[1], p[0], skalaUV);
            }

            public void Silinder(int s, Vector3 dasar, float r0, float r1, float tinggi, int sisi, Quaternion rot, float skalaUV = 2f)
            {
                for (int k = 0; k < sisi; k++)
                {
                    float a0 = k * Mathf.PI * 2f / sisi, a1 = (k + 1) * Mathf.PI * 2f / sisi;
                    Vector3 d0 = new Vector3(Mathf.Cos(a0), 0, Mathf.Sin(a0)), d1 = new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1));
                    Quad(s, dasar + rot * (d0 * r0), dasar + rot * (d1 * r0), dasar + rot * (d1 * r1 + Vector3.up * tinggi), dasar + rot * (d0 * r1 + Vector3.up * tinggi), skalaUV);
                }
            }

            public Mesh Jadi(string nama)
            {
                var m = new Mesh { name = nama };
                if (v.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                m.SetVertices(v); m.SetNormals(n); m.SetUVs(0, uv);
                m.subMeshCount = sub.Count;
                for (int i = 0; i < sub.Count; i++) m.SetTriangles(sub[i], i);
                m.RecalculateTangents(); m.RecalculateBounds();
                return m;
            }
        }

        // Rumah panggung Jawa: submesh 0 kayu (tiang, lantai, dinding papan), 1 atap (seng/ijuk). rusak 0..1 = lubang dinding & atap.
        public static Mesh RumahPanggung(int seed, float lebar, float dalam, float tinggiPanggung, float rusak)
        {
            var r = new System.Random(seed);
            var b = new B(2);
            float tDinding = 2.6f, tAtap = 2.2f;
            for (int ix = 0; ix < 3; ix++) for (int iz = 0; iz < 3; iz++)
                    b.Kotak(0, new Vector3((ix - 1) * lebar * 0.45f, tinggiPanggung * 0.5f - 0.3f, (iz - 1) * dalam * 0.45f), new Vector3(0.22f, tinggiPanggung + 0.6f, 0.22f), Quaternion.identity);
            b.Kotak(0, new Vector3(0, tinggiPanggung, 0), new Vector3(lebar, 0.15f, dalam), Quaternion.identity);
            float y0 = tinggiPanggung + 0.075f;
            int papan = Mathf.RoundToInt(lebar / 0.45f);
            for (int sisi = 0; sisi < 4; sisi++)
            {
                bool panjang = sisi % 2 == 0;
                float L = panjang ? lebar : dalam;
                int np = panjang ? papan : Mathf.RoundToInt(dalam / 0.45f);
                Quaternion q = Quaternion.Euler(0, sisi * 90f, 0);
                float jarak = (panjang ? dalam : lebar) * 0.5f;
                for (int p = 0; p < np; p++)
                {
                    if ((float)r.NextDouble() < rusak * 0.35f) continue; // papan hilang
                    if (sisi == 0 && Mathf.Abs(p - np / 2) <= 1) continue;  // pintu
                    float x = -L * 0.5f + (p + 0.5f) * L / np;
                    float tinggi = tDinding * (1f - (float)r.NextDouble() * rusak * 0.3f);
                    b.Kotak(0, new Vector3(0, y0, 0) + q * new Vector3(x, tinggi * 0.5f, -jarak), new Vector3(L / np - 0.03f, tinggi, 0.05f), q * Quaternion.Euler(0, 0, ((float)r.NextDouble() - 0.5f) * rusak * 6f));
                }
            }
            // Atap pelana dua bidang
            float yA = y0 + tDinding, over = 0.6f;
            for (int s = -1; s <= 1; s += 2)
            {
                if ((float)r.NextDouble() < rusak * 0.25f) continue;
                Vector3 a = new Vector3(-lebar * 0.5f - over, yA, s * (dalam * 0.5f + over)), d = new Vector3(-lebar * 0.5f - over, yA + tAtap, 0f);
                Vector3 bb = new Vector3(lebar * 0.5f + over, yA, s * (dalam * 0.5f + over)), c = new Vector3(lebar * 0.5f + over, yA + tAtap, 0f);
                if (s > 0) b.Quad(1, a, bb, c, d, 2f); else b.Quad(1, bb, a, d, c, 2f);
                if (s > 0) b.Quad(1, bb, a, d, c, 2f); else b.Quad(1, a, bb, c, d, 2f); // sisi bawah atap
            }
            // Tangga
            for (int k = 0; k < 6; k++) b.Kotak(0, new Vector3(0, tinggiPanggung * (k + 0.5f) / 6f, -dalam * 0.5f - 0.3f - (5 - k) * 0.28f), new Vector3(1.0f, 0.06f, 0.25f), Quaternion.identity);
            return b.Jadi("Rumah_" + seed);
        }

        // Heli rusak: 0 badan putih, 1 strip oranye & ekor, 2 logam gelap (rotor, roda).
        public static Mesh HeliRusak()
        {
            var b = new B(3);
            b.Silinder(0, new Vector3(0, 0.2f, -4.5f), 1.25f, 1.25f, 9f, 14, Quaternion.Euler(90, 0, 0));
            b.Kotak(1, new Vector3(0, 0.25f, 0), new Vector3(2.62f, 0.35f, 8.8f), Quaternion.identity);
            b.Silinder(1, new Vector3(0, 0.6f, 4.3f), 0.45f, 0.18f, 4.5f, 8, Quaternion.Euler(90, 0, 0));
            b.Kotak(2, new Vector3(0, 1.75f, 0), new Vector3(0.5f, 0.4f, 0.5f), Quaternion.identity);
            for (int k = 0; k < 3; k++)
                b.Kotak(2, new Vector3(0, 1.9f, 0) + Quaternion.Euler(0, k * 120f, 0) * new Vector3(0, -0.2f * k, 3.2f), new Vector3(0.35f, 0.05f, 6.2f), Quaternion.Euler(8f + k * 7f, k * 120f, k * 4f));
            b.Kotak(2, new Vector3(-1.0f, -1.25f, 0), new Vector3(0.12f, 0.12f, 6f), Quaternion.identity);
            b.Kotak(2, new Vector3(1.0f, -1.25f, 0), new Vector3(0.12f, 0.12f, 6f), Quaternion.identity);
            return b.Jadi("HeliRusak");
        }

        // Mercusuar 22 m: 0 cat putih, 1 strip merah pudar, 2 logam (pagar & ruang lampu).
        public static Mesh Mercusuar()
        {
            var b = new B(3);
            const float T = 22f; const int Seg = 6;
            for (int i = 0; i < Seg; i++)
            {
                float y0 = i * T / Seg, r0 = Mathf.Lerp(3.2f, 2.0f, (float)i / Seg), r1 = Mathf.Lerp(3.2f, 2.0f, (float)(i + 1) / Seg);
                b.Silinder(i % 2 == 0 ? 0 : 1, new Vector3(0, y0, 0), r0, r1, T / Seg, 18, Quaternion.identity);
            }
            b.Silinder(2, new Vector3(0, T, 0), 2.6f, 2.6f, 0.25f, 18, Quaternion.identity);
            b.Silinder(2, new Vector3(0, T + 0.25f, 0), 1.6f, 1.6f, 2.4f, 12, Quaternion.identity);
            b.Silinder(1, new Vector3(0, T + 2.65f, 0), 1.8f, 0.2f, 1.6f, 12, Quaternion.identity);
            return b.Jadi("Mercusuar");
        }

        // Pabrik gula: 0 bata, 1 atap seng berkarat, 2 kayu/rangka. cerobong = true untuk cerobong 38 m.
        public static Mesh GedungPabrik(int seed, float p, float l, float t, bool cerobong)
        {
            var r = new System.Random(seed);
            var b = new B(3);
            if (cerobong)
            {
                b.Kotak(0, new Vector3(0, 2f, 0), new Vector3(5f, 4f, 5f), Quaternion.identity);
                b.Silinder(0, new Vector3(0, 4f, 0), 2.1f, 1.3f, 34f, 16, Quaternion.identity);
                b.Silinder(0, new Vector3(0, 37.6f, 0), 1.55f, 1.55f, 0.6f, 16, Quaternion.identity);
                return b.Jadi("Cerobong");
            }
            float tebal = 0.45f;
            for (int s = 0; s < 4; s++)
            {
                bool panjang = s % 2 == 0;
                float L = panjang ? p : l, jarak = (panjang ? l : p) * 0.5f;
                Quaternion q = Quaternion.Euler(0, s * 90f, 0);
                int jendela = Mathf.Max(1, Mathf.RoundToInt(L / 4f));
                for (int j = 0; j < jendela; j++)
                {
                    float x0 = -L * 0.5f + j * L / jendela, x1 = x0 + L / jendela;
                    float lebarJ = (x1 - x0) * 0.4f, cx = (x0 + x1) * 0.5f;
                    // Dinding dengan lubang jendela (kiri, kanan, bawah, atas).
                    b.Kotak(0, q * new Vector3((x0 + cx - lebarJ * 0.5f) * 0.5f, t * 0.5f, -jarak), new Vector3(cx - lebarJ * 0.5f - x0, t, tebal), q);
                    b.Kotak(0, q * new Vector3((cx + lebarJ * 0.5f + x1) * 0.5f, t * 0.5f, -jarak), new Vector3(x1 - cx - lebarJ * 0.5f, t, tebal), q);
                    b.Kotak(0, q * new Vector3(cx, 0.75f, -jarak), new Vector3(lebarJ, 1.5f, tebal), q);
                    float atasJ = t - 1.2f - (float)r.NextDouble() * 0.6f;
                    b.Kotak(0, q * new Vector3(cx, (atasJ + t) * 0.5f + 0.4f, -jarak), new Vector3(lebarJ, t - atasJ - 0.8f + 0.8f, tebal), q);
                }
            }
            // Rangka atap kayu + atap seng berlubang.
            for (int k = 0; k <= Mathf.RoundToInt(p / 4f); k++) b.Kotak(2, new Vector3(-p * 0.5f + k * 4f, t + 0.15f, 0), new Vector3(0.2f, 0.3f, l), Quaternion.identity);
            int panel = Mathf.RoundToInt(p / 2f);
            for (int k = 0; k < panel; k++)
            {
                if ((float)r.NextDouble() < 0.3f) continue;
                b.Kotak(1, new Vector3(-p * 0.5f + (k + 0.5f) * p / panel, t + 0.4f, 0), new Vector3(p / panel - 0.05f, 0.04f, l + 0.8f), Quaternion.Euler(((float)r.NextDouble() - 0.5f) * 6f, 0, 0));
            }
            return b.Jadi("Gedung_" + seed);
        }

        // Mulut gua: gundukan batu setengah bola terdeformasi dengan lubang gelap. 0 batu, 1 hitam.
        public static Mesh MulutGua(int seed, float radius)
        {
            var b = new B(2);
            var r = new System.Random(seed);
            int lat = 8, lon = 18;
            for (int i = 0; i < lat; i++)
                for (int j = 0; j < lon; j++)
                {
                    if (j <= 2 && i <= 3) continue; // bukaan di depan (+Z)
                    Vector3 P(int ii, int jj)
                    {
                        float th = ii * Mathf.PI * 0.5f / lat, ph = jj * Mathf.PI * 2f / lon - Mathf.PI * 0.17f + Mathf.PI * 0.5f;
                        float k = 1f + 0.18f * Mathf.Sin(ii * 2.1f + jj * 1.3f + seed);
                        return new Vector3(Mathf.Cos(th) * Mathf.Cos(ph), Mathf.Sin(th) * 0.7f, Mathf.Cos(th) * Mathf.Sin(ph)) * radius * k;
                    }
                    b.Quad(0, P(i, j), P(i, j + 1), P(i + 1, j + 1), P(i + 1, j), 3f);
                }
            b.Quad(1, new Vector3(-radius * 0.35f, 0, radius * 0.55f), new Vector3(radius * 0.35f, 0, radius * 0.55f), new Vector3(radius * 0.3f, radius * 0.38f, radius * 0.5f), new Vector3(-radius * 0.3f, radius * 0.38f, radius * 0.5f), 4f);
            return b.Jadi("MulutGua_" + seed);
        }

        // Altar batu 2,4 x 1,2 x 0,9 + 4 lentera. 0 batu.
        public static Mesh Altar()
        {
            var b = new B(1);
            b.Kotak(0, new Vector3(0, 0.45f, 0), new Vector3(2.4f, 0.9f, 1.2f), Quaternion.identity, 1f);
            b.Kotak(0, new Vector3(0, 0.95f, 0), new Vector3(2.6f, 0.12f, 1.4f), Quaternion.identity, 1f);
            for (int k = 0; k < 4; k++)
            {
                Vector3 c = new Vector3((k % 2 == 0 ? -1 : 1) * 2.2f, 0, (k < 2 ? -1 : 1) * 1.6f);
                b.Kotak(0, c + Vector3.up * 0.5f, new Vector3(0.3f, 1f, 0.3f), Quaternion.identity, 1f);
                b.Kotak(0, c + Vector3.up * 1.15f, new Vector3(0.5f, 0.35f, 0.5f), Quaternion.identity, 1f);
            }
            return b.Jadi("Altar");
        }
    }
}
