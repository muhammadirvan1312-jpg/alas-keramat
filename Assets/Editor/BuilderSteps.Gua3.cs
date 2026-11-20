using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using J = AlasKeramat.Core.ResepKarakter.JenisKarakter;
namespace AlasKeramat.EditorTools
{
    // Langkah 7t/7u (T10a-c): arena boss Kubah Akar (Genderuwo + Ki Rogo + 8 tungku), gerbang permukaan, lore dunia, Altar Beringin
    // (RitualAltar + siege), UI layar hitam, layar lore + jurnal, bar atas.
    public static partial class NusantaraMasterBuilder
    {
        public const string NamaGenderuwo = "Genderuwo", NamaKiRogo = "KiRogo";

        static void PasangBaraTungku(GameObject api, Material matBara)
        {
            if (api == null || matBara == null) return;
            var ember = Anak(api, "Bara");
            ember.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            var ps = ember.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 2.0f;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.45f, 1.15f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.14f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.55f, 0.14f, 0.95f), new Color(1f, 0.22f, 0.04f, 0.75f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 24;
            var em = ps.emission; em.rateOverTime = 11f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 20f; sh.radius = 0.22f;
            var col = ps.colorOverLifetime; col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(new Color(1f, 0.72f, 0.22f), 0f), new GradientColorKey(new Color(0.95f, 0.28f, 0.04f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.95f, 0.15f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
            var pr = ember.GetComponent<ParticleSystemRenderer>();
            pr.sharedMaterial = matBara;
            pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        static void BangunArena(GameObject k, RuangGua k3, Material akarMat)
        {
            var arena = Anak(k, "Arena");
            arena.transform.localPosition = k3.pos;
            var batu = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_BatuArena.mat") ?? AmbilMat("M_Batu");
            var apiMat = MatEmisi("M_ApiTungku", "#FF6B1A", new Color(6.0f, 2.2f, 0.35f), false);
            var baraMat = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_Banaspati.mat") ?? apiMat;
            var meshPilar = new[] { SimpanMeshGua(ProcGua.PilarAkar("PilarAkar_0", 22f, 1.45f, 101)), SimpanMeshGua(ProcGua.PilarAkar("PilarAkar_1", 22f, 1.45f, 202)) };
            var meshTungku = SimpanMeshGua(ProcGua.TungkuBatu("TungkuBatu", 11));
            var meshApi = SimpanMeshGua(ProcGua.ApiTungku("ApiTungku", 21));
            var meshPanggung = SimpanMeshGua(ProcGua.AltarPanggung("AltarPanggung", 4.4f, 1.0f, 31));
            var meshKepompong = SimpanMeshGua(ProcGua.KepompongAkar("KepompongAkarMesh", 41));
            var meshDindingAkar = SimpanMeshGua(ProcGua.DindingAkar("DindingAkarBoss", 10.5f, 6.2f, 51));
            var tungku = new TungkuObor[8];
            var panggil = new Transform[4];
            for (int i = 0; i < 8; i++)
            {
                float a = (i + 0.5f) * 45f * Mathf.Deg2Rad;
                var pilar = Anak(arena, "PilarAkar_" + i);
                pilar.transform.localPosition = new Vector3(Mathf.Sin(a) * 13f, 0f, Mathf.Cos(a) * 13f);
                pilar.transform.localRotation = Quaternion.Euler(0f, i * 47f, 0f);
                pilar.layer = GameLayers.Ground;
                pilar.AddComponent<MeshFilter>().sharedMesh = meshPilar[i % 2];
                pilar.AddComponent<MeshRenderer>().sharedMaterial = akarMat;
                var pc = pilar.AddComponent<CapsuleCollider>(); pc.radius = 1.4f; pc.height = 22f; pc.center = Vector3.up * 11f;
                for (int u = 0; u < 2; u++)
                {
                    float ua = (u * 1.8f + i) * 0.9f;
                    var uratPilar = Hias(pilar, PrimitiveType.Capsule, "UratPilar_" + u, new Vector3(Mathf.Cos(ua) * 1.18f, 4.2f + u * 2.4f, Mathf.Sin(ua) * 1.18f), new Vector3(0.22f, 3.8f, 0.22f), _matUrat, false);
                    uratPilar.transform.localRotation = Quaternion.Euler(16f * (u == 0 ? 1f : -1f), ua * Mathf.Rad2Deg, 18f);
                }
                float b = i * 45f * Mathf.Deg2Rad;
                float radT = (i % 2 == 0) ? 10.5f : 15.5f;
                var dasar = Anak(arena, "Tungku_" + i);
                dasar.transform.localPosition = new Vector3(Mathf.Sin(b) * radT, 0f, Mathf.Cos(b) * radT);
                dasar.layer = GameLayers.Interactable; dasar.tag = GameTags.Interactable;
                dasar.AddComponent<MeshFilter>().sharedMesh = meshTungku;
                dasar.AddComponent<MeshRenderer>().sharedMaterial = batu;
                var tc = dasar.AddComponent<BoxCollider>(); tc.size = new Vector3(1.2f, 1.3f, 1.2f); tc.center = new Vector3(0f, 0.65f, 0f);
                var api = Anak(dasar, "Api");
                api.transform.localPosition = new Vector3(0f, 1.05f, 0f);
                api.transform.localRotation = Quaternion.Euler(0f, b * 45f, 0f);
                api.transform.localScale = Vector3.one * 1.10f;
                api.AddComponent<MeshFilter>().sharedMesh = meshApi;
                api.AddComponent<MeshRenderer>().sharedMaterial = apiMat;
                PasangBaraTungku(api, baraMat);
                var lTungku = Lampu(dasar, "Cahaya", new Vector3(0f, 2.0f, 0f), new Color(1f, 0.50f, 0.18f), 28f, 5.2f);
                tungku[i] = dasar.AddComponent<TungkuObor>();
                if (i % 2 == 1) panggil[i / 2] = dasar.transform;
            }
            var panggung = Anak(arena, "Panggung");
            panggung.layer = GameLayers.Ground;
            panggung.AddComponent<MeshFilter>().sharedMesh = meshPanggung;
            panggung.AddComponent<MeshRenderer>().sharedMaterial = batu;
            var pgc = panggung.AddComponent<BoxCollider>(); pgc.size = new Vector3(7.6f, 1.0f, 7.6f); pgc.center = new Vector3(0f, 0.5f, 0f);
            var tutup = Anak(arena, "KepompongAkar");
            tutup.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            tutup.AddComponent<MeshFilter>().sharedMesh = meshKepompong;
            tutup.AddComponent<MeshRenderer>().sharedMaterial = akarMat;
            for (int r = 0; r < 6; r++)
            {
                float rad = r * 60f * Mathf.Deg2Rad;
                var lilit = Hias(tutup, PrimitiveType.Cylinder, "LilitUrat_" + r, new Vector3(Mathf.Sin(rad) * 0.62f, 1.0f, Mathf.Cos(rad) * 0.62f), new Vector3(0.11f, 0.65f, 0.11f), _matUrat, false);
                lilit.transform.localRotation = Quaternion.Euler(24f, r * 60f, 16f);
            }
            Lampu(arena, "CahayaAltarBoss", new Vector3(-1.5f, 5.5f, -1f), new Color(1f, 0.55f, 0.25f), 40f, 7.5f);
            Lampu(arena, "CahayaArenaDepan", new Vector3(-4.5f, 5.5f, -5.5f), new Color(1f, 0.48f, 0.20f), 38f, 6.0f);
            Lampu(arena, "CahayaKubahAtas", new Vector3(-3f, 10.5f, 0f), ProcTex.Hex("#5278AD"), 52f, 3.8f);
            var meshAkarBoss = MuatPropScan("AkarScan");
            var matAkarScan = MaterialSolidScan("AkarAsli", "M_AkarScan", new Color(0.78f, 0.74f, 0.68f, 1f), 0.24f) ?? akarMat;
            var meshBatuBoss = MuatPropScan("BatuBesarScan") ?? MuatPropScan("BatuLumutA");
            var matBatuBoss = MaterialSolidScan("BatuBesarAsli", "M_BatuArenaScan", ProcTex.Hex("#524846"), 0.44f) ?? batu;
            for (int i = 0; i < 8 && (meshAkarBoss != null || meshBatuBoss != null); i++)
            {
                float a = (i + 0.25f) * 45f * Mathf.Deg2Rad;
                float rProp = (i % 2 == 0) ? 6.2f : 11.8f;
                var prp = Anak(arena, "PropArena_" + i);
                prp.transform.localPosition = new Vector3(Mathf.Sin(a) * rProp, -0.05f, Mathf.Cos(a) * rProp);
                prp.transform.localRotation = Quaternion.Euler(0f, i * 63f, 0f);
                prp.transform.localScale = Vector3.one * (0.85f + (i % 2) * 0.35f);
                bool ak = i % 2 == 1 && meshAkarBoss != null;
                prp.AddComponent<MeshFilter>().sharedMesh = ak ? meshAkarBoss : (meshBatuBoss ?? meshAkarBoss);
                prp.AddComponent<MeshRenderer>().sharedMaterial = ak ? matAkarScan : matBatuBoss;
            }
            var shKabut = Shader.Find("AlasKeramat/KabutLokal");
            if (shKabut != null)
            {
                var mKabut = new Material(shKabut) { name = "M_KabutArena" };
                mKabut.SetTexture("_BaseMap", Tex("batu"));
                mKabut.SetColor("_BaseColor", new Color(0.28f, 0.18f, 0.16f, 0.15f));
                AssetDatabase.CreateAsset(mKabut, FolderMat + "/M_KabutArena.mat");
                mKabut = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_KabutArena.mat");
                for (int i = 0; i < 6; i++)
                {
                    float a = i * 60f * Mathf.Deg2Rad;
                    var ka = Hias(arena, PrimitiveType.Quad, "KabutArena_" + i, new Vector3(Mathf.Sin(a) * 9f, 2.4f, Mathf.Cos(a) * 9f), new Vector3(14f, 5f, 1f), mKabut, false);
                    ka.transform.localRotation = Quaternion.Euler(0f, i * 35f, 0f);
                }
            }
            var isi = Wadah(arena, "Paku_3", "Paku Keramat", new Vector3(0f, 1f, 0f), new Vector3(0.12f, 0.5f, 0.12f), AmbilMatGen("M_PakuKeramat"), new[] { ItemID.PakuKeramat3 }, new[] { 1 }, 0, true);
            var dinding = Anak(arena, "DindingAkar");
            dinding.transform.localPosition = new Vector3(0f, 0f, 14.5f);
            dinding.layer = GameLayers.Ground;
            dinding.AddComponent<MeshFilter>().sharedMesh = meshDindingAkar;
            dinding.AddComponent<MeshRenderer>().sharedMaterial = akarMat;
            var dc = dinding.AddComponent<BoxCollider>(); dc.size = new Vector3(10.5f, 6.2f, 1.2f); dc.center = new Vector3(0f, 3.1f, 0f);

            var ki = BuatKarakter(J.KiRogo, NamaKiRogo, ProceduralAnimator.Mode.Normal, false);
            ki.transform.SetParent(arena.transform, false);
            ki.transform.localPosition = new Vector3(0f, 1f, 2.5f); ki.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var kiRogo = ki.AddComponent<KiRogoPanggung>();

            var g = BuatKarakter(J.Genderuwo, NamaGenderuwo, ProceduralAnimator.Mode.Normal, false);
            g.transform.SetParent(arena.transform, false);
            g.transform.localPosition = new Vector3(0f, 0.1f, 17.5f); g.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            g.tag = GameTags.Enemy;
            SetLayer(g, GameLayers.Enemy);
            float tinggi = ResepKarakter.Ambil(J.Genderuwo).tinggi;
            var cc = g.AddComponent<CharacterController>();
            cc.height = tinggi; cc.radius = 0.75f; cc.center = new Vector3(0f, tinggi * 0.5f, 0f); cc.slopeLimit = 45f; cc.stepOffset = 0.4f;
            var punggung = Lampu(g, "Punggung", new Vector3(0f, tinggi * 0.6f, -0.7f), new Color(1f, 0.55f, 0.1f), 4f, 3f);
            var akar = Hias(arena, PrimitiveType.Cylinder, "AkarLempar", Vector3.zero, new Vector3(0.35f, 1.2f, 0.35f), akarMat, false);
            akar.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var akarInduk = Anak(arena, "AkarLemparInduk"); akar.transform.SetParent(akarInduk.transform, false);
            var debu = Hias(arena, PrimitiveType.Cylinder, "CincinDebu", Vector3.zero, Vector3.one, MatEmisi("M_DebuCincin", "#6B5A48A0", Color.black, true), false);
            var boss = g.AddComponent<BossGenderuwo>();
            boss.Pasang(kiRogo, tungku, akarInduk.transform, debu.transform, punggung);
            for (int i = 0; i < 8; i++) tungku[i].Pasang(tungku[i].transform.Find("Cahaya").GetComponent<Light>(), tungku[i].transform.Find("Api").gameObject, i % 2 == 0 ? i / 2 + 1 : 0, kiRogo);
            kiRogo.Pasang(boss, tungku, dinding, tutup, isi.gameObject, panggil);
            foreach (var t in tungku) EditorUtility.SetDirty(t);
            EditorUtility.SetDirty(boss); EditorUtility.SetDirty(kiRogo);
        }

        // ---------- Gerbang permukaan & lore dunia ----------
        static void BangunPermukaanGua()
        {
            _kompleks = null;
            var induk = Anak(_root, "GerbangPermukaan");
            var kayu = AmbilMat("M_Kayu"); var logam = AmbilMat("M_LogamGelap"); var batu = AmbilMat("M_Batu");
            var tanah = MatPolos("M_TanahGembur", "#2E2219");

            // Tanah Gembur Bertanda Silang -> palka bunker.
            var gali = Anak(induk, "TanahGali");
            gali.transform.position = Permukaan(PosGaliBunker.x, PosGaliBunker.z, 0f);
            gali.layer = GameLayers.Interactable; gali.tag = GameTags.Interactable;
            var gc = gali.AddComponent<BoxCollider>(); gc.size = new Vector3(4f, 1.2f, 4f); gc.center = Vector3.up * 0.4f;
            var gundukan = Hias(gali, PrimitiveType.Sphere, "Gundukan", new Vector3(0f, 0f, 0f), new Vector3(5f, 0.6f, 5f), tanah, false);
            for (int i = 0; i < 2; i++) { var p = Hias(gali, PrimitiveType.Cylinder, "Pancang_" + i, new Vector3(0f, 0.8f, 0f), new Vector3(0.12f, 1f, 0.12f), kayu, false); p.transform.localRotation = Quaternion.Euler(0f, 45f + i * 90f, 35f); }
            var palka = Anak(gali, "Palka");
            Hias(palka, PrimitiveType.Cube, "PintuBaja", new Vector3(0f, 0.05f, 0f), new Vector3(1.6f, 0.1f, 1.6f), logam, false);
            Pintu(palka, "Pintu_Palka", Vector3.zero, "TURUN KE BUNKER", Dunia(UndergroundId.UG2_Bunker, new Vector3(0f, 0f, -53.5f)), 0f, WorldCaveAndSaveManager.B_Palka, false, "");
            palka.SetActive(false);
            var tg = gali.AddComponent<TanahGali>(); tg.Pasang(gundukan.transform, palka); EditorUtility.SetDirty(tg);

            // Mulut gua: UG1 (lembah bambu) menghadap +X, UG3 (celah kuil) menghadap -Z.
            Pintu(induk, "Masuk_UG1", Permukaan(262f, -276f, 0f), "MASUK GUA", Dunia(UndergroundId.UG1_GuaSungai, new Vector3(-58f, 0f, 0f)), 90f, 0, false, "");
            Pintu(induk, "Masuk_UG3", Permukaan(0f, 686.5f, 0f), "MASUK CELAH AKAR", Dunia(UndergroundId.UG3_KuilAkar, new Vector3(0f, 0f, -68f)), 0f, 0, false, "");

            // Zipline Jurang Kapur: A (selatan, puncak batu +4) <-> B (utara, tiang pohon).
            var tali = MatPolos("M_TaliZipline", "#6B5634");
            var a = BuatJangkar(induk, "Zipline_A", PosZiplineA, batu, tali, true);
            var b = BuatJangkar(induk, "Zipline_B", PosZiplineB, kayu, null, false);
            a.Pasang(b, true, a.GetComponent<LineRenderer>()); b.Pasang(a, false, null);
            EditorUtility.SetDirty(a); EditorUtility.SetDirty(b);

            // Lore & Cangkul di dunia (04 §6, 03 Zona 3).
            var kertas = MatPolos("M_KertasLore", "#D9CFAE");
            Wadah(induk, "Lore2_KepalaDesa", "Catatan Kepala Desa", Permukaan(564f, 229f, 0f), new Vector3(0.5f, 0.12f, 0.4f), kertas, new ItemID[0], new int[0], (int)LoreID.L02, false);
            Wadah(induk, "Cangkul_Desa", "Cangkul", Permukaan(547f, 146f, 0f), new Vector3(0.15f, 1.1f, 0.15f), kayu, new[] { ItemID.Cangkul }, new[] { 1 }, 0, true);
            Wadah(induk, "Lore3_PapanDarah", "Papan Kayu Berdarah", Permukaan(-452f, 320f, 0f), new Vector3(1.6f, 1.1f, 0.12f), AmbilMat("M_CatMerahPudar"), new ItemID[0], new int[0], (int)LoreID.L03, false);
            Wadah(induk, "Lore7_BukuJaga", "Buku Jaga Mercusuar", Permukaan(-186f, -871f, 0f), new Vector3(0.4f, 0.1f, 0.3f), kertas, new ItemID[0], new int[0], (int)LoreID.L07, false);
            Wadah(induk, "Lore8_SuratNingsih", "Surat Ningsih", Permukaan(204f, -346f, 0f), new Vector3(0.3f, 0.05f, 0.22f), kertas, new ItemID[0], new int[0], (int)LoreID.L08, false);
            Wadah(induk, "Lore10_DaftarTumbal", "Daftar Tumbal", Permukaan(610f, 240f, 0f), new Vector3(0.4f, 0.08f, 0.3f), kertas, new ItemID[0], new int[0], (int)LoreID.L10, false);
            Catat("Gerbang permukaan: tanah gali bunker, 2 mulut gua, zipline jurang, 5 lore dunia + Cangkul");
        }

        static JangkarZipline BuatJangkar(GameObject induk, string nama, Vector3 xz, Material mat, Material tali, bool utama)
        {
            var dasar = Anak(induk, nama);
            dasar.transform.position = Permukaan(xz.x, xz.z, 0f);
            Hias(dasar, PrimitiveType.Cylinder, "Tiang", new Vector3(0f, 2f, 0f), new Vector3(0.9f, 2f, 0.9f), mat, true);
            Hias(dasar, PrimitiveType.Cube, "KainMerah", new Vector3(0f, 3.2f, 0.46f), new Vector3(0.5f, 0.3f, 0.04f), AmbilMat("M_CatMerahPudar"), false);
            var jangkar = Anak(dasar, "Jangkar");
            jangkar.transform.localPosition = new Vector3(0f, 3.8f, 0f);
            jangkar.layer = GameLayers.Interactable; jangkar.tag = GameTags.ZiplineAnchor;
            var sc = jangkar.AddComponent<SphereCollider>(); sc.radius = 2.2f; sc.center = new Vector3(0f, -2.6f, 0f);
            var j = jangkar.AddComponent<JangkarZipline>();
            if (utama)
            {
                var lr = jangkar.AddComponent<LineRenderer>();
                lr.sharedMaterial = tali; lr.widthMultiplier = 0.05f; lr.positionCount = 2; lr.useWorldSpace = true; lr.enabled = false;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            return j;
        }

        // ---------- 7u: Altar Beringin, siege, UI ----------
        static void LangkahBossAltar()
        {
            var altar = GameObject.Find("Altar");
            var induk = Anak(_root, "RitualBeringin");
            Vector3 pos = altar != null ? altar.transform.position : Permukaan(0f, -18f, 0f);
            var titik = Anak(induk, "AltarInteraksi");
            titik.transform.position = pos;
            titik.layer = GameLayers.Interactable; titik.tag = GameTags.Altar;
            var bc = titik.AddComponent<BoxCollider>(); bc.size = new Vector3(2.8f, 1.4f, 1.6f); bc.center = Vector3.up * 0.7f;
            var lubang = new Light[3];
            for (int i = 0; i < 3; i++) { lubang[i] = Lampu(titik, "Lubang_" + (i + 1), new Vector3(-0.7f + i * 0.7f, 1.15f, 0f), new Color(1f, 0.78f, 0.25f), 3f, 2.5f); lubang[i].enabled = false; }
            _kompleks = null;
            var ukiran = Wadah(induk, "Lore12_Prasasti", "Prasasti Batu Langit", pos + new Vector3(0f, 0f, -1.1f), new Vector3(1.2f, 0.5f, 0.1f), AmbilMat("M_Batu"), new ItemID[0], new int[0], (int)LoreID.L12, false).gameObject;
            ukiran.SetActive(false);
            var emas = Hias(induk, PrimitiveType.Cylinder, "CahayaEmas", Permukaan(0f, 0f, 20f), new Vector3(7f, 22f, 7f), MatEmisi("M_CahayaEmas", "#FFD37040", new Color(2.5f, 1.8f, 0.6f), true), false);
            emas.SetActive(false);
            var suar = Anak(induk, "SuarHijau");
            suar.transform.position = Permukaan(0f, -840f, 0f);
            Hias(suar, PrimitiveType.Cylinder, "Kolom", new Vector3(0f, 20f, 0f), new Vector3(0.7f, 20f, 0.7f), MatEmisi("M_SuarHijau", "#3CFF6A80", new Color(0.4f, 3f, 0.8f), true), false);
            Lampu(suar, "Cahaya", Vector3.up * 2f, new Color(0.3f, 1f, 0.45f), 30f, 4f);
            suar.SetActive(false);
            var bay = BuatKarakter(J.KiRogo, "BayanganKiRogo", ProceduralAnimator.Mode.Normal, false);
            bay.transform.SetParent(induk.transform, false);
            bay.transform.position = Permukaan(0f, 20f, 0f); bay.transform.rotation = Quaternion.Euler(0f, 180f, 0f); bay.transform.localScale = Vector3.one * 1.4f;
            bay.SetActive(false);
            var ra = titik.AddComponent<RitualAltar>();
            ra.Pasang(lubang, ukiran, emas, suar, bay);
            EditorUtility.SetDirty(ra);
            BangunUILore();
            Catat("Ritual Beringin: altar 3 lubang, siege 60 detik (9 gelombang), cahaya emas, suar hijau SAR; layar lore + jurnal, bar boss");
        }

        static Image BuatLayarHitam()
        {
            var kanvas = _ui != null ? _ui.transform.Find("Canvas_HUD") : null;
            if (kanvas == null) { Debug.LogError("[AK][Builder] Canvas_HUD tidak ada, layar hitam gua dilewati"); return null; }
            var go = UIStretch(kanvas.gameObject, "LayarHitamGua", Vector2.zero, Vector2.one);
            var img = go.AddComponent<Image>(); img.color = new Color(0f, 0f, 0f, 0f); img.raycastTarget = false; img.enabled = false;
            go.transform.SetAsLastSibling();
            return img;
        }

        static void BangunUILore()
        {
            var kanvas = _ui != null ? _ui.transform.Find("Canvas_HUD") : null;
            if (kanvas == null) { Debug.LogError("[AK][Builder] Canvas_HUD tidak ada, UI lore dilewati"); return; }
            var font = ProceduralAssetFactory.GetSafeFont();
            Text tx;
            Color kertas = ProcTex.Hex("#E9DFC4F5"), tinta = ProcTex.Hex("#2E2014"), coklat = ProcTex.Hex("#5C4529");

            // Bar atas (HP boss / waktu siege).
            var bar = UIStretch(kanvas.gameObject, "BarAtas", new Vector2(0.3f, 0.9f), new Vector2(0.7f, 0.94f));
            bar.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);
            var isi = UIStretch(bar, "Isi", Vector2.zero, Vector2.one);
            var isiImg = isi.AddComponent<Image>(); isiImg.color = new Color(0.62f, 0.08f, 0.06f); isiImg.raycastTarget = false;
            var label = BuatTeks(bar, "Label", "", font, 26, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700f, 40f));
            label.color = Color.white; label.gameObject.AddComponent<Outline>();
            var ba = bar.AddComponent<BarAtas>(); ba.Pasang(bar, isi.GetComponent<RectTransform>(), isiImg, label); EditorUtility.SetDirty(ba);

            var akar = Anak(kanvas.gameObject, "LayarLore");
            var layar = akar.AddComponent<LayarLore>();
            // Panel baca.
            var pb = UIStretch(akar, "PanelBaca", new Vector2(0.18f, 0.08f), new Vector2(0.82f, 0.92f));
            pb.AddComponent<Image>().color = kertas;
            var judul = BuatTeks(pb, "Judul", "", font, 34, new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(1100f, 80f));
            judul.color = ProcTex.Hex("#6A1E12");
            var teks = BuatTeks(pb, "Isi", "", font, 28, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100f, 100f));
            var rt = teks.GetComponent<RectTransform>(); rt.anchorMin = new Vector2(0.07f, 0.18f); rt.anchorMax = new Vector2(0.93f, 0.84f); rt.offsetMin = rt.offsetMax = Vector2.zero;
            teks.alignment = TextAnchor.UpperLeft; teks.horizontalOverflow = HorizontalWrapMode.Wrap; teks.verticalOverflow = VerticalWrapMode.Truncate; teks.color = tinta; teks.lineSpacing = 1.15f;
            var tutup = TombolPanel(pb, "TombolTutup", "TUTUP", font, new Vector2(-170f, 70f), new Vector2(280f, 84f), ProcTex.Hex("#7A1F1F"), out tx);
            UnityEventTools.AddPersistentListener(tutup.onClick, layar.Tutup);
            var keJurnal = TombolPanel(pb, "TombolJurnal", "JURNAL", font, new Vector2(170f, 70f), new Vector2(280f, 84f), coklat, out tx);
            UnityEventTools.AddPersistentListener(keJurnal.onClick, layar.BukaJurnal);
            // Panel jurnal.
            var pj = UIStretch(akar, "PanelJurnal", new Vector2(0.18f, 0.08f), new Vector2(0.82f, 0.92f));
            pj.AddComponent<Image>().color = kertas;
            var jj = BuatTeks(pj, "Judul", "JURNAL", font, 34, new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(1100f, 70f));
            jj.color = ProcTex.Hex("#6A1E12");
            var slot = new Text[LoreLibrary.Jumlah];
            for (int i = 0; i < LoreLibrary.Jumlah; i++)
            {
                int kol = i / 6, bar6 = i % 6;
                var b = TombolPanel(pj, "Lore_" + (i + 1), "#" + (i + 1), font, Vector2.zero, new Vector2(560f, 72f), coklat, out slot[i]);
                var brt = b.GetComponent<RectTransform>();
                brt.anchorMin = brt.anchorMax = new Vector2(kol == 0 ? 0.27f : 0.73f, 1f);
                brt.anchoredPosition = new Vector2(0f, -150f - bar6 * 86f);
                slot[i].fontSize = 22; slot[i].horizontalOverflow = HorizontalWrapMode.Wrap;
                UnityEventTools.AddIntPersistentListener(b.onClick, layar.PilihJurnal, i);
            }
            var tutupJ = TombolPanel(pj, "TombolTutup", "TUTUP", font, new Vector2(0f, 60f), new Vector2(280f, 84f), ProcTex.Hex("#7A1F1F"), out tx);
            UnityEventTools.AddPersistentListener(tutupJ.onClick, layar.Tutup);
            layar.Pasang(pb, judul, teks, pj, jj, slot);
            pb.SetActive(false); pj.SetActive(false);
            EditorUtility.SetDirty(layar);

            // Tombol JURNAL di HUD (kanan di bawah TAS/BUKU).
            var tj = TombolPanel(kanvas.gameObject, "TombolJurnal", "JURNAL", font, Vector2.zero, new Vector2(116f, 58f), coklat, out tx);
            var trt = tj.GetComponent<RectTransform>(); trt.anchorMin = trt.anchorMax = new Vector2(1f, 1f); trt.anchoredPosition = new Vector2(-74f, -482f);
            tx.fontSize = 22;
            UnityEventTools.AddPersistentListener(tj.onClick, layar.BukaJurnal);
            akar.transform.SetAsLastSibling();
            var hitam = kanvas.Find("LayarHitamGua"); if (hitam != null) hitam.SetAsLastSibling();
        }
    }
}
