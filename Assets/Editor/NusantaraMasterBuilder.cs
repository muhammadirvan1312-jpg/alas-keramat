using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AlasKeramat.EditorTools
{
    // Builder 1-klik (idempoten). T0-C: scene minimal "PIPELINE OK". Diperluas mulai T1.
    public static class NusantaraMasterBuilder
    {
        public const string FolderGen = "Assets/Generated";
        public const string PathScene = "Assets/Scenes/AlasKeramat_Main.unity";
        public const string NamaRoot = "[ALAS_KERAMAT_ROOT]";

        [MenuItem("Nusantara Builder/1. BANGUN SELURUH GAME (1-KLIK)")]
        public static void BuildAll()
        {
            Directory.CreateDirectory(FolderGen);
            Directory.CreateDirectory("Assets/Scenes");
            AssetDatabase.Refresh();

            Scene s = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject(NamaRoot);

            var kam = new GameObject("Kamera", typeof(Camera), typeof(AudioListener));
            kam.transform.SetParent(root.transform);
            kam.tag = "MainCamera";
            var c = kam.GetComponent<Camera>();
            c.clearFlags = CameraClearFlags.SolidColor;
            c.backgroundColor = new Color(0.02f, 0.03f, 0.02f);

            var cahaya = new GameObject("Cahaya", typeof(Light));
            cahaya.transform.SetParent(root.transform);
            cahaya.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            cahaya.GetComponent<Light>().type = LightType.Directional;

            var es = new GameObject("EventSystem", typeof(EventSystem));
            AddCorrectInputModule(es);
            es.transform.SetParent(root.transform);

            var kanvas = new GameObject("Kanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            kanvas.transform.SetParent(root.transform);
            kanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var sc = kanvas.GetComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1920, 1080);

            var teks = new GameObject("Judul", typeof(RectTransform), typeof(Text));
            teks.transform.SetParent(kanvas.transform, false);
            var rt = teks.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var t = teks.GetComponent<Text>();
            t.text = "ALAS KERAMAT - PIPELINE OK";
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 72;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = new Color(0.85f, 0.8f, 0.65f);

            EditorSceneManager.SaveScene(s, PathScene);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(PathScene, true) };
            AssetDatabase.SaveAssets();

            Directory.CreateDirectory("Logs");
            File.WriteAllLines("Logs/nusantara_build_report.txt", new List<string> { "Laporan Builder", "scene: " + PathScene, "root: " + NamaRoot });
        }

        public static void AddCorrectInputModule(GameObject esGo)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var t = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (t != null)
            {
                var c = esGo.AddComponent(t);
                var m = t.GetMethod("AssignDefaultActions");
                if (m != null) m.Invoke(c, null);
                return;
            }
            Debug.LogError("[AK][Input] Proyek memakai Input System (New) tetapi modul UI tidak ditemukan.");
#else
            esGo.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
