using System.Collections.Generic;
using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.AI
{
    // 08 §2 (T8): Kang Deden - HP 150 (pulih 1/s di luar tempur), golok 12 / 1,1 s, energi kerja, 15 perintah Buku Catatan,
    // rutinitas malam, takut hantu (menunjuk & berlindung), bertarung bila Takut < 50, kecewa bila dipukul 3x dalam 10 s.
    public partial class KangDedenKelvinAI : CompanionDasar, IInteractable, ISaveable
    {
        public const float HPDeden = 150f, Jalan = 1.5f, Lari = 5f, DamageGolok = 12f, JedaGolok = 1.1f, JarakPerintah = 6f, RadiusKerja = 40f;
        public const float EnergiTurun = 0.05f, EnergiNaik = 0.5f, EnergiLelah = 20f, EnergiLanjut = 80f, LamaKecewa = 60f, RadiusHantuTakut = 20f;

        [SerializeField] Material matKayu;

        public override float HPMaks { get { return HPDeden; } }
        protected override float SisiIkuti { get { return -1f; } }
        public override string NamaTampil { get { return "Kang Deden"; } }
        public float HoldDuration { get { return 0f; } }
        public string SaveKey { get { return "deden"; } }

        public CompanionOrder Perintah { get; private set; } = CompanionOrder.Ikuti;
        public float Energi { get; private set; } = 100f;
        public float TakutHantu { get; private set; }
        public bool Bergabung { get; private set; }
        public bool Kecewa { get { return _tKecewa > 0f; } }

        QuestManager _misi;
        float _tGolok, _tKecewa, _tCek, _tKerja, _tMenunjuk;
        int _fase;
        readonly List<float> _pukulanAndi = new List<float>();
        Vector3 _titikTunggu, _titikKerja;
        CompanionOrder _sebelumLelah;
        bool _lelah;
        Object _sasaranKerja;
        int _iPagar;

        public void Pasang(Material kayu) { matKayu = kayu; }

        protected override void Awake()
        {
            base.Awake();
            GameServices.Register(this);
            _titikTunggu = transform.position;
        }

        void OnEnable() { GameEvents.OnStructureBuilt += Bangunan; }
        void OnDisable() { GameEvents.OnStructureBuilt -= Bangunan; }

        void Bangunan(BlueprintID id)
        {
            if (Bergabung && IsAlive && _andi != null && Datar(_andi.transform.position - transform.position).magnitude < 15f) Klip("JempolKanan");
        }

        void Update()
        {
            try { Tick(Time.deltaTime); }
            catch (System.Exception e) { Debug.LogError("[AK][KangDeden] galat: " + e.Message); enabled = false; }
        }

        void Tick(float dt)
        {
            AmbilLayanan();
            if (Gugur) return;
            if (Kritis) { TickKritis(dt); return; }
            _tCek -= dt;
            if (_tCek <= 0f) { _tCek = 1f; CekBergabung(); }
            if (!Bergabung) return; // masih duduk terhuyung di ekor heli (M02)
            TickCatatan();
            if (_andi == null) return;
            _tGolok -= dt; _tMenunjuk -= dt;
            if (_tKecewa > 0f) { _tKecewa -= dt; if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak; Menuju(TitikIkuti(_andi.transform.position, _andi.transform.forward, -1f) - _andi.transform.forward * 8f, Jalan, dt); return; }
            if (TickTakut(dt)) return;
            if (Perintah == CompanionOrder.Ikuti && TickBertarung(dt)) return;
            bool kerja = Perintah != CompanionOrder.Ikuti && Perintah != CompanionOrder.Tunggu && Perintah != CompanionOrder.Istirahat && Perintah != CompanionOrder.PulangKeKemah;
            if (kerja) { Energi = Mathf.Max(0f, Energi - EnergiTurun * dt); if (Energi < EnergiLelah && !_lelah) { _lelah = true; _sebelumLelah = Perintah; Perintah = CompanionOrder.Istirahat; GameEvents.RaiseSubtitle("(Kang Deden mengusap keringat lalu duduk di dekat api.)"); } }
            else if (Perintah != CompanionOrder.Istirahat) Pulih(1f, dt);
            if (RutinitasMalam(dt)) return;
            TickPerintah(dt);
            if (Perintah == CompanionOrder.Ikuti) CekTeleport(dt);
        }

        void CekBergabung()
        {
            if (Bergabung) return;
            if (_misi == null) _misi = GameServices.Get<QuestManager>();
            if (_misi == null || !_misi.Progres.catatan) return;
            Bergabung = true;
            if (_anim != null) _anim.ModeAktif = ProceduralAnimator.Mode.Normal;
            if (_cc != null) _cc.enabled = true;
            Perintah = CompanionOrder.Ikuti;
            Klip("Mengangguk");
        }

        bool PerintahTerbuka(CompanionOrder o)
        {
            if (_misi == null) _misi = GameServices.Get<QuestManager>();
            bool m02 = _misi != null && _misi.Progres.catatan, m04 = _misi != null && _misi.Progres.selesai != null && _misi.Progres.selesai.Length >= 4 && _misi.Progres.selesai[3];
            return Terbuka(o, m02, m04, _misi != null && _misi.M05Selesai);
        }

        // Dipanggil Buku Catatan: Deden membaca (angguk) lalu menjalankan atau menggeleng + alasan.
        public bool BeriPerintah(CompanionOrder o)
        {
            if (!Bergabung || !IsAlive) return false;
            if (Kecewa) { Klip("GelengBahu"); GameEvents.RaiseSubtitle("(Kang Deden memalingkan muka. Ia masih kecewa.)"); return false; }
            if (!PerintahTerbuka(o)) { Klip("GelengBahu"); GameEvents.RaiseSubtitle("(Kang Deden belum paham perintah itu.)"); return false; }
            string alasan;
            if (!Syarat(o, Konteks(), out alasan)) { Klip("GelengBahu"); GameEvents.RaiseSubtitle("(Kang Deden menggeleng: \"" + alasan + "\")"); return false; }
            Klip(o == CompanionOrder.Ikuti || o == CompanionOrder.Tunggu ? "Mengangguk" : "JempolKanan");
            Perintah = o; _fase = 0; _tKerja = 0f; _iPagar = 0; _sasaranKerja = null; _lelah = false;
            _titikTunggu = transform.position;
            if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak;
            return true;
        }

        public KonteksKemah Konteks()
        {
            var k = new KonteksKemah();
            Vector3? kemah = Kemah();
            k.kemah = kemah.HasValue; k.api = kemah.HasValue;
            if (!kemah.HasValue) return k;
            var rak = Fungsi(BlueprintID.RakKayu, kemah.Value);
            k.rak = rak != null; k.kayuRak = rak != null ? rak.Stok : 0; k.rakPenuh = rak != null && rak.Stok >= FungsiBangunan.KapRak;
            k.penampung = Fungsi(BlueprintID.PenampungHujan, kemah.Value) != null;
            k.cetakBiru = CetakTerdekat(kemah.Value) != null;
            k.strukturRusak = StrukturRusak(kemah.Value) != null;
            return k;
        }

        public static FungsiBangunan Fungsi(BlueprintID jenis, Vector3 dekat)
        {
            FungsiBangunan hasil = null; float terbaik = RadiusKerja;
            foreach (var f in FungsiBangunan.Semua)
            {
                if (f == null || f.Jenis != jenis) continue;
                float d = Datar(f.transform.position - dekat).magnitude;
                if (d < terbaik) { terbaik = d; hasil = f; }
            }
            return hasil;
        }

        public static CetakBiru CetakTerdekat(Vector3 dekat)
        {
            CetakBiru hasil = null; float terbaik = RadiusKerja;
            foreach (var c in CetakBiru.Semua) { if (c == null) continue; float d = Datar(c.transform.position - dekat).magnitude; if (d < terbaik) { terbaik = d; hasil = c; } }
            return hasil;
        }

        public static BagianStruktur StrukturRusak(Vector3 dekat)
        {
            foreach (var b in BagianStruktur.Semua)
                if (b != null && b.IsAlive && b.HP < BagianStruktur.HPAwal(b.Jenis) - 1f && Datar(b.transform.position - dekat).magnitude < RadiusKerja) return b;
            return null;
        }

        // ---------- Takut hantu & bertarung ----------
        bool TickTakut(float dt)
        {
            HantuAI dekat = null; float dTerdekat = 999f;
            foreach (var h in HantuAI.Semua) { if (!h.IsAlive) continue; float d = Datar(h.transform.position - transform.position).magnitude; if (d < dTerdekat) { dTerdekat = d; dekat = h; } }
            if (dTerdekat <= RadiusHantuTakut) TakutHantu = Mathf.Min(100f, TakutHantu + 40f * dt);
            else if (dTerdekat > 30f) TakutHantu = Mathf.Max(0f, TakutHantu - 10f * dt);
            if (TakutHantu < 50f || dekat == null) return false;
            if (_tMenunjuk <= 0f) { _tMenunjuk = 8f; Hadap(dekat.transform.position - transform.position, 1f); Klip("Menunjuk"); GameEvents.RaiseSubtitle("(Kang Deden menunjuk panik ke arah " + HantuAI.NamaTampil(dekat.Jenis) + "!)"); return true; }
            if (_tMenunjuk > 6.5f) { Diam(dt); return true; } // tahan menunjuk sebentar (peringatan berguna)
            Vector3 balik = _andi.transform.position + Datar(_andi.transform.position - dekat.transform.position).normalized * 2f;
            Menuju(balik, Lari, dt);
            return true;
        }

        bool TickBertarung(float dt)
        {
            NusantaraEnemyAndMutantAI sasaran = null; float terbaik = 12f;
            foreach (var m in NusantaraEnemyAndMutantAI.Semua)
            {
                if (!m.IsAlive || (m.Keadaan != NusantaraEnemyAndMutantAI.KeadaanSekte.Serang && m.Keadaan != NusantaraEnemyAndMutantAI.KeadaanSekte.Serbu && m.Keadaan != NusantaraEnemyAndMutantAI.KeadaanSekte.Gertak)) continue;
                float d = Datar(m.transform.position - transform.position).magnitude;
                if (d < terbaik) { terbaik = d; sasaran = m; }
            }
            if (sasaran == null) return false;
            Vector3 ke = sasaran.transform.position - transform.position;
            if (Datar(ke).magnitude > 1.8f) { Menuju(sasaran.transform.position, Lari, dt); if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak; return true; }
            Diam(dt); Hadap(ke, dt);
            if (_tGolok <= 0f)
            {
                _tGolok = JedaGolok;
                if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Ayun;
                sasaran.TakeDamage(new DamageInfo { amount = DamageGolok, type = DamageType.Tebas, point = sasaran.transform.position + Vector3.up * 1.2f, direction = Datar(ke).normalized, source = gameObject });
            }
            else if (_tGolok < JedaGolok - 0.4f && _anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Ancang;
            return true;
        }

        // 19:00 tanpa perintah Ikuti -> pulang & duduk di api; 22:00-06:00 tidur (duduk) di kemah.
        bool RutinitasMalam(float dt)
        {
            if (_jam == null || Perintah == CompanionOrder.Ikuti || Perintah == CompanionOrder.JagaApi) return false;
            float j = _jam.Jam;
            if (!(j >= 19f || j < 6f)) return false;
            Vector3? kemah = Kemah();
            if (!kemah.HasValue) return false;
            Vector3 duduk = kemah.Value + new Vector3(-1.6f, 0f, 1.2f);
            if (Datar(duduk - transform.position).magnitude > 1.2f) { if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak; Menuju(duduk, Jalan, dt); return true; }
            Diam(dt); Hadap(kemah.Value - transform.position, dt);
            if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Intai;
            Energi = Mathf.Min(100f, Energi + EnergiNaik * dt);
            return true;
        }

        public override void TakeDamage(DamageInfo info)
        {
            if (info.source != null && _andi != null && info.source == _andi.gameObject)
            {
                _pukulanAndi.Add(Time.time);
                _pukulanAndi.RemoveAll(t => Time.time - t > 10f);
                if (_pukulanAndi.Count >= 3) { _tKecewa = LamaKecewa; _pukulanAndi.Clear(); GameEvents.RaiseSubtitle("(Kang Deden menjauh, kecewa.)"); }
                return; // pukulan Andi tidak melukai, hanya mengecewakan
            }
            base.TakeDamage(info);
        }

        // ---------- Interaksi ----------
        public string GetPromptText(GameObject actor)
        {
            if (Kritis) return "BERI JAMU";
            if (!Bergabung || Gugur) return "";
            if (_andi != null && Datar(_andi.transform.position - transform.position).magnitude > JarakPerintah) return "";
            return "PERINTAH KANG DEDEN";
        }

        public bool CanInteract(GameObject actor) { return GetPromptText(actor).Length > 0; }

        public void Interact(GameObject actor)
        {
            if (Kritis) { BeriJamu(); return; }
            var ui = GameServices.Get<AlasKeramat.UI.BukuCatatanUI>();
            if (ui != null) { Hadap(_andi.transform.position - transform.position, 1f); Klip("Mengangguk"); ui.Buka(this); }
        }

        public bool Terbuka(CompanionOrder o) { return PerintahTerbuka(o); }

        [System.Serializable] struct DataDeden { public Vector3 pos; public float hp, energi; public int perintah; public bool gabung, gugur; }
        public string CaptureJson() { return JsonUtility.ToJson(new DataDeden { pos = transform.position, hp = HP, energi = Energi, perintah = (int)Perintah, gabung = Bergabung, gugur = Gugur }); }
        public void RestoreJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            var d = JsonUtility.FromJson<DataDeden>(json);
            HP = Mathf.Clamp(d.hp, 0f, HPDeden); Energi = d.energi; Perintah = (CompanionOrder)d.perintah; Gugur = d.gugur;
            if (d.gabung && !Bergabung) { Bergabung = true; if (_anim != null) _anim.ModeAktif = ProceduralAnimator.Mode.Normal; }
            Teleport(d.pos);
        }
    }
}
