using UnityEngine;
namespace AlasKeramat.Core
{
    // Akses bank klip terpanggang, dipakai Builder untuk mengganti klip sintetis dengan rekaman CC0 (Kenney).
    public partial class ProceduralAudioSynth
    {
        public string[] NamaPanggang { get { return clipNames; } }
        public AudioClip[] KlipPanggang { get { return clips; } }
    }
}
