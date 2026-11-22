using System;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Menghidupkan 12 NPC humanoid MakeHuman G3 bertulang asli di seluruh situs kunci pulau:
    // berjalan patroli antar titik, berhenti memanaskan tangan di dekat api, berdoa di altar/candi/kuil,
    // menoleh ke Andi saat mendekat, serta menyalakan obor malam otomatis.
    public class WargaHidupPulau : MonoBehaviour
    {
        public enum GayaHidup { PatroliObor, DoaAltar, PenjagaPos, PenghangatApi }

        [Serializable]
        public struct WargaNPC
        {
            public Transform tr;
            public ProceduralAnimator anim;
            public Light obor;
            public GayaHidup gaya;
            public Vector3 titikA;
            public Vector3 titikB;
            public float laju;
            public float fasa;
        }

        [SerializeField] WargaNPC[] warga = new WargaNPC[0];

        float[] _tSiklus;
        Vector3[] _posKini;
        Transform _kamera;
        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _cuaca;
        bool _siap;

        public int JumlahWarga { get { return warga != null ? warga.Length : 0; } }

        public void Pasang(WargaNPC[] daftar)
        {
            warga = daftar ?? new WargaNPC[0];
        }

        public static bool PerluOborWarga(float jam, WeatherState cuaca)
        {
            return jam >= 17.5f || jam <= 5.8f || cuaca == WeatherState.HujanBadai;
        }

        void Awake()
        {
            int n = warga.Length;
            _tSiklus = new float[n];
            _posKini = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                _tSiklus[i] = warga[i].fasa;
                _posKini[i] = warga[i].tr != null ? warga[i].tr.position : warga[i].titikA;
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
            bool oborOn = PerluOborWarga(jam, cuaca);
            float dt = Time.deltaTime, t = Time.time;

            for (int i = 0; i < warga.Length; i++)
            {
                var w = warga[i];
                if (w.tr == null) continue;
                Vector3 dCam = w.titikA - cPos;
                float distSq = dCam.x * dCam.x + dCam.z * dCam.z;
                bool dekat = distSq < 95f * 95f;
                if (w.tr.gameObject.activeSelf != dekat) w.tr.gameObject.SetActive(dekat);
                if (!dekat) continue;

                if (w.gaya == GayaHidup.PatroliObor)
                {
                    _tSiklus[i] += dt * w.laju;
                    float s = Mathf.Repeat(_tSiklus[i], 2.5f);
                    float u = s < 1f ? s : s < 1.25f ? 1f : s < 2.25f ? 1f - (s - 1.25f) : 0f;
                    bool jeda = (s >= 1f && s < 1.25f) || s >= 2.25f;
                    Vector3 np = Vector3.Lerp(w.titikA, w.titikB, u);
                    np.y = Mathf.Max(0.4f, WorldTerrainGenerator.SampleHeight(np.x, np.z));
                    Vector3 delta = np - _posKini[i]; delta.y = 0f;
                    if (!jeda && delta.sqrMagnitude > 0.0004f)
                        w.tr.rotation = Quaternion.Slerp(w.tr.rotation, Quaternion.LookRotation(delta.normalized, Vector3.up), dt * 5f);
                    _posKini[i] = np;
                    w.tr.position = np;
                    if (w.anim != null) w.anim.KecepatanPaksa = jeda ? 0f : 1.45f;
                }
                else
                {
                    // Berdoa di altar, menjaga pos, atau menghangatkan diri di dekat api; menoleh saat Andi dekat (< 14 m).
                    Vector3 keAndi = cPos - w.tr.position; keAndi.y = 0f;
                    if (keAndi.sqrMagnitude > 0.25f && keAndi.sqrMagnitude < 14f * 14f)
                        w.tr.rotation = Quaternion.Slerp(w.tr.rotation, Quaternion.LookRotation(keAndi.normalized, Vector3.up), dt * 3.5f);
                    else
                    {
                        Vector3 keFokus = w.titikB - w.tr.position; keFokus.y = 0f;
                        if (keFokus.sqrMagnitude > 0.25f)
                            w.tr.rotation = Quaternion.Slerp(w.tr.rotation, Quaternion.LookRotation(keFokus.normalized, Vector3.up), dt * 2.5f);
                    }
                    if (w.anim != null)
                    {
                        w.anim.KecepatanPaksa = 0f;
                        if (w.gaya == GayaHidup.DoaAltar) w.anim.Pose = ProceduralAnimator.PoseTempur.Tari;
                    }
                }

                if (w.obor != null)
                {
                    bool nyala = oborOn && distSq < 72f * 72f;
                    if (w.obor.enabled != nyala) w.obor.enabled = nyala;
                    if (nyala) w.obor.intensity = 2.2f + Mathf.Sin(t * 8.4f + w.fasa) * 0.35f;
                }
            }
        }
    }
}
