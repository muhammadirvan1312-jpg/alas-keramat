using UnityEngine;

namespace AlasKeramat.AI
{
    // 06 §5 pendengaran Sekte: setiap sumber bunyi dicatat sebagai peristiwa (posisi, radius, waktu) di penyangga cincin.
    // Musuh membaca peristiwa baru sejak bacaan terakhirnya; bunyi terdengar bila jarak <= radius bunyi (dan <= jangkauan dengar musuh x 2).
    public static class MeterSuara
    {
        public const float Lari = 20f, Jalan = 8f, Jongkok = 3f, Tebang = 45f, PohonTumbang = 80f, Bedil = 120f, Umpan = 15f, Pertarungan = 25f;
        public const int Kapasitas = 32;

        public struct Bunyi { public Vector3 pos; public float radius, waktu; public int nomor; public bool dariAndi; }

        static readonly Bunyi[] _cincin = new Bunyi[Kapasitas];
        static int _nomor;

        public static int NomorTerakhir { get { return _nomor; } }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() { _nomor = 0; System.Array.Clear(_cincin, 0, Kapasitas); }

        public static void Pancar(Vector3 pos, float radius, bool dariAndi = true)
        {
            _nomor++;
            _cincin[_nomor % Kapasitas] = new Bunyi { pos = pos, radius = radius, waktu = Time.time, nomor = _nomor, dariAndi = dariAndi };
        }

        // Bunyi terkeras yang terdengar oleh pendengar sejak nomor 'sejak'. Mengembalikan false bila tidak ada.
        public static bool Dengar(Vector3 telinga, float jangkauanDengar, ref int sejak, out Bunyi terdengar)
        {
            terdengar = default(Bunyi);
            bool ada = false; float terbaik = 0f;
            int mulai = Mathf.Max(sejak + 1, _nomor - Kapasitas + 1);
            for (int n = mulai; n <= _nomor; n++)
            {
                var b = _cincin[n % Kapasitas];
                if (b.nomor != n) continue;
                if (Terdengar(telinga, jangkauanDengar, b.pos, b.radius))
                {
                    float kuat = 1f - Vector3.Distance(telinga, b.pos) / Mathf.Max(0.01f, b.radius);
                    if (!ada || kuat > terbaik) { terdengar = b; terbaik = kuat; ada = true; }
                }
            }
            sejak = _nomor;
            return ada;
        }

        public static bool Terdengar(Vector3 telinga, float jangkauanDengar, Vector3 sumber, float radiusBunyi)
        {
            if (jangkauanDengar <= 0f) return false;
            float d = Vector3.Distance(telinga, sumber);
            return d <= radiusBunyi && d <= jangkauanDengar * 2f + radiusBunyi * 0.5f;
        }

        // Radius langkah Andi menurut gaya gerak (06 §5).
        public static float RadiusLangkah(bool lari, bool jongkok, float kecepatan)
        {
            if (kecepatan < 0.3f) return 0f;
            if (jongkok) return Jongkok;
            return lari ? Lari : Jalan;
        }
    }
}
