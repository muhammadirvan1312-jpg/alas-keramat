using System.IO;
using UnityEngine;
namespace AlasKeramat.EditorTools
{
    // Format AKV1: hasil panggang headless, hanya mesh & bobot; plugin pembuat aset tidak ikut APK.
    public static class ImporMeshVisual
    {
        public static Mesh Baca(string path) { Vector3[] rest;return Baca(path,out rest); }
        public static Mesh Baca(string path,out Vector3[] rest)
        {
            rest=null;
            if(!File.Exists(path))return null;
            using(var file=File.OpenRead(path))using(var r=new BinaryReader(file))
            {
                if(new string(r.ReadChars(4))!="AKV1")throw new IOException("Format mesh visual tidak dikenali: "+path);
                int count=r.ReadInt32(),sub=r.ReadInt32(),skinned=r.ReadInt32();
                if(count<3 || count>500000 || sub<1 || sub>16)throw new IOException("Ukuran mesh visual tidak sah");
                if(skinned>0) { rest=new Vector3[skinned];for(int i=0;i<skinned;i++)rest[i]=new Vector3(r.ReadSingle(),r.ReadSingle(),r.ReadSingle()); }
                var p=new Vector3[count];var n=new Vector3[count];var uv=new Vector2[count];var w=new BoneWeight[count];
                for(int i=0;i<count;i++)
                {
                    p[i]=new Vector3(r.ReadSingle(),r.ReadSingle(),r.ReadSingle());n[i]=new Vector3(r.ReadSingle(),r.ReadSingle(),r.ReadSingle());uv[i]=new Vector2(r.ReadSingle(),r.ReadSingle());
                    if(skinned!=0)w[i]=new BoneWeight { boneIndex0=r.ReadInt32(),boneIndex1=r.ReadInt32(),boneIndex2=r.ReadInt32(),boneIndex3=r.ReadInt32(),weight0=r.ReadSingle(),weight1=r.ReadSingle(),weight2=r.ReadSingle(),weight3=r.ReadSingle() };
                }
                var mesh=new Mesh { name=Path.GetFileNameWithoutExtension(path),indexFormat=UnityEngine.Rendering.IndexFormat.UInt32 };
                mesh.vertices=p;mesh.normals=n;mesh.uv=uv;mesh.subMeshCount=sub;
                for(int s=0;s<sub;s++) { int k=r.ReadInt32();if(k<0 || k>1500000 || k%3!=0)throw new IOException("Indeks mesh visual tidak sah");var t=new int[k];for(int j=0;j<k;j++)t[j]=r.ReadInt32();mesh.SetTriangles(t,s); }
                if(skinned!=0)mesh.boneWeights=w;
                if(file.Position<file.Length)
                {
                    int shapes=r.ReadInt32();if(shapes<0 || shapes>32)throw new IOException("Jumlah ekspresi mesh tidak sah");
                    for(int s=0;s<shapes;s++)
                    {
                        int length=r.ReadInt32();if(length<1 || length>80)throw new IOException("Nama ekspresi tidak sah");
                        string nama=System.Text.Encoding.UTF8.GetString(r.ReadBytes(length));int entries=r.ReadInt32();
                        if(entries<0 || entries>count)throw new IOException("Bobot ekspresi tidak sah");
                        var delta=new Vector3[count];
                        for(int j=0;j<entries;j++) { int index=r.ReadInt32();if(index<0 || index>=count)throw new IOException("Indeks ekspresi tidak sah");delta[index]=new Vector3(r.ReadSingle(),r.ReadSingle(),r.ReadSingle()); }
                        mesh.AddBlendShapeFrame(nama,100f,delta,null,null);
                    }
                }
                mesh.RecalculateTangents();mesh.RecalculateBounds();return mesh;
            }
        }
    }
}
