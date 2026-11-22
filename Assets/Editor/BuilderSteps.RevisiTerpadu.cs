using AlasKeramat.Core;
using AlasKeramat.UI;
using AlasKeramat.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.EditorTools
{
    // Langkah 7z17: Revisi & Polish Terpadu — HUD Bar HP Boss Buto Ijo + Pill Status Musim/Level/Ancaman + Kilatan Kombo x2/x3,
    // serta 2 Boneka Latih Tempur interaktif di dekat lokasi Heli agar pemain bisa langsung merasakan kombo 3-hit & progresi kekuatan.
    public static partial class NusantaraMasterBuilder
    {
        public static int JumlahBonekaLatih;

        static void LangkahRevisiTerpadu()
        {
            JumlahBonekaLatih = 0;
            PasangBannerBossDanStatusHUD();
            BangunBonekaLatihAwal();
            Catat("Revisi Terpadu: HUD Boss/Kombo/Musim/Level terpasang & " + JumlahBonekaLatih + " boneka latih tempur aktif");
        }

        static void PasangBannerBossDanStatusHUD()
        {
            var hud = Object.FindFirstObjectByType<HUDInfo>(FindObjectsInactive.Include);
            if (hud == null) return;
            var canvas = hud.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.transform.Find("BannerBossDanStatus") != null) return;

            var root = new GameObject("BannerBossDanStatus", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            var rrt = (RectTransform)root.transform;
            rrt.anchorMin = Vector2.zero; rrt.anchorMax = Vector2.one;
            rrt.offsetMin = Vector2.zero; rrt.offsetMax = Vector2.zero;

            // 1. Pill Status Musim / Level / Ancaman Sekte (kanan atas rapi di bawah PenandaLandmarkStrip, y = -208)
            var pill = new GameObject("PillStatusSurvival", typeof(RectTransform), typeof(Image));
            pill.transform.SetParent(root.transform, false);
            var prt = (RectTransform)pill.transform;
            prt.anchorMin = new Vector2(1f, 1f); prt.anchorMax = new Vector2(1f, 1f);
            prt.pivot = new Vector2(1f, 1f); prt.anchoredPosition = new Vector2(-12f, -208f);
            prt.sizeDelta = new Vector2(198f, 20f);
            var pImg = pill.GetComponent<Image>();
            pImg.color = new Color(0.03f, 0.05f, 0.06f, 0.48f); pImg.raycastTarget = false;

            var txtStatus = BuatTeksTerpadu(pill.transform, "TeksStatus", 10, new Color(0.84f, 0.92f, 0.88f, 0.92f), TextAnchor.MiddleCenter);
            txtStatus.text = "Kemarau | Kuat Lv.1 Lari Lv.1";

            // 2. Kilatan Kombo x2 / FINISHER x3 (tengah-bawah reticle)
            var kGo = new GameObject("TeksKomboFlash", typeof(RectTransform), typeof(Text));
            kGo.transform.SetParent(root.transform, false);
            var krt = (RectTransform)kGo.transform;
            krt.anchorMin = new Vector2(0.5f, 0.5f); krt.anchorMax = new Vector2(0.5f, 0.5f);
            krt.pivot = new Vector2(0.5f, 0.5f); krt.anchoredPosition = new Vector2(0f, -68f);
            krt.sizeDelta = new Vector2(220f, 34f);
            var txtKombo = kGo.GetComponent<Text>();
            txtKombo.font = ProceduralAssetFactory.GetSafeFont();
            txtKombo.fontSize = 20; txtKombo.fontStyle = FontStyle.Bold;
            txtKombo.alignment = TextAnchor.MiddleCenter;
            txtKombo.color = new Color(1f, 0.82f, 0.25f, 1f);
            txtKombo.raycastTarget = false; txtKombo.enabled = false;
            txtKombo.text = "KOMBO x2";

            // 3. Bar HP Boss Mini Buto Ijo (muncul dinamis saat boss <= 34 m)
            var bossPanel = new GameObject("PanelHPBoss", typeof(RectTransform), typeof(Image));
            bossPanel.transform.SetParent(root.transform, false);
            var brt = (RectTransform)bossPanel.transform;
            brt.anchorMin = new Vector2(0.5f, 1f); brt.anchorMax = new Vector2(0.5f, 1f);
            brt.pivot = new Vector2(0.5f, 1f); brt.anchoredPosition = new Vector2(0f, -124f);
            brt.sizeDelta = new Vector2(320f, 32f);
            var bBg = bossPanel.GetComponent<Image>();
            bBg.color = new Color(0.08f, 0.02f, 0.02f, 0.72f); bBg.raycastTarget = false;

            var barBg = new GameObject("LatarBar", typeof(RectTransform), typeof(Image));
            barBg.transform.SetParent(bossPanel.transform, false);
            var bgrt = (RectTransform)barBg.transform;
            bgrt.anchorMin = new Vector2(0f, 0f); bgrt.anchorMax = new Vector2(1f, 0f);
            bgrt.pivot = new Vector2(0.5f, 0f); bgrt.offsetMin = new Vector2(8f, 5f); bgrt.offsetMax = new Vector2(-8f, 13f);
            var bgImg = barBg.GetComponent<Image>();
            bgImg.color = new Color(0.22f, 0.08f, 0.08f, 0.9f); bgImg.raycastTarget = false;

            var barFill = new GameObject("IsiBar", typeof(RectTransform), typeof(Image));
            barFill.transform.SetParent(barBg.transform, false);
            var frt = (RectTransform)barFill.transform;
            frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
            frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;
            var fillImg = barFill.GetComponent<Image>();
            fillImg.color = new Color(0.86f, 0.16f, 0.14f, 0.98f);
            fillImg.type = Image.Type.Filled; fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 1f; fillImg.raycastTarget = false;

            var txtBoss = BuatTeksTerpadu(bossPanel.transform, "NamaBoss", 13, new Color(1f, 0.90f, 0.85f, 0.96f), TextAnchor.UpperCenter);
            txtBoss.fontStyle = FontStyle.Bold;
            txtBoss.text = "Buto Ijo Penjaga Alas";
            bossPanel.SetActive(false);

            var comp = root.AddComponent<BannerBossDanStatusHUD>();
            comp.Pasang(txtStatus, txtKombo, bossPanel, txtBoss, fillImg);
            EditorUtility.SetDirty(comp);
        }

        static Text BuatTeksTerpadu(Transform induk, string nama, int ukuran, Color warna, TextAnchor rata)
        {
            var go = new GameObject(nama, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(induk, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(6f, 2f); rt.offsetMax = new Vector2(-6f, -2f);
            var t = go.GetComponent<Text>();
            t.font = ProceduralAssetFactory.GetSafeFont();
            t.fontSize = ukuran; t.alignment = rata;
            t.color = warna; t.raycastTarget = false;
            return t;
        }

        static void BangunBonekaLatihAwal()
        {
            var lm = GameObject.Find(NamaRoot + "/World/Landmark") ?? GameObject.Find(NamaRoot + "/Landmark") ?? _root;
            var grup = Anak(lm, "PosLatihanAwal");
            var kayu = AmbilMat("M_Kayu");
            var jerami = MatPolos("M_JeramiLatih", "#B89A58");

            Vector2[] pos = { new Vector2(24f, -438f), new Vector2(28f, -436f) };
            for (int i = 0; i < pos.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(pos[i].x, pos[i].y);
                var go = Anak(grup, "BonekaLatih_" + i);
                go.transform.position = new Vector3(pos[i].x, y, pos[i].y);
                go.transform.rotation = Quaternion.Euler(0f, -35f + i * 22f, 0f);
                go.layer = GameLayers.Enemy;
                var cc = go.AddComponent<CapsuleCollider>();
                cc.center = new Vector3(0f, 1.0f, 0f); cc.radius = 0.42f; cc.height = 1.9f;

                Hias(go, PrimitiveType.Cylinder, "Tiang", new Vector3(0f, 0.9f, 0f), new Vector3(0.14f, 0.9f, 0.14f), kayu, false);
                Hias(go, PrimitiveType.Cube, "PalangLengan", new Vector3(0f, 1.32f, 0f), new Vector3(1.2f, 0.12f, 0.12f), kayu, false);
                Hias(go, PrimitiveType.Cylinder, "BadanJerami", new Vector3(0f, 1.25f, 0f), new Vector3(0.42f, 0.48f, 0.42f), jerami, false);
                Hias(go, PrimitiveType.Sphere, "KepalaJerami", new Vector3(0f, 1.82f, 0f), new Vector3(0.32f, 0.34f, 0.32f), jerami, false);
                go.AddComponent<BonekaLatihSurvival>();
                JumlahBonekaLatih++;
            }
        }
    }
}
