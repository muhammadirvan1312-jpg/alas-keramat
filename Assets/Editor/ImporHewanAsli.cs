using UnityEditor;
using UnityEngine;

namespace AlasKeramat.EditorTools
{
    // Hewan FBX CC0 di Assets/HewanAsli: rig Legacy dengan klip bawaan; klip mati sekali, lainnya berulang.
    public class ImporHewanAsli : AssetPostprocessor
    {
        public const string Folder = "Assets/HewanAsli";

        public static bool KlipMati(string nama) { return nama.Contains("Dying") || nama.Contains("Die"); }

        void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(Folder + "/")) return;
            var m = (ModelImporter)assetImporter;
            m.animationType = ModelImporterAnimationType.Legacy;
            m.importAnimation = true;
            m.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            m.importBlendShapes = false; m.importCameras = false; m.importLights = false;
            m.meshCompression = ModelImporterMeshCompression.Off;
        }

        void OnPreprocessAnimation()
        {
            if (!assetPath.StartsWith(Folder + "/")) return;
            var m = (ModelImporter)assetImporter;
            var kl = m.defaultClipAnimations;
            for (int i = 0; i < kl.Length; i++)
            {
                int idx = kl[i].name.LastIndexOf('|');
                if (idx >= 0 && idx + 1 < kl[i].name.Length) kl[i].name = kl[i].name.Substring(idx + 1);
                kl[i].wrapMode = KlipMati(kl[i].name) ? WrapMode.ClampForever : WrapMode.Loop;
                kl[i].loopTime = !KlipMati(kl[i].name);
            }
            m.clipAnimations = kl;
        }

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Folder + "/")) return;
            var t = (TextureImporter)assetImporter;
            if (assetPath.Contains("_normal")) t.textureType = TextureImporterType.NormalMap;
            t.maxTextureSize = 2048;
            var a = t.GetPlatformTextureSettings("Android");
            a.overridden = true; a.maxTextureSize = 2048; a.format = TextureImporterFormat.ASTC_4x4;
            t.SetPlatformTextureSettings(a);
        }
    }
}
