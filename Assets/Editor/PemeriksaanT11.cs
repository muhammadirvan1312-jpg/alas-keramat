using System;
using System.IO;
using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using AlasKeramat.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlasKeramat.EditorTools
{
    // Self-test T11 (12 §T11): MenuScreens, preset grafis (Rendah/Sedang/Tinggi/Ultra), pengali kesulitan (Damai/Normal/Sulit),
    // 6 kartu Cara Bermain, penyimpanan pengaturan PlayerPrefs, 3 slot simpan, panel debug 5 ketukan, AKTexturePostprocessor, dan Android build.
    public static partial class PemeriksaanT11
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            var menu = Object.FindFirstObjectByType<MenuScreens>(FindObjectsInactive.Include);
            cek("T11: MenuScreens & TombolJeda terpasang di scene", menu != null && GameObject.Find("TombolJeda") != null);

            // Preset grafis 11 §7.2
            var pRendah = MenuScreens.AmbilPreset(PresetGrafis.Rendah);
            var pSedang = MenuScreens.AmbilPreset(PresetGrafis.Sedang);
            var pTinggi = MenuScreens.AmbilPreset(PresetGrafis.Tinggi);
            var pUltra = MenuScreens.AmbilPreset(PresetGrafis.Ultra);
            cek("T11: Preset grafis 11 §7.2 (Rendah 0.7/0m, Sedang 0.95/35m, Tinggi 1.0/60m, Ultra 90m)",
                Mathf.Approximately(pRendah.renderScale, 0.7f) && Mathf.Approximately(pRendah.bayangan, 0f) && pRendah.targetFps == 30
                && Mathf.Approximately(pSedang.renderScale, 0.95f) && Mathf.Approximately(pSedang.bayangan, 35f) && pSedang.hdr
                && Mathf.Approximately(pTinggi.renderScale, 1.0f) && Mathf.Approximately(pTinggi.bayangan, 60f) && pTinggi.targetFps == 60
                && pUltra.bayangan >= 90f);

            // Kesulitan 05 §9
            cek("T11: Pengali kesulitan 05 §9 (Damai laju 0.7/dmg 0.5, Normal 1/1, Sulit 1.25/1.4)",
                Mathf.Approximately(MenuScreens.PengaliLajuVital(Difficulty.Damai), 0.7f)
                && Mathf.Approximately(MenuScreens.PengaliDamageMusuh(Difficulty.Damai), 0.5f)
                && Mathf.Approximately(MenuScreens.PengaliLajuVital(Difficulty.Normal), 1.0f)
                && Mathf.Approximately(MenuScreens.PengaliLajuVital(Difficulty.Sulit), 1.25f)
                && Mathf.Approximately(MenuScreens.PengaliDamageMusuh(Difficulty.Sulit), 1.4f));

            // Simulasi laju vital Damai vs Sulit
            var goV = new GameObject("UjiVitalKesulitan");
            var kesAwal = EnemyDirector.Kesulitan;
            try
            {
                var vDamai = goV.AddComponent<AndiVitalsAndSanity>();
                var kd = new AndiVitalsAndSanity.Keadaan { jam = 10f, hari = 2, cuaca = WeatherState.Berawan, biome = Biome.HutanCampuran };
                EnemyDirector.Kesulitan = Difficulty.Damai;
                float hAwal = vDamai.Hydration;
                for (int i = 0; i < 100; i++) vDamai.Tick(kd, 0.1f);
                float turunDamai = hAwal - vDamai.Hydration;

                EnemyDirector.Kesulitan = Difficulty.Sulit;
                float hAwal2 = vDamai.Hydration;
                for (int i = 0; i < 100; i++) vDamai.Tick(kd, 0.1f);
                float turunSulit = hAwal2 - vDamai.Hydration;
                cek("T11: Vital haus turun lebih lambat di Damai daripada Sulit", turunDamai > 0f && turunSulit > turunDamai * 1.5f);
            }
            finally
            {
                EnemyDirector.Kesulitan = kesAwal;
                Object.DestroyImmediate(goV);
            }

            // Cara bermain 6 kartu & kredit
            bool caraOk = MenuScreens.JudulCara.Length == 6 && MenuScreens.IsiCara.Length == 6;
            for (int i = 0; i < 6; i++) caraOk &= MenuScreens.JudulCara[i].Length > 5 && MenuScreens.IsiCara[i].Length > 40;
            cek("T11: 6 kartu Cara Bermain & teks Kredit lengkap", caraOk && MenuScreens.TeksKredit.Contains("ALAS KERAMAT"));

            if(menu!=null)CekPengaturanT11(menu,cek);

            // Slot simpan 1-3
            int slotAwal = SaveSystem.SlotAktif;
            string pathUji=SaveSystem.PathSlot(3),bakUji=pathUji+".bak";
            byte[] isiAwal=File.Exists(pathUji)?File.ReadAllBytes(pathUji):null;
            byte[] bakAwal=File.Exists(bakUji)?File.ReadAllBytes(bakUji):null;
            try
            {
                bool pathBerbeda = SaveSystem.PathSlot(1) != SaveSystem.PathSlot(2) && SaveSystem.PathSlot(2) != SaveSystem.PathSlot(3);
                SaveSystem.HapusSlot(3);
                bool kosongAwal = !SaveSystem.AdaSlot(3);
                File.WriteAllText(SaveSystem.PathSlot(3), "{\"versi\":1,\"waktu\":\"2026-11-18T00:00:00Z\",\"data\":[]}");
                bool adaSetelah = SaveSystem.AdaSlot(3) && SaveSystem.RingkasanSlot(3).Contains("Tersimpan");
                SaveSystem.HapusSlot(3);
                cek("T11: 3 slot simpan mandiri (simpan, ringkasan, hapus)", pathBerbeda && kosongAwal && adaSetelah && !SaveSystem.AdaSlot(3));
            }
            finally
            {
                if(isiAwal!=null)File.WriteAllBytes(pathUji,isiAwal);else if(File.Exists(pathUji))File.Delete(pathUji);
                if(bakAwal!=null)File.WriteAllBytes(bakUji,bakAwal);else if(File.Exists(bakUji))File.Delete(bakUji);
                SaveSystem.SlotAktif = slotAwal;
            }

            // Ketuk pojok kiri-atas 5x membuka panel debug
            if (menu != null)
            {
                menu.TutupDebug();
                for (int i = 0; i < MenuScreens.KetukDebugWajib; i++) menu.KetukPojokDebug();
                var pDbg = GameObject.Find("PanelDebugHP");
                bool terbuka = pDbg != null && pDbg.activeSelf;
                menu.TutupDebug();
                cek("T11: Ketuk pojok 5x membuka Panel Debug HP", terbuka);
            }

            // Getar 09 §10 & AKTexturePostprocessor 15 §9 & Android build 11 §15
            cek("T11: Getar haptik >= 20 dmg, AKTexturePostprocessor, & NusantaraAndroidBuild",
                MenuScreens.PerluGetarDamage(25f, true) && !MenuScreens.PerluGetarDamage(10f, true) && !MenuScreens.PerluGetarDamage(30f, false)
                && AKTexturePostprocessor.JalurOverride("Assets/Resources/Override/Textures/BarkTeak_Normal.png")
                && AKTexturePostprocessor.AdalahNormalMap("Assets/Resources/Override/Textures/BarkTeak_Normal.png")
                && NusantaraAndroidBuild.PackageId == "com.alaskeramat.game");
        }
    }
}
