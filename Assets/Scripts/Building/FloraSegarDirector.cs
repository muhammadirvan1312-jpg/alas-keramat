using System.Collections.Generic;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Flora & material alam tambahan agar crafting bangunan, air & senjata terasa melimpah tanpa mengubah kontrak 7 jenis ForagingDirector.
    public class FloraSegarDirector : MonoBehaviour
    {
        public enum JenisFlora { DaunPisang = 0, Ranting = 1, BambuMuda = 2, Kelapa = 3, Pisang = 4, SeratIjuk = 5, Damar = 6, Kepiting = 7 }
        public const float Sel = 25f, Setengah = 1000f, RadiusPool = 28f, Interval = 0.6f;
        public const int MaksPool = 36, HariTumbuh = 2, Benih = 9187;

        [SerializeField] ForagingExtraNode[] pool = new ForagingExtraNode[0];
        [SerializeField] Vector3[] _pos;
        [SerializeField] JenisFlora[] _jenis;
        [SerializeField] int[] _awalSel, _urutanSel;
        readonly int[] _calon = new int[96];
        readonly Dictionary<int, int> _diambil = new Dictionary<int, int>();
        TimeOfDayClock _jam;
        float _t;

        public void Pasang(ForagingExtraNode[] p) { pool = p; }

        public static ItemID Item(JenisFlora j)
        {
            switch (j)
            {
                case JenisFlora.DaunPisang: return ItemID.DaunPisang;
                case JenisFlora.Ranting: return ItemID.Ranting;
                case JenisFlora.BambuMuda: return ItemID.Bambu;
                case JenisFlora.Kelapa: return ItemID.KelapaMuda;
                case JenisFlora.Pisang: return ItemID.Pisang;
                case JenisFlora.SeratIjuk: return ItemID.SeratIjuk;
                case JenisFlora.Damar: return ItemID.Damar;
                default: return ItemID.Kepiting;
            }
        }

        static float Kepadatan(JenisFlora j, Biome b)
        {
            if (b == Biome.Laut) return 0f;
            bool hutan = b == Biome.HutanJatiSelatan || b == Biome.HutanCampuran || b == Biome.KawahBeringin;
            bool sungai = b == Biome.TepiSungai || b == Biome.Pantai;
            switch (j)
            {
                case JenisFlora.DaunPisang: return hutan || sungai ? 6f : b == Biome.Desa ? 3f : 2f;
                case JenisFlora.Ranting: return 10f;
                case JenisFlora.BambuMuda: return b == Biome.HutanBambu ? 12f : sungai ? 4f : 1.5f;
                case JenisFlora.Kelapa: return sungai ? 6f : hutan ? 1.5f : 0f;
                case JenisFlora.Pisang: return hutan || b == Biome.Desa || b == Biome.TepiSungai ? 4f : 0f;
                case JenisFlora.SeratIjuk: return hutan || b == Biome.HutanBambu ? 5f : 1f;
                case JenisFlora.Damar: return hutan ? 4f : 0.5f;
                default: return sungai ? 7f : 0f;
            }
        }

        public void Bangkitkan()
        {
            var pos = new List<Vector3>(14000); var jen = new List<JenisFlora>(14000);
            var rng = new System.Random(Benih);
            int n = Mathf.RoundToInt(2f * Setengah / Sel);
            float luasHa = Sel * Sel / 10000f;
            var semua = (JenisFlora[])System.Enum.GetValues(typeof(JenisFlora));
            for (int ix = 0; ix < n; ix++)
                for (int iz = 0; iz < n; iz++)
                {
                    float cx = -Setengah + (ix + 0.5f) * Sel, cz = -Setengah + (iz + 0.5f) * Sel;
                    if (WorldTerrainGenerator.SampleHeight(cx, cz) < 0.7f) continue;
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
                            if (h < 0.7f || (wl.HasValue && wl.Value > h)) continue;
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

        int Hari() { if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>(); return _jam != null ? _jam.Hari : 1; }
        bool Tersedia(int i) { int h; return !_diambil.TryGetValue(i, out h) || Hari() - h >= HariTumbuh; }

        public void Ambil(int i)
        {
            _diambil[i] = Hari();
            for (int k = 0; k < pool.Length; k++) if (pool[k] != null && pool[k].Indeks == i) { pool[k].Indeks = -1; pool[k].gameObject.SetActive(false); }
        }

        void Update()
        {
            _t += Time.deltaTime;
            if (_t < Interval || _pos == null || _awalSel == null) return;
            _t = 0f;
            var cam = Camera.main;
            if (cam == null || WorldTerrainGenerator.DiBawahTanah(cam.transform.position.y)) return;
            Vector3 c = cam.transform.position;
            float r2 = RadiusPool * RadiusPool;
            for (int k = 0; k < pool.Length; k++)
            {
                var p = pool[k];
                if (p == null || p.Indeks < 0) continue;
                if ((_pos[p.Indeks] - c).sqrMagnitude > r2 * 1.4f) { p.Indeks = -1; p.gameObject.SetActive(false); }
            }
            int n = 0;
            for (int x = KoordinatSel(c.x - RadiusPool); x <= KoordinatSel(c.x + RadiusPool) && n < _calon.Length; x++)
                for (int z = KoordinatSel(c.z - RadiusPool); z <= KoordinatSel(c.z + RadiusPool) && n < _calon.Length; z++)
                {
                    int sel = x * SisiSel + z;
                    for (int k = _awalSel[sel]; k < _awalSel[sel + 1] && n < _calon.Length; k++)
                    {
                        int i = _urutanSel[k];
                        if ((_pos[i] - c).sqrMagnitude <= r2) _calon[n++] = i;
                    }
                }
            for (int q = 0; q < n; q++)
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
        ForagingExtraNode Kosong() { for (int k = 0; k < pool.Length; k++) if (pool[k] != null && pool[k].Indeks < 0) return pool[k]; return null; }
    }
}
