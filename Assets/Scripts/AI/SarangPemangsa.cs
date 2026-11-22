using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.UI;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Sarang pemangsa rimba dalam berisi jarahan langka (Tengkorak, Kulit, Daging, Jamu).
    public class SarangPemangsa : MonoBehaviour, IInteractable, ISaveable
    {
        [SerializeField] string namaPemangsa = "Harimau";
        bool _diambil;

        public bool Diambil { get { return _diambil; } }
        public float HoldDuration { get { return 1.0f; } }
        public string SaveKey { get { return "sarang_" + Mathf.RoundToInt(transform.position.x * 10f) + "_" + Mathf.RoundToInt(transform.position.z * 10f); } }

        public void Pasang(string nama) { namaPemangsa = nama; }
        public string GetPromptText(GameObject actor) { return _diambil ? "" : "GELEDAH SARANG " + namaPemangsa.ToUpperInvariant(); }
        public bool CanInteract(GameObject actor) { return !_diambil; }

        public void Interact(GameObject actor)
        {
            if (_diambil) return;
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv == null) return;
            int a = inv.Tambah(ItemID.KulitHewan, 2);
            int b = inv.Tambah(ItemID.DagingMentah, 2);
            int c = inv.Tambah(ItemID.Tengkorak, 1);
            int d = inv.Tambah(ItemID.JamuEnergi, 1);
            if (a + b + c + d <= 0) { GameEvents.RaiseToast("Tas penuh."); return; }
            _diambil = true;
            GameEvents.RaiseToast("Sarang " + namaPemangsa + ": Kulit x" + a + ", Daging x" + b + ", Tengkorak x" + c + ", Jamu x" + d);
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.PlayItemPickup();
        }

        [System.Serializable] struct DataSarang { public bool k; }
        public string CaptureJson() { return JsonUtility.ToJson(new DataSarang { k = _diambil }); }
        public void RestoreJson(string json) { if (!string.IsNullOrEmpty(json)) _diambil = JsonUtility.FromJson<DataSarang>(json).k; }
    }
}
