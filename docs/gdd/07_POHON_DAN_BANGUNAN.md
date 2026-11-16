# 07 — TEBANG POHON 4 KUADRAN, KAYU FISIK, PANGGUL BAHU, FREEFORM 5 MODE, BLUEPRINT & ZIPLINE
Dibaca pada tahap T5 (pohon & kayu) dan T6 (bangunan & blueprint).

---

## 1. Jenis Pohon
| Jenis | Tinggi | Jari-jari batang | HP tebang | Hasil saat rebah | Ekstra |
|---|---|---|---|---|---|
| Jati | 14–18 m | 0,35 | 120 | 4 Kayu Gelondong + 2–3 Ranting | Daun lebar besar |
| Mahoni | 16–20 m | 0,40 | 140 | 4 Kayu + 2 Ranting | Pukul batang (sebelum rebah) → Damar 1 (maks 2/hari) |
| Kelapa | 10–14 m (melengkung) | 0,18 | 70 | 2 Kayu + 2–3 KelapaMuda jatuh | Lempar batu ke tajuk → 1 Kelapa |
| Aren | 12 m | 0,25 | 90 | 3 Kayu | 3 pukulan pertama per hari → SeratIjuk 1 tiap pukul |
| Sawit Hangus | 7 m | 0,22 | 50 | 2 Kayu (hitam; +100 s saat dibakar) | Rapuh, partikel abu |
| Rumpun Bambu (8–12 batang) | 8–11 m | 0,05 per batang | 2 pukulan/batang | Bambu ×1 per batang (masuk inventaris) | Rumpun tumbuh lagi 2 hari |
| Pisang | 4 m | 0,12 | 1 pukulan | DaunPisang 3 + Pisang 1–2 | Tumbuh tunas 3 hari |
| Beringin Kecil / Agung | — | — | TIDAK BISA | — | Memukul: Sanity −5 + tawa Kuntilanak |
- Mesh pohon: batang 4 segmen loft (bisa melengkung untuk kelapa), cabang utama 3–6, tajuk = gumpalan daun cutout (atlas daun prosedural 512×512: daun jati lebar, daun mahoni majemuk, daun kelapa menyirip, daun bambu runcing). LOD0 ≤ 1.500 segitiga, LOD1 ≤ 400, LOD2 = 2 quad silang (billboard). Bergoyang angin lewat rotasi transform kecil (dekat) atau tidak sama sekali (jauh, instanced).
- Tunggul tersisa (tinggi 0,5 m). Opsi "Pohon Tumbuh Lagi" (default ON): tunggul → anakan setelah 5 hari → pohon penuh 10 hari.

## 2. Tebang dengan Takikan 4 Kuadran (`DirectionalTree`)
- Pohon menyimpan `cutDepth[4]` untuk kuadran dunia **Utara/Timur/Selatan/Barat**.
- Setiap pukulan menambah `damageTebang × (1 + 0,04×(StrengthLevel−1))` ke kuadran yang menghadap Andi (arah pohon→Andi dibulatkan ke kuadran terdekat). Visual: mesh takikan (irisan baji) di batang setinggi 0,9 m, dalamnya sesuai `cutDepth` kuadran itu.
- Total `Σ cutDepth ≥ HP` → **rebah**. **Arah rebah = berlawanan dengan kuadran takikan terdalam** (jadi pohon jatuh menjauhi sisi yang paling banyak ditebang). Bila dua kuadran sama dalam, ambil arah menjauhi Andi.
- Petunjuk mobile: setelah `Σ ≥ 60% HP`, garis panah tipis transparan di tanah menunjukkan arah rebah (memudar 1,5 s setiap pukulan).
- **Fisika rebah**: batang (tanpa tunggul) diberi `Rigidbody` (massa 400) + `HingeJoint` di pangkal; torsi awal ke arah rebah; joint dilepas setelah kemiringan 25°. Suara retak 6× (lihat 10) lalu gemuruh saat menghantam tanah.
- Saat menghantam (atau 4 s setelah mulai rebah): batang pecah → spawn Kayu Gelondong fisik di sepanjang sumbu batang (berurutan dari pangkal), ranting, kelapa; tajuk berubah jadi partikel daun berguguran 2 s lalu hilang.
- Rebah melindas: lihat 06 §12.
- Simpan: ID pohon yang sudah rebah + waktu rebah (untuk tumbuh lagi).

## 3. Kayu Fisik
| Bentuk | Ukuran | Massa | Didapat dari |
|---|---|---|---|
| Kayu Gelondong (ID 1) | panjang 3,6 m, r 0,22 | 45 | Pohon rebah |
| Setengah Gelondong | 1,8 m, r 0,22 | 22 | "POTONG PENDEK" (3 pukulan di tengah kayu yang tergeletak) |
| Papan Kayu (ID 9) | 3,6 × 0,36 × 0,08 m | 18 | "BELAH PAPAN" (3 pukulan memanjang) → 2 papan |
| Batang Bambu (ID 8) | 2,4 m, r 0,05 | 3 | Rumpun bambu (masuk inventaris, maks 6) |
- Kayu tergeletak: `Rigidbody` (drag 0,2, angular drag 0,5), layer `LogPhysics`, `CapsuleCollider`. Tidur fisika (`Sleep`) bila diam > 3 s. Maks 40 kayu aktif; yang terjauh dibekukan (kinematic) bila lebih.
- Kayu menggelinding menuruni lereng > 20° dan mengapung di air (gaya apung sederhana).

## 4. Memanggul di Bahu Kiri (`ShoulderLogAndFreeformBuilder`)
- "AMBIL KAYU" (lihat kayu dalam 2,5 m) → kayu ditempel ke `LeftShoulderMount` (sejajar arah pandang, condong 10°). Maks **2 Gelondong** (tumpuk, offset +0,24 m), atau 2 Setengah, atau **4 Papan**.
- Kecepatan: 1 kayu ×0,85, 2 kayu ×0,75; sprint boleh dengan biaya stamina ×1,5. Tidak bisa berenang membawa kayu (kayu dilepas otomatis & mengapung).
- Animasi FP: lengan kiri naik memegang kayu (klavikula +15°, lengan atas pitch −150°, siku 110°); saat menunduk (> 45°) kayu & lengan terlihat.
- "LEMPAR" melempar kayu teratas ke depan 3 m/s + 1 m/s ke atas.
- Menyimpan: dekati Rak Kayu → "TARUH DI RAK"; Kereta Dorong → "MUAT KAYU"; Blueprint → "TAMBAH KAYU"; Api → lempar ke api (+300 s bahan bakar).

## 5. Bangun Bebas — 5 Mode
Masuk otomatis saat memanggul kayu/papan/bambu dan melihat tanah/struktur dalam 3,5 m. Di samping tombol tangan muncul **cincin mode kecil** (5 ikon) untuk berganti mode; tombol tangan menampilkan aksi.
| Mode | Teks tombol | Aturan |
|---|---|---|
| 1. **Tiang** | "PASANG TIANG" | Kayu vertikal, terbenam 0,2 m ke tanah atau ditumpuk di atas tiang lain / soket ujung balok. Tinggi 3,6 (atau 1,8 untuk setengah). |
| 2. **Balok Dinding** | "PASANG BALOK" | Kayu horizontal di antara 2 soket tiang (jarak ≤ 3,6; bila lebih pendek kayu dipotong pas — sisa dibuang, pesan "Kayu dipotong pas"), ditumpuk di atas balok lain (+0,42 m), atau di tanah sebagai pondasi. |
| 3. **Atap/Tangga** | "PASANG ATAP" / "PASANG TANGGA" | Kayu miring dari soket atas ke balok lain/tanah, sudut 20–45° (atap) atau 25–40° (tangga: membuat 2 pengapit; anak tangga = papan disnap ke soket tangga). |
| 4. **Belah Papan** | "BELAH PAPAN" (pada kayu tergeletak) / "PASANG PAPAN" (saat membawa papan) | Papan melintang di atas 2 balok sejajar (jarak ≤ 3,6), saling menempel sisi (lebar 0,36) → lantai. |
| 5. **Serut Runcing** | "SERUT RUNCING" (pada tiang/bambu tertanam) / "TANAM BAMBU" (membawa bambu) | Pukul ujung atas 2× → ujung runcing = **Pagar Runcing** (15 damage/s ke musuh yang menempel, menghalangi). Bambu ditanam vertikal 2,4 m. |
- **Ghost Snap Preview**: material transparan hijau `#3CFF7A` α 0,35 (valid) / merah `#FF3C3C` α 0,35 (tidak valid); soket terdekat ditandai bola putih kecil; tombol putar ±15° (saat tidak tersnap); "BATAL" mengembalikan kayu ke bahu.
- **Soket**: `TiangAtas`, `TiangBawah`, `TiangSisi` (4 arah, tiap kelipatan 0,42 m), `UjungBalokA/B`, `TengahBalok`, `TepiPapan` (kiri/kanan), `AnakTangga`, `Tanah`. Pencarian soket dalam 1,2 m dari titik raycast kamera (jangkauan 3,5 m); pilih yang jarak + kecocokan orientasinya terbaik.
- Pantangan: tidak di air (kecuali rakit), tidak di Landasan SAR, lereng > 30° untuk tiang di tanah, tidak menembus karakter.
- **BONGKAR** (tahan tombol tangan 1 s pada potongan milik pemain) → potongan jatuh sebagai kayu/papan.

## 6. Integritas Struktur (`StructureIntegrity`)
- Graf: simpul = potongan, sisi = sambungan soket. Simpul **tertanam** = tiang menyentuh tanah atau balok pondasi di tanah.
- Saat potongan hancur/dibongkar: BFS dari simpul tertanam; potongan yang tidak terjangkau → diberi `Rigidbody`, runtuh, 20 s kemudian hilang dan menjatuhkan 50% kayu.
- Aturan bentang: balok harus ditopang di kedua ujung, ATAU satu ujung + titik tengah ≤ 1,8 m; papan butuh 2 penopang. Maks 6 tingkat (≈ 13 m).
- HP potongan: gelondong 200, papan 120, pagar runcing kayu 300, pagar runcing bambu 150. Terbakar: −5/s selama 20 s (hujan memadamkan).

## 7. Buku Panduan 3D & Blueprint (`GuidebookBlueprintsAndCooking`)
- Tombol buku → Andi memegang buku dengan dua tangan di depan kamera (model buku 3D, sampul batik coklat, halaman krem). Halaman = Canvas world-space di permukaan halaman. Geser kiri/kanan = balik halaman (animasi lengkung halaman 0,35 s + suara kertas). Tab kategori di tepi: **Kemah, Makanan & Air, Pertahanan, Utilitas**.
- Ketuk blueprint → mode penempatan: hantu biru transparan `#4FA8FF` α 0,3; hijau/merah validitas; putar ±15°; "TANDAI" → **cetak biru** tertancap di dunia.
- Isi bahan: dekati cetak biru dengan bahan → "TAMBAH KAYU / TAMBAH RANTING / TAMBAH BATU / TAMBAH DAUN / TAMBAH BAMBU / TAMBAH TALI". Potongan muncul solid satu per satu sesuai urutan bangun (pondasi → tiang → lantai → dinding → atap). Kang Deden bisa "Selesaikan Bangunan".
- Batalkan cetak biru: tahan 1 s → bahan yang sudah dimasukkan dikembalikan 100%.
### 7.1 Delapan Blueprint Inti (`BlueprintID` 1–8)
| ID | Nama | Biaya | Jejak | Fungsi |
|---|---|---|---|---|
| 1 | Api Unggun | Ranting 6 + Batu 4 | r 0,8 | "NYALAKAN API" (perlu KorekGas), masak 4 tusuk, cahaya, kering, Sanity |
| 2 | Saung Panggung + Kasur | Kayu 10 + Ranting 8 + Daun 16 | 3,6×3,6, panggung 1,0 m | 4 tiang, lantai papan, atap daun pelana, tangga kecil; **Kasur Pandan** (TIDUR & SIMPAN); di bawah atap = kering |
| 3 | Rak Kayu | Kayu 3 + Ranting 6 | 3,6×1,2 | Simpan 24 kayu (tumpukan terlihat) |
| 4 | Kereta Dorong Kayu | Kayu 4 + Ranting 4 + Tali 2 | 3,8×1,4 | Muat 8 kayu; "DORONG" (×0,8 kecepatan, tanpa sprint; meluncur di turunan) |
| 5 | Penampung Hujan | Kayu 2 + Bambu 3 + Daun 8 | 1,5×1,5 | Saat hujan +1 porsi air / 10 menit game (maks 6); "MINUM" / "ISI BOTOL" |
| 6 | Jemuran Ikan | Ranting 10 + Tali 1 | 2,0×0,8 | 6 slot pengeringan (120 s) |
| 7 | Perangkap Ayun Bambu | Kayu 2 + Bambu 6 + Tali 3 | 2,4×2,4 | Ayunan duri 200 damage; "SETEL ULANG" |
| 8 | Obor Tengkorak | Ranting 4 + Tengkorak 1 + Kain 1 + Damar 1 | r 0,5, tinggi 2,2 | "NYALAKAN" 240 s; menakuti Sekte 25 m (+40 Takut), hantu menghindar 15 m; nyala ulang: Damar 1 |
### 7.2 Enam Blueprint Lanjutan (`BlueprintID` 9–14)
| ID | Nama | Biaya | Fungsi |
|---|---|---|---|
| 9 | Tenda Darurat | Kain 2 + Ranting 4 | "SIMPAN", "ISTIRAHAT" (+1,5/s Rest) |
| 10 | Panci Masak | LogamBekas 2 + Batu 2 | Snap di atas Api Unggun; Sup Hutan (05 §5) |
| 11 | Menara Pengintai | Kayu 16 + Ranting 8 | Panggung 6 m + tangga; dari atas, musuh yang ditatap 1 s ditandai di radar 10 s |
| 12 | Pintu Bambu | Bambu 6 + Tali 2 | Snap di antara 2 tiang berjarak 1,2–1,8 m; "BUKA/TUTUP"; HP 150 |
| 13 | Kentongan Kemah | Bambu 2 + Ranting 2 | "PUKUL KENTONGAN": Deden & Ningsih (Trust ≥ 70) pulang ke kemah; Sekte dalam 120 m +10 kesadaran |
| 14 | Rakit Bambu | Bambu 10 + Tali 4 | Di air; "NAIKI RAKIT"; 2,5 m/s (+ arus sungai), Andi + 1 companion |

### 7.3 Blueprint v3 (`BlueprintID` 15–26) & bangunan batu
Lihat `17_EVALUASI_DAN_PENINGKATAN_V3.md` §4 (Mangkuk Kepiting, Patung Ngeri, Batu Asah, Rumah Pohon, Perangkap Punji, Jerat Hewan, Bubu Ikan, Peti Penyimpanan, Lampu Minyak, Jembatan Gantung, Gerbang Pagar, Kursi & Meja; Fondasi/Dinding/Jalan Batu; Potong Celah).

## 8. Zipline (`ZiplineGun` + `TaliZipline`)
- Bidik batang pohon hidup, tiang struktur, atau `ZiplineAnchor` dalam 60 m → "TEMBAK TALI" (1 TaliZipline). Tali dibentang dari **Patok Tali** otomatis di kaki Andi ke titik tempel (5 m di atas pangkal pohon/tiang).
- Garis pandang harus bebas (raycast). Tali tersimpan di save.
- Meluncur: lihat tali dalam 2,5 m → "MELUNCUR". Turun: kecepatan = 6 + 18×sin(kemiringan), maks 16 m/s. Naik: 3 m/s tangan-demi-tangan, stamina −6/s. Lepas otomatis di ujung atau tombol "LEPAS". Kamera FP: kedua lengan ke atas memegang katrol; suara desis tali (10 §3).
- Wajib: menyeberangi Jurang Kapur (03 §9 Zona 6). Opsional: turun cepat dari tebing ke lembah (jangkar C→D).
- Tepi jurang: dalam 1 m dari tepi, kecepatan ×0,5 & tidak bisa sprint (mencegah jatuh tak sengaja).
