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
        public float Kepadatan = 1f;
        readonly Matrix4x4[] _tampak = new Matrix4x4[1023];
        readonly Plane[] _bidang=new Plane[6];
        void Awake() { GameServices.Register(this); }
        void LateUpdate() { var cam=Camera.main;if(cam!=null)Gambar(cam); }
        public void Gambar(Camera cam)
        {
            if(cam==null || WorldTerrainGenerator.DiBawahTanah(cam.transform.position.y))return;
            GeometryUtility.CalculateFrustumPlanes(cam,_bidang);
            foreach(var k in kelompok)
            {
                if(k.mesh==null || k.material==null || k.matriks==null || k.matriks.Length==0)continue;
                if(k.bounds.SqrDistance(cam.transform.position)>JarakGambar*JarakGambar || !GeometryUtility.TestPlanesAABB(_bidang,k.bounds))continue;
                int jumlah=0, batas=Mathf.Min(k.matriks.Length,Mathf.CeilToInt(k.matriks.Length*Mathf.Clamp01(Kepadatan)));
                float radius=JarakGambar*Mathf.Lerp(0.55f,1f,Mathf.Clamp01(Kepadatan));
                for(int i=0;i<batas && jumlah<_tampak.Length;i++)
                    if(((Vector3)k.matriks[i].GetColumn(3)-cam.transform.position).sqrMagnitude<=radius*radius)_tampak[jumlah++]=k.matriks[i];
                if(jumlah==0)continue;
                var rp=new RenderParams(k.material) { camera=cam,layer=GameLayers.Foliage,worldBounds=k.bounds,shadowCastingMode=ShadowCastingMode.Off,receiveShadows=true,lightProbeUsage=LightProbeUsage.Off };
                Graphics.RenderMeshInstanced(rp,k.mesh,0,_tampak,jumlah);
            }
        }
    }
}
