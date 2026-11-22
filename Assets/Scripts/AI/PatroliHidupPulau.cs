using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Konvoi patroli obor sekte yang benar-benar berjalan menyusuri jalur antar-zona pulau (cahaya obor bergerak di hutan malam).
    public class PatroliHidupPulau : MonoBehaviour
    {
        public const float JarakAktif = 130f, JarakDeteksi = 13f, KecepatanJalan = 2.6f;

        [System.Serializable]
        public struct ReguPatroli
        {
            public Transform tr;
            public Light obor;
            public MeshRenderer apiObor;
            public Vector3 titikA, titikB;
            public float fase;
        }

        [SerializeField] ReguPatroli[] regu = new ReguPatroli[0];
        Transform _cam;
        AndiFPSController _andi;
        TimeOfDayClock _jam;
        float _tPeringatan;

        public int JumlahRegu { get { return regu != null ? regu.Length : 0; } }
        public void Pasang(ReguPatroli[] r) { regu = r ?? new ReguPatroli[0]; }

        public static Vector3 PosisiPatroli(Vector3 a, Vector3 b, float waktu, float fase)
        {
            float dist = Mathf.Max(4f, Vector3.Distance(a, b));
            float u = Mathf.PingPong((waktu * KecepatanJalan / dist) + fase, 1f);
            Vector3 p = Vector3.Lerp(a, b, u);
            p.y = WorldTerrainGenerator.SampleHeight(p.x, p.z);
            return p;
        }

        void Update()
        {
            if (regu == null || regu.Length == 0 || JedaDunia.Dijeda) return;
            if (_cam == null) { var c = Camera.main; if (c != null) _cam = c.transform; }
            if (_cam == null) return;
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();

            Vector3 cp = _cam.position;
            Vector3 ap = _andi != null ? _andi.transform.position : cp;
            float t = Time.time;
            float dt = Time.deltaTime;
            bool malam = _jam == null || _jam.Jam >= 17.8f || _jam.Jam < 5.8f;
            if (_tPeringatan > 0f) _tPeringatan -= dt;

            for (int i = 0; i < regu.Length; i++)
            {
                var r = regu[i];
                if (r.tr == null) continue;
                Vector3 pBaru = PosisiPatroli(r.titikA, r.titikB, t, r.fase);
                Vector3 dc = pBaru - cp;
                bool dekat = dc.x * dc.x + dc.z * dc.z <= JarakAktif * JarakAktif;
                if (r.tr.gameObject.activeSelf != dekat) r.tr.gameObject.SetActive(dekat);
                if (!dekat) continue;

                Vector3 arah = pBaru - r.tr.position; arah.y = 0f;
                if (arah.sqrMagnitude > 1e-5f)
                    r.tr.rotation = Quaternion.Slerp(r.tr.rotation, Quaternion.LookRotation(arah), dt * 5f);
                r.tr.position = pBaru;

                bool nyalaApi = malam && (dc.x * dc.x + dc.z * dc.z <= 75f * 75f);
                if (r.apiObor != null && r.apiObor.enabled != malam) r.apiObor.enabled = malam;
                if (r.obor != null)
                {
                    if (r.obor.enabled != nyalaApi) r.obor.enabled = nyalaApi;
                    if (nyalaApi) r.obor.intensity = 2.0f + Mathf.Sin(t * 11f + i * 2f) * 0.35f;
                }

                if (_andi != null && _tPeringatan <= 0f && !_andi.TersembunyiDiSemak)
                {
                    Vector3 da = pBaru - ap; da.y = 0f;
                    if (da.sqrMagnitude < JarakDeteksi * JarakDeteksi)
                    {
                        _tPeringatan = 22f;
                        var rep = GameServices.Get<ReputasiSekte>();
                        if (rep != null) rep.TambahAncaman(12f, "Terpergok patroli obor sekte");
                        var synth = GameServices.Get<ProceduralAudioSynth>();
                        if (synth != null) synth.Play(ProceduralAudioSynth.KlipTeriakSekte, pBaru, AudioCategory.Efek, 0.7f, 1.05f, 2, 42f);
                    }
                }
            }
        }
    }
}
