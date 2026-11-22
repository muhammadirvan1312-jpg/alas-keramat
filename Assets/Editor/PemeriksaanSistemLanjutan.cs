using System;
using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlasKeramat.EditorTools
{
    // Self-test Sistem Lanjutan SOTF: Kombo 3-serangan, Level Survival, Radio Darurat, Pohon Keramat, & Audio Spasial.
    public static class PemeriksaanSistemLanjutan
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            cek("Sistem Lanjutan: kombo 3-serangan (x2, finisher x3, penalti lelah) & terpasang di Andi",
                SistemKomboDanStagger.PengaliKombo(3, 1f) > SistemKomboDanStagger.PengaliKombo(2, 1f)
                && SistemKomboDanStagger.PengaliKombo(2, 1f) > SistemKomboDanStagger.PengaliKombo(1, 1f)
                && SistemKomboDanStagger.PengaliKombo(3, 0.1f) < SistemKomboDanStagger.PengaliKombo(3, 1f)
                && Object.FindFirstObjectByType<SistemKomboDanStagger>(FindObjectsInactive.Include) != null);

            cek("Sistem Lanjutan: Level Survival 4 atribut tersimpan & terpasang di Andi",
                LevelSurvivalAndi.HitungLevelDariXP(0f) == 1
                && LevelSurvivalAndi.HitungLevelDariXP(250f) == 3
                && LevelSurvivalAndi.HitungLevelDariXP(9999f) == LevelSurvivalAndi.LevelMaks
                && Object.FindFirstObjectByType<LevelSurvivalAndi>(FindObjectsInactive.Include) != null);

            cek("Sistem Lanjutan: 4 stasiun radio darurat & 3 pohon keramat sesajen",
                RadioDaruratPulau.TeksTransmisi(0).Contains("RADIO")
                && Object.FindObjectsByType<RadioDaruratPulau>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length >= 4
                && Object.FindObjectsByType<PohonKeramatSesajen>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length >= 3);

            Vector3 src; float pt;
            cek("Sistem Lanjutan: audio spasial mega-landmark aktif sesuai jarak & jam",
                AudioDinamisRimba.PilihKlipLandmark(Vector3.zero, Vector3.zero, Vector3.one * 500f, Vector3.one * 500f, Vector3.one * 500f, 12f, out src, out pt) == ProceduralAudioSynth.KlipGuruh
                && AudioDinamisRimba.PilihKlipLandmark(Vector3.zero, Vector3.one * 500f, Vector3.zero, Vector3.one * 500f, Vector3.one * 500f, 22f, out src, out pt) == ProceduralAudioSynth.KlipKentongan
                && Object.FindFirstObjectByType<AudioDinamisRimba>(FindObjectsInactive.Include) != null);
        }
    }
}
