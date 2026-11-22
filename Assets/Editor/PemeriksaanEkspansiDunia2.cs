using System;
using AlasKeramat.AI;
using AlasKeramat.Building;
using AlasKeramat.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlasKeramat.EditorTools
{
    // Self-test Ekspansi Dunia 2 (1 GB aset CC0 + sistem survival SOTF lanjutan).
    public static class PemeriksaanEkspansiDunia2
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            cek("Ekspansi Dunia 2: sistem musim dinamis & genangan hujan",
                SistemMusimDinamis.FaseHari(1) == SistemMusimDinamis.FaseMusim.Kemarau
                && SistemMusimDinamis.FaseHari(5) == SistemMusimDinamis.FaseMusim.Pancaroba
                && SistemMusimDinamis.FaseHari(7) == SistemMusimDinamis.FaseMusim.Penghujan
                && SistemMusimDinamis.TargetGenangan(WeatherState.HujanBadai, SistemMusimDinamis.FaseMusim.Penghujan) == 1f
                && SistemMusimDinamis.TargetGenangan(WeatherState.Cerah, SistemMusimDinamis.FaseMusim.Kemarau) == 0f
                && Object.FindFirstObjectByType<SistemMusimDinamis>(FindObjectsInactive.Include) != null);

            cek("Ekspansi Dunia 2: 6 rak jemuran daging/ikan aktif",
                RakPengeringDanMasak.LajuKering(WeatherState.Cerah) > RakPengeringDanMasak.LajuKering(WeatherState.HujanBadai)
                && RakPengeringDanMasak.HasilKering(RakPengeringDanMasak.IsiRak.IkanKering) == ItemID.IkanKering
                && Object.FindObjectsByType<RakPengeringDanMasak>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length >= 6);

            cek("Ekspansi Dunia 2: 6 penampung air hujan & 6 bubu ikan aktif",
                PenampungAirHujan.LajuIsi(WeatherState.HujanBadai) > PenampungAirHujan.LajuIsi(WeatherState.Cerah)
                && BubuIkanAktif.HitungTangkap(1, 160f) == 3
                && Object.FindObjectsByType<PenampungAirHujan>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length >= 6
                && Object.FindObjectsByType<BubuIkanAktif>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length >= 6);

            cek("Ekspansi Dunia 2: 8 tiang suar penanda jalan",
                Object.FindObjectsByType<TiangPenandaJalan>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length >= 8);

            cek("Ekspansi Dunia 2: reputasi sekte dinamis & aksi companion Deden/Ningsih",
                ReputasiSekte.Klasifikasi(10f) == ReputasiSekte.TingkatAncaman.Tenang
                && ReputasiSekte.Klasifikasi(80f) == ReputasiSekte.TingkatAncaman.Murka
                && ReputasiSekte.PengaliJarakDeteksi(90f) > ReputasiSekte.PengaliJarakDeteksi(10f)
                && AksiTemanHidup.PerluPeringatanBahaya(18f, 0f)
                && !AksiTemanHidup.PerluPeringatanBahaya(40f, 0f)
                && Object.FindFirstObjectByType<ReputasiSekte>(FindObjectsInactive.Include) != null
                && Object.FindFirstObjectByType<AksiTemanHidup>(FindObjectsInactive.Include) != null);

            if (NusantaraMasterBuilder.JumlahPropDunia2 > 0)
            {
                cek("Ekspansi Dunia 2: props 4K CC0 Poly Haven terpasang (>= 24)",
                    NusantaraMasterBuilder.JumlahPropDunia2 >= 24
                    && NusantaraMasterBuilder.NamaPropDunia2.Length == 24);
            }
        }
    }
}
