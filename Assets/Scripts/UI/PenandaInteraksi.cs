using System;
using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    // Penanda bulat berdenyut di atas benda yang bisa diambil/dipakai, seperti game survival modern.
    public class PenandaInteraksi : MonoBehaviour
    {
        public const float Pudar = 8f, Denyut = 3.2f;
        [SerializeField] Image penanda;
        InteraksiAndi _inter;
        float _a;
        bool _gagal;

        public void Pasang(Image i) { penanda = i; }

        public static float SkalaDenyut(float waktu) { return 1f + 0.12f * Mathf.Sin(waktu * Denyut); }

        void LateUpdate()
        {
            if (_gagal || penanda == null) return;
            try
            {
                if (_inter == null) _inter = GameServices.Get<InteraksiAndi>();
                var cam = Camera.main;
                bool tampil = false;
                if (_inter != null && _inter.AdaBenda && cam != null)
                {
                    Vector3 s = cam.WorldToScreenPoint(_inter.PosisiTarget);
                    if (s.z > 0.2f)
                    {
                        var rtInduk = (RectTransform)penanda.rectTransform.parent;
                        Vector2 lokal;
                        var kanvas = penanda.canvas;
                        Camera kamUI = kanvas != null && kanvas.renderMode != RenderMode.ScreenSpaceOverlay ? kanvas.worldCamera : null;
                        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rtInduk, s, kamUI, out lokal))
                        { penanda.rectTransform.anchoredPosition = lokal; tampil = true; }
                    }
                }
                _a = Mathf.MoveTowards(_a, tampil ? 0.9f : 0f, Time.unscaledDeltaTime * Pudar);
                var c = penanda.color; c.a = _a; penanda.color = c; penanda.enabled = _a > 0.01f;
                penanda.rectTransform.localScale = Vector3.one * SkalaDenyut(Time.unscaledTime);
            }
            catch (Exception e) { _gagal = true; Debug.LogError("[AK][PenandaInteraksi] Gagal: " + e.Message); }
        }
    }
}
