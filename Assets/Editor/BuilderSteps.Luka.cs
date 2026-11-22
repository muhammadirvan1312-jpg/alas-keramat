using System.IO;
using AlasKeramat.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace AlasKeramat.EditorTools
{
    // Langkah 7z5: vinyet merah & busur arah serangan di HUD (sprite prosedural).
    public static partial class NusantaraMasterBuilder
    {
        static Sprite SpriteLuka(string nama, bool busur)
        {
            EnsureFolder(FolderIkon);
            string p = FolderIkon + "/" + nama + ".png";
            int n = busur ? 256 : 128;
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float u = (x + 0.5f) / n * 2f - 1f, v = (y + 0.5f) / n * 2f - 1f, r = Mathf.Sqrt(u * u + v * v), a;
                    if (busur)
                    {
                        // Busur di atas pusat (arah depan), menipis ke samping.
                        float sudut = Mathf.Abs(Mathf.Atan2(u, v)) * Mathf.Rad2Deg;
                        a = Mathf.Clamp01(1f - Mathf.Abs(r - 0.86f) / 0.07f) * Mathf.Clamp01(1f - sudut / 32f);
                    }
                    else
                    {
                        float tepi = Mathf.Max(Mathf.Abs(u), Mathf.Abs(v)) * 0.55f + r * 0.45f;
                        a = Mathf.Pow(Mathf.Clamp01((tepi - 0.45f) / 0.6f), 1.6f);
                    }
                    px[y * n + x] = new Color(1f, 1f, 1f, a);
                }
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false); t.SetPixels(px); t.Apply();
            File.WriteAllBytes(p, t.EncodeToPNG()); Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(p);
            var imp = (TextureImporter)AssetImporter.GetAtPath(p);
            imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Single;
            imp.alphaIsTransparency = true; imp.mipmapEnabled = false;
            imp.wrapMode = TextureWrapMode.Clamp; imp.textureCompression = TextureImporterCompression.Uncompressed; imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(p);
        }

        static void LangkahIndikatorLuka()
        {
            var kanvas = _ui != null ? _ui.transform.Find("Canvas_HUD") : null;
            if (kanvas == null) { Debug.LogWarning("[AK][Builder] Canvas_HUD tidak ada, indikator luka dilewati"); return; }
            var lama = kanvas.Find("IndikatorLuka"); if (lama != null) Object.DestroyImmediate(lama.gameObject);
            var root = UIStretch(kanvas.gameObject, "IndikatorLuka", Vector2.zero, Vector2.one);
            root.transform.SetAsFirstSibling();
            var gv = UIStretch(root, "Vinyet", Vector2.zero, Vector2.one).AddComponent<Image>();
            gv.sprite = SpriteLuka("ui_vinyet_luka", false); gv.color = new Color(0.62f, 0.02f, 0.02f, 0f); gv.raycastTarget = false; gv.enabled = false;
            var ga = UIGambar(root, "Arah", SpriteLuka("ui_busur_luka", true), new Vector2(560f, 560f), new Vector2(0.5f, 0.5f), Vector2.zero).GetComponent<Image>();
            ga.color = new Color(0.85f, 0.08f, 0.05f, 0f); ga.raycastTarget = false; ga.enabled = false;
            var ind = root.AddComponent<IndikatorLuka>(); ind.Pasang(gv, ga); EditorUtility.SetDirty(ind);
            var mJejak = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_DebuCahaya.mat");
            int nJejak = 0;
            if (mJejak != null)
                foreach (var c in Object.FindObjectsByType<AlasKeramat.Player.CombatAndLeftHandController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                { c.PasangJejak(mJejak); EditorUtility.SetDirty(c); nJejak++; }
            Catat("Indikator luka: vinyet merah + busur arah serangan di HUD; jejak ayunan senjata di " + nJejak + " pengendali");
        }
    }
}
