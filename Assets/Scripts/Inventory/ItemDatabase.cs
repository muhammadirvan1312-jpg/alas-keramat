using System;
using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEngine;
using I = AlasKeramat.Core.ItemID;
using C = AlasKeramat.Core.ItemCategory;

namespace AlasKeramat.Inventory
{
    public static class ItemDatabase
    {
        private const int TidakDapatDibawa = 0;
        private const int MaksUnik = 1;
        private const int MaksMakananDefault = 10;
        private const int MaksRamuanDefault = 5;
        private const int MaksZirahDefault = 10;
        private const int MaksPanahDefault = 30;
        private const int MaksPanahKhususDefault = 15;
        private const int MaksPakuDefault = 6;
        private const int MaksPerbanDefault = 10;
        private const float StaminaPenuh = 100f;
        private const float TidakBasi = 0f;

        private static readonly Dictionary<I, ItemDefinition> Definitions =
            new Dictionary<I, ItemDefinition>();

        private static readonly HashSet<I> WarnedUnknown =
            new HashSet<I>();

        private static IReadOnlyList<ItemDefinition> all =
            Array.AsReadOnly(Array.Empty<ItemDefinition>());

        static ItemDatabase()
        {
            Initialize();
        }

        public static IReadOnlyList<ItemDefinition> All
        {
            get { return all; }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Initialize();
        }

        private static void Initialize()
        {
            Definitions.Clear();
            WarnedUnknown.Clear();
            all = Array.AsReadOnly(Array.Empty<ItemDefinition>());

            try
            {
                // Angka pada tabel adalah data desain, bukan konstanta algoritma.
                // Nilai yang tidak ditentukan dokumen memakai default bernama.
                ItemDefinition[] rows =
                {
                    D(I.None, "Tidak ada benda", "Penanda slot kosong.", C.Bahan, TidakDapatDibawa),
                    D(I.KayuGelondong, "Kayu Gelondong", "Kayu hasil tebangan yang dibawa di bahu.", C.Bahan, 2, "kayu"),
                    D(I.Ranting, "Ranting", "Bahan rakitan dan bahan bakar api.", C.Bahan, 30, "ranting"),
                    D(I.Batu, "Batu", "Bahan alat, bangunan, dan umpan lempar.", C.Bahan, 20, "batu"),
                    D(I.DaunPisang, "Daun Pisang", "Bahan pelindung, bangunan, dan bahan bakar.", C.Bahan, 30, "daun"),
                    D(I.SeratIjuk, "Serat Ijuk", "Serat pohon aren untuk merakit tali.", C.Bahan, 20, "tali"),
                    D(I.TaliIjuk, "Tali Ijuk", "Pengikat untuk alat dan bangunan.", C.Bahan, 10, "tali"),
                    D(I.KainBatik, "Kain Batik", "Kain untuk obor, perban, dan rakitan lainnya.", C.Bahan, 10),
                    D(I.Bambu, "Batang Bambu", "Bahan senjata, alat, dan bangunan.", C.Bahan, 6, "kayu"),
                    D(I.PapanKayu, "Papan Kayu", "Papan hasil belahan gelondong yang dibawa secara fisik.", C.Bahan, 2, "kayu"),
                    D(I.Tulang, "Tulang", "Bahan zirah, busur, dan anak panah.", C.Bahan, 20),
                    D(I.KulitHewan, "Kulit Hewan", "Bahan ketapel dan zirah.", C.Bahan, 6),
                    D(I.Tengkorak, "Tengkorak Kerbau", "Bahan bangunan dan hiasan kemah.", C.Bahan, 4),
                    D(I.Bulu, "Bulu Burung", "Bulu ayam hutan untuk merakit anak panah.", C.Bahan, 30),
                    D(I.Damar, "Getah Damar", "Getah pohon untuk merakit obor dan panah api.", C.Bahan, 10, "api"),
                    D(I.Kemenyan, "Kemenyan", "Dibakar di api untuk menenangkan pikiran dan menjauhkan Pocong.", C.Bahan, 6, "kewarasan"),
                    D(I.LogamBekas, "Logam Bekas", "Bahan senjata, amunisi, dan zirah.", C.Bahan, 12),
                    D(I.Baterai, "Baterai", "Sumber tenaga senter taktis.", C.Bahan, 10),
                    D(I.MinyakTanah, "Minyak Tanah", "Bahan bakar lampu dan bom bambu.", C.Bahan, 4, "api"),
                    D(I.Arang, "Arang", "Sisa pembakaran untuk bahan bakar api.", C.Bahan, 20),
                    D(I.BotolKosong, "Botol Kosong", "Wadah untuk menampung air.", C.Bahan, 3, "air"),
                    D(I.MesiuHitam, "Mesiu Hitam", "Bahan rakitan amunisi.", C.Bahan, 6),
                    D(I.KainKafan, "Kain Kafan", "Dapat menggantikan kain batik pada resep yang meminta kain.", C.Bahan, 6),
                    D(I.PotonganTubuh, "Potongan Tubuh", "Bahan Patung Ngeri pada pengaturan gore penuh. Tidak dapat dimakan.", C.Bahan, 4),
                    D(I.TopengSekte, "Topeng Sekte", "Topeng anggota Sekte untuk bahan patung.", C.Bahan, 6),
                    D(I.BatuBesar, "Batu Besar", "Batu bangunan yang dipanggul satu per satu.", C.Bahan, MaksUnik, "batu"),

                    D(I.KelapaMuda, "Kelapa Muda", "Dibuka dengan golok atau kapak untuk diminum.", C.Makanan, MaksMakananDefault, "makanan", fullness: 6f, hydration: 22f),
                    D(I.Pisang, "Pisang", "Buah hutan yang mengenyangkan.", C.Makanan, MaksMakananDefault, "makanan", fullness: 10f, hydration: 2f, spoilDays: 4f, spoilsInto: I.DagingBusuk),
                    D(I.Jamur, "Jamur", "Jamur yang dapat dimakan atau dimasak.", C.Makanan, MaksMakananDefault, "makanan", fullness: 6f, spoilDays: 3f, spoilsInto: I.DagingBusuk),
                    D(I.JamurBeracun, "Jamur Beracun", "Mengurangi kesehatan selama 10 detik dan mengganggu kewarasan.", C.Makanan, MaksMakananDefault, "makanan", fullness: 4f, health: -10f, sanity: -15f),
                    D(I.DaunSirih, "Daun Sirih", "Daun obat yang memulihkan kesehatan dan menenangkan pikiran.", C.Makanan, MaksMakananDefault, "daun", health: 8f, sanity: 2f),
                    D(I.KunyitObat, "Kunyit Obat", "Umbi obat untuk memulihkan kesehatan dan menghentikan keracunan.", C.Makanan, MaksMakananDefault, "makanan", health: 5f),
                    D(I.JaheHutan, "Jahe Hutan", "Mengurangi dingin dan basah serta memulihkan stamina.", C.Makanan, MaksMakananDefault, "makanan", stamina: 10f),
                    D(I.IkanMentah, "Ikan Mentah", "Ikan belum dimasak. Berisiko menyebabkan keracunan.", C.Makanan, MaksMakananDefault, "makanan", fullness: 8f, sanity: -2f, spoilDays: 2f, spoilsInto: I.DagingBusuk),
                    D(I.IkanMatang, "Ikan Matang", "Ikan matang yang memulihkan kenyang dan kesehatan.", C.Makanan, MaksMakananDefault, "makanan", fullness: 22f, health: 4f, spoilDays: 3f, spoilsInto: I.DagingBusuk),
                    D(I.IkanKering, "Ikan Kering", "Ikan yang diawetkan dengan pengeringan.", C.Makanan, MaksMakananDefault, "makanan", fullness: 18f),
                    D(I.DagingMentah, "Daging Mentah", "Daging hewan belum dimasak. Berisiko menyebabkan keracunan.", C.Makanan, MaksMakananDefault, "makanan", fullness: 10f, sanity: -3f, spoilDays: 2f, spoilsInto: I.DagingBusuk),
                    D(I.DagingMatang, "Daging Matang", "Daging hewan matang yang memulihkan kesehatan.", C.Makanan, MaksMakananDefault, "makanan", fullness: 30f, health: 6f, spoilDays: 3f, spoilsInto: I.DagingBusuk),
                    D(I.DagingKering, "Daging Kering", "Daging hewan yang diawetkan dengan pengeringan.", C.Makanan, MaksMakananDefault, "makanan", fullness: 24f),
                    D(I.TelurAyamHutan, "Telur Ayam Hutan", "Telur dari sarang ayam hutan.", C.Makanan, MaksMakananDefault, "makanan", fullness: 8f, hydration: 2f, spoilDays: 3f, spoilsInto: I.DagingBusuk),
                    D(I.RansumDarurat, "Ransum Darurat", "Bekal darurat yang menyisakan botol kosong setelah dikonsumsi.", C.Makanan, MaksMakananDefault, "makanan", fullness: 40f, hydration: 5f, sanity: 5f),
                    D(I.BotolAirBersih, "Botol Air Bersih", "Air minum yang menyisakan botol kosong.", C.Makanan, 3, "air", hydration: 35f),
                    D(I.SupHutan, "Sup Hutan", "Sup hangat yang memulihkan kenyang, hidrasi, kesehatan, dan kewarasan.", C.Makanan, MaksRamuanDefault, "makanan", fullness: 45f, hydration: 30f, health: 15f, sanity: 5f, spoilDays: 2f, spoilsInto: I.DagingBusuk),
                    D(I.JamuHerbal, "Jamu Herbal", "Memulihkan kesehatan selama 5 detik dan membantu mengatasi racun.", C.Makanan, MaksRamuanDefault, "nyawa", health: 35f, sanity: 10f),
                    D(I.JamuEnergi, "Jamu Energi", "Memulihkan stamina penuh, meningkatkan pemulihannya, dan menambah istirahat.", C.Makanan, MaksRamuanDefault, "stamina", stamina: StaminaPenuh),
                    D(I.MakananGosong, "Makanan Gosong", "Makanan hangus yang sedikit mengenyangkan.", C.Makanan, MaksMakananDefault, "makanan", fullness: 5f, sanity: -1f),
                    D(I.DagingBusuk, "Daging Busuk", "Makanan busuk dengan risiko keracunan tinggi.", C.Makanan, MaksMakananDefault, "makanan", fullness: 6f, sanity: -5f),
                    D(I.Kepiting, "Kepiting", "Kepiting mentah untuk makanan atau penangkal Tuyul. Lebih mengenyangkan setelah dimasak.", C.Makanan, MaksMakananDefault, "makanan", fullness: 8f),
                    D(I.PerbanKain, "Perban Kain", "Menghentikan pendarahan dan memulihkan kesehatan.", C.Alat, MaksPerbanDefault, "nyawa", health: 10f),

                    D(I.GolokPusaka, "Golok Pusaka", "Senjata tebas dan alat untuk merakit.", C.Senjata, MaksUnik, "golok"),
                    D(I.KapakBatu, "Kapak Batu", "Kapak rakitan untuk menebang dan bertahan.", C.Senjata, MaksUnik, "kapak"),
                    D(I.TombakBambu, "Tombak Bambu", "Tombak bambu untuk menusuk atau dilempar.", C.Senjata, 3, "tombak"),
                    D(I.BusurBambu, "Busur Bambu", "Busur rakitan untuk menembakkan anak panah.", C.Senjata, MaksUnik, "busur"),
                    D(I.AnakPanah, "Anak Panah", "Amunisi busur dengan ujung batu.", C.Amunisi, MaksPanahDefault, "panah"),
                    D(I.PanahApi, "Panah Api", "Anak panah berlapis kain dan damar.", C.Amunisi, MaksPanahKhususDefault, "panah"),
                    D(I.Ketapel, "Ketapel", "Senjata rakitan untuk melontarkan batu.", C.Senjata, MaksUnik),
                    D(I.PistolSuar, "Pistol Suar", "Pelontar suar yang ditemukan di bangkai perahu.", C.Senjata, MaksUnik),
                    D(I.PelurSuar, "Peluru Suar", "Amunisi untuk pistol suar.", C.Amunisi, 8, "api"),
                    D(I.BedilLocok, "Bedil Locok", "Senjata api tradisional dari desa.", C.Senjata, MaksUnik),
                    D(I.PeluruLocok, "Peluru Locok", "Amunisi untuk bedil locok.", C.Amunisi, 12),
                    D(I.KerisPusaka, "Keris Pusaka", "Senjata pusaka dari Kuil Akar.", C.Senjata, MaksUnik, "golok"),
                    D(I.GolokApiPusaka, "Golok Api Pusaka", "Golok pusaka yang diperoleh melalui akhir cerita.", C.Senjata, MaksUnik, "golok"),
                    D(I.Cangkul, "Cangkul", "Alat untuk menggali tanah.", C.Alat, MaksUnik),
                    D(I.ZiplineGun, "Peluncur Tali", "Alat untuk memasang tali luncur.", C.Alat, MaksUnik),
                    D(I.TaliZipline, "Gulungan Tali Luncur", "Tali untuk jalur luncur.", C.Bahan, 10, "tali"),
                    D(I.BomBambuMinyak, "Bom Bambu Minyak", "Senjata lempar rakitan berisi minyak.", C.Senjata, 5, "api"),
                    D(I.KapakBaja, "Kapak Baja", "Kapak baja dari bunker.", C.Senjata, MaksUnik, "kapak"),
                    D(I.PakuBesiKuning, "Paku Besi Kuning", "Pengikat Kuntilanak, ditempa di api unggun selama 20 detik.", C.Alat, MaksPakuDefault),
                    D(I.PistolPT, "Pistol PT", "Pistol dari gudang bunker.", C.Senjata, MaksUnik),
                    D(I.PeluruPistol, "Peluru Pistol", "Amunisi untuk pistol PT.", C.Amunisi, 32),
                    D(I.SenapanLaras, "Senapan Laras Pendek", "Senapan dengan tembakan menyebar.", C.Senjata, MaksUnik),
                    D(I.PeluruSenapan, "Peluru Senapan", "Amunisi untuk senapan laras pendek.", C.Amunisi, 18),
                    D(I.Mandau, "Mandau", "Senjata tebas dari batu ritual Watu Ireng.", C.Senjata, MaksUnik, "golok"),
                    D(I.TombakLogam, "Tombak Logam", "Tombak bambu yang diperkuat logam.", C.Senjata, 3, "tombak"),
                    D(I.BusurTanduk, "Busur Tanduk", "Busur yang diperkuat tulang untuk tembakan lebih kuat.", C.Senjata, MaksUnik, "busur"),
                    D(I.PanahTulang, "Panah Tulang", "Anak panah bertulang yang menembus pelindung Buto.", C.Amunisi, MaksPanahDefault, "panah"),
                    D(I.PanahRacun, "Panah Racun", "Anak panah beracun yang tidak meracuni hantu.", C.Amunisi, MaksPanahKhususDefault, "panah"),

                    D(I.OborBambu, "Obor Bambu", "Penerang tangan kiri yang dinyalakan dengan korek.", C.Alat, MaksUnik, "obor"),
                    D(I.SenterTaktis, "Senter Taktis", "Penerang bertenaga baterai yang tahan angin.", C.Alat, MaksUnik, "senter"),
                    D(I.GPSHandset, "GPS", "Perangkat radar dan penanda lokasi.", C.Alat, MaksUnik, "peta"),
                    D(I.BukuPanduan, "Buku Panduan", "Panduan rancangan bangunan.", C.Alat, MaksUnik, "buku"),
                    D(I.BukuCatatan, "Buku Catatan", "Catatan resep, cerita, dan perintah untuk Kang Deden.", C.Alat, MaksUnik, "buku"),
                    D(I.KorekGas, "Korek Gas", "Alat untuk menyalakan api dan obor.", C.Alat, MaksUnik, "api"),
                    D(I.AlatSelamRebreather, "Alat Selam", "Peralatan untuk bernapas lebih lama di bawah air.", C.Alat, MaksUnik),
                    D(I.KartuAksesBunker, "Kartu Akses Bunker", "Kartu untuk membuka pintu akses bunker.", C.Cerita, MaksUnik),
                    D(I.PetaKertas, "Peta Survei", "Peta kertas untuk melihat wilayah.", C.Alat, MaksUnik, "peta"),

                    D(I.ZirahDaun, "Zirah Daun", "Pelindung daun yang cepat rusak saat hujan.", C.Zirah, MaksZirahDefault, "daun"),
                    D(I.ZirahBambu, "Zirah Bambu", "Pelindung bambu yang rentan terhadap api.", C.Zirah, MaksZirahDefault),
                    D(I.ZirahTulang, "Zirah Tulang", "Pelindung tulang yang mengusik kewarasan saat pertama dipakai.", C.Zirah, MaksZirahDefault),
                    D(I.ZirahBaja, "Zirah Baja", "Pelindung baja yang dapat memperlambat renang.", C.Zirah, MaksZirahDefault),
                    D(I.ZirahEmas, "Zirah Emas", "Pelindung kuat yang membuat hantu ragu menyerang.", C.Zirah, MaksZirahDefault),

                    D(I.PakuKeramat1, "Paku Keramat Pertama", "Kunci cerita yang tidak dapat dibuang.", C.Cerita, MaksUnik),
                    D(I.PakuKeramat2, "Paku Keramat Kedua", "Kunci cerita yang tidak dapat dibuang.", C.Cerita, MaksUnik),
                    D(I.PakuKeramat3, "Paku Keramat Ketiga", "Kunci cerita yang tidak dapat dibuang.", C.Cerita, MaksUnik),
                    D(I.PatungKayuDewi, "Patung Kayu untuk Dewi", "Kenangan dari Kang Deden yang tidak dapat dibuang.", C.Cerita, MaksUnik, "kayu"),
                    D(I.TandaPengenalRendra, "Tanda Pengenal Rendra", "Kenangan dari bunker yang tidak dapat dibuang.", C.Cerita, MaksUnik)
                };

                var accepted = new List<ItemDefinition>(rows.Length);
                for (int index = 0; index < rows.Length; index++)
                {
                    ItemDefinition item = rows[index];
                    if (Definitions.ContainsKey(item.id))
                    {
                        Debug.LogError(
                            "[AK][ItemDatabase] ID item ganda: " + (int)item.id);
                        continue;
                    }

                    Definitions.Add(item.id, item);
                    accepted.Add(item);
                }

                all = accepted.AsReadOnly();

                foreach (I id in Enum.GetValues(typeof(I)))
                {
                    if (!Definitions.ContainsKey(id))
                    {
                        Debug.LogError(
                            "[AK][ItemDatabase] Definisi item belum tersedia: " + id);
                    }
                }
            }
            catch (Exception error)
            {
                Debug.LogError(
                    "[AK][ItemDatabase] Inisialisasi gagal: " + error.Message);
            }
        }

        public static ItemDefinition Get(I id)
        {
            ItemDefinition definition;
            if (Definitions.TryGetValue(id, out definition))
                return definition;

            if (WarnedUnknown.Add(id))
            {
                Debug.LogWarning(
                    "[AK][ItemDatabase] ID item tidak dikenal: " + (int)id);
            }

            return D(
                id,
                "Benda tak dikenal",
                "Benda ini belum memiliki definisi.",
                C.Bahan,
                TidakDapatDibawa);
        }

        public static bool Has(I id)
        {
            return Definitions.ContainsKey(id);
        }

        public static bool IsFood(I id)
        {
            ItemDefinition definition;
            return Definitions.TryGetValue(id, out definition)
                && definition.category == C.Makanan;
        }

        public static int MaxCarry(I id)
        {
            return Get(id).maxCarry;
        }

        private static ItemDefinition D(
            I id,
            string displayName,
            string description,
            C category,
            int maxCarry,
            string iconName = "",
            float fullness = 0f,
            float hydration = 0f,
            float health = 0f,
            float stamina = 0f,
            float sanity = 0f,
            float spoilDays = TidakBasi,
            I spoilsInto = I.None)
        {
            return new ItemDefinition
            {
                id = id,
                displayName = displayName,
                description = description,
                category = category,
                maxCarry = maxCarry,
                iconName = iconName,
                fullness = fullness,
                hydration = hydration,
                health = health,
                stamina = stamina,
                sanity = sanity,
                spoilDays = spoilDays,
                spoilsInto = spoilsInto
            };
        }
    }

    public static class RecipeDatabase
    {
        private const int ResepLanjutanPertama = 11;
        private const int JumlahMinimum = 1;

        private static readonly Dictionary<int, CraftingRecipe> Recipes =
            new Dictionary<int, CraftingRecipe>();

        private static readonly List<CraftingRecipe> Ordered =
            new List<CraftingRecipe>();

        static RecipeDatabase()
        {
            Initialize();
        }

        // Salinan mendalam: perubahan inputs oleh pemanggil tidak mengubah database.
        // Ambil saat membuka buku/resep, bukan setiap frame.
        public static IReadOnlyList<CraftingRecipe> All
        {
            get
            {
                var snapshot = new CraftingRecipe[Ordered.Count];
                for (int index = 0; index < Ordered.Count; index++)
                    snapshot[index] = Copy(Ordered[index]);

                return Array.AsReadOnly(snapshot);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Initialize();
        }

        private static void Initialize()
        {
            Recipes.Clear();
            Ordered.Clear();

            try
            {
                // KainBatik pada tabel adalah pilihan baku untuk bahan "Kain".
                // CraftableFrom menggantinya dengan kain yang benar-benar tersedia.
                CraftingRecipe[] rows =
                {
                    R(1, I.TaliIjuk, 1, I.None, S(I.SeratIjuk, 3)),
                    R(2, I.TombakBambu, 1, I.GolokPusaka, S(I.Bambu, 1), S(I.TaliIjuk, 1)),
                    R(3, I.KapakBatu, 1, I.None, S(I.Ranting, 1), S(I.Batu, 1), S(I.TaliIjuk, 1)),
                    R(4, I.OborBambu, 1, I.None, S(I.Bambu, 1), S(I.KainBatik, 1), S(I.Damar, 1)),
                    R(5, I.BusurBambu, 1, I.GolokPusaka, S(I.Bambu, 2), S(I.TaliIjuk, 2)),
                    R(6, I.AnakPanah, 4, I.GolokPusaka, S(I.Ranting, 2), S(I.Bulu, 2), S(I.Batu, 1)),
                    R(7, I.PanahApi, 2, I.None, S(I.AnakPanah, 2), S(I.KainBatik, 1), S(I.Damar, 1)),
                    R(8, I.ZirahDaun, 1, I.None, S(I.DaunPisang, 5)),
                    R(9, I.ZirahBambu, 1, I.GolokPusaka, S(I.Bambu, 2), S(I.TaliIjuk, 1)),
                    R(10, I.ZirahTulang, 1, I.None, S(I.Tulang, 4), S(I.TaliIjuk, 1)),
                    R(11, I.JamuHerbal, 1, I.None, S(I.DaunSirih, 1), S(I.KunyitObat, 1)),
                    R(12, I.JamuEnergi, 1, I.GolokPusaka, S(I.JaheHutan, 1), S(I.KelapaMuda, 1)),
                    R(13, I.Ketapel, 1, I.None, S(I.Ranting, 1), S(I.KulitHewan, 1), S(I.TaliIjuk, 1)),
                    R(14, I.BomBambuMinyak, 1, I.None, S(I.Bambu, 1), S(I.MinyakTanah, 1), S(I.KainBatik, 1)),
                    R(15, I.PeluruLocok, 3, I.None, S(I.LogamBekas, 1), S(I.MesiuHitam, 1)),
                    R(16, I.ZirahBaja, 1, I.None, S(I.LogamBekas, 4), S(I.KulitHewan, 1), S(I.TaliIjuk, 1)),
                    R(17, I.PakuBesiKuning, 1, I.None, S(I.LogamBekas, 1), S(I.Kemenyan, 1)),
                    R(18, I.PerbanKain, 2, I.None, S(I.KainBatik, 1)),
                    R(19, I.TombakLogam, 1, I.GolokPusaka, S(I.TombakBambu, 1), S(I.LogamBekas, 1), S(I.TaliIjuk, 1)),
                    R(20, I.BusurTanduk, 1, I.None, S(I.BusurBambu, 1), S(I.Tulang, 3), S(I.TaliIjuk, 1)),
                    R(21, I.PanahTulang, 4, I.GolokPusaka, S(I.Ranting, 2), S(I.Tulang, 1), S(I.Bulu, 2)),
                    R(22, I.PanahRacun, 2, I.None, S(I.AnakPanah, 2), S(I.JamurBeracun, 1)),
                    R(23, I.PeluruSenapan, 2, I.None, S(I.LogamBekas, 2), S(I.MesiuHitam, 1)),
                    R(24, I.BotolKosong, 1, I.None, S(I.Bambu, 2), S(I.Damar, 1)),
                    R(25, I.KapakBaja, 1, I.None, S(I.KapakBatu, 1), S(I.LogamBekas, 2), S(I.TaliIjuk, 1)),
                    R(26, I.Mandau, 1, I.None, S(I.GolokPusaka, 1), S(I.LogamBekas, 2), S(I.Tulang, 2)),
                    R(27, I.JamuHerbal, 1, I.None, S(I.DaunSirih, 2), S(I.JaheHutan, 1)),
                    R(28, I.PerbanKain, 2, I.None, S(I.DaunPisang, 2), S(I.SeratIjuk, 2)),
                    R(29, I.SupHutan, 1, I.None, S(I.Jamur, 2), S(I.KelapaMuda, 1), S(I.DagingMatang, 1)),
                    R(30, I.OborBambu, 1, I.None, S(I.Ranting, 2), S(I.KainBatik, 1), S(I.Damar, 1)),
                    R(31, I.AnakPanah, 4, I.None, S(I.Bambu, 1), S(I.Ranting, 2), S(I.Batu, 1))
                };

                for (int index = 0; index < rows.Length; index++)
                {
                    CraftingRecipe recipe = rows[index];
                    if (Recipes.ContainsKey(recipe.recipeId))
                    {
                        Debug.LogError(
                            "[AK][RecipeDatabase] ID resep ganda: " + recipe.recipeId);
                        continue;
                    }

                    if (!Valid(recipe))
                    {
                        Debug.LogError(
                            "[AK][RecipeDatabase] Data resep tidak sah: " + recipe.recipeId);
                        continue;
                    }

                    Recipes.Add(recipe.recipeId, recipe);
                    Ordered.Add(recipe);
                }
            }
            catch (Exception error)
            {
                Debug.LogError(
                    "[AK][RecipeDatabase] Inisialisasi gagal: " + error.Message);
            }
        }

        public static CraftingRecipe? Get(int recipeId)
        {
            CraftingRecipe recipe;
            if (Recipes.TryGetValue(recipeId, out recipe))
                return Copy(recipe);

            return null;
        }

        // Hanya memeriksa bahan, tanpa mengonsumsi isiTikar.
        // Pemanggil memeriksa requiredTool, kapasitas hasil, dan stasiun.
        // R17 memerlukan api unggun dan proses 20 detik di sistem crafting.
        // Panggil setelah isi tikar berubah, bukan dari Update setiap frame.
        public static List<CraftingRecipe> CraftableFrom(IList<ItemStack> isiTikar)
        {
            var result = new List<CraftingRecipe>();
            if (isiTikar == null || isiTikar.Count == 0)
                return result;

            var totals = new Dictionary<I, long>();

            for (int index = 0; index < isiTikar.Count; index++)
            {
                ItemStack stack = isiTikar[index];
                if (stack.count < JumlahMinimum
                    || stack.id == I.None
                    || !ItemDatabase.Has(stack.id))
                {
                    continue;
                }

                long count;
                totals.TryGetValue(stack.id, out count);
                totals[stack.id] = count + stack.count;
            }

            for (int index = 0; index < Ordered.Count; index++)
            {
                CraftingRecipe recipe = Ordered[index];
                if (!HasIngredients(recipe, totals))
                    continue;

                CraftingRecipe resolved = Copy(recipe);
                resolved.inputs = ResolveInputs(recipe.inputs, totals);
                result.Add(resolved);
            }

            return result;
        }

        private static bool HasIngredients(
            CraftingRecipe recipe,
            Dictionary<I, long> totals)
        {
            // Valid() menjamin satu entri per jenis bahan.
            for (int index = 0; index < recipe.inputs.Length; index++)
            {
                ItemStack input = recipe.inputs[index];
                long available = Count(totals, input.id);

                if (input.id == I.KainBatik)
                    available += Count(totals, I.KainKafan);

                if (available < input.count)
                    return false;
            }

            return true;
        }

        private static ItemStack[] ResolveInputs(
            ItemStack[] inputs,
            Dictionary<I, long> totals)
        {
            var resolved = new List<ItemStack>(inputs.Length);

            for (int index = 0; index < inputs.Length; index++)
            {
                ItemStack input = inputs[index];
                if (input.id != I.KainBatik)
                {
                    resolved.Add(input);
                    continue;
                }

                int batik = (int)Math.Min(
                    (long)input.count, Count(totals, I.KainBatik));
                int kafan = input.count - batik;

                if (batik > 0)
                    resolved.Add(S(I.KainBatik, batik));

                if (kafan > 0)
                    resolved.Add(S(I.KainKafan, kafan));
            }

            return resolved.ToArray();
        }

        private static long Count(Dictionary<I, long> totals, I id)
        {
            long count;
            return totals.TryGetValue(id, out count) ? count : 0L;
        }

        private static bool Valid(CraftingRecipe recipe)
        {
            if (recipe.recipeId < JumlahMinimum
                || recipe.output.id == I.None
                || recipe.output.count < JumlahMinimum
                || !ItemDatabase.Has(recipe.output.id)
                || recipe.inputs == null
                || recipe.inputs.Length == 0)
            {
                return false;
            }

            if (recipe.requiredTool != I.None
                && !ItemDatabase.Has(recipe.requiredTool))
            {
                return false;
            }

            var seen = new HashSet<I>();
            for (int index = 0; index < recipe.inputs.Length; index++)
            {
                ItemStack input = recipe.inputs[index];
                if (input.id == I.None
                    || input.count < JumlahMinimum
                    || !ItemDatabase.Has(input.id))
                {
                    return false;
                }

                I key = input.id == I.KainKafan ? I.KainBatik : input.id;
                if (!seen.Add(key))
                    return false;
            }

            return true;
        }

        private static CraftingRecipe Copy(CraftingRecipe source)
        {
            CraftingRecipe copy = source;
            copy.inputs = source.inputs == null
                ? Array.Empty<ItemStack>()
                : (ItemStack[])source.inputs.Clone();
            return copy;
        }

        private static ItemStack S(I id, int count)
        {
            return new ItemStack { id = id, count = count };
        }

        private static CraftingRecipe R(
            int recipeId,
            I output,
            int outputCount,
            I requiredTool,
            params ItemStack[] inputs)
        {
            return new CraftingRecipe
            {
                recipeId = recipeId,
                inputs = inputs,
                output = S(output, outputCount),
                requiredTool = requiredTool,
                advanced = recipeId >= ResepLanjutanPertama
            };
        }
    }
}
