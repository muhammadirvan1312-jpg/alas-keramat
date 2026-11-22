using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Perangkap survival aktif setara SOTF: jerat hewan hutan (hasil buruan otomatis) & perangkap duri pertahanan sekte.
    public class PerangkapSurvival : MonoBehaviour, IInteractable, ISaveable
    {
        public enum JenisPerangkap { JeratHewan = 0, DuriSekte = 1 }
        public enum StatusPerangkap { Siap = 0, Terpicu = 1, DapatBuruan = 2 }

        public const float RadiusPicu = 2.1f, DamageDuri = 48f, LamaReset = 1.0f, JedaTangkapPasif = 95f;

        [SerializeField] JenisPerangkap jenis;
        [SerializeField] Transform bagianGerak;
        [SerializeField] GameObject visualBuruan;

        StatusPerangkap _status = StatusPerangkap.Siap;
        float _tCek, _tBuruan;
        AndiFPSController _andi;

        public JenisPerangkap Jenis { get { return jenis; } }
        public StatusPerangkap Status { get { return _status; } }
        public float HoldDuration { get { return _status == StatusPerangkap.DapatBuruan ? 0f : LamaReset; } }
        public string SaveKey { get { return "perangkap_" + Mathf.RoundToInt(transform.position.x * 10f) + "_" + Mathf.RoundToInt(transform.position.z * 10f); } }

        public void Pasang(JenisPerangkap j, Transform gerak, GameObject buruan)
        {
            jenis = j; bagianGerak = gerak; visualBuruan = buruan;
            SegarkanVisual();
        }

        public static float DamageUntukTarget(JenisPerangkap j, bool siap, bool musuhHidup)
        {
            return (j == JenisPerangkap.DuriSekte && siap && musuhHidup) ? DamageDuri : 0f;
        }

        public string GetPromptText(GameObject actor)
        {
            if (_status == StatusPerangkap.DapatBuruan) return "AMBIL HASIL PERANGKAP";
            if (_status == StatusPerangkap.Terpicu) return "PASANG ULANG PERANGKAP";
            return "";
        }

        public bool CanInteract(GameObject actor) { return _status != StatusPerangkap.Siap; }

        public void Interact(GameObject actor)
        {
            if (_status == StatusPerangkap.DapatBuruan)
            {
                var inv = GameServices.Get<TikarPandan3DInventory>();
                if (inv == null) return;
                int a = inv.Tambah(ItemID.DagingMentah, 2);
                int b = inv.Tambah(ItemID.KulitHewan, 1);
                int c = inv.Tambah(ItemID.Tulang, 1);
                if (a + b + c <= 0) { GameEvents.RaiseToast("Tas penuh."); return; }
                _status = StatusPerangkap.Terpicu;
                SegarkanVisual();
                GameEvents.RaiseToast("Hasil perangkap: Daging Mentah x" + a + ", Kulit x" + b + ", Tulang x" + c);
                var synth = GameServices.Get<ProceduralAudioSynth>();
                if (synth != null) synth.PlayItemPickup();
                return;
            }
            if (_status == StatusPerangkap.Terpicu)
            {
                _status = StatusPerangkap.Siap;
                _tBuruan = 0f;
                SegarkanVisual();
                GameEvents.RaiseToast(jenis == JenisPerangkap.JeratHewan ? "Jerat hewan dipasang ulang." : "Perangkap duri dipasang ulang.");
            }
        }

        void Update()
        {
            if (_status != StatusPerangkap.Siap || JedaDunia.Dijeda) return;
            float dt = Time.deltaTime;
            _tCek -= dt;
            if (_tCek > 0f) return;
            _tCek = 0.25f;

            Vector3 p = transform.position;
            if (jenis == JenisPerangkap.DuriSekte)
            {
                var daftar = NusantaraEnemyAndMutantAI.Semua;
                for (int i = 0; i < daftar.Count; i++)
                {
                    var m = daftar[i];
                    if (m == null || !m.IsAlive) continue;
                    Vector3 d = m.transform.position - p; d.y = 0f;
                    if (d.sqrMagnitude > RadiusPicu * RadiusPicu) continue;
                    m.TakeDamage(new DamageInfo { amount = DamageDuri, type = DamageType.Tusuk, point = m.transform.position + Vector3.up * 0.8f, direction = Vector3.up, isHeavy = true, source = gameObject });
                    var gore = GameServices.Get<GoreSystem>();
                    if (gore != null) gore.Semburan(m.transform.position + Vector3.up * 0.7f, Vector3.up, DamageDuri, false, true);
                    _status = StatusPerangkap.Terpicu;
                    SegarkanVisual();
                    GameEvents.RaiseToast("Perangkap duri menghantam musuh!");
                    return;
                }
                return;
            }

            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            bool jauhDariAndi = _andi == null || (_andi.transform.position - p).sqrMagnitude > 18f * 18f;
            if (jauhDariAndi)
            {
                _tBuruan += 0.25f;
                if (_tBuruan >= JedaTangkapPasif)
                {
                    _status = StatusPerangkap.DapatBuruan;
                    SegarkanVisual();
                }
            }
        }

        void SegarkanVisual()
        {
            if (bagianGerak != null)
                bagianGerak.localRotation = _status == StatusPerangkap.Siap ? Quaternion.Euler(-48f, 0f, 0f) : Quaternion.identity;
            if (visualBuruan != null) visualBuruan.SetActive(_status == StatusPerangkap.DapatBuruan);
        }

        [System.Serializable] struct DataPerangkap { public int s; }
        public string CaptureJson() { return JsonUtility.ToJson(new DataPerangkap { s = (int)_status }); }
        public void RestoreJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            _status = (StatusPerangkap)Mathf.Clamp(JsonUtility.FromJson<DataPerangkap>(json).s, 0, 2);
            SegarkanVisual();
        }
    }
}
