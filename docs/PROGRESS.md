# PROGRESS — Alas Keramat: Sons of Nusantara
File ini DIPERBARUI CODEX di akhir setiap sesi (Hukum H14). Pengguna boleh membaca, tidak perlu mengedit.

## Status Saat Ini
AUTOPILOT_TAHAP_BERIKUTNYA: T1

- Tahap aktif: **T0-C selesai ditulis, menunggu AK CI hijau**. Pengerjaan per sesi (Claude); Codex Cloud tidak bisa push/membuat PR di akun ini.
- Versi Unity yang dipakai: 6000.0.58f2 (image GameCI `unityci/editor:ubuntu-6000.0.58f2-android-3`), URP 17.0.3
- Mode input proyek: default proyek baru; EventSystem memakai AddCorrectInputModule (H3)
- Hasil cek kompilasi terakhir: cek_statis.py 0 masalah; kompilasi Unity diuji AK CI (tidak ada Unity di container)

## Riwayat Tahap
| Tahap | Tanggal | Status | File berubah | Self-test | Catatan |
|---|---|---|---|---|---|
| T0 | — | Dilakukan pengguna | — | — | Proyek URP dibuat, AGENTS.md & docs/ disalin |
| T0-C | 2026-11-17 | Menunggu CI | ProjectSettings/ProjectVersion.txt, Packages/manifest.json, .gitignore, Assets/Editor/NusantaraCI.cs, NusantaraMasterBuilder.cs, NusantaraSelfTest.cs, Tools/ambil_aset.py, Tools/cek_statis.py, lock file kosong, ak-autopilot.yml | 5 cek (root tunggal, Build Settings, kamera, teks, font) | Scene minimal "ALAS KERAMAT - PIPELINE OK", APK pengembangan Mono ARMv7 |

Catatan spesifikasi: v3.0 (tahap T1–T11 + T12 opsional; T9 = Teror & Dunia Hidup).

## Keputusan Teknis Baru
- Autopilot mode PR-dulu; tahap baru hanya dikirim ke @codex bila variabel repo `AUTOPILOT_AGEN = codex`. Default: dikerjakan per sesi, Autopilot hanya merge PR hijau + rilis APK.
- PlayerSettings Android diatur NusantaraCI: `com.alaskeramat.game`, Mono, ARMv7, min API 26 (IL2CPP/ARM64 ditunda ke T11).
- Lock file aset CC0 & mocap masih kosong; diisi mulai T2 (docs/gdd/19).

## Masalah Terbuka / Belum Lulus Uji
_(Daftar checklist yang belum lulus beserta dugaan penyebab.)_

## Tahap Berikutnya
_(Tulis tahap berikutnya dan hal yang perlu diperhatikan.)_
