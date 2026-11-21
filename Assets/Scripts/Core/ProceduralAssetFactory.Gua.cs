using System.Collections.Generic;
using UnityEngine;
namespace AlasKeramat.Core
{
    // Mesh ruang bawah tanah (03 §10): dinding superelips + kubah (gua) atau plafon datar (bunker), normal menghadap ke dalam,
    // dengan celah bukaan untuk lorong. Lantai dibuat terpisah (balok ber-collider) oleh Builder.
    public static class ProcGua
    {
        public const int Segmen = 56, BarisDinding = 6, BarisKubah = 10;
        public struct Bukaan { public float sudut; public float lebar; public float tinggi; }

        // Sudut 0 = +Z, 90 = +X (derajat).
        public static float SudutKe(Vector3 dari, Vector3 ke) { Vector3 d = ke - dari; return Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg; }

        public static float JariJari(float sudutDeg, float a, float b, float n)
        {
            float t = sudutDeg * Mathf.Deg2Rad;
            float s = Mathf.Abs(Mathf.Sin(t)) / Mathf.Max(0.01f, a), c = Mathf.Abs(Mathf.Cos(t)) / Mathf.Max(0.01f, b);
            float v = Mathf.Pow(s, n) + Mathf.Pow(c, n);
            return v <= 0f ? Mathf.Max(a, b) : Mathf.Pow(v, -1f / n);
        }

        public static Vector3 Tepi(Vector3 ukuran, bool kubah, float sudutDeg, float masuk)
        {
            float r = JariJari(sudutDeg, ukuran.x * 0.5f, ukuran.z * 0.5f, kubah ? 2.2f : 8f) * masuk;
            float t = sudutDeg * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(t) * r, 0f, Mathf.Cos(t) * r);
        }

        static bool DiBukaan(Bukaan[] bukaan, float sudutDeg, float y, float r)
        {
            if (bukaan == null) return false;
            for (int i = 0; i < bukaan.Length; i++)
            {
                float d = Mathf.Abs(Mathf.DeltaAngle(sudutDeg, bukaan[i].sudut)) * Mathf.Deg2Rad * r;
                if (d < bukaan[i].lebar * 0.5f && y < bukaan[i].tinggi) return true;
            }
            return false;
        }

        // ukuran = (P, T, L). kubah: dinding rendah 36% + kubah bernoise batu kasar; selain itu kotak (bunker) dengan plafon datar.
        public static Mesh Ruang(string nama, Vector3 ukuran, bool kubah, float noise, int seed, Bukaan[] bukaan)
        {
            float a = ukuran.x * 0.5f, b = ukuran.z * 0.5f, n = kubah ? 2.2f : 8f;
            float hDinding = kubah ? ukuran.y * 0.36f : ukuran.y;
            int baris = BarisDinding + (kubah ? BarisKubah : 0);
            var v = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>();
            for (int r = 0; r <= baris; r++)
            {
                float y, skala;
                if (r <= BarisDinding) { y = r == 0 && kubah ? -0.35f : hDinding * r / BarisDinding; skala = 1f; }
                else { float phi = (r - BarisDinding) / (float)BarisKubah * Mathf.PI * 0.5f * 0.94f; y = hDinding + (ukuran.y - hDinding) * Mathf.Sin(phi); skala = Mathf.Cos(phi); }
                for (int s = 0; s <= Segmen; s++)
                {
                    float sud = s * 360f / Segmen;
                    float rr = JariJari(sud, a, b, n) * skala;
                    if (kubah && r > 0)
                    {
                        float crag = (Mathf.PerlinNoise(seed * 0.37f + s * 0.27f, r * 0.39f) - 0.5f) * 1.4f
                                   + (Mathf.PerlinNoise(seed * 0.83f + s * 0.62f, r * 0.91f) - 0.5f) * 0.7f;
                        rr *= 1f + noise * crag;
                        if (r > BarisDinding) y += crag * noise * 2.2f;
                    }
                    float t = sud * Mathf.Deg2Rad;
                    v.Add(new Vector3(Mathf.Sin(t) * rr, y, Mathf.Cos(t) * rr));
                    uv.Add(new Vector2(s * (ukuran.x + ukuran.z) / (Segmen * 2f), Mathf.Max(0f, y) * 0.5f));
                }
            }
            int w = Segmen + 1;
            for (int r = 0; r < baris; r++)
                for (int s = 0; s < Segmen; s++)
                {
                    int v0 = r * w + s, v1 = v0 + 1, v2 = v0 + w, v3 = v2 + 1;
                    float sud = (s + 0.5f) * 360f / Segmen, yTengah = (v[v0].y + v[v2].y) * 0.5f;
                    if (DiBukaan(bukaan, sud, yTengah, JariJari(sud, a, b, n))) continue;
                    tri.Add(v0); tri.Add(v2); tri.Add(v1);
                    tri.Add(v1); tri.Add(v2); tri.Add(v3);
                }
            int pusat = v.Count;
            v.Add(new Vector3(0f, ukuran.y, 0f)); uv.Add(new Vector2(0.5f, 0.5f));
            int atas = baris * w;
            for (int s = 0; s < Segmen; s++) { tri.Add(pusat); tri.Add(atas + s + 1); tri.Add(atas + s); }
            var m = new Mesh { name = nama };
            m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(tri, 0);
            m.RecalculateNormals(); m.RecalculateTangents(); m.RecalculateBounds();
            return m;
        }

        // Lorong gua lengkung batu alami bertekstur noise kasar (tertanam dalam di dinding kubah agar tanpa celah).
        public static Mesh LorongGua(string nama, float panjang, float lebar, float tinggi, int seed)
        {
            const int cincin = 12, seg = 22;
            var v = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>();
            float rx = lebar * 0.60f, ry = tinggi;
            for (int r = 0; r <= cincin; r++)
            {
                float z = -panjang * 0.5f + panjang * r / cincin;
                for (int s = 0; s <= seg; s++)
                {
                    float u = (float)s / seg, sud = u * Mathf.PI * 2f;
                    float xBase = Mathf.Sin(sud) * rx;
                    float cosS = Mathf.Cos(sud);
                    float yBase = cosS < -0.25f ? -0.15f : (cosS + 0.25f) / 1.25f * ry;
                    float disp = yBase < 0.05f ? 1f : 1f + 0.20f * (Mathf.PerlinNoise(seed * 0.23f + r * 0.45f, s * 0.55f) - 0.5f);
                    v.Add(new Vector3(xBase * disp, yBase * (yBase < 0.05f ? 1f : disp), z));
                    uv.Add(new Vector2(u * (lebar + tinggi) * 0.5f, z * 0.4f));
                }
            }
            int w = seg + 1;
            for (int r = 0; r < cincin; r++)
                for (int s = 0; s < seg; s++)
                {
                    int v0 = r * w + s, v1 = v0 + 1, v2 = v0 + w, v3 = v2 + 1;
                    tri.Add(v0); tri.Add(v2); tri.Add(v1); tri.Add(v1); tri.Add(v2); tri.Add(v3);
                }
            var m = new Mesh { name = nama };
            m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(tri, 0);
            m.RecalculateNormals(); m.RecalculateTangents(); m.RecalculateBounds();
            return m;
        }

        // Stalaktit batu berundak & ujung runcing alami.
        public static Mesh Stalaktit(string nama, int seed)
        {
            const int cincin = 7, seg = 11;
            var v = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>();
            for (int r = 0; r <= cincin; r++)
            {
                float t = (float)r / cincin;
                float ledge = 1f + 0.22f * Mathf.Sin(t * Mathf.PI * 3.5f);
                float rad = Mathf.Lerp(0.52f, 0.018f, Mathf.Pow(t, 0.85f)) * ledge;
                for (int s = 0; s <= seg; s++)
                {
                    float a = s * Mathf.PI * 2f / seg;
                    float rib = 0.84f + 0.16f * Mathf.Cos(5f * a + seed);
                    float d = rad * rib * (0.76f + 0.42f * Mathf.PerlinNoise(seed * 0.4f + s * 0.7f, r * 0.9f));
                    v.Add(new Vector3(Mathf.Cos(a) * d, 0.08f - t * 1.15f, Mathf.Sin(a) * d));
                    uv.Add(new Vector2((float)s / seg, t));
                }
            }
            int w = seg + 1;
            for (int r = 0; r < cincin; r++)
                for (int s = 0; s < seg; s++)
                {
                    int v0 = r * w + s, v1 = v0 + 1, v2 = v0 + w, v3 = v2 + 1;
                    tri.Add(v0); tri.Add(v1); tri.Add(v2); tri.Add(v1); tri.Add(v3); tri.Add(v2);
                }
            var m = new Mesh { name = nama };
            m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(tri, 0);
            m.RecalculateNormals(); m.RecalculateTangents(); m.RecalculateBounds();
            return m;
        }

        // Stalagmit lantai gua berprofil batu kapur berlapis.
        public static Mesh Stalagmit(string nama, int seed)
        {
            const int cincin = 7, seg = 11;
            var v = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>();
            for (int r = 0; r <= cincin; r++)
            {
                float t = (float)r / cincin;
                float tier = 1f + 0.18f * Mathf.Sin(t * Mathf.PI * 4.0f);
                float rad = Mathf.Lerp(0.64f, 0.07f, Mathf.Pow(t, 0.82f)) * tier;
                for (int s = 0; s <= seg; s++)
                {
                    float a = s * Mathf.PI * 2f / seg;
                    float rib = 0.86f + 0.14f * Mathf.Sin(5f * a + seed * 0.7f);
                    float d = rad * rib * (0.76f + 0.44f * Mathf.PerlinNoise(seed * 0.43f + s * 0.71f, r * 0.87f));
                    v.Add(new Vector3(Mathf.Cos(a) * d, -0.08f + t * 1.08f, Mathf.Sin(a) * d));
                    uv.Add(new Vector2((float)s / seg, t));
                }
            }
            int w = seg + 1;
            for (int r = 0; r < cincin; r++)
                for (int s = 0; s < seg; s++)
                {
                    int v0 = r * w + s, v1 = v0 + 1, v2 = v0 + w, v3 = v2 + 1;
                    tri.Add(v0); tri.Add(v2); tri.Add(v1); tri.Add(v1); tri.Add(v2); tri.Add(v3);
                }
            int puncak = v.Count;
            v.Add(new Vector3(0f, 1.04f, 0f)); uv.Add(new Vector2(0.5f, 1f));
            int atas = cincin * w;
            for (int s = 0; s < seg; s++) { tri.Add(puncak); tri.Add(atas + s + 1); tri.Add(atas + s); }
            var m = new Mesh { name = nama };
            m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(tri, 0);
            m.RecalculateNormals(); m.RecalculateTangents(); m.RecalculateBounds();
            return m;
        }

        // Rumpun 5 jamur bioluminesensi gua yang halus & bervariasi (submesh 0 = batang ramping melengkung, submesh 1 = payung lonceng bergelombang).
        public static Mesh JamurGua(string nama, int seed)
        {
            var v = new List<Vector3>(); var uv = new List<Vector2>();
            var tBatang = new List<int>(); var tPayung = new List<int>();
            for (int k = 0; k < 5; k++)
            {
                float ang = seed * 0.91f + k * 1.28f + Mathf.PerlinNoise(seed, k * 3.1f) * 0.7f;
                float dist = k == 0 ? 0f : 0.14f + k * 0.06f;
                float bx = Mathf.Cos(ang) * dist, bz = Mathf.Sin(ang) * dist;
                float h = k == 0 ? 0.62f : k == 1 ? 0.48f : k == 2 ? 0.36f : 0.24f + (k % 2) * 0.06f;
                float capR = k == 0 ? 0.25f : k == 1 ? 0.20f : k == 2 ? 0.16f : 0.12f;
                Vector3 condong = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang));

                const int rStem = 5, sSeg = 6;
                int vStart = v.Count, w = sSeg + 1;
                Vector3 tip = Vector3.zero;
                for (int r = 0; r <= rStem; r++)
                {
                    float t = (float)r / rStem;
                    float rad = capR * (0.20f - 0.10f * Mathf.Sin(t * Mathf.PI * 0.85f));
                    Vector3 c = new Vector3(bx, -0.04f + t * (h + 0.04f), bz) + condong * (0.12f * t * t);
                    if (r == rStem) tip = c;
                    for (int s = 0; s <= sSeg; s++)
                    {
                        float a = s * Mathf.PI * 2f / sSeg;
                        v.Add(c + new Vector3(Mathf.Cos(a) * rad, 0f, Mathf.Sin(a) * rad));
                        uv.Add(new Vector2((float)s / sSeg, t));
                    }
                }
                for (int r = 0; r < rStem; r++)
                    for (int s = 0; s < sSeg; s++)
                    {
                        int v0 = vStart + r * w + s, v1 = v0 + 1, v2 = v0 + w, v3 = v2 + 1;
                        tBatang.Add(v0); tBatang.Add(v2); tBatang.Add(v1);
                        tBatang.Add(v1); tBatang.Add(v2); tBatang.Add(v3);
                    }

                const int cSeg = 12;
                float[] pr = { 0.12f, 0.85f, 1.00f, 0.78f, 0.38f, 0.08f };
                float[] py = { -0.14f, -0.12f, -0.02f, 0.24f, 0.42f, 0.50f };
                Quaternion miring = Quaternion.FromToRotation(Vector3.up, (Vector3.up + condong * 0.30f).normalized);
                int cStart = v.Count, cw = cSeg + 1;
                for (int r = 0; r < pr.Length; r++)
                    for (int s = 0; s <= cSeg; s++)
                    {
                        float a = s * Mathf.PI * 2f / cSeg;
                        float ombak = r == 2 ? Mathf.Sin(a * 6f + k) * 0.08f : 0f;
                        float rr = capR * pr[r] * (1f + ombak);
                        Vector3 lp = new Vector3(Mathf.Cos(a) * rr, capR * py[r] + ombak * capR * 0.4f, Mathf.Sin(a) * rr);
                        v.Add(tip + miring * lp);
                        uv.Add(new Vector2(0.5f + Mathf.Cos(a) * pr[r] * 0.5f, 0.5f + Mathf.Sin(a) * pr[r] * 0.5f));
                    }
                for (int r = 0; r < pr.Length - 1; r++)
                    for (int s = 0; s < cSeg; s++)
                    {
                        int v0 = cStart + r * cw + s, v1 = v0 + 1, v2 = v0 + cw, v3 = v2 + 1;
                        tPayung.Add(v0); tPayung.Add(v2); tPayung.Add(v1);
                        tPayung.Add(v1); tPayung.Add(v2); tPayung.Add(v3);
                    }
                int puncakCap = v.Count;
                v.Add(tip + miring * new Vector3(0f, capR * 0.52f, 0f)); uv.Add(new Vector2(0.5f, 0.5f));
                int atasCap = cStart + (pr.Length - 1) * cw;
                for (int s = 0; s < cSeg; s++) { tPayung.Add(puncakCap); tPayung.Add(atasCap + s + 1); tPayung.Add(atasCap + s); }
            }
            var m = new Mesh { name = nama, subMeshCount = 2 };
            m.SetVertices(v); m.SetUVs(0, uv);
            m.SetTriangles(tBatang, 0); m.SetTriangles(tPayung, 1);
            m.RecalculateNormals(); m.RecalculateTangents(); m.RecalculateBounds();
            return m;
        }

        // Bingkai lengkung batu tebal tertanam dalam di dinding gua (tanpa celah miring atau melayang).
        public static Mesh BingkaiMulut(string nama, float lebar, float tinggi, int seed)
        {
            const int segArch = 16, segProf = 10;
            var v = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>();
            float hw = lebar * 0.52f;
            var spine = new Vector3[segArch + 1];
            for (int i = 0; i <= segArch; i++)
            {
                float u = (float)i / segArch;
                if (u < 0.25f)
                    spine[i] = new Vector3(-hw, Mathf.Lerp(-0.55f, tinggi * 0.42f, u / 0.25f), 0f);
                else if (u > 0.75f)
                    spine[i] = new Vector3(hw, Mathf.Lerp(tinggi * 0.42f, -0.55f, (u - 0.75f) / 0.25f), 0f);
                else
                {
                    float ang = Mathf.Lerp(Mathf.PI, 0f, (u - 0.25f) / 0.5f);
                    float yCrown = tinggi * 0.42f + Mathf.Sin(ang) * (tinggi * 0.58f);
                    spine[i] = new Vector3(Mathf.Cos(ang) * hw, yCrown, 0f);
                }
            }
            for (int i = 0; i <= segArch; i++)
            {
                Vector3 T = (spine[Mathf.Min(segArch, i + 1)] - spine[Mathf.Max(0, i - 1)]).normalized;
                Vector3 nIn = Vector3.Cross(T, Vector3.forward).normalized;
                float u = (float)i / segArch;
                for (int k = 0; k <= segProf; k++)
                {
                    float phi = k * Mathf.PI * 2f / segProf;
                    float jagged = 0.72f + 0.56f * Mathf.PerlinNoise(seed * 0.31f + i * 0.85f, k * 0.9f);
                    float rSide = Mathf.Cos(phi) < 0f ? 0.62f * jagged : 2.55f;
                    float rDepth = 2.65f * (0.85f + 0.32f * Mathf.PerlinNoise(seed + k, i * 0.5f));
                    Vector3 off = (-nIn) * (Mathf.Cos(phi) * rSide) + Vector3.forward * (Mathf.Sin(phi) * rDepth);
                    v.Add(spine[i] + off);
                    uv.Add(new Vector2(u * 4f, (float)k / segProf * 2f));
                }
            }
            int w = segProf + 1;
            for (int i = 0; i < segArch; i++)
                for (int k = 0; k < segProf; k++)
                {
                    int v0 = i * w + k, v1 = v0 + 1, v2 = v0 + w, v3 = v2 + 1;
                    tri.Add(v0); tri.Add(v1); tri.Add(v2);
                    tri.Add(v1); tri.Add(v3); tri.Add(v2);
                }
            var m = new Mesh { name = nama };
            m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(tri, 0);
            m.RecalculateNormals(); m.RecalculateTangents(); m.RecalculateBounds();
            return m;
        }

        // Pilar 3 akar beringin raksasa terpilin dengan banir mencengkeram lantai & menjalar ke kubah langit-langit.
        public static Mesh PilarAkar(string nama, float tinggi, float radius, int seed)
        {
            var v = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>();
            const int rings = 14, seg = 14;
            for (int r = 0; r <= rings; r++)
            {
                float t = (float)r / rings, y = -0.35f + t * (tinggi + 0.7f);
                float baseFlare = t < 0.28f ? 1f + 0.90f * Mathf.Pow(1f - t / 0.28f, 1.6f) : 1f;
                float topFlare = t > 0.74f ? 1f + 0.85f * Mathf.Pow((t - 0.74f) / 0.26f, 1.5f) : 1f;
                Vector3 sway = new Vector3(Mathf.Sin(t * 3.1f + seed) * 0.45f, 0f, Mathf.Cos(t * 2.4f + seed * 1.3f) * 0.38f);
                for (int s = 0; s <= seg; s++)
                {
                    float a = s * Mathf.PI * 2f / seg;
                    float ribs = 0.22f * Mathf.Sin(4f * a + t * 5.2f + seed) + 0.24f * Mathf.Max(0f, Mathf.Cos(5f * a + seed * 0.7f)) * Mathf.Pow(Mathf.Clamp01(1f - t * 2.8f), 1.4f);
                    float n = (Mathf.PerlinNoise(seed * 0.5f + s * 0.55f, r * 0.65f) - 0.5f) * 0.28f;
                    float rr = radius * 0.78f * baseFlare * topFlare * (0.88f + ribs + n);
                    v.Add(new Vector3(Mathf.Cos(a) * rr, y, Mathf.Sin(a) * rr) + sway);
                    uv.Add(new Vector2((float)s / seg * 3f, t * tinggi * 0.25f));
                }
            }
            int w = seg + 1;
            for (int r = 0; r < rings; r++)
                for (int s = 0; s < seg; s++)
                {
                    int v0 = r * w + s, v1 = v0 + 1, v2 = v0 + w, v3 = v2 + 1;
                    tri.Add(v0); tri.Add(v2); tri.Add(v1); tri.Add(v1); tri.Add(v2); tri.Add(v3);
                }

            // 3 sulur akar besar membelit batang utama + banir menjalar di lantai & langit-langit.
            for (int k = 0; k < 3; k++)
            {
                float a0 = k * Mathf.PI * 2f / 3f + seed * 0.4f;
                var pts = new Vector3[11];
                for (int i = 0; i < 11; i++)
                {
                    float t = i / 10f;
                    float r = radius * (t < 0.22f ? Mathf.Lerp(2.3f, 0.92f, t / 0.22f) : t > 0.78f ? Mathf.Lerp(0.92f, 2.4f, (t - 0.78f) / 0.22f) : 0.90f);
                    float ang = a0 + t * 4.2f;
                    float y = t < 0.08f ? -0.15f : -0.1f + t * (tinggi + 0.4f);
                    pts[i] = new Vector3(Mathf.Cos(ang) * r, y, Mathf.Sin(ang) * r);
                }
                TambahCabang(v, uv, tri, pts, radius * 0.36f, radius * 0.24f, 6);
            }
            var m = new Mesh { name = nama };
            m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(tri, 0);
            m.RecalculateNormals(); m.RecalculateTangents(); m.RecalculateBounds();
            return m;
        }

        // Tungku batu berundak berukir untuk arena boss.
        public static Mesh TungkuBatu(string nama, int seed)
        {
            const int seg = 8;
            float[] py = { -0.05f, 0.22f, 0.25f, 0.58f, 0.78f, 1.12f, 1.18f, 0.95f };
            float[] pr = { 0.58f, 0.56f, 0.44f, 0.34f, 0.55f, 0.60f, 0.42f, 0.36f };
            var v = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>();
            for (int r = 0; r < py.Length; r++)
                for (int s = 0; s <= seg; s++)
                {
                    float a = (s + 0.5f) * Mathf.PI * 2f / seg;
                    float chip = r < 7 ? 0.94f + 0.12f * Mathf.PerlinNoise(seed * 0.4f + s * 1.1f, r * 0.9f) : 1f;
                    v.Add(new Vector3(Mathf.Cos(a) * pr[r] * chip, py[r], Mathf.Sin(a) * pr[r] * chip));
                    uv.Add(new Vector2((float)s / seg * 2f, py[r]));
                }
            int w = seg + 1;
            for (int r = 0; r < py.Length - 1; r++)
                for (int s = 0; s < seg; s++)
                {
                    int v0 = r * w + s, v1 = v0 + 1, v2 = v0 + w, v3 = v2 + 1;
                    tri.Add(v0); tri.Add(v2); tri.Add(v1); tri.Add(v1); tri.Add(v2); tri.Add(v3);
                }
            int dasarBara = v.Count;
            v.Add(new Vector3(0f, 0.93f, 0f)); uv.Add(new Vector2(0.5f, 0.5f));
            int cincinDalam = (py.Length - 1) * w;
            for (int s = 0; s < seg; s++) { tri.Add(dasarBara); tri.Add(cincinDalam + s + 1); tri.Add(cincinDalam + s); }
            var m = new Mesh { name = nama };
            m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(tri, 0);
            m.RecalculateNormals(); m.RecalculateTangents(); m.RecalculateBounds();
            return m;
        }

        // Lidah api multi-cabang organik untuk obor tungku arena boss.
        public static Mesh ApiTungku(string nama, int seed)
        {
            var v = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>();
            Vector3[] cMain = { new Vector3(0f, -0.05f, 0f), new Vector3(0.05f, 0.30f, -0.02f), new Vector3(-0.06f, 0.68f, 0.05f), new Vector3(0.02f, 1.18f, -0.02f) };
            TambahCabang(v, uv, tri, cMain, 0.30f, 0.02f, 7);
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f + seed * 0.4f;
                Vector3 d = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                Vector3[] pts = { d * 0.14f, d * 0.25f + Vector3.up * 0.26f, d * 0.12f + Vector3.up * 0.56f, d * 0.03f + Vector3.up * (0.76f + (i % 2) * 0.16f) };
                TambahCabang(v, uv, tri, pts, 0.17f, 0.015f, 6);
            }
            var m = new Mesh { name = nama };
            m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(tri, 0);
            m.RecalculateNormals(); m.RecalculateTangents(); m.RecalculateBounds();
            return m;
        }

        // Panggung altar candi batu kuno oktagonal berundak 3 trap.
        public static Mesh AltarPanggung(string nama, float radius, float tinggi, int seed)
        {
            const int seg = 8;
            float[] py = { -0.05f, tinggi * 0.30f, tinggi * 0.33f, tinggi * 0.64f, tinggi * 0.67f, tinggi * 0.96f, tinggi };
            float[] pr = { radius * 1.24f, radius * 1.20f, radius * 1.04f, radius * 1.00f, radius * 0.88f, radius * 0.85f, radius * 0.80f };
            var v = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>();
            for (int r = 0; r < py.Length; r++)
                for (int s = 0; s <= seg; s++)
                {
                    float a = (s + 0.5f) * Mathf.PI * 2f / seg;
                    float chip = 0.94f + 0.12f * Mathf.PerlinNoise(seed * 0.3f + s * 0.9f, r * 0.7f);
                    v.Add(new Vector3(Mathf.Cos(a) * pr[r] * chip, py[r], Mathf.Sin(a) * pr[r] * chip));
                    uv.Add(new Vector2((float)s / seg * 4f, py[r]));
                }
            int w = seg + 1;
            for (int r = 0; r < py.Length - 1; r++)
                for (int s = 0; s < seg; s++)
                {
                    int v0 = r * w + s, v1 = v0 + 1, v2 = v0 + w, v3 = v2 + 1;
                    tri.Add(v0); tri.Add(v2); tri.Add(v1); tri.Add(v1); tri.Add(v2); tri.Add(v3);
                }
            int atasCenter = v.Count;
            v.Add(new Vector3(0f, tinggi, 0f)); uv.Add(new Vector2(0.5f, 0.5f));
            int atasRing = (py.Length - 1) * w;
            for (int s = 0; s < seg; s++) { tri.Add(atasCenter); tri.Add(atasRing + s + 1); tri.Add(atasRing + s); }
            var m = new Mesh { name = nama };
            m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(tri, 0);
            m.RecalculateNormals(); m.RecalculateTangents(); m.RecalculateBounds();
            return m;
        }

        // Kepompong 8 akar berduri menyilang di atas altar Paku Keramat.
        public static Mesh KepompongAkar(string nama, int seed)
        {
            var v = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>();
            for (int i = 0; i < 8; i++)
            {
                float a0 = i * Mathf.PI / 4f + seed * 0.2f;
                var pts = new Vector3[7];
                for (int k = 0; k < 7; k++)
                {
                    float t = k / 6f;
                    float r = Mathf.Sin(t * Mathf.PI * 0.85f + 0.15f) * 1.15f * (1f - t * 0.42f);
                    float ang = a0 + t * (i % 2 == 0 ? 1.45f : -1.25f);
                    pts[k] = new Vector3(Mathf.Cos(ang) * r, -0.05f + t * 2.75f, Mathf.Sin(ang) * r);
                }
                TambahCabang(v, uv, tri, pts, 0.24f, 0.08f, 6);
            }
            var m = new Mesh { name = nama };
            m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(tri, 0);
            m.RecalculateNormals(); m.RecalculateTangents(); m.RecalculateBounds();
            return m;
        }

        // Dinding akar beringin kusut tak beraturan.
        public static Mesh DindingAkar(string nama, float lebar, float tinggi, int seed)
        {
            var v = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>();
            for (int i = 0; i < 7; i++)
            {
                float t = i / 6f;
                float x = Mathf.Lerp(-lebar * 0.44f, lebar * 0.44f, t);
                var pts = new Vector3[5];
                for (int k = 0; k < 5; k++)
                {
                    float h = k / 4f;
                    float n1 = (Mathf.PerlinNoise(seed + i * 2.1f, h * 3f) - 0.5f) * 1.1f;
                    float n2 = (Mathf.PerlinNoise(seed * 1.7f + h * 3f, i * 1.3f) - 0.5f) * 0.7f;
                    pts[k] = new Vector3(x + n1 * Mathf.Sin(h * Mathf.PI), -0.1f + h * (tinggi + 0.3f), n2);
                }
                TambahCabang(v, uv, tri, pts, 0.52f, 0.24f, 6);
            }
            for (int i = 0; i < 4; i++)
            {
                float yA = tinggi * (0.18f + i * 0.20f), yB = tinggi * (0.35f + (i % 2) * 0.35f);
                float dir = i % 2 == 0 ? 1f : -1f;
                Vector3[] cross = {
                    new Vector3(-lebar * 0.46f * dir, yA, -0.3f),
                    new Vector3(-lebar * 0.15f * dir, (yA + yB) * 0.5f + 0.3f, 0.35f),
                    new Vector3(lebar * 0.15f * dir, (yA + yB) * 0.5f - 0.2f, -0.25f),
                    new Vector3(lebar * 0.46f * dir, yB, 0.25f)
                };
                TambahCabang(v, uv, tri, cross, 0.26f, 0.18f, 6);
            }
            var m = new Mesh { name = nama };
            m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(tri, 0);
            m.RecalculateNormals(); m.RecalculateTangents(); m.RecalculateBounds();
            return m;
        }

        static void TambahCabang(List<Vector3> v, List<Vector2> uv, List<int> tri, Vector3[] pts, float rStart, float rEnd, int seg)
        {
            int vStart = v.Count, w = seg + 1;
            Vector3 upRef = Vector3.up;
            for (int i = 0; i < pts.Length; i++)
            {
                Vector3 tan = (pts[Mathf.Min(pts.Length - 1, i + 1)] - pts[Mathf.Max(0, i - 1)]).normalized;
                if (tan.sqrMagnitude < 0.001f) tan = Vector3.up;
                if (Mathf.Abs(Vector3.Dot(tan, upRef)) > 0.92f) upRef = Vector3.forward;
                Vector3 right = Vector3.Cross(upRef, tan).normalized;
                Vector3 binorm = Vector3.Cross(tan, right).normalized;
                upRef = binorm;
                float t = (float)i / Mathf.Max(1, pts.Length - 1);
                float r = Mathf.Lerp(rStart, rEnd, t);
                for (int s = 0; s <= seg; s++)
                {
                    float a = s * Mathf.PI * 2f / seg;
                    v.Add(pts[i] + (right * Mathf.Cos(a) + binorm * Mathf.Sin(a)) * r);
                    uv.Add(new Vector2((float)s / seg, t * 2f));
                }
            }
            for (int i = 0; i < pts.Length - 1; i++)
                for (int s = 0; s < seg; s++)
                {
                    int v0 = vStart + i * w + s, v1 = v0 + 1, v2 = v0 + w, v3 = v2 + 1;
                    tri.Add(v0); tri.Add(v1); tri.Add(v2); tri.Add(v1); tri.Add(v3); tri.Add(v2);
                }
        }
    }
}
