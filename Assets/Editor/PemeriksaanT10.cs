using System;
using System.Collections.Generic;
using AlasKeramat.AI;
using AlasKeramat.Core;
using UnityEngine;
using Object = UnityEngine.Object;
using Q = AlasKeramat.Core.QuestManager;

namespace AlasKeramat.EditorTools
{
    // Self-test T10 (12 T10): 13 misi berurutan dengan gerbang 04 §4 (simulasi bendera), 12 lore tidak kosong & #1-#6 sesuai,
    // 3 kompleks punya pintu masuk/keluar valid, siege 60 s memunculkan gelombang 06 §11, fase boss 06 §10.
    public static class PemeriksaanT10
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            // --- Misi ---
            var p = new Q.ProgresMisiT6();
            var urut = new List<QuestID>();
            p.koper.Add("a"); p.koper.Add("b"); p.koper.Add("c"); p.jamu = p.catatan = p.saung = p.api = true; urut.AddRange(Q.Majukan(p));
            p.malam = p.pagi = true; urut.AddRange(Q.Majukan(p));
            p.cangkul = true; p.lore |= 2; urut.AddRange(Q.Majukan(p));
            cek("M05 butuh pemicu (Lore #1 / Hari 2) sebelum selesai", !p.selesai[4]);
            p.lore |= 1; urut.AddRange(Q.Majukan(p));
            cek("M05 selesai -> semua perintah Deden (m05)", p.selesai[4] && p.m05);
            p.kartu = p.rebreather = true; p.paku |= 1; p.zipline = true; p.keluarBunker = true; p.paku |= 2;
            urut.AddRange(Q.Majukan(p));
            cek("M09 terkunci sebelum M07 (Lore #3)", !p.selesai[8]);
            p.lore |= 4; urut.AddRange(Q.Majukan(p));
            p.sisiUtara = true; urut.AddRange(Q.Majukan(p));
            cek("M11 butuh boss kalah + Paku 3", !p.selesai[10]);
            p.bossKalah = true; p.paku |= 4; urut.AddRange(Q.Majukan(p));
            p.tertancap = 3; cek("M12 menunggu siege selesai", !Q.Syarat(p, QuestID.M12));
            p.siegeSelesai = true; urut.AddRange(Q.Majukan(p));
            p.ningsihTerlihat = true; p.trust31 = true; urut.AddRange(Q.Majukan(p));
            p.ending = 1; urut.AddRange(Q.Majukan(p));
            bool semua = urut.Count == 13; for (int i = 0; i < 13; i++) semua &= p.selesai[i];
            cek("13 misi selesai berurutan tanpa event ganda (" + urut.Count + ")", semua && urut.IndexOf(QuestID.M07) < urut.IndexOf(QuestID.M09) && urut.IndexOf(QuestID.M10) < urut.IndexOf(QuestID.M11) && urut.IndexOf(QuestID.M12) < urut.IndexOf(QuestID.M13));
            var awal = new Q.ProgresMisiT6();
            cek("Misi awal: HUD M01, M06/M13 belum aktif", Q.Prioritas(awal) == 1 && !Q.Mulai(awal, QuestID.M06) && !Q.Mulai(awal, QuestID.M13));
            bool teks = true; for (int i = 1; i <= 13; i++) teks &= !string.IsNullOrEmpty(Q.TeksMisi(awal, i));
            cek("Teks HUD 13 misi ada", teks);

            // --- Lore ---
            bool lore = true; for (int i = 1; i <= LoreLibrary.Jumlah; i++) lore &= LoreLibrary.Judul((LoreID)i).Length > 3 && LoreLibrary.Isi((LoreID)i).Length > 80;
            cek("12 lore: judul & isi tidak kosong", lore);
            cek("Lore #1-#6 sesuai 04 §6.1", LoreLibrary.Isi(LoreID.L01).Contains("struktur logam peninggalan Hindia Belanda tahun 1928")
                && LoreLibrary.Isi(LoreID.L03).Contains("BUNKER BELANDA TERKUBUR DI BAWAH TANAH GEMBUR BARAT LAUT")
                && LoreLibrary.Isi(LoreID.L06).Contains("dipertahankan melewati Malam Darah") && LoreLibrary.HitungDitemukan(0xFFF) == 12);

            // --- Kompleks bawah tanah ---
            var pintu = Object.FindObjectsByType<PintuGua>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int u = 1; u <= 3; u++)
            {
                var id = (UndergroundId)u;
                bool masuk = false, keluar = false;
                foreach (var pg in pintu)
                {
                    bool asalBawah = WorldTerrainGenerator.DiBawahTanah(pg.transform.position.y), tujuanBawah = WorldTerrainGenerator.DiBawahTanah(pg.Tujuan.y);
                    if (!asalBawah && tujuanBawah && WorldCaveAndSaveManager.KompleksDi(pg.Tujuan) == id) masuk = true;
                    if (asalBawah && !tujuanBawah && WorldCaveAndSaveManager.KompleksDi(pg.transform.position) == id) keluar = true;
                }
                cek("Kompleks " + id + " punya pintu masuk & keluar", masuk && keluar);
            }
            var mgr = Object.FindFirstObjectByType<WorldCaveAndSaveManager>();
            cek("WorldCaveAndSaveManager ada", mgr != null);
            cek("Air gua terdaftar (kolam R3, R4, terowongan U1)", Object.FindObjectsByType<AirGua>(FindObjectsSortMode.None).Length >= 7);
            cek("Gerbang: gali, kartu, reruntuhan, angin, zipline, akar", Object.FindFirstObjectByType<TanahGali>() != null && Object.FindFirstObjectByType<PintuKartu>() != null
                && Object.FindFirstObjectByType<RintanganDorong>() != null && Object.FindFirstObjectByType<ZonaAngin>() != null
                && Object.FindObjectsByType<JangkarZipline>(FindObjectsSortMode.None).Length == 2 && Object.FindFirstObjectByType<AkarBakar>() != null);
            cek("Reruntuhan T2 hanya didorong dari sisi R4", RintanganDorong.BolehDari(Vector3.zero, Vector3.right, new Vector3(2f, 0f, 0f)) && !RintanganDorong.BolehDari(Vector3.zero, Vector3.right, new Vector3(-2f, 0f, 0f)));
            cek("KompleksDi: permukaan None, (-600,-80,-600) UG1", WorldCaveAndSaveManager.KompleksDi(new Vector3(0f, 10f, 0f)) == UndergroundId.None && WorldCaveAndSaveManager.KompleksDi(new Vector3(-590f, -80f, -610f)) == UndergroundId.UG1_GuaSungai);

            // --- Boss & siege ---
            cek("Fase boss 06 §10: 100-60% = 1, 60-25% = 2, < 25% = 3", BossGenderuwo.FaseUntuk(1f) == 1 && BossGenderuwo.FaseUntuk(0.61f) == 1 && BossGenderuwo.FaseUntuk(0.5f) == 2 && BossGenderuwo.FaseUntuk(0.2f) == 3);
            cek("Titik lemah punggung hanya dari belakang", BossGenderuwo.DariBelakang(Vector3.forward, Vector3.forward) && !BossGenderuwo.DariBelakang(Vector3.forward, Vector3.back));
            cek("Teka-teki Akar (Damai): urutan I-II-III-IV", KiRogoPanggung.UrutanBenar(0, 1) && !KiRogoPanggung.UrutanBenar(0, 2) && KiRogoPanggung.UrutanBenar(3, 4));
            var jatuh = new List<int>();
            int total = 0;
            float lalu = -1f;
            for (float t = 0f; t <= PenjadwalSiege.Lama; t += 0.5f) { PenjadwalSiege.Jatuh(lalu, t, jatuh); foreach (int i in jatuh) total += PenjadwalSiege.Tabel[i].jenis == EnemyArchetype.KiRogo ? 0 : PenjadwalSiege.Tabel[i].jumlah; lalu = t; }
            cek("Siege: semua gelombang muncul tepat sekali (" + total + " musuh)", total == PenjadwalSiege.TotalMusuh() && total == 16);
            cek("Siege 06 §11: 0 s 4 Abdi + 1 Pemimpin, 15 s 3 Pocong, 25 s 2 Wanita + 1 Buto, 35 s Kunti, 45 s 2 Banaspati + 2 Pocong",
                PenjadwalSiege.JumlahPada(0f, EnemyArchetype.AbdiSekte) == 4 && PenjadwalSiege.JumlahPada(0f, EnemyArchetype.PemimpinPatroli) == 1
                && PenjadwalSiege.JumlahPada(15f, EnemyArchetype.PocongHutan) == 3 && PenjadwalSiege.JumlahPada(25f, EnemyArchetype.AbdiWanita) == 2
                && PenjadwalSiege.JumlahPada(25f, EnemyArchetype.AbdiButo) == 1 && PenjadwalSiege.JumlahPada(35f, EnemyArchetype.Kuntilanak) == 1
                && PenjadwalSiege.JumlahPada(40f, EnemyArchetype.KiRogo) == 1 && PenjadwalSiege.JumlahPada(45f, EnemyArchetype.Banaspati) == 2 && PenjadwalSiege.JumlahPada(45f, EnemyArchetype.PocongHutan) == 2);
            Vector3 s0 = PenjadwalSiege.Titik(Vector3.zero, 0, 0f), s3 = PenjadwalSiege.Titik(Vector3.zero, 3, 1f);
            cek("Titik spawn siege di cincin 35-45 m", Mathf.Abs(s0.magnitude - 35f) < 0.01f && Mathf.Abs(s3.magnitude - 45f) < 0.01f);
            cek("Arena: Genderuwo, Ki Rogo, 8 tungku, altar ritual", Object.FindFirstObjectByType<BossGenderuwo>() != null && Object.FindFirstObjectByType<KiRogoPanggung>() != null
                && Object.FindObjectsByType<TungkuObor>(FindObjectsSortMode.None).Length == 8 && Object.FindFirstObjectByType<RitualAltar>() != null);
            cek("Malam Darah tambahan: malam berikutnya setelah paku", EnemyDirector.MalamBerikutnya(3, 10f) == 3 && EnemyDirector.MalamBerikutnya(3, 21f) == 4 && EnemyDirector.MalamBerikutnya(4, 2f) == 4);
        }
    }
}
