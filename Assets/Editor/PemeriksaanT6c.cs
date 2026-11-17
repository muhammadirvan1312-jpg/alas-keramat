using System;
using System.Collections.Generic;
using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;
using Object = UnityEngine.Object;
namespace AlasKeramat.EditorTools
{
    public static class PemeriksaanT6c
    {
        public static void Jalankan(Action<string,bool> cek)
        {
            cek("Atap 20-45 derajat, tangga 25-40 derajat", ShoulderLogAndFreeformBuilder.SudutSah(ShoulderLogAndFreeformBuilder.ModeBangun.Atap,Vector3.zero,new Vector3(2f,1f,0f)) && !ShoulderLogAndFreeformBuilder.SudutSah(ShoulderLogAndFreeformBuilder.ModeBangun.Tangga,Vector3.zero,new Vector3(1f,2f,0f)));
            cek("Batu HP 500 dan hasil bongkar atap adalah papan", BagianStruktur.HPAwal(BagianStruktur.Tipe.FondasiBatu)==500f && BagianStruktur.HasilPapan(BagianStruktur.Tipe.Atap));
            var p=new QuestManager.ProgresMisiT6();
            var urut=new List<QuestID>();
            p.koper.Add("K1"); p.koper.Add("K2"); p.koper.Add("K3"); urut.AddRange(QuestManager.Majukan(p));
            p.jamu=true; cek("M02 perlu Jamu DAN Catatan",!QuestManager.Syarat(p,QuestID.M02));
            p.catatan=true; urut.AddRange(QuestManager.Majukan(p));
            p.saung=true; p.api=true; urut.AddRange(QuestManager.Majukan(p));
            p.malam=true; cek("M04 menunggu pagi",!QuestManager.Syarat(p,QuestID.M04));
            p.pagi=true; urut.AddRange(QuestManager.Majukan(p));
            cek("Urutan M01 -> M02 -> M03 -> M04, tanpa event ganda", urut.Count==4 && urut[0]==QuestID.M01 && urut[1]==QuestID.M02 && urut[2]==QuestID.M03 && urut[3]==QuestID.M04 && QuestManager.Majukan(p).Count==0);
            var paralel=new QuestManager.ProgresMisiT6 { saung=true,api=true };
            cek("M03 boleh selesai paralel sementara HUD tetap M01", QuestManager.Majukan(paralel).Count==1 && QuestManager.Prioritas(paralel)==1);
            var go=new GameObject("UjiDuniaT6c"); var dunia=go.AddComponent<SimpanDunia>();
            string sebelum=dunia.CaptureJson();
            var kayuLama=new HashSet<KayuFisik>(Object.FindObjectsByType<KayuFisik>(FindObjectsSortMode.None));
            try
            {
                var a=ShoulderLogAndFreeformBuilder.Buat(new ShoulderLogAndFreeformBuilder.Rencana { mode=ShoulderLogAndFreeformBuilder.ModeBangun.Tiang,pos=new Vector3(0f,100f,0f),rot=Quaternion.identity,panjang=3.6f,tertanam=true },null);
                var b=ShoulderLogAndFreeformBuilder.Buat(new ShoulderLogAndFreeformBuilder.Rencana { mode=ShoulderLogAndFreeformBuilder.ModeBangun.Balok,pos=new Vector3(1f,101f,0f),rot=Quaternion.Euler(0f,0f,90f),panjang=2f,a=a },null);
                b.HP=83f;
                var inv=go.AddComponent<TikarPandan3DInventory>(); inv.Tambah(ItemID.Batu,10);
                var peti=BangunanBlueprint.Bangun(BlueprintID.PetiPenyimpanan,new Vector3(5f,100f,0f),Quaternion.identity).GetComponentInChildren<FungsiBangunan>();
                cek("Peti memindahkan item tanpa menggandakan",peti.SimpanItem(inv,ItemID.Batu,7)==7 && inv.Jumlah(ItemID.Batu)==3 && peti.JumlahPeti==7);
                string tersimpan=dunia.CaptureJson();
                b.HP=1f; peti.AmbilItem(inv,ItemID.Batu,2);
                dunia.RestoreJson(tersimpan);
                cek("Simpan -> muat struktur, graf, HP dan isi blueprint identik",dunia.CaptureJson()==tersimpan);
                dunia.RestoreJson(tersimpan);
                cek("Muat dua kali tidak menggandakan bangunan",dunia.CaptureJson()==tersimpan);
                var bongkar=ShoulderLogAndFreeformBuilder.Buat(new ShoulderLogAndFreeformBuilder.Rencana { mode=ShoulderLogAndFreeformBuilder.ModeBangun.Papan,pos=new Vector3(0f,100f,5f),rot=Quaternion.identity,panjang=2f,tertanam=true },null);
                int jumlah=Object.FindObjectsByType<KayuFisik>(FindObjectsSortMode.None).Length;
                cek("Bongkar wajib tahan satu detik",bongkar.HoldDuration==1f);
                bongkar.Interact(null);
                var semuaKayu=Object.FindObjectsByType<KayuFisik>(FindObjectsSortMode.None);
                cek("Bongkar benar-benar menghasilkan KayuFisik papan",semuaKayu.Length==jumlah+1 && Array.Exists(semuaKayu,k=>k.Papan));
                bool objek=true;
                for(int i=15;i<=26;i++)
                {
                    GameObject bangunan=null;
                    try { bangunan=BangunanBlueprint.Bangun((BlueprintID)i,new Vector3(20f+i,100f,0f),Quaternion.identity); objek &= bangunan.GetComponentsInChildren<Collider>().Length>0 && bangunan.GetComponent<IdentitasBangunan>().Id==(BlueprintID)i; }
                    finally { if(bangunan!=null)Object.DestroyImmediate(bangunan); }
                }
                cek("Semua blueprint v3 menghasilkan objek dan collider",objek);
            }
            finally
            {
                dunia.RestoreJson(sebelum);
                foreach(var k in Object.FindObjectsByType<KayuFisik>(FindObjectsSortMode.None))if(!kayuLama.Contains(k))Object.DestroyImmediate(k.gameObject);
                Object.DestroyImmediate(go);
            }
            cek("QuestManager, HUD misi, SimpanDunia dan peti tersambung",Object.FindFirstObjectByType<QuestManager>()!=null && Object.FindFirstObjectByType<AlasKeramat.UI.HUDMisi>()!=null && Object.FindFirstObjectByType<SimpanDunia>()!=null && Object.FindFirstObjectByType<AlasKeramat.UI.PetiUI>()!=null);
            cek("Rakit memiliki kecepatan 2,5 m/s",Mathf.Approximately(AlasKeramat.Player.AndiFPSController.KecepatanRakit,2.5f));
        }
    }
}
