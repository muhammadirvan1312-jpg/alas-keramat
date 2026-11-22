using System;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.World
{
    // Menghidupkan elemen mekanis & sinyal di seluruh landmark pulau:
    // roda gila Pabrik Gula, kincir air Perkebunan, baling-baling angin Menara Api/Gunung Kapur,
    // parabola Kamp Peneliti, lampu suar berkedip (Heli & Menara Radio), serta sangkar/ikan kering bergoyang angin.
    public class MesinDanKincirHidup : MonoBehaviour
    {
        [Serializable]
        public struct RotorHidup
        {
            public Transform tr;
            public Vector3 sumbu;
            public float lajuDerajat;
            public bool ikutAngin;
        }

        [Serializable]
        public struct SuarKedip
        {
            public Renderer rend;
            public Light lampu;
            public Color warnaNyala;
            public float frekuensi;
            public float fasa;
        }

        [SerializeField] RotorHidup[] rotor = new RotorHidup[0];
        [SerializeField] SuarKedip[] suar = new SuarKedip[0];
        [SerializeField] Transform[] gantungAngin = new Transform[0];

        Quaternion[] _rotGantung0;
        MaterialPropertyBlock _mpb;
        Transform _kamera;
        URPAtmosphereAndWeather _cuaca;
        bool _siap;

        public int JumlahMekanis
        {
            get
            {
                return (rotor != null ? rotor.Length : 0)
                    + (suar != null ? suar.Length : 0)
                    + (gantungAngin != null ? gantungAngin.Length : 0);
            }
        }

        public void Pasang(RotorHidup[] r, SuarKedip[] s, Transform[] g)
        {
            rotor = r ?? new RotorHidup[0];
            suar = s ?? new SuarKedip[0];
            gantungAngin = g ?? new Transform[0];
        }

        public static float PengaliAnginCuaca(WeatherState w)
        {
            if (w == WeatherState.HujanBadai) return 2.8f;
            if (w == WeatherState.KabutDarah || w == WeatherState.KabutAsap) return 1.75f;
            if (w == WeatherState.Gerimis) return 1.45f;
            return 1.0f;
        }

        public static float DenyutSuar01(float waktu, float frekuensi, float fasa)
        {
            float s = Mathf.Sin(waktu * frekuensi * Mathf.PI * 2f + fasa);
            return s > 0.35f ? 1f : 0.12f;
        }

        void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            _rotGantung0 = new Quaternion[gantungAngin.Length];
            for (int i = 0; i < gantungAngin.Length; i++)
                if (gantungAngin[i] != null) _rotGantung0[i] = gantungAngin[i].localRotation;
            _siap = true;
        }

        void Update()
        {
            if (!_siap || !Application.isPlaying) return;
            if (_kamera == null) { var c = Camera.main; if (c != null) _kamera = c.transform; }
            if (_kamera == null) return;
            Vector3 cPos = _kamera.position;
            if (WorldTerrainGenerator.DiBawahTanah(cPos.y)) return;

            if (_cuaca == null) _cuaca = GameServices.Get<URPAtmosphereAndWeather>();
            float angin = PengaliAnginCuaca(_cuaca != null ? _cuaca.Cuaca : WeatherState.Cerah);
            float dt = Time.deltaTime, t = Time.time;

            for (int i = 0; i < rotor.Length; i++)
            {
                var r = rotor[i];
                if (r.tr == null) continue;
                if ((r.tr.position - cPos).sqrMagnitude > 140f * 140f) continue;
                float mult = r.ikutAngin ? angin : 1f;
                r.tr.Rotate(r.sumbu, r.lajuDerajat * mult * dt, Space.Self);
            }

            for (int i = 0; i < suar.Length; i++)
            {
                var s = suar[i];
                if (s.rend == null && s.lampu == null) continue;
                Transform tr = s.rend != null ? s.rend.transform : s.lampu.transform;
                bool dekat = (tr.position - cPos).sqrMagnitude < 160f * 160f;
                float p = dekat ? DenyutSuar01(t, s.frekuensi, s.fasa) : 0f;
                if (s.rend != null && dekat)
                {
                    _mpb.SetColor("_EmissionColor", s.warnaNyala * (3.2f * p));
                    s.rend.SetPropertyBlock(_mpb);
                }
                if (s.lampu != null)
                {
                    bool on = dekat && p > 0.5f;
                    if (s.lampu.enabled != on) s.lampu.enabled = on;
                }
            }

            for (int i = 0; i < gantungAngin.Length; i++)
            {
                var tr = gantungAngin[i];
                if (tr == null) continue;
                if ((tr.position - cPos).sqrMagnitude > 85f * 85f) continue;
                float ayunX = Mathf.Sin(t * (1.7f * angin) + i * 1.4f) * (6.5f * angin);
                float ayunZ = Mathf.Cos(t * (1.3f * angin) + i * 1.1f) * (5.0f * angin);
                tr.localRotation = _rotGantung0[i] * Quaternion.Euler(ayunX, 0f, ayunZ);
            }
        }
    }
}
