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
            { S.KlipTebasBasah, "knife_slice" }, { S.KlipKetukan, "impact_wood_heavy_000" }, { S.KlipKerikil, "impact_generic_light_000" },
        };

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
            synth.AssignBakedClips(nama.ToArray(), klip.ToArray());
            EditorUtility.SetDirty(synth);
            Catat("Kenney CC0: " + n + " suara rekaman menggantikan sintetis" + (hilang > 0 ? " (" + hilang + " belum terunduh)" : ""));
        }
    }
}
