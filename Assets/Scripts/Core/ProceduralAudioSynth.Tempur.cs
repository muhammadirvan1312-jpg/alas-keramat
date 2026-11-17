using UnityEngine;

namespace AlasKeramat.Core
{
    // T7a (06 §1, 21 §6): suara tempur prosedural - denting parry, benturan daging, geraman & teriak Sekte, rintih, larut jadi abu.
    // Cadangan prosedural; rekaman CC0 menggantikan saat tersedia (21 §2).
    public partial class ProceduralAudioSynth
    {
        public const string KlipParry = "tempur_parry", KlipKenaDaging = "tempur_kena_daging", KlipGeramSekte = "sekte_geram", KlipTeriakSekte = "sekte_teriak",
            KlipRintihSekte = "sekte_rintih", KlipAbu = "tempur_abu", KlipAyunSekte = "sekte_ayun", KlipMusikTempur = "musik_tempur", KlipTebasBasah = "gore_tebas_basah", KlipPutusTulang = "gore_putus_tulang", KlipTubuhJatuh = "gore_tubuh_jatuh", KlipTembak = "senjata_tembak", KlipLepasPanah = "senjata_panah";
        public static readonly string[] KlipTempur = { KlipParry, KlipKenaDaging, KlipGeramSekte, KlipTeriakSekte, KlipRintihSekte, KlipAbu, KlipAyunSekte, KlipMusikTempur, KlipTebasBasah, KlipPutusTulang, KlipTubuhJatuh, KlipTembak, KlipLepasPanah };

        public static float[] GenerateTempur(string nama, int variasi, out int sr)
        {
            sr = Dsp.SrSfx;
            var rng = Dsp.Rng.DariNama(nama, variasi);
            float[] b;
            switch (nama)
            {
                case KlipParry: b = ResepParry(sr, ref rng); break;
                case KlipKenaDaging: b = ResepKenaDaging(sr, ref rng); break;
                case KlipGeramSekte: b = ResepSuaraSekte(sr, ref rng, 0.9f, 95f, 0.55f); break;
                case KlipTeriakSekte: b = ResepSuaraSekte(sr, ref rng, 1.1f, 170f, 0.9f); break;
                case KlipRintihSekte: b = ResepRintih(sr, ref rng); break;
                case KlipAbu: b = ResepAbu(sr, ref rng); break;
                case KlipAyunSekte: b = ResepAyun(sr, ref rng); break;
                case KlipMusikTempur: b = ResepMusikTempur(sr, ref rng); break;
                case KlipTebasBasah: b = ResepTebasBasah(sr, ref rng); break;
                case KlipPutusTulang: b = ResepPutusTulang(sr, ref rng); break;
                case KlipTubuhJatuh: b = ResepTubuhJatuh(sr, ref rng); break;
                case KlipTembak: b = ResepTembak(sr, ref rng); break;
                case KlipLepasPanah: b = ResepLepasPanah(sr, ref rng); break;
                default: return GenerateTeror(nama, variasi, out sr);
            }
            Dsp.Finalize(b, sr);
            return b;
        }

        // Logam beradu: parsial inharmonis meluruh cepat + transien noise terang.
        static float[] ResepParry(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(0.9f, sr);
            float f0 = rng.Range(820f, 980f);
            float[] rasio = { 1f, 2.76f, 5.4f, 8.93f };
            float[] tau = { 0.35f, 0.22f, 0.12f, 0.06f };
            var hp = Dsp.Biquad.HP(3000f, 0.7f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr, s = 0f;
                for (int k = 0; k < rasio.Length; k++) s += Mathf.Sin(Dsp.DuaPi * f0 * rasio[k] * t + k) * Dsp.ExpDecay(t, tau[k]) / (k + 1);
                s += hp.Process(rng.White()) * Dsp.ExpDecay(t, 0.012f) * 0.8f;
                b[i] = s * 0.8f;
            }
            Dsp.Schroeder(b, 0.12f, sr);
            return b;
        }

        // Benturan tumpul ke tubuh: thump rendah + remah basah pendek.
        static float[] ResepKenaDaging(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(0.35f, sr);
            var lp = Dsp.Biquad.LP(380f, 0.8f, sr);
            var bp = Dsp.Biquad.BP(1400f, 1.2f, sr);
            float f = rng.Range(70f, 95f);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                float thump = Mathf.Sin(Dsp.DuaPi * Dsp.ExpSweep(f * 1.8f, f, t, 0.08f) * t) * Dsp.ExpDecay(t, 0.07f);
                float basah = bp.Process(rng.White()) * Dsp.AD(t, 0.003f, 0.05f);
                b[i] = lp.Process(thump * 1.2f) + basah * 0.5f;
            }
            return b;
        }

        // Suara manusia kasar: gergaji bergetar (jitter) lewat dua formant vokal "a/o", sedikit distorsi.
        static float[] ResepSuaraSekte(int sr, ref Dsp.Rng rng, float lama, float f0, float keras)
        {
            var b = Dsp.Buffer(lama, sr);
            float fase = 0f, jitter = 0f;
            var lp = Dsp.Biquad.LP(2600f, 0.7f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                jitter = jitter * 0.995f + rng.White() * 0.005f;
                float f = f0 * (1f + 0.08f * Mathf.Sin(t * 5.5f) + jitter * 4f) * (1f - 0.15f * t / lama);
                fase += Dsp.DuaPi * f / sr;
                float x = Dsp.Saw(fase) + rng.White() * 0.25f * keras;
                float env = Dsp.ADSR(t, 0.06f, 0.15f, 0.7f, lama - 0.35f, 0.2f);
                b[i] = lp.Process(Dsp.SoftClip(x * (1.5f + keras * 2f))) * env;
            }
            Dsp.Formant(b, 650f, 1080f, sr);
            return b;
        }

        static float[] ResepRintih(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(1.6f, sr);
            float fase = 0f;
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                float f = Mathf.Lerp(210f, 150f, t / 1.6f) * (1f + 0.03f * Mathf.Sin(t * 7f));
                fase += Dsp.DuaPi * f / sr;
                float napas = rng.White() * 0.3f;
                b[i] = (Dsp.Triangle(fase) * 0.6f + napas) * Dsp.ADSR(t, 0.2f, 0.3f, 0.6f, 0.7f, 0.4f) * 0.6f;
            }
            Dsp.Formant(b, 450f, 900f, sr);
            return b;
        }

        // Tubuh larut jadi abu (Gore Mati): desis pasir panjang + gemuruh rendah memudar.
        static float[] ResepAbu(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(4f, sr);
            var bp = Dsp.Biquad.BP(5200f, 0.5f, sr);
            var lp = Dsp.Biquad.LP(120f, 0.7f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                float env = Dsp.AD(t, 0.15f, 1.6f);
                b[i] = (bp.Process(rng.White()) * 0.7f * (0.6f + 0.4f * Mathf.Sin(t * 23f)) + lp.Process(rng.White()) * 1.4f) * env;
            }
            return b;
        }

        // Ayunan senjata Sekte: desir udara (noise band naik-turun).
        static float[] ResepAyun(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(0.4f, sr);
            var bpRendah = Dsp.Biquad.BP(700f, 1.2f, sr);
            var bpTinggi = Dsp.Biquad.BP(2100f, 1.2f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr, u = Mathf.Sin(Mathf.PI * t / 0.4f);
                float w = rng.White();
                b[i] = (bpRendah.Process(w) * (1f - u) + bpTinggi.Process(w) * u) * u * 0.9f;
            }
            return b;
        }

        public static float VolumeKategori(AudioCategory k) { return VolMaster * _volKategori[(int)k]; }

        // Musik tempur dasar (10 §musik, T7): loop 8 s 120 bpm - kendang dung/tak, gong tiap 16 ketuk, bonang pelog (dibuat seamless: ekor dililit ke awal).
        static readonly float[] Pelog = { 293.7f, 311.1f, 349.2f, 415.3f, 440f, 466.2f, 523.3f };
        static readonly int[] PolaBonang = { 0, 2, 3, 2, 4, 3, 2, 0, 1, 3, 4, 6, 4, 3, 1, 2 };
        static float[] ResepMusikTempur(int sr, ref Dsp.Rng rng)
        {
            const float bpm = 120f; const int ketuk = 16;
            float beat = 60f / bpm, lama = ketuk * beat;
            int n = Mathf.RoundToInt(lama * sr);
            var b = new float[n];
            var bpTak = Dsp.Biquad.BP(2200f, 1.5f, sr);
            for (int k = 0; k < ketuk * 2; k++) // seperdelapan
            {
                float t0 = k * beat * 0.5f;
                bool dung = k % 4 == 0 || k % 8 == 6, tak = k % 2 == 1;
                int awal = Mathf.RoundToInt(t0 * sr);
                for (int i = 0; i < sr * 2; i++)
                {
                    int idx = (awal + i) % n;
                    float t = (float)i / sr, x = 0f;
                    if (dung && t < 0.6f) x += Mathf.Sin(Dsp.DuaPi * Dsp.ExpSweep(130f, 62f, t, 0.12f) * t) * Dsp.ExpDecay(t, 0.16f) * 0.9f;
                    if (tak && t < 0.1f) x += bpTak.Process(rng.White()) * Dsp.ExpDecay(t, 0.025f) * 0.5f;
                    float f = Pelog[PolaBonang[(k / 2) % PolaBonang.Length]] * (k % 2 == 0 ? 1f : 2f);
                    if (k % 2 == 0 && t < 1.2f) x += (Mathf.Sin(Dsp.DuaPi * f * t) + 0.35f * Mathf.Sin(Dsp.DuaPi * f * 2.76f * t) * Dsp.ExpDecay(t, 0.1f)) * Dsp.ExpDecay(t, 0.45f) * 0.22f;
                    if (k == 0) x += (Mathf.Sin(Dsp.DuaPi * 55f * t) + 0.5f * Mathf.Sin(Dsp.DuaPi * 55f * 2.3f * t) * Dsp.ExpDecay(t, 0.8f)) * Dsp.ExpDecay(t, 1.8f) * 0.6f * (1f + 0.15f * Mathf.Sin(Dsp.DuaPi * 3.2f * t));
                    if (x == 0f && t > 1.2f) break;
                    b[idx] += x;
                }
            }
            return b;
        }

        // 14 §9 audio gore.
        static float[] ResepTebasBasah(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(0.3f, sr);
            var bp = Dsp.Biquad.BP(1200f, 1.1f, sr); var lp = Dsp.Biquad.LP(800f, 0.7f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                float debam = Mathf.Sin(Dsp.DuaPi * 110f * t) * Dsp.ExpDecay(t, 0.05f);
                float kres = bp.Process(rng.White()) * Dsp.AD(t, 0.002f, 0.04f);
                float basah = Dsp.AM(lp.Process(rng.White()), t, 30f, 0.8f) * Dsp.AD(t, 0.01f, 0.12f);
                b[i] = debam * 0.9f + kres * 0.7f + basah * 0.6f;
            }
            return b;
        }

        static float[] ResepPutusTulang(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(0.35f, sr);
            var bp = Dsp.Biquad.BP(1500f, 1.3f, sr);
            float tImp = 0f;
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                float imp = 0f;
                if (t < 0.08f && t >= tImp) { imp = rng.Range(0.6f, 1f); tImp = t + rng.Range(0.004f, 0.012f); }
                b[i] = imp + bp.Process(rng.White()) * Dsp.ExpDecay(t, 0.06f) * 0.6f + Mathf.Sin(Dsp.DuaPi * 70f * t) * Dsp.ExpDecay(t, 0.09f);
            }
            return b;
        }

        static float[] ResepTubuhJatuh(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(0.5f, sr);
            var lp = Dsp.Biquad.LP(500f, 0.7f, sr); var hp = Dsp.Biquad.HP(2500f, 0.7f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                b[i] = Mathf.Sin(Dsp.DuaPi * Dsp.ExpSweep(90f, 55f, t, 0.2f) * t) * Dsp.ExpDecay(t, 0.12f) + lp.Process(rng.White()) * Dsp.ExpDecay(t, 0.08f) * 0.5f + hp.Process(rng.White()) * Dsp.AD(t, 0.03f, 0.1f) * 0.25f;
            }
            return b;
        }

        // Letusan mesiu: transien noise lebar + dentum rendah + gema (reverb pendek).
        static float[] ResepTembak(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(1.2f, sr);
            var lp = Dsp.Biquad.LP(3500f, 0.7f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                b[i] = lp.Process(rng.White()) * Dsp.ExpDecay(t, 0.03f) * 1.2f + Mathf.Sin(Dsp.DuaPi * Dsp.ExpSweep(160f, 45f, t, 0.15f) * t) * Dsp.ExpDecay(t, 0.12f);
            }
            Dsp.Schroeder(b, 0.35f, sr);
            return b;
        }

        // Tali busur: petikan Karplus-Strong rendah + desir anak panah.
        static float[] ResepLepasPanah(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(0.5f, sr);
            Dsp.KarplusStrong(b, 0, Mathf.RoundToInt(0.35f * sr), 110f, sr, ref rng, 0.8f);
            var bp = Dsp.Biquad.BP(2600f, 1f, sr);
            for (int i = 0; i < b.Length; i++) { float t = (float)i / sr; b[i] += bp.Process(rng.White()) * Dsp.AD(t, 0.02f, 0.15f) * 0.4f; }
            return b;
        }
    }
}
