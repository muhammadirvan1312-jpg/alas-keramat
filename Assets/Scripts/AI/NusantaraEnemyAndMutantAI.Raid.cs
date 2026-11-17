using AlasKeramat.Building;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // T7b (06 §6-7): Serbu kemah (berhenti 30 m -> isyarat -> pukul struktur 8/pukul, Buto 25), ragu Obor Tengkorak (berhenti 25 m 30 s, 50% batal),
    // Pagar Runcing (15 damage/s + terdorong), Seret Teman terluka ke titik mundur 40 m (1,2 m/s), lempar bambu Abdi Wanita 8-18 m.
    public partial class NusantaraEnemyAndMutantAI
    {
        public const float JarakBerhentiRaid = 30f, LamaIsyaratRaid = 3.6f, RadiusSasaran = 25f, JedaPukulStruktur = 1.4f, DamageStruktur = 8f, DamageStrukturButo = 25f;
        public const float DamagePagar = 15f, RadiusPagar = 1.0f, RadiusSeret = 20f, LajuSeret = 1.2f, JarakSeret = 40f, TakutObor = 40f;
        public const float LemparMin = 8f, LemparMaks = 18f, JedaLempar = 6f, AncangLempar = 0.6f;

        Vector3 _kemah;
        bool _pemimpinRaid, _raguObor, _sudahTakutObor;
        int _faseRaid;
        float _tRaid, _tPukulStruktur, _tPagar, _tCekPagar, _jedaLempar;
        BagianStruktur _sasaran;
        NusantaraEnemyAndMutantAI _teman, _diseretOleh;

        public bool Diseret { get { return _diseretOleh != null; } }

        public void MulaiSerbu(Vector3 kemah, bool pemimpin, bool raguObor)
        {
            _kemah = kemah; _pemimpinRaid = pemimpin; _raguObor = raguObor; _faseRaid = 0; _tRaid = 0f;
            _sadar = Mathf.Max(_sadar, AmbangWaspada);
            Ubah(KeadaanSekte.Serbu);
        }

        void TickSerbu(float dt)
        {
            float dk = Datar(_kemah - transform.position).magnitude;
            if (_terlihat && JarakAndi() < 12f) { _diserang = true; Ubah(KeadaanSekte.Serang); return; }
            _tRaid += dt;
            switch (_faseRaid)
            {
                case 0: // mendekat dari tepi hutan
                    float henti = _raguObor ? ZonaPenangkal.RadiusRaidObor : JarakBerhentiRaid;
                    if (dk > henti) { Menuju(_kemah, _d.jalan * 1.6f, dt); break; }
                    _faseRaid = _raguObor ? 3 : 1; _tRaid = 0f;
                    break;
                case 1: // isyarat: pemimpin memukul kentongan, lainnya berteriak
                    Diam(dt); Hadap(_kemah - transform.position, dt);
                    if (_anim != null) _anim.Pose = _pemimpinRaid ? ProceduralAnimator.PoseTempur.Kentongan : ProceduralAnimator.PoseTempur.Gertak;
                    if (_tRaid < dt * 1.5f) { if (_pemimpinRaid && _audio != null) _audio.PlayKentonganBamboo(transform.position + Vector3.up * 1.2f); else Suara(ProceduralAudioSynth.KlipTeriakSekte, 0.9f); }
                    if (_tRaid >= LamaIsyaratRaid) { _faseRaid = 2; _tRaid = 0f; if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak; }
                    break;
                case 2: // rusak struktur di kemah
                    if (_sasaran == null || !_sasaran.IsAlive) _sasaran = SasaranTerdekat();
                    if (_sasaran == null) { _diserang = true; _sadar = AmbangTerlibat; Ubah(KeadaanSekte.Serang); break; }
                    Vector3 ke = Datar(_sasaran.transform.position - transform.position);
                    if (ke.magnitude > 1.6f) { Menuju(_sasaran.transform.position, _d.lari * 0.7f, dt); break; }
                    Diam(dt); Hadap(ke, dt);
                    _tPukulStruktur -= dt;
                    if (_tPukulStruktur <= 0f)
                    {
                        _tPukulStruktur = JedaPukulStruktur;
                        if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Ayun;
                        Suara(ProceduralAudioSynth.KlipAyunSekte, 0.8f);
                        _sasaran.TakeDamage(new DamageInfo { amount = jenis == EnemyArchetype.AbdiButo ? DamageStrukturButo : DamageStruktur, type = DamageType.Tebas, point = _sasaran.transform.position, direction = ke.normalized, source = gameObject });
                    }
                    else if (_tPukulStruktur < JedaPukulStruktur - 0.3f && _anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Ancang;
                    break;
                case 3: // ragu karena Obor Tengkorak menyala
                    Diam(dt); Hadap(_kemah - transform.position, dt);
                    if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Intai;
                    if (_tRaid >= LamaRaguObor())
                    {
                        if (Random.value < 0.5f) { Ubah(KeadaanSekte.Lari); _titikLari = TitikMenjauhDari(_kemah, 120f); }
                        else { _raguObor = false; _faseRaid = 1; _tRaid = 0f; }
                    }
                    break;
            }
        }

        static float LamaRaguObor() { return EnemyDirector.LamaRaguObor; }

        BagianStruktur SasaranTerdekat()
        {
            BagianStruktur hasil = null; float terbaik = RadiusSasaran;
            var s = BagianStruktur.Semua;
            for (int i = 0; i < s.Count; i++)
            {
                var b = s[i];
                if (b == null || !b.IsAlive) continue;
                float dk = Datar(b.transform.position - _kemah).magnitude;
                if (dk > RadiusSasaran) continue;
                float d = Datar(b.transform.position - transform.position).magnitude;
                if (d < terbaik + RadiusSasaran) { if (hasil == null || d < Datar(hasil.transform.position - transform.position).magnitude) hasil = b; }
            }
            return hasil;
        }

        // Pagar Runcing & Obor Tengkorak (dipanggil dari Tick).
        void TickLingkungan(float dt)
        {
            _jedaLempar -= dt;
            _tCekPagar -= dt;
            if (_tCekPagar > 0f) return;
            _tCekPagar = 0.25f;
            if (!_sudahTakutObor && ZonaPenangkal.AdaObor(transform.position, ZonaPenangkal.RadiusRaidObor)) { _sudahTakutObor = true; Takut += TakutObor; }
            var s = BagianStruktur.Semua;
            bool kena = false; Vector3 dorong = Vector3.zero;
            for (int i = 0; i < s.Count; i++)
            {
                var b = s[i];
                if (b == null || !b.IsAlive || (b.Jenis != BagianStruktur.Tipe.PagarRuncing && b.Jenis != BagianStruktur.Tipe.BambuTanam)) continue;
                Vector3 ke = Datar(transform.position - b.transform.position);
                if (ke.magnitude > RadiusPagar) continue;
                kena = true; dorong += ke.sqrMagnitude > 0.0001f ? ke.normalized : -transform.forward;
            }
            if (!kena) { _tPagar = 0f; return; }
            GerakMentah(dorong.normalized * 3f, 0.25f);
            _tPagar += 0.25f;
            if (_tPagar >= 1f)
            {
                _tPagar -= 1f;
                TakeDamage(new DamageInfo { amount = DamagePagar, type = DamageType.Tusuk, point = transform.position + Vector3.up, direction = dorong.normalized, source = null });
            }
        }

        // ---------- Seret Teman ----------
        NusantaraEnemyAndMutantAI TemanTerluka()
        {
            for (int i = 0; i < Semua.Count; i++)
            {
                var o = Semua[i];
                if (o == this || !o.IsAlive || o._keadaan != KeadaanSekte.Terluka || o._diseretOleh != null) continue;
                if (Datar(o.transform.position - transform.position).magnitude <= RadiusSeret) return o;
            }
            return null;
        }

        bool CobaSeret()
        {
            if (_takut >= TakutLari || _keadaan == KeadaanSekte.Terluka || _keadaan == KeadaanSekte.Menyerah) return false;
            var t = TemanTerluka();
            if (t == null) return false;
            _teman = t;
            Ubah(KeadaanSekte.SeretTeman);
            _titikLari = TitikMenjauh(JarakSeret);
            return true;
        }

        void TickSeret(float dt)
        {
            if (_teman == null || !_teman.IsAlive) { LepasSeret(); Ubah(KeadaanSekte.Lari); return; }
            Vector3 keTeman = Datar(_teman.transform.position - transform.position);
            if (_teman._diseretOleh != this)
            {
                if (keTeman.magnitude > 1.3f) { Menuju(_teman.transform.position, _d.lari, dt); return; }
                _teman._diseretOleh = this;
            }
            Vector3 mundur = Datar(_titikLari - transform.position);
            if (mundur.magnitude < 1.5f || _tState > 60f) { LepasSeret(); Ubah(KeadaanSekte.Lari); return; }
            Hadap(-mundur, dt);
            GerakMentah(mundur.normalized * LajuSeret, dt);
            if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Intai;
            Vector3 p = transform.position + transform.forward * 1.1f;
            p.y = WorldTerrainGenerator.SampleHeight(p.x, p.z);
            _teman.transform.position = p;
            _teman.transform.rotation = Quaternion.LookRotation(-transform.forward);
        }

        void LepasSeret()
        {
            if (_teman != null && _teman._diseretOleh == this) _teman._diseretOleh = null;
            _teman = null;
        }

        // ---------- Lempar bambu (Abdi Wanita) ----------
        bool CobaLempar(float d)
        {
            if (jenis != EnemyArchetype.AbdiWanita || _jedaLempar > 0f || d < LemparMin || d > LemparMaks) return false;
            if (_dir == null || _dir.PrefabBambu == null || !_terlihat) return false;
            _jedaLempar = JedaLempar;
            return true;
        }

        void LemparSekarang()
        {
            if (_andi == null || _dir == null) return;
            Vector3 dari = transform.position + Vector3.up * 1.6f + transform.forward * 0.5f;
            BambuLempar.Lempar(_dir.PrefabBambu, dari, _andi.transform.position + Vector3.up * 1.1f, gameObject);
            Suara(ProceduralAudioSynth.KlipAyunSekte, 1f);
        }
    }
}
