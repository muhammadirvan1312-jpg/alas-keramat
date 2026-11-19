using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.AI
{
    // T8: tumpukan hasil kerja Kang Deden di dekat api kemah (kayu, ranting, batu, makanan) bila tak ada cetak biru/Rak yang menampung.
    // "AMBIL TUMPUKAN": ranting/batu/makanan masuk tas, kayu dipanggul (maks 2).
    public class TumpukanKemah : MonoBehaviour, IInteractable, ISaveable
    {
        int _kayu, _ranting, _batu, _makanan;
        Material _mat;
        Transform _vis;

        public int Kayu { get { return _kayu; } }
        public float HoldDuration { get { return 0f; } }
        public string SaveKey { get { return "tumpukan_" + Mathf.RoundToInt(transform.position.x) + "_" + Mathf.RoundToInt(transform.position.z); } }

        public static TumpukanKemah Ambil(Vector3 kemah, Material mat)
        {
            var cols = Physics.OverlapSphere(kemah, 6f, 1 << GameLayers.Interactable, QueryTriggerInteraction.Collide);
            foreach (var c in cols) { var t = c.GetComponentInParent<TumpukanKemah>(); if (t != null) return t; }
            Vector3 p = kemah + new Vector3(2.5f, 0f, -1.5f);
            return BuatDi(new Vector3(p.x,WorldTerrainGenerator.SampleHeight(p.x,p.z),p.z),mat);
        }

        public static TumpukanKemah BuatDi(Vector3 posisi, Material mat)
        {
            var go = new GameObject("TumpukanKemah");
            go.transform.position=posisi;
            go.layer = GameLayers.Interactable;
            var sc = go.AddComponent<SphereCollider>(); sc.isTrigger = true; sc.radius = 1.2f; sc.center = Vector3.up * 0.4f;
            var t2 = go.AddComponent<TumpukanKemah>(); t2._mat = mat;
            return t2;
        }

        public void Tambah(ItemID id, int n)
        {
            if (id == ItemID.KayuGelondong) _kayu += n; else if (id == ItemID.Ranting) _ranting += n; else if (id == ItemID.Batu) _batu += n; else _makanan += n;
            Gambar();
        }

        void Gambar()
        {
            if (_vis == null) { _vis = new GameObject("Visual").transform; _vis.SetParent(transform, false); }
            int perlu = Mathf.Min(12, _kayu + (_ranting + _batu + _makanan + 2) / 3);
            while (_vis.childCount < perlu)
            {
                int i = _vis.childCount;
                var g = GameObject.CreatePrimitive(i < _kayu ? PrimitiveType.Capsule : PrimitiveType.Cube);
                if(Application.isPlaying)Destroy(g.GetComponent<Collider>());else DestroyImmediate(g.GetComponent<Collider>());
                g.transform.SetParent(_vis, false);
                g.transform.localPosition = new Vector3((i % 3 - 1) * 0.35f, 0.15f + (i / 3) * 0.25f, 0f);
                g.transform.localRotation = Quaternion.Euler(0f, 0f, i < _kayu ? 90f : 0f);
                g.transform.localScale = i < _kayu ? new Vector3(0.25f, 0.9f, 0.25f) : Vector3.one * 0.22f;
                if (_mat != null) g.GetComponent<MeshRenderer>().sharedMaterial = _mat;
            }
            for (int i = 0; i < _vis.childCount; i++) _vis.GetChild(i).gameObject.SetActive(i < perlu);
        }

        public string GetPromptText(GameObject actor) { return "AMBIL TUMPUKAN (kayu " + _kayu + ", ranting " + _ranting + ", batu " + _batu + ", makanan " + _makanan + ")"; }
        public bool CanInteract(GameObject actor) { return _kayu + _ranting + _batu + _makanan > 0; }

        public void Interact(GameObject actor)
        {
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv != null)
            {
                _ranting -= inv.Tambah(ItemID.Ranting, _ranting);
                _batu -= inv.Tambah(ItemID.Batu, _batu);
                _makanan -= inv.Tambah(ItemID.KelapaMuda, _makanan);
            }
            var bahu = GameServices.Get<ShoulderLogAndFreeformBuilder>();
            while (bahu != null && _kayu > 0 && bahu.Panggul(ShoulderLogAndFreeformBuilder.Muatan.Gelondong, _mat)) _kayu--;
            Gambar();
        }

        [System.Serializable] struct DataTumpukan { public int k, r, b, m; }
        public string CaptureJson() { return JsonUtility.ToJson(new DataTumpukan { k = _kayu, r = _ranting, b = _batu, m = _makanan }); }
        public void RestoreJson(string json) { if (string.IsNullOrEmpty(json)) return; var d = JsonUtility.FromJson<DataTumpukan>(json); _kayu = d.k; _ranting = d.r; _batu = d.b; _makanan = d.m; Gambar(); }
    }
}
