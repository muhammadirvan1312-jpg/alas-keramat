using AlasKeramat.Core;
using UnityEngine;
namespace AlasKeramat.Player
{
    public partial class AndiFPSController
    {
        public bool GerakDikunci;
        public bool SedangZipline { get; private set; }
        Vector3 _ujungTali;
        public static float KecepatanTali(Vector3 arah)
        { return arah.y>0.01f?3f:Mathf.Min(16f,6f+18f*Mathf.Max(0f,-arah.normalized.y)); }
        public void MulaiZipline(Vector3 asal,Vector3 akhir)
        {
            if(GerakDikunci || SedangZipline)return;
            TurunRakit(); Duduk=false; Teleport(asal); _ujungTali=akhir; SedangZipline=true;
        }
        public void LepasZipline() { SedangZipline=false; _vDatar=Vector3.zero; _vy=0f; _diUdara=false; }
        bool GerakZipline(float dt)
        {
            if(!SedangZipline)return false;
            var v=GetComponent<AndiVitalsAndSanity>();
            Vector3 arah=_ujungTali-transform.position;
            if(arah.y>0.01f && v!=null) { if(v.Stamina<6f*dt) { LepasZipline(); return false; } v.Pakai(6f*dt); }
            Vector3 tujuan=Vector3.MoveTowards(transform.position,_ujungTali,KecepatanTali(arah)*dt);
            var flags=_cc.Move(tujuan-transform.position);
            if((flags & CollisionFlags.Sides)!=0 || (transform.position-_ujungTali).sqrMagnitude<0.04f)LepasZipline();
            Berenang=false; Menyelam=false; SedangLari=false; return true;
        }
        static float DasarGua(Vector3 posisi)
        {
            RaycastHit hit;
            return Physics.Raycast(posisi+Vector3.up*0.4f,Vector3.down,out hit,20f,1<<GameLayers.Ground,QueryTriggerInteraction.Ignore)?hit.point.y:posisi.y-2f;
        }
    }
}
