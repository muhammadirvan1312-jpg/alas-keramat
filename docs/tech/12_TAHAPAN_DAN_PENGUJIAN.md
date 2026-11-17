# 12 — TAHAPAN PENGERJAAN (T0–T12), PROMPT CODEX PER TAHAP, CHECKLIST UJI NON-CODER & TROUBLESHOOTING
Dibaca di SETIAP sesi. Satu sesi Codex = satu tahap. Jangan lompat tahap.

---

> **v4 — Codex Cloud**: pengguna TIDAK menjalankan T0 di bawah ini. Ikuti `docs/tech/18_PIPELINE_CLOUD_OTONOM.md` §2 (persiapan sekali), lalu Codex mengerjakan **T0-C** (18 §4). Checklist "buka Unity" di setiap tahap diganti: CI hijau + APK uji dimainkan di HP. Bagian "Tambahan v4" di bawah wajib.

> **v5**: setiap tahap juga mengerjakan baris tahap itu di `docs/gdd/20` §6 dan `docs/gdd/21` §10. Gerbang G5 Uji Dengar di akhir T2, T7, T9, T10.

## T0 — Persiapan lokal (hanya bila memakai komputer sendiri)
1. Pasang **Unity Hub**, lalu Unity **6 LTS** terbaru (atau 2022.3 LTS) dengan modul **Android Build Support** (centang OpenJDK serta Android SDK & NDK Tools). Pengguna Mac yang ingin iOS: tambah iOS Build Support.
2. Di Unity Hub → New Project → template **"Universal 3D"** (URP) → nama `AlasKeramat` → Create. Tunggu sampai terbuka, lalu **tutup Unity**.
3. Salin `AGENTS.md` dan folder `docs/` ke folder proyek (sejajar folder `Assets`).
4. Pasang **Codex CLI** sesuai petunjuk resmi OpenAI, buka terminal di folder proyek, jalankan `codex`.
5. (Opsional, sangat membantu) Atur variabel lingkungan `UNITY_EXE` ke lokasi `Unity.exe` (lihat 11 §14) supaya Codex bisa mengecek error sendiri.
6. Saat Codex bekerja, **Unity harus tertutup**. Setelah Codex selesai, baru buka Unity dan lakukan checklist.

Prompt umum setiap tahap (ganti `{n}`):
> "Baca AGENTS.md, docs/PROGRESS.md, dan docs/tech/12_TAHAPAN_DAN_PENGUJIAN.md bagian T{n}. Kerjakan TAHAP T{n} sepenuhnya: semua file, fitur, dan self-test yang tercantum. Baca semua dokumen docs/ yang diwajibkan tahap ini. Patuhi semua Hukum Besi. Jalankan lingkaran verifikasi H12 sampai 0 error. Perbarui NusantaraMasterBuilder, NusantaraSelfTest, dan docs/PROGRESS.md. Akhiri dengan instruksi uji untuk saya dalam Bahasa Indonesia sederhana."

---

## T1 — Fondasi, Pabrik Aset, Synth Audio, Builder & Self-Test
**Baca**: AGENTS, 01, 10 §1–3, 11 (semua).
**File**: `SOTFContracts`, `GameServices`, `GameEvents`, `GameBootstrap`, `PhysicsCompat`, `NusantaraDiagnostics`, `ProceduralAssetFactory` (SafeMaterialFactory, ProcTex: noise/fBm/normal-dari-tinggi/simpan PNG, IconPainter dasar 20 ikon), `ProceduralAudioSynth` (Dsp lengkap, penulis WAV, pool suara, 5 klip wajib v1 + klik UI + ambil item), `NusantaraMasterBuilder` (menu §9.1, langkah 1, 2-sebagian, 7-sebagian, 9), `NusantaraSelfTest`, `Tools/cek_kompilasi.*`.
**Isi scene T1**: tanah uji 200×200 bertekstur, matahari, kubus uji bermaterial PBR prosedural, Canvas dengan teks "ALAS KERAMAT — FONDASI OK", 5 tombol uji suara (Tebang, Pohon Tumbang, Kentongan, Kuntilanak, Pocong), EventSystem dengan modul input yang benar.
**Self-test**: layer 6–18 & tag ada; aset URP terpasang di semua level Quality; ≥ 10 tekstur & ≥ 5 material di `Assets/Generated`; tidak ada material ber-shader error; `GetSafeFont()` tidak null; ≥ 8 WAV; scene tersimpan & terdaftar di Build Settings; klik Builder 2× → jumlah objek root sama.
**Checklist pengguna**:
- [ ] Buka Unity, tunggu kompilasi selesai (pojok kanan bawah tidak berputar).
- [ ] Menu **Nusantara Builder** muncul di bar atas.
- [ ] Klik `1. BANGUN SELURUH GAME (1-KLIK)` → muncul dialog ringkasan tanpa kata GAGAL.
- [ ] Tekan **Play**: tanah & kubus bertekstur (tidak pink), teks terbaca, 5 tombol mengeluarkan suara berbeda.
- [ ] Console (Window → General → Console) tidak ada baris merah.
- [ ] Klik `Utilitas/Jalankan Self-Test` → semua LULUS.

## T2 — Dunia 1000×1000, 7 Bioma, Air, Vegetasi Visual, Waktu, Cuaca, Ambien
**Baca**: 01 §7, 03 (semua), 05 §1–2, 10 §4.
**File**: `WorldTerrainGenerator`, `TimeOfDayClock`, `URPAtmosphereAndWeather`, `ProceduralAssetFactory.Textures/.Meshes/.Props` (terrain, WorldAlbedo, air, batu, landmark: heli, rumah panggung, beringin agung, mercusuar, perahu, ekskavator, altar, mulut gua (mesh lengkap; teleport aktif di T10)), `WorldCaveAndSaveManager` (penempatan zona & props), `DirectionalTreeAndForaging.Vegetation` (`VegetationManager` render instanced + pohon dekat statis), ambien & cuaca audio, Builder langkah 3, **Tur Pratinjau Otomatis** (kamera sinematik mengunjungi 7 zona dengan label nama zona, waktu dipercepat ×20 agar siang/Maghrib/malam/cuaca terlihat; aktif bila Andi belum ada).
**Tambahan v4 (baca 19 §2–4, §9)**: terrain = Unity Terrain + `Terrain/Lit` + 8 TerrainLayer CC0 (BUKAN `AK/Terrain`), pantulan HDRI, material props CC0; gerbang G2 (target ≥ 6,5).
**Tambahan v3 (baca 15 §2–6)**: `Assets/Shaders/` — `AK_Common.hlsl`, (`AK/Terrain` dibatalkan v4), `AK/Foliage`, `AK/Bark`, `AK/Water`, `AK/Sky`, `AK/FogCard`, `AK/LightShaft`, `AK/Particle`, `AK/Glow`; WorldControl 1024 + layer terrain berpipa kualitas tinggi (15 §3); AO vertex & bevel props; LUT per suasana; sinar tajuk; partikel kehidupan; cuaca visual (15 §6); fase bulan; menu `Debug/Foto Kualitas`.
**Self-test**: semua shader AK tanpa error (`ShaderUtil.ShaderHasError`) & punya fallback; cek ketinggian 03 §2; 400 chunk; laut ada; `GetBiome` 10 sampel cocok; vegetasi 4.500–5.500; jam maju; semua 7 cuaca bisa diset; 7 zona punya objek kunci.
**Checklist**:
- [ ] Builder 1-klik sukses; Play → tur otomatis melewati Heli, Bambu, Desa, Sawit, Beringin, Tebing, Pantai.
- [ ] Langit berubah merah saat Maghrib dan terdengar kentongan; malam gelap kebiruan; hujan badai ada kilat & guruh.
- [ ] Sungai mengalir dari air terjun ke laut; tidak ada air melayang di udara.
- [ ] FPS di Game view (Stats) ≥ 30 pada preset Sedang.

## T3 — Andi FPS, Kontrol Sentuh, HUD, Radar, Vitals, Interaksi, Inventaris Dasar
**Baca**: 05 §3–4, 09 §1–3 & §8, 04 §7.1 (hanya kartu misi & subtitle), 11 §11.
**File**: `AndiFPSController` (+Swim, Camera), `AndiVitalsAndSanity`, `TouchControls`, `MobileTouchHUDAndRadar` (+Radar), `IconPainter` lengkap, inti inventaris (`ItemDatabase` lengkap + model data inventaris di `TikarPandan3DInventory`), interaksi `IInteractable` (koper, pickup, minum air, isi botol), tangan kiri (Obor menyala dengan KorekGas, GPS mengaktifkan radar), sabuk cepat. Tur pratinjau dimatikan bila Andi ada.
**Self-test**: Andi spawn di (−2,−212); CharacterController & kamera ada; semua tombol HUD punya handler; 3 koper berisi item sesuai 03 §9; laju vitals sesuai 05 (simulasi 60 s).
**Checklist** (bisa diuji dengan mouse di Game view):
- [ ] Joystick muncul di tempat jari/klik; geser penuh ke atas → "LARI".
- [ ] Geser sisi kanan memutar kamera; tombol LOMPAT & JONGKOK bekerja.
- [ ] Buka 3 koper → toast item muncul; radar aktif setelah GPS; obor menyala dan menerangi malam.
- [ ] Minum di anak sungai menaikkan bar Haus; bar turun perlahan seiring waktu.
- [ ] Baris info menampilkan HARI, jam, label waktu, ikon cuaca (bukan kotak kosong).

## T4 — Karakter Prosedural & Animasi
**Baca**: 02 (semua).
**File**: `ProceduralAssetFactory.Characters` (`CharacterMeshBuilder`: loft, kepala & wajah, atlas 512 + emisi, rigid skinning, LOD), `ProceduralAnimator` (+Clips: semua klip 02), Andi 3 renderer + kesadaran tubuh, Deden (idle duduk terhuyung di heli), Ningsih (idle di Sarang (100,−175)), prototipe semua musuh & hewan, interaksi **BERCERMIN** di air, layar Status Karakter (kamera potret wajah), **Galeri Karakter** (menu Debug: teleport ke barisan semua karakter yang memutar klip bergiliran).
**Tambahan v4 (baca 19 §5–6)**: parser ASF/AMC CMU + retarget ke 20 sendi + PoseClip mocap untuk semua manusia; kain CC0 di atlas; aturan sinematografi wajah; gerbang G2 karakter (≥ 6,5).
**Tambahan v3 (baca 15 §3.4, §4, §7; 14 §5)**: shader `AK/Character`, `AK/Hair`, `AK/Eye`, `AK/Ghost`; CharacterMask; tekstur wajah 512 untuk 4 tokoh utama; blendshape prosedural; IK dua tulang; tulang pegas; sakade mata; prop anggota tubuh & tutup potongan untuk tiap varian musuh; visual Kuyang, Tuyul, Genderuwo Hutan, Mutan Gembung & Jemari di Galeri.
**Self-test**: tiap karakter 1 SkinnedMeshRenderer (Andi 3), segitiga dalam batas 02 §1.2, 20 sendi + 4 wajah ada dengan nama benar, tahi lalat Andi ada di posisi 02 §2.
**Checklist**:
- [ ] Menunduk → terlihat dada, kaki melangkah, jaket hijau zaitun; menoleh tidak melihat ke dalam kepala.
- [ ] Layar status / bercermin: wajah Andi dengan **tahi lalat di bawah mata kiri**.
- [ ] Galeri: Deden (rompi oranye, iket, perban telinga, bot kuning), Ningsih (kebaya, jarik parang, bunga kantil, segel berpendar), Ki Rogo, 5 Sekte, Pocong (melompat), Kuntilanak (melayang), Genderuwo, hewan — semua bergerak.

## T5 — Tebang Pohon, Kayu Fisik, Foraging, Hewan, Tikar Pandan 3D, Crafting, Makan, Zirah
**Baca**: 05 §4–7, 07 §1–3, 03 §7–8, 09 §5, 06 §2 (Golok & kapak saja).
**File**: `DirectionalTreeAndForaging` (+Animals), pool pohon interaktif, kayu fisik, foraging, `WildAnimalAI`, melee dasar golok/kapak (ringan + berat) di `CombatAndLeftHandController`, `TikarPandan3DInventory` (+Crafting: panggung 3D, 16 resep), makan/minum/zirah, suara kayu.
**Self-test**: pohon rebah sesuai kuadran (simulasi); hasil kayu sesuai jenis; 16 resep valid (ID unik, bahan ada di ItemDatabase); hewan drop sesuai 03 §8.
**Checklist**:
- [ ] Tebang jati dari satu sisi → pohon rebah menjauhi sisi itu, berbunyi retak & gemuruh, pecah jadi 4 kayu yang bisa menggelinding.
- [ ] Rumpun bambu menghasilkan Batang Bambu; pisang memberi daun & buah.
- [ ] Buka Tikar → item 3D tertata; seret Bambu + Tali ke lingkaran → RAKIT Tombak.
- [ ] Makan Ransum menaikkan Kenyang; pakai Zirah Daun mengisi kotak zirah.
- [ ] Rusa lari saat didekati; berhasil diburu memberi daging & kulit.

## T6 — Panggul Bahu, Bangun Bebas 5 Mode, Integritas, Buku Panduan 14 Blueprint, Masak, Tidur & Simpan
**Baca**: 07 §3–7, 05 §5 & §8, 11 §8, 09 §2.5 (cincin mode).
**File**: `ShoulderLogAndFreeformBuilder` (+Sockets, +Integrity), `GuidebookBlueprintsAndCooking` (+Blueprints, +Cooking), `SaveSystem` lengkap (`WorldCaveAndSaveManager.Save`), Kasur & Tenda, api unggun (bahan bakar, cahaya, masak), Penampung Hujan, Jemuran, Panci, Obor Tengkorak, misi M01–M04 sementara memakai `QuestManager` inti.
**Tambahan v3 (baca 17 §4)**: Batu Besar & mode Fondasi/Dinding/Jalan Batu, Potong Celah, blueprint 15–26 (kecuali 16 Patung Ngeri yang butuh item T7 — tetap dibuat, bahan alternatif Tulang).
**Self-test**: snap tiang→balok→papan valid; BFS integritas meruntuhkan potongan tak tertopang; 14 blueprint biaya sesuai 07; simpan→muat menghasilkan state identik (hash daftar struktur & inventaris).
**Checklist**:
- [ ] Ambil 2 kayu → terlihat di bahu kiri saat menunduk; LEMPAR melempar kayu.
- [ ] Pasang 4 tiang + 2 balok + papan → lantai; hancurkan tiang → bagian atas runtuh.
- [ ] Serut tiang → jadi pagar runcing; belah kayu → 2 papan.
- [ ] Buku Panduan: tandai Saung → isi bahan → Saung jadi; tidur di Kasur → pagi; keluar & muat → semuanya kembali.
- [ ] Masak ikan di api: mentah → matang → gosong bila dibiarkan.

## T7 — Tempur Penuh, Sekte, Hantu, Pathfinding, Spawner, Agresi, Raid, Tertangkap
**Baca**: 06 (semua), 02 §6–7, 10 §3.2 & §3.5, 11 §10.
**File**: `CombatAndLeftHandController` (+Ranged: semua senjata, parry, blok, holster, aim assist), `GridPathfinder` (+ panggang SurfaceGrid di Builder), `NusantaraEnemyAndMutantAI` (+Sekte, +Ghosts, +Director), Totem Topeng ×6, Gua Tawanan, Malam Darah, musik tempur dasar.
**Tambahan v3 (baca 14 semua, 17 §2, §5)**: `GoreSystem` (darah, decal, luka, Pendarahan, mutilasi, ragdoll ringan, mayat, GELEDAH/AMBIL TOPENG/POTONG, 3 level gore + layar 18+), `AK/Blood`, `AK/Wet`, senjata 118–127, Elak, Bunuh Senyap, Asah, meter suara, umpan, seret mayat, kamuflase lumpur, pencarian Sekte, musuh 13–17.
**Self-test**: putus anggota tubuh menyembunyikan bagian mesh yang benar & memunculkan prop (simulasi tiap varian); batas decal/ragdoll/mayat dipatuhi; SurfaceGrid 250.000 sel & ≥ 70% darat bisa dilalui; jalur A* Heli→Desa ditemukan < 4.000 node; tiap archetype punya statistik 06 §4; maks musuh aktif ≤ 12 (simulasi spawn 30).
**Checklist**:
- [ ] Siang Hari 1 sore: sosok bertopeng mengintai dari jauh lalu menghilang.
- [ ] Di Desa: patroli; pemimpin memukul kentongan "Tok-Tok… Tok-Tok" lalu Sekte berkumpul.
- [ ] Parry tepat waktu → musuh terhuyung; Tebasan Berat Penghabisan → musuh larut jadi abu.
- [ ] Sekte sekarat berlutut menyerah → "AMPUNI" → ia lari.
- [ ] Malam: Pocong melompat (dentum sub-bass), Kuntilanak tertawa dari beringin; Obor Tengkorak membuat Sekte ragu.
- [ ] Debug "Agresi 80" + tunggu malam → raid ke kemah; pagar runcing melukai mereka.

## G3 — Irisan Vertikal (gerbang v4, setelah T7)
Zona 1 (heli) + Lapangan Kemah + malam pertama lengkap & dipoles sampai: CI hijau, skor rubrik 19 §7 ≥ 7,5 pada money shot 1, 2, 7, 8, dan `docs/UMPAN_BALIK.md` sudah ditanggapi. Kerjakan sebagai PR polesan (`AUTOPILOT_TAHAP_BERIKUTNYA: G3`) sebelum T8.

## T8 — Kang Deden & Ningsih
**Baca**: 08 (semua), 02 §3–4, 09 §6.
**File**: `KangDedenKelvinAI` (+Orders), `NingsihVirginiaAI` (+Trust), layar Buku Catatan, hadiah, menu interaksi Ningsih, senandung & tari, indra keenam.
**Tambahan v3 (baca 17 §7)**: perintah ke-15 `BangunPagarKeliling`; menu "Berikan Pistol" (Trust ≥ 90).
**Self-test**: 15 perintah punya handler & alasan menolak; Trust berubah sesuai 08 §3 (simulasi); companion teleport hanya di luar pandangan kamera.
**Checklist**:
- [ ] Beri Jamu ke Deden, tunjukkan Buku Catatan → ia menyipit membaca, mengangguk 2×, jempol kanan.
- [ ] Perintah "Ambil Kayu" → ia menebang, memanggul 2 kayu, menaruh di Rak.
- [ ] Hantu mendekat → Deden menunjuk panik lalu berlindung.
- [ ] Ningsih mengintip lalu kabur saat dikejar; simpan senjata + jongkok → ia mendekat.
- [ ] Debug "Trust 70" → ia membawa hadiah; berikan Busur + Pistol Suar → ia menembak musuh sambil melompat ke samping.

## T9 — Sutradara Teror, Folklor Hidup & Dunia yang Hidup (BARU v3)
**Baca**: 13 (semua), 16 (semua), 17 §5–7, 10 §4.
**File**: `Assets/Scripts/Core/TerrorDirector.cs` (+`.Events.cs`: 37 kejadian), `Assets/Scripts/Core/WorldEventScheduler.cs` (13 kejadian dunia), `Assets/Scripts/AI/EcosystemDirector.cs` (+`.Wildlife.cs`: burung, rangkong, elang, monyet, ular, kepiting, penyu, kelelawar, tokek), jadwal & perilaku sosial Sekte + Kampung Watu Ireng + Kemah Rawa + Makam Tua + 5 Pos Pengintai, Kuyang/Tuyul/Genderuwo Hutan/Penyerupa/Hantu Air, Paku Besi Kuning & Lepas Tali Pocong, binaural dipanggang, suara sintetis baru (13 §5.3), pola getar, kalibrasi gelap, Catatan Kang Deden (20), kebun & ziarah Ningsih, TrailMap, jejak kaki, 40 objek PERIKSA, Mangkuk Kepiting.
**Self-test**: fase Sutradara berganti sesuai Tension (simulasi 20 menit malam: ≥ 6 kejadian Ringan, ≤ 2 Keras); tidak ada kejadian berulang dalam 2 hari game; jadwal Sekte 24 jam menghasilkan state yang benar per jam; 20 catatan Deden punya pemicu valid; lokasi baru punya pad & objek kunci.
**Checklist**:
- [ ] Malam pakai headset: langkah membuntuti lalu berhenti setelah kamu berhenti; bisikan "Andi" di satu telinga.
- [ ] Tawa Kuntilanak pelan saat dia dekat dan melengking saat jauh; kelopak kamboja melayang.
- [ ] Jatuhkan Pocong lalu "LEPAS TALI POCONG" → ia runtuh jadi kain kosong.
- [ ] Siang: burung beterbangan saat Sekte mendekat; monyet menjerit; senja ribuan kelelawar keluar dari Gua Air.
- [ ] Menyelinap ke Watu Ireng saat 12:00–14:00: Sekte tidur siang; malam mereka menari mengelilingi api.
- [ ] Pagi hari ada catatan dari Kang Deden di kasurnya.

## T10 — Gua & Bunker, Misi M01–M13, 12 Lore, Cutscene, Boss, Ritual, Siege, 2 Ending, Musik
**Tambahan v3 (baca 17 §6)**: Rendra sebagai Mutan Jemari di UG2 R2 + item 194, misi sampingan S01–S04 (`QuestID` 101–104), epilog bercabang, set-piece horor gua (13 §8).
**Baca**: 03 §10, 04 (semua), 06 §9–11, 10 §5.
**File**: `WorldCaveAndSaveManager` (+Caves, +Quests, +Lore, +Cutscenes, +Endings), `WaypointGraph`, mode dalam ruangan, terowongan bawah air & Alat Selam, pintu kartu, gali bunker 4×, zipline (`AndiFPSController.Zipline` + tembak tali), boss Genderuwo & Ki Rogo, altar 3 lubang, siege, heli SAR, layar pilihan, statistik, sandbox, Golok Api Pusaka, Peta Kertas + kabut eksplorasi, penjadwal musik gamelan.
**Self-test**: 13 misi berurutan dengan gerbang 04 §4 (simulasi flag); 12 lore teks tidak kosong & sesuai; 3 kompleks punya pintu masuk/keluar valid; siege 60 s memunculkan gelombang 06 §11.
**Checklist** (gunakan menu Debug untuk mempercepat):
- [ ] Intro sinematik tampil saat Mulai Baru; kartu misi muncul.
- [ ] Gua Air: lumut menyala, Pocong jatuh dari langit-langit, menyelam pakai Alat Selam, ambil Paku 1.
- [ ] Gali tanah X 4× → palka bunker; pintu terbuka dengan kartu; alarm saat ambil Paku 2.
- [ ] Tembak tali menyeberangi jurang; kuil merah; Ki Rogo bicara; Genderuwo bertarung 3 fase.
- [ ] Tancapkan 3 paku → BERTAHAN 60 DETIK → cahaya emas → heli SAR → pilih ending A & B (dua kali main) → statistik.

## T11 — Menu, Pengaturan, Kesulitan, Performa, Android
**Tambahan v3 (baca 17 §8–9, 15 §8 & §10, 14 §1)**: layar 18+ & pilihan gore saat pertama dibuka, Intensitas Horor, Headset, Mode Imersif (jam & GPS di tangan), prompt di dunia, Mode Kustom, preset visual per 15 §10, `AKTexturePostprocessor` untuk jalur CC0, pass performa semua sistem v3.
**Baca**: 09 §4 & §10, 05 §9, 11 §7 & §15.
**File**: `MenuScreens` (menu utama dengan adegan langsung, jeda, pengaturan lengkap, cara bermain, kredit, slot simpan), preset grafis, kesulitan, `NusantaraAndroidBuild`, panel debug di HP, pass performa (pool, update rate AI, culling), self-test akhir.
**Checklist**:
- [ ] Menu utama → Mulai Baru (Normal) → intro → bermain; Jeda → Pengaturan → ubah ukuran tombol & sensitivitas, tersimpan setelah restart.
- [ ] `Android/Build APK Uji` → APK terbentuk → pasang di HP → stabil ≥ 30 FPS di preset Sedang (lihat panel debug: ketuk pojok kiri atas 5×).
- [ ] Main 20 menit nonstop tanpa crash; HP tidak terlalu panas pada preset Rendah.

## T12 — Konten Lanjutan (opsional)
Jenglot & Manusia Akar, Ruang Emas Majapahit (K4) + Keris Pusaka + Zirah Emas, Lore #7–#12 bila belum, Rakit Bambu di sungai, Paralayang Kain Batik dari tebing (glider: kecepatan 12 m/s, turun 1,5 m/s, kendali joystick), Menara Pengintai, desa terisi warga di sandbox.

---

## Troubleshooting untuk Pengguna Non-Coder
| Gejala | Yang dilakukan |
|---|---|
| Menu **Nusantara Builder** tidak muncul | Ada error kompilasi. Buka Console, klik baris merah pertama, salin teksnya, kirim ke Codex: "Perbaiki error kompilasi ini: …". Atau: "Baca Editor.log dan perbaiki semua error CS." |
| Objek berwarna **pink/magenta** | Klik `1. BANGUN SELURUH GAME` lagi. Jika tetap: "Ada material pink di [nama objek]. Periksa Hukum H5." |
| Tombol layar tidak merespons di Editor | Minta Codex memeriksa `AddCorrectInputModule` (H3). Alternatif: menu `Utilitas/Set Input ke Both`, lalu restart Unity. |
| Teks UI berupa kotak kosong | Ada emoji/simbol di teks. "Ganti semua simbol non-Latin dengan ikon IconPainter (H4)." |
| Codex berhenti di tengah file | "File X terpotong. Lanjutkan sampai utuh, lalu jalankan verifikasi H12." |
| Codex bilang "project is locked" saat cek kompilasi | Tutup Unity, minta Codex mengulang `Tools/cek_kompilasi`. |
| Game patah-patah di HP | Pengaturan → Grafis → Rendah. Kirim angka FPS & jumlah batch dari panel debug ke Codex. |
| Simpanan rusak | Menu utama → MUAT → tahan slot → Hapus. (File di `persistentDataPath`.) |
| Ingin mengulang dari nol | `Utilitas/Hapus Semua Hasil Generate` → `1. BANGUN SELURUH GAME`. Kode tidak terhapus. |

## Cara Mengirim Error ke Codex (paling efektif)
1. Console → klik baris merah → bagian bawah Console menampilkan detail.
2. Salin 3–10 baris pertama detail tersebut.
3. Tempel ke Codex dengan kalimat: "Saat [aksi yang kamu lakukan], muncul error ini: … Perbaiki tanpa merusak fitur lain, jalankan H12, perbarui PROGRESS."
