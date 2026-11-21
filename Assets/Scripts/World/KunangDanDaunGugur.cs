using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.World
{
    // Kunang-kunang malam berkedip keemasan di hutan/sungai/desa & daun tropis gugur tertiup angin saat siang.
    // Nol alokasi GC, culling jarak 72 m, hemat baterai Android.
    public class KunangDanDaunGugur : MonoBehaviour
    {
        public const float JarakAktif = 72f;

        [SerializeField] Transform[] kunang = new Transform[0];
        [SerializeField] Vector3[] asalKunang = new Vector3[0];
        [SerializeField] Transform[] daun = new Transform[0];
        [SerializeField] Vector3[] asalDaun = new Vector3[0];

        TimeOfDayClock _jam;
        AndiFPSController _andi;
        AKAngin _angin;
        MeshRenderer[] _rendKunang;

        public int JumlahKunang { get { return kunang != null ? kunang.Length : 0; } }
        public int JumlahDaun { get { return daun != null ? daun.Length : 0; } }

        public void Pasang(Transform[] k, Vector3[] ak, Transform[] d, Vector3[] ad)
        {
            kunang = k ?? new Transform[0];
            asalKunang = ak ?? new Vector3[0];
            daun = d ?? new Transform[0];
            asalDaun = ad ?? new Vector3[0];
        }

        public static bool WaktuKunang(float jam) { return jam >= 17.6f || jam < 5.4f; }

        void Start()
        {
            _rendKunang = new MeshRenderer[kunang.Length];
            for (int i = 0; i < kunang.Length; i++)
                if (kunang[i] != null) _rendKunang[i] = kunang[i].GetComponent<MeshRenderer>();
        }

        void Update()
        {
            if (JedaDunia.Dijeda) return;
            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            if (_andi == null) return;

            float jam = _jam != null ? _jam.Jam : 9f;
            bool malam = WaktuKunang(jam) && !URPAtmosphereAndWeather.DalamRuangan;
            Vector3 pAndi = _andi.transform.position;
            float now = Time.time;
            float j2 = JarakAktif * JarakAktif;

            // 1. Kunang-kunang malam
            for (int i = 0; i < kunang.Length; i++)
            {
                var tr = kunang[i];
                if (tr == null || i >= asalKunang.Length) continue;
                bool dekat = malam && (asalKunang[i] - pAndi).sqrMagnitude <= j2;
                if (_rendKunang != null && i < _rendKunang.Length && _rendKunang[i] != null && _rendKunang[i].enabled != dekat)
                    _rendKunang[i].enabled = dekat;
                if (!dekat) continue;
                float f = now * (0.9f + (i % 4) * 0.22f) + i * 1.7f;
                tr.position = asalKunang[i] + new Vector3(Mathf.Sin(f) * 1.8f, Mathf.Sin(f * 1.6f) * 0.65f + 0.5f, Mathf.Cos(f * 0.85f) * 1.8f);
                float kedip = 0.45f + 0.55f * Mathf.Abs(Mathf.Sin(now * 2.8f + i * 2.1f));
                tr.localScale = Vector3.one * (0.09f * kedip);
            }

            // 2. Daun tropis gugur saat siang
            bool siang = !malam && !URPAtmosphereAndWeather.DalamRuangan;
            if (_angin == null) _angin = GameServices.Get<AKAngin>();
            float angin = Mathf.Max(0.4f, _angin != null ? _angin.Kekuatan : 0.4f);
            for (int i = 0; i < daun.Length; i++)
            {
                var tr = daun[i];
                if (tr == null || i >= asalDaun.Length) continue;
                bool dekat = siang && (asalDaun[i] - pAndi).sqrMagnitude <= j2;
                if (tr.gameObject.activeSelf != dekat) tr.gameObject.SetActive(dekat);
                if (!dekat) continue;
                float siklus = Mathf.Repeat(now * (0.18f + (i % 3) * 0.05f) + i * 0.37f, 1f);
                float yTurun = (1f - siklus) * 7.5f;
                float ayunX = Mathf.Sin(siklus * 9.5f + i) * 1.4f + siklus * angin * 2.4f;
                float ayunZ = Mathf.Cos(siklus * 7.2f + i) * 1.1f;
                tr.position = asalDaun[i] + new Vector3(ayunX, yTurun, ayunZ);
                tr.rotation = Quaternion.Euler(siklus * 240f + i * 30f, now * 65f + i * 40f, Mathf.Sin(now * 3f + i) * 45f);
            }
        }
    }
}
