using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Player
{
    // Jejak ayunan senjata: garis cahaya tipis di ujung parang/kapak + jejak bara/emas khusus senjata pusaka.
    public partial class CombatAndLeftHandController
    {
        public const float LamaJejak = 0.12f, LebarJejak = 0.07f;
        [SerializeField] Material bahanJejak;
        TrailRenderer[] _jejak = new TrailRenderer[0];
        ParticleSystem _percikPusaka;
        ParticleSystemRenderer _percikPusakaRen;
        float _tPercikPusaka;
        ItemID _idPercikPusaka;

        public void PasangJejak(Material m) { bahanJejak = m; }

        public static bool AdalahSenjataPusaka(ItemID id)
        {
            return id == ItemID.GolokApiPusaka || id == ItemID.KerisPusaka || id == ItemID.GolokPusaka;
        }

        public static Color WarnaJejakSenjata(ItemID id)
        {
            if (id == ItemID.GolokApiPusaka) return new Color(1f, 0.44f, 0.10f, 0.85f);
            if (id == ItemID.KerisPusaka) return new Color(1f, 0.84f, 0.28f, 0.80f);
            if (id == ItemID.GolokPusaka) return new Color(1f, 0.90f, 0.56f, 0.62f);
            return new Color(1f, 0.97f, 0.90f, 0.45f);
        }

        public static float LebarJejakSenjata(ItemID id)
        {
            if (id == ItemID.GolokApiPusaka) return 0.12f;
            if (id == ItemID.KerisPusaka) return 0.105f;
            if (id == ItemID.GolokPusaka) return 0.088f;
            return LebarJejak;
        }

        public static float LamaJejakSenjata(ItemID id)
        {
            if (id == ItemID.GolokApiPusaka) return 0.22f;
            if (id == ItemID.KerisPusaka) return 0.20f;
            if (id == ItemID.GolokPusaka) return 0.16f;
            return LamaJejak;
        }

        // Jejak aktif pada fase ayun (sampai 75% durasi), bukan saat diam/tangkis.
        public static bool JejakAktif(float t, float dur) { return t >= 0f && dur > 0f && t < dur * 0.75f; }

        void MatikanJejak()
        {
            for (int i = 0; i < _jejak.Length; i++)
                if (_jejak[i] != null && _jejak[i].emitting) _jejak[i].emitting = false;
        }

        void TickJejak(float dur)
        {
            if (modelSenjata == null) return;
            if (_jejak.Length != modelSenjata.Length) _jejak = new TrailRenderer[modelSenjata.Length];
            bool aktif = JejakAktif(_t, dur);
            for (int i = 0; i < modelSenjata.Length; i++)
            {
                var m = modelSenjata[i];
                if (m == null) continue;
                ItemID id = i < Tabel.Length ? Tabel[i].id : ItemID.None;
                bool pusaka = AdalahSenjataPusaka(id);
                if (!pusaka && bahanJejak == null) continue;
                if (_jejak[i] == null && aktif && m.gameObject.activeInHierarchy) _jejak[i] = BuatJejak(m, id);
                if (_jejak[i] != null)
                {
                    bool nyala = aktif && m.gameObject.activeInHierarchy;
                    _jejak[i].emitting = nyala;
                    if (nyala && pusaka)
                    {
                        _tPercikPusaka -= Time.deltaTime;
                        if (_tPercikPusaka <= 0f) { _tPercikPusaka = 0.045f; PancarPercikPusaka(_jejak[i].transform.position, id); }
                    }
                }
            }
        }

        void PancarPercikPusaka(Vector3 pos, ItemID id)
        {
            Color w = WarnaJejakSenjata(id);
            if (_percikPusaka == null)
            {
                var go = new GameObject("PercikPusaka");
                _percikPusaka = go.AddComponent<ParticleSystem>();
                _percikPusaka.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var main = _percikPusaka.main;
                main.loop = false; main.playOnAwake = false; main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.startLifetime = 0.28f; main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.4f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.018f, 0.036f); main.gravityModifier = -0.15f;
                main.maxParticles = 24;
                var sh = _percikPusaka.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.05f;
                _percikPusakaRen = go.GetComponent<ParticleSystemRenderer>();
                _percikPusakaRen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _percikPusakaRen.receiveShadows = false;
            }
            if (_idPercikPusaka != id)
            {
                _idPercikPusaka = id;
                var m = _percikPusaka.main; m.startColor = w;
                if (_percikPusakaRen != null)
                    _percikPusakaRen.sharedMaterial = SafeMaterialFactory.CreatePBR("M_PercikPusaka_" + (int)id, w, 0f, 0.3f, null, null, new Color(w.r, w.g, w.b) * 2.8f, SurfaceMode.Transparent);
            }
            _percikPusaka.transform.position = pos;
            _percikPusaka.Emit(2);
        }

        TrailRenderer BuatJejak(Transform model, ItemID id)
        {
            var semuaMr = model.GetComponentsInChildren<MeshRenderer>(true);
            MeshRenderer mr = semuaMr.Length > 0 ? semuaMr[0] : null;
            float maxY = float.MinValue;
            for (int k = 0; k < semuaMr.Length; k++)
            {
                if (semuaMr[k] != null && semuaMr[k].transform.localPosition.y > maxY)
                { maxY = semuaMr[k].transform.localPosition.y; mr = semuaMr[k]; }
            }
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
            bool pusaka = AdalahSenjataPusaka(id);
            Color w = WarnaJejakSenjata(id);
            tr.sharedMaterial = pusaka || bahanJejak == null
                ? SafeMaterialFactory.CreatePBR("M_JejakPusaka_" + (int)id, w, 0f, 0.2f, null, null, new Color(w.r, w.g, w.b) * (pusaka ? 2.4f : 1.0f), SurfaceMode.Transparent)
                : bahanJejak;
            tr.time = LamaJejakSenjata(id); tr.minVertexDistance = 0.02f; tr.emitting = false;
            tr.widthCurve = AnimationCurve.Linear(0f, LebarJejakSenjata(id), 1f, 0f);
            Color akhir = id == ItemID.GolokApiPusaka ? new Color(1f, 0.86f, 0.28f)
                : id == ItemID.KerisPusaka ? new Color(0.32f, 0.95f, 0.78f)
                : id == ItemID.GolokPusaka ? new Color(0.95f, 0.78f, 0.35f) : Color.white;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(new Color(w.r, w.g, w.b), 0f), new GradientColorKey(akhir, 1f) },
                      new[] { new GradientAlphaKey(w.a, 0f), new GradientAlphaKey(0f, 1f) });
            tr.colorGradient = g;
            tr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; tr.receiveShadows = false;
            return tr;
        }
    }
}
