using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;
namespace AlasKeramat.Player
{
    public class DedenTutorial : MonoBehaviour, IInteractable
    {
        public float HoldDuration { get { return 0f; } }
        public string GetPromptText(GameObject actor)
        {
            var q=GameServices.Get<QuestManager>(); if(q==null)return "";
            if(q.Progres.catatan) { var ai=GetComponentInParent<AlasKeramat.AI.KangDedenKelvinAI>(); return ai!=null?ai.GetPromptText(actor):""; } // T8: sesudah tutorial -> perintah Kang Deden
            return q.Progres.jamu?"TUNJUKKAN CATATAN":"BERI JAMU";
        }
        public bool CanInteract(GameObject actor) { return GetPromptText(actor).Length>0; }
        public void Interact(GameObject actor)
        {
            var q=GameServices.Get<QuestManager>(); var inv=GameServices.Get<TikarPandan3DInventory>(); if(q==null || inv==null)return;
            if(q.Progres.catatan) { var ai=GetComponentInParent<AlasKeramat.AI.KangDedenKelvinAI>(); if(ai!=null)ai.Interact(actor); return; }
            if(!q.Progres.jamu)
            {
                if(inv.Kurangi(ItemID.JamuHerbal,1)) { q.BeriJamu(); GameEvents.RaiseToast("Kang Deden mulai pulih"); }
                else GameEvents.RaiseToast("Rakit Jamu Herbal: Daun Sirih + Kunyit");
            }
            else if(inv.Punya(ItemID.BukuCatatan)) { q.TunjukkanCatatan(); GameEvents.RaiseToast("Kang Deden memahami catatanmu"); }
            else GameEvents.RaiseToast("Ambil Buku Catatan dari koper");
        }
    }
}
