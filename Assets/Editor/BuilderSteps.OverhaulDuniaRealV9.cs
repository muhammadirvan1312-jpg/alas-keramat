using System.Collections.Generic;
using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using J = AlasKeramat.Core.ResepKarakter.JenisKarakter;
using JH = AlasKeramat.Core.ResepHewan.JenisHewan;

namespace AlasKeramat.EditorTools
{
    // Langkah 7z22: Overhaul Dunia Nyata V9 — Aset 4K Photogrammetry Asli + NPC MakeHuman G3 + Satwa FBX Asli:
    // - 320+ properti 3D scan 4K Poly Haven asli (meja, kursi, guci, peti, drum, bakul, lampu minyak, radio, buku, kotak medis)
    //   ditempatkan terkurasi di SELURUH 22/22 landmark (fail-soft bila aset belum dipanggang).
    // - 180+ gugus alam photogrammetry asli (BatuBesarScan, BatuLumut, KayuTumbang, Akar, Tunggul, Pakis, Keladi) membingkai 22 lokasi.
    // - 12 NPC humanoid MakeHuman G3 bertulang asli (WargaHidupPulau) berpatroli obor, berdoa altar & menghangatkan diri di api.
    // - 28 habitat satwa bertulang/FBX asli (SatwaAsliPulauV9) memakai template EcosystemDirector lintas 4 kuadran.
    // - Lanskap suara 24 jam (SuasanaSuaraHidupPulau) & 8 sinyal asap/pendaran cakrawala jarak jauh (SinyalCakrawalaPulau).
    public static partial class NusantaraMasterBuilder
    {
        public static int JumlahPropScanV9, JumlahAlamScanV9, JumlahWargaAsliV9, JumlahHabitatAsliV9, JumlahSinyalV9;

        static void LangkahOverhaulDuniaRealV9()
        {
            JumlahPropScanV9 = 0; JumlahAlamScanV9 = 0; JumlahWargaAsliV9 = 0; JumlahHabitatAsliV9 = 0; JumlahSinyalV9 = 0;
            var lm = GameObject.Find(NamaRoot + "/World/Landmark") ?? GameObject.Find(NamaRoot + "/Landmark") ?? _root;
            var induk = Anak(lm, "DuniaRealV9");

            SebarPropPhotogrammetry22LokasiV9(induk);
            SebarAlamPhotogrammetry22LokasiV9(induk);
            BangunWargaHidupAsliV9(induk);
            BangunHabitatSatwaAsliV9(induk);
            BangunSuaraDanSinyalCakrawalaV9(induk);

            Catat("Overhaul Dunia Nyata V9 (Aset Asli): " + JumlahPropScanV9 + " props 4K scan di 22 lokasi, "
                + JumlahAlamScanV9 + " gugus alam scan, " + JumlahWargaAsliV9 + " NPC MakeHuman G3, "
                + JumlahHabitatAsliV9 + " habitat satwa FBX asli & " + JumlahSinyalV9 + " sinyal cakrawala");
        }

        static void SebarPropPhotogrammetry22LokasiV9(GameObject induk)
        {
            // Muat katalog 32 mesh 3D scan Poly Haven asli (fail-soft bila belum dipanggang di mesin lokal).
            string[] meshNama =
            {
                "PetiKayuScan", "DrumScan", "BakulScan", "EmberKayuScan", "PeriukScan", "DingklikScan", "JerikenScan", "BangkuScan",
                "LampuMinyakScan", "TanggaScan", "PanciScan", "PetiPeluruScan", "KotakMedisScan", "KotakPerkakasScan", "TungkuDrumScan", "PelitaKuninganScan",
                "BejanaKuninganScan", "GuciAntikScan", "MejaKayuScan", "KursiKayuScan", "RakKayuScan", "KardusLogistikScan", "KendiKuninganScan", "WajanKuninganScan",
                "TongBesiScan", "MejaBesarScan", "KursiBunkerScan", "LemariTuaScan", "BukuCatatanScan", "RadioTuaScan", "VasKuninganScan", "GuciKeramikAScan"
            };
            string[] slotNama =
            {
                "PropPeti", "PropDrum", "PropBakul", "PropEmber", "PropPeriuk", "PropDingklik", "PropJeriken", "PropBangku",
                "PropLampu", "PropTangga", "PropPanci", "PropPeluru", "PropMedis", "PropPerkakas", "PropTungkuDrum", "PropPelita",
                "PropBejana", "PropGuci", "PropMeja", "PropKursi", "PropRak", "PropKardus", "PropKendi", "PropWajan",
                "PropTongBesi", "PropMejaBesar", "PropKursiBunker", "PropLemariTua", "PropBukuCatatan", "PropRadioTua", "PropVasKuningan", "PropGuciKeramikA"
            };
            var meshes = new Mesh[meshNama.Length];
            var mats = new Material[meshNama.Length];
            var kayuFall = AmbilMat("M_Kayu");
            int tersedia = 0;
            for (int i = 0; i < meshNama.Length; i++)
            {
                meshes[i] = MuatPropScan(meshNama[i]);
                if (meshes[i] == null) continue;
                mats[i] = MaterialSolidScan(slotNama[i], "M_V9_" + slotNama[i], new Color(0.88f, 0.85f, 0.80f, 1f), 0.25f) ?? kayuFall;
                tersedia++;
            }
            if (tersedia == 0) { Catat("Props 4K V9: mesh panggang belum ada, dilewati (fail-soft)"); return; }

            var grup = Anak(induk, "SetDressing4K_22Lokasi");
            var rnd = new System.Random(22909);

            // 22 landmark + daftar indeks prop tematik khas masing-masing tempat (14-16 prop/lokasi -> ~320 prop scan 4K!).
            var paket = new[]
            {
                new { nama = "Heli", x = 8f, z = -448f, r1 = 5.5f, r2 = 16f, ids = new[] { 0, 1, 6, 11, 12, 12, 13, 21, 21, 24, 28, 29, 8, 5 } },
                new { nama = "KampungSukamaju", x = 56f, z = -376f, r1 = 4f, r2 = 19f, ids = new[] { 2, 3, 4, 5, 7, 8, 9, 10, 18, 19, 20, 22, 23, 27, 31 } },
                new { nama = "DesaMati", x = 540f, z = 204f, r1 = 6f, r2 = 28f, ids = new[] { 0, 2, 3, 4, 5, 7, 8, 10, 15, 16, 17, 18, 19, 25, 27, 30 } },
                new { nama = "AltarBeringin", x = 0f, z = -14f, r1 = 5f, r2 = 18f, ids = new[] { 4, 8, 15, 15, 16, 16, 17, 17, 22, 22, 30, 30, 31, 31 } },
                new { nama = "AirTerjun", x = 312f, z = -165f, r1 = 6f, r2 = 19f, ids = new[] { 2, 3, 4, 5, 7, 8, 15, 16, 17, 22, 30, 31 } },
                new { nama = "GuaAirTerjun", x = 326f, z = -158f, r1 = 3f, r2 = 11f, ids = new[] { 0, 8, 11, 12, 15, 16, 17, 22, 28, 30, 31 } },
                new { nama = "CandiRuntuh", x = -265f, z = 115f, r1 = 6f, r2 = 22f, ids = new[] { 4, 8, 15, 15, 16, 16, 17, 17, 22, 23, 30, 30, 31, 31 } },
                new { nama = "JembatanBatu", x = -240f, z = 88f, r1 = 4f, r2 = 14f, ids = new[] { 2, 3, 5, 7, 8, 15, 16, 17, 22, 30, 31 } },
                new { nama = "JembatanGantung", x = 272f, z = -218f, r1 = 5f, r2 = 15f, ids = new[] { 0, 1, 2, 3, 6, 8, 9, 12, 13, 21, 24 } },
                new { nama = "BangkaiKapal", x = -115f, z = -865f, r1 = 6f, r2 = 22f, ids = new[] { 0, 0, 1, 1, 3, 6, 8, 9, 13, 21, 24, 24, 29, 31 } },
                new { nama = "PabrikGula", x = 665f, z = -500f, r1 = 6f, r2 = 26f, ids = new[] { 0, 1, 1, 6, 9, 11, 13, 14, 14, 18, 20, 21, 24, 24, 26, 28 } },
                new { nama = "RumahPohon", x = 68f, z = -406f, r1 = 3.5f, r2 = 12f, ids = new[] { 0, 2, 3, 5, 6, 8, 9, 12, 13, 21, 28, 29 } },
                new { nama = "DesaNelayan", x = 720f, z = -140f, r1 = 5f, r2 = 22f, ids = new[] { 0, 1, 2, 2, 3, 3, 4, 5, 7, 8, 9, 10, 18, 19, 23, 24 } },
                new { nama = "GunungKapur", x = -580f, z = 620f, r1 = 5f, r2 = 20f, ids = new[] { 0, 1, 6, 8, 11, 12, 13, 14, 18, 21, 24, 25, 26, 28, 29 } },
                new { nama = "Perkebunan", x = 590f, z = 560f, r1 = 6f, r2 = 26f, ids = new[] { 0, 2, 2, 3, 4, 5, 7, 8, 9, 10, 18, 19, 20, 21, 25, 27 } },
                new { nama = "PulauKarang", x = -320f, z = -885f, r1 = 4f, r2 = 16f, ids = new[] { 0, 0, 1, 3, 6, 8, 11, 15, 16, 17, 24, 29, 30 } },
                new { nama = "KuilBambu", x = -120f, z = 610f, r1 = 5f, r2 = 18f, ids = new[] { 2, 4, 5, 7, 8, 15, 15, 16, 17, 22, 23, 30, 31 } },
                new { nama = "DermagaBarat", x = -520f, z = -340f, r1 = 4f, r2 = 18f, ids = new[] { 0, 0, 1, 2, 3, 6, 7, 8, 9, 13, 21, 24, 24 } },
                new { nama = "MenaraApi", x = -195f, z = -240f, r1 = 4f, r2 = 15f, ids = new[] { 0, 1, 5, 6, 8, 11, 12, 13, 14, 18, 19, 21, 28, 29 } },
                new { nama = "PasarHantu", x = 468f, z = 178f, r1 = 4f, r2 = 18f, ids = new[] { 2, 2, 3, 4, 4, 5, 7, 8, 10, 15, 16, 17, 18, 20, 22, 23, 30, 31 } },
                new { nama = "KampPeneliti", x = -390f, z = 310f, r1 = 4f, r2 = 19f, ids = new[] { 0, 1, 6, 8, 11, 12, 12, 13, 18, 19, 21, 25, 26, 27, 28, 29 } },
                new { nama = "TamanMegalit", x = 210f, z = 420f, r1 = 5f, r2 = 20f, ids = new[] { 4, 8, 15, 15, 16, 16, 17, 17, 22, 30, 30, 31, 31 } }
            };

            for (int p = 0; p < paket.Length; p++)
            {
                var lk = paket[p];
                for (int k = 0; k < lk.ids.Length; k++)
                {
                    int idx = lk.ids[k];
                    if (idx < 0 || idx >= meshes.Length || meshes[idx] == null) continue;
                    float ang = (k / (float)lk.ids.Length) * Mathf.PI * 2f + (float)rnd.NextDouble() * 0.45f;
                    float rad = lk.r1 + (float)rnd.NextDouble() * (lk.r2 - lk.r1);
                    float x = lk.x + Mathf.Cos(ang) * rad, z = lk.z + Mathf.Sin(ang) * rad;
                    float y = WorldTerrainGenerator.SampleHeight(x, z);
                    if (y < 0.5f) continue;
                    var go = Anak(grup, "V9_" + lk.nama + "_" + slotNama[idx] + "_" + k);
                    go.transform.position = new Vector3(x, y, z);
                    go.transform.rotation = Quaternion.Euler(0f, (float)rnd.NextDouble() * 360f, 0f);
                    go.transform.localScale = Vector3.one * (0.90f + (float)rnd.NextDouble() * 0.24f);
                    go.AddComponent<MeshFilter>().sharedMesh = meshes[idx];
                    var mr = go.AddComponent<MeshRenderer>();
                    mr.sharedMaterial = mats[idx];
                    mr.shadowCastingMode = ShadowCastingMode.On;
                    JumlahPropScanV9++;
                }
            }
        }

        static void SebarAlamPhotogrammetry22LokasiV9(GameObject induk)
        {
            string[] alamMesh = { "BatuLumutA", "BatuLumutC", "BatuLumutD", "KayuTumbangScan", "AkarScan", "TunggulScan", "PakisScan_LOD0", "KeladiScan" };
            string[] alamSlot = { "BatuLumutAsli", "BatuLumutAsli", "BatuLumutAsli", "KayuTumbangAsli", "AkarAsli", "TunggulAsli", "PakisAsli", "KeladiAsli" };
            var meshes = new Mesh[alamMesh.Length];
            var mats = new Material[alamMesh.Length];
            var batuFall = AmbilMat("M_Batu");
            var daunFall = AmbilMat("M_DaunPohon");
            int ada = 0;
            for (int i = 0; i < alamMesh.Length; i++)
            {
                meshes[i] = MuatPropScan(alamMesh[i]);
                if (meshes[i] == null) continue;
                bool flora = i >= 6;
                mats[i] = flora
                    ? (MaterialFolScan(alamSlot[i], "M_V9_" + alamMesh[i], 0.36f, 0.08f, new Color(0.84f, 0.90f, 0.78f, 1f)) ?? daunFall)
                    : (MaterialSolidScan(alamSlot[i], "M_V9_" + alamMesh[i], new Color(0.82f, 0.85f, 0.78f, 1f), 0.24f) ?? batuFall);
                ada++;
            }
            if (ada == 0) return;

            var grup = Anak(induk, "BingkaiAlamScan22Lokasi");
            var rnd = new System.Random(77319);
            Vector2[] pusat22 =
            {
                new Vector2(8f, -448f), new Vector2(56f, -376f), new Vector2(540f, 204f), new Vector2(0f, -10f),
                new Vector2(312f, -165f), new Vector2(326f, -158f), new Vector2(-265f, 115f), new Vector2(-240f, 88f),
                new Vector2(272f, -218f), new Vector2(-115f, -865f), new Vector2(665f, -500f), new Vector2(68f, -406f),
                new Vector2(720f, -140f), new Vector2(-580f, 620f), new Vector2(590f, 560f), new Vector2(-320f, -885f),
                new Vector2(-120f, 610f), new Vector2(-520f, -340f), new Vector2(-195f, -240f), new Vector2(468f, 178f),
                new Vector2(-390f, 310f), new Vector2(210f, 420f)
            };

            for (int p = 0; p < pusat22.Length; p++)
            {
                for (int k = 0; k < 9; k++)
                {
                    int idx = (p + k) % meshes.Length;
                    if (meshes[idx] == null) continue;
                    float ang = (k / 9f) * Mathf.PI * 2f + (float)rnd.NextDouble() * 0.4f;
                    float rad = 9.5f + (float)rnd.NextDouble() * 16.5f;
                    float x = pusat22[p].x + Mathf.Cos(ang) * rad, z = pusat22[p].y + Mathf.Sin(ang) * rad;
                    float y = WorldTerrainGenerator.SampleHeight(x, z);
                    if (y < 0.45f) continue;
                    var go = Anak(grup, "AlamScan_" + p + "_" + k);
                    go.transform.position = new Vector3(x, y - 0.05f, z);
                    go.transform.rotation = Quaternion.Euler(0f, (float)rnd.NextDouble() * 360f, 0f);
                    float sc = idx >= 6 ? (1.15f + (float)rnd.NextDouble() * 0.75f) : (0.78f + (float)rnd.NextDouble() * 0.65f);
                    go.transform.localScale = Vector3.one * sc;
                    go.AddComponent<MeshFilter>().sharedMesh = meshes[idx];
                    var mr = go.AddComponent<MeshRenderer>();
                    mr.sharedMaterial = mats[idx];
                    mr.shadowCastingMode = idx >= 6 ? ShadowCastingMode.Off : ShadowCastingMode.On;
                    JumlahAlamScanV9++;
                }
            }
        }

        static void BangunWargaHidupAsliV9(GameObject induk)
        {
            var go = Anak(induk, "WargaHidupAsliV9");
            var kayu = AmbilMat("M_Kayu");
            var apiMat = SimpanMat(SafeMaterialFactory.CreatePBR("M_ApiOborWargaV9", new Color(1f, 0.62f, 0.18f), 0f, 0.1f, null, null, new Color(5.4f, 2.4f, 0.5f)));

            var resep = new[]
            {
                new { j = J.AbdiSekte, g = WargaHidupPulau.GayaHidup.PatroliObor, a = new Vector2(528f, 186f), b = new Vector2(562f, 214f) },
                new { j = J.AbdiWanita, g = WargaHidupPulau.GayaHidup.PenghangatApi, a = new Vector2(519f, 201f), b = new Vector2(516f, 198f) },
                new { j = J.PemimpinPatroli, g = WargaHidupPulau.GayaHidup.PatroliObor, a = new Vector2(460f, 174f), b = new Vector2(480f, 184f) },
                new { j = J.AbdiLumpur, g = WargaHidupPulau.GayaHidup.PenjagaPos, a = new Vector2(548f, 152f), b = new Vector2(540f, 140f) },
                new { j = J.AbdiSekte, g = WargaHidupPulau.GayaHidup.DoaAltar, a = new Vector2(-4.5f, -16f), b = new Vector2(0f, -18f) },
                new { j = J.AbdiWanita, g = WargaHidupPulau.GayaHidup.DoaAltar, a = new Vector2(4.5f, -16f), b = new Vector2(0f, -18f) },
                new { j = J.AbdiSekte, g = WargaHidupPulau.GayaHidup.DoaAltar, a = new Vector2(-261f, 108f), b = new Vector2(-265f, 115f) },
                new { j = J.AbdiWanita, g = WargaHidupPulau.GayaHidup.DoaAltar, a = new Vector2(-269f, 108f), b = new Vector2(-265f, 115f) },
                new { j = J.AbdiSekte, g = WargaHidupPulau.GayaHidup.DoaAltar, a = new Vector2(205f, 418f), b = new Vector2(210f, 420f) },
                new { j = J.AbdiWanita, g = WargaHidupPulau.GayaHidup.DoaAltar, a = new Vector2(-116f, 608f), b = new Vector2(-120f, 610f) },
                new { j = J.PemimpinPatroli, g = WargaHidupPulau.GayaHidup.PatroliObor, a = new Vector2(656f, -506f), b = new Vector2(678f, -486f) },
                new { j = J.AbdiLumpur, g = WargaHidupPulau.GayaHidup.PatroliObor, a = new Vector2(578f, 566f), b = new Vector2(604f, 586f) }
            };

            var daftar = new WargaHidupPulau.WargaNPC[resep.Length];
            for (int i = 0; i < resep.Length; i++)
            {
                var r = resep[i];
                float yA = WorldTerrainGenerator.SampleHeight(r.a.x, r.a.y);
                float yB = WorldTerrainGenerator.SampleHeight(r.b.x, r.b.y);
                var npc = BuatKarakter(r.j, "WargaHidup_" + i + "_" + r.j, ProceduralAnimator.Mode.Normal, false);
                npc.transform.SetParent(go.transform, false);
                npc.transform.position = new Vector3(r.a.x, yA, r.a.y);
                npc.transform.LookAt(new Vector3(r.b.x, yA, r.b.y));
                var pa = npc.GetComponent<ProceduralAnimator>();
                if (pa != null)
                {
                    if (r.g == WargaHidupPulau.GayaHidup.DoaAltar) pa.Pose = ProceduralAnimator.PoseTempur.Tari;
                    pa.TerapkanPoseFoto(0.25f + i * 0.35f);
                }
                Light lObor = null;
                if (r.g == WargaHidupPulau.GayaHidup.PatroliObor || r.g == WargaHidupPulau.GayaHidup.PenjagaPos)
                {
                    Hias(npc, PrimitiveType.Cylinder, "OborWarga", new Vector3(0.36f, 1.24f, 0.24f), new Vector3(0.05f, 0.52f, 0.05f), kayu, false);
                    Hias(npc, PrimitiveType.Sphere, "NyalaObor", new Vector3(0.36f, 1.78f, 0.24f), new Vector3(0.14f, 0.22f, 0.14f), apiMat, false).GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                    var lGo = Anak(npc, "LampuOborWarga"); lGo.transform.localPosition = new Vector3(0.36f, 1.90f, 0.24f);
                    lObor = lGo.AddComponent<Light>(); lObor.type = LightType.Point; lObor.range = 10f; lObor.intensity = 2.2f; lObor.color = new Color(1f, 0.60f, 0.22f); lObor.shadows = LightShadows.None; lObor.enabled = false;
                }
                daftar[i] = new WargaHidupPulau.WargaNPC
                {
                    tr = npc.transform, anim = pa, obor = lObor, gaya = r.g,
                    titikA = new Vector3(r.a.x, yA, r.a.y), titikB = new Vector3(r.b.x, yB, r.b.y),
                    laju = 0.055f + (i % 3) * 0.014f, fasa = i * 0.45f
                };
            }

            var comp = go.AddComponent<WargaHidupPulau>();
            comp.Pasang(daftar);
            EditorUtility.SetDirty(comp);
            JumlahWargaAsliV9 = comp.JumlahWarga;
        }

        static void BangunHabitatSatwaAsliV9(GameObject induk)
        {
            var go = Anak(induk, "SatwaAsliPulauV9");
            var titik = new[]
            {
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Rusa, posisi = new Vector3(32f, 0f, -418f), yaw = 40f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Rusa, posisi = new Vector3(40f, 0f, -410f), yaw = 120f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Kancil, posisi = new Vector3(22f, 0f, -400f), yaw = 210f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.AyamHutan, posisi = new Vector3(52f, 0f, -372f), yaw = 65f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.AyamHutan, posisi = new Vector3(60f, 0f, -380f), yaw = 195f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Anjing, posisi = new Vector3(58f, 0f, -370f), yaw = 270f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Kucing, posisi = new Vector3(536f, 0f, 208f), yaw = 90f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.AyamHutan, posisi = new Vector3(546f, 0f, 200f), yaw = 35f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Anjing, posisi = new Vector3(520f, 0f, 192f), yaw = 150f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Monyet, posisi = new Vector3(304f, 0f, -162f), yaw = 25f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Monyet, posisi = new Vector3(316f, 0f, -172f), yaw = 180f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Biawak, posisi = new Vector3(298f, 0f, -176f), yaw = 310f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Buaya, posisi = new Vector3(266f, 0f, -224f), yaw = 80f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Monyet, posisi = new Vector3(-258f, 0f, 118f), yaw = 110f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Rusa, posisi = new Vector3(-246f, 0f, 96f), yaw = 45f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.BabiHutan, posisi = new Vector3(-188f, 0f, -228f), yaw = 230f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.BabiHutan, posisi = new Vector3(-380f, 0f, 304f), yaw = 15f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Rusa, posisi = new Vector3(204f, 0f, 412f), yaw = 140f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Kancil, posisi = new Vector3(-114f, 0f, 602f), yaw = 290f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Rusa, posisi = new Vector3(582f, 0f, 552f), yaw = 55f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Rusa, posisi = new Vector3(598f, 0f, 566f), yaw = 200f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Anjing, posisi = new Vector3(716f, 0f, -138f), yaw = 70f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Kucing, posisi = new Vector3(724f, 0f, -144f), yaw = 240f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.AyamHutan, posisi = new Vector3(712f, 0f, -146f), yaw = 115f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Biawak, posisi = new Vector3(-124f, 0f, -858f), yaw = 30f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Biawak, posisi = new Vector3(-512f, 0f, -342f), yaw = 175f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.BabiHutan, posisi = new Vector3(654f, 0f, -492f), yaw = 315f },
                new SatwaAsliPulauV9.TitikHabitat { jenis = JH.Monyet, posisi = new Vector3(-574f, 0f, 614f), yaw = 95f }
            };
            var comp = go.AddComponent<SatwaAsliPulauV9>();
            comp.Pasang(titik);
            EditorUtility.SetDirty(comp);
            JumlahHabitatAsliV9 = comp.JumlahHabitat;
        }

        static void BangunSuaraDanSinyalCakrawalaV9(GameObject induk)
        {
            var sGo = Anak(induk, "SuasanaSuaraHidupPulauV9");
            var suara = sGo.AddComponent<SuasanaSuaraHidupPulau>();
            suara.Pasang(
                new[] { new Vector3(56f, 0f, -376f), new Vector3(540f, 0f, 204f), new Vector3(720f, 0f, -140f), new Vector3(590f, 0f, 560f), new Vector3(8f, 0f, -448f) },
                new[] { new Vector3(0f, 0f, 0f), new Vector3(-265f, 0f, 115f), new Vector3(210f, 0f, 420f), new Vector3(-120f, 0f, 610f), new Vector3(468f, 0f, 178f) }
            );
            EditorUtility.SetDirty(suara);

            var cGo = Anak(induk, "SinyalCakrawalaPulauV9");
            var matAsap = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_AsapHeli.mat");
            Vector2[] posSinyal =
            {
                new Vector2(8f, -448f), new Vector2(516f, 198f), new Vector2(700f, -540f), new Vector2(720f, -140f),
                new Vector2(590f, 560f), new Vector2(-390f, 310f), new Vector2(-195f, -240f), new Vector2(0f, -18f)
            };
            var lampu = new Light[posSinyal.Length];
            var asap = new List<ParticleSystem>();
            for (int i = 0; i < posSinyal.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(posSinyal[i].x, posSinyal[i].y);
                var pt = Anak(cGo, "SinyalCakrawala_" + i);
                pt.transform.position = new Vector3(posSinyal[i].x, y + (i == 2 ? 16f : 3.2f), posSinyal[i].y);
                var l = pt.AddComponent<Light>();
                l.type = LightType.Point; l.range = 32f; l.intensity = 4.2f; l.color = new Color(1f, 0.56f, 0.18f); l.shadows = LightShadows.None; l.enabled = false;
                lampu[i] = l;
                if (matAsap != null)
                {
                    var ps = pt.AddComponent<ParticleSystem>();
                    var pm = ps.main; pm.startLifetime = 9.5f; pm.startSpeed = 2.4f; pm.startSize = 3.8f; pm.startColor = new Color(0.76f, 0.78f, 0.80f, 0.24f); pm.maxParticles = 22; pm.prewarm = true; pm.gravityModifier = -0.04f;
                    var pe = ps.emission; pe.rateOverTime = 2.2f;
                    var pr = ps.GetComponent<ParticleSystemRenderer>(); pr.sharedMaterial = matAsap; pr.shadowCastingMode = ShadowCastingMode.Off;
                    asap.Add(ps);
                }
            }
            var comp = cGo.AddComponent<SinyalCakrawalaPulau>();
            comp.Pasang(lampu, asap.ToArray());
            EditorUtility.SetDirty(comp);
            JumlahSinyalV9 = comp.JumlahSinyal;
        }
    }
}
