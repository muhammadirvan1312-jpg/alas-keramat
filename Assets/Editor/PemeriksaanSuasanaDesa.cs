using System;
using AlasKeramat.AI;
using AlasKeramat.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlasKeramat.EditorTools
{
    // Self-test suasana desa & senja: gagak, kawanan kelelawar Maghrib, laron lampu, sarang laba-laba.
    public static class PemeriksaanSuasanaDesa
    {
        const int MinGagak = 8, MinKawanan = 2, MinSarang = 30, MaksPartikelKawanan = 250;

        public static void Jalankan(Action<string, bool> cek)
        {
            cek("Gagak: berkaok saat terjaga (05:30-19:30), diam malam", GagakDesa.Terjaga(10f) && GagakDesa.Terjaga(17f) && !GagakDesa.Terjaga(22f) && !GagakDesa.Terjaga(3f));
            Vector3 orbit = GagakDesa.PosisiPutar(Vector3.zero, 3.3f, 2);
            cek("Gagak: orbit kabur di atas desa (tinggi >= 13 m, radius <= 18 m)", orbit.y >= GagakDesa.TinggiPutar - 3f && new Vector2(orbit.x, orbit.z).magnitude <= GagakDesa.RadiusPutar * 1.3f);
            cek("Kelelawar senja: hanya keluar saat Maghrib dan memuncak di tengahnya",
                KawananKelelawarSenja.LajuUntuk(12f) == 0f && KawananKelelawarSenja.LajuUntuk(17.9f) == 0f && KawananKelelawarSenja.LajuUntuk(19.5f) == 0f
                && KawananKelelawarSenja.LajuUntuk(18.7f) > KawananKelelawarSenja.LajuUntuk(18.1f) && KawananKelelawarSenja.LajuUntuk(18.1f) > 0f);

            var gd = Object.FindFirstObjectByType<GagakDesa>(FindObjectsInactive.Include);
            bool gagakOk = gd != null && gd.Jumlah >= MinGagak && gd.Tengger != null && gd.Tengger.Length == gd.Jumlah
                && gd.SayapKiri != null && gd.SayapKiri.Length == gd.Jumlah && gd.SayapKanan != null && gd.SayapKanan.Length == gd.Jumlah;
            if (gagakOk) for (int i = 0; i < gd.Jumlah; i++) if (gd.Badan[i] == null || gd.SayapKiri[i] == null || gd.SayapKanan[i] == null) gagakOk = false;
            cek("Gagak: >= " + MinGagak + " gagak bertengger di atap Desa Mati (" + (gd != null ? gd.Jumlah : 0) + ")", gagakOk);

            var kawanan = Object.FindObjectsByType<KawananKelelawarSenja>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            bool kawOk = kawanan.Length >= MinKawanan;
            for (int i = 0; i < kawanan.Length && kawOk; i++)
            {
                var ps = kawanan[i].GetComponent<ParticleSystem>();
                var rd = kawanan[i].GetComponent<ParticleSystemRenderer>();
                kawOk = ps != null && ps.main.maxParticles <= MaksPartikelKawanan && ps.textureSheetAnimation.enabled && rd != null && rd.sharedMaterial != null;
            }
            cek("Kelelawar senja: >= " + MinKawanan + " mulut gua punya kawanan berlembar kepak, partikel <= " + MaksPartikelKawanan + " (" + kawanan.Length + ")", kawOk);

            var lampu = Object.FindObjectsByType<LampuMinyak>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            bool laronOk = true;
            for (int i = 0; i < lampu.Length; i++) if (lampu[i].Laron == null || lampu[i].Laron.main.playOnAwake || lampu[i].Laron.main.maxParticles > 20) laronOk = false;
            cek("Laron: tiap lampu minyak punya laron (mati awal, <= 20 partikel)", laronOk);

            int sarang = 0;
            foreach (var mr in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (mr.gameObject.name.StartsWith("SarangLaba_") && mr.sharedMaterial != null) sarang++;
            cek("Sarang laba-laba: >= " + MinSarang + " di sudut rumah Desa Mati & kampung (" + sarang + ")", sarang >= MinSarang);
        }
    }
}
