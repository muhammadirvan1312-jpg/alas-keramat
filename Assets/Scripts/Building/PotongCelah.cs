using System.Collections.Generic;
using UnityEngine;
namespace AlasKeramat.Building
{
    public static class PotongCelah
    {
        public const int Pukulan=3;
        public static bool Pukul(BagianStruktur bagian, Vector3 titik, bool pintu)
        {
            if(bagian==null || bagian.Jenis!=BagianStruktur.Tipe.Balok || bagian.Panjang<1.2f)return false;
            bagian.PukulCelah++;
            if(bagian.PukulCelah<Pukulan) { Core.GameEvents.RaiseToast("POTONG CELAH "+bagian.PukulCelah+"/3"); return true; }
            Vector3 arah=bagian.transform.up;
            Vector3 pusat=bagian.transform.position+arah*Vector3.Dot(titik-bagian.transform.position,arah);
            float lebar=pintu?1f:0.8f, tinggi=pintu?2f:0.8f;
            float dasar=pintu?Core.WorldTerrainGenerator.SampleHeight(pusat.x,pusat.z):pusat.y-tinggi*0.5f;
            var dinding=new List<BagianStruktur>();
            foreach(var s in BagianStruktur.Semua)
            {
                if(s.Jenis!=BagianStruktur.Tipe.Balok || Mathf.Abs(Vector3.Dot(s.transform.up,arah))<0.95f)continue;
                Vector3 delta=s.transform.position-pusat; delta.y=0f;
                if(Vector3.ProjectOnPlane(delta,arah).magnitude>0.3f || s.transform.position.y<dasar || s.transform.position.y>dasar+tinggi)continue;
                float offset=Vector3.Dot(pusat-s.transform.position,s.transform.up);
                if(Mathf.Abs(offset)+lebar*0.5f<s.Panjang*0.5f)dinding.Add(s);
            }
            foreach(var s in dinding)
            {
                Vector3 u=s.transform.up; float offset=Vector3.Dot(pusat-s.transform.position,u);
                float kiri=offset-lebar*0.5f+s.Panjang*0.5f, kanan=s.Panjang*0.5f-offset-lebar*0.5f;
                var tetangga=new List<BagianStruktur>(s.Tetangga);
                var mat=s.GetComponent<Renderer>().sharedMaterial;
                var a=ShoulderLogAndFreeformBuilder.Buat(new ShoulderLogAndFreeformBuilder.Rencana { mode=ShoulderLogAndFreeformBuilder.ModeBangun.Balok, pos=s.Ujung(0)+u*kiri*0.5f, rot=s.transform.rotation, panjang=kiri, tertanam=s.Tertanam },mat);
                var b=ShoulderLogAndFreeformBuilder.Buat(new ShoulderLogAndFreeformBuilder.Rencana { mode=ShoulderLogAndFreeformBuilder.ModeBangun.Balok, pos=s.Ujung(1)-u*kanan*0.5f, rot=s.transform.rotation, panjang=kanan, tertanam=s.Tertanam },mat);
                a.HP=s.HP; b.HP=s.HP;
                foreach(var t in tetangga)
                {
                    if(t==null)continue;
                    float jarak=Vector3.Dot(t.transform.position-pusat,u);
                    if(jarak<=0f)a.Hubungkan(t); if(jarak>=0f)b.Hubungkan(t);
                }
                s.Hancur(false);
            }
            Core.GameEvents.RaiseToast(pintu?"Celah pintu terbuka":"Celah jendela terbuka"); return true;
        }
    }
}
