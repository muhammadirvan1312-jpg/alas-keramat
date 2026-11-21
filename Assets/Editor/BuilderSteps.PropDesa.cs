using System.Collections.Generic;
using AlasKeramat.Building;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.EditorTools
{
    // Props desa CC0 (Poly Haven, dipanggang Tools/visual/panggang_pohon.py): peti, drum, bakul, ember, periuk,
    // dingklik, jeriken, ban bekas, panci enamel, tangga, bangku, kapak, lampu minyak. Ditebar di sekitar rumah
    // Desa Mati, Kampung Sukamaju, Kampung Watu Ireng, lokasi jatuh heli dan Pabrik Gula. Lampu minyak menyala malam.
    public static partial class NusantaraMasterBuilder
    {
        public const int PropPeti = 0, PropDrum = 1, PropBakul = 2, PropEmber = 3, PropPeriuk = 4, PropDingklik = 5, PropJeriken = 6,
            PropBan = 7, PropPanci = 8, PropTangga = 9, PropBangku = 10, PropKapak = 11, PropLampu = 12;
        public static readonly string[] NamaProp = { "PetiKayu", "Drum", "Bakul", "EmberKayu", "Periuk", "Dingklik", "Jeriken", "BanBekas", "Panci", "Tangga", "Bangku", "Kapak", "LampuMinyak" };
        static readonly string[] MeshProp = { "PetiKayuScan", "DrumScan", "BakulScan", "EmberKayuScan", "PeriukScan", "DingklikScan", "JerikenScan", "BanBekasScan", "PanciScan", "TanggaScan", "BangkuScan", "KapakScan", "LampuMinyakScan" };
        static readonly string[] SlotProp = { "PropPeti", "PropDrum", "PropBakul", "PropEmber", "PropPeriuk", "PropDingklik", "PropJeriken", "PropBan", "PropPanci", "PropTangga", "PropBangku", "PropKapak", "PropLampu" };
        static readonly float[] HalusProp = { 0.18f, 0.34f, 0.12f, 0.16f, 0.30f, 0.18f, 0.32f, 0.10f, 0.45f, 0.15f, 0.20f, 0.24f, 0.50f };
        static readonly bool[] KoliderProp = { true, true, true, true, true, true, true, true, true, true, true, false, false };
        // Wadah yang bisa digeledah (-1 = tidak): peti, drum, bakul, ember, periuk, jeriken, panci.
        static readonly int[] WadahProp = { (int)PropGeledah.Wadah.Peti, (int)PropGeledah.Wadah.BahanBakar, (int)PropGeledah.Wadah.Dapur, (int)PropGeledah.Wadah.Ember, (int)PropGeledah.Wadah.Dapur, -1, (int)PropGeledah.Wadah.BahanBakar, -1, (int)PropGeledah.Wadah.Dapur, -1, -1, -1, -1 };
        public static int JumlahGeledah;
        // Isi halaman rumah yang ditinggal mendadak.
        static readonly int[] PropHalaman = { PropPeriuk, PropEmber, PropBakul, PropDingklik, PropBangku, PropTangga, PropKapak, PropPanci, PropPeti };
        const float CullKecil = 0.035f, CullBesar = 0.015f, JarakCelahRumah = 0.7f, SebaranCelah = 1.4f, LerengMaksProp = 1.1f;
        const float JangkauanLampu = 5.5f, IntensitasLampu = 1.4f;
        static readonly Color WarnaApiLampu = new Color(1f, 0.62f, 0.30f, 1f);
        const int CobaTaruh = 7;
        const float FaktorDiagonal = 0.8f;

        // Atap genteng foto CC0: 1 UV atap = 2 m (ProcBangunan.Quad skala 2); genteng 2,6 m, genteng lumut 3,5 m per ubin tekstur.
        const float UkuranUVAtap = 2f, UkuranGenteng = 2.6f, UkuranGentengLumut = 3.5f;
        static readonly Color TintGenteng = new Color(0.86f, 0.80f, 0.76f, 1f), TintGentengLumut = new Color(0.78f, 0.80f, 0.74f, 1f);

        public static int JumlahPropDesa, JumlahLampuDesa, JumlahAtapGenteng;
        static Mesh[] _meshProp;
        static Material[] _matProp;
        static Material _matApiLampu;

        static void LangkahPropDesa()
        {
            JumlahPropDesa = 0; JumlahLampuDesa = 0; JumlahGeledah = 0;
            var lm = GameObject.Find(NamaRoot + "/World/Landmark");
            if (lm == null) lm = GameObject.Find(NamaRoot + "/Landmark");
            if (lm == null) { Catat("Prop desa: induk Landmark tidak ada, dilewati"); return; }
            LangkahSuasanaDesa(lm);

            _meshProp = new Mesh[NamaProp.Length]; _matProp = new Material[NamaProp.Length];
            int ada = 0;
            for (int i = 0; i < NamaProp.Length; i++)
            {
                _meshProp[i] = MuatPropScan(MeshProp[i]);
                if (_meshProp[i] == null) continue;
                _matProp[i] = MaterialSolidScan(SlotProp[i], "M_Prop" + NamaProp[i], new Color(0.86f, 0.83f, 0.78f, 1f), HalusProp[i]);
                if (_matProp[i] == null) _matProp[i] = AmbilMat("M_Kayu");
                ada++;
            }
            if (ada == 0) { Catat("Prop desa: aset panggang belum ada, dilewati (fail-soft)"); return; }
            _matApiLampu = SimpanMat(SafeMaterialFactory.CreatePBR("M_ApiLampuMinyak", WarnaApiLampu, 0f, 0.1f, null, null, new Color(4f, 2.2f, 0.75f, 1f)));

            GantiAtapGenteng(lm);
            var induk = Anak(lm, "PropDesa");
            Physics.SyncTransforms();
            var rnd = new System.Random(4127);
            var rumah = new List<Transform>();
            for (int i = 0; i < lm.transform.childCount; i++)
            {
                var t = lm.transform.GetChild(i);
                if (t.name.StartsWith("RumahDesa_") || t.name.StartsWith("RumahKampung_") || t.name.StartsWith("KampungWatuIreng_")) rumah.Add(t);
            }
            for (int i = 0; i < rumah.Count; i++)
            {
                var t = rumah[i];
                bool desaMati = t.name.StartsWith("RumahDesa_");
                int jumlah = desaMati ? 3 + rnd.Next(2) : 1 + rnd.Next(3);
                for (int k = 0; k < jumlah; k++) TaruhDiHalaman(induk, t, PropHalaman[rnd.Next(PropHalaman.Length)], rnd);
                bool pakaiLampu = desaMati || (t.name.StartsWith("RumahKampung_") && i % 4 == 0) || (t.name.StartsWith("KampungWatuIreng_") && i % 2 == 0);
                if (pakaiLampu) TaruhLampu(induk, t, rnd);
            }
            // Lokasi jatuh heli: logistik & bahan bakar berserakan.
            KlasterBebas(induk, new Vector2(8f, -448f), 6f, 12f, new[] { PropJeriken, PropJeriken, PropJeriken, PropDrum, PropDrum, PropBan, PropPeti, PropPeti }, rnd);
            // Pabrik Gula Sinar Tebu: drum & ban bekas di halaman pabrik.
            KlasterBebas(induk, new Vector2(665f, -500f), 4f, 22f, new[] { PropDrum, PropDrum, PropDrum, PropDrum, PropBan, PropBan, PropJeriken, PropJeriken, PropPeti, PropPeti, PropTangga }, rnd);
            Catat("Prop desa CC0: " + JumlahPropDesa + " props (" + JumlahGeledah + " bisa digeledah), " + JumlahLampuDesa + " lampu minyak (" + ada + "/" + NamaProp.Length + " jenis)");
        }

        static Material MaterialGenteng(string nama, string tex, Color tint, float ukuran)
        {
            var a = Tex(tex);
            if (a == null) return null;
            var m = SafeMaterialFactory.CreatePBR(nama, tint, 0f, 0.22f, a, Tex(tex + "_n"));
            m.SetTextureScale("_BaseMap", Vector2.one * (UkuranUVAtap / ukuran));
            MaskPBR(m, tex);
            return SimpanMat(m);
        }

        // Atap seng/ijuk rumah desa diganti genteng tanah liat (Kampung Sukamaju, 3 dari 5 rumah) dan genteng
        // berlumut (semua rumah Desa Mati). Kampung Watu Ireng tetap ijuk (adat). Hanya submesh atap (indeks 1).
        static void GantiAtapGenteng(GameObject lm)
        {
            JumlahAtapGenteng = 0;
            var genteng = MaterialGenteng("M_AtapGenteng", "genteng", TintGenteng, UkuranGenteng);
            var lumut = MaterialGenteng("M_AtapGentengLumut", "genteng_lumut", TintGentengLumut, UkuranGentengLumut);
            if (genteng == null && lumut == null) { Catat("Atap genteng: foto CC0 belum ada, atap lama dipakai"); return; }
            for (int i = 0; i < lm.transform.childCount; i++)
            {
                var t = lm.transform.GetChild(i);
                Material baru = null;
                if (t.name.StartsWith("RumahDesa_")) baru = lumut != null ? lumut : genteng;
                else if (t.name.StartsWith("RumahKampung_"))
                {
                    int no;
                    if (int.TryParse(t.name.Substring("RumahKampung_".Length), out no) && no % 5 != 1 && no % 5 != 3) baru = genteng != null ? genteng : lumut;
                }
                if (baru == null) continue;
                var mr = t.GetComponent<MeshRenderer>();
                if (mr == null) continue;
                var mats = mr.sharedMaterials;
                if (mats.Length < 2) continue;
                mats[1] = baru;
                mr.sharedMaterials = mats;
                JumlahAtapGenteng++;
            }
            Catat("Atap genteng CC0: " + JumlahAtapGenteng + " rumah");
        }

        static float RadiusRumah(Transform rumah)
        {
            var r = rumah.GetComponent<Renderer>();
            if (r == null) return 4.5f;
            var e = r.bounds.extents;
            // AABB rumah berputar: pakai 0,8 x diagonal agar props tidak masuk ke sudut dinding.
            return Mathf.Max(Mathf.Max(e.x, e.z), Mathf.Sqrt(e.x * e.x + e.z * e.z) * FaktorDiagonal);
        }

        static bool LokasiBebas(Vector3 pos, float radius)
        {
            float kiri = WorldTerrainGenerator.SampleHeight(pos.x - 1f, pos.z), kanan = WorldTerrainGenerator.SampleHeight(pos.x + 1f, pos.z);
            float depan = WorldTerrainGenerator.SampleHeight(pos.x, pos.z + 1f), belakang = WorldTerrainGenerator.SampleHeight(pos.x, pos.z - 1f);
            if (Mathf.Abs(kiri - kanan) > LerengMaksProp || Mathf.Abs(depan - belakang) > LerengMaksProp) return false;
            return !Physics.CheckSphere(pos + Vector3.up * (radius + 0.15f), radius, 1 << GameLayers.Structure, QueryTriggerInteraction.Ignore);
        }

        static Vector3 CariTitik(Vector2 pusat, float rMin, float rMaks, float radiusBenda, System.Random rnd, out bool ok)
        {
            for (int c = 0; c < CobaTaruh; c++)
            {
                float a = (float)rnd.NextDouble() * Mathf.PI * 2f, r = rMin + (float)rnd.NextDouble() * (rMaks - rMin);
                float x = pusat.x + Mathf.Cos(a) * r, z = pusat.y + Mathf.Sin(a) * r;
                var p = new Vector3(x, WorldTerrainGenerator.SampleHeight(x, z), z);
                if (LokasiBebas(p, radiusBenda)) { ok = true; return p; }
            }
            ok = false; return Vector3.zero;
        }

        static float RadiusBenda(int jenis)
        {
            var m = _meshProp[jenis];
            if (m == null) return 0.3f;
            return Mathf.Clamp(Mathf.Max(m.bounds.extents.x, m.bounds.extents.z), 0.12f, 0.9f);
        }

        static GameObject TaruhDiHalaman(GameObject induk, Transform rumah, int jenis, System.Random rnd)
        {
            if (_meshProp[jenis] == null) return null;
            float rr = RadiusRumah(rumah) + JarakCelahRumah;
            bool ok;
            var p = CariTitik(new Vector2(rumah.position.x, rumah.position.z), rr, rr + SebaranCelah, RadiusBenda(jenis), rnd, out ok);
            if (!ok) return null;
            return BuatProp(induk, jenis, p, (float)rnd.NextDouble() * 360f, rnd);
        }

        static void KlasterBebas(GameObject induk, Vector2 pusat, float rMin, float rMaks, int[] daftar, System.Random rnd)
        {
            for (int i = 0; i < daftar.Length; i++)
            {
                int j = daftar[i];
                if (_meshProp[j] == null) continue;
                bool ok;
                var p = CariTitik(pusat, rMin, rMaks, RadiusBenda(j), rnd, out ok);
                if (ok) BuatProp(induk, j, p, (float)rnd.NextDouble() * 360f, rnd);
            }
        }

        // Rumah yang dihuni sebelum ditinggal: peti di halaman dengan lampu minyak di atasnya (di tanah bila peti tak muat).
        static void TaruhLampu(GameObject induk, Transform rumah, System.Random rnd)
        {
            if (_meshProp[PropLampu] == null) return;
            var peti = TaruhDiHalaman(induk, rumah, PropPeti, rnd);
            Vector3 p; float yaw = (float)rnd.NextDouble() * 360f;
            if (peti != null) p = peti.transform.position + Vector3.up * _meshProp[PropPeti].bounds.max.y;
            else
            {
                float rr = RadiusRumah(rumah) + JarakCelahRumah; bool ok;
                p = CariTitik(new Vector2(rumah.position.x, rumah.position.z), rr, rr + SebaranCelah, RadiusBenda(PropLampu), rnd, out ok);
                if (!ok) return;
            }
            var lampu = BuatProp(induk, PropLampu, p, yaw, null);
            float tinggi = _meshProp[PropLampu].bounds.max.y;

            var api = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            api.name = "Api";
            Object.DestroyImmediate(api.GetComponent<Collider>());
            api.transform.SetParent(lampu.transform, false);
            api.transform.localPosition = new Vector3(0f, tinggi * 0.46f, 0f);
            api.transform.localScale = new Vector3(0.03f, 0.06f, 0.03f);
            var mr = api.GetComponent<MeshRenderer>();
            mr.sharedMaterial = _matApiLampu; mr.shadowCastingMode = ShadowCastingMode.Off; mr.receiveShadows = false; mr.enabled = false;

            var gc = new GameObject("Cahaya");
            gc.transform.SetParent(lampu.transform, false);
            gc.transform.localPosition = new Vector3(0f, tinggi * 0.55f, 0f);
            var l = gc.AddComponent<Light>();
            l.type = LightType.Point; l.range = JangkauanLampu; l.intensity = IntensitasLampu; l.color = WarnaApiLampu;
            l.shadows = LightShadows.None; l.renderMode = LightRenderMode.Auto; l.enabled = false;

            var lm = lampu.AddComponent<LampuMinyak>();
            lm.Api = mr; lm.Cahaya = l; lm.Intensitas = IntensitasLampu;
            lm.Laron = BuatLaron(lampu, tinggi * 0.5f);
            EditorUtility.SetDirty(lm);
            JumlahLampuDesa++;
        }

        static GameObject BuatProp(GameObject induk, int jenis, Vector3 pos, float yaw, System.Random rnd)
        {
            var mesh = _meshProp[jenis];
            var go = Anak(induk, NamaProp[jenis] + "_" + JumlahPropDesa);
            go.layer = GameLayers.Structure;
            var rot = Quaternion.Euler(0f, yaw, 0f);
            // Rumah ditinggal tergesa-gesa: sebagian ember/dingklik terguling, kapak & tangga bersandar miring.
            if (rnd != null)
            {
                double u = rnd.NextDouble();
                if ((jenis == PropEmber && u < 0.35) || (jenis == PropDingklik && u < 0.2))
                {
                    rot = Quaternion.Euler(0f, yaw, 90f);
                    pos += Vector3.up * Mathf.Max(mesh.bounds.extents.x, mesh.bounds.extents.z);
                }
                else if (jenis == PropKapak) rot = Quaternion.Euler(0f, yaw, 14f);
                else if (jenis == PropBan && u < 0.5) rot = Quaternion.Euler(0f, yaw, 8f);
            }
            go.transform.SetPositionAndRotation(pos, rot);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = _matProp[jenis];
            if (KoliderProp[jenis])
            {
                var bc = go.AddComponent<BoxCollider>();
                bc.center = mesh.bounds.center; bc.size = mesh.bounds.size;
            }
            if (WadahProp[jenis] >= 0 && KoliderProp[jenis])
            {
                var gl = go.AddComponent<PropGeledah>();
                gl.Jenis = (PropGeledah.Wadah)WadahProp[jenis];
                EditorUtility.SetDirty(gl);
                JumlahGeledah++;
            }
            var lod = go.AddComponent<LODGroup>();
            lod.SetLODs(new[] { new LOD(KoliderProp[jenis] ? CullBesar : CullKecil, new Renderer[] { mr }) });
            lod.RecalculateBounds();
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
            Physics.SyncTransforms();
            JumlahPropDesa++;
            return go;
        }
    }
}
