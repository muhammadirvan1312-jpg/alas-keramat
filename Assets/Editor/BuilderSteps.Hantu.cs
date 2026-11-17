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
            var jarahan = PrefabJarahan();
            var bambu = PrefabBambu();
            var dir = Object.FindFirstObjectByType<EnemyDirector>();
            if (dir == null) { Debug.LogError("[AK][Builder] EnemyDirector belum ada (langkah 7n), hantu dilewati"); return; }
            dir.PasangMalam(hantu, jarahan, bambu);
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
            for (int i = 0; i < 2; i++)
            {
                if (hantu[i] == null) continue;
                var g = (GameObject)PrefabUtility.InstantiatePrefab(hantu[i]);
                g.transform.SetParent(dio.transform, false);
                float x = PosDioramaHantu.x + i * 5f, z = PosDioramaHantu.z + i * 6f;
                g.transform.position = new Vector3(x, WorldTerrainGenerator.SampleHeight(x, z) + (i == 1 ? 2.2f : 0f), z);
                g.transform.rotation = Quaternion.Euler(0f, 200f, 0f);
                var ai = g.GetComponent<HantuAI>(); if (ai != null) ai.enabled = false;
            }
            dio.SetActive(false);
            Catat("Hantu: prefab Pocong/Kuntilanak/Banaspati, jarahan, bambu lempar, 6 Totem Topeng, MusikTempur");
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
            var h = go.AddComponent<HantuAI>(); h.Pasang(a, null);
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
            string pm = FolderMat + "/M_Banaspati.mat";
            AssetDatabase.DeleteAsset(pm);
            var m = SafeMaterialFactory.CreatePBR("M_Banaspati", new Color(1f, 0.45f, 0.1f), 0f, 0.2f, null, null, new Color(4f, 1.4f, 0.25f));
            AssetDatabase.CreateAsset(m, pm);
            m = AssetDatabase.LoadAssetAtPath<Material>(pm);
            var go = new GameObject("Hantu_Banaspati");
            go.tag = GameTags.Enemy;
            var inti = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.DestroyImmediate(inti.GetComponent<Collider>());
            inti.transform.SetParent(go.transform, false);
            inti.transform.localScale = Vector3.one * 0.55f;
            var mr = inti.GetComponent<MeshRenderer>(); mr.sharedMaterial = m; mr.shadowCastingMode = ShadowCastingMode.Off;
            var luar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.DestroyImmediate(luar.GetComponent<Collider>());
            luar.transform.SetParent(go.transform, false);
            luar.transform.localPosition = new Vector3(0f, 0.15f, 0f); luar.transform.localScale = new Vector3(0.8f, 1.0f, 0.8f);
            var mr2 = luar.GetComponent<MeshRenderer>(); mr2.sharedMaterial = m; mr2.shadowCastingMode = ShadowCastingMode.Off;
            var sc = go.AddComponent<SphereCollider>(); sc.radius = 0.5f;
            var lampu = new GameObject("Cahaya").AddComponent<Light>();
            lampu.transform.SetParent(go.transform, false);
            lampu.type = LightType.Point; lampu.range = 9f; lampu.intensity = 2.4f; lampu.color = new Color(1f, 0.5f, 0.15f); lampu.shadows = LightShadows.None;
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
