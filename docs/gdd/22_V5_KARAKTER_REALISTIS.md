# 22 — V5 KARAKTER REALISTIS (MakeHuman / MPFB2, CC0)
> Mengalahkan 02 (anatomi kotak 20 sendi), 15 §3 (wajah 512) dan 19 §6 untuk semua karakter manusia & humanoid. Hewan dan hantu non-humanoid tetap prosedural.

## 1. Kenapa & Lisensi
Karakter prosedural kotak-kotak adalah titik visual terlemah. v5 memakai **MPFB2** (MakeHuman Plugin For Blender, Blender 4.2+) untuk membuat manusia realistis dengan rangka, wajah, rambut, pakaian, dan kulit.
- Model hasil MakeHuman/MPFB berlisensi **CC0**; aset inti (mesh dasar, target, proxy, kulit "natural") juga CC0. Kode plugin GPL tidak ikut ke game (hanya hasil ekspor).
- **Hanya aset ber-lisensi CC0** yang boleh dipakai (paket "MakeHuman system assets", paket kulit CC0, target CC0). Paket CC-BY (mis. "System eye materials 03") tidak dipakai. Setiap aset dicatat di `Tools/karakter.lock.json` (nama paket, url, sha256, lisensi).

## 2. Pipa Pembuatan (otomatis di CI, tanpa klik)
1. Workflow baru `.github/workflows/ak-karakter.yml` (dipicu saat `Tools/karakter/*.json` atau lock berubah, plus manual): pasang Blender 4.2 LTS headless (`blender -b`), pasang MPFB2 sebagai extension, unduh paket aset CC0 dari lock.
2. `Tools/karakter/buat_karakter.py` (dijalankan Blender headless) membaca resep JSON per karakter (§3): makrodetail (usia, gender, berat, otot, tinggi, proporsi, etnis Asia Tenggara), target wajah, kulit, mata, alis, bulu mata, gigi, rambut proxy, pakaian proxy, rig **"game_engine"**, lalu:
   - mesh LOD0 (≈ 18–28 rb segitiga utama), LOD1 (50%), LOD2 (20%) lewat Decimate;
   - **shape key wajah** 12 ekspresi (§4) + 15 visem (§5) dipanggang dari pose rig wajah MPFB;
   - tekstur 2K (wajah & tubuh), 4K untuk Andi/Deden/Ningsih/Ki Rogo (aset pahlawan 20 §4.3): albedo, normal, roughness, AO, plus peta subsurface (thickness) untuk kulit;
   - ekspor **FBX** (skala 1 = 1 m, Y-up) ke artefak `karakter-<versi>.zip`.
3. Artefak dirilis sebagai GitHub Release `karakter-<versi>`; `Tools/ambil_aset.py` mengunduhnya ke `Assets/ThirdParty/Characters/` (tidak dikomit).
4. Builder Unity: impor FBX (rig Generic, tanpa Animator — sesuai H1 tetap memakai `ProceduralAnimator`), petakan tulang MPFB "game_engine" ke 20 sendi logika lama (tabel di `Tools/karakter/peta_tulang.json`) agar seluruh kode gameplay & mocap CMU tetap berlaku, buat material kulit `AK/Skin` (§6), simpan prefab ke `Assets/Generated/Characters/`.
5. Cadangan: bila artefak tidak ada, Builder memakai karakter prosedural lama (02) dan menulis peringatan di laporan.

## 3. Daftar Karakter & Resep
| Karakter | Resep inti | Pakaian (proxy CC0 + tekstur kain 19) | Catatan |
|---|---|---|---|
| Andi Pratama (34) | Pria Jawa, 172 cm, atletis sedang, rahang tegas, kumis tipis tumbuh seiring hari (shape key janggut 0→1 selama 10 hari game) | Kemeja lapangan khaki, celana kargo, sepatu bot, rompi | Tampil di cutscene & bayangan/kaki saat FPS; luka & kotoran dinamis (§6) |
| Kang Deden Suryana (47) | Pria Sunda, 165 cm, gempal, perut, kulit gelap terbakar matahari, uban | Kaos lusuh, celana kerja oranye, topi | Ekspresi ramah, takut, lelah |
| Ningsih (tampak 19, "tidak menua") | Perempuan Jawa, 155 cm, kurus, pucat sedikit (bukan seram) | Kebaya putih kusam & jarik batik sogan | Kulit lembap halus; ekspresi sedih/curiga/percaya |
| Ki Rogo (± 70) | Pria tua kurus, kulit keriput (target usia maksimum), mata sedikit keruh | Beskap hitam sobek, blangkon, kalung tulang | Aset pahlawan 4K; wajah setengah tertutup bayangan di cutscene |
| Rendra Wijaya (34) | Pria, 175 cm, kacamata, kurus kelaparan (berat −40%) | Seragam lapangan PT Bhumi Cakra robek | Karakter baru cerita v5 (23) |
| Mbah Sastro (± 80) | Juru kunci Alas Wingit, bungkuk (pose dasar), kurus | Surjan lurik, ikat kepala, tongkat | NPC netral baru (23) |
| Sekte (4 tubuh × 6 topeng) | Pria & perempuan dewasa berbagai berat; wajah tertutup topeng kayu | Kain mori kotor, lumpur, ikat pinggang tali | Tubuh realistis membuat Sekte jauh lebih mengerikan |
| Korban & mayat (8 varian) | Variasi acak usia/berat | Pakaian peneliti, warga, pekerja pabrik | Untuk cerita lingkungan & gore (14) |
| Pocong, Kuntilanak, Sundel Bolong, Genderuwo | Tubuh MPFB dimodifikasi: proporsi diregangkan (+12% leher/lengan), kulit pucat kehijauan, mata tanpa pupil, rambut panjang kusut | Kain kafan / daster putih kotor | Gerak "salah" (20 §4.4) |

## 4. Ekspresi Wajah (12 shape key)
Netral, Takut, Ngeri (mata melotot, mulut terbuka), Marah, Sedih, Menangis, Lega, Senyum tipis, Sakit, Lelah, Curiga, Teriak. `FaceController` (baru) mencampur ekspresi dengan kurva 0,15–0,4 s + **kedip acak** (2–6 s, lebih sering saat takut) + **gerak mata** (saccade 3–4×/s kecil, menatap target bicara/objek menarik) + napas (dada & bahu).

## 5. Bicara Tanpa Suara Rekaman
Dialog tetap berupa subtitle (tanpa suara rekaman sesuai pilihan pengguna), tetapi mulut bergerak: 15 visem (A, I, U, E, O, M/B/P, F/V, S/Z, T/D/N, K/G, L, R, W, Y, diam) dipilih per suku kata dari teks Indonesia (`SukuKataKeVisem`, aturan ejaan Indonesia sangat fonetis), kecepatan 13 suku kata/detik, ditambah gumaman napas pelan dari synth (21) agar tidak bisu. Dialog boleh diganti rekaman suara di kemudian hari (slot `DialogAudioBank` disiapkan).

## 6. Kulit, Rambut, Mata (shader HLSL baru, H15)
- `AK/Skin`: GGX ganda (dua lobus spekular), **subsurface palsu** (wrap lighting + warna hamburan merah di tepi bayangan + thickness map untuk telinga/hidung tembus cahaya senter), detail normal pori tileable, lapisan dinamis: kotoran/lumpur, keringat (smoothness naik saat stamina rendah), darah (mask dari GoreSystem), basah (hujan/berenang).
- `AK/Hair`: kartu rambut alpha-test + dithering, spekular anisotropik Kajiya-Kay ganda, bergoyang ringan (vertex).
- `AK/Eye`: kornea refraktif palsu (parallax iris), kilau basah, pupil membesar dalam gelap; mata hantu tanpa pupil.
- Pakaian: tekstur kain CC0 + kotoran progresif; kebaya Ningsih memakai sedikit transmisi.

## 7. Gore dengan Mesh Realistis (mengganti cara "rigid skinning" di 14)
Builder memotong mesh setiap karakter menjadi segmen per tulang besar (kepala, lengan atas/bawah, tangan, paha, betis) berdasarkan bobot tulang dominan, menutup lubang dengan **mesh tutup luka** (cincin daging/tulang bertekstur), dan menyimpan segmen sebagai sub-mesh tersembunyi. Saat mutilasi (sesuai `GoreLevel`, H13/H16): segmen asli disembunyikan (bobot nol), segmen lepas di-spawn sebagai Rigidbody dengan tutup luka, darah memancar dari tulang. Batas mutlak H13 tetap: Andi/Deden/Ningsih tidak pernah dimutilasi.

## 8. Anggaran
Andi/Deden/Ningsih: ≤ 30 rb segitiga LOD0, 4K. Sekte/hantu: ≤ 18 rb, 2K, atlas per varian. Maks 12 karakter penuh di layar (lainnya LOD1/LOD2/impostor). Perkiraan tambahan APK: 120–200 MB.

## 9. Pembagian Tahap
T1: workflow `ak-karakter.yml` + resep JSON + peta tulang (artefak pertama: Andi saja). **T4 (diganti)**: semua karakter §3, `FaceController`, visem, `AK/Skin`/`AK/Hair`/`AK/Eye`, retarget mocap ke rangka MPFB. T7: segmentasi gore §7. T10: cutscene memakai wajah penuh (23).
