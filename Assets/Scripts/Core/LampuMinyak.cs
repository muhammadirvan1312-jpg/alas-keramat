using System.Collections.Generic;
using UnityEngine;

namespace AlasKeramat.Core
{
    // Lampu minyak desa (props CC0): api kecil menyala 18:00-05:30 dan berkedip.
    // Hemat HP RAM 4 GB: hanya MaksCahaya lampu terdekat (dalam JarakCahaya) yang memakai Light titik tanpa bayangan;
    // lampu lain cukup api emisif. Pemilihan diperbarui 2x per detik tanpa alokasi.
    public class LampuMinyak : MonoBehaviour
    {
        public const int MaksCahaya = 2;
        public const float JarakCahaya = 32f, JamNyala = 18f, JamPadam = 5.5f, JarakApi = 90f;

        public Renderer Api;
        public Light Cahaya;
        public float Intensitas = 1.4f;

        static readonly List<LampuMinyak> _semua = new List<LampuMinyak>();
        static readonly LampuMinyak[] _terpilih = new LampuMinyak[MaksCahaya];
        static readonly float[] _jarakTerpilih = new float[MaksCahaya];
        static float _tSusun;
        static int _frameSusun = -1;
        static TimeOfDayClock _jam;

        bool _apiNyala, _cahayaNyala;
        float _fase;
        Vector3 _skalaApi = Vector3.one;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatis()
        {
            _semua.Clear(); _tSusun = 0f; _frameSusun = -1; _jam = null;
            for (int i = 0; i < MaksCahaya; i++) { _terpilih[i] = null; _jarakTerpilih[i] = float.MaxValue; }
        }

        public static int Jumlah { get { return _semua.Count; } }

        public static bool NyalaPada(float jam) { return jam >= JamNyala || jam < JamPadam; }

        // Kedip api: 0,72..1,0 (murni, bisa diuji).
        public static float Kedip(float t)
        {
            return Mathf.Clamp(0.86f + 0.09f * Mathf.Sin(t) + 0.05f * Mathf.Sin(t * 2.7f + 1.3f), 0.72f, 1f);
        }

        void OnEnable()
        {
            _semua.Add(this);
            Vector3 p = transform.position;
            _fase = Mathf.Repeat(p.x * 0.37f + p.z * 0.11f, 6.28f);
            if (Api != null) _skalaApi = Api.transform.localScale;
            Terapkan(false, false);
            _tSusun = 0f;
        }

        void OnDisable()
        {
            _semua.Remove(this);
            for (int i = 0; i < MaksCahaya; i++) if (_terpilih[i] == this) _terpilih[i] = null;
        }

        void Update()
        {
            if (_frameSusun != Time.frameCount && Time.time >= _tSusun)
            {
                _frameSusun = Time.frameCount;
                _tSusun = Time.time + 0.5f;
                Susun();
            }
            if (!_apiNyala) return;
            float k = Kedip(Time.time * 9f + _fase);
            if (Api != null) Api.transform.localScale = new Vector3(_skalaApi.x, _skalaApi.y * k, _skalaApi.z);
            if (_cahayaNyala && Cahaya != null) Cahaya.intensity = Intensitas * k;
        }

        static void Susun()
        {
            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            float jam = _jam != null ? _jam.Jam : TimeOfDayClock.JamMulai;
            bool malam = NyalaPada(jam);
            var cam = Camera.main;
            Vector3 pusat = cam != null ? cam.transform.position : Vector3.zero;

            for (int i = 0; i < MaksCahaya; i++) { _terpilih[i] = null; _jarakTerpilih[i] = float.MaxValue; }
            if (malam)
            {
                float batas = JarakCahaya * JarakCahaya;
                for (int i = 0; i < _semua.Count; i++)
                {
                    var l = _semua[i];
                    if (l == null) continue;
                    float d = (l.transform.position - pusat).sqrMagnitude;
                    if (d > batas) continue;
                    for (int s = 0; s < MaksCahaya; s++)
                    {
                        if (d >= _jarakTerpilih[s]) continue;
                        for (int g = MaksCahaya - 1; g > s; g--) { _terpilih[g] = _terpilih[g - 1]; _jarakTerpilih[g] = _jarakTerpilih[g - 1]; }
                        _terpilih[s] = l; _jarakTerpilih[s] = d;
                        break;
                    }
                }
            }

            float batasApi = JarakApi * JarakApi;
            for (int i = 0; i < _semua.Count; i++)
            {
                var l = _semua[i];
                if (l == null) continue;
                bool dekat = (l.transform.position - pusat).sqrMagnitude <= batasApi;
                bool pilih = false;
                for (int s = 0; s < MaksCahaya; s++) if (_terpilih[s] == l) pilih = true;
                l.Terapkan(malam && dekat, malam && pilih);
            }
        }

        void Terapkan(bool api, bool cahaya)
        {
            if (api != _apiNyala || Api == null || Api.enabled != api)
            {
                _apiNyala = api;
                if (Api != null) { Api.enabled = api; if (!api) Api.transform.localScale = _skalaApi; }
            }
            if (cahaya != _cahayaNyala || Cahaya == null || Cahaya.enabled != cahaya)
            {
                _cahayaNyala = cahaya;
                if (Cahaya != null) Cahaya.enabled = cahaya;
            }
        }
    }
}
