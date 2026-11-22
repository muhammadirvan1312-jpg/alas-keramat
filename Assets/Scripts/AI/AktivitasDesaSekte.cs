using System;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Menghidupkan desa, pasar, dan kampung dengan rutinitas nyata:
    // 6 warga/abdi sekte berjalan membawa obor/kayu antar-rumah, 4 anjing kampung berkeliaran mengendus,
    // serta 6 gagak/rangkong bertengger di atap yang serentak terbang melingkar saat Andi mendekat.
    public class AktivitasDesaSekte : MonoBehaviour
    {
        public enum TipeAgen { KurirSekte, AnjingKampung, GagakAtap }

        [Serializable]
        public struct AgenHidup
        {
            public Transform tr;
            public Transform kepala;
            public Transform sayapKiri;
            public Transform sayapKanan;
            public Light obor;
            public TipeAgen tipe;
            public Vector3 titikA;
            public Vector3 titikB;
            public float laju;
            public float fasa;
        }

        [SerializeField] AgenHidup[] agen = new AgenHidup[0];

        float[] _tJalan;
        float[] _terbangT;
        Vector3[] _posKini;
        Transform _kamera;
        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _cuaca;
        bool _siap;

        public int JumlahAgen { get { return agen != null ? agen.Length : 0; } }

        public void Pasang(AgenHidup[] daftar)
        {
            agen = daftar ?? new AgenHidup[0];
        }

        public static bool HarusTerbangKaget(float jarakSq, float ambangMeter)
        {
            return jarakSq > 0.01f && jarakSq < ambangMeter * ambangMeter;
        }

        public static bool OborHarusNyala(float jam, WeatherState cuaca)
        {
            return jam >= 17.4f || jam <= 5.8f || cuaca == WeatherState.HujanBadai;
        }

        void Awake()
        {
            int n = agen.Length;
            _tJalan = new float[n];
            _terbangT = new float[n];
            _posKini = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                _tJalan[i] = agen[i].fasa;
                _posKini[i] = agen[i].tr != null ? agen[i].tr.position : agen[i].titikA;
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
            bool nyalaObor = OborHarusNyala(jam, cuaca);
            float dt = Time.deltaTime, t = Time.time;

            for (int i = 0; i < agen.Length; i++)
            {
                var a = agen[i];
                if (a.tr == null) continue;
                Vector3 dCam = a.titikA - cPos;
                float distSq = dCam.x * dCam.x + dCam.z * dCam.z;
                bool dekat = distSq < 88f * 88f;
                if (a.tr.gameObject.activeSelf != dekat) a.tr.gameObject.SetActive(dekat);
                if (!dekat) continue;

                if (a.tipe == TipeAgen.GagakAtap)
                {
                    Vector3 kePemain = _posKini[i] - cPos;
                    float jSq = kePemain.x * kePemain.x + kePemain.z * kePemain.z;
                    if (HarusTerbangKaget(jSq, 14f)) _terbangT[i] = 5.5f;

                    if (_terbangT[i] > 0f)
                    {
                        _terbangT[i] -= dt;
                        _tJalan[i] += dt * (1.85f + (i % 3) * 0.25f);
                        float ang = _tJalan[i];
                        float r = 7.5f + (i % 3) * 2.5f;
                        float tinggi = 6.5f + Mathf.Sin(t * 2.2f + a.fasa) * 1.4f;
                        Vector3 target = a.titikA + new Vector3(Mathf.Cos(ang) * r, tinggi, Mathf.Sin(ang) * r);
                        Vector3 arah = target - _posKini[i];
                        _posKini[i] = Vector3.Lerp(_posKini[i], target, dt * 3.8f);
                        a.tr.position = _posKini[i];
                        if (arah.sqrMagnitude > 0.001f)
                            a.tr.rotation = Quaternion.Slerp(a.tr.rotation, Quaternion.LookRotation(arah.normalized, Vector3.up), dt * 6f);
                        float kepak = Mathf.Sin(t * 14f + a.fasa) * 38f;
                        if (a.sayapKiri != null) a.sayapKiri.localRotation = Quaternion.Euler(0f, 0f, kepak);
                        if (a.sayapKanan != null) a.sayapKanan.localRotation = Quaternion.Euler(0f, 0f, -kepak);
                    }
                    else
                    {
                        _posKini[i] = Vector3.Lerp(_posKini[i], a.titikA, dt * 2.8f);
                        a.tr.position = _posKini[i];
                        if (a.sayapKiri != null) a.sayapKiri.localRotation = Quaternion.Euler(0f, 0f, -52f);
                        if (a.sayapKanan != null) a.sayapKanan.localRotation = Quaternion.Euler(0f, 0f, 52f);
                        if (a.kepala != null)
                            a.kepala.localRotation = Quaternion.Euler(Mathf.Sin(t * 2.6f + a.fasa) * 14f, Mathf.Sin(t * 1.3f + a.fasa) * 28f, 0f);
                    }
                }
                else
                {
                    // Kurir Sekte & Anjing Kampung berjalan bolak-balik antar dua titik dengan jeda berhenti alami.
                    _tJalan[i] += dt * a.laju;
                    float siklus = Mathf.Repeat(_tJalan[i], 2.4f);
                    float u;
                    bool berhenti = false;
                    if (siklus < 1.0f) u = siklus;
                    else if (siklus < 1.2f) { u = 1.0f; berhenti = true; }
                    else if (siklus < 2.2f) u = 1.0f - (siklus - 1.2f);
                    else { u = 0.0f; berhenti = true; }

                    Vector3 np = Vector3.Lerp(a.titikA, a.titikB, u);
                    float yTanah = Mathf.Max(0.45f, WorldTerrainGenerator.SampleHeight(np.x, np.z));
                    float ayunLangkah = berhenti ? 0f : Mathf.Abs(Mathf.Sin(t * (a.tipe == TipeAgen.AnjingKampung ? 9.5f : 6.2f) + a.fasa)) * 0.09f;
                    np.y = yTanah + ayunLangkah;

                    Vector3 delta = np - _posKini[i];
                    delta.y = 0f;
                    if (delta.sqrMagnitude > 0.0004f)
                        a.tr.rotation = Quaternion.Slerp(a.tr.rotation, Quaternion.LookRotation(delta.normalized, Vector3.up), dt * 6f);
                    _posKini[i] = np;
                    a.tr.position = np;

                    if (a.kepala != null)
                    {
                        float angguk = berhenti ? (a.tipe == TipeAgen.AnjingKampung ? 24f + Mathf.Sin(t * 6f + a.fasa) * 10f : Mathf.Sin(t * 1.5f + a.fasa) * 8f) : 0f;
                        a.kepala.localRotation = Quaternion.Euler(angguk, Mathf.Sin(t * 1.4f + a.fasa) * 16f, 0f);
                    }
                    if (a.obor != null)
                    {
                        bool on = nyalaObor && distSq < 68f * 68f;
                        if (a.obor.enabled != on) a.obor.enabled = on;
                        if (on) a.obor.intensity = 2.0f + Mathf.Sin(t * 9.3f + a.fasa) * 0.35f;
                    }
                }
            }
        }
    }
}
