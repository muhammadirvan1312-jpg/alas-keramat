using System;
using AlasKeramat.Core;
using AlasKeramat.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AlasKeramat.EditorTools
{
    // Langkah 3, 6, 7, 9: scene, cahaya & langit, sistem, UI tur pratinjau, simpan.
    public static partial class NusantaraMasterBuilder
    {
        static readonly Vector2 ResolusiRef = new Vector2(1920f, 1080f);
        public const string PathSkybox = FolderGen + "/Materials/M_Langit.mat";

        static Scene _scene;
        static GameObject _root, _sistem, _ui, _kamera;
        static Light _sun, _moon, _kilat;
        static Material _matHujan;
        static Material _langit;

        static GameObject Anak(GameObject induk, string nama)
        {
            var go = new GameObject(nama);
            if (induk != null) go.transform.SetParent(induk.transform, false);
            return go;
        }

        static void LangkahScene()
        {
            _scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            _root = Anak(null, NamaRoot);
            _root.tag = GameTags.GeneratedRoot;
            _sistem = Anak(_root, "_Systems");
            var cahaya = Anak(_root, "_Lighting");
            _ui = Anak(_root, "UI");

            _sun = Anak(cahaya, "Sun").AddComponent<Light>();
            _sun.type = LightType.Directional; _sun.shadows = LightShadows.Soft; _sun.shadowStrength = 0.9f;
            _moon = Anak(cahaya, "Moon").AddComponent<Light>();
            _moon.type = LightType.Directional; _moon.shadows = LightShadows.Soft; _moon.shadowStrength = 0.6f;
            _kilat = Anak(cahaya, "Kilat").AddComponent<Light>();
            _kilat.type = LightType.Directional; _kilat.shadows = LightShadows.None; _kilat.color = ProcTex.Hex("#DDE6FF"); _kilat.enabled = false;
            var pShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (pShader != null)
            {
                var mh = new Material(pShader) { name = "M_Hujan" };
                SafeMaterialFactory.SetF(mh, "_Surface", 1f);
                SafeMaterialFactory.SetF(mh, "_Blend", 0f);
                SafeMaterialFactory.SetF(mh, "_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                SafeMaterialFactory.SetF(mh, "_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                SafeMaterialFactory.SetF(mh, "_ZWrite", 0f);
                mh.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); mh.renderQueue = 3000;
                SafeMaterialFactory.SetC(mh, "_BaseColor", new Color(0.8f, 0.85f, 0.9f, 0.5f));
                AssetDatabase.CreateAsset(mh, FolderMat + "/M_Hujan.mat");
                _matHujan = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_Hujan.mat");
            }

            var sky = Shader.Find("Skybox/Procedural");
            if (sky == null) throw new Exception("shader Skybox/Procedural tidak ditemukan");
            _langit = new Material(sky) { name = "M_Langit" };
            AssetDatabase.CreateAsset(_langit, PathSkybox);
            _langit = AssetDatabase.LoadAssetAtPath<Material>(PathSkybox);
            RenderSettings.skybox = _langit;

            _kamera = Anak(_root, "KameraTur");
            _kamera.tag = "MainCamera";
            var c = _kamera.AddComponent<Camera>();
            c.clearFlags = CameraClearFlags.Skybox;
            c.nearClipPlane = 0.3f; c.farClipPlane = 2400f;
            _kamera.AddComponent<AudioListener>();
            _kamera.transform.position = new Vector3(0f, 60f, -620f);
            _kamera.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            Catat("Scene, matahari, bulan, langit prosedural");
        }

        static void LangkahSistem()
        {
            _sistem.AddComponent<GameBootstrap>();
            _sistem.AddComponent<NusantaraDiagnostics>();
            _sistem.AddComponent<TimeOfDayClock>();
            var atmos = _sistem.AddComponent<URPAtmosphereAndWeather>();
            atmos.Pasang(_sun, _moon, _langit);
            atmos.PasangCuaca(_matHujan, _kilat);
            atmos.Terapkan(TimeOfDayClock.JamMulai);
            EditorUtility.SetDirty(atmos);
            var angin = _sistem.AddComponent<AKAngin>();
            angin.Pasang(atmos, WeatherState.Cerah);
            EditorUtility.SetDirty(angin);
            var audioGo = Anak(_sistem, "ProceduralAudioSynth");
            var synth = audioGo.AddComponent<ProceduralAudioSynth>();
            var daftarKlip = new System.Collections.Generic.List<string>(ProceduralAudioSynth.KlipWajib);
            daftarKlip.AddRange(ProceduralAudioSynth.KlipFoley);
            var nama = daftarKlip.ToArray();
            var klip = new AudioClip[nama.Length];
            for (int i = 0; i < nama.Length; i++)
            {
                AudioClip a;
                klip[i] = _klip.TryGetValue(nama[i], out a) ? a : AssetDatabase.LoadAssetAtPath<AudioClip>(FolderAudio + "/" + nama[i] + ".wav");
            }
            synth.AssignBakedClips(nama, klip);
            EditorUtility.SetDirty(synth);
            Catat("Sistem: Bootstrap, Diagnostik, Jam, Atmosfer, Synth (" + klip.Length + " klip)");
        }

        static void LangkahUI()
        {
            var font = ProceduralAssetFactory.GetSafeFont();
            var kanvas = Anak(_ui, "Canvas_HUD");
            kanvas.layer = GameLayers.UI;
            var cv = kanvas.AddComponent<Canvas>();
            cv.renderMode = RenderMode.ScreenSpaceOverlay;
            var sc = kanvas.AddComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = ResolusiRef;
            sc.matchWidthOrHeight = 1f;
            kanvas.AddComponent<GraphicRaycaster>();

            var judul = BuatTeks(kanvas, "Judul", JudulT1, font, 40, new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(1600f, 70f));
            judul.color = ProcTex.Hex("#EDE3C8");
            var zona = BuatTeks(kanvas, "LabelZona", "", font, 56, new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(1700f, 90f));
            zona.color = ProcTex.Hex("#F2EAD3");
            var jam = BuatTeks(kanvas, "LabelJam", "", font, 34, new Vector2(0.5f, 0f), new Vector2(0f, 80f), new Vector2(1200f, 60f));
            jam.color = ProcTex.Hex("#C9B98F");
            var tur = _kamera.AddComponent<TurPratinjau>();
            tur.Pasang(zona, jam);
            EditorUtility.SetDirty(tur);

            var es = Anak(_ui, "EventSystem");
            es.AddComponent<EventSystem>();
            GameBootstrap.AddCorrectInputModule(es);
            Catat("UI: judul, label zona & jam tur pratinjau, EventSystem");
        }

        static Text BuatTeks(GameObject induk, string nama, string isi, Font font, int ukuran, Vector2 jangkar, Vector2 posisi, Vector2 besar)
        {
            var go = Anak(induk, nama);
            go.layer = GameLayers.UI;
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = jangkar;
            rt.anchoredPosition = posisi;
            rt.sizeDelta = besar;
            var t = go.AddComponent<Text>();
            t.raycastTarget = false; // Label, subtitle dan teks sinema tidak boleh menelan sentuhan dunia.
            t.text = isi; t.font = font; t.fontSize = ukuran;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            var ol = go.AddComponent<Outline>();
            ol.effectColor = new Color(0f, 0f, 0f, 0.85f);
            ol.effectDistance = new Vector2(2f, -2f);
            return t;
        }

        static void LangkahSimpan()
        {
            EnsureFolder("Assets/Scenes");
            EditorSceneManager.MarkSceneDirty(_scene);
            if (!EditorSceneManager.SaveScene(_scene, PathScene)) throw new Exception("scene gagal disimpan");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(PathScene, true) };
            AssetDatabase.SaveAssets();
            Catat("Scene: " + PathScene + ", objek di root: " + HitungObjek());
        }

        public static int HitungObjek()
        {
            var root = GameObject.Find(NamaRoot);
            return root == null ? 0 : root.GetComponentsInChildren<Transform>(true).Length;
        }
    }
}
