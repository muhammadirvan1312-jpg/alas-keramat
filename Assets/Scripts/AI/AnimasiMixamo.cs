using AlasKeramat.Core;
using UnityEngine;
namespace AlasKeramat.AI
{
    // Retarget mocap Mixamo ke kerangka karakter (Kerangka, 24 tulang) dengan komponen Animation legacy: klip legacy diputar di kerangka proksi
    // tersembunyi, lalu rotasi tiap tulang disalin sebagai delta relatif pose diam yang sudah dikalibrasi arah tulangnya.
    // Wajah (rahang, mata, alis) tetap prosedural. Jauh dari kamera (> JarakAktif) kembali ke animasi prosedural.
    [DefaultExecutionOrder(200)]
    public partial class AnimasiMixamo : MonoBehaviour
    {
        public enum Peran { Pria, Sekte, Wanita, Terluka, Raksasa }
        public const float Crossfade = 0.25f, JarakAktif = 45f, LajuJalanAsli = 1.45f, LajuLariAsli = 3.8f, BatasLari = 2.6f, BatasDiam = 0.25f;
        static readonly string[] NamaMixamo =
        {
            "Hips", "Spine", "Spine2", "Neck", "Head", null, "LeftShoulder", "RightShoulder", "LeftArm", "RightArm",
            "LeftForeArm", "RightForeArm", "LeftHand", "RightHand", "LeftUpLeg", "RightUpLeg", "LeftLeg", "RightLeg", "LeftFoot", "RightFoot"
        };
        // Tulang anak untuk arah kalibrasi (-1 = ikut induk).
        static readonly int[] AnakArah = { 1, 2, 3, 4, -1, -1, 8, 9, 10, 11, 12, 13, -1, -1, 16, 17, 18, 19, -1, -1 };

        [SerializeField] BankAnimasiMixamo bank;
        [SerializeField] Peran peran;
        public void Pasang(BankAnimasiMixamo b, Peran p) { bank = b; peran = p; }
        public Peran PeranAktif { get { return peran; } }

        ProceduralAnimator _pa;
        Transform[] _t, _p;
        Quaternion[] _invAcuan, _t0;
        Animation _anim;
        Transform _hipsProksi, _kamera;
        float _hips0, _skalaHips;
        Vector3 _pelvis0;
        string _kini = "";
        bool _siap;

        void Start()
        {
            _pa = GetComponent<ProceduralAnimator>();
            if (bank == null || bank.proksi == null || bank.klip.Length == 0 || _pa == null || _pa.Tulang == null) { enabled = false; return; }
            try { Siapkan(); }
            catch (System.Exception e) { Debug.LogError("[AK][AnimasiMixamo] Gagal menyiapkan retarget di " + name + ": " + e.Message); enabled = false; }
        }

        void Siapkan()
        {
            _t = _pa.Tulang;
            var go = Instantiate(bank.proksi, transform, false);
            go.name = "ProksiMixamo";
            go.transform.localPosition = Vector3.zero; go.transform.localRotation = Quaternion.identity;
            _p = new Transform[NamaMixamo.Length];
            var semua = go.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < NamaMixamo.Length; i++)
                if (NamaMixamo[i] != null) foreach (var x in semua) if (x.name == "mixamorig:" + NamaMixamo[i] || x.name.EndsWith(":" + NamaMixamo[i])) { _p[i] = x; break; }
            if (_p[0] == null) throw new System.Exception("tulang mixamorig:Hips tidak ada di proksi");
            _hipsProksi = _p[0];
            _hips0 = _hipsProksi.localPosition.y;
            _pelvis0 = _t[Kerangka.Pelvis].localPosition;
            _skalaHips = _t[Kerangka.Pelvis].position.y - transform.position.y;
            _skalaHips = _skalaHips / Mathf.Max(0.05f, _hipsProksi.position.y - transform.position.y);

            Quaternion invRoot = Quaternion.Inverse(transform.rotation);
            _invAcuan = new Quaternion[NamaMixamo.Length]; _t0 = new Quaternion[NamaMixamo.Length];
            var a = new Quaternion[NamaMixamo.Length];
            for (int i = 0; i < NamaMixamo.Length; i++)
            {
                if (_p[i] == null || _t[i] == null) continue;
                int anak = AnakArah[i];
                Quaternion A = Quaternion.identity;
                if (anak >= 0 && _p[anak] != null && _t[anak] != null)
                {
                    Vector3 dp = invRoot * (_p[anak].position - _p[i].position), dt = invRoot * (_t[anak].position - _t[i].position);
                    if (dp.sqrMagnitude > 1e-6f && dt.sqrMagnitude > 1e-6f) A = Quaternion.FromToRotation(dp, dt);
                }
                else if (Kerangka.Induk[i] >= 0) A = a[Kerangka.Induk[i]];
                a[i] = A;
                _invAcuan[i] = Quaternion.Inverse(A * (invRoot * _p[i].rotation));
                _t0[i] = invRoot * _t[i].rotation;
            }
            if (modelKarakter != null && PasangModel()) { Destroy(go); _siap = true; return; }
            _anim = go.AddComponent<Animation>();
            _anim.playAutomatically = false;
            _anim.cullingType = AnimationCullingType.AlwaysAnimate;
            foreach (var k in bank.klip) if (k != null && _anim.GetClip(k.name) == null) _anim.AddClip(k, k.name);
            _siap = true;
        }

        public static bool BolehTampilModelMixamo(ProceduralAnimator.Mode mode)
        {
            return mode == ProceduralAnimator.Mode.Normal || mode == ProceduralAnimator.Mode.DudukTerhuyung;
        }

        public static string KlipSerangUntuk(Peran peran, bool adaZombieAttack, bool adaGreatSword)
        {
            if (peran == Peran.Raksasa)
                return adaZombieAttack ? "Zombie_Attack" : adaGreatSword ? "Great_Sword_Slash" : "Standing_Melee_Attack_Horizontal";
            if (peran == Peran.Sekte && adaZombieAttack)
                return "Zombie_Attack";
            return "Standing_Melee_Attack_Horizontal";
        }

        // Pilih klip menurut peran, kecepatan dan pose tempur. "" = biarkan animasi prosedural.
        public static string KlipUntuk(Peran peran, float kec, ProceduralAnimator.PoseTempur pose)
        {
            switch (pose)
            {
                case ProceduralAnimator.PoseTempur.Ancang: return peran == Peran.Raksasa ? "Great_Sword_Slash" : "Standing_Melee_Attack_Horizontal";
                case ProceduralAnimator.PoseTempur.Ayun: return peran == Peran.Raksasa ? "Great_Sword_Slash" : "Standing_Melee_Attack_Horizontal";
                case ProceduralAnimator.PoseTempur.Goyah: return "Hit_Reaction";
                case ProceduralAnimator.PoseTempur.Gertak: return "Zombie_Scream";
                case ProceduralAnimator.PoseTempur.Menyerah: return "Kneeling_Idle";
                case ProceduralAnimator.PoseTempur.Merangkak: return "Crawling";
                case ProceduralAnimator.PoseTempur.Intai: return kec > BatasDiam ? "Crouch_Walking" : "Crouching_Idle";
                case ProceduralAnimator.PoseTempur.Kentongan: case ProceduralAnimator.PoseTempur.Tari: return "";
            }
            if (kec < BatasDiam) return peran == Peran.Terluka ? "Sitting_Idle" : "Breathing_Idle";
            if (kec < BatasLari) return peran == Peran.Wanita ? "Female_Walk" : peran == Peran.Terluka ? "Injured_Walk" : "Walking";
            return peran == Peran.Sekte ? "Running" : peran == Peran.Raksasa ? "Zombie_Running" : "Running";
        }

        public static bool BolehTampilModelMixamo(ProceduralAnimator.Mode mode)
        {
            return mode == ProceduralAnimator.Mode.Normal || mode == ProceduralAnimator.Mode.DudukTerhuyung;
        }

        void LateUpdate()
        {
            if (!_siap) return;
            if (_model != null && !_pa.enabled) { MatiModel(); return; }
            bool bolehModel = BolehTampilModelMixamo(_pa.ModeAktif);
            if (_model != null) TampilModel(bolehModel);
            if (!bolehModel) return;
            if (_kamera == null) { var c = Camera.main; if (c != null) _kamera = c.transform; }
            bool dekat = _kamera == null || (_kamera.position - transform.position).sqrMagnitude < JarakAktif * JarakAktif;
            if (_anim.enabled != dekat) _anim.enabled = dekat;
            if (!dekat) return;
            float kec = _pa.Kecepatan;
            string mau = _pa.ModeAktif == ProceduralAnimator.Mode.DudukTerhuyung
                ? (_anim.GetClip("Sitting_Idle") != null ? "Sitting_Idle" : "Kneeling_Idle")
                : (_pa.Pose == ProceduralAnimator.PoseTempur.Ancang || _pa.Pose == ProceduralAnimator.PoseTempur.Ayun)
                    ? KlipSerangUntuk(peran, _anim.GetClip("Zombie_Attack") != null, _anim.GetClip("Great_Sword_Slash") != null)
                    : KlipUntuk(peran, kec, _pa.Pose);
            if (mau.Length > 0 && _anim.GetClip(mau) == null) mau = kec < BatasLari ? "Walking" : "Running";
            if (_model != null && (mau.Length == 0 || _anim.GetClip(mau) == null)) mau = _anim.GetClip("Breathing_Idle") != null ? "Breathing_Idle" : mau;
            if (mau.Length == 0 || _anim.GetClip(mau) == null) { _kini = ""; return; }
            if (mau != _kini) { _anim.CrossFade(mau, Crossfade); _kini = mau; }
            var st = _anim[mau];
            if (st != null) st.speed = mau == "Walking" || mau == "Female_Walk" || mau == "Injured_Walk" ? Mathf.Clamp(kec / LajuJalanAsli, 0.6f, 1.6f)
                : mau == "Running" || mau == "Zombie_Running" ? Mathf.Clamp(kec / LajuLariAsli, 0.7f, 1.5f) : 1f;
            if (_model != null) { KunciPinggul(); GerakKepalaDanMata(); PerbaruiBayanganKontak(); } else Salin();
        }

        void Salin()
        {
            Quaternion root = transform.rotation, invRoot = Quaternion.Inverse(root);
            for (int i = 0; i < NamaMixamo.Length; i++)
            {
                if (_p[i] == null || _t[i] == null) continue;
                Quaternion d = (invRoot * _p[i].rotation) * _invAcuan[i];
                _t[i].rotation = root * (d * _t0[i]);
            }
            var pel = _t[Kerangka.Pelvis];
            pel.localPosition = _pelvis0 + Vector3.up * (_hipsProksi.localPosition.y - _hips0) * _hipsProksi.lossyScale.y / Mathf.Max(0.0001f, pel.parent != null ? pel.parent.lossyScale.y : 1f) * _skalaHips;
        }
    }
}
