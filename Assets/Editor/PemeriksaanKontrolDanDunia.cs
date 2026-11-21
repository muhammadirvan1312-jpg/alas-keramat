using System;
using AlasKeramat.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlasKeramat.EditorTools
{
    // Self-test Overhaul Kontrol Mobile Android & Pengayaan Visual Dunia.
    public static class PemeriksaanKontrolDanDunia
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            Vector2 lambat = ZonaGeser.HitungKurvaGeser(new Vector2(4f, 0f));
            Vector2 cepat = ZonaGeser.HitungKurvaGeser(new Vector2(40f, 0f));
            cek("Kontrol Android: kurva akselerasi kamera (presisi lambat, flick cepat dipercepat)",
                Mathf.Approximately(lambat.x, 4f) && cepat.x > 48f);

            cek("Kontrol Android: syarat auto-sprint lock joystick",
                ZonaJoystick.SyaratKunciLari(ZonaJoystick.RadiusGerak * 1.5f, 10f)
                && !ZonaJoystick.SyaratKunciLari(ZonaJoystick.RadiusGerak * 0.8f, 10f)
                && !ZonaJoystick.SyaratKunciLari(ZonaJoystick.RadiusGerak * 1.5f, 70f));

            cek("Kontrol Android: 3 tombol cepat (Senter/Auto-Lari/Obat) terpasang di HUD",
                GameObject.Find("TombolSenterCepat") != null
                && GameObject.Find("TombolAutoLari") != null
                && GameObject.Find("TombolObatCepat") != null);

            var pen = Object.FindFirstObjectByType<PenandaLandmarkHUD>(FindObjectsInactive.Include);
            float j;
            int idx = PenandaLandmarkHUD.CariTerdekat(Vector3.zero, new[]
            {
                new PenandaLandmarkHUD.TitikLandmark { nama = "A", posisi = new Vector3(0f, 0f, 100f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "B", posisi = new Vector3(0f, 0f, 20f) }
            }, out j);
            cek("Navigasi HUD & Dunia: Penanda Landmark, patok jalur, bunga tropis & pos penjaga",
                idx == 1 && Mathf.Approximately(j, 20f)
                && pen != null && pen.JumlahLandmark >= 8
                && NusantaraMasterBuilder.JumlahPatokJalan >= 20
                && NusantaraMasterBuilder.JumlahBungaTropis >= 40
                && NusantaraMasterBuilder.JumlahPosPenjaga >= 2);
        }
    }
}
