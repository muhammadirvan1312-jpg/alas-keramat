using System.Collections;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;
namespace AlasKeramat.Core
{
    // Jangkar zipline Jurang Kapur (03 Zona 6, 04 §4): TEMBAK TALI dengan Zipline Gun (1 Tali Zipline), lalu MELUNCUR antar-jangkar.
    public class JangkarZipline : MonoBehaviour, IInteractable
    {
        public const float Laju = 12f, Gantung = 1.7f;
        [SerializeField] JangkarZipline pasangan;
        [SerializeField] bool utama;
        [SerializeField] LineRenderer tali;
        int _versi = -1;
        static bool _meluncur;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetLuncur() { _meluncur = false; }

        public void Pasang(JangkarZipline lawan, bool jangkarUtama, LineRenderer garis) { pasangan = lawan; utama = jangkarUtama; tali = garis; }
        public Vector3 Titik { get { return transform.position; } }
        public float HoldDuration { get { return 0.3f; } }
        WorldCaveAndSaveManager M { get { return GameServices.Get<WorldCaveAndSaveManager>(); } }
        bool AdaTali { get { var m = M; return m != null && m.Ada(WorldCaveAndSaveManager.B_Tali); } }

        public string GetPromptText(GameObject actor)
        {
            if (_meluncur || pasangan == null) return "";
            if (AdaTali) return "MELUNCUR";
            return utama ? "TEMBAK TALI" : "";
        }
        public bool CanInteract(GameObject actor) { return !_meluncur && pasangan != null && (AdaTali || utama); }
        public void Interact(GameObject actor)
        {
            var m = M; if (m == null || pasangan == null) return;
            if (!AdaTali)
            {
                var inv = GameServices.Get<TikarPandan3DInventory>();
                if (inv == null || !inv.Punya(ItemID.ZiplineGun)) { GameEvents.RaiseToast("Butuh Zipline Gun untuk menembakkan tali ke seberang."); return; }
                if (!inv.Kurangi(ItemID.TaliZipline, 1)) { GameEvents.RaiseToast("Butuh Tali Zipline."); return; }
                m.Set(WorldCaveAndSaveManager.B_Tali);
                GameEvents.RaiseToast("Tali terpasang ke seberang jurang.");
                return;
            }
            var andi = GameServices.Get<AndiFPSController>();
            if (andi != null) StartCoroutine(Luncur(andi));
        }

        IEnumerator Luncur(AndiFPSController andi)
        {
            _meluncur = true;
            Vector3 a = Titik - Vector3.up * Gantung, b = pasangan.Titik - Vector3.up * Gantung;
            float lama = Mathf.Max(0.5f, Vector3.Distance(a, b) / Laju);
            andi.DudukDi(a);
            for (float t = 0f; t < lama; t += Time.deltaTime)
            {
                float u = Mathf.SmoothStep(0f, 1f, t / lama);
                andi.Teleport(Vector3.Lerp(a, b, u));
                yield return null;
            }
            Vector3 maju = b - a; maju.y = 0f;
            andi.Berdiri();
            andi.Teleport(b + (maju.sqrMagnitude > 0.01f ? maju.normalized * 1.5f : Vector3.zero));
            _meluncur = false;
        }

        void Update()
        {
            var m = M; if (m == null || m.Versi == _versi) return;
            _versi = m.Versi;
            if (tali != null && pasangan != null)
            {
                tali.enabled = AdaTali;
                tali.positionCount = 2; tali.SetPosition(0, Titik); tali.SetPosition(1, pasangan.Titik);
            }
        }
    }
}
