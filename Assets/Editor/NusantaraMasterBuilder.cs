using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AlasKeramat.EditorTools
{
    // Builder 1-klik & idempoten (H7, docs/tech/11 bagian 9). Langkah dipecah di BuilderSteps.*.cs.
    public static partial class NusantaraMasterBuilder
    {
        public const string FolderGen = "Assets/Generated";
        public const string PathScene = "Assets/Scenes/AlasKeramat_Main.unity";
        public const string NamaRoot = "[ALAS_KERAMAT_ROOT]";
        public const string PathLaporan = "Logs/nusantara_build_report.txt";
        public const string JudulT1 = "ALAS KERAMAT - DUNIA";

        static readonly List<string> _laporan = new List<string>();
        static readonly List<string> _gagal = new List<string>();

        [MenuItem("Nusantara Builder/1. BANGUN SELURUH GAME (1-KLIK)", false, 1)]
        public static void BuildAll() { Bangun(false); }

        [MenuItem("Nusantara Builder/2. Bangun Ulang Cepat (pakai aset yang ada)", false, 2)]
        public static void BuildQuick() { Bangun(true); }

        [MenuItem("Nusantara Builder/Tahap/Bangun sampai T2", false, 20)]
        public static void BuildUntilT2() { Bangun(false); }

        [MenuItem("Nusantara Builder/Utilitas/Buka Scene Utama", false, 40)]
        public static void OpenMainScene()
        {
            if (File.Exists(PathScene)) UnityEditor.SceneManagement.EditorSceneManager.OpenScene(PathScene);
            else EditorUtility.DisplayDialog("Alas Keramat", "Scene utama belum ada. Klik 1. BANGUN SELURUH GAME dulu.", "OK");
        }

        [MenuItem("Nusantara Builder/Utilitas/Tampilkan Laporan Build", false, 41)]
        public static void ShowReport()
        {
            string isi = File.Exists(PathLaporan) ? File.ReadAllText(PathLaporan) : "Laporan belum ada.";
            EditorUtility.DisplayDialog("Laporan Build", isi.Length > 1500 ? isi.Substring(0, 1500) : isi, "OK");
        }

        [MenuItem("Nusantara Builder/Utilitas/Hapus Semua Hasil Generate", false, 42)]
        public static void DeleteGenerated()
        {
            if (AssetDatabase.IsValidFolder(FolderGen)) AssetDatabase.DeleteAsset(FolderGen);
            AssetDatabase.Refresh();
        }

        static void Bangun(bool cepat)
        {
            var jam = Stopwatch.StartNew();
            _laporan.Clear(); _gagal.Clear();
            _laporan.Add("Laporan Builder Alas Keramat - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            try
            {
                if (!cepat) Langkah("0. Bersihkan Assets/Generated", 0.01f, DeleteGenerated);
                Langkah("1. Proyek", 0.05f, LangkahProyek);
                Langkah("2. Aset", 0.30f, () => LangkahAset(cepat));
                Langkah("3. Scene & langit", 0.32f, LangkahScene);
                Langkah("3b. Dunia 2x2 km", 0.35f, LangkahDunia);
                Langkah("3c. Vegetasi", 0.62f, LangkahVegetasi);
                Langkah("3d. Landmark", 0.66f, LangkahLandmark);
                Langkah("3e. Ambien hidup", 0.68f, LangkahAmbien);
                Langkah("3f. Pasca-proses", 0.69f, LangkahPasca);
                Langkah("6. Sistem", 0.70f, LangkahSistem);
                Langkah("7. UI", 0.85f, LangkahUI);
                Langkah("7b. Andi & kontrol sentuh", 0.9f, LangkahAndi);
                Langkah("7c. Interaksi, koper, radar", 0.92f, LangkahInteraksi);
                Langkah("7e. Vital Andi & bar HUD", 0.925f, LangkahVital);
                Langkah("7f. Foley, subtitle, sabuk cepat", 0.928f, LangkahHUDLanjut);
                Langkah("7g. Karakter & animasi", 0.935f, LangkahKarakter);
                Langkah("7h. Tebang & melee", 0.94f, LangkahTebang);
                Langkah("7i. Hewan & titik pungut", 0.945f, LangkahHewan);
                Langkah("7j. Tikar Pandan & zirah", 0.948f, LangkahTikar);
                Langkah("7k. Panggul & bangun bebas", 0.95f, LangkahBangun);
                Langkah("7l. Buku Panduan & simpan", 0.952f, LangkahBlueprint);
                Langkah("7m. Dunia, bangunan & misi", 0.954f, LangkahT6c);
                Langkah("7n. Musuh, jalur & tempur", 0.956f, LangkahMusuh);
                Langkah("7o. Hantu, raid & totem", 0.958f, LangkahHantu);
                Langkah("7p. Gore, luka & peringatan 18+", 0.959f, LangkahGore);
                Langkah("7q. Senjata jarak, elak, stealth", 0.96f, LangkahJarak);
                Langkah("7r. Companion Kang Deden & Ningsih", 0.962f, LangkahCompanion);
                Langkah("7s. Sutradara Teror & dunia hidup", 0.964f, LangkahTeror);
                Langkah("7t. Gua, bunker & kuil bawah tanah", 0.966f, LangkahGua);
                Langkah("7u. Ritual Beringin, siege & lore", 0.968f, LangkahBossAltar);
                Langkah("7v. Heli SAR, ending & statistik", 0.969f, LangkahEnding);
                Langkah("7w. G3 visual realistis", 0.979f, LangkahG3);
                Langkah("7w2. Tata kontrol mobile", 0.984f, LangkahTataKontrol);
                Langkah("7x. Menu, pengaturan & debug T11", 0.985f, LangkahT11);
                Langkah("7y. Peta survei & navigasi", 0.987f, LangkahPetaSurvei);
                Langkah("7z. Animasi mocap Mixamo", 0.988f, LangkahMixamo);
                Langkah("7z2. Rekaman CC0 Kenney", 0.989f, LangkahAudioKenney);
                Langkah("7z4. Cookie senter", 0.9897f, LangkahCookieSenter);
                Langkah("7z3. Partikel alam", 0.9895f, LangkahPartikelAlam);
                Langkah("7z5. Indikator luka", 0.9898f, LangkahIndikatorLuka);
                Langkah("7z6. Ikon kontrol & joystick", 0.9899f, LangkahIkonKontrol);
                Langkah("7z7. Hewan model asli", 0.99f, LangkahHewanAsli);
                Langkah("7z8. Props desa CC0 & lampu minyak", 0.991f, LangkahPropDesa);
                Langkah("7z9. Ekspansi besar SOTF (props 2K, biota, perangkap & HUD)", 0.992f, LangkahEkspansiSOTF);
                Langkah("7z10. Ekspansi Dunia 2 (24 model 4K, jemuran, air hujan, bubu, suar, musim & sekte)", 0.994f, LangkahEkspansiDunia2);
                Langkah("7z11. Fitur Baru SOTF (Zipline, Perahu Dayung, Rumah Pohon, Sarang, Boss Buto Ijo & Suar)", 0.996f, LangkahFiturBaruSOTF);
                Langkah("7z12. Overhaul Visual & Mega-Landmark (Air Terjun, Candi, Jembatan Gantung, Kapal, Pendopo)", 0.998f, LangkahOverhaulVisualDunia);
                Langkah("7z13. Sistem Lanjutan SOTF (Kombo, Level Survival, Radio Darurat, Sesajen & Audio Spasial)", 0.999f, LangkahSistemLanjutan);
                Langkah("7z14. Overhaul Kontrol Mobile Android & Pengayaan Jalur/Bunga Dunia", 0.9995f, LangkahOverhaulKontrolDanDunia);
                Langkah("7z15. Ekspansi Penuh 4 Kuadran Pulau 4 km2 & Ekosistem Kawanan", 0.9998f, LangkahEkspansiPulau4Km);
                Langkah("7z16. Dunia Bernapas (Patroli Obor Sekte, Monyet/Kodok, Asap/Kain Angin & Pertemuan Spontan)", 0.9999f, LangkahDuniaBernapas);
                Langkah("7z17. Revisi & Polish Terpadu (HUD Boss/Status/Kombo & Pos Latihan Awal)", 0.99995f, LangkahRevisiTerpadu);
                Langkah("7z18a. Mega Overhaul Visual & Dunia (7 Landmark Baru, God-Rays, Kabut Lembah, Kunang-Kunang & Vignette)", 0.99997f, LangkahOverhaulMegaVisual);
                Langkah("7z18b. Mega Overhaul Gameplay (4 Glider Bambu, 6 Spot Pancing, 8 Peti Parasut, 5 Kuali Jamu, Kentongan & Jurnal 22 Lokasi)", 0.99999f, LangkahOverhaulMegaGameplay);
                Langkah("7z19. Overhaul Karakter Visual & Dunia Hidup V6 (12 Lokasi, 64 Satwa, Aktivitas Karakter & Lentera Desa/Pantai)", 0.999995f, LangkahOverhaulDuniaHidupV6);
                Langkah("7z20. Overhaul Dunia Hidup V7 Skala Penuh (22/22 Lokasi, Kurir Sekte, Anjing, Gagak, Ikan/Kepiting, Kincir & Roh Keramat)", 0.999998f, LangkahOverhaulDuniaHidupV7);
                Langkah("7z21. Overhaul Dunia Hidup V8 (Migrasi Formasi-V, Rantai Makanan 24 Jam, 24 Interaksi Alam, Hayati Tropis & 22 Tugu Jalur)", 0.999999f, LangkahOverhaulDuniaHidupV8);
                Langkah("7z22. Overhaul Dunia Nyata V9 (320+ Props 4K Scan di 22 Lokasi, 180+ Alam Scan, 12 NPC MakeHuman G3, 28 Satwa FBX & Suara 24 Jam)", 0.9999995f, LangkahOverhaulDuniaRealV9);
                Langkah("7z23. Overhaul Arsitektur, Interior 4K, Sosial Companion & Cuaca Hidup V10 (12 Catatan Dunia & Petir Badai)", 0.9999998f, LangkahOverhaulArsitekturDanSosialV10);
                Langkah("7z24. Overhaul Hero Camera, Senjata FP PBR & Gua Bioluminesens V11 (UG1-UG3 + 7 Sudut Hero)", 0.9999999f, LangkahOverhaulHeroDanGuaV11);
                Langkah("7d. Pasca-proses kamera", 0.93f, AktifkanPasca);
                Langkah("9. Simpan scene", 0.95f, LangkahSimpan);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            _laporan.Add("Durasi: " + jam.Elapsed.TotalSeconds.ToString("0.0") + " detik");
            _laporan.Add(_gagal.Count == 0 ? "HASIL: SEMUA LANGKAH LULUS" : "HASIL: GAGAL di " + string.Join(", ", _gagal.ToArray()));
            Directory.CreateDirectory("Logs");
            File.WriteAllLines(PathLaporan, _laporan.ToArray());
            if (!Application.isBatchMode)
                EditorUtility.DisplayDialog("Alas Keramat", string.Join("\n", _laporan.ToArray()), "OK");
            if (_gagal.Count > 0) Debug.LogError("[AK][Builder] Langkah GAGAL: " + string.Join(", ", _gagal.ToArray()));
        }

        static void Langkah(string nama, float progres, Action aksi)
        {
            EditorUtility.DisplayProgressBar("Nusantara Builder", nama, progres);
            try
            {
                aksi();
                _laporan.Add("[LULUS] " + nama);
            }
            catch (Exception e)
            {
                _gagal.Add(nama);
                _laporan.Add("[GAGAL] " + nama + ": " + e.Message);
                Debug.LogError("[AK][Builder] Langkah " + nama + " gagal: " + e);
            }
            LepasMemori();
        }

        // Pulau 4 km + ratusan tekstur foto CC0 (dibaca mentah RGBA32) menghabiskan RAM runner CI 16 GB (exit 137).
        // Lepas aset yang tidak lagi dirujuk scene/kode HANYA di antara langkah: di tengah langkah, objek yang baru dibuat
        // dan hanya dipegang variabel lokal (mis. VegetationData) ikut dihancurkan (MissingReferenceException).
        public static void LepasMemori()
        {
            EditorUtility.UnloadUnusedAssetsImmediate(true);
            GC.Collect();
        }

        public static bool LastBuildSucceeded { get { return _gagal.Count == 0; } }

        static void Catat(string s) { _laporan.Add("  - " + s); }
    }
}
