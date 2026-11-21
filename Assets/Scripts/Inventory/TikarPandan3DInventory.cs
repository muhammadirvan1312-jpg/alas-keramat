using System;
using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Inventory
{
    // T3b: model data inventaris (05 §4: kapasitas per item = MaxCarry, bukan slot). Tampilan tikar 3D menyusul di T5.
    public class TikarPandan3DInventory : MonoBehaviour, ISaveable
    {
        [Serializable] class DataSimpan { public int[] id = new int[0]; public int[] n = new int[0]; }

        readonly Dictionary<ItemID, int> _isi = new Dictionary<ItemID, int>();

        public string SaveKey { get { return "inventaris"; } }
        public IEnumerable<KeyValuePair<ItemID, int>> Isi { get { return _isi; } }

        void Awake() { GameServices.Register(this); }

        public int Jumlah(ItemID id) { int n; return _isi.TryGetValue(id, out n) ? n : 0; }
        public bool Punya(ItemID id, int n = 1) { return Jumlah(id) >= n; }
        public int SisaRuang(ItemID id) { return Mathf.Max(0, ItemDatabase.MaxCarry(id) - Jumlah(id)); }

        // Mengembalikan jumlah yang benar-benar masuk (sisanya tidak muat).
        public int Tambah(ItemID id, int n)
        {
            if (id == ItemID.None || n <= 0 || !ItemDatabase.Has(id)) return 0;
            int masuk = Mathf.Min(n, SisaRuang(id));
            if (masuk <= 0) return 0;
            _isi[id] = Jumlah(id) + masuk;
            GameEvents.RaiseItemChanged(id, _isi[id]);
            return masuk;
        }

        public bool Kurangi(ItemID id, int n)
        {
            int ada = Jumlah(id);
            if (n <= 0 || ada < n) return false;
            if (ada == n) _isi.Remove(id); else _isi[id] = ada - n;
            GameEvents.RaiseItemChanged(id, ada - n);
            return true;
        }

        public string CaptureJson()
        {
            var d = new DataSimpan { id = new int[_isi.Count], n = new int[_isi.Count] };
            int i = 0;
            foreach (var kv in _isi) { d.id[i] = (int)kv.Key; d.n[i] = kv.Value; i++; }
            return JsonUtility.ToJson(d);
        }

        public void RestoreJson(string json)
        {
            try
            {
                var d = JsonUtility.FromJson<DataSimpan>(json);
                if (d == null || d.id == null || d.n == null || d.id.Length!=d.n.Length) throw new ArgumentException("Isi inventaris tidak valid");
                var berubah=new HashSet<ItemID>(_isi.Keys);
                _isi.Clear();
                for (int i = 0; i < d.id.Length && i < d.n.Length; i++)
                    if (d.n[i] > 0 && ItemDatabase.Has((ItemID)d.id[i])) _isi[(ItemID)d.id[i]] = Mathf.Min(d.n[i], ItemDatabase.MaxCarry((ItemID)d.id[i]));
                foreach(var id in _isi.Keys)berubah.Add(id);
                foreach(var id in berubah)GameEvents.RaiseItemChanged(id,Jumlah(id));
            }
            catch (Exception e) { Debug.LogError("[AK][TikarPandan3DInventory] Data simpan rusak: " + e.Message); throw; }
        }
    }
}
