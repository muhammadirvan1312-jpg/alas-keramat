using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Player
{
    // Jejak kaki basah/pasir/lumpur Andi memakai ring buffer 24 quad decal tanpa alokasi GC di Update.
    public class JejakKakiAndi : MonoBehaviour
    {
        public const int Kapasitas = 24;
        public const float JarakLangkah = 1.25f, LamaPudar = 18f;

        struct Jejak
        {
            public Transform tr;
            public MeshRenderer mr;
            public float lahir;
            public bool aktif;
        }

        Jejak[] _pool;
        int _idx;
        bool _kiri;
        Vector3 _posLalu;
        float _jarak;
        AndiFPSController _ctrl;
        URPAtmosphereAndWeather _cuaca;
        MaterialPropertyBlock _mpb;
        static Material _matJejak;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatis() { _matJejak = null; }

        public static bool BolehCetakJejak(Biome b, WeatherState w, bool diTanah, bool berenang)
        {
            if (!diTanah || berenang) return false;
            if (b == Biome.Pantai || b == Biome.TepiSungai || b == WorldTerrainGenerator.RawaBakau) return true;
            return w == WeatherState.Gerimis || w == WeatherState.HujanBadai;
        }

        void Awake()
        {
            _ctrl = GetComponent<AndiFPSController>();
            _posLalu = transform.position;
            _mpb = new MaterialPropertyBlock();
        }

        void SiapkanPool()
        {
            if (_pool != null) return;
            if (_matJejak == null)
                _matJejak = SafeMaterialFactory.CreatePBR("M_JejakKaki", new Color(0.14f, 0.11f, 0.08f, 0.48f), 0f, 0.12f, null, null, null, SurfaceMode.Transparent);
            var induk = new GameObject("PoolJejakKaki").transform;
            _pool = new Jejak[Kapasitas];
            for (int i = 0; i < Kapasitas; i++)
            {
                var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
                q.name = "Jejak_" + i;
                Destroy(q.GetComponent<Collider>());
                q.transform.SetParent(induk, false);
                q.transform.localScale = new Vector3(0.16f, 0.30f, 1f);
                var mr = q.GetComponent<MeshRenderer>();
                mr.sharedMaterial = _matJejak;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = false;
                q.SetActive(false);
                _pool[i] = new Jejak { tr = q.transform, mr = mr };
            }
        }

        void Update()
        {
            if (_ctrl == null || JedaDunia.Dijeda) return;
            float kini = Time.time;
            if (_pool != null)
            {
                for (int i = 0; i < _pool.Length; i++)
                {
                    if (!_pool[i].aktif) continue;
                    float umur = kini - _pool[i].lahir;
                    if (umur >= LamaPudar)
                    {
                        _pool[i].aktif = false;
                        _pool[i].tr.gameObject.SetActive(false);
                    }
                    else
                    {
                        float a = 0.45f * (1f - umur / LamaPudar);
                        _mpb.SetColor("_BaseColor", new Color(0.14f, 0.11f, 0.08f, a));
                        _pool[i].mr.SetPropertyBlock(_mpb);
                    }
                }
            }

            Vector3 p = transform.position;
            Vector3 d = p - _posLalu; d.y = 0f; _posLalu = p;
            if (d.sqrMagnitude > 16f) { _jarak = 0f; return; }
            _jarak += d.magnitude;
            if (_jarak < JarakLangkah) return;
            _jarak = 0f;

            if (_cuaca == null) _cuaca = GameServices.Get<URPAtmosphereAndWeather>();
            var bio = WorldTerrainGenerator.GetBiome(p.x, p.z);
            var w = _cuaca != null ? _cuaca.Cuaca : WeatherState.Cerah;
            if (!BolehCetakJejak(bio, w, _ctrl.DiTanah, _ctrl.Berenang)) return;
            if (WorldTerrainGenerator.DiBawahTanah(p.y)) return;

            SiapkanPool();
            _kiri = !_kiri;
            Vector3 samping = transform.right * (_kiri ? -0.14f : 0.14f);
            Vector3 tp = p + samping;
            float y = WorldTerrainGenerator.SampleHeight(tp.x, tp.z) + 0.03f;
            Vector3 norm = WorldTerrainGenerator.SampleNormal(tp.x, tp.z);
            var j = _pool[_idx];
            j.tr.position = new Vector3(tp.x, y, tp.z);
            Vector3 fwd = Vector3.ProjectOnPlane(transform.forward, norm);
            if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
            j.tr.rotation = Quaternion.LookRotation(fwd, norm) * Quaternion.Euler(90f, 0f, 0f);
            j.tr.gameObject.SetActive(true);
            j.lahir = kini;
            j.aktif = true;
            _mpb.SetColor("_BaseColor", new Color(0.14f, 0.11f, 0.08f, 0.45f));
            j.mr.SetPropertyBlock(_mpb);
            _pool[_idx] = j;
            _idx = (_idx + 1) % Kapasitas;
        }
    }
}
