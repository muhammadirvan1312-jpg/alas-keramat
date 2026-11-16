# 03 — DUNIA, TERRAIN MATEMATIS, 7 BIOMA & 3 KOMPLEKS BAWAH TANAH
Dibaca pada tahap T2 (dunia), T5 (vegetasi & foraging), T10 (gua & POI cerita).
Semua koordinat dalam meter. Permukaan X,Z ∈ [−500, 500], laut Y = 0.

---

## 1. Peta Ringkas
```
                               UTARA (Z=+500) — laut
        Dataran Kapur (h≈50) · Jurang Kapur z 318–330 · Celah Kuil (0,345)
        Jalur naik barat (x≈-175, z 220→300)      Air terjun (90,298)
                                   |                     \
  BARAT                            |                      Sungai turun ke tenggara
  Sawit Terbakar (-260,120)        |              Desa Pasir Lenyap (label 250,90;
  Rawa Merah (-235,95) r55         |              rumah di x 250–300) + dermaga (238,84)
  Tanah Gali Bunker (-280,145)     |                     /
                     \             |                    /
                      ---> KAWAH BERINGIN AGUNG (0,0) <---
                           Altar (0,-9) · rim r≈70 m
                                   |
                     Hutan Bambu & Sungai — Mulut Gua Air (140,-130)
                                   |
                Bangkai Heli & Hutan Jati (0,-220) · Lapangan Kemah (28,-188)
                Anak sungai dangkal lewat ±12 m selatan heli
                                   |
                Pantai Selatan · Landasan SAR (0,-420) · Mercusuar (-95,-438)
                Bangkai Perahu (62,-448)
                               SELATAN (Z=-500)
```

## 2. Fungsi Ketinggian `WorldTerrainGenerator.SampleHeight(x, z)`
Deterministik (tanpa Random global). `Perlin(x,z)` = `Mathf.PerlinNoise(x + 1000.37f, z + 1000.71f)` (offset agar tidak simetris di 0).
Helper:
```csharp
static float S(float a, float b, float v) { float t = Mathf.Clamp01((v - a) / (b - a)); return t * t * (3f - 2f * t); } // bekerja juga bila a > b
static float Fbm(float x, float z, int oct) { float s = 0, amp = 1, f = 1, n = 0; for (int i = 0; i < oct; i++) { s += amp * Perlin(x * f, z * f); n += amp; amp *= 0.5f; f *= 2f; } return s / n; }
static float Ridged(float x, float z) { float r = 1f - Mathf.Abs(2f * Perlin(x, z) - 1f); return r * r; }
static float Gauss(float d, float mu, float sigma) { float k = (d - mu) / sigma; return Mathf.Exp(-k * k); }
```
Urutan perhitungan (WAJIB urut):
```csharp
public static float SampleHeight(float x, float z) {
    // 1. Mask pulau (pantai bergelombang)
    float rc = Mathf.Sqrt(x * x + z * z) / 500f;
    float rb = Mathf.Max(Mathf.Abs(x), Mathf.Abs(z)) / 500f;
    float r = Mathf.Lerp(rc, rb, 0.35f) + (Fbm(x * 0.006f + 11f, z * 0.006f + 5f, 3) - 0.5f) * 0.10f;
    float land = 1f - S(0.82f, 0.97f, r);
    // 2. Dasar + bukit + punggung
    float h = Mathf.Lerp(-8f, 12f, land);
    float hills = (Fbm(x * 0.0045f, z * 0.0045f, 5) - 0.45f) * 22f;
    float ridge = Ridged(x * 0.0018f + 3.1f, z * 0.0018f + 7.7f) * 9f;
    h += (hills + ridge * 0.7f) * land;
    // 3. Pantai selatan landai
    float beachTarget = Mathf.Lerp(1.8f, -1.5f, S(-430f, -475f, z));
    h = Mathf.Lerp(h, beachTarget, S(-350f, -410f, z));
    // 4. Dataran kapur utara + tebing + jalur naik barat + jurang
    float trail = 1f - S(15f, 35f, Mathf.Abs(x + 175f));
    float cliffW = Mathf.Lerp(15f, 80f, trail);
    float tPlateau = S(300f - cliffW, 300f, z) * (1f - S(230f, 300f, Mathf.Abs(x))) * land;
    float plateauH = 50f + (Fbm(x * 0.01f, z * 0.01f, 3) - 0.5f) * 8f;
    h = Mathf.Lerp(h, plateauH, tPlateau);
    float chasm = (1f - S(4f, 7f, Mathf.Abs(z - 324f))) * (1f - S(130f, 150f, Mathf.Abs(x)));
    h -= 38f * chasm * tPlateau;
    // 5. Kawah Beringin
    float d0 = Mathf.Sqrt(x * x + z * z);
    h = Mathf.Lerp(6.0f, h, S(25f, 62f, d0));        // dasar kawah 6 m
    h += 9f * Gauss(d0, 70f, 14f);                   // bibir kawah
    // 6. Bantalan datar POI (tabel §3)
    h = ApplyPads(x, z, h);
    // 7. Rawa Merah (SETELAH pad agar pad Sawit tidak menimbun rawa; garis pantai rawa ≈ 50 m)
    float dSw = Vector2.Distance(new Vector2(x, z), new Vector2(-235f, 95f));
    h = Mathf.Lerp(1.4f, h, S(40f, 65f, dSw));
    // 8. Sungai utama & anak sungai (tabel §4): tanggul dulu, lalu gerus
    h = ApplyRivers(x, z, h);
    return h;
}
```
- `ApplyPads`: untuk tiap pad `(cx, cz, rIn, rOut, hTarget)` **berurutan sesuai tabel §3** (pad umum dulu, pad spesifik terakhir): `h = Mathf.Lerp(hTarget, h, S(rIn, rOut, jarak))`.
- `ApplyRivers`: untuk tiap polyline, cari jarak terdekat `dr` + parameter `t` → lebar `w(t)`, permukaan air `wl(t)` (interpolasi linear antar titik). Tanggul: jika `dr < w/2 + 14` maka `h = Mathf.Max(h, Mathf.Lerp(wl + 0.5f, h, S(w/2, w/2 + 14, dr)))`. Gerus: `bed = wl − kedalaman`; `h = Mathf.Lerp(bed, h, S(w/2 − 2, w/2 + 1.5f, dr))`. Pita air sungai dibuat selebar `w + 1` m agar tepinya masuk ke tebing (tanpa celah). Rumus ini sudah disimulasikan: tidak ada tepi sungai yang lebih rendah dari permukaan air.
- Fungsi pendamping: `SampleNormal` (beda pusat 1 m), `SampleSlopeDeg`, `GetWaterLevelAt(x,z)` (null jika bukan air), `GetBiome(x,z)` (§5), `IsUnderwater(pos)`.
- Cek kewajaran (masuk `NusantaraSelfTest`): `SampleHeight(0,-220)` ≈ 7 ± 0,5; `(0,-9)` ≈ 6 ± 0,5; `(0,345)` ≈ 51 ± 1; `(0,324)` < 20; `(0,-420)` ≈ 1,6 ± 0,4; `(-280,145)` ≈ 3,4 ± 0,3; `(-235,95)` < 2,0 (dasar rawa); `(-495,0)`, `(495,0)`, `(0,495)` < 0 (laut).

## 3. Bantalan Datar POI (pad)
| Nama | Pusat (x,z) | rIn | rOut | Tinggi |
|---|---|---|---|---|
| Heli & kemah awal | (0,−220) | 18 | 40 | 7,0 |
| Lapangan Kemah (saran bangun) | (28,−188) | 12 | 24 | 7,6 |
| Desa (rumah) | (270,98) | 35 | 58 | 10,0 |
| Sawit umum | (−265,125) | 60 | 100 | 3,2 |
| Tanah Gali Bunker (SETELAH Sawit umum) | (−280,145) | 8 | 14 | 3,4 |
| Mulut Gua Air | (140,−130) | 6 | 12 | 5,0 |
| Celah Kuil (utara jurang; radius kecil agar tidak menimbun jurang) | (0,345) | 6 | 12 | 51,0 |
| Landasan SAR | (0,−420) | 25 | 45 | 1,6 |
| Mercusuar (tanjung batu) | (−95,−438) | 10 | 18 | 6,0 |
| Kampung Sekte Watu Ireng (v3) | (−300,250) | 30 | 50 | 14,0 |
| Makam Tua Pasir Lenyap (v3) | (318,58) | 15 | 25 | 11,0 |

Lokasi v3 lain (Kemah Rawa, 5 Pos Pengintai, Kebun Ningsih) & isinya: `16_DUNIA_HIDUP.md` §3. Pad v3 sudah disimulasikan bersama rumus §2.
Catatan: label zona Desa di radar tetap (250,90) sesuai cerita; rumah-rumah berdiri di timur sungai.

## 4. Sungai, Anak Sungai, Air
**Sungai Utama** (titik: x, z, permukaan air wl, lebar w, kedalaman):
| # | x | z | wl | w | dalam |
|---|---|---|---|---|---|
| 0 (kolam air terjun) | 90 | 270 | 13,0 | 10 | 2,0 |
| 1 | 150 | 200 | 11,0 | 9 | 1,6 |
| 2 | 205 | 140 | 9,0 | 10 | 1,6 |
| 3 (dermaga desa) | 232 | 80 | 7,5 | 11 | 1,8 |
| 4 | 205 | −10 | 6,0 | 12 | 1,6 |
| 5 (lembah bambu) | 160 | −120 | 4,0 | 12 | 1,6 |
| 6 | 170 | −250 | 2,2 | 14 | 1,8 |
| 7 | 225 | −390 | 0,6 | 16 | 2,0 |
| 8 (muara) | 255 | −500 | 0,0 | 18 | 2,5 |
**Anak Sungai Heli** (dangkal, aman; lewat ±12 m di selatan titik jatuh heli): (−75,−175, wl 9,0, w 3) → (−30,−232, 6,2, 4) → (10,−238, 5,9, 4) → (60,−255, 4,5, 4) → (110,−262, 3,2, 5) → (170,−250, 2,2, 5); kedalaman 0,5. (Koreksi: rute ini dijaga agar tidak menggerus bantalan heli, spawn Andi, dan koper.)
**Air Terjun**: dari tepi dataran (90, 298, y ≈ 48) jatuh ke dasar tebing (90, 285), lalu mengalir pendek ke kolam (90,270). Pita vertikal 6 m lebar, tekstur menggulir 1,4 m/s, partikel kabut air di dasar, suara (lihat 10).
**Mesh air**: laut = bidang 3000×3000 di y 0 (transparan, normal map digulir 2 arah); sungai = pita mengikuti polyline, tinggi per-vertex = wl; rawa = cakram r 50 di y 2,0 warna `#5A1414` α 0,6; kolam gua lokal.
**Minum**: sungai & anak sungai & sumur desa = air tawar (+Hidrasi); laut = air asin (−Hidrasi, lihat 05).

## 5. Bioma (`GetBiome`, prioritas atas → bawah)
1. `Laut` — h < 0.
2. `Pantai` — z < −340 dan h < 4.
3. `KawahBeringin` — jarak ke (0,0) < 78.
4. `TebingKapur` — tPlateau > 0,5.
5. `SawitTerbakar` — x ∈ [−345, −180], z ∈ [40, 200].
6. `Desa` — jarak ke (270,98) < 70.
7. `HutanBambu` — jarak ke sungai utama < 55 DAN z ∈ [−220, 20] DAN x > 80.
8. `TepiSungai` — jarak ke sungai mana pun < 30.
9. `HutanJatiSelatan` — z < −140.
10. `HutanCampuran` — sisanya.

## 6. Albedo Dunia & Material Terrain
- **v4**: terrain dibuat sebagai **Unity Terrain** (heightmap 1025² dari `SampleHeight`, TerrainLayer CC0, holes untuk mulut gua) — lihat 19 §2. Kalimat chunk di bawah hanya berlaku sebagai cadangan bila Terrain gagal.
- Terrain = 20×20 chunk @ 50 m, grid 2 m (26×26 vertex). LOD1 > 120 m: grid 4 m. Mesh disimpan sebagai aset (`Assets/Generated/Meshes/Terrain/`), collider `MeshCollider` hanya pada LOD0.
- Satu tekstur **WorldAlbedo 2048×2048** (1 px ≈ 0,49 m) dipanggang Builder: warna bioma (palet di 01 §7.2) + tanah coklat di lereng > 28°, batu kapur di lereng > 40° atau tPlateau, pasir di pantai, jalan setapak (lebar 2,5 m, coklat `#7A5A3A`) yang menghubungkan: Heli → Lapangan Kemah → Lembah Bambu → Kawah → Desa; Kawah → Sawit; Heli → Pantai; Sawit → Jalur naik barat → Celah Kuil. UV terrain = (x+500)/1000, (z+500)/1000.
- Detail: `_DetailAlbedoMap` noise abu tiling 4 m + `_DetailNormalMap` (keyword `_DETAIL_MULX2`). Jika detail tidak didukung perangkat, tampilan tetap aman (hanya albedo).
- Smoothness 0,12; tanah basah saat hujan → smoothness naik ke 0,45 (global, via `URPAtmosphereAndWeather`).

## 7. Vegetasi & Sumber Daya (kepadatan per hektar = per 100×100 m)
| Bioma | Pohon (per ha) | Semak/Pakis | Lainnya |
|---|---|---|---|
| HutanJatiSelatan | Jati 55, Mahoni 10, Pisang 6, Aren 4 | Semak 120, Pakis 200 | Batu 30, Sirih 3, Kunyit 2, Jahe 2, Jamur 4, JamurBeracun 3 |
| HutanCampuran | Mahoni 35, Jati 20, Aren 8, Pisang 5, BeringinKecil 1 | Semak 140, Pakis 250 | sama + sarang telur 0,5 |
| HutanBambu | BambuRumpun 45, Pisang 8 | Semak 60 | Kabut lembah pagi |
| TepiSungai | Kelapa 6, BambuRumpun 10 | Semak 80 | Ikan (1 kawanan / 60 m sungai) |
| Pantai | Kelapa miring 22 | Semak pantai 30 | Batu karang, kolam pasang |
| SawitTerbakar | SawitHangus grid 8 m | — | Titik bara 20, papan, tengkorak |
| Desa | Kelapa 8, Pisang 10 | Semak 30 | — |
| KawahBeringin | Mahoni 15 (di bibir) | Semak 50 | Sesajen busuk |
| TebingKapur | Jati 10 | Semak 60 | Bongkah kapur 40 |
- Pengecualian: radius dalam pad POI, di atas air, lereng > 35°, jarak < 3 m dari pohon lain, jalan setapak.
- RNG deterministik per chunk: `new System.Random(1337 + chunkX * 73856093 ^ chunkZ * 19349663)`.
- Tiap pohon punya **ID global** (indeks chunk × 1000 + urutan) untuk save "pohon sudah ditebang".
- Estimasi total: 4.500–5.500 pohon, 25.000 semak/pakis (instanced, tanpa collider kecuali semak stealth: collider trigger).
- Jenis pohon & hasil tebang: lihat `docs/gdd/07` §1.

## 8. Hewan — Titik Muncul
| Hewan | Bioma | Kelompok | Kepadatan | Hasil (detail 05) |
|---|---|---|---|---|
| Rusa Timor | Jati, Campuran, Kawah | 2–4 | 1 kelompok / 4 ha | Daging 3, Kulit 1, Tulang 2 |
| Kancil | Campuran, Bambu | 1 | 1 / 3 ha | Daging 1, Kulit 1, Tulang 1 |
| Babi Hutan | Jati, Campuran | 2–3 | 1 kelompok / 6 ha | Daging 4, Kulit 1, Tulang 3, Tengkorak 1 |
| Ayam Hutan | Bambu, Desa, Tepi Sungai | 2–5 | 1 kelompok / 5 ha | Daging 1, Bulu 3, (sarang: Telur 2) |
| Biawak | Tepi Sungai | 1 | 1 / 200 m sungai | Daging 2, Kulit 1 |
| Ikan | Sungai, laut dangkal | 4–8 | lihat atas | IkanMentah 1 |
| Kelelawar | UG1, puncak Mercusuar | kawanan | — | (tidak diburu) |
| Penyu | Pantai (malam) | 1 | 2 total | Ambien, dilindungi: menyerang = Sanity −10 |
Maks 10 hewan aktif dalam 100 m; despawn di luar 140 m & tidak terlihat; respawn tiap 1 hari game.

## 9. Tujuh Zona Permukaan — Daftar Objek Presisi

### Zona 1 — Titik Jatuh Heli & Hutan Jati Selatan (0,−220)
- **Bangkai heli** di (4,−224), yaw 35°, roll 18°: badan kapsul 9×2,4×2,6 putih `#E8E6E0` dengan strip oranye `#E06A1B` + logo fiktif "BC-SURVEY"; ekor patah 6 m terpisah; rotor ekor di (14,−214); 3 bilah rotor utama bengkok; kaca kokpit pecah. Kolom asap hitam (partikel) sampai Hari 3 08:00; 2 api kecil sampai Hari 1 12:00.
- **Spawn Andi** (−2,−212) berbaring → cutscene bangun. **Kang Deden** duduk terhuyung di ekor (12,−220).
- **3 Koper Darurat** (kotak oranye 0,6×0,4×0,25, bisa dibuka):
  - K1 (−3,−217): `GolokPusaka`, **Lore #1**.
  - K2 (8,−230): `GPSHandset`, `BukuPanduan`, `KorekGas`, `PetaKertas`.
  - K3 (−10,−228): `JamuHerbal` ×2, `OborBambu` ×1, `RansumDarurat` ×2, `BukuCatatan`.
- Puing: 10 panel logam, 2 kursi, `LogamBekas` ×2 bisa diambil, `KainBatik` ×1 (selimut).
- Anak sungai dangkal ±12 m di selatan; Lapangan Kemah datar di (28,−188).

### Zona 2 — Lembah Hutan Bambu & Sungai Berkabut (140,−130)
- Rumpun bambu rapat berderit (suara 10 §4); kabut lembah 05:00–09:00.
- Jembatan bambu tua putus di (160,−95): dua setengah jembatan di kedua tepi.
- **CaveEntrance_1** (140,−130): gundukan batu (gauss +8 m di (128,−138) r 14) dengan mulut gua menghadap timur ke sungai, tirai stalaktit, cahaya biru samar dari dalam, bidang hitam unlit di kedalaman 6 m.
- Titik penampakan pertama Ningsih (120,−160) (lihat 04).
- **Sarang Ningsih** (100,−175): gubuk kecil bersandar di bawah rumpun bambu, alas daun pisang, bunga kantil di kaleng, sisa api kecil — **Lore #8**. Tempat Ningsih bermalam saat Trust < 70.
- Ikan di sungai; sarang ayam hutan.

### Zona 3 — Desa Mati "Pasir Lenyap" (label 250,90)
| Objek | Posisi | Catatan |
|---|---|---|
| R1 Rumah Kepala Desa (8×6, panggung 1,5 m, tangga) | (282,112) yaw 200° | Isi: `Cangkul`, `TaliIjuk` ×2, **Lore #2**, `MinyakTanah` ×1 |
| R2 | (262,118) yaw 180° | `KainBatik` ×2 |
| R3 | (296,88) yaw 260° | Loteng: `BedilLocok`, `PeluruLocok` ×3, `MesiuHitam` ×1 |
| R4 | (272,70) yaw 330° | `Damar` ×2 |
| R5 (runtuh separuh) | (252,100) yaw 120° | `Kemenyan` ×1 |
| R6 (hangus) | (290,130) yaw 210° | `Arang` ×3 |
| Tiang Kentongan Utama 7 m + kain merah | (268,95) | Dipukul sendiri saat Maghrib (audio 3D) |
| Sumur tua berlumut (cincin batu r 0,9, ember) | (258,88) | Sumber air: minum & isi botol |
| Jemuran kain batik kusam (2 tali) | (276,84)–(284,80) | Bisa diambil: `KainBatik` ×2 |
| Tengkorak kerbau di tonggak | (238,104), (300,110) | `Tengkorak` ×1 masing-masing |
| Dermaga tua + sampan bocor | (238,84) | Landmark |
| **Gua Tawanan** (lubang berpagar + kurungan bambu 4×4) | (305,120) | Titik bangun setelah tertangkap; **Lore #10** |
| Api ritual malam Sekte | (270,100) | Menyala 19:00–04:00 |
| **Totem Topeng** ×3 (tiang kayu bertopeng + kain merah, HP 120, bisa dihancurkan) | (245,110), (298,72), (280,135) | Agresi Sekte +5 per totem dihancurkan (06 §7) |
- Siang: patroli Sekte (lihat 06). Rumah punya lantai & dinding mesh berlubang (interior bisa dimasuki).

### Zona 4 — Perkebunan Sawit Terbakar & Rawa Merah (−260,120)
- Barisan `SawitHangus` grid 8 m di x ∈ [−340, −185], z ∈ [50, 195] (kecuali rawa).
- **Rawa Merah** pusat (−235,95) r 50: air merah, kabut merah rendah (bidang partikel tinggi 1,2 m), suara serangga berat. `AbdiLumpur` bersembunyi di sini.
- **Papan kayu bertulisan darah** (Lore #3) di (−226,160) + tengkorak kerbau di pancang (`Tengkorak` ×1).
- **Tanah Gembur Bertanda Silang** (−280,145): tanah gelap lunak r 2,5 + dua pancang menyilang; gali 4× dengan `Cangkul` → muncul **pintu palka baja** (`CaveEntrance_2`). Penanda "?" muncul di GPS setelah Lore #1 dibaca; berubah jadi "X" setelah Lore #3 dibaca.
- Bangkai ekskavator kuning hangus + papan nama terbakar "PT BHUMI CAKRA" di (−300,80). Loot: `LogamBekas` ×4, `MinyakTanah` ×2, `Tengkorak` ×1.
- **Totem Topeng** ×3: (−200,60), (−320,180), (−250,170).
- Malam: `Banaspati` (lihat 06).

### Zona 5 — Pohon Beringin Agung & Altar (0,0)
- **Beringin Agung**: batang r 4,5 m beralur dengan 8 akar papan (buttress), tinggi 28 m, tajuk Ø 44 m (12 gumpalan daun cutout bertumpuk), 40 akar gantung (silinder meruncing 6–18 m, sedikit bergoyang angin). **Kain poleng** melilit batang y 1,0–2,2; **kain kuning** y 2,4–3,0. Tidak bisa ditebang (memukulnya: Sanity −5 + tawa Kuntilanak).
- **Meja Batu Altar Segel** (0,−9): 2,4×1,2×0,9 dengan 3 lubang berukir (I, II, III); lubang menyala emas saat paku ditancapkan. Ukiran di sisi selatan altar = **Lore #12** (bisa dibaca setelah paku pertama tertancap).
- 4 lentera batu di sudut altar; sesajen busuk (keranjang anyaman).
- Malam: **Kuntilanak Beringin** bertengger di dahan; Sanity −0,15/detik dalam 40 m (kecuali Ningsih Trust ≥ 70 ikut bersama).
- Pintu keluar UG3 muncul di akar sisi barat laut (−12, 12) setelah boss dikalahkan.

### Zona 6 — Tebing Kapur Utara & Celah Kuil (0,340)
- Dataran h ≈ 50; tebing curam di z ≈ 285–300; **jalur naik barat** di x ≈ −175 (z 220 → 300).
- **Jurang Kapur** z 318–330, x −140..140, dalam ±38 m. **Jembatan tali tua putus** di x = 0 (tiang di kedua sisi, tali menjuntai).
- **Jangkar Zipline** (objek `ZiplineAnchor`, tiang batu/pohon bertanda kain merah):
  - A (0, puncak batu +4 m, 310) ⇄ B (0, pohon, 338): menyeberang jurang (WAJIB untuk ke UG3).
  - C (40, tiang batu, 296) → D (70, pohon, 180): turun cepat ke lembah selatan.
- **CaveEntrance_3** (0,345): celah batu raksasa 6×12 m, akar hitam menjulur keluar, cahaya merah samar.
- Air terjun (90,298) → kolam (90,270).

### Zona 7 — Pantai Karang Selatan & Landasan Heli (0,−420)
- Pasir, batu karang, kelapa miring, kolam pasang berisi ikan kecil.
- **Mercusuar Tua** (−95,−438): menara 22 m putih kusam bergaris merah pudar; tangga besi luar ke puncak (koloni kelelawar, pemandangan, **Lore #7** di ruang lampu); lampu mati → menyala hijau saat Ending.
- **Bangkai perahu nelayan** (62,−448): `PistolSuar` + `PelurSuar` ×4.
- **Landasan SAR** (0,−420): pasir terbuka r 30 tanpa pohon/bangunan (Builder menolak blueprint di sini). Penyu muncul malam.

## 10. Tiga Kompleks Bawah Tanah (Y = −80)
Aturan umum:
- Dibangun dari mesh prosedural "tabung gua" (loft spline dengan radius bernoise) dan ruangan (hemisfer/kotak terdeformasi dengan normal ke dalam) + `MeshCollider`. ≤ 60.000 segitiga per kompleks.
- **Mode Dalam Ruangan** saat pemain di dalam: matahari & bulan intensitas 0, langit diganti warna solid hitam, ambient gelap (`#0A0E12`), fog hitam densitas 0,06, partikel cuaca mati, `AudioReverbZone` preset Cave, far clip kamera 80 m, maksimal 4 `Light` titik aktif terdekat.
- Masuk/keluar = trigger → fade hitam 1,2 detik → teleport → fade masuk. Posisi permukaan disimpan untuk keluar.
- Semua koordinat di bawah relatif terhadap pusat kompleks.

### UG1 — Gua Sungai Bioluminesensi (pusat −300, −80, −300)
| Ruang | Posisi lokal | Ukuran (P×T×L) | Isi |
|---|---|---|---|
| R0 Mulut Dalam (titik tiba) | (−60,0,0) | 10×6×8 | Pintu keluar → permukaan (146,−128) |
| T1 Lorong | (−55→−25, z 0) | lebar 4, tinggi 4 | Tetes air, lumut tipis |
| R1 Ruang Lumut | (−15,0,0) | kubah 20×10×18 | 40 stalaktit, lumut bercahaya `#3CF2C8`, kolam kecil, kerangka anggota ekspedisi + ransel: **Lore #4**. Pocong Gua #1 jatuh dari langit-langit saat Lore #4 dibaca |
| R2 Kamp Ekspedisi | (10,0,15) | 12×5×10 | Sisa tenda, peti: `AlatSelamRebreather`, `KartuAksesBunker`, `Baterai` ×2, `Tulang` ×3; buku catatan Rendra = **Lore #11** |
| R3 Kolam Hitam | (15,−2,−10) | kolam 14×8, air y −1 | Pintu terowongan bawah air U1 |
| U1 Terowongan Air | (20,−4,−14) → (60,−4,−14) berkelok | panjang 70 m | Napas tanpa alat 25 detik (tidak cukup) → WAJIB Rebreather |
| R4 Altar Paku | (60,0,−14) | 16×9×16 | Altar batu dengan **`PakuKeramat1`**, 2 Pocong Gua penjaga, 1 penyergap saat kembali |
| T2 Jalan Pintas | (60,0,−5) → (0,0,8) | lebar 3 | Tertutup reruntuhan, hanya bisa didorong dari sisi R4 (agar tidak perlu menyelam lagi) |
Cahaya: lumut emisi + 6 point light cyan (maks 4 aktif).

### UG2 — Bunker Kolonial & Laboratorium PT Bhumi Cakra (pusat 0, −80, −300)
| Ruang | Posisi lokal | Ukuran | Isi |
|---|---|---|---|
| E0 Ruang Tangga (titik tiba) | (0,0,−55) | 6×4×6 | Tangga ke palka permukaan (−280,145) |
| C1 Koridor | (0,0,−50 → −20) | lebar 3, tinggi 3 | Beton berkarat, neon berkedip tiap 6 m, genangan |
| D1 Pintu Baja Bersandi | (0,0,−20) | — | Pembaca kartu LED merah/hijau; perlu `KartuAksesBunker` |
| R1 Arsip Belanda 1928 | (−14,0,−8) | 12×3,2×10 | Rak arsip, meja: **Lore #5** |
| R2 Laboratorium PT | (14,0,−8) | 14×3,2×12 | Meja operasi tua, tabung cairan hitam (emisi samar), terminal mati, **`SenterTaktis`**, `Baterai` ×3, Lore #9 (opsional) |
| C2 Koridor | (0,0,0 → 25) | lebar 3 | Lampu darurat merah mati-nyala |
| R3 Gudang Peralatan | (−12,0,30) | 10×3,2×10 | **`ZiplineGun`**, `TaliZipline` ×6, `LogamBekas` ×4, `ZirahBaja` ×2, `KapakBaja` |
| R4 Ruang Segel | (12,0,34) | 16×6×16 | Tabung kaca berisi **`PakuKeramat2`** melayang dalam kabut hitam. Mengambilnya: alarm, lampu merah, dinding runtuh, 3 Abdi Sekte + 2 Pocong Gua menyerbu (lihat 06 §8) |
| X1 Tangga Darurat | dari R4 | — | Keluar ke permukaan (−270,150) |

### UG3 — Kuil Akar Beringin Hitam (pusat 300, −80, −300)
| Ruang | Posisi lokal | Ukuran | Isi |
|---|---|---|---|
| K0 Celah Akar (titik tiba) | (0,0,−70) | lorong turun lebar 3 | Dari celah (0,345) |
| K1 Lorong Urat | (0,0,−60 → −25) | lebar 4, tinggi 5 | Akar seperti urat berdenyut merah (emisi), segmen "Lorong Angin" 15 m: obor padam otomatis (perlu Senter) |
| K2 Ruang Sesaji | (−18,0,−15) | 12×5×10 | Sisa juru kunci + kain kuning: **Lore #6**. Jika Ningsih ikut (Trust ≥ 70): cutscene pendek ia berlutut |
| K3 Kubah Akar Agung (arena boss) | (0,0,20) | kubah Ø 40, tinggi 22 | 8 pilar akar, 8 tungku obor merah, panggung batu tengah dengan **`PakuKeramat3`** di kepompong akar; Ki Rogo berdiri di panggung |
| K4 Ruang Emas Majapahit (lanjutan) | (30,0,40) | 8×4×8 | Di balik akar yang harus dibakar (Panah Api / Bom Bambu): `KerisPusaka`, Zirah Emas |
| K5 Jalan Keluar | (0,0,42 → 60) | — | Terbuka setelah boss; keluar di akar Beringin (−12,12) permukaan |
