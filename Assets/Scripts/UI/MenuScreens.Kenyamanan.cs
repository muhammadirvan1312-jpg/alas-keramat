using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    public partial class MenuScreens
    {
        public const string PrefJedaPanel = "ak_jeda_panel", PrefGoyangan = "ak_goyangan_tempur";
        public bool Goyangan { get; private set; }
        [SerializeField] GameObject[] halamanAtur = new GameObject[0];
        [SerializeField] Slider[] penggeserAtur = new Slider[0];
        [SerializeField] Text[] nilaiAtur = new Text[0];
        [SerializeField] Text[] pilihanNyaman = new Text[0];

        public void PasangPengaturanNyaman(GameObject[] halaman, Slider[] geser, Text[] nilai, Text[] pilihan)
        { halamanAtur = halaman; penggeserAtur = geser; nilaiAtur = nilai; pilihanNyaman = pilihan; }

        public void BukaTabPengaturan(int indeks)
        {
            for (int i = 0; i < halamanAtur.Length; i++) if (halamanAtur[i] != null) halamanAtur[i].SetActive(i == indeks);
            SegarkanPenggeser();
        }

        void SegarkanPenggeser()
        {
            float[] nilai = { Sensitivitas, Fov, SkalaTombol, VolMaster, VolEfek, VolAmbien, VolMusik };
            for (int i = 0; i < nilai.Length; i++)
            {
                if (i < penggeserAtur.Length && penggeserAtur[i] != null) penggeserAtur[i].SetValueWithoutNotify(nilai[i]);
                if (i < nilaiAtur.Length && nilaiAtur[i] != null) nilaiAtur[i].text = i == 1 ? nilai[i].ToString("0") + " derajat" : (nilai[i] * 100f).ToString("0") + "%";
            }
            string[] label = {
                "BALIK Y: " + (BalikY ? "YA" : "TIDAK"),
                "GOYANGAN JALAN: " + (HeadBob ? "AKTIF" : "MATI"),
                "GOYANGAN TEMPUR: " + (Goyangan ? "AKTIF" : "MATI"),
                "BANTUAN BIDIK: " + (CombatAndLeftHandController.AimAssist ? "AKTIF" : "MATI"),
                "GETAR: " + (Getar ? "AKTIF" : "MATI"),
                "JEDA SAAT BUKA TAS/BUKU: " + (PengunciInput.JedaPanel ? "YA" : "TIDAK"),
                "GRAFIS: " + Preset, "TARGET: " + TargetFps + " FPS",
                "HEADSET: " + (TerrorDirector.Headset ? "AKTIF" : "MATI"),
                "GORE: " + GoreSystem.Tingkat, "HOROR: " + TerrorDirector.Setelan,
                "KESULITAN: " + Kesulitan, "SEMBUNYIKAN VITAL: " + (ModeImersif ? "YA" : "TIDAK")
            };
            for (int i = 0; i < pilihanNyaman.Length && i < label.Length; i++) if (pilihanNyaman[i] != null) pilihanNyaman[i].text = label[i];
        }

        // Penggeser diterapkan langsung; persistensi dilakukan saat menutup menu, bukan menulis disk tiap drag.
        public void AturSensitivitas(float nilai) { Sensitivitas = Mathf.Clamp(nilai, 0.5f, 2f); var a = GameServices.Get<AndiFPSController>(); if (a != null) a.PengaliSensitivitas = Sensitivitas; SegarkanPenggeser(); }
        public void AturFov(float nilai) { Fov = Mathf.Clamp(nilai, 60f, 85f); var c = Camera.main; if (c != null) c.fieldOfView = Fov; SegarkanPenggeser(); }
        public void AturUkuranTombol(float nilai)
        {
            SkalaTombol = Mathf.Clamp(nilai, 0.8f, 1.3f);
            if (tombolSkala != null && _ukuranAwalTombol != null)
                for (int i = 0; i < tombolSkala.Length && i < _ukuranAwalTombol.Length; i++)
                    if (tombolSkala[i] != null) tombolSkala[i].sizeDelta = _ukuranAwalTombol[i] * SkalaTombol;
            SegarkanPenggeser();
        }
        public void AturMaster(float nilai) { VolMaster = Mathf.Clamp01(nilai); ProceduralAudioSynth.VolMaster = VolMaster; SegarkanPenggeser(); }
        public void AturEfek(float nilai) { VolEfek = Mathf.Clamp01(nilai); ProceduralAudioSynth.SetVolume(AudioCategory.Efek, VolEfek); SegarkanPenggeser(); }
        public void AturAmbien(float nilai) { VolAmbien = Mathf.Clamp01(nilai); ProceduralAudioSynth.SetVolume(AudioCategory.Ambien, VolAmbien); SegarkanPenggeser(); }
        public void AturMusik(float nilai) { VolMusik = Mathf.Clamp01(nilai); ProceduralAudioSynth.SetVolume(AudioCategory.Musik, VolMusik); SegarkanPenggeser(); }
        public void ToggleHeadBob() { HeadBob = !HeadBob; TerapkanPengaturan(); SimpanPengaturan(); }
        public void ToggleGoyangan() { Goyangan = !Goyangan; TerapkanPengaturan(); SimpanPengaturan(); }
        public void ToggleBantuanBidik() { CombatAndLeftHandController.AimAssist = !CombatAndLeftHandController.AimAssist; SimpanPengaturan(); SegarkanPenggeser(); }
        public void ToggleJedaPanel() { PengunciInput.AturJedaPanel(!PengunciInput.JedaPanel); SimpanPengaturan(); SegarkanPenggeser(); }

        // Bekal survival dasar untuk permainan baru (jika tas masih kosong dan bukan hasil muat simpanan)
        // agar tombol BUKU PANDUAN, Obor, dan alat bertahan hidup langsung bisa dicoba pemain.
        void BeriBekalAwalJikaKosong()
        {
            if (!Application.isPlaying) return;
            var inv = GameServices.Get<AlasKeramat.Inventory.TikarPandan3DInventory>();
            if (inv == null) return;
            foreach (var _ in inv.Isi) return;
            inv.Tambah(ItemID.BukuPanduan, 1);
            inv.Tambah(ItemID.KorekGas, 1);
            inv.Tambah(ItemID.OborBambu, 1);
            inv.Tambah(ItemID.BotolKosong, 1);
            inv.Tambah(ItemID.BotolAirBersih, 1);
            inv.Tambah(ItemID.RansumDarurat, 2);
            inv.Tambah(ItemID.PerbanKain, 2);
        }
#if UNITY_EDITOR
        public void MulaiUjiEditor()
        {
            PlayerPrefs.SetInt(GoreSystem.KunciPeringatan, 1);
            var peringatan = FindFirstObjectByType<LayarPeringatan>(FindObjectsInactive.Include);
            if (peringatan != null) peringatan.gameObject.SetActive(false);
            var boot = GameServices.Get<GameBootstrap>();
            if (boot != null) JedaDunia.Atur(boot.GetInstanceID(), false);
            var q = GameServices.Get<QuestManager>(); if (q != null) q.Progres.intro = true;
            _sesiDimulai = true; Dijeda = false; SembunyikanSemua(); JedaDunia.Atur(GetInstanceID(), false);
            PengunciInput.AturJedaPanel(true);
        }
#endif
    }
}
