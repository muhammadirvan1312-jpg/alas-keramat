using UnityEngine;
namespace AlasKeramat.Building
{
    public class JeratanPunji : MonoBehaviour
    {
        float _sisa; Vector3 _posisi;
        public void Tahan(float detik) { _sisa=detik; _posisi=transform.position; }
        void LateUpdate() { if(_sisa<=0f)return; _sisa-=Time.deltaTime; transform.position=_posisi; }
    }
}
