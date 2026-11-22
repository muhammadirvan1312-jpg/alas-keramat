using AlasKeramat.Core;
using AlasKeramat.Player;
using AlasKeramat.UI;
using UnityEngine;
namespace AlasKeramat.AI
{
    // Boss Genderuwo, Kubah Akar Agung UG3 K3 (06 §10): 3 fase menurut HP, serangan bertelegraf, Amukan menabrak pilar -> pingsan
    // dengan punggung bercahaya (titik lemah x2). Dibangunkan oleh KiRogoPanggung setelah adegan pembuka 04 §7.5.
    public partial class BossGenderuwo : MonoBehaviour, IDamageable
    {
        public const float HPMaks = 1500f, AmbangFase2 = 0.6f, AmbangFase3 = 0.25f, Laju = 3.4f, BonusFase3 = 1.2f, Gravitasi = 20f;
        public const float JarakSapu = 2.5f, SudutSapu = 70f, RadiusHantam = 6f, JarakLempar = 20f, LajuAkar = 18f, KenaAkar = 1.3f;
        public const float DmgHantam = 35f, DmgSapu = 25f, DmgAkar = 20f, DmgAmuk = 30f, LajuAmuk = 9f, LamaAmuk = 3f, LamaPingsan = 2.5f, PengaliPunggung = 2f;
        public const float TelegrafHantam = 1f, TelegrafSapu = 0.6f, TelegrafLempar = 1f, JedaAuman = 15f, SanityAuman = -5f, JedaSerang = 1.4f;
        public enum AksiBoss { Tidur, Kejar, Hantam, Sapu, Lempar, Amuk, Pingsan, Mati }

        [SerializeField] KiRogoPanggung kiRogo;
        [SerializeField] TungkuObor[] tungku;
        [SerializeField] Transform akarLempar;
        [SerializeField] Transform cincinDebu;
        [SerializeField] Light punggung;

        public AksiBoss Aksi { get; private set; }
        public float HP { get; private set; } = HPMaks;
        public int Fase { get { return FaseUntuk(HP / HPMaks); } }
        public bool IsAlive { get { return Aksi != AksiBoss.Mati; } }

        CharacterController _cc;
        ProceduralAnimator _anim;
        AndiFPSController _andi;
        CombatAndLeftHandController _combat;
        BarAtas _bar;
        float _t, _jeda, _vy, _tAuman, _tCincin = -1f;
        bool _kena, _fase3Gelap;
        Vector3 _arahAmuk, _posAkar, _arahAkar;
        float _jarakAkar = -1f;
        Light _lampuAuraBoss;
        ParticleSystem _abuAuraBoss;

        public static float HitungIntensitasAuraBoss(int fase, AksiBoss aksi)
        {
            if (aksi == AksiBoss.Tidur || aksi == AksiBoss.Mati) return 0f;
            return fase >= 3 ? 3.2f : fase == 2 ? 1.85f : 0.85f;
        }

        void PerbaruiAuraBoss()
        {
            float intAura = HitungIntensitasAuraBoss(Fase, Aksi);
            if (intAura > 0f)
            {
                if (_lampuAuraBoss == null)
                {
                    var go = new GameObject("AuraBossGenderuwo");
                    go.transform.SetParent(transform, false);
                    go.transform.localPosition = new Vector3(0f, 1.45f, 0f);
                    _lampuAuraBoss = go.AddComponent<Light>();
                    _lampuAuraBoss.type = LightType.Point;
                    _lampuAuraBoss.color = new Color(0.95f, 0.22f, 0.08f);
                    _lampuAuraBoss.range = 8.5f;
                    _lampuAuraBoss.shadows = LightShadows.None;
                    _abuAuraBoss = go.AddComponent<ParticleSystem>();
                    var m = _abuAuraBoss.main;
                    m.loop = true; m.startLifetime = 1.4f; m.startSpeed = 0.75f; m.startSize = 0.07f;
                    m.maxParticles = 36; m.gravityModifier = -0.18f; m.simulationSpace = ParticleSystemSimulationSpace.World;
                    m.startColor = new Color(1f, 0.35f, 0.10f, 0.85f);
                    var sh = _abuAuraBoss.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.85f;
                    var pr = go.GetComponent<ParticleSystemRenderer>();
                    pr.sharedMaterial = SafeMaterialFactory.CreatePBR("M_AbuAuraBoss", new Color(1f, 0.36f, 0.12f), 0f, 0.3f, null, null, new Color(3.2f, 0.85f, 0.2f));
                    pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
                _lampuAuraBoss.enabled = true;
                _lampuAuraBoss.intensity = intAura * (0.9f + 0.2f * Mathf.PerlinNoise(Time.time * 8f, 0.2f));
                var em = _abuAuraBoss.emission;
                em.rateOverTime = Fase * 7f;
                if (!_abuAuraBoss.isPlaying) _abuAuraBoss.Play();
            }
            else
            {
                if (_lampuAuraBoss != null) _lampuAuraBoss.enabled = false;
                if (_abuAuraBoss != null && _abuAuraBoss.isPlaying) _abuAuraBoss.Stop();
            }
        }

        public static int FaseUntuk(float sisa01) { return sisa01 > AmbangFase2 ? 1 : sisa01 > AmbangFase3 ? 2 : 3; }
        public static bool DariBelakang(Vector3 hadapBoss, Vector3 arahPukulan) { hadapBoss.y = 0f; arahPukulan.y = 0f; return Vector3.Dot(hadapBoss.normalized, arahPukulan.normalized) > 0.2f; }

        public void Pasang(KiRogoPanggung ki, TungkuObor[] t, Transform akar, Transform debu, Light lampuPunggung)
        { kiRogo = ki; tungku = t; akarLempar = akar; cincinDebu = debu; punggung = lampuPunggung; }

        void Awake()
        {
            GameServices.Register(this);
            _cc = GetComponent<CharacterController>();
            _anim = GetComponent<ProceduralAnimator>();
            Aksi = AksiBoss.Tidur;
            if (akarLempar != null) akarLempar.gameObject.SetActive(false);
            if (cincinDebu != null) cincinDebu.gameObject.SetActive(false);
            if (punggung != null) punggung.enabled = false;
        }

        public void Bangunkan()
        {
            if (Aksi != AksiBoss.Tidur) return;
            Aksi = AksiBoss.Kejar; _jeda = JedaSerang; _tAuman = JedaAuman;
            Suara(ProceduralAudioSynth.KlipGeramSekte, 0.45f);
            if (_combat == null) _combat = GameServices.Get<CombatAndLeftHandController>();
            if (_combat != null) _combat.Guncang(0.8f);
            TerrorDirector.Getar(3);
        }

        void Update()
        {
            if (Aksi == AksiBoss.Tidur || Aksi == AksiBoss.Mati) return;
            if (_andi == null) { _andi = GameServices.Get<AndiFPSController>(); if (_andi == null) return; }
            if (_combat == null) _combat = GameServices.Get<CombatAndLeftHandController>();
            if (_bar == null) _bar = GameServices.Get<BarAtas>();
            if (_bar != null) _bar.Tampil("GENDERUWO", HP / HPMaks, new Color(0.62f, 0.08f, 0.06f));
            float dt = Time.deltaTime;
            PerbaruiAuraBoss();
            TickFase(dt);
            TickAkar(dt);
            TickCincin(dt);
            Otak(dt);
        }

        void TickFase(float dt)
        {
            if (Fase < 3) return;
            if (!_fase3Gelap)
            {
                _fase3Gelap = true;
                if (tungku != null) for (int i = 0; i < tungku.Length; i += 2) if (tungku[i] != null) tungku[i].Padamkan();
                GameEvents.RaiseSubtitle("Tungku-tungku padam satu per satu...");
            }
            _tAuman -= dt;
            if (_tAuman <= 0f)
            {
                _tAuman = JedaAuman;
                Suara(ProceduralAudioSynth.KlipGeramSekte, 0.4f);
                var v = GameServices.Get<AndiVitalsAndSanity>(); if (v != null) v.UbahSanity(SanityAuman);
                if (_combat != null) _combat.Guncang(0.5f);
            }
        }

        float LajuKini { get { return Laju * (Fase >= 3 ? BonusFase3 : 1f); } }

        public void TakeDamage(DamageInfo info)
        {
            if (Aksi == AksiBoss.Tidur || Aksi == AksiBoss.Mati) return;
            float dmg = info.amount;
            if (Aksi == AksiBoss.Pingsan && DariBelakang(transform.forward, info.direction)) dmg *= PengaliPunggung;
            HP = Mathf.Max(0f, HP - dmg);
            Suara(ProceduralAudioSynth.KlipKenaDaging, 0.7f);
            var g = GameServices.Get<GoreSystem>();
            if (g != null) g.Semburan(info.point, info.direction, dmg, true, dmg >= 45f);
            if (HP <= 0f) Kalah();
        }

        void Kalah()
        {
            Aksi = AksiBoss.Mati;
            PerbaruiAuraBoss();
            if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Goyah;
            if (punggung != null) punggung.enabled = false;
            if (akarLempar != null) akarLempar.gameObject.SetActive(false);
            if (_bar == null) _bar = GameServices.Get<BarAtas>();
            if (_bar != null) _bar.Sembunyi();
            Suara(ProceduralAudioSynth.KlipTubuhJatuh, 0.6f);
            GameEvents.RaiseEnemyKilled(EnemyArchetype.Genderuwo, false);
            if (kiRogo != null) kiRogo.BossKalah();
            transform.rotation = Quaternion.Euler(-80f, transform.eulerAngles.y, 0f);
        }

        void Suara(string klip, float pitch)
        {
            var a = GameServices.Get<ProceduralAudioSynth>();
            if (a != null) a.Play(klip, transform.position + Vector3.up * 2f, AudioCategory.Efek, 1f, pitch, 6);
        }
    }
}
