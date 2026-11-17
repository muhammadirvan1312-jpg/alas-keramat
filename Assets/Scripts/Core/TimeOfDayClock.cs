using UnityEngine;

namespace AlasKeramat.Core
{
    // Jam dunia (docs/gdd/05 §1): 1 detik nyata = 1 menit game; Hari 1 mulai 08:00, skala 0,65 sampai 18:00.
    public class TimeOfDayClock : MonoBehaviour, ISaveable
    {
        public const float MenitPerDetik = 1f;
        public const float SkalaTutorial = 0.65f;
        public const float JamMulai = 8f, JamAkhirTutorial = 18f, JamMaghrib = 18f;

        [SerializeField] float jam = JamMulai;
        [SerializeField] int hari = 1;
        float _pengaliDebug = 1f;
        int _jamTerakhir = -1;
        bool _maghribHariIni;

        public float Jam { get { return jam; } }
        public int Hari { get { return hari; } }
        public float Pengali { get { return _pengaliDebug; } set { _pengaliDebug = Mathf.Max(0f, value); } }
        public bool Malam { get { return jam >= 19f || jam < 5f; } }
        public string SaveKey { get { return "jam"; } }

        void Awake() { GameServices.Register(this); _jamTerakhir = Mathf.FloorToInt(jam); }

        void Update()
        {
            float skala = (hari == 1 && jam < JamAkhirTutorial) ? SkalaTutorial : 1f;
            Maju(Time.deltaTime * MenitPerDetik / 60f * skala * _pengaliDebug);
        }

        public void Maju(float jamTambah)
        {
            jam += jamTambah;
            while (jam >= 24f) { jam -= 24f; hari++; _maghribHariIni = false; GameEvents.RaiseDayChanged(hari); }
            int j = Mathf.FloorToInt(jam);
            if (j != _jamTerakhir) { _jamTerakhir = j; GameEvents.RaiseHourChanged(j); }
            if (!_maghribHariIni && jam >= JamMaghrib && jam < JamMaghrib + 1f) { _maghribHariIni = true; GameEvents.RaiseMaghrib(); }
        }

        public void SetWaktu(float jamBaru, int hariBaru)
        {
            jam = Mathf.Repeat(jamBaru, 24f); hari = Mathf.Max(1, hariBaru);
            _jamTerakhir = Mathf.FloorToInt(jam); _maghribHariIni = jam >= JamMaghrib;
        }

        // Label HUD (05 §1).
        public static string Label(float j)
        {
            if (j < 4f) return "DINI HARI";
            if (j < 6f) return "SUBUH";
            if (j < 11f) return "PAGI";
            if (j < 15f) return "SIANG";
            if (j < 17.5f) return "SORE";
            if (j < 19f) return "MAGHRIB";
            return "MALAM";
        }

        [System.Serializable] struct Data { public float jam; public int hari; }
        public string CaptureJson() { return JsonUtility.ToJson(new Data { jam = jam, hari = hari }); }
        public void RestoreJson(string json) { var d = JsonUtility.FromJson<Data>(json); SetWaktu(d.jam, d.hari); }
    }
}
