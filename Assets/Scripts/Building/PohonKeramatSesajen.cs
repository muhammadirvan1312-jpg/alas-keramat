using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Pohon Beringin Keramat & Batu Sesajen Nusantara: menaruh sesajen/berdoa memulihkan Sanity (+35) & meredam ancaman Sekte.
    public class PohonKeramatSesajen : MonoBehaviour, IInteractable, ISaveable
    {
        public const float PulihSanity = 35f, RedamSekte = 20f;
        bool _digunakan;

        public bool SudahDigunakan { get { return _digunakan; } }
        public float HoldDuration { get { return 1.0f; } }
        public string SaveKey { get { return "sesajen_" + Mathf.RoundToInt(transform.position.x * 10f) + "_" + Mathf.RoundToInt(transform.position.z * 10f); } }

        public string GetPromptText(GameObject actor) { return _digunakan ? "" : "TARUH SESAJEN & BERDOA"; }
        public bool CanInteract(GameObject actor) { return !_digunakan; }

        public void Interact(GameObject actor)
        {
            if (_digunakan) return;
            _digunakan = true;
            var inv = GameServices.Get<TikarPandan3DInventory>();
            bool pakaiKemenyan = inv != null && (inv.Kurangi(ItemID.Kemenyan, 1) || inv.Kurangi(ItemID.DaunSirih, 1));
            var vital = GameServices.Get<AndiVitalsAndSanity>();
            if (vital != null)
            {
                vital.UbahSanity(pakaiKemenyan ? PulihSanity * 1.3f : PulihSanity);
                vital.ObatiLukaTubuh();
            }
            var rep = GameServices.Get<ReputasiSekte>();
            if (rep != null) rep.TambahAncaman(-RedamSekte);
            var gore = GameServices.Get<GoreSystem>();
            if (gore != null) gore.PercikPartikel(transform.position + Vector3.up * 0.9f, new Color(0.95f, 0.95f, 0.90f, 0.65f), 12, 0.16f, 0.8f, 1.5f);
            GameEvents.RaiseToast(pakaiKemenyan
                ? "Asap kemenyan menenangkan batin (+45 Kewarasan, luka pulih & sekte mereda)."
                : "Doa tenang di pohon keramat (+35 Kewarasan & luka pulih).");
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.PlayItemPickup();
        }

        [System.Serializable] struct DataSesajen { public bool d; }
        public string CaptureJson() { return JsonUtility.ToJson(new DataSesajen { d = _digunakan }); }
        public void RestoreJson(string json) { if (!string.IsNullOrEmpty(json)) _digunakan = JsonUtility.FromJson<DataSesajen>(json).d; }
    }
}
