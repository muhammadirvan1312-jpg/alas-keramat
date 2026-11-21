using UnityEngine;

namespace AlasKeramat.Core
{
    // Suar mercusuar berputar di malam hari, badai, atau kabut tebal untuk orientasi laut dan pantai selatan.
    public class SuarMercusuar : MonoBehaviour
    {
        public const float KecepatanPutar = 28f;
        [SerializeField] Light lampuSuar;
        [SerializeField] Renderer berkasSuar;

        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _atmos;

        public void Pasang(Light l, Renderer r) { lampuSuar = l; berkasSuar = r; }

        public static bool AktifPadaJam(float jam, WeatherState w)
        {
            return jam >= 17.4f || jam <= 6.2f || w == WeatherState.HujanBadai || w == WeatherState.KabutPagi || w == WeatherState.KabutDarah;
        }

        void Update()
        {
            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            if (_atmos == null) _atmos = GameServices.Get<URPAtmosphereAndWeather>();
            float jam = _jam != null ? _jam.Jam : TimeOfDayClock.JamMulai;
            var cuaca = _atmos != null ? _atmos.Cuaca : WeatherState.Cerah;
            bool nyala = AktifPadaJam(jam, cuaca);
            if (lampuSuar != null && lampuSuar.enabled != nyala) lampuSuar.enabled = nyala;
            if (berkasSuar != null && berkasSuar.enabled != nyala) berkasSuar.enabled = nyala;
            if (nyala) transform.Rotate(0f, KecepatanPutar * Time.deltaTime, 0f, Space.Self);
        }
    }
}
