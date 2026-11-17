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
        private const int JumlahBedAmbien = 10;
        private const float DurasiBedAmbien = 24.25f, DurasiPanggilan = 3.8f;
        private const float ReverbAlam = 0.12f, ReverbLogam = 0.38f;
        private const float SerangAmbien = 0.012f, EkorAmbien = 0.25f;

        public static float[] GenerateAmbien(string nama, int variasi, out int sr)
        {
            sr = Dsp.SrSfx;
            int jenis = System.Array.IndexOf(KlipAmbien, nama);
            if (jenis < 0) return null;
            var rng = Dsp.Rng.DariNama(nama, variasi);
            bool bed = jenis < JumlahBedAmbien;
            float[] b = bed ? BuatBedAmbien(jenis, sr, ref rng)
                : BuatPanggilan(jenis - JumlahBedAmbien, sr, ref rng);
            Dsp.Finalize(b, sr);
            // Finalize memberi fade ujung. Overlap dilakukan sesudahnya agar
            // fade tidak membuat lubang setiap pengulangan bed.
            return bed ? Dsp.LoopCrossfade(b, sr) : b;
        }

        private static float[] BuatBedAmbien(int jenis, int sr, ref Dsp.Rng rng)
        {
            const float AnginHz = 650f, OmbakHz = 1700f, Q = 0.7f;
            const float DasarAngin = 0.09f, DasarOmbak = 0.35f;
            const float IntervalMin = 1.8f, IntervalMax = 4.2f;
            var b = Dsp.Buffer(DurasiBedAmbien, sr);
            var pink = new Dsp.Pink();
            var lp = Dsp.Biquad.LP(jenis == 5 ? OmbakHz : AnginHz, Q, sr);
            float offset = rng.Range(0f, Dsp.DuaPi);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                float gust = 0.5f + 0.3f * Mathf.Sin(t * 0.63f + offset)
                    + 0.2f * Mathf.Sin(t * 1.13f);
                float gain = jenis == 5 ? DasarOmbak : DasarAngin;
                b[i] = lp.Process(pink.Next(rng.White())) * gain * gust;
            }
            for (float t = 0.3f; t < DurasiBedAmbien - DurasiPanggilan;
                t += jenis == 7 ? 2.7f : rng.Range(IntervalMin, IntervalMax))
            {
                int call = jenis == 0 ? 5 : jenis == 1 || jenis == 2 ? 4
                    : jenis == 6 ? 1 : jenis == 8 ? 10 : jenis == 9 ? 8 : 0;
                if (jenis == 5) continue;
                float[] voice = BuatPanggilan(call, sr, ref rng);
                float gain = jenis == 4 ? 0.035f : 0.22f;
                MelapisAmbien(b, voice, (int)(t * sr), gain);
            }
            if (jenis == 7)
                MelapisAmbien(b, BuatPanggilan(12, sr, ref rng), sr, 0.08f);
            return b;
        }

        private static void MelapisAmbien(float[] dst, float[] src, int mulai, float gain)
        {
            for (int i = 0; i < src.Length && mulai + i < dst.Length; i++)
                dst[mulai + i] += src[i] * gain;
        }

        private static float[] BuatPanggilan(int jenis, int sr, ref Dsp.Rng rng)
        {
            // Parameter timbre: frekuensi, pulsa, dan peluruhan per spesies.
            float[] nada = { 4700f, 260f, 720f, 360f, 6200f, 2400f,
                680f, 1150f, 180f, 800f, 1450f, 2900f, 190f };
            float[] periode = { 0.38f, 0.64f, 1.05f, 1.35f, 0.12f,
                0.47f, 1.8f, 0.95f, 1.2f, 0.16f, 0.31f, 0.23f, 1.5f };
            const float Q = 1.5f, NoiseGain = 0.25f, DetuneMin = 0.94f, DetuneMax = 1.06f;
            var b = Dsp.Buffer(DurasiPanggilan, sr);
            float f = nada[jenis] * rng.Range(DetuneMin, DetuneMax);
            float period = periode[jenis] * rng.Range(DetuneMin, DetuneMax);
            var bp = Dsp.Biquad.BP(f, Q, sr);
            var air = Dsp.Biquad.BP(3200f, 0.7f, sr);
            float phase = 0f;
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr, p = t % period;
                float env = Dsp.AD(p, SerangAmbien, period * 0.18f);
                float noise = bp.Process(rng.White());
                float sweep = 1f + 0.22f * Mathf.Sin(Dsp.DuaPi * p / period);
                if (jenis == 2)
                {
                    // Dua suku: "to" rendah, "kek" lebih tinggi dan kasar.
                    bool kek = p >= 0.28f;
                    float local = kek ? p - 0.28f : p;
                    sweep = kek ? 1.55f : 0.85f;
                    env = Dsp.AD(local, SerangAmbien, kek ? 0.085f : 0.12f);
                }
                phase = Mathf.Repeat(phase + Dsp.DuaPi * f * sweep / sr, Dsp.DuaPi);
                float fm = Mathf.Sin(phase + 1.4f * Mathf.Sin(phase * 1.97f));
                float s = (fm * 0.6f + noise * NoiseGain) * env;
                if (jenis == 0 || jenis == 4)
                    s = (noise + 0.3f * fm) * env *
                        (0.5f + 0.5f * Mathf.Sin(Dsp.DuaPi * 75f * t));
                if (jenis == 3) s = (Mathf.Sin(phase) + noise * 0.12f) * env;
                if (jenis == 8) s = (noise + fm * 0.2f) * env;
                if (jenis == 9)
                    s = air.Process(rng.White()) * env *
                        Mathf.Clamp01(1f - t / DurasiPanggilan);
                if (jenis == 11)
                    s = (noise + rng.White() * 0.15f) *
                        (0.15f + 0.85f * Mathf.Pow(Mathf.Sin(t * 13f), 2f));
                if (jenis == 12)
                    s = (Mathf.Sin(phase) + 0.5f * Mathf.Sin(phase * 2.76f)
                        + 0.3f * Mathf.Sin(phase * 4.07f)) * Dsp.AD(p, 0.004f, 0.7f);
                b[i] = s * Mathf.Clamp01(t / SerangAmbien)
                    * Mathf.Clamp01((DurasiPanggilan - t) / EkorAmbien);
            }
            if (jenis == 8 || jenis == 10 || jenis == 12)
                Dsp.KarplusStrong(b, 0, b.Length, f, sr, ref rng, 0.2f);
            Dsp.Schroeder(b, jenis >= 10 ? ReverbLogam : ReverbAlam, sr);
            return b;
        }
    }
}
