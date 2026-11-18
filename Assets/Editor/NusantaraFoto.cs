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
        const int Lebar = 1280, Tinggi = 720;
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
            new Shot("t10_01_gua_lumut", new Vector3(-623f, -78.2f, -595f), new Vector3(-608f, -78.5f, -602f), 0.5f, 70f),
            new Shot("t10_02_kubah_akar_boss", new Vector3(600f, -76.5f, -597f), new Vector3(600f, -77.5f, -578f), 0.5f, 70f),
            new Shot("t7_02_pocong_kuntilanak_malam", new Vector3(-58f, 1.7f, -312f), new Vector3(-57f, 1.6f, -297f), 21f, 60f),
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
            var daftar = new List<string>();
            var go = new GameObject("AK_KameraFoto");
            try
            {
                var cam = go.AddComponent<Camera>();
                var utama = Camera.main;
                if (utama != null) { cam.CopyFrom(utama); utama.enabled = false; }
                cam.allowHDR = true;
                var dataCam = UnityEngine.Rendering.Universal.CameraExtensions.GetUniversalAdditionalCameraData(cam);
                if (dataCam != null) dataCam.renderPostProcessing = true;
                var rt = new RenderTexture(Lebar, Tinggi, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
                var atmos = UnityEngine.Object.FindFirstObjectByType<URPAtmosphereAndWeather>();
                var veg = UnityEngine.Object.FindFirstObjectByType<AlasKeramat.Building.VegetationManager>();
                if (veg != null) veg.PengaliJarak = 0.5f; // ringankan render perangkat lunak
                // Ringankan terrain untuk render perangkat lunak (tidak disimpan ke scene).
                foreach (var t in UnityEngine.Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None))
                {
                    t.heightmapPixelError = 12f; t.basemapDistance = 400f; t.drawTreesAndFoliage = false;
                }
                var jam = System.Diagnostics.Stopwatch.StartNew();
                Directory.CreateDirectory("Logs/ci");
                Log("Mulai foto, perangkat: " + SystemInfo.graphicsDeviceName);
                cam.targetTexture = rt;
                cam.farClipPlane = 2600f;
                for (int pemanasan = 0; pemanasan < 2; pemanasan++) cam.Render(); // frame pertama sering gelap (tekstur/cahaya belum siap)
                Log("Pemanasan selesai " + jam.Elapsed.TotalSeconds.ToString("0") + " s");
                var akar = GameObject.Find(NusantaraMasterBuilder.NamaRoot);
                var galeriFoto = akar != null ? akar.transform.Find(NusantaraMasterBuilder.NamaGaleri) : null;
                if (galeriFoto != null) galeriFoto.gameObject.SetActive(true);
                var dioramaFoto = akar != null ? akar.transform.Find(NusantaraMasterBuilder.NamaDiorama) : null;
                if (dioramaFoto != null) dioramaFoto.gameObject.SetActive(true);
                var dioHantu = akar != null ? akar.transform.Find(NusantaraMasterBuilder.NamaDioramaHantu) : null;
                if (dioHantu != null) dioHantu.gameObject.SetActive(true);
                foreach (var s in ShotAktif)
                {
                    if (jam.Elapsed.TotalSeconds > BatasDetik) { Log("Anggaran waktu habis, sisa shot dilewati"); break; }
                    try
                    {
                        if (atmos != null) { atmos.SetCuaca((WeatherState)s.cuaca, true); atmos.Terapkan(s.jam); }
                        bool bawah = WorldTerrainGenerator.DiBawahTanah(s.posisi.y); // T10: shot gua memakai Y absolut
                        var pos = new Vector3(s.posisi.x, (bawah ? 0f : TinggiDasar(s.posisi.x, s.posisi.z)) + s.posisi.y, s.posisi.z);
                        var tgt = new Vector3(s.target.x, (bawah ? 0f : TinggiDasar(s.target.x, s.target.z)) + s.target.y, s.target.z);
                        cam.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(tgt - pos));
                        cam.fieldOfView = s.fov;
                        if (veg != null) veg.Gambar(cam);
                        cam.Render();
                        if (veg != null) veg.Gambar(cam);
                        cam.Render();
                        RenderTexture.active = rt;
                        var tex = new Texture2D(Lebar, Tinggi, TextureFormat.RGB24, false);
                        tex.ReadPixels(new Rect(0, 0, Lebar, Tinggi), 0, 0);
                        tex.Apply();
                        File.WriteAllBytes(Path.Combine(Folder, s.nama + ".png"), tex.EncodeToPNG());
                        UnityEngine.Object.DestroyImmediate(tex);
                        daftar.Add(s.nama);
                        Log(s.nama + " selesai pada " + jam.Elapsed.TotalSeconds.ToString("0") + " s");
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("[AK][Foto] Shot " + s.nama + " gagal: " + e.Message);
                    }
                }
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
    }
}
