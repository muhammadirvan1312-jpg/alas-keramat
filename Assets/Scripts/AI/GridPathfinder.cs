using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // H6: A* 8-arah di SurfaceGrid (tanpa navigasi bawaan). Heuristik oktil berbobot agar ekspansi node kecil; jalur dihaluskan (string pulling).
    // Satu pencarian dibatasi MaksNode; permintaan dari musuh diantrikan maksimal PencarianPerFrame per frame.
    public partial class GridPathfinder : MonoBehaviour
    {
        public const int MaksNode = 4000, PencarianPerFrame = 2, RadiusCariLewat = 4, PandangMaks = 40;
        public const float BobotDefault = 2f, Akar2 = 1.41421356f;
        public static float BobotHeuristik = BobotDefault; // weighted A*: jalur sedikit kurang optimal, ekspansi node jauh lebih kecil

        [SerializeField] TextAsset dataGrid;

        public SurfaceGrid Grid { get; private set; }

        static float[] _g;
        static int[] _induk, _cap, _heap;
        static float[] _f;
        static int _capAktif, _ukuranHeap;
        static readonly int[] _dx = { 1, -1, 0, 0, 1, 1, -1, -1 };
        static readonly int[] _dz = { 0, 0, 1, -1, 1, -1, 1, -1 };

        struct Permintaan { public Vector3 dari, ke; public List<Vector3> hasil; public System.Action<bool> selesai; }
        readonly Queue<Permintaan> _antri = new Queue<Permintaan>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatis() { _g = null; _induk = null; _cap = null; _heap = null; _f = null; _capAktif = 0; _ukuranHeap = 0; }

        public void Pasang(TextAsset data) { dataGrid = data; }

        void Awake()
        {
            GameServices.Register(this);
            if (dataGrid != null) Grid = SurfaceGrid.DariBytes(dataGrid.bytes);
            if (Grid == null) Debug.LogError("[AK][GridPathfinder] SurfaceGrid.bytes tidak ada atau rusak; musuh hanya bergerak lurus. Jalankan Builder ulang.");
        }

        // Antrikan pencarian; callback dipanggil di frame berikutnya (atau langsung false bila grid tidak ada).
        public void Minta(Vector3 dari, Vector3 ke, List<Vector3> hasil, System.Action<bool> selesai)
        {
            if (Grid == null) { if (selesai != null) selesai(false); return; }
            _antri.Enqueue(new Permintaan { dari = dari, ke = ke, hasil = hasil, selesai = selesai });
        }

        void Update()
        {
            for (int n = 0; n < PencarianPerFrame && _antri.Count > 0; n++)
            {
                var p = _antri.Dequeue();
                int k;
                bool ok = false;
                try { ok = CariOtomatis(Grid, p.dari, p.ke, p.hasil, MaksNode, out k); }
                catch (System.Exception e) { Debug.LogError("[AK][GridPathfinder] Pencarian jalur gagal: " + e.Message); }
                if (p.selesai != null) p.selesai(ok);
            }
        }

        static void Siapkan()
        {
            if (_g != null) return;
            _g = new float[SurfaceGrid.JumlahSel]; _f = new float[SurfaceGrid.JumlahSel];
            _induk = new int[SurfaceGrid.JumlahSel]; _cap = new int[SurfaceGrid.JumlahSel];
            _heap = new int[SurfaceGrid.JumlahSel];
        }

        static float Heuristik(int ax, int az, int bx, int bz)
        {
            int dx = Mathf.Abs(ax - bx), dz = Mathf.Abs(az - bz);
            return (Mathf.Max(dx, dz) + (Akar2 - 1f) * Mathf.Min(dx, dz)) * BobotHeuristik;
        }

        // Cari sel bisa-dilalui terdekat (spiral kecil) bila titik awal/tujuan terhalang.
        public static bool SelTerdekat(SurfaceGrid g, int ix, int iz, out int ox, out int oz)
        {
            ox = ix; oz = iz;
            if (g.Lewat(ix, iz)) return true;
            for (int r = 1; r <= RadiusCariLewat; r++)
                for (int dz = -r; dz <= r; dz++)
                    for (int dx = -r; dx <= r; dx++)
                    {
                        if (Mathf.Abs(dx) != r && Mathf.Abs(dz) != r) continue;
                        if (g.Lewat(ix + dx, iz + dz)) { ox = ix + dx; oz = iz + dz; return true; }
                    }
            return false;
        }

        // A* utama. dikunjungi = jumlah node yang diekspansi (untuk self-test anggaran < 4000).
        public static bool Cari(SurfaceGrid g, Vector3 dari, Vector3 ke, List<Vector3> hasil, int maksNode, out int dikunjungi) { return Cari(g, dari, ke, hasil, maksNode, out dikunjungi, null); }

        // izinSimpul (opsional): hanya sel yang wilayahnya ada di koridor yang boleh diekspansi (pencarian bertingkat).
        public static bool Cari(SurfaceGrid g, Vector3 dari, Vector3 ke, List<Vector3> hasil, int maksNode, out int dikunjungi, bool[] izinSimpul)
        {
            dikunjungi = 0;
            if (hasil != null) hasil.Clear();
            if (g == null) return false;
            Siapkan();
            int sx, sz, tx, tz;
            SurfaceGrid.KeSel(dari, out sx, out sz); SurfaceGrid.KeSel(ke, out tx, out tz);
            if (!SelTerdekat(g, sx, sz, out sx, out sz) || !SelTerdekat(g, tx, tz, out tx, out tz)) return false;
            _capAktif++;
            if (_capAktif == int.MaxValue) { System.Array.Clear(_cap, 0, _cap.Length); _capAktif = 1; }
            int cap = _capAktif, capTutup = -cap;
            int awal = SurfaceGrid.Indeks(sx, sz), tujuan = SurfaceGrid.Indeks(tx, tz);
            _ukuranHeap = 0;
            _g[awal] = 0f; _induk[awal] = -1; _cap[awal] = cap; _f[awal] = Heuristik(sx, sz, tx, tz);
            Dorong(awal);
            var data = g.Data;
            var simpulSel = izinSimpul != null ? g.SimpulPerSel : null;
            while (_ukuranHeap > 0)
            {
                int c = Ambil();
                if (_cap[c] == capTutup) continue;
                _cap[c] = capTutup;
                dikunjungi++;
                if (c == tujuan) { Susun(g, tujuan, dari, ke, hasil); return true; }
                if (dikunjungi >= maksNode) return false;
                int cx = c % SurfaceGrid.Sisi, cz = c / SurfaceGrid.Sisi;
                for (int a = 0; a < 8; a++)
                {
                    int nx = cx + _dx[a], nz = cz + _dz[a];
                    if (!SurfaceGrid.DiDalam(nx, nz)) continue;
                    int n = SurfaceGrid.Indeks(nx, nz);
                    byte s = data[n];
                    if (s == SurfaceGrid.Blok || _cap[n] == capTutup) continue;
                    if (izinSimpul != null && !izinSimpul[simpulSel[n]]) continue;
                    bool diagonal = a >= 4;
                    if (diagonal && (data[SurfaceGrid.Indeks(cx + _dx[a], cz)] == SurfaceGrid.Blok || data[SurfaceGrid.Indeks(cx, cz + _dz[a])] == SurfaceGrid.Blok)) continue; // jangan potong sudut tebing
                    float gBaru = _g[c] + (diagonal ? Akar2 : 1f) * SurfaceGrid.Biaya(s);
                    if (_cap[n] == cap && gBaru >= _g[n]) continue;
                    _cap[n] = cap; _g[n] = gBaru; _induk[n] = c; _f[n] = gBaru + Heuristik(nx, nz, tx, tz);
                    Dorong(n);
                }
            }
            return false;
        }

        static void Susun(SurfaceGrid g, int tujuan, Vector3 dari, Vector3 ke, List<Vector3> hasil)
        {
            if (hasil == null) return;
            int jumlah = 0;
            for (int c = tujuan; c >= 0; c = _induk[c]) { _heap[jumlah++] = c; if (jumlah >= _heap.Length) break; } // _heap dipakai ulang sebagai penyangga
            // String pulling: lompati sel selama garis lurus antar-sel tetap bisa dilalui (dan tidak masuk air bila awalnya darat).
            int i = jumlah - 1;
            while (i > 0)
            {
                int j = i - 1;
                for (int k = Mathf.Max(0, i - PandangMaks); k < i; k++) if (GarisLewat(g, _heap[i], _heap[k])) { j = k; break; }
                if (j >= i) j = i - 1;
                var p = SurfaceGrid.PusatSel(_heap[j] % SurfaceGrid.Sisi, _heap[j] / SurfaceGrid.Sisi);
                hasil.Add(j == 0 ? new Vector3(ke.x, WorldTerrainGenerator.SampleHeight(ke.x, ke.z), ke.z) : p);
                i = j;
            }
            if (hasil.Count == 0) hasil.Add(ke);
        }

        // Bresenham: semua sel di garis harus sama jenis (darat-darat) agar jalur halus tidak memotong tebing atau sungai.
        public static bool GarisLewat(SurfaceGrid g, int a, int b)
        {
            int x0 = a % SurfaceGrid.Sisi, z0 = a / SurfaceGrid.Sisi, x1 = b % SurfaceGrid.Sisi, z1 = b / SurfaceGrid.Sisi;
            byte jenis = g.Ambil(x0, z0);
            int dx = Mathf.Abs(x1 - x0), dz = -Mathf.Abs(z1 - z0), sx = x0 < x1 ? 1 : -1, sz = z0 < z1 ? 1 : -1, err = dx + dz;
            while (true)
            {
                if (g.Ambil(x0, z0) != jenis) return false;
                if (x0 == x1 && z0 == z1) return true;
                int e2 = 2 * err;
                if (e2 >= dz) { err += dz; x0 += sx; }
                if (e2 <= dx) { err += dx; z0 += sz; }
            }
        }

        static void Dorong(int n)
        {
            if (_ukuranHeap >= _heap.Length) return;
            int i = _ukuranHeap++;
            _heap[i] = n;
            while (i > 0)
            {
                int p = (i - 1) >> 1;
                if (_f[_heap[p]] <= _f[_heap[i]]) break;
                int t = _heap[p]; _heap[p] = _heap[i]; _heap[i] = t; i = p;
            }
        }

        static int Ambil()
        {
            int top = _heap[0];
            _heap[0] = _heap[--_ukuranHeap];
            int i = 0;
            while (true)
            {
                int l = 2 * i + 1, r = l + 1, m = i;
                if (l < _ukuranHeap && _f[_heap[l]] < _f[_heap[m]]) m = l;
                if (r < _ukuranHeap && _f[_heap[r]] < _f[_heap[m]]) m = r;
                if (m == i) break;
                int t = _heap[m]; _heap[m] = _heap[i]; _heap[i] = t; i = m;
            }
            return top;
        }
    }
}
