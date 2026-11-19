using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEngine;
namespace AlasKeramat.UI
{
    // Tiap panel memiliki kunci sendiri; menutup satu panel tidak membuka panel lain.
    public class PengunciInput : MonoBehaviour
    {
        static readonly HashSet<int> Pemilik = new HashSet<int>();
        public static bool Terblokir { get { return Time.timeScale <= 0f || Pemilik.Count > 0; } }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetKunci() { Pemilik.Clear(); }
        public static void Pasang(GameObject panel)
        {
            if (panel != null && panel.GetComponent<PengunciInput>() == null) panel.AddComponent<PengunciInput>();
        }
        void OnEnable() { Pemilik.Add(GetInstanceID()); Bersihkan(); }
        void OnDisable() { Pemilik.Remove(GetInstanceID()); Bersihkan(); }
        static void Bersihkan()
        {
            var sentuh = GameServices.Get<TouchControls>();
            if (sentuh != null) sentuh.HentikanInput();
            var interaksi = GameServices.Get<AlasKeramat.Player.InteraksiAndi>();
            if (interaksi != null) interaksi.LepasTahan();
        }
    }
}
