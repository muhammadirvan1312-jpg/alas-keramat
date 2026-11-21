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
        const float UkuranUbin = 6f, LerengTanah = 32f, LerengBatu = 44f;
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
                float ubin = i == L_Batu ? 5.2f : i == L_Pasir ? 4.2f : i == L_Daun ? 2.8f : i == L_Rumput ? 2.6f : 2.6f;
                var tl = new TerrainLayer
                {
                    diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/" + LapisTerrain[i] + ".png"),
                    normalMapTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/" + LapisTerrain[i] + "_n.png"),
                    maskMapTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/" + LapisTerrain[i] + "_mask.png"),
                    normalScale = i == L_Batu || i == L_Daun ? 1.45f : i == L_Rumput || i == L_Lumpur ? 1.25f : 1.05f,
                    tileSize = new Vector2(ubin, ubin),
                    smoothness = i == L_Lumpur ? 0.38f : i == L_Batu ? 0.16f : 0.08f
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
            t.heightmapPixelError = 7f;
            t.basemapDistance = 220f;
            t.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
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
                    float variasi = Mathf.PerlinNoise(wx * 0.045f + 3f, wz * 0.045f + 7f);
                    float mikro = Mathf.PerlinNoise(wx * 0.18f + 19f, wz * 0.18f + 41f);
                    float lembap = Mathf.Clamp01((6f - WorldTerrainGenerator.SampleHeight(wx, wz)) * 0.08f);
                    w[L_Daun] *= 0.65f + 0.70f * variasi + 0.25f * mikro;
                    w[L_Rumput] *= 1.15f - 0.60f * variasi + 0.25f * (1f - mikro);
                    w[L_Tanah] += 0.14f * Mathf.Clamp01(mikro - 0.52f);
                    w[L_Lumpur] += 0.22f * lembap * (1f - kBatu);
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
                case (int)Biome.KawahBeringin: w[L_Tanah] = 0.25f; w[L_Daun] = 0.55f; w[L_Rumput] = 0.20f; break;
                case (int)Biome.TebingKapur: w[L_Batu] = 0.65f; w[L_Rumput] = 0.25f; w[L_Tanah] = 0.10f; break;
                case (int)Biome.SawitTerbakar: w[L_Lumpur] = 0.5f; w[L_Tanah] = 0.5f; break;
                case (int)Biome.Desa: w[L_Tanah] = 0.48f; w[L_Daun] = 0.20f; w[L_Rumput] = 0.32f; break;
                case (int)Biome.HutanBambu: w[L_Daun] = 0.62f; w[L_Rumput] = 0.28f; w[L_Tanah] = 0.10f; break;
                case (int)Biome.TepiSungai: w[L_Daun] = 0.46f; w[L_Lumpur] = 0.22f; w[L_Rumput] = 0.24f; w[L_Tanah] = 0.08f; break;
                case (int)Biome.HutanJatiSelatan: w[L_Daun] = 0.54f; w[L_Rumput] = 0.30f; w[L_Tanah] = 0.16f; break;
                case 11: w[L_Lumpur] = 0.8f; w[L_Daun] = 0.2f; break;      // Rawa Bakau
                case 12: w[L_Tanah] = 0.7f; w[L_Batu] = 0.3f; break;       // Pabrik Gula
                case 13: w[L_Batu] = 0.5f; w[L_Rumput] = 0.5f; break;      // Gunung Kapur
                case 14: w[L_Rumput] = 0.6f; w[L_Tanah] = 0.4f; break;     // Ladang Tebu
                case 15: w[L_Daun] = 0.7f; w[L_Lumpur] = 0.3f; break;      // Alas Wingit
                default: w[L_Daun] = 0.58f; w[L_Rumput] = 0.30f; w[L_Tanah] = 0.12f; break;    // Hutan Campuran
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
            var mr = go.AddComponent<MeshRenderer>();
            var mLaut = AirAK("M_Laut", ProcTex.Hex("#1F6B6A55"), ProcTex.Hex("#04202BF0"), 0.12f, new Vector2(0.8f, 0.6f));
            if (mLaut == null) mLaut = MatAir("M_Laut", ProcTex.Hex("#1C4652D9"), 0.92f);
            mr.sharedMaterial = mLaut;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        static void BuatSungai(GameObject dunia)
        {
            var mat = AirAK("M_Sungai", ProcTex.Hex("#3D6B4E50"), ProcTex.Hex("#10261CE6"), 0.6f, new Vector2(1f, 0f));
            if (mat == null) mat = MatAir("M_Sungai", ProcTex.Hex("#2E4A3ECC"), 0.88f);
            var pSh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            Material mJeram = pSh != null ? new Material(pSh) { name = "M_BuihJeram" }
                : SafeMaterialFactory.CreatePBR("M_BuihJeram", new Color(0.92f, 0.96f, 1f, 0.45f), 0f, 0.2f, null, null, null, SurfaceMode.Transparent);
            if (pSh != null)
            {
                SafeMaterialFactory.SetF(mJeram, "_Surface", 1f);
                SafeMaterialFactory.SetF(mJeram, "_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                SafeMaterialFactory.SetF(mJeram, "_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                SafeMaterialFactory.SetF(mJeram, "_ZWrite", 0f);
                mJeram.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); mJeram.renderQueue = 3000;
                SafeMaterialFactory.SetC(mJeram, "_BaseColor", new Color(0.92f, 0.96f, 1f, 0.42f));
            }
            AssetDatabase.CreateAsset(mJeram, FolderMat + "/M_BuihJeram.mat");
            mJeram = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_BuihJeram.mat");
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
                var mrs = go.AddComponent<MeshRenderer>();
                mrs.sharedMaterial = mat;
                mrs.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                for (int i = 0; i < titik.Length - 1; i++)
                {
                    var a = titik[i]; var b = titik[i + 1];
                    if (a.wl - b.wl < 1.2f) continue;
                    var jp = new GameObject("JeramSungai_" + s + "_" + i);
                    jp.transform.SetParent(go.transform, false);
                    jp.transform.position = new Vector3((a.x + b.x) * 0.5f * WorldTerrainGenerator.SkalaXZ,
                        (a.wl + b.wl) * 0.5f * WorldTerrainGenerator.SkalaTinggi + 0.15f,
                        (a.z + b.z) * 0.5f * WorldTerrainGenerator.SkalaXZ);
                    var ps = jp.AddComponent<ParticleSystem>();
                    var pm = ps.main;
                    pm.loop = true; pm.startLifetime = 0.75f; pm.startSpeed = 1.6f; pm.startSize = 0.85f;
                    pm.maxParticles = 56; pm.gravityModifier = 0.25f; pm.startColor = new Color(0.90f, 0.96f, 1f, 0.42f);
                    var em = ps.emission; em.rateOverTime = 22f;
                    var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box;
                    sh.scale = new Vector3(Mathf.Max(3f, a.w * 0.8f), 0.25f, 3.2f);
                    var pr = jp.GetComponent<ParticleSystemRenderer>();
                    pr.sharedMaterial = mJeram; pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
        }
    }
}
