using AlasKeramat.Core;
using AlasKeramat.Player;
using AlasKeramat.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.EditorTools
{
    // Langkah 7f (T3e): foley Andi, subtitle 2 baris + monolog pembuka, sabuk cepat 4 slot.
    public static partial class NusantaraMasterBuilder
    {
        public static readonly string[] MonologPembuka =
        {
            "ANDI: Ugh... kepalaku... Helinya jatuh.",
            "ANDI: Pilotnya... Kang Deden? Aku harus cari koper darurat dulu.",
            "ANDI: Hutan ini terlalu sunyi. Sebelum gelap, aku butuh obor."
        };

        static void LangkahHUDLanjut()
        {
            var andi = Object.FindFirstObjectByType<AndiFPSController>();
            if (andi != null) andi.gameObject.AddComponent<FoleyAndi>();
            var kanvas = _ui != null ? _ui.transform.Find("Canvas_HUD") : null;
            if (kanvas == null) return;
            var font = ProceduralAssetFactory.GetSafeFont();

            var sub = BuatTeks(kanvas.gameObject, "Subtitle", "", font, 34, new Vector2(0.5f, 0f), new Vector2(0f, 200f), new Vector2(1500f, 96f));
            sub.color = ProcTex.Hex("#F2EFE6");
            sub.horizontalOverflow = HorizontalWrapMode.Wrap; sub.raycastTarget = false;
            var ss = sub.gameObject.AddComponent<SubtitleSystem>();
            ss.Pasang(sub, MonologPembuka);
            EditorUtility.SetDirty(ss);

            var lingkaran = AssetDatabase.LoadAssetAtPath<Sprite>(FolderIkon + "/ui_lingkaran.png");
            var namaIkon = IconPainter.NamaIkon;
            var sprite = new Sprite[namaIkon.Length];
            for (int i = 0; i < namaIkon.Length; i++) sprite[i] = AssetDatabase.LoadAssetAtPath<Sprite>(FolderIkon + "/ikon_" + namaIkon[i] + ".png");
            var panel = UIStretch(kanvas.gameObject, "SabukCepat", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            var sabuk = panel.AddComponent<SabukCepat>();
            var slot = new GameObject[SabukCepat.JumlahSlot];
            var ikon = new Image[SabukCepat.JumlahSlot];
            var jml = new Text[SabukCepat.JumlahSlot];
            const float Ukuran = 96f, Jarak = 112f;
            for (int i = 0; i < SabukCepat.JumlahSlot; i++)
            {
                float x = (i - (SabukCepat.JumlahSlot - 1) * 0.5f) * Jarak;
                var b = UIGambar(panel, "Slot" + i, lingkaran, new Vector2(Ukuran, Ukuran), new Vector2(0.5f, 0.5f), new Vector2(x, 70f));
                var im = b.GetComponent<Image>(); im.color = new Color(0.08f, 0.07f, 0.05f, 0.6f); im.raycastTarget = true;
                var btn = b.AddComponent<Button>(); btn.targetGraphic = im;
                UnityEventTools.AddIntPersistentListener(btn.onClick, sabuk.TekanSlot, i);
                var ik = UIGambar(b, "Ikon", null, new Vector2(Ukuran * 0.62f, Ukuran * 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero);
                ik.GetComponent<Image>().color = ProcTex.Hex("#F2EFE6");
                var t = BuatTeks(b, "Jumlah", "", font, 22, new Vector2(1f, 0f), new Vector2(-14f, 14f), new Vector2(40f, 28f));
                t.color = ProcTex.Hex("#F2C14E"); t.raycastTarget = false;
                slot[i] = b; ikon[i] = ik.GetComponent<Image>(); jml[i] = t;
                b.SetActive(false);
            }
            sabuk.Pasang(slot, ikon, jml, namaIkon, sprite);
            EditorUtility.SetDirty(sabuk);
            Catat("Foley Andi, subtitle + monolog pembuka, sabuk cepat 4 slot");
        }
    }
}
