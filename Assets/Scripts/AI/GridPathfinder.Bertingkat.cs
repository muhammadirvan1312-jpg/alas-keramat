using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Pencarian bertingkat untuk jarak jauh (dunia v5 2x2 km): A* di graf wilayah (blok 20 m dipecah per wilayah terhubung),
    // lalu A* sel 4 m hanya di dalam wilayah koridor itu (+ tetangga langsung). Total node (kasar + halus) tetap di bawah MaksNode.
    public partial class GridPathfinder
    {
        public const float JarakBertingkat = 150f, BobotKasar = 2f, JarakHalus = 100f;
        public const int CadanganHalus = 800;
        public static int SimpulKasarTerakhir { get; private set; }

        public static int SegmenGagalTerakhir { get; private set; }

        static float[] _gK, _fK;
        static int[] _indukK, _capK, _heapK;
        static int _capAktifK, _ukuranHeapK;
        static bool[] _izinSimpul;
        static readonly List<int> _jalurSimpul = new List<int>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetKasar() { _gK = null; _fK = null; _indukK = null; _capK = null; _heapK = null; _izinSimpul = null; _capAktifK = 0; _ukuranHeapK = 0; }

        // Pilih otomatis: dekat = A* halus, jauh = bertingkat.
        public static bool CariOtomatis(SurfaceGrid g, Vector3 dari, Vector3 ke, List<Vector3> hasil, int anggaran, out int dikunjungi)
        {
            Vector3 d = ke - dari; d.y = 0f;
            if (d.magnitude <= JarakBertingkat) return Cari(g, dari, ke, hasil, anggaran, out dikunjungi);
            return CariBertingkat(g, dari, ke, hasil, anggaran, out dikunjungi);
        }

        public static bool CariBertingkat(SurfaceGrid g, Vector3 dari, Vector3 ke, List<Vector3> hasil, int anggaran, out int dikunjungi)
        {
            dikunjungi = 0; SegmenGagalTerakhir = 0;
            if (hasil != null) hasil.Clear();
            if (g == null) return false;
            int sx, sz, tx, tz;
            SurfaceGrid.KeSel(dari, out sx, out sz); SurfaceGrid.KeSel(ke, out tx, out tz);
            if (!SelTerdekat(g, sx, sz, out sx, out sz) || !SelTerdekat(g, tx, tz, out tx, out tz)) return false;
            int a = g.SimpulSel(SurfaceGrid.Indeks(sx, sz)), b = g.SimpulSel(SurfaceGrid.Indeks(tx, tz));
            int nK;
            bool okK = CariSimpul(g, a, b, Mathf.Max(100, anggaran - CadanganHalus), out nK);
            SimpulKasarTerakhir = nK;
            if (!okK) { dikunjungi = nK; return false; }
            dikunjungi = nK;
            // Haluskan bagian awal (~100 m) dengan A* sel 4 m di dalam koridor; sisanya titik wakil wilayah (dihaluskan saat pencarian ulang 2 s).
            int ujung = 0; float jarak = 0f;
            Vector3 sebelum = SurfaceGrid.PusatSel(sx, sz);
            for (int i = 1; i < _jalurSimpul.Count; i++)
            {
                int w = g.WakilSimpul(_jalurSimpul[i]);
                Vector3 p = SurfaceGrid.PusatSel(w % SurfaceGrid.Sisi, w / SurfaceGrid.Sisi);
                jarak += Vector3.Distance(new Vector3(p.x, 0f, p.z), new Vector3(sebelum.x, 0f, sebelum.z));
                sebelum = p; ujung = i;
                if (jarak >= JarakHalus) break;
            }
            if (_izinSimpul == null || _izinSimpul.Length != g.JumlahSimpul) _izinSimpul = new bool[g.JumlahSimpul];
            System.Array.Clear(_izinSimpul, 0, _izinSimpul.Length);
            for (int i = 0; i <= ujung && i < _jalurSimpul.Count; i++)
            {
                int s = _jalurSimpul[i];
                _izinSimpul[s] = true;
                for (int k = g.SisiAwal(s); k < g.SisiAkhir(s); k++) _izinSimpul[g.SisiKe(k)] = true;
            }
            bool akhirHalus = ujung >= _jalurSimpul.Count - 1;
            int wu = g.WakilSimpul(_jalurSimpul[ujung]);
            Vector3 target = akhirHalus ? new Vector3(ke.x, 0f, ke.z) : SurfaceGrid.PusatSel(wu % SurfaceGrid.Sisi, wu / SurfaceGrid.Sisi);
            int n;
            bool ok = Cari(g, SurfaceGrid.PusatSel(sx, sz), target, hasil, Mathf.Max(100, anggaran - dikunjungi), out n, _izinSimpul);
            dikunjungi += n;
            if (!ok) { SegmenGagalTerakhir = 1; return false; }
            if (hasil != null)
                for (int i = ujung + 1; i < _jalurSimpul.Count; i++)
                {
                    int w = g.WakilSimpul(_jalurSimpul[i]);
                    hasil.Add(i == _jalurSimpul.Count - 1 ? new Vector3(ke.x, WorldTerrainGenerator.SampleHeight(ke.x, ke.z), ke.z) : SurfaceGrid.PusatSel(w % SurfaceGrid.Sisi, w / SurfaceGrid.Sisi));
                }
            return true;
        }

        static void SiapkanKasar(int n)
        {
            if (_gK != null && _gK.Length == n) return;
            _gK = new float[n]; _fK = new float[n]; _indukK = new int[n]; _capK = new int[n]; _heapK = new int[n * 4];
        }

        static float HeurK(SurfaceGrid g, int a, int b) { return Vector2.Distance(g.PusatSimpul(a), g.PusatSimpul(b)) * BobotKasar; }

        static bool CariSimpul(SurfaceGrid g, int awal, int tujuan, int maksNode, out int dikunjungi)
        {
            dikunjungi = 0; _jalurSimpul.Clear();
            if (awal < 0 || tujuan < 0) return false;
            SiapkanKasar(g.JumlahSimpul);
            _capAktifK++;
            int cap = _capAktifK, tutup = -cap;
            _ukuranHeapK = 0;
            _gK[awal] = 0f; _indukK[awal] = -1; _capK[awal] = cap; _fK[awal] = HeurK(g, awal, tujuan);
            DorongK(awal);
            while (_ukuranHeapK > 0)
            {
                int c = AmbilK();
                if (_capK[c] == tutup) continue;
                _capK[c] = tutup;
                dikunjungi++;
                if (c == tujuan)
                {
                    for (int t = c; t >= 0; t = _indukK[t]) _jalurSimpul.Add(t);
                    _jalurSimpul.Reverse();
                    return true;
                }
                if (dikunjungi >= maksNode) return false;
                Vector2 pc = g.PusatSimpul(c);
                for (int k = g.SisiAwal(c); k < g.SisiAkhir(c); k++)
                {
                    int n = g.SisiKe(k);
                    if (_capK[n] == tutup) continue;
                    float gb = _gK[c] + Vector2.Distance(pc, g.PusatSimpul(n));
                    if (_capK[n] == cap && gb >= _gK[n]) continue;
                    _capK[n] = cap; _gK[n] = gb; _indukK[n] = c; _fK[n] = gb + HeurK(g, n, tujuan);
                    DorongK(n);
                }
            }
            return false;
        }

        static void DorongK(int n)
        {
            if (_ukuranHeapK >= _heapK.Length) return;
            int i = _ukuranHeapK++;
            _heapK[i] = n;
            while (i > 0)
            {
                int p = (i - 1) >> 1;
                if (_fK[_heapK[p]] <= _fK[_heapK[i]]) break;
                int t = _heapK[p]; _heapK[p] = _heapK[i]; _heapK[i] = t; i = p;
            }
        }

        static int AmbilK()
        {
            int top = _heapK[0];
            _heapK[0] = _heapK[--_ukuranHeapK];
            int i = 0;
            while (true)
            {
                int l = 2 * i + 1, r = l + 1, m = i;
                if (l < _ukuranHeapK && _fK[_heapK[l]] < _fK[_heapK[m]]) m = l;
                if (r < _ukuranHeapK && _fK[_heapK[r]] < _fK[_heapK[m]]) m = r;
                if (m == i) break;
                int t = _heapK[m]; _heapK[m] = _heapK[i]; _heapK[i] = t; i = m;
            }
            return top;
        }
    }
}
