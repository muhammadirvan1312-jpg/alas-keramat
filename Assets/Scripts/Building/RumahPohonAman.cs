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

    // Kasur tikar di rumah pohon: tidur 6 jam memulihkan HP/Stamina/Sanity, menyembuhkan luka & simpan otomatis.
    public class KasurRumahPohon : MonoBehaviour, IInteractable
    {
        public float HoldDuration { get { return 0.6f; } }
        public string GetPromptText(GameObject actor) { return "TIDUR & SIMPAN (6 JAM)"; }
        public bool CanInteract(GameObject actor) { return true; }

        public void Interact(GameObject actor)
        {
            var vital = GameServices.Get<AndiVitalsAndSanity>();
            if (vital != null) vital.Pulihkan(6f, true);
            var save = GameServices.Get<SaveSystem>();
            if (save != null) save.Simpan();
            GameEvents.RaiseToast("Istirahat aman di rumah pohon. Kondisi pulih & progres tersimpan.");
        }
    }
}
