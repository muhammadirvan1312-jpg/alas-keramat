using System;
using AlasKeramat.AI;
using AlasKeramat.Building;
using AlasKeramat.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlasKeramat.EditorTools
{
    // Self-test 6 fitur gameplay besar baru SOTF: Zipline, Perahu Dayung, Rumah Pohon Aman,
    // Sarang & Jejak Pemangsa, Boss Mini Buto Ijo, dan Suar Sinyal Darurat.
    public static class PemeriksaanFiturBaruSOTF
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            Vector3 mid = ZiplineRimba.PosisiDiKabel(Vector3.zero, new Vector3(0f, 0f, 100f), 0.5f);
            cek("Fitur Baru SOTF: 5 jalur Zipline & kurva kabel",
                mid.y < -0.5f
                && Object.FindObjectsByType<ZiplineRimba>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length >= 5);

            cek("Fitur Baru SOTF: 3 Perahu Dayung aktif",
                PerahuDayung.KecepatanEfektif(1f) > 5f
                && PerahuDayung.KecepatanEfektif(-1f) < 0f
                && Object.FindObjectsByType<PerahuDayung>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length >= 3);

            cek("Fitur Baru SOTF: 3 Rumah Pohon Aman (tangga panjat & kasur tidur/save)",
                Object.FindObjectsByType<TanggaRumahPohon>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length >= 6
                && Object.FindObjectsByType<KasurRumahPohon>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length >= 3);

            cek("Fitur Baru SOTF: 4 Sarang Pemangsa & 8 Jejak Lacak",
                JejakBuruan.FormatPetunjuk(Vector3.zero, new Vector3(0f, 0f, 50f), "Harimau").Contains("U")
                && Object.FindObjectsByType<SarangPemangsa>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length >= 4
                && Object.FindObjectsByType<JejakBuruan>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length >= 8);

            cek("Fitur Baru SOTF: 2 Boss Mini Buto Ijo (lemah api/pusaka & Ground Slam)",
                BossMiniButoIjo.HitungDamageMasuk(20f, DamageType.Api) > 25f
                && BossMiniButoIjo.HitungDamageMasuk(20f, DamageType.Gaib) > 30f
                && Object.FindObjectsByType<BossMiniButoIjo>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length >= 2);

            cek("Fitur Baru SOTF: 3 Suar Sinyal Darurat pengusir hantu",
                Object.FindObjectsByType<SuarSinyalDarurat>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length >= 3);
        }
    }
}
