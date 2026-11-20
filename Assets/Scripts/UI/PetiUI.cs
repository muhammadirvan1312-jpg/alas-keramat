using System.Collections.Generic;
using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;
using UnityEngine.UI;
namespace AlasKeramat.UI
{
    public class PetiUI : MonoBehaviour
    {
        public GameObject Panel;
        public Text Judul;
        public Text[] LabelTas, LabelPeti;
        readonly List<ItemID> _tas=new List<ItemID>(), _peti=new List<ItemID>();
        FungsiBangunan _buka;
        int _halaman;
        void Awake() { PengunciInput.Pasang(Panel);  GameServices.Register(this); }
        public void Buka(FungsiBangunan b) { _buka=b; _halaman=0; Panel.SetActive(true); Segarkan(); }
        public void Tutup() { Panel.SetActive(false); _buka=null; }
        public void Berikut() { _halaman++; Segarkan(); }
        public void Sebelum() { _halaman=Mathf.Max(0,_halaman-1); Segarkan(); }
        public void SimpanBaris(int i) { var inv=GameServices.Get<TikarPandan3DInventory>(); int k=_halaman*LabelTas.Length+i; if(_buka!=null && k<_tas.Count)_buka.SimpanItem(inv,_tas[k],1); Segarkan(); }
        public void AmbilBaris(int i) { var inv=GameServices.Get<TikarPandan3DInventory>(); int k=_halaman*LabelPeti.Length+i; if(_buka!=null && k<_peti.Count)_buka.AmbilItem(inv,_peti[k],1); Segarkan(); }
        void Segarkan()
        {
            if(_buka==null) { Tutup(); return; }
            var inv=GameServices.Get<TikarPandan3DInventory>(); if(inv==null)return;
            _tas.Clear(); _peti.Clear(); foreach(var s in inv.Isi)if(s.Value>0)_tas.Add(s.Key); foreach(var s in _buka.IsiPeti)if(s.count>0)_peti.Add(s.id);
            _tas.Sort(); _peti.Sort(); _halaman=Mathf.Clamp(_halaman,0,Mathf.Max(0,(Mathf.Max(_tas.Count,_peti.Count)-1)/LabelTas.Length));
            Judul.text="PETI ("+_buka.JumlahPeti+"/40) - HALAMAN "+(_halaman+1)+"\nKetuk item: pindahkan 1 unit";
            for(int i=0;i<LabelTas.Length;i++)
            {
                int k=_halaman*LabelTas.Length+i;
                LabelTas[i].text=k<_tas.Count?ItemDatabase.Get(_tas[k]).displayName+" x"+inv.Jumlah(_tas[k]):"-";
                int n=0; if(k<_peti.Count)foreach(var s in _buka.IsiPeti)if(s.id==_peti[k])n=s.count;
                LabelPeti[i].text=k<_peti.Count?ItemDatabase.Get(_peti[k]).displayName+" x"+n:"-";
            }
        }
        void Update() { if(Panel!=null && Panel.activeSelf && _buka==null)Tutup(); }
    }
}
