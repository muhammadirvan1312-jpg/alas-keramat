using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // 06 §6 Serang: token penyerang (maks 2; Sulit 3), lainnya mengitari 6-9 m. Pola ayun (ancang 0,45 s) & lompatan 3 m (ancang 0,6 s).
    // Plus lokomosi: CharacterController + gravitasi + ground snap ke SampleHeight, jalur A* jauh / setir langsung dekat, hindar & pisah.
    public partial class NusantaraEnemyAndMutantAI
    {
        void TickSerang(float dt)
        {
            float d = JarakAndi();
            Vector3 posAndi = _andi.transform.position;
            if (_vital != null && !_vital.IsAlive) { Ubah(KeadaanSekte.Patroli); return; }
            if (!_terlihat && _tTakTerlihat > LamaLupa && d > JarakLupa) { _adaSelidik = true; _posSelidik = _posTerakhir; Ubah(KeadaanSekte.Selidik); return; }
            if (!_tokenku) _tokenku = _dir == null || _dir.MintaToken(this);
            if (!_tokenku) { Kitari(posAndi, d, dt); return; }

            _tFase += dt;
            switch (_fase)
            {
                case FaseSerang.Dekati:
                    if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak;
                    if (_jedaSerang <= 0f && d > JarakLompat - 0.5f && d < JarakLompat + 1f && Random.value < 0.25f * dt * 4f) { MulaiFase(FaseSerang.AncangLompat); break; }
                    if (CobaLempar(d)) { MulaiFase(FaseSerang.AncangLempar); break; }
                    if (d <= Jangkau && _jedaSerang <= 0f) { MulaiFase(FaseSerang.Ancang); Suara(ProceduralAudioSynth.KlipGeramSekte, 0.6f); break; }
                    if (d > Jangkau * 0.8f) Menuju(posAndi, d > 12f ? _d.lari : Mathf.Lerp(_d.jalan * 1.5f, _d.lari, d / 12f), dt);
                    else Diam(dt);
                    Hadap(posAndi - transform.position, dt);
                    break;
                case FaseSerang.Ancang:
                    Diam(dt); Hadap(posAndi - transform.position, dt);
                    if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Ancang;
                    if (_tFase >= AncangAyun) { MulaiFase(FaseSerang.Ayun); Suara(ProceduralAudioSynth.KlipAyunSekte, 0.9f); }
                    break;
                case FaseSerang.Ayun:
                    if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Ayun;
                    Gerak(transform.position + transform.forward, 1.2f, dt, false);
                    if (_tFase >= MomenKena && _tFase - dt < MomenKena) CobaKenai(_d.damage, Jangkau + 0.5f);
                    if (_tFase >= LamaAyun) Selesai();
                    break;
                case FaseSerang.AncangLompat:
                    Diam(dt); Hadap(posAndi - transform.position, dt);
                    if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Ancang;
                    if (_tFase >= AncangLompat) { _arahLompat = Datar(posAndi - transform.position).normalized; MulaiFase(FaseSerang.Lompat); Suara(ProceduralAudioSynth.KlipTeriakSekte, 0.8f); }
                    break;
                case FaseSerang.Lompat:
                    if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Ayun;
                    GerakMentah(_arahLompat * (JarakLompat / LamaLompat), dt);
                    if (_tFase >= LamaLompat) { CobaKenai(_d.damageBerat, Jangkau + 0.3f); Selesai(); }
                    break;
                case FaseSerang.AncangLempar:
                    Diam(dt); Hadap(posAndi - transform.position, dt);
                    if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Ancang;
                    if (_tFase >= AncangLempar) { if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Ayun; LemparSekarang(); Selesai(); }
                    break;
                case FaseSerang.Jeda:
                    Diam(dt); Hadap(posAndi - transform.position, dt);
                    if (_tFase > 0.5f) { _fase = FaseSerang.Dekati; _tFase = 0f; }
                    break;
            }
        }

        void MulaiFase(FaseSerang f) { _fase = f; _tFase = 0f; }

        void Selesai()
        {
            _jedaSerang = Random.Range(JedaSeranganMin, JedaSeranganMaks);
            MulaiFase(FaseSerang.Jeda);
            if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak;
            // Bergiliran: lepas token setelah satu serangan agar Sekte yang mengitari dapat giliran.
            if (_dir != null && _dir.AdaYangMenunggu(this)) LepasToken();
        }

        void CobaKenai(float damage, float jangkau)
        {
            if (_andi == null || _vital == null || !_vital.IsAlive) return;
            Vector3 ke = Datar(_andi.transform.position - transform.position);
            if (ke.magnitude > jangkau || Vector3.Angle(transform.forward, ke) > KerucutKena) return;
            bool parry = false;
            float akhir = _combat != null ? _combat.TerimaSerangan(transform.position, damage, true, out parry) : damage;
            if (parry) { Diparry(); return; }
            _vital.TakeDamage(new DamageInfo { amount = akhir, type = DamageType.Tebas, point = _andi.transform.position + Vector3.up * 1.2f, direction = ke.normalized, source = gameObject, isHeavy = akhir >= 25f });
        }

        void Kitari(Vector3 posAndi, float d, float dt)
        {
            if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak;
            Vector3 dari = Datar(transform.position - posAndi);
            if (dari.sqrMagnitude < 0.01f) dari = transform.forward;
            float radius = Mathf.Clamp(d, KitarMin, KitarMaks);
            Vector3 arah = Quaternion.Euler(0f, 25f * _sisiKitar, 0f) * dari.normalized;
            if (Random.value < 0.08f * dt) _sisiKitar = -_sisiKitar;
            Vector3 target = posAndi + arah * radius;
            if (d > KitarMaks + 4f) Menuju(target, _d.lari * 0.8f, dt);
            else Gerak(target, _d.jalan * 1.3f, dt, false);
            Hadap(posAndi - transform.position, dt);
        }

        void LepasToken()
        {
            if (!_tokenku) return;
            _tokenku = false;
            if (_dir != null) _dir.LepasToken(this);
        }

        // ---------------- Lokomosi ----------------
        void Menuju(Vector3 tujuan, float laju, float dt)
        {
            float d = Datar(tujuan - transform.position).magnitude;
            if (WorldTerrainGenerator.DiBawahTanah(transform.position.y)) { MenujuGua(tujuan, laju, dt, d); return; }
            if (d < JarakLangsung || _pf == null || _pf.Grid == null) { Gerak(tujuan, laju, dt, true); return; }
            if (!_menungguJalur && (_tJalur <= 0f || Datar(tujuan - _tujuanJalur).sqrMagnitude > 25f))
            {
                _menungguJalur = true; _tujuanJalur = tujuan; _tJalur = JedaJalur;
                _pf.Minta(transform.position, tujuan, _jalurBaru, _cbJalur);
            }
            while (_iJalur < _jalur.Count && Datar(_jalur[_iJalur] - transform.position).magnitude < 1.6f) _iJalur++;
            Gerak(_iJalur < _jalur.Count ? _jalur[_iJalur] : tujuan, laju, dt, true);
        }

        void SelesaiJalur(bool ok)
        {
            _menungguJalur = false;
            if (this == null || !ok) return;
            var t = _jalur; _jalur = _jalurBaru; _jalurBaru = t;
            _iJalur = 0;
        }

        void Gerak(Vector3 tujuan, float laju, float dt, bool hadap)
        {
            Vector3 ke = Datar(tujuan - transform.position);
            float d = ke.magnitude;
            Vector3 arah = d > 0.05f ? ke / d : Vector3.zero;
            if (arah != Vector3.zero) arah = Hindari(arah);
            arah += Pisah() * 0.6f;
            if (arah.sqrMagnitude > 1f) arah.Normalize();
            if (hadap && arah.sqrMagnitude > 0.01f) Hadap(arah, dt);
            float lajuAkhir = Mathf.Min(laju, d / Mathf.Max(dt, 0.0001f));
            GerakMentah(arah * lajuAkhir, dt);
        }

        void GerakMentah(Vector3 datar, float dt)
        {
            if (_cc == null || !_cc.enabled) return;
            _vy = _cc.isGrounded ? -2f : _vy - Gravitasi * dt;
            _cc.Move((datar + Vector3.up * _vy) * dt);
            Vector3 p = transform.position;
            float h = WorldTerrainGenerator.Lantai(p);
            float? air = WorldTerrainGenerator.AirDi(p);
            float lantai = h;
            if (air.HasValue && air.Value - _cc.height * 0.75f > h) lantai = air.Value - _cc.height * 0.75f; // mengapung/mengarungi sungai
            if (p.y < lantai - 0.05f) { transform.position = new Vector3(p.x, lantai, p.z); _vy = 0f; }
        }

        void Diam(float dt) { GerakMentah(Pisah() * 0.5f, dt); }

        void Hadap(Vector3 arah, float dt)
        {
            arah.y = 0f;
            if (arah.sqrMagnitude < 0.0001f) return;
            var target = Quaternion.LookRotation(arah);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, LajuBelok * dt);
        }

        Vector3 Hindari(Vector3 arah)
        {
            int mask = (1 << GameLayers.Tree) | (1 << GameLayers.Structure) | (1 << GameLayers.LogPhysics);
            Vector3 asal = transform.position + Vector3.up * 0.9f;
            if (!Physics.SphereCast(asal, 0.35f, arah, out _hitHindar, 1.6f, mask, QueryTriggerInteraction.Ignore)) return arah;
            Vector3 kanan = Vector3.Cross(Vector3.up, arah);
            bool kananBebas = !Physics.SphereCast(asal, 0.35f, (arah + kanan).normalized, out _hitHindar, 1.6f, mask, QueryTriggerInteraction.Ignore);
            return (arah * 0.3f + (kananBebas ? kanan : -kanan)).normalized;
        }

        RaycastHit _hitHindar;

        Vector3 Pisah()
        {
            Vector3 dorong = Vector3.zero;
            for (int i = 0; i < Semua.Count; i++)
            {
                var o = Semua[i];
                if (o == this || o == null || !o.IsAlive) continue;
                Vector3 ke = Datar(transform.position - o.transform.position);
                float m = ke.magnitude;
                if (m < RadiusPisah && m > 0.001f) dorong += ke / m * (1f - m / RadiusPisah);
            }
            return dorong;
        }

        Vector3 TitikMenjauh(float jarak) { return _andi != null ? TitikMenjauhDari(_andi.transform.position, jarak) : transform.position + transform.forward * jarak; }

        Vector3 TitikMenjauhDari(Vector3 dari, float jarak)
        {
            Vector3 arah = Datar(transform.position - dari);
            if (arah.sqrMagnitude < 0.01f) arah = -transform.forward;
            Vector3 p = transform.position + arah.normalized * jarak;
            p.x = Mathf.Clamp(p.x, -WorldTerrainGenerator.BatasDunia + 20f, WorldTerrainGenerator.BatasDunia - 20f);
            p.z = Mathf.Clamp(p.z, -WorldTerrainGenerator.BatasDunia + 20f, WorldTerrainGenerator.BatasDunia - 20f);
            p.y = WorldTerrainGenerator.Lantai(p);
            return p;
        }

        void Suara(string klip, float vol)
        {
            if (_audio != null) _audio.Play(klip, transform.position + Vector3.up * 1.5f, AudioCategory.Efek, vol, Random.Range(0.9f, 1.1f), 2);
        }
    }
}
