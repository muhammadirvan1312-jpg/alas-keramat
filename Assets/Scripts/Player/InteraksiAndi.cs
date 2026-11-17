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
        IInteractable _ditahan;
        float _lamaTahan;
        public void MulaiTahan()
        {
            if (_bangun || _target == null || _target.HoldDuration <= 0f) { Tekan(); return; }
            _ditahan = _target; _lamaTahan = 0f;
        }
        public void LepasTahan() { _ditahan = null; _lamaTahan = 0f; }
        void OnDisable() { LepasTahan(); }

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
            if (_ctrl.RakitAktif != null || _ctrl.Duduk) { LepasTahan(); _target=null; _air=false; _bangun=false; Prompt=_ctrl.Duduk ? "BERDIRI" : "TURUN"; return; }
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
            if (_ditahan != null)
            {
                if (!ReferenceEquals(_ditahan, _target) || _bangun) LepasTahan();
                else
                {
                    _lamaTahan += Time.deltaTime;
                    if (_lamaTahan >= _ditahan.HoldDuration)
                    {
                        var selesai = _ditahan; LepasTahan(); selesai.Interact(gameObject);
                    }
                }
            }
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
                if (_ctrl != null && _ctrl.RakitAktif != null) { _ctrl.TurunRakit(); return; }
                if (_ctrl != null && _ctrl.Duduk) { _ctrl.Berdiri(); return; }
                if (_bangun && _pembangun != null) { _pembangun.PasangSekarang(); return; }
                if (_target != null) { if (_target.HoldDuration <= 0f) _target.Interact(gameObject); return; }
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
        void LateUpdate() { if (Input.GetKeyDown(KeyCode.E)) MulaiTahan(); if (Input.GetKeyUp(KeyCode.E)) LepasTahan(); }
#endif
    }
}
