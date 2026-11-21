using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.Player
{
    // Tangan kiri (12 §T3): Obor Bambu menyala dengan Korek Gas, memberi cahaya hangat berkedip di malam hari.
    public class TanganKiri : MonoBehaviour
    {
        public const float JangkauanObor = 20f, IntensitasObor = 4.2f, KedipObor = 0.28f, LajuKedip = 7f;
        static readonly Vector3 PosisiObor = new Vector3(-0.32f, -0.34f, 0.48f);
        static readonly Color WarnaObor = new Color(1f, 0.64f, 0.32f);

        [SerializeField] Light lampu;

        public bool OborMenyala { get; private set; }
        TikarPandan3DInventory _inv;
        Transform _oborModel, _apiVisual;
        AndiFPSController _ctrl;

        public void Pasang(Light l) { lampu = l; }

        void Awake()
        {
            GameServices.Register(this);
            _ctrl = GetComponent<AndiFPSController>();
            if (lampu == null && _ctrl != null && _ctrl.Kamera != null)
            {
                var go = new GameObject("CahayaObor");
                go.transform.SetParent(_ctrl.Kamera, false);
                go.transform.localPosition = PosisiObor + new Vector3(0f, 0.22f, 0.1f);
                lampu = go.AddComponent<Light>();
                lampu.type = LightType.Point; lampu.range = JangkauanObor; lampu.color = WarnaObor; lampu.shadows = LightShadows.None;
            }
            if (_oborModel == null && _ctrl != null && _ctrl.Kamera != null) BuatModelObor(_ctrl.Kamera);
            if (lampu != null) lampu.enabled = false;
            SetVisualObor(false);
        }

        void BuatModelObor(Transform kam)
        {
            var akar = new GameObject("OborFP").transform;
            akar.SetParent(kam, false);
            akar.localPosition = PosisiObor;
            akar.localRotation = Quaternion.Euler(24f, 6f, -12f);
            akar.gameObject.layer = GameLayers.Player;
            Bagian(akar, PrimitiveType.Cylinder, new Vector3(0f, 0.08f, 0f), new Vector3(0.032f, 0.18f, 0.032f), SafeMaterialFactory.GetCached("FP_Bambu", new Color(0.42f, 0.34f, 0.16f), 0f, 0.25f));
            Bagian(akar, PrimitiveType.Cylinder, new Vector3(0f, 0.24f, 0f), new Vector3(0.044f, 0.055f, 0.044f), SafeMaterialFactory.GetCached("FP_KainMinyak", new Color(0.18f, 0.11f, 0.07f), 0f, 0.15f));
            var baraMat = SafeMaterialFactory.CreatePBR("FP_Bara", new Color(1f, 0.45f, 0.1f), 0f, 0.2f, null, null, new Color(3.2f, 1.1f, 0.25f));
            var bara = Bagian(akar, PrimitiveType.Sphere, new Vector3(0f, 0.30f, 0f), new Vector3(0.052f, 0.068f, 0.052f), baraMat);
            _apiVisual = bara.transform;
            _oborModel = akar;
        }

        static GameObject Bagian(Transform induk, PrimitiveType tipe, Vector3 pos, Vector3 skala, Material mat)
        {
            var g = GameObject.CreatePrimitive(tipe);
            Destroy(g.GetComponent<Collider>());
            g.layer = GameLayers.Player;
            g.transform.SetParent(induk, false);
            g.transform.localPosition = pos; g.transform.localScale = skala;
            var r = g.GetComponent<Renderer>(); r.sharedMaterial = mat; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return g;
        }

        void SetVisualObor(bool aktif)
        {
            if (_oborModel != null && _oborModel.gameObject.activeSelf != aktif) _oborModel.gameObject.SetActive(aktif);
        }

        public bool BisaObor()
        {
            if (_inv == null) _inv = GameServices.Get<TikarPandan3DInventory>();
            return _inv != null && _inv.Punya(ItemID.OborBambu);
        }

        // Lorong Angin (03 §10 UG3 K1): angin memadamkan obor.
        public void Padamkan() { if (!OborMenyala) return; OborMenyala = false; if (lampu != null) lampu.enabled = false; SetVisualObor(false); GameEvents.RaiseToast("Angin memadamkan obor"); }

        public void ToggleObor()
        {
            if (OborMenyala) { OborMenyala = false; if (lampu != null) lampu.enabled = false; SetVisualObor(false); return; }
            if (!BisaObor()) { GameEvents.RaiseToast("Kamu tidak punya obor"); return; }
            if (!_inv.Punya(ItemID.KorekGas)) { GameEvents.RaiseToast("Butuh Korek Gas untuk menyalakan obor"); return; }
            OborMenyala = true;
            if (lampu != null) lampu.enabled = true;
            SetVisualObor(true);
        }

        void Update()
        {
            if (!OborMenyala || lampu == null) return;
            float n = Mathf.PerlinNoise(Time.time * LajuKedip, 0.37f);
            lampu.intensity = IntensitasObor * (1f - KedipObor + KedipObor * 2f * n);
            if (_apiVisual != null) _apiVisual.localScale = new Vector3(0.048f, 0.062f + n * 0.025f, 0.048f);
            if (_oborModel != null && _ctrl != null)
            {
                float ayun = Mathf.Sin(Time.time * 5.5f) * Mathf.Clamp01(_ctrl.KecepatanDatar / 4f) * 0.012f;
                _oborModel.localPosition = PosisiObor + new Vector3(ayun * 0.5f, Mathf.Abs(ayun), 0f);
            }
        }

#if ENABLE_LEGACY_INPUT_MANAGER && (UNITY_EDITOR || UNITY_STANDALONE)
        void LateUpdate() { if (Input.GetKeyDown(KeyCode.F)) ToggleObor(); }
#endif
    }
}
