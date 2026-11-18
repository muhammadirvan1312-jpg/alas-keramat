using System.Collections.Generic;
using AlasKeramat.Core;
using AlasKeramat.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.EditorTools
{
    // Langkah 7x (T11): Menu Utama, Jeda (tombol JEDA di atas tengah), Pengaturan lengkap, Cara Bermain (6 kartu),
    // Kredit, 3 Slot Simpan, dan Panel Debug HP (ketuk pojok kiri-atas 5x).
    public static partial class NusantaraMasterBuilder
    {
        static void LangkahT11()
        {
            var kanvas = _ui != null ? _ui.transform.Find("Canvas_HUD") : null;
            if (kanvas == null) { Debug.LogError("[AK][Builder] Canvas_HUD tidak ada, UI T11 dilewati"); return; }
            var font = ProceduralAssetFactory.GetSafeFont();
            Text tx;

            var akar = UIStretch(kanvas.gameObject, "LayarMenuT11", Vector2.zero, Vector2.one);
            var menu = akar.AddComponent<MenuScreens>();

            // Tombol JEDA di atas tengah HUD (09 §2.7) + area ketuk pojok kiri-atas 5x untuk Panel Debug HP (12 §T11).
            var kontrol = kanvas.Find("Kontrol");
            var indukHud = kontrol != null ? kontrol.gameObject : kanvas.gameObject;
            var bJeda = TombolPanel(indukHud, "TombolJeda", "JEDA", font, new Vector2(-48f, -46f), new Vector2(110f, 56f), ProcTex.Hex("#2B2A26"), out tx);
            var rtJeda = bJeda.GetComponent<RectTransform>();
            rtJeda.anchorMin = rtJeda.anchorMax = new Vector2(0.5f, 1f);
            rtJeda.anchoredPosition = new Vector2(-64f, -44f);
            tx.fontSize = 22;
            UnityEventTools.AddPersistentListener(bJeda.onClick, menu.ToggleJeda);

            var bPojokDebug = TombolPanel(indukHud, "AreaKetukDebug", "", font, Vector2.zero, new Vector2(130f, 90f), new Color(0f, 0f, 0f, 0f), out tx);
            var rtDbg = bPojokDebug.GetComponent<RectTransform>();
            rtDbg.anchorMin = rtDbg.anchorMax = new Vector2(0f, 1f);
            rtDbg.pivot = new Vector2(0f, 1f);
            rtDbg.anchoredPosition = Vector2.zero;
            bPojokDebug.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
            UnityEventTools.AddPersistentListener(bPojokDebug.onClick, menu.KetukPojokDebug);

            // 1. Panel Menu Utama (09 §4.2)
            var pUtama = PanelGelap(akar, "PanelMenuUtama", new Vector2(0f, 0f), new Vector2(1f, 1f), new Color(0.04f, 0.05f, 0.04f, 0.92f));
            var jUtama = BuatTeks(pUtama, "Judul", "ALAS KERAMAT", font, 72, new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(1200f, 90f));
            jUtama.color = ProcTex.Hex("#F2C14E");
            var subUtama = BuatTeks(pUtama, "Subjudul", "SONS OF NUSANTARA", font, 32, new Vector2(0.5f, 1f), new Vector2(0f, -155f), new Vector2(900f, 50f));
            subUtama.color = ProcTex.Hex("#BFB8A8");

            var bLanjutUtama = TombolPanel(pUtama, "TombolLanjutUtama", "LANJUTKAN PERMAINAN", font, new Vector2(0f, 680f), new Vector2(520f, 78f), ProcTex.Hex("#2F5D3A"), out tx);
            UnityEventTools.AddPersistentListener(bLanjutUtama.onClick, menu.TutupMenuUtama);
            var bBaruNormal = TombolPanel(pUtama, "TombolBaruNormal", "MULAI BARU (NORMAL)", font, new Vector2(-280f, 580f), new Vector2(360f, 74f), ProcTex.Hex("#6A2A18"), out tx);
            UnityEventTools.AddIntPersistentListener(bBaruNormal.onClick, menu.MulaiPermainanBaru, (int)Difficulty.Normal);
            var bBaruDamai = TombolPanel(pUtama, "TombolBaruDamai", "MULAI (DAMAI)", font, new Vector2(100f, 580f), new Vector2(260f, 74f), ProcTex.Hex("#3A5A40"), out tx);
            UnityEventTools.AddIntPersistentListener(bBaruDamai.onClick, menu.MulaiPermainanBaru, (int)Difficulty.Damai);
            var bBaruSulit = TombolPanel(pUtama, "TombolBaruSulit", "MULAI (SULIT)", font, new Vector2(380f, 580f), new Vector2(260f, 74f), ProcTex.Hex("#7A1F1F"), out tx);
            UnityEventTools.AddIntPersistentListener(bBaruSulit.onClick, menu.MulaiPermainanBaru, (int)Difficulty.Sulit);
            var bSlotUtama = TombolPanel(pUtama, "TombolSlotUtama", "SLOT SIMPAN (1-3)", font, new Vector2(0f, 480f), new Vector2(520f, 74f), ProcTex.Hex("#3D405B"), out tx);
            UnityEventTools.AddPersistentListener(bSlotUtama.onClick, menu.BukaPanelSlot);
            var bAturUtama = TombolPanel(pUtama, "TombolPengaturanUtama", "PENGATURAN", font, new Vector2(0f, 390f), new Vector2(520f, 74f), ProcTex.Hex("#4A4E69"), out tx);
            UnityEventTools.AddPersistentListener(bAturUtama.onClick, menu.BukaPengaturan);
            var bCaraUtama = TombolPanel(pUtama, "TombolCaraUtama", "CARA BERMAIN", font, new Vector2(-140f, 300f), new Vector2(250f, 70f), ProcTex.Hex("#3B4A3E"), out tx);
            UnityEventTools.AddPersistentListener(bCaraUtama.onClick, menu.BukaCaraBermain);
            var bKreditUtama = TombolPanel(pUtama, "TombolKreditUtama", "KREDIT", font, new Vector2(140f, 300f), new Vector2(250f, 70f), ProcTex.Hex("#3B4A3E"), out tx);
            UnityEventTools.AddPersistentListener(bKreditUtama.onClick, menu.BukaKredit);
            var bKeluar = TombolPanel(pUtama, "TombolKeluarAplikasi", "KELUAR", font, new Vector2(0f, 200f), new Vector2(360f, 70f), ProcTex.Hex("#5A1E1E"), out tx);
            UnityEventTools.AddPersistentListener(bKeluar.onClick, menu.KeluarAplikasi);
            pUtama.SetActive(false);

            // 2. Panel Jeda (09 §4.3)
            var pJeda = PanelGelap(akar, "PanelJeda", new Vector2(0.25f, 0.12f), new Vector2(0.75f, 0.88f), new Color(0.06f, 0.06f, 0.05f, 0.94f));
            var jJeda = BuatTeks(pJeda, "Judul", "PERMAINAN DIJEDA", font, 46, new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(800f, 65f));
            jJeda.color = ProcTex.Hex("#F2C14E");
            var bLanjut = TombolPanel(pJeda, "TombolLanjutkan", "LANJUTKAN", font, new Vector2(0f, 580f), new Vector2(460f, 76f), ProcTex.Hex("#2F5D3A"), out tx);
            UnityEventTools.AddPersistentListener(bLanjut.onClick, menu.LanjutkanDariJeda);
            var bSimpanCepat = TombolPanel(pJeda, "TombolSlotSimpan", "SIMPAN / MUAT SLOT (1-3)", font, new Vector2(0f, 485f), new Vector2(460f, 74f), ProcTex.Hex("#3D405B"), out tx);
            UnityEventTools.AddPersistentListener(bSimpanCepat.onClick, menu.BukaPanelSlot);
            var bAtur = TombolPanel(pJeda, "TombolPengaturan", "PENGATURAN", font, new Vector2(0f, 390f), new Vector2(460f, 74f), ProcTex.Hex("#4A4E69"), out tx);
            UnityEventTools.AddPersistentListener(bAtur.onClick, menu.BukaPengaturan);
            var bCara = TombolPanel(pJeda, "TombolCara", "CARA BERMAIN", font, new Vector2(0f, 295f), new Vector2(460f, 74f), ProcTex.Hex("#3B4A3E"), out tx);
            UnityEventTools.AddPersistentListener(bCara.onClick, menu.BukaCaraBermain);
            var bKredit = TombolPanel(pJeda, "TombolKredit", "KREDIT", font, new Vector2(0f, 200f), new Vector2(460f, 70f), ProcTex.Hex("#3B4A3E"), out tx);
            UnityEventTools.AddPersistentListener(bKredit.onClick, menu.BukaKredit);
            var bKeMenu = TombolPanel(pJeda, "TombolKeMenuUtama", "MENU UTAMA", font, new Vector2(0f, 105f), new Vector2(460f, 74f), ProcTex.Hex("#7A1F1F"), out tx);
            UnityEventTools.AddPersistentListener(bKeMenu.onClick, menu.BukaMenuUtama);
            pJeda.SetActive(false);

            // 3. Panel Pengaturan (09 §4.4, 11 §7.2)
            var pAtur = PanelGelap(akar, "PanelPengaturan", new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.94f), new Color(0.05f, 0.06f, 0.06f, 0.95f));
            var jAtur = BuatTeks(pAtur, "Judul", "PENGATURAN PERMAINAN & GRAFIS", font, 42, new Vector2(0.5f, 1f), new Vector2(0f, -45f), new Vector2(1200f, 60f));
            jAtur.color = ProcTex.Hex("#F2C14E");
            var infoAtur = BuatTeks(pAtur, "InfoPengaturan", "", font, 25, new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(1420f, 150f));
            infoAtur.color = ProcTex.Hex("#F2EFE6"); infoAtur.lineSpacing = 1.25f;

            var bPreset = TombolPanel(pAtur, "TombolPreset", "PRESET GRAFIS", font, new Vector2(-440f, 540f), new Vector2(360f, 70f), ProcTex.Hex("#2F5D3A"), out tx);
            UnityEventTools.AddPersistentListener(bPreset.onClick, menu.SiklusPreset);
            var bFps = TombolPanel(pAtur, "TombolFps", "TARGET FPS (30/60)", font, new Vector2(0f, 540f), new Vector2(360f, 70f), ProcTex.Hex("#3D405B"), out tx);
            UnityEventTools.AddPersistentListener(bFps.onClick, menu.ToggleTargetFps);
            var bFov = TombolPanel(pAtur, "TombolFov", "FOV KAMERA", font, new Vector2(440f, 540f), new Vector2(360f, 70f), ProcTex.Hex("#3D405B"), out tx);
            UnityEventTools.AddPersistentListener(bFov.onClick, menu.SiklusFov);

            var bSens = TombolPanel(pAtur, "TombolSens", "SENSITIVITAS LIHAT", font, new Vector2(-440f, 440f), new Vector2(360f, 70f), ProcTex.Hex("#4A4E69"), out tx);
            UnityEventTools.AddPersistentListener(bSens.onClick, menu.SiklusSensitivitas);
            var bBalikY = TombolPanel(pAtur, "TombolBalikY", "BALIK SUMBU Y", font, new Vector2(0f, 440f), new Vector2(360f, 70f), ProcTex.Hex("#4A4E69"), out tx);
            UnityEventTools.AddPersistentListener(bBalikY.onClick, menu.ToggleBalikY);
            var bSkala = TombolPanel(pAtur, "TombolSkala", "UKURAN TOMBOL", font, new Vector2(440f, 440f), new Vector2(360f, 70f), ProcTex.Hex("#4A4E69"), out tx);
            UnityEventTools.AddPersistentListener(bSkala.onClick, menu.SiklusSkalaTombol);

            var bVol = TombolPanel(pAtur, "TombolVolume", "VOLUME MASTER", font, new Vector2(-440f, 340f), new Vector2(360f, 70f), ProcTex.Hex("#3B4A3E"), out tx);
            UnityEventTools.AddPersistentListener(bVol.onClick, menu.SiklusVolume);
            var bHeadset = TombolPanel(pAtur, "TombolHeadset", "MODE HEADSET 3D", font, new Vector2(0f, 340f), new Vector2(360f, 70f), ProcTex.Hex("#3B4A3E"), out tx);
            UnityEventTools.AddPersistentListener(bHeadset.onClick, menu.ToggleHeadset);
            var bGore = TombolPanel(pAtur, "TombolGore", "TINGKAT GORE", font, new Vector2(440f, 340f), new Vector2(360f, 70f), ProcTex.Hex("#6A2A18"), out tx);
            UnityEventTools.AddPersistentListener(bGore.onClick, menu.SiklusGore);

            var bHoror = TombolPanel(pAtur, "TombolHoror", "INTENSITAS HOROR", font, new Vector2(-440f, 240f), new Vector2(360f, 70f), ProcTex.Hex("#6A2A18"), out tx);
            UnityEventTools.AddPersistentListener(bHoror.onClick, menu.SiklusHoror);
            var bSulit = TombolPanel(pAtur, "TombolKesulitan", "TINGKAT KESULITAN", font, new Vector2(0f, 240f), new Vector2(360f, 70f), ProcTex.Hex("#6A2A18"), out tx);
            UnityEventTools.AddPersistentListener(bSulit.onClick, menu.SiklusKesulitan);
            var bGetar = TombolPanel(pAtur, "TombolGetar", "GETAR HAPTIK", font, new Vector2(440f, 240f), new Vector2(360f, 70f), ProcTex.Hex("#3B4A3E"), out tx);
            UnityEventTools.AddPersistentListener(bGetar.onClick, menu.ToggleGetar);

            var bImersif = TombolPanel(pAtur, "TombolImersif", "MODE IMERSIF HUD", font, new Vector2(-220f, 110f), new Vector2(360f, 74f), ProcTex.Hex("#3D405B"), out tx);
            UnityEventTools.AddPersistentListener(bImersif.onClick, menu.ToggleImersif);
            var bTutupAtur = TombolPanel(pAtur, "TombolTutupPengaturan", "SIMPAN & KEMBALI", font, new Vector2(220f, 110f), new Vector2(380f, 74f), ProcTex.Hex("#2F5D3A"), out tx);
            UnityEventTools.AddPersistentListener(bTutupAtur.onClick, menu.TutupPengaturan);
            pAtur.SetActive(false);

            // 4. Panel Cara Bermain (09 §4.11 - 6 kartu)
            var pCara = PanelGelap(akar, "PanelCaraBermain", new Vector2(0.15f, 0.14f), new Vector2(0.85f, 0.86f), new Color(0.06f, 0.05f, 0.04f, 0.95f));
            var jCara = BuatTeks(pCara, "JudulCara", MenuScreens.JudulCara[0], font, 42, new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(1100f, 65f));
            jCara.color = ProcTex.Hex("#F2C14E");
            var isiCara = BuatTeks(pCara, "IsiCara", MenuScreens.IsiCara[0], font, 32, new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(1150f, 340f));
            isiCara.color = ProcTex.Hex("#F2EFE6"); isiCara.horizontalOverflow = HorizontalWrapMode.Wrap; isiCara.lineSpacing = 1.3f;
            var bSebelum = TombolPanel(pCara, "TombolSebelum", "SEBELUMNYA", font, new Vector2(-340f, 95f), new Vector2(280f, 74f), ProcTex.Hex("#3D405B"), out tx);
            UnityEventTools.AddPersistentListener(bSebelum.onClick, menu.KartuCaraSebelum);
            var bBerikut = TombolPanel(pCara, "TombolBerikut", "BERIKUTNYA", font, new Vector2(0f, 95f), new Vector2(280f, 74f), ProcTex.Hex("#2F5D3A"), out tx);
            UnityEventTools.AddPersistentListener(bBerikut.onClick, menu.KartuCaraBerikut);
            var bTutupCara = TombolPanel(pCara, "TombolTutupCara", "TUTUP", font, new Vector2(340f, 95f), new Vector2(240f, 74f), ProcTex.Hex("#7A1F1F"), out tx);
            UnityEventTools.AddPersistentListener(bTutupCara.onClick, menu.TutupCaraBermain);
            pCara.SetActive(false);

            // 5. Panel Kredit
            var pKredit = PanelGelap(akar, "PanelKredit", new Vector2(0.18f, 0.12f), new Vector2(0.82f, 0.88f), new Color(0.04f, 0.04f, 0.05f, 0.95f));
            var isiKredit = BuatTeks(pKredit, "IsiKredit", MenuScreens.TeksKredit, font, 28, new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(1050f, 520f));
            isiKredit.color = ProcTex.Hex("#F2EFE6"); isiKredit.lineSpacing = 1.25f;
            var bTutupKredit = TombolPanel(pKredit, "TombolTutupKredit", "TUTUP", font, new Vector2(0f, 90f), new Vector2(300f, 76f), ProcTex.Hex("#2F5D3A"), out tx);
            UnityEventTools.AddPersistentListener(bTutupKredit.onClick, menu.TutupKredit);
            pKredit.SetActive(false);

            // 6. Panel Slot Simpan 1-3 (11 §8)
            var pSlot = PanelGelap(akar, "PanelSlotSimpan", new Vector2(0.16f, 0.14f), new Vector2(0.84f, 0.86f), new Color(0.05f, 0.06f, 0.05f, 0.95f));
            var jSlot = BuatTeks(pSlot, "Judul", "SLOT SIMPAN PERMAINAN (1-3)", font, 40, new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(1000f, 60f));
            jSlot.color = ProcTex.Hex("#F2C14E");
            var infoSlot = BuatTeks(pSlot, "InfoSlot", "", font, 28, new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(1100f, 240f));
            infoSlot.color = ProcTex.Hex("#F2EFE6"); infoSlot.lineSpacing = 1.3f;
            for (int s = 1; s <= 3; s++)
            {
                var bs = TombolPanel(pSlot, "TombolPilihSlot" + s, "PILIH SLOT " + s, font, new Vector2((s - 2) * 340f, 260f), new Vector2(300f, 74f), ProcTex.Hex("#3D405B"), out tx);
                UnityEventTools.AddIntPersistentListener(bs.onClick, menu.PilihSlot, s);
            }
            var bSimpanSlot = TombolPanel(pSlot, "TombolSimpanSlot", "SIMPAN SEKARANG", font, new Vector2(-360f, 155f), new Vector2(310f, 76f), ProcTex.Hex("#2F5D3A"), out tx);
            UnityEventTools.AddPersistentListener(bSimpanSlot.onClick, menu.SimpanKeSlotAktif);
            var bMuatSlot = TombolPanel(pSlot, "TombolMuatSlot", "MUAT SLOT", font, new Vector2(0f, 155f), new Vector2(280f, 76f), ProcTex.Hex("#4A6F8A"), out tx);
            UnityEventTools.AddPersistentListener(bMuatSlot.onClick, menu.MuatDariSlotAktif);
            var bHapusSlot = TombolPanel(pSlot, "TombolHapusSlot", "HAPUS SLOT", font, new Vector2(340f, 155f), new Vector2(280f, 76f), ProcTex.Hex("#7A1F1F"), out tx);
            UnityEventTools.AddPersistentListener(bHapusSlot.onClick, menu.HapusSlotAktif);
            var bTutupSlot = TombolPanel(pSlot, "TombolTutupSlot", "KEMBALI", font, new Vector2(0f, 65f), new Vector2(280f, 68f), ProcTex.Hex("#333333"), out tx);
            UnityEventTools.AddPersistentListener(bTutupSlot.onClick, menu.TutupPanelSlot);
            pSlot.SetActive(false);

            // 7. Panel Debug HP (ketuk pojok kiri-atas 5x)
            var pDebug = PanelGelap(akar, "PanelDebugHP", new Vector2(0.04f, 0.25f), new Vector2(0.56f, 0.88f), new Color(0.02f, 0.05f, 0.03f, 0.9f));
            var infoDebug = BuatTeks(pDebug, "TeksDebug", "FPS: 30", font, 22, new Vector2(0.5f, 0.72f), Vector2.zero, new Vector2(920f, 220f));
            infoDebug.alignment = TextAnchor.UpperLeft; infoDebug.color = ProcTex.Hex("#3CFF7A");
            var bDbgVital = TombolPanel(pDebug, "DebugVital", "ISI VITAL", font, new Vector2(-300f, 190f), new Vector2(260f, 64f), ProcTex.Hex("#2F5D3A"), out tx);
            UnityEventTools.AddPersistentListener(bDbgVital.onClick, menu.DebugIsiVital);
            var bDbgJam = TombolPanel(pDebug, "DebugJam", "+3 JAM", font, new Vector2(0f, 190f), new Vector2(260f, 64f), ProcTex.Hex("#3D405B"), out tx);
            UnityEventTools.AddPersistentListener(bDbgJam.onClick, menu.DebugMajuJam);
            var bDbgItem = TombolPanel(pDebug, "DebugItem", "BERI ITEM", font, new Vector2(300f, 190f), new Vector2(260f, 64f), ProcTex.Hex("#6A2A18"), out tx);
            UnityEventTools.AddPersistentListener(bDbgItem.onClick, menu.DebugBeriItem);
            var bDbgBeringin = TombolPanel(pDebug, "DebugBeringin", "TP BERINGIN", font, new Vector2(-300f, 105f), new Vector2(260f, 64f), ProcTex.Hex("#4A4E69"), out tx);
            UnityEventTools.AddPersistentListener(bDbgBeringin.onClick, menu.DebugTeleportBeringin);
            var bDbgDesa = TombolPanel(pDebug, "DebugDesa", "TP DESA", font, new Vector2(0f, 105f), new Vector2(260f, 64f), ProcTex.Hex("#4A4E69"), out tx);
            UnityEventTools.AddPersistentListener(bDbgDesa.onClick, menu.DebugTeleportDesa);
            var bDbgCuaca = TombolPanel(pDebug, "DebugCuaca", "CUACA BADAI", font, new Vector2(300f, 105f), new Vector2(260f, 64f), ProcTex.Hex("#4A4E69"), out tx);
            UnityEventTools.AddPersistentListener(bDbgCuaca.onClick, menu.DebugCuacaBadai);
            var bDbgTutup = TombolPanel(pDebug, "DebugTutup", "TUTUP DEBUG", font, new Vector2(0f, 32f), new Vector2(260f, 56f), ProcTex.Hex("#7A1F1F"), out tx);
            UnityEventTools.AddPersistentListener(bDbgTutup.onClick, menu.TutupDebug);
            pDebug.SetActive(false);

            // Kumpulkan tombol aksi HUD agar pengaturan Ukuran Tombol (80-130%) bekerja langsung.
            var daftarSkala = new List<RectTransform>();
            if (kontrol != null)
            {
                foreach (var b in kontrol.GetComponentsInChildren<Button>(true))
                {
                    if (b.name == "AreaKetukDebug" || b.name == "TombolJeda") continue;
                    var rt = b.GetComponent<RectTransform>();
                    if (rt != null) daftarSkala.Add(rt);
                }
            }

            menu.Pasang(pUtama, pJeda, pAtur, pCara, pKredit, pSlot, pDebug,
                infoAtur, jCara, isiCara, isiKredit, infoSlot, infoDebug, daftarSkala.ToArray());
            EditorUtility.SetDirty(menu);
            akar.transform.SetAsLastSibling();
            var peringatan = kanvas.Find("LayarPeringatan");
            if (peringatan != null) peringatan.SetAsLastSibling();
            Catat("T11: MenuScreens (Menu Utama, Jeda, Pengaturan, Cara Bermain 6 kartu, Kredit, 3 Slot Simpan, Panel Debug HP)");
        }

        static GameObject PanelGelap(GameObject induk, string nama, Vector2 min, Vector2 maks, Color warna)
        {
            var go = UIStretch(induk, nama, min, maks);
            go.AddComponent<Image>().color = warna;
            return go;
        }
    }
}
