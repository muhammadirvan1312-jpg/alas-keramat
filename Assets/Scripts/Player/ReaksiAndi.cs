using AlasKeramat.AI;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Player
{
    // T7a: (1) meter suara langkah Andi untuk pendengaran Sekte (06 §5: lari 20 m, jalan 8 m, jongkok 3 m),
    // (2) reaksi terkena (06 §3): guncang kamera, erang pelan, kena >= 25 = terjatuh 0,8 s (gerak dikunci, pandangan merunduk).
    public class ReaksiAndi : MonoBehaviour
    {
        public const float JedaLangkah = 0.5f, AmbangJatuh = 25f, LamaJatuh = 0.8f, PitchJatuh = 25f;

        AndiFPSController _ctrl;
        CombatAndLeftHandController _combat;
        float _tLangkah, _tJatuh, _pengaliAsal = 1f;

        public bool Terjatuh { get { return _tJatuh > 0f; } }

        void Awake()
        {
            GameServices.Register(this);
            _ctrl = GetComponent<AndiFPSController>();
            _combat = GetComponent<CombatAndLeftHandController>();
        }

        void OnEnable() { GameEvents.OnPlayerDamaged += Kena; }
        void OnDisable() { GameEvents.OnPlayerDamaged -= Kena; }

        public static bool HarusJatuh(float damage) { return damage >= AmbangJatuh; }

        void Kena(DamageInfo d)
        {
            if (d.type == DamageType.Tenggelam) return;
            if (_combat != null) _combat.Guncang(HarusJatuh(d.amount) ? 0.3f : 0.15f);
            var a = GameServices.Get<ProceduralAudioSynth>();
            if (a != null && d.amount >= 3f) a.Play(ProceduralAudioSynth.KlipKenaDaging, null, AudioCategory.Efek, 0.7f, Random.Range(0.8f, 0.95f), 3);
            if (HarusJatuh(d.amount) && _ctrl != null && _tJatuh <= 0f)
            {
                _pengaliAsal = _ctrl.PengaliKecepatan;
                _ctrl.PengaliKecepatan = 0f;
                _ctrl.InputLihatPx.y -= PitchJatuh / AndiFPSController.SensV;
                _tJatuh = LamaJatuh;
            }
        }

        float _tBakar, _dpsBakar, _tTickBakar;

        // Setrum jeritan Kuntilanak (06 §8): gerak terkunci sesaat.
        public void Setrum(float lama)
        {
            if (_ctrl == null || _tJatuh > 0f) return;
            _pengaliAsal = _ctrl.PengaliKecepatan; _ctrl.PengaliKecepatan = 0f; _tJatuh = lama;
            if (_combat != null) _combat.Guncang(0.4f);
        }

        // Terbakar (Banaspati/api): damage per detik; padam bila berenang (lompat ke air).
        public void Bakar(float dps, float lama) { _dpsBakar = Mathf.Max(_dpsBakar, dps); _tBakar = Mathf.Max(_tBakar, lama); }
        public bool Terbakar { get { return _tBakar > 0f; } }

        void TickBakar(float dt)
        {
            if (_tBakar <= 0f) return;
            if (_ctrl != null && _ctrl.Berenang) { _tBakar = 0f; GameEvents.RaiseToast("Api padam"); return; }
            _tBakar -= dt; _tTickBakar += dt;
            if (_tTickBakar < 1f) return;
            _tTickBakar -= 1f;
            var v = GameServices.Get<AndiVitalsAndSanity>();
            if (v != null) v.TakeDamage(new DamageInfo { amount = _dpsBakar, type = DamageType.Api, point = transform.position + Vector3.up, direction = Vector3.up, source = gameObject });
        }

        void Update()
        {
            if (_ctrl == null) return;
            TickBakar(Time.deltaTime);
            if (_tJatuh > 0f)
            {
                _tJatuh -= Time.deltaTime;
                if (_tJatuh <= 0f) _ctrl.PengaliKecepatan = _pengaliAsal > 0f ? _pengaliAsal : 1f;
            }
            _tLangkah -= Time.deltaTime;
            if (_tLangkah > 0f) return;
            _tLangkah = JedaLangkah;
            float r = MeterSuara.RadiusLangkah(_ctrl.SedangLari, _ctrl.Jongkok, _ctrl.KecepatanDatar);
            if (r > 0f) MeterSuara.Pancar(transform.position, r);
        }
    }
}
