using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Building
{
    // 03 §7: titik pungut deterministik per sel 25 m (kepadatan per ha per bioma), pool 40 visual dalam 22 m; tumbuh lagi setelah 2 hari game.
    public class ForagingDirector : MonoBehaviour
    {
        public enum JenisPungut { Batu, Sirih, Kunyit, Jahe, Jamur, JamurBeracun, Sarang }
        public const float Sel = 25f, Setengah = 1000f, RadiusPool = 22f, Interval = 0.5f;
        public const int MaksPool = 40, HariTumbuh = 2, Benih = 7331;

        [SerializeField] ForagingNode[] pool = new ForagingNode[0];
        Vector3[] _pos;
        JenisPungut[] _jenis;
        readonly Dictionary<int, int> _diambil = new Dictionary<int, int>();
        TimeOfDayClock _jam;
        float _t;

        public int JumlahTitik { get { return _pos == null ? 0 : _pos.Length; } }
        public void Pasang(ForagingNode[] p) { pool = p; }
        public JenisPungut JenisTitik(int i) { return _jenis[i]; }

        public static ItemID Item(JenisPungut j)
        {
            switch (j)
            {
                case JenisPungut.Batu: return ItemID.Batu; case JenisPungut.Sirih: return ItemID.DaunSirih; case JenisPungut.Kunyit: return ItemID.KunyitObat;
                case JenisPungut.Jahe: return ItemID.JaheHutan; case JenisPungut.Jamur: return ItemID.Jamur; case JenisPungut.JamurBeracun: return ItemID.JamurBeracun;
                default: return ItemID.TelurAyamHutan;
            }
        }

        // Kepadatan per ha (03 §7; batu dikurangi dari 30 ke 8 agar layar tidak penuh batu).
        static float Kepadatan(JenisPungut j, Biome b)
        {
            bool hutan = b == Biome.HutanJatiSelatan || b == Biome.HutanCampuran;
            switch (j)
            {
                case JenisPungut.Batu: return b == Biome.Laut ? 0f : 8f;
                case JenisPungut.Sirih: return hutan ? 3f : 0f;
                case JenisPungut.Kunyit: case JenisPungut.Jahe: return hutan ? 2f : 0f;
                case JenisPungut.Jamur: return hutan || b == Biome.HutanBambu ? 4f : 0f;
                case JenisPungut.JamurBeracun: return hutan ? 3f : 0f;
                default: return b == Biome.HutanCampuran ? 0.5f : 0f;
            }
        }

        public void Bangkitkan()
        {
            var pos = new List<Vector3>(12000); var jen = new List<JenisPungut>(12000);
            var rng = new System.Random(Benih);
            int n = Mathf.RoundToInt(2f * Setengah / Sel);
            float luasHa = Sel * Sel / 10000f;
            var semua = (JenisPungut[])System.Enum.GetValues(typeof(JenisPungut));
            for (int ix = 0; ix < n; ix++)
                for (int iz = 0; iz < n; iz++)
                {
                    float cx = -Setengah + (ix + 0.5f) * Sel, cz = -Setengah + (iz + 0.5f) * Sel;
                    if (WorldTerrainGenerator.SampleHeight(cx, cz) < 0.8f) continue;
                    var b = WorldTerrainGenerator.GetBiome(cx, cz);
                    foreach (var j in semua)
                    {
                        double harap = Kepadatan(j, b) * luasHa;
                        int k = (int)harap + (rng.NextDouble() < harap - (int)harap ? 1 : 0);
                        for (int q = 0; q < k; q++)
                        {
                            float x = cx + (float)(rng.NextDouble() - 0.5) * Sel, z = cz + (float)(rng.NextDouble() - 0.5) * Sel;
                            float h = WorldTerrainGenerator.SampleHeight(x, z);
                            float? wl = WorldTerrainGenerator.GetWaterLevelAt(x, z);
                            if (h < 0.8f || (wl.HasValue && wl.Value > h)) continue;
                            pos.Add(new Vector3(x, h, z)); jen.Add(j);
                        }
                    }
                }
            _pos = pos.ToArray(); _jenis = jen.ToArray();
        }

        void Awake() { GameServices.Register(this); Bangkitkan(); }

        int Hari() { if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>(); return _jam != null ? _jam.Hari : 1; }

        public bool Tersedia(int i) { int h; return !_diambil.TryGetValue(i, out h) || Hari() - h >= HariTumbuh; }

        public void Ambil(int i)
        {
            _diambil[i] = Hari();
            for (int k = 0; k < pool.Length; k++) if (pool[k] != null && pool[k].Indeks == i) { pool[k].Indeks = -1; pool[k].gameObject.SetActive(false); }
        }

        void Update()
        {
            _t += Time.deltaTime;
            if (_t < Interval || _pos == null) return;
            _t = 0f;
            var cam = Camera.main;
            if (cam == null) return;
            Vector3 c = cam.transform.position;
            float r2 = RadiusPool * RadiusPool;
            for (int k = 0; k < pool.Length; k++)
            {
                var p = pool[k];
                if (p == null || p.Indeks < 0) continue;
                if ((_pos[p.Indeks] - c).sqrMagnitude > r2 * 1.4f) { p.Indeks = -1; p.gameObject.SetActive(false); }
            }
            // Pencarian linear murah (±10 rb titik, tiap 0,5 s) tanpa alokasi.
            for (int i = 0; i < _pos.Length; i++)
            {
                if ((_pos[i] - c).sqrMagnitude > r2 || !Tersedia(i) || Terpakai(i)) continue;
                var slot = Kosong();
                if (slot == null) return;
                slot.Indeks = i; slot.Tipe = _jenis[i];
                slot.transform.position = _pos[i];
                for (int v = 0; v < slot.transform.childCount; v++) slot.transform.GetChild(v).gameObject.SetActive(v == (int)_jenis[i]);
                slot.gameObject.SetActive(true);
            }
        }

        bool Terpakai(int i) { for (int k = 0; k < pool.Length; k++) if (pool[k] != null && pool[k].Indeks == i) return true; return false; }
        ForagingNode Kosong() { for (int k = 0; k < pool.Length; k++) if (pool[k] != null && pool[k].Indeks < 0) return pool[k]; return null; }
    }
}
