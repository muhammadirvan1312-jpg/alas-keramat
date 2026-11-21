using System;
using System.IO;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
namespace AlasKeramat.EditorTools
{
    public static partial class NusantaraMasterBuilder
    {
        public const string FolderFoto="Assets/ThirdParty/CC0";
        public static int JumlahFotoPBR;
        static readonly string[,] PasanganFoto={
            {"tanah","TanahHutan"},{"rumput","LumutHutan"},{"batu","Batu"},{"kayu","Kayu"},
            {"kulit_kayu","KulitKayu"},{"pasir","Pasir"},{"lumpur","Lumpur"},{"daun","Serasah"},
            {"beton","Beton"},{"logam_karat","Karat"},{"palem","Palem"},{"kain","Kain"},{"batu_gua","BatuGua"},{"bilik","BilikBambu"}
        };
        static Texture2D BacaFoto(string slot,string peta)
        {
            string p=FolderFoto+"/"+slot+"/"+peta+".jpg";
            if(!File.Exists(p))p=FolderFoto+"/"+slot+"/"+peta+".png";
            if(!File.Exists(p))return null;
            AssetDatabase.ImportAsset(p);
            var imp=AssetImporter.GetAtPath(p) as TextureImporter;
            if(imp!=null && (!imp.isReadable || imp.textureType!=TextureImporterType.Default || imp.textureCompression!=TextureImporterCompression.Uncompressed))
            {
                imp.textureType=TextureImporterType.Default;imp.isReadable=true;imp.sRGBTexture=peta=="Albedo";imp.maxTextureSize=2048;imp.textureCompression=TextureImporterCompression.Uncompressed;
                var settings=imp.GetPlatformTextureSettings("Android");settings.overridden=true;settings.maxTextureSize=2048;settings.format=TextureImporterFormat.RGBA32;imp.SetPlatformTextureSettings(settings);imp.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(p);
        }
        static void PanggangFotoPBR()
        {
            JumlahFotoPBR=0;
            for(int i=0;i<PasanganFoto.GetLength(0);i++)
            {
                string nama=PasanganFoto[i,0],slot=PasanganFoto[i,1];
                var a=BacaFoto(slot,"Albedo");var n=BacaFoto(slot,"Normal");var arm=BacaFoto(slot,"ARM");
                if(a==null || n==null || arm==null) { Debug.LogWarning("[AK][CC0] Peta PBR belum lengkap: "+slot);continue; }
                if(nama=="rumput" || nama=="daun" || nama=="tanah" || nama=="batu")
                {
                    var apx=a.GetPixels();
                    for(int p=0;p<apx.Length;p++)
                    {
                        Color c=apx[p];
                        if(nama=="rumput") c=new Color(c.r*0.66f,c.g*0.72f,c.b*0.54f,1f);
                        else if(nama=="daun") c=new Color(c.r*0.62f,c.g*0.58f,c.b*0.48f,1f);
                        else if(nama=="batu") c=new Color(c.r*0.72f,c.g*0.73f,c.b*0.74f,1f);
                        else c=new Color(c.r*0.86f,c.g*0.80f,c.b*0.74f,1f);
                        apx[p]=c;
                    }
                    var graded=new Texture2D(a.width,a.height,TextureFormat.RGBA32,true);
                    graded.SetPixels(apx);graded.Apply();
                    File.WriteAllBytes(FolderTex+"/"+nama+".png",graded.EncodeToPNG());
                    UnityEngine.Object.DestroyImmediate(graded);
                }
                else File.WriteAllBytes(FolderTex+"/"+nama+".png",a.EncodeToPNG());
                File.WriteAllBytes(FolderTex+"/"+nama+"_n.png",n.EncodeToPNG());
                var px=arm.GetPixels();
                for(int p=0;p<px.Length;p++) { Color c=px[p];px[p]=new Color(c.b,c.r,0.5f,1f-c.g); }
                var mask=new Texture2D(arm.width,arm.height,TextureFormat.RGBA32,true,true);mask.SetPixels(px);mask.Apply();
                File.WriteAllBytes(FolderTex+"/"+nama+"_mask.png",mask.EncodeToPNG());UnityEngine.Object.DestroyImmediate(mask);
                JumlahFotoPBR++;
            }
            AssetDatabase.Refresh();
            for(int i=0;i<PasanganFoto.GetLength(0);i++)
            {
                string p=FolderTex+"/"+PasanganFoto[i,0];
                if(!File.Exists(p+"_mask.png"))continue;
                AturImpor(p+".png",false,false,2048);AturImpor(p+"_n.png",true,false,2048);AturImpor(p+"_mask.png",false,false,2048);
                var imp=(TextureImporter)AssetImporter.GetAtPath(p+"_mask.png");imp.sRGBTexture=false;imp.SaveAndReimport();
            }
            Catat("G3b: "+JumlahFotoPBR+" permukaan PBR foto 2K (albedo, normal GL, mask AO/roughness), CC0");
        }
        static void MaskPBR(Material m,string nama)
        {
            var mask=AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex+"/"+nama+"_mask.png");if(mask==null || m==null)return;
            SafeMaterialFactory.SetT(m,"_MetallicGlossMap",mask);SafeMaterialFactory.SetT(m,"_OcclusionMap",mask);
            SafeMaterialFactory.SetF(m,"_Smoothness",1f);SafeMaterialFactory.SetF(m,"_OcclusionStrength",0.55f);
            m.EnableKeyword("_METALLICSPECGLOSSMAP");m.EnableKeyword("_OCCLUSIONMAP");
        }
    }
}
