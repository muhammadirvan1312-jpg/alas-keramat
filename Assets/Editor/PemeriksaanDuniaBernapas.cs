using System;
using AlasKeramat.AI;
using AlasKeramat.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlasKeramat.EditorTools
{
    // Self-test Dunia Bernapas: patroli obor sekte, monyet/kodok + keheningan horor, asap/kain angin, & pertemuan spontan.
    public static class PemeriksaanDuniaBernapas
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            var pat = Object.FindFirstObjectByType<PatroliHidupPulau>(FindObjectsInactive.Include);
            cek("Dunia Bernapas: 3 regu patroli obor sekte bergerak antar-zona",
                PatroliHidupPulau.PosisiPatroli(Vector3.zero, new Vector3(100f, 0f, 0f), 10f, 0f).x > 5f
                && pat != null && pat.JumlahRegu >= 3);

            var mk = Object.FindFirstObjectByType<EkosistemMonyetDanKodok>(FindObjectsInactive.Include);
            cek("Dunia Bernapas: 16 monyet, 12 kodok & efek keheningan horor hantu",
                EkosistemMonyetDanKodok.HarusSenyapHantu(22f, 12f)
                && !EkosistemMonyetDanKodok.HarusSenyapHantu(12f, 12f)
                && !EkosistemMonyetDanKodok.HarusSenyapHantu(22f, 40f)
                && mk != null && mk.Jumlah >= 28);

            var ak = Object.FindFirstObjectByType<AsapDanKainTertiuAngin>(FindObjectsInactive.Include);
            cek("Dunia Bernapas: 10 titik asap mengepul, 18 kain berkibar & pertemuan spontan",
                Mathf.Abs(AsapDanKainTertiuAngin.SudutKibar(1f, 0f, 1f)) > 2f
                && ak != null && ak.JumlahAsap >= 10 && ak.JumlahKain >= 18
                && !string.IsNullOrEmpty(PertemuanDinamisRimba.PesanKejadian(0, true))
                && Object.FindFirstObjectByType<PertemuanDinamisRimba>(FindObjectsInactive.Include) != null);
        }
    }
}
