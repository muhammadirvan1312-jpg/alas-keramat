using System.IO;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
namespace AlasKeramat.EditorTools
{
    public static partial class NusantaraMasterBuilder
    {
        public static int JumlahMeshScan;
        static Texture2D DaunFoto(string slot)
        {
            var a=BacaFoto(slot,"Albedo");var mask=BacaFoto(slot,"Alpha");if(a==null || mask==null)return null;
            int size=1024;var tex=new Texture2D(size,size,TextureFormat.RGBA32,true);
            var p=new Color[size*size];
            Color avg=new Color(0.22f,0.34f,0.16f,0f);float wSum=0f;
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float u=(x+0.5f)/size,v=(y+0.5f)/size;var c=a.GetPixelBilinear(u,v);c.r*=0.78f;c.g*=1.08f;c.b*=0.68f;float al=mask.GetPixelBilinear(u,v).r;
                if(al>0.5f) { avg+=c*al;wSum+=al; }
                c.a=al;p[y*size+x]=c;
            }
            if(wSum>1f)avg/=wSum;
            for(int i=0;i<p.Length;i++)if(p[i].a<0.15f)p[i]=new Color(avg.r,avg.g,avg.b,p[i].a);
            tex.SetPixels(p);tex.Apply();string path=FolderVeg+"/"+slot+".png";File.WriteAllBytes(path,tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);AturImpor(path,false,false,size);
            var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.alphaIsTransparency=true;imp.mipMapsPreserveCoverage=true;imp.alphaTestReferenceValue=0.4f;imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static Texture2D AtlasDaunCampuran()
        {
            var atlas=ProcPohon.AtlasDaun(1024);var foto=BacaFoto("DaunAsli","Albedo");var mask=BacaFoto("DaunAsli","Alpha");
            var pakisA=BacaFoto("PakisAsli","Albedo");var pakisM=BacaFoto("PakisAsli","Alpha");
            if(foto==null || mask==null)return atlas;
            int size=atlas.width,half=size/2;var px=atlas.GetPixels();
            Color bleed=new Color(0.22f,0.34f,0.16f,0f);
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                int cell=x/half+(y/half)*2;
                float u=(x%half+0.5f)/half,v=(y%half+0.5f)/half;
                if(cell==2)
                {
                    if(pakisA==null || pakisM==null)continue;
                    Color cp=pakisA.GetPixelBilinear(u,v);float ap=pakisM.GetPixelBilinear(u,v).r;
                    if(ap<0.15f)cp=bleed;
                    cp.a=ap;px[y*size+x]=cp;continue;
                }
                if(cell==1) { float old=u;u=v;v=1f-old; }
                Color c=foto.GetPixelBilinear(u,v);c.r*=0.78f;c.g*=1.08f;c.b*=0.68f;float alpha=mask.GetPixelBilinear(u,v).r;
                if(cell==0)c=Color.Lerp(c,new Color(c.r*1.06f,c.g*1.04f,c.b*0.88f),0.35f);
                else if(cell==1)c=Color.Lerp(c,new Color(c.r*0.88f,c.g*1.02f,c.b*0.86f),0.4f);
                else if(cell==3)c=new Color(c.grayscale*0.28f,c.grayscale*0.21f,c.grayscale*0.14f);
                if(alpha<0.15f)c=cell==3?new Color(0.22f,0.17f,0.11f,0f):bleed;
                c.a=alpha;px[y*size+x]=c;
            }
            atlas.SetPixels(px);atlas.Apply();return atlas;
        }
        static Material MaterialDaunFoto(string slot)
        {
            var tex=DaunFoto(slot);if(tex==null)return null;
            var norm=BacaFoto(slot,"Normal");
            if(norm!=null)
            {
                string pn=FolderVeg+"/"+slot+"_n.png";File.WriteAllBytes(pn,norm.EncodeToPNG());AssetDatabase.ImportAsset(pn);AturImpor(pn,true,false,1024);
                norm=AssetDatabase.LoadAssetAtPath<Texture2D>(pn);
            }
            return SimpanMat(KeAK(SafeMaterialFactory.CreatePBR("M_"+slot,Color.white,0f,0.14f,tex,norm,null,SurfaceMode.Cutout,0.38f),ShaderDaun,0.025f,0.45f));
        }
        static Mesh ScanMesh(string nama)
        {
            var m=ImporMeshVisual.Baca("Assets/ThirdParty/Prepared/"+nama+".bytes");if(m==null)return null;
            string path=FolderVeg+"/"+nama+".asset";AssetDatabase.CreateAsset(m,path);return m;
        }
        static void PasangMeshScan(AlasKeramat.Building.VegetationData data)
        {
            JumlahMeshScan=0;
            var tree0=ScanMesh("PohonScan_LOD0");var tree1=ScanMesh("PohonScan_LOD1");
            var fern0=ScanMesh("PakisScan_LOD0");var fern1=ScanMesh("PakisScan_LOD1");
            var leaf=MaterialDaunFoto("DaunAsli");var fern=MaterialDaunFoto("PakisAsli");
            var barkN=BacaFoto("KulitPohonAsli","Normal");
            if(barkN!=null)
            {
                string pbn=FolderVeg+"/KulitPohonAsli_n.png";File.WriteAllBytes(pbn,barkN.EncodeToPNG());AssetDatabase.ImportAsset(pbn);AturImpor(pbn,true,false,2048);
                barkN=AssetDatabase.LoadAssetAtPath<Texture2D>(pbn);
            }
            if(barkN==null)barkN=AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex+"/kulit_kayu_n.png");
            var bark=SimpanMat(KeAK(SafeMaterialFactory.CreatePBR("M_KulitScan",Color.white,0f,0.22f,BacaFoto("KulitPohonAsli","Albedo"),barkN),ShaderKulit,0.02f,0f));
            for(int i=0;i<data.jenis.Length;i++)
            {
                var j=data.jenis[i];
                if((j.nama=="Mahoni_1" || j.nama=="Mahoni_2" || j.nama=="Jati_2") && tree0!=null && tree1!=null && leaf!=null)
                { j.lod0=tree0;j.lod1=tree1;j.kayu=bark;j.daun=leaf;j.jarakLod1=42f;JumlahMeshScan++; }
                if(j.nama.StartsWith("Pakis_") && fern0!=null && fern1!=null && fern!=null)
                { j.lod0=fern0;j.lod1=fern1;j.daun=fern;j.jarakLod1=18f;j.jarakMaks=65f;JumlahMeshScan++; }
            }
            Catat("G3b: "+JumlahMeshScan+" varian scan pohon/pakis tersambung ke instancing dan tebang, dengan LOD");
        }
    }
}
