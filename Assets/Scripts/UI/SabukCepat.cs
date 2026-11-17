using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    // Sabuk cepat (12 §T3): 4 slot bawah-tengah berisi makanan/obat yang dimiliki (urutan prioritas), ketuk = makan/minum.
    public class SabukCepat : MonoBehaviour
    {
        public const int JumlahSlot = 4;
        public static readonly ItemID[] Prioritas =
        {
            ItemID.BotolAirBersih, ItemID.JamuHerbal, ItemID.RansumDarurat, ItemID.KelapaMuda, ItemID.Pisang, ItemID.IkanMatang, ItemID.DagingMatang,
            ItemID.SupHutan, ItemID.JamuEnergi, ItemID.KunyitObat, ItemID.DaunSirih, ItemID.TelurAyamHutan, ItemID.IkanKering, ItemID.DagingKering,
            ItemID.Jamur, ItemID.JaheHutan, ItemID.Kepiting, ItemID.IkanMentah, ItemID.DagingMentah, ItemID.MakananGosong
        };

        [SerializeField] GameObject[] slot = new GameObject[JumlahSlot];
        [SerializeField] Image[] ikon = new Image[JumlahSlot];
        [SerializeField] Text[] jumlah = new Text[JumlahSlot];
        [SerializeField] string[] namaIkon = new string[0];
        [SerializeField] Sprite[] spriteIkon = new Sprite[0];

        readonly ItemID[] _isi = new ItemID[JumlahSlot];
        TikarPandan3DInventory _inv;
        bool _kotor = true;

        public void Pasang(GameObject[] s, Image[] i, Text[] j, string[] nama, Sprite[] sprite) { slot = s; ikon = i; jumlah = j; namaIkon = nama; spriteIkon = sprite; }

        void OnEnable() { GameEvents.OnItemChanged += Berubah; _kotor = true; }
        void OnDisable() { GameEvents.OnItemChanged -= Berubah; }
        void Berubah(ItemID id, int n) { _kotor = true; }

        void Update()
        {
            if (!_kotor) return;
            if (_inv == null) _inv = GameServices.Get<TikarPandan3DInventory>();
            if (_inv == null) return;
            _kotor = false;
            int k = 0;
            for (int p = 0; p < Prioritas.Length && k < JumlahSlot; p++)
                if (_inv.Punya(Prioritas[p])) _isi[k++] = Prioritas[p];
            for (int i = 0; i < JumlahSlot; i++)
            {
                bool ada = i < k;
                if (i >= slot.Length || slot[i] == null) continue;
                if (slot[i].activeSelf != ada) slot[i].SetActive(ada);
                if (!ada) { _isi[i] = ItemID.None; continue; }
                if (ikon[i] != null) ikon[i].sprite = Ikon(ItemDatabase.Get(_isi[i]).iconName);
                if (jumlah[i] != null) jumlah[i].text = _inv.Jumlah(_isi[i]).ToString();
            }
        }

        Sprite Ikon(string nama)
        {
            for (int i = 0; i < namaIkon.Length && i < spriteIkon.Length; i++) if (namaIkon[i] == nama) return spriteIkon[i];
            return null;
        }

        public void TekanSlot(int i)
        {
            if (i < 0 || i >= JumlahSlot || _isi[i] == ItemID.None) return;
            var v = GameServices.Get<AndiVitalsAndSanity>();
            var id = _isi[i];
            if (v != null && v.Konsumsi(id)) GameEvents.RaiseToast("Kamu mengonsumsi " + ItemDatabase.Get(id).displayName);
            _kotor = true;
        }
    }
}
