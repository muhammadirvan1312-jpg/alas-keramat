using AlasKeramat.Inventory;
using UnityEngine;
namespace AlasKeramat.Core
{
    // Tanah Gembur Bertanda Silang (03 Zona 4): gali 4x dengan Cangkul -> palka baja bunker muncul (gerbang keras 04 §4).
    public class TanahGali : MonoBehaviour, IInteractable
    {
        public const float TurunPerGali = 0.18f;
        [SerializeField] Transform gundukan;
        [SerializeField] GameObject palka;
        int _versi = -1;
        Vector3 _posAwal;
        bool _siap;

        public void Pasang(Transform tanah, GameObject pintuPalka) { gundukan = tanah; palka = pintuPalka; }
        public float HoldDuration { get { return 1f; } }
        WorldCaveAndSaveManager M { get { return GameServices.Get<WorldCaveAndSaveManager>(); } }

        public string GetPromptText(GameObject actor)
        {
            var m = M; if (m == null || m.Ada(WorldCaveAndSaveManager.B_Palka)) return "";
            return "GALI (" + m.Gali + "/" + WorldCaveAndSaveManager.GaliPerlu + ")";
        }
        public bool CanInteract(GameObject actor) { var m = M; return m != null && !m.Ada(WorldCaveAndSaveManager.B_Palka); }
        public void Interact(GameObject actor)
        {
            var m = M; if (m == null) return;
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv == null || !inv.Punya(ItemID.Cangkul)) { GameEvents.RaiseToast("Tanahnya gembur. Butuh Cangkul untuk menggali."); return; }
            m.TambahGali();
            var a = GameServices.Get<ProceduralAudioSynth>();
            if (a != null) a.Play(ProceduralAudioSynth.KlipTebang, transform.position, AudioCategory.Efek, 1f, 0.55f, 2);
            if (m.Ada(WorldCaveAndSaveManager.B_Palka)) GameEvents.RaiseToast("Logam! Ada pintu palka baja di bawah tanah.");
        }

        void Update()
        {
            var m = M; if (m == null || m.Versi == _versi) return;
            _versi = m.Versi;
            if (!_siap && gundukan != null) { _posAwal = gundukan.localPosition; _siap = true; }
            bool buka = m.Ada(WorldCaveAndSaveManager.B_Palka);
            if (gundukan != null) { gundukan.localPosition = _posAwal - Vector3.up * TurunPerGali * m.Gali; gundukan.gameObject.SetActive(!buka); }
            if (palka != null) palka.SetActive(buka);
            var c = GetComponent<Collider>(); if (c != null) c.enabled = !buka;
        }
    }
}
