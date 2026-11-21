using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Monyet rimba melompat, kodok tepi sungai/rawa melompat ke air saat didekati, & efek "keheningan mendadak" saat hantu mendekat.
    public class EkosistemMonyetDanKodok : MonoBehaviour
    {
        public const float JarakAktif = 65f, RadiusKeheninganHantu = 22f;

        [System.Serializable]
        public struct SatwaKecil
        {
            public Transform tr;
            public Vector3 pusat;
            public float tinggiLompat, laju, fase;
            public bool kodok;
        }

        [SerializeField] SatwaKecil[] daftar = new SatwaKecil[0];
        Transform _cam;
        AndiFPSController _andi;
        TimeOfDayClock _jam;
        float _tSenyap;
        bool _sedangSenyap;

        public int Jumlah { get { return daftar != null ? daftar.Length : 0; } }
        public void Pasang(SatwaKecil[] d) { daftar = d ?? new SatwaKecil[0]; }

        public static bool HarusSenyapHantu(float jam, float jarakHantuTerdekat)
        {
            bool malam = jam >= 18.2f || jam < 5.5f;
            return malam && jarakHantuTerdekat > 0f && jarakHantuTerdekat <= RadiusKeheninganHantu;
        }

        void Update()
        {
            if (JedaDunia.Dijeda) return;
            if (_cam == null) { var c = Camera.main; if (c != null) _cam = c.transform; }
            if (_cam == null) return;
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();

            Vector3 cp = _cam.position;
            Vector3 ap = _andi != null ? _andi.transform.position : cp;
            float t = Time.time;
            float dt = Time.deltaTime;

            // Cek efek Keheningan Mendadak saat hantu mendekat malam hari
            _tSenyap -= dt;
            if (_tSenyap <= 0f)
            {
                _tSenyap = 1.5f;
                float dMin = 999f;
                var hs = HantuAI.Semua;
                for (int i = 0; i < hs.Count; i++)
                {
                    var h = hs[i];
                    if (h == null || !h.IsAlive || h.Fase == HantuAI.FaseHantu.Hilang) continue;
                    float d = Vector3.Distance(ap, h.transform.position);
                    if (d < dMin) dMin = d;
                }
                bool senyap = HarusSenyapHantu(_jam != null ? _jam.Jam : 12f, dMin);
                if (senyap && !_sedangSenyap)
                    GameEvents.RaiseSubtitle("(Suara jangkrik dan kodok malam mendadak senyap...)");
                _sedangSenyap = senyap;
            }

            if (daftar == null || daftar.Length == 0) return;
            float maxSq = JarakAktif * JarakAktif;
            for (int i = 0; i < daftar.Length; i++)
            {
                var s = daftar[i];
                if (s.tr == null) continue;
                Vector3 dc = s.pusat - cp;
                bool dekat = dc.x * dc.x + dc.z * dc.z <= maxSq;
                if (s.tr.gameObject.activeSelf != dekat) s.tr.gameObject.SetActive(dekat);
                if (!dekat) continue;

                Vector3 da = s.tr.position - ap; da.y = 0f;
                float panik = da.sqrMagnitude < 64f ? 2.1f : 1f;
                float lompat = Mathf.Abs(Mathf.Sin(t * s.laju * panik + s.fase)) * s.tinggiLompat;
                float geser = Mathf.Sin(t * s.laju * 0.45f + s.fase) * (s.kodok ? 1.4f : 2.8f);
                Vector3 p = new Vector3(s.pusat.x + geser, s.pusat.y + lompat, s.pusat.z + Mathf.Cos(t * s.laju * 0.35f + s.fase) * 1.2f);
                Vector3 arah = p - s.tr.position; arah.y = 0f;
                if (arah.sqrMagnitude > 1e-5f) s.tr.rotation = Quaternion.LookRotation(arah);
                s.tr.position = p;
            }
        }
    }
}
