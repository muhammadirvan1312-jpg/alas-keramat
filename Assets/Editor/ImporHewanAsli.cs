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
            m.meshCompression = ModelImporterMeshCompression.Medium;
        }

        void OnPreprocessAnimation()
        {
            if (!assetPath.StartsWith(Folder + "/")) return;
            var m = (ModelImporter)assetImporter;
            var kl = m.defaultClipAnimations;
            for (int i = 0; i < kl.Length; i++) { kl[i].wrapMode = KlipMati(kl[i].name) ? WrapMode.ClampForever : WrapMode.Loop; kl[i].loopTime = !KlipMati(kl[i].name); }
            m.clipAnimations = kl;
        }

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Folder + "/")) return;
            var t = (TextureImporter)assetImporter;
            if (assetPath.Contains("_normal")) t.textureType = TextureImporterType.NormalMap;
            var a = t.GetPlatformTextureSettings("Android");
            a.overridden = true; a.maxTextureSize = 1024; a.format = TextureImporterFormat.ASTC_6x6;
            t.SetPlatformTextureSettings(a);
        }
    }
}
