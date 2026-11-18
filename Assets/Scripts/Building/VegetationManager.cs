using AlasKeramat.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.Building
{
    // Menggambar vegetasi dengan GPU instancing (H10: tanpa GameObject per pohon jauh).
    public class VegetationManager : MonoBehaviour, ISaveable
    {
        public const int MaksBatch = 1023;
        [SerializeField] VegetationData data;
        [SerializeField] float pengaliJarak = 1f;

        readonly Plane[] _frustum = new Plane[6];
        RenderParams[] _rpKayu, _rpDaun;
        bool _siap;
        Matrix4x4[][] _mat;                                   // salinan runtime (aset tidak diubah saat pohon ditebang)
        readonly System.Collections.Generic.Dictionary<int, Vector2Int> _letak = new System.Collections.Generic.Dictionary<int, Vector2Int>();
        readonly System.Collections.Generic.HashSet<int> _hilang = new System.Collections.Generic.HashSet<int>();
        ProcPohon.Jenis[] _jenisPohon;

        public struct InfoPohon { public int id, jenisVeg; public ProcPohon.Jenis jenis; public Vector3 posisi; public Quaternion rotasi; public float skala; }

        public VegetationData Data { get { return data; } }
        public float PengaliJarak { get { return pengaliJarak; } set { pengaliJarak = Mathf.Clamp(value, 0.1f, 2f); } }

        public void Pasang(VegetationData d) { data = d; _siap = false; }

        void Awake() { GameServices.Register(this); }

        void Siapkan()
        {
            if (data == null) return;
            int n = data.jenis.Length;
            _rpKayu = new RenderParams[n]; _rpDaun = new RenderParams[n];
            for (int i = 0; i < n; i++)
            {
                var j = data.jenis[i];
                _rpKayu[i] = new RenderParams(j.kayu) { shadowCastingMode = j.bayangan ? ShadowCastingMode.On : ShadowCastingMode.Off, receiveShadows = true, layer = GameLayers.Foliage, lightProbeUsage = LightProbeUsage.Off };
                _rpDaun[i] = new RenderParams(j.daun) { shadowCastingMode = j.bayangan ? ShadowCastingMode.On : ShadowCastingMode.Off, receiveShadows = true, layer = GameLayers.Foliage, lightProbeUsage = LightProbeUsage.Off };
            }
            _mat = new Matrix4x4[data.sel.Length][];
            _letak.Clear();
            for (int c = 0; c < data.sel.Length; c++)
            {
                var sel = data.sel[c];
                _mat[c] = (Matrix4x4[])sel.matriks.Clone();
                if (sel.idGlobal != null) for (int k = 0; k < sel.idGlobal.Length; k++) _letak[sel.idGlobal[k]] = new Vector2Int(c, k);
            }
            _jenisPohon = new ProcPohon.Jenis[n];
            for (int i = 0; i < n; i++)
            {
                string nm = data.jenis[i].nama ?? "";
                int u = nm.IndexOf('_');
                ProcPohon.Jenis jp;
                _jenisPohon[i] = System.Enum.TryParse(u > 0 ? nm.Substring(0, u) : nm, out jp) ? jp : ProcPohon.Jenis.Semak;
            }
            _siap = true;
        }

        // Cari pohon dalam radius r dari p (tanpa alokasi; buffer dari pemanggil). Mengembalikan jumlah yang terisi.
        public int Cari(Vector3 p, float r, InfoPohon[] buffer)
        {
            if (!_siap) Siapkan();
            if (!_siap || buffer == null) return 0;
            int n = 0;
            for (int c = 0; c < data.sel.Length && n < buffer.Length; c++)
            {
                var sel = data.sel[c];
                if ((sel.pusat - p).sqrMagnitude > (sel.radius + r) * (sel.radius + r)) continue;
                var m = _mat[c];
                for (int k = 0; k < m.Length && n < buffer.Length; k++)
                {
                    Vector3 q = m[k].GetColumn(3);
                    if ((q - p).sqrMagnitude > r * r) continue;
                    int id = sel.idGlobal != null && k < sel.idGlobal.Length ? sel.idGlobal[k] : -1;
                    if (id < 0 || _hilang.Contains(id)) continue;
                    buffer[n++] = new InfoPohon { id = id, jenisVeg = sel.jenis, jenis = _jenisPohon[sel.jenis], posisi = q, rotasi = m[k].rotation, skala = m[k].lossyScale.y };
                }
            }
            return n;
        }

        public bool Tersembunyi(int id) { return _hilang.Contains(id); }

        public void Sembunyikan(int id)
        {
            if (!_siap) Siapkan();
            Vector2Int l;
            if (!_siap || !_letak.TryGetValue(id, out l) || !_hilang.Add(id)) return;
            var m = _mat[l.x][l.y];
            _mat[l.x][l.y] = Matrix4x4.TRS(m.GetColumn(3), m.rotation, Vector3.zero);
        }

        [System.Serializable] class PohonSimpanT6 { public int[] hilang; }
        public string SaveKey { get { return "pohon"; } }
        public string CaptureJson() { var ids = new int[_hilang.Count]; _hilang.CopyTo(ids); System.Array.Sort(ids); return JsonUtility.ToJson(new PohonSimpanT6 { hilang = ids }); }
        public void RestoreJson(string json)
        {
            var d = JsonUtility.FromJson<PohonSimpanT6>(json); if (d == null || d.hilang == null) return;
            if (!_siap) Siapkan();
            var lama = new int[_hilang.Count]; _hilang.CopyTo(lama);
            foreach (int id in lama) Tampilkan(id);
            foreach (int id in d.hilang) Sembunyikan(id);
        }

        public void Tampilkan(int id)
        {
            Vector2Int l;
            if (!_siap || !_letak.TryGetValue(id, out l) || !_hilang.Remove(id)) return;
            _mat[l.x][l.y] = data.sel[l.x].matriks[l.y];
        }

        public VegetationData.JenisVeg Jenis(int jenisVeg) { return data != null && jenisVeg >= 0 && jenisVeg < data.jenis.Length ? data.jenis[jenisVeg] : null; }

        void Update()
        {
            var cam = Camera.main;
            if (cam != null) Gambar(cam);
        }

        // Juga dipanggil Foto Kualitas sebelum Camera.Render() di Editor.
        public void Gambar(Camera cam)
        {
            if (data == null || data.sel == null) return;
            if (!_siap) Siapkan();
            if (!_siap) return;
            GeometryUtility.CalculateFrustumPlanes(cam, _frustum);
            Vector3 posKam = cam.transform.position;
            for (int i = 0; i < data.sel.Length; i++)
            {
                var s = data.sel[i];
                var j = data.jenis[s.jenis];
                float jarak = Vector3.Distance(posKam, s.pusat) - s.radius;
                if (jarak > j.jarakMaks * pengaliJarak) continue;
                if (!GeometryUtility.TestPlanesAABB(_frustum, new Bounds(s.pusat, Vector3.one * (s.radius * 2f + 30f)))) continue;
                Mesh m = jarak > j.jarakLod1 * pengaliJarak && j.lod1 != null ? j.lod1 : j.lod0;
                if (m == null) continue;
                var rpK = _rpKayu[s.jenis]; rpK.camera = cam;
                var rpD = _rpDaun[s.jenis]; rpD.camera = cam;
                var mt = _mat[i];
                int n = Mathf.Min(mt.Length, MaksBatch);
                if (j.kayu != null && m.GetSubMesh(0).indexCount > 0) Graphics.RenderMeshInstanced(rpK, m, 0, mt, n);
                if (j.daun != null && m.subMeshCount > 1 && m.GetSubMesh(1).indexCount > 0) Graphics.RenderMeshInstanced(rpD, m, 1, mt, n);
            }
        }
    }
}
