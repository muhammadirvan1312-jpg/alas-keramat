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

Temuan medis tambahan: PerbanKain berkategori Alat sehingga tombol MAKAN sebelumnya nonaktif saat memilih perban. Jalur PAKAI OBAT kini mengaktifkan perban ketika berdarah; jamu/sirih/perban yang dipakai untuk pendarahan juga menerapkan efek vital item tepat sekali. TANGAN dan TANGKIS kini memiliki pemilik pointer dan reset fokus/disable seperti joystick/TEBAS. Pemeriksaan regresi akses perban ditambahkan.

Pemeriksaan integrasi restore: event stok yang menyegarkan HUD tidak boleh dianggap mengambil Paku Keramat baru atau memajukan misi. SaveSystem menandai transaksi muat (termasuk rollback); pemicu misi/alarm gua mengabaikan event stok selama transaksi itu. Regresi memastikan penanda tetap dilepas pada restore sukses maupun gagal.

Audit geometri HUD tambahan: MODE dan LOMPAT, serta GANTI dan TAS sebelumnya bersinggungan. Tata kontrol akhir membagi aksi menjadi tiga kolom, TAS/BUKU ikut safe area, subtitle berada di atas area aksi. Pengaturan ukuran juga mencakup TEBAS/TANGKIS (sebelumnya keduanya bukan komponen Button sehingga terlewat). Regresi memeriksa seluruh pasangan 14 tombol (termasuk toolbar simpan/muat/batu yang tidak ikut dibesarkan) pada skala 85%, 100%, 115% dan 130%, termasuk tombol kontekstual yang sedang tersembunyi.

Pengukuran FFmpeg rekaman mentah: forest_day RMS -50,9 dB / peak -28,2; forest_night -33,8 / -18,4; leaves01 -46,1 / -22,6; wood01 -38,0 / -20,8; stone01 -36,3 / -11,0. Volume bed 0,22 sebelumnya membuat rekaman siang hampir tenggelam. Builder kini memanggang salinan PCM dengan RMS sasaran -25 dB untuk bed dan -24 dB untuk langkah, dibatasi peak -1 dB, lalu mengimpor bed streaming stereo dan langkah mono ADPCM. Berkas unduhan asli tetap utuh sesuai lock. Angka ini pengukuran/normalisasi level berkas, bukan bukti uji dengar HP; clipping akibat codec/mix simultan dan keseimbangan seluruh bank tetap harus diuji.

Pengecekan integrasi panel/tempur: pembatalan input harus menghapus elak beserta durasi kebalnya, mengembalikan state tusukan tombak, dan tetap mengurangi timer reload/buff mengikuti waktu dunia. Regresi memicu elak lalu membuka jalur pembatalan panel dan memeriksa kebal sudah hilang. Peta juga tidak menyimpan timeScale hit-stop sebagai kecepatan dunia saat ditutup.

## Pemeriksaan render f6e4634 (19 November, sesudah CI hijau)

Kompilasi, dua Builder, 253 self-test, APK dan 6 gambar lolos pemeriksaan teknis. Pemeriksaan manusia pada `g3_04_lantai_hutan` dan `g3_02_ningsih_wajah` tetap GAGAL kualitas: tajuk berisi pecahan/gerigi kecil, pola tanah berulang dan terlalu kosong, siluet karakter tidak utuh karena lengan Ningsih hilang.

Akar lengan: pemotongan kulit Ningsih hanya mempertahankan bobot tulang tangan 12/13, sehingga kulit lengan atas 8/9 dan lengan bawah 10/11 ikut dibuang bersama torso. Seleksi kulit kini mempertimbangkan seluruh rantai lengan, dan pemeriksa mesh hasil panggang wajib menemukan kulit kedua lengan bawah. Regresi Python menunjukkan aturan lama menolak kedua forearm dan aturan baru mempertahankan keenam bagian lengan sambil tetap menolak torso tertutup. Hasil mesh/render perbaikan ini masih menunggu CI; jangan menganggapnya selesai hanya dari perbaikan aturan seleksi.

## Kenyamanan seluruh permainan - putaran ketiga, 19 November

Arahan pemain: evaluasi semua sistem yang sudah ada, termasuk analog/POV, performa, grafis dan audio; jangan membatasi penerimaan pada satu area pamer. CI 230ef0f berhasil tetapi render aslinya tetap gagal penilaian visual. Lengan masih masuk torso walaupun 498 referensi vertex forearm lolos tes; tes jumlah vertex tidak cukup.

| Area | Penyebab yang diperiksa | Revisi konkret | Batas bukti |
|---|---|---|---|
| POV/tempur | Bantuan bidik 90 derajat/detik menambah yaw di luar input jari; dua LateUpdate saling menimpa goyangan | Bantuan 18 derajat/detik, berhenti saat membidik manual, satu pengendali kamera; goyangan tempur opsional <=0,6 derajat; goyangan jalan bawaan mati dan opsi tersedia | Memerlukan uji rasa di perangkat |
| HUD seluruh dunia | Aksi tersebar tiga kolom dan MODE tetap terlihat saat tidak membangun | Dua kolom kanan, GANTI/OBOR di kiri bersama TAS/BUKU; MODE hanya saat membawa bahan/melihat struktur; TANGAN muncul saat ada target; sabuk ikut safe area; cincin progres tahan | Uji overlap 14 tombol tetap dijalankan pada 85-130%; gambar HUD saat Play ditambahkan |
| Pengaturan | Tombol menggilir nilai besar sehingga sulit mencari sensitivitas/volume yang nyaman | Tab kontrol, grafis, audio, permainan; penggeser sensitivitas, FOV, ukuran tombol dan empat volume; nilai/status pilihan selalu ditampilkan | Tidak ada klaim telah diuji sentuhan HP |
| Tas, buku, lore, peti, peta, jeda, sinema | Kunci input tidak menjeda dunia, beberapa pemilik menulis timeScale=1 sendiri; kecepatan lama tersisa | JedaDunia menyatukan pemilik jeda dan hit-stop; tas/buku menjeda secara bawaan sesuai GDD09; opsi mematikan jeda panel; momentum analog dibuang; latar/fokus juga menjeda; aksi mati diblokir | Perlu uji home/resume Android dan seluruh kombinasi panel |
| Pungutan & simpan | Awal permainan membangkitkan seluruh 2 km lagi; scan linear setiap 0,5 detik; hasil panen tidak disimpan | Titik/indeks sel dipanggang Builder; query hanya sel sekitar pemain tanpa alokasi; ID deterministik tetap; hari panen ISaveable | Regresi membandingkan query dengan scan penuh dan restore; bukan benchmark HP |
| Render dunia | Seleksi frustum baru tingkat petak; dekorasi dapat mengirim >511 instance; darah senjata mencari semua renderer tiap frame | Bounds per instance dicache; caster dekat tetap dipertahankan; dekorasi batch <=511; renderer senjata dicache dan pembaruan tint 10 Hz | Biaya CPU/GPU dan suhu masih perlu diukur |
| Daun scan | Scan berisi 44.168 daun, masing-masing 24 tris; collapse seluruh 1,6 juta tris membuat pecahan dan merusak UV | Batang disederhanakan terpisah; daun menjadi kartu utuh dari pemetaan UV asal dan subset LOD deterministik; ranting tipis dihilangkan; 13.999/2.200 tris dan varian 12.500/2.000 telah dipanggang Blender lokal | Perbesaran kartu mempertahankan kepadatan perkiraan; transisi LOD/pop dan estetika harus dinilai dari Unity |
| Karakter | Rotasi Z positif lengan kiri yang sudah mengarah ke bawah justru menekuk ke tengah tubuh | Balik arah abduksi pose dasar untuk semua manusia; uji BakeMesh memastikan forearm kiri/kanan berada di luar bahu, bukan hanya bobot ada | Wajah, pakaian, siluet, mocap dan animasi bukan otomatis selesai |
| Foley | Jarak 0,75/1,1 m pada kecepatan 3,4/6,2 m/s menghasilkan 4,5/5,6 langkah/detik; landing bisa memutar bunyi ganda | Jarak 1,45/1,75 m, batas interval, landing tunggal, abaikan teleport; langkah pemain 2D stabil dan variasi pitch dipersempit | Pengujian dengar speaker/headset tetap wajib; bank tempur/hujan/hantu masih sebagian sintetis |
| Bukti main | Pemeriksaan hanya EditMode/foto di scene diam | Job foto diteruskan ke PlayMode: gerak kapsul dengan pointer, dua jari/POV, tas saat analog ditahan, jeda bersarang, peta, restore dan tiga render HUD/pengaturan. Laporan memuat SHA, gagal bila ada exception/error, timeout atau urutan tidak lengkap | Ini uji integrasi Editor, bukan playtest pengguna, benchmark Android atau bukti seluruh konten tamat |

Penerimaan seluruh game tetap TERBUKA: inventaris/crafting/tempur/bangun/teman/misi/survival/navigasi/save harus diuji pada APK. Kekurangan aset bangunan, audio konteks, mocap, konten gua/cerita dan streaming yang dicatat di atas belum ditutupi dengan status hijau. Jangan merilis atau auto-merge PR hanya berdasarkan tes teknis ini. PR #26 tetap draft sampai kualitas main dapat ditinjau.

Pemeriksaan rute sentuh tambahan: helper BuatTeks sebelumnya membuat setiap label/subtitle/teks sinema sebagai target raycast. Label sekarang tidak menelan sentuhan. Uji PlayMode memakai EventSystem.RaycastAll dan ExecuteEvents, memastikan hit teratas benar-benar mencapai zona analog dan POV, sebelum menggerakkan pemain; pemanggilan handler langsung saja tidak dapat mendeteksi overlay yang menghalangi sentuhan.


Audit gerak lanjutan: keluar dari jongkok tidak memeriksa ruang kepala; benturan langit-langit tidak membuang kecepatan naik; respawn memindahkan Transform tetapi meninggalkan kecepatan jatuh/puncak lompatan controller. Revisi memeriksa kapsul berdiri, menghentikan kecepatan naik saat CollisionFlags.Above, dan memanggil Teleport yang membersihkan input serta riwayat jatuh. Tes menggunakan collider atap rendah dan state jatuh lama lalu respawn/mendarat. Pemeriksa statis dijadikan langkah awal CI karena executor lokal sesi terputus; hasil verifikasi revisi ini harus dibaca dari run berikutnya.


Pemisahan tes dari renderer server: log foto terakhir menunjukkan g3_04 selesai dari detik 50 ke 333 untuk dua render llvmpipe. Menjalankan puluhan frame render penuh sebelum menguji kontrol dapat menghabiskan watchdog, dan mengukur jarak gerak dengan waktu nyata menguji kecepatan renderer. Harness kini menjaga kamera/physics/input aktif sambil menonaktifkan gambar dunia otomatis (mask kamera 0), mengukur gerak menurut waktu simulasi, lalu mengembalikan mask dunia+UI untuk tiga render manual 960x540 pada jarak maksimal 120 m. Enam foto survei kualitas tetap memakai jalur sebelumnya. Watchdog PlayMode menjadi 900 detik untuk render perangkat lunak. Ini tidak mengubah kamera APK dan tidak mengesahkan FPS perangkat.


CI d8e0ac8: kompilasi, Builder dua kali dan APK berhasil; 264 pemeriksaan lulus, satu pemeriksaan pose gagal sehingga foto belum berjalan. Kasus jongkok/atap, benturan kepala, respawn, indeks/persistensi pungutan dan 14 tombol lulus. Pemilih pose diperjelas ke karakter utama bernama Ningsih, mencakup renderer anak nonaktif dan mengaktifkan clone; laporan kini memuat jumlah vertex serta rerata X forearm/bahu. Batas geometrinya tetap. Pipeline menerbitkan snapshot bila Builder telah menulis stempel sumber, sehingga foto diagnostik bisa berjalan ketika self-test merah; kegagalan self-test tetap menggagalkan job Unity dan PR tetap draft.
