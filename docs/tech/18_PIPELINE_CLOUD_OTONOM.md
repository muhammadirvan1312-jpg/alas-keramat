# 18 — PIPELINE CLOUD OTONOM: CODEX CLOUD + GITHUB ACTIONS (GameCI) → APK DI HP
Dibaca di SETIAP tugas Codex Cloud. Dokumen ini menggantikan T0 & lingkaran H12 lokal bila Codex berjalan di cloud.

---

## 1. Gambaran
```
 [Issue "AUTOPILOT"] --@codex Kerjakan TAHAP Tn--> [Codex Cloud: tulis kode + cek statis] --PR--> [GitHub]
        ^                                                                                        |
        |                                                                         [AK CI: GameCI Unity]
        |                                                     kompilasi → Builder 1-klik → Self-Test → Foto → APK
        |                                                                                        |
        |        merah: "@codex perbaiki" (maks 5x)  <---- [AK Autopilot] <---- hasil CI ---------+
        +---- hijau: merge → CI main hijau → rilis "APK uji" → perintah tahap berikutnya
 Pengguna: unduh APK uji di HP dari halaman Releases, main, tulis kesan di docs/UMPAN_BALIK.md
```
- Container Codex Cloud **tidak punya Unity**. Hakim satu-satunya untuk "berhasil" adalah **AK CI** (GameCI `unity-builder` di GitHub Actions).
- File `.github/workflows/ak-ci.yml` dan `ak-autopilot.yml` disediakan sebagai titik awal; Codex wajib menguji & memperbaikinya di Tahap **T0-C**.

## 2. Persiapan Sekali oleh Pengguna (±60–90 menit)
| # | Langkah | Bisa dari HP? |
|---|---|---|
| 1 | Buat akun GitHub, lalu repo baru `alas-keramat`. **Public** = menit GitHub Actions gratis tanpa batas (kode terlihat publik). **Private** = 2.000 menit/bulan gratis (±50–80 kali build Unity). | Ya (browser) |
| 2 | Unggah isi paket ini (AGENTS.md, BACA_DULU.md, folder `docs/` dan `.github/`) ke repo: *Add file → Upload files*. Folder `.github/workflows` harus ikut. | Ya |
| 3 | Buat akun Unity (gratis). Ambil **file lisensi Personal (.ulf)**: di komputer mana pun pasang Unity Hub → login → *Preferences → Licenses → Add → Get a free personal license*. File ada di `C:\ProgramData\Unity\Unity_lic.ulf` (Windows) atau `/Library/Application Support/Unity/Unity_lic.ulf` (Mac). Bila file tidak muncul, ikuti panduan resmi GameCI bagian *Activation → Personal license* (ada langkah memastikan file dibuat). Satu-satunya langkah yang perlu komputer (boleh pinjam/warnet; tidak perlu memasang Unity Editor). | Tidak |
| 4 | Repo → *Settings → Secrets and variables → Actions → Secrets*: `UNITY_LICENSE` (seluruh isi file .ulf), `UNITY_EMAIL`, `UNITY_PASSWORD`, `AUTOPILOT_PAT` (lihat 5). | Ya |
| 5 | GitHub → *Settings → Developer settings → Fine-grained tokens*: token hanya untuk repo ini dengan izin Contents, Issues, Pull requests = Read & write; Actions = Read. Simpan sebagai `AUTOPILOT_PAT`. | Ya |
| 6 | Buat issue berjudul `AUTOPILOT`. Lalu *Variables*: `AUTOPILOT` = `ON`, `AUTOPILOT_ISSUE` = nomor issue tadi. | Ya |
| 7 | *Settings → General*: centang **Allow auto-merge**. *Settings → Actions → General → Workflow permissions*: **Read and write**. | Ya |
| 8 | ChatGPT → Codex: hubungkan GitHub, pilih repo, buat **Environment**: setup script (§3), **Agent internet access = On** dengan allowlist §3. | Ya |
| 9 | Mulai: repo → *Actions → AK Autopilot → Run workflow* dengan `paksa_tahap = T0-C`. Setelah itu berjalan sendiri. | Ya |
Bila Codex tidak otomatis membuka pull request dari komentar issue, buka tugasnya di aplikasi ChatGPT → Codex lalu tekan **Create PR** (satu ketukan per tahap).

## 3. Environment Codex Cloud
**Setup script** (berjalan dengan internet, sebelum agen bekerja):
```bash
#!/usr/bin/env bash
set -e
python3 -m pip install --quiet requests pillow numpy
if [ -f Tools/ambil_aset.py ]; then python3 Tools/ambil_aset.py --semua --keluar Assets/ThirdParty || echo "[AK] unduh aset dilewati"; fi
if command -v dotnet >/dev/null 2>&1; then echo "[AK] dotnet tersedia untuk cek sintaks C#"; fi
```
**Allowlist internet agen** (minimal): `api.polyhaven.com`, `dl.polyhaven.org`, `ambientcg.com`, `mocap.cs.cmu.edu`, `raw.githubusercontent.com`, `github.com`. Bila sebuah unduhan dialihkan ke domain lain, Codex mencatat domainnya di `docs/PROGRESS.md` agar pengguna menambahkannya.
Rahasia (secret) TIDAK pernah dimasukkan ke environment Codex; lisensi Unity hanya ada di GitHub Secrets.

## 4. Tahap T0-C — Proyek dari Nol (dikerjakan Codex, sebelum T1)
1. Pilih **Unity 6 LTS** terbaru yang punya image GameCI Android (`unityci/editor:ubuntu-<versi>-android-3`); tulis `ProjectSettings/ProjectVersion.txt`.
2. `Packages/manifest.json`: hanya `com.unity.render-pipelines.universal` (versi cocok dengan editor), `com.unity.ugui`, plus modul bawaan (`com.unity.modules.*` yang dibutuhkan: physics, audio, ui, imgui, jsonserialize, terrain, terrainphysics, particlesystem, animation untuk tipe data saja, unitywebrequest bila perlu). Tidak ada paket lain (H1).
3. `.gitignore` Unity standar + `Assets/ThirdParty/` (aset diunduh, tidak dikomit), `Builds/`, `Library/`, `Temp/`, `Logs/*` kecuali `Logs/ci/LAPORAN_CI.md` & `Logs/foto_kualitas/`.
4. `Assets/Editor/NusantaraCI.cs` (namespace `AlasKeramat.EditorTools`), method statis `BuildAndTest()`:
   - Baca argumen `-customBuildPath` (dari GameCI) dan `-akLaporan` (folder laporan).
   - Pasang `Application.logMessageReceived` → tulis `Logs/ci/editor_ringkas.log` (hanya Warning/Error/Exception).
   - Langkah berurutan, masing-masing try/catch, hasil ke `Logs/ci/ringkasan.md` sebagai baris `[LULUS] nama` / `[GAGAL] nama: alasan`: (a) Builder 1-klik bila kelasnya sudah ada (via reflection agar T0-C bisa jalan sebelum T1), (b) `NusantaraSelfTest.RunAll` bila ada, (c) **Foto Kualitas** (15 §8) — bila rendering tidak tersedia di CI, tulis `[LEWAT] foto: tanpa GPU` (bukan gagal), (d) build APK pengembangan ke `-customBuildPath` dengan scene dari Build Settings.
   - `EditorApplication.Exit(0)` bila semua LULUS/LEWAT, `Exit(1)` bila ada GAGAL.
5. `Tools/ambil_aset.py` (19 §4), `Tools/aset_cc0.lock.json` & `Tools/mocap.lock.json` (boleh kosong dulu), `Tools/cek_statis.py` (§5).
6. Buat `docs/UMPAN_BALIK.md` kosong dengan judul.
7. Scene minimal + Build Settings agar APK pertama terbentuk (layar "ALAS KERAMAT — PIPELINE OK").
8. Tandai `AUTOPILOT_TAHAP_BERIKUTNYA: T1` di `docs/PROGRESS.md` setelah CI hijau.

## 5. Aturan Kerja Codex di Cloud (melengkapi H12)
1. **Jangan pernah menyatakan "sudah dikompilasi"** — tidak ada Unity di container. Yang boleh diklaim hanya hasil cek statis; kelulusan sebenarnya = CI hijau.
2. Jalankan `python3 Tools/cek_statis.py` sebelum membuka PR. Isinya minimal: larangan API H1 (`TMPro`, `NavMesh`, `Animator`, `using UnityEngine.InputSystem`, `Cinemachine`), emoji/simbol non-Latin di string C#, file > 900 baris, kurung kurawal tidak seimbang, `// TODO`, `NotImplementedException`, nama class ganda, `FindObjectOfType`, `rb.velocity` di luar `PhysicsCompat`, shader tanpa `Fallback`. Bila `dotnet` ada: parse sintaks semua `.cs` dengan Roslyn (cek sintaks, bukan referensi).
3. Di awal setiap tugas baca: `docs/PROGRESS.md`, `docs/UMPAN_BALIK.md` (masukan pengguna dari HP — WAJIB ditindaklanjuti atau dijawab di PROGRESS), dan bila tugas perbaikan: `Logs/ci/LAPORAN_CI.md`.
4. **Satu PR per tahap**. Bila tahap terlalu besar untuk satu tugas, pecah: selesaikan bagian yang bisa lulus CI, tulis `AUTOPILOT_TAHAP_BERIKUTNYA: T5b` (lanjutan), dan jelaskan sisa pekerjaan di PROGRESS.
5. Perubahan pada workflow CI hanya untuk memperbaiki pipeline, dicatat di PROGRESS.
6. Jangan mengunduh aset di luar lock file; menambah aset = memperbarui lock file lewat `Tools/ambil_aset.py --cari ... --tambah`.

## 6. Gerbang Kualitas
| Gerbang | Kapan | Syarat lulus |
|---|---|---|
| G1 Teknis | Setiap PR | Kompilasi, Builder, Self-Test LULUS; APK terbentuk |
| G2 Visual | Akhir T2, T4, T6, T9, T10, T11 | Foto Kualitas dinilai dengan rubrik 19 §7; rata-rata ≥ target tahap. Bila kurang: tahap yang sama diulang dengan akhiran `-VISUAL` (maks 3 putaran) |
| G3 Irisan Vertikal | Setelah T7 | Zona 1 + Lapangan Kemah + malam pertama lengkap & dipoles: G1 lulus, skor visual ≥ 7,5, umpan balik pengguna di UMPAN_BALIK.md ditanggapi. Baru lanjut T8 |
| G4 Main di HP | Setiap rilis "APK uji" | Pengguna boleh menulis kesan di `docs/UMPAN_BALIK.md` (edit lewat browser HP); tahap berikutnya wajib menindaklanjuti |
Bila foto tidak bisa dibuat di CI (tanpa GPU), G2 memakai foto dari HP: APK uji punya tombol debug **FOTO** (panel debug, 11 §12) yang menyimpan 12 foto "money shot" (19 §8) ke galeri; pengguna mengunggahnya ke folder `Logs/foto_kualitas/` lewat GitHub web.

## 7. Biaya & Batas
- GitHub Actions: build Unity Android ±20–40 menit/kali. Private repo: 2.000 menit/bulan (paket Free). Hemat: cache Library aktif, artefak disimpan 7 hari.
- Codex Cloud memakai kuota paket ChatGPT pengguna; satu tahap bisa butuh beberapa tugas (pembuatan + perbaikan).
- Autopilot berhenti sendiri setelah 5 perbaikan gagal dalam satu PR (label `butuh-bantuan`).

## 8. Bila Macet
| Gejala | Tindakan |
|---|---|
| CI gagal "license" / "activation" | Perbarui isi `UNITY_LICENSE` dengan file .ulf baru (lisensi Personal bisa kedaluwarsa) |
| CI gagal "No space left on device" | Codex menambah langkah pembersihan disk / mengurangi ukuran aset |
| Codex tidak merespons komentar autopilot | Pastikan `AUTOPILOT_PAT` milik akun yang terhubung ke Codex; atau ketik sendiri `@codex lanjutkan tahap berikutnya` di issue AUTOPILOT |
| Label `butuh-bantuan` | Baca komentar terakhir PR; balas di PR: `@codex ...` dengan petunjuk, atau tutup PR & jalankan ulang tahap |
| Ingin menghentikan | Ubah variabel `AUTOPILOT` menjadi `OFF` |
