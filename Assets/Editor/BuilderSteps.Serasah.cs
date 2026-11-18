using System.Collections.Generic;
using System.IO;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
namespace AlasKeramat.EditorTools
{
    public static partial class NusantaraMasterBuilder
    {
        static Mesh MeshRumputHutan()
        {
            var v=new List<Vector3>();var n=new List<Vector3>();var uv=new List<Vector2>();var c=new List<Color>();var t=new List<int>();
            var r=new System.Random(2026);
            for(int i=0;i<14;i++)
            {
                float a=i*(Mathf.PI*2f/14f)+(float)r.NextDouble()*0.3f;
                float ox=Mathf.Cos(a)*(0.05f+(float)r.NextDouble()*0.25f),oz=Mathf.Sin(a)*(0.05f+(float)r.NextDouble()*0.25f);
                float rot=a+(float)r.NextDouble()*0.6f,h=0.45f+(float)r.NextDouble()*0.45f,w=0.22f+(float)r.NextDouble()*0.14f;
                Vector3 side=new Vector3(Mathf.Cos(rot)*w,0f,Mathf.Sin(rot)*w);
                Vector3 lean=new Vector3(-Mathf.Sin(rot),0f,Mathf.Cos(rot))*(0.12f+(float)r.NextDouble()*0.16f);
                Vector3 p=new Vector3(ox,-0.04f,oz);int bi=v.Count;
                v.Add(p-side);v.Add(p+side);v.Add(p+Vector3.up*h+lean+side*0.35f);v.Add(p+Vector3.up*h+lean-side*0.35f);
                for(int k=0;k<4;k++) { n.Add(Vector3.up);c.Add(k<2?new Color(0.55f,0.62f,0.52f,1f):Color.white); }
                uv.Add(new Vector2(0f,0f));uv.Add(new Vector2(1f,0f));uv.Add(new Vector2(1f,1f));uv.Add(new Vector2(0f,1f));
                t.Add(bi);t.Add(bi+2);t.Add(bi+1);t.Add(bi);t.Add(bi+3);t.Add(bi+2);
            }
            var m=new Mesh { name="RumputHutan" };
            m.SetVertices(v);m.SetNormals(n);m.SetUVs(0,uv);m.SetColors(c);m.SetTriangles(t,0);
            m.RecalculateTangents();m.RecalculateBounds();
            AssetDatabase.CreateAsset(m,FolderMesh+"/RumputHutan.asset");return m;
        }

        static Mesh MeshBatuAlam()
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);var src=go.GetComponent<MeshFilter>().sharedMesh;
            var sv=src.vertices;var sn=src.normals;
            for(int i=0;i<sv.Length;i++)
            {
                Vector3 p=sv[i];
                float d=0.75f+0.42f*Mathf.PerlinNoise(p.x*2.4f+3.1f,p.z*2.4f+1.7f)+0.18f*Mathf.PerlinNoise(p.y*3.2f,p.x*1.9f);
                sv[i]=new Vector3(p.x*d,p.y*d*0.62f,p.z*d);
            }
            var m=new Mesh { name="BatuHutan" };
            m.vertices=sv;m.uv=src.uv;m.triangles=src.triangles;
            Object.DestroyImmediate(go);
            m.RecalculateNormals();m.RecalculateTangents();m.RecalculateBounds();
            AssetDatabase.CreateAsset(m,FolderMesh+"/BatuHutan.asset");return m;
        }

        static Mesh MeshKayuTumbang()
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);var src=go.GetComponent<MeshFilter>().sharedMesh;
            var sv=src.vertices;
            for(int i=0;i<sv.Length;i++)
            {
                Vector3 p=sv[i];float t=(p.y+1f)*0.5f;
                float n=0.88f+0.25f*Mathf.PerlinNoise(p.x*3f+p.y*2f,p.z*3f+1.2f);
                sv[i]=new Vector3(p.x*n*Mathf.Lerp(1.1f,0.75f,t),p.z*n*Mathf.Lerp(1.1f,0.75f,t),p.y*1.6f);
            }
            var m=new Mesh { name="KayuTumbang" };
            m.vertices=sv;m.uv=src.uv;m.triangles=src.triangles;
            Object.DestroyImmediate(go);
            m.RecalculateNormals();m.RecalculateTangents();m.RecalculateBounds();
            AssetDatabase.CreateAsset(m,FolderMesh+"/KayuTumbang.asset");return m;
        }

        static Material MaterialRumputLiar()
        {
            const int sz=256;var t=new Texture2D(sz,sz,TextureFormat.RGBA32,true);var px=new Color[sz*sz];
            for(int y=0;y<sz;y++)for(int x=0;x<sz;x++)
            {
                float u=(float)x/sz,v=(float)y/sz;
                float helai=0f;
                for(int k=0;k<7;k++)
                {
                    float cx=0.14f+k*0.12f+0.04f*Mathf.Sin(v*3.2f+k);
                    float w=Mathf.Lerp(0.055f,0.004f,v);
                    float d=Mathf.Clamp01(1f-Mathf.Abs(u-cx)/Mathf.Max(0.002f,w));
                    if(v<0.96f-k%3*0.08f)helai=Mathf.Max(helai,d*2.2f);
                }
                float g=0.65f+0.35f*Mathf.PerlinNoise(u*16f,v*16f);
                Color col=Color.Lerp(new Color(0.14f,0.24f,0.09f),new Color(0.32f,0.47f,0.16f),v)*g;
                col.a=Mathf.Clamp01(helai);px[y*sz+x]=col;
            }
            t.SetPixels(px);t.Apply();
            string pt=FolderVeg+"/rumput_helai.png";SimpanPng(t,pt);AssetDatabase.ImportAsset(pt);AturImpor(pt,false,false,sz);
            var imp=(TextureImporter)AssetImporter.GetAtPath(pt);imp.alphaIsTransparency=true;imp.mipMapsPreserveCoverage=true;imp.SaveAndReimport();
            var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(pt);
            return SimpanMat(KeAK(SafeMaterialFactory.CreatePBR("M_RumputHutan",Color.white,0f,0.18f,tex,null,null,SurfaceMode.Cutout,0.38f),ShaderDaun,0.03f,0.5f));
        }

        static void BuatSerasahHutan()
        {
            const int sisiSel=32,calon=240;
            var batu=MeshBatuAlam();var kayu=MeshKayuTumbang();var rumput=MeshRumputHutan();
            var mBatu=AssetDatabase.LoadAssetAtPath<Material>(FolderMat+"/M_Batu.mat");
            var mKayu=AssetDatabase.LoadAssetAtPath<Material>(FolderMat+"/M_KulitPohon.mat");
            if(mKayu==null)mKayu=AssetDatabase.LoadAssetAtPath<Material>(FolderMat+"/M_Kayu.mat");
            var mRumput=MaterialRumputLiar();
            var meshes=new[]{batu,kayu,rumput};var bahan=new[]{mBatu,mKayu,mRumput};
            var hasil=new List<SerasahHutan.KelompokSerasah>();
            for(int z=-560;z<-192;z+=sisiSel)for(int x=-224;x<320;x+=sisiSel)
            {
                var rand=new System.Random(x*73856093^z*19349663);
                var daftar=new[]{new List<Matrix4x4>(),new List<Matrix4x4>(),new List<Matrix4x4>()};
                var bounds=new Bounds[3];
                for(int n=0;n<calon;n++)
                {
                    float px=x+(float)rand.NextDouble()*sisiSel,pz=z+(float)rand.NextDouble()*sisiSel,y;
                    if(!BolehTanam(px,pz,out y,true))continue;
                    int tipe=n%10==0?0:n%18==1?1:2;
                    float rY=(float)rand.NextDouble()*360f;
                    Vector3 pos;Quaternion rot;Vector3 scale;
                    if(tipe==0) { float s=0.35f+(float)rand.NextDouble()*0.95f;pos=new Vector3(px,y-0.08f,pz);rot=Quaternion.Euler((float)rand.NextDouble()*18f,rY,0f);scale=new Vector3(s,s*0.65f,s*0.85f); }
                    else if(tipe==1) { float s=0.28f+(float)rand.NextDouble()*0.32f;pos=new Vector3(px,y+s*0.18f,pz);rot=Quaternion.Euler((float)rand.NextDouble()*8f,rY,0f);scale=new Vector3(s,s,0.8f+(float)rand.NextDouble()*0.9f); }
                    else { float s=0.75f+(float)rand.NextDouble()*0.95f;pos=new Vector3(px,y-0.02f,pz);rot=Quaternion.Euler(0f,rY,0f);scale=new Vector3(s,s*(0.8f+0.4f*(float)rand.NextDouble()),s); }
                    daftar[tipe].Add(Matrix4x4.TRS(pos,rot,scale));
                    if(daftar[tipe].Count==1)bounds[tipe]=new Bounds(pos,Vector3.one*5f);else bounds[tipe].Encapsulate(new Bounds(pos,Vector3.one*5f));
                }
                for(int t=0;t<3;t++)if(daftar[t].Count>0)hasil.Add(new SerasahHutan.KelompokSerasah { mesh=meshes[t],material=bahan[t],bounds=bounds[t],matriks=daftar[t].ToArray() });
            }
            var detail=Anak(_root,"SerasahHutan").AddComponent<SerasahHutan>();detail.kelompok=hasil.ToArray();EditorUtility.SetDirty(detail);
            Catat("Lantai hutan selatan: rumput liar 3D, batu lumut alam, & kayu tumbang instanced");
        }
    }
}
