using System;
using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.World;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlasKeramat.EditorTools
{
    // Self-test Overhaul Dunia Nyata V9 (Langkah 7z22 — 320+ props 4K scan, 12 NPC MakeHuman G3, 28 satwa FBX asli, suara 24 jam & sinyal cakrawala).
    public static class PemeriksaanDuniaRealV9
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            var warga = Object.FindFirstObjectByType<WargaHidupPulau>(FindObjectsInactive.Include);
            cek("Dunia Nyata V9: 12 NPC MakeHuman G3 bertulang asli (WargaHidupPulau) & obor malam",
                warga != null && warga.JumlahWarga >= 12
                && WargaHidupPulau.PerluOborWarga(20f, WeatherState.Cerah)
                && !WargaHidupPulau.PerluOborWarga(12f, WeatherState.Cerah));

            var satwa = Object.FindFirstObjectByType<SatwaAsliPulauV9>(FindObjectsInactive.Include);
            cek("Dunia Nyata V9: 28 habitat satwa bertulang/FBX asli lintas 4 kuadran (SatwaAsliPulauV9)",
                satwa != null && satwa.JumlahHabitat >= 28
                && SatwaAsliPulauV9.BolehMuncul(400f, 85f)
                && !SatwaAsliPulauV9.BolehMuncul(10000f, 85f));

            var suara = Object.FindFirstObjectByType<SuasanaSuaraHidupPulau>(FindObjectsInactive.Include);
            cek("Dunia Nyata V9: lanskap suara hidup 24 jam (fajar kokok, maghrib gamelan/kentongan)",
                suara != null && suara.JumlahTitikSuara >= 10
                && SuasanaSuaraHidupPulau.PilihKlipAmbienWaktu(6.5f, true, false) == ProceduralAudioSynth.KlipKokok
                && SuasanaSuaraHidupPulau.PilihKlipAmbienWaktu(18.0f, false, true) == ProceduralAudioSynth.KlipGamelanJauh);

            var sinyal = Object.FindFirstObjectByType<SinyalCakrawalaPulau>(FindObjectsInactive.Include);
            cek("Dunia Nyata V9: sinyal asap/pendaran cakrawala & wadah DuniaRealV9 terpasang",
                sinyal != null && sinyal.JumlahSinyal >= 8
                && SinyalCakrawalaPulau.KekuatanPendarCakrawala(21f) == 1f
                && SinyalCakrawalaPulau.KekuatanPendarCakrawala(12f) == 0f
                && GameObject.Find("DuniaRealV9") != null);
        }
    }
}
