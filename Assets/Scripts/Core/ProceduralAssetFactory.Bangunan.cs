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
            // Bubungan & kasau atap + serambi depan + kusen pintu + jendela + perabotan interior
            b.Kotak(0, new Vector3(0f, yA + tAtap - 0.04f, 0f), new Vector3(lebar + over * 2f, 0.14f, 0.14f), Quaternion.identity);
            for (int rk = -2; rk <= 2; rk++)
                b.Kotak(0, new Vector3(rk * (lebar * 0.22f), yA - 0.04f, 0f), new Vector3(0.10f, 0.12f, dalam + over * 1.8f), Quaternion.identity);
            float zSerambi = -dalam * 0.5f - 0.55f;
            b.Kotak(0, new Vector3(0f, tinggiPanggung - 0.02f, zSerambi), new Vector3(lebar * 0.86f, 0.10f, 1.1f), Quaternion.identity);
            for (int sx = -1; sx <= 1; sx += 2)
            {
                b.Kotak(0, new Vector3(sx * (lebar * 0.40f), tinggiPanggung + 0.45f, zSerambi - 0.45f), new Vector3(0.12f, 0.95f, 0.12f), Quaternion.identity);
                b.Kotak(0, new Vector3(sx * (lebar * 0.27f), tinggiPanggung + 0.78f, zSerambi - 0.45f), new Vector3(lebar * 0.26f, 0.07f, 0.07f), Quaternion.identity);
            }
            b.Kotak(0, new Vector3(-0.62f, y0 + 1.05f, -dalam * 0.5f), new Vector3(0.10f, 2.1f, 0.14f), Quaternion.identity);
            b.Kotak(0, new Vector3( 0.62f, y0 + 1.05f, -dalam * 0.5f), new Vector3(0.10f, 2.1f, 0.14f), Quaternion.identity);
            b.Kotak(0, new Vector3( 0f,    y0 + 2.08f, -dalam * 0.5f), new Vector3(1.36f, 0.10f, 0.14f), Quaternion.identity);
            for (int ws = -1; ws <= 1; ws += 2)
                b.Kotak(1, new Vector3(ws * (lebar * 0.5f + 0.18f), y0 + 1.45f, 0f), new Vector3(0.05f, 0.85f, 1.15f), Quaternion.Euler(0f, 0f, -ws * 28f));
            if (rusak < 0.75f)
            {
                b.Kotak(0, new Vector3(-lebar * 0.22f, y0 + 0.72f, dalam * 0.15f), new Vector3(1.25f, 0.07f, 0.75f), Quaternion.identity);
                b.Kotak(0, new Vector3(-lebar * 0.22f, y0 + 0.36f, dalam * 0.15f), new Vector3(1.05f, 0.68f, 0.12f), Quaternion.identity);
                b.Kotak(0, new Vector3(-lebar * 0.22f, y0 + 0.42f, dalam * 0.15f - 0.58f), new Vector3(1.10f, 0.06f, 0.32f), Quaternion.identity);
                b.Kotak(0, new Vector3(lebar * 0.24f, y0 + 0.32f, dalam * 0.18f), new Vector3(1.35f, 0.12f, 1.85f), Quaternion.identity);
                b.Kotak(0, new Vector3(0f, y0 + 1.55f, dalam * 0.44f), new Vector3(1.8f, 0.06f, 0.34f), Quaternion.identity);
            }
            // Tangga
            for (int k = 0; k < 6; k++) b.Kotak(0, new Vector3(0, tinggiPanggung * (k + 0.5f) / 6f, -dalam * 0.5f - 0.3f - (5 - k) * 0.28f), new Vector3(1.0f, 0.06f, 0.25f), Quaternion.identity);
            return b.Jadi("Rumah_" + seed);
        }

        // Sumur batu desa berpeneduh kayu: 0 batu & kayu, 1 air gelap dalam sumur.
        public static Mesh SumurDesa()
        {
            var b = new B(2);
            b.Silinder(0, Vector3.zero, 1.05f, 1.0f, 0.92f, 12, Quaternion.identity);
            b.Silinder(1, new Vector3(0f, 0.06f, 0f), 0.80f, 0.80f, 0.62f, 10, Quaternion.identity);
            b.Quad(1, new Vector3(-0.75f, 0.66f, -0.75f), new Vector3(0.75f, 0.66f, -0.75f), new Vector3(0.75f, 0.66f, 0.75f), new Vector3(-0.75f, 0.66f, 0.75f), 1.5f);
            b.Kotak(0, new Vector3(-0.92f, 1.35f, 0f), new Vector3(0.14f, 1.6f, 0.14f), Quaternion.identity);
            b.Kotak(0, new Vector3( 0.92f, 1.35f, 0f), new Vector3(0.14f, 1.6f, 0.14f), Quaternion.identity);
            b.Kotak(0, new Vector3(0f, 2.02f, 0f), new Vector3(2.1f, 0.10f, 0.10f), Quaternion.identity);
            b.Kotak(0, new Vector3(0f, 2.32f, 0.32f), new Vector3(2.3f, 0.06f, 0.95f), Quaternion.Euler(24f, 0f, 0f));
            b.Kotak(0, new Vector3(0f, 2.32f, -0.32f), new Vector3(2.3f, 0.06f, 0.95f), Quaternion.Euler(-24f, 0f, 0f));
            return b.Jadi("SumurDesa");
        }

        // Gerobak kayu kampung: bak papan + dua roda berjari-jari. 0 kayu.
        public static Mesh GerobakKayu()
        {
            var b = new B(1);
            b.Kotak(0, new Vector3(0f, 0.68f, 0f), new Vector3(1.28f, 0.10f, 2.05f), Quaternion.identity);
            b.Kotak(0, new Vector3(-0.60f, 0.96f, 0f), new Vector3(0.08f, 0.50f, 2.05f), Quaternion.identity);
            b.Kotak(0, new Vector3( 0.60f, 0.96f, 0f), new Vector3(0.08f, 0.50f, 2.05f), Quaternion.identity);
            b.Kotak(0, new Vector3(0f, 0.96f, 0.98f), new Vector3(1.20f, 0.50f, 0.08f), Quaternion.identity);
            b.Kotak(0, new Vector3(-0.45f, 0.62f, -1.42f), new Vector3(0.09f, 0.09f, 1.1f), Quaternion.Euler(8f, 0f, 0f));
            b.Kotak(0, new Vector3( 0.45f, 0.62f, -1.42f), new Vector3(0.09f, 0.09f, 1.1f), Quaternion.Euler(8f, 0f, 0f));
            for (int sx = -1; sx <= 1; sx += 2)
            {
                float wx = sx * 0.74f;
                b.Silinder(0, new Vector3(wx - 0.04f, 0.52f, 0f), 0.52f, 0.52f, 0.08f, 12, Quaternion.Euler(0f, 0f, -90f));
                for (int sp = 0; sp < 4; sp++)
                    b.Kotak(0, new Vector3(wx, 0.52f, 0f), new Vector3(0.07f, 0.96f, 0.07f), Quaternion.Euler(sp * 45f, 0f, 0f));
            }
            return b.Jadi("GerobakKayu");
        }

        // Perahu cadik nelayan rusak di pesisir pantai: lambung lengkung + bangku + lengan cadik bambu patah. 0 kayu, 1 bilik/rusak.
        public static Mesh PerahuNelayanRusak()
        {
            var b = new B(2);
            const int N = 7;
            Vector3[] pL = new Vector3[N], pR = new Vector3[N], pK = new Vector3[N];
            for (int i = 0; i < N; i++)
            {
                float t = i / (float)(N - 1);
                float z = Mathf.Lerp(-2.6f, 2.6f, t);
                float w = Mathf.Sin(t * Mathf.PI) * 0.72f + 0.06f;
                float y = 0.25f + Mathf.Pow(Mathf.Abs(t - 0.5f) * 2f, 2f) * 0.35f;
                pL[i] = new Vector3(-w, y, z);
                pR[i] = new Vector3(w, y, z);
                pK[i] = new Vector3(0f, y - 0.30f, z);
            }
            for (int i = 0; i + 1 < N; i++)
            {
                b.Quad(0, pK[i], pL[i], pL[i + 1], pK[i + 1], 1.5f);
                b.Quad(0, pR[i], pK[i], pK[i + 1], pR[i + 1], 1.5f);
            }
            for (int tb = -1; tb <= 1; tb++)
                b.Kotak(0, new Vector3(0f, 0.24f, tb * 0.95f), new Vector3(1.1f, 0.06f, 0.26f), Quaternion.identity);
            // Cadik bambu kiri & kanan (salah satu patah miring)
            b.Kotak(1, new Vector3(-0.9f, 0.32f, -0.8f), new Vector3(1.9f, 0.07f, 0.07f), Quaternion.Euler(0f, 0f, -8f));
            b.Kotak(1, new Vector3(-0.9f, 0.32f,  0.8f), new Vector3(1.9f, 0.07f, 0.07f), Quaternion.Euler(0f, 0f, -8f));
            b.Silinder(1, new Vector3(-1.75f, 0.16f, -2.0f), 0.08f, 0.07f, 4.0f, 8, Quaternion.Euler(90f, 0f, 0f));
            return b.Jadi("PerahuNelayanRusak");
        }

        // Mesin ketel & rol penggiling tebu pabrik gula kolonial: 0 besi gelap, 1 besi karat.
        public static Mesh MesinPabrikGula()
        {
            var b = new B(2);
            b.Kotak(0, new Vector3(0f, 0.35f, 0f), new Vector3(3.2f, 0.7f, 1.8f), Quaternion.identity);
            b.Silinder(1, new Vector3(-1.45f, 1.45f, 0f), 0.68f, 0.68f, 2.9f, 12, Quaternion.Euler(0f, 0f, -90f));
            b.Silinder(0, new Vector3(0f, 1.45f, 0.95f), 0.85f, 0.85f, 0.14f, 14, Quaternion.Euler(90f, 0f, 0f));
            b.Silinder(1, new Vector3(0.8f, 2.1f, 0f), 0.16f, 0.16f, 2.2f, 8, Quaternion.identity);
            b.Silinder(0, new Vector3(-0.75f, 1.15f, -0.75f), 0.28f, 0.28f, 1.5f, 10, Quaternion.Euler(90f, 0f, 0f));
            return b.Jadi("MesinPabrikGula");
        }

        // Lori rel tebu di halaman pabrik gula: 0 besi karat, 1 bantalan kayu.
        public static Mesh LoriTebu()
        {
            var b = new B(2);
            for (int r = -1; r <= 1; r += 2)
                b.Kotak(0, new Vector3(r * 0.45f, 0.06f, 0f), new Vector3(0.08f, 0.08f, 5.2f), Quaternion.identity);
            for (int s = -4; s <= 4; s++)
                b.Kotak(1, new Vector3(0f, 0.03f, s * 0.56f), new Vector3(1.35f, 0.06f, 0.14f), Quaternion.identity);
            b.Kotak(0, new Vector3(0f, 0.42f, 0f), new Vector3(1.05f, 0.12f, 1.95f), Quaternion.identity);
            b.Kotak(0, new Vector3(-0.48f, 0.82f, 0f), new Vector3(0.07f, 0.72f, 1.95f), Quaternion.identity);
            b.Kotak(0, new Vector3( 0.48f, 0.82f, 0f), new Vector3(0.07f, 0.72f, 1.95f), Quaternion.identity);
            b.Kotak(0, new Vector3(0f, 0.82f, -0.94f), new Vector3(1.0f, 0.72f, 0.07f), Quaternion.identity);
            b.Kotak(0, new Vector3(0f, 0.82f,  0.94f), new Vector3(1.0f, 0.72f, 0.07f), Quaternion.identity);
            return b.Jadi("LoriTebu");
        }

        // Heli SAR rusak detail (lofted rounded hull): 0 badan putih/abu, 1 strip & ekor oranye SAR, 2 kaca kokpit/logam gelap/rotor/skid.
        public static Mesh HeliRusak()
        {
            var b = new B(3);
            const int ringSeg = 16;
            // Stasiun lambung dari moncong ke pangkal ekor: z, rx, ry, yMid
            float[][] st =
            {
                new[] { -4.35f, 0.34f, 0.24f, -0.44f },
                new[] { -3.98f, 0.72f, 0.44f, -0.34f },
                new[] { -3.30f, 0.96f, 0.66f, -0.06f },
                new[] { -2.35f, 1.16f, 0.84f,  0.22f },
                new[] { -0.95f, 1.22f, 0.88f,  0.25f },
                new[] {  0.75f, 1.20f, 0.88f,  0.25f },
                new[] {  1.95f, 0.88f, 0.68f,  0.34f },
                new[] {  2.55f, 0.48f, 0.44f,  0.50f },
            };
            Vector3 PSt(int ri, int si)
            {
                float a = si * Mathf.PI * 2f / ringSeg;
                float c = Mathf.Cos(a), s = Mathf.Sin(a);
                float sy = s < 0f ? s * 0.72f : s * (Mathf.Abs(c) < 0.7f ? 1.04f : 0.96f);
                return new Vector3(c * st[ri][1], st[ri][3] + sy * st[ri][2], st[ri][0]);
            }
            for (int r = 0; r < st.Length - 1; r++)
                for (int s = 0; s < ringSeg; s++)
                {
                    float aMid = (s + 0.5f) * Mathf.PI * 2f / ringSeg;
                    bool kacaDepan = (r == 1 || r == 2) && Mathf.Sin(aMid) > 0.15f && Mathf.Abs(Mathf.Cos(aMid)) < 0.86f;
                    bool stripOranye = !kacaDepan && Mathf.Sin(aMid) > -0.42f && Mathf.Sin(aMid) < -0.04f;
                    int sub = kacaDepan ? 2 : stripOranye ? 1 : 0;
                    b.Quad(sub, PSt(r, s + 1), PSt(r, s), PSt(r + 1, s), PSt(r + 1, s + 1), 2f);
                }
            // Tutup moncong depan & pilar kaca kokpit.
            Vector3 noseTip = new Vector3(0f, -0.46f, -4.48f);
            for (int s = 0; s < ringSeg; s++) b.Quad(1, PSt(0, s + 1), PSt(0, s), noseTip, noseTip, 1.5f);
            b.Kotak(2, new Vector3(0f, 0.42f, -3.10f), new Vector3(0.10f, 0.72f, 1.50f), Quaternion.Euler(26f, 0f, 0f)); // pilar tengah kaca
            b.Kotak(2, new Vector3(-0.96f, 0.28f, -1.35f), new Vector3(0.24f, 0.62f, 1.35f), Quaternion.identity);   // jendela samping kiri
            b.Kotak(2, new Vector3( 0.96f, 0.28f, -1.35f), new Vector3(0.24f, 0.62f, 1.35f), Quaternion.identity);   // jendela samping kanan
            b.Kotak(2, new Vector3(-1.02f, 0.18f,  0.15f), new Vector3(0.20f, 0.95f, 1.10f), Quaternion.identity);   // bukaan kabin gelap kiri
            // Pintu samping kanan penyok terlepas sebagian.
            b.Kotak(0, new Vector3(1.36f, 0.12f, 0.05f), new Vector3(0.10f, 1.15f, 1.28f), Quaternion.Euler(10f, 24f, -16f));
            b.Kotak(1, new Vector3(1.38f, -0.10f, 0.06f), new Vector3(0.12f, 0.24f, 1.28f), Quaternion.Euler(10f, 24f, -16f));
            // Strip oranye sabuk atas & cowling mesin ganda + 2 intake + 2 knalpot turbin.
            b.Kotak(0, new Vector3(0f, 1.18f, 0.05f), new Vector3(1.18f, 0.52f, 2.75f), Quaternion.identity);
            b.Kotak(1, new Vector3(0f, 1.20f, -0.25f), new Vector3(1.24f, 0.16f, 2.25f), Quaternion.identity);
            b.Silinder(2, new Vector3(-0.34f, 1.18f, -1.35f), 0.22f, 0.18f, 0.45f, 10, Quaternion.Euler(-90f, 0f, 0f));
            b.Silinder(2, new Vector3( 0.34f, 1.18f, -1.35f), 0.22f, 0.18f, 0.45f, 10, Quaternion.Euler(-90f, 0f, 0f));
            b.Silinder(2, new Vector3(-0.34f, 1.22f,  1.38f), 0.18f, 0.15f, 0.85f, 10, Quaternion.Euler(82f, -10f, 0f));
            b.Silinder(2, new Vector3( 0.34f, 1.22f,  1.38f), 0.18f, 0.15f, 0.85f, 10, Quaternion.Euler(82f,  10f, 0f));
            // Mast & swashplate + 4 bilah baling-baling patah terkulai.
            b.Silinder(2, new Vector3(0f, 1.44f, -0.12f), 0.24f, 0.20f, 0.56f, 12, Quaternion.identity);
            b.Silinder(2, new Vector3(0f, 1.88f, -0.12f), 0.36f, 0.32f, 0.12f, 12, Quaternion.identity);
            for (int k = 0; k < 4; k++)
            {
                float yaw = k * 90f + (k % 2 == 0 ? 14f : -16f);
                Quaternion qr = Quaternion.Euler(6f + k * 4.5f, yaw, k % 2 == 0 ? 9f : -11f);
                float blen = k == 2 ? 3.2f : 5.6f; // satu bilah patah pendek
                b.Kotak(2, new Vector3(0f, 1.98f, -0.12f) + qr * new Vector3(0f, -0.14f * (k % 3), blen * 0.52f), new Vector3(0.28f, 0.045f, blen), qr);
            }
            // Ekor tirus (tailboom) + sirip tegak SAR + stabilizer + rotor ekor bengkok.
            b.Silinder(1, new Vector3(0f, 0.54f, 2.35f), 0.48f, 0.20f, 5.15f, 12, Quaternion.Euler(86f, 0f, 0f));
            b.Kotak(1, new Vector3(0f, 1.20f, 7.18f), new Vector3(0.16f, 1.62f, 0.96f), Quaternion.Euler(-24f, 0f, 0f));
            b.Kotak(0, new Vector3(0f, 1.56f, 7.36f), new Vector3(0.18f, 0.32f, 0.68f), Quaternion.Euler(-24f, 0f, 0f));
            b.Kotak(1, new Vector3(0f, 0.48f, 5.65f), new Vector3(2.25f, 0.09f, 0.56f), Quaternion.Euler(0f, 0f, 5f));
            b.Silinder(2, new Vector3(0.18f, 1.54f, 7.35f), 0.12f, 0.10f, 0.22f, 8, Quaternion.Euler(0f, 0f, -90f));
            b.Kotak(2, new Vector3(0.24f, 1.54f, 7.35f), new Vector3(0.05f, 1.05f, 0.15f), Quaternion.Euler(32f, 0f, 8f));
            // Skid pendaratan tubular melengkung + 4 penyangga miring.
            for (int side = -1; side <= 1; side += 2)
            {
                b.Silinder(2, new Vector3(side * 1.15f, -1.04f, -2.55f), 0.075f, 0.075f, 5.15f, 8, Quaternion.Euler(90f, 0f, side * 4f));
                b.Silinder(2, new Vector3(side * 1.15f, -1.04f, -2.55f), 0.075f, 0.06f, 0.55f, 8, Quaternion.Euler(-58f, 0f, 0f));
                b.Silinder(2, new Vector3(side * 1.10f, -1.04f, -1.15f), 0.06f, 0.06f, 0.85f, 6, Quaternion.Euler(0f, 0f, -side * 22f));
                b.Silinder(2, new Vector3(side * 1.10f, -1.04f,  1.15f), 0.06f, 0.06f, 0.85f, 6, Quaternion.Euler(0f, 0f, -side * 22f));
            }
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

        // Gapura candi bentar Nusantara & dua tungku batu sesaji di kanan-kiri gerbang Beringin Agung. 0 batu candi, 1 batu gelap/arang tungku.
        public static Mesh GapuraBatuKeramat()
        {
            var b = new B(2);
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * 2.6f;
                // Alas umpak bertingkat + tubuh gapura berukir + mahkota candi berjenjang.
                b.Kotak(0, new Vector3(x, 0.35f, 0f), new Vector3(1.85f, 0.70f, 1.85f), Quaternion.identity, 1.2f);
                b.Kotak(0, new Vector3(x, 0.88f, 0f), new Vector3(1.52f, 0.36f, 1.52f), Quaternion.identity, 1.2f);
                b.Kotak(0, new Vector3(x, 2.05f, 0f), new Vector3(1.18f, 2.00f, 1.18f), Quaternion.identity, 1.2f);
                b.Kotak(1, new Vector3(x - side * 0.12f, 2.05f, 0f), new Vector3(0.98f, 1.35f, 0.72f), Quaternion.identity, 1f);
                for (int t = 0; t < 4; t++)
                {
                    float w = 1.46f - t * 0.24f;
                    b.Kotak(0, new Vector3(x, 3.22f + t * 0.32f, 0f), new Vector3(w, 0.28f, w), Quaternion.identity, 1f);
                }
                // Tungku batu & mangkuk arang sesaji di samping tiang.
                float tx = side * 3.85f;
                b.Silinder(0, new Vector3(tx, 0f, 0.95f), 0.42f, 0.32f, 0.85f, 8, Quaternion.identity, 1f);
                b.Silinder(0, new Vector3(tx, 0.85f, 0.95f), 0.32f, 0.52f, 0.28f, 8, Quaternion.identity, 1f);
                b.Silinder(1, new Vector3(tx, 1.08f, 0.95f), 0.42f, 0.36f, 0.10f, 8, Quaternion.identity, 1f);
            }
            return b.Jadi("GapuraBatuKeramat");
        }

        // Jembatan kayu gelondong & papan berpagar melintasi sungai (panjang 28 m sepanjang Z). 0 kayu.
        public static Mesh JembatanKayu()
        {
            var b = new B(1);
            for (int side = -1; side <= 1; side += 2)
                b.Kotak(0, new Vector3(side * 1.05f, -0.12f, 0f), new Vector3(0.28f, 0.28f, 26f), Quaternion.identity, 2f);
            for (int p = 0; p < 18; p++)
            {
                float z = -12.2f + p * 1.43f;
                b.Kotak(0, new Vector3(0f, 0.04f, z), new Vector3(2.6f, 0.08f, 1.18f), Quaternion.identity, 1.5f);
            }
            b.Kotak(0, new Vector3(0f, -0.18f, -13.2f), new Vector3(2.6f, 0.12f, 2.2f), Quaternion.Euler(14f, 0f, 0f), 1.5f);
            b.Kotak(0, new Vector3(0f, -0.18f,  13.2f), new Vector3(2.6f, 0.12f, 2.2f), Quaternion.Euler(-14f, 0f, 0f), 1.5f);
            float[] zp = { -8f, 0f, 8f };
            for (int i = 0; i < zp.Length; i++)
                for (int side = -1; side <= 1; side += 2)
                {
                    b.Silinder(0, new Vector3(side * 1.1f, -3.6f, zp[i]), 0.16f, 0.14f, 3.8f, 8, Quaternion.identity);
                    b.Kotak(0, new Vector3(side * 1.24f, 0.6f, zp[i]), new Vector3(0.14f, 1.08f, 0.14f), Quaternion.identity);
                }
            for (int side = -1; side <= 1; side += 2)
                b.Kotak(0, new Vector3(side * 1.24f, 1.05f, 0f), new Vector3(0.10f, 0.10f, 17f), Quaternion.identity, 2f);
            return b.Jadi("JembatanKayu");
        }

        // Dermaga kayu nelayan menjorok ke laut: balok, papan dek, 8 tiang pancang laut & patok tambat. 0 kayu.
        public static Mesh DermagaPantai()
        {
            var b = new B(1);
            for (int side = -1; side <= 1; side += 2)
                b.Kotak(0, new Vector3(side * 1.25f, -0.1f, 0f), new Vector3(0.28f, 0.28f, 18f), Quaternion.identity, 2f);
            for (int p = 0; p < 14; p++)
                b.Kotak(0, new Vector3(0f, 0.06f, -8.4f + p * 1.28f), new Vector3(3.2f, 0.08f, 1.05f), Quaternion.identity, 1.5f);
            for (int r = 0; r < 4; r++)
                for (int side = -1; side <= 1; side += 2)
                {
                    float z = -7.5f + r * 5f;
                    b.Silinder(0, new Vector3(side * 1.35f, -3.8f, z), 0.18f, 0.16f, 4.1f, 8, Quaternion.identity);
                    if (r == 3) b.Silinder(0, new Vector3(side * 1.38f, 0.1f, z + 0.7f), 0.14f, 0.14f, 0.65f, 8, Quaternion.identity);
                }
            return b.Jadi("DermagaPantai");
        }

        // Tenda ekspedisi darurat (A-frame terpal + tiang kayu + matras & peti logistik). 0 terpal, 1 kayu.
        public static Mesh TendaEkspedisi()
        {
            var b = new B(2);
            b.Kotak(0, new Vector3(-0.78f, 0.92f, 0f), new Vector3(0.06f, 2.05f, 3.4f), Quaternion.Euler(0f, 0f, -38f), 1.5f);
            b.Kotak(0, new Vector3( 0.78f, 0.92f, 0f), new Vector3(0.06f, 2.05f, 3.4f), Quaternion.Euler(0f, 0f,  38f), 1.5f);
            b.Kotak(0, new Vector3(0f, 0.05f, 0f), new Vector3(2.2f, 0.08f, 3.1f), Quaternion.identity, 1.5f);
            b.Silinder(1, new Vector3(0f, 0f, -1.65f), 0.05f, 0.045f, 1.78f, 6, Quaternion.identity);
            b.Silinder(1, new Vector3(0f, 0f,  1.65f), 0.05f, 0.045f, 1.78f, 6, Quaternion.identity);
            b.Silinder(1, new Vector3(0f, 1.74f, -1.75f), 0.045f, 0.045f, 3.5f, 6, Quaternion.Euler(90f, 0f, 0f));
            b.Kotak(1, new Vector3(1.55f, 0.30f, -1.1f), new Vector3(0.68f, 0.56f, 0.88f), Quaternion.Euler(0f, 12f, 0f), 1f);
            return b.Jadi("TendaEkspedisi");
        }

        // Menara pantau kayu pos ronda/ekspedisi: 4 tiang sudut, penyangga silang, balkon 3.8 m berpagar, tangga miring & atap limas. 0 kayu, 1 atap.
        public static Mesh MenaraPantauKayu()
        {
            var b = new B(2);
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    b.Kotak(0, new Vector3(sx * 1.35f, 3.1f, sz * 1.35f), new Vector3(0.22f, 6.2f, 0.22f), Quaternion.identity, 2f);
            for (int s = 0; s < 3; s++)
            {
                Quaternion q = Quaternion.Euler(0f, (s + 1) * 90f, 0f);
                b.Kotak(0, q * new Vector3(0f, 1.9f, 1.35f), new Vector3(2.8f, 0.14f, 0.12f), q * Quaternion.Euler(0f, 0f, 34f), 1.5f);
                b.Kotak(0, q * new Vector3(0f, 1.9f, 1.35f), new Vector3(2.8f, 0.14f, 0.12f), q * Quaternion.Euler(0f, 0f, -34f), 1.5f);
            }
            b.Kotak(0, new Vector3(0f, 3.8f, 0f), new Vector3(3.1f, 0.14f, 3.1f), Quaternion.identity, 1.5f);
            for (int s = 0; s < 4; s++)
            {
                Quaternion q = Quaternion.Euler(0f, s * 90f, 0f);
                b.Kotak(0, q * new Vector3(0f, 4.7f, 1.42f), new Vector3(2.8f, 0.10f, 0.10f), q, 1.5f);
            }
            for (int side = -1; side <= 1; side += 2)
                b.Kotak(0, new Vector3(side * 0.32f, 2.0f, -1.75f), new Vector3(0.08f, 4.2f, 0.10f), Quaternion.Euler(-14f, 0f, 0f), 1.5f);
            for (int r = 0; r < 8; r++)
                b.Kotak(0, new Vector3(0f, 0.45f + r * 0.46f, -2.18f + r * 0.11f), new Vector3(0.68f, 0.06f, 0.08f), Quaternion.identity, 1f);
            for (int s = 0; s < 4; s++)
            {
                Quaternion q = Quaternion.Euler(0f, s * 90f, 0f);
                b.Kotak(1, q * new Vector3(0f, 6.45f, 0.92f), new Vector3(3.6f, 0.08f, 2.3f), q * Quaternion.Euler(28f, 0f, 0f), 2f);
            }
            return b.Jadi("MenaraPantauKayu");
        }

        // Rambu petunjuk arah jalur ekspedisi: umpak batu, tiang kayu, 2 papan panah bersilang & lengan gantung lentera. 0 kayu, 1 batu/besi.
        public static Mesh RambuPetunjukKayu()
        {
            var b = new B(2);
            b.Kotak(1, new Vector3(0f, 0.14f, 0f), new Vector3(0.55f, 0.28f, 0.55f), Quaternion.identity, 1f);
            b.Silinder(0, new Vector3(0f, 0.22f, 0f), 0.09f, 0.08f, 2.35f, 8, Quaternion.identity, 1.5f);
            b.Kotak(0, new Vector3(0.28f, 2.15f, 0.08f), new Vector3(0.86f, 0.20f, 0.05f), Quaternion.Euler(0f, 18f, 3f), 1f);
            b.Kotak(0, new Vector3(-0.24f, 1.85f, -0.08f), new Vector3(0.78f, 0.18f, 0.05f), Quaternion.Euler(0f, -35f, -2f), 1f);
            b.Kotak(0, new Vector3(0f, 2.38f, 0.22f), new Vector3(0.07f, 0.07f, 0.48f), Quaternion.identity, 1f);
            b.Silinder(1, new Vector3(0f, 2.14f, 0.40f), 0.07f, 0.05f, 0.18f, 6, Quaternion.identity, 1f);
            return b.Jadi("RambuPetunjukKayu");
        }
    }
}
