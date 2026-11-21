using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace AlasKeramat.EditorTools
{
    // Gaya tombol HUD/panel: sudut membulat 9-slice, tepi terang tipis, gradasi atas, label tebal berbayang (ganti kotak polos).
    public static partial class NusantaraMasterBuilder
    {
        public const int UkuranSpriteTombol = 64, RadiusSpriteTombol = 18, BatasSlice = 22;
        static Sprite _spriteTombol;

        // Alfa persegi sudut bulat (jari-jari r) yang menyusut sebesar inset dari tepi gambar n x n.
        public static float AlfaSudutBulat(float x, float y, int n, float r, float inset)
        {
            float cx = Mathf.Clamp(x, r, n - r), cy = Mathf.Clamp(y, r, n - r);
            float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
            return Mathf.Clamp01(r - inset - d + 0.5f);
        }

        static Sprite SpriteTombol()
        {
            if (_spriteTombol != null) return _spriteTombol;
            EnsureFolder(FolderIkon);
            string p = FolderIkon + "/UI_TombolBulat.png";
            const int n = UkuranSpriteTombol; const float r = RadiusSpriteTombol;
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float fx = x + 0.5f, fy = y + 0.5f;
                    float luar = AlfaSudutBulat(fx, fy, n, r, 0f), dalam = AlfaSudutBulat(fx, fy, n, r, 2.2f);
                    float tepi = Mathf.Clamp01(luar - dalam);
                    float v = Mathf.Lerp(Mathf.Lerp(0.8f, 1f, (float)y / n), 1f, tepi);
                    px[y * n + x] = new Color(v, v, v, dalam * 0.9f + tepi);
                }
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false); t.SetPixels(px); t.Apply();
            File.WriteAllBytes(p, t.EncodeToPNG());
            Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(p);
            var imp = (TextureImporter)AssetImporter.GetAtPath(p);
            imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Single;
            imp.spriteBorder = new Vector4(BatasSlice, BatasSlice, BatasSlice, BatasSlice);
            imp.alphaIsTransparency = true; imp.mipmapEnabled = false; imp.filterMode = FilterMode.Bilinear;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.SaveAndReimport();
            _spriteTombol = AssetDatabase.LoadAssetAtPath<Sprite>(p);
            return _spriteTombol;
        }

        static void GayakanTombol(Image img, Color warna, Text label)
        {
            var s = SpriteTombol();
            if (s != null) { img.sprite = s; img.type = Image.Type.Sliced; img.pixelsPerUnitMultiplier = 1.6f; }
            img.color = new Color(warna.r * 0.9f, warna.g * 0.9f, warna.b * 0.9f, 0.8f);
            if (label == null) return;
            label.fontStyle = FontStyle.Bold;
            if (label.GetComponent<Shadow>() == null) { var sh = label.gameObject.AddComponent<Shadow>(); sh.effectColor = new Color(0f, 0f, 0f, 0.65f); sh.effectDistance = new Vector2(1.5f, -1.5f); }
        }
    }
}
