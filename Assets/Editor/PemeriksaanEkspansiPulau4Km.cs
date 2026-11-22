using System;
using AlasKeramat.AI;
using AlasKeramat.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlasKeramat.EditorTools
{
    // Self-test Ekspansi Pulau 4 km2 (5 zona baru di 4 kuadran luar + jarak pandang diperluas + ekosistem kawanan & burung langit).
    public static class PemeriksaanEkspansiPulau4Km
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            cek("Ekspansi Pulau 4 km2: jarak pandang kamera diperluas di semua preset",
                MenuScreens.AmbilPreset(PresetGrafis.Rendah).farKamera >= 180f
                && MenuScreens.AmbilPreset(PresetGrafis.Ultra).farKamera >= 440f);

            cek("Ekspansi Pulau 4 km2: 5 zona besar baru di 4 kuadran luar",
                GameObject.Find("DesaNelayanTimur") != null
                && GameObject.Find("PosGunungKapur") != null
                && GameObject.Find("PerkebunanTimurLaut") != null
                && GameObject.Find("PulauKarangSelatan") != null
                && GameObject.Find("KuilBambuUtara") != null);

            var eko = Object.FindFirstObjectByType<EkosistemHidupPulau>(FindObjectsInactive.Include);
            cek("Ekspansi Pulau 4 km2: ekosistem 18 satwa kawanan & 20 burung langit",
                EkosistemHidupPulau.PosisiBurung(Vector3.zero, 20f, 0.5f).magnitude > 15f
                && eko != null && eko.JumlahSatwa >= 18 && eko.JumlahBurung >= 20);
        }
    }
}
