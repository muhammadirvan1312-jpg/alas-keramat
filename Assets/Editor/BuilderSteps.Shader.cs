using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;

namespace AlasKeramat.EditorTools
{
    // T2e: material vegetasi & air memakai shader AK (GPT TUGAS 04). Bila shader tidak ada / error, material URP/Lit lama tetap dipakai.
    public static partial class NusantaraMasterBuilder
    {
        public const string ShaderDaun = "AlasKeramat/Daun", ShaderKulit = "AlasKeramat/Kulit", ShaderAir = "AlasKeramat/Air";

        static Shader AmbilShaderAK(string nama)
        {
            var sh = Shader.Find(nama);
            if (sh == null || ShaderUtil.ShaderHasError(sh))
            {
                Debug.LogWarning("[AK][Builder] shader " + nama + " tidak tersedia/error, pakai URP/Lit");
                return null;
            }
            return sh;
        }

        // Ubah material PBR buatan SafeMaterialFactory ke shader AK, mempertahankan tekstur & warna.
        static Material KeAK(Material m, string namaShader, float goyang, float translucency)
        {
            var sh = AmbilShaderAK(namaShader);
            if (sh == null || m == null) return m;
            Texture albedo = m.HasProperty("_BaseMap") ? m.GetTexture("_BaseMap") : null;
            Texture normal = m.HasProperty("_BumpMap") ? m.GetTexture("_BumpMap") : null;
            Color warna = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : Color.white;
            float halus = m.HasProperty("_Smoothness") ? m.GetFloat("_Smoothness") : 0.2f;
            m.shader = sh;
            m.SetTexture("_BaseMap", albedo);
            m.SetColor("_BaseColor", warna);
            m.SetFloat("_Smoothness", halus);
            m.SetFloat("_Goyang", goyang);
            if (m.HasProperty("_BumpMap") && normal != null) m.SetTexture("_BumpMap", normal);
            if (m.HasProperty("_Translucency")) m.SetFloat("_Translucency", translucency);
            if (m.HasProperty("_Cutoff")) m.SetFloat("_Cutoff", 0.5f);
            string nom=m.name.Contains("Palem")?"palem":"kulit_kayu";
            var mask=AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex+"/"+nom+"_mask.png");
            if(mask!=null && m.HasProperty("_MaskMap")) { m.SetTexture("_MaskMap",mask);m.SetFloat("_PakaiMask",1f); }
            m.enableInstancing = true;
            return m;
        }

        static Material AirAK(string nama, Color dangkal, Color dalam, float kecepatan, Vector2 arah)
        {
            var sh = AmbilShaderAK(ShaderAir);
            if (sh == null) return null;
            var m = new Material(sh) { name = nama, enableInstancing = true };
            m.SetColor("_WarnaDangkal", dangkal);
            m.SetColor("_WarnaDalam", dalam);
            m.SetFloat("_Kecepatan", kecepatan);
            m.SetVector("_ArahAlir", new Vector4(arah.x, arah.y, 0f, 0f));
            string p = FolderMat + "/" + nama + ".mat";
            AssetDatabase.CreateAsset(m, p);
            return AssetDatabase.LoadAssetAtPath<Material>(p);
        }
    }
}
