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
