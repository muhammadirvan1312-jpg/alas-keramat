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
        static readonly Dictionary<string,Mesh> _meshReal=new Dictionary<string,Mesh>();
        static readonly Dictionary<string,Vector3[]> _restReal=new Dictionary<string,Vector3[]>();
        static readonly Dictionary<J,Material[]> _bahanReal=new Dictionary<J,Material[]>();
        static Texture2D _normKulitWajah,_texRambutSeram,_texKainBase;
        public static int JumlahKarakterReal;

        static void SiapkanKarakterReal()
        {
            _meshReal.Clear();_restReal.Clear();_bahanReal.Clear();
            _normKulitWajah=null;_texRambutSeram=null;_texKainBase=null;JumlahKarakterReal=0;
        }

        static string NamaModelReal(J j)
        {
            switch(j)
            {
                case J.Andi:return "Andi";
                case J.Deden:return "Deden";
                case J.Ningsih:return "Ningsih";
                case J.KiRogo:return "KiRogo";
                case J.Pocong:return "Pocong";
                case J.Kuntilanak:return "Kuntilanak";
                case J.Genderuwo:return "Genderuwo";
                case J.Kuyang:return "Kuyang";
                case J.Tuyul:return "Tuyul";
                case J.HantuAir:return "HantuAir";
                case J.Penyerupa:return "Ningsih";
                case J.AbdiWanita:return "SekteWanita";
                case J.AbdiButo:return "SekteButo";
                default:return "SektePria";
            }
        }

        static bool CobaKarakterReal(J jenis,GameObject root,Transform[] bones,bool andi)
        {
            string key=NamaModelReal(jenis);
            Mesh mesh;Vector3[] rest;
            if(!_meshReal.TryGetValue(key,out mesh))
            {
                mesh=ImporMeshVisual.Baca("Assets/ThirdParty/Prepared/"+key+".bytes",out rest);
                if(mesh==null || rest==null || rest.Length!=Kerangka.Jumlah)
                {
                    if(jenis<=J.KiRogo)Debug.LogWarning("[AK][G3] Model MakeHuman belum siap: "+key);
                    return false;
                }
                _meshReal[key]=mesh;_restReal[key]=rest;
            }
            rest=_restReal[key];
            for(int i=0;i<bones.Length;i++)bones[i].position=root.transform.TransformPoint(rest[i]);
            var bind=new Matrix4x4[bones.Length];
            for(int i=0;i<bind.Length;i++)bind[i]=bones[i].worldToLocalMatrix*root.transform.localToWorldMatrix;
            if(mesh.bindposes.Length==0)
            {
                mesh.bindposes=bind;
                string ap=FolderKarakter+"/MakeHuman_"+key+".asset";
                AssetDatabase.DeleteAsset(ap);AssetDatabase.CreateAsset(mesh,ap);
                mesh=AssetDatabase.LoadAssetAtPath<Mesh>(ap);_meshReal[key]=mesh;
            }
            var materials=BahanMakeHuman(jenis);
            if(andi)
            {
                var body=BagiReal(mesh,false);var head=BagiReal(mesh,true);
                AssetDatabase.DeleteAsset(FolderKarakter+"/MakeHuman_Andi_Body.asset");
                AssetDatabase.DeleteAsset(FolderKarakter+"/MakeHuman_Andi_HeadArms.asset");
                AssetDatabase.CreateAsset(body,FolderKarakter+"/MakeHuman_Andi_Body.asset");
                AssetDatabase.CreateAsset(head,FolderKarakter+"/MakeHuman_Andi_HeadArms.asset");
                RendererReal(root,"Andi_Body",body,bones,materials,ShadowCastingMode.On);
                RendererReal(root,"Andi_HeadArms",head,bones,materials,ShadowCastingMode.ShadowsOnly);
                var tl=new GameObject("TahiLalat").transform;tl.SetParent(bones[Kerangka.Head],false);
                tl.position=bones[Kerangka.Head].position+new Vector3(0f,0.085f,0.01f)+TahiLalatLokal;
            }
            else RendererReal(root,"Tubuh",mesh,bones,materials,ShadowCastingMode.On);
            root.AddComponent<PenandaModelReal>().Sumber="MakeHuman CC0 hm08";
            var face=root.AddComponent<AlasKeramat.AI.FaceController>();
            face.Pasang(root.GetComponentsInChildren<SkinnedMeshRenderer>(),jenis==J.Deden?"Kang Deden":jenis==J.KiRogo?"Ki Rogo":jenis.ToString());
            if(jenis==J.Pocong || jenis==J.Kuntilanak || jenis==J.Kuyang)face.SetEkspresi(AlasKeramat.AI.EkspresiWajahG3.Ngeri,0.9f);
            else if(jenis==J.Genderuwo)face.SetEkspresi(AlasKeramat.AI.EkspresiWajahG3.Teriak,0.9f);
            else if(jenis==J.Penyerupa || jenis==J.Tuyul)face.SetEkspresi(AlasKeramat.AI.EkspresiWajahG3.Curiga,0.9f);
            else if(jenis==J.Ningsih)face.SetEkspresi(AlasKeramat.AI.EkspresiWajahG3.Senyum,0.45f);
            EditorUtility.SetDirty(face);
            AksesoriReal(jenis,bones,andi);
            JumlahKarakterReal++;return true;
        }

        static void AksesoriReal(J j,Transform[] bones,bool firstPerson)
        {
            Transform head=bones[Kerangka.Head];
            if(j==J.Andi)
            {
                var mole=DetailReal(head,"TahiLalatVisual",new Vector3(-0.032f,0.07f,0.082f),Vector3.one*0.005f,"#2C2118",PrimitiveType.Sphere);
                if(firstPerson)mole.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.ShadowsOnly;
            }
        }

        static GameObject DetailReal(Transform parent,string nama,Vector3 pos,Vector3 scale,string warna,PrimitiveType bentuk)
        {
            var go=GameObject.CreatePrimitive(bentuk);go.name=nama;go.transform.SetParent(parent,false);
            go.transform.localPosition=pos;go.transform.localScale=scale;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            string path=FolderMat+"/M_DetailReal_"+warna.Substring(1)+".mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null)mat=MatPolos("M_DetailReal_"+warna.Substring(1),warna);
            go.GetComponent<Renderer>().sharedMaterial=mat;return go;
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
            var go=Anak(root,nama);var smr=go.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh=mesh;smr.bones=bones;smr.rootBone=bones[0];smr.sharedMaterials=materials;smr.shadowCastingMode=shadows;
            smr.quality=SkinQuality.Bone4;smr.localBounds=new Bounds(new Vector3(0f,1.1f,0f),new Vector3(3.2f,3.4f,3.2f));
        }

        static Texture2D TeksturKarakter(string rel,bool normal=false)
        {
            string path="Assets/ThirdParty/Characters/System/"+rel;if(!File.Exists(path))return null;
            AturImpor(path,normal,false,2048);return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Texture2D NormalKulitWajah()
        {
            if(_normKulitWajah!=null)return _normKulitWajah;
            string p=FolderKarakter+"/kulit_pori_n.png";
            if(!File.Exists(p))
            {
                const int sz=512;
                float[] h=ProcTex.HeightMap(sz,(u,v)=>ProcTex.Fbm(u,v,110,3,907)*0.6f+ProcTex.Fbm(u,v,240,2,911)*0.4f);
                SimpanPng(ProcTex.NormalFromHeight("kulit_pori_n",sz,h,0.22f),p);
                AssetDatabase.ImportAsset(p);AturImpor(p,true,false,sz);
            }
            _normKulitWajah=AssetDatabase.LoadAssetAtPath<Texture2D>(p);return _normKulitWajah;
        }

        static Texture2D BacaReadableKarakter(string path)
        {
            if(!File.Exists(path))return null;
            AssetDatabase.ImportAsset(path);
            var imp=AssetImporter.GetAtPath(path) as TextureImporter;
            if(imp!=null && (!imp.isReadable || imp.textureCompression!=TextureImporterCompression.Uncompressed))
            {
                imp.isReadable=true;imp.maxTextureSize=2048;imp.textureCompression=TextureImporterCompression.Uncompressed;
                var android=imp.GetPlatformTextureSettings("Android");android.overridden=true;android.maxTextureSize=2048;android.format=TextureImporterFormat.RGBA32;
                imp.SetPlatformTextureSettings(android);imp.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Texture2D KainBase()
        {
            if(_texKainBase!=null)return _texKainBase;
            _texKainBase=BacaReadableKarakter(FolderTex+"/kain.png");
            if(_texKainBase==null)_texKainBase=BacaReadableKarakter("Assets/ThirdParty/CC0/Kain/Albedo.jpg");
            return _texKainBase;
        }

        static Texture2D TeksturRambutSeram()
        {
            if(_texRambutSeram!=null)return _texRambutSeram;
            string p=FolderKarakter+"/RambutSeram.png";
            if(!File.Exists(p))
            {
                const int sz=512;
                var px=new Color[sz*sz];
                for(int y=0;y<sz;y++)for(int x=0;x<sz;x++)
                {
                    float u=(float)x/sz,v=(float)y/sz;
                    float helai=Mathf.PerlinNoise(u*115f+1.7f,v*6.5f+0.3f)*0.65f+Mathf.PerlinNoise(u*240f+7.1f,v*13f+2.2f)*0.35f;
                    float tipEdge=Mathf.Clamp01((v-0.04f)/0.38f);
                    float edgeX=Mathf.SmoothStep(0f,0.12f,u)*Mathf.SmoothStep(0f,0.12f,1f-u);
                    float alpha=Mathf.Clamp01((helai*1.35f-(1f-tipEdge)*0.68f)*edgeX*1.65f);
                    float shine=0.72f+0.28f*helai;
                    px[y*sz+x]=new Color(0.06f*shine,0.055f*shine,0.06f*shine,alpha);
                }
                var t=new Texture2D(sz,sz,TextureFormat.RGBA32,true);t.SetPixels(px);t.Apply();
                SimpanPng(t,p);AssetDatabase.ImportAsset(p);AturImpor(p,false,false,512);
                var imp=(TextureImporter)AssetImporter.GetAtPath(p);imp.alphaIsTransparency=true;imp.mipMapsPreserveCoverage=true;imp.SaveAndReimport();
            }
            _texRambutSeram=AssetDatabase.LoadAssetAtPath<Texture2D>(p);return _texRambutSeram;
        }

        static Texture2D KulitHororTex(J j,string skinRel)
        {
            string src="Assets/ThirdParty/Characters/System/skins/"+skinRel+".png";
            var source=BacaReadableKarakter(src);if(source==null)return null;
            bool hantu=j==J.Pocong || j==J.Kuntilanak || j==J.Genderuwo || j==J.Kuyang || j==J.Tuyul || j==J.HantuAir || j==J.Penyerupa;
            if(j==J.Ningsih)return KulitCantikNingsih(source);
            if(j==J.Deden)return KulitPenyintasDeden(source);
            if(!hantu)return source;
            int w=source.width,h=source.height;var px=source.GetPixels();
            for(int i=0;i<px.Length;i++)
            {
                float u=(float)(i%w)/w,v=(float)(i/w)/h;
                Color c=px[i];float lum=c.grayscale;
                float n=Mathf.PerlinNoise(u*24f+5f,v*24f+9f);
                float veins=Mathf.Pow(1f-Mathf.Abs(Mathf.PerlinNoise(u*38f,v*38f)-0.5f)*2f,5f);
                if(j==J.Genderuwo)
                {
                    Color baseBeast=new Color(0.22f,0.15f,0.12f)*Mathf.Lerp(0.45f,1.35f,lum);
                    px[i]=Color.Lerp(baseBeast,new Color(0.11f,0.08f,0.07f),n*0.45f);
                }
                else if(j==J.Tuyul)
                {
                    Color imp=new Color(0.26f,0.28f,0.22f)*Mathf.Lerp(0.4f,1.15f,lum);
                    px[i]=Color.Lerp(imp,new Color(0.11f,0.13f,0.10f),veins*0.55f+Mathf.Max(0f,n-0.45f)*0.55f);
                }
                else if(j==J.HantuAir)
                {
                    Color drowned=new Color(0.40f,0.54f,0.51f)*Mathf.Lerp(0.4f,1.2f,lum);
                    px[i]=Color.Lerp(drowned,new Color(0.14f,0.22f,0.20f),veins*0.65f+Mathf.Max(0f,n-0.45f)*0.55f);
                }
                else
                {
                    Color corpse=j==J.Pocong?new Color(0.56f,0.61f,0.52f):j==J.Penyerupa?new Color(0.75f,0.77f,0.78f):new Color(0.68f,0.72f,0.71f);
                    Color col=corpse*Mathf.Lerp(0.38f,1.22f,lum);
                    col=Color.Lerp(col,new Color(0.22f,0.25f,0.28f),veins*0.55f+Mathf.Max(0f,n-0.55f)*0.6f);
                    float darah=Mathf.Clamp01((Mathf.PerlinNoise(u*16f+12f,v*16f+3f)-0.52f)*3.5f);
                    if((j==J.Kuyang && v<0.55f) || (v>0.46f && v<0.62f && u>0.36f && u<0.64f))
                        col=Color.Lerp(col,new Color(0.24f,0.02f,0.02f),j==J.Kuyang?0.85f:darah*0.75f);
                    px[i]=col;
                }
                px[i].a=1f;
            }
            var tex=new Texture2D(w,h,TextureFormat.RGBA32,true);tex.SetPixels(px);tex.Apply();
            string path=FolderKarakter+"/Kulit_"+j+".png";SimpanPng(tex,path);AssetDatabase.ImportAsset(path);AturImpor(path,false,false,1024);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Texture2D KulitCantikNingsih(Texture2D source)
        {
            int w=source.width,h=source.height;var px=source.GetPixels();
            var orig=(Color[])px.Clone();
            const float vMid=0.4835f;
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)
            {
                float u=(float)x/w,v=(float)y/h;
                int idx=y*w+x;
                Color c=orig[idx];
                if(u>0.74f && u<0.97f && v>0.36f && v<0.61f)
                {
                    // Haluskan noda wajah halus sembari memberi rona hangat kulit sehat Asia.
                    Color blur=(orig[Mathf.Clamp(y+2,0,h-1)*w+x]+orig[Mathf.Clamp(y-2,0,h-1)*w+x]+orig[y*w+Mathf.Clamp(x+2,0,w-1)]+orig[y*w+Mathf.Clamp(x-2,0,w-1)])*0.25f;
                    c=Color.Lerp(c,blur,0.45f);
                }
                c=new Color(Mathf.Min(1f,c.r*1.06f+0.01f),Mathf.Min(1f,c.g*1.01f+0.005f),Mathf.Min(1f,c.b*0.97f),1f);
                if(u>0.76f && u<0.96f && v>0.39f && v<0.58f)
                {
                    float dv=Mathf.Abs(v-vMid); // simetri kiri-kanan wajah terhadap v=0.4835
                    float sv=vMid+dv;
                    // Alis tipis melengkung anggun (v: 0.496 -> 0.520 -> 0.547, u: 0.817 -> 0.813 -> 0.821).
                    if(sv>0.492f && sv<0.550f && u>0.805f && u<0.828f)
                    {
                        float tB=Mathf.InverseLerp(0.492f,0.548f,sv);
                        float uB=Mathf.Lerp(0.817f,0.821f,tB)-0.0042f*Mathf.Sin(tB*Mathf.PI);
                        float duB=Mathf.Abs(u-uB)/(0.0038f*(1f-0.38f*tB*tB));
                        float browMask=Mathf.Exp(-duB*duB*2.6f)*Mathf.SmoothStep(0.492f,0.498f,sv)*Mathf.SmoothStep(0.550f,0.538f,sv);
                        c=Color.Lerp(c,new Color(0.16f,0.10f,0.07f),browMask*0.62f);
                    }
                    // Kelopak hangat & eyeliner tegas natural di garis bulu mata atas (u ~ 0.834).
                    if(sv>0.502f && sv<0.542f && u>0.822f && u<0.844f)
                    {
                        float tE=Mathf.InverseLerp(0.504f,0.538f,sv);
                        float uLine=0.8345f-0.0015f*Mathf.Sin(tE*Mathf.PI);
                        float dL=(u-uLine)/0.0028f;
                        float liner=Mathf.Exp(-dL*dL*2.8f)*Mathf.SmoothStep(0.502f,0.508f,sv)*Mathf.SmoothStep(0.542f,0.531f,sv);
                        float dSh=(u-(uLine-0.005f))/0.007f;
                        float shadow=Mathf.Exp(-dSh*dSh*2.2f)*Mathf.SmoothStep(0.502f,0.512f,sv)*Mathf.SmoothStep(0.540f,0.528f,sv);
                        c=Color.Lerp(c,new Color(0.62f,0.40f,0.34f),shadow*0.22f);
                        c=Color.Lerp(c,new Color(0.12f,0.08f,0.07f),liner*0.68f);
                    }
                    // Rona pipi sehat peach-rose halus (pusat u=0.875, dv=0.062).
                    float dxC=(u-0.875f)/0.020f,dyC=(dv-0.062f)/0.024f;
                    float blush=Mathf.Exp(-(dxC*dxC+dyC*dyC)*1.8f);
                    c=Color.Lerp(c,new Color(0.88f,0.50f,0.46f),blush*0.18f);
                    // Bibir lembut merona rose (pusat u=0.911, v=0.4835).
                    float lFrac=Mathf.Clamp01(dv/0.022f);
                    float uTop=Mathf.Lerp(0.901f,0.911f,lFrac*lFrac);
                    float uBot=Mathf.Lerp(0.923f,0.912f,lFrac*lFrac);
                    if(u>=uTop-0.004f && u<=uBot+0.004f && dv<0.023f)
                    {
                        float midL=0.5f*(uTop+uBot),halfL=Mathf.Max(0.002f,0.5f*(uBot-uTop));
                        float env=Mathf.Clamp01(1f-Mathf.Abs(u-midL)/halfL)*Mathf.SmoothStep(0.023f,0.014f,dv);
                        Color lipCol=new Color(0.76f,0.31f,0.35f)*(1f+0.10f*Mathf.Clamp01(1f-dv/0.010f));
                        c=Color.Lerp(c,lipCol,env*0.54f);
                    }
                }
                c.a=1f;px[idx]=c;
            }
            var tex=new Texture2D(w,h,TextureFormat.RGBA32,true);tex.SetPixels(px);tex.Apply();
            string path=FolderKarakter+"/Kulit_Ningsih.png";SimpanPng(tex,path);AssetDatabase.ImportAsset(path);AturImpor(path,false,false,2048);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Texture2D KulitPenyintasDeden(Texture2D source)
        {
            int w=source.width,h=source.height;var px=source.GetPixels();
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)
            {
                float u=(float)x/w,v=(float)y/h;
                int idx=y*w+x;Color c=px[idx];
                if(u>0.88f && u<0.96f && v>0.41f && v<0.55f)
                {
                    // Janggut tipis & kotoran penyintas heli di rahang & pipi (gaya Kelvin SOTF).
                    float st=Mathf.PerlinNoise(u*220f,v*220f);
                    if(st>0.48f)c=Color.Lerp(c,new Color(0.20f,0.16f,0.13f),(st-0.48f)*0.45f);
                }
                float grime=Mathf.Clamp01((Mathf.PerlinNoise(u*28f+4f,v*28f+9f)-0.56f)*2.4f);
                c=Color.Lerp(c,new Color(0.32f,0.24f,0.18f),grime*0.22f);
                c.a=1f;px[idx]=c;
            }
            var tex=new Texture2D(w,h,TextureFormat.RGBA32,true);tex.SetPixels(px);tex.Apply();
            string path=FolderKarakter+"/Kulit_Deden.png";SimpanPng(tex,path);AssetDatabase.ImportAsset(path);AturImpor(path,false,false,1024);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Material[] BahanMakeHuman(J j)
        {
            Material[] hasil;if(_bahanReal.TryGetValue(j,out hasil))return hasil;
            bool wanita=j==J.Ningsih || j==J.Kuntilanak || j==J.AbdiWanita || j==J.Kuyang || j==J.Penyerupa;
            bool tua=j==J.KiRogo || j==J.Pocong || j==J.Genderuwo || j==J.AbdiButo || j==J.Tuyul;
            string skin=wanita?"young_asian_female/young_lightskinned_female_diffuse3":(j==J.Deden || j==J.AbdiSekte || j==J.PemimpinPatroli || j==J.HantuAir)?"middleage_asian_male/middleage_lightskinned_male_diffuse2":tua?"old_asian_male/old_lightskinned_male_diffuse2":"young_asian_male/young_lightskinned_male_diffuse3";
            string suit=(j==J.Ningsih || j==J.Kuntilanak || j==J.AbdiWanita || j==J.Kuyang || j==J.Penyerupa)?"female_elegantsuit01":(j==J.KiRogo || j==J.AbdiSekte || j==J.AbdiLumpur || j==J.PemimpinPatroli || j==J.HantuAir)?"male_casualsuit01":"male_worksuit01";
            string hair=wanita?"ponytail01":"short01";

            Color skinTint=j==J.Deden?new Color(0.95f,0.85f,0.76f):j==J.Ningsih?Color.white:j==J.KiRogo?new Color(0.89f,0.80f,0.72f):
                j==J.AbdiLumpur?new Color(0.58f,0.50f,0.42f):(j>=J.Pocong || j==J.Penyerupa)?Color.white:new Color(0.95f,0.86f,0.78f);
            float skinSmooth=j==J.Genderuwo?0.18f:(j==J.Pocong || j==J.Kuntilanak || j==J.Kuyang)?0.24f:j==J.Ningsih?0.22f:0.18f;
            var skinMat=SafeMaterialFactory.CreatePBR("M_Skin_"+j,skinTint,0f,skinSmooth,KulitHororTex(j,skin),NormalKulitWajah());
            SafeMaterialFactory.SetF(skinMat,"_BumpScale",j==J.Ningsih?0.08f:j>=J.Pocong?0.75f:0.22f);
            var sh=AmbilShaderAK("AlasKeramat/KulitManusia");if(sh!=null)skinMat.shader=sh;

            Texture2D clothNorm=null;
            Texture2D clothTex=PakaianBerwarna(j,suit,out clothNorm);
            if(clothNorm==null)clothNorm=TeksturKarakter("clothes/"+suit+"/"+suit+"_normal.png",true);
            float clothSmooth=j==J.Kuyang?0.84f:j==J.Ningsih?0.10f:0.11f;
            Color? clothEm=j==J.Kuyang?new Color(0.24f,0.03f,0.02f):(Color?)null;
            SurfaceMode clothMode=j==J.Kuntilanak?SurfaceMode.Cutout:SurfaceMode.Opaque;
            var cloth=SafeMaterialFactory.CreatePBR("M_Pakaian_"+j,Color.white,0f,clothSmooth,clothTex,clothNorm,clothEm,clothMode,0.42f);
            if(j==J.Pocong)SafeMaterialFactory.SetF(cloth,"_BumpScale",1.35f);
            if(j==J.Kuntilanak || j==J.Kuyang)SafeMaterialFactory.SetF(cloth,"_Cull",0f);

            var boots=BuatMaterialSub2(j);
            var rambut=BuatMaterialRambut(j,hair);
            var eye=BuatMaterialMata(j);
            hasil=new[]{SimpanMat(skinMat),SimpanMat(cloth),SimpanMat(boots),SimpanMat(rambut),SimpanMat(eye)};
            _bahanReal[j]=hasil;return hasil;
        }

        static Material BuatMaterialSub2(J j)
        {
            if(j==J.Pocong)
                return SafeMaterialFactory.CreatePBR("M_Tali_"+j,ProcTex.Hex("#3A3024"),0f,0.15f,AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex+"/kain.png"),AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex+"/kulit_kayu_n.png"));
            if(j==J.Genderuwo || j==J.Kuyang)
                return SafeMaterialFactory.CreatePBR("M_Tanduk_"+j,ProcTex.Hex("#82705A"),0.05f,0.38f,AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex+"/tanah.png"),AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex+"/tanah_n.png"));
            if(j==J.Kuntilanak || j==J.Tuyul || j==J.HantuAir)
                return SafeMaterialFactory.CreatePBR("M_Kaki_"+j,j==J.Tuyul?ProcTex.Hex("#30382C"):ProcTex.Hex("#6C706E"),0f,0.25f,AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex+"/tanah.png"));
            if(j>=J.AbdiSekte && j<=J.PemimpinPatroli)
            {
                var shoeSrc=BacaReadableKarakter("Assets/ThirdParty/Characters/System/clothes/shoes01/shoes01_diffuse.png");
                int w=shoeSrc!=null?shoeSrc.width:512,h=shoeSrc!=null?shoeSrc.height:512;
                var tex=new Texture2D(w,h,TextureFormat.RGBA32,true);var px=new Color[w*h];
                Color catTopeng=j==J.PemimpinPatroli?ProcTex.Hex("#781A14"):j==J.AbdiButo?ProcTex.Hex("#3A241A"):ProcTex.Hex("#8B7558");
                for(int y=0;y<h;y++)for(int x=0;x<w;x++)
                {
                    float u=(float)x/w,v=(float)y/h;
                    if(u<0.64f)px[y*w+x]=shoeSrc!=null?shoeSrc.GetPixelBilinear(u/0.62f,v):new Color(0.18f,0.15f,0.12f);
                    else
                    {
                        float ut=(u-0.66f)/0.31f;
                        float grain=0.82f+0.25f*Mathf.PerlinNoise(ut*14f,v*42f);
                        Color col=catTopeng*grain;
                        // Alis & ukiran dahi keramat merah-putih.
                        if(v>0.74f && v<0.82f)col=Color.Lerp(col,new Color(0.55f,0.1f,0.08f),0.75f);
                        // Rongga mata hitam pekat & garis taring putih.
                        float dMata=Mathf.Min(Vector2.Distance(new Vector2(ut,v),new Vector2(0.32f,0.62f)),Vector2.Distance(new Vector2(ut,v),new Vector2(0.68f,0.62f)));
                        if(dMata<0.12f)col*=0.2f;
                        if(v>0.18f && v<0.27f && ut>0.25f && ut<0.75f)col=Mathf.Repeat(ut*14f,1f)<0.45f?new Color(0.82f,0.77f,0.65f):new Color(0.12f,0.05f,0.04f);
                        col.a=1f;px[y*w+x]=col;
                    }
                }
                tex.SetPixels(px);tex.Apply();string path=FolderKarakter+"/SepatuTopeng_"+j+".png";
                SimpanPng(tex,path);AssetDatabase.ImportAsset(path);AturImpor(path,false,false,1024);
                return SafeMaterialFactory.CreatePBR("M_Boots_"+j,Color.white,0f,0.28f,AssetDatabase.LoadAssetAtPath<Texture2D>(path),AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex+"/kayu_n.png"));
            }
            return SafeMaterialFactory.CreatePBR("M_Boots_"+j,Color.white,0f,0.24f,TeksturKarakter("clothes/shoes01/shoes01_diffuse.png"),TeksturKarakter("clothes/shoes01/shoes01_normal.png",true));
        }

        static Material BuatMaterialRambut(J j,string hair)
        {
            if(j==J.Kuntilanak || j==J.Genderuwo || j==J.Kuyang || j==J.HantuAir)
            {
                Color c=j==J.HantuAir?new Color(0.24f,0.38f,0.26f):j==J.Kuntilanak || j==J.Kuyang?new Color(0.88f,0.90f,0.94f):new Color(0.75f,0.68f,0.62f);
                var m=SafeMaterialFactory.CreatePBR("M_Rambut_"+j,c,0f,j==J.HantuAir?0.62f:0.28f,TeksturRambutSeram(),null,null,SurfaceMode.Cutout,0.24f);
                SafeMaterialFactory.SetF(m,"_Cull",0f);return m;
            }
            if(j==J.Pocong || j==J.Tuyul)
                return SafeMaterialFactory.CreatePBR("M_Rambut_"+j,j==J.Tuyul?ProcTex.Hex("#364032"):ProcTex.Hex("#8D8472"),0f,0.15f,AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex+"/kain.png"),null,null,SurfaceMode.Opaque);
            Texture2D htex=TeksturKarakter("hair/"+hair+"/"+hair+"_diffuse.png");
            Texture2D hairNorm=null;
            if(j==J.Ningsih)
            {
                string pHn=FolderKarakter+"/Rambut_Ningsih_n.png";
                if(!File.Exists(pHn))
                {
                    const int nh=512;
                    float[] hh=ProcTex.HeightMap(nh,(u,v)=>Mathf.PerlinNoise(u*160f,v*18f)*0.65f+Mathf.PerlinNoise(u*320f,v*34f)*0.35f);
                    SimpanPng(ProcTex.NormalFromHeight("Rambut_Ningsih_n",nh,hh,1.2f),pHn);
                    AssetDatabase.ImportAsset(pHn);AturImpor(pHn,true,false,nh);
                }
                hairNorm=AssetDatabase.LoadAssetAtPath<Texture2D>(pHn);
                const int w=1024,h=1024;var px=new Color[w*h];
                for(int y=0;y<h;y++)for(int x=0;x<w;x++)
                {
                    float u=(float)x/w,v=(float)y/h;
                    if(u>=0.88f && v>=0.50f)
                    {
                        float pet=0.92f+0.08f*Mathf.PerlinNoise(u*64f,v*64f);
                        px[y*w+x]=new Color(0.98f*pet,0.96f*pet,0.90f*pet,1f);
                    }
                    else if(u>=0.88f)
                    {
                        px[y*w+x]=new Color(0.06f,0.04f,0.04f,1f);
                    }
                    else
                    {
                        float s1=Mathf.PerlinNoise(u*175f,v*5.5f);
                        float s2=Mathf.PerlinNoise(u*360f+7.3f,v*11f+1.7f);
                        float strand=s1*0.64f+s2*0.36f;
                        float gloss=Mathf.Pow(Mathf.Clamp01(1f-Mathf.Abs(v-0.58f)*3.4f),2f)*0.22f;
                        Color baseH=Color.Lerp(new Color(0.08f,0.055f,0.045f),new Color(0.24f,0.15f,0.105f),strand+gloss);
                        float root=Mathf.SmoothStep(0.96f,0.75f,v);
                        baseH*=Mathf.Lerp(0.68f,1.08f,root);
                        float tip=Mathf.Clamp01((v-0.05f)*4.2f+(strand-0.46f)*0.85f);
                        float edgeU=Mathf.SmoothStep(0.02f,0.10f,u)*Mathf.SmoothStep(0.86f,0.76f,u);
                        baseH.a=Mathf.Clamp01(tip*edgeU*(0.72f+strand*0.55f));
                        px[y*w+x]=baseH;
                    }
                }
                var t=new Texture2D(w,h,TextureFormat.RGBA32,true);t.SetPixels(px);t.Apply();
                string pn=FolderKarakter+"/Rambut_Ningsih.png";SimpanPng(t,pn);AssetDatabase.ImportAsset(pn);AturImpor(pn,false,false,1024);
                var imp=(TextureImporter)AssetImporter.GetAtPath(pn);imp.alphaIsTransparency=true;imp.mipMapsPreserveCoverage=true;imp.SaveAndReimport();
                htex=AssetDatabase.LoadAssetAtPath<Texture2D>(pn);
            }
            Color tint=j==J.KiRogo?new Color(0.95f,0.94f,0.92f):j==J.Ningsih?Color.white:new Color(0.86f,0.82f,0.79f);
            float cutoff=j==J.Ningsih?0.26f:0.35f;
            var mr=SafeMaterialFactory.CreatePBR("M_Rambut_"+j,tint,0f,j==J.Ningsih?0.34f:0.26f,htex,hairNorm,null,SurfaceMode.Cutout,cutoff);
            SafeMaterialFactory.SetF(mr,"_Cull",0f);return mr;
        }

        static Material BuatMaterialMata(J j)
        {
            var tex=TeksturKarakter("eyes/materials/brown_eye.png");
            if(j==J.Pocong)
                return SafeMaterialFactory.CreatePBR("M_Mata_"+j,new Color(0.9f,0.95f,0.8f),0f,0.9f,tex,null,new Color(0.62f,0.72f,0.52f));
            if(j==J.Kuntilanak || j==J.Kuyang)
                return SafeMaterialFactory.CreatePBR("M_Mata_"+j,new Color(1f,0.75f,0.7f),0f,0.9f,tex,null,new Color(1.25f,0.20f,0.14f));
            if(j==J.Genderuwo)
                return SafeMaterialFactory.CreatePBR("M_Mata_"+j,new Color(1f,0.4f,0.2f),0f,0.9f,tex,null,new Color(1.9f,0.34f,0.05f));
            if(j==J.Tuyul || j==J.Penyerupa)
                return SafeMaterialFactory.CreatePBR("M_Mata_"+j,new Color(0.04f,0.04f,0.05f),0f,0.96f,tex);
            if(j==J.HantuAir)
                return SafeMaterialFactory.CreatePBR("M_Mata_"+j,new Color(0.75f,0.95f,0.92f),0f,0.92f,tex,null,new Color(0.28f,0.85f,0.78f));
            if(j==J.Ningsih)
                return SafeMaterialFactory.CreatePBR("M_Mata_"+j,new Color(1f,0.96f,0.94f),0f,0.95f,tex,null,new Color(0.06f,0.05f,0.045f));
            return SafeMaterialFactory.CreatePBR("M_Mata_"+j,Color.white,0f,0.88f,tex);
        }

        static Texture2D PakaianBerwarna(J j,string suit,out Texture2D normalOut)
        {
            normalOut=null;
            string src="Assets/ThirdParty/Characters/System/clothes/"+suit+"/"+suit+"_diffuse.png";
            var source=BacaReadableKarakter(src);
            if(source==null)return AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex+"/kain.png");
            var aoSrc=BacaReadableKarakter("Assets/ThirdParty/Characters/System/clothes/"+suit+"/"+suit+"_ao.png");
            int w=source.width,h=source.height;
            var px=new Color[w*h];

            if(j==J.Kuyang)
            {
                // Tekstur organ dalam basah berdarah (paru, jantung, trakea, usus mengilap!).
                for(int y=0;y<h;y++)for(int x=0;x<w;x++)
                {
                    float u=(float)x/w,v=(float)y/h;
                    float vein=Mathf.Pow(1f-Mathf.Abs(Mathf.PerlinNoise(u*26f,v*26f)-0.5f)*2f,4f);
                    Color flesh=u<0.45f?new Color(0.45f,0.08f,0.08f):new Color(0.56f,0.14f,0.12f);
                    Color c=Color.Lerp(flesh,new Color(0.18f,0.015f,0.02f),vein*0.7f+Mathf.PerlinNoise(u*14f,v*14f)*0.3f);
                    c.a=1f;px[y*w+x]=c;
                }
                normalOut=AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex+"/lumpur_n.png");
                var tKu=new Texture2D(w,h,TextureFormat.RGBA32,true);tKu.SetPixels(px);tKu.Apply();
                string pKu=FolderKarakter+"/Pakaian_"+j+".png";SimpanPng(tKu,pKu);AssetDatabase.ImportAsset(pKu);AturImpor(pKu,false,false,1024);
                return AssetDatabase.LoadAssetAtPath<Texture2D>(pKu);
            }

            if(j==J.Ningsih || j==J.Penyerupa)
            {
                var kain=KainBase();
                var kNorm=AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex+"/kain_n.png");
                for(int y=0;y<h;y++)for(int x=0;x<w;x++)
                {
                    float u=(float)x/w,v=(float)y/h;
                    bool skirt=u>=0.50f;
                    float ou=skirt?Mathf.Clamp01((u-0.52f)/0.47f):Mathf.Clamp01((u-0.01f)/0.47f);
                    float lum=kain!=null?kain.GetPixelBilinear(Mathf.Repeat(ou*6f,1f),Mathf.Repeat(v*6f,1f)).grayscale:0.7f;
                    float foldAo=skirt?(0.84f+0.16f*Mathf.Sin(ou*20f+Mathf.PerlinNoise(ou*6f,v*6f)*2.2f)):(0.84f+0.16f*Mathf.SmoothStep(0.05f,0.38f,v));
                    float grain=0.92f+0.08f*Mathf.PerlinNoise(u*340f,v*340f);
                    if(!skirt)
                    {
                        // Kebaya renda merah-delima elegan dengan lis sulam emas di garis kerah sweetheart & sabuk pinggang.
                        float lace=(Mathf.Sin(ou*165f)*Mathf.Sin(v*165f))*0.045f;
                        float brokat=Mathf.PerlinNoise(ou*96f,v*96f)*0.12f;
                        Color ruby=j==J.Penyerupa?new Color(0.42f,0.38f,0.38f):new Color(0.56f,0.11f,0.16f);
                        Color gold=j==J.Penyerupa?new Color(0.48f,0.46f,0.44f):new Color(0.84f,0.64f,0.26f);
                        float collarTrim=v>0.82f?Mathf.Clamp01((v-0.82f)*7f):0f;
                        float beltGold=v<0.17f?Mathf.Clamp01((0.17f-v)*7f):0f;
                        float fil=0.5f+0.5f*Mathf.Sin(ou*220f)*Mathf.Cos(v*180f);
                        Color baseCloth=ruby*(1f-brokat+lace)*Mathf.Lerp(0.90f,1.06f,lum);
                        Color trimCol=Color.Lerp(ruby*0.75f,gold,Mathf.Clamp01(0.45f+0.55f*fil));
                        Color col=Color.Lerp(baseCloth,trimCol,Mathf.Max(collarTrim,beltGold*0.85f))*foldAo*grain;
                        col.a=1f;px[y*w+x]=col;
                    }
                    else
                    {
                        // Batik Parang Rusak mewah (cokelat-eboni gelap, emas hangat & aksen merah delima).
                        float wave=Mathf.Sin((ou*24f+v*20f)*Mathf.PI*2f+Mathf.Sin(ou*48f)*0.9f)*0.5f+0.5f;
                        float isen=Mathf.Clamp01(1f-Mathf.Abs(Mathf.Repeat(ou*38f-v*34f,1f)-0.5f)*4.0f);
                        float pat=Mathf.Clamp01(wave*0.64f+isen*0.36f);
                        Color eboni=new Color(0.09f,0.055f,0.045f),emas=new Color(0.78f,0.56f,0.22f),delima=new Color(0.42f,0.10f,0.14f);
                        Color motif=pat<0.55f?Color.Lerp(eboni,delima,pat/0.55f):Color.Lerp(delima,emas,(pat-0.55f)/0.45f);
                        Color col=motif*Mathf.Lerp(0.92f,1.05f,lum)*foldAo*grain;
                        if(j==J.Penyerupa)col=Color.Lerp(col,new Color(0.2f,0.22f,0.24f),0.45f);
                        col.a=1f;px[y*w+x]=col;
                    }
                }
                normalOut=kNorm;
                var texN=new Texture2D(w,h,TextureFormat.RGBA32,true);texN.SetPixels(px);texN.Apply();
                string pN=FolderKarakter+"/Pakaian_"+j+".png";SimpanPng(texN,pN);AssetDatabase.ImportAsset(pN);AturImpor(pN,false,false,2048);
                return AssetDatabase.LoadAssetAtPath<Texture2D>(pN);
            }

            if(j==J.Pocong)
            {
                var kain=KainBase();
                for(int y=0;y<h;y++)for(int x=0;x<w;x++)
                {
                    float u=(float)x/w,v=(float)y/h;
                    Color k=kain!=null?kain.GetPixelBilinear(u*5f,v*7f):new Color(0.65f,0.64f,0.58f);
                    float foldShade=0.66f+0.34f*Mathf.Sin(u*28f+Mathf.PerlinNoise(u*10f,v*10f)*3.0f);
                    Color baseShroud=new Color(0.74f,0.74f,0.70f)*Mathf.Lerp(0.45f,1.08f,k.grayscale)*foldShade;
                    float mold=Mathf.PerlinNoise(u*14f+3.1f,v*18f+7.4f);
                    float mud=Mathf.Clamp01((0.42f-v)*2.6f+mold*0.42f-0.16f);
                    baseShroud=Color.Lerp(baseShroud,new Color(0.15f,0.13f,0.11f),mud*0.80f);
                    float bloodNoise=Mathf.PerlinNoise(u*22f+8f,v*12f+1.5f);
                    float bloodBand=v>0.54f && v<0.86f?Mathf.Clamp01((bloodNoise-0.50f)*3.6f):0f;
                    baseShroud=Color.Lerp(baseShroud,new Color(0.22f,0.025f,0.02f),bloodBand*0.85f);
                    baseShroud.a=1f;px[y*w+x]=baseShroud;
                }
                normalOut=AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex+"/kain_n.png");
                var texP=new Texture2D(w,h,TextureFormat.RGBA32,true);texP.SetPixels(px);texP.Apply();
                string pP=FolderKarakter+"/Pakaian_"+j+".png";SimpanPng(texP,pP);AssetDatabase.ImportAsset(pP);AturImpor(pP,false,false,1024);
                return AssetDatabase.LoadAssetAtPath<Texture2D>(pP);
            }

            if(j==J.Kuntilanak)
            {
                for(int y=0;y<h;y++)for(int x=0;x<w;x++)
                {
                    float u=(float)x/w,v=(float)y/h;
                    float lum=source.GetPixelBilinear(u,v).grayscale;
                    Color col=new Color(0.72f,0.72f,0.68f)*Mathf.Lerp(0.38f,1.12f,lum);
                    float dirt=Mathf.PerlinNoise(u*18f+2f,v*14f+5f);
                    col=Color.Lerp(col,new Color(0.24f,0.22f,0.18f),Mathf.Clamp01(dirt-0.38f)*0.85f);
                    float darah=Mathf.PerlinNoise(u*24f+11f,v*10f+4f);
                    float darahArea=(v<0.28f || v>0.52f)?1f:0.35f;
                    col=Color.Lerp(col,new Color(0.24f,0.02f,0.02f),Mathf.Clamp01((darah-0.50f)*3.4f)*darahArea);
                    float fray=Mathf.PerlinNoise(u*28f+4f,v*8f)*0.18f+Mathf.PerlinNoise(u*64f,v*16f)*0.08f;
                    col.a=u>=0.48f?Mathf.Clamp01((v-0.06f+fray-0.14f)*7f):1f;
                    px[y*w+x]=col;
                }
                normalOut=AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex+"/kain_n.png");
                var texK=new Texture2D(w,h,TextureFormat.RGBA32,true);texK.SetPixels(px);texK.Apply();
                string pK=FolderKarakter+"/Pakaian_"+j+".png";SimpanPng(texK,pK);AssetDatabase.ImportAsset(pK);AturImpor(pK,false,false,1024);
                var impK=(TextureImporter)AssetImporter.GetAtPath(pK);impK.alphaIsTransparency=true;impK.mipMapsPreserveCoverage=true;impK.SaveAndReimport();
                return AssetDatabase.LoadAssetAtPath<Texture2D>(pK);
            }

            Color tone=j==J.Deden?new Color(0.82f,0.36f,0.11f):j==J.KiRogo?new Color(0.14f,0.12f,0.11f):
                j==J.PemimpinPatroli?new Color(0.48f,0.12f,0.09f):j==J.AbdiLumpur?new Color(0.24f,0.20f,0.15f):
                j>=J.AbdiSekte?new Color(0.30f,0.26f,0.21f):new Color(0.38f,0.39f,0.26f);
            bool remapped=j==J.Deden || j==J.KiRogo;
            var nBase=(j==J.Deden || j==J.KiRogo)?BacaReadableKarakter("Assets/ThirdParty/Characters/System/clothes/"+suit+"/"+suit+"_normal.png"):null;
            var npix2=remapped?new Color[w*h]:null;
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)
            {
                float u=(float)x/w,v=(float)y/h;
                if(remapped && u>=0.89f)
                {
                    float grain=0.9f+0.15f*Mathf.PerlinNoise(u*160f,v*160f);
                    if(j==J.Deden && v>=0.50f)
                    {
                        // Perban kasa putih-krem dengan noda darah kecil.
                        float spot=Mathf.Clamp01((Mathf.PerlinNoise(u*60f,v*40f)-0.62f)*4f);
                        px[y*w+x]=Color.Lerp(new Color(0.82f,0.78f,0.70f),new Color(0.38f,0.06f,0.05f),spot*0.65f)*grain;
                    }
                    else if(j==J.Deden)
                    {
                        // Rompi taktis SAR kanvas zaitun-gelap bernoda lumpur hutan.
                        float weave=0.92f+0.08f*Mathf.Sin(u*320f)*Mathf.Sin(v*320f);
                        Color vest=new Color(0.22f,0.24f,0.19f)*weave*grain;
                        if(Mathf.Abs(v-0.25f)<0.04f)vest*=0.72f;
                        px[y*w+x]=vest;
                    }
                    else
                    {
                        // Blangkon KiRogo motif lurik/batik gelap.
                        float stripe=Mathf.Repeat((u-0.9f)*48f+v*18f,1f)<0.4f?0.75f:1.05f;
                        px[y*w+x]=new Color(0.28f,0.21f,0.14f)*stripe*grain;
                    }
                    px[y*w+x].a=1f;
                    if(npix2!=null)npix2[y*w+x]=new Color(0.5f,0.5f,1f,1f);
                    continue;
                }
                float ou=remapped?Mathf.Clamp01(u/0.88f):u;
                Color asli=source.GetPixelBilinear(ou,v);float lum=asli.grayscale;
                float ao=aoSrc!=null?(0.42f+0.58f*aoSrc.GetPixelBilinear(ou,v).grayscale):1f;
                float denim=Mathf.Clamp01((asli.b-asli.r)*6f);
                Color dyed=tone*Mathf.Lerp(0.20f,1.20f,lum);
                Color shirt=j==J.Deden?asli*new Color(0.86f,0.81f,0.74f):asli*0.92f;
                Color res=(j==J.Andi || j==J.Deden)?Color.Lerp(shirt,dyed,denim):dyed;
                res*=ao;
                float grime=Mathf.Clamp01((Mathf.PerlinNoise(ou*14f+2f,v*14f+6f)-0.48f)*2.4f);
                if(j>=J.Deden)res=Color.Lerp(res,new Color(0.18f,0.14f,0.09f),grime*0.38f);
                res.a=1f;px[y*w+x]=res;
                if(npix2!=null)npix2[y*w+x]=nBase!=null?nBase.GetPixelBilinear(ou,v):new Color(0.5f,0.5f,1f,1f);
            }
            if(npix2!=null)
            {
                var ntex2=new Texture2D(w,h,TextureFormat.RGBA32,true);ntex2.SetPixels(npix2);ntex2.Apply();
                string np2=FolderKarakter+"/Pakaian_"+j+"_n.png";SimpanPng(ntex2,np2);AssetDatabase.ImportAsset(np2);AturImpor(np2,true,false,2048);
                normalOut=AssetDatabase.LoadAssetAtPath<Texture2D>(np2);
            }
            var tex=new Texture2D(w,h,TextureFormat.RGBA32,true);tex.SetPixels(px);tex.Apply();
            string path=FolderKarakter+"/Pakaian_"+j+".png";SimpanPng(tex,path);AssetDatabase.ImportAsset(path);AturImpor(path,false,false,2048);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
