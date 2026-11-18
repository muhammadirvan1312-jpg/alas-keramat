using System.Collections.Generic;
using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.AI
{
    // 08 §1 (T8): dasar companion - lokomosi CharacterController + GridPathfinder, Ikuti (3-5 m belakang-kiri/kanan),
    // teleport bila > 80 m & tak terlihat kamera 3 s (25 m di belakang Andi, tidak pernah di depan kamera), Kritis 45 s -> BERI JAMU / gugur.
    public abstract class CompanionDasar : MonoBehaviour, IDamageable
    {
        public const float JarakTeleport = 80f, LamaTakTerlihat = 3f, MundurTeleport = 25f, LamaKritis = 45f, LamaPingsanDamai = 30f, HPSetelahJamu = 40f;
        public const float Gravitasi = 18f, LajuBelok = 360f, JedaJalur = 2f, JarakLangsung = 10f, SudutPandangKamera = 70f;

        protected CharacterController _cc;
        protected ProceduralAnimator _anim;
        protected AndiFPSController _andi;
        protected ProceduralAudioSynth _audio;
        protected GridPathfinder _pf;
        protected TimeOfDayClock _jam;

        public float HP { get; protected set; }
        public abstract float HPMaks { get; }
        public bool Kritis { get; protected set; }
        public bool Gugur { get; protected set; }
        public bool IsAlive { get { return !Gugur && !Kritis; } }
        protected abstract float SisiIkuti { get; } // -1 kiri (Deden), +1 kanan (Ningsih)

        float _vy, _tKritis, _tTakTerlihat, _tJalur;
        bool _menungguJalur;
        int _iJalur;
        Vector3 _tujuanJalur;
        List<Vector3> _jalur = new List<Vector3>(), _jalurBaru = new List<Vector3>();
        System.Action<bool> _cbJalur;

        protected virtual void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _anim = GetComponent<ProceduralAnimator>();
            _cbJalur = SelesaiJalur;
            HP = HPMaks;
        }

        protected void AmbilLayanan()
        {
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            if (_audio == null) _audio = GameServices.Get<ProceduralAudioSynth>();
            if (_pf == null) _pf = GameServices.Get<GridPathfinder>();
            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
        }

        // ---------- Rumus murni (self-test) ----------
        // Titik di luar pandangan kamera: sudut dari arah pandang > 70 derajat ATAU terhalang (diperiksa pemanggil).
        public static bool BolehTeleport(Vector3 posKamera, Vector3 arahKamera, Vector3 titik)
        {
            Vector3 ke = titik - posKamera;
            return ke.sqrMagnitude > 0.01f && Vector3.Angle(arahKamera, ke) > SudutPandangKamera;
        }

        public static Vector3 TitikIkuti(Vector3 andi, Vector3 maju, float sisi)
        {
            maju.y = 0f; if (maju.sqrMagnitude < 0.001f) maju = Vector3.forward; maju.Normalize();
            Vector3 kanan = Vector3.Cross(Vector3.up, maju);
            return andi - maju * 4f + kanan * (1.6f * sisi);
        }

        protected bool TerlihatKamera()
        {
            var cam = _andi != null ? _andi.Kamera : null;
            if (cam == null) return false;
            Vector3 p = transform.position + Vector3.up * 1.2f;
            if (BolehTeleport(cam.position, cam.forward, p)) return false;
            return !Physics.Linecast(cam.position, p, (1 << GameLayers.Ground) | (1 << GameLayers.Structure) | (1 << GameLayers.Tree), QueryTriggerInteraction.Ignore);
        }

        // Dipanggil subclass tiap frame saat mengikuti / dari mana pun.
        protected void CekTeleport(float dt)
        {
            if (_andi == null || (_andi.transform.position.y < -40f) != (transform.position.y < -40f)) return;
            float d = Datar(_andi.transform.position - transform.position).magnitude;
            if (d <= JarakTeleport || TerlihatKamera()) { _tTakTerlihat = 0f; return; }
            _tTakTerlihat += dt;
            if (_tTakTerlihat < LamaTakTerlihat) return;
            var cam = _andi.Kamera;
            Vector3 maju = _andi.transform.forward;
            for (int k = 0; k < 6; k++)
            {
                Vector3 arah = Quaternion.Euler(0f, (k - 2.5f) * 20f, 0f) * -maju;
                Vector3 p = _andi.transform.position + arah * MundurTeleport;
                p.y = WorldTerrainGenerator.SampleHeight(p.x, p.z);
                if (p.y < 0.2f) continue;
                if (cam != null && !BolehTeleport(cam.position, cam.forward, p + Vector3.up)) continue;
                Teleport(p);
                _tTakTerlihat = 0f;
                return;
            }
        }

        protected void Teleport(Vector3 p)
        {
            bool cc = _cc != null && _cc.enabled;
            if (cc) _cc.enabled = false;
            transform.position = p + Vector3.up * 0.1f;
            if (cc) _cc.enabled = true;
            _jalur.Clear(); _iJalur = 0;
        }

        // ---------- Lokomosi ----------
        protected static Vector3 Datar(Vector3 v) { v.y = 0f; return v; }

        protected void Menuju(Vector3 tujuan, float laju, float dt)
        {
            if ((tujuan.y < -40f) != (transform.position.y < -40f)) { Diam(dt); return; }
            float d = Datar(tujuan - transform.position).magnitude;
            if (d < JarakLangsung || _pf == null || _pf.Grid == null) { Gerak(tujuan, laju, dt); return; }
            _tJalur -= dt;
            if (!_menungguJalur && (_tJalur <= 0f || Datar(tujuan - _tujuanJalur).sqrMagnitude > 25f))
            {
                _menungguJalur = true; _tujuanJalur = tujuan; _tJalur = JedaJalur;
                _pf.Minta(transform.position, tujuan, _jalurBaru, _cbJalur);
            }
            while (_iJalur < _jalur.Count && Datar(_jalur[_iJalur] - transform.position).magnitude < 1.6f) _iJalur++;
            Gerak(_iJalur < _jalur.Count ? _jalur[_iJalur] : tujuan, laju, dt);
        }

        void SelesaiJalur(bool ok)
        {
            _menungguJalur = false;
            if (this == null || !ok) return;
            var t = _jalur; _jalur = _jalurBaru; _jalurBaru = t; _iJalur = 0;
        }

        protected void Gerak(Vector3 tujuan, float laju, float dt)
        {
            Vector3 ke = Datar(tujuan - transform.position);
            float d = ke.magnitude;
            Vector3 arah = d > 0.05f ? ke / d : Vector3.zero;
            if (arah != Vector3.zero) { arah = Hindari(arah); Hadap(arah, dt); }
            GerakMentah(arah * Mathf.Min(laju, d / Mathf.Max(dt, 0.0001f)), dt);
        }

        protected void GerakMentah(Vector3 datar, float dt)
        {
            if (_cc == null || !_cc.enabled) return;
            _vy = _cc.isGrounded ? -2f : _vy - Gravitasi * dt;
            _cc.Move((datar + Vector3.up * _vy) * dt);
            Vector3 p = transform.position;
            float h = WorldTerrainGenerator.SampleHeight(p.x, p.z);
            float? air = WorldTerrainGenerator.GetWaterLevelAt(p.x, p.z);
            float lantai = h;
            if (air.HasValue && air.Value - _cc.height * 0.75f > h) lantai = air.Value - _cc.height * 0.75f; // berenang
            if (p.y < lantai - 0.05f) { transform.position = new Vector3(p.x, lantai, p.z); _vy = 0f; }
        }

        protected void Diam(float dt) { GerakMentah(Vector3.zero, dt); }

        protected void Hadap(Vector3 arah, float dt)
        {
            arah.y = 0f;
            if (arah.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(arah), LajuBelok * dt);
        }

        RaycastHit _hit;
        Vector3 Hindari(Vector3 arah)
        {
            int mask = (1 << GameLayers.Tree) | (1 << GameLayers.Structure) | (1 << GameLayers.LogPhysics);
            Vector3 asal = transform.position + Vector3.up * 0.9f;
            if (!Physics.SphereCast(asal, 0.3f, arah, out _hit, 1.4f, mask, QueryTriggerInteraction.Ignore)) return arah;
            Vector3 kanan = Vector3.Cross(Vector3.up, arah);
            bool bebas = !Physics.SphereCast(asal, 0.3f, (arah + kanan).normalized, out _hit, 1.4f, mask, QueryTriggerInteraction.Ignore);
            return (arah * 0.3f + (bebas ? kanan : -kanan)).normalized;
        }

        // ---------- HP, kritis, gugur ----------
        public virtual void TakeDamage(DamageInfo info)
        {
            if (!IsAlive || info.amount <= 0f) return;
            HP = Mathf.Max(0f, HP - info.amount);
            var gore = GameServices.Get<GoreSystem>();
            if (gore != null && info.amount >= 5f) gore.Semburan(info.point == Vector3.zero ? transform.position + Vector3.up : info.point, info.direction, info.amount * 0.5f, false, false);
            if (HP <= 0f) MasukKritis();
        }

        protected virtual void MasukKritis()
        {
            Kritis = true; _tKritis = 0f;
            if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Menyerah;
            GameEvents.RaiseToast(NamaTampil + " KRITIS - berikan Jamu Herbal atau Daun Sirih!");
        }

        protected void TickKritis(float dt)
        {
            _tKritis += dt;
            if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Merangkak;
            Diam(dt);
            bool damai = EnemyDirector.Kesulitan == Difficulty.Damai;
            if (damai && _tKritis >= LamaPingsanDamai) { Kritis = false; HP = HPSetelahJamu; GameEvents.RaiseToast(NamaTampil + " siuman"); return; }
            if (!damai && _tKritis >= LamaKritis)
            {
                Kritis = false; Gugur = true;
                GameEvents.RaiseSubtitle("(" + NamaTampil + " tidak bergerak lagi.)");
                if (_anim != null) _anim.enabled = false;
                transform.rotation = Quaternion.Euler(-85f, transform.eulerAngles.y, 0f);
            }
        }

        public bool BeriJamu()
        {
            if (!Kritis) return false;
            var inv = GameServices.Get<AlasKeramat.Inventory.TikarPandan3DInventory>();
            if (inv == null || !(inv.Kurangi(ItemID.JamuHerbal, 1) || inv.Kurangi(ItemID.DaunSirih, 1))) { GameEvents.RaiseToast("Butuh Jamu Herbal atau Daun Sirih"); return false; }
            Kritis = false; HP = HPSetelahJamu;
            if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak;
            GameEvents.RaiseToast(NamaTampil + " pulih");
            return true;
        }

        public abstract string NamaTampil { get; }

        protected void Pulih(float perDetik, float dt) { if (IsAlive) HP = Mathf.Min(HPMaks, HP + perDetik * dt); }

        // Kemah = Api Unggun terdekat dari companion dalam 300 m.
        public Vector3? Kemah()
        {
            Vector3 terbaik = Vector3.zero; float jarak = 300f; bool ada = false;
            for (int i = 0; i < ApiUnggun.Semua.Count; i++)
            {
                var a = ApiUnggun.Semua[i]; if (a == null) continue;
                float d = Datar(a.transform.position - transform.position).magnitude;
                if (d < jarak) { jarak = d; terbaik = a.transform.position; ada = true; }
            }
            return ada ? terbaik : (Vector3?)null;
        }

        protected void Klip(string nama) { if (_anim != null) _anim.Mainkan(nama); }
    }
}
