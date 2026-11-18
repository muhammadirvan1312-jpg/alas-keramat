# Audit visual ulang G3 — 18 November 2026

Status: belum memenuhi target. Kelulusan Builder, tes logika, dan APK tidak membuktikan kualitas gambar.

## Bukti yang benar-benar tersedia

Render dunia yang dapat ditinjau masih gambar tahap lama (t2_10_hutan_dalam). Preview Deden/Ningsih berasal dari Blender, bukan gameplay Unity G3. Run 44282888486 berhasil membangun APK, tetapi job foto berhenti 35 menit saat pemanasan kamera; daftar 15 gambar berasal dari checkout lama. Klaim sebelumnya bahwa G3 sudah mendapat 15 foto dikoreksi.

| Area | Temuan | Tindakan revisi | Batas verifikasi |
|---|---|---|---|
| Cahaya/warna | Render lama datar, bayangan dan detail kurang meyakinkan; grading bertumpuk memberi warna tambahan pada foto | Netralisasi filter dan split toning; pertahankan siklus waktu/cuaca | Render G3 baru belum tersedia untuk penilaian |
| Material daun | Normal foto sudah diimpor tetapi shader daun tidak membaca BumpMap | Tambah tangent-space normal dua sisi | Shader harus lolos CI |
| Spesies | Sel atlas palem/bambu ditimpa foto pakis; pewarnaan daun foto terlalu selektif hijau | Pulihkan bentuk pelepah asli, pertahankan warna sumber foto | Siluet perlu dibandingkan pada foto |
| Pohon scan | Skala dihitung per komponen objek; trunk/canopy berpotensi berbeda skala dan dasar | Gunakan bounds seluruh scan dan normal inverse transpose | Panggang ulang dan foto diperlukan |
| Lantai hutan | Gambar lama didominasi permukaan rata kosong | Tambah batu kecil dan ranting instanced radius 65m di irisan selatan | Dekorasi saja, tanpa collider/loot; belum seluruh pulau |
| Karakter | Semua piksel pakaian dijadikan grayscale lalu dicelup; kaos putih dan detail bahan ikut hilang | Warnai bagian denim biru dengan mask chroma; pertahankan kaos putih | Bukan pengganti kebaya/beskap khusus yang masih diperlukan |
| Bukti render | Job membangun scene ulang dan memanaskan kamera di origin dengan far clip 2600m; gambar lama ikut terbit | Transfer scene satu SHA, enam shot prioritas, provenance, hapus warisan, manifest per shot | Survei CI 960x540 dengan radius terbatas, bukan tolok ukur FPS HP |
| Animasi/props/UI | Wajah masih generik, gerak manusia belum retarget mocap; props dan bangunan masih bentuk prosedural; T11 baru menambah menu | Dicatat sebagai pekerjaan tersisa, tidak disembunyikan dengan grading | Tidak diklaim sudah diperbaiki pada revisi ini |

## Gerbang penerimaan

1. Kode, Builder idempoten, self-test dan shader lolos pada commit revisi.
2. Sedikitnya enam PNG baru bersumber dari SHA revisi (dua karakter, hutan siang/malam, dua gua). Gambar lama tidak dihitung.
3. Nilai rubrik 19 §7 dari PNG asli: cahaya, kedalaman, material, vegetasi, komposisi, warna, karakter, horor malam, artefak. Tidak memberi skor G3 sebelum bukti tersedia.
4. Target rata-rata 7,5 belum dinyatakan tercapai. Perbaikan aset pahlawan, animasi, kepadatan vegetasi dan FPS HP tetap membutuhkan putaran tersendiri berdasarkan gambar nyata.
