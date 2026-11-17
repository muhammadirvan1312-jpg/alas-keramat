using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.Building
{
    // 05 §5: Panci di atas api unggun menyala (<= 2 m): 1 Botol Air Bersih + 2 bahan -> 40 s -> 2 Sup Hutan (+ botol kosong kembali).
    public class PanciMasak : MonoBehaviour, IInteractable
    {
        public const float LamaSup = 40f, JarakApi = 2.2f;
        public static readonly ItemID[] Bahan = { ItemID.DagingMentah, ItemID.DagingMatang, ItemID.IkanMentah, ItemID.IkanMatang, ItemID.Jamur, ItemID.Pisang, ItemID.JaheHutan, ItemID.TelurAyamHutan };
        float _t = -1f;
        public float HoldDuration { get { return 0f; } }

        bool AdaApi() { return ApiUnggun.AdaApiDekat(transform.position, JarakApi); }

        void Update() { if (_t >= 0f && _t < LamaSup && AdaApi()) _t += Time.deltaTime; }

        public static int HitungBahan(TikarPandan3DInventory inv) { int n = 0; foreach (var b in Bahan) n += inv.Jumlah(b); return n; }

        public string GetPromptText(GameObject actor)
        {
            if (_t >= LamaSup) return "AMBIL SUP HUTAN";
            if (_t >= 0f) return AdaApi() ? "" : "PANCI BUTUH API";
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv == null || !AdaApi()) return "";
            return inv.Punya(ItemID.BotolAirBersih) && HitungBahan(inv) >= 2 ? "MASAK SUP" : "";
        }

        public bool CanInteract(GameObject actor) { string p = GetPromptText(actor); return p.Length > 0 && p != "PANCI BUTUH API"; }

        public void Interact(GameObject actor)
        {
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv == null) return;
            if (_t >= LamaSup) { if (inv.Tambah(ItemID.SupHutan, 2) > 0) { inv.Tambah(ItemID.BotolKosong, 1); _t = -1f; GameEvents.RaiseToast("+2 Sup Hutan"); } return; }
            if (!inv.Kurangi(ItemID.BotolAirBersih, 1)) return;
            int ambil = 0;
            foreach (var b in Bahan) while (ambil < 2 && inv.Kurangi(b, 1)) ambil++;
            _t = 0f;
            GameEvents.RaiseToast("Sup mulai dimasak");
        }
    }
}
