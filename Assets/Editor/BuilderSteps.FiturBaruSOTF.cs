using AlasKeramat.AI;
using AlasKeramat.Building;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.EditorTools
{
    // Langkah 7z11: Fitur Gameplay Besar Baru SOTF - 5 jalur Zipline, 3 Perahu Dayung, 3 Rumah Pohon Aman (panjat + tidur/save),
    // 4 Sarang Pemangsa + Jejak Lacak, 2 Boss Mini Buto Ijo (Ground Slam), dan 3 Suar Sinyal Darurat.
    public static partial class NusantaraMasterBuilder
    {
        public static int JumlahZipline, JumlahPerahuDayung, JumlahRumahPohon, JumlahSarangPemangsa, JumlahButoIjo, JumlahSuarDarurat;

        static void LangkahFiturBaruSOTF()
        {
            JumlahZipline = 0; JumlahPerahuDayung = 0; JumlahRumahPohon = 0;
            JumlahSarangPemangsa = 0; JumlahButoIjo = 0; JumlahSuarDarurat = 0;
            var lm = GameObject.Find(NamaRoot + "/World/Landmark") ?? GameObject.Find(NamaRoot + "/Landmark") ?? _root;
            var induk = Anak(lm, "FiturBaruSOTF");

            BangunZiplineRimba(induk);
            BangunPerahuDayung(induk);
            BangunRumahPohonAman(induk);
            BangunSarangDanJejakPemangsa(induk);
            BangunBossMiniButoIjo(induk);
            BangunSuarSinyalDarurat(induk);

            Catat("Fitur Baru SOTF: " + JumlahZipline + " zipline, " + JumlahPerahuDayung + " perahu dayung, "
                + JumlahRumahPohon + " rumah pohon aman, " + JumlahSarangPemangsa + " sarang pemangsa, "
                + JumlahButoIjo + " Boss Mini Buto Ijo & " + JumlahSuarDarurat + " suar sinyal darurat");
        }

        static void BangunZiplineRimba(GameObject induk)
        {
            var grup = Anak(induk, "JalurZipline");
            var kayu = AmbilMat("M_Kayu"); var logam = AmbilMat("M_LogamGelap");
            string[] nama = { "Bukit Heli - Desa", "Tebing Utara - Kawah", "Pabrik Gula - Sungai", "Mercusuar - Dermaga", "Rawa - Bukit Selatan" };
            Vector2[] awal = { new Vector2(22f, -430f), new Vector2(12f, 240f), new Vector2(640f, -480f), new Vector2(-165f, -840f), new Vector2(-580f, -520f) };
            Vector2[] akhir = { new Vector2(495f, 175f), new Vector2(18f, -38f), new Vector2(315f, -210f), new Vector2(-12f, -830f), new Vector2(-80f, -410f) };

            for (int i = 0; i < nama.Length; i++)
            {
                Vector3 p0 = new Vector3(awal[i].x, WorldTerrainGenerator.SampleHeight(awal[i].x, awal[i].y) + 3.2f, awal[i].y);
                Vector3 p1 = new Vector3(akhir[i].x, WorldTerrainGenerator.SampleHeight(akhir[i].x, akhir[i].y) + 2.8f, akhir[i].y);
                var zGo = Anak(grup, "Zipline_" + i);
                zGo.transform.position = new Vector3(p0.x, p0.y - 3.2f, p0.z);
                zGo.layer = GameLayers.Interactable; zGo.tag = GameTags.Interactable;
                var bc = zGo.AddComponent<BoxCollider>(); bc.size = new Vector3(1.6f, 3.4f, 1.6f); bc.center = new Vector3(0f, 1.7f, 0f);
                Hias(zGo, PrimitiveType.Cylinder, "TiangAwal", new Vector3(0f, 1.7f, 0f), new Vector3(0.18f, 1.75f, 0.18f), kayu, true);
                Hias(zGo, PrimitiveType.Cube, "Katrol", new Vector3(0f, 3.2f, 0f), new Vector3(0.35f, 0.35f, 0.35f), logam, false);

                var uGo = Anak(grup, "TiangAkhirZipline_" + i);
                uGo.transform.position = new Vector3(p1.x, p1.y - 2.8f, p1.z);
                Hias(uGo, PrimitiveType.Cylinder, "Tiang", new Vector3(0f, 1.5f, 0f), new Vector3(0.18f, 1.5f, 0.18f), kayu, true);

                // Kabel baja visual melintang di udara
                var kabel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                kabel.name = "KabelZipline_" + i;
                Object.DestroyImmediate(kabel.GetComponent<Collider>());
                kabel.transform.SetParent(grup.transform, false);
                Vector3 d = p1 - p0;
                kabel.transform.position = (p0 + p1) * 0.5f;
                kabel.transform.up = d.normalized;
                kabel.transform.localScale = new Vector3(0.035f, d.magnitude * 0.5f, 0.035f);
                var kmr = kabel.GetComponent<MeshRenderer>(); kmr.sharedMaterial = logam; kmr.shadowCastingMode = ShadowCastingMode.Off;

                var zip = zGo.AddComponent<ZiplineRimba>();
                zip.Pasang(p0, p1, nama[i]);
                EditorUtility.SetDirty(zip);
                JumlahZipline++;
            }
        }

        static void BangunPerahuDayung(GameObject induk)
        {
            var grup = Anak(induk, "PerahuDayungAktif");
            var kayu = AmbilMat("M_Kayu");
            Vector2[] pos = { new Vector2(-4f, -846f), new Vector2(268f, -238f), new Vector2(-630f, -558f) };
            for (int i = 0; i < pos.Length; i++)
            {
                float? wl = WorldTerrainGenerator.GetWaterLevelAt(pos[i].x, pos[i].y);
                float y = wl.HasValue ? wl.Value + 0.08f : WorldTerrainGenerator.SampleHeight(pos[i].x, pos[i].y) + 0.12f;
                var go = Anak(grup, "PerahuDayung_" + i);
                go.transform.position = new Vector3(pos[i].x, y, pos[i].y);
                go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
                var bc = go.AddComponent<BoxCollider>(); bc.size = new Vector3(1.5f, 0.8f, 3.4f); bc.center = new Vector3(0f, 0.35f, 0f);
                Hias(go, PrimitiveType.Cube, "Lambung", new Vector3(0f, 0.14f, 0f), new Vector3(1.35f, 0.24f, 3.2f), kayu, false);
                Hias(go, PrimitiveType.Cube, "SisiL", new Vector3(-0.62f, 0.38f, 0f), new Vector3(0.12f, 0.34f, 3.1f), kayu, false);
                Hias(go, PrimitiveType.Cube, "SisiR", new Vector3(0.62f, 0.38f, 0f), new Vector3(0.12f, 0.34f, 3.1f), kayu, false);
                Hias(go, PrimitiveType.Cube, "Bangku", new Vector3(0f, 0.32f, -0.2f), new Vector3(1.15f, 0.08f, 0.42f), kayu, false);
                var pd = go.AddComponent<PerahuDayung>();
                EditorUtility.SetDirty(pd);
                JumlahPerahuDayung++;
            }
        }

        static void BangunRumahPohonAman(GameObject induk)
        {
            var grup = Anak(induk, "RumahPohonAman");
            var kayu = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_KayuLapuk2K.mat") ?? AmbilMat("M_Kayu");
            var kulit = AmbilMat("M_KulitPohon") ?? kayu;
            var daun = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_DaunAsli.mat") ?? AmbilMat("M_DaunPohon");
            var genteng = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_GentengLumut.mat") ?? kayu;
            var mTajuk = MuatPropScan("PohonScan_LOD0") ?? MuatPropScan("PohonScanC_LOD0");
            var kain = MatPolos("M_TikarKasur", "#7A6A4A");
            // RumahPohon_0 dipindah ke (68, -406) agar tidak menghalangi kamera v1_hewan_barisan (45.2, -423.5)
            Vector2[] pos = { new Vector2(68f, -406f), new Vector2(-120f, -80f), new Vector2(-595f, -535f) };

            for (int i = 0; i < pos.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(pos[i].x, pos[i].y);
                var rp = Anak(grup, "RumahPohon_" + i);
                rp.transform.position = new Vector3(pos[i].x, y, pos[i].y);
                // Batang pohon raksasa + 4 penyangga diagonal + dek 6 m + atap pelana miring + tajuk pohon scan asli
                Hias(rp, PrimitiveType.Cylinder, "BatangInduk", new Vector3(0f, 5.2f, 0f), new Vector3(1.15f, 5.2f, 1.15f), kulit, true);
                for (int s = 0; s < 4; s++)
                {
                    float sx = (s % 2 == 0 ? -1f : 1f) * 1.15f, sz = (s < 2 ? -1f : 1f) * 1.15f;
                    var br = Hias(rp, PrimitiveType.Cylinder, "Penyangga_" + s, new Vector3(sx, 4.9f, sz), new Vector3(0.14f, 1.55f, 0.14f), kayu, false);
                    br.transform.localRotation = Quaternion.Euler(sz > 0 ? 34f : -34f, 0f, sx > 0 ? -34f : 34f);
                }
                Hias(rp, PrimitiveType.Cube, "DekAtas", new Vector3(0f, 6.0f, 0f), new Vector3(4.8f, 0.25f, 4.8f), kayu, true);
                Hias(rp, PrimitiveType.Cube, "AtapMiringL", new Vector3(-1.35f, 8.85f, 0f), new Vector3(3.1f, 0.16f, 5.2f), genteng, false).transform.localRotation = Quaternion.Euler(0f, 0f, 22f);
                Hias(rp, PrimitiveType.Cube, "AtapMiringR", new Vector3(1.35f, 8.85f, 0f), new Vector3(3.1f, 0.16f, 5.2f), genteng, false).transform.localRotation = Quaternion.Euler(0f, 0f, -22f);
                Hias(rp, PrimitiveType.Cube, "PagarA", new Vector3(0f, 6.6f, 2.25f), new Vector3(4.7f, 0.9f, 0.12f), kayu, true);
                Hias(rp, PrimitiveType.Cube, "PagarB", new Vector3(2.25f, 6.6f, 0f), new Vector3(0.12f, 0.9f, 4.7f), kayu, true);
                Hias(rp, PrimitiveType.Cube, "PagarC", new Vector3(-2.25f, 6.6f, 0f), new Vector3(0.12f, 0.9f, 4.7f), kayu, true);
                if (mTajuk != null)
                {
                    var tj = Anak(rp, "TajukRumahPohon");
                    tj.transform.localPosition = new Vector3(0f, 3.8f, 0f);
                    tj.transform.localScale = Vector3.one * 0.95f;
                    tj.AddComponent<MeshFilter>().sharedMesh = mTajuk;
                    var mr = tj.AddComponent<MeshRenderer>();
                    mr.sharedMaterials = mTajuk.subMeshCount >= 2 ? new[] { kulit, daun } : new[] { daun };
                }

                // Tangga naik (bawah) & tangga turun (atas)
                var naik = Hias(rp, PrimitiveType.Cube, "TanggaNaik", new Vector3(0f, 3.0f, -2.35f), new Vector3(0.7f, 6.0f, 0.22f), kayu, true);
                naik.layer = GameLayers.Interactable; naik.tag = GameTags.Interactable;
                var tn = naik.AddComponent<TanggaRumahPohon>();
                tn.Pasang(new Vector3(pos[i].x, y + 6.35f, pos[i].y - 1.3f), true);
                EditorUtility.SetDirty(tn);

                var turun = Hias(rp, PrimitiveType.Cube, "TanggaTurun", new Vector3(0f, 6.5f, -2.15f), new Vector3(0.7f, 0.9f, 0.3f), kayu, true);
                turun.layer = GameLayers.Interactable; turun.tag = GameTags.Interactable;
                var tt = turun.AddComponent<TanggaRumahPohon>();
                tt.Pasang(new Vector3(pos[i].x, y + 0.2f, pos[i].y - 3.4f), false);
                EditorUtility.SetDirty(tt);

                // Kasur tidur & simpan di atas dek aman
                var kasur = Hias(rp, PrimitiveType.Cube, "KasurAman", new Vector3(1.1f, 6.25f, 1.0f), new Vector3(1.2f, 0.22f, 2.0f), kain, true);
                kasur.layer = GameLayers.Interactable; kasur.tag = GameTags.Interactable;
                kasur.AddComponent<KasurRumahPohon>();
                JumlahRumahPohon++;
            }
        }

        static void BangunSarangDanJejakPemangsa(GameObject induk)
        {
            var grup = Anak(induk, "SarangPemangsa");
            var tulang = MatPolos("M_TulangSarang", "#DBD2BC");
            var jejakMat = MatPolos("M_JejakCakar", "#3A2218");
            string[] nama = { "Harimau", "Babi Hutan", "Buaya Rawa", "Ular Piton" };
            Vector2[] pos = { new Vector2(-220f, -280f), new Vector2(185f, -140f), new Vector2(-670f, -590f), new Vector2(290f, 260f) };

            for (int i = 0; i < nama.Length; i++)
            {
                Vector3 ps = new Vector3(pos[i].x, WorldTerrainGenerator.SampleHeight(pos[i].x, pos[i].y), pos[i].y);
                var sar = Anak(grup, "Sarang_" + i);
                sar.transform.position = ps;
                sar.layer = GameLayers.Interactable; sar.tag = GameTags.Interactable;
                var bc = sar.AddComponent<BoxCollider>(); bc.size = new Vector3(2.0f, 0.8f, 2.0f); bc.center = new Vector3(0f, 0.4f, 0f);
                for (int k = 0; k < 5; k++)
                {
                    float a = k * 1.25f;
                    Hias(sar, PrimitiveType.Capsule, "Tulang_" + k, new Vector3(Mathf.Cos(a) * 0.7f, 0.14f, Mathf.Sin(a) * 0.7f), new Vector3(0.11f, 0.32f, 0.11f), tulang, false).transform.localRotation = Quaternion.Euler(80f, k * 45f, 0f);
                }
                var sp = sar.AddComponent<SarangPemangsa>(); sp.Pasang(nama[i]); EditorUtility.SetDirty(sp);

                // 2 Jejak cakar di jalur 25 m & 48 m dari sarang
                for (int j = 1; j <= 2; j++)
                {
                    float jx = pos[i].x + j * 22f, jz = pos[i].y - j * 16f;
                    var jGo = Hias(grup, PrimitiveType.Cube, "JejakBuruan_" + i + "_" + j, new Vector3(jx, WorldTerrainGenerator.SampleHeight(jx, jz) + 0.05f, jz), new Vector3(0.7f, 0.08f, 0.7f), jejakMat, true);
                    jGo.layer = GameLayers.Interactable; jGo.tag = GameTags.Interactable;
                    var jb = jGo.AddComponent<JejakBuruan>(); jb.Pasang(ps, nama[i]); EditorUtility.SetDirty(jb);
                }
                JumlahSarangPemangsa++;
            }
        }

        static void BangunBossMiniButoIjo(GameObject induk)
        {
            var grup = Anak(induk, "BossMiniButoIjo");
            var kulitIjo = MatPolos("M_ButoIjoKulit", "#264A28");
            var mataMerah = SimpanMat(SafeMaterialFactory.CreatePBR("M_MataButoIjo", Color.black, 0f, 0.9f, null, null, new Color(4.5f, 0.35f, 0.15f)));
            string[] gelar = { "Buto Ijo Penjaga Watu Ireng", "Buto Ijo Pabrik Gula" };
            Vector2[] pos = { new Vector2(-380f, 220f), new Vector2(688f, -522f) };

            for (int i = 0; i < pos.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(pos[i].x, pos[i].y);
                var b = Anak(grup, "ButoIjo_" + i);
                b.transform.position = new Vector3(pos[i].x, y, pos[i].y);
                b.transform.localScale = Vector3.one * 1.45f;
                b.layer = GameLayers.Enemy; b.tag = GameTags.Enemy;
                var cc = b.AddComponent<CapsuleCollider>(); cc.height = 2.6f; cc.radius = 0.75f; cc.center = new Vector3(0f, 1.3f, 0f);
                Hias(b, PrimitiveType.Capsule, "Torso", new Vector3(0f, 1.45f, 0f), new Vector3(0.95f, 0.95f, 0.75f), kulitIjo, false);
                Hias(b, PrimitiveType.Sphere, "Kepala", new Vector3(0f, 2.55f, 0.08f), new Vector3(0.58f, 0.62f, 0.58f), kulitIjo, false);
                Hias(b, PrimitiveType.Sphere, "MataL", new Vector3(-0.14f, 2.62f, 0.34f), Vector3.one * 0.09f, mataMerah, false);
                Hias(b, PrimitiveType.Sphere, "MataR", new Vector3(0.14f, 2.62f, 0.34f), Vector3.one * 0.09f, mataMerah, false);
                Hias(b, PrimitiveType.Capsule, "LenganL", new Vector3(-0.78f, 1.45f, 0.1f), new Vector3(0.32f, 0.78f, 0.32f), kulitIjo, false);
                Hias(b, PrimitiveType.Capsule, "LenganR", new Vector3(0.78f, 1.45f, 0.1f), new Vector3(0.32f, 0.78f, 0.32f), kulitIjo, false);
                var ai = b.AddComponent<BossMiniButoIjo>(); ai.Pasang(gelar[i]); EditorUtility.SetDirty(ai);
                JumlahButoIjo++;
            }
        }

        static void BangunSuarSinyalDarurat(GameObject induk)
        {
            var grup = Anak(induk, "SuarSinyalDarurat");
            var logam = AmbilMat("M_LogamGelap");
            var apiMerah = SimpanMat(SafeMaterialFactory.CreatePBR("M_ApiSuarMerah", new Color(1f, 0.15f, 0.12f), 0f, 0.1f, null, null, new Color(8f, 1.1f, 0.6f)));
            Vector2[] pos = { new Vector2(12f, -446f), new Vector2(-180f, -866f), new Vector2(536f, 198f) };

            for (int i = 0; i < pos.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(pos[i].x, pos[i].y);
                var go = Anak(grup, "SuarDarurat_" + i);
                go.transform.position = new Vector3(pos[i].x, y, pos[i].y);
                go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
                var bc = go.AddComponent<BoxCollider>(); bc.size = new Vector3(0.8f, 1.2f, 0.8f); bc.center = new Vector3(0f, 0.6f, 0f);
                Hias(go, PrimitiveType.Cylinder, "TabungSuar", new Vector3(0f, 0.45f, 0f), new Vector3(0.22f, 0.45f, 0.22f), logam, false);
                var api = Hias(go, PrimitiveType.Sphere, "NyalaMerah", new Vector3(0f, 0.98f, 0f), new Vector3(0.22f, 0.32f, 0.22f), apiMerah, false);
                var amr = api.GetComponent<MeshRenderer>(); amr.shadowCastingMode = ShadowCastingMode.Off; amr.enabled = false;
                var lGo = Anak(go, "LampuSuar"); lGo.transform.localPosition = new Vector3(0f, 1.3f, 0f);
                var l = lGo.AddComponent<Light>(); l.type = LightType.Point; l.color = new Color(1f, 0.18f, 0.14f); l.range = 24f; l.intensity = 4.2f; l.shadows = LightShadows.None; l.enabled = false;
                var s = go.AddComponent<SuarSinyalDarurat>(); s.Pasang(l, amr); EditorUtility.SetDirty(s);
                JumlahSuarDarurat++;
            }
        }
    }
}
