using System;
using AlasKeramat.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlasKeramat.EditorTools
{
    // Self-test Overhaul Visual & Mega-Landmark Nusantara.
    public static class PemeriksaanOverhaulVisual
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            var atm = Object.FindFirstObjectByType<KabutAirTerjunDanOmbak>(FindObjectsInactive.Include);
            cek("Overhaul Visual: 5 mega-landmark (Air Terjun, Candi, Jembatan Gantung, Bangkai Kapal, Pendopo)",
                GameObject.Find("AirTerjunRimba") != null
                && GameObject.Find("CandiRuntuhNusantara") != null
                && GameObject.Find("JembatanGantungRimba") != null
                && GameObject.Find("BangkaiKapalPantai") != null
                && GameObject.Find("PendopoDesaMati") != null);

            cek("Overhaul Visual: kabut air terjun, ombak pantai & lentera gantung berayun",
                Mathf.Abs(KabutAirTerjunDanOmbak.SudutAyunLentera(1.0f, 0f, 1f)) > 2f
                && Mathf.Abs(KabutAirTerjunDanOmbak.GeserOmbak(1.0f, 0f)) > 0.5f
                && atm != null && atm.JumlahLentera >= 8 && atm.JumlahOmbak >= 8);

            var tebing = GameObject.Find("FormasiTebingDanAkar");
            cek("Overhaul Visual: formasi batu tebing & akar raksasa (>= 30)",
                tebing != null && tebing.transform.childCount >= 30);
        }
    }
}
