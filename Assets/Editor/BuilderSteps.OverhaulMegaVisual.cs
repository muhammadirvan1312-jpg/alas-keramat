using System.Collections.Generic;
using AlasKeramat.Core;
using AlasKeramat.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.EditorTools
{
    // Langkah 7z18a: Mega Overhaul Visual & Dunia — 7 Mega-Landmark Eksplorasi Baru (Dermaga Barat, Menara Pengawas Api 14m,
    // Gua Air Terjun Kristal, Pasar Hantu Desa, Jembatan Batu Candi, Kamp Peneliti, Taman Megalit), 18 Berkas Cahaya God-Rays,
    // 24 Kabut Lembah, 32 Kunang-Kunang + 24 Daun Gugur, & 28 Vignette Cerita Lingkungan.
    public static partial class NusantaraMasterBuilder
    {
        public static int JumlahLandmarkMegaV2, JumlahBerkasCahaya, JumlahKabutLembah, JumlahKunang, JumlahVignetteDunia;

        static void LangkahOverhaulMegaVisual()
        {
            JumlahLandmarkMegaV2 = 0; JumlahBerkasCahaya = 0; JumlahKabutLembah = 0; JumlahKunang = 0; JumlahVignetteDunia = 0;
            var lm = GameObject.Find(NamaRoot + "/World/Landmark") ?? GameObject.Find(NamaRoot + "/Landmark") ?? _root;
            var induk = Anak(lm, "MegaOverhaulVisualV2");

            var matKayu = AmbilMat("M_Kayu");
            var matBatu = AmbilMat("M_Batu");
            var matGenteng = AmbilMat("M_Genteng") ?? MatPolos("M_GentengMega", "#7B3A24");
            var matKain = AmbilMat("M_Kain") ?? MatPolos("M_KainMega", "#8C3B2B");
            var matKarat = AmbilMat("M_LogamKarat") ?? MatPolos("M_KaratMega", "#5B3B2B");
            var matKristal = SimpanMat(SafeMaterialFactory.CreatePBR("M_KristalGuaAir", new Color(0.28f, 0.92f, 0.86f, 0.9f), 0.1f, 0.9f, null, null, new Color(0.55f, 2.2f, 2.0f)));

            BangunDermagaBarat(induk, matKayu, matKarat);
            BangunMenaraPengawasApi(induk, matKayu, matGenteng);
            BangunGuaAirTerjunKristal(induk, matBatu, matKristal);
            BangunPasarHantuDesa(induk, matKayu, matKain, matGenteng);
            BangunJembatanBatuCandi(induk, matBatu);
            BangunKampPenelitiBaratLaut(induk, matKain, matKayu, matKarat);
            BangunTamanMegalitUtara(induk, matBatu);

            PasangBerkasCahayaDanKabutLembah(induk);
            PasangKunangDanDaunGugur(induk);
            SebarVignetteCeritaPulau(induk, matKayu, matBatu, matKain);

            Catat("Mega Overhaul Visual V2: " + JumlahLandmarkMegaV2 + " landmark besar baru, " + JumlahBerkasCahaya + " god-rays, "
                + JumlahKabutLembah + " kabut lembah, " + JumlahKunang + " kunang-kunang & " + JumlahVignetteDunia + " vignette dunia");
        }

        static void BangunDermagaBarat(GameObject induk, Material kayu, Material karat)
        {
            float x = -520f, z = -340f;
            float y = Mathf.Max(0.8f, WorldTerrainGenerator.SampleHeight(x, z));
            var go = Anak(induk, "DermagaTuaBarat");
            go.transform.position = new Vector3(x, y, z);
            Hias(go, PrimitiveType.Cube, "DekUtama", new Vector3(0f, 0.6f, -8f), new Vector3(4.2f, 0.25f, 24f), kayu, true);
            Hias(go, PrimitiveType.Cube, "DekUjung", new Vector3(0f, 0.6f, -21f), new Vector3(7.6f, 0.28f, 4.6f), kayu, true);
            for (int i = 0; i < 6; i++)
            {
                float pz = 2f - i * 4.5f;
                Hias(go, PrimitiveType.Cylinder, "TiangL_" + i, new Vector3(-1.8f, -0.6f, pz), new Vector3(0.22f, 2.2f, 0.22f), kayu, true);
                Hias(go, PrimitiveType.Cylinder, "TiangR_" + i, new Vector3(1.8f, -0.6f, pz), new Vector3(0.22f, 2.2f, 0.22f), kayu, true);
            }
            Hias(go, PrimitiveType.Cube, "PosJagaDermaga", new Vector3(2.8f, 1.8f, 3.2f), new Vector3(2.8f, 2.4f, 2.8f), kayu, true);
            Hias(go, PrimitiveType.Cube, "JangkarKarat", new Vector3(-2.6f, 0.95f, -20.5f), new Vector3(0.9f, 0.4f, 1.4f), karat, false);
            JumlahLandmarkMegaV2++;
        }

        static void BangunMenaraPengawasApi(GameObject induk, Material kayu, Material genteng)
        {
            float x = -195f, z = -240f;
            float y = WorldTerrainGenerator.SampleHeight(x, z);
            var go = Anak(induk, "MenaraPengawasApi");
            go.transform.position = new Vector3(x, y, z);

            // 4 tiang menara 13 m + panggung atas 5,5x5,5 m + tangga landai yang bisa dinaiki pemain
            for (int i = 0; i < 4; i++)
            {
                float sx = (i % 2 == 0 ? -1f : 1f) * 2.2f, sz = (i < 2 ? -1f : 1f) * 2.2f;
                Hias(go, PrimitiveType.Cylinder, "TiangMenara_" + i, new Vector3(sx, 6.5f, sz), new Vector3(0.26f, 6.6f, 0.26f), kayu, true);
            }
            Hias(go, PrimitiveType.Cube, "LantaiTengah", new Vector3(0f, 6.2f, 0f), new Vector3(5.0f, 0.25f, 5.0f), kayu, true);
            Hias(go, PrimitiveType.Cube, "DekPengawas", new Vector3(0f, 12.2f, 0f), new Vector3(6.2f, 0.3f, 6.2f), kayu, true);
            Hias(go, PrimitiveType.Cube, "AtapMenara", new Vector3(0f, 15.2f, 0f), new Vector3(6.8f, 0.45f, 6.8f), genteng, false);

            // 2 ramp tangga zig-zag sehingga Andi dapat naik ke puncak menara setinggi 12 m
            var r1 = Hias(go, PrimitiveType.Cube, "RampBawah", new Vector3(3.6f, 3.1f, 0f), new Vector3(1.5f, 0.22f, 13.5f), kayu, true);
            r1.transform.localRotation = Quaternion.Euler(27f, 0f, 0f);
            var r2 = Hias(go, PrimitiveType.Cube, "RampAtas", new Vector3(-3.6f, 9.2f, 0f), new Vector3(1.5f, 0.22f, 13.5f), kayu, true);
            r2.transform.localRotation = Quaternion.Euler(-27f, 0f, 0f);
            JumlahLandmarkMegaV2++;
        }

        static void BangunGuaAirTerjunKristal(GameObject induk, Material batu, Material kristal)
        {
            float x = 326f, z = -158f;
            float y = WorldTerrainGenerator.SampleHeight(x, z);
            var go = Anak(induk, "GuaAirTerjunRahasia");
            go.transform.position = new Vector3(x, y, z);

            // Lengkungan batu gua rahasia & 7 gugus kristal bercahaya
            Hias(go, PrimitiveType.Cube, "TebingKiri", new Vector3(-3.8f, 2.8f, 0f), new Vector3(2.2f, 6.0f, 9.5f), batu, true);
            Hias(go, PrimitiveType.Cube, "TebingKanan", new Vector3(3.8f, 2.8f, 0f), new Vector3(2.2f, 6.0f, 9.5f), batu, true);
            Hias(go, PrimitiveType.Cube, "AtapGua", new Vector3(0f, 5.6f, 0f), new Vector3(9.8f, 1.8f, 10.0f), batu, true);
            Hias(go, PrimitiveType.Cube, "DindingBelakang", new Vector3(0f, 2.8f, 4.4f), new Vector3(8.4f, 6.0f, 1.8f), batu, true);
            for (int i = 0; i < 7; i++)
            {
                float kx = -2.4f + i * 0.8f, kz = 1.8f + (i % 3) * 0.7f;
                var k = Hias(go, PrimitiveType.Cube, "Kristal_" + i, new Vector3(kx, 0.85f + (i % 2) * 0.45f, kz), new Vector3(0.28f, 1.4f + (i % 3) * 0.4f, 0.28f), kristal, false);
                k.transform.localRotation = Quaternion.Euler((i - 3) * 8f, i * 37f, (i % 2 == 0 ? 12f : -12f));
            }
            var lGo = Anak(go, "CahayaKristal");
            lGo.transform.localPosition = new Vector3(0f, 2.2f, 1.8f);
            var l = lGo.AddComponent<Light>();
            l.type = LightType.Point; l.range = 12f; l.intensity = 2.4f; l.color = new Color(0.28f, 0.92f, 0.88f); l.shadows = LightShadows.None;
            JumlahLandmarkMegaV2++;
        }

        static void BangunPasarHantuDesa(GameObject induk, Material kayu, Material kain, Material genteng)
        {
            float x = 468f, z = 178f;
            float y = WorldTerrainGenerator.SampleHeight(x, z);
            var go = Anak(induk, "PasarHantuDesa");
            go.transform.position = new Vector3(x, y, z);

            // 6 kios kayu pasar tradisional mengapit gang pasar
            for (int i = 0; i < 6; i++)
            {
                float sx = (i % 2 == 0 ? -5.2f : 5.2f);
                float sz = -7.5f + (i / 2) * 7.5f;
                var kios = Anak(go, "KiosPasar_" + i);
                kios.transform.localPosition = new Vector3(sx, 0f, sz);
                Hias(kios, PrimitiveType.Cube, "Meja", new Vector3(0f, 0.85f, 0f), new Vector3(3.4f, 0.2f, 1.8f), kayu, true);
                Hias(kios, PrimitiveType.Cylinder, "TiangA", new Vector3(-1.5f, 1.4f, -0.7f), new Vector3(0.11f, 1.4f, 0.11f), kayu, false);
                Hias(kios, PrimitiveType.Cylinder, "TiangB", new Vector3(1.5f, 1.4f, -0.7f), new Vector3(0.11f, 1.4f, 0.11f), kayu, false);
                var atap = Hias(kios, PrimitiveType.Cube, "KanopiKios", new Vector3(0f, 2.75f, 0f), new Vector3(3.9f, 0.16f, 2.4f), i % 2 == 0 ? kain : genteng, false);
                atap.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
            }
            JumlahLandmarkMegaV2++;
        }

        static void BangunJembatanBatuCandi(GameObject induk, Material batu)
        {
            float x = -240f, z = 88f;
            float y = WorldTerrainGenerator.SampleHeight(x, z);
            var go = Anak(induk, "JembatanBatuKuno");
            go.transform.position = new Vector3(x, y, z);
            go.transform.rotation = Quaternion.Euler(0f, -42f, 0f);
            Hias(go, PrimitiveType.Cube, "JalanBatu", new Vector3(0f, 0.45f, 0f), new Vector3(4.6f, 0.9f, 22f), batu, true);
            for (int i = 0; i < 6; i++)
            {
                float pz = -9f + i * 3.6f;
                Hias(go, PrimitiveType.Cube, "PagarL_" + i, new Vector3(-2.1f, 1.35f, pz), new Vector3(0.5f, 1.2f, 0.5f), batu, true);
                Hias(go, PrimitiveType.Cube, "PagarR_" + i, new Vector3(2.1f, 1.35f, pz), new Vector3(0.5f, 1.2f, 0.5f), batu, true);
            }
            JumlahLandmarkMegaV2++;
        }

        static void BangunKampPenelitiBaratLaut(GameObject induk, Material kain, Material kayu, Material karat)
        {
            float x = -390f, z = 310f;
            float y = WorldTerrainGenerator.SampleHeight(x, z);
            var go = Anak(induk, "KampPenelitiTerbengkalai");
            go.transform.position = new Vector3(x, y, z);

            Vector3[] tenda = { new Vector3(-5.5f, 0f, -2.5f), new Vector3(5.2f, 0f, -1.8f), new Vector3(0.2f, 0f, 6.0f) };
            for (int i = 0; i < tenda.Length; i++)
            {
                var t = Anak(go, "TendaRiset_" + i);
                t.transform.localPosition = tenda[i];
                t.transform.localRotation = Quaternion.Euler(0f, i * 48f - 30f, 0f);
                var sL = Hias(t, PrimitiveType.Cube, "SisiL", new Vector3(-0.9f, 1.05f, 0f), new Vector3(0.12f, 2.3f, 3.6f), kain, true);
                sL.transform.localRotation = Quaternion.Euler(0f, 0f, -34f);
                var sR = Hias(t, PrimitiveType.Cube, "SisiR", new Vector3(0.9f, 1.05f, 0f), new Vector3(0.12f, 2.3f, 3.6f), kain, true);
                sR.transform.localRotation = Quaternion.Euler(0f, 0f, 34f);
                Hias(t, PrimitiveType.Cube, "AlasTenda", new Vector3(0f, 0.08f, 0f), new Vector3(3.1f, 0.16f, 3.5f), kayu, true);
            }
            Hias(go, PrimitiveType.Cube, "GeneratorRiset", new Vector3(-1.2f, 0.65f, 0.8f), new Vector3(1.4f, 1.2f, 1.9f), karat, true);
            Hias(go, PrimitiveType.Cube, "MejaPetaRiset", new Vector3(1.8f, 0.85f, 1.2f), new Vector3(2.2f, 0.18f, 1.2f), kayu, true);
            JumlahLandmarkMegaV2++;
        }

        static void BangunTamanMegalitUtara(GameObject induk, Material batu)
        {
            float x = 210f, z = 420f;
            float y = WorldTerrainGenerator.SampleHeight(x, z);
            var go = Anak(induk, "TamanBatuMegalit");
            go.transform.position = new Vector3(x, y, z);

            Hias(go, PrimitiveType.Cylinder, "UndakBawah", new Vector3(0f, 0.35f, 0f), new Vector3(15f, 0.35f, 15f), batu, true);
            Hias(go, PrimitiveType.Cylinder, "UndakTengah", new Vector3(0f, 0.95f, 0f), new Vector3(10.2f, 0.4f, 10.2f), batu, true);
            Hias(go, PrimitiveType.Cube, "AltarMegalit", new Vector3(0f, 1.75f, 0f), new Vector3(2.4f, 1.0f, 1.6f), batu, true);
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * Mathf.Deg2Rad;
                var m = Hias(go, PrimitiveType.Cube, "Menhir_" + i, new Vector3(Mathf.Cos(a) * 6.4f, 2.6f, Mathf.Sin(a) * 6.4f), new Vector3(1.1f, 4.6f + (i % 3) * 0.7f, 0.85f), batu, true);
                m.transform.localRotation = Quaternion.Euler((i % 2 == 0 ? 5f : -4f), -i * 45f, 0f);
            }
            JumlahLandmarkMegaV2++;
        }

        static void PasangBerkasCahayaDanKabutLembah(GameObject induk)
        {
            var grup = Anak(induk, "AtmosferBerkasDanKabut");
            var matBerkas = SimpanMat(SafeMaterialFactory.CreatePBR("M_BerkasCahayaRimba", new Color(1f, 0.92f, 0.68f, 0.16f), 0f, 0.05f, null, null, new Color(1.0f, 0.86f, 0.52f), SurfaceMode.Transparent));
            var matKabut = SimpanMat(SafeMaterialFactory.CreatePBR("M_KabutLembah", new Color(0.84f, 0.90f, 0.88f, 0.18f), 0f, 0.05f, null, null, null, SurfaceMode.Transparent));

            Vector2[] posBerkas =
            {
                new Vector2(12f, -438f), new Vector2(46f, -382f), new Vector2(120f, -310f), new Vector2(268f, -218f),
                new Vector2(314f, -162f), new Vector2(518f, 196f), new Vector2(468f, 178f), new Vector2(0f, -12f),
                new Vector2(-195f, -236f), new Vector2(-262f, 112f), new Vector2(-390f, 310f), new Vector2(210f, 420f),
                new Vector2(-160f, 680f), new Vector2(580f, 540f), new Vector2(710f, -120f), new Vector2(-680f, -180f),
                new Vector2(-520f, -340f), new Vector2(-115f, -858f)
            };
            var listBerkas = new List<MeshRenderer>(posBerkas.Length);
            for (int i = 0; i < posBerkas.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(posBerkas[i].x, posBerkas[i].y);
                var q = Hias(grup, PrimitiveType.Cylinder, "GodRay_" + i, new Vector3(posBerkas[i].x, y + 11f, posBerkas[i].y), new Vector3(3.2f, 13.5f, 3.2f), matBerkas, false);
                var mr = q.GetComponent<MeshRenderer>();
                mr.shadowCastingMode = ShadowCastingMode.Off;
                mr.receiveShadows = false;
                listBerkas.Add(mr);
                JumlahBerkasCahaya++;
            }

            Vector2[] posKabut =
            {
                new Vector2(240f, -245f), new Vector2(278f, -212f), new Vector2(312f, -165f), new Vector2(190f, -295f),
                new Vector2(-180f, -154f), new Vector2(-220f, -135f), new Vector2(-150f, -190f), new Vector2(532f, 202f),
                new Vector2(468f, 176f), new Vector2(54f, -374f), new Vector2(8f, -445f), new Vector2(0f, 12f),
                new Vector2(-260f, 110f), new Vector2(-240f, 88f), new Vector2(-390f, 310f), new Vector2(210f, 420f),
                new Vector2(-160f, 675f), new Vector2(580f, 540f), new Vector2(705f, -125f), new Vector2(-518f, -345f),
                new Vector2(-640f, -552f), new Vector2(660f, -502f), new Vector2(-115f, -848f), new Vector2(18f, -844f)
            };
            var listKabut = new List<MeshRenderer>(posKabut.Length);
            var listAsal = new List<Vector3>(posKabut.Length);
            for (int i = 0; i < posKabut.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(posKabut[i].x, posKabut[i].y) + 0.9f;
                var pos = new Vector3(posKabut[i].x, y, posKabut[i].y);
                var q = Hias(grup, PrimitiveType.Quad, "KabutLembah_" + i, pos, new Vector3(26f, 26f, 1f), matKabut, false);
                q.transform.rotation = Quaternion.Euler(90f, i * 19f, 0f);
                var mr = q.GetComponent<MeshRenderer>();
                mr.shadowCastingMode = ShadowCastingMode.Off;
                mr.receiveShadows = false;
                listKabut.Add(mr);
                listAsal.Add(pos);
                JumlahKabutLembah++;
            }

            var comp = grup.AddComponent<BerkasCahayaRimba>();
            comp.Pasang(listBerkas.ToArray(), listKabut.ToArray(), listAsal.ToArray());
            EditorUtility.SetDirty(comp);
        }

        static void PasangKunangDanDaunGugur(GameObject induk)
        {
            var grup = Anak(induk, "KunangDanDaunGugur");
            var matKunang = SimpanMat(SafeMaterialFactory.CreatePBR("M_KunangKunang", new Color(0.86f, 1f, 0.36f, 1f), 0f, 0.1f, null, null, new Color(3.6f, 4.4f, 1.1f)));
            var matDaun = MatPolos("M_DaunGugur", "#8E7A2A");
            var rnd = new System.Random(8841);

            var kunangs = new List<Transform>(32);
            var asalK = new List<Vector3>(32);
            var dauns = new List<Transform>(24);
            var asalD = new List<Vector3>(24);

            Vector2[] pusat =
            {
                new Vector2(18f, -440f), new Vector2(56f, -376f), new Vector2(272f, -218f), new Vector2(312f, -165f),
                new Vector2(516f, 198f), new Vector2(468f, 178f), new Vector2(0f, 0f), new Vector2(-265f, 115f)
            };
            for (int i = 0; i < 32; i++)
            {
                var c = pusat[i % pusat.Length];
                float x = c.x + (float)(rnd.NextDouble() * 28.0 - 14.0);
                float z = c.y + (float)(rnd.NextDouble() * 28.0 - 14.0);
                float y = WorldTerrainGenerator.SampleHeight(x, z) + 1.2f + (float)rnd.NextDouble() * 1.6f;
                var pos = new Vector3(x, y, z);
                var b = Hias(grup, PrimitiveType.Sphere, "Kunang_" + i, pos, Vector3.one * 0.09f, matKunang, false);
                b.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                kunangs.Add(b.transform);
                asalK.Add(pos);
                JumlahKunang++;
            }
            for (int i = 0; i < 24; i++)
            {
                var c = pusat[i % pusat.Length];
                float x = c.x + (float)(rnd.NextDouble() * 34.0 - 17.0);
                float z = c.y + (float)(rnd.NextDouble() * 34.0 - 17.0);
                float y = WorldTerrainGenerator.SampleHeight(x, z) + 0.5f;
                var pos = new Vector3(x, y, z);
                var d = Hias(grup, PrimitiveType.Quad, "DaunGugur_" + i, pos, new Vector3(0.24f, 0.14f, 1f), matDaun, false);
                d.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                dauns.Add(d.transform);
                asalD.Add(pos);
            }
            var comp = grup.AddComponent<KunangDanDaunGugur>();
            comp.Pasang(kunangs.ToArray(), asalK.ToArray(), dauns.ToArray(), asalD.ToArray());
            EditorUtility.SetDirty(comp);
        }

        static void SebarVignetteCeritaPulau(GameObject induk, Material kayu, Material batu, Material kain)
        {
            var grup = Anak(induk, "VignetteCeritaPulau");
            var rnd = new System.Random(7419);
            for (int i = 0; i < 28; i++)
            {
                float a = i * (360f / 28f) * Mathf.Deg2Rad;
                float r = 130f + (i % 5) * 145f;
                float x = Mathf.Cos(a) * r, z = Mathf.Sin(a) * r;
                float y = WorldTerrainGenerator.SampleHeight(x, z);
                if (y < 1.2f) continue;

                var v = Anak(grup, "Vignette_" + i);
                v.transform.position = new Vector3(x, y, z);
                v.transform.rotation = Quaternion.Euler(0f, (float)rnd.NextDouble() * 360f, 0f);
                int jenis = i % 3;
                if (jenis == 0)
                {
                    // Pelinggih batu pinggir jalur Nusantara
                    Hias(v, PrimitiveType.Cube, "AlasBatu", new Vector3(0f, 0.45f, 0f), new Vector3(0.9f, 0.9f, 0.9f), batu, true);
                    Hias(v, PrimitiveType.Cube, "KainPelinggih", new Vector3(0f, 0.55f, 0f), new Vector3(0.96f, 0.35f, 0.96f), kain, false);
                }
                else if (jenis == 1)
                {
                    // Bekas perkemahan pemburu (tumpukan kayu + batu tungku)
                    Hias(v, PrimitiveType.Cylinder, "KayuDuduk", new Vector3(1.2f, 0.22f, 0f), new Vector3(0.32f, 0.95f, 0.32f), kayu, true).transform.localRotation = Quaternion.Euler(90f, 20f, 0f);
                    Hias(v, PrimitiveType.Cylinder, "TungkuBatu", new Vector3(0f, 0.14f, 0f), new Vector3(0.95f, 0.14f, 0.95f), batu, false);
                }
                else
                {
                    // Peti ekspedisi terbalik & tonggak peringatan
                    Hias(v, PrimitiveType.Cube, "PetiTerbalik", new Vector3(0f, 0.35f, 0f), new Vector3(0.95f, 0.65f, 0.75f), kayu, true).transform.localRotation = Quaternion.Euler(14f, 25f, -18f);
                    Hias(v, PrimitiveType.Cylinder, "Tonggak", new Vector3(-0.9f, 0.9f, 0.4f), new Vector3(0.11f, 0.95f, 0.11f), kayu, false);
                }
                JumlahVignetteDunia++;
            }
        }
    }
}
