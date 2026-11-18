using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AlasKeramat.Core
{
    // Simpan/muat (11 §8): mengumpulkan semua ISaveable (jam, vital, inventaris, dunia) ke satu JSON di persistentDataPath.
    // Dipanggil saat tidur di kasur/tenda dan dari menu. Pencarian ISaveable hanya saat simpan/muat (bukan per frame).
    public class SaveSystem : MonoBehaviour
    {
        public const string NamaBerkas = "alas_keramat_simpan.json";
        public static int SlotAktif = 1;
        [Serializable] class Entri { public string kunci; public string json; }
        [Serializable] class Berkas { public int versi = 1; public string waktu; public List<Entri> data = new List<Entri>(); }

        public static string Path { get { return System.IO.Path.Combine(Application.persistentDataPath, NamaBerkas); } }
        public bool AdaSimpanan { get { return File.Exists(Path); } }

        void Awake() { GameServices.Register(this); }

        static List<ISaveable> Kumpulkan()
        {
            var hasil = new List<ISaveable>();
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (mb is ISaveable && !string.IsNullOrEmpty(((ISaveable)mb).SaveKey)) hasil.Add((ISaveable)mb);
            return hasil;
        }

        public static string KeJson(IEnumerable<ISaveable> daftar)
        {
            var b = new Berkas { waktu = DateTime.UtcNow.ToString("o") };
            foreach (var s in daftar)
            {
                try { b.data.Add(new Entri { kunci = s.SaveKey, json = s.CaptureJson() }); }
                catch (Exception e) { Debug.LogError("[AK][SaveSystem] Gagal menyimpan " + s.SaveKey + ": " + e.Message); }
            }
            b.data.Sort((x, y) => string.CompareOrdinal(x.kunci, y.kunci));
            return JsonUtility.ToJson(b);
        }

        public static void DariJson(string json, IEnumerable<ISaveable> daftar)
        {
            var b = JsonUtility.FromJson<Berkas>(json);
            if (b == null || b.data == null) return;
            var peta = new Dictionary<string, string>();
            foreach (var e in b.data) peta[e.kunci] = e.json;
            var urut = new List<ISaveable>(daftar);
            urut.Sort((a,c) => Urutan(a.SaveKey).CompareTo(Urutan(c.SaveKey)));
            foreach (var s in urut)
            {
                string j;
                if (!peta.TryGetValue(s.SaveKey, out j)) continue;
                try { s.RestoreJson(j); } catch (Exception e) { Debug.LogError("[AK][SaveSystem] Gagal memuat " + s.SaveKey + ": " + e.Message); }
            }
        }

        static int Urutan(string kunci) { return kunci == "dunia" ? 0 : kunci == "posisi_andi" ? 2 : 1; }

        public static string PathSlot(int slot)
        {
            int s = Mathf.Clamp(slot, 1, 3);
            return System.IO.Path.Combine(Application.persistentDataPath, "alas_keramat_slot" + s + ".json");
        }
        public static bool AdaSlot(int slot) { return File.Exists(PathSlot(slot)); }
        public static void HapusSlot(int slot)
        {
            string ps = PathSlot(slot);
            if (File.Exists(ps)) File.Delete(ps);
        }
        public static string RingkasanSlot(int slot)
        {
            string ps = PathSlot(slot);
            if (!File.Exists(ps)) return "Kosong";
            try
            {
                var info = new FileInfo(ps);
                return "Tersimpan (" + info.LastWriteTime.ToString("yyyy-MM-dd HH:mm") + ")";
            }
            catch { return "Tersimpan"; }
        }
        public void SimpanSlot(int slot)
        {
            SlotAktif = Mathf.Clamp(slot, 1, 3);
            try
            {
                string isi = KeJson(Kumpulkan());
                string ps = PathSlot(SlotAktif);
                File.WriteAllText(ps + ".tmp", isi);
                if (File.Exists(ps)) File.Copy(ps, ps + ".bak", true);
                File.Copy(ps + ".tmp", ps, true); File.Delete(ps + ".tmp");
                File.WriteAllText(Path, isi);
                GameEvents.RaiseToast("Permainan tersimpan di Slot " + SlotAktif);
            }
            catch (Exception e) { Debug.LogError("[AK][SaveSystem] SimpanSlot gagal: " + e.Message); }
        }
        public bool MuatSlot(int slot)
        {
            SlotAktif = Mathf.Clamp(slot, 1, 3);
            try
            {
                string ps = PathSlot(SlotAktif);
                if (!File.Exists(ps)) ps = Path;
                if (!File.Exists(ps)) return false;
                DariJson(File.ReadAllText(ps), Kumpulkan());
                GameEvents.RaiseToast("Slot " + SlotAktif + " dimuat");
                return true;
            }
            catch (Exception e) { Debug.LogError("[AK][SaveSystem] MuatSlot gagal: " + e.Message); return false; }
        }
        public void Simpan()
        {
            try { string isi = KeJson(Kumpulkan());
                File.WriteAllText(Path + ".tmp", isi);
                if (File.Exists(Path)) File.Copy(Path, Path + ".bak", true);
                File.Copy(Path + ".tmp", Path, true); File.Delete(Path + ".tmp"); GameEvents.RaiseToast("Permainan tersimpan"); }
            catch (Exception e) { Debug.LogError("[AK][SaveSystem] Simpan gagal: " + e.Message); }
        }

        public bool Muat()
        {
            try
            {
                if (!File.Exists(Path)) return false;
                DariJson(File.ReadAllText(Path), Kumpulkan());
                GameEvents.RaiseToast("Permainan dimuat");
                return true;
            }
            catch (Exception e) { Debug.LogError("[AK][SaveSystem] Muat gagal: " + e.Message); return false; }
        }
    }
}
