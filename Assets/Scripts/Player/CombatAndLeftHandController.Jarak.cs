using System.Collections.Generic;
using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.Player
{
    // T7d (06 §1-2, 17 §2): senjata jarak jauh & pilihan senjata manual (tombol GANTI), Elak, Bunuh Senyap.
    // Busur: tahan TEBAS = tarik, lepas = tembak. Tombak: ketuk = tusuk, tahan = bidik lalu lempar. Ketapel/suar/bedil/pistol/senapan: ketuk = tembak.
    public partial class CombatAndLeftHandController
    {
        public enum JenisJarak { Busur, Tombak, Ketapel, Suar, Hitscan, Bom }

        public struct SenjataJarak
        {
            public ItemID id; public JenisJarak jenis; public ItemID[] amunisi; public float damage, damageTarik, laju, lajuMaks, gravitasi, tarikMin, tarikMaks;
            public float isiUlang, sebaran, bunyi, jangkau, tusuk; public int butir, magasin; public string label;
        }

        public const float AmbangBidik = 0.35f, StaminaTarikPerDetik = 2f, TarikBebasStamina = 2f, JarakSenyap = 1.5f, SudutSenyap = 60f, BunyiSenyap = 4f;
        public const float JarakElak = 2.5f, LamaElak = 0.25f, StaminaElak = 12f, RadiusElakMusuh = 6f, PengaliSenyapKuat = 3f;

        public static readonly SenjataJarak[] TabelJarak =
        {
            new SenjataJarak { id = ItemID.BusurTanduk, jenis = JenisJarak.Busur, amunisi = new[] { ItemID.AnakPanah, ItemID.PanahTulang, ItemID.PanahApi, ItemID.PanahRacun }, damage = 30f * 1.25f, damageTarik = 20f * 1.25f, laju = 35f, lajuMaks = 55f, gravitasi = 1f, tarikMin = 0.35f, tarikMaks = 1.0f, bunyi = 6f, label = "TARIK" },
            new SenjataJarak { id = ItemID.BusurBambu, jenis = JenisJarak.Busur, amunisi = new[] { ItemID.AnakPanah, ItemID.PanahTulang, ItemID.PanahApi, ItemID.PanahRacun }, damage = 30f, damageTarik = 20f, laju = 35f, lajuMaks = 55f, gravitasi = 1f, tarikMin = 0.4f, tarikMaks = 1.2f, bunyi = 6f, label = "TARIK" },
            new SenjataJarak { id = ItemID.TombakLogam, jenis = JenisJarak.Tombak, amunisi = new[] { ItemID.TombakLogam }, damage = 70f, laju = 25f, gravitasi = 0.5f, tusuk = 28f, jangkau = 2.4f, bunyi = 5f, label = "LEMPAR" },
            new SenjataJarak { id = ItemID.TombakBambu, jenis = JenisJarak.Tombak, amunisi = new[] { ItemID.TombakBambu }, damage = 55f, laju = 25f, gravitasi = 0.5f, tusuk = 22f, jangkau = 2.4f, bunyi = 5f, label = "LEMPAR" },
            new SenjataJarak { id = ItemID.SenapanLaras, jenis = JenisJarak.Hitscan, amunisi = new[] { ItemID.PeluruSenapan }, damage = 12f, butir = 8, sebaran = 6f, jangkau = 40f, isiUlang = 2.5f, magasin = 2, bunyi = 140f, label = "TEMBAK" },
            new SenjataJarak { id = ItemID.BedilLocok, jenis = JenisJarak.Hitscan, amunisi = new[] { ItemID.PeluruLocok }, damage = 95f, butir = 1, sebaran = 1.5f, jangkau = 60f, isiUlang = 4.5f, magasin = 1, bunyi = 120f, label = "TEMBAK" },
            new SenjataJarak { id = ItemID.PistolPT, jenis = JenisJarak.Hitscan, amunisi = new[] { ItemID.PeluruPistol }, damage = 35f, butir = 1, sebaran = 1f, jangkau = 50f, isiUlang = 1.8f, magasin = 8, bunyi = 100f, label = "TEMBAK" },
            new SenjataJarak { id = ItemID.PistolSuar, jenis = JenisJarak.Suar, amunisi = new[] { ItemID.PelurSuar }, damage = 40f, laju = 45f, gravitasi = 0.6f, isiUlang = 2.5f, magasin = 1, bunyi = 30f, label = "TEMBAK" },
            new SenjataJarak { id = ItemID.Ketapel, jenis = JenisJarak.Ketapel, amunisi = new[] { ItemID.Batu }, damage = 10f, laju = 30f, gravitasi = 1f, bunyi = 3f, label = "TEMBAK" },
            new SenjataJarak { id = ItemID.BomBambuMinyak, jenis = JenisJarak.Bom, amunisi = new[] { ItemID.BomBambuMinyak }, damage = 20f, laju = 18f, gravitasi = 1f, bunyi = 4f, label = "LEMPAR" },
        };

        public static event System.Action OnKenaJarak; // tanda kena untuk proyektil
        public static void PicuKenaJarak() { var h = OnKenaJarak; if (h != null) h(); }

        [SerializeField] GameObject[] prefabProyektil = new GameObject[5]; // panah, tombak, batu, suar, bom

        ItemID _pilihan = ItemID.None;
        bool _pilihTanganKosong;
        [SerializeField] Transform[] modelJarak = new Transform[0]; // urutan sama dengan TabelJarak
        public void PasangModelJarak(Transform[] m) { modelJarak = m; }
        int _iJarak = -1;
        float _tTekan = -1f, _tIsi, _tElak;
        bool _menarik;
        Vector3 _arahElak;
        readonly Dictionary<ItemID, int> _isi = new Dictionary<ItemID, int>();

        public void PasangProyektil(GameObject[] p) { prefabProyektil = p; }
        public bool PakaiJarak { get { return _iJarak >= 0; } }
        public SenjataJarak AktifJarak { get { return _iJarak >= 0 ? TabelJarak[_iJarak] : default(SenjataJarak); } }
        public bool Kebal { get { return _tElak > 0f; } }

        public static int IndeksJarak(ItemID id) { for (int i = 0; i < TabelJarak.Length; i++) if (TabelJarak[i].id == id) return i; return -1; }
        public static int IndeksMelee(ItemID id) { for (int i = 0; i < Tabel.Length; i++) if (Tabel[i].id == id) return i; return -1; }

        // Damage busur menurut lama tarik (06 §2: 30 + 0-20 sesuai tarikan).
        public static float DamageTarik(SenjataJarak s, float lama) { return s.damage + s.damageTarik * Mathf.InverseLerp(s.tarikMin, s.tarikMaks, lama); }
        public static float LajuTarik(SenjataJarak s, float lama) { return Mathf.Lerp(s.laju, Mathf.Max(s.laju, s.lajuMaks), Mathf.InverseLerp(s.tarikMin, s.tarikMaks, lama)); }

        // Urutan GANTI: tangan kosong -> melee dimiliki -> jarak dimiliki.
        public void Ganti()
        {
            if (_inv == null) _inv = GameServices.Get<TikarPandan3DInventory>();
            var daftar = new List<ItemID> { ItemID.None };
            for (int i = 0; i < Tabel.Length; i++) if (_inv != null && _inv.Punya(Tabel[i].id)) daftar.Add(Tabel[i].id);
            for (int i = 0; i < TabelJarak.Length; i++) if (_inv != null && _inv.Punya(TabelJarak[i].id)) daftar.Add(TabelJarak[i].id);
            int k = daftar.IndexOf(_pilihTanganKosong ? ItemID.None : _iJarak >= 0 ? TabelJarak[_iJarak].id : _aktif.id);
            _pilihan = daftar[(k + 1) % daftar.Count];
            _pilihTanganKosong = _pilihan == ItemID.None;
            _tCekSenjata = 0f; PilihSenjata();
            GameEvents.RaiseToast("Senjata: " + NamaAktif());
        }

        public string NamaAktif()
        {
            if (_iJarak >= 0) return ItemDatabase.Get(TabelJarak[_iJarak].id).displayName;
            return _aktif.id == ItemID.None ? "Tangan kosong" : ItemDatabase.Get(_aktif.id).displayName;
        }

        public int Amunisi()
        {
            if (_iJarak < 0 || _inv == null) return -1;
            var s = TabelJarak[_iJarak]; int n = 0;
            foreach (var a in s.amunisi) n += _inv.Jumlah(a);
            if (s.magasin > 0) { int isi; _isi.TryGetValue(s.id, out isi); return isi + n; }
            return n;
        }

        // Label tombol TEBAS saat ini (BUNUH SENYAP / TARIK / LEMPAR / TEMBAK / TEBAS).
        public string LabelTebas()
        {
            if (_iJarak < 0) return TargetSenyap() != null ? "BUNUH SENYAP" : "TEBAS";
            return TabelJarak[_iJarak].label;
        }

        // ---------- Input tombol TEBAS ----------
        public void TekanTebas()
        {
            _tTekan = Time.unscaledTime;
            if (_iJarak >= 0 && TabelJarak[_iJarak].jenis == JenisJarak.Busur && AdaAmunisi()) _menarik = true;
        }

        public void LepasTebas()
        {
            float lama = _tTekan < 0f ? 0f : Time.unscaledTime - _tTekan;
            _tTekan = -1f;
            bool tarik = _menarik; _menarik = false;
            if (_tersimpan || (_ctrl != null && _ctrl.Berenang)) return;
            if (_iJarak < 0)
            {
                var korban = TargetSenyap();
                if (korban != null && lama < AmbangBidik) { BunuhSenyap(korban); return; }
                Tebas(AlasKeramat.UI.TombolTahan.Berat(lama));
                return;
            }
            var s = TabelJarak[_iJarak];
            switch (s.jenis)
            {
                case JenisJarak.Busur: if (tarik && lama >= s.tarikMin * 0.5f) Lepaskan(s, lama); break;
                case JenisJarak.Tombak:
                    if (lama < AmbangBidik) TusukTombak(s); else Lepaskan(s, lama);
                    break;
                default: Lepaskan(s, lama); break;
            }
        }

        void TusukTombak(SenjataJarak s)
        {
            if (SedangMenyerang) return;
            var simpan = _aktif;
            _aktif = new Senjata { id = s.id, ringan = s.tusuk, berat = s.tusuk, tebang = 4f, jangkau = s.jangkau, durR = 0.5f, durB = 0.5f, aktifR = 0.18f, aktifB = 0.18f, stamR = 5f, stamB = 5f, tipe = DamageType.Tusuk };
            Tebas(false);
            _kembalikan = simpan; _adaKembalikan = true;
        }

        Senjata _kembalikan; bool _adaKembalikan;

        bool AdaAmunisi()
        {
            if (_iJarak < 0 || _inv == null) return false;
            var s = TabelJarak[_iJarak];
            if (s.magasin > 0) { int isi; _isi.TryGetValue(s.id, out isi); if (isi > 0) return true; }
            foreach (var a in s.amunisi) if (_inv.Punya(a)) return true;
            return false;
        }

        ItemID AmbilAmunisi(SenjataJarak s)
        {
            foreach (var a in s.amunisi) if (_inv.Punya(a)) { _inv.Kurangi(a, 1); return a; }
            return ItemID.None;
        }

        void Lepaskan(SenjataJarak s, float lama)
        {
            if (_tIsi > 0f) { GameEvents.RaiseToast("Mengisi ulang..."); return; }
            var cam = _ctrl != null ? _ctrl.Kamera : null;
            if (cam == null || _inv == null) return;
            ItemID peluru;
            if (s.magasin > 0)
            {
                int isi; _isi.TryGetValue(s.id, out isi);
                if (isi <= 0) { IsiUlang(s); return; }
                _isi[s.id] = isi - 1; peluru = s.amunisi[0];
                if (isi - 1 <= 0) IsiUlang(s);
            }
            else
            {
                peluru = AmbilAmunisi(s);
                if (peluru == ItemID.None) { GameEvents.RaiseToast("Amunisi habis"); return; }
            }
            Vector3 asal = cam.position + cam.forward * 0.4f - cam.up * 0.1f;
            if (s.jenis == JenisJarak.Hitscan) Tembak(s, cam);
            else
            {
                int idx = s.jenis == JenisJarak.Busur ? 0 : s.jenis == JenisJarak.Tombak ? 1 : s.jenis == JenisJarak.Ketapel ? 2 : s.jenis == JenisJarak.Suar ? 3 : 4;
                float dmg = s.jenis == JenisJarak.Busur ? DamageTarik(s, lama) * (peluru == ItemID.PanahTulang ? 34f / 30f : peluru == ItemID.PanahRacun ? 22f / 30f : peluru == ItemID.PanahApi ? 26f / 30f : 1f) : s.damage;
                float laju = s.jenis == JenisJarak.Busur ? LajuTarik(s, lama) : s.laju;
                var go = prefabProyektil != null && idx < prefabProyektil.Length ? prefabProyektil[idx] : null;
                ProyektilAK.Luncurkan(go, asal, cam.forward * laju, s.gravitasi, dmg, peluru, gameObject);
            }
            MeterSuara.Pancar(transform.position, s.bunyi);
            var a = GameServices.Get<ProceduralAudioSynth>();
            if (a != null) a.Play(s.jenis == JenisJarak.Hitscan || s.jenis == JenisJarak.Suar ? ProceduralAudioSynth.KlipTembak : ProceduralAudioSynth.KlipLepasPanah, null, AudioCategory.Efek, 1f, Random.Range(0.95f, 1.05f), 3);
            Guncang(s.jenis == JenisJarak.Hitscan ? 0.12f : 0.04f);
            if (s.jenis == JenisJarak.Tombak || s.jenis == JenisJarak.Bom) { _tCekSenjata = 0f; }
        }

        void IsiUlang(SenjataJarak s)
        {
            int ada = 0; foreach (var a in s.amunisi) ada += _inv.Jumlah(a);
            int isi; _isi.TryGetValue(s.id, out isi);
            int ambil = Mathf.Min(s.magasin - isi, ada);
            if (ambil <= 0) { GameEvents.RaiseToast("Amunisi habis"); return; }
            _inv.Kurangi(s.amunisi[0], ambil);
            _isi[s.id] = isi + ambil;
            _tIsi = s.isiUlang;
            GameEvents.RaiseToast("Mengisi ulang " + ItemDatabase.Get(s.id).displayName);
        }

        void Tembak(SenjataJarak s, Transform cam)
        {
            int mask = (1 << GameLayers.Enemy) | (1 << GameLayers.Tree) | (1 << GameLayers.Structure) | (1 << GameLayers.Ground) | 1;
            for (int b = 0; b < Mathf.Max(1, s.butir); b++)
            {
                Vector3 arah = Quaternion.Euler(Random.Range(-s.sebaran, s.sebaran) * 0.5f, Random.Range(-s.sebaran, s.sebaran) * 0.5f, 0f) * Vector3.forward;
                arah = cam.rotation * arah;
                RaycastHit hit;
                if (!Physics.Raycast(cam.position, arah, out hit, s.jangkau, mask, QueryTriggerInteraction.Ignore)) continue;
                var t = hit.collider.GetComponentInParent<IDamageable>();
                if (t != null && !ReferenceEquals(t, GetComponent<IDamageable>()))
                    t.TakeDamage(new DamageInfo { amount = s.damage, type = DamageType.Proyektil, point = hit.point, direction = arah, source = gameObject, isHeavy = s.damage >= 40f });
                var h = OnKena; if (h != null && t != null) h(false);
            }
        }

        void TickJarak(float dt)
        {
            if (_tIsi > 0f) _tIsi -= dt;
            if (_tElak > 0f)
            {
                _tElak -= dt;
                var cc = GetComponent<CharacterController>();
                if (cc != null && cc.enabled) cc.Move(_arahElak * (JarakElak / LamaElak) * dt);
            }
            if (_menarik && _tTekan >= 0f && Time.unscaledTime - _tTekan > TarikBebasStamina)
            {
                if (_v == null) _v = GameServices.Get<AndiVitalsAndSanity>();
                if (_v != null && !_v.Pakai(StaminaTarikPerDetik * dt)) { _menarik = false; GameEvents.RaiseToast("Terlalu lelah menahan tarikan"); }
            }
            if (_adaKembalikan && !SedangMenyerang) { _aktif = _kembalikan; _adaKembalikan = false; }
            if (pivotFP != null && _menarik && _t < 0f) pivotFP.localRotation = Quaternion.Slerp(pivotFP.localRotation, _restPivot * Quaternion.Euler(0f, -25f, 0f), dt * 8f);
        }

        // ---------- Elak (17 §2.2) ----------
        public bool BisaElak()
        {
            if (_tersimpan || _tElak > 0f) return false;
            var s = NusantaraEnemyAndMutantAI.Semua;
            for (int i = 0; i < s.Count; i++) if (s[i].IsAlive && (s[i].transform.position - transform.position).sqrMagnitude <= RadiusElakMusuh * RadiusElakMusuh) return true;
            var h = HantuAI.Semua;
            for (int i = 0; i < h.Count; i++) if (h[i].IsAlive && (h[i].transform.position - transform.position).sqrMagnitude <= RadiusElakMusuh * RadiusElakMusuh) return true;
            return false;
        }

        public void Elak(Vector2 joystick)
        {
            if (_v == null) _v = GameServices.Get<AndiVitalsAndSanity>();
            if (_v != null && !_v.Pakai(StaminaElak)) { GameEvents.RaiseToast("Terlalu lelah untuk mengelak"); return; }
            Vector3 lokal = joystick.sqrMagnitude > 0.04f ? new Vector3(joystick.x, 0f, joystick.y).normalized : Vector3.back;
            _arahElak = transform.TransformDirection(lokal);
            _tElak = LamaElak;
        }

        // ---------- Bunuh Senyap (17 §2.2) ----------
        static readonly Collider[] _hasilSenyap = new Collider[8];

        public NusantaraEnemyAndMutantAI TargetSenyap()
        {
            if (_ctrl == null || !_ctrl.Jongkok || _aktif.id == ItemID.None || (_aktif.tipe != DamageType.Tebas && _aktif.tipe != DamageType.Tusuk)) return null;
            int n = Physics.OverlapSphereNonAlloc(transform.position, JarakSenyap, _hasilSenyap, 1 << GameLayers.Enemy, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                var ai = _hasilSenyap[i].GetComponentInParent<NusantaraEnemyAndMutantAI>();
                if (ai == null || !ai.IsAlive || !StatMusuh.Sekte(ai.Jenis) || ai.Kesadaran >= NusantaraEnemyAndMutantAI.AmbangPenasaran) continue;
                if (BolehSenyap(ai.transform.forward, transform.position - ai.transform.position)) return ai;
            }
            return null;
        }

        // Dari belakang: arah ke Andi dari musuh membentuk sudut >= 180-60 derajat terhadap hadapan musuh.
        public static bool BolehSenyap(Vector3 hadapMusuh, Vector3 keAndi)
        {
            hadapMusuh.y = 0f; keAndi.y = 0f;
            return keAndi.sqrMagnitude > 0.0001f && Vector3.Angle(hadapMusuh, keAndi) >= 180f - SudutSenyap;
        }

        void BunuhSenyap(NusantaraEnemyAndMutantAI ai)
        {
            bool kuat = ai.Jenis == EnemyArchetype.AbdiButo || ai.Jenis == EnemyArchetype.PemimpinPatroli;
            float dmg = kuat ? _aktif.berat * PengaliSenyapKuat : ai.HP + 1f;
            _berat = true; _t = 0f; _sudahKena = true;
            ai.TakeDamage(new DamageInfo { amount = dmg, type = _aktif.tipe, point = ai.transform.position + Vector3.up * 1.4f, direction = transform.forward, source = gameObject, isHeavy = true });
            MeterSuara.Pancar(transform.position, BunyiSenyap);
            var h = OnKena; if (h != null) h(true);
        }
    }
}
