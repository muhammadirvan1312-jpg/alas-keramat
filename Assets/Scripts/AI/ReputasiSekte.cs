using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Sistem reputasi & eskalasi Sekte khas SOTF (0..100): menebang banyak pohon/menyerang sekte menaikkan ancaman,
    // sementara bersembunyi dan memakai penangkal menurunkan ketegangan bertahap.
    public class ReputasiSekte : MonoBehaviour, ISaveable
    {
        public enum TingkatAncaman { Tenang = 0, Waspada = 1, Memburu = 2, Murka = 3 }
        public const float LajuTurunPerDetik = 0.08f;

        float _ancaman;
        float _t;

        public float NilaiAncaman { get { return _ancaman; } }
        public TingkatAncaman Tingkat { get { return Klasifikasi(_ancaman); } }
        public string SaveKey { get { return "reputasi_sekte_dunia"; } }

        public static TingkatAncaman Klasifikasi(float nilai)
        {
            if (nilai >= 75f) return TingkatAncaman.Murka;
            if (nilai >= 45f) return TingkatAncaman.Memburu;
            if (nilai >= 20f) return TingkatAncaman.Waspada;
            return TingkatAncaman.Tenang;
        }

        public static float PengaliJarakDeteksi(float nilai)
        {
            return Mathf.Lerp(0.92f, 1.38f, Mathf.Clamp01(nilai / 100f));
        }

        public void TambahAncaman(float delta, string alasan = null)
        {
            var lama = Tingkat;
            _ancaman = Mathf.Clamp(_ancaman + delta, 0f, 100f);
            if (Tingkat > lama && !string.IsNullOrEmpty(alasan))
                GameEvents.RaiseToast("Sekte semakin curiga (" + Tingkat + "): " + alasan);
        }

        void Awake() { GameServices.Register(this); }

        void Update()
        {
            if (_ancaman <= 0f || JedaDunia.Dijeda) return;
            _t += Time.deltaTime;
            if (_t < 1f) return;
            _ancaman = Mathf.Max(0f, _ancaman - LajuTurunPerDetik * _t);
            _t = 0f;
        }

        [System.Serializable] struct DataRep { public float a; }
        public string CaptureJson() { return JsonUtility.ToJson(new DataRep { a = _ancaman }); }
        public void RestoreJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            _ancaman = Mathf.Clamp(JsonUtility.FromJson<DataRep>(json).a, 0f, 100f);
        }
    }
}
