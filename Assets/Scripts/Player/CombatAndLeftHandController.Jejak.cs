using UnityEngine;

namespace AlasKeramat.Player
{
    // Jejak ayunan senjata: garis cahaya tipis yang memudar di ujung parang/kapak saat menebas.
    public partial class CombatAndLeftHandController
    {
        public const float LamaJejak = 0.12f, LebarJejak = 0.07f;
        [SerializeField] Material bahanJejak;
        TrailRenderer[] _jejak = new TrailRenderer[0];

        public void PasangJejak(Material m) { bahanJejak = m; }

        // Jejak aktif pada fase ayun (sampai 75% durasi), bukan saat diam/tangkis.
        public static bool JejakAktif(float t, float dur) { return t >= 0f && dur > 0f && t < dur * 0.75f; }

        void TickJejak(float dur)
        {
            if (bahanJejak == null || modelSenjata == null) return;
            if (_jejak.Length != modelSenjata.Length) _jejak = new TrailRenderer[modelSenjata.Length];
            bool aktif = JejakAktif(_t, dur);
            for (int i = 0; i < modelSenjata.Length; i++)
            {
                var m = modelSenjata[i];
                if (m == null) continue;
                if (_jejak[i] == null && aktif && m.gameObject.activeInHierarchy) _jejak[i] = BuatJejak(m);
                if (_jejak[i] != null) _jejak[i].emitting = aktif && m.gameObject.activeInHierarchy;
            }
        }

        TrailRenderer BuatJejak(Transform model)
        {
            var mr = model.GetComponentInChildren<MeshRenderer>();
            Vector3 ujung = Vector3.zero; Transform induk = model;
            if (mr != null)
            {
                induk = mr.transform;
                var b = mr.localBounds; var e = b.extents;
                Vector3 sumbu = e.x >= e.y && e.x >= e.z ? Vector3.right * e.x : e.y >= e.z ? Vector3.up * e.y : Vector3.forward * e.z;
                ujung = b.center + sumbu * 0.9f;
            }
            var go = new GameObject("JejakAyun");
            go.transform.SetParent(induk, false); go.transform.localPosition = ujung;
            var tr = go.AddComponent<TrailRenderer>();
            tr.sharedMaterial = bahanJejak; tr.time = LamaJejak; tr.minVertexDistance = 0.02f; tr.emitting = false;
            tr.widthCurve = AnimationCurve.Linear(0f, LebarJejak, 1f, 0f);
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(new Color(1f, 0.97f, 0.9f), 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(0.45f, 0f), new GradientAlphaKey(0f, 1f) });
            tr.colorGradient = g;
            tr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; tr.receiveShadows = false;
            return tr;
        }
    }
}
