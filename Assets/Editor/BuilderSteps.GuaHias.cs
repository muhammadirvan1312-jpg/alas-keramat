using System.IO;
using AlasKeramat.Core;
using AlasKeramat.World;
using UnityEditor;
using UnityEngine;
namespace AlasKeramat.EditorTools
{
    public static partial class NusantaraMasterBuilder
    {
        static void HiasGua(GameObject parent,int id,Material cyan,Material merah)
        {
            Vector3 origin=parent.transform.position;
            var batu=AmbilMat("M_Batu");var kayu=AmbilMat("M_Kayu");var besi=AmbilMat("M_LogamKarat");
            if(id==1)
            {
                var rng=new System.Random(1010);
                for(int i=0;i<40;i++)
                {
                    float x=-38f+(float)rng.NextDouble()*16f,z=-7f+(float)rng.NextDouble()*14f,h=1f+(float)rng.NextDouble()*3f;
                    var stal=GameObject.CreatePrimitive(PrimitiveType.Sphere);stal.name="Stalaktit_"+i;stal.transform.SetParent(parent.transform,false);stal.transform.localPosition=new Vector3(x,10f-h*0.3f,z);stal.transform.localScale=new Vector3(0.6f,h,0.6f);stal.GetComponent<Renderer>().sharedMaterial=i%5==0?cyan:batu;Object.DestroyImmediate(stal.GetComponent<Collider>());
                }
                for(int i=0;i<3;i++)KotakGua(parent,"TendaRobek",origin+new Vector3(17f+i*2f,0.4f,33f),new Vector3(1.5f,0.7f,2f),kayu);
                KotakGua(parent,"MejaAltar",origin+new Vector3(120f,0.2f,-28f),new Vector3(3f,0.4f,2f),batu);
            }
            else if(id==2)
            {
                for(int i=0;i<4;i++)KotakGua(parent,"RakArsip1928",origin+new Vector3(-32f+i*2.5f,1.4f,-20f),new Vector3(1.7f,2.8f,0.7f),besi);
                KotakGua(parent,"MejaOperasi",origin+new Vector3(31f,0.8f,-16f),new Vector3(2.5f,0.3f,1f),besi);
                for(int i=0;i<3;i++)
                {
                    var tabung=GameObject.CreatePrimitive(PrimitiveType.Cylinder);tabung.name="TabungLab";tabung.transform.SetParent(parent.transform,false);tabung.transform.localPosition=new Vector3(24f+i*3f,1.2f,-20f);tabung.transform.localScale=new Vector3(1f,1.2f,1f);tabung.GetComponent<Renderer>().sharedMaterial=cyan;tabung.layer=GameLayers.Ground;
                }
            }
            else
            {
                for(int i=0;i<8;i++)
                {
                    float a=i*Mathf.PI/4f;Vector3 p=new Vector3(Mathf.Cos(a)*15f,8f,40f+Mathf.Sin(a)*15f);
                    var akar=GameObject.CreatePrimitive(PrimitiveType.Cylinder);akar.name="PilarAkar";akar.transform.SetParent(parent.transform,false);akar.transform.localPosition=p;akar.transform.localScale=new Vector3(1.5f,8f,1.5f);akar.transform.localRotation=Quaternion.Euler(0f,0f,i%2==0?8f:-8f);akar.GetComponent<Renderer>().sharedMaterial=merah;akar.layer=GameLayers.Ground;
                    KotakGua(parent,"TungkuMerah",origin+new Vector3(p.x,0.6f,p.z),new Vector3(1f,1.2f,1f),merah);
                }
                KotakGua(parent,"PanggungKiRogo",origin+new Vector3(0f,0.15f,40f),new Vector3(8f,0.3f,8f),batu);
            }
            const int sr=22050,detik=12;
            var samples=new float[sr*detik];var random=new System.Random(100+id);
            for(int i=0;i<samples.Length;i++)
            {
                float t=(float)i/sr,fade=Mathf.Min(1f,Mathf.Min(t,detik-t)*2f);
                float hum=id==1?Mathf.Sin(t*2f*Mathf.PI*48f)+Mathf.Sin(t*2f*Mathf.PI*50.5f):id==2?Mathf.Sin(t*2f*Mathf.PI*50f)+0.3f*Mathf.Sin(t*2f*Mathf.PI*100f):Mathf.Sin(t*2f*Mathf.PI*40f)*(0.5f+0.5f*Mathf.Sin(t*2f*Mathf.PI/1.4f));
                float tetes=id==1?Mathf.Sin(t*2f*Mathf.PI*1700f)*Mathf.Exp(-Mathf.Repeat(t,1.3f)*32f):0f;
                samples[i]=fade*(hum*0.035f+tetes*0.06f+((float)random.NextDouble()-0.5f)*0.003f);
            }
            string path=FolderGua+"/ambien_ug"+id+".wav";File.WriteAllBytes(path,WavWriter.Encode(samples,sr));AssetDatabase.ImportAsset(path);
            var audio=parent.AddComponent<AudioSource>();audio.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);audio.loop=true;audio.playOnAwake=false;audio.spatialBlend=0f;audio.volume=0.4f;
            parent.AddComponent<AudioGua>().Pasang((UndergroundId)id,audio);
        }
    }
}
