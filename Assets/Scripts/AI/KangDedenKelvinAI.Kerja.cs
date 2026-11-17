using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.AI
{
    // 08 §2.3 eksekusi perintah. Siklus kerja: ke titik kerja -> bekerja (pose & bunyi) -> kembali ke kemah -> setor
    // (cetak biru yang butuh -> Rak Kayu -> Tumpukan Kemah). Energi turun 0,05/s saat bekerja; < 20 istirahat lalu lanjut di 80.
    public partial class KangDedenKelvinAI
    {
        public const float LamaTebang = 8f, LamaPungut = 4f, LamaAir = 3f, LamaPerbaiki = 3f, LamaPagar = 2f, RadiusPagar = 12f, HPPerbaikan = 50f;
        public const int TiangPagar = 24, CelahGerbang = 2;

        void TickPerintah(float dt)
        {
            Vector3? kemahN = Kemah();
            switch (Perintah)
            {
                case CompanionOrder.Ikuti:
                    Vector3 t = TitikIkuti(_andi.transform.position, _andi.transform.forward, SisiIkuti);
                    float d = Datar(t - transform.position).magnitude;
                    if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak;
                    if (d > 1.2f) Menuju(t, d > 10f ? Lari : Jalan * (d > 4f ? 2f : 1f), dt); else { Diam(dt); Hadap(_andi.transform.position - transform.position, dt); }
                    return;
                case CompanionOrder.Tunggu:
                    if (Datar(_titikTunggu - transform.position).magnitude > 1f) Menuju(_titikTunggu, Jalan, dt); else Diam(dt);
                    return;
                case CompanionOrder.Istirahat:
                    Vector3 api = kemahN.HasValue ? kemahN.Value + new Vector3(-1.6f, 0f, 1.2f) : transform.position;
                    if (Datar(api - transform.position).magnitude > 1.2f) { Menuju(api, Jalan, dt); return; }
                    Diam(dt);
                    if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Intai;
                    Energi = Mathf.Min(100f, Energi + EnergiNaik * dt);
                    Pulih(1f, dt);
                    if (_lelah && Energi >= EnergiLanjut) { _lelah = false; Perintah = _sebelumLelah; _fase = 0; if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak; }
                    else if (!_lelah && Energi >= 100f) { Perintah = CompanionOrder.Tunggu; _titikTunggu = transform.position; if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak; }
                    return;
                case CompanionOrder.PulangKeKemah:
                    if (!kemahN.HasValue) { Perintah = CompanionOrder.Tunggu; return; }
                    _titikTunggu = kemahN.Value + new Vector3(2f, 0f, -2f);
                    Perintah = CompanionOrder.Tunggu;
                    return;
            }
            if (!kemahN.HasValue && Perintah != CompanionOrder.CariMakanan) { Perintah = CompanionOrder.Tunggu; GameEvents.RaiseSubtitle("(Kang Deden menggeleng: \"Belum ada kemah.\")"); return; }
            Vector3 kemah = kemahN.HasValue ? kemahN.Value : _andi.transform.position;
            switch (Perintah)
            {
                case CompanionOrder.AmbilKayu: Siklus(dt, kemah, LamaTebang, true, ItemID.KayuGelondong, 2); break;
                case CompanionOrder.AmbilRanting: Siklus(dt, kemah, LamaPungut, false, ItemID.Ranting, 3); break;
                case CompanionOrder.AmbilBatu: Siklus(dt, kemah, LamaPungut, false, ItemID.Batu, 2); break;
                case CompanionOrder.CariMakanan: Siklus(dt, kemah, 6f, false, ItemID.KelapaMuda, 2); break;
                case CompanionOrder.IsiAir: Siklus(dt, kemah, LamaAir, false, ItemID.None, 5); break;
                case CompanionOrder.IsiRakKayu: case CompanionOrder.BersihkanKemah: TickPungutKayu(dt, kemah); break;
                case CompanionOrder.SelesaikanBangunan: TickSelesaikan(dt, kemah); break;
                case CompanionOrder.JagaApi: TickJagaApi(dt, kemah); break;
                case CompanionOrder.PerbaikiBangunan: TickPerbaiki(dt, kemah); break;
                case CompanionOrder.BangunPagarKeliling: TickPagar(dt, kemah); break;
            }
        }

        // Fase 0: tentukan & tuju titik kerja; 1: bekerja; 2: kembali; 3: setor.
        void Siklus(float dt, Vector3 kemah, float lama, bool tebang, ItemID hasil, int jumlah)
        {
            switch (_fase)
            {
                case 0:
                    _titikKerja = Perintah == CompanionOrder.IsiAir ? TitikAir(kemah) : TitikAcak(kemah, Perintah == CompanionOrder.CariMakanan ? 60f : RadiusKerja);
                    _fase = 1; _tKerja = 0f;
                    break;
                case 1:
                    if (Datar(_titikKerja - transform.position).magnitude > 1.5f) { if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak; Menuju(_titikKerja, Jalan * 1.3f, dt); break; }
                    Diam(dt);
                    _tKerja += dt;
                    if (_anim != null) _anim.Pose = tebang ? (Mathf.Repeat(_tKerja, 1.1f) < 0.5f ? ProceduralAnimator.PoseTempur.Ancang : ProceduralAnimator.PoseTempur.Ayun) : ProceduralAnimator.PoseTempur.Intai;
                    if (tebang && Mathf.Repeat(_tKerja, 1.1f) < dt && _audio != null) _audio.PlayWoodChop(transform.position + transform.forward);
                    if (tebang && Mathf.Repeat(_tKerja, 1.1f) < dt) MeterSuara.Pancar(transform.position, MeterSuara.Tebang, false);
                    if (_tKerja >= lama) { _fase = 2; if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak; }
                    break;
                case 2:
                    Vector3 tuju = Perintah == CompanionOrder.CariMakanan && _andi != null && Datar(_andi.transform.position - transform.position).magnitude < 60f ? _andi.transform.position : kemah;
                    if (Datar(tuju - transform.position).magnitude > 2.5f) { Menuju(tuju, Jalan * 1.3f, dt); break; }
                    _fase = 3;
                    break;
                default:
                    Setor(kemah, hasil, jumlah);
                    _fase = 0;
                    break;
            }
        }

        void Setor(Vector3 kemah, ItemID id, int n)
        {
            if (Perintah == CompanionOrder.IsiAir)
            {
                var p = Fungsi(BlueprintID.PenampungHujan, kemah);
                if (p != null) p.Stok = Mathf.Min(FungsiBangunan.MaksAir, p.Stok + n);
                return;
            }
            if (Perintah == CompanionOrder.CariMakanan && _andi != null && Datar(_andi.transform.position - transform.position).magnitude < 6f)
            {
                var inv = GameServices.Get<TikarPandan3DInventory>();
                if (inv != null && inv.Tambah(id, n) > 0) { Klip("JempolKanan"); GameEvents.RaiseToast("Kang Deden memberimu " + n + " " + ItemDatabase.Get(id).displayName); return; }
            }
            var cb = CetakTerdekat(kemah);
            if (cb != null) n -= cb.Isi(id, n);
            if (n > 0 && id == ItemID.KayuGelondong)
            {
                var rak = Fungsi(BlueprintID.RakKayu, kemah);
                if (rak != null) { int masuk = Mathf.Min(n, FungsiBangunan.KapRak - rak.Stok); rak.Stok += masuk; n -= masuk; }
            }
            if (n > 0) TumpukanKemah.Ambil(kemah, matKayu).Tambah(id, n);
        }

        static Vector3 TitikAcak(Vector3 pusat, float r)
        {
            var pf = GameServices.Get<GridPathfinder>();
            for (int k = 0; k < 10; k++)
            {
                Vector2 c = Random.insideUnitCircle * r;
                Vector3 p = pusat + new Vector3(c.x, 0f, c.y);
                if (c.magnitude < 8f) continue;
                int ix, iz; SurfaceGrid.KeSel(p, out ix, out iz);
                if (pf != null && pf.Grid != null && pf.Grid.Ambil(ix, iz) != SurfaceGrid.Darat) continue;
                p.y = WorldTerrainGenerator.SampleHeight(p.x, p.z);
                return p;
            }
            return pusat + Vector3.forward * 10f;
        }

        static Vector3 TitikAir(Vector3 pusat)
        {
            for (float r = 15f; r <= 150f; r += 15f)
                for (int a = 0; a < 12; a++)
                {
                    float s = a * Mathf.PI / 6f;
                    float x = pusat.x + Mathf.Cos(s) * r, z = pusat.z + Mathf.Sin(s) * r;
                    if (WorldTerrainGenerator.GetWaterLevelAt(x, z).HasValue && WorldTerrainGenerator.SampleHeight(x, z) > -1f) return new Vector3(x, WorldTerrainGenerator.SampleHeight(x, z), z);
                }
            return pusat;
        }

        static readonly Collider[] _hasilKayu = new Collider[16];

        // Isi Rak Kayu (<= 50 m) / Bersihkan Kemah (<= 25 m): pungut kayu fisik tergeletak & jarahan, bawa ke Rak / tumpukan.
        void TickPungutKayu(float dt, Vector3 kemah)
        {
            float r = Perintah == CompanionOrder.IsiRakKayu ? 50f : 25f;
            if (_fase == 0)
            {
                _sasaranKerja = null;
                int n = Physics.OverlapSphereNonAlloc(kemah, r, _hasilKayu, (1 << GameLayers.LogPhysics) | (1 << GameLayers.Interactable), QueryTriggerInteraction.Collide);
                for (int i = 0; i < n && _sasaranKerja == null; i++)
                {
                    var k = _hasilKayu[i].GetComponentInParent<KayuFisik>();
                    if (k != null && Datar(k.transform.position - kemah).magnitude > 4f) { _sasaranKerja = k; break; }
                    if (Perintah == CompanionOrder.BersihkanKemah) { var j = _hasilKayu[i].GetComponentInParent<JarahanTanah>(); if (j != null) _sasaranKerja = j; }
                }
                if (_sasaranKerja == null) { Perintah = CompanionOrder.Tunggu; _titikTunggu = kemah + new Vector3(2f, 0f, -2f); GameEvents.RaiseSubtitle("(Kang Deden mengangkat bahu: tidak ada lagi yang perlu dipungut.)"); return; }
                _fase = 1;
            }
            if (_fase == 1)
            {
                var comp = _sasaranKerja as Component;
                if (comp == null) { _fase = 0; return; }
                if (Datar(comp.transform.position - transform.position).magnitude > 1.8f) { Menuju(comp.transform.position, Jalan * 1.3f, dt); return; }
                if (comp is KayuFisik) { Object.Destroy(comp.gameObject); _fase = 2; }
                else { var j = (JarahanTanah)comp; j.Interact(gameObject); _fase = 0; }
                return;
            }
            if (Datar(kemah - transform.position).magnitude > 2.5f) { Menuju(kemah, Jalan, dt); return; }
            var rak = Fungsi(BlueprintID.RakKayu, kemah);
            if (rak != null && rak.Stok < FungsiBangunan.KapRak) rak.Stok++;
            else TumpukanKemah.Ambil(kemah, matKayu).Tambah(ItemID.KayuGelondong, 1);
            _fase = 0;
        }

        void TickSelesaikan(float dt, Vector3 kemah)
        {
            var cb = CetakTerdekat(kemah);
            if (cb == null) { Perintah = CompanionOrder.Tunggu; GameEvents.RaiseSubtitle("(Kang Deden mengacungkan jempol: bangunan selesai.)"); return; }
            var rak = Fungsi(BlueprintID.RakKayu, kemah);
            int perluKayu = cb.Kebutuhan(ItemID.KayuGelondong);
            if (perluKayu > 0 && rak != null && rak.Stok > 0)
            {
                if (Datar(cb.transform.position - transform.position).magnitude > 2f) { Menuju(cb.transform.position, Jalan, dt); return; }
                int m = Mathf.Min(rak.Stok, perluKayu); rak.Stok -= m; cb.Isi(ItemID.KayuGelondong, m); return;
            }
            if (perluKayu > 0) { Siklus(dt, kemah, LamaTebang, true, ItemID.KayuGelondong, 2); return; }
            if (cb.Kebutuhan(ItemID.Ranting) > 0) { Siklus(dt, kemah, LamaPungut, false, ItemID.Ranting, 3); return; }
            if (cb.Kebutuhan(ItemID.Batu) > 0) { Siklus(dt, kemah, LamaPungut, false, ItemID.Batu, 2); return; }
            Perintah = CompanionOrder.Tunggu;
            GameEvents.RaiseSubtitle("(Kang Deden menunjuk cetak biru: bahan sisanya harus darimu.)");
        }

        void TickJagaApi(float dt, Vector3 kemah)
        {
            ApiUnggun api = null; float terbaik = 30f;
            foreach (var a in ApiUnggun.Semua) { if (a == null) continue; float d = Datar(a.transform.position - kemah).magnitude; if (d < terbaik) { terbaik = d; api = a; } }
            if (api == null) { Perintah = CompanionOrder.Tunggu; return; }
            if (api.BahanBakar >= ApiUnggun.BBMaks * 0.5f && _fase == 0)
            {
                Vector3 jaga = api.transform.position + new Vector3(-1.6f, 0f, 1.2f);
                if (Datar(jaga - transform.position).magnitude > 1.2f) Menuju(jaga, Jalan, dt); else { Diam(dt); if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Intai; }
                return;
            }
            if (_fase == 0) { _titikKerja = TitikAcak(api.transform.position, 30f); _fase = 1; _tKerja = 0f; if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak; }
            if (_fase == 1)
            {
                if (Datar(_titikKerja - transform.position).magnitude > 1.5f) { Menuju(_titikKerja, Jalan * 1.3f, dt); return; }
                Diam(dt); _tKerja += dt; if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Intai;
                if (_tKerja >= LamaPungut) { _fase = 2; if (_anim != null) _anim.Pose = ProceduralAnimator.PoseTempur.Tidak; }
                return;
            }
            if (Datar(api.transform.position - transform.position).magnitude > 2f) { Menuju(api.transform.position, Jalan, dt); return; }
            api.BahanBakar = Mathf.Min(ApiUnggun.BBMaks, api.BahanBakar + ApiUnggun.BBRanting * 3f);
            _fase = 0;
        }

        void TickPerbaiki(float dt, Vector3 kemah)
        {
            var b = StrukturRusak(kemah);
            var rak = Fungsi(BlueprintID.RakKayu, kemah);
            if (b == null) { Perintah = CompanionOrder.Tunggu; GameEvents.RaiseSubtitle("(Kang Deden mengangguk: semua bangunan utuh.)"); return; }
            if (rak == null || rak.Stok <= 0) { Perintah = CompanionOrder.Tunggu; GameEvents.RaiseSubtitle("(Kang Deden menunjuk Rak Kayu yang kosong.)"); return; }
            if (Datar(b.transform.position - transform.position).magnitude > 2f) { Menuju(b.transform.position, Jalan, dt); return; }
            Diam(dt); _tKerja += dt;
            if (_anim != null) _anim.Pose = Mathf.Repeat(_tKerja, 1f) < 0.5f ? ProceduralAnimator.PoseTempur.Ancang : ProceduralAnimator.PoseTempur.Ayun;
            if (_tKerja < LamaPerbaiki) return;
            _tKerja = 0f; rak.Stok--;
            b.HP = Mathf.Min(BagianStruktur.HPAwal(b.Jenis), b.HP + HPPerbaikan);
            if (_audio != null) _audio.PlayWoodChop(b.transform.position);
        }

        // 17 §7: 24 tiang runcing melingkar r 12 m dengan celah gerbang 2 tiang menghadap Andi; 1 kayu Rak = 2 tiang.
        public static Vector3 PosisiTiangPagar(Vector3 kemah, int i, float sudutGerbang)
        {
            float s = sudutGerbang + (i + CelahGerbang * 0.5f + 0.5f) * Mathf.PI * 2f / (TiangPagar + CelahGerbang);
            return kemah + new Vector3(Mathf.Sin(s), 0f, Mathf.Cos(s)) * RadiusPagar;
        }

        void TickPagar(float dt, Vector3 kemah)
        {
            var rak = Fungsi(BlueprintID.RakKayu, kemah);
            if (_iPagar >= TiangPagar) { Perintah = CompanionOrder.Tunggu; Klip("JempolKanan"); GameEvents.RaiseSubtitle("(Kang Deden menepuk tangan: pagar selesai.)"); return; }
            if (rak == null || (rak.Stok <= 0 && _iPagar % 2 == 0)) { Perintah = CompanionOrder.Tunggu; GameEvents.RaiseSubtitle("(Kang Deden menggeleng: \"Bahan di Rak tidak cukup.\")"); return; }
            Vector3 ke = Datar(_andi.transform.position - kemah);
            float gerbang = ke.sqrMagnitude > 1f ? Mathf.Atan2(ke.x, ke.z) : 0f;
            Vector3 p = PosisiTiangPagar(kemah, _iPagar, gerbang);
            p.y = WorldTerrainGenerator.SampleHeight(p.x, p.z);
            if (Datar(p - transform.position).magnitude > 1.6f) { Menuju(p, Jalan * 1.3f, dt); return; }
            Diam(dt); _tKerja += dt;
            if (_anim != null) _anim.Pose = Mathf.Repeat(_tKerja, 0.8f) < 0.4f ? ProceduralAnimator.PoseTempur.Ancang : ProceduralAnimator.PoseTempur.Ayun;
            if (_tKerja < LamaPagar) return;
            _tKerja = 0f;
            if (_iPagar % 2 == 0) rak.Stok--;
            var r = new ShoulderLogAndFreeformBuilder.Rencana { sah = true, mode = ShoulderLogAndFreeformBuilder.ModeBangun.Tiang, pos = p + Vector3.up * 1.0f, rot = Quaternion.identity, panjang = 2.0f, tertanam = true };
            var b = ShoulderLogAndFreeformBuilder.Buat(r, matKayu);
            if (b != null) { b.SerutKe = 1; ShoulderLogAndFreeformBuilder.Serut(b); }
            _iPagar++;
            if (_audio != null) _audio.PlayWoodChop(p);
        }
    }
}
