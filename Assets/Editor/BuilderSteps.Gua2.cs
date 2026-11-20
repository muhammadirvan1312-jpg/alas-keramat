using AlasKeramat.AI;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using J = AlasKeramat.Core.ResepKarakter.JenisKarakter;
namespace AlasKeramat.EditorTools
{
    // Langkah 7t lanjutan: UG2 Bunker Kolonial & Lab PT, UG3 Kuil Akar Hitam (arena boss), gerbang permukaan (gali, palka, mulut gua, zipline).
    public static partial class NusantaraMasterBuilder
    {
        public static readonly Vector3 PosGaliBunker = new Vector3(-560f, 0f, 290f), PosTanggaDarurat = new Vector3(-540f, 0f, 300f);
        public static readonly Vector3 PosZiplineA = new Vector3(0f, 0f, 620f), PosZiplineB = new Vector3(0f, 0f, 676f), PosKeluarBeringin = new Vector3(-24f, 0f, 24f);
        public static readonly Vector3 ArenaUG3 = new Vector3(0f, 0f, 20f);

        // ---------- UG2 Bunker ----------
        static GameObject BangunUG2(GameObject induk)
        {
            var beton = MatPolos("M_BetonBunker", "#5F5B53");
            var k = MulaiKompleks(induk, UndergroundId.UG2_Bunker, beton, MatPolos("M_LantaiBunker", "#45423C"));
            var e0 = R("E0_RuangTangga", new Vector3(0f, 0f, -55f), new Vector3(6f, 4f, 6f), false);
            var h = R("H_Aula", new Vector3(0f, 0f, -10f), new Vector3(8f, 3.2f, 16f), false);
            var r1 = R("R1_ArsipBelanda", new Vector3(-14f, 0f, -8f), new Vector3(12f, 3.2f, 10f), false);
            var r2 = R("R2_LabPT", new Vector3(14f, 0f, -8f), new Vector3(14f, 3.2f, 12f), false);
            var j = R("J_Simpang", new Vector3(0f, 0f, 30f), new Vector3(6f, 3.2f, 8f), false);
            var r3 = R("R3_Gudang", new Vector3(-12f, 0f, 30f), new Vector3(10f, 3.2f, 10f), false);
            var r4 = R("R4_RuangSegel", new Vector3(12f, 0f, 34f), new Vector3(16f, 6f, 16f), false);
            Sambung(e0, h, 3f, 3f);
            Sambung(h, r1, 2.2f, 2.6f);
            Sambung(h, r2, 2.2f, 2.6f);
            Sambung(h, j, 3f, 3f);
            Sambung(j, r3, 2.2f, 2.6f);
            Sambung(j, r4, 2.2f, 2.6f);
            BangunKerangka();
            var logam = AmbilMat("M_LogamGelap"); var kayu = AmbilMat("M_Kayu"); var merah = AmbilMat("M_CatMerahPudar");
            var hias = Anak(k, "Hiasan");
            Color neon = new Color(0.85f, 0.92f, 1f), darurat = new Color(1f, 0.15f, 0.1f);
            for (int i = 0; i < 5; i++) Lampu(hias, "Neon_C1_" + i, new Vector3(0f, 2.6f, -48f + i * 6f), neon, 8f, 1.1f);
            Lampu(hias, "Neon_H", h.pos + Vector3.up * 2.6f, neon, 10f, 1.2f);
            Lampu(hias, "Lampu_R1", r1.pos + Vector3.up * 2.6f, new Color(1f, 0.8f, 0.55f), 9f, 1f);
            Lampu(hias, "Lampu_R2", r2.pos + Vector3.up * 2.6f, new Color(0.6f, 1f, 0.7f), 10f, 1f);
            Lampu(hias, "Darurat_C2a", new Vector3(0f, 2.6f, 8f), darurat, 8f, 1.2f);
            Lampu(hias, "Darurat_C2b", new Vector3(0f, 2.6f, 20f), darurat, 8f, 1.2f);
            Lampu(hias, "Lampu_R3", r3.pos + Vector3.up * 2.6f, neon, 9f, 1f);
            Lampu(hias, "Lampu_R4", r4.pos + Vector3.up * 5f, new Color(0.5f, 0.5f, 0.6f), 14f, 1.2f);
            var alarm = Anak(k, "LampuAlarm");
            Lampu(alarm, "Alarm_1", r4.pos + new Vector3(-5f, 5f, 0f), darurat, 16f, 3f);
            Lampu(alarm, "Alarm_2", r4.pos + new Vector3(5f, 5f, 0f), darurat, 16f, 3f);
            alarm.SetActive(false);
            var runtuh = Anak(k, "DindingRuntuh");
            for (int i = 0; i < 6; i++) Hias(runtuh, PrimitiveType.Cube, "Puing_" + i, r4.pos + new Vector3(-6f + i * 2.4f, 0.5f, 6.5f), new Vector3(1.6f, 1f + (i % 3) * 0.4f, 1.2f), beton, true);
            runtuh.SetActive(false);

            // D1 pintu baja bersandi di ujung koridor C1.
            var d1 = Anak(k, "D1_PintuBaja");
            d1.transform.localPosition = new Vector3(0f, 0f, -21f);
            var daun = Balok(d1, "Daun", new Vector3(0f, 1.5f, 0f), Quaternion.identity, new Vector3(3.4f, 3.2f, 0.25f), logam, false);
            var pembaca = Balok(d1, "PembacaKartu", new Vector3(1.4f, 1.3f, -0.6f), Quaternion.identity, new Vector3(0.25f, 0.35f, 0.12f), logam, true);
            var led = Lampu(pembaca, "LED", new Vector3(0f, 0.6f, -0.6f), Color.red, 2.5f, 1.5f);
            var pk = pembaca.AddComponent<PintuKartu>(); pk.Pasang(daun, led); EditorUtility.SetDirty(pk);

            for (int i = 0; i < 3; i++) Hias(hias, PrimitiveType.Cube, "RakArsip_" + i, r1.pos + new Vector3(-4f + i * 3f, 1.1f, 4f), new Vector3(2.2f, 2.2f, 0.6f), kayu, true);
            Wadah(k, "Arsip_Lore5", "Arsip Belanda 1928", r1.pos + new Vector3(0f, 0.8f, 0f), new Vector3(0.6f, 0.08f, 0.45f), MatPolos("M_KertasTua", "#CDBF98"), new ItemID[0], new int[0], (int)LoreID.L05, false);
            Hias(hias, PrimitiveType.Cube, "MejaArsip", r1.pos + new Vector3(0f, 0.4f, 0f), new Vector3(2f, 0.8f, 1f), kayu, true);
            Hias(hias, PrimitiveType.Cube, "MejaOperasi", r2.pos + new Vector3(-3f, 0.45f, 0f), new Vector3(2.2f, 0.9f, 1f), logam, true);
            var cairan = MatEmisi("M_CairanHitam", "#05080A", new Color(0.05f, 0.25f, 0.18f), false);
            for (int i = 0; i < 4; i++) Hias(hias, PrimitiveType.Cylinder, "Tabung_" + i, r2.pos + new Vector3(4.5f, 1f, -4f + i * 2.6f), new Vector3(0.8f, 1f, 0.8f), cairan, true);
            Wadah(k, "MejaLab", "Meja Lab", r2.pos + new Vector3(0f, 0f, 4f), new Vector3(1.6f, 0.9f, 0.8f), logam,
                new[] { ItemID.SenterTaktis, ItemID.Baterai }, new[] { 1, 3 }, (int)LoreID.L09, false);
            Wadah(k, "RakPeralatan", "Rak Peralatan", r3.pos + new Vector3(0f, 0f, 3f), new Vector3(2.2f, 1.2f, 0.7f), logam,
                new[] { ItemID.ZiplineGun, ItemID.TaliZipline, ItemID.LogamBekas, ItemID.ZirahBaja, ItemID.KapakBaja }, new[] { 1, 6, 4, 2, 1 }, 0, false);
            Hias(hias, PrimitiveType.Cylinder, "TabungSegel", r4.pos + new Vector3(0f, 1.2f, 0f), new Vector3(1.2f, 1.2f, 1.2f), MatEmisi("M_KacaSegel", "#2A3038A0", new Color(0.05f, 0.05f, 0.08f), true), false);
            Wadah(k, "Paku_2", "Paku Keramat", r4.pos + new Vector3(0f, 0.9f, 0f), new Vector3(0.12f, 0.5f, 0.12f), AmbilMatGen("M_PakuKeramat"), new[] { ItemID.PakuKeramat2 }, new[] { 1 }, 0, true);

            Pintu(k, "Pintu_Tangga", e0.pos + new Vector3(0f, 0f, -1.5f), "NAIK KE PERMUKAAN", Permukaan(PosGaliBunker.x + 3f, PosGaliBunker.z, 0.3f), 0f, 0, false, "");
            Pintu(k, "Pintu_TanggaDarurat", r4.pos + new Vector3(6f, 0f, 5f), "TANGGA DARURAT", Permukaan(PosTanggaDarurat.x, PosTanggaDarurat.z, 0.3f), 0f, 0, false, "");
            return k;
        }

        static Material AmbilMatGen(string nama) { var m = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/" + nama + ".mat"); return m != null ? m : AmbilMat("M_CatOranye"); }

        // ---------- UG3 Kuil Akar Beringin Hitam ----------
        static GameObject BangunUG3(GameObject induk)
        {
            var akarMat = SimpanMat(SafeMaterialFactory.CreatePBR("M_AkarHitam", ProcTex.Hex("#7E604C"), 0f, 0.28f, Tex("kulit_kayu"), Tex("kulit_kayu_n")));
            MaskPBR(akarMat, "kulit_kayu"); SafeMaterialFactory.SetF(akarMat, "_Smoothness", 0.28f);
            var batuArena = SimpanMat(SafeMaterialFactory.CreatePBR("M_BatuArena", ProcTex.Hex("#665A56"), 0f, 0.36f, Tex("batu"), Tex("batu_n")));
            MaskPBR(batuArena, "batu"); SafeMaterialFactory.SetF(batuArena, "_Smoothness", 0.36f);
            _matUrat = MatEmisi("M_UratMerah", "#4A0A06", new Color(1.4f, 0.16f, 0.05f) * 2.2f, false);
            var k = MulaiKompleks(induk, UndergroundId.UG3_KuilAkar, akarMat, batuArena);
            var k0 = R("K0_CelahAkar", new Vector3(0f, 0f, -70f), new Vector3(6f, 5f, 6f), true);
            var k2 = R("K2_RuangSesaji", new Vector3(-18f, 0f, -15f), new Vector3(12f, 5f, 10f), true);
            var k3 = R("K3_KubahAkar", ArenaUG3, new Vector3(40f, 22f, 40f), true);
            var k4 = R("K4_RuangEmas", new Vector3(30f, 0f, 40f), new Vector3(8f, 4f, 8f), true);
            var k5 = R("K5_JalanKeluar", new Vector3(0f, 0f, 62f), new Vector3(6f, 5f, 6f), true);
            Sambung(k0, k3, 4f, 5f, new Vector3(0f, 0f, -25f));
            Sambung(k2, k3, 3f, 4f);
            Sambung(k3, k4, 3f, 3.5f);
            Sambung(k3, k5, 4f, 4.5f);
            BangunKerangka();
            var hias = Anak(k, "Hiasan");
            // Urat merah berdenyut di Lorong Urat.
            for (int i = 0; i < 10; i++) Hias(hias, PrimitiveType.Cube, "Urat_" + i, new Vector3(i % 2 == 0 ? -1.85f : 1.85f, 1.2f + (i % 3) * 1.1f, -64f + i * 3.6f), new Vector3(0.1f, 0.12f, 2.6f), _matUrat, false);
            var angin = Anak(k, "LorongAngin");
            angin.transform.localPosition = new Vector3(0f, 0f, -42.5f);
            var za = angin.AddComponent<ZonaAngin>(); za.Pasang(new Vector3(4.5f, 5f, 15f)); EditorUtility.SetDirty(za);
            Color merah = new Color(1f, 0.25f, 0.12f);
            Lampu(hias, "Lampu_K0", k0.pos + Vector3.up * 3f, merah, 13f, 1.8f);
            Lampu(hias, "Lampu_K1", new Vector3(0f, 3f, -28f), merah, 13f, 1.8f);
            Lampu(hias, "Lampu_K2", k2.pos + Vector3.up * 3f, new Color(1f, 0.85f, 0.4f), 13f, 2.0f);
            Lampu(hias, "Lampu_K4", k4.pos + Vector3.up * 2.5f, new Color(1f, 0.8f, 0.3f), 12f, 2.2f);
            Wadah(k, "Wasiat_Lore6", "Wasiat Juru Kunci", k2.pos + new Vector3(0f, 0f, 2.5f), new Vector3(0.5f, 0.1f, 0.4f), MatPolos("M_KainKuning", "#C9A227"), new ItemID[0], new int[0], (int)LoreID.L06, false);

            // K4 di balik akar yang harus dibakar.
            var lorK4 = _lorong[2].titik;
            Vector3 posAkar = (lorK4[0] + lorK4[1]) * 0.5f;
            var dinding = Anak(k, "Akar_K4");
            dinding.transform.localPosition = posAkar;
            dinding.transform.localRotation = Quaternion.LookRotation(lorK4[1] - lorK4[0], Vector3.up);
            dinding.layer = GameLayers.Structure;
            var bc = dinding.AddComponent<BoxCollider>(); bc.size = new Vector3(3.2f, 3.6f, 1f); bc.center = Vector3.up * 1.8f;
            var akMesh = SimpanMeshGua(ProcGua.DindingAkar("DindingAkar_K4", 3.2f, 3.6f, 404));
            dinding.AddComponent<MeshFilter>().sharedMesh = akMesh;
            dinding.AddComponent<MeshRenderer>().sharedMaterial = akarMat;
            dinding.AddComponent<AkarBakar>();
            Wadah(k, "PetiEmas", "Peti Emas Majapahit", k4.pos + new Vector3(0f, 0f, 1.5f), new Vector3(1f, 0.6f, 0.7f), MatEmisi("M_EmasKuno", "#B8922E", new Color(0.3f, 0.22f, 0.05f), false),
                new[] { ItemID.KerisPusaka, ItemID.ZirahEmas }, new[] { 1, 1 }, 0, false);

            BangunArena(k, k3, akarMat);
            Pintu(k, "Pintu_Keluar", k0.pos + new Vector3(0f, 0f, -1.5f), "KELUAR GUA", Permukaan(0f, 678f, 0.3f), 180f, 0, false, "");
            Pintu(k, "Pintu_Beringin", k5.pos + new Vector3(0f, 0f, 1.5f), "KELUAR KE BERINGIN", Permukaan(PosKeluarBeringin.x, PosKeluarBeringin.z, 0.3f), 225f, 0, true, "Akar hitam menutup jalan. Kalahkan penjaganya dulu.");
            return k;
        }
    }
}
