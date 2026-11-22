using System;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Keanekaragaman hayati tropis rimba:
    // - 16 Burung Kakaktua / Nuri berwarna cerah di puncak pohon yang berkicau & mengepakkan sayap
    // - 32 Kupu-kupu tropis & lebah emas menari di rumpun bunga & air terjun siang hari
    // - 24 Sulur akar raksasa & tirai lumut gantung bergoyang mengikuti kekuatan angin cuaca
    // - 12 Kura-kura sungai berjemur di kayu tumbang yang menarik kepala saat Andi mendekat
    public class KeanekaragamanRimbaV8 : MonoBehaviour
    {
        [Serializable]
        public struct BurungKanopi
        {
            public Transform tr;
            public Transform sayapKiri;
            public Transform sayapKanan;
            public Vector3 dahan;
            public float fasa;
        }

        [Serializable]
        public struct SeranggaBunga
        {
            public Transform tr;
            public Vector3 pusat;
            public float radius;
            public float laju;
            public float fasa;
        }

        [SerializeField] BurungKanopi[] nuri = new BurungKanopi[0];
        [SerializeField] SeranggaBunga[] serangga = new SeranggaBunga[0];
        [SerializeField] Transform[] sulurGantung = new Transform[0];
        [SerializeField] Transform[] kuraKepala = new Transform[0];

        Quaternion[] _rotSulur0;
        Vector3[] _posKuraKepala0;
        Transform _kamera;
        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _cuaca;
        bool _siap;

        public int JumlahHayati
        {
            get
            {
                return (nuri != null ? nuri.Length : 0)
                    + (serangga != null ? serangga.Length : 0)
                    + (sulurGantung != null ? sulurGantung.Length : 0)
                    + (kuraKepala != null ? kuraKepala.Length : 0);
            }
        }

        public void Pasang(BurungKanopi[] n, SeranggaBunga[] s, Transform[] sl, Transform[] kr)
        {
            nuri = n ?? new BurungKanopi[0];
            serangga = s ?? new SeranggaBunga[0];
            sulurGantung = sl ?? new Transform[0];
            kuraKepala = kr ?? new Transform[0];
        }

        public static bool SeranggaAktifSiang(float jam, WeatherState cuaca)
        {
            bool hujan = cuaca == WeatherState.Gerimis || cuaca == WeatherState.HujanBadai;
            return !hujan && jam >= 6.0f && jam <= 17.5f;
        }

        void Awake()
        {
            _rotSulur0 = new Quaternion[sulurGantung.Length];
            for (int i = 0; i < sulurGantung.Length; i++)
                if (sulurGantung[i] != null) _rotSulur0[i] = sulurGantung[i].localRotation;

            _posKuraKepala0 = new Vector3[kuraKepala.Length];
            for (int i = 0; i < kuraKepala.Length; i++)
                if (kuraKepala[i] != null) _posKuraKepala0[i] = kuraKepala[i].localPosition;

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
            float angin = cuaca == WeatherState.HujanBadai ? 2.5f : cuaca == WeatherState.Gerimis ? 1.55f : 1.0f;
            bool siangCerah = SeranggaAktifSiang(jam, cuaca);
            float t = Time.time;

            // 1. Kakaktua & Nuri kanopi melompat-lompat kecil & mengepakkan sayap di dahan.
            for (int i = 0; i < nuri.Length; i++)
            {
                var n = nuri[i];
                if (n.tr == null) continue;
                bool dekat = (n.dahan - cPos).sqrMagnitude < 85f * 85f;
                if (n.tr.gameObject.activeSelf != dekat) n.tr.gameObject.SetActive(dekat);
                if (!dekat) continue;

                float lompat = Mathf.Max(0f, Mathf.Sin(t * 2.4f + n.fasa)) * 0.16f;
                n.tr.position = n.dahan + new Vector3(0f, lompat, 0f);
                n.tr.rotation = Quaternion.Euler(0f, n.fasa * 45f + Mathf.Sin(t * 0.9f + n.fasa) * 32f, 0f);
                float kepak = lompat > 0.04f ? Mathf.Sin(t * 18f + n.fasa) * 42f : -28f;
                if (n.sayapKiri != null) n.sayapKiri.localRotation = Quaternion.Euler(0f, 0f, kepak);
                if (n.sayapKanan != null) n.sayapKanan.localRotation = Quaternion.Euler(0f, 0f, -kepak);
            }

            // 2. Kupu-kupu tropis & lebah emas menari di kebun/bunga siang hari.
            for (int i = 0; i < serangga.Length; i++)
            {
                var s = serangga[i];
                if (s.tr == null) continue;
                bool aktif = siangCerah && (s.pusat - cPos).sqrMagnitude < 58f * 58f;
                if (s.tr.gameObject.activeSelf != aktif) s.tr.gameObject.SetActive(aktif);
                if (!aktif) continue;

                float ph = t * s.laju + s.fasa;
                Vector3 pos = s.pusat + new Vector3(Mathf.Sin(ph * 1.4f) * s.radius, 0.75f + Mathf.Sin(ph * 3.1f) * 0.45f, Mathf.Cos(ph * 1.1f) * s.radius);
                s.tr.position = pos;
                s.tr.rotation = Quaternion.Euler(0f, ph * 55f, Mathf.Sin(t * 22f + s.fasa) * 18f);
            }

            // 3. Sulur akar & tirai lumut gantung bergoyang alami tertiup angin rimba.
            for (int i = 0; i < sulurGantung.Length; i++)
            {
                var tr = sulurGantung[i];
                if (tr == null) continue;
                if ((tr.position - cPos).sqrMagnitude > 95f * 95f) continue;
                float swX = Mathf.Sin(t * (1.25f * angin) + i * 0.9f) * (6.2f * angin);
                float swZ = Mathf.Cos(t * (0.95f * angin) + i * 1.3f) * (4.8f * angin);
                tr.localRotation = _rotSulur0[i] * Quaternion.Euler(swX, 0f, swZ);
            }

            // 4. Kura-kura sungai menarik kepala ke cangkang saat Andi mendekat (< 6 m).
            for (int i = 0; i < kuraKepala.Length; i++)
            {
                var tr = kuraKepala[i];
                if (tr == null) continue;
                float dSq = (tr.position - cPos).sqrMagnitude;
                if (dSq > 55f * 55f) continue;
                bool sembunyi = dSq < 6.0f * 6.0f;
                Vector3 target = _posKuraKepala0[i] + new Vector3(0f, 0f, sembunyi ? -0.11f : 0f);
                tr.localPosition = Vector3.Lerp(tr.localPosition, target, Time.deltaTime * 5f);
            }
        }
    }
}
