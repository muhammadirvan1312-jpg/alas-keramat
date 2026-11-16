# 06 — SISTEM TEMPUR, SENJATA, MUSUH, AI PSIKOLOGIS, SPAWNER, RAID, BOSS & SIEGE
Dibaca pada tahap T7 dan T10. Semua damage dalam HP; waktu dalam detik nyata.

---

## 1. Kontrol Tempur Layar Sentuh
| Tombol | Ketuk | Tahan | Ketuk ganda |
|---|---|---|---|
| **TEBAS/TEMBAK** (merah bata) | Tebas ringan; kombo jika diketuk lagi dalam 0,6 s (maks 3 pukulan) | ≥ 0,6 s: **Tebasan Berat** saat dilepas; busur: tarik selama ditahan, lepas = tembak; tombak: bidik lempar (titik lintasan), lepas = lempar | — |
| **TANGKIS** (biru baja) | **Parry** (jendela 0,25 s; Keris 0,35 s) | ≥ 0,4 s: **Blok** (damage melee −60%, stamina −1,5 × damage) | **Simpan/Keluarkan Senjata (Holster)** |
| **LEMPAR** | Lempar kayu dari bahu / lempar tombak / lempar bom / lempar botol air | — | — |
- **Bantuan bidik (aim assist)**: melee — saat target dalam 3 m dan kerucut 25°, yaw kamera diputar menuju target maks 90°/s selama ancang-ancang. Jarak jauh — sensitivitas ×0,6 saat crosshair dalam 4° dari target ≤ 40 m. Bisa dimatikan di Pengaturan.
- **Deteksi kena melee**: `Physics.SphereCast` radius 0,35 dari kamera sepanjang jangkauan senjata, hanya di "frame aktif" klip (lihat tabel), mengenai `IDamageable`. Ringan 1 target, berat maks 2 target.
- **Umpan balik**: hit-stop 0,05 s (berat), guncang kamera 0,15, tanda kena putih 0,15 s di crosshair, suara benturan sesuai material (daging/kafan/kayu/logam). Angka damage MATI (opsi di pengaturan).
- **Tebasan Berat Penghabisan**: setelah parry berhasil, tebasan berat dalam 2,0 s = damage ×2,5 dan Sekte langsung tumbang. Dengan Gore Penuh = **Tebasan Berat Penggal** (14 §5); Gore Mati = larut jadi abu. Elak, Bunuh Senyap, Asah, zona tubuh: 17 §2.

## 2. Tabel Senjata (ID dari 05 §4.3)
| Senjata | Ringan | Berat | Damage tebang | Jangkau | Durasi ringan/berat (frame aktif) | Stamina R/B | Khusus |
|---|---|---|---|---|---|---|---|
| Tangan kosong | 6 | 12 | 2 | 1,2 m | 0,4 / 0,9 (0,12–0,2 / 0,45–0,6) | 3 / 8 | — |
| GolokPusaka | 18 (kombo 18-18-26) | 45 | 20 | 1,8 m | 0,45 / 1,0 (0,15–0,25 / 0,55–0,7) | 5 / 14 | Parry normal |
| KapakBatu | 20 | 48 | 28 | 1,7 m | 0,55 / 1,1 | 6 / 15 | — |
| KapakBaja | 26 | 60 | 40 | 1,8 m | 0,5 / 1,05 | 6 / 15 | — |
| Cangkul | 14 | 30 | 8 | 1,9 m | 0,6 / 1,2 | 6 / 15 | Menggali (GALI BUNKER) |
| TombakBambu | Tusuk 22 | Lempar 55 | 4 | 2,4 m / lempar 25 m/s, gravitasi 0,5 | 0,5 / bidik | 5 / 10 | Bisa dipungut kembali |
| BusurBambu + AnakPanah | 30 (+0–20 sesuai tarikan 0,4–1,2 s) | — | — | 35–55 m/s, gravitasi | tarik | 2/s setelah 2 s | Kepala ×2 |
| BusurBambu + PanahApi | 26 + bakar 5/s 6 s | — | — | sama | — | — | Menyalakan api unggun dari jauh, membakar akar UG3 |
| Ketapel + Batu | 10 + goyah 0,6 s | — | — | 30 m/s | — | 3 | Lempar umpan suara (alihkan Sekte) |
| PistolSuar | 40 + bakar 6/s 5 s | — | — | 45 m/s | isi ulang 2,5 s | — | Cahaya suar 8 s, Takut Sekte +30 |
| BedilLocok | 95 | — | — | hitscan 60 m, sebaran 1,5° | isi ulang 4,5 s | — | Bunyi menarik Sekte radius 120 m |
| KerisPusaka | 30 | 75 | 10 | 1,6 m | 0,4 / 0,95 | 4 / 12 | ×2 vs hantu, parry 0,35 s |
| GolokApiPusaka | 28 + bakar 4/s 4 s | 70 | 24 | 1,8 m | 0,45 / 1,0 | 5 / 13 | Cahaya 3 m |
| BomBambuMinyak | area api r 3,5 m 8 s, 20/s | — | — | lempar 18 m/s | — | — | Membakar akar & struktur |
| Obor (tangan kiri, dipukulkan) | 8 + bakar 3/s 3 s | — | — | 1,4 m | 0,5 | 4 | Pocong ×2 |
Tangan kiri (Obor/Senter/GPS) tetap aktif saat tangan kanan menyerang (kecuali busur, tombak lempar, bedil: tangan kiri disimpan sementara).

## 3. Parry, Blok, & Reaksi
- Parry berhasil bila momen kena serangan musuh jatuh di jendela parry DAN musuh dalam kerucut depan 70°: Andi 0 damage, musuh **goyah 1,2 s**, Takut +20, membuka Tebasan Penghabisan.
- Parry terhadap: Pocong (terjangan) → Pocong terjungkal berbaring 2 s (damage ×1,5); Kuntilanak (tukikan) → mundur & menghilang 10 s; Genderuwo (hantaman tanah) → TIDAK bisa di-parry (harus menghindar).
- Reaksi musuh: kena ringan = tersentak 0,25 s; kena berat = goyah 0,7 s; Abdi/Wanita HP < 40% terkena berat = jatuh 1,2 s.
- Andi terkena: guncang kamera, vignette merah 0,3 s, suara erangan sintetis pelan; kena ≥ 25 = terjatuh 0,8 s.

## 4. Statistik Musuh (`EnemyArchetype`, nilai integer final)
| ID | Archetype | HP | Jalan / Lari (m/s) | Lihat siang / malam (m) | Dengar (m) | Damage ke Andi | Drop |
|---|---|---|---|---|---|---|---|
| 0 | AbdiSekte | 80 | 1,6 / 5,4 | 35 / 18 (sumber cahaya terlihat dari 60) | 20 | Parang 12, pentungan 10, lompatan 16 | KainBatik 30%, Damar 20%, AnakPanah ×2 20% |
| 1 | AbdiWanita | 60 | 1,8 / 6,2 | 38 / 20 | 22 | Bambu runcing tusuk 14, lempar 18 | TaliIjuk 30% |
| 2 | AbdiLumpur | 70 | 1,4 / 5,0 | 20 / 15 | 25 | Pisau 9 (sergapan ×2) | Batu ×2 |
| 3 | AbdiButo | 300 | 1,2 / 3,6 | 30 / 15 | 18 | Alu 30 + jatuh | Tengkorak 40%, LogamBekas 30% |
| 4 | PemimpinPatroli | 120 | 1,6 / 5,6 | 40 / 20 | 25 | Parang 14 | KainBatik, MesiuHitam 30% |
| 5 | KiRogo (Bayangan) | 400 | 1,4 / — | — | — | Gelombang tongkat 20 (r 5 m) | — |
| 6 | PocongHutan | 120 | lompat rata-rata 2,8; terjang 3,5 m | indra 30 (45 bila Sanity < 25) | 15 | Terjang 15 | KainKafan 50%, Kemenyan 25% |
| 7 | PocongGua | 100 | sama | 25 | 15 | 15 | KainKafan 50% |
| 8 | Kuntilanak | 220 | melayang 4,0 / tukik 12 | 50 | 40 | Cakar 18; jeritan setrum 1 s | — (diusir, kembali malam berikut) |
| 9 | Banaspati | 50 | 6,0 | 40 | — | 10 + bakar 4/s 3 s | Arang ×2 |
| 10 | Genderuwo (Boss) | 1500 | 2,0 / 9,0 (amuk) | — | — | Hantam 35 (cincin 6 m), sapu 25, amuk 40, lempar akar 20 | — |
| 11 | Jenglot (lanjutan) | 15 | 5,5 merangkak | 15 | 10 | Gigit 4 | — |
| 12 | ManusiaAkar (lanjutan) | 400 | 1,5 / 3,0 | 25 | 15 | 4 lengan 12 ×2 | Tulang ×4 |
Musuh v3 (Kuyang, Tuyul, Genderuwo Hutan, Mutan Gembung, Mutan Jemari, Penyerupa, Hantu Air): `17` §5 & `13` §4. Hantu tidak "mati": ketika HP 0 mereka **diusir** (larut jadi debu/kabut) dan dihitung di statistik "Hantu Diusir".

## 5. Persepsi Sekte
- Meter **Kesadaran** 0–100 per musuh. Naik = faktor terlihat × (1 / max(1, jarak/10)) × 40/s; turun 8/s bila tidak terlihat.
- Faktor terlihat: dalam kerucut 110° & tidak terhalang (raycast ke kepala & pinggul Andi); jongkok ×0,6; jongkok di semak ×0,35 (dan semak memblok garis pandang); malam ×0,5 KECUALI Andi memegang obor/senter (×1,5 & jarak 60 m); hujan badai ×0,7.
- Pendengaran: langkah lari 20 m, jalan 8 m, jongkok 3 m, tebang 45 m, pohon tumbang 80 m, bedil 120 m, ketapel batu jatuh (umpan) 15 m dari titik jatuh.
- Ambang: 30 = penasaran (menoleh), 60 = waspada (menyelidiki, memanggil), 100 = terlibat.

## 6. State Machine Sekte (`NusantaraEnemyAndMutantAI`)
| State | Masuk bila | Perilaku | Keluar ke |
|---|---|---|---|
| **Patroli** | Default | Rute titik di Desa/Sawit; pengintai berkeliling menuju kemah pemain (siang Hari ≥ 1 sore) | Mengintai / Waspada |
| **Mengintai** | Kesadaran ≥ 30 & Agresi kelompok < ambang serang | Jaga jarak 25–40 m di balik penutup (pohon dalam cincin itu), mengintip; 25% memanjat & bertengger di dahan 4 m (Jati/Mahoni) selama 20–60 s | Mundur bila Andi mendekat < 15 m; Gertak bila Agresi > 60 |
| **Isyarat** | Pemimpin melihat Andi / kesadaran 100 | Memukul kentongan "Tok-Tok… Tok-Tok" (audio 3D); Sekte dalam 120 m +40 kesadaran & berkumpul | Serang / Gertak |
| **Gertak** | 40% bila Takut > Agresi×0,6 | Lari 3–4 langkah ke Andi, berhenti 6–8 m, berteriak (geraman sintetis), lengan terbuka, lalu mundur 5 m | Serang / Mengintai |
| **Serang** | Agresi tinggi atau diserang | **Token**: maks 2 penyerang aktif (Sulit 3). Lainnya **Mengitari** pada 6–9 m sambil menyamping. Pola: ayunan (ancang-ancang 0,45 s, lengan terangkat + gerutu), lompatan 3 m (ancang 0,6 s), lempar bambu (Wanita, 8–18 m). Target: Andi 70%, companion 30% | Lari / Menyerah / Terluka |
| **Lari** | Takut ≥ 70, atau HP < 30% (peluang 60%) | Lari ke titik mundur 60 m menjauhi Andi; 30 s kemudian kembali ke Patroli | Patroli |
| **Seret Teman** | Teman "Terluka" dalam 20 m & Takut < 70 | Memegang pergelangan teman, berjalan mundur 1,2 m/s ke titik mundur 40 m | Lari / Serang (bila diserang) |
| **Terluka** | HP < 15% (peluang 50%) | Merangkak lambat, merintih | Mati / diseret |
| **Menyerah** | HP < 25%, Takut > 60, tanpa teman dalam 15 m | Berlutut, tangan terangkat 6 s; prompt **"AMPUNI"** | Lari (diampuni) / Mati |
| **Mati** | HP 0 | Sesuai GoreLevel (14 §2): ragdoll + mayat bertahan, atau larut menjadi abu hitam 4 s; drop item (GELEDAH) | — |
| **Ritual (malam di Desa)** | 19:00–04:00, tanpa gangguan | Menari mengelilingi api ritual (270,100) | Waspada |
- **Takut** naik: teman terbunuh di dekatnya +15, kena parry +20, kena api +10, Obor Tengkorak menyala dalam 25 m +40, suar +30, pohon tumbang dalam 10 m +25. Turun 5/s saat tidak terancam.
- **Mengampuni**: Sekte berdiri, membungkuk, lari. Agresi −4, Sanity +3, statistik `SekteDiampuni` +1. Menyerang yang sedang menyerah: Sanity −4.

## 7. Agresi Sekte & Penyerbuan Kemah
- **Kemarahan Sekte** 0–100, mulai 10: +3 per hari bertahan, +2 per Sekte dibunuh, +5 per **Totem Topeng** dihancurkan (6 totem: 3 di Desa, 3 di Sawit), −4 per pengampunan, −2 per hari tanpa membunuh. Pengali kesulitan.
- **Kemah** = Saung/Api Unggun milik pemain terdekat yang terdaftar.
- Setiap malam (≥ Malam 2) pada waktu acak 21:00–02:00: peluang raid = clamp(Agresi/100 + 0,2, 0, 0,95).
| Agresi | Jumlah | Komposisi |
|---|---|---|
| 0–19 | 0 (hanya 1–2 pengintai saat senja) | — |
| 20–39 | 2–3 | AbdiSekte |
| 40–59 | 4–5 | + AbdiWanita |
| 60–79 | 6–7 | + PemimpinPatroli, AbdiLumpur |
| 80–100 | 8 | + AbdiButo |
| Malam Darah | + Pocong ×2–4 | — |
- Pola raid: muncul dari tepi hutan berkelompok → berhenti 30 m → Isyarat → menyerang struktur & penghuni. Damage ke struktur: tebasan 8/pukul, alu Buto 25/pukul. Pagar Runcing: 15 damage/detik + dorong ke Sekte yang menempel. Obor Tengkorak menyala: raid berhenti di 25 m selama 30 s, peluang batal 50%.
- Malam 2: raid uji pertama dipaksa (2–3 pengintai) bila Agresi < 20.

## 8. Hantu — Perilaku
### Pocong Hutan / Gua
Diam bergoyang bila jauh → bila Andi ≤ 30 m: melompat mendekat → pada 6 m: **terjang** (ancang-ancang jongkok 0,5 s) → pulih 1,2 s (rentan). Api ×2. Menjauh dari asap Kemenyan (20 m, 60 s) & Obor Tengkorak (15 m). Pocong Gua: menyergap dengan jatuh dari langit-langit di zona pemicu.
### Kuntilanak Beringin
Bertengger (tawa 3D periodik 20–40 s) → Andi ≤ 35 m: melayang mengitari 10 m sambil tertawa → **jeritan** (setrum 1 s, Sanity −5, jeda 15 s) → **tukikan** (ancang-ancang 0,6 s, 12 m/s) → rentan 1,5 s. Disorot Senter terus-menerus 2 s → goyah & terlihat jelas. Setelah kehilangan 50% HP → menghilang, muncul di belakang Andi 8 s kemudian. HP 0 → menjerit, menjadi kabut, kembali malam berikutnya. Malam Darah: mendatangi kemah.
### Banaspati
Mengorbit 6 m lalu menerjang lurus. Tidak muncul saat hujan. Padam oleh: lemparan `BotolAirBersih`, masuk air, hujan. Mengenai struktur kayu = terbakar (−5 HP/s selama 20 s; hujan memadamkan). Membakar Andi → lompat ke air memadamkan.
### Jenglot (lanjutan)
Kawanan 4–6, merangkak cepat, mengepung; takut cahaya senter (mundur 3 s).

## 9. Spawner (`EnemyDirector`)
- Malam di permukaan: target Pocong Hutan 2 (Normal) dalam 60–120 m dari Andi di bioma hutan, muncul di luar pandangan (di belakang > 100° atau terhalang), maks 4; Malam Darah ×2. Kuntilanak: 1, terikat 80 m dari (0,0) (Malam Darah: bebas). Banaspati: 1–2 di Sawit (tanpa hujan).
- Siang: tidak ada hantu di permukaan (sisa Malam Darah larut pukul 05:00).
- Sekte siang: Desa 4–6 (patroli + 1 pemimpin), Sawit 3–4 (+ AbdiLumpur di rawa), pengintai 1–2 mendekati kemah mulai Hari 1 15:00.
- Despawn: > 150 m dan tidak terlihat 10 s. **Maks total musuh aktif 12** (antrian spawn).
- Gua (tetap): UG1 — R1 1 Pocong Gua (jatuh saat Lore #4 dibaca), R4 2 penjaga, 1 penyergap saat kembali di T2. UG2 — pelarian (bawah). UG3 — K1: 4 Jenglot (atau 2 Pocong Gua bila Jenglot belum dibuat), K3 boss.
- **Pelarian Bunker (UG2 R4)**: mengambil Paku 2 → alarm 40 s, lampu merah berkedip, dinding R4 runtuh: 3 AbdiSekte masuk; 2 Pocong Gua dari koridor C2. Tangga darurat X1 langsung terbuka.

## 10. Boss — Genderuwo (Kubah Akar Agung)
| Fase | HP | Serangan (telegraf) |
|---|---|---|
| 1 | 100–60% | **Hantaman Tanah** (kedua tinju terangkat + auman 1,0 s → cincin debu radius 6 m, 35); **Sapuan** (0,6 s, busur 2,5 m, 25); **Lempar Akar** (1,0 s, proyektil 20 m, 20) |
| 2 | 60–25% | Ki Rogo memanggil 2 Pocong Gua tiap 30 s; **Amukan** (lari lurus 9 m/s selama 3 s; menabrak pilar/dinding → pingsan 2,5 s, **punggung bercahaya** = titik lemah ×2) |
| 3 | < 25% | Mematikan 4 dari 8 tungku (gelap — butuh senter/obor), auman tiap 15 s (Sanity −5), kecepatan +20% |
- Ki Rogo kebal di panggung (perisai akar) selama pertarungan, lalu kabur (cutscene 04 §7.5).
- **Kesulitan Damai**: tanpa boss — "Teka-teki Akar": nyalakan 4 tungku sesuai urutan simbol I–II–III–IV yang terukir di pilar.

## 11. Siege 60 Detik (Altar Beringin)
Spawn di cincin 35–45 m dari altar dari 6 titik merata; antrian bila > 12 aktif.
| Detik | Gelombang |
|---|---|
| 0 | 4 AbdiSekte + 1 PemimpinPatroli |
| 15 | 3 PocongHutan |
| 25 | 2 AbdiWanita + 1 AbdiButo |
| 35 | Kuntilanak mengamuk (tukikan tiap 8 s) |
| 40 | **Bayangan Ki Rogo** muncul di (0,10): gelombang tongkat tiap 6 s, memanggil 2 Abdi tiap 10 s |
| 45 | 2 Banaspati + 2 PocongHutan |
| 60 | Semua larut menjadi abu (cutscene 04 §7.6) |
- Kang Deden: bertarung dengan golok di dekat Andi (tidak bersembunyi selama siege). Ningsih (Trust ≥ 70, bersenjata): menembak busur & suar.
- Andi tidak bisa tidur/menyimpan selama siege. Jika Andi pingsan saat siege → muat ulang simpanan otomatis "sebelum ritual".

## 12. Lingkungan sebagai Senjata
- **Pohon tumbang**: segmen batang yang bergerak dengan kecepatan ujung > 4 m/s → 150 damage ke musuh yang dilindas (Andi 40, companion 30). Takut Sekte di sekitar +25.
- **Perangkap Ayun Bambu**: radius picu 1,5 m, 200 damage, setel ulang manual "SETEL ULANG".
- **Pagar Bambu Runcing**: lihat §7.
- **Api**: Bom Bambu & Panah Api membakar semak kering (Kemarau) radius kecil 4 m, padam 10 s.
