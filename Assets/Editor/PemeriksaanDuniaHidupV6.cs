using System;
using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.World;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlasKeramat.EditorTools
{
    // Self-test Overhaul Visual Karakter & Dunia Hidup V6 (Langkah 7z19 + perbaikan Mixamo/MakeHuman/Banaspati/Mercusuar/Kuali).
    public static class PemeriksaanDuniaHidupV6
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            cek("Karakter V6: klasifikasi mata/sepatu & warna pakaian fallback Mixamo bukan putih polos",
                AnimasiMixamo.AdalahMaterialMata("Eye_Iris")
                && !AnimasiMixamo.AdalahMaterialMata("Eyelashes")
                && AnimasiMixamo.AdalahMaterialSepatu("Leather_Boots")
                && AnimasiMixamo.WarnaPakaianFallback(AnimasiMixamo.Peran.Pria, false, "Deden", "TopMat").g > 0.35f
                && AnimasiMixamo.WarnaPakaianFallback(AnimasiMixamo.Peran.Wanita, false, "Ningsih", "Shirt").r > 0.80f
                && AnimasiMixamo.WarnaPakaianFallback(AnimasiMixamo.Peran.Pria, false, "KiRogo", "Coat").r > 0.35f);

            var akSemua = Object.FindObjectsByType<AktivitasHidupKarakter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            cek("Karakter Hidup V6: AktivitasHidupKarakter terpasang (>= 8 karakter) & faktor dingin/napas valid",
                akSemua.Length >= 8
                && AktivitasHidupKarakter.FaktorDingin(22f, WeatherState.Cerah) > 0.5f
                && AktivitasHidupKarakter.FaktorDingin(11f, WeatherState.Cerah) == 0f
                && AktivitasHidupKarakter.FaktorDingin(11f, WeatherState.HujanBadai) > 0.7f
                && Mathf.Abs(AktivitasHidupKarakter.SudutNapas(1.0f, 0f, 0.5f)) > 0.1f);

            var eko = Object.FindFirstObjectByType<EkosistemRimbaLengkap>(FindObjectsInactive.Include);
            cek("Ekosistem Rimba V6: 64 satwa 4 kuadran (rusa, ayam, babi, rangkong, camar) & aturan jam/cuaca",
                eko != null && eko.JumlahSatwa >= 60
                && EkosistemRimbaLengkap.AktifPadaWaktu(EkosistemRimbaLengkap.JenisSatwa.AyamKampung, 10f, WeatherState.Cerah)
                && !EkosistemRimbaLengkap.AktifPadaWaktu(EkosistemRimbaLengkap.JenisSatwa.AyamKampung, 22f, WeatherState.Cerah)
                && !EkosistemRimbaLengkap.AktifPadaWaktu(EkosistemRimbaLengkap.JenisSatwa.CamarPantai, 12f, WeatherState.HujanBadai)
                && Mathf.Abs(EkosistemRimbaLengkap.SudutKepakSayap(0.2f, 0f, false)) > Mathf.Abs(EkosistemRimbaLengkap.SudutKepakSayap(0.2f, 0f, true)));

            var suasana = Object.FindFirstObjectByType<SuasanaHidupDesaDanPantai>(FindObjectsInactive.Include);
            cek("Suasana Desa & Pantai V6: jendela malam, jemuran batik, lentera & perahu ombak (>= 45 elemen)",
                suasana != null && suasana.JumlahElemenHidup >= 45
                && SuasanaHidupDesaDanPantai.FaktorLampuMalam(21f, WeatherState.Cerah) > 0.9f
                && SuasanaHidupDesaDanPantai.FaktorLampuMalam(12f, WeatherState.Cerah) == 0f
                && SuasanaHidupDesaDanPantai.FaktorLampuMalam(12f, WeatherState.HujanBadai) > 0.5f);

            var matSuar = AssetDatabase.LoadAssetAtPath<Material>(NusantaraMasterBuilder.FolderMat + "/M_BerkasSuar.mat");
            var matUap = AssetDatabase.LoadAssetAtPath<Material>(NusantaraMasterBuilder.FolderMat + "/M_UapJamu.mat");
            cek("Perbaikan Visual V6: BerkasSuar & UapJamu pakai KabutLembut + properti 12 lokasi terpasang",
                matSuar != null && matSuar.shader != null && matSuar.shader.name == "AlasKeramat/KabutLembut"
                && matUap != null && matUap.shader != null && matUap.shader.name == "AlasKeramat/KabutLembut"
                && GameObject.Find("DuniaHidupV6") != null
                && GameObject.Find("PedupaSesajen_0") != null
                && GameObject.Find("JemuranBatik_0") != null
                && GameObject.Find("PerahuTambatOmbak_0") != null);
        }
    }
}
