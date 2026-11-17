using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    // Subtitle 2 baris (23 §8 T3, 09 §10): antrean, lama tampil = 0,06 s/huruf (min 2,5 s), selalu aktif. Monolog pembuka saat Andi sadar.
    public class SubtitleSystem : MonoBehaviour
    {
        public const float DetikPerHuruf = 0.06f, MinTampil = 2.5f, Pudar = 0.4f, JedaAwal = 2f;

        [SerializeField] Text label;
        [SerializeField] string[] monologAwal = new string[0];

        readonly Queue<string> _antre = new Queue<string>();
        float _sisa, _lama, _tAwal;
        bool _awalDikirim;

        public void Pasang(Text t, string[] awal) { label = t; monologAwal = awal; }

        void OnEnable() { GameEvents.OnSubtitle += Terima; }
        void OnDisable() { GameEvents.OnSubtitle -= Terima; }
        void Terima(string s) { if (!string.IsNullOrEmpty(s)) _antre.Enqueue(s); }

        public static float LamaTampil(string s) { return Mathf.Max(MinTampil, (s == null ? 0 : s.Length) * DetikPerHuruf); }

        void Update()
        {
            if (!_awalDikirim)
            {
                _tAwal += Time.deltaTime;
                if (_tAwal >= JedaAwal) { _awalDikirim = true; foreach (var m in monologAwal) Terima(m); }
            }
            if (label == null) return;
            if (_sisa <= 0f && _antre.Count > 0)
            {
                string s = _antre.Dequeue();
                label.text = s; _lama = _sisa = LamaTampil(s);
            }
            if (_sisa > 0f)
            {
                _sisa -= Time.deltaTime;
                float a = Mathf.Clamp01(Mathf.Min(_sisa, _lama - _sisa) / Pudar);
                var c = label.color; c.a = a; label.color = c;
                if (_sisa <= 0f) label.text = "";
            }
        }
    }
}
