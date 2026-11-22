using System.Collections;
using AlasKeramat.Core;
using AlasKeramat.Player;
using AlasKeramat.UI;
using UnityEngine;
namespace AlasKeramat.AI
{
    // Helikopter SAR (04 §7.6-7.9): turun di Landasan SAR setelah siege; Andi dalam 8 m dari pintu -> Pilihan Takdir;
    // Ending B: heli terbang pergi dan pulau menjadi sandbox (tanpa Malam Darah).
    public class HeliSAR : MonoBehaviour
    {
        public const float JarakPintu = 8f, TinggiDatang = 45f, LamaTurun = 8f, LajuPergi = 14f, LamaPergi = 9f;
        [SerializeField] GameObject model;
        Vector3 _landas;
        bool _turun, _ditawarkan, _pergi;
        AndiFPSController _andi;
        ParticleSystem _debuBaling;
        Light _lampuSorot;

        public static float LajuDebuHeliSAR(float tinggiDariLandasan)
        {
            if (tinggiDariLandasan > 28f || tinggiDariLandasan < -1f) return 0f;
            return Mathf.Lerp(36f, 6f, Mathf.Clamp01(tinggiDariLandasan / 28f));
        }

        void PerbaruiEfekLandas(float tinggi)
        {
            float laju = LajuDebuHeliSAR(tinggi);
            if (laju > 0f && _debuBaling == null)
            {
                var go = new GameObject("DebuBalingHeli");
                go.transform.position = _landas + Vector3.up * 0.25f;
                _debuBaling = go.AddComponent<ParticleSystem>();
                var m = _debuBaling.main; m.loop = true; m.startLifetime = 1.1f; m.startSpeed = 4.2f; m.startSize = 0.45f; m.maxParticles = 40;
                m.startColor = new Color(0.76f, 0.72f, 0.64f, 0.35f);
                var sh = _debuBaling.shape; sh.shapeType = ParticleSystemShapeType.Circle; sh.radius = 3.2f;
                var pr = go.GetComponent<ParticleSystemRenderer>();
                pr.sharedMaterial = SafeMaterialFactory.CreatePBR("M_DebuHeliSAR", new Color(0.76f, 0.72f, 0.64f, 0.32f), 0f, 0.1f, null, null, null, SurfaceMode.Transparent);
                pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                var lGo = new GameObject("LampuSorotHeli");
                lGo.transform.SetParent(transform, false);
                lGo.transform.localPosition = new Vector3(0f, -0.4f, 1.5f);
                lGo.transform.localRotation = Quaternion.Euler(48f, 0f, 0f);
                _lampuSorot = lGo.AddComponent<Light>();
                _lampuSorot.type = LightType.Spot; _lampuSorot.spotAngle = 52f; _lampuSorot.range = 48f; _lampuSorot.intensity = 18f;
                _lampuSorot.color = new Color(0.95f, 0.97f, 1f); _lampuSorot.shadows = LightShadows.None;
            }
            if (_debuBaling != null)
            {
                var em = _debuBaling.emission; em.rateOverTime = laju;
                if (laju > 0f && !_debuBaling.isPlaying) _debuBaling.Play();
                else if (laju <= 0f && _debuBaling.isPlaying) _debuBaling.Stop();
            }
            if (_lampuSorot != null) _lampuSorot.enabled = model != null && model.activeSelf && !_pergi;
        }

        public void Pasang(GameObject m) { model = m; }
        void Awake() { GameServices.Register(this); _landas = transform.position; if (model != null) model.SetActive(false); }

        public static bool BolehDatang(QuestManager.ProgresMisiT6 p) { return p.siegeSelesai && p.ending == 0; }

        void Update()
        {
            var q = GameServices.Get<QuestManager>(); if (q == null || model == null) return;
            if (q.Progres.ending == 2) EnemyDirector.TanpaMalamDarah = true;
            if (_pergi) return;
            bool datang = BolehDatang(q.Progres);
            if (datang != model.activeSelf) model.SetActive(datang);
            if (!datang) return;
            PerbaruiEfekLandas(transform.position.y - _landas.y);
            if (!_turun) { _turun = true; StartCoroutine(Turun()); }
            if (_ditawarkan) return;
            if (_andi == null) { _andi = GameServices.Get<AndiFPSController>(); if (_andi == null) return; }
            if ((_andi.transform.position - transform.position).sqrMagnitude <= JarakPintu * JarakPintu)
            {
                var s = GameServices.Get<LayarSinema>();
                if (s != null) { _ditawarkan = true; s.TampilPilihan(); }
            }
        }

        IEnumerator Turun()
        {
            var a = GameServices.Get<ProceduralAudioSynth>();
            if (a != null) a.Play(ProceduralAudioSynth.KlipPesawat, _landas, AudioCategory.Efek, 1f, 0.9f, 8);
            for (float t = 0f; t < LamaTurun; t += Time.deltaTime) { transform.position = _landas + Vector3.up * TinggiDatang * (1f - Mathf.SmoothStep(0f, 1f, t / LamaTurun)); yield return null; }
            transform.position = _landas;
        }

        public void Terbang() { if (!_pergi && model != null && model.activeSelf) StartCoroutine(Pergi()); }

        IEnumerator Pergi()
        {
            _pergi = true;
            Vector3 awal = transform.position;
            for (float t = 0f; t < LamaPergi; t += Time.deltaTime) { transform.position = awal + new Vector3(0f, t * LajuPergi * 0.5f, -t * LajuPergi); yield return null; }
            model.SetActive(false);
        }
    }
}
