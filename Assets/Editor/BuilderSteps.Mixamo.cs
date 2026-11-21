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

            int n = 0;
            if (AssetDatabase.IsValidFolder(FolderPrefabMusuh))
                foreach (var g in AssetDatabase.FindAssets("t:Prefab", new[] { FolderPrefabMusuh }))
                {
                    string p = AssetDatabase.GUIDToAssetPath(g);
                    var root = PrefabUtility.LoadPrefabContents(p);
                    try
                    {
                        var pa = root.GetComponentInChildren<ProceduralAnimator>(true);
                        if (pa != null && root.GetComponent<NusantaraEnemyAndMutantAI>() != null)
                        {
                            var peran = p.Contains("Buto") ? AnimasiMixamo.Peran.Raksasa : p.Contains("Wanita") ? AnimasiMixamo.Peran.Wanita : AnimasiMixamo.Peran.Sekte;
                            Pasangkan(pa.gameObject, bank, peran);
                            PrefabUtility.SaveAsPrefabAsset(root, p); n++;
                        }
                    }
                    finally { PrefabUtility.UnloadPrefabContents(root); }
                }
            n += PasangDiAdegan<KangDedenKelvinAI>(bank, AnimasiMixamo.Peran.Pria);
            n += PasangDiAdegan<NingsihVirginiaAI>(bank, AnimasiMixamo.Peran.Wanita);
            n += PasangDiAdegan<KiRogoPanggung>(bank, AnimasiMixamo.Peran.Pria);
            n += PasangDiAdegan<BossGenderuwo>(bank, AnimasiMixamo.Peran.Raksasa);
            Catat("Mixamo: " + klip.Count + " klip mocap, retarget dipasang ke " + n + " karakter/prefab");
        }

        static void Pasangkan(GameObject go, BankAnimasiMixamo bank, AnimasiMixamo.Peran peran)
        {
            var am = go.GetComponent<AnimasiMixamo>();
            if (am == null) am = go.AddComponent<AnimasiMixamo>();
            am.Pasang(bank, peran);
            EditorUtility.SetDirty(am);
        }

        static int PasangDiAdegan<T>(BankAnimasiMixamo bank, AnimasiMixamo.Peran peran) where T : Component
        {
            int n = 0;
            foreach (var c in Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var pa = c.GetComponentInChildren<ProceduralAnimator>(true);
                if (pa == null) continue;
                Pasangkan(pa.gameObject, bank, peran); n++;
            }
            return n;
        }
    }
}
