using System.Collections.Generic;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.AI
{
    // T7c (14 §3-6): zona kena (kepala x1,8 / melee x1,5, badan x1, anggota x0,7), darah & luka menempel lewat GoreSystem,
    // HP anggota (kepala 40, lengan 35, kaki 45) -> putus bila senjata tebas & Gore Penuh; penggal pada Tebasan Penghabisan.
    // Mayat: ragdoll (Sedang/Penuh) + genangan; interaksi GELEDAH / AMBIL TOPENG / POTONG (Penuh, tahan 1,5 s).
    public partial class NusantaraEnemyAndMutantAI
    {
        public enum ZonaKena { Badan, Kepala, LenganKiri, LenganKanan, KakiKiri, KakiKanan }
        public const float HPKepala = 40f, HPLengan = 35f, HPKaki = 45f, LamaPotong = 1.5f;

        readonly float[] _hpAnggota = { 0f, HPKepala, HPLengan, HPLengan, HPKaki, HPKaki };
        readonly bool[] _putus = new bool[6];
        readonly List<GameObject> _luka = new List<GameObject>();
        bool _ragdoll, _topengDiambil, _dipotong;
        GoreSystem _gore;
        SkinnedMeshRenderer _smr;

        public static ZonaKena Zona(Vector3 lokal, float tinggi)
        {
            float y = lokal.y / Mathf.Max(0.1f, tinggi);
            if (y > 0.83f) return ZonaKena.Kepala;
            if (y < 0.47f) return lokal.x < 0f ? ZonaKena.KakiKiri : ZonaKena.KakiKanan;
            if (Mathf.Abs(lokal.x) > 0.17f * tinggi / 1.75f) return lokal.x < 0f ? ZonaKena.LenganKiri : ZonaKena.LenganKanan;
            return ZonaKena.Badan;
        }

        public static float PengaliZona(ZonaKena z, bool melee)
        {
            if (z == ZonaKena.Kepala) return melee ? 1.5f : 1.8f;
            return z == ZonaKena.Badan ? 1f : 0.7f;
        }

        public static bool SenjataTebas(DamageType t) { return t == DamageType.Tebas; }

        // Tulang akar potongan per zona: lengan atas/bawah & paha/betis tergantung tinggi kena (5 varian 14 §5).
        public static int TulangPutus(ZonaKena z, float yRelatif)
        {
            switch (z)
            {
                case ZonaKena.Kepala: return Kerangka.Head;
                case ZonaKena.LenganKiri: return yRelatif > 0.66f ? Kerangka.LUpper : Kerangka.LFore;
                case ZonaKena.LenganKanan: return yRelatif > 0.66f ? Kerangka.RUpper : Kerangka.RFore;
                case ZonaKena.KakiKiri: return yRelatif > 0.25f ? Kerangka.LThigh : Kerangka.LCalf;
                case ZonaKena.KakiKanan: return yRelatif > 0.25f ? Kerangka.RThigh : Kerangka.RCalf;
                default: return Kerangka.Chest;
            }
        }

        SkinnedMeshRenderer SMR() { if (_smr == null) _smr = GetComponentInChildren<SkinnedMeshRenderer>(); return _smr; }
        Transform Tulang(int i) { var s = SMR(); return s != null && s.bones != null && i < s.bones.Length ? s.bones[i] : null; }

        // Dipanggil di awal TakeDamage: mengembalikan damage setelah zona; menerapkan darah, luka & mutilasi.
        float LukaiZona(DamageInfo info, bool penghabisan, out ZonaKena zona, out bool putusLengan, out bool putusKaki)
        {
            putusLengan = false; putusKaki = false;
            if (_gore == null) _gore = GameServices.Get<GoreSystem>();
            float tinggi = _cc != null ? _cc.height : 1.75f;
            Vector3 lokal = transform.InverseTransformPoint(info.point);
            zona = info.point == Vector3.zero ? ZonaKena.Badan : Zona(lokal, tinggi);
            bool melee = info.type == DamageType.Tebas || info.type == DamageType.Tumpul || info.type == DamageType.Tusuk;
            float dmg = info.amount * PengaliZona(zona, melee);
            if (_gore == null) return dmg;
            _gore.Semburan(info.point, info.direction, dmg, false, info.isHeavy);
            int ti = TulangPutus(zona, lokal.y / Mathf.Max(0.1f, tinggi));
            var t = Tulang(ti);
            if (t != null && t.localScale.x > 0.01f) _gore.Luka(t, info.point, -info.direction, info.type, _luka);
            int z = (int)zona;
            if (zona != ZonaKena.Badan && !_putus[z])
            {
                _hpAnggota[z] -= dmg;
                bool putus = GoreSystem.BolehMutilasi(GoreSystem.Tingkat) && StatMusuh.Sekte(jenis) && SenjataTebas(info.type) && (_hpAnggota[z] <= 0f || (info.isHeavy && dmg >= _hp));
                if (penghabisan && GoreSystem.BolehMutilasi(GoreSystem.Tingkat) && StatMusuh.Sekte(jenis)) { zona = ZonaKena.Kepala; z = (int)zona; ti = Kerangka.Head; putus = !_putus[z]; }
                if (putus && SMR() != null)
                {
                    _putus[z] = true;
                    _gore.PutusAnggota(SMR(), ti, info.direction * 3f + Vector3.up * 2f, false);
                    Suara(ProceduralAudioSynth.KlipPutusTulang, 1f);
                    if (zona == ZonaKena.LenganKiri || zona == ZonaKena.LenganKanan) putusLengan = true;
                    if (zona == ZonaKena.KakiKiri || zona == ZonaKena.KakiKanan) putusKaki = true;
                }
            }
            Suara(GoreSystem.BolehDarah(GoreSystem.Tingkat) ? ProceduralAudioSynth.KlipTebasBasah : ProceduralAudioSynth.KlipKenaDaging, 0.9f);
            return dmg;
        }

        // Kematian dengan ragdoll + genangan (Sedang/Penuh); Mati tetap larut jadi abu.
        void MatiGore(Vector3 impuls)
        {
            if (_gore == null) _gore = GameServices.Get<GoreSystem>();
            if (_gore == null || !GoreSystem.BolehMayat(GoreSystem.Tingkat) || SMR() == null) return;
            _ragdoll = true;
            _gore.Ragdoll(SMR().bones, impuls, GameLayers.Enemy, gameObject);
            _gore.Genangan(transform.position);
            Suara(ProceduralAudioSynth.KlipTubuhJatuh, 1f);
        }

        string PromptMayat()
        {
            if (!_digeledah && _jarahan.Count > 0) return "GELEDAH";
            if (!_topengDiambil && StatMusuh.Sekte(jenis) && !Abu()) return "AMBIL TOPENG";
            if (!_dipotong && GoreSystem.BolehMutilasi(GoreSystem.Tingkat) && StatMusuh.Sekte(jenis)) return "POTONG";
            return "";
        }

        bool InteraksiMayat()
        {
            string p = PromptMayat();
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (p == "AMBIL TOPENG")
            {
                _topengDiambil = true;
                if (inv != null) inv.Tambah(ItemID.TopengSekte, 1);
                GameEvents.RaiseToast("Dapat: " + ItemDatabase.Get(ItemID.TopengSekte).displayName);
                var kepala = Tulang(Kerangka.Head);
                if (kepala != null) foreach (var r in kepala.GetComponentsInChildren<Renderer>()) if (r.name.Contains("Topeng")) r.enabled = false;
                return true;
            }
            if (p == "POTONG")
            {
                var c = GameServices.Get<AlasKeramat.Player.CombatAndLeftHandController>();
                if (c == null || c.Aktif.tipe != DamageType.Tebas || c.Aktif.id == ItemID.None) { GameEvents.RaiseToast("Butuh senjata tebas"); return true; }
                _dipotong = true;
                if (inv != null) inv.Tambah(ItemID.PotonganTubuh, 1);
                if (_gore != null) _gore.Semburan(transform.position + Vector3.up * 0.3f, Vector3.up, 30f, false, false);
                Suara(ProceduralAudioSynth.KlipPutusTulang, 1f);
                GameEvents.RaiseToast("Dapat: " + ItemDatabase.Get(ItemID.PotonganTubuh).displayName);
                return true;
            }
            return false;
        }
    }
}
