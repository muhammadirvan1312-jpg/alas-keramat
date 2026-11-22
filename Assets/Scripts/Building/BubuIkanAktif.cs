using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Bubu ikan bambu di tepi sungai & pantai: menangkap ikan mentah secara pasif (maks 3 ekor), tersimpan di SaveSystem.
    public class BubuIkanAktif : MonoBehaviour, IInteractable, ISaveable
    {
        public const int MaksIkan = 3;
        public const float JedaTangkap = 75f;

        [SerializeField] GameObject[] ikanVisual = new GameObject[0];
        int _jumlah = 1;
        float _t;

        public int JumlahIkan { get { return _jumlah; } }
        public float HoldDuration { get { return 0f; } }
        public string SaveKey { get { return "bubu_" + Mathf.RoundToInt(transform.position.x * 10f) + "_" + Mathf.RoundToInt(transform.position.z * 10f); } }

        public void Pasang(GameObject[] visual) { ikanVisual = visual ?? new GameObject[0]; Segarkan(); }

        public static int HitungTangkap(int awal, float waktuDetik)
        {
            int tambah = Mathf.FloorToInt(Mathf.Max(0f, waktuDetik) / JedaTangkap);
            return Mathf.Clamp(awal + tambah, 0, MaksIkan);
        }

        public string GetPromptText(GameObject actor) { return _jumlah > 0 ? "AMBIL IKAN BUBU (" + _jumlah + ")" : ""; }
        public bool CanInteract(GameObject actor) { return _jumlah > 0; }

        public void Interact(GameObject actor)
        {
            if (_jumlah <= 0) return;
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv == null) return;
            int dapat = inv.Tambah(ItemID.IkanMentah, _jumlah);
            if (dapat <= 0) { GameEvents.RaiseToast("Tas penuh."); return; }
            _jumlah -= dapat;
            Segarkan();
            GameEvents.RaiseToast("Mengambil " + dapat + " Ikan Mentah dari bubu.");
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.PlayItemPickup();
        }

        void Update()
        {
            if (_jumlah >= MaksIkan || JedaDunia.Dijeda) return;
            _t += Time.deltaTime;
            if (_t >= JedaTangkap)
            {
                _t -= JedaTangkap;
                _jumlah = Mathf.Min(MaksIkan, _jumlah + 1);
                Segarkan();
            }
        }

        void Segarkan()
        {
            if (ikanVisual == null) return;
            for (int i = 0; i < ikanVisual.Length; i++)
                if (ikanVisual[i] != null) ikanVisual[i].SetActive(i < _jumlah);
        }

        [System.Serializable] struct DataBubu { public int n; }
        public string CaptureJson() { return JsonUtility.ToJson(new DataBubu { n = _jumlah }); }
        public void RestoreJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            _jumlah = Mathf.Clamp(JsonUtility.FromJson<DataBubu>(json).n, 0, MaksIkan);
            Segarkan();
        }
    }
}
