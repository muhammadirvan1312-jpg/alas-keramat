using System;
using System.IO;
using AlasKeramat.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AlasKeramat.EditorTools
{
    [InitializeOnLoad]
    public static class NusantaraUjiMain
    {
        const string Kunci = "AK_UJI_MAIN_CI";
        static bool Aktif { get { return SessionState.GetBool(Kunci, false); } }
        static NusantaraUjiMain()
        {
            EditorApplication.playModeStateChanged += SaatMode;
            EditorApplication.update += Awasi;
            EditorApplication.wantsToQuit += BolehKeluar;
            Application.logMessageReceived += SaatLog;
        }
        static bool BolehKeluar() { return !Aktif; }
        public static void Mulai()
        {
            EditorSceneManager.OpenScene(NusantaraMasterBuilder.PathScene);
            SessionState.SetBool(Kunci, true); SessionState.SetBool(Kunci + "selesai", false);
            SessionState.SetBool(Kunci + "error", false); SessionState.SetInt(Kunci + "kode", 1);
            SessionState.SetFloat(Kunci + "batas", (float)EditorApplication.timeSinceStartup + 180f);
            SessionState.SetString(Kunci + "sha", NusantaraCI.SourceCommit());
            SessionState.SetBool(Kunci + "punyaPref", PlayerPrefs.HasKey(GoreSystem.KunciPeringatan));
            SessionState.SetInt(Kunci + "pref", PlayerPrefs.GetInt(GoreSystem.KunciPeringatan, 0));
            PlayerPrefs.SetInt(GoreSystem.KunciPeringatan, 1);
            Directory.CreateDirectory("Logs/ci"); File.WriteAllText("Logs/ci/uji_main.log", "PlayMode CI; bukan benchmark HP\n");
            EditorApplication.EnterPlaymode();
        }
        static void SaatMode(PlayModeStateChange state)
        {
            if (!Aktif) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            { UjiMainOtomatis.Selesai = Selesai; new GameObject("AK_UjiMainCI").AddComponent<UjiMainOtomatis>(); }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (!SessionState.GetBool(Kunci + "selesai", false)) Selesai(false, new[] { "[GAGAL] Play berhenti sebelum urutan selesai" });
                if (SessionState.GetBool(Kunci + "punyaPref", false)) PlayerPrefs.SetInt(GoreSystem.KunciPeringatan, SessionState.GetInt(Kunci + "pref", 0));
                else PlayerPrefs.DeleteKey(GoreSystem.KunciPeringatan);
                SessionState.SetBool(Kunci, false); EditorApplication.Exit(SessionState.GetInt(Kunci + "kode", 1));
            }
        }
        static void SaatLog(string pesan, string jejak, LogType tipe)
        {
            if (!Aktif || (tipe != LogType.Error && tipe != LogType.Exception && tipe != LogType.Assert)) return;
            SessionState.SetBool(Kunci + "error", true); File.AppendAllText("Logs/ci/uji_main.log", "[ERROR] " + pesan + "\n" + jejak + "\n");
        }
        static void Awasi()
        {
            if (Aktif && !SessionState.GetBool(Kunci + "selesai", false) && EditorApplication.timeSinceStartup > SessionState.GetFloat(Kunci + "batas", float.MaxValue))
                Selesai(false, new[] { "[GAGAL] Batas waktu PlayMode 180 detik" });
        }
        [Serializable] class BuktiUjiMainG3 { public string commit; public bool passed; public string[] checks; public string note; }
        static void Selesai(bool lulus, string[] hasil)
        {
            lulus &= !SessionState.GetBool(Kunci + "error", false);
            File.AppendAllLines("Logs/ci/uji_main.log", hasil);
            File.WriteAllText("Logs/ci/uji_main.json", JsonUtility.ToJson(new BuktiUjiMainG3 { commit = SessionState.GetString(Kunci + "sha", ""), passed = lulus, checks = hasil, note = "Urutan kontrol dan panel di Editor PlayMode; tidak mengesahkan FPS atau kenyamanan HP" }, true));
            SessionState.SetBool(Kunci + "selesai", true); SessionState.SetInt(Kunci + "kode", lulus ? 0 : 1);
            if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
            else if (!EditorApplication.isPlayingOrWillChangePlaymode) { SessionState.SetBool(Kunci, false); EditorApplication.Exit(lulus ? 0 : 1); }
        }
    }
}
