using System.IO;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
namespace AlasKeramat.EditorTools
{
    // Langkah 7z3: partikel alam (daun gugur, debu berkilau) dengan tekstur prosedural kecil.
    public static partial class NusantaraMasterBuilder
    {
        static Texture2D TeksturPartikel(string nama, bool isDaun)
        {
            int n = isDaun ? 64 : 32;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, true);
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float u = (x + 0.5f) / n * 2f - 1f, v = (y + 0.5f) / n * 2f - 1f;
                    float a;
                    Color c = Color.white;
                    if (isDaun)
                    {
                        // Daun lonjong runcing miring 35 derajat dengan tulang tengah.
                        float cs = 0.819f, sn = 0.574f, ru = u * cs - v * sn, rv = u * sn + v * cs;
                        float lebar = 0.42f * Mathf.Sqrt(Mathf.Max(0f, 1f - rv * rv)) * (1f - 0.25f * rv);
                        float d = Mathf.Abs(ru) - lebar;
                        a = Mathf.Clamp01(-d * n * 0.25f) * (Mathf.Abs(rv) < 0.95f ? 1f : 0f);
                        float tulang = Mathf.Clamp01(1f - Mathf.Abs(ru) * n * 0.35f);
                        float g = 0.82f + 0.18f * Mathf.PerlinNoise(x * 0.3f, y * 0.3f);
                        c = new Color(g, g, g * 0.95f) * (1f - 0.35f * tulang);
                    }
                    else
                    {
                        float r = Mathf.Sqrt(u * u + v * v);
                        a = Mathf.Pow(Mathf.Clamp01(1f - r), 2.2f);
                    }
                    c.a = a; px[y * n + x] = c;
                }
            t.SetPixels(px); t.Apply();
            string p = FolderTex + "/" + nama + ".png";
            File.WriteAllBytes(p, t.EncodeToPNG()); Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(p);
            var imp = (TextureImporter)AssetImporter.GetAtPath(p);
            imp.alphaIsTransparency = true; imp.wrapMode = TextureWrapMode.Clamp; imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(p);
        }

        static Material MaterialPartikel(string nama, Texture2D tex)
        {
            var sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (sh == null || tex == null) return null;
            var m = new Material(sh) { name = nama };
            SafeMaterialFactory.SetF(m, "_Surface", 1f); SafeMaterialFactory.SetF(m, "_Blend", 0f);
            SafeMaterialFactory.SetF(m, "_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            SafeMaterialFactory.SetF(m, "_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            SafeMaterialFactory.SetF(m, "_ZWrite", 0f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.renderQueue = 3000;
            SafeMaterialFactory.SetT(m, "_BaseMap", tex);
            SafeMaterialFactory.SetC(m, "_BaseColor", Color.white);
            string p = FolderMat + "/" + nama + ".mat";
            AssetDatabase.CreateAsset(m, p);
            return AssetDatabase.LoadAssetAtPath<Material>(p);
        }

        static void LangkahPartikelAlam()
        {
            var md = MaterialPartikel("M_DaunGugur", TeksturPartikel("partikel_daun", true));
            var mb = MaterialPartikel("M_DebuCahaya", TeksturPartikel("partikel_debu", false));
            if (md == null || mb == null) { Debug.LogWarning("[AK][Partikel] Shader partikel URP tidak ada, dilewati"); Catat("Partikel alam: dilewati"); return; }
            var go = Anak(_sistem, "PartikelAlam");
            var pa = go.GetComponent<PartikelAlam>() ?? go.AddComponent<PartikelAlam>();
            pa.Pasang(md, mb); EditorUtility.SetDirty(pa);
            Catat("Partikel alam: daun gugur di hutan siang, debu berkilau saat cerah");
        }
    }
}
