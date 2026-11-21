using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Kawanan kelelawar keluar dari mulut gua saat Maghrib (18:05-19:20), seperti di hutan tropis Indonesia.
    // Satu ParticleSystem billboard dengan lembar sprite kepak 4 frame (dibuat Builder) per mulut gua.
    // Hemat: emisi mati di luar jendela waktu atau bila kamera lebih jauh dari JarakAktif.
    [RequireComponent(typeof(ParticleSystem))]
    public class KawananKelelawarSenja : MonoBehaviour
    {
        public const float JamMulai = 18.08f, JamSelesai = 19.33f, LajuEmisi = 7f, JarakAktif = 420f, LamaHidup = 24f;
        const float JedaCek = 1f, LajuPuncak = 1.6f;

        ParticleSystem _ps;
        TimeOfDayClock _jam;
        float _cekBerikut, _lajuKini = -1f;

        // Murni: laju emisi menurut jam. Memuncak di tengah jendela Maghrib, nol di luar.
        public static float LajuUntuk(float jam)
        {
            if (jam < JamMulai || jam >= JamSelesai) return 0f;
            float u = (jam - JamMulai) / (JamSelesai - JamMulai);
            return LajuEmisi * Mathf.Lerp(1f, LajuPuncak, Mathf.Sin(u * Mathf.PI));
        }

        void Awake() { _ps = GetComponent<ParticleSystem>(); }

        void Update()
        {
            if (Time.time < _cekBerikut || _ps == null) return;
            _cekBerikut = Time.time + JedaCek;
            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            float jam = _jam != null ? _jam.Jam : TimeOfDayClock.JamMulai;
            var cam = Camera.main;
            bool dekat = cam == null || (cam.transform.position - transform.position).sqrMagnitude < JarakAktif * JarakAktif;
            float laju = dekat ? LajuUntuk(jam) : 0f;
            if (Mathf.Abs(laju - _lajuKini) < 0.05f) return;
            _lajuKini = laju;
            var em = _ps.emission;
            em.rateOverTime = laju;
            if (laju > 0f && !_ps.isPlaying) _ps.Play();
        }
    }
}
