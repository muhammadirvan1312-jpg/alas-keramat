using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
namespace AlasKeramat.EditorTools
{
    public static partial class NusantaraMasterBuilder
    {
        static void BuatSerasahHutan()
        {
            const int sisiSel=32,calon=90;
            var batu=new Mesh { name="BatuSerasah" };
            batu.vertices=new[]{Vector3.up,Vector3.right,Vector3.forward,Vector3.left,Vector3.back,Vector3.down};
            batu.triangles=new[]{0,2,1,0,3,2,0,4,3,0,1,4,5,1,2,5,2,3,5,3,4,5,4,1};
            batu.uv=new[]{new Vector2(.5f,1),new Vector2(1,.5f),new Vector2(.75f,.5f),new Vector2(0,.5f),new Vector2(.25f,.5f),new Vector2(.5f,0)};
            batu.RecalculateNormals();batu.RecalculateTangents();batu.RecalculateBounds();AssetDatabase.CreateAsset(batu,FolderMesh+"/BatuSerasah.asset");
            var primitive=GameObject.CreatePrimitive(PrimitiveType.Cube);var ranting=Object.Instantiate(primitive.GetComponent<MeshFilter>().sharedMesh);Object.DestroyImmediate(primitive);
            ranting.name="RantingSerasah";AssetDatabase.CreateAsset(ranting,FolderMesh+"/RantingSerasah.asset");
            var bahan=new[]{AssetDatabase.LoadAssetAtPath<Material>(FolderMat+"/M_Batu.mat"),AssetDatabase.LoadAssetAtPath<Material>(FolderMat+"/M_Kayu.mat")};
            var meshes=new[]{batu,ranting};var hasil=new List<SerasahHutan.KelompokSerasah>();
            // Irisan selatan dekat heli/kemah; area lain diperluas sesudah evaluasi foto.
            for(int z=-560;z<-200;z+=sisiSel)for(int x=-192;x<320;x+=sisiSel)
            {
                var rand=new System.Random(x*73856093^z*19349663);var daftar=new[]{new List<Matrix4x4>(),new List<Matrix4x4>()};
                var bounds=new[]{new Bounds(),new Bounds()};
                for(int n=0;n<calon;n++)
                {
                    float px=x+(float)rand.NextDouble()*sisiSel,pz=z+(float)rand.NextDouble()*sisiSel,y;
                    if(!BolehTanam(px,pz,out y))continue;
                    int tipe=n%2;float ukuran=.09f+(float)rand.NextDouble()*.3f;
                    var pos=new Vector3(px,y,pz);
                    var scale=tipe==0?new Vector3(ukuran,ukuran*.4f,ukuran*.7f):new Vector3(.025f,.025f,.3f+(float)rand.NextDouble()*1.3f);
                    daftar[tipe].Add(Matrix4x4.TRS(pos,Quaternion.Euler(0,(float)rand.NextDouble()*360,0),scale));
                    if(daftar[tipe].Count==1)bounds[tipe]=new Bounds(pos,Vector3.one*4);else bounds[tipe].Encapsulate(new Bounds(pos,Vector3.one*4));
                }
                for(int t=0;t<2;t++)if(daftar[t].Count>0)hasil.Add(new SerasahHutan.KelompokSerasah { mesh=meshes[t],material=bahan[t],bounds=bounds[t],matriks=daftar[t].ToArray() });
            }
            var detail=Anak(_root,"SerasahHutan").AddComponent<SerasahHutan>();detail.kelompok=hasil.ToArray();EditorUtility.SetDirty(detail);
            Catat("Lantai hutan selatan: batu kecil & ranting instanced, radius 65m, tanpa mengubah simpanan pohon");
        }
    }
}
