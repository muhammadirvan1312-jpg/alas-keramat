using AlasKeramat.Core;
using UnityEngine;
namespace AlasKeramat.AI
{
    // Pola serangan Genderuwo (06 §10): Hantaman Tanah, Sapuan, Lempar Akar (semua fase), Amukan (fase 2+).
    public partial class BossGenderuwo
    {
        public const float JarakAmukMin = 8f, PeluangAmuk = 0.45f, PeluangLempar = 0.5f, CekTabrak = 1.6f, LamaCincin = 0.4f;

        void Otak(float dt)
        {
            Vector3 a = _andi.transform.position;
            Vector3 ke = a - transform.position; ke.y = 0f;
            float d = ke.magnitude;
            _t += dt;
            switch (Aksi)
            {
                case AksiBoss.Kejar:
                    Hadap(ke, dt, 4f);
                    _jeda -= dt;
                    if (_jeda <= 0f)
                    {
                        if (d <= JarakSapu + 0.5f) Mulai(Random.value < 0.5f ? AksiBoss.Sapu : AksiBoss.Hantam);
                        else if (Fase >= 2 && d >= JarakAmukMin && Random.value < PeluangAmuk) { _arahAmuk = ke.normalized; Mulai(AksiBoss.Amuk); }
                        else if (d <= JarakLempar && d > RadiusHantam && Random.value < PeluangLempar) Mulai(AksiBoss.Lempar);
                        else if (d <= RadiusHantam * 0.8f) Mulai(AksiBoss.Hantam);
                    }
                    Gerak(d > JarakSapu ? ke.normalized * LajuKini : Vector3.zero, dt);
                    Pose(ProceduralAnimator.PoseTempur.Tidak);
                    break;
                case AksiBoss.Hantam:
                    Pose(ProceduralAnimator.PoseTempur.Gertak);
                    Gerak(Vector3.zero, dt);
                    if (!_kena && _t >= TelegrafHantam)
                    {
                        _kena = true;
                        Suara(ProceduralAudioSynth.KlipGuruh, 1.6f);
                        _tCincin = 0f;
                        if (_combat != null) _combat.Guncang(0.4f);
                        if (d <= RadiusHantam) Kenai(DmgHantam, false);
                    }
                    if (_t >= TelegrafHantam + 0.6f) Selesai();
                    break;
                case AksiBoss.Sapu:
                    Hadap(ke, dt, 6f);
                    Pose(_t < TelegrafSapu ? ProceduralAnimator.PoseTempur.Ancang : ProceduralAnimator.PoseTempur.Ayun);
                    Gerak(Vector3.zero, dt);
                    if (!_kena && _t >= TelegrafSapu)
                    {
                        _kena = true;
                        Suara(ProceduralAudioSynth.KlipAyunSekte, 0.6f);
                        if (d <= JarakSapu + 0.6f && Vector3.Angle(transform.forward, ke) <= SudutSapu) Kenai(DmgSapu, true);
                    }
                    if (_t >= TelegrafSapu + 0.5f) Selesai();
                    break;
                case AksiBoss.Lempar:
                    Hadap(ke, dt, 6f);
                    Pose(ProceduralAnimator.PoseTempur.Ancang);
                    Gerak(Vector3.zero, dt);
                    if (!_kena && _t >= TelegrafLempar)
                    {
                        _kena = true;
                        _posAkar = transform.position + Vector3.up * 2.4f + transform.forward;
                        Vector3 sasaran = a + Vector3.up * 1.2f;
                        _arahAkar = (sasaran - _posAkar).normalized;
                        _jarakAkar = 0f;
                        if (akarLempar != null) { akarLempar.gameObject.SetActive(true); akarLempar.position = _posAkar; akarLempar.rotation = Quaternion.LookRotation(_arahAkar); }
                        Suara(ProceduralAudioSynth.KlipLepasPanah, 0.5f);
                    }
                    if (_t >= TelegrafLempar + 0.4f) Selesai();
                    break;
                case AksiBoss.Amuk:
                    Pose(ProceduralAnimator.PoseTempur.Gertak);
                    Hadap(_arahAmuk, dt, 20f);
                    if (_t < 0.5f) { Gerak(Vector3.zero, dt); break; } // ancang-ancang
                    bool tabrak = Gerak(_arahAmuk * LajuAmuk, dt) || Physics.Raycast(transform.position + Vector3.up * 1.2f, _arahAmuk, CekTabrak, (1 << GameLayers.Ground) | (1 << GameLayers.Structure), QueryTriggerInteraction.Ignore);
                    if (!_kena && d <= 1.8f) { _kena = true; Kenai(DmgAmuk, false); }
                    if (tabrak) { Mulai(AksiBoss.Pingsan); Suara(ProceduralAudioSynth.KlipGuruh, 1.2f); if (_combat != null) _combat.Guncang(0.6f); if (punggung != null) punggung.enabled = true; }
                    else if (_t >= LamaAmuk + 0.5f) Selesai();
                    break;
                case AksiBoss.Pingsan:
                    Pose(ProceduralAnimator.PoseTempur.Goyah);
                    Gerak(Vector3.zero, dt);
                    if (_t >= LamaPingsan) { if (punggung != null) punggung.enabled = false; Selesai(); }
                    break;
            }
        }

        void Mulai(AksiBoss baru) { Aksi = baru; _t = 0f; _kena = false; }
        void Selesai() { Aksi = AksiBoss.Kejar; _t = 0f; _jeda = JedaSerang * (Fase >= 3 ? 0.7f : 1f); }
        void Pose(ProceduralAnimator.PoseTempur p) { if (_anim != null) _anim.Pose = p; }

        void Kenai(float dmg, bool parry)
        {
            if (_combat == null) return;
            bool diparry;
            _combat.TerimaSerangan(transform.position, dmg, parry, out diparry);
        }

        void Hadap(Vector3 arah, float dt, float laju)
        {
            arah.y = 0f;
            if (arah.sqrMagnitude < 0.01f) return;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(arah), laju * dt);
        }

        // Mengembalikan true bila menabrak sisi (pilar/dinding).
        bool Gerak(Vector3 datar, float dt)
        {
            if (_cc == null || !_cc.enabled) return false;
            _vy = _cc.isGrounded ? -2f : _vy - Gravitasi * dt;
            var f = _cc.Move((datar + Vector3.up * _vy) * dt);
            return datar.sqrMagnitude > 1f && (f & CollisionFlags.Sides) != 0;
        }

        void TickAkar(float dt)
        {
            if (_jarakAkar < 0f || akarLempar == null) return;
            float langkah = LajuAkar * dt;
            _posAkar += _arahAkar * langkah; _jarakAkar += langkah;
            akarLempar.position = _posAkar;
            Vector3 mata = _andi.transform.position + Vector3.up * 1.2f;
            if ((mata - _posAkar).sqrMagnitude <= KenaAkar * KenaAkar) { Kenai(DmgAkar, false); _jarakAkar = -1f; akarLempar.gameObject.SetActive(false); return; }
            if (_jarakAkar >= JarakLempar + 4f) { _jarakAkar = -1f; akarLempar.gameObject.SetActive(false); }
        }

        void TickCincin(float dt)
        {
            if (cincinDebu == null || _tCincin < 0f) return;
            _tCincin += dt;
            float u = Mathf.Clamp01(_tCincin / LamaCincin);
            cincinDebu.gameObject.SetActive(u < 1f);
            cincinDebu.position = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);
            float r = RadiusHantam * 2f * u;
            cincinDebu.localScale = new Vector3(r, 0.05f, r);
            if (u >= 1f) _tCincin = -1f;
        }
    }
}
