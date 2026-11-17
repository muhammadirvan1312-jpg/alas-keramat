using System;
using System.Collections.Generic;
using AlasKeramat.Player;
using UnityEngine;
namespace AlasKeramat.Core
{
    public class QuestManager : MonoBehaviour, ISaveable
    {
        [Serializable] public class ProgresMisiT6
        {
            public List<string> koper = new List<string>();
            public bool jamu, catatan, saung, api, malam, pagi;
            public bool m05; // T8: diisi misi M05 (T10); membuka semua perintah Kang Deden
            public bool[] selesai = new bool[4];
        }
        public ProgresMisiT6 Progres = new ProgresMisiT6();
        public bool M05Selesai { get { return Progres.m05; } }
        public string SaveKey { get { return "misi"; } }
        void Awake() { GameServices.Register(this); }
        void OnEnable() { GameEvents.OnStructureBuilt += BangunanSelesai; }
        void OnDisable() { GameEvents.OnStructureBuilt -= BangunanSelesai; }
        public static bool Syarat(ProgresMisiT6 p, QuestID q)
        {
            switch(q)
            {
                case QuestID.M01:return p.koper.Count>=3;
                case QuestID.M02:return p.jamu && p.catatan;
                case QuestID.M03:return p.saung && p.api;
                case QuestID.M04:return p.malam && p.pagi;
                default:return false;
            }
        }
        public static int Prioritas(ProgresMisiT6 p) { for(int i=0;i<4;i++)if(!p.selesai[i] && (i<3 || p.malam))return i+1; return 0; }
        public static List<QuestID> Majukan(ProgresMisiT6 p)
        {
            var hasil=new List<QuestID>();
            for(int i=0;i<4;i++) if(!p.selesai[i] && Syarat(p,(QuestID)(i+1))) { p.selesai[i]=true; hasil.Add((QuestID)(i+1)); }
            return hasil;
        }
        public void Periksa() { foreach(var id in Majukan(Progres))GameEvents.RaiseQuestAdvanced(id); }
        public void KoperDibuka(string id) { if(string.IsNullOrEmpty(id))return; if(!Progres.koper.Contains(id))Progres.koper.Add(id); Periksa(); }
        public void BeriJamu() { Progres.jamu=true; Periksa(); }
        public void TunjukkanCatatan() { if(!Progres.jamu)return; Progres.catatan=true; Periksa(); }
        void BangunanSelesai(BlueprintID id) { if(id==BlueprintID.SaungPanggung)Progres.saung=true; if(id==BlueprintID.ApiUnggun)Progres.api=true; Periksa(); }
        public void Waktu(int hari,float jam,bool hidup)
        {
            if(hari>1 || (hari==1 && jam>=18f))Progres.malam=true;
            if((hari>2 || (hari==2 && jam>=6f)) && hidup)Progres.pagi=true;
            Periksa();
        }
        int _menit=-1;
        void Update()
        {
            var jam=GameServices.Get<TimeOfDayClock>(); if(jam==null)return;
            int menit=jam.Hari*1440+Mathf.FloorToInt(jam.Jam*60f); if(menit==_menit)return; _menit=menit;
            var vital=GameServices.Get<AndiVitalsAndSanity>(); Waktu(jam.Hari,jam.Jam,vital!=null && vital.IsAlive);
        }
        public string Kartu()
        {
            var p=Progres;
            switch(Prioritas(p))
            {
                case 1:return "Ambil Koper Darurat ("+p.koper.Count+"/3)";
                case 2:return p.jamu?"Tolong Kang Deden: tunjukkan Buku Catatan":"Tolong Kang Deden: beri Jamu, lalu tunjukkan Buku Catatan";
                case 3:return "Bangun Saung Panggung & Api Unggun sebelum 18:00\nSaung ("+(p.saung?1:0)+"/1), Api ("+(p.api?1:0)+"/1)";
                case 4:return "Bertahan sampai pagi. Jaga api tetap menyala.";
                default:return "Malam pertama terlewati. Jelajahi pulau.";
            }
        }
        public string CaptureJson() { return JsonUtility.ToJson(Progres); }
        public void RestoreJson(string json) { var p=JsonUtility.FromJson<ProgresMisiT6>(json); if(p!=null && p.selesai!=null && p.selesai.Length==4 && p.koper!=null)Progres=p; _menit=-1; }
    }
}
