using AlasKeramat.Core;
using UnityEngine;
namespace AlasKeramat.Building
{
    public class IdentitasBangunan : MonoBehaviour
    {
        public BlueprintID Id;
        public float Bentang = 6f;
        public string Kunci = System.Guid.NewGuid().ToString("N");
    }
}
