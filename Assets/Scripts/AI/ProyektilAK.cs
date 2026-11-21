using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.AI
{
    // T7d (06 §2, 17 §2.1): proyektil balistik Andi - panah (api/racun/tulang), tombak (bisa dipungut kembali), batu ketapel (goyah + umpan),
    // suar (bakar + cahaya 8 s, Takut Sekte +30), bom bambu (area api r 3,5 m 8 s). Tabrakan = SphereCast dari posisi lalu ke posisi baru.
    public class ProyektilAK : MonoBehaviour
    {
        public const float Gravitasi = 9.81f, UmurMaks = 6f, LamaTancap = 20f, LamaSuar = 8f, RadiusTakutSuar = 15f, TakutSuar = 30f;
        public const float BakarPanahApi = 5f, LamaPanahApi = 6f, RacunPerDetik = 4f, LamaRacun = 8f, BakarSuar = 6f, LamaBakarSuar = 5f, GoyahKetapel = 0.6f;

        Vector3 _v;
        float _g, _damage, _umur;
        ItemID _amunisi;
        GameObject _pemilik;
        bool _berhenti;
        Light _cahaya;

        public static bool PunyaJejakProyektil(ItemID amunisi)
        {
            return amunisi == ItemID.PanahApi || amunisi == ItemID.PanahRacun || amunisi == ItemID.PelurSuar || amunisi == ItemID.BomBambuMinyak;
        }

        void PasangJejakProyektil()
        {
            if (!PunyaJejakProyektil(_amunisi)) return;
            bool racun = _amunisi == ItemID.PanahRacun;
            bool suar = _amunisi == ItemID.PelurSuar;
            Color w = racun ? new Color(0.28f, 0.88f, 0.25f, 0.75f) : suar ? new Color(1f, 0.22f, 0.14f, 0.85f) : new Color(1f, 0.56f, 0.14f, 0.85f);
            var go = new GameObject("JejakProyektil");
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            var m = ps.main;
            m.loop = true; m.startLifetime = 0.35f; m.startSpeed = 0.25f; m.startSize = suar ? 0.09f : 0.055f;
            m.maxParticles = 28; m.simulationSpace = ParticleSystemSimulationSpace.World;
            m.startColor = w;
            var em = ps.emission; em.rateOverTime = 24f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.03f;
            var pr = go.GetComponent<ParticleSystemRenderer>();
            pr.sharedMaterial = SafeMaterialFactory.CreatePBR("M_JejakProyektil_" + (int)_amunisi, w, 0f, 0.3f, null, null, w * 2.2f);
            pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            if (!racun && _cahaya == null)
            {
                _cahaya = go.AddComponent<Light>();
                _cahaya.type = LightType.Point;
                _cahaya.color = w;
                _cahaya.range = suar ? 11f : 5.5f;
                _cahaya.intensity = suar ? 3.2f : 1.8f;
                _cahaya.shadows = LightShadows.None;
            }
        }

        public static void Luncurkan(GameObject prefab, Vector3 asal, Vector3 kecepatan, float skalaGravitasi, float damage, ItemID amunisi, GameObject pemilik)
        {
            GameObject go = prefab != null ? Instantiate(prefab, asal, Quaternion.LookRotation(kecepatan)) : new GameObject("Proyektil");
            if (prefab == null) go.transform.position = asal;
            var p = go.GetComponent<ProyektilAK>();
            if (p == null) p = go.AddComponent<ProyektilAK>();
            p._v = kecepatan; p._g = skalaGravitasi * Gravitasi; p._damage = damage; p._amunisi = amunisi; p._pemilik = pemilik;
            p._cahaya = go.GetComponentInChildren<Light>();
            p.PasangJejakProyektil();
        }

        void Update()
        {
            float dt = Time.deltaTime;
            _umur += dt;
            if (_berhenti)
            {
                if (_cahaya != null) _cahaya.intensity = _umur < LamaSuar ? 3f + Mathf.PerlinNoise(Time.time * 9f, 0.1f) : Mathf.MoveTowards(_cahaya.intensity, 0f, dt * 2f);
                if (_umur > LamaTancap) Destroy(gameObject);
                return;
            }
            Vector3 lalu = transform.position;
            _v.y -= _g * dt;
            Vector3 baru = lalu + _v * dt;
            Vector3 d = baru - lalu;
            int mask = (1 << GameLayers.Enemy) | (1 << GameLayers.Tree) | (1 << GameLayers.Structure) | (1 << GameLayers.Ground) | (1 << GameLayers.LogPhysics) | 1;
            RaycastHit hit;
            if (d.sqrMagnitude > 0.000001f && Physics.SphereCast(lalu, 0.06f, d.normalized, out hit, d.magnitude, mask, QueryTriggerInteraction.Ignore))
            {
                transform.position = hit.point;
                Kena(hit.collider, hit.point, hit.normal);
                return;
            }
            transform.position = baru;
            if (_v.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(_v);
            float h = WorldTerrainGenerator.SampleHeight(baru.x, baru.z);
            if (baru.y <= h) { transform.position = new Vector3(baru.x, h, baru.z); Kena(null, transform.position, Vector3.up); return; }
            if (_umur > UmurMaks) Destroy(gameObject);
        }

        void Kena(Collider c, Vector3 titik, Vector3 normal)
        {
            _berhenti = true; _umur = 0f;
            var target = c != null ? c.GetComponentInParent<IDamageable>() : null;
            if (target != null && _pemilik != null && ReferenceEquals(target, _pemilik.GetComponent<IDamageable>())) target = null;
            DamageType tipe = _amunisi == ItemID.PelurSuar ? DamageType.Api : DamageType.Proyektil;
            if (_amunisi == ItemID.BomBambuMinyak) { ApiArea.Buat(titik); Destroy(gameObject); return; }
            var akar = c != null ? c.GetComponentInParent<AkarBakar>() : null;
            if (akar != null && (_amunisi == ItemID.PanahApi || _amunisi == ItemID.PelurSuar)) akar.Bakar();
            if (target != null)
            {
                target.TakeDamage(new DamageInfo { amount = _damage, type = tipe, point = titik, direction = _v.normalized, source = _pemilik, isHeavy = _damage >= 45f });
                var ai = c.GetComponentInParent<NusantaraEnemyAndMutantAI>();
                var hantu = c.GetComponentInParent<HantuAI>();
                if (_amunisi == ItemID.PanahApi || _amunisi == ItemID.PelurSuar)
                {
                    float dps = _amunisi == ItemID.PanahApi ? BakarPanahApi : BakarSuar, lama = _amunisi == ItemID.PanahApi ? LamaPanahApi : LamaBakarSuar;
                    if (ai != null) ai.Bakar(dps, lama, false); if (hantu != null) hantu.Bakar(dps, lama);
                }
                if (_amunisi == ItemID.PanahRacun && ai != null) ai.Bakar(RacunPerDetik, LamaRacun, true);
                if (_amunisi == ItemID.Batu && ai != null) ai.Goyahkan(GoyahKetapel);
                CombatAndLeftHandController.PicuKenaJarak();
                transform.SetParent(c.transform, true);
            }
            else if (c != null && c.GetComponentInParent<AlasKeramat.Building.BagianStruktur>() != null && _amunisi == ItemID.PanahApi) ApiStruktur.Nyalakan(c.GetComponentInParent<AlasKeramat.Building.BagianStruktur>().gameObject);
            if (_amunisi == ItemID.Batu) MeterSuara.Pancar(titik, MeterSuara.Umpan, false); // umpan suara (17 §2.4)
            if (_amunisi == ItemID.PelurSuar)
            {
                var s = NusantaraEnemyAndMutantAI.Semua;
                for (int i = 0; i < s.Count; i++) if (s[i].IsAlive && (s[i].transform.position - titik).sqrMagnitude < RadiusTakutSuar * RadiusTakutSuar) s[i].Takut += TakutSuar;
            }
            bool pungut = _amunisi == ItemID.TombakBambu || _amunisi == ItemID.TombakLogam || ((_amunisi == ItemID.AnakPanah || _amunisi == ItemID.PanahTulang) && Random.value < 0.5f);
            if (pungut && target == null) { JarahanTanah.Jatuhkan(titik, _amunisi, 1); Destroy(gameObject); }
        }
    }
}
