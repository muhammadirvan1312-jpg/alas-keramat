# 05 — SURVIVAL, WAKTU & CUACA, DAFTAR ITEM FINAL, MAKANAN & MASAK, 16 RESEP, ZIRAH
Dibaca pada tahap T2 (waktu/cuaca), T3 (vitals), T5 (item/crafting), T6 (masak).
Semua laju dalam **per detik nyata** (1 detik nyata = 1 menit game). Nilai dikalikan pengali kesulitan (§9).

---

## 1. Waktu
- 1 hari game = 1.440 detik nyata (24 menit). Hari 1 mulai 08:00; skala waktu 0,65 sampai 18:00 Hari 1 (siang tutorial ≈ 15,4 menit nyata), setelah itu 1,0.
- Label waktu di HUD: 00:00–03:59 **DINI HARI**, 04:00–05:59 **SUBUH**, 06:00–10:59 **PAGI**, 11:00–14:59 **SIANG**, 15:00–17:29 **SORE**, 17:30–18:59 **MAGHRIB**, 19:00–23:59 **MALAM**.
- Malam = 19:00–05:00. `GameEvents.OnHourChanged`, `OnDayChanged`, `OnMaghrib` (18:00).

## 2. Musim & Cuaca
- **Musim**: Kemarau (Hari 1–6, 13–18, …) ↔ Penghujan (Hari 7–12, 19–24, …).
- Cek cuaca tiap 3 jam game. Transisi bertahap (Cerah tidak langsung ke Badai; lewat Berawan). Transisi visual 40 detik.
| Cuaca | Peluang Kemarau | Peluang Penghujan | Efek |
|---|---|---|---|
| Cerah | 55% | 20% | Pengering +1%/s di siang terbuka |
| Berawan | 25% | 25% | Cahaya −25% |
| Gerimis | 10% | 25% | Basah +0,8%/s, bahan bakar api ×1,3, fog +0,004 |
| Hujan Badai | 5% | 30% | Basah +2,5%/s, angin kencang (pohon bergoyang ±6°), petir tiap 8–25 s (kilat + guruh tertunda jarak/340 m/s), fog +0,012, api tanpa atap padam setelah 60 s, langkah pemain teredam (deteksi Sekte −30%) |
| Kabut Asap | 5% (Kemarau saja) | 0% | Fog coklat +0,015, Haus ×1,2 |
| Kabut Pagi | otomatis 05:00–08:30 di lembah (h < 9) | sama | Fog +0,02 |
| Kabut Darah | otomatis 17:45–19:00 & seluruh Malam Darah | sama | Fog merah, Sanity −0,03/s |
- Hari 1 dipaksa Cerah sampai 18:00, lalu Berawan.
- Penghujan: permukaan sungai visual +0,4 m, faktor dingin ×1,2, ikan ×1,5. Kemarau: Haus ×1,4 pukul 10:00–15:00, bara di Sawit lebih besar.

## 3. Vitals Andi (`AndiVitalsAndSanity`)
### 3.1 Kesehatan
- `CurrentHealth` 0–100, `TargetHealthCeiling` = 100 − max(0, 25 − Fullness)×1,2 − max(0, 25 − Hydration)×1,6 − (Rest < 10 ? 10 : 0); minimum 20.
- Regenerasi +0,6/s bila Fullness > 40, Hydration > 40, dan tidak terkena damage 6 detik; +1,5/s saat tidur; tidak melebihi ceiling.
- Fullness = 0 → −0,25/s; Hydration = 0 → −0,5/s.
### 3.2 Stamina
- 0–100, dibatasi `MaxStaminaCap` = 100 − 0,5×ColdWet (maks −50) − (Rest < 20 ? 15 : 0) − (Sanity < 20 ? 10 : 0); minimum 25.
- Biaya: sprint −11/s (×1,5 saat memanggul kayu), lompat −8, tebas ringan −5, tebas berat −14, parry −6, pukulan tebang −6, renang −4/s (cepat −10/s), tahan busur −2/s setelah 2 detik.
- Pulih +16/s setelah 1,0 detik tanpa biaya (×0,5 bila Fullness < 20). Habis (0) → tidak bisa sprint sampai 25.
### 3.3 Basah & Dingin
- `Wetness` 0–100%: hujan (lihat tabel), berenang = 100. Mengering: dekat api ≤ 4 m +6%/s, di bawah atap (raycast ke atas 6 m kena `Structure`) +1,5%/s, siang cerah terbuka +1%/s, lainnya +0,3%/s.
- `ColdWet` = Wetness × ColdFactor; ColdFactor 0,4 (siang) → 1,0 (malam, interpolasi 17:00–20:00 & 05:00–08:00), ×1,2 Penghujan. Dekat api: ColdWet turun 8/s.
- Ditampilkan sebagai bagian biru beku di ujung bar stamina (porsi yang terpotong cap).
### 3.4 Kenyang, Haus, Istirahat
- Fullness: −0,055/s (penuh→0 ≈ 30 menit nyata), ×1,3 saat sprint/tebang, ×0,5 saat tidur.
- Hydration: −0,085/s (≈ 20 menit), ×1,2 sprint, ×1,4 Kemarau siang, ×0,5 tidur.
- Rest: −0,035/s (≈ 2 hari game). Tidur di Kasur Saung +3/s, Tenda/tanah +1,5/s. Rest < 20 → stamina cap −15; < 5 → efek menguap (vignette berkedip) & kecepatan ×0,9.
### 3.5 Kewarasan (Sanity) 0–100
| Turun | Nilai |
|---|---|
| Malam di luar tanpa cahaya dalam 6 m | −0,05/s |
| Di gua tanpa cahaya dekat | −0,06/s |
| Lorong Angin UG3 gelap | −0,3/s |
| Hantu bermusuhan < 15 m | −0,8/s |
| Jeritan Kuntilanak | −5 |
| Pertama melihat Pocong per sesi | −3 |
| Membunuh Sekte | −2 |
| Makan JamurBeracun | −15 |
| Memukul Beringin Agung / menyerang Penyu | −5 / −10 |
| Kabut Darah | −0,03/s |
| Naik | Nilai |
|---|---|
| Dekat api unggun / Obor Tengkorak menyala ≤ 8 m | +0,2/s |
| Tidur | +0,5/s (maks +30 per tidur) |
| Ningsih bersenandung/menari ≤ 10 m | +0,3/s |
| JamuHerbal | +10 |
| Bakar Kemenyan di api (interaksi "BAKAR KEMENYAN") | +20 |
| Mengampuni Sekte | +3 |
| Paku tertancap di altar | +10 per paku |
| Siang cerah di luar | +0,02/s |
Efek (tidak pernah langsung mengurangi HP; halusinasi diatur Sutradara Teror, lihat 13): < 60 bisikan acak; < 40 vignette ungu 0,3 + langkah palsu di belakang; < 25 halusinasi siluet Pocong di 25–40 m yang hilang bila didekati/ditatap 2 detik, kentongan palsu; < 10 regen stamina −30%, denyut layar, detak jantung.
### 3.6 Kekuatan (`StrengthLevel` 1–10)
XP: pukulan tebang +1, bunuh Sekte +8, usir hantu +12, panggul kayu per 10 m +0,2, makan sampai Kenyang > 80 +2. Level = 1 + floor(XP/150), maks 10. Efek per level: damage melee +5%, damage tebang +4%, biaya stamina −2%. Kenyang < 15 selama 1 hari game → turun 1 level.
### 3.7 Zirah 10 Segmen
Kotak kecil di atas bar darah. Pakai dari tikar ("PAKAI ZIRAH": +1 segmen per item, maks 10). Damage masuk diserap segmen terluar 100% hingga durabilitasnya habis; sisa ke HP.
| Zirah | ID | Durabilitas/segmen | Catatan |
|---|---|---|---|
| ZirahDaun | 180 | 8 | Rusak 2× lebih cepat saat hujan |
| ZirahBambu | 181 | 15 | Lemah api (×1,5) |
| ZirahTulang | 182 | 25 | Sanity −1 saat dipakai pertama |
| ZirahBaja | 183 | 40 | Kecepatan renang ×0,85 bila ≥ 6 segmen |
| ZirahEmas | 184 | 70 | Hantu ragu menyerang (agresi −20%) |

## 4. DAFTAR ITEM FINAL (`ItemID`, nilai integer TIDAK BOLEH DIUBAH)
> v3: item 24–26, 71–72, 118–127, 193–194, resep R17–R23, status Pendarahan/Racun/Kamuflase ada di `17_EVALUASI_DAN_PENINGKATAN_V3.md` §2–3 dan `14` §4.
Kolom "Maks" = jumlah maksimal dibawa. Kolom "Ikon" = bentuk dasar `IconPainter` (lihat 09 §8).
### 4.1 Bahan (1–49)
| ID | Nama enum | Nama tampil | Maks | Sumber utama | Ikon |
|---|---|---|---|---|---|
| 1 | KayuGelondong | Kayu Gelondong | 2 (dipanggul, fisik) | Tebang pohon | log |
| 2 | Ranting | Ranting | 30 | Semak, pohon tumbang | stick |
| 3 | Batu | Batu | 20 | Tanah hutan, sungai | rock |
| 4 | DaunPisang | Daun Pisang | 30 | Pohon pisang, semak | leaf |
| 5 | SeratIjuk | Serat Ijuk | 20 | Pohon aren | fiber |
| 6 | TaliIjuk | Tali Ijuk | 10 | Rakit / desa | rope |
| 7 | KainBatik | Kain Batik | 10 | Heli, desa, jemuran | cloth |
| 8 | Bambu | Batang Bambu | 6 | Rumpun bambu | bamboo |
| 9 | PapanKayu | Papan Kayu | 2 (fisik) | Belah gelondong | plank |
| 10 | Tulang | Tulang | 20 | Hewan, kerangka gua | bone |
| 11 | KulitHewan | Kulit Hewan | 6 | Hewan | hide |
| 12 | Tengkorak | Tengkorak Kerbau | 4 | Desa, sawit, babi hutan | skull |
| 13 | Bulu | Bulu Burung | 30 | Ayam hutan | feather |
| 14 | Damar | Getah Damar | 10 | Pohon mahoni (pukul batang 1×) | drop |
| 15 | Kemenyan | Kemenyan | 6 | Desa, Pocong, hadiah | incense |
| 16 | LogamBekas | Logam Bekas | 12 | Heli, ekskavator, bunker | gear |
| 17 | Baterai | Baterai | 10 | Gua, bunker | battery |
| 18 | MinyakTanah | Minyak Tanah | 4 | Desa, sawit | can |
| 19 | Arang | Arang | 20 | Rumah hangus, sisa api | coal |
| 20 | BotolKosong | Botol Kosong | 3 | Heli (Ransum), desa | bottle |
| 21 | MesiuHitam | Mesiu Hitam | 6 | Desa R3, bunker | powder |
| 22 | KainKafan | Kain Kafan | 6 | Pocong | cloth |
Resep yang meminta "Kain" menerima `KainBatik` ATAU `KainKafan`.
### 4.2 Makanan & Minuman (50–79)
| ID | Enum | Kenyang | Haus | HP | Lain | Basi |
|---|---|---|---|---|---|---|
| 50 | KelapaMuda | +6 | +22 | — | Perlu Golok/Kapak untuk membuka ("MINUM KELAPA") | tidak |
| 51 | Pisang | +10 | +2 | — | — | 4 hari |
| 52 | Jamur | +6 | — | — | — | 3 hari |
| 53 | JamurBeracun | +4 | — | −10 (10 s) | Sanity −15, layar bergoyang 8 s | — |
| 54 | DaunSirih | — | — | +8 | Sanity +2; hadiah Ningsih | tidak |
| 55 | KunyitObat | — | — | +5 | Hentikan keracunan | tidak |
| 56 | JaheHutan | — | — | — | ColdWet −20, Stamina +10 | tidak |
| 57 | IkanMentah | +8 | — | — | Sanity −2, 20% keracunan (HP −8/8 s) | 2 hari → DagingBusuk |
| 58 | IkanMatang | +22 | — | +4 | — | 3 hari |
| 59 | IkanKering | +18 | — | — | — | tidak |
| 60 | DagingMentah | +10 | — | — | Sanity −3, 35% keracunan | 2 hari → DagingBusuk |
| 61 | DagingMatang | +30 | — | +6 | — | 3 hari |
| 62 | DagingKering | +24 | — | — | — | tidak |
| 63 | TelurAyamHutan | +8 | +2 | — | — | 3 hari |
| 64 | RansumDarurat | +40 | +5 | — | Sanity +5, sisakan `BotolKosong` | tidak |
| 65 | BotolAirBersih | — | +35 | — | Jadi `BotolKosong` | tidak |
| 66 | SupHutan | +45 | +30 | +15 | Sanity +5 | 2 hari |
| 67 | JamuHerbal | — | — | +35 (5 s) | Sanity +10 | tidak |
| 68 | JamuEnergi | — | — | — | Stamina penuh, regen ×1,5 60 s, Rest +10 | tidak |
| 69 | MakananGosong | +5 | — | — | Sanity −1 | tidak |
| 70 | DagingBusuk | +6 | — | — | 70% keracunan, Sanity −5 | — |
- Minum langsung: sungai/sumur tahan tombol "MINUM AIR" = +12/s Haus; laut = −6 per teguk + subtitle "(Asin! Jangan minum air laut.)". Isi botol: "ISI BOTOL" di air tawar atau Penampung Hujan.
### 4.3 Senjata & Alat (100–129) — statistik tempur di 06 §2
| ID | Enum | Nama tampil | Maks | Sumber |
|---|---|---|---|---|
| 100 | GolokPusaka | Golok Pusaka | 1 | Koper K1 |
| 101 | KapakBatu | Kapak Batu | 1 | Resep R03 |
| 102 | TombakBambu | Tombak Bambu | 3 | Resep R02 |
| 103 | BusurBambu | Busur Bambu | 1 | Resep R05 |
| 104 | AnakPanah | Anak Panah | 30 | Resep R06 |
| 105 | PanahApi | Panah Api | 15 | Resep R07 |
| 106 | Ketapel | Ketapel | 1 | Resep R13 |
| 107 | PistolSuar | Pistol Suar | 1 | Bangkai perahu |
| 108 | PelurSuar | Peluru Suar | 8 | Perahu, bunker |
| 109 | BedilLocok | Bedil Locok | 1 | Desa R3 |
| 110 | PeluruLocok | Peluru Locok | 12 | Desa R3, Resep R15 |
| 111 | KerisPusaka | Keris Pusaka | 1 | UG3 K4 (lanjutan) |
| 112 | GolokApiPusaka | Golok Api Pusaka | 1 | Ending B |
| 113 | Cangkul | Cangkul | 1 | Desa R1 |
| 114 | ZiplineGun | Peluncur Tali | 1 | UG2 R3 |
| 115 | TaliZipline | Gulungan Tali Luncur | 10 | UG2 R3 |
| 116 | BomBambuMinyak | Bom Bambu Minyak | 5 | Resep R14 |
| 117 | KapakBaja | Kapak Baja | 1 | UG2 R3 |
### 4.4 Peralatan (150–169)
| ID | Enum | Nama tampil | Fungsi |
|---|---|---|---|
| 150 | OborBambu | Obor Bambu | Tangan kiri; cahaya 7 m; 600 s nyala (hujan ×2 boros); nyalakan dengan KorekGas |
| 151 | SenterTaktis | Senter Taktis | Tangan kiri; sorot 25 m; 1 Baterai = 600 s; tahan angin |
| 152 | GPSHandset | GPS | Tangan kiri; mengaktifkan radar + penanda; tanpa GPS radar hanya menampilkan kompas |
| 153 | BukuPanduan | Buku Panduan | Membuka blueprint (tombol buku) |
| 154 | BukuCatatan | Buku Catatan | Perintah Kang Deden, Jurnal lore, daftar resep |
| 155 | KorekGas | Korek Gas | Menyalakan api/obor; tak terbatas |
| 156 | AlatSelamRebreather | Alat Selam | Napas bawah air 240 s (isi ulang otomatis di permukaan) |
| 157 | KartuAksesBunker | Kartu Akses Bunker | Membuka pintu D1 |
| 158 | PetaKertas | Peta Survei | Layar peta penuh |
### 4.5 Zirah (180–189) — lihat §3.7
### 4.6 Kunci Cerita (190–199)
190 `PakuKeramat1`, 191 `PakuKeramat2`, 192 `PakuKeramat3` — tidak bisa dibuang, ikon paku emas berpendar.

## 5. Masak, Api, & Bahan Bakar
- **Api Unggun**: bahan bakar maks 600 s. Tambah: Ranting +45 s, Daun +10 s, Kayu Gelondong (lempar ke api) +300 s, Arang +90 s. Hujan tanpa atap: konsumsi ×2; Badai tanpa atap: padam 60 s. Cahaya titik `#FFB347` intensitas 2,2 radius 9 m, berkedip noise 8 Hz ±12%.
- **Masak di api**: "MASAK" menaruh 1 item mentah di tusuk (maks 4). Matang 25 s, gosong setelah 40 s. Model berubah warna (mentah merah muda → coklat → hitam).
- **Jemuran Ikan** (blueprint B06): 6 slot, 120 s → versi Kering.
- **Panci Masak** (B10, di atas api): masukkan 1 `BotolAirBersih` + 2–4 bahan dari {Daging, Ikan, Jamur, Pisang, Jahe, Telur} → 40 s → 2 `SupHutan`.
- **Bakar Kemenyan**: Sanity +20, asap wangi putih 30 s, Pocong menjauh 20 m selama 60 s.

## 6. Tikar Pandan 3D & Crafting
- Kapasitas ditentukan tabel "Maks" per item (bukan slot).
- Tombol tas → tikar pandan terhampar (lihat 09 §5): item dari inventaris tampil sebagai model 3D mini tersusun di grid sesuai kategori; jumlah ditulis di label kecil.
- Seret item ke **Lingkaran Rakit** di tengah tikar. Bila kombinasi cocok resep: muncul hantu hasil + tombol **"RAKIT"**. Jika resep butuh alat (Golok), cek kepemilikan.
- Resep yang pernah dibuat tercatat di Buku Catatan halaman "Resep".

### 6.1 Sepuluh Resep Inti (`RecipeID` 1–10)
| ID | Hasil | Bahan | Alat |
|---|---|---|---|
| 1 R01 | TaliIjuk ×1 | SeratIjuk ×3 | — |
| 2 R02 | TombakBambu ×1 | Bambu ×1 + TaliIjuk ×1 | Golok |
| 3 R03 | KapakBatu ×1 | Ranting ×1 + Batu ×1 + TaliIjuk ×1 | — |
| 4 R04 | OborBambu ×1 | Bambu ×1 + Kain ×1 + Damar ×1 | — |
| 5 R05 | BusurBambu ×1 | Bambu ×2 + TaliIjuk ×2 | Golok |
| 6 R06 | AnakPanah ×4 | Ranting ×2 + Bulu ×2 + Batu ×1 | Golok |
| 7 R07 | PanahApi ×2 | AnakPanah ×2 + Kain ×1 + Damar ×1 | — |
| 8 R08 | ZirahDaun ×1 | DaunPisang ×5 | — |
| 9 R09 | ZirahBambu ×1 | Bambu ×2 + TaliIjuk ×1 | Golok |
| 10 R10 | ZirahTulang ×1 | Tulang ×4 + TaliIjuk ×1 | — |
### 6.2 Enam Resep Lanjutan (`RecipeID` 11–16)
| ID | Hasil | Bahan | Alat |
|---|---|---|---|
| 11 R11 | JamuHerbal ×1 | DaunSirih ×1 + KunyitObat ×1 | — |
| 12 R12 | JamuEnergi ×1 | JaheHutan ×1 + KelapaMuda ×1 | Golok |
| 13 R13 | Ketapel ×1 | Ranting ×1 + KulitHewan ×1 + TaliIjuk ×1 | — |
| 14 R14 | BomBambuMinyak ×1 | Bambu ×1 + MinyakTanah ×1 + Kain ×1 | — |
| 15 R15 | PeluruLocok ×3 | LogamBekas ×1 + MesiuHitam ×1 | — |
| 16 R16 | ZirahBaja ×1 | LogamBekas ×4 + KulitHewan ×1 + TaliIjuk ×1 | — |

## 7. Foraging (simpul sumber daya)
| Simpul | Hasil per panen | Tumbuh lagi |
|---|---|---|
| Semak | Ranting 1–2, Daun 1 (25%) | 1 hari |
| Pohon pisang (tebang 1×) | DaunPisang 3, Pisang 1–2 | 3 hari (tumbuh tunas) |
| Pohon aren (pukul batang) | SeratIjuk 2 (maks 3× per pohon/hari) | 1 hari |
| Mahoni (pukul batang) | Damar 1 (maks 2× per pohon/hari) | 1 hari |
| Sulur sirih di batang | DaunSirih 2 | 2 hari |
| Kunyit / Jahe (umbi) | 1 | 3 hari |
| Jamur di kayu lapuk | Jamur/JamurBeracun 1 (beracun berwarna merah bertotol) | 2 hari |
| Kelapa (pohon kelapa ditebang atau dilempar batu) | KelapaMuda 2–3 | 3 hari |
| Sarang ayam hutan | TelurAyamHutan 2 | 2 hari |
| Batu kecil | Batu 1 | tidak |

## 8. Tidur & Simpan
- **Kasur Saung**: "TIDUR" (hanya bila tidak ada musuh dalam 30 m) → layar pudar, waktu dipercepat ×40 sampai pilihan bangun (06:00 / 4 jam / sampai Rest penuh). Simpan otomatis sebelum tidur.
- **Tenda Darurat**: "SIMPAN" kapan saja aman; "ISTIRAHAT" +1,5/s.
- Simpan otomatis tambahan: setiap masuk/keluar gua, sebelum boss, sebelum ritual.

## 9. Kesulitan
| Pengali | Damai | Normal | Sulit |
|---|---|---|---|
| Laju lapar/haus/rest | 0,7 | 1,0 | 1,25 |
| Damage diterima | 0,5 | 1,0 | 1,4 |
| Musuh | Tidak ada Sekte/hantu (cerita tetap; boss diganti teka-teki akar) | normal | +1 musuh per kelompok, agresi naik ×1,5 |
| Companion gugur permanen | tidak | ya (dengan peringatan "Kang Deden kritis!") | ya |
| Tertangkap sebelum mati | 2× | 2× | 1× |
