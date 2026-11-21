using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Ikan sungai/pantai yang bisa ditangkap langsung saat Andi mengarungi air dangkal (respawn 180 detik).
    public class IkanTangkap : MonoBehaviour, IInteractable
    {
        public const float LamaRespawn = 180f;
        float _respawn;
        Renderer[] _rend;

        public bool Tersedia { get { return _respawn <= 0f; } }
        public float HoldDuration { get { return 0f; } }
        public string GetPromptText(GameObject actor) { return Tersedia ? "TANGKAP IKAN" : ""; }
        public bool CanInteract(GameObject actor) { return Tersedia; }

        void Awake() { _rend = GetComponentsInChildren<Renderer>(true); }

        public void PerbaruiRespawn(float dt)
        {
            if (_respawn <= 0f) return;
            _respawn -= dt;
            if (_respawn <= 0f) AturTampak(true);
        }

        public void Interact(GameObject actor)
        {
            if (!Tersedia) return;
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv == null) return;
            if (inv.Tambah(ItemID.IkanMentah, 1) <= 0) { GameEvents.RaiseToast("Tas penuh."); return; }
            _respawn = LamaRespawn;
            AturTampak(false);
            GameEvents.RaiseToast("Menangkap 1 Ikan Mentah!");
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.PlayItemPickup();
            var gore = GameServices.Get<GoreSystem>();
            if (gore != null) gore.PercikPartikel(transform.position + Vector3.up * 0.1f, new Color(0.75f, 0.90f, 0.98f, 0.7f), 10, 0.055f, 1.8f, 0.5f);
        }

        void AturTampak(bool v)
        {
            if (_rend == null) return;
            for (int i = 0; i < _rend.Length; i++) if (_rend[i] != null) _rend[i].enabled = v;
        }
    }

    // Pengelola tunggal zero-GC untuk kawanan ikan sungai/pantai & kepiting pasir pantai (hemat RAM 4 GB).
    public class BiotaAirHidup : MonoBehaviour
    {
        public const float JarakAktif = 55f, JarakKagetIkan = 4.5f, JarakKagetKepiting = 5.0f;

        [System.Serializable]
        public struct Entitas
        {
            public Transform tr;
            public Vector3 pusat;
            public float radius, laju, fase;
            public bool kepiting;
            public IkanTangkap tangkap;
        }

        [SerializeField] Entitas[] daftar = new Entitas[0];
        Transform _cam;
        AndiFPSController _andi;

        public int JumlahEntitas { get { return daftar != null ? daftar.Length : 0; } }
        public void Pasang(Entitas[] d) { daftar = d ?? new Entitas[0]; }

        public static Vector3 HitungPosisiRenang(Vector3 pusat, float radius, float sudut, bool kepiting)
        {
            if (kepiting)
            {
                float x = Mathf.Sin(sudut) * radius;
                float z = Mathf.Cos(sudut * 0.7f) * radius * 0.55f;
                return new Vector3(pusat.x + x, pusat.y, pusat.z + z);
            }
            return new Vector3(pusat.x + Mathf.Cos(sudut) * radius, pusat.y + Mathf.Sin(sudut * 2.1f) * 0.04f, pusat.z + Mathf.Sin(sudut) * radius);
        }

        void Update()
        {
            if (daftar == null || daftar.Length == 0 || JedaDunia.Dijeda) return;
            if (_cam == null) { var c = Camera.main; if (c != null) _cam = c.transform; }
            if (_cam == null) return;
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            Vector3 cp = _cam.transform.position;
            Vector3 ap = _andi != null ? _andi.transform.position : cp;
            float dt = Time.deltaTime;
            float t = Time.time;
            float maksSq = JarakAktif * JarakAktif;

            for (int i = 0; i < daftar.Length; i++)
            {
                var e = daftar[i];
                if (e.tr == null) continue;
                if (e.tangkap != null) e.tangkap.PerbaruiRespawn(dt);
                Vector3 dCam = e.pusat - cp;
                bool dekat = dCam.x * dCam.x + dCam.z * dCam.z <= maksSq;
                if (e.tr.gameObject.activeSelf != dekat) e.tr.gameObject.SetActive(dekat);
                if (!dekat) continue;

                float jKaget = e.kepiting ? JarakKagetKepiting : JarakKagetIkan;
                Vector3 dAndi = e.tr.position - ap; dAndi.y = 0f;
                float kaget = dAndi.sqrMagnitude < jKaget * jKaget ? 2.4f : 1f;
                float sudut = t * e.laju * kaget + e.fase;
                Vector3 pBaru = HitungPosisiRenang(e.pusat, e.radius, sudut, e.kepiting);
                if (e.kepiting) pBaru.y = WorldTerrainGenerator.SampleHeight(pBaru.x, pBaru.z) + 0.04f;
                Vector3 arah = pBaru - e.tr.position; arah.y = 0f;
                if (arah.sqrMagnitude > 1e-6f)
                {
                    Quaternion rot = Quaternion.LookRotation(arah);
                    if (e.kepiting) rot *= Quaternion.Euler(0f, 90f, 0f); // kepiting berjalan menyamping
                    e.tr.rotation = Quaternion.Slerp(e.tr.rotation, rot, dt * 7f);
                }
                e.tr.position = pBaru;
            }
        }
    }
}
