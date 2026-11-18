using UnityEngine;
namespace AlasKeramat.Core
{
    // Dengung & tetes air per kompleks (diambil dari versi T10a ChatGPT): diputar saat Andi masuk kompleks ini.
    public class AudioGua : MonoBehaviour
    {
        public const float Volume = 0.4f;
        [SerializeField] UndergroundId kompleks;
        [SerializeField] AudioSource sumber;
        public void Pasang(UndergroundId id, AudioSource audio) { kompleks = id; sumber = audio; }
        void OnEnable() { GameEvents.OnEnterUnderground += Masuk; GameEvents.OnExitUnderground += Keluar; }
        void OnDisable() { GameEvents.OnEnterUnderground -= Masuk; GameEvents.OnExitUnderground -= Keluar; Keluar(); }
        void Masuk(UndergroundId id) { if (sumber == null) return; sumber.volume = Volume * ProceduralAudioSynth.VolMaster; if (id == kompleks) sumber.Play(); else sumber.Stop(); }
        void Keluar() { if (sumber != null) sumber.Stop(); }
    }
}
