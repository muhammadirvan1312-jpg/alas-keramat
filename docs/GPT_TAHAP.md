# Cara ChatGPT Mengerjakan SATU TAHAP PENUH (Alas Keramat)

Kamu (ChatGPT, mode Work, terhubung ke GitHub repo ini) mengerjakan satu tahap utuh dan membuka Pull Request sendiri. Claude hanya memberi perintah, meninjau, dan membacakan error CI.
Baca juga `docs/KONTRAK_GPT.md` bagian 1–4 (proyek, alur, aturan, kontrak kode).

## Langkah wajib
1. Baca di cabang `main`: `AGENTS.md`, `docs/PROGRESS.md` (tabel riwayat T1… berisi kelas yang SUDAH ada — perluas, jangan buat ulang), bagian tahap di `docs/tech/12_TAHAPAN_DAN_PENGUJIAN.md`, dokumen `docs/gdd/` yang disebut, dan semua kode yang akan disentuh.
2. Buat cabang baru dari `main`: `tahap-<KODE>` (mis. `tahap-T6c`).
3. Runtime di `Assets/Scripts/<Modul>/` (namespace `AlasKeramat.<Modul>`), editor di `Assets/Editor/`.
4. **Builder**: file baru `Assets/Editor/BuilderSteps.<Nama>.cs` (`public static partial class NusantaraMasterBuilder`, namespace `AlasKeramat.EditorTools`) berisi `static void Langkah<Nama>()`; daftarkan di `Assets/Editor/NusantaraMasterBuilder.cs` dengan `Langkah("7x. ...", 0.95xf, Langkah<Nama>);` setelah langkah terakhir yang ada. Helper tersedia: `Anak, UIStretch, UIGambar, Tombol, TombolPanel, BuatTeks, Kotak2D, AmbilMat, MatPolos, Catat, EnsureFolder, FolderMat, FolderIkon, _root, _sistem, _ui, _sun`. Listener tombol: `UnityEditor.Events.UnityEventTools.AddPersistentListener` (atau `AddIntPersistentListener`).
5. **Self-test**: tambah cek di `Assets/Editor/NusantaraSelfTest.cs` tepat sebelum baris `cek("HUDInfo ada", ...)`: `cek("deskripsi", kondisiBool);`. Uji logika murni (method `public static`) agar jalan di Edit Mode tanpa Play (Awake/Update tidak berjalan). Objek uji sementara dihapus `Object.DestroyImmediate` di `finally`.
6. `docs/PROGRESS.md`: tambah 1 baris tabel tahapmu sebelum baris `Catatan spesifikasi`, ganti `AUTOPILOT_TAHAP_BERIKUTNYA:` ke tahap berikutnya.
7. **Pemeriksa statis `Tools/cek_statis.py` (CI gagal bila dilanggar)**:
   - Nama tipe (class/struct/enum, TERMASUK tipe bersarang) unik di seluruh proyek. Sudah terpakai antara lain: `Jenis, Data, Mode, Keadaan, Segmen, Rencana, DataSimpan, DataVital, Muatan, ModeBangun, Tipe, JenisKarakter, JenisHewan, JenisPungut, InfoPohon, Senjata, Ukuran`.
   - Kata `Animator` dilarang (termasuk komentar). Tanpa `// TODO`, tanpa `NotImplementedException`.
   - Kelas MonoBehaviour/ScriptableObject di file bernama sama.
   - Tanpa `??` / `?.` pada objek Unity. C# <= 9. <= 900 baris per file (pecah `partial`).
   - Bila sandbox-mu bisa menjalankan Python: `python3 Tools/cek_statis.py` sampai 0 masalah.
8. Konvensi: layer/tag dari `GameLayers`/`GameTags`; interaksi `IInteractable` (prompt Bahasa Indonesia KAPITAL) di layer `Interactable`; damage `IDamageable`; layanan `GameServices.Register/Get`; event `GameEvents.Raise*`; input sentuh lewat uGUI (keyboard hanya di `#if ENABLE_LEGACY_INPUT_MANAGER`); log `Debug.LogError("[AK][Kelas] ...")`; teks pemain Bahasa Indonesia.
9. Commit ke cabangmu, **buka Pull Request ke `main`** berjudul `Tahap <KODE>: <ringkasan>`. CI Unity ±25 menit; Autopilot merge bila hijau. Bila Claude mengirim error CI, perbaiki di cabang yang sama (PR otomatis diperbarui).
10. Balas singkat: daftar file, ringkasan fitur, nomor PR. JANGAN menampilkan kode di chat.
