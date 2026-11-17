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
        public bool IsAlive { get { return State != KeadaanHewan.Mati; } }
        public float HoldDuration { get { return 0f; } }

        float _hp, _t, _tState, _fase, _arah, _tSeruduk, _guling;
        Quaternion[] _rest;
        AndiFPSController _andi;
        bool _diambil;

        public void Pasang(JH j, Transform[] t) { jenis = j; tulang = t; }

        public static float HPAwal(JH j)
        {
            switch (j) { case JH.Rusa: return 40f; case JH.Kancil: return 15f; case JH.BabiHutan: return 70f; case JH.AyamHutan: return 8f; default: return 25f; }
        }

        // Hasil buruan 03 §8.
        public static void Hasil(JH j, out ItemID[] id, out int[] n)
        {
            switch (j)
            {
                case JH.Rusa: id = new[] { ItemID.DagingMentah, ItemID.KulitHewan, ItemID.Tulang }; n = new[] { 3, 1, 2 }; return;
                case JH.Kancil: id = new[] { ItemID.DagingMentah, ItemID.KulitHewan, ItemID.Tulang }; n = new[] { 1, 1, 1 }; return;
                case JH.BabiHutan: id = new[] { ItemID.DagingMentah, ItemID.KulitHewan, ItemID.Tulang, ItemID.Tengkorak }; n = new[] { 4, 1, 3, 1 }; return;
                case JH.AyamHutan: id = new[] { ItemID.DagingMentah, ItemID.Bulu }; n = new[] { 1, 3 }; return;
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
            if (State == KeadaanHewan.Mati) { Guling(); return; }
            float dt = Time.deltaTime;
            _tState -= dt;
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            var u = ResepHewan.Ambil(jenis);
            Vector3 p = transform.position;
            float kec = 0f;
            if (_andi != null && State != KeadaanHewan.Menyerang)
            {
                float det = u.deteksi * (_andi.Jongkok ? 0.5f : 1f) * (_andi.SedangLari ? 2f : 1f);
                float d = Vector3.Distance(_andi.transform.position, p);
                if (d < det) { State = KeadaanHewan.Kabur; _tState = LamaKabur; }
                else if (d < det * 1.5f && State == KeadaanHewan.Merumput) { State = KeadaanHewan.Waspada; _tState = 2f; }
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
                        if (ke.magnitude < JarakSeruduk && _tSeruduk <= 0f)
                        {
                            _tSeruduk = JedaSeruduk;
                            var target = _andi.GetComponent<IDamageable>();
                            if (target != null) target.TakeDamage(new DamageInfo { amount = DamageSeruduk, type = DamageType.Tusuk, point = _andi.transform.position, direction = ke.normalized, source = gameObject });
                        }
                    }
                    if (_tState <= 0f) State = KeadaanHewan.Merumput;
                    break;
            }
            Gerak(kec, dt);
            Animasi(kec, dt);
        }

        void Gerak(float kec, float dt)
        {
            Vector3 maju = Quaternion.Euler(0f, _arah, 0f) * Vector3.forward;
            Vector3 p = transform.position + maju * kec * dt;
            float? wl = WorldTerrainGenerator.GetWaterLevelAt(p.x, p.z);
            float h = WorldTerrainGenerator.SampleHeight(p.x, p.z);
            if ((wl.HasValue && wl.Value > h + 0.3f) || h < 0.3f) { _arah += 150f; return; } // jangan masuk air dalam / laut
            transform.SetPositionAndRotation(new Vector3(p.x, h, p.z), Quaternion.Euler(0f, _arah, 0f));
        }

        void Animasi(float kec, float dt)
        {
            if (tulang.Length < ResepHewan.Jumlah) return;
            for (int i = 0; i < tulang.Length; i++) if (tulang[i] != null) tulang[i].localRotation = _rest[i];
            _fase += kec * dt * 6f / Mathf.Max(0.2f, ResepHewan.Ambil(jenis).kaki);
            float amp = Mathf.Clamp01(kec / 2f) * 32f;
            for (int k = 0; k < 4; k++)
            {
                if (jenis == JH.AyamHutan && k < 2) { R(ResepHewan.LFU + k * 3, 0f, 0f, State == KeadaanHewan.Kabur ? Mathf.Sin(_t * 30f) * 40f * (k == 0 ? 1f : -1f) : 0f); continue; }
                float f = _fase + (k == 0 || k == 3 ? 0f : Mathf.PI);
                R(ResepHewan.LFU + k * 3, Mathf.Sin(f) * amp, 0f, 0f);
                R(ResepHewan.LFU + k * 3 + 1, Mathf.Max(0f, -Mathf.Sin(f)) * amp * 0.8f * (k < 2 ? -1f : 1f), 0f, 0f);
            }
            _t += dt;
            float nunduk = State == KeadaanHewan.Merumput && kec < 0.1f ? 35f + Mathf.Sin(_t * 1.3f) * 8f : State == KeadaanHewan.Waspada ? -15f : 0f;
            R(ResepHewan.Neck, nunduk, 0f, 0f);
        }

        void R(int i, float x, float y, float z) { if (tulang[i] != null) tulang[i].localRotation = tulang[i].localRotation * Quaternion.Euler(x, y, z); }

        public void TakeDamage(DamageInfo d)
        {
            if (State == KeadaanHewan.Mati) return;
            _hp -= d.amount;
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
            switch (j) { case JH.Rusa: return "Rusa"; case JH.Kancil: return "Kancil"; case JH.BabiHutan: return "Babi Hutan"; case JH.AyamHutan: return "Ayam Hutan"; default: return "Biawak"; }
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
