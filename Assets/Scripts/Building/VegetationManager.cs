using AlasKeramat.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.Building
{
    // Menggambar vegetasi dengan GPU instancing (H10: tanpa GameObject per pohon jauh).
    public class VegetationManager : MonoBehaviour
    {
        public const int MaksBatch = 1023;
        [SerializeField] VegetationData data;
        [SerializeField] float pengaliJarak = 1f;

        readonly Plane[] _frustum = new Plane[6];
        RenderParams[] _rpKayu, _rpDaun;
        bool _siap;

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
                _rpKayu[i] = new RenderParams(j.kayu) { shadowCastingMode = j.bayangan ? ShadowCastingMode.On : ShadowCastingMode.Off, receiveShadows = true, layer = GameLayers.Foliage };
                _rpDaun[i] = new RenderParams(j.daun) { shadowCastingMode = j.bayangan ? ShadowCastingMode.On : ShadowCastingMode.Off, receiveShadows = true, layer = GameLayers.Foliage };
            }
            _siap = true;
        }

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
                int n = Mathf.Min(s.matriks.Length, MaksBatch);
                if (j.kayu != null && m.GetSubMesh(0).indexCount > 0) Graphics.RenderMeshInstanced(rpK, m, 0, s.matriks, n);
                if (j.daun != null && m.subMeshCount > 1 && m.GetSubMesh(1).indexCount > 0) Graphics.RenderMeshInstanced(rpD, m, 1, s.matriks, n);
            }
        }
    }
}
