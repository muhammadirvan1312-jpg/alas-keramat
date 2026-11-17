using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace AlasKeramat.EditorTools
{
    // Titik masuk CI (GameCI buildMethod). Lihat docs/tech/18 bagian 4.
    public static class NusantaraCI
    {
        static string _folder = "Logs/ci";
        static readonly List<string> _baris = new List<string>();
        static bool _adaGagal;
        static StreamWriter _log;

        public static void BuildAndTest()
        {
            string buildPath = Arg("-customBuildPath") ?? "Builds/Android/AlasKeramat.apk";
            _folder = Arg("-akLaporan") ?? "Logs/ci";
            Directory.CreateDirectory(_folder);
            _log = new StreamWriter(Path.Combine(_folder, "editor_ringkas.log"), false) { AutoFlush = true };
            Application.logMessageReceived += TangkapLog;

            Langkah("pengaturan pemain", AturPemain);
            Langkah("builder", () => PanggilBilaAda("AlasKeramat.EditorTools.NusantaraMasterBuilder", "BuildAll"));
            Langkah("self-test", () => PanggilBilaAda("AlasKeramat.EditorTools.NusantaraSelfTest", "RunAll"));
            Tulis("[LEWAT] foto: tanpa GPU");
            Langkah("build APK", () => BangunApk(buildPath));

            Application.logMessageReceived -= TangkapLog;
            File.WriteAllLines(Path.Combine(_folder, "ringkasan.md"), _baris);
            _log.Dispose();
            EditorApplication.Exit(_adaGagal ? 1 : 0);
        }

        static void AturPemain()
        {
            PlayerSettings.companyName = "AlasKeramat";
            PlayerSettings.productName = "Alas Keramat";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.alaskeramat.game");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.Mono2x);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        }

        static void BangunApk(string path)
        {
            string[] scene = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scene.Length == 0) throw new Exception("Build Settings tidak berisi scene");
            if (!path.EndsWith(".apk")) path = Path.Combine(path, "AlasKeramat.apk");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            EditorUserBuildSettings.buildAppBundle = false;
            var opsi = new BuildPlayerOptions
            {
                scenes = scene,
                locationPathName = path,
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };
            var hasil = BuildPipeline.BuildPlayer(opsi);
            if (hasil.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new Exception("build " + hasil.summary.result + ", error " + hasil.summary.totalErrors);
        }

        static void PanggilBilaAda(string namaKelas, string metode)
        {
            Type t = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(namaKelas, false)).FirstOrDefault(x => x != null);
            if (t == null) throw new Exception("kelas " + namaKelas + " tidak ditemukan");
            MethodInfo m = t.GetMethod(metode, BindingFlags.Public | BindingFlags.Static);
            if (m == null) throw new Exception("metode " + metode + " tidak ditemukan");
            object r = m.Invoke(null, null);
            if (r is bool ok && !ok) throw new Exception(metode + " melaporkan gagal");
            PropertyInfo status = t.GetProperty("LastBuildSucceeded", BindingFlags.Public | BindingFlags.Static);
            if (status != null && !(bool)status.GetValue(null)) throw new Exception(metode + " melaporkan langkah GAGAL (lihat Logs/nusantara_build_report.txt)");
        }

        static void Langkah(string nama, Action aksi)
        {
            try { aksi(); Tulis("[LULUS] " + nama); }
            catch (Exception e)
            {
                Exception inti = e is TargetInvocationException && e.InnerException != null ? e.InnerException : e;
                _adaGagal = true;
                Tulis("[GAGAL] " + nama + ": " + inti.Message);
                Debug.LogError("[AK][GAGAL] " + nama + ": " + inti);
            }
        }

        static void Tulis(string s) { _baris.Add(s); Debug.Log("[AK] " + s); }

        static void TangkapLog(string pesan, string jejak, LogType tipe)
        {
            if (tipe == LogType.Log || _log == null) return;
            _log.WriteLine("[" + tipe + "] " + pesan);
            if (tipe != LogType.Warning) _log.WriteLine(jejak);
        }

        static string Arg(string nama)
        {
            string[] a = Environment.GetCommandLineArgs();
            for (int i = 0; i < a.Length - 1; i++) if (a[i] == nama) return a[i + 1];
            return null;
        }
    }
}
