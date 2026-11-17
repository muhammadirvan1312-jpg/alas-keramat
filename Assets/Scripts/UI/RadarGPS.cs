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

        AndiFPSController _andi;
        TikarPandan3DInventory _inv;
        float _t;

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
            bool aktif = Aktif();
            if (isi != null && isi.activeSelf != aktif) isi.SetActive(aktif);
            if (!aktif) return;
            for (int i = 0; i < penanda.Length && i < posisi.Length; i++)
                if (penanda[i] != null) penanda[i].anchoredPosition = KeRadar(a, posisi[i], yaw);
        }
    }
}
