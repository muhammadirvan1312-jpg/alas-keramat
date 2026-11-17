using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // T7a (02 §6 & 06 §6): pose tempur Sekte di atas lokomosi - ancang-ancang (lengan terangkat), ayunan, goyah, gertak (lengan terbuka),
    // memukul kentongan, berlutut menyerah (tangan terangkat), merangkak terluka, mengintip jongkok. Bobot pose di-crossfade 0,12 s.
    public partial class ProceduralAnimator
    {
        public enum PoseTempur { Tidak, Ancang, Ayun, Goyah, Gertak, Kentongan, Menyerah, Merangkak, Intai, Tari }
        public const float CrossfadePose = 0.12f;

        PoseTempur _pose, _poseLalu;
        float _bobotPose, _tPose;

        public PoseTempur Pose
        {
            get { return _pose; }
            set { if (value == _pose) return; _poseLalu = _pose; _pose = value; _tPose = 0f; _bobotPose = 0f; }
        }

        void TerapkanPoseTempur(float dt, float skala)
        {
            _tPose += dt;
            _bobotPose = Mathf.MoveTowards(_bobotPose, 1f, dt / CrossfadePose);
            if (_pose == PoseTempur.Tidak) { if (_poseLalu != PoseTempur.Tidak && _bobotPose < 1f) Satu(_poseLalu, 1f - _bobotPose, skala); return; }
            if (_poseLalu != PoseTempur.Tidak && _bobotPose < 1f) Satu(_poseLalu, 1f - _bobotPose, skala);
            Satu(_pose, _bobotPose, skala);
        }

        void Satu(PoseTempur p, float w, float skala)
        {
            float t = _tPose;
            switch (p)
            {
                case PoseTempur.Ancang:
                    R(Kerangka.RUpper, -150f * w, 0f, 20f * w); R(Kerangka.RFore, -60f * w, 0f, 0f);
                    R(Kerangka.Spine, -8f * w, -20f * w, 0f); R(Kerangka.LUpper, -30f * w, 0f, -10f * w);
                    break;
                case PoseTempur.Ayun:
                    float u = Mathf.Clamp01(t / 0.25f);
                    R(Kerangka.RUpper, Mathf.Lerp(-150f, -20f, u) * w, 0f, 20f * w); R(Kerangka.RFore, Mathf.Lerp(-60f, -10f, u) * w, 0f, 0f);
                    R(Kerangka.Spine, Mathf.Lerp(-8f, 18f, u) * w, Mathf.Lerp(-20f, 25f, u) * w, 0f);
                    break;
                case PoseTempur.Goyah:
                    R(Kerangka.Spine, -18f * w, 0f, Mathf.Sin(t * 9f) * 8f * w); R(Kerangka.Head, -15f * w, 0f, 0f);
                    R(Kerangka.LUpper, 0f, 0f, -35f * w); R(Kerangka.RUpper, 0f, 0f, 35f * w);
                    tulang[Kerangka.Pelvis].localPosition += Vector3.back * 0.08f * skala * w;
                    break;
                case PoseTempur.Gertak:
                    R(Kerangka.LUpper, -20f * w, 0f, -75f * w); R(Kerangka.RUpper, -20f * w, 0f, 75f * w);
                    R(Kerangka.LFore, -30f * w, 0f, 0f); R(Kerangka.RFore, -30f * w, 0f, 0f);
                    R(Kerangka.Spine, 10f * w, 0f, 0f); R(Kerangka.Head, -10f * w, 0f, 0f); R(Kerangka.Jaw, 18f * w, 0f, 0f);
                    break;
                case PoseTempur.Kentongan:
                    float k = Mathf.Repeat(t, 1.2f);
                    float pukul = k < 0.15f || (k > 0.3f && k < 0.45f) ? 1f : 0f; // "Tok-Tok ... Tok-Tok"
                    R(Kerangka.LUpper, -80f * w, 0f, 0f); R(Kerangka.LFore, -40f * w, 0f, 0f);
                    R(Kerangka.RUpper, (-100f + 40f * pukul) * w, 0f, 0f); R(Kerangka.RFore, -50f * w, 0f, 0f);
                    break;
                case PoseTempur.Menyerah:
                    var pel = tulang[Kerangka.Pelvis];
                    pel.localPosition = Vector3.Lerp(pel.localPosition, new Vector3(pel.localPosition.x, 0.55f * skala, pel.localPosition.z), w);
                    R(Kerangka.LThigh, -10f * w, 0f, 0f); R(Kerangka.RThigh, -10f * w, 0f, 0f);
                    R(Kerangka.LCalf, 95f * w, 0f, 0f); R(Kerangka.RCalf, 95f * w, 0f, 0f);
                    R(Kerangka.LUpper, -160f * w, 0f, -15f * w); R(Kerangka.RUpper, -160f * w, 0f, 15f * w);
                    R(Kerangka.LFore, -25f * w, 0f, 0f); R(Kerangka.RFore, -25f * w, 0f, 0f);
                    R(Kerangka.Head, 20f * w + Mathf.Sin(t * 6f) * 3f * w, 0f, 0f);
                    break;
                case PoseTempur.Merangkak:
                    var pp = tulang[Kerangka.Pelvis];
                    pp.localPosition = Vector3.Lerp(pp.localPosition, new Vector3(pp.localPosition.x, 0.35f * skala, pp.localPosition.z), w);
                    R(Kerangka.Pelvis, 70f * w, 0f, 0f); R(Kerangka.Head, -50f * w, 0f, 0f);
                    float s = Mathf.Sin(t * 2.5f);
                    R(Kerangka.LUpper, (-70f + 20f * s) * w, 0f, 0f); R(Kerangka.RUpper, (-70f - 20f * s) * w, 0f, 0f);
                    R(Kerangka.LThigh, (60f - 15f * s) * w, 0f, 0f); R(Kerangka.RThigh, (60f + 15f * s) * w, 0f, 0f);
                    R(Kerangka.LCalf, 80f * w, 0f, 0f); R(Kerangka.RCalf, 80f * w, 0f, 0f);
                    break;
                case PoseTempur.Intai:
                    var pi = tulang[Kerangka.Pelvis];
                    pi.localPosition = Vector3.Lerp(pi.localPosition, new Vector3(pi.localPosition.x, pi.localPosition.y - 0.35f * skala, pi.localPosition.z), w);
                    R(Kerangka.LThigh, -70f * w, 0f, 0f); R(Kerangka.RThigh, -70f * w, 0f, 0f);
                    R(Kerangka.LCalf, 110f * w, 0f, 0f); R(Kerangka.RCalf, 110f * w, 0f, 0f);
                    R(Kerangka.Spine, 25f * w, 0f, 0f); R(Kerangka.Head, -20f * w, 0f, 0f);
                    break;
                case PoseTempur.Tari:
                    float f = t * 2.2f;
                    R(Kerangka.LUpper, -60f * w, 0f, (-40f + 30f * Mathf.Sin(f)) * w); R(Kerangka.RUpper, -60f * w, 0f, (40f + 30f * Mathf.Sin(f + 1.5f)) * w);
                    R(Kerangka.Spine, 0f, 25f * Mathf.Sin(f * 0.5f) * w, 10f * Mathf.Sin(f) * w); R(Kerangka.Head, 15f * Mathf.Sin(f * 1.3f) * w, 0f, 0f);
                    tulang[Kerangka.Pelvis].localPosition += Vector3.up * 0.05f * Mathf.Abs(Mathf.Sin(f)) * skala * w;
                    break;
            }
        }
    }
}
