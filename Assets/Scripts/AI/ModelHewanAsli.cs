using System;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Model hewan asli (FBX CC0 beranimasi) menggantikan tubuh prosedural: menghadap sesuai tulang kepala,
    // ukuran disamakan panjang tubuh lama, klip legacy dipilih dari kecepatan; roboh memakai klip mati.
    public class ModelHewanAsli : MonoBehaviour
    {
        [SerializeField] GameObject[] varian;
        [SerializeField] Material[] bahan;
        [SerializeField] string[] kunciBahan;
        [SerializeField] Material bahanMata;
        [SerializeField] string klipDiam, klipJalan, klipLari, klipMati;
        [SerializeField] float panjangTarget = 1f;
        WildAnimalAI _ai;
        Animation _anim;
        GameObject _model;
        Renderer[] _lama;
        string _kini = "";
        bool _gagal, _mati;

        public void Pasang(GameObject[] v, Material[] b, string[] k, Material mata, string diam, string jalan, string lari, string mati, float panjang)
        { varian = v; bahan = b; kunciBahan = k; bahanMata = mata; klipDiam = diam; klipJalan = jalan; klipLari = lari; klipMati = mati; panjangTarget = panjang; }

        public bool Aktif { get { return _model != null; } }

        // Klip sesuai kecepatan relatif (0 diam, <0,55 jalan, selebihnya lari); string kosong dilewati ke klip terdekat.
        public static string PilihKlip(float kec, float kecMaks, string diam, string jalan, string lari)
        {
            if (kec < 0.05f) return diam;
            float r = kecMaks > 0.01f ? kec / kecMaks : 1f;
            if (r < 0.55f && !string.IsNullOrEmpty(jalan)) return jalan;
            return !string.IsNullOrEmpty(lari) ? lari : jalan;
        }

        // Indeks bahan untuk slot material FBX berdasarkan kata kunci nama; -1 = mata, 0 = bawaan.
        public static int IndeksBahan(string namaObjek, string namaMaterial, string[] kunci)
        {
            if (namaObjek.IndexOf("eye", StringComparison.OrdinalIgnoreCase) >= 0 || namaMaterial.IndexOf("eye", StringComparison.OrdinalIgnoreCase) >= 0) return -1;
            if (kunci != null)
                for (int i = 0; i < kunci.Length; i++)
                    if (!string.IsNullOrEmpty(kunci[i]) && namaMaterial.IndexOf(kunci[i], StringComparison.OrdinalIgnoreCase) >= 0) return i;
            return 0;
        }

        void Start()
        {
            try
            {
                _ai = GetComponent<WildAnimalAI>();
                if (varian == null || varian.Length == 0 || _ai == null) { enabled = false; return; }
                var pf = varian[Mathf.Abs(GetInstanceID()) % varian.Length];
                if (pf == null) { enabled = false; return; }
                _model = Instantiate(pf, transform, false);
                _model.name = "ModelHewan";
                _model.transform.localPosition = Vector3.zero; _model.transform.localRotation = Quaternion.identity;
                foreach (var r in _model.GetComponentsInChildren<Renderer>(true))
                {
                    var src = r.sharedMaterials; var dst = new Material[src.Length];
                    for (int i = 0; i < src.Length; i++)
                    {
                        int k = IndeksBahan(r.name, src[i] != null ? src[i].name : "", kunciBahan);
                        dst[i] = k < 0 ? (bahanMata != null ? bahanMata : bahan[0]) : bahan[Mathf.Min(k, bahan.Length - 1)];
                    }
                    r.sharedMaterials = dst;
                    if (r is SkinnedMeshRenderer smr) smr.updateWhenOffscreen = false;
                }
                Hadapkan(); Skalakan();
                _lama = GetComponentsInChildren<Renderer>(true);
                foreach (var r in _lama) if (!r.transform.IsChildOf(_model.transform)) r.forceRenderingOff = true;
                _anim = _model.GetComponent<Animation>();
                if (_anim == null) { Destroy(_model); _model = null; Tampilkan(true); enabled = false; return; }
                _anim.cullingType = AnimationCullingType.BasedOnRenderers;
            }
            catch (Exception e) { _gagal = true; Debug.LogError("[AK][ModelHewanAsli] Gagal: " + e.Message); if (_model != null) Destroy(_model); _model = null; Tampilkan(true); }
        }

        void Tampilkan(bool lama) { if (_lama != null) foreach (var r in _lama) if (r != null) r.forceRenderingOff = !lama; }

        Bounds BatasModel()
        {
            var rs = _model.GetComponentsInChildren<Renderer>(true);
            var b = rs.Length > 0 ? rs[0].bounds : new Bounds(_model.transform.position, Vector3.one);
            for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
            return b;
        }

        void Hadapkan()
        {
            Transform kepala = null;
            foreach (var t in _model.GetComponentsInChildren<Transform>(true))
                if ((t.name == "Head" || t.name == "head" || t.name == "Skull" || t.name == "Bip001 Head") && t.GetComponent<Renderer>() == null) { kepala = t; break; }
            if (kepala == null) return;
            var b = BatasModel();
            Vector3 d = kepala.position - b.center; d.y = 0f;
            if (d.sqrMagnitude < 1e-6f) return;
            float sudut = Vector3.SignedAngle(d, transform.forward, Vector3.up);
            _model.transform.rotation = Quaternion.AngleAxis(sudut, Vector3.up) * _model.transform.rotation;
        }

        void Skalakan()
        {
            var b = BatasModel();
            Vector3 lokal = transform.InverseTransformVector(b.size);
            float panjang = Mathf.Max(Mathf.Abs(lokal.z), Mathf.Abs(lokal.x) * 0.5f);
            if (panjang > 1e-4f) _model.transform.localScale *= Mathf.Clamp(panjangTarget / panjang, 0.001f, 1000f);
            var b2 = BatasModel();
            _model.transform.position += Vector3.up * (transform.position.y - b2.min.y);
        }

        // Varian model bisa memakai nama klip sedikit berbeda (Walk/Walking) atau berawalan Armature|.
        string Cari(string k)
        {
            if (string.IsNullOrEmpty(k) || _anim == null) return k;
            if (_anim.GetClip(k) != null) return k;
            string alt = k.EndsWith("ing") ? k.Substring(0, k.Length - 3) : k + "ing";
            if (_anim.GetClip(alt) != null) return alt;
            foreach (AnimationState st in _anim)
            {
                if (st == null) continue;
                if (string.Equals(st.name, k, StringComparison.OrdinalIgnoreCase) || st.name.EndsWith("|" + k, StringComparison.OrdinalIgnoreCase)) return st.name;
                if (string.Equals(st.name, alt, StringComparison.OrdinalIgnoreCase) || st.name.EndsWith("|" + alt, StringComparison.OrdinalIgnoreCase)) return st.name;
            }
            return null;
        }

        void LateUpdate()
        {
            if (_gagal || _model == null || _ai == null) return;
            if (_ai.State == WildAnimalAI.KeadaanHewan.Mati)
            {
                if (!_mati)
                {
                    _mati = true;
                    string km = Cari(klipMati);
                    if (!string.IsNullOrEmpty(km))
                    { var st = _anim[km]; st.wrapMode = WrapMode.ClampForever; _anim.CrossFade(km, 0.12f); }
                }
                return;
            }
            string mau = Cari(PilihKlip(_ai.KecepatanKini, _ai.KecepatanMaks, klipDiam, klipJalan, klipLari));
            if (string.IsNullOrEmpty(mau)) return;
            if (mau != _kini)
            {
                _kini = mau; var st = _anim[mau]; st.wrapMode = WrapMode.Loop;
                _anim.CrossFade(mau, 0.2f);
            }
            if (mau != Cari(klipDiam))
            {
                float acuan = mau == Cari(klipLari) && klipLari != klipJalan ? _ai.KecepatanMaks : _ai.KecepatanMaks * 0.35f;
                _anim[mau].speed = Mathf.Clamp(acuan > 0.01f ? _ai.KecepatanKini / acuan : 1f, 0.6f, 1.6f);
            }
        }
    }
}
