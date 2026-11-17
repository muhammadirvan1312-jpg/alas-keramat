using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // H6 + 12 T7: grid permukaan 500x500 sel (sel 4 m, dunia v5 2x2 km) yang dipanggang Builder ke Assets/Generated/Nav/SurfaceGrid.bytes.
    // Nilai sel: 0 = terhalang (laut/tebing), 1 = darat bisa dilalui, 2 = air sungai (bisa diseberangi, mahal).
    public sealed class SurfaceGrid
    {
        public const int Sisi = 500, JumlahSel = Sisi * Sisi;
        public const float UkuranSel = 2f * WorldTerrainGenerator.BatasDunia / Sisi; // 4 m
        public const float Asal = -WorldTerrainGenerator.BatasDunia;
        public const float LerengMaks = 40f, DalamAirMaks = 4.5f;
        public const byte Blok = 0, Darat = 1, Air = 2;
        public const int VersiData = 1;
        public static readonly byte[] Kepala = { (byte)'A', (byte)'K', (byte)'S', (byte)'G' };

        readonly byte[] _sel;
        public byte[] Data { get { return _sel; } }

        public SurfaceGrid(byte[] sel) { _sel = sel; }

        public static int Indeks(int ix, int iz) { return iz * Sisi + ix; }
        public static bool DiDalam(int ix, int iz) { return ix >= 0 && iz >= 0 && ix < Sisi && iz < Sisi; }
        public static void KeSel(Vector3 p, out int ix, out int iz)
        {
            ix = Mathf.Clamp(Mathf.FloorToInt((p.x - Asal) / UkuranSel), 0, Sisi - 1);
            iz = Mathf.Clamp(Mathf.FloorToInt((p.z - Asal) / UkuranSel), 0, Sisi - 1);
        }
        public static Vector3 PusatSel(int ix, int iz)
        {
            float x = Asal + (ix + 0.5f) * UkuranSel, z = Asal + (iz + 0.5f) * UkuranSel;
            return new Vector3(x, WorldTerrainGenerator.SampleHeight(x, z), z);
        }

        public byte Ambil(int ix, int iz) { return DiDalam(ix, iz) ? _sel[Indeks(ix, iz)] : Blok; }
        public bool Lewat(int ix, int iz) { return Ambil(ix, iz) != Blok; }

        // Biaya masuk sel (dikali jarak langkah). Air sungai = berenang/mengarungi, 4x lebih mahal.
        public static float Biaya(byte s) { return s == Air ? 4f : 1f; }

        // Klasifikasi satu sel dari tinggi pusat + 4 tetangga (lereng rata-rata 4 m) + permukaan air sungai.
        public static byte Klasifikasi(float h, float hKiri, float hKanan, float hBawah, float hAtas, float? permukaanAir, bool sungai)
        {
            if (sungai && permukaanAir.HasValue && permukaanAir.Value > h) return permukaanAir.Value - h <= DalamAirMaks ? Air : Blok;
            if (h < 0f) return Blok;
            float gx = (hKanan - hKiri) / (2f * UkuranSel), gz = (hAtas - hBawah) / (2f * UkuranSel);
            float lereng = Mathf.Atan(Mathf.Sqrt(gx * gx + gz * gz)) * Mathf.Rad2Deg;
            return lereng <= LerengMaks ? Darat : Blok;
        }

        // Panggang seluruh grid (Editor). Tinggi dicicil satu kali per titik grid lalu dipakai ulang untuk lereng.
        public static SurfaceGrid Panggang()
        {
            var tinggi = new float[JumlahSel];
            for (int iz = 0; iz < Sisi; iz++)
                for (int ix = 0; ix < Sisi; ix++)
                {
                    float x = Asal + (ix + 0.5f) * UkuranSel, z = Asal + (iz + 0.5f) * UkuranSel;
                    tinggi[Indeks(ix, iz)] = WorldTerrainGenerator.SampleHeight(x, z);
                }
            var sel = new byte[JumlahSel];
            for (int iz = 0; iz < Sisi; iz++)
                for (int ix = 0; ix < Sisi; ix++)
                {
                    int i = Indeks(ix, iz);
                    float h = tinggi[i];
                    float hk = tinggi[Indeks(Mathf.Max(0, ix - 1), iz)], hn = tinggi[Indeks(Mathf.Min(Sisi - 1, ix + 1), iz)];
                    float hb = tinggi[Indeks(ix, Mathf.Max(0, iz - 1))], ha = tinggi[Indeks(ix, Mathf.Min(Sisi - 1, iz + 1))];
                    float x = Asal + (ix + 0.5f) * UkuranSel, z = Asal + (iz + 0.5f) * UkuranSel;
                    float? wl = WorldTerrainGenerator.GetWaterLevelAt(x, z);
                    bool sungai = wl.HasValue && h >= -2f && wl.Value > 0.01f;
                    sel[i] = Klasifikasi(h, hk, hn, hb, ha, wl, sungai);
                }
            return new SurfaceGrid(sel);
        }

        public byte[] KeBytes()
        {
            var b = new byte[Kepala.Length + 1 + JumlahSel];
            System.Array.Copy(Kepala, b, Kepala.Length);
            b[Kepala.Length] = VersiData;
            System.Array.Copy(_sel, 0, b, Kepala.Length + 1, JumlahSel);
            return b;
        }

        public static SurfaceGrid DariBytes(byte[] b)
        {
            if (b == null || b.Length != Kepala.Length + 1 + JumlahSel) return null;
            for (int i = 0; i < Kepala.Length; i++) if (b[i] != Kepala[i]) return null;
            if (b[Kepala.Length] != VersiData) return null;
            var sel = new byte[JumlahSel];
            System.Array.Copy(b, Kepala.Length + 1, sel, 0, JumlahSel);
            return new SurfaceGrid(sel);
        }

        // Statistik untuk self-test: sel darat (tinggi >= 0 atau sungai) dan yang bisa dilalui.
        public void Hitung(out int darat, out int lewat)
        {
            darat = 0; lewat = 0;
            for (int i = 0; i < JumlahSel; i++) if (_sel[i] != Blok) lewat++;
            for (int iz = 0; iz < Sisi; iz++)
                for (int ix = 0; ix < Sisi; ix++)
                {
                    float x = Asal + (ix + 0.5f) * UkuranSel, z = Asal + (iz + 0.5f) * UkuranSel;
                    if (WorldTerrainGenerator.SampleHeight(x, z) >= 0f || _sel[Indeks(ix, iz)] != Blok) darat++;
                }
        }
    }
}
