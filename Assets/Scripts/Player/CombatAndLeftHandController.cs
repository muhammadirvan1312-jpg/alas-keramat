using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.Player
{
    // T5a: melee dasar golok/kapak (06 §2): tebasan ringan & berat, frame aktif, stamina, jangkauan; menebang pohon & melukai IDamageable.
    // Senjata aktif = yang terbaik di inventaris (kapak baja > kapak batu > golok > tangan kosong). Tangkis, lempar & panah menyusul T7.
    public partial class CombatAndLeftHandController : MonoBehaviour
    {
        public struct Senjata
        {
            public ItemID id; public float ringan, berat, tebang, jangkau, durR, durB, aktifR, aktifB, stamR, stamB; public DamageType tipe;
        }

        public const float PengaliTebangBerat = 1.5f, RadiusSapuan = 0.45f;
        public static readonly Senjata TanganKosong = new Senjata { id = ItemID.None, ringan = 6f, berat = 12f, tebang = 2f, jangkau = 1.2f, durR = 0.4f, durB = 0.9f, aktifR = 0.12f, aktifB = 0.45f, stamR = 3f, stamB = 8f, tipe = DamageType.Tumpul };
        public static readonly Senjata[] Tabel =
        {
            new Senjata { id = ItemID.GolokApiPusaka, ringan = 28f, berat = 70f, tebang = 24f, jangkau = 1.8f, durR = 0.45f, durB = 1.0f, aktifR = 0.15f, aktifB = 0.55f, stamR = 5f, stamB = 13f, tipe = DamageType.Tebas },
            new Senjata { id = ItemID.KerisPusaka, ringan = 30f, berat = 75f, tebang = 10f, jangkau = 1.6f, durR = 0.4f, durB = 0.95f, aktifR = 0.13f, aktifB = 0.5f, stamR = 4f, stamB = 12f, tipe = DamageType.Tebas },
            new Senjata { id = ItemID.Mandau, ringan = 24f, berat = 56f, tebang = 18f, jangkau = 1.8f, durR = 0.42f, durB = 0.95f, aktifR = 0.14f, aktifB = 0.52f, stamR = 5f, stamB = 13f, tipe = DamageType.Tebas },
            new Senjata { id = ItemID.KapakBaja, ringan = 26f, berat = 60f, tebang = 40f, jangkau = 1.8f, durR = 0.5f, durB = 1.05f, aktifR = 0.15f, aktifB = 0.55f, stamR = 6f, stamB = 15f, tipe = DamageType.Tebas },
            new Senjata { id = ItemID.KapakBatu, ringan = 20f, berat = 48f, tebang = 28f, jangkau = 1.7f, durR = 0.55f, durB = 1.1f, aktifR = 0.15f, aktifB = 0.55f, stamR = 6f, stamB = 15f, tipe = DamageType.Tebas },
            new Senjata { id = ItemID.GolokPusaka, ringan = 18f, berat = 45f, tebang = 20f, jangkau = 1.8f, durR = 0.45f, durB = 1.0f, aktifR = 0.15f, aktifB = 0.55f, stamR = 5f, stamB = 14f, tipe = DamageType.Tebas },
            new Senjata { id = ItemID.Cangkul, ringan = 14f, berat = 30f, tebang = 8f, jangkau = 1.9f, durR = 0.6f, durB = 1.2f, aktifR = 0.2f, aktifB = 0.6f, stamR = 6f, stamB = 15f, tipe = DamageType.Tumpul }
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

        float _tajam;
        public void Asah() { if (_aktif.id == ItemID.None) { GameEvents.RaiseToast("Pegang golok atau kapak"); return; } _tajam=300f; GameEvents.RaiseToast("Senjata diasah: daya tebas +10% selama 5 menit"); }
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
            Senjata baru; int jarak = -1;
            if (_pilihTanganKosong) baru = TanganKosong;
            else if (_pilihan != ItemID.None && _inv != null && _inv.Punya(_pilihan))
            {
                int m = IndeksMelee(_pilihan);
                if (m >= 0) baru = Tabel[m]; else { jarak = IndeksJarak(_pilihan); baru = TanganKosong; }
            }
            else baru = _inv != null ? Terbaik(id => _inv.Punya(id)) : TanganKosong;
            if (_adaKembalikan) return; // tusukan tombak sedang berlangsung
            if (baru.id == _aktif.id && jarak == _iJarak && _tCekSenjata > 0f) return;
            _aktif = baru; _iJarak = jarak;
            for (int i = 0; i < modelSenjata.Length && i < Tabel.Length; i++)
                if (modelSenjata[i] != null) modelSenjata[i].gameObject.SetActive(jarak < 0 && Tabel[i].id == _aktif.id);
            for (int i = 0; i < modelJarak.Length && i < TabelJarak.Length; i++)
                if (modelJarak[i] != null) modelJarak[i].gameObject.SetActive(jarak == i);
        }

        public void Tebas(bool berat)
        {
            if (AlasKeramat.UI.PengunciInput.Terblokir || SedangMenyerang || _tersimpan || _blok || (_ctrl != null && _ctrl.Berenang)) return;
            if (_v == null) _v = GameServices.Get<AndiVitalsAndSanity>();
            if (_v != null && !_v.Pakai(berat ? _aktif.stamB : _aktif.stamR)) { GameEvents.RaiseToast("Terlalu lelah untuk mengayun"); return; }
            _berat = berat; _t = 0f; _sudahKena = false;
        }

        public void HentikanAksiPanel()
        {
            BatalkanTebas(); TangkisLepas(); _tParry=-1f; _tElak=0f; _t=-1f;
            if (_ctrl != null) _ctrl.GuncangKamera(0f);
            if(_adaKembalikan) { _aktif=_kembalikan; _adaKembalikan=false; }
        }

        void Update()
        {
            _tajam=Mathf.Max(0f,_tajam-Time.deltaTime);
            if (AlasKeramat.UI.PengunciInput.Terblokir)
            {
                HentikanAksiPanel(); TickTangkis(Time.deltaTime); TickJarak(Time.deltaTime); return;
            }
            TickTangkis(Time.deltaTime);
            TickJarak(Time.deltaTime);
            _tCekSenjata -= Time.deltaTime;
            if (_tCekSenjata <= 0f) { PilihSenjata(); _tCekSenjata = 0.5f; }
#if ENABLE_LEGACY_INPUT_MANAGER && (UNITY_EDITOR || UNITY_STANDALONE)
            if (Input.GetMouseButtonDown(0)) TekanTebas();
            if (Input.GetMouseButtonUp(0)) LepasTebas();
            if (Input.GetKeyDown(KeyCode.G)) Ganti();
            if (Input.GetKeyDown(KeyCode.Q)) Tebas(true);
            if (Input.GetMouseButtonDown(1)) TangkisTekan();
            if (Input.GetMouseButtonUp(1)) TangkisLepas();
#endif
            if (_t < 0f) { if (pivotFP != null) pivotFP.localRotation = Quaternion.Slerp(pivotFP.localRotation, _blok ? _restPivot * PoseBlok : _restPivot, Time.deltaTime * (_blok ? 14f : 10f)); return; }
            _t += Time.deltaTime;
            float dur = _berat ? _aktif.durB : _aktif.durR, aktif = _berat ? _aktif.aktifB : _aktif.aktifR;
            if (!_sudahKena && _t < aktif) Assist(Time.deltaTime);
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
            if (!Physics.SphereCast(cam.position, RadiusSapuan, cam.forward, out hit, _aktif.jangkau + 0.7f, _mask, QueryTriggerInteraction.Ignore)) return;
            var kayu = hit.collider.GetComponent<KayuFisik>();
            if (kayu != null && _aktif.id != ItemID.None) { kayu.Pukul(); return; }
            var bagian = hit.collider.GetComponentInParent<BagianStruktur>();
            var builder=GameServices.Get<ShoulderLogAndFreeformBuilder>();
            if(builder!=null && builder.ModeCelah && _aktif.id!=ItemID.None && PotongCelah.Pukul(bagian,hit.point,builder.ModeAktif==ShoulderLogAndFreeformBuilder.ModeBangun.CelahPintu))return;
            if (bagian != null && bagian.Jenis == BagianStruktur.Tipe.Tiang && hit.point.y > bagian.Puncak.y - 0.5f && _aktif.id != ItemID.None) { ShoulderLogAndFreeformBuilder.Serut(bagian); return; }
            var pohon = hit.collider.GetComponentInParent<DirectionalTree>();
            if (pohon != null) { pohon.Pukul(transform.position, _aktif.tebang * (_berat ? PengaliTebangBerat : 1f)); AlasKeramat.AI.MeterSuara.Pancar(hit.point, AlasKeramat.AI.MeterSuara.Tebang); return; }
            var target = hit.collider.GetComponentInParent<IDamageable>();
            if (target != null && !ReferenceEquals(target, GetComponent<IDamageable>()))
                KenaiTarget(target, hit, cam, _tajam > 0f ? 1.1f : 1f);
        }
    }
}
