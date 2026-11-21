using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace AlasKeramat.EditorTools
{
    // Langkah 7z6: tombol HUD pakai ikon (game-icons.net, CC BY 3.0, lihat Assets/UI/IkonKontrol/KREDIT.txt)
    // dan joystick bergaya game profesional (cincin tipis, panah arah, kenop bulat bervolume).
    public static partial class NusantaraMasterBuilder
    {
        public const string FolderIkonKontrol = "Assets/UI/IkonKontrol";
        public static readonly string[,] PetaIkonTombol =
        {
            { "TombolTebas", "tebas" }, { "TombolTangkis", "tangkis" }, { "TombolLompat", "lompat" }, { "TombolJongkok", "jongkok" },
            { "TombolTas", "tas" }, { "TombolTangan", "tangan" }, { "TombolObor", "obor" }, { "BukaPetaSurvei", "peta" },
            { "TombolJurnal", "jurnal" }, { "TombolSimpan", "simpan" }, { "TombolBatu", "batu" }, { "TombolLempar", "lempar" },
            { "TombolMakan", "makan" }, { "TombolJeda", "jeda" },
        };
        public static int JumlahIkonTerpasang;

        static Sprite MuatIkon(string nama)
        {
            string p = FolderIkonKontrol + "/ikon_" + nama + ".png";
            if (!File.Exists(p)) return null;
            var imp = AssetImporter.GetAtPath(p) as TextureImporter;
            if (imp != null && imp.textureType != TextureImporterType.Sprite)
            {
                imp.textureType = TextureImporterType.Sprite; imp.alphaIsTransparency = true; imp.mipmapEnabled = false;
                imp.textureCompression = TextureImporterCompression.Uncompressed; imp.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(p);
        }

        static void CariSemua(Transform t, string nama, System.Collections.Generic.List<Transform> hasil)
        {
            if (t.name == nama) hasil.Add(t);
            for (int i = 0; i < t.childCount; i++) CariSemua(t.GetChild(i), nama, hasil);
        }

        static Sprite SpriteJoystick(string nama, bool kenop)
        {
            EnsureFolder(FolderIkon);
            string p = FolderIkon + "/" + nama + ".png";
            const int n = 256; float c = n * 0.5f;
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = x + 0.5f - c, dy = y + 0.5f - c, d = Mathf.Sqrt(dx * dx + dy * dy) / c;
                    Color col = new Color(1f, 1f, 1f, 0f);
                    if (kenop)
                    {
                        if (d < 0.96f)
                        {
                            // Bola: terang di kiri-atas, gelap di tepi, garis tepi tipis.
                            float h = Mathf.Clamp01(1f - Mathf.Sqrt((dx + c * 0.3f) * (dx + c * 0.3f) + (dy - c * 0.35f) * (dy - c * 0.35f)) / (c * 1.3f));
                            float v = Mathf.Lerp(0.62f, 1f, h) * (d > 0.88f ? 0.8f : 1f);
                            col = new Color(v, v, v, Mathf.Clamp01((0.96f - d) * 40f));
                        }
                    }
                    else
                    {
                        float a = 0f;
                        if (d < 1f) a = 0.22f;                                                  // isi gelap transparan
                        a = Mathf.Max(a, Mathf.Clamp01(1f - Mathf.Abs(d - 0.94f) / 0.025f) * 0.95f); // cincin luar tipis
                        a = Mathf.Max(a, Mathf.Clamp01(1f - Mathf.Abs(d - 0.58f) / 0.012f) * 0.25f); // cincin dalam samar
                        // Panah kecil di 4 arah.
                        for (int k = 0; k < 4; k++)
                        {
                            float sd = k * Mathf.PI * 0.5f, ux = Mathf.Cos(sd), uy = Mathf.Sin(sd);
                            float maju = (dx * ux + dy * uy) / c, samping = Mathf.Abs(-dx * uy + dy * ux) / c;
                            if (maju > 0.72f && maju < 0.84f && samping < (0.84f - maju) * 0.9f) a = Mathf.Max(a, 0.85f);
                        }
                        float g = d < 1f && a <= 0.22f ? 0.08f : 1f;
                        col = new Color(g, g, g, d < 0.97f || a > 0.3f ? a : 0f);
                    }
                    px[y * n + x] = col;
                }
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false); t.SetPixels(px); t.Apply();
            File.WriteAllBytes(p, t.EncodeToPNG()); Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(p);
            var imp = (TextureImporter)AssetImporter.GetAtPath(p);
            imp.textureType = TextureImporterType.Sprite; imp.alphaIsTransparency = true; imp.mipmapEnabled = false;
            imp.wrapMode = TextureWrapMode.Clamp; imp.textureCompression = TextureImporterCompression.Uncompressed; imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(p);
        }

        static void LangkahIkonKontrol()
        {
            var kanvas = _ui != null ? _ui.transform.Find("Canvas_HUD") : null;
            if (kanvas == null) { Debug.LogWarning("[AK][Builder] Canvas_HUD tidak ada, ikon kontrol dilewati"); return; }
            JumlahIkonTerpasang = 0;
            var daftar = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < PetaIkonTombol.GetLength(0); i++)
            {
                var ikon = MuatIkon(PetaIkonTombol[i, 1]);
                if (ikon == null) { Debug.LogWarning("[AK][Builder] Ikon " + PetaIkonTombol[i, 1] + " tidak ada"); continue; }
                daftar.Clear(); CariSemua(kanvas, PetaIkonTombol[i, 0], daftar);
                foreach (var t in daftar)
                {
                    var lama = t.Find("Ikon"); if (lama != null) Object.DestroyImmediate(lama.gameObject);
                    var rt = (RectTransform)t;
                    float sisi = Mathf.Min(rt.sizeDelta.x, rt.sizeDelta.y) * 0.62f;
                    if (sisi < 24f) sisi = 48f;
                    var go = UIGambar(t.gameObject, "Ikon", ikon, new Vector2(sisi, sisi), new Vector2(0.5f, 0.5f), Vector2.zero);
                    var img = go.GetComponent<Image>(); img.color = new Color(1f, 1f, 1f, 0.95f); img.raycastTarget = false; img.preserveAspect = true;
                    go.AddComponent<Shadow>().effectDistance = new Vector2(2f, -2f);
                    foreach (var tx in t.GetComponentsInChildren<Text>(true)) tx.enabled = false;
                    JumlahIkonTerpasang++;
                }
            }
            // Joystick bergaya profesional.
            daftar.Clear(); CariSemua(kanvas, "ZonaJoystick", daftar);
            int joy = 0;
            if (daftar.Count > 0)
            {
                var basis = daftar[0].Find("Basis");
                var kenop = basis != null ? basis.Find("Kenop") : null;
                if (basis != null && kenop != null)
                {
                    var gb = basis.GetComponent<Image>(); gb.sprite = SpriteJoystick("ui_joy_basis", false); gb.color = Color.white;
                    ((RectTransform)basis).sizeDelta = new Vector2(320f, 320f);
                    var gk = kenop.GetComponent<Image>(); gk.sprite = SpriteJoystick("ui_joy_kenop", true); gk.color = new Color(1f, 1f, 1f, 0.92f);
                    ((RectTransform)kenop).sizeDelta = new Vector2(138f, 138f);
                    if (kenop.GetComponent<Shadow>() == null) kenop.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(0f, -5f);
                    joy = 1;
                }
            }
            Catat("Ikon kontrol: " + JumlahIkonTerpasang + " tombol HUD pakai ikon, joystick profesional " + (joy == 1 ? "terpasang" : "tidak ditemukan"));
        }
    }
}
