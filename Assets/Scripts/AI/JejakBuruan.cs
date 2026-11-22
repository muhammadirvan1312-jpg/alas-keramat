using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.UI;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Jejak cakar/tulang pemangsa di hutan: dilacak untuk menemukan arah & jarak ke Sarang Pemangsa.
    public class JejakBuruan : MonoBehaviour, IInteractable
    {
        [SerializeField] Vector3 tujuanSarang;
        [SerializeField] string namaPemangsa = "Harimau";

        public float HoldDuration { get { return 0f; } }
        public void Pasang(Vector3 sarang, string nama) { tujuanSarang = sarang; namaPemangsa = nama; }
        public string GetPromptText(GameObject actor) { return "LACAK JEJAK " + namaPemangsa.ToUpperInvariant(); }
        public bool CanInteract(GameObject actor) { return true; }

        public static string FormatPetunjuk(Vector3 asal, Vector3 tujuan, string nama)
        {
            Vector3 d = tujuan - asal; d.y = 0f;
            float sudut = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            return "Jejak " + nama + " mengarah ke " + KompasSuhuHUD.ArahKardinal(sudut) + " (~" + Mathf.RoundToInt(d.magnitude) + " m).";
        }

        public void Interact(GameObject actor)
        {
            GameEvents.RaiseToast(FormatPetunjuk(transform.position, tujuanSarang, namaPemangsa));
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.Play(ProceduralAudioSynth.KlipGeramSekte, tujuanSarang, AudioCategory.Efek, 0.55f, 0.72f, 2, 85f);
        }
    }
}
