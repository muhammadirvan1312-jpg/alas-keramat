using System;
using System.Collections.Generic;
using AlasKeramat.Building;
using AlasKeramat.AI;
using UnityEngine;
namespace AlasKeramat.Core
{
    // Satu pemilik snapshot dunia: hapus generasi lama dahulu, buat objek, lalu pulihkan graf dan isi.
    public class SimpanDunia : MonoBehaviour, ISaveable
    {
        [Serializable] public class PotonganT6
        {
            public string id; public BagianStruktur.Tipe jenis; public bool tanah; public float hp, panjang;
            public Vector3 posisi, skala; public Quaternion rotasi; public Color warna; public int serut, pukulCelah;
            public string[] sambungan;
        }
        [Serializable] public class BangunanT6
        {
            public float bentang; public string id; public BlueprintID jenis; public Vector3 posisi; public Quaternion rotasi;
            public string[] fungsi, api, panci;
        }
        [Serializable] public class CetakanT6 { public float bentang; public BlueprintID jenis; public Vector3 posisi; public Quaternion rotasi; public int[] isi; }
        [Serializable] public class KayuSimpanT6 { public string id; public bool papan; public Vector3 posisi, sumbu; public Color warna; }
        [Serializable] public class TumpukanSimpanT6 { public Vector3 posisi; public string isi; }
        [Serializable] public class DuniaT6
        {
            public List<TumpukanSimpanT6> tumpukan = new List<TumpukanSimpanT6>();
            public List<KayuSimpanT6> kayu = new List<KayuSimpanT6>();
            public List<PotonganT6> struktur = new List<PotonganT6>();
            public List<BangunanT6> bangunan = new List<BangunanT6>();
            public List<CetakanT6> cetakan = new List<CetakanT6>();
        }
        public string SaveKey { get { return "dunia"; } }
        void Awake() { GameServices.Register(this); }
        public string CaptureJson()
        {
            var d = new DuniaT6();
            foreach(var t in FindObjectsByType<TumpukanKemah>(FindObjectsSortMode.None))
                d.tumpukan.Add(new TumpukanSimpanT6 { posisi=t.transform.position,isi=t.CaptureJson() });
            d.tumpukan.Sort((a,b)=>string.CompareOrdinal(a.posisi.ToString("R"),b.posisi.ToString("R")));
            foreach (var s in BagianStruktur.Semua)
            {
                if (s == null || !s.BisaDisimpan) continue;
                var links = new List<string>(); foreach (var t in s.Tetangga) if (t != null && t.BisaDisimpan) links.Add(t.Kunci);
                links.Sort(StringComparer.Ordinal);
                var mr = s.GetComponent<Renderer>();
                Color warna = Color.white;
                if (mr != null && mr.sharedMaterial != null) warna = mr.sharedMaterial.color;
                d.struktur.Add(new PotonganT6 { id = s.Kunci, jenis = s.Jenis, tanah = s.Tertanam, hp = s.HP, panjang = s.Panjang, posisi = s.transform.position, rotasi = s.transform.rotation, skala = s.transform.localScale, warna = warna, sambungan = links.ToArray(), serut = s.SerutKe, pukulCelah = s.PukulCelah });
            }
            d.struktur.Sort((a,b) => string.CompareOrdinal(a.id,b.id));
            foreach (var b in FindObjectsByType<IdentitasBangunan>(FindObjectsSortMode.None))
            {
                var f = b.GetComponentsInChildren<FungsiBangunan>(true); var a = b.GetComponentsInChildren<ApiUnggun>(true); var p = b.GetComponentsInChildren<PanciMasak>(true);
                var n = new BangunanT6 { bentang = b.Bentang, id = b.Kunci, jenis = b.Id, posisi = b.transform.position, rotasi = b.transform.rotation, fungsi = new string[f.Length], api = new string[a.Length], panci = new string[p.Length] };
                for (int i=0;i<f.Length;i++) n.fungsi[i]=f[i].CaptureIsi();
                for (int i=0;i<a.Length;i++) n.api[i]=a[i].CaptureIsi();
                for (int i=0;i<p.Length;i++) n.panci[i]=p[i].CaptureIsi();
                d.bangunan.Add(n);
            }
            d.bangunan.Sort((a,b) => string.CompareOrdinal(a.id,b.id));
            foreach (var c in FindObjectsByType<CetakBiru>(FindObjectsSortMode.None)) d.cetakan.Add(new CetakanT6 { bentang=c.Bentang, jenis=c.Id, posisi=c.transform.position-Vector3.up*0.05f, rotasi=c.transform.rotation, isi=(int[])c.Terisi.Clone() });
            foreach(var k in FindObjectsByType<KayuFisik>(FindObjectsSortMode.None))
            {
                var mr=k.GetComponent<Renderer>(); Color w=mr!=null && mr.sharedMaterial!=null?mr.sharedMaterial.color:Color.white;
                d.kayu.Add(new KayuSimpanT6 { id=k.Kunci,papan=k.Papan,posisi=k.transform.position,sumbu=k.transform.up,warna=w });
            }
            d.kayu.Sort((a,b)=>string.CompareOrdinal(a.id,b.id));
            return JsonUtility.ToJson(d);
        }
        static void Hapus(GameObject g) { g.SetActive(false); if (Application.isPlaying) Destroy(g); else DestroyImmediate(g); }
        public void RestoreJson(string json)
        {
            var d = JsonUtility.FromJson<DuniaT6>(json);
            if (d == null || d.struktur == null || d.bangunan == null || d.cetakan == null) throw new ArgumentException("Snapshot dunia tidak valid");
            var identitas=new HashSet<string>();
            foreach(var n in d.struktur)
                if(n==null || string.IsNullOrEmpty(n.id) || !identitas.Add(n.id) || n.sambungan==null)throw new ArgumentException("Struktur simpan tidak valid");
            foreach(var n in d.bangunan)
                if(n==null || string.IsNullOrEmpty(n.id) || !identitas.Add(n.id) || n.fungsi==null || n.api==null || n.panci==null)throw new ArgumentException("Bangunan simpan tidak valid");
            var rakit = GameServices.Get<AlasKeramat.Player.AndiFPSController>(); if (rakit != null) rakit.TurunRakit();
            foreach (var b in FindObjectsByType<IdentitasBangunan>(FindObjectsSortMode.None)) Hapus(b.gameObject);
            foreach (var s in new List<BagianStruktur>(BagianStruktur.Semua)) if (s != null) Hapus(s.gameObject);
            foreach (var c in FindObjectsByType<CetakBiru>(FindObjectsSortMode.None)) Hapus(c.gameObject);
            foreach (var k in FindObjectsByType<KayuFisik>(FindObjectsSortMode.None)) Hapus(k.gameObject);
            foreach (var p in FindObjectsByType<PohonRebah>(FindObjectsSortMode.None)) Hapus(p.gameObject);
            foreach (var t in FindObjectsByType<TumpukanKemah>(FindObjectsSortMode.None)) Hapus(t.gameObject);
            var peta = new Dictionary<string, BagianStruktur>();
            foreach (var n in d.struktur)
            {
                var mode = Mode(n.jenis);
                var s = ShoulderLogAndFreeformBuilder.Buat(new ShoulderLogAndFreeformBuilder.Rencana { mode=mode, pos=n.posisi, rot=n.rotasi, panjang=n.panjang, tertanam=n.tanah }, SafeMaterialFactory.GetCached("Simpan_"+ColorUtility.ToHtmlStringRGBA(n.warna), n.warna,0f,0.3f));
                s.Kunci=n.id; s.Jenis=n.jenis; s.HP=n.hp; s.SerutKe=n.serut; s.PukulCelah=n.pukulCelah; s.transform.localScale=n.skala;
                peta.Add(n.id,s);
            }
            foreach (var n in d.struktur) foreach (var k in n.sambungan) { BagianStruktur lain; if (peta.TryGetValue(k,out lain)) peta[n.id].Hubungkan(lain); }
            foreach (var n in d.bangunan)
            {
                var g=BangunanBlueprint.Bangun(n.jenis,n.posisi,n.rotasi,n.bentang); g.GetComponent<IdentitasBangunan>().Kunci=n.id;
                var f=g.GetComponentsInChildren<FungsiBangunan>(true); var a=g.GetComponentsInChildren<ApiUnggun>(true); var p=g.GetComponentsInChildren<PanciMasak>(true);
                for(int i=0;i<f.Length && i<n.fungsi.Length;i++) f[i].RestoreIsi(n.fungsi[i]);
                for(int i=0;i<a.Length && i<n.api.Length;i++) a[i].RestoreIsi(n.api[i]);
                for(int i=0;i<p.Length && i<n.panci.Length;i++) p[i].RestoreIsi(n.panci[i]);
            }
            foreach(var n in d.cetakan) { var c=CetakBiru.Buat(n.jenis,n.posisi,n.rotasi,n.bentang); c.Terisi=n.isi; }
            if(d.kayu!=null)foreach(var n in d.kayu)
            {
                var k=KayuFisik.Buat(n.posisi,n.sumbu,SafeMaterialFactory.GetCached("KayuSimpan_"+ColorUtility.ToHtmlStringRGBA(n.warna),n.warna),n.papan); k.Kunci=n.id;
            }
            if(d.tumpukan!=null)foreach(var n in d.tumpukan)
                TumpukanKemah.BuatDi(n.posisi,SafeMaterialFactory.GetCached("TumpukanMuat",new Color(0.32f,0.19f,0.09f))).RestoreJson(n.isi);
            Physics.SyncTransforms();
        }
        static ShoulderLogAndFreeformBuilder.ModeBangun Mode(BagianStruktur.Tipe t)
        {
            switch(t)
            {
                case BagianStruktur.Tipe.Balok:return ShoulderLogAndFreeformBuilder.ModeBangun.Balok;
                case BagianStruktur.Tipe.Papan:return ShoulderLogAndFreeformBuilder.ModeBangun.Papan;
                case BagianStruktur.Tipe.Atap:return ShoulderLogAndFreeformBuilder.ModeBangun.Atap;
                case BagianStruktur.Tipe.Tangga:return ShoulderLogAndFreeformBuilder.ModeBangun.Tangga;
                case BagianStruktur.Tipe.FondasiBatu:return ShoulderLogAndFreeformBuilder.ModeBangun.FondasiBatu;
                case BagianStruktur.Tipe.DindingBatu:return ShoulderLogAndFreeformBuilder.ModeBangun.DindingBatu;
                case BagianStruktur.Tipe.JalanBatu:return ShoulderLogAndFreeformBuilder.ModeBangun.JalanBatu;
                default:return ShoulderLogAndFreeformBuilder.ModeBangun.Tiang;
            }
        }
    }
}
