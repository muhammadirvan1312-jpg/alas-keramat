# PROGRESS — Alas Keramat: Sons of Nusantara
File ini DIPERBARUI CODEX di akhir setiap sesi (Hukum H14). Pengguna boleh membaca, tidak perlu mengedit.

## Status Saat Ini
AUTOPILOT_TAHAP_BERIKUTNYA: T2

- Tahap aktif: **T1 selesai ditulis, menunggu AK CI hijau**. Pengerjaan per sesi (Claude).
- Versi Unity yang dipakai: 6000.0.58f2 (image GameCI `unityci/editor:ubuntu-6000.0.58f2-android-3`), URP 17.0.3
- Mode input proyek: default proyek baru; EventSystem memakai AddCorrectInputModule (H3)
- Hasil cek kompilasi terakhir: cek_statis.py 0 masalah; kompilasi Unity diuji AK CI (tidak ada Unity di container)

## Riwayat Tahap
| Tahap | Tanggal | Status | File berubah | Self-test | Catatan |
|---|---|---|---|---|---|
| T0 | — | Dilakukan pengguna | — | — | Proyek URP dibuat, AGENTS.md & docs/ disalin |
| T0-C | 2026-11-17 | Menunggu CI | ProjectSettings/ProjectVersion.txt, Packages/manifest.json, .gitignore, Assets/Editor/NusantaraCI.cs, NusantaraMasterBuilder.cs, NusantaraSelfTest.cs, Tools/ambil_aset.py, Tools/cek_statis.py, lock file kosong, ak-autopilot.yml | 5 cek (root tunggal, Build Settings, kamera, teks, font) | Scene minimal "ALAS KERAMAT - PIPELINE OK", APK pengembangan Mono ARMv7 |
| T1 | 2026-11-17 | Menunggu CI | Core: SOTFContracts, GameServices, GameEvents, GameBootstrap, PhysicsCompat, NusantaraDiagnostics, ProceduralAssetFactory(+IconPainter), ProceduralAudioSynth(+Dsp, +Resep). Editor: NusantaraMasterBuilder + BuilderSteps.Proyek/Aset/Scene, NusantaraSelfTest. Tools: cek_kompilasi.bat/.sh | 17 cek (idempoten, layer/tag, URP semua Quality, 14 tekstur, 6 material, shader, font, 8 WAV, scene, UI) | Scene uji: tanah 200x200, kubus & 3 bola PBR, judul "ALAS KERAMAT - FONDASI OK", 5 tombol suara |

Catatan spesifikasi: v3.0 (tahap T1–T11 + T12 opsional; T9 = Teror & Dunia Hidup).

## Keputusan Teknis Baru
- Autopilot mode PR-dulu; tahap baru hanya dikirim ke @codex bila variabel repo `AUTOPILOT_AGEN = codex`. Default: dikerjakan per sesi, Autopilot hanya merge PR hijau + rilis APK.
- PlayerSettings Android diatur NusantaraCI: `com.alaskeramat.game`, Mono, ARMv7, min API 26 (IL2CPP/ARM64 ditunda ke T11).
- Lock file aset CC0 & mocap masih kosong; diisi mulai T2 (docs/gdd/19).
- T1: aset URP dibuat Builder di `Assets/Generated/Render/` (UniversalRendererData + PostProcessData bawaan paket) dan dipasang ke default + semua level Quality.
- T1: tombol uji suara memanggil `ProceduralAudioSynth.PlayByName(string)` lewat persistent listener (tanpa sambungan manual).
- T1: ikon 20 buah (kayu, batu, ranting, daun, tali, makanan, air, golok, kapak, tombak, busur, panah, obor, senter, peta, buku, api, nyawa, stamina, kewarasan) di `Assets/Generated/Icons/`.

## Masalah Terbuka / Belum Lulus Uji
_(Daftar checklist yang belum lulus beserta dugaan penyebab.)_

## Tahap Berikutnya
_(Tulis tahap berikutnya dan hal yang perlu diperhatikan.)_
