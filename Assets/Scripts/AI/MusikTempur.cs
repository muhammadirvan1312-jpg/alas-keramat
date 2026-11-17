using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Musik tempur dasar (T7): loop gamelan prosedural naik 1,5 s saat Sekte menyerang/menyerbu dalam 45 m atau hantu aktif dalam 40 m,
    // turun 4 s setelah aman. Musik adaptif penuh & leitmotif menyusul T10 (21 §8).
    public class MusikTempur : MonoBehaviour
    {
        public const float RadiusSekte = 45f, RadiusHantu = 40f, Naik = 1.5f, Turun = 4f, Tahan = 4f, VolumeMaks = 0.7f;

        AudioSource _src;
        AndiFPSController _andi;
        float _tAman, _tCek, _vol;
        bool _tegang;

        public bool Tegang { get { return _tegang; } }

        void Awake()
        {
            _src = gameObject.AddComponent<AudioSource>();
            _src.loop = true; _src.playOnAwake = false; _src.spatialBlend = 0f; _src.volume = 0f; _src.priority = 32;
        }

        public static bool AdaAncaman(Vector3 andi)
        {
            var s = NusantaraEnemyAndMutantAI.Semua;
            for (int i = 0; i < s.Count; i++)
            {
                var k = s[i].Keadaan;
                if ((k == NusantaraEnemyAndMutantAI.KeadaanSekte.Serang || k == NusantaraEnemyAndMutantAI.KeadaanSekte.Gertak || k == NusantaraEnemyAndMutantAI.KeadaanSekte.Serbu)
                    && (s[i].transform.position - andi).sqrMagnitude < RadiusSekte * RadiusSekte) return true;
            }
            var h = HantuAI.Semua;
            for (int i = 0; i < h.Count; i++)
                if (h[i].IsAlive && h[i].Fase != HantuAI.FaseHantu.Diam && h[i].Fase != HantuAI.FaseHantu.Hilang && (h[i].transform.position - andi).sqrMagnitude < RadiusHantu * RadiusHantu) return true;
            return false;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            _tCek -= dt;
            if (_tCek <= 0f)
            {
                _tCek = 0.5f;
                if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
                bool ancam = _andi != null && AdaAncaman(_andi.transform.position);
                if (ancam) { _tegang = true; _tAman = 0f; } else { _tAman += 0.5f; if (_tAman >= Tahan) _tegang = false; }
                if (_tegang && _src.clip == null)
                {
                    var a = GameServices.Get<ProceduralAudioSynth>();
                    if (a != null) _src.clip = a.GetClip(ProceduralAudioSynth.KlipMusikTempur);
                }
            }
            _vol = Mathf.MoveTowards(_vol, _tegang ? 1f : 0f, dt / (_tegang ? Naik : Turun));
            _src.volume = _vol * VolumeMaks * ProceduralAudioSynth.VolumeKategori(AudioCategory.Musik);
            if (_vol > 0.001f && !_src.isPlaying && _src.clip != null) _src.Play();
            else if (_vol <= 0.001f && _src.isPlaying) _src.Stop();
        }
    }
}
