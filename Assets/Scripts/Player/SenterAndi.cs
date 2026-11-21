using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;
namespace AlasKeramat.Player
{
    // Senter Taktis (04 §4): cahaya yang tidak padam ditiup angin. Menyala otomatis di bawah tanah bila Andi memilikinya.
    public class SenterAndi : MonoBehaviour
    {
        public const float Jangkau = 30f, Sudut = 54f, Intensitas = 3.6f, Tick = 0.3f;
        [SerializeField] Light lampu;
        [SerializeField] Texture cookie;
        public void PasangCookie(Texture t) { cookie = t; }
        float _t;
        public bool Nyala { get { return lampu != null && lampu.enabled; } }
        public Transform TransformLampu { get { return lampu != null ? lampu.transform : null; } }
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
                lampu.color = new Color(1f, 0.94f, 0.84f); lampu.shadows = LightShadows.None; lampu.enabled = false;
            }
            // Pola pantul reflektor senter (titik terang, cincin, tepi lembut) agar cahaya tidak tampak seperti kerucut polos.
            if (lampu != null && cookie != null) { lampu.cookie = cookie; lampu.innerSpotAngle = Sudut * 0.35f; }
        }
        void Update()
        {
            _t += Time.deltaTime; if (_t < Tick) return; _t = 0f;
            if (lampu == null) return;
            var inv = GameServices.Get<TikarPandan3DInventory>();
            bool punya = inv != null && inv.Punya(ItemID.SenterTaktis);
            var jam = GameServices.Get<TimeOfDayClock>();
            bool gelap = WorldTerrainGenerator.DiBawahTanah(transform.position.y) || (jam != null && (jam.Jam >= 18.5f || jam.Jam < 5.5f));
            bool nyala = punya && gelap;
            if (lampu.enabled != nyala) lampu.enabled = nyala;
        }
    }
}
