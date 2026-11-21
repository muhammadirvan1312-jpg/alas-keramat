# Catatan Visual Lengkap untuk Antigravity

> **Status & Perangkat Uji (konfirmasi 22 Nov 2026)**:
> - **Perangkat uji pengguna**: Entry-level (**RAM 4 GB**) -> default preset **`Rendah` 30 FPS** (`MenuScreens.RekomendasiPreset` sudah memilih `Rendah` untuk RAM < 6000 MB). Semua fitur visual & VFX wajib tunduk pada preset grafis dan `PengawasPerforma` agar HP 4 GB tetap stabil di 30 FPS.
> - **Baseline terbaru**: PR #46 (`imp-sotf-v2`, commit `6d45a2f`) sudah masuk `main` (karakter utama Mixamo `Remy`/`Peasant_Girl`/`Peasant_Man`, 12 hewan FBX PBR 2K, `KelelawarGua`, god rays `SinarTajuk`, `AK_Air.shader` buih/riak). Skor rubrik `docs/gdd/19 §7` baseline PR #46 dicatat di `docs/PROGRESS.md`.

Dokumen ini ditulis 22 Nov 2026 setelah APK `uji-20261121-1830` (279 MB, PR #44). Tujuannya satu: lompatan visual yang terasa jelas di HP, mendekati suasana *Sons of the Forest*, tanpa membuat game patah-patah. Baca bersama `docs/HANDOVER_ANTIGRAVITY_2.md` (status, repo aset privat, pelajaran) dan `docs/gdd/19_VISUAL_V4_ASET_CC0_DAN_MOCAP.md` (rubrik foto §7, money shot §8, anggaran HP §9).

## 0. Aturan main (wajib)
- **Bukti foto, bukan klaim.** Setiap perbaikan visual harus punya foto CI sebelum dan sesudah dari sudut yang sama. Nilai dengan rubrik `docs/gdd/19 §7` (0-10 per kriteria), tulis skornya dan 1 kalimat alasan di `docs/PROGRESS.md`.
- **Satu PR = satu tema** (misalnya "air", "karakter utama"). Lebih mudah dinilai dan dibatalkan. Build CI +-75 menit, foto hanya muncul pada event PR.
- **Anggaran HP** mengikuti tabel `docs/gdd/20 §1` (v5): Ultra <= 900 rb segitiga terlihat, <= 400 batch (SRP Batcher + instancing), tekstur <= 1,2 GB dengan streaming mipmap; preset Tinggi/Sedang/Rendah turun bertahap (kaskade bayangan 4/2/1, jarak 150/90/60 m). Perangkat uji utama pengguna: **Entry-level RAM 4 GB -> default preset `Rendah` 30 FPS**. Android ASTC 6x6, tekstur karakter/UI 4x4. Bayangan realtime hanya matahari/bulan + senter; lampu titik lain tanpa bayangan. Fitur mahal wajib ikut preset (`MenuScreens.AmbilPreset`) dan `PengawasPerforma` (turun otomatis bila FPS jatuh).
- **Lisensi.** Hanya CC0 (Poly Haven, ambientCG, OpenGameArt CC0, Kenney), Mixamo (file mentah hanya di repo privat `alas-keramat-aset`), atau CC BY (wajib atribusi di panel Kredit `MenuScreens`). Jangan pernah commit FBX Mixamo/berlisensi ke repo publik.
- `python3 Tools/cek_statis.py` harus 0 masalah (kata terlarang termasuk komentar; string ASCII saja).
- Jangan menambah Always Included Shaders URP (build APK pernah gagal karena varian Lit).
- Patuhi `AGENTS.md` (16 Hukum Besi): tanpa renderer feature kustom / Shader Graph (H1, H15) - efek layar hanya lewat Volume URP bawaan, shader HLSL `AK_*` sendiri, atau mesh/partikel; font hanya `GetSafeFont()` (H4); teror lewat `TerrorDirector`, darah lewat `GoreSystem` (H16).

## 1. Diagnosa jujur dari foto CI terakhir (pasca PR #44)
| Area | Kondisi | Skor kira-kira |
|---|---|---|
| Lantai hutan siang | Serasah, semak & pohon scan, sorot matahari. Sudah bagus. | 7 |
| Gua lumut | Dinding kapur bertekstur, air memantul, jamur menyala. Terbaik saat ini. | 7,5 |
| Kubah akar boss | Atmosfer dapat, tapi lantai polos & akar seragam. | 6,5 |
| Hutan malam | Terlalu gelap di foto, karakter sulit dibaca, siluet lemah. | 5 |
| Karakter utama (Deden, Ningsih, Ki Rogo) | Masih MakeHuman: kaku, wajah datar, rambut kaku. Titik terlemah. | 3-4 |
| Musuh | 6 prefab sudah Mixamo, tapi beberapa sosok (wanita bergaun putih, pria telanjang dada) masih model lama. | 5 |
| Hewan | Ayam, rusa, kancil, ular sudah FBX; babi hutan, harimau, monyet, biawak, buaya, burung masih prosedural (kotak). | 4 |
| HUD | Ikon sudah ada; joystick & peta mini masih tampak kotak/hijau di foto (cek di HP, mungkin artefak kamera foto). | 6 |
| Air | Shader sudah bagus, belum ada buih tepi & riak interaksi. | 6 |
| Langit | HDRI siang/senja/malam ok, awan statis. | 6,5 |

Urutan dampak terbesar: karakter manusia -> hewan -> cahaya/atmosfer malam -> vegetasi jauh (pop-in) -> air -> efek -> UI.

## 2. P0 - Karakter manusia (dampak terbesar)
### 2.1 Ganti model utama dengan Mixamo
- FBX: `Remy` (Deden), `Peasant_Girl` (Ningsih), `Peasant_Man` (Ki Rogo) di repo privat `alas-keramat-aset/Karakter/`.
- Kode: `Assets/Editor/BuilderSteps.Mixamo.cs` -> `ModelUntuk(nama)` peta `Deden->Remy`, `Ningsih->Peasant_Girl`, `KiRogo->Peasant_Man`; panggil `PasangDiAdegan<...>` untuk NPC Deden/Ningsih/Ki Rogo.
- Runtime: `Assets/Scripts/AI/AnimasiMixamo.Karakter.cs` (tinggi disamakan, pinggul dikunci, roboh pakai klip mati).
- Andi (orang pertama): cek apakah tubuh Andi terlihat di sinema/bayangan. Bila ya, pakai Mixamo pria lain atau Remy dengan material berbeda.

### 2.2 Material agar terasa "Indonesia"
- Kulit: tint sawo matang (`#B07A55` ~ `#8E5B3C`) lewat `MaterialPropertyBlock` atau salinan material di builder; smoothness 0,35; gunakan shader `AK_KulitManusia` bila mendukung SSS palsu.
- Rambut: hitam (`#141110`), alpha clipping 0,35, matikan pantulan lingkungan (seperti perbaikan rambut Ningsih di `BuilderSteps.KarakterReal.cs`).
- Pakaian: tekstur kain CC0 batik/lurik bila ada (ambientCG "Fabric"), atau tint coklat/tanah. Ki Rogo: sarung & ikat kepala (blangkon) bisa jadi mesh kecil terpisah ditempel ke tulang kepala.
- Kotoran & keringat: overlay detail map CC0 (ambientCG "Dirt") di material pakaian, intensitas 0,3.

### 2.3 Wajah & sinematografi
- Tambah shot foto CI close-up wajah tiap karakter (jarak 0,8 m, lampu kunci 45 derajat, rim light biru tipis). Nilai kriteria "Karakter".
- Ikuti `docs/gdd/19 §6`: dalam dialog, kamera sedikit samping, DoF ringan di preset Tinggi, bayangan menutupi separuh wajah.
- Kedip mata & gerak kepala kecil (procedural LookAt ke pemain) pada tulang `mixamorig:Head` di `LateUpdate` setelah animasi.

### 2.4 Musuh yang tersisa
- Cari prefab/adegan untuk wanita bergaun putih (kuntilanak) dan pria telanjang dada di foto hutan malam; petakan: kuntilanak -> `Zombiegirl` (rambut panjang, tint pucat, gaun putih kusam), pria -> `Parasite`/`Mutant`.
- Klip serang monster: gunakan klip zombie (`Zombie_Attack` bila ada di bank) untuk Peran Raksasa/Sekte.
- Mata menyala samar (emisi merah/kuning 0,6) untuk Genderuwo/Maw di malam hari.

## 3. P1 - Hewan
- Sudah FBX (PR #46): 12 hewan darat/amfibi (`AyamHutan`, `Rusa`, `Kancil`, `UlarPiton`, `UlarKobra`, `BabiHutan`, `Harimau`, `Buaya`, `Biawak`, `Monyet`, `Anjing`, `Kucing`) + `KelelawarGua` (`Hewan/kelelawar.fbx`).
- Lanjutan: kelelawar kabur saat senter menyorot; verifikasi dengan foto close-up tiap hewan.

## 4. P1 - Cahaya & atmosfer
### 4.1 Sinar tajuk (god rays)
- Kartu sinar mesh quad additive dengan shader HLSL sendiri `AK_SinarTajuk` (gradien lembut + noise bergulir, soft-fade dekat kamera via depth), alpha mengikuti jam (06-10 & 15-17 terkuat) & cuaca (mati saat hujan).
- Target foto "Bangkai heli 07:30" & "Lantai hutan".

### 4.2 Malam yang "gelap bermakna"
- Ambient malam biru tua (`#0C1424`) intensitas 0,25, bulan arah jelas dengan bayangan lembut, eksposur auto tidak turun di bawah -1,2 EV.
- Senter (cookie) sumber cahaya utama: jarak 25-30 m, sudut 55 derajat, warna agak hangat, partikel `DebuCahaya` aktif di malam dalam kerucut senter.
- Kabut tanah tipis di lembah & sungai saat malam/subuh (`AK_Berkabut` diperbanyak di bioma `TepiSungai`/`Rawa`).
- Mata hewan memantul di semak saat disorot senter.

### 4.3 Bayangan & AO
- Jarak bayangan per preset: Rendah 25 m (atau 0 m di HP entry-level 4 GB dengan blob shadow), Sedang 45 m, Tinggi 70 m, Ultra 100 m; 2 kaskade di Sedang, 4 di Tinggi.
- SSAO bawaan URP hanya di preset Tinggi/Ultra; kontak bayangan palsu (blob shadow) untuk preset Rendah.

### 4.4 Grading per waktu & bioma
- 4-6 LUT (`.cube`/`Texture3D`): subuh kebiruan, siang hijau hangat, sore emas, maghrib merah-ungu, malam biru-hijau, gua teal (`ColorLookup` di `AKPascaProses`).
- Bloom threshold 1,1; film grain halus 0,15 & chromatic aberration saat panik/HP kritis.

## 5. P1 - Vegetasi & dunia jauh
- `SerasahHutan.JarakGambar`: naikkan per preset (Sedang 60, Tinggi 90) + fade (dither alpha clip / skala tumbuh 0->1 dalam 8 m terakhir).
- LOD crossfade pohon (dithering), impostor/billboard > 150 m (atlas 8 sudut di builder).
- Variasi warna per instance +-8% hue/kecerahan via `MaterialPropertyBlock`.
- Lapisan bawah: pakis kecil, rumput tinggi, liana gantung, daun jatuh; perkuat goyang angin & daun beterbangan saat badai.

## 6. P2 - Terrain
- Macro variation (noise 64-128 m) atau 2 skala UV untuk hilangkan tiling.
- Triplanar tebing (> 35 derajat), basah dinamis tepi sungai/rawa & pasca-hujan, jejak kaki/seret di lumpur (decal 30 s).

## 7. P2 - Air
- Buih tepi berbasis depth (`AK_Air.shader`, matikan di preset Rendah).
- Riak interaksi cincin horizontal untuk pemain/hewan/musuh, kaustik dasar sungai dangkal, air terjun Z10 + audio waterfall, busa pantai & gradasi warna laut.

## 8. P2 - Langit & cuaca
- Lapisan awan bergerak (CC0), sprite bulan bercahaya + bintang berkelip, kilat menerangi langit + bayangan tajam sesaat sinkron guntur, overlay tetes hujan di lensa kamera saat menengadah.

## 9. P2 - Efek (VFX)
- Flipbook asap & api CC0 (Kenney / OpenGameArt) untuk api unggun & obor, serpihan tebang/daun semak, decal darah memudar + percikan kamera saat dekat, debu/cipratan langkah, asap hitam tipis saat makhluk gaib lenyap.

## 10. P3 - Bangunan & props
- Desa: atap genteng tanah liat (ambientCG), jendela kayu, kentongan, bale-bale, lampu teplok menyala malam.
- Props Poly Haven CC0 (tong, peti, lentera, gerobak, jerami, keranjang, pagar bambu), interior rumah lengkap, bangkai heli (oli, kaca pecah, asap tipis, kabel).

## 11. P3 - UI & kamera orang pertama
- Cek di HP: joystick & peta mini (`Mask` vs `RectMask2D`, `SpriteImportMode.Single`, dan sprite RGBA32 tak terkompresi).
- Font wajib `GetSafeFont()` (H4); perbaiki keterbacaan lewat `Outline`/Shadow, ukuran, kontras.
- Tangan Andi kulit sawo matang + lengan baju + sway jalan/lari; layar muat bergambar money shot + tips singkat.

## 12. Proses verifikasi visual (bangun ini dulu, +-1 PR - V1)
- Perluas tur foto otomatis di `NusantaraFoto` (lihat `docs/gdd/19 §8`):
  - close-up musuh Mixamo, hewan, karakter utama, wajah 0,8 m;
  - hutan siang 07:30 & 16:30 (sinar tajuk), hutan malam dengan senter, hujan badai, gua, desa senja, air terjun, pantai;
  - HUD di 3 kondisi (normal, HP kritis, terkena serangan).
- Bot uji main: rekam urutan frame jalan -> tebas -> kena serang -> buka tas -> buka peta menjadi GIF di `ci-hasil/<cabang>/foto_kualitas/`.
- Emulator Android di CI (opsional) untuk tangkap crash saat mulai.
- Setiap PR visual menyertakan tabel skor rubrik sebelum/sesudah di deskripsi PR.

## 13. Urutan PR yang disarankan (termasuk temuan audit foto & kode)
| PR | Isi | Perkiraan dampak |
|---|---|---|
| **V1** | Tur foto close-up + bot GIF + **fix `SpriteImportMode.Single` HUD/joystick/minimap/luka & kenop slider** | Alat ukur + langsung membenahi UI kotak |
| **V2** | Karakter utama Mixamo + shader `AK_KulitManusia` + tint Indonesia + LookAt/kedip + musuh tersisa + blob shadow preset Rendah | Sangat besar |
| **V3** | Cahaya & Malam: ground-bounce probe (hapus crushed blacks), shader `AK_SinarTajuk` Additive + guard `_AK_MatikanDepth`, malam senter/kabut/mata + **optimasi VRAM HP 4 GB (`ASTC_6x6` + streaming budget)** | Sangat besar (horor + performa 4 GB) |
| **V4** | Terrain & Vegetasi: tileSize anti-tiling, scale-fade `SerasahHutan`, variasi tint batang/daun, poles Gua Lumut & Kubah Boss | Besar |
| **V5** | Langit, Air & VFX: awan/bulan/bintang di `AK_Langit`, kaustik/air terjun/riak, LUT waktu/bioma, flipbook api/asap | Sedang-besar |

Target akhir rubrik: rata-rata >= 7,5 di semua money shot, karakter >= 7, malam >= 7, tanpa artefak (pink, pop-in kasar, tekstur meregang, HUD kotak).
