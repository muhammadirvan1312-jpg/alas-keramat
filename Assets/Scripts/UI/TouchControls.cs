using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.UI
{
    // Penghubung kontrol sentuh -> AndiFPSController. Tombol memanggil method publik di sini (listener persisten dari Builder).
    public class TouchControls : MonoBehaviour
    {
        [SerializeField] ZonaJoystick joystick;
        [SerializeField] ZonaGeser geser;
        [SerializeField] UnityEngine.UI.Image tombolTangan;
        [SerializeField] UnityEngine.UI.Text labelTangan;
        [SerializeField] GameObject tombolObor;
        [SerializeField] GameObject tombolLempar, tombolMode;
        AlasKeramat.Building.ShoulderLogAndFreeformBuilder _pembangun;
        public const float AlphaTanganRedup = 0.35f, AlphaTanganAktif = 0.95f, DenyutTangan = 3f;

        InteraksiAndi _interaksi;
        TanganKiri _kiri;
        string _promptTampil;
        float _tObor;

        AndiFPSController _andi;
        bool _adaJoy;

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
            if (joystick != null)
            {
                bool aktif = joystick.Nilai.sqrMagnitude > 0.0001f;
                if (aktif || _adaJoy) { _andi.InputGerak = joystick.Nilai; _andi.MintaLari = joystick.Lari; }
                _adaJoy = aktif;
            }
            if (geser != null) _andi.InputLihatPx += geser.AmbilDelta() * SkalaLayar();
            PerbaruiTangan();
            PerbaruiLabelTempur();
        }

        void PerbaruiTangan()
        {
            if (_interaksi == null) _interaksi = GameServices.Get<InteraksiAndi>();
            if (_kiri == null) _kiri = GameServices.Get<TanganKiri>();
            if (_interaksi != null && labelTangan != null && _interaksi.Prompt != _promptTampil)
            {
                _promptTampil = _interaksi.Prompt;
                labelTangan.text = _promptTampil;
            }
            if (tombolTangan != null)
            {
                bool ada = _interaksi != null && _interaksi.AdaTarget;
                var c = tombolTangan.color;
                c.a = ada ? AlphaTanganAktif - 0.15f * (0.5f + 0.5f * Mathf.Sin(Time.time * DenyutTangan * Mathf.PI * 2f)) : AlphaTanganRedup;
                tombolTangan.color = c;
            }
            if (_pembangun == null) _pembangun = GameServices.Get<AlasKeramat.Building.ShoulderLogAndFreeformBuilder>();
            bool bawa = _pembangun != null && _pembangun.Jumlah > 0;
            if (tombolLempar != null && tombolLempar.activeSelf != bawa) tombolLempar.SetActive(bawa);
            bool gel = _pembangun != null;
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

        public void TekanLompat()
        {
            var c = GameServices.Get<CombatAndLeftHandController>();
            if (c != null && c.BisaElak()) { c.Elak(joystick != null ? joystick.Nilai : Vector2.zero); return; } // 17 §2.2: LOMPAT jadi ELAK
            if (_andi != null) _andi.Lompat();
        }
        public void TekanGanti() { var c = GameServices.Get<CombatAndLeftHandController>(); if (c != null) c.Ganti(); }

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
        public void TekanJongkok() { if (_andi != null) _andi.ToggleJongkok(); }
        public void TekanTangan() { if (_interaksi == null) _interaksi = GameServices.Get<InteraksiAndi>(); if (_interaksi != null) _interaksi.Tekan(); }
        public void TekanLempar() { var b = GameServices.Get<AlasKeramat.Building.ShoulderLogAndFreeformBuilder>(); if (b != null) b.Lempar(); }
        public void TekanModeBangun() { var b = GameServices.Get<AlasKeramat.Building.ShoulderLogAndFreeformBuilder>(); if (b != null) b.GantiMode(); }
        public void TekanObor() { if (_kiri == null) _kiri = GameServices.Get<TanganKiri>(); if (_kiri != null) _kiri.ToggleObor(); }
    }
}
