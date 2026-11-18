using AlasKeramat.Core;
using UnityEngine;
namespace AlasKeramat.World
{
    public class AudioGua : MonoBehaviour
    {
        [SerializeField] UndergroundId kompleks;
        [SerializeField] AudioSource sumber;
        public void Pasang(UndergroundId id,AudioSource audio) { kompleks=id;sumber=audio; }
        void OnEnable() { GameEvents.OnEnterUnderground+=Masuk;GameEvents.OnExitUnderground+=Keluar; }
        void OnDisable() { GameEvents.OnEnterUnderground-=Masuk;GameEvents.OnExitUnderground-=Keluar;Keluar(); }
        void Masuk(UndergroundId id) { if(sumber==null)return; if(id==kompleks)sumber.Play();else sumber.Stop(); }
        void Keluar() { if(sumber!=null)sumber.Stop(); }
        void Update() { if(sumber!=null)sumber.volume=0.4f*ProceduralAudioSynth.VolMaster; }
    }
}
