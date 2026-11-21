using System.IO;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace AlasKeramat.EditorTools
{
    public static partial class NusantaraMasterBuilder
    {
        static void LangkahG3()
        {
            BuatSerasahHutan();
            var go=Anak(_root,"AtmosferHutanG3");
            string path=FolderTex+"/noise_kabut.png";
            var tex=ProcTex.FromHeight("NoiseKabut",128,ProcTex.HeightMap(128,(u,v)=>ProcTex.Fbm(u,v,4,4,2026)),ProcTex.Palet(new[]{Color.black,Color.white}));
            File.WriteAllBytes(path,tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);AturImpor(path,false,false,128);
            var sh=AmbilShaderAK("AlasKeramat/KabutLokal");
            if(sh==null)throw new System.Exception("Shader kabut G3 tidak tersedia");
            var mat=new Material(sh) { name="M_KabutLokal" };mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(path));
            mat=SimpanMat(mat);
            var t=new Transform[AtmosferHutan.JumlahKartu];var r=new Renderer[t.Length];
            for(int i=0;i<t.Length;i++)
            {
                var card=GameObject.CreatePrimitive(PrimitiveType.Quad);card.name="KabutLapis_"+i;card.transform.SetParent(go.transform,false);card.transform.localScale=new Vector3(26f,7.5f,1f);
                Object.DestroyImmediate(card.GetComponent<Collider>());t[i]=card.transform;r[i]=card.GetComponent<Renderer>();r[i].sharedMaterial=mat;r[i].shadowCastingMode=ShadowCastingMode.Off;r[i].receiveShadows=false;
            }
            var atmos=go.AddComponent<AtmosferHutan>();atmos.Pasang(t,r);
            PasangSinarTajuk(go,sh,atmos);
            EditorUtility.SetDirty(atmos);
            var cam=Object.FindFirstObjectByType<AlasKeramat.Player.AndiFPSController>();
            if(cam!=null && cam.Kamera!=null)atmos.Terapkan(cam.Kamera.GetComponent<Camera>(),9f,WeatherState.Cerah);
            Catat("G3a: bulan terbaca, gua tidak hitam, grading netral, bayangan 2 kaskade, kabut lokal, god rays hutan & basah dinamis");
        }

        static void PasangSinarTajuk(GameObject induk,Shader sh,AtmosferHutan atmos)
        {
            string pSinar=FolderTex+"/sinar_tajuk.png";
            var tex=new Texture2D(128,128,TextureFormat.RGBA32,false) { name="SinarTajuk" };
            for(int y=0;y<128;y++)
            {
                float v=y/127f;
                float vertikal=Mathf.SmoothStep(0f,0.28f,v)*(1f-Mathf.SmoothStep(0.65f,1f,v));
                for(int x=0;x<128;x++)
                {
                    float u=x/127f;
                    float dx=Mathf.Abs(u-0.5f)*2f;
                    float batang=Mathf.Exp(-dx*dx*5.2f);
                    float berkas=0.68f+0.32f*Mathf.Sin(u*Mathf.PI*7f)*Mathf.Sin(u*Mathf.PI*3f+0.6f);
                    float a=Mathf.Clamp01(batang*vertikal*berkas);
                    tex.SetPixel(x,y,new Color(a,a,a,1f));
                }
            }
            tex.Apply();
            File.WriteAllBytes(pSinar,tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(pSinar);AturImpor(pSinar,false,false,128);
            var shSinar=AmbilShaderAK("AlasKeramat/SinarTajuk")??sh;
            var matSinar=new Material(shSinar) { name="M_SinarTajuk" };matSinar.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(pSinar));
            matSinar=SimpanMat(matSinar);
            var ts=new Transform[AtmosferHutan.JumlahSinar];var rs=new Renderer[ts.Length];
            for(int i=0;i<ts.Length;i++)
            {
                var q=GameObject.CreatePrimitive(PrimitiveType.Quad);q.name="SinarTajuk_"+i;q.transform.SetParent(induk.transform,false);q.transform.localScale=new Vector3(5.8f,21f,1f);
                Object.DestroyImmediate(q.GetComponent<Collider>());ts[i]=q.transform;rs[i]=q.GetComponent<Renderer>();rs[i].sharedMaterial=matSinar;rs[i].shadowCastingMode=ShadowCastingMode.Off;rs[i].receiveShadows=false;
            }
            atmos.PasangSinar(ts,rs);
        }
    }
}
