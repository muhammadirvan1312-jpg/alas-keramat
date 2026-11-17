using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Perilaku per jenis hantu (06 §8).
    public partial class HantuAI
    {
        const float LajuLompatPocong = PocongJangkauTerjang / PocongLamaTerjang, JedaDentum = 0.6f, TinggiKunti = 2.4f, TinggiBertengger = 6f, TinggiBanaspati = 1.4f;

        float _tDentum, _tKitar;

        // ---------------- Pocong ----------------
        void TickPocong(float dt)
        {
            float d = JarakAndi();
            float indra = IndraPocong(_vital != null ? _vital.Sanity : 100f);
            if (TerusirPenangkal(transform.position) && _fase != FaseHantu.Terjungkal)
            {
                Hadap(-KeAndi(), dt);
                Geser(-KeAndi().normalized * _d.jalan, dt, 0f);
                if (_fase != FaseHantu.Diam) Ubah(FaseHantu.Diam);
                return;
            }
            switch (_fase)
            {
                case FaseHantu.Diam:
                    if (d <= indra) Ubah(FaseHantu.Mendekat);
                    break;
                case FaseHantu.Mendekat:
                    Hadap(KeAndi(), dt);
                    Geser(KeAndi().normalized * _d.jalan, dt, 0f);
                    _tDentum -= dt;
                    if (_tDentum <= 0f) { _tDentum = JedaDentum; if (_audio != null) _audio.PlayPocongHopThud(transform.position); }
                    if (d <= PocongJarakTerjang) Ubah(FaseHantu.Ancang);
                    else if (d > indra * 1.5f) Ubah(FaseHantu.Diam);
                    break;
                case FaseHantu.Ancang:
                    Hadap(KeAndi(), dt, 600f);
                    transform.localScale = new Vector3(1f, Mathf.Lerp(1f, 0.85f, _t / PocongAncang), 1f); // jongkok ancang-ancang
                    if (_t >= PocongAncang) { _arahTerjang = KeAndi().normalized; transform.localScale = Vector3.one; Ubah(FaseHantu.Terjang); }
                    break;
                case FaseHantu.Terjang:
                    Geser(_arahTerjang * LajuLompatPocong, dt, 0.3f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(_t / PocongLamaTerjang)));
                    if (!_sudahKena && d < 1.3f)
                    {
                        _sudahKena = true; bool parry;
                        KenaiAndi(_d.damage, true, DamageType.Tumpul, out parry);
                        if (parry) { Ubah(FaseHantu.Terjungkal); break; }
                    }
                    if (_t >= PocongLamaTerjang) Ubah(FaseHantu.Pulih);
                    break;
                case FaseHantu.Pulih:
                    if (_t >= PocongPulih) Ubah(FaseHantu.Mendekat);
                    break;
                case FaseHantu.Terjungkal:
                    var e = transform.eulerAngles;
                    float rebah = _t < PocongTerjungkal - 0.4f ? Mathf.Min(1f, _t / 0.3f) : Mathf.Clamp01((PocongTerjungkal - _t) / 0.4f);
                    transform.rotation = Quaternion.Euler(-80f * rebah, e.y, 0f);
                    if (_t >= PocongTerjungkal) { transform.rotation = Quaternion.Euler(0f, e.y, 0f); Ubah(FaseHantu.Mendekat); }
                    break;
                default: Ubah(FaseHantu.Diam); break;
            }
        }

        // ---------------- Kuntilanak ----------------
        void TickKuntilanak(float dt)
        {
            float d = JarakAndi();
            _tJerit -= dt; _tTawa -= dt;
            if (_tTawa <= 0f && _fase != FaseHantu.Hilang) { _tTawa = Random.Range(KuntiTawaMin, KuntiTawaMaks); if (_audio != null) _audio.PlayKuntilanakLaugh(transform.position); }
            switch (_fase)
            {
                case FaseHantu.Diam:
                    Melayang(Sarang, TinggiBertengger, dt, 3f);
                    if (d <= KuntiIndra) { Ubah(FaseHantu.Mengitari); _tKitar = Random.Range(4f, 8f); }
                    break;
                case FaseHantu.Mengitari:
                    _sudut += 30f * dt;
                    float r = _sudut * Mathf.Deg2Rad;
                    Vector3 target = _andi.transform.position + new Vector3(Mathf.Cos(r), 0f, Mathf.Sin(r)) * KuntiRadiusKitar;
                    Melayang(target, TinggiKunti, dt, _d.jalan);
                    Hadap(KeAndi(), dt);
                    if (Disorot(dt)) { Ubah(FaseHantu.Pulih); GameEvents.RaiseToast("Kuntilanak terpaku cahaya"); break; }
                    if (_tJerit <= 0f && _t > 3f) { Ubah(FaseHantu.Menjerit); break; }
                    if (_t > _tKitar) { Ubah(FaseHantu.Ancang); break; }
                    if (d > KuntiIndra * 2f) Ubah(FaseHantu.Diam);
                    break;
                case FaseHantu.Menjerit:
                    if (!_sudahKena)
                    {
                        _sudahKena = true; _tJerit = KuntiJedaJerit;
                        if (_audio != null) _audio.PlayKuntilanakLaugh(transform.position);
                        if (_vital != null) _vital.UbahSanity(SanityJerit);
                        var rk = GameServices.Get<ReaksiAndi>(); if (rk != null) rk.Setrum(LamaSetrum);
                    }
                    Hadap(KeAndi(), dt);
                    if (_t >= 1f) Ubah(FaseHantu.Mengitari);
                    break;
                case FaseHantu.Ancang:
                    Hadap(KeAndi(), dt, 600f);
                    if (_t >= KuntiAncang) { Vector3 ke = (_andi.transform.position + Vector3.up * 1.4f) - transform.position; _arahTerjang = ke.normalized; Ubah(FaseHantu.Terjang); }
                    break;
                case FaseHantu.Terjang:
                    transform.position += _arahTerjang * KuntiLajuTukik * dt;
                    float hTanah = WorldTerrainGenerator.SampleHeight(transform.position.x, transform.position.z);
                    if (transform.position.y < hTanah + 0.4f) transform.position = new Vector3(transform.position.x, hTanah + 0.4f, transform.position.z);
                    if (!_sudahKena && Vector3.Distance(transform.position, _andi.transform.position + Vector3.up) < 1.4f)
                    {
                        _sudahKena = true; bool parry;
                        KenaiAndi(_d.damage, true, DamageType.Gaib, out parry);
                        if (parry) { Ubah(FaseHantu.Hilang); TampakSemua(false); _t = -2f; break; } // mundur & menghilang 10 s
                    }
                    if (_t >= 1.2f || (_sudahKena && _t > 0.3f)) Ubah(FaseHantu.Pulih);
                    break;
                case FaseHantu.Pulih:
                    Melayang(transform.position, TinggiKunti * 0.6f, dt, 0.5f);
                    if (_t >= KuntiRentan) { Ubah(FaseHantu.Mengitari); _tKitar = Random.Range(4f, 8f); }
                    break;
                case FaseHantu.Hilang:
                    if (_t >= KuntiHilang)
                    {
                        Vector3 belakang = _andi.transform.position - _andi.transform.forward * 4f;
                        transform.position = new Vector3(belakang.x, WorldTerrainGenerator.SampleHeight(belakang.x, belakang.z) + TinggiKunti, belakang.z);
                        TampakSemua(true);
                        if (_audio != null) _audio.PlayKuntilanakLaugh(transform.position);
                        Ubah(FaseHantu.Mengitari); _tKitar = Random.Range(2f, 4f);
                    }
                    break;
                default: Ubah(FaseHantu.Diam); break;
            }
        }

        // Disorot cahaya Andi terus-menerus 2 s (obor/senter menyala & kamera mengarah < 12 derajat) -> goyah & terlihat jelas.
        bool Disorot(float dt)
        {
            bool sorot = _kiri != null && _kiri.OborMenyala && _andi.Kamera != null
                && Vector3.Angle(_andi.Kamera.forward, transform.position - _andi.Kamera.position) < 12f;
            _tSorot = sorot ? _tSorot + dt : Mathf.Max(0f, _tSorot - dt);
            if (_tSorot < KuntiLamaSorot) return false;
            _tSorot = 0f;
            return true;
        }

        void Melayang(Vector3 tujuan, float tinggi, float dt, float laju)
        {
            Vector3 ke = tujuan - transform.position; ke.y = 0f;
            Vector3 v = ke.magnitude > 0.1f ? ke.normalized * Mathf.Min(laju, ke.magnitude / Mathf.Max(dt, 0.0001f)) : Vector3.zero;
            Geser(v, dt, tinggi + 0.2f * Mathf.Sin(Time.time * 1.3f));
        }

        // ---------------- Banaspati ----------------
        void TickBanaspati(float dt)
        {
            bool hujan = _cuaca != null && (_cuaca.Cuaca == WeatherState.Gerimis || _cuaca.Cuaca == WeatherState.HujanBadai);
            if (BanaspatiPadam(hujan, WorldTerrainGenerator.IsUnderwater(transform.position))) { Usir(); return; }
            if (cahaya != null) cahaya.intensity = 2.2f + 0.6f * Mathf.PerlinNoise(Time.time * 6f, 0.3f);
            float d = JarakAndi();
            switch (_fase)
            {
                case FaseHantu.Diam:
                    Melayang(Sarang, TinggiBanaspati, dt, 2f);
                    if (d <= BanaspatiIndra) { Ubah(FaseHantu.Mengitari); _tKitar = Random.Range(3f, 5f); }
                    break;
                case FaseHantu.Mengitari:
                    _sudut += 70f * dt;
                    float r = _sudut * Mathf.Deg2Rad;
                    Melayang(_andi.transform.position + new Vector3(Mathf.Cos(r), 0f, Mathf.Sin(r)) * BanaspatiOrbit, TinggiBanaspati, dt, _d.jalan);
                    if (_t > _tKitar) Ubah(FaseHantu.Ancang);
                    if (d > BanaspatiIndra * 2f) Ubah(FaseHantu.Diam);
                    break;
                case FaseHantu.Ancang:
                    if (_t >= 0.4f) { Vector3 ke = (_andi.transform.position + Vector3.up) - transform.position; _arahTerjang = ke.normalized; Ubah(FaseHantu.Terjang); }
                    break;
                case FaseHantu.Terjang:
                    transform.position += _arahTerjang * BanaspatiLajuTerjang * dt;
                    if (!_sudahKena && Vector3.Distance(transform.position, _andi.transform.position + Vector3.up) < 1.1f)
                    {
                        _sudahKena = true; bool parry;
                        KenaiAndi(_d.damage, false, DamageType.Api, out parry);
                        var rk = GameServices.Get<ReaksiAndi>(); if (rk != null) rk.Bakar(BakarBanaspati, LamaBakar);
                    }
                    BakarStrukturDekat();
                    if (_t >= 1.2f) { Ubah(FaseHantu.Mengitari); _tKitar = Random.Range(3f, 5f); }
                    break;
                default: Ubah(FaseHantu.Diam); break;
            }
        }

        static readonly Collider[] _hasilOverlap = new Collider[8];

        void BakarStrukturDekat()
        {
            int n = Physics.OverlapSphereNonAlloc(transform.position, 0.9f, _hasilOverlap, 1 << GameLayers.Structure, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                var b = _hasilOverlap[i].GetComponentInParent<AlasKeramat.Building.BagianStruktur>();
                if (b != null) ApiStruktur.Nyalakan(b.gameObject);
            }
        }
    }
}
