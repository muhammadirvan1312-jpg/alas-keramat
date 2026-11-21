using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using AlasKeramat.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.EditorTools
{
    // Langkah 7l (T6b): Buku Panduan 26 blueprint (panel per kategori), penempatan cetak biru (hantu biru/merah), SaveSystem.
    public static partial class NusantaraMasterBuilder
    {
        static void LangkahBlueprint()
        {
            Anak(_sistem, "SaveSystem").AddComponent<SaveSystem>();
            var andi = Object.FindFirstObjectByType<AndiFPSController>();
            if (andi == null) return;
            var g = andi.gameObject.AddComponent<GuidebookBlueprintsAndCooking>();
            var biru = SafeMaterialFactory.CreatePBR("M_HantuBiru", ProcTex.Hex("#4FA8FF4D"), 0f, 0.2f, null, null, null, SurfaceMode.Transparent);
            AssetDatabase.DeleteAsset(FolderMat + "/M_HantuBiru.mat"); AssetDatabase.CreateAsset(biru, FolderMat + "/M_HantuBiru.mat");
            g.Pasang(AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_HantuBiru.mat"), AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_HantuSalah.mat"));
            EditorUtility.SetDirty(g);

            var kanvas = _ui != null ? _ui.transform.Find("Canvas_HUD") : null;
            if (kanvas == null) return;
            var font = ProceduralAssetFactory.GetSafeFont();
            var ling = AssetDatabase.LoadAssetAtPath<Sprite>(FolderIkon + "/ui_lingkaran.png");
            var root = UIStretch(kanvas.gameObject, "BukuPanduanUI", Vector2.zero, Vector2.one);
            var ui = root.AddComponent<BukuPanduanUI>();
            var panel = UIStretch(root, "Panel", new Vector2(0.18f, 0.12f), new Vector2(0.82f, 0.9f));
            var bg = panel.AddComponent<Image>(); bg.color = ProcTex.Hex("#3A2A18E6");
            var jdl = BuatTeks(panel, "Judul", "BUKU PANDUAN", font, 40, new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(900f, 60f));
            jdl.color = ProcTex.Hex("#F2C14E");
            var viewport = UIStretch(panel, "DaftarGulir", new Vector2(0f, 0.16f), new Vector2(1f, 0.88f));
            viewport.AddComponent<RectMask2D>();
            var scroll = viewport.AddComponent<ScrollRect>(); scroll.horizontal = false;
            var konten = UIStretch(viewport, "IsiDaftar", new Vector2(0f, 1f), new Vector2(1f, 1f));
            var kontenRt=(RectTransform)konten.transform; kontenRt.pivot=new Vector2(0.5f,1f); kontenRt.sizeDelta=new Vector2(0f,1850f);
            scroll.viewport=(RectTransform)viewport.transform; scroll.content=kontenRt;
            Text tx;
            var semuaBp = GuidebookBlueprintsAndCooking.AmbilSemua();
            for (int k = 0; k < GuidebookBlueprintsAndCooking.Kategori.Length; k++)
            {
                string kat = GuidebookBlueprintsAndCooking.Kategori[k];
                var h = BuatTeks(konten, "Kat" + k, kat.ToUpperInvariant(), font, 28, new Vector2(0f, 1f), new Vector2(150f + k * 300f, -110f), new Vector2(290f, 40f));
                h.color = ProcTex.Hex("#E6D7B0");
                int baris = 0;
                for (int i = 0; i < semuaBp.Length; i++)
                {
                    var d = semuaBp[i];
                    if (d.category != kat) continue;
                    string biaya = "";
                    foreach (var c in d.cost) biaya += (biaya.Length > 0 ? ", " : "") + c.count + " " + ItemDatabase.Get(c.id).displayName;
                    var b = TombolPanel(konten, "BP_" + d.id, d.displayName, font, Vector2.zero, new Vector2(280f, 110f), ProcTex.Hex("#6B4A2A"), out tx);
                    var rt = (RectTransform)b.transform; rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f); rt.anchoredPosition = new Vector2(150f + k * 300f, -190f - baris * 125f);
                    tx.fontSize = 22; tx.alignment = TextAnchor.UpperCenter;
                    var bt = BuatTeks(b.gameObject, "Biaya", biaya, font, 16, new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(270f, 50f));
                    bt.horizontalOverflow = HorizontalWrapMode.Wrap; bt.color = ProcTex.Hex("#E6D7B0"); bt.raycastTarget = false;
                    UnityEventTools.AddIntPersistentListener(b.onClick, ui.Pilih, i);
                    baris++;
                }
            }
            var tutup = TombolPanel(panel, "TombolTutupBuku", "TUTUP", font, new Vector2(0f, 60f), new Vector2(220f, 70f), ProcTex.Hex("#3A3A3A"), out tx);
            UnityEventTools.AddPersistentListener(tutup.onClick, ui.Buka);

            var tempat = UIStretch(root, "PanelTempat", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            var bT = TombolPanel(tempat, "TombolTandai", "TANDAI", font, new Vector2(0f, 120f), new Vector2(260f, 90f), ProcTex.Hex("#2E6EA8"), out tx);
            var bP = TombolPanel(tempat, "TombolPutar", "PUTAR", font, new Vector2(-300f, 120f), new Vector2(200f, 80f), ProcTex.Hex("#5B5A4E"), out tx);
            var bB = TombolPanel(tempat, "TombolBatalBP", "BATAL", font, new Vector2(300f, 120f), new Vector2(200f, 80f), ProcTex.Hex("#5B2A2A"), out tx);
            UnityEventTools.AddPersistentListener(bT.onClick, ui.Tandai);
            UnityEventTools.AddPersistentListener(bP.onClick, ui.Putar);
            UnityEventTools.AddPersistentListener(bB.onClick, ui.Batal);

            var bBuku = Tombol(kanvas.gameObject, "TombolBuku", "BUKU", ling, font, 110f, new Vector2(-240f, -420f), ProcTex.Hex("#5A3E26"));
            var rtB = (RectTransform)bBuku.transform; rtB.anchorMin = rtB.anchorMax = new Vector2(1f, 1f);
            UnityEventTools.AddPersistentListener(bBuku.onClick, ui.Buka);
            ui.Pasang(panel, tempat, bBuku.gameObject);
            EditorUtility.SetDirty(ui);
            panel.SetActive(false); tempat.SetActive(false);
            Catat("Buku Panduan 26 blueprint, penempatan cetak biru, SaveSystem");
        }
    }
}
