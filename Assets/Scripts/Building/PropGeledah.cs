using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Props desa yang bisa digeledah sekali (tahan 1,2 detik): peti, drum, jeriken, bakul, periuk, panci, ember.
    // Isi diundi deterministik dari posisi (konsisten setelah simpan/muat); status "sudah digeledah" ikut disimpan.
    public class PropGeledah : MonoBehaviour, IInteractable, ISaveable
    {
        public enum Wadah { Peti = 0, BahanBakar = 1, Dapur = 2, Ember = 3 }
        public const float LamaGeledah = 1.2f;
        public const int MaksIsi = 3;

        public Wadah Jenis;
        bool _kosong;

        public bool Kosong { get { return _kosong; } }
        public float HoldDuration { get { return LamaGeledah; } }
        public string SaveKey { get { return "geledah_" + Mathf.RoundToInt(transform.position.x * 10f) + "_" + Mathf.RoundToInt(transform.position.z * 10f); } }

        static readonly ItemID[] IsiPeti = { ItemID.KainBatik, ItemID.TaliIjuk, ItemID.PerbanKain, ItemID.MinyakTanah, ItemID.Baterai, ItemID.BotolKosong, ItemID.LogamBekas, ItemID.RansumDarurat, ItemID.KorekGas };
        static readonly ItemID[] IsiBahanBakar = { ItemID.MinyakTanah, ItemID.MinyakTanah, ItemID.LogamBekas, ItemID.BotolKosong };
        static readonly ItemID[] IsiDapur = { ItemID.Arang, ItemID.KunyitObat, ItemID.JaheHutan, ItemID.DaunSirih, ItemID.BotolKosong, ItemID.RansumDarurat };
        static readonly ItemID[] IsiEmber = { ItemID.BotolKosong, ItemID.TaliIjuk, ItemID.Batu };

        public static ItemID[] TabelIsi(Wadah w)
        {
            switch (w)
            {
                case Wadah.BahanBakar: return IsiBahanBakar;
                case Wadah.Dapur: return IsiDapur;
                case Wadah.Ember: return IsiEmber;
                default: return IsiPeti;
            }
        }

        // Murni & deterministik: benih dari posisi. Mengisi 'hasil' (panjang >= MaksIsi), mengembalikan jumlah item.
        // Sekitar 1 dari 4 wadah kosong melompong (desa sudah dijarah sebagian).
        public static int Undi(Wadah w, Vector3 posisi, ItemID[] hasil)
        {
            int benih = Mathf.RoundToInt(posisi.x * 10f) * 73856093 ^ Mathf.RoundToInt(posisi.z * 10f) * 19349663 ^ ((int)w + 1) * 83492791;
            var rnd = new System.Random(benih);
            if (rnd.Next(4) == 0) return 0;
            var tabel = TabelIsi(w);
            int n = 1 + rnd.Next(w == Wadah.Peti ? MaksIsi : 2);
            for (int i = 0; i < n; i++) hasil[i] = tabel[rnd.Next(tabel.Length)];
            return n;
        }

        public string GetPromptText(GameObject actor) { return _kosong ? "" : "GELEDAH"; }
        public bool CanInteract(GameObject actor) { return !_kosong; }

        public void Interact(GameObject actor)
        {
            if (_kosong) return;
            _kosong = true;
            var hasil = new ItemID[MaksIsi];
            int n = Undi(Jenis, transform.position, hasil);
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (n == 0 || inv == null) { GameEvents.RaiseToast("Kosong. Sudah dijarah orang."); return; }
            string pesan = "Dapat: ";
            int dapat = 0;
            for (int i = 0; i < n; i++)
            {
                if (inv.Tambah(hasil[i], 1) <= 0) continue;
                pesan += (dapat > 0 ? ", " : "") + ItemDatabase.Get(hasil[i]).displayName;
                dapat++;
            }
            GameEvents.RaiseToast(dapat > 0 ? pesan : "Tas penuh.");
            if (dapat == 0) _kosong = false;
            var audio = GameServices.Get<ProceduralAudioSynth>();
            if (audio != null && dapat > 0) audio.PlayItemPickup();
        }

        [System.Serializable] struct DataGeledah { public bool k; }
        public string CaptureJson() { return JsonUtility.ToJson(new DataGeledah { k = _kosong }); }
        public void RestoreJson(string json) { if (string.IsNullOrEmpty(json)) return; _kosong = JsonUtility.FromJson<DataGeledah>(json).k; }
    }
}
