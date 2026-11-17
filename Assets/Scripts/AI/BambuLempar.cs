using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.AI
{
    // 06 §6: bambu runcing dilempar Abdi Wanita (8-18 m, 18 damage). Lintasan balistik manual; bisa diblok, tidak bisa di-parry.
    public class BambuLempar : MonoBehaviour
    {
        public const float Gravitasi = 9.81f, Laju = 16f, Damage = 18f, RadiusKena = 0.45f, LamaTancap = 8f, UmurMaks = 4f;

        Vector3 _v;
        GameObject _pelempar;
        float _umur;
        bool _tancap;
        static readonly Collider[] _hasil = new Collider[4];

        // Kecepatan awal agar mendarat di target dengan laju datar Laju (fungsi murni untuk self-test).
        public static Vector3 KecepatanAwal(Vector3 dari, Vector3 ke)
        {
            Vector3 d = ke - dari; float tinggi = d.y; d.y = 0f;
            float t = Mathf.Max(0.2f, d.magnitude / Laju);
            Vector3 v = d / t;
            v.y = (tinggi + 0.5f * Gravitasi * t * t) / t;
            return v;
        }

        public static void Lempar(GameObject prefab, Vector3 dari, Vector3 ke, GameObject pelempar)
        {
            if (prefab == null) return;
            var go = Instantiate(prefab, dari, Quaternion.identity);
            var b = go.GetComponent<BambuLempar>();
            if (b == null) { Destroy(go); return; }
            b._v = KecepatanAwal(dari, ke); b._pelempar = pelempar;
        }

        void Update()
        {
            _umur += Time.deltaTime;
            if (_tancap) { if (_umur > LamaTancap) Destroy(gameObject); return; }
            float dt = Time.deltaTime;
            _v.y -= Gravitasi * dt;
            transform.position += _v * dt;
            if (_v.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(_v) * Quaternion.Euler(90f, 0f, 0f);
            int n = Physics.OverlapSphereNonAlloc(transform.position, RadiusKena, _hasil, (1 << GameLayers.Player) | (1 << GameLayers.Structure), QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                var vital = _hasil[i].GetComponentInParent<AndiVitalsAndSanity>();
                if (vital != null)
                {
                    var c = GameServices.Get<CombatAndLeftHandController>();
                    bool parry;
                    float dmg = c != null ? c.TerimaSerangan(transform.position - _v.normalized, Damage, false, out parry) : Damage;
                    vital.TakeDamage(new DamageInfo { amount = dmg, type = DamageType.Tusuk, point = transform.position, direction = _v.normalized, source = _pelempar });
                }
                Tancap(); return;
            }
            float h = WorldTerrainGenerator.SampleHeight(transform.position.x, transform.position.z);
            if (transform.position.y <= h || _umur > UmurMaks) Tancap();
        }

        void Tancap() { _tancap = true; _umur = 0f; }
    }
}
