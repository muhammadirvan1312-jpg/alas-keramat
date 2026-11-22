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
            var baja = SimpanMat(SafeMaterialFactory.CreatePBR("FP_BajaTajam", ProcTex.Hex("#D4DCE4"), 0.90f, 0.86f, Tex("logam_karat"), Tex("logam_karat_n")));
            var lilit = SimpanMat(SafeMaterialFactory.CreatePBR("FP_KulitLilit", ProcTex.Hex("#4A3020"), 0.05f, 0.32f, Tex("kulit_kayu"), Tex("kulit_kayu_n")));
            var matKulitFP = SafeMaterialFactory.CreatePBR("FP_Kulit", new Color(0.84f, 0.65f, 0.50f), 0f, 0.28f, null, AssetDatabase.LoadAssetAtPath<Texture2D>(FolderKarakter + "/kulit_pori_n.png"));
            var shKulitFP = AmbilShaderAK("AlasKeramat/KulitManusia");
            if (shKulitFP != null) matKulitFP.shader = shKulitFP;
            var kulit = SimpanMat(matKulitFP);
            var lengan = SimpanMat(SafeMaterialFactory.CreatePBR("FP_Lengan", ProcTex.Hex("#4B5B40"), 0f, 0.16f, Tex("kain"), Tex("kain_n")));
            var emas = SimpanMat(SafeMaterialFactory.CreatePBR("FP_Emas", ProcTex.Hex("#E2B94A"), 0.92f, 0.86f, Tex("logam_karat"), Tex("logam_karat_n")));
            var merah = SimpanMat(SafeMaterialFactory.CreatePBR("FP_Jambul", ProcTex.Hex("#B82B24"), 0f, 0.18f, Tex("kain"), Tex("kain_n")));
            var bara = MatEmisi("FP_BaraGolok", "#FF5A1C", new Color(3.2f, 0.92f, 0.18f), false);
            var dialJam = MatEmisi("FP_DialJam", "#1B4428", new Color(0.22f, 0.95f, 0.48f) * 1.6f, false);
            var arm = Anak(pivot.gameObject, "LenganKananFP").transform;
            arm.localPosition = new Vector3(0.05f, -0.14f, -0.18f); arm.localRotation = Quaternion.Euler(52f, -14f, -8f);
            BagianFP(arm, PrimitiveType.Cylinder, new Vector3(0f, -0.14f, 0f), new Vector3(0.088f, 0.14f, 0.088f), lengan);
            BagianFP(arm, PrimitiveType.Cylinder, new Vector3(0f, -0.02f, 0f), new Vector3(0.095f, 0.022f, 0.095f), lilit);
            BagianFP(arm, PrimitiveType.Cylinder, new Vector3(0f, 0.06f, 0f), new Vector3(0.072f, 0.13f, 0.072f), kulit);
            BagianFP(arm, PrimitiveType.Cylinder, new Vector3(-0.03f, 0.11f, -0.01f), new Vector3(0.044f, 0.012f, 0.044f), logam, new Vector3(0f, 0f, 82f));
            BagianFP(arm, PrimitiveType.Cylinder, new Vector3(-0.036f, 0.11f, -0.01f), new Vector3(0.032f, 0.008f, 0.032f), dialJam, new Vector3(0f, 0f, 82f));
            BagianFP(arm, PrimitiveType.Sphere, new Vector3(0f, 0.19f, 0.01f), new Vector3(0.085f, 0.095f, 0.085f), kulit);
            BagianFP(arm, PrimitiveType.Capsule, new Vector3(-0.042f, 0.22f, 0.048f), new Vector3(0.022f, 0.038f, 0.022f), kulit, new Vector3(58f, 22f, -20f));
            for (int f = 0; f < 4; f++)
                BagianFP(arm, PrimitiveType.Capsule, new Vector3(0.034f, 0.20f + f * 0.015f, 0.055f), new Vector3(0.019f, 0.035f, 0.019f), kulit, new Vector3(68f, -15f, 40f));
            var model = new Transform[CombatAndLeftHandController.Tabel.Length];
            for (int i = 0; i < model.Length; i++)
            {
                var id = CombatAndLeftHandController.Tabel[i].id;
                var g = Anak(pivot.gameObject, "FP_" + id).transform;
                BagianFP(g, PrimitiveType.Cylinder, new Vector3(0f, 0.10f, 0f), new Vector3(0.036f, 0.16f, 0.042f), kayu);
                for (int rg = 0; rg < 3; rg++)
                    BagianFP(g, PrimitiveType.Cylinder, new Vector3(0f, 0.02f + rg * 0.065f, 0f), new Vector3(0.041f, 0.016f, 0.047f), lilit);
                BagianFP(g, PrimitiveType.Cube, new Vector3(0f, -0.04f, -0.01f), new Vector3(0.044f, 0.04f, 0.055f), emas);
                if (id == ItemID.GolokPusaka || id == ItemID.GolokApiPusaka)
                {
                    var bilah = id == ItemID.GolokApiPusaka ? bara : baja;
                    BagianFP(g, PrimitiveType.Cylinder, new Vector3(0f, 0.24f, 0f), new Vector3(0.048f, 0.02f, 0.065f), emas);
                    BagianFP(g, PrimitiveType.Cube, new Vector3(0f, 0.43f, 0.018f), new Vector3(0.011f, 0.36f, 0.068f), logam);
                    BagianFP(g, PrimitiveType.Cube, new Vector3(0f, 0.45f, 0.052f), new Vector3(0.007f, 0.35f, 0.022f), bilah);
                    BagianFP(g, PrimitiveType.Cube, new Vector3(0f, 0.63f, 0.035f), new Vector3(0.009f, 0.11f, 0.048f), bilah, new Vector3(-28f, 0f, 0f));
                    if (id == ItemID.GolokApiPusaka)
                    {
                        var lBara = Anak(g.gameObject, "CahayaBara").AddComponent<Light>();
                        lBara.transform.localPosition = new Vector3(0f, 0.48f, 0.06f);
                        lBara.type = LightType.Point; lBara.range = 4.2f; lBara.intensity = 1.6f; lBara.color = new Color(1f, 0.44f, 0.12f); lBara.shadows = LightShadows.None;
                    }
                }
                else if (id == ItemID.KerisPusaka)
                {
                    BagianFP(g, PrimitiveType.Sphere, new Vector3(0f, 0.23f, 0f), new Vector3(0.068f, 0.028f, 0.095f), emas);
                    for (int k = 0; k < 3; k++)
                        BagianFP(g, PrimitiveType.Cube, new Vector3(0f, 0.32f + k * 0.11f, (k % 2 == 0 ? 0.014f : -0.014f)), new Vector3(0.009f, 0.13f, 0.036f), baja, new Vector3(k % 2 == 0 ? 18f : -18f, 0f, 0f));
                }
                else if (id == ItemID.Mandau)
                {
                    BagianFP(g, PrimitiveType.Cube, new Vector3(0f, 0.02f, -0.045f), new Vector3(0.026f, 0.06f, 0.06f), merah);
                    BagianFP(g, PrimitiveType.Cube, new Vector3(0f, 0.46f, 0.02f), new Vector3(0.010f, 0.48f, 0.056f), baja, new Vector3(-6f, 0f, 0f));
                    BagianFP(g, PrimitiveType.Cube, new Vector3(0f, 0.46f, 0.004f), new Vector3(0.013f, 0.44f, 0.018f), logam, new Vector3(-6f, 0f, 0f));
                }
                else if (id == ItemID.KapakBatu || id == ItemID.KapakBaja)
                {
                    var kepala = id == ItemID.KapakBatu ? batu : logam;
                    var mataKapak = id == ItemID.KapakBatu ? batu : baja;
                    BagianFP(g, PrimitiveType.Cylinder, new Vector3(0f, 0.22f, 0f), new Vector3(0.032f, 0.26f, 0.032f), kayu);
                    BagianFP(g, PrimitiveType.Cube, new Vector3(0f, 0.40f, 0.06f), new Vector3(0.042f, 0.11f, 0.18f), kepala);
                    BagianFP(g, PrimitiveType.Cube, new Vector3(0f, 0.40f, 0.145f), new Vector3(0.012f, 0.135f, 0.055f), mataKapak);
                    BagianFP(g, PrimitiveType.Cube, new Vector3(0f, 0.40f, 0.005f), new Vector3(0.055f, 0.08f, 0.06f), lilit);
                }
                else
                {
                    BagianFP(g, PrimitiveType.Cylinder, new Vector3(0f, 0.25f, 0f), new Vector3(0.032f, 0.30f, 0.032f), kayu);
                    BagianFP(g, PrimitiveType.Cube, new Vector3(0f, 0.46f, 0.09f), new Vector3(0.14f, 0.02f, 0.18f), baja, new Vector3(28f, 0f, 0f));
                }
                g.localRotation = Quaternion.Euler(-35f, 0f, 0f);
                g.gameObject.SetActive(false);
                model[i] = g;
            }
            combat.Pasang(pivot, model);
            EditorUtility.SetDirty(combat);

            var kanvas = _ui != null ? _ui.transform.Find("Canvas_HUD") : null;
            var kontrol = kanvas != null ? kanvas.Find("Kontrol") : null;
            if (kontrol == null) return;
            Geser(kontrol, "TombolLompat", new Vector2(-118f, 345f), 126f);
            Geser(kontrol, "TombolJongkok", new Vector2(-475f, 82f), 108f);
            Geser(kontrol, "TombolObor", new Vector2(-470f, 210f), 104f);
            var lingkaran = AssetDatabase.LoadAssetAtPath<Sprite>(FolderIkon + "/ui_lingkaran.png");
            var tebas = UIGambar(kontrol.gameObject, "TombolTebas", lingkaran, new Vector2(176f, 176f), new Vector2(1f, 0f), new Vector2(-135f, 145f));
            var img = tebas.GetComponent<Image>();
            img.color = ProcTex.Hex("#B5462EC0"); img.raycastTarget = true;
            tebas.AddComponent<TombolTahan>();
            var t = BuatTeks(tebas, "Label", "TEBAS", ProceduralAssetFactory.GetSafeFont(), 30, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200f, 44f));
            t.color = ProcTex.Hex("#F2EFE6"); t.raycastTarget = false;
            Catat("Tebang: pool pohon interaktif, melee golok/kapak, model senjata FP, tombol TEBAS");
        }

        static void BagianFP(Transform induk, PrimitiveType tipe, Vector3 pos, Vector3 skala, Material mat, Vector3 rot = default)
        {
            var g = GameObject.CreatePrimitive(tipe);
            Object.DestroyImmediate(g.GetComponent<Collider>());
            g.transform.SetParent(induk, false);
            g.transform.localPosition = pos; g.transform.localScale = skala; g.transform.localRotation = Quaternion.Euler(rot);
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
