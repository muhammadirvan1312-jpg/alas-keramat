using System.Collections.Generic;
using UnityEngine;
namespace AlasKeramat.Core
{
    // Bawah tanah (H6, 03 §10): di Y = -80 tinggi permukaan tidak berlaku. Lantai dicari lewat raycast ke layer Ground,
    // air gua dari daftar kotak air yang didaftarkan kompleks saat dimuat.
    public static partial class WorldTerrainGenerator
    {
        public const float BatasBawahTanah = -40f, AngkatRaycast = 1.5f, JangkauRaycast = 8f, JatuhBebas = 100f;
        static readonly List<Bounds> _airGua = new List<Bounds>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetBawah() { _airGua.Clear(); }

        public static bool DiBawahTanah(float y) { return y < BatasBawahTanah; }

        public static float Lantai(Vector3 p)
        {
            if (!DiBawahTanah(p.y)) return SampleHeight(p.x, p.z);
            RaycastHit h;
            if (Physics.Raycast(p + Vector3.up * AngkatRaycast, Vector3.down, out h, JangkauRaycast, 1 << GameLayers.Ground, QueryTriggerInteraction.Ignore)) return h.point.y;
            return p.y - JatuhBebas;
        }

        // Permukaan air: kotak.max.y = muka air.
        public static void DaftarAirGua(Bounds b) { if (!_airGua.Contains(b)) _airGua.Add(b); }
        public static int JumlahAirGua { get { return _airGua.Count; } }

        public static float? AirDi(Vector3 p)
        {
            if (!DiBawahTanah(p.y)) return GetWaterLevelAt(p.x, p.z);
            for (int i = 0; i < _airGua.Count; i++)
            {
                var b = _airGua[i];
                if (p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z && p.y <= b.max.y + 2f && p.y >= b.min.y - 2f) return b.max.y;
            }
            return null;
        }
    }
}
