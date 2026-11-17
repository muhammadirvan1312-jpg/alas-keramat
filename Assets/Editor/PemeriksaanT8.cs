using System;
using System.Collections.Generic;
using AlasKeramat.AI;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using D = AlasKeramat.AI.KangDedenKelvinAI;
using N = AlasKeramat.AI.NingsihVirginiaAI;

namespace AlasKeramat.EditorTools
{
    // Self-test T8 (12 T8): 15 perintah punya handler & alasan menolak, syarat & jadwal buka, teleport tak terlihat kamera,
    // simulasi Trust (tahap, tabel kejadian, batas per perjumpaan), pagar keliling, hadiah, kelengkapan scene.
    public static class PemeriksaanT8
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            var set = new HashSet<CompanionOrder>(); bool ok = D.Daftar.Length == 15;
            foreach (var d in D.Daftar) ok &= set.Add(d.id) && d.teks.Length > 0 && d.halaman.Length > 0;
            for (int i = 0; i < 15; i++) ok &= set.Contains((CompanionOrder)i);
            var kosong = new D.KonteksKemah(); string alasan;
            foreach (var d in D.Daftar) if (!D.Syarat(d.id, kosong, out alasan)) ok &= alasan.Length > 0;
            cek("15 perintah Kang Deden lengkap; tiap penolakan punya alasan", ok);
            var penuh = new D.KonteksKemah { kemah = true, api = true, rak = true, penampung = true, cetakBiru = true, strukturRusak = true, kayuRak = 12 };
            bool syarat = D.Syarat(CompanionOrder.Ikuti, kosong, out alasan) && !D.Syarat(CompanionOrder.AmbilKayu, kosong, out alasan) && alasan == "Belum ada kemah."
                && !D.Syarat(CompanionOrder.IsiAir, kosong, out alasan) && alasan == "Belum ada Penampung Hujan.";
            foreach (var d in D.Daftar) syarat &= D.Syarat(d.id, penuh, out alasan);
            var kurang = penuh; kurang.kayuRak = 11; syarat &= !D.Syarat(CompanionOrder.BangunPagarKeliling, kurang, out alasan);
            var rakPenuh = penuh; rakPenuh.rakPenuh = true; syarat &= !D.Syarat(CompanionOrder.IsiRakKayu, rakPenuh, out alasan);
            cek("Syarat perintah: kemah, penampung, rak, cetak biru, struktur rusak, kayu pagar 12", syarat);
            bool buka = D.Terbuka(CompanionOrder.Ikuti, true, false, false) && !D.Terbuka(CompanionOrder.Ikuti, false, false, false)
                && !D.Terbuka(CompanionOrder.AmbilKayu, true, false, false) && D.Terbuka(CompanionOrder.AmbilKayu, true, true, false)
                && !D.Terbuka(CompanionOrder.SelesaikanBangunan, true, true, false) && D.Terbuka(CompanionOrder.SelesaikanBangunan, true, true, true);
            cek("Jadwal buka perintah: M02 (umum), M04 (kumpul dasar), M05 (semua)", buka);
            bool tp = CompanionDasar.BolehTeleport(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, -25f)) && !CompanionDasar.BolehTeleport(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 25f))
                && !CompanionDasar.BolehTeleport(Vector3.zero, Vector3.forward, new Vector3(10f, 0f, 25f));
            Vector3 ikut = CompanionDasar.TitikIkuti(Vector3.zero, Vector3.forward, -1f);
            cek("Teleport hanya di luar pandangan kamera; titik ikuti 4 m di belakang-kiri", tp && ikut.z < -3f && ikut.x < 0f);
            float didapat; float t = N.NaikPerjumpaan(10f, 0f, N.NaikTahap1, N.MaksPerjumpaan1, 20f, out didapat);
            bool trust = N.Tahap(30f) == 1 && N.Tahap(31f) == 2 && N.Tahap(69.9f) == 2 && N.Tahap(70f) == 3 && Mathf.Approximately(t, 25f) && Mathf.Approximately(didapat, 15f)
                && Mathf.Approximately(N.Terapkan(50f, N.KejadianTrust.DipukulAndi), 10f) && Mathf.Approximately(N.Terapkan(98f, N.KejadianTrust.BeriMakanan), 100f)
                && Mathf.Approximately(N.Terapkan(1f, N.KejadianTrust.TakBertemuSehari), 0f) && N.Delta(N.KejadianTrust.BunuhSekteDiDepan) == 5f && N.Delta(N.KejadianTrust.SerangYangMenyerah) == -5f
                && N.AndiTenang(true, true, 0f, 10f) && !N.AndiTenang(false, true, 0f, 10f) && !N.AndiTenang(true, true, 2f, 10f);
            cek("Simulasi Trust Ningsih: tahap 31/70, +2/s maks 15 per perjumpaan, tabel kejadian 08 §3.3", trust);
            bool pagar = true; float minSela = 999f, maksSela = 0f;
            for (int i = 0; i < D.TiangPagar; i++)
            {
                Vector3 p = D.PosisiTiangPagar(Vector3.zero, i, 0f);
                pagar &= Mathf.Abs(p.magnitude - D.RadiusPagar) < 0.01f;
                Vector3 q = D.PosisiTiangPagar(Vector3.zero, (i + 1) % D.TiangPagar, 0f);
                float sela = Vector3.Distance(p, q); minSela = Mathf.Min(minSela, sela); maksSela = Mathf.Max(maksSela, sela);
            }
            cek("Pagar keliling: 24 tiang r 12 m dengan celah gerbang", pagar && maksSela > minSela * 2.5f);
            ItemID id; int n; N.IsiHadiah(0.1f, out id, out n); bool hadiah = id == ItemID.DaunSirih && n == 2;
            N.IsiHadiah(0.95f, out id, out n); hadiah &= id == ItemID.Kemenyan && n == 1;
            cek("Hadiah harian Ningsih (sirih x2 / kelapa / ikan x2 / kemenyan)", hadiah);
            var deden = GameObject.Find("KangDeden"); var ningsih = GameObject.Find("Ningsih");
            cek("Kang Deden & Ningsih: AI + CharacterController", deden != null && deden.GetComponent<D>() != null && deden.GetComponent<CharacterController>() != null
                && ningsih != null && ningsih.GetComponent<N>() != null && ningsih.GetComponent<CharacterController>() != null);
            var buku = Object.FindFirstObjectByType<AlasKeramat.UI.BukuCatatanUI>(FindObjectsInactive.Include);
            var menu = Object.FindFirstObjectByType<AlasKeramat.UI.MenuNingsihUI>(FindObjectsInactive.Include);
            bool ui = buku != null && menu != null;
            if (ui) { ui &= new SerializedObject(buku).FindProperty("tombol").arraySize == 15 && new SerializedObject(menu).FindProperty("tombol").arraySize == 7; }
            cek("Buku Catatan (15 tombol) & menu Ningsih (7 tombol)", ui);
        }
    }
}
