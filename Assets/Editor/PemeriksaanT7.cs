using System;
using System.Collections.Generic;
using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlasKeramat.EditorTools
{
    // Self-test T7a (12 T7): statistik archetype 06 §4, SurfaceGrid 250.000 sel & >= 70% darat bisa dilalui, A* Heli -> Desa < 4.000 node,
    // maks 12 musuh aktif (simulasi spawn 30), rumus persepsi, parry/blok, keputusan Sekte, suara tempur, kelengkapan scene.
    public static class PemeriksaanT7
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            bool statOk = true;
            for (int i = 0; i < StatMusuh.JumlahInti; i++)
            {
                var a = (EnemyArchetype)i;
                var d = StatMusuh.Ambil(a);
                statOk &= StatMusuh.Ada(a) && d.jenis == a && d.hp > 0f && d.drop.Length == d.peluang.Length && d.drop.Length == d.jumlah.Length;
            }
            var abdi = StatMusuh.Ambil(EnemyArchetype.AbdiSekte); var buto = StatMusuh.Ambil(EnemyArchetype.AbdiButo);
            statOk &= abdi.hp == 80f && abdi.lari == 5.4f && abdi.lihatSiang == 35f && abdi.lihatMalam == 18f && abdi.damage == 12f && buto.hp == 300f
                && StatMusuh.Ambil(EnemyArchetype.Kuntilanak).hp == 220f && StatMusuh.Ambil(EnemyArchetype.Genderuwo).hp == 1500f && StatMusuh.Ambil(EnemyArchetype.Jenglot).hp == 15f
                && StatMusuh.Ambil(EnemyArchetype.PocongHutan).hantu && !abdi.hantu;
            cek("Statistik 13 archetype sesuai 06 §4 (Abdi 80 HP, Buto 300, Kuntilanak 220, Genderuwo 1500)", statOk);

            var teks = AssetDatabase.LoadAssetAtPath<TextAsset>(NusantaraMasterBuilder.PathGrid);
            var grid = teks != null ? SurfaceGrid.DariBytes(teks.bytes) : null;
            if (grid == null) { cek("SurfaceGrid.bytes ada & valid", false); }
            else
            {
                int darat, lewat; grid.Hitung(out darat, out lewat);
                float rasio = darat > 0 ? (float)lewat / darat : 0f;
                cek("SurfaceGrid " + grid.Data.Length + " sel, darat bisa dilalui " + (rasio * 100f).ToString("0") + "% (>= 70%)", grid.Data.Length == 250000 && rasio >= 0.7f);
                var jalur = new List<Vector3>();
                int node;
                Vector3 heli = NusantaraMasterBuilder.PosDeden, desa = EnemyDirector.PusatDesa;
                var jam = System.Diagnostics.Stopwatch.StartNew();
                bool ada = GridPathfinder.CariOtomatis(grid, heli, desa, jalur, GridPathfinder.MaksNode, out node);
                int segGagal = GridPathfinder.SegmenGagalTerakhir;
                string diag = "";
                if (!ada || node >= 4000)
                {
                    int nBebas, nW3; float simpan = GridPathfinder.BobotHeuristik;
                    bool adaBebas = GridPathfinder.Cari(grid, heli, desa, null, SurfaceGrid.JumlahSel, out nBebas);
                    GridPathfinder.BobotHeuristik = 3f; bool adaW3 = GridPathfinder.Cari(grid, heli, desa, null, SurfaceGrid.JumlahSel, out nW3);
                    GridPathfinder.BobotHeuristik = simpan;
                    diag = " [diag: tanpa batas " + adaBebas + "/" + nBebas + " node, bobot 3 " + adaW3 + "/" + nW3 + " node]";
                }
                cek("A* bertingkat Heli -> Desa ditemukan dengan " + node + " node (< 4.000), " + jalur.Count + " titik, segmen gagal " + segGagal + ", " + jam.ElapsedMilliseconds + " ms" + diag, ada && node < 4000 && jalur.Count > 0 && segGagal == 0);
                bool lurus = GridPathfinder.Cari(grid, heli, heli + new Vector3(8f, 0f, 0f), jalur, GridPathfinder.MaksNode, out node);
                cek("A* jarak pendek selesai cepat (" + node + " node)", lurus && node < 50);
            }

            var antri = new EnemyDirector.AntrianSpawn();
            antri.Tambah(30);
            int aktif = 0, maks = 0;
            for (int langkah = 0; langkah < 20; langkah++)
            {
                aktif += antri.Ambil(aktif);
                maks = Mathf.Max(maks, aktif);
                if (langkah % 3 == 2) aktif = Mathf.Max(0, aktif - 2); // sebagian mati/despawn, antrian mengisi lagi
            }
            cek("Maks musuh aktif <= 12 (simulasi spawn 30, puncak " + maks + ")", maks == EnemyDirector.MaksAktif && antri.Menunggu >= 0);
            cek("Token penyerang Normal 2 / Sulit 3", EnemyDirector.TokenNormal == 2 && EnemyDirector.TokenSulit == 3);

            const float e = 0.001f;
            bool persepsi = Mathf.Abs(NusantaraEnemyAndMutantAI.FaktorTerlihat(true, false, true, false, true, false, false) - 0.3f) < e
                && Mathf.Abs(NusantaraEnemyAndMutantAI.FaktorTerlihat(true, false, false, false, true, true, false) - 1.5f) < e
                && NusantaraEnemyAndMutantAI.FaktorTerlihat(true, true, false, false, false, false, false) == 0f
                && NusantaraEnemyAndMutantAI.FaktorTerlihat(false, false, false, false, false, false, false) == 0f
                && Mathf.Abs(NusantaraEnemyAndMutantAI.FaktorTerlihat(true, false, true, true, false, false, true) - 0.245f) < e
                && Mathf.Abs(NusantaraEnemyAndMutantAI.LajuSadar(1f, 20f) - 20f) < e && Mathf.Abs(NusantaraEnemyAndMutantAI.LajuSadar(1f, 5f) - 40f) < e
                && NusantaraEnemyAndMutantAI.LangkahKesadaran(50f, 0f, 10f, 35f, 1f) == 42f
                && NusantaraEnemyAndMutantAI.LangkahKesadaran(10f, 1f, 50f, 35f, 1f) == 2f
                && NusantaraEnemyAndMutantAI.DalamKerucut(Vector3.forward, new Vector3(1f, 0f, 1f)) && !NusantaraEnemyAndMutantAI.DalamKerucut(Vector3.forward, Vector3.back)
                && Mathf.Approximately(StatMusuh.JarakLihat(abdi, true, true), 60f) && Mathf.Approximately(StatMusuh.JarakLihat(abdi, true, false), 18f);
            cek("Persepsi 06 §5: jongkok malam 0,3; obor malam 1,5; laju 40/(jarak/10); turun 8/s; kerucut 110 derajat", persepsi);

            bool tangkis = CombatAndLeftHandController.NilaiTangkis(true, false, 10f, true) == 1 && CombatAndLeftHandController.NilaiTangkis(true, false, 50f, true) == 0
                && CombatAndLeftHandController.NilaiTangkis(false, true, 0f, true) == 2 && CombatAndLeftHandController.NilaiTangkis(true, true, 0f, false) == 2
                && Mathf.Approximately(CombatAndLeftHandController.Jendela(ItemID.KerisPusaka), 0.35f) && Mathf.Approximately(CombatAndLeftHandController.Jendela(ItemID.GolokPusaka), 0.25f)
                && Mathf.Approximately(CombatAndLeftHandController.PengaliPenghabisan, 2.5f) && ReaksiAndi.HarusJatuh(25f) && !ReaksiAndi.HarusJatuh(24f);
            cek("Parry 0,25 s (Keris 0,35) dalam kerucut 70, blok -60%, penghabisan x2,5, jatuh bila kena >= 25", tangkis);

            var K = NusantaraEnemyAndMutantAI.KeadaanSekte.Isyarat;
            bool otak = NusantaraEnemyAndMutantAI.PutuskanTerlibat(EnemyArchetype.PemimpinPatroli, false, false, 10f, 0f, 0.9f) == K
                && NusantaraEnemyAndMutantAI.PutuskanTerlibat(EnemyArchetype.AbdiSekte, false, false, 10f, 0f, 0.9f) == NusantaraEnemyAndMutantAI.KeadaanSekte.Mengintai
                && NusantaraEnemyAndMutantAI.PutuskanTerlibat(EnemyArchetype.AbdiSekte, false, true, 10f, 0f, 0.9f) == NusantaraEnemyAndMutantAI.KeadaanSekte.Serang
                && NusantaraEnemyAndMutantAI.PutuskanTerlibat(EnemyArchetype.AbdiSekte, true, true, 50f, 80f, 0.1f) == NusantaraEnemyAndMutantAI.KeadaanSekte.Gertak
                && NusantaraEnemyAndMutantAI.HarusMenyerah(0.2f, 70f, false) && !NusantaraEnemyAndMutantAI.HarusMenyerah(0.2f, 70f, true)
                && NusantaraEnemyAndMutantAI.HarusLari(75f, 1f, 0.99f) && NusantaraEnemyAndMutantAI.HarusLari(10f, 0.2f, 0.5f) && !NusantaraEnemyAndMutantAI.HarusLari(10f, 0.2f, 0.7f)
                && NusantaraEnemyAndMutantAI.LamaReaksi(EnemyArchetype.AbdiSekte, true, 0.3f) == 1.2f && NusantaraEnemyAndMutantAI.LamaReaksi(EnemyArchetype.AbdiButo, true, 0.3f) == 0.7f
                && NusantaraEnemyAndMutantAI.LamaReaksi(EnemyArchetype.AbdiSekte, false, 0.3f) == 0.25f;
            cek("Keputusan Sekte 06 §6 (isyarat pemimpin, intai, serang, gertak, menyerah, lari, reaksi kena)", otak);

            bool dengar = MeterSuara.RadiusLangkah(true, false, 5f) == 20f && MeterSuara.RadiusLangkah(false, false, 2f) == 8f && MeterSuara.RadiusLangkah(false, true, 1f) == 3f
                && MeterSuara.RadiusLangkah(false, false, 0f) == 0f && MeterSuara.Terdengar(Vector3.zero, 20f, new Vector3(0f, 0f, 40f), 45f) && !MeterSuara.Terdengar(Vector3.zero, 20f, new Vector3(0f, 0f, 30f), 20f);
            cek("Pendengaran: lari 20 / jalan 8 / jongkok 3 / tebang 45 m", dengar);

            bool suara = true; int sr;
            foreach (var k in ProceduralAudioSynth.KlipTempur) { var b = ProceduralAudioSynth.Generate(k, 0, out sr); suara &= b != null && b.Length > 100; }
            cek("Suara tempur prosedural (" + ProceduralAudioSynth.KlipTempur.Length + " klip)", suara);

            var dir = Object.FindFirstObjectByType<EnemyDirector>();
            bool prefabOk = dir != null;
            if (dir != null)
            {
                var so = new SerializedObject(dir).FindProperty("prefab");
                prefabOk = so != null && so.arraySize == 5;
                for (int i = 0; prefabOk && i < so.arraySize; i++)
                {
                    var go = so.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                    var ai = go != null ? go.GetComponent<NusantaraEnemyAndMutantAI>() : null;
                    prefabOk &= ai != null && ai.Jenis == (EnemyArchetype)i && go.GetComponent<CharacterController>() != null && go.GetComponent<ProceduralAnimator>() != null && go.layer == GameLayers.Enemy;
                }
            }
            cek("EnemyDirector + 5 prefab Sekte (AI, CharacterController, animasi, layer Enemy)", prefabOk);
            var andi = Object.FindFirstObjectByType<AndiFPSController>();
            cek("GridPathfinder, TANGKIS, ReaksiAndi, TandaTempur, diorama", Object.FindFirstObjectByType<GridPathfinder>() != null && GameObject.Find("TombolTangkis") != null
                && andi != null && andi.GetComponent<ReaksiAndi>() != null && Object.FindFirstObjectByType<AlasKeramat.UI.TandaTempur>() != null
                && FindTermasukNonaktif(NusantaraMasterBuilder.NamaDiorama));
        }

        static bool FindTermasukNonaktif(string nama)
        {
            var akar = GameObject.Find(NusantaraMasterBuilder.NamaRoot);
            return akar != null && akar.transform.Find(nama) != null;
        }
    }
}
