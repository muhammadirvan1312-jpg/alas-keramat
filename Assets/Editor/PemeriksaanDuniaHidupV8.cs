using System;
using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.World;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlasKeramat.EditorTools
{
    // Self-test Overhaul Dunia Hidup V8 (Langkah 7z21 + peningkatan kapasitas EcosystemDirector).
    public static class PemeriksaanDuniaHidupV8
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            cek("Dunia Hidup V8: kapasitas EcosystemDirector naik (>= 40 satwa aktif)",
                EcosystemDirector.MaksAktif >= 40);

            var kj = Object.FindFirstObjectByType<KejadianHidupPulau>(FindObjectsInactive.Include);
            cek("Dunia Hidup V8: KejadianHidupPulau (>= 72 elemen: 24 burung migrasi-V, 36 serbuk alam & 12 semak kaget)",
                kj != null && kj.JumlahElemenKejadian >= 72
                && KejadianHidupPulau.WaktuMigrasiAktif(8f, WeatherState.Cerah)
                && !KejadianHidupPulau.WaktuMigrasiAktif(8f, WeatherState.HujanBadai)
                && KejadianHidupPulau.TinggiKagetBurung(1f) > 10f);

            var dk = Object.FindFirstObjectByType<DinamikaKawananAlam>(FindObjectsInactive.Include);
            cek("Dunia Hidup V8: DinamikaKawananAlam (>= 30 satwa: 18 migrasi, 6 predator & 6 reptil berjemur)",
                dk != null && dk.JumlahSatwaDinamika >= 30
                && DinamikaKawananAlam.FaseMigrasiHarian(7f) == 0
                && DinamikaKawananAlam.FaseMigrasiHarian(12f) == 1
                && DinamikaKawananAlam.FaseMigrasiHarian(17.5f) == 2
                && DinamikaKawananAlam.ReptilHarusMasukAir(25f, WeatherState.Cerah, 12f)
                && !DinamikaKawananAlam.ReptilHarusMasukAir(400f, WeatherState.Cerah, 12f));

            var titik = Object.FindObjectsByType<TitikKehidupanInteraktif>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            cek("Dunia Hidup V8: 24 TitikKehidupanInteraktif (10 sesajen, 8 pohon buah guncang, 6 api penyintas)",
                titik.Length >= 24
                && TitikKehidupanInteraktif.TeksPromptUntuk(TitikKehidupanInteraktif.JenisTitik.PohonBuahGuncang, "Pantai", true).Contains("GUNCANG"));

            var hy = Object.FindFirstObjectByType<KeanekaragamanRimbaV8>(FindObjectsInactive.Include);
            cek("Dunia Hidup V8: KeanekaragamanRimbaV8 (>= 84 hayati: 16 nuri, 32 kupu, 24 sulur & 12 kura) + 22 Tugu Jalur",
                hy != null && hy.JumlahHayati >= 84
                && KeanekaragamanRimbaV8.SeranggaAktifSiang(10f, WeatherState.Cerah)
                && !KeanekaragamanRimbaV8.SeranggaAktifSiang(22f, WeatherState.Cerah)
                && GameObject.Find("DuniaHidupV8") != null
                && GameObject.Find("TuguJalur_0") != null
                && GameObject.Find("TuguJalur_21") != null);
        }
    }
}
