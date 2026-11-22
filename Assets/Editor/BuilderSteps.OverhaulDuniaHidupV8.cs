using System.Collections.Generic;
using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.EditorTools
{
    // Langkah 7z21: Overhaul Dunia Hidup V8 — Ekosistem Dinamis, Migrasi Harian, Interaksi Alam & 22 Tugu Jalur:
    // - KejadianHidupPulau (2 kawanan migrasi formasi-V 24 burung, 36 serbuk alam, 12 semak kaget)
    // - DinamikaKawananAlam (18 rusa/banteng migrasi pagi-siang-sore, 6 predator pengintai, 6 biawak/buaya berjemur)
    // - 24 TitikKehidupanInteraktif (10 sesajen keramat, 8 pohon kelapa/buah guncang, 6 api unggun penyintas)
    // - KeanekaragamanRimbaV8 (16 nuri/kakaktua kanopi, 32 kupu/lebah, 24 sulur lumut gantung, 12 kura-kura sungai)
    // - 22 Tugu Penunjuk Jalur Nusantara menghubungkan seluruh 22 landmark pulau 4 km2.
    public static partial class NusantaraMasterBuilder
    {
        public static int JumlahKejadianV8, JumlahKawananV8, JumlahTitikInteraktifV8, JumlahHayatiV8, JumlahTuguJalurV8;

        static void LangkahOverhaulDuniaHidupV8()
        {
            JumlahKejadianV8 = 0; JumlahKawananV8 = 0; JumlahTitikInteraktifV8 = 0; JumlahHayatiV8 = 0; JumlahTuguJalurV8 = 0;
            var lm = GameObject.Find(NamaRoot + "/World/Landmark") ?? GameObject.Find(NamaRoot + "/Landmark") ?? _root;
            var induk = Anak(lm, "DuniaHidupV8");

            var kayu = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_KayuLapuk2K.mat") ?? AmbilMat("M_Kayu");
            var batu = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_BatuCandi2K.mat") ?? AmbilMat("M_Batu");
            var daun = AmbilMat("M_DaunPohon") ?? MatPolos("M_DaunV8", "#2E6A26");
            var kulitPohon = AmbilMat("M_KulitPohon") ?? kayu;
            var apiMat = SimpanMat(SafeMaterialFactory.CreatePBR("M_ApiHidupV8", new Color(1f, 0.60f, 0.16f), 0f, 0.1f, null, null, new Color(5.4f, 2.3f, 0.50f)));

            BangunKejadianHidupPulauV8(induk);
            BangunDinamikaKawananAlamV8(induk);
            BangunTitikKehidupanInteraktifV8(induk, kayu, batu, kulitPohon, daun, apiMat);
            BangunKeanekaragamanRimbaV8(induk, kayu, daun);
            Bangun22TuguPenunjukJalurV8(induk, batu, kayu, apiMat);

            Catat("Overhaul Dunia Hidup V8: " + JumlahKejadianV8 + " kejadian/migrasi, " + JumlahKawananV8
                + " kawanan predator-mangsa, " + JumlahTitikInteraktifV8 + " titik alam interaktif, "
                + JumlahHayatiV8 + " hayati tropis & " + JumlahTuguJalurV8 + " tugu penunjuk 22 lokasi");
        }

        static void BangunKejadianHidupPulauV8(GameObject induk)
        {
            var go = Anak(induk, "KejadianHidupPulauV8");
            var matMigrasi = MatPolos("M_BurungMigrasiV8", "#2B2622");
            var matSayap = MatPolos("M_SayapMigrasiV8", "#4A3C30");
            var matSerbuk = SimpanMat(SafeMaterialFactory.CreatePBR("M_SerbukEmasV8", new Color(1f, 0.92f, 0.52f), 0f, 0.2f, null, null, new Color(1.8f, 1.45f, 0.45f)));
            var matKaget = MatPolos("M_BurungKagetV8", "#C84B28");

            // 1. 2 Kawanan migrasi formasi-V (masing-masing 12 burung = 24 ekor) melintasi seluruh pulau.
            var kawanan = new KejadianHidupPulau.KawananMigrasi[2];
            Vector3[] asal = { new Vector3(-760f, 0f, -680f), new Vector3(720f, 0f, -520f) };
            Vector3[] tujuan = { new Vector3(740f, 0f, 680f), new Vector3(-720f, 0f, 640f) };
            for (int k = 0; k < 2; k++)
            {
                var fl = Anak(go, "KawananMigrasiV_" + k);
                fl.transform.position = asal[k];
                var sk = new Transform[12];
                var sn = new Transform[12];
                for (int b = 0; b < 12; b++)
                {
                    int baris = (b + 1) / 2;
                    float sisi = b == 0 ? 0f : (b % 2 == 1 ? -1f : 1f);
                    var br = Anak(fl, "BurungV_" + b);
                    br.transform.localPosition = new Vector3(sisi * baris * 2.4f, (b % 3) * 0.4f, -baris * 2.1f);
                    Hias(br, PrimitiveType.Capsule, "Tubuh", Vector3.zero, new Vector3(0.22f, 0.42f, 0.22f), matMigrasi, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    var sL = Anak(br, "SayapL"); sL.transform.localPosition = new Vector3(-0.12f, 0f, 0f);
                    Hias(sL, PrimitiveType.Cube, "BuluL", new Vector3(-0.56f, 0f, 0f), new Vector3(1.12f, 0.04f, 0.36f), matSayap, false);
                    var sR = Anak(br, "SayapR"); sR.transform.localPosition = new Vector3(0.12f, 0f, 0f);
                    Hias(sR, PrimitiveType.Cube, "BuluR", new Vector3(0.56f, 0f, 0f), new Vector3(1.12f, 0.04f, 0.36f), matSayap, false);
                    sk[b] = sL.transform; sn[b] = sR.transform;
                }
                kawanan[k] = new KejadianHidupPulau.KawananMigrasi
                {
                    induk = fl.transform, sayapKiri = sk, sayapKanan = sn,
                    asal = asal[k], tujuan = tujuan[k], tinggi = 44f + k * 14f, laju = 0.011f + k * 0.004f, fasa = k * 0.48f
                };
            }

            // 2. 36 serbuk sari emas / kelopak rimba melayang lembut di udara sekitar pemain.
            var serbuk = new Transform[36];
            for (int i = 0; i < serbuk.Length; i++)
            {
                var sp = Anak(go, "SerbukAlam_" + i);
                var m = Hias(sp, PrimitiveType.Cube, "Butir", Vector3.zero, new Vector3(0.06f, 0.02f, 0.06f), matSerbuk, false);
                m.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                serbuk[i] = sp.transform;
            }

            // 3. 12 titik semak kaget lintas hutan: burung melesat terbang saat Andi mendekat.
            Vector2[] posSemak =
            {
                new Vector2(28f, -418f), new Vector2(88f, -356f), new Vector2(240f, -245f), new Vector2(338f, -152f),
                new Vector2(490f, 194f), new Vector2(180f, 388f), new Vector2(-132f, 588f), new Vector2(-248f, 126f),
                new Vector2(-368f, 288f), new Vector2(-175f, -225f), new Vector2(-490f, -320f), new Vector2(615f, -475f)
            };
            var semak = new KejadianHidupPulau.SemakKaget[posSemak.Length];
            for (int i = 0; i < posSemak.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(posSemak[i].x, posSemak[i].y) + 0.6f;
                var sm = Anak(go, "BurungSemakKaget_" + i);
                sm.transform.position = new Vector3(posSemak[i].x, y, posSemak[i].y);
                for (int c = -1; c <= 1; c++)
                {
                    Hias(sm, PrimitiveType.Sphere, "Ekor_" + c, new Vector3(c * 0.45f, Mathf.Abs(c) * 0.18f, -Mathf.Abs(c) * 0.35f), new Vector3(0.22f, 0.16f, 0.32f), matKaget, false);
                    Hias(sm, PrimitiveType.Cube, "Sayap_" + c, new Vector3(c * 0.45f, Mathf.Abs(c) * 0.18f, -Mathf.Abs(c) * 0.35f), new Vector3(0.88f, 0.03f, 0.24f), matKaget, false);
                }
                sm.SetActive(false);
                semak[i] = new KejadianHidupPulau.SemakKaget { tr = sm.transform, pusat = new Vector3(posSemak[i].x, y, posSemak[i].y), fasa = i * 1.3f };
            }

            var comp = go.AddComponent<KejadianHidupPulau>();
            comp.Pasang(kawanan, serbuk, semak);
            EditorUtility.SetDirty(comp);
            JumlahKejadianV8 = comp.JumlahElemenKejadian;
        }

        static void BangunDinamikaKawananAlamV8(GameObject induk)
        {
            var go = Anak(induk, "DinamikaKawananAlamV8");
            var matBanteng = MatPolos("M_BantengRimbaV8", "#2B2018");
            var matRusaMig = MatPolos("M_RusaMigrasiV8", "#94643E");
            var matTanduk = MatPolos("M_TandukV8", "#D6C8AE");
            var matHarimau = MatPolos("M_HarimauPatroliV8", "#D07C28");
            var matBiawak = MatPolos("M_BiawakJemurV8", "#3E4B32");

            // 1. 18 Herbivora (3 kawanan x 6 ekor) migrasi harian: Air Pagi -> Hutan Siang -> Padang Sore.
            Vector2[] airPagi = { new Vector2(288f, -192f), new Vector2(-225f, 82f), new Vector2(560f, 520f) };
            Vector2[] hutanSiang = { new Vector2(140f, -310f), new Vector2(-145f, 210f), new Vector2(360f, 460f) };
            Vector2[] padangSore = { new Vector2(48f, -396f), new Vector2(-285f, 155f), new Vector2(615f, 588f) };
            var herb = new DinamikaKawananAlam.HerbivoraMigrasi[18];
            for (int i = 0; i < 18; i++)
            {
                int k = i / 6;
                int slot = i % 6;
                bool banteng = k == 2 || slot == 0;
                Vector3 off = new Vector3(Mathf.Cos(slot * 1.05f) * 5.2f, 0f, Mathf.Sin(slot * 1.05f) * 5.2f);
                float sx = hutanSiang[k].x + off.x, sz = hutanSiang[k].y + off.z;
                float sy = Mathf.Max(0.5f, WorldTerrainGenerator.SampleHeight(sx, sz));
                var h = Anak(go, (banteng ? "BantengMigrasi_" : "RusaMigrasi_") + i);
                h.transform.position = new Vector3(sx, sy, sz);
                var mH = banteng ? matBanteng : matRusaMig;
                float sc = banteng ? 1.25f : 1.0f;
                Hias(h, PrimitiveType.Capsule, "Badan", new Vector3(0f, 0.96f * sc, 0f), new Vector3(0.54f * sc, 0.78f * sc, 0.56f * sc), mH, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                for (int f = 0; f < 4; f++)
                    Hias(h, PrimitiveType.Cylinder, "Kaki_" + f, new Vector3((f % 2 == 0 ? -0.20f : 0.20f) * sc, 0.46f * sc, (f < 2 ? 0.40f : -0.40f) * sc), new Vector3(0.10f * sc, 0.46f * sc, 0.10f * sc), mH, false);
                var kep = Anak(h, "Kepala"); kep.transform.localPosition = new Vector3(0f, 1.10f * sc, 0.58f * sc);
                Hias(kep, PrimitiveType.Sphere, "Moncong", new Vector3(0f, 0.22f * sc, 0.20f * sc), new Vector3(0.28f * sc, 0.25f * sc, 0.36f * sc), mH, false);
                Hias(kep, PrimitiveType.Cylinder, "TandukL", new Vector3(-0.14f * sc, 0.44f * sc, 0.16f * sc), new Vector3(0.045f, 0.24f, 0.045f), matTanduk, false).transform.localRotation = Quaternion.Euler(0f, 0f, 34f);
                Hias(kep, PrimitiveType.Cylinder, "TandukR", new Vector3(0.14f * sc, 0.44f * sc, 0.16f * sc), new Vector3(0.045f, 0.24f, 0.045f), matTanduk, false).transform.localRotation = Quaternion.Euler(0f, 0f, -34f);
                herb[i] = new DinamikaKawananAlam.HerbivoraMigrasi
                {
                    tr = h.transform, kepala = kep.transform,
                    titikAirPagi = new Vector3(airPagi[k].x, 0f, airPagi[k].y),
                    titikHutanSiang = new Vector3(hutanSiang[k].x, 0f, hutanSiang[k].y),
                    titikPadangSore = new Vector3(padangSore[k].x, 0f, padangSore[k].y),
                    offsetKawanan = off, laju = 1.65f + (slot % 3) * 0.22f, fasa = i * 1.15f
                };
            }

            // 2. 6 Predator rimba mengintai tepi kawanan pada senja/malam.
            Vector2[] posPred = { new Vector2(150f, -295f), new Vector2(-155f, 200f), new Vector2(380f, 450f), new Vector2(-360f, 290f), new Vector2(215f, 405f), new Vector2(-120f, 580f) };
            var pred = new DinamikaKawananAlam.PredatorPengintai[posPred.Length];
            for (int i = 0; i < posPred.Length; i++)
            {
                float y = Mathf.Max(0.5f, WorldTerrainGenerator.SampleHeight(posPred[i].x, posPred[i].y));
                var p = Anak(go, "HarimauPengintai_" + i);
                p.transform.position = new Vector3(posPred[i].x, y, posPred[i].y);
                Hias(p, PrimitiveType.Capsule, "Badan", new Vector3(0f, 0.65f, 0f), new Vector3(0.46f, 0.78f, 0.48f), matHarimau, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Hias(p, PrimitiveType.Sphere, "Kepala", new Vector3(0f, 0.72f, 0.74f), new Vector3(0.36f, 0.32f, 0.38f), matHarimau, false);
                Hias(p, PrimitiveType.Cylinder, "Ekor", new Vector3(0f, 0.60f, -0.82f), new Vector3(0.07f, 0.45f, 0.07f), matHarimau, false).transform.localRotation = Quaternion.Euler(-65f, 0f, 0f);
                pred[i] = new DinamikaKawananAlam.PredatorPengintai
                {
                    tr = p.transform, pusatPatroli = new Vector3(posPred[i].x, y, posPred[i].y),
                    radius = 22f + (i % 3) * 8f, laju = (i % 2 == 0 ? 1f : -1f) * (0.24f + (i % 3) * 0.05f), fasa = i * 1.4f
                };
            }

            // 3. 6 Biawak / Buaya berjemur di tepi sungai & pantai lalu meluncur ke air saat didekati/hujan.
            Vector2[] batuJemur = { new Vector2(304f, -170f), new Vector2(266f, -214f), new Vector2(-236f, 86f), new Vector2(-516f, -348f), new Vector2(736f, -144f), new Vector2(-132f, -864f) };
            var rep = new DinamikaKawananAlam.ReptilBerjemur[batuJemur.Length];
            for (int i = 0; i < batuJemur.Length; i++)
            {
                float y = Mathf.Max(0.25f, WorldTerrainGenerator.SampleHeight(batuJemur[i].x, batuJemur[i].y)) + 0.14f;
                var r = Anak(go, "ReptilBerjemur_" + i);
                r.transform.position = new Vector3(batuJemur[i].x, y, batuJemur[i].y);
                Hias(r, PrimitiveType.Capsule, "Tubuh", Vector3.zero, new Vector3(0.28f, 0.68f, 0.18f), matBiawak, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Hias(r, PrimitiveType.Cylinder, "Ekor", new Vector3(0f, 0f, -0.78f), new Vector3(0.11f, 0.48f, 0.09f), matBiawak, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Hias(r, PrimitiveType.Sphere, "Kepala", new Vector3(0f, 0.04f, 0.68f), new Vector3(0.20f, 0.14f, 0.30f), matBiawak, false);
                rep[i] = new DinamikaKawananAlam.ReptilBerjemur
                {
                    tr = r.transform, batuJemur = new Vector3(batuJemur[i].x, y, batuJemur[i].y),
                    titikAir = new Vector3(batuJemur[i].x + 3.8f, y - 0.55f, batuJemur[i].y - 3.2f), fasa = i * 1.1f
                };
            }

            var comp = go.AddComponent<DinamikaKawananAlam>();
            comp.Pasang(herb, pred, rep);
            EditorUtility.SetDirty(comp);
            JumlahKawananV8 = comp.JumlahSatwaDinamika;
        }

        static void BangunTitikKehidupanInteraktifV8(GameObject induk, Material kayu, Material batu, Material kulitPohon, Material daun, Material apiMat)
        {
            var grup = Anak(induk, "TitikKehidupanInteraktifV8");
            var buahMat = MatPolos("M_KelapaSegarV8", "#5A8A2E");
            var bungaMat = MatPolos("M_BungaSesajenV8", "#E0484C");

            // A. 10 Batu Sesajen & Sumur Keramat Interaktif lintas situs suci pulau.
            var sesajen = new[]
            {
                new { nama = "Altar Beringin", x = 5.5f, z = -15.5f }, new { nama = "Candi Runtuh", x = -258f, z = 110f },
                new { nama = "Taman Megalit", x = 214f, z = 416f }, new { nama = "Kuil Bambu", x = -114f, z = 614f },
                new { nama = "Desa Mati", x = 524f, z = 194f }, new { nama = "Pasar Hantu", x = 464f, z = 182f },
                new { nama = "Jembatan Batu", x = -236f, z = 92f }, new { nama = "Air Terjun", x = 316f, z = -162f },
                new { nama = "Gua Air Terjun", x = 324f, z = -154f }, new { nama = "Pulau Karang", x = -316f, z = -882f }
            };
            for (int i = 0; i < sesajen.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(sesajen[i].x, sesajen[i].z);
                var g = Anak(grup, "SesajenInteraktif_" + i);
                g.transform.position = new Vector3(sesajen[i].x, y, sesajen[i].z);
                g.layer = GameLayers.Interactable; g.tag = GameTags.Interactable;
                var bc = g.AddComponent<BoxCollider>(); bc.size = new Vector3(1.2f, 1.3f, 1.2f); bc.center = new Vector3(0f, 0.65f, 0f);
                Hias(g, PrimitiveType.Cylinder, "Pilar", new Vector3(0f, 0.45f, 0f), new Vector3(0.56f, 0.45f, 0.56f), batu, true);
                var cawan = Hias(g, PrimitiveType.Sphere, "Sesajen", new Vector3(0f, 0.96f, 0f), new Vector3(0.42f, 0.16f, 0.42f), bungaMat, false);
                var lGo = Anak(g, "CahayaDupa"); lGo.transform.localPosition = new Vector3(0f, 1.35f, 0f);
                var l = lGo.AddComponent<Light>(); l.type = LightType.Point; l.range = 8f; l.intensity = 1.4f; l.color = new Color(1f, 0.78f, 0.34f); l.shadows = LightShadows.None;
                var comp = g.AddComponent<TitikKehidupanInteraktif>();
                comp.Pasang(TitikKehidupanInteraktif.JenisTitik.SesajenKeramat, sesajen[i].nama, cawan.transform, l);
                EditorUtility.SetDirty(comp); JumlahTitikInteraktifV8++;
            }

            // B. 8 Pohon Kelapa & Buah Hidup yang Bisa Diguncang untuk Memanen Buah Segar.
            var pohon = new[]
            {
                new { nama = "Pantai Selatan", x = -112f, z = -852f }, new { nama = "Mercusuar", x = -176f, z = -864f },
                new { nama = "Desa Nelayan", x = 712f, z = -134f }, new { nama = "Dermaga Barat", x = -508f, z = -336f },
                new { nama = "Kampung Sukamaju", x = 64f, z = -370f }, new { nama = "Air Terjun", x = 298f, z = -176f },
                new { nama = "Perkebunan", x = 584f, z = 568f }, new { nama = "Rumah Pohon", x = 74f, z = -402f }
            };
            for (int i = 0; i < pohon.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(pohon[i].x, pohon[i].z);
                var g = Anak(grup, "PohonBuahGuncang_" + i);
                g.transform.position = new Vector3(pohon[i].x, y, pohon[i].z);
                g.layer = GameLayers.Interactable; g.tag = GameTags.Interactable;
                var bc = g.AddComponent<BoxCollider>(); bc.size = new Vector3(1.2f, 2.2f, 1.2f); bc.center = new Vector3(0f, 1.1f, 0f);
                var pivot = Anak(g, "BatangGoyang");
                Hias(pivot, PrimitiveType.Cylinder, "Batang", new Vector3(0f, 2.6f, 0f), new Vector3(0.32f, 2.6f, 0.32f), kulitPohon, true);
                for (int d = 0; d < 5; d++)
                {
                    float a = d * 72f;
                    var pl = Hias(pivot, PrimitiveType.Cube, "Daun_" + d, new Vector3(Mathf.Sin(a * Mathf.Deg2Rad) * 1.2f, 5.1f, Mathf.Cos(a * Mathf.Deg2Rad) * 1.2f), new Vector3(0.60f, 0.08f, 2.4f), daun, false);
                    pl.transform.localRotation = Quaternion.Euler(24f, a, 0f);
                }
                for (int b = 0; b < 3; b++)
                    Hias(pivot, PrimitiveType.Sphere, "Kelapa_" + b, new Vector3(Mathf.Cos(b * 2.1f) * 0.34f, 4.7f, Mathf.Sin(b * 2.1f) * 0.34f), Vector3.one * 0.28f, buahMat, false);
                var comp = g.AddComponent<TitikKehidupanInteraktif>();
                comp.Pasang(TitikKehidupanInteraktif.JenisTitik.PohonBuahGuncang, pohon[i].nama, pivot.transform, null);
                EditorUtility.SetDirty(comp); JumlahTitikInteraktifV8++;
            }

            // C. 6 Api Unggun Penyintas Interaktif di Pos Eksplorasi Utama.
            var api = new[]
            {
                new { nama = "Pos Heli", x = 18.5f, z = -442f }, new { nama = "Kampung Sukamaju", x = 50f, z = -378f },
                new { nama = "Menara Pengawas Api", x = -190f, z = -236f }, new { nama = "Kamp Peneliti", x = -384f, z = 314f },
                new { nama = "Pos Gunung Kapur", x = -576f, z = 612f }, new { nama = "Pabrik Gula", x = 662f, z = -492f }
            };
            var apiMesh = ProcGua.ApiTungku("ApiPenyintasV8", 61);
            string pApi = FolderLandmark + "/ApiPenyintasV8.asset"; AssetDatabase.DeleteAsset(pApi); AssetDatabase.CreateAsset(apiMesh, pApi);
            apiMesh = AssetDatabase.LoadAssetAtPath<Mesh>(pApi);
            for (int i = 0; i < api.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(api[i].x, api[i].z);
                var g = Anak(grup, "ApiPenyintas_" + i);
                g.transform.position = new Vector3(api[i].x, y, api[i].z);
                g.layer = GameLayers.Interactable; g.tag = GameTags.Interactable;
                var bc = g.AddComponent<BoxCollider>(); bc.size = new Vector3(1.5f, 1.1f, 1.5f); bc.center = new Vector3(0f, 0.5f, 0f);
                for (int r = 0; r < 6; r++)
                    Hias(g, PrimitiveType.Sphere, "Batu_" + r, new Vector3(Mathf.Cos(r * 1.05f) * 0.56f, 0.14f, Mathf.Sin(r * 1.05f) * 0.56f), new Vector3(0.28f, 0.20f, 0.28f), batu, false);
                Hias(g, PrimitiveType.Cylinder, "BangkuLog", new Vector3(1.35f, 0.22f, 0f), new Vector3(0.32f, 0.85f, 0.32f), kayu, true).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                var nyala = Anak(g, "Nyala"); nyala.transform.localPosition = new Vector3(0f, 0.16f, 0f); nyala.transform.localScale = new Vector3(0.44f, 0.62f, 0.44f);
                nyala.AddComponent<MeshFilter>().sharedMesh = apiMesh;
                var nmr = nyala.AddComponent<MeshRenderer>(); nmr.sharedMaterial = apiMat; nmr.shadowCastingMode = ShadowCastingMode.Off;
                var lGo = Anak(g, "CahayaApi"); lGo.transform.localPosition = new Vector3(0f, 0.95f, 0f);
                var l = lGo.AddComponent<Light>(); l.type = LightType.Point; l.range = 13f; l.intensity = 2.8f; l.color = new Color(1f, 0.58f, 0.20f); l.shadows = LightShadows.None;
                var comp = g.AddComponent<TitikKehidupanInteraktif>();
                comp.Pasang(TitikKehidupanInteraktif.JenisTitik.ApiUnggunPenyintas, api[i].nama, nyala.transform, l);
                EditorUtility.SetDirty(comp); JumlahTitikInteraktifV8++;
            }
        }

        static void BangunKeanekaragamanRimbaV8(GameObject induk, Material kayu, Material daun)
        {
            var go = Anak(induk, "KeanekaragamanRimbaV8");
            var matNuriA = MatPolos("M_NuriMerahV8", "#D63228");
            var matNuriB = MatPolos("M_NuriZamrudV8", "#2EA844");
            var matParuh = MatPolos("M_ParuhNuriV8", "#F0BE32");
            var matKupuA = MatPolos("M_KupuEmasV8", "#F2B830");
            var matKupuB = MatPolos("M_KupuBiruV8", "#32A8E8");
            var matSulur = MatPolos("M_SulurLumutV8", "#355C26");
            var matKura = MatPolos("M_KuraSungaiV8", "#3E522C");

            // 1. 16 Burung Kakaktua & Nuri tropis cerah bertengger di puncak dahan.
            Vector2[] posNuri = { new Vector2(16f, -432f), new Vector2(62f, -384f), new Vector2(306f, -158f), new Vector2(582f, 552f), new Vector2(-118f, 602f), new Vector2(-258f, 122f), new Vector2(548f, 212f), new Vector2(12f, -8f) };
            var nuri = new KeanekaragamanRimbaV8.BurungKanopi[16];
            for (int i = 0; i < 16; i++)
            {
                var c = posNuri[i % posNuri.Length];
                float x = c.x + Mathf.Cos(i * 1.9f) * 6.5f, z = c.y + Mathf.Sin(i * 1.9f) * 6.5f;
                float y = WorldTerrainGenerator.SampleHeight(x, z) + 5.8f + (i % 4) * 1.4f;
                var n = Anak(go, "NuriKanopi_" + i);
                n.transform.position = new Vector3(x, y, z);
                var mN = i % 2 == 0 ? matNuriA : matNuriB;
                Hias(n, PrimitiveType.Capsule, "Badan", Vector3.zero, new Vector3(0.16f, 0.24f, 0.16f), mN, false);
                Hias(n, PrimitiveType.Sphere, "Kepala", new Vector3(0f, 0.22f, 0.04f), Vector3.one * 0.14f, mN, false);
                Hias(n, PrimitiveType.Cube, "Paruh", new Vector3(0f, 0.21f, 0.13f), new Vector3(0.05f, 0.06f, 0.10f), matParuh, false);
                var sL = Anak(n, "SayapL"); sL.transform.localPosition = new Vector3(-0.08f, 0.04f, 0f);
                Hias(sL, PrimitiveType.Cube, "BuluL", new Vector3(-0.26f, 0f, 0f), new Vector3(0.52f, 0.03f, 0.18f), mN, false);
                var sR = Anak(n, "SayapR"); sR.transform.localPosition = new Vector3(0.08f, 0.04f, 0f);
                Hias(sR, PrimitiveType.Cube, "BuluR", new Vector3(0.26f, 0f, 0f), new Vector3(0.52f, 0.03f, 0.18f), mN, false);
                nuri[i] = new KeanekaragamanRimbaV8.BurungKanopi { tr = n.transform, sayapKiri = sL.transform, sayapKanan = sR.transform, dahan = new Vector3(x, y, z), fasa = i * 1.2f };
            }

            // 2. 32 Kupu-kupu tropis & lebah emas menari di kebun, air terjun & padang bunga.
            Vector2[] posKupu = { new Vector2(14f, -442f), new Vector2(56f, -376f), new Vector2(312f, -168f), new Vector2(590f, 560f), new Vector2(-120f, 610f), new Vector2(0f, -12f), new Vector2(-265f, 115f), new Vector2(210f, 420f) };
            var serangga = new KeanekaragamanRimbaV8.SeranggaBunga[32];
            for (int i = 0; i < 32; i++)
            {
                var c = posKupu[i % posKupu.Length];
                float y = WorldTerrainGenerator.SampleHeight(c.x, c.y);
                var s = Anak(go, "KupuTropis_" + i);
                s.transform.position = new Vector3(c.x, y + 1f, c.y);
                Hias(s, PrimitiveType.Cube, "Sayap", Vector3.zero, new Vector3(0.22f, 0.02f, 0.16f), i % 2 == 0 ? matKupuA : matKupuB, false);
                serangga[i] = new KeanekaragamanRimbaV8.SeranggaBunga { tr = s.transform, pusat = new Vector3(c.x, y, c.y), radius = 3.2f + (i % 4) * 1.4f, laju = 1.1f + (i % 4) * 0.25f, fasa = i * 1.35f };
            }

            // 3. 24 Sulur akar raksasa & tirai lumut gantung bergoyang angin di situs ikonik.
            Vector3[] posSulur = { new Vector3(0f, 11f, 0f), new Vector3(-265f, 7f, 115f), new Vector3(326f, 6f, -158f), new Vector3(-120f, 7f, 610f), new Vector3(272f, 6f, -218f), new Vector3(68f, 7f, -406f) };
            var sulur = new Transform[24];
            for (int i = 0; i < 24; i++)
            {
                var c = posSulur[i % posSulur.Length];
                float x = c.x + Mathf.Cos(i * 1.55f) * 5.2f, z = c.z + Mathf.Sin(i * 1.55f) * 5.2f;
                float y = WorldTerrainGenerator.SampleHeight(x, z) + c.y;
                var pv = Anak(go, "SulurLumut_" + i);
                pv.transform.position = new Vector3(x, y, z);
                float pj = 3.2f + (i % 4) * 0.9f;
                Hias(pv, PrimitiveType.Cylinder, "Untai", new Vector3(0f, -pj * 0.5f, 0f), new Vector3(0.08f, pj * 0.5f, 0.08f), matSulur, false);
                Hias(pv, PrimitiveType.Cube, "DaunSulur", new Vector3(0.08f, -pj * 0.65f, 0f), new Vector3(0.26f, pj * 0.45f, 0.06f), daun, false);
                sulur[i] = pv.transform;
            }

            // 4. 12 Kura-kura sungai berjemur di kayu tumbang tepi sungai & air terjun.
            Vector2[] posKura = { new Vector2(306f, -174f), new Vector2(268f, -222f), new Vector2(-242f, 90f), new Vector2(-180f, -152f) };
            var kuraKep = new Transform[12];
            for (int i = 0; i < 12; i++)
            {
                var c = posKura[i % posKura.Length];
                float x = c.x + (i % 3 - 1) * 1.6f, z = c.y + (i / 4) * 1.2f;
                float y = Mathf.Max(0.22f, WorldTerrainGenerator.SampleHeight(x, z)) + 0.10f;
                var kr = Anak(go, "KuraSungai_" + i);
                kr.transform.position = new Vector3(x, y, z);
                kr.transform.rotation = Quaternion.Euler(0f, i * 47f, 0f);
                Hias(kr, PrimitiveType.Cylinder, "KayuJemur", new Vector3(0f, -0.06f, 0f), new Vector3(0.42f, 0.55f, 0.42f), kayu, false).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                Hias(kr, PrimitiveType.Sphere, "Tempurung", new Vector3(0f, 0.14f, 0f), new Vector3(0.32f, 0.16f, 0.38f), matKura, false);
                var kp = Hias(kr, PrimitiveType.Sphere, "KepalaKura", new Vector3(0f, 0.14f, 0.24f), new Vector3(0.10f, 0.09f, 0.14f), matKura, false);
                kuraKep[i] = kp.transform;
            }

            var comp = go.AddComponent<KeanekaragamanRimbaV8>();
            comp.Pasang(nuri, serangga, sulur, kuraKep);
            EditorUtility.SetDirty(comp);
            JumlahHayatiV8 = comp.JumlahHayati;
        }

        static void Bangun22TuguPenunjukJalurV8(GameObject induk, Material batu, Material kayu, Material apiMat)
        {
            var grup = Anak(induk, "TuguJalur22LokasiV8");
            Vector2[] pos22 =
            {
                new Vector2(12f, -440f), new Vector2(54f, -372f), new Vector2(536f, 200f), new Vector2(4f, -8f),
                new Vector2(308f, -162f), new Vector2(322f, -154f), new Vector2(-260f, 112f), new Vector2(-238f, 86f),
                new Vector2(270f, -214f), new Vector2(-118f, -860f), new Vector2(660f, -496f), new Vector2(66f, -402f),
                new Vector2(716f, -136f), new Vector2(-576f, 616f), new Vector2(586f, 556f), new Vector2(-318f, -880f),
                new Vector2(-118f, 606f), new Vector2(-516f, -336f), new Vector2(-192f, -238f), new Vector2(466f, 174f),
                new Vector2(-386f, 306f), new Vector2(208f, 416f)
            };
            for (int i = 0; i < pos22.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(pos22[i].x, pos22[i].y);
                var tg = Anak(grup, "TuguJalur_" + i);
                tg.transform.position = new Vector3(pos22[i].x, y, pos22[i].y);
                tg.transform.rotation = Quaternion.Euler(0f, i * 31f, 0f);
                Hias(tg, PrimitiveType.Cylinder, "TumpukBatuA", new Vector3(0f, 0.24f, 0f), new Vector3(0.58f, 0.24f, 0.58f), batu, true);
                Hias(tg, PrimitiveType.Cylinder, "TumpukBatuB", new Vector3(0f, 0.56f, 0f), new Vector3(0.40f, 0.18f, 0.40f), batu, false);
                Hias(tg, PrimitiveType.Cylinder, "TiangPenunjuk", new Vector3(0f, 1.25f, 0f), new Vector3(0.09f, 0.85f, 0.09f), kayu, false);
                Hias(tg, PrimitiveType.Cube, "PapanArah", new Vector3(0.28f, 1.82f, 0f), new Vector3(0.76f, 0.20f, 0.06f), kayu, false);
                Hias(tg, PrimitiveType.Sphere, "PelitaKecil", new Vector3(0f, 2.12f, 0f), Vector3.one * 0.14f, apiMat, false).GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                JumlahTuguJalurV8++;
            }
        }
    }
}
