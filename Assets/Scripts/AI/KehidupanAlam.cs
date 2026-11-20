using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Satwa ambien super ringan (tanpa collider/rigidbody) agar hutan terasa hidup siang maupun malam.
    public class KehidupanAlam : MonoBehaviour
    {
        public const int JumlahBurung = 10, JumlahKupu = 18, JumlahKunang = 24, JumlahIkan = 12;

        [SerializeField] GameObject wadah;
        [SerializeField] Transform[] burung = new Transform[0];
        [SerializeField] Transform[] sayapKiri = new Transform[0];
        [SerializeField] Transform[] sayapKanan = new Transform[0];
        [SerializeField] Transform[] kupu = new Transform[0];
        [SerializeField] Transform[] kunang = new Transform[0];
        [SerializeField] Transform[] ikan = new Transform[0];

        float[] _rBurung, _sBurung, _tBurung;
        Vector3[] _pusatKupu, _pusatKunang, _pusatIkan;
        float[] _fasa;
        TimeOfDayClock _jam;
        bool _siap;

        public void Pasang(GameObject w, Transform[] b, Transform[] sk, Transform[] sn, Transform[] kp, Transform[] kn, Transform[] ik)
        {
            wadah = w; burung = b; sayapKiri = sk; sayapKanan = sn; kupu = kp; kunang = kn; ikan = ik;
        }

        void Awake()
        {
            int nB = burung.Length;
            _rBurung = new float[nB]; _sBurung = new float[nB]; _tBurung = new float[nB];
            for (int i = 0; i < nB; i++) { _rBurung[i] = 28f + (i % 5) * 9f; _sBurung[i] = (i % 2 == 0 ? 1f : -1f) * (0.14f + (i % 3) * 0.04f); _tBurung[i] = i * 0.63f; }
            _pusatKupu = new Vector3[kupu.Length];
            _pusatKunang = new Vector3[kunang.Length];
            _pusatIkan = new Vector3[ikan.Length];
            int mf = Mathf.Max(kupu.Length, kunang.Length);
            _fasa = new float[Mathf.Max(mf, ikan.Length)];
            for (int i = 0; i < _fasa.Length; i++) _fasa[i] = i * 1.37f;
            _siap = true;
        }

        void Update()
        {
            if (!_siap || !Application.isPlaying) return;
            var cam = Camera.main;
            if (cam == null) return;
            Vector3 c = cam.transform.position;
            bool gua = WorldTerrainGenerator.DiBawahTanah(c.y);
            if (wadah != null && wadah.activeSelf == gua) wadah.SetActive(!gua);
            if (gua) return;

            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            float jam = _jam != null ? _jam.Jam : 10f;
            bool siang = jam >= 5.5f && jam < 17.8f;
            float dt = Time.deltaTime, t = Time.time;

            for (int i = 0; i < burung.Length; i++)
            {
                var tr = burung[i]; if (tr == null) continue;
                _tBurung[i] += _sBurung[i] * dt;
                float a = _tBurung[i], r = _rBurung[i];
                float bx = c.x + Mathf.Cos(a) * r, bz = c.z + Mathf.Sin(a) * r;
                float ground = WorldTerrainGenerator.SampleHeight(bx, bz);
                float by = Mathf.Max(ground + 24f + (i % 4) * 6f, 28f);
                tr.position = new Vector3(bx, by + Mathf.Sin(t * 1.1f + i) * 1.2f, bz);
                float heading = (_sBurung[i] > 0f ? 90f : -90f) - a * Mathf.Rad2Deg;
                tr.rotation = Quaternion.Euler(0f, heading, (_sBurung[i] > 0f ? -14f : 14f));
                float kepak = Mathf.Sin(t * 7f + i * 1.9f) * 28f;
                if (i < sayapKiri.Length && sayapKiri[i] != null) sayapKiri[i].localRotation = Quaternion.Euler(0f, 0f, kepak);
                if (i < sayapKanan.Length && sayapKanan[i] != null) sayapKanan[i].localRotation = Quaternion.Euler(0f, 0f, -kepak);
            }

            TickKecil(kupu, _pusatKupu, c, siang, 18f, 1.3f, t, dt, 1f, 9f);
            TickKecil(kunang, _pusatKunang, c, !siang, 22f, 1.4f, t, dt, 0.55f, 2.2f);
            TickIkan(c, t, dt);
        }

        void TickKecil(Transform[] list, Vector3[] pusat, Vector3 c, bool aktif, float radius, float tinggiMin, float t, float dt, float laju, float frek)
        {
            for (int i = 0; i < list.Length; i++)
            {
                var tr = list[i]; if (tr == null) continue;
                if (tr.gameObject.activeSelf != aktif) tr.gameObject.SetActive(aktif);
                if (!aktif) continue;
                Vector3 p = pusat[i];
                if (p == Vector3.zero || (p - c).sqrMagnitude > radius * radius * 1.8f)
                {
                    float s = _fasa[i] + t * 0.1f, r = 4f + (i * 0.73f % 1f) * (radius - 4f);
                    p = new Vector3(c.x + Mathf.Cos(s) * r, 0f, c.z + Mathf.Sin(s) * r);
                    p.y = Mathf.Max(0.5f, WorldTerrainGenerator.SampleHeight(p.x, p.z)) + tinggiMin + (i % 4) * 0.45f;
                    pusat[i] = p;
                }
                float f = _fasa[i] + t * laju;
                Vector3 pos = p + new Vector3(Mathf.Sin(f * 1.3f) * 1.6f, Mathf.Sin(f * frek) * 0.35f, Mathf.Cos(f * 1.1f) * 1.6f);
                tr.position = pos;
                tr.rotation = Quaternion.Euler(0f, f * 45f, Mathf.Sin(t * 16f + i) * 12f);
            }
        }

        void TickIkan(Vector3 c, float t, float dt)
        {
            for (int i = 0; i < ikan.Length; i++)
            {
                var tr = ikan[i]; if (tr == null) continue;
                Vector3 p = _pusatIkan[i];
                if (p == Vector3.zero || (p - c).sqrMagnitude > 38f * 38f)
                {
                    float ang = (i / (float)Mathf.Max(1, ikan.Length)) * Mathf.PI * 2f + t * 0.05f;
                    float r = 6f + (i % 5) * 4.5f;
                    float x = c.x + Mathf.Cos(ang) * r, z = c.z + Mathf.Sin(ang) * r;
                    float? wl = WorldTerrainGenerator.GetWaterLevelAt(x, z);
                    float g = WorldTerrainGenerator.SampleHeight(x, z);
                    bool adaAir = wl.HasValue && wl.Value > g + 0.35f;
                    if (!adaAir) { if (tr.gameObject.activeSelf) tr.gameObject.SetActive(false); continue; }
                    p = new Vector3(x, Mathf.Lerp(g + 0.18f, wl.Value - 0.14f, 0.55f), z);
                    _pusatIkan[i] = p;
                    if (!tr.gameObject.activeSelf) tr.gameObject.SetActive(true);
                }
                float ph = _fasa[i] + t * 1.4f;
                Vector3 d = new Vector3(Mathf.Sin(ph) * 2.2f, 0f, Mathf.Cos(ph * 0.9f) * 2.2f);
                Vector3 np = p + d;
                float? nwl = WorldTerrainGenerator.GetWaterLevelAt(np.x, np.z);
                float ng = WorldTerrainGenerator.SampleHeight(np.x, np.z);
                if (!nwl.HasValue || nwl.Value <= ng + 0.25f) { _pusatIkan[i] = Vector3.zero; if (tr.gameObject.activeSelf) tr.gameObject.SetActive(false); continue; }
                np.y = Mathf.Lerp(ng + 0.18f, nwl.Value - 0.15f, 0.55f);
                Vector3 dir = np - tr.position;
                tr.position = np;
                if (dir.sqrMagnitude > 0.0001f) tr.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            }
        }
    }
}
