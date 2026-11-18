using System.Collections;
using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;
namespace AlasKeramat.AI
{
    // Ki Rogo di panggung Kubah Akar (04 §7.5, 06 §10): adegan pembuka 14 s, kebal (perisai akar), memanggil 2 Pocong Gua tiap 30 s di fase 2,
    // tenggelam ke akar setelah Genderuwo kalah; kepompong terbuka menampakkan Paku 3. Kesulitan Damai: Teka-teki Akar tanpa boss.
    public class KiRogoPanggung : MonoBehaviour
    {
        public const float RadiusPicu = 18f, JedaPanggil = 30f, LamaTenggelam = 3f;
        [SerializeField] BossGenderuwo boss;
        [SerializeField] TungkuObor[] tungku;
        [SerializeField] GameObject dindingAkar, kepompongTutup, isiPaku;
        [SerializeField] Transform[] titikPanggil;
        public bool TekaTekiAktif { get; private set; }
        public bool AdeganMulai { get; private set; }
        AndiFPSController _andi;
        float _tPanggil = JedaPanggil;
        int _urutan;
        bool _selesai;

        public void Pasang(BossGenderuwo b, TungkuObor[] t, GameObject dinding, GameObject tutup, GameObject paku, Transform[] panggil)
        { boss = b; tungku = t; dindingAkar = dinding; kepompongTutup = tutup; isiPaku = paku; titikPanggil = panggil; }

        public static bool UrutanBenar(int urutanSekarang, int simbol) { return simbol == urutanSekarang + 1; }

        void Start()
        {
            if (isiPaku != null) isiPaku.SetActive(false);
            var q = GameServices.Get<QuestManager>();
            if (q != null && q.Progres.bossKalah) Tamat(false);
        }

        void Update()
        {
            if (_selesai) return;
            var q = GameServices.Get<QuestManager>();
            if (q != null && q.Progres.bossKalah && !AdeganMulai) { Tamat(false); return; }
            if (_andi == null) { _andi = GameServices.Get<AndiFPSController>(); if (_andi == null) return; }
            if (!AdeganMulai && (_andi.transform.position - transform.position).sqrMagnitude < RadiusPicu * RadiusPicu)
            {
                AdeganMulai = true;
                StartCoroutine(Adegan());
            }
            if (boss != null && boss.IsAlive && boss.Fase >= 2 && boss.Aksi != BossGenderuwo.AksiBoss.Tidur)
            {
                _tPanggil -= Time.deltaTime;
                if (_tPanggil <= 0f) { _tPanggil = JedaPanggil; Panggil(); }
            }
        }

        IEnumerator Adegan()
        {
            if (tungku != null) for (int i = 0; i < tungku.Length; i++) { if (tungku[i] != null && EnemyDirector.Kesulitan != Difficulty.Damai) tungku[i].Nyalakan(); yield return new WaitForSeconds(0.6f); }
            yield return new WaitForSeconds(0.2f);
            GameEvents.RaiseSubtitle("Ki Rogo: Kau datang mengembalikan paku-paku itu, anak kota? Hutan ini sudah memilih tuannya.");
            if (EnemyDirector.Kesulitan == Difficulty.Damai)
            {
                TekaTekiAktif = true;
                yield return new WaitForSeconds(4f);
                GameEvents.RaiseSubtitle("Andi: (Simbol di pilar... I, II, III, IV. Nyalakan tungkunya berurutan.)");
                yield break;
            }
            yield return new WaitForSeconds(5f);
            if (dindingAkar != null) dindingAkar.SetActive(false);
            var a = GameServices.Get<ProceduralAudioSynth>();
            if (a != null) a.Play(ProceduralAudioSynth.KlipGuruh, transform.position, AudioCategory.Efek, 1f, 0.7f, 6);
            yield return new WaitForSeconds(4f);
            if (boss != null) boss.Bangunkan();
        }

        void Panggil()
        {
            var dir = GameServices.Get<EnemyDirector>();
            if (dir == null || titikPanggil == null) return;
            for (int i = 0; i < 2 && i < titikPanggil.Length; i++) if (titikPanggil[i] != null) dir.PanggilHantuDi(0, titikPanggil[(i + _urutan) % titikPanggil.Length].position);
            _urutan++;
            GameEvents.RaiseSubtitle("Ki Rogo menghentakkan tongkatnya. Sosok berkafan muncul dari akar.");
        }

        public void TungkuDinyalakan(TungkuObor t)
        {
            if (!TekaTekiAktif || _selesai) return;
            if (UrutanBenar(_urutan, t.Simbol))
            {
                t.Nyalakan(); _urutan++;
                if (_urutan >= 4) { TekaTekiAktif = false; BossKalah(); }
            }
            else
            {
                _urutan = 0;
                if (tungku != null) for (int i = 0; i < tungku.Length; i++) if (tungku[i] != null) tungku[i].Padamkan();
                GameEvents.RaiseToast("Api padam. Urutannya salah.");
            }
        }

        public void BossKalah() { if (!_selesai) StartCoroutine(Kalah()); }

        IEnumerator Kalah()
        {
            _selesai = true;
            GameEvents.RaiseSubtitle("Ki Rogo: Belum berakhir... Malam Darah akan menjemputmu.");
            Vector3 awal = transform.position;
            for (float t = 0f; t < LamaTenggelam; t += Time.deltaTime) { transform.position = awal - Vector3.up * (t / LamaTenggelam) * 2.5f; yield return null; }
            Tamat(true);
        }

        void Tamat(bool baru)
        {
            _selesai = true;
            if (kepompongTutup != null) kepompongTutup.SetActive(false);
            if (isiPaku != null) isiPaku.SetActive(true);
            if (dindingAkar != null) dindingAkar.SetActive(false);
            if (boss != null && !baru) boss.gameObject.SetActive(false);
            gameObject.SetActive(false);
            if (!baru) return;
            var q = GameServices.Get<QuestManager>(); if (q != null) q.TandaiBossKalah();
            GameEvents.RaiseToast("Kepompong akar terbuka. Paku Keramat ketiga!");
        }
    }
}
