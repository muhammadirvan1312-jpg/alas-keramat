using System;
using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AlasKeramat.EditorTools
{
    // Langkah 1: Tag/Layer, matriks tabrakan, Player Settings dasar, aset URP + Quality, Always Included Shaders.
    public static partial class NusantaraMasterBuilder
    {
        public const string FolderRender = FolderGen + "/Render";
        public const string PathUrpAsset = FolderRender + "/AK_URP.asset";
        public const string PathUrpRenderer = FolderRender + "/AK_URP_Renderer.asset";
        const string PathPostProcessData = "Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset";
        const float JarakBayangan = 60f;

        public static readonly string[] NamaLayer =
        {
            "Ground", "Player", "Companion", "Enemy", "Interactable", "Tree", "LogPhysics", "Structure",
            "BuildGhost", "InventoryStage", "Projectile", "Foliage", "TriggerZone"
        };

        public static readonly string[] TagTambahan =
        {
            GameTags.Companion, GameTags.Enemy, GameTags.TreeNode, GameTags.LogItem, GameTags.StructurePiece,
            GameTags.Interactable, GameTags.CaveEntrance, GameTags.LoreDocument, GameTags.Altar, GameTags.SaungBed,
            GameTags.Campfire, GameTags.WaterSource, GameTags.Effigy, GameTags.GeneratedRoot, GameTags.ZiplineAnchor
        };

        // Shader URP TIDAK boleh masuk Always Included (varian > 1 juta, build Android gagal);
        // URP ikut ter-build lewat material .mat di Assets/Generated.
        public static readonly string[] ShaderWajib = { "Skybox/Procedural" };
        const string AwalanUrp = "Universal Render Pipeline/";

        static void LangkahProyek()
        {
            DaftarTagLayer();
            GameBootstrap.ApplyCollisionMatrix();
            AturPlayer();
            PasangUrp();
            TambahShaderWajib();
            AssetDatabase.SaveAssets();
        }

        static void DaftarTagLayer()
        {
            var aset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (aset == null || aset.Length == 0) throw new Exception("TagManager.asset tidak ditemukan");
            var so = new SerializedObject(aset[0]);
            var tags = so.FindProperty("tags");
            var ada = new HashSet<string>();
            for (int i = 0; i < tags.arraySize; i++) ada.Add(tags.GetArrayElementAtIndex(i).stringValue);
            foreach (var t in TagTambahan)
            {
                if (ada.Contains(t)) continue;
                tags.InsertArrayElementAtIndex(tags.arraySize);
                tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = t;
            }
            var layers = so.FindProperty("layers");
            for (int i = 0; i < NamaLayer.Length; i++) layers.GetArrayElementAtIndex(GameLayers.Ground + i).stringValue = NamaLayer[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            Catat("Tag " + TagTambahan.Length + " & layer 6-18 terdaftar");
        }

        static void AturPlayer()
        {
            PlayerSettings.companyName = "AlasKeramat";
            PlayerSettings.productName = "Alas Keramat";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.alaskeramat.game");
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            Catat("Player Settings: Linear, landscape, com.alaskeramat.game");
        }

        static void PasangUrp()
        {
            EnsureFolder(FolderRender);
            var data = ScriptableObject.CreateInstance<UniversalRendererData>();
            var ppd = AssetDatabase.LoadAssetAtPath<PostProcessData>(PathPostProcessData);
            if (ppd != null) data.postProcessData = ppd;
            AssetDatabase.CreateAsset(data, PathUrpRenderer);
            var urp = UniversalRenderPipelineAsset.Create(data);
            urp.shadowDistance = JarakBayangan;
            urp.msaaSampleCount = 1;
            urp.supportsHDR = true;
            AssetDatabase.CreateAsset(urp, PathUrpAsset);
            AssetDatabase.SaveAssets();

            urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PathUrpAsset);
            GraphicsSettings.defaultRenderPipeline = urp;
            int awal = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = urp;
            }
            QualitySettings.SetQualityLevel(awal, false);
            Catat("Aset URP dipasang di " + QualitySettings.names.Length + " level Quality");
        }

        static void TambahShaderWajib()
        {
            var gs = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
            if (gs == null || gs.Length == 0) throw new Exception("GraphicsSettings.asset tidak ditemukan");
            var so = new SerializedObject(gs[0]);
            var arr = so.FindProperty("m_AlwaysIncludedShaders");
            for (int i = arr.arraySize - 1; i >= 0; i--)
            {
                var sh0 = arr.GetArrayElementAtIndex(i).objectReferenceValue as Shader;
                if (sh0 != null && sh0.name.StartsWith(AwalanUrp)) arr.DeleteArrayElementAtIndex(i);
            }
            foreach (var nama in ShaderWajib)
            {
                var sh = Shader.Find(nama);
                if (sh == null) { Debug.LogWarning("[AK][Builder] Shader tidak ditemukan: " + nama); continue; }
                bool ada = false;
                for (int i = 0; i < arr.arraySize; i++) if (arr.GetArrayElementAtIndex(i).objectReferenceValue == sh) ada = true;
                if (ada) continue;
                arr.InsertArrayElementAtIndex(arr.arraySize);
                arr.GetArrayElementAtIndex(arr.arraySize - 1).objectReferenceValue = sh;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string induk = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(induk);
            AssetDatabase.CreateFolder(induk, System.IO.Path.GetFileName(path));
        }
    }
}
