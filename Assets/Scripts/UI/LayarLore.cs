using AlasKeramat.Core;
using UnityEngine;
using UnityEngine.UI;
namespace AlasKeramat.UI
{
    // Layar kertas lore (04 §6): tampil saat dokumen dibaca; Jurnal berisi semua lore yang sudah ditemukan (x/12).
    public class LayarLore : MonoBehaviour
    {
        [SerializeField] GameObject panelBaca, panelJurnal;
        [SerializeField] Text judul, isi, judulJurnal;
        [SerializeField] Text[] labelSlot = new Text[LoreLibrary.Jumlah];

        public void Pasang(GameObject baca, Text j, Text i, GameObject jurnal, Text jj, Text[] slot)
        { panelBaca = baca; judul = j; isi = i; panelJurnal = jurnal; judulJurnal = jj; labelSlot = slot; }

        public bool Terbuka { get { return (panelBaca != null && panelBaca.activeSelf) || (panelJurnal != null && panelJurnal.activeSelf); } }
        void Awake() { GameServices.Register(this); Tutup(); }
        void OnEnable() { GameEvents.OnLoreFound += Baca; }
        void OnDisable() { GameEvents.OnLoreFound -= Baca; }

        public void Baca(LoreID id)
        {
            if (panelBaca == null) return;
            if (panelJurnal != null) panelJurnal.SetActive(false);
            if (judul != null) judul.text = LoreLibrary.Judul(id);
            if (isi != null) isi.text = LoreLibrary.Isi(id);
            panelBaca.SetActive(true);
        }

        public void Tutup()
        {
            if (panelBaca != null) panelBaca.SetActive(false);
            if (panelJurnal != null) panelJurnal.SetActive(false);
        }

        public void BukaJurnal()
        {
            if (panelJurnal == null) return;
            if (panelBaca != null) panelBaca.SetActive(false);
            int mask = Mask();
            for (int i = 0; i < labelSlot.Length && i < LoreLibrary.Jumlah; i++)
                if (labelSlot[i] != null) labelSlot[i].text = (mask & (1 << i)) != 0 ? "#" + (i + 1) + "  " + LoreLibrary.Judul((LoreID)(i + 1)) : "#" + (i + 1) + "  ???";
            if (judulJurnal != null) judulJurnal.text = "JURNAL  -  Lore Ditemukan " + LoreLibrary.HitungDitemukan(mask) + "/" + LoreLibrary.Jumlah;
            panelJurnal.SetActive(true);
        }

        public void PilihJurnal(int i)
        {
            if ((Mask() & (1 << i)) == 0) return;
            Baca((LoreID)(i + 1));
        }

        static int Mask() { var q = GameServices.Get<QuestManager>(); return q != null ? q.LoreMask : 0; }
    }
}
