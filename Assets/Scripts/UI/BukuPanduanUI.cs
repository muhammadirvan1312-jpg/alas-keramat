using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.UI
{
    // Buku Panduan (07 §7): tombol BUKU (bila punya Buku Panduan) membuka daftar 14 blueprint per kategori; pilih -> mode penempatan
    // dengan tombol TANDAI / PUTAR / BATAL.
    public class BukuPanduanUI : MonoBehaviour
    {
        [SerializeField] GameObject panel, panelTempat, tombolBuku;
        GuidebookBlueprintsAndCooking _g;
        TikarPandan3DInventory _inv;
        float _t;

        public void Pasang(GameObject p, GameObject tempat, GameObject buku) { panel = p; panelTempat = tempat; tombolBuku = buku; }

        void Awake() { PengunciInput.Pasang(panel);  if (panel != null) panel.SetActive(false); if (panelTempat != null) panelTempat.SetActive(false); }

        bool Siap() { if (_g == null) _g = GameServices.Get<GuidebookBlueprintsAndCooking>(); return _g != null; }

        void Update()
        {
            _t += Time.deltaTime;
            if (_t < 0.3f) return;
            _t = 0f;
            if (_inv == null) _inv = GameServices.Get<TikarPandan3DInventory>();
            bool punya = _inv != null && _inv.Punya(ItemID.BukuPanduan);
            if (tombolBuku != null && tombolBuku.activeSelf != punya) tombolBuku.SetActive(punya);
            bool tempat = Siap() && _g.Menempatkan;
            if (panelTempat != null && panelTempat.activeSelf != tempat) panelTempat.SetActive(tempat);
        }

        public void Buka() { if (panel != null) panel.SetActive(!panel.activeSelf); }

        public void Pilih(int indeks)
        {
            var semua = GuidebookBlueprintsAndCooking.AmbilSemua();
            if (!Siap() || indeks < 0 || indeks >= semua.Length) return;
            _g.Mulai(semua[indeks].id);
            if (panel != null) panel.SetActive(false);
            if (panelTempat != null) panelTempat.SetActive(true);
        }

        public void Tandai() { if (Siap()) _g.Tandai(); }
        public void Putar() { if (Siap()) _g.Putar(); }
        public void Batal() { if (Siap()) _g.Batal(); }
    }
}
