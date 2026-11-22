using System.Collections.Generic;
using AlasKeramat.AI;
using AlasKeramat.Building;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.EditorTools
{
    // Langkah 7z15: Ekspansi Penuh 4 Kuadran Pulau (4 km2) - 5 Zona Besar Baru (Desa Nelayan Timur, Pos Gunung Kapur + Zipline 320m,
    // Perkebunan & Gudang Kolonial Timur Laut, Pulau Karang Lepas Pantai Selatan, Kuil Bambu Utara) + Ekosistem 18 Satwa Kawanan & 20 Burung Langit.
    public static partial class NusantaraMasterBuilder
    {
        public static int JumlahZonaKuadranLuar;

        static void LangkahEkspansiPulau4Km()
        {
            JumlahZonaKuadranLuar = 0;
            var lm = GameObject.Find(NamaRoot + "/World/Landmark") ?? GameObject.Find(NamaRoot + "/Landmark") ?? _root;
            var induk = Anak(lm, "EkspansiPulau4Km");

            var matKayu = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_KayuLapuk2K.mat") ?? AmbilMat("M_Kayu");
            var matBatu = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_BatuCandi2K.mat") ?? AmbilMat("M_Batu");
            var matGenteng = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_GentengLumut.mat") ?? matBatu;
            var matKarat = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_KaratPabrik2K.mat") ?? AmbilMat("M_LogamGelap");

            BangunDesaNelayanTimur(induk, matKayu, matGenteng);
            BangunPosGunungKapurDanZiplineRaksasa(induk, matKayu, matKarat);
            BangunPerkebunanTimurLaut(induk, matBatu, matGenteng);
            BangunPulauKarangSelatan(induk, matBatu, matKayu);
            BangunKuilBambuUtara(induk, matKayu, matBatu);
            BangunEkosistemHidup4Km(induk);

            Catat("Ekspansi Pulau 4 km2: " + JumlahZonaKuadranLuar + " zona besar baru di 4 kuadran luar + 18 satwa kawanan & 20 burung langit");
        }

        static void BangunDesaNelayanTimur(GameObject induk, Material kayu, Material genteng)
        {
            var go = Anak(induk, "DesaNelayanTimur");
            var bilik = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_Bilik.mat") ?? kayu;
            float cx = 720f, cz = -140f;
            for (int i = 0; i < 5; i++)
            {
                float x = cx + (i % 3) * 14f, z = cz + (i / 3) * 16f;
                float y = WorldTerrainGenerator.SampleHeight(x, z);
                // Rumah panggung Nusantara asli (atap pelana miring + kolong + beranda), bukan 3 kubus datar.
                var meshRumah = Simpan(ProcBangunan.RumahPanggung(720 + i, 6.2f, 5.0f, 1.35f, 0.25f));
                var r = Anak(go, "RumahNelayanTimur_" + i);
                r.transform.position = new Vector3(x, y, z);
                r.transform.rotation = Quaternion.Euler(0f, (i % 2 == 0 ? 180f : 0f) + i * 7f, 0f);
                r.AddComponent<MeshFilter>().sharedMesh = meshRumah;
                r.AddComponent<MeshRenderer>().sharedMaterials = new[] { i % 2 == 0 ? bilik : kayu, genteng };
                r.AddComponent<MeshCollider>().sharedMesh = meshRumah;
                var peti = Hias(r, PrimitiveType.Cube, "PetiNelayan", new Vector3(3.4f, 0.35f, 0f), new Vector3(0.9f, 0.7f, 0.7f), kayu, true);
                peti.layer = GameLayers.Interactable; peti.tag = GameTags.Interactable;
                var pg = peti.AddComponent<PropGeledah>(); pg.Jenis = PropGeledah.Wadah.Peti; EditorUtility.SetDirty(pg);
            }
            // Dermaga Timur & Perahu Dayung Timur
            float yd = WorldTerrainGenerator.SampleHeight(768f, -140f);
            Hias(go, PrimitiveType.Cube, "DermagaTimur", new Vector3(768f, Mathf.Max(yd, 0.6f), -140f), new Vector3(18f, 0.3f, 3.2f), kayu, true);
            var pr = Anak(go, "PerahuTimur");
            pr.transform.position = new Vector3(780f, 0.2f, -142f);
            pr.layer = GameLayers.Interactable; pr.tag = GameTags.Interactable;
            var bc = pr.AddComponent<BoxCollider>(); bc.size = new Vector3(1.5f, 0.8f, 3.4f); bc.center = new Vector3(0f, 0.35f, 0f);
            Hias(pr, PrimitiveType.Cube, "Lambung", new Vector3(0f, 0.14f, 0f), new Vector3(1.35f, 0.24f, 3.2f), kayu, false);
            pr.AddComponent<PerahuDayung>();
            JumlahZonaKuadranLuar++;
        }

        static void BangunPosGunungKapurDanZiplineRaksasa(GameObject induk, Material kayu, Material karat)
        {
            float x = -580f, z = 620f;
            float y = WorldTerrainGenerator.SampleHeight(x, z);
            var go = Anak(induk, "PosGunungKapur");
            go.transform.position = new Vector3(x, y, z);

            // Menara Radio Puncak 18 m + Pos Penelitian
            Hias(go, PrimitiveType.Cube, "PosRiset", new Vector3(0f, 1.5f, 0f), new Vector3(6.5f, 3.0f, 5.0f), karat, true);
            Hias(go, PrimitiveType.Cylinder, "MenaraRadioPuncak", new Vector3(5.5f, 9.0f, 2f), new Vector3(0.45f, 9.0f, 0.45f), karat, true);
            Hias(go, PrimitiveType.Cube, "AntenaSilang", new Vector3(5.5f, 17.2f, 2f), new Vector3(3.8f, 0.18f, 0.18f), karat, false);

            // Zipline Raksasa dari Puncak Gunung Kapur -> Lembah Barat (~320 m!)
            Vector3 p0 = new Vector3(x + 3f, y + 3.4f, z - 6f);
            Vector3 p1 = new Vector3(-340f, WorldTerrainGenerator.SampleHeight(-340f, 390f) + 2.8f, 390f);
            var zGo = Anak(go, "ZiplineGunungKapur");
            zGo.transform.position = new Vector3(p0.x, p0.y - 3.4f, p0.z);
            zGo.layer = GameLayers.Interactable; zGo.tag = GameTags.Interactable;
            var bc = zGo.AddComponent<BoxCollider>(); bc.size = new Vector3(1.8f, 3.6f, 1.8f); bc.center = new Vector3(0f, 1.8f, 0f);
            Hias(zGo, PrimitiveType.Cylinder, "Tiang", new Vector3(0f, 1.8f, 0f), new Vector3(0.2f, 1.8f, 0.2f), kayu, true);
            var zip = zGo.AddComponent<ZiplineRimba>();
            zip.Pasang(p0, p1, "Puncak Gunung Kapur - Lembah");
            EditorUtility.SetDirty(zip);
            JumlahZonaKuadranLuar++;
        }

        static void BangunPerkebunanTimurLaut(GameObject induk, Material batu, Material genteng)
        {
            float x = 590f, z = 560f;
            float y = WorldTerrainGenerator.SampleHeight(x, z);
            var go = Anak(induk, "PerkebunanTimurLaut");
            go.transform.position = new Vector3(x, y, z);

            // 2 Gudang Kolonial Tua beratap pelana miring & pilar beranda.
            for (int i = 0; i < 2; i++)
            {
                var g = Anak(go, "GudangKolonial_" + i);
                g.transform.localPosition = new Vector3(i * 15f, 0f, 0f);
                Hias(g, PrimitiveType.Cube, "Badan", new Vector3(0f, 2.2f, 0f), new Vector3(9.5f, 4.4f, 6.5f), batu, true);
                Hias(g, PrimitiveType.Cube, "AtapL", new Vector3(0f, 5.15f, -1.9f), new Vector3(10.8f, 0.26f, 4.4f), genteng, false).transform.localRotation = Quaternion.Euler(-24f, 0f, 0f);
                Hias(g, PrimitiveType.Cube, "AtapR", new Vector3(0f, 5.15f, 1.9f), new Vector3(10.8f, 0.26f, 4.4f), genteng, false).transform.localRotation = Quaternion.Euler(24f, 0f, 0f);
                Hias(g, PrimitiveType.Cube, "Bubungan", new Vector3(0f, 6.05f, 0f), new Vector3(10.9f, 0.28f, 0.45f), genteng, false);
            }
            // 24 baris tanaman teh/perdu (memakai mesh pakis/rumput scan organik bila ada, bukan balok kotak polos).
            var tehMat = MatPolos("M_PerduTeh", "#2C5422");
            var perduScan = MuatPropScan("PakisScan_LOD0") ?? MuatPropScan("RumputScanA");
            var matPerduScan = perduScan != null ? (MaterialFolScan("PakisAsli", "M_PerduTehScan", 0.35f, 0.06f, new Color(0.68f, 0.84f, 0.56f, 1f)) ?? tehMat) : tehMat;
            for (int r = 0; r < 24; r++)
            {
                float px = x - 18f + (r % 6) * 8f, pz = z - 24f - (r / 6) * 7f;
                float py = WorldTerrainGenerator.SampleHeight(px, pz);
                if (perduScan != null)
                {
                    var row = Anak(induk, "BarisTeh_" + r);
                    row.transform.position = new Vector3(px, py, pz);
                    for (int k = -1; k <= 1; k++)
                    {
                        var b = Anak(row, "Semak_" + (k + 1));
                        b.transform.localPosition = new Vector3(k * 2.1f, 0f, 0f);
                        b.transform.localRotation = Quaternion.Euler(0f, r * 37f + k * 60f, 0f);
                        b.transform.localScale = new Vector3(1.8f, 1.25f, 1.5f);
                        b.AddComponent<MeshFilter>().sharedMesh = perduScan;
                        var bmr = b.AddComponent<MeshRenderer>(); bmr.sharedMaterial = matPerduScan; bmr.shadowCastingMode = ShadowCastingMode.Off;
                    }
                }
                else
                {
                    Hias(induk, PrimitiveType.Cube, "BarisTeh_" + r, new Vector3(px, py + 0.45f, pz), new Vector3(6.2f, 0.9f, 1.4f), tehMat, false);
                }
            }
            JumlahZonaKuadranLuar++;
        }

        static void BangunPulauKarangSelatan(GameObject induk, Material batu, Material kayu)
        {
            float x = -320f, z = -885f;
            var go = Anak(induk, "PulauKarangSelatan");
            go.transform.position = new Vector3(x, 0f, z);
            // Daratan pulau karang + 10 bongkahan tebing batu scan 4K alami.
            Hias(go, PrimitiveType.Cylinder, "KarangPulau", new Vector3(0f, 0.8f, 0f), new Vector3(24f, 2.2f, 20f), batu, true);
            Hias(go, PrimitiveType.Cylinder, "MenaraKarang", new Vector3(0f, 4.5f, 0f), new Vector3(2.8f, 3.2f, 2.8f), batu, true);
            var batuScan = MuatPropScan("BatuBesarScan") ?? MuatPropScan("BatuLumutA");
            var matKarang = MaterialSolidScan("BatuLumutAsli", "M_KarangScanV10", new Color(0.76f, 0.78f, 0.74f, 1f), 0.25f) ?? batu;
            if (batuScan != null)
            {
                for (int b = 0; b < 10; b++)
                {
                    float ang = b * (Mathf.PI * 2f / 10f);
                    var bk = Anak(go, "TebingKarang_" + b);
                    bk.transform.localPosition = new Vector3(Mathf.Cos(ang) * 9.5f, 1.1f + (b % 3) * 0.7f, Mathf.Sin(ang) * 8.2f);
                    bk.transform.localRotation = Quaternion.Euler(b * 17f, b * 49f, 0f);
                    bk.transform.localScale = Vector3.one * (2.4f + (b % 3) * 0.85f);
                    bk.AddComponent<MeshFilter>().sharedMesh = batuScan;
                    bk.AddComponent<MeshRenderer>().sharedMaterial = matKarang;
                }
            }
            // Peti harta karun pulau karang
            var peti = Hias(go, PrimitiveType.Cube, "PetiPulauKarang", new Vector3(3.2f, 2.15f, 1.5f), new Vector3(1.1f, 0.7f, 0.8f), kayu, true);
            peti.layer = GameLayers.Interactable; peti.tag = GameTags.Interactable;
            var pg = peti.AddComponent<PropGeledah>(); pg.Jenis = PropGeledah.Wadah.Peti; EditorUtility.SetDirty(pg);
            JumlahZonaKuadranLuar++;
        }

        static void BangunKuilBambuUtara(GameObject induk, Material kayu, Material batu)
        {
            float x = -120f, z = 610f;
            float y = WorldTerrainGenerator.SampleHeight(x, z);
            var go = Anak(induk, "KuilBambuUtara");
            go.transform.position = new Vector3(x, y, z);
            var genteng = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_AtapIjuk.mat") ?? kayu;
            Hias(go, PrimitiveType.Cube, "Pelataran", new Vector3(0f, 0.35f, 0f), new Vector3(9.5f, 0.7f, 9.5f), batu, true);
            // Atap Joglo/Meru 2 tingkat miring khas kuil Nusantara (bukan kubus datar).
            var atapKuil = Anak(go, "AtapKuil"); atapKuil.transform.localPosition = new Vector3(0f, 4.2f, 0f);
            Hias(atapKuil, PrimitiveType.Cube, "Tingkat1U", new Vector3(0f, 0.35f, 2.2f), new Vector3(8.6f, 0.20f, 4.8f), genteng, false).transform.localRotation = Quaternion.Euler(22f, 0f, 0f);
            Hias(atapKuil, PrimitiveType.Cube, "Tingkat1S", new Vector3(0f, 0.35f, -2.2f), new Vector3(8.6f, 0.20f, 4.8f), genteng, false).transform.localRotation = Quaternion.Euler(-22f, 0f, 0f);
            Hias(atapKuil, PrimitiveType.Cube, "Tingkat2U", new Vector3(0f, 1.85f, 1.25f), new Vector3(5.2f, 0.20f, 2.9f), genteng, false).transform.localRotation = Quaternion.Euler(30f, 0f, 0f);
            Hias(atapKuil, PrimitiveType.Cube, "Tingkat2S", new Vector3(0f, 1.85f, -1.25f), new Vector3(5.2f, 0.20f, 2.9f), genteng, false).transform.localRotation = Quaternion.Euler(-30f, 0f, 0f);
            Hias(atapKuil, PrimitiveType.Cube, "Mustaka", new Vector3(0f, 2.85f, 0f), new Vector3(0.55f, 0.85f, 0.55f), batu, false);
            for (int i = 0; i < 4; i++)
                Hias(go, PrimitiveType.Cylinder, "TiangKuil_" + i, new Vector3((i % 2 == 0 ? -3f : 3f), 2.2f, (i < 2 ? -3f : 3f)), new Vector3(0.22f, 2.0f, 0.22f), kayu, true);
            var altar = Hias(go, PrimitiveType.Cube, "AltarSesajenUtara", new Vector3(0f, 1.0f, 0f), new Vector3(1.4f, 0.8f, 1.0f), batu, true);
            altar.layer = GameLayers.Interactable; altar.tag = GameTags.Interactable;
            altar.AddComponent<PohonKeramatSesajen>();
            JumlahZonaKuadranLuar++;
        }

        static void BangunEkosistemHidup4Km(GameObject induk)
        {
            var go = Anak(induk, "EkosistemHidupPulau");
            var matRusa = MatPolos("M_KawananRusa", "#8A623C");
            var matBabi = MatPolos("M_KawananBabi", "#3E3028");
            var matCamar = MatPolos("M_BurungCamar", "#EAECEB");
            var matElang = MatPolos("M_BurungElang", "#3E2C1E");

            var daftarSatwa = new List<EkosistemHidupPulau.SatwaKawanan>();
            Vector2[] padang = { new Vector2(-420f, 440f), new Vector2(460f, 420f), new Vector2(580f, -120f), new Vector2(-380f, -240f), new Vector2(80f, 520f), new Vector2(-560f, 120f) };
            for (int i = 0; i < 18; i++)
            {
                var p = padang[i % padang.Length];
                float x = p.x + Mathf.Cos(i * 1.3f) * 8f, z = p.y + Mathf.Sin(i * 1.3f) * 8f;
                float y = WorldTerrainGenerator.SampleHeight(x, z);
                bool babi = i % 3 == 2;
                var s = Anak(go, (babi ? "BabiKawanan_" : "RusaKawanan_") + i);
                s.transform.position = new Vector3(x, y, z);
                var m = babi ? matBabi : matRusa;
                Hias(s, PrimitiveType.Capsule, "Badan", new Vector3(0f, babi ? 0.5f : 0.9f, 0f), babi ? new Vector3(0.42f, 0.55f, 0.45f) : new Vector3(0.45f, 0.72f, 0.45f), m, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Hias(s, PrimitiveType.Sphere, "Kepala", new Vector3(0f, babi ? 0.58f : 1.32f, babi ? 0.55f : 0.62f), Vector3.one * (babi ? 0.34f : 0.30f), m, false);
                daftarSatwa.Add(new EkosistemHidupPulau.SatwaKawanan
                {
                    tr = s.transform, pusat = new Vector3(p.x, y, p.y),
                    radius = 12f + (i % 4) * 5f, laju = 0.14f + (i % 3) * 0.05f, fase = i * 1.2f
                });
            }

            var daftarBurung = new List<EkosistemHidupPulau.BurungLangit>();
            Vector3[] langit =
            {
                new Vector3(-115f, 26f, -855f), new Vector3(740f, 24f, -140f), new Vector3(-580f, 72f, 620f),
                new Vector3(312f, 38f, -165f), new Vector3(0f, 46f, 0f)
            };
            for (int i = 0; i < 20; i++)
            {
                var c = langit[i % langit.Length];
                bool camar = (i % langit.Length) < 2;
                var b = Anak(go, (camar ? "Camar_" : "Elang_") + i);
                b.transform.position = c;
                var m = camar ? matCamar : matElang;
                Hias(b, PrimitiveType.Capsule, "Tubuh", Vector3.zero, new Vector3(0.18f, 0.36f, 0.18f), m, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                var sl = Hias(b, PrimitiveType.Cube, "SayapL", new Vector3(-0.55f, 0f, 0f), new Vector3(1.1f, 0.04f, 0.32f), m, false);
                var sr = Hias(b, PrimitiveType.Cube, "SayapR", new Vector3(0.55f, 0f, 0f), new Vector3(1.1f, 0.04f, 0.32f), m, false);
                daftarBurung.Add(new EkosistemHidupPulau.BurungLangit
                {
                    badan = b.transform, sayapL = sl.transform, sayapR = sr.transform,
                    pusat = c + new Vector3(0f, (i % 4) * 3.5f, 0f),
                    radius = 22f + (i % 5) * 9f, laju = 0.24f + (i % 3) * 0.07f, fase = i * 1.4f
                });
            }

            var eko = go.AddComponent<EkosistemHidupPulau>();
            eko.Pasang(daftarSatwa.ToArray(), daftarBurung.ToArray());
            EditorUtility.SetDirty(eko);
        }
    }
}
