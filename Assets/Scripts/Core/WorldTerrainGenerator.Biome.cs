using UnityEngine;

namespace AlasKeramat.Core
{
    // Bioma (03 §5 diskalakan v5) + zona baru v5 (20 §2.2). ID biome baru FINAL.
    public static partial class WorldTerrainGenerator
    {
        public const Biome RawaBakau = (Biome)11, PabrikGula = (Biome)12, GunungKapur = (Biome)13, LadangTebu = (Biome)14, AlasWingit = (Biome)15;

        public struct InfoZona { public string nama; public Vector3 pusat; public InfoZona(string n, float x, float z) { nama = n; pusat = new Vector3(x, 0f, z); } }

        // 12 zona permukaan v5 (pusat dalam koordinat dunia).
        public static readonly InfoZona[] Zona =
        {
            new InfoZona("Zona 1 - Titik Jatuh Heli", 0f, -440f), new InfoZona("Zona 2 - Lembah Bambu", 280f, -260f),
            new InfoZona("Zona 3 - Desa Mati Pasir Lenyap", 500f, 180f), new InfoZona("Zona 4 - Sawit Terbakar & Rawa Merah", -520f, 240f),
            new InfoZona("Zona 5 - Beringin Agung", 0f, 0f), new InfoZona("Zona 6 - Tebing Kapur Utara", 0f, 680f),
            new InfoZona("Zona 7 - Pantai Karang", 0f, -840f), new InfoZona("Zona 8 - Rawa Bakau", -650f, -560f),
            new InfoZona("Zona 9 - Pabrik Gula Sinar Tebu", 660f, -500f), new InfoZona("Zona 10 - Gunung Kapur & Air Terjun", -600f, 660f),
            new InfoZona("Zona 11 - Kampung Sukamaju", 620f, 620f), new InfoZona("Zona 12 - Alas Wingit", -760f, -40f),
        };

        public static Biome GetBiome(float x, float z)
        {
            float h = SampleHeight(x, z);
            if (h < 0f) return Biome.Laut;
            if (Dist(x, z, -650f, -560f) < 200f && h < 8f) return RawaBakau;
            if (Dist(x, z, 660f, -500f) < 170f) return PabrikGula;
            if (Dist(x, z, -600f, 660f) < 260f) return GunungKapur;
            if (Dist(x, z, 620f, 620f) < 260f) return LadangTebu;
            if (Dist(x, z, -760f, -40f) < 230f) return AlasWingit;

            float x3 = x / SkalaXZ, z3 = z / SkalaXZ;
            if (z3 < -340f && h < 4f * SkalaTinggi) return Biome.Pantai;
            if (Mathf.Sqrt(x3 * x3 + z3 * z3) < 78f) return Biome.KawahBeringin;
            if (TPlateau03(x3, z3, Land03(x3, z3)) > 0.5f) return Biome.TebingKapur;
            if (x3 >= -345f && x3 <= -180f && z3 >= 40f && z3 <= 200f) return Biome.SawitTerbakar;
            if (Dist(x3, z3, 270f, 98f) < 70f) return Biome.Desa;
            float wl, w, dalam;
            float dUtama = JarakSungai(SungaiUtama, x3, z3, out wl, out w, out dalam);
            if (dUtama < 55f && z3 >= -220f && z3 <= 20f && x3 > 80f) return Biome.HutanBambu;
            float dAnak = JarakSungai(AnakSungaiHeli, x3, z3, out wl, out w, out dalam);
            if (Mathf.Min(dUtama, dAnak) < 30f) return Biome.TepiSungai;
            if (z3 < -140f) return Biome.HutanJatiSelatan;
            return Biome.HutanCampuran;
        }

        public static string NamaZonaTerdekat(Vector3 p)
        {
            int best = 0; float bd = float.MaxValue;
            for (int i = 0; i < Zona.Length; i++)
            {
                float d = Dist(p.x, p.z, Zona[i].pusat.x, Zona[i].pusat.z);
                if (d < bd) { bd = d; best = i; }
            }
            return Zona[best].nama;
        }
    }
}
