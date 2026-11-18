using AlasKeramat.Inventory;
using UnityEngine;
namespace AlasKeramat.Core
{
    // Pintu Baja Bersandi D1 (03 §10 UG2): pembaca kartu LED merah/hijau, perlu Kartu Akses Bunker.
    public class PintuKartu : MonoBehaviour, IInteractable
    {
        [SerializeField] GameObject daun;
        [SerializeField] Light led;
        int _versi = -1;
        public void Pasang(GameObject daunPintu, Light lampu) { daun = daunPintu; led = lampu; }
        public float HoldDuration { get { return 0.4f; } }
        WorldCaveAndSaveManager M { get { return GameServices.Get<WorldCaveAndSaveManager>(); } }
        public string GetPromptText(GameObject actor) { var m = M; return m != null && !m.Ada(WorldCaveAndSaveManager.B_PintuKartu) ? "GESEK KARTU" : ""; }
        public bool CanInteract(GameObject actor) { var m = M; return m != null && !m.Ada(WorldCaveAndSaveManager.B_PintuKartu); }
        public void Interact(GameObject actor)
        {
            var m = M; if (m == null) return;
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv == null || !inv.Punya(ItemID.KartuAksesBunker)) { GameEvents.RaiseToast("Lampu merah. Butuh Kartu Akses Bunker."); return; }
            m.Set(WorldCaveAndSaveManager.B_PintuKartu);
            GameEvents.RaiseToast("Bip. Pintu baja terbuka.");
        }
        void Update()
        {
            var m = M; if (m == null || m.Versi == _versi) return;
            _versi = m.Versi;
            bool buka = m.Ada(WorldCaveAndSaveManager.B_PintuKartu);
            if (daun != null) daun.SetActive(!buka);
            if (led != null) led.color = buka ? Color.green : Color.red;
        }
    }
}
