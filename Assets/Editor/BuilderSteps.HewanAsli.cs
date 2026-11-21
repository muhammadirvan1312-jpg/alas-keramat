using AlasKeramat.AI;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using JH = AlasKeramat.Core.ResepHewan.JenisHewan;
namespace AlasKeramat.EditorTools
{
    // Langkah 7z7: hewan memakai model FBX beranimasi + tekstur PBR 2K bila tersedia di Assets/HewanAsli, serta kelelawar gua di UG1/UG3.
    public static partial class NusantaraMasterBuilder
    {
        static Material BahanHewan(string nama, string albedo, string normal)
        {
            var ta = AssetDatabase.LoadAssetAtPath<Texture2D>(ImporHewanAsli.Folder + "/" + albedo);
            if (ta == null) return null;
            var tn = normal != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(ImporHewanAsli.Folder + "/" + normal) : null;
            string p = FolderMat + "/" + nama + ".mat";
            AssetDatabase.DeleteAsset(p);
            var m = SafeMaterialFactory.CreatePBR(nama, Color.white, 0f, 0.28f, ta, tn);
            AssetDatabase.CreateAsset(m, p);
            return AssetDatabase.LoadAssetAtPath<Material>(p);
        }

        static GameObject FbxHewan(string nama) { return AssetDatabase.LoadAssetAtPath<GameObject>(ImporHewanAsli.Folder + "/" + nama + ".fbx"); }

        public static string ModelHewanUntuk(JH j)
        {
            switch (j)
            {
                case JH.AyamHutan: return "ayam_jago";
                case JH.Rusa: case JH.Kancil: return "rusa";
                case JH.UlarPiton: case JH.UlarKobra: return "ular";
                case JH.BabiHutan: return "babi_hutan";
                case JH.Harimau: return "harimau";
                case JH.Buaya: return "buaya";
                case JH.Biawak: return "biawak";
                case JH.Monyet: return "monyet";
                case JH.Anjing: return "anjing";
                case JH.Kucing: return "kucing";
                default: return null;
            }
        }

        static void PasangKelelawarGua()
        {
            bool adaFolder = AssetDatabase.IsValidFolder(ImporHewanAsli.Folder);
            GameObject fbx = adaFolder ? FbxHewan("kelelawar") : null;
            Material mUtama = adaFolder ? BahanHewan("M_HewanKelelawar", "kelelawar_albedo.jpg", "kelelawar_normal.jpg") : null;
            Material mBagian = adaFolder ? BahanHewan("M_HewanKelelawarBagian", "kelelawar_parts.jpg", null) : null;
            if (mUtama == null) mUtama = MatPolos("M_KelelawarCadangan", "#1D1614");
            if (mBagian == null) mBagian = mUtama;

            var bawah = GameObject.Find(NamaRoot + "/BawahTanah");
            var wadah = Anak(bawah != null ? bawah : _root, "KawananKelelawarGua");
            Vector3 pUG1 = WorldCaveAndSaveManager.Pusat[(int)UndergroundId.UG1_GuaSungai];
            Vector3 pUG3 = WorldCaveAndSaveManager.Pusat[(int)UndergroundId.UG3_KuilAkar];
            Vector3[] titikPusat =
            {
                pUG1 + new Vector3(-15f, 8.4f, 0f),
                pUG1 + new Vector3(-13f, 7.8f, 2f),
                pUG1 + new Vector3(-17f, 8.1f, -2f),
                pUG1 + new Vector3(-15f, 8.8f, 1f),
                pUG1 + new Vector3(15f, 5.1f, -10f),
                pUG1 + new Vector3(16f, 4.8f, -9f),
                pUG1 + new Vector3(60f, 6.4f, -14f),
                pUG1 + new Vector3(58f, 6.1f, -13f),
                pUG3 + new Vector3(0f, 15.2f, 20f),
                pUG3 + new Vector3(4f, 14.6f, 22f),
                pUG3 + new Vector3(-4f, 15.8f, 18f),
                pUG3 + new Vector3(2f, 14.9f, 16f)
            };
            for (int i = 0; i < titikPusat.Length; i++)
            {
                var go = Anak(wadah, "KelelawarGua_" + i);
                float rad = 2.4f + (i % 4) * 0.75f;
                float tinggi = 0.35f + (i % 3) * 0.14f;
                float kec = 1.15f + (i % 5) * 0.16f;
                float fase = i * 0.85f;
                go.transform.position = KelelawarGua.HitungPosisi(titikPusat[i], rad, tinggi, kec, fase, 0f);
                var kg = go.AddComponent<KelelawarGua>();
                kg.Pasang(fbx, mUtama, mBagian, titikPusat[i], rad, tinggi, kec, fase, 0.36f, "Bat_Flying");
                EditorUtility.SetDirty(kg);
            }
            Catat("Kelelawar gua: " + titikPusat.Length + " ekor dipasang di UG1 (Ruang Lumut, Kolam Hitam, Altar Paku) & UG3 (Kubah Akar)");
        }

        static void LangkahHewanAsli()
        {
            PasangKelelawarGua();
            if (!AssetDatabase.IsValidFolder(ImporHewanAsli.Folder)) { Catat("Hewan asli: folder belum ada, model prosedural dipakai"); return; }
            var mata = MatPolos("M_MataHewan", "#0B0806"); SafeMaterialFactory.SetF(mata, "_Smoothness", 0.85f);
            var mJago = BahanHewan("M_HewanAyamJago", "ayam_jago_albedo.png", null);
            var mBetina = BahanHewan("M_HewanAyamBetina", "ayam_betina_albedo.png", "ayam_betina_normal.png");
            var mRusaBadan = BahanHewan("M_HewanRusaBadan", "rusa_body.png", null);
            var mRusaKepala = BahanHewan("M_HewanRusaKepala", "rusa_head.png", null);
            var mUlar = BahanHewan("M_HewanUlar", "ular_albedo.png", null);
            var mBabi = BahanHewan("M_HewanBabiHutan", "babi_hutan_albedo.png", "babi_hutan_normal.png");
            var mHarimau = BahanHewan("M_HewanHarimau", "harimau_albedo.png", "harimau_normal.png");
            var mBuaya = BahanHewan("M_HewanBuaya", "buaya_albedo.png", "buaya_normal.png");
            var mBiawak = BahanHewan("M_HewanBiawak", "biawak_albedo.png", "biawak_normal.png");
            var mMonyet = BahanHewan("M_HewanMonyet", "monyet_albedo.png", "monyet_normal.png");
            var mAnjing = BahanHewan("M_HewanAnjing", "anjing_albedo.png", "anjing_normal.png");
            var mKucing = BahanHewan("M_HewanKucing", "kucing_albedo.png", "kucing_normal.png");
            int n = 0;
            foreach (var ai in Object.FindObjectsByType<WildAnimalAI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                string nm = ModelHewanUntuk(ai.Jenis);
                if (nm == null) continue;
                var col = ai.GetComponent<BoxCollider>();
                float panjang = col != null ? col.size.z : 1f;
                var mh = ai.GetComponent<ModelHewanAsli>() ?? ai.gameObject.AddComponent<ModelHewanAsli>();
                if (nm == "ayam_jago" && mJago != null)
                {
                    var betina = FbxHewan("ayam_betina");
                    var v = betina != null && mBetina != null ? new[] { FbxHewan("ayam_jago"), betina } : new[] { FbxHewan("ayam_jago") };
                    mh.Pasang(v, new[] { mJago, mBetina ?? mJago, mBetina ?? mJago, mBetina ?? mJago }, new[] { "Head", "Chicken", "Comb", "Material" }, mata, "Idle1", "Walking", "Walking", "Dying", panjang);
                }
                else if (nm == "rusa" && mRusaBadan != null)
                    mh.Pasang(new[] { FbxHewan("rusa") }, new[] { mRusaBadan, mRusaKepala ?? mRusaBadan }, new[] { "body", "head" }, mata, "Idle", "", "Run", "Die", panjang);
                else if (nm == "ular" && mUlar != null)
                    mh.Pasang(new[] { FbxHewan("ular") }, new[] { mUlar }, new[] { "snake" }, mata, "idle", "walk", "walk", "", panjang * (ai.Jenis == JH.UlarPiton ? 1.6f : 1.1f));
                else if (nm == "babi_hutan" && mBabi != null)
                    mh.Pasang(new[] { FbxHewan("babi_hutan") }, new[] { mBabi }, new[] { "M_Boar" }, mata, "idle", "Walk_Forward", "Run_Forward", "Death", panjang);
                else if (nm == "harimau" && mHarimau != null)
                    mh.Pasang(new[] { FbxHewan("harimau") }, new[] { mHarimau }, new[] { "M_Tiger" }, mata, "idle", "Walk_Forward", "Run_Forward", "Death", panjang);
                else if (nm == "buaya" && mBuaya != null)
                    mh.Pasang(new[] { FbxHewan("buaya") }, new[] { mBuaya }, new[] { "M_Crocodile" }, mata, "idle", "Walk_Forward", "Run_Forward", "Death", panjang);
                else if (nm == "biawak" && mBiawak != null)
                    mh.Pasang(new[] { FbxHewan("biawak") }, new[] { mBiawak }, new[] { "Material" }, mata, "Take 001", "Take 001", "Take 001", "", panjang);
                else if (nm == "monyet" && mMonyet != null)
                    mh.Pasang(new[] { FbxHewan("monyet") }, new[] { mMonyet }, new[] { "Material" }, mata, "Take 001", "Take 001", "Take 001", "", panjang);
                else if (nm == "anjing" && mAnjing != null)
                    mh.Pasang(new[] { FbxHewan("anjing") }, new[] { mAnjing }, new[] { "M_Wolf" }, mata, "idle", "Walk_Forward", "Run_Forward", "Death", panjang);
                else if (nm == "kucing" && mKucing != null)
                    mh.Pasang(new[] { FbxHewan("kucing") }, new[] { mKucing }, new[] { "M_Cat" }, mata, "idle", "Walk_Forward", "Run_Forward", "Death", panjang);
                else { Object.DestroyImmediate(mh); continue; }
                EditorUtility.SetDirty(mh); n++;
            }
            Catat("Hewan asli: " + n + " templat hewan memakai model FBX beranimasi + tekstur PBR 2K");
        }
    }
}
