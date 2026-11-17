using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Pencarian bertingkat untuk jarak jauh (dunia v5 2x2 km): A* di grid kasar 100x100 (blok 20 m) menentukan koridor,
    // lalu A* sel 4 m dijalankan hanya di dalam koridor itu. Total node (kasar + halus) tetap di bawah MaksNode.
    public partial class GridPathfinder
    {
        public const float JarakBertingkat = 150f, BobotKasar = 1.2f;
        
        public static int SegmenGagalTerakhir { get; private set; }

        static float[] _gK, _fK;
        static int[] _indukK, _capK, _heapK;
        static int _capAktifK, _ukuranHeapK;
        static readonly List<Vector3> _koridor = new List<Vector3>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetKasar() { _gK = null; _fK = null; _indukK = null; _capK = null; _heapK = null; _capAktifK = 0; _ukuranHeapK = 0; }

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
            int nK;
            if (!CariKasar(g, dari, ke, _koridor, anggaran / 2, out nK)) { dikunjungi = nK; return false; }
            dikunjungi = nK;
            // A* halus dibatasi koridor (blok kasar jalur +- 1); bila koridor terputus di tingkat halus, lebarkan +- 2.
            for (int lebar = 1; lebar <= 2; lebar++)
            {
                TandaiKoridor(lebar);
                int n;
                bool ok = Cari(g, dari, ke, hasil, Mathf.Max(100, anggaran - dikunjungi), out n, _izinBlok);
                dikunjungi += n;
                if (ok) return true;
                SegmenGagalTerakhir++;
                if (dikunjungi >= anggaran) break;
            }
            return false;
        }

        static bool[] _izinBlok;

        static void TandaiKoridor(int lebar)
        {
            int S = SurfaceGrid.SisiKasar;
            if (_izinBlok == null) _izinBlok = new bool[S * S];
            System.Array.Clear(_izinBlok, 0, _izinBlok.Length);
            for (int i = 0; i < _koridor.Count; i++)
            {
                int cx, cz; KeKasar(_koridor[i], out cx, out cz);
                for (int dz = -lebar; dz <= lebar; dz++)
                    for (int dx = -lebar; dx <= lebar; dx++)
                    {
                        int x = cx + dx, z = cz + dz;
                        if (x >= 0 && z >= 0 && x < S && z < S) _izinBlok[z * S + x] = true;
                    }
            }
        }

        static void SiapkanKasar()
        {
            if (_gK != null) return;
            int n = SurfaceGrid.SisiKasar * SurfaceGrid.SisiKasar;
            _gK = new float[n]; _fK = new float[n]; _indukK = new int[n]; _capK = new int[n]; _heapK = new int[n * 8];
        }

        static void KeKasar(Vector3 p, out int cx, out int cz)
        {
            int ix, iz; SurfaceGrid.KeSel(p, out ix, out iz);
            cx = ix / SurfaceGrid.FaktorKasar; cz = iz / SurfaceGrid.FaktorKasar;
        }

        static bool LewatKasar(byte[] k, int cx, int cz)
        {
            return cx >= 0 && cz >= 0 && cx < SurfaceGrid.SisiKasar && cz < SurfaceGrid.SisiKasar && k[cz * SurfaceGrid.SisiKasar + cx] != SurfaceGrid.Blok;
        }

        static bool TerdekatKasar(byte[] k, ref int cx, ref int cz)
        {
            if (LewatKasar(k, cx, cz)) return true;
            for (int r = 1; r <= 2; r++)
                for (int dz = -r; dz <= r; dz++)
                    for (int dx = -r; dx <= r; dx++)
                        if (LewatKasar(k, cx + dx, cz + dz)) { cx += dx; cz += dz; return true; }
            return false;
        }

        static bool CariKasar(SurfaceGrid g, Vector3 dari, Vector3 ke, List<Vector3> hasil, int maksNode, out int dikunjungi)
        {
            dikunjungi = 0; hasil.Clear();
            SiapkanKasar();
            var k = g.Kasar;
            int S = SurfaceGrid.SisiKasar;
            int sx, sz, tx, tz;
            KeKasar(dari, out sx, out sz); KeKasar(ke, out tx, out tz);
            if (!TerdekatKasar(k, ref sx, ref sz) || !TerdekatKasar(k, ref tx, ref tz)) return false;
            _capAktifK++;
            int cap = _capAktifK, tutup = -cap;
            int awal = sz * S + sx, tujuan = tz * S + tx;
            _ukuranHeapK = 0;
            _gK[awal] = 0f; _indukK[awal] = -1; _capK[awal] = cap; _fK[awal] = HeurK(sx, sz, tx, tz);
            DorongK(awal);
            while (_ukuranHeapK > 0)
            {
                int c = AmbilK();
                if (_capK[c] == tutup) continue;
                _capK[c] = tutup;
                dikunjungi++;
                if (c == tujuan)
                {
                    for (int t = c; t >= 0; t = _indukK[t]) hasil.Add(SurfaceGrid.PusatKasar(t % S, t / S));
                    hasil.Reverse();
                    return true;
                }
                if (dikunjungi >= maksNode) return false;
                int cx = c % S, cz = c / S;
                for (int a = 0; a < 8; a++)
                {
                    int nx = cx + _dx[a], nz = cz + _dz[a];
                    if (!LewatKasar(k, nx, nz)) continue;
                    int n = nz * S + nx;
                    if (_capK[n] == tutup) continue;
                    bool diagonal = a >= 4;
                    if (!diagonal && !g.LintasKasar(cx, cz, _dx[a], _dz[a])) continue;
                    if (diagonal && (!LewatKasar(k, nx, cz) || !LewatKasar(k, cx, nz)
                        || !g.LintasKasar(cx, cz, _dx[a], 0) || !g.LintasKasar(nx, cz, 0, _dz[a])
                        || !g.LintasKasar(cx, cz, 0, _dz[a]) || !g.LintasKasar(cx, nz, _dx[a], 0))) continue;
                    float gb = _gK[c] + (diagonal ? Akar2 : 1f);
                    if (_capK[n] == cap && gb >= _gK[n]) continue;
                    _capK[n] = cap; _gK[n] = gb; _indukK[n] = c; _fK[n] = gb + HeurK(nx, nz, tx, tz);
                    DorongK(n);
                }
            }
            return false;
        }

        static float HeurK(int ax, int az, int bx, int bz)
        {
            int dx = Mathf.Abs(ax - bx), dz = Mathf.Abs(az - bz);
            return (Mathf.Max(dx, dz) + (Akar2 - 1f) * Mathf.Min(dx, dz)) * BobotKasar;
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
