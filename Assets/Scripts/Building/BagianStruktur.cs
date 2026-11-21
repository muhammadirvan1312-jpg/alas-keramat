using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Potongan bangunan bebas (07 §5-6): tiang, balok, papan, pagar runcing. Simpul graf integritas; HP 07 §6.
    public class BagianStruktur : MonoBehaviour, IDamageable, IInteractable
    {
        public enum Tipe { Tiang, Balok, Papan, PagarRuncing, BambuTanam, Atap, Tangga, FondasiBatu, DindingBatu, JalanBatu }
        public const float HPGelondong = 200f, HPPapan = 120f, HPRuncing = 300f, HPRuncingBambu = 150f, LamaRuntuh = 20f;

        public Tipe Jenis;
        public bool Tertanam;
        public float HP;
        public float Panjang;
        public readonly List<BagianStruktur> Tetangga = new List<BagianStruktur>();
        public string Kunci = System.Guid.NewGuid().ToString("N");
        public int SerutKe, PukulCelah;
        bool _runtuh;
        public bool Batu { get { return Jenis >= Tipe.FondasiBatu; } }
        public bool BisaDisimpan { get { return !_runtuh; } }
        public float HoldDuration { get { return 1f; } }
        public string GetPromptText(GameObject actor) { return "BONGKAR"; }
        public bool CanInteract(GameObject actor) { return !_runtuh; }
        public void Interact(GameObject actor) { Hancur(true); }
        public static bool HasilPapan(Tipe t) { return t == Tipe.Papan || t == Tipe.Atap; }

        public static readonly List<BagianStruktur> Semua = new List<BagianStruktur>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() { Semua.Clear(); }
        void OnEnable() { if (!Semua.Contains(this)) Semua.Add(this); }
        void OnDisable() { Semua.Remove(this); }

        public bool IsAlive { get { return HP > 0f; } }

        public static float HPAwal(Tipe t) { return t >= Tipe.FondasiBatu ? 500f : t == Tipe.Papan ? HPPapan : t == Tipe.PagarRuncing ? HPRuncing : t == Tipe.BambuTanam ? HPRuncingBambu : HPGelondong; }

        public static int JumlahPartikelRuntuh(Tipe jenis, bool batu)
        {
            return batu ? 18 : jenis == Tipe.Papan ? 10 : 14;
        }

        void PancarSerpihanRuntuh()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("SerpihanRuntuh");
            go.transform.position = transform.position + Vector3.up * 0.4f;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var m = ps.main;
            m.playOnAwake = false; m.loop = false; m.startLifetime = 0.65f; m.startSpeed = 2.8f;
            m.startSize = Batu ? 0.08f : 0.06f; m.gravityModifier = 1.4f;
            m.simulationSpace = ParticleSystemSimulationSpace.World;
            Color w = Batu ? new Color(0.52f, 0.50f, 0.48f) : new Color(0.56f, 0.40f, 0.22f);
            m.startColor = w;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.35f;
            var pr = go.GetComponent<ParticleSystemRenderer>();
            pr.sharedMaterial = SafeMaterialFactory.GetCached(Batu ? "M_DebuRuntuhBatu" : "M_SerpihRuntuhKayu", w, 0f, 0.2f);
            pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ps.Emit(JumlahPartikelRuntuh(Jenis, Batu));
            Destroy(go, 1.2f);
        }

        public void Hubungkan(BagianStruktur lain)
        {
            if (lain == null || lain == this) return;
            if (!Tetangga.Contains(lain)) Tetangga.Add(lain);
            if (!lain.Tetangga.Contains(this)) lain.Tetangga.Add(this);
        }

        public Vector3 Ujung(int sisi) { return transform.position + transform.up * (sisi == 0 ? -Panjang * 0.5f : Panjang * 0.5f); }
        public Vector3 Puncak { get { return Jenis == Tipe.Tiang || Jenis == Tipe.PagarRuncing || Jenis == Tipe.BambuTanam ? Ujung(1) : transform.position; } }

        public void TakeDamage(DamageInfo d)
        {
            if (_runtuh || HP <= 0f || (Batu && d.type == DamageType.Api)) return;
            HP -= d.amount * (d.type == DamageType.Api ? 1.5f : 1f);
            if (HP <= 0f) Hancur(true);
        }

        public void Hancur(bool jatuhkanKayu)
        {
            if (_runtuh) return;
            _runtuh = true;
            PancarSerpihanRuntuh();
            var tetangga = new List<BagianStruktur>(Tetangga);
            foreach (var t in tetangga) if (t != null) t.Tetangga.Remove(this);
            Tetangga.Clear();
            if (jatuhkanKayu && Batu) KayuFisik.Benda("BatuBesar", transform.position, ItemID.BatuBesar, 1, "BATU BESAR");
            if (jatuhkanKayu && !Batu && Jenis != Tipe.BambuTanam) KayuFisik.Buat(transform.position + Vector3.up * 0.3f, transform.up, GetComponent<MeshRenderer>() != null ? GetComponent<MeshRenderer>().sharedMaterial : null, HasilPapan(Jenis));
            StructureIntegrity.Periksa(tetangga);
            gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(gameObject); else DestroyImmediate(gameObject);
        }

        // Dipanggil integritas saat potongan tidak tertopang: jadi rigidbody, jatuh, hilang 20 s lalu menjatuhkan 50% kayu.
        public void Runtuhkan()
        {
            if (_runtuh) return;
            _runtuh = true;
            foreach (var t in Tetangga) if (t != null) t.Tetangga.Remove(this);
            Tetangga.Clear();
            gameObject.layer = GameLayers.LogPhysics;
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = Jenis == Tipe.Papan ? 18f : 45f;
            Destroy(gameObject, LamaRuntuh);
            if (!Batu && Random.value < 0.5f) KayuFisik.Buat(transform.position + Vector3.up * 0.5f, transform.up, GetComponent<MeshRenderer>() != null ? GetComponent<MeshRenderer>().sharedMaterial : null, HasilPapan(Jenis));
        }
    }
}
