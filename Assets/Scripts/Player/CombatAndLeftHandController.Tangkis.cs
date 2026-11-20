using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Player
{
    // T7a (06 §1 & §3): TANGKIS = ketuk parry (0,25 s; Keris 0,35 s), tahan >= 0,4 s = blok (melee -60%, stamina 1,5 x damage),
    // ketuk ganda = simpan/keluarkan senjata. Parry berhasil membuka Tebasan Berat Penghabisan 2 s (x2,5).
    // Umpan balik: hit-stop 0,05 s (berat), guncang kamera, tanda kena di crosshair, aim assist melee 3 m / 25 derajat dengan bantuan lembut yang tidak melawan geseran jari.
    public partial class CombatAndLeftHandController
    {
        public const float JendelaParry = 0.25f, JendelaParryKeris = 0.35f, AmbangBlok = 0.4f, KetukGanda = 0.3f;
        public const float ReduksiBlok = 0.6f, StaminaBlokPerDamage = 1.5f, KerucutTangkis = 70f;
        public const float LamaPenghabisan = 2f, PengaliPenghabisan = 2.5f;
        public const float HitStop = 0.05f, SkalaHitStop = 0.05f, GuncangBerat = 0.15f, GuncangRingan = 0.06f, LamaTandaKena = 0.15f;
        public const float AssistJarak = 3.6f, AssistKerucut = 38f, AssistLaju = 140f;

        public static event System.Action<bool> OnKena; // true = berat/penghabisan

        float _tParry = -1f, _tTahan = -1f, _tPenghabisan, _tKetukLalu = -10f, _darahSenjata;
        bool _blok, _tersimpan, _pukulanPenghabisan;
        MaterialPropertyBlock _mpbDarah;
        Renderer[] _rendererDarah;
        float _tVisualDarah;
        public float DarahSenjata { get { return _darahSenjata; } }

        public bool Blok { get { return _blok; } }
        public bool ParryAktif { get { return _tParry > 0f; } }
        public bool Tersimpan { get { return _tersimpan; } }
        public bool JendelaPenghabisan { get { return _tPenghabisan > 0f; } }
        public bool SedangPenghabisan { get { return _pukulanPenghabisan; } }

        public static float Jendela(ItemID senjata) { return senjata == ItemID.KerisPusaka ? JendelaParryKeris : JendelaParry; }

        // Hasil tangkisan murni (untuk self-test): 0 = kena penuh, 1 = parry, 2 = blok.
        public static int NilaiTangkis(bool parryAktif, bool blok, float sudutPenyerang, bool bisaDiparry)
        {
            if (sudutPenyerang > KerucutTangkis * 0.5f) return 0;
            if (parryAktif && bisaDiparry) return 1;
            return blok ? 2 : 0;
        }

        public void TangkisTekan()
        {
            if(AlasKeramat.UI.PengunciInput.Terblokir)return;
            if (Time.unscaledTime - _tKetukLalu < KetukGanda) { ToggleSimpan(); _tKetukLalu = -10f; _tTahan = -1f; return; }
            _tKetukLalu = Time.unscaledTime;
            if (_tersimpan) return;
            _tTahan = 0f;
            _tParry = Jendela(_aktif.id);
        }

        public void TangkisLepas() { _tTahan = -1f; _blok = false; }

        public void ToggleSimpan()
        {
            _tersimpan = !_tersimpan;
            _blok = false; _tParry = -1f;
            if (pivotFP != null) pivotFP.gameObject.SetActive(!_tersimpan);
            GameEvents.RaiseToast(_tersimpan ? "Senjata disimpan" : "Senjata dikeluarkan");
        }

        // Dipanggil musuh tepat saat serangannya mengenai Andi. Mengembalikan damage akhir; parry = 0.
        public float TerimaSerangan(Vector3 posPenyerang, float damage, bool bisaDiparry, out bool diparry)
        {
            diparry = false;
            if (Kebal) return 0f; // Elak: kebal 0,25 s (17 §2.2)
            Vector3 ke = posPenyerang - transform.position; ke.y = 0f;
            float sudut = ke.sqrMagnitude < 0.0001f ? 0f : Vector3.Angle(transform.forward, ke);
            int hasil = NilaiTangkis(ParryAktif, _blok, sudut, bisaDiparry && !_tersimpan);
            if (hasil == 1)
            {
                diparry = true; _tParry = -1f; _tPenghabisan = LamaPenghabisan;
                Guncang(GuncangRingan); MulaiHitStop();
                var a = GameServices.Get<ProceduralAudioSynth>();
                if (a != null) a.Play(ProceduralAudioSynth.KlipParry, posPenyerang, AudioCategory.Efek, 1f, Random.Range(0.95f, 1.08f), 3);
                return 0f;
            }
            if (hasil == 2)
            {
                if (_v == null) _v = GameServices.Get<AndiVitalsAndSanity>();
                float biaya = damage * StaminaBlokPerDamage;
                if (_v != null && !_v.Pakai(biaya)) { _blok = false; GameEvents.RaiseToast("Tangkisan jebol, stamina habis"); return damage; }
                Guncang(GuncangRingan);
                return damage * (1f - ReduksiBlok);
            }
            return damage;
        }

        void TickTangkis(float dt)
        {
            if (_darahSenjata > 0f)
            {
                bool cuci = _ctrl != null && _ctrl.Berenang;
                _darahSenjata = cuci ? 0f : Mathf.Max(0f, _darahSenjata - dt / 30f);
                _tVisualDarah -= dt;
                if (_tVisualDarah <= 0f || _darahSenjata <= 0f) { _tVisualDarah = 0.1f; TerapkanDarahSenjata(); }
            }
            if (_tParry > 0f) _tParry -= dt;
            if (_tPenghabisan > 0f) _tPenghabisan -= dt;
            if (_tTahan >= 0f)
            {
                _tTahan += dt;
                if (!_blok && _tTahan >= AmbangBlok) _blok = true;
            }
        }

        public void Guncang(float lama) { if (_ctrl != null) _ctrl.GuncangKamera(lama); }

        void MulaiHitStop() { JedaDunia.HitStop(HitStop); }

        // Aim assist melee selama ancang-ancang: putar yaw menuju musuh terdekat dalam 3 m & kerucut 25 derajat.
        readonly Collider[] _sasaranAssist = new Collider[64];
        void Assist(float dt)
        {
            if (_ctrl == null || !AimAssist || _ctrl.SedangMembidikManual) return;
            int jumlah=Physics.OverlapSphereNonAlloc(transform.position,AssistJarak,_sasaranAssist,1 << GameLayers.Enemy,QueryTriggerInteraction.Ignore);
            var cols=_sasaranAssist;
            float terbaik = 999f, yaw = 0f;
            for (int i = 0; i < jumlah; i++)
            {
                Vector3 ke = cols[i].bounds.center - transform.position; ke.y = 0f;
                if (ke.sqrMagnitude < 0.01f) continue;
                float s = Vector3.SignedAngle(transform.forward, ke, Vector3.up);
                if (Mathf.Abs(s) <= AssistKerucut && Mathf.Abs(s) < Mathf.Abs(terbaik)) { terbaik = s; yaw = s; }
            }
            if (terbaik > 900f) return;
            float langkah = Mathf.Clamp(yaw, -AssistLaju * dt, AssistLaju * dt);
            _ctrl.InputLihatPx.x += langkah / (AndiFPSController.SensH * Mathf.Max(0.01f, _ctrl.PengaliSensitivitas));
        }

        public static bool AimAssist = true;
        static readonly Quaternion PoseBlok = Quaternion.Euler(-20f, 0f, 70f);

        // Mengenai IDamageable: penghabisan bila jendela parry masih terbuka & tebasan berat.
        void KenaiTarget(IDamageable target, RaycastHit hit, Transform cam, float pengali)
        {
            bool habis = _berat && _tPenghabisan > 0f;
            float dmg = (_berat ? _aktif.berat : _aktif.ringan) * pengali * (habis ? PengaliPenghabisan : 1f);
            _pukulanPenghabisan = habis;
            try { target.TakeDamage(new DamageInfo { amount = dmg, type = _aktif.tipe, point = hit.point, direction = cam.forward, source = gameObject, isHeavy = _berat }); }
            finally { _pukulanPenghabisan = false; }
            if (habis) _tPenghabisan = 0f;
            if (GoreSystem.BolehDarah(GoreSystem.Tingkat)) { _darahSenjata = Mathf.Clamp01(_darahSenjata + (habis ? 0.7f : 0.35f)); TerapkanDarahSenjata(); }
            if (_berat) MulaiHitStop();
            Guncang(_berat ? GuncangBerat : GuncangRingan);
            var h = OnKena; if (h != null) h(_berat || habis);
            AlasKeramat.AI.MeterSuara.Pancar(hit.point, AlasKeramat.AI.MeterSuara.Pertarungan);
        }

        void TerapkanDarahSenjata()
        {
            if (pivotFP == null) return;
            if (_mpbDarah == null) _mpbDarah = new MaterialPropertyBlock();
            if (_rendererDarah == null) _rendererDarah = pivotFP.GetComponentsInChildren<Renderer>(true);
            var rends = _rendererDarah;
            Color tint = Color.Lerp(Color.white, new Color(0.45f, 0.04f, 0.04f, 1f), _darahSenjata * 0.85f);
            for (int i = 0; i < rends.Length; i++)
            {
                if (rends[i] == null) continue;
                rends[i].GetPropertyBlock(_mpbDarah);
                _mpbDarah.SetColor("_BaseColor", tint);
                _mpbDarah.SetFloat("_Smoothness", Mathf.Lerp(0.35f, 0.92f, _darahSenjata));
                rends[i].SetPropertyBlock(_mpbDarah);
            }
        }
    }
}
