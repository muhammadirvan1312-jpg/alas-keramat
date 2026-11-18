using AlasKeramat.Core;
using UnityEngine;
namespace AlasKeramat.AI
{
    // Akar penutup Ruang Emas Majapahit UG3 K4 (03 §10): hanya terbakar oleh api (Panah Api, Bom Bambu, suar).
    public class AkarBakar : MonoBehaviour, IDamageable
    {
        int _versi = -1;
        public bool IsAlive { get { var m = GameServices.Get<WorldCaveAndSaveManager>(); return m == null || !m.Ada(WorldCaveAndSaveManager.B_AkarK4); } }
        public void TakeDamage(DamageInfo info) { if (info.type == DamageType.Api) Bakar(); }
        public void Bakar()
        {
            var m = GameServices.Get<WorldCaveAndSaveManager>();
            if (m == null || m.Ada(WorldCaveAndSaveManager.B_AkarK4)) return;
            m.Set(WorldCaveAndSaveManager.B_AkarK4);
            ApiArea.Buat(transform.position);
            GameEvents.RaiseToast("Akar hitam terbakar habis. Ada ruangan di baliknya.");
        }
        void Update()
        {
            var m = GameServices.Get<WorldCaveAndSaveManager>();
            if (m == null || m.Versi == _versi) return;
            _versi = m.Versi;
            bool habis = m.Ada(WorldCaveAndSaveManager.B_AkarK4);
            for (int i = 0; i < transform.childCount; i++) transform.GetChild(i).gameObject.SetActive(!habis);
            var c = GetComponent<Collider>(); if (c != null) c.enabled = !habis;
        }
    }
}
