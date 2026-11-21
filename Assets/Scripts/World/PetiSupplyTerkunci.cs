using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.World
{
    // Peti logistik ekspedisi & parasut terjatuh: tahan 1,8 detik untuk membongkar kunci & mendapat jarahan langka.
    public class PetiSupplyTerkunci : MonoBehaviour, IInteractable, ISaveable
    {
        [SerializeField] string idPeti = "supply_0";
        [SerializeField] string labelPeti = "Peti Logistik Ekspedisi";
        [SerializeField] Transform tutupPeti;
        [SerializeField] int tierLoot = 1;

        bool _terbuka;

        public bool SudahTerbuka { get { return _terbuka; } }
        public string SaveKey { get { return "peti_supply_" + idPeti; } }
        public float HoldDuration { get { return 1.8f; } }

        public void Pasang(string id, string label, Transform tutup, int tier)
        {
            idPeti = string.IsNullOrEmpty(id) ? "supply_0" : id;
            labelPeti = string.IsNullOrEmpty(label) ? "Peti Logistik" : label;
            tutupPeti = tutup;
            tierLoot = Mathf.Clamp(tier, 1, 3);
        }

        public string GetPromptText(GameObject actor)
        {
            return _terbuka ? "" : "BONGKAR " + labelPeti.ToUpperInvariant() + " (TAHAN)";
        }

        public bool CanInteract(GameObject actor) { return !_terbuka; }

        public void Interact(GameObject actor)
        {
            if (_terbuka) return;
            _terbuka = true;
            BukaVisual();
            BerikanHadiah();
        }

        void BukaVisual()
        {
            if (tutupPeti != null) tutupPeti.localRotation = Quaternion.Euler(-78f, 0f, 0f);
        }

        void BerikanHadiah()
        {
            Vector3 p = transform.position + Vector3.up * 0.7f;
            var inv = GameServices.Get<TikarPandan3DInventory>();
            TambahAtauJatuh(inv, p, ItemID.RansumDarurat, 1 + tierLoot / 2);
            TambahAtauJatuh(inv, p, ItemID.PerbanKain, 2);
            TambahAtauJatuh(inv, p, ItemID.LogamBekas, 2 + tierLoot);
            if (tierLoot >= 2) TambahAtauJatuh(inv, p, ItemID.AnakPanah, 6);
            if (tierLoot >= 3) TambahAtauJatuh(inv, p, ItemID.JamuEnergi, 2);
            else TambahAtauJatuh(inv, p, ItemID.Baterai, 2);

            var gore = GameServices.Get<GoreSystem>();
            if (gore != null) gore.PercikPartikel(p, new Color(1f, 0.78f, 0.32f, 0.9f), 10, 0.05f, 2.5f, 0.45f);
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.PlayItemPickup();
            var lvl = GameServices.Get<LevelSurvivalAndi>();
            if (lvl != null) lvl.TambahXP(LevelSurvivalAndi.Atribut.KeahlianRakit, 18f);
            GameEvents.RaiseToast(labelPeti + " terbuka! Logistik ekspedisi didapat.");
        }

        static void TambahAtauJatuh(TikarPandan3DInventory inv, Vector3 p, ItemID id, int n)
        {
            int masuk = inv != null ? inv.Tambah(id, n) : 0;
            if (masuk < n) JarahanTanah.Jatuhkan(p, id, n - masuk);
        }

        [System.Serializable] struct DataPeti { public bool b; }
        public string CaptureJson() { return JsonUtility.ToJson(new DataPeti { b = _terbuka }); }
        public void RestoreJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            _terbuka = JsonUtility.FromJson<DataPeti>(json).b;
            if (_terbuka) BukaVisual();
        }
    }
}
