using UnityEngine;
namespace AlasKeramat.Core
{
    // Pintu masuk/keluar kompleks bawah tanah (03 §10): interaksi -> fade 1,2 s -> teleport ke titik tujuan.
    public class PintuGua : MonoBehaviour, IInteractable
    {
        [SerializeField] string prompt = "MASUK GUA";
        [SerializeField] Vector3 tujuan;
        [SerializeField] float yaw;
        [SerializeField] int syaratBendera;     // 0 = bebas; selain itu bendera WorldCaveAndSaveManager
        [SerializeField] bool perluBossKalah;
        [SerializeField] string pesanTerkunci = "";

        public Vector3 Tujuan { get { return tujuan; } }
        public bool PerluBossKalah { get { return perluBossKalah; } }
        public void Pasang(string p, Vector3 ke, float arah, int syarat, bool boss, string terkunci)
        { prompt = p; tujuan = ke; yaw = arah; syaratBendera = syarat; perluBossKalah = boss; pesanTerkunci = terkunci; }

        public float HoldDuration { get { return 0.3f; } }
        bool Terbuka()
        {
            var m = GameServices.Get<WorldCaveAndSaveManager>();
            if (syaratBendera != 0 && (m == null || !m.Ada(syaratBendera))) return false;
            if (perluBossKalah) { var q = GameServices.Get<QuestManager>(); if (q == null || !q.Progres.bossKalah) return false; }
            return true;
        }
        public string GetPromptText(GameObject actor) { return Terbuka() ? prompt : (string.IsNullOrEmpty(pesanTerkunci) ? "" : pesanTerkunci); }
        public bool CanInteract(GameObject actor) { var m = GameServices.Get<WorldCaveAndSaveManager>(); return m != null && !m.SedangPindah && (Terbuka() || !string.IsNullOrEmpty(pesanTerkunci)); }
        public void Interact(GameObject actor)
        {
            if (!Terbuka()) { GameEvents.RaiseToast(pesanTerkunci); return; }
            var m = GameServices.Get<WorldCaveAndSaveManager>();
            if (m != null) m.Pindah(tujuan, yaw);
        }
    }
}
