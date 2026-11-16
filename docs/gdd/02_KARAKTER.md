# 02 — KARAKTER: ANATOMI PROSEDURAL, VISUAL, PSIKOLOGI & ANIMASI
Dibaca pada tahap T4 (model & animasi), T7 (musuh), T8 (companion), T10 (boss & cutscene).

---

## 1. Teknik Pembuatan Karakter (berlaku untuk semua humanoid)

### 1.1 Kerangka: 20 sendi tubuh + 4 sendi wajah + titik pasang
Catatan koreksi v1: daftar sendi di v1 ternyata berjumlah **20**, bukan 16.

| # | Sendi | Induk | Posisi model-space (m) untuk tinggi acuan 1,75 m (karakter menghadap +Z, kanan = +X) |
|---|---|---|---|
| 1 | `Pelvis` | Root | (0, 0.98, 0) |
| 2 | `Spine` | Pelvis | (0, 1.10, 0) |
| 3 | `Chest` | Spine | (0, 1.28, 0) |
| 4 | `Neck` | Chest | (0, 1.48, 0) |
| 5 | `Head` | Neck | (0, 1.58, 0.01) |
| 6 | `Jaw` | Head | (0, 1.60, 0.035) |
| 7–8 | `L/R_Clavicle` | Chest | (∓0.04, 1.44, 0) |
| 9–10 | `L/R_UpperArm` | Clavicle | (∓0.19, 1.43, 0) |
| 11–12 | `L/R_Forearm` | UpperArm | (∓0.21, 1.14, 0) |
| 13–14 | `L/R_Hand` | Forearm | (∓0.22, 0.88, 0.01) |
| 15–16 | `L/R_Thigh` | Pelvis | (∓0.10, 0.95, 0) |
| 17–18 | `L/R_Calf` | Thigh | (∓0.10, 0.52, 0.01) |
| 19–20 | `L/R_Foot` | Calf | (∓0.10, 0.09, 0) |
| W1–W2 | `L/R_Eye` | Head | (∓0.032, 1.676, 0.092) |
| W3–W4 | `L/R_Brow` | Head | (∓0.033, 1.705, 0.096) |

Titik pasang (Transform kosong, bukan sendi): `LeftShoulderMount` (anak `L_Clavicle`, offset (−0.05, 0.07, −0.02)) untuk 2 kayu; `RightHandGrip`, `LeftHandGrip` (anak Hand, offset (0, −0.08, 0.02)); `HeadCamMount` (anak Head, offset (0, 0.09, 0.09)); `BackMount` (anak Chest, offset (0, 0, −0.16)); `HipMount_R` (golok di pinggang).

Pose dasar: **I-pose** (lengan turun). Semua posisi lain diskalakan: `posisi × (tinggi/1.75)` untuk sumbu Y dan `× lebar` untuk X/Z.

### 1.2 Mesh: "Loft Ellipse" per bagian, digabung jadi SATU mesh rigid-skinned
- Setiap bagian tubuh dibuat dengan `ProcMesh.Loft(stations)` — deretan elips (radiusX, radiusZ) pada jarak di sepanjang tulang, 10 segmen keliling. Ujung ditutup (cap).
- Pakaian = loft kedua sedikit lebih besar (offset +0,006–0,015 m) dengan sel atlas pakaian; kulit di bawah pakaian tidak dibuat (hemat segitiga).
- Semua bagian digabung menjadi 1 `Mesh` dengan `boneWeights` rigid (tiap vertex bobot 1,0 ke satu sendi), `bindposes` dari kerangka, lalu dipasang ke 1 `SkinnedMeshRenderer` → **1 draw call per karakter**.
- Target segitiga: Andi/Deden/Ningsih 2.800–3.600; Sekte 1.800–2.400; Pocong 900; Kuntilanak 2.200 (rambut banyak); Genderuwo 3.200; hewan 600–1.200.
- LOD: > 35 m pakai mesh LOD1 (keliling 6 segmen, tanpa jari/aksesori kecil) — `LODGroup` dengan 2 `SkinnedMeshRenderer` berbagi tulang.

Contoh profil loft (radius X, radius Z dalam meter; acuan Andi):
| Bagian | Dari → ke | Stasiun (posisi relatif 0..1 : rX, rZ) |
|---|---|---|
| Pinggul | Pelvis | 0:(0.16,0.11) 0.5:(0.17,0.12) 1:(0.15,0.10) |
| Perut | Spine→Chest | 0:(0.14,0.10) 1:(0.15,0.10) |
| Dada | Chest→Neck | 0:(0.16,0.11) 0.6:(0.18,0.115) 1:(0.10,0.08) |
| Leher | Neck→Head | 0:(0.055,0.055) 1:(0.05,0.05) |
| Lengan atas | UpperArm→Forearm | 0:(0.055,0.055) 0.3:(0.05,0.052) 1:(0.04,0.04) |
| Lengan bawah | Forearm→Hand | 0:(0.042,0.042) 0.4:(0.04,0.038) 1:(0.028,0.024) |
| Paha | Thigh→Calf | 0:(0.085,0.085) 0.4:(0.08,0.08) 1:(0.055,0.055) |
| Betis | Calf→Foot | 0:(0.055,0.055) 0.35:(0.06,0.065) 1:(0.035,0.035) |
| Kaki | Foot→ujung (0,0,0.17) | kotak membulat 0.09×0.07×0.24 |
| Tangan | Hand | kepalan membulat 0.08×0.035×0.10 + ibu jari (silinder kecil) |

### 1.3 Kepala & wajah (detail yang membuat karakter "hidup")
- Tengkorak: UV-sphere 16×12 dideformasi menjadi elips (rX 0.078, rY 0.105, rZ 0.095), pusat = `Head` + (0, 0.085, 0.01).
- Deformasi vertex: dagu maju (+0.012 di z pada y < −0.06), pipi (+0.006 pada |x|>0.05, y≈−0.02), hidung = baji kecil (0.018 lebar, 0.03 tinggi, menonjol 0.022) di (0, −0.005, rZ).
- Mata: bola putih r 0.0125 (sel `PutihMata`) + cakram iris r 0.0065 (sel iris) menghadap +Z, anak `L/R_Eye`.
- Kedip: skala Y sendi Eye → 0.1 selama 0,12 detik, interval acak 2,5–6 detik.
- Alis: kotak tipis 0.03×0.006×0.008, anak `Brow`. Ekspresi: naik (+0.004 y) = kaget, miring dalam turun = waspada/marah, miring luar turun = sedih/lelah.
- Mulut: garis bibir = kotak pipih (0.034×0.005×0.006) sel `Bibir`; rahang (`Jaw`) membuka 0–35° untuk teriak/kaget.
- Rambut: kumpulan "kartu rambut" (quad melengkung 2 sisi, `SurfaceMode.Cutout`) atau gumpalan kerucut untuk rambut pendek.

### 1.4 Atlas warna karakter (1 tekstur 512×512 + 1 peta emisi 512×512)
Grid 8×8 sel @ 64 px, padding 3 px. UV setiap bagian dipetakan ke sel berikut. Sel bermotif digambar prosedural sesuai rumus di §1.5. Material karakter: metallic 0, smoothness 0,25 (satu nilai untuk seluruh atlas).

| Sel | Nama | Warna/motif |
|---|---|---|
| 0 | KulitSawoMatang (Andi) | `#C68E63` + noise halus |
| 1 | KulitSawoGelap (Deden) | `#A8734D` |
| 2 | KulitKuningLangsat (Ningsih) | `#D9A77E` |
| 3 | KulitPucatPocong | `#A9AE9C` + bercak gelap |
| 4 | KulitKuntilanak | `#D8D6CC` |
| 5 | KulitGenderuwo | `#3B2E26` + serat bulu |
| 6 | Bibir | `#8E4A44` |
| 7 | PutihMata | `#EDE8DD` |
| 8 | IrisCoklatTua | `#2B1A12` |
| 9 | RambutHitam | `#121010` + garis serat |
| 10 | AlisHitam | `#16110E` |
| 11 | TahiLalat | `#2A1712` |
| 12 | Gigi | `#E6DCC4` |
| 13 | DarahKering | `#5A1A14` |
| 14 | PerbanPutih | `#E8E4D8` + kisi kasa |
| 15 | SegelEmas | `#E8C766` (emisi) |
| 16 | WindbreakerZaitun | `#4B5A33` + kisi ripstop 4 px |
| 17 | WindbreakerSobek | `#3F4C2A` + tepi koyak (zigzag kulit) |
| 18 | KausHitam | `#1C1C1E` |
| 19 | SarungTanganTaktis | `#2A2A2A` |
| 20 | CelanaKargo | `#6B6248` + noda lumpur `#4A3B2A` di bawah |
| 21 | SepatuBoot | `#3A2C20` |
| 22 | JamKompas | `#1A1A1A` + titik `#9EA3A6` |
| 23 | RanselAndi | `#3C4A3A` |
| 24 | RompiHiVisOranye | `#D9661E` kusam |
| 25 | StripReflektif | `#C9CCCB` (smoothness tinggi) |
| 26 | FlanelKotak | kotak-kotak `#7A2E22`/`#3A2A22`/`#B9A58A` |
| 27 | IketBatikSunda | batik hitam-coklat motif lereng |
| 28 | CelanaKerjaTebal | `#3E3A33` |
| 29 | BotKaretKuning | `#D9B21E` |
| 30 | GagangGolok | `#4A3322` |
| 31 | RanselBambu | anyaman `#B39A5E` |
| 32 | KebayaPutihKrem | `#EDE3CC` + motif renda |
| 33 | JarikParangGelap | motif parang `#3B2A1C`/`#1A120C`/`#7A5A36` |
| 34 | BungaKantil | `#F6F3E6` |
| 35 | SelendangMerahTua | `#8C2F2F` |
| 36 | KainKuningBeringin | `#C9A22A` |
| 37 | KainPoleng | kotak hitam-putih 8 px |
| 38 | KainKafan | `#E9E6DA` + lipatan gelap |
| 39 | TaliKafan | `#D8D2C0` |
| 40 | TopengKayu | `#8A6A45` + ukiran garis |
| 41 | TopengMerah | `#7A1E14` + garis emas pudar |
| 42 | PakaianSekte | `#4A4136` sobek-sobek |
| 43 | SelendangMerahSekte | `#6E1612` |
| 44 | CatTubuhPutih | `#D9D4C6` garis-garis |
| 45 | Lumpur | `#4A3B2A` |
| 46 | IjukRambut | `#2A231C` |
| 47 | CapingAnyaman | `#B08A52` |
| 48 | JubahHitam | `#141012` |
| 49 | SurjanLurik | garis vertikal `#5A4A3A`/`#2E241C` |
| 50 | BlangkonHitam | `#0F0D0C` + batik samar |
| 51 | KalungTaring | `#E8DCC0` |
| 52 | BuluGenderuwo | `#2A211B` serat |
| 53 | MataMerahMenyala | `#FF2A1A` (emisi ×3) |
| 54 | GaunKuntilanak | `#E9E7E0` + tepi koyak |
| 55 | RambutKuntilanak | `#0D0C0C` |
| 56 | Logam | `#9EA3A6` (kesan logam dari warna terang + garis highlight di tekstur) |
| 57 | KayuGagang | `#6B4F36` |
| 58 | TaliIjuk | `#3A3026` |
| 59 | BambuHijau | `#9DB85A` |
| 60–63 | Cadangan | — |

Peta emisi: hitam semua kecuali sel 15 (SegelEmas), 53 (MataMerah), dan titik mata Pocong saat malam (dikendalikan `MaterialPropertyBlock` `_EmissionColor`).

### 1.5 Pola kain prosedural (dipakai juga untuk properti dunia)
- **Batik Parang**: putar UV 45°; `u' = frac(u*6)`, `v' = v*6`; garis S: `d = abs(u' - 0.5 - 0.22*sin(v'*2π))`; isi gelap jika d < 0.12, garis terang `#7A5A36` jika 0.12 ≤ d < 0.16, latar coklat tua.
- **Poleng**: `((floor(u*8)+floor(v*8)) mod 2)` hitam/putih, beri noise kusam 10%.
- **Lurik**: garis vertikal lebar acak deterministik (seed tetap).
- **Flanel**: dua set garis horizontal & vertikal semi-transparan ditumpuk (efek kotak tenun).
- **Anyaman (pandan/bambu/caping)**: `checker` diagonal 2 lapis dengan gradasi tepi tiap pita.

### 1.6 Kamera orang-pertama & kesadaran tubuh (khusus Andi)
Andi punya 3 renderer:
1. `Andi_Body` (skinned, tanpa kepala & lengan) — tampil normal (dilihat saat menunduk).
2. `Andi_HeadArms` (skinned, kepala + rambut + lengan) — `ShadowCastingMode.ShadowsOnly` saat mode FP; `On` saat cutscene/layar status.
3. `Andi_FPArms` (skinned, lengan dari bahu, anak kamera) — tampil hanya di FP, tanpa bayangan, layer `Player`, digambar dengan near clip 0,03.
Rotasi lengan FP disalin ke lengan `Andi_HeadArms` setiap frame (agar bayangan sesuai).
- Kamera di `HeadCamMount`. Saat pitch > 45° ke bawah, kamera bergeser maju +0,08 m dan turun −0,04 m agar tidak melihat ke dalam dada; dada, torso, kaki melangkah, dan 2 kayu di `LeftShoulderMount` terlihat.
- Pitch dibatasi −80° (bawah) s/d +80° (atas).

### 1.7 Sistem animasi prosedural (`ProceduralAnimator`)
Lapisan, dievaluasi berurutan di `LateUpdate`:
1. **Pose dasar**: lengan atas abduksi 8°, siku 10°, jari kepal.
2. **Lokomosi** (fase dari jarak tempuh): `φ += (Δjarak / panjangLangkah) * π`.
   - Paha pitch = `A_paha * sin φ`; lutut = `A_lutut * max(0, sin(φ + π/2)) + 5°`; kaki menyesuaikan agar telapak sejajar tanah.
   - Lengan atas pitch = `−0.6 * A_paha * sin φ`; siku = `A_siku`.
   - Pelvis naik-turun `B * |sin φ|`, yaw ±5°, spine kontra-twist ±4°.
   | Gait | Kecepatan (m/s) | Langkah (m) | A_paha | A_lutut | A_siku | B | Condong dada |
   |---|---|---|---|---|---|---|---|
   | Jalan | 1,0–3,4 | 0,75 | 25° | 35° | 15° | 0,03 | 3° |
   | Lari | 3,4–5,6 | 1,2 | 40° | 65° | 60° | 0,05 | 8° |
   | Sprint | 5,6–7,5 | 1,5 | 48° | 80° | 80° | 0,06 | 14° |
   | Jongkok | 0–1,8 | 0,5 | 18° | 100° (dasar) | 30° | 0,01 | 25°, pelvis −0,38 m |
3. **Klip aksi tubuh atas** (`PoseClip`: daftar `(waktu, sendi, euler)` diinterpolasi Hermite; crossfade 0,12 detik). Data klip disimpan sebagai array statis di kode (bukan aset).
4. **Aditif**: napas (dada ±2°, 0,25 Hz istirahat / 0,7 Hz setelah sprint), look-at kepala (yaw ±70°, pitch ±40°), reaksi pukulan (spine terhentak 12° selama 0,2 detik), IK kaki ringan (raycast tiap kaki, angkat/turun pelvis maksimum 0,25 m, tekuk lutut sesuai).
5. **Wajah**: kedip, alis, rahang.

---

## 2. ANDI PRATAMA (27) — Protagonis, Investigator Geologi & Kehutanan
**Psikologi**: rasional, keras kepala, menyimpan rasa bersalah karena tidak ikut ekspedisi pertama (teman kuliahnya, Rendra, ada di tim yang hilang). Takut gelap tapi menolak mengakuinya — monolog batinnya sarkastik saat tegang.

**Fisik**: tinggi 1,75 m, ramping-atletis, proporsi 7,5 kepala ("gaya manhwa aksi"): bahu sedikit lebar (skala X dada 1,05), pinggang ramping (perut 0,92).
**Wajah**: rahang tegas, hidung lurus, alis tebal sedikit menukik, mata coklat tua. Rambut hitam pendek berantakan (12 gumpalan kerucut miring ke kanan, poni jatuh ke dahi kiri).
**TAHI LALAT KHAS**: bola gelap r 0,0045 m (sel 11) di posisi relatif pusat kepala **(−0,036, −0,014, 0,081)** — tepat di bawah mata KIRI, tulang pipi atas. Wajib terlihat jelas pada: layar status karakter, adegan bercermin di sungai (interaksi "BERCERMIN" di permukaan air), cutscene intro & ending.
**Pakaian (luar→dalam)**:
- Jaket windbreaker hijau zaitun (sel 16) hingga pinggul, kerah berdiri, ritsleting abu, **robek di lengan kiri** (lengan atas kiri pakai sel 17 + celah kulit 4 cm berbentuk zigzag), lengan digulung sampai siku kanan.
- Kaus dalam hitam (sel 18) terlihat di leher.
- Sarung tangan taktis setengah jari (sel 19; ujung jari pakai kulit).
- Jam tangan kompas di pergelangan KIRI (silinder 0,02×0,008, sel 22).
- Celana kargo (sel 20) dengan 2 saku samping paha (kotak 0,07×0,09), noda lumpur di lutut ke bawah.
- Sepatu boot lapangan (sel 21).
- Aksesori dinamis: golok di pinggang kanan saat disimpan (holster), ransel kecil di punggung (sel 23).
**Animasi khas**: menyeka keringat (idle > 20 detik siang), menggigil (Basah > 60% malam), melihat jam kompas (saat membuka radar), terengah (stamina < 15%).

## 3. KANG DEDEN SURYANA (42) — Companion 1, Mekanik & Perintis Hutan
**Psikologi**: ayah dua anak dari Garut, sabar, pekerja keras, humor diam-diam. Benturan heli merusak gendang telinganya: **tuli total & bisu traumatis**. Ia membaca tulisan dan bahasa tubuh. Ketika takut ia menjadi kekanak-kanakan (bersembunyi), ketika aman ia sangat produktif.

**Fisik**: tinggi 1,68 m, gempal (skala X/Z 1,12; perut 1,15), lengan kekar (lengan atas ×1,1). Kulit sawo gelap (sel 1). Kumis tipis + janggut tipis (gumpalan pipih sel 9). Rambut cepak beruban di pelipis (campur sel 9 & `#8A8580`).
**Luka**: darah mengering di pelipis kiri (decal sel 13, 3×2 cm), **perban melingkar menutup kedua telinga** (torus pipih sel 14).
**Pakaian**: kemeja flanel kotak-kotak lusuh (sel 26) lengan digulung; **rompi kerja hi-vis oranye kusam** (sel 24) dengan 2 strip reflektif horizontal (sel 25); **iket batik Sunda hitam** melingkar kepala dengan simpul di belakang (torus + 2 sayap kecil, sel 27); celana kerja tebal (sel 28); **sepatu bot karet kuning** setinggi betis (sel 29); golok kerja di pinggang kiri (gagang sel 30); **ransel kerangka bambu** di punggung (rangka 4 batang + tali, sel 31 & 58).
**Animasi khas** (wajib):
| Klip | Deskripsi gerak (durasi) |
|---|---|
| `BacaCatatan` | Kepala condong 10° ke depan, alis turun-dalam (menyipit), mata ke tangan Andi (1,4 s) |
| `SenyumMengangguk2x` | Jaw 0°, alis naik, kepala pitch +15°→0° dua kali (0,8 s) |
| `JempolKanan` | Lengan kanan atas pitch −70°, siku 80°, tangan roll 90° (ibu jari ke atas), tahan 0,7 s |
| `GelengBahu` (tidak bisa) | Kepala yaw ±20° 2×, bahu naik 0,04 m, telapak terbuka (1,2 s) |
| `DudukBersila` | Pelvis turun ke 0,18 m, paha abduksi 65° + rotasi luar 40°, betis menyilang (transisi 0,8 s) |
| `HangatkanTangan` | Duduk bersila, kedua lengan bawah ke depan api, tangan menggosok osilasi 3 Hz amplitudo 3 cm |
| `PanikMenunjuk` | Lengan kanan lurus ke arah hantu, getar 4 Hz ±3°, mundur 3 langkah, mulut terbuka tanpa suara |
| `BerlindungDiBelakang` | Jongkok di belakang Andi (offset −1,2 m arah hantu), kedua tangan di atas kepala |
| `MemanggulKayu` | Sama dengan Andi (lihat 07) |
| `Menebang` | Ayunan horizontal golok/kapak 0,9 s |
| `Tidur` | Berbaring miring di kasur saung |

## 4. NINGSIH (22) — Companion 2, Putri Juru Kunci
**Psikologi**: cerdas, liar, trauma; tiga bulan hidup sendiri di hutan. Membenci Sekte, takut pada orang asing bersenjata. Rasa ingin tahu terhadap Andi tumbuh pelan. Ia tidak berbicara (memilih diam sejak ritual) — ekspresi lewat senandung, gerak tari, dan tatapan.
**Indra keenam**: merasakan hantu & pintu tersembunyi. **Tiga tanda segel kuno** (pola lingkaran-trisula sederhana, decal emisi sel 15) di lengan atas kiri, lengan bawah kanan, dan bahu kanan — berpendar samar (intensitas 0,6) dan **berdenyut lebih terang** (hingga 2,5) saat hantu dalam 40 m: ini juga petunjuk visual bagi pemain.

**Fisik**: tinggi 1,58 m, ramping lincah (X 0,88). Kulit kuning langsat (sel 2). Wajah oval, mata besar sedikit sipit, alis tipis melengkung.
**Rambut**: hitam panjang tergerai hingga pinggang — 14 kartu rambut lengkung (Cutout, 2 sisi), bergoyang prosedural (pegas: kekakuan 18, redaman 4) mengikuti kecepatan & angin. **Bunga kantil putih** (5 kelopak memanjang, sel 34) di telinga kanan.
**Pakaian**: kebaya putih-krem (sel 32) model kutu baru, **koyak di kedua lengan** (lengan pendek tidak rata, kulit terlihat); bawahan **kain jarik batik parang gelap selutut** (sel 33) dililit dengan ujung lipatan wiru di depan; **bertelanjang kaki** (kaki pakai sel kulit, kuku `#E3B9A0`); selendang merah tua (sel 35) terikat di pinggang (ujung menjuntai, bergoyang).
**Animasi khas**:
| Klip | Deskripsi |
|---|---|
| `Mengintip` | Bersandar di sisi pohon, spine side-bend 25°, hanya kepala & bahu terlihat; kepala miring 10° |
| `Kabur` | Lari 7,5 m/s dengan lompatan melewati semak (lompat 0,7 m tiap 3 langkah) |
| `MendekatWaspada` | Jalan 1,1 m/s, lengan rapat di dada, berhenti tiap 4 detik menatap Andi |
| `Bersenandung` | Kepala bergoyang ±5° 0,5 Hz, mata setengah tertutup |
| `TariPagi` | Gaya tari klasik pelan: lutut mendhak 15°, pelvis sway ±6°, tangan ngithing (pergelangan berputar bergantian), langkah kecil kiri-kanan; siklus 8 ketuk 4,8 s; diulang 4–6 siklus |
| `MemberiHadiah` | Kedua tangan terulur di depan dada memegang item (item ditempel di `RightHandGrip`), kepala sedikit menunduk |
| `LompatSamping` | Pelvis melengkung 0,6 m ke samping 2,2 m dalam 0,45 s, mendarat jongkok |
| `TembakBusur` | Tarik 0,5 s, lepas, kembali 0,3 s |
| `TembakSuar` | Lengan kanan lurus, hentakan recoil 12° |
| `MerasakanGaib` | Mata terpejam, tangan kanan di dada, segel menyala ×2,5, lalu menunjuk arah (2,2 s) |
| `DudukTimpuh` | Duduk dengan kaki terlipat ke samping, dekat api |

## 5. KI ROGO (58) — Pimpinan Sekte Penghuni Alas (Antagonis)
**Psikologi**: mantan dukun desa yang bersekutu dengan PT Bhumi Cakra demi kuasa; percaya kegelapan di bawah beringin adalah "dewa" yang memberinya umur panjang. Karismatik, dingin, tidak pernah berteriak.
**Fisik**: 1,82 m, sangat kurus (X 0,85), jenggot abu panjang hingga dada (kartu rambut `#8E8A84`), mata menyala merah samar saat malam (sel 53). **Blangkon hitam** (sel 50), **jubah hitam** panjang menyapu tanah (sel 48) di atas **surjan lurik** (sel 49), kalung taring (sel 51), topeng kayu merah tergantung di pinggang (sel 41), **tongkat kayu** 1,7 m dengan tengkorak kerbau kecil di ujung.
**Animasi**: berjalan lambat bertongkat, mengangkat tongkat (memanggil), menghentak tongkat (gelombang kejut), mundur menembus akar (dissolve), wujud bayangan (material transparan hitam-merah α 0,6 saat siege).

## 6. SEKTE PENGHUNI ALAS (5 varian)
Penduduk desa & pekerja yang kehilangan akal karena kutukan. Semua memakai topeng kayu (oval, lubang mata sipit, hidung panjang terukir; variasi warna).
| Varian | Tinggi/skala | Visual | Senjata |
|---|---|---|---|
| `AbdiSekte` (pria) | 1,68–1,78; X 1,0 | Bertelanjang dada dengan garis cat putih (sel 44), celana lusuh selutut (sel 42), selendang merah (sel 43), topeng kayu (40) | Parang / pentungan kayu |
| `AbdiWanita` | 1,55–1,62; X 0,9 | Baju kurung lusuh panjang (42), rambut ijuk dikepang (46), topeng setengah wajah | Bambu runcing (tusuk & lempar) |
| `AbdiLumpur` | 1,70; X 1,0 | Seluruh tubuh berlumur lumpur (45), **caping anyaman** (47), tanpa topeng (wajah lumpur) | Pisau batu; menyergap dari rawa |
| `AbdiButo` (gendut) | 1,85; X/Z 1,6 | Perut besar, topeng merah (41), kain sarung lusuh | **Alu** (penumbuk padi) berat |
| `PemimpinPatroli` | 1,75 | Ikat kepala merah, kentongan bambu tergantung di dada, topeng berukir emas pudar | Parang + kentongan |
**Animasi**: jalan bungkuk patroli, jongkok mengamati, mengintip dari pohon, memukul kentongan (pegang tabung di tangan kiri, pukul dengan tongkat kanan), gertakan (lari 3 langkah, berhenti, lengan terbuka lebar, kepala ke depan), serangan ayun, lompatan serang 3 m, **menyerah** (berlutut, kedua tangan terangkat telapak terbuka, topeng menunduk), **menyeret teman** (jalan mundur memegang pergelangan teman), takut (mundur sambil melindungi wajah), bertengger di dahan (jongkok di cabang 3–5 m), menari ritual di sekitar api (malam di desa), mati (tumbang → kejang 0,6 s → larut menjadi abu hitam 4 s).

## 7. HANTU & MAKHLUK (desain visual)
| Makhluk | Tinggi | Visual kunci | Gerak |
|---|---|---|---|
| **Pocong Hutan** | 1,75 | Kain kafan kusam bernoda tanah (sel 38) membungkus dari kepala hingga kaki; **simpul tali di atas kepala & di pergelangan kaki** (sel 39); wajah terbuka pucat abu-hijau (sel 3), lingkar mata gelap; titik mata emisi samar malam | Melompat dua kaki: jongkok-pipih 0,9 → melambung 1,08 (tinggi 0,5 m, jarak 2,5 m, interval 0,9 s) → mendarat pipih 0,85; diam bergoyang ±4°; kepala miring 25° saat melihat pemain; serangan = lompatan terjang 3,5 m |
| **Pocong Gua** | 1,70 | Sama, kafan lebih gelap & basah (`#9A9686`), lumut kebiruan di kaki | Melompat dari langit-langit gua (jatuh vertikal) |
| **Kuntilanak Beringin** | 1,70 (melayang 0,3–1,2 m) | Gaun putih panjang koyak (sel 54) menutup kaki; **rambut hitam sangat panjang** menutupi sebagian wajah (20 kartu rambut, sel 55); lengan pucat (sel 4), kuku panjang; mata kadang merah | Melayang bob ±0,15 m 0,4 Hz; meluncur antar pohon; duduk di dahan dengan rambut menjuntai; menjerit (kepala ke belakang, rahang 35°, lengan terbuka); tertawa (bahu bergetar 6 Hz); menukik dengan cakar; menghilang (skala → 0 + partikel kabut) |
| **Banaspati** | Bola Ø 0,9 | Bola api oranye-merah dengan wajah samar (2 mata gelap + mulut), ekor api partikel | Mengorbit target, menerjang lurus; padam oleh hujan/air/`BotolAirBersih` dilempar |
| **Genderuwo (Boss)** | 2,8 | Raksasa berbulu kasar gelap (sel 52; bulu = 300 duri kerucut kecil di punggung/bahu/lengan), lengan sampai lutut, bahu sangat lebar (X 1,9), **mata merah menyala** (sel 53), taring bawah | Jalan bertumpu buku jari; hentakan dua tinju (gelombang 6 m); menyapu lengan; mengamuk berlari; mengaum; tertatih (fase 3) |
| **Jenglot Gua** (lanjutan) | 0,4 | Humanoid kering kecil, kulit coklat keriput, rambut panjang kusut, taring | Merangkak cepat berkelompok 4–6 |
| **Manusia Akar** (lanjutan) | 2,0 | Dua tubuh Sekte menyatu oleh akar beringin di punggung | Berjalan miring, menyerang dengan 4 lengan |
| **Kuyang** (v3) | kepala Ø 0,25 + juntaian 1,2 m | Kepala perempuan pucat, rambut panjang, mata menyala; di bawah leher menjuntai jantung, paru, usus (mesh organ basah `AK/Wet`, tulang pegas) yang menetes | Melayang berayun, juntaian bergoyang, menukik |
| **Tuyul** (v3) | 0,7 | Makhluk kecil gundul abu-hijau, telinga runcing, mata besar hitam, bercawat lusuh; TIDAK pernah diberi efek gore | Berlari kecil terkikik, memeluk barang curian |
| **Genderuwo Hutan** (v3) | 2,2 | Versi kecil Genderuwo, bulu lebih jarang, satu mata merah | Bertumpu buku jari, melempar batu |
| **Mutan Gembung** (v3) | 1,8 | Tubuh bengkak 1,7× penuh urat hitam berdenyut (emisi gelap), kulit pucat mengilap, sisa baju lab | Terseok, perut berdenyut sebelum meledak |
| **Mutan Jemari** (v3) | 2,3 | Sangat kurus, sendi berlebih, jari 40 cm, kepala tanpa wajah dengan mulut vertikal; satu memakai tanda pengenal "RENDRA S." | Merayap di dinding/langit-langit, gerak patah-patah (pose disentak) |
| **Penyerupa** (v3) | sesuai yang ditiru | Salinan Deden/Ningsih tanpa bayangan & tanpa napas | Diam, kepala berputar 180° |

Kualitas animasi v3 (IK, tulang pegas, blendshape, sakade): `15_VISUAL_FIDELITY_DAN_SHADER.md` §7. Prop anggota tubuh & tutup potongan per varian: `14` §5.

## 8. TOKOH PENDUKUNG (cutscene/ending)
- **Pilot SAR "Kapten Hendra"**: helm putih, baju terbang oranye, hanya muncul di pintu helikopter ending (berdiri, melambai, menerima dokumen).
- **Rendra** (teman Andi di ekspedisi pertama): hanya muncul sebagai foto di dokumen lore (tidak dimodelkan).

## 9. HEWAN (kerangka quadruped 15 sendi: Root, Spine, Chest, Neck, Head, 4 kaki × (Upper, Lower, Foot); ekor opsional 1–2 sendi)
| Hewan | Ukuran | Visual | Perilaku singkat (detail di 03/05) |
|---|---|---|---|
| Rusa Timor | tinggi bahu 0,95 | Coklat kemerahan, tanduk bercabang 3 (jantan) | Merumput, lari zig-zag saat takut |
| Kancil | 0,45 | Coklat oranye, kaki ramping | Melesat ke semak |
| Babi Hutan | 0,8 | Hitam keabu, taring putih, bulu kasar | Menyeruduk jika dilukai |
| Ayam Hutan | 0,45 (biped) | Jantan merah-emas-hijau, ekor melengkung | Berkotek, terbang pendek |
| Biawak | panjang 1,6 | Abu-gelap bertotol kuning | Berjemur, kabur ke air |
| Ikan | 0,3–0,6 | Perak/hijau | Berenang berkelompok di sungai/laut |
| Kelelawar Gua | rentang 0,4 | Hitam | Terbang massal saat diganggu (partikel mesh) |
| Penyu (ambien, tidak bisa diburu) | 0,9 | Hijau-coklat | Merangkak di pantai malam |
