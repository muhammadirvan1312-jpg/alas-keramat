using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.World
{
    // Menghidupkan seluruh kompleks gua bawah tanah (UG1 Gua Sungai, UG2 Bunker, UG3 Kubah Akar Boss):
    // jamur bioluminesens berdenyut, lampu darurat bunker berkedip tegang, tungku kubah boss menyala berkobar,
    // serta spora gua melayang — hanya berjalan saat pemain di bawah tanah (nol beban di permukaan!).
    public class AtmosferGuaHidup : MonoBehaviour
    {
        [SerializeField] Light[] lampuJamur = new Light[0];
        [SerializeField] Light[] lampuDaruratBunker = new Light[0];
        [SerializeField] Light[] lampuKubahBoss = new Light[0];
        [SerializeField] Transform[] sporaGua = new Transform[0];

        float[] _intJamur0, _intBoss0;
        Vector3[] _posSpora0;
        Transform _kamera;
        bool _siap;

        public int JumlahElemenGua
        {
            get
            {
                return (lampuJamur != null ? lampuJamur.Length : 0)
                    + (lampuDaruratBunker != null ? lampuDaruratBunker.Length : 0)
                    + (lampuKubahBoss != null ? lampuKubahBoss.Length : 0)
                    + (sporaGua != null ? sporaGua.Length : 0);
            }
        }

        public void Pasang(Light[] jamur, Light[] bunker, Light[] boss, Transform[] spora)
        {
            lampuJamur = jamur ?? new Light[0];
            lampuDaruratBunker = bunker ?? new Light[0];
            lampuKubahBoss = boss ?? new Light[0];
            sporaGua = spora ?? new Transform[0];
        }

        public static float FaktorDenyutBio(float waktu, float fasa)
        {
            return 0.72f + 0.28f * Mathf.Sin(waktu * 1.85f + fasa);
        }

        public static float FaktorKedipBunker(float waktu, float fasa)
        {
            float s = Mathf.Sin(waktu * 7.5f + fasa) * Mathf.Sin(waktu * 19.3f + fasa * 1.7f);
            return s > -0.45f ? 1.0f : 0.18f;
        }

        void Awake()
        {
            _intJamur0 = new float[lampuJamur.Length];
            for (int i = 0; i < lampuJamur.Length; i++) if (lampuJamur[i] != null) _intJamur0[i] = lampuJamur[i].intensity;
            _intBoss0 = new float[lampuKubahBoss.Length];
            for (int i = 0; i < lampuKubahBoss.Length; i++) if (lampuKubahBoss[i] != null) _intBoss0[i] = lampuKubahBoss[i].intensity;
            _posSpora0 = new Vector3[sporaGua.Length];
            for (int i = 0; i < sporaGua.Length; i++) if (sporaGua[i] != null) _posSpora0[i] = sporaGua[i].position;
            _siap = true;
        }

        void Update()
        {
            if (!_siap || !Application.isPlaying) return;
            if (_kamera == null) { var c = Camera.main; if (c != null) _kamera = c.transform; }
            if (_kamera == null || !WorldTerrainGenerator.DiBawahTanah(_kamera.position.y)) return;

            Vector3 cPos = _kamera.position;
            float t = Time.time;

            for (int i = 0; i < lampuJamur.Length; i++)
            {
                var l = lampuJamur[i];
                if (l == null || (l.transform.position - cPos).sqrMagnitude > 80f * 80f) continue;
                l.intensity = _intJamur0[i] * FaktorDenyutBio(t, i * 1.1f);
            }

            for (int i = 0; i < lampuDaruratBunker.Length; i++)
            {
                var l = lampuDaruratBunker[i];
                if (l == null || (l.transform.position - cPos).sqrMagnitude > 80f * 80f) continue;
                l.intensity = 2.6f * FaktorKedipBunker(t, i * 2.3f);
            }

            for (int i = 0; i < lampuKubahBoss.Length; i++)
            {
                var l = lampuKubahBoss[i];
                if (l == null || (l.transform.position - cPos).sqrMagnitude > 90f * 90f) continue;
                float bara = 0.84f + 0.16f * Mathf.Sin(t * 6.8f + i * 1.4f);
                l.intensity = _intBoss0[i] * bara;
            }

            for (int i = 0; i < sporaGua.Length; i++)
            {
                var tr = sporaGua[i];
                if (tr == null || (_posSpora0[i] - cPos).sqrMagnitude > 65f * 65f) continue;
                float ph = t * 0.75f + i * 1.37f;
                tr.position = _posSpora0[i] + new Vector3(Mathf.Sin(ph) * 0.85f, Mathf.Sin(ph * 1.6f) * 0.55f, Mathf.Cos(ph * 0.9f) * 0.85f);
            }
        }
    }
}
