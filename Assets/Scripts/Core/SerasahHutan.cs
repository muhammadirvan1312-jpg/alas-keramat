using UnityEngine;
using UnityEngine.Rendering;
namespace AlasKeramat.Core
{
    // Dekorasi lantai tanpa collider, item, atau perubahan ID pohon tersimpan.
    public class SerasahHutan : MonoBehaviour
    {
        [System.Serializable] public class KelompokSerasah
        {
            public Mesh mesh;public Material material;public Bounds bounds;public Matrix4x4[] matriks;
            [System.NonSerialized] public Bounds[] batasInstance;
        }
        public const float JarakGambar=48f;
        public KelompokSerasah[] kelompok=new KelompokSerasah[0];
        public float Kepadatan = 1f;
        readonly Matrix4x4[] _tampak = new Matrix4x4[1023];
        readonly Plane[] _bidang=new Plane[6];
        RenderParams[] _rp;
        Camera _cam;
        void Awake() { GameServices.Register(this); }
        void SiapkanRp()
        {
            _rp=new RenderParams[kelompok.Length];
            for(int i=0;i<kelompok.Length;i++)
            {
                var k=kelompok[i];
                if(k.material==null)continue;
                _rp[i]=new RenderParams(k.material) { layer=GameLayers.Foliage,worldBounds=k.bounds,shadowCastingMode=ShadowCastingMode.Off,receiveShadows=true,lightProbeUsage=LightProbeUsage.Off };
            }
        }
        void LateUpdate() { if(_cam==null || !_cam.isActiveAndEnabled)_cam=Camera.main; if(_cam!=null)Gambar(_cam); }
        public void Gambar(Camera cam)
        {
            if(cam==null || WorldTerrainGenerator.DiBawahTanah(cam.transform.position.y))return;
            if(_rp==null || _rp.Length!=kelompok.Length)SiapkanRp();
            GeometryUtility.CalculateFrustumPlanes(cam,_bidang);
            Vector3 cp=cam.transform.position;
            float kep=Mathf.Clamp01(Kepadatan), radius=JarakGambar*Mathf.Lerp(0.55f,1f,kep), radiusSq=radius*radius;
            for(int idx=0;idx<kelompok.Length;idx++)
            {
                var k=kelompok[idx];
                if(k.mesh==null || k.material==null || k.matriks==null || k.matriks.Length==0)continue;
                if(k.bounds.SqrDistance(cp)>JarakGambar*JarakGambar || !GeometryUtility.TestPlanesAABB(_bidang,k.bounds))continue;
                if(k.batasInstance==null || k.batasInstance.Length!=k.matriks.Length)
                {
                    k.batasInstance=new Bounds[k.matriks.Length];
                    for(int i=0;i<k.matriks.Length;i++)k.batasInstance[i]=AlasKeramat.Building.VegetationManager.BatasInstance(k.mesh.bounds,k.matriks[i]);
                }
                int jumlah=0, batas=Mathf.Min(k.matriks.Length,Mathf.CeilToInt(k.matriks.Length*kep));
                for(int i=0;i<batas && jumlah<_tampak.Length;i++)
                    if(((Vector3)k.matriks[i].GetColumn(3)-cp).sqrMagnitude<=radiusSq &&
                        GeometryUtility.TestPlanesAABB(_bidang,k.batasInstance[i]))_tampak[jumlah++]=k.matriks[i];
                if(jumlah==0)continue;
                var rp=_rp[idx]; rp.camera=cam;
                for(int awal=0;awal<jumlah;awal+=AlasKeramat.Building.VegetationManager.MaksRender)
                    Graphics.RenderMeshInstanced(rp,k.mesh,0,_tampak,Mathf.Min(AlasKeramat.Building.VegetationManager.MaksRender,jumlah-awal),awal);
            }
        }
    }
}
