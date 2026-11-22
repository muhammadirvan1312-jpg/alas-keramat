using System;
using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.World;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlasKeramat.EditorTools
{
    // Self-test Overhaul Dunia Hidup V7 (22/22 Lokasi + 4 Sistem Runtime Baru).
    public static class PemeriksaanDuniaHidupV7
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            var agen = Object.FindFirstObjectByType<AktivitasDesaSekte>(FindObjectsInactive.Include);
            cek("Dunia Hidup V7: AktivitasDesaSekte (>= 16 agen: kurir sekte, anjing kampung & gagak atap kaget)",
                agen != null && agen.JumlahAgen >= 16
                && AktivitasDesaSekte.HarusTerbangKaget(100f, 14f)
                && !AktivitasDesaSekte.HarusTerbangKaget(400f, 14f)
                && AktivitasDesaSekte.OborHarusNyala(20f, WeatherState.Cerah)
                && !AktivitasDesaSekte.OborHarusNyala(12f, WeatherState.Cerah));

            var biota = Object.FindFirstObjectByType<IkanLompatDanKepiting>(FindObjectsInactive.Include);
            cek("Dunia Hidup V7: IkanLompatDanKepiting (>= 44 biota: ikan lompat, kepiting pantai & capung)",
                biota != null && biota.JumlahBiota >= 44
                && IkanLompatDanKepiting.TinggiLompatIkan(0.12f, 1.2f) > 0.8f
                && IkanLompatDanKepiting.TinggiLompatIkan(0.65f, 1.2f) < 0f);

            var mek = Object.FindFirstObjectByType<MesinDanKincirHidup>(FindObjectsInactive.Include);
            cek("Dunia Hidup V7: MesinDanKincirHidup (roda pabrik, kincir air, turbin, parabola, suar & jemuran ikan)",
                mek != null && mek.JumlahMekanis >= 28
                && MesinDanKincirHidup.PengaliAnginCuaca(WeatherState.HujanBadai) > MesinDanKincirHidup.PengaliAnginCuaca(WeatherState.Cerah)
                && MesinDanKincirHidup.DenyutSuar01(0.25f, 1f, 0f) > 0.9f);

            var roh = Object.FindFirstObjectByType<RohKeramatPenjaga>(FindObjectsInactive.Include);
            cek("Dunia Hidup V7: RohKeramatPenjaga (>= 38 entitas: 24 roh orbit, 8 kelelawar malam & 6 rune menhir)",
                roh != null && roh.JumlahEntitasMistis >= 38
                && RohKeramatPenjaga.KekuatanMistisMalam(22f) == 1f
                && RohKeramatPenjaga.KekuatanMistisMalam(12f) == 0f
                && GameObject.Find("DuniaHidupV7") != null
                && GameObject.Find("DwarapalaKiri") != null
                && GameObject.Find("KincirAirPerkebunan") != null
                && GameObject.Find("ParabolaKampPeneliti") != null);
        }
    }
}
