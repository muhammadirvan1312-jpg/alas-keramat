using System.Collections.Generic;
using UnityEngine;

namespace AlasKeramat.Core
{
    // 13 §2 (T9): mesin fase Sutradara Teror murni (tanpa scene) - Tension, fase TENANG/MEREMANG/MENCEKAM/LEGA, anggaran kejutan,
    // larangan ulang 2 hari game, jeda 20 s antar kejadian. Dipakai TerrorDirector saat main & disimulasikan NusantaraSelfTest.
    public class PenjadwalTeror
    {
        public enum Fase { Tenang, Meremang, Mencekam, Lega }
        public enum TipeTeror { Ringan, Sedang, Keras }
        public enum Intensitas { Ringan, Normal, Ekstrem }

        public struct Konteks
        {
            public bool malam, gelap, bawahTanah, malamDarah, diApiBersamaTeman, siangCerah, sendirian, headset, bawaCahaya, hujanBadai, berenang, diStruktur, jongkokGelap, tempur, dekatGua;
            public bool kuntiDekat80, kuntiDekat30, kuntiAktif120, hantuDekat20, pocongTerlihat30, dedenTidurDiKemah, ningsihDiKemah, dekatDesa250, tidur;
            public float sanity, ancaman, jam, kemarahan; public int hari; public float jarakDariKemah;
        }

        public struct Kejadian { public string id, nama; public TipeTeror tipe; public bool ulang; public System.Func<Konteks, bool> syarat; }

        public const float MinTenang = 120f, MinMeremang = 90f, MaksMeremang = 240f, MinCekam = 20f, MaksCekam = 60f, MinLega = 30f, MaksLega = 60f;
        public const float JedaAntarKejadian = 20f, AmbangMeremang = 40f, LamaAmbang = 20f, AmbangCekam = 65f, DetikPerHariGame = 1440f;
        public const float JedaKeras = 480f, JedaKerasEkstrem = 300f, JedaSedang = 180f;

        public Fase FaseKini { get; private set; } = Fase.Tenang;
        public float Tension { get; private set; } = 15f;
        public Intensitas Setelan = Intensitas.Normal;
        public int JumlahRingan, JumlahSedang, JumlahKeras;

        readonly List<Kejadian> _katalog;
        readonly Dictionary<string, float> _terakhir = new Dictionary<string, float>();
        readonly HashSet<string> _pernah = new HashSet<string>();
        readonly System.Random _rng;
        float _waktu, _tFase, _lamaFase, _tAmbang, _tRingan, _terakhirKejadian = -999f, _terakhirKeras, _terakhirSedang = -999f;
        int _ringanFaseIni;

        public PenjadwalTeror(List<Kejadian> katalog, int seed) { _katalog = katalog; _rng = new System.Random(seed); _lamaFase = MinTenang; }

        public static float HitungTension(Konteks k)
        {
            float t = 15f + (k.malam ? 20f : 0f) + (k.gelap ? 15f : 0f) + (k.bawahTanah ? 25f : 0f) + (k.malamDarah ? 20f : 0f)
                + 0.25f * (100f - k.sanity) + Mathf.Clamp(k.ancaman, 0f, 30f) - (k.diApiBersamaTeman ? 20f : 0f) - (k.siangCerah ? 10f : 0f);
            return Mathf.Clamp(t, 0f, 100f);
        }

        public static float Ancaman(float jarakMusuh) { return jarakMusuh <= 10f ? 30f : Mathf.Clamp01((60f - jarakMusuh) / 50f) * 30f; }

        float Acak(float a, float b) { return a + (float)_rng.NextDouble() * (b - a); }
        float JedaKerasAktif { get { return Setelan == Intensitas.Ekstrem ? JedaKerasEkstrem : JedaKeras; } }

        // Maju satu langkah; mengembalikan kejadian yang dipicu (id kosong bila tidak ada).
        public Kejadian Langkah(float dt, Konteks k)
        {
            _waktu += dt; _tFase += dt;
            Tension = Mathf.Lerp(Tension, HitungTension(k), Mathf.Clamp01(0.5f * dt));
            _tAmbang = Tension > AmbangMeremang ? _tAmbang + dt : 0f;
            var kosong = new Kejadian();
            switch (FaseKini)
            {
                case Fase.Tenang:
                    if (_tFase >= MinTenang && _tAmbang >= LamaAmbang) Ganti(Fase.Meremang, Acak(MinMeremang, MaksMeremang));
                    return kosong;
                case Fase.Meremang:
                    _tRingan -= dt;
                    bool bisaCekam = Tension > AmbangCekam && _ringanFaseIni >= 1 && (BolehKeras() || BolehSedang());
                    if (_tFase >= _lamaFase && (_ringanFaseIni >= 2 || _tFase >= MaksMeremang))
                    {
                        if (bisaCekam) { Ganti(Fase.Mencekam, Acak(MinCekam, MaksCekam)); return kosong; }
                        if (Tension <= AmbangMeremang) { Ganti(Fase.Lega, Acak(MinLega, MaksLega)); return kosong; }
                    }
                    if (_tRingan <= 0f && Boleh(k))
                    {
                        var r = Pilih(TipeTeror.Ringan, k);
                        if (r.id != null) { _tRingan = Acak(45f, 90f); _ringanFaseIni++; Catat(r); return r; }
                    }
                    return kosong;
                case Fase.Mencekam:
                    if (_tFase >= 4f && _ringanFaseIni >= 0) // jeda sunyi 3-5 s sebelum kejutan
                    {
                        _ringanFaseIni = -1;
                        if (Boleh(k))
                        {
                            Kejadian e = BolehKeras() && _rng.NextDouble() < 0.4 ? Pilih(TipeTeror.Keras, k) : kosong;
                            if (e.id == null && BolehSedang()) e = Pilih(TipeTeror.Sedang, k);
                            if (e.id != null) { Catat(e); return e; }
                        }
                    }
                    if (_tFase >= _lamaFase) Ganti(Fase.Lega, Acak(MinLega, MaksLega));
                    return kosong;
                default:
                    if (_tFase >= _lamaFase) Ganti(Fase.Tenang, MinTenang);
                    return kosong;
            }
        }

        void Ganti(Fase f, float lama) { FaseKini = f; _tFase = 0f; _lamaFase = lama; _ringanFaseIni = 0; _tRingan = f == Fase.Meremang ? Acak(8f, 20f) : 0f; }

        bool Boleh(Konteks k) { return !k.tempur && _waktu - _terakhirKejadian >= JedaAntarKejadian; }
        bool BolehKeras() { return Setelan != Intensitas.Ringan && _waktu - _terakhirKeras >= JedaKerasAktif; }
        bool BolehSedang() { return _waktu - _terakhirSedang >= JedaSedang; }

        public bool Diblokir(Kejadian e)
        {
            float t;
            return !e.ulang && _terakhir.TryGetValue(e.id, out t) && _waktu - t < DetikPerHariGame * 2f; // 2 hari game = 48 menit nyata
        }

        Kejadian Pilih(TipeTeror tipe, Konteks k)
        {
            float total = 0f;
            for (int i = 0; i < _katalog.Count; i++) { var e = _katalog[i]; if (e.tipe == tipe && !Diblokir(e) && e.syarat(k)) total += _pernah.Contains(e.id) ? 1f : 2f; }
            if (total <= 0f) return new Kejadian();
            float r = (float)_rng.NextDouble() * total;
            for (int i = 0; i < _katalog.Count; i++)
            {
                var e = _katalog[i];
                if (e.tipe != tipe || Diblokir(e) || !e.syarat(k)) continue;
                r -= _pernah.Contains(e.id) ? 1f : 2f;
                if (r <= 0f) return e;
            }
            return new Kejadian();
        }

        void Catat(Kejadian e)
        {
            _terakhir[e.id] = _waktu; _pernah.Add(e.id); _terakhirKejadian = _waktu;
            if (e.tipe == TipeTeror.Keras) { _terakhirKeras = _waktu; JumlahKeras++; }
            else if (e.tipe == TipeTeror.Sedang) { _terakhirSedang = _waktu; JumlahSedang++; }
            else JumlahRingan++;
        }

        public bool PernahUlangDalam2Hari(string id, float sejakDetik) { float t; return _terakhir.TryGetValue(id, out t) && _waktu - t < sejakDetik; }
        public float Waktu { get { return _waktu; } }
    }
}
