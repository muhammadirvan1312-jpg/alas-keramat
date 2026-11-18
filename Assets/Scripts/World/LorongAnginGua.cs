using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;
namespace AlasKeramat.World
{
    public class LorongAnginGua : MonoBehaviour
    {
        public const float SanityGelapPerDetik=0.6f;
        void OnTriggerStay(Collider other)
        {
            var tangan=other.GetComponent<TanganKiri>(); if(tangan==null)return;
            tangan.PadamkanObor();
            if(!tangan.SenterMenyala)
            { var vital=other.GetComponent<AndiVitalsAndSanity>(); if(vital!=null)vital.UbahSanity(-SanityGelapPerDetik*Time.fixedDeltaTime); }
        }
        void OnTriggerEnter(Collider other)
        { if(other.GetComponent<AndiFPSController>()!=null)GameEvents.RaiseToast("Angin memadamkan obor. Gunakan senter."); }
    }
}
