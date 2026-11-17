using System.Collections.Generic;
using UnityEngine;

namespace AlasKeramat.Building
{
    // 07 §6: BFS dari simpul tertanam; potongan yang tak terjangkau runtuh. Maks 6 tingkat dicek saat memasang.
    public static class StructureIntegrity
    {
        public const int MaksTingkat = 6;
        public const float TinggiTingkat = 2.2f;

        // Mengembalikan semua potongan yang terhubung dengan 'awal' tetapi tidak terjangkau dari simpul tertanam.
        public static List<BagianStruktur> TakTertopang(IEnumerable<BagianStruktur> awal)
        {
            var komponen = new HashSet<BagianStruktur>();
            var antre = new Queue<BagianStruktur>();
            foreach (var a in awal) if (a != null && komponen.Add(a)) antre.Enqueue(a);
            while (antre.Count > 0) { var n = antre.Dequeue(); foreach (var t in n.Tetangga) if (t != null && komponen.Add(t)) antre.Enqueue(t); }
            var tertopang = new HashSet<BagianStruktur>();
            foreach (var n in komponen) if (n.Tertanam && tertopang.Add(n)) antre.Enqueue(n);
            while (antre.Count > 0) { var n = antre.Dequeue(); foreach (var t in n.Tetangga) if (t != null && tertopang.Add(t)) antre.Enqueue(t); }
            var hasil = new List<BagianStruktur>();
            foreach (var n in komponen) if (!tertopang.Contains(n)) hasil.Add(n);
            return hasil;
        }

        public static void Periksa(IEnumerable<BagianStruktur> awal)
        {
            foreach (var b in TakTertopang(awal)) if (b != null) b.Runtuhkan();
        }
    }
}
