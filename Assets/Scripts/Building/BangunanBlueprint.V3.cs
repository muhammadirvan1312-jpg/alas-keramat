using AlasKeramat.Core;
using UnityEngine;
namespace AlasKeramat.Building
{
    public static partial class BangunanBlueprint
    {
        static void BangunV3(GameObject root, BlueprintID id)
        {
            Transform r=root.transform; int S=GameLayers.Structure, I=GameLayers.Interactable;
            GameObject inti=null; Light lamp=null; Transform engsel=null;
            switch(id)
            {
                case BlueprintID.MangkukKepiting:
                    inti=P(r,PrimitiveType.Cylinder,new Vector3(0f,0.12f,0f),new Vector3(0.6f,0.12f,0.6f),Batu,I,true);
                    P(r,PrimitiveType.Sphere,new Vector3(0f,0.26f,0f),new Vector3(0.2f,0.06f,0.15f),new Color(0.6f,0.15f,0.1f),0,false); break;
                case BlueprintID.PatungNgeri:
                    inti=P(r,PrimitiveType.Cylinder,new Vector3(0f,1.1f,0f),new Vector3(0.1f,1.1f,0.1f),Kayu,I,true);
                    for(int i=0;i<4;i++) P(r,PrimitiveType.Capsule,new Vector3(0f,0.8f+i*0.3f,0f),new Vector3(0.09f,0.45f,0.09f),Tulang,0,false,new Vector3(0f,0f,60f+i*20f));
                    lamp=Lampu(r,Vector3.up*2.2f,10f,new Color(1f,0.5f,0.2f)); break;
                case BlueprintID.BatuAsah:
                    inti=P(r,PrimitiveType.Sphere,new Vector3(0f,0.35f,0f),new Vector3(0.8f,0.7f,0.6f),Batu,I,true); break;
                case BlueprintID.RumahPohon:
                    P(r,PrimitiveType.Cube,new Vector3(0f,4f,0f),new Vector3(3.6f,0.15f,3.6f),Kayu,S,true);
                    for(int i=0;i<4;i++) P(r,PrimitiveType.Cylinder,new Vector3(i%2==0?-1.6f:1.6f,5f,i<2?-1.6f:1.6f),new Vector3(0.14f,1.1f,0.14f),Kayu,S,true);
                    for(int i=0;i<2;i++) P(r,PrimitiveType.Cube,new Vector3(i==0?-0.95f:0.95f,6.2f,0f),new Vector3(2.2f,0.1f,3.9f),Daun,S,true,new Vector3(0f,0f,i==0?25f:-25f));
                    for(int i=0;i<13;i++) P(r,PrimitiveType.Cube,new Vector3(0f,0.3f+i*0.3f,2f),new Vector3(0.7f,0.09f,0.12f),Kayu,0,false);
                    inti=P(r,PrimitiveType.Cube,new Vector3(0f,1.2f,2f),new Vector3(0.7f,2.4f,0.1f),Kayu,I,true);
                    var kasur=P(r,PrimitiveType.Cube,new Vector3(0.7f,4.2f,0f),new Vector3(0.8f,0.12f,1.8f),Daun,I,true);
                    Fungsi(kasur.transform,BlueprintID.SaungPanggung);
                    break;
                case BlueprintID.PerangkapPunji:
                    inti=P(r,PrimitiveType.Cube,new Vector3(0f,0.15f,0f),new Vector3(2f,0.3f,2f),Daun,I,true);
                    inti.GetComponent<Collider>().isTrigger=true;
                    for(int i=0;i<8;i++) P(r,PrimitiveType.Cylinder,new Vector3((i%4-1.5f)*0.45f,0.3f,(i/4-0.5f)*0.7f),new Vector3(0.06f,0.3f,0.06f),Bambu,0,false,new Vector3(15f,0f,0f)); break;
                case BlueprintID.JeratHewan:
                    inti=P(r,PrimitiveType.Cylinder,new Vector3(0f,0.03f,0f),new Vector3(0.8f,0.03f,0.8f),Kayu,I,true);
                    P(r,PrimitiveType.Cylinder,new Vector3(0.4f,0.6f,0f),new Vector3(0.05f,0.6f,0.05f),Kayu,S,true); break;
                case BlueprintID.BubuIkan:
                    inti=P(r,PrimitiveType.Cylinder,new Vector3(0f,0.15f,0f),new Vector3(0.6f,0.65f,0.6f),Bambu,I,true,new Vector3(90f,0f,0f));
                    for(int i=0;i<6;i++) P(r,PrimitiveType.Cylinder,new Vector3(Mathf.Cos(i*Mathf.PI/3f)*0.34f,0.2f+Mathf.Sin(i*Mathf.PI/3f)*0.34f,0f),new Vector3(0.03f,0.7f,0.03f),Kayu,0,false,new Vector3(90f,0f,0f)); break;
                case BlueprintID.PetiPenyimpanan:
                    inti=P(r,PrimitiveType.Cube,new Vector3(0f,0.4f,0f),new Vector3(1.4f,0.8f,0.8f),Bambu,I,true);
                    P(r,PrimitiveType.Cube,new Vector3(0f,0.82f,0f),new Vector3(1.45f,0.08f,0.85f),Kayu,S,true); break;
                case BlueprintID.LampuMinyak:
                    inti=P(r,PrimitiveType.Cylinder,new Vector3(0f,0.25f,0f),new Vector3(0.3f,0.25f,0.3f),Logam,I,true);
                    lamp=Lampu(r,Vector3.up*0.6f,10f,new Color(1f,0.75f,0.4f)); break;
                case BlueprintID.JembatanGantung:
                    float panjang=root.GetComponent<IdentitasBangunan>().Bentang;
                    int n=Mathf.CeilToInt(panjang/0.35f);
                    for(int i=0;i<n;i++) P(r,PrimitiveType.Cube,new Vector3(0f,0f,(i+0.5f)*panjang/n),new Vector3(1.2f,0.12f,panjang/n),Kayu,S,true);
                    for(int i=0;i<2;i++) P(r,PrimitiveType.Cylinder,new Vector3(i==0?-0.62f:0.62f,0.9f,panjang*0.5f),new Vector3(0.04f,panjang*0.5f,0.04f),Kayu,0,false,new Vector3(90f,0f,0f));
                    break;
                case BlueprintID.GerbangPagar:
                    engsel=new GameObject("Engsel").transform; engsel.SetParent(r,false); engsel.localPosition=Vector3.left;
                    inti=P(engsel,PrimitiveType.Cube,new Vector3(1f,1f,0f),new Vector3(2f,2f,0.15f),Kayu,I,true); break;
                case BlueprintID.KursiMeja:
                    P(r,PrimitiveType.Cube,new Vector3(0f,0.8f,0f),new Vector3(1.4f,0.1f,0.8f),Kayu,S,true);
                    for(int i=0;i<4;i++) P(r,PrimitiveType.Cube,new Vector3(i%2==0?-0.6f:0.6f,0.4f,i<2?-0.3f:0.3f),new Vector3(0.1f,0.8f,0.1f),Kayu,S,true);
                    inti=P(r,PrimitiveType.Cube,new Vector3(0f,0.45f,-1f),new Vector3(0.6f,0.12f,0.6f),Kayu,I,true);
                    P(r,PrimitiveType.Cube,new Vector3(0f,0.8f,-1.3f),new Vector3(0.6f,0.7f,0.08f),Kayu,S,true); break;
            }
            if(inti!=null) Fungsi(inti.transform,id,null,lamp,engsel);
        }
    }
}
