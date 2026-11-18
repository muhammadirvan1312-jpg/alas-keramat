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
            for(int y=0;y<size;y++)for(int x=0;x<size;x++) { float u=(x+0.5f)/size,v=(y+0.5f)/size;var c=a.GetPixelBilinear(u,v);c.a=mask.GetPixelBilinear(u,v).r;p[y*size+x]=c; }
            tex.SetPixels(p);tex.Apply();string path=FolderVeg+"/"+slot+".png";File.WriteAllBytes(path,tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);AturImpor(path,false,false,size);
            var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.alphaIsTransparency=true;imp.mipMapsPreserveCoverage=true;imp.alphaTestReferenceValue=0.4f;imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static Material MaterialDaunFoto(string slot)
        {
            var tex=DaunFoto(slot);if(tex==null)return null;
            return SimpanMat(KeAK(SafeMaterialFactory.CreatePBR("M_"+slot,Color.white,0f,0.3f,tex,null,null,SurfaceMode.Cutout,0.4f),ShaderDaun,0.025f,0.38f));
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
            var bark=SimpanMat(KeAK(SafeMaterialFactory.CreatePBR("M_KulitScan",Color.white,0f,0.2f,BacaFoto("KulitPohonAsli","Albedo")),ShaderKulit,0.02f,0f));
            var oldBark=AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex+"/kulit_kayu_n.png");SafeMaterialFactory.SetT(bark,"_BumpMap",oldBark);
            for(int i=0;i<data.jenis.Length;i++)
            {
                var j=data.jenis[i];
                if(j.nama.StartsWith("Mahoni_2") && tree0!=null && tree1!=null && leaf!=null)
                { j.lod0=tree0;j.lod1=tree1;j.kayu=bark;j.daun=leaf;j.jarakLod1=42f;JumlahMeshScan++; }
                if(j.nama.StartsWith("Pakis_") && fern0!=null && fern1!=null && fern!=null)
                { j.lod0=fern0;j.lod1=fern1;j.daun=fern;j.jarakLod1=18f;j.jarakMaks=65f;JumlahMeshScan++; }
            }
            Catat("G3b: "+JumlahMeshScan+" varian scan pohon/pakis tersambung ke instancing dan tebang, dengan LOD");
        }
    }
}
