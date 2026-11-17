using System;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.Player
{
    // Vital Andi (05 §3, 24 §1): kesehatan + plafon, stamina + cap, basah & dingin, kenyang (rata-rata 4 gizi), haus, istirahat, kewarasan.
    // Laju per detik nyata (1 s = 1 menit game). Lapisan suhu tubuh & gizi di AndiVitalsAndSanity.Tubuh.cs.
    public partial class AndiVitalsAndSanity : MonoBehaviour, IDamageable, IVitalAndi, ISaveable
    {
        public const float Maks = 100f, HPMinPlafon = 20f, StaminaMinCap = 25f;
        public const float RegenHP = 0.6f, RegenHPTidur = 1.5f, JedaRegen = 6f, RusakLapar = 0.25f, RusakHaus = 0.5f;
        public const float BiayaSprint = 11f, BiayaLompat = 8f, BiayaRenang = 4f, BiayaRenangCepat = 10f;
        public const float PulihStamina = 16f, JedaPulih = 1f, AmbangHabis = 25f;
        public const float LajuHaus = 0.085f, LajuIstirahat = 0.035f, PengaliTidur = 0.5f;
        public const float BasahHujanGerimis = 0.8f, BasahHujanBadai = 2.5f;
        public const float KeringApi = 6f, KeringAtap = 1.5f, KeringCerah = 1f, KeringLain = 0.3f, DinginTurunApi = 8f;
        public const float SanityMalamGelap = 0.05f, SanityKabutDarah = 0.03f, SanityApi = 0.2f, SanityCerah = 0.02f, SanityTidur = 0.5f;
        public const float JarakCahaya = 6f, TinggiAtap = 6f, RespawnDetik = 3f;

        public float Health { get; private set; } = Maks;
        public float Stamina { get; private set; } = Maks;
        public float Hydration { get; private set; } = 85f;
        public float Rest { get; private set; } = 90f;
        public float Sanity { get; private set; } = 85f;
        public float Wetness { get; private set; }
        public float ColdWet { get; private set; }
        public bool Tidur { get; private set; }
        public bool StaminaHabis { get; private set; }
        public float TargetHealthCeiling { get { return PlafonHP(Fullness, Hydration, Rest); } }
        public float MaxStaminaCap { get { return CapStamina(ColdWet, Rest, Sanity, SuhuTubuh); } }
        public bool IsAlive { get { return Health > 0f; } }
        public string SaveKey { get { return "vital"; } }
        public event Action OnMati;

        // Input lingkungan (diset sistem lain; nilai bawaan = alam terbuka).
        bool _dekatApi, _berteduh;
        float _sejakDamage = JedaRegen, _sejakBiaya = JedaPulih, _tMati;
        AndiFPSController _ctrl;
        TanganKiri _kiri;
        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _atmos;

        public void SetDekatApi(bool v) { _dekatApi = v; }
        public void SetBerteduh(bool v) { _berteduh = v; }
        public void SetTidur(bool v) { Tidur = v; }

        public static float PlafonHP(float full, float haus, float rest)
        {
            float p = Maks - Mathf.Max(0f, 25f - full) * 1.2f - Mathf.Max(0f, 25f - haus) * 1.6f - (rest < 10f ? 10f : 0f);
            return Mathf.Max(HPMinPlafon, p);
        }

        public static float CapStamina(float coldWet, float rest, float sanity, float suhu)
        {
            float c = Maks - Mathf.Min(50f, 0.5f * coldWet) - (rest < 20f ? 15f : 0f) - (sanity < 20f ? 10f : 0f) - (suhu < SuhuKedinginan ? 15f : 0f);
            return Mathf.Max(StaminaMinCap, c);
        }

        void Awake()
        {
            GameServices.Register(this);
            _ctrl = GetComponent<AndiFPSController>();
            _kiri = GetComponent<TanganKiri>();
            if (_ctrl != null) _ctrl.OnLompat += BayarLompat;
            AwalGizi();
        }

        void OnDestroy() { if (_ctrl != null) _ctrl.OnLompat -= BayarLompat; }

        void BayarLompat() { Pakai(BiayaLompat); }

        public bool Pakai(float biaya)
        {
            if (Stamina <= 0f) return false;
            Stamina = Mathf.Max(0f, Stamina - biaya);
            _sejakBiaya = 0f;
            if (Stamina <= 0f) StaminaHabis = true;
            return true;
        }

        void Update()
        {
            try
            {
                if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
                if (_atmos == null) _atmos = GameServices.Get<URPAtmosphereAndWeather>();
                if (!IsAlive) { TungguBangkit(Time.deltaTime); return; }
                var k = new Keadaan
                {
                    jam = _jam != null ? _jam.Jam : TimeOfDayClock.JamMulai,
                    hari = _jam != null ? _jam.Hari : 1,
                    cuaca = _atmos != null ? _atmos.Cuaca : WeatherState.Cerah,
                    sprint = _ctrl != null && _ctrl.SedangLari,
                    renang = _ctrl != null && _ctrl.Berenang,
                    renangCepat = _ctrl != null && _ctrl.Berenang && _ctrl.SedangLari,
                    napasHabis = _ctrl != null && _ctrl.Berenang && _ctrl.Napas <= 0f,
                    dekatApi = _dekatApi,
                    berteduh = _berteduh || CekAtap(),
                    cahaya = _dekatApi || (_kiri != null && _kiri.OborMenyala),
                    biome = WorldTerrainGenerator.GetBiome(transform.position.x, transform.position.z)
                };
                Tick(k, Time.deltaTime);
                if (_ctrl != null) _ctrl.LariDiblokir = StaminaHabis || Stamina < 1f;
            }
            catch (Exception e) { Debug.LogError("[AK][AndiVitalsAndSanity] " + e.Message); }
        }

        public struct Keadaan
        {
            public float jam; public int hari; public WeatherState cuaca; public Biome biome;
            public bool sprint, renang, renangCepat, napasHabis, dekatApi, berteduh, cahaya;
        }

        // Satu langkah simulasi; dipakai Update dan Self-Test (simulasi 60 s).
        public void Tick(Keadaan k, float dt)
        {
            bool malam = k.jam >= 19f || k.jam < 5f;
            bool kemarau = URPAtmosphereAndWeather.MusimHari(k.hari) == Season.Kemarau;
            bool hujan = k.cuaca == WeatherState.Gerimis || k.cuaca == WeatherState.HujanBadai;

            // Basah & dingin (05 §3.3)
            if (k.renang) Wetness = Maks;
            else if (hujan && !k.berteduh) Wetness = Mathf.Min(Maks, Wetness + (k.cuaca == WeatherState.HujanBadai ? BasahHujanBadai : BasahHujanGerimis) * dt);
            else
            {
                float kering = k.dekatApi ? KeringApi : k.berteduh ? KeringAtap : (!malam && k.cuaca == WeatherState.Cerah ? KeringCerah : KeringLain);
                Wetness = Mathf.Max(0f, Wetness - kering * dt);
            }
            float faktor = FaktorDingin(k.jam) * (kemarau ? 1f : 1.2f);
            ColdWet = k.dekatApi ? Mathf.Max(0f, ColdWet - DinginTurunApi * dt) : Wetness * faktor;

            // Haus, istirahat, gizi
            float pHaus = (k.sprint ? 1.2f : 1f) * (kemarau && k.jam >= 10f && k.jam < 15f ? 1.4f : 1f) * (Tidur ? PengaliTidur : 1f) * (SuhuTubuh > SuhuKepanasan ? 1.8f : 1f);
            if (k.cuaca == WeatherState.KabutAsap) pHaus *= 1.2f;
            Hydration = Mathf.Max(0f, Hydration - LajuHaus * pHaus * dt);
            Rest = Tidur ? Mathf.Min(Maks, Rest + 1.5f * dt) : Mathf.Max(0f, Rest - LajuIstirahat * dt);
            TickGizi(k.sprint, dt);
            TickSuhu(k, dt);
            TickPenyakit(dt);

            // Stamina (05 §3.2)
            _sejakBiaya += dt;
            if (k.sprint) { Stamina = Mathf.Max(0f, Stamina - BiayaSprint * dt); _sejakBiaya = 0f; }
            if (k.renang) { Stamina = Mathf.Max(0f, Stamina - (k.renangCepat ? BiayaRenangCepat : BiayaRenang) * dt); _sejakBiaya = 0f; }
            if (Stamina <= 0f) StaminaHabis = true;
            if (_sejakBiaya >= JedaPulih)
            {
                float pulih = PulihStamina * (Fullness < 20f || Karbo < 20f ? 0.5f : 1f) * (Sanity < 10f ? 0.7f : 1f) * (SuhuTubuh > SuhuKepanasan ? 0.5f : 1f);
                Stamina = Mathf.Min(MaxStaminaCap, Stamina + pulih * dt);
            }
            if (Stamina > MaxStaminaCap) Stamina = MaxStaminaCap;
            if (StaminaHabis && Stamina >= AmbangHabis) StaminaHabis = false;

            // Kesehatan (05 §3.1)
            _sejakDamage += dt;
            float plafon = TargetHealthCeiling;
            if (Fullness <= 0f) Health -= RusakLapar * dt;
            if (Hydration <= 0f) Health -= RusakHaus * dt;
            if (k.napasHabis) Health -= 8f * dt;
            Health -= RusakSuhu() * dt;
            bool bisaRegen = Fullness > 40f && Hydration > 40f && _sejakDamage >= JedaRegen;
            if (bisaRegen && Health < plafon) Health = Mathf.Min(plafon, Health + (Tidur ? RegenHPTidur : RegenHP) * dt);
            if (Health > plafon && !bisaRegen) Health = Mathf.Max(plafon, Health - RegenHP * dt);

            // Kewarasan (05 §3.5)
            float s = 0f;
            if (malam && !k.cahaya && k.biome != Biome.BawahTanah) s -= SanityMalamGelap;
            if (k.cuaca == WeatherState.KabutDarah) s -= SanityKabutDarah;
            if (k.dekatApi) s += SanityApi;
            if (Tidur) s += SanityTidur;
            if (!malam && k.cuaca == WeatherState.Cerah) s += SanityCerah;
            Sanity = Mathf.Clamp(Sanity + s * dt, 0f, Maks);

            if (Health <= 0f) Mati();
        }

        public static float FaktorDingin(float jam)
        {
            if (jam >= 20f || jam < 5f) return 1f;
            if (jam >= 8f && jam < 17f) return 0.4f;
            if (jam >= 17f) return Mathf.Lerp(0.4f, 1f, (jam - 17f) / 3f);
            return Mathf.Lerp(1f, 0.4f, (jam - 5f) / 3f);
        }

        bool CekAtap()
        {
            return Physics.Raycast(transform.position + Vector3.up * 1.7f, Vector3.up, TinggiAtap, 1 << GameLayers.Structure, QueryTriggerInteraction.Ignore);
        }

        public void TakeDamage(DamageInfo info)
        {
            if (!IsAlive || info.amount <= 0f) return;
            Health = Mathf.Max(0f, Health - info.amount);
            _sejakDamage = 0f;
            GameEvents.RaisePlayerDamaged(info);
            if (Health <= 0f) Mati();
        }

        public void Minum(float hidrasi, bool airMentah)
        {
            Hydration = Mathf.Min(Maks, Hydration + hidrasi);
            if (airMentah) RisikoAirMentah();
        }

        public bool Konsumsi(ItemID id)
        {
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv == null || !inv.Punya(id) || !ItemDatabase.IsFood(id)) return false;
            inv.Kurangi(id, 1);
            TerapkanMakanan(id);
            return true;
        }

        public void TerapkanMakanan(ItemID id)
        {
            var d = ItemDatabase.Get(id);
            Hydration = Mathf.Clamp(Hydration + d.hydration, 0f, Maks);
            Health = Mathf.Clamp(Health + d.health, 0f, Maks);
            Stamina = Mathf.Clamp(Stamina + d.stamina, 0f, Maks);
            Sanity = Mathf.Clamp(Sanity + d.sanity, 0f, Maks);
            TambahGizi(id, d.fullness);
            if (id == ItemID.BotolAirBersih || id == ItemID.RansumDarurat) { var inv = GameServices.Get<TikarPandan3DInventory>(); if (inv != null) inv.Tambah(ItemID.BotolKosong, 1); }
            if (id == ItemID.DagingMentah || id == ItemID.IkanMentah || id == ItemID.DagingBusuk) RisikoMentah(id == ItemID.DagingBusuk ? 0.7f : 0.3f);
        }

        void Mati()
        {
            if (_tMati > 0f) return;
            Health = 0f; _tMati = RespawnDetik;
            GameEvents.RaiseToast("Andi pingsan...");
            var h = OnMati; if (h != null) h();
        }

        // Sementara sampai sistem simpan T6: bangun lagi di titik awal dengan vital setengah.
        void TungguBangkit(float dt)
        {
            _tMati -= dt;
            if (_tMati > 0f) return;
            _tMati = 0f;
            Health = Maks * 0.5f; Stamina = Maks * 0.5f; Hydration = Mathf.Max(Hydration, 40f); Sanity = Mathf.Max(Sanity, 40f);
            SetGiziMinimal(40f); SuhuTubuh = SuhuNormal; Wetness = 0f;
            var cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            transform.position = new Vector3(-4f, Mathf.Max(0f, WorldTerrainGenerator.SampleHeight(-4f, -424f)) + 0.3f, -424f);
            if (cc != null) cc.enabled = true;
            GameEvents.RaiseToast("Kamu tersadar di dekat heli");
        }

        [Serializable] class DataVital { public float hp, st, haus, rest, san, basah, suhu, karbo, protein, lemak, vitamin; }

        public string CaptureJson()
        {
            return JsonUtility.ToJson(new DataVital { hp = Health, st = Stamina, haus = Hydration, rest = Rest, san = Sanity, basah = Wetness, suhu = SuhuTubuh, karbo = Karbo, protein = Protein, lemak = Lemak, vitamin = Vitamin });
        }

        public void RestoreJson(string json)
        {
            try
            {
                var d = JsonUtility.FromJson<DataVital>(json);
                if (d == null) return;
                Health = d.hp; Stamina = d.st; Hydration = d.haus; Rest = d.rest; Sanity = d.san; Wetness = d.basah; SuhuTubuh = d.suhu;
                Karbo = d.karbo; Protein = d.protein; Lemak = d.lemak; Vitamin = d.vitamin;
            }
            catch (Exception e) { Debug.LogError("[AK][AndiVitalsAndSanity] Data simpan rusak: " + e.Message); }
        }
    }
}
