using AlasKeramat.Player;
using UnityEngine;
namespace AlasKeramat.Core
{
    // Lorong Angin UG3 K1 (03 §10, gerbang lunak 04 §4): obor padam otomatis; tanpa Senter gelap & Kewarasan turun cepat.
    public class ZonaAngin : MonoBehaviour
    {
        public const float Tick = 0.25f, SanityPerDetikGelap = -1.5f;
        [SerializeField] Vector3 ukuran = new Vector3(4f, 5f, 15f);
        float _t;
        AndiFPSController _andi;
        public void Pasang(Vector3 u) { ukuran = u; }
        public bool Berisi(Vector3 p) { Vector3 l = transform.InverseTransformPoint(p); return Mathf.Abs(l.x) <= ukuran.x * 0.5f && Mathf.Abs(l.z) <= ukuran.z * 0.5f && l.y >= -1f && l.y <= ukuran.y; }
        void Update()
        {
            _t += Time.deltaTime; if (_t < Tick) return; _t = 0f;
            if (_andi == null) { _andi = GameServices.Get<AndiFPSController>(); if (_andi == null) return; }
            if (!Berisi(_andi.transform.position)) return;
            var tangan = GameServices.Get<TanganKiri>();
            if (tangan != null && tangan.OborMenyala) tangan.Padamkan();
            var senter = GameServices.Get<SenterAndi>();
            if (senter == null || !senter.Nyala)
            {
                var v = GameServices.Get<AndiVitalsAndSanity>();
                if (v != null) v.UbahSanity(SanityPerDetikGelap * Tick);
            }
        }
    }
}
