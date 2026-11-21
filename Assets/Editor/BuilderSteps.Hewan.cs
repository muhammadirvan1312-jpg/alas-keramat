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
            BangunFloraSegar();
            BangunKehidupanAlam();
            Catat("Hewan: 13 template (rusa, kancil, babi hutan, ayam hutan, biawak, harimau, buaya, tawon, piton, kobra, monyet, anjing, kucing); titik pungut pool " + pool.Length);
        }

        static void BangunFloraSegar()
        {
            var go = Anak(_sistem, "FloraSegarDirector");
            var fd = go.AddComponent<FloraSegarDirector>();
            var mDaun = AmbilMat("M_DaunPohon") ?? MatPolos("M_PisangDaun", "#2F6B24");
            var mKayu = AmbilMat("M_Kayu") ?? MatPolos("M_RantingCoklat", "#5C4026");
            var mBambu = MatPolos("M_BambuSegar", "#5A7D32");
            var mKelapa = MatPolos("M_Kelapa", "#4E6826");
            var mPisang = MatPolos("M_BuahPisang", "#D4B828");
            var mIjuk = MatPolos("M_SeratIjuk", "#2E261E");
            var mDamar = MatPolos("M_Damar", "#C87A18");
            var mKepiting = MatPolos("M_Kepiting", "#A83B24");

            var pool = new ForagingExtraNode[FloraSegarDirector.MaksPool];
            for (int k = 0; k < pool.Length; k++)
            {
                var g = Anak(go, "Flora_" + k);
                g.layer = GameLayers.Interactable; g.tag = GameTags.Interactable;
                var sc = g.AddComponent<SphereCollider>(); sc.isTrigger = true; sc.radius = 0.42f; sc.center = Vector3.up * 0.2f;
                pool[k] = g.AddComponent<ForagingExtraNode>();

                // 0..7 urutan PERSIS sesuai FloraSegarDirector.JenisFlora
                var cDaun = Anak(g, "DaunPisang"); cDaun.layer = GameLayers.Interactable;
                BagianVis(cDaun, PrimitiveType.Cube, new Vector3(0f, 0.28f, 0f), new Vector3(0.45f, 0.03f, 0.9f), mDaun);
                BagianVis(cDaun, PrimitiveType.Cube, new Vector3(0.18f, 0.34f, 0.1f), new Vector3(0.4f, 0.03f, 0.8f), mDaun);

                var cRanting = Anak(g, "Ranting"); cRanting.layer = GameLayers.Interactable;
                BagianVis(cRanting, PrimitiveType.Cylinder, new Vector3(0f, 0.06f, 0f), new Vector3(0.05f, 0.35f, 0.05f), mKayu);
                var rt2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder); Object.DestroyImmediate(rt2.GetComponent<Collider>());
                rt2.transform.SetParent(cRanting.transform, false); rt2.transform.localPosition = new Vector3(0.08f, 0.07f, 0.04f); rt2.transform.localRotation = Quaternion.Euler(82f, 35f, 0f); rt2.transform.localScale = new Vector3(0.04f, 0.32f, 0.04f);
                rt2.layer = GameLayers.Interactable; rt2.GetComponent<MeshRenderer>().sharedMaterial = mKayu; rt2.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;

                Vis(g, "BambuMuda", PrimitiveType.Cylinder, Vector3.up * 0.35f, new Vector3(0.08f, 0.35f, 0.08f), mBambu, mDaun, new Vector3(0.1f, 0.5f, 0f), new Vector3(0.28f, 0.04f, 0.12f));
                Vis(g, "Kelapa", PrimitiveType.Sphere, new Vector3(-0.08f, 0.14f, 0f), Vector3.one * 0.26f, mKelapa, mKelapa, new Vector3(0.12f, 0.14f, 0.08f), Vector3.one * 0.24f);
                Vis(g, "Pisang", PrimitiveType.Capsule, Vector3.up * 0.18f, new Vector3(0.12f, 0.18f, 0.12f), mPisang, mDaun, new Vector3(0f, 0.32f, 0f), new Vector3(0.45f, 0.04f, 0.35f));
                Vis(g, "SeratIjuk", PrimitiveType.Sphere, Vector3.up * 0.09f, new Vector3(0.32f, 0.14f, 0.26f), mIjuk, mKayu, new Vector3(0f, 0.06f, 0f), new Vector3(0.2f, 0.06f, 0.2f));
                Vis(g, "Damar", PrimitiveType.Cube, Vector3.up * 0.1f, new Vector3(0.18f, 0.2f, 0.16f), mDamar, mKayu, new Vector3(0f, 0.04f, 0f), new Vector3(0.24f, 0.05f, 0.22f));
                Vis(g, "Kepiting", PrimitiveType.Sphere, Vector3.up * 0.06f, new Vector3(0.24f, 0.09f, 0.2f), mKepiting, mKepiting, new Vector3(0f, 0.07f, 0.12f), new Vector3(0.28f, 0.05f, 0.08f));
                g.SetActive(false);
            }
            fd.Pasang(pool);
            fd.Bangkitkan();
            EditorUtility.SetDirty(fd);
        }

        static void BangunKehidupanAlam()
        {
            var go = Anak(_sistem, "KehidupanAlam");
            var ka = go.AddComponent<KehidupanAlam>();
            var wadah = Anak(go, "Wadah");
            var mBurung = MatPolos("M_BurungHutan", "#26201B");
            var mSayap = MatPolos("M_SayapBurung", "#3A2F24");
            var mKupu = MatPolos("M_Kupu", "#E0A838");
            var mIkan = MatPolos("M_IkanSungai", "#56707C");
            var mKunang = MatEmisi("M_Kunang", "#C8FF5A", new Color(2.4f, 3.2f, 0.6f, 1f), false);

            var burung = new Transform[KehidupanAlam.JumlahBurung];
            var sk = new Transform[KehidupanAlam.JumlahBurung];
            var sn = new Transform[KehidupanAlam.JumlahBurung];
            for (int i = 0; i < burung.Length; i++)
            {
                var b = Anak(wadah, "Burung_" + i);
                BagianVis(b, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.22f, 0.45f, 0.22f), mBurung);
                b.transform.GetChild(0).localRotation = Quaternion.Euler(90f, 0f, 0f);
                var kGo = Anak(b, "SayapKiri"); kGo.transform.localPosition = new Vector3(-0.12f, 0f, 0f);
                BagianVis(kGo, PrimitiveType.Cube, new Vector3(-0.48f, 0f, 0f), new Vector3(0.95f, 0.04f, 0.34f), mSayap);
                var nGo = Anak(b, "SayapKanan"); nGo.transform.localPosition = new Vector3(0.12f, 0f, 0f);
                BagianVis(nGo, PrimitiveType.Cube, new Vector3(0.48f, 0f, 0f), new Vector3(0.95f, 0.04f, 0.34f), mSayap);
                burung[i] = b.transform; sk[i] = kGo.transform; sn[i] = nGo.transform;
            }
            var kupu = new Transform[KehidupanAlam.JumlahKupu];
            for (int i = 0; i < kupu.Length; i++)
            {
                var k = Anak(wadah, "Kupu_" + i);
                BagianVis(k, PrimitiveType.Cube, Vector3.zero, new Vector3(0.20f, 0.02f, 0.14f), mKupu);
                kupu[i] = k.transform;
            }
            var kunang = new Transform[KehidupanAlam.JumlahKunang];
            for (int i = 0; i < kunang.Length; i++)
            {
                var k = Anak(wadah, "Kunang_" + i);
                BagianVis(k, PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.08f, mKunang);
                k.SetActive(false);
                kunang[i] = k.transform;
            }
            var ikan = new Transform[KehidupanAlam.JumlahIkan];
            for (int i = 0; i < ikan.Length; i++)
            {
                var ik = Anak(wadah, "Ikan_" + i);
                BagianVis(ik, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.11f, 0.24f, 0.14f), mIkan);
                ik.transform.GetChild(0).localRotation = Quaternion.Euler(90f, 0f, 0f);
                ik.SetActive(false);
                ikan[i] = ik.transform;
            }
            ka.Pasang(wadah, burung, sk, sn, kupu, kunang, ikan);
            EditorUtility.SetDirty(ka);
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
            if (u.terbang) { bc.size = new Vector3(0.85f, 0.85f, 0.85f); bc.center = new Vector3(0f, u.kaki, 0f); }
            else { bc.size = new Vector3(Mathf.Max(0.45f, u.lebar * 1.25f), Mathf.Max(0.45f, u.kaki + u.lebar), Mathf.Max(0.6f, u.panjang + u.kepala)); bc.center = new Vector3(0f, bc.size.y * 0.5f, 0f); }
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
            if (nama.StartsWith("Jamur") && m2 != null)
            {
                BagianVis(g, PrimitiveType.Cylinder, new Vector3(0.06f, 0.035f, 0.04f), new Vector3(0.022f, 0.035f, 0.022f), m);
                BagianVis(g, PrimitiveType.Sphere, new Vector3(0.06f, 0.075f, 0.04f), new Vector3(0.09f, 0.03f, 0.09f), m2);
                BagianVis(g, PrimitiveType.Cylinder, new Vector3(-0.05f, 0.03f, -0.04f), new Vector3(0.018f, 0.03f, 0.018f), m);
                BagianVis(g, PrimitiveType.Sphere, new Vector3(-0.05f, 0.062f, -0.04f), new Vector3(0.075f, 0.026f, 0.075f), m2);
            }
            else if (nama == "Sirih")
            {
                BagianVis(g, PrimitiveType.Cube, new Vector3(0.07f, 0.18f, 0.04f), new Vector3(0.16f, 0.03f, 0.18f), m);
                BagianVis(g, PrimitiveType.Cube, new Vector3(-0.06f, 0.21f, -0.05f), new Vector3(0.15f, 0.03f, 0.16f), m);
            }
            else if (nama == "Sarang" && m2 != null)
            {
                BagianVis(g, PrimitiveType.Sphere, new Vector3(-0.05f, 0.1f, 0.03f), skala2 * 0.94f, m2);
                BagianVis(g, PrimitiveType.Sphere, new Vector3(0.01f, 0.1f, -0.05f), skala2 * 0.90f, m2);
            }
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
