using System;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Musik jelajah ala survival-horror: sesekali (jeda acak 4-9 menit) satu lagu pelan sesuai siang/malam,
    // memudar keluar saat ada ancaman atau musik tempur, senyap di gua. Rekaman CC0 dari bank synth.
    public class MusikJelajah : MonoBehaviour
    {
        public const string KlipSiang = "rekaman_musik_siang", KlipMalam = "rekaman_musik_malam", KlipMisteri = "rekaman_musik_misteri";
        public const float JedaMin = 240f, JedaMaks = 540f, JedaAwal = 70f, Volume = 0.38f, Pudar = 0.12f, BatasGua = -40f;
        AudioSource _src;
        ProceduralAudioSynth _synth;
        TimeOfDayClock _jam;
        MusikTempur _tempur;
        float _tunggu = JedaAwal, _vol, _tCek;
        bool _gagal, _aman = true;

        public static string PilihKlip(float jam, int acak)
        {
            bool malam = jam >= 18.5f || jam < 5f;
            if (acak % 4 == 0) return KlipMisteri;
            return malam ? KlipMalam : KlipSiang;
        }

        void Update()
        {
            if (_gagal) return;
            try
            {
                if (_src == null)
                {
                    _synth = GetComponent<ProceduralAudioSynth>();
                    if (_synth == null) { enabled = false; return; }
                    _src = gameObject.AddComponent<AudioSource>();
                    _src.loop = false; _src.playOnAwake = false; _src.spatialBlend = 0f; _src.priority = 30; _src.volume = 0f;
                }
                if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
                if (_tempur == null) _tempur = FindFirstObjectByType<MusikTempur>();
                var cam = Camera.main;
                _tCek -= Time.deltaTime;
                if (_tCek <= 0f) { _tCek = 1f; _aman = cam == null || (cam.transform.position.y > BatasGua && !MusikTempur.AdaAncaman(cam.transform.position)); }
                bool boleh = _aman && (_tempur == null || !_tempur.Tegang);
                if (_src.isPlaying)
                {
                    float target = boleh ? Volume : 0f;
                    _vol = Mathf.MoveTowards(_vol, target, Pudar * Time.deltaTime);
                    if (!boleh && _vol <= 0.001f) _src.Stop();
                }
                else if (boleh)
                {
                    _tunggu -= Time.deltaTime;
                    if (_tunggu <= 0f)
                    {
                        _tunggu = UnityEngine.Random.Range(JedaMin, JedaMaks);
                        var c = _synth.AmbilRekaman(PilihKlip(_jam != null ? _jam.Jam : 12f, UnityEngine.Random.Range(0, 1000)));
                        if (c != null) { _src.clip = c; _vol = 0f; _src.Play(); }
                    }
                }
                _src.volume = _vol * ProceduralAudioSynth.VolumeKategori(AudioCategory.Musik);
            }
            catch (Exception e) { _gagal = true; Debug.LogError("[AK][MusikJelajah] Gagal: " + e.Message); }
        }
    }
}
