using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Player
{
    // 24 §1: suhu tubuh, gizi 4 cadangan (HUD Kenyang = rata-rata), air mentah -> diare, daging mentah -> keracunan.
    public partial class AndiVitalsAndSanity
    {
        public const float SuhuNormal = 36.8f, SuhuMin = 34f, SuhuMaks = 41f;
        public const float SuhuKedinginan = 36f, SuhuHipotermia = 35f, SuhuKepanasan = 38f, SuhuHeatstroke = 39.5f;
        public const float LajuSuhu = 0.012f, SprintPanas = 0.02f, ApiHangat = 1.4f, RusakHipotermia = 0.2f, RusakHeatstroke = 0.3f;
        public const float LajuGizi = 0.055f, GiziAwal = 70f;
        public const float PeluangDiare = 0.35f, LamaDiare = 180f, HausDiare = 0.15f, LamaKeracunan = 60f, RusakKeracunan = 0.2f, RusakBisaUlar = 0.9f;

        public float SuhuTubuh { get; private set; } = SuhuNormal;
        public float SuhuUdaraSaatIni { get; private set; } = 26f;
        public float Karbo { get; private set; } = GiziAwal;
        public float Protein { get; private set; } = GiziAwal;
        public float Lemak { get; private set; } = GiziAwal;
        public float Vitamin { get; private set; } = GiziAwal;
        public float Fullness { get { return (Karbo + Protein + Lemak + Vitamin) * 0.25f; } }
        public bool Diare { get { return _diare > 0f; } }
        public float LukaKepala { get; private set; }
        public float LukaLengan { get; private set; }
        public float LukaKaki { get; private set; }
        public StatusEffect Status { get; private set; }
        public static bool ModeSengsara, ModePantangMundur;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetModeTantangan() { ModeSengsara = false; ModePantangMundur = false; }
        public static float PengaliModeSengsara(bool aktif) { return aktif ? 1.45f : 1f; }

        float _diare, _racun, _bisa;

        public static float PengaliKecepatanLuka(float lukaKaki) { return lukaKaki > 0f ? 0.78f : 1f; }
        public static float PengaliBiayaStaminaLuka(float lukaLengan) { return lukaLengan > 0f ? 1.25f : 1f; }

        public void ObatiLukaTubuh()
        {
            if (LukaKepala > 0f || LukaLengan > 0f || LukaKaki > 0f)
                GameEvents.RaiseToast("Luka pada tubuhmu mulai pulih");
            LukaKepala = 0f; LukaLengan = 0f; LukaKaki = 0f;
        }

        void TerapkanLukaTubuh(DamageInfo info, float sisaDamage)
        {
            if (info.type == DamageType.Jatuh && sisaDamage >= 8f)
            {
                LukaKaki = Mathf.Max(LukaKaki, 90f);
                GameEvents.RaiseToast("Pergelangan kakimu terkilir! Gerakan melambat sampai diobati.");
                return;
            }
            if (sisaDamage < 14f) return;
            if (info.isHeavy)
            {
                if (Random.value < 0.45f) { LukaKepala = Mathf.Max(LukaKepala, 60f); GameEvents.RaiseToast("Pandanganmu berkunang-kunang karena benturan kepala!"); }
                else { LukaLengan = Mathf.Max(LukaLengan, 75f); GameEvents.RaiseToast("Lenganmu terluka! Konsumsi stamina meningkat."); }
            }
            else if (Random.value < 0.35f)
            {
                LukaKaki = Mathf.Max(LukaKaki, 65f);
                GameEvents.RaiseToast("Kakimu terluka! Balut dengan Perban atau Jamu.");
            }
        }

        void AwalGizi() { Karbo = Protein = Lemak = Vitamin = GiziAwal; }
        void SetGiziMinimal(float v) { Karbo = Mathf.Max(Karbo, v); Protein = Mathf.Max(Protein, v); Lemak = Mathf.Max(Lemak, v); Vitamin = Mathf.Max(Vitamin, v); }
        void BersihkanPenyakit() { _diare = 0f; _racun = 0f; _bisa = 0f; LukaKepala = 0f; LukaLengan = 0f; LukaKaki = 0f; SetStatus(StatusEffect.Keracunan, false); SetStatus(StatusEffect.Racun, false); }

        void TickGizi(bool sprint, float dt)
        {
            float d = LajuGizi * (sprint ? 1.3f : 1f) * (Tidur ? PengaliTidur : 1f) * dt;
            Karbo = Mathf.Max(0f, Karbo - d); Protein = Mathf.Max(0f, Protein - d);
            Lemak = Mathf.Max(0f, Lemak - d); Vitamin = Mathf.Max(0f, Vitamin - d);
        }

        // Pembagian nilai kenyang item ke 4 cadangan (karbo, protein, lemak, vitamin), total = 4 x fullness agar rata-rata naik sebesar fullness.
        public static Vector4 ProfilGizi(ItemID id)
        {
            switch (id)
            {
                case ItemID.Pisang: case ItemID.MakananGosong: return new Vector4(0.7f, 0.05f, 0.05f, 0.2f);
                case ItemID.KelapaMuda: return new Vector4(0.5f, 0.05f, 0.4f, 0.05f);
                case ItemID.Jamur: case ItemID.JamurBeracun: case ItemID.DaunSirih: case ItemID.KunyitObat: case ItemID.JaheHutan: return new Vector4(0.1f, 0.2f, 0f, 0.7f);
                case ItemID.IkanMentah: case ItemID.IkanMatang: case ItemID.IkanKering: return new Vector4(0f, 0.6f, 0.3f, 0.1f);
                case ItemID.DagingMentah: case ItemID.DagingMatang: case ItemID.DagingKering: case ItemID.DagingBusuk: return new Vector4(0f, 0.6f, 0.4f, 0f);
                case ItemID.TelurAyamHutan: return new Vector4(0f, 0.6f, 0.35f, 0.05f);
                case ItemID.Kepiting: return new Vector4(0f, 0.75f, 0.15f, 0.1f);
                case ItemID.RansumDarurat: return new Vector4(0.45f, 0.25f, 0.2f, 0.1f);
                default: return new Vector4(0.25f, 0.25f, 0.25f, 0.25f);
            }
        }

        void TambahGizi(ItemID id, float fullness)
        {
            if (fullness <= 0f) { ObatiDariMakanan(id); return; }
            Vector4 p = ProfilGizi(id) * (fullness * 4f);
            Karbo = Mathf.Min(Maks, Karbo + p.x); Protein = Mathf.Min(Maks, Protein + p.y);
            Lemak = Mathf.Min(Maks, Lemak + p.z); Vitamin = Mathf.Min(Maks, Vitamin + p.w);
            ObatiDariMakanan(id);
        }

        void ObatiDariMakanan(ItemID id)
        {
            if (id == ItemID.PerbanKain) ObatiLukaTubuh();
            if (id == ItemID.KunyitObat || id == ItemID.JamuHerbal || id == ItemID.DaunSirih)
            {
                if (_diare > 0f || _racun > 0f || _bisa > 0f) GameEvents.RaiseToast("Ramuan meredakan racun dan sakit perutmu");
                if (id != ItemID.DaunSirih) { _diare = 0f; _racun = 0f; SetStatus(StatusEffect.Keracunan, false); }
                _bisa = 0f; SetStatus(StatusEffect.Racun, false);
                ObatiLukaTubuh();
            }
        }

        // Suhu udara (24 §1.1): pagi 22, siang 31-35 kemarau (29 penghujan), malam 19-24, gua 17, -0,6 C per 100 m.
        public static float SuhuUdara(float jam, bool kemarau, Biome biome, float tinggi, WeatherState cuaca)
        {
            if (biome == Biome.BawahTanah) return 17f;
            float siang = kemarau ? 33f : 29f, malam = kemarau ? 21f : 22f;
            float t;
            if (jam >= 6f && jam < 14f) t = Mathf.Lerp(22f, siang, (jam - 6f) / 8f);
            else if (jam >= 14f && jam < 20f) t = Mathf.Lerp(siang, malam, (jam - 14f) / 6f);
            else if (jam >= 20f) t = malam;
            else t = Mathf.Lerp(malam, 19.5f, jam / 6f);
            if (cuaca == WeatherState.HujanBadai) t -= 4f; else if (cuaca == WeatherState.Gerimis) t -= 2f; else if (cuaca == WeatherState.Berawan) t -= 1f;
            return t - Mathf.Max(0f, tinggi) * 0.006f;
        }

        void TickSuhu(Keadaan k, float dt)
        {
            bool kemarau = URPAtmosphereAndWeather.MusimHari(k.hari) == Season.Kemarau;
            SuhuUdaraSaatIni = SuhuUdara(k.jam, kemarau, k.biome, transform.position.y, k.cuaca);
            float lemakDingin = Lemak < 20f ? 1.4f : 1f;
            float target = SuhuNormal + (SuhuUdaraSaatIni - 26f) * 0.09f - Wetness * 0.018f * lemakDingin * (k.cuaca == WeatherState.HujanBadai ? 1.3f : 1f);
            if (k.dekatApi) target += ApiHangat;
            if (k.renang) target -= 0.8f;
            SuhuTubuh = Mathf.MoveTowards(SuhuTubuh, target, LajuSuhu * dt);
            if (k.sprint) SuhuTubuh += SprintPanas * dt;
            SuhuTubuh = Mathf.Clamp(SuhuTubuh, SuhuMin, SuhuMaks);
        }

        float RusakSuhu()
        {
            float r = 0f;
            if (SuhuTubuh < SuhuHipotermia) r += RusakHipotermia;
            if (SuhuTubuh > SuhuHeatstroke) r += RusakHeatstroke;
            if (_racun > 0f) r += RusakKeracunan;
            if (_bisa > 0f) r += RusakBisaUlar;
            return r;
        }

        void TickPenyakit(float dt)
        {
            if (_diare > 0f) { _diare -= dt; Hydration = Mathf.Max(0f, Hydration - HausDiare * dt); if (_diare <= 0f) GameEvents.RaiseToast("Diaremu reda"); }
            if (_racun > 0f) { _racun -= dt; if (_racun <= 0f) SetStatus(StatusEffect.Keracunan, false); }
            if (_bisa > 0f) { _bisa -= dt; if (_bisa <= 0f) { SetStatus(StatusEffect.Racun, false); GameEvents.RaiseToast("Bisa ular mulai mereda"); } }
            if (LukaKepala > 0f) LukaKepala = Mathf.Max(0f, LukaKepala - dt);
            if (LukaLengan > 0f) LukaLengan = Mathf.Max(0f, LukaLengan - dt);
            if (LukaKaki > 0f) LukaKaki = Mathf.Max(0f, LukaKaki - dt);
        }

        public void KenaBisaUlar(float lama = 20f)
        {
            _bisa = Mathf.Max(_bisa, lama);
            SetStatus(StatusEffect.Racun, true);
            GameEvents.RaiseToast("Terpatuk Ular Kobra! Obati dengan Jamu Herbal, Kunyit, atau Daun Sirih!");
        }

        void RisikoAirMentah()
        {
            if (Random.value < PeluangDiare && _diare <= 0f) { _diare = LamaDiare; GameEvents.RaiseToast("Perutmu melilit. Air sungai mentah bikin diare"); }
        }

        void RisikoMentah(float peluang)
        {
            if (Random.value < peluang) { _racun = LamaKeracunan; SetStatus(StatusEffect.Keracunan, true); GameEvents.RaiseToast("Kamu keracunan makanan mentah"); }
        }

        void SetStatus(StatusEffect s, bool aktif)
        {
            var baru = aktif ? (Status | s) : (Status & ~s);
            if (baru == Status) return;
            Status = baru;
            GameEvents.RaiseStatusChanged(Status);
        }
    }
}
