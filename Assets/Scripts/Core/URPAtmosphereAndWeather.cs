using UnityEngine;

namespace AlasKeramat.Core
{
    // Atmosfer menurut jam: matahari, bulan, langit, kabut, cahaya ambien (T2a). Cuaca penuh di T2b.
    public partial class URPAtmosphereAndWeather : MonoBehaviour
    {
        public const float SudutUtara = -35f;
        const float IntensitasMatahari = 2.15f, IntensitasBulan = 1.65f;
        const float KabutSiang = 0.0038f, KabutMalam = 0.0058f, KabutMaghrib = 0.0046f; // kabut jarak lembut memadukan bukit ke langit

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
            RenderSettings.reflectionIntensity = 0.22f;
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
            // Maks 8 kunci per Gradient (batas Unity) — pantulan ambien senja/maghrib/malam diangkat agar siluet kaya detail.
            _warnaMatahari = G("#34486E@0", "#FF9252@5.8", "#FFDAA6@7.5", "#FFF4DF@12", "#FF6836@18", "#C83420@18.6", "#34486E@19.5", "#34486E@24");
            _warnaLangit = G("#0A1020@0", "#F09B64@6", "#88B6DE@8", "#74A6DA@13", "#D45A34@18", "#822422@18.6", "#0E1626@19.6", "#0A1020@24");
            _warnaKabut = G("#142234@0", "#646072@5.5", "#B8C8C4@7", "#9ABCC4@12", "#AE523A@18", "#5E2424@18.7", "#18283C@19.6", "#142234@24");
            _ambienAtas = G("#284062@0", "#5E5A78@5.6", "#AED0EC@8", "#C4DDF0@12", "#BA6046@18", "#482428@18.8", "#284062@19.6", "#284062@24");
            _ambienTengah = G("#1B2D46@0", "#46424C@5.6", "#6A8664@8", "#76906E@12", "#7E3E2E@18", "#341C1E@18.8", "#1B2D46@19.6", "#1B2D46@24");
            _ambienBawah = G("#121D30@0", "#302A2C@5.6", "#424C38@8", "#4C5840@12", "#46261E@18", "#221414@18.8", "#121D30@19.6", "#121D30@24");
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
                moon.color = ProcTex.Hex("#A0C4F4");
                moon.enabled = moon.intensity > 0.001f;
            }
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = _ambienAtas.Evaluate(u);
            RenderSettings.ambientEquatorColor = _ambienTengah.Evaluate(u);
            RenderSettings.ambientGroundColor = _ambienBawah.Evaluate(u);
            TerapkanProbeAmbien(RenderSettings.ambientEquatorColor,RenderSettings.ambientSkyColor,RenderSettings.ambientGroundColor);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = _warnaKabut.Evaluate(u);
            bool maghrib = jam >= 17.6f && jam < 19.2f;
            RenderSettings.fogDensity = maghrib ? KabutMaghrib : Mathf.Lerp(KabutSiang, KabutMalam, Malam01(jam));
            RenderSettings.reflectionIntensity = Mathf.Lerp(0.54f, 0.26f, Malam01(jam));
            if (skybox != null)
            {
                RenderSettings.skybox = skybox;
                float m01 = Malam01(jam);
                Color langit = Color.Lerp(new Color(0.5f, 0.5f, 0.5f), _warnaLangit.Evaluate(u), maghrib ? 0.75f : 0.25f);
                SafeMaterialFactory.SetC(skybox, "_SkyTint", langit);
                SafeMaterialFactory.SetC(skybox, "_GroundColor", Color.Lerp(langit, Color.black, 0.6f));
                SafeMaterialFactory.SetF(skybox, "_AtmosphereThickness", maghrib ? 2.0f : 1.25f);
                SafeMaterialFactory.SetF(skybox, "_SunSize", 0.035f);

                // Shader HDRI AlasKeramat/Langit
                bool hdriLangit = skybox.HasProperty("_NightBlend");
                SafeMaterialFactory.SetF(skybox, "_NightBlend", m01);
                bool adaSenja = skybox.HasProperty("_DuskTex") && skybox.GetTexture("_DuskTex") != null;
                float senja = adaSenja ? Senja01(jam) : 0f;
                SafeMaterialFactory.SetF(skybox, "_DuskBlend", senja);
                Color tint = maghrib ? new Color(1.18f, 0.62f, 0.42f) : Color.Lerp(new Color(0.95f, 0.98f, 1.0f), new Color(0.46f, 0.60f, 0.90f), m01);
                if (senja > 0f) tint = Color.Lerp(tint, new Color(1.0f, 0.92f, 0.86f), senja * 0.7f);
                SafeMaterialFactory.SetC(skybox, "_Tint", tint);
                SafeMaterialFactory.SetF(skybox, "_Exposure", hdriLangit ? (maghrib ? 0.86f : Mathf.Lerp(0.88f, 0.44f, m01)) : Mathf.Lerp(1.22f, 0.24f, m01));
                SafeMaterialFactory.SetC(skybox, "_HorizonColor", RenderSettings.fogColor);
                SafeMaterialFactory.SetF(skybox, "_HorizonHeight", maghrib ? 0.22f : Mathf.Lerp(0.14f, 0.24f, m01));
                Light aktif = (sun != null && sun.enabled) ? sun : moon;
                if (aktif != null)
                {
                    Vector3 arah = -aktif.transform.forward;
                    skybox.SetVector("_SunDir", new Vector4(arah.x, arah.y, arah.z, 0f));
                    SafeMaterialFactory.SetC(skybox, "_SunColor", aktif == sun ? aktif.color * 1.15f : aktif.color * 0.55f);
                }
            }
            TerapkanCuaca();
            if (sun != null) RenderSettings.sun = sun.enabled ? sun : moon;
        }

        // Isi SH secara eksplisit: Builder/foto headless tidak menunggu bake langit atau GI realtime.
        public static void TerapkanProbeAmbien(Color tengah,Color atas)
        {
            TerapkanProbeAmbien(tengah,atas,Color.Lerp(tengah,Color.black,0.35f));
        }

        public static void TerapkanProbeAmbien(Color tengah,Color atas,Color bawah)
        {
            var probe=new UnityEngine.Rendering.SphericalHarmonicsL2();
            probe.AddAmbientLight(tengah.linear);
            probe.AddDirectionalLight(Vector3.up,atas.linear,0.42f);
            probe.AddDirectionalLight(Vector3.down,bawah.linear,0.22f);
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

        // Puncak langit senja (HDRI) sekitar 06:00 dan 18:10, 0 di luar fajar/maghrib.
        public static float Senja01(float jam)
        {
            float pagi = 1f - Mathf.Clamp01(Mathf.Abs(jam - 6.0f) / 1.0f);
            float sore = 1f - Mathf.Clamp01(Mathf.Abs(jam - 18.1f) / 1.3f);
            return Mathf.SmoothStep(0f, 1f, Mathf.Max(pagi * 0.75f, sore));
        }
    }
}
