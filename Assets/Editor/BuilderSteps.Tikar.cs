using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.EditorTools
{
    // Langkah 7j (T5c): panggung Tikar Pandan 3D (layer InventoryStage, kamera & lampu sendiri), layar Tikar (RAKIT/MAKAN/PAKAI/BATAL/TUTUP), tombol TAS, kotak zirah HUD.
    public static partial class NusantaraMasterBuilder
    {
        public static readonly Vector3 PosTikar = new Vector3(0f, -300f, 0f);

        static void LangkahTikar()
        {
            var akar = Anak(_root, "TikarPanggung");
            akar.transform.position = PosTikar;
            int lyr = GameLayers.InventoryStage;
            var alas = GameObject.CreatePrimitive(PrimitiveType.Cube);
            alas.name = "Tikar"; alas.layer = lyr; Object.DestroyImmediate(alas.GetComponent<Collider>());
            alas.transform.SetParent(akar.transform, false); alas.transform.localScale = new Vector3(2.6f, 0.02f, 2.2f); alas.transform.localPosition = new Vector3(0f, 0f, -0.35f);
            alas.GetComponent<MeshRenderer>().sharedMaterial = MatPolos("M_TikarPandan", "#B8A66A");
            var cincin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cincin.name = "LingkaranRakit"; cincin.layer = lyr; Object.DestroyImmediate(cincin.GetComponent<Collider>());
            cincin.transform.SetParent(akar.transform, false); cincin.transform.localScale = new Vector3(0.55f, 0.004f, 0.55f); cincin.transform.localPosition = new Vector3(0f, 0.012f, 0.55f);
            cincin.GetComponent<MeshRenderer>().sharedMaterial = MatPolos("M_LingkaranRakit", "#6A3E1E");
            var wadah = Anak(akar, "Benda"); wadah.transform.localPosition = new Vector3(0f, 0f, 0.05f);
            var camGo = Anak(akar, "KameraTikar");
            camGo.transform.localPosition = new Vector3(0f, 1.9f, 1.55f);
            camGo.transform.LookAt(akar.transform.position + new Vector3(0f, 0f, -0.35f));
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = ProcTex.Hex("#140E08");
            cam.cullingMask = 1 << lyr; cam.depth = 10; cam.fieldOfView = 55f; cam.nearClipPlane = 0.05f; cam.farClipPlane = 10f; cam.enabled = false;
            var lampu = Anak(akar, "LampuTikar").AddComponent<Light>();
            lampu.type = LightType.Point; lampu.range = 6f; lampu.intensity = 2.2f; lampu.color = ProcTex.Hex("#FFE2B8"); lampu.cullingMask = 1 << lyr;
            lampu.transform.localPosition = new Vector3(0f, 1.6f, 0.6f);
            if (_sun != null) _sun.cullingMask &= ~(1 << lyr);
            if (_moon != null) _moon.cullingMask &= ~(1 << lyr);
            var pg = akar.AddComponent<TikarPanggung>();
            pg.Pasang(cam, wadah.transform, cincin.transform);
            EditorUtility.SetDirty(pg);

            var kanvas = _ui != null ? _ui.transform.Find("Canvas_HUD") : null;
            if (kanvas == null) return;
            var font = ProceduralAssetFactory.GetSafeFont();
            var lingk = AssetDatabase.LoadAssetAtPath<Sprite>(FolderIkon + "/ui_lingkaran.png");
            var root = UIStretch(kanvas.gameObject, "TikarUI", Vector2.zero, Vector2.one);
            var ui = root.AddComponent<TikarUI>();
            var panel = UIStretch(root, "Panel", Vector2.zero, Vector2.one);
            var bg = panel.AddComponent<Image>(); bg.color = new Color(0f, 0f, 0f, 0.15f); bg.raycastTarget = true;
            var judul = BuatTeks(panel, "Judul", "TIKAR PANDAN - ketuk benda untuk memilih", font, 30, new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(1700f, 50f));
            judul.color = ProcTex.Hex("#F2C14E");
            var label = new Text[TikarPanggung.MaksTampil];
            for (int i = 0; i < label.Length; i++)
            {
                label[i] = BuatTeks(panel, "Label" + i, "", font, 18, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220f, 28f));
                label[i].raycastTarget = false; label[i].color = ProcTex.Hex("#F2EFE6");
            }
            Text teksRakit;
            var bRakit = TombolPanel(panel, "TombolRakit", "RAKIT", font, new Vector2(0f, 90f), new Vector2(520f, 90f), ProcTex.Hex("#B5462E"), out teksRakit);
            Text tx;
            var bMakan = TombolPanel(panel, "TombolMakan", "MAKAN", font, new Vector2(-560f, 90f), new Vector2(240f, 80f), ProcTex.Hex("#5B7A3A"), out tx);
            var bPakai = TombolPanel(panel, "TombolPakai", "PAKAI ZIRAH", font, new Vector2(560f, 90f), new Vector2(280f, 80f), ProcTex.Hex("#4A6F8A"), out tx);
            var bBatal = TombolPanel(panel, "TombolBatal", "BATAL", font, new Vector2(-820f, 90f), new Vector2(200f, 80f), ProcTex.Hex("#5B5A4E"), out tx);
            var bTutup = TombolPanel(panel, "TombolTutup", "TUTUP", font, new Vector2(820f, 90f), new Vector2(200f, 80f), ProcTex.Hex("#3A3A3A"), out tx);
            UnityEventTools.AddPersistentListener(bRakit.onClick, ui.TekanRakit);
            UnityEventTools.AddPersistentListener(bMakan.onClick, ui.TekanMakan);
            UnityEventTools.AddPersistentListener(bPakai.onClick, ui.TekanPakai);
            UnityEventTools.AddPersistentListener(bBatal.onClick, ui.TekanBatal);
            UnityEventTools.AddPersistentListener(bTutup.onClick, ui.Tutup);
            var sebelum=TombolPanel(panel,"Sebelum","SEBELUM",font,new Vector2(-720f,200f),new Vector2(280f,70f),ProcTex.Hex("#3A3A3A"),out tx);
            var berikut=TombolPanel(panel,"Berikut","BERIKUT",font,new Vector2(720f,200f),new Vector2(280f,70f),ProcTex.Hex("#3A3A3A"),out tx);
            var senjata=TombolPanel(panel,"PakaiSenjata","PAKAI SENJATA",font,new Vector2(0f,200f),new Vector2(380f,70f),ProcTex.Hex("#4A6F8A"),out tx);
            var halaman=BuatTeks(panel,"Halaman","",font,24,new Vector2(0.5f,1f),new Vector2(0,-100f),new Vector2(500f,40f)); halaman.raycastTarget=false;
            var bDaftar = TombolPanel(panel, "TombolDaftarResep", "DAFTAR RESEP", font, new Vector2(660f, -95f), new Vector2(280f, 64f), ProcTex.Hex("#3E6A4E"), out tx);
            var rtDaftar = (RectTransform)bDaftar.transform; rtDaftar.anchorMin = rtDaftar.anchorMax = new Vector2(0.5f, 1f);
            tx.fontSize = 22;

            // Panel gulir 1-ketuk daftar resep senjata, alat, obat & zirah
            var panelResep = UIStretch(panel, "PanelResep", new Vector2(0.14f, 0.14f), new Vector2(0.86f, 0.88f));
            var bgResep = panelResep.AddComponent<Image>(); bgResep.color = ProcTex.Hex("#1E1812F2");
            var jdlR = BuatTeks(panelResep, "JudulResep", "DAFTAR RESEP SURVIVAL (KETUK UNTUK RAKIT)", font, 30, new Vector2(0.5f, 1f), new Vector2(0f, -38f), new Vector2(1100f, 52f));
            jdlR.color = ProcTex.Hex("#F2C14E");
            var vpR = UIStretch(panelResep, "GulirResep", new Vector2(0.03f, 0.14f), new Vector2(0.97f, 0.88f));
            vpR.AddComponent<RectMask2D>();
            var srR = vpR.AddComponent<ScrollRect>(); srR.horizontal = false;
            var isiR = UIStretch(vpR, "IsiResep", new Vector2(0f, 1f), new Vector2(1f, 1f));
            var allRecipes = RecipeDatabase.All;
            var rtIsiR = (RectTransform)isiR.transform; rtIsiR.pivot = new Vector2(0.5f, 1f);
            int rowsR = (allRecipes.Count + 1) / 2;
            rtIsiR.sizeDelta = new Vector2(0f, 40f + rowsR * 105f);
            srR.viewport = (RectTransform)vpR.transform; srR.content = rtIsiR;
            var tombolResep = new Button[allRecipes.Count];
            var idResep = new int[allRecipes.Count];
            for (int rIdx = 0; rIdx < allRecipes.Count; rIdx++)
            {
                var rec = allRecipes[rIdx];
                idResep[rIdx] = rec.recipeId;
                int col = rIdx % 2, row = rIdx / 2;
                var sbBahan = new System.Text.StringBuilder();
                foreach (var inp in rec.inputs) { if (sbBahan.Length > 0) sbBahan.Append(" + "); sbBahan.Append(inp.count).Append(" ").Append(ItemDatabase.Get(inp.id).displayName); }
                if (rec.requiredTool != ItemID.None) sbBahan.Append(" [").Append(ItemDatabase.Get(rec.requiredTool).displayName).Append("]");
                string labelR = ItemDatabase.Get(rec.output.id).displayName + (rec.output.count > 1 ? " x" + rec.output.count : "");
                var br = TombolPanel(isiR, "Resep_" + rec.recipeId, labelR, font, Vector2.zero, new Vector2(580f, 92f), ProcTex.Hex("#383430"), out tx);
                var rtr = (RectTransform)br.transform; rtr.anchorMin = rtr.anchorMax = new Vector2(0f, 1f);
                rtr.anchoredPosition = new Vector2(310f + col * 620f, -60f - row * 105f);
                tx.fontSize = 22; tx.alignment = TextAnchor.UpperCenter;
                var sub = BuatTeks(br.gameObject, "Bahan", sbBahan.ToString(), font, 16, new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(560f, 36f));
                sub.color = ProcTex.Hex("#DFD4B8"); sub.horizontalOverflow = HorizontalWrapMode.Wrap; sub.raycastTarget = false;
                UnityEventTools.AddIntPersistentListener(br.onClick, ui.RakitResep, rec.recipeId);
                tombolResep[rIdx] = br;
            }
            var bTutupR = TombolPanel(panelResep, "TutupResep", "TUTUP RESEP", font, new Vector2(0f, 44f), new Vector2(260f, 64f), ProcTex.Hex("#4A3A2E"), out tx);
            UnityEventTools.AddPersistentListener(bTutupR.onClick, ui.TogglePanelResep);
            UnityEventTools.AddPersistentListener(bDaftar.onClick, ui.TogglePanelResep);
            panelResep.SetActive(false);

            UnityEventTools.AddPersistentListener(sebelum.onClick,ui.HalamanSebelum);
            UnityEventTools.AddPersistentListener(berikut.onClick,ui.HalamanBerikut);
            UnityEventTools.AddPersistentListener(senjata.onClick,ui.TekanSenjata);
            ui.PasangHalaman(halaman,senjata);
            ui.PasangResep(panelResep, tombolResep, idResep);
            ui.Pasang(panel, judul, label, bRakit, bMakan, bPakai, teksRakit);
            EditorUtility.SetDirty(ui);

            var bTas = Tombol(kanvas.gameObject, "TombolTas", "TAS", lingk, font, 110f, new Vector2(-110f, -420f), ProcTex.Hex("#8A6A3A"));
            var rtTas = (RectTransform)bTas.transform; rtTas.anchorMin = rtTas.anchorMax = new Vector2(1f, 1f);
            UnityEventTools.AddPersistentListener(bTas.onClick, ui.Buka);

            // Kotak zirah di atas bar darah.
            var vital = kanvas.Find("Vital");
            var hud = vital != null ? vital.GetComponent<HUDVital>() : null;
            if (hud != null)
            {
                var kotak = new Image[10];
                for (int i = 0; i < kotak.Length; i++) { var k = Kotak2D(vital.gameObject, "Zirah" + i, new Vector2(i * 36f, 18f), new Vector2(30f, 12f), new Color(1f, 1f, 1f, 0.12f)); kotak[i] = k.GetComponent<Image>(); }
                hud.PasangZirah(kotak);
                EditorUtility.SetDirty(hud);
            }
            panel.SetActive(false);
            Catat("Tikar Pandan 3D: panggung, kamera, layar rakit/makan/zirah, tombol TAS, kotak zirah");
        }

        static Button TombolPanel(GameObject induk, string nama, string teks, Font font, Vector2 pos, Vector2 ukuran, Color warna, out Text label)
        {
            var go = Anak(induk, nama); go.layer = GameLayers.UI;
            var rt = go.AddComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f); rt.anchoredPosition = pos; rt.sizeDelta = ukuran;
            var img = go.AddComponent<Image>(); img.color = new Color(warna.r, warna.g, warna.b, 0.85f);
            var b = go.AddComponent<Button>(); b.targetGraphic = img;
            label = BuatTeks(go, "Label", teks, font, 28, new Vector2(0.5f, 0.5f), Vector2.zero, ukuran);
            label.raycastTarget = false; label.color = ProcTex.Hex("#F2EFE6");
            GayakanTombol(img, warna, label);
            return b;
        }
    }
}
