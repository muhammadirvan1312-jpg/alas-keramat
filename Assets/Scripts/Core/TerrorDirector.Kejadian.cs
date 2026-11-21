using System.Collections.Generic;
using AlasKeramat.AI;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.UI;
using T = AlasKeramat.Core.PenjadwalTeror.TipeTeror;

namespace AlasKeramat.Core
{
    // 13 §3: katalog kejadian yang aktif di T9 (25 dari 37; sisanya set-piece terskrip T10: E15-E17, E19-E20, E25-E26, E28-E29, E31, E36-E37).
    public partial class TerrorDirector
    {
        [SerializeField] GameObject prefabSosok;     // Pocong tanpa AI (siluet, makin dekat, kedipan)
        [SerializeField] GameObject prefabPengawas;  // Pemimpin Sekte tanpa AI (Ki Rogo mengawasi)
        [SerializeField] Material matBatu, matKain;
        [SerializeField] Image kedip;

        public void Pasang(GameObject sosok, GameObject pengawas, Material batu, Material kain, Image hitam) { prefabSosok = sosok; prefabPengawas = pengawas; matBatu = batu; matKain = kain; kedip = hitam; }

        public static List<PenjadwalTeror.Kejadian> Katalog()
        {
            var d = new List<PenjadwalTeror.Kejadian>();
            d.Add(K("E01", "Kentongan Jauh", T.Ringan, true, k => k.malam));
            d.Add(K("E02", "Langkah Bayangan", T.Ringan, false, k => k.malam && k.sendirian && k.sanity < 70f));
            d.Add(K("E03", "Bisikan Telinga", T.Ringan, false, k => (k.malam || k.bawahTanah) && k.headset));
            d.Add(K("E04", "Tangis Bayi di Hutan", T.Ringan, false, k => k.malam && k.kuntiAktif120));
            d.Add(K("E05", "Tawa Terbalik", T.Ringan, true, k => k.kuntiDekat80));
            d.Add(K("E06", "Kang Deden Memanggil", T.Ringan, false, k => k.malam && k.dedenTidurDiKemah && k.jarakDariKemah > 60f));
            d.Add(K("E07", "Gamelan Tanpa Penabuh", T.Ringan, false, k => k.jam >= 1f && k.jam < 3f && k.dekatDesa250));
            d.Add(K("E08", "Batu di Atap", T.Sedang, false, k => k.malam && k.diStruktur));
            d.Add(K("E09", "Tiga Ketukan", T.Sedang, false, k => k.diStruktur && k.malam));
            d.Add(K("E10", "Napas di Belakang", T.Sedang, false, k => k.jongkokGelap));
            d.Add(K("E11", "Ayam Berkokok Tengah Malam", T.Ringan, false, k => k.jam >= 23f || k.jam < 2f));
            d.Add(K("E12", "Kedasih Berkicau", T.Ringan, false, k => k.jam >= 17.5f || k.jam < 5f));
            d.Add(K("E13", "Siluet di Batas Cahaya", T.Ringan, true, k => k.bawaCahaya && k.malam));
            d.Add(K("E18", "Makin Dekat", T.Sedang, false, k => k.malam && !k.diStruktur));
            d.Add(K("E21", "Tumpukan Batu", T.Ringan, false, k => k.jam >= 5f && k.jam < 9f && k.kemarahan >= 30f && k.jarakDariKemah < 30f));
            d.Add(K("E22", "Boneka Ranting", T.Ringan, false, k => k.jam >= 5f && k.jam < 9f && k.kemarahan >= 50f && k.jarakDariKemah < 30f));
            d.Add(K("E23", "Bunga Kamboja", T.Ringan, true, k => k.kuntiDekat30));
            d.Add(K("E24", "Cahaya Sekarat", T.Ringan, true, k => k.hantuDekat20 && k.bawaCahaya));
            d.Add(K("E27", "Di Antara Kedipan", T.Sedang, false, k => k.sanity < 35f));
            d.Add(K("E30", "Tatapan Ningsih", T.Ringan, false, k => k.ningsihDiKemah && k.malam));
            d.Add(K("E32", "Ki Rogo Mengawasi", T.Sedang, false, k => (k.hari == 4 || k.hari == 6) && k.jam >= 17f && k.jam < 19f));
            d.Add(K("E33", "Mimpi Buruk", T.Keras, false, k => k.tidur && k.sanity < 40f));
            d.Add(K("E34", "Tangan dari Air", T.Keras, false, k => k.berenang && k.malam));
            d.Add(K("E35", "Jeritan dari Gua", T.Ringan, false, k => k.malam && k.dekatGua));
            d.Add(K("E14", "Sosok di Kilat", T.Sedang, false, k => k.hujanBadai));
            d.Add(K("E15", "Bayangan di Air", T.Ringan, false, k => k.malam && k.berenang));
            d.Add(K("E16", "Gema Langkah Gua", T.Ringan, true, k => k.bawahTanah && k.sendirian));
            d.Add(K("E17", "Ranting Patah Dekat", T.Ringan, true, k => k.malam && k.dekatGua));
            d.Add(K("E19", "Pocong di Balik Pohon", T.Sedang, false, k => k.malam && k.pocongTerlihat30));
            d.Add(K("E20", "Suara Sinden Samar", T.Ringan, false, k => k.malam && k.dekatDesa250));
            d.Add(K("E25", "Api Unggun Meredup", T.Ringan, false, k => k.malam && k.hantuDekat20));
            d.Add(K("E26", "Bayangan di Dinding Gua", T.Sedang, false, k => k.bawahTanah && k.bawaCahaya));
            d.Add(K("E28", "Tetesan Darah Langit-Langit", T.Sedang, false, k => k.bawahTanah && k.sanity < 45f));
            d.Add(K("E29", "Pintu Berderit Sendiri", T.Ringan, false, k => k.malam && k.diStruktur));
            d.Add(K("E31", "Isyarat Panik Kang Deden", T.Ringan, false, k => k.malam && k.dedenTidurDiKemah && k.hantuDekat20));
            d.Add(K("E36", "Kabut Berbisik", T.Ringan, true, k => k.hujanBadai && k.sendirian));
            d.Add(K("E37", "Gema Nama Andi", T.Sedang, false, k => k.bawahTanah && k.sanity < 35f));
            return d;
        }

        static PenjadwalTeror.Kejadian K(string id, string nama, T tipe, bool ulang, System.Func<PenjadwalTeror.Konteks, bool> s)
        { return new PenjadwalTeror.Kejadian { id = id, nama = nama, tipe = tipe, ulang = ulang, syarat = s }; }

        struct Tunda { public float t; public System.Action aksi; }
        readonly List<Tunda> _tunda = new List<Tunda>();
        float _tLangkah, _lamaLangkah, _tBerhenti; bool _andiBergerakLalu;
        GameObject _sosok; float _tTatap, _umurSosok; int _majuSosok; bool _sosokMaju;
        float _tKedip;

        void Nanti(float detik, System.Action a) { _tunda.Add(new Tunda { t = Time.time + detik, aksi = a }); }

        Vector3 Arah(float sudut) { return Quaternion.Euler(0f, sudut, 0f) * Vector3.forward; }
        Vector3 Belakang() { var f = _andi.transform.forward; f.y = 0f; return -f.normalized; }

        void Bunyi(string klip, Vector3 pos, float vol, float pitch = 1f)
        {
            if (_audio != null) _audio.Play(klip, pos, AudioCategory.Efek, vol, pitch, 4, 300f);
        }

        void Jalankan(string id)
        {
            Vector3 a = _andi.transform.position;
            switch (id)
            {
                case "E01": { Vector3 d = Arah(Random.value * 360f); Bunyi(ProceduralAudioSynth.KlipKentongan, a + d * 120f, 1f); Nanti(2.5f, () => Bunyi(ProceduralAudioSynth.KlipKentongan, _andi.transform.position - d * 130f, 0.9f, 0.95f)); break; }
                case "E02": _lamaLangkah = 15f; _tBerhenti = 0f; break;
                case "E03": { Vector3 sisi = _andi.transform.right * (Random.value < 0.5f ? -0.6f : 0.6f); Bunyi(ProceduralAudioSynth.KlipBisikan, a + Vector3.up * 1.6f + sisi, 0.7f); break; }
                case "E04": Bunyi(ProceduralAudioSynth.KlipTangisBayi, a + Arah(Random.value * 360f) * 60f, 0.8f); break;
                case "E05": TawaTerbalik(); break;
                case "E06":
                    {
                        Vector3 keKemah = Belakang();
                        foreach (var api in AlasKeramat.Building.ApiUnggun.Semua) if (api != null) { keKemah = (api.transform.position - a).normalized; break; }
                        Bunyi(ProceduralAudioSynth.KlipPanggilan, a - keKemah * 50f, 0.9f);
                        GameEvents.RaiseSubtitle("(Itu suara Kang Deden... tapi Kang Deden kan bisu.)");
                        break;
                    }
                case "E07": Bunyi(ProceduralAudioSynth.KlipGamelanJauh, new Vector3(EnemyDirector.PusatDesa.x, a.y, EnemyDirector.PusatDesa.z), 0.8f); break;
                case "E08": Bunyi(ProceduralAudioSynth.KlipKerikil, a + Vector3.up * 3f, 1f); Getar(1); break;
                case "E09": Bunyi(ProceduralAudioSynth.KlipKetukan, a + _andi.transform.right * 2.5f, 1f); Getar(1); break;
                case "E10": Bunyi(ProceduralAudioSynth.KlipNapasBerat, a + Vector3.up * 1.2f + Belakang() * 0.5f, 0.9f); break;
                case "E11": Bunyi(ProceduralAudioSynth.KlipKokok, a + Arah(Random.value * 360f) * 150f, 0.7f); Nanti(60f, () => PanggilHantu(0)); break;
                case "E12": Bunyi(ProceduralAudioSynth.KlipKedasih, a + Arah(Random.value * 360f) * 40f, 0.8f); Nanti(Random.Range(60f, 120f), () => PanggilHantu(0)); break;
                case "E13": MunculSosok(a + (_andi.transform.forward * 0.6f + _andi.transform.right * (Random.value < 0.5f ? -0.8f : 0.8f)).normalized * 7.5f, false); break;
                case "E14": MunculSosok(a + _andi.transform.forward * 40f, false); Nanti(1.2f, HapusSosok); break;
                case "E18": MunculSosok(a + _andi.transform.forward * 40f + _andi.transform.right * 6f, true); break;
                case "E21": Tumpukan(3, false); break;
                case "E22": Tumpukan(1, true); break;
                case "E23": GameEvents.RaiseSubtitle("(Bau bunga kamboja... di tengah hutan?)"); break;
                case "E24": CahayaSekarat(); break;
                case "E27": _tKedip = 0.01f; Nanti(0.6f, () => MunculSosok(_andi.transform.position + _andi.transform.forward * 3f, false)); Nanti(1.3f, () => { _tKedip = 0.01f; }); Nanti(1.45f, HapusSosok); break;
                case "E30":
                    {
                        Vector3 arah = Arah(Random.value * 360f);
                        GameEvents.RaiseSubtitle("(Ningsih menatap satu titik gelap di hutan. Tanda di lengannya menyala.)");
                        Nanti(20f, () => PanggilHantu(0, _andi.transform.position + arah * 45f));
                        break;
                    }
                case "E32":
                    if (prefabPengawas != null) { var g = Instantiate(prefabPengawas, a + _andi.transform.forward * 80f, Quaternion.LookRotation(-_andi.transform.forward)); Diamkan(g); Tanah(g); Nanti(25f, () => { if (g != null) Destroy(g); }); }
                    break;
                case "E33": _tKedip = 0.01f; Bunyi(ProceduralAudioSynth.KlipStinger, a, 1f); GameEvents.RaiseSubtitle("(Mimpi: kursi helikopter. Penumpang di sebelahmu menoleh... kain kafan.)"); if (_vital != null) _vital.UbahSanity(-6f); Getar(3); break;
                case "E34":
                    {
                        var cc = _andi.GetComponent<CharacterController>();
                        if (cc != null && cc.enabled) cc.Move(Vector3.down * 0.5f);
                        if (_vital != null) _vital.Pakai(20f);
                        Bunyi(ProceduralAudioSynth.KlipStinger, a, 0.8f, 1.2f); Getar(3);
                        GameEvents.RaiseSubtitle("(Sesuatu menarik pergelangan kakimu ke dalam air!)");
                        break;
                    }
                case "E35": foreach (var g in MulutGua) if (new Vector2(a.x - g.x, a.z - g.z).magnitude < 50f) { Bunyi(ProceduralAudioSynth.KlipJeritGua, new Vector3(g.x, a.y, g.z), 1f); break; } break;
                case "E15": Bunyi(ProceduralAudioSynth.KlipNapasBerat, a - Vector3.up * 0.4f, 0.8f); GameEvents.RaiseSubtitle("(Ada bayangan melintas tepat di bawah kakimu di dalam air...)"); break;
                case "E16": Bunyi(ProceduralAudioSynth.NamaLangkah(SurfaceType.BatuGua, 0), a + Belakang() * 4f, 0.85f, 0.9f); break;
                case "E17": Bunyi(ProceduralAudioSynth.KlipKerikil, a + _andi.transform.right * 4f, 0.9f, 0.7f); break;
                case "E19": MunculSosok(a + _andi.transform.forward * 14f + _andi.transform.right * 3f, false); break;
                case "E20": Bunyi(ProceduralAudioSynth.KlipGamelanJauh, a + Arah(Random.value * 360f) * 65f, 0.75f, 1.15f); GameEvents.RaiseSubtitle("(Alunan sinden samar terbawa angin malam...)"); break;
                case "E25": CahayaSekarat(); GameEvents.RaiseSubtitle("(Lidah api unggun mendadak mengecil sejenak...)"); break;
                case "E26": MunculSosok(a + _andi.transform.forward * 6f, false); Nanti(0.9f, HapusSosok); break;
                case "E28": Bunyi(ProceduralAudioSynth.KlipTebasBasah, a + Vector3.up * 2f, 0.5f, 1.3f); if (_vital != null) _vital.UbahSanity(-3f); break;
                case "E29": Bunyi(ProceduralAudioSynth.KlipKetukan, a + _andi.transform.forward * 3f, 0.75f, 0.8f); break;
                case "E31": GameEvents.RaiseSubtitle("(Kang Deden menunjuk gelisah ke luar lingkaran api!)"); break;
                case "E36": Bunyi(ProceduralAudioSynth.KlipBisikan, a + _andi.transform.right * 1.4f, 0.7f, 0.85f); break;
                case "E37": Bunyi(ProceduralAudioSynth.KlipBisikan, a - _andi.transform.right * 1.2f, 0.8f, 0.75f); GameEvents.RaiseSubtitle("("Andi..." — gema memanggil dari lorong gelap.)"); break;
            }
            if (_audio != null && (id == "E14" || id == "E18" || id == "E27")) Bunyi(ProceduralAudioSynth.KlipStinger, a, 0.6f);
        }

        // 13 §4.1: tawa terdengar jauh = ia dekat; terdengar dekat = ia jauh. Diputar 1 m di samping kepala (bukan posisi aslinya).
        public static float VolumeTawaTerbalik(float jarak) { return jarak <= 15f ? 0.25f : jarak <= 40f ? 0.5f : 1f; }

        void TawaTerbalik()
        {
            float d = 999f;
            foreach (var h in HantuAI.Semua) if (h.IsAlive && h.Jenis == EnemyArchetype.Kuntilanak) d = Mathf.Min(d, Vector3.Distance(h.transform.position, _andi.transform.position));
            Bunyi(ProceduralAudioSynth.KlipKuntilanak, _andi.transform.position + Vector3.up * 1.6f + _andi.transform.right * (Random.value < 0.5f ? -1f : 1f), VolumeTawaTerbalik(d), d <= 15f ? 0.8f : 1.1f);
        }

        void PanggilHantu(int idx) { PanggilHantu(idx, Vector3.zero); }
        void PanggilHantu(int idx, Vector3 pos) { if (_dir != null && _andi != null) _dir.PanggilHantu(idx, pos == Vector3.zero ? _andi.transform.position - _andi.transform.forward * 50f : pos); }

        void MunculSosok(Vector3 p, bool maju)
        {
            HapusSosok();
            if (prefabSosok == null) return;
            p.y = WorldTerrainGenerator.SampleHeight(p.x, p.z);
            _sosok = Instantiate(prefabSosok, p, Quaternion.LookRotation(Belakang()));
            Diamkan(_sosok);
            _tTatap = 0f; _umurSosok = 0f; _majuSosok = 0; _sosokMaju = maju;
        }

        void HapusSosok() { if (_sosok != null) Destroy(_sosok); _sosok = null; }

        static void Diamkan(GameObject g)
        {
            foreach (var mb in g.GetComponentsInChildren<MonoBehaviour>()) if (!(mb is ProceduralAnimator)) mb.enabled = false;
            foreach (var c in g.GetComponentsInChildren<Collider>()) c.enabled = false;
        }

        static void Tanah(GameObject g) { var p = g.transform.position; p.y = WorldTerrainGenerator.SampleHeight(p.x, p.z); g.transform.position = p; }

        void Tumpukan(int n, bool boneka)
        {
            Vector3 kemah = _andi.transform.position;
            foreach (var api in AlasKeramat.Building.ApiUnggun.Semua) if (api != null) { kemah = api.transform.position; break; }
            for (int i = 0; i < n; i++)
            {
                Vector3 p = kemah + Arah(i * 120f + Random.value * 30f) * 9f;
                var akar = new GameObject(boneka ? "BonekaRanting" : "TumpukanBatuSekte");
                akar.transform.position = new Vector3(p.x, WorldTerrainGenerator.SampleHeight(p.x, p.z), p.z);
                for (int k = 0; k < (boneka ? 3 : 4); k++)
                {
                    var g = GameObject.CreatePrimitive(boneka ? PrimitiveType.Cylinder : PrimitiveType.Sphere);
                    Destroy(g.GetComponent<Collider>());
                    g.transform.SetParent(akar.transform, false);
                    if (boneka) { g.transform.localScale = k == 0 ? new Vector3(0.04f, 0.6f, 0.04f) : new Vector3(0.03f, 0.3f, 0.03f); g.transform.localPosition = new Vector3(0f, k == 0 ? 0.6f : 0.9f, 0f); g.transform.localRotation = Quaternion.Euler(0f, 0f, k == 0 ? 0f : k == 1 ? 90f : 30f); }
                    else { float s = 0.32f - k * 0.06f; g.transform.localScale = Vector3.one * s; g.transform.localPosition = new Vector3(0f, 0.1f + k * 0.2f, 0f); }
                    var m = boneka && k == 2 ? matKain : boneka ? matKain : matBatu;
                    if (m != null) g.GetComponent<MeshRenderer>().sharedMaterial = m;
                }
            }
            GameEvents.RaiseSubtitle(boneka ? "(Boneka ranting berkain merah tergantung di tepi kemah.)" : "(Tiga tumpukan batu kecil di tepi kemah. Ada yang mengawasi.)");
        }

        void CahayaSekarat()
        {
            if (_kiri == null) return;
            var l = _kiri.GetComponentInChildren<Light>();
            if (l == null) return;
            float asal = l.intensity;
            l.intensity = asal * 0.2f;
            Nanti(0.4f, () => { if (l != null) l.intensity = asal * 0.6f; });
            Nanti(0.9f, () => { if (l != null) l.intensity = asal * 0.15f; });
            Nanti(2f, () => { if (l != null) l.intensity = asal; });
        }

        // Pola getar 13 §7 (Android): 1 = ketuk pendek, 3 = kejutan keras.
        public static void Getar(int kuat)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (PlayerPrefs.GetInt("ak_getar", 1) == 1) Handheld.Vibrate();
#endif
        }

        void TickEfek(float dt)
        {
            for (int i = _tunda.Count - 1; i >= 0; i--) if (Time.time >= _tunda[i].t) { var a = _tunda[i].aksi; _tunda.RemoveAt(i); try { a(); } catch (System.Exception e) { Debug.LogWarning("[AK][TerrorDirector] efek tertunda gagal: " + e.Message); } }
            // E02: langkah membuntuti, berhenti 0,4 s setelah Andi berhenti.
            if (_lamaLangkah > 0f)
            {
                _lamaLangkah -= dt;
                bool gerak = _andi.KecepatanDatar > 0.5f;
                if (gerak) _tBerhenti = 0f; else _tBerhenti += dt;
                _tLangkah -= dt;
                if (_tLangkah <= 0f && (gerak || _tBerhenti < 0.4f)) { _tLangkah = 0.5f; Bunyi(ProceduralAudioSynth.NamaLangkah(SurfaceType.DaunKering, Random.Range(0, 3)), _andi.transform.position + Belakang() * 2.5f, 0.6f, 0.9f); }
            }
            // Sosok: hilang setelah ditatap 1 s; E18 maju 6 m tiap kamera berpaling > 60 derajat (3x) lalu hilang.
            if (_sosok != null)
            {
                _umurSosok += dt;
                var cam = _andi.Kamera;
                float sudut = cam != null ? Vector3.Angle(cam.forward, _sosok.transform.position + Vector3.up - cam.position) : 180f;
                if (sudut < 15f) _tTatap += dt;
                if (!_sosokMaju && (_tTatap >= 1f || _umurSosok > 20f)) HapusSosok();
                else if (_sosokMaju && sudut > 60f && _umurSosok > 1f)
                {
                    _umurSosok = 0f;
                    if (++_majuSosok > 3) HapusSosok();
                    else { Vector3 ke = _andi.transform.position - _sosok.transform.position; ke.y = 0f; _sosok.transform.position += ke.normalized * 6f; Tanah(_sosok); }
                }
            }
        }

        // Kedipan (E27/E33): layar hitam 0,12 s, per frame.
        void LateUpdate()
        {
            if (kedip == null) return;
            if (_tKedip > 0f) { _tKedip += Time.unscaledDeltaTime; bool hitam = _tKedip < 0.13f; if (kedip.enabled != hitam) kedip.enabled = hitam; if (!hitam) _tKedip = 0f; }
            else if (kedip.enabled) kedip.enabled = false;
        }
    }
}
