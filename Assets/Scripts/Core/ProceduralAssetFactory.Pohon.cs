using System.Collections.Generic;
using UnityEngine;

namespace AlasKeramat.Core
{
    // Mesh pohon & semak prosedural (2 submesh: 0 = kayu, 1 = daun cutout). Deterministik per seed.
    public static class ProcPohon
    {
        public enum Jenis { Jati = 0, Mahoni = 1, Kelapa = 2, Aren = 3, SawitHangus = 4, BambuRumpun = 5, Pisang = 6, BeringinKecil = 7, Tebu = 8, Semak = 9, Pakis = 10 }
        public const int JumlahJenis = 11;

        sealed class Pembangun
        {
            public readonly List<Vector3> v = new List<Vector3>();
            public readonly List<Vector3> n = new List<Vector3>();
            public readonly List<Vector2> uv = new List<Vector2>();
            public readonly List<Color> c = new List<Color>();
            public readonly List<int> kayu = new List<int>();
            public readonly List<int> daun = new List<int>();

            // Tabung meruncing di sepanjang jalur (UV u = keliling, v = panjang/2 m).
            public void Tabung(Vector3[] jalur, float[] radius, int sisi, Color ao)
            {
                int awal = v.Count; float panjang = 0f;
                for (int i = 0; i < jalur.Length; i++)
                {
                    Vector3 arah = (i < jalur.Length - 1 ? jalur[i + 1] - jalur[i] : jalur[i] - jalur[i - 1]).normalized;
                    Vector3 kanan = Vector3.Cross(arah, Mathf.Abs(arah.y) > 0.95f ? Vector3.right : Vector3.up).normalized;
                    Vector3 depan = Vector3.Cross(kanan, arah);
                    if (i > 0) panjang += Vector3.Distance(jalur[i], jalur[i - 1]);
                    for (int s = 0; s <= sisi; s++)
                    {
                        float a = s * Mathf.PI * 2f / sisi;
                        Vector3 nn = kanan * Mathf.Cos(a) + depan * Mathf.Sin(a);
                        v.Add(jalur[i] + nn * radius[i]); n.Add(nn);
                        uv.Add(new Vector2((float)s / sisi, panjang / 2f));
                        c.Add(i == 0 ? ao : Color.white);
                    }
                }
                int baris = sisi + 1;
                for (int i = 0; i < jalur.Length - 1; i++)
                    for (int s = 0; s < sisi; s++)
                    {
                        int a = awal + i * baris + s, b = a + baris;
                        kayu.Add(a); kayu.Add(b); kayu.Add(a + 1); kayu.Add(a + 1); kayu.Add(b); kayu.Add(b + 1);
                    }
            }

            // Kartu daun dua sisi (quad) dengan sel atlas 2x2 dan sedikit lengkungan + AO ke pusat tajuk.
            public void Kartu(Vector3 pusat, Vector3 kanan, Vector3 atas, int sel, Color warna, Vector3 pusatTajuk)
            {
                float u0 = (sel % 2) * 0.5f, v0 = (sel / 2) * 0.5f;
                Vector3 keluar = (pusat - pusatTajuk);
                Vector3 nKeluar = keluar.sqrMagnitude > 0.001f ? keluar.normalized : Vector3.up;
                Vector3 nrm = Vector3.Cross(kanan, atas).normalized;
                Vector3 nAtas = (nrm * 0.35f + nKeluar * 0.55f + Vector3.up * 0.55f).normalized;
                Vector3 tekuk = nKeluar * (kanan.magnitude * 0.16f);
                int i = v.Count;
                v.Add(pusat - kanan - atas - tekuk); v.Add(pusat + kanan - atas - tekuk); v.Add(pusat + kanan + atas + tekuk); v.Add(pusat - kanan + atas + tekuk);
                for (int k = 0; k < 4; k++) { n.Add(nAtas); c.Add(warna); }
                uv.Add(new Vector2(u0, v0)); uv.Add(new Vector2(u0 + 0.5f, v0)); uv.Add(new Vector2(u0 + 0.5f, v0 + 0.5f)); uv.Add(new Vector2(u0, v0 + 0.5f));
                daun.Add(i); daun.Add(i + 2); daun.Add(i + 1); daun.Add(i); daun.Add(i + 3); daun.Add(i + 2);
            }

            // Gumpalan daun: kartu berlapis dari inti gelap (AO) ke pucuk terang menghadap luar.
            public void Gumpal(System.Random r, Vector3 pusat, float rad, int kartu, float ukuran, int sel)
            {
                for (int k = 0; k < kartu; k++)
                {
                    Vector3 arah = Random3(r);
                    if (arah.sqrMagnitude < 0.001f) arah = Vector3.up;
                    arah.Normalize();
                    float lapis = Mathf.Pow((float)r.NextDouble(), 0.65f);
                    Vector3 off = new Vector3(arah.x * rad * 0.8f, (arah.y * 0.55f + 0.15f) * rad, arah.z * rad * 0.8f) * lapis;
                    Vector3 tangen = Vector3.Cross(arah, Mathf.Abs(arah.y) > 0.9f ? Vector3.right : Vector3.up).normalized;
                    Vector3 bitangen = Vector3.Cross(tangen, arah).normalized;
                    float putar = (float)r.NextDouble() * Mathf.PI * 2f;
                    Vector3 kanan = (tangen * Mathf.Cos(putar) + bitangen * Mathf.Sin(putar)) * (ukuran * Mathf.Lerp(1.08f, 0.86f, lapis));
                    Vector3 atas = (-tangen * Mathf.Sin(putar) + bitangen * Mathf.Cos(putar) + Vector3.up * 0.25f).normalized * (ukuran * Mathf.Lerp(1.08f, 0.86f, lapis));
                    float ao = Mathf.Lerp(0.58f, 1.06f, lapis) * (0.88f + (float)r.NextDouble() * 0.22f);
                    Kartu(pusat + off, kanan, atas, sel, new Color(ao, ao, ao, 1f), pusat);
                }
            }

            // Pelepah (daun palem/pisang): strip melengkung dari pangkal ke ujung.
            public void Pelepah(Vector3 pangkal, Vector3 arah, float panjang, float lebar, float lengkung, int segmen, int sel)
            {
                Vector3 samping = Vector3.Cross(arah, Vector3.up).normalized;
                if (samping.sqrMagnitude < 0.01f) samping = Vector3.right;
                Vector3 p = pangkal;
                for (int s = 0; s < segmen; s++)
                {
                    float t = (float)s / segmen, t2 = (float)(s + 1) / segmen;
                    Vector3 q = pangkal + arah * panjang * t2 + Vector3.down * lengkung * t2 * t2;
                    Vector3 tengah = (p + q) * 0.5f;
                    float w = lebar * Mathf.Sin(Mathf.PI * Mathf.Lerp(t, t2, 0.5f) * 0.9f + 0.15f);
                    float ao = Mathf.Lerp(0.72f, 1.04f, t2);
                    Kartu(tengah, samping * w, (q - p) * 0.5f, sel, new Color(ao, ao, ao, 1f), pangkal);
                    p = q;
                }
            }

            public Mesh Jadi(string nama)
            {
                var m = new Mesh { name = nama };
                if (v.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                m.SetVertices(v); m.SetNormals(n); m.SetUVs(0, uv); m.SetColors(c);
                m.subMeshCount = 2; m.SetTriangles(kayu, 0); m.SetTriangles(daun, 1);
                m.RecalculateTangents(); m.RecalculateBounds();
                return m;
            }
        }

        static Vector3 Random3(System.Random r) { return new Vector3((float)r.NextDouble() * 2f - 1f, (float)r.NextDouble() * 2f - 1f, (float)r.NextDouble() * 2f - 1f); }
        static float R(System.Random r, float a, float b) { return a + (b - a) * (float)r.NextDouble(); }

        // Sel atlas daun: 0 = daun lebar, 1 = daun kecil rapat, 2 = daun panjang (palem/bambu), 3 = daun kering/hangus.
        public static Mesh Buat(Jenis j, int varian, bool lod1)
        {
            var r = new System.Random(9173 + (int)j * 101 + varian * 7);
            var b = new Pembangun();
            int sisi = lod1 ? 5 : 9;
            float kurang = lod1 ? 0.4f : 1f;
            switch (j)
            {
                case Jenis.Jati: Pohon(b, r, R(r, 17f, 23f), 0.32f, 0.55f, 6, 3.2f, 1.25f, 0, sisi, kurang); break;
                case Jenis.Mahoni: Pohon(b, r, R(r, 14f, 19f), 0.38f, 0.45f, 9, 3.6f, 0.9f, 1, sisi, kurang); break;
                case Jenis.BeringinKecil: Pohon(b, r, R(r, 11f, 14f), 0.6f, 0.4f, 11, 4.2f, 0.8f, 1, sisi, kurang); AkarGantung(b, r, 10, sisi); break;
                case Jenis.Kelapa: Palem(b, r, R(r, 14f, 19f), 0.22f, 12, 4.6f, 1.6f, 2, sisi, kurang, R(r, 0.6f, 2.4f)); break;
                case Jenis.Aren: Palem(b, r, R(r, 9f, 13f), 0.3f, 14, 4.0f, 2.2f, 2, sisi, kurang, 0.2f); break;
                case Jenis.SawitHangus: Palem(b, r, R(r, 5f, 8f), 0.32f, 6, 1.4f, 0.6f, 3, sisi, kurang, 0.1f); break;
                case Jenis.Pisang: Pisang(b, r, sisi, kurang); break;
                case Jenis.BambuRumpun: Rumpun(b, r, 18, R(r, 10f, 14f), 0.055f, 2, sisi, kurang); break;
                case Jenis.Tebu: Rumpun(b, r, 12, R(r, 2.6f, 3.4f), 0.025f, 2, 4, kurang); break;
                case Jenis.Semak: b.Gumpal(r, new Vector3(0f, 0.6f, 0f), 1.1f, Mathf.Max(3, (int)(9 * kurang)), 0.55f, 1); break;
                case Jenis.Pakis:
                    for (int k = 0; k < (lod1 ? 5 : 9); k++)
                    {
                        float a = k * Mathf.PI * 2f / (lod1 ? 5 : 9) + R(r, -0.2f, 0.2f);
                        b.Pelepah(new Vector3(0f, 0.05f, 0f), new Vector3(Mathf.Cos(a), 0.9f, Mathf.Sin(a)).normalized, 1.0f, 0.22f, 0.45f, lod1 ? 2 : 4, 2);
                    }
                    break;
            }
            return b.Jadi(j + "_" + varian + (lod1 ? "_LOD1" : "_LOD0"));
        }

        static void Pohon(Pembangun b, System.Random r, float tinggi, float rBatang, float awalTajuk, int gumpal, float radGumpal, float ukuranDaun, int sel, int sisi, float kurang)
        {
            int seg = 6;
            var jalur = new Vector3[seg + 1]; var rad = new float[seg + 1];
            Vector3 lean = new Vector3(R(r, -0.08f, 0.08f), 0f, R(r, -0.08f, 0.08f));
            for (int i = 0; i <= seg; i++)
            {
                float t = (float)i / seg;
                jalur[i] = new Vector3(lean.x * t * tinggi + Mathf.Sin(t * 5f) * 0.15f, t * tinggi * 0.92f, lean.z * t * tinggi);
                rad[i] = rBatang * Mathf.Lerp(1.38f, 0.32f, t) * (i == 0 ? 1.55f : i == 1 ? 1.15f : 1f);
            }
            b.Tabung(jalur, rad, sisi, new Color(0.52f, 0.52f, 0.52f, 1f));
            if (kurang >= 1f)
            {
                for (int ak = 0; ak < 4; ak++)
                {
                    float a = ak * Mathf.PI * 0.5f + R(r, -0.25f, 0.25f);
                    Vector3 arah = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                    b.Tabung(new[] { new Vector3(0f, rBatang * 2.8f, 0f) + arah * (rBatang * 0.45f), new Vector3(0f, rBatang * 1.1f, 0f) + arah * (rBatang * 1.35f), arah * (rBatang * 2.3f) },
                        new[] { rBatang * 0.36f, rBatang * 0.48f, rBatang * 0.22f }, 4, new Color(0.45f, 0.45f, 0.45f, 1f));
                }
            }
            int nGumpal = Mathf.Max(3, Mathf.RoundToInt(gumpal * Mathf.Lerp(0.6f, 1f, kurang)));
            for (int g = 0; g < nGumpal; g++)
            {
                float t = Mathf.Lerp(awalTajuk, 1f, (float)r.NextDouble());
                float sudut = (float)r.NextDouble() * Mathf.PI * 2f, jauh = radGumpal * Mathf.Lerp(0.3f, 1.2f, (float)r.NextDouble()) * (1.1f - t * 0.4f);
                Vector3 pangkal = Vector3.Lerp(jalur[0], jalur[seg], t);
                Vector3 ujung = pangkal + new Vector3(Mathf.Cos(sudut) * jauh, R(r, 0.5f, 2f), Mathf.Sin(sudut) * jauh);
                if (kurang >= 1f) b.Tabung(new[] { pangkal, Vector3.Lerp(pangkal, ujung, 0.5f) + Vector3.up * 0.4f, ujung }, new[] { rBatang * 0.35f, rBatang * 0.2f, rBatang * 0.08f }, 5, Color.white);
                b.Gumpal(r, ujung, radGumpal, Mathf.Max(4, (int)(14 * kurang)), ukuranDaun * (kurang < 1f ? 1.6f : 1f), sel);
            }
            b.Gumpal(r, jalur[seg] + Vector3.up * 0.8f, radGumpal * 1.1f, Mathf.Max(4, (int)(16 * kurang)), ukuranDaun * (kurang < 1f ? 1.6f : 1f), sel);
        }

        static void AkarGantung(Pembangun b, System.Random r, int n, int sisi)
        {
            for (int k = 0; k < n; k++)
            {
                float a = R(r, 0f, Mathf.PI * 2f), d = R(r, 1.5f, 3.5f), y = R(r, 6f, 9f);
                var atas = new Vector3(Mathf.Cos(a) * d, y, Mathf.Sin(a) * d);
                b.Tabung(new[] { atas, new Vector3(atas.x, y * 0.4f, atas.z), new Vector3(atas.x * 1.05f, 0f, atas.z * 1.05f) }, new[] { 0.05f, 0.06f, 0.08f }, Mathf.Max(4, sisi / 2), Color.white);
            }
        }

        static void Palem(Pembangun b, System.Random r, float tinggi, float rBatang, int pelepah, float panjang, float lengkung, int sel, int sisi, float kurang, float condong)
        {
            int seg = 8;
            var jalur = new Vector3[seg + 1]; var rad = new float[seg + 1];
            float arahC = R(r, 0f, Mathf.PI * 2f);
            for (int i = 0; i <= seg; i++)
            {
                float t = (float)i / seg;
                float geser = condong * t * t * 3f;
                jalur[i] = new Vector3(Mathf.Cos(arahC) * geser, t * tinggi, Mathf.Sin(arahC) * geser);
                rad[i] = rBatang * Mathf.Lerp(1.25f, 0.8f, t);
            }
            b.Tabung(jalur, rad, sisi, new Color(0.6f, 0.6f, 0.6f, 1f));
            int np = Mathf.Max(5, Mathf.RoundToInt(pelepah * Mathf.Lerp(0.6f, 1f, kurang)));
            for (int k = 0; k < np; k++)
            {
                float a = k * Mathf.PI * 2f / np + R(r, -0.15f, 0.15f);
                var arah = new Vector3(Mathf.Cos(a), R(r, 0.15f, 0.55f), Mathf.Sin(a)).normalized;
                b.Pelepah(jalur[seg], arah, panjang * R(r, 0.85f, 1.1f), 0.55f, lengkung, kurang < 1f ? 3 : 6, sel);
            }
        }

        static void Pisang(Pembangun b, System.Random r, int sisi, float kurang)
        {
            float tinggi = R(r, 2.6f, 3.8f);
            b.Tabung(new[] { Vector3.zero, new Vector3(0f, tinggi * 0.5f, 0f), new Vector3(0.05f, tinggi, 0f) }, new[] { 0.17f, 0.14f, 0.1f }, sisi, new Color(0.6f, 0.6f, 0.6f, 1f));
            int n = kurang < 1f ? 5 : 8;
            for (int k = 0; k < n; k++)
            {
                float a = k * Mathf.PI * 2f / n + R(r, -0.2f, 0.2f);
                b.Pelepah(new Vector3(0f, tinggi, 0f), new Vector3(Mathf.Cos(a), R(r, 0.5f, 1.1f), Mathf.Sin(a)).normalized, R(r, 1.8f, 2.4f), 0.9f, 0.9f, kurang < 1f ? 2 : 4, 0);
            }
        }

        static void Rumpun(Pembangun b, System.Random r, int batang, float tinggi, float rad, int sel, int sisi, float kurang)
        {
            int nb = Mathf.Max(5, Mathf.RoundToInt(batang * Mathf.Lerp(0.5f, 1f, kurang)));
            for (int k = 0; k < nb; k++)
            {
                float a = R(r, 0f, Mathf.PI * 2f), d = R(r, 0.1f, 0.9f), h = tinggi * R(r, 0.75f, 1.05f);
                var dasar = new Vector3(Mathf.Cos(a) * d, 0f, Mathf.Sin(a) * d);
                var luar = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * h * 0.18f;
                var tengah = dasar + Vector3.up * h * 0.55f + luar * 0.35f;
                var ujung = dasar + Vector3.up * h + luar;
                b.Tabung(new[] { dasar, tengah, ujung }, new[] { rad, rad * 0.85f, rad * 0.5f }, Mathf.Max(4, sisi / 2), new Color(0.6f, 0.6f, 0.6f, 1f));
                int daun = Mathf.Max(2, (int)(5 * kurang));
                for (int q = 0; q < daun; q++)
                    b.Gumpal(r, Vector3.Lerp(tengah, ujung, 0.4f + 0.6f * q / daun), h * 0.07f, 2, h * 0.06f, sel);
            }
        }

        // Atlas daun 2x2 (alpha untuk cutout): lebar, kecil rapat, panjang, kering.
        public static Texture2D AtlasDaun(int ukuran)
        {
            var t = new Texture2D(ukuran, ukuran, TextureFormat.RGBA32, true) { name = "atlas_daun" };
            var px = new Color[ukuran * ukuran];
            int h = ukuran / 2;
            var r = new System.Random(4242);
            Color[] dasar = { ProcTex.Hex("#3E6B2A"), ProcTex.Hex("#2F5A23"), ProcTex.Hex("#5C7D2E"), ProcTex.Hex("#3A2A1C") };
            for (int sel = 0; sel < 4; sel++)
            {
                int ox = (sel % 2) * h, oy = (sel / 2) * h;
                int daun = sel == 1 ? 40 : sel == 2 ? 14 : 18;
                for (int k = 0; k < daun; k++)
                {
                    float cx = (float)r.NextDouble(), cy = (float)r.NextDouble();
                    float rx = sel == 2 ? 0.035f : sel == 1 ? 0.06f : 0.11f, ry = sel == 2 ? 0.32f : rx * 1.9f;
                    float ang = (float)r.NextDouble() * Mathf.PI;
                    float g = 0.75f + (float)r.NextDouble() * 0.45f;
                    for (int y = 0; y < h; y++)
                        for (int x = 0; x < h; x++)
                        {
                            float u = (float)x / h - cx, v = (float)y / h - cy;
                            float ru = u * Mathf.Cos(ang) + v * Mathf.Sin(ang), rv = -u * Mathf.Sin(ang) + v * Mathf.Cos(ang);
                            float e = (ru * ru) / (rx * rx) + (rv * rv) / (ry * ry);
                            if (e > 1f) continue;
                            float urat = Mathf.Abs(ru) < rx * 0.08f ? 0.8f : 1f;
                            var col = dasar[sel] * g * urat * (1.05f - e * 0.25f);
                            col.a = 1f;
                            px[(oy + y) * ukuran + ox + x] = col;
                        }
                }
            }
            t.SetPixels(px);
            t.Apply(true);
            return t;
        }
    }
}
