using AlasKeramat.Core;
using AlasKeramat.UI;
using UnityEditor;
using UnityEngine;
namespace AlasKeramat.EditorTools
{
    public static partial class NusantaraMasterBuilder
    {
        // Tiga kolom aksi, disisakan jarak bahkan pada ukuran tombol 130%.
        static void LangkahTataKontrol()
        {
            var canvas=_ui!=null?_ui.transform.Find("Canvas_HUD"):null;
            var kontrol=canvas!=null?canvas.Find("Kontrol"):null;
            if(kontrol==null)return;
            string[] nama={"TombolTebas","TombolLompat","TombolGanti","TombolTangkis","TombolTangan","TombolMode","TombolJongkok","TombolLempar","TombolObor","TombolTas","TombolBuku"};
            var posisi=new[]{new Vector2(-150,170),new Vector2(-150,420),new Vector2(-150,610),new Vector2(-430,110),new Vector2(-430,340),new Vector2(-430,550),new Vector2(-660,110),new Vector2(-660,300),new Vector2(-660,490),new Vector2(110,-500),new Vector2(280,-500)};
            for(int i=0;i<nama.Length;i++)
            {
                var t=kontrol.Find(nama[i]);if(t==null)t=canvas.Find(nama[i]);
                if(t==null) { Debug.LogError("[AK][TataKontrol] Tombol tidak ada: "+nama[i]); continue; }
                var rt=(RectTransform)t;rt.SetParent(kontrol,false);
                rt.anchorMin=rt.anchorMax=i<9?new Vector2(1,0):new Vector2(0,1);
                rt.anchoredPosition=posisi[i];EditorUtility.SetDirty(rt);
            }
            var subtitle=canvas.Find("Subtitle");
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
