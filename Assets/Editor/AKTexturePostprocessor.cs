using UnityEditor;

namespace AlasKeramat.EditorTools
{
    // T11 (15 §9, 12 §T11): Mengatur impor tekstur override CC0 di Assets/Resources/Override/Textures secara otomatis.
    public class AKTexturePostprocessor : AssetPostprocessor
    {
        public const string PrefixOverride = "Assets/Resources/Override/Textures/";

        public static bool JalurOverride(string path)
        {
            return !string.IsNullOrEmpty(path) && path.Replace('\\', '/').StartsWith(PrefixOverride);
        }

        public static bool AdalahNormalMap(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            string n = path.ToLowerInvariant();
            return n.Contains("_normal") || n.Contains("_nor") || n.EndsWith("_n.png") || n.EndsWith("_n.jpg");
        }

        void OnPreprocessTexture()
        {
            if (!JalurOverride(assetPath)) return;
            var imp = assetImporter as TextureImporter;
            if (imp == null) return;
            bool normal = AdalahNormalMap(assetPath);
            bool rough = assetPath.ToLowerInvariant().Contains("_rough") || assetPath.ToLowerInvariant().Contains("_mask");
            imp.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            imp.sRGBTexture = !normal && !rough;
            imp.mipmapEnabled = true;
            imp.streamingMipmaps = true;
            imp.maxTextureSize = 2048;
            var andr = imp.GetPlatformTextureSettings("Android");
            andr.overridden = true;
            andr.maxTextureSize = 2048;
            andr.format = TextureImporterFormat.ASTC_4x4;
            imp.SetPlatformTextureSettings(andr);
        }
    }
}
