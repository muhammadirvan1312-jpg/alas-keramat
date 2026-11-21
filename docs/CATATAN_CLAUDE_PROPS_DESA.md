# Catatan Claude: Props Desa CC0, Lampu Minyak & Kelelawar Panik

Cabang: `imp-claude-props-desa`, dibuat dari `trial-lengkap` (6fad927). **Belum diuji CI.** Sengaja belum dibuka PR, sesuai permintaan pengguna, supaya diuji bareng hasil Antigravity.
Tidak tumpang tindih dengan V1-V5 maupun SOTF Gelombang 3-23. Belum ada props Poly Haven, lampu desa, atau reaksi kelelawar terhadap senter di cabang itu.

## Isi
1. **13 props CC0 Poly Haven**. Model 1k FBX ditambah tekstur diff dan nor_gl, dikunci di `Tools/aset_cc0.lock.json` dengan sha256.
   - Daftar props: peti kayu, drum, bakul, ember kayu, periuk, dingklik, jeriken, ban bekas, panci enamel, tangga, bangku, kapak, dan lampu minyak.
   - Dipanggang oleh `Tools/visual/panggang_pohon.py` (blok `PROPS`, mode `whole=True`) menjadi `Assets/ThirdParty/Prepared/*Scan.bytes`. Nama mesh juga ditambahkan ke `EXPECTED` di `siapkan.py`.
   - Sudah diuji panggang lokal dengan bpy. Semua 13 mesh keluar, 300-700 segitiga per props, kecuali bakul sekitar 2,3 ribu karena anyamannya tidak bisa didesimasi lebih jauh.
2. **Langkah Builder 7z8** (`Assets/Editor/BuilderSteps.PropDesa.cs`). Props ditebar di:
   - halaman 34 rumah: Desa Mati, Kampung Sukamaju, dan Kampung Watu Ireng;
   - lokasi jatuh heli: jeriken, drum, ban, dan peti;
   - halaman Pabrik Gula: drum, ban, jeriken, peti, dan tangga.

   Aturan penempatan:
   - Penempatan deterministik (seed 4127) dan idempoten.
   - Posisi dicek dulu: tidak boleh menabrak bangunan (`CheckSphere` pada layer Structure) dan lereng harus landai.
   - Sebagian ember dan dingklik sengaja terguling, kapak bersandar miring, supaya kesan desa ditinggal mendadak terasa.
   - Props besar memakai BoxCollider. Semua props diberi LODGroup dengan cull di 1,5-3,5% tinggi layar dan BatchingStatic.
   - Material dibuat lewat `MaterialSolidScan` (shader AK Kulit, tekstur foto + normal).
3. **Lampu minyak malam** (`Assets/Scripts/Core/LampuMinyak.cs`).
   - Lampu diletakkan di atas peti di setiap rumah Desa Mati dan sebagian rumah kampung.
   - Api emisif menyala jam 18:00-05:30 dan berkedip.
   - Hemat HP 4 GB:
     - paling banyak 2 Light titik aktif, yaitu lampu terdekat dalam radius 32 m;
     - tanpa bayangan, jangkauan 5,5 m;
     - api di luar 90 m dimatikan;
     - pemilihan lampu diperbarui 2x per detik tanpa alokasi.
4. **Kelelawar gua panik** (`Assets/Scripts/AI/KelelawarGua.cs`). Bila tersorot senter Andi (kerucut 24 derajat, jarak 10 m atau kurang), kelelawar terbang lebih cepat, lebih lebar, dan naik ke langit-langit selama 3,2 detik. Klip kepaknya juga dipercepat.
5. **Self-test** `Assets/Editor/PemeriksaanPropDesa.cs` (dipanggil dari `NusantaraSelfTest`):
   - jam nyala lampu dan rentang kedip;
   - 13 jenis props unik;
   - minimal 50 props dan 10 lampu, tanpa material pink;
   - cahaya lampu mati di awal, tanpa bayangan, jangkauan 6 m atau kurang;
   - kerucut sorot kelelawar.
6. **Foto CI** baru di `NusantaraFoto`: `c3_01_props_desa_siang` dan `c3_02_lampu_minyak_malam` (dekat RumahDesa_1).

## Cara menggabungkan
`git merge origin/imp-claude-props-desa` ke cabang uji Antigravity.

Kemungkinan konflik kecil, cukup ambil kedua sisi:
- satu baris `Langkah(...)` di `NusantaraMasterBuilder.cs`;
- satu baris di `NusantaraSelfTest.cs`;
- dua `Shot` di `NusantaraFoto.cs`;
- entri di `aset_cc0.lock.json`, yang digabung per `path`.

Hash lock dan skrip panggang berubah, jadi cache `thirdparty` dan `Prepared` di CI akan dibangun ulang. Wajar bila build pertama lebih lama.
