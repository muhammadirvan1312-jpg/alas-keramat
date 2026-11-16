# 13 — HOROR & TEROR: SUTRADARA TEROR, 37 KEJADIAN SERAM, FOLKLOR SEBAGAI MEKANIK, AUDIO & VISUAL HOROR
Dibaca pada tahap T9 (Teror & Dunia Hidup); dipakai juga di T7 (hantu) dan T10 (gua & cerita).
Tujuan: pemain merinding karena **menunggu** sesuatu — bukan hanya kaget. Rasio target: 70% teror pelan (dread), 25% kejutan sedang, 5% jump scare keras.

---

## 1. Prinsip Takut
1. **Yang tidak terlihat lebih menakutkan.** Suara, siluet, bekas, dan jejak muncul jauh sebelum makhluknya.
2. **Ketidakpastian.** Pemain tidak pernah 100% yakin: nyata atau halusinasi Sanity? (Halusinasi tidak pernah melukai; hantu nyata selalu meninggalkan jejak fisik, mis. jejak kaki basah atau kain kafan robek.)
3. **Rasa aman yang dilanggar.** Kemah bukan tempat aman mutlak: batu di atap, ketukan dinding, pintu terbuka sendiri, Kang Deden hilang dari kasur.
4. **Folklor yang orang Indonesia kenal dijadikan aturan main.** Pemain yang tahu mitosnya mendapat keuntungan (tawa Kuntilanak terbalik, tali Pocong, Kuyang dan duri).
5. **Ritme, bukan spam.** Teror dipandu Sutradara Teror (§2) dengan anggaran kejutan, agar takut tidak berubah jadi kebal.
6. **Gelap sungguhan.** Malam tanpa cahaya = jarak pandang 8–12 m. Cahaya adalah sumber daya.

## 2. Sutradara Teror (`TerrorDirector`)
### 2.1 Ketegangan (Tension) 0–100 — dihitung 2×/detik
```
T = 15
  + 20 × malam                    (19:00–05:00)
  + 15 × gelap                    (tanpa cahaya dalam 6 m)
  + 25 × bawahTanah
  + 20 × malamDarah
  + 0,25 × (100 − Sanity)
  + ancaman                       (0–30; musuh terdekat: 30 bila ≤ 10 m, turun linear sampai 0 di 60 m)
  − 20 × diApiKemahBersamaTeman   (api menyala ≤ 8 m dan companion ≤ 10 m)
  − 10 × siangCerah
dibatasi 0–100, dihaluskan (lerp 0,5/detik)
```
### 2.2 Empat Fase (siklus berulang)
| Fase | Durasi | Perilaku |
|---|---|---|
| **TENANG** | ≥ 120 s | Tanpa kejadian; ambien normal; musik tenang boleh |
| **MEREMANG** | 90–240 s | Kejadian Ringan tiap 45–120 s; ambien serangga perlahan dikurangi 40%; dengung rendah masuk |
| **MENCEKAM** | 20–60 s | 1 kejadian Sedang ATAU Keras; "jeda sunyi" 3–5 s sebelumnya (ambien → 0); detak jantung; FOV berdenyut ±1,5° |
| **LEGA** | 30–60 s | Ambien kembali, napas lega Andi, subtitle monolog kadang |
Masuk MEREMANG bila T > 40 selama 20 s; MENCEKAM bila T > 65 dan anggaran tersedia; kembali TENANG setelah LEGA.
### 2.3 Anggaran & Aturan
- Kejutan **Keras**: maks 1 per 8 menit nyata (Ekstrem 5 menit; Ringan 0). Kejutan **Sedang**: maks 1 per 3 menit.
- Kejadian sama tidak berulang dalam 2 hari game (kecuali ditandai ↻).
- Tidak ada kejadian baru selama tempur aktif, cutscene, menu, 60 s setelah memuat/bangun dari pingsan, dan 20 s setelah kejadian lain.
- Kejadian dipilih acak berbobot dari yang syaratnya terpenuhi; bobot ×2 untuk yang belum pernah dialami.
- **Intensitas Horor** (Pengaturan): **Ringan** (tanpa kejutan Keras, stinger lebih pelan), **Normal**, **Ekstrem** (anggaran ×1,6, halusinasi mulai Sanity < 70).

## 3. Katalog 37 Kejadian
Tipe: R = Ringan, S = Sedang, K = Keras. "Bukti" = jejak fisik nyata yang tertinggal (membedakan dari halusinasi).
| ID | Nama | Syarat | Efek | Tipe |
|---|---|---|---|---|
| E01 | Kentongan Jauh ↻ | Malam | "Tok-Tok… Tok-Tok" 3D 100–140 m, dibalas dari arah lain | R |
| E02 | Langkah Bayangan | Malam, berjalan sendirian, Sanity < 70 | Langkah di belakang menyamai langkah Andi, berhenti 0,4 s SETELAH Andi berhenti; menoleh → kosong | R |
| E03 | Bisikan Telinga | Malam/gua, Headset ON | Bisikan binaural "Andiii…" di telinga kiri atau kanan (§5.2) | R |
| E04 | Tangis Bayi di Hutan | Malam, Kuntilanak aktif ≤ 120 m | Tangis sayup dari pepohonan; 30 s kemudian Kuntilanak mendekat | R |
| E05 | Tawa Terbalik ↻ | Kuntilanak ≤ 80 m | Lihat §4.1 | R/S |
| E06 | Kang Deden Memanggil | Deden hidup & tidur/di kemah, Andi > 60 m, malam | Suara sintetis "An… di…" dari arah hutan berlawanan dengan kemah; subtitle *"(Itu suara Kang Deden... tapi Kang Deden kan bisu.)"*; bila didatangi: kain kafan tergantung di dahan (bukti) | S |
| E07 | Gamelan Tanpa Penabuh | 01:00–03:00, ≤ 250 m dari Desa | Gamelan slendro sayup dari Desa; didatangi → gamelan kayu tua di balai desa, berhenti saat dilihat | R |
| E08 | Batu di Atap | Andi di dalam Saung/tidur, malam | Kerikil berjatuhan di atap 6–10×; tidur terbangun; pagi: kerikil berserakan (bukti) | S |
| E09 | Tiga Ketukan | Andi di dalam struktur | 3 ketukan di dinding luar; dibuka → kosong; pagi: jejak kaki besar di sekeliling (bukti) | S |
| E10 | Napas di Belakang | Jongkok di semak, gelap | Napas berat binaural tepat di belakang 4 s | S |
| E11 | Ayam Berkokok Tengah Malam | 23:00–02:00 | Kokok ayam hutan dari kejauhan → 60 s kemudian Pocong muncul | R |
| E12 | Kedasih Berkicau | Senja/malam | Kicau "pi-pi-pi-piii" sendu (pertanda maut dalam cerita Jawa) → hantu muncul dalam 2 menit | R |
| E13 | Siluet di Batas Cahaya ↻ | Memegang cahaya, gelap | Sosok berdiri tepat di tepi radius cahaya; hilang setelah ditatap 1 s | R |
| E14 | Pocong di Kilat | Hujan badai | Pocong berdiri di jalan setapak 30–50 m, HANYA terlihat saat kilat (2–3 kilat, makin dekat), lalu hilang | S |
| E15 | Tamu di Api Unggun | Sekali per permainan, Hari ≥ 5, Deden tidur di Saung | "Deden" duduk di api unggun. Didekati 4 m → kepalanya berputar pelan 180°, mata hitam, mulut terbuka tanpa suara; api padam; ia hilang | K |
| E16 | Wajah di Celah | Malam, menatap celah dinding/lantai saung | Wajah pucat Kuntilanak di celah 0,8 s | S |
| E17 | Rambut Menjuntai | Malam, berjalan di bawah pohon besar | Helai rambut hitam masuk dari atas layar; menengadah → Kuntilanak terbalik di dahan, tertawa, menghilang | K |
| E18 | Makin Dekat | Pocong terlihat ≥ 30 m | Setiap kali kamera berpaling > 60°, Pocong maju 6 m tanpa bergerak saat dilihat (3×), lalu hilang | S |
| E19 | Kafan di Jemuran | Jemuran ada, malam | Kain kafan tergantung di jemuran; didekati → kain bergerak seperti ada tubuh di dalamnya, jatuh kosong (kain tetap ada = bukti, bisa diambil) | R |
| E20 | Jejak Kaki Basah | Pagi setelah malam berhantu | Jejak telanjang kaki basah melintasi kemah, masuk ke saung, berhenti di kasur | R |
| E21 | Tumpukan Batu | Pagi, Agresi ≥ 30 | 3 tumpukan batu kecil di tepi kemah (tanda Sekte mengawasi) | R |
| E22 | Boneka Ranting | Pagi, Agresi ≥ 50 | Boneka ranting & kain merah tergantung di pintu masuk kemah | R |
| E23 | Bunga Kamboja ↻ | Kuntilanak ≤ 30 m | Kelopak putih melayang + subtitle *"(Bau bunga kamboja... di tengah hutan?)"* | R |
| E24 | Cahaya Sekarat ↻ | Hantu ≤ 20 m | Obor menyusut / senter berkedip 2 s | R |
| E25 | Radar Gaib | Membawa GPS, malam | Titik "?" mendekat dari belakang di radar lalu hilang | S |
| E26 | Pantulan | Sekali per permainan, BERCERMIN malam | Pantulan wajah Andi terlambat 0,3 s, lalu Pocong terlihat di belakangnya dalam pantulan; menoleh → kosong | K |
| E27 | Di Antara Kedipan | Sanity < 35 | Andi berkedip (layar hitam 0,12 s); di kedipan berikutnya sosok sudah ada di depan, kedipan berikut hilang | S |
| E28 | Pintu Terbuka Sendiri | Pintu Bambu tertutup, malam | Pintu berderit terbuka perlahan | R |
| E29 | Kasur Kosong | 02:00–04:00, Deden tidur | Kasur Deden kosong; ia berdiri diam menghadap hutan di tepi kemah (tidur berjalan karena kutukan); disentuh → terkejut, normal | S |
| E30 | Tatapan Ningsih | Ningsih di kemah | Ningsih menatap satu titik gelap, segel menyala maksimal, 20 s kemudian hantu datang dari arah itu | R |
| E31 | Prosesi Obor | Senja Hari ≥ 2, ≥ 120 m | Barisan 8 Sekte bertopeng membawa obor berjalan diam melintasi hutan + gamelan sayup; tidak menyerang kecuali diganggu | S |
| E32 | Ki Rogo Mengawasi | Senja Hari 4 & 6 | Siluet Ki Rogo di punggung bukit 80 m, mata merah; hilang saat didekati | S |
| E33 | Mimpi Buruk | Tidur dengan Sanity < 40 | Mimpi 10 s: kursi heli, penumpang sebelah adalah Pocong yang menoleh; bangun tersentak (getar detak) | K |
| E34 | Tangan dari Air | Berenang di rawa/sungai malam | Pergelangan kaki ditarik 0,5 m ke bawah, stamina −20; suara gelembung | K |
| E35 | Jeritan dari Gua | Malam, ≤ 40 m mulut gua | Jeritan panjang bergema dari dalam gua | R |
| E36 | Lampu Bunker Padam | Skrip UG2 C2 | Semua lampu mati 4 s; menyala → mutan berdiri di ujung koridor; padam lagi → hilang | K |
| E37 | Langit-langit Bergerak | UG3 K1, senter menyala | Sorot senter memperlihatkan Jenglot merayap di langit-langit | S |

## 4. Folklor Menjadi Mekanik
### 4.1 Kuntilanak — tawa terbalik & kamboja
- **Tawa terbalik** (mitos: tawa terdengar jauh = ia dekat; terdengar dekat = ia jauh): jarak ≤ 15 m → versi tawa pelan berbisik volume 0,25 (dengan napas); 15–40 m → volume 0,5; > 40 m → tawa melengking keras volume 1,0. Diputar 2D-terpanning (bukan atenuasi 3D) agar pemain tidak bisa menghitung jarak dari keras-pelannya.
- Tanda kedatangan: tangis bayi (E04) → kelopak kamboja (E23) → tawa.
- **Paku Besi Kuning** (item baru ID 118, resep R17: LogamBekas 1 + Kemenyan 1 + tempa di Api Unggun 20 s): saat Kuntilanak goyah (disorot senter 2 s atau di-parry), prompt **"TANCAPKAN PAKU"** (tahan 0,8 s) di tengkuknya → ia terikat dan lenyap permanen dari pohon itu (kembali hanya saat Malam Darah). Statistik "Hantu Diikat".
### 4.2 Pocong — tali kafan
- Saat Pocong roboh (parry/api), prompt **"LEPAS TALI POCONG"** (tahan 1,2 s; ia meronta & menjerit) → arwah dilepas: Pocong runtuh menjadi kain kosong, Sanity +8, drop Kemenyan. Alternatif selain menghancurkannya; dihitung "Arwah Dilepas" (memengaruhi teks Ending).
- Di turunan > 15° menuju Andi, Pocong **berguling** (6 m/s, berputar di sumbu panjang) — lebih cepat dari lompatan.
### 4.3 Genderuwo Hutan (varian non-boss, 2,2 m, HP 400)
Muncul hanya Malam Darah di luar kemah ≥ 25 m: melempar kerikil ke atap (E08), mengetuk (E09), mengguncang saung (kamera bergoyang di dalam), menghilang bila disorot api ≤ 6 m. Jejak kaki besar 40 cm (bukti).
### 4.4 Kuyang (musuh baru, ID 13)
- Kepala perempuan melayang dengan **isi perut menjuntai** (organ & usus berkilau basah, menetes), mata menyala, rambut panjang. Terbang 5 m/s, 3–8 m di atas tanah, malam di Desa & Sawit. HP 90.
- **Tertarik darah**: bila Andi berstatus Pendarahan (14 §4) radius deteksi 120 m (normal 50 m). Menyerang dengan gigitan menukik (14 + Pendarahan).
- **Kelemahan folklor**: isi perutnya tersangkut duri → menembus Pagar Runcing/semak berduri = terjebak 4 s (damage ×2). Tubuh tanpa kepalanya tersembunyi di salah satu rumah Desa siang hari (acak R2/R4/R5, tertutup kain): "BAKAR TUBUH" (perlu api/korek) → Kuyang mati saat fajar dan tidak muncul lagi. Kuyang meninggalkan tetesan darah di tanah (jejak).
### 4.5 Tuyul (gangguan non-tempur, ID 14)
Makhluk kecil gundul yang terkikik, mencuri 1–3 item kecil (Baterai, PelurSuar, Kemenyan, Damar) dari Rak/kemah pada malam hari bila tidak ada orang ≤ 15 m. Tidak bisa dilukai (kabur bila disorot). Penangkal folklor: **Mangkuk Kepiting** (blueprint B15: Batu 2 + Kepiting dari pantai) di kemah → Tuyul asyik bermain kepiting dan tidak mencuri; pagi hari jejak kaki kecil di sekitar mangkuk.
### 4.6 Makhluk Penyerupa (E15)
Meniru companion. Tanda pembeda bagi pemain jeli: tidak berbayangan & tidak bernapas (tidak ada animasi napas), api di dekatnya tidak berkedip.

## 5. Audio Horor (tambahan untuk 10)
### 5.1 Lapisan
- **Jeda sunyi**: sebelum kejutan S/K, ambien & musik turun ke 0 dalam 1,5 s, tahan 3–5 s (hanya napas Andi).
- **Dengung takut**: 32–42 Hz + harmonik 2 (agar terdengar di speaker HP) dimodulasi amplitudo pada laju detak jantung saat Tension > 60.
- **Stinger**: klaster gamelan disonan (3 nada pelog berdekatan dipukul bersamaan) + sapuan simbal terbalik (noise HP 3 kHz amplop naik 1,2 s lalu putus).
- **Detak jantung adaptif**: 70 → 140 bpm mengikuti Tension & HP.
### 5.2 Binaural Dipanggang (khusus Headset)
Klip stereo dipanggang di Editor: sumber mono (bisikan, napas, langkah) → telinga jauh ditunda **0,6 ms** (ITD) + LP 2,5 kHz −6 dB (ILD/bayangan kepala); varian "belakang": kedua telinga LP 4 kHz + dekorelasi kecil. Diputar 2D (spatialBlend 0) sesuai arah sumber relatif kamera (kiri/kanan/belakang). Layar awal menanyakan "Pakai headset?" (bisa diubah di Pengaturan).
### 5.3 Suara Sintetis Baru
| Klip | Resep |
|---|---|
| "An… di…" (penyerupa) | Sumber gigi gergaji 110 Hz jitter 3% (versi bisik: noise): /a/ F1 800 F2 1.200 (0,35 s) → /n/ F1 250 F2 2.000 amp 0,3 (0,12 s) → /d/ ledakan noise 15 ms → /i/ F1 300 F2 2.300 (0,5 s, meluruh); reverb hutan; diulang 2× berjeda 2 s |
| Tangis bayi jauh | FM 450 Hz (modulator 7 Hz indeks 3) dengan formant "wa" (F1 900 → 600) siklus 0,8 s; LP 2 kHz + reverb (terdengar jauh) |
| Kedasih | 4 siulan sinus naik 2,2 → 2,8 kHz, tiap 0,25 s, nada terakhir panjang 0,6 s |
| Kokok ayam | Gergaji 600 Hz sapuan 600 → 900 → 500 Hz, 1,4 s, formant |
| Kerikil di atap | 6–10 ketukan acak 1,2–2,5 kHz τ 0,03 + gulir kerikil (impuls rapat menurun) |
| Ketukan dinding | 3 debam kayu 140 Hz τ 0,08, jarak 0,45 s |
| Pocong meronta | Gesekan kain (noise BP 1,5 kHz AM 8 Hz) + jeritan pendek FM |
| Kuyang | Tetesan basah (sinus plink 300–600 Hz) + kepakan rambut (noise BP 800 AM 12 Hz) + desis |
| Tuyul terkikik | FM pendek 1,1 kHz AM 9 Hz, 0,5 s |

## 6. Visual & Kamera Horor
- **Kegelapan malam**: cahaya bulan mengikuti **fase bulan** (siklus 8 hari): purnama 0,18 → bulan mati 0,03. Tanpa cahaya, jarak pandang 8–12 m. **Kalibrasi kecerahan** di awal: geser sampai simbol kiri hampir tak terlihat (mengatur exposure/Lift).
- **Penampakan tepi**: sosok hanya di-spawn 25–40° dari tengah layar; hilang bila masuk 15° dari tengah atau ditatap 1 s.
- **Kamera tegang** (Tension > 70): FOV berdenyut ±1,5° mengikuti detak, vignette mengetat 0,22 → 0,4, chromatic aberration 0,25, film grain naik (Tinggi), tangan FP gemetar (noise rotasi ±0,6°).
- **Kilat**: beberapa hal hanya terlihat saat kilat (E14) — implementasi: renderer diaktifkan hanya pada frame kilat (0,12 s).
- **BERCERMIN**: kamera kedua merender wajah Andi ke RenderTexture 256×256 yang ditampilkan di permukaan air (hanya saat interaksi; dipakai juga E26).

## 7. Getar (Android)
Pola via `AndroidJavaObject` (Vibrator `vibrate(long[] pola, -1)`), hanya `#if UNITY_ANDROID && !UNITY_EDITOR`, dengan try/catch; iOS: `Handheld.Vibrate()`.
| Pola | Nilai (ms) |
|---|---|
| Detak jantung | [0, 60, 120, 60, 700] diulang selama MENCEKAM |
| Kejutan keras | [0, 220] |
| Pohon tumbang dekat | [0, 80, 60, 140] |
| Kena damage berat | [0, 120] |

## 8. Set-Piece Terskrip per Babak
| Kapan | Set-piece |
|---|---|
| Malam 1, 21:00 | Kentongan mengepung dari 4 arah bergantian (E01 dipaksa), lalu siluet di batas cahaya api (E13) |
| Malam 2 | Raid uji Sekte (06 §7) + pagi harinya Boneka Ranting (E22) |
| Malam 3 (Malam Darah pertama) | Bulan merah, Genderuwo Hutan melempar batu ke saung, Pocong di kilat (bila badai) |
| Kunjungan malam pertama ke Desa | Gamelan tanpa penabuh + Sekte menari mengelilingi api ritual di bawah tubuh tergantung terbungkus kain (14 §6) |
| UG1 | Lore #4 dibaca → Pocong jatuh di belakang; di terowongan air: mayat anggota ekspedisi melayang perlahan melintas di depan masker |
| UG2 | E36 di koridor C2; ruang otopsi: tubuh di meja berkedut lalu bangkit (mutan Gembung, 17 §5) |
| UG3 | Lorong Angin memadamkan obor; dinding akar berisi wajah-wajah korban yang membuka mata saat disorot senter; bisikan "pulang… pulang…" |
| Hari 5 malam | E15 Tamu di Api Unggun (sekali) |
| Siege | Semua lapisan audio maksimal, langit kilat merah terus-menerus |
