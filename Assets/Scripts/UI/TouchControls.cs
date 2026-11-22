using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.UI
{
    // Penghubung kontrol sentuh -> AndiFPSController. Tombol memanggil method publik di sini (listener persisten dari Builder).
    [DefaultExecutionOrder(100)]
    public class TouchControls : MonoBehaviour
    {
        [SerializeField] ZonaJoystick joystick;
        [SerializeField] ZonaGeser geser;
        [SerializeField] UnityEngine.UI.Image tombolTangan;
        [SerializeField] UnityEngine.UI.Text labelTangan;
        [SerializeField] GameObject tombolObor;
        [SerializeField] GameObject tombolLempar, tombolMode;
        [SerializeField] UnityEngine.UI.Image progresTangan;
        [SerializeField] UnityEngine.UI.Image crosshairDot;
        [SerializeField] GameObject promptBanner;
        [SerializeField] UnityEngine.UI.Text promptBannerTeks;
        public void PasangProgres(UnityEngine.UI.Image cincin) { progresTangan = cincin; }
        public void PasangCrosshair(UnityEngine.UI.Image dot, GameObject banner, UnityEngine.UI.Text teksBanner) { crosshairDot = dot; promptBanner = banner; promptBannerTeks = teksBanner; }
        AlasKeramat.Building.ShoulderLogAndFreeformBuilder _pembangun;
        public const float AlphaTanganRedup = 0.35f, AlphaTanganAktif = 0.95f, DenyutTangan = 3f;

        InteraksiAndi _interaksi;
        TanganKiri _kiri;
        string _promptTampil;
        float _tObor;

        AndiFPSController _andi;
        bool _adaJoy;
        TombolTahan _tombolSerang;
        float _tKenaCrosshair;
        bool _kenaBerat;
        CombatAndLeftHandController _tempur;

        public static float SkalaCrosshair(bool adaTargetInteraksi, float tarikBusur01, float sisaKena)
        {
            if (sisaKena > 0f) return 1.85f;
            if (tarikBusur01 > 0.01f) return Mathf.Lerp(1.35f, 0.65f, tarikBusur01);
            return adaTargetInteraksi ? 1.45f : 1.0f;
        }

        void SaatKenaMelee(bool berat) { _tKenaCrosshair = 0.18f; _kenaBerat = berat; }
        void SaatKenaJarak() { _tKenaCrosshair = 0.18f; _kenaBerat = false; }

        void OnEnable()
        {
            CombatAndLeftHandController.OnKena += SaatKenaMelee;
            CombatAndLeftHandController.OnKenaJarak += SaatKenaJarak;
        }

        public void Pasang(ZonaJoystick j, ZonaGeser g) { joystick = j; geser = g; }
        public void PasangPanggul(GameObject lempar, GameObject mode) { tombolLempar = lempar; tombolMode = mode; }
        public void PasangTangan(UnityEngine.UI.Image tangan, UnityEngine.UI.Text label, GameObject obor) { tombolTangan = tangan; labelTangan = label; tombolObor = obor; }

        void Awake()
        {
            GameServices.Register(this);
            if (joystick == null) joystick = GetComponentInChildren<ZonaJoystick>(true);
            if (geser == null) geser = GetComponentInChildren<ZonaGeser>(true);
            if (joystick == null || geser == null) Debug.LogError("[AK][TouchControls] joystick/area geser tidak ditemukan, kontrol sentuh nonaktif.");
        }

        void Update()
        {
            if (_andi == null) { _andi = GameServices.Get<AndiFPSController>(); if (_andi == null) return; }
            if (PengunciInput.Terblokir) { HentikanInput(); return; }
            if (joystick != null)
            {
                bool aktif = joystick.Nilai.sqrMagnitude > 0.0001f;
                if (aktif || _adaJoy) { _andi.InputGerak = joystick.Nilai; _andi.MintaLari = joystick.Lari; }
                _adaJoy = aktif;
            }
            if (geser != null) _andi.LihatManual(geser.AmbilDelta() * SkalaLayar());
            PerbaruiTangan();
            PerbaruiLabelTempur();
        }

        public void HentikanInput()
        {
            if (joystick != null) joystick.ResetSentuh();
            if (geser != null) geser.ResetSentuh();
            if(_tombolSerang==null)_tombolSerang=GetComponentInChildren<TombolTahan>(true);
            if(_tombolSerang!=null)_tombolSerang.ResetSentuh();
            if (_andi != null) _andi.HentikanInputGerak();
            var tempur=GameServices.Get<CombatAndLeftHandController>();
            if(tempur!=null)tempur.HentikanAksiPanel();
            _adaJoy = false;
        }
        void OnDisable()
        {
            CombatAndLeftHandController.OnKena -= SaatKenaMelee;
            CombatAndLeftHandController.OnKenaJarak -= SaatKenaJarak;
            HentikanInput();
        }
        void OnApplicationFocus(bool fokus) { if (!fokus) HentikanInput(); }

        void PerbaruiTangan()
        {
            if (_tKenaCrosshair > 0f) _tKenaCrosshair = Mathf.Max(0f, _tKenaCrosshair - Time.unscaledDeltaTime);
            if (_tempur == null) _tempur = GameServices.Get<CombatAndLeftHandController>();
            if (_interaksi == null) _interaksi = GameServices.Get<InteraksiAndi>();
            if (_kiri == null) _kiri = GameServices.Get<TanganKiri>();
            if (_interaksi != null && labelTangan != null && _interaksi.Prompt != _promptTampil)
            {
                _promptTampil = _interaksi.Prompt;
                labelTangan.text = _promptTampil;
                if (promptBannerTeks != null) promptBannerTeks.text = _promptTampil;
            }
            bool ada = _interaksi != null && _interaksi.AdaTarget;
            if (promptBanner != null && promptBanner.activeSelf != ada) promptBanner.SetActive(ada);
            if (crosshairDot != null)
            {
                float tarik01 = _tempur != null ? _tempur.TarikBusurAktif01 : 0f;
                crosshairDot.color = _tKenaCrosshair > 0f
                    ? (_kenaBerat ? new Color(1f, 0.28f, 0.22f, 1f) : new Color(1f, 0.86f, 0.35f, 1f))
                    : (ada ? new Color(0.95f, 0.76f, 0.31f, 0.98f) : new Color(0.96f, 0.95f, 0.92f, 0.78f));
                crosshairDot.rectTransform.localScale = Vector3.one * SkalaCrosshair(ada, tarik01, _tKenaCrosshair);
            }
            if (tombolTangan != null)
            {
                if (tombolTangan.gameObject.activeSelf != ada) tombolTangan.gameObject.SetActive(ada);
                var c = tombolTangan.color;
                c.a = ada ? AlphaTanganAktif - 0.15f * (0.5f + 0.5f * Mathf.Sin(Time.time * DenyutTangan * Mathf.PI * 2f)) : AlphaTanganRedup;
                tombolTangan.color = c;
            }
            if (progresTangan != null)
            {
                float p = _interaksi != null ? _interaksi.ProgresTahan : 0f;
                progresTangan.enabled = p > 0f; progresTangan.fillAmount = p;
            }
            if (_pembangun == null) _pembangun = GameServices.Get<AlasKeramat.Building.ShoulderLogAndFreeformBuilder>();
            bool bawa = _pembangun != null && _pembangun.Jumlah > 0;
            if (tombolLempar != null && tombolLempar.activeSelf != bawa) tombolLempar.SetActive(bawa);
            bool gel = _pembangun != null && (bawa || _pembangun.ModeCelah || (_interaksi != null && _interaksi.MelihatStruktur));
            if (tombolMode != null && tombolMode.activeSelf != gel) tombolMode.SetActive(gel);
            _tObor += Time.deltaTime;
            if (tombolObor != null && _kiri != null && _tObor > 0.5f)
            {
                _tObor = 0f;
                bool bisa = _kiri.BisaObor();
                if (tombolObor.activeSelf != bisa) tombolObor.SetActive(bisa);
            }
        }

        // Sensitivitas 09 §3 dinyatakan per piksel referensi 1080p.
        static float SkalaLayar() { return Screen.height > 0 ? 1080f / Screen.height : 1f; }

        public void GeserDariTombol(Vector2 delta) { if(_andi!=null && !PengunciInput.Terblokir)_andi.LihatManual(delta*SkalaLayar()); }
        public void TekanLompat()
        { if(PengunciInput.Terblokir)return;
            var c = GameServices.Get<CombatAndLeftHandController>();
            if (c != null && c.BisaElak()) { c.Elak(joystick != null ? joystick.Nilai : Vector2.zero); return; } // 17 §2.2: LOMPAT jadi ELAK
            if (_andi != null) _andi.Lompat();
        }
        public void TekanGanti() { if(PengunciInput.Terblokir)return; var c = GameServices.Get<CombatAndLeftHandController>(); if (c != null) c.Ganti(); }

        UnityEngine.UI.Text _labelLompat, _labelGanti;
        float _tLabelTempur;
        string _isiLompat, _isiGanti;

        void PerbaruiLabelTempur()
        {
            _tLabelTempur -= Time.unscaledDeltaTime;
            if (_tLabelTempur > 0f) return;
            _tLabelTempur = 0.25f;
            if (_labelLompat == null || _labelGanti == null)
                foreach (var t in GetComponentsInChildren<UnityEngine.UI.Text>(true))
                {
                    if (t.transform.parent != null && t.transform.parent.name == "TombolLompat") _labelLompat = t;
                    if (t.transform.parent != null && t.transform.parent.name == "TombolGanti") _labelGanti = t;
                }
            var c = GameServices.Get<CombatAndLeftHandController>();
            if (c == null) return;
            string l = c.BisaElak() ? "ELAK" : "LOMPAT";
            if (_labelLompat != null && l != _isiLompat) { _isiLompat = l; _labelLompat.text = l; }
            int am = c.Amunisi();
            string g = c.NamaAktif() + (am >= 0 ? " (" + am + ")" : "");
            if (_labelGanti != null && g != _isiGanti) { _isiGanti = g; _labelGanti.text = g; }
        }
        public void TekanJongkok() { if(PengunciInput.Terblokir)return; if (_andi != null) _andi.ToggleJongkok(); }
        public void TekanTangan() { if(PengunciInput.Terblokir)return; if (_interaksi == null) _interaksi = GameServices.Get<InteraksiAndi>(); if (_interaksi != null) _interaksi.Tekan(); }
        public void TekanLempar() { if(PengunciInput.Terblokir)return; var b = GameServices.Get<AlasKeramat.Building.ShoulderLogAndFreeformBuilder>(); if (b != null) b.Lempar(); }
        public void TekanModeBangun() { if(PengunciInput.Terblokir)return; var b = GameServices.Get<AlasKeramat.Building.ShoulderLogAndFreeformBuilder>(); if (b != null) b.GantiMode(); }
        public void TekanObor() { if(PengunciInput.Terblokir)return; if (_kiri == null) _kiri = GameServices.Get<TanganKiri>(); if (_kiri != null) _kiri.ToggleObor(); }
        public void TekanSenter()
        {
            if (PengunciInput.Terblokir) return;
            var s = GameServices.Get<SenterAndi>();
            if (s != null) { s.ToggleSenter(); GameEvents.RaiseToast("Senter dialihkan."); }
        }
        public void TekanAutoLari()
        {
            if (PengunciInput.Terblokir || joystick == null) return;
            joystick.ToggleKunciLari();
        }
        public void TekanObatCepat()
        {
            if (PengunciInput.Terblokir) return;
            var inv = GameServices.Get<AlasKeramat.Inventory.TikarPandan3DInventory>();
            var vital = GameServices.Get<AndiVitalsAndSanity>();
            if (inv == null || vital == null) return;
            if (inv.Kurangi(ItemID.PerbanKain, 1)) { vital.ObatiLukaTubuh(); vital.UbahSanity(4f); GameEvents.RaiseToast("Memakai Perban Kain (luka pulih)."); }
            else if (inv.Kurangi(ItemID.JamuHerbal, 1)) { vital.ObatiLukaTubuh(); vital.UbahSanity(12f); GameEvents.RaiseToast("Minum Jamu Herbal (+kewarasan & pulih)."); }
            else if (inv.Kurangi(ItemID.JamuEnergi, 1)) { vital.UbahSanity(10f); GameEvents.RaiseToast("Minum Jamu Energi!"); }
            else GameEvents.RaiseToast("Tidak ada Perban atau Jamu di tas.");
        }
    }
}
