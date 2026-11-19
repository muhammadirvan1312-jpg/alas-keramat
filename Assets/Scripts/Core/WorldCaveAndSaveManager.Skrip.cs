using AlasKeramat.AI;
using AlasKeramat.Inventory;
using UnityEngine;
namespace AlasKeramat.Core
{
    // Kejadian terskrip gua (03 §10, 13 §8): Pocong jatuh saat Lore #4, penjaga altar UG1, penyergap, alarm Ruang Segel UG2.
    public partial class WorldCaveAndSaveManager
    {
        public static readonly Vector3 R1UG1 = new Vector3(-15f, 0f, 0f), R4UG1 = new Vector3(60f, 0f, -14f), R4UG2 = new Vector3(12f, 0f, 34f);
        public const float RadiusR4 = 8f, TinggiLangitLangit = 6f;

        void Lore(LoreID id)
        {
            if (id != LoreID.L04 || Ada(B_PocongJatuh) || _andi == null || Kini != UndergroundId.UG1_GuaSungai) return;
            Set(B_PocongJatuh);
            var dir = GameServices.Get<EnemyDirector>();
            Vector3 a = _andi.transform.position;
            Vector3 p = a + _andi.transform.forward * 3f + Vector3.up * TinggiLangitLangit;
            if (dir != null) dir.PanggilHantuDi(0, p);
            GameEvents.RaiseSubtitle("Andi: (Ada yang jatuh dari langit-langit...)");
        }

        void Item(ItemID id, int n)
        {
            if(SaveSystem.SedangMemuat)return;
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv == null || !inv.Punya(id)) return;
            var dir = GameServices.Get<EnemyDirector>();
            var jam = GameServices.Get<TimeOfDayClock>();
            if (id == ItemID.PakuKeramat1 && !Ada(B_MalamDarah1))
            {
                Set(B_MalamDarah1);
                if (jam != null) EnemyDirector.MalamDarahPaksa = EnemyDirector.MalamBerikutnya(jam.Hari, jam.Jam);
                if (!Ada(B_Penyergap) && Kini == UndergroundId.UG1_GuaSungai && dir != null)
                {
                    Set(B_Penyergap);
                    dir.PanggilHantuDi(0, Lokal(UndergroundId.UG1_GuaSungai, new Vector3(56f, TinggiLangitLangit, -8f)));
                }
                GameEvents.RaiseToast("Paku Keramat pertama. Hutan menjadi gelisah...");
            }
            if (id == ItemID.PakuKeramat2 && !Ada(B_MalamDarah2))
            {
                Set(B_MalamDarah2);
                if (jam != null) EnemyDirector.MalamDarahPaksa = EnemyDirector.MalamBerikutnya(jam.Hari, jam.Jam);
                if (!Ada(B_Alarm) && Kini == UndergroundId.UG2_Bunker) Alarm(dir);
            }
        }

        // 03 §10 UG2 R4: alarm, lampu merah, dinding runtuh, 3 Abdi Sekte + 2 Pocong Gua menyerbu.
        void Alarm(EnemyDirector dir)
        {
            Set(B_Alarm);
            var k = kompleks != null && kompleks.Length > 2 ? kompleks[2] : null;
            if (k != null)
            {
                var alarm = k.Find("LampuAlarm");
                if (alarm != null) alarm.gameObject.SetActive(true);
                var runtuh = k.Find("DindingRuntuh");
                if (runtuh != null) runtuh.gameObject.SetActive(true);
            }
            var audio = GameServices.Get<ProceduralAudioSynth>();
            if (audio != null && _andi != null) audio.Play(ProceduralAudioSynth.KlipKentongan, _andi.transform.position, AudioCategory.Efek, 1f, 1.4f, 9);
            GameEvents.RaiseToast("ALARM! Dinding runtuh!");
            if (dir == null || _andi == null) return;
            Vector3 a = _andi.transform.position;
            for (int i = 0; i < 3; i++)
                dir.SpawnDi(EnemyArchetype.AbdiSekte, Lokal(UndergroundId.UG2_Bunker, R4UG2 + new Vector3(-6f + i * 6f, 0.2f, 7f)), a, i == 0);
            for (int i = 0; i < 2; i++)
                dir.PanggilHantuDi(0, Lokal(UndergroundId.UG2_Bunker, R4UG2 + new Vector3(i == 0 ? -7f : 7f, 4f, -6f)));
        }

        void Skrip()
        {
            if (Kini == UndergroundId.UG1_GuaSungai && !Ada(B_PenjagaR4))
            {
                Vector3 r4 = Lokal(UndergroundId.UG1_GuaSungai, R4UG1);
                if ((_andi.transform.position - r4).sqrMagnitude < RadiusR4 * RadiusR4)
                {
                    Set(B_PenjagaR4);
                    var dir = GameServices.Get<EnemyDirector>();
                    if (dir != null) for (int i = 0; i < 2; i++) dir.PanggilHantuDi(0, r4 + new Vector3(i == 0 ? -5f : 5f, TinggiLangitLangit, 4f));
                    GameEvents.RaiseSubtitle("Andi: (Dua sosok berkafan... menjaga altar.)");
                }
            }
        }
    }
}
