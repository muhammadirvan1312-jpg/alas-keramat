using UnityEngine;
namespace AlasKeramat.AI
{
    // Bank animasi mocap Mixamo (klip legacy) + kerangka proksi tanpa mesh. Dibuat Builder dari Assets/Mixamo/*.fbx.
    public class BankAnimasiMixamo : ScriptableObject
    {
        public GameObject proksi;
        public AnimationClip[] klip = new AnimationClip[0];
        public float tinggiPinggul = 1f;

        public AnimationClip Cari(string nama)
        {
            if (string.IsNullOrEmpty(nama)) return null;
            for (int i = 0; i < klip.Length; i++) if (klip[i] != null && klip[i].name == nama) return klip[i];
            return null;
        }
    }
}
