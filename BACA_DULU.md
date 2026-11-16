# BACA DULU — Paket Spesifikasi "Alas Keramat" v4.0


## MULAI CEPAT v4 (Codex Cloud → APK di HP)
1. Buat repo GitHub `alas-keramat`, unggah SEMUA isi paket ini (termasuk folder `.github/`).
2. Siapkan lisensi Unity Personal (.ulf) — satu-satunya langkah yang butuh komputer, sekali saja.
3. Isi Secrets & Variables di repo, buat issue `AUTOPILOT`, hubungkan repo ke Codex Cloud dengan setup script & allowlist.
4. Jalankan workflow **AK Autopilot** dengan `paksa_tahap = T0-C`.
5. Tunggu. Setiap ada rilis **APK uji** di halaman Releases: pasang di HP, main, tulis kesan di `docs/UMPAN_BALIK.md`.
Langkah rinci: `docs/tech/18_PIPELINE_CLOUD_OTONOM.md` §2. Penilaian jujur & batasnya: `docs/EVALUASI_V4.md`.

## Isi paket
- `AGENTS.md` — aturan utama untuk Codex (sengaja di bawah 32 KB supaya tidak terpotong).
- `docs/gdd/01–10` — dokumen desain rinci: visi & arah seni, karakter, dunia, cerita & lore, survival & item, tempur & musuh, pohon & bangunan, companion, UI HP, audio.
- `docs/gdd/13–17` (BARU v3) — horor & teror, gore, visual & shader, dunia hidup, evaluasi & peningkatan mekanik.
- `docs/tech/11–12` — arsitektur teknis, tahapan T0–T12 + prompt Codex per tahap + checklist uji untukmu.
- `docs/PROGRESS.md` — buku catatan progres yang diisi Codex sendiri.

## Cara pakai singkat
1. Ikuti **T0** di `docs/tech/12_TAHAPAN_DAN_PENGUJIAN.md` (pasang Unity 6 LTS + Android, buat proyek template "Universal 3D").
2. Salin `AGENTS.md` dan folder `docs/` ke folder proyek (sejajar `Assets`). File ini (`BACA_DULU.md`) boleh ikut disalin atau tidak.
3. Tutup Unity → jalankan Codex di folder proyek → tempel prompt tahap T1.
4. Setelah Codex selesai: buka Unity → klik menu **Nusantara Builder → 1. BANGUN SELURUH GAME (1-KLIK)** → Play → jalankan checklist T1.
5. Ulangi untuk T2, T3, … satu tahap per sesi.

## Yang diperbaiki dari v1
| Masalah di v1 | Akibatnya | Perbaikan v2 |
|---|---|---|
| AGENTS.md 30 KB, hampir menyentuh batas baca Codex 32 KB | Detail tambahan akan terpotong diam-diam | AGENTS.md ringkas + detail di `docs/` |
| Emoji (🌧️ 🔥 🎒 📖 ★ ⌂) di teks UI | Tampil kotak kosong di HP | Ikon digambar prosedural (`IconPainter`) |
| Template Unity 6 sering memakai Input System baru | Tombol sentuh error/tidak merespons | Modul input dipilih otomatis, kontrol 100% lewat UI |
| Material dibuat runtime dengan `Shader.Find` | Bisa PINK di build Android | Semua tekstur/material/mesh/audio disimpan sebagai aset |
| Fallback font `Arial.ttf` | Error di Unity 2022.2+ | Hanya `LegacyRuntime.ttf` + font OS |
| Disebut 16 sendi, padahal daftarnya 20 | Rangka tidak konsisten | 20 sendi tubuh + 4 sendi wajah |
| Lokasi Lore #3–#5 bentrok antara Bagian 2 & 3 | Cerita kacau | Lokasi final di `04` |
| Disebut 16 file, daftar berisi 17 | Codex bingung | Manifest 17 file + file tambahan resmi |
| Ukuran tombol 75–110 px | ±5–7 mm di HP, mudah meleset | Tombol ≥ 9 mm + slider ukuran |
| AI "berjalan di gua" pakai `SampleHeight` | Gua di Y −80 tidak punya rumus tinggi | Raycast + graf waypoint gua |
| Satu prompt untuk seluruh game | Output terpotong, proyek tidak terkompilasi | Tahap T1–T11 (+T12 opsional) + cek kompilasi otomatis + self-test |
| "1:1 Sons of the Forest" | Grafis foto-realistis mustahil tanpa aset | Paritas mekanik inti; arah seni low-poly atmosferik; jalur ganti aset asli lewat `Resources/Override` |

## Tambahan baru di v2 (ringkas)
Rumus terrain lengkap yang sudah disimulasikan (sungai tidak melayang, jurang & rawa benar), denah ruang 3 kompleks bawah tanah, 13 misi + grafik gerbang progresi, 6 lore tambahan (total 12), naskah cutscene & 2 ending, 78 item dengan ID tetap, 16 resep, 14 blueprint, 14 perintah Kang Deden, sistem Trust Ningsih rinci, 13 jenis musuh + state machine, agresi & raid kemah, boss 3 fase, siege 60 detik per gelombang, musim kemarau/penghujan, rumus semua suara + musik gamelan pelog/slendro, tata letak HUD presisi, mode kesulitan, menu debug untuk menguji cepat, dan build APK 1-klik.

## Yang baru di v3
| Bagian | Isi |
|---|---|
| Horor (13) | Sutradara Teror dengan 4 fase & anggaran kejutan; 37 kejadian seram; folklor jadi aturan main (tawa Kuntilanak terbalik, lepas tali Pocong, Paku Besi Kuning, Kuyang & duri, Tuyul & kepiting); suara "Kang Deden" memanggil padahal ia bisu; bisikan binaural untuk headset; malam gelap sungguhan dengan fase bulan; getar detak jantung |
| Gore (14) | Darah & genangan, luka menempel, Pendarahan, anggota tubuh putus & penggal, ragdoll, mayat yang membusuk, Patung Ngeri, kamp Sekte & lab yang mengerikan; pengaturan Mati/Sedang/Penuh; rating 18+ |
| Visual (15) | Shader HLSL buatan sendiri (terrain 4 lapis, daun bergoyang & tembus cahaya, air, kulit, rambut, mata, langit, kabut, sinar tajuk); tekstur prosedural kualitas tinggi; wajah 512 untuk tokoh utama; LUT per suasana; cuaca visual; animasi lebih hidup; foto kualitas otomatis; jalur tekstur CC0 |
| Dunia hidup (16) | Ekosistem satwa (burung sebagai tanda bahaya, monyet pencuri, ular, kelelawar senja), Kampung Sekte Watu Ireng, Makam Tua, jadwal harian & kehidupan sosial Sekte, 13 kejadian dunia, 20 catatan tulisan Kang Deden, kebun & ziarah Ningsih, jalan setapak yang terbentuk sendiri |
| Evaluasi (17) | Rapor v2 per aspek + perbaikan: 10 senjata baru (termasuk pistol & senapan), elak, bunuh senyap, stealth lengkap, bangunan batu, 12 blueprint baru, 7 musuh baru, alur Rendra, misi sampingan, epilog bercabang, Mode Imersif, Mode Kustom |
| Tahapan | Tahap baru **T9 (Teror & Dunia Hidup)**; tahap lama T9–T11 bergeser jadi T10–T12 |

## Yang baru di v4
| Bagian | Isi |
|---|---|
| Pipeline (18 + `.github/workflows`) | Codex Cloud menulis kode → GitHub Actions + GameCI mengompilasi, menjalankan Builder & Self-Test, memotret, membuat APK → Autopilot merge & lanjut tahap, atau menyuruh Codex memperbaiki (maks 5×) |
| Visual (19) | Tekstur foto, model, HDRI CC0 diunduh otomatis; Unity Terrain bawaan menggantikan shader terrain kustom; motion-capture CMU untuk gerak manusia; sinematografi yang menyembunyikan kelemahan wajah; rubrik foto & 12 money shot |
| Kualitas | Gerbang G1–G4 termasuk Irisan Vertikal setelah T7; umpan balik kamu dari HP lewat `docs/UMPAN_BALIK.md` |
| Evaluasi | `docs/EVALUASI_V4.md`: rapor semua aspek, yang paling menentukan, dan yang tetap tidak mungkin |
