using System.Collections.Generic;
using AlasKeramat.Building;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // 16 §6.1 (T9): 20 Catatan Kang Deden - ia tuli, jadi hanya menulis yang ia LIHAT. Tiap pagi 06:00 satu catatan baru (yang syaratnya terpenuhi)
    // ditinggalkan di dekat api kemah: "BACA CATATAN DEDEN". Terbaca tersimpan.
    public partial class KangDedenKelvinAI
    {
        public struct KonteksCatatan { public int hari, bangunan, sekteDibunuh, sekteDiampuni, hantuDiusir; public float trustNingsih, kemarahan; public bool malamDarahLewat, pakuDidapat, ningsihBertemu, hujanBadai, lukaParah; }
        public struct DataCatatan { public string teks; public System.Func<KonteksCatatan, bool> syarat; }

        public static readonly DataCatatan[] Catatan =
        {
            C("Andi, telinga saya berdenging terus. Tapi saya masih bisa kerja. Tulis saja apa yang harus saya lakukan.", k => k.hari >= 2),
            C("Tadi pagi saya lihat orang bertopeng di seberang sungai. Dia diam lama sekali melihat kemah kita.", k => k.kemarahan >= 15f),
            C("Burung-burung kabur dari pohon besar semalam. Ada yang tidak beres di sana.", k => k.hari >= 3),
            C("Gadis itu menaruh ikan di dekat api waktu kamu tidur. Saya pura-pura tidak lihat.", k => k.trustNingsih >= 40f),
            C("Atap bocor di sisi kiri. Sudah saya tambal pakai daun.", k => k.bangunan >= 2 && k.hujanBadai),
            C("Saya mimpi anak-anak saya di Garut. Dewi sudah bisa naik sepeda.", k => k.hari >= 4),
            C("Malam tadi ada yang berdiri di pinggir kemah. Saya kira kamu. Waktu saya dekati, tidak ada siapa-siapa.", k => k.hantuDiusir >= 1 && k.hari >= 3),
            C("Saya tidak mau ke hutan malam-malam lagi. Maaf, Andi.", k => k.malamDarahLewat),
            C("Kalau kita pulang, saya traktir kamu sate maranggi.", k => k.hari >= 5),
            C("Tanda di lengan gadis itu menyala waktu Pocong datang. Dia tahu lebih dulu dari kita.", k => k.trustNingsih >= 60f && k.hantuDiusir >= 1),
            C("Kemah kita makin besar. Bapak saya dulu tukang bangunan. Dia pasti bangga lihat tiang-tiang ini.", k => k.bangunan >= 5),
            C("Kamu kelihatan capek sekali kemarin. Makan dulu, Andi. Kerja bisa nanti.", k => k.lukaParah),
            C("Orang bertopeng itu berlutut di depanmu, lalu lari. Saya lihat kamu biarkan dia pergi. Kamu orang baik.", k => k.sekteDiampuni >= 1),
            C("Banyak darah di bajumu tadi. Saya cuci di sungai. Jangan tanya darah siapa, saya juga tidak mau tahu.", k => k.sekteDibunuh >= 3),
            C("Paku itu dingin sekali waktu saya pegang. Simpan baik-baik, ya.", k => k.pakuDidapat),
            C("Gadis itu tersenyum waktu saya kasih jempol. Pertama kali saya lihat dia senyum.", k => k.trustNingsih >= 70f),
            C("Langit merah semalam. Saya peluk golok sampai pagi. Tidak tidur sama sekali.", k => k.malamDarahLewat && k.hari >= 4),
            C("Saya sudah ukir patung kecil buat Dewi. Belum selesai, tapi matanya sudah mirip dia.", k => k.hari >= 6),
            C("Ada asap dapur dari desa di timur tiap pagi. Mereka manusia juga, Andi. Atau dulu manusia.", k => k.kemarahan >= 30f),
            C("Hari ini kita masih hidup. Itu sudah cukup. Besok kita usaha lagi.", k => k.hari >= 8),
        };

        static DataCatatan C(string t, System.Func<KonteksCatatan, bool> s) { return new DataCatatan { teks = t, syarat = s }; }

        readonly HashSet<int> _catatanTerbit = new HashSet<int>();
        int _hariCatatan = -1;

        public static int PilihCatatan(KonteksCatatan k, ICollection<int> sudah)
        {
            for (int i = 0; i < Catatan.Length; i++) if (!sudah.Contains(i) && Catatan[i].syarat(k)) return i;
            return -1;
        }

        KonteksCatatan IsiKonteksCatatan()
        {
            var k = new KonteksCatatan { hari = _jam != null ? _jam.Hari : 1 };
            var dir = GameServices.Get<EnemyDirector>();
            if (dir != null) { k.kemarahan = dir.Kemarahan; k.sekteDibunuh = dir.SekteDibunuh; k.sekteDiampuni = dir.SekteDiampuni; k.hantuDiusir = dir.HantuDiusir; k.malamDarahLewat = k.hari > EnemyDirector.HariMalamDarahPertama; }
            var n = GameServices.Get<NingsihVirginiaAI>(); if (n != null) k.trustNingsih = n.Trust;
            k.bangunan = BagianStruktur.Semua.Count / 6 + FungsiBangunan.Semua.Count;
            var inv = GameServices.Get<AlasKeramat.Inventory.TikarPandan3DInventory>(); k.pakuDidapat = inv != null && (inv.Punya(ItemID.PakuKeramat1) || inv.Punya(ItemID.PakuBesiKuning));
            var c = GameServices.Get<URPAtmosphereAndWeather>(); k.hujanBadai = c != null && c.Cuaca == WeatherState.HujanBadai;
            var v = GameServices.Get<AlasKeramat.Player.AndiVitalsAndSanity>(); k.lukaParah = v != null && v.Health < 40f;
            return k;
        }

        // Dipanggil dari Tick: pukul 06:00 tiap hari menulis satu catatan baru di dekat api.
        void TickCatatan()
        {
            if (_jam == null || !Bergabung || Gugur || _jam.Jam < 6f || _jam.Jam >= 7f || _hariCatatan == _jam.Hari) return;
            _hariCatatan = _jam.Hari;
            int i = PilihCatatan(IsiKonteksCatatan(), _catatanTerbit);
            if (i < 0) return;
            _catatanTerbit.Add(i);
            Vector3? kemah = Kemah();
            Vector3 p = kemah.HasValue ? kemah.Value + new Vector3(-1.2f, 0f, -1.4f) : transform.position + transform.forward;
            CatatanDeden.Taruh(p, i, matKayu);
        }
    }
}
