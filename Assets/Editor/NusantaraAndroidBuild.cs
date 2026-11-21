using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.EditorTools
{
    // T11 (11 §15, 12 §T11): Konfigurasi Player Settings Android & Build APK Uji 1-klik.
    public static class NusantaraAndroidBuild
    {
        public const string PathApkUji = "Builds/AlasKeramat_uji.apk";
        public const string PackageId = "com.alaskeramat.game";

        [MenuItem("Nusantara Builder/Android/Atur Player Settings Android", false, 60)]
        public static void ConfigureAndroidSettings()
        {
            TerapkanPengaturanAndroid(true);
        }

        public static void TerapkanPengaturanAndroid(bool il2cpp)
        {
            PlayerSettings.companyName = "AlasKeramat";
            PlayerSettings.productName = "Alas Keramat";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageId);
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.gpuSkinning = true;
            PlayerSettings.bakeCollisionMeshes = true;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Low);

            if (il2cpp)
            {
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
                PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.Android, Il2CppCompilerConfiguration.Release);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            }
            else
            {
                // DIAG: seperti build 18 Nov yang jalan di Samsung A16 (Mono, 32-bit).
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.Mono2x);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
            }
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, true);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Nusantara Builder/Android/Build APK Uji", false, 61)]
        public static void BuildTestApkMenu()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            {
                if (!Application.isBatchMode)
                    EditorUtility.DisplayDialog(
                        "Alas Keramat - Android",
                        "Modul Android belum terpasang. Buka Unity Hub -> Installs -> Add modules -> Android Build Support (centang OpenJDK & SDK/NDK).",
                        "OK");
                Debug.LogError("[AK][Android] Modul Android Build Support belum terpasang.");
                return;
            }
            TerapkanPengaturanAndroid(true);
            string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                NusantaraMasterBuilder.BuildAll();
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            }
            Directory.CreateDirectory(Path.GetDirectoryName(PathApkUji));
            EditorUserBuildSettings.buildAppBundle = false;
            var opsi = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = PathApkUji,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
            var laporan = BuildPipeline.BuildPlayer(opsi);
            if (laporan.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("[AK][Android] APK berhasil dibuat: " + PathApkUji);
                if (!Application.isBatchMode) EditorUtility.RevealInFinder(PathApkUji);
            }
            else
            {
                Debug.LogError("[AK][Android] Build APK gagal: " + laporan.summary.result);
            }
        }
    }
}
