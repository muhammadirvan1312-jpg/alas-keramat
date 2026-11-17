using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using J = AlasKeramat.Core.ResepKarakter.JenisKarakter;

namespace AlasKeramat.EditorTools
{
    // Langkah 7r (T8): Kang Deden & Ningsih jadi companion (CharacterController + AI + titik interaksi), Buku Catatan 15 perintah, menu Ningsih.
    public static partial class NusantaraMasterBuilder
    {
        static void LangkahCompanion()
        {
            var deden = GameObject.Find("KangDeden");
            var ningsih = GameObject.Find("Ningsih");
            var kayu = AmbilMat("M_Kayu");
            if (deden != null)
            {
                Kapsul(deden, ResepKarakter.Ambil(J.Deden).tinggi);
                var ai = deden.AddComponent<KangDedenKelvinAI>();
                ai.Pasang(kayu);
                SetLayerSatu(deden, GameLayers.Companion);
                EditorUtility.SetDirty(ai);
            }
            else Debug.LogError("[AK][Builder] KangDeden tidak ditemukan (langkah 7g)");
            if (ningsih != null)
            {
                Kapsul(ningsih, ResepKarakter.Ambil(J.Ningsih).tinggi);
                ningsih.AddComponent<NingsihVirginiaAI>();
                SetLayerSatu(ningsih, GameLayers.Companion);
                var t = Anak(ningsih, "InteraksiNingsih");
                t.layer = GameLayers.Interactable;
                var c = t.AddComponent<CapsuleCollider>(); c.isTrigger = true; c.center = Vector3.up * 0.8f; c.height = 1.6f; c.radius = 0.5f;
            }
            else Debug.LogError("[AK][Builder] Ningsih tidak ditemukan (langkah 7g)");

            var kanvas = _ui != null ? _ui.transform.Find("Canvas_HUD") : null;
            if (kanvas == null) { Debug.LogError("[AK][Builder] Canvas_HUD tidak ada, UI companion dilewati"); return; }
            var font = ProceduralAssetFactory.GetSafeFont();
            Text tx;

            var akarBuku = Anak(kanvas.gameObject, "BukuCatatanUI");
            var buku = akarBuku.AddComponent<BukuCatatanUI>();
            var pb = UIStretch(akarBuku, "Panel", new Vector2(0.12f, 0.08f), new Vector2(0.88f, 0.92f));
            pb.AddComponent<Image>().color = ProcTex.Hex("#E9DFC4F5");
            var judul = BuatTeks(pb, "Judul", "BUKU CATATAN  -  Perintah untuk Kang Deden", font, 34, new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(1200f, 60f));
            judul.color = ProcTex.Hex("#3B2A1A"); judul.alignment = TextAnchor.MiddleCenter;
            string[] halaman = { "Umum", "Kumpulkan", "Kemah" };
            for (int h = 0; h < 3; h++)
            {
                var th = BuatTeks(pb, "Halaman" + h, halaman[h].ToUpperInvariant(), font, 28, new Vector2(0.18f + h * 0.32f, 1f), new Vector2(0f, -120f), new Vector2(400f, 50f));
                th.color = ProcTex.Hex("#7A3B1C"); th.alignment = TextAnchor.MiddleCenter;
            }
            var tombolBuku = new Button[KangDedenKelvinAI.Daftar.Length];
            var baris = new int[3];
            for (int i = 0; i < tombolBuku.Length; i++)
            {
                var d = KangDedenKelvinAI.Daftar[i];
                int h = d.halaman == "Umum" ? 0 : d.halaman == "Kumpulkan" ? 1 : 2;
                var b = TombolPanel(pb, "Perintah_" + d.id, d.teks, font, Vector2.zero, new Vector2(400f, 74f), ProcTex.Hex("#5C4529"), out tx);
                var rt = b.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.18f + h * 0.32f, 1f);
                rt.anchoredPosition = new Vector2(0f, -190f - baris[h] * 88f);
                baris[h]++;
                tx.fontSize = 24;
                UnityEventTools.AddIntPersistentListener(b.onClick, buku.Pilih, i);
                tombolBuku[i] = b;
            }
            var tutup = TombolPanel(pb, "TombolTutup", "TUTUP", font, new Vector2(0f, 60f), new Vector2(260f, 80f), ProcTex.Hex("#7A1F1F"), out tx);
            UnityEventTools.AddPersistentListener(tutup.onClick, buku.Tutup);
            buku.Pasang(pb, tombolBuku);
            EditorUtility.SetDirty(buku);

            var akarMenu = Anak(kanvas.gameObject, "MenuNingsihUI");
            var menu = akarMenu.AddComponent<MenuNingsihUI>();
            var pn = UIStretch(akarMenu, "Panel", new Vector2(0.3f, 0.12f), new Vector2(0.7f, 0.88f));
            pn.AddComponent<Image>().color = ProcTex.Hex("#1E1418F0");
            var jn = BuatTeks(pn, "Judul", "NINGSIH", font, 32, new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(700f, 60f));
            jn.color = ProcTex.Hex("#F2EFE6"); jn.alignment = TextAnchor.MiddleCenter;
            var tombolMenu = new Button[MenuNingsihUI.Label.Length];
            for (int i = 0; i < tombolMenu.Length; i++)
            {
                var b = TombolPanel(pn, "Aksi_" + i, MenuNingsihUI.Label[i], font, Vector2.zero, new Vector2(560f, 74f), ProcTex.Hex("#5A2430"), out tx);
                var rt = b.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(0f, -140f - i * 88f);
                tx.fontSize = 24;
                UnityEventTools.AddIntPersistentListener(b.onClick, menu.Pilih, i);
                tombolMenu[i] = b;
            }
            var tutupN = TombolPanel(pn, "TombolTutup", "TUTUP", font, new Vector2(0f, 60f), new Vector2(260f, 80f), ProcTex.Hex("#7A1F1F"), out tx);
            UnityEventTools.AddPersistentListener(tutupN.onClick, menu.Tutup);
            menu.Pasang(pn, tombolMenu, jn);
            EditorUtility.SetDirty(menu);
            Catat("Companion: Kang Deden (15 perintah) & Ningsih (Trust 3 tahap), Buku Catatan & menu Ningsih");
        }

        static void Kapsul(GameObject go, float tinggi)
        {
            var cc = go.GetComponent<CharacterController>();
            if (cc == null) cc = go.AddComponent<CharacterController>();
            cc.height = tinggi; cc.radius = 0.32f; cc.center = new Vector3(0f, tinggi * 0.5f, 0f); cc.slopeLimit = 45f; cc.stepOffset = 0.4f;
        }

        static void SetLayerSatu(GameObject go, int layer) { go.layer = layer; }
    }
}
