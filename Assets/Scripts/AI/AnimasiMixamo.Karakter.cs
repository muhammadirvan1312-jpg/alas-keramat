using System.Collections.Generic;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Model karakter Mixamo asli (bertulang mixamorig) menggantikan tampilan prosedural:
    // klip legacy diputar langsung pada model, tinggi disamakan dengan karakter lama, pinggul dikunci (tanpa geser akar).
    public partial class AnimasiMixamo
    {
        [SerializeField] GameObject modelKarakter;
        GameObject _model;
        Transform _hipsModel;
        Vector3 _hipsModel0;
        readonly List<Renderer> _lama = new List<Renderer>();
        bool _modelTampil = true;

        public void PasangModel(GameObject prefab) { modelKarakter = prefab; }
        public bool PakaiModelAsli { get { return _model != null; } }

        // Skala agar tinggi model = tinggi karakter lama (batas 0,5-2,5 agar aman bila bounds aneh).
        public static float SkalaTinggi(float tinggiLama, float tinggiModel)
        {
            if (tinggiLama <= 0.1f || tinggiModel <= 0.1f) return 1f;
            return Mathf.Clamp(tinggiLama / tinggiModel, 0.5f, 2.5f);
        }

        bool PasangModel()
        {
            float bawah = float.MaxValue, atas = float.MinValue;
            _lama.Clear();
            foreach (var r in GetComponentsInChildren<Renderer>(true))
            {
                if (!(r is SkinnedMeshRenderer) && !(r is MeshRenderer)) continue;
                _lama.Add(r);
                if (!r.enabled) continue;
                bawah = Mathf.Min(bawah, r.bounds.min.y); atas = Mathf.Max(atas, r.bounds.max.y);
            }
            float tinggiLama = atas > bawah ? atas - bawah : 1.75f;
            _model = Instantiate(modelKarakter, transform, false);
            _model.name = "ModelMixamo";
            _model.transform.localPosition = Vector3.zero; _model.transform.localRotation = Quaternion.identity;
            float mb = float.MaxValue, ma = float.MinValue;
            foreach (var r in _model.GetComponentsInChildren<Renderer>(true)) { mb = Mathf.Min(mb, r.bounds.min.y); ma = Mathf.Max(ma, r.bounds.max.y); }
            float s = SkalaTinggi(tinggiLama, ma > mb ? ma - mb : 1.75f);
            _model.transform.localScale = _model.transform.localScale * s / Mathf.Max(0.0001f, transform.lossyScale.y);
            foreach (var t in _model.GetComponentsInChildren<Transform>(true)) if (t.name.EndsWith(":Hips")) { _hipsModel = t; break; }
            if (_hipsModel == null) { Destroy(_model); _model = null; return false; }
            _hipsModel0 = _hipsModel.localPosition;
            foreach (var r in _model.GetComponentsInChildren<Renderer>(true)) { r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; }
            _anim = _model.GetComponent<Animation>();
            if (_anim == null) _anim = _model.AddComponent<Animation>();
            _anim.playAutomatically = false;
            _anim.cullingType = AnimationCullingType.BasedOnRenderers;
            foreach (var k in bank.klip) if (k != null && _anim.GetClip(k.name) == null) _anim.AddClip(k, k.name);
            _modelTampil = false; TampilModel(true);
            return true;
        }

        // Mode normal: model Mixamo tampil, tubuh prosedural disembunyikan; mode lain (mati, ragdoll, sinema) sebaliknya.
        void TampilModel(bool tampil)
        {
            if (tampil == _modelTampil || _model == null) return;
            _modelTampil = tampil;
            _model.SetActive(tampil);
            foreach (var r in _lama) if (r != null) r.forceRenderingOff = tampil;
        }

        bool _mati;
        // Animator prosedural dimatikan saat musuh tewas: model Mixamo memutar klip roboh sekali lalu diam di tanah.
        void MatiModel()
        {
            if (_mati) { KunciPinggul(); return; }
            _mati = true;
            string k = PilihKlipMati(_anim.GetClip("Zombie_Dying") != null, _anim.GetClip("Dying") != null, peran);
            if (k.Length == 0) return;
            var st = _anim[k]; st.wrapMode = WrapMode.ClampForever; st.speed = 1f;
            _anim.CrossFade(k, 0.15f); _kini = k;
        }

        public static string PilihKlipMati(bool adaZombie, bool adaBiasa, Peran p)
        {
            if (p == Peran.Raksasa || p == Peran.Sekte) return adaZombie ? "Zombie_Dying" : adaBiasa ? "Dying" : "";
            return adaBiasa ? "Dying" : adaZombie ? "Zombie_Dying" : "";
        }

        // Klip Mixamo bukan "in place": kunci geser horizontal pinggul agar posisi tetap dikendalikan AI.
        void KunciPinggul()
        {
            var p = _hipsModel.localPosition;
            _hipsModel.localPosition = new Vector3(_hipsModel0.x, p.y, _hipsModel0.z);
        }
    }
}
