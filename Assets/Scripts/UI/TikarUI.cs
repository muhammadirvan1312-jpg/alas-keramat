using System.Collections.Generic;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    // Layar Tikar Pandan: ketuk item 3D -> lingkaran rakit; tombol RAKIT (resep cocok), MAKAN, PAKAI ZIRAH, BATAL, TUTUP. Label item mengikuti posisi 3D.
    public class TikarUI : MonoBehaviour, IPointerClickHandler
    {
        public const string ApiResep17 = "Butuh api unggun untuk menempa paku";

        [SerializeField] GameObject panel;
        [SerializeField] Text judul;
        [SerializeField] Text[] label = new Text[TikarPanggung.MaksTampil];
        [SerializeField] Button tombolRakit, tombolMakan, tombolPakai;
        [SerializeField] Text teksRakit;

        [SerializeField] Text teksHalaman;
        [SerializeField] Button tombolSenjata;
        bool _kotor;
        public void PasangHalaman(Text teks, Button senjata) { teksHalaman=teks; tombolSenjata=senjata; }
        TikarPanggung _p;
        TikarPandan3DInventory _inv;
        readonly List<ItemID> _tmp = new List<ItemID>();

        public void Pasang(GameObject p, Text j, Text[] l, Button r, Button m, Button z, Text tr) { panel = p; judul = j; label = l; tombolRakit = r; tombolMakan = m; tombolPakai = z; teksRakit = tr; }

        void Awake() { PengunciInput.Pasang(panel);  GameServices.Register(this); if (panel != null) panel.SetActive(false); }
        void OnEnable() { GameEvents.OnItemChanged += Berubah; }
        void OnDisable() { GameEvents.OnItemChanged -= Berubah; }
        void Berubah(ItemID id, int n) { _kotor=true; }
        void LateUpdate() { if (_kotor && _p != null && _p.Terbuka) { _kotor=false; _p.Segarkan(); Perbarui(); } }
        public void HalamanSebelum() { if(Siap()) { _p.PindahHalaman(-1); Perbarui(); } }
        public void HalamanBerikut() { if(Siap()) { _p.PindahHalaman(1); Perbarui(); } }
        public void TekanSenjata()
        {
            if(!Siap())return;
            var c=GameServices.Get<CombatAndLeftHandController>();
            if(c!=null && c.PakaiDariTas(Tunggal()))Tutup();
        }

        bool Siap()
        {
            if (_p == null) _p = GameServices.Get<TikarPanggung>();
            if (_inv == null) _inv = GameServices.Get<TikarPandan3DInventory>();
            return _p != null && _inv != null;
        }

        public void Buka()
        {
            if (!Siap()) return;
            bool buka = !_p.Terbuka;
            _p.Buka(buka);
            if (panel != null) panel.SetActive(buka);
            Perbarui();
        }

        public void Tutup() { if (Siap() && _p.Terbuka) Buka(); }

        public void OnPointerClick(PointerEventData e)
        {
            if (!Siap() || !_p.Terbuka) return;
            var id = _p.Ketuk(e.position);
            if (id != ItemID.None && judul != null) judul.text = ItemDatabase.Get(id).displayName + " - " + ItemDatabase.Get(id).description;
            Perbarui();
        }

        CraftingRecipe? Resep() { _tmp.Clear(); foreach (var i in _p.Pilihan) _tmp.Add(i); return TikarPanggung.CariResep(_tmp, _inv); }

        ItemID Tunggal() { if (_p.JumlahPilihan != 1) return ItemID.None; foreach (var i in _p.Pilihan) return i; return ItemID.None; }

        void Perbarui()
        {
            if (!Siap()) return;
            var r = _p.Terbuka ? Resep() : null;
            if (tombolRakit != null) tombolRakit.interactable = r.HasValue;
            if (teksRakit != null) teksRakit.text = r.HasValue ? "RAKIT: " + ItemDatabase.Get(r.Value.output.id).displayName.ToUpperInvariant() : "RAKIT";
            var t = _p.Terbuka ? Tunggal() : ItemID.None;
            if (tombolMakan != null) tombolMakan.interactable = t != ItemID.None && ItemDatabase.IsFood(t);
            if (tombolPakai != null) tombolPakai.interactable = t != ItemID.None && AndiVitalsAndSanity.Durabilitas(t) > 0f;
            if(teksHalaman!=null)teksHalaman.text="HALAMAN "+(_p.Halaman+1)+" / "+_p.JumlahHalaman;
            if(tombolSenjata!=null)tombolSenjata.interactable=t!=ItemID.None && (CombatAndLeftHandController.IndeksMelee(t)>=0 || CombatAndLeftHandController.IndeksJarak(t)>=0);
            TataLabel();
        }

        void TataLabel()
        {
            var cam = _p.Kamera;
            var rt = panel != null ? (RectTransform)panel.transform : null;
            for (int i = 0; i < label.Length; i++)
            {
                if (label[i] == null) continue;
                bool ada = _p.Terbuka && i < _p.JumlahBenda && cam != null && rt != null;
                label[i].enabled = ada;
                if (!ada) continue;
                var id = _p.IdBenda(i);
                label[i].text = ItemDatabase.Get(id).displayName + " x" + _inv.Jumlah(id);
                Vector2 lokal;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, cam.WorldToScreenPoint(_p.PosisiBenda(i) + Vector3.down * 0.06f), null, out lokal);
                label[i].rectTransform.anchoredPosition = lokal;
            }
        }

        public void TekanRakit()
        {
            if (!Siap()) return;
            var r = Resep();
            if (!r.HasValue) return;
            var v = GameServices.Get<AndiVitalsAndSanity>();
            if (r.Value.recipeId == 17 && (v == null || !v.DekatApi)) { GameEvents.RaiseToast(ApiResep17); return; }
            if (TikarPanggung.Rakit(r.Value, _inv)) { StatistikPemain.CatatRakit(); GameEvents.RaiseToast("Berhasil merakit " + ItemDatabase.Get(r.Value.output.id).displayName); }
            else GameEvents.RaiseToast("Gagal merakit: tas penuh");
            _p.BatalPilih(); _p.Segarkan(); Perbarui();
        }

        public void TekanMakan()
        {
            if (!Siap()) return;
            var t = Tunggal();
            var v = GameServices.Get<AndiVitalsAndSanity>();
            var rkObat = GameServices.Get<AlasKeramat.Player.ReaksiAndi>();
            if (rkObat != null && rkObat.Obati(t)) { _p.BatalPilih(); _p.Segarkan(); Perbarui(); return; } // T7c: perban/jamu/sirih untuk pendarahan
            if (t != ItemID.None && v != null && v.Konsumsi(t)) GameEvents.RaiseToast("Kamu mengonsumsi " + ItemDatabase.Get(t).displayName);
            _p.BatalPilih(); _p.Segarkan(); Perbarui();
        }

        public void TekanPakai()
        {
            if (!Siap()) return;
            var t = Tunggal();
            var v = GameServices.Get<AndiVitalsAndSanity>();
            if (t != ItemID.None && v != null) GameEvents.RaiseToast(v.PakaiZirah(t) ? "Zirah dipakai (" + v.JumlahSegmen + "/10)" : "Zirah sudah penuh");
            _p.BatalPilih(); _p.Segarkan(); Perbarui();
        }

        public void TekanBatal() { if (Siap()) { _p.BatalPilih(); Perbarui(); } }
    }
}
