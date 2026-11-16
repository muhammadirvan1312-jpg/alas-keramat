# 09 — UI/UX LAYAR SENTUH HP, PARAMETER GERAK, SEMUA LAYAR & IKON PROSEDURAL
Dibaca pada tahap T3 (HUD & kontrol), T5 (tikar), T6 (buku panduan), T8 (buku catatan), T11 (menu & pengaturan).

---

## 1. Pengaturan Canvas
- `Canvas_HUD` (Screen Space Overlay, sort 10) dan `Canvas_Menus` (sort 20). `CanvasScaler`: Scale With Screen Size, referensi **1920×1080**, `matchWidthOrHeight = 1` (ikut tinggi, karena lebar HP bervariasi 16:9–21:9).
- Semua elemen HUD di dalam panel `SafeArea` yang mengikuti `Screen.safeArea` (poni/kamera berlubang).
- Orientasi: hanya Landscape Left/Right. `Screen.sleepTimeout = SleepTimeout.NeverSleep` saat bermain.
- Font: `GetSafeFont()`; warna teks utama `#F2EFE6`, sekunder `#BFB8A8`, emas `#F2C14E`, bahaya `#E5533D`. Semua teks HUD diberi `Outline` hitam α 0,8 jarak (1,5, −1,5).
- **Koreksi v1**: ukuran tombol v1 (110/90/80/75 px) terlalu kecil di HP (±5–7 mm). Ukuran baru di bawah memberi target sentuh ≥ 9 mm; ada pengaturan "Ukuran Tombol" 80–130%.

## 2. Tata Letak HUD (koordinat referensi 1920×1080)
### 2.1 Kiri Atas — Vitals & Cuaca (anchor kiri-atas, posisi (40, −32), lebar 440)
| Elemen | Ukuran | Warna isi / latar | Catatan |
|---|---|---|---|
| 10 kotak Zirah | 30×12, jarak 6 | aktif: warna jenis zirah (Daun `#7FA650`, Bambu `#C2A766`, Tulang `#E8DCC0`, Baja `#9EA3A6`, Emas `#E8C766`); kosong `#FFFFFF` α 0,12 | Di atas bar darah; kotak berkedip saat menyerap damage |
| Bar 1 Darah | 360×22 | `#E53935` di atas `#5A1414` | Bagian di atas `TargetHealthCeiling` diarsir gelap `#2A0A0A` bergaris miring |
| Bar 2 Stamina | 360×16 | `#7FD3FF` di atas `#4A4F55` | Bagian terpotong ColdWet = biru beku `#2E6EA8` bergaris es |
| Bar 3 Kenyang | 260×10 | `#F28C28` | Ikon garpu 18 px di kiri |
| Bar 4 Haus | 260×10 | `#2EC4C9` | Ikon tetes |
| Bar 5 Kewarasan | 260×10 | `#9B59D0` berpendar (α 0,6↔1,0, 0,5 Hz) | Ikon mata |
| Baris info | tinggi 32, font 26 | putih | `HARI 1 │ 17:45 (MAGHRIB) │ [ikon cuaca] Hujan Badai │ [ikon termometer] Basah 45%` — ikon = `Image` sprite IconPainter, bukan emoji |
Bar berkedip merah bila < 20%. Bar beranimasi halus (lerp 6/s). Teks hanya diperbarui saat nilai berubah.
### 2.2 Kanan Atas — Radar GPS & Misi (anchor kanan-atas, (−40, −32))
- Lingkaran Ø 300: latar `#0B1A0F` α 0,55, cincin `#3CFF7A` α 0,8, garis silang samar, penanda "U" (utara) di cincin yang ikut berputar. Jangkauan 150 m (ketuk radar = 300 m).
- Ikon 30 px: Andi = panah putih di tengah (radar berputar mengikuti yaw kamera); **K** (lingkaran hijau + huruf), **N** (lingkaran krem + huruf); **G1/G2/G3** (ikon lengkung gua; abu bila belum ditemukan, emas bila misi aktif); **Beringin** (sprite bintang, bukan karakter ★); **Saung/Kemah** (sprite rumah); misi (berlian emas); musuh (titik merah) hanya bila ditandai Menara Pengintai/indra Ningsih; "?" & "X" bunker (lihat 03). Di luar jangkauan: dijepit ke tepi + panah kecil.
- Tanpa `GPSHandset`: radar hanya menampilkan cincin + penanda U.
- Di bawah radar: **Misi Aktif** (lebar 440, rata kanan, font 28 tebal `#F2C14E`, maks 2 baris + progres), lalu bar HP mini companion bila terluka.
### 2.3 Kiri Bawah — Joystick Mengambang
- Zona aktif: 45% kiri layar, 70% bawah. Basis Ø 300, kenop Ø 130, α 0,35 saat diam; muncul di titik sentuh; radius gerak 110 px.
- **Sprint**: kenop > 95% radius dan sudut ≤ 30° dari atas selama 0,25 s → cincin basis jadi oranye `#FF9A3C` + label "LARI".
### 2.4 Area Geser Kamera
Seluruh area kanan yang tidak tertutup tombol. Sentuhan pertama di area ini = putar kamera (multi-touch didukung bersamaan dengan joystick).
### 2.5 Kanan Bawah — Kluster Tombol Aksi (anchor kanan-bawah)
| Tombol | Ø | Posisi | Warna | Isi |
|---|---|---|---|---|
| TEBAS/TEMBAK | 200 | (−150, 170) | `#B5462E` merah bata | Ikon senjata aktif + label |
| TANGKIS | 140 | (−330, 110) | `#4A6F8A` biru baja | Ikon perisai; ketuk ganda = holster |
| TANGAN (kontekstual) | 160 | (−320, 300) | `#E0B33A` kuning emas | Redup α 0,35 tanpa target; terang + berdenyut saat ada objek di tengah bidikan; teks di atasnya (font 26 tebal) mis. "AMBIL KAYU", "MINUM KELAPA", "PASANG TIANG", "BELAH PAPAN", "SERUT RUNCING", "PERINTAH KANG DEDEN", "GALI BUNKER" |
| LEMPAR | 120 | (−130, 380) | `#6B5A45` | Lempar kayu/tombak/bom/botol |
| LOMPAT | 120 | (−470, 70) | `#3E4A3A` | — |
| JONGKOK | 120 | (−480, 230) | `#3E4A3A` | Toggle; ikon berubah saat di semak (mata tertutup = tersembunyi) |
| Cincin Mode Bangun | 5 × Ø 70 | busur mengelilingi TANGAN | `#2A2A2A` α 0,7 | Muncul hanya saat memanggul kayu/papan/bambu |
- Aksi tahan (gali, bongkar, minum lama): cincin progres di sekeliling tombol TANGAN.
- Mode tangan kidal: kluster dicerminkan ke kiri, joystick ke kanan.
### 2.6 Bawah Tengah — Sabuk Cepat
8 slot 96×96 (jarak 10): `[1 Golok] [2 Tombak] [3 Busur] [4 Cangkul/Zipline] │ [Tangan Kiri: Obor/Senter/GPS — ketuk untuk berganti] │ [Tas → Tikar] [Buku → Panduan]`. Slot 1–4 bisa diatur dari tikar ("PASANG KE SLOT"). Slot aktif berbingkai emas; jumlah amunisi di sudut kanan bawah slot.
### 2.7 Elemen Lain
- Atas tengah: tombol **JEDA** Ø 70 dan **CATATAN** Ø 70 (Jurnal & Resep; perintah Deden tetap lewat tombol tangan).
- Crosshair: titik 6 px putih α 0,7 → lingkaran 28 px saat ada objek interaksi; tanda kena (X putih) 0,15 s.
- Indikator arah damage: busur merah di radius 160 px dari tengah, 0,8 s.
- Toast item: di bawah tombol JEDA, "+2 Ranting" (maks 4 baris, pudar 2,5 s), font 24.
- Subtitle: bawah tengah di atas sabuk, lebar 1200, font 34, latar hitam α 0,4.
- Siege: bar 600×20 + "BERTAHAN 00:42" di atas tengah. Boss: bar 900×18 + nama.
- HP < 25: vignette merah berdenyut + detak jantung.

## 3. Parameter Gerak Andi (`AndiFPSController`)
| Parameter | Nilai |
|---|---|
| Jalan maks | 3,4 m/s (linier dari magnitudo joystick 0,2–0,95) |
| Sprint | 6,2 m/s |
| Jongkok | 1,8 m/s (tinggi kapsul 1,1) |
| Mundur / menyamping | ×0,7 / ×0,9 |
| Akselerasi / perlambatan | 18 / 22 m/s² |
| Lompat | v 4,8 m/s (≈ 1,2 m), gravitasi 18 m/s², coyote time 0,12 s |
| CharacterController | tinggi 1,8, radius 0,32, slope limit 42°, step offset 0,35 |
| Damage jatuh | > 6 m: 7 × (tinggi − 6); > 18 m: mati |
| Berenang | Kedalaman air > 1,3 m. 2,2 m/s (cepat 3,2 m/s, stamina). Menyelam: menunduk + maju. Napas 25 s (Alat Selam 240 s); habis → −8 HP/s |
| Head bob | vertikal 0,035 m, lateral 0,02 m (bisa dimatikan) |
| Kamera | FOV 70 (60–85), sensitivitas 0,12°/px horizontal, 0,10°/px vertikal (×0,5–×2), balik Y opsional, pitch −80°..+80° |
| Interaksi | Raycast dari tengah kamera 2,5 m, SphereCast radius 0,15, layer `Interactable`, `Tree`, `LogPhysics`, `Structure` |

## 4. Daftar Layar
1. **Loading**: hitam, judul "ALAS KERAMAT" (96), subjudul "Sons of Nusantara" (36), tips bergiliran, bar progres (pembuatan audio & inisialisasi).
2. **Menu Utama**: latar = adegan langsung (kamera orbit lambat mengelilingi Beringin saat Maghrib berkabut). Tombol kolom kiri: MULAI BARU (→ pilih Damai/Normal/Sulit + slot 1–3), LANJUTKAN, MUAT, PENGATURAN, CARA BERMAIN, KREDIT, KELUAR (Android).
3. **Jeda**: LANJUTKAN, PENGATURAN, CARA BERMAIN, KELUAR KE MENU (peringatan: progres sejak simpanan terakhir hilang).
4. **Pengaturan** (disimpan `PlayerPrefs`):
   - Grafis: Preset Rendah/Sedang/Tinggi, Jarak Pandang, Bayangan, Kepadatan Semak, Target FPS 30/60, Render Scale.
   - Kontrol: Sensitivitas X/Y, Balik Y, Ukuran Tombol 80–130%, Transparansi Tombol, Tangan Kidal, Bantuan Bidik, Getar.
   - Audio: Master, Efek, Ambien, Musik.
   - Tampilan: FOV, Head Bob, Ukuran Subtitle, Angka Damage.
   - Permainan: Pohon Tumbuh Lagi, Jeda saat Menu Terbuka (default ON).
   - **v3**: Gore (Mati/Sedang/Penuh), Intensitas Horor (Ringan/Normal/Ekstrem), Headset (binaural), Kalibrasi Kecerahan, Mode Imersif, Prompt di Dunia, Meter Suara — lihat 13 §2.3 & §6, 14 §2, 17 §8.
5. **Tikar Pandan 3D** (§5). 6. **Buku Panduan 3D** (07 §7). 7. **Buku Catatan** (§6). 8. **Pembaca Lore** (§7). 9. **Peta** (§9).
10. **Pingsan/Mati**, **Pilihan Takdir**, **Statistik**, **Kredit** (04 §7–8).
11. **Cara Bermain**: 6 kartu bergambar ikon (Gerak, Tempur, Tebang & Panggul, Bangun, Tikar & Rakit, Teman).

## 5. Tikar Pandan 3D
- Panggung tersembunyi di (0, 500, 0) pada layer `InventoryStage` dengan 1 directional light khusus (culling mask hanya layer itu). Kamera overlay URP (camera stacking) pitch 55° melihat tikar 2,4×1,6 m. Dunia di belakang diredupkan panel UI hitam α 0,5.
- Tikar: tekstur anyaman pandan (01 §7, 02 §1.5) warna `#C9B07A`/`#A88A55`, tepi jahitan merah.
- Zona: Senjata (kiri atas), Alat (kanan atas), Bahan (kiri), Makanan (kanan), Zirah (kiri bawah), Kunci Cerita (kanan bawah); **Lingkaran Rakit** bersulam di tengah (Ø 0,5 m).
- Item = model 3D mini dari `ProceduralAssetFactory` (skala seragam ±0,15 m), label jumlah (UI mengikuti posisi layar).
- Ketuk item → panel info kanan (lebar 520): nama (36), deskripsi (26), tombol [PAKAI/MAKAN/MINUM] [PASANG KE SLOT 1–4] [TARUH DI RAKIT] [BUANG]. Seret item ke lingkaran juga bisa.
- Resep cocok → model hasil berputar di atas lingkaran + tombol besar **RAKIT** (Ø 160 emas). Suara akor bonang.
- Tutup: tombol X / tombol Tas lagi.

## 6. Buku Catatan (Notepad)
- Model 3D buku saku berspiral di atas dipegang di depan kamera; halaman krem bergaris biru tipis; tulisan gaya tangan (font miring).
- Tab samping: **Umum · Kumpulkan · Kemah · Jurnal · Resep**. Halaman perintah berisi daftar baris perintah (08 §2.3) dengan kotak centang; perintah terkunci tampil dicoret abu "(belum bisa)".
- Ketuk perintah → animasi lingkaran tangan mengitari baris (sprite) 0,4 s → buku ditutup → reaksi Deden.

## 7. Pembaca Lore
- Lembar 1000×700 di tengah. Gaya per jenis: **Kertas** (tekstur krem `#E9DFC4` bernoda), **Papan Kayu** (tekstur kayu, teks merah tua `#7A1A14` kapital), **Kain** (tekstur kain kuning), **Arsip Ketik** (kertas putih kekuningan, teks gaya mesin ketik).
- Judul tebal 36, isi 30, jarak baris 1,15. Tombol TUTUP. Teks baru disimpan ke Jurnal + toast "Jurnal diperbarui".

## 8. Ikon Prosedural (`IconPainter`)
- Setiap ikon = `Texture2D` 128×128 RGBA, digambar dengan primitif SDF (lingkaran, kotak bulat, garis tebal, poligon, busur), glyph putih di latar transparan, tepi anti-alias 1,5 px; diwarnai lewat `Image.color`. Disimpan Builder ke `Assets/Generated/UI/Icons/*.png` dan diimpor sebagai Sprite.
- Daftar minimum: hati, petir (stamina), garpu, tetes, mata, perisai, matahari, bulan, awan, hujan, badai (awan+petir), kabut (3 garis), termometer, rumah, bintang, gua (lengkung), berlian, panah, lencana K, lencana N, kayu, ranting, batu, daun, serat, tali, kain, bambu, papan, tulang, kulit, tengkorak, bulu, getah, kemenyan, roda gigi, baterai, kaleng, arang, botol, mesiu, golok, kapak, tombak, busur, anak panah, ketapel, pistol suar, bedil, keris, cangkul, peluncur tali, bom, obor, senter, GPS, buku, buku catatan, korek, alat selam, kartu, peta, zirah, paku, kelapa, pisang, jamur, daun sirih, umbi, ikan, daging, telur, ransum, mangkuk sup, botol jamu, X (tutup), jeda, pengaturan, tas, tangan, lompat, jongkok, lempar, plus/minus.
- Tekstur panel UI (kertas, kayu, kain, anyaman) juga dibuat prosedural 256×256.

## 9. Peta Kertas
- Tekstur 1024×1024 dibuat dari `SampleHeight` & `GetBiome`: warna kertas `#E6D7B0`, kontur tiap 10 m (`#8A6A45` α 0,35), air biru pudar, nama zona tulisan tangan (teks UI di atas gambar).
- Kabut eksplorasi: area belum dikunjungi diarsir (bitmask 64×64 tersimpan di save; terbuka radius 60 m di sekitar posisi yang pernah dilalui).
- Penanda: Andi, K, N, kemah, gua yang ditemukan, misi aktif. Geser untuk menggeser; tombol + / − untuk zoom (1×–3×).

## 10. Getar & Aksesibilitas
- Getar (`Handheld.Vibrate()` hanya di `#if UNITY_ANDROID || UNITY_IOS`): kena damage ≥ 20, pohon tumbang ≤ 15 m, siege dimulai.
- Subtitle selalu aktif untuk monolog; ukuran 28–44.
- Semua informasi warna juga punya bentuk/ikon (bukan warna saja).
