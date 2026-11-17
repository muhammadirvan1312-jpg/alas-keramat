using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.Player
{
    // T5a: melee dasar golok/kapak (06 §2): tebasan ringan & berat, frame aktif, stamina, jangkauan; menebang pohon & melukai IDamageable.
    // Senjata aktif = yang terbaik di inventaris (kapak baja > kapak batu > golok > tangan kosong). Tangkis, lempar & panah menyusul T7.
    public class CombatAndLeftHandController : MonoBehaviour
    {
        public struct Senjata
        {
            public ItemID id; public float ringan, berat, tebang, jangkau, durR, durB, aktifR, aktifB, stamR, stamB; public DamageType tipe;
        }

        public const float PengaliTebangBerat = 1.5f, RadiusSapuan = 0.25f;
        public static readonly Senjata TanganKosong = new Senjata { id = ItemID.None, ringan = 6f, berat = 12f, tebang = 2f, jangkau = 1.2f, durR = 0.4f, durB = 0.9f, aktifR = 0.12f, aktifB = 0.45f, stamR = 3f, stamB = 8f, tipe = DamageType.Tumpul };
        public static readonly Senjata[] Tabel =
        {
            new Senjata { id = ItemID.KapakBaja, ringan = 26f, berat = 60f, tebang = 40f, jangkau = 1.8f, durR = 0.5f, durB = 1.05f, aktifR = 0.15f, aktifB = 0.55f, stamR = 6f, stamB = 15f, tipe = DamageType.Tebas },
            new Senjata { id = ItemID.KapakBatu, ringan = 20f, berat = 48f, tebang = 28f, jangkau = 1.7f, durR = 0.55f, durB = 1.1f, aktifR = 0.15f, aktifB = 0.55f, stamR = 6f, stamB = 15f, tipe = DamageType.Tebas },
            new Senjata { id = ItemID.GolokPusaka, ringan = 18f, berat = 45f, tebang = 20f, jangkau = 1.8f, durR = 0.45f, durB = 1.0f, aktifR = 0.15f, aktifB = 0.55f, stamR = 5f, stamB = 14f, tipe = DamageType.Tebas }
        };

        [SerializeField] Transform[] modelSenjata = new Transform[0]; // urutan sama dengan Tabel; null = tidak ada model
        [SerializeField] Transform pivotFP;

        AndiFPSController _ctrl;
        AndiVitalsAndSanity _v;
        TikarPandan3DInventory _inv;
        Senjata _aktif = TanganKosong;
        float _t = -1f, _tCekSenjata;
        bool _berat, _sudahKena;
        int _mask;
        Quaternion _restPivot = Quaternion.identity;

        public Senjata Aktif { get { return _aktif; } }
        public bool SedangMenyerang { get { return _t >= 0f; } }

        public void Pasang(Transform pivot, Transform[] model) { pivotFP = pivot; modelSenjata = model; }

        void Awake()
        {
            GameServices.Register(this);
            _ctrl = GetComponent<AndiFPSController>();
            _mask = (1 << GameLayers.Tree) | (1 << GameLayers.Enemy) | (1 << GameLayers.LogPhysics) | (1 << GameLayers.Structure) | 1; // layer 0 = Default
            if (pivotFP != null) _restPivot = pivotFP.localRotation;
        }

        public static Senjata Terbaik(System.Func<ItemID, bool> punya)
        {
            for (int i = 0; i < Tabel.Length; i++) if (punya(Tabel[i].id)) return Tabel[i];
            return TanganKosong;
        }

        void PilihSenjata()
        {
            if (_inv == null) _inv = GameServices.Get<TikarPandan3DInventory>();
            var baru = _inv != null ? Terbaik(id => _inv.Punya(id)) : TanganKosong;
            if (baru.id == _aktif.id && _tCekSenjata > 0f) return;
            _aktif = baru;
            for (int i = 0; i < modelSenjata.Length && i < Tabel.Length; i++)
                if (modelSenjata[i] != null) modelSenjata[i].gameObject.SetActive(Tabel[i].id == _aktif.id);
        }

        public void Tebas(bool berat)
        {
            if (SedangMenyerang || (_ctrl != null && _ctrl.Berenang)) return;
            if (_v == null) _v = GameServices.Get<AndiVitalsAndSanity>();
            if (_v != null && !_v.Pakai(berat ? _aktif.stamB : _aktif.stamR)) { GameEvents.RaiseToast("Terlalu lelah untuk mengayun"); return; }
            _berat = berat; _t = 0f; _sudahKena = false;
        }

        void Update()
        {
            _tCekSenjata -= Time.deltaTime;
            if (_tCekSenjata <= 0f) { PilihSenjata(); _tCekSenjata = 0.5f; }
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetMouseButtonDown(0)) Tebas(false);
            if (Input.GetKeyDown(KeyCode.Q)) Tebas(true);
#endif
            if (_t < 0f) { if (pivotFP != null) pivotFP.localRotation = Quaternion.Slerp(pivotFP.localRotation, _restPivot, Time.deltaTime * 10f); return; }
            _t += Time.deltaTime;
            float dur = _berat ? _aktif.durB : _aktif.durR, aktif = _berat ? _aktif.aktifB : _aktif.aktifR;
            if (!_sudahKena && _t >= aktif) { _sudahKena = true; Kenai(); }
            if (pivotFP != null)
            {
                float u = Mathf.Clamp01(_t / dur);
                float ayun = u < aktif / dur ? Mathf.Lerp(0f, -70f, u / (aktif / dur)) : Mathf.Lerp(-70f, 60f, (u - aktif / dur) / (1f - aktif / dur));
                pivotFP.localRotation = _restPivot * Quaternion.Euler(-ayun * (_berat ? 1.3f : 1f), 0f, ayun * 0.4f);
            }
            if (_t >= dur) _t = -1f;
        }

        void Kenai()
        {
            var cam = _ctrl != null ? _ctrl.Kamera : null;
            if (cam == null) return;
            RaycastHit hit;
            if (!Physics.SphereCast(cam.position, RadiusSapuan, cam.forward, out hit, _aktif.jangkau + 0.3f, _mask, QueryTriggerInteraction.Ignore)) return;
            var kayu = hit.collider.GetComponent<KayuFisik>();
            if (kayu != null && _aktif.id != ItemID.None) { kayu.Pukul(); return; }
            var bagian = hit.collider.GetComponentInParent<BagianStruktur>();
            if (bagian != null && bagian.Jenis == BagianStruktur.Tipe.Tiang && hit.point.y > bagian.Puncak.y - 0.5f && _aktif.id != ItemID.None) { ShoulderLogAndFreeformBuilder.Serut(bagian); return; }
            var pohon = hit.collider.GetComponentInParent<DirectionalTree>();
            if (pohon != null) { pohon.Pukul(transform.position, _aktif.tebang * (_berat ? PengaliTebangBerat : 1f)); return; }
            var target = hit.collider.GetComponentInParent<IDamageable>();
            if (target != null && !ReferenceEquals(target, GetComponent<IDamageable>()))
                target.TakeDamage(new DamageInfo { amount = _berat ? _aktif.berat : _aktif.ringan, type = _aktif.tipe, point = hit.point, direction = cam.forward, source = gameObject, isHeavy = _berat });
        }
    }
}
