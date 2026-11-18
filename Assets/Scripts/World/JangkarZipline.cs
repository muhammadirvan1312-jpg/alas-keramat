using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;
namespace AlasKeramat.World
{
    public class JangkarZipline : MonoBehaviour, IInteractable, ISaveable
    {
        public const float JarakMaks=60f;
        [SerializeField] JangkarZipline pasangan;
        [SerializeField] LineRenderer tali;
        [SerializeField] Vector3 berangkat, tiba;
        [SerializeField] string kunci;
        public bool Terpasang { get; private set; }
        public string SaveKey { get { return kunci; } }
        public float HoldDuration { get { return 0f; } }
        public void Pasang(JangkarZipline ujung,LineRenderer garis,Vector3 asal,Vector3 akhir,string id)
        { pasangan=ujung;tali=garis;berangkat=asal;tiba=akhir;kunci=id; }
        public string CaptureJson() { return Terpasang?"1":"0"; }
        public void RestoreJson(string json) { Terpasang=json=="1"; if(tali!=null)tali.enabled=Terpasang; }
        public bool BisaBidik(GameObject actor)
        { return !Terpasang && pasangan!=null && actor!=null && Vector3.Distance(actor.transform.position,pasangan.berangkat)<3f; }
        public bool CanInteract(GameObject actor) { return pasangan!=null && actor!=null; }
        public string GetPromptText(GameObject actor) { return Terpasang?"MELUNCUR":"TEMBAK TALI"; }
        public void Interact(GameObject actor)
        {
            if(pasangan==null || actor==null)return;
            if(!Terpasang)
            {
                var inv=GameServices.Get<TikarPandan3DInventory>();
                if(inv==null || !inv.Punya(ItemID.ZiplineGun) || !inv.Punya(ItemID.TaliZipline)) { GameEvents.RaiseToast("Butuh Zipline Gun dan Tali Zipline");return; }
                Vector3 a=berangkat+Vector3.up*2.2f,b=tiba+Vector3.up*2.2f;
                if(Vector3.Distance(a,b)>JarakMaks || Physics.Linecast(a,b,(1<<GameLayers.Ground)|(1<<GameLayers.Structure),QueryTriggerInteraction.Ignore))
                { GameEvents.RaiseToast("Jalur tali terhalang atau terlalu jauh"); return; }
                if(!inv.Kurangi(ItemID.TaliZipline,1))return;
                RestoreJson("1");pasangan.RestoreJson("1");GameEvents.RaiseToast("Tali terpasang. Dekati jangkar untuk meluncur.");return;
            }
            var ctrl=actor.GetComponent<AndiFPSController>();if(ctrl!=null)ctrl.MulaiZipline(berangkat,tiba);
        }
    }
}
