using AlasKeramat.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    // Toast (09): maks 3 baris, tiap baris tampil 3 s lalu pudar. Teks hanya diperbarui saat isi berubah.
    public class ToastHUD : MonoBehaviour
    {
        public const int MaksBaris = 3;
        public const float LamaTampil = 3f, LamaPudar = 0.6f;

        [SerializeField] Text label;
        readonly string[] _baris = new string[MaksBaris];
        readonly float[] _umur = new float[MaksBaris];
        int _n;
        bool _kotor;

        public void Pasang(Text t) { label = t; }

        void OnEnable() { GameEvents.OnToast += Terima; }
        void OnDisable() { GameEvents.OnToast -= Terima; }

        void Terima(string s)
        {
            if (string.IsNullOrEmpty(s)) return;
            if (_n == MaksBaris) Geser();
            _baris[_n] = s; _umur[_n] = 0f; _n++; _kotor = true;
        }

        void Geser()
        {
            for (int i = 1; i < _n; i++) { _baris[i - 1] = _baris[i]; _umur[i - 1] = _umur[i]; }
            _n--; _kotor = true;
        }

        void Update()
        {
            if (label == null) return;
            for (int i = 0; i < _n; i++) _umur[i] += Time.deltaTime;
            while (_n > 0 && _umur[0] > LamaTampil + LamaPudar) Geser();
            if (_kotor)
            {
                _kotor = false;
                label.text = _n == 0 ? "" : string.Join("\n", _baris, 0, _n);
            }
            float a = _n == 0 ? 0f : 1f - Mathf.Clamp01((_umur[_n - 1] - LamaTampil) / LamaPudar);
            var c = label.color;
            if (!Mathf.Approximately(c.a, a)) { c.a = a; label.color = c; }
        }
    }
}
