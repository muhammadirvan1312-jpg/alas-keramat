using UnityEngine;

namespace AlasKeramat.Core
{
    // Atmosfer menurut jam: matahari, bulan, langit, kabut, cahaya ambien (T2a). Cuaca penuh di T2b.
    public partial class URPAtmosphereAndWeather : MonoBehaviour
    {
        public const float SudutUtara = -35f;
        const float IntensitasMatahari = 1.58f, IntensitasBulan = 0.82f;
        const float KabutSiang = 0.00175f, KabutMalam = 0.00285f, KabutMaghrib = 0.0022f; // kabut jarak lembut memadukan bukit ke langit

        [SerializeField] Light sun;
        [SerializeField] Light moon;
        [SerializeField] Material skybox;

        TimeOfDayClock _jam;
        float _jamTerapan = -1f;

        // Gradien warna sepanjang 24 jam (indeks 0..1 = 00:00..24:00).
        static Gradient _warnaMatahari, _warnaLangit, _warnaKabut, _ambienAtas, _ambienTengah, _ambienBawah;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatis() { _warnaMatahari = null; }

        public void Pasang(Light matahari, Light bulan, Material langit) { sun = matahari; moon = bulan; skybox = langit; }

        void Awake()
        {
            GameServices.Register(this);
            if (sun == null) Debug.LogWarning("[AK][Atmosfer] Lampu matahari belum dipasang Builder; atmosfer tidak berubah.");
        }

        void Start() { _jam = GameServices.Get<TimeOfDayClock>(); Terapkan(_jam != null ? _jam.Jam : 9f); }

        // Mode Dalam Ruangan (03 §10): diatur WorldCaveAndSaveManager saat Andi di gua.
        public static bool DalamRuangan;
        bool _ruanganTerapan;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetRuangan() { DalamRuangan = false; }

        public void TerapkanRuangan()
        {
            if (sun != null) sun.intensity = 0f;
            if (moon != null) moon.intensity = 0f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = WorldCaveAndSaveManager.AmbienGua;
            TerapkanProbeAmbien(WorldCaveAndSaveManager.AmbienGua,WorldCaveAndSaveManager.AmbienGua);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.025f, 0.03f, 0.04f);
            RenderSettings.fogDensity = WorldCaveAndSaveManager.KabutGua;
            RenderSettings.skybox = null;
            if (_hujan != null && _hujan.isPlaying) _hujan.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        void Update()
        {
            if (_jam == null) return;
            if (DalamRuangan) { if (!_ruanganTerapan) { _ruanganTerapan = true; TerapkanRuangan(); } return; }
            if (_ruanganTerapan) { _ruanganTerapan = false; _jamTerapan = -99f; }
            UpdateCuaca();
            // Update visual 50x per jam game cukup halus; saat transisi cuaca diperbarui tiap frame.
            if (Mathf.Abs(_jam.Jam - _jamTerapan) > 0.02f || _transisi < 1f) Terapkan(_jam.Jam);
        }

        static Gradient G(params string[] hexPerJam)
        {
            var g = new Gradient();
            var ck = new GradientColorKey[hexPerJam.Length];
            for (int i = 0; i < hexPerJam.Length; i++)
            {
                string[] bag = hexPerJam[i].Split('@');
                ck[i] = new GradientColorKey(ProcTex.Hex(bag[0]), float.Parse(bag[1], System.Globalization.CultureInfo.InvariantCulture) / 24f);
            }
            g.SetKeys(ck, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return g;
        }

        static void SiapkanGradien()
        {
            if (_warnaMatahari != null) return;
            // Maks 8 kunci per Gradient (batas Unity).
            _warnaMatahari = G("#2A3550@0", "#FF8A4C@5.8", "#FFD9A8@7.5", "#FFF4E0@12", "#FF5A2A@18", "#B31E12@18.6", "#2A3550@19.5", "#2A3550@24");
            _warnaLangit = G("#05070D@0", "#F09A62@6", "#8FB4D8@8", "#7FA8D6@13", "#D2502A@18", "#7A1410@18.6", "#0A0C16@19.6", "#05070D@24");
            _warnaKabut = G("#101A28@0", "#5A5868@5.5", "#C9B9A6@7", "#B9CFCB@12", "#B0402A@18", "#5A1210@18.7", "#141D2A@19.6", "#101A28@24");
            _ambienAtas = G("#344B68@0", "#54526E@5.6", "#9FC0DE@8", "#BBD3E8@12", "#B5523A@18", "#3A1412@18.8", "#344B68@19.6", "#344B68@24");
            _ambienTengah = G("#22364A@0", "#3E3C42@5.6", "#586E52@8", "#657C5D@12", "#7A3424@18", "#24100E@18.8", "#22364A@19.6", "#22364A@24");
            _ambienBawah = G("#14202C@0", "#262022@5.6", "#332E24@8", "#3C3629@12", "#3A1A12@18", "#140A08@18.8", "#101822@19.6", "#14202C@24");
        }

        public void Terapkan(float jam)
        {
            SiapkanGradien();
            _jamTerapan = jam;
            float u = Mathf.Repeat(jam, 24f) / 24f;
            // Matahari terbit 06:00 di timur, tenggelam 18:00 di barat.
            float sudutMatahari = (jam - 6f) / 12f * 180f;
            if (sun != null)
            {
                sun.transform.rotation = Quaternion.Euler(sudutMatahari, SudutUtara + 90f, 0f);
                float tinggi = Mathf.Sin(sudutMatahari * Mathf.Deg2Rad);
                sun.intensity = IntensitasMatahari * Mathf.Clamp01(tinggi * 3f);
                sun.color = _warnaMatahari.Evaluate(u);
                sun.enabled = sun.intensity > 0.001f;
            }
            if (moon != null)
            {
                moon.transform.rotation = Quaternion.Euler(sudutMatahari + 180f, SudutUtara + 70f, 0f);
                float tinggiBulan = Mathf.Sin((sudutMatahari + 180f) * Mathf.Deg2Rad);
                moon.intensity = IntensitasBulan * Mathf.Clamp01(tinggiBulan * 3f);
                moon.color = ProcTex.Hex("#8EB2EA");
                moon.enabled = moon.intensity > 0.001f;
            }
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = _ambienAtas.Evaluate(u);
            RenderSettings.ambientEquatorColor = _ambienTengah.Evaluate(u);
            RenderSettings.ambientGroundColor = _ambienBawah.Evaluate(u);
            TerapkanProbeAmbien(RenderSettings.ambientEquatorColor,RenderSettings.ambientSkyColor);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = _warnaKabut.Evaluate(u);
            bool maghrib = jam >= 17.6f && jam < 19.2f;
            RenderSettings.fogDensity = maghrib ? KabutMaghrib : Mathf.Lerp(KabutSiang, KabutMalam, Malam01(jam));
            if (skybox != null)
            {
                RenderSettings.skybox = skybox;
                // _SkyTint bawaan 0,5 abu; warna penuh membuat langit kuning-hijau. Hanya digeser sebagian ke warna jam.
                Color langit = Color.Lerp(new Color(0.5f, 0.5f, 0.5f), _warnaLangit.Evaluate(u), maghrib ? 0.75f : 0.25f);
                SafeMaterialFactory.SetC(skybox, "_SkyTint", langit);
                SafeMaterialFactory.SetC(skybox, "_GroundColor", Color.Lerp(langit, Color.black, 0.6f));
                SafeMaterialFactory.SetF(skybox, "_Exposure", Mathf.Lerp(1.22f, 0.16f, Malam01(jam)));
                SafeMaterialFactory.SetF(skybox, "_AtmosphereThickness", maghrib ? 2.0f : 1.25f);
                SafeMaterialFactory.SetF(skybox, "_SunSize", 0.035f);
            }
            TerapkanCuaca();
            if (sun != null) RenderSettings.sun = sun.enabled ? sun : moon;
        }

        // Isi SH secara eksplisit: Builder/foto headless tidak menunggu bake langit atau GI realtime.
        public static void TerapkanProbeAmbien(Color tengah,Color atas)
        {
            var probe=new UnityEngine.Rendering.SphericalHarmonicsL2();
            probe.AddAmbientLight(tengah.linear);
            probe.AddDirectionalLight(Vector3.up,atas.linear,0.35f);
            RenderSettings.ambientProbe=probe;
        }

        // 0 = siang penuh, 1 = malam penuh (transisi 17:00-20:00 dan 05:00-07:00).
        public static float Malam01(float jam)
        {
            if (jam >= 20f || jam < 5f) return 1f;
            if (jam >= 17f) return Mathf.InverseLerp(17f, 20f, jam);
            if (jam < 7f) return 1f - Mathf.InverseLerp(5f, 7f, jam);
            return 0f;
        }
    }
}
