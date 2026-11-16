# 14 — GORE & KEKERASAN: DARAH, LUKA, MUTILASI, RAGDOLL, MAYAT, DEKORASI MENGERIKAN
Dibaca pada tahap T7 (tempur) dan T9 (dunia hidup). Mengganti larangan gore di v2.
Konten ini fiksi untuk game horor dewasa. Ia menaikkan rating; lihat §1.

---

## 1. Rating & Gerbang Usia
- Klasifikasi Indonesia (IGRS, Permen 2/2024) menilai kategori konten termasuk **"darah, mutilasi, dan kanibalisme"** serta **"horor"**. Kelompok 15+ boleh menampilkan darah tetapi tidak mutilasi manusia → karena ada mutilasi, game ini dirilis sebagai **18+**. Pengaturan gore di dalam game TIDAK menurunkan rating.
- Isi kuesioner rating Google Play (IARC) dengan jujur: kekerasan realistis, darah, mutilasi, horor.
- Layar pertama kali dibuka: "Game ini mengandung kekerasan, darah, mutilasi, dan horor. Khusus 18 tahun ke atas." + tombol LANJUTKAN, lalu pilihan Gore (§2).
- **Batas mutlak** (tidak bisa diaktifkan): tidak ada kekerasan seksual; tidak ada interaksi menyiksa makhluk hidup yang tak berdaya; tidak ada gore pada sosok berwujud anak (Tuyul tidak bisa dilukai; Jenglot = makhluk mumi kering, hancur jadi debu); tidak ada penodaan simbol agama nyata; pemain tidak memakan manusia; Andi, Kang Deden, dan Ningsih tidak pernah dimutilasi.

## 2. Pengaturan Gore (`GoreLevel`: Mati = 0, Sedang = 1, Penuh = 2)
| Fitur | Mati | Sedang | Penuh |
|---|---|---|---|
| Partikel darah saat kena | Debu/abu | Ya | Ya, lebih banyak + kabut darah |
| Decal percikan, genangan, jejak seret | — | Ya | Ya |
| Luka menempel di tubuh | — | Ya | Ya |
| Mutilasi (anggota tubuh putus, kepala) | — | — | Ya |
| Ragdoll & mayat tertinggal | Larut jadi abu (v2) | Ya | Ya |
| Ambil potongan tubuh dari mayat | — | — | Ya |
| Dekorasi tubuh manusia di kamp/lab | Diganti bungkusan kain & tulang hewan | Diganti bungkusan kain & tulang | Penuh |
| Darah hitam hantu & mutan | Asap | Ya | Ya |
Default saat pertama: ditanyakan (pilihan disorot: Sedang). Bisa diubah kapan saja di Pengaturan → Tampilan.

## 3. Darah
### 3.1 Partikel
- **Semburan kena**: arah = arah pukulan dipantulkan ke permukaan + gravitasi; jumlah tetes = 6 + damage/4 (maks 40); ukuran 0,01–0,04 m; warna `#7A0A0A` → `#3E0505`; tetes yang menyentuh permukaan membuat decal kecil (maks 3 per semburan).
- **Kabut darah** (Penuh, tebasan berat): puff merah transparan 0,6 m, 0,4 s.
- **Semburan arteri** dari potongan anggota tubuh: berdenyut 1,2 Hz selama 4 s, melemah.
- **Tetesan** dari luka & senjata berdarah: 1 tetes / 0,6 s selama 10 s.
### 3.2 Decal (quad terproyeksi, tanpa Decal Renderer Feature)
- Raycast dari titik kena searah pukulan + gravitasi ke layer Ground/Structure; quad disejajarkan normal, offset 0,01 m, rotasi acak.
- Jenis: **Percikan** (0,3–1,2 m, 4 varian), **Genangan** (tumbuh di bawah mayat 0 → r 0,9 m dalam 6 s), **Jejak Seret** (strip memanjang saat Sekte menyeret tubuh / Andi berdarah merangkak), **Jejak Kaki Berdarah** (Andi menginjak genangan → 6 jejak memudar).
- Shader `AK/Blood` (15 §3): warna basah `#4A0505` smoothness 0,85 → mengering 120 s menjadi `#2A0A06` smoothness 0,3. Hujan mencuci ×4 lebih cepat.
- Maks 64 decal (ring buffer, yang tertua memudar 1 s); semua memudar setelah 10 menit nyata.
### 3.3 Darah di Senjata, Tangan & Layar
- Bilah senjata punya `_BloodAmount` (0–1): +0,35 per kena makhluk berdarah; turun ke 0 dalam 30 s atau langsung saat masuk air ("cuci").
- Lengan FP Andi tampak berdarah saat HP < 40 (overlay di atlas lengan).
- Kena dari jarak ≤ 1,5 m oleh luka berdarah: tetes darah di tepi layar (UI overlay prosedural) 1,5 s.
- Hantu & mutan: "darah" hitam pekat `#0A0A0A` dengan kilau hijau-hitam; Kuyang meneteskan merah gelap.

## 4. Luka, Zona Kena, & Status
- **Zona kena** (collider per sendi pada musuh humanoid): kepala ×1,8 (melee ×1,5), badan ×1,0, anggota gerak ×0,7.
- **Luka menempel**: quad kecil (cutout) menjadi anak tulang yang terkena, di titik kena, sejajar permukaan; jenis sayatan, tusukan, memar, luka bakar (atlas luka prosedural 4×2); maks 6 per karakter.
- **Pincang & lepas senjata**: kaki menerima > 40% HP maks → kecepatan ×0,6 dan pincang (gait asimetris); lengan kanan terluka parah → senjata terlepas, Sekte beralih mencakar/kabur.
- **Pendarahan Andi** (status baru): peluang 25% dari tebasan Sekte, gigitan Kuyang 100%, jatuh > 10 m 50%. −0,5 HP/s sampai dihentikan: **PerbanKain** (ID 72, resep R18: Kain ×1 → Perban ×2) menghentikan; DaunSirih memperlambat 50%; JamuHerbal menghentikan. Meninggalkan jejak tetes darah (Kuyang & babi hutan tertarik). Ikon tetes merah berdenyut di sebelah bar darah.

## 5. Mutilasi (Gore Penuh)
- **HP anggota tubuh** (musuh humanoid & mutan): Kepala 40, tiap Lengan 35, tiap Kaki 45 — terpisah dari HP utama.
- Anggota putus bila HP anggotanya habis karena **senjata tebas** (Golok, Kapak, Kapak Baja, Keris, Golok Api) ATAU pukulan maut berupa tebasan berat di zona itu. Senjata tumpul/panah tidak memutus (kecuali Bedil Locok ke kepala dari ≤ 8 m).
- **Teknik pada mesh rigid-skinned**:
  1. Skala tulang yang putus (mis. `L_Forearm`, `L_UpperArm`, `L_Thigh`, `Head`) menjadi 0,0001 → bagian itu & anak-anaknya hilang dari mesh tubuh.
  2. Spawn **prop anggota tubuh** (mesh terpisah yang dibuat `CharacterMeshBuilder` untuk tiap varian: Kepala, Lengan Atas+Bawah+Tangan, Lengan Bawah+Tangan, Paha+Betis+Kaki, Betis+Kaki) dengan Rigidbody & collider, kecepatan awal dari arah pukulan.
  3. Pasang **tutup potongan** (stump cap) di sendi induk pada tubuh dan di ujung potong prop: cakram berlapis — tepi kulit, lapisan lemak tipis `#E8D8A0`, otot `#7A1010`, tulang `#EDE6D6` di tengah dengan titik sumsum.
  4. Semburan arteri + genangan.
- **Penggal**: Tebasan Berat Penghabisan (setelah parry, 06 §1) pada Sekte/mutan dengan Gore Penuh memenggal kepala (nama tombol aksi saat itu: **"TEBASAN BERAT PENGGAL"**). Topeng Sekte terlempar terpisah.
- Sekte yang kehilangan lengan: menjerit, mundur memegangi potongan (state Lari/Terluka). Kaki putus: merangkak (state Terluka).
- **Hantu**: Pocong tidak putus — kafan robek memperlihatkan wajah busuk & rusuk berlendir hitam (mesh lapis dalam). Kuntilanak: gaun & rambut koyak. **Kuyang**: isi perutnya bisa ditebas → jatuh menggelepar (penghabisan). Mutan: anggota tubuh putus dengan darah hitam.

## 6. Mayat, Ragdoll, & Sisa
- **Ragdoll ringan** (Sedang/Penuh): saat mati, tambahkan saat runtime Rigidbody + CapsuleCollider + `CharacterJoint` ke 11 tulang (Pelvis, Chest, Head, 2 UpperArm, 2 Forearm, 2 Thigh, 2 Calf) dari konfigurasi pool; impuls dari pukulan maut. Batas sendi: lutut 0..−130°, siku 0..140°, leher ±45°, bahu ±80°, pinggul −30..+100°. Setelah 6 s diam → kinematic (pose dibekukan). Maks **4 ragdoll aktif**; lebih dari itu → animasi tumbang prosedural.
- **Mayat bertahan**: maks 12 di seluruh dunia (yang tertua & tidak terlihat dihapus). Pembusukan: Hari+1 kulit menggelap + lalat (partikel & dengung); Hari+2 → tumpukan tulang (pungut Tulang ×3, Tengkorak ×1).
- Sekte mengambil jenazah teman (menyeret, jejak seret, berlutut berduka 6 s) — 16 §4. Biawak & babi hutan memakan bangkai setelah 1 hari.
- Interaksi mayat: **GELEDAH** (semua level; ambil drop), **AMBIL TOPENG** (TopengSekte, ID 25), **POTONG** (Penuh; tahan 1,5 s dengan senjata tebas → PotonganTubuh ID 24).
- **Patung Ngeri** (blueprint B16): Ranting 6 + PotonganTubuh 2 + TopengSekte 1 (Gore Sedang/Mati: Tulang 4 + TopengSekte 1 + Ranting 6). Saat dinyalakan (Damar 1, 8 jam game): Takut Sekte +50 dalam 35 m, Sekte tidak mau masuk 20 m. Versi kuat dari Obor Tengkorak.

## 7. Dekorasi Mengerikan di Lokasi (versi Penuh; versi lain diganti bungkusan kain/tulang hewan)
| Lokasi | Dekorasi |
|---|---|
| Kampung Sekte Watu Ireng (16 §3) | Rak pengasap berisi anggota tubuh, tubuh terbungkus kain tergantung di pohon (kepompong), batu altar berlumur darah, tiang tengkorak, genangan & lalat |
| Desa — api ritual | Tubuh terbungkus kain tergantung di dahan beringin kecil, darah di tanah |
| Sawit | Mayat pekerja PT hangus di kabin ekskavator & tanah, helm kuning |
| Bunker lab | Meja otopsi dengan subjek terbuka (organ prosedural), tabung kaca berisi potongan tubuh dalam cairan hitam, tulisan darah "TOLONG" & "JANGAN BUKA", jejak seret ke ventilasi |
| UG1 | Kerangka ekspedisi, mayat kembung mengapung di Kolam Hitam |
| UG3 | Dinding akar dengan tubuh-tubuh tertanam (wajah membuka mata saat disorot) |
| Gua Tawanan | Kurungan dengan tulang & kain berdarah |

## 8. Aset Gore Prosedural
- Tekstur: 4 percikan (256 RGBA: tetes acak + coretan memanjang), genangan (noise radial), jejak seret, atlas luka 4×2, atlas organ, tutup potongan.
- Mesh: usus (tabung di sepanjang spline dengan tonjolan berkala), organ (gumpalan noise), tutup potongan (cakram 4 cincin), prop anggota tubuh per varian karakter.
- Shader basah (`AK/Blood`, `AK/Wet` di 15): smoothness 0,9, warna organ `#6A0F12` → `#9A3030`, sedikit emisi gelap agar tetap terbaca di malam.

## 9. Audio Gore (tambahan 10)
| Klip | Resep |
|---|---|
| Tebas daging basah | Debam 110 Hz + noise BP 1,2 kHz "kres" + lapisan basah (noise LP 800 AM 30 Hz) 0,15 s |
| Putus tulang | Deret impuls rapat (kretek) 0,08 s + noise BP 1,5 kHz + debam 70 Hz |
| Semburan | Noise LP 1 kHz berdenyut 1,2 Hz + gurgle (sinus 200–400 Hz acak) |
| Tetes | Plink 400–700 Hz τ 0,02 |
| Tubuh jatuh | Debam 60 Hz 0,25 s + kain |
| Langkah di genangan darah | Langkah tanah + lapisan basah |
| Lalat (loop) | Gigi gergaji 180–240 Hz vibrato 9 Hz, 3 lapis detune |

## 10. Batas Performa
Decal 64; luka 6 per karakter; ragdoll aktif 4; prop anggota tubuh 12 (tertua dihapus bila tidak terlihat); partikel darah maks 300 hidup; mayat 12.
