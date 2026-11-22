using System;
using AlasKeramat.Core;
using UnityEngine;
using JH = AlasKeramat.Core.ResepHewan.JenisHewan;

namespace AlasKeramat.AI
{
    // Memunculkan & menjaga 28 satwa bertulang/FBX ASLI (memakai template asli EcosystemDirector: Rusa, Kancil,
    // Babi Hutan, Ayam Hutan, Monyet, Anjing, Kucing, Biawak, Buaya) secara permanen di sekitar landmark kunci
    // sehingga seluruh 4 kuadran pulau selalu dipenuhi satwa berkualitas tinggi, bukan kapsul primitif.
    public class SatwaAsliPulauV9 : MonoBehaviour
    {
        [Serializable]
        public struct TitikHabitat
        {
            public JH jenis;
            public Vector3 posisi;
            public float yaw;
        }

        [SerializeField] TitikHabitat[] habitat = new TitikHabitat[0];

        GameObject[] _instansi;
        EcosystemDirector _eco;
        Transform _kamera;
        float _cekT;
        bool _siap;

        public int JumlahHabitat { get { return habitat != null ? habitat.Length : 0; } }

        public void Pasang(TitikHabitat[] daftar)
        {
            habitat = daftar ?? new TitikHabitat[0];
        }

        public static bool BolehMuncul(float jarakSq, float batasMeter)
        {
            return jarakSq < batasMeter * batasMeter;
        }

        void Awake()
        {
            _instansi = new GameObject[habitat.Length];
            _siap = true;
        }

        void Update()
        {
            if (!_siap || !Application.isPlaying) return;
            _cekT -= Time.deltaTime;
            if (_cekT > 0f) return;
            _cekT = 1.25f;

            if (_kamera == null) { var c = Camera.main; if (c != null) _kamera = c.transform; }
            if (_kamera == null) return;
            Vector3 cPos = _kamera.position;
            if (WorldTerrainGenerator.DiBawahTanah(cPos.y)) return;

            if (_eco == null) _eco = GameServices.Get<EcosystemDirector>();
            if (_eco == null) return;

            for (int i = 0; i < habitat.Length; i++)
            {
                var h = habitat[i];
                Vector3 d = h.posisi - cPos;
                float distSq = d.x * d.x + d.z * d.z;
                bool dekat = BolehMuncul(distSq, 85f);

                if (dekat && _instansi[i] == null)
                {
                    var tpl = _eco.AmbilTemplate(h.jenis);
                    if (tpl == null) continue;
                    float y = Mathf.Max(0.45f, WorldTerrainGenerator.SampleHeight(h.posisi.x, h.posisi.z));
                    var go = Instantiate(tpl, new Vector3(h.posisi.x, y, h.posisi.z), Quaternion.Euler(0f, h.yaw, 0f), transform);
                    go.name = "SatwaAsliV9_" + h.jenis + "_" + i;
                    go.SetActive(true);
                    _instansi[i] = go;
                }
                else if (!dekat && _instansi[i] != null && distSq > 115f * 115f)
                {
                    Destroy(_instansi[i]);
                    _instansi[i] = null;
                }
            }
        }
    }
}
