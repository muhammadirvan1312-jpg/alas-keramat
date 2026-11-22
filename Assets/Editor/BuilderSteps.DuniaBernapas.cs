using System.Collections.Generic;
using AlasKeramat.AI;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.EditorTools
{
    // Langkah 7z16: Dunia Bernapas & Hidup - 3 regu patroli obor sekte berjalan antar-zona, 16 monyet rimba + 12 kodok air +
    // keheningan horor hantu, 10 titik asap mengepul + 18 kain/bendera berkibar, & pertemuan alam spontan.
    public static partial class NusantaraMasterBuilder
    {
        static void LangkahDuniaBernapas()
        {
            var lm = GameObject.Find(NamaRoot + "/World/Landmark") ?? GameObject.Find(NamaRoot + "/Landmark") ?? _root;
            var induk = Anak(lm, "DuniaBernapas");

            BangunPatroliOborSekte(induk);
            BangunMonyetDanKodok(induk);
            BangunAsapDanKainBerkibar(induk);
            Anak(induk, "PertemuanDinamisRimba").AddComponent<PertemuanDinamisRimba>();

            Catat("Dunia Bernapas: 3 regu patroli obor sekte, 16 monyet & 12 kodok + eerie silence, 10 asap cerobong/tungku, 18 kain berkibar & pertemuan alam dinamis");
        }

        static void BangunPatroliOborSekte(GameObject induk)
        {
            var go = Anak(induk, "PatroliOborSekte");
            var kainTex = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/kain.png");
            var kainNorm = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/kain_n.png");
            var jubah = SimpanMat(SafeMaterialFactory.CreatePBR("M_JubahPatroli", new Color(0.32f, 0.19f, 0.15f), 0f, 0.14f, kainTex, kainNorm));
            var topeng = SimpanMat(SafeMaterialFactory.CreatePBR("M_TopengPatroli", new Color(0.76f, 0.66f, 0.50f), 0f, 0.22f, AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/kayu.png"), AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/kayu_n.png")));
            var kayu = AmbilMat("M_Kayu");
            var apiMat = SimpanMat(SafeMaterialFactory.CreatePBR("M_ApiPatroliSekte", new Color(1f, 0.56f, 0.16f), 0f, 0.1f, null, null, new Color(5.8f, 2.4f, 0.48f)));
            var apiMesh = ProcGua.ApiTungku("ApiOborPatroli", 19);
            string pApi = FolderLandmark + "/ApiOborPatroli.asset"; AssetDatabase.DeleteAsset(pApi); AssetDatabase.CreateAsset(apiMesh, pApi);
            apiMesh = AssetDatabase.LoadAssetAtPath<Mesh>(pApi);

            Vector3[] titikA = { new Vector3(525f, 0f, 190f), new Vector3(-185f, 0f, -155f), new Vector3(655f, 0f, -490f) };
            Vector3[] titikB = { new Vector3(275f, 0f, -210f), new Vector3(-25f, 0f, -45f), new Vector3(715f, 0f, -150f) };
            var daftar = new PatroliHidupPulau.ReguPatroli[titikA.Length];

            for (int i = 0; i < titikA.Length; i++)
            {
                var p = Anak(go, "ReguPatroli_" + i);
                p.transform.position = titikA[i];
                // Figur patroli sekte berjubah, berbahu, bertopeng kayu & memegang batang obor menyala.
                Hias(p, PrimitiveType.Cylinder, "JubahBawah", new Vector3(0f, 0.62f, 0f), new Vector3(0.46f, 0.62f, 0.42f), jubah, false);
                Hias(p, PrimitiveType.Cube, "BahuJubah", new Vector3(0f, 1.38f, 0f), new Vector3(0.58f, 0.34f, 0.30f), jubah, false);
                Hias(p, PrimitiveType.Sphere, "TudungKepala", new Vector3(0f, 1.78f, -0.02f), new Vector3(0.32f, 0.34f, 0.32f), jubah, false);
                Hias(p, PrimitiveType.Cube, "TopengRitual", new Vector3(0f, 1.76f, 0.14f), new Vector3(0.22f, 0.28f, 0.06f), topeng, false);
                var tongkat = Hias(p, PrimitiveType.Cylinder, "BatangObor", new Vector3(0.38f, 1.24f, 0.24f), new Vector3(0.05f, 0.56f, 0.05f), kayu, false);
                tongkat.transform.localRotation = Quaternion.Euler(14f, 0f, -8f);
                var api = Anak(p, "ApiObor");
                api.transform.localPosition = new Vector3(0.42f, 1.76f, 0.34f);
                api.transform.localScale = new Vector3(0.22f, 0.34f, 0.22f);
                api.AddComponent<MeshFilter>().sharedMesh = apiMesh;
                var amr = api.AddComponent<MeshRenderer>(); amr.sharedMaterial = apiMat; amr.shadowCastingMode = ShadowCastingMode.Off;
                var lGo = Anak(p, "CahayaObor"); lGo.transform.localPosition = new Vector3(0.42f, 1.88f, 0.34f);
                var l = lGo.AddComponent<Light>(); l.type = LightType.Point; l.color = new Color(1f, 0.56f, 0.20f); l.range = 10f; l.intensity = 2.2f; l.shadows = LightShadows.None; l.enabled = false;
                daftar[i] = new PatroliHidupPulau.ReguPatroli { tr = p.transform, obor = l, apiObor = amr, titikA = titikA[i], titikB = titikB[i], fase = i * 0.33f };
            }
            var comp = go.AddComponent<PatroliHidupPulau>();
            comp.Pasang(daftar);
            EditorUtility.SetDirty(comp);
        }

        static void BangunMonyetDanKodok(GameObject induk)
        {
            var go = Anak(induk, "MonyetDanKodok");
            var matMonyet = MatPolos("M_MonyetRimba", "#6A4E37");
            var matWajahMonyet = MatPolos("M_WajahMonyet", "#B89474");
            var matKodok = MatPolos("M_KodokRawa", "#3C6B2E");
            var matMataKodok = MatPolos("M_MataKodok", "#D8C248");
            var daftar = new List<EkosistemMonyetDanKodok.SatwaKecil>();

            Vector2[] areaMonyet = { new Vector2(305f, -172f), new Vector2(-255f, 110f), new Vector2(-115f, 595f), new Vector2(45f, -350f) };
            for (int i = 0; i < 16; i++)
            {
                var c = areaMonyet[i % areaMonyet.Length];
                float x = c.x + Mathf.Cos(i * 1.5f) * 7f, z = c.y + Mathf.Sin(i * 1.5f) * 7f;
                float y = WorldTerrainGenerator.SampleHeight(x, z) + 0.35f;
                var m = Anak(go, "MonyetLompat_" + i);
                m.transform.position = new Vector3(x, y, z);
                Hias(m, PrimitiveType.Capsule, "Badan", Vector3.zero, new Vector3(0.24f, 0.30f, 0.26f), matMonyet, false);
                Hias(m, PrimitiveType.Sphere, "Kepala", new Vector3(0f, 0.34f, 0.10f), Vector3.one * 0.21f, matMonyet, false);
                Hias(m, PrimitiveType.Sphere, "Moncong", new Vector3(0f, 0.31f, 0.19f), new Vector3(0.12f, 0.10f, 0.08f), matWajahMonyet, false);
                var ekor = Hias(m, PrimitiveType.Cylinder, "Ekor", new Vector3(0f, 0.06f, -0.26f), new Vector3(0.04f, 0.32f, 0.04f), matMonyet, false);
                ekor.transform.localRotation = Quaternion.Euler(-52f, 0f, 0f);
                daftar.Add(new EkosistemMonyetDanKodok.SatwaKecil
                {
                    tr = m.transform, pusat = new Vector3(x, y, z), tinggiLompat = 0.85f + (i % 3) * 0.3f,
                    laju = 2.4f + (i % 4) * 0.4f, fase = i * 0.9f, kodok = false
                });
            }

            Vector2[] areaKodok = { new Vector2(260f, -242f), new Vector2(-640f, -555f), new Vector2(315f, -180f) };
            for (int i = 0; i < 12; i++)
            {
                var c = areaKodok[i % areaKodok.Length];
                float x = c.x + (i % 4) * 4.5f, z = c.y + (i / 4) * 4.2f;
                float y = WorldTerrainGenerator.SampleHeight(x, z) + 0.08f;
                var k = Anak(go, "KodokAir_" + i);
                k.transform.position = new Vector3(x, y, z);
                Hias(k, PrimitiveType.Sphere, "Tubuh", Vector3.zero, new Vector3(0.16f, 0.11f, 0.20f), matKodok, false);
                Hias(k, PrimitiveType.Sphere, "MataKiri", new Vector3(-0.045f, 0.06f, 0.08f), Vector3.one * 0.04f, matMataKodok, false);
                Hias(k, PrimitiveType.Sphere, "MataKanan", new Vector3(0.045f, 0.06f, 0.08f), Vector3.one * 0.04f, matMataKodok, false);
                daftar.Add(new EkosistemMonyetDanKodok.SatwaKecil
                {
                    tr = k.transform, pusat = new Vector3(x, y, z), tinggiLompat = 0.42f,
                    laju = 3.1f + (i % 3) * 0.5f, fase = i * 1.3f, kodok = true
                });
            }

            var comp = go.AddComponent<EkosistemMonyetDanKodok>();
            comp.Pasang(daftar.ToArray());
            EditorUtility.SetDirty(comp);
        }

        static void BangunAsapDanKainBerkibar(GameObject induk)
        {
            var go = Anak(induk, "AsapDanKainDunia");
            Vector2[] asapXZ =
            {
                new Vector2(8f, -448f), new Vector2(665f, -500f), new Vector2(540f, 204f), new Vector2(56f, -376f),
                new Vector2(-185f, -160f), new Vector2(240f, 140f), new Vector2(-110f, 320f),
                new Vector2(725f, -140f), new Vector2(590f, 560f), new Vector2(-265f, 115f)
            };
            var titikAsap = new Vector3[asapXZ.Length];
            for (int i = 0; i < asapXZ.Length; i++)
                titikAsap[i] = new Vector3(asapXZ[i].x, WorldTerrainGenerator.SampleHeight(asapXZ[i].x, asapXZ[i].y) + (i == 1 ? 9.5f : 2.6f), asapXZ[i].y);

            var kayu = AmbilMat("M_Kayu");
            var kainDasar = AmbilMat("M_Kain");
            var kainMerah = SimpanMat(SafeMaterialFactory.CreatePBR("M_UmbulSekte", new Color(0.56f, 0.14f, 0.11f), 0f, 0.15f, kainDasar != null ? kainDasar.mainTexture as Texture2D : null));
            var kainPutih = SimpanMat(SafeMaterialFactory.CreatePBR("M_UmbulDesa", new Color(0.78f, 0.72f, 0.58f), 0f, 0.15f, kainDasar != null ? kainDasar.mainTexture as Texture2D : null));
            var daftarKain = new Transform[18];
            for (int i = 0; i < 18; i++)
            {
                var c = asapXZ[i % asapXZ.Length];
                float x = c.x + Mathf.Cos(i * 1.7f) * 11.5f, z = c.y + Mathf.Sin(i * 1.7f) * 11.5f;
                float y = WorldTerrainGenerator.SampleHeight(x, z);
                var tiang = Anak(go, "TiangBendera_" + i);
                tiang.transform.position = new Vector3(x, y, z);
                // Umbul-umbul bambu vertikal khas Nusantara (melengkung lembut, bukan persegi kaku)
                var bambu = Hias(tiang, PrimitiveType.Cylinder, "Tiang", new Vector3(0f, 2.25f, 0f), new Vector3(0.055f, 2.25f, 0.055f), kayu, false);
                bambu.transform.localRotation = Quaternion.Euler(0f, 0f, -6f);
                var kain = Hias(tiang, PrimitiveType.Cube, "Kain", new Vector3(0.32f, 3.15f, 0f), new Vector3(0.34f, 1.95f, 0.025f), (i % 2 == 0) ? kainMerah : kainPutih, false);
                daftarKain[i] = kain.transform;
            }

            var comp = go.AddComponent<AsapDanKainTertiuAngin>();
            comp.Pasang(titikAsap, daftarKain);
            EditorUtility.SetDirty(comp);
        }
    }
}
