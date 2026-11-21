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
        public static bool SedangMemuat { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetMuat() { SedangMemuat=false; }
        [Serializable] class Entri { public string kunci; public string json; }
        [Serializable] class Berkas { public int versi = 1; public string waktu; public List<Entri> data = new List<Entri>(); }

        public static string Path { get { return System.IO.Path.Combine(Application.persistentDataPath, NamaBerkas); } }
        public bool AdaSimpanan { get { return AdaSlot(SlotAktif) || BolehMuatLama(SlotAktif); } }

        void Awake() { GameServices.Register(this); }

        static List<ISaveable> Kumpulkan()
        {
            var hasil = new List<ISaveable>();
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (!(mb is AlasKeramat.AI.TumpukanKemah) && mb is ISaveable && !string.IsNullOrEmpty(((ISaveable)mb).SaveKey)) hasil.Add((ISaveable)mb);
            return hasil;
        }

        public static string KeJson(IEnumerable<ISaveable> daftar)
        {
            var b = new Berkas { waktu = DateTime.UtcNow.ToString("o") };
            var kunci = new HashSet<string>();
            foreach (var s in daftar)
            {
                if (s == null || string.IsNullOrEmpty(s.SaveKey) || !kunci.Add(s.SaveKey))
                    throw new InvalidDataException("Kunci simpan kosong/duplikat");
                string isi = s.CaptureJson();
                if (string.IsNullOrEmpty(isi)) throw new InvalidDataException("Snapshot kosong: " + s.SaveKey);
                b.data.Add(new Entri { kunci = s.SaveKey, json = isi });
            }
            b.data.Sort((x, y) => string.CompareOrdinal(x.kunci, y.kunci));
            return JsonUtility.ToJson(b);
        }

        public static void DariJson(string json, IEnumerable<ISaveable> daftar)
        {
            bool sebelumnya=SedangMemuat; SedangMemuat=true;
            try { PulihkanJson(json,daftar); }
            finally { SedangMemuat=sebelumnya; }
        }

        static void PulihkanJson(string json, IEnumerable<ISaveable> daftar)
        {
            var peta = BacaPeta(json);
            var urut = new List<ISaveable>(daftar);
            urut.Sort((a,c) => Urutan(a.SaveKey).CompareTo(Urutan(c.SaveKey)));
            // Ambil snapshot sebelum restore: kegagalan tidak boleh dilaporkan sebagai muat sukses.
            var semula = new Dictionary<ISaveable,string>();
            foreach (var s in urut) if (peta.ContainsKey(s.SaveKey)) semula.Add(s,s.CaptureJson());
            if (semula.Count == 0) throw new InvalidDataException("Tidak ada data permainan yang cocok");
            try
            {
                foreach (var s in urut) if (peta.TryGetValue(s.SaveKey, out string isi)) s.RestoreJson(isi);
            }
            catch
            {
                foreach (var s in urut)
                    if (semula.TryGetValue(s,out string isi))
                        try { s.RestoreJson(isi); } catch (Exception e) { Debug.LogError("[AK][SaveSystem] Pemulihan gagal: " + e.Message); }
                throw;
            }
        }

        static Dictionary<string,string> BacaPeta(string json)
        {
            var b = JsonUtility.FromJson<Berkas>(json);
            if (b == null || b.versi != 1 || b.data == null || b.data.Count == 0)
                throw new InvalidDataException("Berkas simpan tidak valid");
            var peta = new Dictionary<string,string>();
            foreach (var e in b.data)
            {
                if (e == null || string.IsNullOrEmpty(e.kunci) || string.IsNullOrEmpty(e.json))
                    throw new InvalidDataException("Entri simpan tidak valid");
                string kunci = e.kunci;
                // Versi lama menulis dua entri dunia. Pisahkan berdasarkan schema sebelum membuat peta.
                if (kunci == "dunia" && e.json.Contains("\"jenazah\"")) kunci = "peristiwa_dunia";
                if (peta.ContainsKey(kunci)) throw new InvalidDataException("Kunci simpan duplikat: " + kunci);
                peta.Add(kunci,e.json);
            }
            if(peta.TryGetValue("dunia",out string dunia))
            {
                var d=JsonUtility.FromJson<SimpanDunia.DuniaT6>(dunia);
                if(d!=null)
                {
                    if(d.tumpukan==null)d.tumpukan=new List<SimpanDunia.TumpukanSimpanT6>();
                    foreach(var entri in peta)
                    {
                        if(!entri.Key.StartsWith("tumpukan_",StringComparison.Ordinal))continue;
                        var xyz=entri.Key.Split('_');
                        if(xyz.Length==3 && int.TryParse(xyz[1],out int x) && int.TryParse(xyz[2],out int z))
                            d.tumpukan.Add(new SimpanDunia.TumpukanSimpanT6{posisi=new Vector3(x,WorldTerrainGenerator.SampleHeight(x,z),z),isi=entri.Value});
                    }
                    peta["dunia"]=JsonUtility.ToJson(d);
                }
            }
            return peta;
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
            foreach (var berkas in new[]{ps,ps+".bak",ps+".tmp"})
                if (File.Exists(berkas)) File.Delete(berkas);
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
                GameEvents.RaiseToast("Permainan tersimpan di Slot " + SlotAktif);
            }
            catch (Exception e) { Debug.LogError("[AK][SaveSystem] SimpanSlot gagal: " + e.Message); GameEvents.RaiseToast("Gagal menyimpan. Simpanan sebelumnya tetap tersedia."); }
        }
        public bool MuatSlot(int slot)
        {
            int tujuan = Mathf.Clamp(slot, 1, 3);
            try
            {
                string ps = PathSlot(tujuan);
                if (!File.Exists(ps)) return false;
                DariJson(File.ReadAllText(ps), Kumpulkan());
                SlotAktif = tujuan;
                GameEvents.RaiseToast("Slot " + SlotAktif + " dimuat");
                return true;
            }
            catch (Exception e) { Debug.LogError("[AK][SaveSystem] MuatSlot gagal: " + e.Message); GameEvents.RaiseToast("Simpanan gagal dimuat"); return false; }
        }
        public void Simpan() { SimpanSlot(SlotAktif); }

        static bool BolehMuatLama(int slot)
        {
            return slot == 1 && File.Exists(Path) && !AdaSlot(1) && !AdaSlot(2) && !AdaSlot(3);
        }
        public bool Muat()
        {
            if (AdaSlot(SlotAktif)) return MuatSlot(SlotAktif);
            if (!BolehMuatLama(SlotAktif)) return false;
            try { DariJson(File.ReadAllText(Path),Kumpulkan()); GameEvents.RaiseToast("Simpanan lama dimuat; simpan lagi untuk memakai Slot 1"); return true; }
            catch (Exception e) { Debug.LogError("[AK][SaveSystem] Muat gagal: " + e.Message); GameEvents.RaiseToast("Simpanan gagal dimuat"); return false; }
        }
    }
}
