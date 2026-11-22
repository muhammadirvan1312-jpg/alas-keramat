using System;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Dinamika ekosistem kawanan & rantai makanan alami 24 jam di 4 kuadran pulau:
    // - 18 Herbivora (Rusa & Banteng Rimba) bermigrasi harian antara sumber air (pagi), hutan teduh (siang) & padang rumput (sore)
    // - 6 Predator Rimba (Harimau / Macan Kumbang) mengintai tepi kawanan saat senja/malam -> membuat herbivora kabur berpencar!
    // - 6 Biawak / Buaya berjemur di batu sungai siang hari & langsung meluncur masuk ke air saat Andi mendekat atau hujan.
    public class DinamikaKawananAlam : MonoBehaviour
    {
        [Serializable]
        public struct HerbivoraMigrasi
        {
            public Transform tr;
            public Transform kepala;
            public Vector3 titikAirPagi;
            public Vector3 titikHutanSiang;
            public Vector3 titikPadangSore;
            public Vector3 offsetKawanan;
            public float laju;
            public float fasa;
        }

        [Serializable]
        public struct PredatorPengintai
        {
            public Transform tr;
            public Vector3 pusatPatroli;
            public float radius;
            public float laju;
            public float fasa;
        }

        [Serializable]
        public struct ReptilBerjemur
        {
            public Transform tr;
            public Vector3 batuJemur;
            public Vector3 titikAir;
            public float fasa;
        }

        [SerializeField] HerbivoraMigrasi[] herbivora = new HerbivoraMigrasi[0];
        [SerializeField] PredatorPengintai[] predator = new PredatorPengintai[0];
        [SerializeField] ReptilBerjemur[] reptil = new ReptilBerjemur[0];

        Vector3[] _posHerb;
        float[] _panikT;
        float[] _tPred;
        float[] _masukAir01;
        Transform _kamera;
        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _cuaca;
        bool _siap;

        public int JumlahSatwaDinamika
        {
            get
            {
                return (herbivora != null ? herbivora.Length : 0)
                    + (predator != null ? predator.Length : 0)
                    + (reptil != null ? reptil.Length : 0);
            }
        }

        public void Pasang(HerbivoraMigrasi[] h, PredatorPengintai[] p, ReptilBerjemur[] r)
        {
            herbivora = h ?? new HerbivoraMigrasi[0];
            predator = p ?? new PredatorPengintai[0];
            reptil = r ?? new ReptilBerjemur[0];
        }

        // 0 = minum di sungai pagi (05:30-09:30), 1 = berteduh hutan siang (09:30-16:00), 2 = merumput padang sore/malam.
        public static int FaseMigrasiHarian(float jam)
        {
            if (jam >= 5.5f && jam < 9.5f) return 0;
            if (jam >= 9.5f && jam < 16.0f) return 1;
            return 2;
        }

        public static bool ReptilHarusMasukAir(float jarakPemainSq, WeatherState cuaca, float jam)
        {
            bool hujan = cuaca == WeatherState.Gerimis || cuaca == WeatherState.HujanBadai || cuaca == WeatherState.KabutDarah;
            bool malam = jam >= 18.0f || jam < 7.0f;
            return hujan || malam || jarakPemainSq < 9.5f * 9.5f;
        }

        void Awake()
        {
            _posHerb = new Vector3[herbivora.Length];
            _panikT = new float[herbivora.Length];
            for (int i = 0; i < herbivora.Length; i++)
                _posHerb[i] = herbivora[i].tr != null ? herbivora[i].tr.position : herbivora[i].titikHutanSiang;

            _tPred = new float[predator.Length];
            for (int i = 0; i < predator.Length; i++) _tPred[i] = predator[i].fasa;

            _masukAir01 = new float[reptil.Length];
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
            int fase = FaseMigrasiHarian(jam);
            float dt = Time.deltaTime, t = Time.time;

            // 1. Update predator pengintai terlebih dahulu agar bisa memicu kepanikan herbivora.
            bool predatorAktif = jam >= 16.5f || jam <= 6.8f;
            for (int i = 0; i < predator.Length; i++)
            {
                var p = predator[i];
                if (p.tr == null) continue;
                bool dekat = predatorAktif && (p.pusatPatroli - cPos).sqrMagnitude < 120f * 120f;
                if (p.tr.gameObject.activeSelf != dekat) p.tr.gameObject.SetActive(dekat);
                if (!dekat) continue;

                _tPred[i] += p.laju * dt;
                float ang = _tPred[i];
                float px = p.pusatPatroli.x + Mathf.Cos(ang) * p.radius;
                float pz = p.pusatPatroli.z + Mathf.Sin(ang * 0.85f) * p.radius;
                float py = Mathf.Max(0.5f, WorldTerrainGenerator.SampleHeight(px, pz));
                Vector3 np = new Vector3(px, py, pz);
                Vector3 arah = np - p.tr.position; arah.y = 0f;
                p.tr.position = np;
                if (arah.sqrMagnitude > 0.0005f)
                    p.tr.rotation = Quaternion.Slerp(p.tr.rotation, Quaternion.LookRotation(arah.normalized, Vector3.up), dt * 5f);
            }

            // 2. Kawanan herbivora bermigrasi mengikuti jadwal harian & berlari berpencar jika predator/Andi dekat.
            for (int i = 0; i < herbivora.Length; i++)
            {
                var h = herbivora[i];
                if (h.tr == null) continue;
                Vector3 tujuan = (fase == 0 ? h.titikAirPagi : fase == 1 ? h.titikHutanSiang : h.titikPadangSore) + h.offsetKawanan;
                bool dekat = (_posHerb[i] - cPos).sqrMagnitude < 115f * 115f || (tujuan - cPos).sqrMagnitude < 115f * 115f;
                if (h.tr.gameObject.activeSelf != dekat) h.tr.gameObject.SetActive(dekat);
                if (!dekat) continue;

                Vector3 ancaman = Vector3.zero;
                bool terancam = false;
                Vector3 dPemain = _posHerb[i] - cPos; dPemain.y = 0f;
                if (dPemain.sqrMagnitude < 10f * 10f) { ancaman = dPemain.normalized; terancam = true; }
                else if (predatorAktif)
                {
                    for (int p = 0; p < predator.Length; p++)
                    {
                        if (predator[p].tr == null || !predator[p].tr.gameObject.activeSelf) continue;
                        Vector3 dp = _posHerb[i] - predator[p].tr.position; dp.y = 0f;
                        if (dp.sqrMagnitude < 16f * 16f) { ancaman = dp.normalized; terancam = true; break; }
                    }
                }
                if (terancam) _panikT[i] = 3.0f;

                Vector3 target = _panikT[i] > 0f ? (_posHerb[i] + (ancaman.sqrMagnitude > 0.01f ? ancaman : Vector3.forward) * 9f) : tujuan;
                float speed = _panikT[i] > 0f ? h.laju * 2.4f : h.laju;
                if (_panikT[i] > 0f) _panikT[i] -= dt;

                Vector3 delta = target - _posHerb[i]; delta.y = 0f;
                if (delta.sqrMagnitude > 0.35f)
                {
                    _posHerb[i] += delta.normalized * Mathf.Min(delta.magnitude, speed * dt);
                    h.tr.rotation = Quaternion.Slerp(h.tr.rotation, Quaternion.LookRotation(delta.normalized, Vector3.up), dt * 4.5f);
                }
                float gy = Mathf.Max(0.5f, WorldTerrainGenerator.SampleHeight(_posHerb[i].x, _posHerb[i].z));
                float gerak = delta.sqrMagnitude > 0.35f ? Mathf.Abs(Mathf.Sin(t * (_panikT[i] > 0f ? 11f : 5.5f) + h.fasa)) * (_panikT[i] > 0f ? 0.30f : 0.08f) : 0f;
                h.tr.position = new Vector3(_posHerb[i].x, gy + gerak, _posHerb[i].z);
                if (h.kepala != null)
                {
                    bool tunduk = _panikT[i] <= 0f && delta.sqrMagnitude <= 1.2f;
                    h.kepala.localRotation = Quaternion.Euler(tunduk ? 34f + Mathf.Sin(t * 2f + h.fasa) * 7f : -6f, Mathf.Sin(t * 0.9f + h.fasa) * 12f, 0f);
                }
            }

            // 3. Biawak & buaya berjemur di batu sungai lalu meluncur masuk air saat didekati atau hujan.
            for (int i = 0; i < reptil.Length; i++)
            {
                var r = reptil[i];
                if (r.tr == null) continue;
                Vector3 dCam = r.batuJemur - cPos;
                float distSq = dCam.x * dCam.x + dCam.z * dCam.z;
                bool aktif = distSq < 88f * 88f;
                if (r.tr.gameObject.activeSelf != aktif) r.tr.gameObject.SetActive(aktif);
                if (!aktif) continue;

                bool sembunyi = ReptilHarusMasukAir(distSq, cuaca, jam);
                _masukAir01[i] = Mathf.MoveTowards(_masukAir01[i], sembunyi ? 1f : 0f, dt * (sembunyi ? 0.9f : 0.25f));
                Vector3 pos = Vector3.Lerp(r.batuJemur, r.titikAir, _masukAir01[i]);
                r.tr.position = pos;
                Vector3 arah = (sembunyi ? r.titikAir - r.batuJemur : r.batuJemur - r.titikAir); arah.y = 0f;
                if (arah.sqrMagnitude > 0.01f)
                    r.tr.rotation = Quaternion.Slerp(r.tr.rotation, Quaternion.LookRotation(arah.normalized, Vector3.up), dt * 4f);
            }
        }
    }
}
