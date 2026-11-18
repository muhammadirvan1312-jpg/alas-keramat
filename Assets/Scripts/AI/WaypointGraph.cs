using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEngine;
namespace AlasKeramat.AI
{
    // Graf waypoint per kompleks gua (H6): simpul = pusat ruang + titik lorong, dipanggang Builder. BFS dengan penyangga tetap
    // (tanpa alokasi per panggilan). Dipakai musuh di bawah tanah; permukaan tetap memakai GridPathfinder.
    public class WaypointGraph : MonoBehaviour
    {
        public static readonly List<WaypointGraph> Semua = new List<WaypointGraph>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetGraf() { Semua.Clear(); }

        [SerializeField] UndergroundId kompleks;
        [SerializeField] Vector3[] simpul = new Vector3[0];      // koordinat dunia
        [SerializeField] Vector2Int[] sambungan = new Vector2Int[0];
        int[] _sebelum;
        readonly Queue<int> _antre = new Queue<int>();

        public UndergroundId Kompleks { get { return kompleks; } }
        public int Jumlah { get { return simpul.Length; } }
        public Vector3[] Simpul { get { return simpul; } }
        public void Pasang(UndergroundId u, Vector3[] titik, Vector2Int[] sisi) { kompleks = u; simpul = titik; sambungan = sisi; }

        void OnEnable() { if (!Semua.Contains(this)) Semua.Add(this); }
        void OnDisable() { Semua.Remove(this); }

        public static WaypointGraph Untuk(Vector3 p)
        {
            var u = WorldCaveAndSaveManager.KompleksDi(p);
            for (int i = 0; i < Semua.Count; i++) if (Semua[i] != null && Semua[i].kompleks == u) return Semua[i];
            return null;
        }

        public int Terdekat(Vector3 p)
        {
            int terbaik = -1; float d = float.MaxValue;
            for (int i = 0; i < simpul.Length; i++) { float m = (simpul[i] - p).sqrMagnitude; if (m < d) { d = m; terbaik = i; } }
            return terbaik;
        }

        public bool Cari(int awal, int akhir, List<Vector3> hasil)
        {
            hasil.Clear();
            if (awal < 0 || akhir < 0 || awal >= simpul.Length || akhir >= simpul.Length) return false;
            if (_sebelum == null || _sebelum.Length != simpul.Length) _sebelum = new int[simpul.Length];
            for (int i = 0; i < _sebelum.Length; i++) _sebelum[i] = -1;
            _antre.Clear(); _antre.Enqueue(awal); _sebelum[awal] = awal;
            while (_antre.Count > 0)
            {
                int u = _antre.Dequeue(); if (u == akhir) break;
                for (int k = 0; k < sambungan.Length; k++)
                {
                    var e = sambungan[k];
                    int v = e.x == u ? e.y : e.y == u ? e.x : -1;
                    if (v < 0 || v >= simpul.Length || _sebelum[v] >= 0) continue;
                    _sebelum[v] = u; _antre.Enqueue(v);
                }
            }
            if (_sebelum[akhir] < 0) return false;
            for (int u = akhir; ; u = _sebelum[u]) { hasil.Add(simpul[u]); if (u == awal) break; }
            hasil.Reverse();
            return true;
        }

        // Jalur dari -> ke lewat simpul terdekat masing-masing; titik akhir = tujuan persis.
        public bool Jalur(Vector3 dari, Vector3 ke, List<Vector3> hasil)
        {
            if (!Cari(Terdekat(dari), Terdekat(ke), hasil)) return false;
            hasil.Add(ke);
            return true;
        }
    }
}
