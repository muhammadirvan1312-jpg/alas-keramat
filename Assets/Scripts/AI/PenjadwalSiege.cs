using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEngine;
namespace AlasKeramat.AI
{
    // Gelombang siege 60 detik Altar Beringin (06 §11). Logika murni: diuji self-test tanpa Play.
    public static class PenjadwalSiege
    {
        public const float Lama = 60f, RadiusMin = 35f, RadiusMaks = 45f, DetikBayangan = 40f;
        public const int TitikSpawn = 6, MaksAktif = 12;
        public struct GelombangSiege { public float detik; public EnemyArchetype jenis; public int jumlah; }
        static GelombangSiege G(float d, EnemyArchetype j, int n) { return new GelombangSiege { detik = d, jenis = j, jumlah = n }; }

        // Ki Rogo (detik 40) = Bayangan Ki Rogo (kebal, gelombang tongkat tiap 6 s, memanggil 2 Abdi tiap 10 s).
        public static readonly GelombangSiege[] Tabel =
        {
            G(0f, EnemyArchetype.AbdiSekte, 4), G(0f, EnemyArchetype.PemimpinPatroli, 1),
            G(15f, EnemyArchetype.PocongHutan, 3),
            G(25f, EnemyArchetype.AbdiWanita, 2), G(25f, EnemyArchetype.AbdiButo, 1),
            G(35f, EnemyArchetype.Kuntilanak, 1),
            G(DetikBayangan, EnemyArchetype.KiRogo, 1),
            G(45f, EnemyArchetype.Banaspati, 2), G(45f, EnemyArchetype.PocongHutan, 2),
        };

        // Indeks gelombang yang jatuh di selang (lalu, kini]; detik 0 ikut bila lalu < 0.
        public static int Jatuh(float lalu, float kini, List<int> hasil)
        {
            hasil.Clear();
            for (int i = 0; i < Tabel.Length; i++) if (Tabel[i].detik > lalu && Tabel[i].detik <= kini) hasil.Add(i);
            return hasil.Count;
        }

        public static int JumlahPada(float detik, EnemyArchetype j)
        {
            int n = 0;
            for (int i = 0; i < Tabel.Length; i++) if (Mathf.Approximately(Tabel[i].detik, detik) && Tabel[i].jenis == j) n += Tabel[i].jumlah;
            return n;
        }

        public static int TotalMusuh() { int n = 0; for (int i = 0; i < Tabel.Length; i++) if (Tabel[i].jenis != EnemyArchetype.KiRogo) n += Tabel[i].jumlah; return n; }

        // 6 titik merata di cincin 35-45 m (indeks k berputar), acak01 memilih jari-jari.
        public static Vector3 Titik(Vector3 pusat, int k, float acak01)
        {
            float sudut = (k % TitikSpawn) * (360f / TitikSpawn) * Mathf.Deg2Rad;
            float r = Mathf.Lerp(RadiusMin, RadiusMaks, Mathf.Clamp01(acak01));
            return pusat + new Vector3(Mathf.Sin(sudut) * r, 0f, Mathf.Cos(sudut) * r);
        }

        public static int IndeksHantu(EnemyArchetype j) { return j == EnemyArchetype.PocongHutan ? 0 : j == EnemyArchetype.Kuntilanak ? 1 : j == EnemyArchetype.Banaspati ? 2 : -1; }
    }
}
