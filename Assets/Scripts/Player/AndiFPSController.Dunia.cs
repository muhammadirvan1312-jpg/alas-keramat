using AlasKeramat.Building;
using AlasKeramat.Core;
using UnityEngine;
namespace AlasKeramat.Player
{
    public partial class AndiFPSController : ISaveable
    {
        public const float KecepatanRakit = 2.5f;
        public FungsiBangunan RakitAktif { get; private set; }
        public bool Duduk { get; private set; }
        Transform _rakitRoot;
        public string SaveKey { get { return "posisi_andi"; } }
        [System.Serializable] class PosisiAndiT6 { public Vector3 posisi; public float yaw, pitch; }
        public string CaptureJson() { return JsonUtility.ToJson(new PosisiAndiT6 { posisi=transform.position, yaw=_yaw, pitch=_pitch }); }
        public void RestoreJson(string json)
        {
            var d=JsonUtility.FromJson<PosisiAndiT6>(json); if(d==null)return;
            TurunRakit(); Duduk=false; Teleport(d.posisi); _yaw=d.yaw; _pitch=d.pitch;
            transform.rotation=Quaternion.Euler(0f,_yaw,0f);
        }
        public void Teleport(Vector3 posisi)
        {
            HentikanInputGerak(); KecepatanDatar=0f; DiTanah=false; Berenang=false; Menyelam=false;
            var c=GetComponent<CharacterController>(); bool aktif=c!=null && c.enabled;
            if(c!=null)c.enabled=false;
            transform.position=posisi; _vDatar=Vector3.zero; _vy=0f; _diUdara=false; _puncakY=posisi.y;
            if(c!=null)c.enabled=aktif;
        }
        public void NaikRakit(FungsiBangunan rakit)
        {
            if(rakit==null || RakitAktif!=null)return;
            var id=rakit.GetComponentInParent<IdentitasBangunan>(); if(id==null)return;
            Duduk=false; RakitAktif=rakit; _rakitRoot=id.transform;
            if(_cc!=null)_cc.enabled=false;
            Berenang=false; Menyelam=false; _diUdara=false;
            transform.position=_rakitRoot.position+Vector3.up*0.4f;
        }
        public void TurunRakit()
        {
            if(RakitAktif!=null && _rakitRoot!=null)
            {
                Vector3 p=_rakitRoot.position+_rakitRoot.right*1.8f+Vector3.up*0.5f;
                RakitAktif=null; _rakitRoot=null;
                Teleport(p);
            }
            if(_cc!=null)_cc.enabled=true;
        }
        public void DudukDi(Vector3 p) { TurunRakit(); Teleport(p); Duduk=true; }
        public void Berdiri() { Duduk=false; }
        bool GerakKendaraan(float dt)
        {
            if(Duduk) { KecepatanDatar=0f; SedangLari=false; return true; }
            if(RakitAktif==null || _rakitRoot==null) return false;
            Vector3 gerak=transform.TransformDirection(new Vector3(InputGerak.x,0f,InputGerak.y));
            gerak=Vector3.ClampMagnitude(gerak,1f)*KecepatanRakit*dt;
            Vector3 p=_rakitRoot.position, calon=p+gerak;
            float? air=WorldTerrainGenerator.GetWaterLevelAt(calon.x,calon.z);
            bool sah=air.HasValue && air.Value>WorldTerrainGenerator.SampleHeight(calon.x,calon.z)+0.25f;
            for(int i=0;i<4 && sah;i++)
            {
                Vector3 q=calon+_rakitRoot.right*(i%2==0?-1.2f:1.2f)+_rakitRoot.forward*(i<2?-1.8f:1.8f);
                float? w=WorldTerrainGenerator.GetWaterLevelAt(q.x,q.z);
                sah=w.HasValue && w.Value>WorldTerrainGenerator.SampleHeight(q.x,q.z)+0.15f;
            }
            if(sah && gerak.sqrMagnitude>0f)
            {
                RaycastHit hit;
                int mask=(1<<GameLayers.Ground)|(1<<GameLayers.Structure);
                if(!Physics.BoxCast(p+Vector3.up*0.8f,new Vector3(1.1f,0.3f,1.7f),gerak.normalized,out hit,_rakitRoot.rotation,gerak.magnitude,mask,QueryTriggerInteraction.Ignore))
                { calon.y=air.Value; _rakitRoot.position=calon; }
            }
            transform.position=_rakitRoot.position+Vector3.up*0.4f;
            Berenang=false; Menyelam=false; SedangLari=false; KecepatanDatar=0f; Napas=NapasMaks;
            return true;
        }
    }
}
