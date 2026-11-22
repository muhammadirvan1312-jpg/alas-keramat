using System;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.Core
{
    // Jurnal Eksplorasi Pulau Alas Keramat (22 lokasi ikonik): memicu banner penemuan sinematik,
    // memberi XP Ketahanan & Stamina Lari, dan menyimpan progres penjelajahan ke SaveSystem.
    public class JurnalEksplorasiPulau : MonoBehaviour, ISaveable
    {
        [Serializable]
        public struct ZonaTemu
        {
            public string nama;
            public Vector2 posisiXZ;
            public float radius;
        }

        [SerializeField] ZonaTemu[] daftarZona = new ZonaTemu[0];
        bool[] _ditemukan = new bool[0];
        int _jumlahTemu;
        float _tCek;
        AndiFPSController _andi;

        public event Action<string, int, int> OnLokasiDitemukan;

        public int JumlahDitemukan { get { return _jumlahTemu; } }
        public int TotalZona { get { return daftarZona != null ? daftarZona.Length : 0; } }
        public string SaveKey { get { return "jurnal_eksplorasi_pulau"; } }

        public void Pasang(ZonaTemu[] zona)
        {
            daftarZona = zona ?? new ZonaTemu[0];
            _ditemukan = new bool[daftarZona.Length];
            _jumlahTemu = 0;
        }

        public static bool CekDalamRadius(Vector3 pos, Vector2 pusatXZ, float radius)
        {
            float dx = pos.x - pusatXZ.x, dz = pos.z - pusatXZ.y;
            return dx * dx + dz * dz <= radius * radius;
        }

        void Awake()
        {
            GameServices.Register(this);
            if (_ditemukan == null || _ditemukan.Length != daftarZona.Length)
                _ditemukan = new bool[daftarZona.Length];
        }

        void Update()
        {
            if (JedaDunia.Dijeda || daftarZona.Length == 0) return;
            _tCek += Time.deltaTime;
            if (_tCek < 0.65f) return;
            _tCek = 0f;

            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            if (_andi == null) return;
            Vector3 p = _andi.transform.position;

            for (int i = 0; i < daftarZona.Length; i++)
            {
                if (_ditemukan[i]) continue;
                if (!CekDalamRadius(p, daftarZona[i].posisiXZ, daftarZona[i].radius)) continue;
                _ditemukan[i] = true;
                _jumlahTemu++;

                var lvl = GameServices.Get<LevelSurvivalAndi>();
                if (lvl != null)
                {
                    lvl.TambahXP(LevelSurvivalAndi.Atribut.Ketahanan, 25f);
                    lvl.TambahXP(LevelSurvivalAndi.Atribut.StaminaLari, 15f);
                }
                var synth = GameServices.Get<ProceduralAudioSynth>();
                if (synth != null) synth.PlayItemPickup();

                var h = OnLokasiDitemukan;
                if (h != null) h(daftarZona[i].nama, _jumlahTemu, daftarZona.Length);

                if (_jumlahTemu == 5 || _jumlahTemu == 10 || _jumlahTemu == 15 || _jumlahTemu == daftarZona.Length)
                {
                    var vital = GameServices.Get<AndiVitalsAndSanity>();
                    if (vital != null) vital.UbahSanity(20f);
                    GameEvents.RaiseToast("Tonggak Penjelajah (" + _jumlahTemu + "/" + daftarZona.Length + ")! Kewarasan +20.");
                }
                break;
            }
        }

        void PastikanArray()
        {
            if (_ditemukan == null || _ditemukan.Length != daftarZona.Length)
                _ditemukan = new bool[daftarZona.Length];
        }

        [Serializable] struct DataJurnal { public int mask; }
        public string CaptureJson()
        {
            PastikanArray();
            int mask = 0;
            for (int i = 0; i < _ditemukan.Length && i < 31; i++) if (_ditemukan[i]) mask |= (1 << i);
            return JsonUtility.ToJson(new DataJurnal { mask = mask });
        }

        public void RestoreJson(string json)
        {
            PastikanArray();
            if (string.IsNullOrEmpty(json)) return;
            int mask = JsonUtility.FromJson<DataJurnal>(json).mask;
            _jumlahTemu = 0;
            for (int i = 0; i < _ditemukan.Length && i < 31; i++)
            {
                _ditemukan[i] = (mask & (1 << i)) != 0;
                if (_ditemukan[i]) _jumlahTemu++;
            }
        }
    }
}
