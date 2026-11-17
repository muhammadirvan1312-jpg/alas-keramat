using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.Player
{
    // Tangan kiri (12 §T3): Obor Bambu menyala dengan Korek Gas, memberi cahaya hangat berkedip di malam hari.
    public class TanganKiri : MonoBehaviour
    {
        public const float JangkauanObor = 14f, IntensitasObor = 2.4f, KedipObor = 0.35f, LajuKedip = 7f;
        static readonly Vector3 PosisiObor = new Vector3(-0.35f, -0.25f, 0.55f);
        static readonly Color WarnaObor = new Color(1f, 0.62f, 0.3f);

        [SerializeField] Light lampu;

        public bool OborMenyala { get; private set; }
        TikarPandan3DInventory _inv;

        public void Pasang(Light l) { lampu = l; }

        void Awake()
        {
            GameServices.Register(this);
            if (lampu == null)
            {
                var ctrl = GetComponent<AndiFPSController>();
                if (ctrl != null && ctrl.Kamera != null)
                {
                    var go = new GameObject("CahayaObor");
                    go.transform.SetParent(ctrl.Kamera, false);
                    go.transform.localPosition = PosisiObor;
                    lampu = go.AddComponent<Light>();
                    lampu.type = LightType.Point; lampu.range = JangkauanObor; lampu.color = WarnaObor; lampu.shadows = LightShadows.None;
                }
            }
            if (lampu != null) lampu.enabled = false;
        }

        public bool BisaObor()
        {
            if (_inv == null) _inv = GameServices.Get<TikarPandan3DInventory>();
            return _inv != null && _inv.Punya(ItemID.OborBambu);
        }

        public void ToggleObor()
        {
            if (OborMenyala) { OborMenyala = false; if (lampu != null) lampu.enabled = false; return; }
            if (!BisaObor()) { GameEvents.RaiseToast("Kamu tidak punya obor"); return; }
            if (!_inv.Punya(ItemID.KorekGas)) { GameEvents.RaiseToast("Butuh Korek Gas untuk menyalakan obor"); return; }
            OborMenyala = true;
            if (lampu != null) lampu.enabled = true;
        }

        void Update()
        {
            if (!OborMenyala || lampu == null) return;
            float n = Mathf.PerlinNoise(Time.time * LajuKedip, 0.37f);
            lampu.intensity = IntensitasObor * (1f - KedipObor + KedipObor * 2f * n);
        }

#if ENABLE_LEGACY_INPUT_MANAGER
        void LateUpdate() { if (Input.GetKeyDown(KeyCode.F)) ToggleObor(); }
#endif
    }
}
