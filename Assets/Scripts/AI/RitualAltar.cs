using System.Collections;
using System.Collections.Generic;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using AlasKeramat.UI;
using UnityEngine;
namespace AlasKeramat.AI
{
    // Altar Segel Beringin (03 Zona 5, 04 §7.6, 06 §11): TANCAPKAN PAKU x3 -> siege 60 detik -> cahaya emas, semua jadi abu,
    // matahari terbit dipercepat, suar hijau di Landasan SAR, M13 dimulai. Andi pingsan saat siege = ritual gagal, paku terakhir lepas.
    public class RitualAltar : MonoBehaviour, IInteractable
    {
        public const float SanityPaku = 10f, JedaTongkat = 6f, JedaAbdi = 10f, RadiusTongkat = 25f, DmgTongkat = 15f, JamFajar = 5.67f, PengaliFajar = 10f, JamPagi = 7f, RadiusAbu = 140f;
        static readonly ItemID[] Paku = { ItemID.PakuKeramat1, ItemID.PakuKeramat2, ItemID.PakuKeramat3 };
        [SerializeField] Light[] lubang = new Light[3];
        [SerializeField] GameObject ukiranLore, cahayaEmas, suarHijau, bayanganKiRogo;
        public static bool SiegeBerlangsung { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetSiege() { SiegeBerlangsung = false; }

        readonly List<int> _jatuh = new List<int>();
        readonly Queue<EnemyArchetype> _antri = new Queue<EnemyArchetype>();
        ItemID _terakhir;
        int _tampil = -1, _kSpawn;
        bool _pengaliAktif;
        Transform[] _pakuAltar;
        ParticleSystem _partikelSegel;

        public static float LajuPartikelSegelAltar(int tertancap, bool siege)
        {
            if (tertancap <= 0) return 0f;
            return siege ? 28f : tertancap * 6f;
        }

        void PerbaruiVisualPakuDanSegel(int tertancap)
        {
            if (_pakuAltar == null)
            {
                _pakuAltar = new Transform[3];
                var mEmas = SafeMaterialFactory.CreatePBR("M_PakuKeramatAltar", new Color(0.95f, 0.78f, 0.25f), 0.85f, 0.82f, null, null, new Color(2.4f, 1.7f, 0.3f));
                for (int i = 0; i < 3; i++)
                {
                    var pk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    Destroy(pk.GetComponent<Collider>());
                    pk.name = "PakuAltar_" + i;
                    pk.transform.SetParent(transform, false);
                    pk.transform.localPosition = new Vector3((i - 1) * 0.55f, 1.08f, 0f);
                    pk.transform.localScale = new Vector3(0.06f, 0.22f, 0.06f);
                    pk.GetComponent<MeshRenderer>().sharedMaterial = mEmas;
                    _pakuAltar[i] = pk.transform;
                }
                var sg = new GameObject("PartikelSegelAltar");
                sg.transform.SetParent(transform, false);
                sg.transform.localPosition = new Vector3(0f, 1.15f, 0f);
                _partikelSegel = sg.AddComponent<ParticleSystem>();
                var pm = _partikelSegel.main; pm.loop = true; pm.startLifetime = 1.4f; pm.startSpeed = 0.85f; pm.startSize = 0.065f;
                pm.maxParticles = 36; pm.gravityModifier = -0.25f; pm.simulationSpace = ParticleSystemSimulationSpace.World;
                pm.startColor = new Color(1f, 0.84f, 0.28f, 0.9f);
                var psh = _partikelSegel.shape; psh.shapeType = ParticleSystemShapeType.Box; psh.scale = new Vector3(1.4f, 0.1f, 0.5f);
                var ppr = sg.GetComponent<ParticleSystemRenderer>();
                ppr.sharedMaterial = mEmas; ppr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            for (int i = 0; i < 3; i++)
                if (_pakuAltar[i] != null) _pakuAltar[i].gameObject.SetActive(i < tertancap);
            if (_partikelSegel != null)
            {
                float laju = LajuPartikelSegelAltar(tertancap, SiegeBerlangsung);
                var em = _partikelSegel.emission; em.rateOverTime = laju;
                if (laju > 0f && !_partikelSegel.isPlaying) _partikelSegel.Play();
                else if (laju <= 0f && _partikelSegel.isPlaying) _partikelSegel.Stop();
            }
        }

        public void Pasang(Light[] l, GameObject lore, GameObject emas, GameObject suar, GameObject bayangan) { lubang = l; ukiranLore = lore; cahayaEmas = emas; suarHijau = suar; bayanganKiRogo = bayangan; }
        QuestManager Q { get { return GameServices.Get<QuestManager>(); } }
        int Tertancap { get { var q = Q; return q != null ? q.Progres.tertancap : 0; } }

        public static ItemID PakuDimiliki(TikarPandan3DInventory inv) { if (inv != null) for (int i = 0; i < Paku.Length; i++) if (inv.Punya(Paku[i])) return Paku[i]; return ItemID.None; }

        public float HoldDuration { get { return 1f; } }
        public string GetPromptText(GameObject actor)
        {
            if (SiegeBerlangsung || Tertancap >= 3) return "";
            return PakuDimiliki(GameServices.Get<TikarPandan3DInventory>()) != ItemID.None ? "TANCAPKAN PAKU (" + Tertancap + "/3)" : "ALTAR BERLUBANG TIGA";
        }
        public bool CanInteract(GameObject actor) { return !SiegeBerlangsung && Tertancap < 3; }
        public void Interact(GameObject actor)
        {
            var inv = GameServices.Get<TikarPandan3DInventory>();
            var p = PakuDimiliki(inv);
            if (p == ItemID.None) { GameEvents.RaiseToast("Tiga lubang berukir. Sepertinya untuk Paku Keramat."); return; }
            if (!inv.Kurangi(p, 1)) return;
            _terakhir = p;
            var q = Q; int n = Tertancap + 1;
            if (q != null) q.TandaiTertancap(n);
            Suara(ProceduralAudioSynth.KlipGamelanJauh, 0.8f);
            var v = GameServices.Get<AndiVitalsAndSanity>(); if (v != null) v.UbahSanity(SanityPaku);
            GameEvents.RaiseToast("Lubang " + TungkuObor.Romawi[n] + " menyala emas.");
            if (n >= 3) StartCoroutine(Siege());
        }

        void Update()
        {
            var q = Q; if (q == null) return;
            int t = q.Progres.tertancap + (q.Progres.siegeSelesai ? 10 : 0);
            if (t == _tampil) return;
            _tampil = t;
            for (int i = 0; i < lubang.Length; i++) if (lubang[i] != null) lubang[i].enabled = i < q.Progres.tertancap;
            if (ukiranLore != null) ukiranLore.SetActive(q.Progres.tertancap >= 1);
            if (suarHijau != null) suarHijau.SetActive(q.Progres.siegeSelesai && q.Progres.ending == 0);
            PerbaruiVisualPakuDanSegel(q.Progres.tertancap);
        }

        IEnumerator Siege()
        {
            SiegeBerlangsung = true;
            var dir = GameServices.Get<EnemyDirector>();
            var andi = GameServices.Get<AndiFPSController>();
            var vital = GameServices.Get<AndiVitalsAndSanity>();
            var combat = GameServices.Get<CombatAndLeftHandController>();
            var bar = GameServices.Get<BarAtas>();
            Suara(ProceduralAudioSynth.KlipGuruh, 0.9f);
            if (combat != null) combat.Guncang(1f);
            TerrorDirector.Getar(3);
            GameEvents.RaiseToast("BERTAHAN 60 DETIK");
            _antri.Clear();
            float t = 0f, lalu = -1f, tTongkat = JedaTongkat, tAbdi = JedaAbdi;
            Vector3 pusat = transform.position;
            while (t < PenjadwalSiege.Lama)
            {
                if (vital != null && !vital.IsAlive) { Gagal(dir, bar); yield break; }
                PenjadwalSiege.Jatuh(lalu, t, _jatuh);
                for (int i = 0; i < _jatuh.Count; i++)
                {
                    var g = PenjadwalSiege.Tabel[_jatuh[i]];
                    if (g.jenis == EnemyArchetype.KiRogo) { if (bayanganKiRogo != null) bayanganKiRogo.SetActive(true); GameEvents.RaiseSubtitle("Bayangan Ki Rogo bangkit di balik beringin."); continue; }
                    for (int k = 0; k < g.jumlah; k++) _antri.Enqueue(g.jenis);
                }
                while (_antri.Count > 0 && (dir == null || dir.Aktif < PenjadwalSiege.MaksAktif)) Munculkan(dir, _antri.Dequeue(), pusat);
                if (bayanganKiRogo != null && bayanganKiRogo.activeSelf && andi != null)
                {
                    tTongkat -= Time.deltaTime; tAbdi -= Time.deltaTime;
                    if (tTongkat <= 0f)
                    {
                        tTongkat = JedaTongkat;
                        Suara(ProceduralAudioSynth.KlipGuruh, 1.5f);
                        if (combat != null && (andi.transform.position - bayanganKiRogo.transform.position).sqrMagnitude < RadiusTongkat * RadiusTongkat)
                        { bool p; combat.TerimaSerangan(bayanganKiRogo.transform.position, DmgTongkat, false, out p); combat.Guncang(0.4f); }
                    }
                    if (tAbdi <= 0f) { tAbdi = JedaAbdi; _antri.Enqueue(EnemyArchetype.AbdiSekte); _antri.Enqueue(EnemyArchetype.AbdiSekte); }
                }
                float sisa = PenjadwalSiege.Lama - t;
                if (bar != null) bar.Tampil("BERTAHAN " + Mathf.CeilToInt(sisa) + " DETIK", sisa / PenjadwalSiege.Lama, new Color(0.95f, 0.75f, 0.2f));
                lalu = t;
                yield return null;
                t += Time.deltaTime;
            }
            Menang(dir, bar);
            yield return new WaitForSeconds(6f);
            Suara(ProceduralAudioSynth.KlipPesawat, 1f);
            if (suarHijau != null) suarHijau.SetActive(true);
            GameEvents.RaiseSubtitle("Andi: (Sinyalnya tembus. Mereka datang.)");
            yield return new WaitForSeconds(8f);
            if (cahayaEmas != null) cahayaEmas.SetActive(false);
        }

        void Munculkan(EnemyDirector dir, EnemyArchetype j, Vector3 pusat)
        {
            if (dir == null) return;
            Vector3 p = PenjadwalSiege.Titik(pusat, _kSpawn++, Random.value);
            p.y = WorldTerrainGenerator.SampleHeight(p.x, p.z);
            int h = PenjadwalSiege.IndeksHantu(j);
            if (h >= 0) dir.PanggilHantu(h, p);
            else dir.SpawnDi(j, p, pusat, j == EnemyArchetype.PemimpinPatroli);
        }

        void Menang(EnemyDirector dir, BarAtas bar)
        {
            SiegeBerlangsung = false;
            if (bar != null) bar.Sembunyi();
            if (dir != null) dir.AbukanSemua(transform.position, RadiusAbu);
            if (bayanganKiRogo != null) bayanganKiRogo.SetActive(false);
            if (cahayaEmas != null) cahayaEmas.SetActive(true);
            Suara(ProceduralAudioSynth.KlipGamelanJauh, 1f);
            var atm = GameServices.Get<URPAtmosphereAndWeather>(); if (atm != null) atm.SetCuaca(WeatherState.Cerah, false);
            var jam = GameServices.Get<TimeOfDayClock>();
            if (jam != null) { jam.SetWaktu(JamFajar, jam.Hari); jam.Pengali = PengaliFajar; _pengaliAktif = true; StartCoroutine(KembaliNormal(jam)); }
            var q = Q; if (q != null) q.TandaiSiegeSelesai();
            GameEvents.RaiseToast("Cahaya emas menembus tajuk. Kutukan runtuh.");
        }

        IEnumerator KembaliNormal(TimeOfDayClock jam)
        {
            while (_pengaliAktif && jam != null && jam.Jam < JamPagi && jam.Jam >= JamFajar - 0.1f) yield return null;
            if (jam != null) jam.Pengali = 1f;
            _pengaliAktif = false;
        }

        void Gagal(EnemyDirector dir, BarAtas bar)
        {
            SiegeBerlangsung = false;
            _antri.Clear();
            if (bar != null) bar.Sembunyi();
            if (bayanganKiRogo != null) bayanganKiRogo.SetActive(false);
            if (dir != null) dir.AbukanSemua(transform.position, RadiusAbu);
            var q = Q; if (q != null) q.TandaiTertancap(2);
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (inv != null && _terakhir != ItemID.None) inv.Tambah(_terakhir, 1);
            GameEvents.RaiseToast("Ritual gagal. Paku terakhir terlepas dari altar.");
        }

        void Suara(string klip, float pitch)
        {
            var a = GameServices.Get<ProceduralAudioSynth>();
            if (a != null) a.Play(klip, transform.position + Vector3.up, AudioCategory.Efek, 1f, pitch, 7);
        }
    }
}
