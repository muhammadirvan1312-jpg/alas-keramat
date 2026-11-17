using System;
using System.Collections.Generic;
using AlasKeramat.AI;
using AlasKeramat.Core;
using UnityEngine;
using Object = UnityEngine.Object;
using P = AlasKeramat.Core.PenjadwalTeror;
using KS = AlasKeramat.AI.EnemyDirector.KegiatanSekte;

namespace AlasKeramat.EditorTools
{
    // Self-test T9 (12 T9): simulasi 20 menit malam (>= 6 Ringan, <= 2 Keras, tanpa ulang dalam 2 hari game), Tension, jadwal Sekte per jam,
    // 20 catatan Deden punya pemicu valid, tawa terbalik, undian kejadian dunia, kelengkapan scene.
    public static class PemeriksaanT9
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            var k = new P.Konteks { malam = true, gelap = true, malamDarah = true, sendirian = true, headset = true, sanity = 50f, jam = 23.5f, hari = 5 };
            float t = P.HitungTension(k);
            cek("Tension 13 §2.1: malam+gelap+Malam Darah+Sanity 50 = " + t.ToString("0.0"), Mathf.Approximately(t, 82.5f) && Mathf.Approximately(P.HitungTension(new P.Konteks { sanity = 100f, siangCerah = true }), 5f)
                && Mathf.Approximately(P.Ancaman(5f), 30f) && Mathf.Approximately(P.Ancaman(60f), 0f));
            var katalog = TerrorDirector.Katalog();
            var p = new P(katalog, 12345);
            var jumlah = new Dictionary<string, int>(); bool ulangSalah = false;
            for (float w = 0f; w < 1200f; w += 0.5f)
            {
                var e = p.Langkah(0.5f, k);
                if (e.id == null) continue;
                int n; jumlah.TryGetValue(e.id, out n); jumlah[e.id] = n + 1;
                if (!e.ulang && n >= 1) ulangSalah = true;
            }
            cek("Simulasi 20 menit malam: Ringan " + p.JumlahRingan + " (>= 6), Sedang " + p.JumlahSedang + ", Keras " + p.JumlahKeras + " (<= 2), tanpa ulang", p.JumlahRingan >= 6 && p.JumlahKeras <= 2 && !ulangSalah);
            var p2 = new P(katalog, 7) { Setelan = P.Intensitas.Ringan };
            var kKeras = k; kKeras.tidur = true; kKeras.sanity = 20f; kKeras.berenang = true;
            for (float w = 0f; w < 1200f; w += 0.5f) p2.Langkah(0.5f, kKeras);
            cek("Intensitas Ringan: tanpa kejutan Keras (" + p2.JumlahKeras + ")", p2.JumlahKeras == 0);
            bool ids = true; var set = new HashSet<string>();
            foreach (var e in katalog) ids &= set.Add(e.id) && e.syarat != null && e.nama.Length > 0;
            cek("Katalog " + katalog.Count + " kejadian unik dengan syarat", ids && katalog.Count >= 25);
            bool jadwal = EnemyDirector.Jadwal(13f, false) == KS.TidurSiang && EnemyDirector.Jadwal(20f, false) == KS.Ritual && EnemyDirector.Jadwal(1f, false) == KS.Tidur
                && EnemyDirector.Jadwal(7f, false) == KS.Pagi && EnemyDirector.Jadwal(10f, false) == KS.Patroli && EnemyDirector.Jadwal(15f, false) == KS.Patroli
                && EnemyDirector.Jadwal(17.5f, false) == KS.Pulang && EnemyDirector.Jadwal(5.5f, false) == KS.Bangun && EnemyDirector.Jadwal(1f, true) == KS.Berburu;
            for (int j = 0; j < 24; j++) { var g = EnemyDirector.Jadwal(j + 0.5f, false); jadwal &= Enum.IsDefined(typeof(KS), g); }
            cek("Jadwal Sekte 24 jam (16 §4.1)", jadwal);
            bool catatan = KangDedenKelvinAI.Catatan.Length == 20; var kosong = new HashSet<int>();
            var kMaks = new KangDedenKelvinAI.KonteksCatatan { hari = 10, bangunan = 10, sekteDibunuh = 5, sekteDiampuni = 3, hantuDiusir = 3, trustNingsih = 90f, kemarahan = 50f, malamDarahLewat = true, pakuDidapat = true, hujanBadai = true, lukaParah = true };
            var kMin = new KangDedenKelvinAI.KonteksCatatan { hari = 1 };
            foreach (var c in KangDedenKelvinAI.Catatan) catatan &= c.teks.Length > 10 && c.syarat(kMaks) && !c.syarat(kMin);
            catatan &= KangDedenKelvinAI.PilihCatatan(kMin, kosong) == -1 && KangDedenKelvinAI.PilihCatatan(kMaks, kosong) == 0;
            cek("20 Catatan Kang Deden: teks & pemicu valid (Hari 1 belum ada catatan)", catatan);
            cek("Tawa terbalik 13 §4.1 (dekat pelan, jauh keras)", TerrorDirector.VolumeTawaTerbalik(10f) == 0.25f && TerrorDirector.VolumeTawaTerbalik(30f) == 0.5f && TerrorDirector.VolumeTawaTerbalik(60f) == 1f);
            int u4 = WorldEventScheduler.Undi(4, 0, 0, new System.Random(1)), u2 = WorldEventScheduler.Undi(2, 0, 5, new System.Random(1));
            cek("Kejadian dunia: Hari 4 pesawat terskrip, maks 2 per pagi, jenazah maks 5", (u4 & 1) != 0 && Bit(u4) <= 2 && (u2 & 4) == 0 && Bit(WorldEventScheduler.Undi(9, 0, 0, new System.Random(3))) <= 2);
            var td = Object.FindFirstObjectByType<TerrorDirector>();
            bool scene = td != null && Object.FindFirstObjectByType<WorldEventScheduler>() != null;
            if (td != null) { var so = new UnityEditor.SerializedObject(td); scene &= so.FindProperty("prefabSosok").objectReferenceValue != null && so.FindProperty("kedip").objectReferenceValue != null; }
            cek("TerrorDirector (sosok, layar kedip) & WorldEventScheduler di scene", scene);
            int sr; bool suara = true; foreach (var s in ProceduralAudioSynth.KlipTeror) { var b = ProceduralAudioSynth.Generate(s, 0, out sr); suara &= b != null && b.Length > 100; }
            cek("Suara teror prosedural (" + ProceduralAudioSynth.KlipTeror.Length + " klip)", suara);
        }

        static int Bit(int v) { int n = 0; while (v != 0) { n += v & 1; v >>= 1; } return n; }
    }
}
