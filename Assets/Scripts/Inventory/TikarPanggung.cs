using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Inventory
{
    // 05 §6 & 09 §5: Tikar Pandan 3D. Panggung terpisah (layer InventoryStage) dengan kamera sendiri; item milik Andi ditata 3D di atas tikar.
    // Ketuk item = masuk/keluar lingkaran rakit. Logika resep memakai RecipeDatabase (bahan dipilih menentukan resep).
    public class TikarPanggung : MonoBehaviour
    {
        public const int Kolom = 6, MaksTampil = 36;
        public const float Jarak = 0.32f, NaikPilih = 0.08f;

        [SerializeField] Camera kamera;
        [SerializeField] Transform wadah;
        [SerializeField] Transform lingkaran;

        readonly List<GameObject> _benda = new List<GameObject>();
        readonly List<ItemID> _idBenda = new List<ItemID>();
        readonly HashSet<ItemID> _pilih = new HashSet<ItemID>();
        TikarPandan3DInventory _inv;

        public Camera Kamera { get { return kamera; } }
        public bool Terbuka { get; private set; }
        public IEnumerable<ItemID> Pilihan { get { return _pilih; } }
        public int JumlahPilihan { get { return _pilih.Count; } }
        public int JumlahBenda { get { return _benda.Count; } }
        public ItemID IdBenda(int i) { return _idBenda[i]; }
        public Vector3 PosisiBenda(int i) { return _benda[i].transform.position; }

        public void Pasang(Camera c, Transform w, Transform l) { kamera = c; wadah = w; lingkaran = l; }

        void Awake() { GameServices.Register(this); if (kamera != null) kamera.enabled = false; }

        public void Buka(bool v)
        {
            Terbuka = v;
            if (kamera != null) kamera.enabled = v;
            _pilih.Clear();
            if (v) Segarkan();
        }

        public void Segarkan()
        {
            if (_inv == null) _inv = GameServices.Get<TikarPandan3DInventory>();
            foreach (var b in _benda) if (b != null) Destroy(b);
            _benda.Clear(); _idBenda.Clear();
            if (_inv == null || wadah == null) return;
            var urut = new List<ItemID>();
            foreach (var kv in _inv.Isi) urut.Add(kv.Key);
            urut.Sort();
            for (int i = 0; i < urut.Count && i < MaksTampil; i++)
            {
                var id = urut[i];
                var g = Model(id);
                g.transform.SetParent(wadah, false);
                g.transform.localPosition = new Vector3((i % Kolom - (Kolom - 1) * 0.5f) * Jarak, 0.05f, -(i / Kolom) * Jarak);
                g.layer = GameLayers.InventoryStage;
                _benda.Add(g); _idBenda.Add(id);
            }
            TataPilihan();
        }

        static GameObject Model(ItemID id)
        {
            var d = ItemDatabase.Get(id);
            PrimitiveType t; Vector3 s;
            switch (d.category)
            {
                case ItemCategory.Makanan: t = PrimitiveType.Sphere; s = new Vector3(0.14f, 0.12f, 0.14f); break;
                case ItemCategory.Senjata: t = PrimitiveType.Cube; s = new Vector3(0.05f, 0.03f, 0.28f); break;
                case ItemCategory.Amunisi: t = PrimitiveType.Cylinder; s = new Vector3(0.03f, 0.12f, 0.03f); break;
                case ItemCategory.Zirah: t = PrimitiveType.Cube; s = new Vector3(0.2f, 0.03f, 0.2f); break;
                case ItemCategory.Alat: t = PrimitiveType.Capsule; s = new Vector3(0.06f, 0.1f, 0.06f); break;
                default: t = PrimitiveType.Cube; s = new Vector3(0.14f, 0.07f, 0.14f); break;
            }
            var g = GameObject.CreatePrimitive(t);
            g.name = "Tikar_" + id;
            g.transform.localScale = s;
            float h = ((int)id * 0.618034f) % 1f;
            var warna = Color.HSVToRGB(h, 0.45f, 0.75f);
            g.GetComponent<MeshRenderer>().sharedMaterial = SafeMaterialFactory.GetCached("Tikar_" + (int)id, warna, 0f, 0.3f);
            return g;
        }

        // Ketuk di layar: pilih/lepas item. Mengembalikan item yang terkena (None bila tidak ada).
        public ItemID Ketuk(Vector2 layar)
        {
            if (kamera == null) return ItemID.None;
            RaycastHit hit;
            if (!Physics.Raycast(kamera.ScreenPointToRay(layar), out hit, 10f, 1 << GameLayers.InventoryStage, QueryTriggerInteraction.Collide)) return ItemID.None;
            int i = _benda.IndexOf(hit.collider.gameObject);
            if (i < 0) return ItemID.None;
            var id = _idBenda[i];
            if (!_pilih.Remove(id)) _pilih.Add(id);
            TataPilihan();
            return id;
        }

        public void BatalPilih() { _pilih.Clear(); TataPilihan(); }

        void TataPilihan()
        {
            int k = 0;
            for (int i = 0; i < _benda.Count; i++)
            {
                bool p = _pilih.Contains(_idBenda[i]);
                var t = _benda[i].transform;
                Vector3 asal = new Vector3((i % Kolom - (Kolom - 1) * 0.5f) * Jarak, 0.05f, -(i / Kolom) * Jarak);
                if (p && lingkaran != null) { t.position = lingkaran.position + new Vector3((k - 1) * 0.16f, NaikPilih, 0f); k++; }
                else t.localPosition = asal;
            }
        }

        // Resep yang bahannya persis jenis-jenis terpilih dan tersedia di tas (KainKafan menggantikan KainBatik).
        public static CraftingRecipe? CariResep(ICollection<ItemID> pilihan, TikarPandan3DInventory inv)
        {
            if (pilihan == null || pilihan.Count == 0 || inv == null) return null;
            var stok = new List<ItemStack>();
            foreach (var kv in inv.Isi) stok.Add(new ItemStack { id = kv.Key, count = kv.Value });
            foreach (var r in RecipeDatabase.CraftableFrom(stok))
            {
                if (r.requiredTool != ItemID.None && !inv.Punya(r.requiredTool)) continue;
                bool cocok = r.inputs.Length == pilihan.Count;
                foreach (var b in r.inputs) if (!pilihan.Contains(b.id)) cocok = false;
                if (cocok) return r;
            }
            return null;
        }

        public static bool Rakit(CraftingRecipe r, TikarPandan3DInventory inv)
        {
            if (inv == null) return false;
            foreach (var b in r.inputs) if (!inv.Punya(b.id, b.count)) return false;
            if (inv.SisaRuang(r.output.id) <= 0) return false;
            foreach (var b in r.inputs) inv.Kurangi(b.id, b.count);
            inv.Tambah(r.output.id, r.output.count);
            return true;
        }
    }
}
