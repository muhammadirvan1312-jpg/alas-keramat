using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.EditorTools
{
    // Self-test editor. Mengembalikan true bila semua cek lulus.
    public static class NusantaraSelfTest
    {
        [MenuItem("Nusantara Builder/Self-Test")]
        public static bool RunAll()
        {
            var gagal = new List<string>();
            NusantaraMasterBuilder.BuildAll();
            NusantaraMasterBuilder.BuildAll(); // idempoten: dua kali harus identik
            EditorSceneManager.OpenScene(NusantaraMasterBuilder.PathScene);

            Cek(gagal, "satu root", UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Count(x => x.name == NusantaraMasterBuilder.NamaRoot && x.parent == null) == 1);
            Cek(gagal, "satu scene di Build Settings", EditorBuildSettings.scenes.Length == 1 && EditorBuildSettings.scenes[0].path == NusantaraMasterBuilder.PathScene);
            Cek(gagal, "kamera utama", Camera.main != null);
            var teks = UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None);
            Cek(gagal, "teks PIPELINE OK", teks.Any(t => t.text.Contains("PIPELINE OK")));
            Cek(gagal, "font aman", teks.All(t => t.font != null));

            foreach (var g in gagal) Debug.LogError("[AK][GAGAL] self-test: " + g);
            System.IO.File.WriteAllLines("Logs/nusantara_selftest.txt", gagal.Count == 0 ? new[] { "SEMUA LULUS" } : gagal.ToArray());
            return gagal.Count == 0;
        }

        static void Cek(List<string> gagal, string nama, bool ok)
        {
            if (!ok) gagal.Add(nama);
        }
    }
}
