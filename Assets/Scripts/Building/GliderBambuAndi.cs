using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Layang-Layang Bambu / Hang Glider khas SOTF: meluncur melintasi kanopi rimba dari puncak bukit & menara.
    // Dikendalikan mulus lewat joystick/geser kamera Android, mendarat otomatis saat menyentuh tanah/air.
    public class GliderBambuAndi : MonoBehaviour, IInteractable
    {
        public const float KecepatanMaju = 15.2f, LajuTurun = 1.75f, AngkatTermal = 0.65f, KecepatanBelok = 68f;

        public static bool SedangTerbangGlobal { get; private set; }
        public static float KetinggianTanahGlobal { get; private set; }
        public static float KecepatanGlobal { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatis() { SedangTerbangGlobal = false; KetinggianTanahGlobal = 0f; KecepatanGlobal = 0f; }

        [SerializeField] string namaTitik = "Bukit Luncur";
        [SerializeField] Transform sayapVisual;

        bool _terbang;
        float _yaw, _tAngin;
        Vector3 _posAsalSayap;
        Quaternion _rotAsalSayap;
        Transform _indukAsalSayap;
        AndiFPSController _andi;
        CharacterController _cc;

        public bool SedangTerbang { get { return _terbang; } }
        public float HoldDuration { get { return 0f; } }

        public void Pasang(string nama, Transform sayap)
        {
            namaTitik = string.IsNullOrEmpty(nama) ? "Bukit Luncur" : nama;
            sayapVisual = sayap;
        }

        public static Vector3 LangkahTerbang(Vector3 pos, float yawDeg, float maju, float turun, float dt)
        {
            float rad = yawDeg * Mathf.Deg2Rad;
            Vector3 arah = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
            return pos + arah * (maju * dt) - Vector3.up * (turun * dt);
        }

        public string GetPromptText(GameObject actor)
        {
            return _terbang ? "" : "TERBANG GLIDER BAMBU (" + namaTitik + ")";
        }

        public bool CanInteract(GameObject actor) { return !_terbang && !SedangTerbangGlobal; }

        public void Interact(GameObject actor)
        {
            if (_terbang || SedangTerbangGlobal) return;
            _andi = GameServices.Get<AndiFPSController>();
            if (_andi == null) return;
            _cc = _andi.GetComponent<CharacterController>();
            _terbang = true;
            SedangTerbangGlobal = true;
            _yaw = _andi.transform.eulerAngles.y;
            _andi.HentikanInputGerak();
            if (_cc != null) _cc.enabled = false;
            _andi.transform.position += Vector3.up * 2.2f + _andi.transform.forward * 1.8f;

            if (sayapVisual != null)
            {
                _indukAsalSayap = sayapVisual.parent;
                _posAsalSayap = sayapVisual.localPosition;
                _rotAsalSayap = sayapVisual.localRotation;
                sayapVisual.SetParent(_andi.transform, false);
                sayapVisual.localPosition = new Vector3(0f, 2.35f, 0.25f);
                sayapVisual.localRotation = Quaternion.identity;
            }
            GameEvents.RaiseToast("Terbang dengan Glider Bambu! Geser kiri/kanan untuk berbelok.");
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.Play(ProceduralAudioSynth.KlipLepasPanah, transform.position, AudioCategory.Efek, 0.78f, 0.78f, 2);
        }

        void Update()
        {
            if (!_terbang || _andi == null || JedaDunia.Dijeda) return;
            float dt = Time.deltaTime;

            // Belok mengikuti joystick horizontal + arah kamera
            float belok = _andi.InputGerak.x;
            if (_andi.Kamera != null)
            {
                float camYaw = _andi.Kamera.eulerAngles.y;
                _yaw = Mathf.LerpAngle(_yaw, camYaw, dt * 1.8f);
            }
            _yaw += belok * KecepatanBelok * dt;

            // Angin termal lembut saat meluncur jauh di atas lembah
            Vector3 p = _andi.transform.position;
            float tanah = WorldTerrainGenerator.SampleHeight(p.x, p.z);
            float? air = WorldTerrainGenerator.GetWaterLevelAt(p.x, p.z);
            float dasar = air.HasValue ? Mathf.Max(tanah, air.Value) : tanah;
            float tinggi = p.y - dasar;
            float turun = tinggi > 18f ? Mathf.Max(0.85f, LajuTurun - AngkatTermal) : LajuTurun;
            float maju = KecepatanMaju + Mathf.Clamp(_andi.InputGerak.y, -0.35f, 0.35f) * 4.5f;

            Vector3 baru = LangkahTerbang(p, _yaw, maju, turun, dt);
            baru.x = Mathf.Clamp(baru.x, -940f, 940f);
            baru.z = Mathf.Clamp(baru.z, -940f, 940f);
            _andi.transform.position = baru;

            KetinggianTanahGlobal = Mathf.Max(0f, baru.y - dasar);
            KecepatanGlobal = maju;

            if (sayapVisual != null)
                sayapVisual.localRotation = Quaternion.Euler(4f, 0f, -belok * 16f);

            _tAngin -= dt;
            if (_tAngin <= 0f)
            {
                _tAngin = 2.6f;
                var synth = GameServices.Get<ProceduralAudioSynth>();
                if (synth != null) synth.Play(ProceduralAudioSynth.KlipLepasPanah, baru, AudioCategory.Ambien, 0.32f, 0.62f, 1);
            }

            // Mendarat otomatis saat dekat permukaan tanah/air atau pemain menekan jongkok
            if (KetinggianTanahGlobal <= 1.25f || _andi.Jongkok)
            {
                Mendarat(baru, dasar);
            }
        }

        void Mendarat(Vector3 pos, float dasar)
        {
            _terbang = false;
            SedangTerbangGlobal = false;
            KetinggianTanahGlobal = 0f;
            KecepatanGlobal = 0f;
            pos.y = dasar + 0.18f;
            _andi.transform.position = pos;
            if (_cc != null) _cc.enabled = true;
            _andi.GuncangKamera(0.14f);

            if (sayapVisual != null && _indukAsalSayap != null)
            {
                sayapVisual.SetParent(_indukAsalSayap, false);
                sayapVisual.localPosition = _posAsalSayap;
                sayapVisual.localRotation = _rotAsalSayap;
            }
            var lvl = GameServices.Get<LevelSurvivalAndi>();
            if (lvl != null) lvl.TambahXP(LevelSurvivalAndi.Atribut.StaminaLari, 14f);
            GameEvents.RaiseToast("Mendarat mulus dengan Glider Bambu.");
        }
    }
}
