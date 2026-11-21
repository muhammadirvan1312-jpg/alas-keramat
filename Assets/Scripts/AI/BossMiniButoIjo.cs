using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Boss Mini Baru: Buto Ijo Penjaga Alas (raksasa 2,1x dengan serangan Hentakan Tanah / Ground Slam area).
    public class BossMiniButoIjo : MonoBehaviour, IDamageable
    {
        public const float HPMax = 420f, RadiusDeteksi = 26f, RadiusHentak = 5.2f, DamageHentak = 32f, JedaHentak = 4.2f, Kecepatan = 4.4f;

        [SerializeField] string namaGelar = "Buto Ijo Penjaga Alas";
        float _hp = HPMax, _tHentak, _tSuara, _fase;
        Vector3 _posAsal;
        AndiFPSController _andi;
        bool _mati;

        public bool IsAlive { get { return !_mati && _hp > 0f; } }
        public float HP { get { return _hp; } }

        public void Pasang(string gelar) { namaGelar = string.IsNullOrEmpty(gelar) ? "Buto Ijo Penjaga Alas" : gelar; }

        public static float HitungDamageMasuk(float dasar, DamageType tipe)
        {
            if (tipe == DamageType.Api) return dasar * 1.5f;
            if (tipe == DamageType.Keramat) return dasar * 1.6f;
            return dasar;
        }

        void Awake() { _posAsal = transform.position; _hp = HPMax; }

        public void TakeDamage(DamageInfo d)
        {
            if (_mati || d.amount <= 0f) return;
            float dmg = HitungDamageMasuk(d.amount, d.type);
            _hp = Mathf.Max(0f, _hp - dmg);
            var gore = GameServices.Get<GoreSystem>();
            if (gore != null) gore.Semburan(d.point, d.direction, dmg, true, d.isHeavy);
            if (_hp <= 0f)
            {
                _mati = true;
                GameEvents.RaiseToast(namaGelar + " TUMBANG! Jarahan langka terjatuh.");
                JarahanTanah.Jatuhkan(transform.position + Vector3.up * 0.4f, ItemID.JamuEnergi, 3);
                JarahanTanah.Jatuhkan(transform.position + Vector3.right * 0.7f + Vector3.up * 0.4f, ItemID.Tengkorak, 2);
                JarahanTanah.Jatuhkan(transform.position - Vector3.right * 0.7f + Vector3.up * 0.4f, ItemID.LogamBekas, 4);
                var rep = GameServices.Get<ReputasiSekte>();
                if (rep != null) rep.TambahAncaman(25f, "Penjaga Alas dikalahkan");
                transform.rotation *= Quaternion.Euler(85f, 0f, 0f);
                enabled = false;
                return;
            }
            if (Mathf.RoundToInt(_hp) % 80 < 25)
                GameEvents.RaiseToast(namaGelar + " HP: " + Mathf.RoundToInt(_hp) + "/" + Mathf.RoundToInt(HPMax));
        }

        void Update()
        {
            if (_mati || JedaDunia.Dijeda) return;
            float dt = Time.deltaTime;
            _tHentak -= dt; _tSuara -= dt;
            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            if (_andi == null) return;

            Vector3 p = transform.position;
            Vector3 ke = _andi.transform.position - p; ke.y = 0f;
            float jarak = ke.magnitude;

            if (jarak > RadiusDeteksi)
            {
                // Patroli melingkar di sekitar pos penjagaan
                _fase += dt * 0.4f;
                Vector3 target = _posAsal + new Vector3(Mathf.Cos(_fase) * 5.5f, 0f, Mathf.Sin(_fase) * 5.5f);
                Vector3 dPat = target - p; dPat.y = 0f;
                if (dPat.sqrMagnitude > 0.25f)
                {
                    Vector3 langkah = p + dPat.normalized * (Kecepatan * 0.45f * dt);
                    langkah.y = WorldTerrainGenerator.SampleHeight(langkah.x, langkah.z);
                    transform.SetPositionAndRotation(langkah, Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dPat), dt * 4f));
                }
                return;
            }

            if (jarak > 2.4f)
            {
                Vector3 arah = ke / jarak;
                Vector3 baru = p + arah * Kecepatan * dt;
                baru.y = WorldTerrainGenerator.SampleHeight(baru.x, baru.z);
                transform.SetPositionAndRotation(baru, Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(arah), dt * 6f));
            }

            if (jarak <= RadiusHentak && _tHentak <= 0f)
            {
                _tHentak = JedaHentak;
                _andi.GuncangKamera(0.45f);
                var dmgTarget = _andi.GetComponent<IDamageable>();
                if (dmgTarget != null)
                    dmgTarget.TakeDamage(new DamageInfo { amount = DamageHentak, type = DamageType.Tumpul, point = _andi.transform.position, direction = (ke.sqrMagnitude > 0.01f ? ke.normalized : Vector3.forward), isHeavy = true, source = gameObject });
                var gore = GameServices.Get<GoreSystem>();
                if (gore != null) gore.PercikPartikel(p + Vector3.up * 0.3f, new Color(0.32f, 0.28f, 0.22f, 0.85f), 18, 0.22f, 3.8f, 0.9f);
                var synth = GameServices.Get<ProceduralAudioSynth>();
                if (synth != null) synth.Play(ProceduralAudioSynth.KlipGuruh, p, AudioCategory.Efek, 0.85f, 0.78f, 2, 55f);
                GameEvents.RaiseToast(namaGelar + " menghantam tanah!");
            }
            else if (_tSuara <= 0f)
            {
                _tSuara = 7f;
                var synth = GameServices.Get<ProceduralAudioSynth>();
                if (synth != null) synth.Play(ProceduralAudioSynth.KlipTeriakSekte, p, AudioCategory.Efek, 0.75f, 0.55f, 2, 48f);
            }
        }
    }
}
