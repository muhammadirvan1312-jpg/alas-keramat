using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Rak jemuran daging & ikan khas SOTF: gantung DagingMentah/IkanMentah -> mengering 90 detik -> DagingKering/IkanKering tahan lama.
    public class RakPengeringDanMasak : MonoBehaviour, IInteractable, ISaveable
    {
        public const float LamaKering = 90f;
        public enum IsiRak { Kosong = 0, DagingMentah = 1, IkanMentah = 2, DagingKering = 3, IkanKering = 4 }

        [SerializeField] GameObject[] stripVisual = new GameObject[0];
        IsiRak _isi = IsiRak.Kosong;
        float _progres;
        URPAtmosphereAndWeather _cuaca;

        public IsiRak Isi { get { return _isi; } }
        public float HoldDuration { get { return 0.4f; } }
        public string SaveKey { get { return "rak_jemur_" + Mathf.RoundToInt(transform.position.x * 10f) + "_" + Mathf.RoundToInt(transform.position.z * 10f); } }

        public void Pasang(GameObject[] s) { stripVisual = s ?? new GameObject[0]; Segarkan(); }

        public static float LajuKering(WeatherState w)
        {
            if (w == WeatherState.HujanBadai || w == WeatherState.Gerimis) return 0.15f;
            if (w == WeatherState.Cerah) return 1.25f;
            return 0.85f;
        }

        public static ItemID HasilKering(IsiRak r)
        {
            return r == IsiRak.IkanKering ? ItemID.IkanKering : ItemID.DagingKering;
        }

        public string GetPromptText(GameObject actor)
        {
            if (_isi == IsiRak.DagingKering) return "AMBIL DAGING KERING";
            if (_isi == IsiRak.IkanKering) return "AMBIL IKAN KERING";
            if (_isi == IsiRak.DagingMentah || _isi == IsiRak.IkanMentah) return "MENGERING (" + Mathf.RoundToInt(Mathf.Clamp01(_progres / LamaKering) * 100f) + "%)";
            return "GANTUNG DAGING / IKAN";
        }

        public bool CanInteract(GameObject actor)
        {
            return _isi == IsiRak.Kosong || _isi == IsiRak.DagingKering || _isi == IsiRak.IkanKering;
        }

        public void Interact(GameObject actor)
        {
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv == null) return;
            var synth = GameServices.Get<ProceduralAudioSynth>();

            if (_isi == IsiRak.DagingKering || _isi == IsiRak.IkanKering)
            {
                var id = HasilKering(_isi);
                if (inv.Tambah(id, 1) <= 0) { GameEvents.RaiseToast("Tas penuh."); return; }
                _isi = IsiRak.Kosong; _progres = 0f;
                Segarkan();
                GameEvents.RaiseToast("Mengambil " + ItemDatabase.Get(id).displayName + ".");
                if (synth != null) synth.PlayItemPickup();
                return;
            }
            if (_isi == IsiRak.Kosong)
            {
                if (inv.Kurangi(ItemID.DagingMentah, 1))
                {
                    _isi = IsiRak.DagingMentah; _progres = 0f; Segarkan();
                    GameEvents.RaiseToast("Daging digantung di rak jemuran.");
                }
                else if (inv.Kurangi(ItemID.IkanMentah, 1))
                {
                    _isi = IsiRak.IkanMentah; _progres = 0f; Segarkan();
                    GameEvents.RaiseToast("Ikan digantung di rak jemuran.");
                }
                else GameEvents.RaiseToast("Butuh Daging Mentah atau Ikan Mentah.");
            }
        }

        void Update()
        {
            if ((_isi != IsiRak.DagingMentah && _isi != IsiRak.IkanMentah) || JedaDunia.Dijeda) return;
            if (_cuaca == null) _cuaca = GameServices.Get<URPAtmosphereAndWeather>();
            var w = _cuaca != null ? _cuaca.Cuaca : WeatherState.Cerah;
            _progres += Time.deltaTime * LajuKering(w);
            if (_progres >= LamaKering)
            {
                _isi = _isi == IsiRak.IkanMentah ? IsiRak.IkanKering : IsiRak.DagingKering;
                Segarkan();
            }
        }

        void Segarkan()
        {
            if (stripVisual == null) return;
            bool ada = _isi != IsiRak.Kosong;
            for (int i = 0; i < stripVisual.Length; i++)
                if (stripVisual[i] != null) stripVisual[i].SetActive(ada);
        }

        [System.Serializable] struct DataRak { public int isi; public float p; }
        public string CaptureJson() { return JsonUtility.ToJson(new DataRak { isi = (int)_isi, p = _progres }); }
        public void RestoreJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            var d = JsonUtility.FromJson<DataRak>(json);
            _isi = (IsiRak)Mathf.Clamp(d.isi, 0, 4);
            _progres = Mathf.Max(0f, d.p);
            Segarkan();
        }
    }
}
