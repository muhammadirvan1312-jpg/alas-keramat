using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Penampung air hujan bambu khas survival SOTF: terisi otomatis (4x lebih cepat saat hujan), air bersih siap diminum.
    public class PenampungAirHujan : MonoBehaviour, IInteractable, ISaveable
    {
        public const float LajuEmbun = 0.004f, LajuHujan = 0.022f, HidrasiMinum = 38f;

        [SerializeField] Transform permukaanAir;
        float _isi01 = 0.45f;
        URPAtmosphereAndWeather _cuaca;

        public float Isi01 { get { return _isi01; } }
        public float HoldDuration { get { return 0f; } }
        public string SaveKey { get { return "air_hujan_" + Mathf.RoundToInt(transform.position.x * 10f) + "_" + Mathf.RoundToInt(transform.position.z * 10f); } }

        public void Pasang(Transform air) { permukaanAir = air; Segarkan(); }

        public static float LajuIsi(WeatherState w)
        {
            if (w == WeatherState.HujanBadai) return LajuHujan * 1.4f;
            if (w == WeatherState.Gerimis) return LajuHujan;
            if (w == WeatherState.KabutPagi) return LajuEmbun * 2f;
            return LajuEmbun;
        }

        public string GetPromptText(GameObject actor)
        {
            return _isi01 >= 0.25f ? "MINUM AIR BERSIH (" + Mathf.RoundToInt(_isi01 * 100f) + "%)" : "PENAMPUNG AIR KOSONG";
        }

        public bool CanInteract(GameObject actor) { return _isi01 >= 0.25f; }

        public void Interact(GameObject actor)
        {
            if (_isi01 < 0.25f) return;
            var vital = GameServices.Get<AndiVitalsAndSanity>();
            if (vital == null) return;
            vital.Minum(HidrasiMinum, false);
            _isi01 = Mathf.Max(0f, _isi01 - 0.35f);
            Segarkan();
            GameEvents.RaiseToast("Minum air hujan bersih (+38 hidrasi).");
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.PlayItemPickup();
        }

        void Update()
        {
            if (_isi01 >= 1f || JedaDunia.Dijeda) return;
            if (_cuaca == null) _cuaca = GameServices.Get<URPAtmosphereAndWeather>();
            var w = _cuaca != null ? _cuaca.Cuaca : WeatherState.Cerah;
            _isi01 = Mathf.Min(1f, _isi01 + Time.deltaTime * LajuIsi(w));
            Segarkan();
        }

        void Segarkan()
        {
            if (permukaanAir == null) return;
            permukaanAir.gameObject.SetActive(_isi01 > 0.05f);
            var lp = permukaanAir.localPosition;
            lp.y = Mathf.Lerp(0.18f, 0.56f, _isi01);
            permukaanAir.localPosition = lp;
        }

        [System.Serializable] struct DataAir { public float a; }
        public string CaptureJson() { return JsonUtility.ToJson(new DataAir { a = _isi01 }); }
        public void RestoreJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            _isi01 = Mathf.Clamp01(JsonUtility.FromJson<DataAir>(json).a);
            Segarkan();
        }
    }
}
