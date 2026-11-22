using System;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.World
{
    // Atmosfer mistis hidup di situs keramat Nusantara (Altar Beringin, Taman Megalit, Kuil Bambu, Candi & Pasar Hantu):
    // 24 bola cahaya roh penjaga yang mengorbit pelan saat Maghrib/Malam, 8 kelelawar/burung malam melintas senja,
    // serta menhir berukir yang berpendar saat Andi mendekat di malam hari.
    public class RohKeramatPenjaga : MonoBehaviour
    {
        [Serializable]
        public struct RohOrbit
        {
            public Transform tr;
            public Vector3 pusat;
            public float radius;
            public float tinggi;
            public float laju;
            public float fasa;
        }

        [Serializable]
        public struct KelelawarMalam
        {
            public Transform tr;
            public Transform sayapKiri;
            public Transform sayapKanan;
            public Vector3 pusat;
            public float radius;
            public float laju;
            public float fasa;
        }

        [SerializeField] RohOrbit[] roh = new RohOrbit[0];
        [SerializeField] KelelawarMalam[] kelelawar = new KelelawarMalam[0];
        [SerializeField] Renderer[] menhirRune = new Renderer[0];

        MaterialPropertyBlock _mpb;
        Transform _kamera;
        TimeOfDayClock _jam;
        bool _siap;

        public int JumlahEntitasMistis
        {
            get
            {
                return (roh != null ? roh.Length : 0)
                    + (kelelawar != null ? kelelawar.Length : 0)
                    + (menhirRune != null ? menhirRune.Length : 0);
            }
        }

        public void Pasang(RohOrbit[] r, KelelawarMalam[] k, Renderer[] m)
        {
            roh = r ?? new RohOrbit[0];
            kelelawar = k ?? new KelelawarMalam[0];
            menhirRune = m ?? new Renderer[0];
        }

        public static float KekuatanMistisMalam(float jam)
        {
            if (jam >= 18.2f || jam <= 5.2f) return 1.0f;
            if (jam >= 17.0f) return Mathf.InverseLerp(17.0f, 18.2f, jam);
            if (jam <= 6.4f) return 1.0f - Mathf.InverseLerp(5.2f, 6.4f, jam);
            return 0f;
        }

        void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            _siap = true;
        }

        void Update()
        {
            if (!_siap || !Application.isPlaying) return;
            if (_kamera == null) { var c = Camera.main; if (c != null) _kamera = c.transform; }
            if (_kamera == null) return;
            Vector3 cPos = _kamera.position;
            if (WorldTerrainGenerator.DiBawahTanah(cPos.y)) return;

            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            float jam = _jam != null ? _jam.Jam : 10f;
            float mistis = KekuatanMistisMalam(jam);
            float t = Time.time;

            // Bola roh penjaga mengorbit situs keramat saat Maghrib & Malam.
            for (int i = 0; i < roh.Length; i++)
            {
                var r = roh[i];
                if (r.tr == null) continue;
                bool aktif = mistis > 0.05f && (r.pusat - cPos).sqrMagnitude < 110f * 110f;
                if (r.tr.gameObject.activeSelf != aktif) r.tr.gameObject.SetActive(aktif);
                if (!aktif) continue;

                float ph = t * r.laju + r.fasa;
                float x = r.pusat.x + Mathf.Cos(ph) * r.radius;
                float z = r.pusat.z + Mathf.Sin(ph * 1.5f) * r.radius;
                float y = r.pusat.y + r.tinggi + Mathf.Sin(ph * 2.1f) * 0.95f;
                r.tr.position = new Vector3(x, y, z);
                float skala = (0.65f + 0.35f * Mathf.Sin(t * 3.4f + r.fasa)) * mistis;
                r.tr.localScale = Vector3.one * (0.22f * skala);
            }

            // Kelelawar & burung malam terbang melingkar di atas Candi, Megalit & Desa Mati.
            for (int i = 0; i < kelelawar.Length; i++)
            {
                var k = kelelawar[i];
                if (k.tr == null) continue;
                bool aktif = mistis > 0.15f && (k.pusat - cPos).sqrMagnitude < 95f * 95f;
                if (k.tr.gameObject.activeSelf != aktif) k.tr.gameObject.SetActive(aktif);
                if (!aktif) continue;

                float ph = t * k.laju + k.fasa;
                float x = k.pusat.x + Mathf.Cos(ph) * k.radius;
                float z = k.pusat.z + Mathf.Sin(ph) * k.radius;
                float y = k.pusat.y + 7.5f + Mathf.Sin(ph * 2.6f) * 2.2f;
                Vector3 np = new Vector3(x, y, z);
                Vector3 arah = np - k.tr.position;
                k.tr.position = np;
                if (arah.sqrMagnitude > 0.0001f)
                    k.tr.rotation = Quaternion.LookRotation(arah.normalized, Vector3.up);
                float kepak = Mathf.Sin(t * 16f + k.fasa) * 42f;
                if (k.sayapKiri != null) k.sayapKiri.localRotation = Quaternion.Euler(0f, 0f, kepak);
                if (k.sayapKanan != null) k.sayapKanan.localRotation = Quaternion.Euler(0f, 0f, -kepak);
            }

            // Rune batu megalit & candi berpendar hangat saat malam dan menguat saat Andi mendekat.
            for (int i = 0; i < menhirRune.Length; i++)
            {
                var mr = menhirRune[i];
                if (mr == null) continue;
                float dSq = (mr.transform.position - cPos).sqrMagnitude;
                if (dSq > 100f * 100f) continue;
                float dekat = 1f - Mathf.Clamp01(Mathf.Sqrt(dSq) / 36f);
                float denyut = 0.72f + 0.28f * Mathf.Sin(t * 2.4f + i * 1.1f);
                float kuat = mistis * (0.45f + 0.95f * dekat) * denyut;
                _mpb.SetColor("_EmissionColor", new Color(0.28f * kuat, 1.35f * kuat, 1.15f * kuat, 1f));
                mr.SetPropertyBlock(_mpb);
            }
        }
    }
}
