using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;
namespace AlasKeramat.Player
{
    // Senter Taktis (04 §4): cahaya yang tidak padam ditiup angin, lengkap dengan bodi taktis FP & berkas cahaya volumetrik.
    public class SenterAndi : MonoBehaviour
    {
        public const float Jangkau = 28f, Sudut = 48f, Intensitas = 3.2f, Tick = 0.3f, RadiusGangguanHantu = 18f;
        [SerializeField] Light lampu;
        [SerializeField] Texture cookie;
        public void PasangCookie(Texture t) { cookie = t; }
        float _t, _jarakHantu = 999f;
        bool _paksaMati;
        GameObject _badanSenter;
        MeshRenderer _berkasRen;
        public bool Nyala { get { return lampu != null && lampu.enabled; } }

        // Pengali intensitas senter (0.12..1.0) saat hantu mendekat dalam radius 18 m.
        public static float PengaliKedipHantu(float jarakHantu, float waktu)
        {
            if (jarakHantu <= 0f || jarakHantu >= RadiusGangguanHantu) return 1f;
            float dekat01 = 1f - Mathf.Clamp01(jarakHantu / RadiusGangguanHantu);
            float n = 0.5f + 0.5f * Mathf.Sin(waktu * 21f + Mathf.Sin(waktu * 7.5f) * 2.4f);
            float ambang = 0.35f + dekat01 * 0.40f;
            float turun = n < ambang ? Mathf.Lerp(0.14f, 0.55f, n / Mathf.Max(0.01f, ambang)) : 1f;
            return Mathf.Clamp(Mathf.Lerp(1f, turun, 0.45f + dekat01 * 0.55f), 0.12f, 1f);
        }

        public static bool HarusMenyala(float jam, bool bawahTanah, bool paksaMati)
        {
            if (paksaMati) return false;
            return bawahTanah || jam >= 17.5f || jam <= 6.2f;
        }

        public void ToggleSenter() { _paksaMati = !_paksaMati; _t = Tick; }

        void Awake()
        {
            GameServices.Register(this);
            var ctrl = GetComponent<AndiFPSController>();
            if (lampu == null && ctrl != null && ctrl.Kamera != null)
            {
                var go = new GameObject("CahayaSenter");
                go.transform.SetParent(ctrl.Kamera, false);
                go.transform.localPosition = new Vector3(0.25f, -0.2f, 0.3f);
                lampu = go.AddComponent<Light>();
                lampu.type = LightType.Spot; lampu.range = Jangkau; lampu.spotAngle = Sudut; lampu.intensity = Intensitas;
                lampu.color = new Color(0.92f, 0.95f, 1f); lampu.shadows = LightShadows.None; lampu.enabled = false;
            }
            if (lampu != null && _badanSenter == null) BangunVisualSenter(lampu.transform);
            // Pola pantul reflektor senter (titik terang, cincin, tepi lembut) agar cahaya tidak tampak seperti kerucut polos.
            if (lampu != null && cookie != null) { lampu.cookie = cookie; lampu.innerSpotAngle = Sudut * 0.35f; }
        }

        void BangunVisualSenter(Transform induk)
        {
            _badanSenter = new GameObject("BadanSenter");
            _badanSenter.transform.SetParent(induk, false);
            _badanSenter.layer = GameLayers.Player;
            var matBodi = SafeMaterialFactory.GetCached("M_SenterBodi", new Color(0.14f, 0.16f, 0.18f), 0.82f, 0.72f);
            var matLensa = SafeMaterialFactory.CreatePBR("M_SenterLensa", new Color(1f, 0.98f, 0.9f), 0f, 0.95f, null, null, new Color(2.2f, 2.1f, 1.85f));
            BagianSenter(_badanSenter.transform, "SilinderSenter", PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.036f, 0.11f, 0.036f), Quaternion.Euler(90f, 0f, 0f), matBodi);
            BagianSenter(_badanSenter.transform, "KepalaSenter", PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.11f), new Vector3(0.048f, 0.032f, 0.048f), Quaternion.Euler(90f, 0f, 0f), matBodi);
            BagianSenter(_badanSenter.transform, "LensaSenter", PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.142f), new Vector3(0.042f, 0.005f, 0.042f), Quaternion.Euler(90f, 0f, 0f), matLensa);
            var berkas = GameObject.CreatePrimitive(PrimitiveType.Quad);
            berkas.name = "BerkasSenter";
            var colB = berkas.GetComponent<Collider>(); if (colB != null) Destroy(colB);
            berkas.layer = GameLayers.Player;
            berkas.transform.SetParent(induk, false);
            berkas.transform.localPosition = new Vector3(0f, 0f, 1.45f);
            berkas.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            berkas.transform.localScale = new Vector3(2.4f, 0.55f, 1f);
            _berkasRen = berkas.GetComponent<MeshRenderer>();
            _berkasRen.sharedMaterial = SafeMaterialFactory.CreatePBR("M_BerkasSenter", new Color(1f, 0.96f, 0.85f, 0.09f), 0f, 0.1f, null, null, new Color(1.2f, 1.15f, 0.98f), SurfaceMode.Transparent);
            _berkasRen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _berkasRen.receiveShadows = false;
            _badanSenter.SetActive(false);
            _berkasRen.enabled = false;
        }

        static GameObject BagianSenter(Transform induk, string nama, PrimitiveType tipe, Vector3 pos, Vector3 skala, Quaternion rot, Material mat)
        {
            var g = GameObject.CreatePrimitive(tipe); g.name = nama;
            var col = g.GetComponent<Collider>(); if (col != null) Destroy(col);
            g.layer = GameLayers.Player;
            g.transform.SetParent(induk, false);
            g.transform.localPosition = pos; g.transform.localRotation = rot; g.transform.localScale = skala;
            g.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return g;
        }

        void Update()
        {
            _t += Time.deltaTime;
            if (_t >= Tick)
            {
                _t = 0f;
                if (lampu == null) return;
                var inv = GameServices.Get<TikarPandan3DInventory>();
                bool punya = inv != null && inv.Punya(ItemID.SenterTaktis);
                var jam = GameServices.Get<TimeOfDayClock>();
                bool bawah = WorldTerrainGenerator.DiBawahTanah(transform.position.y);
                bool nyala = punya && HarusMenyala(jam != null ? jam.Jam : 12f, bawah, _paksaMati);
                if (lampu.enabled != nyala) lampu.enabled = nyala;
                if (_badanSenter != null && _badanSenter.activeSelf != nyala) _badanSenter.SetActive(nyala);
                if (_berkasRen != null && _berkasRen.enabled != nyala) _berkasRen.enabled = nyala;
                _jarakHantu = 999f;
                if (nyala)
                {
                    Vector3 pos = transform.position;
                    for (int i = 0; i < HantuAI.Semua.Count; i++)
                    {
                        var h = HantuAI.Semua[i];
                        if (h == null || !h.IsAlive || h.Fase == HantuAI.FaseHantu.Hilang) continue;
                        float d = Vector3.Distance(pos, h.transform.position);
                        if (d < _jarakHantu) _jarakHantu = d;
                    }
                }
                else
                {
                    lampu.intensity = Intensitas;
                }
            }
            if (lampu != null && lampu.enabled)
            {
                float k = PengaliKedipHantu(_jarakHantu, Time.time);
                lampu.intensity = Intensitas * k;
                if (_berkasRen != null) _berkasRen.enabled = k > 0.35f;
            }
        }
    }
}
