using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEngine;
using JH = AlasKeramat.Core.ResepHewan.JenisHewan;

namespace AlasKeramat.AI
{
    // 03 §8: maks 10 hewan aktif dalam 100 m; muncul berkelompok 50-90 m dari kamera di bioma yang cocok; hilang > 140 m bila tak terlihat.
    public class EcosystemDirector : MonoBehaviour
    {
        public const int MaksAktif = 10;
        public const float RadiusAktif = 100f, JarakMunculMin = 50f, JarakMunculMaks = 90f, JarakHilang = 140f, Interval = 2f;

        [SerializeField] GameObject[] template = new GameObject[0]; // indeks = JenisHewan
        readonly List<WildAnimalAI> _hidup = new List<WildAnimalAI>();
        float _t;

        public int Aktif { get { return _hidup.Count; } }
        public void Pasang(GameObject[] t) { template = t; }

        void Awake() { GameServices.Register(this); }

        public static bool Cocok(JH j, Biome b)
        {
            switch (j)
            {
                case JH.Rusa: return b == Biome.HutanJatiSelatan || b == Biome.HutanCampuran || b == Biome.KawahBeringin;
                case JH.Kancil: return b == Biome.HutanCampuran || b == Biome.HutanBambu;
                case JH.BabiHutan: return b == Biome.HutanJatiSelatan || b == Biome.HutanCampuran;
                case JH.AyamHutan: return b == Biome.HutanBambu || b == Biome.Desa || b == Biome.TepiSungai;
                default: return b == Biome.TepiSungai;
            }
        }

        public static Vector2Int Kelompok(JH j)
        {
            switch (j) { case JH.Rusa: return new Vector2Int(2, 4); case JH.BabiHutan: return new Vector2Int(2, 3); case JH.AyamHutan: return new Vector2Int(2, 5); default: return new Vector2Int(1, 1); }
        }

        void Update()
        {
            _t += Time.deltaTime;
            if (_t < Interval) return;
            _t = 0f;
            var cam = Camera.main;
            if (cam == null) return;
            Vector3 c = cam.transform.position;
            for (int i = _hidup.Count - 1; i >= 0; i--)
            {
                var h = _hidup[i];
                if (h == null) { _hidup.RemoveAt(i); continue; }
                Vector3 d = h.transform.position - c;
                if (d.magnitude > JarakHilang && Vector3.Dot(d.normalized, cam.transform.forward) < 0.3f) { Destroy(h.gameObject); _hidup.RemoveAt(i); }
            }
            if (_hidup.Count >= MaksAktif || template.Length == 0) return;
            float sudut = Random.value * Mathf.PI * 2f, jarak = Random.Range(JarakMunculMin, JarakMunculMaks);
            Vector3 p = c + new Vector3(Mathf.Cos(sudut), 0f, Mathf.Sin(sudut)) * jarak;
            var b = WorldTerrainGenerator.GetBiome(p.x, p.z);
            JH pilih = (JH)Random.Range(0, template.Length);
            if (!Cocok(pilih, b) || template[(int)pilih] == null) return;
            var k = Kelompok(pilih);
            int n = Mathf.Min(Random.Range(k.x, k.y + 1), MaksAktif - _hidup.Count);
            for (int i = 0; i < n; i++)
            {
                Vector3 q = p + new Vector3(Random.Range(-4f, 4f), 0f, Random.Range(-4f, 4f));
                float h = WorldTerrainGenerator.SampleHeight(q.x, q.z);
                if (h < 0.5f) continue;
                var go = Instantiate(template[(int)pilih], new Vector3(q.x, h, q.z), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), transform);
                go.SetActive(true);
                var ai = go.GetComponent<WildAnimalAI>();
                if (ai != null) _hidup.Add(ai);
            }
        }
    }
}
