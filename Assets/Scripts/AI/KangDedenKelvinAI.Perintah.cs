using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // 08 §2.3-2.4: data 15 perintah Buku Catatan (halaman, teks, syarat, alasan menolak) + jadwal pembukaan (M02 / M04 / M05).
    public partial class KangDedenKelvinAI
    {
        public struct DataPerintah { public CompanionOrder id; public string halaman, teks, alasan; public int buka; } // buka: 0 = M02, 1 = M04, 2 = M05

        public static readonly DataPerintah[] Daftar =
        {
            P(CompanionOrder.Ikuti, "Umum", "Ikuti Saya", "", 0),
            P(CompanionOrder.Tunggu, "Umum", "Tunggu di Sini", "", 0),
            P(CompanionOrder.Istirahat, "Umum", "Istirahat", "", 0),
            P(CompanionOrder.PulangKeKemah, "Umum", "Pulang ke Kemah", "Belum ada kemah.", 1),
            P(CompanionOrder.AmbilKayu, "Kumpulkan", "Ambil Kayu", "Belum ada kemah.", 1),
            P(CompanionOrder.AmbilRanting, "Kumpulkan", "Ambil Ranting", "Belum ada kemah.", 1),
            P(CompanionOrder.AmbilBatu, "Kumpulkan", "Ambil Batu", "Belum ada kemah.", 1),
            P(CompanionOrder.CariMakanan, "Kumpulkan", "Cari Makanan", "", 2),
            P(CompanionOrder.IsiAir, "Kumpulkan", "Isi Air", "Belum ada Penampung Hujan.", 2),
            P(CompanionOrder.IsiRakKayu, "Kemah", "Isi Rak Kayu", "Rak penuh / belum ada Rak.", 1),
            P(CompanionOrder.SelesaikanBangunan, "Kemah", "Selesaikan Bangunan", "Tidak ada cetak biru.", 2),
            P(CompanionOrder.JagaApi, "Kemah", "Jaga Api", "Belum ada api unggun.", 1),
            P(CompanionOrder.BersihkanKemah, "Kemah", "Bersihkan Kemah", "Belum ada kemah.", 2),
            P(CompanionOrder.PerbaikiBangunan, "Kemah", "Perbaiki Bangunan", "Semua bangunan utuh.", 2),
            P(CompanionOrder.BangunPagarKeliling, "Kemah", "Bangun Pagar Keliling", "Bahan di Rak tidak cukup.", 2),
        };

        static DataPerintah P(CompanionOrder id, string h, string t, string a, int b) { return new DataPerintah { id = id, halaman = h, teks = t, alasan = a, buka = b }; }

        public static DataPerintah Data(CompanionOrder o) { for (int i = 0; i < Daftar.Length; i++) if (Daftar[i].id == o) return Daftar[i]; return Daftar[0]; }

        // Keadaan kemah yang menentukan syarat perintah (diisi dari dunia; murni untuk self-test).
        public struct KonteksKemah { public bool kemah, api, rak, rakPenuh, penampung, cetakBiru, strukturRusak; public int kayuRak; }

        public const int KayuPagar = 12;

        public static bool Syarat(CompanionOrder o, KonteksKemah k, out string alasan)
        {
            alasan = Data(o).alasan;
            switch (o)
            {
                case CompanionOrder.Ikuti: case CompanionOrder.Tunggu: case CompanionOrder.Istirahat: case CompanionOrder.CariMakanan: return true;
                case CompanionOrder.PulangKeKemah: case CompanionOrder.AmbilKayu: case CompanionOrder.AmbilRanting: case CompanionOrder.AmbilBatu: case CompanionOrder.BersihkanKemah: return k.kemah;
                case CompanionOrder.IsiAir: return k.penampung;
                case CompanionOrder.IsiRakKayu: return k.rak && !k.rakPenuh;
                case CompanionOrder.SelesaikanBangunan: return k.cetakBiru;
                case CompanionOrder.JagaApi: return k.api;
                case CompanionOrder.PerbaikiBangunan: return k.strukturRusak;
                case CompanionOrder.BangunPagarKeliling: return k.kemah && k.rak && k.kayuRak >= KayuPagar;
            }
            return false;
        }

        public static bool Terbuka(CompanionOrder o, bool m02, bool m04, bool m05)
        {
            int b = Data(o).buka;
            return b == 0 ? m02 : b == 1 ? m04 || m05 : m05;
        }
    }
}
