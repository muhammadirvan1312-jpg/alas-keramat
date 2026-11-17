using AlasKeramat.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    // Tur Pratinjau Otomatis (docs/tech/12 T2): kamera sinematik mengunjungi 12 zona, waktu dipercepat
    // agar pagi, Maghrib, dan malam terlihat. Aktif selama Andi belum ada (sebelum T3).
    public class TurPratinjau : MonoBehaviour
    {
        public const float DetikPerZona = 14f;
        public const float PercepatWaktu = 20f;
        const float JarakKamera = 140f, TinggiKamera = 45f, TinggiTarget = 8f, OrbitDerajat = 25f;

        [SerializeField] Text labelZona;
        [SerializeField] Text labelJam;

        float _t;
        int _zona;
        TimeOfDayClock _jam;
        string _jamTampil = "";

        public void Pasang(Text zona, Text jam) { labelZona = zona; labelJam = jam; }

        void Start()
        {
            _jam = GameServices.Get<TimeOfDayClock>();
            if (_jam != null) _jam.Pengali = PercepatWaktu;
            TampilkanZona();
        }

        void LateUpdate()
        {
            var zona = WorldTerrainGenerator.Zona;
            if (zona.Length == 0) return;
            _t += Time.deltaTime;
            if (_t >= DetikPerZona) { _t = 0f; _zona = (_zona + 1) % zona.Length; TampilkanZona(); }
            float k = _t / DetikPerZona;
            Vector3 pusat = zona[_zona].pusat;
            pusat.y = Mathf.Max(0f, WorldTerrainGenerator.SampleHeight(pusat.x, pusat.z)) + TinggiTarget;
            // Orbit pelan mengelilingi pusat zona, menghadap ke arah laut di sisi luar.
            float sudut = Mathf.Atan2(pusat.z, pusat.x) * Mathf.Rad2Deg + 180f + Mathf.Lerp(-OrbitDerajat, OrbitDerajat, Mathf.SmoothStep(0f, 1f, k));
            Vector3 arah = Quaternion.Euler(0f, -sudut, 0f) * Vector3.right;
            Vector3 pos = pusat - arah * JarakKamera;
            float tanah = Mathf.Max(0f, WorldTerrainGenerator.SampleHeight(pos.x, pos.z));
            pos.y = Mathf.Max(tanah + 6f, pusat.y + TinggiKamera * Mathf.Lerp(1.2f, 0.7f, k));
            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(pusat - pos);

            if (labelJam != null && _jam != null)
            {
                int j = Mathf.FloorToInt(_jam.Jam), m = Mathf.FloorToInt((_jam.Jam - j) * 60f);
                string s = "Hari " + _jam.Hari + "  " + j.ToString("00") + ":" + (m / 10 * 10).ToString("00") + "  " + TimeOfDayClock.Label(_jam.Jam);
                if (s != _jamTampil) { _jamTampil = s; labelJam.text = s; }
            }
        }

        void TampilkanZona()
        {
            if (labelZona != null && WorldTerrainGenerator.Zona.Length > 0) labelZona.text = WorldTerrainGenerator.Zona[_zona].nama;
        }
    }
}
