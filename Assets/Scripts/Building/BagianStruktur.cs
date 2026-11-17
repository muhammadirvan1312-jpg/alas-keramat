using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Potongan bangunan bebas (07 §5-6): tiang, balok, papan, pagar runcing. Simpul graf integritas; HP 07 §6.
    public class BagianStruktur : MonoBehaviour, IDamageable
    {
        public enum Tipe { Tiang, Balok, Papan, PagarRuncing, BambuTanam }
        public const float HPGelondong = 200f, HPPapan = 120f, HPRuncing = 300f, HPRuncingBambu = 150f, LamaRuntuh = 20f;

        public Tipe Jenis;
        public bool Tertanam;
        public float HP;
        public float Panjang;
        public readonly List<BagianStruktur> Tetangga = new List<BagianStruktur>();
        public int SerutKe;
        bool _runtuh;
        public static readonly List<BagianStruktur> Semua = new List<BagianStruktur>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() { Semua.Clear(); }
        void OnEnable() { Semua.Add(this); }
        void OnDisable() { Semua.Remove(this); }

        public bool IsAlive { get { return HP > 0f; } }

        public static float HPAwal(Tipe t) { return t == Tipe.Papan ? HPPapan : t == Tipe.PagarRuncing ? HPRuncing : t == Tipe.BambuTanam ? HPRuncingBambu : HPGelondong; }

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
            if (_runtuh || HP <= 0f) return;
            HP -= d.amount * (d.type == DamageType.Api ? 1.5f : 1f);
            if (HP <= 0f) Hancur(true);
        }

        public void Hancur(bool jatuhkanKayu)
        {
            if (_runtuh) return;
            _runtuh = true;
            var tetangga = new List<BagianStruktur>(Tetangga);
            foreach (var t in tetangga) t.Tetangga.Remove(this);
            Tetangga.Clear();
            if (jatuhkanKayu && Jenis != Tipe.BambuTanam) KayuFisik.Buat(transform.position + Vector3.up * 0.3f, transform.up, GetComponent<MeshRenderer>() != null ? GetComponent<MeshRenderer>().sharedMaterial : null, Jenis == Tipe.Papan);
            StructureIntegrity.Periksa(tetangga);
            Destroy(gameObject);
        }

        // Dipanggil integritas saat potongan tidak tertopang: jadi rigidbody, jatuh, hilang 20 s lalu menjatuhkan 50% kayu.
        public void Runtuhkan()
        {
            if (_runtuh) return;
            _runtuh = true;
            foreach (var t in Tetangga) t.Tetangga.Remove(this);
            Tetangga.Clear();
            gameObject.layer = GameLayers.LogPhysics;
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = Jenis == Tipe.Papan ? 18f : 45f;
            Destroy(gameObject, LamaRuntuh);
            if (Random.value < 0.5f) KayuFisik.Buat(transform.position + Vector3.up * 0.5f, transform.up, GetComponent<MeshRenderer>() != null ? GetComponent<MeshRenderer>().sharedMaterial : null, Jenis == Tipe.Papan);
        }
    }
}
