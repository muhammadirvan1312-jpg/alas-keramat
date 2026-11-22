using System;
using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.World;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlasKeramat.EditorTools
{
    // Self-test Overhaul Arsitektur, Interior 4K, Kehidupan Sosial Companion & Cuaca Hidup V10 (Langkah 7z23).
    public static class PemeriksaanDuniaV10
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            var sosial = Object.FindObjectsByType<InteraksiSosialTeman>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            cek("Dunia V10: InteraksiSosialTeman terpasang pada Kang Deden & Ningsih + hadiah herbal & sapaan cuaca",
                sosial.Length >= 2
                && InteraksiSosialTeman.PilihHadiahNingsih(0) == ItemID.DaunSirih
                && InteraksiSosialTeman.PilihHadiahNingsih(3) == ItemID.KelapaMuda
                && InteraksiSosialTeman.PilihSapaan(InteraksiSosialTeman.PeranTeman.KangDeden, 22f, WeatherState.Cerah, 0).Contains("Kang Deden"));

            var catatan = Object.FindObjectsByType<CatatanDuniaHidup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            cek("Dunia V10: 12 CatatanDuniaHidup interaktif di 12 lokasi utama",
                catatan.Length >= 12
                && CatatanDuniaHidup.FormatPrompt("Uji", false).Contains("BACA CATATAN"));

            var cuaca = Object.FindFirstObjectByType<CuacaHidupDunia>(FindObjectsInactive.Include);
            cek("Dunia V10: CuacaHidupDunia (petir badai & kabut reaktif hujan) + arsitektur upgrade V10",
                cuaca != null && cuaca.JumlahKabutDinamis >= 4
                && CuacaHidupDunia.PengaliKabutCuaca(WeatherState.HujanBadai) > CuacaHidupDunia.PengaliKabutCuaca(WeatherState.Cerah)
                && GameObject.Find("RumahNelayanTimur_0") != null
                && GameObject.Find("MenaraPengawasApi") != null
                && GameObject.Find("KuilBambuUtara") != null);
        }
    }
}
