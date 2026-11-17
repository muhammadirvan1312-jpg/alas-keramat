using System;
using AlasKeramat.Core;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AlasKeramat.EditorTools
{
    // Langkah 3, 6, 7, 9: scene uji T1 (tanah 200x200, matahari, kubus PBR, UI 5 tombol suara).
    public static partial class NusantaraMasterBuilder
    {
        static readonly Vector2 ResolusiRef = new Vector2(1920f, 1080f);
        static readonly string[] TombolSuara = { ProceduralAudioSynth.KlipTebang, ProceduralAudioSynth.KlipPohonTumbang, ProceduralAudioSynth.KlipKentongan, ProceduralAudioSynth.KlipKuntilanak, ProceduralAudioSynth.KlipPocong };
        static readonly string[] LabelSuara = { "Tebang", "Pohon Tumbang", "Kentongan", "Kuntilanak", "Pocong" };

        static Scene _scene;
        static GameObject _root, _sistem, _ui;

        static GameObject Anak(GameObject induk, string nama)
        {
            var go = new GameObject(nama);
            if (induk != null) go.transform.SetParent(induk.transform, false);
            return go;
        }

        static void LangkahDuniaT1()
        {
            _scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            _root = Anak(null, NamaRoot);
            _root.tag = GameTags.GeneratedRoot;
            _sistem = Anak(_root, "_Systems");
            var cahaya = Anak(_root, "_Lighting");
            var dunia = Anak(_root, "World");
            _ui = Anak(_root, "UI");

            var sun = Anak(cahaya, "Sun");
            sun.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            var l = sun.AddComponent<Light>();
            l.type = LightType.Directional; l.intensity = 1.2f; l.shadows = LightShadows.Soft;
            l.color = ProcTex.Hex("#FFF1D6");
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = ProcTex.Hex("#8FA3B8");
            RenderSettings.ambientEquatorColor = ProcTex.Hex("#5E6B55");
            RenderSettings.ambientGroundColor = ProcTex.Hex("#2E2A22");

            var tanah = Anak(dunia, "TanahUji");
            tanah.layer = GameLayers.Ground;
            tanah.AddComponent<MeshFilter>().sharedMesh = _meshTanah;
            tanah.AddComponent<MeshRenderer>().sharedMaterial = AmbilMat("M_Tanah");
            tanah.AddComponent<MeshCollider>().sharedMesh = _meshTanah;

            var kubus = GameObject.CreatePrimitive(PrimitiveType.Cube);
            kubus.name = "KubusUji";
            kubus.transform.SetParent(dunia.transform, false);
            kubus.transform.position = new Vector3(0f, 1f, 6f);
            kubus.transform.rotation = Quaternion.Euler(0f, 30f, 0f);
            kubus.transform.localScale = Vector3.one * 2f;
            kubus.GetComponent<MeshRenderer>().sharedMaterial = AmbilMat("M_Kayu");

            string[] matLain = { "M_Batu", "M_Beton", "M_LogamKarat" };
            for (int i = 0; i < matLain.Length; i++)
            {
                var bola = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bola.name = "BolaUji_" + matLain[i];
                bola.transform.SetParent(dunia.transform, false);
                bola.transform.position = new Vector3(-4f + i * 4f, 0.75f, 11f);
                bola.transform.localScale = Vector3.one * 1.5f;
                bola.GetComponent<MeshRenderer>().sharedMaterial = AmbilMat(matLain[i]);
            }

            var kam = Anak(dunia, "KameraUji");
            kam.tag = "MainCamera";
            kam.transform.position = new Vector3(0f, 3.2f, -4f);
            kam.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            var c = kam.AddComponent<Camera>();
            c.clearFlags = CameraClearFlags.SolidColor;
            c.backgroundColor = ProcTex.Hex("#6F8296");
            c.farClipPlane = 400f;
            kam.AddComponent<AudioListener>();
            Catat("Dunia uji: tanah 200x200, kubus & 3 bola PBR, matahari");
        }

        static Material AmbilMat(string nama)
        {
            Material m;
            if (_mat.TryGetValue(nama, out m) && m != null) return m;
            m = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/" + nama + ".mat");
            if (m == null) throw new Exception("material tidak ada: " + nama);
            return m;
        }

        static void LangkahSistem()
        {
            _sistem.AddComponent<GameBootstrap>();
            _sistem.AddComponent<NusantaraDiagnostics>();
            var audioGo = Anak(_sistem, "ProceduralAudioSynth");
            var synth = audioGo.AddComponent<ProceduralAudioSynth>();
            var nama = ProceduralAudioSynth.KlipWajib;
            var klip = new AudioClip[nama.Length];
            for (int i = 0; i < nama.Length; i++)
            {
                AudioClip c;
                klip[i] = _klip.TryGetValue(nama[i], out c) ? c : AssetDatabase.LoadAssetAtPath<AudioClip>(FolderAudio + "/" + nama[i] + ".wav");
            }
            synth.AssignBakedClips(nama, klip);
            EditorUtility.SetDirty(synth);
            Catat("Sistem: GameBootstrap, NusantaraDiagnostics, ProceduralAudioSynth (" + klip.Length + " klip)");
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

            var judul = BuatTeks(kanvas, "Judul", JudulT1, font, 64, new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(1600f, 120f));
            judul.color = ProcTex.Hex("#EDE3C8");

            var synth = _sistem.GetComponentInChildren<ProceduralAudioSynth>();
            const float Lebar = 300f, Tinggi = 120f, Jarak = 30f;
            float total = TombolSuara.Length * Lebar + (TombolSuara.Length - 1) * Jarak;
            for (int i = 0; i < TombolSuara.Length; i++)
            {
                var b = Anak(kanvas, "Tombol_" + TombolSuara[i]);
                b.layer = GameLayers.UI;
                var rt = b.AddComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
                rt.sizeDelta = new Vector2(Lebar, Tinggi);
                rt.anchoredPosition = new Vector2(-total / 2f + Lebar / 2f + i * (Lebar + Jarak), 140f);
                var img = b.AddComponent<Image>();
                img.color = ProcTex.Hex("#2B2A26E6");
                var btn = b.AddComponent<Button>();
                var warna = btn.colors;
                warna.highlightedColor = ProcTex.Hex("#4A463C"); warna.pressedColor = ProcTex.Hex("#7A6A48");
                btn.colors = warna;
                if (synth != null) UnityEventTools.AddStringPersistentListener(btn.onClick, synth.PlayByName, TombolSuara[i]);
                var t = BuatTeks(b, "Label", LabelSuara[i], font, 34, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(Lebar, Tinggi));
                t.color = ProcTex.Hex("#F2EAD3");
            }

            var es = Anak(_ui, "EventSystem");
            es.AddComponent<EventSystem>();
            GameBootstrap.AddCorrectInputModule(es);
            Catat("UI: judul + " + TombolSuara.Length + " tombol suara + EventSystem");
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
            t.text = isi; t.font = font; t.fontSize = ukuran;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            var ol = go.AddComponent<Outline>();
            ol.effectColor = new Color(0f, 0f, 0f, 0.8f);
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
