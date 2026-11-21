using System.IO;
using UnityEditor;
using UnityEngine;
namespace AlasKeramat.EditorTools
{
    // Impor FBX Mixamo di Assets/Mixamo: tipe animasi Legacy (H1), nama klip = nama berkas, gerak berulang di-loop.
    public class ImporMixamo : AssetPostprocessor
    {
        public const string Folder = "Assets/Mixamo";
        static readonly string[] Ulang = { "Idle", "Walk", "Running", "Run", "Swim", "Crawl", "Treading", "Crouch", "Strafe", "Sitting", "Sleeping", "Kneeling", "Terrified", "Looking" };

        public static bool Berulang(string nama) { foreach (var u in Ulang) if (nama.Contains(u)) return true; return false; }

        void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(Folder + "/")) return;
            var mi = (ModelImporter)assetImporter;
            mi.animationType = ModelImporterAnimationType.Legacy;
            mi.importAnimation = true;
            mi.materialImportMode = ModelImporterMaterialImportMode.None;
            mi.importBlendShapes = false;
            mi.importCameras = false; mi.importLights = false;
            mi.animationCompression = ModelImporterAnimationCompression.Optimal;
        }

        void OnPreprocessAnimation()
        {
            if (!assetPath.StartsWith(Folder + "/")) return;
            var mi = (ModelImporter)assetImporter;
            var klip = mi.defaultClipAnimations;
            string nama = Path.GetFileNameWithoutExtension(assetPath);
            for (int i = 0; i < klip.Length; i++)
            {
                klip[i].name = i == 0 ? nama : nama + "_" + i;
                bool loop = Berulang(nama);
                klip[i].loopTime = loop;
                klip[i].wrapMode = loop ? WrapMode.Loop : WrapMode.ClampForever;
            }
            mi.clipAnimations = klip;
        }
    }
}
