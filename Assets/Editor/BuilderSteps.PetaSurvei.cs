using AlasKeramat.Core;
using AlasKeramat.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
namespace AlasKeramat.EditorTools
{
    public static partial class NusantaraMasterBuilder
    {
        static void LangkahPetaSurvei()
        {
            var kanvas=_ui.transform.Find("Canvas_HUD");if(kanvas==null)return;
            var root=UIStretch(kanvas.gameObject,"NavigasiPeta",Vector2.zero,Vector2.one);
            var menu=kanvas.Find("LayarMenuT11");if(menu!=null)root.transform.SetSiblingIndex(menu.GetSiblingIndex());
            var diagnostik=Anak(_sistem,"DiagnostikMain").AddComponent<DiagnostikMain>();diagnostik.Commit=NusantaraCI.SourceCommit();EditorUtility.SetDirty(diagnostik);
            var nav=root.AddComponent<PetaSurveiUI>();var font=ProceduralAssetFactory.GetSafeFont();
            nav.Kompas=BuatTeks(root,"ArahTujuan","CARI GPS DI KOPER DARURAT",font,24,new Vector2(0.5f,1f),new Vector2(0,-105f),new Vector2(1100f,45f));
            nav.Kompas.raycastTarget=false;nav.Kompas.gameObject.AddComponent<Shadow>().effectDistance=new Vector2(2,-2);
            Text teks;var tombol=TombolPanel(root,"BukaPetaSurvei","PETA",font,new Vector2(-180f,-60f),new Vector2(140f,65f),ProcTex.Hex("#314639"),out teks);
            var rt=(RectTransform)tombol.transform;rt.anchorMin=rt.anchorMax=new Vector2(0.5f,1f);rt.anchoredPosition=new Vector2(150,-45);
            UnityEventTools.AddPersistentListener(tombol.onClick,nav.Toggle);
            nav.Panel=UIStretch(root,"PanelPetaSurvei",Vector2.zero,Vector2.one);nav.Panel.AddComponent<Image>().color=ProcTex.Hex("#111A16F8");
            const int n=512;var tex=new Texture2D(n,n,TextureFormat.RGB24,false);var warna=new Color[n*n];
            for(int z=0;z<n;z++)for(int x=0;x<n;x++)
            {
                Vector2 p=PetaSurveiUI.KeDunia(new Vector2((x+0.5f)/n,(z+0.5f)/n));
                float h=WorldTerrainGenerator.SampleHeight(p.x,p.y);var air=WorldTerrainGenerator.GetWaterLevelAt(p.x,p.y);
                Color c=h<=0f || (air.HasValue && h<air.Value)?ProcTex.Hex("#3B7684"):Color.Lerp(ProcTex.Hex("#557447"),ProcTex.Hex("#C9B793"),Mathf.Clamp01(h/150f));
                if(h>0f && Mathf.Repeat(h,15f)<0.8f)c*=0.76f;
                warna[z*n+x]=c;
            }
            tex.SetPixels(warna);tex.Apply();string path=FolderTex+"/peta_survei.png";SimpanPng(tex,path);AssetDatabase.ImportAsset(path);AturImpor(path,false,false,n);
            var map=Anak(nav.Panel,"BidangPeta");var mr=map.AddComponent<RectTransform>();mr.anchorMin=mr.anchorMax=new Vector2(0.5f,0.5f);mr.sizeDelta=new Vector2(760,760);
            var raw=map.AddComponent<RawImage>();raw.texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);nav.Bidang=mr;
            for(int i=0;i<PetaSurveiUI.Lokasi.Length;i++)
            {
                var label=BuatTeks(map,"Lokasi"+i,"+ "+PetaSurveiUI.NamaLokasi[i],font,19,PetaSurveiUI.KeUV(PetaSurveiUI.Lokasi[i]),Vector2.zero,new Vector2(180,38));
                label.color=Color.white;label.raycastTarget=false;label.gameObject.AddComponent<Shadow>().effectDistance=new Vector2(1,-1);
            }
            nav.Andi=KotakPenandaPeta(map,"PosisiAndi",Color.red,14f);
            nav.Tujuan=KotakPenandaPeta(map,"TujuanPeta",Color.yellow,12f);nav.Tujuan.anchorMin=nav.Tujuan.anchorMax=PetaSurveiUI.KeUV(PetaSurveiUI.Lokasi[0]);
            nav.Info=BuatTeks(nav.Panel,"PetunjukPeta","",font,24,new Vector2(0.5f,1f),new Vector2(0,-65),new Vector2(1600,90));nav.Info.raycastTarget=false;
            var tutup=TombolPanel(nav.Panel,"TutupPetaSurvei","KEMBALI",font,new Vector2(0,50),new Vector2(280,70),ProcTex.Hex("#36563A"),out teks);UnityEventTools.AddPersistentListener(tutup.onClick,nav.Tutup);
            nav.Panel.SetActive(false);EditorUtility.SetDirty(nav);
            Catat("Peta Survei: kontur, sungai, legenda lokasi, posisi GPS & tujuan ketuk");
        }
        static RectTransform KotakPenandaPeta(GameObject map,string nama,Color warna,float size)
        {
            var go=Anak(map,nama);var rt=go.AddComponent<RectTransform>();rt.anchorMin=rt.anchorMax=Vector2.one*0.5f;rt.sizeDelta=Vector2.one*size;
            var img=go.AddComponent<Image>();img.color=warna;img.raycastTarget=false;return rt;
        }
    }
}
