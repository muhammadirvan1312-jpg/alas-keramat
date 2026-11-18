using UnityEngine;
namespace AlasKeramat.Core
{
    // Jalan Pintas T2 UG1 (03 §10): reruntuhan hanya bisa didorong dari sisi Ruang Altar agar tak perlu menyelam lagi.
    public class RintanganDorong : MonoBehaviour, IInteractable
    {
        [SerializeField] Vector3 sisiBoleh = Vector3.right; // arah dunia dari reruntuhan ke sisi yang boleh mendorong
        int _versi = -1;
        public void Pasang(Vector3 sisi) { sisiBoleh = sisi.normalized; }
        public float HoldDuration { get { return 1.2f; } }
        WorldCaveAndSaveManager M { get { return GameServices.Get<WorldCaveAndSaveManager>(); } }
        public static bool BolehDari(Vector3 posRintangan, Vector3 sisi, Vector3 posPelaku) { Vector3 d = posPelaku - posRintangan; d.y = 0f; return Vector3.Dot(d, sisi) > 0f; }
        public string GetPromptText(GameObject actor)
        {
            var m = M; if (m == null || m.Ada(WorldCaveAndSaveManager.B_Rintangan)) return "";
            return actor != null && BolehDari(transform.position, sisiBoleh, actor.transform.position) ? "DORONG RERUNTUHAN" : "TERHALANG RERUNTUHAN";
        }
        public bool CanInteract(GameObject actor) { var m = M; return m != null && !m.Ada(WorldCaveAndSaveManager.B_Rintangan); }
        public void Interact(GameObject actor)
        {
            var m = M; if (m == null) return;
            if (actor == null || !BolehDari(transform.position, sisiBoleh, actor.transform.position)) { GameEvents.RaiseToast("Terlalu berat didorong dari sisi ini."); return; }
            m.Set(WorldCaveAndSaveManager.B_Rintangan);
            GameEvents.RaiseToast("Reruntuhan bergeser. Jalan pintas terbuka.");
        }
        void Update()
        {
            var m = M; if (m == null || m.Versi == _versi) return;
            _versi = m.Versi;
            bool buka = m.Ada(WorldCaveAndSaveManager.B_Rintangan);
            for (int i = 0; i < transform.childCount; i++) transform.GetChild(i).gameObject.SetActive(!buka);
            var c = GetComponent<Collider>(); if (c != null) c.enabled = !buka;
        }
    }
}
