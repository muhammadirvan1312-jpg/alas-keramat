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
            prt.anchoredPosition = new Vector2(40f, -32f); prt.sizeDelta = new Vector2(440f, 130f);

            var warna = new[] { "#E53935", "#7FD3FF", "#F28C28", "#2EC4C9", "#9B59D0" };
            var latar = new[] { "#5A1414", "#4A4F55", "#3A2A18", "#123A3C", "#2C1A3C" };
            var lebar = new[] { 360f, 360f, 260f, 260f, 260f };
            var tinggi = new[] { 22f, 16f, 10f, 10f, 10f };
            var nama = new[] { "Darah", "Stamina", "Kenyang", "Haus", "Kewarasan" };
            var isi = new RectTransform[HUDVital.Jumlah];
            var img = new Image[HUDVital.Jumlah];
            RectTransform plafon = null, beku = null;
            float y = 0f;
            for (int i = 0; i < HUDVital.Jumlah; i++)
            {
                var bar = Kotak2D(panel, "Bar" + nama[i], new Vector2(0f, y), new Vector2(lebar[i], tinggi[i]), ProcTex.Hex(latar[i]));
                var fill = Kotak2D(bar, "Isi", Vector2.zero, new Vector2(lebar[i], tinggi[i]), ProcTex.Hex(warna[i]));
                isi[i] = (RectTransform)fill.transform; img[i] = fill.GetComponent<Image>();
                // Bar bulat (sprite 9-slice sama dengan tombol HUD): latar redup, isian penuh warna.
                var gBar = bar.GetComponent<Image>(); GayakanTombol(gBar, ProcTex.Hex(latar[i]), null); gBar.color = new Color(gBar.color.r, gBar.color.g, gBar.color.b, 0.85f);
                GayakanTombol(img[i], ProcTex.Hex(warna[i]), null); img[i].color = ProcTex.Hex(warna[i]);
                img[i].type = Image.Type.Filled; img[i].fillMethod = Image.FillMethod.Horizontal; img[i].fillOrigin = 0;
                if (i == HUDVital.Darah) plafon = Rentang(bar, "PlafonDarah", ProcTex.Hex("#2A0A0AE6"));
                if (i == HUDVital.Stamina) beku = Rentang(bar, "BekuStamina", ProcTex.Hex("#2E6EA8E6"));
                y -= tinggi[i] + 8f;
            }
            var suhu = BuatTeks(panel, "LabelSuhu", "", font, 22, new Vector2(0f, 1f), new Vector2(0f, y - 4f), new Vector2(600f, 30f));
            suhu.alignment = TextAnchor.UpperLeft; suhu.rectTransform.pivot = new Vector2(0f, 1f); suhu.color = ProcTex.Hex("#BFB8A8");
            var hud = panel.AddComponent<HUDVital>();
            hud.Pasang(isi, img, plafon, beku, suhu);
            EditorUtility.SetDirty(hud);

            var info = kanvas.Find("LabelJam");
            if (info != null) ((RectTransform)info).anchoredPosition = new Vector2(40f, -32f + y - 40f);
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
