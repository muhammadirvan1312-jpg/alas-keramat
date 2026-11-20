using AlasKeramat.AI;
using AlasKeramat.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    // 08 §2.2 & 09 §6: Buku Catatan - 15 perintah Kang Deden dalam 3 halaman (Umum, Kumpulkan, Kemah); perintah terkunci tampak pudar.
    public class BukuCatatanUI : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] Button[] tombol = new Button[15]; // urutan = KangDedenKelvinAI.Daftar
        KangDedenKelvinAI _deden;

        public void Pasang(GameObject p, Button[] t) { panel = p; tombol = t; }
        void Awake() { PengunciInput.Pasang(panel);  GameServices.Register(this); if (panel != null) panel.SetActive(false); }

        public bool Terbuka { get { return panel != null && panel.activeSelf; } }

        public void Buka(KangDedenKelvinAI d)
        {
            _deden = d;
            if (panel == null || d == null) return;
            for (int i = 0; i < tombol.Length && i < KangDedenKelvinAI.Daftar.Length; i++)
            {
                if (tombol[i] == null) continue;
                bool buka = d.Terbuka(KangDedenKelvinAI.Daftar[i].id);
                var img = tombol[i].GetComponent<Image>();
                if (img != null) img.color = buka ? new Color(0.36f, 0.27f, 0.17f, 0.95f) : new Color(0.25f, 0.25f, 0.25f, 0.6f);
                var t = tombol[i].GetComponentInChildren<Text>();
                if (t != null) t.text = KangDedenKelvinAI.Daftar[i].teks + (buka ? "" : " (terkunci)") + (d.Perintah == KangDedenKelvinAI.Daftar[i].id ? "  <" : "");
            }
            panel.SetActive(true);
        }

        public void Pilih(int i)
        {
            if (_deden != null && i >= 0 && i < KangDedenKelvinAI.Daftar.Length) _deden.BeriPerintah(KangDedenKelvinAI.Daftar[i].id);
            Tutup();
        }

        public void Tutup() { if (panel != null) panel.SetActive(false); }
    }
}
