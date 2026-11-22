using System.Collections.Generic;
using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.EditorTools
{
    // Langkah 7z19: Overhaul Dunia Hidup & Review Lokasi V6 — memperkaya 12 lokasi kunci lintas pulau 4 km2
    // (Heli, Desa Mati, Pasar Hantu, Kampung Sukamaju, Altar Beringin, Pantai/Mercusuar/Dermaga, Candi, Pabrik Gula,
    // Menara Api, Kamp Peneliti, Taman Megalit, Mulut Gua) + 64 satwa hidup 4 kuadran + aktivitas hidup karakter +
    // jendela malam menyala, jemuran batik, lentera gantung, & perahu tambat berombak.
    public static partial class NusantaraMasterBuilder
    {
        public static int JumlahSatwaHidupV6, JumlahElemenHidupV6, JumlahKarakterHidupV6, JumlahPropLokasiV6;

        static void LangkahOverhaulDuniaHidupV6()
        {
            JumlahSatwaHidupV6 = 0; JumlahElemenHidupV6 = 0; JumlahKarakterHidupV6 = 0; JumlahPropLokasiV6 = 0;
            var lm = GameObject.Find(NamaRoot + "/World/Landmark") ?? GameObject.Find(NamaRoot + "/Landmark") ?? _root;
            var induk = Anak(lm, "DuniaHidupV6");

            var kayu = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_KayuLapuk2K.mat") ?? AmbilMat("M_Kayu");
            var batu = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_BatuCandi2K.mat") ?? AmbilMat("M_Batu");
            var karat = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_KaratPabrik2K.mat") ?? AmbilMat("M_LogamKarat") ?? MatPolos("M_KaratV6", "#5E4030");
            var kainTex = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/kain.png");
            var kainNorm = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/kain_n.png");
            var terakota = SimpanMat(SafeMaterialFactory.CreatePBR("M_TempayanTanahLiat", new Color(0.64f, 0.34f, 0.21f), 0f, 0.22f, AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/tanah.png"), AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/tanah_n.png")));
            var jendelaMat = SimpanMat(SafeMaterialFactory.CreatePBR("M_JendelaLampuDesa", new Color(0.24f, 0.18f, 0.12f), 0f, 0.65f, null, null, new Color(0.05f, 0.03f, 0.01f)));
            var lenteraMat = SimpanMat(SafeMaterialFactory.CreatePBR("M_LenteraHangatV6", new Color(0.95f, 0.72f, 0.34f), 0f, 0.25f, null, null, new Color(2.8f, 1.55f, 0.48f)));
            var batikA = SimpanMat(SafeMaterialFactory.CreatePBR("M_BatikJemurSogan", new Color(0.68f, 0.42f, 0.20f), 0f, 0.14f, kainTex, kainNorm));
            var batikB = SimpanMat(SafeMaterialFactory.CreatePBR("M_BatikJemurDelima", new Color(0.62f, 0.18f, 0.20f), 0f, 0.14f, kainTex, kainNorm));
            var batikC = SimpanMat(SafeMaterialFactory.CreatePBR("M_BatikJemurIndigo", new Color(0.22f, 0.32f, 0.48f), 0f, 0.14f, kainTex, kainNorm));
            var daunKelapa = AmbilMat("M_DaunPohon") ?? MatPolos("M_DaunKelapaV6", "#2E5E26");
            var batangKelapa = AmbilMat("M_KulitPohon") ?? kayu;
            var apiMat = SimpanMat(SafeMaterialFactory.CreatePBR("M_ApiSesajenV6", new Color(1f, 0.62f, 0.18f), 0f, 0.1f, null, null, new Color(5.4f, 2.4f, 0.52f)));

            var jendelaList = new List<Renderer>();
            var lenteraList = new List<Light>();
            var kainList = new List<Transform>();
            var perahuList = new List<Transform>();

            PerkayaLokasiHeliDanDeden(induk, kayu, karat, batikA, lenteraMat, lenteraList, kainList);
            PerkayaDesaMatiPasarDanKampung(induk, kayu, terakota, jendelaMat, lenteraMat, new[] { batikA, batikB, batikC }, jendelaList, lenteraList, kainList);
            PerkayaAltarCandiMegalitDanPabrik(induk, batu, karat, terakota, apiMat, lenteraMat, lenteraList);
            PerkayaPantaiMercusuarDanDermaga(induk, kayu, batangKelapa, daunKelapa, lenteraMat, lenteraList, perahuList);

            var suasana = induk.AddComponent<SuasanaHidupDesaDanPantai>();
            suasana.Pasang(jendelaList.ToArray(), lenteraList.ToArray(), kainList.ToArray(), perahuList.ToArray());
            EditorUtility.SetDirty(suasana);
            JumlahElemenHidupV6 = suasana.JumlahElemenHidup;

            BangunEkosistemRimbaLengkap(induk);
            PasangAktivitasHidupKarakter(induk, kayu, kainTex, kainNorm);

            Catat("Overhaul Dunia Hidup V6: " + JumlahPropLokasiV6 + " properti 12 lokasi, " + JumlahElemenHidupV6
                + " elemen desa/pantai hidup, " + JumlahSatwaHidupV6 + " satwa 4 kuadran & " + JumlahKarakterHidupV6 + " karakter beraktivitas");
        }

        static void PerkayaLokasiHeliDanDeden(GameObject induk, Material kayu, Material karat, Material terpal, Material lenteraMat, List<Light> lenteraList, List<Transform> kainList)
        {
            var grup = Anak(induk, "DetailHeliDanPosDeden");
            // Bilah baling-baling patah & peti ekspedisi berserakan di sekitar bangkai heli (8, -448).
            Vector3[] posBilah = { new Vector3(13.5f, 0f, -451f), new Vector3(3.2f, 0f, -442f), new Vector3(11.8f, 0f, -439f) };
            for (int i = 0; i < posBilah.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(posBilah[i].x, posBilah[i].z);
                var b = Anak(grup, "BilahHeliPatah_" + i);
                b.transform.position = new Vector3(posBilah[i].x, y + 0.12f, posBilah[i].z);
                b.transform.rotation = Quaternion.Euler(4f + i * 3f, 28f + i * 62f, (i % 2 == 0 ? 11f : -9f));
                Hias(b, PrimitiveType.Cube, "Bilah", Vector3.zero, new Vector3(0.32f, 0.06f, 3.6f), karat, false);
                JumlahPropLokasiV6++;
            }
            // Peti kargo & kotak medis darurat ekspedisi sekitar pos selamat Deden.
            var matMedis = MatPolos("M_KotakMedisHeli", "#C84232");
            Vector2[] posPeti = { new Vector2(18.5f, -445f), new Vector2(21.2f, -436.5f), new Vector2(12.2f, -453.5f), new Vector2(6.5f, -438.5f) };
            for (int i = 0; i < posPeti.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(posPeti[i].x, posPeti[i].y);
                var p = Anak(grup, "PetiEkspedisiHeli_" + i);
                p.transform.position = new Vector3(posPeti[i].x, y, posPeti[i].y);
                p.transform.rotation = Quaternion.Euler(0f, i * 47f + 15f, 0f);
                Hias(p, PrimitiveType.Cube, "Kotak", new Vector3(0f, 0.28f, 0f), new Vector3(0.85f, 0.56f, 0.58f), i % 2 == 0 ? kayu : matMedis, true);
                JumlahPropLokasiV6++;
            }
            // Tenda terpal darurat + lentera gantung hangat di sisi pos Kang Deden (26.5, -442.5) tanpa menghalangi kamera potret.
            float tx = 26.8f, tz = -442.8f, ty = WorldTerrainGenerator.SampleHeight(tx, tz);
            var posD = Anak(grup, "NaunganDaruratDeden");
            posD.transform.position = new Vector3(tx, ty, tz);
            posD.transform.rotation = Quaternion.Euler(0f, -35f, 0f);
            Hias(posD, PrimitiveType.Cylinder, "TiangA", new Vector3(-1.3f, 1.15f, -1.0f), new Vector3(0.08f, 1.15f, 0.08f), kayu, false);
            Hias(posD, PrimitiveType.Cylinder, "TiangB", new Vector3(1.3f, 1.15f, -1.0f), new Vector3(0.08f, 1.15f, 0.08f), kayu, false);
            Hias(posD, PrimitiveType.Cylinder, "TiangC", new Vector3(-1.3f, 0.85f, 1.1f), new Vector3(0.08f, 0.85f, 0.08f), kayu, false);
            Hias(posD, PrimitiveType.Cylinder, "TiangD", new Vector3(1.3f, 0.85f, 1.1f), new Vector3(0.08f, 0.85f, 0.08f), kayu, false);
            var atapT = Hias(posD, PrimitiveType.Cube, "TerpalAtap", new Vector3(0f, 2.08f, 0.05f), new Vector3(2.9f, 0.05f, 2.5f), terpal, false);
            atapT.transform.localRotation = Quaternion.Euler(14f, 0f, 0f);
            kainList.Add(atapT.transform);
            BuatLenteraTiang(posD.transform, "LenteraPosDeden", new Vector3(-1.15f, 1.92f, -0.95f), lenteraMat, 2.4f, 11f, lenteraList, kainList);
            JumlahPropLokasiV6 += 2;
        }

        static void PerkayaDesaMatiPasarDanKampung(GameObject induk, Material kayu, Material terakota, Material jendelaMat,
            Material lenteraMat, Material[] batik, List<Renderer> jendelaList, List<Light> lenteraList, List<Transform> kainList)
        {
            var grup = Anak(induk, "KehidupanDesaDanPasar");

            // 1. Jendela hangat menyala malam + tempayan air + tumpukan kayu bakar di 6 rumah Desa Mati.
            float[,] desa = { { 564, 224, 200 }, { 524, 236, 180 }, { 592, 176, 260 }, { 544, 140, 330 }, { 504, 200, 120 }, { 580, 260, 210 } };
            for (int i = 0; i < desa.GetLength(0); i++)
            {
                float x = desa[i, 0], z = desa[i, 1], yaw = desa[i, 2];
                float y = WorldTerrainGenerator.SampleHeight(x, z);
                var h = Anak(grup, "HidupRumahDesa_" + i);
                h.transform.position = new Vector3(x, y, z);
                h.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

                // 2 panel jendela bercahaya senja/malam pada sisi depan & samping rumah panggung.
                var j1 = Hias(h, PrimitiveType.Quad, "JendelaDepan", new Vector3(0f, 3.05f, 3.15f), new Vector3(1.15f, 0.95f, 1f), jendelaMat, false);
                var j2 = Hias(h, PrimitiveType.Quad, "JendelaSamping", new Vector3(3.62f, 3.05f, 0f), new Vector3(1.10f, 0.90f, 1f), jendelaMat, false);
                j2.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                jendelaList.Add(j1.GetComponent<Renderer>());
                jendelaList.Add(j2.GetComponent<Renderer>());

                // Lentera teras rumah yang otomatis menyala saat senja/malam.
                if (i < 4) BuatLenteraTiang(h.transform, "LenteraTeras_" + i, new Vector3(1.8f, 2.55f, 3.45f), lenteraMat, 2.8f, 13f, lenteraList, kainList);

                // Tempayan tanah liat & tumpukan kayu bakar di bawah kolong/teras.
                Hias(h, PrimitiveType.Sphere, "Tempayan_" + i, new Vector3(-2.6f, 0.42f, 3.2f), new Vector3(0.58f, 0.72f, 0.58f), terakota, false);
                Hias(h, PrimitiveType.Cylinder, "LeherTempayan", new Vector3(-2.6f, 0.78f, 3.2f), new Vector3(0.32f, 0.16f, 0.32f), terakota, false);
                for (int k = 0; k < 3; k++)
                {
                    var log = Hias(h, PrimitiveType.Cylinder, "KayuBakar_" + k, new Vector3(2.4f, 0.16f + k * 0.14f, 3.3f), new Vector3(0.14f, 0.58f, 0.14f), kayu, false);
                    log.transform.localRotation = Quaternion.Euler(0f, 12f * k, 90f);
                }
                JumlahPropLokasiV6 += 3;
            }

            // 2. 6 jemuran bambu berisi kain batik warna-warni berkibar di Desa Mati & Kampung Sukamaju.
            Vector3[] posJemur =
            {
                new Vector3(536f, 0f, 212f), new Vector3(556f, 0f, 186f), new Vector3(518f, 0f, 178f),
                new Vector3(62f, 0f, -382f), new Vector3(50f, 0f, -368f), new Vector3(578f, 0f, 574f)
            };
            for (int i = 0; i < posJemur.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(posJemur[i].x, posJemur[i].z);
                var jm = Anak(grup, "JemuranBatik_" + i);
                jm.transform.position = new Vector3(posJemur[i].x, y, posJemur[i].z);
                jm.transform.rotation = Quaternion.Euler(0f, i * 43f + 20f, 0f);
                Hias(jm, PrimitiveType.Cylinder, "TiangKiri", new Vector3(-1.8f, 1.15f, 0f), new Vector3(0.08f, 1.15f, 0.08f), kayu, false);
                Hias(jm, PrimitiveType.Cylinder, "TiangKanan", new Vector3(1.8f, 1.15f, 0f), new Vector3(0.08f, 1.15f, 0.08f), kayu, false);
                var palang = Hias(jm, PrimitiveType.Cylinder, "PalangBambu", new Vector3(0f, 2.22f, 0f), new Vector3(0.05f, 1.9f, 0.05f), kayu, false);
                palang.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                for (int c = 0; c < 3; c++)
                {
                    var pivot = Anak(jm, "KainBatikPivot_" + c);
                    pivot.transform.localPosition = new Vector3(-1.05f + c * 1.05f, 2.20f, 0f);
                    Hias(pivot, PrimitiveType.Cube, "SarungBatik", new Vector3(0f, -0.58f, 0f), new Vector3(0.72f, 1.12f, 0.03f), batik[(i + c) % batik.Length], false);
                    kainList.Add(pivot.transform);
                }
                JumlahPropLokasiV6++;
            }

            // 3. 8 lentera gantung & keranjang pasar di Pasar Hantu Desa (468, 178).
            for (int i = 0; i < 8; i++)
            {
                float ang = i * Mathf.PI * 0.25f;
                float px = 468f + Mathf.Cos(ang) * 9.5f, pz = 178f + Mathf.Sin(ang) * 9.5f;
                float py = WorldTerrainGenerator.SampleHeight(px, pz);
                var tiang = Anak(grup, "TiangLenteraPasar_" + i);
                tiang.transform.position = new Vector3(px, py, pz);
                tiang.transform.rotation = Quaternion.Euler(0f, -ang * Mathf.Rad2Deg, 0f);
                Hias(tiang, PrimitiveType.Cylinder, "Bambu", new Vector3(0f, 1.55f, 0f), new Vector3(0.07f, 1.55f, 0.07f), kayu, false);
                Hias(tiang, PrimitiveType.Cube, "Lengan", new Vector3(0f, 3.05f, 0.35f), new Vector3(0.08f, 0.08f, 0.78f), kayu, false);
                BuatLenteraTiang(tiang.transform, "LenteraPasar_" + i, new Vector3(0f, 2.82f, 0.68f), lenteraMat, i % 2 == 0 ? 2.4f : 0f, 11f, lenteraList, kainList);
                JumlahPropLokasiV6++;
            }

            // 4. Detail Kampung Sukamaju dekat (56, -376): bangku kayu, pagar bambu, tempayan, & lentera beranda.
            Vector2[] posKampung = { new Vector2(54f, -374f), new Vector2(61f, -378f), new Vector2(572f, 566f), new Vector2(598f, 592f) };
            for (int i = 0; i < posKampung.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(posKampung[i].x, posKampung[i].y);
                var kg = Anak(grup, "SudutKampung_" + i);
                kg.transform.position = new Vector3(posKampung[i].x, y, posKampung[i].y);
                kg.transform.rotation = Quaternion.Euler(0f, i * 65f, 0f);
                Hias(kg, PrimitiveType.Cube, "BangkuKayu", new Vector3(0f, 0.45f, 0f), new Vector3(1.65f, 0.10f, 0.46f), kayu, true);
                Hias(kg, PrimitiveType.Cube, "KakiBangkuL", new Vector3(-0.68f, 0.22f, 0f), new Vector3(0.12f, 0.44f, 0.40f), kayu, false);
                Hias(kg, PrimitiveType.Cube, "KakiBangkuR", new Vector3(0.68f, 0.22f, 0f), new Vector3(0.12f, 0.44f, 0.40f), kayu, false);
                Hias(kg, PrimitiveType.Sphere, "TempayanKampung", new Vector3(1.25f, 0.38f, 0.35f), new Vector3(0.52f, 0.66f, 0.52f), terakota, false);
                BuatLenteraTiang(kg.transform, "LenteraKampung_" + i, new Vector3(-1.1f, 2.1f, -0.4f), lenteraMat, 2.2f, 11f, lenteraList, kainList);
                Hias(kg, PrimitiveType.Cylinder, "TiangLentera", new Vector3(-1.1f, 1.15f, -0.4f), new Vector3(0.07f, 1.15f, 0.07f), kayu, false);
                JumlahPropLokasiV6 += 2;
            }
        }

        static void PerkayaAltarCandiMegalitDanPabrik(GameObject induk, Material batu, Material karat, Material terakota, Material apiMat, Material lenteraMat, List<Light> lenteraList)
        {
            var grup = Anak(induk, "SitusKeramatDanPabrik");
            var apiMesh = ProcGua.ApiTungku("ApiBaraAltarV6", 53);
            string pApi = FolderLandmark + "/ApiBaraAltarV6.asset"; AssetDatabase.DeleteAsset(pApi); AssetDatabase.CreateAsset(apiMesh, pApi);
            apiMesh = AssetDatabase.LoadAssetAtPath<Mesh>(pApi);
            var matAsap = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_AsapHeli.mat");
            var matBunga = MatPolos("M_SesajenBunga", "#D84248");
            var matKuning = MatPolos("M_SesajenKenanga", "#E2B834");

            // 1. 8 pedupa sesajen bunga & asap kemenyan mengelilingi Altar Beringin Agung (0, -14).
            for (int i = 0; i < 8; i++)
            {
                float ang = i * Mathf.PI * 0.25f;
                float x = Mathf.Cos(ang) * 11.5f, z = -12f + Mathf.Sin(ang) * 11.5f;
                float y = WorldTerrainGenerator.SampleHeight(x, z);
                var ped = Anak(grup, "PedupaSesajen_" + i);
                ped.transform.position = new Vector3(x, y, z);
                Hias(ped, PrimitiveType.Cylinder, "PilarBatu", new Vector3(0f, 0.52f, 0f), new Vector3(0.46f, 0.52f, 0.46f), batu, true);
                Hias(ped, PrimitiveType.Cylinder, "Cawan", new Vector3(0f, 1.08f, 0f), new Vector3(0.58f, 0.12f, 0.58f), terakota, false);
                Hias(ped, PrimitiveType.Sphere, "KembangA", new Vector3(-0.08f, 1.20f, 0.06f), new Vector3(0.22f, 0.10f, 0.22f), matBunga, false);
                Hias(ped, PrimitiveType.Sphere, "KembangB", new Vector3(0.10f, 1.20f, -0.06f), new Vector3(0.18f, 0.10f, 0.18f), matKuning, false);
                if (i % 2 == 0)
                {
                    var nyala = Anak(ped, "ApiPedupa");
                    nyala.transform.localPosition = new Vector3(0f, 1.18f, 0f);
                    nyala.transform.localScale = new Vector3(0.32f, 0.48f, 0.32f);
                    nyala.AddComponent<MeshFilter>().sharedMesh = apiMesh;
                    var nmr = nyala.AddComponent<MeshRenderer>(); nmr.sharedMaterial = apiMat; nmr.shadowCastingMode = ShadowCastingMode.Off;
                    var lGo = Anak(ped, "CahayaPedupa"); lGo.transform.localPosition = new Vector3(0f, 1.65f, 0f);
                    var l = lGo.AddComponent<Light>(); l.type = LightType.Point; l.range = 12f; l.intensity = 2.8f; l.color = new Color(1f, 0.62f, 0.22f); l.shadows = LightShadows.None;
                    lenteraList.Add(l);
                    if (matAsap != null)
                    {
                        var dup = Anak(ped, "AsapKemenyan"); dup.transform.localPosition = new Vector3(0f, 1.32f, 0f);
                        var ps = dup.AddComponent<ParticleSystem>();
                        var pm = ps.main; pm.startLifetime = 3.2f; pm.startSpeed = 0.42f; pm.startSize = 0.55f; pm.startColor = new Color(0.86f, 0.88f, 0.92f, 0.28f); pm.maxParticles = 14; pm.prewarm = true;
                        var pe = ps.emission; pe.rateOverTime = 4f;
                        var pr = ps.GetComponent<ParticleSystemRenderer>(); pr.sharedMaterial = matAsap; pr.shadowCastingMode = ShadowCastingMode.Off;
                    }
                }
                JumlahPropLokasiV6++;
            }

            // 2. Candi Runtuh (-265, 115) & Taman Megalit (210, 420): 6 pilar lentera batu lumut.
            Vector2[] posCandi = { new Vector2(-254f, 108f), new Vector2(-276f, 118f), new Vector2(-262f, 128f), new Vector2(202f, 416f), new Vector2(218f, 424f), new Vector2(210f, 434f) };
            for (int i = 0; i < posCandi.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(posCandi[i].x, posCandi[i].y);
                var pil = Anak(grup, "LenteraBatuKuno_" + i);
                pil.transform.position = new Vector3(posCandi[i].x, y, posCandi[i].y);
                Hias(pil, PrimitiveType.Cube, "Alas", new Vector3(0f, 0.25f, 0f), new Vector3(0.85f, 0.50f, 0.85f), batu, true);
                Hias(pil, PrimitiveType.Cylinder, "Batang", new Vector3(0f, 1.05f, 0f), new Vector3(0.42f, 0.65f, 0.42f), batu, true);
                Hias(pil, PrimitiveType.Cube, "BilikCahaya", new Vector3(0f, 1.85f, 0f), new Vector3(0.46f, 0.42f, 0.46f), lenteraMat, false);
                Hias(pil, PrimitiveType.Cube, "MahkotaAtap", new Vector3(0f, 2.22f, 0f), new Vector3(0.82f, 0.26f, 0.82f), batu, false);
                if (i % 2 == 0)
                {
                    var lGo = Anak(pil, "CahayaBatu"); lGo.transform.localPosition = new Vector3(0f, 1.92f, 0f);
                    var l = lGo.AddComponent<Light>(); l.type = LightType.Point; l.range = 12f; l.intensity = 2.4f; l.color = new Color(1f, 0.68f, 0.28f); l.shadows = LightShadows.None;
                    lenteraList.Add(l);
                }
                JumlahPropLokasiV6++;
            }

            // 3. Pabrik Gula (665, -500), Menara Pengawas (-195, -240) & Kamp Peneliti (-390, 310): pipa berkarat & lentera suar.
            Vector2[] posIndustri = { new Vector2(654f, -514f), new Vector2(678f, -496f), new Vector2(-192f, -242f), new Vector2(-388f, 308f) };
            for (int i = 0; i < posIndustri.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(posIndustri[i].x, posIndustri[i].y);
                var ti = Anak(grup, "TiangSuarLokasi_" + i);
                ti.transform.position = new Vector3(posIndustri[i].x, y, posIndustri[i].y);
                Hias(ti, PrimitiveType.Cylinder, "TiangBesi", new Vector3(0f, 1.75f, 0f), new Vector3(0.14f, 1.75f, 0.14f), karat, true);
                Hias(ti, PrimitiveType.Sphere, "LampuSuar", new Vector3(0f, 3.62f, 0f), Vector3.one * 0.34f, lenteraMat, false);
                var lGo = Anak(ti, "CahayaSuar"); lGo.transform.localPosition = new Vector3(0f, 3.65f, 0f);
                var l = lGo.AddComponent<Light>(); l.type = LightType.Point; l.range = 14f; l.intensity = 2.6f; l.color = new Color(1f, 0.76f, 0.38f); l.shadows = LightShadows.None;
                lenteraList.Add(l);
                JumlahPropLokasiV6++;
            }

            // 4. Kabut mistis lembut di mulut gua UG1 (256, -276) & UG3 (0, 694).
            var shKabut = AmbilShaderAK("AlasKeramat/KabutLembut");
            if (shKabut != null)
            {
                var matGua = new Material(shKabut) { name = "M_KabutMulutGuaV6" };
                SafeMaterialFactory.SetC(matGua, "_BaseColor", new Color(0.45f, 0.82f, 0.84f, 0.24f));
                SafeMaterialFactory.SetF(matGua, "_Mode", 0f);
                SafeMaterialFactory.SetF(matGua, "_SkalaNoise", 2.8f);
                SafeMaterialFactory.SetF(matGua, "_Kecepatan", 0.18f);
                matGua = SimpanMat(matGua);
                Vector2[] posGua = { new Vector2(256f, -276f), new Vector2(0f, 694f), new Vector2(324f, -158f) };
                for (int i = 0; i < posGua.Length; i++)
                {
                    float y = WorldTerrainGenerator.SampleHeight(posGua[i].x, posGua[i].y);
                    var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    q.name = "KabutGuaV6_" + i; Object.DestroyImmediate(q.GetComponent<Collider>());
                    q.transform.SetParent(grup.transform, false);
                    q.transform.position = new Vector3(posGua[i].x, y + 1.8f, posGua[i].y);
                    q.transform.localScale = new Vector3(7.5f, 4.5f, 1f);
                    var mr = q.GetComponent<MeshRenderer>(); mr.sharedMaterial = matGua; mr.shadowCastingMode = ShadowCastingMode.Off; mr.receiveShadows = false;
                    JumlahPropLokasiV6++;
                }
            }
        }

        static void PerkayaPantaiMercusuarDanDermaga(GameObject induk, Material kayu, Material batangKelapa, Material daunKelapa, Material lenteraMat, List<Light> lenteraList, List<Transform> perahuList)
        {
            var grup = Anak(induk, "KehidupanPantaiDanDermaga");
            var perahuMesh = AssetDatabase.LoadAssetAtPath<Mesh>(FolderLandmark + "/PerahuNelayanRusak.asset") ?? ProcBangunan.PerahuNelayanRusak();
            if (AssetDatabase.GetAssetPath(perahuMesh) == "") perahuMesh = Simpan(perahuMesh);

            // 1. 6 perahu nelayan tambat mengayun di ombak Pantai Selatan, Dermaga Barat & Desa Nelayan Timur.
            Vector3[] posPerahu =
            {
                new Vector3(-138f, 0.08f, -882f), new Vector3(-118f, 0.08f, -884f),
                new Vector3(-526f, 0.08f, -356f), new Vector3(-512f, 0.08f, -362f),
                new Vector3(742f, 0.08f, -148f), new Vector3(754f, 0.08f, -132f)
            };
            for (int i = 0; i < posPerahu.Length; i++)
            {
                var p = Anak(grup, "PerahuTambatOmbak_" + i);
                p.transform.position = posPerahu[i];
                p.transform.rotation = Quaternion.Euler(0f, i * 52f + 18f, 0f);
                p.AddComponent<MeshFilter>().sharedMesh = perahuMesh;
                p.AddComponent<MeshRenderer>().sharedMaterials = new[] { kayu, kayu };
                perahuList.Add(p.transform);
                JumlahPropLokasiV6++;
            }

            // 2. 8 pohon kelapa pantai condong ke laut + kayu apung alami sepanjang pantai selatan & barat.
            Vector2[] posKelapa =
            {
                new Vector2(-168f, -864f), new Vector2(-152f, -858f), new Vector2(-102f, -854f), new Vector2(-82f, -858f),
                new Vector2(-210f, -866f), new Vector2(-506f, -334f), new Vector2(708f, -132f), new Vector2(732f, -154f)
            };
            for (int i = 0; i < posKelapa.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(posKelapa[i].x, posKelapa[i].y);
                var klp = Anak(grup, "KelapaPantai_" + i);
                klp.transform.position = new Vector3(posKelapa[i].x, y - 0.15f, posKelapa[i].y);
                klp.transform.rotation = Quaternion.Euler(14f + (i % 3) * 4f, i * 47f + 165f, 0f);
                Hias(klp, PrimitiveType.Cylinder, "Batang", new Vector3(0f, 3.8f, 0f), new Vector3(0.36f, 3.8f, 0.36f), batangKelapa, true);
                for (int d = 0; d < 6; d++)
                {
                    float a = d * 60f;
                    var pelepah = Hias(klp, PrimitiveType.Cube, "Pelepah_" + d, new Vector3(Mathf.Sin(a * Mathf.Deg2Rad) * 1.45f, 7.45f, Mathf.Cos(a * Mathf.Deg2Rad) * 1.45f), new Vector3(0.68f, 0.08f, 3.1f), daunKelapa, false);
                    pelepah.transform.localRotation = Quaternion.Euler(26f, a, 0f);
                }
                JumlahPropLokasiV6++;
            }

            // 3. Lentera ujung dermaga (Dermaga Pantai, Dermaga Barat, Desa Nelayan Timur).
            Vector2[] posDermaga = { new Vector2(-130f, -876f), new Vector2(-520f, -360f), new Vector2(730f, -142f) };
            for (int i = 0; i < posDermaga.Length; i++)
            {
                float y = Mathf.Max(1.1f, WorldTerrainGenerator.SampleHeight(posDermaga[i].x, posDermaga[i].y));
                var d = Anak(grup, "LenteraDermaga_" + i);
                d.transform.position = new Vector3(posDermaga[i].x, y, posDermaga[i].y);
                Hias(d, PrimitiveType.Cylinder, "Tiang", new Vector3(0f, 1.45f, 0f), new Vector3(0.11f, 1.45f, 0.11f), kayu, false);
                Hias(d, PrimitiveType.Sphere, "Lentera", new Vector3(0f, 3.0f, 0f), Vector3.one * 0.32f, lenteraMat, false);
                var lGo = Anak(d, "CahayaDermaga"); lGo.transform.localPosition = new Vector3(0f, 3.05f, 0f);
                var l = lGo.AddComponent<Light>(); l.type = LightType.Point; l.range = 14f; l.intensity = 2.8f; l.color = new Color(1f, 0.72f, 0.32f); l.shadows = LightShadows.None;
                lenteraList.Add(l);
                JumlahPropLokasiV6++;
            }
        }

        static void BuatLenteraTiang(Transform induk, string nama, Vector3 lokalPos, Material lenteraMat, float intensitas, float range, List<Light> lenteraList, List<Transform> kainList)
        {
            var g = Anak(induk.gameObject, nama);
            g.transform.localPosition = lokalPos;
            Hias(g, PrimitiveType.Cylinder, "BodiLentera", new Vector3(0f, -0.18f, 0f), new Vector3(0.18f, 0.18f, 0.18f), lenteraMat, false);
            if (kainList != null) kainList.Add(g.transform);
            if (intensitas > 0.01f && lenteraList != null)
            {
                var lGo = Anak(g, "Cahaya"); lGo.transform.localPosition = new Vector3(0f, -0.22f, 0f);
                var l = lGo.AddComponent<Light>();
                l.type = LightType.Point; l.range = range; l.intensity = intensitas; l.color = new Color(1f, 0.68f, 0.28f); l.shadows = LightShadows.None;
                lenteraList.Add(l);
            }
        }

        static void BangunEkosistemRimbaLengkap(GameObject induk)
        {
            var go = Anak(induk, "EkosistemRimbaLengkapV6");
            var matRusa = MatPolos("M_RusaV6", "#8B5E3C");
            var matTanduk = MatPolos("M_TandukRusaV6", "#D2C4A8");
            var matAyamA = MatPolos("M_AyamKampungA", "#B86E2E");
            var matAyamB = MatPolos("M_AyamKampungB", "#D8C8AE");
            var matJengger = MatPolos("M_JenggerAyam", "#C82824");
            var matBabi = MatPolos("M_BabiHutanV6", "#4A3A30");
            var matRangkong = MatPolos("M_RangkongV6", "#1C1A18");
            var matParuhRangkong = MatPolos("M_ParuhRangkong", "#E09A24");
            var matCamar = MatPolos("M_CamarPantaiV6", "#E4E6E8");

            var daftar = new List<EkosistemRimbaLengkap.AnggotaSatwa>();

            // 1. 18 Rusa merumput & kabur tersebar di 6 lembah/padang rimba 4 kuadran.
            Vector2[] zonaRusa =
            {
                new Vector2(36f, -412f), new Vector2(180f, -260f), new Vector2(-140f, -190f),
                new Vector2(-240f, 150f), new Vector2(540f, 520f), new Vector2(-110f, 560f)
            };
            for (int i = 0; i < 18; i++)
            {
                var c = zonaRusa[i % zonaRusa.Length];
                float x = c.x + Mathf.Cos(i * 1.4f) * 14f, z = c.y + Mathf.Sin(i * 1.4f) * 14f;
                float y = Mathf.Max(0.5f, WorldTerrainGenerator.SampleHeight(x, z));
                var r = Anak(go, "RusaHidup_" + i);
                r.transform.position = new Vector3(x, y, z);
                Hias(r, PrimitiveType.Capsule, "Badan", new Vector3(0f, 0.92f, 0f), new Vector3(0.46f, 0.72f, 0.52f), matRusa, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                for (int k = 0; k < 4; k++)
                    Hias(r, PrimitiveType.Cylinder, "Kaki_" + k, new Vector3((k % 2 == 0 ? -0.18f : 0.18f), 0.44f, (k < 2 ? 0.36f : -0.36f)), new Vector3(0.09f, 0.44f, 0.09f), matRusa, false);
                var leher = Anak(r, "LeherKepala");
                leher.transform.localPosition = new Vector3(0f, 1.05f, 0.52f);
                Hias(leher, PrimitiveType.Cylinder, "Leher", new Vector3(0f, 0.24f, 0.08f), new Vector3(0.16f, 0.28f, 0.16f), matRusa, false).transform.localRotation = Quaternion.Euler(22f, 0f, 0f);
                Hias(leher, PrimitiveType.Sphere, "Kepala", new Vector3(0f, 0.46f, 0.22f), new Vector3(0.24f, 0.22f, 0.34f), matRusa, false);
                if (i % 2 == 0)
                {
                    Hias(leher, PrimitiveType.Cylinder, "TandukL", new Vector3(-0.10f, 0.68f, 0.16f), new Vector3(0.04f, 0.22f, 0.04f), matTanduk, false).transform.localRotation = Quaternion.Euler(-15f, 0f, 22f);
                    Hias(leher, PrimitiveType.Cylinder, "TandukR", new Vector3(0.10f, 0.68f, 0.16f), new Vector3(0.04f, 0.22f, 0.04f), matTanduk, false).transform.localRotation = Quaternion.Euler(-15f, 0f, -22f);
                }
                daftar.Add(new EkosistemRimbaLengkap.AnggotaSatwa
                {
                    tr = r.transform, kepala = leher.transform, jenis = EkosistemRimbaLengkap.JenisSatwa.RusaMerumput,
                    rumah = new Vector3(c.x, y, c.y), radius = 16f, laju = 0.24f + (i % 3) * 0.06f, fasa = i * 1.1f
                });
            }

            // 2. 12 Ayam kampung mematuk tanah di Kampung Sukamaju, Desa Mati & Desa Nelayan Timur.
            Vector2[] zonaAyam = { new Vector2(56f, -376f), new Vector2(534f, 206f), new Vector2(720f, -140f), new Vector2(582f, 578f) };
            for (int i = 0; i < 12; i++)
            {
                var c = zonaAyam[i % zonaAyam.Length];
                float x = c.x + Mathf.Cos(i * 1.9f) * 8f, z = c.y + Mathf.Sin(i * 1.9f) * 8f;
                float y = Mathf.Max(0.5f, WorldTerrainGenerator.SampleHeight(x, z));
                var ay = Anak(go, "AyamKampung_" + i);
                ay.transform.position = new Vector3(x, y, z);
                var mAy = i % 2 == 0 ? matAyamA : matAyamB;
                Hias(ay, PrimitiveType.Sphere, "Tubuh", new Vector3(0f, 0.28f, 0f), new Vector3(0.28f, 0.24f, 0.36f), mAy, false);
                Hias(ay, PrimitiveType.Cube, "Ekor", new Vector3(0f, 0.38f, -0.20f), new Vector3(0.10f, 0.18f, 0.16f), mAy, false).transform.localRotation = Quaternion.Euler(-35f, 0f, 0f);
                var kep = Anak(ay, "Kepala"); kep.transform.localPosition = new Vector3(0f, 0.38f, 0.18f);
                Hias(kep, PrimitiveType.Sphere, "Bulat", Vector3.zero, Vector3.one * 0.14f, mAy, false);
                Hias(kep, PrimitiveType.Cube, "Jengger", new Vector3(0f, 0.09f, 0.01f), new Vector3(0.04f, 0.08f, 0.10f), matJengger, false);
                daftar.Add(new EkosistemRimbaLengkap.AnggotaSatwa
                {
                    tr = ay.transform, kepala = kep.transform, jenis = EkosistemRimbaLengkap.JenisSatwa.AyamKampung,
                    rumah = new Vector3(c.x, y, c.y), radius = 10f, laju = 0.42f + (i % 3) * 0.08f, fasa = i * 1.7f
                });
            }

            // 3. 10 Babi hutan mencari makan di pinggir sungai, hutan jati & sawit.
            Vector2[] zonaBabi = { new Vector2(280f, -210f), new Vector2(-460f, 220f), new Vector2(120f, -310f), new Vector2(-360f, 290f), new Vector2(215f, 395f) };
            for (int i = 0; i < 10; i++)
            {
                var c = zonaBabi[i % zonaBabi.Length];
                float x = c.x + Mathf.Cos(i * 1.6f) * 11f, z = c.y + Mathf.Sin(i * 1.6f) * 11f;
                float y = Mathf.Max(0.5f, WorldTerrainGenerator.SampleHeight(x, z));
                var bb = Anak(go, "BabiHutanHidup_" + i);
                bb.transform.position = new Vector3(x, y, z);
                Hias(bb, PrimitiveType.Capsule, "Badan", new Vector3(0f, 0.48f, 0f), new Vector3(0.52f, 0.56f, 0.48f), matBabi, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                var kep = Anak(bb, "Moncong"); kep.transform.localPosition = new Vector3(0f, 0.46f, 0.46f);
                Hias(kep, PrimitiveType.Sphere, "Kepala", Vector3.zero, new Vector3(0.32f, 0.28f, 0.36f), matBabi, false);
                Hias(kep, PrimitiveType.Cylinder, "TaringL", new Vector3(-0.12f, -0.04f, 0.18f), new Vector3(0.03f, 0.12f, 0.03f), matTanduk, false).transform.localRotation = Quaternion.Euler(35f, 0f, 25f);
                Hias(kep, PrimitiveType.Cylinder, "TaringR", new Vector3(0.12f, -0.04f, 0.18f), new Vector3(0.03f, 0.12f, 0.03f), matTanduk, false).transform.localRotation = Quaternion.Euler(35f, 0f, -25f);
                daftar.Add(new EkosistemRimbaLengkap.AnggotaSatwa
                {
                    tr = bb.transform, kepala = kep.transform, jenis = EkosistemRimbaLengkap.JenisSatwa.BabiMencariMakan,
                    rumah = new Vector3(c.x, y, c.y), radius = 14f, laju = 0.28f + (i % 3) * 0.05f, fasa = i * 2.1f
                });
            }

            // 4. 24 Burung Rangkong kanopi & Camar pantai terbang mengepakkan sayap di atas landmark & pantai.
            Vector2[] zonaBurung =
            {
                new Vector2(8f, -440f), new Vector2(0f, 0f), new Vector2(540f, 204f), new Vector2(312f, -165f),
                new Vector2(-265f, 115f), new Vector2(-140f, -860f), new Vector2(-520f, -340f), new Vector2(720f, -140f)
            };
            for (int i = 0; i < 24; i++)
            {
                int zi = i % zonaBurung.Length;
                bool camar = zi >= 5;
                var c = zonaBurung[zi];
                float y = WorldTerrainGenerator.SampleHeight(c.x, c.y) + (camar ? 12f : 20f);
                var bg = Anak(go, (camar ? "CamarPantai_" : "RangkongKanopi_") + i);
                bg.transform.position = new Vector3(c.x, y, c.y);
                var mB = camar ? matCamar : matRangkong;
                Hias(bg, PrimitiveType.Capsule, "Tubuh", Vector3.zero, new Vector3(0.24f, 0.48f, 0.24f), mB, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Hias(bg, PrimitiveType.Cube, "Paruh", new Vector3(0f, 0.04f, 0.56f), new Vector3(0.10f, 0.14f, 0.34f), matParuhRangkong, false);
                var sKiri = Anak(bg, "SayapKiri"); sKiri.transform.localPosition = new Vector3(-0.14f, 0f, 0f);
                Hias(sKiri, PrimitiveType.Cube, "BuluKiri", new Vector3(-0.62f, 0f, 0f), new Vector3(1.24f, 0.04f, 0.42f), mB, false);
                var sKanan = Anak(bg, "SayapKanan"); sKanan.transform.localPosition = new Vector3(0.14f, 0f, 0f);
                Hias(sKanan, PrimitiveType.Cube, "BuluKanan", new Vector3(0.62f, 0f, 0f), new Vector3(1.24f, 0.04f, 0.42f), mB, false);
                daftar.Add(new EkosistemRimbaLengkap.AnggotaSatwa
                {
                    tr = bg.transform, sayapKiri = sKiri.transform, sayapKanan = sKanan.transform,
                    jenis = camar ? EkosistemRimbaLengkap.JenisSatwa.CamarPantai : EkosistemRimbaLengkap.JenisSatwa.RangkongKanopi,
                    rumah = new Vector3(c.x, y, c.y), radius = 24f + (i % 4) * 8f,
                    laju = (i % 2 == 0 ? 1f : -1f) * (0.22f + (i % 3) * 0.06f), fasa = i * 0.85f
                });
            }

            var comp = go.AddComponent<EkosistemRimbaLengkap>();
            comp.Pasang(daftar.ToArray());
            EditorUtility.SetDirty(comp);
            JumlahSatwaHidupV6 = comp.JumlahSatwa;
        }

        static void PasangAktivitasHidupKarakter(GameObject induk, Material kayu, Texture2D kainTex, Texture2D kainNorm)
        {
            // 1. Pasang aktivitas hidup alami pada Kang Deden, Ningsih, dan Ki Rogo.
            var deden = GameObject.Find("KangDeden");
            if (deden != null)
            {
                var ak = deden.GetComponent<AktivitasHidupKarakter>() ?? deden.AddComponent<AktivitasHidupKarakter>();
                ak.Pasang(AktivitasHidupKarakter.Rutinitas.PenyintasApi, new Vector3(15.5f, WorldTerrainGenerator.SampleHeight(15.5f, -444.5f) + 0.5f, -444.5f), true, 0.4f);
                EditorUtility.SetDirty(ak); JumlahKarakterHidupV6++;
            }
            var ningsih = GameObject.Find("Ningsih");
            if (ningsih != null)
            {
                var ak = ningsih.GetComponent<AktivitasHidupKarakter>() ?? ningsih.AddComponent<AktivitasHidupKarakter>();
                ak.Pasang(AktivitasHidupKarakter.Rutinitas.PenjagaTenang, Vector3.zero, false, 1.7f);
                EditorUtility.SetDirty(ak); JumlahKarakterHidupV6++;
            }
            var kiRogo = Object.FindFirstObjectByType<KiRogoPanggung>(FindObjectsInactive.Include);
            if (kiRogo != null)
            {
                var ak = kiRogo.GetComponent<AktivitasHidupKarakter>() ?? kiRogo.gameObject.AddComponent<AktivitasHidupKarakter>();
                ak.Pasang(AktivitasHidupKarakter.Rutinitas.TetuaRitual, new Vector3(0f, WorldTerrainGenerator.SampleHeight(0f, -18f) + 1f, -18f), true, 2.9f);
                EditorUtility.SetDirty(ak); JumlahKarakterHidupV6++;
            }

            // 2. 6 pemuja sekte beraktivitas ritual mengelilingi tungku alun-alun Desa Mati (516, 198).
            var grup = Anak(induk, "PemujaRitualDesaMati");
            var jubah = SimpanMat(SafeMaterialFactory.CreatePBR("M_JubahPemujaV6", new Color(0.38f, 0.22f, 0.16f), 0f, 0.14f, kainTex, kainNorm));
            var topeng = SimpanMat(SafeMaterialFactory.CreatePBR("M_TopengPemujaV6", new Color(0.78f, 0.68f, 0.52f), 0f, 0.22f, AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/kayu.png"), AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/kayu_n.png")));
            Vector3 apiDesa = new Vector3(516f, WorldTerrainGenerator.SampleHeight(516f, 198f) + 0.6f, 198f);
            for (int i = 0; i < 6; i++)
            {
                float ang = i * Mathf.PI / 3f + 0.25f;
                float x = 516f + Mathf.Cos(ang) * 4.8f, z = 198f + Mathf.Sin(ang) * 4.8f;
                float y = WorldTerrainGenerator.SampleHeight(x, z);
                var p = Anak(grup, "PemujaDesa_" + i);
                p.transform.position = new Vector3(x, y, z);
                p.transform.LookAt(new Vector3(516f, y, 198f));
                var tubuh = Anak(p, "Tubuh");
                Hias(tubuh, PrimitiveType.Cylinder, "Jubah", new Vector3(0f, 0.62f, 0f), new Vector3(0.46f, 0.62f, 0.42f), jubah, false);
                Hias(tubuh, PrimitiveType.Cube, "Bahu", new Vector3(0f, 1.36f, 0f), new Vector3(0.56f, 0.34f, 0.28f), jubah, false);
                var kep = Anak(tubuh, "Kepala"); kep.transform.localPosition = new Vector3(0f, 1.74f, 0f);
                Hias(kep, PrimitiveType.Sphere, "Tudung", Vector3.zero, new Vector3(0.30f, 0.32f, 0.30f), jubah, false);
                Hias(kep, PrimitiveType.Cube, "Topeng", new Vector3(0f, -0.02f, 0.14f), new Vector3(0.21f, 0.26f, 0.06f), topeng, false);
                var lKiri = Anak(tubuh, "LeftArm"); lKiri.transform.localPosition = new Vector3(-0.32f, 1.36f, 0.12f);
                Hias(lKiri, PrimitiveType.Cylinder, "LenganL", new Vector3(0f, -0.22f, 0.14f), new Vector3(0.10f, 0.28f, 0.10f), jubah, false).transform.localRotation = Quaternion.Euler(52f, 0f, 0f);
                var lKanan = Anak(tubuh, "RightArm"); lKanan.transform.localPosition = new Vector3(0.32f, 1.36f, 0.12f);
                Hias(lKanan, PrimitiveType.Cylinder, "LenganR", new Vector3(0f, -0.22f, 0.14f), new Vector3(0.10f, 0.28f, 0.10f), jubah, false).transform.localRotation = Quaternion.Euler(52f, 0f, 0f);
                var ak = p.AddComponent<AktivitasHidupKarakter>();
                ak.Pasang(AktivitasHidupKarakter.Rutinitas.PemujaSekte, apiDesa, true, i * 1.15f);
                EditorUtility.SetDirty(ak);
                JumlahKarakterHidupV6++;
            }
        }
    }
}
