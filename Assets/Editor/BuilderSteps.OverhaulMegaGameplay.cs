using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.UI;
using AlasKeramat.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace AlasKeramat.EditorTools
{
    // Langkah 7z18b: Mega Overhaul Gameplay — 4 Glider Bambu (Hang Glider SOTF), 6 Spot Memancing Aktif,
    // 8 Peti Logistik Parasut Terkunci, 5 Kuali Jamu Nusantara, 6 Kentongan Desa Interaktif, 16 Tungku Api Kuno,
    // Jurnal Eksplorasi 22 Lokasi + Banner Penemuan/Glider/Bahaya HUD.
    public static partial class NusantaraMasterBuilder
    {
        public static int JumlahGlider, JumlahSpotPancing, JumlahPetiSupply, JumlahKualiJamu, JumlahKentongan, JumlahTungkuApi;

        static void LangkahOverhaulMegaGameplay()
        {
            JumlahGlider = 0; JumlahSpotPancing = 0; JumlahPetiSupply = 0;
            JumlahKualiJamu = 0; JumlahKentongan = 0; JumlahTungkuApi = 0;

            var lm = GameObject.Find(NamaRoot + "/World/Landmark") ?? GameObject.Find(NamaRoot + "/Landmark") ?? _root;
            var induk = Anak(lm, "MegaOverhaulGameplayV2");
            var kayu = AmbilMat("M_Kayu");
            var batu = AmbilMat("M_Batu");
            var kain = AmbilMat("M_Kain") ?? MatPolos("M_KainGlider", "#C84B28");
            var besi = AmbilMat("M_LogamKarat") ?? MatPolos("M_PetiLogistik", "#3B4A3E");
            var parasut = MatPolos("M_ParasutMerah", "#C93228");
            var apiMat = SimpanMat(SafeMaterialFactory.CreatePBR("M_NyalaTungkuKuno", new Color(1f, 0.58f, 0.16f), 0f, 0.1f, null, null, new Color(5.2f, 2.3f, 0.5f)));

            PasangGliderBambu(induk, kayu, kain);
            PasangSpotMemancingAktif(induk, kayu, parasut);
            PasangPetiSupplyParasut(induk, besi, parasut);
            PasangKualiJamuNusantara(induk, besi, apiMat);
            PasangKentonganInteraktif(induk, kayu);
            PasangTungkuApiKuno(induk, batu, apiMat);
            PasangJurnalEksplorasiDanHUD();

            Catat("Mega Overhaul Gameplay V2: " + JumlahGlider + " glider bambu, " + JumlahSpotPancing + " spot pancing, "
                + JumlahPetiSupply + " peti supply parasut, " + JumlahKualiJamu + " kuali jamu, "
                + JumlahKentongan + " kentongan interaktif, " + JumlahTungkuApi + " tungku api kuno & Jurnal 22 Lokasi");
        }

        static void PasangGliderBambu(GameObject induk, Material kayu, Material kain)
        {
            var grup = Anak(induk, "GliderBambuRacks");
            var daftar = new[]
            {
                new { nama = "Puncak Menara Pengawas Api", pos = new Vector3(-195f, WorldTerrainGenerator.SampleHeight(-195f, -240f) + 12.4f, -237f) },
                new { nama = "Pos Gunung Kapur", pos = new Vector3(-576f, WorldTerrainGenerator.SampleHeight(-576f, 616f) + 0.5f, 616f) },
                new { nama = "Tebing Air Terjun Rimba", pos = new Vector3(302f, WorldTerrainGenerator.SampleHeight(302f, -148f) + 0.5f, -148f) },
                new { nama = "Teras Atas Candi Kuno", pos = new Vector3(-260f, WorldTerrainGenerator.SampleHeight(-260f, 122f) + 0.5f, 122f) }
            };
            for (int i = 0; i < daftar.Length; i++)
            {
                var go = Anak(grup, "GliderBambu_" + i);
                go.transform.position = daftar[i].pos;
                go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
                var bc = go.AddComponent<BoxCollider>(); bc.size = new Vector3(2.8f, 1.8f, 2.2f); bc.center = new Vector3(0f, 0.9f, 0f);
                Hias(go, PrimitiveType.Cube, "RakLuncur", new Vector3(0f, 0.35f, 0f), new Vector3(2.2f, 0.2f, 1.8f), kayu, false);
                var sayap = Anak(go, "SayapGlider");
                sayap.transform.localPosition = new Vector3(0f, 1.3f, 0f);
                Hias(sayap, PrimitiveType.Cube, "LayarKiri", new Vector3(-1.15f, 0f, -0.25f), new Vector3(2.3f, 0.06f, 1.35f), kain, false).transform.localRotation = Quaternion.Euler(0f, 16f, 6f);
                Hias(sayap, PrimitiveType.Cube, "LayarKanan", new Vector3(1.15f, 0f, -0.25f), new Vector3(2.3f, 0.06f, 1.35f), kain, false).transform.localRotation = Quaternion.Euler(0f, -16f, -6f);
                Hias(sayap, PrimitiveType.Cylinder, "PalangBambu", new Vector3(0f, -0.25f, 0f), new Vector3(0.06f, 1.9f, 0.06f), kayu, false).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                var g = go.AddComponent<GliderBambuAndi>();
                g.Pasang(daftar[i].nama, sayap.transform);
                EditorUtility.SetDirty(g);
                JumlahGlider++;
            }
        }

        static void PasangSpotMemancingAktif(GameObject induk, Material kayu, Material pelampungMat)
        {
            var grup = Anak(induk, "SpotPancingBambu");
            var spots = new[]
            {
                new { nama = "Dermaga Barat", x = -520f, z = -360f },
                new { nama = "Desa Nelayan Timur", x = 764f, z = -140f },
                new { nama = "Kolam Air Terjun", x = 308f, z = -172f },
                new { nama = "Tepi Sungai Jembatan", x = 266f, z = -224f },
                new { nama = "Pantai Bangkai Kapal", x = -104f, z = -846f },
                new { nama = "Sungai Kemah Rawa", x = -178f, z = -150f }
            };
            for (int i = 0; i < spots.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(spots[i].x, spots[i].z);
                var go = Anak(grup, "SpotPancing_" + i);
                go.transform.position = new Vector3(spots[i].x, y, spots[i].z);
                go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
                var bc = go.AddComponent<BoxCollider>(); bc.size = new Vector3(1.5f, 1.6f, 1.5f); bc.center = new Vector3(0f, 0.8f, 0f);
                var joran = Hias(go, PrimitiveType.Cylinder, "JoranBambu", new Vector3(0f, 1.15f, 0.65f), new Vector3(0.045f, 1.45f, 0.045f), kayu, false);
                joran.transform.localRotation = Quaternion.Euler(46f, 0f, 0f);
                var kail = Hias(go, PrimitiveType.Sphere, "Pelampung", new Vector3(0f, 0.35f, 1.9f), new Vector3(0.14f, 0.18f, 0.14f), pelampungMat, false);
                var p = go.AddComponent<PancingBambuAktif>();
                p.Pasang(spots[i].nama, kail.transform);
                EditorUtility.SetDirty(p);
                JumlahSpotPancing++;
            }
        }

        static void PasangPetiSupplyParasut(GameObject induk, Material besi, Material parasut)
        {
            var grup = Anak(induk, "PetiSupplyParasut");
            var lokasi = new[]
            {
                new { id = "heli", label = "Peti Logistik Heli", x = 22f, z = -432f, tier = 1 },
                new { id = "menara", label = "Peti Darurat Menara Api", x = -192f, z = -236f, tier = 2 },
                new { id = "gua_air", label = "Peti Relik Air Terjun", x = 326f, z = -156f, tier = 3 },
                new { id = "pasar", label = "Peti Bekal Pasar Hantu", x = 468f, z = 180f, tier = 2 },
                new { id = "riset", label = "Peti Ekspedisi Peneliti", x = -388f, z = 308f, tier = 3 },
                new { id = "megalit", label = "Peti Purbakala Megalit", x = 212f, z = 422f, tier = 3 },
                new { id = "dermaga", label = "Peti Kargo Dermaga Barat", x = -518f, z = -342f, tier = 2 },
                new { id = "kapur", label = "Peti Airdrop Gunung Kapur", x = -578f, z = 622f, tier = 3 }
            };
            for (int i = 0; i < lokasi.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(lokasi[i].x, lokasi[i].z);
                var go = Anak(grup, "PetiSupply_" + lokasi[i].id);
                go.transform.position = new Vector3(lokasi[i].x, y, lokasi[i].z);
                go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
                var bc = go.AddComponent<BoxCollider>(); bc.size = new Vector3(1.35f, 0.95f, 0.95f); bc.center = new Vector3(0f, 0.45f, 0f);
                Hias(go, PrimitiveType.Cube, "BadanPeti", new Vector3(0f, 0.35f, 0f), new Vector3(1.25f, 0.65f, 0.85f), besi, true);
                var engsel = Anak(go, "EngselTutup");
                engsel.transform.localPosition = new Vector3(0f, 0.68f, -0.42f);
                Hias(engsel, PrimitiveType.Cube, "TutupPeti", new Vector3(0f, 0.06f, 0.42f), new Vector3(1.28f, 0.14f, 0.88f), besi, false);
                var kainP = Hias(go, PrimitiveType.Cube, "ParasutJatuh", new Vector3(-0.9f, 0.18f, -0.75f), new Vector3(1.8f, 0.12f, 1.4f), parasut, false);
                kainP.transform.localRotation = Quaternion.Euler(8f, 32f, -10f);
                var ps = go.AddComponent<PetiSupplyTerkunci>();
                ps.Pasang(lokasi[i].id, lokasi[i].label, engsel.transform, lokasi[i].tier);
                EditorUtility.SetDirty(ps);
                JumlahPetiSupply++;
            }
        }

        static void PasangKualiJamuNusantara(GameObject induk, Material besi, Material apiMat)
        {
            var grup = Anak(induk, "KualiJamuNusantara");
            var shKabut = AmbilShaderAK("AlasKeramat/KabutLembut");
            Material matUap;
            if (shKabut != null)
            {
                var mU = new Material(shKabut) { name = "M_UapJamu" };
                SafeMaterialFactory.SetC(mU, "_BaseColor", new Color(0.82f, 0.90f, 0.78f, 0.22f));
                SafeMaterialFactory.SetF(mU, "_Mode", 1f);
                SafeMaterialFactory.SetF(mU, "_SkalaNoise", 4.2f);
                SafeMaterialFactory.SetF(mU, "_Kecepatan", 0.42f);
                matUap = SimpanMat(mU);
            }
            else
            {
                matUap = SimpanMat(SafeMaterialFactory.CreatePBR("M_UapJamu", new Color(0.80f, 0.88f, 0.76f, 0.18f), 0f, 0.1f, null, null, null, SurfaceMode.Transparent));
            }
            var matAsap = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_AsapHeli.mat") ?? matUap;
            var lokasi = new[]
            {
                new { nama = "Pos Heli", x = 15.5f, z = -444.5f },
                new { nama = "Kampung Sukamaju", x = 58f, z = -372f },
                new { nama = "Alun-Alun Desa Mati", x = 518f, z = 196f },
                new { nama = "Kamp Peneliti", x = -386f, z = 312f },
                new { nama = "Kuil Bambu Utara", x = -118f, z = 606f }
            };
            for (int i = 0; i < lokasi.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(lokasi[i].x, lokasi[i].z);
                var go = Anak(grup, "KualiJamu_" + i);
                go.transform.position = new Vector3(lokasi[i].x, y, lokasi[i].z);
                go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
                var bc = go.AddComponent<BoxCollider>(); bc.size = new Vector3(1.1f, 1.0f, 1.1f); bc.center = new Vector3(0f, 0.5f, 0f);
                Hias(go, PrimitiveType.Cylinder, "Tungku", new Vector3(0f, 0.22f, 0f), new Vector3(0.85f, 0.22f, 0.85f), besi, true);
                Hias(go, PrimitiveType.Sphere, "Kuali", new Vector3(0f, 0.52f, 0f), new Vector3(0.82f, 0.52f, 0.82f), besi, false);
                Hias(go, PrimitiveType.Sphere, "Bara", new Vector3(0f, 0.24f, 0f), new Vector3(0.42f, 0.18f, 0.42f), apiMat, false);
                var uap = Hias(go, PrimitiveType.Cylinder, "UapHerbal", new Vector3(0f, 0.96f, 0f), new Vector3(0.28f, 0.24f, 0.28f), matUap, false);
                uap.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                var ps = uap.AddComponent<ParticleSystem>();
                var pm = ps.main; pm.startLifetime = 2.2f; pm.startSpeed = 0.35f; pm.startSize = 0.42f; pm.startColor = new Color(0.88f, 0.94f, 0.84f, 0.34f); pm.maxParticles = 12; pm.prewarm = true;
                var pe = ps.emission; pe.rateOverTime = 4f;
                var psh = ps.shape; psh.shapeType = ParticleSystemShapeType.Circle; psh.radius = 0.18f;
                var pr = ps.GetComponent<ParticleSystemRenderer>(); pr.sharedMaterial = matAsap; pr.shadowCastingMode = ShadowCastingMode.Off;
                var kj = go.AddComponent<KualiJamuNusantara>();
                kj.Pasang(lokasi[i].nama, uap.transform);
                EditorUtility.SetDirty(kj);
                JumlahKualiJamu++;
            }
        }

        static void PasangKentonganInteraktif(GameObject induk, Material kayu)
        {
            var grup = Anak(induk, "KentonganDesa");
            var lokasi = new[]
            {
                new { nama = "Pos Heli", x = 15f, z = -436f },
                new { nama = "Kampung Sukamaju", x = 52f, z = -380f },
                new { nama = "Desa Mati", x = 514f, z = 202f },
                new { nama = "Pasar Hantu", x = 472f, z = 176f },
                new { nama = "Desa Nelayan Timur", x = 716f, z = -138f },
                new { nama = "Menara Pengawas Api", x = -192f, z = -242f }
            };
            for (int i = 0; i < lokasi.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(lokasi[i].x, lokasi[i].z);
                var go = Anak(grup, "Kentongan_" + i);
                go.transform.position = new Vector3(lokasi[i].x, y, lokasi[i].z);
                go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
                var bc = go.AddComponent<BoxCollider>(); bc.size = new Vector3(0.95f, 2.0f, 0.95f); bc.center = new Vector3(0f, 1.0f, 0f);
                Hias(go, PrimitiveType.Cylinder, "Tiang", new Vector3(-0.35f, 1.1f, 0f), new Vector3(0.11f, 1.1f, 0.11f), kayu, true);
                Hias(go, PrimitiveType.Cube, "Palang", new Vector3(0f, 2.15f, 0f), new Vector3(0.9f, 0.1f, 0.1f), kayu, false);
                var tabung = Hias(go, PrimitiveType.Cylinder, "TabungKentongan", new Vector3(0.12f, 1.5f, 0f), new Vector3(0.24f, 0.52f, 0.24f), kayu, false);
                var kd = go.AddComponent<KentonganDesaInteraktif>();
                kd.Pasang(lokasi[i].nama, tabung.transform);
                EditorUtility.SetDirty(kd);
                JumlahKentongan++;
            }
        }

        static void PasangTungkuApiKuno(GameObject induk, Material batu, Material apiMat)
        {
            var grup = Anak(induk, "TungkuApiKuno");
            Vector2[] pos =
            {
                new Vector2(-244f, 84f), new Vector2(-236f, 92f), new Vector2(-260f, 108f), new Vector2(-270f, 120f),
                new Vector2(204f, 418f), new Vector2(216f, 418f), new Vector2(210f, 428f), new Vector2(210f, 412f),
                new Vector2(464f, 174f), new Vector2(472f, 182f), new Vector2(512f, 196f), new Vector2(520f, 200f),
                new Vector2(-198f, -238f), new Vector2(-392f, 306f), new Vector2(-516f, -344f), new Vector2(-116f, 614f)
            };
            var apiMesh = ProcGua.ApiTungku("ApiTungkuKunoV6", 31);
            string pApiM = FolderLandmark + "/ApiTungkuKunoV6.asset";
            AssetDatabase.DeleteAsset(pApiM); AssetDatabase.CreateAsset(apiMesh, pApiM);
            apiMesh = AssetDatabase.LoadAssetAtPath<Mesh>(pApiM);
            for (int i = 0; i < pos.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(pos[i].x, pos[i].y);
                var go = Anak(grup, "TungkuApi_" + i);
                go.transform.position = new Vector3(pos[i].x, y, pos[i].y);
                go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
                var bc = go.AddComponent<BoxCollider>(); bc.size = new Vector3(0.95f, 1.35f, 0.95f); bc.center = new Vector3(0f, 0.65f, 0f);
                Hias(go, PrimitiveType.Cylinder, "PilarBatu", new Vector3(0f, 0.45f, 0f), new Vector3(0.62f, 0.45f, 0.62f), batu, true);
                Hias(go, PrimitiveType.Cylinder, "MangkukBatu", new Vector3(0f, 0.90f, 0f), new Vector3(0.76f, 0.12f, 0.76f), batu, false);
                var nyala = Anak(go, "NyalaApi");
                nyala.transform.localPosition = new Vector3(0f, 0.96f, 0f);
                nyala.transform.localScale = new Vector3(0.46f, 0.68f, 0.46f);
                nyala.AddComponent<MeshFilter>().sharedMesh = apiMesh;
                var nmr = nyala.AddComponent<MeshRenderer>(); nmr.sharedMaterial = apiMat; nmr.shadowCastingMode = ShadowCastingMode.Off;
                var lGo = Anak(go, "CahayaTungku"); lGo.transform.localPosition = new Vector3(0f, 1.35f, 0f);
                var l = lGo.AddComponent<Light>();
                l.type = LightType.Point; l.range = 10.5f; l.intensity = 2.6f; l.color = new Color(1f, 0.58f, 0.20f); l.shadows = LightShadows.None;
                var t = go.AddComponent<TungkuApiCandi>();
                t.Pasang("t" + i, nyala, l, i % 4 == 0);
                EditorUtility.SetDirty(t);
                JumlahTungkuApi++;
            }
        }

        static void PasangJurnalEksplorasiDanHUD()
        {
            var sis = _sistem ?? GameObject.Find(NamaRoot + "/Sistem") ?? _root;
            var jurnal = sis.GetComponent<JurnalEksplorasiPulau>() ?? sis.AddComponent<JurnalEksplorasiPulau>();
            jurnal.Pasang(new[]
            {
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Bangkai Heli", posisiXZ = new Vector2(8f, -448f), radius = 26f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Kampung Sukamaju", posisiXZ = new Vector2(56f, -376f), radius = 34f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Desa Mati", posisiXZ = new Vector2(540f, 204f), radius = 42f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Altar Beringin Agung", posisiXZ = new Vector2(0f, 0f), radius = 40f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Air Terjun Rimba", posisiXZ = new Vector2(312f, -165f), radius = 34f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Gua Air Terjun Rahasia", posisiXZ = new Vector2(326f, -158f), radius = 16f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Candi Runtuh Nusantara", posisiXZ = new Vector2(-265f, 115f), radius = 38f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Jembatan Batu Kuno", posisiXZ = new Vector2(-240f, 88f), radius = 22f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Jembatan Gantung Rimba", posisiXZ = new Vector2(272f, -218f), radius = 28f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Bangkai Kapal Pantai", posisiXZ = new Vector2(-115f, -865f), radius = 32f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Pabrik Gula Sinar Tebu", posisiXZ = new Vector2(665f, -500f), radius = 44f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Rumah Pohon Heli", posisiXZ = new Vector2(68f, -406f), radius = 18f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Desa Nelayan Timur", posisiXZ = new Vector2(720f, -140f), radius = 40f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Pos Gunung Kapur", posisiXZ = new Vector2(-580f, 620f), radius = 38f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Perkebunan Timur Laut", posisiXZ = new Vector2(590f, 560f), radius = 44f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Pulau Karang Selatan", posisiXZ = new Vector2(-320f, -885f), radius = 38f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Kuil Bambu Utara", posisiXZ = new Vector2(-120f, 610f), radius = 36f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Dermaga Tua Barat", posisiXZ = new Vector2(-520f, -340f), radius = 32f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Menara Pengawas Api", posisiXZ = new Vector2(-195f, -240f), radius = 28f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Pasar Hantu Desa", posisiXZ = new Vector2(468f, 178f), radius = 30f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Kamp Peneliti Terbengkalai", posisiXZ = new Vector2(-390f, 310f), radius = 34f },
                new JurnalEksplorasiPulau.ZonaTemu { nama = "Taman Batu Megalit", posisiXZ = new Vector2(210f, 420f), radius = 34f }
            });
            EditorUtility.SetDirty(jurnal);

            // Pasang BannerEksplorasiDanBahayaHUD pada Canvas HUD
            var hud = Object.FindFirstObjectByType<HUDInfo>(FindObjectsInactive.Include);
            if (hud == null) return;
            var canvas = hud.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.transform.Find("BannerEksplorasiDanBahaya") != null) return;

            var root = new GameObject("BannerEksplorasiDanBahaya", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            var rrt = (RectTransform)root.transform;
            rrt.anchorMin = Vector2.zero; rrt.anchorMax = Vector2.one;
            rrt.offsetMin = Vector2.zero; rrt.offsetMax = Vector2.zero;

            // 1. Banner Penemuan Sinematik (tengah-atas y = -152)
            var pTemu = new GameObject("PanelPenemuan", typeof(RectTransform), typeof(Image));
            pTemu.transform.SetParent(root.transform, false);
            var trt = (RectTransform)pTemu.transform;
            trt.anchorMin = new Vector2(0.5f, 1f); trt.anchorMax = new Vector2(0.5f, 1f);
            trt.pivot = new Vector2(0.5f, 1f); trt.anchoredPosition = new Vector2(0f, -152f);
            trt.sizeDelta = new Vector2(420f, 52f);
            var bgT = pTemu.GetComponent<Image>();
            bgT.color = new Color(0.05f, 0.08f, 0.06f, 0.78f); bgT.raycastTarget = false;

            var tJudul = BuatTeksTerpadu(pTemu.transform, "JudulPenemuan", 16, new Color(1f, 0.88f, 0.45f, 1f), TextAnchor.UpperCenter);
            tJudul.fontStyle = FontStyle.Bold;
            tJudul.text = "MENEMUKAN LOKASI BARU";
            var tSub = BuatTeksTerpadu(pTemu.transform, "SubPenemuan", 12, new Color(0.84f, 0.94f, 0.86f, 0.92f), TextAnchor.LowerCenter);
            tSub.text = "Jurnal Pulau 1/22";
            pTemu.SetActive(false);

            // 2. Panel Altimeter Glider Bambu (tengah-bawah y = 148)
            var pGlider = new GameObject("PanelGliderHUD", typeof(RectTransform), typeof(Image));
            pGlider.transform.SetParent(root.transform, false);
            var grt = (RectTransform)pGlider.transform;
            grt.anchorMin = new Vector2(0.5f, 0f); grt.anchorMax = new Vector2(0.5f, 0f);
            grt.pivot = new Vector2(0.5f, 0f); grt.anchoredPosition = new Vector2(0f, 148f);
            grt.sizeDelta = new Vector2(340f, 28f);
            var bgG = pGlider.GetComponent<Image>();
            bgG.color = new Color(0.04f, 0.12f, 0.16f, 0.72f); bgG.raycastTarget = false;
            var tGlider = BuatTeksTerpadu(pGlider.transform, "TeksGlider", 13, new Color(0.75f, 0.96f, 1f, 0.98f), TextAnchor.MiddleCenter);
            tGlider.fontStyle = FontStyle.Bold;
            pGlider.SetActive(false);

            // 3. Teks Peringatan Arah Bahaya (atas reticle y = 74)
            var bGo = new GameObject("TeksBahayaArah", typeof(RectTransform), typeof(Text));
            bGo.transform.SetParent(root.transform, false);
            var brt = (RectTransform)bGo.transform;
            brt.anchorMin = new Vector2(0.5f, 0.5f); brt.anchorMax = new Vector2(0.5f, 0.5f);
            brt.pivot = new Vector2(0.5f, 0.5f); brt.anchoredPosition = new Vector2(0f, 74f);
            brt.sizeDelta = new Vector2(260f, 28f);
            var tBahaya = bGo.GetComponent<Text>();
            tBahaya.font = ProceduralAssetFactory.GetSafeFont();
            tBahaya.fontSize = 15; tBahaya.fontStyle = FontStyle.Bold;
            tBahaya.alignment = TextAnchor.MiddleCenter;
            tBahaya.color = new Color(1f, 0.34f, 0.24f, 0.96f);
            tBahaya.raycastTarget = false; tBahaya.enabled = false;
            tBahaya.text = "! BAHAYA DI DEPAN !";

            var comp = root.AddComponent<BannerEksplorasiDanBahayaHUD>();
            comp.Pasang(pTemu, tJudul, tSub, pGlider, tGlider, tBahaya);
            EditorUtility.SetDirty(comp);
        }
    }
}
