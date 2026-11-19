# Audit gagal uji main G3 - 19 November 2026

Build pemain: 02aee43, artifact hasil-92. Pemain menyatakan performa, visual, sentuh, audio dan navigasi buruk. Status penerimaan: GAGAL. Tidak ada rekaman profiler HP atau model perangkat; penyebab biaya render di bawah ditemukan melalui pembacaan kode, bukan pengukuran FPS perangkat.

| Area | Bukti sebelum revisi | Perbaikan | Bukti yang masih diperlukan |
|---|---|---|---|
| Gerak | Magnitudo mengurangi laju lalu mengalikan arah lagi. Pada input 0,575, laju maju 0,9775 m/s, seharusnya 1,7 m/s | Normalisasi arah; tes regresi laju setengah rentang dan diagonal | Rasa analog di layar HP |
| HUD | SIMPAN/MUAT/PANGGUL BATU pada Y=50 menutup slot cepat Y=70; kartu misi bertabrakan area aksi kanan | Toolbar terpisah di bawah vital dan kartu misi tengah atas; tes rect overlap slot | Keterbacaan pada ukuran layar perangkat |
| Sentuh | Tidak ada reset fokus/disable; drag memakai threshold bawaan, konsumsi delta tidak diurutkan dengan gerak | Reset kepemilikan jari/delta, urutan eksekusi, drag tombol serang, safe area | Dua jari, pause/tas/home/resume, bidik sambil menyerang |
| Vegetasi | Satu pohon dekat memaksa seluruh petak 125 m memakai mesh terinci; semua rumput memakai jarak/kepadatan tetap | Seleksi LOD dan jarak per instance, buffer tetap, kepadatan rumput per preset, penghitung segitiga | FPS dan P95 serta biaya GPU; tidak menjanjikan 30 FPS dari tes logika |
| Setelan | Diterapkan di Awake ketika layanan lain belum tentu terdaftar; Ultra 60 FPS, kategori bloom tidak diterapkan | Terapkan ulang di Start, rekomendasi awal konservatif, Ultra 30 FPS, bloom sesuai preset | Persistensi, pilihan manual, suhu dan memori perangkat |
| Visual | Foto kamera tanpa AA; atlas daun tanpa preserve coverage; cutoff material foto ditimpa 0,5 | FXAA, mip coverage atlas, pertahankan cutoff bahan sumber | Render revisi dan penilaian di HP; ini belum mengganti kualitas aset/animasi |
| Audio | Semua ambien dan langkah dibangkitkan sintetis; AmbienceDirector melewati volume kategori | Rekaman alam/langkah CC0, streaming stereo, bank bernama, kontrol kategori | Uji dengar speaker/headset; mixing keseluruhan, langkah air dan suara tempur masih perlu perbaikan |
| Peta | Item PetaKertas ada tetapi tidak ada layar peta | Peta kontur/air 2 km, legenda, titik GPS, tujuan ketuk, kompas HUD | Keterbacaan dan orientasi pemain baru; jarak lurus bukan rute aman |

## Batas aset dan desain yang belum diselesaikan

Animasi manusia masih belum retarget mocap lengkap. Bangunan dan beberapa landmark masih prosedural. Pantulan HDRI, occlusion/contact, transisi LOD lembut, terrain streaming penuh dan mixing audio v5 belum terbukti selesai. Mengganti warna atau menambah jumlah pohon tidak membuktikan kualitas setara game acuan. Jangan tandai G3 selesai atau menggabungkan PR hanya karena pemeriksa kode lulus.

## Uji penerimaan build berikutnya

1. CI kompilasi, Builder dua kali, self-test dan build APK pada SHA revisi; foto baru berprovenance.
2. Main di area heli-kemah selama 10 menit, siang dan malam: FPS sasaran 30, P95 frame sasaran <= 40 ms. Catat data saat bergerak/berputar, bukan menu diam. Ini target uji, bukan hasil.
3. Gerak setengah analog dapat dikendalikan, bidik/serang dengan dua jari, buka tas/peta/jeda lalu kembali tanpa input tersangkut; tombol tidak tertutup notch/navigasi sistem.
4. Temukan GPS/peta di koper, buka PETA, pilih tujuan dan kembali ke heli mengikuti kompas. Penanda tidak mengganti akses misi/gerbang dunia.
5. Dengarkan rekaman alam dan langkah tanpa dengung dominan; volume AMBIEN dapat dimatikan terpisah dari efek. Uji arah serangan, hujan dan gua dengan headset.
6. Panel debug (ketuk kiri atas lima kali) -> SALIN LAPORAN HP. Sertakan model HP dan preset. Gambar baru dan umpan balik pemain menentukan apakah perubahan cukup.

## Sumber audio

- TinyWorlds: https://opengameart.org/content/forest-ambience (CC0).
- Wolfgang_: https://opengameart.org/content/crickets-ambient-noise-loopable (CC0).
- qubodup / pdsounds.org: https://opengameart.org/content/different-steps-on-wood-stone-leaves-gravel-and-mud (CC0).

URL, lisensi, ukuran dan SHA256 ada di Tools/aset_cc0.lock.json. Pengunduh memvalidasi hash ZIP sebelum membaca anggota, lalu hash setiap berkas hasil. Rekaman ini tidak diklaim direkam di Indonesia.

Instancing runtime dibagi maksimal 511 instance per pengiriman sesuai batas dua matriks bawaan Unity; kelompok simpanan tetap 1023 agar ID pohon tidak bergeser. Referensi API: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Graphics.RenderMeshInstanced.html .

## Audit lintas-sistem lanjutan (19 November, putaran kedua)

Temuan berikut berasal dari jalur kode dan foto build 02aee43, bukan klaim sudah menuntaskan playtest semua fitur.

| Sistem | Temuan | Tindakan revisi | Status penerimaan |
|---|---|---|---|
| Input Android | Pembacaan mouse tempur ikut dikompilasi ke Android; sentuh dapat ditiru menjadi mouse | Batasi input PC ke Editor/Standalone, tetap gunakan pointer uGUI Android | Menunggu uji multitouch APK |
| Tas, buku, peti, lore, perintah teman | Panel tidak mengunci input dunia; gerak/serangan lama dapat berlanjut di belakang UI | Kunci per panel, buang gerak/aim/charge/interaksi; tutup satu panel tidak melepas kunci panel lainnya | Menunggu uji buka-tutup dan fokus HP |
| Inventaris | Hanya 36 jenis pertama terlihat; tidak ada pakai senjata terpilih | Halaman semua jenis, pilihan bahan lintas halaman, PAKAI SENJATA, pembaruan UI digabung per frame | Menunggu uji sentuh dan keterbacaan label |
| Crafting | Cek hanya ruang lebih dari nol; hasil lebih dari satu dapat terpotong setelah bahan diambil | Tolak sebelum mengurangi bahan bila seluruh hasil tidak muat | Regresi Unity ditambahkan |
| Restore inventaris | Tidak mengirim perubahan stok, sabuk cepat dapat tetap menampilkan isi sebelumnya | Kirim event untuk item yang hilang dan masuk setelah seluruh stok dipulihkan | Regresi Unity ditambahkan |
| Save dunia | SimpanDunia dan WorldEventScheduler sama-sama memakai kunci dunia | Pisahkan kunci peristiwa, migrasikan dua entri lama berdasarkan schema, tolak kunci duplikat baru | Regresi Unity ditambahkan |
| Slot save | Slot kosong memuat berkas global dari slot lain; HUD tidak menyimpan slot aktif; Mulai Baru menghapus slot | Slot eksplisit mandiri, HUD mengikuti slot aktif, Mulai Baru mempertahankan simpanan sampai penyimpanan berikutnya | Uji slot dan relaunch APK masih perlu |
| Data dinamis Deden | Tumpukan dibuat saat main, tidak dibuat kembali setelah relaunch | Snapshot posisi/isi tumpukan dimiliki SimpanDunia, migrasi entri tumpukan lama | Regresi restore dua kali ditambahkan |
| Kegagalan save/load | Capture/restore yang gagal ditelan dan masih dapat dilaporkan berhasil | Batalkan penulisan bila snapshot gagal; restore gagal mencoba memulihkan snapshot semula dan melaporkan gagal | Belum menjamin semua komponen memvalidasi seluruh schema |
| Peluncuran | Intro berjalan di balik peringatan, menu awal disembunyikan, Lanjutkan tidak memuat save | Peringatan -> menu -> muat/baru -> intro jika belum pernah; kembali dari panel awal ke menu awal | Wajib uji fresh install dan relaunch |
| Tempur | Aim assist mengalokasikan array collider tiap frame ancang-ancang | Buffer overlap tetap; input serang/tangkis diblokir selama panel | Belum menilai rasa ayunan/hit reaction musuh |
| Rumput | Tinggi mesh 0,45–0,9 m dikali skala sampai >1,7; menutupi pandangan dan tampak bilah lebar | Rumpun 0,16–0,4 m dengan kartu lebih sempit; pertahankan pembersihan spawn/koper | Menunggu foto baru; material masih prosedural |
| Kebaya Ningsih | Albedo/normal motif baju sumber ikut dikalikan ke kebaya, terlihat garis kotor | Gunakan serat kain terpisah, kurangi AO kebaya, buang emboss motif sumber | Menunggu foto baru; wajah/animasi masih belum mencapai sasaran |
| Dunia dan gua | Manager bawah tanah masih hanya menampung UG1–UG3; spesifikasi v5 meminta enam kompleks | Dicatat sebagai kekurangan konten, bukan ditutupi status tahap selesai | BELUM SELESAI |
| Cerita, misi dan teman | Misi inti tersedia; 42 cutscene, mocap dan keluwesan perilaku belum terbukti lengkap | Tidak menganggap tes misi sebagai bukti seluruh pengalaman cerita selesai | BELUM LULUS PLAYTEST |
| Aset bangunan, loot dan tas | Banyak model tetap primitive/prosedural; atlas foto tidak memperbaiki siluet/model dengan sendirinya | Wajib audit aset per kategori dan penggantian model, bukan tambah efek kamera | BELUM MENCAPAI TARGET |
| Hujan, audio gua dan tempur | Rekaman alam/langkah baru mencakup sebagian bank, belum seluruh mixing v5 | Uji dengar terpisah kategori dan konteks tetap wajib | BELUM LULUS UJI DENGAR |
| Performa keseluruhan | Belum ada trace CPU/GPU, P95, panas dan memori HP yang dapat mengesahkan revisi | Diagnostik disediakan, release tanpa Development, LOD/density diperbaiki | BELUM LULUS UJI PERANGKAT |

Kriteria penutupan tetap pengalaman main yang dapat diperiksa pada APK dengan SHA yang benar. CI hijau hanya mengesahkan kompilasi dan pemeriksaan otomatis yang dijalankan; tidak mengesahkan kualitas visual, rasa kontrol, audio, atau kelengkapan konten v5.
