using UnityEngine;

namespace AlasKeramat.Core
{
    // 21 §10 T3: foley langkah per permukaan (3 variasi), napas lelah, detak jantung. Nama: langkah_<permukaan>_<v>.
    public partial class ProceduralAudioSynth
    {
        public static readonly string[] PermukaanLangkah = { "tanah", "daun", "pasir", "air", "kayu", "batu" };
        public const int VariasiLangkah = 3;
        public const string KlipNapas = "napas_lelah", KlipJantung = "jantung";
        public static readonly string[] KlipFoley = BuatDaftarFoley();

        static string[] BuatDaftarFoley()
        {
            var d = new string[PermukaanLangkah.Length * VariasiLangkah + 2];
            int i = 0;
            foreach (var p in PermukaanLangkah) for (int v = 0; v < VariasiLangkah; v++) d[i++] = "langkah_" + p + "_" + v;
            d[i++] = KlipNapas; d[i] = KlipJantung;
            return d;
        }

        public static string NamaLangkah(SurfaceType s, int v)
        {
            int idx = s == SurfaceType.DaunKering ? 1 : s == SurfaceType.Pasir ? 2 : s == SurfaceType.Air ? 3 : s == SurfaceType.Kayu ? 4 : (s == SurfaceType.Beton || s == SurfaceType.BatuGua) ? 5 : 0;
            return "langkah_" + PermukaanLangkah[idx] + "_" + Mathf.Abs(v % VariasiLangkah);
        }

        public static float[] GenerateFoley(string nama, int variasi, out int sr)
        {
            sr = Dsp.SrSfx;
            var rng = Dsp.Rng.DariNama(nama, variasi);
            float[] b;
            if (nama == KlipNapas) b = ResepNapas(sr, ref rng);
            else if (nama == KlipJantung) b = ResepJantung(sr);
            else if (nama.StartsWith("langkah_"))
            {
                string p = nama.Substring(8, nama.LastIndexOf('_') - 8);
                b = ResepLangkah(System.Array.IndexOf(PermukaanLangkah, p), sr, ref rng);
                if (b == null) return null;
            }
            else return null;
            Dsp.Finalize(b, sr);
            return b;
        }

        // Langkah = hentakan tumit (thump rendah) + gesekan/remah sesuai permukaan.
        static float[] ResepLangkah(int jenis, int sr, ref Dsp.Rng rng)
        {
            if (jenis < 0) return null;
            var b = Dsp.Buffer(jenis == 3 ? 0.45f : 0.32f, sr);
            float fThump = jenis == 4 ? 140f : jenis == 5 ? 220f : 85f;
            var lpThump = Dsp.Biquad.LP(fThump * 2.2f, 0.9f, sr);
            var bp = jenis == 1 ? Dsp.Biquad.BP(3200f, 0.6f, sr) : jenis == 2 ? Dsp.Biquad.BP(2400f, 0.5f, sr) : jenis == 3 ? Dsp.Biquad.BP(900f, 0.8f, sr)
                : jenis == 4 ? Dsp.Biquad.BP(700f, 2.5f, sr) : jenis == 5 ? Dsp.Biquad.BP(2800f, 1.5f, sr) : Dsp.Biquad.BP(1300f, 0.7f, sr);
            float gesek = jenis == 1 ? 0.9f : jenis == 2 ? 0.55f : jenis == 3 ? 0.8f : jenis == 5 ? 0.45f : 0.35f;
            float ekor = jenis == 1 ? 0.09f : jenis == 3 ? 0.16f : 0.05f;
            float geser = rng.Range(0.012f, 0.03f);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                float thump = lpThump.Process(rng.White()) * Dsp.AD(t, 0.003f, 0.035f) * 1.4f + Mathf.Sin(Dsp.DuaPi * fThump * t) * Dsp.AD(t, 0.002f, 0.03f) * 0.5f;
                float n = rng.White();
                if (jenis == 1 && rng.Range(0f, 1f) < 0.02f) n *= 4f; // remah daun kering
                float rasp = bp.Process(n) * Dsp.AD(t - geser, 0.004f, ekor) * gesek;
                if (jenis == 3) rasp *= 0.6f + 0.4f * Mathf.Sin(Dsp.DuaPi * 18f * t); // cipratan
                b[i] = thump + rasp;
            }
            if (jenis == 4) Dsp.KarplusStrong(b, 0, b.Length / 3, 180f + rng.Range(-20f, 20f), sr, ref rng, 0.15f); // papan berderit
            if (jenis == 5) Dsp.Schroeder(b, 0.25f, sr);
            return b;
        }

        // Napas berat: tarik (noise naik, formant) - buang (lebih panjang), 2,2 s.
        static float[] ResepNapas(int sr, ref Dsp.Rng rng)
        {
            var b = Dsp.Buffer(2.2f, sr);
            var bp = Dsp.Biquad.BP(1100f, 0.9f, sr);
            var lp = Dsp.Biquad.LP(2600f, 0.7f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                float tarik = t < 0.75f ? Mathf.Sin(Mathf.PI * t / 0.75f) * 0.7f : 0f;
                float buang = t >= 0.85f && t < 2.0f ? Mathf.Sin(Mathf.PI * (t - 0.85f) / 1.15f) : 0f;
                b[i] = lp.Process(bp.Process(rng.White()) * (tarik + buang) + rng.White() * 0.05f * buang);
            }
            Dsp.Formant(b, 600f, 1250f, sr);
            return b;
        }

        // Detak jantung "lub-dub" teredam, satu siklus 0,85 s.
        static float[] ResepJantung(int sr)
        {
            var b = Dsp.Buffer(0.85f, sr);
            for (int i = 0; i < b.Length; i++)
            {
                float t = (float)i / sr;
                float lub = Mathf.Sin(Dsp.DuaPi * Dsp.ExpSweep(65f, 42f, t, 0.12f) * t) * Dsp.AD(t, 0.006f, 0.07f);
                float dub = Mathf.Sin(Dsp.DuaPi * Dsp.ExpSweep(80f, 50f, t - 0.28f, 0.1f) * (t - 0.28f)) * Dsp.AD(t - 0.28f, 0.005f, 0.05f) * 0.7f;
                b[i] = Dsp.SoftClip((lub + dub) * 1.6f);
            }
            return b;
        }
    }
}
