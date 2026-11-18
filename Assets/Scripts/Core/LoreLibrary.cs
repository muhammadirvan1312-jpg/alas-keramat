namespace AlasKeramat.Core
{
    // 12 dokumen lore (04 §6). #1-#6 disalin persis dari spesifikasi v1 (04 §6.1); #7-#12 dari 04 §6.2.
    public static class LoreLibrary
    {
        public const int Jumlah = 12;

        static readonly string[] _judul =
        {
            "Surat Tugas Andi Pratama",
            "Catatan Kepala Desa (1998)",
            "Pesan Darah di Papan Kayu",
            "Jurnal Tim Ekspedisi Pertama",
            "Arsip Oost-Indische Onderzoek (1928) & Laporan PT Bhumi Cakra",
            "Wasiat Juru Kunci (Ayah Ningsih)",
            "Buku Jaga Penjaga Mercusuar (1974)",
            "Surat yang Tak Pernah Dikirim",
            "Log Laboratorium - Subjek N",
            "Daftar Tumbal (tulisan tangan Ki Rogo)",
            "Pesan Terakhir Rendra",
            "Prasasti Batu Langit",
        };

        static readonly string[] _isi =
        {
            "Surat Tugas Andi Pratama - Peringatan: Citra termal menunjukkan struktur logam peninggalan Hindia Belanda tahun 1928 di bawah perkebunan sawit barat dan anomali elektromagnetik di titik pusat pulau.",
            "Catatan Kepala Desa (1998) - Sejak perusahaan membuka lahan sawit dan mencabut Paku Besi Kuning dari akar Beringin Agung, warga tidak bisa tidur. Setiap Maghrib, suara kentongan berbunyi sendiri dari hutan. Ningsih, anak juru kunci, menolak ikut ritual Ki Rogo dan lari ke lembah sungai.",
            "Pesan Darah di Papan Kayu - 'KITA YANG MENGHIRUP ASAPNYA, MEREKA YANG MEMBANGUNKAN IBLISNYA. BUNKER BELANDA TERKUBUR DI BAWAH TANAH GEMBUR BARAT LAUT.'",
            "Jurnal Tim Ekspedisi Pertama - Air di gua ini mengandung spora hitam yang memicu halusinasi akut. Kami menemukan Paku Keramat pertama di altar batu, tapi dua rekan kami diterkam sosok terbungkus kain kafan yang melompat dari kegelapan. Ambil Alat Selam dan Kartu Bunker ini jika kau masih hidup.",
            "Arsip Oost-Indische Onderzoek (1928) & Laporan PT Bhumi Cakra - Tiga Paku Keramat dari meteorit besi kuno dulunya ditancapkan oleh leluhur Nusantara untuk mengunci retakan kabut di bawah Beringin Agung. Saat ketiganya dicabut untuk penambangan, penghuni hutan kehilangan akal sehat dan berubah menjadi abdi kegelapan.",
            "Wasiat Juru Kunci (Ayah Ningsih) - Hanya ketika ketiga Paku Keramat ditancapkan kembali ke batu altar Beringin Agung dan dipertahankan melewati Malam Darah, kutukan Alas Keramat akan runtuh dan langit kembali terbuka bagi sinyal penyelamat.",
            "Malam ke-12 bulan ini lampu kembali menyala merah sendiri. Bukan dari menara - dari hutan. Kapal-kapal nelayan memutar haluan jauh sebelum karang. Kata orang desa, selama paku-paku masih tertancap, hutan hanya tidur. Aku tidak tahu paku apa yang mereka maksud. Aku hanya tahu: jangan pernah memadamkan lampu menara saat kabut datang dari utara.",
            "Bapak, aku masih di lembah. Aku makan pisang dan ikan dari sungai seperti yang Bapak ajarkan. Tanda di lenganku menyala setiap kali mereka dekat - yang berkafan, yang tertawa di pohon. Aku tidak takut lagi pada gelap. Aku takut pada orang yang membawa golok dan tidak mau menyimpannya. Kalau suatu hari ada orang asing yang mau duduk diam di dekat apiku, mungkin aku akan berani menyapanya.",
            "Hari 4. Subjek N (perempuan, 22 tahun, diserahkan oleh 'tetua' setempat) menunjukkan respons luar biasa terhadap paparan kabut cairan hitam: denyut nadi stabil, pupil bereaksi pada kegelapan total, tiga pola kulit berpendar di lengan dan bahu. Hari 6. Subjek N melarikan diri lewat saluran ventilasi. Subjek R (laki-laki, anggota tim survei) tidak melarikan diri: tulang jari memanjang, wajah tidak lagi dapat dikenali, dipindahkan ke ruang ventilasi. Rekomendasi: hentikan seluruh paparan. Rekomendasi ditolak oleh direksi.",
            "Untuk membuka gerbang sepenuhnya, darah juru kunci harus mengalir di altar pada Malam Darah. Juru kunci tua sudah menolak dan sudah dikubur bersama sumpahnya. Tinggal anak gadisnya. Para abdi, cari dia di lembah. Jangan lukai wajahnya. Hutan ingin melihatnya utuh.",
            "Kalau Andi yang menemukan ini - maaf aku tidak menunggumu, Ndi. Kau benar soal data termal itu. Mereka bukan menambang bijih, mereka menambang sesuatu yang seharusnya dibiarkan terkubur. Kartu bunker kutinggalkan di peti. Jangan masuk ke kuil di utara sebelum kau punya cahaya yang tidak bisa dipadamkan angin.",
            "Tiga paku dari batu yang jatuh dari langit, ditempa di bawah bulan purnama, ditanam di akar pohon tua. Selama ketiganya bersatu, retakan tidur dan hutan menjaga manusia. Bila dicabut, hutan lapar. Bila dikembalikan, hutan kembali menjadi rumah.",
        };

        static bool Sah(LoreID id) { int i = (int)id; return i >= 1 && i <= Jumlah; }
        public static string Judul(LoreID id) { return Sah(id) ? _judul[(int)id - 1] : ""; }
        public static string Isi(LoreID id) { return Sah(id) ? _isi[(int)id - 1] : ""; }
        public static int Bit(LoreID id) { return Sah(id) ? 1 << ((int)id - 1) : 0; }
        public static int HitungDitemukan(int mask) { int n = 0; for (int i = 0; i < Jumlah; i++) if ((mask & (1 << i)) != 0) n++; return n; }
    }
}
