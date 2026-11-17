using System.Collections.Generic;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.Building
{
    // 07 §3: kayu gelondong fisik (3,6 m, r 0,22, massa 45), layer LogPhysics, menggelinding & mengapung; maks 40 aktif (terjauh dibekukan).
    // Interaksi "AMBIL KAYU" -> inventaris (maks 2 dibawa; panggul bahu di T6).
    public class KayuFisik : MonoBehaviour, IInteractable
    {
        public const float Panjang = 3.6f, Radius = 0.22f, Massa = 45f, MaksAktif = 40, GayaApung = 14f;
        static readonly List<KayuFisik> _aktif = new List<KayuFisik>();
        Rigidbody _rb;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() { _aktif.Clear(); }

        public static int JumlahAktif { get { return _aktif.Count; } }

        public static KayuFisik Buat(Vector3 pusat, Vector3 sumbu, Material kulit)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "KayuGelondong";
            go.layer = GameLayers.LogPhysics; go.tag = GameTags.LogItem;
            go.transform.SetPositionAndRotation(pusat, Quaternion.FromToRotation(Vector3.up, sumbu));
            go.transform.localScale = new Vector3(Radius * 2f, Panjang * 0.5f, Radius * 2f);
            if (kulit != null) go.GetComponent<MeshRenderer>().sharedMaterial = kulit;
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = Massa;
            PhysicsCompat.SetDrag(rb, 0.2f); PhysicsCompat.SetAngularDrag(rb, 0.5f);
            rb.sleepThreshold = 0.1f;
            var k = go.AddComponent<KayuFisik>();
            k._rb = rb;
            return k;
        }

        // Benda kecil yang bisa diambil (ranting, kelapa) dijatuhkan di dekat tajuk.
        public static void Benda(string nama, Vector3 pos, ItemID id, int n, string tampil)
        {
            float h = WorldTerrainGenerator.SampleHeight(pos.x, pos.z);
            var go = GameObject.CreatePrimitive(id == ItemID.KelapaMuda ? PrimitiveType.Sphere : PrimitiveType.Cube);
            go.name = nama;
            go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
            go.transform.position = new Vector3(pos.x, Mathf.Max(h, 0f) + 0.15f, pos.z);
            go.transform.localScale = id == ItemID.KelapaMuda ? Vector3.one * 0.25f : new Vector3(0.6f, 0.05f, 0.05f);
            var w = go.AddComponent<WadahItem>();
            w.Pasang(tampil, new[] { id }, new[] { n }, 0, true, null);
        }

        void OnEnable() { _aktif.Add(this); Batasi(); }
        void OnDisable() { _aktif.Remove(this); }

        void Batasi()
        {
            if (_aktif.Count <= MaksAktif) return;
            var cam = Camera.main;
            Vector3 p = cam != null ? cam.transform.position : transform.position;
            KayuFisik jauh = null; float dj = -1f;
            foreach (var k in _aktif) { if (k._rb == null || k._rb.isKinematic) continue; float d = (k.transform.position - p).sqrMagnitude; if (d > dj) { dj = d; jauh = k; } }
            if (jauh != null) jauh._rb.isKinematic = true;
        }

        void FixedUpdate()
        {
            if (_rb == null || _rb.isKinematic) return;
            Vector3 p = transform.position;
            float? wl = WorldTerrainGenerator.GetWaterLevelAt(p.x, p.z);
            if (wl.HasValue && p.y < wl.Value) _rb.AddForce(Vector3.up * GayaApung * Massa * Mathf.Clamp01((wl.Value - p.y) / Radius), ForceMode.Force);
        }

        public float HoldDuration { get { return 0f; } }
        public string GetPromptText(GameObject actor) { return "AMBIL KAYU"; }
        public bool CanInteract(GameObject actor) { return true; }

        public void Interact(GameObject actor)
        {
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv == null) return;
            if (inv.Tambah(ItemID.KayuGelondong, 1) == 0) { GameEvents.RaiseToast("Tak sanggup membawa lebih dari " + ItemDatabase.MaxCarry(ItemID.KayuGelondong) + " kayu"); return; }
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.PlayLogPickup(transform.position);
            GameEvents.RaiseToast("+1 Kayu Gelondong");
            Destroy(gameObject);
        }
    }
}
