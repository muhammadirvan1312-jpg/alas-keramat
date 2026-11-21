using UnityEngine;

namespace AlasKeramat.AI
{
    // Kelelawar penghuni gua bawah tanah (UG1 / UG3): terbang mengitari langit-langit gua memakai model FBX CC0
    // (kelelawar.fbx, klip Bat_Flying) atau sayap prosedural cadangan bila aset privat belum tersedia.
    public class KelelawarGua : MonoBehaviour
    {
        [SerializeField] GameObject sumberModel;
        [SerializeField] Material bahanUtama;
        [SerializeField] Material bahanBagian;
        [SerializeField] Vector3 pusatOrbit;
        [SerializeField] float radiusOrbit = 3.2f;
        [SerializeField] float tinggiGelombang = 0.45f;
        [SerializeField] float kecepatanSudut = 1.35f;
        [SerializeField] float faseAwal;
        [SerializeField] float skalaModel = 0.35f;
        [SerializeField] string klipTerbang = "Bat_Flying";

        // Panik saat tersorot senter Andi: orbit lebih cepat, lebar, dan naik ke langit-langit gua.
        public const float JarakSorot = 10f, SudutSorot = 24f, DurasiPanik = 3.2f, PengaliLajuPanik = 3.2f, PengaliRadiusPanik = 1.6f, NaikPanik = 0.7f;
        const float JedaCekSorot = 0.2f, LajuPanik = 2.5f;

        Transform _sayapKiri, _sayapKanan;
        Vector3 _posSebelum;
        AlasKeramat.Player.SenterAndi _senter;
        float _waktu, _panik, _akhirPanik, _cekBerikut;
        AnimationState _klip;

        public float Panik { get { return _panik; } }

        // Murni (diuji self-test): kelelawar berada di kerucut sorot senter dan cukup dekat.
        public static bool TersorotSenter(Vector3 posKamera, Vector3 arahKamera, Vector3 posKelelawar, float jarakMaks, float sudutMaks)
        {
            Vector3 d = posKelelawar - posKamera;
            float jarak = d.magnitude;
            if (jarak < 0.01f || jarak > jarakMaks) return false;
            return Vector3.Angle(arahKamera, d) <= sudutMaks;
        }

        public void Pasang(GameObject model, Material utama, Material bagian, Vector3 pusat, float radius, float tinggi, float kecepatan, float fase, float skala, string klip)
        {
            sumberModel = model;
            bahanUtama = utama;
            bahanBagian = bagian;
            pusatOrbit = pusat;
            radiusOrbit = radius;
            tinggiGelombang = tinggi;
            kecepatanSudut = kecepatan;
            faseAwal = fase;
            skalaModel = skala;
            klipTerbang = klip;
        }

        public static Vector3 HitungPosisi(Vector3 pusat, float radius, float tinggi, float kecepatan, float fase, float waktu)
        {
            float r = Mathf.Max(0.25f, radius);
            float t = waktu * kecepatan + fase;
            float x = Mathf.Cos(t) * r;
            float z = Mathf.Sin(t * 1.5f) * (r * 0.72f);
            float y = Mathf.Sin(t * 2.2f) * tinggi;
            return pusat + new Vector3(x, y, z);
        }

        void Start()
        {
            if (pusatOrbit.sqrMagnitude < 0.001f) pusatOrbit = transform.position;
            _posSebelum = transform.position;
            _waktu = Time.time;
            _senter = AlasKeramat.Core.GameServices.Get<AlasKeramat.Player.SenterAndi>();
            if (_senter == null) _senter = FindFirstObjectByType<AlasKeramat.Player.SenterAndi>();
            if (sumberModel != null)
            {
                var inst = Instantiate(sumberModel, transform, false);
                inst.name = "ModelKelelawar";
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = Quaternion.identity;
                inst.transform.localScale = Vector3.one * skalaModel;
                foreach (var r in inst.GetComponentsInChildren<Renderer>(true))
                {
                    var src = r.sharedMaterials;
                    var dst = new Material[src.Length];
                    for (int i = 0; i < src.Length; i++)
                    {
                        string nm = src[i] != null ? src[i].name : "";
                        bool bagian = nm.IndexOf("part", System.StringComparison.OrdinalIgnoreCase) >= 0 || i > 0;
                        dst[i] = bagian && bahanBagian != null ? bahanBagian : (bahanUtama != null ? bahanUtama : src[i]);
                    }
                    r.sharedMaterials = dst;
                }
                var anim = inst.GetComponentInChildren<Animation>(true);
                if (anim != null)
                {
                    anim.cullingType = AnimationCullingType.BasedOnRenderers;
                    string pakai = CariKlip(anim, klipTerbang);
                    if (!string.IsNullOrEmpty(pakai))
                    {
                        var st = anim[pakai];
                        if (st != null) { _klip = st; st.wrapMode = WrapMode.Loop; st.normalizedTime = Mathf.Repeat(faseAwal * 0.159f, 1f); }
                        anim.Play(pakai);
                    }
                }
            }
            else
            {
                BuatSayapCadangan();
            }
        }

        static string CariKlip(Animation anim, string nama)
        {
            if (anim == null) return null;
            if (!string.IsNullOrEmpty(nama) && anim.GetClip(nama) != null) return nama;
            string pertama = null;
            foreach (AnimationState st in anim)
            {
                if (st == null) continue;
                if (pertama == null) pertama = st.name;
                if (!string.IsNullOrEmpty(nama) && (st.name.EndsWith("|" + nama, System.StringComparison.OrdinalIgnoreCase) || string.Equals(st.name, nama, System.StringComparison.OrdinalIgnoreCase)))
                    return st.name;
            }
            return pertama;
        }

        void BuatSayapCadangan()
        {
            var badan = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            badan.name = "Badan";
            badan.transform.SetParent(transform, false);
            badan.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            badan.transform.localScale = new Vector3(0.11f, 0.14f, 0.11f);
            Destroy(badan.GetComponent<Collider>());
            if (bahanUtama != null) badan.GetComponent<Renderer>().sharedMaterial = bahanUtama;

            _sayapKiri = BuatSayap("SayapKiri", -1f);
            _sayapKanan = BuatSayap("SayapKanan", 1f);
        }

        Transform BuatSayap(string nama, float sisi)
        {
            var pivot = new GameObject(nama).transform;
            pivot.SetParent(transform, false);
            var bilah = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bilah.transform.SetParent(pivot, false);
            bilah.transform.localPosition = new Vector3(sisi * 0.2f, 0f, 0f);
            bilah.transform.localScale = new Vector3(0.4f, 0.015f, 0.18f);
            Destroy(bilah.GetComponent<Collider>());
            if (bahanUtama != null) bilah.GetComponent<Renderer>().sharedMaterial = bahanUtama;
            return pivot;
        }

        void Update()
        {
            if (Time.time >= _cekBerikut)
            {
                _cekBerikut = Time.time + JedaCekSorot;
                var cam = Camera.main;
                if (_senter != null && _senter.Nyala && cam != null
                    && TersorotSenter(cam.transform.position, cam.transform.forward, transform.position, JarakSorot, SudutSorot))
                    _akhirPanik = Time.time + DurasiPanik;
            }
            _panik = Mathf.MoveTowards(_panik, Time.time < _akhirPanik ? 1f : 0f, Time.deltaTime * LajuPanik);
            _waktu += Time.deltaTime * Mathf.Lerp(1f, PengaliLajuPanik, _panik);
            Vector3 p = HitungPosisi(pusatOrbit + Vector3.up * (NaikPanik * _panik), radiusOrbit * Mathf.Lerp(1f, PengaliRadiusPanik, _panik), tinggiGelombang, kecepatanSudut, faseAwal, _waktu);
            Vector3 arah = p - _posSebelum;
            transform.position = p;
            if (arah.sqrMagnitude > 1e-6f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(arah.normalized, Vector3.up), Time.deltaTime * 8f);
            _posSebelum = p;
            if (_klip != null) _klip.speed = Mathf.Lerp(1f, PengaliLajuPanik * 0.6f, _panik);
            if (_sayapKiri != null && _sayapKanan != null)
            {
                float kepak = Mathf.Sin(_waktu * 15f + faseAwal) * 38f;
                _sayapKiri.localRotation = Quaternion.Euler(0f, 0f, kepak);
                _sayapKanan.localRotation = Quaternion.Euler(0f, 0f, -kepak);
            }
        }
    }
}
