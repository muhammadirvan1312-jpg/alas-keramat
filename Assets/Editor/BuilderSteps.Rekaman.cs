using System.IO;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
namespace AlasKeramat.EditorTools
{
    public static partial class NusantaraMasterBuilder
    {
        static AudioClip RekamanCC0(string nama)
        {
            string file=null;
            if(nama==ProceduralAudioSynth.BedPagi || nama==ProceduralAudioSynth.BedSiang || nama==ProceduralAudioSynth.BedSenja)file="forest_day.mp3";
            if(nama==ProceduralAudioSynth.BedMalam || nama==ProceduralAudioSynth.BedDini || nama==ProceduralAudioSynth.BedRawa || nama==ProceduralAudioSynth.BedWingit)file="forest_night.mp3";
            if(nama.StartsWith("langkah_"))
            {
                int v=nama[nama.Length-1]-'0';
                if(nama.StartsWith("langkah_daun_"))file=v%2==0?"leaves01.ogg":"leaves02.ogg";
                else if(nama.StartsWith("langkah_kayu_"))file="wood0"+(1+v%3)+".ogg";
                else if(nama.StartsWith("langkah_batu_"))file="stone01.ogg";
                else if(nama.StartsWith("langkah_tanah_"))file="mud02.ogg";
                else if(nama.StartsWith("langkah_pasir_"))file="gravel.ogg";
            }
            if(file==null)return null;
            string path="Assets/ThirdParty/CC0/Audio/"+file;
            if(!File.Exists(path))throw new System.Exception("Rekaman CC0 wajib belum diunduh: "+path);
            var imp=AssetImporter.GetAtPath(path) as AudioImporter;
            if(imp==null)throw new System.Exception("Rekaman CC0 belum diimpor: "+path);
            bool bed=nama.StartsWith("bed_");
            string hasil=FolderAudio+"/Rekaman_"+Path.GetFileNameWithoutExtension(file)+".wav";
            if(!File.Exists(hasil))
            {
                var baca=imp.defaultSampleSettings;baca.loadType=AudioClipLoadType.DecompressOnLoad;baca.compressionFormat=AudioCompressionFormat.PCM;
                imp.defaultSampleSettings=baca;imp.forceToMono=!bed;imp.loadInBackground=false;imp.SaveAndReimport();
                var raw=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if(raw==null)throw new System.Exception("Rekaman tidak dapat dibaca: "+path);
                raw.LoadAudioData();var sampel=new float[raw.samples*raw.channels];
                if(!raw.GetData(sampel,0))throw new System.Exception("PCM rekaman tidak tersedia: "+path);
                double total=0;float peak=0;
                foreach(float v in sampel) { total+=(double)v*v;peak=Mathf.Max(peak,Mathf.Abs(v)); }
                float rms=(float)System.Math.Sqrt(total/Mathf.Max(1,sampel.Length));
                float gain=GainRekaman(rms,peak,bed);
                if(gain<=0f)throw new System.Exception("Rekaman hening: "+path);
                for(int i=0;i<sampel.Length;i++)sampel[i]*=gain;
                TulisPcmRekaman(hasil,sampel,raw.frequency,raw.channels);
                AssetDatabase.ImportAsset(hasil);
                Catat("Level rekaman "+file+": RMS "+(20f*Mathf.Log10(rms)).ToString("F1")+" dB, gain "+(20f*Mathf.Log10(gain)).ToString("F1")+" dB, peak "+(20f*Mathf.Log10(peak*gain)).ToString("F1")+" dB");
            }
            AturRekaman(imp,bed);
            var jadi=AssetImporter.GetAtPath(hasil) as AudioImporter;
            if(jadi==null)throw new System.Exception("Rekaman seimbang belum diimpor: "+hasil);
            AturRekaman(jadi,bed);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(hasil);
        }

        public static float GainRekaman(float rms,float peak,bool bed)
        {
            if(rms<=0.0000001f || peak<=0.0000001f)return 0f;
            return Mathf.Min(Mathf.Pow(10f,(bed?-25f:-24f)/20f)/rms,Mathf.Pow(10f,-1f/20f)/peak);
        }
        static void AturRekaman(AudioImporter imp,bool bed)
        {
            var s=imp.defaultSampleSettings;s.loadType=bed?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;
            s.compressionFormat=bed?AudioCompressionFormat.Vorbis:AudioCompressionFormat.ADPCM;s.quality=0.7f;
            imp.defaultSampleSettings=s;imp.forceToMono=!bed;imp.loadInBackground=bed;imp.SaveAndReimport();
        }
        static void TulisPcmRekaman(string path,float[] sampel,int rate,int channels)
        {
            using(var stream=File.Create(path))using(var w=new BinaryWriter(stream))
            {
                int size=sampel.Length*2;
                w.Write(new[]{'R','I','F','F'});w.Write(36+size);w.Write(new[]{'W','A','V','E'});
                w.Write(new[]{'f','m','t',' '});w.Write(16);w.Write((short)1);w.Write((short)channels);
                w.Write(rate);w.Write(rate*channels*2);w.Write((short)(channels*2));w.Write((short)16);
                w.Write(new[]{'d','a','t','a'});w.Write(size);
                foreach(float v in sampel)w.Write((short)(Mathf.Clamp(v,-1f,1f)*32767));
            }
        }
    }
}
