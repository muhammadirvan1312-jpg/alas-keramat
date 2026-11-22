using AlasKeramat.Core;
using AlasKeramat.Player;
using AlasKeramat.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.EditorTools
{
    // Langkah 7e (T3d): vital Andi + bar HUD kiri-atas (09 §2.1); baris info turun ke bawah bar.
    public static partial class NusantaraMasterBuilder
    {
        static void LangkahVital()
        {
            var andi = Object.FindFirstObjectByType<AndiFPSController>();
            if (andi == null) { Debug.LogError("[AK][Builder] Andi tidak ada, vital dilewati"); return; }
            andi.gameObject.AddComponent<AndiVitalsAndSanity>();
            var kanvas = _ui != null ? _ui.transform.Find("Canvas_HUD") : null;
            if (kanvas == null) return;
            var font = ProceduralAssetFactory.GetSafeFont();

            var panel = Anak(kanvas.gameObject, "Vital");
            panel.layer = GameLayers.UI;
            var prt = panel.AddComponent<RectTransform>();
            prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(0f, 1f);
            prt.anchoredPosition = new Vector2(28f, -24f); prt.sizeDelta = new Vector2(320f, 134f);

            // Kartu kaca gelap profesional bergaya AAA survival di belakang bar vital.
            var kartuBg = Kotak2D(panel, "LatarVitalGlass", new Vector2(-12f, 10f), new Vector2(316f, 128f), new Color(0.04f, 0.06f, 0.08f, 0.66f));
            var bgIm = kartuBg.GetComponent<Image>();
            GayakanTombol(bgIm, new Color(0.04f, 0.06f, 0.08f, 0.66f), null);
            bgIm.color = new Color(0.04f, 0.06f, 0.08f, 0.66f);

            var warna = new[] { "#C83838", "#DCE6EC", "#D49844", "#4DAAB8", "#8C6AB8" };
            var latar = new[] { "#161C22", "#161C22", "#161C22", "#161C22", "#161C22" };
            var lebar = new[] { 288f, 288f, 218f, 218f, 218f };
            var tinggi = new[] { 16f, 10f, 7f, 7f, 7f };
            var nama = new[] { "Darah", "Stamina", "Kenyang", "Haus", "Kewarasan" };
            var isi = new RectTransform[HUDVital.Jumlah];
            var img = new Image[HUDVital.Jumlah];
            RectTransform plafon = null, beku = null;
            float y = -2f;
            for (int i = 0; i < HUDVital.Jumlah; i++)
            {
                var bar = Kotak2D(panel, "Bar" + nama[i], new Vector2(0f, y), new Vector2(lebar[i], tinggi[i]), ProcTex.Hex(latar[i]));
                var fill = Kotak2D(bar, "Isi", Vector2.zero, new Vector2(lebar[i], tinggi[i]), ProcTex.Hex(warna[i]));
                isi[i] = (RectTransform)fill.transform; img[i] = fill.GetComponent<Image>();
                var gBar = bar.GetComponent<Image>(); GayakanTombol(gBar, ProcTex.Hex(latar[i]), null); gBar.color = new Color(0.09f, 0.11f, 0.13f, 0.88f);
                GayakanTombol(img[i], ProcTex.Hex(warna[i]), null); img[i].color = ProcTex.Hex(warna[i]);
                img[i].type = Image.Type.Filled; img[i].fillMethod = Image.FillMethod.Horizontal; img[i].fillOrigin = 0;
                if (i == HUDVital.Darah) plafon = Rentang(bar, "PlafonDarah", ProcTex.Hex("#2A0A0AE6"));
                if (i == HUDVital.Stamina) beku = Rentang(bar, "BekuStamina", ProcTex.Hex("#2E6EA8E6"));
                y -= tinggi[i] + 7f;
            }
            var suhu = BuatTeks(panel, "LabelSuhu", "", font, 16, new Vector2(0f, 1f), new Vector2(0f, y - 4f), new Vector2(360f, 24f));
            suhu.alignment = TextAnchor.UpperLeft; suhu.rectTransform.pivot = new Vector2(0f, 1f); suhu.color = new Color(0.78f, 0.82f, 0.78f, 0.92f);
            var hud = panel.AddComponent<HUDVital>();
            hud.Pasang(isi, img, plafon, beku, suhu);
            EditorUtility.SetDirty(hud);

            var info = kanvas.Find("LabelJam");
            if (info != null)
            {
                ((RectTransform)info).anchoredPosition = new Vector2(28f, -24f + y - 34f);
                var tInfo = info.GetComponent<Text>();
                if (tInfo != null) { tInfo.fontSize = 18; tInfo.color = new Color(0.90f, 0.92f, 0.88f, 0.92f); }
            }
            Catat("Vital Andi + 5 bar HUD + suhu/basah");
        }

        static GameObject Kotak2D(GameObject induk, string nama, Vector2 pos, Vector2 ukuran, Color warna)
        {
            var go = Anak(induk, nama);
            go.layer = GameLayers.UI;
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos; rt.sizeDelta = ukuran;
            var im = go.AddComponent<Image>(); im.color = warna; im.raycastTarget = false;
            return go;
        }

        static RectTransform Rentang(GameObject induk, string nama, Color warna)
        {
            var go = Anak(induk, nama);
            go.layer = GameLayers.UI;
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f); rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
            var im = go.AddComponent<Image>(); im.color = warna; im.raycastTarget = false;
            return rt;
        }
    }
}
