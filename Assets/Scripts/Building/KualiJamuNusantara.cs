using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Kuali Jamu Nusantara: meracik 3 ramuan taktis (Jamu Kuat, Jamu Tolak Bala, Jamu Sembuh) di kuali herbal berapi.
    public class KualiJamuNusantara : MonoBehaviour, IInteractable
    {
        public enum ResepJamu { Kuat = 0, TolakBala = 1, Sembuh = 2 }
        public const float JedaRacik = 30f;

        [SerializeField] string lokasi = "Kampung";
        [SerializeField] Transform uapVisual;

        int _pilihan;
        float _siapPada = -99f;

        public float HoldDuration { get { return 0f; } }

        public void Pasang(string namaLokasi, Transform uap)
        {
            lokasi = string.IsNullOrEmpty(namaLokasi) ? "Kuali" : namaLokasi;
            uapVisual = uap;
        }

        public static string NamaResep(ResepJamu r)
        {
            if (r == ResepJamu.Kuat) return "Jamu Kuat (Stamina + XP Kekuatan)";
            if (r == ResepJamu.TolakBala) return "Jamu Tolak Bala (Sanity + Redam Sekte)";
            return "Jamu Sembuh (HP +45 & Hidrasi +30)";
        }

        public string GetPromptText(GameObject actor)
        {
            float sisa = _siapPada - Time.time;
            if (sisa > 0f) return "Kuali mendidih (" + Mathf.CeilToInt(sisa) + " dtk lagi)";
            return "RACIK " + NamaResep((ResepJamu)_pilihan);
        }

        public bool CanInteract(GameObject actor) { return Time.time >= _siapPada; }

        public void Interact(GameObject actor)
        {
            if (Time.time < _siapPada) return;
            var vital = GameServices.Get<AndiVitalsAndSanity>();
            var inv = GameServices.Get<TikarPandan3DInventory>();
            var lvl = GameServices.Get<LevelSurvivalAndi>();

            // Pakai herbal dari tas bila ada untuk bonus ganda, tetap bisa racik ramuan dasar kuali bila tas kosong
            bool pakaiHerbal = false;
            if (inv != null)
            {
                if (inv.Kurangi(ItemID.KunyitObat, 1) || inv.Kurangi(ItemID.JaheHutan, 1) || inv.Kurangi(ItemID.DaunSirih, 1))
                    pakaiHerbal = true;
            }
            float bonus = pakaiHerbal ? 1.35f : 1f;
            var resep = (ResepJamu)_pilihan;

            if (resep == ResepJamu.Kuat)
            {
                if (vital != null) vital.TambahStamina(65f * bonus);
                if (lvl != null) lvl.TambahXP(LevelSurvivalAndi.Atribut.Kekuatan, 16f * bonus);
                GameEvents.RaiseToast("Meminum Jamu Kuat" + (pakaiHerbal ? " Spesial" : "") + "! Stamina & kekuatan meningkat.");
            }
            else if (resep == ResepJamu.TolakBala)
            {
                if (vital != null) vital.UbahSanity(38f * bonus);
                var rep = GameServices.Get<ReputasiSekte>();
                if (rep != null) rep.TambahAncaman(-14f * bonus);
                GameEvents.RaiseToast("Meminum Jamu Tolak Bala! Batin tenang & aura sekte mereda.");
            }
            else
            {
                if (vital != null)
                {
                    vital.TambahHealth(45f * bonus);
                    vital.Minum(30f * bonus, false);
                }
                if (lvl != null) lvl.TambahXP(LevelSurvivalAndi.Atribut.Ketahanan, 16f * bonus);
                GameEvents.RaiseToast("Meminum Jamu Sembuh! Luka pulih & dahaga hilang.");
            }

            _pilihan = (_pilihan + 1) % 3;
            _siapPada = Time.time + JedaRacik;
            var gore = GameServices.Get<GoreSystem>();
            if (gore != null) gore.PercikPartikel(transform.position + Vector3.up * 0.7f, new Color(0.55f, 0.85f, 0.35f, 0.8f), 10, 0.08f, 1.4f, 0.8f);
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.PlayItemPickup();
        }

        void Update()
        {
            if (uapVisual == null || JedaDunia.Dijeda) return;
            float s = 0.85f + 0.25f * Mathf.Sin(Time.time * 3.4f);
            uapVisual.localScale = new Vector3(s, 0.7f + 0.3f * Mathf.Cos(Time.time * 2.6f), s);
        }
    }
}
