using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.World
{
    // Lokasi memancing aktif dengan joran bambu: lempar kail, tunggu pelampung bergerak 3,5-5,5 dtk, tarik hasil tangkapan!
    public class PancingBambuAktif : MonoBehaviour, IInteractable
    {
        public const float WaktuTungguMin = 3.2f, WaktuTungguMaks = 5.4f;

        [SerializeField] string namaSpot = "Tepi Sungai";
        [SerializeField] Transform pelampung;

        bool _sedangMancing, _ikanMenyambar;
        float _sisaTunggu, _sisaSambar;
        Vector3 _posAsalPelampung;

        public float HoldDuration { get { return 0f; } }

        public void Pasang(string nama, Transform kail)
        {
            namaSpot = string.IsNullOrEmpty(nama) ? "Tepi Sungai" : nama;
            pelampung = kail;
            if (pelampung != null) _posAsalPelampung = pelampung.localPosition;
        }

        public static ItemID PilihTangkapan(float roll01)
        {
            if (roll01 < 0.56f) return ItemID.IkanMentah;
            if (roll01 < 0.78f) return ItemID.Kepiting;
            if (roll01 < 0.91f) return ItemID.LogamBekas;
            return ItemID.JamuEnergi;
        }

        public string GetPromptText(GameObject actor)
        {
            if (_ikanMenyambar) return "TARIK PANCING SEKARANG! (Ikan menyambar!)";
            if (_sedangMancing) return "Menunggu ikan menyambar di " + namaSpot + "...";
            return "MEMANCING DI " + namaSpot.ToUpperInvariant();
        }

        public bool CanInteract(GameObject actor) { return !_sedangMancing || _ikanMenyambar; }

        public void Interact(GameObject actor)
        {
            if (!_sedangMancing)
            {
                _sedangMancing = true;
                _ikanMenyambar = false;
                _sisaTunggu = Random.Range(WaktuTungguMin, WaktuTungguMaks);
                GameEvents.RaiseToast("Melempar kail di " + namaSpot + "... tunggu pelampung bergetar!");
                var synth = GameServices.Get<ProceduralAudioSynth>();
                if (synth != null) synth.Play(ProceduralAudioSynth.KlipLepasPanah, transform.position, AudioCategory.Efek, 0.55f, 1.25f, 1);
                return;
            }

            if (_ikanMenyambar)
            {
                _sedangMancing = false;
                _ikanMenyambar = false;
                var hasil = PilihTangkapan(Random.value);
                var inv = GameServices.Get<TikarPandan3DInventory>();
                int masuk = inv != null ? inv.Tambah(hasil, 1) : 0;
                if (masuk <= 0) JarahanTanah.Jatuhkan(transform.position + Vector3.up * 0.6f, hasil, 1);
                var lvl = GameServices.Get<LevelSurvivalAndi>();
                if (lvl != null) lvl.TambahXP(LevelSurvivalAndi.Atribut.KeahlianRakit, 12f);
                var synth = GameServices.Get<ProceduralAudioSynth>();
                if (synth != null) synth.PlayItemPickup();
                GameEvents.RaiseToast("Berhasil memancing: " + hasil + "!");
            }
        }

        void Update()
        {
            if (!_sedangMancing || JedaDunia.Dijeda) return;
            float dt = Time.deltaTime;
            if (!_ikanMenyambar)
            {
                _sisaTunggu -= dt;
                if (pelampung != null)
                    pelampung.localPosition = _posAsalPelampung + new Vector3(0f, Mathf.Sin(Time.time * 3f) * 0.04f, 0f);
                if (_sisaTunggu <= 0f)
                {
                    _ikanMenyambar = true;
                    _sisaSambar = 4.2f;
                    var gore = GameServices.Get<GoreSystem>();
                    if (gore != null && pelampung != null)
                        gore.PercikPartikel(pelampung.position, new Color(0.85f, 0.94f, 0.98f, 0.85f), 8, 0.07f, 1.8f, 0.45f);
                    GameEvents.RaiseToast("Kail ditarik ikan! Tekan INTERAKSI sekarang!");
                }
            }
            else
            {
                _sisaSambar -= dt;
                if (pelampung != null)
                    pelampung.localPosition = _posAsalPelampung + new Vector3(Mathf.Sin(Time.time * 28f) * 0.12f, -0.14f, 0f);
                if (_sisaSambar <= 0f)
                {
                    _sedangMancing = false;
                    _ikanMenyambar = false;
                    if (pelampung != null) pelampung.localPosition = _posAsalPelampung;
                    GameEvents.RaiseToast("Ikan lolos dari kail...");
                }
            }
        }
    }
}
