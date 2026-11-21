using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    // Penanda jarak & arah landmark terdekat di HUD atas (memudahkan navigasi pemain Android di hutan luas).
    public class PenandaLandmarkHUD : MonoBehaviour
    {
        public const float Interval = 0.5f;

        [System.Serializable]
        public struct TitikLandmark
        {
            public string nama;
            public Vector3 posisi;
        }

        [SerializeField] Text teksLandmark;
        [SerializeField] TitikLandmark[] daftar = new TitikLandmark[0];

        float _t;
        int _lastHash = int.MinValue;
        AndiFPSController _andi;

        public int JumlahLandmark { get { return daftar != null ? daftar.Length : 0; } }
        public void Pasang(Text t, TitikLandmark[] d) { teksLandmark = t; daftar = d ?? new TitikLandmark[0]; }

        public static int CariTerdekat(Vector3 pos, TitikLandmark[] d, out float jarak)
        {
            jarak = float.MaxValue;
            int best = -1;
            if (d == null) return -1;
            for (int i = 0; i < d.Length; i++)
            {
                Vector3 v = d[i].posisi - pos; v.y = 0f;
                float m = v.magnitude;
                if (m < jarak) { jarak = m; best = i; }
            }
            return best;
        }

        void Update()
        {
            if (teksLandmark == null || daftar == null || daftar.Length == 0) return;
            _t += Time.deltaTime;
            if (_t < Interval) return;
            _t = 0f;

            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            if (_andi == null) return;
            Vector3 p = _andi.transform.position;
            if (WorldTerrainGenerator.DiBawahTanah(p.y))
            {
                if (_lastHash != -1) { _lastHash = -1; teksLandmark.text = "BAWAH TANAH"; }
                return;
            }
            float jarak;
            int idx = CariTerdekat(p, daftar, out jarak);
            if (idx < 0) return;
            int jBulat = Mathf.RoundToInt(jarak / 5f) * 5;
            Vector3 dir = daftar[idx].posisi - p;
            string arah = KompasSuhuHUD.ArahKardinal(Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg);
            int hash = idx * 10000 + jBulat * 16 + arah[0];
            if (hash == _lastHash) return;
            _lastHash = hash;
            teksLandmark.text = "TERDEKAT: " + daftar[idx].nama + " (" + arah + ", " + jBulat + " m)";
        }
    }
}
