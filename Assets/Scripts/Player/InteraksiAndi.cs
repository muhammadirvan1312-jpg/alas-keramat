using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Player
{
    // Interaksi dari tengah kamera (09 §3): SphereCast 2,5 m r 0,15 ke Interactable/Tree/LogPhysics/Structure; air dangkal di depan = MINUM AIR.
    public class InteraksiAndi : MonoBehaviour
    {
        public const float Jarak = 2.5f, Radius = 0.15f, LangkahAir = 0.5f, HidrasiTeguk = 20f;
        const string TeksMinum = "MINUM AIR";

        public string Prompt { get; private set; } = "";
        public bool AdaTarget { get { return Prompt.Length > 0; } }

        AndiFPSController _ctrl;
        IInteractable _target;
        bool _air, _bangun;
        int _mask;
        Building.ShoulderLogAndFreeformBuilder _pembangun;

        void Awake()
        {
            _ctrl = GetComponent<AndiFPSController>();
            _mask = (1 << GameLayers.Interactable) | (1 << GameLayers.Tree) | (1 << GameLayers.LogPhysics) | (1 << GameLayers.Structure);
            GameServices.Register(this);
        }

        void Update()
        {
            var cam = _ctrl != null ? _ctrl.Kamera : null;
            if (cam == null) return;
            _target = null; _air = false;
            RaycastHit hit;
            if (Physics.SphereCast(cam.position, Radius, cam.forward, out hit, Jarak, _mask, QueryTriggerInteraction.Collide))
            {
                var t = hit.collider.GetComponentInParent<IInteractable>();
                if (t != null && t.CanInteract(gameObject)) _target = t;
            }
            if (_target == null) _air = CariAir(cam.position, cam.forward);
            string p = _target != null ? _target.GetPromptText(gameObject) : (_air ? TeksMinum : "");
            if (_pembangun == null) _pembangun = GameServices.Get<Building.ShoulderLogAndFreeformBuilder>();
            _bangun = _pembangun != null && _pembangun.Prompt.Length > 0 && !(_target is Building.KayuFisik);
            if (_bangun) p = _pembangun.Prompt;
            if (p != Prompt) Prompt = p;
        }

        static bool CariAir(Vector3 asal, Vector3 arah)
        {
            for (float d = LangkahAir; d <= Jarak; d += LangkahAir)
            {
                Vector3 q = asal + arah * d;
                float? wl = WorldTerrainGenerator.GetWaterLevelAt(q.x, q.z);
                if (wl.HasValue && q.y <= wl.Value + 0.1f && wl.Value > WorldTerrainGenerator.SampleHeight(q.x, q.z)) return true;
            }
            return false;
        }

        // Dipanggil tombol TANGAN atau tombol E (Editor).
        public void Tekan()
        {
            try
            {
                if (_bangun && _pembangun != null) { _pembangun.PasangSekarang(); return; }
                if (_target != null) { _target.Interact(gameObject); return; }
                if (_air)
                {
                    var v = GetComponent<IVitalAndi>();
                    if (v != null) v.Minum(HidrasiTeguk, true);
                    GameEvents.RaiseToast("Kamu meneguk air sungai");
                }
            }
            catch (System.Exception e) { Debug.LogError("[AK][InteraksiAndi] Interaksi gagal: " + e.Message); }
        }

#if ENABLE_LEGACY_INPUT_MANAGER
        void LateUpdate() { if (Input.GetKeyDown(KeyCode.E)) Tekan(); }
#endif
    }
}
