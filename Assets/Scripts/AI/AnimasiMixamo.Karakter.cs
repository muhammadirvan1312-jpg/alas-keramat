using System;
using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Model karakter Mixamo asli (bertulang mixamorig) menggantikan tampilan prosedural:
    // klip legacy diputar langsung pada model, tinggi disamakan, material disesuaikan Indonesia,
    // LookAt kepala & kedip mata halus, mata menyala malam monster, serta blob shadow di preset Rendah.
    public partial class AnimasiMixamo
    {
        [SerializeField] GameObject modelKarakter;
        [SerializeField] Shader shaderKulit;
        [SerializeField] bool adalahKuntilanak, mataMenyalaMalam;
        [SerializeField] string namaIdentitas;
        GameObject _model, _mataMalam, _bayanganKontak;
        Transform _hipsModel, _kepalaModel;
        Vector3 _hipsModel0;
        Quaternion _lihatHalus = Quaternion.identity;
        float _sudutLihat, _tKedip = 2.5f;
        readonly List<Renderer> _lama = new List<Renderer>();
        bool _modelTampil = true;
        TimeOfDayClock _jamDunia;

        public void PasangModel(GameObject prefab) { modelKarakter = prefab; }
        public void PasangShaderKulit(Shader s) { shaderKulit = s; }
        public void PasangOpsiVisual(bool kunti, bool mataMalam, string identitas = "") { adalahKuntilanak = kunti; mataMenyalaMalam = mataMalam; namaIdentitas = identitas ?? ""; }
        public bool PakaiModelAsli { get { return _model != null; } }

        public static bool AdalahMaterialKulit(string namaMat)
        {
            if (string.IsNullOrEmpty(namaMat)) return false;
            string n = namaMat.ToLowerInvariant();
            if (n.Contains("cloth") || n.Contains("shirt") || n.Contains("pant") || n.Contains("shoe") || n.Contains("boot")
                || n.Contains("hair") || n.Contains("brow") || n.Contains("lash") || n.Contains("eye")
                || n.Contains("dress") || n.Contains("skirt") || n.Contains("kain") || n.Contains("belt")
                || n.Contains("top") || n.Contains("bottom") || n.Contains("jacket") || n.Contains("vest") || n.Contains("hat")) return false;
            return n.Contains("skin") || n.Contains("body") || n.Contains("head") || n.Contains("face")
                || n.Contains("arm") || n.Contains("hand") || n.Contains("leg") || n.Contains("kulit")
                || n.Contains("ch33") || n.Contains("remy_body") || n.Contains("flesh");
        }

        public static bool AdalahMaterialRambut(string namaMat)
        {
            if (string.IsNullOrEmpty(namaMat)) return false;
            string n = namaMat.ToLowerInvariant();
            return n.Contains("hair") || n.Contains("brow") || n.Contains("lash") || n.Contains("rambut") || n.Contains("beard") || n.Contains("ponytail");
        }

        public static bool AdalahMaterialMata(string namaMat)
        {
            if (string.IsNullOrEmpty(namaMat)) return false;
            string n = namaMat.ToLowerInvariant();
            if (n.Contains("lash") || n.Contains("brow")) return false;
            return n.Contains("eye") || n.Contains("iris") || n.Contains("cornea") || n.Contains("mata");
        }

        public static bool AdalahMaterialSepatu(string namaMat)
        {
            if (string.IsNullOrEmpty(namaMat)) return false;
            string n = namaMat.ToLowerInvariant();
            return n.Contains("shoe") || n.Contains("boot") || n.Contains("sandal") || n.Contains("foot") || n.Contains("sepatu");
        }

        public static Color WarnaKulitIndonesia(Peran p, bool kuntilanak)
        {
            if (kuntilanak) return new Color(0.74f, 0.78f, 0.77f);
            if (p == Peran.Wanita) return new Color(0.88f, 0.70f, 0.56f);
            if (p == Peran.Pria) return new Color(0.80f, 0.60f, 0.46f);
            if (p == Peran.Raksasa) return new Color(0.32f, 0.24f, 0.20f);
            if (p == Peran.Sekte) return new Color(0.70f, 0.54f, 0.42f);
            return new Color(0.80f, 0.62f, 0.48f);
        }

        public static Color WarnaPakaianFallback(Peran p, bool kuntilanak, string identitas, string namaMat)
        {
            string id = (identitas ?? "").ToLowerInvariant();
            string mn = (namaMat ?? "").ToLowerInvariant();
            bool bawahan = mn.Contains("bottom") || mn.Contains("pant") || mn.Contains("skirt") || mn.Contains("trouser") || mn.Contains("jarik");
            if (kuntilanak) return new Color(0.76f, 0.78f, 0.74f, 1f);
            if (id.Contains("kirogo"))
                return bawahan ? new Color(0.28f, 0.20f, 0.14f, 1f) : new Color(0.42f, 0.18f, 0.14f, 1f);
            if (id.Contains("ningsih") || p == Peran.Wanita)
                return bawahan ? new Color(0.56f, 0.34f, 0.18f, 1f) : new Color(0.90f, 0.84f, 0.72f, 1f);
            if (id.Contains("deden") || p == Peran.Pria || p == Peran.Terluka)
                return bawahan ? new Color(0.42f, 0.36f, 0.26f, 1f) : new Color(0.34f, 0.39f, 0.26f, 1f);
            if (p == Peran.Raksasa) return new Color(0.24f, 0.17f, 0.14f, 1f);
            return bawahan ? new Color(0.26f, 0.20f, 0.16f, 1f) : new Color(0.46f, 0.22f, 0.16f, 1f);
        }

        public static bool PerluBayanganKontak(ShadowQuality q, float shadowDistance)
        {
            return q == ShadowQuality.Disable || shadowDistance <= 0.1f;
        }

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
            foreach (var t in _model.GetComponentsInChildren<Transform>(true))
            {
                if (_hipsModel == null && t.name.EndsWith(":Hips")) _hipsModel = t;
                if (_kepalaModel == null && (t.name.EndsWith(":Head") || t.name == "Head")) _kepalaModel = t;
            }
            if (_hipsModel == null) { Destroy(_model); _model = null; return false; }
            _hipsModel0 = _hipsModel.localPosition;
            foreach (var r in _model.GetComponentsInChildren<Renderer>(true)) { r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; }
            KonversiMaterialIndonesia();
            PasangMataMalamDanBayanganKontak();
            _anim = _model.GetComponent<Animation>();
            if (_anim == null) _anim = _model.AddComponent<Animation>();
            _anim.playAutomatically = false;
            _anim.cullingType = AnimationCullingType.BasedOnRenderers;
            foreach (var k in bank.klip) if (k != null && _anim.GetClip(k.name) == null) _anim.AddClip(k, k.name);
            _modelTampil = false; TampilModel(true);
            return true;
        }

        static readonly Dictionary<string, Material> _cacheMatMixamo = new Dictionary<string, Material>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetCacheMixamo() { _cacheMatMixamo.Clear(); }

        void KonversiMaterialIndonesia()
        {
            TerapkanMaterialVisual(_model, peran, adalahKuntilanak, string.IsNullOrEmpty(namaIdentitas) ? name : namaIdentitas, shaderKulit, true);
        }

        // Konversi material FBX Mixamo agar selalu memakai shader URP/KulitManusia + warna Nusantara (tidak pernah putih kapur).
        public static void TerapkanMaterialVisual(GameObject model, Peran p, bool kunti, string identitas, Shader shKulit, bool pakaiCache)
        {
            if (model == null) return;
            string id = identitas ?? "";
            string tag = (kunti ? "Kunti" : p.ToString()) + "_" + id;
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            bool kiRogo = id.ToLowerInvariant().Contains("kirogo");

            foreach (var r in model.GetComponentsInChildren<Renderer>(true))
            {
                var src = r.sharedMaterials;
                var dst = new Material[src.Length];
                string namaObj = r.gameObject.name;
                for (int i = 0; i < src.Length; i++)
                {
                    var m = src[i];
                    if (m == null) { dst[i] = null; continue; }
                    string kunciNama = m.name + "_" + namaObj;
                    string k = tag + "_" + kunciNama;
                    Material cached;
                    if (pakaiCache && _cacheMatMixamo.TryGetValue(k, out cached) && cached != null) { dst[i] = cached; continue; }

                    Texture baseTex = m.HasProperty("_BaseMap") ? m.GetTexture("_BaseMap") : null;
                    if (baseTex == null && m.HasProperty("_MainTex")) baseTex = m.GetTexture("_MainTex");
                    Texture bumpTex = m.HasProperty("_BumpMap") ? m.GetTexture("_BumpMap") : null;
                    bool punyaTex = baseTex != null;

                    var clone = new Material(m) { name = "AK_" + k };
                    // Jika FBX masih memakai shader Standard/Legacy atau shader hilang, naikkan ke URP/Lit terlebih dulu.
                    if (urpLit != null && (clone.shader == null || !clone.shader.name.Contains("Universal") && !clone.shader.name.Contains("AlasKeramat")))
                    {
                        clone.shader = urpLit;
                        if (baseTex != null) SafeMaterialFactory.SetT(clone, "_BaseMap", baseTex);
                        if (bumpTex != null) SafeMaterialFactory.SetT(clone, "_BumpMap", bumpTex);
                    }

                    if (AdalahMaterialRambut(kunciNama))
                    {
                        Color rambut = kunti ? new Color(0.06f, 0.06f, 0.07f, 1f)
                            : kiRogo ? new Color(0.38f, 0.36f, 0.34f, 1f)
                            : new Color(0.09f, 0.068f, 0.055f, 1f);
                        SafeMaterialFactory.SetC(clone, "_BaseColor", rambut);
                        SafeMaterialFactory.SetC(clone, "_Color", rambut);
                        SafeMaterialFactory.SetF(clone, "_Smoothness", 0.14f);
                        SafeMaterialFactory.SetF(clone, "_EnvironmentReflections", 0f);
                        clone.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
                        if (punyaTex)
                        {
                            SafeMaterialFactory.SetF(clone, "_AlphaClip", 1f);
                            SafeMaterialFactory.SetF(clone, "_Cutoff", 0.34f);
                            clone.EnableKeyword("_ALPHATEST_ON");
                        }
                    }
                    else if (AdalahMaterialMata(kunciNama))
                    {
                        Color mata = kunti ? new Color(0.88f, 0.24f, 0.18f, 1f)
                            : p == Peran.Raksasa ? new Color(0.95f, 0.32f, 0.10f, 1f)
                            : (punyaTex ? Color.white : new Color(0.18f, 0.11f, 0.07f, 1f));
                        SafeMaterialFactory.SetC(clone, "_BaseColor", mata);
                        SafeMaterialFactory.SetC(clone, "_Color", mata);
                        SafeMaterialFactory.SetF(clone, "_Smoothness", 0.94f);
                        if (kunti || p == Peran.Raksasa)
                        {
                            SafeMaterialFactory.SetC(clone, "_EmissionColor", kunti ? new Color(1.1f, 0.18f, 0.12f) : new Color(1.8f, 0.32f, 0.08f));
                            clone.EnableKeyword("_EMISSION");
                        }
                    }
                    else if (AdalahMaterialSepatu(kunciNama))
                    {
                        Color sep = punyaTex ? new Color(0.85f, 0.80f, 0.74f, 1f) : new Color(0.24f, 0.18f, 0.14f, 1f);
                        SafeMaterialFactory.SetC(clone, "_BaseColor", sep);
                        SafeMaterialFactory.SetC(clone, "_Color", sep);
                        SafeMaterialFactory.SetF(clone, "_Smoothness", 0.24f);
                    }
                    else if (AdalahMaterialKulit(kunciNama))
                    {
                        if (shKulit != null && !kunti)
                        {
                            clone.shader = shKulit;
                            if (baseTex != null) SafeMaterialFactory.SetT(clone, "_BaseMap", baseTex);
                            if (bumpTex != null) SafeMaterialFactory.SetT(clone, "_BumpMap", bumpTex);
                            SafeMaterialFactory.SetF(clone, "_BumpScale", 0.68f);
                        }
                        Color kulit = WarnaKulitIndonesia(p, kunti);
                        SafeMaterialFactory.SetC(clone, "_BaseColor", kulit);
                        SafeMaterialFactory.SetC(clone, "_Color", kulit);
                        SafeMaterialFactory.SetF(clone, "_Smoothness", kunti ? 0.22f : 0.34f);
                    }
                    else
                    {
                        // Pakaian / kain: bila FBX tidak punya tekstur, gunakan warna pakaian khas karakter (bukan putih polos).
                        Color c1;
                        if (punyaTex)
                        {
                            Color c0 = clone.HasProperty("_BaseColor") ? clone.GetColor("_BaseColor") : Color.white;
                            c1 = kunti ? new Color(0.82f, 0.83f, 0.80f, 1f) : Color.Lerp(c0, new Color(c0.r * 0.92f, c0.g * 0.88f, c0.b * 0.82f, c0.a), 0.50f);
                        }
                        else
                        {
                            c1 = WarnaPakaianFallback(p, kunti, id, kunciNama);
                        }
                        SafeMaterialFactory.SetC(clone, "_BaseColor", c1);
                        SafeMaterialFactory.SetC(clone, "_Color", c1);
                        SafeMaterialFactory.SetF(clone, "_Smoothness", 0.16f);
                    }
                    if (pakaiCache) _cacheMatMixamo[k] = clone;
                    dst[i] = clone;
                }
                r.sharedMaterials = dst;
            }
        }

        void PasangMataMalamDanBayanganKontak()
        {
            if (mataMenyalaMalam && _mataMalam == null)
            {
                _mataMalam = new GameObject("MataMenyalaMalam");
                Transform indukMata = _kepalaModel != null ? _kepalaModel : _model.transform;
                _mataMalam.transform.SetParent(indukMata, false);
                _mataMalam.transform.localPosition = _kepalaModel != null ? new Vector3(0f, 0.06f, 0.11f) : new Vector3(0f, 1.88f, 0.14f);
                var mMata = SafeMaterialFactory.CreatePBR("M_MataMonster", new Color(1f, 0.24f, 0.08f), 0f, 0.9f, null, null, new Color(3.4f, 0.55f, 0.14f));
                for (int s = -1; s <= 1; s += 2)
                {
                    var e = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    e.name = s < 0 ? "MataKiri" : "MataKanan";
                    var col = e.GetComponent<Collider>(); if (col != null) Destroy(col);
                    e.transform.SetParent(_mataMalam.transform, false);
                    e.transform.localPosition = new Vector3(s * 0.036f, 0f, 0f);
                    e.transform.localScale = Vector3.one * 0.022f;
                    var mr = e.GetComponent<MeshRenderer>();
                    mr.sharedMaterial = mMata; mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
            if (_bayanganKontak == null)
            {
                _bayanganKontak = GameObject.CreatePrimitive(PrimitiveType.Quad);
                _bayanganKontak.name = "BayanganKontak";
                var col = _bayanganKontak.GetComponent<Collider>(); if (col != null) Destroy(col);
                _bayanganKontak.transform.SetParent(transform, false);
                _bayanganKontak.transform.localPosition = new Vector3(0f, 0.035f, 0f);
                _bayanganKontak.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                float d = peran == Peran.Raksasa ? 1.45f : 0.85f;
                _bayanganKontak.transform.localScale = new Vector3(d, d, 1f);
                var mr = _bayanganKontak.GetComponent<MeshRenderer>();
                mr.sharedMaterial = SafeMaterialFactory.CreatePBR("M_BayanganKontak", new Color(0.02f, 0.02f, 0.02f, 0.36f), 0f, 0f, null, null, null, SurfaceMode.Transparent);
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = false;
            }
        }

        void GerakKepalaDanMata()
        {
            if (_kepalaModel != null && _kamera != null && !mataMenyalaMalam)
            {
                Vector3 ke = _kamera.position - _kepalaModel.position;
                float dSq = ke.sqrMagnitude;
                float targetYaw = 0f;
                if (dSq > 0.25f && dSq < 144f)
                {
                    Vector3 lokal = transform.InverseTransformDirection(ke.normalized);
                    if (lokal.z > 0.2f) targetYaw = Mathf.Clamp(Mathf.Atan2(lokal.x, lokal.z) * Mathf.Rad2Deg, -28f, 28f);
                }
                _sudutLihat = Mathf.MoveTowards(_sudutLihat, targetYaw, Time.deltaTime * 90f);
                _tKedip -= Time.deltaTime;
                if (_tKedip <= 0f) _tKedip = UnityEngine.Random.Range(2.4f, 5.2f);
                float angguk = Mathf.Sin(Time.time * 1.4f) * 1.8f;
                _kepalaModel.localRotation = _kepalaModel.localRotation * Quaternion.Euler(angguk, _sudutLihat, 0f);
            }
            if (_mataMalam != null)
            {
                if (_jamDunia == null) _jamDunia = GameServices.Get<TimeOfDayClock>();
                float j = _jamDunia != null ? _jamDunia.Jam : 12f;
                bool nyala = WorldTerrainGenerator.DiBawahTanah(transform.position.y) || j >= 18.0f || j <= 5.8f;
                if (_mataMalam.activeSelf != nyala) _mataMalam.SetActive(nyala);
            }
        }

        void PerbaruiBayanganKontak()
        {
            if (_bayanganKontak == null) return;
            bool perlu = _modelTampil && PerluBayanganKontak(QualitySettings.shadows, QualitySettings.shadowDistance);
            if (_bayanganKontak.activeSelf != perlu) _bayanganKontak.SetActive(perlu);
        }

        // Mode normal: model Mixamo tampil, tubuh prosedural disembunyikan; mode lain (mati, ragdoll, sinema) sebaliknya.
        void TampilModel(bool tampil)
        {
            if (tampil == _modelTampil || _model == null) return;
            _modelTampil = tampil;
            _model.SetActive(tampil);
            foreach (var r in _lama) if (r != null) r.forceRenderingOff = tampil;
            if (!tampil && _bayanganKontak != null) _bayanganKontak.SetActive(false);
        }

        bool _mati;
        // Penggerak prosedural dimatikan saat musuh tewas: model Mixamo memutar klip roboh sekali lalu diam di tanah.
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
