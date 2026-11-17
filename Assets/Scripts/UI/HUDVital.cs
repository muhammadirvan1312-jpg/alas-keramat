using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    // Bar vital kiri-atas (09 §2.1): darah (+ arsiran di atas plafon), stamina (+ bagian beku), kenyang, haus, kewarasan (berpendar).
    // Isi bar = skala X anak "Isi" (tanpa sprite). Lerp 6/s, berkedip merah < 20%.
    public class HUDVital : MonoBehaviour
    {
        public const float LajuLerp = 6f, AmbangKritis = 0.2f, FrekKedip = 3f, FrekPendar = 0.5f;
        public const int Darah = 0, Stamina = 1, Kenyang = 2, Haus = 3, Waras = 4, Jumlah = 5;

        [SerializeField] RectTransform[] isi = new RectTransform[Jumlah];
        [SerializeField] Image[] gambar = new Image[Jumlah];
        [SerializeField] RectTransform plafonDarah;
        [SerializeField] RectTransform bekuStamina;
        [SerializeField] Text labelSuhu;

        Color[] _warnaAsli;
        readonly float[] _tampil = { 1f, 1f, 1f, 1f, 1f };
        AndiVitalsAndSanity _v;
        int _suhuTampil = -1, _basahTampil = -1;

        public void Pasang(RectTransform[] i, Image[] g, RectTransform plafon, RectTransform beku, Text suhu)
        {
            isi = i; gambar = g; plafonDarah = plafon; bekuStamina = beku; labelSuhu = suhu;
        }

        void Awake()
        {
            _warnaAsli = new Color[Jumlah];
            for (int i = 0; i < Jumlah && i < gambar.Length; i++) if (gambar[i] != null) _warnaAsli[i] = gambar[i].color;
        }

        void Update()
        {
            if (_v == null) { _v = GameServices.Get<AndiVitalsAndSanity>(); if (_v == null) return; }
            float m = AndiVitalsAndSanity.Maks;
            Set(Darah, _v.Health / m);
            Set(Stamina, _v.Stamina / m);
            Set(Kenyang, _v.Fullness / m);
            Set(Haus, _v.Hydration / m);
            Set(Waras, _v.Sanity / m);
            if (plafonDarah != null) { float p = _v.TargetHealthCeiling / m; plafonDarah.anchorMin = new Vector2(p, 0f); }
            if (bekuStamina != null) { float c = _v.MaxStaminaCap / m; bekuStamina.anchorMin = new Vector2(c, 0f); }
            if (gambar.Length > Waras && gambar[Waras] != null)
            {
                var c = _warnaAsli[Waras];
                c.a = 0.6f + 0.4f * (0.5f + 0.5f * Mathf.Sin(Time.time * FrekPendar * Mathf.PI * 2f));
                if (_tampil[Waras] >= AmbangKritis) gambar[Waras].color = c;
            }
            if (labelSuhu != null)
            {
                int suhu = Mathf.RoundToInt(_v.SuhuTubuh * 10f), basah = Mathf.RoundToInt(_v.Wetness);
                if (suhu != _suhuTampil || basah != _basahTampil)
                {
                    _suhuTampil = suhu; _basahTampil = basah;
                    labelSuhu.text = "Suhu " + (suhu / 10) + "," + (suhu % 10) + " C  |  Basah " + basah + "%" + (_v.Diare ? "  |  DIARE" : "") + ((_v.Status & StatusEffect.Keracunan) != 0 ? "  |  KERACUNAN" : "");
                }
            }
        }

        void Set(int i, float target)
        {
            if (i >= isi.Length || isi[i] == null) return;
            _tampil[i] = Mathf.MoveTowards(_tampil[i], Mathf.Clamp01(target), Time.deltaTime * LajuLerp * Mathf.Max(0.05f, Mathf.Abs(target - _tampil[i])));
            var s = isi[i].localScale; s.x = _tampil[i]; isi[i].localScale = s;
            if (i >= gambar.Length || gambar[i] == null) return;
            bool kritis = _tampil[i] < AmbangKritis;
            if (i == Waras && !kritis) return; // warna kewarasan diatur pendar di Update
            bool merah = kritis && Mathf.Repeat(Time.time * FrekKedip, 1f) < 0.5f;
            gambar[i].color = merah ? new Color(0.9f, 0.2f, 0.2f, 1f) : _warnaAsli[i];
        }
    }
}
