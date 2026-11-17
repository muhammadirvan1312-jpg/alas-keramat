using UnityEngine;

namespace AlasKeramat.Core
{
    // Fungsi dunia deterministik (docs/gdd/03 §2–5, diskalakan v5 docs/gdd/20 §2: koordinat x2, tinggi x1,5).
    public static partial class WorldTerrainGenerator
    {
        public const float SkalaXZ = 2f, SkalaTinggi = 1.5f;
        public const float BatasDunia = 1000f;
        public const float UkuranTile = 500f;
        public const int TileSisi = 4;
        public const float DasarY = -30f, TinggiMaks = 300f;
        public const float TinggiPabrik = 14f;

        // ---------- Helper (03 §2) ----------
        static float Perlin(float x, float z) { return Mathf.PerlinNoise(x + 1000.37f, z + 1000.71f); }
        public static float S(float a, float b, float v) { float t = Mathf.Clamp01((v - a) / (b - a)); return t * t * (3f - 2f * t); }
        static float Fbm(float x, float z, int oct)
        {
            float s = 0f, amp = 1f, f = 1f, n = 0f;
            for (int i = 0; i < oct; i++) { s += amp * Perlin(x * f, z * f); n += amp; amp *= 0.5f; f *= 2f; }
            return s / n;
        }
        static float Ridged(float x, float z) { float r = 1f - Mathf.Abs(2f * Perlin(x, z) - 1f); return r * r; }
        static float Gauss(float d, float mu, float sigma) { float k = (d - mu) / sigma; return Mathf.Exp(-k * k); }

        // ---------- Data pad (03 §3, urutan WAJIB) ----------
        struct Pad { public float cx, cz, rIn, rOut, h; public Pad(float a, float b, float c, float d, float e) { cx = a; cz = b; rIn = c; rOut = d; h = e; } }
        static readonly Pad[] Pads =
        {
            new Pad(0f, -220f, 18f, 40f, 7.0f), new Pad(28f, -188f, 12f, 24f, 7.6f), new Pad(270f, 98f, 35f, 58f, 10.0f),
            new Pad(-265f, 125f, 60f, 100f, 3.2f), new Pad(-280f, 145f, 8f, 14f, 3.4f), new Pad(140f, -130f, 6f, 12f, 5.0f),
            new Pad(0f, 345f, 6f, 12f, 51.0f), new Pad(0f, -420f, 25f, 45f, 1.6f), new Pad(-95f, -438f, 10f, 18f, 6.0f),
            new Pad(-300f, 250f, 30f, 50f, 14.0f), new Pad(318f, 58f, 15f, 25f, 11.0f),
        };

        // ---------- Sungai (03 §4): x, z, wl, w, dalam ----------
        public struct TitikSungai { public float x, z, wl, w, dalam; public TitikSungai(float a, float b, float c, float d, float e) { x = a; z = b; wl = c; w = d; dalam = e; } }
        public static readonly TitikSungai[] SungaiUtama =
        {
            new TitikSungai(90f, 270f, 13.0f, 10f, 2.0f), new TitikSungai(150f, 200f, 11.0f, 9f, 1.6f), new TitikSungai(205f, 140f, 9.0f, 10f, 1.6f),
            new TitikSungai(232f, 80f, 7.5f, 11f, 1.8f), new TitikSungai(205f, -10f, 6.0f, 12f, 1.6f), new TitikSungai(160f, -120f, 4.0f, 12f, 1.6f),
            new TitikSungai(170f, -250f, 2.2f, 14f, 1.8f), new TitikSungai(225f, -390f, 0.6f, 16f, 2.0f), new TitikSungai(255f, -500f, 0.0f, 18f, 2.5f),
        };
        public static readonly TitikSungai[] AnakSungaiHeli =
        {
            new TitikSungai(-75f, -175f, 9.0f, 3f, 0.5f), new TitikSungai(-30f, -232f, 6.2f, 4f, 0.5f), new TitikSungai(10f, -238f, 5.9f, 4f, 0.5f),
            new TitikSungai(60f, -255f, 4.5f, 4f, 0.5f), new TitikSungai(110f, -262f, 3.2f, 5f, 0.5f), new TitikSungai(170f, -250f, 2.2f, 5f, 0.5f),
        };
        public static readonly TitikSungai[][] SemuaSungai = { SungaiUtama, AnakSungaiHeli };

        // Jarak ke polyline (koordinat 03) + parameter interpolasi.
        public static float JarakSungai(TitikSungai[] p, float x, float z, out float wl, out float w, out float dalam)
        {
            float best = float.MaxValue; wl = 0f; w = 0f; dalam = 0f;
            for (int i = 0; i < p.Length - 1; i++)
            {
                var a = p[i]; var b = p[i + 1];
                float dx = b.x - a.x, dz = b.z - a.z;
                float t = Mathf.Clamp01(((x - a.x) * dx + (z - a.z) * dz) / (dx * dx + dz * dz));
                float px = a.x + dx * t - x, pz = a.z + dz * t - z;
                float d = Mathf.Sqrt(px * px + pz * pz);
                if (d < best) { best = d; wl = Mathf.Lerp(a.wl, b.wl, t); w = Mathf.Lerp(a.w, b.w, t); dalam = Mathf.Lerp(a.dalam, b.dalam, t); }
            }
            return best;
        }

        // Mask daratan (03 §2 langkah 1) dalam koordinat 03.
        static float Land03(float x, float z)
        {
            float rc = Mathf.Sqrt(x * x + z * z) / 500f;
            float rb = Mathf.Max(Mathf.Abs(x), Mathf.Abs(z)) / 500f;
            float r = Mathf.Lerp(rc, rb, 0.35f) + (Fbm(x * 0.006f + 11f, z * 0.006f + 5f, 3) - 0.5f) * 0.10f;
            return 1f - S(0.82f, 0.97f, r);
        }

        static float TPlateau03(float x, float z, float land)
        {
            float trail = 1f - S(15f, 35f, Mathf.Abs(x + 175f));
            float cliffW = Mathf.Lerp(15f, 80f, trail);
            return S(300f - cliffW, 300f, z) * (1f - S(230f, 300f, Mathf.Abs(x))) * land;
        }

        // Rumus asli 03 §2 (koordinat & tinggi 1 km).
        public static float SampleHeight03(float x, float z)
        {
            float land = Land03(x, z);
            float h = Mathf.Lerp(-8f, 12f, land);
            float hills = (Fbm(x * 0.0045f, z * 0.0045f, 5) - 0.45f) * 22f;
            float ridge = Ridged(x * 0.0018f + 3.1f, z * 0.0018f + 7.7f) * 9f;
            h += (hills + ridge * 0.7f) * land;
            float beachTarget = Mathf.Lerp(1.8f, -1.5f, S(-430f, -475f, z));
            h = Mathf.Lerp(h, beachTarget, S(-350f, -410f, z));
            float tPlateau = TPlateau03(x, z, land);
            float plateauH = 50f + (Fbm(x * 0.01f, z * 0.01f, 3) - 0.5f) * 8f;
            h = Mathf.Lerp(h, plateauH, tPlateau);
            float chasm = (1f - S(4f, 7f, Mathf.Abs(z - 324f))) * (1f - S(130f, 150f, Mathf.Abs(x)));
            h -= 38f * chasm * tPlateau;
            float d0 = Mathf.Sqrt(x * x + z * z);
            h = Mathf.Lerp(6.0f, h, S(25f, 62f, d0));
            h += 9f * Gauss(d0, 70f, 14f);
            for (int i = 0; i < Pads.Length; i++)
            {
                var p = Pads[i];
                float dx = x - p.cx, dz = z - p.cz;
                h = Mathf.Lerp(p.h, h, S(p.rIn, p.rOut, Mathf.Sqrt(dx * dx + dz * dz)));
            }
            float dSwx = x + 235f, dSwz = z - 95f;
            h = Mathf.Lerp(1.4f, h, S(40f, 65f, Mathf.Sqrt(dSwx * dSwx + dSwz * dSwz)));
            for (int s = 0; s < SemuaSungai.Length; s++)
            {
                float wl, w, dalam;
                float dr = JarakSungai(SemuaSungai[s], x, z, out wl, out w, out dalam);
                if (dr < w * 0.5f + 14f) h = Mathf.Max(h, Mathf.Lerp(wl + 0.5f, h, S(w * 0.5f, w * 0.5f + 14f, dr)));
                float bed = wl - dalam;
                h = Mathf.Lerp(bed, h, S(w * 0.5f - 2f, w * 0.5f + 1.5f, dr));
            }
            return h;
        }

        // Ketinggian dunia v5 (koordinat dunia 2 km).
        public static float SampleHeight(float x, float z)
        {
            float x3 = x / SkalaXZ, z3 = z / SkalaXZ;
            float h = SkalaTinggi * SampleHeight03(x3, z3);
            float land = Land03(x3, z3);
            if (land > 0.001f) h += FiturV5(x, z) * land * ZonaBebasSungai(x3, z3);
            // Z9 dataran pabrik gula: diratakan ke 14 m (pad v5, setelah fitur).
            h = Mathf.Lerp(TinggiPabrik, h, S(120f, 200f, Dist(x, z, 660f, -500f)));
            return h;
        }

        // Fitur zona baru (20 §2.2). Dijauhkan dari sungai agar tepi air tetap benar.
        static float FiturV5(float x, float z)
        {
            float f = 0f;
            float d8 = Dist(x, z, -650f, -560f);
            f -= 6f * Gauss(d8, 0f, 160f) * (1f - S(220f, 300f, d8));                               // Z8 rawa bakau
            float d10 = Dist(x, z, -600f, 660f);
            f += 150f * Gauss(d10, 0f, 170f) * (1f - S(220f, 320f, d10)) * (0.8f + 0.4f * Fbm(x * 0.01f, z * 0.01f, 4)); // Z10 gunung kapur
            f += 8f * (1f - S(150f, 300f, Dist(x, z, 620f, 620f)));                                // Z11 landai kampung
            float wing = 1f - S(150f, 260f, Dist(x, z, -760f, -40f));
            f += (Fbm(x * 0.02f + 40f, z * 0.02f + 9f, 4) - 0.5f) * 24f * wing;                   // Z12 Alas Wingit
            return f;
        }

        static float ZonaBebasSungai(float x3, float z3)
        {
            float m = 1f;
            for (int s = 0; s < SemuaSungai.Length; s++)
            {
                float wl, w, dalam;
                float dr = JarakSungai(SemuaSungai[s], x3, z3, out wl, out w, out dalam);
                m = Mathf.Min(m, S(w * 0.5f + 10f, w * 0.5f + 40f, dr));
            }
            return m;
        }

        static float Dist(float x, float z, float cx, float cz) { float dx = x - cx, dz = z - cz; return Mathf.Sqrt(dx * dx + dz * dz); }

        public static Vector3 SampleNormal(float x, float z)
        {
            const float E = 1f;
            float hl = SampleHeight(x - E, z), hr = SampleHeight(x + E, z), hd = SampleHeight(x, z - E), hu = SampleHeight(x, z + E);
            return new Vector3(hl - hr, 2f * E, hd - hu).normalized;
        }

        public static float SampleSlopeDeg(float x, float z) { return Vector3.Angle(SampleNormal(x, z), Vector3.up); }

        // Permukaan air sungai di titik (koordinat dunia), atau null bila bukan sungai.
        public static float? GetWaterLevelAt(float x, float z)
        {
            float x3 = x / SkalaXZ, z3 = z / SkalaXZ;
            for (int s = 0; s < SemuaSungai.Length; s++)
            {
                float wl, w, dalam;
                float dr = JarakSungai(SemuaSungai[s], x3, z3, out wl, out w, out dalam);
                if (dr <= w * 0.5f + 0.5f) return wl * SkalaTinggi;
            }
            if (SampleHeight(x, z) < 0f) return 0f;
            return null;
        }

        public static bool IsUnderwater(Vector3 pos)
        {
            float? wl = GetWaterLevelAt(pos.x, pos.z);
            return wl.HasValue && pos.y < wl.Value;
        }
    }
}
