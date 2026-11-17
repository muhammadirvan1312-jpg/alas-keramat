using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Bom Bambu Minyak (06 §2): area api radius 3,5 m selama 8 s, 20 damage/s ke makhluk (tick 0,5 s), membakar struktur kayu & Andi bila di dalam.
    public class ApiArea : MonoBehaviour
    {
        public const float Radius = 3.5f, Lama = 8f, DamagePerDetik = 20f, Tick = 0.5f;
        float _umur, _tTick;
        Light _lampu;
        static readonly Collider[] _hasil = new Collider[24];

        public static void Buat(Vector3 titik)
        {
            var go = new GameObject("ApiArea");
            go.transform.position = titik + Vector3.up * 0.3f;
            var a = go.AddComponent<ApiArea>();
            a._lampu = go.AddComponent<Light>();
            a._lampu.type = LightType.Point; a._lampu.range = 10f; a._lampu.color = new Color(1f, 0.5f, 0.15f); a._lampu.shadows = LightShadows.None;
            var audio = GameServices.Get<ProceduralAudioSynth>();
            if (audio != null) audio.Play(ProceduralAudioSynth.KlipTembak, titik, AudioCategory.Efek, 0.8f, 0.6f, 3);
            MeterSuara.Pancar(titik, 30f, false);
        }

        void Update()
        {
            _umur += Time.deltaTime; _tTick += Time.deltaTime;
            if (_lampu != null) _lampu.intensity = (_umur < Lama ? 3.5f : 0f) * (0.8f + 0.4f * Mathf.PerlinNoise(Time.time * 10f, 0.4f));
            if (_umur >= Lama) { Destroy(gameObject); return; }
            if (_tTick < Tick) return;
            _tTick -= Tick;
            int n = Physics.OverlapSphereNonAlloc(transform.position, Radius, _hasil, (1 << GameLayers.Enemy) | (1 << GameLayers.Structure) | (1 << GameLayers.Player), QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                var c = _hasil[i];
                if (c.gameObject.layer == GameLayers.Player) { var r = c.GetComponentInParent<ReaksiAndi>(); if (r != null) r.Bakar(4f, 2f); continue; }
                var b = c.GetComponentInParent<AlasKeramat.Building.BagianStruktur>();
                if (b != null) { ApiStruktur.Nyalakan(b.gameObject); continue; }
                var t = c.GetComponentInParent<IDamageable>();
                if (t != null) t.TakeDamage(new DamageInfo { amount = DamagePerDetik * Tick, type = DamageType.Api, point = c.bounds.center, direction = Vector3.up, source = gameObject });
            }
        }
    }
}
