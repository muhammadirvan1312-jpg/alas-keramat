using System;
using System.Collections.Generic;
using System.IO;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;

namespace AlasKeramat.EditorTools
{
    // Langkah 2: tekstur, ikon, material, mesh, audio WAV dipanggang ke Assets/Generated.
    public static partial class NusantaraMasterBuilder
    {
        public const string FolderTex = FolderGen + "/Textures";
        public const string FolderIkon = FolderGen + "/Icons";
        public const string FolderMat = FolderGen + "/Materials";
        public const string FolderMesh = FolderGen + "/Meshes";
        public const string FolderAudio = FolderGen + "/Audio";
        const int UkuranTex = 256;
        const int MaksTexDunia = 2048, MaksTexIkon = 128;

        // Hasil aset yang dipakai langkah scene.
        static readonly Dictionary<string, Material> _mat = new Dictionary<string, Material>();
        static readonly Dictionary<string, AudioClip> _klip = new Dictionary<string, AudioClip>();

        struct SpekTex
        {
            public string nama; public int periode, oktaf, seed; public bool ridged, normal; public float kuatNormal; public Color[] warna;
        }

        static readonly SpekTex[] DaftarTex =
        {
            new SpekTex { nama = "tanah", periode = 4, oktaf = 5, seed = 11, normal = true, kuatNormal = 6f, warna = new[] { ProcTex.Hex("#2E2418"), ProcTex.Hex("#4A3A26"), ProcTex.Hex("#6B5536") } },
            new SpekTex { nama = "rumput", periode = 8, oktaf = 5, seed = 12, warna = new[] { ProcTex.Hex("#24361A"), ProcTex.Hex("#3E5A26"), ProcTex.Hex("#5F7A35") } },
            new SpekTex { nama = "batu", periode = 4, oktaf = 6, seed = 13, ridged = true, normal = true, kuatNormal = 10f, warna = new[] { ProcTex.Hex("#3D3E3C"), ProcTex.Hex("#6E706C"), ProcTex.Hex("#9A9B96") } },
            new SpekTex { nama = "kayu", periode = 2, oktaf = 4, seed = 14, normal = true, kuatNormal = 4f, warna = new[] { ProcTex.Hex("#4E321A"), ProcTex.Hex("#7A5230"), ProcTex.Hex("#A47648") } },
            new SpekTex { nama = "kulit_kayu", periode = 6, oktaf = 5, seed = 15, ridged = true, warna = new[] { ProcTex.Hex("#2A1F15"), ProcTex.Hex("#4B3826"), ProcTex.Hex("#6A5440") } },
            new SpekTex { nama = "daun", periode = 8, oktaf = 4, seed = 16, warna = new[] { ProcTex.Hex("#1E3314"), ProcTex.Hex("#335C22"), ProcTex.Hex("#58823A") } },
            new SpekTex { nama = "pasir", periode = 16, oktaf = 4, seed = 17, warna = new[] { ProcTex.Hex("#9C8B66"), ProcTex.Hex("#C2AE86"), ProcTex.Hex("#DCCBA3") } },
            new SpekTex { nama = "lumpur", periode = 4, oktaf = 5, seed = 18, warna = new[] { ProcTex.Hex("#1E1710"), ProcTex.Hex("#33281C"), ProcTex.Hex("#4A3C2B") } },
            new SpekTex { nama = "beton", periode = 8, oktaf = 6, seed = 19, normal = true, kuatNormal = 3f, warna = new[] { ProcTex.Hex("#5C5C58"), ProcTex.Hex("#7E7E79"), ProcTex.Hex("#9E9E98") } },
            new SpekTex { nama = "logam_karat", periode = 6, oktaf = 5, seed = 20, warna = new[] { ProcTex.Hex("#3A2A20"), ProcTex.Hex("#7A3E1E"), ProcTex.Hex("#5E6266") } },
        };

        static void LangkahAset(bool cepat)
        {
            foreach (var f in new[] { FolderTex, FolderIkon, FolderMat, FolderMesh, FolderAudio }) EnsureFolder(f);
            _mat.Clear(); _klip.Clear();
            BuatTekstur(cepat);
            BuatIkon(cepat);
            BuatMaterial();
            BuatAudio(cepat);
            AssetDatabase.SaveAssets();
        }

        static float[] Tinggi(SpekTex s)
        {
            return ProcTex.HeightMap(UkuranTex, (u, v) => s.ridged ? ProcTex.Ridged(u, v, s.periode, s.oktaf, s.seed) : ProcTex.Fbm(u, v, s.periode, s.oktaf, s.seed));
        }

        static void BuatTekstur(bool cepat)
        {
            int n = 0;
            foreach (var s in DaftarTex)
            {
                string pa = FolderTex + "/" + s.nama + ".png";
                string pn = FolderTex + "/" + s.nama + "_n.png";
                bool adaAlbedo = cepat && File.Exists(pa), adaNormal = !s.normal || (cepat && File.Exists(pn));
                if (!adaAlbedo || !adaNormal)
                {
                    float[] h = Tinggi(s);
                    if (!adaAlbedo) SimpanPng(ProcTex.FromHeight(s.nama, UkuranTex, h, ProcTex.Palet(s.warna)), pa);
                    if (!adaNormal) SimpanPng(ProcTex.NormalFromHeight(s.nama + "_n", UkuranTex, h, s.kuatNormal), pn);
                }
                n += s.normal ? 2 : 1;
            }
            AssetDatabase.Refresh();
            foreach (var s in DaftarTex)
            {
                AturImpor(FolderTex + "/" + s.nama + ".png", false, false, MaksTexDunia);
                if (s.normal) AturImpor(FolderTex + "/" + s.nama + "_n.png", true, false, MaksTexDunia);
            }
            Catat(n + " tekstur");
        }

        static void BuatIkon(bool cepat)
        {
            foreach (var nama in IconPainter.NamaIkon)
            {
                string p = FolderIkon + "/ikon_" + nama + ".png";
                if (!(cepat && File.Exists(p))) SimpanPng(IconPainter.Paint(nama), p);
            }
            AssetDatabase.Refresh();
            foreach (var nama in IconPainter.NamaIkon) AturImpor(FolderIkon + "/ikon_" + nama + ".png", false, true, MaksTexIkon);
            Catat(IconPainter.NamaIkon.Length + " ikon");
        }

        static void BuatMaterial()
        {
            BuatMat("M_Tanah", "tanah", Color.white, 0f, 0.15f, SurfaceMode.Opaque);
            BuatMat("M_Kayu", "kayu", Color.white, 0f, 0.3f, SurfaceMode.Opaque);
            BuatMat("M_Batu", "batu", Color.white, 0f, 0.25f, SurfaceMode.Opaque);
            BuatMat("M_Beton", "beton", Color.white, 0f, 0.2f, SurfaceMode.Opaque);
            BuatMat("M_LogamKarat", "logam_karat", Color.white, 0.6f, 0.35f, SurfaceMode.Opaque);
            BuatMat("M_Daun", "daun", Color.white, 0f, 0.35f, SurfaceMode.Cutout);
            Catat(_mat.Count + " material");
        }

        static void BuatMat(string nama, string tex, Color warna, float metal, float halus, SurfaceMode mode)
        {
            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/" + tex + ".png");
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/" + tex + "_n.png");
            var m = SafeMaterialFactory.CreatePBR(nama, warna, metal, halus, albedo, normal, null, mode);
            string p = FolderMat + "/" + nama + ".mat";
            AssetDatabase.DeleteAsset(p);
            AssetDatabase.CreateAsset(m, p);
            _mat[nama] = AssetDatabase.LoadAssetAtPath<Material>(p);
        }

        static void BuatAudio(bool cepat)
        {
            foreach (var nama in ProceduralAudioSynth.KlipWajib)
            {
                string p = FolderAudio + "/" + nama + ".wav";
                if (cepat && File.Exists(p)) continue;
                int sr;
                float[] data = ProceduralAudioSynth.Generate(nama, 0, out sr);
                if (data == null) throw new Exception("resep audio tidak ada: " + nama);
                File.WriteAllBytes(p, WavWriter.Encode(data, sr));
            }
            AssetDatabase.Refresh();
            foreach (var nama in ProceduralAudioSynth.KlipWajib)
            {
                string p = FolderAudio + "/" + nama + ".wav";
                var imp = AssetImporter.GetAtPath(p) as AudioImporter;
                if (imp != null)
                {
                    var s = imp.defaultSampleSettings;
                    s.loadType = AudioClipLoadType.DecompressOnLoad;
                    s.compressionFormat = AudioCompressionFormat.ADPCM;
                    imp.defaultSampleSettings = s;
                    imp.SaveAndReimport();
                }
                var c = AssetDatabase.LoadAssetAtPath<AudioClip>(p);
                if (c == null) throw new Exception("klip gagal diimpor: " + p);
                _klip[nama] = c;
            }
            Catat(_klip.Count + " klip WAV");
        }

        static void SimpanPng(Texture2D t, string path)
        {
            File.WriteAllBytes(path, t.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(t);
        }

        static void AturImpor(string path, bool normal, bool sprite, int maks)
        {
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) throw new Exception("importer tekstur tidak ada: " + path);
            imp.textureType = normal ? TextureImporterType.NormalMap : sprite ? TextureImporterType.Sprite : TextureImporterType.Default;
            imp.mipmapEnabled = !sprite;
            imp.wrapMode = sprite ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
            imp.alphaIsTransparency = sprite;
            imp.maxTextureSize = maks;
            var android = imp.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.maxTextureSize = maks;
            android.format = sprite ? TextureImporterFormat.ASTC_4x4 : TextureImporterFormat.ASTC_6x6;
            imp.SetPlatformTextureSettings(android);
            imp.SaveAndReimport();
        }
    }
}
