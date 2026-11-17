using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.AI
{
    // 08 §3.2: BERI MAKANAN (tahap 2+, jeda 2 jam game), TERIMA HADIAH, menu BERINTERAKSI DENGAN NINGSIH (tahap 3), BERI JAMU saat kritis; simpan.
    public partial class NingsihVirginiaAI
    {
        public enum AksiNingsih { BerikanBusur, BerikanSuar, Ikuti, Tunggu, PulangKeKemah, BeriMakanan, BerikanPistol }
        public static readonly ItemID[] MakananNingsih = { ItemID.KelapaMuda, ItemID.IkanMatang, ItemID.Pisang, ItemID.DaunSirih };

        ItemID MakananDimiliki()
        {
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv == null) return ItemID.None;
            foreach (var m in MakananNingsih) if (inv.Punya(m)) return m;
            return ItemID.None;
        }

        public string GetPromptText(GameObject actor)
        {
            if (Kritis) return "BERI JAMU";
            if (Gugur || !Dekat(6f) || Sikap == SikapNingsih.DiSarang || Sikap == SikapNingsih.Kabur) return "";
            if (HadiahSiap) return "TERIMA HADIAH";
            int t = Tahap(Trust);
            if (t == 3) return "BERINTERAKSI DENGAN NINGSIH";
            if (t == 2 && MakananDimiliki() != ItemID.None && Time.time - _tMakan >= JedaMakan) return "BERI MAKANAN";
            return "";
        }

        public bool CanInteract(GameObject actor) { return GetPromptText(actor).Length > 0; }

        public void Interact(GameObject actor)
        {
            if (Kritis) { BeriJamu(); return; }
            string p = GetPromptText(actor);
            if (p == "TERIMA HADIAH") { TerimaHadiah(); return; }
            if (p == "BERI MAKANAN") { Jalankan(AksiNingsih.BeriMakanan); return; }
            var ui = GameServices.Get<AlasKeramat.UI.MenuNingsihUI>();
            if (ui != null) ui.Buka(this);
        }

        // Hadiah acak 08 §3.2: DaunSirih x2 / KelapaMuda x1 / IkanMentah x2 / Kemenyan x1.
        public static void IsiHadiah(float acak, out ItemID id, out int n)
        {
            if (acak < 0.3f) { id = ItemID.DaunSirih; n = 2; }
            else if (acak < 0.6f) { id = ItemID.KelapaMuda; n = 1; }
            else if (acak < 0.85f) { id = ItemID.IkanMentah; n = 2; }
            else { id = ItemID.Kemenyan; n = 1; }
        }

        void TerimaHadiah()
        {
            ItemID id; int n; IsiHadiah(Random.value, out id, out n);
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv != null) inv.Tambah(id, n);
            _hadiahDiterima = true;
            UbahTrust(KejadianTrust.TerimaHadiah);
            GameEvents.RaiseToast("Ningsih memberimu " + n + " " + ItemDatabase.Get(id).displayName);
        }

        public bool BisaAksi(AksiNingsih a)
        {
            var inv = GameServices.Get<TikarPandan3DInventory>();
            switch (a)
            {
                case AksiNingsih.BerikanBusur: return !PunyaBusur && inv != null && inv.Punya(ItemID.BusurBambu);
                case AksiNingsih.BerikanSuar: return !PunyaSuar && inv != null && inv.Punya(ItemID.PistolSuar);
                case AksiNingsih.BerikanPistol: return !PunyaPistol && Trust >= BatasPistol && inv != null && inv.Punya(ItemID.PistolPT);
                case AksiNingsih.BeriMakanan: return MakananDimiliki() != ItemID.None && Time.time - _tMakan >= JedaMakan;
                default: return Tahap(Trust) == 3;
            }
        }

        public void Jalankan(AksiNingsih a)
        {
            if (!BisaAksi(a)) { GameEvents.RaiseToast("Belum bisa"); return; }
            var inv = GameServices.Get<TikarPandan3DInventory>();
            switch (a)
            {
                case AksiNingsih.BerikanBusur: inv.Kurangi(ItemID.BusurBambu, 1); PunyaBusur = true; GameEvents.RaiseSubtitle("(Ningsih menimbang busur itu, lalu tersenyum tipis.)"); break;
                case AksiNingsih.BerikanSuar: inv.Kurangi(ItemID.PistolSuar, 1); PunyaSuar = true; GameEvents.RaiseSubtitle("(Ningsih menyelipkan pistol suar di pinggangnya.)"); break;
                case AksiNingsih.BerikanPistol: inv.Kurangi(ItemID.PistolPT, 1); PunyaPistol = true; break;
                case AksiNingsih.Ikuti: Sikap = SikapNingsih.Ikuti; Klip("Mengangguk"); break;
                case AksiNingsih.Tunggu: Sikap = SikapNingsih.Tunggu; _titikTunggu = transform.position; Klip("Mengangguk"); break;
                case AksiNingsih.PulangKeKemah: Sikap = SikapNingsih.Kemah; break;
                case AksiNingsih.BeriMakanan:
                    var m = MakananDimiliki();
                    if (inv.Kurangi(m, 1)) { _tMakan = Time.time; UbahTrust(KejadianTrust.BeriMakanan); GameEvents.RaiseSubtitle("(Ningsih menerima " + ItemDatabase.Get(m).displayName + " dan bersenandung pelan.)"); }
                    break;
            }
        }

        [System.Serializable] struct DataNingsih { public Vector3 pos; public float hp, trust; public int sikap, hariKabur, hadiahHari; public bool busur, suar, pistol, gugur, hadiahDiterima; }
        public string CaptureJson()
        {
            return JsonUtility.ToJson(new DataNingsih { pos = transform.position, hp = HP, trust = Trust, sikap = (int)Sikap, hariKabur = _hariKabur, hadiahHari = _hadiahHari, busur = PunyaBusur, suar = PunyaSuar, pistol = PunyaPistol, gugur = Gugur, hadiahDiterima = _hadiahDiterima });
        }
        public void RestoreJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            var d = JsonUtility.FromJson<DataNingsih>(json);
            HP = Mathf.Clamp(d.hp, 0f, HPNingsih); Trust = Mathf.Clamp(d.trust, 0f, 100f); Sikap = (SikapNingsih)d.sikap; _hariKabur = d.hariKabur; _hadiahHari = d.hadiahHari;
            PunyaBusur = d.busur; PunyaSuar = d.suar; PunyaPistol = d.pistol; Gugur = d.gugur; _hadiahDiterima = d.hadiahDiterima;
            Teleport(d.pos);
        }

        public void SetTrustUji(float t) { Trust = Mathf.Clamp(t, 0f, 100f); }
    }
}
