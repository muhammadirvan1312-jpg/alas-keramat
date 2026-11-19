using AlasKeramat.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    // 14 §1-2: layar peringatan 18+ saat pertama dibuka, lalu pilihan Gore (Mati / Sedang [disorot] / Penuh). Waktu dijeda selama tampil.
    public class LayarPeringatan : MonoBehaviour
    {
        [SerializeField] GameObject panelPeringatan, panelGore;
        [SerializeField] Image[] sorotGore = new Image[3];

        public void Pasang(GameObject peringatan, GameObject gore, Image[] sorot) { panelPeringatan = peringatan; panelGore = gore; sorotGore = sorot; }

        public static bool PerluTampil() { return PlayerPrefs.GetInt(GoreSystem.KunciPeringatan, 0) == 0; }

        void Start()
        {
            if (panelPeringatan == null || panelGore == null) return;
            bool tampil = PerluTampil();
            panelPeringatan.SetActive(tampil); panelGore.SetActive(false);
            if (tampil) Time.timeScale = 0f;
        }

        public void TekanLanjutkan()
        {
            if (panelPeringatan != null) panelPeringatan.SetActive(false);
            if (panelGore != null) panelGore.SetActive(true);
            Sorot((int)GoreSystem.Tingkat);
        }

        // Dipanggil tombol MATI (0), SEDANG (1), PENUH (2).
        public void PilihGore(int tingkat)
        {
            GoreSystem.Tingkat = (GoreLevel)Mathf.Clamp(tingkat, 0, 2);
            PlayerPrefs.SetInt(GoreSystem.KunciPeringatan, 1);
            PlayerPrefs.Save();
            if (panelGore != null) panelGore.SetActive(false);
            var menu=GameServices.Get<MenuScreens>();
            Time.timeScale = menu!=null && !menu.PermainanDimulai ? 0f : 1f;
            GameEvents.RaiseToast("Gore: " + (tingkat == 0 ? "Mati" : tingkat == 1 ? "Sedang" : "Penuh") + ". Bisa diubah di Pengaturan.");
        }

        void Sorot(int i)
        {
            if (sorotGore == null) return;
            for (int k = 0; k < sorotGore.Length; k++) if (sorotGore[k] != null) sorotGore[k].color = k == i ? new Color(0.71f, 0.27f, 0.18f, 0.95f) : new Color(0.2f, 0.2f, 0.2f, 0.85f);
        }
    }
}
