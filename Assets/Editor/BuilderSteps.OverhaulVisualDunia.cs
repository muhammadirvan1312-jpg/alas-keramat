using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.EditorTools
{
    // Langkah 7z12: Overhaul Visual & Dunia Mega-Landmark Nusantara - Air Terjun Rimba 14 m, Reruntuhan Candi Kuno,
    // Jembatan Gantung Rimba 28 m, Bangkai Kapal Pantai 16 m, Pendopo & Gerbang Lampion Desa, 48 formasi batu/akar raksasa,
    // serta ombak pantai & lentera gantung berayun.
    public static partial class NusantaraMasterBuilder
    {
        public static int JumlahFormasiTebing, JumlahMegaLandmark;

        static void LangkahOverhaulVisualDunia()
        {
            JumlahFormasiTebing = 0; JumlahMegaLandmark = 0;
            var lm = GameObject.Find(NamaRoot + "/World/Landmark") ?? GameObject.Find(NamaRoot + "/Landmark") ?? _root;
            var induk = Anak(lm, "OverhaulVisualDunia");
            var lentera = new List<Transform>();
            var ombak = new List<Transform>();

            var matBatuCandi = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_BatuCandi2K.mat")
                ?? MatPBRBaru("M_BatuCandiOverhaul", "bata_lumut", new Color(0.82f, 0.84f, 0.78f), 0f, 0.24f, 1.5f) ?? AmbilMat("M_Batu");
            var matKayuLapuk = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_KayuLapuk2K.mat")
                ?? MatPBRBaru("M_KayuOverhaul", "papan_rumah", new Color(0.84f, 0.80f, 0.74f), 0f, 0.20f, 1.6f) ?? AmbilMat("M_Kayu");
            var matKarat = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_KaratPabrik2K.mat")
                ?? MatPBRBaru("M_KaratOverhaul", "seng_karat", new Color(0.82f, 0.76f, 0.70f), 0.4f, 0.26f, 1.5f) ?? AmbilMat("M_LogamGelap");
            var matGenteng = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_GentengLumut.mat") ?? matBatuCandi;

            Vector3 posAirTerjun = BangunAirTerjunRimba(induk, matBatuCandi);
            BangunCandiRuntuhNusantara(induk, matBatuCandi, lentera);
            BangunJembatanGantungRimba(induk, matKayuLapuk, lentera);
            BangunBangkaiKapalPantai(induk, matKayuLapuk, matKarat, ombak);
            BangunPendopoDanGerbangDesa(induk, matKayuLapuk, matGenteng, lentera);
            BangunFormasiBatuDanAkarRaksasa(induk, matBatuCandi);

            var atmGo = Anak(induk, "AtmosferAirTerjunOmbak");
            var comp = atmGo.AddComponent<KabutAirTerjunDanOmbak>();
            comp.Pasang(posAirTerjun, lentera.ToArray(), ombak.ToArray());
            EditorUtility.SetDirty(comp);

            Catat("Overhaul Visual Dunia: " + JumlahMegaLandmark + " mega-landmark (Air Terjun, Candi Kuno, Jembatan Gantung, Bangkai Kapal, Pendopo Desa), "
                + JumlahFormasiTebing + " formasi batu/akar raksasa, " + lentera.Count + " lentera ayun & " + ombak.Count + " ombak pantai");
        }

        static Vector3 BangunAirTerjunRimba(GameObject induk, Material batu)
        {
            float x = 312f, z = -165f;
            float y = WorldTerrainGenerator.SampleHeight(x, z);
            var go = Anak(induk, "AirTerjunRimba");
            go.transform.position = new Vector3(x, y, z);

            // Tebing batu berundak di belakang air terjun
            Hias(go, PrimitiveType.Cube, "TebingBelakang", new Vector3(0f, 6.5f, 3.2f), new Vector3(13f, 14f, 5f), batu, true);
            Hias(go, PrimitiveType.Cube, "TebingKiri", new Vector3(-6.2f, 5.0f, 1.2f), new Vector3(4.5f, 11f, 6f), batu, true);
            Hias(go, PrimitiveType.Cube, "TebingKanan", new Vector3(6.2f, 5.2f, 1.2f), new Vector3(4.5f, 11.5f, 6f), batu, true);

            // Tirai air mengalir vertikal memakai shader AlasKeramat/Air
            var matAirTerjun = AirAK("M_AirTerjunRimba", new Color(0.45f, 0.78f, 0.82f, 0.72f), new Color(0.14f, 0.42f, 0.48f, 0.88f), 2.4f, new Vector2(0f, -1f))
                ?? SimpanMat(SafeMaterialFactory.CreatePBR("M_AirTerjunFallback", new Color(0.55f, 0.82f, 0.88f, 0.72f), 0f, 0.9f, null, null, null, SurfaceMode.Transparent));
            var tiraiA = Hias(go, PrimitiveType.Quad, "TiraiAirUtama", new Vector3(0f, 6.8f, 0.4f), new Vector3(6.8f, 13.8f, 1f), matAirTerjun, false);
            tiraiA.transform.localRotation = Quaternion.Euler(-6f, 0f, 0f);
            var tiraiB = Hias(go, PrimitiveType.Quad, "TiraiAirSamping", new Vector3(2.8f, 5.2f, 0.1f), new Vector3(2.8f, 10.5f, 1f), matAirTerjun, false);
            tiraiB.transform.localRotation = Quaternion.Euler(-8f, -14f, 0f);

            // Kolam zamrud di dasar air terjun
            var kolam = Hias(go, PrimitiveType.Cylinder, "KolamZamrud", new Vector3(0f, 0.18f, -3.5f), new Vector3(11.5f, 0.12f, 9.5f), matAirTerjun, false);
            kolam.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;

            // Batu lumut scan mengapit kolam
            var meshBatu = MuatPropScan("BatuBesarScan") ?? MuatPropScan("BatuLumutA");
            if (meshBatu != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    float a = (i / 7f) * Mathf.PI + Mathf.PI;
                    var b = Anak(go, "BatuKolam_" + i);
                    b.transform.localPosition = new Vector3(Mathf.Cos(a) * 6.4f, -0.2f, -3.5f + Mathf.Sin(a) * 4.8f);
                    b.transform.localScale = Vector3.one * (1.2f + (i % 3) * 0.45f);
                    b.transform.localRotation = Quaternion.Euler(0f, i * 47f, 0f);
                    b.AddComponent<MeshFilter>().sharedMesh = meshBatu;
                    b.AddComponent<MeshRenderer>().sharedMaterial = batu;
                }
            }
            JumlahMegaLandmark++;
            return new Vector3(x, y + 0.8f, z - 0.5f);
        }

        static void BangunCandiRuntuhNusantara(GameObject induk, Material batu, List<Transform> lentera)
        {
            float x = -265f, z = 115f;
            float y = WorldTerrainGenerator.SampleHeight(x, z);
            var go = Anak(induk, "CandiRuntuhNusantara");
            go.transform.position = new Vector3(x, y, z);

            // 3 undakan teras candi gaya Majapahit
            Hias(go, PrimitiveType.Cube, "Teras1", new Vector3(0f, 0.6f, 0f), new Vector3(16f, 1.2f, 16f), batu, true);
            Hias(go, PrimitiveType.Cube, "Teras2", new Vector3(0f, 1.65f, 0f), new Vector3(11.5f, 1.1f, 11.5f), batu, true);
            Hias(go, PrimitiveType.Cube, "Teras3", new Vector3(0f, 2.65f, 0f), new Vector3(7.2f, 1.0f, 7.2f), batu, true);
            // Tangga batu depan
            for (int t = 0; t < 6; t++)
                Hias(go, PrimitiveType.Cube, "TanggaCandi_" + t, new Vector3(0f, 0.25f + t * 0.45f, -8.5f + t * 0.85f), new Vector3(3.8f, 0.48f, 1.1f), batu, true);
            // 8 pilar candi patah berundak + stupa tengah
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * Mathf.Deg2Rad;
                float hPilar = 2.8f + (i % 3) * 1.4f;
                Hias(go, PrimitiveType.Cube, "PilarCandi_" + i, new Vector3(Mathf.Sin(a) * 5.2f, 1.8f + hPilar * 0.5f, Mathf.Cos(a) * 5.2f), new Vector3(0.95f, hPilar, 0.95f), batu, true);
            }
            Hias(go, PrimitiveType.Cylinder, "StupaDasar", new Vector3(0f, 4.1f, 0f), new Vector3(2.2f, 1.0f, 2.2f), batu, true);
            Hias(go, PrimitiveType.Sphere, "StupaKubah", new Vector3(0f, 5.2f, 0f), new Vector3(2.0f, 1.6f, 2.0f), batu, false);
            Hias(go, PrimitiveType.Cylinder, "StupaPuncak", new Vector3(0f, 6.4f, 0f), new Vector3(0.45f, 0.9f, 0.45f), batu, false);

            // 4 lentera pelita di sudut teras candi
            for (int i = 0; i < 4; i++)
            {
                float sx = (i % 2 == 0 ? -1f : 1f) * 6.8f, sz = (i < 2 ? -1f : 1f) * 6.8f;
                var lent = BuatLenteraGantung(go, "LenteraCandi_" + i, new Vector3(sx, 2.6f, sz), batu);
                lentera.Add(lent);
            }
            JumlahMegaLandmark++;
        }

        static void BangunJembatanGantungRimba(GameObject induk, Material kayu, List<Transform> lentera)
        {
            float x = 272f, z = -218f;
            float y = WorldTerrainGenerator.SampleHeight(x, z) + 2.6f;
            var go = Anak(induk, "JembatanGantungRimba");
            go.transform.position = new Vector3(x, y, z);
            var tali = AmbilMat("M_LogamGelap");

            // 4 tiang gerbang kayu di kedua ujung jembatan (panjang 28 m)
            float[] ujung = { -14f, 14f };
            for (int u = 0; u < ujung.Length; u++)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    Hias(go, PrimitiveType.Cylinder, "TiangJem_" + u + "_" + s, new Vector3(ujung[u], 0.8f, s * 1.35f), new Vector3(0.24f, 3.2f, 0.24f), kayu, true);
                }
                Hias(go, PrimitiveType.Cube, "PalangGerbang_" + u, new Vector3(ujung[u], 3.7f, 0f), new Vector3(0.24f, 0.26f, 3.1f), kayu, false);
                lentera.Add(BuatLenteraGantung(go, "LenteraJembatan_" + u, new Vector3(ujung[u], 3.35f, 0f), kayu));
            }
            // 26 papan kayu melengkung sedikit di tengah + kabel pegangan kiri/kanan
            for (int i = 0; i < 26; i++)
            {
                float t = i / 25f;
                float px = Mathf.Lerp(-13.5f, 13.5f, t);
                float sag = -Mathf.Sin(t * Mathf.PI) * 0.85f;
                Hias(go, PrimitiveType.Cube, "PapanJem_" + i, new Vector3(px, sag, 0f), new Vector3(0.88f, 0.12f, 2.4f), kayu, true);
            }
            for (int s = -1; s <= 1; s += 2)
                Hias(go, PrimitiveType.Cube, "KabelPegangan_" + s, new Vector3(0f, 0.95f, s * 1.25f), new Vector3(28f, 0.07f, 0.07f), tali, false);
            JumlahMegaLandmark++;
        }

        static void BangunBangkaiKapalPantai(GameObject induk, Material kayu, Material karat, List<Transform> ombak)
        {
            float x = -115f, z = -865f;
            float y = WorldTerrainGenerator.SampleHeight(x, z) + 0.3f;
            var go = Anak(induk, "BangkaiKapalPantai");
            go.transform.position = new Vector3(x, y, z);
            go.transform.rotation = Quaternion.Euler(7f, 32f, -14f);

            Hias(go, PrimitiveType.Cube, "LambungBawah", new Vector3(0f, 0.8f, 0f), new Vector3(4.6f, 1.8f, 15.5f), kayu, true);
            Hias(go, PrimitiveType.Cube, "DekKapal", new Vector3(0f, 1.85f, 0f), new Vector3(4.9f, 0.25f, 16.0f), kayu, true);
            Hias(go, PrimitiveType.Cube, "KabinKarat", new Vector3(0f, 3.1f, -3.5f), new Vector3(3.4f, 2.3f, 4.8f), karat, true);
            var tiang = Hias(go, PrimitiveType.Cylinder, "TiangLayarPatah", new Vector3(0f, 5.2f, 2.5f), new Vector3(0.24f, 4.8f, 0.24f), kayu, true);
            tiang.transform.localRotation = Quaternion.Euler(18f, 0f, -12f);

            // 8 garis ombak putih menyapu pesisir Pantai selatan
            var matOmbak = SimpanMat(SafeMaterialFactory.CreatePBR("M_OmbakPantai", new Color(0.90f, 0.96f, 0.98f, 0.56f), 0f, 0.35f, null, null, null, SurfaceMode.Transparent));
            var ombInduk = Anak(induk, "OmbakPantaiGaris");
            for (int i = 0; i < 8; i++)
            {
                float ox = -140f + i * 36f, oz = -846f;
                float? wl = WorldTerrainGenerator.GetWaterLevelAt(ox, oz);
                float oy = (wl.HasValue ? wl.Value : 0.35f) + 0.06f;
                var q = Hias(ombInduk, PrimitiveType.Quad, "OmbakPantai_" + i, new Vector3(ox, oy, oz), new Vector3(34f, 2.2f, 1f), matOmbak, false);
                q.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                q.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                ombak.Add(q.transform);
            }
            JumlahMegaLandmark++;
        }

        static void BangunPendopoDanGerbangDesa(GameObject induk, Material kayu, Material genteng, List<Transform> lentera)
        {
            // Pendopo alun-alun barat Desa Mati (bebas tabrakan dari RumahDesa_*)
            float x = 516f, z = 198f;
            float y = WorldTerrainGenerator.SampleHeight(x, z);
            var p = Anak(induk, "PendopoDesaMati");
            p.transform.position = new Vector3(x, y, z);
            Hias(p, PrimitiveType.Cube, "LantaiPendopo", new Vector3(0f, 0.25f, 0f), new Vector3(8.5f, 0.5f, 8.5f), AmbilMat("M_Batu"), true);
            for (int i = 0; i < 4; i++)
            {
                float sx = (i % 2 == 0 ? -1f : 1f) * 3.4f, sz = (i < 2 ? -1f : 1f) * 3.4f;
                Hias(p, PrimitiveType.Cylinder, "SokoGuru_" + i, new Vector3(sx, 2.1f, sz), new Vector3(0.22f, 1.9f, 0.22f), kayu, true);
            }
            Hias(p, PrimitiveType.Cube, "AtapPendopoBawah", new Vector3(0f, 4.15f, 0f), new Vector3(9.4f, 0.35f, 9.4f), genteng, false);
            Hias(p, PrimitiveType.Cube, "AtapPendopoAtas", new Vector3(0f, 5.15f, 0f), new Vector3(5.2f, 1.4f, 5.2f), genteng, false);
            lentera.Add(BuatLenteraGantung(p, "LenteraPendopo", new Vector3(0f, 3.6f, 0f), kayu));

            // Gerbang Bambu Desa Mati & Kampung Sukamaju
            Vector2[] gerbang = { new Vector2(526f, 188f), new Vector2(48f, -388f) };
            for (int i = 0; i < gerbang.Length; i++)
            {
                float gy = WorldTerrainGenerator.SampleHeight(gerbang[i].x, gerbang[i].y);
                var g = Anak(induk, "GerbangDesa_" + i);
                g.transform.position = new Vector3(gerbang[i].x, gy, gerbang[i].y);
                Hias(g, PrimitiveType.Cylinder, "TiangL", new Vector3(-2.4f, 2.1f, 0f), new Vector3(0.18f, 2.1f, 0.18f), kayu, true);
                Hias(g, PrimitiveType.Cylinder, "TiangR", new Vector3(2.4f, 2.1f, 0f), new Vector3(0.18f, 2.1f, 0.18f), kayu, true);
                Hias(g, PrimitiveType.Cube, "PalangAtas", new Vector3(0f, 4.0f, 0f), new Vector3(5.6f, 0.28f, 0.35f), kayu, false);
                lentera.Add(BuatLenteraGantung(g, "LenteraGerbang_" + i, new Vector3(0f, 3.55f, 0f), kayu));
            }
            JumlahMegaLandmark++;
        }

        static Transform BuatLenteraGantung(GameObject induk, string nama, Vector3 localPos, Material rangka)
        {
            var g = Anak(induk, nama);
            g.transform.localPosition = localPos;
            var matApi = SimpanMat(SafeMaterialFactory.CreatePBR("M_ApiLenteraGantung", new Color(1f, 0.64f, 0.25f), 0f, 0.1f, null, null, new Color(4.8f, 2.4f, 0.7f)));
            Hias(g, PrimitiveType.Cylinder, "Tali", new Vector3(0f, -0.15f, 0f), new Vector3(0.025f, 0.16f, 0.025f), rangka, false);
            var bola = Hias(g, PrimitiveType.Sphere, "Lampion", new Vector3(0f, -0.38f, 0f), new Vector3(0.24f, 0.30f, 0.24f), matApi, false);
            var mr = bola.GetComponent<MeshRenderer>(); mr.shadowCastingMode = ShadowCastingMode.Off;
            var lGo = Anak(g, "Cahaya"); lGo.transform.localPosition = new Vector3(0f, -0.38f, 0f);
            var l = lGo.AddComponent<Light>(); l.type = LightType.Point; l.range = 6.5f; l.intensity = 1.6f; l.color = new Color(1f, 0.62f, 0.26f); l.shadows = LightShadows.None; l.enabled = false;
            var lm = g.AddComponent<LampuMinyak>(); lm.Api = mr; lm.Cahaya = l; lm.Intensitas = 1.6f;
            EditorUtility.SetDirty(lm);
            return g.transform;
        }

        static void BangunFormasiBatuDanAkarRaksasa(GameObject induk, Material batu)
        {
            var grup = Anak(induk, "FormasiTebingDanAkar");
            var mBatu = MuatPropScan("BatuBesarScan") ?? MuatPropScan("BatuLumutA");
            var mKayu = MuatPropScan("KayuTumbangScan") ?? MuatPropScan("AkarScan");
            var matKayu = AmbilMat("M_KulitPohon") ?? AmbilMat("M_Kayu");
            var rnd = new System.Random(6203);

            // 48 formasi batu raksasa & kayu tumbang sepanjang tepi sungai, pantai, & bibir kawah Beringin
            for (int i = 0; i < 48; i++)
            {
                float x, z;
                if (i < 16) { x = 210f + i * 11f; z = -290f + (i % 4) * 28f; }
                else if (i < 32) { x = -190f + (i - 16) * 24f; z = -832f + (i % 3) * 8f; }
                else { float a = (i - 32) * 22.5f * Mathf.Deg2Rad; x = Mathf.Cos(a) * 44f; z = Mathf.Sin(a) * 44f; }
                float y = WorldTerrainGenerator.SampleHeight(x, z);
                if (y < 0.4f) continue;

                bool pakaiKayu = (i % 3 == 2) && mKayu != null;
                var mesh = pakaiKayu ? mKayu : mBatu;
                if (mesh != null)
                {
                    var go = Anak(grup, "Formasi_" + i);
                    go.transform.position = new Vector3(x, y - 0.12f, z);
                    go.transform.rotation = Quaternion.Euler((float)rnd.NextDouble() * 10f - 5f, (float)rnd.NextDouble() * 360f, 0f);
                    go.transform.localScale = Vector3.one * (1.35f + (float)rnd.NextDouble() * 1.25f);
                    go.AddComponent<MeshFilter>().sharedMesh = mesh;
                    var mr = go.AddComponent<MeshRenderer>();
                    mr.sharedMaterial = pakaiKayu ? matKayu : batu;
                    mr.shadowCastingMode = ShadowCastingMode.On;
                }
                else
                {
                    Hias(grup, PrimitiveType.Sphere, "FormasiBatu_" + i, new Vector3(x, y + 0.5f, z), new Vector3(2.8f, 1.8f, 2.4f), batu, true);
                }
                JumlahFormasiTebing++;
            }
        }
    }
}
