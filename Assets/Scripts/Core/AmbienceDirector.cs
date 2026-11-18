using System;
using System.Collections.Generic;
using UnityEngine;

namespace AlasKeramat.Core
{
    public enum AmbienSnapshot { Tenang, Meremang, Mencekam, Lega, Tempur, Gua, DalamSaung, BawahAir, Menu }

    public class AmbienceDirector : MonoBehaviour
    {
        private const int PoolSize = 12, ScanBudget = 64, BedCount = 2;
        private const float Radius = 60f, Crossfade = 4f, Transition = 2f;
        private const float ClearHz = 22000f, UnderwaterHz = 400f, MenuHz = 800f;
        private const float BedVolume = 0.22f, ShotVolume = 0.45f;
        private const float MinInterval = 8f, MaxInterval = 24f, QuietRate = 8f;
        private const float SilenceMin = 6f, SilenceMax = 12f;
        private const float WaveMin = 0.1f, WaveMax = 0.6f;
        private const float DayHours = 24f, DefaultHour = 12f;
        [SerializeField] private List<AudioClip> klip = new List<AudioClip>();
        [SerializeField] private Vector3[] posisiEmitter = new Vector3[0];
        [SerializeField] private byte[] jenisEmitter = new byte[0];
        // Indeks emitter mengikuti urutan ini, bukan seluruh KlipAmbien.
        public static readonly string[] JenisOneShot =
        {
            ProceduralAudioSynth.OsJangkrik, ProceduralAudioSynth.OsKatak,
            ProceduralAudioSynth.OsTokek, ProceduralAudioSynth.OsHantu,
            ProceduralAudioSynth.OsTonggeret, ProceduralAudioSynth.OsPagi,
            ProceduralAudioSynth.OsAyam, ProceduralAudioSynth.OsKedasih,
            ProceduralAudioSynth.OsBambu, ProceduralAudioSynth.OsKepak,
            ProceduralAudioSynth.OsRantai, ProceduralAudioSynth.OsRadio,
            ProceduralAudioSynth.OsGamelan
        };
        private readonly Dictionary<string, AudioClip> bank = new Dictionary<string, AudioClip>();
        private AudioSource[] beds, shots;
        private AudioLowPassFilter[] filters;
        private int[] owners;
        private float[] next, muteFrom, muteUntil;
        private int cursor, active;
        private float blend = 1f, gain = 1f, cutoff = ClearHz, rain = 1f;
        private TimeOfDayClock clock;
        private bool fault;
        public int JumlahEmitter { get { return posisiEmitter == null ? 0 : posisiEmitter.Length; } }
        public string BedAktif { get; private set; } = "";
        public AmbienSnapshot Snapshot { get; private set; }

        public void SetKlip(IList<AudioClip> daftar)
        {
            var copy = new List<AudioClip>();
            if (daftar != null)
                for (int i = 0; i < daftar.Count; i++)
                    if (daftar[i] != null) copy.Add(daftar[i]);
            klip = copy;
            bank.Clear();
            for (int i = 0; i < klip.Count; i++) bank[klip[i].name] = klip[i];
            BedAktif = "";
            if (beds != null)
                for (int i = 0; i < beds.Length; i++) beds[i].Stop();
        }

        public void SetEmitter(Vector3[] posisi, byte[] jenis)
        {
            int n = posisi == null || jenis == null ? 0 : Mathf.Min(posisi.Length, jenis.Length);
            posisiEmitter = new Vector3[n]; jenisEmitter = new byte[n];
            if (n > 0) { Array.Copy(posisi, posisiEmitter, n); Array.Copy(jenis, jenisEmitter, n); }
            next = new float[n]; muteFrom = new float[n]; muteUntil = new float[n];
            for (int i = 0; i < n; i++)
            {
                next[i] = Time.time + UnityEngine.Random.Range(0f, MaxInterval);
                muteFrom[i] = float.PositiveInfinity;
            }
            cursor = 0;
            if (shots != null)
                for (int i = 0; i < shots.Length; i++) { shots[i].Stop(); owners[i] = -1; }
        }

        private void Awake()
        {
            try
            {
                GameServices.Register(this);
                beds = new AudioSource[BedCount]; shots = new AudioSource[PoolSize];
                owners = new int[PoolSize]; filters = new AudioLowPassFilter[BedCount + PoolSize];
                for (int i = 0; i < filters.Length; i++)
                {
                    var go = new GameObject("Ambien_" + i);
                    go.transform.SetParent(transform, false);
                    var source = go.AddComponent<AudioSource>();
                    source.playOnAwake = false; source.dopplerLevel = 0f;
                    source.loop = i < BedCount; source.spatialBlend = i < BedCount ? 0f : 1f;
                    source.rolloffMode = AudioRolloffMode.Logarithmic;
                    source.minDistance = 2f; source.maxDistance = Radius;
                    filters[i] = go.AddComponent<AudioLowPassFilter>();
                    if (i < BedCount) beds[i] = source;
                    else { shots[i - BedCount] = source; owners[i - BedCount] = -1; }
                }
                SetKlip(klip); SetEmitter(posisiEmitter, jenisEmitter);
            }
            catch (Exception e) { Gagal(e); }
        }
        private void Start() { clock = GameServices.Get<TimeOfDayClock>(); }
        private void OnEnable() { GameEvents.OnWeatherChanged += Cuaca; }
        private void OnDisable()
        {
            GameEvents.OnWeatherChanged -= Cuaca;
            if (beds != null) foreach (var s in beds) if (s != null) s.Stop();
            if (shots != null) foreach (var s in shots) if (s != null) s.Stop();
            BedAktif = "";
        }
        private void Cuaca(WeatherState w)
        {
            rain = w == WeatherState.HujanBadai ? 0.35f : w == WeatherState.Gerimis ? 0.7f : 1f;
        }
        public static string PilihBed(Biome biome, float jam)
        {
            if (float.IsNaN(jam) || float.IsInfinity(jam)) jam = DefaultHour;
            jam = Mathf.Repeat(jam, DayHours);
            if (biome == Biome.Pantai || biome == Biome.Laut) return ProceduralAudioSynth.BedPantai;
            if (biome == WorldTerrainGenerator.AlasWingit) return ProceduralAudioSynth.BedWingit;
            if (biome == WorldTerrainGenerator.PabrikGula) return ProceduralAudioSynth.BedPabrik;
            if (biome == WorldTerrainGenerator.GunungKapur || biome == Biome.TebingKapur)
                return ProceduralAudioSynth.BedTebing;
            if (biome == WorldTerrainGenerator.RawaBakau && (jam >= 18.5f || jam < 4f))
                return ProceduralAudioSynth.BedRawa;
            if (jam >= 4f && jam < 7f) return ProceduralAudioSynth.BedPagi;
            if (jam >= 7f && jam < 16f) return ProceduralAudioSynth.BedSiang;
            if (jam >= 16f && jam < 18.5f) return ProceduralAudioSynth.BedSenja;
            return jam >= 1.5f && jam < 4f ? ProceduralAudioSynth.BedDini : ProceduralAudioSynth.BedMalam;
        }
        private void Update()
        {
            if (fault) return;
            try { Tick(); } catch (Exception e) { Gagal(e); }
        }
        private void Gagal(Exception e)
        {
            if (!fault) Debug.LogError("[AK][AmbienceDirector] Modul dihentikan: " + e.Message);
            fault = true; enabled = false;
        }
        private void Tick()
        {
            var listener = Camera.main;
            if (listener == null) return;
            float jam = clock != null ? clock.Jam : DefaultHour;
            Vector3 p = listener.transform.position;
            string name = PilihBed(WorldTerrainGenerator.GetBiome(p.x, p.z), jam);
            AudioClip clip;
            if (name != BedAktif && blend >= 1f && bank.TryGetValue(name, out clip) && clip != null)
            {
                active = 1 - active;
                beds[active].clip = clip; beds[active].volume = 0f; beds[active].Play();
                BedAktif = name; blend = 0f;
            }
            blend = Mathf.MoveTowards(blend, 1f, Time.deltaTime / Crossfade);
            float target = Snapshot == AmbienSnapshot.Meremang ? 0.708f :
                Snapshot == AmbienSnapshot.Mencekam ? 0.355f :
                Snapshot == AmbienSnapshot.Tempur ? 0.251f :
                Snapshot == AmbienSnapshot.BawahAir ? 0.126f :
                Snapshot == AmbienSnapshot.Gua ? 0.4f :
                Snapshot == AmbienSnapshot.DalamSaung ? 0.6f : 1f;
            float hz = Snapshot == AmbienSnapshot.BawahAir ? UnderwaterHz :
                Snapshot == AmbienSnapshot.Menu ? MenuHz :
                Snapshot == AmbienSnapshot.DalamSaung ? 3000f :
                Snapshot == AmbienSnapshot.Gua ? 2000f : ClearHz;
            gain = Mathf.MoveTowards(gain, target, Time.deltaTime / Transition);
            cutoff = Mathf.MoveTowards(cutoff, hz, ClearHz * Time.deltaTime / Transition);
            float volume = gain * rain * ProceduralAudioSynth.VolMaster * (p.y < -40f ? 0f : 1f);
            beds[active].volume = BedVolume * blend * volume;
            beds[1 - active].volume = BedVolume * (1f - blend) * volume;
            if (blend >= 1f) beds[1 - active].Stop();
            for (int i = 0; i < filters.Length; i++) filters[i].cutoffFrequency = cutoff;
            for (int i = 0; i < shots.Length; i++)
            {
                int owner = owners[i];
                bool silent = owner >= 0 && Time.time >= muteFrom[owner] && Time.time < muteUntil[owner];
                if (silent || (owner >= 0 && !Boleh(jenisEmitter[owner], jam))) shots[i].Stop();
                shots[i].volume = ShotVolume * volume;
            }
            int budget = Mathf.Min(ScanBudget, JumlahEmitter);
            for (int k = 0; k < budget; k++)
            {
                int i = cursor; cursor = (cursor + 1) % JumlahEmitter;
                if (Time.time < next[i]) continue;
                float rate = jam >= 2f && jam < 3f ? QuietRate : 1f;
                if (jenisEmitter[i] == 4 && jam >= 17.5f && jam < 18f) rate *= 0.35f;
                if (Snapshot == AmbienSnapshot.Meremang) rate *= 2f;
                next[i] = Time.time + UnityEngine.Random.Range(MinInterval, MaxInterval) * rate;
                if (Time.time >= muteFrom[i] && Time.time < muteUntil[i]) continue;
                if ((posisiEmitter[i] - p).sqrMagnitude > Radius * Radius || !Boleh(jenisEmitter[i], jam)) continue;
                PlayShot(jenisEmitter[i], posisiEmitter[i], i);
            }
        }
        private static bool Boleh(int jenis, float jam)
        {
            if (jenis < 0 || jenis >= JenisOneShot.Length) return false;
            if (jenis <= 3) return jam >= 19f || jam < 4f;
            if (jenis == 4) return jam >= 7f && jam < 18f;
            if (jenis == 5) return jam >= 5.5f && jam < 7f;
            if (jenis == 6 || jenis == 7) return jam >= 4.5f && jam < 5.5f;
            return jenis != 9; // Kepakan dipicu kejadian, bukan jadwal.
        }
        private void PlayShot(int jenis, Vector3 pos, int owner)
        {
            if (shots == null || jenis < 0 || jenis >= JenisOneShot.Length) return;
            AudioClip clip;
            if (!bank.TryGetValue(JenisOneShot[jenis], out clip) || clip == null) return;
            for (int i = 0; i < shots.Length; i++)
            {
                if (shots[i].isPlaying) continue;
                shots[i].transform.position = pos; shots[i].clip = clip;
                shots[i].pitch = UnityEngine.Random.Range(0.94f, 1.06f);
                shots[i].volume = ShotVolume * gain * rain * ProceduralAudioSynth.VolMaster;
                owners[i] = owner; shots[i].Play(); return;
            }
        }
        public void PancarkanHening(Vector3 pusat, float radius, float lamaDetik)
        {
            if (next == null || float.IsNaN(radius) || float.IsNaN(lamaDetik)) return;
            radius = Mathf.Max(0f, radius);
            float duration = Mathf.Clamp(lamaDetik, SilenceMin, SilenceMax);
            // Sort hanya saat kejadian; Update tidak mengalokasikan.
            var ids = new List<int>();
            for (int i = 0; i < JumlahEmitter; i++)
                if ((posisiEmitter[i] - pusat).sqrMagnitude <= radius * radius) ids.Add(i);
            ids.Sort((a, b) => (posisiEmitter[a] - pusat).sqrMagnitude.CompareTo((posisiEmitter[b] - pusat).sqrMagnitude));
            float start = Time.time;
            for (int k = 0; k < ids.Count; k++)
            {
                int i = ids[k]; start += UnityEngine.Random.Range(WaveMin, WaveMax);
                if (muteUntil[i] <= Time.time) muteFrom[i] = start;
                else muteFrom[i] = Mathf.Min(muteFrom[i], start);
                muteUntil[i] = Mathf.Max(muteUntil[i], start + duration);
                next[i] = Mathf.Max(next[i], muteUntil[i]);
            }
        }
        public void BurungKaget(Vector3 pos) { PlayShot(9, pos, -1); }
        public void SetSnapshot(AmbienSnapshot s)
        {
            if ((int)s < 0 || (int)s > (int)AmbienSnapshot.Menu) return;
            Snapshot = s;
        }
    }
}
