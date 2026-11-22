using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.Player;
using AlasKeramat.UI;
using AlasKeramat.World;
using UnityEngine;

namespace AlasKeramat.EditorTools
{
    // Pemeriksaan Self-Test Revisi Terpadu: HUD Boss/Status/Kombo, Boneka Latih Awal, relokasi Pendopo Desa Mati,
    // 15 landmark HUD, dan integrasi loop survival (Musim -> Hidrasi, Reputasi -> Persepsi, Kombo & Level).
    public static class PemeriksaanRevisiTerpadu
    {
        public static void Jalankan(System.Action<string, bool> cek)
        {
            var hud = Object.FindFirstObjectByType<BannerBossDanStatusHUD>(FindObjectsInactive.Include);
            cek("BannerBossDanStatusHUD terpasang (" + BannerBossDanStatusHUD.NamaMusimSingkat(SistemMusimDinamis.FaseMusim.Kemarau) + ")", hud != null);

            float rBoss = BannerBossDanStatusHUD.HitungRasioBoss(210f, BossMiniButoIjo.HPMax);
            cek("Rasio HP Boss akurat (" + rBoss.ToString("0.00") + ")", Mathf.Abs(rBoss - 0.5f) < 0.02f);

            var boneka = Object.FindObjectsByType<BonekaLatihSurvival>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            bool bonekaOk = boneka != null && boneka.Length >= 2;
            cek("BonekaLatihSurvival awal aktif (" + (boneka != null ? boneka.Length : 0) + ")", bonekaOk);

            var pendopo = GameObject.Find("PendopoDesaMati");
            bool bebasTabrak = pendopo != null && Vector2.Distance(new Vector2(pendopo.transform.position.x, pendopo.transform.position.z), new Vector2(540f, 204f)) >= 18f;
            cek("PendopoDesaMati bebas tabrakan DesaMati", bebasTabrak);

            bool loopOk = SistemMusimDinamis.PengaliHidrasiMusim(SistemMusimDinamis.FaseMusim.Kemarau) > 1.1f
                && ReputasiSekte.PengaliJarakDeteksi(80f) > 1.2f
                && SistemKomboDanStagger.PengaliKombo(3, 0.8f) >= 1.4f
                && LevelSurvivalAndi.HitungLevelDariXP(250f) == 3;
            cek("Integrasi 4 loop inti (Musim/Reputasi/Kombo/Level)", loopOk);
        }
    }
}
