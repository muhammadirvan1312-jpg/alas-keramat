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
            if (!Berdarah && PicuPendarahan(d, Random.value)) MulaiPendarahan();
            var gore = GameServices.Get<GoreSystem>();
            if (gore != null && d.amount >= 5f && d.source != null) gore.Semburan(d.point, d.direction, d.amount * 0.5f, false, false);
            if (_combat != null && d.source != null) _combat.Guncang(HarusJatuh(d.amount) ? 0.3f : 0.15f);
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

        // ---------- Meter suara & kamuflase lumpur (17 §2.4) ----------
        public const float LamaLumpur = 600f, FaktorLumpur = 0.7f;
        public float RadiusSuara { get; private set; }
        public float SisaLumpur { get; private set; }
        public bool Berlumpur { get { return SisaLumpur > 0f; } }

        public void BalurLumpur() { SisaLumpur = LamaLumpur; GameEvents.RaiseToast("Tubuhmu berlumpur: lebih sulit terlihat (10 menit)"); }

        void TickLumpur(float dt)
        {
            if (SisaLumpur <= 0f) return;
            SisaLumpur -= dt;
            var cuaca = GameServices.Get<URPAtmosphereAndWeather>();
            bool hujan = cuaca != null && (cuaca.Cuaca == WeatherState.Gerimis || cuaca.Cuaca == WeatherState.HujanBadai);
            if ((_ctrl != null && _ctrl.Berenang) || hujan) { SisaLumpur = 0f; GameEvents.RaiseToast("Lumpur di tubuhmu luntur"); }
        }

        // ---------- Pendarahan (14 §4) ----------
        public const float PeluangPendarahanTebas = 0.25f, LajuPendarahan = 0.5f;
        float _tTickDarah, _pengaliDarah = 1f;
        public bool Berdarah { get; private set; }

        public static bool PicuPendarahan(DamageInfo d, float acak)
        {
            return d.type == DamageType.Tebas && d.source != null && d.source.GetComponent<AlasKeramat.AI.NusantaraEnemyAndMutantAI>() != null && acak < PeluangPendarahanTebas;
        }

        public void MulaiPendarahan()
        {
            if (Berdarah) return;
            Berdarah = true; _pengaliDarah = 1f;
            GameEvents.RaiseStatusChanged(StatusEffect.Pendarahan);
            GameEvents.RaiseToast("Kamu berdarah. Pakai perban kain atau jamu.");
        }

        // Dipanggil UI saat memakai item: PerbanKain/JamuHerbal menghentikan, DaunSirih memperlambat 50%.
        public bool Obati(ItemID id)
        {
            if (!Berdarah || (id != ItemID.PerbanKain && id != ItemID.JamuHerbal && id != ItemID.DaunSirih)) return false;
            var inv = GameServices.Get<AlasKeramat.Inventory.TikarPandan3DInventory>();
            if (inv == null || !inv.Kurangi(id, 1)) return false;
            if (id == ItemID.DaunSirih) { _pengaliDarah = 0.5f; GameEvents.RaiseToast("Daun sirih memperlambat pendarahan"); return true; }
            Berdarah = false;
            GameEvents.RaiseStatusChanged(StatusEffect.None);
            GameEvents.RaiseToast("Pendarahan berhenti");
            return true;
        }

        void TickDarah(float dt)
        {
            if (!Berdarah) return;
            _tTickDarah += dt;
            if (_tTickDarah < 1f) return;
            _tTickDarah -= 1f;
            var v = GameServices.Get<AndiVitalsAndSanity>();
            if (v == null) return;
            if (!v.IsAlive) { Berdarah = false; return; }
            v.TakeDamage(new DamageInfo { amount = LajuPendarahan * _pengaliDarah, type = DamageType.Tebas, point = transform.position + Vector3.up, direction = Vector3.down, source = null });
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
            TickDarah(Time.deltaTime);
            TickLumpur(Time.deltaTime);
            if (_tJatuh > 0f)
            {
                _tJatuh -= Time.deltaTime;
                if (_tJatuh <= 0f) _ctrl.PengaliKecepatan = _pengaliAsal > 0f ? _pengaliAsal : 1f;
            }
            _tLangkah -= Time.deltaTime;
            if (_tLangkah > 0f) return;
            _tLangkah = JedaLangkah;
            float r = MeterSuara.RadiusLangkah(_ctrl.SedangLari, _ctrl.Jongkok, _ctrl.KecepatanDatar);
            RadiusSuara = r;
            if (r > 0f) MeterSuara.Pancar(transform.position, r);
        }
    }
}
