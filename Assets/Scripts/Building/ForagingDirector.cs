using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Building
{
    // 03 §7: titik pungut deterministik per sel 25 m (kepadatan per ha per bioma), pool 40 visual dalam 22 m; tumbuh lagi setelah 2 hari game.
    public class ForagingDirector : MonoBehaviour, ISaveable
    {
        public enum JenisPungut { Batu, Sirih, Kunyit, Jahe, Jamur, JamurBeracun, Sarang }
        public const float Sel = 25f, Setengah = 1000f, RadiusPool = 30f, Interval = 0.5f;
        public const int MaksPool = 56, HariTumbuh = 2, Benih = 7331;

        [SerializeField] ForagingNode[] pool = new ForagingNode[0];
        [SerializeField] Vector3[] _pos;
        [SerializeField] JenisPungut[] _jenis;
        [SerializeField] int[] _awalSel, _urutanSel;
        readonly int[] _calon = new int[128];
        public int KandidatDiperiksa { get; private set; }
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
            if (b == Biome.Laut) return 0f;
            bool hutan = b == Biome.HutanJatiSelatan || b == Biome.HutanCampuran || b == Biome.KawahBeringin;
            bool lembap = b == Biome.TepiSungai || b == Biome.HutanBambu;
            switch (j)
            {
                case JenisPungut.Batu: return 11f;
                case JenisPungut.Sirih: return hutan ? 5.5f : lembap ? 3.5f : 1f;
                case JenisPungut.Kunyit: case JenisPungut.Jahe: return hutan ? 4f : lembap ? 2.5f : 0.5f;
                case JenisPungut.Jamur: return hutan || lembap ? 6f : 1.5f;
                case JenisPungut.JamurBeracun: return hutan ? 3.5f : lembap ? 1.5f : 0f;
                default: return (hutan || lembap) ? 1.2f : 0.3f;
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
            IndeksSel();
        }

        void Awake()
        {
            GameServices.Register(this);
            if (_pos == null || _pos.Length == 0 || _jenis == null || _jenis.Length != _pos.Length) Bangkitkan();
            else if (_awalSel == null || _awalSel.Length != SisiSel * SisiSel + 1 || _urutanSel == null || _urutanSel.Length != _pos.Length) IndeksSel();
        }

        static int SisiSel { get { return Mathf.RoundToInt(2f * Setengah / Sel); } }
        static int KoordinatSel(float p) { return Mathf.Clamp(Mathf.FloorToInt((p + Setengah) / Sel), 0, SisiSel - 1); }
        static int NomorSel(Vector3 p) { return KoordinatSel(p.x) * SisiSel + KoordinatSel(p.z); }
        void IndeksSel()
        {
            int n = SisiSel * SisiSel;
            var jumlah = new int[n]; foreach (var p in _pos) jumlah[NomorSel(p)]++;
            _awalSel = new int[n + 1]; for (int i = 0; i < n; i++) _awalSel[i + 1] = _awalSel[i] + jumlah[i];
            _urutanSel = new int[_pos.Length]; var kursor = (int[])_awalSel.Clone();
            for (int i = 0; i < _pos.Length; i++) _urutanSel[kursor[NomorSel(_pos[i])]++] = i;
        }

        public int CariDalamRadius(Vector3 pusat, float radius, int[] hasil)
        {
            KandidatDiperiksa = 0;
            if (_pos == null || _awalSel == null || hasil == null || radius < 0f) return 0;
            int n = 0; float r2 = radius * radius;
            for (int x = KoordinatSel(pusat.x - radius); x <= KoordinatSel(pusat.x + radius); x++)
                for (int z = KoordinatSel(pusat.z - radius); z <= KoordinatSel(pusat.z + radius); z++)
                {
                    int sel = x * SisiSel + z;
                    for (int k = _awalSel[sel]; k < _awalSel[sel + 1]; k++)
                    {
                        int i = _urutanSel[k]; KandidatDiperiksa++;
                        if ((_pos[i] - pusat).sqrMagnitude <= r2 && n < hasil.Length) hasil[n++] = i;
                    }
                }
            return n;
        }

        public Vector3 PosisiTitik(int i) { return _pos[i]; }
        [System.Serializable] class PungutSimpanG3 { public int[] id, hari; }
        public string SaveKey { get { return "pungutan"; } }
        public string CaptureJson()
        {
            var id = new int[_diambil.Count]; _diambil.Keys.CopyTo(id, 0); System.Array.Sort(id);
            var hari = new int[id.Length]; for (int i = 0; i < id.Length; i++) hari[i] = _diambil[id[i]];
            return JsonUtility.ToJson(new PungutSimpanG3 { id = id, hari = hari });
        }
        public void RestoreJson(string json)
        {
            var d = JsonUtility.FromJson<PungutSimpanG3>(json);
            if (d == null || d.id == null || d.hari == null || d.id.Length != d.hari.Length) throw new System.ArgumentException("Data pungutan tidak sah");
            _diambil.Clear(); for (int i = 0; i < d.id.Length; i++) if (d.id[i] >= 0 && d.id[i] < JumlahTitik) _diambil[d.id[i]] = d.hari[i];
            foreach (var p in pool) if (p != null) { p.Indeks = -1; p.gameObject.SetActive(false); }
            _t = Interval;
        }

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
            // Hanya sel sekitar pemain; ID global tetap sama dengan generasi deterministik lama.
            int calon = CariDalamRadius(c, RadiusPool, _calon);
            for (int q = 0; q < calon; q++)
            {
                int i = _calon[q];
                if (!Tersedia(i) || Terpakai(i)) continue;
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
