using System.Collections.Generic;
using System.IO;
using AlasKeramat.AI;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.EditorTools
{
    // Suasana desa & senja (dipanggil dari langkah 7z8): gagak di atap Desa Mati, kawanan kelelawar keluar gua
    // saat Maghrib, laron di lampu minyak, dan sarang laba-laba di rumah yang ditinggal.
    public static partial class NusantaraMasterBuilder
    {
        public const int GagakPerRumah = 2, SarangDesaMati = 4, SarangKampung = 1;
        const float PanggungDesa = 1.5f, PanggungKampung = 0.6f, TinggiDinding = 2.6f, CelahSarang = 0.22f;
        const int UkuranLembarKelelawar = 64, FrameKelelawar = 4, UkuranSarang = 256, JariSarang = 13, LingkarSarang = 9;
        public static int JumlahGagak, JumlahKawananSenja, JumlahSarang;

        static void LangkahSuasanaDesa(GameObject lm)
        {
            JumlahGagak = 0; JumlahKawananSenja = 0; JumlahSarang = 0;
            try { BuatGagak(lm); } catch (System.Exception e) { Debug.LogError("[AK][SuasanaDesa] Gagal membuat gagak: " + e.Message); }
            try { BuatKelelawarSenja(lm); } catch (System.Exception e) { Debug.LogError("[AK][SuasanaDesa] Gagal membuat kelelawar senja: " + e.Message); }
            try { BuatSarangLaba(lm); } catch (System.Exception e) { Debug.LogError("[AK][SuasanaDesa] Gagal membuat sarang laba-laba: " + e.Message); }
            Catat("Suasana desa: " + JumlahGagak + " gagak, " + JumlahKawananSenja + " kawanan kelelawar Maghrib, " + JumlahSarang + " sarang laba-laba");
        }

        // ---------- Gagak ----------
        static Mesh GabungPrimitif(string nama, PrimitiveType[] jenis, Vector3[] pos, Vector3[] skala, Vector3[] rot)
        {
            var gabung = new CombineInstance[jenis.Length];
            var sementara = new List<GameObject>();
            for (int i = 0; i < jenis.Length; i++)
            {
                var g = GameObject.CreatePrimitive(jenis[i]); sementara.Add(g);
                gabung[i].mesh = g.GetComponent<MeshFilter>().sharedMesh;
                gabung[i].transform = Matrix4x4.TRS(pos[i], Quaternion.Euler(rot[i]), skala[i]);
            }
            var m = new Mesh { name = nama };
            m.CombineMeshes(gabung, true, true);
            m.RecalculateBounds();
            for (int i = 0; i < sementara.Count; i++) Object.DestroyImmediate(sementara[i]);
            string p = FolderLandmark + "/" + nama + ".asset";
            AssetDatabase.CreateAsset(m, p);
            return AssetDatabase.LoadAssetAtPath<Mesh>(p);
        }

        static void BuatGagak(GameObject lm)
        {
            var C = PrimitiveType.Cube; var S = PrimitiveType.Sphere; var K = PrimitiveType.Capsule;
            var badan = GabungPrimitif("Mesh_GagakBadan",
                new[] { K, S, C, C, C, C },
                new[] { new Vector3(0f, 0.095f, 0f), new Vector3(0f, 0.15f, 0.12f), new Vector3(0f, 0.145f, 0.185f), new Vector3(0f, 0.085f, -0.17f), new Vector3(-0.025f, 0.03f, 0.01f), new Vector3(0.025f, 0.03f, 0.01f) },
                new[] { new Vector3(0.1f, 0.14f, 0.1f), Vector3.one * 0.085f, new Vector3(0.022f, 0.02f, 0.065f), new Vector3(0.07f, 0.01f, 0.13f), new Vector3(0.01f, 0.06f, 0.01f), new Vector3(0.01f, 0.06f, 0.01f) },
                new[] { new Vector3(90f, 0f, 0f), Vector3.zero, new Vector3(8f, 0f, 0f), new Vector3(-12f, 0f, 0f), Vector3.zero, Vector3.zero });
            var kiri = GabungPrimitif("Mesh_GagakSayapKiri", new[] { C }, new[] { new Vector3(-0.12f, 0f, -0.01f) }, new[] { new Vector3(0.24f, 0.012f, 0.11f) }, new[] { Vector3.zero });
            var kanan = GabungPrimitif("Mesh_GagakSayapKanan", new[] { C }, new[] { new Vector3(0.12f, 0f, -0.01f) }, new[] { new Vector3(0.24f, 0.012f, 0.11f) }, new[] { Vector3.zero });
            var mat = SimpanMat(SafeMaterialFactory.CreatePBR("M_Gagak", new Color(0.05f, 0.05f, 0.06f, 1f), 0.1f, 0.55f));

            var rumah = new List<Transform>();
            for (int i = 0; i < lm.transform.childCount; i++) { var t = lm.transform.GetChild(i); if (t.name.StartsWith("RumahDesa_")) rumah.Add(t); }
            if (rumah.Count == 0) return;
            var induk = Anak(lm, "GagakDesa");
            var gd = induk.AddComponent<GagakDesa>();
            int n = rumah.Count * GagakPerRumah;
            var badanT = new Transform[n]; var kiriT = new Transform[n]; var kananT = new Transform[n]; var tengger = new Vector3[n]; var arah = new float[n];
            var rnd = new System.Random(911);
            for (int i = 0; i < n; i++)
            {
                var r = rumah[i / GagakPerRumah];
                var mr = r.GetComponent<Renderer>();
                float puncak = mr != null ? mr.bounds.max.y : r.position.y + PanggungDesa + TinggiDinding + 2f;
                float geser = ((i % GagakPerRumah) == 0 ? -1f : 1f) * (0.6f + (float)rnd.NextDouble() * 1.6f);
                tengger[i] = new Vector3(r.position.x, puncak - 0.03f, r.position.z) + r.right * geser;
                arah[i] = r.eulerAngles.y + ((float)rnd.NextDouble() < 0.5f ? 90f : -90f) + (float)(rnd.NextDouble() - 0.5) * 50f;
                var g = Anak(induk, "Gagak_" + i);
                g.transform.SetPositionAndRotation(tengger[i], Quaternion.Euler(0f, arah[i], 0f));
                g.AddComponent<MeshFilter>().sharedMesh = badan;
                var gr = g.AddComponent<MeshRenderer>(); gr.sharedMaterial = mat; gr.shadowCastingMode = ShadowCastingMode.Off;
                kiriT[i] = BuatSayapGagak(g, "SayapKiri", kiri, mat, -0.04f);
                kananT[i] = BuatSayapGagak(g, "SayapKanan", kanan, mat, 0.04f);
                badanT[i] = g.transform;
            }
            gd.Badan = badanT; gd.SayapKiri = kiriT; gd.SayapKanan = kananT; gd.Tengger = tengger; gd.ArahTengger = arah;
            EditorUtility.SetDirty(gd);
            JumlahGagak = n;
        }

        static Transform BuatSayapGagak(GameObject badan, string nama, Mesh mesh, Material mat, float x)
        {
            var s = Anak(badan, nama);
            s.transform.localPosition = new Vector3(x, 0.12f, 0.01f);
            s.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = s.AddComponent<MeshRenderer>(); r.sharedMaterial = mat; r.shadowCastingMode = ShadowCastingMode.Off;
            return s.transform;
        }

        // ---------- Kelelawar senja ----------
        static Texture2D LembarKelelawar()
        {
            string p = FolderTex + "/partikel_kelelawar_senja.png";
            int w = UkuranLembarKelelawar * FrameKelelawar, h = UkuranLembarKelelawar;
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var px = new Color[w * h];
            float[] ujung = { -14f, -4f, 11f, -4f }; // posisi ujung sayap per frame (kepak naik-turun)
            for (int f = 0; f < FrameKelelawar; f++)
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < UkuranLembarKelelawar; x++)
                    {
                        float cx = x - 32f, cy = y - 32f, ax = Mathf.Abs(cx);
                        bool isi = (cx * cx) / 16f + (cy * cy) / 81f <= 1f;                // badan
                        isi |= (cx * cx + (cy - 10f) * (cy - 10f)) <= 16f;                  // kepala
                        isi |= ax > 1f && ax < 4f && cy > 12f && cy < 17f;                   // telinga
                        if (ax >= 3f && ax <= 29f)
                        {
                            float u = (ax - 3f) / 26f;
                            float atas = 4f + ujung[f] * u - 4f * (1f - u);
                            float bawah = atas - (12f * Mathf.Pow(1f - u, 0.8f) + 2f) + 3f * Mathf.Abs(Mathf.Sin(u * Mathf.PI * 3f));
                            isi |= cy <= atas && cy >= bawah;
                        }
                        px[y * w + f * UkuranLembarKelelawar + x] = isi ? new Color(0.06f, 0.05f, 0.06f, 1f) : new Color(0f, 0f, 0f, 0f);
                    }
            t.SetPixels(px); t.Apply();
            File.WriteAllBytes(p, t.EncodeToPNG()); Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(p);
            var imp = AssetImporter.GetAtPath(p) as TextureImporter;
            if (imp != null) { imp.alphaIsTransparency = true; imp.wrapMode = TextureWrapMode.Clamp; imp.mipmapEnabled = true; imp.SaveAndReimport(); }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(p);
        }

        static void BuatKelelawarSenja(GameObject lm)
        {
            var mat = MaterialPartikel("M_KelelawarSenja", LembarKelelawar());
            if (mat == null) { Debug.LogWarning("[AK][SuasanaDesa] Shader partikel URP tidak ada, kelelawar senja dilewati"); return; }
            for (int i = 0; i < lm.transform.childCount; i++)
            {
                var gua = lm.transform.GetChild(i);
                if (!gua.name.StartsWith("CaveEntrance_")) continue;
                var go = Anak(lm, "KelelawarSenja_" + gua.name);
                go.transform.position = gua.position + gua.forward * 2f + Vector3.up * 3f;
                go.transform.rotation = Quaternion.LookRotation((gua.forward + Vector3.up * 1.3f).normalized, Vector3.up);
                var ps = go.AddComponent<ParticleSystem>();
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var main = ps.main;
                main.duration = 5f; main.loop = true; main.playOnAwake = true;
                main.startLifetime = KawananKelelawarSenja.LamaHidup;
                main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 8.5f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.38f, 0.6f);
                main.startColor = Color.white;
                main.maxParticles = 220;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                var em = ps.emission; em.rateOverTime = 0f;
                var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 22f; sh.radius = 1.5f;
                var vel = ps.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.World;
                Vector3 hanyut = gua.forward * 1.6f + Vector3.up * 0.5f;
                vel.x = new ParticleSystem.MinMaxCurve(hanyut.x); vel.y = new ParticleSystem.MinMaxCurve(hanyut.y); vel.z = new ParticleSystem.MinMaxCurve(hanyut.z);
                var nz = ps.noise; nz.enabled = true; nz.strength = 2.2f; nz.frequency = 0.35f; nz.scrollSpeed = 0.4f; nz.damping = true; nz.quality = ParticleSystemNoiseQuality.Medium;
                var ts = ps.textureSheetAnimation; ts.enabled = true; ts.numTilesX = FrameKelelawar; ts.numTilesY = 1;
                ts.cycleCount = Mathf.RoundToInt(KawananKelelawarSenja.LamaHidup * 5f);
                var col = ps.colorOverLifetime; col.enabled = true;
                var gr = new Gradient();
                gr.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                    new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.04f), new GradientAlphaKey(1f, 0.88f), new GradientAlphaKey(0f, 1f) });
                col.color = gr;
                var rd = go.GetComponent<ParticleSystemRenderer>();
                rd.renderMode = ParticleSystemRenderMode.Billboard; rd.sharedMaterial = mat;
                rd.shadowCastingMode = ShadowCastingMode.Off; rd.receiveShadows = false; rd.minParticleSize = 0.002f;
                go.AddComponent<KawananKelelawarSenja>();
                JumlahKawananSenja++;
            }
        }

        // Laron kecil berputar di sekitar api lampu minyak (dipanggil dari TaruhLampu).
        static ParticleSystem BuatLaron(GameObject lampu, float tinggiApi)
        {
            var tex = TeksturPartikel("partikel_debu", false);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_Laron.mat");
            if (mat == null) mat = MaterialPartikel("M_Laron", tex);
            if (mat == null) return null;
            var go = Anak(lampu, "Laron");
            go.transform.localPosition = new Vector3(0f, tinggiApi, 0f);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = true; main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.2f, 3.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.035f);
            main.startColor = new Color(1f, 0.93f, 0.8f, 0.9f);
            main.maxParticles = 16;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            var em = ps.emission; em.rateOverTime = 6f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.25f;
            var nz = ps.noise; nz.enabled = true; nz.strength = 0.9f; nz.frequency = 1.8f; nz.quality = ParticleSystemNoiseQuality.Low;
            var rd = go.GetComponent<ParticleSystemRenderer>();
            rd.renderMode = ParticleSystemRenderMode.Billboard; rd.sharedMaterial = mat;
            rd.shadowCastingMode = ShadowCastingMode.Off; rd.receiveShadows = false;
            return ps;
        }

        // ---------- Sarang laba-laba ----------
        static Texture2D TeksturSarang()
        {
            string p = FolderTex + "/sarang_laba.png";
            int s = UkuranSarang; float c = s * 0.5f, rMaks = s * 0.48f;
            var t = new Texture2D(s, s, TextureFormat.RGBA32, true);
            var px = new Color[s * s];
            var rnd = new System.Random(77);
            var sudut = new float[JariSarang];
            for (int j = 0; j < JariSarang; j++) sudut[j] = (j + (float)rnd.NextDouble() * 0.35f) * Mathf.PI * 2f / JariSarang;
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float dx = x - c, dy = y - c, r = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = 0f;
                    if (r < rMaks)
                    {
                        float th = Mathf.Atan2(dy, dx); if (th < 0f) th += Mathf.PI * 2f;
                        for (int j = 0; j < JariSarang; j++)
                        {
                            float d = Mathf.Abs(Mathf.DeltaAngle(th * Mathf.Rad2Deg, sudut[j] * Mathf.Rad2Deg)) * Mathf.Deg2Rad * r;
                            a = Mathf.Max(a, 1f - Mathf.Clamp01(d / 1.3f));
                        }
                        // Lingkar spiral yang sedikit melengkung antar jari.
                        float ring = r / rMaks * LingkarSarang + 0.15f * Mathf.Sin(th * JariSarang);
                        float dr = Mathf.Abs(ring - Mathf.Round(ring)) * rMaks / LingkarSarang;
                        if (r > s * 0.04f) a = Mathf.Max(a, 0.85f * (1f - Mathf.Clamp01(dr / 1.1f)));
                        a *= Mathf.Lerp(1f, 0.55f, r / rMaks);
                    }
                    px[y * s + x] = new Color(0.86f, 0.86f, 0.83f, a * 0.85f);
                }
            t.SetPixels(px); t.Apply();
            File.WriteAllBytes(p, t.EncodeToPNG()); Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(p);
            var imp = AssetImporter.GetAtPath(p) as TextureImporter;
            if (imp != null) { imp.alphaIsTransparency = true; imp.wrapMode = TextureWrapMode.Clamp; imp.mipmapEnabled = true; imp.SaveAndReimport(); }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(p);
        }

        static void BuatSarangLaba(GameObject lm)
        {
            var tex = TeksturSarang();
            var m = SafeMaterialFactory.CreatePBR("M_SarangLaba", Color.white, 0f, 0.35f, tex, null, null, SurfaceMode.Transparent);
            SafeMaterialFactory.SetF(m, "_Cull", 0f);
            var mat = SimpanMat(m);
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var meshQuad = quad.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(quad);
            var rnd = new System.Random(5150);
            var induk = Anak(lm, "SarangLabaLaba");
            for (int i = 0; i < lm.transform.childCount; i++)
            {
                var r = lm.transform.GetChild(i);
                bool desa = r.name.StartsWith("RumahDesa_"), kampung = r.name.StartsWith("RumahKampung_");
                if (!desa && !kampung) continue;
                bool besar = r.name == "RumahDesa_1";
                float lebar = desa ? (besar ? 8f : 7f) : 6.5f, dalam = desa ? (besar ? 6f : 5.5f) : 5f;
                float atas = (desa ? PanggungDesa : PanggungKampung) + TinggiDinding - 0.18f;
                int jumlah = desa ? SarangDesaMati : SarangKampung;
                int mulai = rnd.Next(4);
                for (int k = 0; k < jumlah; k++)
                {
                    int sudut = (mulai + k) % 4;
                    float sx = sudut % 2 == 0 ? -1f : 1f, sz = sudut < 2 ? -1f : 1f;
                    // Sudut dalam rumah (cekung): bidang sarang memotong diagonal sudut.
                    Vector3 lokal = new Vector3(sx * (lebar * 0.5f - CelahSarang), atas, sz * (dalam * 0.5f - CelahSarang));
                    Vector3 diag = new Vector3(sx, 0f, sz).normalized;
                    var g = new GameObject("SarangLaba_" + JumlahSarang);
                    g.transform.SetParent(induk.transform, false);
                    g.transform.position = r.TransformPoint(lokal);
                    g.transform.rotation = r.rotation * Quaternion.LookRotation(diag, Vector3.up) * Quaternion.Euler(-25f, 0f, (float)rnd.NextDouble() * 360f);
                    float uk = 0.7f + (float)rnd.NextDouble() * 0.45f;
                    g.transform.localScale = new Vector3(uk, uk, 1f);
                    g.AddComponent<MeshFilter>().sharedMesh = meshQuad;
                    var mr = g.AddComponent<MeshRenderer>(); mr.sharedMaterial = mat; mr.shadowCastingMode = ShadowCastingMode.Off; mr.receiveShadows = false;
                    JumlahSarang++;
                }
            }
        }
    }
}
