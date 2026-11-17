using AlasKeramat.Building;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // 16 §4 (T9): jadwal harian Sekte, kentongan Maghrib bersahutan, persembahan "mantan warga" setelah >= 3 Sekte diampuni.
    public partial class EnemyDirector
    {
        public enum KegiatanSekte { Tidur, Bangun, Pagi, Patroli, TidurSiang, Pulang, Ritual, Berburu }
        public const float PeluangPersembahan = 0.25f;
        public const int SyaratPersembahan = 3;

        public static KegiatanSekte Jadwal(float jam, bool malamDarah)
        {
            if (malamDarah && (jam >= 19f || jam < 5f)) return KegiatanSekte.Berburu; // Malam Darah: tidak tidur, pemburu keluar
            if (jam >= 23f || jam < 5f) return KegiatanSekte.Tidur;
            if (jam < 6f) return KegiatanSekte.Bangun;
            if (jam < 9f) return KegiatanSekte.Pagi;
            if (jam < 12f) return KegiatanSekte.Patroli;
            if (jam < 14f) return KegiatanSekte.TidurSiang;
            if (jam < 17f) return KegiatanSekte.Patroli;
            if (jam < 19f) return KegiatanSekte.Pulang;
            return KegiatanSekte.Ritual;
        }

        public KegiatanSekte KegiatanKini { get { return _jam != null ? Jadwal(_jam.Jam, MalamDarah) : KegiatanSekte.Patroli; } }

        int _hariMaghrib = -1, _hariPersembahan = -1;

        void TickJadwal()
        {
            if (_jam == null || _andi == null) return;
            if (_jam.Jam >= 17.5f && _jam.Jam < 18f && _hariMaghrib != _jam.Hari)
            {
                _hariMaghrib = _jam.Hari;
                var a = GameServices.Get<ProceduralAudioSynth>();
                Vector3 p = new Vector3(PusatDesa.x, WorldTerrainGenerator.SampleHeight(PusatDesa.x, PusatDesa.z) + 6f, PusatDesa.z);
                if (a != null) { a.Play(ProceduralAudioSynth.KlipKentongan, p, AudioCategory.Efek, 1f, 1f, 3, 400f); a.Play(ProceduralAudioSynth.KlipKentongan, p + new Vector3(60f, 0f, 40f), AudioCategory.Efek, 0.9f, 0.93f, 3, 400f); }
            }
            if (_jam.Jam >= 6f && _jam.Jam < 7f && _hariPersembahan != _jam.Hari)
            {
                _hariPersembahan = _jam.Hari;
                if (SekteDiampuni >= SyaratPersembahan && Random.value < PeluangPersembahan)
                {
                    Vector3 kemah = Kemah != Vector3.zero ? Kemah : _andi.transform.position;
                    foreach (var api in ApiUnggun.Semua) if (api != null) { kemah = api.transform.position; break; }
                    var pilih = new[] { ItemID.IkanMentah, ItemID.Damar, ItemID.KainBatik };
                    JarahanTanah.Jatuhkan(kemah + new Vector3(7f, 0.5f, -5f), pilih[Random.Range(0, pilih.Length)], 1);
                    GameEvents.RaiseSubtitle("(Ada bungkusan kecil di tepi kemah. Seseorang dari desa meninggalkannya diam-diam.)");
                }
            }
        }
    }
}
