# EVALUASI v4 — RAPOR JUJUR SEMUA ASPEK & ARAH PERBAIKAN
Untuk pengguna dan Codex. Skala 1–10 = perkiraan kualitas hasil akhir bila dokumen dijalankan dengan baik, dibandingkan pengalaman bermain Sons of the Forest (PC).

## 1. Rapor
| Aspek | v3 | v4 | Yang berubah di v4 | Batas yang tetap ada |
|---|---|---|---|---|
| **Cara kerja (otomasi)** | 3 | 8 | Codex Cloud + GitHub Actions/GameCI + Autopilot: tulis kode → kompilasi & uji di cloud → APK di HP → perbaikan otomatis (18) | Perlu file lisensi Unity dari komputer sekali; Codex mungkin perlu 1 ketukan "Create PR" per tahap |
| **Verifikasi** | 2 | 8 | Kompilasi, Builder, Self-Test, APK nyata di setiap PR; laporan error otomatis dibaca Codex | Uji "rasa main" tetap butuh kamu memainkan APK |
| **Visual lingkungan** | 5 | 7,5 | Tekstur foto CC0, Unity Terrain, pantulan HDRI, model batu/props CC0 (19) | GPU HP: kepadatan & jarak pandang tetap di bawah PC |
| **Visual karakter** | 4 | 6 | Mocap asli, kain bertekstur foto, sinematografi yang menyembunyikan wajah | Wajah manusia prosedural tetap titik terlemah |
| **Animasi** | 4 | 7 | Motion-capture CMU untuk manusia; prosedural hanya untuk makhluk gaib & lapisan | Transisi antar-klip perlu tuning dari umpan balik |
| **Horor** | 7 | 7,5 | Rubrik "rasa takut" di foto malam, umpan balik main di HP | Rasa takut sejati hanya bisa diuji dengan dimainkan |
| **Gore** | 7 | 7 | — (sistem v3 sudah lengkap) | Detail luka di HP sederhana dibanding PC |
| **Dunia hidup** | 7 | 7 | — | — |
| **Tempur & AI** | 7 | 7,5 | Gerak mocap membuat Sekte lebih meyakinkan | AI tanpa NavMesh perlu banyak tuning |
| **Bangunan & survival** | 8 | 8 | — | — |
| **Cerita** | 7 | 7 | — | Tanpa sulih suara (hanya teks & suara sintetis) |
| **Audio** | 6 | 6 | — | Suara sintetis tidak setara rekaman asli |
| **Risiko teknis** | 4 (shader terrain kustom, tanpa uji) | 7 | Terrain bawaan Unity menggantikan shader terrain kustom; CI sebagai hakim | Shader AK lain tetap berisiko; ada fallback otomatis |

## 2. Yang Paling Menentukan Hasil Akhir (urut)
1. **Irisan Vertikal dulu** (gerbang G3): satu area kecil (heli + kemah + malam pertama) dibuat sampai terlihat & terasa bagus SEBELUM pulau penuh diisi. Studio sungguhan bekerja seperti ini; kualitas pulau mengikuti standar irisan ini.
2. **Umpan balik kamu dari HP** di `docs/UMPAN_BALIK.md` setiap ada APK uji. Codex tidak bisa merasakan takut atau bosan; kamu bisa.
3. **Aset foto CC0 + mocap**: lompatan visual terbesar per usaha.
4. **Tidak menambah fitur sebelum yang ada lulus**: setiap tahap harus CI hijau.

## 3. Yang Tetap Tidak Mungkin (jujur)
- **Setara persis SOTF di HP**: SOTF dibuat studio puluhan orang bertahun-tahun dengan aset pindaian, motion-capture khusus, sulih suara, dan GPU PC. Target realistis paket ini: game survival-horror HP yang **terasa** seperti SOTF (loop, kebebasan bangun, teman AI, kanibal/hantu yang cerdas, gua, ending) dengan visual atmosferik semi-realistis.
- **Satu prompt lalu jadi sempurna tanpa dimainkan**: pipeline bisa berjalan sendiri dari nol sampai APK lengkap, tetapi penyetelan rasa (kesulitan, ritme takut, kenyamanan kontrol) butuh minimal beberapa putaran umpan balik dari kamu.

## 4. Apa yang Kamu Lakukan
1. Persiapan sekali (18 §2).
2. Jalankan Autopilot (`paksa_tahap = T0-C`).
3. Setiap muncul rilis "APK uji": pasang di HP, main 10–20 menit, tulis 3–5 kalimat kesan di `docs/UMPAN_BALIK.md` (apa yang jelek, apa yang tidak seram, apa yang membingungkan).
4. Bila PR berlabel `butuh-bantuan`: baca komentar terakhir, balas dengan `@codex` + petunjuk singkat.
