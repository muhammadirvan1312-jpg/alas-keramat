using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Player
{
    // Foley Andi (21 §10 T3): langkah per permukaan sesuai jarak tempuh, mendarat, napas saat lelah, detak jantung saat takut/sekarat.
    public class FoleyAndi : MonoBehaviour
    {
        public const float LangkahJalan = 1.45f, LangkahLari = 1.75f, LangkahJongkok = 0.85f;
        public const float VolJalan = 0.45f, VolLari = 0.8f, VolJongkok = 0.18f, AmbangNapas = 30f, AmbangTakut = 25f;
        const float JedaNapas = 2.2f, BpmMin = 70f, BpmMaks = 135f;

        AndiFPSController _ctrl;
        AndiVitalsAndSanity _v;
        ProceduralAudioSynth _synth;
        float _jarak, _tNapas, _tJantung, _sejakLangkah;
        int _var;
        bool _diTanahLalu = true;
        Vector3 _posLalu;
        ParticleSystem _psLangkah;

        public static int JumlahPartikelLangkah(SurfaceType s, float vol)
        {
            if (vol < 0.35f) return 0;
            if (s == SurfaceType.Air) return Mathf.RoundToInt(10f * vol);
            if (s == SurfaceType.Pasir || s == SurfaceType.DaunKering || s == SurfaceType.Tanah) return Mathf.RoundToInt(6f * vol);
            return 0;
        }

        void PancarLangkah(SurfaceType s, Vector3 pos, float vol)
        {
            int n = JumlahPartikelLangkah(s, vol);
            if (n <= 0) return;
            if (_psLangkah == null)
            {
                var go = new GameObject("PartikelLangkah");
                go.transform.SetParent(transform, false);
                _psLangkah = go.AddComponent<ParticleSystem>();
                _psLangkah.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var m = _psLangkah.main;
                m.playOnAwake = false; m.loop = false; m.startLifetime = 0.38f;
                m.startSpeed = 1.8f; m.startSize = 0.08f; m.gravityModifier = 0.65f;
                m.simulationSpace = ParticleSystemSimulationSpace.World; m.maxParticles = 64;
                var sh = _psLangkah.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.18f;
                var pr = go.GetComponent<ParticleSystemRenderer>();
                pr.sharedMaterial = SafeMaterialFactory.GetCached("M_DebuLangkah", new Color(0.72f, 0.68f, 0.60f), 0f, 0.2f);
                pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            var main = _psLangkah.main;
            main.startColor = s == SurfaceType.Air ? new Color(0.80f, 0.90f, 0.98f, 0.75f) : new Color(0.56f, 0.46f, 0.34f, 0.55f);
            _psLangkah.transform.position = pos + Vector3.up * 0.08f;
            _psLangkah.Emit(n);
        }

        void Awake() { _ctrl = GetComponent<AndiFPSController>(); _posLalu = transform.position; }

        public static SurfaceType Permukaan(Biome b, bool diAir, bool diStruktur)
        {
            if (diAir) return SurfaceType.Air;
            if (diStruktur) return SurfaceType.Kayu;
            if (b == Biome.Pantai) return SurfaceType.Pasir;
            if (b == Biome.BawahTanah || b == Biome.TebingKapur || b == WorldTerrainGenerator.GunungKapur) return SurfaceType.BatuGua;
            if (b == Biome.HutanJatiSelatan || b == Biome.SawitTerbakar || b == Biome.HutanBambu) return SurfaceType.DaunKering;
            return SurfaceType.Tanah;
        }

        void Update()
        {
            if (_ctrl == null) return;
            if (_synth == null) _synth = GameServices.Get<ProceduralAudioSynth>();
            if (_v == null) _v = GameServices.Get<AndiVitalsAndSanity>();
            if (_synth == null) return;
            Vector3 p = transform.position;
            Vector3 d = p - _posLalu; d.y = 0f; _posLalu = p;
            if (Time.deltaTime <= 0f) return;
            if (d.sqrMagnitude > 9f) { _jarak = 0f; _diTanahLalu = _ctrl.DiTanah; return; }
            _sejakLangkah += Time.deltaTime;
            if (_ctrl.DiTanah && !_ctrl.Berenang)
            {
                _jarak += d.magnitude;
                float jarakLangkah = _ctrl.Jongkok ? LangkahJongkok : _ctrl.SedangLari ? LangkahLari : LangkahJalan;
                if (!_diTanahLalu && _ctrl.TinggiJatuhTerakhir > 0.8f) { _jarak = 0f; _sejakLangkah = 0f; Langkah(Mathf.Clamp01(0.5f + _ctrl.TinggiJatuhTerakhir * 0.12f)); }
                else if (_jarak >= jarakLangkah && _sejakLangkah >= 0.24f)
                { _jarak %= jarakLangkah; _sejakLangkah = 0f; Langkah(_ctrl.Jongkok ? VolJongkok : _ctrl.SedangLari ? VolLari : VolJalan); }
            }
            else if (_ctrl.Berenang)
            {
                _jarak += d.magnitude;
                if (_jarak >= 1.4f) { _jarak = 0f; Mainkan(ProceduralAudioSynth.NamaLangkah(SurfaceType.Air, _var++), p, 0.5f); PancarLangkah(SurfaceType.Air, p, 0.55f); }
            }
            if (d.sqrMagnitude < 0.0001f) _jarak = 0f;
            _diTanahLalu = _ctrl.DiTanah;
            if (_v == null) return;
            _tNapas -= Time.deltaTime;
            if (_v.Stamina < AmbangNapas && _tNapas <= 0f) { _tNapas = JedaNapas; _synth.Play(ProceduralAudioSynth.KlipNapas, null, AudioCategory.Efek, Mathf.Lerp(0.6f, 0.2f, _v.Stamina / AmbangNapas), Random.Range(0.95f, 1.05f), 2); }
            float takut = Mathf.Max(Mathf.InverseLerp(AmbangTakut, 0f, _v.Sanity), Mathf.InverseLerp(AmbangTakut, 0f, _v.Health));
            if (takut > 0f)
            {
                _tJantung -= Time.deltaTime;
                if (_tJantung <= 0f) { _tJantung = 60f / Mathf.Lerp(BpmMin, BpmMaks, takut); _synth.Play(ProceduralAudioSynth.KlipJantung, null, AudioCategory.Efek, 0.35f + 0.5f * takut, 1f, 3); }
            }
        }

        void Langkah(float vol)
        {
            Vector3 p = transform.position;
            bool struktur = Physics.Raycast(p + Vector3.up * 0.3f, Vector3.down, 0.6f, 1 << GameLayers.Structure, QueryTriggerInteraction.Ignore);
            float? wl = WorldTerrainGenerator.GetWaterLevelAt(p.x, p.z);
            var s = Permukaan(WorldTerrainGenerator.GetBiome(p.x, p.z), wl.HasValue && wl.Value > p.y, struktur);
            Mainkan(ProceduralAudioSynth.NamaLangkah(s, _var++), p, vol);
            PancarLangkah(s, p, vol);
        }

        void Mainkan(string nama, Vector3 p, float vol) { _synth.Play(nama, null, AudioCategory.Efek, vol, Random.Range(0.96f, 1.04f), 2); }
    }
}
