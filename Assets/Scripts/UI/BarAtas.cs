using AlasKeramat.Core;
using UnityEngine;
using UnityEngine.UI;
namespace AlasKeramat.UI
{
    // Bar di atas tengah layar: HP boss "GENDERUWO" (04 §7.5) dan waktu "BERTAHAN 60 DETIK" (04 §7.6).
    public class BarAtas : MonoBehaviour
    {
        [SerializeField] GameObject akar;
        [SerializeField] RectTransform isi;
        [SerializeField] Image warnaIsi;
        [SerializeField] Text label;
        float _nilai = -1f;
        string _teks = "";

        public void Pasang(GameObject r, RectTransform fill, Image img, Text t) { akar = r; isi = fill; warnaIsi = img; label = t; }
        void Awake() { GameServices.Register(this); Sembunyi(); }

        public void Tampil(string teks, float nilai01, Color warna)
        {
            if (akar == null) return;
            if (!akar.activeSelf) akar.SetActive(true);
            nilai01 = Mathf.Clamp01(nilai01);
            if (isi != null && Mathf.Abs(nilai01 - _nilai) > 0.001f) { _nilai = nilai01; isi.anchorMax = new Vector2(nilai01, 1f); }
            if (label != null && teks != _teks) { _teks = teks; label.text = teks; }
            if (warnaIsi != null) warnaIsi.color = warna;
        }

        public void Sembunyi() { if (akar != null) akar.SetActive(false); _nilai = -1f; _teks = ""; }
    }
}
