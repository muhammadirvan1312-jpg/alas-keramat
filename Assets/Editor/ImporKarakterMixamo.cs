using UnityEditor;

namespace AlasKeramat.EditorTools
{
    // Karakter Mixamo (FBX bertulang mixamorig + tekstur tertanam) di Assets/MixamoKarakter: rig Legacy agar klip
    // mocap legacy bisa diputar langsung, material ikut diimpor (dikonversi URP).
    public class ImporKarakterMixamo : AssetPostprocessor
    {
        public const string Folder = "Assets/MixamoKarakter";

        void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(Folder + "/")) return;
            var m = (ModelImporter)assetImporter;
            m.animationType = ModelImporterAnimationType.Legacy;
            m.importAnimation = false;
            m.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            m.materialLocation = ModelImporterMaterialLocation.InPrefab;
            m.importBlendShapes = false; m.importCameras = false; m.importLights = false;
            m.meshCompression = ModelImporterMeshCompression.Off;
            m.optimizeMeshVertices = true; m.optimizeMeshPolygons = true;
        }

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Folder + "/")) return;
            var t = (TextureImporter)assetImporter;
            t.maxTextureSize = 2048;
            var a = t.GetPlatformTextureSettings("Android");
            a.overridden = true; a.maxTextureSize = 2048; a.format = TextureImporterFormat.ASTC_4x4;
            t.SetPlatformTextureSettings(a);
        }
    }
}
