using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.Core
{
    public enum SurfaceMode { Opaque, Cutout, Transparent }

    // Pintu masuk umum pabrik aset prosedural.
    public static class ProceduralAssetFactory
    {
        static Font _safeFont;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatis() { _safeFont = null; }

        // H4: font aman untuk UI.Text.
        public static Font GetSafeFont()
        {
            if (_safeFont != null) return _safeFont;
            _safeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_safeFont == null) _safeFont = Font.CreateDynamicFontFromOSFont(new[] { "Roboto", "Noto Sans", "Arial" }, 32);
            return _safeFont;
        }
    }

    // H5: satu-satunya cara membuat material.
    public static class SafeMaterialFactory
    {
        static Shader _lit;
        static readonly Dictionary<string, Material> _cache = new Dictionary<string, Material>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatis() { _lit = null; _cache.Clear(); }

        public static Shader LitShader()
        {
            if (_lit == null) _lit = Shader.Find("Universal Render Pipeline/Lit");
            if (_lit == null) _lit = Shader.Find("Standard");
            return _lit;
        }

        public static Material CreatePBR(string name, Color albedo, float metallic, float smoothness,
            Texture2D mainTex = null, Texture2D bumpTex = null, Color? emission = null,
            SurfaceMode mode = SurfaceMode.Opaque, float cutoff = 0.45f)
        {
            var m = new Material(LitShader());
            m.name = name;
            SetC(m, "_BaseColor", albedo); SetC(m, "_Color", albedo);
            SetF(m, "_Metallic", metallic); SetF(m, "_Smoothness", smoothness); SetF(m, "_Glossiness", smoothness);
            if (mainTex != null) { SetT(m, "_BaseMap", mainTex); SetT(m, "_MainTex", mainTex); }
            if (bumpTex != null && m.HasProperty("_BumpMap")) { m.SetTexture("_BumpMap", bumpTex); m.EnableKeyword("_NORMALMAP"); }
            if (emission.HasValue && emission.Value.maxColorComponent > 0.01f)
            {
                m.EnableKeyword("_EMISSION"); SetC(m, "_EmissionColor", emission.Value);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }
            if (mode == SurfaceMode.Cutout)
            {
                SetF(m, "_AlphaClip", 1f); SetF(m, "_Cutoff", cutoff); SetF(m, "_Cull", 0f);
                m.EnableKeyword("_ALPHATEST_ON"); m.renderQueue = 2450;
            }
            else if (mode == SurfaceMode.Transparent)
            {
                SetF(m, "_Surface", 1f); SetF(m, "_Blend", 0f); SetF(m, "_ZWrite", 0f);
                SetF(m, "_SrcBlend", (float)BlendMode.SrcAlpha);
                SetF(m, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.SetOverrideTag("RenderType", "Transparent");
                m.renderQueue = 3000;
            }
            m.enableInstancing = true;
            return m;
        }

        // Material runtime ber-cache (warna/efek sesaat).
        public static Material GetCached(string key, Color albedo, float metallic = 0f, float smoothness = 0.3f)
        {
            Material m;
            if (_cache.TryGetValue(key, out m) && m != null) return m;
            m = CreatePBR(key, albedo, metallic, smoothness);
            _cache[key] = m;
            return m;
        }

        public static void SetC(Material m, string p, Color c) { if (m.HasProperty(p)) m.SetColor(p, c); }
        public static void SetF(Material m, string p, float f) { if (m.HasProperty(p)) m.SetFloat(p, f); }
        public static void SetT(Material m, string p, Texture t) { if (m.HasProperty(p)) m.SetTexture(p, t); }
    }

    // Tekstur prosedural: noise periodik (tileable), fBm, normal dari ketinggian.
    public static class ProcTex
    {
        public const int UkuranDefault = 256;
        public const float KilauAlbedo = 0.12f;

        static float Hash(int x, int y, int seed)
        {
            uint h = (uint)(x * 374761393 + y * 668265263 + seed * 144665);
            h = (h ^ (h >> 13)) * 1274126177u;
            h ^= h >> 16;
            return (h & 0xFFFFFF) / 16777215f;
        }

        static float Smooth(float t) { return t * t * (3f - 2f * t); }

        // Value noise periodik dengan periode p (sel).
        public static float ValueNoise(float x, float y, int p, int seed)
        {
            int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
            float fx = Smooth(x - x0), fy = Smooth(y - y0);
            int ax = ((x0 % p) + p) % p, ay = ((y0 % p) + p) % p;
            int bx = (ax + 1) % p, by = (ay + 1) % p;
            float a = Mathf.Lerp(Hash(ax, ay, seed), Hash(bx, ay, seed), fx);
            float b = Mathf.Lerp(Hash(ax, by, seed), Hash(bx, by, seed), fx);
            return Mathf.Lerp(a, b, fy);
        }

        // fBm tileable di [0,1]; u,v dalam [0,1).
        public static float Fbm(float u, float v, int basePeriod, int oktaf, int seed, float persistence = 0.5f)
        {
            float jumlah = 0f, amp = 1f, total = 0f;
            int p = basePeriod;
            for (int o = 0; o < oktaf; o++)
            {
                jumlah += ValueNoise(u * p, v * p, p, seed + o * 31) * amp;
                total += amp; amp *= persistence; p *= 2;
            }
            return jumlah / total;
        }

        public static float Ridged(float u, float v, int basePeriod, int oktaf, int seed)
        {
            float n = Fbm(u, v, basePeriod, oktaf, seed);
            return 1f - Mathf.Abs(n * 2f - 1f);
        }

        // Peta tinggi tileable dari fungsi (u,v) -> [0,1].
        public static float[] HeightMap(int size, Func<float, float, float> f)
        {
            var h = new float[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    h[y * size + x] = f((float)x / size, (float)y / size);
            return h;
        }

        public static Texture2D FromHeight(string name, int size, float[] h, Gradient g)
        {
            var t = new Texture2D(size, size, TextureFormat.RGBA32, true);
            t.name = name;
            var px = new Color[size * size];
            // Alpha = smoothness bagi shader Terrain/Lit URP (tanpa mask map). Alpha 1 membuat tanah mengilap seperti cermin.
            for (int i = 0; i < px.Length; i++) { var c = g.Evaluate(Mathf.Clamp01(h[i])); c.a = KilauAlbedo * (0.6f + 0.8f * h[i]); px[i] = c; }
            t.SetPixels(px);
            t.Apply(true);
            t.wrapMode = TextureWrapMode.Repeat;
            return t;
        }

        // Normal map tangent-space dari ketinggian (tileable, beda pusat).
        public static Texture2D NormalFromHeight(string name, int size, float[] h, float kuat)
        {
            var t = new Texture2D(size, size, TextureFormat.RGBA32, true, true);
            t.name = name;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float l = h[y * size + (x - 1 + size) % size], r = h[y * size + (x + 1) % size];
                    float d = h[((y - 1 + size) % size) * size + x], u = h[((y + 1) % size) * size + x];
                    var n = new Vector3((l - r) * kuat, (d - u) * kuat, 1f).normalized;
                    px[y * size + x] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
                }
            t.SetPixels(px);
            t.Apply(true);
            t.wrapMode = TextureWrapMode.Repeat;
            return t;
        }

        public static Gradient Palet(params Color[] warna)
        {
            var g = new Gradient();
            var ck = new GradientColorKey[warna.Length];
            for (int i = 0; i < warna.Length; i++) ck[i] = new GradientColorKey(warna[i], warna.Length == 1 ? 0f : (float)i / (warna.Length - 1));
            g.SetKeys(ck, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return g;
        }

        public static Color Hex(string hex)
        {
            Color c;
            return ColorUtility.TryParseHtmlString(hex, out c) ? c : Color.magenta;
        }
    }

    // Mesh prosedural dasar.
    public static class ProcMesh
    {
        // Bidang datar XZ berpusat di asal, UV diulang 'ulangUV' kali.
        public static Mesh Grid(string name, float ukuran, int segmen, float ulangUV)
        {
            int n = segmen + 1;
            var v = new Vector3[n * n];
            var uv = new Vector2[n * n];
            var tri = new int[segmen * segmen * 6];
            float half = ukuran * 0.5f;
            for (int z = 0; z < n; z++)
                for (int x = 0; x < n; x++)
                {
                    float fx = (float)x / segmen, fz = (float)z / segmen;
                    v[z * n + x] = new Vector3(fx * ukuran - half, 0f, fz * ukuran - half);
                    uv[z * n + x] = new Vector2(fx * ulangUV, fz * ulangUV);
                }
            int k = 0;
            for (int z = 0; z < segmen; z++)
                for (int x = 0; x < segmen; x++)
                {
                    int i = z * n + x;
                    tri[k++] = i; tri[k++] = i + n; tri[k++] = i + 1;
                    tri[k++] = i + 1; tri[k++] = i + n; tri[k++] = i + n + 1;
                }
            var m = new Mesh { name = name };
            m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(tri, 0);
            m.RecalculateNormals(); m.RecalculateTangents(); m.RecalculateBounds();
            return m;
        }
    }
}
