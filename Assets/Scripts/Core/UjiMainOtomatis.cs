#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using AlasKeramat.Building;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using AlasKeramat.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AlasKeramat.Core
{
    // Hanya masuk assembly Editor. Menggerakkan kontrol nyata di scene, bukan mengukur FPS HP.
    public class UjiMainOtomatis : MonoBehaviour
    {
        public static Action<bool, string[]> Selesai;
        public static Action<string> AmbilFoto;
        readonly List<string> _hasil = new List<string>();
        bool _lulus = true;
        void Cek(string nama, bool benar) { _hasil.Add((benar ? "[LULUS] " : "[GAGAL] ") + nama); _lulus &= benar; }

        IEnumerator Start()
        {
            for (int i = 0; i < 6; i++) yield return null;
            var menu = GameServices.Get<MenuScreens>();
            var andi = GameServices.Get<AndiFPSController>();
            var kontrol = GameServices.Get<TouchControls>();
            var tas = GameServices.Get<TikarPandan3DInventory>();
            var tikar = GameServices.Get<TikarPanggung>();
            var uiTas = UnityEngine.Object.FindFirstObjectByType<TikarUI>();
            Cek("Layanan pemain, tas dan kontrol aktif saat Play", menu != null && andi != null && kontrol != null && tas != null && uiTas != null && tikar != null);
            if (!_lulus) { Akhiri(); yield break; }
            menu.MulaiUjiEditor();
            for (int i = 0; i < 20; i++) yield return null;
            Cek("Menu awal dapat ditutup tanpa jeda tertinggal", !JedaDunia.Dijeda && Time.timeScale == 1f);
            string awalTas = tas.CaptureJson(), awalAndi = andi.CaptureJson();
            var joy = kontrol.GetComponentInChildren<ZonaJoystick>(true);
            var geser = kontrol.GetComponentInChildren<ZonaGeser>(true);
            Cek("Zona kedua jari tersambung", joy != null && geser != null);
            if (!_lulus) { Akhiri(); yield break; }
            var canvas = joy.GetComponentInParent<Canvas>();
            var rtJoy = (RectTransform)joy.transform;
            Vector2 sentuh = RectTransformUtility.WorldToScreenPoint(null, rtJoy.TransformPoint(new Vector3(260, 260, 0)));
            var kiri = new PointerEventData(EventSystem.current) { pointerId = 1001, position = sentuh };
            var kanan = new PointerEventData(EventSystem.current) { pointerId = 1002, delta = new Vector2(40, 0), position = new Vector2(Screen.width * 0.72f, Screen.height * 0.55f) };
            var targetJoy = TargetSentuh(kiri); var targetLihat = TargetSentuh(kanan);
            Cek("Raycast UI benar-benar mencapai analog dan area POV", targetJoy != null && targetJoy.GetComponentInParent<ZonaJoystick>() == joy && targetLihat != null && targetLihat.GetComponentInParent<ZonaGeser>() == geser);
            if (!_lulus) { Akhiri(); yield break; }
            ExecuteEvents.ExecuteHierarchy(targetJoy, kiri, ExecuteEvents.pointerDownHandler);
            kiri.position += Vector2.up * ZonaJoystick.RadiusGerak * 0.575f * canvas.scaleFactor; ExecuteEvents.Execute(joy.gameObject, kiri, ExecuteEvents.dragHandler);
            Vector3 pos = andi.transform.position; float yaw = andi.transform.eulerAngles.y;
            ExecuteEvents.ExecuteHierarchy(targetLihat, kanan, ExecuteEvents.pointerDownHandler); ExecuteEvents.Execute(geser.gameObject, kanan, ExecuteEvents.dragHandler);
            yield return new WaitForSecondsRealtime(1f);
            float gerak = Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(andi.transform.position.x, andi.transform.position.z));
            Cek("Analog setengah rentang memindahkan kapsul nyata (" + gerak.ToString("0.00") + " m)", gerak > 0.7f && gerak < 2.6f);
            Cek("Jari kanan memutar POV sambil jari kiri berjalan", Mathf.Abs(Mathf.DeltaAngle(yaw, andi.transform.eulerAngles.y)) > 1f && andi.SedangMembidikManual == false);
            geser.OnPointerUp(kanan);
            uiTas.Buka(); yield return null;
            float waktu = Time.time; pos = andi.transform.position;
            yield return new WaitForSecondsRealtime(0.25f);
            Cek("Tas menjeda dunia dan membuang input yang masih ditahan", tikar.Terbuka && JedaDunia.Dijeda && Mathf.Abs(Time.time - waktu) < 0.001f && Vector3.Distance(pos, andi.transform.position) < 0.001f && joy.Nilai == Vector2.zero);
            menu.ToggleJeda(); uiTas.Tutup(); yield return null;
            Cek("Menutup tas tidak membatalkan menu Jeda", menu.Dijeda && JedaDunia.Dijeda && Time.timeScale == 0f);
            menu.LanjutkanDariJeda(); yield return null;
            pos = andi.transform.position;
            yield return new WaitForSecondsRealtime(0.25f);
            Cek("Kembali bermain tanpa meluncur dari analog lama", !JedaDunia.Dijeda && Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(andi.transform.position.x, andi.transform.position.z)) < 0.03f);
            tas.Tambah(ItemID.PetaKertas, 1); tas.Tambah(ItemID.GPSHandset, 1);
            var peta = GameServices.Get<PetaSurveiUI>();
            if (peta != null)
            {
                peta.Toggle(); yield return null; Cek("Peta dapat dibuka dan menjeda", peta.Terbuka && JedaDunia.Dijeda);
                peta.Tutup(); yield return null; Cek("Peta dapat ditutup tanpa jeda bocor", !peta.Terbuka && !JedaDunia.Dijeda);
            }
            else Cek("Layar peta tersedia", false);
            string stok = tas.CaptureJson(), posisi = andi.CaptureJson();
            var snapshot = SaveSystem.KeJson(new ISaveable[] { tas, andi });
            tas.Tambah(ItemID.Ranting, 1); andi.Teleport(andi.transform.position + Vector3.right * 2f);
            SaveSystem.DariJson(snapshot, new ISaveable[] { tas, andi });
            Cek("Restore saat Play memulihkan tas dan posisi", tas.CaptureJson() == stok && andi.CaptureJson() == posisi);
            AmbilFoto("main_01_hud");
            menu.ToggleJeda(); menu.BukaPengaturan(); menu.BukaTabPengaturan(0); yield return null; AmbilFoto("main_02_kontrol");
            menu.BukaTabPengaturan(2); yield return null; AmbilFoto("main_03_audio");
            menu.TutupPengaturan(); menu.LanjutkanDariJeda();
            tas.RestoreJson(awalTas); andi.RestoreJson(awalAndi);
            Akhiri();
        }

        void Akhiri() { if (Selesai != null) Selesai(_lulus, _hasil.ToArray()); }

        static GameObject TargetSentuh(PointerEventData e)
        {
            var hasil = new List<RaycastResult>(); EventSystem.current.RaycastAll(e, hasil);
            if (hasil.Count == 0) return null;
            e.pointerPressRaycast = hasil[0]; e.pointerCurrentRaycast = hasil[0];
            return hasil[0].gameObject;
        }

    }
}
#endif
