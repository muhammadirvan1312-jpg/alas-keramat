using AlasKeramat.AI;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;
namespace AlasKeramat.Core
{
    // T10: bendera gerbang dari dunia (lore, item, Ningsih, posisi Andi, gua, boss, altar).
    public partial class QuestManager
    {
        public const float TrustM06 = 31f, JarakLihatNingsih = 50f, ZUtaraJurang = 662f;
        NingsihVirginiaAI _ningsih;
        AndiFPSController _andi;

        void LoreDitemukan(LoreID id)
        {
            int bit = LoreLibrary.Bit(id);
            if (bit == 0 || (Progres.lore & bit) != 0) return;
            Progres.lore |= bit;
            if (id == LoreID.L08 && _ningsih != null) _ningsih.UbahTrust(NingsihVirginiaAI.KejadianTrust.LoreDelapanDiApi); // 08: Lore #8 +10 Trust
            Periksa();
        }

        void ItemBerubah(ItemID id, int n)
        {
            if(SaveSystem.SedangMemuat)return;
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv == null) return;
            bool ubah = false;
            if (id == ItemID.Cangkul && inv.Punya(id)) { ubah |= !Progres.cangkul; Progres.cangkul = true; }
            if (id == ItemID.KartuAksesBunker && inv.Punya(id)) { ubah |= !Progres.kartu; Progres.kartu = true; }
            if (id == ItemID.AlatSelamRebreather && inv.Punya(id)) { ubah |= !Progres.rebreather; Progres.rebreather = true; }
            if (id == ItemID.ZiplineGun && inv.Punya(id)) { ubah |= !Progres.zipline; Progres.zipline = true; }
            int bit = id == ItemID.PakuKeramat1 ? 1 : id == ItemID.PakuKeramat2 ? 2 : id == ItemID.PakuKeramat3 ? 4 : 0;
            if (bit != 0 && inv.Punya(id) && (Progres.paku & bit) == 0) { Progres.paku |= bit; ubah = true; }
            if (ubah) Periksa();
        }

        void PeriksaDunia()
        {
            if (_ningsih == null) _ningsih = GameServices.Get<NingsihVirginiaAI>();
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            if (_ningsih != null)
            {
                if (!Progres.trust31 && _ningsih.Trust >= TrustM06) Progres.trust31 = true;
                if (!Progres.ningsihTerlihat && _andi != null && _ningsih.Tampak && Vector3.Distance(_andi.transform.position, _ningsih.transform.position) <= JarakLihatNingsih) Progres.ningsihTerlihat = true;
            }
            if (!Progres.sisiUtara && _andi != null && !WorldTerrainGenerator.DiBawahTanah(_andi.transform.position.y) && _andi.transform.position.z >= ZUtaraJurang) Progres.sisiUtara = true;
            PeriksaMisiSampingan();
        }

        void PeriksaMisiSampingan()
        {
            if (Progres.sampingan == null || Progres.sampingan.Length < 4) Progres.sampingan = new bool[4];
            var inv = GameServices.Get<TikarPandan3DInventory>();
            var vital = GameServices.Get<AndiVitalsAndSanity>();
            if (!Progres.sampingan[0] && AdaLore(Progres, 6) && Progres.trust31)
            {
                Progres.sampingan[0] = true;
                if (vital != null) vital.UbahSanity(15f);
                GameEvents.RaiseToast("Misi Sampingan Selesai: Kenangan Dewi");
            }
            if (!Progres.sampingan[1] && (Progres.keluarBunker || (inv != null && inv.Punya(ItemID.TandaPengenalRendra))))
            {
                Progres.sampingan[1] = true;
                if (inv != null && !inv.Punya(ItemID.TandaPengenalRendra)) inv.Tambah(ItemID.TandaPengenalRendra, 1);
                GameEvents.RaiseToast("Misi Sampingan Selesai: Jejak Rendra");
            }
            if (!Progres.sampingan[2] && ZonaPenangkal.AdaKemenyan(Vector3.zero) == false && HantuAI.Semua.Count > 0)
            {
                foreach (var h in HantuAI.Semua) if (!h.IsAlive) { Progres.sampingan[2] = true; if (vital != null) vital.UbahSanity(15f); GameEvents.RaiseToast("Misi Sampingan Selesai: Arwah Tersesat"); break; }
            }
            if (!Progres.sampingan[3] && _andi != null && ZonaPenangkal.AdaKemenyan(_andi.transform.position))
            {
                Progres.sampingan[3] = true;
                if (vital != null) vital.UbahSanity(10f);
                GameEvents.RaiseToast("Misi Sampingan Selesai: Persembahan Kemenyan");
            }
        }

        public void TandaiKeluarBunker() { if (Progres.keluarBunker) return; Progres.keluarBunker = true; Periksa(); }
        public void TandaiBossKalah() { if (Progres.bossKalah) return; Progres.bossKalah = true; Periksa(); }
        public void TandaiTertancap(int n) { Progres.tertancap = Mathf.Clamp(n, 0, 3); Periksa(); }
        public void TandaiSiegeSelesai() { if (Progres.siegeSelesai) return; Progres.siegeSelesai = true; Periksa(); }
        public void TandaiEnding(int e) { Progres.ending = Mathf.Clamp(e, 1, 3); Periksa(); }
        public int LoreMask { get { return Progres.lore; } }
    }
}
