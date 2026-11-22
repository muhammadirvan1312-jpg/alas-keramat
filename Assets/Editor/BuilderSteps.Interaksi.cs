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
    // Langkah 7c (T3b): inventaris, 3 koper darurat + puing heli (03 §9 Zona 1, koordinat x2), interaksi TANGAN, obor, radar GPS, toast.
    public static partial class NusantaraMasterBuilder
    {
        public static readonly Vector3[] PosisiKoper = { new Vector3(-6f, 0f, -434f), new Vector3(16f, 0f, -460f), new Vector3(-20f, 0f, -456f) };
        public static readonly ItemID[][] IsiKoper =
        {
            new[] { ItemID.GolokPusaka },
            new[] { ItemID.GPSHandset, ItemID.BukuPanduan, ItemID.KorekGas, ItemID.PetaKertas },
            new[] { ItemID.JamuHerbal, ItemID.OborBambu, ItemID.RansumDarurat, ItemID.BukuCatatan }
        };
        public static readonly int[][] JumlahKoper = { new[] { 1 }, new[] { 1, 1, 1, 1 }, new[] { 2, 1, 2, 1 } };
        static readonly Vector3 UkuranKoper = new Vector3(0.6f, 0.25f, 0.4f);

        // Penanda radar: heli, Beringin Agung, 2 mulut gua (koordinat landmark T2c).
        static readonly Vector3[] PenandaRadar = { new Vector3(8f, 0f, -448f), new Vector3(0f, 0f, -18f), new Vector3(256f, 0f, -276f), new Vector3(0f, 0f, 694f) };

        static void LangkahInteraksi()
        {
            var inv = _sistem.AddComponent<TikarPandan3DInventory>();
            EditorUtility.SetDirty(inv);

            var induk = Anak(_root, "Interaksi");
            var oranye = AmbilMat("M_CatOranye");
            for (int i = 0; i < PosisiKoper.Length; i++)
                BuatKoper(induk, "Koper_K" + (i + 1), PosisiKoper[i], IsiKoper[i], JumlahKoper[i], i == 0 ? (int)LoreID.L01 : 0, oranye);
            var logam = AmbilMat("M_LogamKarat");
            BuatBenda(induk, "Puing_Logam1", new Vector3(2f, 0f, -444f), new Vector3(0.5f, 0.05f, 0.4f), logam, "Logam Bekas", ItemID.LogamBekas, 1);
            BuatBenda(induk, "Puing_Logam2", new Vector3(13f, 0f, -452f), new Vector3(0.4f, 0.05f, 0.6f), logam, "Logam Bekas", ItemID.LogamBekas, 1);
            BuatBenda(induk, "Selimut_Batik", new Vector3(-2f, 0f, -452f), new Vector3(0.8f, 0.03f, 0.6f), AmbilMat("M_CatMerahPudar"), "Kain Batik", ItemID.KainBatik, 1);

            var andi = Object.FindFirstObjectByType<AndiFPSController>();
            if (andi != null) { andi.gameObject.AddComponent<InteraksiAndi>(); andi.gameObject.AddComponent<TanganKiri>(); }

            var kanvas = _ui != null ? _ui.transform.Find("Canvas_HUD") : null;
            var kontrol = kanvas != null ? kanvas.Find("Kontrol") : null;
            var tc = kontrol != null ? kontrol.GetComponent<TouchControls>() : null;
            if (kanvas == null || tc == null) { Debug.LogError("[AK][Builder] HUD Andi tidak ada, HUD interaksi dilewati"); return; }
            var font = ProceduralAssetFactory.GetSafeFont();
            var lingkaran = AssetDatabase.LoadAssetAtPath<Sprite>(FolderIkon + "/ui_lingkaran.png");
            var cincin = AssetDatabase.LoadAssetAtPath<Sprite>(FolderIkon + "/ui_cincin.png");

            var bTangan = Tombol(kontrol.gameObject, "TombolTangan", "TANGAN", lingkaran, font, 148f, new Vector2(-320f, 235f), ProcTex.Hex("#E0B33A"));
            bTangan.gameObject.AddComponent<TanganTahan>();
            var prompt = BuatTeks(bTangan.gameObject, "Prompt", "", font, 24, new Vector2(0.5f, 1f), new Vector2(0f, 22f), new Vector2(460f, 36f));
            prompt.color = ProcTex.Hex("#F2C14E");
            var bObor = Tombol(kontrol.gameObject, "TombolObor", "OBOR", lingkaran, font, 104f, new Vector2(-470f, 210f), ProcTex.Hex("#C0612B"));
            UnityEventTools.AddPersistentListener(bObor.onClick, tc.TekanObor);
            tc.PasangTangan(bTangan.GetComponent<Image>(), prompt, bObor.gameObject);

            var dotGo = kanvas.Find("CrosshairDot") != null ? kanvas.Find("CrosshairDot").gameObject : UIGambar(kanvas.gameObject, "CrosshairDot", lingkaran, new Vector2(7f, 7f), new Vector2(0.5f, 0.5f), Vector2.zero);
            var dotImg = dotGo.GetComponent<Image>(); dotImg.color = new Color(0.96f, 0.94f, 0.88f, 0.72f); dotImg.raycastTarget = false;
            var pBanner = Anak(kanvas.gameObject, "PromptTengah");
            pBanner.layer = GameLayers.UI;
            var prt = pBanner.AddComponent<RectTransform>();
            prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.anchoredPosition = new Vector2(0f, -68f); prt.sizeDelta = new Vector2(560f, 48f);
            var pImg = pBanner.AddComponent<Image>(); pImg.color = ProcTex.Hex("#0B0A08CC"); pImg.raycastTarget = false;
            var pTxt = BuatTeks(pBanner, "Teks", "", font, 26, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(540f, 44f));
            pTxt.color = ProcTex.Hex("#F2C14E"); pTxt.raycastTarget = false;
            pBanner.SetActive(false);
            tc.PasangCrosshair(dotImg, pBanner, pTxt);
            EditorUtility.SetDirty(tc);

            // Toast bawah-tengah.
            var toast = BuatTeks(kanvas.gameObject, "Toast", "", font, 30, new Vector2(0.5f, 0f), new Vector2(0f, 330f), new Vector2(1200f, 140f));
            toast.color = ProcTex.Hex("#F2EFE6");
            var th = toast.gameObject.AddComponent<ToastHUD>();
            th.Pasang(toast);
            EditorUtility.SetDirty(th);

            // Radar taktis kanan-atas (kaca obsidian gelap + bingkai gading-emas elegan, bukan hijau neon).
            var radar = UIGambar(kanvas.gameObject, "RadarGPS", lingkaran, new Vector2(300f, 300f), new Vector2(1f, 1f), new Vector2(-115f, -115f));
            ((RectTransform)radar.transform).localScale = Vector3.one * 0.66f;
            radar.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.08f, 0.76f);
            var ring = UIGambar(radar, "Cincin", cincin, new Vector2(300f, 300f), new Vector2(0.5f, 0.5f), Vector2.zero);
            ring.GetComponent<Image>().color = new Color(0.84f, 0.78f, 0.64f, 0.78f);
            var u = BuatTeks(radar, "Utara", "U", font, 26, new Vector2(0.5f, 0.5f), new Vector2(0f, 134f), new Vector2(40f, 40f));
            u.color = new Color(0.95f, 0.90f, 0.76f, 0.96f);
            var isi = UIStretch(radar, "Isi", Vector2.zero, Vector2.one);
            var andiPanah = UIGambar(isi, "Andi", lingkaran, new Vector2(18f, 18f), new Vector2(0.5f, 0.5f), Vector2.zero);
            andiPanah.GetComponent<Image>().color = Color.white;
            var warna = new[] { ProcTex.Hex("#E06A1B"), ProcTex.Hex("#F2C14E"), ProcTex.Hex("#9E9E9E"), ProcTex.Hex("#9E9E9E") };
            var tanda = new RectTransform[PenandaRadar.Length];
            for (int i = 0; i < PenandaRadar.Length; i++)
            {
                var g = UIGambar(isi, "Penanda" + i, lingkaran, new Vector2(22f, 22f), new Vector2(0.5f, 0.5f), Vector2.zero);
                g.GetComponent<Image>().color = warna[i];
                tanda[i] = (RectTransform)g.transform;
            }
            var rg = radar.AddComponent<RadarGPS>();
            rg.Pasang((RectTransform)u.transform, tanda, PenandaRadar, isi);
            isi.SetActive(false);
            EditorUtility.SetDirty(rg);
            Catat("Interaksi: inventaris, 3 koper, 3 puing, tombol TANGAN & OBOR, toast, radar GPS");
        }

        static void BuatKoper(GameObject induk, string nama, Vector3 xz, ItemID[] isi, int[] n, int lore, Material mat)
        {
            float h = Mathf.Max(0f, WorldTerrainGenerator.SampleHeight(xz.x, xz.z));
            var go = Anak(induk, nama);
            go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
            go.transform.position = new Vector3(xz.x, h, xz.z);
            go.transform.rotation = Quaternion.Euler(0f, (xz.x * 37f + xz.z * 11f) % 360f, 0f);
            var badan = Kotak(go, "Badan", new Vector3(0f, UkuranKoper.y * 0.35f, 0f), new Vector3(UkuranKoper.x, UkuranKoper.y * 0.7f, UkuranKoper.z), mat);
            var engsel = Anak(go, "Engsel");
            engsel.transform.localPosition = new Vector3(0f, UkuranKoper.y * 0.7f, -UkuranKoper.z * 0.5f);
            Kotak(engsel, "Tutup", new Vector3(0f, UkuranKoper.y * 0.15f, UkuranKoper.z * 0.5f), new Vector3(UkuranKoper.x, UkuranKoper.y * 0.3f, UkuranKoper.z), mat);
            var suar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(suar.GetComponent<Collider>());
            suar.transform.SetParent(engsel.transform, false);
            suar.transform.localPosition = new Vector3(0f, UkuranKoper.y * 0.31f, UkuranKoper.z * 0.5f);
            suar.transform.localScale = new Vector3(UkuranKoper.x * 0.85f, 0.02f, 0.08f);
            var suarMat = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_SuarKoper.mat");
            if (suarMat == null)
            {
                suarMat = SafeMaterialFactory.CreatePBR("M_SuarKoper", ProcTex.Hex("#FF7A28"), 0f, 0.3f, null, null, new Color(1.4f, 0.45f, 0.08f));
                AssetDatabase.CreateAsset(suarMat, FolderMat + "/M_SuarKoper.mat");
                suarMat = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_SuarKoper.mat");
            }
            suar.GetComponent<MeshRenderer>().sharedMaterial = suarMat;
            var l = Anak(go, "CahayaSuar").AddComponent<Light>();
            l.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            l.type = LightType.Point; l.range = 4.5f; l.intensity = 1.2f; l.color = ProcTex.Hex("#FF7A28"); l.shadows = LightShadows.None;
            var col = go.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, UkuranKoper.y * 0.5f, 0f); col.size = UkuranKoper;
            Object.DestroyImmediate(badan.GetComponent<Collider>());
            var w = go.AddComponent<WadahItem>();
            w.Pasang("Koper", (ItemID[])isi.Clone(), (int[])n.Clone(), lore, false, engsel.transform);
            EditorUtility.SetDirty(w);
        }

        static void BuatBenda(GameObject induk, string nama, Vector3 xz, Vector3 ukuran, Material mat, string tampil, ItemID id, int n)
        {
            float h = Mathf.Max(0f, WorldTerrainGenerator.SampleHeight(xz.x, xz.z));
            var go = Kotak(induk, nama, new Vector3(xz.x, h + ukuran.y * 0.5f, xz.z), ukuran, mat);
            go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
            var w = go.AddComponent<WadahItem>();
            w.Pasang(tampil, new[] { id }, new[] { n }, 0, true, null);
            EditorUtility.SetDirty(w);
        }

        static GameObject Kotak(GameObject induk, string nama, Vector3 posLokal, Vector3 ukuran, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = nama;
            go.transform.SetParent(induk.transform, false);
            go.transform.localPosition = posLokal; go.transform.localScale = ukuran;
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            go.layer = GameLayers.Interactable;
            return go;
        }
    }
}
