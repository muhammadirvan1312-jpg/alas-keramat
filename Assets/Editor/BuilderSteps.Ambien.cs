using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;

namespace AlasKeramat.EditorTools
{
    // Langkah 3e (T2d): ambien hidup 21 §5 — AmbienceDirector + emitter titik per bioma (+ titik khusus landmark).
    public static partial class NusantaraMasterBuilder
    {
        public static int JumlahEmitter;

        const float JarakGridEmitter = 52f, Setengah = 1000f, AcakEmitter = 20f, TinggiEmitter = 2.5f, TinggiMinDarat = 0.6f;
        const int BenihEmitter = 52021;
        // Indeks = urutan AmbienceDirector.JenisOneShot.
        const byte Jangkrik = 0, Katak = 1, Tokek = 2, Hantu = 3, Tonggeret = 4, Pagi = 5, Ayam = 6, Kedasih = 7,
            Bambu = 8, Kepak = 9, Rantai = 10, Radio = 11, Gamelan = 12;

        static byte[] KandidatEmitter(Biome b)
        {
            if (b == Biome.Pantai) return new[] { Jangkrik, Pagi, Kepak };
            if (b == Biome.HutanBambu) return new[] { Bambu, Bambu, Jangkrik, Kedasih, Tokek };
            if (b == Biome.TepiSungai) return new[] { Katak, Katak, Jangkrik, Pagi, Kedasih };
            if (b == Biome.Desa) return new[] { Tokek, Ayam, Jangkrik, Hantu };
            if (b == Biome.SawitTerbakar) return new[] { Kepak, Hantu, Jangkrik };
            if (b == Biome.TebingKapur || b == WorldTerrainGenerator.GunungKapur) return new[] { Kepak, Pagi, Tonggeret };
            if (b == WorldTerrainGenerator.RawaBakau) return new[] { Katak, Katak, Katak, Jangkrik, Hantu };
            if (b == WorldTerrainGenerator.PabrikGula) return new[] { Rantai, Rantai, Tokek, Hantu };
            if (b == WorldTerrainGenerator.LadangTebu) return new[] { Jangkrik, Jangkrik, Tonggeret, Ayam };
            if (b == WorldTerrainGenerator.AlasWingit) return new[] { Gamelan, Kedasih, Hantu, Jangkrik };
            return new[] { Jangkrik, Tokek, Hantu, Tonggeret, Pagi, Ayam, Kedasih, Jangkrik };
        }

        static void LangkahAmbien()
        {
            var pos = new List<Vector3>(1700);
            var jenis = new List<byte>(1700);
            var rng = new System.Random(BenihEmitter);
            int n = Mathf.RoundToInt(2f * Setengah / JarakGridEmitter);
            for (int ix = 0; ix < n; ix++)
                for (int iz = 0; iz < n; iz++)
                {
                    float x = -Setengah + (ix + 0.5f) * JarakGridEmitter + (float)(rng.NextDouble() * 2 - 1) * AcakEmitter;
                    float z = -Setengah + (iz + 0.5f) * JarakGridEmitter + (float)(rng.NextDouble() * 2 - 1) * AcakEmitter;
                    float h = WorldTerrainGenerator.SampleHeight(x, z);
                    if (h < TinggiMinDarat) continue;
                    var k = KandidatEmitter(WorldTerrainGenerator.GetBiome(x, z));
                    pos.Add(new Vector3(x, h + TinggiEmitter, z));
                    jenis.Add(k[rng.Next(k.Length)]);
                }
            // Titik khusus 21 §5: pabrik (rantai), balai kampung (radio statis), Alas Wingit (gamelan dari tanah).
            Khusus(pos, jenis, 700f, -540f, Rantai, 6, 25f, rng);
            Khusus(pos, jenis, 630f, 600f, Radio, 2, 8f, rng);
            Khusus(pos, jenis, -760f, -40f, Gamelan, 8, 120f, rng);

            var go = Anak(_sistem, "AmbienceDirector");
            var dir = go.AddComponent<AmbienceDirector>();
            var klip = new List<AudioClip>();
            foreach (var nama in ProceduralAudioSynth.KlipAmbien)
            {
                AudioClip a;
                if (!_klip.TryGetValue(nama, out a)) a = AssetDatabase.LoadAssetAtPath<AudioClip>(FolderAudio + "/" + nama + ".wav");
                if (a != null) klip.Add(a); else Debug.LogError("[AK][Builder] klip ambien hilang: " + nama);
            }
            dir.SetKlip(klip);
            dir.SetEmitter(pos.ToArray(), jenis.ToArray());
            EditorUtility.SetDirty(dir);
            JumlahEmitter = dir.JumlahEmitter;
            Catat("Ambien: " + klip.Count + " klip, " + JumlahEmitter + " emitter titik");
        }

        static void Khusus(List<Vector3> pos, List<byte> jenis, float cx, float cz, byte j, int jumlah, float sebar, System.Random rng)
        {
            for (int i = 0; i < jumlah; i++)
            {
                float x = cx + (float)(rng.NextDouble() * 2 - 1) * sebar, z = cz + (float)(rng.NextDouble() * 2 - 1) * sebar;
                pos.Add(new Vector3(x, Mathf.Max(0f, WorldTerrainGenerator.SampleHeight(x, z)) + TinggiEmitter, z));
                jenis.Add(j);
            }
        }
    }
}
