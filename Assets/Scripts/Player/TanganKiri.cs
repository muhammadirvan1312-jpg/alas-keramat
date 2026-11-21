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
        ParticleSystem _baraObor;

        public static float LajuBaraObor(bool menyala, float kecepatanDatar)
        {
            if (!menyala) return 0f;
            return Mathf.Clamp(6f + Mathf.Max(0f, kecepatanDatar) * 2.2f, 6f, 20f);
        }

        void PerbaruiBaraObor()
        {
            if (OborMenyala && _apiVisual != null)
            {
                if (_baraObor == null)
                {
                    var go = new GameObject("BaraOborFP");
                    go.layer = GameLayers.Player;
                    go.transform.SetParent(_apiVisual, false);
                    go.transform.localPosition = new Vector3(0f, 0.04f, 0f);
                    _baraObor = go.AddComponent<ParticleSystem>();
                    var m = _baraObor.main;
                    m.loop = true; m.simulationSpace = ParticleSystemSimulationSpace.World;
                    m.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.85f);
                    m.startSpeed = new ParticleSystem.MinMaxCurve(0.35f, 0.85f);
                    m.startSize = new ParticleSystem.MinMaxCurve(0.012f, 0.026f);
                    m.gravityModifier = -0.22f; m.maxParticles = 24;
                    m.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.78f, 0.25f, 0.95f), new Color(1f, 0.36f, 0.08f, 0.80f));
                    var sh = _baraObor.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.035f;
                    var pr = go.GetComponent<ParticleSystemRenderer>();
                    pr.sharedMaterial = SafeMaterialFactory.CreatePBR("M_BaraOborFP", new Color(1f, 0.58f, 0.16f), 0f, 0.2f, null, null, new Color(3.4f, 1.35f, 0.25f));
                    pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; pr.receiveShadows = false;
                }
                var em = _baraObor.emission;
                em.rateOverTime = LajuBaraObor(true, _ctrl != null ? _ctrl.KecepatanDatar : 0f);
                if (!_baraObor.isPlaying) _baraObor.Play();
            }
            else if (_baraObor != null && _baraObor.isPlaying) _baraObor.Stop();
        }

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
            var lenganMat = SafeMaterialFactory.GetCached("FP_LenganKiri", new Color(0.23f, 0.29f, 0.20f), 0f, 0.35f);
            var kulitMat = SafeMaterialFactory.GetCached("FP_KulitKiri", new Color(0.72f, 0.53f, 0.39f), 0f, 0.32f);
            Bagian(akar, PrimitiveType.Cylinder, new Vector3(-0.03f, -0.22f, -0.09f), new Vector3(0.084f, 0.13f, 0.084f), lenganMat).transform.localRotation = Quaternion.Euler(26f, 0f, -18f);
            Bagian(akar, PrimitiveType.Cylinder, new Vector3(-0.01f, -0.05f, -0.03f), new Vector3(0.068f, 0.10f, 0.068f), kulitMat).transform.localRotation = Quaternion.Euler(14f, 0f, -10f);
            Bagian(akar, PrimitiveType.Sphere, new Vector3(0f, 0.03f, 0f), new Vector3(0.072f, 0.078f, 0.072f), kulitMat);
            for (int f = 0; f < 4; f++)
                Bagian(akar, PrimitiveType.Capsule, new Vector3(-0.025f, 0.02f + f * 0.016f, 0.026f), new Vector3(0.018f, 0.032f, 0.018f), kulitMat).transform.localRotation = Quaternion.Euler(0f, 0f, 74f);

            var bambuMat = SafeMaterialFactory.GetCached("FP_Bambu", new Color(0.42f, 0.34f, 0.16f), 0f, 0.25f);
            Bagian(akar, PrimitiveType.Cylinder, new Vector3(0f, 0.08f, 0f), new Vector3(0.032f, 0.18f, 0.032f), bambuMat);
            Bagian(akar, PrimitiveType.Cylinder, new Vector3(0f, 0.12f, 0f), new Vector3(0.038f, 0.009f, 0.038f), SafeMaterialFactory.GetCached("FP_RuasBambu", new Color(0.28f, 0.22f, 0.11f), 0f, 0.2f));
            Bagian(akar, PrimitiveType.Cylinder, new Vector3(0f, 0.24f, 0f), new Vector3(0.044f, 0.055f, 0.044f), SafeMaterialFactory.GetCached("FP_KainMinyak", new Color(0.18f, 0.11f, 0.07f), 0f, 0.15f));
            var baraMat = SafeMaterialFactory.CreatePBR("FP_Bara", new Color(1f, 0.45f, 0.1f), 0f, 0.2f, null, null, new Color(3.2f, 1.1f, 0.25f));
            var bara = Bagian(akar, PrimitiveType.Sphere, new Vector3(0f, 0.30f, 0f), new Vector3(0.052f, 0.068f, 0.052f), baraMat);
            Bagian(bara.transform, PrimitiveType.Capsule, new Vector3(-0.2f, 0.45f, 0f), new Vector3(0.55f, 0.75f, 0.55f), baraMat);
            Bagian(bara.transform, PrimitiveType.Capsule, new Vector3(0.2f, 0.40f, 0.15f), new Vector3(0.50f, 0.70f, 0.50f), baraMat);
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
            if (!aktif && _baraObor != null && _baraObor.isPlaying) _baraObor.Stop();
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
            PerbaruiBaraObor();
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
