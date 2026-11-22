using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.World
{
    // Titik interaksi kehidupan dunia di 22 landmark pulau:
    // - SesajenKeramat: menyalakan dupa/doa -> pulihkan Sanity (+18), Stamina (+14), & redam ReputasiSekte (-8)
    // - PohonBuahGuncang: mengguncang pohon kelapa/pisang hidup -> batang bergoyang & panen buah segar ke tas
    // - ApiUnggunPenyintas: menghangatkan diri di api unggun hidup -> pulihkan HP (+24), Stamina (+30) & keringkan tubuh
    public class TitikKehidupanInteraktif : MonoBehaviour, IInteractable
    {
        public enum JenisTitik { SesajenKeramat = 0, PohonBuahGuncang = 1, ApiUnggunPenyintas = 2 }
        public const float JedaPakai = 40f;

        [SerializeField] JenisTitik jenis = JenisTitik.SesajenKeramat;
        [SerializeField] string namaLokasi = "Rimba";
        [SerializeField] Transform targetGerak;
        [SerializeField] Light cahayaEfek;

        float _siapPada = -99f;
        float _goyangT;
        Quaternion _rotAwal = Quaternion.identity;

        public JenisTitik JenisAktif { get { return jenis; } }
        public float HoldDuration { get { return 0f; } }

        public void Pasang(JenisTitik j, string lokasi, Transform gerak, Light cahaya)
        {
            jenis = j;
            namaLokasi = string.IsNullOrEmpty(lokasi) ? "Pulau" : lokasi;
            targetGerak = gerak;
            cahayaEfek = cahaya;
        }

        public static string TeksPromptUntuk(JenisTitik j, string lokasi, bool siap)
        {
            if (!siap) return "Menunggu pulih (" + lokasi + ")";
            if (j == JenisTitik.SesajenKeramat) return "NYALAKAN DUPA & DOA (" + lokasi + ")";
            if (j == JenisTitik.PohonBuahGuncang) return "GUNCANG POHON BUAH (" + lokasi + ")";
            return "HANGATKAN DIRI DI API (" + lokasi + ")";
        }

        void Awake()
        {
            if (targetGerak != null) _rotAwal = targetGerak.localRotation;
        }

        void Update()
        {
            if (_goyangT > 0f)
            {
                _goyangT -= Time.deltaTime;
                if (targetGerak != null)
                {
                    float s = Mathf.Sin(_goyangT * 26f) * 5.5f * Mathf.Clamp01(_goyangT);
                    targetGerak.localRotation = _rotAwal * Quaternion.Euler(s, 0f, s * 0.7f);
                }
            }
            else if (targetGerak != null && targetGerak.localRotation != _rotAwal)
            {
                targetGerak.localRotation = _rotAwal;
            }
        }

        public string GetPromptText(GameObject actor)
        {
            return TeksPromptUntuk(jenis, namaLokasi, Time.time >= _siapPada);
        }

        public bool CanInteract(GameObject actor)
        {
            return Time.time >= _siapPada;
        }

        public void Interact(GameObject actor)
        {
            if (Time.time < _siapPada) return;
            _siapPada = Time.time + JedaPakai;
            _goyangT = 1.15f;

            var vital = GameServices.Get<AndiVitalsAndSanity>();
            if (jenis == JenisTitik.SesajenKeramat)
            {
                if (vital != null) { vital.UbahSanity(18f); vital.TambahStamina(14f); }
                var rep = GameServices.Get<ReputasiSekte>();
                if (rep != null) rep.TambahAncaman(-8f);
                if (cahayaEfek != null) cahayaEfek.intensity = 4.2f;
                GameEvents.RaiseToast("Dupa menyala di " + namaLokasi + ". Batin tenang & aura sekte mereda.");
            }
            else if (jenis == JenisTitik.PohonBuahGuncang)
            {
                if (vital != null) { vital.Minum(22f, false); vital.TambahStamina(18f); vital.TambahHealth(10f); }
                var inv = GameServices.Get<TikarPandan3DInventory>();
                if (inv != null) inv.Tambah(ItemID.KelapaMuda, 1);
                GameEvents.RaiseToast("Memanen kelapa segar dari pohon di " + namaLokasi + "!");
            }
            else
            {
                if (vital != null) { vital.TambahHealth(24f); vital.TambahStamina(30f); vital.UbahSanity(12f); vital.SetDekatApi(true); }
                if (cahayaEfek != null) cahayaEfek.intensity = 4.8f;
                GameEvents.RaiseToast("Menghangatkan diri di api unggun " + namaLokasi + ". Tenaga pulih!");
            }
        }
    }
}
