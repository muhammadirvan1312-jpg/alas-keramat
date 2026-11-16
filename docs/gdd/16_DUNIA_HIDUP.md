# 16 — DUNIA YANG HIDUP: EKOSISTEM, MASYARAKAT SEKTE, KEJADIAN DINAMIS, KESEHARIAN TEMAN, CERITA LINGKUNGAN
Dibaca pada tahap T9. Prinsip: setiap 30–60 detik pemain harus melihat/mendengar sesuatu yang bergerak atau berubah tanpa ia picu.

---

## 1. Ekosistem Satwa (tambahan 03 §8)
| Satwa | Habitat | Perilaku yang terlihat pemain |
|---|---|---|
| **Kawanan burung** (pipit, kutilang, jalak) | Semua hutan | Mencari makan di tanah/ranting; **beterbangan serempak** bila ada Sekte/hantu ≤ 40 m atau Andi berlari → isyarat bahaya alami (suara kepak massal) |
| **Burung hinggap** | Siang cerah | Andi jongkok diam 20 s lalu "ULURKAN TANGAN" → burung kecil hinggap di tangan (Sanity +5) |
| **Rangkong** | Tajuk tinggi | Berpasangan terbang di atas tajuk dengan bunyi kepak "wuss-wuss" khas; bersarang di pohon besar |
| **Elang bondol** | Sungai, pantai | Berputar di langit, menukik menyambar ikan |
| **Monyet ekor panjang** | Bambu, Desa, tepi sungai | Kawanan 5–10; mencuri makanan dari Jemuran/kemah, melempar buah, **menjerit alarm** saat Sekte dekat; menggigit bila terpojok (4); bisa diberi Pisang → jarang mencuri 2 hari |
| **Rusa, kancil, babi hutan** | Hutan | Rusa menyalak saat waspada; babi menyeruduk tanah mencari umbi; babi tertarik bau darah |
| **Ayam hutan** | Bambu, Desa | Berkokok 05:30–06:30 (alami; beda dengan E11) |
| **Biawak** | Tepi sungai | Memakan bangkai & telur |
| **Ular Sanca** (baru) | Rawa, sungai | Menyergap dari air: melilit → tekan TEBAS berulang (8× dalam 4 s) untuk lepas; gagal = 25 damage |
| **Ular Kobra** (baru) | Reruntuhan Desa | Berdiri mendesis (peringatan 1,5 s) lalu mematuk → status **Racun** (−0,3 HP/s 30 s; obat KunyitObat/JamuHerbal) |
| **Kepiting** (baru, item ID 71) | Pantai | Berlarian menyamping; ditangkap tangan; makanan & penangkal Tuyul |
| **Penyu** | Pantai malam | Bertelur di pasir (Hari ≥ 4), tukik merangkak ke laut di pagi berikutnya (momen indah) |
| **Kelelawar** | UG1, Mercusuar | **Eksodus senja** 18:15: ribuan kelelawar (partikel mesh) keluar dari mulut Gua Air & puncak mercusuar |
| **Tokek** | Tiang saung, rumah Desa | Terlihat menempel; berbunyi; subtitle iseng saat 7 kali: *"Andi: (Tujuh kali... katanya pertanda baik?)"* |
| **Capung, kupu-kupu, kunang-kunang, katak, ikan melompat** | Air & semak | Partikel/hewan kecil tanpa AI berat |
- **Rantai makanan**: elang menyambar ayam hutan/ikan, biawak memakan telur & bangkai, Sekte berburu rusa dengan tombak (terlihat dari jauh).
- **Hutan mendadak sunyi**: dalam 50 m dari hantu, serangga & burung berhenti bersuara (ambien dikurangi) — isyarat yang sama dipakai Sutradara Teror.
- **Populasi**: per wilayah (grid 200 m) dengan stok; perburuan menurunkan stok, pulih 10%/hari.

## 2. Flora & Lingkungan yang Berubah
- Pohon tumbuh lagi (07 §1); **pohon roboh alami** setelah Hujan Badai (3–6 pohon di 150 m sekitar pemain: kayu gratis, jalur terhalang).
- Kemarau: kebakaran kecil di Sawit/semak kering (menjalar 30–60 m lalu padam), area hangus menghijau lagi dalam 10 hari. Penghujan: jamur ×2, sungai naik.
- Bunga & buah musiman: pisang berbuah di Penghujan, kamboja berbunga sepanjang tahun di Makam.
- **Jalan setapak terbentuk sendiri**: `TrailMap` 256×256 (global shader texture) dicat tiap menit di posisi yang sering dilalui pemain/companion/Sekte → `AK/Terrain` mencampur ke tanah jalan setapak. Rute pemain sendiri lama-lama menjadi jalur nyata.
- Struktur pemain menua: lumut tumbuh di kayu setelah 7 hari (shader `_Moss`), daun atap menguning.

## 3. Lokasi Baru (tambahkan ke 03 §3 & §9)
| Lokasi | Posisi | Pad (rIn/rOut/h) | Isi |
|---|---|---|---|
| **Kampung Sekte "Watu Ireng"** | (−300, 250) | 30/50/14 | 8 gubuk panggung beratap ijuk, lubang api tengah, lingkaran batu ritual, rak pengasap, 3 tiang totem, kandang tawanan, 2 panggung jaga di pohon, kendang & menara kentongan; penghuni 8–12; loot: TopengSekte, PelurSuar ×2, MesiuHitam, Damar; dekorasi gore 14 §7 |
| **Kemah Rawa** (panggung di atas rawa) | (−265, 70) | — (di air) | 3 panggung bertiang 2,5 m di atas rawa, jembatan papan; markas Abdi Lumpur |
| **Makam Tua Pasir Lenyap** | (318, 58) | 15/25/11 | 2 pohon kamboja, ±30 nisan kayu & batu lapuk tanpa tulisan/simbol, pagar bambu roboh; Pocong 1–2 malam hari; makam juru kunci (ayah Ningsih) bertanda kain kuning. **Tidak bisa digali** (subtitle: *"(Tidak pantas menggali di sini.)"*) |
| **Pos Pengintai Sekte** ×5 | (−120,−60), (80,40), (200,−200), (−200,−250), (60,250) | — | Panggung kayu di pohon 4 m, tangga tali, 1 pengintai siang; bisa dibongkar (Agresi +3) |
| **Kebun Ningsih** | 6 m dari api kemah (dibuat Ningsih saat Trust ≥ 70) | — | 4 bedeng kunyit/jahe/sirih; panen tiap 2 hari |

## 4. Masyarakat Sekte
### 4.1 Jadwal Harian
| Jam | Kegiatan |
|---|---|
| 05:00 | Bangun, api dinyalakan, asap dapur terlihat dari jauh |
| 06:00–09:00 | Menangkap ikan di sungai (tombak), berburu rusa, mengambil air |
| 09:00–12:00 | Patroli & mengumpulkan; pengintai menuju kemah pemain |
| 12:00–14:00 | Berteduh, tidur siang di gubuk (penjagaan longgar — waktu terbaik menyelinap) |
| 14:00–17:00 | Patroli; mengintai kemah pemain (06 §6) |
| 17:00–18:00 | Pulang; **Maghrib**: kentongan bersahutan dari menara |
| 19:00–23:00 | Ritual: menari mengelilingi api dengan kendang & gumam tanpa kata |
| 23:00–04:00 | Tidur; 2 penjaga di panggung pohon |
| Malam Darah | Tidak tidur; kelompok pemburu keluar |
### 4.2 Perilaku Sosial (terlihat dari jauh dengan menyelinap)
- **Menjemput jenazah**: setelah tempur, 2 Sekte datang menyeret mayat teman ke kampung (jejak seret), berlutut berduka 6 s, lalu menggantungnya terbungkus kain.
- Duduk berkelompok berbagi makanan; pemimpin memberi isyarat tangan; mendorong satu sama lain saat bertengkar; memperbaiki gubuk.
- **Sisa kemanusiaan**: setelah Andi mengampuni ≥ 3 Sekte, kadang (25%/pagi) ada **persembahan** di tepi kemah: ikan, Damar, atau kain (dari "mantan warga" yang ragu). Statistik & teks Ending B memperhitungkan ini.
- Sekte memasang **boneka peringatan** di dekat kemah pemain (E22), dan membongkar perangkap pemain sesekali (Agresi ≥ 60).
- **Mutan vs Sekte**: setelah Hari 6, kadang terjadi perkelahian Sekte melawan mutan lab di sekitar Sawit (mutan memusuhi semua). Pemain bisa menonton atau memanfaatkan.

## 5. Kejadian Dunia Dinamis (`WorldEventScheduler`)
Setiap pagi 06:00 diundi maks 2 kejadian (bobot, syarat, jeda) + kejadian terskrip.
| ID | Kejadian | Syarat | Isi |
|---|---|---|---|
| W01 | **Pesawat Pencari Lewat** | Terskrip Hari 4 08:00 + acak 10%/hari | Pesawat kecil melintas tinggi. Bila Andi menembakkan suar / api unggun menyala dalam 30 s: pesawat berbelok sedikit lalu pergi. Subtitle: *"(Mereka mencari... tapi kabut menutupi semuanya.)"* |
| W02 | Pohon Roboh Badai | Setelah Hujan Badai | 3–6 pohon roboh alami |
| W03 | Kebakaran Kecil | Kemarau, Sawit/semak | Api menjalar 30–60 m, Banaspati malamnya |
| W04 | Banjir Sungai | Penghujan 15%/hari | Sungai +1,2 m sehari; mulut Gua Air tergenang (hanya bila M08 sudah selesai) |
| W05 | Peti Terdampar | 20%/hari | Peti di pantai: acak Baterai, PerbanKain, RansumDarurat, PelurSuar |
| W06 | Jenazah Ekspedisi | Tiap 3 hari, maks 5 | Mayat anggota ekspedisi + ransel (loot) di tepi sungai; tanda pengenal nama (lihat §7) |
| W07 | Prosesi Obor | = E31 | — |
| W08 | Sekte vs Mutan | Hari ≥ 6, malam | Perkelahian di sekitar Sawit |
| W09 | Babi Mengamuk | 10% | Babi hutan menyerbu kemah |
| W10 | Monyet Pencuri | 25% jika ada Jemuran | Monyet mencuri 1–2 makanan |
| W11 | **Hujan Meteor** | Terskrip malam Hari 7 (cerah) | Bintang jatuh berulang; Ningsih menunjuk ke langit, Deden tersenyum; Sanity +15 (momen tenang) |
| W12 | Kunang-kunang Massal | 8%, malam cerah, dekat sungai | Ribuan kunang-kunang; Sanity +10 |
| W13 | Penyu Bertelur | Hari ≥ 4 | Pagi berikutnya tukik ke laut |

## 6. Keseharian Teman
### 6.1 Kang Deden
- Pagi peregangan & menguap; **mengasah golok** di batu (bunyi gesek); memancing dengan tombak saat senggang; memperbaiki struktur rusak sendiri (prioritas rendah); duduk di dekat Andi saat Andi terluka; mendengkur keras saat tidur (ia tuli, tidak sadar; bila Agresi ≥ 60, dengkuran menambah kesadaran Sekte dalam 30 m).
- **Memahat patung kayu kecil** untuk dua anaknya; Hari 6 ia memberikan satu ke Andi dengan catatan: *"Untuk anak saya, Dewi. Kalau saya tidak pulang, tolong berikan."* (item kenangan, tampil di Ending A/B).
- Malam: memandangi foto keluarga (prop kecil) di bawah cahaya api.
- Setelah menyaksikan kejadian horor, ia menolak keluar kemah saat malam (GelengBahu) sampai pagi.
- **Catatan Kang Deden** (fitur baru): ia menulis di bukunya & meninggalkannya di kasur/meja kemah; Andi membacanya ("BACA CATATAN DEDEN"; tersimpan di Buku Catatan tab "Deden"). Karena tuli, catatannya hanya tentang hal yang ia **lihat**. 20 catatan dipicu kejadian, contoh:
  1. "Andi, telinga saya berdenging terus. Tapi saya masih bisa kerja. Tulis saja apa yang harus saya lakukan."
  2. "Tadi pagi saya lihat orang bertopeng di seberang sungai. Dia diam lama sekali melihat kemah kita."
  3. "Burung-burung kabur dari pohon besar semalam. Ada yang tidak beres di sana."
  4. "Gadis itu menaruh ikan di dekat api waktu kamu tidur. Saya pura-pura tidak lihat."
  5. "Atap bocor di sisi kiri. Sudah saya tambal pakai daun."
  6. "Saya mimpi anak-anak saya di Garut. Dewi sudah bisa naik sepeda."
  7. "Malam tadi ada yang berdiri di pinggir kemah. Saya kira kamu. Waktu saya dekati, tidak ada siapa-siapa." (setelah E29)
  8. "Saya tidak mau ke hutan malam-malam lagi. Maaf, Andi."
  9. "Kalau kita pulang, saya traktir kamu sate maranggi."
  10. "Tanda di lengan gadis itu menyala waktu Pocong datang. Dia tahu lebih dulu dari kita."
  (Codex menulis sisanya dengan nada sama: sederhana, hangat, Sunda halus, hanya hal yang terlihat; pemicu: Hari, jumlah bangunan, Trust Ningsih, Malam Darah, Paku didapat.)
### 6.2 Ningsih
- Memanjat pohon & duduk di dahan menatap senja; **merawat kebun** (§3); mengepang rambut; bersenandung malam; burung hinggap di tangannya; memperbaiki anak panah; berenang di sungai pagi.
- **Menggambar simbol segel di tanah** dengan ranting (petunjuk urutan teka-teki & ruang rahasia K4).
- Trust ≥ 70: saat fajar mengunjungi **makam ayahnya** di Makam Tua, menaruh bunga kantil (pemain bisa ikut; subtitle hening; Trust +5 bila Andi menemani diam).
- Menonton hujan meteor (W11), menari pagi (08), menatap gelap saat merasakan hantu (E30).

## 7. Cerita Lingkungan (objek "PERIKSA" — 1–2 baris monolog Andi)
Builder menempatkan 40 objek; 15 di antaranya tetap:
| Objek | Posisi | Monolog |
|---|---|---|
| Pita survei oranye di pohon (jejak ke bunker) | 6 titik garis (−120,40) → (−270,140) | "(Pita survei PT. Mereka menandai jalan ke barat.)" |
| Patok batas "O.I.O. 1928" | (−240,180) | "(Patok Belanda. 1928... sama dengan tahun di surat tugas.)" |
| Layangan tersangkut di pohon | Desa (275,120) | "(Dulu ada anak-anak di sini.)" |
| Sesajen di akar pohon | 5 titik hutan campuran | "(Masih segar. Seseorang terus merawat ini.)" |
| Tenda ekspedisi robek | (60,−60) | "(Tenda Rendra? Bukan. Ini lebih tua.)" |
| Kamera rusak | UG1 R2 | "(Fotonya buram... sosok putih di ujung gua.)" |
| Helm kuning PT hangus | Sawit (−300,80) | "(Mereka tidak sempat lari.)" |
| Papan "DILARANG MASUK — KAWASAN KONSESI" | (−180,90) | "(Konsesi. Hutan keramat dijadikan konsesi.)" |
| Ayunan bambu di Desa | (262,92) | "(Bergoyang sendiri... angin, pasti angin.)" |
| Radio rusak di heli | (4,−224) | "(Hanya desis. Tidak ada yang menjawab.)" |
| Foto keluarga di dompet mayat ekspedisi | W06 | "(Dia juga punya keluarga yang menunggu.)" |
| Coretan "JANGAN PERCAYA SUARA" di batu | (100,180) | "(Suara siapa?)" |
| Keranjang anyaman berisi bunga kantil | Sarang Ningsih | "(Bunga yang sama dengan di rambutnya.)" |
| Lukisan dinding gua (tiga paku & pohon) | UG1 R1 | "(Lukisan tua. Tiga titik emas di akar pohon.)" |
| Rantai berkarat di pohon beringin kecil | (−60,120) | "(Ada yang pernah dirantai di sini.)" |
25 sisanya diacak per zona dari daftar jenis di atas.

## 8. Audio Hidup (tambahan 10 §4)
- **Paduan subuh** 05:30–06:30: ledakan kicau, ayam hutan, monyet. Senja 18:15–18:45: tonggeret berganti jangkrik, kepak kelelawar massal.
- **Embusan angin berjalan**: gelombang suara daun bergerak melintasi tajuk dari satu arah ke arah lain (panning bertahap) tiap 20–60 s.
- Panggilan satwa jauh: rusa menyalak, babi mendengus, rangkong, elang memekik.
- Suara kampung Sekte dari jauh: kendang, gumam ritual tanpa kata, kapak memotong kayu.
- Derit rumah Desa, ayunan bambu, sumur berdenting.

## 9. Memori Dunia (disimpan)
Jejak kaki di lumpur & pasir (pemain, Sekte, hewan, Genderuwo, Tuyul) memudar 1 hari; abu api unggun bertahan; tunggul; bekas darah; peti terbuka; pos pengintai yang dibongkar; TrailMap; persembahan Sekte; catatan Deden yang sudah dibaca.
