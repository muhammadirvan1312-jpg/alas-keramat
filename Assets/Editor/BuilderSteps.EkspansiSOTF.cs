using System.Collections.Generic;
using AlasKeramat.AI;
using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Player;
using AlasKeramat.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace AlasKeramat.EditorTools
{
    // Langkah 7z9: Ekspansi Besar SOTF - 15 props CC0 Poly Haven 2K (bunker, pabrik, candi/altar, kemah sekte, pantai),
    // upgrade 8 permukaan 2K PBR (batu candi, kayu lapuk, karat pabrik, jalan batu), biota air (ikan & kepiting),
    // serangga siang (kupu-kupu & capung), 10 perangkap survival, jejak kaki basah/pasir, dan kompas suhu HUD.
    public static partial class NusantaraMasterBuilder
    {
        public static readonly string[] NamaPropEkspansi =
        {
            "PetiPeluru", "KotakMedis", "KotakPerkakas", "TungkuDrum", "PelitaKuningan",
            "BejanaKuningan", "GuciAntik", "MejaKayu", "KursiKayu", "RakKayu",
            "KardusLogistik", "KarungSemen", "KendiKuningan", "WajanKuningan", "PiringKayu"
        };
        static readonly string[] MeshPropEkspansi =
        {
            "PetiPeluruScan", "KotakMedisScan", "KotakPerkakasScan", "TungkuDrumScan", "PelitaKuninganScan",
            "BejanaKuninganScan", "GuciAntikScan", "MejaKayuScan", "KursiKayuScan", "RakKayuScan",
            "KardusLogistikScan", "KarungSemenScan", "KendiKuninganScan", "WajanKuninganScan", "PiringKayuScan"
        };
        static readonly string[] SlotPropEkspansi =
        {
            "PropPeluru", "PropMedis", "PropPerkakas", "PropTungkuDrum", "PropPelita",
            "PropBejana", "PropGuci", "PropMeja", "PropKursi", "PropRak",
            "PropKardus", "PropSemen", "PropKendi", "PropWajan", "PropPiringKayu"
        };
        static readonly float[] HalusPropEkspansi =
        {
            0.30f, 0.36f, 0.38f, 0.28f, 0.58f,
            0.52f, 0.34f, 0.20f, 0.22f, 0.20f,
            0.12f, 0.14f, 0.50f, 0.44f, 0.26f
        };
        static readonly int[] WadahPropEkspansi =
        {
            (int)PropGeledah.Wadah.Peti, (int)PropGeledah.Wadah.Peti, (int)PropGeledah.Wadah.Peti, -1, -1,
            (int)PropGeledah.Wadah.Dapur, (int)PropGeledah.Wadah.Dapur, -1, -1, -1,
            (int)PropGeledah.Wadah.Peti, -1, (int)PropGeledah.Wadah.Dapur, (int)PropGeledah.Wadah.Dapur, -1
        };

        public static int JumlahPropEkspansi, JumlahUpgradePBR, JumlahPerangkapAktif, JumlahKemahSekte;
        static Mesh[] _meshEkspansi;
        static Material[] _matEkspansi;

        static void LangkahEkspansiSOTF()
        {
            JumlahPropEkspansi = 0; JumlahUpgradePBR = 0; JumlahPerangkapAktif = 0; JumlahKemahSekte = 0;
            var lm = GameObject.Find(NamaRoot + "/World/Landmark") ?? GameObject.Find(NamaRoot + "/Landmark") ?? _root;
            var induk = Anak(lm, "EkspansiSOTF");

            UpgradePermukaanPBR2K(lm, induk);
            PasangPropsPolyHavenEkspansi(induk);
            BangunKemahSekteHidup(induk);
            PasangPerangkapSurvival(induk);
            PasangBiotaAirHidup(induk);
            PasangSeranggaSiang(induk);
            PasangJejakKakiDanKompas();

            Catat("Ekspansi SOTF: " + JumlahPropEkspansi + " props 2K CC0, " + JumlahUpgradePBR + " upgrade PBR 2K, "
                + JumlahKemahSekte + " kemah sekte, " + JumlahPerangkapAktif + " perangkap, biota air, serangga siang, jejak kaki & kompas suhu");
        }

        static Material MatPBRBaru(string nama, string tex, Color tint, float metalik, float halus, float ubin)
        {
            var a = Tex(tex);
            if (a == null) return null;
            var m = SafeMaterialFactory.CreatePBR(nama, tint, metalik, halus, a, Tex(tex + "_n"));
            m.SetTextureScale("_BaseMap", Vector2.one * ubin);
            MaskPBR(m, tex);
            return SimpanMat(m);
        }

        static void UpgradePermukaanPBR2K(GameObject lm, GameObject induk)
        {
            var matCandi = MatPBRBaru("M_BatuCandi2K", "batu_candi", new Color(0.84f, 0.85f, 0.80f), 0f, 0.24f, 1.4f);
            var matKayu = MatPBRBaru("M_KayuLapuk2K", "kayu_lapuk", new Color(0.86f, 0.82f, 0.76f), 0f, 0.20f, 1.6f);
            var matKarat = MatPBRBaru("M_KaratPabrik2K", "karat_pabrik", new Color(0.84f, 0.78f, 0.72f), 0.45f, 0.28f, 1.8f);
            var matJalan = MatPBRBaru("M_JalanBatu2K", "jalan_batu_desa", new Color(0.80f, 0.82f, 0.76f), 0f, 0.22f, 2.2f);
            var matBeton = MatPBRBaru("M_BetonBunker2K", "beton_bunker", new Color(0.82f, 0.84f, 0.82f), 0f, 0.18f, 1.5f);

            foreach (var r in lm.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (r == null) continue;
                string n = r.transform.root != null ? r.gameObject.name : "";
                string p = r.transform.parent != null ? r.transform.parent.name : "";
                if (matCandi != null && (p.StartsWith("GapuraKeramat") || p.StartsWith("MonolitWatuIreng") || p.StartsWith("MakamTua") || n.StartsWith("GapuraKeramat")))
                { r.sharedMaterial = matCandi; JumlahUpgradePBR++; }
                else if (matKayu != null && (p.StartsWith("JembatanSungai") || p.StartsWith("DermagaPantai") || p.StartsWith("MenaraPantau") || p.StartsWith("JembatanRawa")))
                { r.sharedMaterial = matKayu; JumlahUpgradePBR++; }
                else if (matKarat != null && (p.StartsWith("MesinPabrik") || p.StartsWith("LoriTebu")))
                { r.sharedMaterial = matKarat; JumlahUpgradePBR++; }
            }

            var ug2 = GameObject.Find("UG2_Bunker");
            if (ug2 != null && matBeton != null)
            {
                foreach (var r in ug2.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (r == null || r.sharedMaterial == null) continue;
                    if (r.gameObject.name.StartsWith("Puing_") || r.gameObject.name.StartsWith("Dinding"))
                    { r.sharedMaterial = matBeton; JumlahUpgradePBR++; }
                }
            }

            // Lempeng jalan batu berlumut ditanam rata tanah di jalur masuk selatan Desa Mati & Altar Beringin (bebas tabrakan meja/rumah).
            var matPelapis = matJalan ?? AmbilMat("M_Batu");
            var jalanInduk = Anak(induk, "JalanBatuDesa");
            for (int i = 0; i < 14; i++)
            {
                float x = 524f + Mathf.Sin(i * 0.35f) * 1.1f, z = 156f + i * 2.15f;
                float y = WorldTerrainGenerator.SampleHeight(x, z) - 0.015f;
                var q = Hias(jalanInduk, PrimitiveType.Cube, "LempengDesa_" + i, new Vector3(x, y, z), new Vector3(1.85f, 0.07f, 1.45f), matPelapis, false);
                q.transform.rotation = Quaternion.Euler(0f, (i % 3 - 1) * 7f, 0f);
            }
            for (int i = 0; i < 10; i++)
            {
                float x = 2f + Mathf.Sin(i * 0.4f) * 1.2f, z = -34f + i * 2.5f;
                float y = WorldTerrainGenerator.SampleHeight(x, z) - 0.015f;
                var q = Hias(jalanInduk, PrimitiveType.Cube, "LempengAltar_" + i, new Vector3(x, y, z), new Vector3(2.0f, 0.07f, 1.55f), matPelapis, false);
                q.transform.rotation = Quaternion.Euler(0f, (i % 2) * 8f, 0f);
            }
        }

        static void PasangPropsPolyHavenEkspansi(GameObject induk)
        {
            _meshEkspansi = new Mesh[NamaPropEkspansi.Length];
            _matEkspansi = new Material[NamaPropEkspansi.Length];
            int ada = 0;
            for (int i = 0; i < NamaPropEkspansi.Length; i++)
            {
                _meshEkspansi[i] = MuatPropScan(MeshPropEkspansi[i]);
                if (_meshEkspansi[i] == null) continue;
                _matEkspansi[i] = MaterialSolidScan(SlotPropEkspansi[i], "M_Eksp" + NamaPropEkspansi[i], new Color(0.88f, 0.85f, 0.80f, 1f), HalusPropEkspansi[i]) ?? AmbilMat("M_Kayu");
                ada++;
            }
            if (ada == 0) { Catat("Props ekspansi 2K: mesh panggang belum ada, dilewati (fail-soft)"); return; }

            var rnd = new System.Random(7919);
            var grup = Anak(induk, "PropsEkspansi2K");
            // 1. Altar Beringin & Gapura Keramat: pelita kuningan menyala malam, bejana, guci antik, kendi, piring ukir.
            SebarKlasterEkspansi(grup, new Vector2(0f, -14f), 3.5f, 12f, new[] { 4, 4, 4, 5, 5, 6, 6, 12, 14 }, rnd, true);
            // 2. Pabrik Gula Sinar Tebu: kotak perkakas, tungku drum, kardus logistik, karung semen, rak, meja.
            SebarKlasterEkspansi(grup, new Vector2(662f, -496f), 5f, 20f, new[] { 2, 2, 3, 3, 7, 9, 10, 10, 11, 11, 11 }, rnd, true);
            // 3. Lokasi jatuh heli: kotak medis, peti peluru, kardus logistik, kotak perkakas.
            SebarKlasterEkspansi(grup, new Vector2(8f, -448f), 5f, 14f, new[] { 0, 0, 1, 1, 1, 2, 10, 10 }, rnd, false);
            // 4. Desa Mati & Makam Tua: pelita, guci antik, meja, kursi, wajan, piring kayu.
            SebarKlasterEkspansi(grup, new Vector2(546f, 206f), 4f, 18f, new[] { 4, 4, 6, 6, 7, 8, 8, 12, 13, 14 }, rnd, true);
            // 5. Pantai & Dermaga: karung semen, kardus logistik, kotak medis, peti peluru, wajan.
            SebarKlasterEkspansi(grup, new Vector2(6f, -836f), 4f, 16f, new[] { 0, 1, 10, 10, 11, 11, 13 }, rnd, false);

            // 6. Bunker UG2 interior props (fail-soft bila UG2 ditemukan).
            var ug2 = GameObject.Find("UG2_Bunker");
            if (ug2 != null)
            {
                var pBunker = ug2.transform.position;
                int[] daftarBunker = { 0, 1, 2, 7, 8, 9, 10, 11 };
                for (int i = 0; i < daftarBunker.Length; i++)
                {
                    int idx = daftarBunker[i];
                    if (_meshEkspansi[idx] == null) continue;
                    Vector3 pos = pBunker + new Vector3(-3.5f + (i % 4) * 2.2f, 0.02f, -2.5f + (i / 4) * 3.8f);
                    TaruhPropEkspansi(grup, idx, pos, i * 47f, false);
                }
            }
        }

        static void SebarKlasterEkspansi(GameObject induk, Vector2 pusat, float rMin, float rMaks, int[] daftar, System.Random rnd, bool nyalakanApi)
        {
            for (int i = 0; i < daftar.Length; i++)
            {
                int idx = daftar[i];
                if (_meshEkspansi[idx] == null) continue;
                float a = (float)rnd.NextDouble() * Mathf.PI * 2f;
                float r = rMin + (float)rnd.NextDouble() * (rMaks - rMin);
                float x = pusat.x + Mathf.Cos(a) * r, z = pusat.y + Mathf.Sin(a) * r;
                float y = WorldTerrainGenerator.SampleHeight(x, z);
                if (y < 0.8f) continue;
                var go = TaruhPropEkspansi(induk, idx, new Vector3(x, y, z), (float)rnd.NextDouble() * 360f, nyalakanApi && (idx == 3 || idx == 4));
                if (go != null && idx == 4) go.transform.position += Vector3.up * 0.02f;
            }
        }

        static GameObject TaruhPropEkspansi(GameObject induk, int idx, Vector3 pos, float yaw, bool nyalaMalam)
        {
            var mesh = _meshEkspansi[idx];
            if (mesh == null) return null;
            var go = Anak(induk, "Eksp_" + NamaPropEkspansi[idx] + "_" + JumlahPropEkspansi);
            go.layer = GameLayers.Structure;
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = _matEkspansi[idx];
            mr.shadowCastingMode = ShadowCastingMode.On;
            mr.receiveShadows = true;
            var b = mesh.bounds;
            if (WadahPropEkspansi[idx] >= 0 || b.size.magnitude > 0.7f)
            {
                var bc = go.AddComponent<BoxCollider>();
                bc.center = b.center; bc.size = b.size;
            }
            if (WadahPropEkspansi[idx] >= 0)
            {
                go.layer = GameLayers.Interactable;
                go.tag = GameTags.Interactable;
                var pg = go.AddComponent<PropGeledah>();
                pg.Jenis = (PropGeledah.Wadah)WadahPropEkspansi[idx];
                EditorUtility.SetDirty(pg);
            }
            var lod = go.AddComponent<LODGroup>();
            lod.SetLODs(new[] { new LOD(b.size.magnitude > 0.8f ? 0.015f : 0.03f, new Renderer[] { mr }) });
            lod.RecalculateBounds();
            if (nyalaMalam) PasangApiLampuMinyak(go, b.max.y * 0.82f, idx == 3 ? 2.1f : 1.35f);
            JumlahPropEkspansi++;
            return go;
        }

        static void PasangApiLampuMinyak(GameObject induk, float tinggiApi, float intensitas)
        {
            var matApi = SimpanMat(SafeMaterialFactory.CreatePBR("M_ApiPelitaEkspansi", new Color(1f, 0.65f, 0.26f), 0f, 0.1f, null, null, new Color(4.5f, 2.3f, 0.7f)));
            var api = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            api.name = "ApiPelita";
            Object.DestroyImmediate(api.GetComponent<Collider>());
            api.transform.SetParent(induk.transform, false);
            api.transform.localPosition = new Vector3(0f, Mathf.Max(0.18f, tinggiApi), 0f);
            api.transform.localScale = new Vector3(0.05f, 0.09f, 0.05f);
            var mr = api.GetComponent<MeshRenderer>();
            mr.sharedMaterial = matApi; mr.shadowCastingMode = ShadowCastingMode.Off; mr.receiveShadows = false; mr.enabled = false;
            var gc = new GameObject("CahayaPelita");
            gc.transform.SetParent(induk.transform, false);
            gc.transform.localPosition = new Vector3(0f, Mathf.Max(0.26f, tinggiApi + 0.12f), 0f);
            var l = gc.AddComponent<Light>();
            l.type = LightType.Point; l.range = 6.0f; l.intensity = intensitas; l.color = new Color(1f, 0.60f, 0.25f);
            l.shadows = LightShadows.None; l.enabled = false;
            var lm = induk.AddComponent<LampuMinyak>();
            lm.Api = mr; lm.Cahaya = l; lm.Intensitas = intensitas;
            lm.Laron = BuatLaron(induk, Mathf.Max(0.18f, tinggiApi));
            EditorUtility.SetDirty(lm);
        }

        // 3 Kemah Sekte Hidup di rimba dalam: tungku api malam, peti logistik geledah, totem tengkorak.
        static void BangunKemahSekteHidup(GameObject induk)
        {
            Vector2[] lokasi = { new Vector2(-185f, -160f), new Vector2(240f, 140f), new Vector2(-110f, 320f) };
            var kayu = AmbilMat("M_Kayu"); var tulang = MatPolos("M_TulangKemah", "#D6CCB4");
            var kain = MatPolos("M_KainSekte", "#5A1F18");
            var rnd = new System.Random(3109);

            for (int i = 0; i < lokasi.Length; i++)
            {
                float x = lokasi[i].x, z = lokasi[i].y, y = WorldTerrainGenerator.SampleHeight(x, z);
                var kemah = Anak(induk, "KemahSekteHidup_" + i);
                kemah.transform.position = new Vector3(x, y, z);

                // Tenda sekte dari palang bambu + kain merah kusam
                var tenda = Hias(kemah, PrimitiveType.Cube, "AtapTenda", new Vector3(-2.4f, 1.15f, 1.4f), new Vector3(2.8f, 0.14f, 2.2f), kain, true);
                tenda.transform.localRotation = Quaternion.Euler(14f, 22f, 0f);
                Hias(kemah, PrimitiveType.Cylinder, "TiangA", new Vector3(-3.5f, 0.7f, 0.6f), new Vector3(0.08f, 0.7f, 0.08f), kayu, false);
                Hias(kemah, PrimitiveType.Cylinder, "TiangB", new Vector3(-1.3f, 0.7f, 2.2f), new Vector3(0.08f, 0.7f, 0.08f), kayu, false);

                // Totem tengkorak sekte
                Hias(kemah, PrimitiveType.Cylinder, "TiangTotem", new Vector3(2.6f, 1.1f, -1.8f), new Vector3(0.11f, 1.1f, 0.11f), kayu, true);
                Hias(kemah, PrimitiveType.Sphere, "TengkorakTotem", new Vector3(2.6f, 2.25f, -1.8f), new Vector3(0.28f, 0.32f, 0.30f), tulang, false);

                if (_meshEkspansi != null && _meshEkspansi[3] != null)
                    TaruhPropEkspansi(kemah, 3, new Vector3(x, y, z), i * 55f, true); // TungkuDrum menyala malam
                if (_meshEkspansi != null)
                {
                    int[] paket = { 0, 1, 5, 7, 8, 10 };
                    for (int k = 0; k < paket.Length; k++)
                    {
                        float a = k * 1.05f + i * 0.5f;
                        float px = x + Mathf.Cos(a) * 3.4f, pz = z + Mathf.Sin(a) * 3.4f;
                        TaruhPropEkspansi(kemah, paket[k], new Vector3(px, WorldTerrainGenerator.SampleHeight(px, pz), pz), (float)rnd.NextDouble() * 360f, false);
                    }
                }
                JumlahKemahSekte++;
            }
        }

        // 10 perangkap survival aktif: 5 jerat hewan di jalur hutan + 5 perangkap duri pertahanan desa/kemah.
        static void PasangPerangkapSurvival(GameObject induk)
        {
            var grup = Anak(induk, "PerangkapSurvival");
            var kayu = AmbilMat("M_Kayu"); var logam = AmbilMat("M_LogamGelap");
            var daging = MatPolos("M_BuruanPerangkap", "#6A4B34");
            Vector2[] titikJerat =
            {
                new Vector2(36f, -412f), new Vector2(-42f, -350f), new Vector2(128f, -260f),
                new Vector2(215f, -320f), new Vector2(-95f, -190f)
            };
            Vector2[] titikDuri =
            {
                new Vector2(522f, 184f), new Vector2(548f, 232f), new Vector2(-175f, -152f),
                new Vector2(230f, 134f), new Vector2(16f, -430f)
            };

            for (int i = 0; i < titikJerat.Length; i++)
                BuatPerangkap(grup, "JeratHewan_" + i, titikJerat[i], PerangkapSurvival.JenisPerangkap.JeratHewan, kayu, logam, daging);
            for (int i = 0; i < titikDuri.Length; i++)
                BuatPerangkap(grup, "PerangkapDuri_" + i, titikDuri[i], PerangkapSurvival.JenisPerangkap.DuriSekte, kayu, logam, daging);
        }

        static void BuatPerangkap(GameObject induk, string nama, Vector2 xz, PerangkapSurvival.JenisPerangkap jenis, Material kayu, Material logam, Material daging)
        {
            float y = WorldTerrainGenerator.SampleHeight(xz.x, xz.y);
            var go = Anak(induk, nama);
            go.transform.position = new Vector3(xz.x, y, xz.y);
            go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
            var bc = go.AddComponent<BoxCollider>(); bc.size = new Vector3(1.1f, 0.9f, 1.1f); bc.center = new Vector3(0f, 0.45f, 0f); bc.isTrigger = true;
            Hias(go, PrimitiveType.Cube, "Rangka", new Vector3(0f, 0.08f, 0f), new Vector3(0.95f, 0.14f, 0.95f), kayu, false);
            var engsel = Anak(go, "Engsel");
            engsel.transform.localPosition = new Vector3(0f, 0.16f, -0.35f);
            Hias(engsel, PrimitiveType.Cylinder, "Tuas", new Vector3(0f, 0.32f, 0.2f), new Vector3(0.06f, 0.45f, 0.06f), jenis == PerangkapSurvival.JenisPerangkap.DuriSekte ? logam : kayu, false);
            GameObject buruan = null;
            if (jenis == PerangkapSurvival.JenisPerangkap.JeratHewan)
            {
                buruan = Hias(go, PrimitiveType.Capsule, "Buruan", new Vector3(0f, 0.24f, 0f), new Vector3(0.24f, 0.18f, 0.36f), daging, false);
                buruan.SetActive(false);
            }
            var ps = go.AddComponent<PerangkapSurvival>();
            ps.Pasang(jenis, engsel.transform, buruan);
            EditorUtility.SetDirty(ps);
            JumlahPerangkapAktif++;
        }

        static void PasangBiotaAirHidup(GameObject induk)
        {
            var go = Anak(induk, "BiotaAirHidup");
            var matIkan = MatPolos("M_IkanSungai", "#4C6E74");
            var matSirip = MatPolos("M_SiripIkan", "#8AA4A8");
            var matKepiting = MatPolos("M_KepitingPantai", "#A84A2C");
            var daftar = new List<BiotaAirHidup.Entitas>();

            // 24 ikan di sungai & pantai dangkal (bisa ditangkap)
            for (int i = 0; i < 24; i++)
            {
                float x = i < 16 ? 240f + (i % 8) * 14f : -30f + (i - 16) * 9f;
                float z = i < 16 ? -260f + (i / 8) * 32f : -848f + (i % 3) * 4f;
                float? wl = WorldTerrainGenerator.GetWaterLevelAt(x, z);
                float y = wl.HasValue ? wl.Value - 0.18f : WorldTerrainGenerator.SampleHeight(x, z) + 0.1f;
                var ikan = Anak(go, "Ikan_" + i);
                ikan.transform.position = new Vector3(x, y, z);
                ikan.layer = GameLayers.Interactable; ikan.tag = GameTags.Interactable;
                var sc = ikan.AddComponent<SphereCollider>(); sc.radius = 0.45f; sc.isTrigger = true;
                Hias(ikan, PrimitiveType.Capsule, "Badan", Vector3.zero, new Vector3(0.08f, 0.20f, 0.12f), matIkan, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Hias(ikan, PrimitiveType.Cube, "Ekor", new Vector3(0f, 0f, -0.22f), new Vector3(0.02f, 0.12f, 0.10f), matSirip, false);
                var tangkap = ikan.AddComponent<IkanTangkap>();
                daftar.Add(new BiotaAirHidup.Entitas
                {
                    tr = ikan.transform, pusat = new Vector3(x, y, z),
                    radius = 1.6f + (i % 4) * 0.65f, laju = 0.55f + (i % 3) * 0.18f, fase = i * 1.1f,
                    kepiting = false, tangkap = tangkap
                });
            }

            // 14 kepiting pasir di sepanjang Pantai selatan
            for (int i = 0; i < 14; i++)
            {
                float x = -72f + i * 11f, z = -834f + (i % 3) * 3.5f;
                float y = WorldTerrainGenerator.SampleHeight(x, z) + 0.04f;
                var kep = Anak(go, "Kepiting_" + i);
                kep.transform.position = new Vector3(x, y, z);
                Hias(kep, PrimitiveType.Sphere, "Cangkang", Vector3.zero, new Vector3(0.18f, 0.07f, 0.14f), matKepiting, false);
                Hias(kep, PrimitiveType.Cube, "CapitL", new Vector3(-0.12f, 0.01f, 0.08f), new Vector3(0.06f, 0.04f, 0.08f), matKepiting, false);
                Hias(kep, PrimitiveType.Cube, "CapitR", new Vector3(0.12f, 0.01f, 0.08f), new Vector3(0.06f, 0.04f, 0.08f), matKepiting, false);
                daftar.Add(new BiotaAirHidup.Entitas
                {
                    tr = kep.transform, pusat = new Vector3(x, y, z),
                    radius = 1.4f + (i % 3) * 0.55f, laju = 0.42f + (i % 4) * 0.14f, fase = i * 1.7f,
                    kepiting = true, tangkap = null
                });
            }

            var pengelola = go.AddComponent<BiotaAirHidup>();
            pengelola.Pasang(daftar.ToArray());
            EditorUtility.SetDirty(pengelola);
        }

        static void PasangSeranggaSiang(GameObject induk)
        {
            var go = Anak(induk, "SeranggaSiang");
            var matKupuA = MatPolos("M_KupuEmas", "#E8B830");
            var matKupuB = MatPolos("M_KupuBiru", "#3A9BC4");
            var matCapung = MatPolos("M_CapungSungai", "#42B694");
            var daftar = new List<SeranggaSiang.Serangga>();

            // 18 kupu-kupu di Desa, Kampung, sekitar heli & Altar
            Vector2[] pusatKupu = { new Vector2(10f, -445f), new Vector2(545f, 205f), new Vector2(0f, -20f), new Vector2(56f, -376f) };
            for (int i = 0; i < 18; i++)
            {
                var c = pusatKupu[i % pusatKupu.Length];
                float x = c.x + Mathf.Cos(i * 1.4f) * (5f + (i % 4) * 3f);
                float z = c.y + Mathf.Sin(i * 1.4f) * (5f + (i % 4) * 3f);
                float y = WorldTerrainGenerator.SampleHeight(x, z) + 1.3f;
                var b = Anak(go, "Kupu_" + i);
                b.transform.position = new Vector3(x, y, z);
                var m = (i % 2 == 0) ? matKupuA : matKupuB;
                var sl = Hias(b, PrimitiveType.Quad, "SL", new Vector3(-0.055f, 0f, 0f), new Vector3(0.11f, 0.08f, 1f), m, false);
                var sr = Hias(b, PrimitiveType.Quad, "SR", new Vector3(0.055f, 0f, 0f), new Vector3(0.11f, 0.08f, 1f), m, false);
                sl.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                sr.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                daftar.Add(new SeranggaSiang.Serangga
                {
                    badan = b.transform, sayapL = sl.transform, sayapR = sr.transform,
                    pusat = new Vector3(x, y, z), radius = 2.6f + (i % 3) * 1.1f, laju = 0.65f + (i % 4) * 0.15f, fase = i * 0.9f, capung = false
                });
            }

            // 12 capung di tepi sungai & Rawa Bakau
            for (int i = 0; i < 12; i++)
            {
                float x = i < 8 ? 255f + i * 12f : -645f + (i - 8) * 14f;
                float z = i < 8 ? -245f + (i % 3) * 18f : -555f + (i % 2) * 12f;
                float y = WorldTerrainGenerator.SampleHeight(x, z) + 0.9f;
                var b = Anak(go, "Capung_" + i);
                b.transform.position = new Vector3(x, y, z);
                Hias(b, PrimitiveType.Cylinder, "Tubuh", Vector3.zero, new Vector3(0.015f, 0.09f, 0.015f), matCapung, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                var sl = Hias(b, PrimitiveType.Quad, "SL", new Vector3(-0.06f, 0f, 0.02f), new Vector3(0.12f, 0.03f, 1f), matCapung, false);
                var sr = Hias(b, PrimitiveType.Quad, "SR", new Vector3(0.06f, 0f, 0.02f), new Vector3(0.12f, 0.03f, 1f), matCapung, false);
                daftar.Add(new SeranggaSiang.Serangga
                {
                    badan = b.transform, sayapL = sl.transform, sayapR = sr.transform,
                    pusat = new Vector3(x, y, z), radius = 3.2f + (i % 3) * 1.2f, laju = 1.15f + (i % 3) * 0.25f, fase = i * 1.3f, capung = true
                });
            }

            var comp = go.AddComponent<SeranggaSiang>();
            comp.Pasang(daftar.ToArray());
            EditorUtility.SetDirty(comp);
        }

        static void PasangJejakKakiDanKompas()
        {
            var andi = Object.FindFirstObjectByType<AndiFPSController>(FindObjectsInactive.Include);
            if (andi != null && andi.GetComponent<JejakKakiAndi>() == null)
            {
                andi.gameObject.AddComponent<JejakKakiAndi>();
                EditorUtility.SetDirty(andi.gameObject);
            }

            var hud = Object.FindFirstObjectByType<HUDInfo>(FindObjectsInactive.Include);
            if (hud == null) return;
            var canvas = hud.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.transform.Find("KompasSuhuStrip") != null) return;

            var strip = new GameObject("KompasSuhuStrip", typeof(RectTransform), typeof(Image));
            strip.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)strip.transform;
            rt.anchorMin = new Vector2(1f, 1f); rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f); rt.anchoredPosition = new Vector2(-12f, -158f);
            rt.sizeDelta = new Vector2(198f, 24f);
            var bg = strip.GetComponent<Image>();
            bg.color = new Color(0.04f, 0.06f, 0.07f, 0.56f); bg.raycastTarget = false;

            var tGo = new GameObject("TeksKompasSuhu", typeof(RectTransform), typeof(Text));
            tGo.transform.SetParent(strip.transform, false);
            var trt = (RectTransform)tGo.transform;
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(6f, 1f); trt.offsetMax = new Vector2(-6f, -1f);
            var txt = tGo.GetComponent<Text>();
            txt.font = ProceduralAssetFactory.GetSafeFont();
            txt.fontSize = 12; txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(0.92f, 0.95f, 0.90f, 0.95f); txt.raycastTarget = false;
            txt.text = "U | Hutan Rimba | 28 C";

            var kompas = strip.AddComponent<KompasSuhuHUD>();
            kompas.Pasang(txt);
            EditorUtility.SetDirty(kompas);
        }
    }
}
