using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.Player
{
    // Sistem kombo 3-serangan & critical hit: ayunan beruntun dalam 1,35 detik memicu Kombo x2 (1,18x) dan Finisher x3 (1,45x).
    public class SistemKomboDanStagger : MonoBehaviour
    {
        public const float JendelaKombo = 1.35f;

        int _langkahKombo;
        float _waktuTerakhir = -10f;

        public int LangkahKombo { get { return _langkahKombo; } }

        public static float PengaliKombo(int langkah, float stamina01)
        {
            float dasar = langkah >= 3 ? 1.45f : (langkah == 2 ? 1.18f : 1.0f);
            if (stamina01 < 0.15f) dasar *= 0.78f;
            return dasar;
        }

        public float CatatSerangan(float waktuKini, float stamina01, Vector3 titikKena, out bool finisher)
        {
            if (waktuKini - _waktuTerakhir <= JendelaKombo)
                _langkahKombo = (_langkahKombo % 3) + 1;
            else
                _langkahKombo = 1;
            _waktuTerakhir = waktuKini;
            finisher = _langkahKombo == 3;
            float mult = PengaliKombo(_langkahKombo, stamina01);

            if (finisher)
            {
                var andi = GameServices.Get<AndiFPSController>();
                if (andi != null) andi.GuncangKamera(0.16f);
                var gore = GameServices.Get<GoreSystem>();
                if (gore != null) gore.PercikPartikel(titikKena, new Color(1f, 0.78f, 0.35f, 0.85f), 8, 0.06f, 2.4f, 0.45f);
                var lvl = GameServices.Get<LevelSurvivalAndi>();
                if (lvl != null) lvl.TambahXP(LevelSurvivalAndi.Atribut.Kekuatan, 12f);
            }
            return mult;
        }

        void Awake() { GameServices.Register(this); }
    }
}
