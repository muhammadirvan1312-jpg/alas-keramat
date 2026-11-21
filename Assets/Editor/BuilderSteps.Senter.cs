using System.IO;
using UnityEditor;
using UnityEngine;
namespace AlasKeramat.EditorTools
{
    // Langkah 7z4: cookie senter prosedural (hotspot, cincin reflektor, debu lensa) untuk SenterAndi.
    public static partial class NusantaraMasterBuilder
    {
        public const string PathCookieSenter = FolderTex + "/cookie_senter.png";

        public static float NilaiCookie(float r, float sudut, float noise)
        {
            if (r >= 1f) return 0f;
            float pusat = Mathf.Exp(-r * r * 9f);
            float cincin = 0.22f * Mathf.Exp(-Mathf.Pow((r - 0.55f) * 9f, 2f)) + 0.12f * Mathf.Exp(-Mathf.Pow((r - 0.8f) * 12f, 2f));
            float e = Mathf.InverseLerp(0.8f, 1f, r), tepi = 1f - e * e * (3f - 2f * e);
            float v = (0.38f + 0.62f * pusat + cincin) * tepi * (0.93f + 0.07f * noise) * (1f + 0.03f * Mathf.Sin(sudut * 7f));
            return Mathf.Clamp01(v);
        }

        static void LangkahCookieSenter()
        {
            const int n = 256;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float u = (x + 0.5f) / n * 2f - 1f, v = (y + 0.5f) / n * 2f - 1f;
                    float c = NilaiCookie(Mathf.Sqrt(u * u + v * v), Mathf.Atan2(v, u), Mathf.PerlinNoise(x * 0.08f, y * 0.08f));
                    px[y * n + x] = new Color(c, c, c, c);
                }
            t.SetPixels(px); t.Apply();
            File.WriteAllBytes(PathCookieSenter, t.EncodeToPNG()); Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(PathCookieSenter);
            var imp = (TextureImporter)AssetImporter.GetAtPath(PathCookieSenter);
            imp.textureType = TextureImporterType.Cookie; imp.wrapMode = TextureWrapMode.Clamp; imp.mipmapEnabled = false; imp.SaveAndReimport();
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(PathCookieSenter);
            int dipasang = 0;
            foreach (var s in Object.FindObjectsByType<AlasKeramat.Player.SenterAndi>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            { s.PasangCookie(tex); EditorUtility.SetDirty(s); dipasang++; }
            Catat("Senter: cookie reflektor 256px terpasang di " + dipasang + " senter");
        }
    }
}
