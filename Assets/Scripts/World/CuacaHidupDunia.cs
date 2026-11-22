using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.World
{
    // Reaktivitas cuaca & atmosfer hidup lintas pulau:
    // - Kilatan petir langit & guruh bertunda saat HujanBadai
    // - Penguatan skala kabut lembah & embun air terjun otomatis saat hujan lebat/badai
    public class CuacaHidupDunia : MonoBehaviour
    {
        [SerializeField] Light cahayaPetir;
        [SerializeField] Transform[] kabutDinamis = new Transform[0];

        Vector3[] _skalaAwal;
        URPAtmosphereAndWeather _cuaca;
        ProceduralAudioSynth _audio;
        Transform _kamera;
        float _petirBerikut = 12f, _sisaKilat, _tundaGuruh = -1f;
        bool _siap;

        public int JumlahKabutDinamis { get { return kabutDinamis != null ? kabutDinamis.Length : 0; } }

        public void Pasang(Light petir, Transform[] kabut)
        {
            cahayaPetir = petir;
            kabutDinamis = kabut ?? new Transform[0];
        }

        public static float PengaliKabutCuaca(WeatherState w)
        {
            if (w == WeatherState.HujanBadai) return 1.42f;
            if (w == WeatherState.HujanLebat) return 1.24f;
            if (w == WeatherState.Gerimis) return 1.12f;
            return 1.0f;
        }

        void Awake()
        {
            _skalaAwal = new Vector3[kabutDinamis.Length];
            for (int i = 0; i < kabutDinamis.Length; i++)
                if (kabutDinamis[i] != null) _skalaAwal[i] = kabutDinamis[i].localScale;
            _siap = true;
        }

        void Update()
        {
            if (!_siap || !Application.isPlaying) return;
            if (_kamera == null) { var c = Camera.main; if (c != null) _kamera = c.transform; }
            if (_kamera == null || WorldTerrainGenerator.DiBawahTanah(_kamera.position.y)) return;

            if (_cuaca == null) _cuaca = GameServices.Get<URPAtmosphereAndWeather>();
            WeatherState cuaca = _cuaca != null ? _cuaca.Cuaca : WeatherState.Cerah;
            float dt = Time.deltaTime;

            // 1. Kilat & guruh realistis saat HujanBadai.
            if (cuaca == WeatherState.HujanBadai)
            {
                _petirBerikut -= dt;
                if (_petirBerikut <= 0f)
                {
                    _petirBerikut = Random.Range(11f, 24f);
                    _sisaKilat = 0.24f;
                    _tundaGuruh = Random.Range(0.7f, 2.2f);
                    if (cahayaPetir != null)
                        cahayaPetir.transform.position = _kamera.position + new Vector3(Random.Range(-45f, 45f), 38f, Random.Range(-45f, 45f));
                }
            }

            if (cahayaPetir != null)
            {
                if (_sisaKilat > 0f)
                {
                    _sisaKilat -= dt;
                    if (!cahayaPetir.enabled) cahayaPetir.enabled = true;
                    cahayaPetir.intensity = Mathf.Clamp01(_sisaKilat / 0.24f) * 6.5f;
                }
                else if (cahayaPetir.enabled)
                {
                    cahayaPetir.enabled = false;
                }
            }

            if (_tundaGuruh > 0f)
            {
                _tundaGuruh -= dt;
                if (_tundaGuruh <= 0f)
                {
                    if (_audio == null) _audio = GameServices.Get<ProceduralAudioSynth>();
                    if (_audio != null) _audio.Play(ProceduralAudioSynth.KlipGuruh, null, AudioCategory.Ambien, 0.58f, Random.Range(0.88f, 1.08f), 2);
                }
            }

            // 2. Kabut lembah & embun membesar lembut mengikuti intensitas hujan.
            float mult = PengaliKabutCuaca(cuaca);
            for (int i = 0; i < kabutDinamis.Length; i++)
            {
                var tr = kabutDinamis[i];
                if (tr == null) continue;
                tr.localScale = Vector3.Lerp(tr.localScale, _skalaAwal[i] * mult, dt * 0.6f);
            }
        }
    }
}
