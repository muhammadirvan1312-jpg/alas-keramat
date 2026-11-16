# 04 — CERITA (5 BABAK), MISI, GERBANG PROGRESI, 12 DOKUMEN LORE & NASKAH CUTSCENE
Dibaca pada tahap T10. Teks berbahasa Indonesia di dokumen ini adalah TEKS FINAL dan wajib disalin persis ke `LoreLibrary` / `QuestManager` / `CutsceneDirector`.

---

## 1. Premis
16 November. Andi Pratama (investigator geologi & kehutanan) dan Kang Deden Suryana (teknisi alat berat) diterbangkan ke zona larangan terbang **Alas Keramat** untuk mencari tim peneliti PT Bhumi Cakra yang hilang, termasuk Rendra — sahabat kuliah Andi. Kabut merah dari tajuk Pohon Beringin Agung melumpuhkan instrumen; petir merah menghantam heli. Mereka selamat di hutan jati selatan, tetapi hutan ini menyimpan kutukan: tiga Paku Keramat dari meteorit besi yang dulu mengunci retakan gaib telah dicabut perusahaan, dan penghuni hutan berubah menjadi abdi kegelapan di bawah Ki Rogo.

## 2. Lima Babak
| Babak | Hari (perkiraan) | Inti | Misi |
|---|---|---|---|
| 1. Jatuhnya Ekspedisi Bhumi Cakra | Hari 1 | Bangun, selamatkan Kang Deden, saung pertama | M01–M04 |
| 2. Rahasia Desa Pasir Lenyap & Gadis Kebaya | Hari 1–3 | Diawasi Sekte & Ningsih, desa mati, sawit, lore | M05–M07 |
| 3. Malam Darah & Pengepungan Saung | Malam 2–3 dst. | Raid pertama, Malam Darah pertama, gua pertama | M08 |
| 4. Penyatuan 3 Paku Keramat | Hari 4–10 | Bunker, jurang, kuil, boss, ritual, siege 60 detik | M09–M12 |
| 5. Dua Pilihan Takdir | Setelah siege | Helikopter SAR, ending A/B | M13 |

## 3. Daftar Misi (ID integer final)
Teks misi tampil kuning emas di bawah radar. Sub-target memakai format `(x/y)`.
| ID | Nama | Teks di HUD | Pemicu mulai | Syarat selesai | Hadiah / efek |
|---|---|---|---|---|---|
| 1 M01 | Koper Darurat | "Ambil Koper Darurat (0/3)" | Akhir cutscene intro | 3 koper dibuka | Tips kontrol tikar |
| 2 M02 | Tolong Kang Deden | "Tolong Kang Deden: beri Jamu, lalu tunjukkan Buku Catatan" | Bersamaan M01 | Interaksi `BERI JAMU` + `TUNJUKKAN CATATAN` | Perintah Ikuti/Tunggu/Istirahat terbuka |
| 3 M03 | Saung Sebelum Maghrib | "Bangun Saung Panggung & Api Unggun sebelum 18:00" | Bersamaan M01 | Blueprint Saung + Api Unggun selesai | Kasur = simpan pertama |
| 4 M04 | Malam Pertama | "Bertahan sampai pagi. Jaga api tetap menyala." | 18:00 Hari 1 | 06:00 Hari 2 dengan HP > 0 | Perintah Ambil Kayu/Ranting/Batu, Isi Rak, Jaga Api terbuka |
| 5 M05 | Desa Pasir Lenyap | "Selidiki desa di timur sungai (cari Cangkul)" | Lore #1 dibaca atau Hari 2 07:00 | `Cangkul` + Lore #2 diambil | Semua perintah Deden terbuka |
| 6 M06 | Gadis Berkebaya | "Dekati gadis berkebaya tanpa senjata terhunus" | Penampakan pertama Ningsih | Trust ≥ 31 | Ningsih mulai datang ke kemah |
| 7 M07 | Pesan di Kebun Sawit | "Periksa kebun sawit terbakar di barat" | Lore #2 dibaca | Lore #3 dibaca | Penanda X bunker di GPS |
| 8 M08 | Gua Sungai | "Masuki gua di lembah bambu" | Hari 2 atau Lore #3 | `PakuKeramat1` + `KartuAksesBunker` + `AlatSelamRebreather` | Malam berikutnya = Malam Darah |
| 9 M09 | Bunker Belanda | "Gali tanah bertanda X dan buka bunker" | M07 selesai | `PakuKeramat2` + `ZiplineGun` + keluar bunker | Malam berikutnya = Malam Darah |
| 10 M10 | Jurang Kapur | "Seberangi Jurang Kapur dengan tali luncur" | `ZiplineGun` didapat | Pemain di sisi utara jurang | — |
| 11 M11 | Kuil Akar Hitam | "Masuki Kuil Akar Hitam dan rebut Paku Ketiga" | M10 selesai | Genderuwo kalah + `PakuKeramat3` | Pintu keluar ke Beringin |
| 12 M12 | Ritual Beringin | "Tancapkan tiga Paku Keramat di Altar Beringin (x/3)" | Memiliki ≥ 1 paku | 3 paku tertancap + bertahan 60 detik | Kutukan runtuh, heli SAR datang |
| 13 M13 | Pulang atau Bertahan | "Temui Helikopter SAR di Pantai Selatan" | Akhir siege | Pilihan ending dibuat | Ending A/B |
Misi paralel boleh aktif bersamaan; HUD menampilkan maksimal 2 baris (prioritas ID terkecil yang belum selesai, plus M06 jika aktif).

## 4. Grafik Gerbang Progresi
```
Intro -> M01/M02/M03 -> M04 (malam 1)
             |
   Lore#1 -> M05 Desa (CANGKUL) -> Lore#2 -> M07 Sawit -> Lore#3 (X bunker)
             |                                                  |
             +--> M08 Gua Sungai: Rebreather + KARTU + PAKU 1   |
                              |                                 |
                              +------- butuh CANGKUL + KARTU ---+--> M09 Bunker: SENTER + ZIPLINE GUN + PAKU 2
                                                                                 |
                                                         M10 Jurang (butuh ZIPLINE GUN) -> M11 Kuil: PAKU 3
                                                                                 |
                                          M12 Altar (3 paku) -> Siege 60 detik -> M13 Ending
```
Gerbang keras: Cangkul (gali bunker), Kartu (pintu D1 bunker), Rebreather (terowongan U1), Zipline Gun (jurang). Gerbang lunak: Senter (Lorong Angin di UG3 memadamkan obor — tanpa senter pemain gelap & Sanity turun cepat).

## 5. Jadwal Malam & Peristiwa
- **Maghrib (setiap hari 18:00)**: transisi langit 17:30–18:10 ke merah; 3 kentongan berbunyi dari 3 arah (posisi 3D 60–90 m dari pemain, di arah hutan); angin berhenti; HUD mengingatkan "Kembali ke kemah." (hanya Hari 1–3).
- **Malam 2**: raid uji pertama (2–3 pengintai Sekte, lihat 06).
- **Malam Darah**: malam Hari 3 (cerita), lalu setiap 4 malam (Hari 7, 11, 15, ...), DAN malam setelah mendapatkan Paku 1 atau Paku 2 (jika belum dijadwalkan). Bulan merah, kabut darah, populasi hantu ×2, raid Sekte terbesar sesuai agresi. Dimulai 19:00, berakhir 05:00.
- Setelah Ending B (sandbox): tidak ada Malam Darah dan tidak ada hantu.

## 6. Dua Belas Dokumen Lore
Objek lore = kertas/kain/papan di dunia dengan cahaya pulsa kuning samar (radius 0,6 m) agar mudah dilihat. Interaksi "BACA" → UI kertas. Setelah dibaca, tersimpan di Jurnal (menu Buku Catatan halaman "Jurnal").

### 6.1 Lore Wajib (teks asli spesifikasi v1 — salin persis)
**#1 — Di Koper Helikopter (K1, Zona 1)**
Judul: "Surat Tugas Andi Pratama"
> "Surat Tugas Andi Pratama — Peringatan: Citra termal menunjukkan struktur logam peninggalan Hindia Belanda tahun 1928 di bawah perkebunan sawit barat dan anomali elektromagnetik di titik pusat pulau."

**#2 — Di Desa Mati Pasir Lenyap (R1 Rumah Kepala Desa)**
Judul: "Catatan Kepala Desa (1998)"
> "Catatan Kepala Desa (1998) — Sejak perusahaan membuka lahan sawit dan mencabut Paku Besi Kuning dari akar Beringin Agung, warga tidak bisa tidur. Setiap Maghrib, suara kentongan berbunyi sendiri dari hutan. Ningsih, anak juru kunci, menolak ikut ritual Ki Rogo dan lari ke lembah sungai."

**#3 — Di Perkebunan Sawit Terbakar (papan kayu (−226,160))**
Judul: "Pesan Darah di Papan Kayu"
> "Pesan Darah di Papan Kayu — 'KITA YANG MENGHIRUP ASAPNYA, MEREKA YANG MEMBANGUNKAN IBLISNYA. BUNKER BELANDA TERKUBUR DI BAWAH TANAH GEMBUR BARAT LAUT.'"

**#4 — Di Gua Sungai Bawah Tanah (UG1 R1)**
Judul: "Jurnal Tim Ekspedisi Pertama"
> "Jurnal Tim Ekspedisi Pertama — Air di gua ini mengandung spora hitam yang memicu halusinasi akut. Kami menemukan Paku Keramat pertama di altar batu, tapi dua rekan kami diterkam sosok terbungkus kain kafan yang melompat dari kegelapan. Ambil Alat Selam dan Kartu Bunker ini jika kau masih hidup."

**#5 — Di Bunker Kolonial Belanda (UG2 R1)**
Judul: "Arsip Oost-Indische Onderzoek (1928) & Laporan PT Bhumi Cakra"
> "Arsip Oost-Indische Onderzoek (1928) & Laporan PT Bhumi Cakra — Tiga Paku Keramat dari meteorit besi kuno dulunya ditancapkan oleh leluhur Nusantara untuk mengunci retakan kabut di bawah Beringin Agung. Saat ketiganya dicabut untuk penambangan, penghuni hutan kehilangan akal sehat dan berubah menjadi abdi kegelapan."

**#6 — Di Kuil Akar Beringin Hitam (UG3 K2)**
Judul: "Wasiat Juru Kunci (Ayah Ningsih)"
> "Wasiat Juru Kunci (Ayah Ningsih) — Hanya ketika ketiga Paku Keramat ditancapkan kembali ke batu altar Beringin Agung dan dipertahankan melewati Malam Darah, kutukan Alas Keramat akan runtuh dan langit kembali terbuka bagi sinyal penyelamat."

Koreksi v1: lokasi Lore #3, #4, #5 di Bagian 2 v1 sempat bertentangan dengan Bagian 3 v1. Tabel di atas adalah lokasi FINAL.

### 6.2 Lore Tambahan (opsional, memperdalam cerita)
**#7 — Puncak Mercusuar Tua**
Judul: "Buku Jaga Penjaga Mercusuar (1974)"
> "Malam ke-12 bulan ini lampu kembali menyala merah sendiri. Bukan dari menara — dari hutan. Kapal-kapal nelayan memutar haluan jauh sebelum karang. Kata orang desa, selama paku-paku masih tertancap, hutan hanya tidur. Aku tidak tahu paku apa yang mereka maksud. Aku hanya tahu: jangan pernah memadamkan lampu menara saat kabut datang dari utara."

**#8 — Sarang Ningsih (gubuk kecil di bawah rumpun bambu, (100,−175))**
Judul: "Surat yang Tak Pernah Dikirim"
> "Bapak, aku masih di lembah. Aku makan pisang dan ikan dari sungai seperti yang Bapak ajarkan. Tanda di lenganku menyala setiap kali mereka dekat — yang berkafan, yang tertawa di pohon. Aku tidak takut lagi pada gelap. Aku takut pada orang yang membawa golok dan tidak mau menyimpannya. Kalau suatu hari ada orang asing yang mau duduk diam di dekat apiku, mungkin aku akan berani menyapanya."

**#9 — Laboratorium PT (UG2 R2)**
Judul: "Log Laboratorium — Subjek N"
> "Hari 4. Subjek N (perempuan, 22 tahun, diserahkan oleh 'tetua' setempat) menunjukkan respons luar biasa terhadap paparan kabut cairan hitam: denyut nadi stabil, pupil bereaksi pada kegelapan total, tiga pola kulit berpendar di lengan dan bahu. Hari 6. Subjek N melarikan diri lewat saluran ventilasi. Subjek R (laki-laki, anggota tim survei) tidak melarikan diri: tulang jari memanjang, wajah tidak lagi dapat dikenali, dipindahkan ke ruang ventilasi. Rekomendasi: hentikan seluruh paparan. Rekomendasi ditolak oleh direksi."

**#10 — Gua Tawanan, Desa (305,120)**
Judul: "Daftar Tumbal (tulisan tangan Ki Rogo)"
> "Untuk membuka gerbang sepenuhnya, darah juru kunci harus mengalir di altar pada Malam Darah. Juru kunci tua sudah menolak dan sudah dikubur bersama sumpahnya. Tinggal anak gadisnya. Para abdi, cari dia di lembah. Jangan lukai wajahnya. Hutan ingin melihatnya utuh."

**#11 — Kamp Ekspedisi, UG1 R2**
Judul: "Pesan Terakhir Rendra"
> "Kalau Andi yang menemukan ini — maaf aku tidak menunggumu, Ndi. Kau benar soal data termal itu. Mereka bukan menambang bijih, mereka menambang sesuatu yang seharusnya dibiarkan terkubur. Kartu bunker kutinggalkan di peti. Jangan masuk ke kuil di utara sebelum kau punya cahaya yang tidak bisa dipadamkan angin."

**#12 — Ukiran di sisi Altar Beringin (terbaca setelah paku pertama ditancapkan)**
Judul: "Prasasti Batu Langit"
> "Tiga paku dari batu yang jatuh dari langit, ditempa di bawah bulan purnama, ditanam di akar pohon tua. Selama ketiganya bersatu, retakan tidur dan hutan menjaga manusia. Bila dicabut, hutan lapar. Bila dikembalikan, hutan kembali menjadi rumah."

## 7. Naskah Cutscene (in-engine; bisa dilewati setelah 3 detik dengan tombol "LEWATI", kecuali di playthrough pertama untuk intro)
Format: `waktu (detik) — kejadian`. Kamera memakai jalur Bezier & easing `SmoothStep`. Subtitle: teks putih ukuran 34, bayangan hitam, maks 2 baris, tampil 3,5 detik.

### 7.1 Intro "Kabut Merah" (± 38 detik)
- 0,0 — Layar hitam. Audio: putaran rotor heli (loop) + angin.
- 1,0 — Teks sinematik diketik (35 karakter/detik), tengah layar, ukuran 38:
  *"16 November — Tim Survei Geologi Andi Pratama & Teknisi Deden Suryana memasuki zona larangan terbang Alas Keramat untuk mencari tim peneliti yang hilang. Tiba-tiba kabut merah membumbung dari tajuk Pohon Beringin Agung, melumpuhkan instrumen kabin..."*
- 12,0 — Petir + kilatan merah 0,1 detik; guncangan kamera 0,6 selama 3 detik; teks merah berkedip "PERINGATAN: INSTRUMEN MATI"; bunyi alarm.
- 16,0 — Ledakan; guncangan 1,0; pudar ke hitam 0,5 detik.
- 17,0 — Hening 2 detik + dengung telinga 6 kHz memudar.
- 19,0 — Efek membuka mata: vignette hitam membuka dalam 2 kedipan selama 3 detik, saturasi naik dari 0 ke normal. Kamera = kepala Andi berbaring (−2,−212) menatap tajuk & asap.
- 23,0 — Kamera berputar ke bangkai heli terbakar, lalu ke Kang Deden di ekor heli: memegang telinga berdarah, menggeleng, menunjuk telinga, lalu menirukan gerakan menulis dan menunjuk koper K3.
- 30,0 — Subtitle: *"Andi: (Kang Deden... telinganya. Dia tidak bisa mendengarku.)"*
- 33,0 — Kartu misi: **"Ambil Koper Darurat, Tolong Kang Deden, dan Bangun Saung Sebelum Maghrib."**
- 36,0 — Kontrol kembali; gelembung tutorial joystick & tombol tangan.

### 7.2 Penampakan Pertama Ningsih (tidak memblokir kontrol)
Pemicu (yang pertama terjadi): pemain dalam 40 m dari (120,−160) setelah 11:00 Hari 1, ATAU pemain menebang di dalam 60 m dari Lapangan Kemah setelah 14:00 Hari 1. Ningsih muncul 35–45 m dari pemain di balik pohon (`Mengintip`), suara derit bambu + senandung pelan. Jika pemain menatapnya 1,5 detik: subtitle *"Andi: (Seorang gadis... berkebaya? Di tengah hutan ini?)"*. Setelah 4 detik atau jika pemain mendekat: `Kabur`. M06 dimulai.

### 7.3 Sekte Pertama (tidak memblokir)
Hari 1 ±15:00 atau saat pemain dalam 120 m dari Desa: satu Abdi Sekte bertopeng menatap dari 35 m lalu menghilang ke semak. Subtitle: *"Andi: (Ada yang mengawasi... topeng kayu?)"*.

### 7.4 Kang Deden Membaca Catatan (M02)
Andi mengulurkan Buku Catatan (layar Buku Catatan terbuka) → Deden `BacaCatatan` → `SenyumMengangguk2x` → `JempolKanan`. Subtitle: *"Andi: (Oke. Mulai sekarang kita bicara lewat tulisan, Kang.)"*

### 7.5 Boss Kuil Akar (± 14 detik)
- 0 — Kamera menyapu kubah dari pintu ke panggung; tungku obor menyala satu per satu (bunyi "whoosh").
- 5 — Ki Rogo berbalik. Subtitle: *"Ki Rogo: Kau datang mengembalikan paku-paku itu, anak kota? Hutan ini sudah memilih tuannya."*
- 10 — Ki Rogo menghentakkan tongkat; dinding akar pecah; Genderuwo keluar dan mengaum (guncangan 0,8).
- 14 — Pertarungan dimulai; bar HP boss "GENDERUWO" di atas tengah layar.
Setelah Genderuwo kalah (± 8 detik): *"Ki Rogo: Belum berakhir... Malam Darah akan menjemputmu."* Ki Rogo tenggelam ke akar (dissolve); kepompong terbuka menampakkan Paku 3.

### 7.6 Ritual & Siege
- Menancapkan paku: interaksi "TANCAPKAN PAKU" di altar (urutan bebas). Setiap paku: lubang menyala emas + dentang gong + Sanity +10.
- Paku ketiga (± 8 detik cutscene): petir merah menyambar batang beringin, tanah bergetar, angin kencang. Teks tengah: **"BERTAHAN 60 DETIK"**; bar waktu di atas tengah.
- Selama siege: Andi, Kang Deden, dan Ningsih (jika hidup) bertahan; detail gelombang di 06 §9.
- Detik 60: cahaya emas menembus tajuk (sinar volumetrik palsu = quad aditif), semua musuh larut jadi abu, kabut merah hilang, jam dipaksa ke 05:40 lalu matahari terbit dipercepat 10×. Gong + akor gamelan. 6 detik kemudian: suara heli di kejauhan; kolom suar hijau di (0,−420). Subtitle: *"Andi: (Sinyalnya tembus. Mereka datang.)"* M13 dimulai.

### 7.7 Pilihan Takdir Akhir
Saat Andi dalam 8 m dari pintu heli: layar gelap 60%, judul "PILIHAN TAKDIR", dua tombol besar:
- **"NAIK HELIKOPTER & PULANG"**
- **"AMBIL RANSEL & TETAP MENJAGA ALAS KERAMAT"**
Jika Deden/Ningsih gugur, teks ending menyesuaikan (kalimat tentang tokoh itu diganti kalimat penghormatan).

### 7.8 Ending A — Pulang (± 30 detik)
Andi merangkul bahu Kang Deden, mengulurkan tangan kepada Ningsih; ketiganya naik. Heli lepas landas; bidikan udara pulau saat matahari terbit; lampu mercusuar menyala hijau. Teks: *"Alas Keramat kembali sunyi. Di bawah akar Beringin Agung, tiga paku kembali tertidur — dan kali ini, ada yang tahu cara menjaganya."* → Layar Statistik → Kredit → Menu (slot ditandai "Tamat A").

### 7.9 Ending B — Tetap Menjaga (± 30 detik)
Andi menyerahkan dokumen bukti kepada Kapten Hendra; pilot memberi hormat dan terbang. Andi, Kang Deden (jempol), dan Ningsih (tersenyum, bunga kantil di telinga) menatap heli dari pantai. Teks: *"Sebagian orang pulang. Sebagian lagi memilih menjadi penjaga."* → Layar Statistik → kembali ke dunia: **Mode Sandbox Tanpa Akhir** terbuka, **`GolokApiPusaka`** masuk inventaris, tanpa Malam Darah & hantu; sisa Sekte yang diampuni kembali menjadi warga dan perlahan menghuni Desa.

### 7.10 Layar Statistik
Hari Bertahan · Pohon Ditebang · Bangunan Dibuat · Musuh Dikalahkan · Hantu Diusir · Sekte Diampuni · Barang Dirakit · Jarak Ditempuh (km, 1 desimal) · Lore Ditemukan (x/12).

## 8. Tertangkap, Pingsan, & Mati
- HP 0 ketika ada Sekte dalam 25 m DAN jumlah tertangkap < 2 → **Pingsan**: pudar hitam, bangun di **Gua Tawanan** (305,120) pukul 06:00 berikutnya dengan HP 30; inventaris tetap, senjata yang dipegang jatuh di lokasi (penanda "Senjata Terjatuh" di radar). Subtitle: *"Andi: (Kepalaku... mereka membawaku ke sini?)"*. Pintu kurungan didobrak 6× (interaksi "DOBRAK", stamina 10 tiap kali). Lore #10 ada di sini.
- Selain itu (atau tertangkap ke-3): **Mati** → layar "Andi tidak sadarkan diri untuk selamanya..." → tombol "MUAT SIMPANAN TERAKHIR" / "MENU UTAMA".

## 9. Monolog Batin Andi (subtitle; maks 1 tiap 20 detik; tiap baris sekali per pemicu kecuali ditandai ↻)
| Pemicu | Teks |
|---|---|
| Kenyang < 25 ↻ | "(Perutku melilit. Aku harus makan sesuatu.)" |
| Haus < 25 ↻ | "(Tenggorokanku kering sekali...)" |
| Basah > 70 malam ↻ | "(Dingin... aku butuh api.)" |
| Malam pertama | "(Gelap sekali. Jangan jauh-jauh dari api.)" |
| Pertama melihat Pocong | "(Itu... tidak. Itu tidak mungkin nyata.)" |
| Pertama tawa Kuntilanak | "(Suara tawa... dari atas pohon?)" |
| Membunuh Sekte pertama | "(Mereka manusia. Atau... dulu manusia.)" |
| Mengampuni Sekte pertama | "(Pergilah. Aku tidak mau jadi seperti kalian.)" |
| Masuk gua pertama | "(Lumutnya menyala... seperti di film dokumenter.)" |
| Paku Keramat 1 | "(Berat... dan hangat. Ini bukan besi biasa.)" |
| Paku Keramat 2 | "(Dua. Satu lagi.)" |
| Paku Keramat 3 | "(Sudah lengkap. Saatnya mengembalikan semuanya.)" |
| Sanity < 30 ↻ | "(Itu... nyata? Tidak, tidak mungkin.)" |
| Deden terluka | "(Kang Deden! Bertahan, Kang!)" |
| Hadiah pertama Ningsih | "(Sirih? ... Terima kasih.)" |
| Ningsih diberi senjata | "(Kau lebih jago memanah daripada aku, ya.)" |
| Baca Lore #11 | "(Rendra... aku akan menyelesaikan ini.)" |
| Pohon pertama tumbang | "(Awas — TIMBER!)" |
| Membangun saung selesai | "(Lumayan. Setidaknya ada atap.)" |
| Malam Darah pertama | "(Bulannya... merah.)" |
| Stamina habis ↻ | "(Napasku... habis.)" |
| Melihat diri di air (BERCERMIN) | "(Masih aku. Tahi lalat ini, wajah lelah ini. Masih aku.)" |
