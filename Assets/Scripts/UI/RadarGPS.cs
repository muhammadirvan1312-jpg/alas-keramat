using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.UI
{
    // Radar GPS (09 §2.2): Ø300, jangkauan 150 m, berputar mengikuti yaw kamera, penanda "U". Isi hanya tampil bila punya GPSHandset.
    public class RadarGPS : MonoBehaviour
    {
        public const float Jangkauan = 150f, JariJari = 135f, Interval = 0.1f;

        [SerializeField] RectTransform utara;
        [SerializeField] RectTransform[] penanda = new RectTransform[0];
        [SerializeField] Vector3[] posisi = new Vector3[0];
        [SerializeField] GameObject isi;
        [SerializeField] UnityEngine.UI.RawImage petaMini; // peta survei berputar di bawah penanda (minimap)
        public const float SisiDunia = 2000f, LebihRotasi = 1.42f;
        public void PasangPeta(UnityEngine.UI.RawImage r) { petaMini = r; }

        // Jendela UV peta survei berpusat di Andi (selebar 2 x Jangkauan, diperbesar agar sudut tetap tertutup saat berputar).
        public static Rect JendelaUV(Vector3 a)
        {
            float w = Jangkauan * 2f * LebihRotasi / SisiDunia;
            return new Rect((a.x + SisiDunia * 0.5f) / SisiDunia - w * 0.5f, (a.z + SisiDunia * 0.5f) / SisiDunia - w * 0.5f, w, w);
        }

        AndiFPSController _andi;
        TikarPandan3DInventory _inv;
        AlasKeramat.AI.KangDedenKelvinAI _deden;
        AlasKeramat.AI.NingsihVirginiaAI _ningsih;
        RectTransform _blipDeden, _blipNingsih;
        float _t;

        public static bool TampilBlipTeman(bool aktifGPS, bool hidup, bool tampakAtauGabung)
        {
            return aktifGPS && hidup && tampakAtauGabung;
        }

        RectTransform BuatBlipTeman(string nama, Color warna)
        {
            if (isi == null) return null;
            var go = new GameObject(nama);
            go.transform.SetParent(isi.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(18f, 18f);
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = warna; img.raycastTarget = false;
            return rt;
        }

        public void Pasang(RectTransform u, RectTransform[] p, Vector3[] pos, GameObject wadahIsi)
        {
            utara = u; penanda = p; posisi = pos; isi = wadahIsi;
        }

        public bool Aktif()
        {
            if (_inv == null) _inv = GameServices.Get<TikarPandan3DInventory>();
            return _inv != null && _inv.Punya(ItemID.GPSHandset);
        }

        // Posisi penanda di radar (piksel), dibatasi tepi lingkaran.
        public static Vector2 KeRadar(Vector3 dari, Vector3 ke, float yawDerajat)
        {
            Vector2 d = new Vector2(ke.x - dari.x, ke.z - dari.z);
            float r = yawDerajat * Mathf.Deg2Rad;
            Vector2 p = new Vector2(d.x * Mathf.Cos(r) - d.y * Mathf.Sin(r), d.x * Mathf.Sin(r) + d.y * Mathf.Cos(r));
            p *= JariJari / Jangkauan;
            return Vector2.ClampMagnitude(p, JariJari);
        }

        void Update()
        {
            _t += Time.deltaTime;
            if (_t < Interval) return;
            _t = 0f;
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            if (_andi == null) return;
            Vector3 a = _andi.transform.position;
            float yaw = _andi.transform.eulerAngles.y;
            if (utara != null) utara.anchoredPosition = KeRadar(a, a + Vector3.forward * Jangkauan * 2f, yaw);
            if (petaMini != null)
            {
                bool on = Aktif() && !WorldTerrainGenerator.DiBawahTanah(a.y);
                if (petaMini.enabled != on) petaMini.enabled = on;
                if (on) { petaMini.uvRect = JendelaUV(a); petaMini.rectTransform.localEulerAngles = new Vector3(0f, 0f, yaw); }
            }
            bool aktif = Aktif();
            if (isi != null && isi.activeSelf != aktif) isi.SetActive(aktif);
            if (!aktif) return;
            for (int i = 0; i < penanda.Length && i < posisi.Length; i++)
                if (penanda[i] != null) penanda[i].anchoredPosition = KeRadar(a, posisi[i], yaw);
            if (_deden == null) _deden = GameServices.Get<AlasKeramat.AI.KangDedenKelvinAI>();
            if (_ningsih == null) _ningsih = GameServices.Get<AlasKeramat.AI.NingsihVirginiaAI>();
            bool tampilD = _deden != null && TampilBlipTeman(aktif, _deden.IsAlive, _deden.Bergabung);
            if (tampilD && _blipDeden == null) _blipDeden = BuatBlipTeman("BlipDeden", new Color(0.22f, 0.72f, 1f, 0.95f));
            if (_blipDeden != null)
            {
                if (_blipDeden.gameObject.activeSelf != tampilD) _blipDeden.gameObject.SetActive(tampilD);
                if (tampilD) _blipDeden.anchoredPosition = KeRadar(a, _deden.transform.position, yaw);
            }
            bool tampilN = _ningsih != null && TampilBlipTeman(aktif, _ningsih.IsAlive, _ningsih.Tampak);
            if (tampilN && _blipNingsih == null) _blipNingsih = BuatBlipTeman("BlipNingsih", new Color(1f, 0.48f, 0.72f, 0.95f));
            if (_blipNingsih != null)
            {
                if (_blipNingsih.gameObject.activeSelf != tampilN) _blipNingsih.gameObject.SetActive(tampilN);
                if (tampilN) _blipNingsih.anchoredPosition = KeRadar(a, _ningsih.transform.position, yaw);
            }
        }
    }
}
