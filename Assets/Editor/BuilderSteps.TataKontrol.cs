using AlasKeramat.Core;
using AlasKeramat.UI;
using UnityEditor;
using UnityEngine;
namespace AlasKeramat.EditorTools
{
    public static partial class NusantaraMasterBuilder
    {
        // Dua kolom aksi; utilitas di kiri supaya ibu jari kanan punya ruang melihat.
        static void LangkahTataKontrol()
        {
            var canvas=_ui!=null?_ui.transform.Find("Canvas_HUD"):null;
            var kontrol=canvas!=null?canvas.Find("Kontrol"):null;
            if(kontrol==null)return;
            string[] nama={"TombolTebas","TombolLompat","TombolGanti","TombolTangkis","TombolTangan","TombolMode","TombolJongkok","TombolLempar","TombolObor","TombolTas","TombolBuku"};
            var posisi=new[]{new Vector2(-150,170),new Vector2(-150,420),new Vector2(450,-500),new Vector2(-430,110),new Vector2(-430,340),new Vector2(-430,550),new Vector2(-150,610),new Vector2(-430,750),new Vector2(620,-500),new Vector2(110,-500),new Vector2(280,-500)};
            for(int i=0;i<nama.Length;i++)
            {
                var t=kontrol.Find(nama[i]);if(t==null)t=canvas.Find(nama[i]);
                if(t==null) { Debug.LogError("[AK][TataKontrol] Tombol tidak ada: "+nama[i]); continue; }
                var rt=(RectTransform)t;rt.SetParent(kontrol,false);
                rt.anchorMin=rt.anchorMax=(i<9 && i!=2 && i!=8)?new Vector2(1,0):new Vector2(0,1);
                rt.anchoredPosition=posisi[i];
                if(nama[i]=="TombolLompat" || nama[i]=="TombolJongkok" || nama[i]=="TombolTangan" || nama[i]=="TombolTangkis")
                    if(t.GetComponent<GeserKameraDariTombol>()==null) t.gameObject.AddComponent<GeserKameraDariTombol>();
                EditorUtility.SetDirty(rt);
            }
            var subtitle=canvas.Find("Subtitle");
            var tangan = kontrol.Find("TombolTangan");
            if (tangan != null)
            {
                var go = UIGambar(tangan.gameObject, "ProgresTahan", SpriteUI("ui_cincin", true), new Vector2(176, 176), new Vector2(0.5f, 0.5f), Vector2.zero);
                var img = go.GetComponent<UnityEngine.UI.Image>(); img.raycastTarget = false;
                img.type = UnityEngine.UI.Image.Type.Filled; img.fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
                img.fillOrigin = 2; img.fillAmount = 0f; img.color = ProcTex.Hex("#FFF4CA"); img.enabled = false;
                var tc = kontrol.GetComponent<TouchControls>(); if (tc != null) { tc.PasangProgres(img); EditorUtility.SetDirty(tc); }
            }
            var sabuk = canvas.Find("SabukCepat");
            if (sabuk != null) sabuk.SetParent(kontrol, false);
            if(subtitle!=null)
            {
                var teks=subtitle.GetComponent<UnityEngine.UI.Text>();
                var rt=(RectTransform)subtitle;rt.SetParent(kontrol,false);rt.anchorMin=rt.anchorMax=new Vector2(0.5f,0);
                rt.anchoredPosition=new Vector2(0,700);rt.sizeDelta=new Vector2(1000,96);
                if(teks!=null) { teks.fontSize=30; EditorUtility.SetDirty(teks); }
                EditorUtility.SetDirty(rt);
            }
            Catat("Kontrol aksi berjarak, TAS/BUKU ikut safe area, subtitle di atas area aksi");
        }
    }
}
