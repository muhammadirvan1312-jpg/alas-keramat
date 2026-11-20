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
        public const float JarakGambar=56f;
        public KelompokSerasah[] kelompok=new KelompokSerasah[0];
        public float Kepadatan = 1f;
        public bool BayanganPadat = true;
        readonly Matrix4x4[] _tampak = new Matrix4x4[1023];
        readonly Plane[] _bidang=new Plane[6];
        RenderParams[] _rp, _rpNoShadow;
        Vector3[][] _pos;
        int[] _sub;
        const float UkuranGrid = 32f, MinGrid = -640f, MaxGrid = 448f;
        const int SisiGrid = 34;
        int[][] _grid;
        Camera _cam;
        void Awake() { GameServices.Register(this); }
        void SiapkanRp()
        {
            int n = kelompok.Length;
            _rp = new RenderParams[n];
            _rpNoShadow = new RenderParams[n];
            _pos = new Vector3[n][];
            _sub = new int[n];
            var ember = new System.Collections.Generic.List<int>[SisiGrid * SisiGrid];
            for(int i=0;i<n;i++)
            {
                var k=kelompok[i];
                if(k.mesh!=null) _sub[i] = k.mesh.GetIndexCount(0)>0 ? 0 : (k.mesh.subMeshCount>1 ? 1 : 0);
                if(k.matriks!=null && k.matriks.Length>0)
                {
                    if(k.batasInstance==null || k.batasInstance.Length!=k.matriks.Length)
                        k.batasInstance=new Bounds[k.matriks.Length];
                    _pos[i]=new Vector3[k.matriks.Length];
                    for(int m=0;m<k.matriks.Length;m++)
                    {
                        _pos[i][m]=(Vector3)k.matriks[m].GetColumn(3);
                        if(k.mesh!=null) k.batasInstance[m]=AlasKeramat.Building.VegetationManager.BatasInstance(k.mesh.bounds,k.matriks[m]);
                    }
                    int gx=Mathf.Clamp(Mathf.FloorToInt((k.bounds.center.x-MinGrid)/UkuranGrid),0,SisiGrid-1);
                    int gz=Mathf.Clamp(Mathf.FloorToInt((k.bounds.center.z-MinGrid)/UkuranGrid),0,SisiGrid-1);
                    int gi=gx*SisiGrid+gz;
                    if(ember[gi]==null)ember[gi]=new System.Collections.Generic.List<int>();
                    ember[gi].Add(i);
                }
                if(k.material==null)continue;
                bool padat=k.material.shader!=null && k.material.shader.name!="AlasKeramat/Daun";
                _rp[i]=new RenderParams(k.material) { layer=GameLayers.Foliage,worldBounds=k.bounds,shadowCastingMode=padat?ShadowCastingMode.On:ShadowCastingMode.Off,receiveShadows=true,lightProbeUsage=LightProbeUsage.Off };
                _rpNoShadow[i]=new RenderParams(k.material) { layer=GameLayers.Foliage,worldBounds=k.bounds,shadowCastingMode=ShadowCastingMode.Off,receiveShadows=true,lightProbeUsage=LightProbeUsage.Off };
            }
            _grid=new int[ember.Length][];
            for(int i=0;i<ember.Length;i++) _grid[i]=ember[i]!=null?ember[i].ToArray():System.Array.Empty<int>();
        }
        void LateUpdate() { if(_cam==null || !_cam.isActiveAndEnabled)_cam=Camera.main; if(_cam!=null)Gambar(_cam); }
        public void Gambar(Camera cam)
        {
            if(cam==null || WorldTerrainGenerator.DiBawahTanah(cam.transform.position.y))return;
            if(_rp==null || _rp.Length!=kelompok.Length)SiapkanRp();
            GeometryUtility.CalculateFrustumPlanes(cam,_bidang);
            Vector3 cp=cam.transform.position;
            float kep=Mathf.Clamp01(Kepadatan), radius=JarakGambar*Mathf.Lerp(0.55f,1f,kep), radiusSq=radius*radius;
            float maxDist=JarakGambar+40f, maxSq=JarakGambar*JarakGambar;
            int gx0=Mathf.Clamp(Mathf.FloorToInt((cp.x-maxDist-MinGrid)/UkuranGrid),0,SisiGrid-1);
            int gx1=Mathf.Clamp(Mathf.FloorToInt((cp.x+maxDist-MinGrid)/UkuranGrid),0,SisiGrid-1);
            int gz0=Mathf.Clamp(Mathf.FloorToInt((cp.z-maxDist-MinGrid)/UkuranGrid),0,SisiGrid-1);
            int gz1=Mathf.Clamp(Mathf.FloorToInt((cp.z+maxDist-MinGrid)/UkuranGrid),0,SisiGrid-1);
            for(int gx=gx0;gx<=gx1;gx++)for(int gz=gz0;gz<=gz1;gz++)
            {
                var daftar=_grid[gx*SisiGrid+gz];
                for(int di=0;di<daftar.Length;di++)
                {
                    int idx=daftar[di];
                    var k=kelompok[idx];
                    if(k.mesh==null || k.material==null || k.matriks==null || k.matriks.Length==0)continue;
                    if(k.bounds.SqrDistance(cp)>maxSq || !GeometryUtility.TestPlanesAABB(_bidang,k.bounds))continue;
                    var ps=_pos[idx];
                    var bs=k.batasInstance;
                    int jumlah=0, batas=Mathf.Min(k.matriks.Length,Mathf.CeilToInt(k.matriks.Length*kep));
                    for(int i=0;i<batas && jumlah<_tampak.Length;i++)
                    {
                        Vector3 d=ps[i]-cp;
                        if(d.x*d.x+d.y*d.y+d.z*d.z<=radiusSq && GeometryUtility.TestPlanesAABB(_bidang,bs[i]))_tampak[jumlah++]=k.matriks[i];
                    }
                    if(jumlah==0)continue;
                    var rp=BayanganPadat?_rp[idx]:_rpNoShadow[idx]; rp.camera=cam;
                    int sub=_sub[idx];
                    for(int awal=0;awal<jumlah;awal+=AlasKeramat.Building.VegetationManager.MaksRender)
                        Graphics.RenderMeshInstanced(rp,k.mesh,sub,_tampak,Mathf.Min(AlasKeramat.Building.VegetationManager.MaksRender,jumlah-awal),awal);
                }
            }
        }
    }
}
