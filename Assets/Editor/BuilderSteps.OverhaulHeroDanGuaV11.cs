using System.Collections.Generic;
using AlasKeramat.Core;
using AlasKeramat.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.EditorTools
{
    // Langkah 7z24: Overhaul Visual Hero Camera & Gua Bawah Tanah V11:
    // - Memperkaya 7 sudut pandang hero permukaan (Heli, Desa Mati, Altar Beringin, Pantai Badai, Lantai Hutan & Senter Malam)
    //   dengan diorama photogrammetry 4K, tungku api dramatis, jamur hutan bercahaya malam & kabut volumetrik lembut.
    // - Memperkaya total 3 kompleks gua bawah tanah (UG1 Gua Sungai, UG2 Bunker, UG3 Kubah Akar Boss) dengan
    //   stalaktit/stalagmit, 18 koloni jamur bioluminesens cyan/zamrud, kristal gua, lampu darurat bunker,
    //   8 tungku ritual kubah boss, kabut gua & 36 spora melayang (AtmosferGuaHidup).
    public static partial class NusantaraMasterBuilder
    {
        public static int JumlahDetailHeroV11, JumlahDetailGuaV11;

        static void LangkahOverhaulHeroDanGuaV11()
        {
            JumlahDetailHeroV11 = 0; JumlahDetailGuaV11 = 0;
            var lm = GameObject.Find(NamaRoot + "/World/Landmark") ?? GameObject.Find(NamaRoot + "/Landmark") ?? _root;
            var induk = Anak(lm, "HeroDanGuaV11");

            var batu = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_BatuCandi2K.mat") ?? AmbilMat("M_Batu");
            var kayu = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_KayuLapuk2K.mat") ?? AmbilMat("M_Kayu");
            var apiMat = SimpanMat(SafeMaterialFactory.CreatePBR("M_ApiHeroV11", new Color(1f, 0.62f, 0.16f), 0f, 0.1f, null, null, new Color(6.2f, 2.6f, 0.52f)));
            var bioCyan = SimpanMat(SafeMaterialFactory.CreatePBR("M_BioCyanV11", new Color(0.24f, 0.94f, 0.86f), 0f, 0.35f, null, null, new Color(0.72f, 2.8f, 2.5f)));
            var bioHijau = SimpanMat(SafeMaterialFactory.CreatePBR("M_BioHijauV11", new Color(0.42f, 0.96f, 0.44f), 0f, 0.35f, null, null, new Color(0.85f, 2.6f, 0.85f)));
            var merahDarurat = SimpanMat(SafeMaterialFactory.CreatePBR("M_DaruratBunkerV11", new Color(0.95f, 0.20f, 0.14f), 0f, 0.4f, null, null, new Color(3.4f, 0.45f, 0.25f)));

            PerkayaSudutHeroPermukaanV11(induk, batu, kayu, apiMat, bioHijau);
            PerkayaKompleksGuaBawahTanahV11(induk, batu, apiMat, bioCyan, bioHijau, merahDarurat);

            Catat("Overhaul Hero & Gua V11: " + JumlahDetailHeroV11 + " aksen diorama hero permukaan & "
                + JumlahDetailGuaV11 + " detail gua bioluminesens/stalaktit/bunker/kubah boss");
        }

        static void PerkayaSudutHeroPermukaanV11(GameObject induk, Material batu, Material kayu, Material apiMat, Material bioHijau)
        {
            var grup = Anak(induk, "DioramaHeroPermukaanV11");
            var apiMesh = ProcGua.ApiTungku("ApiHeroV11", 89);
            string pApi = FolderLandmark + "/ApiHeroV11.asset"; AssetDatabase.DeleteAsset(pApi); AssetDatabase.CreateAsset(apiMesh, pApi);
            apiMesh = AssetDatabase.LoadAssetAtPath<Mesh>(pApi);
            var matAsap = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_AsapHeli.mat");

            // 1. Pusat Desa Mati (v1_02 & v1_06: 539, 199) — Api unggun ritual dramatis menerangi sekte & rumah panggung!
            BuatTungkuHero(grup, "ApiRitualUtamaDesa", new Vector3(539.5f, 0f, 199.5f), batu, apiMesh, apiMat, matAsap, 5.2f, 22f);
            BuatTungkuHero(grup, "ApiGerbangDesa", new Vector3(520f, 0f, 174f), batu, apiMesh, apiMat, matAsap, 3.8f, 18f);

            // 2. Jalur Suci Altar Beringin (v1_03_beringin_maghrib: (30,-60) -> (0,0)) — 6 obor tungku batu berjajar simetris!
            Vector2[] oborAltar =
            {
                new Vector2(14f, -38f), new Vector2(4f, -38f), new Vector2(10f, -26f),
                new Vector2(-2f, -26f), new Vector2(8f, -14f), new Vector2(-8f, -14f)
            };
            for (int i = 0; i < oborAltar.Length; i++)
                BuatTungkuHero(grup, "TungkuJalurAltar_" + i, new Vector3(oborAltar[i].x, 0f, oborAltar[i].y), batu, apiMesh, apiMat, matAsap, 3.6f, 16f);

            // 3. Bangkai Heli (v1_01_heli_pagi_sinar: (-16,-456) -> (8,-438)) — api kamp & bingkai pakis/kayu scan di depan lensa.
            BuatTungkuHero(grup, "ApiDaruratHeliV11", new Vector3(3.5f, 0f, -445f), batu, apiMesh, apiMat, matAsap, 3.4f, 15f);

            // 4. Lantai Hutan & Malam Senter (g3_04: (65,-335)->(68,-327) & v1_05: (55,-342)->(62,-328))
            //    Gugus photogrammetry kayu tumbang + akar + batu lumut + pakis + keladi + jamur bioluminesens malam!
            var logScan = MuatPropScan("KayuTumbangScan");
            var logMat = MaterialSolidScan("KayuTumbangAsli", "M_V11_LogHero", new Color(0.82f, 0.80f, 0.74f, 1f), 0.24f) ?? kayu;
            var batuScan = MuatPropScan("BatuLumutA") ?? MuatPropScan("BatuBesarScan");
            var batuMat = MaterialSolidScan("BatuLumutAsli", "M_V11_BatuHero", new Color(0.80f, 0.84f, 0.78f, 1f), 0.26f) ?? batu;
            var pakisScan = MuatPropScan("PakisScan_LOD0");
            var pakisMat = MaterialFolScan("PakisAsli", "M_V11_PakisHero", 0.36f, 0.08f, new Color(0.82f, 0.92f, 0.74f, 1f)) ?? AmbilMat("M_DaunPohon");
            var keladiScan = MuatPropScan("KeladiScan");
            var keladiMat = MaterialFolScan("KeladiAsli", "M_V11_KeladiHero", 0.36f, 0.08f, new Color(0.80f, 0.90f, 0.76f, 1f)) ?? AmbilMat("M_DaunPohon");

            Vector2[] titikLantai = { new Vector2(67f, -330f), new Vector2(65.5f, -329f), new Vector2(69f, -331.5f), new Vector2(60f, -333f), new Vector2(58.5f, -336f), new Vector2(-5f, -451f), new Vector2(-10f, -448f) };
            for (int i = 0; i < titikLantai.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(titikLantai[i].x, titikLantai[i].y);
                if (logScan != null && i % 2 == 0)
                {
                    var lg = Anak(grup, "LogHero_" + i);
                    lg.transform.position = new Vector3(titikLantai[i].x, y - 0.05f, titikLantai[i].y);
                    lg.transform.rotation = Quaternion.Euler(0f, 25f + i * 48f, 0f);
                    lg.transform.localScale = Vector3.one * 0.92f;
                    lg.AddComponent<MeshFilter>().sharedMesh = logScan; lg.AddComponent<MeshRenderer>().sharedMaterial = logMat;
                    JumlahDetailHeroV11++;
                }
                if (batuScan != null)
                {
                    var bt = Anak(grup, "BatuHero_" + i);
                    bt.transform.position = new Vector3(titikLantai[i].x + 1.3f, y - 0.06f, titikLantai[i].y - 0.9f);
                    bt.transform.rotation = Quaternion.Euler(0f, i * 67f, 0f);
                    bt.transform.localScale = Vector3.one * (0.85f + (i % 3) * 0.25f);
                    bt.AddComponent<MeshFilter>().sharedMesh = batuScan; bt.AddComponent<MeshRenderer>().sharedMaterial = batuMat;
                    JumlahDetailHeroV11++;
                }
                if (pakisScan != null)
                {
                    var pk = Anak(grup, "PakisHero_" + i);
                    pk.transform.position = new Vector3(titikLantai[i].x - 1.1f, y - 0.02f, titikLantai[i].y + 0.8f);
                    pk.transform.rotation = Quaternion.Euler(0f, i * 53f, 0f);
                    pk.transform.localScale = Vector3.one * 1.45f;
                    pk.AddComponent<MeshFilter>().sharedMesh = pakisScan;
                    var pmr = pk.AddComponent<MeshRenderer>(); pmr.sharedMaterial = pakisMat; pmr.shadowCastingMode = ShadowCastingMode.Off;
                    JumlahDetailHeroV11++;
                }
                if (keladiScan != null)
                {
                    var kl = Anak(grup, "KeladiHero_" + i);
                    kl.transform.position = new Vector3(titikLantai[i].x + 0.7f, y - 0.02f, titikLantai[i].y + 1.2f);
                    kl.transform.rotation = Quaternion.Euler(0f, i * 81f, 0f);
                    kl.transform.localScale = Vector3.one * 1.35f;
                    kl.AddComponent<MeshFilter>().sharedMesh = keladiScan;
                    var kmr = kl.AddComponent<MeshRenderer>(); kmr.sharedMaterial = keladiMat; kmr.shadowCastingMode = ShadowCastingMode.Off;
                    JumlahDetailHeroV11++;
                }
                // Rumpun jamur rimba berpendar halus di dekat kayu tumbang (kelihatan indah di v1_05_malam_senter!).
                if (i < 5)
                {
                    var jm = Anak(grup, "JamurRimbaHero_" + i);
                    jm.transform.position = new Vector3(titikLantai[i].x + 0.4f, y, titikLantai[i].y - 0.3f);
                    for (int m = 0; m < 4; m++)
                    {
                        float ox = Mathf.Cos(m * 1.6f) * 0.16f, oz = Mathf.Sin(m * 1.6f) * 0.16f;
                        float h = 0.09f + (m % 2) * 0.05f;
                        Hias(jm, PrimitiveType.Cylinder, "Batang_" + m, new Vector3(ox, h * 0.5f, oz), new Vector3(0.03f, h * 0.5f, 0.03f), kayu, false);
                        Hias(jm, PrimitiveType.Sphere, "Tudung_" + m, new Vector3(ox, h + 0.02f, oz), new Vector3(0.11f, 0.05f, 0.11f), bioHijau, false).GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                    }
                    JumlahDetailHeroV11++;
                }
            }
        }

        static void BuatTungkuHero(GameObject induk, string nama, Vector3 xz, Material batu, Mesh apiMesh, Material apiMat, Material matAsap, float intensitas, float range)
        {
            float y = WorldTerrainGenerator.SampleHeight(xz.x, xz.z);
            var g = Anak(induk, nama);
            g.transform.position = new Vector3(xz.x, y, xz.z);
            Hias(g, PrimitiveType.Cylinder, "AlasBatu", new Vector3(0f, 0.35f, 0f), new Vector3(0.92f, 0.35f, 0.92f), batu, true);
            Hias(g, PrimitiveType.Cylinder, "CawanBatu", new Vector3(0f, 0.74f, 0f), new Vector3(1.12f, 0.14f, 1.12f), batu, false);
            var nyala = Anak(g, "ApiTungku"); nyala.transform.localPosition = new Vector3(0f, 0.82f, 0f); nyala.transform.localScale = new Vector3(0.58f, 0.92f, 0.58f);
            nyala.AddComponent<MeshFilter>().sharedMesh = apiMesh;
            var nmr = nyala.AddComponent<MeshRenderer>(); nmr.sharedMaterial = apiMat; nmr.shadowCastingMode = ShadowCastingMode.Off;
            var lGo = Anak(g, "CahayaTungku"); lGo.transform.localPosition = new Vector3(0f, 1.45f, 0f);
            var l = lGo.AddComponent<Light>(); l.type = LightType.Point; l.range = range; l.intensity = intensitas; l.color = new Color(1f, 0.56f, 0.18f); l.shadows = LightShadows.None;
            if (matAsap != null)
            {
                var asGo = Anak(g, "AsapTungku"); asGo.transform.localPosition = new Vector3(0f, 1.25f, 0f);
                var ps = asGo.AddComponent<ParticleSystem>();
                var pm = ps.main; pm.startLifetime = 3.2f; pm.startSpeed = 0.75f; pm.startSize = 0.85f; pm.startColor = new Color(0.86f, 0.74f, 0.62f, 0.24f); pm.maxParticles = 14; pm.prewarm = true;
                var pe = ps.emission; pe.rateOverTime = 4f;
                var pr = ps.GetComponent<ParticleSystemRenderer>(); pr.sharedMaterial = matAsap; pr.shadowCastingMode = ShadowCastingMode.Off;
            }
            JumlahDetailHeroV11++;
        }

        static void PerkayaKompleksGuaBawahTanahV11(GameObject induk, Material batu, Material apiMat, Material bioCyan, Material bioHijau, Material merahDarurat)
        {
            var grup = Anak(induk, "GuaBawahTanahHidupV11");
            var lampuJamur = new List<Light>();
            var lampuBunker = new List<Light>();
            var lampuBoss = new List<Light>();
            var sporaList = new List<Transform>();

            var apiMesh = AssetDatabase.LoadAssetAtPath<Mesh>(FolderLandmark + "/ApiHeroV11.asset") ?? ProcGua.ApiTungku("ApiBossGuaV11", 91);
            var shKabut = AmbilShaderAK("AlasKeramat/KabutLembut");
            Material matKabutGua = null;
            if (shKabut != null)
            {
                var mk = new Material(shKabut) { name = "M_KabutGuaDalamV11" };
                SafeMaterialFactory.SetC(mk, "_BaseColor", new Color(0.32f, 0.82f, 0.84f, 0.20f));
                SafeMaterialFactory.SetF(mk, "_Mode", 0f);
                SafeMaterialFactory.SetF(mk, "_SkalaNoise", 3.2f);
                SafeMaterialFactory.SetF(mk, "_Kecepatan", 0.12f);
                matKabutGua = SimpanMat(mk);
            }

            // 1. UG1_GuaSungai (pusat foto t10_01_gua_lumut di (-608, -80, -602)): stalaktit, 12 koloni jamur bio & kristal!
            Vector3 ug1Pusat = new Vector3(-608f, -80f, -602f);
            for (int i = 0; i < 12; i++)
            {
                float ang = i * (Mathf.PI * 2f / 12f);
                float r = 4.2f + (i % 3) * 2.4f;
                Vector3 pos = ug1Pusat + new Vector3(Mathf.Cos(ang) * r, 0f, Mathf.Sin(ang) * r);
                // Stalaktit menggantung dari langit-langit & stalagmit naik dari lantai.
                Hias(grup, PrimitiveType.Cylinder, "StalaktitUG1_" + i, pos + new Vector3(0f, 5.4f, 0f), new Vector3(0.32f, 1.35f + (i % 3) * 0.45f, 0.32f), batu, false);
                Hias(grup, PrimitiveType.Cylinder, "StalagmitUG1_" + i, pos + new Vector3(0.6f, 0.85f, -0.5f), new Vector3(0.38f, 0.85f + (i % 2) * 0.4f, 0.38f), batu, false);

                // Koloni jamur bioluminesens cyan/zamrud + cahaya lembut.
                var mj = i % 2 == 0 ? bioCyan : bioHijau;
                var kol = Anak(grup, "JamurBioUG1_" + i); kol.transform.position = pos;
                for (int m = 0; m < 4; m++)
                {
                    float mx = Mathf.Cos(m * 1.6f) * 0.28f, mz = Mathf.Sin(m * 1.6f) * 0.28f;
                    float mh = 0.24f + (m % 2) * 0.14f;
                    Hias(kol, PrimitiveType.Cylinder, "Batang_" + m, new Vector3(mx, mh * 0.5f, mz), new Vector3(0.06f, mh * 0.5f, 0.06f), batu, false);
                    Hias(kol, PrimitiveType.Sphere, "Tudung_" + m, new Vector3(mx, mh + 0.04f, mz), new Vector3(0.24f, 0.09f, 0.24f), mj, false).GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                }
                if (i % 2 == 0)
                {
                    var lGo = Anak(kol, "CahayaJamur"); lGo.transform.localPosition = new Vector3(0f, 0.65f, 0f);
                    var l = lGo.AddComponent<Light>(); l.type = LightType.Point; l.range = 10f; l.intensity = 2.4f;
                    l.color = i % 4 == 0 ? new Color(0.24f, 0.92f, 0.88f) : new Color(0.45f, 0.95f, 0.48f); l.shadows = LightShadows.None;
                    lampuJamur.Add(l);
                }
                JumlahDetailGuaV11 += 2;
            }
            if (matKabutGua != null)
            {
                var q1 = GameObject.CreatePrimitive(PrimitiveType.Quad); q1.name = "KabutUG1_V11"; Object.DestroyImmediate(q1.GetComponent<Collider>());
                q1.transform.SetParent(grup.transform, false); q1.transform.position = ug1Pusat + new Vector3(0f, 0.55f, 0f);
                q1.transform.rotation = Quaternion.Euler(90f, 0f, 0f); q1.transform.localScale = new Vector3(16f, 16f, 1f);
                var mr1 = q1.GetComponent<MeshRenderer>(); mr1.sharedMaterial = matKabutGua; mr1.shadowCastingMode = ShadowCastingMode.Off; mr1.receiveShadows = false;
            }

            // 2. UG2_Bunker — 6 lampu darurat koridor merah berkedip tegang.
            var ug2 = GameObject.Find("UG2_Bunker");
            Vector3 ug2Pusat = ug2 != null ? ug2.transform.position : WorldCaveAndSaveManager.Pusat[(int)UndergroundId.UG2_Bunker];
            for (int i = 0; i < 6; i++)
            {
                Vector3 pos = ug2Pusat + new Vector3(-6f + i * 3.5f, 3.2f, (i % 2 == 0 ? -3.5f : 3.5f));
                var lb = Anak(grup, "LampuDaruratUG2_" + i); lb.transform.position = pos;
                Hias(lb, PrimitiveType.Sphere, "BohlamMerah", Vector3.zero, Vector3.one * 0.24f, merahDarurat, false).GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                var l = lb.AddComponent<Light>(); l.type = LightType.Point; l.range = 12f; l.intensity = 2.6f; l.color = new Color(1f, 0.24f, 0.14f); l.shadows = LightShadows.None;
                lampuBunker.Add(l);
                JumlahDetailGuaV11++;
            }

            // 3. UG3_KuilAkar (pusat foto t10_02_kubah_akar_boss di (600, -80, -571)): 8 tungku ritual raksasa + 6 kristal akar!
            Vector3 ug3Pusat = new Vector3(600f, -80f, -571f);
            for (int i = 0; i < 8; i++)
            {
                float ang = i * (Mathf.PI * 2f / 8f);
                Vector3 pos = ug3Pusat + new Vector3(Mathf.Cos(ang) * 9.2f, 0f, Mathf.Sin(ang) * 9.2f);
                var tg = Anak(grup, "TungkuKubahBoss_" + i); tg.transform.position = pos;
                Hias(tg, PrimitiveType.Cylinder, "PilarRitual", new Vector3(0f, 0.65f, 0f), new Vector3(0.85f, 0.65f, 0.85f), batu, true);
                var nyala = Anak(tg, "ApiBoss"); nyala.transform.localPosition = new Vector3(0f, 1.32f, 0f); nyala.transform.localScale = new Vector3(0.62f, 1.05f, 0.62f);
                nyala.AddComponent<MeshFilter>().sharedMesh = apiMesh;
                var nmr = nyala.AddComponent<MeshRenderer>(); nmr.sharedMaterial = apiMat; nmr.shadowCastingMode = ShadowCastingMode.Off;
                var lGo = Anak(tg, "CahayaBoss"); lGo.transform.localPosition = new Vector3(0f, 2.0f, 0f);
                var l = lGo.AddComponent<Light>(); l.type = LightType.Point; l.range = 18f; l.intensity = 4.6f; l.color = new Color(1f, 0.44f, 0.14f); l.shadows = LightShadows.None;
                lampuBoss.Add(l);
                JumlahDetailGuaV11++;
            }

            // 4. 36 spora gua bercahaya melayang di UG1 & UG3.
            for (int i = 0; i < 36; i++)
            {
                Vector3 baseP = i < 18 ? ug1Pusat : ug3Pusat;
                float a = i * 1.15f, r = 2.0f + (i % 6) * 1.6f;
                var sp = Anak(grup, "SporaGuaV11_" + i);
                sp.transform.position = baseP + new Vector3(Mathf.Cos(a) * r, 1.2f + (i % 4) * 0.9f, Mathf.Sin(a) * r);
                var mSp = i < 18 ? bioCyan : apiMat;
                Hias(sp, PrimitiveType.Sphere, "Butir", Vector3.zero, Vector3.one * 0.08f, mSp, false).GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                sporaList.Add(sp.transform);
                JumlahDetailGuaV11++;
            }

            var comp = grup.AddComponent<AtmosferGuaHidup>();
            comp.Pasang(lampuJamur.ToArray(), lampuBunker.ToArray(), lampuBoss.ToArray(), sporaList.ToArray());
            EditorUtility.SetDirty(comp);
        }
    }
}
