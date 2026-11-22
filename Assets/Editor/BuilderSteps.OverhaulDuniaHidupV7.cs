using System.Collections.Generic;
using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.EditorTools
{
    // Langkah 7z20: Overhaul Dunia Hidup V7 Skala Penuh (Seluruh 22 Lokasi + 4 Sistem Runtime Baru):
    // - AktivitasDesaSekte (16 agen: 6 kurir sekte berobor, 4 anjing kampung, 6 gagak atap kaget terbang)
    // - IkanLompatDanKepiting (44 biota: 18 ikan melompat, 16 kepiting pantai kabur, 10 capung siang)
    // - MesinDanKincirHidup (roda gila pabrik, kincir air perkebunan, turbin angin, parabola, suar kedip, jemuran ikan/sangkar)
    // - RohKeramatPenjaga (24 bola roh orbit situs keramat, 8 kelelawar malam, 6 rune menhir berpendar)
    // Plus pengayaan prop cerita lingkungan untuk seluruh 22 lokasi pulau 4 km2.
    public static partial class NusantaraMasterBuilder
    {
        public static int JumlahAgenDesaV7, JumlahBiotaAirV7, JumlahMekanisV7, JumlahEntitasMistisV7, JumlahLokasiDiperkayaV7;

        static void LangkahOverhaulDuniaHidupV7()
        {
            JumlahAgenDesaV7 = 0; JumlahBiotaAirV7 = 0; JumlahMekanisV7 = 0; JumlahEntitasMistisV7 = 0; JumlahLokasiDiperkayaV7 = 0;
            var lm = GameObject.Find(NamaRoot + "/World/Landmark") ?? GameObject.Find(NamaRoot + "/Landmark") ?? _root;
            var induk = Anak(lm, "DuniaHidupV7");

            var kayu = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_KayuLapuk2K.mat") ?? AmbilMat("M_Kayu");
            var batu = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_BatuCandi2K.mat") ?? AmbilMat("M_Batu");
            var karat = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_KaratPabrik2K.mat") ?? AmbilMat("M_LogamKarat") ?? MatPolos("M_KaratV7", "#5C3D2E");
            var kainTex = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/kain.png");
            var kainNorm = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/kain_n.png");
            var merahKain = SimpanMat(SafeMaterialFactory.CreatePBR("M_KainSinyalV7", new Color(0.84f, 0.24f, 0.16f), 0f, 0.15f, kainTex, kainNorm));
            var emasKain = SimpanMat(SafeMaterialFactory.CreatePBR("M_KainPolengV7", new Color(0.86f, 0.72f, 0.30f), 0f, 0.15f, kainTex, kainNorm));
            var putihKain = SimpanMat(SafeMaterialFactory.CreatePBR("M_LayarLusuhV7", new Color(0.76f, 0.72f, 0.62f), 0f, 0.15f, kainTex, kainNorm));

            BangunAktivitasDesaSekteV7(induk, kayu, kainTex, kainNorm);
            BangunIkanLompatDanKepitingV7(induk);
            BangunMesinKincirDanDetail22LokasiV7(induk, kayu, batu, karat, merahKain, emasKain, putihKain);
            BangunRohKeramatDanKelelawarV7(induk, batu);

            Catat("Overhaul Dunia Hidup V7 (22/22 Lokasi): " + JumlahLokasiDiperkayaV7 + " situs diperkaya, "
                + JumlahAgenDesaV7 + " agen desa/anjing/gagak, " + JumlahBiotaAirV7 + " ikan/kepiting/capung, "
                + JumlahMekanisV7 + " mekanis/kincir/suar & " + JumlahEntitasMistisV7 + " roh/kelelawar/rune");
        }

        static void BangunAktivitasDesaSekteV7(GameObject induk, Material kayu, Texture2D kainTex, Texture2D kainNorm)
        {
            var go = Anak(induk, "AktivitasDesaSekteV7");
            var jubah = SimpanMat(SafeMaterialFactory.CreatePBR("M_JubahKurirV7", new Color(0.36f, 0.20f, 0.15f), 0f, 0.15f, kainTex, kainNorm));
            var topeng = MatPolos("M_TopengKurirV7", "#C6AE8A");
            var apiMat = SimpanMat(SafeMaterialFactory.CreatePBR("M_ApiKurirV7", new Color(1f, 0.62f, 0.18f), 0f, 0.1f, null, null, new Color(5.2f, 2.3f, 0.5f)));
            var anjingMatA = MatPolos("M_AnjingKampungA", "#8B5A32");
            var anjingMatB = MatPolos("M_AnjingKampungB", "#3A2C22");
            var gagakMat = MatPolos("M_GagakAtapV7", "#181A1E");
            var paruhGagak = MatPolos("M_ParuhGagakV7", "#C89828");

            var daftar = new List<AktivitasDesaSekte.AgenHidup>();

            // 1. 6 kurir/warga sekte berjalan bolak-balik di Desa Mati & Pasar Hantu membawa kayu & obor.
            Vector2[] rA = { new Vector2(512f, 196f), new Vector2(552f, 224f), new Vector2(582f, 180f), new Vector2(534f, 148f), new Vector2(474f, 174f), new Vector2(462f, 186f) };
            Vector2[] rB = { new Vector2(546f, 212f), new Vector2(526f, 232f), new Vector2(558f, 200f), new Vector2(562f, 168f), new Vector2(492f, 192f), new Vector2(484f, 166f) };
            for (int i = 0; i < rA.Length; i++)
            {
                float yA = WorldTerrainGenerator.SampleHeight(rA[i].x, rA[i].y);
                float yB = WorldTerrainGenerator.SampleHeight(rB[i].x, rB[i].y);
                var ag = Anak(go, "KurirSekte_" + i);
                ag.transform.position = new Vector3(rA[i].x, yA, rA[i].y);
                Hias(ag, PrimitiveType.Cylinder, "Jubah", new Vector3(0f, 0.65f, 0f), new Vector3(0.45f, 0.65f, 0.40f), jubah, false);
                Hias(ag, PrimitiveType.Cube, "Bahu", new Vector3(0f, 1.38f, 0f), new Vector3(0.56f, 0.34f, 0.28f), jubah, false);
                var kep = Anak(ag, "Kepala"); kep.transform.localPosition = new Vector3(0f, 1.74f, 0f);
                Hias(kep, PrimitiveType.Sphere, "Tudung", Vector3.zero, new Vector3(0.30f, 0.32f, 0.30f), jubah, false);
                Hias(kep, PrimitiveType.Cube, "Topeng", new Vector3(0f, -0.02f, 0.14f), new Vector3(0.20f, 0.25f, 0.06f), topeng, false);
                // Beban kayu bakar di bahu kiri & batang obor di tangan kanan.
                Hias(ag, PrimitiveType.Cylinder, "BebanKayu", new Vector3(-0.34f, 1.48f, 0f), new Vector3(0.10f, 0.55f, 0.10f), kayu, false).transform.localRotation = Quaternion.Euler(78f, 0f, 12f);
                Hias(ag, PrimitiveType.Cylinder, "BatangObor", new Vector3(0.36f, 1.24f, 0.24f), new Vector3(0.05f, 0.52f, 0.05f), kayu, false);
                Hias(ag, PrimitiveType.Sphere, "ApiObor", new Vector3(0.36f, 1.78f, 0.24f), new Vector3(0.14f, 0.22f, 0.14f), apiMat, false).GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                var lGo = Anak(ag, "LampuObor"); lGo.transform.localPosition = new Vector3(0.36f, 1.88f, 0.24f);
                var l = lGo.AddComponent<Light>(); l.type = LightType.Point; l.range = 9f; l.intensity = 2.0f; l.color = new Color(1f, 0.58f, 0.22f); l.shadows = LightShadows.None; l.enabled = false;
                daftar.Add(new AktivitasDesaSekte.AgenHidup
                {
                    tr = ag.transform, kepala = kep.transform, obor = l, tipe = AktivitasDesaSekte.TipeAgen.KurirSekte,
                    titikA = new Vector3(rA[i].x, yA, rA[i].y), titikB = new Vector3(rB[i].x, yB, rB[i].y),
                    laju = 0.065f + (i % 3) * 0.015f, fasa = i * 0.42f
                });
            }

            // 2. 4 anjing kampung berkeliaran mengendus di Kampung Sukamaju & Desa Nelayan Timur.
            Vector2[] dA = { new Vector2(52f, -372f), new Vector2(568f, 570f), new Vector2(714f, -134f), new Vector2(728f, -146f) };
            Vector2[] dB = { new Vector2(66f, -384f), new Vector2(594f, 588f), new Vector2(734f, -144f), new Vector2(710f, -152f) };
            for (int i = 0; i < dA.Length; i++)
            {
                float yA = WorldTerrainGenerator.SampleHeight(dA[i].x, dA[i].y);
                float yB = WorldTerrainGenerator.SampleHeight(dB[i].x, dB[i].y);
                var dg = Anak(go, "AnjingKampung_" + i);
                dg.transform.position = new Vector3(dA[i].x, yA, dA[i].y);
                var mD = i % 2 == 0 ? anjingMatA : anjingMatB;
                Hias(dg, PrimitiveType.Capsule, "Badan", new Vector3(0f, 0.44f, 0f), new Vector3(0.30f, 0.44f, 0.32f), mD, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                for (int k = 0; k < 4; k++)
                    Hias(dg, PrimitiveType.Cylinder, "Kaki_" + k, new Vector3(k % 2 == 0 ? -0.12f : 0.12f, 0.22f, k < 2 ? 0.24f : -0.24f), new Vector3(0.07f, 0.22f, 0.07f), mD, false);
                Hias(dg, PrimitiveType.Cylinder, "Ekor", new Vector3(0f, 0.56f, -0.44f), new Vector3(0.05f, 0.20f, 0.05f), mD, false).transform.localRotation = Quaternion.Euler(-45f, 0f, 0f);
                var kep = Anak(dg, "Kepala"); kep.transform.localPosition = new Vector3(0f, 0.56f, 0.38f);
                Hias(kep, PrimitiveType.Sphere, "Tengkorak", Vector3.zero, new Vector3(0.22f, 0.20f, 0.26f), mD, false);
                Hias(kep, PrimitiveType.Cube, "Moncong", new Vector3(0f, -0.02f, 0.16f), new Vector3(0.11f, 0.09f, 0.14f), mD, false);
                daftar.Add(new AktivitasDesaSekte.AgenHidup
                {
                    tr = dg.transform, kepala = kep.transform, tipe = AktivitasDesaSekte.TipeAgen.AnjingKampung,
                    titikA = new Vector3(dA[i].x, yA, dA[i].y), titikB = new Vector3(dB[i].x, yB, dB[i].y),
                    laju = 0.09f + (i % 2) * 0.025f, fasa = i * 0.65f
                });
            }

            // 3. 6 gagak bertengger di atap Desa Mati, Kampung Sukamaju, Pabrik Gula & Rumah Pohon (kaget terbang saat didekati).
            Vector3[] posGagak =
            {
                new Vector3(564f, 5.2f, 224f), new Vector3(524f, 5.0f, 236f), new Vector3(58f, 3.8f, -376f),
                new Vector3(642f, 7.6f, -518f), new Vector3(68f, 8.8f, -406f), new Vector3(468f, 3.6f, 178f)
            };
            for (int i = 0; i < posGagak.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(posGagak[i].x, posGagak[i].z) + posGagak[i].y;
                var gg = Anak(go, "GagakAtap_" + i);
                gg.transform.position = new Vector3(posGagak[i].x, y, posGagak[i].z);
                Hias(gg, PrimitiveType.Capsule, "Badan", Vector3.zero, new Vector3(0.18f, 0.28f, 0.18f), gagakMat, false).transform.localRotation = Quaternion.Euler(65f, 0f, 0f);
                var kep = Anak(gg, "Kepala"); kep.transform.localPosition = new Vector3(0f, 0.18f, 0.18f);
                Hias(kep, PrimitiveType.Sphere, "Bulat", Vector3.zero, Vector3.one * 0.14f, gagakMat, false);
                Hias(kep, PrimitiveType.Cube, "Paruh", new Vector3(0f, -0.01f, 0.11f), new Vector3(0.05f, 0.05f, 0.14f), paruhGagak, false);
                var sKiri = Anak(gg, "SayapKiri"); sKiri.transform.localPosition = new Vector3(-0.10f, 0.04f, 0f);
                Hias(sKiri, PrimitiveType.Cube, "BuluL", new Vector3(-0.38f, 0f, 0f), new Vector3(0.76f, 0.03f, 0.26f), gagakMat, false);
                var sKanan = Anak(gg, "SayapKanan"); sKanan.transform.localPosition = new Vector3(0.10f, 0.04f, 0f);
                Hias(sKanan, PrimitiveType.Cube, "BuluR", new Vector3(0.38f, 0f, 0f), new Vector3(0.76f, 0.03f, 0.26f), gagakMat, false);
                daftar.Add(new AktivitasDesaSekte.AgenHidup
                {
                    tr = gg.transform, kepala = kep.transform, sayapKiri = sKiri.transform, sayapKanan = sKanan.transform,
                    tipe = AktivitasDesaSekte.TipeAgen.GagakAtap,
                    titikA = new Vector3(posGagak[i].x, y, posGagak[i].z), titikB = new Vector3(posGagak[i].x, y + 6f, posGagak[i].z),
                    laju = 1.6f, fasa = i * 1.1f
                });
            }

            var comp = go.AddComponent<AktivitasDesaSekte>();
            comp.Pasang(daftar.ToArray());
            EditorUtility.SetDirty(comp);
            JumlahAgenDesaV7 = comp.JumlahAgen;
        }

        static void BangunIkanLompatDanKepitingV7(GameObject induk)
        {
            var go = Anak(induk, "IkanLompatDanKepitingV7");
            var matIkan = SimpanMat(SafeMaterialFactory.CreatePBR("M_IkanPerakV7", new Color(0.68f, 0.78f, 0.84f), 0.55f, 0.82f));
            var matKepiting = MatPolos("M_KepitingPantaiV7", "#B84228");
            var matCapung = SimpanMat(SafeMaterialFactory.CreatePBR("M_CapungBiruV7", new Color(0.22f, 0.78f, 0.92f), 0.3f, 0.85f));
            var daftar = new List<IkanLompatDanKepiting.BiotaHidup>();

            // 1. 18 ikan melompat di Air Terjun, Jembatan Gantung, Jembatan Batu, Dermaga Barat, Desa Nelayan & Pulau Karang.
            Vector2[] posIkan =
            {
                new Vector2(308f, -172f), new Vector2(268f, -220f), new Vector2(-240f, 88f),
                new Vector2(-520f, -356f), new Vector2(746f, -142f), new Vector2(-318f, -876f)
            };
            for (int i = 0; i < 18; i++)
            {
                var c = posIkan[i % posIkan.Length];
                float? wl = WorldTerrainGenerator.GetWaterLevelAt(c.x, c.y);
                float wy = wl.HasValue ? wl.Value : Mathf.Max(0.1f, WorldTerrainGenerator.SampleHeight(c.x, c.y));
                var ik = Anak(go, "IkanLompat_" + i);
                ik.transform.position = new Vector3(c.x, wy, c.y);
                Hias(ik, PrimitiveType.Capsule, "Tubuh", Vector3.zero, new Vector3(0.14f, 0.34f, 0.18f), matIkan, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Hias(ik, PrimitiveType.Cube, "Ekor", new Vector3(0f, 0f, -0.34f), new Vector3(0.04f, 0.22f, 0.16f), matIkan, false);
                daftar.Add(new IkanLompatDanKepiting.BiotaHidup
                {
                    tr = ik.transform, jenis = IkanLompatDanKepiting.JenisBiota.IkanLompat,
                    pusat = new Vector3(c.x, wy, c.y), radius = 3.6f + (i % 3) * 1.4f,
                    laju = 0.75f + (i % 4) * 0.18f, fasa = i * 1.3f
                });
            }

            // 2. 16 kepiting pantai berjalan menyamping & kabur di Pantai Selatan, Bangkai Kapal, Pulau Karang & Desa Nelayan.
            Vector2[] posKepiting = { new Vector2(-124f, -864f), new Vector2(-180f, -870f), new Vector2(-322f, -882f), new Vector2(726f, -148f) };
            for (int i = 0; i < 16; i++)
            {
                var c = posKepiting[i % posKepiting.Length];
                float x = c.x + Mathf.Cos(i * 1.7f) * 6.5f, z = c.y + Mathf.Sin(i * 1.7f) * 4.5f;
                float y = Mathf.Max(0.2f, WorldTerrainGenerator.SampleHeight(x, z)) + 0.06f;
                var kp = Anak(go, "KepitingPantai_" + i);
                kp.transform.position = new Vector3(x, y, z);
                Hias(kp, PrimitiveType.Sphere, "Cangkang", Vector3.zero, new Vector3(0.26f, 0.10f, 0.22f), matKepiting, false);
                Hias(kp, PrimitiveType.Cube, "CapitL", new Vector3(-0.18f, 0.02f, 0.12f), new Vector3(0.08f, 0.05f, 0.14f), matKepiting, false);
                Hias(kp, PrimitiveType.Cube, "CapitR", new Vector3(0.18f, 0.02f, 0.12f), new Vector3(0.08f, 0.05f, 0.14f), matKepiting, false);
                daftar.Add(new IkanLompatDanKepiting.BiotaHidup
                {
                    tr = kp.transform, jenis = IkanLompatDanKepiting.JenisBiota.KepitingPantai,
                    pusat = new Vector3(c.x, y, c.y), radius = 7.5f, laju = 0.45f + (i % 3) * 0.12f, fasa = i * 1.5f
                });
            }

            // 3. 10 capung biru melesat di atas kolam Air Terjun, Jembatan Gantung & Kuil Bambu Utara.
            Vector2[] posCapung = { new Vector2(310f, -168f), new Vector2(272f, -218f), new Vector2(-120f, 610f) };
            for (int i = 0; i < 10; i++)
            {
                var c = posCapung[i % posCapung.Length];
                float y = Mathf.Max(0.4f, WorldTerrainGenerator.SampleHeight(c.x, c.y));
                var cp = Anak(go, "CapungAir_" + i);
                cp.transform.position = new Vector3(c.x, y + 0.9f, c.y);
                Hias(cp, PrimitiveType.Cylinder, "Badan", Vector3.zero, new Vector3(0.03f, 0.16f, 0.03f), matCapung, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Hias(cp, PrimitiveType.Cube, "Sayap", new Vector3(0f, 0.02f, 0.03f), new Vector3(0.28f, 0.01f, 0.06f), matCapung, false);
                daftar.Add(new IkanLompatDanKepiting.BiotaHidup
                {
                    tr = cp.transform, jenis = IkanLompatDanKepiting.JenisBiota.CapungAir,
                    pusat = new Vector3(c.x, y, c.y), radius = 4.5f + (i % 3) * 1.2f, laju = 1.8f + (i % 3) * 0.35f, fasa = i * 2.1f
                });
            }

            var comp = go.AddComponent<IkanLompatDanKepiting>();
            comp.Pasang(daftar.ToArray());
            EditorUtility.SetDirty(comp);
            JumlahBiotaAirV7 = comp.JumlahBiota;
        }

        static void BangunMesinKincirDanDetail22LokasiV7(GameObject induk, Material kayu, Material batu, Material karat,
            Material merahKain, Material emasKain, Material putihKain)
        {
            var go = Anak(induk, "MekanisDanDetail22LokasiV7");
            var suarMerahMat = SimpanMat(SafeMaterialFactory.CreatePBR("M_SuarMerahV7", new Color(0.95f, 0.18f, 0.14f), 0f, 0.6f, null, null, new Color(2.4f, 0.25f, 0.18f)));
            var suarHijauMat = SimpanMat(SafeMaterialFactory.CreatePBR("M_SuarRisetV7", new Color(0.22f, 0.92f, 0.56f), 0f, 0.6f, null, null, new Color(0.35f, 2.2f, 0.95f)));
            var ikanAsinMat = MatPolos("M_IkanAsinV7", "#9C8E7A");
            var lenteraMat = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_LenteraHangatV6.mat") ?? emasKain;

            var rotorList = new List<MesinDanKincirHidup.RotorHidup>();
            var suarList = new List<MesinDanKincirHidup.SuarKedip>();
            var gantungList = new List<Transform>();

            // LOKASI 1: Bangkai Heli (8, -448) — Suar SAR merah berkedip + bendera darurat oranye-merah berkibar.
            TambahSuarKedip(go, "SuarSARHeli", new Vector3(11.4f, WorldTerrainGenerator.SampleHeight(11.4f, -450.2f) + 2.85f, -450.2f), karat, suarMerahMat, new Color(1f, 0.18f, 0.12f), 0.95f, 0f, suarList);
            TambahBenderaGantung(go, "BenderaDaruratHeli", new Vector3(4.5f, WorldTerrainGenerator.SampleHeight(4.5f, -445f), -445f), kayu, merahKain, gantungList);
            JumlahLokasiDiperkayaV7++;

            // LOKASI 2: Kampung Sukamaju (56, -376) — Lesung kayu + 2 sangkar burung gantung bergoyang angin.
            float yKmp = WorldTerrainGenerator.SampleHeight(53.5f, -377.5f);
            var lesung = Anak(go, "LesungKampungSukamaju"); lesung.transform.position = new Vector3(53.5f, yKmp, -377.5f);
            Hias(lesung, PrimitiveType.Cube, "Lesung", new Vector3(0f, 0.28f, 0f), new Vector3(1.55f, 0.42f, 0.55f), kayu, true);
            Hias(lesung, PrimitiveType.Cylinder, "Alu", new Vector3(0.25f, 0.65f, 0f), new Vector3(0.08f, 0.85f, 0.08f), kayu, false).transform.localRotation = Quaternion.Euler(0f, 0f, 28f);
            TambahSangkarGantung(go, "SangkarKampung_0", new Vector3(58.5f, WorldTerrainGenerator.SampleHeight(58.5f, -374.5f), -374.5f), kayu, gantungList);
            TambahSangkarGantung(go, "SangkarKampung_1", new Vector3(566f, WorldTerrainGenerator.SampleHeight(566f, 566f), 566f), kayu, gantungList);
            JumlahLokasiDiperkayaV7++;

            // LOKASI 3 & 4: Desa Mati (540, 204) & Pasar Hantu (468, 178) — Lonceng angin bambu + kabut pasar hantu.
            TambahSangkarGantung(go, "LoncengAnginDesa_0", new Vector3(522f, WorldTerrainGenerator.SampleHeight(522f, 198f), 198f), kayu, gantungList);
            TambahSangkarGantung(go, "LoncengAnginPasar_0", new Vector3(466f, WorldTerrainGenerator.SampleHeight(466f, 176f), 176f), kayu, gantungList);
            TambahKabutLokasi(go, "KabutPasarHantuV7", new Vector3(468f, WorldTerrainGenerator.SampleHeight(468f, 178f) + 0.65f, 178f), new Vector3(16f, 16f, 1f), new Color(0.74f, 0.82f, 0.88f, 0.20f));
            JumlahLokasiDiperkayaV7 += 2;

            // LOKASI 5: Altar Beringin Agung (0, 0) — 4 umbul-umbul janur emas-merah keramat bergoyang.
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f + 0.4f;
                float x = Mathf.Cos(a) * 14f, z = -10f + Mathf.Sin(a) * 14f;
                TambahBenderaGantung(go, "JanurAltar_" + i, new Vector3(x, WorldTerrainGenerator.SampleHeight(x, z), z), kayu, i % 2 == 0 ? emasKain : merahKain, gantungList);
            }
            JumlahLokasiDiperkayaV7++;

            // LOKASI 6 & 7: Air Terjun (312, -165) & Gua Air Terjun (326, -158) — Batu pijakan lumut & embun air terjun.
            for (int i = 0; i < 5; i++)
            {
                float sx = 304f + i * 2.1f, sz = -175f + Mathf.Sin(i) * 1.4f;
                var st = Anak(go, "BatuPijakanAirTerjun_" + i);
                st.transform.position = new Vector3(sx, WorldTerrainGenerator.SampleHeight(sx, sz) + 0.14f, sz);
                Hias(st, PrimitiveType.Cylinder, "Batu", Vector3.zero, new Vector3(1.1f, 0.22f, 0.95f), batu, true);
            }
            TambahKabutLokasi(go, "EmbunKolamAirTerjunV7", new Vector3(312f, WorldTerrainGenerator.SampleHeight(312f, -168f) + 1.2f, -168f), new Vector3(15f, 15f, 1f), new Color(0.82f, 0.94f, 0.98f, 0.22f));
            JumlahLokasiDiperkayaV7 += 2;

            // LOKASI 8: Candi Runtuh (-265, 115) — 2 Arca Penjaga Dwarapala di kanan-kiri tangga candi.
            for (int s = -1; s <= 1; s += 2)
            {
                float cx = -265f + s * 5.2f, cz = 104f;
                var arca = Anak(go, s < 0 ? "DwarapalaKiri" : "DwarapalaKanan");
                arca.transform.position = new Vector3(cx, WorldTerrainGenerator.SampleHeight(cx, cz), cz);
                Hias(arca, PrimitiveType.Cube, "Alas", new Vector3(0f, 0.4f, 0f), new Vector3(1.4f, 0.8f, 1.4f), batu, true);
                Hias(arca, PrimitiveType.Cylinder, "Badan", new Vector3(0f, 1.35f, 0f), new Vector3(0.85f, 0.72f, 0.85f), batu, true);
                Hias(arca, PrimitiveType.Sphere, "Kepala", new Vector3(0f, 2.25f, 0f), new Vector3(0.68f, 0.62f, 0.68f), batu, false);
                Hias(arca, PrimitiveType.Cylinder, "Gada", new Vector3(s * 0.52f, 1.45f, 0.35f), new Vector3(0.22f, 0.85f, 0.22f), batu, false);
            }
            JumlahLokasiDiperkayaV7++;

            // LOKASI 9 & 10: Jembatan Batu Kuno (-240, 88) & Jembatan Gantung (272, -218) — Pita doa & lentera tali.
            TambahBenderaGantung(go, "PitaJembatanBatu", new Vector3(-237f, WorldTerrainGenerator.SampleHeight(-237f, 88f), 88f), kayu, emasKain, gantungList);
            TambahSangkarGantung(go, "LenteraJembatanGantung_0", new Vector3(266f, WorldTerrainGenerator.SampleHeight(266f, -218f), -218f), kayu, gantungList);
            TambahSangkarGantung(go, "LenteraJembatanGantung_1", new Vector3(278f, WorldTerrainGenerator.SampleHeight(278f, -218f), -218f), kayu, gantungList);
            JumlahLokasiDiperkayaV7 += 2;

            // LOKASI 11: Bangkai Kapal Pantai (-115, -865) & Mercusuar (-190, -876) — Kabut ombak pantai & tong kargo.
            TambahKabutLokasi(go, "KabutOmbakPantai_0", new Vector3(-125f, 0.65f, -874f), new Vector3(18f, 14f, 1f), new Color(0.88f, 0.94f, 0.98f, 0.18f));
            TambahKabutLokasi(go, "KabutOmbakPantai_1", new Vector3(-186f, 0.65f, -882f), new Vector3(18f, 14f, 1f), new Color(0.88f, 0.94f, 0.98f, 0.18f));
            JumlahLokasiDiperkayaV7++;

            // LOKASI 12: Pabrik Gula Sinar Tebu (665, -500) — 2 Roda Gila Besi Berputar Pelan di Gedung Giling.
            for (int i = 0; i < 2; i++)
            {
                float fx = 650f + i * 9f, fz = -516f;
                var roda = Anak(go, "RodaGilaPabrik_" + i);
                roda.transform.position = new Vector3(fx, WorldTerrainGenerator.SampleHeight(fx, fz) + 2.2f, fz);
                Hias(roda, PrimitiveType.Cylinder, "Ring", Vector3.zero, new Vector3(2.8f, 0.24f, 2.8f), karat, false).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                Hias(roda, PrimitiveType.Cube, "PalangA", Vector3.zero, new Vector3(0.32f, 2.7f, 0.26f), karat, false);
                Hias(roda, PrimitiveType.Cube, "PalangB", Vector3.zero, new Vector3(0.32f, 0.26f, 2.7f), karat, false);
                rotorList.Add(new MesinDanKincirHidup.RotorHidup { tr = roda.transform, sumbu = Vector3.right, lajuDerajat = 14f + i * 5f, ikutAngin = true });
            }
            JumlahLokasiDiperkayaV7++;

            // LOKASI 13: Rumah Pohon Heli (68, -406) — Bendera pos & lentera gantung di bawah dek.
            TambahBenderaGantung(go, "BenderaRumahPohon", new Vector3(71f, WorldTerrainGenerator.SampleHeight(71f, -406f), -406f), kayu, emasKain, gantungList);
            JumlahLokasiDiperkayaV7++;

            // LOKASI 14: Desa Nelayan Timur (720, -140) — 3 Rak Jemuran Ikan Asin Bergoyang Angin Laut.
            for (int i = 0; i < 3; i++)
            {
                float ix = 716f + i * 5.2f, iz = -136f - i * 3.2f;
                var rak = Anak(go, "JemuranIkanNelayan_" + i);
                rak.transform.position = new Vector3(ix, WorldTerrainGenerator.SampleHeight(ix, iz), iz);
                rak.transform.rotation = Quaternion.Euler(0f, i * 28f, 0f);
                Hias(rak, PrimitiveType.Cylinder, "TiangL", new Vector3(-1.3f, 0.95f, 0f), new Vector3(0.08f, 0.95f, 0.08f), kayu, false);
                Hias(rak, PrimitiveType.Cylinder, "TiangR", new Vector3(1.3f, 0.95f, 0f), new Vector3(0.08f, 0.95f, 0.08f), kayu, false);
                Hias(rak, PrimitiveType.Cylinder, "Palang", new Vector3(0f, 1.85f, 0f), new Vector3(0.05f, 1.4f, 0.05f), kayu, false).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                for (int f = 0; f < 4; f++)
                {
                    var pv = Anak(rak, "IkanGantung_" + f); pv.transform.localPosition = new Vector3(-0.9f + f * 0.6f, 1.82f, 0f);
                    Hias(pv, PrimitiveType.Capsule, "Ikan", new Vector3(0f, -0.32f, 0f), new Vector3(0.12f, 0.24f, 0.06f), ikanAsinMat, false);
                    gantungList.Add(pv.transform);
                }
            }
            JumlahLokasiDiperkayaV7++;

            // LOKASI 15: Pos Gunung Kapur (-580, 620) — Turbin Angin Berputar + Suar Merah Menara Radio 18 m.
            TambahTurbinAngin(go, "TurbinGunungKapur", new Vector3(-574f, WorldTerrainGenerator.SampleHeight(-574f, 618f), 618f), karat, 92f, rotorList);
            TambahSuarKedip(go, "SuarMenaraGunungKapur", new Vector3(-580f, WorldTerrainGenerator.SampleHeight(-580f, 620f) + 18.2f, 620f), karat, suarMerahMat, new Color(1f, 0.20f, 0.14f), 0.72f, 0.5f, suarList);
            JumlahLokasiDiperkayaV7++;

            // LOKASI 16: Perkebunan Timur Laut (590, 560) — Kincir Air Irigasi Kayu Berputar + Kabut Pagi Perkebunan.
            var kincir = Anak(go, "KincirAirPerkebunan");
            float yPk = WorldTerrainGenerator.SampleHeight(586f, 554f);
            kincir.transform.position = new Vector3(586f, yPk + 1.85f, 554f);
            Hias(kincir, PrimitiveType.Cylinder, "Poros", Vector3.zero, new Vector3(0.28f, 0.95f, 0.28f), kayu, false).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            for (int b = 0; b < 6; b++)
            {
                var bilah = Hias(kincir, PrimitiveType.Cube, "Sudu_" + b, Vector3.zero, new Vector3(0.65f, 3.2f, 0.18f), kayu, false);
                bilah.transform.localRotation = Quaternion.Euler(b * 30f, 0f, 0f);
            }
            rotorList.Add(new MesinDanKincirHidup.RotorHidup { tr = kincir.transform, sumbu = Vector3.right, lajuDerajat = 28f, ikutAngin = false });
            TambahKabutLokasi(go, "KabutPerkebunanV7", new Vector3(592f, yPk + 0.9f, 562f), new Vector3(22f, 22f, 1f), new Color(0.86f, 0.92f, 0.88f, 0.16f));
            JumlahLokasiDiperkayaV7++;

            // LOKASI 17: Pulau Karang Selatan (-320, -885) — Tiang Layar Karam + Layar Lusuh Berkibar.
            var tiangK = Anak(go, "TiangKaramKarang");
            tiangK.transform.position = new Vector3(-320f, WorldTerrainGenerator.SampleHeight(-320f, -885f), -885f);
            Hias(tiangK, PrimitiveType.Cylinder, "Tiang", new Vector3(0f, 3.1f, 0f), new Vector3(0.20f, 3.1f, 0.20f), kayu, true).transform.localRotation = Quaternion.Euler(8f, 0f, -10f);
            var layarPv = Anak(tiangK, "LayarPivot"); layarPv.transform.localPosition = new Vector3(0.2f, 5.2f, 0f);
            Hias(layarPv, PrimitiveType.Cube, "LayarLusuh", new Vector3(0.95f, -1.2f, 0f), new Vector3(1.85f, 2.3f, 0.04f), putihKain, false);
            gantungList.Add(layarPv.transform);
            JumlahLokasiDiperkayaV7++;

            // LOKASI 18: Kuil Bambu Utara (-120, 610) — Pancuran Bambu & Umbul-Umbul Kuil.
            var pancur = Anak(go, "PancuranKuilBambu");
            pancur.transform.position = new Vector3(-116f, WorldTerrainGenerator.SampleHeight(-116f, 612f), 612f);
            Hias(pancur, PrimitiveType.Cylinder, "BakBatu", new Vector3(0f, 0.32f, 0f), new Vector3(0.95f, 0.32f, 0.95f), batu, true);
            Hias(pancur, PrimitiveType.Cylinder, "PipaBambu", new Vector3(-0.55f, 0.85f, 0f), new Vector3(0.08f, 0.55f, 0.08f), kayu, false).transform.localRotation = Quaternion.Euler(0f, 0f, -62f);
            TambahBenderaGantung(go, "UmbulKuilBambu", new Vector3(-124f, WorldTerrainGenerator.SampleHeight(-124f, 608f), 608f), kayu, merahKain, gantungList);
            JumlahLokasiDiperkayaV7++;

            // LOKASI 19: Dermaga Tua Barat (-520, -340) — Lentera Badai Dermaga & Peti Ikan.
            TambahSangkarGantung(go, "LenteraBadaiDermagaBarat", new Vector3(-518f, WorldTerrainGenerator.SampleHeight(-518f, -346f), -346f), kayu, gantungList);
            JumlahLokasiDiperkayaV7++;

            // LOKASI 20: Menara Pengawas Api (-195, -240) — Turbin Angin Puncak + Suar Pengawas Hangat.
            float yMn = WorldTerrainGenerator.SampleHeight(-195f, -240f);
            TambahTurbinAngin(go, "AnemometerMenaraApi", new Vector3(-192.2f, yMn + 12.3f, -240f), karat, 118f, rotorList);
            TambahSuarKedip(go, "SuarMenaraApi", new Vector3(-195f, yMn + 15.6f, -240f), karat, lenteraMat, new Color(1f, 0.78f, 0.34f), 0.55f, 0.2f, suarList);
            JumlahLokasiDiperkayaV7++;

            // LOKASI 21: Kamp Peneliti Terbengkalai (-390, 310) — Parabola Berputar Pelan + Suar Status Generator Hijau.
            float yKp = WorldTerrainGenerator.SampleHeight(-392f, 312f);
            var parab = Anak(go, "ParabolaKampPeneliti");
            parab.transform.position = new Vector3(-392f, yKp + 1.85f, 312f);
            Hias(parab, PrimitiveType.Cylinder, "Piringan", Vector3.zero, new Vector3(1.6f, 0.14f, 1.6f), putihKain, false).transform.localRotation = Quaternion.Euler(52f, 0f, 0f);
            Hias(parab, PrimitiveType.Cylinder, "TiangFeed", new Vector3(0f, 0.45f, 0.55f), new Vector3(0.06f, 0.42f, 0.06f), karat, false);
            rotorList.Add(new MesinDanKincirHidup.RotorHidup { tr = parab.transform, sumbu = Vector3.up, lajuDerajat = 18f, ikutAngin = false });
            TambahSuarKedip(go, "SuarGeneratorPeneliti", new Vector3(-388f, WorldTerrainGenerator.SampleHeight(-388f, 308f) + 1.45f, 308f), karat, suarHijauMat, new Color(0.24f, 0.96f, 0.56f), 1.35f, 0.9f, suarList);
            JumlahLokasiDiperkayaV7++;

            // LOKASI 22: Taman Batu Megalit (210, 420) — Bendera ritual & kabut kuno lingkaran batu.
            TambahKabutLokasi(go, "KabutMegalitV7", new Vector3(210f, WorldTerrainGenerator.SampleHeight(210f, 420f) + 0.55f, 420f), new Vector3(18f, 18f, 1f), new Color(0.54f, 0.86f, 0.82f, 0.18f));
            JumlahLokasiDiperkayaV7++;

            var comp = go.AddComponent<MesinDanKincirHidup>();
            comp.Pasang(rotorList.ToArray(), suarList.ToArray(), gantungList.ToArray());
            EditorUtility.SetDirty(comp);
            JumlahMekanisV7 = comp.JumlahMekanis;
        }

        static void BangunRohKeramatDanKelelawarV7(GameObject induk, Material batu)
        {
            var go = Anak(induk, "RohKeramatPenjagaV7");
            var rohEmas = SimpanMat(SafeMaterialFactory.CreatePBR("M_RohEmasV7", new Color(1f, 0.86f, 0.36f), 0f, 0.1f, null, null, new Color(3.6f, 2.5f, 0.65f)));
            var rohCyan = SimpanMat(SafeMaterialFactory.CreatePBR("M_RohCyanV7", new Color(0.36f, 0.95f, 0.88f), 0f, 0.1f, null, null, new Color(0.75f, 3.2f, 2.8f)));
            var kelMat = MatPolos("M_KelelawarMalamV7", "#1A1618");
            var runeMat = SimpanMat(SafeMaterialFactory.CreatePBR("M_RuneMenhirV7", new Color(0.24f, 0.42f, 0.40f), 0f, 0.3f, null, null, new Color(0.08f, 0.35f, 0.30f)));

            var rohList = new List<RohKeramatPenjaga.RohOrbit>();
            var kelList = new List<RohKeramatPenjaga.KelelawarMalam>();
            var runeList = new List<Renderer>();

            // 24 bola roh mengorbit 5 situs keramat: Altar Beringin, Taman Megalit, Kuil Bambu, Candi Runtuh & Pasar Hantu.
            Vector2[] situs = { new Vector2(0f, -6f), new Vector2(210f, 420f), new Vector2(-120f, 610f), new Vector2(-265f, 115f), new Vector2(468f, 178f) };
            for (int i = 0; i < 24; i++)
            {
                var c = situs[i % situs.Length];
                float y = WorldTerrainGenerator.SampleHeight(c.x, c.y);
                var r = Anak(go, "RohPenjaga_" + i);
                r.transform.position = new Vector3(c.x, y + 2.5f, c.y);
                var sp = Hias(r, PrimitiveType.Sphere, "Inti", Vector3.zero, Vector3.one, i % 2 == 0 ? rohEmas : rohCyan, false);
                sp.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                rohList.Add(new RohKeramatPenjaga.RohOrbit
                {
                    tr = r.transform, pusat = new Vector3(c.x, y, c.y), radius = 5.5f + (i % 5) * 2.6f,
                    tinggi = 2.0f + (i % 4) * 1.15f, laju = (i % 2 == 0 ? 1f : -1f) * (0.45f + (i % 3) * 0.18f), fasa = i * 1.25f
                });
            }

            // 8 kelelawar malam melingkar di atas Candi, Megalit, Desa Mati & Gua Air Terjun saat malam.
            Vector2[] sarang = { new Vector2(-265f, 115f), new Vector2(210f, 420f), new Vector2(540f, 204f), new Vector2(326f, -158f) };
            for (int i = 0; i < 8; i++)
            {
                var c = sarang[i % sarang.Length];
                float y = WorldTerrainGenerator.SampleHeight(c.x, c.y);
                var k = Anak(go, "KelelawarMalamV7_" + i);
                k.transform.position = new Vector3(c.x, y + 7f, c.y);
                Hias(k, PrimitiveType.Sphere, "Badan", Vector3.zero, new Vector3(0.16f, 0.14f, 0.24f), kelMat, false);
                var sL = Anak(k, "SayapKiri"); sL.transform.localPosition = new Vector3(-0.08f, 0f, 0f);
                Hias(sL, PrimitiveType.Cube, "MembranL", new Vector3(-0.34f, 0f, 0f), new Vector3(0.68f, 0.02f, 0.22f), kelMat, false);
                var sR = Anak(k, "SayapKanan"); sR.transform.localPosition = new Vector3(0.08f, 0f, 0f);
                Hias(sR, PrimitiveType.Cube, "MembranR", new Vector3(0.34f, 0f, 0f), new Vector3(0.68f, 0.02f, 0.22f), kelMat, false);
                kelList.Add(new RohKeramatPenjaga.KelelawarMalam
                {
                    tr = k.transform, sayapKiri = sL.transform, sayapKanan = sR.transform,
                    pusat = new Vector3(c.x, y, c.y), radius = 8.5f + (i % 3) * 3.5f,
                    laju = (i % 2 == 0 ? 1f : -1f) * (1.15f + (i % 3) * 0.25f), fasa = i * 1.4f
                });
            }

            // 6 menhir berukir rune berpendar malam di Taman Megalit, Candi Runtuh & Kuil Bambu.
            Vector2[] posRune = { new Vector2(206f, 422f), new Vector2(214f, 422f), new Vector2(-261f, 112f), new Vector2(-269f, 112f), new Vector2(-118f, 608f), new Vector2(0f, -14f) };
            for (int i = 0; i < posRune.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(posRune[i].x, posRune[i].y);
                var mn = Anak(go, "MenhirRune_" + i);
                mn.transform.position = new Vector3(posRune[i].x, y, posRune[i].y);
                Hias(mn, PrimitiveType.Cube, "BatuMenhir", new Vector3(0f, 1.35f, 0f), new Vector3(0.72f, 2.7f, 0.42f), batu, true);
                var rn = Hias(mn, PrimitiveType.Cube, "UkiranRune", new Vector3(0f, 1.55f, 0.22f), new Vector3(0.28f, 1.35f, 0.04f), runeMat, false);
                runeList.Add(rn.GetComponent<Renderer>());
            }

            var comp = go.AddComponent<RohKeramatPenjaga>();
            comp.Pasang(rohList.ToArray(), kelList.ToArray(), runeList.ToArray());
            EditorUtility.SetDirty(comp);
            JumlahEntitasMistisV7 = comp.JumlahEntitasMistis;
        }

        static void TambahTurbinAngin(GameObject induk, string nama, Vector3 pos, Material karat, float laju, List<MesinDanKincirHidup.RotorHidup> list)
        {
            var g = Anak(induk, nama); g.transform.position = pos;
            Hias(g, PrimitiveType.Cylinder, "Tiang", new Vector3(0f, 1.9f, 0f), new Vector3(0.14f, 1.9f, 0.14f), karat, false);
            var hub = Anak(g, "BalingBaling"); hub.transform.localPosition = new Vector3(0f, 3.85f, 0.22f);
            for (int i = 0; i < 3; i++)
                Hias(hub, PrimitiveType.Cube, "Bilah_" + i, Vector3.zero, new Vector3(0.16f, 1.65f, 0.04f), karat, false).transform.localRotation = Quaternion.Euler(0f, 0f, i * 60f);
            list.Add(new MesinDanKincirHidup.RotorHidup { tr = hub.transform, sumbu = Vector3.forward, lajuDerajat = laju, ikutAngin = true });
        }

        static void TambahSuarKedip(GameObject induk, string nama, Vector3 pos, Material tiangMat, Material suarMat, Color warna, float frek, float fasa, List<MesinDanKincirHidup.SuarKedip> list)
        {
            var g = Anak(induk, nama); g.transform.position = pos;
            Hias(g, PrimitiveType.Cylinder, "Dudukan", new Vector3(0f, -0.35f, 0f), new Vector3(0.10f, 0.35f, 0.10f), tiangMat, false);
            var bola = Hias(g, PrimitiveType.Sphere, "LampuSuar", Vector3.zero, Vector3.one * 0.26f, suarMat, false);
            var lGo = Anak(g, "CahayaSuar");
            var l = lGo.AddComponent<Light>(); l.type = LightType.Point; l.range = 12f; l.intensity = 2.6f; l.color = warna; l.shadows = LightShadows.None;
            list.Add(new MesinDanKincirHidup.SuarKedip { rend = bola.GetComponent<Renderer>(), lampu = l, warnaNyala = warna, frekuensi = frek, fasa = fasa });
        }

        static void TambahBenderaGantung(GameObject induk, string nama, Vector3 pos, Material kayu, Material kain, List<Transform> list)
        {
            var g = Anak(induk, nama); g.transform.position = pos;
            Hias(g, PrimitiveType.Cylinder, "Tiang", new Vector3(0f, 1.85f, 0f), new Vector3(0.07f, 1.85f, 0.07f), kayu, false);
            var pv = Anak(g, "KainPivot"); pv.transform.localPosition = new Vector3(0.08f, 3.45f, 0f);
            Hias(pv, PrimitiveType.Cube, "Kain", new Vector3(0.45f, -0.38f, 0f), new Vector3(0.88f, 0.72f, 0.03f), kain, false);
            list.Add(pv.transform);
        }

        static void TambahSangkarGantung(GameObject induk, string nama, Vector3 pos, Material kayu, List<Transform> list)
        {
            var g = Anak(induk, nama); g.transform.position = pos;
            Hias(g, PrimitiveType.Cylinder, "Tiang", new Vector3(0f, 1.45f, 0f), new Vector3(0.07f, 1.45f, 0.07f), kayu, false);
            Hias(g, PrimitiveType.Cube, "Palang", new Vector3(0.28f, 2.82f, 0f), new Vector3(0.68f, 0.07f, 0.07f), kayu, false);
            var pv = Anak(g, "GantunganPivot"); pv.transform.localPosition = new Vector3(0.56f, 2.78f, 0f);
            Hias(pv, PrimitiveType.Cylinder, "Sangkar", new Vector3(0f, -0.34f, 0f), new Vector3(0.32f, 0.28f, 0.32f), kayu, false);
            list.Add(pv.transform);
        }

        static void TambahKabutLokasi(GameObject induk, string nama, Vector3 pos, Vector3 skala, Color warna)
        {
            var sh = AmbilShaderAK("AlasKeramat/KabutLembut");
            if (sh == null) return;
            var m = new Material(sh) { name = "M_" + nama };
            SafeMaterialFactory.SetC(m, "_BaseColor", warna);
            SafeMaterialFactory.SetF(m, "_Mode", 0f);
            SafeMaterialFactory.SetF(m, "_SkalaNoise", 2.4f);
            SafeMaterialFactory.SetF(m, "_Kecepatan", 0.14f);
            m = SimpanMat(m);
            var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
            q.name = nama; Object.DestroyImmediate(q.GetComponent<Collider>());
            q.transform.SetParent(induk.transform, false);
            q.transform.position = pos; q.transform.rotation = Quaternion.Euler(90f, 0f, 0f); q.transform.localScale = skala;
            var mr = q.GetComponent<MeshRenderer>(); mr.sharedMaterial = m; mr.shadowCastingMode = ShadowCastingMode.Off; mr.receiveShadows = false;
        }
    }
}
