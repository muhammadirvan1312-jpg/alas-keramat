# 15 — LONCATAN VISUAL: AK SHADER PACK, TEKSTUR PROSEDURAL KUALITAS TINGGI, CAHAYA & GRADING, CUACA, ANIMASI, JALUR ASET CC0
Dibaca pada tahap T2 (shader, langit, terrain), T4 (karakter), T5–T6 (props), T11 (kualitas & performa).
Aturan kunci: kualitas visual datang dari **cahaya → kabut → warna → bentuk → tekstur**, berurutan.

---

## 1. Target Jujur
| Jalur | Hasil yang realistis dicapai |
|---|---|
| A. 100% prosedural (wajib) | "Atmosferik semi-realistis": cahaya & kabut sinematik, permukaan berdetail, siluet kuat. Lebih dekat ke game horor indie modern daripada v2, tetapi tidak fotorealistis. |
| B. Prosedural + tekstur CC0 (opsional, §9) | Permukaan tanah, batu, kayu, kain mendekati foto. Bersama shader §2, ini jalur terdekat ke tampilan kelas SOTF di HP. |
Batas perangkat: GPU HP menengah ±5–10× lebih lemah dari GPU PC untuk SOTF; jadi target adalah **kesan** visual kelas atas lewat pencahayaan & atmosfer, bukan jumlah poligon.

## 2. AK Shader Pack (HLSL URP tulisan tangan — `Assets/Shaders/`)
### 2.1 Aturan wajib setiap shader
- `Fallback "Universal Render Pipeline/Lit"`; include `Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl` & `Lighting.hlsl`.
- Pass: `UniversalForward`, `ShadowCaster`, `DepthOnly` (+ `DepthNormals` bila perlu).
- `#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE`, `_ _SHADOWS_SOFT`, `_ _ADDITIONAL_LIGHTS`, `multi_compile_fog`, `multi_compile_instancing`; keyword fitur pakai `shader_feature_local` (hemat varian).
- Semua properti material di `CBUFFER_START(UnityPerMaterial)` (kompatibel SRP Batcher).
- Cahaya tambahan memakai `GetAdditionalLightsCount()` + `GetAdditionalLight(i, posWS)`. Builder menyetel renderer URP ke jalur **Forward** (bukan Forward+) agar loop klasik berlaku di 2022.3 & Unity 6.
- `SafeMaterialFactory`: bila `shader == null || !shader.isSupported` → material Lit setara (tekstur sama). `NusantaraSelfTest` memanggil `ShaderUtil.ShaderHasError(shader)` untuk tiap shader AK.
- Include bersama `AK_Common.hlsl`: kabut tinggi + hamburan matahari, fungsi basah, fungsi angin, dither crossfade LOD, hash/noise.
### 2.2 Properti global (diset `URPAtmosphereAndWeather` via `Shader.SetGlobal*`)
`_AK_SunDir`, `_AK_SunColor`, `_AK_FogColor`, `_AK_FogDensity`, `_AK_FogHeightFalloff`, `_AK_SkyZenith`, `_AK_SkyHorizon`, `_AK_Wetness` (0–1), `_AK_RainAmount`, `_AK_WindDir`, `_AK_WindStrength`, `_AK_PlayerPos`, `_AK_LightningFlash`, `_AK_Time`.
**Kabut tinggi** (semua shader AK): `f = 1 − exp(−density × jarak × exp(−falloff × max(0, y)))`; warna = `lerp(FogColor, SunColor × 1,4, pow(saturate(dot(arahPandang, SunDir)), 8) × 0,5)` → kabut bercahaya saat menghadap matahari (efek "volumetrik" murah).
### 2.3 Daftar Shader
| Shader | Teknik inti | Biaya |
|---|---|---|
| ~~`AK/Terrain`~~ (DIBATALKAN v4 — pakai Unity Terrain + URP `Terrain/Lit`, lihat 19 §2) | 4 lapis splat (R hutan/rumput, G tanah/jalan, B batu kapur, A pasir) dari **WorldControl 1024**; pencampuran berbasis ketinggian (alpha albedo = tinggi) agar transisi alami; **triplanar** untuk lapis batu di lereng > 35° (cabang hanya bila bobot > 0,05); tiling ganda jarak (3 m dekat, 12 m jauh) mengurangi pengulangan; tint makro dari WorldAlbedo (bara sawit, rawa, lumpur); basah: albedo ×0,6, smoothness +0,5, **genangan** di area datar bila basah > 0,5 | 10 sampel (Rendah: `AK/TerrainLite` 2 lapis) |
| `AK/Foliage` | Alpha-clip 2 sisi (normal dibalik di sisi belakang), **translusensi** (cahaya matahari tembus daun saat membelakangi matahari), **angin**: tekuk batang berdasar tinggi + getar daun (vertex color R = bobot getar, G = fase), dorongan dari `_AK_PlayerPos` untuk rumput/semak, variasi warna per instance (hash posisi), fade dither di jarak | sedang |
| `AK/Bark` | Albedo + normal + **lumut** di sisi atas & utara (mask normal dunia + noise), basah, AO vertex | rendah |
| `AK/Water` | 2 gelombang Gerstner kecil, 2 normal map bergulir, **warna kedalaman dari vertex color** (kedalaman dipanggang Builder dari `SampleHeight`: dangkal `#3E8C7A` → dalam `#0F3A4A`) tanpa depth texture, **buih tepi** (kedalaman < 0,3 + noise), fresnel memantulkan warna langit global, kilau matahari, riak hujan prosedural, arah arus sungai (UV2) | sedang |
| `AK/Character` | Atlas + **CharacterMask** (R kulit, G logam, B basah/lendir, A kain): kulit = difus wrap 0,4 + rona bawah-permukaan `#C04030` di garis terminator + rim; kain = sheen lembut; logam = spekular; properti `_BloodAmount`, `_WetAmount`, `_DissolveAmount` (larut abu dengan tepi bara), `_HitFlash` | sedang |
| `AK/Hair` | Alpha-clip + dither, highlight anisotropik ala Kajiya-Kay (geser sepanjang tangen), 2 sisi | sedang |
| `AK/Eye` | Iris + pupil prosedural, smoothness 0,95 → **pantulan cahaya (catchlight)**, refraksi palsu (parallax kecil) | rendah |
| `AK/Sky` | Skybox prosedural: gradasi zenit–horizon per kunci waktu, cakram matahari + halo, **bulan dengan fase**, **bintang berkelip** (grid hash), **2 lapis awan** fBm bergulir (kepadatan sesuai cuaca, diterangi warna matahari), Maghrib merah, bulan darah, kilat | rendah |
| `AK/FogCard` | Lembar kabut menempel tanah/dinding lembah: noise bergulir, pudar dekat kamera & di perpotongan (berdasar tinggi vertex) — Kabut Pagi, Kabut Darah, rawa | rendah |
| `AK/LightShaft` | **Sinar tajuk (god rays)** aditif: kerucut/quad di celah tajuk, noise debu bergulir, intensitas = keselarasan arah matahari × jam (puncak 07:00–09:00 & 15:30–17:00), pudar saat kamera dekat | rendah |
| `AK/Blood`, `AK/Decal` | Decal quad transparan dengan normal, basah → kering (14 §3) | rendah |
| `AK/Wet` | Organ/lendir: smoothness 0,9, warna gelap, emisi sangat redup | rendah |
| `AK/Ghost` | Fresnel transparan + distorsi noise + dissolve (Kuntilanak menghilang, Penyerupa, Ki Rogo bayangan) | rendah |
| `AK/Glow` | Lumut bioluminesensi, segel Ningsih, mata hantu: emisi berdenyut | rendah |
| `AK/Particle` | Partikel lembut (pudar dekat kamera & tanah): asap, bara, kunang-kunang, debu | rendah |

## 3. Tekstur Prosedural Kualitas Tinggi
### 3.1 Pipa per material
`tinggi` (fBm multi-oktaf + **domain warp** + **Worley/seluler** untuk retak/kulit kayu/sisik) → `albedo` (gradient ramp + variasi makro + **AO kavitas** dari tinggi) → `normal` (Sobel, kekuatan per material) → `smoothness` (dari kavitas & basah) → `mask`.
### 3.2 Resep material
| Material | Resep inti |
|---|---|
| Lantai hutan | Tanah fBm + **stempel daun** jatuh acak (bentuk daun jati/mahoni berotasi, 3 warna coklat-kuning) + ranting tipis + humus gelap di cekungan |
| Rumput tropis | Helai bergaris + gumpalan; warna 3 hijau + ujung menguning |
| Kulit kayu jati | Worley diregangkan vertikal + alur fBm + retak dalam; lumut di alur |
| Batu kapur | Worley retak + lapisan strata horizontal + lubang erosi + noda lumut & air |
| Batu sungai | Halus membulat, noda basah, lumut di bawah |
| Pasir | Riak (sinus terdistorsi) + butir + cangkang kecil |
| Lumpur/rawa | Gelap basah, genangan, jejak |
| Kayu papan/balok | Serat cincin (sinus terdistorsi), simpul kayu, retak ujung, kusam abu di permukaan atas |
| Atap rumbia/ijuk | Kartu helai bertumpuk + gradasi kering |
| Bambu | Ruas, garis vertikal, bercak kering |
| Beton bunker | fBm + **noda rembes menetes** (garis vertikal dari retak) + retak Worley + lumut di bawah |
| Logam berkarat | Cat terkelupas (ambang noise) di atas karat oranye-coklat bertekstur |
| Kain (batik, poleng, kafan, kebaya) | Pola (02 §1.5) + **anyaman benang** di normal + noda kotor di tepi bawah |
| Kulit manusia (wajah utama) | Lihat 3.4 |
### 3.3 Resolusi
Terrain layer 1024 (Tinggi) / 512 (Sedang) / 256 (Rendah); props pahlawan (Beringin, heli, rumah, altar) 1024; props umum 512; atlas karakter 1024 (Tinggi). Dibuat sekali di Builder (bukan saat main).
### 3.4 Wajah tokoh utama (Andi, Deden, Ningsih, Ki Rogo) — tekstur 512 terpisah
Zona warna kulit (pipi & hidung lebih merah, bawah mata lebih gelap, dagu/rahang abu pada Deden & Ki Rogo = bakal janggut), pori halus (noise di normal), bibir bergradasi, alis per helai (garis tipis), **tahi lalat Andi**, AO lipatan (dari kelengkungan mesh kepala), noda tanah & darah kering opsional.
### 3.5 AO vertex & bevel
- Builder memanggang **AO vertex** (16 sinar hemisfer per vertex) untuk props, struktur, karakter → vertex color A; shader mengalikan. Celah & sudut jadi gelap alami.
- Semua props bersudut (balok, papan, rumah, peti, altar) memakai **bevel 1–3 cm** (helper `ProcMesh.Bevel`) agar tepi menangkap cahaya — peningkatan kualitas terbesar per biaya.

## 4. Geometri & Detail
| Aset | v2 | v3 (Tinggi / Sedang) |
|---|---|---|
| Beringin Agung | gumpalan sederhana | ±25k / 12k segitiga: batang beralur, akar papan berliku, 60 akar gantung, kain melilit mesh mengikuti batang |
| Pohon LOD0 | ≤ 1.500 | 2.500 / 1.500, cabang bercabang 2 tingkat, kartu daun berlapis |
| Rumah panggung | kotak | Dibangun papan per papan (rotasi acak ±1,5°), tiang miring, atap rumbia berlapis, jendela kisi |
| Batu | sederhana | Ikosfer subdivisi 3 + displacement noise 3D; kapur = faset datar |
| Karakter | 2.800–3.600 | 5.500 / 3.600: kelopak mata, bulu mata (kartu), gigi, lipatan kain, jari dikelompokkan 2 |

## 5. Cahaya, Grading, & Atmosfer
- **LUT per suasana** (URP Color Lookup, strip 1024×32, sRGB off, tanpa kompresi, dipanggang Builder): Siang Tropis (sorot hangat, bayangan teal), Senja Emas, **Maghrib Merah** (biru ditekan, merah didorong), **Malam Biru** (desaturasi cyan), Gua Sian, Bunker Neon Hijau, Kuil Merah, **Malam Darah** (hampir monokrom merah). Dua Volume berbobot untuk transisi halus.
- Bayangan: lunak (Tinggi), 2 kaskade; Rendah: bayangan gumpal (blob) di bawah karakter.
- Api unggun di Tinggi: 1 lampu titik dengan bayangan (satu-satunya).
- **Sinar tajuk**: diletakkan Builder di celah tajuk (kepadatan tajuk < ambang) dekat jalur & POI + partikel debu melayang.
- Partikel kehidupan: kunang-kunang (malam, dekat air), kupu-kupu (siang), daun berguguran, serbuk sari di sinar, bara Sawit, asap dapur Sekte di kejauhan.
- Kalibrasi gelap (13 §6) memakai Lift Gamma Gain.

## 6. Cuaca Visual
Hujan: garis partikel di sekitar kamera (maks 2.000), percikan cincin di tanah, riak di air & genangan, tetesan dari tepi atap saung, **tetes di lensa** (overlay UI meluncur, hanya bila tidak di bawah atap & menengadah), tirai kabut hujan jauh. Badai: **mesh petir bercabang** prosedural (garis strip emisif), kilat langit, angin kuat. Kabut sesuai 05 §2 dengan `AK/FogCard` berlapis.

## 7. Animasi Lebih Hidup (tambahan 02 §1.7)
- **IK dua tulang** analitik untuk tangan memegang senjata/kayu & kaki di tanah (pelvis turun mengikuti kaki terendah).
- **Tulang pegas** (spring) untuk ujung iket, selendang, jarik, rambut panjang, simpul kafan, akar gantung.
- **Blendshape prosedural** (`Mesh.AddBlendShapeFrame`): senyum, takut, marah, mulut terbuka, menyipit, kedip — dihitung dari offset vertex wajah; dipakai ekspresi Deden/Ningsih/Andi (cutscene) & Sekte tanpa topeng.
- Mata bergerak sendiri: **sakade mikro** tiap 0,3–1,2 s, mengikuti titik menarik.
- Condong saat berbelok, antisipasi saat mulai/berhenti, reaksi kena berarah, napas berbeda per kondisi.
- Kamera FP: ayun senjata tertinggal (lag), turun saat mendarat, FOV +5 saat sprint, goyang tangan saat takut.

## 8. Uji Kualitas Visual Otomatis
Menu `Debug/Foto Kualitas`: kamera berpindah ke 7 zona × 4 waktu (08:00, 17:50, 22:00, hujan 14:00) + 3 kompleks bawah tanah, menyimpan PNG ke `Logs/foto_kualitas/`. Kriteria lulus: tidak ada permukaan besar berwarna datar, tidak ada hitam pekat tanpa detail (kecuali sengaja malam), siluet landmark terbaca, pengulangan tekstur tidak terlihat di 30 m, tebing tanpa tekstur meregang, wajah terbaca di 3 m. Pengguna boleh mengirim foto ini untuk dievaluasi.

## 9. Jalur B — Tekstur CC0 (v4: WAJIB & otomatis, lihat 19 — bagian ini hanya untuk referensi)
- Sumber: **Poly Haven** dan **ambientCG** (keduanya CC0: boleh dipakai komersial tanpa kredit). Unduh resolusi 1K (HP), format PNG/JPG.
- Simpan ke `Assets/Resources/Override/Textures/<Slot>_Albedo.png`, `<Slot>_Normal.png`, `<Slot>_Rough.png` (opsional). `AssetPostprocessor` di Editor mengatur impor otomatis (normal map, ASTC, maks 1024). Builder memakai override bila ada, mencatatnya di laporan.
| Slot | Kata kunci pencarian di situs |
|---|---|
| `TerrainForest` | forest floor leaves |
| `TerrainGrass` | grass ground |
| `TerrainDirt` | brown mud dirt |
| `TerrainRock` | limestone rock |
| `TerrainSand` | beach sand |
| `TerrainBurnt` | burnt ground ash |
| `BarkTeak` | tree bark |
| `BarkPalm` | palm bark |
| `Bamboo` | bamboo |
| `WoodPlank` | wood planks old |
| `WoodLog` | wood log |
| `Thatch` | thatch roof straw |
| `RockRiver` | river rock |
| `Concrete` | concrete dirty |
| `MetalRust` | rusty metal |
| `MetalPaint` | painted metal chipped |
| `FabricCotton` | fabric cotton |
| `Leather` | leather |
| `MudWet` | wet mud |
- Model 3D CC0 (mis. gaya low-poly) TIDAK dianjurkan untuk karakter karena gaya & rangka tidak cocok; untuk props kecil boleh lewat `Override/Meshes/<NamaProp>`.

## 10. Biaya per Preset
| Fitur | Rendah | Sedang | Tinggi |
|---|---|---|---|
| Terrain | TerrainLite 2 lapis | 4 lapis, tanpa triplanar | 4 lapis + triplanar |
| Translusensi daun & dorongan rumput | mati | nyala | nyala |
| Air | tanpa Gerstner | 1 gelombang | 2 gelombang + riak |
| Sinar tajuk | mati | 50% | 100% |
| LUT | 1 statis | dinamis | dinamis |
| Awan langit | 1 lapis | 2 lapis | 2 lapis + kilau tepi |
| Wajah 512 & blendshape | atlas saja | ya | ya |
| Partikel kehidupan | 30% | 70% | 100% |
