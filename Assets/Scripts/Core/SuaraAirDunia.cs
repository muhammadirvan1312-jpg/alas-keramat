using System;
using UnityEngine;

namespace AlasKeramat.Core
{
    // Suara gemericik sungai & deburan ombak pantai yang mengikuti posisi kamera secara mulus.
    public class SuaraAirDunia : MonoBehaviour
    {
        public const float LajuUbah = 0.35f;
        AudioSource _src;
        float _vol;
        bool _gagal;

        public static float HitungVolumeAir(float jarakSungaiMeter, float tinggiTanah, bool bawahTanah)
        {
            if (bawahTanah) return 0.12f;
            float vSungai = 1f - Mathf.Clamp01((jarakSungaiMeter - 6f) / 48f);
            float vPantai = tinggiTanah <= 4.2f ? 1f - Mathf.Clamp01((tinggiTanah - 0.5f) / 3.7f) : 0f;
            return Mathf.Clamp01(Mathf.Max(vSungai * 0.42f, vPantai * 0.48f));
        }

        public static float JarakSungaiTerdekatMeter(Vector3 pos)
        {
            float x03 = pos.x / WorldTerrainGenerator.SkalaXZ;
            float z03 = pos.z / WorldTerrainGenerator.SkalaXZ;
            float best = float.MaxValue;
            var daftar = WorldTerrainGenerator.SemuaSungai;
            for (int i = 0; i < daftar.Length; i++)
            {
                float wl, w, d;
                float dist03 = WorldTerrainGenerator.JarakSungai(daftar[i], x03, z03, out wl, out w, out d);
                float tepi = Mathf.Max(0f, (dist03 - w * 0.5f) * WorldTerrainGenerator.SkalaXZ);
                if (tepi < best) best = tepi;
            }
            return best;
        }

        void OnDisable()
        {
            if (_src != null) _src.Stop();
        }

        void Update()
        {
            if (_gagal) return;
            try
            {
                if (_src == null)
                {
                    var synth = GetComponent<ProceduralAudioSynth>();
                    var c = synth != null ? (synth.AmbilRekaman(ProceduralAudioSynth.BedPantai) ?? synth.GetClip(ProceduralAudioSynth.BedPantai)) : null;
                    if (c == null) { enabled = false; return; }
                    var go = new GameObject("SuaraAirDuniaLoop");
                    go.transform.SetParent(transform, false);
                    _src = go.AddComponent<AudioSource>();
                    _src.clip = c; _src.loop = true; _src.playOnAwake = false; _src.spatialBlend = 0f; _src.volume = 0f; _src.priority = 55;
                }
                var cam = Camera.main;
                if (cam == null) return;
                Vector3 p = cam.transform.position;
                bool bawah = WorldTerrainGenerator.DiBawahTanah(p.y);
                float h = bawah ? 0f : WorldTerrainGenerator.SampleHeight(p.x, p.z);
                float target = HitungVolumeAir(JarakSungaiTerdekatMeter(p), h, bawah);
                _vol = Mathf.MoveTowards(_vol, target, LajuUbah * Time.deltaTime);
                _src.volume = _vol * ProceduralAudioSynth.VolumeKategori(AudioCategory.Ambien);
                if (_vol > 0.002f) { if (!_src.isPlaying) _src.Play(); }
                else if (_src.isPlaying) _src.Stop();
            }
            catch (Exception e) { _gagal = true; Debug.LogError("[AK][SuaraAirDunia] Gagal: " + e.Message); }
        }
    }
}
