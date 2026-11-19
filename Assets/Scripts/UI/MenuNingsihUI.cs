using AlasKeramat.AI;
using AlasKeramat.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    // 08 §3.2 tahap 3: menu "BERINTERAKSI DENGAN NINGSIH" (7 aksi); aksi yang belum bisa tampak pudar.
    public class MenuNingsihUI : MonoBehaviour
    {
        public static readonly string[] Label = { "Berikan Busur Bambu", "Berikan Pistol Suar", "Ikuti Saya", "Tunggu di Sini", "Pulang ke Kemah", "Beri Makanan", "Berikan Pistol" };
        [SerializeField] GameObject panel;
        [SerializeField] Button[] tombol = new Button[7];
        [SerializeField] Text judul;
        NingsihVirginiaAI _n;

        public void Pasang(GameObject p, Button[] t, Text j) { panel = p; tombol = t; judul = j; }
        void Awake() { PengunciInput.Pasang(panel);  GameServices.Register(this); if (panel != null) panel.SetActive(false); }

        public void Buka(NingsihVirginiaAI n)
        {
            _n = n;
            if (panel == null || n == null) return;
            if (judul != null) judul.text = "NINGSIH  -  Kepercayaan " + Mathf.RoundToInt(n.Trust);
            for (int i = 0; i < tombol.Length; i++)
            {
                if (tombol[i] == null) continue;
                bool bisa = n.BisaAksi((NingsihVirginiaAI.AksiNingsih)i);
                var img = tombol[i].GetComponent<Image>();
                if (img != null) img.color = bisa ? new Color(0.45f, 0.18f, 0.22f, 0.95f) : new Color(0.25f, 0.25f, 0.25f, 0.6f);
            }
            panel.SetActive(true);
        }

        public void Pilih(int i) { if (_n != null) _n.Jalankan((NingsihVirginiaAI.AksiNingsih)i); Tutup(); }
        public void Tutup() { if (panel != null) panel.SetActive(false); }
    }
}
