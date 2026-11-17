using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // 02 §1.7: animasi prosedural murni kode (tanpa komponen animasi bawaan). Lapisan: pose dasar -> lokomosi (fase dari jarak tempuh) -> klip aksi -> aditif (napas, tatap, kedip).
    // Mode khusus: duduk terhuyung (Deden di heli), pocong melompat, kuntilanak melayang, galeri (memutar klip bergiliran).
    public class ProceduralAnimator : MonoBehaviour
    {
        public enum Mode { Normal, DudukTerhuyung, Pocong, Melayang, Galeri }
        public static readonly string[] Klip = { "Melambai", "Menunjuk", "JempolKanan", "GelengBahu", "Mengangguk" };
        public const float DurasiKlip = 1.6f, CrossfadeKlip = 0.12f, JedaGaleri = 3.2f;
        const float LihatYawMaks = 70f, LihatPitchMaks = 40f, JarakLihat = 10f;

        [SerializeField] Transform[] tulang = new Transform[Kerangka.Jumlah];
        [SerializeField] Mode mode;
        [SerializeField] float tinggi = 1.75f;
        public Transform TargetLihat;
        public float KecepatanPaksa = -1f; // >= 0: kecepatan lokomosi dipaksa (galeri)

        Quaternion[] _rest;
        Vector3 _restPelvis, _posLalu;
        Vector3 _restEyeScale = Vector3.one;
        float _fase, _kec, _t, _tKedip = 3f, _kedip, _tKlip = -1f, _tGaleri;
        int _klip = -1, _klipGaleri;

        public Mode ModeAktif { get { return mode; } set { mode = value; } }
        public Transform[] Tulang { get { return tulang; } }

        public void Pasang(Transform[] t, Mode m, float tinggiBadan) { tulang = t; mode = m; tinggi = tinggiBadan; }

        void Awake()
        {
            if (tulang == null || tulang.Length < Kerangka.Jumlah || tulang[Kerangka.Pelvis] == null) { Debug.LogError("[AK][ProceduralAnimator] Kerangka tidak lengkap di " + name); enabled = false; return; }
            _rest = new Quaternion[tulang.Length];
            for (int i = 0; i < tulang.Length; i++) _rest[i] = tulang[i] != null ? tulang[i].localRotation : Quaternion.identity;
            _restPelvis = tulang[Kerangka.Pelvis].localPosition;
            if (tulang[Kerangka.LEye] != null) _restEyeScale = tulang[Kerangka.LEye].localScale;
            _posLalu = transform.position;
            _t = Random.value * 10f;
        }

        public void Mainkan(string nama)
        {
            int i = System.Array.IndexOf(Klip, nama);
            if (i < 0) return;
            _klip = i; _tKlip = 0f;
        }

        void R(int i, float x, float y, float z) { var b = tulang[i]; if (b != null) b.localRotation = b.localRotation * Quaternion.Euler(x, y, z); }

        // Parameter gait 02 §1.7 diinterpolasi menurut kecepatan: (langkah, paha, lutut, siku, bob, condong).
        static void Gait(float v, bool jongkok, out float langkah, out float paha, out float lutut, out float siku, out float bob, out float condong)
        {
            if (jongkok) { langkah = 0.5f; paha = 18f; lutut = 100f; siku = 30f; bob = 0.01f; condong = 25f; return; }
            float k = Mathf.InverseLerp(1f, 3.4f, v), l = Mathf.InverseLerp(3.4f, 5.6f, v), s = Mathf.InverseLerp(5.6f, 7.5f, v);
            langkah = v < 3.4f ? 0.75f : v < 5.6f ? Mathf.Lerp(0.75f, 1.2f, l) : Mathf.Lerp(1.2f, 1.5f, s);
            paha = v < 3.4f ? Mathf.Lerp(15f, 25f, k) : v < 5.6f ? Mathf.Lerp(25f, 40f, l) : Mathf.Lerp(40f, 48f, s);
            lutut = v < 3.4f ? 35f : v < 5.6f ? Mathf.Lerp(35f, 65f, l) : Mathf.Lerp(65f, 80f, s);
            siku = v < 3.4f ? 15f : v < 5.6f ? Mathf.Lerp(15f, 60f, l) : Mathf.Lerp(60f, 80f, s);
            bob = v < 3.4f ? 0.03f : v < 5.6f ? Mathf.Lerp(0.03f, 0.05f, l) : 0.06f;
            condong = v < 3.4f ? 3f : v < 5.6f ? Mathf.Lerp(3f, 8f, l) : Mathf.Lerp(8f, 14f, s);
        }

        void LateUpdate()
        {
            if (_rest == null) return;
            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            _t += dt;
            Vector3 d = transform.position - _posLalu; d.y = 0f; _posLalu = transform.position;
            float vMentah = KecepatanPaksa >= 0f ? KecepatanPaksa : d.magnitude / dt;
            _kec = Mathf.Lerp(_kec, Mathf.Min(vMentah, 8f), dt * 8f);
            for (int i = 0; i < tulang.Length; i++) if (tulang[i] != null) tulang[i].localRotation = _rest[i];
            var pel = tulang[Kerangka.Pelvis];
            pel.localPosition = _restPelvis;
            float skala = tinggi / 1.75f;

            // Pose dasar: lengan sedikit abduksi, siku ditekuk.
            R(Kerangka.LUpper, 0f, 0f, -8f); R(Kerangka.RUpper, 0f, 0f, 8f);
            R(Kerangka.LFore, -10f, 0f, 0f); R(Kerangka.RFore, -10f, 0f, 0f);

            if (mode == Mode.Galeri) TickGaleri(dt);
            if (mode == Mode.DudukTerhuyung) Duduk(skala);
            else if (mode == Mode.Pocong) Pocong(skala);
            else if (mode == Mode.Melayang) Melayang(skala);
            else Lokomosi(dt, skala);

            KlipAksi(dt);
            float napasHz = _kec > 3f ? 0.7f : 0.25f;
            R(Kerangka.Chest, Mathf.Sin(_t * napasHz * Mathf.PI * 2f) * 2f, 0f, 0f);
            Tatap();
            Kedip(dt);
        }

        void Lokomosi(float dt, float skala)
        {
            float langkah, paha, lutut, siku, bob, condong;
            Gait(_kec, false, out langkah, out paha, out lutut, out siku, out bob, out condong);
            float bobot = Mathf.Clamp01(_kec / 0.6f);
            _fase += _kec * dt / (langkah * skala) * Mathf.PI;
            float s = Mathf.Sin(_fase);
            R(Kerangka.LThigh, -paha * s * bobot, 0f, 0f); R(Kerangka.RThigh, paha * s * bobot, 0f, 0f);
            R(Kerangka.LCalf, (lutut * Mathf.Max(0f, Mathf.Sin(_fase + Mathf.PI * 0.5f)) + 5f) * bobot, 0f, 0f);
            R(Kerangka.RCalf, (lutut * Mathf.Max(0f, Mathf.Sin(_fase - Mathf.PI * 0.5f)) + 5f) * bobot, 0f, 0f);
            R(Kerangka.LUpper, 0.6f * paha * s * bobot, 0f, 0f); R(Kerangka.RUpper, -0.6f * paha * s * bobot, 0f, 0f);
            R(Kerangka.LFore, -siku * bobot, 0f, 0f); R(Kerangka.RFore, -siku * bobot, 0f, 0f);
            tulang[Kerangka.Pelvis].localPosition += Vector3.up * bob * skala * Mathf.Abs(s) * bobot;
            R(Kerangka.Pelvis, 0f, 5f * s * bobot, 0f); R(Kerangka.Spine, condong * bobot, -4f * s * bobot, 0f);
        }

        void Duduk(float skala)
        {
            var p = tulang[Kerangka.Pelvis];
            p.localPosition = new Vector3(_restPelvis.x, 0.2f * skala, _restPelvis.z);
            R(Kerangka.LThigh, -85f, -40f, -30f); R(Kerangka.RThigh, -85f, 40f, 30f);
            R(Kerangka.LCalf, 120f, 0f, 0f); R(Kerangka.RCalf, 120f, 0f, 0f);
            float goyang = Mathf.Sin(_t * 0.6f) * 6f + Mathf.Sin(_t * 1.7f) * 2f; // terhuyung pusing
            R(Kerangka.Spine, 12f, 0f, goyang); R(Kerangka.Head, 18f + Mathf.Sin(_t * 0.9f) * 5f, 0f, -goyang * 0.8f);
            R(Kerangka.LUpper, -35f, 0f, -10f); R(Kerangka.RUpper, -35f, 0f, 10f);
            R(Kerangka.LFore, -55f, 0f, 0f); R(Kerangka.RFore, -55f, 0f, 0f);
        }

        void Pocong(float skala)
        {
            // Lompatan kaku 0,6 s; tinggi lebih besar saat bergerak.
            float siklus = Mathf.Repeat(_t / 0.6f, 1f);
            float tinggiLompat = (_kec > 0.3f ? 0.35f : 0.12f) * skala;
            tulang[Kerangka.Pelvis].localPosition += Vector3.up * tinggiLompat * Mathf.Sin(siklus * Mathf.PI);
            R(Kerangka.Head, Mathf.Sin(_t * 0.7f) * 4f, 0f, 8f);
        }

        void Melayang(float skala)
        {
            tulang[Kerangka.Pelvis].localPosition += Vector3.up * (0.45f + 0.08f * Mathf.Sin(_t * 1.3f)) * skala;
            R(Kerangka.Spine, 6f, 0f, Mathf.Sin(_t * 0.5f) * 4f);
            R(Kerangka.Head, 22f, 0f, 14f);
            R(Kerangka.LUpper, -12f, 0f, 4f); R(Kerangka.RUpper, -12f, 0f, -4f);
            R(Kerangka.LFore, -25f, 0f, 0f); R(Kerangka.RFore, -25f, 0f, 0f);
        }

        void TickGaleri(float dt)
        {
            _tGaleri += dt;
            KecepatanPaksa = Mathf.Repeat(_tGaleri, JedaGaleri * 2f) < JedaGaleri ? 1.6f : 0f;
            if (_tGaleri >= JedaGaleri * 2f) { _tGaleri = 0f; Mainkan(Klip[_klipGaleri]); _klipGaleri = (_klipGaleri + 1) % Klip.Length; }
        }

        void KlipAksi(float dt)
        {
            if (_klip < 0) return;
            _tKlip += dt;
            if (_tKlip > DurasiKlip) { _klip = -1; return; }
            float w = Mathf.Clamp01(Mathf.Min(_tKlip, DurasiKlip - _tKlip) / CrossfadeKlip);
            float u = _tKlip / DurasiKlip;
            switch (_klip)
            {
                case 0: R(Kerangka.RUpper, -20f * w, 0f, 150f * w); R(Kerangka.RFore, 0f, 0f, (30f + 25f * Mathf.Sin(u * 18f)) * w); break; // melambai
                case 1: R(Kerangka.RUpper, -85f * w, 0f, 0f); R(Kerangka.RFore, 5f * w, 0f, 0f); break;                                   // menunjuk
                case 2: R(Kerangka.RUpper, -70f * w, 0f, 0f); R(Kerangka.RFore, -80f * w, 0f, 0f); R(Kerangka.RHand, 0f, 0f, 90f * w); break; // jempol
                case 3: R(Kerangka.Head, 0f, 20f * Mathf.Sin(u * Mathf.PI * 4f) * w, 0f); R(Kerangka.LClav, 0f, 0f, -8f * w); R(Kerangka.RClav, 0f, 0f, 8f * w); break; // geleng
                default: R(Kerangka.Head, 15f * Mathf.Abs(Mathf.Sin(u * Mathf.PI * 2f)) * w, 0f, 0f); break;                              // angguk
            }
        }

        void Tatap()
        {
            var kepala = tulang[Kerangka.Head];
            if (TargetLihat == null || kepala == null || mode == Mode.Pocong) return;
            Vector3 arah = TargetLihat.position - kepala.position;
            if (arah.sqrMagnitude > JarakLihat * JarakLihat) return;
            Vector3 lokal = transform.InverseTransformDirection(arah.normalized);
            float yaw = Mathf.Clamp(Mathf.Atan2(lokal.x, lokal.z) * Mathf.Rad2Deg, -LihatYawMaks, LihatYawMaks);
            float pitch = Mathf.Clamp(-Mathf.Asin(Mathf.Clamp(lokal.y, -1f, 1f)) * Mathf.Rad2Deg, -LihatPitchMaks, LihatPitchMaks);
            R(Kerangka.Neck, pitch * 0.4f, yaw * 0.4f, 0f); R(Kerangka.Head, pitch * 0.6f, yaw * 0.6f, 0f);
        }

        void Kedip(float dt)
        {
            _tKedip -= dt;
            if (_tKedip <= 0f) { _kedip = 0.12f; _tKedip = Random.Range(2.5f, 6f); }
            float sy = 1f;
            if (_kedip > 0f) { _kedip -= dt; sy = 0.1f; }
            for (int i = Kerangka.LEye; i <= Kerangka.REye; i++)
                if (tulang[i] != null) tulang[i].localScale = new Vector3(_restEyeScale.x, _restEyeScale.y * sy, _restEyeScale.z);
        }
    }
}
