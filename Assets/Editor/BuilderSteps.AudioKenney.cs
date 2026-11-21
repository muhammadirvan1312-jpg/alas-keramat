using System.Collections.Generic;
using System.IO;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using S = AlasKeramat.Core.ProceduralAudioSynth;
namespace AlasKeramat.EditorTools
{
    // Langkah 7z2: rekaman CC0 Kenney (Impact Sounds, RPG Audio) menggantikan suara sintetis tempur, tebang, ambil barang dan ketukan teror.
    public static partial class NusantaraMasterBuilder
    {
        public const string FolderKenney = "Assets/ThirdParty/CC0/Audio/kenney";
        public static readonly string[,] PetaKenney =
        {
            { S.KlipKenaDaging, "impact_punch_heavy_000" }, { S.KlipParry, "impact_metal_heavy_000" }, { S.KlipTubuhJatuh, "impact_soft_heavy_000" },
            { S.KlipTebang, "chop" }, { S.KlipAmbilKayu, "impact_wood_medium_000" }, { S.KlipAmbilItem, "handle_small_leather" },
            { S.KlipTebasBasah, "knife_slice" }, { S.KlipKetukan, "impact_wood_heavy_000" }, { S.KlipKerikil, "impact_generic_light_000" }, { S.KlipUIKlik, "click_001" },
        };

        // Rekaman CC0 OpenGameArt: {nama klip synth, file di FolderOGA, mode} mode m=efek mono, b=bed/musik stereo streaming.
        public const string FolderOGA = "Assets/ThirdParty/CC0/Audio/oga";
        public static readonly string[,] PetaOGA =
        {
            { S.KlipJantung, "jantung.wav", "m" }, { S.KlipNapas, "napas_lelah.ogg", "m" }, { S.KlipNapasBerat, "napas_horor.ogg", "m" },
            { S.KlipTembak, "tembak.wav", "m" }, { S.KlipGeramSekte, "geram.ogg", "m" }, { S.KlipTeriakSekte, "teriak.ogg", "m" },
            { S.KlipJeritGua, "jerit_gua.ogg", "m" }, { S.KlipRintihSekte, "rintih.ogg", "m" },
            { S.BedTebing, "angin.ogg", "b" }, { S.BedPabrik, "angin.ogg", "b" }, { S.BedWingit, "musik_seram.ogg", "b" },
            { S.KlipMusikTempur, "musik_tegang.ogg", "b" }, { S.KlipHujan, "hujan_malam.ogg", "b" }, { S.KlipApi, "api.ogg", "m" },
            { AlasKeramat.AI.MusikJelajah.KlipSiang, "musik_siang.mp3", "b" }, { AlasKeramat.AI.MusikJelajah.KlipMalam, "musik_malam.ogg", "b" },
            { AlasKeramat.AI.MusikJelajah.KlipMisteri, "musik_misteri.ogg", "b" }, { S.BedDini, "musik_kuil.ogg", "b" },
            { S.OsJangkrik, "jangkrik.ogg", "m" }, { S.OsKatak, "katak.ogg", "m" }, { S.OsHantu, "burung_malam.ogg", "m" }, { S.OsPagi, "burung_pagi.ogg", "m" },
        };

        static AudioClip ImporRekaman(string path, bool bed)
        {
            var imp = AssetImporter.GetAtPath(path) as AudioImporter;
            if (imp == null) return null;
            var s = imp.defaultSampleSettings;
            if (bed) { s.loadType = AudioClipLoadType.Streaming; s.compressionFormat = AudioCompressionFormat.Vorbis; s.quality = 0.55f; }
            else { s.loadType = AudioClipLoadType.DecompressOnLoad; s.compressionFormat = AudioCompressionFormat.ADPCM; }
            imp.defaultSampleSettings = s; imp.forceToMono = !bed; imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        static void LangkahAudioKenney()
        {
            var synth = Object.FindFirstObjectByType<S>();
            if (synth == null || synth.NamaPanggang == null) { Catat("Kenney: synth tidak ada, dilewati"); return; }
            var nama = new List<string>(synth.NamaPanggang);
            var klip = new List<AudioClip>(synth.KlipPanggang);
            int n = 0, hilang = 0;
            for (int i = 0; i < PetaKenney.GetLength(0); i++)
            {
                string path = FolderKenney + "/" + PetaKenney[i, 1] + ".ogg";
                if (!File.Exists(path)) path = FolderKenney + "/" + PetaKenney[i, 1] + ".wav";
                if (!File.Exists(path)) { hilang++; continue; }
                var imp = AssetImporter.GetAtPath(path) as AudioImporter;
                if (imp == null) { hilang++; continue; }
                var s = imp.defaultSampleSettings;
                s.loadType = AudioClipLoadType.DecompressOnLoad; s.compressionFormat = AudioCompressionFormat.ADPCM;
                imp.defaultSampleSettings = s; imp.forceToMono = true; imp.SaveAndReimport();
                var c = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (c == null) { hilang++; continue; }
                int idx = nama.IndexOf(PetaKenney[i, 0]);
                if (idx >= 0) klip[idx] = c; else { nama.Add(PetaKenney[i, 0]); klip.Add(c); }
                n++;
            }
            int nOga = 0;
            for (int i = 0; i < PetaOGA.GetLength(0); i++)
            {
                string path = FolderOGA + "/" + PetaOGA[i, 1];
                if (!File.Exists(path)) { hilang++; continue; }
                var c = ImporRekaman(path, PetaOGA[i, 2] == "b");
                if (c == null) { hilang++; continue; }
                int idx = nama.IndexOf(PetaOGA[i, 0]);
                if (idx >= 0) klip[idx] = c; else { nama.Add(PetaOGA[i, 0]); klip.Add(c); }
                nOga++;
            }
            if (synth.GetComponent<SuaraHujan>() == null) synth.gameObject.AddComponent<SuaraHujan>();
            if (synth.GetComponent<AlasKeramat.AI.MusikJelajah>() == null) synth.gameObject.AddComponent<AlasKeramat.AI.MusikJelajah>();
            synth.AssignBakedClips(nama.ToArray(), klip.ToArray());
            EditorUtility.SetDirty(synth);
            Catat("Kenney CC0: " + n + " + OpenGameArt CC0: " + nOga + " rekaman (musik tegang, hujan, monster, napas, jantung, tembakan)" + (hilang > 0 ? " (" + hilang + " belum terunduh)" : ""));
        }
    }
}
