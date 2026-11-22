using System.Text;
using AlasKeramat.AI;
using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.UI
{
    // Banner Penemuan Lokasi Sinematik + Altimeter Glider Bambu + Peringatan Arah Bahaya (Boss/Patroli Sekte).
    // Ringan & zero-GC untuk Android.
    public class BannerEksplorasiDanBahayaHUD : MonoBehaviour
    {
        public const float JarakBahaya = 24f;

        [SerializeField] GameObject panelTemu;
        [SerializeField] Text teksTemuJudul;
        [SerializeField] Text teksTemuSub;
        [SerializeField] GameObject panelGlider;
        [SerializeField] Text teksGlider;
        [SerializeField] Text teksBahaya;

        readonly StringBuilder _sb = new StringBuilder(80);
        JurnalEksplorasiPulau _jurnal;
        AndiFPSController _andi;
        BossMiniButoIjo[] _bosses;
        float _tBanner, _tCek;

        public void Pasang(GameObject pTemu, Text tJudul, Text tSub, GameObject pGlider, Text tGlider, Text tBahaya)
        {
            panelTemu = pTemu;
            teksTemuJudul = tJudul;
            teksTemuSub = tSub;
            panelGlider = pGlider;
            teksGlider = tGlider;
            teksBahaya = tBahaya;
        }

        public static string FormatArahBahaya(float sudutRelatifDeg)
        {
            if (sudutRelatifDeg < -28f) return "<< BAHAYA DI KIRI";
            if (sudutRelatifDeg > 28f) return "BAHAYA DI KANAN >>";
            return "! BAHAYA DI DEPAN !";
        }

        void Start()
        {
            if (panelTemu != null) panelTemu.SetActive(false);
            if (panelGlider != null) panelGlider.SetActive(false);
            if (teksBahaya != null) teksBahaya.enabled = false;
            _bosses = Object.FindObjectsByType<BossMiniButoIjo>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            _jurnal = GameServices.Get<JurnalEksplorasiPulau>();
            if (_jurnal != null) _jurnal.OnLokasiDitemukan += TampilkanPenemuan;
        }

        void OnDestroy()
        {
            if (_jurnal != null) _jurnal.OnLokasiDitemukan -= TampilkanPenemuan;
        }

        void TampilkanPenemuan(string nama, int ke, int total)
        {
            if (panelTemu == null) return;
            panelTemu.SetActive(true);
            _tBanner = 3.8f;
            if (teksTemuJudul != null) teksTemuJudul.text = "MENEMUKAN: " + nama.ToUpperInvariant();
            if (teksTemuSub != null) teksTemuSub.text = "Jurnal Pulau " + ke + "/" + total + "  |  +25 XP Ketahanan";
        }

        void Update()
        {
            if (JedaDunia.Dijeda) return;
            float dt = Time.deltaTime;

            if (_jurnal == null)
            {
                _jurnal = GameServices.Get<JurnalEksplorasiPulau>();
                if (_jurnal != null) _jurnal.OnLokasiDitemukan += TampilkanPenemuan;
            }

            if (_tBanner > 0f)
            {
                _tBanner -= dt;
                if (_tBanner <= 0f && panelTemu != null) panelTemu.SetActive(false);
            }

            // Altimeter Glider Bambu
            if (panelGlider != null)
            {
                bool terbang = GliderBambuAndi.SedangTerbangGlobal;
                if (panelGlider.activeSelf != terbang) panelGlider.SetActive(terbang);
                if (terbang && teksGlider != null)
                {
                    _sb.Length = 0;
                    _sb.Append("GLIDER BAMBU  |  Tinggi: ").Append(Mathf.RoundToInt(GliderBambuAndi.KetinggianTanahGlobal))
                       .Append(" m  |  Laju: ").Append(Mathf.RoundToInt(GliderBambuAndi.KecepatanGlobal)).Append(" m/s");
                    teksGlider.text = _sb.ToString();
                }
            }

            _tCek += dt;
            if (_tCek < 0.25f) return;
            _tCek = 0f;
            PerbaruiArahBahaya();
        }

        void PerbaruiArahBahaya()
        {
            if (teksBahaya == null || _bosses == null) return;
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            if (_andi == null) return;

            Vector3 pAndi = _andi.transform.position;
            Vector3 terdekat = Vector3.zero;
            float jMin = JarakBahaya * JarakBahaya;
            bool ada = false;

            for (int i = 0; i < _bosses.Length; i++)
            {
                var b = _bosses[i];
                if (b == null || !b.IsAlive) continue;
                float sq = (b.transform.position - pAndi).sqrMagnitude;
                if (sq < jMin) { jMin = sq; terdekat = b.transform.position; ada = true; }
            }

            if (!ada)
            {
                if (teksBahaya.enabled) teksBahaya.enabled = false;
                return;
            }
            Vector3 ke = terdekat - pAndi; ke.y = 0f;
            float sudut = Vector3.SignedAngle(_andi.transform.forward, ke, Vector3.up);
            teksBahaya.enabled = true;
            teksBahaya.text = FormatArahBahaya(sudut);
        }
    }
}
