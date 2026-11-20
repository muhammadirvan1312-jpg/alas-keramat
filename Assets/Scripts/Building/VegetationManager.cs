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
        RenderParams[] _rpKayu, _rpDaun, _rpKayuNoShadow, _rpDaunNoShadow;
        Camera _cam;
        bool _siap;
        Matrix4x4[][] _mat;                                   // salinan runtime (aset tidak diubah saat pohon ditebang)
        Vector3[][] _posInst;
        float[][] _extInst;
        bool[][] _kosong;
        Bounds[][] _batasInstance;
        Bounds[] _selBounds;
        bool[] _bayanganJenis;
        int[] _sub0Lod0, _sub1Lod0, _sub0Lod1, _sub1Lod1;
        float[] _jarakMaksJenis, _lod1Jenis;
        const int SisiGrid = 16;
        const float UkuranGrid = 125f, SetengahDunia = 1000f;
        int[][] _gridSel;
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
            _rpKayuNoShadow = new RenderParams[n]; _rpDaunNoShadow = new RenderParams[n];
            _bayanganJenis = new bool[n];
            _jarakMaksJenis = new float[n];
            _lod1Jenis = new float[n];
            _sub0Lod0 = new int[n]; _sub1Lod0 = new int[n];
            _sub0Lod1 = new int[n]; _sub1Lod1 = new int[n];
            for (int i = 0; i < n; i++)
            {
                var j = data.jenis[i];
                _bayanganJenis[i] = j.bayangan;
                _jarakMaksJenis[i] = j.jarakMaks;
                _lod1Jenis[i] = j.jarakLod1;
                if (j.lod0 != null)
                {
                    _sub0Lod0[i] = (int)j.lod0.GetIndexCount(0);
                    _sub1Lod0[i] = j.lod0.subMeshCount > 1 ? (int)j.lod0.GetIndexCount(1) : 0;
                }
                if (j.lod1 != null)
                {
                    _sub0Lod1[i] = (int)j.lod1.GetIndexCount(0);
                    _sub1Lod1[i] = j.lod1.subMeshCount > 1 ? (int)j.lod1.GetIndexCount(1) : 0;
                }
                _rpKayu[i] = new RenderParams(j.kayu) { shadowCastingMode = j.bayangan ? ShadowCastingMode.On : ShadowCastingMode.Off, receiveShadows = true, layer = GameLayers.Foliage, lightProbeUsage = LightProbeUsage.Off };
                _rpDaun[i] = new RenderParams(j.daun) { shadowCastingMode = j.bayangan ? ShadowCastingMode.On : ShadowCastingMode.Off, receiveShadows = true, layer = GameLayers.Foliage, lightProbeUsage = LightProbeUsage.Off };
                _rpKayuNoShadow[i] = new RenderParams(j.kayu) { shadowCastingMode = ShadowCastingMode.Off, receiveShadows = true, layer = GameLayers.Foliage, lightProbeUsage = LightProbeUsage.Off };
                _rpDaunNoShadow[i] = new RenderParams(j.daun) { shadowCastingMode = ShadowCastingMode.Off, receiveShadows = true, layer = GameLayers.Foliage, lightProbeUsage = LightProbeUsage.Off };
            }
            _mat = new Matrix4x4[data.sel.Length][];
            _posInst = new Vector3[data.sel.Length][];
            _extInst = new float[data.sel.Length][];
            _kosong = new bool[data.sel.Length][];
            _batasInstance = new Bounds[data.sel.Length][];
            _selBounds = new Bounds[data.sel.Length];
            var ember = new System.Collections.Generic.List<int>[SisiGrid * SisiGrid];
            _letak.Clear();
            for (int c = 0; c < data.sel.Length; c++)
            {
                var sel = data.sel[c];
                _mat[c] = (Matrix4x4[])sel.matriks.Clone();
                _posInst[c] = new Vector3[sel.matriks.Length];
                _extInst[c] = new float[sel.matriks.Length];
                _kosong[c] = new bool[sel.matriks.Length];
                var jenis = data.jenis[sel.jenis];
                Bounds lokal = jenis.lod0 != null ? jenis.lod0.bounds : new Bounds(Vector3.zero, Vector3.one * 20f);
                if (jenis.lod1 != null) lokal.Encapsulate(jenis.lod1.bounds);
                _batasInstance[c] = new Bounds[sel.matriks.Length];
                for (int k = 0; k < sel.matriks.Length; k++)
                {
                    _posInst[c][k] = sel.matriks[k].GetColumn(3);
                    _kosong[c][k] = sel.matriks[k].GetColumn(0).sqrMagnitude < 0.00001f;
                    var bi = BatasInstance(lokal, sel.matriks[k]);
                    _batasInstance[c][k] = bi;
                    _extInst[c][k] = bi.extents.magnitude;
                }
                _selBounds[c] = new Bounds(sel.pusat, Vector3.one * (sel.radius * 2f + 30f));
                int gx = Mathf.Clamp(Mathf.FloorToInt((sel.pusat.x + SetengahDunia) / UkuranGrid), 0, SisiGrid - 1);
                int gz = Mathf.Clamp(Mathf.FloorToInt((sel.pusat.z + SetengahDunia) / UkuranGrid), 0, SisiGrid - 1);
                int g = gx * SisiGrid + gz;
                if (ember[g] == null) ember[g] = new System.Collections.Generic.List<int>();
                ember[g].Add(c);
                if (sel.idGlobal != null) for (int k = 0; k < sel.idGlobal.Length; k++) _letak[sel.idGlobal[k]] = new Vector2Int(c, k);
            }
            _gridSel = new int[ember.Length][];
            for (int g = 0; g < ember.Length; g++) _gridSel[g] = ember[g] != null ? ember[g].ToArray() : System.Array.Empty<int>();
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
            if (_kosong != null) _kosong[l.x][l.y] = true;
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
            if (_kosong != null) _kosong[l.x][l.y] = false;
        }

        public VegetationData.JenisVeg Jenis(int jenisVeg) { return data != null && jenisVeg >= 0 && jenisVeg < data.jenis.Length ? data.jenis[jenisVeg] : null; }

        void Update()
        {
            if (_cam == null || !_cam.isActiveAndEnabled) _cam = Camera.main;
            if (_cam != null) Gambar(_cam);
        }

        // Juga dipanggil Foto Kualitas sebelum Camera.Render() di Editor.
        public void Gambar(Camera cam)
        {
            if (cam == null || data == null || data.sel == null) return;
            Vector3 posKam = cam.transform.position;
            if (WorldTerrainGenerator.DiBawahTanah(posKam.y)) return;
            if (!_siap) Siapkan();
            if (!_siap) return;
            GeometryUtility.CalculateFrustumPlanes(cam, _frustum);
            InstanceTerlihat = 0; SegitigaTerlihat = 0;
            var urp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
            float bayangan = urp != null ? urp.shadowDistance : QualitySettings.shadowDistance;
            float maksJarak = 0f;
            for (int n = 0; n < _jarakMaksJenis.Length; n++) if (_jarakMaksJenis[n] > maksJarak) maksJarak = _jarakMaksJenis[n];
            float jangkau = Mathf.Min(maksJarak * pengaliJarak, cam.farClipPlane) + 140f;
            int gx0 = Mathf.Clamp(Mathf.FloorToInt((posKam.x - jangkau + SetengahDunia) / UkuranGrid), 0, SisiGrid - 1);
            int gx1 = Mathf.Clamp(Mathf.FloorToInt((posKam.x + jangkau + SetengahDunia) / UkuranGrid), 0, SisiGrid - 1);
            int gz0 = Mathf.Clamp(Mathf.FloorToInt((posKam.z - jangkau + SetengahDunia) / UkuranGrid), 0, SisiGrid - 1);
            int gz1 = Mathf.Clamp(Mathf.FloorToInt((posKam.z + jangkau + SetengahDunia) / UkuranGrid), 0, SisiGrid - 1);

            for (int gx = gx0; gx <= gx1; gx++) for (int gz = gz0; gz <= gz1; gz++)
            {
                var ember = _gridSel[gx * SisiGrid + gz];
                for (int eIdx = 0; eIdx < ember.Length; eIdx++)
                {
                    int i = ember[eIdx];
                    var s = data.sel[i];
                    var j = data.jenis[s.jenis];
                    float batasJarak = Mathf.Min(j.jarakMaks * pengaliJarak, cam.farClipPlane);
                    float maxCel = batasJarak + s.radius;
                    Vector3 dc = s.pusat - posKam;
                    if (dc.sqrMagnitude > maxCel * maxCel) continue;
                    if (!GeometryUtility.TestPlanesAABB(_frustum, _selBounds[i])) continue;

                    float batasSq = batasJarak * batasJarak;
                    float lodDist = j.jarakLod1 * PengaliDetail;
                    float lodSq = lodDist * lodDist;
                    bool adaBayang = _bayanganJenis[s.jenis] && bayangan > 0.5f;
                    int dekat = 0, jauh = 0;
                    var mt = _mat[i];
                    var ps = _posInst[i];
                    var ex = _extInst[i];
                    var ks = _kosong[i];
                    var bs = _batasInstance[i];
                    for (int k = 0; k < mt.Length; k++)
                    {
                        if (ks[k]) continue;
                        Vector3 dVec = ps[k] - posKam;
                        float dSq = dVec.x * dVec.x + dVec.y * dVec.y + dVec.z * dVec.z;
                        if (dSq > batasSq) continue;
                        if (!GeometryUtility.TestPlanesAABB(_frustum, bs[k]))
                        {
                            float bLim = bayangan + ex[k];
                            if (!adaBayang || dSq > bLim * bLim) continue;
                        }
                        if (dSq > lodSq && j.lod1 != null) _jauh[jauh++] = mt[k];
                        else _dekat[dekat++] = mt[k];
                    }
                    if (dekat > 0) GambarKelompok(cam, j, s, j.lod0, _sub0Lod0[s.jenis], _sub1Lod0[s.jenis], _dekat, dekat, adaBayang);
                    if (jauh > 0) GambarKelompok(cam, j, s, j.lod1, _sub0Lod1[s.jenis], _sub1Lod1[s.jenis], _jauh, jauh, false);
                }
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
        void GambarKelompok(Camera cam, VegetationData.JenisVeg jenis, VegetationData.Sel sel, Mesh mesh, int sub0Cnt, int sub1Cnt, Matrix4x4[] matriks, int jumlah, bool pakaiBayangan)
        {
            if (mesh == null || jumlah == 0) return;
            var rpK = (pakaiBayangan ? _rpKayu : _rpKayuNoShadow)[sel.jenis]; rpK.camera = cam;
            var rpD = (pakaiBayangan ? _rpDaun : _rpDaunNoShadow)[sel.jenis]; rpD.camera = cam;
            var bounds = _selBounds[0].size == Vector3.zero ? new Bounds(sel.pusat, Vector3.one * (sel.radius * 2f + 30f)) : new Bounds(sel.pusat, Vector3.one * (sel.radius * 2f + 30f));
            rpK.worldBounds = bounds; rpD.worldBounds = bounds;
            for(int awal=0;awal<jumlah;awal+=MaksRender)
            {
                int n=Mathf.Min(MaksRender,jumlah-awal);
                if (jenis.kayu != null && sub0Cnt > 0) Graphics.RenderMeshInstanced(rpK, mesh, 0, matriks, n, awal);
                if (jenis.daun != null && sub1Cnt > 0) Graphics.RenderMeshInstanced(rpD, mesh, 1, matriks, n, awal);
            }
            InstanceTerlihat += jumlah;
            SegitigaTerlihat += ((sub0Cnt + sub1Cnt) / 3) * jumlah;
        }
    }
}
