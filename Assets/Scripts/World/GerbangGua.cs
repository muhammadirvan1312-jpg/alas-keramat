using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;
namespace AlasKeramat.World
{
    public enum JenisGerbangGua { Masuk, Keluar, Gali, Kartu, Selam, Reruntuhan }
    public class GerbangGua : MonoBehaviour, IInteractable, ISaveable
    {
        public const int PukulanBunker=4;
        [SerializeField] JenisGerbangGua jenis;
        [SerializeField] Vector3 tujuan;
        [SerializeField] GameObject penghalang;
        [SerializeField] string kunci;
        [SerializeField] bool sisiPositif;
        public JenisGerbangGua Jenis { get { return jenis; } }
        public Vector3 Tujuan { get { return tujuan; } }
        public int Pukulan { get; private set; }
        public bool Terbuka { get; private set; }
        public string SaveKey { get { return kunci; } }
        public float HoldDuration { get { return jenis==JenisGerbangGua.Gali && !Terbuka ? 0.6f : 0f; } }
        [System.Serializable] class KeadaanGerbangT10 { public int pukulan; public bool terbuka; }
        public void Pasang(JenisGerbangGua j,Vector3 p,string id,GameObject blok=null,bool positif=false) { jenis=j;tujuan=p;kunci=id;penghalang=blok;sisiPositif=positif; Terapkan(); }
        public string CaptureJson() { return JsonUtility.ToJson(new KeadaanGerbangT10 { pukulan=Pukulan,terbuka=Terbuka }); }
        public void RestoreJson(string json)
        {
            var d=JsonUtility.FromJson<KeadaanGerbangT10>(json); if(d==null)return;
            Pukulan=Mathf.Clamp(d.pukulan,0,PukulanBunker); Terbuka=d.terbuka; Terapkan();
        }
        void Terapkan()
        {
            if(penghalang!=null)penghalang.SetActive(!Terbuka);
            if(jenis==JenisGerbangGua.Gali) { var r=GetComponent<Renderer>(); if(r!=null)r.enabled=Terbuka; }
        }
        public string GetPromptText(GameObject actor)
        {
            switch(jenis)
            {
                case JenisGerbangGua.Gali: return Terbuka?"MASUK BUNKER":"GALI BUNKER ("+Pukulan+"/4)";
                case JenisGerbangGua.Kartu:return "BUKA PINTU KARTU";
                case JenisGerbangGua.Selam:return "PASANG ALAT SELAM";
                case JenisGerbangGua.Reruntuhan:return "DORONG RERUNTUHAN";
                case JenisGerbangGua.Keluar:return "KELUAR KE PERMUKAAN";
                default:return "MASUK GUA";
            }
        }
        public bool CanInteract(GameObject actor)
        { return actor!=null && (!Terbuka || jenis==JenisGerbangGua.Masuk || jenis==JenisGerbangGua.Keluar || jenis==JenisGerbangGua.Gali); }
        public bool CobaBuka(bool punyaAlat,bool sisiBenar)
        {
            if(Terbuka)return true;
            if(!punyaAlat || !sisiBenar)return false;
            if(jenis==JenisGerbangGua.Gali) { Pukulan=Mathf.Min(PukulanBunker,Pukulan+1); Terbuka=Pukulan>=PukulanBunker; }
            else Terbuka=true;
            Terapkan(); return Terbuka;
        }
        public void Interact(GameObject actor)
        {
            if(actor==null)return;
            var andi=actor.GetComponent<AndiFPSController>(); if(andi==null)return;
            var inv=GameServices.Get<TikarPandan3DInventory>();
            ItemID alat=jenis==JenisGerbangGua.Gali?ItemID.Cangkul:jenis==JenisGerbangGua.Kartu?ItemID.KartuAksesBunker:jenis==JenisGerbangGua.Selam?ItemID.AlatSelamRebreather:ItemID.None;
            bool punya=alat==ItemID.None || (inv!=null && inv.Punya(alat));
            if(!Terbuka && jenis!=JenisGerbangGua.Masuk && jenis!=JenisGerbangGua.Keluar)
            {
                bool sisi=jenis!=JenisGerbangGua.Reruntuhan || (actor.transform.position.x>transform.position.x)==sisiPositif;
                if(!punya) { GameEvents.RaiseToast("Butuh "+ItemDatabase.Get(alat).displayName); return; }
                CobaBuka(punya,sisi);
                if(jenis==JenisGerbangGua.Gali)GameEvents.RaiseToast(Terbuka?"Palka bunker terbuka":"Tanah mulai terbuka");
                return;
            }
            if(jenis==JenisGerbangGua.Masuk || jenis==JenisGerbangGua.Keluar || jenis==JenisGerbangGua.Gali)
            {
                var dunia=GameServices.Get<WorldCaveAndSaveManager>();
                if(dunia!=null)dunia.Pindah(andi,tujuan,jenis!=JenisGerbangGua.Keluar);
            }
        }
    }
}
