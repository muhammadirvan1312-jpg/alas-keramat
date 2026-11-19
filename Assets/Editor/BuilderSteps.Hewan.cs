using System.IO;
using AlasKeramat.AI;
using AlasKeramat.Building;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using JH = AlasKeramat.Core.ResepHewan.JenisHewan;
using JP = AlasKeramat.Building.ForagingDirector.JenisPungut;

namespace AlasKeramat.EditorTools
{
    // Langkah 7i (T5b): 5 template hewan (kerangka 17 sendi + SkinnedMesh + WildAnimalAI) di EcosystemDirector; pool 40 titik pungut di ForagingDirector.
    public static partial class NusantaraMasterBuilder
    {
        static void LangkahHewan()
        {
            EnsureFolder(FolderKarakter);
            var eco = Anak(_sistem, "EcosystemDirector");
            var dir = eco.AddComponent<EcosystemDirector>();
            var semua = (JH[])System.Enum.GetValues(typeof(JH));
            var tpl = new GameObject[semua.Length];
            for (int i = 0; i < semua.Length; i++) tpl[i] = BuatHewan(semua[i], eco.transform);
            dir.Pasang(tpl);
            EditorUtility.SetDirty(dir);

            var fgo = Anak(_sistem, "ForagingDirector");
            var fd = fgo.AddComponent<ForagingDirector>();
            var batu = AmbilMat("M_Batu");
            var mat = new[] { batu, MatPolos("M_Sirih", "#3E7A2E"), MatPolos("M_Kunyit", "#D8A020"), MatPolos("M_Jahe", "#B89060"), MatPolos("M_Jamur", "#C8A070"), MatPolos("M_JamurBeracun", "#B82818"), MatPolos("M_Sarang", "#6A5030") };
            var batang = MatPolos("M_BatangJamur", "#E8E0D0"); var telur = MatPolos("M_Telur", "#E8E0C8");
            var pool = new ForagingNode[ForagingDirector.MaksPool];
            for (int k = 0; k < pool.Length; k++)
            {
                var g = Anak(fgo, "Pungut_" + k);
                g.layer = GameLayers.Interactable; g.tag = GameTags.Interactable;
                var sc = g.AddComponent<SphereCollider>(); sc.isTrigger = true; sc.radius = 0.35f; sc.center = Vector3.up * 0.15f;
                pool[k] = g.AddComponent<ForagingNode>();
                Vis(g, "Batu", PrimitiveType.Sphere, Vector3.up * 0.08f, new Vector3(0.3f, 0.16f, 0.24f), mat[0], null, Vector3.zero, Vector3.zero);
                Vis(g, "Sirih", PrimitiveType.Sphere, Vector3.up * 0.12f, new Vector3(0.28f, 0.2f, 0.28f), mat[1], null, Vector3.zero, Vector3.zero);
                Vis(g, "Kunyit", PrimitiveType.Capsule, Vector3.up * 0.15f, new Vector3(0.08f, 0.15f, 0.08f), mat[2], mat[1], new Vector3(0f, 0.32f, 0f), new Vector3(0.25f, 0.06f, 0.1f));
                Vis(g, "Jahe", PrimitiveType.Capsule, Vector3.up * 0.15f, new Vector3(0.07f, 0.15f, 0.07f), mat[3], mat[1], new Vector3(0f, 0.3f, 0f), new Vector3(0.08f, 0.2f, 0.08f));
                Vis(g, "Jamur", PrimitiveType.Cylinder, Vector3.up * 0.05f, new Vector3(0.03f, 0.05f, 0.03f), batang, mat[4], new Vector3(0f, 0.11f, 0f), new Vector3(0.13f, 0.04f, 0.13f));
                Vis(g, "JamurBeracun", PrimitiveType.Cylinder, Vector3.up * 0.05f, new Vector3(0.03f, 0.05f, 0.03f), batang, mat[5], new Vector3(0f, 0.11f, 0f), new Vector3(0.13f, 0.04f, 0.13f));
                Vis(g, "Sarang", PrimitiveType.Cylinder, Vector3.up * 0.04f, new Vector3(0.35f, 0.04f, 0.35f), mat[6], telur, new Vector3(0.04f, 0.1f, 0f), new Vector3(0.07f, 0.09f, 0.07f));
                g.SetActive(false);
            }
            fd.Pasang(pool);
            fd.Bangkitkan();
            EditorUtility.SetDirty(fd);
            Catat("Hewan: 5 template (rusa, kancil, babi hutan, ayam hutan, biawak); titik pungut pool " + pool.Length);
        }

        static GameObject BuatHewan(JH j, Transform induk)
        {
            var root = new GameObject("Hewan_" + j);
            var t = ResepHewan.BuatKerangka(root.transform, j);
            string pm = FolderKarakter + "/M_Hewan_" + j + ".asset";
            var mesh = ResepHewan.BangunMesh(j, root.transform, t);
            AssetDatabase.DeleteAsset(pm); AssetDatabase.CreateAsset(mesh, pm);
            string pt = FolderKarakter + "/Atlas_Hewan_" + j + ".png";
            var tex = CharacterMeshBuilder.Atlas(ResepHewan.Warna(j));
            File.WriteAllBytes(pt, tex.EncodeToPNG()); Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(pt); AturImpor(pt, false, false, CharacterMeshBuilder.UkuranAtlas);
            var mat = SafeMaterialFactory.CreatePBR("M_Hewan_" + j, Color.white, 0f, 0.25f, AssetDatabase.LoadAssetAtPath<Texture2D>(pt));
            string pmat = FolderKarakter + "/M_Hewan_" + j + ".mat";
            AssetDatabase.DeleteAsset(pmat); AssetDatabase.CreateAsset(mat, pmat);
            var r = new GameObject("Tubuh"); r.transform.SetParent(root.transform, false);
            var smr = r.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = mesh; smr.bones = t; smr.rootBone = t[ResepHewan.Pelvis]; smr.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(pmat);
            smr.localBounds = new Bounds(Vector3.zero, Vector3.one * 3f);
            var u = ResepHewan.Ambil(j);
            var bc = root.AddComponent<BoxCollider>();
            bc.size = new Vector3(u.lebar * 1.2f, u.kaki + u.lebar, u.panjang + u.kepala); bc.center = new Vector3(0f, (u.kaki + u.lebar) * 0.5f, 0f);
            var ai = root.AddComponent<WildAnimalAI>();
            ai.Pasang(j, t);
            EditorUtility.SetDirty(ai);
            root.transform.SetParent(induk, false);
            root.SetActive(false);
            return root;
        }

        static Material MatPolos(string nama, string hex)
        {
            string p = FolderMat + "/" + nama + ".mat";
            AssetDatabase.DeleteAsset(p);
            var m = SafeMaterialFactory.CreatePBR(nama, ProcTex.Hex(hex), 0f, 0.3f);
            AssetDatabase.CreateAsset(m, p);
            return AssetDatabase.LoadAssetAtPath<Material>(p);
        }

        static void Vis(GameObject induk, string nama, PrimitiveType tipe, Vector3 pos, Vector3 skala, Material m, Material m2, Vector3 pos2, Vector3 skala2)
        {
            var g = new GameObject(nama); g.transform.SetParent(induk.transform, false); g.layer = GameLayers.Interactable;
            BagianVis(g, tipe, pos, skala, m);
            if (m2 != null) BagianVis(g, nama == "Sarang" ? PrimitiveType.Sphere : nama.StartsWith("Jamur") ? PrimitiveType.Sphere : PrimitiveType.Cube, pos2, skala2, m2);
        }

        static void BagianVis(GameObject induk, PrimitiveType tipe, Vector3 pos, Vector3 skala, Material m)
        {
            var g = GameObject.CreatePrimitive(tipe);
            Object.DestroyImmediate(g.GetComponent<Collider>());
            g.transform.SetParent(induk.transform, false); g.transform.localPosition = pos; g.transform.localScale = skala;
            g.layer = GameLayers.Interactable;
            var mr = g.GetComponent<MeshRenderer>(); mr.sharedMaterial = m; mr.shadowCastingMode = ShadowCastingMode.Off;
        }
    }
}
