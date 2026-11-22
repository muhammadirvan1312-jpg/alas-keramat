using AlasKeramat.Inventory;
using UnityEngine;
namespace AlasKeramat.Core
{
    // Tanah Gembur Bertanda Silang (03 Zona 4): gali 4x dengan Cangkul -> palka baja bunker muncul (gerbang keras 04 §4).
    public class TanahGali : MonoBehaviour, IInteractable
    {
        public const float TurunPerGali = 0.18f;
        [SerializeField] Transform gundukan;

        public static int JumlahPartikelGali(bool tembusPalka) { return tembusPalka ? 26 : 16; }

        void PancarTanahGali(bool tembusPalka)
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("SemburanGali");
            go.transform.position = transform.position + Vector3.up * 0.2f;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var m = ps.main;
            m.playOnAwake = false; m.loop = false; m.startLifetime = 0.6f; m.startSpeed = 3.2f;
            m.startSize = tembusPalka ? 0.07f : 0.09f; m.gravityModifier = 1.6f;
            m.simulationSpace = ParticleSystemSimulationSpace.World;
            Color w = tembusPalka ? new Color(1f, 0.72f, 0.28f) : new Color(0.36f, 0.24f, 0.14f);
            m.startColor = w;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 35f; sh.radius = 0.25f;
            var pr = go.GetComponent<ParticleSystemRenderer>();
            pr.sharedMaterial = SafeMaterialFactory.GetCached(tembusPalka ? "M_PercikPalka" : "M_TanahGali", w, 0f, 0.2f);
            pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ps.Emit(JumlahPartikelGali(tembusPalka));
            Destroy(go, 1.2f);
        }
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
            PancarTanahGali(m.Ada(WorldCaveAndSaveManager.B_Palka));
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
