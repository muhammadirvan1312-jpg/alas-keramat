using System.IO;
using AlasKeramat.Core;
using AlasKeramat.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.EditorTools
{
    // Langkah 7p (T7c): tekstur & material gore (4 percikan, genangan, atlas luka 4x2 + 8 mesh luka, tutup potongan), sistem partikel darah,
    // GoreSystem, ikon pendarahan, layar peringatan 18+ & pilihan Gore. Shader AK/Blood khusus menyusul fase poles (pakai Lit transparan).
    public static partial class NusantaraMasterBuilder
    {
        public const string FolderGore = FolderGen + "/Gore";
        const int UkuranTeksGore = 512;

        static void LangkahGore()
        {
            EnsureFolder(FolderGore);
            var normDarah = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/lumpur_n.png");
            var percikan = new Material[4];
            for (int i = 0; i < 4; i++) percikan[i] = MatGore("M_Percikan" + i, TeksPercikan("percikan" + i, 9100 + i * 37), SurfaceMode.Transparent, normDarah);
            var genangan = MatGore("M_Genangan", TeksGenangan(), SurfaceMode.Transparent, normDarah);
            var luka = MatGore("M_Luka", TeksLuka(), SurfaceMode.Cutout, normDarah);
            var tutup = MatGore("M_TutupPotongan", TeksTutup(), SurfaceMode.Opaque, normDarah);
            var meshLuka = new Mesh[8];
            for (int i = 0; i < 8; i++)
            {
                var m = new Mesh { name = "Luka" + i };
                float u0 = (i % 4) * 0.25f, v0 = (i / 4) * 0.5f;
                m.vertices = new[] { new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f), new Vector3(-0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0f) };
                m.uv = new[] { new Vector2(u0, v0), new Vector2(u0 + 0.25f, v0), new Vector2(u0, v0 + 0.5f), new Vector2(u0 + 0.25f, v0 + 0.5f) };
                m.triangles = new[] { 0, 2, 1, 2, 3, 1 };
                m.RecalculateNormals(); m.RecalculateBounds();
                string p = FolderGore + "/Luka" + i + ".asset";
                AssetDatabase.DeleteAsset(p); AssetDatabase.CreateAsset(m, p);
                meshLuka[i] = AssetDatabase.LoadAssetAtPath<Mesh>(p);
            }

            var goGore = Anak(_sistem, "GoreSystem");
            var gore = goGore.AddComponent<GoreSystem>();
            var ps = PartikelDarah(goGore);
            gore.Pasang(ps, percikan, genangan, luka, tutup, meshLuka);
            EditorUtility.SetDirty(gore);

            var kanvas = _ui != null ? _ui.transform.Find("Canvas_HUD") : null;
            if (kanvas == null) { Debug.LogError("[AK][Builder] Canvas_HUD tidak ada, UI gore dilewati"); return; }
            var tanda = kanvas.GetComponent<TandaTempur>();
            var ling = AssetDatabase.LoadAssetAtPath<Sprite>(FolderIkon + "/ui_lingkaran.png");
            var ikon = UIGambar(kanvas.gameObject, "IkonPendarahan", ling, new Vector2(34f, 44f), new Vector2(0f, 1f), new Vector2(430f, -60f));
            ikon.GetComponent<Image>().color = ProcTex.Hex("#B01010");
            ikon.GetComponent<Image>().enabled = false;
            if (tanda != null) { tanda.PasangIkonDarah(ikon.GetComponent<Image>()); EditorUtility.SetDirty(tanda); }
            LayarUsia(kanvas.gameObject);
            Catat("Gore: 4 percikan, genangan, atlas luka 8, tutup potongan, partikel darah, GoreSystem, peringatan 18+");
        }

        static Material MatGore(string nama, Texture2D tex, SurfaceMode mode, Texture2D norm = null)
        {
            string p = FolderGore + "/" + nama + ".mat";
            AssetDatabase.DeleteAsset(p);
            Color? emisi = mode == SurfaceMode.Opaque ? new Color(0.08f, 0.01f, 0.01f) : new Color(0.04f, 0.005f, 0.005f);
            var m = SafeMaterialFactory.CreatePBR(nama, Color.white, 0.05f, 0.92f, tex, norm, emisi, mode, 0.38f);
            AssetDatabase.CreateAsset(m, p);
            return AssetDatabase.LoadAssetAtPath<Material>(p);
        }

        static Texture2D SimpanTeksGore(string nama, Color32[] px, int n)
        {
            string p = FolderGore + "/" + nama + ".png";
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            tex.SetPixels32(px);
            File.WriteAllBytes(p, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(p);
            AturImpor(p, false, false, n);
            var imp = AssetImporter.GetAtPath(p) as TextureImporter;
            if (imp != null) { imp.alphaIsTransparency = true; imp.wrapMode = TextureWrapMode.Clamp; imp.SaveAndReimport(); }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(p);
        }

        // Percikan: tetes acak + coretan memanjang (14 §8), putih (warna lewat _BaseColor), alfa = bentuk.
        static Texture2D TeksPercikan(string nama, int seed)
        {
            int n = UkuranTeksGore;
            var a = new float[n * n];
            var rng = new System.Random(seed);
            float c = n * 0.5f;
            Cakram(a, n, c, c, n * 0.16f, 1f);
            int tetes = 18 + rng.Next(14);
            for (int i = 0; i < tetes; i++)
            {
                double sudut = rng.NextDouble() * System.Math.PI * 2.0; float jarak = (float)(0.15 + rng.NextDouble() * 0.33) * n;
                float x = c + (float)System.Math.Cos(sudut) * jarak, y = c + (float)System.Math.Sin(sudut) * jarak;
                Cakram(a, n, x, y, (float)(2 + rng.NextDouble() * 9), 1f);
                if (rng.NextDouble() < 0.4) for (int k = 1; k < 8; k++) Cakram(a, n, c + (x - c) * k / 8f, c + (y - c) * k / 8f, 3f + k * 0.4f, 0.9f); // coretan
            }
            return SimpanTeksGore(nama, KeWarna(a, n), n);
        }

        static Texture2D TeksGenangan()
        {
            int n = UkuranTeksGore; var a = new float[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float u = (x + 0.5f) / n * 2f - 1f, v = (y + 0.5f) / n * 2f - 1f;
                    float r = Mathf.Sqrt(u * u + v * v), ang = Mathf.Atan2(v, u);
                    float tepi = 0.78f + 0.12f * Mathf.PerlinNoise(Mathf.Cos(ang) * 2f + 5f, Mathf.Sin(ang) * 2f + 5f);
                    a[y * n + x] = Mathf.Clamp01((tepi - r) * 12f);
                }
            return SimpanTeksGore("genangan", KeWarna(a, n), n);
        }

        // Atlas luka 4x2: sayatan x2, tusukan x2, memar x2, luka bakar x2 (14 §4).
        static Texture2D TeksLuka()
        {
            int n = UkuranTeksGore; var px = new Color32[n * n];
            int sw = n / 4, sh = n / 2;
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    int sel = (y / sh) * 4 + x / sw;
                    float u = ((x % sw) + 0.5f) / sw * 2f - 1f, v = ((y % sh) + 0.5f) / sh * 2f - 1f;
                    Color c = Color.clear;
                    int jenis = sel / 2;
                    if (jenis == 0) { float d = Mathf.Abs(v - u * 0.2f * (sel % 2 == 0 ? 1f : -1f)); if (d < 0.12f && Mathf.Abs(u) < 0.85f) c = Color.Lerp(new Color(0.35f, 0.02f, 0.02f, 1f), new Color(0.6f, 0.08f, 0.06f, 1f), d / 0.12f); }
                    else if (jenis == 1) { float r = Mathf.Sqrt(u * u + v * v); if (r < 0.35f) c = r < 0.15f ? new Color(0.12f, 0.01f, 0.01f, 1f) : new Color(0.5f, 0.06f, 0.05f, 1f); }
                    else if (jenis == 2) { float r = Mathf.Sqrt(u * u + v * v); if (r < 0.7f) c = new Color(0.32f, 0.12f, 0.25f, Mathf.Clamp01((0.7f - r) * 3f)); }
                    else { float r = Mathf.Sqrt(u * u + v * v) + 0.15f * Mathf.PerlinNoise(u * 3f + 9f, v * 3f + 2f); if (r < 0.7f) c = Color.Lerp(new Color(0.15f, 0.08f, 0.05f, 1f), new Color(0.55f, 0.2f, 0.12f, 1f), r / 0.7f); }
                    px[y * n + x] = c;
                }
            return SimpanTeksGore("atlas_luka", px, n);
        }

        // Tutup potongan: cincin kulit, lemak #E8D8A0, otot #7A1010, tulang #EDE6D6 + sumsum (14 §5).
        static Texture2D TeksTutup()
        {
            int n = 128; var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float u = (x + 0.5f) / n * 2f - 1f, v = (y + 0.5f) / n * 2f - 1f, r = Mathf.Sqrt(u * u + v * v);
                    Color c = r > 0.92f ? new Color(0.55f, 0.38f, 0.3f) : r > 0.84f ? ProcTex.Hex("#E8D8A0") : r > 0.3f ? ProcTex.Hex("#7A1010") : r > 0.1f ? ProcTex.Hex("#EDE6D6") : new Color(0.45f, 0.12f, 0.1f);
                    px[y * n + x] = c;
                }
            return SimpanTeksGore("tutup_potongan", px, n);
        }

        static void Cakram(float[] a, int n, float cx, float cy, float r, float nilai)
        {
            int x0 = Mathf.Max(0, (int)(cx - r - 1)), x1 = Mathf.Min(n - 1, (int)(cx + r + 1)), y0 = Mathf.Max(0, (int)(cy - r - 1)), y1 = Mathf.Min(n - 1, (int)(cy + r + 1));
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    a[y * n + x] = Mathf.Max(a[y * n + x], Mathf.Clamp01(r - d + 0.5f) * nilai);
                }
        }

        static Color32[] KeWarna(float[] a, int n)
        {
            var px = new Color32[n * n];
            for (int i = 0; i < px.Length; i++) px[i] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(a[i]) * 255f));
            return px;
        }

        static ParticleSystem PartikelDarah(GameObject induk)
        {
            var go = Anak(induk, "PartikelDarah");
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = false; main.playOnAwake = false; main.startSpeed = 0f; main.gravityModifier = 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World; main.maxParticles = GoreSystem.MaksPartikel; main.startLifetime = 1f;
            var em = ps.emission; em.enabled = false;
            var sh = ps.shape; sh.enabled = false;
            var r = go.GetComponent<ParticleSystemRenderer>();
            string p = FolderGore + "/M_PartikelDarah.mat";
            AssetDatabase.DeleteAsset(p);
            var sd = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (sd == null) sd = SafeMaterialFactory.LitShader();
            var m = new Material(sd) { name = "M_PartikelDarah" };
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderIkon + "/ui_lingkaran.png");
            if (tex != null && m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
            if (m.HasProperty("_Surface")) { m.SetFloat("_Surface", 1f); m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.renderQueue = 3000; }
            AssetDatabase.CreateAsset(m, p);
            r.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(p);
            r.renderMode = ParticleSystemRenderMode.Billboard;
            return ps;
        }

        static void LayarUsia(GameObject kanvas)
        {
            var font = ProceduralAssetFactory.GetSafeFont();
            var root = UIStretch(kanvas, "LayarPeringatan", Vector2.zero, Vector2.one);
            var layar = root.AddComponent<LayarPeringatan>();
            var p1 = UIStretch(root, "PanelPeringatan", Vector2.zero, Vector2.one);
            p1.AddComponent<Image>().color = ProcTex.Hex("#0A0606F2");
            var t1 = BuatTeks(p1, "Teks", "PERINGATAN 18+\n\nGame ini mengandung kekerasan, darah, mutilasi, dan horor.\nKhusus 18 tahun ke atas.", font, 40, new Vector2(0.5f, 0.6f), Vector2.zero, new Vector2(1400f, 400f));
            t1.color = ProcTex.Hex("#F2EFE6"); t1.alignment = TextAnchor.MiddleCenter;
            Text tx;
            var lanjut = TombolPanel(p1, "TombolLanjutkan", "LANJUTKAN", font, new Vector2(0f, 220f), new Vector2(360f, 100f), ProcTex.Hex("#B5462E"), out tx);
            UnityEventTools.AddPersistentListener(lanjut.onClick, layar.TekanLanjutkan);
            var p2 = UIStretch(root, "PanelGore", Vector2.zero, Vector2.one);
            p2.AddComponent<Image>().color = ProcTex.Hex("#0A0606F2");
            var t2 = BuatTeks(p2, "Teks", "PILIH TINGKAT GORE\n\nMati: tanpa darah, musuh larut jadi abu.  Sedang: darah, luka, mayat.  Penuh: + mutilasi.\nBisa diubah kapan saja di Pengaturan.", font, 34, new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(1500f, 320f));
            t2.color = ProcTex.Hex("#F2EFE6"); t2.alignment = TextAnchor.MiddleCenter;
            var label = new[] { "MATI", "SEDANG", "PENUH" };
            var sorot = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var b = TombolPanel(p2, "TombolGore" + label[i], label[i], font, new Vector2((i - 1) * 400f, 240f), new Vector2(340f, 110f), ProcTex.Hex("#333333"), out tx);
                UnityEventTools.AddIntPersistentListener(b.onClick, layar.PilihGore, i);
                sorot[i] = b.GetComponent<Image>();
            }
            layar.Pasang(p1, p2, sorot);
            p1.SetActive(false); p2.SetActive(false);
            root.transform.SetAsLastSibling();
            EditorUtility.SetDirty(layar);
        }
    }
}
