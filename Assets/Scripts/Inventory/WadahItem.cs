using System.Text;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Inventory
{
    // Koper darurat (03 §9 Zona 1) atau benda lepas yang bisa diambil. Tutup koper berputar saat dibuka.
    public class WadahItem : MonoBehaviour, IInteractable
    {
        public const float SudutTutupBuka = -110f, LajuTutup = 240f;

        [SerializeField] string namaTampil = "Koper";
        [SerializeField] ItemID[] item = new ItemID[0];
        [SerializeField] int[] jumlah = new int[0];
        [SerializeField] int lore;            // 0 = tanpa lore
        [SerializeField] bool hilangSetelahDiambil;
        [SerializeField] Transform tutup;

        bool _kosong;
        float _sudut;

        public bool Kosong { get { return _kosong; } }
        public ItemID[] DaftarItem { get { return item; } }
        public int[] DaftarJumlah { get { return jumlah; } }
        public float HoldDuration { get { return 0f; } }

        public void Pasang(string nama, ItemID[] isi, int[] n, int loreId, bool hilang, Transform tutupKoper)
        {
            namaTampil = nama; item = isi; jumlah = n; lore = loreId; hilangSetelahDiambil = hilang; tutup = tutupKoper;
        }

        public string GetPromptText(GameObject actor) { return hilangSetelahDiambil ? "AMBIL " + namaTampil.ToUpperInvariant() : "BUKA " + namaTampil.ToUpperInvariant(); }
        public bool CanInteract(GameObject actor) { return !_kosong; }

        public void Interact(GameObject actor)
        {
            if (_kosong) return;
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv == null) { Debug.LogError("[AK][WadahItem] inventaris tidak ada"); return; }
            var sb = new StringBuilder();
            bool sisa = false;
            for (int i = 0; i < item.Length && i < jumlah.Length; i++)
            {
                if (jumlah[i] <= 0) continue;
                int masuk = inv.Tambah(item[i], jumlah[i]);
                if (masuk > 0)
                {
                    if (sb.Length > 0) sb.Append(", ");
                    sb.Append(ItemDatabase.Get(item[i]).displayName);
                    if (masuk > 1) sb.Append(" x").Append(masuk);
                }
                jumlah[i] -= masuk;
                if (jumlah[i] > 0) sisa = true;
            }
            if (lore > 0) { GameEvents.RaiseLoreFound((LoreID)lore); sb.Append(sb.Length > 0 ? ", " : "").Append("Dokumen Lore"); lore = 0; }
            GameEvents.RaiseToast(sb.Length > 0 ? "Didapat: " + sb : "Tas penuh");
            if (sisa) { GameEvents.RaiseToast("Sebagian tidak muat"); return; }
            _kosong = true;
            if (hilangSetelahDiambil) gameObject.SetActive(false);
        }

        void Update()
        {
            if (tutup == null || !_kosong || _sudut <= SudutTutupBuka) return;
            _sudut = Mathf.Max(SudutTutupBuka, _sudut - LajuTutup * Time.deltaTime);
            tutup.localRotation = Quaternion.Euler(_sudut, 0f, 0f);
        }
    }
}
