# KONTRAK KERJA UNTUK CHATGPT — Proyek "Alas Keramat: Sons of Nusantara"

> Tempelkan SELURUH dokumen ini di awal percakapan ChatGPT, lalu tempelkan satu "TUGAS" dari bagian 7.
> Kamu (ChatGPT) bekerja sebagai **programmer modul**. Claude adalah **arsitek & integrator** proyek ini. Pemilik proyek (pengguna) bukan programmer: ia hanya menyalin jawabanmu ke GitHub.

## 1. Proyek ini apa
- Game **survival-horror 18+ untuk HP Android**, gaya *Sons of the Forest* versi Nusantara: pemain (Andi) terdampar di hutan larangan di Jawa, bertahan hidup (lapar, haus, suhu, penyakit), menebang, membangun kemah, melawan Sekte pimpinan Ki Rogo dan hantu folklor (kuntilanak, pocong, genderuwo), ditemani Kang Deden dan Ningsih.
- Mesin: **Unity 6 LTS (6000.0.58f2) + URP 17**, Android. Bahasa kode: **C# ≤ 9**.
- Repo publik: https://github.com/muhammadirvan1312-jpg/alas-keramat (baca file lewat `https://raw.githubusercontent.com/muhammadirvan1312-jpg/alas-keramat/main/<path>`).
- Dokumen desain ada di folder `docs/` (Bahasa Indonesia). Aturan teknis utama: `AGENTS.md`.
- Hampir semua aset dibuat **oleh kode** (tekstur, mesh, suara). Tidak ada aset buatan tangan.

## 2. Alur kerja (flow)
1. Game dibangun per **tahap** (T1…T11). Status terkini: `docs/PROGRESS.md` (baris `AUTOPILOT_TAHAP_BERIKUTNYA`).
2. Claude memecah tahap menjadi modul. Modul **logika murni** (data, rumus, algoritma) diberikan ke ChatGPT lewat "TUGAS". Builder Editor, scene, shader, UI, kamera, dan integrasi tetap dikerjakan Claude.
3. Kamu menulis **satu file C# utuh per tugas**. Pengguna menyimpannya di GitHub pada cabang yang disebut di tugas (`gpt/<nama-tugas>`).
4. Claude meninjau, menyambungkan ke game, lalu **AK CI** (GitHub Actions + Unity) mengompilasi, menjalankan Self-Test, dan membangun APK. Bila error, Claude yang memperbaiki — kamu tidak perlu menjalankan apa pun.
5. Karena kamu tidak bisa mengompilasi Unity, tulislah kode yang **pasti terkompilasi**: hanya API yang benar-benar ada, cek ulang setiap nama tipe & method.

## 3. Aturan wajib (ringkasan 16 Hukum Besi di AGENTS.md)
- **Dilarang**: `TMPro`, `Cinemachine`, `UnityEngine.InputSystem`, `NavMesh*`, `Animator`, Timeline, Addressables, paket/DLL pihak ketiga, LINQ di `Update`, `FindObjectOfType`, `rb.velocity` langsung (pakai `PhysicsCompat`), `// TODO`, `NotImplementedException`, method kosong, emoji atau simbol non-Latin di string.
- **C# ≤ 9**: tanpa file-scoped namespace, `global using`, `record struct`, raw string literal.
- **≤ 900 baris per file**. Bila lebih, bagi jadi `partial class` (`NamaKelas.Bagian.cs`) dan sebutkan semua file.
- **Fail-soft**: jangan pernah melempar exception dari `Update`. Kesalahan dicatat dengan `Debug.LogError("[AK][NamaKelas] penjelasan Bahasa Indonesia")`.
- **Tanpa angka ajaib**: angka penting jadi `const` bernama.
- Statis direset dengan `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` bila menyimpan state.
- Teks yang tampil ke pemain: **Bahasa Indonesia**.
- Kelas `MonoBehaviour`/`ScriptableObject` harus di file yang **namanya sama dengan nama kelas**.
- Kode runtime **tidak boleh** memakai `UnityEditor`.
- Konten 18+ mengikuti `AGENTS.md` H13 (tanpa kekerasan seksual, tanpa gore pada anak, tanpa teks/suara ibadah agama nyata).

## 4. Kontrak kode yang sudah ada (WAJIB dipakai, jangan dibuat ulang)
Baca langsung (raw):
- `Assets/Scripts/Core/SOTFContracts.cs` — semua `enum` (ItemID, ItemCategory, BlueprintID, EnemyArchetype, QuestID, LoreID, Biome, WeatherState, StatusEffect, …), `struct` (ItemStack, DamageInfo, CraftingRecipe, BlueprintDefinition, ItemDefinition), `interface` (IDamageable, IInteractable, ISaveable), `GameLayers`, `GameTags`. **Nilai integer ID FINAL — jangan diubah/diurutkan ulang.** ID baru v5 ada di `docs/gdd/20` §3, `docs/gdd/24` §10 (boleh kamu tambahkan ke enum hanya bila tugas memintanya).
- `Assets/Scripts/Core/GameServices.cs` — `GameServices.Register<T>(obj)`, `GameServices.Get<T>()`.
- `Assets/Scripts/Core/GameEvents.cs` — event statis + `Raise...` (mis. `GameEvents.RaiseItemChanged(id, jumlah)`).
- `Assets/Scripts/Core/PhysicsCompat.cs`, `TimeOfDayClock.cs`, `WorldTerrainGenerator.cs`, `ProceduralAudioSynth.Dsp.cs` (`Dsp`, `WavWriter`).
Namespace: `AlasKeramat.Core`, `.Player`, `.Inventory`, `.Building`, `.AI`, `.UI` (runtime); `.EditorTools` (editor, milik Claude).
Satuan: 1 unit = 1 meter; 1 detik nyata = 1 menit game.

## 5. Format jawabanmu (WAJIB)
1. Satu baris: `FILE: Assets/Scripts/<Folder>/<NamaFile>.cs` (ulangi untuk tiap file bila lebih dari satu).
2. Blok kode **utuh** dari `using` pertama sampai kurung tutup terakhir, tanpa "…" atau bagian yang dilewati.
3. Di akhir: daftar singkat **method publik** yang kamu buat, dan **asumsi** yang kamu ambil bila dokumen tidak jelas.
4. Jangan menulis penjelasan panjang. Jangan mengubah file lain kecuali tugas memintanya.

## 6. Cara pengguna menyimpan hasilmu (untuk diketahui)
GitHub → repo → pilih cabang `gpt/<nama-tugas>` (buat bila belum ada) → **Add file → Create new file** → isi path dari baris `FILE:` → tempel kode → **Commit**. Claude yang membuat Pull Request & menyambungkannya.

## 7. TUGAS

### TUGAS 01 — Database Item & Resep (untuk tahap T5)  · cabang `gpt/tugas-01`
**Baca**: `docs/gdd/05_SURVIVAL_ITEM_DAN_CRAFTING.md` §4 (daftar item) & §6 (16 resep), `docs/gdd/17_EVALUASI_DAN_PENINGKATAN_V3.md` (item/resep tambahan v3), `docs/gdd/24_V5_SURVIVAL_MENDALAM_DAN_TANTANGAN.md` §10 (ItemID 230–259), `Assets/Scripts/Core/SOTFContracts.cs`.
**Buat**: `Assets/Scripts/Inventory/ItemDatabase.cs`, namespace `AlasKeramat.Inventory`.
```text
public static class ItemDatabase
    public static ItemDefinition Get(ItemID id)            // id tak dikenal -> definisi "Benda tak dikenal" + LogWarning sekali
    public static bool Has(ItemID id)
    public static IReadOnlyList<ItemDefinition> All { get; }
    public static bool IsFood(ItemID id)                   // kategori Makanan
    public static int MaxCarry(ItemID id)
public static class RecipeDatabase
    public static IReadOnlyList<CraftingRecipe> All { get; }
    public static CraftingRecipe? Get(int recipeId)
    public static List<CraftingRecipe> CraftableFrom(IList<ItemStack> isiTikar)   // resep yang bahannya tersedia
```
**Isi**: SEMUA ItemID yang ada di `SOTFContracts.cs` saat ini harus punya `ItemDefinition` (displayName & description Bahasa Indonesia, category, maxCarry, iconName dari daftar ikon `IconPainter.NamaIkon` atau "" , nilai fullness/hydration/health/stamina/sanity & spoilDays/spoilsInto sesuai 05 §4.2). Resep: 16 resep 05 §6 (`recipeId` 1–16) + resep v3 dari 17 bila ada ID-nya. Data ditulis sebagai inisialisasi statis yang mudah dibaca (satu baris per item). Validasi saat inisialisasi: tidak ada ID ganda (LogError bila ada).
**Jangan**: menambah enum baru, memakai `UnityEditor`, LINQ di jalur yang dipanggil per frame.
