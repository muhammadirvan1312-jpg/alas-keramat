using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;

namespace AlasKeramat.EditorTools
{
    // Langkah 3d (T2c): landmark zona (03 §9 dikali 2, 20 §2.2) — Beringin Agung, heli, desa, kampung, mercusuar, pabrik gula, gua, altar.
    public static partial class NusantaraMasterBuilder
    {
        public const string FolderLandmark = FolderGen + "/Landmark";
        public static int JumlahLandmark;

        static GameObject _induk;

        static void LangkahLandmark()
        {
            EnsureFolder(FolderLandmark);
            var dunia = GameObject.Find(NamaRoot + "/World");
            _induk = Anak(dunia != null ? dunia : _root, "Landmark");
            JumlahLandmark = 0;
            Material kayu = AmbilMat("M_Kayu"), seng = AmbilMat("M_LogamKarat"), batu = AmbilMat("M_Batu");
            var bata = SimpanMat(SafeMaterialFactory.CreatePBR("M_Bata", ProcTex.Hex("#B0583E"), 0f, 0.15f, Tex("beton"), Tex("beton_n")));
            var putihTex = TeksturPanelHeli("Tex_HeliPutih", ProcTex.Hex("#CFCBC2"), ProcTex.Hex("#6C675E"));
            var oranyeTex = TeksturPanelHeli("Tex_HeliOranye", ProcTex.Hex("#DF6116"), ProcTex.Hex("#7A3310"));
            var putih = SimpanMat(SafeMaterialFactory.CreatePBR("M_CatPutih", Color.white, 0.35f, 0.42f, putihTex, Tex("logam_karat_n")));
            MaskPBR(putih, "logam_karat");
            var oranye = SimpanMat(SafeMaterialFactory.CreatePBR("M_CatOranye", Color.white, 0.32f, 0.44f, oranyeTex, Tex("logam_karat_n")));
            MaskPBR(oranye, "logam_karat");
            var merah = SimpanMat(SafeMaterialFactory.CreatePBR("M_CatMerahPudar", ProcTex.Hex("#8E3A30"), 0f, 0.3f, Tex("beton")));
            var gelap = SimpanMat(SafeMaterialFactory.CreatePBR("M_LogamGelap", ProcTex.Hex("#1A1E24"), 0.78f, 0.82f));
            var hitam = SimpanMat(SafeMaterialFactory.CreatePBR("M_GelapGua", Color.black, 0f, 0f));
            var ijuk = SimpanMat(SafeMaterialFactory.CreatePBR("M_AtapIjuk", ProcTex.Hex("#3A2E22"), 0f, 0.1f, Tex("kulit_kayu")));

            // Zona 5: Beringin Agung (28 m) + altar.
            var beringin = ProcPohon.Buat(ProcPohon.Jenis.BeringinKecil, 7, false);
            Taruh("BeringinAgung", Simpan(beringin), new[] { AmbilMat("M_KulitPohon"), AmbilMat("M_DaunPohon") }, 0f, 0f, 0f, new Vector3(2.6f, 2.2f, 2.6f), false);
            Taruh("Altar", Simpan(ProcBangunan.Altar()), new[] { batu }, 0f, -18f, 180f, Vector3.one, true);

            // Zona 1: heli rusak (yaw 35, roll 18) + bara turbin & asap kecelakaan.
            var heli = Taruh("HeliRusak", Simpan(ProcBangunan.HeliRusak()), new[] { putih, oranye, gelap }, 8f, -448f, 35f, Vector3.one, true);
            heli.transform.position += Vector3.up * 1.0f;
            heli.transform.rotation = Quaternion.Euler(0f, 35f, 18f);
            var bara = Anak(heli, "BaraTurbin");
            bara.transform.localPosition = new Vector3(0f, 1.45f, 1.3f);
            var lBara = bara.AddComponent<Light>();
            lBara.type = LightType.Point; lBara.range = 6f; lBara.intensity = 1.5f; lBara.color = ProcTex.Hex("#FF6418"); lBara.shadows = LightShadows.None;
            const int ssz = 64;
            var sTex = new Texture2D(ssz, ssz, TextureFormat.RGBA32, true);
            var spx = new Color[ssz * ssz];
            for (int sy = 0; sy < ssz; sy++) for (int sx = 0; sx < ssz; sx++)
            {
                float u = (float)sx / ssz - 0.5f, v = (float)sy / ssz - 0.5f;
                float r = Mathf.Sqrt(u * u + v * v) * 2f;
                float n = Mathf.PerlinNoise(sx * 0.14f + 2.3f, sy * 0.14f + 5.7f);
                float a = Mathf.Clamp01(1f - r + (n - 0.45f) * 0.35f) * Mathf.SmoothStep(1f, 0.25f, r);
                spx[sy * ssz + sx] = new Color(0.85f, 0.84f, 0.82f, a * a);
            }
            sTex.SetPixels(spx); sTex.Apply();
            string pAsapTex = FolderLandmark + "/Tex_AsapHeli.png";
            SimpanPng(sTex, pAsapTex); AssetDatabase.ImportAsset(pAsapTex); AturImpor(pAsapTex, false, false, ssz);
            var simp = (TextureImporter)AssetImporter.GetAtPath(pAsapTex); simp.alphaIsTransparency = true; simp.wrapMode = TextureWrapMode.Clamp; simp.SaveAndReimport();
            sTex = AssetDatabase.LoadAssetAtPath<Texture2D>(pAsapTex);
            string pAsapMat = FolderMat + "/M_AsapHeli.mat";
            AssetDatabase.DeleteAsset(pAsapMat);
            var pSh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            Material mAsap = pSh != null ? new Material(pSh) { name = "M_AsapHeli" }
                : SafeMaterialFactory.CreatePBR("M_AsapHeli", new Color(0.22f, 0.21f, 0.20f, 0.45f), 0f, 0.05f, sTex, null, null, SurfaceMode.Transparent);
            if (pSh != null)
            {
                SafeMaterialFactory.SetF(mAsap, "_Surface", 1f);
                SafeMaterialFactory.SetF(mAsap, "_Blend", 0f);
                SafeMaterialFactory.SetF(mAsap, "_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                SafeMaterialFactory.SetF(mAsap, "_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                SafeMaterialFactory.SetF(mAsap, "_ZWrite", 0f);
                mAsap.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); mAsap.renderQueue = 3000;
                SafeMaterialFactory.SetT(mAsap, "_BaseMap", sTex);
                SafeMaterialFactory.SetC(mAsap, "_BaseColor", Color.white);
            }
            AssetDatabase.CreateAsset(mAsap, pAsapMat);
            mAsap = AssetDatabase.LoadAssetAtPath<Material>(pAsapMat);
            var pAsap = bara.AddComponent<ParticleSystem>();
            var main = pAsap.main; main.startLifetime = 4.6f; main.startSpeed = 1.25f; main.startSize = 2.3f; main.startColor = new Color(0.14f, 0.13f, 0.12f, 0.56f); main.gravityModifier = -0.07f; main.maxParticles = 48; main.prewarm = true;
            var em = pAsap.emission; em.rateOverTime = 10f;
            var shp = pAsap.shape; shp.shapeType = ParticleSystemShapeType.Sphere; shp.radius = 0.45f;
            var col = pAsap.colorOverLifetime; col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(new[] { new GradientColorKey(new Color(0.85f, 0.55f, 0.35f), 0f), new GradientColorKey(new Color(0.28f, 0.28f, 0.30f), 0.25f), new GradientColorKey(new Color(0.7f, 0.7f, 0.72f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.95f, 0.18f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
            var pr = pAsap.GetComponent<ParticleSystemRenderer>(); pr.sharedMaterial = mAsap; pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            DandanLokasiHeli(8f, -448f);

            // Zona 3: Desa Mati (6 rumah, dikali 2 dari 03).
            float[,] desa = { { 564, 224, 200, 0.3f }, { 524, 236, 180, 0.4f }, { 592, 176, 260, 0.3f }, { 544, 140, 330, 0.4f }, { 504, 200, 120, 0.85f }, { 580, 260, 210, 0.9f } };
            for (int i = 0; i < desa.GetLength(0); i++)
                Taruh("RumahDesa_" + (i + 1), Simpan(ProcBangunan.RumahPanggung(100 + i, i == 0 ? 8f : 7f, i == 0 ? 6f : 5.5f, 1.5f, desa[i, 3])), new[] { kayu, i % 2 == 0 ? seng : ijuk }, desa[i, 0], desa[i, 1], desa[i, 2], Vector3.one, true);

            // Zona 11: Kampung Sukamaju (22 rumah seragam, ditinggal mendadak).
            for (int i = 0; i < 22; i++)
            {
                int baris = i / 6, kolom = i % 6;
                float x = 560f + kolom * 26f + (baris % 2) * 8f, z = 560f + baris * 30f;
                Taruh("RumahKampung_" + (i + 1), Simpan(ProcBangunan.RumahPanggung(300 + i, 6.5f, 5f, 0.6f, 0.15f)), new[] { kayu, seng }, x, z, baris % 2 == 0 ? 180f : 0f, Vector3.one, true);
            }

            // Zona 7: Mercusuar.
            Taruh("Mercusuar", Simpan(ProcBangunan.Mercusuar()), new[] { putih, merah, gelap }, -190f, -876f, 0f, Vector3.one, true);

            // Zona 9: Pabrik Gula Sinar Tebu.
            Taruh("Cerobong", Simpan(ProcBangunan.GedungPabrik(9, 0, 0, 0, true)), new[] { bata, seng, kayu }, 700f, -540f, 0f, Vector3.one, true);
            Taruh("GedungGiling", Simpan(ProcBangunan.GedungPabrik(10, 30f, 16f, 7f, false)), new[] { bata, seng, kayu }, 640f, -520f, 10f, Vector3.one, true);
            Taruh("GudangGula", Simpan(ProcBangunan.GedungPabrik(11, 20f, 12f, 6f, false)), new[] { bata, seng, kayu }, 690f, -470f, 100f, Vector3.one, true);
            Taruh("KantorAdministratur", Simpan(ProcBangunan.GedungPabrik(12, 14f, 10f, 5f, false)), new[] { putih, seng, kayu }, 620f, -455f, 190f, Vector3.one, true);

            // Mulut gua: UG1 lembah bambu (menghadap timur ke sungai), UG3 celah kuil.
            Taruh("CaveEntrance_1", Simpan(ProcBangunan.MulutGua(1, 9f)), new[] { batu, hitam }, 256f, -276f, 90f, Vector3.one, true);
            Taruh("CaveEntrance_3", Simpan(ProcBangunan.MulutGua(3, 12f)), new[] { batu, hitam }, 0f, 694f, 180f, Vector3.one, true);
            Catat("Landmark: " + JumlahLandmark + " objek (Beringin Agung, heli, 6 rumah desa, 22 rumah kampung, mercusuar, pabrik gula, 2 mulut gua, altar)");
        }

        static Texture2D Tex(string nama) { return AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/" + nama + ".png"); }

        static Material AmbilMat(string nama)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/" + nama + ".mat");
            if (m == null) Debug.LogError("[AK][Builder] material tidak ditemukan: " + nama);
            return m;
        }

        static Mesh Simpan(Mesh m)
        {
            string p = FolderLandmark + "/" + m.name + ".asset";
            AssetDatabase.DeleteAsset(p);
            AssetDatabase.CreateAsset(m, p);
            return AssetDatabase.LoadAssetAtPath<Mesh>(p);
        }

        static Texture2D TeksturPanelHeli(string nama, Color cat, Color kotor)
        {
            const int sz = 256;
            var rust = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/logam_karat.png");
            var t = new Texture2D(sz, sz, TextureFormat.RGBA32, true);
            var px = new Color[sz * sz];
            for (int y = 0; y < sz; y++) for (int x = 0; x < sz; x++)
            {
                float u = (float)x / sz, v = (float)y / sz;
                float rl = rust != null ? rust.GetPixelBilinear(u, v).grayscale : 0.5f;
                float panel = (Mathf.Abs(Mathf.Repeat(u * 3f, 1f) - 0.5f) > 0.485f || Mathf.Abs(Mathf.Repeat(v * 3f, 1f) - 0.5f) > 0.485f) ? 0.55f : 1.0f;
                float soot = Mathf.Clamp01((Mathf.PerlinNoise(u * 9f + 1.4f, v * 9f + 4.1f) - 0.48f) * 2.2f);
                Color c = Color.Lerp(cat, kotor, soot * 0.65f + (1f - rl) * 0.20f) * panel;
                c.a = 1f; px[y * sz + x] = c;
            }
            t.SetPixels(px); t.Apply();
            string pt = FolderLandmark + "/" + nama + ".png";
            SimpanPng(t, pt); AssetDatabase.ImportAsset(pt); AturImpor(pt, false, false, sz);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(pt);
        }

        static void DandanLokasiHeli(float hx, float hz)
        {
            var mHangus = AmbilMat("M_Tanah");
            var bekas = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bekas.name = "JejakJatuhHeli"; Object.DestroyImmediate(bekas.GetComponent<Collider>());
            bekas.transform.SetParent(_induk.transform, false);
            bekas.transform.position = new Vector3(hx, WorldTerrainGenerator.SampleHeight(hx, hz) + 0.04f, hz + 0.6f);
            bekas.transform.rotation = Quaternion.Euler(90f, 35f, 0f);
            bekas.transform.localScale = new Vector3(7.5f, 11.5f, 1f);
            bekas.GetComponent<Renderer>().sharedMaterial = mHangus;

            var meshKayu = MuatPropScan("KayuTumbangScan");
            var matKayu = MaterialSolidScan("KayuTumbangAsli", "M_KayuTumbangScan", new Color(0.80f, 0.77f, 0.72f, 1f), 0.22f) ?? AmbilMat("M_Kayu");
            if (meshKayu != null)
            {
                Vector3[] posK = { new Vector3(-4.2f, 0f, -3.5f), new Vector3(4.5f, 0f, 3.2f) };
                for (int i = 0; i < posK.Length; i++)
                {
                    var g = Anak(_induk, "PohonPatahHeli_" + i);
                    float x = hx + posK[i].x, z = hz + posK[i].z;
                    g.transform.position = new Vector3(x, WorldTerrainGenerator.SampleHeight(x, z) - 0.05f, z);
                    g.transform.rotation = Quaternion.Euler(4f, 45f + i * 95f, -5f);
                    g.AddComponent<MeshFilter>().sharedMesh = meshKayu; g.AddComponent<MeshRenderer>().sharedMaterial = matKayu;
                }
            }
            var meshBatu = MuatPropScan("BatuBesarScan") ?? MuatPropScan("BatuLumutA");
            var matBatu = (meshBatu != null && meshBatu.name.Contains("BatuBesar")
                ? MaterialSolidScan("BatuBesarAsli", "M_BatuBesarScan", new Color(0.82f, 0.84f, 0.80f, 1f), 0.28f)
                : MaterialSolidScan("BatuLumutAsli", "M_BatuLumutScan", new Color(0.84f, 0.86f, 0.82f, 1f), 0.26f)) ?? AmbilMat("M_Batu");
            if (meshBatu != null)
            {
                Vector3[] posB = { new Vector3(-3.2f, 0f, 2.8f), new Vector3(3.4f, 0f, -2.6f), new Vector3(-1.5f, 0f, -4.4f), new Vector3(2.8f, 0f, 4.1f) };
                for (int i = 0; i < posB.Length; i++)
                {
                    var g = Anak(_induk, "BatuHeli_" + i);
                    float x = hx + posB[i].x, z = hz + posB[i].z;
                    g.transform.position = new Vector3(x, WorldTerrainGenerator.SampleHeight(x, z) - 0.1f, z);
                    g.transform.rotation = Quaternion.Euler(0f, i * 83f, 0f);
                    g.transform.localScale = Vector3.one * (0.65f + (i % 2) * 0.35f);
                    g.AddComponent<MeshFilter>().sharedMesh = meshBatu; g.AddComponent<MeshRenderer>().sharedMaterial = matBatu;
                }
            }
        }

        static GameObject Taruh(string nama, Mesh mesh, Material[] mats, float x, float z, float yaw, Vector3 skala, bool collider)
        {
            var go = Anak(_induk, nama);
            go.layer = GameLayers.Structure;
            go.tag = GameTags.StructurePiece;
            float y = WorldTerrainGenerator.SampleHeight(x, z);
            go.transform.position = new Vector3(x, y, z);
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            go.transform.localScale = skala;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterials = mats;
            if (collider) go.AddComponent<MeshCollider>().sharedMesh = mesh;
            JumlahLandmark++;
            return go;
        }
    }
}
