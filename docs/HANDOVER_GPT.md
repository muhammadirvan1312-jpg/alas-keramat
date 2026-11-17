# Serah Terima ke ChatGPT (dari Claude, 18 Nov 2026)

Mulai sekarang ChatGPT mengerjakan proyek ini sendiri: menulis kode, membuka PR, membaca hasil CI, dan memperbaiki error. Pengguna non-coder, bicara santai (gua/lu), mau hasil bagus tapi hemat.

## 1. Status sekarang
- Tahap SELESAI & ter-merge ke `main`: T1-T6c, T7a (tempur inti + Sekte), T7b (hantu, raid, totem), T7c (gore), T7d (senjata jarak, stealth), T8 (Kang Deden 15 perintah + 20 catatan, Ningsih + Trust), T9 (Sutradara Teror 25 kejadian, penjadwal kejadian dunia). PR terakhir #23.
- Rilis APK terakhir: lihat GitHub Releases (`uji-<tanggal>`); build `main` setelah PR #23 membuat rilis baru.
- Tahap berikutnya: **T10**, lalu **T11**, lalu **fase poles** (lihat §5). Baris `AUTOPILOT_TAHAP_BERIKUTNYA:` di `docs/PROGRESS.md`.
- Daftar kelas yang SUDAH ada + keputusan desain + hal yang ditunda: tabel riwayat & "Masalah Terbuka" di `docs/PROGRESS.md`. Perluas kelas yang ada, jangan buat ulang.

## 2. Cara kerja satu tahap
Ikuti `docs/GPT_TAHAP.md` (langkah 1-10) dan `docs/KONTRAK_GPT.md` §1-4. Ringkas:
1. Cabang `tahap-T10` dari `main`. Runtime `Assets/Scripts/<Modul>/`, editor `Assets/Editor/`.
2. Builder: `Assets/Editor/BuilderSteps.<Nama>.cs` (partial `NusantaraMasterBuilder`), daftarkan `Langkah("7t. ...", 0.97f, Langkah<Nama>)` di `NusantaraMasterBuilder.cs` setelah langkah terakhir (7s Teror).
3. Self-test: buat `Assets/Editor/PemeriksaanT10.cs` dengan `public static void Jalankan(System.Action<string,bool> cek)` dan panggil di `NusantaraSelfTest.cs` sebelum `cek("HUDInfo ada"` (contoh: `PemeriksaanT9.Jalankan(cek);`).
4. Perbarui `docs/PROGRESS.md` (baris riwayat + tahap berikutnya).
5. PR ke `main`. CI **AK CI** (GameCI, 20-30 menit). Hijau -> **AK Autopilot** squash-merge + rilis APK otomatis.

## 3. Membaca hasil CI (tanpa akses log Actions)
CI menulis hasil ke cabang **`ci-hasil`**, folder `<nama-cabang>/` (mis. `ci-hasil:tahap-T10/`):
- `ringkasan.md` / `LAPORAN_CI.md`: LULUS/GAGAL per langkah (pengaturan pemain, builder, self-test, build APK) + baris `error CS`.
- `nusantara_selftest.txt`: setiap cek `[LULUS]/[GAGAL]`.
- `nusantara_build_report.txt`, `foto.log`, `foto_kualitas/*.png` (foto adegan untuk dinilai, rubrik `docs/gdd/19` §7).
Bila merah: baca file itu, perbaiki di cabang yang sama, push lagi (run lama otomatis dibatalkan).

## 4. Aturan yang sering bikin CI merah (`Tools/cek_statis.py`)
- Nama tipe (termasuk tipe bersarang) unik di seluruh proyek. Sudah terpakai juga: `TipeTeror, Fase, Intensitas, Konteks, KeadaanSekte, PoseTempur, JenisJarak, KegiatanSekte, AksiNingsih` dll. Cek dengan grep sebelum menamai.
- Kata `Animator` dan kata-kata nav-mesh dilarang, termasuk di komentar. Tanpa `// TODO`.
- Kelas MonoBehaviour di file bernama sama. Tanpa `??`/`?.` pada objek Unity. C# <= 9. <= 900 baris per file (pecah `partial`).
- Input keyboard hanya di dalam `#if ENABLE_LEGACY_INPUT_MANAGER`.
- Event C# tidak boleh di-Invoke dari luar kelasnya (buat method `Picu...()`).
- Nama method tidak boleh sama dengan nama kelas bersarang di kelas yang sama.
- Cek nama anggota yang benar sebelum dipakai (contoh salah yang pernah terjadi: `v.HP` -> `v.Health`, `SurfaceType.Daun` -> `DaunKering`).
- JANGAN masukkan shader `Universal Render Pipeline/*` ke Always Included Shaders (build APK gagal).

## 5. Fakta teknis penting
- Dunia v5 2x2 km: `WorldTerrainGenerator.BatasDunia = 1000`. Koordinat di dokumen 03/04 dikali 2. Desa (540,196), Sawit (-520,240), heli/Deden (24,-440), sarang Ningsih (200,-350), mulut gua (256,-276) & (0,694), titik gali bunker (-560,290).
- Pathfinding: `SurfaceGrid` 500x500 sel 4 m (dipanggang Builder ke `Assets/Generated/Nav/SurfaceGrid.bytes`), `GridPathfinder.CariOtomatis` = A* bertingkat (graf wilayah + A* halus 100 m pertama). Jarak jauh jangan pakai A* halus penuh (anggaran 4.000 node).
- Gore: satu sumber `GoreSystem.Tingkat` (PlayerPrefs `ak_gore`). Layer dari `GameLayers` (Player 7, Companion 8, Enemy 9, Interactable 10, Tree 11, LogPhysics 12, Structure 13, Projectile 16).
- Interaksi: `IInteractable` di layer Interactable (prompt KAPITAL Bahasa Indonesia, mendukung tahan via HoldDuration). Companion turunan `CompanionDasar`. Teror lewat `PenjadwalTeror` (logika murni, mudah diuji).
- Musuh/hantu: `NusantaraEnemyAndMutantAI` (13 arketipe di `StatMusuh`), `HantuAI` (Pocong/Kuntilanak/Banaspati), `EnemyDirector` (+ `.Malam`, `.Jadwal`; ISaveable "musuh"). Paku Kuning sudah dipakai `HantuAI` (TANCAPKAN PAKU).

## 6. Rencana T10 (spesifikasi: `docs/tech/12` bagian T10, `docs/gdd/04` semua, `06` §9-11, `03` §10, `10` §5, `17` §6)
Pecah jadi beberapa PR kecil agar mudah lulus CI:
- **T10a** Gua & Bunker UG1-UG3 di Y=-80 (teleport fade di mulut gua), gerbang: gali cangkul 4x -> palka, pintu kartu, terowongan selam (Alat Selam), zipline/tembak tali di jurang, koridor angin perlu obor. Self-test: 3 kompleks punya pintu masuk/keluar valid.
- **T10b** Misi M05-M13 dengan gerbang 04 §4 (`QuestManager` sudah punya M01-M04 + `Progres.m05`; sambungkan M05 = semua perintah Deden terbuka, M06 = Trust Ningsih >= 31), 12 lore (#1-#6 teks persis 04 §6.1) + UI baca + jurnal, Trust +10 Lore #8, Trust +3 tidur di Saung. Self-test: 13 misi berurutan (simulasi flag), 12 lore tidak kosong.
- **T10c** Boss Genderuwo 3 fase (06 §10) + Ki Rogo, altar 3 lubang -> siege 60 detik. Gelombang 06 §11: 0s 4 Abdi + 1 Pemimpin; 15s 3 Pocong; 25s 2 Wanita + 1 Buto; 35s Kuntilanak; 40s bayangan Ki Rogo; 45s 2 Banaspati + 2 Pocong; 60s semua jadi abu. Self-test: penjadwal gelombang murni.
- **T10d** Cutscene intro + teks boss/ending, heli SAR, layar pilihan nasib -> Ending A/B -> statistik, sandbox setelah B, musik, Golok Api Pusaka, Peta Kertas + kabut. Tambahan v3: Rendra (Mutan Jemari UG2 R2), misi sampingan S01-S04.

## 7. T11 lalu fase poles
- **T11**: menu utama/jeda/pengaturan (gore, intensitas horor, headset, getar, kesulitan, ukuran tombol, sensitivitas), slot simpan, preset grafis, panel debug HP (ketuk pojok kiri atas 5x), pass performa (pool, update rate AI, culling), target >= 30 FPS preset Sedang.
- **Poles** (daftar lengkap di "Masalah Terbuka" PROGRESS): karakter MakeHuman, tekstur & shader AK/Blood + AK/Wet, G3 Irisan Vertikal, kejadian teror terskrip yang ditunda, musuh Kuyang/Tuyul/Genderuwo Hutan/Penyerupa/Hantu Air, satwa baru, lokasi set-piece. Catatan foto CI: adegan malam (`t7_02_pocong_kuntilanak_malam`) hampir hitam total, perlu cahaya bulan/eksposur.

## 8. Batasan dari pengguna
- Pengguna mengetik sendiri password/login/2FA. Jangan pernah menulis token/password/lisensi ke repo; rahasia hanya di GitHub Secrets.
- Lisensi Unity Personal (`UNITY_LICENSE`) berakhir 16 Des 2026: pengguna memperpanjang lewat Unity Hub lalu paste ulang ke secret.
- Laporan ke pengguna singkat, Bahasa Indonesia santai: tahap apa selesai, link rilis APK, apa yang bisa dicoba di HP.
