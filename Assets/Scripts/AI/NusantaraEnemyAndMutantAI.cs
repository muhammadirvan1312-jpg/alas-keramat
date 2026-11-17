using System.Collections.Generic;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.AI
{
    // 06 §5-6 (T7a): Sekte manusia - persepsi (kesadaran 0-100, pandang & dengar), state machine Patroli/Selidik/Mengintai/Isyarat/Gertak/
    // Serang (token, mengitari)/Lari/Terluka/Menyerah (AMPUNI)/Mati (GELEDAH)/Ritual. Lokomosi CharacterController + GridPathfinder (H6).
    // Hantu (06 §8), Seret Teman, lempar bambu & raid menyusul T7b; gore/ragdoll T7c.
    public partial class NusantaraEnemyAndMutantAI : MonoBehaviour, IDamageable, IInteractable
    {
        public enum KeadaanSekte { Patroli, Selidik, Mengintai, Isyarat, Gertak, Serang, Lari, Terluka, Menyerah, Mati, Ritual, Serbu, SeretTeman }
        enum FaseSerang { Dekati, Ancang, Ayun, AncangLompat, Lompat, Jeda, AncangLempar }

        public const float Jangkau = 1.7f, AncangAyun = 0.45f, LamaAyun = 0.25f, MomenKena = 0.1f, AncangLompat = 0.6f, JarakLompat = 3f, LamaLompat = 0.35f;
        public const float JedaSeranganMin = 1.0f, JedaSeranganMaks = 1.8f, KitarMin = 6f, KitarMaks = 9f, KerucutKena = 60f;
        public const float IntaiMin = 25f, IntaiMaks = 40f, IntaiMundur = 15f, JarakLari = 60f, LamaLari = 30f, LamaMenyerah = 6f, LamaIsyarat = 3.6f;
        public const float Gravitasi = 18f, LajuBelok = 360f, TickPersepsi = 0.2f, JedaJalur = 2f, JarakLangsung = 10f, RadiusPisah = 1.4f;
        public const float LamaLupa = 15f, JarakLupa = 70f, RadiusRitual = 6f, LamaMatiAbu = 4f, SanityAmpuni = 3f, SanitySerangMenyerah = -4f;

        public static event System.Action OnMenyerahDiserang; // T8: Ningsih -5 Trust
        public static readonly List<NusantaraEnemyAndMutantAI> Semua = new List<NusantaraEnemyAndMutantAI>();

        [SerializeField] EnemyArchetype jenis;
        [SerializeField] GameObject prompt;

        public EnemyArchetype Jenis { get { return jenis; } }
        public KeadaanSekte Keadaan { get { return _keadaan; } }
        public float Kesadaran { get { return _sadar; } }
        public float Takut { get { return _takut; } set { _takut = Mathf.Clamp(value, 0f, 100f); } }
        public float HP { get { return _hp; } }
        public bool IsAlive { get { return _keadaan != KeadaanSekte.Mati; } }
        public float HoldDuration { get { return _keadaan == KeadaanSekte.Mati && PromptMayat() == "POTONG" ? LamaPotong : 0f; } }
        public bool Pengintai { get; set; }
        public Vector3[] Rute { get; set; }
        public Vector3 TitikRitual { get; set; }
        public EnemyDirector.Regu Asal { get; set; }

        StatMusuh.DataMusuh _d;
        KeadaanSekte _keadaan;
        FaseSerang _fase;
        CharacterController _cc;
        ProceduralAnimator _anim;
        EnemyDirector _dir;
        GridPathfinder _pf;
        ProceduralAudioSynth _audio;
        AndiFPSController _andi;
        AndiVitalsAndSanity _vital;
        CombatAndLeftHandController _combat;
        TanganKiri _kiri;
        ReaksiAndi _reaksiAndi;
        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _cuaca;
        Renderer[] _rend;
        MaterialPropertyBlock _mpb;

        float _hp, _sadar, _takut, _tState, _tPersepsi, _tGoyah, _vy, _tFase, _jedaSerang, _tTakTerlihat, _tSuara, _tJalur, _sisiKitar = 1f, _sudutRitual, _lamaIntai;
        bool _siap, _terlihat, _diserang, _sudahIsyarat, _adaSelidik, _digeledah, _menungguJalur, _tokenku, _logError;
        int _nomorSuara, _iRute, _iJalur;
        Vector3 _posTerakhir, _posSelidik, _titikLari, _tujuanJalur, _arahLompat, _posLahir;
        List<Vector3> _jalur = new List<Vector3>(), _jalurBaru = new List<Vector3>();
        System.Action<bool> _cbJalur;
        readonly List<ItemStack> _jarahan = new List<ItemStack>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatis() { Semua.Clear(); }

        public void Pasang(EnemyArchetype j, GameObject promptGo) { jenis = j; prompt = promptGo; }

        void Awake()
        {
            _d = StatMusuh.Ambil(jenis);
            _hp = _d.hp;
            _cc = GetComponent<CharacterController>();
            _anim = GetComponent<ProceduralAnimator>();
            _rend = GetComponentsInChildren<Renderer>(true);
            _mpb = new MaterialPropertyBlock();
            _cbJalur = SelesaiJalur;
            _posLahir = transform.position;
            _sudutRitual = Random.value * 360f;
            _sisiKitar = Random.value < 0.5f ? -1f : 1f;
            if (prompt != null) prompt.SetActive(false);
            if (_cc == null) { Debug.LogError("[AK][Musuh] " + name + " tanpa CharacterController, musuh dinonaktifkan."); enabled = false; return; }
            _siap = true;
        }

        void OnEnable() { if (!Semua.Contains(this)) Semua.Add(this); }
        void OnDisable() { Semua.Remove(this); LepasToken(); }

        void Ambil()
        {
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            if (_vital == null) _vital = GameServices.Get<AndiVitalsAndSanity>();
            if (_combat == null) _combat = GameServices.Get<CombatAndLeftHandController>();
            if (_kiri == null) _kiri = GameServices.Get<TanganKiri>();
            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            if (_cuaca == null) _cuaca = GameServices.Get<URPAtmosphereAndWeather>();
            if (_dir == null) _dir = GameServices.Get<EnemyDirector>();
            if (_pf == null) _pf = GameServices.Get<GridPathfinder>();
            if (_audio == null) _audio = GameServices.Get<ProceduralAudioSynth>();
        }

        void Update()
        {
            if (!_siap) return;
            try { Tick(Time.deltaTime); }
            catch (System.Exception e)
            {
                if (!_logError) { Debug.LogError("[AK][Musuh] " + name + " galat di Update: " + e.Message); _logError = true; }
            }
        }

        void Tick(float dt)
        {
            Ambil();
            _tState += dt;
            if (_keadaan == KeadaanSekte.Mati) { TickMati(dt); return; }
            if (_andi == null) { Diam(dt); return; }
            _takut = Mathf.Max(0f, _takut - TurunTakut * dt);
            _jedaSerang -= dt; _tJalur -= dt; _tSuara -= dt;
            _tPersepsi -= dt;
            if (_tPersepsi <= 0f)
            {
                Persepsi(); _tPersepsi = TickPersepsi * (JarakAndi() > 120f ? 3f : 1f);
                if (_keadaan == KeadaanSekte.Patroli || _keadaan == KeadaanSekte.Selidik || _keadaan == KeadaanSekte.Mengintai || _keadaan == KeadaanSekte.Serang) CobaSeret();
            }
            TickLingkungan(dt);
            TickBakarMusuh(dt);
            if (_keadaan == KeadaanSekte.Mati) return;
            if (_tGoyah > 0f)
            {
                _tGoyah -= dt;
                if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Goyah;
                Diam(dt);
                return;
            }
            switch (_keadaan)
            {
                case KeadaanSekte.Patroli: TickPatroli(dt); break;
                case KeadaanSekte.Selidik: TickSelidik(dt); break;
                case KeadaanSekte.Mengintai: TickIntai(dt); break;
                case KeadaanSekte.Isyarat: TickIsyarat(dt); break;
                case KeadaanSekte.Gertak: TickGertak(dt); break;
                case KeadaanSekte.Serang: TickSerang(dt); break;
                case KeadaanSekte.Lari: TickLari(dt); break;
                case KeadaanSekte.Terluka: TickTerluka(dt); break;
                case KeadaanSekte.Menyerah: TickMenyerah(dt); break;
                case KeadaanSekte.Ritual: TickRitual(dt); break;
                case KeadaanSekte.Serbu: TickSerbu(dt); break;
                case KeadaanSekte.SeretTeman: TickSeret(dt); break;
            }
        }

        public void Ubah(KeadaanSekte k)
        {
            if (k == _keadaan) return;
            if (_keadaan == KeadaanSekte.SeretTeman) LepasSeret();
            if (_keadaan == KeadaanSekte.Serang) LepasToken();
            _keadaan = k; _tState = 0f; _fase = FaseSerang.Dekati; _tFase = 0f;
            _jalur.Clear(); _iJalur = 0; _tJalur = 0f;
            if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak;
            if (prompt != null) prompt.SetActive(k == KeadaanSekte.Menyerah || (k == KeadaanSekte.Mati && _jarahan.Count > 0));
            if (k == KeadaanSekte.Lari) _titikLari = TitikMenjauh(JarakLari);
            if (k == KeadaanSekte.Mengintai) _lamaIntai = Random.Range(30f, 60f);
        }

        // ---------------- Persepsi ----------------
        float JarakAndi() { return _andi != null ? Datar(_andi.transform.position - transform.position).magnitude : 9999f; }
        static Vector3 Datar(Vector3 v) { v.y = 0f; return v; }
        Vector3 Mata() { return transform.position + Vector3.up * (_cc != null ? _cc.height * 0.9f : 1.6f); }

        void Persepsi()
        {
            bool malam = _jam != null && _jam.Malam;
            bool cahaya = _kiri != null && _kiri.OborMenyala;
            bool badai = _cuaca != null && _cuaca.Cuaca == WeatherState.HujanBadai;
            Vector3 mata = Mata();
            Vector3 kepala = _andi.Kamera != null ? _andi.Kamera.position : _andi.transform.position + Vector3.up * 1.6f;
            Vector3 pinggul = _andi.transform.position + Vector3.up * 0.9f;
            float jarak = Vector3.Distance(mata, kepala), jl = StatMusuh.JarakLihat(_d, malam, cahaya), f = 0f;
            if (jarak <= jl && _vital != null && _vital.IsAlive)
            {
                bool kerucut = DalamKerucut(transform.forward, kepala - mata);
                bool halang = kerucut && Terhalang(mata, kepala) && Terhalang(mata, pinggul);
                f = FaktorTerlihat(kerucut, halang, _andi.Jongkok, false, malam, cahaya, badai);
                if (_reaksiAndi == null) _reaksiAndi = GameServices.Get<ReaksiAndi>();
                if (_reaksiAndi != null && _reaksiAndi.Berlumpur) f *= ReaksiAndi.FaktorLumpur; // kamuflase lumpur 17 §2.4
            }
            _terlihat = f > 0f;
            if (_terlihat) { _posTerakhir = _andi.transform.position; _tTakTerlihat = 0f; } else _tTakTerlihat += TickPersepsi;
            _sadar = LangkahKesadaran(_sadar, f, jarak, jl, TickPersepsi);
            MeterSuara.Bunyi b;
            if (MeterSuara.Dengar(mata, _d.dengar, ref _nomorSuara, out b))
            {
                _sadar = Mathf.Min(AmbangTerlibat, Mathf.Max(_sadar + 30f, AmbangPenasaran + 1f));
                _posSelidik = b.pos; _adaSelidik = true;
            }
        }

        static int MaskHalang() { return (1 << GameLayers.Ground) | (1 << GameLayers.Tree) | (1 << GameLayers.Structure) | 1; }
        static bool Terhalang(Vector3 a, Vector3 b) { return Physics.Linecast(a, b, MaskHalang(), QueryTriggerInteraction.Ignore); }

        public void Waspadai(Vector3 pos, float tambahSadar)
        {
            if (!IsAlive) return;
            _sadar = Mathf.Min(AmbangTerlibat, _sadar + tambahSadar);
            _posSelidik = pos; _posTerakhir = pos; _adaSelidik = true;
        }

        void Terlibat()
        {
            float agresi = _dir != null ? _dir.Kemarahan : 10f;
            var k = PutuskanTerlibat(jenis, _sudahIsyarat, _diserang, agresi, _takut, Random.value);
            if (Pengintai && !_diserang && k == KeadaanSekte.Serang) k = KeadaanSekte.Mengintai;
            Ubah(k);
        }

        // ---------------- State ----------------
        void TickPatroli(float dt)
        {
            if (_sadar >= AmbangTerlibat) { Terlibat(); return; }
            if (_sadar >= AmbangWaspada || (_adaSelidik && _sadar >= AmbangPenasaran)) { Ubah(KeadaanSekte.Selidik); return; }
            if (WaktuRitual() && _sadar < AmbangPenasaran) { Ubah(KeadaanSekte.Ritual); return; }
            if (_sadar >= AmbangPenasaran) { Hadap(_andi.transform.position - transform.position, dt); Diam(dt); return; } // menoleh penasaran
            if (Pengintai)
            {
                if (JarakAndi() <= IntaiMaks + 5f) { Ubah(KeadaanSekte.Mengintai); return; }
                Menuju(_andi.transform.position, _d.jalan * 1.4f, dt);
                return;
            }
            if (Rute == null || Rute.Length == 0) { Diam(dt); return; }
            var wp = Rute[_iRute % Rute.Length];
            if (Datar(wp - transform.position).magnitude < 1.5f) _iRute = (_iRute + 1) % Rute.Length;
            Menuju(wp, _d.jalan, dt);
        }

        void TickSelidik(float dt)
        {
            if (_sadar >= AmbangTerlibat) { Terlibat(); return; }
            Vector3 tuju = _adaSelidik ? _posSelidik : _posTerakhir;
            if (_tSuara <= 0f && _tState < 1f) { Suara(ProceduralAudioSynth.KlipGeramSekte, 0.7f); _tSuara = 8f; }
            if (Datar(tuju - transform.position).magnitude > 2f && _tState < 25f) { Menuju(tuju, _d.jalan * 1.3f, dt); return; }
            Diam(dt);
            transform.Rotate(0f, Mathf.Sin(_tState * 1.3f) * 60f * dt, 0f); // menengok sekeliling
            if (_tState > 30f || (_sadar < AmbangPenasaran && _tState > 6f)) { _adaSelidik = false; Ubah(KeadaanSekte.Patroli); }
        }

        void TickIntai(float dt)
        {
            float d = JarakAndi();
            float agresi = _dir != null ? _dir.Kemarahan : 10f;
            if (d < IntaiMundur) { Ubah(KeadaanSekte.Lari); _titikLari = TitikMenjauh(IntaiMaks); return; }
            if (!Pengintai && agresi > AmbangGertakAgresi) { Ubah(KeadaanSekte.Gertak); return; }
            if (Pengintai && _tState > _lamaIntai) { Pengintai = false; Ubah(KeadaanSekte.Lari); _titikLari = TitikMenjauh(160f); return; }
            if (_tTakTerlihat > LamaLupa * 2f && d > JarakLupa) { Ubah(KeadaanSekte.Patroli); return; }
            Vector3 dariAndi = Datar(transform.position - _andi.transform.position);
            if (dariAndi.sqrMagnitude < 0.01f) dariAndi = -_andi.transform.forward;
            Vector3 target = _andi.transform.position + dariAndi.normalized * ((IntaiMin + IntaiMaks) * 0.5f);
            if (d < IntaiMin || d > IntaiMaks) { if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak; Menuju(target, d > IntaiMaks ? _d.lari * 0.6f : _d.jalan, dt); }
            else
            {
                Diam(dt);
                if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Intai;
            }
            Hadap(_andi.transform.position - transform.position, dt);
        }

        void TickIsyarat(float dt)
        {
            Diam(dt);
            Hadap(_posTerakhir - transform.position, dt);
            if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Kentongan;
            if (!_sudahIsyarat)
            {
                _sudahIsyarat = true;
                if (_dir != null) _dir.Isyarat(transform.position, _posTerakhir);
                if (_audio != null) _audio.PlayKentonganBamboo(transform.position + Vector3.up * 1.2f);
                _tSuara = 1.2f;
            }
            else if (_tSuara <= 0f && _tState < LamaIsyarat - 0.6f) { if (_audio != null) _audio.PlayKentonganBamboo(transform.position + Vector3.up * 1.2f); _tSuara = 1.2f; }
            if (_tState >= LamaIsyarat) { _diserang = true; Terlibat(); }
        }

        void TickGertak(float dt)
        {
            float d = JarakAndi();
            Vector3 keAndi = _andi.transform.position - transform.position;
            Hadap(keAndi, dt);
            if (_tState < 1.2f && d > 7f) { Gerak(_andi.transform.position, _d.lari * 0.8f, dt, true); return; }
            if (_tState < 2.8f)
            {
                Diam(dt);
                if (_anim != null && _anim.Pose != ProceduralAnimator.PoseTempur.Gertak) { _anim.Pose = ProceduralAnimator.PoseTempur.Gertak; Suara(ProceduralAudioSynth.KlipTeriakSekte, 1f); }
                return;
            }
            if (_tState < 4f) { if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak; Gerak(transform.position - Datar(keAndi).normalized * 3f, _d.jalan * 2f, dt, false); return; }
            float agresi = _dir != null ? _dir.Kemarahan : 10f;
            Ubah(_diserang || agresi >= AmbangSerangAgresi ? KeadaanSekte.Serang : KeadaanSekte.Mengintai);
        }

        void TickLari(float dt)
        {
            if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak;
            if (Datar(_titikLari - transform.position).magnitude > 2f && _tState < LamaLari) { Menuju(_titikLari, _d.lari, dt); return; }
            Diam(dt);
            if (_tState >= LamaLari) { _sadar = Mathf.Min(_sadar, 20f); _diserang = false; _takut *= 0.5f; Ubah(KeadaanSekte.Patroli); }
        }

        void TickTerluka(float dt)
        {
            if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Merangkak;
            if (Diseret) { _vy = 0f; return; }
            if (_tSuara <= 0f) { Suara(ProceduralAudioSynth.KlipRintihSekte, 0.8f); _tSuara = Random.Range(3.5f, 6f); }
            Gerak(TitikMenjauhDari(_andi.transform.position, 10f), 0.4f, dt, true);
        }

        void TickMenyerah(float dt)
        {
            Diam(dt);
            Hadap(_andi.transform.position - transform.position, dt);
            if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Menyerah;
            if (_tState >= LamaMenyerah && JarakAndi() > 6f) { Ubah(KeadaanSekte.Lari); }
        }

        void TickRitual(float dt)
        {
            if (!WaktuRitual() || _sadar >= AmbangWaspada) { Ubah(_sadar >= AmbangWaspada ? KeadaanSekte.Selidik : KeadaanSekte.Patroli); return; }
            _sudutRitual += 18f * dt;
            float r = _sudutRitual * Mathf.Deg2Rad;
            Vector3 p = TitikRitual + new Vector3(Mathf.Cos(r), 0f, Mathf.Sin(r)) * RadiusRitual;
            if (Datar(p - transform.position).magnitude > 1.2f) { if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak; Menuju(p, _d.jalan, dt); }
            else { Gerak(p, 0.9f, dt, true); if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tari; }
        }

        bool WaktuRitual()
        {
            if (_jam == null || TitikRitual == Vector3.zero || Pengintai) return false;
            float j = _jam.Jam;
            return (j >= 19f || j < 4f) && Datar(TitikRitual - _posLahir).magnitude < 160f;
        }

        // ---------------- Damage & interaksi ----------------
        public void TakeDamage(DamageInfo info)
        {
            if (!IsAlive || info.amount <= 0f) return;
            Ambil();
            if (_keadaan == KeadaanSekte.Menyerah) { if (_vital != null) _vital.UbahSanity(SanitySerangMenyerah); var ev = OnMenyerahDiserang; if (ev != null) ev(); }
            bool penghabisan = _combat != null && _combat.SedangPenghabisan && StatMusuh.Sekte(jenis);
            ZonaKena zona; bool putusLengan, putusKaki;
            float dmg = LukaiZona(info, penghabisan, out zona, out putusLengan, out putusKaki);
            _hp -= dmg;
            _diserang = true; _sadar = AmbangTerlibat;
            if (info.source != null) _posTerakhir = info.source.transform.position;
            if (info.type == DamageType.Api) Takut += TakutApi;
            if (_hp <= 0f || penghabisan) { Mati(penghabisan, info.direction * (info.isHeavy ? 4f : 2f)); return; }
            float r = _hp / _d.hp;
            _tGoyah = Mathf.Max(_tGoyah, LamaReaksi(jenis, info.isHeavy, r));
            if (_keadaan == KeadaanSekte.Terluka || _keadaan == KeadaanSekte.Menyerah) return;
            if (putusKaki) { Ubah(KeadaanSekte.Terluka); return; }
            if (putusLengan) { Takut += 30f; Ubah(KeadaanSekte.Lari); return; }
            bool temanDekat = _dir != null && _dir.AdaTemanDekat(this, RadiusTemanMenyerah);
            if (HarusTerluka(r, Random.value)) Ubah(KeadaanSekte.Terluka);
            else if (HarusMenyerah(r, _takut, temanDekat)) Ubah(KeadaanSekte.Menyerah);
            else if (HarusLari(_takut, r, Random.value)) Ubah(KeadaanSekte.Lari);
            else if (_keadaan != KeadaanSekte.Serang) Ubah(KeadaanSekte.Serang);
        }

        public void Diparry() { _tGoyah = 1.2f; Takut += TakutParry; _fase = FaseSerang.Jeda; _jedaSerang = 1.5f; }

        void Mati(bool penghabisan, Vector3 impuls)
        {
            _hp = 0f;
            LepasToken();
            LepasSeret(); _diseretOleh = null;
            _jarahan.Clear();
            for (int i = 0; i < _d.drop.Length; i++) if (Random.value < _d.peluang[i]) _jarahan.Add(new ItemStack { id = _d.drop[i], count = _d.jumlah[i] });
            Ubah(KeadaanSekte.Mati);
            if (_cc != null) _cc.enabled = false;
            if (_anim != null) { _anim.Pose = ProceduralAnimator.PoseTempur.Tidak; _anim.enabled = false; }
            if (Abu()) { Suara(ProceduralAudioSynth.KlipAbu, 1f); if (StatMusuh.Sekte(jenis)) _jarahan.Add(new ItemStack { id = ItemID.TopengSekte, count = 1 }); }
            else MatiGore(impuls);
            if (prompt != null) prompt.SetActive(PromptMayat().Length > 0);
            if (_dir != null) _dir.LaporMati(this);
            GameEvents.RaiseEnemyKilled(jenis, false);
            if (penghabisan) GameEvents.RaiseToast("Tebasan Penghabisan");
        }

        static bool Abu() { return EnemyDirector.TingkatGore == GoreLevel.Mati; }

        void TickMati(float dt)
        {
            if (Abu())
            {
                float u = Mathf.Clamp01(_tState / LamaMatiAbu);
                for (int i = 0; i < _rend.Length; i++)
                {
                    if (_rend[i] == null) continue;
                    _rend[i].GetPropertyBlock(_mpb);
                    _mpb.SetColor("_BaseColor", Color.Lerp(Color.white, new Color(0.05f, 0.05f, 0.05f), u * 2f));
                    _rend[i].SetPropertyBlock(_mpb);
                }
                transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.05f, Mathf.Max(0f, u - 0.4f) / 0.6f);
                if (u >= 1f && _jarahan.Count == 0) gameObject.SetActive(false);
                return;
            }
            if (_ragdoll) return;
            // Tanpa ragdoll (cadangan): tumbang telentang 0,5 s lalu diam sebagai mayat.
            float t = Mathf.Clamp01(_tState / 0.5f);
            var e = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(Mathf.Lerp(0f, -88f, t * t), e.y, 0f);
            float h = WorldTerrainGenerator.SampleHeight(transform.position.x, transform.position.z);
            if (transform.position.y > h + 0.25f) transform.position = new Vector3(transform.position.x, Mathf.Max(h + 0.25f, transform.position.y - 3f * dt), transform.position.z);
        }

        public string GetPromptText(GameObject actor) { return _keadaan == KeadaanSekte.Menyerah ? "AMPUNI" : PromptMayat(); }

        public bool CanInteract(GameObject actor)
        {
            return _keadaan == KeadaanSekte.Menyerah || (_keadaan == KeadaanSekte.Mati && PromptMayat().Length > 0);
        }

        public void Interact(GameObject actor)
        {
            Ambil();
            if (_keadaan == KeadaanSekte.Menyerah) { Ampuni(); return; }
            if (_keadaan != KeadaanSekte.Mati) return;
            if (PromptMayat() != "GELEDAH")
            {
                InteraksiMayat();
                if (prompt != null) prompt.SetActive(PromptMayat().Length > 0);
                return;
            }
            _digeledah = true;
            var inv = GameServices.Get<TikarPandan3DInventory>();
            var sb = new System.Text.StringBuilder("Dapat: ");
            for (int i = 0; i < _jarahan.Count; i++)
            {
                if (inv != null) inv.Tambah(_jarahan[i].id, _jarahan[i].count);
                if (i > 0) sb.Append(", ");
                sb.Append(ItemDatabase.Get(_jarahan[i].id).displayName).Append(" x").Append(_jarahan[i].count);
            }
            GameEvents.RaiseToast(sb.ToString());
            _jarahan.Clear();
            if (prompt != null) prompt.SetActive(PromptMayat().Length > 0);
            if (Abu()) gameObject.SetActive(false);
        }

        void Ampuni()
        {
            if (_vital != null) _vital.UbahSanity(SanityAmpuni);
            if (_dir != null) _dir.LaporAmpuni(this);
            GameEvents.RaiseEnemyKilled(jenis, true);
            GameEvents.RaiseToast("Ia membungkuk lalu lari ke hutan");
            _diserang = false; _sadar = 0f; Pengintai = false;
            Ubah(KeadaanSekte.Lari);
            _titikLari = TitikMenjauh(160f);
        }
    }
}
