using System;
using UnityEngine;

namespace AlasKeramat.Core
{
    // Daun gugur melayang di hutan saat siang dan debu berkilau di udara saat cerah; mengikuti kamera, mati di gua dan saat hujan.
    public class PartikelAlam : MonoBehaviour
    {
        public const float LajuDaun = 7f, LajuDebu = 14f, BatasGua = -40f;
        public const float LajuKunangMalam = 18f, LajuSporaGua = 14f;
        [SerializeField] Material daun, debu;
        ParticleSystem _daun, _debu, _percik, _kunang;
        public const float PercikGerimis = 110f, PercikBadai = 380f;

        // Percikan air di tanah sekitar pemain saat hujan (partikel/detik).
        public static float LajuPercik(WeatherState w)
        {
            return w == WeatherState.HujanBadai ? PercikBadai : w == WeatherState.Gerimis ? PercikGerimis : 0f;
        }

        public static float LajuKunang(float jam, WeatherState w, bool hutan, bool diGua)
        {
            if (diGua) return LajuSporaGua;
            if (w == WeatherState.HujanBadai) return 0f;
            bool malam = jam >= 17.5f || jam <= 5.8f;
            if (!malam || !hutan) return 0f;
            return w == WeatherState.Gerimis ? LajuKunangMalam * 0.45f : LajuKunangMalam;
        }
        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _atmos;
        bool _gagal;

        public void Pasang(Material d, Material b) { daun = d; debu = b; }

        public static bool BiomaHutan(Biome b)
        {
            int i = (int)b;
            return b == Biome.HutanCampuran || b == Biome.HutanJatiSelatan || b == Biome.HutanBambu || b == Biome.TepiSungai || b == Biome.KawahBeringin || i == 11 || i == 15;
        }

        // Laju emisi (partikel/detik) menurut jam, cuaca dan bioma; dipakai juga oleh self-test.
        public static void Laju(float jam, WeatherState w, bool hutan, out float rDaun, out float rDebu)
        {
            bool siang = jam >= 6.5f && jam < 17.5f;
            bool basah = w == WeatherState.Gerimis || w == WeatherState.HujanBadai;
            rDaun = siang && hutan && !basah ? LajuDaun : 0f;
            rDebu = siang && !basah && (w == WeatherState.Cerah || w == WeatherState.Berawan || w == WeatherState.KabutPagi) ? (hutan ? LajuDebu : LajuDebu * 0.4f) : 0f;
        }

        ParticleSystem Buat(string nama, Material m, bool isDaun)
        {
            var go = new GameObject(nama);
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = true; main.simulationSpace = ParticleSystemSimulationSpace.World; main.playOnAwake = false;
            main.startLifetime = isDaun ? 9f : 7f;
            main.startSpeed = isDaun ? 0.15f : 0.05f;
            main.startSize = isDaun ? new ParticleSystem.MinMaxCurve(0.07f, 0.15f) : new ParticleSystem.MinMaxCurve(0.012f, 0.03f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.gravityModifier = isDaun ? 0.035f : 0f;
            main.maxParticles = isDaun ? 90 : 160;
            main.startColor = isDaun ? new ParticleSystem.MinMaxGradient(new Color(0.55f, 0.48f, 0.26f), new Color(0.36f, 0.44f, 0.20f)) : new ParticleSystem.MinMaxGradient(new Color(1f, 0.95f, 0.8f, 0.55f));
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = isDaun ? new Vector3(34f, 2f, 34f) : new Vector3(16f, 6f, 16f);
            var noise = ps.noise; noise.enabled = true; noise.strength = isDaun ? 0.7f : 0.25f; noise.frequency = isDaun ? 0.35f : 0.6f; noise.scrollSpeed = 0.3f;
            var col = ps.colorOverLifetime; col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.12f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
            if (isDaun) { var rot = ps.rotationOverLifetime; rot.enabled = true; rot.z = new ParticleSystem.MinMaxCurve(-3f, 3f); }
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Billboard; r.sharedMaterial = m;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
            return ps;
        }

        void Update()
        {
            if (_gagal || daun == null || debu == null) return;
            try
            {
                if (_daun == null) { _daun = Buat("DaunGugur", daun, true); _debu = Buat("DebuCahaya", debu, false); _percik = BuatPercik(); _kunang = BuatKunang(); }
                if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
                if (_atmos == null) _atmos = GameServices.Get<URPAtmosphereAndWeather>();
                var cam = Camera.main;
                if (cam == null) return;
                Vector3 p = cam.transform.position;
                float rd = 0f, rb = 0f;
                float jamVal = _jam != null ? _jam.Jam : 12f;
                var w = _atmos != null ? _atmos.Cuaca : WeatherState.Cerah;
                bool hutan = BiomaHutan(WorldTerrainGenerator.GetBiome(p.x, p.z));
                bool diGua = p.y <= BatasGua;
                if (!diGua) Laju(jamVal, w, hutan, out rd, out rb);
                float rp = !diGua ? LajuPercik(w) : 0f;
                float rk = LajuKunang(jamVal, w, hutan, diGua);
                _percik.transform.position = new Vector3(p.x, WorldTerrainGenerator.SampleHeight(p.x, p.z) + 0.04f, p.z) + cam.transform.forward * 4f;
                Atur(_percik, rp);
                _kunang.transform.position = p + cam.transform.forward * 6f + Vector3.up * 1.1f;
                Atur(_kunang, rk);
                _daun.transform.position = p + Vector3.up * 9f;
                _debu.transform.position = p + cam.transform.forward * 5f;
                Atur(_daun, rd); Atur(_debu, rb);
            }
            catch (Exception e) { _gagal = true; Debug.LogError("[AK][PartikelAlam] Gagal: " + e.Message); }
        }

        ParticleSystem BuatKunang()
        {
            var go = new GameObject("KunangMalam");
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = true; main.simulationSpace = ParticleSystemSimulationSpace.World; main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.2f, 4.0f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.075f);
            main.maxParticles = 120;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.75f, 1.0f, 0.42f, 0.88f), new Color(0.45f, 0.96f, 0.85f, 0.80f));
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(18f, 4.5f, 18f);
            var noise = ps.noise; noise.enabled = true; noise.strength = 0.65f; noise.frequency = 0.8f; noise.scrollSpeed = 0.45f;
            var col = ps.colorOverLifetime; col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.25f), new GradientAlphaKey(0.8f, 0.7f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Billboard; r.sharedMaterial = debu;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
            return ps;
        }

        ParticleSystem BuatPercik()
        {
            var go = new GameObject("PercikHujan");
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = true; main.simulationSpace = ParticleSystemSimulationSpace.World; main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.12f, 0.22f); main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.11f); main.maxParticles = 220;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.85f, 0.9f, 1f, 0.45f));
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(16f, 0.4f, 16f);
            var sz = ps.sizeOverLifetime; sz.enabled = true; sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.4f, 1f, 1.6f));
            var col = ps.colorOverLifetime; col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.HorizontalBillboard; r.sharedMaterial = debu;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
            return ps;
        }

        static void Atur(ParticleSystem ps, float laju)
        {
            var em = ps.emission; em.rateOverTime = laju;
            if (laju > 0f && !ps.isPlaying) ps.Play();
        }
    }
}
