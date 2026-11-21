using System.Text;
using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    // HUD Terpadu: Bar HP Boss Buto Ijo dinamis (muncul saat jarak <= 34 m), indikator Musim/Ancaman Sekte/Level,
    // serta kilatan live KOMBO x2 & FINISHER x3. Nol alokasi GC di Update.
    public class BannerBossDanStatusHUD : MonoBehaviour
    {
        public const float JarakTampilBoss = 34f;

        [SerializeField] Text teksStatus;
        [SerializeField] Text teksKombo;
        [SerializeField] GameObject panelBoss;
        [SerializeField] Text teksBoss;
        [SerializeField] Image isiHpBoss;

        readonly StringBuilder _sb = new StringBuilder(96);
        AndiFPSController _andi;
        TimeOfDayClock _jam;
        ReputasiSekte _rep;
        LevelSurvivalAndi _lvl;
        SistemKomboDanStagger _kombo;
        BossMiniButoIjo[] _daftarBoss;
        float _tCek, _tKombo;
        int _komboTerakhir;

        public void Pasang(Text status, Text kombo, GameObject bossPanel, Text bossNama, Image bossBar)
        {
            teksStatus = status;
            teksKombo = kombo;
            panelBoss = bossPanel;
            teksBoss = bossNama;
            isiHpBoss = bossBar;
        }

        public static float HitungRasioBoss(float hp, float hpMax)
        {
            return hpMax > 0f ? Mathf.Clamp01(hp / hpMax) : 0f;
        }

        public static string NamaMusimSingkat(SistemMusimDinamis.FaseMusim f)
        {
            if (f == SistemMusimDinamis.FaseMusim.Kemarau) return "Kemarau";
            if (f == SistemMusimDinamis.FaseMusim.Pancaroba) return "Pancaroba";
            return "Penghujan";
        }

        void Start()
        {
            _daftarBoss = Object.FindObjectsByType<BossMiniButoIjo>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (panelBoss != null) panelBoss.SetActive(false);
            if (teksKombo != null) teksKombo.enabled = false;
        }

        void Update()
        {
            if (JedaDunia.Dijeda) return;
            float dt = Time.deltaTime;

            // Kilatan cepat Kombo x2 / FINISHER x3
            if (_kombo == null) _kombo = GameServices.Get<SistemKomboDanStagger>();
            if (_kombo != null && teksKombo != null)
            {
                int k = _kombo.LangkahKombo;
                if (k != _komboTerakhir && k >= 2)
                {
                    _komboTerakhir = k;
                    _tKombo = 1.15f;
                    teksKombo.enabled = true;
                    teksKombo.text = k >= 3 ? "FINISHER x3!" : "KOMBO x2";
                    teksKombo.color = k >= 3 ? new Color(1f, 0.78f, 0.22f, 1f) : new Color(0.95f, 0.92f, 0.65f, 0.95f);
                }
                else if (k <= 1) _komboTerakhir = k;

                if (_tKombo > 0f)
                {
                    _tKombo -= dt;
                    if (_tKombo <= 0f) teksKombo.enabled = false;
                }
            }

            _tCek += dt;
            if (_tCek < 0.22f) return;
            _tCek = 0f;

            PerbaruiStatus();
            PerbaruiBarBoss();
        }

        void PerbaruiStatus()
        {
            if (teksStatus == null) return;
            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            if (_rep == null) _rep = GameServices.Get<ReputasiSekte>();
            if (_lvl == null) _lvl = GameServices.Get<LevelSurvivalAndi>();

            var musim = SistemMusimDinamis.FaseHari(_jam != null ? _jam.Hari : 1);
            _sb.Length = 0;
            _sb.Append(NamaMusimSingkat(musim));
            if (_lvl != null)
            {
                _sb.Append(" | Kuat Lv.").Append(_lvl.AmbilLevel(LevelSurvivalAndi.Atribut.Kekuatan));
                _sb.Append(" Lari Lv.").Append(_lvl.AmbilLevel(LevelSurvivalAndi.Atribut.StaminaLari));
            }
            if (_rep != null && _rep.Tingkat > ReputasiSekte.TingkatAncaman.Tenang)
            {
                _sb.Append(" | Sekte: ").Append(_rep.Tingkat);
            }
            teksStatus.text = _sb.ToString();
        }

        void PerbaruiBarBoss()
        {
            if (panelBoss == null || _daftarBoss == null || _daftarBoss.Length == 0) return;
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            if (_andi == null) { panelBoss.SetActive(false); return; }

            BossMiniButoIjo terdekat = null;
            float jMin = JarakTampilBoss * JarakTampilBoss;
            Vector3 pAndi = _andi.transform.position;
            for (int i = 0; i < _daftarBoss.Length; i++)
            {
                var b = _daftarBoss[i];
                if (b == null || !b.IsAlive) continue;
                float sq = (b.transform.position - pAndi).sqrMagnitude;
                if (sq < jMin) { jMin = sq; terdekat = b; }
            }

            if (terdekat == null)
            {
                if (panelBoss.activeSelf) panelBoss.SetActive(false);
                return;
            }
            if (!panelBoss.activeSelf) panelBoss.SetActive(true);
            float rasio = HitungRasioBoss(terdekat.HP, BossMiniButoIjo.HPMax);
            if (isiHpBoss != null) isiHpBoss.fillAmount = rasio;
            if (teksBoss != null)
            {
                _sb.Length = 0;
                _sb.Append(terdekat.NamaGelar).Append("  ").Append(Mathf.CeilToInt(terdekat.HP)).Append("/").Append(Mathf.RoundToInt(BossMiniButoIjo.HPMax));
                teksBoss.text = _sb.ToString();
            }
        }
    }
}
