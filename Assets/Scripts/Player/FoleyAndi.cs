using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Player
{
    // Foley Andi (21 §10 T3): langkah per permukaan sesuai jarak tempuh, mendarat, napas saat lelah, detak jantung saat takut/sekarat.
    public class FoleyAndi : MonoBehaviour
    {
        public const float LangkahJalan = 0.75f, LangkahLari = 1.1f, LangkahJongkok = 0.6f;
        public const float VolJalan = 0.45f, VolLari = 0.8f, VolJongkok = 0.18f, AmbangNapas = 30f, AmbangTakut = 25f;
        const float JedaNapas = 2.2f, BpmMin = 70f, BpmMaks = 135f;

        AndiFPSController _ctrl;
        AndiVitalsAndSanity _v;
        ProceduralAudioSynth _synth;
        float _jarak, _tNapas, _tJantung;
        int _var;
        bool _diTanahLalu = true;
        Vector3 _posLalu;

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
            if (_ctrl.DiTanah && !_ctrl.Berenang)
            {
                _jarak += d.magnitude;
                float jarakLangkah = _ctrl.Jongkok ? LangkahJongkok : _ctrl.SedangLari ? LangkahLari : LangkahJalan;
                if (_jarak >= jarakLangkah) { _jarak = 0f; Langkah(_ctrl.Jongkok ? VolJongkok : _ctrl.SedangLari ? VolLari : VolJalan); }
                if (!_diTanahLalu && _ctrl.TinggiJatuhTerakhir > 0.8f) Langkah(Mathf.Clamp01(0.5f + _ctrl.TinggiJatuhTerakhir * 0.12f));
            }
            else if (_ctrl.Berenang)
            {
                _jarak += d.magnitude;
                if (_jarak >= 1.4f) { _jarak = 0f; Mainkan(ProceduralAudioSynth.NamaLangkah(SurfaceType.Air, _var++), p, 0.5f); }
            }
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
        }

        void Mainkan(string nama, Vector3 p, float vol) { _synth.Play(nama, p, AudioCategory.Efek, vol, Random.Range(0.9f, 1.1f), 2); }
    }
}
