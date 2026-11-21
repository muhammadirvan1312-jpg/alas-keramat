using UnityEngine;
namespace AlasKeramat.Core
{
    // Akses bank klip terpanggang, dipakai Builder untuk mengganti klip sintetis dengan rekaman CC0 (Kenney).
    public partial class ProceduralAudioSynth
    {
        public string[] NamaPanggang { get { return clipNames; } }
        public AudioClip[] KlipPanggang { get { return clips; } }
        public const string KlipHujan = "rekaman_hujan";

        // Hanya klip terpanggang (rekaman), tanpa cadangan sintetis; null bila tidak ada.
        public AudioClip AmbilRekaman(string nama)
        {
            AudioClip c;
            return _peta.TryGetValue(nama, out c) ? c : null;
        }
    }
}
