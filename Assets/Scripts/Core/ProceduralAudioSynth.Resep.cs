using UnityEngine;

namespace AlasKeramat.Core
{
    // Resep efek suara T1 (docs/gdd/10 bagian 3).
    public partial class ProceduralAudioSynth
    {
        // Tebas kayu: nada 140 -> 55 Hz, amplop exp(-t/0,05) + noise BP 1.800 Hz 20 ms + klik 2 ms.
        static float[] ResepTebang(int sr, ref Dsp.Rng rng)
        {
            const float T = 0.18f;
            var b = Dsp.Buffer(T, sr);
            var bp = Dsp.Biquad.BP(1800f, 1.2f, sr);
            float fase = 0f;
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                fase += Dsp.DuaPi * Dsp.ExpSweep(140f, 55f, t, T) / sr;
                float s = Dsp.Sine(fase) * Dsp.ExpDecay(t, 0.05f);
                float n = bp.Process(rng.White());
                if (t < 0.02f) s += n * 1.6f * (1f - t / 0.02f);
                if (t < 0.002f) s += rng.White() * 0.8f;
                b[i] = s;
            }
            return b;
        }

        // Pohon tumbang: 6 retakan, hantaman 45 Hz + pink LP 300 Hz, desir daun BP 4 kHz.
        static float[] ResepPohonTumbang(int sr, ref Dsp.Rng rng)
        {
            const float T = 3.6f, Hantam = 1.6f;
            var b = Dsp.Buffer(T, sr);
            var hp = Dsp.Biquad.HP(1200f, 0.8f, sr);
            var lp = Dsp.Biquad.LP(300f, 0.7f, sr);
            var daun = Dsp.Biquad.BP(4000f, 1f, sr);
            var pink = new Dsp.Pink();
            float[] gainRetak = new float[6];
            for (int k = 0; k < 6; k++) gainRetak[k] = rng.Range(0.5f, 1f);
            float fase = 0f, amAcak = 0.5f;
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr, s = 0f;
                float w = rng.White();
                float retak = hp.Process(w);
                for (int k = 0; k < 6; k++) s += retak * gainRetak[k] * Dsp.ExpDecay(t - k * 0.25f, 0.012f);
                float th = t - Hantam;
                if (th >= 0f && th < 1.8f)
                {
                    fase += Dsp.DuaPi * 45f / sr;
                    float env = Dsp.AD(th, 0.03f, 0.6f);
                    s += (Dsp.Sine(fase) * 0.9f + lp.Process(pink.Next(w)) * 2.2f) * env;
                }
                float td = t - (Hantam + 0.1f);
                if (td >= 0f && td < 1.2f)
                {
                    if (i % 2205 == 0) amAcak = rng.Range(0.3f, 1f);
                    s += daun.Process(w) * 0.5f * amAcak * (1f - td / 1.2f);
                }
                b[i] = s;
            }
            return b;
        }

        // Kentongan bambu "Tok-Tok... Tok-Tok": 380 Hz + harmonik 760 Hz, klik kayu 5 ms.
        static float[] ResepKentongan(int sr, ref Dsp.Rng rng)
        {
            float[] pukul = { 0f, 0.16f, 0.90f, 1.06f };
            float dasar = rng.Range(340f, 420f);
            var b = Dsp.Buffer(1.5f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr, s = 0f;
                for (int k = 0; k < pukul.Length; k++)
                {
                    float tk = t - pukul[k];
                    if (tk < 0f || tk > 0.25f) continue;
                    float env = Dsp.ExpDecay(tk, 0.055f);
                    s += (Mathf.Sin(Dsp.DuaPi * dasar * tk) + 0.5f * Mathf.Sin(Dsp.DuaPi * dasar * 2f * tk)) * env;
                    if (tk < 0.005f) s += rng.White() * 0.6f * (1f - tk / 0.005f);
                }
                b[i] = s;
            }
            return b;
        }

        // Tawa Kuntilanak: pembawa 620 -> 520 Hz, vibrato 6 Hz +-120 Hz, AM "hi-hi" 6 Hz, gema 0,15 s, napas BP 2,5 kHz.
        static float[] ResepKuntilanak(int sr, ref Dsp.Rng rng)
        {
            const float T = 2.5f;
            var b = Dsp.Buffer(T, sr);
            var napas = Dsp.Biquad.BP(2500f, 1.5f, sr);
            float fase = 0f;
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                float fc = Mathf.Lerp(620f, 520f, t / T) + 120f * Mathf.Sin(Dsp.DuaPi * 6f * t);
                fase += Dsp.DuaPi * fc / sr;
                float hihi = Mathf.Pow(0.5f + 0.5f * Mathf.Sin(Dsp.DuaPi * 6f * t), 2f);
                float s = (Dsp.Sine(fase) * 0.8f + Dsp.Saw(fase) * 0.2f) * hihi;
                s += napas.Process(rng.White()) * 0.2f;
                b[i] = s * Dsp.ADSR(t, 0.08f, 0.2f, 0.85f, T - 0.4f, 0.4f);
            }
            Dsp.Delay(b, 0.15f, 0.35f, 0.4f, sr);
            return b;
        }

        // Pocong melompat: dentum 48 Hz exp(-t/0,08) + desis BP 2-4 kHz AM 9 Hz.
        static float[] ResepPocong(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(0.4f, sr);
            var desis = Dsp.Biquad.BP(3000f, 1.1f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                float s = Mathf.Sin(Dsp.DuaPi * 48f * t) * Dsp.ExpDecay(t, 0.08f) * (t < 0.25f ? 1f : 0.3f);
                if (t < 0.3f) s += Dsp.AM(desis.Process(rng.White()), t, 9f, 0.9f) * 0.6f * (1f - t / 0.3f);
                b[i] = s;
            }
            return b;
        }

        // Klik UI: sinus 1.200 Hz 30 ms.
        static float[] ResepKlik(int sr)
        {
            var b = Dsp.Buffer(0.03f, sr);
            for (int i = 0; i < b.Length; i++) { float t = (float)i / sr; b[i] = Mathf.Sin(Dsp.DuaPi * 1200f * t) * Dsp.ExpDecay(t, 0.012f); }
            return b;
        }

        // Ambil item: segitiga 660 lalu 880 Hz, masing-masing 80 ms.
        static float[] ResepAmbilItem(int sr)
        {
            var b = Dsp.Buffer(0.16f, sr);
            float fase = 0f;
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                float f = t < 0.08f ? 660f : 880f;
                float tl = t < 0.08f ? t : t - 0.08f;
                fase += Dsp.DuaPi * f / sr;
                b[i] = Dsp.Triangle(fase) * Dsp.AD(tl, 0.004f, 0.04f);
            }
            return b;
        }

        // Ambil kayu: debam 90 Hz tau 0,05 + gesek noise BP 1 kHz.
        static float[] ResepAmbilKayu(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(0.15f, sr);
            var bp = Dsp.Biquad.BP(1000f, 1f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                b[i] = Mathf.Sin(Dsp.DuaPi * 90f * t) * Dsp.ExpDecay(t, 0.05f) + bp.Process(rng.White()) * 0.5f * (1f - t / 0.15f);
            }
            return b;
        }
    }
}
