using System.Collections.Generic;
using AlasKeramat.AI;
using AlasKeramat.Building;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.EditorTools
{
    // Langkah 7z10: Ekspansi Dunia 2 (target 1 GB aset CC0): 24 model 3D scan 4K Poly Haven baru, 10 permukaan 4K PBR,
    // rak jemuran daging/ikan, penampung air hujan, bubu ikan aktif, 8 tiang suar jalan, musim + genangan hujan,
    // reputasi sekte dinamis, & aksi companion Deden/Ningsih.
    public static partial class NusantaraMasterBuilder
    {
        public static readonly string[] NamaPropDunia2 =
        {
            "TongBesi", "KotakKayuKecil", "MejaBesar", "MejaKecil", "KursiBunker", "KursiSekolah",
            "LemariTua", "LemariKayu", "MejaTeh", "KunciInggris", "Ragum", "BukuCatatan",
            "RadioTua", "PanciKuningan", "VasKuningan", "GuciKuningan", "GuciKeramikA", "GuciKeramikB",
            "GuciKeramikC", "GuciKeramikD", "TekoLogam", "PapanPeringatan", "PembakarLab", "KursiGoyang"
        };
        static readonly string[] MeshPropDunia2 =
        {
            "TongBesiScan", "KotakKayuKecilScan", "MejaBesarScan", "MejaKecilScan", "KursiBunkerScan", "KursiSekolahScan",
            "LemariTuaScan", "LemariKayuScan", "MejaTehScan", "KunciInggrisScan", "RagumScan", "BukuCatatanScan",
            "RadioTuaScan", "PanciKuninganScan", "VasKuninganScan", "GuciKuninganScan", "GuciKeramikAScan", "GuciKeramikBScan",
            "GuciKeramikCScan", "GuciKeramikDScan", "TekoLogamScan", "PapanPeringatanScan", "PembakarLabScan", "KursiGoyangScan"
        };
        static readonly string[] SlotPropDunia2 =
        {
            "PropTongBesi", "PropKotakKayuKecil", "PropMejaBesar", "PropMejaKecil", "PropKursiBunker", "PropKursiSekolah",
            "PropLemariTua", "PropLemariKayu", "PropMejaTeh", "PropKunciInggris", "PropRagum", "PropBukuCatatan",
            "PropRadioTua", "PropPanciKuningan", "PropVasKuningan", "PropGuciKuningan", "PropGuciKeramikA", "PropGuciKeramikB",
            "PropGuciKeramikC", "PropGuciKeramikD", "PropTekoLogam", "PropPapanPeringatan", "PropPembakarLab", "PropKursiGoyang"
        };
        static readonly int[] WadahPropDunia2 =
        {
            (int)PropGeledah.Wadah.BahanBakar, (int)PropGeledah.Wadah.Peti, -1, -1, -1, -1,
            (int)PropGeledah.Wadah.Peti, (int)PropGeledah.Wadah.Peti, -1, -1, -1, -1,
            -1, (int)PropGeledah.Wadah.Dapur, (int)PropGeledah.Wadah.Dapur, (int)PropGeledah.Wadah.Dapur,
            (int)PropGeledah.Wadah.Dapur, (int)PropGeledah.Wadah.Dapur, (int)PropGeledah.Wadah.Dapur, (int)PropGeledah.Wadah.Dapur,
            (int)PropGeledah.Wadah.Ember, -1, -1, -1
        };

        public static int JumlahPropDunia2, JumlahRakJemur, JumlahPenampungAir, JumlahBubuAktif, JumlahTiangSuar;
        static Mesh[] _meshDunia2;
        static Material[] _matDunia2;

        static void LangkahEkspansiDunia2()
        {
            JumlahPropDunia2 = 0; JumlahRakJemur = 0; JumlahPenampungAir = 0; JumlahBubuAktif = 0; JumlahTiangSuar = 0;
            var lm = GameObject.Find(NamaRoot + "/World/Landmark") ?? GameObject.Find(NamaRoot + "/Landmark") ?? _root;
            var induk = Anak(lm, "EkspansiDunia2");

            PasangPropsDunia2(induk);
            PasangFasilitasSurvival(induk);
            PasangTiangSuarJalan(induk);
            PasangSistemMusimDanGenangan(induk);
            PasangSistemReputasiDanTeman(induk);

            Catat("Ekspansi Dunia 2: " + JumlahPropDunia2 + " props 4K CC0, " + JumlahRakJemur + " rak jemuran, "
                + JumlahPenampungAir + " penampung air, " + JumlahBubuAktif + " bubu ikan, " + JumlahTiangSuar + " tiang suar, musim + reputasi sekte + aksi companion");
        }

        static void PasangPropsDunia2(GameObject induk)
        {
            _meshDunia2 = new Mesh[NamaPropDunia2.Length];
            _matDunia2 = new Material[NamaPropDunia2.Length];
            int ada = 0;
            for (int i = 0; i < NamaPropDunia2.Length; i++)
            {
                _meshDunia2[i] = MuatPropScan(MeshPropDunia2[i]);
                if (_meshDunia2[i] == null) continue;
                _matDunia2[i] = MaterialSolidScan(SlotPropDunia2[i], "M_D2_" + NamaPropDunia2[i], new Color(0.88f, 0.85f, 0.80f, 1f), 0.28f) ?? AmbilMat("M_Kayu");
                ada++;
            }
            if (ada == 0) { Catat("Props Dunia 2: mesh panggang belum ada, dilewati (fail-soft)"); return; }

            var grup = Anak(induk, "PropsDunia2_4K");
            var rnd = new System.Random(9013);
            // Desa Mati: lemari tua, meja besar/teh, kursi goyang, radio tua, panci/vas/guci keramik, teko.
            SebarKlasterDunia2(grup, new Vector2(544f, 204f), 4f, 24f, new[] { 2, 3, 6, 7, 8, 12, 13, 16, 17, 18, 20, 23 }, rnd);
            // Kampung Sukamaju & Watu Ireng: kursi sekolah, lemari kayu, guci kuningan, keramik, meja kecil, kotak kayu.
            SebarKlasterDunia2(grup, new Vector2(56f, -376f), 5f, 20f, new[] { 1, 3, 5, 7, 13, 14, 15, 19, 20, 23 }, rnd);
            // Pabrik Gula Sinar Tebu: tong besi, kunci inggris, ragum, papan peringatan, buku catatan, kursi bunker.
            SebarKlasterDunia2(grup, new Vector2(668f, -502f), 5f, 22f, new[] { 0, 0, 1, 4, 9, 9, 10, 11, 21, 21 }, rnd);
            // Altar Beringin & Makam Tua: vas kuningan, guci kuningan, 4 guci keramik, panci kuningan.
            SebarKlasterDunia2(grup, new Vector2(0f, -12f), 4f, 16f, new[] { 13, 14, 15, 16, 17, 18, 19 }, rnd);
            // Pantai & Mercusuar: tong besi, radio tua, kotak kayu kecil, kursi, teko logam.
            SebarKlasterDunia2(grup, new Vector2(-178f, -860f), 4f, 18f, new[] { 0, 1, 5, 11, 12, 20 }, rnd);

            // Bunker UG2 Laboratorium: kursi bunker, pembakar lab, buku catatan, papan peringatan, ragum, meja, radio tua.
            var ug2 = GameObject.Find("UG2_Bunker");
            if (ug2 != null)
            {
                var pB = ug2.transform.position;
                int[] lab = { 0, 2, 4, 4, 9, 10, 11, 12, 21, 22, 22 };
                for (int i = 0; i < lab.Length; i++)
                {
                    int idx = lab[i];
                    if (_meshDunia2[idx] == null) continue;
                    Vector3 pos = pB + new Vector3(-4.2f + (i % 4) * 2.4f, 0.02f, 2.2f + (i / 4) * 2.6f);
                    TaruhPropDunia2(grup, idx, pos, i * 41f);
                }
            }
        }

        static void SebarKlasterDunia2(GameObject induk, Vector2 pusat, float rMin, float rMaks, int[] daftar, System.Random rnd)
        {
            for (int i = 0; i < daftar.Length; i++)
            {
                int idx = daftar[i];
                if (_meshDunia2[idx] == null) continue;
                float a = (float)rnd.NextDouble() * Mathf.PI * 2f;
                float r = rMin + (float)rnd.NextDouble() * (rMaks - rMin);
                float x = pusat.x + Mathf.Cos(a) * r, z = pusat.y + Mathf.Sin(a) * r;
                float y = WorldTerrainGenerator.SampleHeight(x, z);
                if (y < 0.8f) continue;
                TaruhPropDunia2(induk, idx, new Vector3(x, y, z), (float)rnd.NextDouble() * 360f);
            }
        }

        static GameObject TaruhPropDunia2(GameObject induk, int idx, Vector3 pos, float yaw)
        {
            var mesh = _meshDunia2[idx];
            if (mesh == null) return null;
            var go = Anak(induk, "D2_" + NamaPropDunia2[idx] + "_" + JumlahPropDunia2);
            go.layer = GameLayers.Structure;
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = _matDunia2[idx];
            mr.shadowCastingMode = ShadowCastingMode.On;
            mr.receiveShadows = true;
            var b = mesh.bounds;
            if (WadahPropDunia2[idx] >= 0 || b.size.magnitude > 0.7f)
            {
                var bc = go.AddComponent<BoxCollider>();
                bc.center = b.center; bc.size = b.size;
            }
            if (WadahPropDunia2[idx] >= 0)
            {
                go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
                var pg = go.AddComponent<PropGeledah>();
                pg.Jenis = (PropGeledah.Wadah)WadahPropDunia2[idx];
                EditorUtility.SetDirty(pg);
            }
            var lod = go.AddComponent<LODGroup>();
            lod.SetLODs(new[] { new LOD(b.size.magnitude > 0.8f ? 0.015f : 0.03f, new Renderer[] { mr }) });
            lod.RecalculateBounds();
            JumlahPropDunia2++;
            return go;
        }

        static void PasangFasilitasSurvival(GameObject induk)
        {
            var kayu = AmbilMat("M_Kayu"); var daging = MatPolos("M_DagingJemur", "#7A2E22");
            var airMat = SimpanMat(SafeMaterialFactory.CreatePBR("M_AirPenampung", new Color(0.24f, 0.54f, 0.64f, 0.78f), 0f, 0.9f, null, null, null, SurfaceMode.Transparent));
            var ikanMat = MatPolos("M_IkanBubu", "#527880");

            // 6 Rak Jemuran Daging/Ikan
            Vector2[] posRak = { new Vector2(18f, -442f), new Vector2(538f, 200f), new Vector2(58f, -368f), new Vector2(-180f, -154f), new Vector2(14f, -834f), new Vector2(-640f, -552f) };
            for (int i = 0; i < posRak.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(posRak[i].x, posRak[i].y);
                var go = Anak(induk, "RakJemuran_" + i);
                go.transform.position = new Vector3(posRak[i].x, y, posRak[i].y);
                go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
                var bc = go.AddComponent<BoxCollider>(); bc.size = new Vector3(1.6f, 1.5f, 0.8f); bc.center = new Vector3(0f, 0.75f, 0f);
                Hias(go, PrimitiveType.Cylinder, "TiangL", new Vector3(-0.65f, 0.7f, 0f), new Vector3(0.08f, 0.7f, 0.08f), kayu, false);
                Hias(go, PrimitiveType.Cylinder, "TiangR", new Vector3(0.65f, 0.7f, 0f), new Vector3(0.08f, 0.7f, 0.08f), kayu, false);
                Hias(go, PrimitiveType.Cube, "Palang", new Vector3(0f, 1.32f, 0f), new Vector3(1.5f, 0.08f, 0.08f), kayu, false);
                var strips = new GameObject[3];
                for (int s = 0; s < 3; s++)
                    strips[s] = Hias(go, PrimitiveType.Cube, "Strip_" + s, new Vector3(-0.35f + s * 0.35f, 1.05f, 0f), new Vector3(0.14f, 0.36f, 0.06f), daging, false);
                var rak = go.AddComponent<RakPengeringDanMasak>();
                rak.Pasang(strips);
                EditorUtility.SetDirty(rak);
                JumlahRakJemur++;
            }

            // 6 Penampung Air Hujan Bambu
            Vector2[] posAir = { new Vector2(14f, -445f), new Vector2(542f, 208f), new Vector2(52f, -372f), new Vector2(656f, -496f), new Vector2(-644f, -558f), new Vector2(8f, -836f) };
            for (int i = 0; i < posAir.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(posAir[i].x, posAir[i].y);
                var go = Anak(induk, "PenampungAir_" + i);
                go.transform.position = new Vector3(posAir[i].x, y, posAir[i].y);
                go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
                var bc = go.AddComponent<BoxCollider>(); bc.size = new Vector3(0.9f, 0.9f, 0.9f); bc.center = new Vector3(0f, 0.45f, 0f);
                Hias(go, PrimitiveType.Cylinder, "Wadah", new Vector3(0f, 0.35f, 0f), new Vector3(0.75f, 0.35f, 0.75f), kayu, false);
                var air = Hias(go, PrimitiveType.Cylinder, "Air", new Vector3(0f, 0.45f, 0f), new Vector3(0.68f, 0.04f, 0.68f), airMat, false);
                var pen = go.AddComponent<PenampungAirHujan>();
                pen.Pasang(air.transform);
                EditorUtility.SetDirty(pen);
                JumlahPenampungAir++;
            }

            // 6 Bubu Ikan Aktif di tepi sungai & pantai
            Vector2[] posBubu = { new Vector2(252f, -252f), new Vector2(280f, -220f), new Vector2(310f, -195f), new Vector2(-18f, -844f), new Vector2(22f, -846f), new Vector2(-635f, -562f) };
            for (int i = 0; i < posBubu.Length; i++)
            {
                float? wl = WorldTerrainGenerator.GetWaterLevelAt(posBubu[i].x, posBubu[i].y);
                float y = wl.HasValue ? wl.Value - 0.12f : WorldTerrainGenerator.SampleHeight(posBubu[i].x, posBubu[i].y);
                var go = Anak(induk, "BubuIkan_" + i);
                go.transform.position = new Vector3(posBubu[i].x, y, posBubu[i].y);
                go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
                var bc = go.AddComponent<BoxCollider>(); bc.size = new Vector3(1.1f, 0.7f, 1.1f); bc.center = new Vector3(0f, 0.3f, 0f);
                Hias(go, PrimitiveType.Cylinder, "Keranjang", new Vector3(0f, 0.25f, 0f), new Vector3(0.55f, 0.42f, 0.55f), kayu, false).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                var ikans = new GameObject[3];
                for (int k = 0; k < 3; k++)
                    ikans[k] = Hias(go, PrimitiveType.Capsule, "Ikan_" + k, new Vector3(-0.18f + k * 0.18f, 0.28f, 0f), new Vector3(0.08f, 0.16f, 0.08f), ikanMat, false);
                var bubu = go.AddComponent<BubuIkanAktif>();
                bubu.Pasang(ikans);
                EditorUtility.SetDirty(bubu);
                JumlahBubuAktif++;
            }
        }

        static void PasangTiangSuarJalan(GameObject induk)
        {
            var kayu = AmbilMat("M_Kayu");
            var apiMat = SimpanMat(SafeMaterialFactory.CreatePBR("M_ApiSuarJalan", new Color(1f, 0.56f, 0.18f), 0f, 0.1f, null, null, new Color(5.2f, 2.4f, 0.5f)));
            string[] nama = { "Jalur Heli - Desa", "Simpang Kampung", "Gerbang Altar", "Jalur Pabrik Gula", "Dermaga Pantai", "Jalur Kemah Rawa", "Jalur Mercusuar", "Bukit Utara" };
            Vector2[] pos =
            {
                new Vector2(30f, -415f), new Vector2(180f, -260f), new Vector2(12f, -55f), new Vector2(560f, -420f),
                new Vector2(12f, -790f), new Vector2(-520f, -490f), new Vector2(-145f, -820f), new Vector2(-60f, 280f)
            };
            for (int i = 0; i < pos.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(pos[i].x, pos[i].y);
                var go = Anak(induk, "TiangSuar_" + i);
                go.transform.position = new Vector3(pos[i].x, y, pos[i].y);
                go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
                var bc = go.AddComponent<BoxCollider>(); bc.size = new Vector3(0.9f, 2.4f, 0.9f); bc.center = new Vector3(0f, 1.2f, 0f);
                Hias(go, PrimitiveType.Cylinder, "Tiang", new Vector3(0f, 1.15f, 0f), new Vector3(0.11f, 1.15f, 0.11f), kayu, true);
                Hias(go, PrimitiveType.Cube, "PapanArah", new Vector3(0.28f, 1.85f, 0f), new Vector3(0.68f, 0.18f, 0.06f), kayu, false);
                var api = Hias(go, PrimitiveType.Sphere, "ApiSuar", new Vector3(0f, 2.38f, 0f), new Vector3(0.18f, 0.26f, 0.18f), apiMat, false);
                var mr = api.GetComponent<MeshRenderer>(); mr.shadowCastingMode = ShadowCastingMode.Off; mr.receiveShadows = false;
                var tp = go.AddComponent<TiangPenandaJalan>();
                tp.Pasang(nama[i], mr);
                EditorUtility.SetDirty(tp);
                JumlahTiangSuar++;
            }
        }

        static void PasangSistemMusimDanGenangan(GameObject induk)
        {
            var go = Anak(induk, "SistemMusimDinamis");
            var matGen = SimpanMat(SafeMaterialFactory.CreatePBR("M_GenanganHujan", new Color(0.08f, 0.14f, 0.16f, 0.65f), 0.05f, 0.94f, null, null, null, SurfaceMode.Transparent));
            Vector2[] posG = { new Vector2(12f, -440f), new Vector2(544f, 206f), new Vector2(56f, -372f), new Vector2(662f, -498f), new Vector2(4f, -24f), new Vector2(-182f, -158f) };
            var rends = new MeshRenderer[posG.Length];
            for (int i = 0; i < posG.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(posG[i].x, posG[i].y) + 0.04f;
                var q = Hias(go, PrimitiveType.Quad, "GenanganHujan_" + i, new Vector3(posG[i].x, y, posG[i].y), new Vector3(4.5f + (i % 2) * 1.5f, 3.8f, 1f), matGen, false);
                q.transform.rotation = Quaternion.Euler(90f, i * 35f, 0f);
                var mr = q.GetComponent<MeshRenderer>();
                mr.shadowCastingMode = ShadowCastingMode.Off; mr.receiveShadows = true; mr.enabled = false;
                rends[i] = mr;
            }
            var musim = go.AddComponent<SistemMusimDinamis>();
            musim.PasangGenangan(rends);
            EditorUtility.SetDirty(musim);
        }

        static void PasangSistemReputasiDanTeman(GameObject induk)
        {
            var repGo = Anak(induk, "ReputasiSekte");
            repGo.AddComponent<ReputasiSekte>();
            var temanGo = Anak(induk, "AksiTemanHidup");
            temanGo.AddComponent<AksiTemanHidup>();
        }
    }
}
