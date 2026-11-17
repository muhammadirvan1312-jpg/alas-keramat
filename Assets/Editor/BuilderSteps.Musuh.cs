using System.IO;
using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.Player;
using AlasKeramat.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using J = AlasKeramat.Core.ResepKarakter.JenisKarakter;

namespace AlasKeramat.EditorTools
{
    // Langkah 7n (T7a): panggang SurfaceGrid (500x500, sel 4 m) + GridPathfinder, prefab 5 archetype Sekte (model + CharacterController + AI + prompt),
    // EnemyDirector, reaksi & suara langkah Andi, tombol TANGKIS, tanda kena & vignette merah, diorama Sekte untuk foto.
    public static partial class NusantaraMasterBuilder
    {
        public const string FolderNav = FolderGen + "/Nav", PathGrid = FolderNav + "/SurfaceGrid.bytes", FolderPrefabMusuh = FolderGen + "/Prefabs/Musuh";
        public const string NamaDiorama = "DioramaSekte";
        public static readonly EnemyArchetype[] ArchetypeSekte = { EnemyArchetype.AbdiSekte, EnemyArchetype.AbdiWanita, EnemyArchetype.AbdiLumpur, EnemyArchetype.AbdiButo, EnemyArchetype.PemimpinPatroli };
        const float RadiusKapsulMusuh = 0.35f, RadiusPrompt = 0.9f;

        static void LangkahMusuh()
        {
            EnsureFolder(FolderNav);
            EnsureFolder(FolderGen + "/Prefabs");
            EnsureFolder(FolderPrefabMusuh);

            var grid = SurfaceGrid.Panggang();
            File.WriteAllBytes(PathGrid, grid.KeBytes());
            AssetDatabase.ImportAsset(PathGrid, ImportAssetOptions.ForceUpdate);
            var teks = AssetDatabase.LoadAssetAtPath<TextAsset>(PathGrid);
            int darat, lewat; grid.Hitung(out darat, out lewat);
            var pf = Anak(_sistem, "GridPathfinder").AddComponent<GridPathfinder>();
            pf.Pasang(teks);
            EditorUtility.SetDirty(pf);

            var prefab = new GameObject[ArchetypeSekte.Length];
            for (int i = 0; i < ArchetypeSekte.Length; i++) prefab[i] = PrefabMusuh(ArchetypeSekte[i]);
            var dir = Anak(_sistem, "EnemyDirector").AddComponent<EnemyDirector>();
            dir.Pasang(prefab);
            EditorUtility.SetDirty(dir);

            var andi = Object.FindFirstObjectByType<AndiFPSController>();
            if (andi != null && andi.GetComponent<ReaksiAndi>() == null) andi.gameObject.AddComponent<ReaksiAndi>();

            HUDTempur();
            Diorama(prefab);
            Catat("Musuh: SurfaceGrid " + SurfaceGrid.Sisi + "x" + SurfaceGrid.Sisi + " (lewat " + lewat + "/" + darat + " darat), prefab Sekte x" + prefab.Length + ", EnemyDirector, TANGKIS");
        }

        static GameObject PrefabMusuh(EnemyArchetype a)
        {
            var j = (J)System.Enum.Parse(typeof(J), a.ToString());
            var d = ResepKarakter.Ambil(j);
            var go = BuatKarakter(j, "Musuh_" + a, ProceduralAnimator.Mode.Normal, false);
            go.tag = GameTags.Enemy;
            SetLayer(go, GameLayers.Enemy);
            var cc = go.AddComponent<CharacterController>();
            cc.height = d.tinggi; cc.radius = RadiusKapsulMusuh * Mathf.Max(1f, d.lebar); cc.center = new Vector3(0f, d.tinggi * 0.5f, 0f);
            cc.slopeLimit = SurfaceGrid.LerengMaks + 5f; cc.stepOffset = 0.4f; cc.skinWidth = 0.04f;
            var prompt = new GameObject("Prompt");
            prompt.transform.SetParent(go.transform, false);
            prompt.layer = GameLayers.Interactable;
            var sc = prompt.AddComponent<SphereCollider>();
            sc.isTrigger = true; sc.radius = RadiusPrompt; sc.center = new Vector3(0f, 0.7f, 0f);
            var ai = go.AddComponent<NusantaraEnemyAndMutantAI>();
            ai.Pasang(a, prompt);
            string p = FolderPrefabMusuh + "/Musuh_" + a + ".prefab";
            AssetDatabase.DeleteAsset(p);
            var hasil = PrefabUtility.SaveAsPrefabAsset(go, p);
            Object.DestroyImmediate(go);
            if (hasil == null) Debug.LogError("[AK][Builder] Gagal menyimpan prefab " + p);
            return hasil;
        }

        static void HUDTempur()
        {
            var kanvas = _ui != null ? _ui.transform.Find("Canvas_HUD") : null;
            var kontrol = kanvas != null ? kanvas.Find("Kontrol") : null;
            if (kanvas == null || kontrol == null) { Debug.LogError("[AK][Builder] Canvas_HUD/Kontrol tidak ada, tombol TANGKIS dilewati"); return; }
            var lingkaran = AssetDatabase.LoadAssetAtPath<Sprite>(FolderIkon + "/ui_lingkaran.png");
            var tangkis = UIGambar(kontrol.gameObject, "TombolTangkis", lingkaran, new Vector2(150f, 150f), new Vector2(1f, 0f), new Vector2(-330f, 85f));
            var img = tangkis.GetComponent<Image>();
            img.color = ProcTex.Hex("#3E5C76C0"); img.raycastTarget = true;
            tangkis.AddComponent<TombolTangkis>();
            var t = BuatTeks(tangkis, "Label", "TANGKIS", ProceduralAssetFactory.GetSafeFont(), 26, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150f, 40f));
            t.color = ProcTex.Hex("#F2EFE6"); t.raycastTarget = false;

            var vignette = UIGambar(kanvas.gameObject, "VignetteLuka", SpriteVignette(), Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero);
            var rv = vignette.GetComponent<RectTransform>();
            rv.anchorMin = Vector2.zero; rv.anchorMax = Vector2.one; rv.sizeDelta = Vector2.zero;
            vignette.GetComponent<Image>().color = new Color(0.55f, 0.02f, 0.02f, 0f);
            vignette.transform.SetAsFirstSibling();
            var kena = UIGambar(kanvas.gameObject, "TandaKena", AssetDatabase.LoadAssetAtPath<Sprite>(FolderIkon + "/ui_cincin.png"), new Vector2(34f, 34f), new Vector2(0.5f, 0.5f), Vector2.zero);
            kena.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
            var tanda = kanvas.gameObject.AddComponent<TandaTempur>();
            tanda.Pasang(kena.GetComponent<Image>(), vignette.GetComponent<Image>());
            EditorUtility.SetDirty(tanda);
        }

        // Vignette: alfa 0 di tengah, naik kuadratik ke tepi (dipanggang PNG agar tidak dibuat runtime).
        static Sprite SpriteVignette()
        {
            const int n = 128;
            string p = FolderIkon + "/ui_vignette.png";
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float u = (x + 0.5f) / n * 2f - 1f, v = (y + 0.5f) / n * 2f - 1f;
                    float a = Mathf.Clamp01((Mathf.Sqrt(u * u + v * v) - 0.45f) / 0.75f);
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(a * a * 255f));
                }
            tex.SetPixels32(px);
            File.WriteAllBytes(p, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(p);
            AturImpor(p, false, true, n);
            return AssetDatabase.LoadAssetAtPath<Sprite>(p);
        }

        // Diorama nonaktif: 5 Sekte mengelilingi titik ritual Desa (diaktifkan NusantaraFoto untuk shot t7_01).
        static void Diorama(GameObject[] prefab)
        {
            var dio = Anak(_root, NamaDiorama);
            Vector3 pusat = EnemyDirector.TitikRitualDesa;
            for (int i = 0; i < prefab.Length; i++)
            {
                if (prefab[i] == null) continue;
                var g = (GameObject)PrefabUtility.InstantiatePrefab(prefab[i]);
                g.transform.SetParent(dio.transform, false);
                float s = i * Mathf.PI * 2f / prefab.Length;
                float x = pusat.x + Mathf.Cos(s) * 4.5f, z = pusat.z + Mathf.Sin(s) * 4.5f;
                g.transform.position = new Vector3(x, WorldTerrainGenerator.SampleHeight(x, z), z);
                g.transform.rotation = Quaternion.LookRotation(new Vector3(pusat.x - x, 0f, pusat.z - z));
                var ai = g.GetComponent<NusantaraEnemyAndMutantAI>(); if (ai != null) ai.enabled = false;
                var cc = g.GetComponent<CharacterController>(); if (cc != null) cc.enabled = false;
            }
            dio.SetActive(false);
        }
    }
}
