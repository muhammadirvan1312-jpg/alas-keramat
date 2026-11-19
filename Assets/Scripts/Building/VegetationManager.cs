using AlasKeramat.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.Building
{
    // Menggambar vegetasi dengan GPU instancing (H10: tanpa GameObject per pohon jauh).
    public class VegetationManager : MonoBehaviour, ISaveable
    {
        public const int MaksBatch = 1023;
        public const int MaksRender = 511;
        [SerializeField] VegetationData data;
        [SerializeField] float pengaliJarak = 1f;

        readonly Plane[] _frustum = new Plane[6];
        readonly Matrix4x4[] _dekat = new Matrix4x4[MaksBatch], _jauh = new Matrix4x4[MaksBatch];
        public float PengaliDetail = 1f;
        public int InstanceTerlihat { get; private set; }
        public int SegitigaTerlihat { get; private set; }
        RenderParams[] _rpKayu, _rpDaun;
        bool _siap;
        Matrix4x4[][] _mat;                                   // salinan runtime (aset tidak diubah saat pohon ditebang)
        Bounds[][] _batasInstance;
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
            _batasInstance = new Bounds[data.sel.Length][];
            _letak.Clear();
            for (int c = 0; c < data.sel.Length; c++)
            {
                var sel = data.sel[c];
                _mat[c] = (Matrix4x4[])sel.matriks.Clone();
                var jenis = data.jenis[sel.jenis];
                Bounds lokal = jenis.lod0 != null ? jenis.lod0.bounds : new Bounds(Vector3.zero, Vector3.one * 20f);
                if (jenis.lod1 != null) lokal.Encapsulate(jenis.lod1.bounds);
                _batasInstance[c] = new Bounds[sel.matriks.Length];
                for (int k = 0; k < sel.matriks.Length; k++) _batasInstance[c][k] = BatasInstance(lokal, sel.matriks[k]);
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
            InstanceTerlihat = 0; SegitigaTerlihat = 0;
            var urp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
            float bayangan = urp != null ? urp.shadowDistance : QualitySettings.shadowDistance;
            for (int i = 0; i < data.sel.Length; i++)
            {
                var s = data.sel[i];
                var j = data.jenis[s.jenis];
                float jarak = Vector3.Distance(posKam, s.pusat) - s.radius;
                if (jarak > j.jarakMaks * pengaliJarak) continue;
                if (!GeometryUtility.TestPlanesAABB(_frustum, new Bounds(s.pusat, Vector3.one * (s.radius * 2f + 30f)))) continue;
                float batas = Mathf.Min(j.jarakMaks * pengaliJarak, cam.farClipPlane);
                int dekat = 0, jauh = 0;
                var mt = _mat[i];
                for (int k = 0; k < mt.Length; k++)
                {
                    var matrix = mt[k];
                    if (matrix.GetColumn(0).sqrMagnitude < 0.00001f) continue;
                    float d = Vector3.Distance(posKam, matrix.GetColumn(3));
                    if (d > batas) continue;
                    // Pertahankan caster dekat di luar pandangan supaya bayangan di tepi layar tidak hilang.
                    if (!GeometryUtility.TestPlanesAABB(_frustum, _batasInstance[i][k]) && (!j.bayangan || d > bayangan + _batasInstance[i][k].extents.magnitude)) continue;
                    if (PakaiLodJauh(d, j.jarakLod1 * PengaliDetail) && j.lod1 != null) _jauh[jauh++] = matrix;
                    else _dekat[dekat++] = matrix;
                }
                GambarKelompok(cam, j, s, j.lod0, _dekat, dekat);
                GambarKelompok(cam, j, s, j.lod1, _jauh, jauh);
            }
        }

        public static bool PakaiLodJauh(float jarak, float ambang) { return jarak > ambang; }
        public static Bounds BatasInstance(Bounds lokal, Matrix4x4 m)
        {
            Vector3 e = lokal.extents;
            var ukuran = new Vector3(Mathf.Abs(m.m00) * e.x + Mathf.Abs(m.m01) * e.y + Mathf.Abs(m.m02) * e.z,
                Mathf.Abs(m.m10) * e.x + Mathf.Abs(m.m11) * e.y + Mathf.Abs(m.m12) * e.z,
                Mathf.Abs(m.m20) * e.x + Mathf.Abs(m.m21) * e.y + Mathf.Abs(m.m22) * e.z);
            return new Bounds(m.MultiplyPoint3x4(lokal.center), ukuran * 2f + Vector3.one * 2f);
        }
        void GambarKelompok(Camera cam, VegetationData.JenisVeg jenis, VegetationData.Sel sel, Mesh mesh, Matrix4x4[] matriks, int jumlah)
        {
            if (mesh == null || jumlah == 0) return;
            var rpK = _rpKayu[sel.jenis]; rpK.camera = cam;
            var rpD = _rpDaun[sel.jenis]; rpD.camera = cam;
            var bounds = new Bounds(sel.pusat, Vector3.one * (sel.radius * 2f + 30f));
            rpK.worldBounds = bounds; rpD.worldBounds = bounds;
            for(int awal=0;awal<jumlah;awal+=MaksRender)
            {
                int n=Mathf.Min(MaksRender,jumlah-awal);
                if (jenis.kayu != null && mesh.GetSubMesh(0).indexCount > 0) Graphics.RenderMeshInstanced(rpK, mesh, 0, matriks, n, awal);
                if (jenis.daun != null && mesh.subMeshCount > 1 && mesh.GetSubMesh(1).indexCount > 0) Graphics.RenderMeshInstanced(rpD, mesh, 1, matriks, n, awal);
            }
            InstanceTerlihat += jumlah;
            for (int sub = 0; sub < mesh.subMeshCount; sub++) SegitigaTerlihat += (int)(mesh.GetIndexCount(sub) / 3) * jumlah;
        }
    }
}
