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
            return b;
        }
    }
}
