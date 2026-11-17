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
            cek("EventSystem ada", Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null);
            cek("Kamera utama ada", Camera.main != null);

            // ---- T2a: dunia ----
            var terrains = Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None);
            cek("16 tile terrain (" + terrains.Length + ")", terrains.Length == WorldTerrainGenerator.TileSisi * WorldTerrainGenerator.TileSisi);
            cek("Terrain punya 6 lapisan & material URP", terrains.Length > 0 && terrains.All(t => t.terrainData != null && t.terrainData.terrainLayers.Length == 6 && t.materialTemplate != null));
            cek("Laut ada", GameObject.Find("Laut") != null);
            cek("Sungai ada", GameObject.Find("Sungai_0") != null && GameObject.Find("Sungai_1") != null);
            CekTinggi(cek, 0f, -440f, 10.5f, 0.75f);
            CekTinggi(cek, 0f, -18f, 9f, 0.75f);
            CekTinggi(cek, 0f, 690f, 76.5f, 1.5f);
            CekTinggi(cek, 0f, -840f, 2.4f, 0.6f);
            CekTinggi(cek, -560f, 290f, 5.1f, 0.45f);
            cek("Jurang (0,648) < 30", WorldTerrainGenerator.SampleHeight(0f, 648f) < 30f);
            cek("Dasar rawa merah (-470,190) < 3", WorldTerrainGenerator.SampleHeight(-470f, 190f) < 3f);
            cek("Laut di tepi (3 titik) < 0", WorldTerrainGenerator.SampleHeight(-990f, 0f) < 0f && WorldTerrainGenerator.SampleHeight(990f, 0f) < 0f && WorldTerrainGenerator.SampleHeight(0f, 990f) < 0f);
            cek("Bioma Kawah (0,0)", WorldTerrainGenerator.GetBiome(0f, 0f) == Biome.KawahBeringin);
            cek("Bioma Sawit (-520,240)", WorldTerrainGenerator.GetBiome(-520f, 240f) == Biome.SawitTerbakar);
            cek("Bioma Tebing (0,700)", WorldTerrainGenerator.GetBiome(0f, 700f) == Biome.TebingKapur);
            cek("Bioma Pabrik Gula (660,-500)", WorldTerrainGenerator.GetBiome(660f, -500f) == WorldTerrainGenerator.PabrikGula);
            cek("Bioma Gunung Kapur (-600,660)", WorldTerrainGenerator.GetBiome(-600f, 660f) == WorldTerrainGenerator.GunungKapur);
            cek("Bioma Ladang Tebu (620,620)", WorldTerrainGenerator.GetBiome(620f, 620f) == WorldTerrainGenerator.LadangTebu);
            cek("Bioma Alas Wingit (-760,-40)", WorldTerrainGenerator.GetBiome(-760f, -40f) == WorldTerrainGenerator.AlasWingit);
            cek("Bioma Laut (-990,0)", WorldTerrainGenerator.GetBiome(-990f, 0f) == Biome.Laut);
            cek("Gunung Kapur menjulang > 120 m", WorldTerrainGenerator.SampleHeight(-600f, 660f) > 120f);
            var jam = Object.FindFirstObjectByType<TimeOfDayClock>();
            bool jamOk = false;
            if (jam != null) { float a = jam.Jam; jam.Maju(1.5f); jamOk = Mathf.Abs(jam.Jam - (a + 1.5f)) < 0.01f; jam.SetWaktu(TimeOfDayClock.JamMulai, 1); }
            cek("Jam dunia maju", jamOk);
            var atmos = Object.FindFirstObjectByType<URPAtmosphereAndWeather>();
            cek("Atmosfer & langit terpasang", atmos != null && RenderSettings.skybox != null);
            cek("Pohon 8.000-45.000 (" + NusantaraMasterBuilder.JumlahPohon + ")", NusantaraMasterBuilder.JumlahPohon >= 8000 && NusantaraMasterBuilder.JumlahPohon <= 45000);
            cek("Semak/pakis >= 10.000 (" + NusantaraMasterBuilder.JumlahSemak + ")", NusantaraMasterBuilder.JumlahSemak >= 10000);
            cek("VegetationManager & data terpasang", Object.FindFirstObjectByType<AlasKeramat.Building.VegetationManager>() != null && Object.FindFirstObjectByType<AlasKeramat.Building.VegetationManager>().Data != null);
            bool cuacaOk = atmos != null;
            if (atmos != null)
                foreach (WeatherState w in System.Enum.GetValues(typeof(WeatherState)))
                {
                    try { atmos.SetCuaca(w, true); if (atmos.Cuaca != w) cuacaOk = false; }
                    catch (System.Exception e) { cuacaOk = false; Debug.LogError("[AK][SelfTest] cuaca " + w + ": " + e.Message); }
                }
            if (atmos != null) atmos.SetCuaca(WeatherState.Cerah, true);
            cek("Semua 7 cuaca bisa diset", cuacaOk);
            cek("Tur pratinjau di kamera", Object.FindFirstObjectByType<AlasKeramat.UI.TurPratinjau>() != null);

            Directory.CreateDirectory("Logs");
            File.WriteAllLines(PathHasil, hasil.ToArray());
            foreach (var h in hasil) if (h.StartsWith("[GAGAL]")) Debug.LogError("[AK][GAGAL] self-test: " + h);
            return semua;
        }

        static void CekTinggi(System.Action<string, bool> cek, float x, float z, float harap, float toleransi)
        {
            float h = WorldTerrainGenerator.SampleHeight(x, z);
            cek("Tinggi (" + x + "," + z + ") = " + h.ToString("0.00") + " ~ " + harap + " +- " + toleransi, Mathf.Abs(h - harap) <= toleransi);
        }

        static bool GraphicsSettings_DefaultAda() { return UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline != null; }
    }
}
