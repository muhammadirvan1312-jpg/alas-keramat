using System.Collections.Generic;
using AlasKeramat.AI;
using AlasKeramat.Building;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.Core
{
    // H16 + 13 §2-3 (T9): Sutradara Teror - satu-satunya sumber kejadian seram. Mengisi Konteks dari dunia 2x/detik, menjalankan
    // PenjadwalTeror, lalu mengeksekusi efek kejadian (TerrorDirector.Kejadian.cs). Ambien diredam saat MEREMANG, sunyi sebelum kejutan.
    public partial class TerrorDirector : MonoBehaviour
    {
        public const float Tick = 0.5f, JarakGelap = 6f;

        public static PenjadwalTeror.Intensitas Setelan = PenjadwalTeror.Intensitas.Normal;
        public static bool Headset = true;

        PenjadwalTeror _p;
        PenjadwalTeror.Konteks _k;
        float _t;
        AndiFPSController _andi;
        AndiVitalsAndSanity _vital;
        TanganKiri _kiri;
        TimeOfDayClock _jam;
        URPAtmosphereAndWeather _cuaca;
        ProceduralAudioSynth _audio;
        EnemyDirector _dir;
        MusikTempur _musik;

        public PenjadwalTeror.Fase Fase { get { return _p != null ? _p.FaseKini : PenjadwalTeror.Fase.Tenang; } }
        public float Tension { get { return _p != null ? _p.Tension : 0f; } }

        void Awake()
        {
            GameServices.Register(this);
            _p = new PenjadwalTeror(Katalog(), System.Environment.TickCount);
        }

        void Update()
        {
            _t -= Time.deltaTime;
            if (_t > 0f) return;
            _t = Tick;
            try
            {
                Ambil();
                if (_andi == null || _vital == null) return;
                IsiKonteks();
                _p.Setelan = Setelan;
                var e = _p.Langkah(Tick, _k);
                if (e.id != null) Jalankan(e.id);
                TickEfek(Tick);
            }
            catch (System.Exception ex) { Debug.LogError("[AK][TerrorDirector] galat: " + ex.Message); }
        }

        void Ambil()
        {
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            if (_vital == null) _vital = GameServices.Get<AndiVitalsAndSanity>();
            if (_kiri == null) _kiri = GameServices.Get<TanganKiri>();
            if (_jam == null) _jam = GameServices.Get<TimeOfDayClock>();
            if (_cuaca == null) _cuaca = GameServices.Get<URPAtmosphereAndWeather>();
            if (_audio == null) _audio = GameServices.Get<ProceduralAudioSynth>();
            if (_dir == null) _dir = GameServices.Get<EnemyDirector>();
            if (_musik == null) _musik = Object.FindFirstObjectByType<MusikTempur>();
        }

        void IsiKonteks()
        {
            Vector3 a = _andi.transform.position;
            float jam = _jam != null ? _jam.Jam : 12f;
            var k = new PenjadwalTeror.Konteks();
            k.jam = jam; k.hari = _jam != null ? _jam.Hari : 1;
            k.malam = jam >= 19f || jam < 5f;
            k.bawahTanah = a.y < -40f;
            k.malamDarah = _dir != null && _dir.MalamDarah;
            k.sanity = _vital.Sanity;
            k.tidur = _vital.Tidur;
            k.kemarahan = _dir != null ? _dir.Kemarahan : 10f;
            k.bawaCahaya = _kiri != null && (_kiri.OborMenyala || _kiri.SenterMenyala);
            ApiUnggun apiDekat = null; float dApi = 999f;
            foreach (var api in ApiUnggun.Semua) { if (api == null) continue; float d = Vector3.Distance(api.transform.position, a); if (d < dApi) { dApi = d; apiDekat = api; } }
            bool apiMenyala = apiDekat != null && apiDekat.Menyala && dApi <= JarakGelap;
            k.gelap = (k.malam || k.bawahTanah) && !k.bawaCahaya && !apiMenyala;
            k.jarakDariKemah = dApi;
            var deden = GameServices.Get<KangDedenKelvinAI>(); var ningsih = GameServices.Get<NingsihVirginiaAI>();
            bool temanDekat = (deden != null && deden.Bergabung && !deden.Gugur && Vector3.Distance(deden.transform.position, a) <= 10f)
                || (ningsih != null && ningsih.Sikap != NingsihVirginiaAI.SikapNingsih.DiSarang && Vector3.Distance(ningsih.transform.position, a) <= 10f);
            k.diApiBersamaTeman = apiMenyala && dApi <= 8f && temanDekat;
            k.sendirian = !temanDekat;
            var cuaca = _cuaca != null ? _cuaca.Cuaca : WeatherState.Cerah;
            k.siangCerah = !k.malam && cuaca == WeatherState.Cerah;
            k.hujanBadai = cuaca == WeatherState.HujanBadai;
            k.berenang = _andi.Berenang;
            k.jongkokGelap = _andi.Jongkok && k.gelap;
            k.diStruktur = Physics.Raycast(a + Vector3.up * 1f, Vector3.up, 4f, 1 << GameLayers.Structure, QueryTriggerInteraction.Ignore);
            k.headset = Headset;
            k.tempur = _musik != null && _musik.Tegang;
            float dMusuh = 999f, dKunti = 999f, dHantu = 999f, dPocong = 999f;
            foreach (var m in NusantaraEnemyAndMutantAI.Semua) if (m.IsAlive) dMusuh = Mathf.Min(dMusuh, Vector3.Distance(m.transform.position, a));
            foreach (var h in HantuAI.Semua)
            {
                if (!h.IsAlive) continue;
                float d = Vector3.Distance(h.transform.position, a);
                dMusuh = Mathf.Min(dMusuh, d); dHantu = Mathf.Min(dHantu, d);
                if (h.Jenis == EnemyArchetype.Kuntilanak) dKunti = Mathf.Min(dKunti, d);
                if (h.Jenis == EnemyArchetype.PocongHutan || h.Jenis == EnemyArchetype.PocongGua) dPocong = Mathf.Min(dPocong, d);
            }
            k.ancaman = PenjadwalTeror.Ancaman(dMusuh);
            k.kuntiDekat80 = dKunti <= 80f; k.kuntiDekat30 = dKunti <= 30f; k.kuntiAktif120 = dKunti <= 120f; k.hantuDekat20 = dHantu <= 20f; k.pocongTerlihat30 = dPocong >= 30f && dPocong < 90f;
            k.dedenTidurDiKemah = deden != null && deden.Bergabung && !deden.Gugur && apiDekat != null && Vector3.Distance(deden.transform.position, apiDekat.transform.position) < 6f;
            k.ningsihDiKemah = ningsih != null && ningsih.Sikap == NingsihVirginiaAI.SikapNingsih.Kemah;
            k.dekatDesa250 = Vector3.Distance(new Vector3(a.x, 0f, a.z), EnemyDirector.PusatDesa) <= 250f;
            k.dekatGua = DekatGua(a, 40f);
            _k = k;
        }

        public static readonly Vector3[] MulutGua = { new Vector3(256f, 0f, -276f), new Vector3(0f, 0f, 694f), new Vector3(-560f, 0f, 290f) }; // CaveEntrance_1, _3 (Landmark), Tanah Gali Bunker (-280,145) x2
        static bool DekatGua(Vector3 a, float r) { foreach (var g in MulutGua) if (new Vector2(a.x - g.x, a.z - g.z).magnitude <= r) return true; return false; }
    }
}
