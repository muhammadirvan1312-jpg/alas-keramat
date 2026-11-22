using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Tangga panjat rumah pohon tinggi (6 m di atas tanah): aman dari sekte & hewan darat.
    public class TanggaRumahPohon : MonoBehaviour, IInteractable
    {
        [SerializeField] Vector3 tujuanPosisi;
        [SerializeField] bool arahNaik = true;

        public float HoldDuration { get { return 0f; } }
        public void Pasang(Vector3 tujuan, bool naik) { tujuanPosisi = tujuan; arahNaik = naik; }
        public string GetPromptText(GameObject actor) { return arahNaik ? "PANJAT RUMAH POHON" : "TURUN TANGGA"; }
        public bool CanInteract(GameObject actor) { return true; }

        public void Interact(GameObject actor)
        {
            var andi = GameServices.Get<AndiFPSController>();
            if (andi == null) return;
            var cc = andi.GetComponent<CharacterController>();
            andi.HentikanInputGerak();
            if (cc != null) cc.enabled = false;
            andi.transform.position = tujuanPosisi;
            if (cc != null) cc.enabled = true;
            GameEvents.RaiseToast(arahNaik ? "Naik ke rumah pohon aman." : "Turun ke tanah.");
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.PlayItemPickup();
        }
    }
}
