using System.Collections.Generic;
using System.IO;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using J=AlasKeramat.Core.ResepKarakter.JenisKarakter;
namespace AlasKeramat.EditorTools
{
    public static partial class NusantaraMasterBuilder
    {
        static readonly Dictionary<J,Mesh> _meshReal=new Dictionary<J,Mesh>();
        static readonly Dictionary<J,Vector3[]> _restReal=new Dictionary<J,Vector3[]>();
        static readonly Dictionary<J,Material[]> _bahanReal=new Dictionary<J,Material[]>();
        public static int JumlahKarakterReal;
        static void SiapkanKarakterReal() { _meshReal.Clear();_restReal.Clear();_bahanReal.Clear();JumlahKarakterReal=0; }
        static bool CobaKarakterReal(J jenis,GameObject root,Transform[] bones,bool andi)
        {
            if(jenis>J.KiRogo)return false;
            Mesh mesh;Vector3[] rest;
            if(!_meshReal.TryGetValue(jenis,out mesh))
            {
                mesh=ImporMeshVisual.Baca("Assets/ThirdParty/Prepared/"+jenis+".bytes",out rest);
                if(mesh==null || rest==null || rest.Length!=Kerangka.Jumlah) { Debug.LogWarning("[AK][G3] Model MakeHuman belum siap: "+jenis);return false; }
                _meshReal[jenis]=mesh;_restReal[jenis]=rest;
            }
            rest=_restReal[jenis];
            for(int i=0;i<bones.Length;i++)bones[i].position=root.transform.TransformPoint(rest[i]);
            var bind=new Matrix4x4[bones.Length];for(int i=0;i<bind.Length;i++)bind[i]=bones[i].worldToLocalMatrix*root.transform.localToWorldMatrix;
            if(mesh.bindposes.Length==0)
            { mesh.bindposes=bind;AssetDatabase.CreateAsset(mesh,FolderKarakter+"/MakeHuman_"+jenis+".asset"); }
            var materials=BahanMakeHuman(jenis);
            if(andi)
            {
                var body=BagiReal(mesh,false);var head=BagiReal(mesh,true);
                AssetDatabase.CreateAsset(body,FolderKarakter+"/MakeHuman_Andi_Body.asset");AssetDatabase.CreateAsset(head,FolderKarakter+"/MakeHuman_Andi_HeadArms.asset");
                RendererReal(root,"Andi_Body",body,bones,materials,ShadowCastingMode.On);
                RendererReal(root,"Andi_HeadArms",head,bones,materials,ShadowCastingMode.ShadowsOnly);
                var tl=new GameObject("TahiLalat").transform;tl.SetParent(bones[Kerangka.Head],false);tl.position=bones[Kerangka.Head].position+new Vector3(0f,0.085f,0.01f)+TahiLalatLokal;
            }
            else RendererReal(root,"Tubuh",mesh,bones,materials,ShadowCastingMode.On);
            root.AddComponent<PenandaModelReal>().Sumber="MakeHuman CC0 hm08";
            var face=root.AddComponent<AlasKeramat.AI.FaceController>();face.Pasang(root.GetComponentsInChildren<SkinnedMeshRenderer>(),jenis==J.Deden?"Kang Deden":jenis==J.KiRogo?"Ki Rogo":jenis.ToString());EditorUtility.SetDirty(face);
            AksesoriReal(jenis,bones);
            JumlahKarakterReal++;return true;
        }
        static void AksesoriReal(J j,Transform[] bones)
        {
            Transform head=bones[Kerangka.Head];
            if(j==J.Andi)DetailReal(head,"TahiLalatVisual",new Vector3(-0.032f,0.07f,0.082f),Vector3.one*0.006f,"#2C2118",PrimitiveType.Sphere);
            if(j==J.Deden)
            {
                DetailReal(head,"IketDeden",new Vector3(0f,0.14f,-0.005f),new Vector3(0.19f,0.012f,0.18f),"#514A3C",PrimitiveType.Cylinder);
                for(int i=0;i<2;i++)DetailReal(head,"PerbanTelinga",new Vector3(i==0?-0.078f:0.078f,0.06f,0f),new Vector3(0.025f,0.07f,0.045f),"#C9C0A9",PrimitiveType.Sphere);
            }
            if(j==J.KiRogo)DetailReal(head,"Blangkon",new Vector3(0f,0.15f,-0.014f),new Vector3(0.18f,0.065f,0.18f),"#221C18",PrimitiveType.Sphere);
            if(j==J.Ningsih)
            {
                for(int i=0;i<5;i++)DetailReal(head,"Kantil",new Vector3(0.065f+Mathf.Cos(i*1.25f)*0.008f,0.075f+Mathf.Sin(i*1.25f)*0.012f,-0.05f),new Vector3(0.008f,0.024f,0.006f),"#DDD5AE",PrimitiveType.Sphere);
            }
        }
        static void DetailReal(Transform parent,string nama,Vector3 pos,Vector3 scale,string warna,PrimitiveType bentuk)
        {
            var go=GameObject.CreatePrimitive(bentuk);go.name=nama;go.transform.SetParent(parent,false);go.transform.localPosition=pos;go.transform.localScale=scale;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            string path=FolderMat+"/M_DetailReal_"+warna.Substring(1)+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat==null)mat=MatPolos("M_DetailReal_"+warna.Substring(1),warna);go.GetComponent<Renderer>().sharedMaterial=mat;
        }
        static Mesh BagiReal(Mesh source,bool kepalaLengan)
        {
            var m=Object.Instantiate(source);m.name=source.name+(kepalaLengan?"_HeadArms":"_Body");var w=source.boneWeights;
            for(int sub=0;sub<source.subMeshCount;sub++)
            {
                int[] tri=source.GetTriangles(sub);var pilih=new List<int>();
                for(int i=0;i<tri.Length;i+=3)
                {
                    int bone=w[tri[i]].boneIndex0;
                    bool upper=bone==3 || bone==4 || bone==5 || (bone>=8 && bone<=13) || bone>=20;
                    if(upper==kepalaLengan) { pilih.Add(tri[i]);pilih.Add(tri[i+1]);pilih.Add(tri[i+2]); }
                }
                m.SetTriangles(pilih,sub);
            }
            return m;
        }
        static void RendererReal(GameObject root,string nama,Mesh mesh,Transform[] bones,Material[] materials,ShadowCastingMode shadows)
        {
            var go=Anak(root,nama);var smr=go.AddComponent<SkinnedMeshRenderer>();smr.sharedMesh=mesh;smr.bones=bones;smr.rootBone=bones[0];smr.sharedMaterials=materials;smr.shadowCastingMode=shadows;
            smr.quality=SkinQuality.Bone4;smr.localBounds=new Bounds(new Vector3(0f,1f,0f),new Vector3(2.8f,3f,2.8f));
        }
        static Texture2D TeksturKarakter(string rel,bool normal=false)
        {
            string path="Assets/ThirdParty/Characters/System/"+rel;if(!File.Exists(path))return null;
            AturImpor(path,normal,false,2048);return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static Material[] BahanMakeHuman(J j)
        {
            Material[] hasil;if(_bahanReal.TryGetValue(j,out hasil))return hasil;
            string skin=j==J.Ningsih?"young_asian_female/young_lightskinned_female_diffuse3":j==J.Deden?"middleage_asian_male/middleage_lightskinned_male_diffuse2":j==J.KiRogo?"old_asian_male/old_lightskinned_male_diffuse2":"young_asian_male/young_lightskinned_male_diffuse3";
            string suit=j==J.Ningsih?"female_elegantsuit01":j==J.KiRogo?"male_casualsuit01":"male_worksuit01";
            string hair=j==J.Ningsih?"ponytail01":"short01";
            var skinMat=SafeMaterialFactory.CreatePBR("M_Skin_"+j,j==J.Deden?new Color(0.65f,0.54f,0.43f):j==J.Ningsih?new Color(0.95f,0.91f,0.85f):new Color(0.8f,0.7f,0.59f),0f,0.32f,TeksturKarakter("skins/"+skin+".png"));
            var sh=AmbilShaderAK("AlasKeramat/KulitManusia");if(sh!=null)skinMat.shader=sh;
            var cloth=SafeMaterialFactory.CreatePBR("M_Pakaian_"+j,Color.white,0f,0.25f,PakaianBerwarna(j,suit),j==J.Ningsih?AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex+"/kain_n.png"):TeksturKarakter("clothes/"+suit+"/"+suit+"_normal.png",true));
            var boots=SafeMaterialFactory.CreatePBR("M_Boots_"+j,Color.white,0f,0.22f,TeksturKarakter("clothes/shoes01/shoes01_diffuse.png"),TeksturKarakter("clothes/shoes01/shoes01_normal.png",true));
            var rambut=SafeMaterialFactory.CreatePBR("M_Rambut_"+j,new Color(0.18f,0.14f,0.11f),0f,0.33f,TeksturKarakter("hair/"+hair+"/"+hair+"_diffuse.png"),null,null,SurfaceMode.Cutout,0.35f);
            SafeMaterialFactory.SetF(rambut,"_Cull",0f);
            var eye=SafeMaterialFactory.CreatePBR("M_Mata_"+j,Color.white,0f,0.85f,TeksturKarakter("eyes/materials/brown_eye.png"));
            hasil=new[]{SimpanMat(skinMat),SimpanMat(cloth),SimpanMat(boots),SimpanMat(rambut),SimpanMat(eye)};_bahanReal[j]=hasil;return hasil;
        }
        static Texture2D PakaianBerwarna(J j,string suit)
        {
            string src="Assets/ThirdParty/Characters/System/clothes/"+suit+"/"+suit+"_diffuse.png";
            var imp=AssetImporter.GetAtPath(src) as TextureImporter;if(imp==null)return null;imp.isReadable=true;imp.maxTextureSize=2048;imp.SaveAndReimport();
            var source=AssetDatabase.LoadAssetAtPath<Texture2D>(src);var px=source.GetPixels();
            Color tone=j==J.Deden?new Color(0.88f,0.43f,0.12f):j==J.Ningsih?new Color(0.86f,0.83f,0.73f):j==J.KiRogo?new Color(0.14f,0.13f,0.12f):new Color(0.5f,0.48f,0.31f);
            for(int i=0;i<px.Length;i++)
            {
                float lum=px[i].grayscale;px[i]=tone*Mathf.Lerp(0.35f,1.25f,lum);px[i].a=1f;
                if(j==J.Ningsih)
                {
                    float u=(float)(i%source.width)/source.width,v=(float)(i/source.width)/source.height;
                    float grain=0.92f+0.08f*Mathf.PerlinNoise(u*340f,v*340f);
                    if(u<0.5f)px[i]=new Color(0.7f,0.68f,0.61f)*grain;
                    else
                    {
                        float x=Mathf.Repeat(u*32f,1f)-0.5f,y=Mathf.Repeat(v*24f,1f)-0.5f;
                        float kawung=Mathf.SmoothStep(0.22f,0.28f,Mathf.Abs(x)+Mathf.Abs(y));
                        px[i]=Color.Lerp(new Color(0.42f,0.26f,0.11f),new Color(0.11f,0.065f,0.035f),kawung)*grain;
                    }
                    px[i].a=1f;
                }
            }
            var tex=new Texture2D(source.width,source.height,TextureFormat.RGBA32,true);tex.SetPixels(px);tex.Apply();string path=FolderKarakter+"/Pakaian_"+j+".png";File.WriteAllBytes(path,tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);AturImpor(path,false,false,2048);return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
