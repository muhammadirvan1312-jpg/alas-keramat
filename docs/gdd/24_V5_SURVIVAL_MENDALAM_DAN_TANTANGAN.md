# 24 — V5 SURVIVAL MENDALAM & TANTANGAN
> Melengkapi 05 (rumus vitals lama tetap berlaku) dan mengalahkannya bila bertentangan. Prinsip: **hutan tropis Jawa yang membunuh pelan-pelan** — bukan hanya lapar & haus, tetapi air kotor, penyakit, luka, cuaca, hewan, dan keputusan buruk. Setiap bahaya punya tanda, pencegahan, dan obat yang bisa dibuat dari hutan.

## 1. Tubuh Andi (lapisan baru di `AndiVitalsAndSanity` + file partial `.Tubuh.cs`, `.Penyakit.cs`)
### 1.1 Suhu tubuh (menggantikan "ColdWet" sebagai inti; ColdWet tetap untuk tampilan)
`SuhuTubuh` 34,0–41,0 °C, normal 36,8. Dipengaruhi suhu udara (pagi 22°, siang 31–35° kemarau, malam 19–24°, gua 17°, puncak Z10 14°), basah, angin, pakaian, api, tenda/saung, aktivitas (sprint +0,02°/s), makan/minum panas.
| Kondisi | Ambang | Efek | Penanganan |
|---|---|---|---|
| Kedinginan | < 36,0 | Tangan gemetar (bidik goyang), stamina cap −15 | Api, pakaian kering, minuman panas |
| Hipotermia | < 35,0 | Penglihatan kabur, lambat, HP −0,2/s | Api + selimut kulit; jangan berenang malam |
| Kepanasan | > 38,0 | Haus ×1,8, stamina pulih ×0,5 | Teduh, air, mandi sungai, topi daun |
| Heatstroke | > 39,5 | Pingsan acak 3–6 s, HP −0,3/s | Air + teduh + kain basah; kemarau siang bahaya |

### 1.2 Gizi (empat cadangan, menggantikan satu bar "Kenyang" di logika; HUD tetap satu bar Kenyang = rata-rata)
| Cadangan | Sumber | Bila rendah (< 20) |
|---|---|---|
| Karbo (energi) | Pisang, singkong, ubi, sagu, tebu, nasi ransum | Stamina pulih ×0,5, cepat lelah |
| Protein | Daging, ikan, telur, serangga goreng, kepiting | Luka sembuh ×0,4, Kekuatan tidak naik |
| Lemak | Daging berlemak, kelapa, ikan, minyak | Dingin ×1,4 |
| Vitamin | Buah, daun singkong, pakis, jambu, kelor | Luka infeksi ×2, rabun senja (malam lebih gelap) |
Makan monoton (satu jenis > 60% selama 2 hari) → "Bosan": nafsu makan turun, Kewarasan −5/hari. Pemain didorong berburu, mencari, dan memasak variasi.

### 1.3 Air bersih
| Sumber | Risiko |
|---|---|
| Air sungai/rawa mentah | 35% Diare (rawa 60% + Leptospirosis 10% bila ada bangkai tikus di hulu) |
| Air hujan tertampung | Aman (penampung harus bersih) |
| Air kelapa | Aman, Haus +25, Karbo +5 |
| Air direbus (panci) | Aman, butuh api 40 s game |
| Saringan arang–pasir–ijuk (blueprint) | Risiko ×0,3, tidak butuh api |
| Air sumur Z11 | Selalu kotor (cerita) |
**Diare**: Haus ×2,5, berkurang dengan oralit improvisasi (air + garam laut + gula aren) atau jamu kunyit. Tanpa obat 1,5 hari game.

### 1.4 Tidur & kelelahan
Hutang tidur menumpuk (Rest lama tetap). Begadang > 26 jam: mikro-tidur (layar gelap 1 s), halusinasi ringan (bayangan di tepi layar, bisikan — ditandai `TerrorDirector` sebagai kejadian palsu). Tidur di tanah tanpa alas: Rest pulih ×0,6 + 30% gigitan nyamuk/semut. Tidur di saung berlantai & kelambu = penuh.

## 2. Penyakit & Racun (`StatusEffect` diperluas; nilai flag baru FINAL)
`Diare = 32, Malaria = 64, Demam = 128, Infeksi = 256, Leptospirosis = 512, BisaUlar = 1024, Lintah = 2048, SengatanTawon = 4096, Hipotermia = 8192, Heatstroke = 16384, Kelelahan = 32768`
| Penyakit | Penyebab | Gejala | Obat dari hutan |
|---|---|---|---|
| Malaria | Gigitan nyamuk malam di Z8 rawa & dekat air diam (peluang per gigitan 3%) | Demam berulang tiap 12 jam game (menggigil → panas), stamina −30% | Daun sambiloto (pahit, Kewarasan −3), kina dari arsip pabrik (langka); cegah: kelambu, asap api, lotion sereh |
| Infeksi luka | Luka terbuka > 4 jam game tanpa dibersihkan | Bar luka menghitam, Demam, HP cap turun | Bersihkan (air rebus/alkohol aren), daun sirih, jahit luka |
| Leptospirosis | Air rawa/kencing tikus | Nyeri otot, mata merah, berat | Jamu + istirahat 2 hari; kalau parah butuh antibiotik bunker UG2 |
| Bisa ular (weling/kobra/viper hijau) | Gigitan di semak malam, gua | Lengan/kaki bengkak, penglihatan berbayang, HP −0,4/s | Bebat tekan (bukan torniket — torniket = kehilangan tangan sementara), jangan lari (bisa menyebar ×2), serum di bunker; ramuan daun hanya memperlambat |
| Lintah | Rawa, sungai hutan | Pendarahan kecil, Kewarasan −1 | Garam/tembakau/api di ujung lintah; dicabut paksa = luka |
| Sengatan tawon | Merusak sarang (ada di pohon) | Bengkak, alergi 5% → sesak | Lumpur/cuka aren; asap untuk mengusir sebelum ambil madu |
| Keracunan jamur | Jamur beracun (05) | Muntah, halusinasi | Arang tumbuk, air banyak |

## 3. Luka per Bagian Tubuh (melengkapi Pendarahan 14)
| Luka | Efek | Perawatan |
|---|---|---|
| Sayatan dalam | Pendarahan | Perban kain (05), jahit dengan jarum+benang (20 §3) |
| Kaki terkilir/patah (jatuh > 4 m, lori, serudukan babi) | Jalan pincang, tidak bisa sprint/lompat | Bidai bambu + kain; sembuh 2–4 hari game |
| Lengan patah | Tidak bisa senjata dua tangan/busur | Gendongan kain; 3 hari |
| Gegar otak (dihantam Genderuwo, jatuh kepala) | Layar berdenging, kabur, muntah | Tidur, hindari pertarungan 1 hari |
| Luka bakar (api, bom bambu) | Nyeri, infeksi mudah | Lidah buaya, kain bersih |
Luka terlihat di tubuh Andi (lengan/tangan FPS) dan di layar kamera saku.

## 4. Alat, Api & Kerajinan
- **Ketahanan alat**: kapak, golok, tombak, busur, senter, korek punya durabilitas; tumpul → damage & kecepatan tebang turun; diasah di Batu Asah (BlueprintID 17); patah → bahan sisa bisa dipakai lagi.
- **Membuat api** bertingkat: korek gas (isi terbatas 40 kali) → batu api + sabut kelapa → bor tangan kayu (mini-game ritme, gagal saat basah) → membawa **bara di tempurung kelapa** antar kemah. Kayu basah berasap & sulit menyala; kayu dikeringkan di rak dekat api.
- **Pengawetan**: asap (jemuran ikan/daging di atas api, 6 jam game, tahan 6 hari), garam laut (rebus air laut di pantai Z7 → garam), keringkan matahari kemarau, sale pisang; makanan basi menarik tikus, lalat, babi hutan, dan **Sekte** (bau).
- **Kualitas kerajinan**: setiap hasil kerajinan punya mutu Kasar/Baik/Halus, naik sesuai pengalaman membuat jenis itu (tanpa menu level) — tombak halus lebih kuat & awet.

## 5. Perkemahan Hidup
- **Hama**: tikus mencuri makanan di rak terbuka (butuh peti/penggantung), rayap merusak bangunan kayu yang menyentuh tanah langsung (pakai fondasi batu/tiang panggung), semut merah di gula.
- **Bocor & banjir**: atap daun bocor saat badai (lantai basah, api padam); sungai meluap musim penghujan — kemah di dataran rendah bisa kebanjiran (barang hanyut).
- **Bau & asap**: api besar dan masakan terlihat/tercium Sekte dari jauh; api kecil di lubang (Dakota fire pit) tersembunyi.
- **Pertanian**: kebun kecil (singkong, ubi jalar, cabai, kangkung, pisang) — tanam stek/bibit dari desa; tumbuh 4–8 hari game; butuh air & pagar dari babi hutan; panen musim berbeda.
- **Ternak liar**: tangkap ayam hutan dengan jerat → kandang bambu → telur harian.

## 6. Bahaya Alam & Hewan
| Bahaya | Kapan | Tanda | Respon |
|---|---|---|---|
| Banjir bandang | Hujan badai di hulu (Z10) | Air keruh naik, gemuruh, kayu hanyut | Naik ke tempat tinggi dalam 40 s |
| Longsor | Lereng curam setelah hujan > 6 jam | Kerikil berjatuhan, tanah retak | Menjauh dari tebing |
| Pohon tumbang badai | Angin > 60% | Derit kayu keras | Jangan berteduh di bawah pohon mati |
| Kebakaran hutan | Kemarau + api tak terjaga / petir | Asap coklat, burung kabur | Bakar balik (firebreak), lari melawan angin |
| Petir | Badai di tempat terbuka/tinggi | Rambut berdiri (efek visual), dengung | Turun dari bukit, jauhi pohon tunggal |
| Babi hutan | Siang, ladang | Dengus & semak tersibak | Pagar, tombak, panjat |
| Buaya muara | Z8, air keruh | Gelembung, mata di permukaan | Jangan menyeberang rawa malam |
| Ular | Semak & gua malam | Desis, gerak rumput | Senter, sepatu bot, tongkat di depan |
| Macan tutul jawa (langka, satu individu) | Z10/Z12 malam | Langkah hampir tak terdengar, monyet menjerit | Api, jangan lari, tatap; membunuhnya = Kewarasan −20 (hewan langka, dihormati Mbah Sastro) |
| Anjing liar | Desa mati Z3, kampung Z11 | Gonggong bersahutan | Lempar daging, panjat |
| Tawon | Pohon tertentu | Dengung | Asap |

## 7. Navigasi & Tersesat
Tanpa penanda GPS di awal (GPS dari Rendra/heli muncul babak 2). Navigasi: matahari, bintang (Waluku/Orion di selatan jam tertentu), suara sungai, peta kertas yang digambar otomatis seiring eksplorasi (kabut perang), menandai pohon dengan golok (goresan terlihat di dunia), menumpuk batu. **Alas Wingit**: kompas berputar, jalan "berulang" (pemain bisa tersesat sungguhan) — keluar dengan mengikuti aliran air atau benang yang diikat dari pintu masuk.

## 8. Tingkat Kesulitan & Mode Tantangan (mengganti 05 §9)
| Mode | Ringkas |
|---|---|
| Damai | Tanpa Sekte agresif & hantu menyerang, vitals ×0,5 — untuk cerita |
| Normal | Nilai dasar dokumen |
| Sulit | Vitals ×1,3, penyakit ×1,5, alat lebih cepat rusak, Sekte lebih cerdas (mengapit, memasang jebakan) |
| **Sengsara** | Semua sistem §1–§7 maksimal: suhu ekstrem, penyakit realistis, tanpa HUD vitals (hanya tanda tubuh: napas, gemetar, warna layar, suara perut), simpan hanya saat tidur di kasur, sumber daya dekat kemah habis permanen (tumbuh lagi 20 hari), malam lebih panjang (jam 18:00–06:00 bernilai ×1,5 waktu nyata) |
| **Pantang Mundur** | Sengsara + satu nyawa (mati = simpanan dihapus). Medali di layar statistik |
| Kustom | 05 Mode Kustom + pengali suhu, penyakit, banjir, ketahanan alat, kelangkaan, panjang malam |
**Tantangan opsional** (dicentang saat Mulai Baru, memberi medali): *Vegetarian* (tanpa daging), *Tanpa Bangunan* (hanya tenda), *Sunyi* (tidak boleh membunuh manusia/Sekte), *Pemburu* (hanya busur & tombak), *Tanpa Deden*, *Malam Abadi* (hari 3 jam, malam 21 jam).

## 9. Kurva Kesulitan Alami
Hari 1–3: belajar (sumber daya melimpah dekat Z1). Hari 4–8: sumber daya dekat kemah menipis, penyakit mulai muncul, Sekte mulai menyerang kemah. Hari 9+: musim penghujan pertama (banjir, malaria), Malam Darah, wajib pindah/perluas kemah dan bertani. Pemain yang bertahan 30 hari mendapat "Penghuni Hutan" — Mbah Sastro mengakui Andi.

## 10. ID Baru (FINAL; tambahkan ke `SOTFContracts` pada tahap yang memakainya)
`ItemID` 230 SabutKelapa, 231 BatuApi, 232 BorTangan, 233 BaraTempurung, 234 GaramLaut, 235 GulaAren, 236 Oralit, 237 DaunSambiloto, 238 Kina, 239 SerumBisa, 240 Antibiotik, 241 LidahBuaya, 242 Kelambu, 243 LotionSereh, 244 Madu, 245 StekSingkong, 246 BibitUbi, 247 BijiCabai, 248 Singkong, 249 Ubi, 250 Cabai, 251 Kangkung, 252 DaunKelor, 253 Jambu, 254 SeranggaGoreng, 255 SelimutKulit, 256 TopiDaun, 257 BenangPenanda, 258 KainBasah, 259 TemaketAir (tempurung minum). `BlueprintID` 31 SaringanAir, 32 RakPengering, 33 KebunKecil, 34 KandangAyam, 35 PenggantungMakanan, 36 TungkuDakota, 37 TendaKulit. `RecipeID` 17–40 (resep §1–§5, daftar final ditulis di T5/T6).

## 11. Pembagian Tahap
T3: suhu tubuh, gizi 4 cadangan, tidur, air bersih dasar. T5: alat & ketahanan, membuat api bertingkat, pengawetan, kualitas kerajinan, hewan §6. T6: perkemahan hidup (hama, bocor, banjir, kebun, kandang). T7: luka per bagian tubuh, bisa ular. T9: penyakit §2 lengkap, bahaya alam §6, navigasi §7. T11: mode kesulitan §8 & tantangan opsional.
