using UnityEngine;

namespace AlasKeramat.Core
{
    // Sistem musim dinamis setara SOTF (Kemarau, Pancaroba, Penghujan) + genangan hujan reflektif yang muncul saat hujan dan menguap saat cerah.
    public class SistemMusimDinamis : MonoBehaviour
    {
        public enum FaseMusim { Kemarau = 0, Pancaroba = 1, Penghujan = 2 }
        public const float LajuGenanganNaik = 0.08f, LajuGenanganSurut = 0.035f;

        [SerializeField] MeshRenderer[] genangan = new MeshRenderer[0];
        float _genangan01, _t;
        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _cuaca;
        MaterialPropertyBlock _mpb;

        public float Genangan01 { get { return _genangan01; } }
        public void PasangGenangan(MeshRenderer[] g) { genangan = g ?? new MeshRenderer[0]; }

        public static FaseMusim FaseHari(int hari)
        {
            int siklus = Mathf.Max(0, hari - 1) % 9;
            if (siklus < 4) return FaseMusim.Kemarau;
            if (siklus == 4) return FaseMusim.Pancaroba;
            return FaseMusim.Penghujan;
        }

        public static float PengaliHidrasiMusim(FaseMusim f)
        {
            return f == FaseMusim.Kemarau ? 1.18f : f == FaseMusim.Penghujan ? 0.85f : 1.0f;
        }

        public static float TargetGenangan(WeatherState w, FaseMusim f)
        {
            if (w == WeatherState.HujanBadai) return 1f;
            if (w == WeatherState.Gerimis) return 0.65f;
            return f == FaseMusim.Penghujan && w == WeatherState.Berawan ? 0.28f : 0f;
        }

        void Awake()
        {
            GameServices.Register(this);
            _mpb = new MaterialPropertyBlock();
        }

        void Update()
        {
            if (JedaDunia.Dijeda) return;
            _t += Time.deltaTime;
            if (_t < 0.25f) return;
            float dt = _t; _t = 0f;

            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            if (_cuaca == null) _cuaca = GameServices.Get<URPAtmosphereAndWeather>();
            var fase = FaseHari(_jam != null ? _jam.Hari : 1);
            var w = _cuaca != null ? _cuaca.Cuaca : WeatherState.Cerah;
            float target = TargetGenangan(w, fase);
            float laju = target > _genangan01 ? LajuGenanganNaik : LajuGenanganSurut;
            _genangan01 = Mathf.MoveTowards(_genangan01, target, dt * laju);

            if (genangan == null || genangan.Length == 0) return;
            bool tampil = _genangan01 > 0.02f;
            _mpb.SetColor("_BaseColor", new Color(0.08f, 0.14f, 0.16f, _genangan01 * 0.72f));
            for (int i = 0; i < genangan.Length; i++)
            {
                var r = genangan[i];
                if (r == null) continue;
                if (r.enabled != tampil) r.enabled = tampil;
                if (tampil) r.SetPropertyBlock(_mpb);
            }
        }
    }
}
