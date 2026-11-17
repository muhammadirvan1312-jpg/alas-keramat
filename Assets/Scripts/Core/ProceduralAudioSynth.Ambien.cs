using UnityEngine;

namespace AlasKeramat.Core
{
    public partial class ProceduralAudioSynth
    {
        public const string BedPagi = "bed_hutan_pagi", BedSiang = "bed_hutan_siang",
            BedSenja = "bed_hutan_senja", BedMalam = "bed_hutan_malam",
            BedDini = "bed_dinihari", BedPantai = "bed_pantai",
            BedRawa = "bed_rawa_malam", BedWingit = "bed_wingit",
            BedPabrik = "bed_pabrik_angin", BedTebing = "bed_angin_tebing";
        public const string OsJangkrik = "os_jangkrik", OsKatak = "os_katak",
            OsTokek = "os_tokek", OsHantu = "os_burung_hantu",
            OsTonggeret = "os_tonggeret", OsPagi = "os_burung_pagi",
            OsAyam = "os_ayam_hutan", OsKedasih = "os_kedasih",
            OsBambu = "os_bambu_derit", OsKepak = "os_kepak_burung",
            OsRantai = "os_rantai", OsRadio = "os_radio_statis",
            OsGamelan = "os_gamelan_samar";
        public static readonly string[] KlipAmbien =
        {
            BedPagi, BedSiang, BedSenja, BedMalam, BedDini, BedPantai,
            BedRawa, BedWingit, BedPabrik, BedTebing,
            OsJangkrik, OsKatak, OsTokek, OsHantu, OsTonggeret, OsPagi,
            OsAyam, OsKedasih, OsBambu, OsKepak, OsRantai, OsRadio, OsGamelan
        };

        private const float BedSeconds = 24.25f, ShotSeconds = 4f;
        private const float Tau = Dsp.DuaPi, AirQ = 0.707f;
        private const float EndFade = 0.06f;
        // Resep dipanggang di Editor. Tidak ada buffer baru di loop sampel.
        public static float[] GenerateAmbien(string nama, int variasi, out int sr)
        {
            sr = Dsp.SrSfx;
            if (nama == null) return null;
            var r = Dsp.Rng.DariNama(nama, variasi);
            float[] b;
            bool loop = true;
            float level = 0.55f;
            switch (nama)
            {
                case BedPagi: b = AmbPagi(sr, ref r); break;
                case BedSiang: b = AmbSiang(sr, ref r); break;
                case BedSenja: b = AmbSenja(sr, ref r); break;
                case BedMalam: b = AmbMalam(sr, ref r); level = 0.38f; break;
                case BedDini: b = AmbDini(sr, ref r); level = 0.09f; break;
                case BedPantai: b = AmbPantai(sr, ref r); break;
                case BedRawa: b = AmbRawa(sr, ref r); level = 0.4f; break;
                case BedWingit: b = AmbWingit(sr, ref r); level = 0.32f; break;
                case BedPabrik: b = AmbPabrik(sr, ref r); break;
                case BedTebing: b = AmbTebing(sr, ref r); break;
                case OsJangkrik: b = AmbJangkrik(sr, ref r); loop = false; break;
                case OsKatak: b = AmbKatak(sr, ref r); loop = false; break;
                case OsTokek: b = AmbTokek(sr, ref r); loop = false; break;
                case OsHantu: b = AmbHantu(sr, ref r); loop = false; break;
                case OsTonggeret: b = AmbTonggeret(sr, ref r); loop = false; break;
                case OsPagi: b = AmbBurung(sr, ref r); loop = false; break;
                case OsAyam: b = AmbAyam(sr, ref r); loop = false; break;
                case OsKedasih: b = AmbKedasih(sr, ref r); loop = false; break;
                case OsBambu: b = AmbBambu(sr, ref r); loop = false; break;
                case OsKepak: b = AmbKepak(sr, ref r); loop = false; break;
                case OsRantai: b = AmbRantai(sr, ref r); loop = false; break;
                case OsRadio: b = AmbRadio(sr, ref r); loop = false; break;
                case OsGamelan: b = AmbGamelan(sr, ref r); loop = false; level = 0.2f; break;
                default: return null;
            }
            // DC dan normalisasi sebelum overlap: fade Finalize tidak memotong sambungan.
            var dc = Dsp.OnePole.HP(18f, sr);
            for (int i = 0; i < b.Length; i++) b[i] = dc.Process(b[i]);
            Dsp.Finalize(b, sr);
            if (loop) b = Dsp.LoopCrossfade(b, sr);
            for (int i = 0; i < b.Length; i++) b[i] *= level;
            return b;
        }

        // Kereta 3-4 pulsa per chirp, 20-30 pulsa/detik, tiga individu.
        private static float[] AmbJangkrik(int sr, ref Dsp.Rng r)
        {
            const int Individuals = 3;
            const float CarrierMin = 4100f, CarrierMax = 4900f;
            var b = Dsp.Buffer(ShotSeconds, sr);
            for (int v = 0; v < Individuals; v++)
            {
                float hz = r.Range(CarrierMin, CarrierMax), rate = r.Range(20f, 30f);
                float gap = r.Range(0.38f, 0.7f), offset = r.Range(0f, gap);
                int pulses = (int)(r.Next() % 2u) + 3;
                float phase = r.Range(0f, Tau);
                var band = Dsp.Biquad.BP(hz, 7f, sr);
                for (int i = 0; i < b.Length; i++)
                {
                    float t = (float)i / sr, local = (t + offset) % gap;
                    float pulse = local * rate;
                    float env = pulse < pulses ? Mathf.Pow(Mathf.Sin(Mathf.PI * (pulse % 1f)), 4f) : 0f;
                    phase = Mathf.Repeat(phase + Tau * hz / sr, Tau);
                    float rasp = band.Process(r.White());
                    b[i] += (Mathf.Sin(phase) * 0.55f + rasp * 0.45f)
                        * env * (0.8f + 0.2f * Mathf.Sin(t * 9f + v)) / Individuals;
                }
            }
            return b;
        }

        // Pitch glotis menurun, formant tenggorokan, dan resonansi kantong suara.
        private static float[] AmbKatak(int sr, ref Dsp.Rng r)
        {
            const int Frogs = 3;
            var b = Dsp.Buffer(ShotSeconds, sr);
            var voice = new float[b.Length];
            for (int v = 0; v < Frogs; v++)
            {
                System.Array.Clear(voice, 0, voice.Length);
                float start = 0.12f + v * 0.83f + r.Range(0f, 0.15f);
                float baseHz = r.Range(105f, 175f), phase = 0f;
                var sac = Dsp.Biquad.BP(baseHz * 2f, 3f, sr);
                for (int i = 0; i < voice.Length; i++)
                {
                    float t = (float)i / sr - start;
                    if (t < 0f || t > 0.85f) continue;
                    float local = t % 0.28f;
                    phase = Mathf.Repeat(phase + Tau * Dsp.ExpSweep(baseHz * 1.8f, baseHz, local, 0.22f) / sr, Tau);
                    float throat = Dsp.Saw(phase) * 0.65f + r.White() * 0.2f;
                    voice[i] = (throat + sac.Process(throat)) * Dsp.AD(local, 0.018f, 0.075f);
                }
                Dsp.Formant(voice, 480f + v * 80f, 1250f + v * 130f, sr);
                AmbMix(b, voice, 0f, 0.65f, sr);
            }
            Dsp.Schroeder(b, 0.16f, sr);
            return b;
        }

        private static float[] AmbTokek(int sr, ref Dsp.Rng r)
        {
            const float BreathEnd = 0.32f;
            var b = Dsp.Buffer(ShotSeconds, sr);
            var rasp = Dsp.Biquad.BP(1500f, 1.2f, sr);
            for (int i = 0; i < (int)(BreathEnd * sr); i++)
                b[i] = rasp.Process(r.White()) * 0.22f * Mathf.Sin(Mathf.PI * i / (BreathEnd * sr));
            int calls = 5 + (int)(r.Next() % 4u);
            float start = BreathEnd, hz = r.Range(650f, 820f);
            for (int k = 0; k < calls; k++)
            {
                float gain = 1f - 0.65f * k / calls;
                AmbSyllable(b, start, 0.12f, hz * 0.75f, hz * 0.64f, gain, 0.8f, sr, ref r);
                AmbSyllable(b, start + 0.15f, 0.15f, hz * 1.7f, hz * 1.2f, gain, 2.4f, sr, ref r);
                start += 0.35f + 0.022f * k;
            }
            Dsp.Formant(b, 900f, 2400f, sr);
            return b;
        }

        private static float[] AmbHantu(int sr, ref Dsp.Rng r)
        {
            var b = Dsp.Buffer(ShotSeconds, sr);
            float hz = r.Range(350f, 450f);
            var breath = Dsp.Biquad.BP(600f, 0.8f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                float env = AmbWindow(t - 0.18f, 0.4f) + AmbWindow(t - 0.85f, 1.05f);
                float vibrato = 0.07f * Mathf.Sin(Tau * 4f * t);
                b[i] = (Mathf.Sin(Tau * hz * t + vibrato) + 0.12f * breath.Process(r.White())) * env;
            }
            AmbDistant(b, 1100f, 0.55f, sr);
            return b;
        }

        private static float[] AmbTonggeret(int sr, ref Dsp.Rng r)
        {
            var b = Dsp.Buffer(ShotSeconds, sr);
            float hz = r.Range(5100f, 6900f), tremolo = r.Range(42f, 65f);
            var band = Dsp.Biquad.BP(hz, 2.8f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                float rise = Mathf.Clamp01(t / 1.3f);
                float pulse = 0.65f + 0.35f * Mathf.Sin(Tau * 2.3f * t);
                b[i] = (band.Process(r.White()) + 0.12f * Dsp.FM(t, hz, 91f, 2.3f))
                    * rise * pulse * (0.6f + 0.4f * Mathf.Sin(Tau * tremolo * t));
            }
            return b;
        }

        private static float[] AmbBurung(int sr, ref Dsp.Rng r)
        {
            const int Calls = 9;
            var b = Dsp.Buffer(ShotSeconds, sr);
            for (int k = 0; k < Calls; k++)
            {
                float start = r.Range(0.05f, 3.2f), dur = r.Range(0.08f, 0.42f);
                float hz = r.Range(1700f, 3600f);
                int type = k % 3;
                int first = (int)(start * sr), count = (int)(dur * sr);
                float phase = r.Range(0f, Tau), distance = r.Range(0.25f, 0.9f);
                for (int j = 0; j < count && first + j < b.Length; j++)
                {
                    float t = (float)j / sr;
                    float f = type == 0 ? Dsp.ExpSweep(hz, hz * 1.9f, t, dur) :
                        type == 1 ? hz + 350f * Mathf.Sin(Tau * 32f * t) : hz;
                    phase = Mathf.Repeat(phase + Tau * f / sr, Tau);
                    b[first + j] += (Mathf.Sin(phase) + 0.14f * Mathf.Sin(phase * 2f)
                        + 0.04f * r.White()) * AmbWindow(t, dur) * distance;
                }
            }
            Dsp.Delay(b, 0.071f, 0.15f, 0.12f, sr);
            return b;
        }

        private static float[] AmbAyam(int sr, ref Dsp.Rng r)
        {
            var b = Dsp.Buffer(ShotSeconds, sr);
            float hz = r.Range(570f, 700f);
            AmbSyllable(b, 0.12f, 0.16f, hz, hz * 1.1f, 0.7f, 2.2f, sr, ref r);
            AmbSyllable(b, 0.34f, 0.18f, hz * 1.2f, hz, 0.9f, 2.5f, sr, ref r);
            AmbSyllable(b, 0.58f, 0.25f, hz * 1.5f, hz * 1.4f, 1f, 3f, sr, ref r);
            AmbSyllable(b, 0.91f, 0.67f, hz * 1.4f, hz * 0.7f, 0.85f, 3.5f, sr, ref r);
            Dsp.Formant(b, 850f, 2200f, sr);
            Dsp.Schroeder(b, 0.22f, sr);
            return b;
        }

        private static float[] AmbKedasih(int sr, ref Dsp.Rng r)
        {
            var b = Dsp.Buffer(ShotSeconds, sr);
            float hz = r.Range(1550f, 1850f);
            for (int k = 0; k < 5; k++)
            {
                float f = hz * Mathf.Pow(0.91f, k);
                AmbSyllable(b, 0.15f + k * 0.55f, 0.32f, f, f * 0.91f,
                    0.8f - 0.08f * k, 0.08f, sr, ref r);
            }
            AmbDistant(b, 2500f, 0.4f, sr);
            return b;
        }

        private static float[] AmbBambu(int sr, ref Dsp.Rng r)
        {
            var b = Dsp.Buffer(ShotSeconds, sr);
            var wood = Dsp.Biquad.BP(r.Range(330f, 520f), 8f, sr);
            var hollow = Dsp.Biquad.BP(980f, 5f, sr);
            float slip = 0f, threshold = r.Range(0.02f, 0.06f);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                float pressure = Mathf.Pow(Mathf.Sin(Mathf.PI * t / ShotSeconds), 2f);
                slip += (0.3f + pressure) / sr;
                float impulse = 0f;
                if (slip > threshold) { slip = 0f; threshold = r.Range(0.002f, 0.018f); impulse = r.Range(0.4f, 1f); }
                float friction = impulse + r.White() * 0.025f * pressure;
                b[i] = (wood.Process(friction) * 3f + hollow.Process(friction)) * pressure;
            }
            Dsp.KarplusStrong(b, (int)(1.1f * sr), (int)(0.4f * sr), 210f, sr, ref r, 0.08f);
            return b;
        }

        private static float[] AmbKepak(int sr, ref Dsp.Rng r)
        {
            var b = Dsp.Buffer(ShotSeconds, sr);
            for (int bird = 0; bird < 6; bird++)
            {
                var wing = Dsp.Biquad.BP(r.Range(650f, 1300f), 0.7f, sr);
                float offset = r.Range(0f, 0.35f), rate = r.Range(7f, 12f);
                for (int i = 0; i < b.Length; i++)
                {
                    float t = (float)i / sr - offset;
                    if (t < 0f) continue;
                    float pulse = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(Tau * rate * t)), 5f);
                    b[i] += wing.Process(r.White()) * pulse * Mathf.Exp(-t / 0.9f) * 0.35f;
                }
                AmbSyllable(b, offset, 0.24f, r.Range(2200f, 3400f), 1400f, 0.13f, 1.5f, sr, ref r);
            }
            Dsp.Schroeder(b, 0.2f, sr);
            return b;
        }

        private static float[] AmbRantai(int sr, ref Dsp.Rng r)
        {
            var b = Dsp.Buffer(ShotSeconds, sr);
            for (int k = 0; k < 15; k++)
                AmbMetal(b, r.Range(0.05f, 2.3f), r.Range(800f, 2300f),
                    r.Range(0.025f, 0.11f), r.Range(0.1f, 0.5f), sr, ref r);
            Dsp.Delay(b, 0.23f, 0.5f, 0.3f, sr);
            Dsp.Schroeder(b, 0.62f, sr);
            return b;
        }

        private static float[] AmbRadio(int sr, ref Dsp.Rng r)
        {
            var b = Dsp.Buffer(ShotSeconds, sr);
            var band = Dsp.Biquad.BP(1800f, 0.65f, sr);
            var hiss = Dsp.OnePole.HP(2500f, sr);
            float phase = r.Range(0f, Tau);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                float am = 0.25f + 0.75f * Mathf.Pow(Mathf.Sin(t * 3.7f + phase), 2f);
                b[i] = band.Process(r.White()) * am * 0.35f + hiss.Process(r.White()) * 0.025f;
            }
            // Potongan petikan sinkop; bukan sampel lagu atau vokal.
            float[] notes = { 330f, 392f, 440f, 392f, 294f };
            for (int k = 0; k < notes.Length; k++)
                Dsp.KarplusStrong(b, (int)((0.65f + k * 0.19f) * sr),
                    (int)(0.16f * sr), notes[k] * r.Range(0.98f, 1.02f), sr, ref r, 0.18f);
            var radio = Dsp.Biquad.BP(1300f, 0.65f, sr);
            for (int i = 0; i < b.Length; i++) b[i] = radio.Process(Dsp.SoftClip(b[i] * 3f));
            return b;
        }

        private static float[] AmbGamelan(int sr, ref Dsp.Rng r)
        {
            var b = Dsp.Buffer(ShotSeconds, sr);
            float[] pelog = { 196f, 207f, 233f, 294f, 311f };
            float t = 0.04f;
            for (int k = 0; k < 4; k++)
            {
                float hz = pelog[(int)(r.Next() % (uint)pelog.Length)];
                AmbMetal(b, t, hz, 0.65f, 0.45f, sr, ref r);
                AmbMetal(b, t, hz + 1.3f, 0.8f, 0.3f, sr, ref r);
                t += r.Range(0.35f, 0.8f);
            }
            Dsp.Delay(b, 0.31f, 0.72f, 0.45f, sr);
            AmbDistant(b, 680f, 0.78f, sr);
            return b;
        }

        // Lapisan bed disusun eksplisit; tidak ada pemilihan timbre generik.
        private static float[] AmbPagi(int sr, ref Dsp.Rng r)
        {
            var b = AmbWind(sr, ref r, 0.1f, 0.09f);
            var birds = AmbBurung(sr, ref r);
            AmbMix(b, birds, 1.2f, 0.24f, sr); AmbMix(b, birds, 12.7f, 0.13f, sr);
            var ayam = AmbAyam(sr, ref r); AmbDistant(ayam, 1600f, 0.5f, sr);
            AmbMix(b, ayam, 6.3f, 0.07f, sr);
            var kedasih = AmbKedasih(sr, ref r); AmbMix(b, kedasih, 18f, 0.06f, sr);
            return b;
        }
        private static float[] AmbSiang(int sr, ref Dsp.Rng r)
        {
            var b = AmbWind(sr, ref r, 0.12f, 0.14f);
            var cicada = AmbTonggeret(sr, ref r);
            AmbScatter(b, cicada, 1f, 4.7f, 0.22f, sr, ref r);
            var birds = AmbBurung(sr, ref r); AmbDistant(birds, 2300f, 0.3f, sr);
            AmbMix(b, birds, 15f, 0.045f, sr);
            return b;
        }
        private static float[] AmbSenja(int sr, ref Dsp.Rng r)
        {
            var b = AmbWind(sr, ref r, 0.11f, 0.12f);
            var cicada = AmbTonggeret(sr, ref r);
            AmbScatter(b, cicada, 0f, 3.3f, 0.45f, sr, ref r);
            var bamboo = AmbBambu(sr, ref r); AmbMix(b, bamboo, 11f, 0.08f, sr);
            return b;
        }
        private static float[] AmbMalam(int sr, ref Dsp.Rng r)
        {
            var b = AmbWind(sr, ref r, 0.055f, 0.035f);
            var insects = AmbJangkrik(sr, ref r);
            AmbScatter(b, insects, 0f, 3.8f, 0.15f, sr, ref r);
            var frogs = AmbKatak(sr, ref r); AmbDistant(frogs, 1600f, 0.4f, sr);
            AmbMix(b, frogs, 7f, 0.14f, sr);
            AmbMix(b, AmbHantu(sr, ref r), 17f, 0.065f, sr);
            return b;
        }
        private static float[] AmbDini(int sr, ref Dsp.Rng r)
        {
            var b = AmbWind(sr, ref r, 0.018f, 0.008f);
            AmbDrone(b, r.Range(40f, 60f), 0.018f, sr);
            // Retakan pendek jauh, kayu tereksitasi impuls acak.
            var twig = Dsp.Buffer(ShotSeconds, sr);
            var snap = Dsp.Biquad.BP(1100f, 1f, sr);
            for (int i = 0; i < twig.Length; i++)
                twig[i] = snap.Process(r.White()) * Dsp.AD((float)i / sr, 0.001f, 0.008f);
            AmbDistant(twig, 800f, 0.65f, sr);
            AmbMix(b, twig, r.Range(10f, 18f), 0.12f, sr);
            return b;
        }
        private static float[] AmbPantai(int sr, ref Dsp.Rng r)
        {
            var b = AmbWind(sr, ref r, 0.2f, 0.02f);
            var foam = Dsp.Biquad.BP(2100f, 0.6f, sr);
            var surf = Dsp.OnePole.LP(380f, sr);
            var pink = new Dsp.Pink();
            float offset = r.Range(0f, Tau);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                float wave = Mathf.Pow(0.5f + 0.5f * Mathf.Sin(t * 0.8f + offset), 3f);
                float w = r.White();
                b[i] += surf.Process(pink.Next(w)) * wave * 0.65f
                    + foam.Process(w) * wave * wave * 0.13f;
            }
            return b;
        }
        private static float[] AmbRawa(int sr, ref Dsp.Rng r)
        {
            var b = AmbWind(sr, ref r, 0.045f, 0.055f);
            var frogs = AmbKatak(sr, ref r);
            AmbScatter(b, frogs, 0.4f, 4.3f, 0.3f, sr, ref r);
            var insects = AmbJangkrik(sr, ref r);
            AmbScatter(b, insects, 2f, 4.1f, 0.065f, sr, ref r);
            AmbMix(b, AmbTokek(sr, ref r), 13f, 0.09f, sr);
            return b;
        }
        private static float[] AmbWingit(int sr, ref Dsp.Rng r)
        {
            var b = AmbWind(sr, ref r, 0.025f, 0.015f);
            AmbDrone(b, 47f, 0.025f, sr);
            // Satu motif identik disalin bit-for-bit, tanpa jitter per ulang.
            var motif = AmbJangkrik(sr, ref r);
            for (int k = 0; k < 6; k++) AmbMix(b, motif, k * ShotSeconds, 0.12f, sr);
            var gong = AmbGamelan(sr, ref r); AmbDistant(gong, 420f, 0.8f, sr);
            AmbMix(b, gong, 3f, 0.06f, sr); AmbMix(b, gong, 15f, 0.06f, sr);
            return b;
        }
        private static float[] AmbPabrik(int sr, ref Dsp.Rng r)
        {
            var b = AmbWind(sr, ref r, 0.12f, 0.03f);
            var pipe = Dsp.Biquad.BP(r.Range(115f, 145f), 18f, sr);
            var pipe2 = Dsp.Biquad.BP(385f, 12f, sr);
            var sheet = Dsp.Biquad.BP(730f, 7f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr, w = r.White();
                float gust = 0.5f + 0.5f * Mathf.Sin(t * 0.47f);
                float chatter = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(t * 37f)), 8f);
                b[i] += (pipe.Process(w) * 0.8f + pipe2.Process(w) * 0.35f) * gust
                    + sheet.Process(w) * chatter * gust * 0.1f;
            }
            var chain = AmbRantai(sr, ref r); AmbMix(b, chain, 5f, 0.12f, sr);
            AmbMix(b, chain, 18f, 0.08f, sr);
            // Logam tertarik angin: mode inharmonik bergerak perlahan.
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr, env = AmbWindow(t - 11f, 2.4f);
                b[i] += Dsp.FM(t, 280f, 61f, 1f + Mathf.Sin(t)) * env * 0.025f;
            }
            return b;
        }
        private static float[] AmbTebing(int sr, ref Dsp.Rng r)
        {
            var b = AmbWind(sr, ref r, 0.4f, 0.04f);
            var whistle = Dsp.Biquad.BP(r.Range(700f, 1100f), 9f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                b[i] += whistle.Process(r.White()) * Mathf.Pow(0.5f + 0.5f * Mathf.Sin(t * 0.37f), 3f) * 0.2f;
            }
            Dsp.Delay(b, 0.19f, 0.28f, 0.2f, sr);
            return b;
        }

        // Blok bersama hanya primitif akustik/mixing, bukan generator spesies.
        private static float AmbWindow(float t, float duration)
        {
            if (t <= 0f || t >= duration) return 0f;
            return Mathf.Pow(Mathf.Sin(Mathf.PI * t / duration), 2f);
        }
        private static void AmbSyllable(float[] b, float start, float duration,
            float from, float to, float gain, float roughness, int sr, ref Dsp.Rng r)
        {
            int first = (int)(start * sr), n = (int)(duration * sr);
            float phase = 0f;
            var breath = Dsp.Biquad.BP(1800f, 0.8f, sr);
            for (int j = 0; j < n && first + j < b.Length; j++)
            {
                float t = (float)j / sr;
                float f = Dsp.ExpSweep(from, to, t, duration);
                phase = Mathf.Repeat(phase + Tau * f / sr, Tau);
                float source = Mathf.Sin(phase + roughness * Mathf.Sin(phase * 0.49f))
                    + breath.Process(r.White()) * roughness * 0.06f;
                b[first + j] += source * gain * AmbWindow(t, duration);
            }
        }
        private static void AmbMetal(float[] b, float start, float hz, float decay,
            float gain, int sr, ref Dsp.Rng r)
        {
            int first = (int)(start * sr);
            float detune = r.Range(0.99f, 1.01f);
            for (int i = first; i < b.Length; i++)
            {
                float t = (float)(i - first) / sr;
                float mode = Mathf.Sin(Tau * hz * t) * Mathf.Exp(-t / decay)
                    + 0.6f * Mathf.Sin(Tau * hz * 2.71f * detune * t) * Mathf.Exp(-t / (decay * 0.55f))
                    + 0.3f * Mathf.Sin(Tau * hz * 4.13f * t) * Mathf.Exp(-t / (decay * 0.3f));
                b[i] += (mode + r.White() * Mathf.Exp(-t / 0.002f) * 0.3f) * gain;
            }
        }
        private static void AmbDistant(float[] b, float hz, float wet, int sr)
        {
            var lp = Dsp.Biquad.LP(hz, AirQ, sr);
            for (int i = 0; i < b.Length; i++) b[i] = lp.Process(b[i]);
            Dsp.Schroeder(b, wet, sr);
        }
        private static void AmbMix(float[] b, float[] voice, float at, float gain, int sr)
        {
            int first = Mathf.Max(0, (int)(at * sr));
            int n = Mathf.Min(voice.Length, b.Length - first);
            int fade = (int)(EndFade * sr);
            for (int i = 0; i < n; i++)
            {
                float tail = Mathf.Clamp01((float)(voice.Length - 1 - i) / fade);
                b[first + i] += voice[i] * gain * tail;
            }
        }
        private static void AmbScatter(float[] b, float[] voice, float start,
            float interval, float gain, int sr, ref Dsp.Rng r)
        {
            for (float t = start; t < BedSeconds; t += interval + r.Range(-0.25f, 0.25f))
                AmbMix(b, voice, t, gain * r.Range(0.55f, 1f), sr);
        }
        private static void AmbDrone(float[] b, float hz, float gain, int sr)
        {
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                b[i] += (Mathf.Sin(Tau * hz * t) + 0.4f * Mathf.Sin(Tau * (hz + 0.7f) * t))
                    * gain * (0.7f + 0.3f * Mathf.Sin(t * 0.23f));
            }
        }
        private static float[] AmbWind(int sr, ref Dsp.Rng r, float wind, float leaves)
        {
            var b = Dsp.Buffer(BedSeconds, sr);
            var pink = new Dsp.Pink();
            var low = Dsp.OnePole.LP(650f, sr);
            var high = Dsp.OnePole.HP(2700f, sr);
            var middle = Dsp.Biquad.BP(1200f, 0.6f, sr);
            float offset = r.Range(0f, Tau);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr, w = r.White();
                float gust = 0.55f + 0.25f * Mathf.Sin(t * 0.39f + offset)
                    + 0.2f * Mathf.Sin(t * 0.87f);
                float rustle = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(t * 2.7f + offset)), 4f);
                b[i] = low.Process(pink.Next(w)) * wind * gust
                    + middle.Process(w) * wind * gust * 0.08f
                    + high.Process(w) * leaves * rustle * gust;
            }
            return b;
        }
    }
}
