using System;
using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Player;
using AlasKeramat.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using Object=UnityEngine.Object;
namespace AlasKeramat.EditorTools
{
    public static class PemeriksaanUjiMain
    {
        public static void Jalankan(Action<string,bool> cek)
        {
            PemeriksaanRegresiSimpan.Jalankan(cek);
            var arah=AndiFPSController.ArahGerak(new Vector2(0,0.575f));
            cek("Analog: setengah rentang menghasilkan setengah laju tanpa redaman kedua",Mathf.Abs((arah*AndiFPSController.KecepatanTarget(0.575f,false,false)).y-AndiFPSController.JalanMaks*0.5f)<0.001f);
            cek("Analog: diagonal dinormalisasi dan nol tidak bergerak",Mathf.Abs(AndiFPSController.ArahGerak(Vector2.one).magnitude-1f)<0.001f && AndiFPSController.ArahGerak(Vector2.zero)==Vector2.zero);
            var go=new GameObject("UjiMultitouchMain");var es=Object.FindFirstObjectByType<EventSystem>();
            try
            {
                var look=go.AddComponent<ZonaGeser>();var satu=new PointerEventData(es){pointerId=1,delta=new Vector2(12,4)};var dua=new PointerEventData(es){pointerId=2,delta=new Vector2(90,90)};
                look.OnPointerDown(satu);look.OnPointerDown(dua);look.OnDrag(dua);look.OnPointerUp(dua);look.OnDrag(satu);
                cek("Sentuh: jari kedua tidak merebut kamera",look.AmbilDelta()==new Vector2(12,4));
                look.ResetSentuh();look.OnDrag(satu);cek("Sentuh: reset membuang delta dan kepemilikan jari",look.AmbilDelta()==Vector2.zero);
            }
            finally { Object.DestroyImmediate(go); }
            var tempurUji=new GameObject("UjiBatalElakPanel");var vitalSemula=GameServices.Get<AndiVitalsAndSanity>();
            try
            {
                var vital=tempurUji.AddComponent<AndiVitalsAndSanity>();GameServices.Register(vital);
                var tempur=tempurUji.AddComponent<CombatAndLeftHandController>();tempur.Elak(Vector2.up);
                bool kebalSaatElak=tempur.Kebal;
                tempur.HentikanAksiPanel();
                cek("Panel: membatalkan elak tidak meninggalkan kebal permanen atau serangan",kebalSaatElak && !tempur.Kebal && !tempur.SedangMenyerang);
            }
            finally { if(vitalSemula!=null)GameServices.Register(vitalSemula);Object.DestroyImmediate(tempurUji); }
            cek("LOD: dua pohon dalam satu petak tetap memilih detail berdasarkan jaraknya sendiri",!VegetationManager.PakaiLodJauh(8,40) && VegetationManager.PakaiLodJauh(100,40));
            cek("Preset otomatis konservatif & tidak menganggap RAM sebagai bukti Ultra",MenuScreens.RekomendasiPreset(4096,0)==PresetGrafis.Rendah && MenuScreens.RekomendasiPreset(8192,2048)==PresetGrafis.Sedang && MenuScreens.RekomendasiPreset(16384,8192)==PresetGrafis.Tinggi);
            cek("Preset: rumput bertingkat dan Ultra 30 FPS",MenuScreens.AmbilPreset(PresetGrafis.Rendah).rumput<MenuScreens.AmbilPreset(PresetGrafis.Sedang).rumput && MenuScreens.AmbilPreset(PresetGrafis.Sedang).rumput<MenuScreens.AmbilPreset(PresetGrafis.Ultra).rumput && MenuScreens.AmbilPreset(PresetGrafis.Ultra).targetFps==30);
            Vector2 p=new Vector2(-560,290);
            cek("Peta: koordinat dunia dua kilometer pulang-pergi & utara benar",Vector2.Distance(p,PetaSurveiUI.KeDunia(PetaSurveiUI.KeUV(p)))<0.001f && Mathf.Abs(PetaSurveiUI.Bearing(Vector2.up))<0.001f && Mathf.Abs(PetaSurveiUI.Bearing(Vector2.right)-90f)<0.001f);
            CekRuangTombol(cek);
            Canvas.ForceUpdateCanvases();bool tumpuk=false;
            var sabuk=Object.FindFirstObjectByType<SabukCepat>();
            foreach(string nama in new[]{"TombolSimpan","TombolMuat","TombolBatu"})
            {
                var goTombol=GameObject.Find(nama);
                if(goTombol==null || sabuk==null){tumpuk=true;continue;}
                foreach(var slot in sabuk.GetComponentsInChildren<UnityEngine.UI.Button>())
                    tumpuk |= KotakUI((RectTransform)goTombol.transform).Overlaps(KotakUI((RectTransform)slot.transform));
            }
            cek("HUD: simpan, muat, dan panggul batu tidak menutup slot cepat",!tumpuk);
            var peta=Object.FindFirstObjectByType<PetaSurveiUI>(FindObjectsInactive.Include);
            cek("Peta: panel, gambar, GPS, tujuan & kompas tersambung",peta!=null && peta.Panel!=null && peta.Bidang!=null && peta.Andi!=null && peta.Tujuan!=null && peta.Kompas!=null);
            cek("Spawn dan koper darurat tidak tertutup dekorasi tinggi",NusantaraMasterBuilder.DekorasiMenutupAwal(NusantaraMasterBuilder.SpawnAndi.x,NusantaraMasterBuilder.SpawnAndi.z) && NusantaraMasterBuilder.DekorasiMenutupAwal(16,-460) && !NusantaraMasterBuilder.DekorasiMenutupAwal(250,-300));
            cek("Diagnostik main & safe area kontrol terpasang",Object.FindFirstObjectByType<DiagnostikMain>()!=null && Object.FindFirstObjectByType<RuangAmanKontrol>()!=null);
            float g=NusantaraMasterBuilder.GainRekaman(Mathf.Pow(10f,-50.9f/20f),Mathf.Pow(10f,-28.2f/20f),true);
            cek("Audio: rekaman pelan diangkat ke RMS sasaran tanpa clipping",g>1f && Mathf.Abs(20f*Mathf.Log10(Mathf.Pow(10f,-50.9f/20f)*g)+25f)<0.01f && Mathf.Pow(10f,-28.2f/20f)*g<=Mathf.Pow(10f,-1f/20f));
            cek("Audio: normalisasi membatasi peak dan menolak hening",NusantaraMasterBuilder.GainRekaman(0.01f,0.99f,false)*0.99f<=Mathf.Pow(10f,-1f/20f)+0.00001f && NusantaraMasterBuilder.GainRekaman(0,0,true)==0f);
            bool rekaman=true;
            foreach(var file in new[]{"forest_day.mp3","forest_night.mp3","leaves01.ogg","wood01.ogg","stone01.ogg"})
            {
                string path="Assets/ThirdParty/CC0/Audio/"+file;var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);var imp=AssetImporter.GetAtPath(path) as AudioImporter;
                rekaman &= clip!=null && clip.length>0f && imp!=null;
                if(imp!=null && file.EndsWith("mp3"))rekaman &= imp.defaultSampleSettings.loadType==AudioClipLoadType.Streaming;
            }
            foreach(string file in new[]{"forest_day","forest_night","leaves01","wood01","stone01"})
            {
                var klip=AssetDatabase.LoadAssetAtPath<AudioClip>(NusantaraMasterBuilder.FolderAudio+"/Rekaman_"+file+".wav");
                rekaman &= klip!=null && klip.length>0;
            }
            cek("Audio: rekaman CC0 dinormalisasi ke aset panggang dan bed streaming",rekaman);
            float master=ProceduralAudioSynth.VolMaster;
            ProceduralAudioSynth.VolMaster=1f;
            float kategori=ProceduralAudioSynth.VolumeKategori(AudioCategory.Ambien);
            try
            {
                ProceduralAudioSynth.VolMaster=0.5f;ProceduralAudioSynth.SetVolume(AudioCategory.Ambien,0.4f);
                cek("Audio: master dan kategori dikalikan tepat sekali",Mathf.Abs(ProceduralAudioSynth.VolumeKategori(AudioCategory.Ambien)-0.2f)<0.0001f);
                ProceduralAudioSynth.SetVolume(AudioCategory.Ambien,0f);
                cek("Audio: kategori ambien dapat dimatikan",ProceduralAudioSynth.VolumeKategori(AudioCategory.Ambien)==0f);
            }
            finally { ProceduralAudioSynth.SetVolume(AudioCategory.Ambien,kategori);ProceduralAudioSynth.VolMaster=master; }
        }
        static void CekRuangTombol(Action<string,bool> cek)
        {
            var tc=Object.FindFirstObjectByType<TouchControls>();
            string[] nama={"TombolTebas","TombolLompat","TombolGanti","TombolTangkis","TombolTangan","TombolMode","TombolJongkok","TombolLempar","TombolObor","TombolTas","TombolBuku","TombolSimpan","TombolMuat","TombolBatu"};
            var tombol=new RectTransform[nama.Length];var ukuran=new Vector2[nama.Length];bool lengkap=tc!=null,bersih=true;
            for(int i=0;i<nama.Length;i++)
            {
                tombol[i]=tc!=null?tc.transform.Find(nama[i]) as RectTransform:null;
                if(tombol[i]==null && tc!=null)tombol[i]=tc.transform.parent.Find(nama[i]) as RectTransform;
                lengkap &= tombol[i]!=null;if(tombol[i]!=null)ukuran[i]=tombol[i].sizeDelta;
            }
            try
            {
                foreach(float skala in new[]{0.85f,1f,1.15f,1.3f})
                {
                    for(int i=0;i<tombol.Length;i++)if(tombol[i]!=null)tombol[i].sizeDelta=ukuran[i]*(i<11?skala:1f);
                    Canvas.ForceUpdateCanvases();
                    for(int i=0;i<tombol.Length;i++)for(int j=i+1;j<tombol.Length;j++)
                        if(tombol[i]!=null && tombol[j]!=null && KotakUI(tombol[i]).Overlaps(KotakUI(tombol[j])))bersih=false;
                }
            }
            finally { for(int i=0;i<tombol.Length;i++)if(tombol[i]!=null)tombol[i].sizeDelta=ukuran[i];Canvas.ForceUpdateCanvases(); }
            cek("HUD: 14 tombol aksi/tas/buku/simpan tidak bertumpuk pada skala 85 sampai 130 persen",lengkap && bersih);
        }
        static Rect KotakUI(RectTransform rt)
        {
            var sudut=new Vector3[4];rt.GetWorldCorners(sudut);
            return Rect.MinMaxRect(sudut[0].x,sudut[0].y,sudut[2].x,sudut[2].y);
        }
    }
}
