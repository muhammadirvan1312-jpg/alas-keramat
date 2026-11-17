using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.EditorTools
{
    // Foto Kualitas (docs/gdd/15 §8, 19 §8): merender kamera ke PNG di Logs/foto_kualitas/.
    // Di CI berjalan di bawah xvfb + render perangkat lunak; bila tidak ada perangkat grafis -> dilewati.
    public static class NusantaraFoto
    {
        public const string Folder = "Logs/foto_kualitas";
        const int Lebar = 1280, Tinggi = 720;

        public struct Shot
        {
            public string nama; public Vector3 posisi; public Vector3 euler; public float fov;
            public Shot(string n, Vector3 p, Vector3 e, float f = 60f) { nama = n; posisi = p; euler = e; fov = f; }
        }

        // Daftar shot per tahap. Tahap berikutnya menambah shot ke daftar ini (12 money shot penuh di T2+).
        public static readonly Shot[] ShotT1 =
        {
            new Shot("t1_01_lebar", new Vector3(0f, 3.2f, -4f), new Vector3(12f, 0f, 0f)),
            new Shot("t1_02_kubus_dekat", new Vector3(2.5f, 1.8f, 2.5f), new Vector3(10f, -30f, 0f), 50f),
            new Shot("t1_03_bola_material", new Vector3(0f, 1.6f, 6.5f), new Vector3(8f, 0f, 0f), 55f),
            new Shot("t1_04_tanah_rendah", new Vector3(-6f, 0.4f, -2f), new Vector3(4f, 35f, 0f), 70f),
        };

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
                var rt = new RenderTexture(Lebar, Tinggi, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
                foreach (var s in ShotT1)
                {
                    try
                    {
                        cam.transform.SetPositionAndRotation(s.posisi, Quaternion.Euler(s.euler));
                        cam.fieldOfView = s.fov;
                        cam.targetTexture = rt;
                        cam.Render();
                        RenderTexture.active = rt;
                        var tex = new Texture2D(Lebar, Tinggi, TextureFormat.RGB24, false);
                        tex.ReadPixels(new Rect(0, 0, Lebar, Tinggi), 0, 0);
                        tex.Apply();
                        File.WriteAllBytes(Path.Combine(Folder, s.nama + ".png"), tex.EncodeToPNG());
                        UnityEngine.Object.DestroyImmediate(tex);
                        daftar.Add(s.nama);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("[AK][Foto] Shot " + s.nama + " gagal: " + e.Message);
                    }
                }
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
