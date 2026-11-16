# 17 — EVALUASI JUJUR v2 vs ACUAN SOTF & PENINGKATAN MEKANIK v3
Dibaca di setiap tahap yang disebut di kolom "Tahap". Dokumen ini MENAMBAH dan, bila bertentangan, MENGALAHKAN angka di dokumen 05–09.

---

## 1. Rapor Evaluasi
| Aspek | Nilai v2 | Celah utama dibanding acuan | Perbaikan v3 | Dokumen | Tahap |
|---|---|---|---|---|---|
| Rasa takut | 4/10 | Hantu hanya "musuh lain"; tanpa ritme, tanpa antisipasi | Sutradara Teror, 37 kejadian, folklor sebagai aturan, audio binaural, gelap sungguhan | 13 | T9 |
| Gore | 1/10 | Musuh larut jadi abu | Darah, luka, mutilasi, ragdoll, mayat, dekorasi kamp, 3 level gore | 14 | T7 |
| Visual | 4/10 | Material Lit standar, langit bawaan, tekstur polos | AK Shader Pack, tekstur pipa kualitas tinggi, LUT, sinar tajuk, kabut tinggi, jalur CC0 | 15 | T2, T4, T11 |
| Dunia hidup | 4/10 | Hewan & Sekte hanya "spawn" | Ekosistem, kampung Sekte berjadwal, 13 kejadian dunia, keseharian teman, cerita lingkungan | 16 | T9 |
| Tempur | 6/10 | Tanpa stealth kill, elakan, upgrade, senjata api modern, zona tubuh | §2 | 17 | T7 |
| Stealth | 5/10 | Hanya jongkok di semak | Meter suara, umpan, sembunyikan mayat, kamuflase lumpur, pencarian Sekte | 17 §2.4 | T7 |
| Bangunan | 6/10 | Hanya kayu; tanpa penyimpanan, jendela, jembatan, rumah pohon | Batu, potong celah, 12 blueprint baru | 17 §4 | T6 |
| Musuh | 6/10 | Ragam mutan kurang horor tubuh | Kuyang, Tuyul, Genderuwo Hutan, Gembung, Jemari, Penyerupa, Hantu Air | 13, 17 §5 | T7, T9 |
| Cerita | 7/10 | Kurang ikatan emosional | Rendra, patung untuk Dewi, makam ayah Ningsih, epilog bercabang | 17 §6 | T10 |
| Companion | 7/10 | Kurang kehidupan pribadi | Catatan Deden, kebun Ningsih, perintah pagar keliling | 16 §6, 17 §7 | T8, T9 |
| UI/UX | 6/10 | HUD selalu tampil, kurang imersif | Mode Imersif (jam & GPS di tangan), prompt di dunia, meter suara, mode kustom | 17 §8 | T3, T11 |
| Kelanjutan main | 5/10 | Satu jalur | Misi sampingan, epilog, Mode Kustom, loot & kejadian acak | 17 §6, §8 | T10, T11 |

## 2. Tempur & Stealth v3
### 2.1 Senjata baru (ID final)
| ID | Senjata | Statistik | Didapat |
|---|---|---|---|
| 118 | Paku Besi Kuning | Pengikat Kuntilanak (13 §4.1) | Resep R17 |
| 119 | Pistol PT | 35 damage, magasin 8, isi ulang 1,8 s, hitscan 50 m, bunyi 100 m | UG2 R3 (+16 peluru) |
| 120 | Peluru Pistol | maks 32 | Peti terdampar, Watu Ireng |
| 121 | Senapan Laras Pendek | 8 butir × 12, sebaran 6°, 2 peluru, isi ulang 2,5 s, dorong mundur, bunyi 140 m | Gubuk kepala Watu Ireng (+6 peluru) |
| 122 | Peluru Senapan | maks 18 | Resep R23 |
| 123 | Mandau | 24 / 56, tebang 18, Pendarahan musuh 30%, 0,42 / 0,95 s | Batu ritual Watu Ireng |
| 124 | Tombak Logam | Tusuk 28, lempar 70 | Resep R19 |
| 125 | Busur Tanduk | Damage panah ×1,25, tarik 0,35–1,0 s | Resep R20 |
| 126 | Panah Tulang | 34, menembus pelindung Buto | Resep R21 |
| 127 | Panah Racun | 22 + Racun 4/s 8 s (tidak untuk hantu) | Resep R22 |
Ningsih (Trust ≥ 90) juga bisa diberi Pistol PT (08 §3.2 menu tambahan "Berikan Pistol").
### 2.2 Gerak tempur
- **Elak**: saat senjata terhunus dan musuh ≤ 6 m, tombol LOMPAT berubah menjadi **ELAK** → langkah 2,5 m ke arah joystick (default mundur), kebal 0,25 s, stamina −12.
- **Bunuh Senyap**: dari belakang (sudut ≤ 60°), jongkok, musuh Kesadaran < 30, ≤ 1,5 m, senjata tebas/tusuk → tombol TEBAS menjadi **"BUNUH SENYAP"**: mati seketika (Buto & Pemimpin: damage ×3), klip 1,2 s, bunyi 4 m.
- **Asah Senjata**: di Batu Asah (B17) "ASAH" 3 s → damage senjata tebas +15% untuk 40 pukulan (ikon kecil di slot).
- **Zona tubuh** (14 §4) berlaku untuk semua senjata; bedil/pistol ke kepala ×2.
### 2.3 Resep baru (ID final)
| ID | Hasil | Bahan | Alat |
|---|---|---|---|
| R17 (17) | Paku Besi Kuning ×1 | LogamBekas 1 + Kemenyan 1 | Tempa di Api Unggun 20 s |
| R18 (18) | Perban Kain ×2 | Kain 1 | — |
| R19 (19) | Tombak Logam | TombakBambu 1 + LogamBekas 1 + TaliIjuk 1 | Golok |
| R20 (20) | Busur Tanduk | BusurBambu 1 + Tulang 3 + TaliIjuk 1 | — |
| R21 (21) | Panah Tulang ×4 | Ranting 2 + Tulang 1 + Bulu 2 | Golok |
| R22 (22) | Panah Racun ×2 | AnakPanah 2 + JamurBeracun 1 | — |
| R23 (23) | Peluru Senapan ×2 | LogamBekas 2 + MesiuHitam 1 | — |
### 2.4 Stealth
- **Meter suara**: lingkaran tipis di sekitar crosshair melebar sesuai radius bunyi Andi saat itu (jalan 8 m, lari 20 m, jongkok 3 m, tebang 45 m, tembak sesuai senjata).
- **Umpan**: lempar Batu/Kelapa (tombol LEMPAR) → bunyi jatuh menarik Sekte memeriksa titik itu (Kesadaran +35 dalam 15 m).
- **Seret mayat**: "SERET MAYAT" (kecepatan ×0,5) → sembunyikan di semak; mayat terlihat Sekte = Kesadaran +60 & Isyarat.
- **Kamuflase Lumpur**: di Rawa "BALUR LUMPUR" → deteksi visual ×0,7 selama 10 menit nyata; hilang saat hujan/berenang; Andi tampak berlumpur di FP.
- **Pencarian Sekte**: kehilangan jejak → memeriksa posisi terakhir; malam membawa obor menyisir 60 s dalam pola melingkar.

## 3. Item Baru Lain (ID final)
| ID | Item | Keterangan |
|---|---|---|
| 24 | Potongan Tubuh | Gore Penuh; bahan Patung Ngeri; maks 4 |
| 25 | Topeng Sekte | Dari mayat Sekte; bahan patung; maks 6 |
| 26 | Batu Besar | Fisik, dipanggul 1 di bahu seperti kayu; bahan bangunan batu |
| 71 | Kepiting | Makanan (+8 Kenyang mentah, +18 dimasak) & penangkal Tuyul |
| 72 | Perban Kain | Menghentikan Pendarahan, +10 HP |
| 193 | Patung Kayu untuk Dewi | Kenangan dari Kang Deden (tidak bisa dibuang) |
| 194 | Tanda Pengenal Rendra | Kenangan dari UG2 (tidak bisa dibuang) |
Status baru Andi: **Pendarahan** (14 §4), **Racun** (−0,3 HP/s 30 s; obat KunyitObat/JamuHerbal), **Kamuflase Lumpur**.

## 4. Bangunan v3
### 4.1 Mode bebas tambahan
- **Batu**: Batu Besar dipanggul (maks 1, kecepatan ×0,7) → mode **Fondasi Batu** (blok 0,6 × 0,6 × 0,4, HP 500, tidak terbakar), **Dinding Batu** (ditumpuk), **Jalan Batu** (pijakan datar, rumput tidak tumbuh).
- **Potong Celah**: pada dinding balok, "POTONG CELAH" (3 pukulan) → bukaan jendela (0,8 × 0,8) atau pintu (1,0 × 2,0); Pintu Bambu bisa dipasang di celah pintu.
### 4.2 Blueprint baru (`BlueprintID` 15–26)
| ID | Nama | Biaya | Fungsi |
|---|---|---|---|
| 15 | Mangkuk Kepiting | Batu 2 + Kepiting 1 | Penangkal Tuyul (13 §4.5) |
| 16 | Patung Ngeri | 14 §6 | Effigy kuat |
| 17 | Batu Asah | Batu 4 + Ranting 2 | "ASAH" senjata |
| 18 | Rumah Pohon | Kayu 14 + Ranting 10 + Tali 4 + Daun 12 | Di pohon Jati/Mahoni hidup; panggung 4 m + tangga tali; tidur & simpan; Sekte sulit menyerang |
| 19 | Perangkap Punji | Bambu 8 + Ranting 4 (+ gali dengan Cangkul) | Lubang bambu runcing tersembunyi: 120 damage, terjebak 3 s |
| 20 | Jerat Hewan | Tali 2 + Ranting 3 | Menangkap ayam hutan/kancil (peluang 30% per 2 jam game) |
| 21 | Bubu Ikan | Bambu 4 + Tali 2 | Di sungai: +1 ikan per jam game (maks 4) |
| 22 | Peti Penyimpanan | Bambu 4 + Ranting 6 + Tali 2 | Simpan 40 unit item; ditampilkan di tikar kedua |
| 23 | Lampu Minyak | LogamBekas 1 + MinyakTanah 1 + Kain 1 | Cahaya 10 m selama 8 jam game; Sanity +0,1/s dekatnya |
| 24 | Jembatan Gantung | Tali 6 + Papan 6 | Antara 2 jangkar/pohon/tiang ≤ 15 m; bergoyang |
| 25 | Gerbang Pagar | Kayu 4 + Tali 2 | Celah di pagar runcing; terbuka otomatis untuk Andi & teman |
| 26 | Kursi & Meja | Kayu 2 + Papan 2 | "DUDUK": Rest +0,5/s, Sanity +0,1/s |

## 5. Musuh Baru (`EnemyArchetype` 13–19, ID final)
| ID | Nama | HP | Gerak | Serangan | Keterangan |
|---|---|---|---|---|---|
| 13 | Kuyang | 90 | Terbang 5 m/s | Gigit menukik 14 + Pendarahan | 13 §4.4 |
| 14 | Tuyul | — | 6 m/s | Tidak menyerang | Mencuri; tidak bisa dilukai (13 §4.5) |
| 15 | Genderuwo Hutan | 400 | 2,0 / 6,0 | Hantam 25, lempar batu 12 | Malam Darah, mengganggu kemah (13 §4.3) |
| 16 | **Mutan Gembung** | 150 | 1,0 | Pukul 15; **meledak** jadi awan spora hitam r 4 m 6 s (Sanity −20, Racun) bila mati dekat atau 20 s di dekat Andi | Subjek lab gagal, tubuh bengkak penuh urat hitam; UG2 & Sawit malam (Hari ≥ 6) |
| 17 | **Mutan Jemari** | 180 | 7,5; merayap di dinding & langit-langit ventilasi | Cakar 18 × 2 | Tinggi kurus 2,3 m, jari sangat panjang; goyah bila disorot senter 1 s; UG2 & permukaan malam Hari ≥ 8 |
| 18 | Penyerupa | 60 | — | Tidak menyerang | E15; lenyap bila dipukul |
| 19 | Hantu Air | — | — | Tarik ke bawah (E34) | Kejadian, bukan unit |
Mutan memusuhi semua (termasuk Sekte). Darah hitam; mutilasi berlaku (14 §5).

## 6. Cerita v3 (tanpa misi utama baru)
- **Rendra**: Mutan Jemari pertama di UG2 R2 mengenakan **tanda pengenal ekspedisi bernama "RENDRA S."**. Setelah dikalahkan, "PERIKSA" → item 194 + monolog: *"(Rendra... maafkan aku. Aku terlambat.)"* Lore #9 kini menyebut "Subjek R" selain "Subjek N".
- **Patung untuk Dewi**: 16 §6.1; bila Deden gugur, Ending A menampilkan Andi menyerahkan patung kepada anak perempuan Deden di Garut.
- **Makam ayah Ningsih**: 16 §3 & §6.2; Ending B menampilkan Ningsih dan Andi menanam kantil di makam.
- **Misi Sampingan** (`QuestID` 101–104): S01 *Kenangan untuk Dewi* (jaga Deden tetap hidup sampai ending), S02 *Mencari Rendra* (temukan tanda pengenal), S03 *Arwah yang Tersesat* (lepas tali 5 Pocong), S04 *Persembahan* (terima 3 persembahan Sekte).
- **Epilog bercabang**: setelah Ending A/B, 3–6 slide teks + gambar adegan statis, dipilih dari: Deden hidup/gugur, Ningsih Trust ≥ 70/gugur, Sekte Diampuni ≥ 5 (Desa hidup kembali), Arwah Dilepas ≥ 5 (Makam tenang), Rendra ditemukan, Hantu Diikat ≥ 3.

## 7. Companion v3
- **Perintah Deden ke-15** (`CompanionOrder` 14 `BangunPagarKeliling`): membangun pagar runcing melingkar r 12 m di sekitar kemah dengan celah gerbang, memakai kayu/bambu dari Rak; berhenti bila bahan habis. Halaman "Kemah".
- Catatan Deden (16 §6.1), kebun & makam Ningsih (16 §6.2), Ningsih menerima Pistol (Trust ≥ 90).

## 8. UI/UX v3
- **Mode Imersif** (Pengaturan → Tampilan): bar vitals disembunyikan; Andi melihat **jam kompas** di pergelangan kiri (tombol jam kecil atau otomatis saat menunduk ke tangan) yang menampilkan jam, hari, cincin darah/stamina/kenyang/haus; radar tampil di **layar GPS** di tangan kiri (render ke tekstur 256). Misi hanya muncul 6 s saat berubah.
- **Prompt di dunia**: label kecil melayang di atas objek interaksi terdekat (selain tombol TANGAN).
- **Mode Kustom** (Menu → Mulai Baru → Kustom): Agresi Sekte, Frekuensi Hantu, Laju Lapar/Haus, Damage, Gore, Intensitas Horor, Musuh merusak bangunan (ya/tidak), Pohon tumbuh lagi, Panjang musim, Companion gugur permanen.
- Indikator status (Pendarahan, Racun, Lumpur, Asah) sebagai ikon kecil di sebelah bar darah.

## 9. Anggaran Performa Tambahan
| Sistem | Batas |
|---|---|
| Satwa kecil (burung, monyet, kepiting) | 25 aktif, AI 2 Hz, animasi sederhana |
| Kejadian dunia | maks 2/hari |
| TrailMap | update 1×/menit, tekstur 256 R8 |
| Penampakan & kejadian teror | maks 1 objek penampakan aktif |
| Perangkap aktif | 20 |
| Total musuh aktif (termasuk mutan) | tetap ≤ 12 |
