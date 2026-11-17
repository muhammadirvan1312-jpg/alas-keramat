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
        public string Kunci = System.Guid.NewGuid().ToString("N");
        Rigidbody _rb;
        public bool Papan { get; private set; }
        int _pukulBelah;
        public const int PukulanBelah = 3;
        public static readonly Vector3 UkuranPapan = new Vector3(0.36f, Panjang, 0.08f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() { _aktif.Clear(); }

        public static int JumlahAktif { get { return _aktif.Count; } }

        public static KayuFisik Buat(Vector3 pusat, Vector3 sumbu, Material kulit, bool papan = false)
        {
            var go = GameObject.CreatePrimitive(papan ? PrimitiveType.Cube : PrimitiveType.Capsule);
            go.name = papan ? "PapanKayu" : "KayuGelondong";
            go.layer = GameLayers.LogPhysics; go.tag = GameTags.LogItem;
            go.transform.SetPositionAndRotation(pusat, Quaternion.FromToRotation(Vector3.up, sumbu));
            go.transform.localScale = papan ? UkuranPapan : new Vector3(Radius * 2f, Panjang * 0.5f, Radius * 2f);
            if (kulit != null) go.GetComponent<MeshRenderer>().sharedMaterial = kulit;
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = papan ? 18f : Massa;
            PhysicsCompat.SetDrag(rb, 0.2f); PhysicsCompat.SetAngularDrag(rb, 0.5f);
            rb.sleepThreshold = 0.1f;
            var k = go.AddComponent<KayuFisik>();
            k._rb = rb; k.Papan = papan;
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
        public string GetPromptText(GameObject actor) { return Papan ? "AMBIL PAPAN" : "AMBIL KAYU"; }

        // 07 §5 mode 4: 3 pukulan memanjang pada gelondong tergeletak -> 2 papan.
        public void Pukul()
        {
            if (Papan) return;
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.PlayWoodChop(transform.position);
            if (++_pukulBelah < PukulanBelah) { GameEvents.RaiseToast("BELAH PAPAN " + _pukulBelah + "/" + PukulanBelah); return; }
            Vector3 s = transform.up, kanan = Vector3.Cross(s, Vector3.up).normalized;
            if (kanan.sqrMagnitude < 0.01f) kanan = Vector3.right;
            var mat = GetComponent<MeshRenderer>().sharedMaterial;
            Buat(transform.position + kanan * 0.2f + Vector3.up * 0.1f, s, mat, true);
            Buat(transform.position - kanan * 0.2f + Vector3.up * 0.1f, s, mat, true);
            GameEvents.RaiseToast("Kayu terbelah jadi 2 papan");
            Destroy(gameObject);
        }
        public bool CanInteract(GameObject actor) { return true; }

        public void Interact(GameObject actor)
        {
            var pemanggul = GameServices.Get<ShoulderLogAndFreeformBuilder>();
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (pemanggul != null && pemanggul.Panggul(Papan ? ShoulderLogAndFreeformBuilder.Muatan.Papan : ShoulderLogAndFreeformBuilder.Muatan.Gelondong, GetComponent<MeshRenderer>().sharedMaterial))
            {
                if (synth != null) synth.PlayLogPickup(transform.position);
                Destroy(gameObject);
                return;
            }
            var inv = GameServices.Get<TikarPandan3DInventory>();
            var id = Papan ? ItemID.PapanKayu : ItemID.KayuGelondong;
            if (inv == null || inv.Tambah(id, 1) == 0) { GameEvents.RaiseToast("Bahu & tas sudah penuh"); return; }
            if (synth != null) synth.PlayLogPickup(transform.position);
            GameEvents.RaiseToast("+1 " + ItemDatabase.Get(id).displayName);
            Destroy(gameObject);
        }
    }
}
