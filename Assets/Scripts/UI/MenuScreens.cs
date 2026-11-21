using System;
using System.Collections;
using AlasKeramat.AI;
using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    public enum PresetGrafis { Rendah = 0, Sedang = 1, Tinggi = 2, Ultra = 3 }

    [Serializable]
    public struct DataPresetGrafis
    {
        public float renderScale, bayangan, farKamera, kabutTambah, pengaliVeg, detailVeg, rumput;
        public int cascade, msaa, targetFps;
        public bool hdr, bloom;
    }

    // T11 (09 §4 & §10, 05 §9, 11 §7, 17 §8): Menu Utama, Jeda, Pengaturan lengkap (PlayerPrefs + klon URP runtime),
    // Mode Imersif, Getar Haptik, Cara Bermain (6 kartu), Kredit, 3 Slot Simpan, dan Panel Debug HP (ketuk pojok kiri-atas 5x).
    public partial class MenuScreens : MonoBehaviour
    {
        public const string PrefPreset = "ak_preset_grafis", PrefSens = "ak_sens", PrefBalikY = "ak_balik_y";
        public const string PrefSkalaTombol = "ak_skala_tombol", PrefVolMaster = "ak_vol_master", PrefVolEfek = "ak_vol_efek";
        public const string PrefVolAmbien = "ak_vol_ambien", PrefVolMusik = "ak_vol_musik", PrefFov = "ak_fov";
        public const string PrefHeadBob = "ak_headbob", PrefHoror = "ak_horor", PrefHeadset = "ak_headset";
        public const string PrefKesulitan = "ak_kesulitan", PrefSlot = "ak_slot_aktif";
        public const string PrefGetar = "ak_getar", PrefImersif = "ak_imersif";
        public const int KetukDebugWajib = 5;
        public const float JendelaKetukDebug = 2.5f, AmbangDamageGetar = 20f;

        public static readonly string[] JudulCara =
        {
            "1. GERAK & EKSPLORASI",
            "2. TEMPUR, TANGKIS & ELAK",
            "3. TEBANG POHON & PANGGUL KAYU",
            "4. BANGUN BEBAS & CETAK BIRU",
            "5. TIKAR PANDAN & SURVIVAL",
            "6. COMPANION & TEROR MALAM"
        };

        public static readonly string[] IsiCara =
        {
            "Gunakan joystick kiri untuk berjalan, dorong penuh ke atas untuk LARI. Geser sisi kanan layar untuk melihat sekeliling. Ketuk LOMPAT atau JONGKOK untuk menyelinap di semak dan menghindari deteksi musuh.",
            "Ketuk TEBAS untuk serangan cepat atau tahan untuk tebasan berat penghancur pertahanan. Ketuk TANGKIS tepat saat musuh mengayun untuk memantulkan serangan (parry), atau ketuk LOMPAT + arah saat bertempur untuk ELAK.",
            "Arahkan ke batang pohon dan ketuk TEBAS untuk membuat takikan arah rebah. Pohon tumbang menjauhi sisi takikan terdalam. Ambil gelondong kayu ke bahu (maks 2) untuk dibangun atau dilempar.",
            "Saat memanggul kayu, ketuk MODE untuk memilih Tiang, Balok, Papan, atau Runcing lalu ketuk TANGAN untuk memasang. Buka BUKU PANDUAN untuk menaruh cetak biru Saung, Api Unggun, Perangkap, dan Benteng.",
            "Ketuk TAS untuk menggelar Tikar Pandan 3D. Pilih bahan ke lingkaran tengah lalu ketuk RAKIT. Jaga Kesehatan, Stamina, Kenyang, Haus, Suhu Tubuh, dan Kewarasan. Masak daging dan rebus air di Api Unggun.",
            "Beri perintah kepada Kang Deden lewat Buku Catatan atau tombol TANGAN (ambil kayu, jaga api, bangun pagar). Bangun kepercayaan Ningsih dengan tidak menyakitinya. Saat malam tiba, nyalakan obor atau berlindung di dekat api."
        };

        public const string TeksKredit =
            "ALAS KERAMAT: SONS OF NUSANTARA\n\n" +
            "Pengarah Permainan & Arsitektur Dunia\nTim Alas Keramat Studio\n\n" +
            "Sistem Prosedural, AI Folklor & Audio Sintetis\nNusantara Engine Pipeline\n\n" +
            "Aset Visual CC0 & Animasi\nPoly Haven, ambientCG, MakeHuman CC0, CMU Mocap, Adobe Mixamo\n\nRekaman Alam, Foley & Musik CC0\nTinyWorlds, Wolfgang_, qubodup / pdsounds.org, Kenney, OpenGameArt (Juhani Junkala dkk.)\n\n" +
            "Ikon Kontrol\ngame-icons.net oleh Lorc, Delapouite, guard13007 (CC BY 3.0)\n\n" +
            "Terima kasih telah bermain dan menjaga Alas Keramat.";

        [SerializeField] GameObject panelMenuUtama, panelJeda, panelPengaturan, panelCara, panelKredit, panelSlot, panelDebug;
        [SerializeField] Text teksInfoPengaturan, teksJudulCara, teksIsiCara, teksKredit, teksSlotInfo, teksDebug;
        [SerializeField] RectTransform[] tombolSkala = new RectTransform[0];

        public PresetGrafis Preset { get; private set; } = PresetGrafis.Sedang;
        public int TargetFps { get; private set; } = 60;
        public float Sensitivitas { get; private set; } = 1f;
        public bool BalikY { get; private set; }
        public float SkalaTombol { get; private set; } = 1f;
        public float VolMaster { get; private set; } = 1f;
        public float VolEfek { get; private set; } = 1f;
        public float VolAmbien { get; private set; } = 1f;
        public float VolMusik { get; private set; } = 1f;
        public float Fov { get; private set; } = 70f;
        public bool HeadBob { get; private set; }
        public bool Getar { get; private set; } = true;
        public bool ModeImersif { get; private set; }
        public Difficulty Kesulitan { get; private set; } = Difficulty.Normal;
        public int IndeksCara { get; private set; }
        public bool Dijeda { get; private set; }

        bool _sesiDimulai;
        static bool _mulaiBaruBerikutnya;
        public bool PermainanDimulai { get { return _sesiDimulai; } }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetPeluncuran() { _mulaiBaruBerikutnya=false; }
        int _ketukDebug;
        float _tKetukDebug, _tSegarDebug, _fpsHalus = 30f;
        Vector2[] _ukuranAwalTombol;
        UniversalRenderPipelineAsset _klonUrp;
        Terrain[] _terrains;

        public void Pasang(
            GameObject menuUtama, GameObject jeda, GameObject pengaturan, GameObject cara, GameObject kredit, GameObject slot, GameObject debug,
            Text infoPengaturan, Text judulCara, Text isiCara, Text isiKredit, Text infoSlot, Text isiDebug, RectTransform[] daftarTombol)
        {
            panelMenuUtama = menuUtama; panelJeda = jeda; panelPengaturan = pengaturan;
            panelCara = cara; panelKredit = kredit; panelSlot = slot; panelDebug = debug;
            teksInfoPengaturan = infoPengaturan; teksJudulCara = judulCara; teksIsiCara = isiCara;
            teksKredit = isiKredit; teksSlotInfo = infoSlot; teksDebug = isiDebug;
            tombolSkala = daftarTombol ?? new RectTransform[0];
        }

        // ---------- Rumus Murni (diuji Self-Test T11) ----------
        public static DataPresetGrafis AmbilPreset(PresetGrafis p)
        {
            switch (p)
            {
                case PresetGrafis.Rendah:
                    return new DataPresetGrafis { renderScale = 0.7f, bayangan = 0f, cascade = 1, msaa = 1, hdr = false, bloom = false, farKamera = 135f, kabutTambah = 0.006f, pengaliVeg = 0.55f, detailVeg = 0.45f, rumput = 0.15f, targetFps = 30 };
                case PresetGrafis.Tinggi:
                    return new DataPresetGrafis { renderScale = 1.0f, bayangan = 60f, cascade = 2, msaa = 1, hdr = true, bloom = true, farKamera = 250f, kabutTambah = 0f, pengaliVeg = 1.15f, detailVeg = 0.9f, rumput = 0.65f, targetFps = 60 };
                case PresetGrafis.Ultra:
                    return new DataPresetGrafis { renderScale = 1.0f, bayangan = 90f, cascade = 4, msaa = 2, hdr = true, bloom = true, farKamera = 360f, kabutTambah = 0f, pengaliVeg = 1.4f, detailVeg = 1.15f, rumput = 1f, targetFps = 30 };
                default:
                    return new DataPresetGrafis { renderScale = 0.95f, bayangan = 35f, cascade = 1, msaa = 1, hdr = true, bloom = false, farKamera = 185f, kabutTambah = 0.002f, pengaliVeg = 0.85f, detailVeg = 0.65f, rumput = 0.35f, targetFps = 30 }; // 30 stabil: 60 yang tak tercapai membuat frame tidak rata (terasa patah-patah)
            }
        }

        public static float PengaliLajuVital(Difficulty d)
        {
            return d == Difficulty.Damai ? 0.7f : d == Difficulty.Sulit ? 1.25f : 1.0f;
        }

        public static float PengaliDamageMusuh(Difficulty d)
        {
            return d == Difficulty.Damai ? 0.5f : d == Difficulty.Sulit ? 1.4f : 1.0f;
        }

        public static bool PerluGetarDamage(float damage, bool getarAktif)
        {
            return getarAktif && damage >= AmbangDamageGetar;
        }

        void Awake()
        {
            GameServices.Register(this);
            CatatUkuranTombol();
            // Build lama menyimpan HeadBob=1 tanpa menyediakan tombol untuk mematikannya.
            if (!PlayerPrefs.HasKey("ak_kontrol_nyaman_v1"))
            { PlayerPrefs.SetInt(PrefHeadBob, 0); PlayerPrefs.SetInt("ak_kontrol_nyaman_v1", 1); }
            MuatPengaturan();
            TerapkanPengaturan();
            SembunyikanSemua();
            JedaDunia.Atur(GetInstanceID(), true);
        }

        IEnumerator Start()
        {
            TerapkanPengaturan();
            yield return null; yield return null;
            while(LayarPeringatan.PerluTampil())yield return null;
            if(_mulaiBaruBerikutnya) { _mulaiBaruBerikutnya=false; _sesiDimulai=true; BeriBekalAwalJikaKosong(); JedaDunia.Atur(GetInstanceID(), false); }
            else if(!_sesiDimulai) BukaMenuUtama();
        }

        public static PresetGrafis RekomendasiPreset(int ramMb, int vramMb)
        {
            if(ramMb>0 && ramMb<6000)return PresetGrafis.Rendah;
            if(ramMb>=12000 && vramMb>=4000)return PresetGrafis.Tinggi;
            return PresetGrafis.Sedang;
        }

        void OnEnable() { GameEvents.OnPlayerDamaged += SaatKenaDamage; }
        void OnDisable() { GameEvents.OnPlayerDamaged -= SaatKenaDamage; JedaDunia.Atur(GetInstanceID(), false); }

        void SaatKenaDamage(DamageInfo info)
        {
            if (PerluGetarDamage(info.amount, Getar)) GetarPerangkat();
        }

        public static void GetarPerangkat()
        {
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }

        void CatatUkuranTombol()
        {
            if (tombolSkala == null) return;
            _ukuranAwalTombol = new Vector2[tombolSkala.Length];
            for (int i = 0; i < tombolSkala.Length; i++)
                if (tombolSkala[i] != null) _ukuranAwalTombol[i] = tombolSkala[i].sizeDelta;
        }

        void SembunyikanSemua()
        {
            if (panelMenuUtama != null) panelMenuUtama.SetActive(false);
            if (panelJeda != null) panelJeda.SetActive(false);
            if (panelPengaturan != null) panelPengaturan.SetActive(false);
            if (panelCara != null) panelCara.SetActive(false);
            if (panelKredit != null) panelKredit.SetActive(false);
            if (panelSlot != null) panelSlot.SetActive(false);
            if (panelDebug != null) panelDebug.SetActive(false);
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (dt > 0.0001f) _fpsHalus = Mathf.Lerp(_fpsHalus, 1f / dt, 0.1f);
            if (_ketukDebug > 0)
            {
                _tKetukDebug -= dt;
                if (_tKetukDebug <= 0f) _ketukDebug = 0;
            }
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Escape)) ToggleJeda();
#endif
            if (panelDebug != null && panelDebug.activeSelf)
            {
                _tSegarDebug -= dt;
                if (_tSegarDebug <= 0f) { _tSegarDebug = 0.25f; SegarkanTeksDebug(); }
            }
        }

        // ---------- Pengaturan (PlayerPrefs + Runtime) ----------
        public void MuatPengaturan()
        {
            Preset = (PresetGrafis)Mathf.Clamp(PlayerPrefs.GetInt(PrefPreset, (int)RekomendasiPreset(SystemInfo.systemMemorySize,SystemInfo.graphicsMemorySize)), 0, 3);
            TargetFps = PlayerPrefs.GetInt(GameBootstrap.PrefFps, AmbilPreset(Preset).targetFps);
            Sensitivitas = Mathf.Clamp(PlayerPrefs.GetFloat(PrefSens, 1f), 0.5f, 2f);
            BalikY = PlayerPrefs.GetInt(PrefBalikY, 0) != 0;
            SkalaTombol = Mathf.Clamp(PlayerPrefs.GetFloat(PrefSkalaTombol, 1f), 0.8f, 1.3f);
            VolMaster = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefVolMaster, 1f));
            VolEfek = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefVolEfek, 1f));
            VolAmbien = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefVolAmbien, 1f));
            VolMusik = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefVolMusik, 1f));
            Fov = Mathf.Clamp(PlayerPrefs.GetFloat(PrefFov, 70f), 60f, 85f);
            HeadBob = PlayerPrefs.GetInt(PrefHeadBob, 0) != 0;
            CombatAndLeftHandController.AimAssist = PlayerPrefs.GetInt("ak_bantuan_bidik", 1) != 0;
            PengunciInput.AturJedaPanel(PlayerPrefs.GetInt(PrefJedaPanel, 1) != 0);
            Goyangan = PlayerPrefs.GetInt(PrefGoyangan, 0) != 0;
            Getar = PlayerPrefs.GetInt(PrefGetar, 1) != 0;
            ModeImersif = PlayerPrefs.GetInt(PrefImersif, 0) != 0;
            Kesulitan = (Difficulty)Mathf.Clamp(PlayerPrefs.GetInt(PrefKesulitan, (int)Difficulty.Normal), 0, 2);
            SaveSystem.SlotAktif = Mathf.Clamp(PlayerPrefs.GetInt(PrefSlot, 1), 1, 3);
            TerrorDirector.Setelan = (PenjadwalTeror.Intensitas)Mathf.Clamp(PlayerPrefs.GetInt(PrefHoror, (int)PenjadwalTeror.Intensitas.Normal), 0, 2);
            TerrorDirector.Headset = PlayerPrefs.GetInt(PrefHeadset, 1) != 0;
        }

        public void SimpanPengaturan()
        {
            PlayerPrefs.SetInt(PrefPreset, (int)Preset);
            PlayerPrefs.SetInt(GameBootstrap.PrefFps, TargetFps);
            PlayerPrefs.SetFloat(PrefSens, Sensitivitas);
            PlayerPrefs.SetInt(PrefBalikY, BalikY ? 1 : 0);
            PlayerPrefs.SetFloat(PrefSkalaTombol, SkalaTombol);
            PlayerPrefs.SetFloat(PrefVolMaster, VolMaster);
            PlayerPrefs.SetFloat(PrefVolEfek, VolEfek);
            PlayerPrefs.SetFloat(PrefVolAmbien, VolAmbien);
            PlayerPrefs.SetFloat(PrefVolMusik, VolMusik);
            PlayerPrefs.SetFloat(PrefFov, Fov);
            PlayerPrefs.SetInt(PrefHeadBob, HeadBob ? 1 : 0);
            PlayerPrefs.SetInt("ak_bantuan_bidik", CombatAndLeftHandController.AimAssist ? 1 : 0);
            PlayerPrefs.SetInt(PrefJedaPanel, PengunciInput.JedaPanel ? 1 : 0);
            PlayerPrefs.SetInt(PrefGoyangan, Goyangan ? 1 : 0);
            PlayerPrefs.SetInt(PrefGetar, Getar ? 1 : 0);
            PlayerPrefs.SetInt(PrefImersif, ModeImersif ? 1 : 0);
            PlayerPrefs.SetInt(PrefKesulitan, (int)Kesulitan);
            PlayerPrefs.SetInt(PrefSlot, SaveSystem.SlotAktif);
            PlayerPrefs.SetInt(PrefHoror, (int)TerrorDirector.Setelan);
            PlayerPrefs.SetInt(PrefHeadset, TerrorDirector.Headset ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void TerapkanPengaturan()
        {
            var dp = AmbilPreset(Preset);
            QualitySettings.vSyncCount = 0;
            QualitySettings.asyncUploadTimeSlice = 2;
            QualitySettings.asyncUploadBufferSize = 16;
            Application.targetFrameRate = TargetFps;
            Time.fixedDeltaTime = 0.025f;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // Klon aset URP saat runtime (11 §7.2) agar aset proyek di disk tidak berubah.
            if (Application.isPlaying)
            {
                var urpAsli = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
                if (urpAsli != null && _klonUrp == null)
                {
                    _klonUrp = Instantiate(urpAsli);
                    _klonUrp.name = urpAsli.name + "_RuntimeClone";
                    QualitySettings.renderPipeline = _klonUrp;
                }
                if (_klonUrp != null)
                {
                    _klonUrp.renderScale = dp.renderScale;
                    _klonUrp.shadowDistance = dp.bayangan;
                    _klonUrp.shadowCascadeCount = dp.cascade;
                    _klonUrp.msaaSampleCount = dp.msaa;
                    _klonUrp.supportsHDR = dp.hdr;
                    bool depthOn = dp.bayangan >= 35f;
                    _klonUrp.supportsCameraDepthTexture = depthOn;
                    Shader.SetGlobalFloat("_AK_MatikanDepth", depthOn ? 0f : 1f);
                    AturSSAO(Preset >= PresetGrafis.Tinggi);
                }
                QualitySettings.skinWeights = Preset <= PresetGrafis.Sedang ? SkinWeights.TwoBones : SkinWeights.FourBones;
            }

            var cam = Camera.main;
            if (cam != null)
            {
                cam.fieldOfView = Fov; AlasKeramat.Player.AndiFPSController.FovDasar = Fov;
                if (Application.isPlaying) cam.farClipPlane = Mathf.Max(dp.farKamera, 135f);
            }

            var veg = GameServices.Get<VegetationManager>();
            if (veg != null) { veg.PengaliJarak = dp.pengaliVeg; veg.PengaliDetail = dp.detailVeg; }
            var serasah=GameServices.Get<SerasahHutan>();
            if(serasah!=null) { serasah.Kepadatan=dp.rumput; serasah.BayanganPadat=Preset>=PresetGrafis.Tinggi; }
            if(Application.isPlaying)
            {
                var volume=UnityEngine.Object.FindFirstObjectByType<Volume>();
                Bloom bloom;
                if(volume!=null && volume.profile!=null && volume.profile.TryGet(out bloom))bloom.active=dp.bloom;
                if(_terrains==null)_terrains=Terrain.activeTerrains;
                float pxErr=Preset==PresetGrafis.Rendah?22f:Preset==PresetGrafis.Sedang?15f:Preset==PresetGrafis.Tinggi?9f:6f;
                float bDist=Preset==PresetGrafis.Rendah?55f:Preset==PresetGrafis.Sedang?90f:Preset==PresetGrafis.Tinggi?130f:180f;
                for(int i=0;i<_terrains.Length;i++) { if(_terrains[i]==null)continue; _terrains[i].heightmapPixelError=pxErr; _terrains[i].basemapDistance=bDist; _terrains[i].drawInstanced=true; _terrains[i].shadowCastingMode=ShadowCastingMode.Off; }
                if(cam!=null)cam.GetUniversalAdditionalCameraData().antialiasing=(Preset>=PresetGrafis.Sedang && dp.msaa<=1)?AntialiasingMode.FastApproximateAntialiasing:AntialiasingMode.None;
            }

            var andi = GameServices.Get<AndiFPSController>();
            if (andi != null)
            {
                andi.PengaliSensitivitas = Sensitivitas;
                andi.BalikY = BalikY;
                andi.PakaiHeadBob = HeadBob;
                andi.GoyanganTempur = Goyangan;
            }

            ProceduralAudioSynth.VolMaster = VolMaster;
            ProceduralAudioSynth.SetVolume(AudioCategory.Efek, VolEfek);
            ProceduralAudioSynth.SetVolume(AudioCategory.Ambien, VolAmbien);
            ProceduralAudioSynth.SetVolume(AudioCategory.Musik, VolMusik);

            EnemyDirector.Kesulitan = Kesulitan;

            if (Application.isPlaying)
            {
                var hudV = GameServices.Get<HUDVital>();
                if (hudV != null) hudV.enabled = !ModeImersif;
            }

            if (tombolSkala != null && _ukuranAwalTombol != null)
            {
                for (int i = 0; i < tombolSkala.Length && i < _ukuranAwalTombol.Length; i++)
                    if (tombolSkala[i] != null && _ukuranAwalTombol[i].sqrMagnitude > 1f)
                        tombolSkala[i].sizeDelta = _ukuranAwalTombol[i] * SkalaTombol;
            }
            SegarkanTeksPengaturan();
            SegarkanPenggeser();
        }

        void SegarkanTeksPengaturan()
        {
            SegarkanPenggeser();
            if (teksInfoPengaturan == null) return;
            teksInfoPengaturan.text =
                "Preset Grafis: " + Preset + "  |  Target FPS: " + TargetFps + "  |  FOV: " + Fov.ToString("0") + "\n" +
                "Sensitivitas: " + (Sensitivitas * 100f).ToString("0") + "%  |  Balik Y: " + (BalikY ? "YA" : "TIDAK") +
                "  |  Ukuran Tombol: " + (SkalaTombol * 100f).ToString("0") + "%\n" +
                "Volume Master: " + (VolMaster * 100f).ToString("0") + "%  |  Headset: " + (TerrorDirector.Headset ? "AKTIF" : "MATI") +
                "  |  Getar: " + (Getar ? "AKTIF" : "MATI") + "  |  Imersif: " + (ModeImersif ? "YA" : "TIDAK") + "\n" +
                "Tingkat Gore: " + GoreSystem.Tingkat + "  |  Intensitas Horor: " + TerrorDirector.Setelan + "  |  Kesulitan: " + Kesulitan;
        }

        // ---------- Aksi Tombol Menu & Jeda ----------
        public void ToggleJeda()
        {
            if(!_sesiDimulai) return;
            var peta=GameServices.Get<PetaSurveiUI>();
            if(peta!=null && peta.Terbuka) { peta.Tutup(); return; }
            var sinema = GameServices.Get<LayarSinema>();
            if (sinema != null && sinema.Berjalan) return;
            Dijeda = !Dijeda;
            SembunyikanSemua();
            if (panelJeda != null) panelJeda.SetActive(Dijeda);
            JedaDunia.Atur(GetInstanceID(), Dijeda);
            BunyiKlik();
        }

        public void LanjutkanDariJeda()
        {
            if(!_sesiDimulai) { TutupMenuUtama(); return; }
            Dijeda = false;
            SembunyikanSemua();
            JedaDunia.Atur(GetInstanceID(), false);
            BunyiKlik();
        }

        public void BukaMenuUtama()
        {
            Dijeda = true;
            JedaDunia.Atur(GetInstanceID(), true);
            SembunyikanSemua();
            if (panelMenuUtama != null) panelMenuUtama.SetActive(true);
            BunyiKlik();
        }

        public void TutupMenuUtama()
        {
            if(!_sesiDimulai)
            {
                var simpan=GameServices.Get<SaveSystem>();
                if(simpan!=null && simpan.AdaSimpanan && !simpan.Muat())return;
                _sesiDimulai=true;
                BeriBekalAwalJikaKosong();
            }
            Dijeda = false;
            SembunyikanSemua();
            JedaDunia.Atur(GetInstanceID(), false);
            BunyiKlik();
        }

        public void MulaiPermainanBaru(int kesulitan)
        {
            Kesulitan = (Difficulty)Mathf.Clamp(kesulitan, 0, 2);
            EnemyDirector.Kesulitan = Kesulitan;
            SimpanPengaturan();
            Dijeda = false;
            SembunyikanSemua();
            JedaDunia.Atur(GetInstanceID(), false);
            _mulaiBaruBerikutnya=true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void BukaPanelSlot()
        {
            SembunyikanSemua();
            if (panelSlot != null) panelSlot.SetActive(true);
            SegarkanInfoSlot();
            BunyiKlik();
        }

        public void TutupPanelSlot()
        {
            if (panelSlot != null) panelSlot.SetActive(false);
            if (Dijeda) { var kembali=_sesiDimulai?panelJeda:panelMenuUtama; if(kembali!=null)kembali.SetActive(true); }
            BunyiKlik();
        }

        public void PilihSlot(int slot)
        {
            SaveSystem.SlotAktif = Mathf.Clamp(slot, 1, 3);
            SimpanPengaturan();
            SegarkanInfoSlot();
            BunyiKlik();
        }

        public void SimpanKeSlotAktif()
        {
            if(!_sesiDimulai) { GameEvents.RaiseToast("Mulai atau muat permainan sebelum menyimpan"); return; }
            var sv = GameServices.Get<SaveSystem>();
            if (sv != null) sv.SimpanSlot(SaveSystem.SlotAktif);
            SegarkanInfoSlot();
            BunyiKlik();
        }

        public void MuatDariSlotAktif()
        {
            var sv = GameServices.Get<SaveSystem>();
            if (sv != null && sv.MuatSlot(SaveSystem.SlotAktif))
            {
                _sesiDimulai=true;
                Dijeda = false;
                SembunyikanSemua();
                JedaDunia.Atur(GetInstanceID(), false);
            }
            SegarkanInfoSlot();
            BunyiKlik();
        }

        public void HapusSlotAktif()
        {
            SaveSystem.HapusSlot(SaveSystem.SlotAktif);
            SegarkanInfoSlot();
            BunyiKlik();
        }

        void SegarkanInfoSlot()
        {
            if (teksSlotInfo == null) return;
            string s = "SLOT AKTIF: SLOT " + SaveSystem.SlotAktif + "\n\n";
            for (int i = 1; i <= 3; i++)
                s += "Slot " + i + ": " + SaveSystem.RingkasanSlot(i) + (i == SaveSystem.SlotAktif ? "  [DIPILIH]" : "") + "\n";
            teksSlotInfo.text = s;
        }

        public void BukaPengaturan()
        {
            SembunyikanSemua();
            if (panelPengaturan != null) panelPengaturan.SetActive(true);
            SegarkanTeksPengaturan();
            BunyiKlik();
        }

        public void TutupPengaturan()
        {
            SimpanPengaturan();
            TerapkanPengaturan();
            if (panelPengaturan != null) panelPengaturan.SetActive(false);
            if (Dijeda) { var kembali=_sesiDimulai?panelJeda:panelMenuUtama; if(kembali!=null)kembali.SetActive(true); }
            BunyiKlik();
        }

        public void SiklusPreset()
        {
            Preset = (PresetGrafis)(((int)Preset + 1) % 4);
            TargetFps = AmbilPreset(Preset).targetFps;
            TerapkanPengaturan();
            SimpanPengaturan();
            BunyiKlik();
        }

        public void SetPreset(int p)
        {
            Preset = (PresetGrafis)Mathf.Clamp(p, 0, 3);
            TargetFps = AmbilPreset(Preset).targetFps;
            TerapkanPengaturan();
            SimpanPengaturan();
        }

        public void ToggleTargetFps()
        {
            TargetFps = TargetFps == 30 ? 60 : 30;
            TerapkanPengaturan();
            SimpanPengaturan();
            BunyiKlik();
        }

        public void SiklusSensitivitas()
        {
            Sensitivitas = Sensitivitas < 0.9f ? 1.0f : Sensitivitas < 1.3f ? 1.5f : Sensitivitas < 1.8f ? 2.0f : 0.6f;
            TerapkanPengaturan();
            SimpanPengaturan();
            BunyiKlik();
        }

        public void ToggleBalikY()
        {
            BalikY = !BalikY;
            TerapkanPengaturan();
            SimpanPengaturan();
            BunyiKlik();
        }

        public void SiklusSkalaTombol()
        {
            SkalaTombol = SkalaTombol < 0.95f ? 1.0f : SkalaTombol < 1.1f ? 1.15f : SkalaTombol < 1.25f ? 1.3f : 0.85f;
            TerapkanPengaturan();
            SimpanPengaturan();
            BunyiKlik();
        }

        public void SiklusVolume()
        {
            VolMaster = VolMaster > 0.9f ? 0.7f : VolMaster > 0.6f ? 0.4f : VolMaster > 0.2f ? 0f : 1f;
            TerapkanPengaturan();
            SimpanPengaturan();
            BunyiKlik();
        }

        public void SiklusVolumeKategori(int kategori)
        {
            if(kategori<0 || kategori>2)return;
            float v=kategori==0?VolEfek:kategori==1?VolAmbien:VolMusik;
            v=v>0.9f?0.7f:v>0.6f?0.4f:v>0.2f?0f:1f;
            if(kategori==0)VolEfek=v;else if(kategori==1)VolAmbien=v;else VolMusik=v;
            TerapkanPengaturan();SimpanPengaturan();
            GameEvents.RaiseToast("VOLUME "+((AudioCategory)kategori).ToString().ToUpperInvariant()+" "+(v*100f).ToString("0")+"%");
        }

        public void SiklusFov()
        {
            Fov = Fov < 68f ? 70f : Fov < 74f ? 78f : Fov < 82f ? 85f : 62f;
            TerapkanPengaturan();
            SimpanPengaturan();
            BunyiKlik();
        }

        public void SiklusGore()
        {
            GoreSystem.Tingkat = (GoreLevel)(((int)GoreSystem.Tingkat + 1) % 3);
            SegarkanTeksPengaturan();
            BunyiKlik();
        }

        public void SiklusHoror()
        {
            TerrorDirector.Setelan = (PenjadwalTeror.Intensitas)(((int)TerrorDirector.Setelan + 1) % 3);
            SimpanPengaturan();
            SegarkanTeksPengaturan();
            BunyiKlik();
        }

        public void ToggleHeadset()
        {
            TerrorDirector.Headset = !TerrorDirector.Headset;
            SimpanPengaturan();
            SegarkanTeksPengaturan();
            BunyiKlik();
        }

        public void ToggleGetar()
        {
            Getar = !Getar;
            SimpanPengaturan();
            SegarkanTeksPengaturan();
            BunyiKlik();
        }

        public void ToggleImersif()
        {
            ModeImersif = !ModeImersif;
            TerapkanPengaturan();
            SimpanPengaturan();
            BunyiKlik();
        }

        public void SiklusKesulitan()
        {
            Kesulitan = (Difficulty)(((int)Kesulitan + 1) % 3);
            TerapkanPengaturan();
            SimpanPengaturan();
            BunyiKlik();
        }

        public void SetKesulitan(int k)
        {
            Kesulitan = (Difficulty)Mathf.Clamp(k, 0, 2);
            TerapkanPengaturan();
            SimpanPengaturan();
        }

        // ---------- Cara Bermain & Kredit ----------
        public void BukaCaraBermain()
        {
            SembunyikanSemua();
            IndeksCara = 0;
            if (panelCara != null) panelCara.SetActive(true);
            SegarkanKartuCara();
            BunyiKlik();
        }

        public void TutupCaraBermain()
        {
            if (panelCara != null) panelCara.SetActive(false);
            if (Dijeda) { var kembali=_sesiDimulai?panelJeda:panelMenuUtama; if(kembali!=null)kembali.SetActive(true); }
            BunyiKlik();
        }

        public void KartuCaraBerikut()
        {
            IndeksCara = (IndeksCara + 1) % JudulCara.Length;
            SegarkanKartuCara();
            BunyiKlik();
        }

        public void KartuCaraSebelum()
        {
            IndeksCara = (IndeksCara - 1 + JudulCara.Length) % JudulCara.Length;
            SegarkanKartuCara();
            BunyiKlik();
        }

        void SegarkanKartuCara()
        {
            if (teksJudulCara != null) teksJudulCara.text = JudulCara[IndeksCara] + " (" + (IndeksCara + 1) + "/" + JudulCara.Length + ")";
            if (teksIsiCara != null) teksIsiCara.text = IsiCara[IndeksCara];
        }

        public void BukaKredit()
        {
            SembunyikanSemua();
            if (panelKredit != null) panelKredit.SetActive(true);
            if (teksKredit != null) teksKredit.text = TeksKredit;
            BunyiKlik();
        }

        public void TutupKredit()
        {
            if (panelKredit != null) panelKredit.SetActive(false);
            if (Dijeda) { var kembali=_sesiDimulai?panelJeda:panelMenuUtama; if(kembali!=null)kembali.SetActive(true); }
            BunyiKlik();
        }

        public void KeluarAplikasi()
        {
            SimpanPengaturan();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ---------- Panel Debug HP (Ketuk Pojok Kiri-Atas 5x) ----------
        public void KetukPojokDebug()
        {
            _tKetukDebug = JendelaKetukDebug;
            _ketukDebug++;
            if (_ketukDebug >= KetukDebugWajib)
            {
                _ketukDebug = 0;
                if (panelDebug != null)
                {
                    bool buka = !panelDebug.activeSelf;
                    panelDebug.SetActive(buka);
                    if (buka) SegarkanTeksDebug();
                }
            }
        }

        public void SalinLaporanHp() { var d=GameServices.Get<DiagnostikMain>();if(d!=null)d.SalinLaporan(); }

        public void TutupDebug() { if (panelDebug != null) panelDebug.SetActive(false); }

        public void DebugIsiVital()
        {
            var v = GameServices.Get<AndiVitalsAndSanity>();
            if (v != null) v.Pulihkan(12f, true);
            GameEvents.RaiseToast("Debug: Vital dipulihkan");
            SegarkanTeksDebug();
        }

        public void DebugMajuJam()
        {
            var j = GameServices.Get<TimeOfDayClock>();
            if (j != null) j.Maju(3f);
            GameEvents.RaiseToast("Debug: Waktu +3 jam");
            SegarkanTeksDebug();
        }

        public void DebugBeriItem()
        {
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv != null)
            {
                inv.Tambah(ItemID.GolokPusaka, 1); inv.Tambah(ItemID.KapakBaja, 1); inv.Tambah(ItemID.BusurBambu, 1);
                inv.Tambah(ItemID.AnakPanah, 20); inv.Tambah(ItemID.OborBambu, 1); inv.Tambah(ItemID.KorekGas, 1);
                inv.Tambah(ItemID.RansumDarurat, 5); inv.Tambah(ItemID.BotolAirBersih, 5); inv.Tambah(ItemID.JamuHerbal, 5);
            }
            GameEvents.RaiseToast("Debug: Paket survival diberikan");
            SegarkanTeksDebug();
        }

        public void DebugTeleportBeringin() { TeleportAndi(new Vector3(0f, 0f, -24f)); }
        public void DebugTeleportDesa() { TeleportAndi(new Vector3(520f, 0f, 196f)); }

        public void DebugCuacaBadai()
        {
            var a = GameServices.Get<URPAtmosphereAndWeather>();
            if (a != null) a.SetCuaca(a.Cuaca == WeatherState.HujanBadai ? WeatherState.Cerah : WeatherState.HujanBadai, true);
            SegarkanTeksDebug();
        }

        static void TeleportAndi(Vector3 pos)
        {
            var andi = GameServices.Get<AndiFPSController>();
            if (andi == null) return;
            var cc = andi.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            float y = Mathf.Max(0f, WorldTerrainGenerator.SampleHeight(pos.x, pos.z)) + 0.5f;
            andi.transform.position = new Vector3(pos.x, y, pos.z);
            if (cc != null) cc.enabled = true;
            GameEvents.RaiseToast("Debug: Teleport ke (" + pos.x.ToString("0") + ", " + pos.z.ToString("0") + ")");
        }

        void SegarkanTeksDebug()
        {
            if (teksDebug == null) return;
            var andi = GameServices.Get<AndiFPSController>();
            var jam = GameServices.Get<TimeOfDayClock>();
            var cuaca = GameServices.Get<URPAtmosphereAndWeather>();
            var dir = GameServices.Get<EnemyDirector>();
            var diag = GameServices.Get<NusantaraDiagnostics>();
            Vector3 p = andi != null ? andi.transform.position : Vector3.zero;
            string s = "FPS: " + _fpsHalus.ToString("0.0") + " (Target " + TargetFps + ", Preset " + Preset + ")\n" +
                       "Posisi: (" + p.x.ToString("0") + ", " + p.y.ToString("0.0") + ", " + p.z.ToString("0") + ") | Bioma: " + WorldTerrainGenerator.GetBiome(p.x, p.z) + "\n" +
                       "Waktu: Hari " + (jam != null ? jam.Hari : 1) + " Jam " + (jam != null ? jam.Jam.ToString("0.00") : "0") +
                       " | Cuaca: " + (cuaca != null ? cuaca.Cuaca.ToString() : "-") + "\n" +
                       "Musuh Aktif: " + (dir != null ? dir.Aktif : 0) + " | Kemarahan: " + (dir != null ? dir.Kemarahan.ToString("0") : "0") +
                       " | Kesulitan: " + EnemyDirector.Kesulitan;
            if (diag != null)
            {
                int n = 0;
                foreach (var b in diag.EntriTerakhir) { if (n == 0) s += "\nLog: "; if (n++ < 2) s += b + " "; }
            }
            teksDebug.text = s;
            var diagnostik=GameServices.Get<DiagnostikMain>();
            if(diagnostik!=null)teksDebug.text+="\n"+diagnostik.Ringkas;
        }

        static void AturSSAO(bool aktif)
        {
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp == null) return;
            var f = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var list = f != null ? f.GetValue(urp) as ScriptableRendererData[] : null;
            if (list == null) return;
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == null || list[i].rendererFeatures == null) continue;
                foreach (var feat in list[i].rendererFeatures)
                    if (feat != null && feat.GetType().Name.Contains("ScreenSpaceAmbientOcclusion"))
                        feat.SetActive(aktif);
            }
        }

        static void BunyiKlik()
        {
            var a = GameServices.Get<ProceduralAudioSynth>();
            if (a != null) a.PlayUIClick();
        }
    }
}
