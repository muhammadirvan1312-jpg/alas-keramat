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
        [SerializeField] Image[] kotakZirah = new Image[0];
        int _zirahTampil = -1; float _durTampil = -1f;

        Color[] _warnaAsli;
        readonly float[] _tampil = { 1f, 1f, 1f, 1f, 1f };
        AndiVitalsAndSanity _v;
        int _suhuTampil = -1, _basahTampil = -1;

        public void PasangZirah(Image[] k) { kotakZirah = k; }

        public static Color WarnaZirah(ItemID id)
        {
            switch (id) { case ItemID.ZirahDaun: return new Color(0.5f, 0.65f, 0.31f); case ItemID.ZirahBambu: return new Color(0.76f, 0.65f, 0.4f); case ItemID.ZirahTulang: return new Color(0.91f, 0.86f, 0.75f); case ItemID.ZirahBaja: return new Color(0.62f, 0.64f, 0.65f); default: return new Color(0.91f, 0.78f, 0.4f); }
        }

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
            PerbaruiZirah();
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

        void PerbaruiZirah()
        {
            int n = _v.JumlahSegmen;
            float dur = n > 0 ? _v.AmbilSegmen(n - 1).dur : 0f;
            if (n == _zirahTampil && Mathf.Approximately(dur, _durTampil)) return;
            _zirahTampil = n; _durTampil = dur;
            for (int i = 0; i < kotakZirah.Length; i++)
            {
                if (kotakZirah[i] == null) continue;
                if (i < n) { var sg = _v.AmbilSegmen(i); var c = WarnaZirah(sg.jenis); c.a = 0.4f + 0.6f * sg.dur / Mathf.Max(0.01f, sg.durMaks); kotakZirah[i].color = c; }
                else kotakZirah[i].color = new Color(1f, 1f, 1f, 0.12f);
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
