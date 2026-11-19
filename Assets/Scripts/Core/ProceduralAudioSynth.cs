using System;
using System.Collections.Generic;
using UnityEngine;

namespace AlasKeramat.Core
{
    public enum AudioCategory { Efek = 0, Ambien = 1, Musik = 2, UI = 3 }

    // Synth audio prosedural: resep klip (docs/gdd/10), klip dipanggang Builder, pool 24 suara.
    public partial class ProceduralAudioSynth : MonoBehaviour
    {
        public const int Suara3D = 16, Suara2D = 8;
        public const float MinJarak3D = 2f, MaksJarak3D = 60f, MaksJarakKentongan = 120f;

        public const string KlipTebang = "Tebang", KlipPohonTumbang = "PohonTumbang", KlipKentongan = "Kentongan",
            KlipKuntilanak = "Kuntilanak", KlipPocong = "Pocong", KlipUIKlik = "UIKlik", KlipAmbilItem = "AmbilItem",
            KlipAmbilKayu = "AmbilKayu", KlipGuruh = "Guruh";

        // Daftar klip wajib T1 (Builder memanggang semuanya).
        public static readonly string[] KlipWajib = { KlipTebang, KlipPohonTumbang, KlipKentongan, KlipKuntilanak, KlipPocong, KlipUIKlik, KlipAmbilItem, KlipAmbilKayu, KlipGuruh };

        public static float VolMaster = 1f;
        static readonly float[] _volKategori = { 1f, 1f, 1f, 1f };

        [SerializeField] string[] clipNames = new string[0];
        [SerializeField] AudioClip[] clips = new AudioClip[0];

        readonly Dictionary<string, AudioClip> _peta = new Dictionary<string, AudioClip>();
        AudioSource[] _pool3D, _pool2D;
        int[] _prioritas3D, _prioritas2D;
        float[] _mulai3D, _mulai2D;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatis() { VolMaster = 1f; for (int i = 0; i < _volKategori.Length; i++) _volKategori[i] = 1f; }

        public static void SetVolume(AudioCategory k, float v) { _volKategori[(int)k] = Mathf.Clamp01(v); }

        // Dipanggil Builder setelah memanggang WAV.
        public void AssignBakedClips(string[] names, AudioClip[] baked)
        {
            clipNames = names; clips = baked; _peta.Clear();
        }

        public int BakedClipCount { get { return clips == null ? 0 : clips.Length; } }

        void Awake()
        {
            GameServices.Register(this);
            for (int i = 0; i < clipNames.Length && i < clips.Length; i++)
                if (clips[i] != null) _peta[clipNames[i]] = clips[i];
            BuatPool();
        }

        void BuatPool()
        {
            _pool3D = new AudioSource[Suara3D]; _prioritas3D = new int[Suara3D]; _mulai3D = new float[Suara3D];
            _pool2D = new AudioSource[Suara2D]; _prioritas2D = new int[Suara2D]; _mulai2D = new float[Suara2D];
            for (int i = 0; i < Suara3D; i++)
            {
                var go = new GameObject("Suara3D_" + i); go.transform.SetParent(transform, false);
                var s = go.AddComponent<AudioSource>();
                s.playOnAwake = false; s.spatialBlend = 1f; s.rolloffMode = AudioRolloffMode.Logarithmic;
                s.minDistance = MinJarak3D; s.maxDistance = MaksJarak3D; s.dopplerLevel = 0f;
                _pool3D[i] = s;
            }
            for (int i = 0; i < Suara2D; i++)
            {
                var go = new GameObject("Suara2D_" + i); go.transform.SetParent(transform, false);
                var s = go.AddComponent<AudioSource>();
                s.playOnAwake = false; s.spatialBlend = 0f;
                _pool2D[i] = s;
            }
        }

        // Klip dari hasil panggang; bila hilang dibuat saat runtime (cadangan) + peringatan.
        public AudioClip GetClip(string nama)
        {
            AudioClip c;
            if (_peta.TryGetValue(nama, out c) && c != null) return c;
            int sr;
            float[] data = Generate(nama, 0, out sr);
            if (data == null) { Debug.LogWarning("[AK][Audio] Resep klip tidak dikenal: " + nama); return null; }
            c = AudioClip.Create(nama, data.Length, 1, sr, false);
            c.SetData(data, 0);
            _peta[nama] = c;
            Debug.LogWarning("[AK][Audio] Klip " + nama + " tidak dipanggang Builder, dibuat saat runtime.");
            return c;
        }

        AudioSource Ambil(bool tigaD, int prioritas)
        {
            var pool = tigaD ? _pool3D : _pool2D;
            var pr = tigaD ? _prioritas3D : _prioritas2D;
            var mulai = tigaD ? _mulai3D : _mulai2D;
            if (pool == null) return null;
            int pilih = -1;
            for (int i = 0; i < pool.Length; i++) if (!pool[i].isPlaying) { pilih = i; break; }
            if (pilih < 0)
            {
                // Curi suara berprioritas terendah yang tertua.
                for (int i = 0; i < pool.Length; i++)
                    if (pr[i] <= prioritas && (pilih < 0 || pr[i] < pr[pilih] || (pr[i] == pr[pilih] && mulai[i] < mulai[pilih]))) pilih = i;
                if (pilih < 0) return null;
            }
            pr[pilih] = prioritas; mulai[pilih] = Time.unscaledTime;
            return pool[pilih];
        }

        public AudioSource Play(string nama, Vector3? posisi, AudioCategory kategori, float volume = 1f, float pitch = 1f, int prioritas = 1, float maksJarak = MaksJarak3D)
        {
            var clip = GetClip(nama);
            if (clip == null) return null;
            var s = Ambil(posisi.HasValue, kategori == AudioCategory.UI ? int.MaxValue : prioritas);
            if (s == null) return null;
            if (posisi.HasValue) { s.transform.position = posisi.Value; s.maxDistance = maksJarak; }
            s.clip = clip; s.pitch = pitch;
            s.volume = volume * VolMaster * _volKategori[(int)kategori];
            s.Play();
            return s;
        }

        // Tombol uji & UI memanggil dengan nama klip (2D).
        public void PlayByName(string nama) { Play(nama, null, nama == KlipUIKlik || nama == KlipAmbilItem ? AudioCategory.UI : AudioCategory.Efek); }

        public void PlayWoodChop(Vector3 p) { Play(KlipTebang, p, AudioCategory.Efek, 1f, UnityEngine.Random.Range(0.92f, 1.08f), 2); }
        public void PlayTreeCrackAndFall(Vector3 p) { Play(KlipPohonTumbang, p, AudioCategory.Efek, 1f, 1f, 3); }
        public void PlayKentonganBamboo(Vector3 p) { Play(KlipKentongan, p, AudioCategory.Efek, 1f, UnityEngine.Random.Range(0.9f, 1.1f), 3, MaksJarakKentongan); }
        public void PlayKuntilanakLaugh(Vector3 p) { Play(KlipKuntilanak, p, AudioCategory.Efek, 1f, 1f, 4); }
        public void PlayPocongHopThud(Vector3 p) { Play(KlipPocong, p, AudioCategory.Efek, 1f, 1f, 3); }
        public void PlayUIClick() { Play(KlipUIKlik, null, AudioCategory.UI); }
        public void PlayItemPickup() { Play(KlipAmbilItem, null, AudioCategory.UI); }
        public void PlayLogPickup(Vector3 p) { Play(KlipAmbilKayu, p, AudioCategory.Efek, 1f, 1f, 1); }

        // Resep klip berdasarkan nama. Mengembalikan null bila tidak dikenal.
        public static float[] Generate(string nama, int variasi, out int sr)
        {
            sr = Dsp.SrSfx;
            var rng = Dsp.Rng.DariNama(nama, variasi);
            float[] b;
            switch (nama)
            {
                case KlipTebang: b = ResepTebang(sr, ref rng); break;
                case KlipPohonTumbang: b = ResepPohonTumbang(sr, ref rng); break;
                case KlipKentongan: b = ResepKentongan(sr, ref rng); break;
                case KlipKuntilanak: b = ResepKuntilanak(sr, ref rng); break;
                case KlipPocong: b = ResepPocong(sr, ref rng); break;
                case KlipUIKlik: b = ResepKlik(sr); break;
                case KlipAmbilItem: b = ResepAmbilItem(sr); break;
                case KlipAmbilKayu: b = ResepAmbilKayu(sr, ref rng); break;
                case KlipGuruh: b = ResepGuruh(sr, ref rng); break;
                default:
                    var amb = GenerateAmbien(nama, variasi, out sr);
                    if (amb != null) return amb;
                    return GenerateFoley(nama, variasi, out sr);
            }
            Dsp.Finalize(b, sr);
            return b;
        }
    }
}
