using System;
using System.Collections.Generic;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;
using Object=UnityEngine.Object;
namespace AlasKeramat.EditorTools
{
    public static class PemeriksaanRegresiSimpan
    {
        sealed class Snapshot : ISaveable
        {
            public string SaveKey { get; set; }
            public string Isi="{}";
            public bool Tolak, SaatMuat;
            public string CaptureJson() { return Isi; }
            public void RestoreJson(string json) { SaatMuat=SaveSystem.SedangMemuat; if(Tolak) { Tolak=false; throw new InvalidOperationException("uji"); } Isi=json; }
        }
        public static void Jalankan(Action<string,bool> cek)
        {
            cek("Tas: perban dapat dipakai saat berdarah meskipun kategorinya alat",AlasKeramat.UI.TikarUI.BisaPakaiItem(ItemID.PerbanKain,true) && !AlasKeramat.UI.TikarUI.BisaPakaiItem(ItemID.PerbanKain,false));
            var dunia=new Snapshot{SaveKey="dunia",Isi="{\"struktur\":[],\"bangunan\":[]}"};
            var peristiwa=new Snapshot{SaveKey="peristiwa_dunia",Isi="{\"jenazah\":2,\"hariMeteor\":7}"};
            string salinan=SaveSystem.KeJson(new ISaveable[]{dunia,peristiwa});
            dunia.Isi="{}";peristiwa.Isi="{}";
            SaveSystem.DariJson(salinan,new ISaveable[]{dunia,peristiwa});
            cek("Simpan: struktur dan peristiwa memakai kunci berbeda dan keduanya pulih",dunia.Isi.Contains("struktur") && peristiwa.Isi.Contains("jenazah"));
            cek("Muat: event stok ditandai sebagai restore dan penanda dilepas setelah selesai",dunia.SaatMuat && peristiwa.SaatMuat && !SaveSystem.SedangMemuat);
            string lama=salinan.Replace("\"kunci\":\"peristiwa_dunia\"","\"kunci\":\"dunia\"");
            dunia.Isi="{}";peristiwa.Isi="{}";SaveSystem.DariJson(lama,new ISaveable[]{dunia,peristiwa});
            cek("Simpan: dua entri dunia versi lama dimigrasikan tanpa saling menimpa",dunia.Isi.Contains("struktur") && peristiwa.Isi.Contains("jenazah"));
            bool duplikat=false,kosong=false,pulih=false;
            try { SaveSystem.KeJson(new ISaveable[]{dunia,dunia}); } catch(System.IO.InvalidDataException) { duplikat=true; }
            try { SaveSystem.DariJson("{}",new ISaveable[]{dunia}); } catch(System.IO.InvalidDataException) { kosong=true; }
            dunia.Isi="{\"semula\":1}";peristiwa.Tolak=true;
            try { SaveSystem.DariJson(salinan,new ISaveable[]{dunia,peristiwa}); } catch(InvalidOperationException) { pulih=dunia.Isi.Contains("semula"); }
            cek("Simpan: kunci ganda dan berkas kosong ditolak; restore gagal memulihkan state",duplikat && kosong && pulih && !SaveSystem.SedangMemuat);
            var kunci=new HashSet<string>();bool unik=true;
            foreach(var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                if(mb is ISaveable s && !string.IsNullOrEmpty(s.SaveKey))unik &= kunci.Add(s.SaveKey);
            cek("Simpan: semua pemilik snapshot scene memiliki kunci unik",unik);
            var go=new GameObject("UjiKapasitasRakit");
            try
            {
                var inv=go.AddComponent<TikarPandan3DInventory>();
                inv.Tambah(ItemID.Ranting,1);inv.Tambah(ItemID.PerbanKain,Mathf.Max(0,ItemDatabase.MaxCarry(ItemID.PerbanKain)-1));
                int sebelum=inv.Jumlah(ItemID.PerbanKain);
                var resep=new CraftingRecipe{output=new ItemStack{id=ItemID.PerbanKain,count=2},inputs=new[]{new ItemStack{id=ItemID.Ranting,count=1}}};
                cek("Rakit: output tidak muat seluruhnya tidak memakan bahan",!TikarPanggung.Rakit(resep,inv) && inv.Jumlah(ItemID.Ranting)==1 && inv.Jumlah(ItemID.PerbanKain)==sebelum);
                int n=0;Action<ItemID,int> perubahan=(id,jumlah)=>{if(id==ItemID.Ranting && jumlah==0)n++;};
                GameEvents.OnItemChanged+=perubahan;
                try { inv.RestoreJson("{\"id\":[],\"n\":[]}"); } finally { GameEvents.OnItemChanged-=perubahan; }
                cek("Muat inventaris: penghapusan item mengirim event untuk menyegarkan HUD",n==1 && inv.Jumlah(ItemID.Ranting)==0);
            }
            finally { Object.DestroyImmediate(go); }
            var tercakup=new HashSet<int>();
            for(int page=0;page<TikarPanggung.HitungHalaman(ItemDatabase.All.Count);page++)
                for(int i=0;i<TikarPanggung.MaksTampil && page*TikarPanggung.MaksTampil+i<ItemDatabase.All.Count;i++)tercakup.Add(page*TikarPanggung.MaksTampil+i);
            cek("Tas: seluruh jenis item dapat dijangkau melalui halaman, termasuk sesudah item 36",tercakup.Count==ItemDatabase.All.Count && TikarPanggung.HitungHalaman(37)==2);
        }
    }
}
