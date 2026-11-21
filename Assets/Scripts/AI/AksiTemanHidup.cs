using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Aksi hidup companion (seperti Kelvin & Virginia di SOTF): Kang Deden berkala mengumpulkan ranting/batu untuk Andi,
    // sementara Ningsih memberi peringatan dini ketika hantu atau patroli sekte mengintai dalam jarak 28 m.
    public class AksiTemanHidup : MonoBehaviour
    {
        public const float JedaBantuDeden = 140f, JedaPeringatanNingsih = 32f, RadiusPeringatan = 28f;

        float _tDeden = 60f, _tNingsih = 12f, _tCek;
        AndiFPSController _andi;

        public static bool PerluPeringatanBahaya(float jarakAncaman, float sisaJeda)
        {
            return sisaJeda <= 0f && jarakAncaman > 0f && jarakAncaman <= RadiusPeringatan;
        }

        public static ItemID PilihHadiahDeden(int langkah)
        {
            switch (Mathf.Abs(langkah) % 4)
            {
                case 0: return ItemID.Ranting;
                case 1: return ItemID.Batu;
                case 2: return ItemID.DaunSirih;
                default: return ItemID.TaliIjuk;
            }
        }

        int _hitungHadiah;

        void Update()
        {
            if (JedaDunia.Dijeda) return;
            float dt = Time.deltaTime;
            _tDeden -= dt;
            _tNingsih -= dt;
            _tCek -= dt;
            if (_tCek > 0f) return;
            _tCek = 1.0f;

            if (_andi == null) _andi = GameServices.Get<AndiFPSController>();
            if (_andi == null) return;
            Vector3 ap = _andi.transform.position;

            if (_tDeden <= 0f && !WorldTerrainGenerator.DiBawahTanah(ap.y))
            {
                _tDeden = JedaBantuDeden;
                var inv = GameServices.Get<TikarPandan3DInventory>();
                if (inv != null)
                {
                    var hadiah = PilihHadiahDeden(_hitungHadiah++);
                    if (inv.Tambah(hadiah, 2) > 0)
                        GameEvents.RaiseToast("Kang Deden mengumpulkan " + ItemDatabase.Get(hadiah).displayName + " x2 untukmu.");
                }
            }

            if (_tNingsih <= 0f)
            {
                float dMin = 999f;
                bool hantu = false;
                var daftarH = HantuAI.Semua;
                for (int i = 0; i < daftarH.Count; i++)
                {
                    var h = daftarH[i];
                    if (h == null || !h.IsAlive || h.Fase == HantuAI.FaseHantu.Hilang) continue;
                    float d = Vector3.Distance(ap, h.transform.position);
                    if (d < dMin) { dMin = d; hantu = true; }
                }
                var daftarM = NusantaraEnemyAndMutantAI.Semua;
                for (int i = 0; i < daftarM.Count; i++)
                {
                    var m = daftarM[i];
                    if (m == null || !m.IsAlive) continue;
                    float d = Vector3.Distance(ap, m.transform.position);
                    if (d < dMin) { dMin = d; hantu = false; }
                }
                if (PerluPeringatanBahaya(dMin, _tNingsih))
                {
                    _tNingsih = JedaPeringatanNingsih;
                    GameEvents.RaiseToast(hantu
                        ? "Ningsih berbisik: 'Awas Mas, hawa dingin mendekat...'"
                        : "Ningsih memberi isyarat: 'Patroli sekte di sekitar sini!'");
                }
            }
        }
    }
}
