using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Menghidupkan bahasa tubuh alami karakter utama (Deden, Ningsih, Ki Rogo) dan figur sekte desa:
    // napas dada lembut, tatapan rasa ingin tahu ke Andi/api unggun, reaksi dingin hujan/malam, & gerak ritual.
    public class AktivitasHidupKarakter : MonoBehaviour
    {
        public enum Rutinitas { PenyintasApi, PenjagaTenang, TetuaRitual, PemujaSekte }

        [SerializeField] Rutinitas rutinitas = Rutinitas.PenyintasApi;
        [SerializeField] Vector3 titikFokusApi;
        [SerializeField] bool punyaFokusApi;
        [SerializeField] float fasaAcak;

        Transform _dada, _kepala, _lenganKiri, _lenganKanan, _kamera;
        Quaternion _rotDada0, _rotKepala0, _rotLenganKiri0, _rotLenganKanan0;
        bool _siap, _punyaPenggerak;
        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _cuaca;

        public Rutinitas RutinitasAktif { get { return rutinitas; } }

        public void Pasang(Rutinitas r, Vector3 fokusApi, bool adaApi, float fasa)
        {
            rutinitas = r;
            titikFokusApi = fokusApi;
            punyaFokusApi = adaApi;
            fasaAcak = fasa;
        }

        public static float FaktorDingin(float jam, WeatherState cuaca)
        {
            float malam = (jam >= 18.0f || jam <= 5.5f) ? 0.65f : 0f;
            float hujan = cuaca == WeatherState.HujanBadai ? 0.9f : cuaca == WeatherState.HujanLebat ? 0.7f : cuaca == WeatherState.Gerimis ? 0.35f : 0f;
            return Mathf.Clamp01(Mathf.Max(malam, hujan));
        }

        public static float SudutNapas(float waktu, float fasa, float dingin)
        {
            float laju = 1.65f + dingin * 0.55f;
            float amp = 1.8f + dingin * 1.1f;
            return Mathf.Sin(waktu * laju + fasa) * amp;
        }

        void Start()
        {
            CariTulang();
        }

        void CariTulang()
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                string n = t.name;
                if (_dada == null && (n == "Spine2" || n == "Chest" || n.EndsWith(":Spine2") || n == "Tubuh")) _dada = t;
                if (_kepala == null && (n == "Head" || n.EndsWith(":Head") || n == "Kepala")) _kepala = t;
                if (_lenganKiri == null && (n == "LeftArm" || n == "LeftUpperArm" || n.EndsWith(":LeftArm"))) _lenganKiri = t;
                if (_lenganKanan == null && (n == "RightArm" || n == "RightUpperArm" || n.EndsWith(":RightArm"))) _lenganKanan = t;
            }
            if (_dada != null) _rotDada0 = _dada.localRotation;
            if (_kepala != null) _rotKepala0 = _kepala.localRotation;
            if (_lenganKiri != null) _rotLenganKiri0 = _lenganKiri.localRotation;
            if (_lenganKanan != null) _rotLenganKanan0 = _lenganKanan.localRotation;
            _punyaPenggerak = GetComponentInChildren<ProceduralAnimator>(true) != null;
            _siap = _dada != null || _kepala != null;
        }

        void LateUpdate()
        {
            if (!_siap || !Application.isPlaying) return;
            if (_kamera == null) { var c = Camera.main; if (c != null) _kamera = c.transform; }
            if (_kamera != null && (_kamera.position - transform.position).sqrMagnitude > 48f * 48f) return;

            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            if (_cuaca == null) _cuaca = GameServices.Get<URPAtmosphereAndWeather>();
            float jam = _jam != null ? _jam.Jam : 10f;
            WeatherState cuaca = _cuaca != null ? _cuaca.Cuaca : WeatherState.Cerah;
            float dingin = FaktorDingin(jam, cuaca);
            float t = Time.time;

            // Gerak napas & ayun tubuh lembut (atau ayunan ritual khusyuk untuk pemuja sekte).
            float napas = SudutNapas(t, fasaAcak, dingin);
            if (_dada != null)
            {
                Quaternion basisDada = _punyaPenggerak ? _dada.localRotation : _rotDada0;
                if (rutinitas == Rutinitas.PemujaSekte)
                {
                    float ayun = Mathf.Sin(t * 1.35f + fasaAcak) * 7.5f;
                    float runduk = 6f + Mathf.Sin(t * 2.1f + fasaAcak) * 3.5f;
                    _dada.localRotation = basisDada * Quaternion.Euler(runduk, 0f, ayun);
                }
                else
                {
                    float goyang = Mathf.Sin(t * 0.72f + fasaAcak) * 1.4f;
                    _dada.localRotation = basisDada * Quaternion.Euler(napas * 0.28f, goyang * 0.2f, goyang * 0.15f);
                }
            }

            // Tatapan alami ke pemain dekat atau menghangatkan diri menatap api unggun saat malam/hujan.
            if (_kepala != null)
            {
                Quaternion basisKep = _punyaPenggerak ? _kepala.localRotation : _rotKepala0;
                float yaw = Mathf.Sin(t * 0.55f + fasaAcak) * 9f;
                float pitch = Mathf.Sin(t * 0.85f + fasaAcak * 1.7f) * 3.5f;
                if (_kamera != null)
                {
                    Vector3 keKam = _kamera.position - _kepala.position;
                    if (keKam.sqrMagnitude > 0.36f && keKam.sqrMagnitude < 144f)
                    {
                        Vector3 loc = transform.InverseTransformDirection(keKam.normalized);
                        if (loc.z > 0.15f)
                        {
                            yaw = Mathf.Clamp(Mathf.Atan2(loc.x, loc.z) * Mathf.Rad2Deg, -34f, 34f);
                            pitch = Mathf.Clamp(-loc.y * 24f, -14f, 14f);
                        }
                    }
                    else if (punyaFokusApi && dingin > 0.3f)
                    {
                        Vector3 keApi = transform.InverseTransformDirection((titikFokusApi - _kepala.position).normalized);
                        yaw = Mathf.Clamp(Mathf.Atan2(keApi.x, keApi.z) * Mathf.Rad2Deg, -26f, 26f);
                        pitch = 8f;
                    }
                }
                float skala = _punyaPenggerak ? 0.25f : 0.75f;
                _kepala.localRotation = basisKep * Quaternion.Euler(pitch * skala, yaw * skala, 0f);
            }

            // Lengan sedikit terangkat/bergetar ringan saat hujan deras atau ritual.
            if (rutinitas == Rutinitas.PemujaSekte && _lenganKiri != null && _lenganKanan != null)
            {
                float puja = Mathf.Sin(t * 1.8f + fasaAcak) * 14f;
                _lenganKiri.localRotation = (_punyaPenggerak ? _lenganKiri.localRotation : _rotLenganKiri0) * Quaternion.Euler(-16f + puja, 0f, 0f);
                _lenganKanan.localRotation = (_punyaPenggerak ? _lenganKanan.localRotation : _rotLenganKanan0) * Quaternion.Euler(-16f - puja, 0f, 0f);
            }
            else if (dingin > 0.55f && _lenganKiri != null && _lenganKanan != null)
            {
                float getar = Mathf.Sin(t * 14f + fasaAcak) * 0.9f * dingin;
                _lenganKiri.localRotation = (_punyaPenggerak ? _lenganKiri.localRotation : _rotLenganKiri0) * Quaternion.Euler(getar, 0f, 0f);
                _lenganKanan.localRotation = (_punyaPenggerak ? _lenganKanan.localRotation : _rotLenganKanan0) * Quaternion.Euler(-getar, 0f, 0f);
            }
        }
    }
}
