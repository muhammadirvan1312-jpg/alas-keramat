using System.Collections.Generic;
using AlasKeramat.AI;
using UnityEditor;
using UnityEngine;
namespace AlasKeramat.EditorTools
{
    // Langkah 7z: bank animasi mocap Mixamo (Assets/Mixamo/*.fbx, legacy) + kerangka proksi, dipasang ke Sekte, Deden, Ningsih,
    // Ki Rogo dan Genderuwo lewat AnimasiMixamo. Tanpa aset Mixamo langkah ini dilewati (animasi prosedural tetap jalan).
    public static partial class NusantaraMasterBuilder
    {
        public const string FolderMixamoGen = FolderGen + "/Mixamo", PathBankMixamo = FolderMixamoGen + "/BankAnimasiMixamo.asset", NamaModelProksi = "Breathing_Idle";

        static void LangkahMixamo()
        {
            if (!AssetDatabase.IsValidFolder(ImporMixamo.Folder)) { Catat("Mixamo: folder Assets/Mixamo belum ada, animasi prosedural dipakai"); return; }
            var klip = new List<AnimationClip>();
            string pathProksi = null;
            foreach (var g in AssetDatabase.FindAssets("t:Model", new[] { ImporMixamo.Folder }))
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                if (System.IO.Path.GetFileNameWithoutExtension(p) == NamaModelProksi) pathProksi = p;
                foreach (var o in AssetDatabase.LoadAllAssetsAtPath(p)) { var c = o as AnimationClip; if (c != null && !c.name.StartsWith("__preview__")) klip.Add(c); }
            }
            if (klip.Count == 0 || pathProksi == null) { Catat("Mixamo: klip/proksi tidak ditemukan (" + klip.Count + " klip)"); return; }
            EnsureFolder(FolderMixamoGen);

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(pathProksi);
            var inst = Object.Instantiate(model);
            foreach (var r in inst.GetComponentsInChildren<Renderer>(true)) Object.DestroyImmediate(r.gameObject.GetComponent<Renderer>());
            foreach (var mf in inst.GetComponentsInChildren<MeshFilter>(true)) Object.DestroyImmediate(mf);
            foreach (var an in inst.GetComponentsInChildren<Animation>(true)) Object.DestroyImmediate(an);
            float hips = 1f;
            foreach (var t in inst.GetComponentsInChildren<Transform>(true)) if (t.name.EndsWith(":Hips")) hips = t.position.y;
            string pathPrefab = FolderMixamoGen + "/ProksiMixamo.prefab";
            AssetDatabase.DeleteAsset(pathPrefab);
            var proksi = PrefabUtility.SaveAsPrefabAsset(inst, pathPrefab);
            Object.DestroyImmediate(inst);

            AssetDatabase.DeleteAsset(PathBankMixamo);
            var bank = ScriptableObject.CreateInstance<BankAnimasiMixamo>();
            bank.proksi = proksi; bank.klip = klip.ToArray(); bank.tinggiPinggul = hips;
            AssetDatabase.CreateAsset(bank, PathBankMixamo);
            AssetDatabase.SaveAssets();

            int n = 0; JumlahModelMixamo = 0;
            if (AssetDatabase.IsValidFolder(FolderPrefabMusuh))
                foreach (var g in AssetDatabase.FindAssets("t:Prefab", new[] { FolderPrefabMusuh }))
                {
                    string p = AssetDatabase.GUIDToAssetPath(g);
                    var root = PrefabUtility.LoadPrefabContents(p);
                    try
                    {
                        var pa = root.GetComponentInChildren<ProceduralAnimator>(true);
                        if (pa != null && (root.GetComponent<NusantaraEnemyAndMutantAI>() != null || root.GetComponent<HantuAI>() != null))
                        {
                            string fn = System.IO.Path.GetFileNameWithoutExtension(p);
                            var peran = (p.Contains("Buto") || p.Contains("Genderuwo")) ? AnimasiMixamo.Peran.Raksasa : (p.Contains("Wanita") || p.Contains("Kuntilanak") || p.Contains("Penyerupa")) ? AnimasiMixamo.Peran.Wanita : AnimasiMixamo.Peran.Sekte;
                            Pasangkan(pa.gameObject, bank, peran, fn);
                            PrefabUtility.SaveAsPrefabAsset(root, p); n++;
                        }
                    }
                    finally { PrefabUtility.UnloadPrefabContents(root); }
                }
            n += PasangDiAdegan<KangDedenKelvinAI>(bank, AnimasiMixamo.Peran.Pria, "Deden");
            n += PasangDiAdegan<NingsihVirginiaAI>(bank, AnimasiMixamo.Peran.Wanita, "Ningsih");
            n += PasangDiAdegan<KiRogoPanggung>(bank, AnimasiMixamo.Peran.Pria, "KiRogo");
            n += PasangDiAdegan<BossGenderuwo>(bank, AnimasiMixamo.Peran.Raksasa, "Genderuwo");
            Catat("Mixamo: " + klip.Count + " klip mocap, retarget dipasang ke " + n + " karakter/prefab, " + JumlahModelMixamo + " memakai model karakter Mixamo asli");
        }

        // Musuh & karakter utama -> model karakter Mixamo (file di Assets/MixamoKarakter, dari repo aset privat). null = tetap model lama.
        public static string ModelUntuk(string nama)
        {
            if (string.IsNullOrEmpty(nama)) return null;
            if (nama == "Deden" || nama.Contains("KangDeden")) return "Remy";
            if (nama == "Ningsih") return "Peasant_Girl";
            if (nama == "KiRogo" || nama == "KiRogoPanggung") return "Peasant_Man";
            if (nama.Contains("Genderuwo")) return "Warrok";
            if (nama.Contains("Kuntilanak")) return "Zombiegirl";
            if (nama.Contains("Penyerupa")) return "Peasant_Girl";
            if (nama.Contains("AbdiButo")) return "Maw";
            if (nama.Contains("AbdiWanita")) return "Zombiegirl";
            if (nama.Contains("AbdiLumpur") || nama.Contains("MutanGembung")) return "Mutant";
            if (nama.Contains("PemimpinPatroli")) return "Parasite";
            if (nama.Contains("MutanJemari")) return "Mremireh";
            if (nama.Contains("AbdiSekte")) return "Nightshade";
            return null;
        }
        public static int JumlahModelMixamo;

        static GameObject MuatModelKarakter(string nama)
        {
            if (nama == null) return null;
            string p = ImporKarakterMixamo.Folder + "/" + nama + ".fbx";
            ImporKarakterMixamo.PastikanTeksturDiekstrak(p);
            return AssetDatabase.LoadAssetAtPath<GameObject>(p);
        }

        static void Pasangkan(GameObject go, BankAnimasiMixamo bank, AnimasiMixamo.Peran peran, string namaUntukModel = null)
        {
            var am = go.GetComponent<AnimasiMixamo>();
            if (am == null) am = go.AddComponent<AnimasiMixamo>();
            am.Pasang(bank, peran);
            string nm = namaUntukModel ?? "";
            var model = MuatModelKarakter(ModelUntuk(nm));
            am.PasangModel(model);
            am.PasangShaderKulit(Shader.Find("AlasKeramat/KulitManusia"));
            am.PasangOpsiVisual(nm.Contains("Kuntilanak"), nm.Contains("Genderuwo") || nm.Contains("Buto"), nm);
            if (model != null) JumlahModelMixamo++;
            EditorUtility.SetDirty(am);
        }

        static int PasangDiAdegan<T>(BankAnimasiMixamo bank, AnimasiMixamo.Peran peran, string namaModel = null) where T : Component
        {
            int n = 0;
            foreach (var c in Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var pa = c.GetComponentInChildren<ProceduralAnimator>(true);
                if (pa == null) continue;
                Pasangkan(pa.gameObject, bank, peran, namaModel); n++;
            }
            return n;
        }
    }
}
