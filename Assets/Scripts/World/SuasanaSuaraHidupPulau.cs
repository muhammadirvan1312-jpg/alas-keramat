using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.World
{
    // Lanskap suara hidup 24 jam berbasis lokasi & waktu pulau menggunakan rekaman CC0/synth ProceduralAudioSynth:
    // - Fajar (05:00-08:00): kokok ayam kampung di sekitar Kampung/Desa/Heli
    // - Siang (08:00-17:00): burung kedasih & suara rimba tropis di lembah/air terjun/hutan
    // - Maghrib (17:15-19:15): tabuhan kentongan & gamelan jauh keramat di Altar/Desa/Candi
    // - Malam (19:15-05:00): bisikan hutan, geraman jauh & gemericik api unggun
    public class SuasanaSuaraHidupPulau : MonoBehaviour
    {
        [SerializeField] Vector3[] titikDesa = new Vector3[0];
        [SerializeField] Vector3[] titikKeramat = new Vector3[0];

        Transform _kamera;
        TimeOfDayClock _jam;
        ProceduralAudioSynth _audio;
        float _jedaBerikut = 6f;

        public int JumlahTitikSuara
        {
            get { return (titikDesa != null ? titikDesa.Length : 0) + (titikKeramat != null ? titikKeramat.Length : 0); }
        }

        public void Pasang(Vector3[] desa, Vector3[] keramat)
        {
            titikDesa = desa ?? new Vector3[0];
            titikKeramat = keramat ?? new Vector3[0];
        }

        public static string PilihKlipAmbienWaktu(float jam, bool dekatDesa, bool dekatKeramat)
        {
            if (jam >= 5.0f && jam < 8.0f) return dekatDesa ? ProceduralAudioSynth.KlipKokok : ProceduralAudioSynth.KlipKedasih;
            if (jam >= 17.2f && jam < 19.3f) return dekatKeramat ? ProceduralAudioSynth.KlipGamelanJauh : ProceduralAudioSynth.KlipKentongan;
            if (jam >= 19.3f || jam < 5.0f) return dekatKeramat ? ProceduralAudioSynth.KlipBisikan : ProceduralAudioSynth.KlipKedasih;
            return ProceduralAudioSynth.KlipKedasih;
        }

        void Update()
        {
            if (!Application.isPlaying) return;
            _jedaBerikut -= Time.deltaTime;
            if (_jedaBerikut > 0f) return;
            _jedaBerikut = UnityEngine.Random.Range(14f, 26f);

            if (_kamera == null) { var c = Camera.main; if (c != null) _kamera = c.transform; }
            if (_kamera == null) return;
            Vector3 cPos = _kamera.position;
            if (WorldTerrainGenerator.DiBawahTanah(cPos.y)) return;

            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            if (_audio == null) _audio = GameServices.Get<ProceduralAudioSynth>();
            if (_audio == null) return;

            float jam = _jam != null ? _jam.Jam : 10f;
            bool dekatDesa = AdaTitikDekat(titikDesa, cPos, 95f);
            bool dekatKeramat = AdaTitikDekat(titikKeramat, cPos, 110f);
            string klip = PilihKlipAmbienWaktu(jam, dekatDesa, dekatKeramat);
            float ang = UnityEngine.Random.value * Mathf.PI * 2f;
            float r = UnityEngine.Random.Range(18f, 38f);
            Vector3 posSuara = cPos + new Vector3(Mathf.Cos(ang) * r, 3f, Mathf.Sin(ang) * r);
            _audio.Play(klip, posSuara, AudioCategory.Ambien, 0.36f, UnityEngine.Random.Range(0.92f, 1.08f), 1, 75f);
        }

        static bool AdaTitikDekat(Vector3[] daftar, Vector3 cPos, float batas)
        {
            if (daftar == null) return false;
            float bSq = batas * batas;
            for (int i = 0; i < daftar.Length; i++)
            {
                Vector3 d = daftar[i] - cPos;
                if (d.x * d.x + d.z * d.z < bSq) return true;
            }
            return false;
        }
    }
}
