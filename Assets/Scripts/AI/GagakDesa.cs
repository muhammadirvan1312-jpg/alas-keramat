using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Gagak hitam bertengger di bubungan atap Desa Mati. Siang/senja sesekali berkaok (rekaman CC0 OpenGameArt),
    // kabur beterbangan bila Andi mendekat, berputar di atas desa, lalu kembali bertengger saat Andi menjauh.
    // Satu pengelola untuk semua gagak (tanpa alokasi per frame); tidak ada jump scare (H16): hanya suasana.
    public class GagakDesa : MonoBehaviour
    {
        public const string KlipGagak = "rekaman_gagak";
        public const float JarakKaget = 11f, JarakKembali = 26f, JarakDengar = 50f, TinggiPutar = 16f, RadiusPutar = 14f;
        public const float LajuTerbang = 7.5f, DurasiKabur = 3.5f, JedaKaokMin = 9f, JedaKaokMaks = 26f, JamTidur = 19.5f, JamBangun = 5.5f;
        const float JedaCek = 0.25f, KepakHinggap = 0.45f, FrekKepak = 11f, SudutKepak = 55f, LajuKembali = 4.5f;

        public enum Keadaan { Hinggap = 0, Kabur = 1, Putar = 2, Kembali = 3 }

        public Transform[] Badan;
        public Transform[] SayapKiri;
        public Transform[] SayapKanan;
        public Vector3[] Tengger;
        public float[] ArahTengger;

        Keadaan[] _keadaan;
        float[] _waktuKeadaan, _kaokBerikut, _fase;
        Vector3[] _kecepatan;
        float _cekBerikut;
        Vector3 _posPemain;
        TimeOfDayClock _jam;
        ProceduralAudioSynth _audio;
        System.Random _acak = new System.Random(733);

        public int Jumlah { get { return Badan != null ? Badan.Length : 0; } }
        public Keadaan KeadaanGagak(int i) { return _keadaan != null && i >= 0 && i < _keadaan.Length ? _keadaan[i] : Keadaan.Hinggap; }

        // Murni: gagak berkaok hanya saat terjaga (05:30-19:30).
        public static bool Terjaga(float jam) { return jam >= JamBangun && jam < JamTidur; }

        // Murni: posisi orbit di atas desa pada sudut t.
        public static Vector3 PosisiPutar(Vector3 pusat, float t, int indeks)
        {
            float r = RadiusPutar * (0.75f + 0.08f * (indeks % 4));
            float a = t * (0.45f + 0.03f * (indeks % 3)) + indeks * 0.9f;
            return pusat + new Vector3(Mathf.Cos(a) * r, TinggiPutar + Mathf.Sin(a * 1.7f) * 2f, Mathf.Sin(a) * r);
        }

        void Start()
        {
            int n = Jumlah;
            if (n == 0 || Tengger == null || Tengger.Length < n) { enabled = false; Debug.Log("[AK][GagakDesa] Tidak ada gagak terpasang, dinonaktifkan."); return; }
            _keadaan = new Keadaan[n]; _waktuKeadaan = new float[n]; _kaokBerikut = new float[n]; _fase = new float[n]; _kecepatan = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                _fase[i] = i * 1.37f;
                _kaokBerikut[i] = Time.time + Acak(2f, JedaKaokMaks);
                if (Badan[i] != null) Badan[i].SetPositionAndRotation(Tengger[i], Quaternion.Euler(0f, ArahTengger != null && i < ArahTengger.Length ? ArahTengger[i] : 0f, 0f));
                LipatSayap(i, 1f);
            }
        }

        float Acak(float a, float b) { return a + (float)_acak.NextDouble() * (b - a); }

        Vector3 PusatDesa()
        {
            Vector3 p = Vector3.zero;
            for (int i = 0; i < Tengger.Length; i++) p += Tengger[i];
            return Tengger.Length > 0 ? p / Tengger.Length : transform.position;
        }

        void Update()
        {
            if (_keadaan == null) return;
            if (Time.time >= _cekBerikut)
            {
                _cekBerikut = Time.time + JedaCek;
                var cam = Camera.main;
                _posPemain = cam != null ? cam.transform.position : new Vector3(1e6f, 0f, 1e6f);
                if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
                if (_audio == null) _audio = GameServices.Get<ProceduralAudioSynth>();
            }
            float jam = _jam != null ? _jam.Jam : TimeOfDayClock.JamMulai;
            bool terjaga = Terjaga(jam);
            Vector3 pusat = PusatDesa();
            float dt = Time.deltaTime;

            for (int i = 0; i < _keadaan.Length; i++)
            {
                var b = Badan[i];
                if (b == null) continue;
                float jarak = (b.position - _posPemain).magnitude;
                float jarakTengger = (Tengger[i] - _posPemain).magnitude;
                switch (_keadaan[i])
                {
                    case Keadaan.Hinggap:
                        LipatSayap(i, 1f);
                        if (jarak < JarakKaget) { Ganti(i, Keadaan.Kabur); Kaok(i, 1.15f); break; }
                        if (terjaga && jarak < JarakDengar && Time.time >= _kaokBerikut[i])
                        {
                            _kaokBerikut[i] = Time.time + Acak(JedaKaokMin, JedaKaokMaks);
                            Kaok(i, Acak(0.9f, 1.08f));
                            // Mengangguk saat berkaok.
                            b.rotation = Quaternion.Euler(-18f, b.eulerAngles.y + Acak(-25f, 25f), 0f);
                        }
                        else b.rotation = Quaternion.Slerp(b.rotation, Quaternion.Euler(0f, b.eulerAngles.y, 0f), dt * 3f);
                        break;
                    case Keadaan.Kabur:
                    {
                        Vector3 menjauh = b.position - _posPemain; menjauh.y = 0f;
                        if (menjauh.sqrMagnitude < 0.01f) menjauh = Vector3.forward;
                        Vector3 arah = (menjauh.normalized + Vector3.up * 0.9f).normalized;
                        _kecepatan[i] = Vector3.Lerp(_kecepatan[i], arah * LajuTerbang, dt * 3f);
                        Terbang(i, dt);
                        if (Time.time - _waktuKeadaan[i] > DurasiKabur) Ganti(i, Keadaan.Putar);
                        break;
                    }
                    case Keadaan.Putar:
                    {
                        Vector3 target = PosisiPutar(pusat, Time.time, i);
                        _kecepatan[i] = Vector3.Lerp(_kecepatan[i], Vector3.ClampMagnitude(target - b.position, LajuTerbang), dt * 1.5f);
                        Terbang(i, dt);
                        if (jarakTengger > JarakKembali && Time.time - _waktuKeadaan[i] > DurasiKabur * 2f) Ganti(i, Keadaan.Kembali);
                        break;
                    }
                    case Keadaan.Kembali:
                    {
                        Vector3 ke = Tengger[i] - b.position;
                        if (jarakTengger < JarakKaget) { Ganti(i, Keadaan.Putar); break; }
                        if (ke.magnitude < 0.12f)
                        {
                            b.SetPositionAndRotation(Tengger[i], Quaternion.Euler(0f, ArahTengger != null && i < ArahTengger.Length ? ArahTengger[i] : b.eulerAngles.y, 0f));
                            _kecepatan[i] = Vector3.zero; Ganti(i, Keadaan.Hinggap); break;
                        }
                        _kecepatan[i] = Vector3.Lerp(_kecepatan[i], Vector3.ClampMagnitude(ke * 1.5f, LajuKembali), dt * 2.5f);
                        Terbang(i, dt);
                        break;
                    }
                }
            }
        }

        void Ganti(int i, Keadaan k) { _keadaan[i] = k; _waktuKeadaan[i] = Time.time; }

        void Terbang(int i, float dt)
        {
            var b = Badan[i];
            b.position += _kecepatan[i] * dt;
            if (_kecepatan[i].sqrMagnitude > 0.01f)
                b.rotation = Quaternion.Slerp(b.rotation, Quaternion.LookRotation(_kecepatan[i].normalized, Vector3.up), dt * 6f);
            _fase[i] += dt * FrekKepak;
            float kepak = Mathf.Sin(_fase[i]) * SudutKepak;
            if (SayapKiri[i] != null) { SayapKiri[i].localRotation = Quaternion.Euler(0f, 0f, kepak); SayapKiri[i].localScale = Vector3.one; }
            if (SayapKanan[i] != null) { SayapKanan[i].localRotation = Quaternion.Euler(0f, 0f, -kepak); SayapKanan[i].localScale = Vector3.one; }
        }

        void LipatSayap(int i, float lipat)
        {
            var s = new Vector3(Mathf.Lerp(1f, KepakHinggap, lipat), 1f, 1f);
            if (SayapKiri != null && i < SayapKiri.Length && SayapKiri[i] != null) { SayapKiri[i].localRotation = Quaternion.Euler(0f, 0f, -12f); SayapKiri[i].localScale = s; }
            if (SayapKanan != null && i < SayapKanan.Length && SayapKanan[i] != null) { SayapKanan[i].localRotation = Quaternion.Euler(0f, 0f, 12f); SayapKanan[i].localScale = s; }
        }

        void Kaok(int i, float pitch)
        {
            if (_audio == null) return;
            _audio.Play(KlipGagak, Badan[i].position, AudioCategory.Efek, 0.85f, pitch, 1, JarakDengar);
        }
    }
}
