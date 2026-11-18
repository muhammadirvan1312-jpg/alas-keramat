using UnityEngine;
namespace AlasKeramat.Core
{
    // Kotak air bawah tanah (kolam & terowongan selam UG1). Muka air = puncak kotak. Didaftarkan ke WorldTerrainGenerator saat aktif.
    public class AirGua : MonoBehaviour
    {
        [SerializeField] Vector3 ukuran = Vector3.one;
        public void Pasang(Vector3 u) { ukuran = u; }
        public Bounds Kotak { get { return new Bounds(transform.position, ukuran); } }
        void OnEnable() { WorldTerrainGenerator.DaftarAirGua(Kotak); }
    }
}
