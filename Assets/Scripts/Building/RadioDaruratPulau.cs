using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Stasiun radio lapangan: memutar transmisi frekuensi darurat & membuka petunjuk lokasi bunker/candi/ekspedisi.
    public class RadioDaruratPulau : MonoBehaviour, IInteractable, ISaveable
    {
        static readonly string[] Transmisi =
        {
            "[RADIO 142.8 MHz] '...tim ekspedisi terjebak di Bunker UG2 sebelah utara sungai...'",
            "[RADIO 144.2 MHz] '...jangan padamkan api saat Maghrib. Sesuatu mengawasi dari puncak Beringin...'",
            "[RADIO 146.5 MHz] '...candi tua di barat laut menyimpan jalan menuju altar kuno...'",
            "[RADIO 148.0 MHz] '...bangkai kapal di pantai barat masih membawa logistik medis...'"
        };

        [SerializeField] int indeksTransmisi;
        [SerializeField] MeshRenderer lampuIndikator;
        bool _diputar;

        public bool SudahDiputar { get { return _diputar; } }
        public float HoldDuration { get { return 0.3f; } }
        public string SaveKey { get { return "radio_darurat_" + indeksTransmisi; } }

        public void Pasang(int idx, MeshRenderer lampu)
        {
            indeksTransmisi = Mathf.Abs(idx) % Transmisi.Length;
            lampuIndikator = lampu;
            Segarkan();
        }

        public static string TeksTransmisi(int idx)
        {
            return Transmisi[Mathf.Abs(idx) % Transmisi.Length];
        }

        public string GetPromptText(GameObject actor) { return _diputar ? "PUTAR ULANG RADIO DARURAT" : "TERIMA SINYAL RADIO DARURAT"; }
        public bool CanInteract(GameObject actor) { return true; }

        public void Interact(GameObject actor)
        {
            _diputar = true;
            Segarkan();
            GameEvents.RaiseSubtitle(TeksTransmisi(indeksTransmisi));
            GameEvents.RaiseToast("Transmisi radio diterima.");
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.Play(ProceduralAudioSynth.KlipKentongan, transform.position, AudioCategory.Efek, 0.45f, 1.35f, 2, 24f);
        }

        void Segarkan()
        {
            if (lampuIndikator != null) lampuIndikator.enabled = _diputar;
        }

        [System.Serializable] struct DataRadio { public bool p; }
        public string CaptureJson() { return JsonUtility.ToJson(new DataRadio { p = _diputar }); }
        public void RestoreJson(string json) { if (!string.IsNullOrEmpty(json)) { _diputar = JsonUtility.FromJson<DataRadio>(json).p; Segarkan(); } }
    }
}
