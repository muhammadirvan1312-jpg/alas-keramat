# 01 — VISI, PILAR DESAIN, PARITAS FITUR & ARAH SENI
Dokumen induk. Dibaca sekali di setiap sesi sebelum dokumen lain.

---

## 1. Visi Satu Kalimat
Bertahan hidup di hutan tropis terkutuk Nusantara bersama mekanik tuli dan gadis juru kunci, menebang dan membangun dengan kayu fisik, sambil mengungkap dan menutup kembali retakan gaib di bawah Pohon Beringin Agung — dimainkan penuh dengan layar sentuh HP.

## 2. Lima Pilar Desain
| Pilar | Arti praktis | Contoh fitur |
|---|---|---|
| **P1. Fisik & Kerajinan Tangan** | Semua bahan berat terasa berat: kayu dipanggul, dipotong, dirakit satu per satu | Panggul 2 kayu di bahu, belah papan, serut runcing, kereta dorong kayu |
| **P2. Hutan yang Mengamati** | Musuh punya niat; mereka mengintai sebelum menyerang | Sekte mengintip dari balik pohon, kentongan bersahutan, gertakan palsu |
| **P3. Teman yang Hidup** | Companion bukan alat; mereka bereaksi, takut, dan berkembang | Kang Deden mengangguk & jempol, Ningsih menari pagi, Trust bertahap |
| **P4. Misteri Berlapis Bawah Tanah** | Cerita ditemukan, bukan diceritakan | 12 dokumen lore, 3 kompleks bawah tanah, gerbang alat (cangkul, kartu, alat selam, tali luncur) |
| **P5. Nusantara yang Otentik & Hormat** | Folklor, material, bunyi, dan warna lokal; tanpa mengolok agama | Batik parang, kain poleng, kentongan, gamelan pelog/slendro, jamu, saung |

## 3. Pengalaman Inti (Core Loop)
```
Pagi  : jelajah & kumpulkan (tebang, cari kelapa/ikan, gali) -> baca jejak cerita
Siang : bangun & perkuat kemah (freeform + blueprint), perintah Kang Deden, dekati Ningsih
Sore  : masak, isi air, nyalakan Obor Tengkorak -> 18:00 MAGHRIB (langit merah, kentongan)
Malam : bertahan (Sekte menguji kemah, Pocong/Kuntilanak berkeliaran) atau masuk gua
Tidur : simpan permainan di Kasur Saung -> hari berikutnya agresi Sekte naik
Makro : ambil 3 Paku Keramat dari 3 kompleks bawah tanah -> Ritual Altar -> Siege 60 detik -> Ending
```

## 4. Matriks Paritas Fitur (SOTF → Alas Keramat)
Kolom "Tahap" merujuk `docs/tech/12`.

| # | Fitur acuan SOTF | Padanan Alas Keramat | Status | Tahap |
|---|---|---|---|---|
| 1 | Kelvin tuli, perintah lewat notepad | Kang Deden tuli & bisu, Buku Catatan 14 perintah | Wajib | T8 |
| 2 | Virginia, trust, hadiah, diberi senjata | Ningsih, Trust 0–100, hadiah sirih/kelapa/ikan/kemenyan, Busur + Pistol Suar | Wajib | T8 |
| 3 | Kanibal dengan perilaku psikologis | Sekte Penghuni Alas: intai, kentongan, gertak, menyerah, seret teman | Wajib | T7 |
| 4 | Mutan (Fingers, Twins, Puffy, Demon, Baby) | Pocong, Kuntilanak, Banaspati, Manusia Akar, Jenglot, Genderuwo | Wajib (Manusia Akar & Jenglot: lanjutan) | T7/T10 |
| 5 | Musim (salju) | Musim Kemarau ↔ Penghujan (tiap 6 hari), Kabut Asap, Hujan Badai | Wajib | T2 |
| 6 | Bangun bebas dengan kayu fisik | 5 mode freeform: Tiang, Balok Dinding, Atap/Tangga, Belah Papan, Serut Runcing | Wajib | T6 |
| 7 | Guidebook blueprint | Buku Panduan 3D: 8 blueprint inti + 6 lanjutan | Wajib | T6 |
| 8 | Inventory mat 3D & crafting | Tikar Pandan 3D, 10 resep inti + 6 lanjutan | Wajib | T5 |
| 9 | Tebang pohon arah jatuh, pohon menimpa musuh | Takikan 4 kuadran, pohon tumbang membunuh musuh | Wajib | T5 |
| 10 | Gua, keycard, sekop, rebreather, rope gun | 3 kompleks bawah tanah, Kartu Akses, Cangkul, Alat Selam, Zipline Gun | Wajib | T10 |
| 11 | GPS locator & peta | Radar GPS + Peta Kertas layar penuh | Wajib | T3/T10 |
| 12 | Berburu, memancing, meramu, panci masak | Rusa, kancil, babi hutan, ayam hutan, biawak, ikan; Panci Masak & Jemuran | Wajib | T5/T6 |
| 13 | Stealth semak | Jongkok di semak = deteksi ×0,35 | Wajib | T3/T7 |
| 14 | Zirah daun/tulang/teknis | Zirah Daun, Bambu, Tulang, Baja (10 segmen) | Wajib | T5 |
| 15 | Effigy | Obor Tengkorak | Wajib | T6 |
| 16 | Tertangkap → bangun di gua | Pingsan → "Gua Tawanan" Desa (2× pertama), ke-3 = mati | Wajib | T7 |
| 17 | Tidur & simpan, tenda | Kasur Saung (simpan+tidur), Tenda Darurat (simpan cepat) | Wajib | T6 |
| 18 | Mode kesulitan | Damai / Normal / Sulit | Wajib | T11 |
| 19 | Dua ending (pulang/tinggal) | Naik Helikopter SAR / Tetap Menjaga Alas (+Golok Api Pusaka, sandbox) | Wajib | T10 |
| 20 | Kendaraan (golf cart, hang glider, unicycle) | Rakit Bambu (sungai), Paralayang Kain Batik (opsional) | Lanjutan | T11+ |
| 21 | Senjata rahasia & armor emas | Keris Pusaka, Zirah Emas Majapahit (ruang rahasia UG3) | Lanjutan | T10+ |
| 22 | Multiplayer | TIDAK dalam lingkup | Di luar lingkup | — |
| 23 | Gore (potong tubuh, mayat, effigy dari tubuh) | GoreSystem 3 level, Patung Ngeri | Wajib (v3) | T7 |
| 24 | Senjata api modern (pistol, shotgun) | Pistol PT, Senapan Laras Pendek | Wajib (v3) | T7 |
| 25 | Stealth kill & elakan | Bunuh Senyap, Elak | Wajib (v3) | T7 |
| 26 | Bangunan batu, penyimpanan, rumah pohon | 17 §4 | Wajib (v3) | T6 |
| 27 | Dunia dengan satwa & kanibal yang punya kehidupan | 16 | Wajib (v3) | T9 |
| 28 | Mode kustom | 17 §8 | Wajib (v3) | T11 |

## 5. Batas Lingkup yang Jujur
Yang dibuat SAMA SEPERTI acuan: loop survival, perilaku AI, sistem bangunan fisik, struktur cerita & progresi gua.
Yang SENGAJA BERBEDA karena keterbatasan zero-asset mobile:
- **Grafis**: v3 menaikkan target ke *Atmosferik Semi-Realistis* lewat AK Shader Pack, cahaya, kabut, dan grading (`15_VISUAL_FIDELITY_DAN_SHADER.md`); mendekati fotorealistis hanya lewat jalur tekstur CC0 opsional (15 §9). Kualitas tetap terutama datang dari cahaya, kabut, warna, dan siluet.
- **Animasi**: prosedural (kode), bukan motion capture. Fokus pada gestur yang terbaca jelas.
- **Suara**: sintetis; tidak ada dialog berbicara. Andi berbicara lewat subtitle monolog batin; Kang Deden memang bisu; Ningsih hanya bersenandung.
- **Multiplayer**: tidak ada.
- **Jalur peningkatan**: `ProceduralAssetFactory` memeriksa `Resources/Override/<NamaAset>` lebih dulu. Jika suatu hari pengguna punya model/tekstur/audio asli dengan nama yang sama, aset itu otomatis dipakai menggantikan versi prosedural (lihat `docs/tech/11` §6.4).

## 6. Nada & Rating
- **v3: Survival-horror dewasa 18+** dengan darah, mutilasi, dan horor intens yang diatur pengaturan Gore (Mati/Sedang/Penuh) — detail & batas mutlak di `14_GORE_DAN_KEKERASAN.md`. Tanpa konten seksual.
- Kematian musuh: sesuai Gore — ragdoll & mayat (Sedang/Penuh) atau larut menjadi abu hitam (Mati). Hantu selalu "diusir" (larut/kabut), bukan berdarah merah.
- Ritme takut diatur Sutradara Teror (`13_HOROR_DAN_TEROR.md`).
- Ketakutan dibangun lewat: suara dari arah tak terlihat, siluet di batas kabut, cahaya obor yang terbatas, dan ketidakpastian (apakah itu halusinasi Sanity rendah?).

## 7. Arah Seni: "Low-Poly Atmosferik Tropis"
### 7.1 Prinsip
1. **Siluet dulu**: setiap karakter & landmark harus dikenali dari bayangannya saja (Beringin raksasa, Pocong tegak, Kuntilanak rambut terurai, topi caping Sekte).
2. **Faset lembut**: mesh low-poly dengan normal halus pada organik (tubuh, batang), normal bersudut pada batu & bangunan.
3. **Tekstur prosedural 256–512 px** dengan noise halus + normal map; tidak ada tekstur "polos datar" pada objek besar.
4. **Cahaya bercerita**: siang hangat-hijau, Maghrib merah-oranye pekat, malam biru-ungu gelap dengan pusat hangat api.
5. **Kabut = kedalaman**: fog eksponensial selalu aktif; jarak pandang dipakai sebagai alat horor.

### 7.2 Palet Utama (hex)
| Kelompok | Warna |
|---|---|
| Hutan daun (terang→gelap) | `#7FA650` `#5C8A3A` `#3E6B2A` `#26451B` |
| Tanah & jalan setapak | `#7A5A3A` `#5E4630` `#3F2F20` |
| Kulit kayu jati/mahoni | `#6B4F36` `#4E3826` |
| Bambu | `#9DB85A` `#6E8F3A` (kering `#C2A766`) |
| Pasir pantai | `#E3D3A6` `#CDBB8C` |
| Batu kapur utara | `#C9C3B4` `#9E978A` |
| Air sungai / laut | `#2E6F6A` (α 0,72) / `#1F5A7A` (α 0,78) |
| Sawit hangus & bara | `#1E1A17` `#2C2420` bara `#FF5A1F` |
| Kabut merah Rawa | `#7A1E1E` (α 0,35) |
| Lumut bioluminesensi | `#3CF2C8` emisi ×2,2 |
| Beton bunker & karat | `#7D7F7A` `#8A4B2A` |
| Neon bunker berkedip | `#CFF7FF` emisi ×3 |
| Api / obor | `#FFB347` → `#FF6A1A` |
| Segel Ningsih | `#E8C766` emisi ×1,5 (berdenyut) |
| UI emas misi | `#F2C14E` |

### 7.3 Pencahayaan per Waktu (kunci interpolasi; detail di `URPAtmosphereAndWeather`)
| Jam | Matahari/bulan (warna, intensitas) | Ambient langit / ekuator / tanah | Fog warna, densitas |
|---|---|---|---|
| 00:00 | Bulan `#8FA6D8`, 0,18 | `#1B2238` / `#141A28` / `#0B0E14` | `#0E1320`, 0,028 |
| 05:00 | Bulan `#9AA8C8`, 0,15 | `#2A3150` / `#1E2236` / `#10121A` | `#2B3044`, 0,030 (kabut pagi) |
| 06:00 | Matahari `#FFB27A`, 0,55 | `#B98E7A` / `#6E5A50` / `#3A2E28` | `#C8A48C`, 0,022 |
| 08:00 | `#FFE2B8`, 1,05 | `#9CC3E6` / `#7E9A7A` / `#4A4232` | `#B9CBB8`, 0,010 |
| 12:00 | `#FFF6E5`, 1,25 | `#A9D2F2` / `#8FAF88` / `#55503C` | `#C7D8C8`, 0,008 |
| 16:00 | `#FFD7A0`, 1,0 | `#9FC0DD` / `#86987A` / `#4E4434` | `#C2C6B0`, 0,011 |
| 17:30 | `#FF9A5A`, 0,75 | `#C98B6A` / `#7A5246` / `#3A2722` | `#B5765A`, 0,016 |
| 18:00 MAGHRIB | `#FF3B2A`, 0,55 | `#8A2A22` / `#4E1A18` / `#220C0A` | `#6E1E18`, 0,024 |
| 19:00 | Bulan `#7F8FC0`, 0,20 | `#1E2340` / `#171B2C` / `#0C0E16` | `#151A2A`, 0,026 |
| Malam Darah (override) | Bulan `#C8302A`, 0,25 | `#3A0E10` / `#24090A` / `#100405` | `#3A0C0C`, 0,034 |

### 7.4 Post-Processing (URP Volume global, profil disimpan sebagai aset)
- Tonemapping ACES; Color Adjustments (saturasi siang +8, malam −25, Maghrib +15 dengan hue shift −6); Bloom threshold 1,1 intensitas 0,6 (Rendah: off); Vignette 0,22 (naik ke 0,45 saat Sanity < 30, warna ungu `#3A1450`); Film Grain hanya preset Tinggi; Depth of Field OFF di HP.
- Efek layar sementara (via Volume kedua, weight dianimasikan): kena pukul = vignette merah 0,4 selama 0,3 detik; Kuntilanak menjerit = chromatic aberration 0,8 selama 1 detik; Malam Darah = color filter `#FFB0A8`.

### 7.5 Bahasa Bentuk Landmark
| Landmark | Ciri visual yang wajib terbaca dari jauh |
|---|---|
| Pohon Beringin Agung | Tinggi 28 m, tajuk payung lebar Ø 44 m, puluhan akar gantung, kain poleng hitam-putih + kain kuning melilit batang |
| Bangkai Helikopter | Badan putih-oranye survei, baling-baling bengkok, kolom asap hitam setinggi 40 m (partikel) di hari 1–2 |
| Mercusuar Tua | Menara putih kusam bergaris merah pudar, 22 m, lampu mati (menyala hijau saat Ending) |
| Desa Pasir Lenyap | Atap ijuk/rumbia miring curam, tiang kentongan tinggi 7 m dengan kain merah |
| Sawit Terbakar | Barisan batang hitam lurus tanpa daun, bara oranye di tanah, kabut merah rendah |
| Tebing Kapur Utara | Dinding putih-abu 35–45 m, air terjun putih tipis |

## 8. Referensi Rasa (deskriptif, bukan aset)
- Siang: rindang, lembap, cahaya menembus tajuk, suara tonggeret & burung.
- Maghrib: semua berubah merah, angin berhenti, kentongan dari arah tak terlihat.
- Malam: dunia kecil seluas cahaya obor; di luar itu hanya suara.
- Gua: sunyi berdengung, tetes air bergema, lumut bercahaya biru-hijau.
- Bunker: logam, neon berkedip, dengung listrik, dokumen berserakan.
- Kuil Akar: merah obor, akar seperti urat, detak rendah seperti jantung.

## 9. Catatan Legal Singkat
- Di dalam game, layar judul, dan metadata build: gunakan judul **"Alas Keramat"** (subjudul "Sons of Nusantara" boleh di layar judul). Jangan menyebut produk/merek lain.
- Semua nama tokoh, sekte, perusahaan (PT Bhumi Cakra), dan lembaga kolonial (Oost-Indische Onderzoek) adalah fiktif.
