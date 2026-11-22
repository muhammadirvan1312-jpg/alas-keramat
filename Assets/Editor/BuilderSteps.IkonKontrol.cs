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
            if (imp != null && (imp.textureType != TextureImporterType.Sprite || imp.spriteImportMode != SpriteImportMode.Single))
            {
                imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Single;
                imp.alphaIsTransparency = true; imp.mipmapEnabled = false;
                imp.textureCompression = TextureImporterCompression.Uncompressed; imp.SaveAndReimport();
            }
            var sp = AssetDatabase.LoadAssetAtPath<Sprite>(p);
            if (sp != null) return sp;
            // Cadangan bila pengimpor lain menimpa tipe tekstur: buat aset Sprite terpisah dari Texture2D.
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
            if (tex == null) return null;
            EnsureFolder(FolderIkon);
            string ps = FolderIkon + "/ikon_" + nama + "_sprite.asset";
            AssetDatabase.DeleteAsset(ps);
            sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            sp.name = "ikon_" + nama;
            AssetDatabase.CreateAsset(sp, ps);
            return AssetDatabase.LoadAssetAtPath<Sprite>(ps);
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
            imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Single;
            imp.alphaIsTransparency = true; imp.mipmapEnabled = false;
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
                    var img = go.GetComponent<Image>(); img.color = new Color(0.96f, 0.94f, 0.89f, 0.95f); img.raycastTarget = false; img.preserveAspect = true;
                    go.AddComponent<Shadow>().effectDistance = new Vector2(1.5f, -1.5f);
                    foreach (var tx in t.GetComponentsInChildren<Text>(true)) tx.enabled = false;
                    TerapkanGayaKacaTaktis(t.gameObject, PetaIkonTombol[i, 0]);
                    JumlahIkonTerpasang++;
                }
            }
            // Seragamkan juga tombol utilitas teks (MUAT, GANTI, MODE, BUKU) ke tema kaca gelap taktis.
            foreach (var nm in new[] { "TombolMuat", "TombolGanti", "TombolMode", "TombolBuku" })
            {
                daftar.Clear(); CariSemua(kanvas, nm, daftar);
                foreach (var t in daftar) TerapkanGayaKacaTaktis(t.gameObject, nm);
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
            // Penanda bulat benda interaktif (di belakang tombol, di atas dunia).
            var lamaP = kanvas.Find("PenandaInteraksi"); if (lamaP != null) Object.DestroyImmediate(lamaP.gameObject);
            var cincinP = AssetDatabase.LoadAssetAtPath<Sprite>(FolderIkon + "/ui_cincin.png");
            var gp = UIGambar(kanvas.gameObject, "PenandaInteraksi", cincinP, new Vector2(46f, 46f), new Vector2(0.5f, 0.5f), Vector2.zero);
            gp.transform.SetSiblingIndex(1);
            var ip = gp.GetComponent<Image>(); ip.color = new Color(1f, 0.95f, 0.8f, 0f); ip.raycastTarget = false; ip.enabled = false;
            var pi = gp.AddComponent<AlasKeramat.UI.PenandaInteraksi>(); pi.Pasang(ip); EditorUtility.SetDirty(pi);
            Catat("Ikon kontrol: " + JumlahIkonTerpasang + " tombol HUD pakai ikon + tema kaca taktis gelap AAA, joystick profesional " + (joy == 1 ? "terpasang" : "tidak ditemukan"));
        }

        // Tema tombol kaca gelap taktis profesional: dasar obsidian transparan + cincin tipis gading/aksen.
        static void TerapkanGayaKacaTaktis(GameObject go, string nama)
        {
            if (go == null) return;
            var bg = go.GetComponent<Image>();
            if (bg == null) return;
            Color isi = new Color(0.055f, 0.075f, 0.095f, 0.76f);
            Color tepi = new Color(0.90f, 0.86f, 0.76f, 0.42f);
            if (nama == "TombolTebas") { isi = new Color(0.56f, 0.14f, 0.12f, 0.84f); tepi = new Color(1f, 0.72f, 0.64f, 0.62f); }
            else if (nama == "TombolTangan") { isi = new Color(0.52f, 0.39f, 0.15f, 0.80f); tepi = new Color(1f, 0.88f, 0.54f, 0.65f); }
            else if (nama == "TombolTangkis") { isi = new Color(0.10f, 0.18f, 0.25f, 0.80f); tepi = new Color(0.68f, 0.86f, 0.96f, 0.52f); }
            bg.color = isi;

            bool bulat = bg.sprite != null && bg.sprite.name.IndexOf("lingkaran", System.StringComparison.OrdinalIgnoreCase) >= 0;
            if (bulat && go.transform.Find("CincinTaktis") == null)
            {
                var cincin = AssetDatabase.LoadAssetAtPath<Sprite>(FolderIkon + "/ui_cincin.png");
                if (cincin != null)
                {
                    var rt = (RectTransform)go.transform;
                    var ring = UIGambar(go, "CincinTaktis", cincin, rt.sizeDelta, new Vector2(0.5f, 0.5f), Vector2.zero);
                    var rrt = (RectTransform)ring.transform;
                    rrt.anchorMin = Vector2.zero; rrt.anchorMax = Vector2.one;
                    rrt.offsetMin = Vector2.zero; rrt.offsetMax = Vector2.zero;
                    ring.GetComponent<Image>().color = tepi;
                }
            }
            if (nama == "TombolGanti")
            {
                foreach (var tx in go.GetComponentsInChildren<Text>(true))
                {
                    tx.fontSize = 15;
                    tx.color = new Color(0.92f, 0.90f, 0.84f, 0.92f);
                }
            }
        }
    }
}
