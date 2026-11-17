using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Player;
using AlasKeramat.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace AlasKeramat.EditorTools
{
    // Langkah 7h (T5a): pool pohon interaktif, melee golok/kapak + model senjata orang-pertama, tombol TEBAS (ketuk/tahan), tata ulang kluster tombol.
    public static partial class NusantaraMasterBuilder
    {
        static void LangkahTebang()
        {
            Anak(_sistem, "PohonInteraktif").AddComponent<PohonInteraktif>();
            var andi = Object.FindFirstObjectByType<AndiFPSController>();
            if (andi == null || andi.Kamera == null) { Debug.LogError("[AK][Builder] Andi/kamera tidak ada, melee dilewati"); return; }
            var combat = andi.gameObject.AddComponent<CombatAndLeftHandController>();
            var pivot = Anak(andi.Kamera.gameObject, "SenjataFP").transform;
            pivot.localPosition = new Vector3(0.28f, -0.32f, 0.5f);
            var kayu = AmbilMat("M_Kayu"); var logam = AmbilMat("M_LogamGelap"); var batu = AmbilMat("M_Batu");
            var model = new Transform[CombatAndLeftHandController.Tabel.Length];
            for (int i = 0; i < model.Length; i++)
            {
                var id = CombatAndLeftHandController.Tabel[i].id;
                var g = Anak(pivot.gameObject, "FP_" + id).transform;
                BagianFP(g, PrimitiveType.Cylinder, new Vector3(0f, 0.12f, 0f), new Vector3(0.035f, 0.2f, 0.035f), kayu);
                if (id == ItemID.GolokPusaka || id == ItemID.KerisPusaka || id == ItemID.Mandau || id == ItemID.GolokApiPusaka) BagianFP(g, PrimitiveType.Cube, new Vector3(0f, 0.5f, 0.02f), new Vector3(0.012f, 0.45f, 0.07f), logam);
                else BagianFP(g, PrimitiveType.Cube, new Vector3(0f, 0.32f, 0.06f), new Vector3(0.04f, 0.09f, 0.16f), id == ItemID.KapakBatu ? batu : logam);
                g.localRotation = Quaternion.Euler(-35f, 0f, 0f);
                g.gameObject.SetActive(false);
                model[i] = g;
            }
            combat.Pasang(pivot, model);
            EditorUtility.SetDirty(combat);

            var kanvas = _ui != null ? _ui.transform.Find("Canvas_HUD") : null;
            var kontrol = kanvas != null ? kanvas.Find("Kontrol") : null;
            if (kontrol == null) return;
            Geser(kontrol, "TombolLompat", new Vector2(-120f, 410f), 130f);
            Geser(kontrol, "TombolJongkok", new Vector2(-480f, 90f), 120f);
            Geser(kontrol, "TombolObor", new Vector2(-560f, 240f), 110f);
            var lingkaran = AssetDatabase.LoadAssetAtPath<Sprite>(FolderIkon + "/ui_lingkaran.png");
            var tebas = UIGambar(kontrol.gameObject, "TombolTebas", lingkaran, new Vector2(200f, 200f), new Vector2(1f, 0f), new Vector2(-150f, 170f));
            var img = tebas.GetComponent<Image>();
            img.color = ProcTex.Hex("#B5462EC0"); img.raycastTarget = true;
            tebas.AddComponent<TombolTahan>();
            var t = BuatTeks(tebas, "Label", "TEBAS", ProceduralAssetFactory.GetSafeFont(), 30, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200f, 44f));
            t.color = ProcTex.Hex("#F2EFE6"); t.raycastTarget = false;
            Catat("Tebang: pool pohon interaktif, melee golok/kapak, model senjata FP, tombol TEBAS");
        }

        static void BagianFP(Transform induk, PrimitiveType tipe, Vector3 pos, Vector3 skala, Material mat)
        {
            var g = GameObject.CreatePrimitive(tipe);
            Object.DestroyImmediate(g.GetComponent<Collider>());
            g.transform.SetParent(induk, false);
            g.transform.localPosition = pos; g.transform.localScale = skala;
            g.layer = GameLayers.Player;
            var mr = g.GetComponent<MeshRenderer>();
            if (mat != null) mr.sharedMaterial = mat;
            mr.shadowCastingMode = ShadowCastingMode.Off;
        }

        static void Geser(Transform kontrol, string nama, Vector2 pos, float d)
        {
            var t = kontrol.Find(nama) as RectTransform;
            if (t == null) return;
            t.anchoredPosition = pos; t.sizeDelta = new Vector2(d, d);
        }
    }
}
