using System;
using System.IO;
using UnityEngine;

namespace AlasKeramat.Core
{
    // Blok DSP statis (docs/gdd/10 bagian 2). Semua bekerja pada float[] mono.
    public static class Dsp
    {
        public const float DuaPi = Mathf.PI * 2f;
        public const int SrSfx = 44100;
        public const int SrAmbien = 22050;
        public const float PuncakNormal = 0.89f;
        public const float FadeDetik = 0.005f;

        // ---------- Osilator (fase dalam radian) ----------
        public static float Sine(float fase) { return Mathf.Sin(fase); }
        public static float Triangle(float fase) { float p = Frac(fase / DuaPi); return 4f * Mathf.Abs(p - 0.5f) - 1f; }
        public static float Saw(float fase) { return 2f * Frac(fase / DuaPi) - 1f; }
        public static float Square(float fase) { return Frac(fase / DuaPi) < 0.5f ? 1f : -1f; }
        static float Frac(float x) { return x - Mathf.Floor(x); }

        // Sapuan frekuensi eksponensial f(t) = f0 * (f1/f0)^(t/T)
        public static float ExpSweep(float f0, float f1, float t, float T) { return f0 * Mathf.Pow(f1 / f0, Mathf.Clamp01(t / T)); }

        // ---------- Noise ----------
        public struct Rng
        {
            uint _s;
            public Rng(uint seed) { _s = seed == 0 ? 2463534242u : seed; }
            public static Rng DariNama(string nama, int variasi)
            {
                uint h = 2166136261u;
                for (int i = 0; i < nama.Length; i++) { h ^= nama[i]; h *= 16777619u; }
                return new Rng(h ^ (uint)(variasi * 0x9E3779B9));
            }
            public uint Next() { _s ^= _s << 13; _s ^= _s >> 17; _s ^= _s << 5; return _s; }
            public float White() { return (Next() & 0xFFFFFF) / 8388607.5f - 1f; }
            public float Range(float a, float b) { return a + (b - a) * ((Next() & 0xFFFFFF) / 16777215f); }
        }

        // Noise pink (filter Paul Kellet).
        public struct Pink
        {
            float b0, b1, b2, b3, b4, b5, b6;
            public float Next(float w)
            {
                b0 = 0.99886f * b0 + w * 0.0555179f; b1 = 0.99332f * b1 + w * 0.0750759f;
                b2 = 0.96900f * b2 + w * 0.1538520f; b3 = 0.86650f * b3 + w * 0.3104856f;
                b4 = 0.55000f * b4 + w * 0.5329522f; b5 = -0.7616f * b5 - w * 0.0168980f;
                float o = b0 + b1 + b2 + b3 + b4 + b5 + b6 + w * 0.5362f;
                b6 = w * 0.115926f;
                return o * 0.11f;
            }
        }

        // Noise coklat b = 0,98b + 0,02w (dinormalisasi kira-kira).
        public struct Brown
        {
            float b;
            public float Next(float w) { b = 0.98f * b + 0.02f * w; return b * 3.5f; }
        }

        // ---------- Filter ----------
        public struct OnePole
        {
            float a, z; bool hp;
            public static OnePole LP(float fc, int sr) { return new OnePole { a = Mathf.Exp(-DuaPi * fc / sr) }; }
            public static OnePole HP(float fc, int sr) { return new OnePole { a = Mathf.Exp(-DuaPi * fc / sr), hp = true }; }
            public float Process(float x) { z = (1f - a) * x + a * z; return hp ? x - z : z; }
        }

        // Biquad RBJ Audio EQ Cookbook.
        public struct Biquad
        {
            float b0, b1, b2, a1, a2, x1, x2, y1, y2;

            static Biquad Buat(float b0, float b1, float b2, float a0, float a1, float a2)
            {
                return new Biquad { b0 = b0 / a0, b1 = b1 / a0, b2 = b2 / a0, a1 = a1 / a0, a2 = a2 / a0 };
            }
            static void Dasar(float f, float q, int sr, out float w0, out float alpha)
            {
                w0 = DuaPi * Mathf.Clamp(f, 10f, sr * 0.45f) / sr;
                alpha = Mathf.Sin(w0) / (2f * Mathf.Max(q, 0.05f));
            }
            public static Biquad LP(float f, float q, int sr) { float w, a; Dasar(f, q, sr, out w, out a); float c = Mathf.Cos(w); return Buat((1 - c) / 2, 1 - c, (1 - c) / 2, 1 + a, -2 * c, 1 - a); }
            public static Biquad HP(float f, float q, int sr) { float w, a; Dasar(f, q, sr, out w, out a); float c = Mathf.Cos(w); return Buat((1 + c) / 2, -(1 + c), (1 + c) / 2, 1 + a, -2 * c, 1 - a); }
            public static Biquad BP(float f, float q, int sr) { float w, a; Dasar(f, q, sr, out w, out a); float c = Mathf.Cos(w); return Buat(a, 0, -a, 1 + a, -2 * c, 1 - a); }
            public static Biquad Notch(float f, float q, int sr) { float w, a; Dasar(f, q, sr, out w, out a); float c = Mathf.Cos(w); return Buat(1, -2 * c, 1, 1 + a, -2 * c, 1 - a); }
            public static Biquad Peak(float f, float q, float gainDb, int sr)
            {
                float w, a; Dasar(f, q, sr, out w, out a); float c = Mathf.Cos(w); float A = Mathf.Pow(10f, gainDb / 40f);
                return Buat(1 + a * A, -2 * c, 1 - a * A, 1 + a / A, -2 * c, 1 - a / A);
            }
            public float Process(float x)
            {
                float y = b0 * x + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;
                x2 = x1; x1 = x; y2 = y1; y1 = y;
                return y;
            }
        }

        // ---------- Amplop ----------
        public static float ExpDecay(float t, float tau) { return t < 0f ? 0f : Mathf.Exp(-t / tau); }
        public static float AD(float t, float serang, float tau) { return t < 0f ? 0f : t < serang ? t / serang : Mathf.Exp(-(t - serang) / tau); }
        public static float ADSR(float t, float a, float d, float s, float lamaTahan, float r)
        {
            if (t < 0f) return 0f;
            if (t < a) return t / a;
            if (t < a + d) return Mathf.Lerp(1f, s, (t - a) / d);
            if (t < lamaTahan) return s;
            return Mathf.Max(0f, s * (1f - (t - lamaTahan) / r));
        }

        // ---------- Modulasi ----------
        public static float FM(float t, float fc, float fm, float indeks) { return Mathf.Sin(DuaPi * fc * t + indeks * Mathf.Sin(DuaPi * fm * t)); }
        public static float AM(float x, float t, float fa, float d) { return x * (1f - d + d * (0.5f + 0.5f * Mathf.Sin(DuaPi * fa * t))); }

        // Karplus-Strong (petikan).
        public static void KarplusStrong(float[] buf, int start, int len, float f, int sr, ref Rng rng, float gain)
        {
            int n = Mathf.Max(2, Mathf.RoundToInt(sr / f));
            var garis = new float[n];
            for (int i = 0; i < n; i++) garis[i] = rng.White();
            int idx = 0;
            for (int i = 0; i < len && start + i < buf.Length; i++)
            {
                int berikut = (idx + 1) % n;
                float v = garis[idx];
                garis[idx] = 0.5f * (garis[idx] + garis[berikut]) * 0.996f;
                buf[start + i] += v * gain;
                idx = berikut;
            }
        }

        // Gema delay dengan feedback (in-place).
        public static void Delay(float[] buf, float detik, float feedback, float mix, int sr)
        {
            int d = Mathf.Max(1, Mathf.RoundToInt(detik * sr));
            var garis = new float[d];
            int i0 = 0;
            for (int i = 0; i < buf.Length; i++)
            {
                float tunda = garis[i0];
                garis[i0] = buf[i] + tunda * feedback;
                buf[i] += tunda * mix;
                i0 = (i0 + 1) % d;
            }
        }

        // Reverb Schroeder ringan: 4 comb + 2 allpass (varian "jauh").
        public static void Schroeder(float[] buf, float mix, int sr)
        {
            float skala = sr / 44100f;
            int[] comb = { 1116, 1188, 1277, 1356 };
            int[] ap = { 556, 441 };
            var basah = new float[buf.Length];
            for (int c = 0; c < comb.Length; c++)
            {
                int n = Mathf.Max(1, (int)(comb[c] * skala)); var g = new float[n]; int k = 0;
                for (int i = 0; i < buf.Length; i++) { float y = g[k]; g[k] = buf[i] + y * 0.78f; basah[i] += y * 0.25f; k = (k + 1) % n; }
            }
            for (int a = 0; a < ap.Length; a++)
            {
                int n = Mathf.Max(1, (int)(ap[a] * skala)); var g = new float[n]; int k = 0;
                for (int i = 0; i < basah.Length; i++) { float b = g[k]; float y = -basah[i] + b; g[k] = basah[i] + b * 0.5f; basah[i] = y; k = (k + 1) % n; }
            }
            for (int i = 0; i < buf.Length; i++) buf[i] = buf[i] * (1f - mix) + basah[i] * mix;
        }

        // Formant: 2 BP paralel pada sumber (gigi gergaji).
        public static void Formant(float[] buf, float f1, float f2, int sr)
        {
            var b1 = Biquad.BP(f1, 5f, sr); var b2 = Biquad.BP(f2, 6f, sr);
            for (int i = 0; i < buf.Length; i++) buf[i] = b1.Process(buf[i]) + 0.7f * b2.Process(buf[i]);
        }

        public static float SoftClip(float x) { return x / (1f + Mathf.Abs(x)); }

        // Akhiran: fade 5 ms, normalisasi puncak 0,89.
        public static void Finalize(float[] buf, int sr)
        {
            int f = Mathf.Min(buf.Length / 2, Mathf.RoundToInt(FadeDetik * sr));
            for (int i = 0; i < f; i++) { float g = (float)i / f; buf[i] *= g; buf[buf.Length - 1 - i] *= g; }
            float puncak = 0f;
            for (int i = 0; i < buf.Length; i++) puncak = Mathf.Max(puncak, Mathf.Abs(buf[i]));
            if (puncak > 1e-6f) { float k = PuncakNormal / puncak; for (int i = 0; i < buf.Length; i++) buf[i] *= k; }
        }

        // Loop mulus: crossfade 0,25 s ekor ke awal.
        public static float[] LoopCrossfade(float[] buf, int sr)
        {
            int x = Mathf.Min(buf.Length / 3, Mathf.RoundToInt(0.25f * sr));
            var hasil = new float[buf.Length - x];
            Array.Copy(buf, hasil, hasil.Length);
            for (int i = 0; i < x; i++) { float t = (float)i / x; hasil[i] = buf[i] * t + buf[hasil.Length + i] * (1f - t); }
            return hasil;
        }

        public static float[] Buffer(float detik, int sr) { return new float[Mathf.Max(1, Mathf.RoundToInt(detik * sr))]; }
    }

    // Penulis WAV RIFF 16-bit mono.
    public static class WavWriter
    {
        public static byte[] Encode(float[] s, int sr)
        {
            using (var ms = new MemoryStream(44 + s.Length * 2))
            using (var w = new BinaryWriter(ms))
            {
                int data = s.Length * 2;
                w.Write(new[] { 'R', 'I', 'F', 'F' }); w.Write(36 + data); w.Write(new[] { 'W', 'A', 'V', 'E' });
                w.Write(new[] { 'f', 'm', 't', ' ' }); w.Write(16); w.Write((short)1); w.Write((short)1);
                w.Write(sr); w.Write(sr * 2); w.Write((short)2); w.Write((short)16);
                w.Write(new[] { 'd', 'a', 't', 'a' }); w.Write(data);
                for (int i = 0; i < s.Length; i++) w.Write((short)(Mathf.Clamp(s[i], -1f, 1f) * 32767f));
                w.Flush();
                return ms.ToArray();
            }
        }
    }
}
