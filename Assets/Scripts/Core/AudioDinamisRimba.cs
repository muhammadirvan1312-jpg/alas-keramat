using UnityEngine;

namespace AlasKeramat.Core
{
    // Lapisan audio ambien spasial untuk mega-landmark baru (air terjun, candi kuno malam, jembatan gantung, bangkai kapal).
    public class AudioDinamisRimba : MonoBehaviour
    {
        public const float JedaPutar = 6.5f;

        [SerializeField] Vector3 posAirTerjun, posCandi, posJembatan, posKapal;
        float _t;
        Transform _cam;
        TimeOfDayClock _jam;
        ProceduralAudioSynth _synth;

        public void Pasang(Vector3 airTerjun, Vector3 candi, Vector3 jembatan, Vector3 kapal)
        {
            posAirTerjun = airTerjun; posCandi = candi; posJembatan = jembatan; posKapal = kapal;
        }

        public static string PilihKlipLandmark(Vector3 camPos, Vector3 airTerjun, Vector3 candi, Vector3 jembatan, Vector3 kapal, float jam, out Vector3 sumber, out float pitch)
        {
            sumber = camPos; pitch = 1f;
            if ((camPos - airTerjun).sqrMagnitude < 45f * 45f) { sumber = airTerjun; pitch = 0.72f; return ProceduralAudioSynth.KlipGuruh; }
            if ((camPos - candi).sqrMagnitude < 42f * 42f && (jam >= 18f || jam < 5.5f)) { sumber = candi; pitch = 0.85f; return ProceduralAudioSynth.KlipKentongan; }
            if ((camPos - jembatan).sqrMagnitude < 28f * 28f) { sumber = jembatan; pitch = 0.92f; return ProceduralAudioSynth.KlipAmbilKayu; }
            if ((camPos - kapal).sqrMagnitude < 36f * 36f) { sumber = kapal; pitch = 0.78f; return ProceduralAudioSynth.KlipAmbilKayu; }
            return null;
        }

        void Update()
        {
            if (JedaDunia.Dijeda) return;
            _t -= Time.deltaTime;
            if (_t > 0f) return;
            _t = JedaPutar;

            if (_cam == null) { var c = Camera.main; if (c != null) _cam = c.transform; }
            if (_cam == null) return;
            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            if (_synth == null) _synth = GameServices.Get<ProceduralAudioSynth>();
            if (_synth == null) return;

            Vector3 sumber; float pitch;
            string klip = PilihKlipLandmark(_cam.position, posAirTerjun, posCandi, posJembatan, posKapal, _jam != null ? _jam.Jam : 12f, out sumber, out pitch);
            if (!string.IsNullOrEmpty(klip))
                _synth.Play(klip, sumber, AudioCategory.Ambien, 0.34f, pitch, 1, 52f);
        }
    }
}
