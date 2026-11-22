using UnityEngine;

namespace AlasKeramat.Core
{
    // Dunia bergerak: 10 titik asap mengepul dari tungku/cerobong/heli + 18 kain/bendera berkibar mengikuti angin global.
    public class AsapDanKainTertiuAngin : MonoBehaviour
    {
        public const float JarakAktifAsap = 120f;

        [SerializeField] Vector3[] titikAsap = new Vector3[0];
        [SerializeField] Transform[] kainBerkibar = new Transform[0];

        Transform _cam;
        GoreSystem _gore;
        float _tAsap;

        public int JumlahAsap { get { return titikAsap != null ? titikAsap.Length : 0; } }
        public int JumlahKain { get { return kainBerkibar != null ? kainBerkibar.Length : 0; } }

        public void Pasang(Vector3[] asap, Transform[] kain)
        {
            titikAsap = asap ?? new Vector3[0];
            kainBerkibar = kain ?? new Transform[0];
        }

        public static float SudutKibar(float waktu, float fase, float angin)
        {
            return Mathf.Sin(waktu * (3.2f + angin * 2.5f) + fase) * (10f + Mathf.Clamp01(angin) * 18f);
        }

        void Update()
        {
            if (JedaDunia.Dijeda) return;
            if (_cam == null) { var c = Camera.main; if (c != null) _cam = c.transform; }
            if (_cam == null) return;
            Vector3 cp = _cam.position;
            float t = Time.time;
            float angin = Shader.GetGlobalFloat("_AK_AnginKuat");

            if (kainBerkibar != null)
            {
                for (int i = 0; i < kainBerkibar.Length; i++)
                {
                    var k = kainBerkibar[i];
                    if (k == null) continue;
                    if ((k.position - cp).sqrMagnitude > 90f * 90f) continue;
                    k.localRotation = Quaternion.Euler(0f, SudutKibar(t, i * 1.1f, angin), 0f);
                }
            }

            _tAsap -= Time.deltaTime;
            if (_tAsap > 0f || titikAsap == null || titikAsap.Length == 0) return;
            _tAsap = 0.55f;
            if (_gore == null) _gore = GameServices.Get<GoreSystem>();
            if (_gore == null) return;
            float maxSq = JarakAktifAsap * JarakAktifAsap;
            for (int i = 0; i < titikAsap.Length; i++)
            {
                Vector3 d = titikAsap[i] - cp;
                if (d.x * d.x + d.z * d.z > maxSq) continue;
                _gore.PercikPartikel(titikAsap[i], new Color(0.76f, 0.78f, 0.80f, 0.36f), 2, 0.42f, 1.05f, 2.2f);
            }
        }
    }
}
