using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Zipline / Tali Luncur ikonik SOTF: meluncur kencang antar bukit & lembah sepanjang kabel baja.
    public class ZiplineRimba : MonoBehaviour, IInteractable
    {
        public const float KecepatanLuncur = 18.5f, LenturanKabel = 1.35f;

        [SerializeField] Vector3 titikAwal, titikAkhir;
        [SerializeField] string namaJalur = "Jalur Zipline";

        bool _sedangLuncur;
        float _jarakTotal, _progres;
        AndiFPSController _andi;
        CharacterController _cc;

        public bool SedangLuncur { get { return _sedangLuncur; } }
        public float PanjangJalur { get { return Vector3.Distance(titikAwal, titikAkhir); } }
        public float HoldDuration { get { return 0f; } }

        public void Pasang(Vector3 awal, Vector3 akhir, string nama)
        {
            titikAwal = awal; titikAkhir = akhir;
            namaJalur = string.IsNullOrEmpty(nama) ? "Jalur Zipline" : nama;
            _jarakTotal = Mathf.Max(1f, Vector3.Distance(awal, akhir));
        }

        public static Vector3 PosisiDiKabel(Vector3 awal, Vector3 akhir, float u)
        {
            u = Mathf.Clamp01(u);
            Vector3 p = Vector3.Lerp(awal, akhir, u);
            p.y -= Mathf.Sin(u * Mathf.PI) * LenturanKabel;
            return p;
        }

        public string GetPromptText(GameObject actor)
        {
            return _sedangLuncur ? "" : "MELUNCUR ZIPLINE: " + namaJalur + " (" + Mathf.RoundToInt(PanjangJalur) + " m)";
        }

        public bool CanInteract(GameObject actor) { return !_sedangLuncur; }

        public void Interact(GameObject actor)
        {
            if (_sedangLuncur) return;
            _andi = GameServices.Get<AndiFPSController>();
            if (_andi == null) return;
            _cc = _andi.GetComponent<CharacterController>();
            _jarakTotal = Mathf.Max(1f, Vector3.Distance(titikAwal, titikAkhir));
            _progres = 0f;
            _sedangLuncur = true;
            _andi.HentikanInputGerak();
            if (_cc != null) _cc.enabled = false;
            GameEvents.RaiseToast("Meluncur di " + namaJalur + "!");
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.Play(ProceduralAudioSynth.KlipLepasPanah, transform.position, AudioCategory.Efek, 0.7f, 1.35f, 2);
        }

        void Update()
        {
            if (!_sedangLuncur || _andi == null || JedaDunia.Dijeda) return;
            _progres += (KecepatanLuncur * Time.deltaTime) / Mathf.Max(1f, _jarakTotal);
            Vector3 p = PosisiDiKabel(titikAwal, titikAkhir, _progres) - Vector3.up * 1.45f;
            _andi.transform.position = p;
            if (_progres >= 1f)
            {
                _sedangLuncur = false;
                Vector3 darat = titikAkhir;
                darat.y = WorldTerrainGenerator.SampleHeight(darat.x, darat.z) + 0.15f;
                _andi.transform.position = darat;
                if (_cc != null) _cc.enabled = true;
                _andi.GuncangKamera(0.14f);
                GameEvents.RaiseToast("Mendarat di ujung " + namaJalur + ".");
            }
        }
    }
}
