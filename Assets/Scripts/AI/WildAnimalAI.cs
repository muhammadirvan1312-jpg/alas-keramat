using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;
using JH = AlasKeramat.Core.ResepHewan.JenisHewan;

namespace AlasKeramat.AI
{
    // 03 §8: hewan liar. Merumput (jalan acak) -> waspada -> kabur menjauhi Andi (jongkok = deteksi separuh, lari = ganda).
    // Babi hutan menyeruduk bila dilukai. Mati -> terguling, interaksi "AMBIL HASIL BURUAN" memberi hasil 03 §8.
    public class WildAnimalAI : MonoBehaviour, IDamageable, IInteractable
    {
        public enum KeadaanHewan { Merumput, Waspada, Kabur, Menyerang, Mati }
        public const float JedaSeruduk = 1.5f, DamageSeruduk = 12f, JarakSeruduk = 1.4f, LamaMarah = 8f, LamaKabur = 12f, LajuBelok = 240f;

        [SerializeField] JH jenis;
        [SerializeField] Transform[] tulang = new Transform[ResepHewan.Jumlah];

        public JH Jenis { get { return jenis; } }
        public KeadaanHewan State { get; private set; }
        public float KecepatanKini { get; private set; }
        public float KecepatanMaks { get { return ResepHewan.Ambil(jenis).kecepatan; } }
        ModelHewanAsli _modelAsli;
        bool _cekModel;
        bool PakaiModel() { if (!_cekModel) { _cekModel = true; _modelAsli = GetComponent<ModelHewanAsli>(); } return _modelAsli != null && _modelAsli.Aktif; }
        public bool IsAlive { get { return State != KeadaanHewan.Mati; } }
        public float HoldDuration { get { return 0f; } }

        float _hp, _t, _tState, _fase, _arah, _tSeruduk, _guling, _tMata;
        Quaternion[] _rest;
        AndiFPSController _andi;
        SenterAndi _senter;
        TimeOfDayClock _jam;
        GameObject _kilauMata;
        bool _diambil;
        static Material _matKilauMata;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatisMata() { _matKilauMata = null; }

        // Kilau mata hewan malam (tapetum lucidum) memantul saat disorot senter Andi.
        public static bool MataMenyalaDiSenter(bool nokturnal, float jam, bool senterNyala, Vector3 posHewan, Vector3 posKamera, Vector3 arahKamera, out float intensitas)
        {
            intensitas = 0f;
            if (!nokturnal || !senterNyala) return false;
            if (jam < 18.5f && jam >= 5.5f) return false;
            Vector3 d = posHewan - posKamera;
            float dist = d.magnitude;
            if (dist < 3.5f || dist > 26f) return false;
            float dot = Vector3.Dot(d / dist, arahKamera.normalized);
            if (dot < 0.78f) return false;
            intensitas = Mathf.Clamp01((dot - 0.78f) / 0.22f) * Mathf.Clamp01(1f - (dist - 16f) / 10f);
            return intensitas > 0.05f;
        }

        void PerbaruiKilauMata(float dt)
        {
            _tMata -= dt;
            if (_tMata > 0f) return;
            _tMata = 0.2f;
            var u = ResepHewan.Ambil(jenis);
            if (!u.pemangsa && !u.agresif) { if (_kilauMata != null && _kilauMata.activeSelf) _kilauMata.SetActive(false); return; }
            if (_senter == null) _senter = GameServices.Get<SenterAndi>();
            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            var cam = Camera.main;
            float inten = 0f;
            bool nyala = State != KeadaanHewan.Mati && cam != null && _senter != null
                && MataMenyalaDiSenter(true, _jam != null ? _jam.Jam : 12f, _senter.Nyala, transform.position, cam.transform.position, cam.transform.forward, out inten);
            if (!nyala) { if (_kilauMata != null && _kilauMata.activeSelf) _kilauMata.SetActive(false); return; }
            if (_kilauMata == null)
            {
                if (_matKilauMata == null)
                    _matKilauMata = SafeMaterialFactory.CreatePBR("MataKilauMalam", Color.black, 0f, 0.9f, emission: new Color(1.5f, 0.95f, 0.22f));
                _kilauMata = new GameObject("KilauMata");
                Transform kepala = null;
                foreach (var t in GetComponentsInChildren<Transform>(true))
                    if (t.name == "Head" || t.name == "head" || t.name == "Skull" || t.name == "Bip001 Head") { kepala = t; break; }
                _kilauMata.transform.SetParent(kepala != null ? kepala : transform, false);
                _kilauMata.transform.localPosition = kepala != null ? new Vector3(0f, 0.04f, 0.09f) : new Vector3(0f, Mathf.Max(0.28f, u.kaki * 0.95f), Mathf.Max(0.25f, u.panjang * 0.42f));
                for (int i = -1; i <= 1; i += 2)
                {
                    var m = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    m.name = i < 0 ? "MataL" : "MataR";
                    Object.Destroy(m.GetComponent<Collider>());
                    m.transform.SetParent(_kilauMata.transform, false);
                    m.transform.localPosition = new Vector3(i * 0.045f, 0f, 0f);
                    m.transform.localScale = Vector3.one * 0.022f;
                    var mr = m.GetComponent<MeshRenderer>();
                    if (mr != null) { mr.sharedMaterial = _matKilauMata; mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false; }
                }
            }
            if (!_kilauMata.activeSelf) _kilauMata.SetActive(true);
            float s = 0.75f + inten * 0.45f;
            _kilauMata.transform.localScale = new Vector3(s, s, s);
        }

        public void Pasang(JH j, Transform[] t) { jenis = j; tulang = t; }

        public static float HPAwal(JH j)
        {
            switch (j)
            {
                case JH.Rusa: return 40f; case JH.Kancil: return 15f; case JH.BabiHutan: return 70f; case JH.AyamHutan: return 8f;
                case JH.Harimau: return 110f; case JH.Buaya: return 130f; case JH.Tawon: return 22f; case JH.UlarPiton: return 85f;
                case JH.UlarKobra: return 45f; case JH.Monyet: return 30f; case JH.Anjing: return 55f; case JH.Kucing: return 22f;
                default: return 25f;
            }
        }

        // Hasil buruan 03 §8 & hewan buas Nusantara.
        public static void Hasil(JH j, out ItemID[] id, out int[] n)
        {
            switch (j)
            {
                case JH.Rusa: id = new[] { ItemID.DagingMentah, ItemID.KulitHewan, ItemID.Tulang }; n = new[] { 3, 1, 2 }; return;
                case JH.Kancil: id = new[] { ItemID.DagingMentah, ItemID.KulitHewan, ItemID.Tulang }; n = new[] { 1, 1, 1 }; return;
                case JH.BabiHutan: id = new[] { ItemID.DagingMentah, ItemID.KulitHewan, ItemID.Tulang, ItemID.Tengkorak }; n = new[] { 4, 1, 3, 1 }; return;
                case JH.AyamHutan: id = new[] { ItemID.DagingMentah, ItemID.Bulu }; n = new[] { 1, 3 }; return;
                case JH.Harimau: id = new[] { ItemID.DagingMentah, ItemID.KulitHewan, ItemID.Tulang, ItemID.Tengkorak }; n = new[] { 4, 2, 3, 1 }; return;
                case JH.Buaya: id = new[] { ItemID.DagingMentah, ItemID.KulitHewan, ItemID.Tulang }; n = new[] { 4, 2, 3 }; return;
                case JH.Tawon: id = new[] { ItemID.JamuEnergi, ItemID.Damar }; n = new[] { 1, 1 }; return;
                case JH.UlarPiton: id = new[] { ItemID.DagingMentah, ItemID.KulitHewan, ItemID.Tulang }; n = new[] { 3, 2, 2 }; return;
                case JH.UlarKobra: id = new[] { ItemID.DagingMentah, ItemID.KulitHewan, ItemID.JamurBeracun }; n = new[] { 2, 1, 1 }; return;
                case JH.Monyet: id = new[] { ItemID.DagingMentah, ItemID.KulitHewan, ItemID.Pisang }; n = new[] { 2, 1, 1 }; return;
                case JH.Anjing: id = new[] { ItemID.DagingMentah, ItemID.KulitHewan, ItemID.Tulang }; n = new[] { 2, 1, 2 }; return;
                case JH.Kucing: id = new[] { ItemID.DagingMentah, ItemID.KulitHewan }; n = new[] { 1, 1 }; return;
                default: id = new[] { ItemID.DagingMentah, ItemID.KulitHewan }; n = new[] { 2, 1 }; return;
            }
        }

        void Awake()
        {
            _hp = HPAwal(jenis);
            _rest = new Quaternion[tulang.Length];
            for (int i = 0; i < tulang.Length; i++) _rest[i] = tulang[i] != null ? tulang[i].localRotation : Quaternion.identity;
            _arah = transform.eulerAngles.y;
            _t = Random.value * 5f;
        }

        void Update()
        {
            if (State == KeadaanHewan.Mati) { KecepatanKini = 0f; if (_kilauMata != null && _kilauMata.activeSelf) _kilauMata.SetActive(false); if (!PakaiModel()) Guling(); return; }
            float dt = Time.deltaTime;
            _tState -= dt;
            PerbaruiKilauMata(dt);
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            var u = ResepHewan.Ambil(jenis);
            Vector3 p = transform.position;
            float kec = 0f;
            if (_andi != null && State != KeadaanHewan.Menyerang)
            {
                float det = u.deteksi * 0.75f * (_andi.Jongkok ? 0.5f : 1f) * (_andi.SedangLari ? 2f : 1f);
                float d = Vector3.Distance(_andi.transform.position, p);
                if (d < det)
                {
                    if (u.pemangsa && EnemyDirector.Kesulitan != Difficulty.Damai) { if (State != KeadaanHewan.Menyerang) BunyikanSuara(0.85f); State = KeadaanHewan.Menyerang; _tState = LamaMarah; }
                    else { State = KeadaanHewan.Kabur; _tState = LamaKabur; }
                }
                else if (d < det * 1.45f && State == KeadaanHewan.Merumput) { State = KeadaanHewan.Waspada; _tState = 2f; }
            }
            switch (State)
            {
                case KeadaanHewan.Merumput:
                    if (_tState <= 0f) { _tState = Random.Range(3f, 8f); _arah += Random.Range(-90f, 90f); }
                    kec = _tState > 4f ? u.kecepatan * 0.12f : 0f;
                    break;
                case KeadaanHewan.Waspada:
                    if (_tState <= 0f) State = KeadaanHewan.Merumput;
                    break;
                case KeadaanHewan.Kabur:
                    if (_andi != null) { Vector3 jauh = p - _andi.transform.position; _arah = Mathf.MoveTowardsAngle(_arah, Mathf.Atan2(jauh.x, jauh.z) * Mathf.Rad2Deg, LajuBelok * dt); }
                    kec = u.kecepatan;
                    if (_tState <= 0f) State = KeadaanHewan.Merumput;
                    break;
                case KeadaanHewan.Menyerang:
                    if (_andi != null)
                    {
                        Vector3 ke = _andi.transform.position - p;
                        _arah = Mathf.MoveTowardsAngle(_arah, Mathf.Atan2(ke.x, ke.z) * Mathf.Rad2Deg, LajuBelok * dt);
                        kec = u.kecepatan;
                        _tSeruduk -= dt;
                        float jangkauSerang = u.jangkau > 0f ? u.jangkau : JarakSeruduk;
                        if (ke.magnitude < jangkauSerang && _tSeruduk <= 0f)
                        {
                            _tSeruduk = u.jeda > 0f ? u.jeda : JedaSeruduk;
                            BunyikanSuara(0.95f);
                            float dmg = u.damage > 0f ? u.damage : DamageSeruduk;
                            var target = _andi.GetComponent<IDamageable>();
                            if (target != null) target.TakeDamage(new DamageInfo { amount = dmg, type = DamageType.Tusuk, point = _andi.transform.position, direction = ke.normalized, source = gameObject });
                            if (u.beracun)
                            {
                                var vAndi = _andi.GetComponent<AndiVitalsAndSanity>();
                                if (vAndi != null) vAndi.KenaBisaUlar(20f);
                            }
                        }
                    }
                    if (_tState <= 0f) State = KeadaanHewan.Merumput;
                    break;
            }
            Gerak(kec, dt);
            KecepatanKini = kec;
            if (PakaiModel()) return;
            if (_andi == null || (_andi.transform.position - transform.position).sqrMagnitude <= 3025f || (Time.frameCount & 1) == 0)
                Animasi(kec, dt);
        }

        void Gerak(float kec, float dt)
        {
            var u = ResepHewan.Ambil(jenis);
            Vector3 maju = Quaternion.Euler(0f, _arah, 0f) * Vector3.forward;
            Vector3 p = transform.position + maju * kec * dt;
            float? wl = WorldTerrainGenerator.GetWaterLevelAt(p.x, p.z);
            float h = WorldTerrainGenerator.SampleHeight(p.x, p.z);
            if (u.terbang)
            {
                float yAir = wl.HasValue ? Mathf.Max(h, wl.Value) : h;
                float yTerbang = yAir + 1.25f + Mathf.Sin(_t * 6f) * 0.25f;
                transform.SetPositionAndRotation(new Vector3(p.x, yTerbang, p.z), Quaternion.Euler(0f, _arah, 0f));
                return;
            }
            float toleransiAir = u.amfibi ? 1.25f : 0.3f;
            if ((wl.HasValue && wl.Value > h + toleransiAir) || h < 0.3f) { _arah += 150f; return; }
            float y = u.amfibi && wl.HasValue && wl.Value > h ? Mathf.Max(h, wl.Value - 0.14f) : h;
            transform.SetPositionAndRotation(new Vector3(p.x, y, p.z), Quaternion.Euler(0f, _arah, 0f));
        }

        void Animasi(float kec, float dt)
        {
            if (tulang.Length < ResepHewan.Jumlah) return;
            for (int i = 0; i < tulang.Length; i++) if (tulang[i] != null) tulang[i].localRotation = _rest[i];
            var u = ResepHewan.Ambil(jenis);
            _t += dt;
            if (u.terbang)
            {
                float kepak = Mathf.Sin(_t * 42f) * 35f;
                R(ResepHewan.LFU, 0f, 0f, kepak); R(ResepHewan.RFU, 0f, 0f, -kepak);
                return;
            }
            if (u.ular)
            {
                _fase += (kec * 1.4f + 0.6f) * dt * 6f;
                float ampUlar = State == KeadaanHewan.Menyerang ? 26f : 18f;
                R(ResepHewan.Pelvis, 0f, Mathf.Sin(_fase) * ampUlar, 0f);
                R(ResepHewan.Spine, 0f, Mathf.Sin(_fase + 1.1f) * ampUlar, 0f);
                float tegak = jenis == JH.UlarKobra && (State == KeadaanHewan.Menyerang || State == KeadaanHewan.Waspada) ? -38f : 0f;
                R(ResepHewan.Chest, tegak, Mathf.Sin(_fase + 2.2f) * ampUlar * 0.7f, 0f);
                R(ResepHewan.Neck, tegak * 0.35f, Mathf.Sin(_fase + 3.1f) * ampUlar * 0.5f, 0f);
                R(ResepHewan.Head, -tegak * 0.6f, 0f, 0f);
                return;
            }
            _fase += kec * dt * 6f / Mathf.Max(0.2f, u.kaki);
            float amp = Mathf.Clamp01(kec / 2f) * 32f;
            for (int k = 0; k < 4; k++)
            {
                if (jenis == JH.AyamHutan && k < 2) { R(ResepHewan.LFU + k * 3, 0f, 0f, State == KeadaanHewan.Kabur ? Mathf.Sin(_t * 30f) * 40f * (k == 0 ? 1f : -1f) : 0f); continue; }
                float f = _fase + (k == 0 || k == 3 ? 0f : Mathf.PI);
                R(ResepHewan.LFU + k * 3, Mathf.Sin(f) * amp, 0f, 0f);
                R(ResepHewan.LFU + k * 3 + 1, Mathf.Max(0f, -Mathf.Sin(f)) * amp * 0.8f * (k < 2 ? -1f : 1f), 0f, 0f);
            }
            float nunduk = State == KeadaanHewan.Merumput && kec < 0.1f ? 35f + Mathf.Sin(_t * 1.3f) * 8f : State == KeadaanHewan.Waspada ? -15f : 0f;
            R(ResepHewan.Neck, nunduk, 0f, 0f);
        }

        void R(int i, float x, float y, float z) { if (tulang[i] != null) tulang[i].localRotation = tulang[i].localRotation * Quaternion.Euler(x, y, z); }

        public static string KlipSuaraHewan(JH j, out float pitch)
        {
            switch (j)
            {
                case JH.Harimau: pitch = 0.68f; return ProceduralAudioSynth.KlipTeriakSekte;
                case JH.Buaya: pitch = 0.58f; return ProceduralAudioSynth.KlipGeramSekte;
                case JH.BabiHutan: pitch = 0.86f; return ProceduralAudioSynth.KlipGeramSekte;
                case JH.Anjing: pitch = 1.22f; return ProceduralAudioSynth.KlipGeramSekte;
                case JH.Kucing: pitch = 1.48f; return ProceduralAudioSynth.KlipRintihSekte;
                case JH.Monyet: pitch = 1.58f; return ProceduralAudioSynth.KlipTeriakSekte;
                case JH.AyamHutan: pitch = 1.0f; return ProceduralAudioSynth.OsAyam;
                case JH.Tawon: pitch = 1.35f; return ProceduralAudioSynth.OsKepak;
                case JH.UlarPiton: case JH.UlarKobra: pitch = 1.45f; return ProceduralAudioSynth.KlipAbu;
                default: pitch = 1.12f; return ProceduralAudioSynth.OsKepak;
            }
        }

        void BunyikanSuara(float vol)
        {
            var s = GameServices.Get<ProceduralAudioSynth>();
            if (s == null) return;
            float pt;
            string kl = KlipSuaraHewan(jenis, out pt);
            s.Play(kl, transform.position, AudioCategory.Efek, vol, pt * Random.Range(0.94f, 1.06f), 2, 38f);
        }

        public void TakeDamage(DamageInfo d)
        {
            if (State == KeadaanHewan.Mati) return;
            _hp -= d.amount;
            BunyikanSuara(0.8f);
            if (_hp <= 0f) { State = KeadaanHewan.Mati; _guling = 0f; gameObject.layer = GameLayers.Interactable; GameEvents.RaiseToast(NamaTampil(jenis) + " tumbang"); return; }
            if (ResepHewan.Ambil(jenis).agresif) { State = KeadaanHewan.Menyerang; _tState = LamaMarah; }
            else { State = KeadaanHewan.Kabur; _tState = LamaKabur; }
        }

        void Guling()
        {
            if (_guling >= 90f) return;
            _guling = Mathf.Min(90f, _guling + Time.deltaTime * 180f);
            transform.rotation = Quaternion.Euler(0f, _arah, _guling);
        }

        public static string NamaTampil(JH j)
        {
            switch (j)
            {
                case JH.Rusa: return "Rusa"; case JH.Kancil: return "Kancil"; case JH.BabiHutan: return "Babi Hutan"; case JH.AyamHutan: return "Ayam Hutan";
                case JH.Harimau: return "Harimau"; case JH.Buaya: return "Buaya"; case JH.Tawon: return "Kawanan Tawon"; case JH.UlarPiton: return "Ular Piton";
                case JH.UlarKobra: return "Ular Kobra"; case JH.Monyet: return "Monyet"; case JH.Anjing: return "Anjing Liar"; case JH.Kucing: return "Kucing Hutan";
                default: return "Biawak";
            }
        }

        public string GetPromptText(GameObject actor) { return "AMBIL HASIL BURUAN"; }
        public bool CanInteract(GameObject actor) { return State == KeadaanHewan.Mati && !_diambil; }

        public void Interact(GameObject actor)
        {
            if (!CanInteract(actor)) return;
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv == null) return;
            ItemID[] id; int[] n;
            Hasil(jenis, out id, out n);
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < id.Length; i++)
            {
                int m = inv.Tambah(id[i], n[i]);
                if (m > 0) { if (sb.Length > 0) sb.Append(", "); sb.Append(ItemDatabase.Get(id[i]).displayName).Append(" x").Append(m); }
            }
            GameEvents.RaiseToast(sb.Length > 0 ? "Didapat: " + sb : "Tas penuh");
            _diambil = true;
            Destroy(gameObject, 0.2f);
        }
    }
}
