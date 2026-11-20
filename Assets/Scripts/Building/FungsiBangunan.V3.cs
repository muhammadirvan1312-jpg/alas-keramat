using System.Collections.Generic;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;
namespace AlasKeramat.Building
{
    public partial class FungsiBangunan
    {
        public const int KapPeti=40, KapBubu=4;
        public const float LamaMinyak=480f, IntervalJerat=2f, PeluangJerat=0.3f;
        public List<ItemStack> IsiPeti = new List<ItemStack>();
        public int JumlahPeti { get { int n=0; foreach(var s in IsiPeti)n+=s.count; return n; } }
        public static readonly List<FungsiBangunan> Semua = new List<FungsiBangunan>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetV3() { Semua.Clear(); }
        void OnEnable() { if(!Semua.Contains(this))Semua.Add(this); }
        void OnDisable() { Semua.Remove(this); AlasKeramat.AI.ZonaPenangkal.SetObor(this, Vector3.zero, false); }
        public static bool MenangkalTuyul(Vector3 posisi)
        {
            foreach(var f in Semua) if(f!=null && f.Jenis==BlueprintID.MangkukKepiting && (f.transform.position-posisi).sqrMagnitude<=100f)return true;
            return false;
        }
        public static float TakutSekte(Vector3 posisi)
        {
            float hasil=0f;
            foreach(var f in Semua) if(f!=null && f.NyalaSisa>0f)
            {
                float r=f.Jenis==BlueprintID.PatungNgeri?35f:f.Jenis==BlueprintID.OborTengkorak?25f:0f;
                if(r>0f && (f.transform.position-posisi).sqrMagnitude<=r*r)hasil=Mathf.Max(hasil,f.Jenis==BlueprintID.PatungNgeri?50f:40f);
            }
            return hasil;
        }
        public bool MinyakAwal=true;
        readonly Collider[] _temanGerbang=new Collider[8];
        double _jamProduksi=-1;
        AndiFPSController _duduk;
        [System.Serializable] class IsiBangunanT6
        {
            public int stok; public bool siap, terbuka, minyakAwal; public float nyala, tAir; public double jamProduksi;
            public ItemID[] jemur; public float[] tJemur; public List<ItemStack> peti;
        }
        public string CaptureIsi() { return JsonUtility.ToJson(new IsiBangunanT6 { minyakAwal=MinyakAwal, stok=Stok, siap=Siap, terbuka=Terbuka, nyala=NyalaSisa,tAir=_tAir,jemur=_jemur,tJemur=_tJemur,peti=IsiPeti,jamProduksi=_jamProduksi }); }
        public void RestoreIsi(string json)
        {
            var d=JsonUtility.FromJson<IsiBangunanT6>(json); if(d==null)return;
            MinyakAwal=d.minyakAwal; Stok=d.stok; Siap=d.siap; Terbuka=d.terbuka; NyalaSisa=d.nyala; _tAir=d.tAir; _jamProduksi=d.jamProduksi;
            IsiPeti=d.peti ?? new List<ItemStack>();
            for(int i=0;i<SlotJemur;i++) { _jemur[i]=d.jemur[i]; _tJemur[i]=d.tJemur[i]; }
        }
        public int SimpanItem(TikarPandan3DInventory inv, ItemID id, int jumlah)
        {
            if(inv==null || jumlah<=0 || !ItemDatabase.Has(id))return 0;
            int n=Mathf.Min(jumlah,Mathf.Min(KapPeti-JumlahPeti,inv.Jumlah(id)));
            if(n<=0 || !inv.Kurangi(id,n))return 0;
            for(int i=0;i<IsiPeti.Count;i++) if(IsiPeti[i].id==id) { var s=IsiPeti[i]; s.count+=n; IsiPeti[i]=s; return n; }
            IsiPeti.Add(new ItemStack { id=id,count=n }); return n;
        }
        public int AmbilItem(TikarPandan3DInventory inv, ItemID id, int jumlah)
        {
            if(inv==null || jumlah<=0)return 0;
            for(int i=0;i<IsiPeti.Count;i++) if(IsiPeti[i].id==id)
            {
                var s=IsiPeti[i]; int n=inv.Tambah(id,Mathf.Min(jumlah,s.count)); s.count-=n;
                if(s.count==0)IsiPeti.RemoveAt(i); else IsiPeti[i]=s; return n;
            }
            return 0;
        }
        void UpdateV3(float dt)
        {
            var jam=GameServices.Get<TimeOfDayClock>();
            if(Jenis==BlueprintID.BubuIkan || Jenis==BlueprintID.JeratHewan || Jenis==BlueprintID.KebunTanam)
            {
                double kini=jam!=null ? (jam.Hari-1)*24d+jam.Jam : 0d;
                if(_jamProduksi<0 || kini<_jamProduksi)_jamProduksi=kini;
                double interval=Jenis==BlueprintID.BubuIkan?1d:Jenis==BlueprintID.KebunTanam?2d:IntervalJerat;
                while(kini-_jamProduksi>=interval)
                {
                    _jamProduksi+=interval;
                    if(Jenis==BlueprintID.BubuIkan) Stok=Mathf.Min(KapBubu,Stok+1);
                    else if(Jenis==BlueprintID.KebunTanam) Stok=Mathf.Min(4,Stok+1);
                    else if(Stok==0 && Random.value<PeluangJerat)Stok=1;
                }
            }
            var andi=GameServices.Get<AndiFPSController>();
            if(Jenis==BlueprintID.GerbangPagar)
            {
                Terbuka=andi!=null && Vector3.Distance(transform.position,andi.transform.position)<3f;
                if(!Terbuka)Terbuka=Physics.OverlapSphereNonAlloc(transform.position,3f,_temanGerbang,1<<GameLayers.Companion,QueryTriggerInteraction.Collide)>0;
            }
            var v=GameServices.Get<AndiVitalsAndSanity>();
            if(Jenis==BlueprintID.LampuMinyak && NyalaSisa>0f && v!=null && (v.transform.position-transform.position).sqrMagnitude<100f)v.UbahSanity(0.1f*dt);
            if(_duduk!=null && _duduk.Duduk && (_duduk.transform.position-transform.position).sqrMagnitude<4f && v!=null)v.IstirahatDuduk(dt);
        }
        string PromptV3(GameObject actor)
        {
            switch(Jenis)
            {
                case BlueprintID.PetiPenyimpanan:return "BUKA PETI";
                case BlueprintID.LampuMinyak:return NyalaSisa>0f?"PADAMKAN LAMPU":"NYALAKAN LAMPU";
                case BlueprintID.PatungNgeri:return NyalaSisa>0f?"":"NYALAKAN PATUNG";
                case BlueprintID.MangkukKepiting:return "PERIKSA MANGKUK";
                case BlueprintID.BatuAsah:return "ASAH";
                case BlueprintID.RumahPohon:return "NAIK RUMAH POHON";
                case BlueprintID.PerangkapPunji:return Siap?"":"SETEL ULANG";
                case BlueprintID.BubuIkan:return Stok>0?"AMBIL IKAN":"";
                case BlueprintID.JeratHewan:return Stok>0?"AMBIL HASIL JERAT":"";
                case BlueprintID.GerbangPagar:return Terbuka?"TUTUP GERBANG":"BUKA GERBANG";
                case BlueprintID.KursiMeja:return "DUDUK";
                case BlueprintID.PondokKayu:return "TIDUR DI PONDOK";
                case BlueprintID.PagarKayu:return "";
                case BlueprintID.KebunTanam:return Stok>0?"PANEN KEBUN ("+Stok+"/4)":"KEBUN (MENUNGGU PANEN)";
                case BlueprintID.MejaKerja:return "ASAH & PULIHKAN DI MEJA";
                case BlueprintID.KomporBatu:
                    var invK=GameServices.Get<TikarPandan3DInventory>();
                    if(invK!=null && invK.Punya(ItemID.BotolKosong))return "REBUS AIR BERSIH";
                    if(invK!=null && (invK.Punya(ItemID.DagingMentah) || invK.Punya(ItemID.IkanMentah)))return "MASAK DAGING / IKAN";
                    return "NYALAKAN TUNGKU BATU";
                default:return "";
            }
        }
        void InteraksiV3(GameObject actor)
        {
            var inv=GameServices.Get<TikarPandan3DInventory>();
            var ctrl=actor!=null?actor.GetComponent<AndiFPSController>():null;
            switch(Jenis)
            {
                case BlueprintID.PetiPenyimpanan:
                    var ui=GameServices.Get<AlasKeramat.UI.PetiUI>(); if(ui!=null)ui.Buka(this); break;
                case BlueprintID.LampuMinyak:
                    if(NyalaSisa>0f)NyalaSisa=0f;
                    else if(inv!=null && inv.Punya(ItemID.KorekGas) && (MinyakAwal || inv.Kurangi(ItemID.MinyakTanah,1))) { NyalaSisa=LamaMinyak; MinyakAwal=false; }
                    else GameEvents.RaiseToast("Butuh minyak tanah dan korek gas"); break;
                case BlueprintID.PatungNgeri:
                    if(inv!=null && inv.Kurangi(ItemID.Damar,1))NyalaSisa=LamaMinyak; break;
                case BlueprintID.MangkukKepiting: GameEvents.RaiseToast("Kepiting menjaga isi kemah dari Tuyul"); break;
                case BlueprintID.BatuAsah:
                    var c=GameServices.Get<CombatAndLeftHandController>(); if(c!=null)c.Asah(); break;
                case BlueprintID.RumahPohon:
                    var b=GetComponentInParent<IdentitasBangunan>(); if(ctrl!=null && b!=null)ctrl.Teleport(b.transform.TransformPoint(new Vector3(0f,4.2f,1f))); break;
                case BlueprintID.PerangkapPunji:Siap=true;break;
                case BlueprintID.BubuIkan:case BlueprintID.JeratHewan:
                    if(inv!=null && Stok>0) Stok-=inv.Tambah(Jenis==BlueprintID.BubuIkan?ItemID.IkanMentah:ItemID.DagingMentah,Stok);break;
                case BlueprintID.GerbangPagar:Terbuka=!Terbuka;break;
                case BlueprintID.KursiMeja:
                    if(ctrl!=null){ _duduk=ctrl; ctrl.DudukDi(transform.position+Vector3.up*0.1f); }break;
                case BlueprintID.PondokKayu:
                    Tidur(true); break;
                case BlueprintID.KebunTanam:
                    if(Stok<=0){ GameEvents.RaiseToast("Tanaman belum siap dipanen"); break; }
                    if(inv!=null){ inv.Tambah(ItemID.Pisang,2); inv.Tambah(ItemID.DaunSirih,1); inv.Tambah(ItemID.KunyitObat,1); Stok--; GameEvents.RaiseToast("Panen kebun: +2 Pisang, Sirih & Kunyit"); }
                    break;
                case BlueprintID.MejaKerja:
                    var cm=GameServices.Get<CombatAndLeftHandController>(); if(cm!=null)cm.Asah();
                    var vk=GameServices.Get<AndiVitalsAndSanity>(); if(vk!=null)vk.UbahSanity(10f);
                    break;
                case BlueprintID.KomporBatu:
                    NyalaSisa=LamaMinyak;
                    if(inv!=null && inv.Kurangi(ItemID.BotolKosong,1)) { inv.Tambah(ItemID.BotolAirBersih,1); GameEvents.RaiseToast("Air direbus: +1 Botol Air Bersih"); }
                    else if(inv!=null && inv.Kurangi(ItemID.DagingMentah,1)) { inv.Tambah(ItemID.DagingMatang,1); GameEvents.RaiseToast("Daging matang!"); }
                    else if(inv!=null && inv.Kurangi(ItemID.IkanMentah,1)) { inv.Tambah(ItemID.IkanMatang,1); GameEvents.RaiseToast("Ikan matang!"); }
                    else GameEvents.RaiseToast("Tungku batu menyala hangat");
                    break;
            }
        }
    }
}
