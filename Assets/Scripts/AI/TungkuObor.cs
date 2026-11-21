using AlasKeramat.Core;
using UnityEngine;
namespace AlasKeramat.AI
{
    // Tungku obor merah Kubah Akar (03 §10 UG3 K3). Pada kesulitan Damai menjadi Teka-teki Akar (06 §10): nyalakan I-II-III-IV berurutan.
    public class TungkuObor : MonoBehaviour, IInteractable
    {
        public const float IntensitasNyala = 50f;
        [SerializeField] Light lampu;
        [SerializeField] GameObject api;
        [SerializeField] int simbol;          // 1-4 = I-IV (teka-teki), 0 = hiasan
        [SerializeField] KiRogoPanggung pengatur;
        public bool Menyala { get; private set; }
        public int Simbol { get { return simbol; } }
        public static readonly string[] Romawi = { "", "I", "II", "III", "IV" };

        public void Pasang(Light l, GameObject nyala, int sim, KiRogoPanggung ki) { lampu = l; api = nyala; simbol = sim; pengatur = ki; }
        void Awake() { Padamkan(); }
        public void Nyalakan() { Menyala = true; if (lampu != null) { lampu.enabled = true; lampu.intensity = IntensitasNyala; } if (api != null) api.SetActive(true); }
        public void Padamkan() { Menyala = false; if (lampu != null) lampu.enabled = false; if (api != null) api.SetActive(false); }

        public float HoldDuration { get { return 0.5f; } }
        bool ModeTekaTeki { get { return pengatur != null && pengatur.TekaTekiAktif && simbol > 0; } }
        public string GetPromptText(GameObject actor) { return ModeTekaTeki && !Menyala ? "NYALAKAN TUNGKU " + Romawi[Mathf.Clamp(simbol, 0, 4)] : ""; }
        public bool CanInteract(GameObject actor) { return ModeTekaTeki && !Menyala; }
        public void Interact(GameObject actor) { if (ModeTekaTeki) pengatur.TungkuDinyalakan(this); }
    }
}
