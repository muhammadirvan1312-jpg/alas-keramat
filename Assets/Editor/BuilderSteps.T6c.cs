using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using AlasKeramat.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
namespace AlasKeramat.EditorTools
{
    public static partial class NusantaraMasterBuilder
    {
        static void LangkahT6c()
        {
            Anak(_sistem,"SimpanDunia").AddComponent<SimpanDunia>();
            Anak(_sistem,"QuestManager").AddComponent<QuestManager>();
            var koper=Object.FindObjectsByType<WadahItem>(FindObjectsSortMode.None);
            foreach(var k in koper) if(k.name.StartsWith("Koper")) { k.KunciSimpan="koper_"+k.name; EditorUtility.SetDirty(k); }
            var deden=GameObject.Find("KangDeden");
            if(deden!=null)
            {
                var interaksi=Anak(deden,"InteraksiDeden"); interaksi.layer=GameLayers.Interactable; interaksi.tag=GameTags.Interactable;
                var c=interaksi.AddComponent<CapsuleCollider>(); c.center=Vector3.up*0.8f; c.height=1.6f; c.radius=0.45f;
                interaksi.AddComponent<DedenTutorial>();
            }
            var kanvas=_ui.transform.Find("Canvas_HUD"); if(kanvas==null)return;
            var font=ProceduralAssetFactory.GetSafeFont(); Text teks;
            var kartu=BuatTeks(kanvas.gameObject,"KartuMisi","Ambil Koper Darurat (0/3)",font,19,new Vector2(1f,1f),new Vector2(0f,-156f),new Vector2(540f,60f));
            kartu.rectTransform.anchorMin=kartu.rectTransform.anchorMax=new Vector2(0.5f,1f);
            kartu.color=ProcTex.Hex("#E8C468"); kartu.raycastTarget=false;
            kartu.gameObject.AddComponent<Shadow>().effectDistance=new Vector2(1.5f,-1.5f);
            var hud=kartu.gameObject.AddComponent<HUDMisi>(); hud.Kartu=kartu;
            var simpan=TombolPanel(kanvas.gameObject,"TombolSimpan","SIMPAN",font,new Vector2(-80f,50f),new Vector2(130f,55f),new Color(0.055f,0.075f,0.095f,0.78f),out teks);
            var muat=TombolPanel(kanvas.gameObject,"TombolMuat","MUAT",font,new Vector2(80f,50f),new Vector2(130f,55f),new Color(0.055f,0.075f,0.095f,0.78f),out teks);
            UnityEventTools.AddPersistentListener(simpan.onClick,hud.Simpan); UnityEventTools.AddPersistentListener(muat.onClick,hud.Muat);
            var builder=Object.FindFirstObjectByType<ShoulderLogAndFreeformBuilder>();
            var batu=TombolPanel(kanvas.gameObject,"TombolBatu","PANGGUL BATU",font,new Vector2(-280f,50f),new Vector2(210f,55f),new Color(0.055f,0.075f,0.095f,0.78f),out teks);
            if(builder!=null)UnityEventTools.AddPersistentListener(batu.onClick,builder.PanggulBatu);
            var toolbar=new[]{simpan,muat,batu};
            for(int i=0;i<toolbar.Length;i++)
            {
                var rt=(RectTransform)toolbar[i].transform;rt.anchorMin=rt.anchorMax=new Vector2(0f,1f);
                rt.anchoredPosition=i<2?new Vector2(110f+i*150f,-260f):new Vector2(185f,-335f);
            }
            var bahu=GameObject.Find("BahuKiri");
            if(bahu!=null)
            {
                var model=GameObject.CreatePrimitive(PrimitiveType.Sphere); model.name="Batu0";
                Object.DestroyImmediate(model.GetComponent<Collider>()); model.transform.SetParent(bahu.transform,false);
                model.transform.localScale=new Vector3(0.6f,0.4f,0.6f); model.GetComponent<Renderer>().sharedMaterial=AmbilMat("M_Batu"); model.SetActive(false);
            }
            BuatPanelPeti(kanvas.gameObject,font);
            // Batu Besar tersedia dekat titik awal; wadah tetap tersimpan agar tidak bisa digandakan lewat MUAT.
            for(int i=0;i<12;i++)
            {
                var p=SpawnAndi+new Vector3(5f+(i%4)*1.2f,0f,4f+(i/4)*1.2f); p.y=WorldTerrainGenerator.SampleHeight(p.x,p.z)+0.2f;
                var g=GameObject.CreatePrimitive(PrimitiveType.Sphere); g.name="BatuBesar_"+i; g.transform.SetParent(_root.transform,false); g.transform.position=p; g.transform.localScale=new Vector3(0.6f,0.4f,0.6f);
                g.layer=GameLayers.Interactable; g.tag=GameTags.Interactable;
                g.GetComponent<Renderer>().sharedMaterial=AmbilMat("M_Batu");
                var w=g.AddComponent<WadahItem>(); w.Pasang("BATU BESAR",new[]{ItemID.BatuBesar},new[]{1},0,true,null); w.KunciSimpan="batu_besar_"+i;
            }
            Catat("T6c: dunia tersimpan, 26 blueprint, rakit, bongkar dan misi M01-M04");
        }
        static void BuatPanelPeti(GameObject kanvas,Font font)
        {
            var root=UIStretch(kanvas,"PetiUI",Vector2.zero,Vector2.one); var ui=root.AddComponent<PetiUI>();
            var panel=UIStretch(root,"TikarPeti",new Vector2(0.15f,0.15f),new Vector2(0.85f,0.85f));
            panel.AddComponent<Image>().color=ProcTex.Hex("#3A2A18F5"); ui.Panel=panel;
            ui.Judul=BuatTeks(panel,"JudulPeti","PETI",font,28,new Vector2(0.5f,1f),new Vector2(0f,-55f),new Vector2(900f,90f));
            ui.LabelTas=new Text[6]; ui.LabelPeti=new Text[6];
            BuatTeks(panel,"KolomTas","TAS ANDI",font,24,new Vector2(0.5f,1f),new Vector2(-270f,-115f),new Vector2(450f,40f));
            BuatTeks(panel,"KolomPeti","ISI PETI",font,24,new Vector2(0.5f,1f),new Vector2(270f,-115f),new Vector2(450f,40f));
            for(int i=0;i<6;i++)
            {
                var a=TombolPanel(panel,"SimpanItem"+i,"-",font,Vector2.zero,new Vector2(450f,65f),ProcTex.Hex("#6B4A2A"),out ui.LabelTas[i]);
                var b=TombolPanel(panel,"AmbilItem"+i,"-",font,Vector2.zero,new Vector2(450f,65f),ProcTex.Hex("#4B5A2A"),out ui.LabelPeti[i]);
                foreach(var tombol in new[]{a,b}) { var rt=(RectTransform)tombol.transform; rt.anchorMin=rt.anchorMax=new Vector2(0.5f,1f); rt.anchoredPosition=new Vector2(tombol==a?-270f:270f,-170f-i*70f); }
                UnityEventTools.AddIntPersistentListener(a.onClick,ui.SimpanBaris,i); UnityEventTools.AddIntPersistentListener(b.onClick,ui.AmbilBaris,i);
            }
            Text t;
            var kiri=TombolPanel(panel,"HalamanSebelum","SEBELUM",font,new Vector2(-270f,50f),new Vector2(200f,60f),Color.gray,out t);
            var kanan=TombolPanel(panel,"HalamanBerikut","BERIKUT",font,new Vector2(270f,50f),new Vector2(200f,60f),Color.gray,out t);
            var tutup=TombolPanel(panel,"TutupPeti","TUTUP",font,new Vector2(0f,50f),new Vector2(200f,60f),Color.gray,out t);
            UnityEventTools.AddPersistentListener(kiri.onClick,ui.Sebelum); UnityEventTools.AddPersistentListener(kanan.onClick,ui.Berikut); UnityEventTools.AddPersistentListener(tutup.onClick,ui.Tutup);
            panel.SetActive(false); EditorUtility.SetDirty(ui);
        }
    }
}
