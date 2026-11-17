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
            cek("Landmark >= 35 (" + NusantaraMasterBuilder.JumlahLandmark + ")", NusantaraMasterBuilder.JumlahLandmark >= 35);
            cek("Beringin Agung & altar & cerobong ada", GameObject.Find("BeringinAgung") != null && GameObject.Find("Altar") != null && GameObject.Find("Cerobong") != null);
            var amb = Object.FindFirstObjectByType<AmbienceDirector>();
            cek("AmbienceDirector ada", amb != null);
            cek("Emitter ambien >= 1000 (" + (amb != null ? amb.JumlahEmitter : 0) + ")", amb != null && amb.JumlahEmitter >= 1000);
            int klipAmb = 0;
            foreach (var nm in ProceduralAudioSynth.KlipAmbien)
            {
                var c = AssetDatabase.LoadAssetAtPath<AudioClip>(NusantaraMasterBuilder.FolderAudio + "/" + nm + ".wav");
                if (c != null && c.length > 0.2f) klipAmb++;
            }
            cek("Klip ambien " + klipAmb + "/" + ProceduralAudioSynth.KlipAmbien.Length, klipAmb == ProceduralAudioSynth.KlipAmbien.Length);
            cek("Bed: pantai siang", AmbienceDirector.PilihBed(Biome.Pantai, 10f) == ProceduralAudioSynth.BedPantai);
            cek("Bed: hutan dini hari", AmbienceDirector.PilihBed(Biome.HutanCampuran, 2.5f) == ProceduralAudioSynth.BedDini);
            cek("Bed: Alas Wingit", AmbienceDirector.PilihBed(WorldTerrainGenerator.AlasWingit, 12f) == ProceduralAudioSynth.BedWingit);
            cek("Bed: hutan Maghrib -> malam", AmbienceDirector.PilihBed(Biome.HutanJatiSelatan, 19f) == ProceduralAudioSynth.BedMalam);
            foreach (var sn in new[] { NusantaraMasterBuilder.ShaderDaun, NusantaraMasterBuilder.ShaderKulit, NusantaraMasterBuilder.ShaderAir })
            {
                var sh = Shader.Find(sn);
                cek("Shader " + sn + " ada & tanpa error", sh != null && !ShaderUtil.ShaderHasError(sh));
            }
            var mDaun = AssetDatabase.LoadAssetAtPath<Material>(NusantaraMasterBuilder.FolderMat + "/M_DaunPohon.mat");
            cek("Daun pakai shader AK", mDaun != null && mDaun.shader.name == NusantaraMasterBuilder.ShaderDaun && mDaun.enableInstancing);
            var mSungai = AssetDatabase.LoadAssetAtPath<Material>(NusantaraMasterBuilder.FolderMat + "/M_Sungai.mat");
            cek("Sungai pakai shader air AK", mSungai != null && mSungai.shader.name == NusantaraMasterBuilder.ShaderAir);
            cek("AKAngin ada", Object.FindFirstObjectByType<AKAngin>() != null);
            var andi = Object.FindFirstObjectByType<AlasKeramat.Player.AndiFPSController>();
            cek("Andi ada dengan CharacterController", andi != null && andi.GetComponent<CharacterController>() != null);
            if (andi != null)
            {
                var pa = andi.transform.position;
                cek("Andi spawn dekat heli (" + pa.x.ToString("0") + "," + pa.z.ToString("0") + ")", Mathf.Abs(pa.x - NusantaraMasterBuilder.SpawnAndi.x) < 1f && Mathf.Abs(pa.z - NusantaraMasterBuilder.SpawnAndi.z) < 1f);
                cek("Kamera Andi = MainCamera", andi.Kamera != null && andi.Kamera.GetComponent<Camera>() != null && Camera.main == andi.Kamera.GetComponent<Camera>());
            }
            int listener = 0;
            foreach (var al in Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None)) if (al.enabled) listener++;
            cek("Tepat 1 AudioListener aktif (" + listener + ")", listener == 1);
            var turA = Object.FindFirstObjectByType<AlasKeramat.UI.TurPratinjau>();
            cek("Tur pratinjau mati saat Andi ada", turA != null && !turA.enabled);
            cek("TouchControls & joystick & area geser", Object.FindFirstObjectByType<AlasKeramat.UI.TouchControls>() != null
                && Object.FindFirstObjectByType<AlasKeramat.UI.ZonaJoystick>() != null && Object.FindFirstObjectByType<AlasKeramat.UI.ZonaGeser>() != null);
            bool handler = true; int nTombol = 0;
            foreach (var b in Object.FindObjectsByType<Button>(FindObjectsSortMode.None)) { nTombol++; if (b.onClick.GetPersistentEventCount() == 0) handler = false; }
            cek("Semua tombol HUD punya handler (" + nTombol + ")", nTombol >= 2 && handler);
            cek("HUDInfo ada", Object.FindFirstObjectByType<AlasKeramat.UI.HUDInfo>() != null);
            cek("Gerak: jalan 3,4 / lari 6,2 / jongkok 1,8 m/s", Mathf.Approximately(AlasKeramat.Player.AndiFPSController.KecepatanTarget(1f, false, false), 3.4f)
                && Mathf.Approximately(AlasKeramat.Player.AndiFPSController.KecepatanTarget(1f, true, false), 6.2f)
                && Mathf.Approximately(AlasKeramat.Player.AndiFPSController.KecepatanTarget(1f, false, true), 1.8f)
                && AlasKeramat.Player.AndiFPSController.KecepatanTarget(0.1f, false, false) == 0f);
            cek("Sprint: joystick penuh ke atas", AlasKeramat.UI.ZonaJoystick.PosisiSprint(new Vector2(0f, 1f)) && !AlasKeramat.UI.ZonaJoystick.PosisiSprint(new Vector2(1f, 0f)));
            cek("Label waktu: 18:30 = MAGHRIB", AlasKeramat.UI.HUDInfo.LabelWaktu(18.5f) == "MAGHRIB");

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
