using System;
using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;

namespace AlasKeramat.EditorTools
{
    // Langkah 3 (T2a): 16 tile Unity Terrain 2x2 km dari WorldTerrainGenerator, splat per bioma & lereng, laut, sungai.
    public static partial class NusantaraMasterBuilder
    {
        public const string FolderTerrain = FolderGen + "/Terrain";
        public const int ResolusiHeight = 1025, ResolusiSplat = 512;
        const float UkuranUbin = 6f, LerengTanah = 28f, LerengBatu = 40f;
        const float UkuranLaut = 6000f, LebarSungaiTambah = 1f, LangkahSungai = 4f;

        // Urutan lapisan terrain.
        static readonly string[] LapisTerrain = { "rumput", "tanah", "batu", "pasir", "lumpur", "daun" };
        const int L_Rumput = 0, L_Tanah = 1, L_Batu = 2, L_Pasir = 3, L_Lumpur = 4, L_Daun = 5;

        public static readonly List<Terrain> TerrainDibuat = new List<Terrain>();

        static void LangkahDunia()
        {
            EnsureFolder(FolderTerrain);
            var dunia = new GameObject("World");
            dunia.transform.SetParent(_root.transform, false);
            var induk = new GameObject("Terrain");
            induk.transform.SetParent(dunia.transform, false);

            var layers = BuatTerrainLayers();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit")) { name = "M_Terrain" };
            AssetDatabase.CreateAsset(mat, FolderMat + "/M_Terrain.mat");

            TerrainDibuat.Clear();
            int n = WorldTerrainGenerator.TileSisi;
            var grid = new Terrain[n, n];
            for (int tz = 0; tz < n; tz++)
                for (int tx = 0; tx < n; tx++)
                {
                    EditorUtility.DisplayProgressBar("Nusantara Builder", "Terrain " + (tz * n + tx + 1) + "/16", 0.3f + 0.3f * (tz * n + tx) / (n * n));
                    grid[tx, tz] = BuatTile(induk, tx, tz, layers, mat);
                    TerrainDibuat.Add(grid[tx, tz]);
                }
            for (int tz = 0; tz < n; tz++)
                for (int tx = 0; tx < n; tx++)
                    grid[tx, tz].SetNeighbors(tx > 0 ? grid[tx - 1, tz] : null, tz < n - 1 ? grid[tx, tz + 1] : null,
                                              tx < n - 1 ? grid[tx + 1, tz] : null, tz > 0 ? grid[tx, tz - 1] : null);
            BuatLaut(dunia);
            BuatSungai(dunia);
            Catat("Terrain " + TerrainDibuat.Count + " tile @" + ResolusiHeight + ", laut, " + WorldTerrainGenerator.SemuaSungai.Length + " sungai");
        }

        static TerrainLayer[] BuatTerrainLayers()
        {
            var hasil = new TerrainLayer[LapisTerrain.Length];
            for (int i = 0; i < LapisTerrain.Length; i++)
            {
                var tl = new TerrainLayer
                {
                    diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/" + LapisTerrain[i] + ".png"),
                    normalMapTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/" + LapisTerrain[i] + "_n.png"),
                    tileSize = new Vector2(UkuranUbin, UkuranUbin),
                    smoothness = i == L_Lumpur ? 0.35f : 0.1f
                };
                if (tl.diffuseTexture == null) throw new Exception("tekstur terrain hilang: " + LapisTerrain[i]);
                string p = FolderTerrain + "/L_" + LapisTerrain[i] + ".terrainlayer";
                AssetDatabase.CreateAsset(tl, p);
                hasil[i] = AssetDatabase.LoadAssetAtPath<TerrainLayer>(p);
            }
            return hasil;
        }

        static Terrain BuatTile(GameObject induk, int tx, int tz, TerrainLayer[] layers, Material mat)
        {
            float ox = -WorldTerrainGenerator.BatasDunia + tx * WorldTerrainGenerator.UkuranTile;
            float oz = -WorldTerrainGenerator.BatasDunia + tz * WorldTerrainGenerator.UkuranTile;
            var data = new TerrainData { heightmapResolution = ResolusiHeight };
            data.size = new Vector3(WorldTerrainGenerator.UkuranTile, WorldTerrainGenerator.TinggiMaks, WorldTerrainGenerator.UkuranTile);
            var h = new float[ResolusiHeight, ResolusiHeight];
            float langkah = WorldTerrainGenerator.UkuranTile / (ResolusiHeight - 1);
            for (int z = 0; z < ResolusiHeight; z++)
                for (int x = 0; x < ResolusiHeight; x++)
                {
                    float y = WorldTerrainGenerator.SampleHeight(ox + x * langkah, oz + z * langkah);
                    h[z, x] = Mathf.Clamp01((y - WorldTerrainGenerator.DasarY) / WorldTerrainGenerator.TinggiMaks);
                }
            data.SetHeights(0, 0, h);
            data.terrainLayers = layers;
            data.alphamapResolution = ResolusiSplat;
            data.SetAlphamaps(0, 0, HitungSplat(ox, oz, h, langkah));
            string p = FolderTerrain + "/T_" + tx + "_" + tz + ".asset";
            AssetDatabase.CreateAsset(data, p);
            data = AssetDatabase.LoadAssetAtPath<TerrainData>(p);

            var go = Terrain.CreateTerrainGameObject(data);
            go.name = "Terrain_" + tx + "_" + tz;
            go.layer = GameLayers.Ground;
            go.transform.SetParent(induk.transform, false);
            go.transform.position = new Vector3(ox, WorldTerrainGenerator.DasarY, oz);
            var t = go.GetComponent<Terrain>();
            t.materialTemplate = mat;
            t.drawInstanced = true;
            t.heightmapPixelError = 5f;
            t.basemapDistance = 160f;
            return t;
        }

        static float[,,] HitungSplat(float ox, float oz, float[,] hNorm, float langkahH)
        {
            int r = ResolusiSplat, nL = LapisTerrain.Length;
            var a = new float[r, r, nL];
            float langkah = WorldTerrainGenerator.UkuranTile / r;
            var w = new float[nL];
            for (int z = 0; z < r; z++)
                for (int x = 0; x < r; x++)
                {
                    float wx = ox + (x + 0.5f) * langkah, wz = oz + (z + 0.5f) * langkah;
                    Array.Clear(w, 0, nL);
                    BobotBioma(WorldTerrainGenerator.GetBiome(wx, wz), w);
                    float lereng = LerengDariPeta(hNorm, (wx - ox) / langkahH, (wz - oz) / langkahH, langkahH);
                    float kTanah = WorldTerrainGenerator.S(LerengTanah - 6f, LerengTanah + 4f, lereng);
                    float kBatu = WorldTerrainGenerator.S(LerengBatu - 6f, LerengBatu + 4f, lereng);
                    for (int i = 0; i < nL; i++) w[i] *= 1f - Mathf.Max(kTanah, kBatu);
                    w[L_Tanah] += kTanah * (1f - kBatu);
                    w[L_Batu] += kBatu;
                    float variasi = Mathf.PerlinNoise(wx * 0.05f + 3f, wz * 0.05f + 7f);
                    w[L_Daun] *= 0.6f + 0.8f * variasi;
                    w[L_Rumput] *= 1.4f - 0.8f * variasi;
                    float total = 0f;
                    for (int i = 0; i < nL; i++) total += w[i];
                    if (total < 1e-4f) { w[L_Tanah] = 1f; total = 1f; }
                    for (int i = 0; i < nL; i++) a[z, x, i] = w[i] / total;
                }
            return a;
        }

        // Lereng (derajat) dari peta tinggi tile yang sudah dihitung (lebih cepat dari SampleSlopeDeg).
        static float LerengDariPeta(float[,] hNorm, float fx, float fz, float langkah)
        {
            int n = hNorm.GetLength(0);
            int x = Mathf.Clamp(Mathf.RoundToInt(fx), 1, n - 2), z = Mathf.Clamp(Mathf.RoundToInt(fz), 1, n - 2);
            float k = WorldTerrainGenerator.TinggiMaks;
            float dx = (hNorm[z, x + 1] - hNorm[z, x - 1]) * k / (2f * langkah);
            float dz = (hNorm[z + 1, x] - hNorm[z - 1, x]) * k / (2f * langkah);
            return Mathf.Atan(Mathf.Sqrt(dx * dx + dz * dz)) * Mathf.Rad2Deg;
        }

        static void BobotBioma(Biome b, float[] w)
        {
            switch ((int)b)
            {
                case (int)Biome.Laut: case (int)Biome.Pantai: w[L_Pasir] = 1f; break;
                case (int)Biome.KawahBeringin: w[L_Tanah] = 0.5f; w[L_Daun] = 0.5f; break;
                case (int)Biome.TebingKapur: w[L_Batu] = 0.6f; w[L_Rumput] = 0.4f; break;
                case (int)Biome.SawitTerbakar: w[L_Lumpur] = 0.5f; w[L_Tanah] = 0.5f; break;
                case (int)Biome.Desa: w[L_Tanah] = 0.6f; w[L_Rumput] = 0.4f; break;
                case (int)Biome.HutanBambu: w[L_Daun] = 0.6f; w[L_Rumput] = 0.4f; break;
                case (int)Biome.TepiSungai: w[L_Lumpur] = 0.4f; w[L_Rumput] = 0.6f; break;
                case (int)Biome.HutanJatiSelatan: w[L_Daun] = 0.7f; w[L_Tanah] = 0.3f; break;
                case 11: w[L_Lumpur] = 0.8f; w[L_Daun] = 0.2f; break;      // Rawa Bakau
                case 12: w[L_Tanah] = 0.7f; w[L_Batu] = 0.3f; break;       // Pabrik Gula
                case 13: w[L_Batu] = 0.5f; w[L_Rumput] = 0.5f; break;      // Gunung Kapur
                case 14: w[L_Rumput] = 0.6f; w[L_Tanah] = 0.4f; break;     // Ladang Tebu
                case 15: w[L_Daun] = 0.7f; w[L_Lumpur] = 0.3f; break;      // Alas Wingit
                default: w[L_Rumput] = 0.55f; w[L_Daun] = 0.45f; break;    // Hutan Campuran
            }
        }

        static Material MatAir(string nama, Color warna, float halus)
        {
            var m = SafeMaterialFactory.CreatePBR(nama, warna, 0f, halus, null, AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/batu_n.png"), null, SurfaceMode.Transparent);
            string p = FolderMat + "/" + nama + ".mat";
            AssetDatabase.CreateAsset(m, p);
            return AssetDatabase.LoadAssetAtPath<Material>(p);
        }

        static void BuatLaut(GameObject dunia)
        {
            var m = ProcMesh.Grid("Laut", UkuranLaut, 60, 400f);
            AssetDatabase.CreateAsset(m, FolderMesh + "/Laut.asset");
            var go = new GameObject("Laut");
            go.layer = GameLayers.Water;
            go.transform.SetParent(dunia.transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = m;
            go.AddComponent<MeshRenderer>().sharedMaterial = MatAir("M_Laut", ProcTex.Hex("#1C4652D9"), 0.92f);
        }

        static void BuatSungai(GameObject dunia)
        {
            var mat = MatAir("M_Sungai", ProcTex.Hex("#2E4A3ECC"), 0.88f);
            for (int s = 0; s < WorldTerrainGenerator.SemuaSungai.Length; s++)
            {
                var titik = WorldTerrainGenerator.SemuaSungai[s];
                var v = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>();
                float jarakUV = 0f;
                for (int i = 0; i < titik.Length - 1; i++)
                {
                    var a = titik[i]; var b = titik[i + 1];
                    Vector2 pa = new Vector2(a.x, a.z) * WorldTerrainGenerator.SkalaXZ, pb = new Vector2(b.x, b.z) * WorldTerrainGenerator.SkalaXZ;
                    Vector2 dir = (pb - pa).normalized, kiri = new Vector2(-dir.y, dir.x);
                    int langkah = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(pa, pb) / LangkahSungai));
                    for (int k = (i == 0 ? 0 : 1); k <= langkah; k++)
                    {
                        float t = (float)k / langkah;
                        Vector2 p = Vector2.Lerp(pa, pb, t);
                        float lebar = (Mathf.Lerp(a.w, b.w, t) + LebarSungaiTambah) * WorldTerrainGenerator.SkalaXZ * 0.5f;
                        float y = Mathf.Lerp(a.wl, b.wl, t) * WorldTerrainGenerator.SkalaTinggi;
                        v.Add(new Vector3(p.x + kiri.x * lebar, y, p.y + kiri.y * lebar));
                        v.Add(new Vector3(p.x - kiri.x * lebar, y, p.y - kiri.y * lebar));
                        uv.Add(new Vector2(0f, jarakUV / 10f)); uv.Add(new Vector2(1f, jarakUV / 10f));
                        jarakUV += Vector2.Distance(pa, pb) / langkah;
                        int n = v.Count;
                        if (n >= 4) { tri.Add(n - 4); tri.Add(n - 2); tri.Add(n - 3); tri.Add(n - 3); tri.Add(n - 2); tri.Add(n - 1); }
                    }
                }
                var mesh = new Mesh { name = "Sungai_" + s };
                mesh.SetVertices(v); mesh.SetUVs(0, uv); mesh.SetTriangles(tri, 0);
                mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds();
                AssetDatabase.CreateAsset(mesh, FolderMesh + "/Sungai_" + s + ".asset");
                var go = new GameObject("Sungai_" + s);
                go.layer = GameLayers.Water;
                go.transform.SetParent(dunia.transform, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            }
        }
    }
}
