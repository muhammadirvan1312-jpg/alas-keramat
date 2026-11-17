# PROGRESS — Alas Keramat: Sons of Nusantara
File ini DIPERBARUI CODEX di akhir setiap sesi (Hukum H14). Pengguna boleh membaca, tidak perlu mengedit.

## Status Saat Ini
AUTOPILOT_TAHAP_BERIKUTNYA: T4

- Tahap aktif: **T2a selesai ditulis (dunia 2x2 km), menunggu AK CI**. T2 dipecah: T2a medan/air/waktu/langit/tur, T2b zona & props & vegetasi & cuaca, T2c ambien audio v5 + shader AK.
- Versi Unity yang dipakai: 6000.0.58f2 (image GameCI `unityci/editor:ubuntu-6000.0.58f2-android-3`), URP 17.0.3
- Mode input proyek: default proyek baru; EventSystem memakai AddCorrectInputModule (H3)
- Hasil cek kompilasi terakhir: cek_statis.py 0 masalah; kompilasi Unity diuji AK CI (tidak ada Unity di container)

## Riwayat Tahap
| Tahap | Tanggal | Status | File berubah | Self-test | Catatan |
|---|---|---|---|---|---|
| T0 | — | Dilakukan pengguna | — | — | Proyek URP dibuat, AGENTS.md & docs/ disalin |
| T0-C | 2026-11-17 | Menunggu CI | ProjectSettings/ProjectVersion.txt, Packages/manifest.json, .gitignore, Assets/Editor/NusantaraCI.cs, NusantaraMasterBuilder.cs, NusantaraSelfTest.cs, Tools/ambil_aset.py, Tools/cek_statis.py, lock file kosong, ak-autopilot.yml | 5 cek (root tunggal, Build Settings, kamera, teks, font) | Scene minimal "ALAS KERAMAT - PIPELINE OK", APK pengembangan Mono ARMv7 |
| T1 | 2026-11-17 | Menunggu CI | Core: SOTFContracts, GameServices, GameEvents, GameBootstrap, PhysicsCompat, NusantaraDiagnostics, ProceduralAssetFactory(+IconPainter), ProceduralAudioSynth(+Dsp, +Resep). Editor: NusantaraMasterBuilder + BuilderSteps.Proyek/Aset/Scene, NusantaraSelfTest. Tools: cek_kompilasi.bat/.sh | 17 cek (idempoten, layer/tag, URP semua Quality, 14 tekstur, 6 material, shader, font, 8 WAV, scene, UI) | Scene uji: tanah 200x200, kubus & 3 bola PBR, judul "ALAS KERAMAT - FONDASI OK", 5 tombol suara |

| T2a | 2026-11-17 | Menunggu CI | Core: WorldTerrainGenerator (+Biome), TimeOfDayClock, URPAtmosphereAndWeather; UI: TurPratinjau; Editor: BuilderSteps.Dunia/Scene (baru), NusantaraFoto (9 shot dunia + pemanasan), SelfTest T2a | 30+ cek (16 tile, 6 lapisan, laut, sungai, 6 cek tinggi, 9 bioma, jam, atmosfer, tur) | Dunia v5 2x2 km: 16 Unity Terrain 1025², splat 6 lapis per bioma+lereng, laut & 2 sungai, siang-malam & Maghrib merah, tur kamera 12 zona (waktu x20) |
| T2b | 2026-11-17 | Menunggu CI | Core: ProcPohon (11 jenis x 2 varian x 2 LOD + atlas daun), URPAtmosphereAndWeather.Cuaca (7 cuaca, hujan, kilat, guruh), resep Guruh; Building: VegetationData, VegetationManager (RenderMeshInstanced); Editor: BuilderSteps.Vegetasi, foto +3 shot | + cek pohon 8k-45k, semak >= 10k, 7 cuaca | Hutan v5 per bioma & zona baru, sawit hangus grid, cuaca dinamis |
| T2c | 2026-11-17 | Menunggu CI | Core: ProcBangunan (rumah panggung, heli, mercusuar, gedung & cerobong pabrik, mulut gua, altar); Editor: BuilderSteps.Landmark; perbaikan visual dari foto T2a: alpha albedo = smoothness rendah (tanah tidak mengilap), kabut 3x lebih tipis, tint langit | + cek landmark >= 35 | Beringin Agung 28 m, heli rusak, 6 rumah desa, 22 rumah kampung, mercusuar, pabrik gula + cerobong 38 m, 2 mulut gua, altar |

| T2d | 2026-11-17 | Menunggu CI | GPT (TUGAS 02): ProceduralAudioSynth.Ambien (23 resep: 10 bed loop + 13 one-shot), AmbienceDirector (bed per bioma x waktu silang 4 s, emitter titik, jadwal alam, gelombang hening, burung kaget, snapshot). Claude: BuilderSteps.Ambien (emitter per bioma + titik khusus pabrik/kampung/Wingit), bake WAV ambien (bed Vorbis), hook Generate | + cek director, emitter >= 1000, 23 klip, 4 cek PilihBed | Dunia bersuara: jangkrik, tokek, kedasih, burung hantu, tonggeret, katak rawa, rantai pabrik, radio statis kampung, gamelan samar Alas Wingit |

| T2e | 2026-11-17 | Menunggu CI | GPT (TUGAS 04): shader AlasKeramat/Daun (angin, kibasan, cahaya tembus, variasi warna, instancing), /Kulit (PBR + goyang sinkron), /Air (riak prosedural, kedalaman, fresnel, buih tepi), AKAngin (angin global ikut cuaca). Claude: BuilderSteps.Shader (konversi material, fallback URP/Lit), air laut & sungai tanpa bayangan, URP depth texture aktif | + cek 3 shader tanpa error, material daun & sungai pakai shader AK, AKAngin | Hutan bergoyang ditiup angin (kencang saat badai), daun tembus cahaya, air beriak dengan buih |

| T3a | 2026-11-17 | Menunggu CI | Player: AndiFPSController (jalan/lari/jongkok/lompat + coyote, berenang & menyelam dasar, napas, damage jatuh via IDamageable, kamera pitch/yaw + head bob, keyboard/mouse hanya ENABLE_LEGACY_INPUT_MANAGER); UI: ZonaJoystick (mengambang, sprint LARI), ZonaGeser, TouchControls, HUDInfo (HARI/jam/label waktu/cuaca + nama zona); Editor: BuilderSteps.Andi (spawn (-4,-424) dekat heli, kamera Andi, tur dimatikan, sprite UI lingkaran) | + 11 cek (Andi, spawn, MainCamera, 1 AudioListener, tur mati, kontrol, handler tombol, HUDInfo, kecepatan 09 §3, sprint, label waktu) | Pertama kali bisa jalan-jalan di hutan dari HP |

| T3b | 2026-11-17 | Menunggu CI | Inventory: TikarPandan3DInventory (model data, Maks per item, simpan JSON), WadahItem (koper & benda lepas, tutup berputar); Player: InteraksiAndi (SphereCast 2,5 m, MINUM AIR), TanganKiri (obor + korek, cahaya berkedip), IVitalAndi (kontrak untuk vitals GPT TUGAS 03); UI: tombol TANGAN kontekstual & OBOR, ToastHUD, RadarGPS (150 m, U berputar, aktif dengan GPS); Editor: BuilderSteps.Interaksi (3 koper 03 §9 x2, puing heli) | + cek 3 koper sesuai isi, Maks inventaris, radar/toast/interaksi, arah radar | Buka koper, ambil golok/GPS/obor, radar menyala, obor menerangi malam, minum di sungai |

| T3c | 2026-11-17 | Menunggu CI | Core: AKPascaProses (eksposur/saturasi/suhu/vignette mengikuti jam & cuaca: siang hangat, Maghrib merah, malam biru gelap, badai kelabu); Editor: BuilderSteps.Pasca (VolumeProfile ACES, color adjustments, white balance, bloom, vignette, split-toning, lift; pasca-proses + FXAA di semua kamera & kamera foto) | + cek volume ACES & bloom, kamera utama pasca-proses, malam lebih gelap & dingin | Warna sinematik, tidak kusam |

| T3d | 2026-11-17 | Menunggu CI | Player: AndiVitalsAndSanity (+.Tubuh): HP + plafon, stamina + cap (sprint/lompat/renang, habis -> tak bisa lari sampai 25), basah & dingin, haus, istirahat, kewarasan (malam gelap, kabut darah, api, cerah), suhu tubuh 24 §1.1 (kedinginan/hipotermia/kepanasan/heatstroke), gizi 4 cadangan, air mentah 35% diare, daging mentah keracunan, kunyit/jamu mengobati, pingsan -> bangun di heli; IDamageable/IVitalAndi/ISaveable; UI: HUDVital (5 bar, plafon, beku, kedip kritis, pendar kewarasan, suhu & basah) | + cek laju 60 s (haus 5,1 / kenyang 3,3 / istirahat 2,1), plafon HP, suhu udara | Bar vital hidup; minum sungai menaikkan haus |

| T3e | 2026-11-17 | Menunggu CI | Core: ProceduralAudioSynth.Foley (langkah 6 permukaan x 3 variasi, napas lelah, detak jantung); Player: FoleyAndi (langkah per jarak & permukaan, mendarat, berenang, napas saat stamina < 30, jantung saat kewarasan/HP < 25); UI: SubtitleSystem (2 baris, antrean, monolog pembuka Andi), SabukCepat (4 slot makanan/obat, ketuk = konsumsi). CI: laporan & foto ke cabang ci-hasil | + cek 20 klip foley, permukaan, komponen, lama subtitle | Langkah terdengar beda di pasir/daun/air, Andi bermonolog saat sadar, bisa makan/minum dari sabuk |

Catatan spesifikasi: v3.0 (tahap T1–T11 + T12 opsional; T9 = Teror & Dunia Hidup).

## Keputusan Teknis Baru
- **v5 (2026-11-17)**: pengguna memilih kualitas maksimal. Dokumen baru `docs/gdd/20` (dunia 2×2 km, 12 zona, UG1–6, tekstur 2K/4K, preset Ultra, mekanik baru) & `docs/gdd/21` (audio v5). Perkiraan APK 1,0–1,6 GB (+0,1–0,2 GB karakter). Ditambah `docs/gdd/22` (karakter realistis MakeHuman/MPFB2 CC0) & `docs/gdd/23` (cerita 6 babak, 42 cutscene, 3 ending) atas pilihan pengguna, serta `docs/gdd/24` (survival mendalam: suhu tubuh, gizi, air, penyakit, luka, api, hama, bahaya alam, navigasi, mode Sengsara & Pantang Mundur). Tahap T1 yang sedang di PR #3 tetap; penyesuaian v5 untuk T1 (preset grafis 4 tingkat, pool 48 suara, tekstur 1K–2K) dikerjakan di awal T2.
- Autopilot mode PR-dulu; tahap baru hanya dikirim ke @codex bila variabel repo `AUTOPILOT_AGEN = codex`. Default: dikerjakan per sesi, Autopilot hanya merge PR hijau + rilis APK.
- PlayerSettings Android diatur NusantaraCI: `com.alaskeramat.game`, Mono, ARMv7, min API 26 (IL2CPP/ARM64 ditunda ke T11).
- Lock file aset CC0 & mocap masih kosong; diisi mulai T2 (docs/gdd/19).
- T1: aset URP dibuat Builder di `Assets/Generated/Render/` (UniversalRendererData + PostProcessData bawaan paket) dan dipasang ke default + semua level Quality.
- Kelas MonoBehaviour/ScriptableObject WAJIB di file bernama sama dengan kelasnya (VegetationManager.cs, VegetationData.cs) agar bisa diserialisasi Unity; nama file manifest 11 menyesuaikan.
- T1: tombol uji suara memanggil `ProceduralAudioSynth.PlayByName(string)` lewat persistent listener (tanpa sambungan manual).
- T1: ikon 20 buah (kayu, batu, ranting, daun, tali, makanan, air, golok, kapak, tombak, busur, panah, obor, senter, peta, buku, api, nyawa, stamina, kewarasan) di `Assets/Generated/Icons/`.

## Masalah Terbuka / Belum Lulus Uji
_(Daftar checklist yang belum lulus beserta dugaan penyebab.)_

## Tahap Berikutnya
_(Tulis tahap berikutnya dan hal yang perlu diperhatikan.)_
