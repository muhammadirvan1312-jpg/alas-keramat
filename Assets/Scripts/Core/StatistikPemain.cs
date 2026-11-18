using AlasKeramat.AI;
using AlasKeramat.Player;
using UnityEngine;
namespace AlasKeramat.Core
{
    // Layar Statistik (04 §7.10): Hari Bertahan, Pohon Ditebang, Bangunan Dibuat, Musuh Dikalahkan, Hantu Diusir, Sekte Diampuni,
    // Barang Dirakit, Jarak Ditempuh (km, 1 desimal), Lore Ditemukan (x/12).
    public class StatistikPemain : MonoBehaviour, ISaveable
    {
        public const float TickJarak = 0.5f, LompatanMaks = 20f;
        [System.Serializable] class DataStat { public int pohon, bangunan, musuh, rakit; public float jarak; }
        DataStat _d = new DataStat();
        AndiFPSController _andi;
        Vector3 _posLalu;
        bool _adaPos;
        float _t;

        public string SaveKey { get { return "statistik"; } }
        public int Pohon { get { return _d.pohon; } }
        public float JarakMeter { get { return _d.jarak; } }
        void Awake() { GameServices.Register(this); }
        void OnEnable() { GameEvents.OnTreeFelled += Pohon1; GameEvents.OnStructureBuilt += Bangun1; GameEvents.OnEnemyKilled += Musuh1; }
        void OnDisable() { GameEvents.OnTreeFelled -= Pohon1; GameEvents.OnStructureBuilt -= Bangun1; GameEvents.OnEnemyKilled -= Musuh1; }
        void Pohon1(int id, TreeKind k) { _d.pohon++; }
        void Bangun1(BlueprintID b) { _d.bangunan++; }
        void Musuh1(EnemyArchetype a, bool diampuni) { if (!diampuni) _d.musuh++; }
        public static void CatatRakit() { var s = GameServices.Get<StatistikPemain>(); if (s != null) s._d.rakit++; }

        void Update()
        {
            _t += Time.deltaTime; if (_t < TickJarak) return; _t = 0f;
            if (_andi == null) { _andi = GameServices.Get<AndiFPSController>(); if (_andi == null) return; }
            Vector3 p = _andi.transform.position; p.y = 0f;
            if (_adaPos) { float m = Vector3.Distance(p, _posLalu); if (m < LompatanMaks) _d.jarak += m; }
            _posLalu = p; _adaPos = true;
        }

        public static string Susun(int hari, int pohon, int bangunan, int musuh, int hantu, int ampuni, int rakit, float meter, int lore)
        {
            return "Hari Bertahan  " + hari + "\nPohon Ditebang  " + pohon + "\nBangunan Dibuat  " + bangunan + "\nMusuh Dikalahkan  " + musuh
                + "\nHantu Diusir  " + hantu + "\nSekte Diampuni  " + ampuni + "\nBarang Dirakit  " + rakit
                + "\nJarak Ditempuh  " + (meter / 1000f).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " km\nLore Ditemukan  " + lore + "/" + LoreLibrary.Jumlah;
        }

        public string Teks()
        {
            var jam = GameServices.Get<TimeOfDayClock>();
            var dir = GameServices.Get<EnemyDirector>();
            var q = GameServices.Get<QuestManager>();
            return Susun(jam != null ? jam.Hari : 1, _d.pohon, _d.bangunan, _d.musuh, dir != null ? dir.HantuDiusir : 0, dir != null ? dir.SekteDiampuni : 0,
                _d.rakit, _d.jarak, q != null ? LoreLibrary.HitungDitemukan(q.LoreMask) : 0);
        }

        public string CaptureJson() { return JsonUtility.ToJson(_d); }
        public void RestoreJson(string json) { var d = JsonUtility.FromJson<DataStat>(json); if (d != null) _d = d; _adaPos = false; }
    }
}
