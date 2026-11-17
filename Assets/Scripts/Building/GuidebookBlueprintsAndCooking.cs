using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.Building
{
    // 07 §7: Buku Panduan & 14 blueprint inti/lanjutan. Pilih blueprint -> hantu biru (hijau/merah validitas) -> TANDAI = cetak biru di dunia.
    public partial class GuidebookBlueprintsAndCooking : MonoBehaviour
    {
        public const string Kemah = "Kemah", Makanan = "Makanan & Air", Pertahanan = "Pertahanan", Utilitas = "Utilitas";
        public static readonly string[] Kategori = { Kemah, Makanan, Pertahanan, Utilitas };
        public const float Jangkau = 7f, LerengMaks = 20f, LangkahPutar = 15f;

        static ItemStack S(ItemID id, int n) { return new ItemStack { id = id, count = n }; }
        static BlueprintDefinition B(BlueprintID id, string nama, string kat, Vector2 jejak, params ItemStack[] biaya)
        {
            return new BlueprintDefinition { id = id, displayName = nama, category = kat, cost = biaya, footprint = jejak, advanced = (int)id >= 9 };
        }

        public static readonly BlueprintDefinition[] Tabel =
        {
            B(BlueprintID.ApiUnggun, "Api Unggun", Kemah, new Vector2(1.6f, 1.6f), S(ItemID.Ranting, 6), S(ItemID.Batu, 4)),
            B(BlueprintID.SaungPanggung, "Saung Panggung + Kasur", Kemah, new Vector2(3.6f, 3.6f), S(ItemID.KayuGelondong, 10), S(ItemID.Ranting, 8), S(ItemID.DaunPisang, 16)),
            B(BlueprintID.RakKayu, "Rak Kayu", Utilitas, new Vector2(3.6f, 1.2f), S(ItemID.KayuGelondong, 3), S(ItemID.Ranting, 6)),
            B(BlueprintID.KeretaDorongKayu, "Kereta Dorong Kayu", Utilitas, new Vector2(3.8f, 1.4f), S(ItemID.KayuGelondong, 4), S(ItemID.Ranting, 4), S(ItemID.TaliIjuk, 2)),
            B(BlueprintID.PenampungHujan, "Penampung Hujan", Makanan, new Vector2(1.5f, 1.5f), S(ItemID.KayuGelondong, 2), S(ItemID.Bambu, 3), S(ItemID.DaunPisang, 8)),
            B(BlueprintID.JemuranIkan, "Jemuran Ikan", Makanan, new Vector2(2.0f, 0.8f), S(ItemID.Ranting, 10), S(ItemID.TaliIjuk, 1)),
            B(BlueprintID.PerangkapAyunBambu, "Perangkap Ayun Bambu", Pertahanan, new Vector2(2.4f, 2.4f), S(ItemID.KayuGelondong, 2), S(ItemID.Bambu, 6), S(ItemID.TaliIjuk, 3)),
            B(BlueprintID.OborTengkorak, "Obor Tengkorak", Pertahanan, new Vector2(1f, 1f), S(ItemID.Ranting, 4), S(ItemID.Tengkorak, 1), S(ItemID.KainBatik, 1), S(ItemID.Damar, 1)),
            B(BlueprintID.TendaDarurat, "Tenda Darurat", Kemah, new Vector2(2.2f, 1.6f), S(ItemID.KainBatik, 2), S(ItemID.Ranting, 4)),
            B(BlueprintID.PanciMasak, "Panci Masak", Makanan, new Vector2(0.8f, 0.8f), S(ItemID.LogamBekas, 2), S(ItemID.Batu, 2)),
            B(BlueprintID.MenaraPengintai, "Menara Pengintai", Pertahanan, new Vector2(3.6f, 3.6f), S(ItemID.KayuGelondong, 16), S(ItemID.Ranting, 8)),
            B(BlueprintID.PintuBambu, "Pintu Bambu", Pertahanan, new Vector2(1.6f, 0.4f), S(ItemID.Bambu, 6), S(ItemID.TaliIjuk, 2)),
            B(BlueprintID.KentonganKemah, "Kentongan Kemah", Utilitas, new Vector2(0.8f, 0.8f), S(ItemID.Bambu, 2), S(ItemID.Ranting, 2)),
            B(BlueprintID.RakitBambu, "Rakit Bambu", Utilitas, new Vector2(2.4f, 3.6f), S(ItemID.Bambu, 10), S(ItemID.TaliIjuk, 4)),
            B(BlueprintID.MangkukKepiting, "Mangkuk Kepiting", Pertahanan, new Vector2(0.6f, 0.6f), S(ItemID.Batu, 2), S(ItemID.Kepiting, 1)),
            B(BlueprintID.PatungNgeri, "Patung Ngeri", Pertahanan, new Vector2(1f, 1f), S(ItemID.Ranting, 6), S(ItemID.Tulang, 5)),
            B(BlueprintID.BatuAsah, "Batu Asah", Utilitas, new Vector2(0.8f, 0.6f), S(ItemID.Batu, 4), S(ItemID.Ranting, 2)),
            B(BlueprintID.RumahPohon, "Rumah Pohon", Kemah, new Vector2(3.6f, 3.6f), S(ItemID.KayuGelondong, 14), S(ItemID.Ranting, 10), S(ItemID.TaliIjuk, 4), S(ItemID.DaunPisang, 12)),
            B(BlueprintID.PerangkapPunji, "Perangkap Punji", Pertahanan, new Vector2(2f, 2f), S(ItemID.Bambu, 8), S(ItemID.Ranting, 4)),
            B(BlueprintID.JeratHewan, "Jerat Hewan", Makanan, new Vector2(1f, 1f), S(ItemID.TaliIjuk, 2), S(ItemID.Ranting, 3)),
            B(BlueprintID.BubuIkan, "Bubu Ikan", Makanan, new Vector2(1f, 1.5f), S(ItemID.Bambu, 4), S(ItemID.TaliIjuk, 2)),
            B(BlueprintID.PetiPenyimpanan, "Peti Penyimpanan", Utilitas, new Vector2(1.4f, 0.8f), S(ItemID.Bambu, 4), S(ItemID.Ranting, 6), S(ItemID.TaliIjuk, 2)),
            B(BlueprintID.LampuMinyak, "Lampu Minyak", Kemah, new Vector2(0.5f, 0.5f), S(ItemID.LogamBekas, 1), S(ItemID.MinyakTanah, 1), S(ItemID.KainBatik, 1)),
            B(BlueprintID.JembatanGantung, "Jembatan Gantung", Utilitas, new Vector2(1.2f, 15f), S(ItemID.TaliIjuk, 6), S(ItemID.PapanKayu, 6)),
            B(BlueprintID.GerbangPagar, "Gerbang Pagar", Pertahanan, new Vector2(2f, 0.4f), S(ItemID.KayuGelondong, 4), S(ItemID.TaliIjuk, 2)),
            B(BlueprintID.KursiMeja, "Kursi & Meja", Kemah, new Vector2(2f, 2f), S(ItemID.KayuGelondong, 2), S(ItemID.PapanKayu, 2))
        };

        public static BlueprintDefinition Ambil(BlueprintID id) { foreach (var b in Tabel) if (b.id == id) return b; return Tabel[0]; }

        [SerializeField] Material hantuBiru, hantuSalah;
        AndiFPSController _ctrl;
        GameObject _hantu;
        BlueprintDefinition _pilih;
        float _putar;
        bool _sah;
        public bool Menempatkan { get; private set; }
        public bool Sah { get { return _sah; } }

        public void Pasang(Material biru, Material salah) { hantuBiru = biru; hantuSalah = salah; }

        void Awake()
        {
            GameServices.Register(this);
            _ctrl = GetComponent<AndiFPSController>();
            _hantu = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _hantu.name = "HantuCetakBiru";
            Destroy(_hantu.GetComponent<Collider>());
            _hantu.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _hantu.SetActive(false);
        }

        public void Mulai(BlueprintID id) { _pilih = Ambil(id); _putar = 0f; Menempatkan = true; }
        public void Batal() { Menempatkan = false; _hantu.SetActive(false); }
        public void Putar() { _putar += LangkahPutar; }

        public static bool LokasiSah(BlueprintID id, Vector3 p, Vector3 normal)
        {
            float? wl = WorldTerrainGenerator.GetWaterLevelAt(p.x, p.z);
            bool diAir = wl.HasValue && wl.Value > p.y + 0.1f;
            if (id == BlueprintID.BubuIkan && (!wl.HasValue || wl.Value <= 0f)) return false;
            if (id == BlueprintID.RakitBambu || id == BlueprintID.BubuIkan) return wl.HasValue && wl.Value > WorldTerrainGenerator.SampleHeight(p.x, p.z) + 0.1f;
            return !diAir && Vector3.Angle(normal, Vector3.up) <= LerengMaks && p.y > 0.3f;
        }

        void Update()
        {
            if (!Menempatkan || _ctrl == null || _ctrl.Kamera == null) return;
            var cam = _ctrl.Kamera;
            RaycastHit hit;
            if (!Physics.Raycast(cam.position, cam.forward, out hit, Jangkau, (1 << GameLayers.Ground) | 1 | (1 << GameLayers.Water) | (1 << GameLayers.Tree) | (1 << GameLayers.Structure), QueryTriggerInteraction.Ignore)) { _hantu.SetActive(false); _sah = false; return; }
            Vector3 p = hit.point;
            if (_pilih.id == BlueprintID.RakitBambu) { float? wl = WorldTerrainGenerator.GetWaterLevelAt(p.x, p.z); if (wl.HasValue) p.y = Mathf.Max(p.y, wl.Value); }
            _sah = LokasiSah(_pilih.id, hit.point, hit.normal);
            float yaw = cam.eulerAngles.y + _putar;
            Quaternion rotasi = Quaternion.Euler(0f, yaw, 0f);
            _sah = Khusus(hit, ref p, ref rotasi, _sah);
            _hantu.SetActive(true);
            _hantu.transform.SetPositionAndRotation(p + Vector3.up * 0.5f, rotasi);
            _hantu.transform.localScale = new Vector3(_pilih.footprint.x, 1f, _pilih.footprint.y);
            _hantu.GetComponent<MeshRenderer>().sharedMaterial = _sah ? hantuBiru : hantuSalah;
        }

        public void Tandai()
        {
            if (!Menempatkan || !_sah) { GameEvents.RaiseToast("Tempat tidak cocok"); return; }
            var t = _hantu.transform;
            CetakBiru.Buat(_pilih.id, t.position - Vector3.up * 0.5f, t.rotation, _bentang);
            GameEvents.RaiseToast("Cetak biru " + _pilih.displayName + " ditandai. Isi bahannya.");
            Batal();
        }
    }
}
