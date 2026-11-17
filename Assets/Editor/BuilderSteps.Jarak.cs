using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.Player;
using AlasKeramat.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.EditorTools
{
    // Langkah 7q (T7d): prefab proyektil (panah, tombak, batu, suar, bom), model FP senjata jarak, tombol GANTI (nama + amunisi),
    // cincin meter suara di crosshair, titik lumpur rawa untuk kamuflase.
    public static partial class NusantaraMasterBuilder
    {
        public static readonly Vector3[] PosLumpur = { new Vector3(-640f, 0f, -540f), new Vector3(-700f, 0f, -600f), new Vector3(-470f, 0f, 320f) };

        static void LangkahJarak()
        {
            var kayu = AmbilMat("M_Kayu"); var logam = AmbilMat("M_LogamGelap"); var batu = AmbilMat("M_Batu"); var cat = AmbilMat("M_CatOranye");
            var p = new GameObject[5];
            p[0] = Proyektil("Proyektil_Panah", PrimitiveType.Cylinder, new Vector3(0.012f, 0.35f, 0.012f), kayu, false);
            p[1] = Proyektil("Proyektil_Tombak", PrimitiveType.Cylinder, new Vector3(0.025f, 0.9f, 0.025f), kayu, false);
            p[2] = Proyektil("Proyektil_Batu", PrimitiveType.Sphere, Vector3.one * 0.06f, batu, false);
            p[3] = Proyektil("Proyektil_Suar", PrimitiveType.Sphere, Vector3.one * 0.07f, AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_Banaspati.mat"), true);
            p[4] = Proyektil("Proyektil_Bom", PrimitiveType.Cylinder, new Vector3(0.06f, 0.15f, 0.06f), kayu, false);
            var andi = Object.FindFirstObjectByType<AndiFPSController>();
            var combat = andi != null ? andi.GetComponent<CombatAndLeftHandController>() : null;
            if (combat == null) { Debug.LogError("[AK][Builder] CombatAndLeftHandController tidak ada, senjata jarak dilewati"); return; }
            combat.PasangProyektil(p);
            var pivot = andi.Kamera != null ? andi.Kamera.Find("SenjataFP") : null;
            var model = new Transform[CombatAndLeftHandController.TabelJarak.Length];
            if (pivot != null)
                for (int i = 0; i < model.Length; i++)
                {
                    var s = CombatAndLeftHandController.TabelJarak[i];
                    var g = Anak(pivot.gameObject, "FPJ_" + s.id).transform;
                    switch (s.jenis)
                    {
                        case CombatAndLeftHandController.JenisJarak.Busur:
                            BagianFP(g, PrimitiveType.Cylinder, new Vector3(0f, 0.3f, 0f), new Vector3(0.02f, 0.32f, 0.02f), kayu);
                            BagianFP(g, PrimitiveType.Cube, new Vector3(0f, 0.3f, -0.08f), new Vector3(0.004f, 0.6f, 0.004f), logam); break;
                        case CombatAndLeftHandController.JenisJarak.Tombak:
                            BagianFP(g, PrimitiveType.Cylinder, new Vector3(0f, 0.35f, 0f), new Vector3(0.025f, 0.6f, 0.025f), kayu);
                            BagianFP(g, PrimitiveType.Cube, new Vector3(0f, 0.98f, 0f), new Vector3(0.03f, 0.14f, 0.012f), logam); break;
                        case CombatAndLeftHandController.JenisJarak.Hitscan:
                        case CombatAndLeftHandController.JenisJarak.Suar:
                            BagianFP(g, PrimitiveType.Cube, new Vector3(0f, 0.05f, 0.18f), new Vector3(0.045f, 0.06f, s.id == ItemID.BedilLocok || s.id == ItemID.SenapanLaras ? 0.7f : 0.22f), s.id == ItemID.PistolSuar ? cat : logam);
                            BagianFP(g, PrimitiveType.Cube, new Vector3(0f, -0.04f, 0.05f), new Vector3(0.035f, 0.12f, 0.05f), kayu); break;
                        case CombatAndLeftHandController.JenisJarak.Ketapel:
                            BagianFP(g, PrimitiveType.Cylinder, new Vector3(0f, 0.08f, 0f), new Vector3(0.02f, 0.1f, 0.02f), kayu);
                            BagianFP(g, PrimitiveType.Cube, new Vector3(0f, 0.2f, 0f), new Vector3(0.12f, 0.02f, 0.02f), kayu); break;
                        default:
                            BagianFP(g, PrimitiveType.Cylinder, new Vector3(0f, 0.1f, 0f), new Vector3(0.06f, 0.12f, 0.06f), kayu); break;
                    }
                    g.localRotation = Quaternion.Euler(s.jenis == CombatAndLeftHandController.JenisJarak.Hitscan || s.jenis == CombatAndLeftHandController.JenisJarak.Suar ? 0f : -35f, 0f, 0f);
                    g.gameObject.SetActive(false);
                    model[i] = g;
                }
            combat.PasangModelJarak(model);
            EditorUtility.SetDirty(combat);

            var kanvas = _ui != null ? _ui.transform.Find("Canvas_HUD") : null;
            var kontrol = kanvas != null ? kanvas.Find("Kontrol") : null;
            var tc = kontrol != null ? kontrol.GetComponentInParent<TouchControls>() : null;
            if (tc == null && kanvas != null) tc = kanvas.GetComponentInChildren<TouchControls>(true);
            if (kontrol != null && tc != null)
            {
                var font = ProceduralAssetFactory.GetSafeFont();
                var ling = AssetDatabase.LoadAssetAtPath<Sprite>(FolderIkon + "/ui_lingkaran.png");
                var b = Tombol(kontrol.gameObject, "TombolGanti", "GANTI", ling, font, 110f, new Vector2(-90f, 560f), ProcTex.Hex("#5B5A4E"));
                b.GetComponentInChildren<Text>().fontSize = 18;
                UnityEventTools.AddPersistentListener(b.onClick, tc.TekanGanti);
            }
            if (kanvas != null)
            {
                var cincin = UIGambar(kanvas.gameObject, "CincinSuara", AssetDatabase.LoadAssetAtPath<Sprite>(FolderIkon + "/ui_cincin.png"), new Vector2(40f, 40f), new Vector2(0.5f, 0.5f), Vector2.zero);
                cincin.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.22f);
                var tanda = kanvas.GetComponent<TandaTempur>();
                if (tanda != null) { tanda.PasangCincinSuara(cincin.GetComponent<RectTransform>()); EditorUtility.SetDirty(tanda); }
            }

            var induk = Anak(_root, "TitikLumpur");
            for (int i = 0; i < PosLumpur.Length; i++)
            {
                var g = Anak(induk, "Lumpur_" + i);
                float h = WorldTerrainGenerator.SampleHeight(PosLumpur[i].x, PosLumpur[i].z);
                g.transform.position = new Vector3(PosLumpur[i].x, h, PosLumpur[i].z);
                BagianPrimitif(g.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.02f, 0f), new Vector3(2.4f, 0.02f, 2.4f), AmbilMat("M_Batu"), false);
                var sc = g.AddComponent<SphereCollider>(); sc.isTrigger = true; sc.radius = 1.6f;
                g.layer = GameLayers.Interactable;
                g.AddComponent<TitikLumpur>();
            }
            Catat("Senjata jarak: 5 proyektil, " + model.Length + " model FP, tombol GANTI, cincin suara, 3 titik lumpur");
        }

        static GameObject Proyektil(string nama, PrimitiveType tipe, Vector3 skala, Material mat, bool cahaya)
        {
            var go = new GameObject(nama);
            var bagian = GameObject.CreatePrimitive(tipe);
            Object.DestroyImmediate(bagian.GetComponent<Collider>());
            bagian.transform.SetParent(go.transform, false);
            bagian.transform.localScale = skala;
            if (tipe == PrimitiveType.Cylinder) bagian.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // sumbu panjang searah terbang (z)
            if (mat != null) bagian.GetComponent<MeshRenderer>().sharedMaterial = mat;
            if (cahaya)
            {
                var l = new GameObject("Cahaya").AddComponent<Light>();
                l.transform.SetParent(go.transform, false);
                l.type = LightType.Point; l.range = 14f; l.intensity = 3f; l.color = new Color(1f, 0.35f, 0.25f); l.shadows = LightShadows.None;
            }
            go.layer = GameLayers.Projectile;
            go.AddComponent<ProyektilAK>();
            return SimpanPrefab(go, nama);
        }
    }
}
