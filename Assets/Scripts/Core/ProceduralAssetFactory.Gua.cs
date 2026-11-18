using System.Collections.Generic;
using UnityEngine;
namespace AlasKeramat.Core
{
    // Mesh ruang bawah tanah (03 §10): dinding superelips + kubah (gua) atau plafon datar (bunker), normal menghadap ke dalam,
    // dengan celah bukaan untuk lorong. Lantai dibuat terpisah (balok ber-collider) oleh Builder.
    public static class ProcGua
    {
        public const int Segmen = 40, BarisDinding = 4, BarisKubah = 7;
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

        // ukuran = (P, T, L). kubah: dinding rendah 35% + kubah bernoise; selain itu kotak (bunker) dengan plafon datar.
        public static Mesh Ruang(string nama, Vector3 ukuran, bool kubah, float noise, int seed, Bukaan[] bukaan)
        {
            float a = ukuran.x * 0.5f, b = ukuran.z * 0.5f, n = kubah ? 2.2f : 8f;
            float hDinding = kubah ? ukuran.y * 0.35f : ukuran.y;
            int baris = BarisDinding + (kubah ? BarisKubah : 0);
            var v = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>();
            for (int r = 0; r <= baris; r++)
            {
                float y, skala;
                if (r <= BarisDinding) { y = hDinding * r / BarisDinding; skala = 1f; }
                else { float phi = (r - BarisDinding) / (float)BarisKubah * Mathf.PI * 0.5f * 0.92f; y = hDinding + (ukuran.y - hDinding) * Mathf.Sin(phi); skala = Mathf.Cos(phi); }
                for (int s = 0; s <= Segmen; s++)
                {
                    float sud = s * 360f / Segmen;
                    float rr = JariJari(sud, a, b, n) * skala;
                    if (kubah && r > 0) rr *= 1f + noise * (Mathf.PerlinNoise(seed * 0.37f + s * 0.31f, r * 0.43f) - 0.5f) * 2f;
                    float t = sud * Mathf.Deg2Rad;
                    v.Add(new Vector3(Mathf.Sin(t) * rr, y, Mathf.Cos(t) * rr));
                    uv.Add(new Vector2(s * (ukuran.x + ukuran.z) / (Segmen * 2f), y * 0.5f));
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
            m.RecalculateNormals(); m.RecalculateBounds();
            return m;
        }
    }
}
