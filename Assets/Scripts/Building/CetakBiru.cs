using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Cetak biru di dunia (07 §7): "TAMBAH <bahan>" mengisi satu jenis bahan yang dimiliki (kayu dari bahu dulu), selesai -> bangunan jadi.
    public class CetakBiru : MonoBehaviour, IInteractable
    {
        public BlueprintID Id;
        public float Bentang = 6f;
        public int[] Terisi = new int[0];
        public float HoldDuration { get { return 0f; } }
        public static readonly System.Collections.Generic.List<CetakBiru> Semua = new System.Collections.Generic.List<CetakBiru>();
        void OnEnable() { if (!Semua.Contains(this)) Semua.Add(this); }
        void OnDisable() { Semua.Remove(this); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void ResetSemua() { Semua.Clear(); }

        public static CetakBiru Buat(BlueprintID id, Vector3 pos, Quaternion rot, float bentang = 6f)
        {
            var d = GuidebookBlueprintsAndCooking.Ambil(id);
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "CetakBiru_" + id;
            go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
            go.transform.SetPositionAndRotation(pos + Vector3.up * 0.05f, rot);
            go.transform.localScale = new Vector3(d.footprint.x, 0.1f, d.footprint.y);
            go.GetComponent<MeshRenderer>().sharedMaterial = SafeMaterialFactory.GetCached("CetakBiru", new Color(0.31f, 0.66f, 1f), 0f, 0.1f);
            var c = go.AddComponent<CetakBiru>();
            c.Bentang = bentang; c.Id = id; c.Terisi = new int[d.cost.Length];
            return c;
        }

        int Kurang(int i) { var d = GuidebookBlueprintsAndCooking.Ambil(Id); return d.cost[i].count - Terisi[i]; }

        // Bahan berikutnya yang bisa ditambahkan dari tas/bahu; -1 bila tidak ada.
        int BahanBisa(out int punya)
        {
            punya = 0;
            var d = GuidebookBlueprintsAndCooking.Ambil(Id);
            var inv = GameServices.Get<TikarPandan3DInventory>();
            var bahu = GameServices.Get<ShoulderLogAndFreeformBuilder>();
            for (int i = 0; i < d.cost.Length; i++)
            {
                if (Kurang(i) <= 0) continue;
                var id = d.cost[i].id;
                int n = inv != null ? inv.Jumlah(id) : 0;
                if (id == ItemID.KainBatik && inv != null) n += inv.Jumlah(ItemID.KainKafan);
                if (bahu != null && ((id == ItemID.KayuGelondong && bahu.Bawa == ShoulderLogAndFreeformBuilder.Muatan.Gelondong) || (id == ItemID.PapanKayu && bahu.Bawa == ShoulderLogAndFreeformBuilder.Muatan.Papan))) n += bahu.Jumlah;
                if (n > 0) { punya = n; return i; }
            }
            return -1;
        }

        public string GetPromptText(GameObject actor)
        {
            int punya; int i = BahanBisa(out punya);
            var d = GuidebookBlueprintsAndCooking.Ambil(Id);
            if (i >= 0) return "ISI " + ItemDatabase.Get(d.cost[i].id).displayName.ToUpperInvariant() + " (" + Terisi[i] + "/" + d.cost[i].count + ")";
            int diisi = 0, perlu = 0;
            for (int k = 0; k < d.cost.Length; k++) { perlu += d.cost[k].count; if (k < Terisi.Length) diisi += Terisi[k]; }
            return "CETAK BIRU " + d.displayName.ToUpperInvariant() + " (" + diisi + "/" + perlu + ")";
        }

        public bool CanInteract(GameObject actor) { return Id != 0; }

        public void Interact(GameObject actor)
        {
            var d = GuidebookBlueprintsAndCooking.Ambil(Id);
            var inv = GameServices.Get<TikarPandan3DInventory>();
            var bahu = GameServices.Get<ShoulderLogAndFreeformBuilder>();
            int totalMasuk = 0;
            for (int i = 0; i < d.cost.Length; i++)
            {
                int perlu = Kurang(i);
                if (perlu <= 0) continue;
                var id = d.cost[i].id;
                int masuk = 0;
                var muatan = id == ItemID.PapanKayu ? ShoulderLogAndFreeformBuilder.Muatan.Papan : ShoulderLogAndFreeformBuilder.Muatan.Gelondong;
                if ((id == ItemID.KayuGelondong || id == ItemID.PapanKayu) && bahu != null)
                    while (masuk < perlu && bahu.Bawa == muatan && bahu.Jumlah > 0) { bahu.Lepaskan(); masuk++; }
                while (masuk < perlu && inv != null && inv.Kurangi(id, 1)) masuk++;
                if (id == ItemID.KainBatik)
                    while (masuk < perlu && inv != null && inv.Kurangi(ItemID.KainKafan, 1)) masuk++;
                if (masuk > 0) { Terisi[i] += masuk; totalMasuk += masuk; }
            }
            if (totalMasuk > 0) CekSelesai();
            if (this != null && gameObject != null)
            {
                var kurang = new System.Text.StringBuilder();
                for (int k = 0; k < d.cost.Length; k++)
                {
                    int rem = Kurang(k);
                    if (rem <= 0) continue;
                    if (kurang.Length > 0) kurang.Append(", ");
                    kurang.Append(rem).Append(" ").Append(ItemDatabase.Get(d.cost[k].id).displayName);
                }
                if (kurang.Length > 0) GameEvents.RaiseToast((totalMasuk > 0 ? "+" + totalMasuk + " bahan masuk. Kurang: " : "Kurang bahan: ") + kurang);
            }
        }

        // T8: dipakai Kang Deden (Selesaikan Bangunan / setor bahan). Mengembalikan jumlah yang terpakai.
        public int Kebutuhan(ItemID id)
        {
            var d = GuidebookBlueprintsAndCooking.Ambil(Id); int n = 0;
            for (int i = 0; i < d.cost.Length && i < Terisi.Length; i++) if (d.cost[i].id == id) n += Mathf.Max(0, Kurang(i));
            return n;
        }

        public int Isi(ItemID id, int n)
        {
            var d = GuidebookBlueprintsAndCooking.Ambil(Id); int pakai = 0;
            for (int i = 0; i < d.cost.Length && i < Terisi.Length && pakai < n; i++)
                if (d.cost[i].id == id) { int m = Mathf.Min(n - pakai, Mathf.Max(0, Kurang(i))); Terisi[i] += m; pakai += m; }
            if (pakai > 0) CekSelesai();
            return pakai;
        }

        void CekSelesai()
        {
            var d = GuidebookBlueprintsAndCooking.Ambil(Id);
            for (int k = 0; k < d.cost.Length; k++) if (Kurang(k) > 0) return;
            BangunanBlueprint.Bangun(Id, transform.position - Vector3.up * 0.05f, transform.rotation, Bentang);
            GameEvents.RaiseStructureBuilt(Id);
            GameEvents.RaiseToast(d.displayName + " selesai dibangun");
            Destroy(gameObject);
        }
    }
}
