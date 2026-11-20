using System.Collections.Generic;
using UnityEngine;

namespace AlasKeramat.Core
{
    // Satu sumber skala waktu: menutup tas tidak membatalkan jeda peta/menu/sinema.
    public static class JedaDunia
    {
        static readonly HashSet<int> Pemilik = new HashSet<int>();
        static float _akhirHitStop;
        public static bool Dijeda { get { return Pemilik.Count > 0; } }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetSesi() { Pemilik.Clear(); _akhirHitStop = 0f; Time.timeScale = 1f; }

        public static void Atur(int pemilik, bool jeda)
        {
            if (jeda) Pemilik.Add(pemilik); else Pemilik.Remove(pemilik);
            Tick();
        }

        public static void HitStop(float detik)
        {
            if (!Dijeda) _akhirHitStop = Mathf.Max(_akhirHitStop, Time.realtimeSinceStartup + detik);
            Tick();
        }

        public static void Tick()
        {
            float skala = Dijeda ? 0f : Time.realtimeSinceStartup < _akhirHitStop ? 0.05f : 1f;
            if (!Mathf.Approximately(Time.timeScale, skala)) Time.timeScale = skala;
        }
    }
}
