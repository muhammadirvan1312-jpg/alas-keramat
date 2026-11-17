using System.Collections.Generic;
using UnityEngine;

namespace AlasKeramat.AI
{
    // 06 §6-8: penangkal - asap Kemenyan (20 m, 60 s) dan Obor Tengkorak menyala (hantu 15 m; Sekte ragu & raid berhenti 25 m).
    // Sumber mendaftar sendiri (ApiUnggun saat BAKAR KEMENYAN, FungsiBangunan Obor Tengkorak saat menyala).
    public static class ZonaPenangkal
    {
        public const float LamaKemenyan = 60f, RadiusRaidObor = 25f;

        struct Asap { public Vector3 pos; public float sampai; }
        static readonly List<Asap> _asap = new List<Asap>();
        static readonly Dictionary<int, Vector3> _obor = new Dictionary<int, Vector3>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() { _asap.Clear(); _obor.Clear(); }

        public static void TambahKemenyan(Vector3 p) { _asap.Add(new Asap { pos = p, sampai = Time.time + LamaKemenyan }); }

        public static void SetObor(Object pemilik, Vector3 p, bool menyala)
        {
            if (pemilik == null) return;
            int id = pemilik.GetInstanceID();
            if (menyala) _obor[id] = p; else _obor.Remove(id);
        }

        public static int JumlahObor { get { return _obor.Count; } }

        public static bool AdaKemenyan(Vector3 p, float r)
        {
            for (int i = _asap.Count - 1; i >= 0; i--)
            {
                if (_asap[i].sampai < Time.time) { _asap.RemoveAt(i); continue; }
                if ((_asap[i].pos - p).sqrMagnitude <= r * r) return true;
            }
            return false;
        }

        public static bool AdaObor(Vector3 p, float r)
        {
            foreach (var kv in _obor) if ((kv.Value - p).sqrMagnitude <= r * r) return true;
            return false;
        }

        public static bool Ada(Vector3 p, float rKemenyan, float rObor) { return AdaKemenyan(p, rKemenyan) || AdaObor(p, rObor); }
    }
}
