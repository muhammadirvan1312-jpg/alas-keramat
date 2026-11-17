using System.Collections.Generic;
using UnityEngine;

namespace AlasKeramat.Core
{
    // H16 + 14 (T7c): satu pintu untuk semua darah, luka, mutilasi, ragdoll & mayat; membaca GoreLevel (Mati/Sedang/Penuh).
    // Partikel darah (pool 1 ParticleSystem, maks 300 hidup), decal percikan/genangan (ring buffer 64, mengering 120 s, pudar 10 menit),
    // luka menempel (maks 6/karakter), ragdoll (maks 4 aktif), prop anggota tubuh (maks 12), mayat (maks 12).
    public partial class GoreSystem : MonoBehaviour
    {
        public const int MaksDecal = 64, MaksLuka = 6, MaksRagdoll = 4, MaksProp = 12, MaksMayat = 12, MaksPartikel = 300, MaksTetesDecal = 3;
        public const float LamaKering = 120f, LamaPudar = 600f, LamaPudarTua = 1f, LamaGenangan = 6f, RadiusGenangan = 0.9f;
        public const string KunciPengaturan = "ak_gore", KunciPeringatan = "ak_peringatan_18";
        static readonly Color BasahMerah = new Color(0.29f, 0.02f, 0.02f, 1f), KeringMerah = new Color(0.165f, 0.04f, 0.024f, 1f);
        static readonly Color DarahPartikel = new Color(0.478f, 0.04f, 0.04f, 1f), DarahHitam = new Color(0.04f, 0.04f, 0.04f, 1f), Debu = new Color(0.35f, 0.33f, 0.3f, 1f);

        static GoreLevel _tingkat = GoreLevel.Sedang;
        static bool _dimuat;

        public static GoreLevel Tingkat
        {
            get { if (!_dimuat) { _dimuat = true; _tingkat = (GoreLevel)Mathf.Clamp(PlayerPrefs.GetInt(KunciPengaturan, (int)GoreLevel.Sedang), 0, 2); } return _tingkat; }
            set { _tingkat = value; _dimuat = true; PlayerPrefs.SetInt(KunciPengaturan, (int)value); PlayerPrefs.Save(); }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatis() { _dimuat = false; }

        [SerializeField] ParticleSystem partikel;
        [SerializeField] Material[] matPercikan = new Material[4];
        [SerializeField] Material matGenangan, matLuka, matTutup;
        [SerializeField] Mesh[] meshLuka = new Mesh[8];

        class Decal { public Transform t; public Renderer r; public float lahir, ukuranAkhir; public bool genangan; public float pudar = -1f; }
        readonly Decal[] _decal = new Decal[MaksDecal];
        int _iDecal;
        MaterialPropertyBlock _mpb;
        URPAtmosphereAndWeather _cuaca;

        public void Pasang(ParticleSystem ps, Material[] percikan, Material genangan, Material luka, Material tutup, Mesh[] lukaMesh)
        { partikel = ps; matPercikan = percikan; matGenangan = genangan; matLuka = luka; matTutup = tutup; meshLuka = lukaMesh; }

        void Awake()
        {
            GameServices.Register(this);
            _mpb = new MaterialPropertyBlock();
            if (partikel != null) { var m = partikel.main; m.maxParticles = MaksPartikel; }
        }

        // ---------- Rumus murni ----------
        public static int JumlahTetes(float damage) { return Mathf.Min(40, 6 + Mathf.FloorToInt(damage / 4f)); }
        public static bool BolehDarah(GoreLevel g) { return g != GoreLevel.Mati; }
        public static bool BolehMutilasi(GoreLevel g) { return g == GoreLevel.Penuh; }
        public static bool BolehMayat(GoreLevel g) { return g != GoreLevel.Mati; }
        public static Color WarnaKering(float umur, float lajuHujan) { return Color.Lerp(BasahMerah, KeringMerah, Mathf.Clamp01(umur * lajuHujan / LamaKering)); }

        // ---------- Darah ----------
        // Semburan di titik kena: arah pukulan + gravitasi; hantu/mutan darah hitam; Gore Mati = debu.
        public void Semburan(Vector3 titik, Vector3 arah, float damage, bool hitam, bool berat)
        {
            if (partikel == null) return;
            var g = Tingkat;
            int n = JumlahTetes(damage) * (g == GoreLevel.Penuh ? 2 : 1);
            if (berat && g == GoreLevel.Penuh) n += 10; // kabut darah
            var ep = new ParticleSystem.EmitParams();
            Color warna = g == GoreLevel.Mati ? Debu : hitam ? DarahHitam : DarahPartikel;
            for (int i = 0; i < n; i++)
            {
                ep.position = titik;
                ep.velocity = (arah.normalized * Random.Range(1.5f, 4.5f) + Random.insideUnitSphere * 1.6f + Vector3.up * 1.2f);
                ep.startSize = Random.Range(0.01f, 0.04f) * (g == GoreLevel.Mati ? 3f : 1f);
                ep.startColor = warna;
                ep.startLifetime = Random.Range(0.5f, 1.1f);
                partikel.Emit(ep, 1);
            }
            if (!BolehDarah(g)) return;
            int tetes = Mathf.Min(MaksTetesDecal, 1 + n / 12);
            for (int i = 0; i < tetes; i++)
            {
                Vector3 d = (arah.normalized + Vector3.down * 1.5f + Random.insideUnitSphere * 0.4f).normalized;
                RaycastHit hit;
                if (Physics.Raycast(titik, d, out hit, 4f, (1 << GameLayers.Ground) | (1 << GameLayers.Structure) | 1, QueryTriggerInteraction.Ignore))
                    TaruhDecal(hit.point, hit.normal, Random.Range(0.3f, 1.2f) * (hitam ? 0.6f : 1f), false, hitam);
                else
                {
                    Vector3 p = titik + d * 2f;
                    float h = WorldTerrainGenerator.SampleHeight(p.x, p.z);
                    TaruhDecal(new Vector3(p.x, h, p.z), WorldTerrainGenerator.SampleNormal(p.x, p.z), Random.Range(0.3f, 1.0f), false, hitam);
                }
            }
        }

        public void Genangan(Vector3 posisi)
        {
            if (!BolehDarah(Tingkat)) return;
            float h = WorldTerrainGenerator.SampleHeight(posisi.x, posisi.z);
            TaruhDecal(new Vector3(posisi.x, Mathf.Max(h, posisi.y - 0.5f), posisi.z), WorldTerrainGenerator.SampleNormal(posisi.x, posisi.z), RadiusGenangan * 2f, true, false);
        }

        void TaruhDecal(Vector3 p, Vector3 n, float ukuran, bool genangan, bool hitam)
        {
            Material m = genangan ? matGenangan : (matPercikan != null && matPercikan.Length > 0 ? matPercikan[Random.Range(0, matPercikan.Length)] : null);
            if (m == null) return;
            var d = _decal[_iDecal];
            if (d == null)
            {
                d = new Decal();
                var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Destroy(go.GetComponent<Collider>());
                go.name = "DecalDarah";
                go.transform.SetParent(transform, false);
                d.t = go.transform; d.r = go.GetComponent<Renderer>();
                d.r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _decal[_iDecal] = d;
            }
            d.r.sharedMaterial = m;
            d.t.gameObject.SetActive(true);
            d.t.position = p + n * (0.01f + _iDecal * 0.0002f);
            d.t.rotation = Quaternion.LookRotation(-n) * Quaternion.Euler(0f, 0f, Random.value * 360f);
            d.ukuranAkhir = ukuran; d.genangan = genangan; d.lahir = Time.time; d.pudar = -1f;
            d.t.localScale = Vector3.one * (genangan ? 0.05f : ukuran);
            _mpb.Clear(); _mpb.SetColor("_BaseColor", hitam ? DarahHitam : BasahMerah); d.r.SetPropertyBlock(_mpb);
            _iDecal = (_iDecal + 1) % MaksDecal;
            // Yang tertua berikutnya memudar 1 s sebelum dipakai ulang.
            var tua = _decal[_iDecal];
            if (tua != null && tua.t.gameObject.activeSelf) tua.pudar = Time.time;
        }

        public int DecalAktif { get { int n = 0; for (int i = 0; i < MaksDecal; i++) if (_decal[i] != null && _decal[i].t.gameObject.activeSelf) n++; return n; } }

        float _tDecal;
        void Update()
        {
            _tDecal -= Time.deltaTime;
            if (_tDecal > 0f) return;
            _tDecal = 0.25f;
            if (_cuaca == null) _cuaca = GameServices.Get<URPAtmosphereAndWeather>();
            float hujan = _cuaca != null && (_cuaca.Cuaca == WeatherState.Gerimis || _cuaca.Cuaca == WeatherState.HujanBadai) ? 4f : 1f;
            for (int i = 0; i < MaksDecal; i++)
            {
                var d = _decal[i];
                if (d == null || !d.t.gameObject.activeSelf) continue;
                float umur = Time.time - d.lahir;
                if (d.genangan) d.t.localScale = Vector3.one * Mathf.Lerp(0.05f, d.ukuranAkhir, Mathf.Clamp01(umur / LamaGenangan));
                float alfa = 1f;
                if (d.pudar >= 0f) alfa = 1f - Mathf.Clamp01((Time.time - d.pudar) / LamaPudarTua);
                if (umur > LamaPudar) alfa = Mathf.Min(alfa, 1f - Mathf.Clamp01((umur - LamaPudar) / 10f));
                if (alfa <= 0f) { d.t.gameObject.SetActive(false); continue; }
                d.r.GetPropertyBlock(_mpb);
                Color c = WarnaKering(umur, hujan); c.a = alfa;
                _mpb.SetColor("_BaseColor", c);
                _mpb.SetFloat("_Smoothness", Mathf.Lerp(0.85f, 0.3f, Mathf.Clamp01(umur * hujan / LamaKering)));
                d.r.SetPropertyBlock(_mpb);
            }
        }

        // ---------- Luka menempel ----------
        public void Luka(Transform tulang, Vector3 titik, Vector3 normal, DamageType tipe, List<GameObject> daftar)
        {
            if (!BolehDarah(Tingkat) || tulang == null || matLuka == null || meshLuka == null || meshLuka.Length == 0) return;
            if (daftar != null && daftar.Count >= MaksLuka) { var lama = daftar[0]; daftar.RemoveAt(0); if (lama != null) Destroy(lama); }
            int v = tipe == DamageType.Tusuk || tipe == DamageType.Proyektil ? 2 : tipe == DamageType.Tumpul ? 4 : tipe == DamageType.Api ? 6 : 0;
            v += Random.Range(0, 2);
            var mesh = meshLuka[Mathf.Clamp(v, 0, meshLuka.Length - 1)];
            if (mesh == null) return;
            var go = new GameObject("Luka");
            go.transform.SetParent(tulang, false);
            go.transform.position = titik + normal * 0.005f;
            go.transform.rotation = Quaternion.LookRotation(-normal) * Quaternion.Euler(0f, 0f, Random.value * 360f);
            go.transform.localScale = Vector3.one * Random.Range(0.07f, 0.12f) / Mathf.Max(0.01f, tulang.lossyScale.x);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>(); r.sharedMaterial = matLuka; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            if (daftar != null) daftar.Add(go);
        }

        public Material MaterialTutup { get { return matTutup; } }
    }
}
