using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.World
{
    // Tungku api batu & pelita kuno yang dapat dinyalakan permanen: menerangi candi/pasar/desa di malam hari,
    // menghangatkan tubuh, dan menjaga kewarasan Andi saat berada di sekitar nyala api.
    public class TungkuApiCandi : MonoBehaviour, IInteractable, ISaveable
    {
        [SerializeField] string idTungku = "tungku_0";
        [SerializeField] GameObject nyalaObj;
        [SerializeField] Light cahayaApi;

        bool _menyala;
        float _tHangat;

        public bool Menyala { get { return _menyala; } }
        public string SaveKey { get { return "tungku_api_" + idTungku; } }
        public float HoldDuration { get { return 0f; } }

        public void Pasang(string id, GameObject nyala, Light lampu, bool nyalaAwal)
        {
            idTungku = string.IsNullOrEmpty(id) ? "tungku_0" : id;
            nyalaObj = nyala;
            cahayaApi = lampu;
            _menyala = nyalaAwal;
            TerapkanVisual();
        }

        public string GetPromptText(GameObject actor)
        {
            return _menyala ? "" : "NYALAKAN TUNGKU API KUNO";
        }

        public bool CanInteract(GameObject actor) { return !_menyala; }

        public void Interact(GameObject actor)
        {
            if (_menyala) return;
            _menyala = true;
            TerapkanVisual();
            var vital = GameServices.Get<AndiVitalsAndSanity>();
            if (vital != null) vital.UbahSanity(6f);
            var gore = GameServices.Get<GoreSystem>();
            if (gore != null) gore.PercikPartikel(transform.position + Vector3.up * 1.1f, new Color(1f, 0.58f, 0.16f, 0.9f), 10, 0.08f, 1.9f, 0.6f);
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.Play(ProceduralAudioSynth.KlipAmbilKayu, transform.position, AudioCategory.Efek, 0.7f, 1.1f, 1);
            GameEvents.RaiseToast("Tungku api kuno menyala! Cahaya mengusir gulita malam.");
        }

        void TerapkanVisual()
        {
            if (nyalaObj != null) nyalaObj.SetActive(_menyala);
            if (cahayaApi != null) cahayaApi.enabled = _menyala;
        }

        void Update()
        {
            if (!_menyala || JedaDunia.Dijeda) return;
            if (cahayaApi != null && cahayaApi.enabled)
                cahayaApi.intensity = 2.6f + 0.35f * Mathf.Sin(Time.time * 7.8f + transform.position.x);
            if (nyalaObj != null)
            {
                float s = 0.9f + 0.16f * Mathf.Sin(Time.time * 11f + transform.position.z);
                nyalaObj.transform.localScale = new Vector3(0.38f * s, 0.48f / s, 0.38f * s);
            }

            _tHangat += Time.deltaTime;
            if (_tHangat < 2f) return;
            _tHangat = 0f;
            var andi = GameServices.Get<AndiFPSController>();
            if (andi != null && (andi.transform.position - transform.position).sqrMagnitude <= 36f)
            {
                var vital = GameServices.Get<AndiVitalsAndSanity>();
                if (vital != null) vital.UbahSanity(0.6f);
            }
        }

        [System.Serializable] struct DataTungku { public bool m; }
        public string CaptureJson() { return JsonUtility.ToJson(new DataTungku { m = _menyala }); }
        public void RestoreJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            _menyala = JsonUtility.FromJson<DataTungku>(json).m;
            TerapkanVisual();
        }
    }
}
