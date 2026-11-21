using UnityEngine;
namespace AlasKeramat.Core
{
    // Akses bank klip terpanggang, dipakai Builder untuk mengganti klip sintetis dengan rekaman CC0 (Kenney).
    public partial class ProceduralAudioSynth
    {
        public string[] NamaPanggang { get { return clipNames; } }
        public AudioClip[] KlipPanggang { get { return clips; } }
        public const string KlipHujan = "rekaman_hujan", KlipApi = "rekaman_api";

        // Hanya klip terpanggang (rekaman), tanpa cadangan sintetis; null bila tidak ada.
        public AudioClip AmbilRekaman(string nama)
        {
            AudioClip c;
            if (_peta.TryGetValue(nama, out c)) return c;
            // Sebelum Awake (Editor/self-test) peta masih kosong: cari langsung di bank terpanggang.
            if (clipNames != null && clips != null)
                for (int i = 0; i < clipNames.Length && i < clips.Length; i++) if (clipNames[i] == nama) return clips[i];
            return null;
        }
    }
}
