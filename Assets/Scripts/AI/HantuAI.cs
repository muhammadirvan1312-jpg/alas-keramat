using System.Collections.Generic;
using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.AI
{
    // 06 §8 (T7b): hantu - Pocong Hutan/Gua (lompat, terjang 6 m), Kuntilanak (bertengger, mengitari, jeritan setrum, tukikan, menghilang
    // ke belakang Andi), Banaspati (mengorbit 6 m lalu menerjang, padam oleh air/hujan, membakar). Hantu tidak mati: HP 0 = "diusir".
    public partial class HantuAI : MonoBehaviour, IDamageable, IInteractable
    {
        public enum FaseHantu { Diam, Mendekat, Ancang, Terjang, Pulih, Terjungkal, Mengitari, Menjerit, Hilang, Diusir }

        public const float PocongIndra = 30f, PocongIndraTakut = 45f, PocongJarakTerjang = 6f, PocongAncang = 0.5f, PocongJangkauTerjang = 3.5f, PocongLamaTerjang = 0.4f;
        public const float PocongPulih = 1.2f, PocongTerjungkal = 3.5f, PocongPengaliRentan = 1.5f, PengaliApi = 2f, PengaliKeris = 2f;
        public const float KuntiIndra = 35f, KuntiRadiusKitar = 10f, KuntiJedaJerit = 15f, KuntiAncang = 0.6f, KuntiLajuTukik = 12f, KuntiRentan = 1.5f;
        public const float KuntiLamaSorot = 2f, KuntiHilang = 8f, KuntiTawaMin = 20f, KuntiTawaMaks = 40f, SanityJerit = -5f, LamaSetrum = 1f;
        public const float BanaspatiOrbit = 6f, BanaspatiIndra = 40f, BanaspatiLajuTerjang = 9f, BakarBanaspati = 4f, LamaBakar = 3f;
        public const float RadiusKemenyan = 20f, RadiusOborTengkorak = 15f, LamaLarut = 2.5f, Gravitasi = 18f;

        public static readonly List<HantuAI> Semua = new List<HantuAI>();

        [SerializeField] EnemyArchetype jenis = EnemyArchetype.PocongHutan;
        [SerializeField] Light cahaya;
        [SerializeField] GameObject titikInteraksi; // T9: LEPAS TALI POCONG / TANCAPKAN PAKU

        public EnemyArchetype Jenis { get { return jenis; } }
        public FaseHantu Fase { get { return _fase; } }
        public bool IsAlive { get { return _fase != FaseHantu.Diusir; } }
        public float HP { get { return _hp; } }
        public Vector3 Sarang { get; set; }

        StatMusuh.DataMusuh _d;
        FaseHantu _fase;
        float _hp, _t, _tTawa, _tJerit, _tSorot, _vy, _sudut, _tBakar;
        bool _setengahHilang, _sudahKena, _siap, _logError;
        Vector3 _arahTerjang;
        AndiFPSController _andi;
        AndiVitalsAndSanity _vital;
        CombatAndLeftHandController _combat;
        TanganKiri _kiri;
        ProceduralAudioSynth _audio;
        URPAtmosphereAndWeather _cuaca;
        ProceduralAnimator _anim;
        Renderer[] _rend;
        MaterialPropertyBlock _mpb;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatis() { Semua.Clear(); }

        public void Pasang(EnemyArchetype j, Light l) { jenis = j; cahaya = l; }
        public void PasangInteraksi(GameObject g) { titikInteraksi = g; }

        void Awake()
        {
            _d = StatMusuh.Ambil(jenis);
            _hp = _d.hp;
            _anim = GetComponent<ProceduralAnimator>();
            _rend = GetComponentsInChildren<Renderer>(true);
            _mpb = new MaterialPropertyBlock();
            Sarang = transform.position;
            _tTawa = Random.Range(KuntiTawaMin, KuntiTawaMaks) * 0.3f;
            _sudut = Random.value * 360f;
            _siap = true;
        }

        void OnEnable() { if (!Semua.Contains(this)) Semua.Add(this); }
        void OnDisable() { Semua.Remove(this); }

        void Ambil()
        {
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            if (_vital == null) _vital = GameServices.Get<AndiVitalsAndSanity>();
            if (_combat == null) _combat = GameServices.Get<CombatAndLeftHandController>();
            if (_kiri == null) _kiri = GameServices.Get<TanganKiri>();
            if (_audio == null) _audio = GameServices.Get<ProceduralAudioSynth>();
            if (_cuaca == null) _cuaca = GameServices.Get<URPAtmosphereAndWeather>();
        }

        void Update()
        {
            if (!_siap) return;
            try
            {
                Ambil();
                float dt = Time.deltaTime;
                _t += dt;
                if (_tBakar > 0f) _tBakar -= dt;
                TickApi(dt);
                SinkronInteraksi();
                if (_fase == FaseHantu.Diusir) { TickLarut(); return; }
                if (_andi == null) return;
                if (jenis == EnemyArchetype.Kuntilanak) TickKuntilanak(dt);
                else if (jenis == EnemyArchetype.Banaspati) TickBanaspati(dt);
                else if (jenis == EnemyArchetype.Kuyang) TickKuyang(dt);
                else if (jenis == EnemyArchetype.Tuyul) TickTuyul(dt);
                else if (jenis == EnemyArchetype.HantuAir || jenis == EnemyArchetype.GenderuwoHutan) TickPemburuMalam(dt);
                else if (jenis == EnemyArchetype.Penyerupa) TickPenyerupa(dt);
                else TickPocong(dt);
            }
            catch (System.Exception e)
            {
                if (!_logError) { Debug.LogError("[AK][Hantu] " + name + " galat: " + e.Message); _logError = true; }
            }
        }

        void Ubah(FaseHantu f)
        {
            _fase = f; _t = 0f; _sudahKena = false;
            if (f == FaseHantu.Diusir)
            {
                var gore = GameServices.Get<GoreSystem>();
                if (gore != null) gore.PercikPartikel(transform.position + Vector3.up * 0.9f, new Color(0.14f, 0.14f, 0.17f, 0.78f), 14, 0.14f, 1.3f, 0.95f);
            }
        }

        // ---------- Rumus murni (self-test) ----------
        public static float IndraPocong(float sanity) { return sanity < 25f ? PocongIndraTakut : PocongIndra; }
        public static float PengaliDamage(EnemyArchetype j, DamageType tipe, ItemID senjata, bool rentan)
        {
            float k = 1f;
            if (tipe == DamageType.Api && (j == EnemyArchetype.PocongHutan || j == EnemyArchetype.PocongGua)) k *= PengaliApi;
            if (senjata == ItemID.KerisPusaka) k *= PengaliKeris;
            if (rentan) k *= PocongPengaliRentan;
            return k;
        }
        public static bool BanaspatiPadam(bool hujan, bool diAir) { return hujan || diAir; }

        // Kemenyan (20 m, 60 s) & Obor Tengkorak (15 m) menolak Pocong & hantu lain.
        public static bool TerusirPenangkal(Vector3 p) { return ZonaPenangkal.Ada(p, RadiusKemenyan, RadiusOborTengkorak); }

        // ---------- Damage ----------
        public void TakeDamage(DamageInfo info)
        {
            if (!IsAlive || info.amount <= 0f || _fase == FaseHantu.Hilang) return;
            if (jenis == EnemyArchetype.Tuyul) { Ubah(FaseHantu.Hilang); TampakSemua(false); GameEvents.RaiseToast("Tuyul kabur menghilang dalam gelap"); return; } // 14 §1: tanpa gore
            Ambil();
            bool rentan = _fase == FaseHantu.Pulih || _fase == FaseHantu.Terjungkal;
            var senjata = _combat != null ? _combat.Aktif.id : ItemID.None;
            if (info.source == null || _combat == null || info.source != _combat.gameObject) senjata = ItemID.None;
            _hp -= info.amount * PengaliDamage(jenis, info.type, senjata, rentan);
            Suara(ProceduralAudioSynth.KlipKenaDaging, 0.6f);
            var gore = GameServices.Get<GoreSystem>();
            if (gore != null && jenis != EnemyArchetype.Banaspati) gore.Semburan(info.point, info.direction, info.amount, true, info.isHeavy); // darah hitam hantu (14 §3.3)
            if (_hp <= 0f) { Usir(); return; }
            if ((jenis == EnemyArchetype.PocongHutan || jenis == EnemyArchetype.PocongGua) && info.type == DamageType.Api && _fase != FaseHantu.Terjungkal) Ubah(FaseHantu.Terjungkal); // 13 §4.2: roboh oleh api
            if (jenis == EnemyArchetype.Kuntilanak && !_setengahHilang && _hp <= _d.hp * 0.5f) { _setengahHilang = true; Ubah(FaseHantu.Hilang); TampakSemua(false); }
        }

        void Usir()
        {
            _hp = 0f;
            Ubah(FaseHantu.Diusir);
            if (jenis == EnemyArchetype.Kuntilanak && _audio != null) _audio.PlayKuntilanakLaugh(transform.position);
            var dir = GameServices.Get<EnemyDirector>();
            if (dir != null) dir.LaporHantuDiusir(this);
            GameEvents.RaiseEnemyKilled(jenis, false);
            GameEvents.RaiseToast(NamaTampil(jenis) + " diusir");
            for (int i = 0; i < _d.drop.Length; i++)
                if (Random.value < _d.peluang[i]) JarahanTanah.Jatuhkan(transform.position + Vector3.up * 0.3f, _d.drop[i], _d.jumlah[i]);
        }

        float _dpsApi, _tApi, _tTickApi;
        public void Bakar(float dps, float lama) { if (IsAlive && jenis != EnemyArchetype.Banaspati) { _dpsApi = Mathf.Max(_dpsApi, dps); _tApi = Mathf.Max(_tApi, lama); } }

        void TickApi(float dt)
        {
            if (_tApi <= 0f) return;
            _tApi -= dt; _tTickApi += dt;
            if (_tTickApi < 1f) return;
            _tTickApi -= 1f;
            TakeDamage(new DamageInfo { amount = _dpsApi, type = DamageType.Api, point = transform.position + Vector3.up, direction = Vector3.up, source = null });
        }

        // ---------- 13 §4.1-4.2: folklor sebagai mekanik ----------
        public const float LamaLepasTali = 1.2f, LamaPaku = 0.8f, SanityLepasTali = 8f;
        public static int ArwahDilepas { get; private set; }

        bool BisaLepasTali { get { return (jenis == EnemyArchetype.PocongHutan || jenis == EnemyArchetype.PocongGua) && _fase == FaseHantu.Terjungkal; } }
        bool BisaPaku
        {
            get
            {
                if (jenis != EnemyArchetype.Kuntilanak || _fase != FaseHantu.Pulih) return false;
                var inv = GameServices.Get<AlasKeramat.Inventory.TikarPandan3DInventory>();
                return inv != null && inv.Punya(ItemID.PakuBesiKuning);
            }
        }

        public float HoldDuration { get { return BisaPaku ? LamaPaku : LamaLepasTali; } }
        public string GetPromptText(GameObject actor) { return BisaLepasTali ? "LEPAS TALI POCONG" : BisaPaku ? "TANCAPKAN PAKU" : ""; }
        public bool CanInteract(GameObject actor) { return BisaLepasTali || BisaPaku; }

        public void Interact(GameObject actor)
        {
            Ambil();
            if (BisaLepasTali)
            {
                ArwahDilepas++;
                if (_vital != null) _vital.UbahSanity(SanityLepasTali);
                JarahanTanah.Jatuhkan(transform.position + Vector3.up * 0.2f, ItemID.Kemenyan, 1);
                JarahanTanah.Jatuhkan(transform.position + Vector3.right * 0.4f, ItemID.KainKafan, 1);
                GameEvents.RaiseSubtitle("(Tali kafan terlepas. Pocong itu runtuh menjadi kain kosong.)");
                Ubah(FaseHantu.Diusir);
                var dir = GameServices.Get<EnemyDirector>(); if (dir != null) dir.LaporHantuDiusir(this);
                return;
            }
            if (BisaPaku)
            {
                var inv = GameServices.Get<AlasKeramat.Inventory.TikarPandan3DInventory>();
                if (inv == null || !inv.Kurangi(ItemID.PakuBesiKuning, 1)) return;
                EnemyDirector.KuntiTerikat = true;
                GameEvents.RaiseSubtitle("(Paku menembus tengkuknya. Jeritannya berhenti. Ia tidak akan kembali.)");
                Ubah(FaseHantu.Diusir);
                var dir = GameServices.Get<EnemyDirector>(); if (dir != null) dir.LaporHantuDiusir(this);
            }
        }

        void SinkronInteraksi() { if (titikInteraksi != null) { bool aktif = BisaLepasTali || (jenis == EnemyArchetype.Kuntilanak && _fase == FaseHantu.Pulih); if (titikInteraksi.activeSelf != aktif) titikInteraksi.SetActive(aktif); } }

        public static string NamaTampil(EnemyArchetype j)
        {
            switch (j) { case EnemyArchetype.PocongHutan: case EnemyArchetype.PocongGua: return "Pocong"; case EnemyArchetype.Kuntilanak: return "Kuntilanak"; case EnemyArchetype.Banaspati: return "Banaspati"; case EnemyArchetype.Kuyang: return "Kuyang"; case EnemyArchetype.Tuyul: return "Tuyul"; case EnemyArchetype.GenderuwoHutan: return "Genderuwo Hutan"; case EnemyArchetype.HantuAir: return "Hantu Air"; case EnemyArchetype.Penyerupa: return "Penyerupa"; default: return j.ToString(); }
        }

        // Larut jadi kabut/abu 2,5 s lalu nonaktif (kembali malam berikutnya lewat EnemyDirector).
        void TickLarut()
        {
            float u = Mathf.Clamp01(_t / LamaLarut);
            for (int i = 0; i < _rend.Length; i++)
            {
                if (_rend[i] == null) continue;
                _rend[i].GetPropertyBlock(_mpb);
                _mpb.SetColor("_BaseColor", Color.Lerp(Color.white, new Color(0.35f, 0.35f, 0.38f, 1f), u));
                _rend[i].SetPropertyBlock(_mpb);
            }
            transform.position += Vector3.up * Time.deltaTime * 0.6f;
            transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.1f, u);
            if (cahaya != null) cahaya.intensity *= 0.9f;
            if (u >= 1f) gameObject.SetActive(false);
        }

        public void Larutkan() { if (IsAlive) Ubah(FaseHantu.Diusir); }

        void TampakSemua(bool tampak) { for (int i = 0; i < _rend.Length; i++) if (_rend[i] != null) _rend[i].enabled = tampak; if (cahaya != null) cahaya.enabled = tampak; }

        // ---------- Utilitas ----------
        float JarakAndi() { Vector3 d = _andi.transform.position - transform.position; d.y = 0f; return d.magnitude; }
        Vector3 KeAndi() { Vector3 d = _andi.transform.position - transform.position; d.y = 0f; return d; }

        void Hadap(Vector3 arah, float dt, float laju = 300f)
        {
            arah.y = 0f;
            if (arah.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(arah), laju * dt);
        }

        void Geser(Vector3 datar, float dt, float tinggiDiAtasTanah)
        {
            Vector3 p = transform.position + datar * dt;
            float h = WorldTerrainGenerator.Lantai(p);
            float target = (WorldTerrainGenerator.DiBawahTanah(p.y) ? h : Mathf.Max(h, 0f)) + tinggiDiAtasTanah;
            p.y = Mathf.MoveTowards(p.y, target, (tinggiDiAtasTanah > 0.5f ? 6f : 30f) * dt);
            transform.position = p;
        }

        void KenaiAndi(float damage, bool bisaDiparry, DamageType tipe, out bool diparry)
        {
            diparry = false;
            if (_vital == null || !_vital.IsAlive) return;
            float akhir = _combat != null ? _combat.TerimaSerangan(transform.position, damage, bisaDiparry, out diparry) : damage;
            if (diparry) return;
            _vital.TakeDamage(new DamageInfo { amount = akhir, type = tipe, point = _andi.transform.position + Vector3.up, direction = KeAndi().normalized, source = gameObject, isHeavy = akhir >= 25f });
        }

        void Suara(string klip, float vol)
        {
            if (_audio != null) _audio.Play(klip, transform.position + Vector3.up * 1.2f, AudioCategory.Efek, vol, Random.Range(0.85f, 1.05f), 3);
        }
    }
}
