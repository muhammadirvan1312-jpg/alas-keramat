using System;
using System.Collections.Generic;
using AlasKeramat.Player;
using UnityEngine;
namespace AlasKeramat.Core
{
    // Misi M01-M13 (04 §3) dengan gerbang progresi 04 §4. Logika murni (Mulai/Syarat/Majukan) diuji self-test tanpa Play.
    public partial class QuestManager : MonoBehaviour, ISaveable
    {
        public const int JumlahMisi = 13;
        [Serializable] public class ProgresMisiT6
        {
            public List<string> koper = new List<string>();
            public bool jamu, catatan, saung, api, malam, pagi;
            public bool m05; // T8: diisi misi M05; membuka semua perintah Kang Deden
            public bool[] selesai = new bool[JumlahMisi];
            // T10: bendera gerbang 04 §4
            public int lore, paku;            // bitmask lore 1-12 ditemukan; paku keramat 1-3 pernah didapat
            public bool hari2, ningsihTerlihat, trust31, cangkul, kartu, rebreather, zipline, keluarBunker, sisiUtara, bossKalah, siegeSelesai;
            public int tertancap, ending;     // paku tertancap di altar (0-3); ending 0 = belum, 1 = A, 2 = B
            public bool intro;                // T10d: cutscene intro sudah diputar
            public bool[] sampingan = new bool[4]; // S01-S04 selesai
        }

        public static string TeksMisiSampingan(int indeks, bool selesai)
        {
            if (selesai) return "";
            switch (indeks)
            {
                case 0: return "Sampingan: Temukan kenangan Dewi di Alas Keramat";
                case 1: return "Sampingan: Cari jejak Rendra di bunker bawah tanah";
                case 2: return "Sampingan: Tenangkan arwah yang tersesat di malam hari";
                case 3: return "Sampingan: Nyalakan kemenyan persembahan di api unggun";
                default: return "";
            }
        }
        public ProgresMisiT6 Progres = new ProgresMisiT6();
        public bool M05Selesai { get { return Progres.m05; } }
        public string SaveKey { get { return "misi"; } }
        void Awake() { GameServices.Register(this); }
        void OnEnable() { GameEvents.OnStructureBuilt += BangunanSelesai; GameEvents.OnLoreFound += LoreDitemukan; GameEvents.OnItemChanged += ItemBerubah; }
        void OnDisable() { GameEvents.OnStructureBuilt -= BangunanSelesai; GameEvents.OnLoreFound -= LoreDitemukan; GameEvents.OnItemChanged -= ItemBerubah; }

        public static bool AdaLore(ProgresMisiT6 p, int nomor) { return (p.lore & (1 << (nomor - 1))) != 0; }
        static bool Selesai(ProgresMisiT6 p, QuestID q) { int i = (int)q - 1; return i >= 0 && i < p.selesai.Length && p.selesai[i]; }

        // Pemicu mulai (04 §3 kolom "Pemicu mulai").
        public static bool Mulai(ProgresMisiT6 p, QuestID q)
        {
            switch (q)
            {
                case QuestID.M01: case QuestID.M02: case QuestID.M03: return true;
                case QuestID.M04: return p.malam;
                case QuestID.M05: return AdaLore(p, 1) || p.hari2;
                case QuestID.M06: return p.ningsihTerlihat;
                case QuestID.M07: return AdaLore(p, 2);
                case QuestID.M08: return p.hari2 || AdaLore(p, 3);
                case QuestID.M09: return Selesai(p, QuestID.M07);
                case QuestID.M10: return p.zipline;
                case QuestID.M11: return Selesai(p, QuestID.M10);
                case QuestID.M12: return p.paku != 0;
                case QuestID.M13: return p.siegeSelesai;
                default: return false;
            }
        }

        public static bool Syarat(ProgresMisiT6 p, QuestID q)
        {
            switch (q)
            {
                case QuestID.M01: return p.koper.Count >= 3;
                case QuestID.M02: return p.jamu && p.catatan;
                case QuestID.M03: return p.saung && p.api;
                case QuestID.M04: return p.malam && p.pagi;
                case QuestID.M05: return p.cangkul && AdaLore(p, 2);
                case QuestID.M06: return p.trust31;
                case QuestID.M07: return AdaLore(p, 3);
                case QuestID.M08: return (p.paku & 1) != 0 && p.kartu && p.rebreather;
                case QuestID.M09: return (p.paku & 2) != 0 && p.zipline && p.keluarBunker;
                case QuestID.M10: return p.sisiUtara;
                case QuestID.M11: return p.bossKalah && (p.paku & 4) != 0;
                case QuestID.M12: return p.tertancap >= 3 && p.siegeSelesai;
                case QuestID.M13: return p.ending != 0;
                default: return false;
            }
        }

        // ID terkecil yang aktif & belum selesai (0 = tidak ada). M06 ditampilkan sebagai baris kedua (04 §3).
        public static int Prioritas(ProgresMisiT6 p)
        {
            for (int i = 0; i < JumlahMisi; i++) { var q = (QuestID)(i + 1); if (!p.selesai[i] && q != QuestID.M06 && Mulai(p, q)) return i + 1; }
            return !p.selesai[5] && Mulai(p, QuestID.M06) ? 6 : 0;
        }

        public static List<QuestID> Majukan(ProgresMisiT6 p)
        {
            var hasil = new List<QuestID>();
            bool lagi = true;
            while (lagi)
            {
                lagi = false;
                for (int i = 0; i < JumlahMisi; i++)
                {
                    var q = (QuestID)(i + 1);
                    if (p.selesai[i] || !Mulai(p, q) || !Syarat(p, q)) continue;
                    p.selesai[i] = true; hasil.Add(q); lagi = true;
                    if (q == QuestID.M05) p.m05 = true;
                }
            }
            return hasil;
        }

        public void Periksa() { foreach (var id in Majukan(Progres)) GameEvents.RaiseQuestAdvanced(id); }
        public void KoperDibuka(string id) { if (string.IsNullOrEmpty(id)) return; if (!Progres.koper.Contains(id)) Progres.koper.Add(id); Periksa(); }
        public void BeriJamu() { Progres.jamu = true; Periksa(); }
        public void TunjukkanCatatan() { if (!Progres.jamu) return; Progres.catatan = true; Periksa(); }
        void BangunanSelesai(BlueprintID id) { if (id == BlueprintID.SaungPanggung) Progres.saung = true; if (id == BlueprintID.ApiUnggun) Progres.api = true; Periksa(); }
        public void Waktu(int hari, float jam, bool hidup)
        {
            if (hari > 1 || (hari == 1 && jam >= 18f)) Progres.malam = true;
            if ((hari > 2 || (hari == 2 && jam >= 6f)) && hidup) Progres.pagi = true;
            if (hari > 2 || (hari == 2 && jam >= 7f)) Progres.hari2 = true;
            Periksa();
        }
        int _menit = -1;
        void Update()
        {
            var jam = GameServices.Get<TimeOfDayClock>(); if (jam == null) return;
            int menit = jam.Hari * 1440 + Mathf.FloorToInt(jam.Jam * 60f); if (menit == _menit) return; _menit = menit;
            var vital = GameServices.Get<AndiVitalsAndSanity>();
            PeriksaDunia();
            Waktu(jam.Hari, jam.Jam, vital != null && vital.IsAlive);
        }

        public string Kartu()
        {
            var p = Progres;
            int utama = Prioritas(p);
            string teks = TeksMisi(p, utama);
            if (utama != 6 && !p.selesai[5] && Mulai(p, QuestID.M06)) teks += "\n" + TeksMisi(p, 6);
            return teks;
        }

        public static string TeksMisi(ProgresMisiT6 p, int id)
        {
            switch (id)
            {
                case 1: return "Ambil Koper Darurat (" + p.koper.Count + "/3)";
                case 2: return p.jamu ? "Tolong Kang Deden: tunjukkan Buku Catatan" : "Tolong Kang Deden: beri Jamu, lalu tunjukkan Buku Catatan";
                case 3: return "Bangun Saung Panggung & Api Unggun sebelum 18:00\nSaung (" + (p.saung ? 1 : 0) + "/1), Api (" + (p.api ? 1 : 0) + "/1)";
                case 4: return "Bertahan sampai pagi. Jaga api tetap menyala.";
                case 5: return "Selidiki desa di timur sungai (cari Cangkul)";
                case 6: return "Dekati gadis berkebaya tanpa senjata terhunus";
                case 7: return "Periksa kebun sawit terbakar di barat";
                case 8: return "Masuki gua di lembah bambu";
                case 9: return "Gali tanah bertanda X dan buka bunker";
                case 10: return "Seberangi Jurang Kapur dengan tali luncur";
                case 11: return "Masuki Kuil Akar Hitam dan rebut Paku Ketiga";
                case 12: return "Tancapkan tiga Paku Keramat di Altar Beringin (" + p.tertancap + "/3)";
                case 13: return "Temui Helikopter SAR di Pantai Selatan";
                default: return "Jelajahi pulau. Cari jejak tim yang hilang.";
            }
        }

        public string CaptureJson() { return JsonUtility.ToJson(Progres); }
        public void RestoreJson(string json)
        {
            var p = JsonUtility.FromJson<ProgresMisiT6>(json);
            if (p == null || p.selesai == null || p.selesai.Length < 4 || p.koper == null) return;
            if (p.selesai.Length < JumlahMisi) { var b = new bool[JumlahMisi]; Array.Copy(p.selesai, b, p.selesai.Length); p.selesai = b; } // simpanan T6 (4 misi)
            Progres = p; _menit = -1;
        }
    }
}
