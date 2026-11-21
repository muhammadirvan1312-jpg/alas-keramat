using System.Collections.Generic;
using System.IO;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
namespace AlasKeramat.EditorTools
{
    public static partial class NusantaraMasterBuilder
    {
        static Mesh MuatPropScan(string nama)
        {
            string p=FolderVeg+"/"+nama+".asset";
            var ada=AssetDatabase.LoadAssetAtPath<Mesh>(p);if(ada!=null)return ada;
            var m=ImporMeshVisual.Baca("Assets/ThirdParty/Prepared/"+nama+".bytes");
            if(m==null)return null;
            AssetDatabase.CreateAsset(m,p);
            return AssetDatabase.LoadAssetAtPath<Mesh>(p);
        }

        static Material MaterialFolScan(string slot,string namaMat,float cutoff,float goyang,Color tint)
        {
            var sudah=AssetDatabase.LoadAssetAtPath<Material>(FolderMat+"/"+namaMat+".mat");if(sudah!=null)return sudah;
            var tex=DaunFoto(slot);if(tex==null)return null;
            var norm=BacaFoto(slot,"Normal");
            if(norm!=null)
            {
                string pn=FolderVeg+"/"+slot+"_n.png";File.WriteAllBytes(pn,norm.EncodeToPNG());AssetDatabase.ImportAsset(pn);AturImpor(pn,true,false,1024);
                norm=AssetDatabase.LoadAssetAtPath<Texture2D>(pn);
            }
            return SimpanMat(KeAK(SafeMaterialFactory.CreatePBR(namaMat,tint,0f,0.20f,tex,norm,null,SurfaceMode.Cutout,cutoff),ShaderDaun,goyang,0.52f));
        }

        static Material MaterialSolidScan(string slot,string namaMat,Color tint,float halus)
        {
            var sudah=AssetDatabase.LoadAssetAtPath<Material>(FolderMat+"/"+namaMat+".mat");if(sudah!=null)return sudah;
            var alb=BacaFoto(slot,"Albedo");var norm=BacaFoto(slot,"Normal");
            if(alb==null)return null;
            string pa=FolderVeg+"/"+slot+"_a.png";File.WriteAllBytes(pa,alb.EncodeToPNG());AssetDatabase.ImportAsset(pa);AturImpor(pa,false,false,1024);
            alb=AssetDatabase.LoadAssetAtPath<Texture2D>(pa);
            if(norm!=null)
            {
                string pn=FolderVeg+"/"+slot+"_n.png";File.WriteAllBytes(pn,norm.EncodeToPNG());AssetDatabase.ImportAsset(pn);AturImpor(pn,true,false,1024);
                norm=AssetDatabase.LoadAssetAtPath<Texture2D>(pn);
            }
            return SimpanMat(KeAK(SafeMaterialFactory.CreatePBR(namaMat,tint,0f,halus,alb,norm),ShaderKulit,0f,0f));
        }

        static Mesh MeshRumputHutan()
        {
            var v=new List<Vector3>();var n=new List<Vector3>();var uv=new List<Vector2>();var c=new List<Color>();var t=new List<int>();
            var r=new System.Random(2026);
            for(int i=0;i<5;i++)
            {
                float a=i*(Mathf.PI*2f/5f)+(float)r.NextDouble()*0.35f;
                float ox=Mathf.Cos(a)*0.12f,oz=Mathf.Sin(a)*0.12f;
                float rot=a+0.4f+(float)r.NextDouble()*0.5f,h=0.34f+(float)r.NextDouble()*0.28f,w=0.18f+(float)r.NextDouble()*0.08f;
                Vector3 side=new Vector3(Mathf.Cos(rot)*w,0f,Mathf.Sin(rot)*w);
                Vector3 lean=new Vector3(-Mathf.Sin(rot),0f,Mathf.Cos(rot))*(0.14f+(float)r.NextDouble()*0.14f);
                Vector3 p0=new Vector3(ox,-0.03f,oz),p1=p0+Vector3.up*(h*0.52f)+lean*0.35f,p2=p0+Vector3.up*h+lean;
                int bi=v.Count;
                v.Add(p0-side); v.Add(p0+side); v.Add(p1-side*0.85f); v.Add(p1+side*0.85f); v.Add(p2-side*0.45f); v.Add(p2+side*0.45f);
                for(int k=0;k<6;k++) { n.Add(Vector3.up); c.Add(k<2?new Color(0.76f,0.84f,0.68f,1f):k<4?new Color(0.90f,0.94f,0.84f,1f):Color.white); }
                uv.Add(new Vector2(0f,0f));uv.Add(new Vector2(1f,0f));uv.Add(new Vector2(0f,0.52f));uv.Add(new Vector2(1f,0.52f));uv.Add(new Vector2(0f,1f));uv.Add(new Vector2(1f,1f));
                t.Add(bi);t.Add(bi+2);t.Add(bi+1);t.Add(bi+1);t.Add(bi+2);t.Add(bi+3);t.Add(bi+2);t.Add(bi+4);t.Add(bi+3);t.Add(bi+3);t.Add(bi+4);t.Add(bi+5);
            }
            var m=new Mesh { name="RumputHutan" };
            m.SetVertices(v);m.SetNormals(n);m.SetUVs(0,uv);m.SetColors(c);m.SetTriangles(t,0);
            m.RecalculateTangents();m.RecalculateBounds();
            AssetDatabase.CreateAsset(m,FolderMesh+"/RumputHutan.asset");return m;
        }

        static Mesh MeshBatuAlam()
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);var src=go.GetComponent<MeshFilter>().sharedMesh;
            var sv=src.vertices;var cols=new Color[sv.Length];
            for(int i=0;i<sv.Length;i++)
            {
                Vector3 p=sv[i];
                float d=0.75f+0.42f*Mathf.PerlinNoise(p.x*2.4f+3.1f,p.z*2.4f+1.7f)+0.18f*Mathf.PerlinNoise(p.y*3.2f,p.x*1.9f);
                sv[i]=new Vector3(p.x*d,p.y*d*0.62f,p.z*d);
                cols[i]=Color.Lerp(Color.white,new Color(0.72f,0.84f,0.62f,1f),Mathf.Clamp01(p.y*1.4f+0.25f)*0.65f);
            }
            var m=new Mesh { name="BatuHutan" };
            m.vertices=sv;m.uv=src.uv;m.colors=cols;m.triangles=src.triangles;
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
                float u=(float)x/sz,v=(float)y/sz,helai=0f;
                for(int k=0;k<11;k++)
                {
                    float cx=0.09f+k*0.082f+0.045f*Mathf.Sin(v*3.4f+k*1.3f),w=Mathf.Lerp(0.068f,0.006f,Mathf.Pow(v,0.85f));
                    if(v<0.96f-(k%3)*0.09f)helai=Mathf.Max(helai,Mathf.Clamp01(1f-Mathf.Abs(u-cx)/Mathf.Max(0.002f,w))*2.4f);
                }
                float g=0.75f+0.25f*Mathf.PerlinNoise(u*16f,v*16f);
                Color col=Color.Lerp(new Color(0.18f,0.34f,0.11f),new Color(0.44f,0.62f,0.22f),v)*g;col.a=Mathf.Clamp01(helai);px[y*sz+x]=col;
            }
            t.SetPixels(px);t.Apply();
            string pt=FolderVeg+"/rumput_helai.png";SimpanPng(t,pt);AssetDatabase.ImportAsset(pt);AturImpor(pt,false,false,sz);
            var imp=(TextureImporter)AssetImporter.GetAtPath(pt);imp.alphaIsTransparency=true;imp.mipMapsPreserveCoverage=true;imp.SaveAndReimport();
            var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(pt);
            return SimpanMat(KeAK(SafeMaterialFactory.CreatePBR("M_RumputHutan",Color.white,0f,0.22f,tex,null,null,SurfaceMode.Cutout,0.32f),ShaderDaun,0.04f,0.65f));
        }

        public static bool DekorasiMenutupAwal(float x,float z)
        {
            var p=new Vector2(x,z);
            if(Vector2.Distance(p,new Vector2(SpawnAndi.x,SpawnAndi.z))<4f)return true;
            foreach(var koper in PosisiKoper)if(Vector2.Distance(p,new Vector2(koper.x,koper.z))<3f)return true;
            return false;
        }

        // Hutan di luar zona awal: batu lumut, kayu tumbang, akar, tunggul, ranting, pakis dan keladi jarang tapi merata.
        static bool BiomaHutan(Biome b)
        {
            int i=(int)b;
            return b==Biome.HutanCampuran || b==Biome.HutanJatiSelatan || b==Biome.HutanBambu || b==Biome.TepiSungai || b==Biome.KawahBeringin || i==11 || i==15;
        }
        static bool BiomaBatu(Biome b) { return b==Biome.TebingKapur || b==WorldTerrainGenerator.GunungKapur; }
        static readonly int[] TipeSebaran={4,4,10,10,11,5,6,7,8,8,9,2,2,3};
        static int SebaranPulau(Mesh[] meshes,Material[] bahan,List<SerasahHutan.KelompokSerasah> hasil)
        {
            const int sel=64,calon=26;int total=0;
            var mBesar=MuatPropScan("BatuBesarScan");
            var matBesar=MaterialSolidScan("BatuBesarAsli","M_BatuBesarScan",new Color(0.86f,0.85f,0.82f,1f),0.22f);
            for(int z=-1000;z<1000;z+=sel)for(int x=-1000;x<1000;x+=sel)
            {
                if(x+sel>-256 && x<352 && z+sel>-576 && z<-160)continue;
                var rand=new System.Random(x*92821^z*68917^0x5A17);
                var daftar=new List<Matrix4x4>[meshes.Length];
                for(int k=0;k<meshes.Length;k++)daftar[k]=new List<Matrix4x4>();
                var batuBesar=new List<Matrix4x4>();
                for(int n=0;n<calon;n++)
                {
                    float px=x+(float)rand.NextDouble()*sel,pz=z+(float)rand.NextDouble()*sel,y;
                    var bio=WorldTerrainGenerator.GetBiome(px,pz);
                    if(BiomaBatu(bio) && mBesar!=null && matBesar!=null && n%3==0 && BolehTanam(px,pz,out y,true))
                    {
                        float sb=0.9f+(float)rand.NextDouble()*2.2f;
                        batuBesar.Add(Matrix4x4.TRS(new Vector3(px,y-0.25f*sb,pz),Quaternion.Euler((float)rand.NextDouble()*20f-10f,(float)rand.NextDouble()*360f,(float)rand.NextDouble()*20f-10f),new Vector3(sb,sb*(0.7f+0.4f*(float)rand.NextDouble()),sb)));
                        continue;
                    }
                    if(!BiomaHutan(bio) || !BolehTanam(px,pz,out y,true))continue;
                    int tipe=TipeSebaran[rand.Next(TipeSebaran.Length)];
                    if(tipe>=meshes.Length)continue;
                    float s=0.7f+(float)rand.NextDouble()*0.8f,rY=(float)rand.NextDouble()*360f;
                    var rot=tipe==2 || tipe==3 || tipe==6 || tipe==7?Quaternion.Euler(0f,rY,0f):Quaternion.Euler((float)rand.NextDouble()*10f-5f,rY,(float)rand.NextDouble()*10f-5f);
                    daftar[tipe].Add(Matrix4x4.TRS(new Vector3(px,y-0.05f,pz),rot,new Vector3(s,s*(0.85f+0.3f*(float)rand.NextDouble()),s)));
                }
                if(batuBesar.Count>0)
                {
                    var ab=batuBesar.ToArray();var bb=new Bounds((Vector3)ab[0].GetColumn(3),Vector3.one*8f);
                    for(int k=1;k<ab.Length;k++)bb.Encapsulate(new Bounds((Vector3)ab[k].GetColumn(3),Vector3.one*8f));
                    hasil.Add(new SerasahHutan.KelompokSerasah { mesh=mBesar,material=matBesar,bounds=bb,matriks=ab });total+=ab.Length;
                }
                for(int t=0;t<meshes.Length;t++)
                {
                    var arr=daftar[t].ToArray();if(arr.Length==0)continue;
                    var b=new Bounds((Vector3)arr[0].GetColumn(3),Vector3.one*6f);
                    for(int k=1;k<arr.Length;k++)b.Encapsulate(new Bounds((Vector3)arr[k].GetColumn(3),Vector3.one*6f));
                    hasil.Add(new SerasahHutan.KelompokSerasah { mesh=meshes[t],material=bahan[t],bounds=b,matriks=arr });
                    total+=arr.Length;
                }
            }
            return total;
        }

        static bool AreaBebasSerasah(float x,float z)
        {
            var p=new Vector2(x,z);
            if(Vector2.Distance(p,new Vector2(PosDeden.x,PosDeden.z))<1.9f || Vector2.Distance(p,new Vector2(25.2f,-437.8f))<1.5f)return true;
            if(Vector2.Distance(p,new Vector2(PosNingsih.x,PosNingsih.z))<1.8f || Vector2.Distance(p,new Vector2(198.7f,-351.6f))<1.5f)return true;
            if(Vector2.Distance(p,new Vector2(PosDioramaHantu.x,PosDioramaHantu.z))<3.5f || Vector2.Distance(p,new Vector2(-59.6f,-305.6f))<1.8f)return true;
            if(Vector2.Distance(p,new Vector2(65f,-335f))<1.5f)return true;
            return false;
        }

        static void BuatSerasahHutan()
        {
            const int sisiSel=32,calon=410;
            var fallRumput=MeshRumputHutan();var fallBatu=MeshBatuAlam();var fallKayu=MeshKayuTumbang();
            var mFallBatu=AssetDatabase.LoadAssetAtPath<Material>(FolderMat+"/M_Batu.mat");
            var mFallKayu=AssetDatabase.LoadAssetAtPath<Material>(FolderMat+"/M_KulitPohon.mat")??AssetDatabase.LoadAssetAtPath<Material>(FolderMat+"/M_Kayu.mat");
            var mFallRumput=MaterialRumputLiar();

            var meshes=new Mesh[]{
                MuatPropScan("RumputScanA")??fallRumput, MuatPropScan("RumputScanB")??fallRumput,
                MuatPropScan("PakisScan_LOD0")??fallRumput, MuatPropScan("KeladiScan")??fallRumput,
                MuatPropScan("BatuLumutA")??fallBatu, MuatPropScan("KayuTumbangScan")??fallKayu, MuatPropScan("AkarScan")??fallBatu,
                MuatPropScan("TunggulScan")??fallKayu, MuatPropScan("RantingScanA")??fallKayu, MuatPropScan("RantingScanB")??fallKayu,
                MuatPropScan("BatuLumutC")??fallBatu, MuatPropScan("BatuLumutD")??fallBatu
            };
            Color folTint=new Color(0.76f,0.82f,0.68f,1f);
            var mPakis=AssetDatabase.LoadAssetAtPath<Material>(FolderMat+"/M_PakisAsli.mat");
            var bahan=new Material[]{
                MaterialFolScan("RumputAsliA","M_RumputScanA",0.30f,0.045f,folTint)??mFallRumput,
                MaterialFolScan("RumputAsliB","M_RumputScanB",0.30f,0.045f,folTint)??mFallRumput,
                mPakis??mFallRumput,
                MaterialFolScan("KeladiAsli","M_KeladiScan",0.36f,0.035f,new Color(0.84f,0.88f,0.78f,1f))??mFallRumput,
                MaterialSolidScan("BatuLumutAsli","M_BatuLumutScan",new Color(0.84f,0.86f,0.82f,1f),0.26f)??mFallBatu,
                MaterialSolidScan("KayuTumbangAsli","M_KayuTumbangScan",new Color(0.80f,0.77f,0.72f,1f),0.22f)??mFallKayu,
                MaterialSolidScan("AkarAsli","M_AkarScan",new Color(0.78f,0.74f,0.68f,1f),0.24f)??mFallBatu,
                MaterialSolidScan("TunggulAsli","M_TunggulScan",new Color(0.80f,0.76f,0.70f,1f),0.20f)??mFallKayu,
                MaterialSolidScan("RantingAsli","M_RantingScan",new Color(0.74f,0.70f,0.64f,1f),0.18f)??mFallKayu,
                MaterialSolidScan("RantingAsli","M_RantingScan",new Color(0.74f,0.70f,0.64f,1f),0.18f)??mFallKayu,
                MaterialSolidScan("BatuLumutAsliB","M_BatuLumutScanB",new Color(0.82f,0.85f,0.80f,1f),0.24f)??mFallBatu,
                MaterialSolidScan("BatuLumutAsliB","M_BatuLumutScanB",new Color(0.82f,0.85f,0.80f,1f),0.24f)??mFallBatu
            };

            var hasil=new List<SerasahHutan.KelompokSerasah>();
            for(int z=-576;z<-160;z+=sisiSel)for(int x=-256;x<352;x+=sisiSel)
            {
                var rand=new System.Random(x*73856093^z*19349663);
                var daftar=new List<Matrix4x4>[meshes.Length];
                for(int k=0;k<meshes.Length;k++)daftar[k]=new List<Matrix4x4>();
                for(int n=0;n<calon;n++)
                {
                    float px=x+(float)rand.NextDouble()*sisiSel,pz=z+(float)rand.NextDouble()*sisiSel,y;
                    if(!BolehTanam(px,pz,out y,true) || DekorasiMenutupAwal(px,pz) || AreaBebasSerasah(px,pz))continue;
                    int roll=n%32;
                    int tipe=roll<6?0:roll<11?1:roll<17?2:roll<20?3:roll<23?4:roll==23?5:roll==24?6:roll==25?7:roll<28?8:roll==28?9:roll<31?10:11;
                    float rY=(float)rand.NextDouble()*360f;
                    Vector3 pos;Quaternion rot;Vector3 scale;
                    if(tipe==8 || tipe==9) { float s=0.75f+(float)rand.NextDouble()*0.6f;pos=new Vector3(px,y-0.01f,pz);rot=Quaternion.Euler((float)rand.NextDouble()*8f-4f,rY,(float)rand.NextDouble()*8f-4f);scale=new Vector3(s,s,s); }
                    else if(tipe==10 || tipe==11) { float s=0.70f+(float)rand.NextDouble()*0.9f;pos=new Vector3(px,y-0.05f,pz);rot=Quaternion.Euler((float)rand.NextDouble()*14f-7f,rY,(float)rand.NextDouble()*14f-7f);scale=new Vector3(s,s*(0.8f+(float)rand.NextDouble()*0.35f),s); }
                    else if(tipe==4) { float s=0.72f+(float)rand.NextDouble()*0.95f;pos=new Vector3(px,y-0.04f,pz);rot=Quaternion.Euler((float)rand.NextDouble()*12f-6f,rY,(float)rand.NextDouble()*12f-6f);scale=new Vector3(s,s*(0.85f+(float)rand.NextDouble()*0.3f),s); }
                    else if(tipe==5) { float s=0.65f+(float)rand.NextDouble()*0.55f;pos=new Vector3(px,y-0.04f,pz);rot=Quaternion.Euler((float)rand.NextDouble()*6f-3f,rY,(float)rand.NextDouble()*6f-3f);scale=new Vector3(s,s,s); }
                    else if(tipe==6 || tipe==7) { float s=0.70f+(float)rand.NextDouble()*0.50f;pos=new Vector3(px,y-0.06f,pz);rot=Quaternion.Euler(0f,rY,0f);scale=new Vector3(s,s,s); }
                    else { float s=0.80f+(float)rand.NextDouble()*0.65f;pos=new Vector3(px,y-0.02f,pz);rot=Quaternion.Euler(0f,rY,0f);scale=new Vector3(s,s*(0.82f+0.36f*(float)rand.NextDouble()),s); }
                    daftar[tipe].Add(Matrix4x4.TRS(pos,rot,scale));
                }
                for(int t=0;t<meshes.Length;t++)
                {
                    var all=daftar[t];
                    for(int a=0;a<all.Count;a+=AlasKeramat.Building.VegetationManager.MaksRender)
                    {
                        int cnt=Mathf.Min(AlasKeramat.Building.VegetationManager.MaksRender,all.Count-a);
                        var arr=all.GetRange(a,cnt).ToArray();
                        var b=new Bounds((Vector3)arr[0].GetColumn(3),Vector3.one*5f);
                        for(int k=1;k<cnt;k++)b.Encapsulate(new Bounds((Vector3)arr[k].GetColumn(3),Vector3.one*5f));
                        hasil.Add(new SerasahHutan.KelompokSerasah { mesh=meshes[t],material=bahan[t],bounds=b,matriks=arr });
                    }
                }
            }
            int sebaran=SebaranPulau(meshes,bahan,hasil);
            var detail=Anak(_root,"SerasahHutan").AddComponent<SerasahHutan>();detail.kelompok=hasil.ToArray();EditorUtility.SetDirty(detail);
            Catat("Lantai hutan photogrammetry CC0: "+hasil.Count+" batch, "+sebaran+" sebaran se-pulau (rumput scan, pakis, keladi, batu lumut 3 jenis, kayu tumbang, akar, tunggul, ranting kering)");
        }
    }
}
