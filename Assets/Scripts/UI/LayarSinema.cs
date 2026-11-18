using System.Collections;
using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace AlasKeramat.UI
{
    // Sinematik layar penuh (04 §7): intro "Kabut Merah", Pilihan Takdir, Ending A/B, Layar Statistik (04 §7.10).
    // Waktu nyata (timeScale 0 saat adegan); tombol LEWATI muncul setelah 3 detik.
    public class LayarSinema : MonoBehaviour
    {
        public const float LajuKetik = 35f, LewatiSetelah = 3f, LamaEnding = 9f;
        public const string TeksIntro = "16 November - Tim Survei Geologi Andi Pratama & Teknisi Deden Suryana memasuki zona larangan terbang Alas Keramat untuk mencari tim peneliti yang hilang. Tiba-tiba kabut merah membumbung dari tajuk Pohon Beringin Agung, melumpuhkan instrumen kabin...";
        public const string KartuMisiIntro = "Ambil Koper Darurat, Tolong Kang Deden, dan Bangun Saung Sebelum Maghrib.";

        [SerializeField] Image hitam;
        [SerializeField] Text teksBesar, teksMerah, teksStat;
        [SerializeField] GameObject tombolLewati, panelPilihan, panelStat, tombolMulaiBaru;
        [SerializeField] Text labelLanjut;

        public bool Berjalan { get; private set; }
        int _ending;
        bool _lewati;

        public void Pasang(Image h, Text besar, Text merah, Text stat, GameObject lewati, GameObject pilihan, GameObject pStat, GameObject mulaiBaru, Text lanjut)
        { hitam = h; teksBesar = besar; teksMerah = merah; teksStat = stat; tombolLewati = lewati; panelPilihan = pilihan; panelStat = pStat; tombolMulaiBaru = mulaiBaru; labelLanjut = lanjut; }

        void Awake() { GameServices.Register(this); Bersih(); }
        void OnDisable() { if (Berjalan) Time.timeScale = 1f; }

        IEnumerator Start()
        {
            yield return null; yield return null; // tunggu layanan & muat
            var q = GameServices.Get<QuestManager>();
            if (q != null && !q.Progres.intro) StartCoroutine(Intro());
        }

        void Bersih()
        {
            if (hitam != null) { hitam.color = new Color(0f, 0f, 0f, 0f); hitam.enabled = false; }
            if (teksBesar != null) teksBesar.text = "";
            if (teksMerah != null) teksMerah.text = "";
            if (tombolLewati != null) tombolLewati.SetActive(false);
            if (panelPilihan != null) panelPilihan.SetActive(false);
            if (panelStat != null) panelStat.SetActive(false);
        }

        void Gelap(float a) { if (hitam == null) return; hitam.enabled = a > 0.001f; hitam.color = new Color(0f, 0f, 0f, a); }
        void Bunyi(string klip, float pitch) { var a = GameServices.Get<ProceduralAudioSynth>(); if (a != null) a.Play(klip, null, AudioCategory.Efek, 1f, pitch, 9); }

        // Menunggu waktu nyata; berhenti lebih awal bila LEWATI ditekan.
        IEnumerator Tunggu(float detik) { for (float t = 0f; t < detik && !_lewati; t += Time.unscaledDeltaTime) yield return null; }

        // ---------- 04 §7.1 Intro ----------
        IEnumerator Intro()
        {
            Berjalan = true; _lewati = false; Time.timeScale = 0f;
            Gelap(1f);
            Bunyi(ProceduralAudioSynth.KlipPesawat, 0.8f);
            float mulai = Time.unscaledTime;
            StartCoroutine(TombolLewatiNanti());
            yield return Tunggu(1f);
            for (int i = 1; i <= TeksIntro.Length && !_lewati; i++)
            {
                if (teksBesar != null) teksBesar.text = TeksIntro.Substring(0, i);
                yield return Tunggu(1f / LajuKetik);
            }
            yield return Tunggu(Mathf.Max(0f, 12f - (Time.unscaledTime - mulai)));
            if (!_lewati)
            {
                Bunyi(ProceduralAudioSynth.KlipGuruh, 1.3f); Bunyi(ProceduralAudioSynth.KlipKentongan, 1.8f);
                for (int k = 0; k < 8 && !_lewati; k++) { if (teksMerah != null) teksMerah.text = k % 2 == 0 ? "PERINGATAN: INSTRUMEN MATI" : ""; Gelap(k % 2 == 0 ? 0.85f : 1f); yield return Tunggu(0.5f); }
                Bunyi(ProceduralAudioSynth.KlipGuruh, 0.6f);
                if (teksBesar != null) teksBesar.text = ""; if (teksMerah != null) teksMerah.text = "";
                Gelap(1f);
                yield return Tunggu(3f);
            }
            Time.timeScale = 1f;
            if (tombolLewati != null) tombolLewati.SetActive(false);
            if (teksBesar != null) teksBesar.text = ""; if (teksMerah != null) teksMerah.text = "";
            var q = GameServices.Get<QuestManager>(); if (q != null) q.Progres.intro = true;
            // Membuka mata: dua kedipan lalu terang.
            float[] kedip = { 1f, 0.45f, 0.9f, 0.25f, 0.7f, 0f };
            for (int i = 0; i + 1 < kedip.Length; i++) for (float t = 0f; t < 0.5f; t += Time.unscaledDeltaTime) { Gelap(Mathf.Lerp(kedip[i], kedip[i + 1], t / 0.5f)); yield return null; }
            Gelap(0f);
            Berjalan = false;
            yield return new WaitForSeconds(_lewati ? 0.5f : 4f);
            GameEvents.RaiseSubtitle("Andi: (Kang Deden... telinganya. Dia tidak bisa mendengarku.)");
            yield return new WaitForSeconds(_lewati ? 1f : 3f);
            GameEvents.RaiseToast(KartuMisiIntro);
        }

        IEnumerator TombolLewatiNanti()
        {
            for (float t = 0f; t < LewatiSetelah; t += Time.unscaledDeltaTime) yield return null;
            if (Berjalan && tombolLewati != null) tombolLewati.SetActive(true);
        }

        public void Lewati() { _lewati = true; if (tombolLewati != null) tombolLewati.SetActive(false); }

        // ---------- 04 §7.7 Pilihan Takdir ----------
        public void TampilPilihan()
        {
            if (panelPilihan == null || Berjalan) return;
            Berjalan = true; Time.timeScale = 0f;
            Gelap(0.6f);
            panelPilihan.SetActive(true);
        }
        public void PilihA() { Pilih(1); }
        public void PilihB() { Pilih(2); }
        void Pilih(int e)
        {
            if (panelPilihan != null) panelPilihan.SetActive(false);
            _ending = e;
            var q = GameServices.Get<QuestManager>(); if (q != null) q.TandaiEnding(e);
            StartCoroutine(Ending());
        }

        public static string TeksEnding(int e, bool ningsihIkut)
        {
            if (e == 1)
                return (ningsihIkut ? "Andi merangkul bahu Kang Deden dan mengulurkan tangan kepada Ningsih. Mereka bertiga naik." : "Andi merangkul bahu Kang Deden. Sekali lagi ia menatap lembah bambu, untuk gadis berkebaya yang tak sempat ia kenal.")
                    + " Helikopter lepas landas saat matahari terbit, dan lampu mercusuar menyala hijau.\n\nAlas Keramat kembali sunyi. Di bawah akar Beringin Agung, tiga paku kembali tertidur - dan kali ini, ada yang tahu cara menjaganya.";
            return "Andi menyerahkan dokumen bukti kepada Kapten Hendra. Pilot memberi hormat dan terbang. Kang Deden mengacungkan jempol"
                + (ningsihIkut ? ", dan Ningsih tersenyum dengan bunga kantil di telinga." : ". Di tepi hutan, bayangan berkebaya mengawasi dari kejauhan.")
                + "\n\nSebagian orang pulang. Sebagian lagi memilih menjadi penjaga.";
        }

        IEnumerator Ending()
        {
            Berjalan = true; Time.timeScale = 0f; _lewati = false;
            for (float t = 0f; t < 1.5f; t += Time.unscaledDeltaTime) { Gelap(Mathf.Lerp(0.6f, 1f, t / 1.5f)); yield return null; }
            Gelap(1f);
            Bunyi(ProceduralAudioSynth.KlipGamelanJauh, 1f);
            var n = GameServices.Get<NingsihVirginiaAI>();
            bool ikut = n != null && n.Trust >= QuestManager.TrustM06;
            if (teksBesar != null) teksBesar.text = TeksEnding(_ending, ikut);
            if (tombolLewati != null) tombolLewati.SetActive(true);
            yield return Tunggu(LamaEnding);
            if (tombolLewati != null) tombolLewati.SetActive(false);
            if (teksBesar != null) teksBesar.text = "";
            var s = GameServices.Get<StatistikPemain>();
            if (teksStat != null) teksStat.text = s != null ? s.Teks() : "";
            if (labelLanjut != null) labelLanjut.text = _ending == 1 ? "LANJUT JELAJAH" : "JAGA ALAS KERAMAT";
            if (tombolMulaiBaru != null) tombolMulaiBaru.SetActive(_ending == 1);
            if (panelStat != null) panelStat.SetActive(true);
        }

        // Kembali ke dunia. Ending B: Mode Sandbox (tanpa Malam Darah & hantu) + Golok Api Pusaka (04 §7.9).
        public void Lanjut()
        {
            if (panelStat != null) panelStat.SetActive(false);
            Gelap(0f); Time.timeScale = 1f; Berjalan = false;
            var heli = GameServices.Get<HeliSAR>();
            if (heli != null) heli.Terbang();
            if (_ending != 2) return;
            EnemyDirector.TanpaMalamDarah = true;
            for (int i = HantuAI.Semua.Count - 1; i >= 0; i--) if (HantuAI.Semua[i] != null && HantuAI.Semua[i].IsAlive) HantuAI.Semua[i].TakeDamage(new DamageInfo { amount = 99999f, type = DamageType.Gaib, direction = Vector3.up });
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv != null) inv.Tambah(ItemID.GolokApiPusaka, 1);
            GameEvents.RaiseToast("Mode Sandbox terbuka. Golok Api Pusaka didapat.");
        }

        public void MulaiBaru()
        {
            Time.timeScale = 1f;
            try { if (System.IO.File.Exists(SaveSystem.Path)) System.IO.File.Delete(SaveSystem.Path); }
            catch (System.Exception e) { Debug.LogError("[AK][LayarSinema] Gagal menghapus simpanan: " + e.Message); }
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
