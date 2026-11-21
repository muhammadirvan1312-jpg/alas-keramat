using AlasKeramat.Building;
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

            if (!NusantaraMasterBuilder.LastBuildSucceeded || NusantaraMasterBuilder.HitungObjek() == 0) NusantaraMasterBuilder.BuildAll();
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
            foreach (var b in Object.FindObjectsByType<Button>(FindObjectsSortMode.None)) { nTombol++; if (b.onClick.GetPersistentEventCount() == 0 && b.GetComponent<AlasKeramat.UI.TanganTahan>() == null) handler = false; }
            cek("Semua tombol HUD punya handler (" + nTombol + ")", nTombol >= 2 && handler);
            var koper = Object.FindObjectsByType<AlasKeramat.Inventory.WadahItem>(FindObjectsSortMode.None);
            int kOk = 0;
            foreach (var k in koper)
            {
                int idx = k.name == "Koper_K1" ? 0 : k.name == "Koper_K2" ? 1 : k.name == "Koper_K3" ? 2 : -1;
                if (idx < 0) continue;
                bool sama = k.DaftarItem.Length == NusantaraMasterBuilder.IsiKoper[idx].Length;
                for (int i = 0; sama && i < k.DaftarItem.Length; i++)
                    sama = k.DaftarItem[i] == NusantaraMasterBuilder.IsiKoper[idx][i] && k.DaftarJumlah[i] == NusantaraMasterBuilder.JumlahKoper[idx][i];
                if (sama && k.gameObject.layer == GameLayers.Interactable && k.GetComponent<Collider>() != null) kOk++;
            }
            cek("3 koper berisi item sesuai 03 §9 (" + kOk + ")", kOk == 3);
            var invGo = new GameObject("UjiInventaris");
            try
            {
                var inv = invGo.AddComponent<AlasKeramat.Inventory.TikarPandan3DInventory>();
                int masuk = inv.Tambah(ItemID.GolokPusaka, 3);
                cek("Inventaris menghormati Maks (golok 3 -> " + masuk + ")", masuk == AlasKeramat.Inventory.ItemDatabase.MaxCarry(ItemID.GolokPusaka) && inv.Jumlah(ItemID.GolokPusaka) == masuk);
            }
            finally { Object.DestroyImmediate(invGo); }
            cek("Radar, toast, interaksi, tangan kiri ada", Object.FindFirstObjectByType<AlasKeramat.UI.RadarGPS>() != null && Object.FindFirstObjectByType<AlasKeramat.UI.ToastHUD>() != null
                && Object.FindFirstObjectByType<AlasKeramat.Player.InteraksiAndi>() != null && Object.FindFirstObjectByType<AlasKeramat.Player.TanganKiri>() != null);
            var rDepan = AlasKeramat.UI.RadarGPS.KeRadar(Vector3.zero, new Vector3(0f, 0f, 50f), 0f);
            var rPutar = AlasKeramat.UI.RadarGPS.KeRadar(Vector3.zero, new Vector3(50f, 0f, 0f), 90f);
            cek("Radar: depan = atas (saat menghadap utara & timur)", rDepan.y > 40f && Mathf.Abs(rDepan.x) < 1f && rPutar.y > 40f && Mathf.Abs(rPutar.x) < 1f);
            var volG = Object.FindFirstObjectByType<UnityEngine.Rendering.Volume>();
            UnityEngine.Rendering.Universal.Tonemapping tmCek = null; UnityEngine.Rendering.Universal.Bloom blCek = null;
            cek("Volume global + ACES + bloom", volG != null && volG.isGlobal && volG.sharedProfile != null
                && volG.sharedProfile.TryGet(out tmCek) && volG.sharedProfile.TryGet(out blCek));
            var camAndi = Camera.main;
            cek("Kamera utama pakai pasca-proses", camAndi != null && UnityEngine.Rendering.Universal.CameraExtensions.GetUniversalAdditionalCameraData(camAndi).renderPostProcessing);
            cek("Pasca-proses malam lebih gelap & dingin dari siang", AKPascaProses.Target(23f, WeatherState.Cerah).x < AKPascaProses.Target(12f, WeatherState.Cerah).x
                && AKPascaProses.Target(23f, WeatherState.Cerah).z < AKPascaProses.Target(12f, WeatherState.Cerah).z);
            cek("Vital Andi & bar HUD ada", Object.FindFirstObjectByType<AlasKeramat.Player.AndiVitalsAndSanity>() != null && Object.FindFirstObjectByType<AlasKeramat.UI.HUDVital>() != null);
            var vGo = new GameObject("UjiVital");
            try
            {
                var v = vGo.AddComponent<AlasKeramat.Player.AndiVitalsAndSanity>();
                float h0 = v.Hydration, k0 = v.Fullness, r0 = v.Rest;
                var kd = new AlasKeramat.Player.AndiVitalsAndSanity.Keadaan { jam = 9f, hari = 7, cuaca = WeatherState.Berawan, biome = Biome.HutanCampuran };
                for (int i = 0; i < 600; i++) v.Tick(kd, 0.1f);
                float dh = h0 - v.Hydration, dk = k0 - v.Fullness, dr = r0 - v.Rest;
                cek("Laju vital 60 s sesuai 05 (haus -" + dh.ToString("0.00") + ", kenyang -" + dk.ToString("0.00") + ", istirahat -" + dr.ToString("0.00") + ")",
                    Mathf.Abs(dh - 5.1f) < 0.2f && Mathf.Abs(dk - 3.3f) < 0.2f && Mathf.Abs(dr - 2.1f) < 0.2f);
            }
            finally { Object.DestroyImmediate(vGo); }
            cek("Plafon HP 05 §3.1 (lapar 10, haus 10 -> " + AlasKeramat.Player.AndiVitalsAndSanity.PlafonHP(10f, 10f, 50f).ToString("0") + ")", Mathf.Approximately(AlasKeramat.Player.AndiVitalsAndSanity.PlafonHP(10f, 10f, 50f), 100f - 18f - 24f));
            cek("Suhu udara: siang kemarau > malam", AlasKeramat.Player.AndiVitalsAndSanity.SuhuUdara(13f, true, Biome.HutanCampuran, 0f, WeatherState.Cerah) > AlasKeramat.Player.AndiVitalsAndSanity.SuhuUdara(2f, true, Biome.HutanCampuran, 0f, WeatherState.Cerah) + 8f);
            int foleyOk = 0;
            foreach (var nm in ProceduralAudioSynth.KlipFoley) { var c = AssetDatabase.LoadAssetAtPath<AudioClip>(NusantaraMasterBuilder.FolderAudio + "/" + nm + ".wav"); if (c != null && c.length > 0.1f) foleyOk++; }
            cek("Klip foley " + foleyOk + "/" + ProceduralAudioSynth.KlipFoley.Length, foleyOk == ProceduralAudioSynth.KlipFoley.Length);
            cek("Permukaan langkah: pantai=pasir, air=air, struktur=kayu", AlasKeramat.Player.FoleyAndi.Permukaan(Biome.Pantai, false, false) == SurfaceType.Pasir
                && AlasKeramat.Player.FoleyAndi.Permukaan(Biome.HutanCampuran, true, false) == SurfaceType.Air && AlasKeramat.Player.FoleyAndi.Permukaan(Biome.Desa, false, true) == SurfaceType.Kayu);
            cek("Foley, subtitle, sabuk cepat ada", Object.FindFirstObjectByType<AlasKeramat.Player.FoleyAndi>() != null && Object.FindFirstObjectByType<AlasKeramat.UI.SubtitleSystem>() != null
                && Object.FindFirstObjectByType<AlasKeramat.UI.SabukCepat>() != null);
            cek("Subtitle: lama tampil min 2,5 s", Mathf.Approximately(AlasKeramat.UI.SubtitleSystem.LamaTampil("Halo"), 2.5f));
            var semuaAnim = Object.FindObjectsByType<AlasKeramat.AI.ProceduralAnimator>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int karOk = 0, karTotal = 0; string karGagal = "";
            foreach (var an in semuaAnim)
            {
                karTotal++;
                var smrs = an.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                bool isAndi = an.name == "ModelAndi";
                int tri = 0; foreach (var r in smrs) if (r.sharedMesh != null) tri += r.sharedMesh.triangles.Length / 3;
                bool namaOk = true;
                foreach (var nm in Kerangka.Nama) if (FindRekursif(an.transform, nm) == null) namaOk = false;
                bool real=an.GetComponent<PenandaModelReal>()!=null;
                bool ok = smrs.Length == (isAndi ? 2 : 1) && tri >= 400 && tri <= (real?30000:6500) && namaOk;
                if (ok) karOk++; else karGagal += " " + an.name + "(smr " + smrs.Length + ", tri " + tri + ", nama " + namaOk + ")";
            }
            cek("Karakter: 1 SkinnedMeshRenderer (Andi 2), segitiga sesuai anggaran (real <=30000), 24 sendi bernama (" + karOk + "/" + karTotal + ")" + karGagal, karTotal >= 14 && karOk == karTotal);
            var tahi = GameObject.Find("TahiLalat");
            bool tahiOk = false;
            if (tahi != null)
            {
                var kepalaA = tahi.transform.parent;
                Vector3 rel = kepalaA.InverseTransformPoint(tahi.transform.position) - new Vector3(0f, 0.085f, 0.01f); // ruang lokal kepala (Andi berputar menghadap heli)
                tahiOk = Vector3.Distance(rel, new Vector3(-0.036f, -0.014f, 0.081f)) < 0.003f && kepalaA.name == "Head";
            }
            cek("Tahi lalat Andi di bawah mata kiri (02 §2)", tahiOk);
            cek("Deden di heli & Ningsih di Sarang", GameObject.Find("KangDeden") != null && GameObject.Find("Ningsih") != null);
            var cutS = new float[4]; cutS[AlasKeramat.Building.DirectionalTree.Selatan] = 120f;
            cek("Pohon rebah menjauhi takikan (tebang dari selatan -> rebah ke utara)", AlasKeramat.Building.DirectionalTree.Kuadran(Vector3.back) == AlasKeramat.Building.DirectionalTree.Selatan
                && Vector3.Dot(AlasKeramat.Building.DirectionalTree.ArahRebah(cutS, Vector3.back), Vector3.forward) > 0.99f);
            cek("Takikan seri -> rebah menjauhi Andi", Vector3.Dot(AlasKeramat.Building.DirectionalTree.ArahRebah(new float[4], Vector3.right), Vector3.left) > 0.99f);
            cek("Hasil kayu 07 §1 (jati 4, aren 3, kelapa 2; HP jati 120)", AlasKeramat.Building.DirectionalTree.JumlahKayu(ProcPohon.Jenis.Jati) == 4 && AlasKeramat.Building.DirectionalTree.JumlahKayu(ProcPohon.Jenis.Aren) == 3
                && AlasKeramat.Building.DirectionalTree.JumlahKayu(ProcPohon.Jenis.Kelapa) == 2 && Mathf.Approximately(AlasKeramat.Building.DirectionalTree.HPTebang(ProcPohon.Jenis.Jati), 120f));
            cek("Senjata terbaik (golok saja -> tebang 20; + kapak batu -> 28)", Mathf.Approximately(AlasKeramat.Player.CombatAndLeftHandController.Terbaik(id => id == ItemID.GolokPusaka).tebang, 20f)
                && Mathf.Approximately(AlasKeramat.Player.CombatAndLeftHandController.Terbaik(id => id == ItemID.GolokPusaka || id == ItemID.KapakBatu).tebang, 28f));
            cek("Pool pohon, melee, senjata FP, tombol TEBAS", Object.FindFirstObjectByType<AlasKeramat.Building.PohonInteraktif>() != null && Object.FindFirstObjectByType<AlasKeramat.Player.CombatAndLeftHandController>() != null
                && GameObject.Find("SenjataFP") != null && Object.FindFirstObjectByType<AlasKeramat.UI.TombolTahan>() != null && AlasKeramat.UI.TombolTahan.Berat(0.5f) && !AlasKeramat.UI.TombolTahan.Berat(0.1f));
            ItemID[] hid; int[] hn;
            AlasKeramat.AI.WildAnimalAI.Hasil(ResepHewan.JenisHewan.Rusa, out hid, out hn);
            bool rusaOk = hid.Length == 3 && hid[0] == ItemID.DagingMentah && hn[0] == 3 && hn[1] == 1 && hn[2] == 2;
            AlasKeramat.AI.WildAnimalAI.Hasil(ResepHewan.JenisHewan.BabiHutan, out hid, out hn);
            cek("Hasil buruan 03 §8 (rusa daging 3/kulit 1/tulang 2; babi + tengkorak)", rusaOk && System.Array.IndexOf(hid, ItemID.Tengkorak) >= 0 && hn[0] == 4);
            var ecoD = Object.FindFirstObjectByType<AlasKeramat.AI.EcosystemDirector>();
            var hewanT = Object.FindObjectsByType<AlasKeramat.AI.WildAnimalAI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int hOk = 0;
            foreach (var h in hewanT) { bool ok = h.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length == 1 && h.GetComponent<Collider>() != null; foreach (var nm in ResepHewan.Nama) if (FindRekursif(h.transform, nm) == null) ok = false; if (ok) hOk++; }
            cek("5 template hewan (1 SMR, 17 sendi, collider) (" + hOk + ")", ecoD != null && hOk >= 5);
            cek("Rusa cocok di hutan jati, tidak di pantai", AlasKeramat.AI.EcosystemDirector.Cocok(ResepHewan.JenisHewan.Rusa, Biome.HutanJatiSelatan) && !AlasKeramat.AI.EcosystemDirector.Cocok(ResepHewan.JenisHewan.Rusa, Biome.Pantai));
            var fdGo = new GameObject("UjiPungut");
            try
            {
                var fdU = fdGo.AddComponent<AlasKeramat.Building.ForagingDirector>();
                fdU.Bangkitkan();
                var adaJenis = new bool[7];
                for (int i = 0; i < fdU.JumlahTitik; i++) adaJenis[(int)fdU.JenisTitik(i)] = true;
                bool semuaJenis = true; foreach (var a in adaJenis) semuaJenis &= a;
                cek("Titik pungut deterministik (" + fdU.JumlahTitik + ", 7 jenis ada)", fdU.JumlahTitik > 1500 && semuaJenis);
                var calon = new int[256]; bool sama = true, hemat = true;
                foreach (int titik in new[] { 0, fdU.JumlahTitik / 3, fdU.JumlahTitik - 1 })
                {
                    Vector3 pusat = fdU.PosisiTitik(titik); int jumlah = fdU.CariDalamRadius(pusat, 22f, calon);
                    var cocok = new HashSet<int>(); for (int i = 0; i < jumlah; i++) cocok.Add(calon[i]);
                    int penuh = 0; for (int i = 0; i < fdU.JumlahTitik; i++) if ((fdU.PosisiTitik(i) - pusat).sqrMagnitude <= 22f * 22f) { penuh++; sama &= cocok.Contains(i); }
                    sama &= penuh == jumlah; hemat &= fdU.KandidatDiperiksa < fdU.JumlahTitik / 10;
                }
                cek("Pungutan: indeks sel sama dengan scan penuh, memeriksa kurang dari 10 persen titik", sama && hemat);
                fdU.Ambil(0); string snapshotPungut = fdU.CaptureJson(); bool habis = !fdU.Tersedia(0);
                fdU.RestoreJson("{\"id\":[],\"hari\":[]}"); bool tersedia = fdU.Tersedia(0); fdU.RestoreJson(snapshotPungut);
                cek("Pungutan: hasil panen tidak muncul kembali setelah muat", habis && tersedia && !fdU.Tersedia(0) && fdU.CaptureJson() == snapshotPungut);
            }
            finally { Object.DestroyImmediate(fdGo); }
            var ujiGo = new GameObject("UjiTikar");
            try
            {
                var inv2 = ujiGo.AddComponent<AlasKeramat.Inventory.TikarPandan3DInventory>();
                inv2.Tambah(ItemID.Bambu, 1); inv2.Tambah(ItemID.TaliIjuk, 1); inv2.Tambah(ItemID.GolokPusaka, 1);
                var r2 = AlasKeramat.Inventory.TikarPanggung.CariResep(new List<ItemID> { ItemID.Bambu, ItemID.TaliIjuk }, inv2);
                bool rakit = r2.HasValue && r2.Value.output.id == ItemID.TombakBambu && AlasKeramat.Inventory.TikarPanggung.Rakit(r2.Value, inv2);
                cek("Rakit di tikar: Bambu + Tali (+ golok) -> Tombak Bambu", rakit && inv2.Punya(ItemID.TombakBambu) && !inv2.Punya(ItemID.Bambu));
                var v2 = ujiGo.AddComponent<AlasKeramat.Player.AndiVitalsAndSanity>();
                v2.PakaiZirah(ItemID.ZirahDaun, false);
                float hp0 = v2.Health;
                v2.TakeDamage(new DamageInfo { amount = 10f, type = DamageType.Tebas });
                cek("Zirah daun (8) menyerap 8 dari 10 damage", Mathf.Abs((hp0 - v2.Health) - 2f) < 0.01f && v2.JumlahSegmen == 0);
            }
            finally { Object.DestroyImmediate(ujiGo); }
            cek("Panggung tikar, layar tikar, tombol TAS", Object.FindFirstObjectByType<AlasKeramat.Inventory.TikarPanggung>() != null && Object.FindFirstObjectByType<AlasKeramat.UI.TikarUI>() != null && GameObject.Find("TombolTas") != null);
            var ujiB = new GameObject("UjiBangun");
            try
            {
                Vector3 o = new Vector3(0f, -500f, 0f);
                System.Func<ShoulderLogAndFreeformBuilder.ModeBangun, Vector3, Quaternion, float, bool, AlasKeramat.Building.BagianStruktur> mk = (m, p, r, len, tanam) =>
                {
                    var b = ShoulderLogAndFreeformBuilder.Buat(new ShoulderLogAndFreeformBuilder.Rencana { mode = m, pos = p, rot = r, panjang = len, tertanam = tanam, sah = true }, null);
                    b.transform.SetParent(ujiB.transform, true); return b;
                };
                var tA = mk(ShoulderLogAndFreeformBuilder.ModeBangun.Tiang, o, Quaternion.identity, 3.6f, true);
                var tB = mk(ShoulderLogAndFreeformBuilder.ModeBangun.Tiang, o + Vector3.right * 3f, Quaternion.identity, 3.6f, true);
                var bal = mk(ShoulderLogAndFreeformBuilder.ModeBangun.Balok, o + new Vector3(1.5f, 1f, 0f), Quaternion.FromToRotation(Vector3.up, Vector3.right), 3f, false);
                var pap = mk(ShoulderLogAndFreeformBuilder.ModeBangun.Papan, o + new Vector3(1.5f, 1.3f, 0.5f), Quaternion.FromToRotation(Vector3.up, Vector3.forward), 1f, false);
                bal.Hubungkan(tA); bal.Hubungkan(tB); pap.Hubungkan(bal);
                bool snap = bal.Jenis == AlasKeramat.Building.BagianStruktur.Tipe.Balok && pap.Jenis == AlasKeramat.Building.BagianStruktur.Tipe.Papan && bal.Tetangga.Count == 3 && Mathf.Approximately(bal.HP, 200f) && Mathf.Approximately(pap.HP, 120f);
                tA.Tetangga.Remove(bal); bal.Tetangga.Remove(tA);
                int sisaSatu = AlasKeramat.Building.StructureIntegrity.TakTertopang(new[] { bal }).Count;
                tB.Tetangga.Remove(bal); bal.Tetangga.Remove(tB);
                int tanpaTiang = AlasKeramat.Building.StructureIntegrity.TakTertopang(new[] { bal }).Count;
                cek("Snap tiang -> balok -> papan valid (HP 200/120)", snap);
                cek("Integritas BFS: satu tiang hilang tetap berdiri, kedua hilang -> balok & papan runtuh (" + sisaSatu + "/" + tanpaTiang + ")", sisaSatu == 0 && tanpaTiang == 2);
            }
            finally { Object.DestroyImmediate(ujiB); }
            cek("Pemanggul kayu & tombol LEMPAR/MODE", Object.FindFirstObjectByType<ShoulderLogAndFreeformBuilder>() != null && GameObject.Find("BahuKiri") != null);
            var tbl = GuidebookBlueprintsAndCooking.Tabel;
            var idUnik = new HashSet<BlueprintID>(); bool bpOk = tbl.Length == 26;
            foreach (var bp in tbl) { bpOk &= idUnik.Add(bp.id) && (int)bp.id >= 1 && (int)bp.id <= 26; foreach (var c in bp.cost) bpOk &= AlasKeramat.Inventory.ItemDatabase.Has(c.id) && c.count > 0; }
            var api = GuidebookBlueprintsAndCooking.Ambil(BlueprintID.ApiUnggun); var saung = GuidebookBlueprintsAndCooking.Ambil(BlueprintID.SaungPanggung);
            bpOk &= api.cost.Length == 2 && api.cost[0].id == ItemID.Ranting && api.cost[0].count == 6 && api.cost[1].count == 4 && saung.cost[0].count == 10 && saung.cost[2].count == 16;
            cek("26 blueprint, biaya sesuai 07 §7 (api: ranting 6 + batu 4; saung: kayu 10/ranting 8/daun 16)", bpOk);
            cek("Masak: ikan mentah 30 s = matang, 50 s = gosong", ApiUnggun.KeadaanMasak(ItemID.IkanMentah, 30f) == ItemID.IkanMatang && ApiUnggun.KeadaanMasak(ItemID.IkanMentah, 50f) == ItemID.MakananGosong && ApiUnggun.KeadaanMasak(ItemID.IkanMentah, 10f) == ItemID.IkanMentah);
            cek("Jemuran: ikan matang -> ikan kering", FungsiBangunan.HasilJemur(ItemID.IkanMatang) == ItemID.IkanKering);
            var ujiS = new GameObject("UjiSimpan");
            try
            {
                var invS = ujiS.AddComponent<AlasKeramat.Inventory.TikarPandan3DInventory>();
                var jamS = ujiS.AddComponent<TimeOfDayClock>();
                invS.Tambah(ItemID.Batu, 7); invS.Tambah(ItemID.GolokPusaka, 1); jamS.Maju(5.5f);
                var daftar = new ISaveable[] { invS, jamS };
                string a = SaveSystem.KeJson(daftar);
                invS.Kurangi(ItemID.Batu, 5); jamS.Maju(3f);
                SaveSystem.DariJson(a, daftar);
                string b = SaveSystem.KeJson(daftar);
                cek("Simpan -> muat menghasilkan state identik", a.Substring(a.IndexOf("\"data\"")) == b.Substring(b.IndexOf("\"data\"")) && invS.Jumlah(ItemID.Batu) == 7);
            }
            finally { Object.DestroyImmediate(ujiS); }
            cek("Buku Panduan, SaveSystem, penempatan", Object.FindFirstObjectByType<GuidebookBlueprintsAndCooking>() != null && Object.FindFirstObjectByType<SaveSystem>() != null && Object.FindFirstObjectByType<AlasKeramat.UI.BukuPanduanUI>() != null);
            PemeriksaanT6c.Jalankan(cek);
            PemeriksaanT7.Jalankan(cek);
            PemeriksaanT7.JalankanB(cek);
            PemeriksaanT7c.Jalankan(cek);
            PemeriksaanT7c.JalankanD(cek);
            PemeriksaanT8.Jalankan(cek);
            PemeriksaanT9.Jalankan(cek);
            PemeriksaanT10.Jalankan(cek);
            PemeriksaanMixamo.Jalankan(cek);
            PemeriksaanPropDesa.Jalankan(cek);
            PemeriksaanEkspansiSOTF.Jalankan(cek);
            PemeriksaanEkspansiDunia2.Jalankan(cek);
            PemeriksaanFiturBaruSOTF.Jalankan(cek);
            PemeriksaanOverhaulVisual.Jalankan(cek);
            PemeriksaanSistemLanjutan.Jalankan(cek);
            PemeriksaanKontrolDanDunia.Jalankan(cek);
            PemeriksaanEkspansiPulau4Km.Jalankan(cek);
            PemeriksaanG3.Jalankan(cek);
            PemeriksaanT11.Jalankan(cek);
            PemeriksaanUjiMain.Jalankan(cek);
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
    
        static Transform FindRekursif(Transform t, string nama)
        {
            if (t.name == nama) return t;
            for (int i = 0; i < t.childCount; i++) { var r = FindRekursif(t.GetChild(i), nama); if (r != null) return r; }
            return null;
        }
    }
}
