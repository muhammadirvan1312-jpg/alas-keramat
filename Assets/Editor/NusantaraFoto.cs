using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using AlasKeramat.Core;
using UnityEngine.Rendering;

namespace AlasKeramat.EditorTools
{
    // Foto Kualitas (docs/gdd/15 §8, 19 §8): merender kamera ke PNG di Logs/foto_kualitas/.
    // Di CI berjalan di bawah xvfb + render perangkat lunak; bila tidak ada perangkat grafis -> dilewati.
    public static class NusantaraFoto
    {
        public const string Folder = "Logs/foto_kualitas";
        const int Lebar = 960, Tinggi = 540;
        public const int JumlahFotoAudit = 12;
        const double BatasDetik = 1200; // anggaran total render (render perangkat lunak bisa lambat)
        const string PathLog = "Logs/ci/foto.log";

        // Posisi & target: (x, tinggi di atas tanah/air, z) dalam koordinat dunia; jam = waktu game saat difoto.
        public struct Shot
        {
            public string nama; public Vector3 posisi; public Vector3 target; public float jam; public float fov; public int cuaca;
            public Shot(string n, Vector3 p, Vector3 t, float j, float f = 60f, int c = 0) { nama = n; posisi = p; target = t; jam = j; fov = f; cuaca = c; }
        }

        // Daftar shot aktif (T2a: dunia). Tahap berikutnya menambah shot (12 money shot 19 §8).
        public static readonly Shot[] ShotAktif =
        {
            new Shot("g3_01_deden_wajah", new Vector3(25.2f,1.24f,-437.8f),new Vector3(22.4f,0.94f,-441.2f),9.5f,46f),
            new Shot("g3_02_ningsih_wajah", new Vector3(198.7f,1.36f,-351.6f),new Vector3(200f,1.24f,-350f),14.5f,38f),
            new Shot("g3_03_hutan_malam", new Vector3(-59.6f,1.38f,-305.6f),new Vector3(-59.5f,1.25f,-298.8f),21.5f,62f),
            new Shot("g3_04_lantai_hutan", new Vector3(65f,1.45f,-335f),new Vector3(68f,0.45f,-327f),10.2f,70f),
            new Shot("t10_01_gua_lumut", new Vector3(-623f, -78.2f, -595f), new Vector3(-608f, -78.5f, -602f), 0.5f, 70f),
            new Shot("t10_02_kubah_akar_boss", new Vector3(593.5f, -75.6f, -588.5f), new Vector3(600f, -77.3f, -571f), 0.5f, 70f),
            new Shot("v1_01_heli_pagi_sinar", new Vector3(-16f, 2.4f, -456f), new Vector3(8f, 1.5f, -438f), 7.5f, 60f),
            new Shot("v1_02_desa_senja", new Vector3(500f, 6f, 150f), new Vector3(550f, 3f, 210f), 17.0f, 60f),
            new Shot("v1_03_beringin_maghrib", new Vector3(30f, 4f, -60f), new Vector3(0f, 14f, 0f), 18.3f, 68f),
            new Shot("v1_04_pantai_badai", new Vector3(-110f, 7f, -810f), new Vector3(-185f, 14f, -870f), 16.2f, 62f, (int)WeatherState.HujanBadai),
            new Shot("v1_05_malam_senter", new Vector3(55f, 1.65f, -342f), new Vector3(62f, 1.2f, -328f), 22.0f, 62f),
            new Shot("v1_06_sekte_desa", new Vector3(533f, 1.7f, 190f), new Vector3(540f, 1.1f, 200f), 10f, 58f),
            new Shot("c3_01_props_desa_siang", new Vector3(553f, 1.7f, 213f), new Vector3(564f, 0.9f, 224f), 10.5f, 62f),
            new Shot("c3_02_lampu_minyak_malam", new Vector3(553f, 1.7f, 213f), new Vector3(564f, 0.9f, 224f), 21f, 62f),
            new Shot("e1_01_kemah_sekte", new Vector3(-196f, 2.2f, -168f), new Vector3(-185f, 1.1f, -160f), 19.8f, 62f),
            new Shot("e1_02_pabrik_props", new Vector3(650f, 2.4f, -508f), new Vector3(664f, 1.4f, -496f), 15.5f, 62f),
            new Shot("e1_03_altar_pelita", new Vector3(6f, 2.0f, -26f), new Vector3(0f, 1.4f, -12f), 20.5f, 62f),
            new Shot("e1_04_pantai_hidup", new Vector3(-6f, 2.2f, -826f), new Vector3(6f, 0.6f, -840f), 9.5f, 62f),
            new Shot("e2_01_desa_hidup", new Vector3(530f, 2.1f, 192f), new Vector3(546f, 1.2f, 206f), 10.0f, 62f),
            new Shot("e2_02_kampung_survival", new Vector3(44f, 2.0f, -384f), new Vector3(58f, 1.2f, -370f), 16.0f, 62f),
            new Shot("e2_03_bubu_sungai", new Vector3(244f, 2.2f, -262f), new Vector3(256f, 0.8f, -250f), 8.5f, 62f),
            new Shot("e2_04_mercusuar_pantai", new Vector3(-164f, 2.5f, -846f), new Vector3(-180f, 1.8f, -864f), 17.5f, 62f),
            new Shot("f1_01_zipline_heli", new Vector3(14f, 3.2f, -438f), new Vector3(26f, 3.4f, -426f), 9.0f, 62f),
            new Shot("f1_02_perahu_pantai", new Vector3(-14f, 2.4f, -836f), new Vector3(-4f, 0.6f, -848f), 10.5f, 62f),
            new Shot("f1_03_rumah_pohon", new Vector3(34f, 3.2f, -430f), new Vector3(46f, 5.5f, -418f), 15.0f, 64f),
            new Shot("f1_04_buto_ijo_boss", new Vector3(676f, 2.2f, -512f), new Vector3(688f, 2.4f, -522f), 17.8f, 62f),
            new Shot("v6_01_air_terjun", new Vector3(304f, 3.2f, -182f), new Vector3(312f, 6.2f, -165f), 9.5f, 64f),
            new Shot("v6_02_candi_kuno", new Vector3(-265f, 3.8f, 92f), new Vector3(-265f, 3.4f, 115f), 17.4f, 64f),
            new Shot("v6_03_jembatan_gantung", new Vector3(250f, 5.0f, -234f), new Vector3(272f, 3.5f, -218f), 8.5f, 64f),
            new Shot("v6_04_bangkai_kapal", new Vector3(-96f, 3.5f, -850f), new Vector3(-115f, 2.2f, -865f), 16.8f, 64f),
            new Shot("w4_01_desa_timur", new Vector3(705f, 4.5f, -158f), new Vector3(735f, 2.5f, -138f), 9.0f, 64f),
            new Shot("w4_02_gunung_kapur", new Vector3(-560f, 70f, 595f), new Vector3(-580f, 66f, 620f), 16.5f, 66f),
            new Shot("w4_03_perkebunan", new Vector3(575f, 6f, 530f), new Vector3(595f, 4f, 560f), 10.0f, 64f),
            new Shot("w4_04_pulau_karang", new Vector3(-285f, 7f, -855f), new Vector3(-320f, 3f, -885f), 17.2f, 64f),
            new Shot("t7_02_pocong_kuntilanak_malam", new Vector3(-59.6f, 1.38f, -305.6f), new Vector3(-59.5f, 1.25f, -298.8f), 21.5f, 62f),
            new Shot("t7_01_sekte_ritual_desa", new Vector3(533f, 1.7f, 190f), new Vector3(540f, 1.0f, 200f), 10f, 60f),
            new Shot("t4_01_galeri_karakter", new Vector3(56f, 1.7f, -386f), new Vector3(56f, 1.1f, -376f), 10f, 75f),
            new Shot("t4_02_galeri_dekat", new Vector3(50f, 1.6f, -380f), new Vector3(52f, 1.3f, -376f), 10f, 55f),
            new Shot("t4_03_deden_heli", new Vector3(19f, 1.6f, -434f), new Vector3(24f, 0.9f, -440f), 9f, 55f),
            new Shot("t2_15_beringin_agung", new Vector3(30f, 4f, -60f), new Vector3(0f, 14f, 0f), 18.4f, 70f),
            new Shot("t2_14_pabrik_gula", new Vector3(600f, 8f, -590f), new Vector3(680f, 15f, -500f), 17.2f, 60f),
            new Shot("t2_13_desa_mati", new Vector3(500f, 6f, 150f), new Vector3(550f, 3f, 210f), 16.8f, 60f),
            new Shot("t2_08_maghrib_merah", new Vector3(0f, 90f, -620f), new Vector3(0f, 10f, 0f), 18.3f),
            new Shot("t2_09_malam", new Vector3(-40f, 14f, -520f), new Vector3(0f, 4f, -440f), 22f),
            new Shot("t2_10_hutan_dalam", new Vector3(-150f, 1.7f, -250f), new Vector3(-150f, 3f, -200f), 9.5f, 65f),
            new Shot("t2_11_badai", new Vector3(-40f, 20f, -540f), new Vector3(0f, 6f, -440f), 15f, 60f, (int)WeatherState.HujanBadai),
            new Shot("t2_01_udara_pulau_pagi", new Vector3(0f, 520f, -1250f), new Vector3(0f, 0f, 0f), 8f, 55f),
            new Shot("t2_02_heli_pagi", new Vector3(-40f, 14f, -520f), new Vector3(0f, 4f, -440f), 7.5f),
            new Shot("t2_03_kawah_beringin_siang", new Vector3(90f, 30f, -150f), new Vector3(0f, 6f, 0f), 12f),
            new Shot("t2_04_tebing_kapur_siang", new Vector3(30f, 25f, 470f), new Vector3(0f, 20f, 690f), 13f),
            new Shot("t2_05_sungai_bambu_pagi", new Vector3(300f, 10f, -330f), new Vector3(330f, 2f, -200f), 9f),
            new Shot("t2_06_pantai_sore", new Vector3(60f, 8f, -760f), new Vector3(0f, 0f, -980f), 16.5f),
            new Shot("t2_07_gunung_kapur", new Vector3(-320f, 60f, 380f), new Vector3(-600f, 60f, 660f), 10f),
            new Shot("t2_12_hutan_jati", new Vector3(60f, 2f, -330f), new Vector3(20f, 6f, -300f), 8.5f, 65f),
        };

        static float TinggiDasar(float x, float z) { return Mathf.Max(0f, WorldTerrainGenerator.SampleHeight(x, z)); }

        static void Log(string s) { Debug.Log("[AK][Foto] " + s); File.AppendAllText(PathLog, s + "\n"); }

        public static bool AdaPerangkatGrafis { get { return SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null; } }

        [MenuItem("Nusantara Builder/Debug (saat Play)/Foto Kualitas", false, 80)]
        public static void FotoDariMenu()
        {
            int n = AmbilSemua();
            if (!Application.isBatchMode) EditorUtility.DisplayDialog("Foto Kualitas", n + " foto disimpan di " + Folder, "OK");
        }

        // Mengembalikan jumlah foto yang berhasil; -1 bila tidak ada perangkat grafis.
        public static int AmbilSemua()
        {
            if (!AdaPerangkatGrafis) return -1;
            if (File.Exists(NusantaraMasterBuilder.PathScene)) EditorSceneManager.OpenScene(NusantaraMasterBuilder.PathScene);
            bool asinkronLama = EditorSettings.asyncShaderCompilation;
            EditorSettings.asyncShaderCompilation = false; // hindari shader placeholder (biru muda) di foto
            Directory.CreateDirectory(Folder);
            foreach(var lama in Directory.GetFiles(Folder,"*.png"))File.Delete(lama);
            Directory.CreateDirectory("Logs/ci");File.WriteAllText(PathLog,"");
            string sumber=NusantaraCI.SourceCommit();
            File.WriteAllText(Path.Combine(Folder,"sumber.txt"),sumber+"\n960x540; survei CI, radius foto permukaan 400m, potret 120m\n");
            var daftar = new List<string>();
            var go = new GameObject("AK_KameraFoto");
            try
            {
                var cam = go.AddComponent<Camera>();
                var utama = Camera.main;
                if (utama != null) { cam.CopyFrom(utama); utama.enabled = false; }
                cam.allowHDR = true;
                var dataCam = UnityEngine.Rendering.Universal.CameraExtensions.GetUniversalAdditionalCameraData(cam);
                if (dataCam != null) { dataCam.renderPostProcessing = true; dataCam.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.FastApproximateAntialiasing; }
                var rt = new RenderTexture(Lebar, Tinggi, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
                var atmos = UnityEngine.Object.FindFirstObjectByType<URPAtmosphereAndWeather>();
                var volumeFoto=UnityEngine.Object.FindFirstObjectByType<UnityEngine.Rendering.Volume>();
                var guaFoto=UnityEngine.Object.FindFirstObjectByType<WorldCaveAndSaveManager>();
                var kabutLokal=UnityEngine.Object.FindFirstObjectByType<AtmosferHutan>();
                var serasah=UnityEngine.Object.FindFirstObjectByType<SerasahHutan>();
                var veg = UnityEngine.Object.FindFirstObjectByType<AlasKeramat.Building.VegetationManager>();
                if (veg != null) veg.PengaliJarak = 1.0f;
                // Ringankan terrain untuk render perangkat lunak (tidak disimpan ke scene).
                foreach (var t in UnityEngine.Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None))
                {
                    t.heightmapPixelError = 7f; t.basemapDistance = 420f; t.drawTreesAndFoliage = false;
                }
                var jam = System.Diagnostics.Stopwatch.StartNew();
                Directory.CreateDirectory("Logs/ci");
                Log("Mulai foto, perangkat: " + SystemInfo.graphicsDeviceName);
                cam.targetTexture = rt;

                var akar = GameObject.Find(NusantaraMasterBuilder.NamaRoot);
                var galeriFoto = akar != null ? akar.transform.Find(NusantaraMasterBuilder.NamaGaleri) : null;
                if (galeriFoto != null) galeriFoto.gameObject.SetActive(true);
                var dioramaFoto = akar != null ? akar.transform.Find(NusantaraMasterBuilder.NamaDiorama) : null;
                if (dioramaFoto != null) dioramaFoto.gameObject.SetActive(true);
                var dioHantu = akar != null ? akar.transform.Find(NusantaraMasterBuilder.NamaDioramaHantu) : null;
                if (dioHantu != null) dioHantu.gameObject.SetActive(true);
                foreach (var pa in UnityEngine.Object.FindObjectsByType<AlasKeramat.AI.ProceduralAnimator>(FindObjectsInactive.Include, FindObjectsSortMode.None)) pa.TerapkanPoseFoto(0.65f);
                foreach (var s in ShotAktif)
                {
                    if(Application.isBatchMode && daftar.Count>=JumlahFotoAudit)break;
                    if (jam.Elapsed.TotalSeconds > BatasDetik) { Log("Anggaran waktu habis, sisa shot dilewati"); break; }
                    try
                    {
                        if (atmos != null) { atmos.SetCuaca((WeatherState)s.cuaca, true); atmos.Terapkan(s.jam); }
                        bool bawah = WorldTerrainGenerator.DiBawahTanah(s.posisi.y); // T10: shot gua memakai Y absolut
                        var pos = new Vector3(s.posisi.x, (bawah ? 0f : TinggiDasar(s.posisi.x, s.posisi.z)) + s.posisi.y, s.posisi.z);
                        var tgt = new Vector3(s.target.x, (bawah ? 0f : TinggiDasar(s.target.x, s.target.z)) + s.target.y, s.target.z);
                        if (atmos != null && bawah) atmos.TerapkanRuangan();
                        cam.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(tgt - pos));
                        cam.fieldOfView = s.fov;
                        URPAtmosphereAndWeather.DalamRuangan=bawah;
                        if(bawah && atmos!=null)atmos.TerapkanRuangan();
                        if(guaFoto!=null)guaFoto.TerapkanLampuFoto(pos);
                        cam.farClipPlane=bawah?95f:Application.isBatchMode?420f:2600f;cam.clearFlags=bawah?CameraClearFlags.SolidColor:CameraClearFlags.Skybox;cam.backgroundColor=Color.black;
                        if(volumeFoto!=null)AKPascaProses.TerapkanFoto(volumeFoto.profile,s.jam,(WeatherState)s.cuaca,bawah);
                        if(kabutLokal!=null)kabutLokal.Terapkan(cam,s.jam,(WeatherState)s.cuaca);
                        Light senterTemp = null;
                        if (s.nama == "v1_05_malam_senter")
                        {
                            senterTemp = go.AddComponent<Light>();
                            senterTemp.type = LightType.Spot; senterTemp.range = 28f; senterTemp.spotAngle = 54f;
                            senterTemp.intensity = 4.2f; senterTemp.color = new Color(1f, 0.94f, 0.84f);
                            var sAndi = UnityEngine.Object.FindFirstObjectByType<AlasKeramat.Player.SenterAndi>(FindObjectsInactive.Include);
                            if (sAndi != null) { var ck = new SerializedObject(sAndi).FindProperty("cookie").objectReferenceValue as Texture; if (ck != null) senterTemp.cookie = ck; }
                        }
                        try
                        {
                            Log("Render " + s.nama);
                            if(serasah!=null)serasah.Gambar(cam);
                            if (veg != null) veg.Gambar(cam);
                            cam.Render();
                            if(serasah!=null)serasah.Gambar(cam);
                            if (veg != null) veg.Gambar(cam);
                            cam.Render();
                            SimpanFrameFoto(rt, s.nama);
                            daftar.Add(s.nama);
                            File.WriteAllLines(Path.Combine(Folder,"daftar.txt"),daftar);
                            Log(s.nama + " selesai pada " + jam.Elapsed.TotalSeconds.ToString("0") + " s");
                        }
                        finally { if (senterTemp != null) UnityEngine.Object.DestroyImmediate(senterTemp); }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("[AK][Foto] Shot " + s.nama + " gagal: " + e.Message);
                    }
                }
                if (jam.Elapsed.TotalSeconds < BatasDetik)
                    AmbilCloseUpV1(cam, rt, atmos, volumeFoto, kabutLokal, serasah, veg, daftar, jam);
                URPAtmosphereAndWeather.DalamRuangan=false;
                if (atmos != null) { atmos.SetCuaca(WeatherState.Cerah, true); atmos.Terapkan(TimeOfDayClock.JamMulai); }
                if (veg != null) veg.PengaliJarak = 1f;
                cam.targetTexture = null;
                RenderTexture.active = null;
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
                if (utama != null) utama.enabled = true;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                EditorSettings.asyncShaderCompilation = asinkronLama;
            }
            File.WriteAllLines(Path.Combine(Folder, "daftar.txt"), new[] { "Perangkat grafis: " + SystemInfo.graphicsDeviceName + " (" + SystemInfo.graphicsDeviceType + ")" });
            File.AppendAllLines(Path.Combine(Folder, "daftar.txt"), daftar);
            return daftar.Count;
        }

        static void SimpanFrameFoto(RenderTexture rt, string nama)
        {
            RenderTexture.active = rt;
            var tex = new Texture2D(Lebar, Tinggi, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, Lebar, Tinggi), 0, 0);
            tex.Apply();
            File.WriteAllBytes(Path.Combine(Folder, nama + ".png"), tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        static void AmbilCloseUpV1(Camera cam, RenderTexture rt, URPAtmosphereAndWeather atmos, Volume volumeFoto, AtmosferHutan kabutLokal,
            SerasahHutan serasah, AlasKeramat.Building.VegetationManager veg, List<string> daftar, System.Diagnostics.Stopwatch jam)
        {
            var kr = UnityEngine.Object.FindFirstObjectByType<AlasKeramat.AI.KiRogoPanggung>(FindObjectsInactive.Include);
            PotretKarakterV1("v1_wajah_deden", GameObject.Find("KangDeden"), cam, rt, atmos, volumeFoto, kabutLokal, serasah, veg, daftar, jam);
            PotretKarakterV1("v1_wajah_ningsih", GameObject.Find("Ningsih"), cam, rt, atmos, volumeFoto, kabutLokal, serasah, veg, daftar, jam);
            PotretKarakterV1("v1_wajah_kirogo", kr != null ? kr.gameObject : null, cam, rt, atmos, volumeFoto, kabutLokal, serasah, veg, daftar, jam);
            PotretHewanV1(cam, rt, atmos, volumeFoto, kabutLokal, serasah, veg, daftar, jam);
        }

        static void PotretKarakterV1(string nama, GameObject target, Camera cam, RenderTexture rt, URPAtmosphereAndWeather atmos,
            Volume volumeFoto, AtmosferHutan kabutLokal, SerasahHutan serasah, AlasKeramat.Building.VegetationManager veg, List<string> daftar, System.Diagnostics.Stopwatch jam)
        {
            if (target == null || jam.Elapsed.TotalSeconds > BatasDetik) return;
            GameObject preview = null, keyGo = null, rimGo = null;
            var tersembunyi = new List<Renderer>();
            try
            {
                Transform kepala = null;
                var am = target.GetComponentInChildren<AlasKeramat.AI.AnimasiMixamo>(true);
                GameObject modelPrefab = am != null ? new SerializedObject(am).FindProperty("modelKarakter").objectReferenceValue as GameObject : null;
                if (modelPrefab != null)
                {
                    float bawahLama = float.MaxValue, atasLama = float.MinValue;
                    foreach (var r in target.GetComponentsInChildren<Renderer>(true))
                    {
                        if (!r.forceRenderingOff && r.enabled) { bawahLama = Mathf.Min(bawahLama, r.bounds.min.y); atasLama = Mathf.Max(atasLama, r.bounds.max.y); }
                        if (!r.forceRenderingOff) { r.forceRenderingOff = true; tersembunyi.Add(r); }
                    }
                    preview = UnityEngine.Object.Instantiate(modelPrefab, target.transform, false);
                    float mb = float.MaxValue, ma = float.MinValue;
                    foreach (var r in preview.GetComponentsInChildren<Renderer>(true)) { mb = Mathf.Min(mb, r.bounds.min.y); ma = Mathf.Max(ma, r.bounds.max.y); }
                    float s = AlasKeramat.AI.AnimasiMixamo.SkalaTinggi(atasLama > bawahLama ? atasLama - bawahLama : 1.75f, ma > mb ? ma - mb : 1.75f);
                    preview.transform.localScale = preview.transform.localScale * s / Mathf.Max(0.0001f, target.transform.lossyScale.y);
                    foreach (var t in preview.GetComponentsInChildren<Transform>(true))
                        if (t.name.EndsWith(":Head") || t.name == "Head") { kepala = t; break; }
                }
                if (kepala == null)
                    foreach (var t in target.GetComponentsInChildren<Transform>(true))
                        if (t.name.EndsWith(":Head") || t.name == "Head") { kepala = t; break; }

                Vector3 headPos = kepala != null ? kepala.position : target.transform.position + Vector3.up * 1.48f;
                Vector3 depan = target.transform.forward;
                Vector3 posCam = headPos + depan * 0.85f + Vector3.up * 0.04f;
                bool bawah = WorldTerrainGenerator.DiBawahTanah(headPos.y);
                float jamFoto = bawah ? 0.5f : 10.5f;
                if (atmos != null) { atmos.SetCuaca(WeatherState.Cerah, true); if (bawah) atmos.TerapkanRuangan(); else atmos.Terapkan(jamFoto); }
                URPAtmosphereAndWeather.DalamRuangan = bawah;

                keyGo = new GameObject("V1_KeyLight");
                keyGo.transform.position = headPos + (depan + target.transform.right * 0.85f).normalized * 2.2f + Vector3.up * 0.9f;
                keyGo.transform.LookAt(headPos);
                var kl = keyGo.AddComponent<Light>(); kl.type = LightType.Spot; kl.range = 6f; kl.spotAngle = 60f; kl.intensity = 2.4f; kl.color = new Color(1f, 0.92f, 0.82f);

                rimGo = new GameObject("V1_RimLight");
                rimGo.transform.position = headPos + (-depan * 0.7f - target.transform.right * 0.8f).normalized * 2.0f + Vector3.up * 0.6f;
                rimGo.transform.LookAt(headPos);
                var rl = rimGo.AddComponent<Light>(); rl.type = LightType.Spot; rl.range = 6f; rl.spotAngle = 65f; rl.intensity = 1.6f; rl.color = new Color(0.55f, 0.78f, 1f);

                cam.transform.SetPositionAndRotation(posCam, Quaternion.LookRotation(headPos + Vector3.up * 0.02f - posCam));
                cam.fieldOfView = 38f;
                cam.farClipPlane = bawah ? 60f : 180f;
                cam.clearFlags = bawah ? CameraClearFlags.SolidColor : CameraClearFlags.Skybox;
                cam.backgroundColor = Color.black;
                if (volumeFoto != null) AKPascaProses.TerapkanFoto(volumeFoto.profile, jamFoto, WeatherState.Cerah, bawah);
                if (kabutLokal != null) kabutLokal.Terapkan(cam, jamFoto, WeatherState.Cerah);
                if (serasah != null) serasah.Gambar(cam);
                if (veg != null) veg.Gambar(cam);
                cam.Render();
                SimpanFrameFoto(rt, nama);
                daftar.Add(nama);
                Log(nama + " selesai pada " + jam.Elapsed.TotalSeconds.ToString("0") + " s");
            }
            catch (Exception e) { Debug.LogWarning("[AK][Foto] Close-up " + nama + " gagal: " + e.Message); }
            finally
            {
                if (preview != null) UnityEngine.Object.DestroyImmediate(preview);
                if (keyGo != null) UnityEngine.Object.DestroyImmediate(keyGo);
                if (rimGo != null) UnityEngine.Object.DestroyImmediate(rimGo);
                foreach (var r in tersembunyi) if (r != null) r.forceRenderingOff = false;
            }
        }

        static void PotretHewanV1(Camera cam, RenderTexture rt, URPAtmosphereAndWeather atmos, Volume volumeFoto,
            AtmosferHutan kabutLokal, SerasahHutan serasah, AlasKeramat.Building.VegetationManager veg, List<string> daftar, System.Diagnostics.Stopwatch jam)
        {
            var eco = UnityEngine.Object.FindFirstObjectByType<AlasKeramat.AI.EcosystemDirector>(FindObjectsInactive.Include);
            if (eco == null || jam.Elapsed.TotalSeconds > BatasDetik) return;
            var pilih = new[]
            {
                ResepHewan.JenisHewan.Harimau, ResepHewan.JenisHewan.Rusa, ResepHewan.JenisHewan.BabiHutan,
                ResepHewan.JenisHewan.Buaya, ResepHewan.JenisHewan.UlarKobra, ResepHewan.JenisHewan.AyamHutan
            };
            var semua = eco.GetComponentsInChildren<AlasKeramat.AI.WildAnimalAI>(true);
            var dipindah = new List<AlasKeramat.AI.WildAnimalAI>();
            var posAwal = new List<Vector3>();
            var rotAwal = new List<Quaternion>();
            var aktifAwal = new List<bool>();
            var pratinjau = new List<GameObject>();
            var tersembunyi = new List<Renderer>();
            try
            {
                int slot = 0;
                foreach (var jenis in pilih)
                {
                    AlasKeramat.AI.WildAnimalAI target = null;
                    for (int i = 0; i < semua.Length; i++) if (semua[i].Jenis == jenis) { target = semua[i]; break; }
                    if (target == null) continue;
                    dipindah.Add(target);
                    posAwal.Add(target.transform.position);
                    rotAwal.Add(target.transform.rotation);
                    aktifAwal.Add(target.gameObject.activeSelf);
                    float x = 40f + slot * 2.1f, z = -415f;
                    float y = TinggiDasar(x, z);
                    target.gameObject.SetActive(true);
                    target.transform.SetPositionAndRotation(new Vector3(x, y, z), Quaternion.Euler(0f, 165f, 0f));

                    var mh = target.GetComponent<AlasKeramat.AI.ModelHewanAsli>();
                    if (mh != null)
                    {
                        var so = new SerializedObject(mh);
                        var arrVar = so.FindProperty("varian");
                        var arrMat = so.FindProperty("bahan");
                        var pf = arrVar != null && arrVar.arraySize > 0 ? arrVar.GetArrayElementAtIndex(0).objectReferenceValue as GameObject : null;
                        if (pf != null)
                        {
                            foreach (var r in target.GetComponentsInChildren<Renderer>(true))
                                if (!r.forceRenderingOff) { r.forceRenderingOff = true; tersembunyi.Add(r); }
                            var pv = UnityEngine.Object.Instantiate(pf, target.transform, false);
                            pv.transform.localPosition = Vector3.zero; pv.transform.localRotation = Quaternion.identity;
                            Material m0 = arrMat != null && arrMat.arraySize > 0 ? arrMat.GetArrayElementAtIndex(0).objectReferenceValue as Material : null;
                            if (m0 != null) foreach (var mr in pv.GetComponentsInChildren<Renderer>(true)) mr.sharedMaterial = m0;
                            float panjangTarget = so.FindProperty("panjangTarget").floatValue;
                            var rs = pv.GetComponentsInChildren<Renderer>(true);
                            if (rs.Length > 0 && panjangTarget > 0.05f)
                            {
                                Bounds bp = rs[0].bounds;
                                for (int k = 1; k < rs.Length; k++) bp.Encapsulate(rs[k].bounds);
                                Vector3 lok = target.transform.InverseTransformVector(bp.size);
                                float pj = Mathf.Max(Mathf.Abs(lok.z), Mathf.Abs(lok.x) * 0.5f);
                                if (pj > 1e-4f) pv.transform.localScale *= Mathf.Clamp(panjangTarget / pj, 0.001f, 1000f);
                            }
                            pratinjau.Add(pv);
                        }
                    }
                    slot++;
                }
                if (dipindah.Count == 0) return;
                URPAtmosphereAndWeather.DalamRuangan = false;
                if (atmos != null) { atmos.SetCuaca(WeatherState.Cerah, true); atmos.Terapkan(10.0f); }
                float yCam = TinggiDasar(45.2f, -423.5f);
                Vector3 posCam = new Vector3(45.2f, yCam + 1.85f, -423.5f);
                Vector3 tgtCam = new Vector3(45.2f, TinggiDasar(45.2f, -415f) + 0.65f, -415f);
                cam.transform.SetPositionAndRotation(posCam, Quaternion.LookRotation(tgtCam - posCam));
                cam.fieldOfView = 52f; cam.farClipPlane = 140f; cam.clearFlags = CameraClearFlags.Skybox;
                if (volumeFoto != null) AKPascaProses.TerapkanFoto(volumeFoto.profile, 10.0f, WeatherState.Cerah, false);
                if (kabutLokal != null) kabutLokal.Terapkan(cam, 10.0f, WeatherState.Cerah);
                if (serasah != null) serasah.Gambar(cam);
                if (veg != null) veg.Gambar(cam);
                cam.Render();
                SimpanFrameFoto(rt, "v1_hewan_barisan");
                daftar.Add("v1_hewan_barisan");
                Log("v1_hewan_barisan selesai pada " + jam.Elapsed.TotalSeconds.ToString("0") + " s");
            }
            catch (Exception e) { Debug.LogWarning("[AK][Foto] Barisan hewan gagal: " + e.Message); }
            finally
            {
                foreach (var pv in pratinjau) if (pv != null) UnityEngine.Object.DestroyImmediate(pv);
                foreach (var r in tersembunyi) if (r != null) r.forceRenderingOff = false;
                for (int i = 0; i < dipindah.Count; i++)
                {
                    if (dipindah[i] == null) continue;
                    dipindah[i].transform.SetPositionAndRotation(posAwal[i], rotAwal[i]);
                    dipindah[i].gameObject.SetActive(aktifAwal[i]);
                }
            }
        }
    }
}
