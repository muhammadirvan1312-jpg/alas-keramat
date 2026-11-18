using UnityEngine;
using UnityEngine.Rendering;
namespace AlasKeramat.Core
{
    // Dekorasi lantai tanpa collider, item, atau perubahan ID pohon tersimpan.
    public class SerasahHutan : MonoBehaviour
    {
        [System.Serializable] public class KelompokSerasah
        { public Mesh mesh;public Material material;public Bounds bounds;public Matrix4x4[] matriks; }
        public const float JarakGambar=65f;
        public KelompokSerasah[] kelompok=new KelompokSerasah[0];
        readonly Plane[] _bidang=new Plane[6];
        void LateUpdate() { var cam=Camera.main;if(cam!=null)Gambar(cam); }
        public void Gambar(Camera cam)
        {
            if(cam==null || WorldTerrainGenerator.DiBawahTanah(cam.transform.position.y))return;
            GeometryUtility.CalculateFrustumPlanes(cam,_bidang);
            foreach(var k in kelompok)
            {
                if(k.mesh==null || k.material==null || k.matriks==null || k.matriks.Length==0)continue;
                if(k.bounds.SqrDistance(cam.transform.position)>JarakGambar*JarakGambar || !GeometryUtility.TestPlanesAABB(_bidang,k.bounds))continue;
                var rp=new RenderParams(k.material) { camera=cam,layer=GameLayers.Foliage,worldBounds=k.bounds,shadowCastingMode=ShadowCastingMode.Off,receiveShadows=true,lightProbeUsage=LightProbeUsage.Off };
                Graphics.RenderMeshInstanced(rp,k.mesh,0,k.matriks,k.matriks.Length);
            }
        }
    }
}
