using System.IO;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace AlasKeramat.EditorTools
{
    public static partial class NusantaraMasterBuilder
    {
        static void LangkahG3()
        {
            BuatSerasahHutan();
            var go=Anak(_root,"AtmosferHutanG3");
            string path=FolderTex+"/noise_kabut.png";
            var tex=ProcTex.FromHeight("NoiseKabut",128,ProcTex.HeightMap(128,(u,v)=>ProcTex.Fbm(u,v,4,4,2026)),ProcTex.Palet(new[]{Color.black,Color.white}));
            File.WriteAllBytes(path,tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);AturImpor(path,false,false,128);
            var sh=AmbilShaderAK("AlasKeramat/KabutLokal");
            if(sh==null)throw new System.Exception("Shader kabut G3 tidak tersedia");
            var mat=new Material(sh) { name="M_KabutLokal" };mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(path));
            mat=SimpanMat(mat);
            var t=new Transform[AtmosferHutan.JumlahKartu];var r=new Renderer[t.Length];
            for(int i=0;i<t.Length;i++)
            {
                var card=GameObject.CreatePrimitive(PrimitiveType.Quad);card.name="KabutLapis_"+i;card.transform.SetParent(go.transform,false);card.transform.localScale=new Vector3(24f,6f,1f);
                Object.DestroyImmediate(card.GetComponent<Collider>());t[i]=card.transform;r[i]=card.GetComponent<Renderer>();r[i].sharedMaterial=mat;r[i].shadowCastingMode=ShadowCastingMode.Off;r[i].receiveShadows=false;
            }
            var atmos=go.AddComponent<AtmosferHutan>();atmos.Pasang(t,r);EditorUtility.SetDirty(atmos);
            var cam=Object.FindFirstObjectByType<AlasKeramat.Player.AndiFPSController>();
            if(cam!=null && cam.Kamera!=null)atmos.Terapkan(cam.Kamera.GetComponent<Camera>(),9f,WeatherState.Cerah);
            Catat("G3a: bulan terbaca, gua tidak hitam, grading netral, bayangan 2 kaskade, kabut lokal 12 kartu & basah dinamis");
        }
    }
}
