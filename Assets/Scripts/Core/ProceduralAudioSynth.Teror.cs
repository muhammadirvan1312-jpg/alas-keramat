using UnityEngine;

namespace AlasKeramat.Core
{
    // T9 (13 §5.3): suara sintetis teror - bisikan "Andiii", panggilan "An... di", tangis bayi sayup, kokok ayam, kedasih, napas berat,
    // ketukan dinding, kerikil di atap, jeritan gua, gamelan slendro jauh. Cadangan prosedural (rekaman CC0 menggantikan, 21 §2).
    public partial class ProceduralAudioSynth
    {
        public const string KlipBisikan = "teror_bisikan", KlipPanggilan = "teror_panggilan", KlipTangisBayi = "teror_tangis", KlipKokok = "teror_kokok",
            KlipKedasih = "teror_kedasih", KlipNapasBerat = "teror_napas", KlipKetukan = "teror_ketukan", KlipKerikil = "teror_kerikil",
            KlipJeritGua = "teror_jerit_gua", KlipGamelanJauh = "teror_gamelan", KlipStinger = "teror_stinger", KlipPesawat = "dunia_pesawat";
        public static readonly string[] KlipTeror = { KlipBisikan, KlipPanggilan, KlipTangisBayi, KlipKokok, KlipKedasih, KlipNapasBerat, KlipKetukan, KlipKerikil, KlipJeritGua, KlipGamelanJauh, KlipStinger, KlipPesawat };

        public static float[] GenerateTeror(string nama, int variasi, out int sr)
        {
            sr = Dsp.SrSfx;
            var rng = Dsp.Rng.DariNama(nama, variasi);
            float[] b;
            switch (nama)
            {
                case KlipBisikan: b = Vokal(sr, ref rng, 1.8f, 0f, new[] { 730f, 1090f, 2440f }, new[] { 300f, 1300f, 2200f }, 0.55f); break; // "aaa..n-diii" berbisik
                case KlipPanggilan: b = Vokal(sr, ref rng, 1.6f, 125f, new[] { 730f, 1090f }, new[] { 300f, 2300f }, 0.45f); break;
                case KlipTangisBayi: b = Tangis(sr, ref rng); break;
                case KlipKokok: b = Kokok(sr, ref rng); break;
                case KlipKedasih: b = Kedasih(sr, ref rng); break;
                case KlipNapasBerat: b = Napas(sr, ref rng); break;
                case KlipKetukan: b = Ketuk(sr, ref rng, 3, 0.55f, 180f); break;
                case KlipKerikil: b = Ketuk(sr, ref rng, 8, 0.32f, 900f); break;
                case KlipJeritGua: b = Jerit(sr, ref rng); break;
                case KlipGamelanJauh: b = GamelanJauh(sr, ref rng); break;
                case KlipStinger: b = Stinger(sr, ref rng); break;
                case KlipPesawat: b = Pesawat(sr, ref rng); break;
                default: return null;
            }
            Dsp.Finalize(b, sr);
            return b;
        }

        // Dua suku kata: noise (bisik) atau gergaji (suara) lewat formant yang bergeser dari suku 1 ke suku 2.
        static float[] Vokal(int sr, ref Dsp.Rng rng, float lama, float f0, float[] f1, float[] f2, float jeda)
        {
            var b = Dsp.Buffer(lama, sr);
            float fase = 0f;
            var a1 = Dsp.Biquad.BP(f1[0], 6f, sr); var a2 = Dsp.Biquad.BP(f1[1], 8f, sr);
            var b1 = Dsp.Biquad.BP(f2[0], 6f, sr); var b2 = Dsp.Biquad.BP(f2[1], 8f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                fase += Dsp.DuaPi * f0 * (1f + 0.04f * Mathf.Sin(t * 5f)) / sr;
                float src = f0 > 0f ? Dsp.Saw(fase) * 0.6f + rng.White() * 0.25f : rng.White();
                bool kedua = t > lama * jeda;
                float env = kedua ? Dsp.AD(t - lama * jeda, 0.08f, lama * (1f - jeda) * 0.8f) : Dsp.AD(t, 0.12f, lama * jeda * 0.7f);
                float y = kedua ? b1.Process(src) + b2.Process(src) * 0.6f : a1.Process(src) + a2.Process(src) * 0.6f;
                b[i] = y * env;
            }
            Dsp.Schroeder(b, 0.25f, sr);
            return b;
        }

        static float[] Tangis(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(3f, sr);
            float fase = 0f;
            var lp = Dsp.Biquad.LP(2500f, 0.7f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr, siklus = Mathf.Repeat(t, 1f);
                float f = 420f + 160f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(siklus / 0.8f)) + 20f * Mathf.Sin(t * 37f);
                fase += Dsp.DuaPi * f / sr;
                float env = siklus < 0.8f ? Mathf.Sin(Mathf.PI * siklus / 0.8f) : 0f;
                b[i] = lp.Process(Dsp.Saw(fase) * 0.6f + rng.White() * 0.1f) * env * 0.5f;
            }
            Dsp.Formant(b, 900f, 2400f, sr);
            Dsp.Schroeder(b, 0.5f, sr); // sayup, jauh di pepohonan
            return b;
        }

        static float[] Kokok(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(1.6f, sr);
            float fase = 0f;
            float[] awal = { 0f, 0.18f, 0.36f, 0.7f }, nada = { 600f, 700f, 760f, 650f }, lama = { 0.15f, 0.15f, 0.3f, 0.7f };
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr, y = 0f;
                for (int k = 0; k < 4; k++)
                {
                    float u = t - awal[k];
                    if (u < 0f || u > lama[k]) continue;
                    float f = nada[k] * (k == 3 ? Mathf.Lerp(1.1f, 0.85f, u / lama[k]) : 1f);
                    fase += Dsp.DuaPi * f / sr;
                    y += Dsp.SoftClip(Dsp.Saw(fase) * 2f) * Mathf.Sin(Mathf.PI * u / lama[k]);
                }
                b[i] = y * 0.5f + rng.White() * 0.02f;
            }
            Dsp.Formant(b, 1100f, 2600f, sr);
            Dsp.Schroeder(b, 0.6f, sr);
            return b;
        }

        // "pi-pi-pi-piii" sendu: siulan menurun.
        static float[] Kedasih(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(2.4f, sr);
            float fase = 0f;
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                int k = Mathf.Min(3, (int)(t / 0.38f));
                float u = t - k * 0.38f, panjang = k == 3 ? 1.1f : 0.22f;
                float env = u < panjang ? Mathf.Sin(Mathf.PI * u / panjang) : 0f;
                float f = 2600f - k * 180f - (k == 3 ? 500f * u : 0f);
                fase += Dsp.DuaPi * f / sr;
                b[i] = Mathf.Sin(fase) * env * 0.6f;
            }
            Dsp.Schroeder(b, 0.7f, sr);
            return b;
        }

        static float[] Napas(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(4f, sr);
            var bp = Dsp.Biquad.BP(700f, 0.9f, sr); var lp = Dsp.Biquad.LP(300f, 0.7f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr, s = Mathf.Repeat(t, 2f);
                float env = s < 0.9f ? Mathf.Sin(Mathf.PI * s / 0.9f) : s > 1.0f && s < 1.9f ? 0.7f * Mathf.Sin(Mathf.PI * (s - 1f) / 0.9f) : 0f;
                float w = rng.White();
                b[i] = (bp.Process(w) + lp.Process(w) * 1.5f) * env * 0.8f;
            }
            return b;
        }

        static float[] Ketuk(int sr, ref Dsp.Rng rng, int n, float jeda, float f)
        {
            var b = Dsp.Buffer(n * jeda + 0.6f, sr);
            for (int k = 0; k < n; k++)
            {
                float t0 = k * jeda + rng.Range(-0.04f, 0.04f) * (n > 3 ? 1f : 0f);
                int awal = Mathf.Max(0, Mathf.RoundToInt(t0 * sr));
                var bp = Dsp.Biquad.BP(f * rng.Range(0.9f, 1.15f), 2f, sr);
                for (int i = 0; i < sr / 4 && awal + i < b.Length; i++)
                {
                    float t = (float)i / sr;
                    b[awal + i] += (bp.Process(rng.White()) * 0.8f + Mathf.Sin(Dsp.DuaPi * f * 0.5f * t) * 0.6f) * Dsp.ExpDecay(t, 0.03f);
                }
            }
            return b;
        }

        static float[] Jerit(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(3f, sr);
            float fase = 0f;
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                float f = 520f + 280f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / 2.2f)) + 30f * Mathf.Sin(t * 41f);
                fase += Dsp.DuaPi * f / sr;
                b[i] = Dsp.SoftClip((Dsp.Saw(fase) + rng.White() * 0.3f) * 1.6f) * Dsp.ADSR(t, 0.1f, 0.3f, 0.8f, 1.9f, 0.6f) * 0.5f;
            }
            Dsp.Formant(b, 800f, 1900f, sr);
            Dsp.Schroeder(b, 0.9f, sr); // gema gua
            return b;
        }

        // Gamelan slendro sayup (5 nada) dari kejauhan: bonang lambat + gong, difilter low-pass & bergema.
        static readonly float[] Slendro = { 261f, 299f, 344f, 395f, 454f };
        static float[] GamelanJauh(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(8f, sr);
            int[] pola = { 0, 2, 1, 3, 2, 4, 3, 1 };
            for (int k = 0; k < pola.Length; k++)
            {
                int awal = Mathf.RoundToInt(k * sr);
                float f = Slendro[pola[k]];
                for (int i = 0; i < sr * 2 && awal + i < b.Length; i++)
                {
                    float t = (float)i / sr;
                    b[awal + i] += (Mathf.Sin(Dsp.DuaPi * f * t) + 0.3f * Mathf.Sin(Dsp.DuaPi * f * 2.7f * t) * Dsp.ExpDecay(t, 0.15f)) * Dsp.ExpDecay(t, 0.8f) * 0.3f;
                    if (k == 0) b[awal + i] += Mathf.Sin(Dsp.DuaPi * 50f * t) * Dsp.ExpDecay(t, 1.5f) * 0.4f;
                }
            }
            var lp = Dsp.Biquad.LP(1400f, 0.7f, sr);
            for (int i = 0; i < b.Length; i++) b[i] = lp.Process(b[i]);
            Dsp.Schroeder(b, 0.8f, sr);
            return b;
        }

        // Stinger kejutan: dentum rendah + gesekan tinggi disonan (dipakai kejadian Keras).
        static float[] Stinger(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(2.5f, sr);
            var hp = Dsp.Biquad.HP(3000f, 0.7f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                float dentum = Mathf.Sin(Dsp.DuaPi * Dsp.ExpSweep(90f, 35f, t, 0.5f) * t) * Dsp.ExpDecay(t, 0.6f);
                float gesek = (Mathf.Sin(Dsp.DuaPi * 1870f * t) + Mathf.Sin(Dsp.DuaPi * 1990f * t)) * 0.2f * Dsp.AD(t, 0.02f, 1.6f) + hp.Process(rng.White()) * 0.15f * Dsp.AD(t, 0.01f, 0.8f);
                b[i] = dentum + gesek;
            }
            return b;
        }

        // W01: pesawat kecil melintas tinggi - dengung baling-baling dengan efek Doppler & naik-turun keras.
        static float[] Pesawat(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(14f, sr);
            float fase = 0f;
            var lp = Dsp.Biquad.LP(900f, 0.7f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr, u = t / 14f;
                float doppler = Mathf.Lerp(1.08f, 0.92f, Mathf.SmoothStep(0f, 1f, u));
                fase += Dsp.DuaPi * 92f * doppler / sr;
                float x = Dsp.Saw(fase) * 0.5f + Mathf.Sin(fase * 2f) * 0.3f + rng.White() * 0.15f;
                b[i] = lp.Process(x) * Mathf.Sin(Mathf.PI * u) * (0.8f + 0.2f * Mathf.Sin(t * 23f));
            }
            return b;
        }
    }
}
