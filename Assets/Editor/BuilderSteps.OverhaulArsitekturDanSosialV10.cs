using System.Collections.Generic;
using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.EditorTools
{
    // Langkah 7z23: Overhaul Arsitektur, Interior 4K, Kehidupan Sosial Companion & Catatan Dunia V10:
    // - Pasang InteraksiSosialTeman pada Kang Deden & Ningsih (sapaan dinamis, hadiah herbal, peringatan cuaca/malam)
    // - Pasang 12 CatatanDuniaHidup (catatan ekspedisi & cerita lokal bisa dibaca) di 12 reruntuhan utama pulau
    // - Pasang CuacaHidupDunia (petir langit + guruh bertunda + kabut dinamis reaktif badai)
    // - Isi interior/beranda Pendopo, Pabrik Gula, Menara Pengawas Api, Kamp Peneliti & Rumah Desa dengan mebel 4K scan asli.
    public static partial class NusantaraMasterBuilder
    {
        public static int JumlahCatatanV10, JumlahInteriorScanV10;

        static void LangkahOverhaulArsitekturDanSosialV10()
        {
            JumlahCatatanV10 = 0; JumlahInteriorScanV10 = 0;
            var lm = GameObject.Find(NamaRoot + "/World/Landmark") ?? GameObject.Find(NamaRoot + "/Landmark") ?? _root;
            var induk = Anak(lm, "DuniaSosialDanInteriorV10");

            PasangSosialCompanionV10();
            Pasang12CatatanDuniaV10(induk);
            PasangInteriorMebel4KV10(induk);
            PasangCuacaHidupDuniaV10();

            Catat("Overhaul Dunia V10: sosial Kang Deden & Ningsih, " + JumlahCatatanV10
                + " catatan cerita 12 lokasi, " + JumlahInteriorScanV10 + " mebel interior 4K & cuaca badai hidup");
        }

        static void PasangSosialCompanionV10()
        {
            var deden = GameObject.Find("KangDeden");
            if (deden != null)
            {
                var s = deden.GetComponent<InteraksiSosialTeman>() ?? deden.AddComponent<InteraksiSosialTeman>();
                s.Pasang(InteraksiSosialTeman.PeranTeman.KangDeden);
                EditorUtility.SetDirty(s);
            }
            var ningsih = GameObject.Find("Ningsih");
            if (ningsih != null)
            {
                var s = ningsih.GetComponent<InteraksiSosialTeman>() ?? ningsih.AddComponent<InteraksiSosialTeman>();
                s.Pasang(InteraksiSosialTeman.PeranTeman.Ningsih);
                EditorUtility.SetDirty(s);
            }
        }

        static void Pasang12CatatanDuniaV10(GameObject induk)
        {
            var grup = Anak(induk, "CatatanDunia12Lokasi");
            var bukuMesh = MuatPropScan("BukuCatatanScan");
            var bukuMat = bukuMesh != null
                ? (MaterialSolidScan("PropBukuCatatan", "M_V10_BukuCatatan", new Color(0.92f, 0.88f, 0.80f, 1f), 0.25f) ?? AmbilMat("M_Kayu"))
                : MatPolos("M_BukuCatatanV10", "#D4C29A");
            var mejaMat = AmbilMat("M_Kayu");

            var catatan = new[]
            {
                new { j = "Log Penerbangan SAR-04", isi = "Turbin kehilangan daya tepat di atas Kawah Beringin. Kompas berputar liar sebelum jatuh.", x = 13f, z = -444f },
                new { j = "Surat Kepala Desa Mati", isi = "Warga mulai mendengar kentongan tiap Maghrib. Yang menolak ikut ritual menghilang ke gua.", x = 518f, z = 200f },
                new { j = "Daftar Dagang Pasar Hantu", isi = "Tukar garam, kemenyan, dan minyak jarak sebelum matahari terbenam. Jangan menoleh jika dipanggil.", x = 470f, z = 180f },
                new { j = "Prasasti Beringin Agung", isi = "Akar purba mengikat perjanjian lama. Nyalakan dupa agar penjaga rimba tidak murka.", x = 4f, z = -16f },
                new { j = "Catatan Arkeolog Candi", isi = "Arca Dwarapala menghadap jalur batu kuno. Di bawah candi ada lorong udara menuju bunker utara.", x = -262f, z = 112f },
                new { j = "Buku Mandor Sinar Tebu", isi = "Mesin giling berhenti sendiri pada tahun 1978 setelah asap merah keluar dari cerobong.", x = 656f, z = -504f },
                new { j = "Laporan Menara Pengawas", isi = "Dari puncak 12 meter terlihat obor bergerak antara Desa Mati dan Jembatan Gantung setiap malam.", x = -194f, z = -238f },
                new { j = "Jurnal Dr. Rendra", isi = "Sampel spora di Gua Air Terjun berpendar biru pekat saat tekanan udara turun menjelang badai.", x = -388f, z = 312f },
                new { j = "Transmisi Gunung Kapur", isi = "Sinyal radio hanya tembus dari puncak menara 18 m. Zipline disiapkan untuk evakuasi cepat.", x = -578f, z = 618f },
                new { j = "Kitab Kuil Bambu", isi = "Air pancuran utara menenangkan batin. Peziarah selalu meninggalkan sesajen sebelum menyeberangi kawah.", x = -118f, z = 608f },
                new { j = "Catatan Nelayan Timur", isi = "Ikan melompat tinggi sebelum badai besar datang. Jangan melaut menuju Pulau Karang saat malam.", x = 722f, z = -138f },
                new { j = "Pesan Pelaut Karang", isi = "Kapal kandas di batu selatan. Peti logistik kami sembunyikan di puncak karang bersama suar.", x = -318f, z = -884f }
            };

            for (int i = 0; i < catatan.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(catatan[i].x, catatan[i].z);
                var go = Anak(grup, "CatatanDunia_" + i);
                go.transform.position = new Vector3(catatan[i].x, y, catatan[i].z);
                go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
                var bc = go.AddComponent<BoxCollider>(); bc.size = new Vector3(0.95f, 1.15f, 0.95f); bc.center = new Vector3(0f, 0.55f, 0f);
                Hias(go, PrimitiveType.Cylinder, "PodiumKayu", new Vector3(0f, 0.42f, 0f), new Vector3(0.32f, 0.42f, 0.32f), mejaMat, false);
                if (bukuMesh != null)
                {
                    var bk = Anak(go, "BukuScan");
                    bk.transform.localPosition = new Vector3(0f, 0.86f, 0f);
                    bk.transform.localRotation = Quaternion.Euler(0f, i * 37f, 0f);
                    bk.transform.localScale = Vector3.one * 1.15f;
                    bk.AddComponent<MeshFilter>().sharedMesh = bukuMesh;
                    bk.AddComponent<MeshRenderer>().sharedMaterial = bukuMat;
                }
                else
                {
                    Hias(go, PrimitiveType.Cube, "Buku", new Vector3(0f, 0.88f, 0f), new Vector3(0.36f, 0.06f, 0.48f), bukuMat, false);
                }
                var comp = go.AddComponent<CatatanDuniaHidup>();
                comp.Pasang(catatan[i].j, catatan[i].isi);
                EditorUtility.SetDirty(comp);
                JumlahCatatanV10++;
            }
        }

        static void PasangInteriorMebel4KV10(GameObject induk)
        {
            string[] mNama = { "MejaBesarScan", "KursiGoyangScan", "LemariTuaScan", "RadioTuaScan", "PembakarLabScan", "PapanPeringatanScan", "MejaKayuScan", "KursiKayuScan" };
            string[] sNama = { "PropMejaBesar", "PropKursiGoyang", "PropLemariTua", "PropRadioTua", "PropPembakarLab", "PropPapanPeringatan", "PropMeja", "PropKursi" };
            var meshes = new Mesh[mNama.Length];
            var mats = new Material[mNama.Length];
            var fall = AmbilMat("M_Kayu");
            int ada = 0;
            for (int i = 0; i < mNama.Length; i++)
            {
                meshes[i] = MuatPropScan(mNama[i]);
                if (meshes[i] == null) continue;
                mats[i] = MaterialSolidScan(sNama[i], "M_V10_" + sNama[i], new Color(0.90f, 0.86f, 0.80f, 1f), 0.26f) ?? fall;
                ada++;
            }
            if (ada == 0) return;

            var grup = Anak(induk, "MebelInterior4K");
            var titik = new[]
            {
                new { x = 516f, z = 198f, ids = new[] { 0, 1, 2, 3, 6, 7 } },     // Pendopo Desa Mati
                new { x = 640f, z = -520f, ids = new[] { 0, 2, 3, 5, 5, 6 } },    // Gedung Giling Pabrik Gula
                new { x = 620f, z = -455f, ids = new[] { 0, 1, 2, 3, 6, 7 } },    // Kantor Administratur
                new { x = -390f, z = 310f, ids = new[] { 0, 3, 4, 4, 5, 7 } },    // Kamp Peneliti
                new { x = -195f, z = -240f, ids = new[] { 3, 5, 6, 7 } },         // Menara Pengawas Api
                new { x = -580f, z = 620f, ids = new[] { 0, 3, 4, 5, 7 } }        // Pos Gunung Kapur
            };
            for (int t = 0; t < titik.Length; t++)
            {
                for (int k = 0; k < titik[t].ids.Length; k++)
                {
                    int idx = titik[t].ids[k];
                    if (meshes[idx] == null) continue;
                    float ang = k * 1.1f + t * 0.6f;
                    float x = titik[t].x + Mathf.Cos(ang) * (2.6f + k * 0.45f);
                    float z = titik[t].z + Mathf.Sin(ang) * (2.6f + k * 0.45f);
                    float y = WorldTerrainGenerator.SampleHeight(x, z);
                    var g = Anak(grup, "Mebel4K_" + t + "_" + k);
                    g.transform.position = new Vector3(x, y, z);
                    g.transform.rotation = Quaternion.Euler(0f, ang * Mathf.Rad2Deg, 0f);
                    g.AddComponent<MeshFilter>().sharedMesh = meshes[idx];
                    g.AddComponent<MeshRenderer>().sharedMaterial = mats[idx];
                    JumlahInteriorScanV10++;
                }
            }
        }

        static void PasangCuacaHidupDuniaV10()
        {
            var sis = _sistem ?? GameObject.Find(NamaRoot + "/Sistem") ?? _root;
            var go = Anak(sis, "CuacaHidupDuniaV10");
            var lGo = Anak(go, "CahayaPetirLangit");
            var l = lGo.AddComponent<Light>();
            l.type = LightType.Point; l.range = 140f; l.intensity = 6f; l.color = new Color(0.76f, 0.88f, 1f); l.shadows = LightShadows.None; l.enabled = false;

            var kabutList = new List<Transform>();
            string[] namaKabut = { "KabutPasarHantuV7", "EmbunKolamAirTerjunV7", "KabutOmbakPantai_0", "KabutOmbakPantai_1", "KabutPerkebunanV7", "KabutMegalitV7" };
            for (int i = 0; i < namaKabut.Length; i++)
            {
                var k = GameObject.Find(namaKabut[i]);
                if (k != null) kabutList.Add(k.transform);
            }
            var comp = go.AddComponent<CuacaHidupDunia>();
            comp.Pasang(l, kabutList.ToArray());
            EditorUtility.SetDirty(comp);
        }
    }
}
