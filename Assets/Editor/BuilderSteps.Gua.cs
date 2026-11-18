using System.Collections.Generic;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace AlasKeramat.EditorTools
{
    public static partial class NusantaraMasterBuilder
    {
        public const string FolderGua=FolderGen+"/Gua";
        const float SelGua=2f;
        static void LangkahGua()
        {
            EnsureFolder(FolderGua);
            var root=Anak(_root,"BawahTanah");
            var lights=new List<Light>();
            var cyan=SimpanMat(SafeMaterialFactory.CreatePBR("M_LumutGua",ProcTex.Hex("#123F37"),0f,0.7f,null,null,ProcTex.Hex("#3CF2C8")));
            var merah=SimpanMat(SafeMaterialFactory.CreatePBR("M_AkarGua",ProcTex.Hex("#421911"),0f,0.3f,null,null,new Color(0.65f,0.03f,0.01f)));
            for(int id=1;id<=3;id++)BuatKompleks(root,id,lights,cyan,merah);
            Vector3 ug1=new Vector3(-600f,-80f,-600f),ug2=new Vector3(0f,-80f,-600f),ug3=new Vector3(600f,-80f,-600f);
            Vector3 s1=PermukaanGua(264f,-276f),s2=PermukaanGua(-560f,290f),s3=PermukaanGua(0f,684f);
            BuatGerbang(root,"PintuUG1",s1,JenisGerbangGua.Masuk,ug1+new Vector3(-120f,0.2f,0f),null);
            BuatGerbang(root,"PalkaUG2",s2,JenisGerbangGua.Gali,ug2+new Vector3(0f,0.2f,-110f),null);
            // Dua pancang X menandai tanah gembur.
            var tanah=KotakGua(root,"TanahGembur",s2-Vector3.up*0.15f,new Vector3(4f,0.25f,4f),AmbilMat("M_Batu"));
            for(int i=0;i<2;i++) { var p=KotakGua(root,"PancangX",s2+new Vector3(i==0?-1f:1f,0.8f,0f),new Vector3(0.13f,2f,0.13f),AmbilMat("M_Kayu"));p.transform.rotation=Quaternion.Euler(0f,0f,i==0?35f:-35f); }
            BuatGerbang(root,"PintuUG3",s3,JenisGerbangGua.Masuk,ug3+new Vector3(0f,0.2f,-140f),null);
            BuatGerbang(root,"KeluarUG1",ug1+new Vector3(-124f,0.2f,0f),JenisGerbangGua.Keluar,s1+Vector3.right*3f,null);
            BuatGerbang(root,"KeluarUG2",ug2+new Vector3(0f,0.2f,-114f),JenisGerbangGua.Keluar,s2+Vector3.right*3f,null);
            BuatGerbang(root,"KeluarUG3",ug3+new Vector3(0f,0.2f,-144f),JenisGerbangGua.Keluar,s3-Vector3.forward*3f,null);
            BuatGerbang(root,"TanggaDaruratUG2",ug2+new Vector3(28f,0.2f,74f),JenisGerbangGua.Keluar,PermukaanGua(-540f,300f),null);
            var baja=KotakGua(root,"D1_Baja",ug2+new Vector3(0f,1.6f,-40f),new Vector3(8f,3.2f,0.5f),AmbilMat("M_LogamKarat"));
            BuatGerbang(root,"PembacaKartu",ug2+new Vector3(-1.8f,0f,-41f),JenisGerbangGua.Kartu,Vector3.zero,baja);
            var selam=KotakGua(root,"U1_PasangRebreather",ug1+new Vector3(40f,0f,-28f),new Vector3(0.3f,8f,8f),AmbilMat("M_GelapGua"));
            BuatGerbang(root,"GerbangSelam",ug1+new Vector3(38f,-1.4f,-28f),JenisGerbangGua.Selam,Vector3.zero,selam);
            var runtuh=KotakGua(root,"ReruntuhanT2",ug1+new Vector3(100f,5f,16f),new Vector3(1f,10f,8f),AmbilMat("M_Batu"));
            var dorong=BuatGerbang(root,"PintasUG1",ug1+new Vector3(101f,0f,16f),JenisGerbangGua.Reruntuhan,Vector3.zero,runtuh);
            dorong.Pasang(JenisGerbangGua.Reruntuhan,Vector3.zero,"gua_PintasUG1",runtuh,true);
            var airMat=SimpanMat(SafeMaterialFactory.CreatePBR("M_AirGua",new Color(0.03f,0.12f,0.14f,0.55f),0.1f,0.9f,null,null,null,SurfaceMode.Transparent));
            var air=KotakGua(root,"AirUG1",ug1+new Vector3(76f,-1f,-28f),new Vector3(88f,0.05f,8f),airMat);
            Object.DestroyImmediate(air.GetComponent<Collider>());air.layer=GameLayers.Water;
            var angin=Anak(root,"LorongAngin");angin.transform.position=ug3+new Vector3(0f,2f,-88f);
            var ac=angin.AddComponent<BoxCollider>();ac.isTrigger=true;ac.size=new Vector3(6f,6f,15f);angin.layer=GameLayers.TriggerZone;
            angin.AddComponent<LorongAnginGua>();
            PetiGua(root,"KampEkspedisi",ug1+new Vector3(20f,0f,30f),new[]{ItemID.AlatSelamRebreather,ItemID.KartuAksesBunker,ItemID.Baterai,ItemID.Tulang},new[]{1,1,2,3});
            PetiGua(root,"AltarPaku1",ug1+new Vector3(120f,0f,-28f),new[]{ItemID.PakuKeramat1},new[]{1});
            PetiGua(root,"Laboratorium",ug2+new Vector3(28f,0f,-16f),new[]{ItemID.SenterTaktis,ItemID.Baterai},new[]{1,3});
            PetiGua(root,"GudangPeralatan",ug2+new Vector3(-24f,0f,60f),new[]{ItemID.ZiplineGun,ItemID.TaliZipline,ItemID.LogamBekas,ItemID.ZirahBaja,ItemID.KapakBaja,ItemID.PistolPT,ItemID.PeluruPistol},new[]{1,6,4,2,1,1,16});
            PetiGua(root,"SegelPaku2",ug2+new Vector3(24f,0f,68f),new[]{ItemID.PakuKeramat2},new[]{1});
            // Paku 3 dan pintu boss disambungkan T10c; arena dan jalur balik ke K0 sudah bisa dijelajahi.
            PetiGua(root,"CangkulDesa",PermukaanGua(560f,216f),new[]{ItemID.Cangkul,ItemID.TaliIjuk,ItemID.MinyakTanah},new[]{1,2,1});
            BuatZiplineGua(root);
            Image tirai=null;
            var canvas=_ui.transform.Find("Canvas_HUD");
            if(canvas!=null) { var go=UIStretch(canvas.gameObject,"FadeGua",Vector2.zero,Vector2.one);tirai=go.AddComponent<Image>();tirai.color=Color.black;tirai.raycastTarget=false;tirai.enabled=false; }
            var manager=Anak(_sistem,"WorldCaveAndSaveManager").AddComponent<WorldCaveAndSaveManager>();
            manager.Pasang(tirai,lights.ToArray(),new Bounds(ug1+new Vector3(76f,-5f,-28f),new Vector3(88f,8f,8f)));
            EditorUtility.SetDirty(manager);
            Catat("T10a: UG1-UG3, gerbang alat, air gua, reverb, zipline, "+lights.Count+" lampu (maks 4 aktif)");
        }
        static Vector3 PermukaanGua(float x,float z) { return new Vector3(x,WorldTerrainGenerator.SampleHeight(x,z)+0.1f,z); }
        static GameObject KotakGua(GameObject parent,string nama,Vector3 p,Vector3 ukuran,Material mat)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=nama;go.transform.SetParent(parent.transform,false);go.transform.position=p;go.transform.localScale=ukuran;
            go.GetComponent<Renderer>().sharedMaterial=mat;go.layer=GameLayers.Ground;return go;
        }
        static GerbangGua BuatGerbang(GameObject parent,string nama,Vector3 p,JenisGerbangGua jenis,Vector3 tujuan,GameObject blok)
        {
            var go=KotakGua(parent,nama,p+Vector3.up*0.8f,new Vector3(1f,1.6f,0.3f),AmbilMat("M_LogamKarat"));go.layer=GameLayers.Interactable;
            var g=go.AddComponent<GerbangGua>();g.Pasang(jenis,tujuan,"gua_"+nama,blok);EditorUtility.SetDirty(g);return g;
        }
        static void PetiGua(GameObject parent,string nama,Vector3 p,ItemID[] item,int[] jumlah)
        {
            var go=KotakGua(parent,nama,p+Vector3.up*0.45f,new Vector3(1.3f,0.9f,0.8f),AmbilMat("M_Kayu"));go.layer=GameLayers.Interactable;
            var wadah=go.AddComponent<WadahItem>();wadah.KunciSimpan="gua_loot_"+nama;wadah.Pasang(nama,item,jumlah,0,false,null);EditorUtility.SetDirty(wadah);
        }
        static void BuatZiplineGua(GameObject parent)
        {
            Vector3 a=PermukaanGua(0f,620f)+Vector3.up*0.5f,b=PermukaanGua(0f,676f)+Vector3.up*0.5f;
            var ga=KotakGua(parent,"JangkarJurangA",a+Vector3.up,new Vector3(0.35f,2f,0.35f),AmbilMat("M_Kayu"));
            var gb=KotakGua(parent,"JangkarJurangB",b+Vector3.up,new Vector3(0.35f,2f,0.35f),AmbilMat("M_Kayu"));
            ga.layer=gb.layer=GameLayers.Interactable;
            var ja=ga.AddComponent<JangkarZipline>();var jb=gb.AddComponent<JangkarZipline>();
            var lr=Anak(parent,"TaliJurang").AddComponent<LineRenderer>();lr.sharedMaterial=AmbilMat("M_Kayu");lr.positionCount=2;lr.SetPosition(0,a+Vector3.up*2.2f);lr.SetPosition(1,b+Vector3.up*2.2f);lr.startWidth=lr.endWidth=0.045f;lr.enabled=false;
            // Naik di sisi jangkar, sehingga kapsul tidak menabrak tiangnya sendiri.
            ja.Pasang(jb,lr,a+Vector3.right,b+Vector3.right,"zipline_A");jb.Pasang(ja,lr,b+Vector3.right,a+Vector3.right,"zipline_B");
            EditorUtility.SetDirty(ja);EditorUtility.SetDirty(jb);
        }
        static void BuatKompleks(GameObject root,int id,List<Light> lights,Material cyan,Material merah)
        {
            var go=Anak(root,"UG"+id);go.transform.position=new Vector3((id-2)*600f,-80f,-600f);
            var sel=new HashSet<Vector2Int>();
            var node=new List<Vector3>();var edges=new List<Vector2Int>();
            if(id==1)
            {
                RuangGua(sel,-120,0,12,10);RuangGua(sel,-30,0,20,18);RuangGua(sel,20,30,12,10);RuangGua(sel,30,-20,14,12);RuangGua(sel,120,-28,16,16);
                KoridorGua(sel,-120,0,-30,0,4);KoridorGua(sel,-30,0,20,0,4);KoridorGua(sel,20,0,20,30,4);KoridorGua(sel,20,0,30,0,4);KoridorGua(sel,30,0,30,-28,4);KoridorGua(sel,30,-28,120,-28,4);
                KoridorGua(sel,120,-28,120,16,4);KoridorGua(sel,120,16,0,16,4);KoridorGua(sel,0,16,0,0,4);
                node.AddRange(new[]{new Vector3(-120,0,0),new Vector3(-30,0,0),new Vector3(20,0,0),new Vector3(20,0,30),new Vector3(30,0,0),new Vector3(30,0,-28),new Vector3(76,-4,-28),new Vector3(120,0,-28)});
                edges.AddRange(new[]{new Vector2Int(0,1),new Vector2Int(1,2),new Vector2Int(2,3),new Vector2Int(2,4),new Vector2Int(4,5),new Vector2Int(5,6),new Vector2Int(6,7)});
            }
            else if(id==2)
            {
                RuangGua(sel,0,-110,8,10);KoridorGua(sel,0,-110,0,68,4);
                RuangGua(sel,-28,-16,12,10);RuangGua(sel,28,-16,14,12);KoridorGua(sel,-28,-16,28,-16,4);
                RuangGua(sel,-24,60,10,10);RuangGua(sel,24,68,16,16);KoridorGua(sel,-24,60,0,60,4);KoridorGua(sel,0,68,24,68,4);
                node.AddRange(new[]{new Vector3(0,0,-110),new Vector3(0,0,-40),new Vector3(0,0,-16),new Vector3(-28,0,-16),new Vector3(28,0,-16),new Vector3(0,0,60),new Vector3(-24,0,60),new Vector3(0,0,68),new Vector3(24,0,68)});
                edges.AddRange(new[]{new Vector2Int(0,1),new Vector2Int(1,2),new Vector2Int(2,3),new Vector2Int(2,4),new Vector2Int(2,5),new Vector2Int(5,6),new Vector2Int(5,7),new Vector2Int(7,8)});
            }
            else
            {
                RuangGua(sel,0,-140,8,12);KoridorGua(sel,0,-140,0,40,4);RuangGua(sel,-36,-30,12,10);KoridorGua(sel,-36,-30,0,-30,4);
                RuangGua(sel,0,40,40,40);KoridorGua(sel,0,40,0,120,4);RuangGua(sel,0,120,8,8);
                node.AddRange(new[]{new Vector3(0,0,-140),new Vector3(0,0,-88),new Vector3(0,0,-30),new Vector3(-36,0,-30),new Vector3(0,0,40),new Vector3(0,0,120)});
                edges.AddRange(new[]{new Vector2Int(0,1),new Vector2Int(1,2),new Vector2Int(2,3),new Vector2Int(2,4),new Vector2Int(4,5)});
            }
            var mesh=MeshGua(sel,id);AssetDatabase.CreateAsset(mesh,FolderGua+"/UG"+id+".asset");
            go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=AmbilMat(id==2?"M_LogamKarat":"M_Batu");
            go.AddComponent<MeshCollider>().sharedMesh=mesh;go.layer=GameLayers.Ground;
            for(int i=0;i<node.Count;i++)node[i]+=go.transform.position;
            go.AddComponent<WaypointGraph>().Pasang(node.ToArray(),edges.ToArray());
            var rv=go.AddComponent<AudioReverbZone>();rv.minDistance=200f;rv.maxDistance=220f;rv.reverbPreset=id==1?AudioReverbPreset.Cave:id==2?AudioReverbPreset.Hallway:AudioReverbPreset.Arena;
            for(int i=0;i<node.Count;i++)
            {
                Vector3 p=node[i]+Vector3.up*(id==2?2.5f:3.5f);
                var deco=KotakGua(go,"PenandaCahaya_"+i,p,new Vector3(id==2?1.4f:0.5f,0.15f,0.25f),id==3?merah:cyan);
                Object.DestroyImmediate(deco.GetComponent<Collider>());
                var l=deco.AddComponent<Light>();l.type=LightType.Point;l.color=id==3?new Color(1f,0.12f,0.05f):id==2?new Color(0.7f,0.8f,0.7f):new Color(0.24f,0.95f,0.78f);l.range=id==3?18f:14f;l.intensity=2f;l.shadows=LightShadows.None;l.enabled=false;lights.Add(l);
            }
            HiasGua(go,id,cyan,merah);
        }
        static void RuangGua(HashSet<Vector2Int> cells,int x,int z,int lebar,int dalam)
        { for(int a=x-lebar/2;a<x+lebar/2;a+=2)for(int b=z-dalam/2;b<z+dalam/2;b+=2)cells.Add(new Vector2Int(Mathf.FloorToInt(a/2f),Mathf.FloorToInt(b/2f))); }
        static void KoridorGua(HashSet<Vector2Int> cells,int x,int z,int xx,int zz,int lebar)
        { for(int a=Mathf.Min(x,xx)-lebar/2;a<=Mathf.Max(x,xx)+lebar/2;a+=2)for(int b=Mathf.Min(z,zz)-lebar/2;b<=Mathf.Max(z,zz)+lebar/2;b+=2)cells.Add(new Vector2Int(Mathf.FloorToInt(a/2f),Mathf.FloorToInt(b/2f))); }
        static float LantaiGua(int id,float x,float z)
        { return id==1 && z<=-20f?-4f*Mathf.Clamp01((x-32f)/16f)*Mathf.Clamp01((120f-x)/12f):0f; }
        static float LangitGua(int id,float x,float z,float h)
        {
            if(id==1 && z<=-20f && x>40f && x<116f)
                return Mathf.Lerp(h,-1.2f,Mathf.Clamp01((x-40f)/8f)*Mathf.Clamp01((116f-x)/6f));
            return id==2?h:h+Mathf.PerlinNoise(x*0.13f+40f,z*0.13f+40f)*1.2f;
        }
        static Mesh MeshGua(HashSet<Vector2Int> cells,int id)
        {
            var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector2>();
            foreach(var c in cells)
            {
                float x=c.x*SelGua,z=c.y*SelGua;
                float h=id==2?3.2f:id==3?22f:10f;
                Vector3 a=new Vector3(x,LantaiGua(id,x,z),z),b=new Vector3(x+2,LantaiGua(id,x+2,z),z),d=new Vector3(x,LantaiGua(id,x,z+2),z+2),e=new Vector3(x+2,LantaiGua(id,x+2,z+2),z+2);
                Vector3 aa=new Vector3(x,LangitGua(id,x,z,h),z),bb=new Vector3(x+2,LangitGua(id,x+2,z,h),z),dd=new Vector3(x,LangitGua(id,x,z+2,h),z+2),ee=new Vector3(x+2,LangitGua(id,x+2,z+2,h),z+2);
                QuadGua(v,t,uv,a,d,e,b);QuadGua(v,t,uv,aa,bb,ee,dd);
                if(!cells.Contains(c+Vector2Int.left))QuadGua(v,t,uv,a,aa,dd,d);
                if(!cells.Contains(c+Vector2Int.right))QuadGua(v,t,uv,b,e,ee,bb);
                if(!cells.Contains(c+Vector2Int.down))QuadGua(v,t,uv,a,b,bb,aa);
                if(!cells.Contains(c+Vector2Int.up))QuadGua(v,t,uv,d,dd,ee,e);
            }
            var m=new Mesh { name="UG"+id+"_Interior",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32 };m.SetVertices(v);m.SetTriangles(t,0);m.SetUVs(0,uv);m.RecalculateNormals();m.RecalculateBounds();return m;
        }
        static void QuadGua(List<Vector3> v,List<int> t,List<Vector2> uv,Vector3 a,Vector3 b,Vector3 c,Vector3 d)
        { int n=v.Count;v.Add(a);v.Add(b);v.Add(c);v.Add(d);t.Add(n);t.Add(n+1);t.Add(n+2);t.Add(n);t.Add(n+2);t.Add(n+3);uv.Add(Vector2.zero);uv.Add(Vector2.up);uv.Add(Vector2.one);uv.Add(Vector2.right); }
    }
}
