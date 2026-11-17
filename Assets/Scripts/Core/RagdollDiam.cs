using UnityEngine;

namespace AlasKeramat.Core
{
    // Membekukan ragdoll (kinematic) setelah 6 s diam atau saat dipaksa (batas 4 aktif).
    public class RagdollDiam : MonoBehaviour
    {
        Rigidbody[] _rb;
        float _tDiam;
        public bool Beku { get; private set; }
        public void Pasang(Rigidbody[] rb) { _rb = rb; }

        void FixedUpdate()
        {
            if (Beku || _rb == null) return;
            float v = 0f;
            for (int i = 0; i < _rb.Length; i++) if (_rb[i] != null) v = Mathf.Max(v, PhysicsCompat.GetVelocity(_rb[i]).sqrMagnitude);
            _tDiam = v < 0.04f ? _tDiam + Time.fixedDeltaTime : 0f;
            if (_tDiam >= GoreSystem.LamaDiamRagdoll) Bekukan();
        }

        public void Bekukan()
        {
            if (Beku) return;
            Beku = true;
            if (_rb == null) return;
            for (int i = 0; i < _rb.Length; i++) if (_rb[i] != null) { _rb[i].isKinematic = true; }
        }
    }
}
