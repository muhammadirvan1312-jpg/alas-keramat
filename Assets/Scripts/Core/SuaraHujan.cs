using System;
using UnityEngine;

namespace AlasKeramat.Core
{
    // Suara hujan rekaman CC0 yang mengikuti cuaca: gerimis pelan, badai keras, senyap di dalam gua.
    public class SuaraHujan : MonoBehaviour
    {
        public const float VolGerimis = 0.32f, VolBadai = 0.85f, LajuUbah = 0.25f, BatasGua = -40f;
        AudioSource _src;
        float _target, _vol;
        bool _gagal;

        public static float TargetUntuk(WeatherState w)
        {
            return w == WeatherState.HujanBadai ? VolBadai : w == WeatherState.Gerimis ? VolGerimis : 0f;
        }

        void OnEnable() { GameEvents.OnWeatherChanged += Terima; }
        void OnDisable() { GameEvents.OnWeatherChanged -= Terima; if (_src != null) _src.Stop(); }
        void Terima(WeatherState w) { _target = TargetUntuk(w); }

        void Update()
        {
            if (_gagal) return;
            try
            {
                if (_src == null)
                {
                    var synth = GetComponent<ProceduralAudioSynth>();
                    var c = synth != null ? synth.AmbilRekaman(ProceduralAudioSynth.KlipHujan) : null;
                    if (c == null) { enabled = false; return; }
                    _src = gameObject.AddComponent<AudioSource>();
                    _src.clip = c; _src.loop = true; _src.playOnAwake = false; _src.spatialBlend = 0f; _src.volume = 0f; _src.priority = 40;
                }
                var cam = Camera.main;
                float t = cam != null && cam.transform.position.y < BatasGua ? 0f : _target;
                _vol = Mathf.MoveTowards(_vol, t, LajuUbah * Time.deltaTime);
                _src.volume = _vol * ProceduralAudioSynth.VolumeKategori(AudioCategory.Ambien);
                if (_vol > 0.001f) { if (!_src.isPlaying) _src.Play(); }
                else if (_src.isPlaying) _src.Stop();
            }
            catch (Exception e) { _gagal = true; Debug.LogError("[AK][SuaraHujan] Gagal: " + e.Message); }
        }
    }
}
