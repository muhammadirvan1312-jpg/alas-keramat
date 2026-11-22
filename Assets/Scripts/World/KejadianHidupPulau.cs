using System;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.World
{
    // Kejadian alam hidup dinamis di seluruh pulau 4 km2:
    // - 2 kawanan burung migrasi formasi-V (24 ekor) melintasi langit fajar/siang/senja
    // - 36 partikel serbuk sari emas / kelopak bunga / bara abu terbang mengikuti bioma & cuaca
    // - 12 titik semak kaget: kawanan burung rimba melesat ke langit saat Andi melintas dekat
    public class KejadianHidupPulau : MonoBehaviour
    {
        [Serializable]
        public struct KawananMigrasi
        {
            public Transform induk;
            public Transform[] sayapKiri;
            public Transform[] sayapKanan;
            public Vector3 asal;
            public Vector3 tujuan;
            public float tinggi;
            public float laju;
            public float fasa;
        }

        [Serializable]
        public struct SemakKaget
        {
            public Transform tr;
            public Vector3 pusat;
            public float fasa;
        }

        [SerializeField] KawananMigrasi[] kawanan = new KawananMigrasi[0];
        [SerializeField] Transform[] serbukAlam = new Transform[0];
        [SerializeField] SemakKaget[] semakKaget = new SemakKaget[0];

        float[] _progresMigrasi;
        float[] _kagetT;
        float[] _jedaKaget;
        Vector3[] _posSerbuk;
        Transform _kamera;
        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _cuaca;
        bool _siap;

        public int JumlahElemenKejadian
        {
            get
            {
                int k = 0;
                if (kawanan != null) for (int i = 0; i < kawanan.Length; i++) k += kawanan[i].sayapKiri != null ? kawanan[i].sayapKiri.Length : 0;
                return k + (serbukAlam != null ? serbukAlam.Length : 0) + (semakKaget != null ? semakKaget.Length : 0);
            }
        }

        public void Pasang(KawananMigrasi[] k, Transform[] s, SemakKaget[] sm)
        {
            kawanan = k ?? new KawananMigrasi[0];
            serbukAlam = s ?? new Transform[0];
            semakKaget = sm ?? new SemakKaget[0];
        }

        public static bool WaktuMigrasiAktif(float jam, WeatherState cuaca)
        {
            if (cuaca == WeatherState.HujanBadai) return false;
            return jam >= 5.2f && jam <= 18.8f;
        }

        public static float TinggiKagetBurung(float progres01)
        {
            return Mathf.Clamp01(progres01) * 14.5f;
        }

        void Awake()
        {
            _progresMigrasi = new float[kawanan.Length];
            for (int i = 0; i < kawanan.Length; i++) _progresMigrasi[i] = kawanan[i].fasa;
            _kagetT = new float[semakKaget.Length];
            _jedaKaget = new float[semakKaget.Length];
            _posSerbuk = new Vector3[serbukAlam.Length];
            _siap = true;
        }

        void Update()
        {
            if (!_siap || !Application.isPlaying) return;
            if (_kamera == null) { var c = Camera.main; if (c != null) _kamera = c.transform; }
            if (_kamera == null) return;
            Vector3 cPos = _kamera.position;
            if (WorldTerrainGenerator.DiBawahTanah(cPos.y)) return;

            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            if (_cuaca == null) _cuaca = GameServices.Get<URPAtmosphereAndWeather>();
            float jam = _jam != null ? _jam.Jam : 10f;
            WeatherState cuaca = _cuaca != null ? _cuaca.Cuaca : WeatherState.Cerah;
            float dt = Time.deltaTime, t = Time.time;

            // 1. Kawanan burung migrasi formasi-V melintasi langit pulau.
            bool migrasiOn = WaktuMigrasiAktif(jam, cuaca);
            for (int i = 0; i < kawanan.Length; i++)
            {
                var k = kawanan[i];
                if (k.induk == null) continue;
                if (k.induk.gameObject.activeSelf != migrasiOn) k.induk.gameObject.SetActive(migrasiOn);
                if (!migrasiOn) continue;

                _progresMigrasi[i] = Mathf.Repeat(_progresMigrasi[i] + dt * k.laju, 1f);
                Vector3 pos = Vector3.Lerp(k.asal, k.tujuan, _progresMigrasi[i]);
                pos.y = k.tinggi + Mathf.Sin(t * 0.7f + i * 2.1f) * 3.5f;
                k.induk.position = pos;
                Vector3 arah = k.tujuan - k.asal; arah.y = 0f;
                if (arah.sqrMagnitude > 0.01f) k.induk.rotation = Quaternion.LookRotation(arah.normalized, Vector3.up);

                if (k.sayapKiri != null && k.sayapKanan != null)
                {
                    int nB = Mathf.Min(k.sayapKiri.Length, k.sayapKanan.Length);
                    for (int b = 0; b < nB; b++)
                    {
                        float kepak = Mathf.Sin(t * 7.8f + b * 0.65f + i) * 30f;
                        if (k.sayapKiri[b] != null) k.sayapKiri[b].localRotation = Quaternion.Euler(0f, 0f, kepak);
                        if (k.sayapKanan[b] != null) k.sayapKanan[b].localRotation = Quaternion.Euler(0f, 0f, -kepak);
                    }
                }
            }

            // 2. Serbuk sari emas / kelopak / abu ringan melayang di sekitar pemain.
            float angin = cuaca == WeatherState.HujanBadai ? 2.6f : cuaca == WeatherState.HujanLebat ? 1.7f : 0.85f;
            for (int i = 0; i < serbukAlam.Length; i++)
            {
                var tr = serbukAlam[i];
                if (tr == null) continue;
                Vector3 p = _posSerbuk[i];
                if (p == Vector3.zero || (p - cPos).sqrMagnitude > 24f * 24f)
                {
                    float a = i * 1.13f + t * 0.15f;
                    float r = 2.5f + (i % 9) * 2.1f;
                    float gx = cPos.x + Mathf.Cos(a) * r, gz = cPos.z + Mathf.Sin(a) * r;
                    float gy = WorldTerrainGenerator.SampleHeight(gx, gz) + 0.8f + (i % 5) * 0.95f;
                    p = new Vector3(gx, gy, gz);
                    _posSerbuk[i] = p;
                }
                p.x += Mathf.Sin(t * 0.6f + i) * 0.35f * angin * dt;
                p.z += Mathf.Cos(t * 0.5f + i * 1.4f) * 0.35f * angin * dt;
                p.y += Mathf.Sin(t * 1.2f + i * 0.9f) * 0.18f * dt;
                _posSerbuk[i] = p;
                tr.position = p;
                tr.rotation = Quaternion.Euler(t * 25f + i * 17f, t * 40f + i * 29f, 0f);
            }

            // 3. Burung kaget melesat keluar dari semak rimba saat Andi lewat dekat (< 11 m).
            for (int i = 0; i < semakKaget.Length; i++)
            {
                var sm = semakKaget[i];
                if (sm.tr == null) continue;
                if (_jedaKaget[i] > 0f) _jedaKaget[i] -= dt;
                Vector3 d = sm.pusat - cPos;
                float dSq = d.x * d.x + d.z * d.z;
                if (_kagetT[i] <= 0f && _jedaKaget[i] <= 0f && dSq < 11f * 11f)
                {
                    _kagetT[i] = 3.2f;
                    _jedaKaget[i] = 28f;
                }

                if (_kagetT[i] > 0f)
                {
                    _kagetT[i] -= dt;
                    float p01 = 1f - Mathf.Clamp01(_kagetT[i] / 3.2f);
                    if (!sm.tr.gameObject.activeSelf) sm.tr.gameObject.SetActive(true);
                    float ang = sm.fasa + p01 * 2.4f;
                    Vector3 np = sm.pusat + new Vector3(Mathf.Cos(ang) * p01 * 18f, TinggiKagetBurung(p01), Mathf.Sin(ang) * p01 * 18f);
                    Vector3 arah = np - sm.tr.position;
                    sm.tr.position = np;
                    if (arah.sqrMagnitude > 0.001f) sm.tr.rotation = Quaternion.LookRotation(arah.normalized, Vector3.up);
                }
                else if (sm.tr.gameObject.activeSelf)
                {
                    sm.tr.position = sm.pusat;
                    sm.tr.gameObject.SetActive(false);
                }
            }
        }
    }
}
