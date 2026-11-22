using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Ikan sungai/pantai yang bisa ditangkap langsung saat Andi mengarungi air dangkal (respawn 180 detik).
    public class IkanTangkap : MonoBehaviour, IInteractable
    {
        public const float LamaRespawn = 180f;
        float _respawn;
        Renderer[] _rend;

        public bool Tersedia { get { return _respawn <= 0f; } }
        public float HoldDuration { get { return 0f; } }
        public string GetPromptText(GameObject actor) { return Tersedia ? "TANGKAP IKAN" : ""; }
        public bool CanInteract(GameObject actor) { return Tersedia; }

        void Awake() { _rend = GetComponentsInChildren<Renderer>(true); }

        public void PerbaruiRespawn(float dt)
        {
            if (_respawn <= 0f) return;
            _respawn -= dt;
            if (_respawn <= 0f) AturTampak(true);
        }

        public void Interact(GameObject actor)
        {
            if (!Tersedia) return;
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv == null) return;
            if (inv.Tambah(ItemID.IkanMentah, 1) <= 0) { GameEvents.RaiseToast("Tas penuh."); return; }
            _respawn = LamaRespawn;
            AturTampak(false);
            GameEvents.RaiseToast("Menangkap 1 Ikan Mentah!");
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.PlayItemPickup();
            var gore = GameServices.Get<GoreSystem>();
            if (gore != null) gore.PercikPartikel(transform.position + Vector3.up * 0.1f, new Color(0.75f, 0.90f, 0.98f, 0.7f), 10, 0.055f, 1.8f, 0.5f);
        }

        void AturTampak(bool v)
        {
            if (_rend == null) return;
            for (int i = 0; i < _rend.Length; i++) if (_rend[i] != null) _rend[i].enabled = v;
        }
    }
}
