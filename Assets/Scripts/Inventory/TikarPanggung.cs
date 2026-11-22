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
        public int Halaman { get; private set; }
        public int JumlahHalaman { get; private set; } = 1;
        public static int HitungHalaman(int jumlah) { return Mathf.Max(1,(jumlah+MaksTampil-1)/MaksTampil); }
        public void PindahHalaman(int selisih) { Halaman=Mathf.Clamp(Halaman+selisih,0,JumlahHalaman-1); Segarkan(); }
        public ItemID IdBenda(int i) { return _idBenda[i]; }
        public Vector3 PosisiBenda(int i) { return _benda[i].transform.position; }

        public void Pasang(Camera c, Transform w, Transform l) { kamera = c; wadah = w; lingkaran = l; }

        void Awake() { GameServices.Register(this); if (kamera != null) kamera.enabled = false; }

        public void Buka(bool v)
        {
            Terbuka = v;
            if (kamera != null) kamera.enabled = v;
            _pilih.Clear();
            if (v) { Halaman=0; Segarkan(); }
        }

        public void Segarkan()
        {
            if (_inv == null) _inv = GameServices.Get<TikarPandan3DInventory>();
            foreach (var b in _benda) if (b != null) { b.SetActive(false); Destroy(b); }
            _benda.Clear(); _idBenda.Clear();
            if (_inv == null || wadah == null) return;
            var urut = new List<ItemID>();
            foreach (var kv in _inv.Isi) urut.Add(kv.Key);
            urut.Sort();
            _pilih.RemoveWhere(id => !_inv.Punya(id));
            JumlahHalaman=HitungHalaman(urut.Count); Halaman=Mathf.Clamp(Halaman,0,JumlahHalaman-1);
            for (int i = 0; i < MaksTampil && Halaman*MaksTampil+i < urut.Count; i++)
            {
                var id = urut[Halaman*MaksTampil+i];
                var g = Model(id);
                g.transform.SetParent(wadah, false);
                g.transform.localPosition = new Vector3((i % Kolom - (Kolom - 1) * 0.5f) * Jarak, 0.05f, -(i / Kolom) * Jarak);
                g.layer = GameLayers.InventoryStage;
                _benda.Add(g); _idBenda.Add(id);
            }
            TataPilihan();
        }

        public static int JumlahBagianModelItem(ItemID id)
        {
            var d = ItemDatabase.Get(id);
            switch (d.category)
            {
                case ItemCategory.Senjata: return 3;
                case ItemCategory.Amunisi: return 3;
                case ItemCategory.Alat: return 2;
                case ItemCategory.Zirah: return 4;
                default: return 2;
            }
        }

        public static Quaternion HitungRotasiInspeksi(bool terpilih, float waktu)
        {
            if (!terpilih) return Quaternion.identity;
            return Quaternion.Euler(12f * Mathf.Sin(waktu * 2.2f), (waktu * 48f) % 360f, 0f);
        }

        void Update()
        {
            if (!Terbuka) return;
            float w = Time.unscaledTime;
            for (int i = 0; i < _benda.Count; i++)
            {
                if (_benda[i] == null) continue;
                bool p = _pilih.Contains(_idBenda[i]);
                _benda[i].transform.localRotation = HitungRotasiInspeksi(p, w + i * 0.4f);
            }
        }

        static void Bagian(GameObject induk, PrimitiveType tipe, Vector3 pos, Vector3 rot, Vector3 skala, Material mat)
        {
            var p = GameObject.CreatePrimitive(tipe);
            Destroy(p.GetComponent<Collider>());
            p.transform.SetParent(induk.transform, false);
            p.transform.localPosition = pos;
            p.transform.localRotation = Quaternion.Euler(rot);
            p.transform.localScale = skala;
            p.layer = GameLayers.InventoryStage;
            var mr = p.GetComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        static GameObject Model(ItemID id)
        {
            var d = ItemDatabase.Get(id);
            var g = new GameObject("Tikar_" + id);
            g.layer = GameLayers.InventoryStage;
            var bc = g.AddComponent<BoxCollider>();
            bc.center = new Vector3(0f, 0.04f, 0f);
            bc.size = new Vector3(0.22f, 0.16f, 0.26f);

            float h = ((int)id * 0.618034f) % 1f;
            var warnaUtama = Color.HSVToRGB(h, 0.45f, 0.75f);
            var mUtama = SafeMaterialFactory.GetCached("Tikar_" + (int)id, warnaUtama, 0f, 0.35f);
            var mKayu = SafeMaterialFactory.GetCached("Tikar_Kayu", new Color(0.36f, 0.24f, 0.15f), 0f, 0.25f);
            var mKuningan = SafeMaterialFactory.GetCached("Tikar_Kuningan", new Color(0.78f, 0.63f, 0.28f), 0.75f, 0.72f);
            var mBaja = SafeMaterialFactory.GetCached("Tikar_Baja", new Color(0.72f, 0.76f, 0.80f), 0.85f, 0.82f);
            var mPutih = SafeMaterialFactory.GetCached("Tikar_Putih", new Color(0.88f, 0.85f, 0.78f), 0f, 0.25f);

            switch (d.category)
            {
                case ItemCategory.Senjata:
                    if (id == ItemID.BusurBambu || id == ItemID.BusurTanduk)
                    {
                        Bagian(g, PrimitiveType.Cylinder, new Vector3(-0.02f, 0.04f, 0.06f), new Vector3(72f, 15f, 0f), new Vector3(0.022f, 0.08f, 0.022f), mKayu);
                        Bagian(g, PrimitiveType.Cylinder, new Vector3(-0.02f, 0.04f, -0.06f), new Vector3(108f, -15f, 0f), new Vector3(0.022f, 0.08f, 0.022f), mKayu);
                        Bagian(g, PrimitiveType.Cylinder, new Vector3(0.02f, 0.04f, 0f), new Vector3(90f, 0f, 0f), new Vector3(0.006f, 0.12f, 0.006f), mPutih);
                    }
                    else if (id == ItemID.PistolSuar || id == ItemID.PistolPT || id == ItemID.BedilLocok || id == ItemID.SenapanLaras || id == ItemID.ZiplineGun)
                    {
                        Bagian(g, PrimitiveType.Cube, new Vector3(0f, 0.03f, -0.06f), new Vector3(22f, 0f, 0f), new Vector3(0.034f, 0.06f, 0.07f), mKayu);
                        Bagian(g, PrimitiveType.Cylinder, new Vector3(0f, 0.055f, 0.04f), new Vector3(90f, 0f, 0f), new Vector3(0.028f, 0.10f, 0.028f), id == ItemID.PistolSuar ? mUtama : mBaja);
                        Bagian(g, PrimitiveType.Cube, new Vector3(0f, 0.045f, -0.01f), Vector3.zero, new Vector3(0.038f, 0.035f, 0.06f), mKuningan);
                    }
                    else
                    {
                        var mBilah = id == ItemID.GolokApiPusaka
                            ? SafeMaterialFactory.CreatePBR("Tikar_BilahApi", new Color(1f, 0.42f, 0.12f), 0.5f, 0.8f, null, null, new Color(2.4f, 0.85f, 0.2f))
                            : id == ItemID.KerisPusaka ? mKuningan : mBaja;
                        Bagian(g, PrimitiveType.Cylinder, new Vector3(0f, 0.035f, -0.09f), new Vector3(90f, 0f, 0f), new Vector3(0.028f, 0.045f, 0.028f), mKayu);
                        Bagian(g, PrimitiveType.Cube, new Vector3(0f, 0.038f, -0.04f), Vector3.zero, new Vector3(0.055f, 0.024f, 0.018f), mKuningan);
                        Bagian(g, PrimitiveType.Cube, new Vector3(0f, 0.038f, 0.055f), Vector3.zero, new Vector3(0.036f, 0.014f, 0.16f), mBilah);
                    }
                    break;
                case ItemCategory.Amunisi:
                    Bagian(g, PrimitiveType.Cylinder, new Vector3(0f, 0.035f, 0f), new Vector3(90f, 0f, 0f), new Vector3(0.016f, 0.09f, 0.016f), mKayu);
                    Bagian(g, PrimitiveType.Cube, new Vector3(0f, 0.035f, 0.09f), new Vector3(0f, 45f, 0f), new Vector3(0.026f, 0.012f, 0.026f), mBaja);
                    Bagian(g, PrimitiveType.Cube, new Vector3(0f, 0.042f, -0.075f), Vector3.zero, new Vector3(0.032f, 0.022f, 0.035f), mUtama);
                    break;
                case ItemCategory.Zirah:
                    for (int k = -1; k <= 1; k++)
                        Bagian(g, PrimitiveType.Cube, new Vector3(k * 0.055f, 0.035f, 0f), new Vector3(0f, 0f, -k * 6f), new Vector3(0.048f, 0.024f, 0.17f), mUtama);
                    Bagian(g, PrimitiveType.Cube, new Vector3(0f, 0.022f, 0f), Vector3.zero, new Vector3(0.18f, 0.012f, 0.045f), mKayu);
                    break;
                case ItemCategory.Alat:
                    Bagian(g, PrimitiveType.Capsule, new Vector3(0f, 0.04f, -0.02f), new Vector3(90f, 0f, 0f), new Vector3(0.055f, 0.075f, 0.055f), mUtama);
                    Bagian(g, PrimitiveType.Cylinder, new Vector3(0f, 0.045f, 0.055f), new Vector3(90f, 0f, 0f), new Vector3(0.042f, 0.025f, 0.042f), mKuningan);
                    break;
                case ItemCategory.Makanan:
                    Bagian(g, PrimitiveType.Sphere, new Vector3(0f, 0.045f, 0f), Vector3.zero, new Vector3(0.13f, 0.09f, 0.13f), mUtama);
                    Bagian(g, PrimitiveType.Cylinder, new Vector3(0f, 0.085f, 0f), Vector3.zero, new Vector3(0.045f, 0.02f, 0.045f), mKayu);
                    break;
                default:
                    Bagian(g, PrimitiveType.Cube, new Vector3(0f, 0.035f, -0.015f), Vector3.zero, new Vector3(0.13f, 0.055f, 0.13f), mUtama);
                    Bagian(g, PrimitiveType.Cylinder, new Vector3(0.02f, 0.068f, 0.015f), new Vector3(0f, 25f, 90f), new Vector3(0.035f, 0.055f, 0.035f), id == ItemID.Tengkorak ? mPutih : mKayu);
                    break;
            }
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
            int dikonsumsi=0;
            foreach (var b in r.inputs) if(b.id==r.output.id) dikonsumsi+=b.count;
            if (r.output.count<=0 || inv.SisaRuang(r.output.id)+dikonsumsi < r.output.count) return false;
            foreach (var b in r.inputs) inv.Kurangi(b.id, b.count);
            inv.Tambah(r.output.id, r.output.count);
            return true;
        }
    }
}
