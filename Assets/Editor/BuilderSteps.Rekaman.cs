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
            var settings=imp.defaultSampleSettings;
            bool bed=nama.StartsWith("bed_");
            settings.loadType=bed?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat=bed?AudioCompressionFormat.Vorbis:AudioCompressionFormat.ADPCM;
            settings.quality=0.7f;imp.defaultSampleSettings=settings;imp.forceToMono=!bed;imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
    }
}
