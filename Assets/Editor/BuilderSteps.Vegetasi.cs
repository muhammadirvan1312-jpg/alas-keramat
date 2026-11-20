using System;
using System.Collections.Generic;
using AlasKeramat.Building;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using J = AlasKeramat.Core.ProcPohon.Jenis;

namespace AlasKeramat.EditorTools
{
    // Langkah 3c (T2b): mesh pohon & semak prosedural, penempatan deterministik per bioma (03 §7, 20 §2), VegetationData.
    public static partial class NusantaraMasterBuilder
    {
        public const string FolderVeg = FolderGen + "/Vegetasi";
        public const string PathVegData = FolderVeg + "/VegetationData.asset";
        const int Varian = 3, CalonPohonPerHa = 140, CalonSemakPerHa = 560, UkuranSel = 125;
        const float FaktorSemak = 0.85f, LerengMaksVeg = 42f, JarakGridSawit = 8f;

        struct Kepadatan { public J jenis; public float perHa; public Kepadatan(J j, float d) { jenis = j; perHa = d; } }

        static Kepadatan[] TabelPohon(int biome)
        {
            switch (biome)
            {
                case (int)Biome.HutanJatiSelatan: return new[] { new Kepadatan(J.Jati, 68), new Kepadatan(J.Mahoni, 32), new Kepadatan(J.Aren, 10), new Kepadatan(J.Pisang, 8), new Kepadatan(J.BeringinKecil, 4) };
                case (int)Biome.HutanCampuran: return new[] { new Kepadatan(J.Mahoni, 55), new Kepadatan(J.Jati, 40), new Kepadatan(J.Aren, 12), new Kepadatan(J.Pisang, 8), new Kepadatan(J.BeringinKecil, 4) };
                case (int)Biome.HutanBambu: return new[] { new Kepadatan(J.BambuRumpun, 48), new Kepadatan(J.Mahoni, 14), new Kepadatan(J.Pisang, 10) };
                case (int)Biome.TepiSungai: return new[] { new Kepadatan(J.Mahoni, 42), new Kepadatan(J.Jati, 34), new Kepadatan(J.BambuRumpun, 18), new Kepadatan(J.Aren, 10), new Kepadatan(J.Pisang, 10), new Kepadatan(J.Kelapa, 6) };
                case (int)Biome.Pantai: return new[] { new Kepadatan(J.Kelapa, 24) };
                case (int)Biome.Desa: return new[] { new Kepadatan(J.Kelapa, 8), new Kepadatan(J.Pisang, 10) };
                case (int)Biome.KawahBeringin: return new[] { new Kepadatan(J.Mahoni, 28), new Kepadatan(J.Jati, 16) };
                case (int)Biome.TebingKapur: return new[] { new Kepadatan(J.Jati, 24), new Kepadatan(J.Mahoni, 18) };
                case 11: return new[] { new Kepadatan(J.BeringinKecil, 14), new Kepadatan(J.Aren, 8), new Kepadatan(J.Pisang, 4) };
                case 12: return new[] { new Kepadatan(J.Kelapa, 3) };
                case 13: return new[] { new Kepadatan(J.Jati, 24), new Kepadatan(J.Mahoni, 14) };
                case 14: return new[] { new Kepadatan(J.Tebu, 70), new Kepadatan(J.Kelapa, 3) };
                case 15: return new[] { new Kepadatan(J.Mahoni, 60), new Kepadatan(J.BeringinKecil, 10), new Kepadatan(J.Aren, 12), new Kepadatan(J.Jati, 18) };
                default: return new Kepadatan[0];
            }
        }

        static Kepadatan[] TabelSemak(int biome)
        {
            switch (biome)
            {
                case (int)Biome.HutanJatiSelatan: return new[] { new Kepadatan(J.Pakis, 340), new Kepadatan(J.Semak, 200) };
                case (int)Biome.HutanCampuran: return new[] { new Kepadatan(J.Pakis, 360), new Kepadatan(J.Semak, 220) };
                case (int)Biome.HutanBambu: return new[] { new Kepadatan(J.Semak, 90), new Kepadatan(J.Pakis, 70) };
                case (int)Biome.TepiSungai: return new[] { new Kepadatan(J.Pakis, 320), new Kepadatan(J.Semak, 200) };
                case (int)Biome.Pantai: case (int)Biome.Desa: return new[] { new Kepadatan(J.Semak, 40) };
                case (int)Biome.KawahBeringin: return new[] { new Kepadatan(J.Pakis, 120), new Kepadatan(J.Semak, 80) };
                case (int)Biome.TebingKapur: return new[] { new Kepadatan(J.Pakis, 90), new Kepadatan(J.Semak, 80) };
                case 11: return new[] { new Kepadatan(J.Semak, 60), new Kepadatan(J.Pakis, 80) };
                case 13: return new[] { new Kepadatan(J.Semak, 90), new Kepadatan(J.Pakis, 70) };
                case 14: return new[] { new Kepadatan(J.Semak, 14) };
                case 15: return new[] { new Kepadatan(J.Pakis, 360), new Kepadatan(J.Semak, 200) };
                default: return new Kepadatan[0];
            }
        }

        // Area bebas vegetasi (pad POI v5): x, z, radius.
        static readonly Vector3[] AreaBebas =
        {
            new Vector3(8f, -448f, 10.5f), new Vector3(56f, -376f, 24f), new Vector3(0f, -840f, 50f),
            new Vector3(0f, 0f, 22f), new Vector3(660f, -500f, 110f), new Vector3(0f, 690f, 14f),
        };

        static void LangkahVegetasi()
        {
            EnsureFolder(FolderVeg);
            var atlas = AtlasDaunCampuran();
            string pAtlas = FolderVeg + "/atlas_daun.png";
            SimpanPng(atlas, pAtlas);
            AssetDatabase.Refresh();
            var imp = (TextureImporter)AssetImporter.GetAtPath(pAtlas);
            imp.alphaIsTransparency = true; imp.mipmapEnabled = true; imp.wrapMode = TextureWrapMode.Clamp; imp.maxTextureSize = 1024;
            imp.mipMapsPreserveCoverage=true;imp.alphaTestReferenceValue=0.5f;imp.filterMode=FilterMode.Trilinear;
            imp.SaveAndReimport();
            var texAtlas = AssetDatabase.LoadAssetAtPath<Texture2D>(pAtlas);

            var matDaun = SimpanMat(KeAK(SafeMaterialFactory.CreatePBR("M_DaunPohon", Color.white, 0f, 0.25f, texAtlas, null, null, SurfaceMode.Cutout, 0.5f), ShaderDaun, 0.03f, 0.45f));
            var matDaunHangus = SimpanMat(KeAK(SafeMaterialFactory.CreatePBR("M_DaunHangus", ProcTex.Hex("#3A3028"), 0f, 0.1f, texAtlas, null, null, SurfaceMode.Cutout, 0.5f), ShaderDaun, 0.015f, 0.1f));
            var kulit = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/kulit_kayu.png");
            var kulitN = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/kulit_kayu_n.png");
            var palem = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/palem.png");
            var palemN = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/palem_n.png");
            var matKulit = SimpanMat(KeAK(SafeMaterialFactory.CreatePBR("M_KulitPohon", Color.white, 0f, 0.18f, kulit, kulitN), ShaderKulit, 0.02f, 0f));
            var matPalem = SimpanMat(KeAK(SafeMaterialFactory.CreatePBR("M_BatangPalem", ProcTex.Hex("#7A7466"), 0f, 0.2f, palem != null ? palem : kulit, palemN != null ? palemN : kulitN), ShaderKulit, 0.035f, 0f));
            var matBambu = SimpanMat(KeAK(SafeMaterialFactory.CreatePBR("M_Bambu", ProcTex.Hex("#3B5822"), 0f, 0.3f, AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/kulit_kayu.png"), AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/kulit_kayu_n.png")), ShaderKulit, 0.05f, 0f));
            var matHangus = SimpanMat(KeAK(SafeMaterialFactory.CreatePBR("M_BatangHangus", ProcTex.Hex("#2A2420"), 0f, 0.1f, kulit, kulitN), ShaderKulit, 0.01f, 0f));

            var data = ScriptableObject.CreateInstance<VegetationData>();
            var jenisList = new List<VegetationData.JenisVeg>();
            for (int j = 0; j < ProcPohon.JumlahJenis; j++)
                for (int v = 0; v < Varian; v++)
                {
                    var jj = (J)j;
                    bool semak = jj == J.Semak || jj == J.Pakis;
                    var m0 = ProcPohon.Buat(jj, v, false); var m1 = ProcPohon.Buat(jj, v, true);
                    AssetDatabase.CreateAsset(m0, FolderVeg + "/" + m0.name + ".asset");
                    AssetDatabase.CreateAsset(m1, FolderVeg + "/" + m1.name + ".asset");
                    Material kayu = jj == J.BambuRumpun || jj == J.Tebu ? matBambu : jj == J.SawitHangus ? matHangus : jj == J.Kelapa || jj == J.Aren || jj == J.Pisang ? matPalem : matKulit;
                    jenisList.Add(new VegetationData.JenisVeg
                    {
                        nama = jj + "_" + v, lod0 = m0, lod1 = m1, kayu = kayu, daun = jj == J.SawitHangus ? matDaunHangus : matDaun,
                        jarakLod1 = semak ? 20f : 45f, jarakMaks = semak ? 70f : jj == J.Tebu ? 220f : 450f, bayangan = !semak
                    });
                }
            data.jenis = jenisList.ToArray();
            PasangMeshScan(data);
            data.sel = TempatkanVegetasi(data.jenis.Length);
            AssetDatabase.CreateAsset(data, PathVegData);

            var vm = Anak(_sistem, "VegetationManager").AddComponent<VegetationManager>();
            vm.Pasang(AssetDatabase.LoadAssetAtPath<VegetationData>(PathVegData));
            EditorUtility.SetDirty(vm);
            int pohon = data.HitungInstance(i => i / Varian != (int)J.Semak && i / Varian != (int)J.Pakis);
            int semakTotal = data.HitungInstance(i => i / Varian == (int)J.Semak || i / Varian == (int)J.Pakis);
            JumlahPohon = pohon; JumlahSemak = semakTotal;
            Catat("Vegetasi: " + pohon + " pohon, " + semakTotal + " semak/pakis, " + data.sel.Length + " sel instancing");
        }

        public static int JumlahPohon, JumlahSemak;

        static Material SimpanMat(Material m)
        {
            string p = FolderMat + "/" + m.name + ".mat";
            AssetDatabase.CreateAsset(m, p);
            return AssetDatabase.LoadAssetAtPath<Material>(p);
        }

        static bool BolehTanam(float x, float z, out float y, bool semak = false)
        {
            y = WorldTerrainGenerator.SampleHeight(x, z);
            if (y < 0.6f) return false;
            float? wl = WorldTerrainGenerator.GetWaterLevelAt(x, z);
            if (wl.HasValue && y < wl.Value + 0.4f) return false;
            for (int i = 0; i < AreaBebas.Length; i++)
            {
                float dx = x - AreaBebas[i].x, dz = z - AreaBebas[i].y;
                float r = semak && i == 0 ? 5.8f : semak && i == 1 ? 9.5f : AreaBebas[i].z;
                if (dx * dx + dz * dz < r * r) return false;
            }
            if (semak)
            {
                float dhx = x - 8f, dhz = z + 448f;
                if (dhx * dhx + dhz * dhz < 5.8f * 5.8f) return false; // badan rongsokan heli saja
            }
            else
            {
                if ((x + 4f) * (x + 4f) + (z + 424f) * (z + 424f) < 16f) return false; // SpawnAndi
                if ((x - 24f) * (x - 24f) + (z + 440f) * (z + 440f) < 12f) return false; // KangDeden
                for (int k = 0; k < PosisiKoper.Length; k++)
                {
                    float kx = x - PosisiKoper[k].x, kz = z - PosisiKoper[k].z;
                    if (kx * kx + kz * kz < 9f) return false;
                }
            }
            return WorldTerrainGenerator.SampleSlopeDeg(x, z) <= LerengMaksVeg;
        }

        static VegetationData.Sel[] TempatkanVegetasi(int nJenis)
        {
            // Kunci: indeks jenis + sel 125 m -> daftar matriks.
            var kelompok = new Dictionary<long, List<Matrix4x4>>();
            var idK = new Dictionary<long, List<int>>();
            int idGlobal = 0;
            Action<int, Vector3, float, float> tambah = (jenis, pos, rotY, skala) =>
            {
                int sx = Mathf.FloorToInt((pos.x + WorldTerrainGenerator.BatasDunia) / UkuranSel), sz = Mathf.FloorToInt((pos.z + WorldTerrainGenerator.BatasDunia) / UkuranSel);
                long kunci = ((long)jenis << 32) | (long)(sx * 1000 + sz);
                List<Matrix4x4> l;
                if (!kelompok.TryGetValue(kunci, out l)) { l = new List<Matrix4x4>(); kelompok[kunci] = l; idK[kunci] = new List<int>(); }
                l.Add(Matrix4x4.TRS(pos, Quaternion.Euler(0f, rotY, 0f), Vector3.one * skala));
                idK[kunci].Add(idGlobal++);
            };

            int haSisi = Mathf.RoundToInt(WorldTerrainGenerator.BatasDunia * 2f / 100f);
            for (int hz = 0; hz < haSisi; hz++)
                for (int hx = 0; hx < haSisi; hx++)
                {
                    var r = new System.Random(1337 + hx * 73856093 ^ hz * 19349663);
                    float ox = -WorldTerrainGenerator.BatasDunia + hx * 100f, oz = -WorldTerrainGenerator.BatasDunia + hz * 100f;
                    Tanam(r, ox, oz, CalonPohonPerHa, 1f, false, tambah);
                    Tanam(r, ox, oz, CalonSemakPerHa, FaktorSemak, true, tambah);
                }

            // Sawit hangus: grid 8 m di area sawit (03 §9 Zona 4, dikali 2).
            for (float x = -680f; x <= -370f; x += JarakGridSawit)
                for (float z = 100f; z <= 390f; z += JarakGridSawit)
                {
                    float y;
                    if (WorldTerrainGenerator.GetBiome(x, z) != Biome.SawitTerbakar || !BolehTanam(x, z, out y)) continue;
                    var r = new System.Random((int)(x * 31 + z * 17));
                    tambah((int)J.SawitHangus * Varian + r.Next(Varian), new Vector3(x + (float)r.NextDouble() - 0.5f, y - 0.2f, z), (float)r.NextDouble() * 360f, 0.85f + 0.3f * (float)r.NextDouble());
                }

            var hasil = new List<VegetationData.Sel>();
            foreach (var kv in kelompok)
            {
                var semua = kv.Value; var ids = idK[kv.Key];
                for (int a = 0; a < semua.Count; a += VegetationManager.MaksBatch)
                {
                    int n = Mathf.Min(VegetationManager.MaksBatch, semua.Count - a);
                    var m = semua.GetRange(a, n).ToArray();
                    Vector3 min = Vector3.positiveInfinity, max = Vector3.negativeInfinity;
                    for (int i = 0; i < n; i++) { Vector3 p = m[i].GetColumn(3); min = Vector3.Min(min, p); max = Vector3.Max(max, p); }
                    hasil.Add(new VegetationData.Sel { jenis = (int)(kv.Key >> 32), pusat = (min + max) * 0.5f, radius = (max - min).magnitude * 0.5f + 15f, matriks = m, idGlobal = ids.GetRange(a, n).ToArray() });
                }
            }
            return hasil.ToArray();
        }

        static void Tanam(System.Random r, float ox, float oz, int calon, float faktor, bool semak, Action<int, Vector3, float, float> tambah)
        {
            for (int c = 0; c < calon; c++)
            {
                float x = ox + (float)r.NextDouble() * 100f, z = oz + (float)r.NextDouble() * 100f;
                double undian = r.NextDouble() * calon;
                float y;
                int biome = (int)WorldTerrainGenerator.GetBiome(x, z);
                var tabel = semak ? TabelSemak(biome) : TabelPohon(biome);
                if (tabel.Length == 0) continue;
                double kum = 0; int pilih = -1;
                for (int i = 0; i < tabel.Length; i++) { kum += tabel[i].perHa * faktor; if (undian < kum) { pilih = i; break; } }
                if (pilih < 0 || !BolehTanam(x, z, out y, semak)) continue;
                var jn = tabel[pilih].jenis;
                float skala = semak ? 0.7f + 0.7f * (float)r.NextDouble() : 0.8f + 0.45f * (float)r.NextDouble();
                tambah((int)jn * Varian + r.Next(Varian), new Vector3(x, y - (semak ? 0.05f : 0.25f), z), (float)r.NextDouble() * 360f, skala);
            }
        }
    }
}
