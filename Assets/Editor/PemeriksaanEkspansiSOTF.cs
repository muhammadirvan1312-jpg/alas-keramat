using System;
using AlasKeramat.AI;
using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Player;
using AlasKeramat.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlasKeramat.EditorTools
{
    // Self-test Ekspansi Besar SOTF: biota air (ikan/kepiting), serangga siang, jejak kaki,
    // perangkap survival, kompas suhu HUD, 3 kemah sekte hidup, & props 2K CC0 Poly Haven.
    public static class PemeriksaanEkspansiSOTF
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            var pRenang = BiotaAirHidup.HitungPosisiRenang(Vector3.zero, 2f, 0.5f, false);
            var pKep = BiotaAirHidup.HitungPosisiRenang(Vector3.zero, 2f, 0.5f, true);
            var biota = Object.FindFirstObjectByType<BiotaAirHidup>(FindObjectsInactive.Include);
            cek("Ekspansi SOTF: biota air (ikan tangkap & kepiting pantai)",
                pRenang.magnitude > 1.5f && pKep.magnitude > 0.5f
                && biota != null && biota.JumlahEntitas >= 30
                && Object.FindObjectsByType<IkanTangkap>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length >= 20);

            var serangga = Object.FindFirstObjectByType<SeranggaSiang>(FindObjectsInactive.Include);
            cek("Ekspansi SOTF: kupu-kupu & capung aktif siang cerah, mati malam/badai",
                SeranggaSiang.AktifSiang(10f, WeatherState.Cerah)
                && !SeranggaSiang.AktifSiang(22f, WeatherState.Cerah)
                && !SeranggaSiang.AktifSiang(10f, WeatherState.HujanBadai)
                && serangga != null && serangga.Jumlah >= 24);

            cek("Ekspansi SOTF: jejak kaki basah/pasir & komponen terpasang di Andi",
                JejakKakiAndi.BolehCetakJejak(Biome.Pantai, WeatherState.Cerah, true, false)
                && JejakKakiAndi.BolehCetakJejak(Biome.HutanCampuran, WeatherState.HujanBadai, true, false)
                && !JejakKakiAndi.BolehCetakJejak(Biome.HutanCampuran, WeatherState.Cerah, true, false)
                && !JejakKakiAndi.BolehCetakJejak(Biome.Pantai, WeatherState.Cerah, false, false)
                && Object.FindFirstObjectByType<JejakKakiAndi>(FindObjectsInactive.Include) != null);

            var traps = Object.FindObjectsByType<PerangkapSurvival>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            cek("Ekspansi SOTF: 10 perangkap survival (jerat hewan & duri sekte)",
                PerangkapSurvival.DamageUntukTarget(PerangkapSurvival.JenisPerangkap.DuriSekte, true, true) >= 40f
                && PerangkapSurvival.DamageUntukTarget(PerangkapSurvival.JenisPerangkap.DuriSekte, false, true) == 0f
                && PerangkapSurvival.DamageUntukTarget(PerangkapSurvival.JenisPerangkap.JeratHewan, true, true) == 0f
                && traps.Length >= 10);

            cek("Ekspansi SOTF: kompas kardinal & suhu lingkungan HUD",
                KompasSuhuHUD.ArahKardinal(0f) == "U"
                && KompasSuhuHUD.ArahKardinal(90f) == "T"
                && KompasSuhuHUD.ArahKardinal(180f) == "S"
                && KompasSuhuHUD.HitungSuhuLingkungan(12f, WeatherState.Cerah, false, false) > KompasSuhuHUD.HitungSuhuLingkungan(23f, WeatherState.Cerah, false, false)
                && KompasSuhuHUD.HitungSuhuLingkungan(23f, WeatherState.Cerah, false, true) > KompasSuhuHUD.HitungSuhuLingkungan(23f, WeatherState.Cerah, false, false)
                && Object.FindFirstObjectByType<KompasSuhuHUD>(FindObjectsInactive.Include) != null);

            cek("Ekspansi SOTF: 3 kemah sekte hidup & jalan batu desa",
                GameObject.Find("KemahSekteHidup_0") != null
                && GameObject.Find("KemahSekteHidup_1") != null
                && GameObject.Find("KemahSekteHidup_2") != null
                && GameObject.Find("LempengDesa_0") != null);

            if (NusantaraMasterBuilder.JumlahPropEkspansi > 0)
            {
                cek("Ekspansi SOTF: props 2K CC0 Poly Haven terpasang (>= 25)",
                    NusantaraMasterBuilder.JumlahPropEkspansi >= 25
                    && NusantaraMasterBuilder.NamaPropEkspansi.Length == 15);
            }
        }
    }
}
