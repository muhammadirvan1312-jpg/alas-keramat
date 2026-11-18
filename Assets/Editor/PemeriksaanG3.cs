using System;
using AlasKeramat.Core;
using AlasKeramat.Building;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
namespace AlasKeramat.EditorTools
{
    public static class PemeriksaanG3
    {
        public static void Jalankan(Action<string,bool> cek)
        {
            var serasah=Object.FindFirstObjectByType<SerasahHutan>();
            bool detail=serasah!=null && serasah.kelompok.Length>0;
            if(serasah!=null)foreach(var group in serasah.kelompok)
            {
                detail &= group.mesh!=null && group.material!=null && group.matriks.Length<=1023;
                foreach(var matrix in group.matriks)detail &= group.bounds.Contains((Vector3)matrix.GetColumn(3));
            }
            cek("Lantai hutan: batch dekorasi valid dan bounds meliputi posisi instance",detail);
            bool foto=true;
            foreach(string slot in new[]{"tanah","rumput","batu","kayu","kulit_kayu","pasir","lumpur","beton","logam_karat","kain"})
            {
                string p=NusantaraMasterBuilder.FolderTex+"/"+slot;
                var a=AssetDatabase.LoadAssetAtPath<Texture2D>(p+".png");var n=AssetImporter.GetAtPath(p+"_n.png") as TextureImporter;var mask=AssetImporter.GetAtPath(p+"_mask.png") as TextureImporter;
                foto &= a!=null && a.width>=2048 && n!=null && n.textureType==TextureImporterType.NormalMap && n.streamingMipmaps && mask!=null && !mask.sRGBTexture;
            }
            cek("G3b: 10 permukaan foto PBR 2K, normal GL, mask linear & mip streaming",foto);
            bool real=true;
            foreach(string nama in new[]{"Andi","Deden","Ningsih","KiRogo"})
            {
                var m=AssetDatabase.LoadAssetAtPath<Mesh>(NusantaraMasterBuilder.FolderKarakter+"/MakeHuman_"+nama+".asset");
                if(m==null) { real=false;continue; }
                real &= m.triangles.Length/3<=30000 && m.triangles.Length/3>12000 && m.bindposes.Length==24 && m.subMeshCount==5 && m.blendShapeCount>=13 && m.bounds.size.y>1.45f && m.bounds.size.y<1.9f;
                foreach(var w in m.boneWeights)
                {
                    float total=w.weight0+w.weight1+w.weight2+w.weight3;
                    real &= Mathf.Abs(total-1f)<0.002f && w.boneIndex0<24 && w.boneIndex1<24 && w.boneIndex2<24 && w.boneIndex3<24;
                }
            }
            cek("G3c: 4 MakeHuman berpakaian, <=30rb tris, 24 tulang, bobot normal, 13 bentuk wajah",real);
            var realScene=Object.FindObjectsByType<PenandaModelReal>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            bool pasang=realScene.Length>=4;
            foreach(var model in realScene)
            {
                pasang &= model.GetComponent<AlasKeramat.AI.FaceController>()!=null;
                foreach(var smr in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                { pasang &= smr.bones.Length==24 && smr.sharedMaterials.Length==5;foreach(var mat in smr.sharedMaterials)pasang &= mat!=null && mat.mainTexture!=null; }
            }
            cek("G3c: model nyata tersambung ke scene, material & kendali wajah",pasang);
            var data=AssetDatabase.LoadAssetAtPath<VegetationData>(NusantaraMasterBuilder.PathVegData);bool scan=false,pakis=false;
            if(data!=null)foreach(var j in data.jenis)
            {
                if(j.nama=="Mahoni_2")scan=j.lod0!=null && j.lod1!=null && j.lod0.name.Contains("PohonScan") && j.lod0.triangles.Length/3<=18000 && j.lod1.triangles.Length<j.lod0.triangles.Length*0.3f && j.jarakLod1<=45f;
                if(j.nama=="Pakis_0")pakis=j.lod0!=null && j.lod0.name.Contains("PakisScan") && j.lod1.triangles.Length<j.lod0.triangles.Length;
            }
            cek("G3b: pohon & pakis pemindaian tersambung ke instancing dengan LOD",scan && pakis);
            foreach(var sn in new[]{"AlasKeramat/KabutLokal","AlasKeramat/KulitManusia"})
            { var s=Shader.Find(sn);cek("G3 shader "+sn+" tanpa error",s!=null && !ShaderUtil.ShaderHasError(s)); }
            cek("G3a: kabut lokal tetap, basah cuaca, malam & gua terbaca",Object.FindFirstObjectByType<AtmosferHutan>()!=null && AtmosferHutan.BasahTarget(WeatherState.HujanBadai)==1f && AtmosferHutan.BasahTarget(WeatherState.Cerah)==0f && AKPascaProses.Target(22f,WeatherState.Cerah).x>0f && AKPascaProses.TargetGua.x>0f && WorldCaveAndSaveManager.AmbienGua.g>0.1f);
        }
    }
}
