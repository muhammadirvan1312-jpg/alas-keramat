using System;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Biota perairan & pesisir hidup: 18 ikan sungai/karang melompat parabolis dari air,
    // 16 kepiting pantai berjalan menyamping & kabur saat Andi mendekat, serta 10 capung tepi air siang hari.
    public class IkanLompatDanKepiting : MonoBehaviour
    {
        public enum JenisBiota { IkanLompat, KepitingPantai, CapungAir }

        [Serializable]
        public struct BiotaHidup
        {
            public Transform tr;
            public JenisBiota jenis;
            public Vector3 pusat;
            public float radius;
            public float laju;
            public float fasa;
        }

        [SerializeField] BiotaHidup[] biota = new BiotaHidup[0];

        Vector3[] _posKini;
        float[] _sudut;
        float[] _kaburT;
        Transform _kamera;
        TimeOfDayClock _jam;
        bool _siap;

        public int JumlahBiota { get { return biota != null ? biota.Length : 0; } }

        public void Pasang(BiotaHidup[] d)
        {
            biota = d ?? new BiotaHidup[0];
        }

        public static float TinggiLompatIkan(float faseSiklus01, float tinggiMaks)
        {
            // Fase 0..0.24 = melompat parabolis di atas permukaan, 0.24..1.0 = berenang di bawah permukaan.
            if (faseSiklus01 < 0f || faseSiklus01 > 0.24f) return -0.22f;
            float u = faseSiklus01 / 0.24f;
            return 4f * u * (1f - u) * tinggiMaks - 0.10f;
        }

        void Awake()
        {
            int n = biota.Length;
            _posKini = new Vector3[n];
            _sudut = new float[n];
            _kaburT = new float[n];
            for (int i = 0; i < n; i++)
            {
                _posKini[i] = biota[i].tr != null ? biota[i].tr.position : biota[i].pusat;
                _sudut[i] = biota[i].fasa;
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
            float jam = _jam != null ? _jam.Jam : 10f;
            bool siang = jam >= 6.0f && jam < 17.8f;
            float dt = Time.deltaTime, t = Time.time;

            for (int i = 0; i < biota.Length; i++)
            {
                var b = biota[i];
                if (b.tr == null) continue;
                Vector3 dCam = b.pusat - cPos;
                float distSq = dCam.x * dCam.x + dCam.z * dCam.z;
                bool aktif = distSq < 74f * 74f && (b.jenis != JenisBiota.CapungAir || siang);
                if (b.tr.gameObject.activeSelf != aktif) b.tr.gameObject.SetActive(aktif);
                if (!aktif) continue;

                if (b.jenis == JenisBiota.IkanLompat)
                {
                    _sudut[i] += b.laju * dt;
                    float ang = _sudut[i];
                    float x = b.pusat.x + Mathf.Cos(ang) * b.radius;
                    float z = b.pusat.z + Mathf.Sin(ang) * b.radius;
                    float fase = Mathf.Repeat(t * 0.28f + b.fasa * 0.37f, 1f);
                    float dy = TinggiLompatIkan(fase, 1.05f + (i % 3) * 0.25f);
                    Vector3 np = new Vector3(x, b.pusat.y + dy, z);
                    Vector3 arah = np - b.tr.position;
                    b.tr.position = np;
                    if (arah.sqrMagnitude > 0.0001f)
                        b.tr.rotation = Quaternion.LookRotation(arah.normalized, Vector3.up);
                }
                else if (b.jenis == JenisBiota.KepitingPantai)
                {
                    Vector3 dariPemain = _posKini[i] - cPos;
                    dariPemain.y = 0f;
                    float jp = dariPemain.magnitude;
                    if (jp < 6.5f && jp > 0.1f) _kaburT[i] = 1.8f;

                    Vector3 target;
                    float speed;
                    if (_kaburT[i] > 0f)
                    {
                        _kaburT[i] -= dt;
                        target = _posKini[i] + (dariPemain / Mathf.Max(0.1f, jp)) * 3.5f;
                        Vector3 kePusat = target - b.pusat; kePusat.y = 0f;
                        if (kePusat.magnitude > b.radius * 2.0f) target = b.pusat + kePusat.normalized * (b.radius * 2.0f);
                        speed = 2.6f;
                    }
                    else
                    {
                        _sudut[i] += b.laju * dt;
                        float ang = _sudut[i];
                        target = new Vector3(b.pusat.x + Mathf.Cos(ang) * b.radius, 0f, b.pusat.z + Mathf.Sin(ang) * b.radius);
                        speed = Mathf.Sin(t * 1.4f + b.fasa) > 0.1f ? 0.9f : 0.05f;
                    }
                    Vector3 delta = target - _posKini[i]; delta.y = 0f;
                    if (delta.sqrMagnitude > 0.01f)
                    {
                        _posKini[i] += delta.normalized * Mathf.Min(delta.magnitude, speed * dt);
                        // Kepiting berjalan menyamping 90 derajat dari arah hadap cangkangnya.
                        Quaternion rot = Quaternion.LookRotation(delta.normalized, Vector3.up) * Quaternion.Euler(0f, 90f, 0f);
                        b.tr.rotation = Quaternion.Slerp(b.tr.rotation, rot, dt * 8f);
                    }
                    float y = Mathf.Max(0.18f, WorldTerrainGenerator.SampleHeight(_posKini[i].x, _posKini[i].z)) + 0.06f;
                    b.tr.position = new Vector3(_posKini[i].x, y, _posKini[i].z);
                }
                else
                {
                    // Capung melesat & melayang di atas permukaan sungai/kolam air terjun.
                    float ph = t * b.laju + b.fasa;
                    float x = b.pusat.x + Mathf.Sin(ph * 1.3f) * b.radius;
                    float z = b.pusat.z + Mathf.Cos(ph * 0.95f) * b.radius;
                    float y = b.pusat.y + 0.85f + Mathf.Sin(ph * 3.4f) * 0.28f;
                    Vector3 np = new Vector3(x, y, z);
                    Vector3 arah = np - b.tr.position;
                    b.tr.position = np;
                    if (arah.sqrMagnitude > 0.0001f)
                        b.tr.rotation = Quaternion.LookRotation(arah.normalized, Vector3.up);
                }
            }
        }
    }
}
