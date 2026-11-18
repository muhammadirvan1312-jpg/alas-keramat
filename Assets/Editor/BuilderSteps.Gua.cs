using System.Collections.Generic;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEditor;
using UnityEngine;
namespace AlasKeramat.EditorTools
{
    // Langkah 7t (T10a): tiga kompleks bawah tanah Y = -80 (03 §10, pusat v5 20 §2.3). Ruang = mesh dinding/kubah ProcGua berbukaan,
    // lorong = balok, kolam & terowongan selam, gerbang (kartu, reruntuhan, angin), isi peti & lore, lampu, gema gua.
    public static partial class NusantaraMasterBuilder
    {
        public const string FolderGua = FolderGen + "/Gua";
        public static readonly string[] NamaKompleks = { "", "UG1_GuaSungai", "UG2_Bunker", "UG3_KuilAkar" };

        class RuangGua
        {
            public string nama; public Vector3 pos, ukuran; public bool kubah;
            public bool berlubang; public Vector2 lubangPusat, lubangUkuran;
            public readonly List<ProcGua.Bukaan> bukaan = new List<ProcGua.Bukaan>();
            public GameObject go;
        }
        class LorongGua { public readonly List<Vector3> titik = new List<Vector3>(); public float lebar, tinggi; public string nama; public RuangGua a, b; }

        static readonly List<RuangGua> _ruang = new List<RuangGua>();
        static readonly List<LorongGua> _lorong = new List<LorongGua>();
        static GameObject _kompleks;
        static Material _matDinding, _matLantai, _matLumut, _matUrat, _matAir;
        static int _seedGua;

        static void LangkahGua()
        {
            EnsureFolder(FolderGua);
            var induk = Anak(_root, "BawahTanah");
            var akar = new Transform[4];
            akar[1] = BangunUG1(induk).transform;
            akar[2] = BangunUG2(induk).transform;
            akar[3] = BangunUG3(induk).transform;
            BangunPermukaanGua();
            var mgrGo = Anak(_sistem, "WorldCaveAndSaveManager");
            var mgr = mgrGo.AddComponent<WorldCaveAndSaveManager>();
            mgr.Pasang(akar, BuatLayarHitam());
            EditorUtility.SetDirty(mgr);
            var andi = Object.FindFirstObjectByType<AlasKeramat.Player.AndiFPSController>();
            if (andi != null && andi.GetComponent<AlasKeramat.Player.SenterAndi>() == null) andi.gameObject.AddComponent<AlasKeramat.Player.SenterAndi>();
            Catat("Bawah tanah: 3 kompleks (Gua Sungai, Bunker, Kuil Akar), " + Object.FindObjectsByType<PintuGua>(FindObjectsSortMode.None).Length + " pintu gua, kolam & terowongan selam");
        }

        // ---------- Kerangka kompleks ----------
        static GameObject MulaiKompleks(GameObject induk, UndergroundId u, Material dinding, Material lantai)
        {
            _ruang.Clear(); _lorong.Clear();
            _kompleks = Anak(induk, NamaKompleks[(int)u]);
            _kompleks.transform.position = WorldCaveAndSaveManager.Pusat[(int)u];
            _matDinding = dinding; _matLantai = lantai; _seedGua = (int)u * 100;
            Balok(_kompleks, "BatuDasar", new Vector3(0f, -9f, 0f), Quaternion.identity, new Vector3(220f, 1f, 220f), lantai, false); // penangkap jatuh
            var gema = _kompleks.AddComponent<AudioReverbZone>();
            gema.reverbPreset = AudioReverbPreset.Cave; gema.minDistance = 30f; gema.maxDistance = 140f;
            return _kompleks;
        }

        static RuangGua R(string nama, Vector3 pos, Vector3 ukuran, bool kubah)
        {
            var r = new RuangGua { nama = nama, pos = pos, ukuran = ukuran, kubah = kubah };
            _ruang.Add(r);
            return r;
        }

        static void Sambung(RuangGua a, RuangGua b, float lebar, float tinggi, params Vector3[] tengah)
        {
            Vector3 ka = tengah.Length > 0 ? tengah[0] : b.pos, kb = tengah.Length > 0 ? tengah[tengah.Length - 1] : a.pos;
            float sa = ProcGua.SudutKe(a.pos, ka), sb = ProcGua.SudutKe(b.pos, kb);
            a.bukaan.Add(new ProcGua.Bukaan { sudut = sa, lebar = lebar + 0.8f, tinggi = tinggi + 0.3f });
            b.bukaan.Add(new ProcGua.Bukaan { sudut = sb, lebar = lebar + 0.8f, tinggi = tinggi + 0.3f });
            var l = new LorongGua { lebar = lebar, tinggi = tinggi, nama = "Lorong_" + a.nama + "_" + b.nama, a = a, b = b };
            l.titik.Add(a.pos + ProcGua.Tepi(a.ukuran, a.kubah, sa, 0.85f));
            l.titik.AddRange(tengah);
            l.titik.Add(b.pos + ProcGua.Tepi(b.ukuran, b.kubah, sb, 0.85f));
            _lorong.Add(l);
        }

        static void LorongBebas(string nama, float lebar, float tinggi, params Vector3[] titik)
        {
            var l = new LorongGua { lebar = lebar, tinggi = tinggi, nama = nama };
            l.titik.AddRange(titik);
            _lorong.Add(l);
        }

        static void BangunKerangka()
        {
            foreach (var r in _ruang)
            {
                r.go = Anak(_kompleks, r.nama);
                r.go.transform.localPosition = r.pos;
                r.go.layer = GameLayers.Ground;
                var mesh = SimpanMeshGua(ProcGua.Ruang("Gua_" + _kompleks.name + "_" + r.nama, r.ukuran, r.kubah, r.kubah ? 0.12f : 0f, _seedGua++, r.bukaan.ToArray()));
                r.go.AddComponent<MeshFilter>().sharedMesh = mesh;
                r.go.AddComponent<MeshRenderer>().sharedMaterial = _matDinding;
                r.go.AddComponent<MeshCollider>().sharedMesh = mesh;
                if (r.berlubang) LantaiBerlubang(r);
                else Balok(r.go, "Lantai", new Vector3(0f, -0.25f, 0f), Quaternion.identity, new Vector3(r.ukuran.x + 1f, 0.5f, r.ukuran.z + 1f), _matLantai, false);
            }
            foreach (var l in _lorong)
            {
                var go = Anak(_kompleks, l.nama);
                for (int i = 0; i + 1 < l.titik.Count; i++) Segmen(go, l.titik[i], l.titik[i + 1], l.lebar, l.tinggi);
            }
            PanggangGraf();
            AudioKompleks();
        }

        // WaypointGraph: pusat ruang + titik lorong berjalan kaki (terowongan selam dilewati: musuh tidak menyelam).
        static void PanggangGraf()
        {
            var titik = new List<Vector3>(); var sisi = new List<Vector2Int>();
            Vector3 o = _kompleks.transform.position + Vector3.up * 0.3f;
            for (int i = 0; i < _ruang.Count; i++) titik.Add(o + _ruang[i].pos);
            foreach (var l in _lorong)
            {
                if (l.a == null || l.b == null) continue;
                int awal = titik.Count;
                for (int i = 0; i < l.titik.Count; i++) { titik.Add(o + l.titik[i]); if (i > 0) sisi.Add(new Vector2Int(awal + i - 1, awal + i)); }
                sisi.Add(new Vector2Int(_ruang.IndexOf(l.a), awal));
                sisi.Add(new Vector2Int(awal + l.titik.Count - 1, _ruang.IndexOf(l.b)));
            }
            var g = _kompleks.AddComponent<AlasKeramat.AI.WaypointGraph>();
            g.Pasang(WorldCaveAndSaveManager.KompleksDi(_kompleks.transform.position), titik.ToArray(), sisi.ToArray());
            EditorUtility.SetDirty(g);
        }

        // Dengung + tetes air per kompleks (resep dari versi T10a ChatGPT), WAV 12 s berulang.
        static void AudioKompleks()
        {
            const int sr = 22050, detik = 12;
            int id = (int)WorldCaveAndSaveManager.KompleksDi(_kompleks.transform.position);
            var s = new float[sr * detik]; var acak = new System.Random(100 + id);
            for (int i = 0; i < s.Length; i++)
            {
                float t = (float)i / sr, fade = Mathf.Min(1f, Mathf.Min(t, detik - t) * 2f);
                float hum = id == 1 ? Mathf.Sin(t * 2f * Mathf.PI * 48f) + Mathf.Sin(t * 2f * Mathf.PI * 50.5f)
                    : id == 2 ? Mathf.Sin(t * 2f * Mathf.PI * 50f) + 0.3f * Mathf.Sin(t * 2f * Mathf.PI * 100f)
                    : Mathf.Sin(t * 2f * Mathf.PI * 40f) * (0.5f + 0.5f * Mathf.Sin(t * 2f * Mathf.PI / 1.4f));
                float tetes = id == 1 ? Mathf.Sin(t * 2f * Mathf.PI * 1700f) * Mathf.Exp(-Mathf.Repeat(t, 1.3f) * 32f) : 0f;
                s[i] = fade * (hum * 0.035f + tetes * 0.06f + ((float)acak.NextDouble() - 0.5f) * 0.003f);
            }
            string path = FolderGua + "/ambien_ug" + id + ".wav";
            System.IO.File.WriteAllBytes(path, WavWriter.Encode(s, sr));
            AssetDatabase.ImportAsset(path);
            var a = _kompleks.AddComponent<AudioSource>();
            a.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path); a.loop = true; a.playOnAwake = false; a.spatialBlend = 0f; a.volume = AudioGua.Volume;
            _kompleks.AddComponent<AudioGua>().Pasang((UndergroundId)id, a);
        }

        static void Segmen(GameObject induk, Vector3 p, Vector3 q, float lebar, float tinggi)
        {
            Vector3 d = q - p;
            float panjang = d.magnitude + 0.8f;
            if (panjang < 0.9f) return;
            var rot = Quaternion.LookRotation(d.normalized, Vector3.up);
            Vector3 c = (p + q) * 0.5f;
            bool guaAlam = _kompleks != null && WorldCaveAndSaveManager.KompleksDi(_kompleks.transform.position) != UndergroundId.UG2_Bunker;
            if (guaAlam)
            {
                var go = Anak(induk, "LorongBatuan_" + _seedGua++);
                go.transform.localPosition = c; go.transform.localRotation = rot;
                var mesh = SimpanMeshGua(ProcGua.LorongGua(go.name, panjang, lebar, tinggi, _seedGua * 17));
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                go.AddComponent<MeshRenderer>().sharedMaterial = _matDinding;
                go.AddComponent<MeshCollider>().sharedMesh = mesh;
                go.layer = GameLayers.Ground;
                return;
            }
            Balok(induk, "Lantai", c + rot * new Vector3(0f, -0.25f, 0f), rot, new Vector3(lebar + 0.8f, 0.5f, panjang), _matLantai, false);
            Balok(induk, "Atap", c + rot * new Vector3(0f, tinggi + 0.25f, 0f), rot, new Vector3(lebar + 0.8f, 0.5f, panjang), _matDinding, false);
            Balok(induk, "DindingKiri", c + rot * new Vector3(-lebar * 0.5f - 0.2f, tinggi * 0.5f, 0f), rot, new Vector3(0.4f, tinggi + 0.2f, panjang), _matDinding, false);
            Balok(induk, "DindingKanan", c + rot * new Vector3(lebar * 0.5f + 0.2f, tinggi * 0.5f, 0f), rot, new Vector3(0.4f, tinggi + 0.2f, panjang), _matDinding, false);
        }

        static void LantaiBerlubang(RuangGua r)
        {
            float x0 = -r.ukuran.x * 0.5f - 0.5f, x1 = r.ukuran.x * 0.5f + 0.5f, z0 = -r.ukuran.z * 0.5f - 0.5f, z1 = r.ukuran.z * 0.5f + 0.5f;
            float hx0 = r.lubangPusat.x - r.lubangUkuran.x * 0.5f, hx1 = r.lubangPusat.x + r.lubangUkuran.x * 0.5f;
            float hz0 = r.lubangPusat.y - r.lubangUkuran.y * 0.5f, hz1 = r.lubangPusat.y + r.lubangUkuran.y * 0.5f;
            Papan(r.go, "LantaiS", x0, x1, z0, hz0);
            Papan(r.go, "LantaiU", x0, x1, hz1, z1);
            Papan(r.go, "LantaiB", x0, hx0, hz0, hz1);
            Papan(r.go, "LantaiT", hx1, x1, hz0, hz1);
        }
        static void Papan(GameObject induk, string nama, float x0, float x1, float z0, float z1)
        {
            if (x1 - x0 < 0.05f || z1 - z0 < 0.05f) return;
            Balok(induk, nama, new Vector3((x0 + x1) * 0.5f, -0.25f, (z0 + z1) * 0.5f), Quaternion.identity, new Vector3(x1 - x0, 0.5f, z1 - z0), _matLantai, false);
        }

        // Kolam (lokal kompleks): dasar -5, dinding sampai 0, celah di sisi selatan untuk terowongan selam, tanjakan keluar.
        static void Kolam(string nama, float x0, float x1, float z0, float z1, float celahX, Vector3 tanjakAtas, Vector3 tanjakBawah)
        {
            const float dasar = -5f, muka = -1f, lebarCelah = 3.6f;
            var go = Anak(_kompleks, nama);
            Balok(go, "Dasar", new Vector3((x0 + x1) * 0.5f, dasar - 0.25f, (z0 + z1) * 0.5f), Quaternion.identity, new Vector3(x1 - x0 + 1f, 0.5f, z1 - z0 + 1f), _matLantai, false);
            float tinggi = -dasar, yc = dasar * 0.5f;
            Balok(go, "DindingU", new Vector3((x0 + x1) * 0.5f, yc, z1 + 0.2f), Quaternion.identity, new Vector3(x1 - x0 + 0.8f, tinggi, 0.4f), _matDinding, false);
            Balok(go, "DindingB", new Vector3(x0 - 0.2f, yc, (z0 + z1) * 0.5f), Quaternion.identity, new Vector3(0.4f, tinggi, z1 - z0 + 0.8f), _matDinding, false);
            Balok(go, "DindingT", new Vector3(x1 + 0.2f, yc, (z0 + z1) * 0.5f), Quaternion.identity, new Vector3(0.4f, tinggi, z1 - z0 + 0.8f), _matDinding, false);
            float ca = Mathf.Max(x0, celahX - lebarCelah * 0.5f), cb = Mathf.Min(x1, celahX + lebarCelah * 0.5f);
            if (ca - x0 > 0.05f) Balok(go, "DindingS1", new Vector3((x0 + ca) * 0.5f, yc, z0 - 0.2f), Quaternion.identity, new Vector3(ca - x0 + 0.4f, tinggi, 0.4f), _matDinding, false);
            if (x1 - cb > 0.05f) Balok(go, "DindingS2", new Vector3((cb + x1) * 0.5f, yc, z0 - 0.2f), Quaternion.identity, new Vector3(x1 - cb + 0.4f, tinggi, 0.4f), _matDinding, false);
            Balok(go, "AtasCelah", new Vector3(celahX, -1f, z0 - 0.2f), Quaternion.identity, new Vector3(lebarCelah + 0.4f, 2f, 0.4f), _matDinding, false);
            Tanjakan(go, tanjakAtas, tanjakBawah, Mathf.Min(x1 - x0, z1 - z0) - 0.2f);
            var air = Balok(go, "MukaAir", new Vector3((x0 + x1) * 0.5f, muka, (z0 + z1) * 0.5f), Quaternion.identity, new Vector3(x1 - x0, 0.02f, z1 - z0), _matAir, false);
            Object.DestroyImmediate(air.GetComponent<Collider>());
            Air(go, new Vector3(x0, dasar, z0), new Vector3(x1, muka, z1));
        }

        static void Tanjakan(GameObject induk, Vector3 atas, Vector3 bawah, float lebar)
        {
            Vector3 d = bawah - atas;
            var rot = Quaternion.LookRotation(d.normalized, Vector3.up);
            Balok(induk, "Tanjakan", (atas + bawah) * 0.5f + rot * new Vector3(0f, -0.2f, 0f), rot, new Vector3(lebar, 0.4f, d.magnitude + 0.4f), _matLantai, false);
        }

        // Kotak air dalam koordinat lokal kompleks (min, maks); maks.y = muka air.
        static void Air(GameObject induk, Vector3 min, Vector3 maks)
        {
            var go = Anak(induk, "Air");
            go.transform.position = _kompleks.transform.position + (min + maks) * 0.5f;
            go.AddComponent<AirGua>().Pasang(maks - min);
        }

        // ---------- Primitif ----------
        static GameObject Balok(GameObject induk, string nama, Vector3 posLokal, Quaternion rot, Vector3 ukuran, Material mat, bool interaksi)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = nama;
            go.transform.SetParent(induk.transform, false);
            go.transform.localPosition = posLokal; go.transform.localRotation = rot; go.transform.localScale = ukuran;
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            go.layer = interaksi ? GameLayers.Interactable : GameLayers.Ground;
            if (interaksi) go.tag = GameTags.Interactable;
            return go;
        }

        static GameObject Hias(GameObject induk, PrimitiveType t, string nama, Vector3 posLokal, Vector3 skala, Material mat, bool kolider)
        {
            var go = GameObject.CreatePrimitive(t);
            go.name = nama;
            go.transform.SetParent(induk.transform, false);
            go.transform.localPosition = posLokal; go.transform.localScale = skala;
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            if (!kolider) Object.DestroyImmediate(go.GetComponent<Collider>()); else go.layer = GameLayers.Ground;
            return go;
        }

        static Light Lampu(GameObject induk, string nama, Vector3 posLokal, Color warna, float jangkau, float kuat)
        {
            var go = Anak(induk, nama);
            go.transform.localPosition = posLokal;
            var l = go.AddComponent<Light>();
            bool gua = _kompleks != null;
            l.type = LightType.Point; l.color = warna; l.range = jangkau * (gua ? 1.4f : 1f); l.intensity = kuat * 2f; l.shadows = LightShadows.None;
            return l;
        }

        // Wadah (peti/dokumen/benda) di posisi lokal kompleks.
        static WadahItem Wadah(GameObject induk, string nama, string tampil, Vector3 posLokal, Vector3 ukuran, Material mat, ItemID[] isi, int[] n, int lore, bool hilang)
        {
            var go = Balok(induk, nama, posLokal + Vector3.up * ukuran.y * 0.5f, Quaternion.identity, ukuran, mat, true);
            var w = go.AddComponent<WadahItem>();
            w.Pasang(tampil, isi, n, lore, hilang, null);
            w.KunciSimpan = "wadah_" + (_kompleks != null ? _kompleks.name : "dunia") + "_" + nama;
            EditorUtility.SetDirty(w);
            return w;
        }

        static PintuGua Pintu(GameObject induk, string nama, Vector3 posLokal, string prompt, Vector3 tujuanDunia, float yaw, int syarat, bool perluBoss, string terkunci)
        {
            var go = Anak(induk, nama);
            go.transform.localPosition = posLokal;
            go.layer = GameLayers.Interactable; go.tag = GameTags.CaveEntrance;
            var c = go.AddComponent<SphereCollider>(); c.radius = 1.6f; c.center = Vector3.up * 1f;
            var p = go.AddComponent<PintuGua>();
            p.Pasang(prompt, tujuanDunia, yaw, syarat, perluBoss, terkunci);
            EditorUtility.SetDirty(p);
            return p;
        }

        static Vector3 Dunia(UndergroundId u, Vector3 lokal) { return WorldCaveAndSaveManager.Pusat[(int)u] + lokal + Vector3.up * 0.2f; }
        static Vector3 Permukaan(float x, float z, float naik) { return new Vector3(x, Mathf.Max(0f, WorldTerrainGenerator.SampleHeight(x, z)) + naik, z); }

        static Mesh SimpanMeshGua(Mesh m)
        {
            string p = FolderGua + "/" + m.name + ".asset";
            AssetDatabase.DeleteAsset(p);
            AssetDatabase.CreateAsset(m, p);
            return AssetDatabase.LoadAssetAtPath<Mesh>(p);
        }

        static Material MatEmisi(string nama, string hex, Color emisi, bool transparan)
        {
            string p = FolderMat + "/" + nama + ".mat";
            AssetDatabase.DeleteAsset(p);
            var c = ProcTex.Hex(hex);
            var m = SafeMaterialFactory.CreatePBR(nama, c, 0f, transparan ? 0.9f : 0.2f, null, null, emisi, transparan ? SurfaceMode.Transparent : SurfaceMode.Opaque);
            AssetDatabase.CreateAsset(m, p);
            return AssetDatabase.LoadAssetAtPath<Material>(p);
        }

        // ---------- UG1 Gua Sungai Bioluminesensi ----------
        static GameObject BangunUG1(GameObject induk)
        {
            _matLumut = MatEmisi("M_LumutGua", "#1E5A50", new Color(0.24f, 0.95f, 0.78f) * 2.4f, false);
            _matAir = MatEmisi("M_AirGua", "#0A1418B0", Color.black, true);
            var k = MulaiKompleks(induk, UndergroundId.UG1_GuaSungai, AmbilMat("M_Batu"), AmbilMat("M_Batu"));
            var r0 = R("R0_MulutDalam", new Vector3(-60f, 0f, 0f), new Vector3(10f, 6f, 8f), true);
            var r1 = R("R1_RuangLumut", new Vector3(-15f, 0f, 0f), new Vector3(20f, 10f, 18f), true);
            var r2 = R("R2_KampEkspedisi", new Vector3(10f, 0f, 15f), new Vector3(12f, 5f, 10f), true);
            var r3 = R("R3_KolamHitam", new Vector3(15f, 0f, -10f), new Vector3(18f, 7f, 14f), true);
            r3.berlubang = true; r3.lubangPusat = new Vector2(0f, 0f); r3.lubangUkuran = new Vector2(12f, 7f);
            var r4 = R("R4_AltarPaku", new Vector3(60f, 0f, -14f), new Vector3(16f, 9f, 16f), true);
            r4.berlubang = true; r4.lubangPusat = new Vector2(-3f, -2f); r4.lubangUkuran = new Vector2(4f, 8f);
            Sambung(r0, r1, 4f, 4f);
            Sambung(r1, r2, 3f, 3.5f);
            Sambung(r1, r3, 3f, 3.5f);
            Sambung(r4, r2, 3f, 3.2f, new Vector3(40f, 0f, 2f), new Vector3(24f, 0f, 10f));
            // U1 terowongan bawah air (~92 m; tanpa Rebreather napas 25 s tidak cukup).
            Vector3[] u1 = { new Vector3(19f, -5f, -12f), new Vector3(22f, -5f, -34f), new Vector3(38f, -5f, -42f), new Vector3(42f, -5f, -28f), new Vector3(50f, -5f, -40f), new Vector3(57f, -5f, -19.5f) };
            LorongBebas("U1_TerowonganAir", 3f, 3f, u1);
            BangunKerangka();
            var u1Go = k.transform.Find("U1_TerowonganAir").gameObject;
            for (int i = 0; i + 1 < u1.Length; i++)
                Air(u1Go, Vector3.Min(u1[i], u1[i + 1]) - new Vector3(1.6f, 0f, 1.6f), Vector3.Max(u1[i], u1[i + 1]) + new Vector3(1.6f, 4f, 1.6f));
            Kolam("Kolam_R3", 9f, 21f, -13.5f, -6.5f, 19.2f, new Vector3(9.5f, 0f, -10f), new Vector3(17f, -5f, -10f));
            Kolam("Kolam_R4", 55f, 59f, -20f, -12f, 56.8f, new Vector3(57f, 0f, -12.5f), new Vector3(57f, -5f, -19.8f));

            // Lumut bercahaya, stalaktit, lampu cyan (6, maks 4 aktif).
            var hias = Anak(k, "Hiasan");
            var rng = new System.Random(41);
            var meshStalaktit = SimpanMeshGua(ProcGua.Stalaktit("StalaktitAlam", 77));
            for (int i = 0; i < 40; i++)
            {
                float a = (float)rng.NextDouble() * Mathf.PI * 2f, rr = (float)rng.NextDouble() * 7.5f;
                var st = Anak(hias, "Stalaktit_" + i);
                st.transform.localPosition = r1.pos + new Vector3(Mathf.Cos(a) * rr, 9.2f, Mathf.Sin(a) * rr * 0.8f);
                st.transform.localScale = new Vector3(0.45f + (float)rng.NextDouble() * 0.4f, 1.4f + (float)rng.NextDouble() * 1.9f, 0.45f + (float)rng.NextDouble() * 0.4f);
                st.AddComponent<MeshFilter>().sharedMesh = meshStalaktit;
                st.AddComponent<MeshRenderer>().sharedMaterial = _matDinding;
            }
            for (int i = 0; i < 45; i++)
            {
                var r = i < 24 ? r1 : i < 31 ? r0 : i < 38 ? r4 : r2;
                float a = (float)rng.NextDouble() * Mathf.PI * 2f;
                Vector3 tepi = ProcGua.Tepi(r.ukuran, true, a * Mathf.Rad2Deg, 0.9f);
                Hias(hias, PrimitiveType.Sphere, "Lumut_" + i, r.pos + tepi + Vector3.up * (0.05f + (float)rng.NextDouble() * 1.8f), new Vector3(1.35f, 0.22f + (float)rng.NextDouble() * 0.45f, 1.35f), _matLumut, false);
            }
            Color cyan = ProcTex.Hex("#3CF2C8");
            Lampu(hias, "Lampu_R0", r0.pos + Vector3.up * 3f, cyan, 12f, 2.0f);
            Lampu(hias, "Lampu_R1a", r1.pos + new Vector3(-5f, 4.5f, 3f), cyan, 18f, 2.6f);
            Lampu(hias, "Lampu_R1b", r1.pos + new Vector3(5f, 4.5f, -3f), cyan, 18f, 2.6f);
            Lampu(hias, "Lampu_R2", r2.pos + Vector3.up * 3f, cyan, 12f, 2.0f);
            Lampu(hias, "Lampu_R3", r3.pos + Vector3.up * 4f, cyan, 14f, 2.2f);
            Lampu(hias, "Lampu_R4", r4.pos + Vector3.up * 5f, cyan, 16f, 2.4f);

            // Isi ruang.
            var batu = AmbilMat("M_Batu"); var kayu = AmbilMat("M_Kayu"); var kain = AmbilMat("M_CatOranye");
            var tulang = MatPolos("M_TulangGua", "#D8D0BC");
            for (int i = 0; i < 4; i++) Hias(hias, PrimitiveType.Capsule, "Kerangka_" + i, r1.pos + new Vector3(3f + i * 0.35f, 0.15f, -4f), new Vector3(0.12f, 0.35f, 0.12f), tulang, false);
            Wadah(k, "Ransel_Lore4", "Jurnal Ekspedisi", r1.pos + new Vector3(4.5f, 0f, -4.2f), new Vector3(0.6f, 0.45f, 0.4f), kain, new ItemID[0], new int[0], (int)LoreID.L04, false);
            Hias(hias, PrimitiveType.Cube, "Tenda", r2.pos + new Vector3(-2f, 0.9f, 1f), new Vector3(3f, 1.8f, 2.4f), kain, true);
            Wadah(k, "Peti_Ekspedisi", "Peti Ekspedisi", r2.pos + new Vector3(2.5f, 0f, -1f), new Vector3(1.1f, 0.6f, 0.7f), kayu,
                new[] { ItemID.AlatSelamRebreather, ItemID.KartuAksesBunker, ItemID.Baterai, ItemID.Tulang }, new[] { 1, 1, 2, 3 }, (int)LoreID.L11, false);
            Hias(hias, PrimitiveType.Cube, "AltarBatu", r4.pos + new Vector3(2f, 0.5f, 3f), new Vector3(2.4f, 1f, 1.4f), batu, true);
            Wadah(k, "Paku_1", "Paku Keramat", r4.pos + new Vector3(2f, 1f, 3f), new Vector3(0.12f, 0.5f, 0.12f), MatEmisi("M_PakuKeramat", "#C9A23A", new Color(1f, 0.75f, 0.2f) * 1.4f, false),
                new[] { ItemID.PakuKeramat1 }, new[] { 1 }, 0, true);

            // T2 jalan pintas: reruntuhan hanya didorong dari sisi R4.
            var t2 = _lorong[3].titik;
            Vector3 posRuntuh = t2[0] + (t2[1] - t2[0]).normalized * 3f;
            var runtuh = Anak(k, "Reruntuhan_T2");
            runtuh.transform.localPosition = posRuntuh;
            runtuh.transform.localRotation = Quaternion.LookRotation(t2[1] - t2[0], Vector3.up);
            runtuh.layer = GameLayers.Interactable; runtuh.tag = GameTags.Interactable;
            var bc = runtuh.AddComponent<BoxCollider>(); bc.size = new Vector3(3f, 3f, 1.5f); bc.center = Vector3.up * 1.5f;
            for (int i = 0; i < 5; i++) Hias(runtuh, PrimitiveType.Sphere, "Batu_" + i, new Vector3(-1f + (i % 3), 0.6f + (i / 3) * 1.1f, 0f), Vector3.one * 1.3f, batu, true);
            var rd = runtuh.AddComponent<RintanganDorong>();
            rd.Pasang(r4.pos - posRuntuh);
            EditorUtility.SetDirty(rd);

            // Pintu keluar R0 -> permukaan (mulut gua di lembah bambu).
            Pintu(k, "Pintu_Keluar", r0.pos + new Vector3(-3f, 0f, 0f), "KELUAR GUA", Permukaan(270f, -276f, 0.3f), 90f, 0, false, "");
            return k;
        }
    }
}
