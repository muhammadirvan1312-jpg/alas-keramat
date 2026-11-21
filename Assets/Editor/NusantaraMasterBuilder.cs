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
                Langkah("7z3. Partikel alam", 0.9895f, LangkahPartikelAlam);
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
        }

        public static bool LastBuildSucceeded { get { return _gagal.Count == 0; } }

        static void Catat(string s) { _laporan.Add("  - " + s); }
    }
}
