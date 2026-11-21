using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Perahu rakit kayu/bambu yang bisa dinaiki & didayung di sungai dan laut (mengikuti InputGerak joystick/WASD).
    public class PerahuDayung : MonoBehaviour, IInteractable
    {
        public const float KecepatanDayung = 7.8f, LajuBelok = 72f;

        bool _dinaiki;
        float _yaw, _tPercik;
        AndiFPSController _andi;
        CharacterController _cc;

        public bool Dinaiki { get { return _dinaiki; } }
        public float HoldDuration { get { return 0f; } }

        public static float KecepatanEfektif(float inputMaju)
        {
            return Mathf.Clamp(inputMaju, -0.45f, 1f) * KecepatanDayung;
        }

        public string GetPromptText(GameObject actor) { return _dinaiki ? "TURUN PERAHU" : "NAIK PERAHU DAYUNG"; }
        public bool CanInteract(GameObject actor) { return true; }

        void Awake() { _yaw = transform.eulerAngles.y; }

        public void Interact(GameObject actor)
        {
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            if (_andi == null) return;
            if (_cc == null) _cc = _andi.GetComponent<CharacterController>();

            _dinaiki = !_dinaiki;
            if (_dinaiki)
            {
                _andi.HentikanInputGerak();
                if (_cc != null) _cc.enabled = false;
                GameEvents.RaiseToast("Naik perahu! Gerakkan analog untuk mendayung & berbelok.");
            }
            else
            {
                Vector3 turun = transform.position + transform.right * 1.6f;
                turun.y = Mathf.Max(WorldTerrainGenerator.SampleHeight(turun.x, turun.z), transform.position.y) + 0.25f;
                _andi.transform.position = turun;
                if (_cc != null) _cc.enabled = true;
                GameEvents.RaiseToast("Turun dari perahu.");
            }
        }

        void Update()
        {
            if (JedaDunia.Dijeda) return;
            float dt = Time.deltaTime;
            float oleng = Mathf.Sin(Time.time * 2.2f + transform.position.x * 0.2f) * 3.2f;

            if (!_dinaiki || _andi == null)
            {
                transform.rotation = Quaternion.Euler(0f, _yaw, oleng);
                return;
            }

            Vector2 inp = _andi.InputGerak;
            _yaw += inp.x * LajuBelok * dt;
            float kec = KecepatanEfektif(inp.y);
            Vector3 maju = Quaternion.Euler(0f, _yaw, 0f) * Vector3.forward;
            Vector3 calon = transform.position + maju * kec * dt;
            float? wl = WorldTerrainGenerator.GetWaterLevelAt(calon.x, calon.z);
            float hTanah = WorldTerrainGenerator.SampleHeight(calon.x, calon.z);
            // Tetap mengapung di air; jangan meluncur naik ke daratan tinggi.
            if (wl.HasValue && wl.Value >= hTanah - 0.15f)
            {
                calon.y = wl.Value + 0.08f + Mathf.Sin(Time.time * 2.6f) * 0.04f;
                transform.position = calon;
            }
            transform.rotation = Quaternion.Euler(Mathf.Clamp(-kec * 0.6f, -4f, 4f), _yaw, oleng);
            _andi.transform.position = transform.position + Vector3.up * 0.55f;

            if (Mathf.Abs(kec) > 1.2f)
            {
                _tPercik -= dt;
                if (_tPercik <= 0f)
                {
                    _tPercik = 0.28f;
                    var gore = GameServices.Get<GoreSystem>();
                    if (gore != null) gore.PercikPartikel(transform.position - maju * 1.4f, new Color(0.80f, 0.92f, 0.98f, 0.72f), 5, 0.07f, 1.5f, 0.5f);
                }
            }
        }
    }
}
