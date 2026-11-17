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

        AndiFPSController _andi;
        bool _adaJoy;

        public void Pasang(ZonaJoystick j, ZonaGeser g) { joystick = j; geser = g; }

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
        }

        // Sensitivitas 09 §3 dinyatakan per piksel referensi 1080p.
        static float SkalaLayar() { return Screen.height > 0 ? 1080f / Screen.height : 1f; }

        public void TekanLompat() { if (_andi != null) _andi.Lompat(); }
        public void TekanJongkok() { if (_andi != null) _andi.ToggleJongkok(); }
    }
}
