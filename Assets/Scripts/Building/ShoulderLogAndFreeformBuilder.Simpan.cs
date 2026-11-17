using AlasKeramat.Core;
using UnityEngine;
namespace AlasKeramat.Building
{
    public partial class ShoulderLogAndFreeformBuilder : ISaveable
    {
        [System.Serializable] class BahuSimpanT6 { public Muatan muatan; public int jumlah; public ModeBangun mode; public Color[] warna; }
        public string SaveKey { get { return "bahu"; } }
        public string CaptureJson()
        {
            var warna=new Color[Jumlah]; for(int i=0;i<Jumlah;i++)warna[i]=_mat[i]!=null?_mat[i].color:Color.white;
            return JsonUtility.ToJson(new BahuSimpanT6 { muatan=Bawa,jumlah=Jumlah,mode=ModeAktif,warna=warna });
        }
        public void RestoreJson(string json)
        {
            var d=JsonUtility.FromJson<BahuSimpanT6>(json); if(d==null || d.warna==null)return;
            Jumlah=Mathf.Clamp(d.jumlah,0,d.muatan==Muatan.Batu?1:d.muatan==Muatan.Gelondong?MaksGelondong:MaksPapan);
            Bawa=Jumlah==0?Muatan.Kosong:d.muatan; ModeAktif=d.mode;
            for(int i=0;i<Jumlah && i<d.warna.Length;i++)_mat[i]=SafeMaterialFactory.GetCached("Bahu_"+ColorUtility.ToHtmlStringRGBA(d.warna[i]),d.warna[i]);
            Visual(); Prompt="";
        }
    }
}
