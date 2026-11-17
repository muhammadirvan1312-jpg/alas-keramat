using System;
using UnityEngine;

namespace AlasKeramat.Core
{
    // Ikon prosedural (H4: pengganti emoji). Kanvas 128x128, latar transparan, garis tepi gelap.
    public static class IconPainter
    {
        public const int Ukuran = 128;
        static readonly Color Tepi = new Color(0.08f, 0.07f, 0.06f, 1f);

        public static readonly string[] NamaIkon =
        {
            "kayu", "batu", "ranting", "daun", "tali", "makanan", "air", "golok", "kapak", "tombak",
            "busur", "panah", "obor", "senter", "peta", "buku", "api", "nyawa", "stamina", "kewarasan"
        };

        public static Texture2D Paint(string nama)
        {
            var c = new Kanvas(Ukuran);
            switch (nama)
            {
                case "kayu": c.Rect(20, 50, 108, 78, H("#8B5A2B")); c.Circle(108, 64, 16, H("#C89B62")); c.Circle(108, 64, 7, H("#8B5A2B")); break;
                case "batu": c.Circle(64, 60, 40, H("#7D7F7C")); c.Circle(50, 72, 14, H("#9A9C98")); break;
                case "ranting": c.Line(20, 24, 108, 104, 7, H("#7A4E26")); c.Line(60, 60, 92, 30, 5, H("#7A4E26")); break;
                case "daun": c.Circle(64, 64, 38, H("#3F7D35")); c.Line(30, 30, 100, 100, 3, H("#244A1F")); break;
                case "tali": for (int i = 0; i < 4; i++) c.Ring(64, 64, 18 + i * 9, 4, H("#B38B4D")); break;
                case "makanan": c.Circle(64, 70, 34, H("#D9822B")); c.Rect(60, 24, 68, 40, H("#5B3A1A")); break;
                case "air": c.Circle(64, 76, 30, H("#3C8DD9")); c.Tri(64, 18, 38, 70, 90, 70, H("#3C8DD9")); break;
                case "golok": c.Line(30, 98, 100, 28, 12, H("#C9CDD2")); c.Line(20, 108, 38, 90, 10, H("#5B3A1A")); break;
                case "kapak": c.Line(36, 108, 76, 28, 7, H("#7A4E26")); c.Rect(66, 22, 100, 52, H("#8E9399")); break;
                case "tombak": c.Line(24, 104, 92, 36, 6, H("#C2A35A")); c.Tri(104, 24, 84, 34, 94, 44, H("#B9BEC4")); break;
                case "busur": c.Ring(40, 64, 50, 6, H("#8B5A2B")); c.Rect(40, 14, 44, 114, H("#E0D6C2")); c.Rect(0, 0, 38, 128, Color.clear); break;
                case "panah": c.Line(20, 108, 100, 28, 4, H("#C2A35A")); c.Tri(108, 20, 88, 28, 100, 40, H("#B9BEC4")); c.Tri(20, 108, 18, 92, 34, 106, H("#E0D6C2")); break;
                case "obor": c.Rect(58, 56, 70, 112, H("#7A4E26")); c.Circle(64, 42, 18, H("#F28C28")); c.Circle(64, 38, 9, H("#FFD45E")); break;
                case "senter": c.Rect(30, 54, 86, 76, H("#3B3F44")); c.Rect(86, 46, 100, 84, H("#5A6067")); c.Tri(100, 64, 126, 40, 126, 88, H("#FFF2B0")); break;
                case "peta": c.Rect(20, 24, 108, 104, H("#D8C49A")); c.Line(30, 90, 98, 40, 3, H("#B23A2E")); c.Circle(98, 40, 6, H("#B23A2E")); break;
                case "buku": c.Rect(26, 22, 102, 106, H("#6E2E2A")); c.Rect(34, 28, 98, 100, H("#EFE6D2")); c.Rect(62, 22, 66, 106, H("#4A1E1B")); break;
                case "api": c.Circle(64, 80, 28, H("#E2581F")); c.Tri(64, 18, 38, 78, 90, 78, H("#E2581F")); c.Circle(64, 86, 14, H("#FFC94A")); break;
                case "nyawa": c.Circle(46, 52, 22, H("#C8312C")); c.Circle(82, 52, 22, H("#C8312C")); c.Tri(64, 108, 26, 60, 102, 60, H("#C8312C")); break;
                case "stamina": c.Tri(70, 12, 36, 70, 64, 70, H("#F2C230")); c.Tri(58, 116, 92, 58, 64, 58, H("#F2C230")); break;
                case "kewarasan": c.Circle(64, 64, 40, H("#E8E2D0")); c.Circle(64, 64, 20, H("#5B7C99")); c.Circle(64, 64, 9, H("#101010")); break;
                default: c.Circle(64, 64, 40, Color.gray); break;
            }
            return c.Selesai("ikon_" + nama);
        }

        static Color H(string hex) { return ProcTex.Hex(hex); }

        // Kanvas piksel sederhana dengan garis tepi otomatis.
        sealed class Kanvas
        {
            readonly int _n;
            readonly Color[] _px;

            public Kanvas(int n) { _n = n; _px = new Color[n * n]; }

            void Set(int x, int y, Color c)
            {
                if (x < 0 || y < 0 || x >= _n || y >= _n) return;
                _px[(_n - 1 - y) * _n + x] = c; // y=0 di atas
            }

            public void Rect(int x0, int y0, int x1, int y1, Color c)
            {
                for (int y = y0; y < y1; y++) for (int x = x0; x < x1; x++) Set(x, y, c);
            }

            public void Circle(int cx, int cy, int r, Color c)
            {
                for (int y = cy - r; y <= cy + r; y++)
                    for (int x = cx - r; x <= cx + r; x++)
                        if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r) Set(x, y, c);
            }

            public void Ring(int cx, int cy, int r, int tebal, Color c)
            {
                int r0 = (r - tebal) * (r - tebal), r1 = r * r;
                for (int y = cy - r; y <= cy + r; y++)
                    for (int x = cx - r; x <= cx + r; x++)
                    {
                        int d = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                        if (d <= r1 && d >= r0) Set(x, y, c);
                    }
            }

            public void Line(int x0, int y0, int x1, int y1, int tebal, Color c)
            {
                int langkah = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));
                for (int i = 0; i <= langkah; i++)
                {
                    float t = langkah == 0 ? 0f : (float)i / langkah;
                    Circle(Mathf.RoundToInt(Mathf.Lerp(x0, x1, t)), Mathf.RoundToInt(Mathf.Lerp(y0, y1, t)), tebal / 2, c);
                }
            }

            public void Tri(int ax, int ay, int bx, int by, int cx, int cy, Color c)
            {
                int minX = Math.Min(ax, Math.Min(bx, cx)), maxX = Math.Max(ax, Math.Max(bx, cx));
                int minY = Math.Min(ay, Math.Min(by, cy)), maxY = Math.Max(ay, Math.Max(by, cy));
                for (int y = minY; y <= maxY; y++)
                    for (int x = minX; x <= maxX; x++)
                    {
                        float d1 = (x - bx) * (ay - by) - (ax - bx) * (y - by);
                        float d2 = (x - cx) * (by - cy) - (bx - cx) * (y - cy);
                        float d3 = (x - ax) * (cy - ay) - (cx - ax) * (y - ay);
                        bool neg = d1 < 0 || d2 < 0 || d3 < 0, pos = d1 > 0 || d2 > 0 || d3 > 0;
                        if (!(neg && pos)) Set(x, y, c);
                    }
            }

            public Texture2D Selesai(string name)
            {
                // Garis tepi 2 px di sekitar piksel terisi.
                var hasil = (Color[])_px.Clone();
                for (int y = 0; y < _n; y++)
                    for (int x = 0; x < _n; x++)
                    {
                        if (_px[y * _n + x].a > 0.5f) continue;
                        bool dekat = false;
                        for (int dy = -2; dy <= 2 && !dekat; dy++)
                            for (int dx = -2; dx <= 2 && !dekat; dx++)
                            {
                                int xx = x + dx, yy = y + dy;
                                if (xx >= 0 && yy >= 0 && xx < _n && yy < _n && _px[yy * _n + xx].a > 0.5f) dekat = true;
                            }
                        if (dekat) hasil[y * _n + x] = Tepi;
                    }
                var t = new Texture2D(_n, _n, TextureFormat.RGBA32, false) { name = name };
                t.SetPixels(hasil);
                t.Apply(false);
                return t;
            }
        }
    }
}
