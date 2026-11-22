using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Player
{
    // 17 §2.4: genangan lumpur di rawa - "BALUR LUMPUR" (tahan 1 s) = deteksi visual x0,7 selama 10 menit; luntur saat hujan/berenang.
    public class TitikLumpur : MonoBehaviour, IInteractable
    {
        public static int JumlahPercikanLumpur() { return 14; }
        public float HoldDuration { get { return 1f; } }
        public string GetPromptText(GameObject actor) { return "BALUR LUMPUR"; }
        public bool CanInteract(GameObject actor) { var r = GameServices.Get<ReaksiAndi>(); return r != null && r.SisaLumpur < ReaksiAndi.LamaLumpur * 0.9f; }
        public void Interact(GameObject actor)
        {
            var r = GameServices.Get<ReaksiAndi>();
            if (r != null) r.BalurLumpur();
            if (!Application.isPlaying) return;
            var go = new GameObject("PercikanLumpur");
            go.transform.position = transform.position + Vector3.up * 0.25f;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var m = ps.main; m.playOnAwake = false; m.loop = false; m.startLifetime = 0.5f; m.startSpeed = 2.4f; m.startSize = 0.08f; m.gravityModifier = 1.5f;
            Color w = new Color(0.28f, 0.22f, 0.16f); m.startColor = w;
            var pr = go.GetComponent<ParticleSystemRenderer>();
            pr.sharedMaterial = SafeMaterialFactory.GetCached("M_PercikLumpur", w, 0f, 0.65f);
            pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ps.Emit(JumlahPercikanLumpur());
            Destroy(go, 1f);
        }
    }
}
