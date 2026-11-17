# 20 — V5 KUALITAS MAKSIMAL: DUNIA 2×2 KM, VISUAL, ISI & MEKANIK
> Berlaku mulai v5 dan MENGALAHKAN dokumen 01–19 bila bertentangan (kecuali AGENTS.md untuk aturan teknis H1–H16 yang tidak diubah di sini).
> Arah pengguna: **kualitas di atas segalanya**. Spesifikasi HP bukan batasan desain; preset Rendah/Sedang tetap ada hanya agar game bisa dibuka di HP lemah.

## 1. Target Perangkat & Anggaran Baru (mengganti 11 §7.1 dan 19 §9)
| Hal | v4 | **v5** |
|---|---|---|
| Perangkat acuan | Snapdragon 720G / RAM 4–6 GB | **Snapdragon 8 Gen 2 ke atas / RAM 12 GB** (preset **Ultra**); preset Tinggi/Sedang/Rendah turun bertahap |
| FPS | 30 stabil | 30 stabil (Ultra), 60 opsional di Tinggi |
| Tekstur | 1K, ≤ 300 MB memori | **2K** untuk terrain, batu, kayu, kulit kayu, kain, kulit karakter; **4K** untuk 8 aset pahlawan (§4.3); ≤ 1,2 GB memori di Ultra (streaming mipmap wajib) |
| Segitiga terlihat | ≤ 250 rb | ≤ 900 rb (Ultra) |
| Batch | ≤ 150 | ≤ 400 dengan SRP Batcher + GPU instancing |
| Musuh aktif | ≤ 12 | ≤ 20 |
| Suara bersamaan | 24 | 48 (32 3D + 16 2D), lihat 21 |
| Ukuran APK | ~ 0,3 GB | **perkiraan 1,0–1,6 GB** (APK tunggal; bila > 1,9 GB pecah menjadi APK + paket data `.obb` yang diunduh pertama kali dari GitHub Releases) |

Preset grafis (Pengaturan → Grafis): Ultra (semua 2K/4K, bayangan 4 kaskade 150 m, rumput rapat 100%, jarak pandang 1.200 m), Tinggi (2K, 2 kaskade 90 m, rumput 70%), Sedang (1K, 1 kaskade 60 m, rumput 40%), Rendah (512, tanpa bayangan lunak, rumput 15%). Preset dipilih otomatis saat pertama dibuka dari `SystemInfo.systemMemorySize` + `graphicsMemorySize`, bisa diubah.

## 2. Dunia 2×2 km (mengganti batas di 03 & AGENTS Bagian 5)
- Permukaan X,Z ∈ **[−1000, 1000]** (4 km²). Laut di luar radius pulau 930 m.
- **Aturan skala**: semua koordinat permukaan di `docs/gdd/03` (zona, props, sungai, jalan, pad) dan 16 **dikali 2**. Ketinggian dari rumus 03 §2 **dikali 1,5**. Bawah tanah tetap Y = −80; X,Z bawah tanah juga dikali 2.
- `WorldTerrainGenerator.SampleHeight(x,z)` v5 = `1,5 × SampleHeight03(x/2, z/2) + Σ FiturV5(x,z)` dengan FiturV5 = puncak/lekuk Gaussian tambahan per zona baru (§2.2).
- **Streaming**: 16 tile Unity Terrain 500×500 m (4×4), heightmap 1025² per tile (resolusi 0,49 m), splatmap 1024² per tile, tetangga disambung (`SetNeighbors`). Tile dalam radius 1 dari pemain aktif penuh; tile lain memakai versi basemap + collider ringan; vegetasi detail hanya pada 3×3 tile terdekat.
- Jarak pandang 1.200 m (Ultra) dengan **impostor** pohon (billboard 8 arah dipanggang Builder ke atlas 4K) di atas 180 m.

### 2.1 Zona lama (koordinat baru setelah dikali 2)
Z1 Titik Jatuh Heli (0,−440) · Z2 Lembah Bambu (280,−260) · Z3 Desa Mati (500,180) · Z4 Sawit Terbakar & Rawa Merah (−520,240) · Z5 Beringin Agung (0,0) · Z6 Tebing Kapur Utara (0,680) · Z7 Pantai Karang & Landasan Heli (0,−840).

### 2.2 Lima zona baru
| Zona | Pusat | Biome (ID baru) | Isi utama | FiturV5 tinggi |
|---|---|---|---|---|
| Z8 Rawa Bakau & Muara Hitam | (−650,−560) | RawaBakau = 11 | Akar bakau tinggi, air payau setinggi pinggang, buaya muara (hewan), perahu nelayan karam, rumah panggung roboh, kabut rendah permanen | lekuk −6 m r 220 |
| Z9 Pabrik Gula "Sinar Tebu" (fiktif, era kolonial) | (660,−500) | PabrikGula = 12 | Kompleks bata merah 6 bangunan, cerobong 38 m (bisa didaki dalam, menara pengintai), rel lori ke Z11, gudang rantai, kantor administratur dengan arsip | dataran rata r 160 |
| Z10 Gunung Kapur & Air Terjun Sigar Getih | (−600,660) | GunungKapur = 13 | Puncak 210 m (titik tertinggi), air terjun 46 m bertingkat, gua kelelawar, jalur tali panjat, kolam merah karena tanah besi | puncak +150 m r 260 |
| Z11 Kampung Transmigran Sukamaju & Ladang Tebu | (620,620) | LadangTebu = 14 | 22 rumah kayu seragam ditinggalkan mendadak (makan malam masih di meja), balai desa, masjid kecil **tanpa audio ibadah** (H13), ladang tebu 3 m yang menutup pandangan, sumur tua | landai +8 m r 300 |
| Z12 Alas Wingit (hutan larangan) | (−760,−40) | AlasWingit = 15 | Hutan primer paling rapat & gelap (kanopi 35 m), pohon raksasa berlumut, sesajen di tiap persimpangan, kabut 24 jam, kompas berputar, tempat paling seram (Tension dasar +15) | bergelombang ±12 m |

### 2.3 Bawah tanah (6 kompleks)
UG1 Gua Sungai (−600,−80,−600) · UG2 Bunker PT (0,−80,−600) · UG3 Kuil Akar (600,−80,−600) · **UG4 Terowongan Lori Belanda** (700,−80,−300): 900 m rel bawah tanah dari pabrik gula, lori bisa didorong, ruang pompa banjir · **UG5 Sumur Tua & Ruang Pemujaan** (620,−80,700): turun dengan tali dari sumur kampung, ruang batu berukir, kolam darah (set-piece) · **UG6 Gua Kapur Bertingkat** (−600,−80,800): gua vertikal 5 lantai, panjat tali & tangga bambu, danau bawah tanah (selam dengan rebreather), keluar di balik air terjun Z10.
`UndergroundId` tambahan: UG4_TerowonganLori = 4, UG5_SumurTua = 5, UG6_GuaKapur = 6.

## 3. Isi & Mekanik Baru (setara/lebih dari loop SOTF)
| Mekanik | Isi | Tahap |
|---|---|---|
| Panjat tali & tebing | Tali panjat (item) dipasang di titik jangkar; animasi tangan prosedural; stamina terkuras; jatuh = damage | T6 |
| Berenang & selam | Air dalam di Z8/Z10/UG6; napas 25 s (60 s dengan rebreather); arus sungai mendorong; senter bawah air | T3 (dasar), T10 (gua) |
| Kendaraan | **Rakit bambu** (sungai & rawa), **lori tangan** (rel UG4–Z11), **sepeda ontel** (jalan tanah; pedal stamina; jatuh di lereng) | T6, T10 |
| Berburu & menguliti | Babi hutan, rusa, kijang, ayam hutan, biawak, ikan, kepiting, buaya (Z8, berbahaya); bangkai dikuliti → daging/kulit/tulang; bau bangkai menarik musuh | T5 |
| Perangkap | Jerat, punji, bubu, perangkap ayun; pemeriksaan berkala | T5 |
| Api dinamis | Api menjalar di rumput kering musim kemarau (grid 2 m, angin), bisa dipakai menyerang kamp Sekte, bisa membakar kemah sendiri | T6, T9 |
| Struktur rusak | Setiap potongan bangunan punya HP; Sekte/Genderuwo merusak; potongan jatuh fisik | T6, T7 |
| Musim | Kemarau/Penghujan bergantian tiap 6 hari game: banjir sungai, jalan lumpur, panen buah berbeda | T9 |
| Luka & medis | Pendarahan, patah tulang (jalan pincang, bidai dari bambu+kain), infeksi luka (demam, penglihatan kabur), jahit luka | T7 |
| Stealth lanjut | Kamuflase lumpur, semak, suara langkah per permukaan memengaruhi deteksi, lempar batu pengalih | T7 |
| Sekte membangun | Sekte membangun pos kayu & patung ngeri baru di wilayah yang kosong; pos bisa dibakar | T9 |
| Foto & jurnal | Kamera saku tua (item): foto disimpan di Buku Catatan; beberapa hantu hanya terlihat di foto | T9 |

ID baru (tambahkan ke `SOTFContracts` pada tahap yang memakainya, jangan mengubah ID lama): `ItemID` 200–259 (200 TaliPanjat, 201 Bidai, 202 JarumBenang, 203 KameraSaku, 204 FilmKamera, 205 PelampungBambu, 206 SenterSelam, 207 DagingBuaya, 208 KulitBuaya, 209 DagingRusa, 210 Tanduk, 211 BatuPengalih, 212 Lumpur, 213 KunciLori, 214 TebuBatang, 215 GulaMerah, 216 ArsipPabrik, 217 MinyakLampuGas …, sisa nomor dicadangkan); `BlueprintID` 27 RakitBesar, 28 SepedaOntelDudukan, 29 TitikTaliPanjat, 30 MenaraSinyal; `EnemyArchetype` 20 BuayaMuara (hewan agresif), 21 MandorPabrik (hantu elit Z9), 22 SundelBolong (Z11), 23 PenungguSumur (UG5), 24 KelelawarRaksasa (UG6); `QuestID` M14–M18 = 14–18 (zona baru), S05–S08 = 105–108.

## 4. Visual v5 (melengkapi 15 & 19)
### 4.1 Pencahayaan & atmosfer
- **Kabut volumetrik palsu**: tumpukan quad "fog card" ber-shader `AK/FogVolume` (noise 3D bergulir, soft-particle depth fade, diterangi warna matahari/bulan) di lembah, rawa, Alas Wingit; dikendalikan cuaca. (Tetap tanpa renderer feature, sesuai H15.)
- **Sinar tajuk** (god rays) kartu mesh di celah kanopi + partikel debu; intensitas mengikuti sudut matahari.
- **Bayangan kontak** di kaki karakter & props (decal gelap lembut) + AO SSAO URP bawaan aktif di Ultra/Tinggi.
- **Malam benar-benar gelap**: eksposur malam 0,08 dari siang; satu-satunya cahaya kuat = obor/senter/bulan. Senter memakai cookie tekstur pola lensa + bayangan real-time (1 lampu bayangan tambahan).
- **Basah**: hujan menaikkan smoothness semua material dunia (+0,35), genangan muncul di cekungan (mask dari heightmap), tetes air di layar saat menengadah.

### 4.2 Vegetasi
- 14 spesies pohon (9 lama + Bakau, Tebu rumpun, Pohon Kapuk, Randu Alas raksasa, Pakis Haji), masing-masing 3 varian bentuk + 4 LOD + impostor.
- Rumput/semak 3 lapis kepadatan, angin global + gangguan lokal (rumput tersibak pemain/musuh — petunjuk ada yang mendekat).
- Daun berguguran, ranting patah terlihat setelah musuh lewat (jejak).

### 4.3 Aset pahlawan 4K
Beringin Agung, Altar, wajah Andi/Deden/Ningsih, topeng Sekte, Ki Rogo, cerobong pabrik gula, gerbang Kuil Akar, pocong (kain kafan). Lainnya 2K.

### 4.4 Karakter & animasi
- Mocap CMU naik dari ≤ 40 menjadi **≤ 220 klip** (jalan/lari 8 arah, jongkok, merangkak, panjat, berenang, jatuh, terhuyung, berbagai serangan, mati 12 variasi, idle gelisah, membawa kayu, kerja). Blend prosedural 2D (arah × kecepatan) + IK kaki ke tanah + tolehan kepala.
- Sekte 4 varian tubuh × 6 topeng × warna kain; hantu memakai gerak "salah" (frame dilompati, sendi berputar melebihi batas manusia) agar tidak wajar.

## 5. Gerbang Kualitas v5
- G2 (rubrik 19 §7) target dinaikkan: rata-rata ≥ 7,5 pada T2/T4/T6, ≥ 8,0 pada T9–T11.
- G5 baru **Uji Dengar** (lihat 21 §9) di akhir T2, T7, T9, T10.
- Irisan vertikal G3 kini mencakup Z1 + Z12 Alas Wingit malam pertama.

## 6. Pembagian ke Tahap (tambahan pada 12)
T1: preset grafis 4 tingkat & deteksi otomatis, tekstur Builder naik ke 1K–2K. T2: dunia 2×2 km 16 tile + streaming + 12 zona + impostor + kabut volumetrik palsu + ambien v5 (21). T3: berenang dasar. T4: mocap 220 klip + varian Sekte. T5: berburu, menguliti, hewan baru, perangkap. T6: panjat tali, rakit, sepeda, api dinamis, kerusakan struktur. T7: luka & medis, stealth lanjut, musuh 20–24. T8: companion menyesuaikan dunia 2×2 km (jalan jauh, naik rakit). T9: musim, Sekte membangun, kamera saku, kejadian di zona baru. T10: UG4–UG6, misi M14–M18 & S05–S08. T11: preset otomatis, pemecahan APK + data bila > 1,9 GB.
