using System.Collections.Generic;
using System.IO;
using System.Linq;
using AlasKeramat.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.EditorTools
{
    // Self-test editor (docs/tech/11 bagian 12, pemeriksaan T1 dari docs/tech/12). Hasil di Logs/nusantara_selftest.txt.
    public static class NusantaraSelfTest
    {
        const string PathHasil = "Logs/nusantara_selftest.txt";
        const int MinTekstur = 10, MinMaterial = 5, MinWav = 8;

        [MenuItem("Nusantara Builder/Utilitas/Jalankan Self-Test", false, 43)]
        public static void RunFromMenu()
        {
            bool ok = RunAll();
            if (!Application.isBatchMode)
                EditorUtility.DisplayDialog("Self-Test", ok ? "SEMUA LULUS" : "Ada yang GAGAL. Lihat " + PathHasil, "OK");
        }

        public static void RunFromCommandLine() { EditorApplication.Exit(RunAll() ? 0 : 1); }

        public static bool RunAll()
        {
            var hasil = new List<string>();
            bool semua = true;
            System.Action<string, bool> cek = (nama, ok) => { hasil.Add((ok ? "[LULUS] " : "[GAGAL] ") + nama); if (!ok) semua = false; };

            NusantaraMasterBuilder.BuildAll();
            cek("Builder pertama tanpa langkah gagal", NusantaraMasterBuilder.LastBuildSucceeded);
            int objek1 = NusantaraMasterBuilder.HitungObjek();
            NusantaraMasterBuilder.BuildAll();
            cek("Builder kedua tanpa langkah gagal", NusantaraMasterBuilder.LastBuildSucceeded);
            int objek2 = NusantaraMasterBuilder.HitungObjek();
            cek("Idempoten: jumlah objek sama (" + objek1 + " = " + objek2 + ")", objek1 > 0 && objek1 == objek2);

            EditorSceneManager.OpenScene(NusantaraMasterBuilder.PathScene);

            bool layerOk = true;
            for (int i = 0; i < NusantaraMasterBuilder.NamaLayer.Length; i++)
                if (LayerMask.LayerToName(GameLayers.Ground + i) != NusantaraMasterBuilder.NamaLayer[i]) layerOk = false;
            cek("Layer 6-18 terdaftar", layerOk);
            var tagAda = new HashSet<string>(UnityEditorInternal.InternalEditorUtility.tags);
            cek("Semua tag terdaftar", NusantaraMasterBuilder.TagTambahan.All(tagAda.Contains));

            bool urpOk = GraphicsSettings_DefaultAda();
            for (int i = 0; i < QualitySettings.names.Length; i++) if (QualitySettings.GetRenderPipelineAssetAt(i) == null) urpOk = false;
            cek("Aset URP terpasang di semua level Quality", urpOk);

            int tex = AssetDatabase.FindAssets("t:Texture2D", new[] { NusantaraMasterBuilder.FolderTex }).Length;
            cek("Tekstur >= " + MinTekstur + " (" + tex + ")", tex >= MinTekstur);
            var mats = AssetDatabase.FindAssets("t:Material", new[] { NusantaraMasterBuilder.FolderGen })
                .Select(g => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g))).ToList();
            cek("Material >= " + MinMaterial + " (" + mats.Count + ")", mats.Count >= MinMaterial);
            cek("Tidak ada material ber-shader error", mats.All(m => m != null && m.shader != null && !ShaderUtil.ShaderHasError(m.shader) && m.shader.name != "Hidden/InternalErrorShader"));
            cek("GetSafeFont() tidak null", ProceduralAssetFactory.GetSafeFont() != null);
            int wav = Directory.Exists(NusantaraMasterBuilder.FolderAudio) ? Directory.GetFiles(NusantaraMasterBuilder.FolderAudio, "*.wav").Length : 0;
            cek("WAV >= " + MinWav + " (" + wav + ")", wav >= MinWav);
            cek("Scene tersimpan & terdaftar di Build Settings", File.Exists(NusantaraMasterBuilder.PathScene)
                && EditorBuildSettings.scenes.Length > 0 && EditorBuildSettings.scenes[0].path == NusantaraMasterBuilder.PathScene);
            var synth = Object.FindFirstObjectByType<ProceduralAudioSynth>();
            cek("Synth punya klip dipanggang", synth != null && synth.BakedClipCount >= ProceduralAudioSynth.KlipWajib.Length);
            var teks = Object.FindObjectsByType<Text>(FindObjectsSortMode.None);
            cek("Teks judul T1 ada", teks.Any(t => t.text == NusantaraMasterBuilder.JudulT1));
            cek("5 tombol suara", Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Length >= 5);
            cek("EventSystem ada", Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null);
            cek("Kamera utama ada", Camera.main != null);

            Directory.CreateDirectory("Logs");
            File.WriteAllLines(PathHasil, hasil.ToArray());
            foreach (var h in hasil) if (h.StartsWith("[GAGAL]")) Debug.LogError("[AK][GAGAL] self-test: " + h);
            return semua;
        }

        static bool GraphicsSettings_DefaultAda() { return UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline != null; }
    }
}
