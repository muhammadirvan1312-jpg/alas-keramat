using AlasKeramat.AI;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using JH = AlasKeramat.Core.ResepHewan.JenisHewan;
namespace AlasKeramat.EditorTools
{
    // Langkah 7z7: hewan memakai model FBX CC0 beranimasi (OpenGameArt) bila tersedia di Assets/HewanAsli.
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
                default: return null;
            }
        }

        static void LangkahHewanAsli()
        {
            if (!AssetDatabase.IsValidFolder(ImporHewanAsli.Folder)) { Catat("Hewan asli: folder belum ada, model prosedural dipakai"); return; }
            var mata = MatPolos("M_MataHewan", "#0B0806"); SafeMaterialFactory.SetF(mata, "_Smoothness", 0.85f);
            var mJago = BahanHewan("M_HewanAyamJago", "ayam_jago_albedo.png", null);
            var mBetina = BahanHewan("M_HewanAyamBetina", "ayam_betina_albedo.png", "ayam_betina_normal.png");
            var mRusaBadan = BahanHewan("M_HewanRusaBadan", "rusa_body.png", null);
            var mRusaKepala = BahanHewan("M_HewanRusaKepala", "rusa_head.png", null);
            var mUlar = BahanHewan("M_HewanUlar", "ular_albedo.png", null);
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
                    // Satu bahan per varian tidak didukung: jago & betina memakai bahan masing-masing lewat kunci nama material.
                    mh.Pasang(v, new[] { mJago, mBetina ?? mJago, mBetina ?? mJago, mBetina ?? mJago }, new[] { "Head", "Chicken", "Comb", "Material" }, mata, "Idle1", "Walking", "Walking", "Dying", panjang);
                }
                else if (nm == "rusa" && mRusaBadan != null)
                    mh.Pasang(new[] { FbxHewan("rusa") }, new[] { mRusaBadan, mRusaKepala ?? mRusaBadan }, new[] { "body", "head" }, mata, "Idle", "", "Run", "Die", panjang);
                else if (nm == "ular" && mUlar != null)
                    mh.Pasang(new[] { FbxHewan("ular") }, new[] { mUlar }, new[] { "snake" }, mata, "idle", "walk", "walk", "", panjang * (ai.Jenis == JH.UlarPiton ? 1.6f : 1.1f));
                else { Object.DestroyImmediate(mh); continue; }
                EditorUtility.SetDirty(mh); n++;
            }
            Catat("Hewan asli: " + n + " templat hewan memakai model FBX CC0 beranimasi (ayam, rusa, kancil, ular)");
        }
    }
}
