using AlasKeramat.Core;
using AlasKeramat.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.EditorTools
{
    // Langkah 7z14: Overhaul Kontrol Mobile Android (tombol cepat Senter, Auto-Lari, Obat Cepat + Penanda Landmark HUD)
    // serta pengayaan visual dunia (36 patok jalan setapak leading-lines, 60 rumpun bunga tropis, 2 pos penjaga hutan/pantai).
    public static partial class NusantaraMasterBuilder
    {
        public static int JumlahPatokJalan, JumlahBungaTropis, JumlahPosPenjaga;

        static void LangkahOverhaulKontrolDanDunia()
        {
            JumlahPatokJalan = 0; JumlahBungaTropis = 0; JumlahPosPenjaga = 0;
            var lm = GameObject.Find(NamaRoot + "/World/Landmark") ?? GameObject.Find(NamaRoot + "/Landmark") ?? _root;
            var induk = Anak(lm, "VisualDanJalurDunia");

            PasangTombolCepatAndroid();
            PasangPenandaLandmarkHUD();
            BangunPatokJalanSetapak(induk);
            BangunBungaTropisLiar(induk);
            BangunPosPenjagaHutanDanPantai(induk);

            Catat("Overhaul Kontrol Mobile & Dunia: 3 tombol cepat Android (Senter/Auto-Lari/Obat), Penanda Landmark HUD, "
                + JumlahPatokJalan + " patok jalur, " + JumlahBungaTropis + " rumpun bunga tropis & " + JumlahPosPenjaga + " pos penjaga");
        }

        static void PasangTombolCepatAndroid()
        {
            var tc = Object.FindFirstObjectByType<TouchControls>(FindObjectsInactive.Include);
            if (tc == null) return;
            var canvas = tc.GetComponentInParent<Canvas>();
            if (canvas == null) return;
            var ikonBulat = AssetDatabase.LoadAssetAtPath<Sprite>(FolderIkon + "/ui_lingkaran.png");

            BuatTombolCepat(canvas.transform, "TombolSenterCepat", "SENTER", new Vector2(0f, 0f), new Vector2(92f, 295f), new Color(0.95f, 0.82f, 0.35f, 0.78f), ikonBulat, tc.TekanSenter);
            BuatTombolCepat(canvas.transform, "TombolAutoLari", "AUTO", new Vector2(0f, 0f), new Vector2(182f, 295f), new Color(0.42f, 0.86f, 0.62f, 0.78f), ikonBulat, tc.TekanAutoLari);
            BuatTombolCepat(canvas.transform, "TombolObatCepat", "OBAT", new Vector2(1f, 0f), new Vector2(-240f, 295f), new Color(0.92f, 0.36f, 0.32f, 0.80f), ikonBulat, tc.TekanObatCepat);
        }

        static void BuatTombolCepat(Transform induk, string nama, string label, Vector2 anchor, Vector2 pos, Color warna, Sprite sprite, UnityEngine.Events.UnityAction aksi)
        {
            if (induk.Find(nama) != null) return;
            var go = new GameObject(nama, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(induk, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(74f, 74f);
            var img = go.GetComponent<Image>();
            img.color = warna;
            if (sprite != null) { img.sprite = sprite; img.preserveAspect = true; }
            var btn = go.GetComponent<Button>();
            UnityEventTools.AddPersistentListener(btn.onClick, aksi);

            var tGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            tGo.transform.SetParent(go.transform, false);
            var trt = (RectTransform)tGo.transform;
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
            var txt = tGo.GetComponent<Text>();
            txt.font = ProceduralAssetFactory.GetSafeFont();
            txt.fontSize = 14; txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white; txt.raycastTarget = false;
            txt.text = label;
            EditorUtility.SetDirty(btn);
        }

        static void PasangPenandaLandmarkHUD()
        {
            var hud = Object.FindFirstObjectByType<HUDInfo>(FindObjectsInactive.Include);
            if (hud == null) return;
            var canvas = hud.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.transform.Find("PenandaLandmarkStrip") != null) return;

            var strip = new GameObject("PenandaLandmarkStrip", typeof(RectTransform), typeof(Image));
            strip.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)strip.transform;
            rt.anchorMin = new Vector2(1f, 1f); rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f); rt.anchoredPosition = new Vector2(-12f, -184f);
            rt.sizeDelta = new Vector2(198f, 22f);
            var bg = strip.GetComponent<Image>();
            bg.color = new Color(0.04f, 0.06f, 0.07f, 0.50f); bg.raycastTarget = false;

            var tGo = new GameObject("TeksLandmark", typeof(RectTransform), typeof(Text));
            tGo.transform.SetParent(strip.transform, false);
            var trt = (RectTransform)tGo.transform;
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(5f, 1f); trt.offsetMax = new Vector2(-5f, -1f);
            var txt = tGo.GetComponent<Text>();
            txt.font = ProceduralAssetFactory.GetSafeFont();
            txt.fontSize = 11; txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(0.95f, 0.85f, 0.48f, 0.94f); txt.raycastTarget = false;
            txt.text = "TERDEKAT: Lokasi Heli";

            var titik = new[]
            {
                new PenandaLandmarkHUD.TitikLandmark { nama = "Bangkai Heli", posisi = new Vector3(8f, 0f, -448f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Kampung Sukamaju", posisi = new Vector3(56f, 0f, -376f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Desa Mati", posisi = new Vector3(540f, 0f, 204f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Altar Beringin", posisi = new Vector3(0f, 0f, 0f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Air Terjun Rimba", posisi = new Vector3(312f, 0f, -165f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Candi Kuno", posisi = new Vector3(-265f, 0f, 115f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Jembatan Gantung", posisi = new Vector3(272f, 0f, -218f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Bangkai Kapal", posisi = new Vector3(-115f, 0f, -865f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Pabrik Gula", posisi = new Vector3(665f, 0f, -500f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Rumah Pohon Heli", posisi = new Vector3(46f, 0f, -418f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Desa Nelayan Timur", posisi = new Vector3(720f, 0f, -140f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Pos Gunung Kapur", posisi = new Vector3(-580f, 0f, 620f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Perkebunan Timur Laut", posisi = new Vector3(590f, 0f, 560f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Pulau Karang Selatan", posisi = new Vector3(-320f, 0f, -885f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Kuil Bambu Utara", posisi = new Vector3(-120f, 0f, 610f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Dermaga Tua Barat", posisi = new Vector3(-520f, 0f, -340f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Menara Pengawas Api", posisi = new Vector3(-195f, 0f, -240f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Gua Air Terjun Rahasia", posisi = new Vector3(326f, 0f, -158f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Pasar Hantu Desa", posisi = new Vector3(468f, 0f, 178f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Jembatan Batu Kuno", posisi = new Vector3(-240f, 0f, 88f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Kamp Peneliti", posisi = new Vector3(-390f, 0f, 310f) },
                new PenandaLandmarkHUD.TitikLandmark { nama = "Taman Batu Megalit", posisi = new Vector3(210f, 0f, 420f) }
            };
            var comp = strip.AddComponent<PenandaLandmarkHUD>();
            comp.Pasang(txt, titik);
            EditorUtility.SetDirty(comp);
        }

        static void BangunPatokJalanSetapak(GameObject induk)
        {
            var grup = Anak(induk, "PatokJalanSetapak");
            var kayu = AmbilMat("M_Kayu");
            var kainMerah = MatPolos("M_PitaJalur", "#A83224");
            Vector2[] rute = { new Vector2(12f, -442f), new Vector2(56f, -376f), new Vector2(270f, -218f), new Vector2(532f, 194f), new Vector2(12f, -26f) };
            for (int seg = 0; seg < rute.Length - 1; seg++)
            {
                for (int k = 1; k <= 9; k++)
                {
                    float t = k / 10f;
                    float x = Mathf.Lerp(rute[seg].x, rute[seg + 1].x, t);
                    float z = Mathf.Lerp(rute[seg].y, rute[seg + 1].y, t);
                    float y = WorldTerrainGenerator.SampleHeight(x, z);
                    if (y < 1.0f) continue;
                    var p = Anak(grup, "Patok_" + JumlahPatokJalan);
                    p.transform.position = new Vector3(x, y, z);
                    Hias(p, PrimitiveType.Cylinder, "Kayu", new Vector3(0f, 0.55f, 0f), new Vector3(0.08f, 0.55f, 0.08f), kayu, false);
                    Hias(p, PrimitiveType.Cube, "Pita", new Vector3(0f, 0.92f, 0f), new Vector3(0.14f, 0.14f, 0.14f), kainMerah, false);
                    JumlahPatokJalan++;
                }
            }
        }

        static void BangunBungaTropisLiar(GameObject induk)
        {
            var grup = Anak(induk, "BungaTropisLiar");
            var mats = new[]
            {
                MatPolos("M_BungaMerah", "#9C2C26"),
                MatPolos("M_BungaKuning", "#B89428"),
                MatPolos("M_BungaUngu", "#6E387E")
            };
            var mKeladi = MuatPropScan("KeladiScan");
            var mPakis = MuatPropScan("PakisScan_LOD0");
            var mRumput = MuatPropScan("RumputScanA");
            var matDaun = AmbilMat("M_DaunPohon") ?? MatPolos("M_DaunBunga", "#25441D");
            Vector2[] pusat = { new Vector2(18f, -432f), new Vector2(56f, -376f), new Vector2(516f, 186f), new Vector2(312f, -172f), new Vector2(-265f, 105f), new Vector2(0f, -22f) };
            var rnd = new System.Random(5519);
            for (int i = 0; i < 60; i++)
            {
                var c = pusat[i % pusat.Length];
                float a = (float)rnd.NextDouble() * Mathf.PI * 2f;
                float r = 7f + (float)rnd.NextDouble() * 19f;
                float x = c.x + Mathf.Cos(a) * r, z = c.y + Mathf.Sin(a) * r;
                float y = WorldTerrainGenerator.SampleHeight(x, z);
                if (y < 1.0f) continue;
                var b = Anak(grup, "Bunga_" + i);
                b.transform.position = new Vector3(x, y - 0.03f, z);
                b.transform.rotation = Quaternion.Euler(0f, (float)rnd.NextDouble() * 360f, 0f);

                // Pakai mesh daun scan asli (Keladi/Pakis/Rumput) + kelopak tipis menyilang (bukan bola kartun).
                var scan = (i % 3 == 0 ? mKeladi : (i % 3 == 1 ? mPakis : mRumput)) ?? mKeladi ?? mPakis;
                if (scan != null)
                {
                    var d = Anak(b, "DaunScan");
                    d.transform.localScale = Vector3.one * (0.65f + (float)rnd.NextDouble() * 0.45f);
                    d.AddComponent<MeshFilter>().sharedMesh = scan;
                    var mr = d.AddComponent<MeshRenderer>();
                    mr.sharedMaterial = matDaun;
                    mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
                else
                {
                    var q1 = Hias(b, PrimitiveType.Quad, "DaunA", new Vector3(0f, 0.18f, 0f), new Vector3(0.45f, 0.32f, 1f), matDaun, false);
                    q1.transform.localRotation = Quaternion.Euler(25f, 0f, 0f);
                }
                var kA = Hias(b, PrimitiveType.Quad, "KelopakA", new Vector3(0.05f, 0.28f, 0.04f), new Vector3(0.11f, 0.11f, 1f), mats[i % mats.Length], false);
                kA.transform.localRotation = Quaternion.Euler(55f, 25f, 0f);
                kA.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                var kB = Hias(b, PrimitiveType.Quad, "KelopakB", new Vector3(-0.07f, 0.24f, -0.05f), new Vector3(0.09f, 0.09f, 1f), mats[(i + 1) % mats.Length], false);
                kB.transform.localRotation = Quaternion.Euler(62f, 145f, 0f);
                kB.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                JumlahBungaTropis++;
            }
        }

        static void BangunPosPenjagaHutanDanPantai(GameObject induk)
        {
            var kayu = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_KayuLapuk2K.mat") ?? AmbilMat("M_Kayu");
            var genteng = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_GentengLumut.mat") ?? kayu;
            Vector2[] pos = { new Vector2(165f, -305f), new Vector2(-60f, -820f) };
            for (int i = 0; i < pos.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(pos[i].x, pos[i].y);
                var g = Anak(induk, "PosPenjaga_" + i);
                g.transform.position = new Vector3(pos[i].x, y, pos[i].y);
                Hias(g, PrimitiveType.Cube, "Lantai", new Vector3(0f, 0.4f, 0f), new Vector3(3.8f, 0.25f, 3.8f), kayu, true);
                for (int k = 0; k < 4; k++)
                {
                    float sx = (k % 2 == 0 ? -1f : 1f) * 1.65f, sz = (k < 2 ? -1f : 1f) * 1.65f;
                    Hias(g, PrimitiveType.Cylinder, "Tiang_" + k, new Vector3(sx, 1.6f, sz), new Vector3(0.14f, 1.5f, 0.14f), kayu, true);
                }
                Hias(g, PrimitiveType.Cube, "Atap", new Vector3(0f, 3.2f, 0f), new Vector3(4.5f, 0.28f, 4.5f), genteng, false);
                JumlahPosPenjaga++;
            }
        }
    }
}
