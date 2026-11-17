using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // 06 §8 Banaspati: potongan kayu yang tersambar terbakar -5 HP/s selama 20 s; hujan memadamkan. Cahaya oranye berkedip.
    public class ApiStruktur : MonoBehaviour
    {
        public const float DamagePerDetik = 5f, Lama = 20f;

        float _sisa, _tTick;
        Light _lampu;
        IDamageable _target;
        URPAtmosphereAndWeather _cuaca;

        public static void Nyalakan(GameObject g)
        {
            if (g == null) return;
            var a = g.GetComponent<ApiStruktur>();
            if (a == null) a = g.AddComponent<ApiStruktur>();
            a._sisa = Lama;
        }

        void Awake()
        {
            _target = GetComponent<IDamageable>();
            var go = new GameObject("ApiStruktur_Lampu");
            go.transform.SetParent(transform, false);
            _lampu = go.AddComponent<Light>();
            _lampu.type = LightType.Point; _lampu.range = 5f; _lampu.color = new Color(1f, 0.55f, 0.2f); _lampu.shadows = LightShadows.None;
        }

        void Update()
        {
            if (_cuaca == null) _cuaca = GameServices.Get<URPAtmosphereAndWeather>();
            bool hujan = _cuaca != null && (_cuaca.Cuaca == WeatherState.Gerimis || _cuaca.Cuaca == WeatherState.HujanBadai);
            _sisa -= Time.deltaTime;
            if (_sisa <= 0f || hujan || _target == null || !_target.IsAlive) { Destroy(_lampu.gameObject); Destroy(this); return; }
            _lampu.intensity = 1.6f + 0.8f * Mathf.PerlinNoise(Time.time * 8f, 0.2f);
            _tTick += Time.deltaTime;
            if (_tTick >= 1f)
            {
                _tTick -= 1f;
                _target.TakeDamage(new DamageInfo { amount = DamagePerDetik, type = DamageType.Api, point = transform.position, direction = Vector3.up, source = gameObject });
            }
        }
    }
}
