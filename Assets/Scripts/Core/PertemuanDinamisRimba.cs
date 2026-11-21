using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.Core
{
    // Pertemuan & kejadian alam spontan saat Andi menjelajah (geraman predator berburu, angin kencang, kentongan malam, buah jatuh).
    public class PertemuanDinamisRimba : MonoBehaviour
    {
        public const float JedaMin = 65f, JedaMaks = 110f;

        float _sisa = 45f;
        int _urutan;
        AndiFPSController _andi;
        TimeOfDayClock _jam;

        public static string PesanKejadian(int jenis, bool malam)
        {
            if (malam && (jenis % 2 == 0)) return "(Terdengar bunyi kentongan kayu samar dari arah desa...)";
            switch (Mathf.Abs(jenis) % 4)
            {
                case 0: return "(Geraman pemangsa menggema di balik pepohonan rimba...)";
                case 1: return "(Hembusan angin kencang menggoyangkan tajuk pepohonan di atasmu.)";
                case 2: return "(Sekawanan burung kaget beterbangan dari puncak pohon!)";
                default: return "(Suara ranting patah terdengar di kejauhan...)";
            }
        }

        void Update()
        {
            if (JedaDunia.Dijeda) return;
            _sisa -= Time.deltaTime;
            if (_sisa > 0f) return;
            _sisa = Random.Range(JedaMin, JedaMaks);

            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            if (_andi == null || WorldTerrainGenerator.DiBawahTanah(_andi.transform.position.y)) return;
            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();

            bool malam = _jam != null && (_jam.Jam >= 18.2f || _jam.Jam < 5.5f);
            int idx = _urutan++;
            GameEvents.RaiseSubtitle(PesanKejadian(idx, malam));

            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth == null) return;
            Vector3 p = _andi.transform.position + Quaternion.Euler(0f, idx * 97f, 0f) * Vector3.forward * 28f;
            string klip = malam && (idx % 2 == 0) ? ProceduralAudioSynth.KlipKentongan
                : (idx % 4 == 0 ? ProceduralAudioSynth.KlipGeramSekte : ProceduralAudioSynth.KlipPohonTumbang);
            synth.Play(klip, p, AudioCategory.Ambien, 0.42f, Random.Range(0.85f, 1.12f), 1, 65f);
        }
    }
}
