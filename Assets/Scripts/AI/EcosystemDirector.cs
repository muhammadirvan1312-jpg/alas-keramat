using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEngine;
using JH = AlasKeramat.Core.ResepHewan.JenisHewan;

namespace AlasKeramat.AI
{
    // 03 §8: maks 10 hewan aktif dalam 100 m; muncul berkelompok 50-90 m dari kamera di bioma yang cocok; hilang > 140 m bila tak terlihat.
    public class EcosystemDirector : MonoBehaviour
    {
        public const int MaksAktif = 42;
        public const float RadiusAktif = 115f, JarakMunculMin = 20f, JarakMunculMaks = 68f, JarakHilang = 135f, Interval = 1.25f;

        [SerializeField] GameObject[] template = new GameObject[0]; // indeks = JenisHewan
        readonly List<WildAnimalAI> _hidup = new List<WildAnimalAI>();
        readonly JH[] _cocok = new JH[16];
        float _t;

        public int Aktif { get { return _hidup.Count; } }
        public void Pasang(GameObject[] t) { template = t; }

        void Awake() { GameServices.Register(this); }

        void Start()
        {
            if (!Application.isPlaying) return;
            var cam = Camera.main;
            if (cam == null || WorldTerrainGenerator.DiBawahTanah(cam.transform.position.y)) return;
            for (int i = 0; i < 4 && _hidup.Count < MaksAktif; i++) MunculkanKelompok(cam.transform.position);
        }

        public static bool Cocok(JH j, Biome b)
        {
            switch (j)
            {
                case JH.Rusa: return b == Biome.HutanJatiSelatan || b == Biome.HutanCampuran || b == Biome.KawahBeringin;
                case JH.Kancil: return b == Biome.HutanCampuran || b == Biome.HutanBambu || b == Biome.HutanJatiSelatan || b == Biome.TepiSungai;
                case JH.BabiHutan: return b == Biome.HutanJatiSelatan || b == Biome.HutanCampuran || b == Biome.TepiSungai || b == Biome.KawahBeringin;
                case JH.AyamHutan: return b == Biome.HutanBambu || b == Biome.Desa || b == Biome.TepiSungai || b == Biome.HutanJatiSelatan || b == Biome.HutanCampuran;
                case JH.Harimau: return b == Biome.HutanJatiSelatan || b == Biome.HutanCampuran || b == Biome.KawahBeringin;
                case JH.Buaya: return b == Biome.TepiSungai || b == Biome.Pantai;
                case JH.Tawon: return b == Biome.HutanJatiSelatan || b == Biome.HutanCampuran || b == Biome.HutanBambu || b == Biome.TepiSungai || b == Biome.Desa;
                case JH.UlarPiton: return b == Biome.HutanCampuran || b == Biome.HutanBambu || b == Biome.TepiSungai || b == Biome.HutanJatiSelatan;
                case JH.UlarKobra: return b == Biome.HutanJatiSelatan || b == Biome.HutanBambu || b == Biome.Desa || b == Biome.SawitTerbakar;
                case JH.Monyet: return b == Biome.HutanJatiSelatan || b == Biome.HutanCampuran || b == Biome.HutanBambu || b == Biome.TepiSungai || b == Biome.Desa;
                case JH.Anjing: return b == Biome.Desa || b == Biome.HutanJatiSelatan || b == Biome.SawitTerbakar || b == Biome.TepiSungai;
                case JH.Kucing: return b == Biome.Desa || b == Biome.HutanCampuran || b == Biome.HutanJatiSelatan || b == Biome.TepiSungai;
                default: return b == Biome.TepiSungai || b == Biome.Pantai;
            }
        }

        public static Vector2Int Kelompok(JH j)
        {
            switch (j)
            {
                case JH.Rusa: case JH.Monyet: return new Vector2Int(2, 4);
                case JH.BabiHutan: case JH.Anjing: return new Vector2Int(2, 3);
                case JH.AyamHutan: return new Vector2Int(2, 5);
                case JH.Harimau: case JH.UlarPiton: return new Vector2Int(1, 1);
                default: return new Vector2Int(1, 2);
            }
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
            if (_hidup.Count >= MaksAktif || template.Length == 0 || WorldTerrainGenerator.DiBawahTanah(c.y)) return;
            MunculkanKelompok(c);
        }

        void MunculkanKelompok(Vector3 c)
        {
            if (template.Length == 0) return;
            float sudut = Random.value * Mathf.PI * 2f, jarak = Random.Range(JarakMunculMin, JarakMunculMaks);
            Vector3 p = c + new Vector3(Mathf.Cos(sudut), 0f, Mathf.Sin(sudut)) * jarak;
            var b = WorldTerrainGenerator.GetBiome(p.x, p.z);
            int nCocok = 0;
            for (int i = 0; i < template.Length && nCocok < _cocok.Length; i++)
            {
                if (template[i] != null && Cocok((JH)i, b)) _cocok[nCocok++] = (JH)i;
            }
            if (nCocok == 0) return;
            JH pilih = _cocok[Random.Range(0, nCocok)];
            var u = ResepHewan.Ambil(pilih);
            if (u.pemangsa && Vector2.Distance(new Vector2(p.x, p.z), new Vector2(-4f, -424f)) < 28f) return;
            var k = Kelompok(pilih);
            int n = Mathf.Min(Random.Range(k.x, k.y + 1), MaksAktif - _hidup.Count);
            for (int i = 0; i < n; i++)
            {
                Vector3 q = p + new Vector3(Random.Range(-4.5f, 4.5f), 0f, Random.Range(-4.5f, 4.5f));
                float h = WorldTerrainGenerator.SampleHeight(q.x, q.z);
                float? wl = WorldTerrainGenerator.GetWaterLevelAt(q.x, q.z);
                if (h < 0.5f || (!u.terbang && !u.amfibi && wl.HasValue && wl.Value > h + 0.25f)) continue;
                float sy = u.terbang ? Mathf.Max(h, wl ?? h) + 1.25f : h;
                var go = Instantiate(template[(int)pilih], new Vector3(q.x, sy, q.z), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), transform);
                go.SetActive(true);
                var ai = go.GetComponent<WildAnimalAI>();
                if (ai != null) _hidup.Add(ai);
            }
        }
    }
}
