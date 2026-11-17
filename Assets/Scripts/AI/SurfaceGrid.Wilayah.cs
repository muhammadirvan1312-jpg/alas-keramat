using System.Collections.Generic;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Graf wilayah untuk pencarian bertingkat (HPA ringan): tiap blok 5x5 sel dipecah menjadi wilayah terhubung (4-arah),
    // simpul = (blok, wilayah), sisi = pasangan sel bertetangga 4-arah lintas batas blok. Karena wilayah benar-benar terhubung,
    // gabungan wilayah di jalur kasar selalu memuat jalur halus -> A* halus dalam koridor itu selalu berhasil.
    public sealed partial class SurfaceGrid
    {
        int[] _simpulSel;              // id simpul per sel (-1 = terhalang)
        int[] _sisiAwal, _sisiTujuan;   // CSR: sisi simpul i = _sisiTujuan[_sisiAwal[i] .. _sisiAwal[i+1])
        Vector2[] _pusatSimpul;         // pusat wilayah dalam satuan sel
        int _jumlahSimpul;

        public int JumlahSimpul { get { Siapkan(); return _jumlahSimpul; } }
        public int SimpulSel(int indeksSel) { Siapkan(); return _simpulSel[indeksSel]; }
        public int[] SimpulPerSel { get { Siapkan(); return _simpulSel; } }
        public Vector2 PusatSimpul(int s) { Siapkan(); return _pusatSimpul[s]; }
        public int SisiAwal(int s) { Siapkan(); return _sisiAwal[s]; }
        public int SisiAkhir(int s) { Siapkan(); return _sisiAwal[s + 1]; }
        public int SisiKe(int k) { return _sisiTujuan[k]; }

        void Siapkan()
        {
            if (_simpulSel != null) return;
            _simpulSel = new int[JumlahSel];
            for (int i = 0; i < JumlahSel; i++) _simpulSel[i] = -1;
            var pusat = new List<Vector2>();
            var tumpuk = new Stack<int>();
            for (int bz = 0; bz < SisiKasar; bz++)
                for (int bx = 0; bx < SisiKasar; bx++)
                    for (int dz = 0; dz < FaktorKasar; dz++)
                        for (int dx = 0; dx < FaktorKasar; dx++)
                        {
                            int x0 = bx * FaktorKasar + dx, z0 = bz * FaktorKasar + dz, i0 = Indeks(x0, z0);
                            if (_sel[i0] == Blok || _simpulSel[i0] >= 0) continue;
                            int id = pusat.Count; float sx = 0f, sz = 0f; int n = 0;
                            _simpulSel[i0] = id; tumpuk.Push(i0);
                            while (tumpuk.Count > 0)
                            {
                                int c = tumpuk.Pop(); int cx = c % Sisi, cz = c / Sisi;
                                sx += cx; sz += cz; n++;
                                for (int a = 0; a < 4; a++)
                                {
                                    int nx = cx + (a == 0 ? 1 : a == 1 ? -1 : 0), nz = cz + (a == 2 ? 1 : a == 3 ? -1 : 0);
                                    if (nx / FaktorKasar != bx || nz / FaktorKasar != bz || !DiDalam(nx, nz)) continue;
                                    int ni = Indeks(nx, nz);
                                    if (_sel[ni] == Blok || _simpulSel[ni] >= 0) continue;
                                    _simpulSel[ni] = id; tumpuk.Push(ni);
                                }
                            }
                            pusat.Add(new Vector2(sx / n, sz / n));
                        }
            _jumlahSimpul = pusat.Count;
            _pusatSimpul = pusat.ToArray();
            var sisi = new HashSet<long>();
            for (int z = 0; z < Sisi; z++)
                for (int x = 0; x < Sisi; x++)
                {
                    int a = _simpulSel[Indeks(x, z)];
                    if (a < 0) continue;
                    if (x + 1 < Sisi) { int b = _simpulSel[Indeks(x + 1, z)]; if (b >= 0 && b != a) { sisi.Add(((long)a << 32) | (uint)b); sisi.Add(((long)b << 32) | (uint)a); } }
                    if (z + 1 < Sisi) { int b = _simpulSel[Indeks(x, z + 1)]; if (b >= 0 && b != a) { sisi.Add(((long)a << 32) | (uint)b); sisi.Add(((long)b << 32) | (uint)a); } }
                }
            var hitung = new int[_jumlahSimpul + 1];
            foreach (var e in sisi) hitung[(int)(e >> 32) + 1]++;
            for (int i = 0; i < _jumlahSimpul; i++) hitung[i + 1] += hitung[i];
            _sisiAwal = (int[])hitung.Clone();
            _sisiTujuan = new int[sisi.Count];
            var isi = (int[])hitung.Clone();
            foreach (var e in sisi) { int a = (int)(e >> 32); _sisiTujuan[isi[a]++] = (int)(e & 0xFFFFFFFF); }
        }
    }
}
