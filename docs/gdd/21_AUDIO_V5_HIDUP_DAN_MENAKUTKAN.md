# 21 — AUDIO V5: HUTAN YANG HIDUP, SUARA YANG MENAKUTKAN
> Mengalahkan 10 dan 13 §5 bila bertentangan. Tujuan: pemain dengan headset merasa benar-benar berada di hutan Jawa malam hari, dan bisa **mendengar bahaya sebelum melihatnya**.

## 1. Filosofi
1. **Hutan tidak pernah sunyi — kecuali saat ada yang salah.** Jangkrik, tonggeret, katak, burung malam terus berbunyi. Ketika makhluk mendekat, serangga di radius 40 m **berhenti bersamaan** (gelombang hening menyebar dari arah makhluk). Hening itu adalah tanda bahaya utama.
2. **Suara jujur**: setiap serangan musuh punya "aba-aba" suara 0,4–0,8 s sebelum kena, terdengar dari arah yang benar. Takut karena tahu, bukan karena curang.
3. **Kontras**: siang terang dan ramai; malam sempit, dekat, berbisik. Volume total malam lebih kecil tapi detailnya lebih dekat ke telinga.
4. **Lapisan nyata + prosedural**: rekaman CC0 memberi tekstur alam yang tidak bisa disintesis; prosedural memberi variasi tanpa batas dan suara gaib yang tidak ada di dunia nyata.

## 2. Sumber Suara (mengubah "100% prosedural" di AGENTS H1 untuk audio)
- **CC0 saja**, diunduh `Tools/ambil_aset.py` dan dikunci di `Tools/audio_cc0.lock.json` (url, path, sha256, sumber, lisensi, pencipta). Tidak dikomit ke repo.
  - Freesound: hanya hasil berfilter lisensi **Creative Commons 0**; unduh via tautan pratinjau HQ (OGG/MP3) atau file asli bila secret `FREESOUND_TOKEN` tersedia.
  - OpenGameArt (filter CC0) dan Kenney (semua CC0).
- Domain internet yang perlu ditambahkan ke allowlist CI/Codex: `freesound.org`, `cdn.freesound.org`, `opengameart.org`, `kenney.nl`.
- `docs/KREDIT_AUDIO.md` dibuat otomatis dari lock file (walau CC0 tidak mewajibkan).
- Bila unduhan gagal: Builder memakai resep prosedural pengganti (10, 13 §5.3, §6 di bawah) — game tidak boleh bisu.
- Pengolahan dipanggang di Editor (bukan saat main): pitch, filter, reverb, potong, normalisasi loudness, variasi.

## 3. Daftar Slot Rekaman CC0 (± 260 file setelah variasi)
| Kelompok | Slot (jumlah variasi) | Kata kunci pencarian |
|---|---|---|
| Ambien siang | HutanTropisSiang (4 × 90 s), BurungSiang (12 one-shot), TonggeretSenja (3 × 60 s), AnginBambu (3), SungaiDangkal (3), SungaiDeras (2), AirTerjun (2), Ombak (3), RawaSiang (2) | rainforest day, cicada, bamboo creak, stream, waterfall, waves, swamp |
| Ambien malam | HutanMalam (4 × 120 s), Jangkrik (4), KatakRawa (3), BurungHantu (6 one-shot), TokekBunyi (4), Kelelawar (3), AnjingJauh (4) | night forest, crickets, frogs, owl, gecko, bats, distant dog |
| Cuaca | GerimisDaun (2), HujanLebat (3), HujanAtapSeng (2), HujanAtapDaun (2), Guntur dekat/jauh (8), AnginKencang (3) | rain on leaves, heavy rain, rain tin roof, thunder |
| Foley langkah | Tanah, DaunKering, Rumput, Lumpur, Pasir, Air dangkal, Kayu, Beton, BatuGua, Seng — masing-masing jalan/lari/jongkok × 8 variasi | footsteps leaves, mud, gravel, wood floor |
| Foley tubuh | Kain bergesek (6), Ransel (4), Napas berat/lelah/takut (10), Jantung (2), Menelan/makan (6), Batuk (4) | cloth rustle, backpack, breathing scared |
| Kayu & alat | Kapak ke kayu (10), Golok ke semak (8), Ranting patah (12), Pohon retak/tumbang (4), Kayu jatuh/tumpuk (8), Paku & palu (6) | axe chop, machete, branch snap, tree fall |
| Api & air | Api unggun kecil/besar (3), Obor (2), Celup air/berenang/bawah air (10), Tetes gua (6) | campfire, torch, underwater, cave drips |
| Tempur | Tebasan & hantaman daging (12), Tulang patah (6), Busur/anak panah (8), Senjata api tua (6), Peluru melintas (4) | flesh impact, bone break, bow, old rifle |
| Bangunan & benda | Pintu kayu berderit (6), Seng berderak (4), Rantai (4), Mesin tua/generator (3), Radio statis (3), Lori besi di rel (3) | door creak, chain, generator, radio static, rail cart |
| Gamelan rekaman | Gong, kenong, kempul, saron, bonang, kendang (multi-sampel per nada) | gamelan gong, saron, bonang |
| Bahan vokal mentah | Bisikan (8), Tawa perempuan (4), Tangis (4), Erangan (6), Napas di dekat mikrofon (4) | whisper, female laugh, crying, moan |

Bahan vokal **tidak pernah dipakai mentah**: selalu diolah (§6) menjadi suara makhluk.

## 4. Mesin Audio v5 (`ProceduralAudioSynth` + file partial baru)
### 4.1 Pool & prioritas
48 suara (32 3D, 16 2D). Prioritas: aba-aba serangan & suara makhluk dekat (5) > UI (4) > tempur (3) > foley pemain (2) > ambien one-shot (1) > bed ambien (0). Batas per jenis (mis. maks 6 langkah musuh sekaligus).

### 4.2 Spasial "HRTF-lite" (tanpa plugin, sesuai H1)
Untuk 8 sumber terdekat yang ditandai `Intim` (makhluk, bisikan, napas, langkah di belakang) bila mode Headset aktif: komponen `AKBinauralFilter` (`OnAudioFilterRead`, spatialBlend 0 lalu dipanning manual):
- ITD: tunda telinga jauh 0–0,66 ms sesuai azimut (interpolasi pecahan sampel).
- ILD/bayangan kepala: LP satu-kutub telinga jauh 1,5–8 kHz sesuai azimut.
- Depan/belakang: sumber di belakang → notch 4–6 kHz + LP 9 kHz kedua telinga (terasa "di belakang kepala").
- Ketinggian: puncak 7–8 kHz naik bila sumber di atas (Kuntilanak di pucuk pohon).
- Jarak sangat dekat (< 1 m): efek proksimitas (bass naik 3 dB, dry penuh, tanpa reverb) — bisikan "di telinga".
Sumber lain memakai panning 3D Unity biasa. Mode Speaker HP: binaural dimatikan, diganti panning stereo lebar + kompresi agar jelas di speaker kecil.

### 4.3 Akustik dunia
- **Oklusi & obstruksi**: 4×/detik raycast pendengar→sumber; tembok/tebing = LP 900 Hz −10 dB; vegetasi lebat = LP 3 kHz −3 dB; terrain = LP 600 Hz −14 dB.
- **Gema lingkungan**: 6 raycast horizontal dari pendengar tiap 0,5 s mengukur "keterbukaan"; hutan rapat → reverb pendek lembap; lapangan → kering; dekat tebing (Z6, Z10) → slapback delay = 2 × jarak ke tebing / 343 m/s.
- `AudioReverbZone` per area: gua (Cave), bunker (Hallway), terowongan lori (Sewer pipe), sumur (Stone corridor), dalam rumah kayu (Room), pabrik (Hangar), bawah air (Underwater + LP 500 Hz pada listener).
- **Bawah air**: semua di atas air diredam LP 400 Hz −18 dB, gelembung napas, detak jantung lebih keras.

### 4.4 Snapshot campuran (tanpa AudioMixer aset; pengali volume & filter lewat kode, transisi 0,5–3 s)
Tenang · Meremang (ambien −3 dB, serangga mulai menipis) · Mencekam (ambien −9 dB, dengung takut naik, napas Andi terdengar) · Lega (ambien kembali, burung pagi) · Tempur (musik perkusi, ambien −12 dB) · Gua · Dalam Saung (hujan teredam) · Bawah Air · Menu (LP 800 Hz).
**Ducking**: aba-aba serangan & dialog menurunkan semua kategori lain 6 dB selama 0,6 s.

### 4.5 Loudness & format
Target campuran −16 LUFS terintegrasi, puncak sejati −1 dBTP. SFX pahlawan 48 kHz; ambien 44,1 kHz stereo. Impor: SFX pendek ADPCM Decompress On Load; ambien & musik Vorbis kualitas 0,7 **Streaming**; one-shot ambien Vorbis 0,6 Compressed In Memory. Perkiraan audio di APK: 180–260 MB.

## 5. Ambien Hidup (`AmbienceDirector`, file baru)
- **3 lapis**: (1) *bed* loop panjang per biome × waktu (pagi/siang/senja/malam/dini hari) dicampur silang 4 s; (2) *emitter titik* tersebar di dunia (±1.500 titik ditanam Builder: sarang burung, kolam katak, pohon tonggeret, rumpun bambu berderit) yang memutar one-shot acak 3D; (3) *cuaca* di atas semuanya.
- **Jadwal alam** (jam game): 04:30 ayam hutan & kedasih · 05:30–07:00 paduan burung pagi · siang serangga panas · 17:30 tonggeret memuncak · 18:00 Maghrib: semua burung diam, kentongan jauh, angin · 19:00–04:00 jangkrik, katak, tokek, burung hantu · 02:00–03:00 "jam sepi": ambien tipis, kejadian teror lebih sering.
- **Gelombang hening**: setiap makhluk/hantu memancarkan radius hening 25–40 m; emitter di dalamnya berhenti dengan jeda acak 0,1–0,6 s berurutan dari dekat makhluk (pemain mendengar diam menjalar ke arahnya). Hening kembali 6–12 s setelah makhluk pergi.
- **Reaksi**: burung terbang ribut (kepakan + teriak) saat pohon tumbang, tembakan, atau Sekte lari di dekatnya — petunjuk posisi musuh.
- **Zona khusus**: Z12 Alas Wingit — bed ambien tidak alami (serangga berbunyi dalam pola berulang yang "salah", gamelan samar dari tanah); Z9 pabrik — besi berderit ditiup angin, rantai bergoyang, mesin hantu menyala sendiri jam 00:00; Z11 kampung — radio kosong di balai desa memutar statis dan potongan lagu keroncong prosedural.

## 6. Suara Makhluk (desain per musuh)
Setiap makhluk punya set: *diam/idle* · *curiga* · *melihat* · *mengejar* · *aba-aba serang* · *serang* · *sakit* · *mati* · *jauh* (versi LP + reverb untuk > 60 m). Diolah dari bahan vokal CC0 + prosedural:
| Makhluk | Tanda khas |
|---|---|
| Abdi Sekte | Gumam mantra tak bermakna (formant 13 §5.3 diacak), napas di balik topeng (LP + resonansi kayu 400 Hz), teriakan kode bersahutan antar-anggota |
| Kuntilanak | Tawa: jauh = dekat, dekat = jauh (aturan folklor, 13 §4.1); bau kamboja diberi tanda suara "angin manis" (pad tinggi lembut) sebelum muncul; versi pucuk pohon pakai ketinggian HRTF |
| Pocong | Debam lompatan + gesek kain kafan; saat dekat: napas tersumbat kain; tali kafan berderit |
| Genderuwo | Langkah sangat berat (sub 35 Hz, getaran ponsel), geraman formant rendah dengan pitch −12 semitone dari bahan erangan, ketukan di dinding saung |
| Kuyang | Tetes basah, kepak rambut, organ berdenyut (detak lembek 50 bpm), terbang di atas kepala (HRTF ketinggian) |
| Sundel Bolong (Z11) | Senandung lirih lagu tidur (melodi pelog 5 nada, prosedural) yang berhenti mendadak saat pemain menoleh |
| Mandor Pabrik (Z9) | Sepatu bot di lantai besi, cambuk, siulan kolonial, suara mesin pabrik hidup saat ia aktif |
| Penunggu Sumur (UG5) | Gema suara Andi sendiri yang terlambat 1,5 s (pemain mendengar langkah/napasnya diulang) |
| Penyerupa | Meniru suara Deden/Ningsih memanggil nama Andi dari arah yang salah (formant 13 §5.3, intonasi datar) |
| Hewan | Babi hutan (dengus), rusa (pekik alarm), buaya (desis + cebur), kelelawar (cicit + kepakan massal) |

## 7. Musik Adaptif (melengkapi 10 §5)
- **Stem**: drone gong rendah, saron/bonang pola pelog, kendang tempur, gesekan rebab sintetis, paduan suara "sinden" tanpa kata (vokal diolah, bukan doa/bacaan agama — H13).
- Tiap fase teror memilih stem; transisi pada ketukan (dspTime). Malam tanpa bahaya = musik diam, hanya alam.
- **Leitmotif**: Ningsih (melodi slendro lembut), Ki Rogo (gong terbalik + nada turun), Alas Wingit (gamelan disetel salah sepersekian nada).
- Stinger kejutan 6 varian; **dilarang** memakai stinger untuk kejadian kecil (agar kejutan besar tetap efektif).

## 8. Andi Bersuara
Napas sesuai stamina & takut (tenang/terengah/menahan napas saat jongkok dekat musuh — bisa ditahan dengan tombol 5 s), detak jantung adaptif 60–160 bpm, desah sakit, batuk saat asap, gigi gemeletuk saat basah & dingin, perut keroncongan saat lapar. **Bisikan pikiran** Andi saat Kewarasan rendah (bisikan acak yang tidak jelas, terdengar dari dalam kepala — mono tengah, sangat dekat).

## 9. Uji Dengar (Gerbang G5)
APK uji punya menu Debug → **Uji Dengar**: memutar urutan 12 situasi (pagi di Z1, Maghrib, malam di Z12, hujan di saung, makhluk berputar 360° di sekitar kepala, langkah di belakang, gua, bawah air, pabrik, kuntilanak di pohon, gelombang hening, serangan Sekte). Pengguna memakai headset dan menulis kesan di `docs/UMPAN_BALIK.md`. Lulus bila pengguna menilai ≥ 8/10 "terasa nyata & menakutkan" dan arah suara benar pada uji 360°.

## 10. Pembagian ke Tahap
T1: pool 48, kategori, loudness, penulis WAV stereo, struktur `AudioBank`. T2: `AmbienceDirector`, slot ambien & cuaca CC0, emitter titik, jadwal alam, akustik dunia. T3: foley langkah per permukaan, napas & jantung Andi, mode Headset/Speaker, `AKBinauralFilter`. T5: suara kayu/alat/hewan CC0. T7: suara makhluk §6, aba-aba serangan, gelombang hening. T9: snapshot teror, bisikan pikiran, Uji Dengar. T10: musik adaptif penuh & leitmotif, reverb gua/bunker/pabrik. T11: mixing akhir & loudness.
