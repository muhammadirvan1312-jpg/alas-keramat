using System.IO;
using UnityEditor;

namespace AlasKeramat.EditorTools
{
    // Karakter Mixamo (FBX bertulang mixamorig + tekstur tertanam) di Assets/MixamoKarakter: rig Legacy agar klip
    // mocap legacy bisa diputar langsung, tekstur tertanam diekstrak agar tidak putih polos di URP.
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

        // Ekstrak tekstur tertanam FBX ke subfolder Textures/<NamaModel> supaya material URP punya _BaseMap asli.
        public static void PastikanTeksturDiekstrak(string fbxPath)
        {
            if (string.IsNullOrEmpty(fbxPath) || !File.Exists(fbxPath)) return;
            var imp = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (imp == null) return;
            string nama = Path.GetFileNameWithoutExtension(fbxPath);
            string texDir = Folder + "/Textures_" + nama;
            if (AssetDatabase.IsValidFolder(texDir) && AssetDatabase.FindAssets("t:Texture2D", new[] { texDir }).Length > 0) return;
            if (!Directory.Exists(texDir)) Directory.CreateDirectory(texDir);
            try
            {
                imp.ExtractTextures(texDir);
                imp.SearchAndRemapMaterials(ModelImporterMaterialName.BasedOnMaterialName, ModelImporterMaterialSearch.RecursiveUp);
                imp.SaveAndReimport();
            }
            catch { /* FBX tanpa tekstur tertanam akan memakai palet PBR fallback di AnimasiMixamo */ }
        }

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Folder + "/")) return;
            var t = (TextureImporter)assetImporter;
            if (assetPath.ToLowerInvariant().Contains("normal") || assetPath.ToLowerInvariant().Contains("_n."))
                t.textureType = TextureImporterType.NormalMap;
            t.streamingMipmaps = true;
            t.maxTextureSize = 2048;
            var a = t.GetPlatformTextureSettings("Android");
            a.overridden = true; a.maxTextureSize = 2048; a.format = TextureImporterFormat.ASTC_4x4;
            t.SetPlatformTextureSettings(a);
        }
    }
}
