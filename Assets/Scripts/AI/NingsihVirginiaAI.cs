using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.AI
{
    // 08 §3 (T8): Ningsih - HP 110, Trust 3 tahap (mengintip -> mendekat -> sahabat), kabur bila diburu/dibidik, beri makanan, tari pagi,
    // hadiah harian, menu interaksi (busur/suar/pistol, ikuti, tunggu, pulang), tempur jarak jauh (busur 30 / 1,4 s, suar 40 / 6 s), indra keenam.
    public partial class NingsihVirginiaAI : CompanionDasar, IInteractable, ISaveable
    {
        public enum SikapNingsih { DiSarang, Mengintip, Kabur, Kemah, Ikuti, Tunggu }
        public const float HPNingsih = 110f, Jalan = 1.7f, Lari = 7.5f, JedaBusur = 1.4f, JedaSuar = 6f, DamageBusur = 30f, DamageSuar = 40f;
        public const float JarakIntipMin = 30f, JarakIntipMaks = 45f, LamaIntip1 = 60f, LamaIntip2 = 120f, JedaMakan = 120f, SanityTari = 0.3f, JedaIndra = 20f;
        public static readonly Vector3 Sarang = new Vector3(200f, 0f, -350f);

        public override float HPMaks { get { return HPNingsih; } }
        protected override float SisiIkuti { get { return 1f; } }
        public override string NamaTampil { get { return "Ningsih"; } }
        public float HoldDuration { get { return 0f; } }
        public string SaveKey { get { return "ningsih"; } }

        public float Trust { get; private set; }
        public SikapNingsih Sikap { get; private set; } = SikapNingsih.DiSarang;
        public bool PunyaBusur { get; private set; }
        public bool PunyaSuar { get; private set; }
        public bool PunyaPistol { get; private set; }
        public bool HadiahSiap { get { return _hadiahHari == HariIni() && !_hadiahDiterima; } }

        CombatAndLeftHandController _combat;
        AndiVitalsAndSanity _vital;
        float _tMuncul = 90f, _didapat, _tKabur, _tMakan = -999f, _tBusur, _tSuar, _tLompat, _tIndra;
        int _hadiahHari = -1, _hariKabur = -1, _hariBertemu = -1;
        bool _hadiahDiterima, _bertemuHariIni;
        Vector3 _titikTunggu;

        protected override void Awake()
        {
            base.Awake();
            GameServices.Register(this);
        }

        void OnEnable() { GameEvents.OnEnemyKilled += MusuhMati; GameEvents.OnDayChanged += HariBaru; NusantaraEnemyAndMutantAI.OnMenyerahDiserang += MenyerahDiserang; }
        void OnDisable() { GameEvents.OnEnemyKilled -= MusuhMati; GameEvents.OnDayChanged -= HariBaru; NusantaraEnemyAndMutantAI.OnMenyerahDiserang -= MenyerahDiserang; }

        int HariIni() { return _jam != null ? _jam.Hari : 1; }
        bool Dekat(float r) { return _andi != null && Datar(_andi.transform.position - transform.position).magnitude <= r; }
        bool Tampak { get { return Sikap != SikapNingsih.DiSarang; } }

        public void UbahTrust(KejadianTrust k) { Trust = Terapkan(Trust, k); }

        void MusuhMati(EnemyArchetype j, bool diampuni) { if (!diampuni && StatMusuh.Sekte(j) && Tampak && Dekat(30f)) UbahTrust(KejadianTrust.BunuhSekteDiDepan); }
        void MenyerahDiserang() { if (Tampak && Dekat(30f)) UbahTrust(KejadianTrust.SerangYangMenyerah); }
        void HariBaru(int h) { if (!_bertemuHariIni && h > 1) UbahTrust(KejadianTrust.TakBertemuSehari); _bertemuHariIni = false; }

        void Update()
        {
            try { Tick(Time.deltaTime); }
            catch (System.Exception e) { Debug.LogError("[AK][Ningsih] galat: " + e.Message); enabled = false; }
        }

        void Tick(float dt)
        {
            AmbilLayanan();
            if (_combat == null) _combat = GameServices.Get<CombatAndLeftHandController>();
            if (_vital == null) _vital = GameServices.Get<AndiVitalsAndSanity>();
            if (Gugur || _andi == null) return;
            if (Kritis) { TickKritis(dt); return; }
            _tBusur -= dt; _tSuar -= dt; _tLompat -= dt; _tIndra -= dt;
            if (Tampak && Dekat(40f)) { _bertemuHariIni = true; _hariBertemu = HariIni(); }
            IndraKeenam();
            int tahap = Tahap(Trust);
            float jam = _jam != null ? _jam.Jam : 12f;
            bool siang = jam >= 6f && jam < 17.5f;
            if (_hariKabur >= 0 && HariIni() <= _hariKabur) { KeSarang(dt); return; }
            if (tahap == 3 && Sikap != SikapNingsih.Ikuti && Sikap != SikapNingsih.Tunggu) Sikap = SikapNingsih.Kemah;
            if (Sikap == SikapNingsih.Kabur) { TickKabur(dt); return; }
            if (TickTempur(dt, tahap)) return;
            switch (Sikap)
            {
                case SikapNingsih.DiSarang:
                    KeSarang(dt);
                    if (siang) { _tMuncul -= dt; if (_tMuncul <= 0f) Muncul(tahap); }
                    break;
                case SikapNingsih.Mengintip: TickIntip(dt, tahap, siang); break;
                case SikapNingsih.Kemah: TickKemah(dt, jam); break;
                case SikapNingsih.Ikuti:
                    Vector3 t = TitikIkuti(_andi.transform.position, _andi.transform.forward, SisiIkuti);
                    float d = Datar(t - transform.position).magnitude;
                    if (d > 1.2f) Menuju(t, d > 10f ? Lari : Jalan * 2f, dt); else { Diam(dt); Hadap(_andi.transform.position - transform.position, dt); }
                    CekTeleport(dt);
                    break;
                case SikapNingsih.Tunggu:
                    if (Datar(_titikTunggu - transform.position).magnitude > 1f) Menuju(_titikTunggu, Jalan, dt); else Diam(dt);
                    break;
            }
            Pulih(0.5f, dt);
        }

        void KeSarang(float dt)
        {
            Vector3 s = new Vector3(Sarang.x, WorldTerrainGenerator.SampleHeight(Sarang.x, Sarang.z), Sarang.z);
            if (Datar(s - transform.position).magnitude < 2f) { Diam(dt); if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Intai; return; }
            if (!TerlihatKamera()) { Teleport(s); return; }
            Menuju(s, Lari, dt);
        }

        // Muncul 30-45 m dari Andi di luar pandangan kamera (tahap 1 tiap 4-7 menit, tahap 2 tiap 2-4 menit).
        void Muncul(int tahap)
        {
            _tMuncul = tahap == 1 ? Random.Range(240f, 420f) : Random.Range(120f, 240f);
            var cam = _andi.Kamera;
            for (int k = 0; k < 10; k++)
            {
                float s = Random.value * Mathf.PI * 2f, r = Random.Range(JarakIntipMin, JarakIntipMaks);
                Vector3 p = _andi.transform.position + new Vector3(Mathf.Cos(s), 0f, Mathf.Sin(s)) * r;
                p.y = WorldTerrainGenerator.SampleHeight(p.x, p.z);
                if (p.y < 0.3f || WorldTerrainGenerator.IsUnderwater(p + Vector3.up)) continue;
                if (cam != null && !BolehTeleport(cam.position, cam.forward, p + Vector3.up)) continue;
                Teleport(p);
                Sikap = SikapNingsih.Mengintip; _didapat = 0f; _tKabur = 0f;
                if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Intai;
                return;
            }
        }

        void TickIntip(float dt, int tahap, bool siang)
        {
            Vector3 keAndi = _andi.transform.position - transform.position;
            float d = Datar(keAndi).magnitude;
            Hadap(keAndi, dt);
            _tKabur += dt;
            var cam = _andi.Kamera;
            float tatap = cam != null ? Vector3.Angle(cam.forward, transform.position + Vector3.up * 1.3f - cam.position) : 180f;
            bool tersimpan = _combat == null || _combat.Tersimpan;
            bool bidik = _combat != null && !_combat.Tersimpan && _combat.PakaiJarak && tatap < 10f;
            bool memburu = _andi.SedangLari && d < 25f && Vector3.Dot(_andi.transform.forward, -Datar(keAndi).normalized) > 0.7f;
            if (bidik) Trust = Mathf.Max(0f, Trust - TurunBidik * dt);
            if (memburu) Trust = Mathf.Max(0f, Trust - TurunLari * dt);
            if (bidik || memburu || (tahap == 1 && d < 12f)) { MulaiKabur(); return; }
            bool tenang = AndiTenang(tersimpan, _andi.Jongkok, _andi.KecepatanDatar, tatap);
            if (tahap == 1)
            {
                Diam(dt);
                if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Intai;
                if (tenang && d <= 40f) Trust = NaikPerjumpaan(Trust, _didapat, NaikTahap1, MaksPerjumpaan1, dt, out _didapat);
            }
            else
            {
                Vector3 api = ApiDekatAndi();
                Vector3 tuju = api != Vector3.zero ? api + Datar(transform.position - api).normalized * 2.5f : _andi.transform.position - Datar(keAndi).normalized * -4f;
                if (tenang || api != Vector3.zero)
                {
                    if (Datar(tuju - transform.position).magnitude > 1f) { if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak; Menuju(tuju, Jalan, dt); }
                    else { Diam(dt); if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Intai; }
                    if (tenang) Trust = NaikPerjumpaan(Trust, _didapat, NaikTahap2, MaksPerjumpaan2, dt, out _didapat);
                }
                else Diam(dt);
            }
            if (!siang || _tKabur > (tahap == 1 ? LamaIntip1 : LamaIntip2)) { Sikap = SikapNingsih.DiSarang; if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak; }
        }

        Vector3 ApiDekatAndi()
        {
            foreach (var a in AlasKeramat.Building.ApiUnggun.Semua)
                if (a != null && a.Menyala && Datar(a.transform.position - _andi.transform.position).magnitude <= 10f) return a.transform.position;
            return Vector3.zero;
        }

        void MulaiKabur() { Sikap = SikapNingsih.Kabur; _tKabur = 0f; if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak; }

        void TickKabur(float dt)
        {
            _tKabur += dt;
            Vector3 jauh = transform.position + Datar(transform.position - _andi.transform.position).normalized * 30f;
            Gerak(jauh, Lari, dt);
            if (_tKabur > 8f && !TerlihatKamera()) { Sikap = Tahap(Trust) == 3 ? SikapNingsih.Kemah : SikapNingsih.DiSarang; Teleport(new Vector3(Sarang.x, WorldTerrainGenerator.SampleHeight(Sarang.x, Sarang.z), Sarang.z)); }
        }

        // Tahap 3: tinggal di kemah siang hari, tari pagi 06:00-08:00, hadiah harian 06:00-10:00, malam timpuh di api.
        void TickKemah(float dt, float jam)
        {
            Vector3? kemah = Kemah();
            Vector3 pusat = kemah.HasValue ? kemah.Value : Sarang;
            if (jam >= 6f && jam < 10f && _hadiahHari != HariIni()) { _hadiahHari = HariIni(); _hadiahDiterima = false; }
            if (HadiahSiap && Datar(_andi.transform.position - pusat).magnitude < 40f)
            {
                if (!Dekat(2.5f)) { Menuju(_andi.transform.position, Jalan * 2f, dt); return; }
                Diam(dt); Hadap(_andi.transform.position - transform.position, dt); Klip("Melambai"); return;
            }
            Vector3 duduk = pusat + new Vector3(1.8f, 0f, 1.2f);
            if (Datar(duduk - transform.position).magnitude > 1.2f) { if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak; Menuju(duduk, Jalan, dt); return; }
            Diam(dt);
            bool tari = jam >= 6f && jam < 8f;
            if (_anim != null) _anim.Pose = tari ? ProceduralAnimator.PoseTempur.Tari : ProceduralAnimator.PoseTempur.Intai;
            if (tari && Dekat(10f) && _vital != null) _vital.UbahSanity(SanityTari * dt);
        }

        // ---------- Tempur jarak jauh ----------
        bool TickTempur(float dt, int tahap)
        {
            if (tahap < 3 || Sikap == SikapNingsih.DiSarang) return false;
            Transform sasaran = null; bool hantu = false; float terbaik = 30f;
            foreach (var h in HantuAI.Semua) { if (!h.IsAlive) continue; float d = Datar(h.transform.position - transform.position).magnitude; if (d < terbaik) { terbaik = d; sasaran = h.transform; hantu = true; } }
            if (sasaran == null)
                foreach (var m in NusantaraEnemyAndMutantAI.Semua)
                {
                    if (!m.IsAlive || m.Keadaan == NusantaraEnemyAndMutantAI.KeadaanSekte.Menyerah || m.Kesadaran < NusantaraEnemyAndMutantAI.AmbangWaspada) continue;
                    float d = Datar(m.transform.position - transform.position).magnitude; if (d < terbaik) { terbaik = d; sasaran = m.transform; }
                }
            if (sasaran == null) return false;
            Vector3 ke = sasaran.position - transform.position;
            if (!PunyaBusur && !PunyaSuar && !PunyaPistol) { Menuju(_andi.transform.position - Datar(ke).normalized * 2f, Lari, dt); return true; } // tanpa senjata: berlindung ke Andi
            Hadap(ke, dt);
            if (terbaik <= 3f && _tLompat <= 0f) { _tLompat = 2f; GerakMentah(Vector3.Cross(Vector3.up, Datar(ke).normalized) * (Random.value < 0.5f ? -10f : 10f), 0.2f); return true; } // lompat samping
            Vector3 asal = transform.position + Vector3.up * 1.4f + transform.forward * 0.4f;
            Vector3 bidik = sasaran.position + Vector3.up * 1.1f - asal;
            if (PunyaSuar && terbaik <= 25f && _tSuar <= 0f && (hantu || !PunyaBusur))
            {
                _tSuar = JedaSuar;
                ProyektilAK.Luncurkan(_combat != null ? _combat.PrefabProyektil(3) : null, asal, bidik.normalized * 45f + Vector3.up * 1f, 0.6f, DamageSuar, ItemID.PelurSuar, gameObject);
                return true;
            }
            if ((PunyaBusur || PunyaPistol) && terbaik > 6f && _tBusur <= 0f)
            {
                _tBusur = JedaBusur;
                float meleset = Mathf.Lerp(0f, 3f, (terbaik - 6f) / 30f) * (Random.value < 0.8f ? 0.2f : 1f); // akurasi 80% di 20 m
                Vector3 arah = Quaternion.Euler(Random.Range(-meleset, meleset), Random.Range(-meleset, meleset), 0f) * bidik.normalized;
                ProyektilAK.Luncurkan(_combat != null ? _combat.PrefabProyektil(0) : null, asal, arah * 45f + Vector3.up * (terbaik * 0.11f), 1f, PunyaPistol ? 35f : DamageBusur, ItemID.AnakPanah, gameObject);
                if (_audio != null) _audio.Play(ProceduralAudioSynth.KlipLepasPanah, transform.position, AudioCategory.Efek, 0.8f, 1.05f, 2);
                return true;
            }
            Diam(dt);
            return true;
        }

        void IndraKeenam()
        {
            if (_tIndra > 0f || Sikap == SikapNingsih.DiSarang || _andi.Kamera == null) return;
            foreach (var h in HantuAI.Semua)
            {
                if (!h.IsAlive || Datar(h.transform.position - transform.position).magnitude > 40f) continue;
                bool terlihatAndi = Vector3.Angle(_andi.Kamera.forward, h.transform.position - _andi.Kamera.position) < 35f;
                if (terlihatAndi) continue;
                _tIndra = JedaIndra;
                Hadap(h.transform.position - transform.position, 1f);
                Klip("Menunjuk");
                GameEvents.RaiseSubtitle("(Ningsih menegang, lalu menunjuk ke kegelapan.)");
                return;
            }
        }

        public override void TakeDamage(DamageInfo info)
        {
            if (info.source != null && _andi != null && info.source == _andi.gameObject)
            {
                UbahTrust(KejadianTrust.DipukulAndi);
                _hariKabur = HariIni() + 1;
                MulaiKabur();
                GameEvents.RaiseSubtitle("(Ningsih menatapmu dengan mata terluka, lalu menghilang ke hutan.)");
                return;
            }
            base.TakeDamage(info);
        }
    }
}
