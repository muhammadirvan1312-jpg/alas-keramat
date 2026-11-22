using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Fungsi bangunan blueprint (07 §7.1-7.2) selain api & panci: tidur/istirahat, rak & kereta kayu, penampung hujan, jemuran,
    // perangkap ayun, obor tengkorak, menara (naik/turun), pintu, kentongan, rakit.
    public partial class FungsiBangunan : MonoBehaviour, IInteractable
    {
        public const int KapRak = 24, KapKereta = 8, MaksAir = 8, SlotJemur = 6;
        public const float DetikPerAir = 10f, LamaJemur = 120f, LamaObor = 240f, DamagePerangkap = 200f, JamBangun = 6f, TidurSiang = 2f;

        public BlueprintID Jenis;
        [SerializeField] Transform[] visual = new Transform[0];
        [SerializeField] Light lampu;
        [SerializeField] Transform pintu;
        [SerializeField] Vector3 titikAtas;
        public int Stok;
        public bool Siap = true, Terbuka;
        public float NyalaSisa;
        readonly ItemID[] _jemur = new ItemID[SlotJemur];
        readonly float[] _tJemur = new float[SlotJemur];
        float _tAir;
        URPAtmosphereAndWeather _atmos;
        ParticleSystem _baraBangunan;
        Transform _airPenampung;

        public static float TinggiAirPenampung01(int stok)
        {
            return Mathf.Clamp01((float)stok / MaksAir);
        }

        void PerbaruiVisualPenampungDanJemuran()
        {
            if (Jenis == BlueprintID.PenampungHujan)
            {
                float t01 = TinggiAirPenampung01(Stok);
                if (t01 > 0f && _airPenampung == null)
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    Destroy(go.GetComponent<Collider>());
                    go.name = "PermukaanAirHujan";
                    go.transform.SetParent(transform, false);
                    var mr = go.GetComponent<MeshRenderer>();
                    mr.sharedMaterial = SafeMaterialFactory.CreatePBR("M_AirPenampungHujan", new Color(0.24f, 0.52f, 0.64f, 0.78f), 0.1f, 0.92f, null, null, null, SurfaceMode.Transparent);
                    mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    _airPenampung = go.transform;
                }
                if (_airPenampung != null)
                {
                    bool ada = t01 > 0f;
                    if (_airPenampung.gameObject.activeSelf != ada) _airPenampung.gameObject.SetActive(ada);
                    if (ada)
                    {
                        _airPenampung.localPosition = new Vector3(0f, Mathf.Lerp(0.18f, 0.62f, t01), 0f);
                        _airPenampung.localScale = new Vector3(0.54f, 0.015f, 0.54f);
                    }
                }
            }
            else if (Jenis == BlueprintID.JemuranIkan)
            {
                float w = Time.time;
                for (int i = 0; i < visual.Length; i++)
                    if (visual[i] != null && visual[i].gameObject.activeSelf)
                        visual[i].localRotation = Quaternion.Euler(Mathf.Sin(w * 2.3f + i * 1.1f) * 6f, 0f, Mathf.Cos(w * 1.8f + i * 0.9f) * 4f);
            }
        }

        public static bool PunyaBaraBangunan(BlueprintID j, float nyalaSisa)
        {
            return nyalaSisa > 0f && (j == BlueprintID.OborTengkorak || j == BlueprintID.LampuMinyak || j == BlueprintID.PatungNgeri || j == BlueprintID.KomporBatu);
        }

        void PerbaruiBaraBangunan()
        {
            if (PunyaBaraBangunan(Jenis, NyalaSisa))
            {
                if (_baraBangunan == null)
                {
                    var go = new GameObject("BaraBangunan");
                    go.transform.SetParent(transform, false);
                    go.transform.localPosition = lampu != null ? lampu.transform.localPosition : new Vector3(0f, 1.1f, 0f);
                    _baraBangunan = go.AddComponent<ParticleSystem>();
                    var m = _baraBangunan.main;
                    m.loop = true; m.startLifetime = 0.85f; m.startSpeed = 0.55f; m.startSize = 0.045f;
                    m.maxParticles = 20; m.gravityModifier = -0.25f; m.simulationSpace = ParticleSystemSimulationSpace.World;
                    m.startColor = new Color(1f, 0.58f, 0.16f, 0.9f);
                    var em = _baraBangunan.emission; em.rateOverTime = 7f;
                    var sh = _baraBangunan.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.12f;
                    var pr = go.GetComponent<ParticleSystemRenderer>();
                    pr.sharedMaterial = SafeMaterialFactory.CreatePBR("M_BaraBangunan", new Color(1f, 0.55f, 0.15f), 0f, 0.2f, null, null, new Color(3.2f, 1.2f, 0.25f));
                    pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
                if (!_baraBangunan.isPlaying) _baraBangunan.Play();
            }
            else if (_baraBangunan != null && _baraBangunan.isPlaying) _baraBangunan.Stop();
        }

        public float HoldDuration { get { return 0f; } }
        public void Pasang(BlueprintID j, Transform[] vis, Light l, Transform p, Vector3 atas) { Jenis = j; visual = vis; lampu = l; pintu = p; titikAtas = atas; }

        public static ItemID HasilJemur(ItemID id) { return id == ItemID.IkanMatang ? ItemID.IkanKering : id == ItemID.DagingMatang ? ItemID.DagingKering : ItemID.None; }

        void Update()
        {
            float dt = Time.deltaTime;
            UpdateV3(dt);
            if (Jenis == BlueprintID.PenampungHujan)
            {
                if (_atmos == null) _atmos = GameServices.Get<URPAtmosphereAndWeather>();
                bool hujan = _atmos != null && (_atmos.Cuaca == WeatherState.Gerimis || _atmos.Cuaca == WeatherState.HujanBadai);
                var jamAir = GameServices.Get<TimeOfDayClock>();
                bool embun = jamAir != null && jamAir.Jam >= 5f && jamAir.Jam < 8.5f;
                float laju = hujan ? (_atmos.Cuaca == WeatherState.HujanBadai ? 2.5f : 1.5f) : (embun ? 0.45f : 0f);
                if (laju > 0f && Stok < MaksAir) { _tAir += dt * laju; if (_tAir >= DetikPerAir) { _tAir = 0f; Stok++; } }
            }
            if (Jenis == BlueprintID.JemuranIkan) for (int i = 0; i < SlotJemur; i++) if (_jemur[i] != ItemID.None) _tJemur[i] += dt;
            if ((Jenis == BlueprintID.OborTengkorak || Jenis == BlueprintID.LampuMinyak || Jenis == BlueprintID.PatungNgeri || Jenis == BlueprintID.KomporBatu) && NyalaSisa > 0f) NyalaSisa -= dt;
            if (Jenis == BlueprintID.OborTengkorak || Jenis == BlueprintID.PatungNgeri) AlasKeramat.AI.ZonaPenangkal.SetObor(this, transform.position, NyalaSisa > 0f); // T7b: penangkal hantu & raid
            if (lampu != null) { lampu.enabled = NyalaSisa > 0f; if (lampu.enabled) lampu.intensity = 1.8f * (0.9f + 0.2f * Mathf.PerlinNoise(Time.time * 7f, 0.5f)); }
            PerbaruiBaraBangunan();
            PerbaruiVisualPenampungDanJemuran();
            if (pintu != null) pintu.localRotation = Quaternion.Slerp(pintu.localRotation, Quaternion.Euler(0f, Terbuka ? 95f : 0f, 0f), dt * 6f);
            int tampil = Jenis == BlueprintID.JemuranIkan ? Terisi() : Stok;
            for (int i = 0; i < visual.Length; i++) if (visual[i] != null && visual[i].gameObject.activeSelf != (i < tampil)) visual[i].gameObject.SetActive(i < tampil);
        }

        int Terisi() { int n = 0; for (int i = 0; i < SlotJemur; i++) if (_jemur[i] != ItemID.None) n++; return n; }

        void OnTriggerEnter(Collider c)
        {
            if ((Jenis != BlueprintID.PerangkapAyunBambu && Jenis != BlueprintID.PerangkapPunji) || !Siap || c.gameObject.layer != GameLayers.Enemy) return;
            var d = c.GetComponentInParent<IDamageable>();
            if (d == null) return;
            d.TakeDamage(new DamageInfo { amount = Jenis == BlueprintID.PerangkapPunji ? 120f : DamagePerangkap, type = DamageType.Tusuk, point = c.transform.position, direction = transform.forward, source = gameObject, isHeavy = true });
            Siap = false;
            if (Jenis == BlueprintID.PerangkapPunji)
            {
                var tahan = c.GetComponentInParent<JeratanPunji>();
                if (tahan == null) tahan = c.gameObject.AddComponent<JeratanPunji>();
                tahan.Tahan(3f);
            }
            if (visual.Length > 0 && visual[0] != null) visual[0].localRotation = Quaternion.Euler(80f, 0f, 0f);
        }

        public string GetPromptText(GameObject actor)
        {
            var inv = GameServices.Get<TikarPandan3DInventory>();
            var bahu = GameServices.Get<ShoulderLogAndFreeformBuilder>();
            if ((int)Jenis >= 15) return PromptV3(actor);
            switch (Jenis)
            {
                case BlueprintID.SaungPanggung: return "TIDUR";
                case BlueprintID.TendaDarurat: return "ISTIRAHAT";
                case BlueprintID.RakKayu:
                case BlueprintID.KeretaDorongKayu:
                    int kap = Jenis == BlueprintID.RakKayu ? KapRak : KapKereta;
                    if (bahu != null && bahu.Bawa == ShoulderLogAndFreeformBuilder.Muatan.Gelondong && Stok < kap) return (Jenis == BlueprintID.RakKayu ? "TARUH DI RAK (" : "MUAT KAYU (") + Stok + "/" + kap + ")";
                    return Stok > 0 ? "AMBIL KAYU (" + Stok + ")" : "";
                case BlueprintID.PenampungHujan:
                    if (Stok <= 0) return "PENAMPUNG HUJAN (0/" + MaksAir + " - TUNGGU HUJAN/EMBUN)";
                    return inv != null && inv.Punya(ItemID.BotolKosong) ? "ISI BOTOL AIR (" + Stok + "/" + MaksAir + ")" : "MINUM AIR HUJAN (" + Stok + "/" + MaksAir + ")";
                case BlueprintID.JemuranIkan:
                    for (int i = 0; i < SlotJemur; i++) if (_jemur[i] != ItemID.None && _tJemur[i] >= LamaJemur) return "AMBIL " + ItemDatabase.Get(HasilJemur(_jemur[i])).displayName.ToUpperInvariant();
                    if (Terisi() < SlotJemur && inv != null) { if (inv.Punya(ItemID.IkanMatang)) return "JEMUR IKAN MATANG"; if (inv.Punya(ItemID.DagingMatang)) return "JEMUR DAGING MATANG"; }
                    return "";
                case BlueprintID.PerangkapAyunBambu: return Siap ? "" : "SETEL ULANG";
                case BlueprintID.OborTengkorak:
                    if (NyalaSisa > 0f) return "";
                    return inv != null && (inv.Punya(ItemID.KorekGas) || inv.Punya(ItemID.Damar)) ? "NYALAKAN OBOR" : "";
                case BlueprintID.MenaraPengintai: return actor != null && actor.transform.position.y > transform.position.y + 3f ? "TURUN MENARA" : "NAIK MENARA";
                case BlueprintID.PintuBambu: return Terbuka ? "TUTUP PINTU" : "BUKA PINTU";
                case BlueprintID.KentonganKemah: return "PUKUL KENTONGAN";
                case BlueprintID.RakitBambu: return "NAIKI RAKIT";
                default: return "";
            }
        }

        public bool CanInteract(GameObject actor) { return GetPromptText(actor).Length > 0; }

        public void Interact(GameObject actor)
        {
            var inv = GameServices.Get<TikarPandan3DInventory>();
            var bahu = GameServices.Get<ShoulderLogAndFreeformBuilder>();
            var synth = GameServices.Get<ProceduralAudioSynth>();
            string p = GetPromptText(actor);
            if ((int)Jenis >= 15) { InteraksiV3(actor); return; }
            switch (Jenis)
            {
                case BlueprintID.SaungPanggung: case BlueprintID.TendaDarurat: Tidur(Jenis == BlueprintID.SaungPanggung); return;
                case BlueprintID.RakKayu: case BlueprintID.KeretaDorongKayu:
                    if (p.StartsWith("AMBIL")) { if (bahu != null && bahu.Panggul(ShoulderLogAndFreeformBuilder.Muatan.Gelondong, null)) Stok--; else GameEvents.RaiseToast("Bahu sudah penuh"); }
                    else if (bahu != null) { bahu.Lepaskan(); Stok++; }
                    return;
                case BlueprintID.PenampungHujan:
                    if (Stok <= 0) { GameEvents.RaiseToast("Air belum terkumpul, tunggu hujan atau embun pagi"); return; }
                    if (p.StartsWith("ISI") && inv.Kurangi(ItemID.BotolKosong, 1)) { inv.Tambah(ItemID.BotolAirBersih, 1); Stok--; GameEvents.RaiseToast("Botol terisi air hujan bersih"); }
                    else { var v = GameServices.Get<AndiVitalsAndSanity>(); if (v != null) v.Minum(28f, false); Stok--; GameEvents.RaiseToast("Air hujan segar memulihkan dahaga"); }
                    return;
                case BlueprintID.JemuranIkan:
                    for (int i = 0; i < SlotJemur; i++)
                        if (_jemur[i] != ItemID.None && _tJemur[i] >= LamaJemur) { if (inv.Tambah(HasilJemur(_jemur[i]), 1) > 0) { _jemur[i] = ItemID.None; GameEvents.RaiseToast("Hasil jemuran diambil"); } return; }
                    var id = inv.Punya(ItemID.IkanMatang) ? ItemID.IkanMatang : ItemID.DagingMatang;
                    for (int i = 0; i < SlotJemur; i++) if (_jemur[i] == ItemID.None && inv.Kurangi(id, 1)) { _jemur[i] = id; _tJemur[i] = 0f; return; }
                    return;
                case BlueprintID.PerangkapAyunBambu: Siap = true; if (visual.Length > 0 && visual[0] != null) visual[0].localRotation = Quaternion.identity; return;
                case BlueprintID.OborTengkorak:
                    if (inv.Punya(ItemID.KorekGas) || inv.Kurangi(ItemID.Damar, 1)) NyalaSisa = LamaObor;
                    return;
                case BlueprintID.MenaraPengintai:
                    var cc = actor.GetComponent<CharacterController>();
                    if (cc != null) cc.enabled = false;
                    actor.transform.position = p.StartsWith("NAIK") ? transform.TransformPoint(titikAtas) : transform.position + transform.forward * 2.5f + Vector3.up * 0.2f;
                    if (cc != null) cc.enabled = true;
                    return;
                case BlueprintID.PintuBambu: Terbuka = !Terbuka; return;
                case BlueprintID.KentonganKemah: if (synth != null) synth.PlayKentonganBamboo(transform.position); return;
                case BlueprintID.RakitBambu: var pengayuh = actor.GetComponent<AndiFPSController>(); if (pengayuh != null) pengayuh.NaikRakit(this); return;
            }
        }

        // Tidur: malam -> sampai 06:00; siang -> tidur sebentar 2 jam. Istirahat (tenda) memulihkan tanpa melompat ke pagi bila siang.
        void Tidur(bool kasur)
        {
            if (AlasKeramat.AI.RitualAltar.SiegeBerlangsung) { GameEvents.RaiseToast("Tidak bisa tidur saat ritual berlangsung!"); return; }
            var jam = GameServices.Get<TimeOfDayClock>();
            var v = GameServices.Get<AndiVitalsAndSanity>();
            if (jam == null) return;
            float j = jam.Jam;
            float lama = (j >= 18f || j < JamBangun) ? (j >= 18f ? 24f - j + JamBangun : JamBangun - j) : TidurSiang;
            jam.Maju(lama);
            if (v != null) v.Pulihkan(lama, kasur);
            if (kasur && lama > TidurSiang) { var n = GameServices.Get<AlasKeramat.AI.NingsihVirginiaAI>(); if (n != null) n.UbahTrust(AlasKeramat.AI.NingsihVirginiaAI.KejadianTrust.TidurDiSaung); } // 08: +3 tidur di Saung
            GameEvents.RaiseToast(lama > TidurSiang ? "Kamu tidur sampai pagi" : "Kamu beristirahat sejenak");
            var simpan = GameServices.Get<Core.SaveSystem>();
            if (simpan != null) simpan.Simpan();
        }
    }
}
