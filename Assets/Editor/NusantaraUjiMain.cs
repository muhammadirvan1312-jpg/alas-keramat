using System;
using System.IO;
using AlasKeramat.Core;
using AlasKeramat.Building;
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
            { UjiMainOtomatis.Selesai = Selesai; UjiMainOtomatis.AmbilFoto = AmbilFoto; new GameObject("AK_UjiMainCI").AddComponent<UjiMainOtomatis>(); }
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
        static void AmbilFoto(string nama)
        {
            var cam = Camera.main; if (cam == null) throw new InvalidOperationException("Kamera pemain tidak ada");
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            var mode = new RenderMode[canvases.Length]; var kamera = new Camera[canvases.Length]; var jarak = new float[canvases.Length];
            var lama = cam.targetTexture; var aktif = RenderTexture.active;
            var target = new RenderTexture(960, 540, 24, RenderTextureFormat.ARGB32); Texture2D tex = null;
            try
            {
                cam.targetTexture = target;
                for (int i = 0; i < canvases.Length; i++)
                {
                    mode[i] = canvases[i].renderMode; kamera[i] = canvases[i].worldCamera; jarak[i] = canvases[i].planeDistance;
                    if (mode[i] != RenderMode.ScreenSpaceOverlay) continue;
                    canvases[i].renderMode = RenderMode.ScreenSpaceCamera; canvases[i].worldCamera = cam; canvases[i].planeDistance = cam.nearClipPlane + 0.2f;
                }
                Canvas.ForceUpdateCanvases();
                var veg = GameServices.Get<VegetationManager>(); if (veg != null) veg.Gambar(cam);
                var serasah = GameServices.Get<SerasahHutan>(); if (serasah != null) serasah.Gambar(cam);
                cam.Render(); RenderTexture.active = target;
                tex = new Texture2D(960, 540, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 960, 540), 0, 0); tex.Apply();
                Directory.CreateDirectory("Logs/foto_kualitas"); File.WriteAllBytes("Logs/foto_kualitas/" + nama + ".png", tex.EncodeToPNG());
            }
            finally
            {
                for (int i = 0; i < canvases.Length; i++) { canvases[i].renderMode = mode[i]; canvases[i].worldCamera = kamera[i]; canvases[i].planeDistance = jarak[i]; }
                cam.targetTexture = lama; RenderTexture.active = aktif; target.Release(); UnityEngine.Object.Destroy(target); if (tex != null) UnityEngine.Object.Destroy(tex);
            }
        }
    }
}
