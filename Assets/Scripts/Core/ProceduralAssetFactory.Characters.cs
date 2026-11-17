using System.Collections.Generic;
using UnityEngine;

namespace AlasKeramat.Core
{
    // 02 §1.1-1.4: kerangka 20 sendi + 4 sendi wajah, mesh "loft ellipse" rigid-skinned satu mesh, atlas warna 8x8 sel.
    // T4a: mesh prosedural; T4b (poles) menggantinya dengan model MakeHuman/MPFB berkerangka sama (22 §2).
    public static class Kerangka
    {
        public const int Pelvis = 0, Spine = 1, Chest = 2, Neck = 3, Head = 4, Jaw = 5, LClav = 6, RClav = 7, LUpper = 8, RUpper = 9,
            LFore = 10, RFore = 11, LHand = 12, RHand = 13, LThigh = 14, RThigh = 15, LCalf = 16, RCalf = 17, LFoot = 18, RFoot = 19,
            LEye = 20, REye = 21, LBrow = 22, RBrow = 23, Jumlah = 24;
        public static readonly string[] Nama =
        {
            "Pelvis", "Spine", "Chest", "Neck", "Head", "Jaw", "L_Clavicle", "R_Clavicle", "L_UpperArm", "R_UpperArm",
            "L_Forearm", "R_Forearm", "L_Hand", "R_Hand", "L_Thigh", "R_Thigh", "L_Calf", "R_Calf", "L_Foot", "R_Foot",
            "L_Eye", "R_Eye", "L_Brow", "R_Brow"
        };
        public static readonly int[] Induk = { -1, 0, 1, 2, 3, 4, 2, 2, 6, 7, 8, 9, 10, 11, 0, 0, 14, 15, 16, 17, 4, 4, 4, 4 };
        // Posisi model-space untuk tinggi 1,75 m (02 §1.1). Kiri = -X (karakter menghadap +Z).
        public static readonly Vector3[] Posisi =
        {
            new Vector3(0f, 0.98f, 0f), new Vector3(0f, 1.10f, 0f), new Vector3(0f, 1.28f, 0f), new Vector3(0f, 1.48f, 0f), new Vector3(0f, 1.58f, 0.01f),
            new Vector3(0f, 1.60f, 0.035f), new Vector3(-0.04f, 1.44f, 0f), new Vector3(0.04f, 1.44f, 0f), new Vector3(-0.19f, 1.43f, 0f), new Vector3(0.19f, 1.43f, 0f),
            new Vector3(-0.21f, 1.14f, 0f), new Vector3(0.21f, 1.14f, 0f), new Vector3(-0.22f, 0.88f, 0.01f), new Vector3(0.22f, 0.88f, 0.01f),
            new Vector3(-0.10f, 0.95f, 0f), new Vector3(0.10f, 0.95f, 0f), new Vector3(-0.10f, 0.52f, 0.01f), new Vector3(0.10f, 0.52f, 0.01f),
            new Vector3(-0.10f, 0.09f, 0f), new Vector3(0.10f, 0.09f, 0f),
            new Vector3(-0.032f, 1.676f, 0.092f), new Vector3(0.032f, 1.676f, 0.092f), new Vector3(-0.033f, 1.705f, 0.096f), new Vector3(0.033f, 1.705f, 0.096f)
        };

        public static Vector3 Skala(Vector3 p, float tinggi, float lebar) { return new Vector3(p.x * lebar, p.y * tinggi / 1.75f, p.z * lebar); }

        // Membuat transform tulang di bawah root (pose-I, rotasi identitas). Mengembalikan array berurutan sesuai indeks.
        public static Transform[] Buat(Transform root, float tinggi, float lebar)
        {
            var t = new Transform[Jumlah];
            for (int i = 0; i < Jumlah; i++)
            {
                var go = new GameObject(Nama[i]);
                t[i] = go.transform;
                t[i].SetParent(Induk[i] < 0 ? root : t[Induk[i]], false);
                t[i].position = root.TransformPoint(Skala(Posisi[i], tinggi, lebar));
            }
            return t;
        }
    }

    // Pembangun mesh karakter: semua bagian ke satu mesh, tiap vertex terikat satu tulang (rigid), UV ke pusat sel atlas.
    public class CharacterMeshBuilder
    {
        public const int SelPerSisi = 8, UkuranAtlas = 512, Keliling = 10;
        readonly List<Vector3> _v = new List<Vector3>();
        readonly List<Vector3> _n = new List<Vector3>();
        readonly List<Vector2> _uv = new List<Vector2>();
        readonly List<int> _t = new List<int>();
        readonly List<BoneWeight> _w = new List<BoneWeight>();

        public int Segitiga { get { return _t.Count / 3; } }

        public static Vector2 UVSel(int sel)
        {
            float u = (sel % SelPerSisi + 0.5f) / SelPerSisi, v = (sel / SelPerSisi + 0.5f) / SelPerSisi;
            return new Vector2(u, v);
        }

        int Tambah(Vector3 p, Vector3 n, int tulang, int sel)
        {
            _v.Add(p); _n.Add(n); _uv.Add(UVSel(sel)); _w.Add(new BoneWeight { boneIndex0 = tulang, weight0 = 1f });
            return _v.Count - 1;
        }

        // Loft elips dari a ke b. stasiun: (posisi 0..1, rX, rZ) dalam meter.
        public void Loft(Vector3 a, Vector3 b, Vector3[] stasiun, int tulang, int sel, bool tutup = true, int keliling = Keliling)
        {
            Vector3 sumbu = (b - a).normalized;
            Vector3 kanan = Vector3.ProjectOnPlane(Mathf.Abs(Vector3.Dot(sumbu, Vector3.right)) > 0.9f ? Vector3.up : Vector3.right, sumbu).normalized;
            Vector3 depan = Vector3.Cross(kanan, sumbu).normalized;
            int awal = _v.Count;
            for (int s = 0; s < stasiun.Length; s++)
            {
                Vector3 c = Vector3.Lerp(a, b, stasiun[s].x);
                for (int k = 0; k < keliling; k++)
                {
                    float sud = k * Mathf.PI * 2f / keliling;
                    Vector3 o = kanan * Mathf.Cos(sud) * stasiun[s].y + depan * Mathf.Sin(sud) * stasiun[s].z;
                    Tambah(c + o, o.normalized, tulang, sel);
                }
            }
            for (int s = 0; s < stasiun.Length - 1; s++)
                for (int k = 0; k < keliling; k++)
                {
                    int i0 = awal + s * keliling + k, i1 = awal + s * keliling + (k + 1) % keliling;
                    int j0 = i0 + keliling, j1 = i1 + keliling;
                    _t.Add(i0); _t.Add(j0); _t.Add(i1); _t.Add(i1); _t.Add(j0); _t.Add(j1);
                }
            if (!tutup) return;
            Tutup(awal, keliling, Vector3.Lerp(a, b, stasiun[0].x), -sumbu, tulang, sel, true);
            Tutup(awal + (stasiun.Length - 1) * keliling, keliling, Vector3.Lerp(a, b, stasiun[stasiun.Length - 1].x), sumbu, tulang, sel, false);
        }

        void Tutup(int cincin, int keliling, Vector3 pusat, Vector3 n, int tulang, int sel, bool bawah)
        {
            int c = Tambah(pusat, n, tulang, sel);
            for (int k = 0; k < keliling; k++)
            {
                int i0 = cincin + k, i1 = cincin + (k + 1) % keliling;
                if (bawah) { _t.Add(c); _t.Add(i0); _t.Add(i1); } else { _t.Add(c); _t.Add(i1); _t.Add(i0); }
            }
        }

        // Elipsoid (UV-sphere) berpusat c dengan jari-jari r.
        public void Bola(Vector3 c, Vector3 r, int tulang, int sel, int seg = 12, int cincin = 8)
        {
            int awal = _v.Count;
            for (int y = 0; y <= cincin; y++)
            {
                float v = Mathf.PI * y / cincin;
                for (int x = 0; x <= seg; x++)
                {
                    float u = Mathf.PI * 2f * x / seg;
                    Vector3 n = new Vector3(Mathf.Sin(v) * Mathf.Cos(u), Mathf.Cos(v), Mathf.Sin(v) * Mathf.Sin(u));
                    Tambah(c + Vector3.Scale(n, r), n, tulang, sel);
                }
            }
            for (int y = 0; y < cincin; y++)
                for (int x = 0; x < seg; x++)
                {
                    int i0 = awal + y * (seg + 1) + x, i1 = i0 + 1, j0 = i0 + seg + 1, j1 = j0 + 1;
                    _t.Add(i0); _t.Add(i1); _t.Add(j0); _t.Add(i1); _t.Add(j1); _t.Add(j0);
                }
        }

        // Kotak membulat sederhana (6 sisi) berorientasi rot.
        public void Kotak(Vector3 c, Vector3 ukuran, Quaternion rot, int tulang, int sel)
        {
            Vector3 h = ukuran * 0.5f;
            Vector3[] arah = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
            foreach (var n in arah)
            {
                Vector3 u = Mathf.Abs(n.y) > 0.5f ? Vector3.right : Vector3.up;
                Vector3 w = Vector3.Cross(n, u);
                int a = _v.Count;
                for (int q = 0; q < 4; q++)
                {
                    float su = (q == 0 || q == 3) ? -1f : 1f, sw = q < 2 ? -1f : 1f;
                    Vector3 p = Vector3.Scale(n + u * su + w * sw, h);
                    Tambah(c + rot * p, rot * n, tulang, sel);
                }
                _t.Add(a); _t.Add(a + 1); _t.Add(a + 2); _t.Add(a); _t.Add(a + 2); _t.Add(a + 3);
            }
        }

        // Kerucut (gumpalan rambut, tanduk, hidung) dari dasar c ke puncak.
        public void Kerucut(Vector3 c, Vector3 puncak, float r, int tulang, int sel, int seg = 6)
        {
            Loft(c, puncak, new[] { new Vector3(0f, r, r), new Vector3(1f, 0.002f, 0.002f) }, tulang, sel, true, seg);
        }

        public Mesh Selesai(string nama, Transform root, Transform[] tulang)
        {
            var m = new Mesh { name = nama };
            if (_v.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.SetVertices(_v); m.SetNormals(_n); m.SetUVs(0, _uv); m.SetTriangles(_t, 0);
            m.boneWeights = _w.ToArray();
            var bp = new Matrix4x4[tulang.Length];
            for (int i = 0; i < tulang.Length; i++) bp[i] = tulang[i].worldToLocalMatrix * root.localToWorldMatrix;
            m.bindposes = bp;
            m.RecalculateBounds();
            return m;
        }

        public static Texture2D Atlas(Color[] warna)
        {
            var tex = new Texture2D(UkuranAtlas, UkuranAtlas, TextureFormat.RGBA32, true) { name = "AtlasKarakter", filterMode = FilterMode.Bilinear };
            int sel = UkuranAtlas / SelPerSisi;
            var px = new Color32[UkuranAtlas * UkuranAtlas];
            for (int y = 0; y < UkuranAtlas; y++)
                for (int x = 0; x < UkuranAtlas; x++)
                {
                    int i = (y / sel) * SelPerSisi + x / sel;
                    Color c = i < warna.Length ? warna[i] : Color.magenta;
                    float n = Mathf.PerlinNoise(x * 0.11f, y * 0.11f) * 0.08f - 0.04f; // serat kain/kulit halus
                    c = new Color(Mathf.Clamp01(c.r + n), Mathf.Clamp01(c.g + n), Mathf.Clamp01(c.b + n), ProcTex.KilauAlbedo);
                    px[y * UkuranAtlas + x] = c;
                }
            tex.SetPixels32(px); tex.Apply(true);
            return tex;
        }
    }
}
