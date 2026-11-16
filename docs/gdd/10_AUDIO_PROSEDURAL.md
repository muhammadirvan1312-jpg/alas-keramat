# 10 — AUDIO PROSEDURAL: BLOK DSP, RUMUS SEMUA EFEK, AMBIEN, MUSIK GAMELAN, MIXING 3D
Dibaca pada tahap T1 (inti synth), T2 (ambien & cuaca), T5 (kayu), T7 (tempur & hantu), T10 (musik cerita).
Aturan: game tidak boleh bisu. Tidak ada suara agama (adzan, sholawat, bacaan kitab).

---

## 1. Strategi: Dipanggang di Editor, Cadangan di Runtime
1. `ProceduralAudioSynth` menghasilkan sampel (`float[]`) dengan rumus di dokumen ini.
2. **Builder** memanggil generator yang sama di Editor, menulis **WAV 16-bit** (penulis RIFF sederhana) ke `Assets/Generated/Audio/` lalu mengatur impor: SFX pendek = ADPCM + Decompress On Load; loop ambien & musik = Vorbis kualitas 0,5 + Compressed In Memory. Hemat memori & tanpa jeda pembuatan di HP.
3. Runtime memakai klip yang dipasang Builder. Jika klip hilang → buat via `AudioClip.Create(name, sampleCount, 1, sampleRate, false)` + `SetData` (cadangan aman) sambil log peringatan.
4. Laju sampel: SFX 44.100 Hz; loop ambien 22.050 Hz.
5. Semua variasi memakai RNG deterministik (xorshift32, seed per nama klip + nomor variasi).

## 2. Blok DSP (`Dsp`, statis)
| Blok | Rumus / catatan |
|---|---|
| Osilator | sinus, segitiga, gigi gergaji, kotak; akumulator fase `φ += 2πf/sr` |
| Sapuan frekuensi eksponensial | `f(t) = f0 × (f1/f0)^(t/T)` |
| Noise putih | xorshift32 → [−1,1] |
| Noise pink | filter Paul Kellet (7 koefisien) |
| Noise coklat | `b = 0,98b + 0,02w`, dinormalisasi |
| Filter satu-kutub LP/HP | `a = exp(−2πfc/sr)` |
| Biquad LP/HP/BP/Notch/Peak | resep RBJ Audio EQ Cookbook |
| Amplop | AD, ADSR, `exp(−t/τ)` |
| FM | `sin(2π fc t + I sin(2π fm t))` |
| AM | `x × (1 − d + d × (0,5 + 0,5 sin 2π fa t))` |
| Karplus-Strong | buffer noise panjang `sr/f`, rata-rata 2 sampel × 0,996 (petikan senar busur, tali) |
| Delay/gema | buffer + feedback |
| Reverb ringan Schroeder | 4 comb (1116, 1188, 1277, 1356 sampel @44,1k) + 2 allpass (556, 441) — hanya untuk varian "jauh"; reverb ruang memakai `AudioReverbZone` |
| Formant | 2 BP paralel (F1, F2) pada sumber gigi gergaji — untuk geraman tak bermakna |
| Akhiran | fade-in/out 5 ms; normalisasi puncak 0,89 (−1 dBFS); loop: crossfade 0,25 s ekor ke awal agar mulus |

## 3. Efek Suara (nama method / klip → resep)
### 3.1 Kayu & Bangunan
| Klip | Durasi | Resep |
|---|---|---|
| `PlayWoodChop()` | 0,18 s | Nada 140 → 55 Hz eksponensial, amplop `exp(−t/0,05)` + noise putih 0,02 s pertama lewat BP 1.800 Hz Q 1,2 + klik 2 ms. 3 variasi (pitch ±8%) |
| `PlayTreeCrackAndFall()` | ≈ 3,6 s | 6 retakan (impuls noise HP 1.200 Hz, τ 0,012, pitch acak) setiap 0,25 s, lalu hantaman: sinus 45 Hz + noise pink LP 300 Hz selama 1,8 s (serang 0,03, τ 0,6) + desir daun (noise BP 4 kHz, 1,2 s, AM acak) |
| Ambil kayu | 0,15 s | Debam 90 Hz τ 0,05 + gesek noise BP 1 kHz |
| Jatuhkan kayu | 0,35 s | Ketuk 70 Hz 0,2 s + pantulan 0,15 s (−8 dB, 60 Hz) |
| Pasang kayu (snap) | 0,12 s | Ketukan kayu 220 Hz + parsial 440 Hz τ 0,04 |
| Belah papan | 0,4 s | Retak + derit (gergaji 180 → 120 Hz bergetar, BP 600) |
| Serut runcing | 3 × 0,08 s | Gesek noise BP 3 kHz |
| Gali tanah | 0,25 s | Noise LP 500 + kres (impuls BP 2 kHz) |
| Bangunan selesai | 1,2 s | Akor bonang (§5) + 2 ketukan kayu |
### 3.2 Tempur
| Klip | Resep |
|---|---|
| Ayunan (whoosh) | Noise BP sapuan 600 → 2.000 Hz, 0,22 s, amplop lonceng |
| Kena daging | Debam 110 Hz 0,08 s + noise LP 800 0,06 s |
| Kena kafan | Debam lembut 90 Hz + noise HP 2 kHz "fwump" 0,1 s |
| Kena kayu / logam | Kayu: 300 Hz τ 0,05; logam: parsial 520, 1.430, 2.780 Hz τ 0,4 |
| Parry (dentang) | Parsial 880, 2.350, 3.910 Hz τ 0,6 + ledakan noise 10 ms |
| Busur tarik / lepas | Derit gergaji 90 Hz bergetar LP; petikan Karplus-Strong 196 Hz 0,4 s |
| Panah meluncur / menancap | Noise BP 3 kHz 0,15 s / ketuk 250 Hz 0,05 s |
| Pistol suar | Letup (noise 0,05 s) + desis noise HP 3 kHz meluruh 2 s |
| Bedil locok | Dentum noise LP 400 τ 0,5 + debam 60 Hz + gema delay 0,35 s fb 0,3 |
| Api menyala (obor) | Noise BP naik 300 → 1.500 Hz 0,4 s |
| Api unggun (loop 6 s) | Noise pink LP 1.500 + letupan Poisson λ 12/s (impuls HP 2 kHz) |
| Bom bambu | Pecah bambu (ketukan 600 Hz) + wuss api (noise BP 800 naik) |
| Larut jadi abu | Noise HP 1 kHz memudar 2 s + wuss rendah 120 Hz |
### 3.3 Pemain & Lingkungan Dekat
| Klip | Resep |
|---|---|
| Langkah Tanah (6 var) | Noise LP 600 τ 0,07 + debam 80 Hz |
| Langkah Daun Kering | Noise BP 3,5 kHz + kresek (impuls acak) 0,12 s |
| Langkah Pasir | Noise BP 1,5 kHz lembut 0,1 s |
| Langkah Air | Noise BP 900 + gelembung (sinus kicau 400–900 Hz acak) 0,25 s |
| Langkah Kayu (lantai) | Ketuk 180 Hz 0,06 s + noise |
| Langkah Beton/Batu Gua | Klik HP 2 kHz 0,04 s (ekor dari reverb zone) |
| Napas berat (loop) | Noise BP 700 Hz, tarik 0,6 s / hembus 0,8 s |
| Detak jantung | 2 debam 55 Hz (0,09 s) jarak 0,18 s, siklus 0,9 s |
| Terkena (tanpa vokal) | Debam 100 Hz + hembus napas noise BP 900 0,2 s |
| Makan / minum | Kres (3 ledakan noise BP 2 kHz) / teguk (sinus 300 → 180 Hz + gelembung) |
| Renang | Kecipak noise BP 700 0,3 s bergantian |
| Meluncur zipline (loop) | Noise BP 2,5 kHz; pitch sumber = 0,8 + kecepatan/20 |
| Tali ditembak | Letup + petikan Karplus-Strong 110 Hz panjang 1,2 s |
### 3.4 UI
Klik (sinus 1.200 Hz 30 ms) · Ambil item (segitiga 660 → 880 Hz, 2 × 80 ms) · Rakit berhasil (akor bonang) · Balik halaman (noise HP 2 kHz amplop menyapu 0,35 s) · Coretan buku (noise BP 3 kHz AM 25 Hz 0,5 s) · Misi diperbarui (gong kecil kenong + denting 1.760 Hz) · Lore ditemukan (dengung rendah 110 Hz 1,5 s + kertas).
### 3.5 Musuh & Hantu
| Klip | Resep |
|---|---|
| `PlayKentonganBamboo()` | Resonansi tabung: sinus 380 Hz + harmonik ke-2 760 Hz (×0,5), peluruhan 0,22 s + klik kayu 5 ms. Pola **"Tok-Tok… Tok-Tok"**: pukulan di 0; 0,16; 0,90; 1,06 s. Variasi nada dasar 340–420 Hz per pengintai. Diputar 3D dari posisi pengintai Sekte |
| `PlayKuntilanakLaugh()` | FM: pembawa 620 Hz (meluncur turun ke 520 Hz) dimodulasi sinus 6 Hz deviasi ±120 Hz, AM "hi-hi" 6 Hz, 2,5 s, gema delay 0,15 s feedback 0,35 mix 0,4, + noise napas BP 2,5 kHz 20%. 3D dari pucuk beringin |
| `PlayPocongHopThud()` | Dentum sub-bass 48 Hz 0,25 s `exp(−t/0,08)` + bisikan desis noise BP 2–4 kHz 0,3 s AM 9 Hz |
| Jeritan Kuntilanak | FM pembawa 900 → 1.600 → 700 Hz (1,2 s), modulator 37 Hz indeks 4 + noise; soft-clip |
| Erangan Pocong (diam) | Noise BP 250 Hz AM 0,7 Hz, 2 s |
| Auman Genderuwo | Gergaji 70 → 55 Hz + sub 35 Hz + noise LP 600 lewat formant (400/900 Hz) vibrato 5 Hz, 2 s |
| Hantaman tanah Genderuwo | Debam 40 Hz 0,6 s + puing (noise LP 1,5 kHz bergemeretak) |
| Banaspati (loop) | Api unggun loop dipitch ×1,4 + wuss menerjang (noise BP sapuan) |
| Jenglot | Klik cepat 30/s BP 3 kHz 0,6 s |
| Tongkat Ki Rogo | Dengung 55 Hz + kilau 1,1 kHz tremolo 7 Hz + hentakan 50 Hz |
| Gertakan Sekte | Formant "aah" (F1 700, F2 1.200) pada gergaji 140 → 110 Hz + noise, 0,6 s (tak bermakna) |
| Sakit Sekte | Formant sama 0,25 s pitch naik 15% |

## 4. Ambien (loop 12–20 s @22.050 Hz, maks 4 lapis dicampur silang 3 s)
| Lapisan | Kapan | Resep |
|---|---|---|
| Hutan siang | 06:00–17:30, bioma hutan | Tonggeret: noise BP 4,5 kHz AM 45 Hz semburan 2–4 s membengkak; burung: kicau FM 2–5 kHz (80–200 ms), 3 pola "spesies" (nada berulang cepat, dua nada "tu-wu", trilil menurun), 0,5–2 per detik; angin pink LP 400 modulasi lambat |
| Derit bambu | Bioma bambu | Gergaji 60–90 Hz bergetar → BP 400 Q 8 amplop tak teratur ("krieet") + ketukan batang 600–900 Hz sesekali |
| Sungai | ≤ 40 m dari sungai | Noise pink BP 600 & 2 kHz + gelembung kicau sinus acak |
| Air terjun | ≤ 80 m | Noise pink LP 3 kHz stabil |
| Ombak laut | Pantai | Noise coklat LP 500 dengan amplop ombak 7 s + desis buih HP 2 kHz di puncak |
| Hujan ringan / lebat / di bawah atap | Cuaca | Noise putih HP 1 kHz + tetes acak (BP 3–6 kHz); lebat + gemuruh LP; di bawah atap: ketukan BP 1,5 kHz seperti genderang |
| Guruh (bukan loop) | Badai | Retak (noise HP 1 kHz 0,1 s) + gemuruh coklat LP 200 3–5 s AM acak; jarak → tunda & LP |
| Malam hutan | 19:00–05:00 | Jangkrik: sinus 4,2 kHz 3 pulsa @30 ms per kicau, 1,5 kicau/s, 3 lapis detune; **tokek** ("tok" 350 Hz pendek + "keh" 220 Hz lebih panjang + noise) tiap 20–60 s; kodok: gergaji 140 Hz AM 25 Hz 0,3 s dekat air; burung hantu: sinus 380 Hz "huu" 0,6 s meluncur sedikit |
| Gua | UG1 & gua | Tetesan "plink" (sinus 1,2–2,5 kHz turun cepat 40 ms) acak + dengung 48 & 50,5 Hz (berdenyut) + desir LP 200 |
| Bunker | UG2 | Dengung listrik 50 Hz + harmonik 100/150 + dengung neon (kotak 100 Hz pelan) sinkron kedipan lampu + derit logam jauh |
| Kuil Akar | UG3 | Denyut 40 Hz tiap 1,4 s + derit akar + bisikan noise |
| Sawit | Bioma sawit | Letupan bara + angin rendah + dengung lalat (gergaji 180 Hz vibrato) dekat rawa |
| Maghrib (stinger) | 18:00 | Ambien diredam ke 30% dalam 3 s, kentongan dari 3 arah, gong jauh sekali, dengung rendah naik |
| Malam Darah (dasar) | Malam Darah | Dengung disonan (tumpukan interval pelog) + detak lambat |

## 5. Musik Gamelan Prosedural
### 5.1 Instrumen (catatan nada dipanggang per pitch)
| Instrumen | Model |
|---|---|
| Saron / Bonang / Gender | Parsial tak harmonis f × [1; 2,76; 5,40; 8,93], amplitudo [1; 0,5; 0,25; 0,12], τ [1,2; 0,6; 0,3; 0,15] s + noise pemukul 3 ms |
| Gong ageng | f 45–60 Hz, parsial [1; 1,47; 2,09; 2,56], τ 6–8 s, pasangan detune +0,6 Hz untuk "ombak" |
| Kenong / ketuk | Gong menengah 220–440 Hz, τ 1,5 s |
| Kendang sintetis | "Dhung": sinus 80 → 60 Hz 0,15 s; "Tak": noise tamparan BP 1,5 kHz 0,05 s |
| Suling | Sinus + noise napas, vibrato 5 Hz ±2% |
| Senandung Ningsih | Sinus lembut lewat LP 900 Hz (vokal "mm"), vibrato 4,5 Hz, mengikuti melodi slendro |
### 5.2 Tangga Nada (perkiraan, Hz)
- **Slendro**: 270,0 · 310,5 · 356,4 · 409,6 · 470,9 · 540,0
- **Pelog**: 270,0 · 289,0 · 321,0 · 370,0 · 395,0 · 420,0 · 466,0 · 540,0
### 5.3 Mode Musik (penjadwal real-time memakai `AudioSettings.dspTime` + `PlayScheduled`, pool 8 sumber 2D)
| Mode | Kapan | Isi |
|---|---|---|
| Tenang | Siang di kemah, sesekali (tiap 5–8 menit selama 1–2 menit) | Slendro 60 bpm, balungan saron 8 nada dari tabel pola, gong tiap 16 ketuk, volume 0,25 |
| Tegang | Dekat Sekte/gua | Dengung pelog rendah + bonang jarang (jeda acak 2–5 s) |
| Tempur | Intensitas tempur > 0,3 | Kendang 120 bpm + ostinato saron cepat (pelog); masuk/keluar 2 s |
| Boss | Kubah Akar | Tempur + gong tiap 8 ketuk + tremolo disonan |
| Ending | Ending A/B | Melodi slendro pelan (gender) + gong, nuansa lega |

## 5b. Tambahan v3
Audio horor (jeda sunyi, dengung takut, binaural dipanggang, suara sintetis baru): `13` §5. Audio gore: `14` §9. Audio dunia hidup (paduan subuh, embusan angin berjalan, satwa, kampung Sekte): `16` §8.

## 6. Mixing & 3D
- Kategori: Master, Efek, Ambien, Musik, UI — pengali volume dari Pengaturan (tanpa aset AudioMixer).
- Pool `AudioSource` **24 suara** (16 3D, 8 2D). Prioritas: UI tertinggi, tempur, lingkungan, ambien terendah; curi suara paling pelan/tertua berprioritas rendah.
- 3D: spatialBlend 1, rolloff Logarithmic min 2 m max 60 m (kentongan 120 m), doppler 0.
- **Oklusi murah**: 4×/detik per sumber aktif, raycast pendengar→sumber; terhalang terrain/struktur → `AudioLowPassFilter` meluncur ke 1.200 Hz.
- Reverb: `AudioReverbZone` — UG1 preset Cave, UG2 Hallway, UG3 Arena; di dalam Saung beratap: Room kecil.
- Menu terbuka: `AudioLowPassFilter` pada listener 800 Hz + volume 0,6.
- Anggaran: total klip dipanggang ≤ 25 MB di disk; memori audio ≤ 40 MB.
