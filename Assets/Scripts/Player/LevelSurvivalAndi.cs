using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Player
{
    // Sistem evolusi fisik survival setara SOTF (Strength/Fitness): 4 atribut naik level 1..10 dari aksi nyata & tersimpan.
    public class LevelSurvivalAndi : MonoBehaviour, ISaveable
    {
        public enum Atribut { Kekuatan = 0, StaminaLari = 1, Ketahanan = 2, KeahlianRakit = 3 }
        public const int LevelMaks = 10;
        public const float XPPerLevel = 100f;

        readonly float[] _xp = new float[4];
        readonly int[] _lvl = { 1, 1, 1, 1 };

        public string SaveKey { get { return "level_survival_andi"; } }
        public int AmbilLevel(Atribut a) { return _lvl[(int)a]; }
        public float BonusDamage { get { return 1f + (_lvl[0] - 1) * 0.06f; } }
        public float PenghematanStamina { get { return 1f - (_lvl[1] - 1) * 0.035f; } }
        public float BonusKetahanan { get { return 1f - (_lvl[2] - 1) * 0.03f; } }

        public static int HitungLevelDariXP(float xpTotal)
        {
            return Mathf.Clamp(1 + Mathf.FloorToInt(Mathf.Max(0f, xpTotal) / XPPerLevel), 1, LevelMaks);
        }

        public static string NamaAtribut(Atribut a)
        {
            switch (a)
            {
                case Atribut.Kekuatan: return "Kekuatan";
                case Atribut.StaminaLari: return "Stamina Lari";
                case Atribut.Ketahanan: return "Ketahanan Tubuh";
                default: return "Keahlian Rakit";
            }
        }

        void Awake() { GameServices.Register(this); }

        public void TambahXP(Atribut a, float jumlah)
        {
            if (jumlah <= 0f) return;
            int idx = (int)a;
            _xp[idx] += jumlah;
            int baru = HitungLevelDariXP(_xp[idx]);
            if (baru > _lvl[idx])
            {
                _lvl[idx] = baru;
                GameEvents.RaiseToast(NamaAtribut(a) + " naik ke Level " + baru + "!");
                var synth = GameServices.Get<ProceduralAudioSynth>();
                if (synth != null) synth.PlayItemPickup();
            }
        }

        [System.Serializable] struct DataLevel { public float x0, x1, x2, x3; }
        public string CaptureJson() { return JsonUtility.ToJson(new DataLevel { x0 = _xp[0], x1 = _xp[1], x2 = _xp[2], x3 = _xp[3] }); }
        public void RestoreJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            var d = JsonUtility.FromJson<DataLevel>(json);
            _xp[0] = d.x0; _xp[1] = d.x1; _xp[2] = d.x2; _xp[3] = d.x3;
            for (int i = 0; i < 4; i++) _lvl[i] = HitungLevelDariXP(_xp[i]);
        }
    }
}
