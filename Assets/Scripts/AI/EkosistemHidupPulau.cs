using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Pengelola tunggal zero-GC untuk ekosistem luas 4 km2: kawanan satwa merumput/bermigrasi di 4 kuadran luar
    // serta kawanan burung camar pantai & elang gunung melingkar di langit.
    public class EkosistemHidupPulau : MonoBehaviour
    {
        public const float JarakAktifSatwa = 85f, JarakAktifBurung = 220f, JarakKagetSatwa = 10f;

        [System.Serializable]
        public struct SatwaKawanan
        {
            public Transform tr;
            public Vector3 pusat;
            public float radius, laju, fase;
        }

        [System.Serializable]
        public struct BurungLangit
        {
            public Transform badan, sayapL, sayapR;
            public Vector3 pusat;
            public float radius, laju, fase;
        }

        [SerializeField] SatwaKawanan[] satwa = new SatwaKawanan[0];
        [SerializeField] BurungLangit[] burung = new BurungLangit[0];

        Transform _cam;
        AndiFPSController _andi;

        public int JumlahSatwa { get { return satwa != null ? satwa.Length : 0; } }
        public int JumlahBurung { get { return burung != null ? burung.Length : 0; } }

        public void Pasang(SatwaKawanan[] s, BurungLangit[] b)
        {
            satwa = s ?? new SatwaKawanan[0];
            burung = b ?? new BurungLangit[0];
        }

        public static Vector3 PosisiBurung(Vector3 pusat, float radius, float sudut)
        {
            return new Vector3(
                pusat.x + Mathf.Cos(sudut) * radius,
                pusat.y + Mathf.Sin(sudut * 1.5f) * 2.4f,
                pusat.z + Mathf.Sin(sudut) * radius);
        }

        void Update()
        {
            if (JedaDunia.Dijeda) return;
            if (_cam == null) { var c = Camera.main; if (c != null) _cam = c.transform; }
            if (_cam == null) return;
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            Vector3 cp = _cam.position;
            Vector3 ap = _andi != null ? _andi.transform.position : cp;
            float t = Time.time;
            float dt = Time.deltaTime;

            if (satwa != null)
            {
                float maxSq = JarakAktifSatwa * JarakAktifSatwa;
                for (int i = 0; i < satwa.Length; i++)
                {
                    var s = satwa[i];
                    if (s.tr == null) continue;
                    Vector3 dc = s.pusat - cp;
                    bool dekat = dc.x * dc.x + dc.z * dc.z <= maxSq;
                    if (s.tr.gameObject.activeSelf != dekat) s.tr.gameObject.SetActive(dekat);
                    if (!dekat) continue;

                    Vector3 da = s.tr.position - ap; da.y = 0f;
                    float kaget = da.sqrMagnitude < JarakKagetSatwa * JarakKagetSatwa ? 2.6f : 1f;
                    float a = t * s.laju * kaget + s.fase;
                    float x = s.pusat.x + Mathf.Cos(a) * s.radius;
                    float z = s.pusat.z + Mathf.Sin(a * 0.85f) * s.radius;
                    float y = WorldTerrainGenerator.SampleHeight(x, z);
                    Vector3 baru = new Vector3(x, y, z);
                    Vector3 arah = baru - s.tr.position; arah.y = 0f;
                    if (arah.sqrMagnitude > 1e-5f)
                        s.tr.rotation = Quaternion.Slerp(s.tr.rotation, Quaternion.LookRotation(arah), dt * 5f);
                    s.tr.position = baru;
                }
            }

            if (burung != null)
            {
                float maxSq = JarakAktifBurung * JarakAktifBurung;
                for (int i = 0; i < burung.Length; i++)
                {
                    var b = burung[i];
                    if (b.badan == null) continue;
                    Vector3 dc = b.pusat - cp;
                    bool dekat = dc.x * dc.x + dc.z * dc.z <= maxSq;
                    if (b.badan.gameObject.activeSelf != dekat) b.badan.gameObject.SetActive(dekat);
                    if (!dekat) continue;

                    float a = t * b.laju + b.fase;
                    Vector3 baru = PosisiBurung(b.pusat, b.radius, a);
                    Vector3 arah = baru - b.badan.position;
                    if (arah.sqrMagnitude > 1e-5f)
                        b.badan.rotation = Quaternion.Slerp(b.badan.rotation, Quaternion.LookRotation(arah), dt * 4f);
                    b.badan.position = baru;

                    float kepak = Mathf.Sin(t * 7.5f + b.fase) * 28f;
                    if (b.sayapL != null) b.sayapL.localRotation = Quaternion.Euler(0f, 0f, kepak);
                    if (b.sayapR != null) b.sayapR.localRotation = Quaternion.Euler(0f, 0f, -kepak);
                }
            }
        }
    }
}
