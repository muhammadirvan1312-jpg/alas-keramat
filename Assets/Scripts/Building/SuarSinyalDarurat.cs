using AlasKeramat.AI;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Suar Sinyal Darurat: menembakkan cahaya & kepulan asap merah ke langit 90 detik, mengusir hantu di radius 25 m.
    public class SuarSinyalDarurat : MonoBehaviour, IInteractable, ISaveable
    {
        public const float LamaNyala = 90f, RadiusUsirHantu = 25f;

        [SerializeField] Light cahayaSuar;
        [SerializeField] MeshRenderer apiSuar;
        bool _aktif;
        float _sisa, _tAsap;

        public bool Aktif { get { return _aktif; } }
        public float HoldDuration { get { return 0.5f; } }
        public string SaveKey { get { return "suar_sinyal_" + Mathf.RoundToInt(transform.position.x * 10f) + "_" + Mathf.RoundToInt(transform.position.z * 10f); } }

        public void Pasang(Light l, MeshRenderer api) { cahayaSuar = l; apiSuar = api; Segarkan(); }
        public string GetPromptText(GameObject actor) { return _aktif ? "SUAR SINYAL MENYALA (" + Mathf.CeilToInt(_sisa) + " s)" : "NYALAKAN SUAR SINYAL DARURAT"; }
        public bool CanInteract(GameObject actor) { return !_aktif; }

        public void Interact(GameObject actor)
        {
            if (_aktif) return;
            _aktif = true; _sisa = LamaNyala;
            Segarkan();
            GameEvents.RaiseToast("Suar sinyal merah menyala ke langit! Hantu menjauh dalam radius 25 m.");
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.Play(ProceduralAudioSynth.KlipLepasPanah, transform.position, AudioCategory.Efek, 0.8f, 1.5f, 2, 50f);
        }

        void Update()
        {
            if (!_aktif || JedaDunia.Dijeda) return;
            float dt = Time.deltaTime;
            _sisa -= dt;
            _tAsap -= dt;
            if (_tAsap <= 0f)
            {
                _tAsap = 0.25f;
                var gore = GameServices.Get<GoreSystem>();
                if (gore != null) gore.PercikPartikel(transform.position + Vector3.up * 0.7f, new Color(0.95f, 0.16f, 0.12f, 0.82f), 5, 0.28f, 2.6f, 1.8f);
                var hs = HantuAI.Semua;
                for (int i = 0; i < hs.Count; i++)
                {
                    var h = hs[i];
                    if (h != null && h.IsAlive && (h.transform.position - transform.position).sqrMagnitude <= RadiusUsirHantu * RadiusUsirHantu)
                        h.Larutkan();
                }
            }
            if (cahayaSuar != null) cahayaSuar.intensity = 4.2f + Mathf.Sin(Time.time * 18f) * 0.8f;
            if (_sisa <= 0f) { _aktif = false; Segarkan(); GameEvents.RaiseToast("Suar sinyal darurat padam."); }
        }

        void Segarkan()
        {
            if (cahayaSuar != null) cahayaSuar.enabled = _aktif;
            if (apiSuar != null) apiSuar.enabled = _aktif;
        }

        [System.Serializable] struct DataSinyal { public bool a; public float s; }
        public string CaptureJson() { return JsonUtility.ToJson(new DataSinyal { a = _aktif, s = _sisa }); }
        public void RestoreJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            var d = JsonUtility.FromJson<DataSinyal>(json);
            _aktif = d.a && d.s > 0f; _sisa = d.s;
            Segarkan();
        }
    }
}
