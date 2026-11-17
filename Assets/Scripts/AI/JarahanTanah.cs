using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Barang jatuhan hantu yang diusir (06 §4: Kain Kafan, Kemenyan, Arang). Prefab buntelan dari Builder; tanpa prefab langsung masuk tas.
    public class JarahanTanah : MonoBehaviour, IInteractable
    {
        public const float LamaHilang = 300f;

        [SerializeField] ItemID id;
        [SerializeField] int jumlah = 1;
        float _umur;

        public static void Jatuhkan(Vector3 pos, ItemID id, int n)
        {
            var dir = GameServices.Get<EnemyDirector>();
            if (dir == null || dir.PrefabJarahan == null)
            {
                var inv = GameServices.Get<TikarPandan3DInventory>();
                if (inv != null) inv.Tambah(id, n);
                return;
            }
            float h = WorldTerrainGenerator.SampleHeight(pos.x, pos.z);
            var go = Instantiate(dir.PrefabJarahan, new Vector3(pos.x, Mathf.Max(h, pos.y - 2f) + 0.1f, pos.z), Quaternion.Euler(0f, Random.value * 360f, 0f));
            var j = go.GetComponent<JarahanTanah>();
            if (j != null) { j.id = id; j.jumlah = n; }
        }

        void Update() { _umur += Time.deltaTime; if (_umur > LamaHilang) Destroy(gameObject); }

        public float HoldDuration { get { return 0f; } }
        public string GetPromptText(GameObject actor) { return "AMBIL " + ItemDatabase.Get(id).displayName.ToUpperInvariant(); }
        public bool CanInteract(GameObject actor) { return id != ItemID.None; }
        public void Interact(GameObject actor)
        {
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv == null) return;
            inv.Tambah(id, jumlah);
            GameEvents.RaiseToast("Dapat: " + ItemDatabase.Get(id).displayName + " x" + jumlah);
            Destroy(gameObject);
        }
    }
}
