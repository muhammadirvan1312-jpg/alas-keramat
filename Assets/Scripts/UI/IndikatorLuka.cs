using System;
using AlasKeramat.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    // Umpan balik terkena: vinyet merah di tepi layar (kuat sesuai damage) dan busur penunjuk arah penyerang.
    public class IndikatorLuka : MonoBehaviour
    {
        public const float LamaVinyet = 1.1f, LamaArah = 1.6f, AlfaMaks = 0.65f;
        [SerializeField] Image vinyet, arah;
        float _tV, _aV, _tA;
        Vector3 _sumber;
        bool _gagal;

        public void Pasang(Image v, Image a) { vinyet = v; arah = a; }

        public static float AlfaUntuk(float damage) { return Mathf.Clamp(0.18f + damage / 45f, 0f, AlfaMaks); }

        // Sudut layar (derajat, 0 = depan/atas, positif = kanan) dari arah hadap ke sumber serangan.
        public static float SudutArah(Vector3 maju, Vector3 dariPemainKeSumber)
        {
            maju.y = 0f; dariPemainKeSumber.y = 0f;
            if (maju.sqrMagnitude < 1e-4f || dariPemainKeSumber.sqrMagnitude < 1e-4f) return 0f;
            return Vector3.SignedAngle(maju, dariPemainKeSumber, Vector3.up);
        }

        void OnEnable() { GameEvents.OnPlayerDamaged += Kena; }
        void OnDisable() { GameEvents.OnPlayerDamaged -= Kena; }

        void Kena(DamageInfo d)
        {
            if (d.type == DamageType.Tenggelam || d.amount < 1f) return;
            _aV = Mathf.Max(_aV * (_tV / LamaVinyet), AlfaUntuk(d.amount)); _tV = LamaVinyet;
            if (d.source != null) { _sumber = d.source.transform.position; _tA = LamaArah; }
        }

        void Update()
        {
            if (_gagal || vinyet == null || arah == null) return;
            try
            {
                float dt = Time.unscaledDeltaTime;
                _tV = Mathf.Max(0f, _tV - dt); _tA = Mathf.Max(0f, _tA - dt);
                var cv = vinyet.color; cv.a = _aV * (_tV / LamaVinyet); vinyet.color = cv; vinyet.enabled = cv.a > 0.002f;
                var ca = arah.color; ca.a = 0.85f * Mathf.Clamp01(_tA / LamaArah * 1.5f); arah.color = ca; arah.enabled = ca.a > 0.002f;
                if (arah.enabled)
                {
                    var cam = Camera.main;
                    if (cam != null) arah.rectTransform.localEulerAngles = new Vector3(0f, 0f, -SudutArah(cam.transform.forward, _sumber - cam.transform.position));
                }
            }
            catch (Exception e) { _gagal = true; Debug.LogError("[AK][IndikatorLuka] Gagal: " + e.Message); }
        }
    }
}
