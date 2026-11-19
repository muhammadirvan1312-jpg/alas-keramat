using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEngine;
namespace AlasKeramat.UI
{
    // Tiap panel memiliki kunci sendiri; menutup satu panel tidak membuka panel lain.
    public class PengunciInput : MonoBehaviour
    {
        static readonly HashSet<int> Pemilik = new HashSet<int>();
        public static bool Terblokir
        {
            get
            {
                var vital = GameServices.Get<AlasKeramat.Player.AndiVitalsAndSanity>();
                return Time.timeScale <= 0f || Pemilik.Count > 0 || (vital != null && !vital.IsAlive);
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetKunci() { Pemilik.Clear(); JedaPanel = true; }
        public static void Pasang(GameObject panel)
        {
            if (panel != null && panel.GetComponent<PengunciInput>() == null) panel.AddComponent<PengunciInput>();
        }
        public static bool JedaPanel = true;
        public static void AturJedaPanel(bool aktif)
        {
            JedaPanel = aktif;
            foreach (int id in Pemilik) JedaDunia.Atur(id, aktif);
        }
        void OnEnable() { Pemilik.Add(GetInstanceID()); JedaDunia.Atur(GetInstanceID(), JedaPanel); Bersihkan(); }
        void OnDisable() { Pemilik.Remove(GetInstanceID()); JedaDunia.Atur(GetInstanceID(), false); Bersihkan(); }
        static void Bersihkan()
        {
            var sentuh = GameServices.Get<TouchControls>();
            if (sentuh != null) sentuh.HentikanInput();
            var interaksi = GameServices.Get<AlasKeramat.Player.InteraksiAndi>();
            if (interaksi != null) interaksi.LepasTahan();
        }
    }
}
