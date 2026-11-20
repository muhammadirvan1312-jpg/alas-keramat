using AlasKeramat.AI;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using J = AlasKeramat.Core.ResepKarakter.JenisKarakter;

namespace AlasKeramat.EditorTools
{
    // Langkah 7o (T7b): prefab hantu (Pocong Hutan, Kuntilanak, Banaspati), buntelan jarahan, bambu lempar, 6 Totem Topeng (3 Desa, 3 Sawit),
    // MusikTempur, sambungan EnemyDirector malam, diorama hantu untuk foto malam.
    public static partial class NusantaraMasterBuilder
    {
        public const string NamaDioramaHantu = "DioramaHantu";
        public static readonly Vector3[] PosTotem =
        {
            new Vector3(505f, 0f, 170f), new Vector3(575f, 0f, 165f), new Vector3(545f, 0f, 245f),
            new Vector3(-560f, 0f, 205f), new Vector3(-470f, 0f, 280f), new Vector3(-600f, 0f, 300f),
        };
        public static readonly Vector3 PosDioramaHantu = new Vector3(-60f, 0f, -300f);

        static void LangkahHantu()
        {
            EnsureFolder(FolderPrefabMusuh);
            var hantu = new GameObject[3];
            hantu[0] = PrefabHantuManusia(EnemyArchetype.PocongHutan, J.Pocong, ProceduralAnimator.Mode.Pocong);
            hantu[1] = PrefabHantuManusia(EnemyArchetype.Kuntilanak, J.Kuntilanak, ProceduralAnimator.Mode.Melayang);
            hantu[2] = PrefabBanaspati();
            var hantuTambahan = new GameObject[5];
            hantuTambahan[0] = PrefabHantuManusia(EnemyArchetype.Kuyang, J.Kuyang, ProceduralAnimator.Mode.Melayang);
            hantuTambahan[1] = PrefabHantuManusia(EnemyArchetype.Tuyul, J.Tuyul, ProceduralAnimator.Mode.Normal);
            hantuTambahan[2] = PrefabHantuManusia(EnemyArchetype.GenderuwoHutan, J.Genderuwo, ProceduralAnimator.Mode.Normal);
            hantuTambahan[3] = PrefabHantuManusia(EnemyArchetype.HantuAir, J.HantuAir, ProceduralAnimator.Mode.Normal);
            hantuTambahan[4] = PrefabHantuManusia(EnemyArchetype.Penyerupa, J.Penyerupa, ProceduralAnimator.Mode.Normal);
            var jarahan = PrefabJarahan();
            var bambu = PrefabBambu();
            var dir = Object.FindFirstObjectByType<EnemyDirector>();
            if (dir == null) { Debug.LogError("[AK][Builder] EnemyDirector belum ada (langkah 7n), hantu dilewati"); return; }
            dir.PasangMalam(hantu, jarahan, bambu);
            dir.PasangMalamTambahan(hantuTambahan);
            EditorUtility.SetDirty(dir);
            Anak(_sistem, "MusikTempur").AddComponent<MusikTempur>();

            var induk = Anak(_root, "TotemTopeng");
            var kayu = AmbilMat("M_Kayu"); var cat = AmbilMat("M_CatMerahPudar"); var batu = AmbilMat("M_Batu");
            for (int i = 0; i < PosTotem.Length; i++)
            {
                var t = Anak(induk, "Totem_" + i);
                float h = WorldTerrainGenerator.SampleHeight(PosTotem[i].x, PosTotem[i].z);
                t.transform.position = new Vector3(PosTotem[i].x, h, PosTotem[i].z);
                t.transform.rotation = Quaternion.Euler(0f, i * 57f, 0f);
                BagianPrimitif(t.transform, PrimitiveType.Cylinder, new Vector3(0f, 1.3f, 0f), new Vector3(0.22f, 1.3f, 0.22f), kayu, true);
                BagianPrimitif(t.transform, PrimitiveType.Cube, new Vector3(0f, 2.35f, 0.12f), new Vector3(0.42f, 0.55f, 0.08f), cat, false);   // topeng
                BagianPrimitif(t.transform, PrimitiveType.Cube, new Vector3(-0.1f, 2.42f, 0.17f), new Vector3(0.08f, 0.06f, 0.02f), batu, false); // lubang mata
                BagianPrimitif(t.transform, PrimitiveType.Cube, new Vector3(0.1f, 2.42f, 0.17f), new Vector3(0.08f, 0.06f, 0.02f), batu, false);
                BagianPrimitif(t.transform, PrimitiveType.Cube, new Vector3(0f, 2.75f, 0f), new Vector3(0.7f, 0.06f, 0.06f), kayu, false);       // palang tanduk
                var tt = t.AddComponent<TotemTopeng>();
                tt.Pasang(i);
                SetLayer(t, GameLayers.Enemy);
            }

            var dio = Anak(_root, NamaDioramaHantu);
            // Panggung horor malam: Pocong, Kuntilanak, Kuyang, Tuyul, Genderuwo Hutan, dan Banaspati dalam bulan purnama.
            var formasi = new[]
            {
                new Vector4(-2.3f, 0f, 0.2f, 168f),
                new Vector4(0.1f, 0.45f, 0.4f, 180f),
                new Vector4(2.1f, 0.65f, 0.2f, 194f),
                new Vector4(-1.05f, 0f, -1.5f, 172f),
                new Vector4(1.35f, 0f, 2.2f, 186f),
                new Vector4(-1.55f, 1.55f, -0.4f, 175f),
            };
            var dioramaPrefabs = new[] { hantu[0], hantu[1], hantuTambahan[0], hantuTambahan[1], hantuTambahan[2], hantu[2] };
            for (int i = 0; i < dioramaPrefabs.Length; i++)
            {
                if (dioramaPrefabs[i] == null) continue;
                var g = (GameObject)PrefabUtility.InstantiatePrefab(dioramaPrefabs[i]);
                g.transform.SetParent(dio.transform, false);
                float x = PosDioramaHantu.x + formasi[i].x, z = PosDioramaHantu.z + formasi[i].z;
                g.transform.position = new Vector3(x, WorldTerrainGenerator.SampleHeight(x, z) + formasi[i].y, z);
                g.transform.rotation = Quaternion.Euler(0f, formasi[i].w, 0f);
                var ai = g.GetComponent<HantuAI>(); if (ai != null) ai.enabled = false;
            }
            dio.SetActive(false);
            Catat("Hantu: Pocong, Kuntilanak, Banaspati, Kuyang, Tuyul, Genderuwo Hutan, Hantu Air, Penyerupa + 6 Totem Topeng");
        }

        static void BagianPrimitif(Transform induk, PrimitiveType tipe, Vector3 pos, Vector3 skala, Material mat, bool kolider)
        {
            var g = GameObject.CreatePrimitive(tipe);
            if (!kolider) Object.DestroyImmediate(g.GetComponent<Collider>());
            g.transform.SetParent(induk, false);
            g.transform.localPosition = pos; g.transform.localScale = skala;
            if (mat != null) g.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        static GameObject PrefabHantuManusia(EnemyArchetype a, J j, ProceduralAnimator.Mode mode)
        {
            var d = ResepKarakter.Ambil(j);
            var go = BuatKarakter(j, "Hantu_" + a, mode, false);
            go.tag = GameTags.Enemy;
            SetLayer(go, GameLayers.Enemy);
            var cap = go.AddComponent<CapsuleCollider>();
            cap.height = d.tinggi; cap.radius = 0.32f; cap.center = new Vector3(0f, d.tinggi * 0.5f + (mode == ProceduralAnimator.Mode.Melayang ? 0.45f : 0f), 0f);
            var pendar = new GameObject("PendarHantu").AddComponent<Light>();
            pendar.transform.SetParent(go.transform, false);
            pendar.transform.localPosition = new Vector3(0f, d.tinggi * 0.88f, -0.5f);
            pendar.type = LightType.Point; pendar.range = 6.5f;
            pendar.intensity = a == EnemyArchetype.Kuntilanak ? 0.65f : a == EnemyArchetype.Kuyang ? 0.70f : a == EnemyArchetype.GenderuwoHutan ? 0.85f : a == EnemyArchetype.Tuyul ? 0.25f : 0.45f;
            pendar.color = a == EnemyArchetype.Kuntilanak ? new Color(0.55f, 0.68f, 0.92f) : a == EnemyArchetype.Kuyang ? new Color(0.85f, 0.25f, 0.18f) : a == EnemyArchetype.HantuAir ? new Color(0.32f, 0.80f, 0.72f) : a == EnemyArchetype.GenderuwoHutan ? new Color(0.95f, 0.42f, 0.18f) : new Color(0.68f, 0.78f, 0.72f);
            pendar.shadows = LightShadows.None;
            var h = go.AddComponent<HantuAI>(); h.Pasang(a, pendar);
            var tali = new GameObject("TitikInteraksi");
            tali.transform.SetParent(go.transform, false);
            tali.layer = GameLayers.Interactable;
            var sc = tali.AddComponent<SphereCollider>(); sc.isTrigger = true; sc.radius = 1.1f; sc.center = new Vector3(0f, 0.6f, 0f);
            tali.SetActive(false);
            h.PasangInteraksi(tali); // T9: LEPAS TALI POCONG / TANCAPKAN PAKU
            return SimpanPrefab(go, "Hantu_" + a);
        }

        static GameObject PrefabBanaspati()
        {
            string ptex = FolderPrefabMusuh + "/Tex_ApiBanaspati.png";
            const int sz = 128;
            var ftex = new Texture2D(sz, sz, TextureFormat.RGBA32, true);
            var fpx = new Color[sz * sz];
            for (int y = 0; y < sz; y++) for (int x = 0; x < sz; x++)
            {
                float u = (float)x / sz, v = (float)y / sz;
                float cx = Mathf.Abs(u - 0.5f) * 2f;
                float n = Mathf.PerlinNoise(u * 6f + 1.3f, v * 5f - 0.8f);
                float softR = Mathf.Sqrt(cx * cx * 1.4f + Mathf.Pow((v - 0.35f) * 1.45f, 2f));
                float a = Mathf.Clamp01(1f - softR + (n - 0.45f) * 0.45f) * Mathf.SmoothStep(0f, 0.28f, v) * Mathf.SmoothStep(1f, 0.35f, v);
                Color col = Color.Lerp(new Color(0.95f, 0.22f, 0.04f), new Color(1f, 0.92f, 0.45f), Mathf.Clamp01(1f - softR * 0.9f));
                col.a = a * a; fpx[y * sz + x] = col;
            }
            ftex.SetPixels(fpx); ftex.Apply(); SimpanPng(ftex, ptex); AssetDatabase.ImportAsset(ptex); AturImpor(ptex, false, false, sz);
            var fimp = (TextureImporter)AssetImporter.GetAtPath(ptex); fimp.alphaIsTransparency = true; fimp.SaveAndReimport();
            ftex = AssetDatabase.LoadAssetAtPath<Texture2D>(ptex);

            string pm = FolderMat + "/M_Banaspati.mat";
            AssetDatabase.DeleteAsset(pm);
            var pShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            var m = pShader != null ? new Material(pShader) { name = "M_Banaspati" }
                : SafeMaterialFactory.CreatePBR("M_Banaspati", Color.white, 0f, 0.1f, ftex, null, new Color(4.8f, 1.65f, 0.28f), SurfaceMode.Transparent, 0.3f);
            if (pShader != null)
            {
                SafeMaterialFactory.SetF(m, "_Surface", 1f);
                SafeMaterialFactory.SetF(m, "_Blend", 2f);
                SafeMaterialFactory.SetF(m, "_SrcBlend", (float)BlendMode.SrcAlpha);
                SafeMaterialFactory.SetF(m, "_DstBlend", (float)BlendMode.One);
                SafeMaterialFactory.SetF(m, "_ZWrite", 0f);
                SafeMaterialFactory.SetF(m, "_Cull", 0f);
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.renderQueue = 3000;
                SafeMaterialFactory.SetT(m, "_BaseMap", ftex);
                SafeMaterialFactory.SetC(m, "_BaseColor", new Color(1.45f, 0.68f, 0.22f, 0.85f));
            }
            AssetDatabase.CreateAsset(m, pm);
            m = AssetDatabase.LoadAssetAtPath<Material>(pm);
            var mSkull = SafeMaterialFactory.CreatePBR("M_BanaspatiTengkorak", new Color(0.18f, 0.12f, 0.1f), 0f, 0.35f,
                AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/tanah.png"), AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/tanah_n.png"),
                new Color(1.1f, 0.28f, 0.04f));
            string ps = FolderMat + "/M_BanaspatiTengkorak.mat"; AssetDatabase.DeleteAsset(ps); AssetDatabase.CreateAsset(mSkull, ps);
            mSkull = AssetDatabase.LoadAssetAtPath<Material>(ps);
            var go = new GameObject("Hantu_Banaspati");
            go.tag = GameTags.Enemy;
            var skullMesh = ImporMeshVisual.Baca("Assets/ThirdParty/Prepared/Banaspati.bytes");
            if (skullMesh != null)
            {
                string pmsh = FolderPrefabMusuh + "/Mesh_BanaspatiSkull.asset";
                AssetDatabase.DeleteAsset(pmsh); AssetDatabase.CreateAsset(skullMesh, pmsh);
                skullMesh = AssetDatabase.LoadAssetAtPath<Mesh>(pmsh);
                var inti = Anak(go, "TengkorakBanaspati");
                inti.AddComponent<MeshFilter>().sharedMesh = skullMesh;
                var mr = inti.AddComponent<MeshRenderer>();
                mr.sharedMaterials = new[] { mSkull, m };
                mr.shadowCastingMode = ShadowCastingMode.Off;
            }
            else
            {
                var inti = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Object.DestroyImmediate(inti.GetComponent<Collider>());
                inti.transform.SetParent(go.transform, false); inti.transform.localScale = Vector3.one * 0.45f;
                var mr = inti.GetComponent<MeshRenderer>(); mr.sharedMaterial = m; mr.shadowCastingMode = ShadowCastingMode.Off;
            }
            for (int k = 0; k < 6; k++)
            {
                float a = k * 30f;
                var lidah = GameObject.CreatePrimitive(PrimitiveType.Quad);
                lidah.name = "LidahApi_" + k;
                Object.DestroyImmediate(lidah.GetComponent<Collider>());
                lidah.transform.SetParent(go.transform, false);
                lidah.transform.localPosition = new Vector3(Mathf.Sin(a * Mathf.Deg2Rad) * 0.10f, 0.12f + (k % 2) * 0.08f, Mathf.Cos(a * Mathf.Deg2Rad) * 0.10f);
                lidah.transform.localRotation = Quaternion.Euler(0f, a, (k % 2 == 0 ? 1 : -1) * 14f);
                lidah.transform.localScale = new Vector3(0.28f, 0.52f, 0.28f);
                var fmr = lidah.GetComponent<MeshRenderer>(); fmr.sharedMaterial = m; fmr.shadowCastingMode = ShadowCastingMode.Off;
            }
            var bara = Anak(go, "BaraApi");
            var psys = bara.AddComponent<ParticleSystem>();
            var main = psys.main; main.startLifetime = 0.8f; main.startSpeed = 0.9f; main.startSize = 0.18f; main.startColor = new Color(1f, 0.55f, 0.15f, 0.85f); main.maxParticles = 24;
            var em = psys.emission; em.rateOverTime = 14f;
            var shp = psys.shape; shp.shapeType = ParticleSystemShapeType.Sphere; shp.radius = 0.22f;
            var pr = psys.GetComponent<ParticleSystemRenderer>(); pr.sharedMaterial = m; pr.shadowCastingMode = ShadowCastingMode.Off;
            var sc = go.AddComponent<SphereCollider>(); sc.radius = 0.5f;
            var lampu = new GameObject("Cahaya").AddComponent<Light>();
            lampu.transform.SetParent(go.transform, false);
            lampu.type = LightType.Point; lampu.range = 8f; lampu.intensity = 1.85f; lampu.color = new Color(1f, 0.45f, 0.12f); lampu.shadows = LightShadows.None;
            SetLayer(go, GameLayers.Enemy);
            go.AddComponent<HantuAI>().Pasang(EnemyArchetype.Banaspati, lampu);
            return SimpanPrefab(go, "Hantu_Banaspati");
        }

        static GameObject PrefabJarahan()
        {
            var go = new GameObject("Jarahan");
            BagianPrimitif(go.transform, PrimitiveType.Sphere, new Vector3(0f, 0.15f, 0f), new Vector3(0.35f, 0.28f, 0.3f), AmbilMat("M_CatMerahPudar"), false);
            BagianPrimitif(go.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.32f, 0f), new Vector3(0.08f, 0.06f, 0.08f), AmbilMat("M_Kayu"), false);
            var sc = go.AddComponent<SphereCollider>(); sc.isTrigger = true; sc.radius = 0.6f; sc.center = new Vector3(0f, 0.2f, 0f);
            go.layer = GameLayers.Interactable;
            go.AddComponent<JarahanTanah>();
            return SimpanPrefab(go, "Jarahan");
        }

        static GameObject PrefabBambu()
        {
            var go = new GameObject("BambuLempar");
            BagianPrimitif(go.transform, PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.05f, 0.9f, 0.05f), AmbilMat("M_Kayu"), false);
            go.layer = GameLayers.Projectile;
            go.AddComponent<BambuLempar>();
            return SimpanPrefab(go, "BambuLempar");
        }

        static GameObject SimpanPrefab(GameObject go, string nama)
        {
            string p = FolderPrefabMusuh + "/" + nama + ".prefab";
            AssetDatabase.DeleteAsset(p);
            var hasil = PrefabUtility.SaveAsPrefabAsset(go, p);
            Object.DestroyImmediate(go);
            if (hasil == null) Debug.LogError("[AK][Builder] Gagal menyimpan prefab " + p);
            return hasil;
        }
    }
}
