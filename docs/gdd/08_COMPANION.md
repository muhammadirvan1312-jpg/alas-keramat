# 08 — COMPANION: KANG DEDEN (BUKU CATATAN 15 PERINTAH) & NINGSIH (TRUST, HADIAH, SENJATA GANDA, INDRA KEENAM)
Dibaca pada tahap T8. Visual & klip animasi di `02_KARAKTER.md` §3–4.

---

## 1. Aturan Umum Companion
- Lokomosi: `CharacterController` + `GridPathfinder` (06/11). Jalan 1,5 m/s, lari 5,0 m/s (Ningsih 1,7 / 7,5). Bisa berenang di sungai.
- **Ikuti**: posisi target 3–5 m di belakang-kiri (Deden) / belakang-kanan (Ningsih) Andi. Jika > 80 m dan tidak terlihat kamera selama 3 s → teleport ke titik aman 25 m di belakang Andi (tidak pernah di depan kamera).
- **Gua**: Kang Deden selalu menunggu di mulut gua (subtitle sekali: "(Kang Deden menunjuk ke mulut gua lalu duduk. Dia akan menunggu di sini.)"). Ningsih hanya ikut ke UG3 bila Trust ≥ 70 dan sedang Ikuti.
- **Kritis**: HP 0 → berlutut/berbaring "KRITIS" 45 s; Andi bisa "BERI JAMU" (JamuHerbal atau DaunSirih) untuk memulihkan ke HP 40. Lewat 45 s → **gugur permanen** (Normal/Sulit) dengan subtitle & penanda batu kecil di lokasi; Damai: hanya pingsan 30 s.
- Ikon radar: lingkaran hijau berhuruf **K** (Deden) & **N** (Ningsih); bar HP kecil muncul di bawah radar saat HP < 100%.
- Simpan: posisi, HP, perintah aktif, Trust, energi, status gugur.

## 2. KANG DEDEN (`KangDedenKelvinAI`)
### 2.1 Statistik
HP 150 (pulih 1/s di luar tempur), Golok 12 damage / 1,1 s, damage tebang 18, energi kerja 0–100 (−0,05/s saat bekerja; < 20 → duduk di api `DudukBersila`+`HangatkanTangan`, +0,5/s sampai 80).
### 2.2 Cara Memberi Perintah
1. Andi ≤ 6 m dari Deden & Deden terlihat → tombol tangan "PERINTAH KANG DEDEN".
2. Layar **Buku Catatan** terbuka (Andi memegang buku menghadap Deden; lihat 09 §6).
3. Ketuk perintah → Deden `BacaCatatan` (1,4 s) → bisa dilakukan: `SenyumMengangguk2x` + `JempolKanan` lalu eksekusi; tidak bisa: `GelengBahu` + subtitle alasan.
### 2.3 Lima Belas Perintah (`CompanionOrder`, integer final)
| ID | Halaman | Teks di buku | Perilaku | Syarat | Alasan menolak |
|---|---|---|---|---|---|
| 0 | Umum | Ikuti Saya | Mengikuti; membantu bertarung | — | — |
| 1 | Umum | Tunggu di Sini | Diam di titik itu, berjaga | — | — |
| 2 | Umum | Istirahat | Duduk di api/saung sampai energi 100 | — | — |
| 13 | Umum | Pulang ke Kemah | Ke kemah, bersantai/berjaga | Kemah ada | "Belum ada kemah." |
| 3 | Kumpulkan | Ambil Kayu | Tebang pohon ≤ 40 m dari kemah (≥ 8 m dari struktur, pilih yang terdekat & aman arah rebahnya), panggul 2 gelondong → Rak Kayu (bila ada ruang) → cetak biru yang butuh kayu → tumpuk di kemah. Ulangi | Kemah ada | "Belum ada kemah." |
| 4 | Kumpulkan | Ambil Ranting | Kumpulkan ranting dari semak ≤ 40 m → setor ke cetak biru / tumpukan ranting di kemah | Kemah | sama |
| 5 | Kumpulkan | Ambil Batu | Sama untuk batu | Kemah | sama |
| 10 | Kumpulkan | Cari Makanan | Petik/jatuhkan kelapa & tangkap ikan (tombak) ≤ 60 m → serahkan ke Andi bila dekat ("Kang Deden memberimu 2 Kelapa Muda") atau taruh di dekat api | — | — |
| 11 | Kumpulkan | Isi Air | Ambil air sungai/sumur → isi Penampung Hujan (+5 porsi per perjalanan) | Penampung ada | "Belum ada Penampung Hujan." |
| 6 | Kemah | Isi Rak Kayu | Kumpulkan gelondong tergeletak ≤ 50 m → Rak | Rak ada & belum penuh | "Rak penuh / belum ada Rak." |
| 7 | Kemah | Selesaikan Bangunan | Isi cetak biru terdekat dengan bahan dari Rak/tumpukan; bila kurang, kumpulkan sendiri | Ada cetak biru | "Tidak ada cetak biru." |
| 8 | Kemah | Jaga Api | Jaga bahan bakar api unggun kemah > 50% (ranting/kayu ≤ 30 m); prioritas malam | Api ada | "Belum ada api unggun." |
| 9 | Kemah | Bersihkan Kemah | Pungut kayu/ranting/batu tercecer ≤ 25 m dari kemah → Rak/tumpukan | Kemah | sama |
| 12 | Kemah | Perbaiki Bangunan | Perbaiki potongan rusak (+50 HP per kayu dari Rak) | Ada struktur rusak | "Semua bangunan utuh." |
| 14 | Kemah | Bangun Pagar Keliling | Pagar runcing melingkar r 12 m + celah gerbang dari bahan di Rak (17 §7) | Kemah & Rak berisi | "Bahan di Rak tidak cukup." |

### 2.4 Jadwal Pembukaan Perintah
- Setelah M02: Ikuti, Tunggu, Istirahat.
- Setelah M04: Ambil Kayu, Ambil Ranting, Ambil Batu, Isi Rak Kayu, Jaga Api, Pulang ke Kemah.
- Setelah M05: semua.
### 2.5 Perilaku Otomatis
(v3: keseharian, patung untuk Dewi, dan 20 Catatan Kang Deden di `16_DUNIA_HIDUP.md` §6.1.)
- **Rutinitas malam**: 19:00 tanpa perintah aktif (atau perintah kerja) → pulang, duduk di api; 22:00 tidur di Saung jika ada; 06:00 bangun, menguap, peregangan.
- **Takut hantu**: hantu ≤ 20 m → `PanikMenunjuk` ke arah hantu (peringatan berguna!), lalu `BerlindungDiBelakang` Andi atau lari masuk Saung bila ≤ 30 m. Rasa takut 0–100, turun 10/s bila hantu > 30 m.
- **Sekte menyerang**: Takut < 50 → bertarung golok; ≥ 50 → mundur ke Andi. Saat siege: selalu bertarung.
- **Terkena Andi**: 3 pukulan sengaja dalam 10 s → Deden "Kecewa": menjauh dan menolak perintah 60 s.
- **Reaksi**: jempol saat Andi menyelesaikan bangunan dalam 15 m; tepuk bahu (animasi tangan) saat Andi pulang ke kemah dengan HP < 30; dengan Ningsih Trust ≥ 70 → kadang duduk berbagi makanan dengannya (idle bersama).

## 3. NINGSIH (`NingsihVirginiaAI`)
### 3.1 Statistik
HP 110, kebal takut hantu, takut Sekte bila tidak bersenjata. Busur 30 damage tiap 1,4 s, akurasi 80% pada 20 m. Pistol Suar 40 + bakar, jeda 6 s, dipakai bila musuh ≤ 25 m (prioritas hantu karena api ×2). Amunisi tak terbatas.
### 3.2 Trust 0–100 — Tiga Tahap
**Tahap 1 — Mengintip & Waspada (0–30)**
- Siang (06:00–17:30) setiap 4–7 menit nyata, muncul 30–45 m dari Andi di balik pohon (`Mengintip`).
- +2/s bila Andi: senjata disimpan (holster), jongkok, diam, dan menatap ke arahnya dalam 40 m (maks +15 per perjumpaan).
- Kabur bila: Andi berlari ke arahnya (−5/s dalam 25 m), mengacungkan/membidik senjata (−15/s), atau mendekat < 12 m.
- Malam: kembali ke **Sarang Ningsih** (100,−175).
**Tahap 2 — Mendekat & Mengamati (31–69)**
- Muncul lebih sering (2–4 menit) dan berani mendekat sampai 4 m; duduk timpuh di dekat api unggun Andi bila menyala.
- **+8/s** bila Andi holster + jongkok tenang saat ia mendekat (maks +20 per perjumpaan).
- "BERI MAKANAN" (KelapaMuda / IkanMatang / Pisang / DaunSirih): +10 (jeda 2 jam game).
- Bersenandung pelan; pagi 06:00–08:00 **TariPagi** di dekat api/kemah (Sanity Andi +0,3/s dalam 10 m).
**Tahap 3 — Sahabat Setia & Pengawal Mematikan (70–100)**
- Tinggal di kemah siang hari; malam tidur di Saung (atau timpuh di api).
- **Membawa hadiah** sekali per hari game (06:00–10:00): acak dari `DaunSirih` ×2, `KelapaMuda` ×1, `IkanMentah` ×2 (segar), `Kemenyan` ×1. Ia mendekat dengan `MemberiHadiah`; "TERIMA HADIAH" (+2 Trust).
- Menu interaksi **"BERINTERAKSI DENGAN NINGSIH"**: [Berikan Busur Bambu] [Berikan Pistol Suar] [Ikuti Saya] [Tunggu di Sini] [Pulang ke Kemah] [Beri Makanan]. Kedua senjata bisa diberikan **sekaligus**; item pindah dari inventaris Andi ke Ningsih (terlihat di tangan/punggungnya).
- Tempur: lompat lincah ke samping (`LompatSamping`, jeda 2 s) bila musuh ≤ 3 m; menembak busur pada jarak > 6 m; suar bila musuh ≤ 25 m. Tidak pernah melee.
### 3.3 Perubahan Trust Lain
| Kejadian | Trust |
|---|---|
| Andi membunuh Sekte di depannya | +5 |
| Andi tidur di Saung saat ia di kemah | +3 |
| Andi memukulnya | −40, kabur 1 hari game |
| Andi menyerang Sekte yang menyerah di depannya | −5 |
| Tidak bertemu seharian | −2 per hari |
| Lore #8 dibaca lalu Andi duduk di api saat ia datang | +10 (sekali) |
### 3.4 Indra Keenam
- Intensitas segel = lerp(0,6 → 2,5) saat hantu terdekat mendekat dari 40 m → 0 m (petunjuk visual untuk pemain).
- Hantu ≤ 40 m yang belum terlihat Andi → `MerasakanGaib` lalu menunjuk ke arahnya (jeda 20 s).
- Trust ≥ 70: **Jalan Rahasia** — radar menampilkan penanda tersembunyi (mulut gua yang belum ditemukan, Ruang Emas K4 saat di UG3); ia memimpin jalan bila diperintah Ikuti di dekat lokasi tersebut.
- Di UG3 K2: cutscene pendek (Ningsih berlutut di depan Wasiat ayahnya; subtitle: *"(Ningsih menyentuh kain kuning itu lama sekali. Untuk pertama kalinya, ia menangis.)"*), Trust +10.

## 4. Interaksi Deden ↔ Ningsih
- Trust < 31: Deden kaget (alis naik) saat Ningsih muncul, menunjuk ke Andi.
- Trust ≥ 70: keduanya kadang duduk berdampingan di api; Deden memberi jempol saat Ningsih menari.
- Siege: keduanya bertahan di dekat altar dalam 10 m dari Andi.
