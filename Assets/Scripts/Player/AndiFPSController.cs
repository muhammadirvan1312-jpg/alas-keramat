using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Player
{
    // T3a: gerak orang pertama Andi (09 §3): jalan/lari/jongkok/lompat, coyote time, berenang & menyelam dasar,
    // damage jatuh lewat IDamageable di objek yang sama, kamera pitch/yaw + head bob. Input dari TouchControls atau keyboard/mouse.
    [RequireComponent(typeof(CharacterController))]
    [DefaultExecutionOrder(200)]
    public partial class AndiFPSController : MonoBehaviour
    {
        public const float JalanMaks = 3.4f, Sprint = 6.2f, JongkokMaks = 1.8f, Renang = 2.2f, RenangCepat = 3.2f;
        public const float FaktorMundur = 0.7f, FaktorSamping = 0.9f, Akselerasi = 18f, Perlambatan = 22f;
        public const float VLompat = 4.8f, Gravitasi = 18f, Coyote = 0.12f;
        public const float TinggiKapsul = 1.8f, TinggiJongkok = 1.1f, Radius = 0.32f, BatasLereng = 42f, LangkahTangga = 0.35f;
        public const float MataBerdiri = 1.65f, MataJongkok = 0.95f, PitchMaks = 80f;
        public const float SensH = 0.22f, SensV = 0.17f, SensMouse = 2.2f;
        public const float BobV = 0.035f, BobL = 0.02f, FrekBob = 1.9f;
        public const float AirMinRenang = 1.3f, MataDiBawahAir = 0.25f, NapasNormal = 25f, PitchSelam = -25f, Apung = 2.5f;
        public const float JatuhAman = 6f, JatuhMati = 18f, DamagePerMeter = 7f;
        const float JoyMati = 0.2f, JoyPenuh = 0.95f, LerpTinggi = 8f, LajuNapasPulih = 5f;

        [SerializeField] Transform kamera;

        public Vector2 InputGerak;      // -1..1 (x samping, y maju)
        public Vector2 InputLihatPx;    // piksel layar yang diakumulasi sejak frame lalu
        public bool MintaLari;
        public bool LariDiblokir;
        public float PengaliKecepatan = 1f;
        public float PengaliSensitivitas = 1f;
        public bool BalikY;
        public bool PakaiHeadBob; // Opsional; bawaan tanpa goyangan untuk kenyamanan layar sentuh.
        public event System.Action OnLompat;
        public bool Jongkok { get; private set; }
        public bool Berenang { get; private set; }
        public bool Menyelam { get; private set; }
        public bool DiTanah { get; private set; }
        public bool SedangLari { get; private set; }
        public float KecepatanDatar { get; private set; }
        public float Napas { get; private set; } = NapasNormal;
        public float NapasMaks = NapasNormal;
        public float TinggiJatuhTerakhir { get; private set; }
        public Transform Kamera { get { return kamera; } }
        float _lihatManual = -10f, _akhirGuncang, _lamaGuncang;
        public bool SedangMembidikManual { get { return Time.unscaledTime - _lihatManual < 0.2f; } }
        public bool GoyanganTempur = true;
        public void LihatManual(Vector2 delta)
        {
            if (delta.sqrMagnitude <= 0.0001f) return;
            _lihatManual = Time.unscaledTime; InputLihatPx += delta;
        }
        public void GuncangKamera(float lama)
        {
            if (lama <= 0f) { _akhirGuncang = 0f; return; }
            if (!GoyanganTempur || lama <= 0f) return;
            _lamaGuncang = Mathf.Max(lama, 0.01f); _akhirGuncang = Time.unscaledTime + lama;
        }

        CharacterController _cc;
        Vector3 _vDatar;
        float _vy, _yaw, _pitch, _sejakTanah, _faseBob, _mata = MataBerdiri, _puncakY;
        bool _mintaLompat, _diUdara;
#if ENABLE_LEGACY_INPUT_MANAGER && (UNITY_EDITOR || UNITY_STANDALONE)
        bool _keyboardGerak;
#endif

        public void Pasang(Transform cam) { kamera = cam; }
        public void HentikanInputGerak()
        {
            InputGerak = Vector2.zero; InputLihatPx = Vector2.zero; MintaLari = false;
            _mintaLompat = false; _vDatar = Vector3.zero; SedangLari = false;
        }
        public void Lompat() { _mintaLompat = true; }
        public void ToggleJongkok()
        {
            if (Jongkok && !BisaBerdiri()) { GameEvents.RaiseToast("Ruang di atas kepala terlalu sempit"); return; }
            Jongkok = !Jongkok;
        }
        public bool BisaBerdiri()
        {
            Vector3 kaki = transform.position;
            int mask = (1 << GameLayers.Ground) | (1 << GameLayers.Structure) | 1;
            return !Physics.CheckCapsule(kaki + Vector3.up * (TinggiJongkok - Radius),
                kaki + Vector3.up * (TinggiKapsul - Radius), Radius - 0.02f, mask, QueryTriggerInteraction.Ignore);
        }
        public static float VertikalSetelahTabrakan(float kecepatan, CollisionFlags flags)
        { return kecepatan > 0f && (flags & CollisionFlags.Above) != 0 ? 0f : kecepatan; }

        public static float KecepatanTarget(float magnitudo, bool lari, bool jongkok)
        {
            if (magnitudo < JoyMati) return 0f;
            if (jongkok) return JongkokMaks * Mathf.Clamp01((magnitudo - JoyMati) / (JoyPenuh - JoyMati));
            if (lari) return Sprint;
            return JalanMaks * Mathf.Clamp01((magnitudo - JoyMati) / (JoyPenuh - JoyMati));
        }

        public static Vector2 ArahGerak(Vector2 input) { return input.sqrMagnitude>0.000001f?input.normalized:Vector2.zero; }

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _cc.height = TinggiKapsul; _cc.radius = Radius; _cc.slopeLimit = BatasLereng; _cc.stepOffset = LangkahTangga;
            _cc.center = new Vector3(0f, TinggiKapsul * 0.5f, 0f);
            _yaw = transform.eulerAngles.y;
            GameServices.Register(this);
        }

        void Update()
        {
            try
            {
                if (Time.timeScale <= 0f) { HentikanInputGerak(); return; }
                if (AlasKeramat.UI.PengunciInput.Terblokir)
                {
                    HentikanInputGerak();
                }
                else BacaKeyboardMouse();
                if (!GerakKendaraan(Time.deltaTime)) Gerak(Time.deltaTime);
            }
            catch (System.Exception e) { Debug.LogError("[AK][AndiFPSController] " + e.Message); }
        }

        void LateUpdate()
        {
            if (kamera == null || AlasKeramat.UI.PengunciInput.Terblokir) { InputLihatPx = Vector2.zero; _akhirGuncang = 0f; return; }
            float kecGeser = InputLihatPx.magnitude;
            float kurva = kecGeser > 12f ? 1f + Mathf.Clamp01((kecGeser - 12f) / 55f) * 0.7f : 1f;
            _yaw += InputLihatPx.x * kurva * SensH * PengaliSensitivitas;
            _pitch = Mathf.Clamp(_pitch - InputLihatPx.y * kurva * SensV * PengaliSensitivitas * (BalikY ? -1f : 1f), -PitchMaks, PitchMaks);
            InputLihatPx = Vector2.zero;
            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            float targetMata = Jongkok ? MataJongkok : MataBerdiri;
            _mata = Mathf.Lerp(_mata, targetMata, Time.deltaTime * LerpTinggi);
            float laju = DiTanah && !Berenang ? KecepatanDatar / JalanMaks : 0f;
            _faseBob += Time.deltaTime * FrekBob * Mathf.PI * 2f * Mathf.Max(0.0001f, laju);
            float fBob = PakaiHeadBob ? 1f : 0f;
            float bobV = Mathf.Sin(_faseBob * 2f) * BobV * Mathf.Clamp01(laju) * fBob;
            float bobL = Mathf.Sin(_faseBob) * BobL * Mathf.Clamp01(laju) * fBob;
            kamera.localPosition = new Vector3(bobL, _mata + bobV, 0f);
            float sisa = GoyanganTempur ? Mathf.Clamp01((_akhirGuncang - Time.unscaledTime) / Mathf.Max(_lamaGuncang, 0.01f)) : 0f;
            float guncang = Mathf.Sin(Time.unscaledTime * 95f) * 0.6f * sisa;
            kamera.localRotation = Quaternion.Euler(_pitch + guncang, guncang * 0.45f, 0f);
        }

        // H3: keyboard/mouse hanya untuk uji di Editor/PC, di dalam ENABLE_LEGACY_INPUT_MANAGER.
        void BacaKeyboardMouse()
        {
#if ENABLE_LEGACY_INPUT_MANAGER && (UNITY_EDITOR || UNITY_STANDALONE)
            float x = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
            float y = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
            if (x != 0f || y != 0f || _keyboardGerak) { InputGerak = Vector2.ClampMagnitude(new Vector2(x, y), 1f); MintaLari = Input.GetKey(KeyCode.LeftShift); }
            _keyboardGerak = x != 0f || y != 0f;
            if (Input.GetKeyDown(KeyCode.Space)) Lompat();
            if (Input.GetKeyDown(KeyCode.C)) ToggleJongkok();
            if (Input.GetMouseButton(1))
                LihatManual(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * SensMouse / SensH);
#endif
        }

        void Gerak(float dt)
        {
            Vector3 p = transform.position;
            float? air = WorldTerrainGenerator.AirDi(p);
            float dasar = WorldTerrainGenerator.Lantai(p);
            bool airDalam = air.HasValue && air.Value - dasar > AirMinRenang && p.y + _mata < air.Value + 0.1f;
            Berenang = airDalam;
            if (Berenang && Jongkok) Jongkok = false;

            float mag = Mathf.Clamp01(InputGerak.magnitude);
            SedangLari = MintaLari && !LariDiblokir && !Jongkok && InputGerak.y > 0.5f && mag >= JoyPenuh;
            float target = Berenang ? (mag < JoyMati ? 0f : (SedangLari ? RenangCepat : Renang)) : KecepatanTarget(mag, SedangLari, Jongkok);
            Vector2 arah = ArahGerak(InputGerak);
            float faktor = arah.y < 0f ? FaktorMundur : (Mathf.Abs(arah.x) > Mathf.Abs(arah.y) ? FaktorSamping : 1f);
            Vector3 lokal = new Vector3(arah.x, 0f, arah.y);
            if (lokal.sqrMagnitude > 1f) lokal.Normalize();
            Vector3 inginDatar = transform.TransformDirection(lokal) * target * faktor * PengaliKecepatan;
            float a = inginDatar.sqrMagnitude > _vDatar.sqrMagnitude ? Akselerasi : Perlambatan;
            _vDatar = Vector3.MoveTowards(_vDatar, inginDatar, a * dt);

            if (Berenang)
            {
                Menyelam = _pitch > -PitchSelam && InputGerak.y > 0.3f;
                float targetY = air.Value - _mata + MataDiBawahAir;
                if (Menyelam) _vy = Mathf.MoveTowards(_vy, -Mathf.Sin(_pitch * Mathf.Deg2Rad) * target, Akselerasi * dt);
                else _vy = Mathf.MoveTowards(_vy, Mathf.Clamp((targetY - p.y) * Apung, -Renang, Renang), Akselerasi * dt);
                if (_mintaLompat && !Menyelam) _vy = VLompat * 0.6f;
                bool kepalaDiAir = p.y + _mata < air.Value - 0.05f;
                Napas = kepalaDiAir ? Mathf.Max(0f, Napas - dt) : Mathf.Min(NapasMaks, Napas + dt * LajuNapasPulih);
                _diUdara = false; _puncakY = p.y;
            }
            else
            {
                Menyelam = false;
                Napas = Mathf.Min(NapasMaks, Napas + dt * LajuNapasPulih);
                if (DiTanah) _sejakTanah = 0f; else _sejakTanah += dt;
                if (DiTanah && _vy < 0f) _vy = -2f;
                if (_mintaLompat && _sejakTanah <= Coyote && !Jongkok) { _vy = VLompat; _sejakTanah = Coyote + 1f; var h = OnLompat; if (h != null) h(); }
                _vy -= Gravitasi * dt;
            }
            _mintaLompat = false;

            float tinggi = Jongkok ? TinggiJongkok : TinggiKapsul;
            if (!Mathf.Approximately(_cc.height, tinggi)) { _cc.height = tinggi; _cc.center = new Vector3(0f, tinggi * 0.5f, 0f); }

            var flags = _cc.Move((_vDatar + Vector3.up * _vy) * dt);
            _vy = VertikalSetelahTabrakan(_vy, flags);
            bool tanahBaru = (flags & CollisionFlags.Below) != 0;
            CekJatuh(tanahBaru);
            DiTanah = tanahBaru;
            Vector3 v = _cc.velocity; v.y = 0f;
            KecepatanDatar = v.magnitude;
        }

        void CekJatuh(bool tanahBaru)
        {
            float y = transform.position.y;
            if (!DiTanah && !Berenang)
            {
                if (!_diUdara) { _diUdara = true; _puncakY = y; }
                _puncakY = Mathf.Max(_puncakY, y);
            }
            if (tanahBaru && _diUdara)
            {
                _diUdara = false;
                TinggiJatuhTerakhir = _puncakY - y;
                if (TinggiJatuhTerakhir > JatuhAman)
                {
                    var target = GetComponent<IDamageable>();
                    float dmg = TinggiJatuhTerakhir > JatuhMati ? 9999f : DamagePerMeter * (TinggiJatuhTerakhir - JatuhAman);
                    if (target != null) target.TakeDamage(new DamageInfo { amount = dmg, type = DamageType.Jatuh, point = transform.position, direction = Vector3.down, source = gameObject });
                }
            }
        }
    }
}
