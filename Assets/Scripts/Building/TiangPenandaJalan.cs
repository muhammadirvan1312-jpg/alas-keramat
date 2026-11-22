using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Tiang penanda jalan & suar rimba: Andi bisa menyalakan obor penanda agar rute antar zona mudah dilihat saat malam.
    public class TiangPenandaJalan : MonoBehaviour, IInteractable, ISaveable
    {
        [SerializeField] string namaLokasi = "Persimpangan Rimba";
        [SerializeField] MeshRenderer apiSuar;
        bool _menyala;

        public bool Menyala { get { return _menyala; } }
        public string NamaLokasi { get { return namaLokasi; } }
        public float HoldDuration { get { return 0f; } }
        public string SaveKey { get { return "suar_jalan_" + Mathf.RoundToInt(transform.position.x * 10f) + "_" + Mathf.RoundToInt(transform.position.z * 10f); } }

        public void Pasang(string nama, MeshRenderer api)
        {
            namaLokasi = string.IsNullOrEmpty(nama) ? "Persimpangan Rimba" : nama;
            apiSuar = api;
            Segarkan();
        }

        public string GetPromptText(GameObject actor)
        {
            return _menyala ? "PADAMKAN SUAR: " + namaLokasi : "NYALAKAN SUAR: " + namaLokasi;
        }

        public bool CanInteract(GameObject actor) { return true; }

        public void Interact(GameObject actor)
        {
            _menyala = !_menyala;
            Segarkan();
            GameEvents.RaiseToast(_menyala ? "Suar penanda menyala di " + namaLokasi + "." : "Suar penanda dipadamkan.");
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.PlayItemPickup();
        }

        void Segarkan()
        {
            if (apiSuar != null) apiSuar.enabled = _menyala;
        }

        [System.Serializable] struct DataSuar { public bool n; }
        public string CaptureJson() { return JsonUtility.ToJson(new DataSuar { n = _menyala }); }
        public void RestoreJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            _menyala = JsonUtility.FromJson<DataSuar>(json).n;
            Segarkan();
        }
    }
}
