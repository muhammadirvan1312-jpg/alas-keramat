using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.UI;
using AlasKeramat.World;
using UnityEngine;

namespace AlasKeramat.EditorTools
{
    // Pemeriksaan Self-Test Mega Overhaul V2 (Visual, Dunia & Gameplay):
    // memverifikasi shader langit badai/awan 2 lapis, God-Rays & Kabut Lembah, Kunang-Kunang,
    // 4 Glider Bambu, 6 Spot Pancing, 8 Peti Parasut, 5 Kuali Jamu, 6 Kentongan, 16 Tungku Api & Jurnal 22 Lokasi.
    public static class PemeriksaanMegaOverhaul
    {
        public static void Jalankan(System.Action<string, bool> cek)
        {
            var sky = RenderSettings.skybox;
            bool skyOk = sky != null && sky.HasProperty("_StormBlend") && sky.HasProperty("_CloudCover") && sky.HasProperty("_Lightning");
            cek("Shader Langit mendukung Badai/Awan 2 Lapis/Kilat", skyOk);

            var berkas = Object.FindFirstObjectByType<BerkasCahayaRimba>(FindObjectsInactive.Include);
            cek("BerkasCahayaRimba & Kabut Lembah terpasang", berkas != null && berkas.JumlahBerkas >= 18 && berkas.JumlahKabut >= 24);
            cek("Intensitas God-Ray pagi > 0 & malam = 0", BerkasCahayaRimba.IntensitasBerkas(8f, WeatherState.Cerah) > 0.5f
                && BerkasCahayaRimba.IntensitasBerkas(22f, WeatherState.Cerah) == 0f);

            var kunang = Object.FindFirstObjectByType<KunangDanDaunGugur>(FindObjectsInactive.Include);
            cek("Kunang-Kunang (32) & Daun Gugur (24) aktif", kunang != null && kunang.JumlahKunang >= 32 && kunang.JumlahDaun >= 24);

            var gliders = Object.FindObjectsByType<GliderBambuAndi>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            cek("4 Glider Bambu (Hang Glider SOTF) siap terbang", gliders != null && gliders.Length >= 4);
            Vector3 pTerbang = GliderBambuAndi.LangkahTerbang(Vector3.up * 30f, 0f, 15f, 2f, 1f);
            cek("Fisika Glider Bambu meluncur maju & turun mulus", pTerbang.z > 14f && pTerbang.y < 29f);

            var pancing = Object.FindObjectsByType<PancingBambuAktif>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            cek("6 Spot Memancing Aktif & tabel tangkapan valid", pancing != null && pancing.Length >= 6
                && PancingBambuAktif.PilihTangkapan(0.2f) == ItemID.IkanMentah
                && PancingBambuAktif.PilihTangkapan(0.95f) == ItemID.JamuEnergi);

            var peti = Object.FindObjectsByType<PetiSupplyTerkunci>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            cek("8 Peti Supply Parasut Terkunci terpasang", peti != null && peti.Length >= 8);

            var kuali = Object.FindObjectsByType<KualiJamuNusantara>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            cek("5 Kuali Jamu Nusantara terpasang", kuali != null && kuali.Length >= 5);

            var kentongan = Object.FindObjectsByType<KentonganDesaInteraktif>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var tungku = Object.FindObjectsByType<TungkuApiCandi>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            cek("6 Kentongan Interaktif & 16 Tungku Api Kuno", kentongan != null && kentongan.Length >= 6 && tungku != null && tungku.Length >= 16);

            var jurnal = Object.FindFirstObjectByType<JurnalEksplorasiPulau>(FindObjectsInactive.Include);
            var banner = Object.FindFirstObjectByType<BannerEksplorasiDanBahayaHUD>(FindObjectsInactive.Include);
            cek("Jurnal Eksplorasi 22 Lokasi & HUD Banner/Glider/Bahaya", jurnal != null && jurnal.TotalZona >= 22 && banner != null);
        }
    }
}
