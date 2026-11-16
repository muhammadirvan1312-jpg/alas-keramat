# 19 — VISUAL v4: TEKSTUR FOTO CC0, MODEL CC0, MOTION-CAPTURE CMU, UNITY TERRAIN, RUBRIK FOTO
Dibaca pada T0-C (pengunduh), T2 (terrain & material), T4 (karakter & animasi), T5–T6 (props), setiap gerbang G2.
Dokumen ini MENGALAHKAN 15 bila bertentangan.

---

## 1. Diagnosis: Kenapa v3 Masih Akan Terlihat Kurang
| Penyebab | Dampak | Solusi v4 |
|---|---|---|
| Tekstur 100% prosedural | Permukaan terlihat "CG", pola berulang, kurang detail mikro | Tekstur **foto PBR CC0** (Poly Haven, ambientCG) untuk semua permukaan alam & bahan |
| Animasi 100% prosedural | Gerak kaku seperti robot | **Motion-capture CMU** diretarget ke rangka 20 sendi; prosedural hanya untuk lapisan (IK, napas, lihat) & makhluk gaib |
| Shader terrain kustom (`AK/Terrain`) | Risiko error tinggi lintas versi URP | **Unity Terrain bawaan** + shader URP `Terrain/Lit` (teruji, instancing, LOD otomatis) |
| Wajah prosedural | Wajah manusia paling mudah terlihat "palsu" | **Arah sinematografi**: wajah jarang tampil dekat & terang (§6) |
| Pantulan langit polos | Permukaan basah/air terlihat datar | Cubemap pantulan dari **HDRI CC0** per suasana |

## 2. Tumpukan Visual Baru (HP)
- **Terrain**: `TerrainData` 1000×1000 m, heightmap 1025² diisi dari `WorldTerrainGenerator.SampleHeight` (03 §2 tetap berlaku), 8 `TerrainLayer` (§3) dengan albedo + normal + mask map (R metallic 0, G AO, A smoothness = 1 − roughness), splatmap dilukis Builder dari `GetBiome` + lereng + jalan setapak + TrailMap. Shader URP `Universal Render Pipeline/Terrain/Lit` dengan **height-based blend**. `drawInstanced = true`, pixel error 5 (Sedang) / 8 (Rendah), basemap distance 120 m. Mulut gua memakai **terrain holes**. `AK/Terrain` dihapus dari daftar shader.
- **Vegetasi**: mesh pohon tetap prosedural (bentuk & tebang 4 kuadran), dengan **kulit kayu & atlas daun CC0**; semak/pakis/rumput instanced memakai `AK/Foliage` + tekstur daun CC0.
- **Props**: model CC0 Poly Haven (batu, bongkah, kayu lapuk, tunggul, tong, peti, perabot kayu, alat) bila polycount ≤ 8.000 segitiga (sebaran) atau ≤ 20.000 (props pahlawan). Selebihnya prosedural dengan material CC0.
- **Pantulan**: 4 HDRI CC0 (siang cerah, mendung, senja, malam) diimpor sebagai cubemap 256; `RenderSettings.customReflectionTexture` dipertukarkan halus sesuai jam. Langit tetap `AK/Sky` prosedural (siang-malam dinamis).
- **Karakter**: mesh & rangka prosedural tetap; sel atlas pakaian dikomposit dari **tekstur kain CC0** (katun, denim, kulit, kanvas, anyaman) diwarnai sesuai palet 02 §1.4. Kulit: prosedural + noise pori.
- **Air**: `AK/Water` + normal map air CC0.
- Post-processing, LUT, kabut tinggi, sinar tajuk: tetap (15 §5).

## 3. Slot Aset CC0 (diisi Codex lewat API, dikunci di `Tools/aset_cc0.lock.json`)
Resolusi **1K** (HP), format JPG/PNG. Peta: albedo/diffuse, normal (GL), roughness, AO, (height bila ada untuk blend).
| Slot | Pakai untuk | Sumber utama | Kata kunci / kategori |
|---|---|---|---|
| T_LantaiHutan | Terrain hutan | Poly Haven / ambientCG | forest ground, leaves ground |
| T_Rumput | Terrain rumput | ambientCG | grass |
| T_Tanah | Jalan setapak | ambientCG | ground dirt, mud dry |
| T_Lumpur | Rawa, tepi sungai | ambientCG | mud wet |
| T_BatuKapur | Tebing utara | Poly Haven | rock face, cliff, limestone |
| T_Pasir | Pantai | Poly Haven / ambientCG | beach sand |
| T_Kerikil | Dasar sungai | ambientCG | gravel, pebbles |
| T_Abu | Sawit hangus | ambientCG | ground burnt, ash |
| M_KulitJati | Batang jati/mahoni | Poly Haven / ambientCG | bark |
| M_KulitPalem | Kelapa, sawit | ambientCG | palm bark |
| M_DaunAtlas | Daun pohon & semak | ambientCG | leaf set / foliage atlas (dengan opacity) |
| M_Bambu | Bambu | ambientCG | bamboo |
| M_KayuPapan | Rumah, bangunan pemain | Poly Haven | wood planks old |
| M_KayuGelondong | Kayu fisik | Poly Haven | log / wood bark cut |
| M_Atap | Atap rumbia/ijuk | ambientCG | thatch / straw |
| M_BatuSungai | Batu props | Poly Haven | river rock |
| M_Beton | Bunker | Poly Haven / ambientCG | concrete dirty / stained |
| M_LogamKarat | Bunker, heli, ekskavator | ambientCG | rust / metal painted chipped |
| M_Ubin | Lantai lab bunker | ambientCG | tiles dirty |
| M_KainKatun | Kebaya, kemeja, kafan | ambientCG | fabric |
| M_Denim | Celana kargo | ambientCG | denim |
| M_Kulit | Sarung tangan, boot | ambientCG | leather |
| M_Anyaman | Tikar pandan, caping, keranjang | ambientCG | wicker / rattan |
| M_AirNormal | Air | Poly Haven / ambientCG | water normal |
| H_Siang, H_Mendung, H_Senja, H_Malam | Pantulan | Poly Haven | HDRI kategori outdoor / skies / night |
| P_Batu ×6, P_Bongkah ×3, P_KayuLapuk ×3, P_Tunggul ×2, P_Tong, P_Peti ×2, P_Meja, P_Kursi | Props | Poly Haven (model) | rocks, boulder, log, stump, barrel, crate, table, chair |
Pemilihan: ambil 3 kandidat per slot dari API, pilih yang (1) warnanya paling cocok palet 01 §7.2, (2) tidak berpola khas Eropa/beton modern untuk bahan alam, (3) polycount sesuai batas. Catat id, url, ukuran, hash di lock file.

## 4. `Tools/ambil_aset.py` (Python 3, tanpa dependensi selain `requests`)
- Mode: `--cari <slot> <kata kunci>` (butuh internet agen; menulis kandidat ke `Logs/kandidat_aset.json`), `--tambah <slot> <sumber> <id>` (memperbarui lock), `--semua` (mengunduh semua isi lock; dipakai CI & setup script).
- Poly Haven: API publik `https://api.polyhaven.com` (daftar aset, info aset termasuk polycount, daftar file per peta/resolusi/format). Wajib header **User-Agent khas** (mis. `AlasKeramatBuilder/1.0`). Game tidak memanggil API saat dimainkan; aset hanya diunduh saat build.
- ambientCG: API `https://ambientcg.com/api/v3/assets` (atau v2 `full_json`) untuk mencari & mendapatkan tautan unduhan ZIP 1K-JPG.
- Verifikasi ukuran/hash, ulangi 3× bila gagal, lanjutkan unduhan yang terputus, ekstrak ke `Assets/ThirdParty/CC0/<Slot>/` dengan nama baku `<Slot>_Albedo`, `_Normal`, `_Rough`, `_AO`, `_Height`.
- `AKTexturePostprocessor` (Editor): normal map → NormalMap, roughness+AO → mask map URP dipak Builder, maks 1024, ASTC 6×6 (Android), mipmap ON, streaming mipmap ON.
- Kredit (tidak wajib untuk CC0, tetapi baik): layar Kredit mencantumkan "Tekstur & model: Poly Haven, ambientCG (CC0). Data gerak: CMU Graphics Lab Motion Capture Database."

## 5. Motion-Capture CMU → Rangka 20 Sendi
- Sumber resmi `mocap.cs.cmu.edu` (format ASF/AMC; data boleh disalin, dimodifikasi, dan didistribusikan ulang). Codex menulis **parser ASF/AMC** (teks) di Editor, me-**retarget** ke rangka 02 §1.1 (pemetaan sendi + koreksi pose istirahat + skala tinggi), mengekstrak **root motion** (kecepatan maju) dan menyimpan hasil sebagai `PoseClip` (aset ScriptableObject di `Assets/Generated/Anim/`), lalu `ProceduralAnimator` memutarnya sebagai lapisan dasar (IK kaki, look-at, napas tetap di atasnya).
- `Tools/mocap.lock.json` mencatat subjek/percobaan yang dipilih. Pilih berdasarkan deskripsi gerak di indeks situs:
| Klip | Dipakai oleh | Kata kunci deskripsi CMU |
|---|---|---|
| Jalan, lari, sprint, jalan mundur, belok | Semua manusia | walk, run, jog, turn, backwards |
| Jalan jongkok / mengendap | Andi (bayangan), Sekte | sneak, crouch walk |
| Diam bernapas, menoleh, menggeser berat | Semua | stand, idle, look around |
| Ayun kapak / memotong | Andi, Deden | chop, axe, swing |
| Melempar | Andi, Sekte Wanita | throw |
| Lompat, mendarat, jatuh, bangun | Semua | jump, fall, get up |
| Memanggul beban | Andi, Deden | carry, heavy object |
| Menyeret | Sekte | drag, pull |
| Duduk, duduk di lantai | Deden, Ningsih | sit, sit on floor |
| Merangkak | Sekte terluka | crawl |
| Mabuk/limbung, jalan pincang | Sekte terluka, Mutan Gembung | stagger, limp |
| Tarian pelan | Ningsih (ditambah gerak tangan prosedural "ngithing") | dance, slow |
| Pantomim takut/menunjuk | Deden | point, scared |
- Tetap prosedural: Pocong (melompat), Kuntilanak (melayang), Kuyang, Banaspati, Jenglot, Mutan Jemari (gerak patah-patah), lengan FP Andi (agar senjata presisi).

## 6. Sinematografi untuk Menyembunyikan Kelemahan Wajah
- Cutscene memakai **cahaya dari belakang/samping** (api, senter, bulan), kedalaman kabut, dan bidikan dari belakang bahu; close-up wajah hanya ≤ 2 detik dan di cahaya redup.
- Kang Deden sering terlihat dari samping/membelakangi (bekerja); Ningsih sering tertutup rambut & bayangan pohon; Sekte bertopeng; hantu menutup wajah (kafan, rambut).
- Layar status karakter & BERCERMIN: pencahayaan lembut + vignette + grain agar tekstur wajah tidak terlihat "plastik".

## 7. Rubrik Foto Kualitas (G2) — skor 0–10 per foto, dinilai Codex dari gambar & dicatat di PROGRESS
| Kriteria | 3 = buruk | 6 = layak | 9 = sangat baik |
|---|---|---|---|
| Cahaya & bayangan | Rata, tanpa arah | Arah jelas | Dramatis, kontras terkontrol, ada sorot/rim |
| Atmosfer & kedalaman | Tanpa kabut | Kabut ada | Lapisan jarak terbaca, sinar tajuk/kabut berpendar |
| Material | Permukaan polos/berulang | Bertekstur | Detail mikro, basah/kering meyakinkan |
| Vegetasi | Jarang, seragam | Cukup | Lebat bervariasi, bergerak, ada lapisan bawah |
| Komposisi & siluet | Membingungkan | Terbaca | Landmark menonjol, mata dituntun |
| Warna & grading | Tidak konsisten | Konsisten | Sesuai suasana (01 §7.3), emosional |
| Karakter | Kaku/rusak | Wajar | Pose & proporsi meyakinkan, pakaian bertekstur |
| Rasa takut (foto malam) | Terang & aman | Gelap | Gelap bermakna, ada sesuatu yang "mungkin ada" |
| Artefak | Pink, z-fighting, tekstur meregang, pop-in | Sedikit | Bersih |
Target rata-rata: T2 ≥ 6,5; T4 ≥ 6,5 (karakter); T6 ≥ 7; Irisan Vertikal ≥ 7,5; T10–T11 ≥ 7,5. Penilaian harus jujur: sertakan 1 kalimat alasan per kriteria yang < 6 dan rencana perbaikan.

## 8. Dua Belas "Money Shot" (set Foto Kualitas tetap)
1. Bangkai heli pagi 07:30 dengan asap & sinar tajuk. 2. Lapangan Kemah siang dengan Saung jadi. 3. Lembah bambu berkabut 06:00. 4. Air terjun & kolam dari bawah. 5. Desa Pasir Lenyap senja. 6. Beringin Agung saat Maghrib (langit merah). 7. Api unggun malam, Deden & Ningsih duduk, kabut di tepi cahaya. 8. Hutan malam hanya dengan obor (sosok samar di batas cahaya). 9. Sawit hangus & rawa merah malam. 10. Gua lumut bioluminesensi. 11. Koridor bunker alarm merah. 12. Hujan badai di pantai dengan kilat & mercusuar.

## 9. Anggaran HP (pengganti 11 §7.1 untuk tekstur)
Terrain layer 1K × 8 (ASTC 6×6 ≈ 0,5 MB/peta), props CC0 ≤ 512–1K, total memori tekstur ≤ 300 MB pada preset Tinggi, ≤ 180 MB Sedang (streaming mipmap aktif). Klip mocap: ≤ 40 klip, dikompres (keyframe dikurangi bila error rotasi < 0,5°).
