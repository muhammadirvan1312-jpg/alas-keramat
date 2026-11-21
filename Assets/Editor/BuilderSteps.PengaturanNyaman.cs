using AlasKeramat.Core;
using AlasKeramat.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AlasKeramat.EditorTools
{
    public static partial class NusantaraMasterBuilder
    {
        static void BangunPengaturanNyaman(GameObject panel, MenuScreens menu, Font font, out Text info)
        {
            BuatTeks(panel, "Judul", "PENGATURAN", font, 42, new Vector2(0.5f, 1f), new Vector2(0, -45), new Vector2(1200, 60));
            info = BuatTeks(panel, "InfoPengaturan", "", font, 22, new Vector2(0.5f, 0), new Vector2(0, 78), new Vector2(1400, 120));
            info.gameObject.SetActive(false);
            string[] tab = { "KONTROL & KAMERA", "GRAFIS", "AUDIO", "PERMAINAN" };
            var halaman = new GameObject[4]; var slider = new Slider[7]; var nilai = new Text[7]; var pilihan = new Text[15];
            for (int i = 0; i < tab.Length; i++)
            {
                Text label;
                var b = TombolPanel(panel, "Tab" + i, tab[i], font, Vector2.zero, new Vector2(345, 66), ProcTex.Hex("#3D405B"), out label);
                var r = (RectTransform)b.transform; r.anchorMin = r.anchorMax = new Vector2(0.5f, 1f); r.anchoredPosition = new Vector2(-555 + i * 370, -125);
                label.fontSize = 24;
                UnityEventTools.AddIntPersistentListener(b.onClick, menu.BukaTabPengaturan, i);
                halaman[i] = UIStretch(panel, "HalamanAtur" + i, new Vector2(0.04f, 0.13f), new Vector2(0.96f, 0.77f));
            }
            string[] namaGeser = { "SENSITIVITAS LIHAT", "SUDUT PANDANG (FOV)", "UKURAN TOMBOL", "VOLUME UTAMA", "EFEK & LANGKAH", "ALAM & HUJAN", "MUSIK" };
            UnityAction<float>[] aksi = { menu.AturSensitivitas, menu.AturFov, menu.AturUkuranTombol, menu.AturMaster, menu.AturEfek, menu.AturAmbien, menu.AturMusik };
            for (int i = 0; i < slider.Length; i++)
            {
                bool audio = i >= 3; float x = audio ? -360 + ((i - 3) % 2) * 720 : -360;
                float y = audio ? -70 - ((i - 3) / 2) * 240 : -60 - i * 180;
                float min = i == 0 ? 0.5f : i == 1 ? 60f : i == 2 ? 0.8f : 0f;
                float max = i == 0 ? 2f : i == 1 ? 85f : i == 2 ? 1.3f : 1f;
                slider[i] = PenggeserNyaman(halaman[audio ? 2 : 0], "Penggeser" + i, namaGeser[i], font, new Vector2(x, y), min, max, out nilai[i]);
                UnityEventTools.AddPersistentListener(slider[i].onValueChanged, aksi[i]);
            }
            UnityAction[] tindakan = { menu.ToggleBalikY, menu.ToggleHeadBob, menu.ToggleGoyangan, menu.ToggleBantuanBidik, menu.ToggleGetar, menu.ToggleJedaPanel,
                menu.SiklusPreset, menu.ToggleTargetFps, menu.ToggleHeadset, menu.SiklusGore, menu.SiklusHoror, menu.SiklusKesulitan, menu.ToggleImersif,
                menu.ToggleModeSengsara, menu.ToggleModePantangMundur };
            for (int i = 0; i < tindakan.Length; i++)
            {
                int page = i < 6 ? 0 : i < 8 ? 1 : i == 8 ? 2 : 3;
                float x = page == 0 ? 360 : 0;
                float y = page == 0 ? -40 - i * 92 : page == 1 ? -80 - (i - 6) * 150 : page == 2 ? -530 : -40 - (i - 9) * 92;
                var b = TombolPanel(halaman[page], "PilihanNyaman" + i, "", font, Vector2.zero, new Vector2(630, 72), ProcTex.Hex("#3B4A3E"), out pilihan[i]);
                var r = (RectTransform)b.transform; r.anchorMin = r.anchorMax = new Vector2(0.5f, 1); r.anchoredPosition = new Vector2(x, y);
                pilihan[i].fontSize = 24; UnityEventTools.AddPersistentListener(b.onClick, tindakan[i]);
            }
            var petunjuk = BuatTeks(halaman[1], "PetunjukPerforma", "Mulai dari SEDANG / 30 FPS.\nJika gerak tersendat, pilih RENDAH.\n60 FPS memerlukan perangkat yang mampu.\nGunakan laporan HP di menu Jeda untuk mengukur.", font, 28, new Vector2(0.5f, 1), new Vector2(0, -430), new Vector2(1200, 220));
            petunjuk.raycastTarget = false;
            Text tutup;
            var kembali = TombolPanel(panel, "TombolTutupPengaturan", "SIMPAN & KEMBALI", font, new Vector2(0, 55), new Vector2(600, 76), ProcTex.Hex("#2F5D3A"), out tutup);
            UnityEventTools.AddPersistentListener(kembali.onClick, menu.TutupPengaturan);
            menu.PasangPengaturanNyaman(halaman, slider, nilai, pilihan); menu.BukaTabPengaturan(0); EditorUtility.SetDirty(menu);
        }

        static Slider PenggeserNyaman(GameObject induk, string nama, string label, Font font, Vector2 pos, float min, float max, out Text nilai)
        {
            var go = UIStretch(induk, nama, new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            var rt = (RectTransform)go.transform; rt.sizeDelta = new Vector2(600, 100); rt.anchoredPosition = pos;
            var judul = BuatTeks(go, "Judul", label, font, 26, new Vector2(0.5f, 1), new Vector2(-85, 15), new Vector2(420, 44));
            judul.alignment = TextAnchor.MiddleLeft; judul.raycastTarget = false;
            nilai = BuatTeks(go, "Nilai", "", font, 26, new Vector2(1, 1), new Vector2(-65, 15), new Vector2(170, 44)); nilai.raycastTarget = false;
            // Latar transparen sebagai area sentuh; rel bulat tipis, isian emas, kenop bulat.
            var fondasi = UIGambar(go, "Latar", null, new Vector2(600, 64), new Vector2(0.5f, 0.5f), new Vector2(0, -25));
            fondasi.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.001f);
            var rel = UIGambar(fondasi, "Rel", null, new Vector2(560, 16), new Vector2(0.5f, 0.5f), Vector2.zero);
            GayakanTombol(rel.GetComponent<Image>(), new Color(0.12f, 0.14f, 0.13f), null); rel.GetComponent<Image>().raycastTarget = false;
            var areaIsi = UIStretch(fondasi, "AreaIsi", new Vector2(0.05f, 0.5f), new Vector2(0.95f, 0.5f));
            ((RectTransform)areaIsi.transform).sizeDelta = new Vector2(0f, 16f);
            var isi = UIStretch(areaIsi, "Isi", new Vector2(0f, 0f), new Vector2(0f, 1f));
            var gIsi = isi.AddComponent<Image>(); GayakanTombol(gIsi, ProcTex.Hex("#E0B33A"), null); gIsi.color = ProcTex.Hex("#E0B33A"); gIsi.raycastTarget = false;
            var area = UIStretch(fondasi, "AreaKenop", new Vector2(0.05f, 0), new Vector2(0.95f, 1));
            var kenop = UIGambar(area, "Kenop", null, new Vector2(58, 58), new Vector2(0.5f, 0.5f), Vector2.zero);
            var gk = kenop.GetComponent<Image>(); var sk = SpriteLingkaran(); if (sk != null) gk.sprite = sk;
            gk.color = ProcTex.Hex("#F2CE5A");
            var s = fondasi.AddComponent<Slider>(); s.minValue = min; s.maxValue = max; s.wholeNumbers = max > 10;
            s.targetGraphic = gk; s.handleRect = (RectTransform)kenop.transform; s.fillRect = (RectTransform)isi.transform;
            var nav = s.navigation; nav.mode = Navigation.Mode.None; s.navigation = nav;
            return s;
        }
    }
}
