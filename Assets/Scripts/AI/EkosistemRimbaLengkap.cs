using System;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Ekosistem satwa hidup lintas 4 kuadran pulau: kawanan rusa merumput/kabur, ayam kampung desa,
    // babi hutan mencari makan, dan burung rangkong/camar pantai yang terbang & menukik sesuai jam & cuaca.
    public class EkosistemRimbaLengkap : MonoBehaviour
    {
        public enum JenisSatwa { RusaMerumput, AyamKampung, BabiMencariMakan, RangkongKanopi, CamarPantai }

        [Serializable]
        public struct AnggotaSatwa
        {
            public Transform tr;
            public Transform kepala;
            public Transform sayapKiri;
            public Transform sayapKanan;
            public JenisSatwa jenis;
            public Vector3 rumah;
            public float radius;
            public float laju;
            public float fasa;
        }

        [SerializeField] AnggotaSatwa[] daftar = new AnggotaSatwa[0];

        Vector3[] _posKini;
        float[] _sudut;
        float[] _kaburT;
        Transform _kamera;
        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _cuaca;
        bool _siap;

        public int JumlahSatwa { get { return daftar != null ? daftar.Length : 0; } }

        public void Pasang(AnggotaSatwa[] d)
        {
            daftar = d ?? new AnggotaSatwa[0];
        }

        public static bool AktifPadaWaktu(JenisSatwa jenis, float jam, WeatherState cuaca)
        {
            bool badai = cuaca == WeatherState.HujanBadai;
            bool siang = jam >= 5.5f && jam < 18.2f;
            if (jenis == JenisSatwa.AyamKampung) return siang && !badai;
            if (jenis == JenisSatwa.CamarPantai || jenis == JenisSatwa.RangkongKanopi) return siang && !badai;
            if (jenis == JenisSatwa.BabiMencariMakan) return !badai;
            return true;
        }

        public static float SudutKepakSayap(float waktu, float fasa, bool melayang)
        {
            if (melayang) return Mathf.Sin(waktu * 1.4f + fasa) * 7f;
            return Mathf.Sin(waktu * 8.2f + fasa) * 32f;
        }

        void Awake()
        {
            int n = daftar.Length;
            _posKini = new Vector3[n];
            _sudut = new float[n];
            _kaburT = new float[n];
            for (int i = 0; i < n; i++)
            {
                _posKini[i] = daftar[i].tr != null ? daftar[i].tr.position : daftar[i].rumah;
                _sudut[i] = daftar[i].fasa;
            }
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

            for (int i = 0; i < daftar.Length; i++)
            {
                var a = daftar[i];
                if (a.tr == null) continue;
                Vector3 dCam = a.rumah - cPos;
                float distSq = dCam.x * dCam.x + dCam.z * dCam.z;
                bool aktif = distSq < 92f * 92f && AktifPadaWaktu(a.jenis, jam, cuaca);
                if (a.tr.gameObject.activeSelf != aktif) a.tr.gameObject.SetActive(aktif);
                if (!aktif) continue;

                if (a.jenis == JenisSatwa.RangkongKanopi || a.jenis == JenisSatwa.CamarPantai)
                {
                    _sudut[i] += a.laju * dt;
                    float ang = _sudut[i];
                    float rx = a.rumah.x + Mathf.Cos(ang) * a.radius;
                    float rz = a.rumah.z + Mathf.Sin(ang) * a.radius;
                    float g = WorldTerrainGenerator.SampleHeight(rx, rz);
                    float tinggi = (a.jenis == JenisSatwa.CamarPantai ? 11f : 19f) + Mathf.Sin(t * 0.9f + a.fasa) * 2.4f;
                    Vector3 np = new Vector3(rx, Mathf.Max(g + 7f, tinggi), rz);
                    Vector3 arah = np - a.tr.position;
                    a.tr.position = np;
                    if (arah.sqrMagnitude > 0.0001f)
                        a.tr.rotation = Quaternion.LookRotation(arah.normalized, Vector3.up) * Quaternion.Euler(0f, 0f, -16f);
                    bool melayang = Mathf.Sin(t * 0.55f + a.fasa) > 0.35f;
                    float kepak = SudutKepakSayap(t, a.fasa, melayang);
                    if (a.sayapKiri != null) a.sayapKiri.localRotation = Quaternion.Euler(0f, 0f, kepak);
                    if (a.sayapKanan != null) a.sayapKanan.localRotation = Quaternion.Euler(0f, 0f, -kepak);
                }
                else
                {
                    Vector3 dariPemain = _posKini[i] - cPos;
                    dariPemain.y = 0f;
                    float jarakP = dariPemain.magnitude;
                    if (jarakP < 11f && jarakP > 0.2f) _kaburT[i] = 2.6f;

                    Vector3 target;
                    float speed;
                    if (_kaburT[i] > 0f)
                    {
                        _kaburT[i] -= dt;
                        Vector3 lariDir = jarakP > 0.1f ? dariPemain / jarakP : Vector3.forward;
                        target = _posKini[i] + lariDir * 6.5f;
                        // Tetap di dalam sabuk 2.4x radius rumah agar tidak masuk laut.
                        Vector3 keRumah = target - a.rumah;
                        keRumah.y = 0f;
                        float batas = a.radius * 2.4f;
                        if (keRumah.magnitude > batas) target = a.rumah + keRumah.normalized * batas;
                        speed = a.jenis == JenisSatwa.RusaMerumput ? 5.6f : 3.2f;
                    }
                    else
                    {
                        _sudut[i] += a.laju * dt;
                        float ang = _sudut[i];
                        float r = a.radius * (0.55f + 0.45f * Mathf.Sin(ang * 0.6f + a.fasa));
                        target = new Vector3(a.rumah.x + Mathf.Cos(ang) * r, 0f, a.rumah.z + Mathf.Sin(ang) * r);
                        bool makan = Mathf.Sin(t * 0.8f + a.fasa) > 0.25f;
                        speed = makan ? 0.12f : (a.jenis == JenisSatwa.AyamKampung ? 1.15f : 1.65f);
                    }

                    Vector3 delta = target - _posKini[i];
                    delta.y = 0f;
                    if (delta.sqrMagnitude > 0.04f)
                    {
                        Vector3 langkah = delta.normalized * Mathf.Min(delta.magnitude, speed * dt);
                        _posKini[i] += langkah;
                        Quaternion rotTarget = Quaternion.LookRotation(delta.normalized, Vector3.up);
                        a.tr.rotation = Quaternion.Slerp(a.tr.rotation, rotTarget, dt * 5f);
                    }
                    float yTanah = Mathf.Max(0.4f, WorldTerrainGenerator.SampleHeight(_posKini[i].x, _posKini[i].z));
                    float lompat = _kaburT[i] > 0f ? Mathf.Abs(Mathf.Sin(t * 11f + a.fasa)) * 0.28f
                        : (speed > 0.5f ? Mathf.Abs(Mathf.Sin(t * 6.5f + a.fasa)) * 0.07f : 0f);
                    a.tr.position = new Vector3(_posKini[i].x, yTanah + lompat, _posKini[i].z);

                    if (a.kepala != null)
                    {
                        bool tunduk = _kaburT[i] <= 0f && speed < 0.35f;
                        float patuk = a.jenis == JenisSatwa.AyamKampung
                            ? (tunduk ? 34f + Mathf.Sin(t * 9f + a.fasa) * 18f : 4f)
                            : (tunduk ? 32f + Mathf.Sin(t * 2.2f + a.fasa) * 8f : -4f);
                        a.kepala.localRotation = Quaternion.Euler(patuk, Mathf.Sin(t * 1.1f + a.fasa) * 14f, 0f);
                    }
                }
            }
        }
    }
}
