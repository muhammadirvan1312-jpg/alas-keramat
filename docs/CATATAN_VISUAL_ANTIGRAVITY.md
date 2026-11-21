# Catatan Visual Lengkap untuk Antigravity

Dokumen ini ditulis 22 Nov 2026 setelah APK `uji-20261121-1830` (279 MB, PR #44). Tujuannya satu: lompatan visual yang **terasa jelas** di HP, mendekati suasana *Sons of the Forest*, tanpa membuat game patah-patah. Baca bersama `docs/HANDOVER_ANTIGRAVITY_2.md` (status, repo aset privat, pelajaran) dan `docs/gdd/19_VISUAL_V4_ASET_CC0_DAN_MOCAP.md` (rubrik foto §7, money shot §8, anggaran HP §9).

---

## 0. Aturan main (wajib)

1. **Bukti foto, bukan klaim.** Setiap perbaikan visual harus punya foto CI *sebelum* dan *sesudah* dari sudut yang sama. Nilai dengan rubrik `docs/gdd/19` §7 (0–10 per kriteria), tulis skornya dan 1 kalimat alasan di `docs/PROGRESS.md`.
2. **Satu PR = satu tema** (misalnya "air", "karakter utama"). Lebih mudah dinilai dan dibatalkan. Build CI ±75 menit, foto hanya muncul pada event PR.
3. **Anggaran HP** mengikuti tabel `docs/gdd/20` §1 (v5): Ultra ≤ 900 rb segitiga terlihat, ≤ 400 batch (SRP Batcher + instancing), tekstur ≤ 1,2 GB dengan streaming mipmap; preset Tinggi/Sedang/Rendah turun bertahap (kaskade bayangan 4/2/1, jarak 150/90/60 m). Model HP pengguna belum diketahui — tanyakan dan sesuaikan default preset. Android ASTC 6×6, tekstur karakter/UI 4×4. Bayangan realtime hanya matahari/bulan + senter; lampu titik lain tanpa bayangan.
   Fitur mahal **wajib** ikut preset (`MenuScreens.AmbilPreset`) dan `PengawasPerforma` (turun otomatis bila FPS jatuh).
4. **Lisensi.** Hanya CC0 (Poly Haven, ambientCG, OpenGameArt CC0, Kenney), Mixamo (file mentah hanya di repo privat `alas-keramat-aset`), atau CC BY (wajib atribusi di panel Kredit `MenuScreens`). Jangan pernah commit FBX Mixamo/berlisensi ke repo publik.
5. `python3 Tools/cek_statis.py` harus 0 masalah (kata "Animator"/"NavMesh" terlarang, termasuk komentar; string ASCII saja).
6. Jangan menambah Always Included Shaders URP (build APK pernah gagal karena varian Lit).
7. Patuhi `AGENTS.md` (16 Hukum Besi): **tanpa renderer feature kustom / Shader Graph** (H1, H15) — efek layar hanya lewat Volume URP bawaan, shader HLSL `AK_*` sendiri, atau mesh/partikel; font hanya `GetSafeFont()` (H4); teror lewat `TerrorDirector`, darah lewat `GoreSystem` (H16).

---

## 1. Diagnosa jujur dari foto CI terakhir

| Area | Kondisi | Skor kira-kira |
|---|---|---|
| Lantai hutan siang | Serasah, semak & pohon scan, sorot matahari. Sudah bagus. | 7 |
| Gua lumut | Dinding kapur bertekstur, air memantul, jamur menyala. Terbaik saat ini. | 7,5 |
| Kubah akar boss | Atmosfer dapat, tapi lantai polos & akar seragam. | 6,5 |
| Hutan malam | Terlalu gelap di foto, karakter sulit dibaca, siluet lemah. | 5 |
| **Karakter utama** (Deden, Ningsih, Ki Rogo) | Masih MakeHuman: kaku, wajah datar, rambut kaku. **Titik terlemah.** | 3–4 |
| Musuh | 6 prefab sudah Mixamo, tapi beberapa sosok (wanita bergaun putih, pria telanjang dada) masih model lama. | 5 |
| Hewan | Ayam, rusa, kancil, ular sudah FBX; babi hutan, harimau, monyet, biawak, buaya, burung masih prosedural (kotak). | 4 |
| HUD | Ikon sudah ada; joystick & peta mini masih tampak kotak/hijau di foto (cek di HP, mungkin artefak kamera foto). | 6 |
| Air | Shader sudah bagus, belum ada buih tepi & riak interaksi. | 6 |
| Langit | HDRI siang/senja/malam ok, awan statis. | 6,5 |

**Urutan dampak terbesar:** karakter manusia → hewan → cahaya/atmosfer malam → vegetasi jauh (pop-in) → air → efek → UI.

---

## 2. P0 — Karakter manusia (dampak terbesar)

### 2.1 Ganti model utama dengan Mixamo
- FBX sudah diunduh di `C:\Users\muhammad.irvan\Downloads`: **Remy** (Deden), **Peasant Girl** (Ningsih), **Peasant Man** (Ki Rogo). `Remy.fbx` kemungkinan belum selesai terunduh (sempat `.crdownload`) — unduh ulang di mixamo.com (Format FBX Binary, Pose T-pose).
- Salin ke `alas-keramat-aset/Karakter/` (nama: `Remy.fbx`, `PeasantGirl.fbx`, `PeasantMan.fbx`), push ke repo privat.
- Kode: `Assets/Editor/BuilderSteps.Mixamo.cs` → `ModelUntuk(nama)` tambah peta `Deden→Remy`, `Ningsih→PeasantGirl`, `KiRogo→PeasantMan`; panggil `PasangDiAdegan<...>` untuk komponen NPC Deden/Ningsih/Ki Rogo dengan nama model (lihat cara Genderuwo).
- Runtime sudah siap: `Assets/Scripts/AI/AnimasiMixamo.Karakter.cs` (tinggi disamakan, pinggul dikunci, roboh pakai klip mati).
- **Andi** (orang pertama): cek apakah tubuh Andi terlihat di sinema/bayangan. Bila ya, pakai Mixamo pria lain atau Remy dengan material berbeda.

### 2.2 Material agar terasa "Indonesia"
- Mixamo wajah bule tidak bisa diubah bentuknya, tapi **warna kulit, rambut, pakaian** bisa:
  - Kulit: tint sawo matang (#B07A55 ~ #8E5B3C) lewat `MaterialPropertyBlock` atau salinan material di builder; smoothness 0,35; gunakan shader `AK_KulitManusia` bila mendukung SSS palsu.
  - Rambut: hitam (#141110), alpha clipping 0,35, matikan pantulan lingkungan (seperti perbaikan rambut Ningsih di `BuilderSteps.KarakterReal.cs`).
  - Pakaian: tekstur kain CC0 batik/lurik bila ada (ambientCG "Fabric"), atau tint coklat/tanah. Ki Rogo: sarung & ikat kepala (blangkon) bisa jadi mesh kecil terpisah ditempel ke tulang kepala.
- Kotoran & keringat: overlay detail map CC0 (ambientCG "Dirt") di material pakaian, intensitas 0,3.

### 2.3 Wajah & sinematografi
- Tambah shot foto CI **close-up wajah** tiap karakter (jarak 0,8 m, lampu kunci 45°, rim light biru tipis). Nilai kriteria "Karakter".
- Ikuti `docs/gdd/19` §6: dalam dialog, kamera sedikit samping, kedalaman bidang (DoF) ringan di preset Tinggi, bayangan menutupi separuh wajah — menyembunyikan kelemahan wajah.
- Kedip mata & gerak kepala kecil (procedural `LookAt` ke pemain) membuat karakter jauh lebih hidup; pasang pada tulang `mixamorig:Head` di `LateUpdate` setelah animasi.

### 2.4 Musuh yang tersisa
- Cari prefab/adegan untuk **wanita bergaun putih (kuntilanak)** dan **pria telanjang dada** di foto hutan malam; petakan: kuntilanak → `Zombiegirl` (rambut panjang, tint pucat, gaun putih: ganti material bajunya ke putih kusam), pria → `Parasite`/`Mutant`.
- Klip serang monster: gunakan klip zombie (`Zombie_Attack` bila ada di bank) untuk Peran Raksasa/Sekte, bukan pukulan manusia.
- Mata menyala samar (emisi merah/kuning 0,6) untuk Genderuwo/Maw di malam hari — sangat efektif untuk horor.

---

## 3. P1 — Hewan

- Sudah: ayam (jago/betina), rusa, kancil (rusa diperkecil), ular — `AI/ModelHewanAsli.cs`, `Editor/BuilderSteps.HewanAsli.cs`, aset di repo privat `Hewan/`.
- **Kelelawar** (`Hewan/kelelawar.fbx`, klip Bat_Flying/Idle/Attack/Die) belum dipasang: buat kawanan 6–15 ekor di mulut gua & dalam gua (`KehidupanAlam` punya pola burung), terbang berputar, kabur saat senter menyorot.
- Sisa prosedural: **babi hutan, harimau, monyet, biawak, buaya, burung**.
  - Sumber gratis realistis: Sketchfab **WildMesh_3D** (demo gratis Deer, Boar, Alligator, Buffalo; login Google), periksa lisensi tiap model (CC BY → atribusi).
  - OpenGameArt CC0 (tanpa login) gaya semi-realistis TinyWorlds: kelinci, tupai, burung.
  - Konversi .blend → FBX bisa dengan `pip install bpy` (Blender sebagai modul Python) — contoh skrip: lihat riwayat sesi (`ekspor_hewan.py`): impor, hapus objek bantu, `export_scene.fbx(bake_anim_use_all_actions=True, add_leaf_bones=False)`.
- Arah hadap & skala sudah otomatis di `ModelHewanAsli` (tulang Head/Skull, panjang collider). Verifikasi dengan foto close-up tiap hewan.

---

## 4. P1 — Cahaya & atmosfer (paling menentukan "mahal/murahan")

### 4.1 Sinar tajuk (god rays)
- URP mobile tidak punya volumetrik, dan renderer feature kustom dilarang (AGENTS H1/H15). Pakai **kartu sinar**: mesh quad panjang additive dengan shader HLSL sendiri `AK_SinarTajuk` (gradien lembut + noise bergulir, soft-fade dekat kamera via depth), ditempatkan di celah tajuk menghadap arah matahari, alpha mengikuti jam (06–10 & 15–17 paling kuat) dan cuaca (mati saat hujan). Spawn per sel di sekitar pemain dengan instancing (pola `SerasahHutan`). `docs/gdd/15` sudah menyebut "sinar tajuk" — ikuti spesifikasinya.
- Target foto "Bangkai heli 07:30" & "Lantai hutan".

### 4.2 Malam yang "gelap bermakna"
- Malam sekarang terlalu gelap di foto. Atur: ambient malam biru tua (#0C1424) intensitas 0,25, bulan arah jelas dengan bayangan lembut, eksposur auto tidak turun di bawah -1,2 EV.
- Senter (cookie sudah ada) harus menjadi sumber cahaya utama: jarak 25–30 m, sudut 55°, sedikit warna hangat, partikel debu terlihat di sorotan (partikel `DebuCahaya` aktif juga di malam dalam kerucut senter).
- Kabut tanah tipis di lembah & sungai saat malam/subuh (kartu kabut `AK_Berkabut` sudah ada; perbanyak di bioma TepiSungai/Rawa).
- Kunang-kunang sudah ada; tambahkan **mata hewan memantul** (titik emisi kecil) di semak saat disorot senter — trik horor murah dan efektif.

### 4.3 Bayangan & AO
- Jarak bayangan per preset: Rendah 25 m, Sedang 45 m, Tinggi 70 m, Ultra 100 m; 2 kaskade di Sedang, 4 di Tinggi.
- SSAO: fitur SSAO bawaan URP (bukan kustom) hanya di aset URP preset Tinggi/Ultra, intensitas 0,6, radius 0,25 — pastikan cek AGENTS H1 mengizinkan; bila ragu, pakai AO tanah palsu (decal gelap) saja.
- Kontak bayangan palsu: decal gelap lembut di bawah pohon/batu/karakter (blob shadow) untuk preset Rendah.

### 4.4 Grading per waktu & bioma
- Buat 4–6 LUT (`.cube`/Texture3D) CC0 atau buatan sendiri: subuh kebiruan, siang hijau hangat, sore emas, maghrib merah-ungu, malam biru-hijau, gua teal. `AKPascaProses` sudah mengatur eksposur/saturasi/suhu/vinyet — tambah `ColorLookup` dengan bobot per waktu.
- Bloom: threshold 1,1 agar hanya api, jamur, langit terang yang berpendar.
- Film grain halus 0,15 & chromatic aberration hanya saat panik/HP kritis.

---

## 5. P1 — Vegetasi & dunia jauh

- **Pop-in**: `SerasahHutan.JarakGambar = 56 m` → props muncul tiba-tiba. Naikkan per preset (Sedang 60, Tinggi 90) dan tambahkan *fade* (dither alpha clip atau skala tumbuh 0→1 dalam 8 m terakhir).
- **LOD crossfade** pohon (dithering) agar tidak "meletup".
- **Impostor/billboard** untuk pohon > 150 m (render 8 sudut ke atlas di builder) — hutan jauh terlihat lebat dengan biaya kecil.
- **Variasi warna per instance**: tint acak ±8% hue/kecerahan via `MaterialPropertyBlock` (pohon, semak, rumput) — menghilangkan kesan "copy-paste".
- **Lapisan bawah**: tambah pakis kecil, rumput tinggi, liana gantung, daun jatuh di jalur (Poly Haven: `fern_02`, `shrub_04`, `grass_medium_01`, ambientCG decals daun).
- **Angin**: perkuat goyang daun & semak saat badai (`_Angin` global sudah ada), daun beterbangan saat badai.

---

## 6. P2 — Terrain

- **Ubin berulang** terlihat di dataran luas: tambahkan *macro variation* (tekstur noise besar 64–128 m dikalikan ke albedo) atau campur 2 skala UV per layer.
- **Lereng**: batu otomatis di kemiringan > 35° (sudah ada sebagian), tambah **triplanar** untuk tebing agar tidak meregang.
- **Basah**: tepi sungai/rawa lebih gelap & mengkilap (smoothness naik) via splat basah; setelah hujan semua permukaan sedikit lebih gelap (sudah ada "basah dinamis", pastikan terlihat di foto hujan).
- **Jejak kaki & bekas seret** di lumpur (decal sementara 30 detik).

---

## 7. P2 — Air

- **Buih tepi**: di shader `AK_Air.shader`, pakai selisih kedalaman (scene depth − permukaan) < 0,4 m → buih putih noise bergerak. Preset Rendah: matikan.
- **Riak interaksi**: partikel cincin horizontal saat pemain/hewan/musuh melangkah di air + percikan saat lari/lompat; suara langkah basah.
- **Kaustik** di dasar sungai dangkal (tekstur kaustik bergulir diproyeksikan, CC0 atau dibuat prosedural).
- **Air terjun**: kartu air bergulir + kabut percikan + suara (rekaman CC0 OpenGameArt "waterfall").
- Laut: gelombang di pantai (busa garis pantai), warna toska dangkal → biru tua.

---

## 8. P2 — Langit & cuaca

- **Awan bergerak**: lapisan awan kedua (dome/plane dengan tekstur awan CC0) bergulir pelan; saat badai lebih tebal & gelap.
- **Bintang & bulan** malam: HDRI malam sudah ada; tambahkan bulan bercahaya (sprite) sejajar arah cahaya bulan, bintang berkelip halus.
- **Petir**: kilat menerangi seluruh langit 0,1 detik + bayangan tajam sesaat (lampu arah sementara), sinkron dengan suara guntur (jeda jarak).
- **Hujan**: garis hujan sudah ada, percikan tanah sudah ada; tambahkan tetes di lensa kamera (overlay) saat menengadah, genangan memantul.

---

## 9. P2 — Efek (VFX)

- **Api**: ganti api unggun/obor dengan flipbook asap & api CC0 (Kenney "Particle Pack" CC0, OpenGameArt fire spritesheet), percikan bara naik, cahaya berkedip (sudah sebagian).
- **Tebang & tebas**: serpihan kayu, daun berhamburan saat menebas semak, debu saat pohon tumbang.
- **Darah**: decal di tanah/batang pohon (sudah ada gore; tambah decal yang memudar), percikan ke kamera saat dekat.
- **Langkah**: debu kering di tanah, cipratan di lumpur.
- **Mati musuh**: asap hitam tipis untuk makhluk gaib (Genderuwo, kuntilanak) saat lenyap.

---

## 10. P3 — Bangunan & props

- Desa: sudah bilik bambu; tambah atap genteng tanah liat (ambientCG "RoofingTiles"), jendela kayu, kentongan, bale-bale, lampu teplok menyala di malam hari.
- Props Poly Haven CC0: tong kayu, peti, lentera, gerobak, jerami, keranjang rotan, pagar bambu (`bamboo` dari ambientCG).
- Interior rumah: lantai papan + tikar pandan, lemari kayu, kain jemuran.
- Bangkai heli: noda oli, kaca pecah, asap tipis berkepanjangan, kabel terjuntai.

---

## 11. P3 — UI & kamera orang pertama

- **Cek di HP**: joystick (`ui_joy_basis`/`ui_joy_kenop`) & peta mini (Mask bulat) — di foto CI tampak kotak (kemungkinan karena kamera foto tanpa stencil). Bila di HP juga kotak: ganti `Mask` dengan `RectMask2D` + sprite bulat, dan pastikan sprite UI tidak dikompres (lihat `AturImpor` sprite ≤ 256 → RGBA32).
- **Font**: AGENTS H4 mewajibkan `GetSafeFont()` (LegacyRuntime). Jangan ganti font kecuali pengguna setuju mengubah aturan; perbaiki keterbacaan lewat `Outline`/`Shadow`, ukuran, dan kontras.
- **Tangan Andi**: pastikan model tangan orang pertama bertekstur kulit sawo matang, ada lengan baju; senjata bergoyang saat berjalan (sway) dan menunduk saat lari.
- **Layar muat**: gambar latar (render money shot) + tips singkat.

---

## 12. Proses verifikasi visual (minta Antigravity membangun ini dulu, ±1 PR)

1. **Tur foto otomatis diperluas** di `NusantaraFoto` (lihat set money shot `docs/gdd/19` §8). Tambah shot:
   - close-up tiap musuh Mixamo (7), tiap hewan (5+), tiap karakter utama (3), wajah 0,8 m;
   - hutan siang 07:30 & 16:30 (sinar tajuk), hutan malam dengan senter, hujan badai, gua, desa senja, air terjun, pantai;
   - HUD di 3 kondisi (normal, HP kritis, terkena serangan).
2. **Bot uji main**: rekam 10–20 detik GIF (urutan frame) jalan → tebas → kena serang → buka tas → buka peta; simpan ke `ci-hasil/<cabang>/foto_kualitas/`.
3. **Emulator Android di CI** (opsional): `reactivecircus/android-emulator-runner`, pasang APK, tunggu 60 detik, ambil `adb logcat | grep -E "AK|Exception"` dan screenshot — menangkap crash saat mulai.
4. Setiap PR visual: tabel skor rubrik sebelum/sesudah di deskripsi PR.

---

## 13. Urutan PR yang disarankan

| PR | Isi | Perkiraan dampak |
|---|---|---|
| V1 | Tur foto close-up + bot GIF (alat ukur) | Dasar penilaian |
| V2 | Karakter utama Mixamo + material Indonesia + musuh tersisa | Sangat besar |
| V3 | Malam: ambient, senter, kabut tanah, mata memantul | Besar (horor) |
| V4 | Sinar tajuk + grading LUT per waktu + SSAO Tinggi | Besar |
| V5 | Vegetasi: pop-in fade, crossfade LOD, impostor, variasi warna | Besar |
| V6 | Kelelawar gua + hewan sisa (WildMesh/OpenGameArt) | Sedang–besar |
| V7 | Air: buih, riak, kaustik, air terjun | Sedang |
| V8 | Langit awan bergerak, bulan, petir, hujan lensa | Sedang |
| V9 | VFX api/tebang/darah/langkah | Sedang |
| V10 | Desa & props, UI font, layar muat | Sedang |

Target akhir rubrik: rata-rata ≥ 7,5 di semua money shot, karakter ≥ 7, malam ≥ 7, tanpa artefak (pink, pop-in kasar, tekstur meregang).
