using System.Collections.Generic;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.Building
{
    // 05 §5: api unggun. Bahan bakar maks 600 s (ranting +45, kayu +300), cahaya #FFB347 2,2 / 9 m berkedip 8 Hz ±12%,
    // hujan tanpa atap x2, badai tanpa atap padam 60 s; masak 4 tusuk: matang 25 s, gosong 40 s; bakar kemenyan kewarasan +20.
    public class ApiUnggun : MonoBehaviour, IInteractable
    {
        public const float BBMaks = 600f, BBRanting = 45f, BBKayu = 300f, BBAwal = 120f, Matang = 25f, Gosong = 40f, JarakHangat = 4f, PadamBadai = 60f;
        public const int Tusuk = 4;
        public static readonly List<ApiUnggun> Semua = new List<ApiUnggun>();

        [SerializeField] Light lampu;
        [SerializeField] Transform[] slotVisual = new Transform[Tusuk];
        public float BahanBakar = BBAwal;
        public bool Menyala { get; private set; }
        readonly ItemID[] _masak = new ItemID[Tusuk];
        readonly float[] _t = new float[Tusuk];
        float _badai;
        URPAtmosphereAndWeather _atmos;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() { Semua.Clear(); }
        void OnEnable() { Semua.Add(this); }
        void OnDisable() { Semua.Remove(this); }

        public void Pasang(Light l, Transform[] slot) { lampu = l; slotVisual = slot; }
        public float HoldDuration { get { return 0f; } }

        public static bool AdaApiDekat(Vector3 p, float r)
        {
            foreach (var a in Semua) if (a != null && a.Menyala && (a.transform.position - p).sqrMagnitude <= r * r) return true;
            return false;
        }

        public static ItemID HasilMasak(ItemID mentah) { return mentah == ItemID.IkanMentah ? ItemID.IkanMatang : mentah == ItemID.DagingMentah ? ItemID.DagingMatang : ItemID.None; }

        public static ItemID KeadaanMasak(ItemID mentah, float t) { return t < Matang ? mentah : t < Gosong ? HasilMasak(mentah) : ItemID.MakananGosong; }

        bool Beratap() { return Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.up, 6f, 1 << GameLayers.Structure, QueryTriggerInteraction.Ignore); }

        void Update()
        {
            if (_atmos == null) _atmos = GameServices.Get<URPAtmosphereAndWeather>();
            float dt = Time.deltaTime;
            if (Menyala)
            {
                bool atap = Beratap();
                var c = _atmos != null ? _atmos.Cuaca : WeatherState.Cerah;
                bool hujan = !atap && (c == WeatherState.Gerimis || c == WeatherState.HujanBadai);
                BahanBakar -= dt * (hujan ? 2f : 1f);
                _badai = !atap && c == WeatherState.HujanBadai ? _badai + dt : 0f;
                if (BahanBakar <= 0f || _badai >= PadamBadai) { Menyala = false; BahanBakar = Mathf.Max(0f, BahanBakar); GameEvents.RaiseToast("Api unggun padam"); }
                for (int i = 0; i < Tusuk; i++) if (_masak[i] != ItemID.None) _t[i] += dt;
            }
            if (lampu != null)
            {
                lampu.enabled = Menyala;
                if (Menyala) lampu.intensity = 2.2f * (1f + 0.12f * (Mathf.PerlinNoise(Time.time * 8f, 0.3f) * 2f - 1f));
            }
            for (int i = 0; i < Tusuk && i < slotVisual.Length; i++)
            {
                if (slotVisual[i] == null) continue;
                bool ada = _masak[i] != ItemID.None;
                if (slotVisual[i].gameObject.activeSelf != ada) slotVisual[i].gameObject.SetActive(ada);
                if (!ada) continue;
                var mr = slotVisual[i].GetComponent<MeshRenderer>();
                float u = _t[i];
                Color w = u < Matang ? Color.Lerp(new Color(0.85f, 0.5f, 0.5f), new Color(0.55f, 0.33f, 0.15f), u / Matang) : u < Gosong ? new Color(0.55f, 0.33f, 0.15f) : new Color(0.08f, 0.06f, 0.05f);
                mr.sharedMaterial = SafeMaterialFactory.GetCached("Masak" + Mathf.RoundToInt(w.r * 20f) + "_" + Mathf.RoundToInt(w.g * 20f), w, 0f, 0.3f);
            }
        }

        [System.Serializable] class IsiApiT6 { public float bahan, badai; public bool nyala; public ItemID[] masak; public float[] waktu; }
        public string CaptureIsi() { return JsonUtility.ToJson(new IsiApiT6 { bahan=BahanBakar, badai=_badai, nyala=Menyala, masak=_masak, waktu=_t }); }
        public void RestoreIsi(string json)
        {
            var d=JsonUtility.FromJson<IsiApiT6>(json); if(d==null)return;
            BahanBakar=d.bahan; _badai=d.badai; Menyala=d.nyala;
            for(int i=0;i<Tusuk;i++) { _masak[i]=d.masak[i]; _t[i]=d.waktu[i]; }
        }

        int SlotKosong() { for (int i = 0; i < Tusuk; i++) if (_masak[i] == ItemID.None) return i; return -1; }
        int SlotSiap() { for (int i = 0; i < Tusuk; i++) if (_masak[i] != ItemID.None && _t[i] >= Matang) return i; return -1; }

        ItemID MentahDimiliki(TikarPandan3DInventory inv)
        {
            if (inv == null) return ItemID.None;
            if (inv.Punya(ItemID.IkanMentah)) return ItemID.IkanMentah;
            if (inv.Punya(ItemID.DagingMentah)) return ItemID.DagingMentah;
            return ItemID.None;
        }

        public string GetPromptText(GameObject actor)
        {
            var inv = GameServices.Get<TikarPandan3DInventory>();
            var bahu = GameServices.Get<ShoulderLogAndFreeformBuilder>();
            int siap = SlotSiap();
            if (siap >= 0) return "AMBIL " + ItemDatabase.Get(KeadaanMasak(_masak[siap], _t[siap])).displayName.ToUpperInvariant();
            if (!Menyala) return BahanBakar > 0f ? "NYALAKAN API" : (inv != null && inv.Punya(ItemID.Ranting) ? "TAMBAH RANTING" : "");
            if (bahu != null && bahu.Bawa == ShoulderLogAndFreeformBuilder.Muatan.Gelondong && BahanBakar < BBMaks - BBKayu * 0.5f) return "TAMBAH KAYU KE API";
            var mentah = MentahDimiliki(inv);
            if (mentah != ItemID.None && SlotKosong() >= 0) return "MASAK " + ItemDatabase.Get(mentah).displayName.ToUpperInvariant();
            if (inv != null && inv.Punya(ItemID.Kemenyan)) return "BAKAR KEMENYAN";
            if (inv != null && inv.Punya(ItemID.Ranting) && BahanBakar < BBMaks - BBRanting) return "TAMBAH RANTING";
            return "";
        }

        public bool CanInteract(GameObject actor) { return GetPromptText(actor).Length > 0; }

        public void Interact(GameObject actor)
        {
            var inv = GameServices.Get<TikarPandan3DInventory>();
            var bahu = GameServices.Get<ShoulderLogAndFreeformBuilder>();
            string p = GetPromptText(actor);
            if (p.StartsWith("AMBIL"))
            {
                int s = SlotSiap();
                var hasil = KeadaanMasak(_masak[s], _t[s]);
                if (inv != null && inv.Tambah(hasil, 1) > 0) { _masak[s] = ItemID.None; _t[s] = 0f; GameEvents.RaiseToast("+1 " + ItemDatabase.Get(hasil).displayName); }
                return;
            }
            if (p == "NYALAKAN API")
            {
                if (inv == null || !inv.Punya(ItemID.KorekGas)) { GameEvents.RaiseToast("Butuh Korek Gas"); return; }
                Menyala = true; GameEvents.RaiseToast("Api menyala");
                return;
            }
            if (p == "TAMBAH KAYU KE API" && bahu != null) { bahu.Lepaskan(); BahanBakar = Mathf.Min(BBMaks, BahanBakar + BBKayu); return; }
            if (p == "TAMBAH RANTING" && inv != null && inv.Kurangi(ItemID.Ranting, 1)) { BahanBakar = Mathf.Min(BBMaks, BahanBakar + BBRanting); return; }
            if (p == "BAKAR KEMENYAN" && inv != null && inv.Kurangi(ItemID.Kemenyan, 1))
            {
                var v = GameServices.Get<AndiVitalsAndSanity>();
                if (v != null) v.UbahSanity(20f);
                GameEvents.RaiseToast("Asap kemenyan menenangkan pikiranmu");
                AlasKeramat.AI.ZonaPenangkal.TambahKemenyan(transform.position); // T7b: Pocong menjauh 20 m selama 60 s
                return;
            }
            if (p.StartsWith("MASAK"))
            {
                var m = MentahDimiliki(inv); int s = SlotKosong();
                if (m != ItemID.None && s >= 0 && inv.Kurangi(m, 1)) { _masak[s] = m; _t[s] = 0f; }
            }
        }
    }
}
