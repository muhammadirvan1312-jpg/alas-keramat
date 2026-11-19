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
