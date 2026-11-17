using AlasKeramat.Core;
using UnityEngine;
using UnityEngine.UI;
namespace AlasKeramat.UI
{
    public class HUDMisi : MonoBehaviour
    {
        public Text Kartu;
        float _jeda;
        void Update()
        {
            _jeda-=Time.unscaledDeltaTime; if(_jeda>0f)return; _jeda=0.25f;
            var q=GameServices.Get<QuestManager>(); if(q==null || Kartu==null)return;
            string teks=q.Kartu(); if(Kartu.text!=teks)Kartu.text=teks;
        }
        public void Simpan() { var s=GameServices.Get<SaveSystem>(); if(s!=null)s.Simpan(); }
        public void Muat() { var s=GameServices.Get<SaveSystem>(); if(s!=null && !s.Muat())GameEvents.RaiseToast("Belum ada simpanan yang bisa dimuat"); }
    }
}
