# AGENTS.md — "ALAS KERAMAT: SONS OF NUSANTARA" (v5.0)
# Survival-horror dewasa (18+) open world FPS mobile, terinspirasi loop inti Sons of the Forest, berlatar folklor Nusantara.
# Runtime: Unity 6 LTS (6000.x) — kompatibel mundur ke Unity 2022.3 LTS — Universal Render Pipeline — Android (HP saja).
# Simpan file ini di ROOT proyek Unity (sejajar `Assets/`). Folder `docs/` WAJIB ikut disalin di root yang sama.

> PENTING UNTUK CODEX: file ini sengaja ringkas (< 32 KiB) karena Codex memotong AGENTS.md di atas batas itu.
> Semua detail desain ada di `docs/`. Kamu WAJIB membaca dokumen `docs/` yang relevan sebelum menulis kode (lihat Bagian 3).
> Jika AGENTS.md dan `docs/` bertentangan: AGENTS.md menang untuk aturan teknis, `docs/gdd/` menang untuk angka & isi desain. Dokumen v3 (13–17) mengalahkan dokumen 01–10 bila angkanya berbeda.

---

## BAGIAN 0 — MODE CLOUD (v4, BERLAKU SEKARANG)
Pengguna menjalankan **Codex Cloud** (tugas dipicu dari issue `AUTOPILOT` / komentar `@codex`). Akibatnya:
- Container kamu **tidak punya Unity**. Jangan pernah mengklaim kode "sudah terkompilasi". Hakimnya adalah workflow **AK CI** (GameCI) di GitHub Actions; hasilnya kamu baca di `Logs/ci/LAPORAN_CI.md` pada tugas perbaikan. Ikuti `docs/tech/18_PIPELINE_CLOUD_OTONOM.md` §5 (cek statis, satu PR per tahap, pecah tahap bila perlu).
- Tahap pertama adalah **T0-C** (18 §4): proyek Unity dibuat dari nol oleh kamu (ProjectVersion, manifest, NusantaraCI, pengunduh aset).
- Tekstur foto, model, dan HDRI **CC0** (Poly Haven, ambientCG) serta gerak **motion-capture CMU** WAJIB dipakai sesuai `docs/gdd/19` — diunduh `Tools/ambil_aset.py` ke `Assets/ThirdParty/` (tidak dikomit, dikunci lewat lock file). Prosedural tetap menjadi cadangan bila aset tidak ada.
- Di awal SETIAP tugas baca `docs/PROGRESS.md` dan `docs/UMPAN_BALIK.md` (masukan pengguna dari HP; wajib ditindaklanjuti). Di akhir, perbarui baris `AUTOPILOT_TAHAP_BERIKUTNYA:` di PROGRESS (dibaca workflow Autopilot).
- Gerbang kualitas G1–G4 (18 §6), termasuk **Irisan Vertikal** setelah T7 dan rubrik foto (19 §7).
- Aturan lokal (UNITY_EXE, Editor.log, skrip cek_kompilasi) di H12 hanya berlaku bila suatu saat dijalankan di komputer lokal.

## BAGIAN 0b — V5 KUALITAS MAKSIMAL (BERLAKU SEKARANG, mengalahkan 01–19)
- Pengguna memilih **kualitas di atas segalanya**. Baca `docs/gdd/20` (dunia 2×2 km 16 tile + streaming, 12 zona, 6 kompleks bawah tanah, tekstur 2K/4K, mocap ≤ 220 klip, mekanik baru, preset Ultra) dan `docs/gdd/21` (audio v5: rekaman CC0 + prosedural, HRTF-lite, akustik dunia, gelombang hening, suara makhluk, musik adaptif) di SETIAP tahap.
- Batas di H10 & 11 §7 diganti tabel 20 §1. Audio tidak lagi 100% prosedural: rekaman CC0 wajib sesuai 21 §2 (prosedural = cadangan).
- Koordinat permukaan 03/16 dikali 2 (20 §2). ID baru 20 §3 & §2.3 ditambahkan ke `SOTFContracts` saat tahap yang memakainya.
- **Karakter realistis** MakeHuman/MPFB2 CC0 (`docs/gdd/22`) menggantikan karakter kotak 02 & aturan "rigid skinning" H10; tetap tanpa Animator (rig Generic + `ProceduralAnimator`). **Cerita 6 babak, 42 cutscene, 3 ending** (`docs/gdd/23`) melengkapi 04.
- Tahap kini dikerjakan per sesi (bukan Codex Cloud); Autopilot hanya merge PR hijau + rilis APK. Aturan cloud (Bagian 0) tetap berlaku.

## BAGIAN 1 — PERAN KAMU & PROFIL PENGGUNA

Kamu adalah **Principal Game Director + Lead Narrative Designer + Principal Unity Engine Architect** untuk proyek ini.

Pengguna adalah **100% NON-CODER**:
- TIDAK BISA menulis/membaca C#, menyambung referensi di Inspector, membuat Animator Controller, prefab, Tag/Layer, atau material.
- TIDAK PUNYA aset 3D, tekstur, audio, atau font eksternal. Semua aset dibuat prosedural oleh kode.
- Yang BISA dilakukan pengguna: membuka Unity, menunggu kompilasi, mengklik menu `Nusantara Builder`, menekan Play, menjalankan checklist uji di `docs/tech/12_TAHAPAN_DAN_PENGUJIAN.md`, menyalin pesan error ke kamu.

Artinya: setiap langkah yang biasanya dilakukan manusia di Editor harus kamu otomatiskan lewat script Editor, dan setiap kegagalan harus terbaca jelas di log berbahasa Indonesia.

---

## BAGIAN 2 — 16 HUKUM BESI (MELANGGAR SATU = TAHAP DIANGGAP GAGAL)

### H1. Murni modul bawaan Unity + URP
- BOLEH: `UnityEngine.*` (Core, Physics, Audio, Animation-free, ParticleSystem, UI/uGUI `UnityEngine.UI`, `UnityEngine.EventSystems`), `UnityEngine.Rendering` + `UnityEngine.Rendering.Universal` (paket URP bawaan template), `UnityEditor.*` (hanya di folder `Assets/Editor/`), serta shader HLSL buatan sendiri di `Assets/Shaders/` (H15), Unity Terrain bawaan (`TerrainData`, `TerrainLayer`, shader URP `Terrain/Lit`), dan aset CC0/mocap yang diunduh sesuai `docs/gdd/19`.
- DILARANG: `TMPro`, `Cinemachine`, `UnityEngine.InputSystem` (sebagai `using`/referensi compile-time), `Unity.AI.Navigation`, `NavMeshAgent`, `NavMesh`, `Animator`/`AnimatorController`, Timeline, ProBuilder, Addressables, paket Asset Store, DLL pihak ketiga, `ScriptableRendererFeature` kustom.
- Jika butuh tipe dari paket yang mungkin ada (mis. modul UI Input System), akses lewat **reflection** dengan pengecekan null (lihat H3).

### H2. Nol sambungan manual
- Tidak boleh ada field `[SerializeField]` yang WAJIB diisi tangan. Semua referensi diisi oleh `NusantaraMasterBuilder` (via `SerializedObject`) ATAU dicari saat runtime lewat `GameServices.Get<T>()`.
- Setiap komponen harus tetap berfungsi (fail-soft) bila referensinya kosong: cari ulang, buat ulang, atau nonaktifkan diri dengan log `[AK][NamaKelas] ...`.

### H3. Input sentuh tanpa paket Input System
- SEMUA input gameplay berasal dari komponen uGUI (`IPointerDownHandler`, `IDragHandler`, `IPointerUpHandler`): joystick, area geser kamera, tombol aksi. Jangan memanggil `Input.GetTouch`/`Input.touches` untuk gameplay.
- `EventSystem` dibuat oleh Builder dengan modul yang benar:
```csharp
public static void AddCorrectInputModule(GameObject esGo) {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
    var t = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
    if (t != null) {
        var c = esGo.AddComponent(t);
        var m = t.GetMethod("AssignDefaultActions");
        if (m != null) m.Invoke(c, null);
        return;
    }
    Debug.LogError("[AK][Input] Proyek memakai Input System (New) tetapi modul UI tidak ditemukan.");
#else
    esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
}
```
- Kontrol keyboard/mouse untuk uji di Editor HANYA di dalam `#if ENABLE_LEGACY_INPUT_MANAGER`. (Template Unity 6 terbaru sering default ke "Input System Package (New)"; kode di atas membuat game tetap jalan tanpa mengubah setting.)

### H4. Teks & ikon
```csharp
static Font _safeFont;
public static Font GetSafeFont() {
    if (_safeFont != null) return _safeFont;
    _safeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    if (_safeFont == null) _safeFont = Font.CreateDynamicFontFromOSFont(new[] { "Roboto", "Noto Sans", "Arial" }, 32);
    return _safeFont;
}
```
- JANGAN memanggil `GetBuiltinResource<Font>("Arial.ttf")` (sudah dihapus sejak 2022.2 dan melempar error).
- DILARANG emoji & simbol non-Latin di `UI.Text` (🌧️ 🔥 🎒 📖 ★ ⌂ dll. tampil sebagai kotak kosong di HP). Semua ikon digambar prosedural oleh `IconPainter` menjadi `Sprite` (lihat `docs/gdd/09_UI_UX_MOBILE.md` §8). Nama menu Editor juga tanpa emoji.

### H5. Material, shader, tekstur, mesh — dipanggang jadi aset
- Gunakan `SafeMaterialFactory` (di `ProceduralAssetFactory.cs`). Jangan pakai operator `??` pada objek Unity; pakai `if (x == null)`.
```csharp
public enum SurfaceMode { Opaque, Cutout, Transparent }
static Shader _lit;
public static Shader LitShader() {
    if (_lit == null) _lit = Shader.Find("Universal Render Pipeline/Lit");
    if (_lit == null) _lit = Shader.Find("Standard");
    return _lit;
}
public static Material CreatePBR(string name, Color albedo, float metallic, float smoothness,
    Texture2D mainTex = null, Texture2D bumpTex = null, Color? emission = null,
    SurfaceMode mode = SurfaceMode.Opaque, float cutoff = 0.45f) {
    var m = new Material(LitShader()); m.name = name;
    SetC(m, "_BaseColor", albedo); SetC(m, "_Color", albedo);
    SetF(m, "_Metallic", metallic); SetF(m, "_Smoothness", smoothness); SetF(m, "_Glossiness", smoothness);
    if (mainTex != null) { SetT(m, "_BaseMap", mainTex); SetT(m, "_MainTex", mainTex); }
    if (bumpTex != null && m.HasProperty("_BumpMap")) { m.SetTexture("_BumpMap", bumpTex); m.EnableKeyword("_NORMALMAP"); }
    if (emission.HasValue && emission.Value.maxColorComponent > 0.01f) {
        m.EnableKeyword("_EMISSION"); SetC(m, "_EmissionColor", emission.Value);
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
    }
    if (mode == SurfaceMode.Cutout) {
        SetF(m, "_AlphaClip", 1f); SetF(m, "_Cutoff", cutoff); SetF(m, "_Cull", 0f);
        m.EnableKeyword("_ALPHATEST_ON"); m.renderQueue = 2450;
    } else if (mode == SurfaceMode.Transparent) {
        SetF(m, "_Surface", 1f); SetF(m, "_Blend", 0f); SetF(m, "_ZWrite", 0f);
        SetF(m, "_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        SetF(m, "_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.SetOverrideTag("RenderType", "Transparent");
        m.renderQueue = 3000;
    }
    m.enableInstancing = true;
    return m;
}
// SetC/SetF/SetT = helper yang mengecek m.HasProperty(nama) sebelum Set.
```
- Builder WAJIB menyimpan semua tekstur (`.png` via `EncodeToPNG` + `AssetDatabase.ImportAsset`), material (`.mat`), mesh (`.asset`), `VolumeProfile`, dan skybox ke `Assets/Generated/...` sehingga shader & keyword ikut ter-include di build Android (material buatan runtime dengan `Shader.Find` bisa tampil PINK di build).
- Builder hanya menambahkan "Skybox/Procedural" ke *Always Included Shaders*. **JANGAN** memasukkan shader URP (Lit punya > 1 juta varian, build Android gagal — terbukti di CI T1); shader URP ikut ter-build lewat material `.mat` di `Assets/Generated/`.
- Runtime hanya boleh membuat material baru via `SafeMaterialFactory` (dengan cache) — terutama untuk instance warna/efek sesaat.

### H6. AI tanpa NavMesh
- Lokomosi: `CharacterController` + ground snap (`Physics.Raycast` ke layer `Ground`, fallback `WorldTerrainGenerator.SampleHeight(x,z)` di permukaan) + `Physics.SphereCast` untuk menghindari halangan.
- Pathfinding: `GridPathfinder` (A* di grid 2 m yang dipanggang Builder ke `Assets/Generated/Nav/SurfaceGrid.bytes`) + `WaypointGraph` buatan tangan-kode untuk tiap gua. Batas ekspansi node per frame (lihat `docs/tech/11`).
- Di gua (Y = -80) JANGAN memakai `SampleHeight`; pakai raycast + waypoint.

### H7. Builder 1-klik & idempoten
- Menu wajib: `Nusantara Builder/1. BANGUN SELURUH GAME (1-KLIK)` + submenu per tahap & utilitas (daftar lengkap di `docs/tech/11` §9).
- Setiap klik: (1) daftar Tag/Layer via `SerializedObject` pada `ProjectSettings/TagManager.asset`, (2) hapus root lama `[ALAS_KERAMAT_ROOT]` + regenerasi isi `Assets/Generated/`, (3) bangun ulang semuanya, (4) atur URP/Quality/Player untuk mobile, (5) `EditorSceneManager.MarkSceneDirty` + `SaveScene` ke `Assets/Scenes/AlasKeramat_Main.unity` dan daftarkan di Build Settings, (6) tampilkan dialog ringkasan + tulis laporan `Logs/nusantara_build_report.txt`.
- Gunakan `EditorUtility.DisplayProgressBar` selama build dan `ClearProgressBar` di `finally`.

### H8. Kode lengkap, tapi dipecah rapi
- Dilarang: `// TODO`, `throw new NotImplementedException()`, method kosong pura-pura, komentar "implementasi disederhanakan", angka ajaib tanpa konstanta bernama.
- Batas ±900 baris per file. Jika modul lebih besar, pecah memakai `partial class` dengan nama file `NamaKelas.Bagian.cs` (contoh `AndiFPSController.Swim.cs`). Nama class & method publik di 17 file wajib TIDAK BERUBAH.
- Satu sesi Codex = satu tahap (lihat `docs/tech/12`). Jangan menulis semua tahap sekaligus kecuali pengguna memakai Cara A.

### H9. Kompatibel Unity 2022.3 & Unity 6
- C# maksimal versi 9: dilarang file-scoped namespace, `global using`, `record struct`, raw string literal.
- `FindFirstObjectByType` / `FindObjectsByType(FindObjectsSortMode.None)` — bukan `FindObjectOfType`.
- Kecepatan Rigidbody HANYA via `PhysicsCompat` (`#if UNITY_6000_0_OR_NEWER` → `linearVelocity`/`linearDamping`/`angularDamping`, selain itu `velocity`/`drag`/`angularDrag`).
- Baca pipeline aktif via `GraphicsSettings.currentRenderPipeline`; jangan menulis `GraphicsSettings.renderPipelineAsset`.
- Statis di-reset dengan `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` (aman bila Domain Reload dimatikan).

### H10. Anggaran performa HP (detail di `docs/tech/11` §7)
- Target 30 FPS stabil di HP kelas menengah (contoh: Snapdragon 720G / Helio G99, RAM 4–6 GB). Preset Tinggi boleh 60 FPS.
- ≤ 150 batch/SetPass di area terpadat, ≤ 250 ribu segitiga terlihat, ≤ 12 musuh aktif, ≤ 40 Rigidbody kayu aktif, ≤ 24 suara bersamaan.
- Karakter = 1 `SkinnedMeshRenderer` + 1 material atlas per karakter (rigid skinning). Vegetasi jauh = `Graphics.RenderMeshInstanced`. Tanpa baked lighting.
- Dilarang alokasi GC per frame di `Update` (tanpa LINQ, tanpa `new List` di loop, tanpa string concat per frame untuk UI — update teks hanya saat nilai berubah).

### H11. Fail-soft & log berbahasa Indonesia
- Tidak boleh ada exception di `Update`/`FixedUpdate`. Bungkus inisialisasi besar dengan try/catch yang menulis `Debug.LogError("[AK][Kelas] Penjelasan Bahasa Indonesia: ...")`.
- `NusantaraDiagnostics` (runtime) menyalin semua Warning/Error saat Play di Editor ke `Logs/nusantara_runtime.log` agar kamu bisa membacanya.

### H12. Lingkaran verifikasi kompilasi (WAJIB sebelum menyatakan tahap selesai)
1. Jika variabel lingkungan `UNITY_EXE` ada dan Unity Editor sedang TERTUTUP: jalankan `Tools/cek_kompilasi.bat` (Windows) atau `Tools/cek_kompilasi.sh` (macOS/Linux). Perbaiki semua baris `error CS` sampai bersih.
2. Jika tidak bisa menjalankan Unity: baca `Editor.log` (Windows `%LOCALAPPDATA%\Unity\Editor\Editor.log`, macOS `~/Library/Logs/Unity/Editor.log`, Linux `~/.config/unity3d/Editor.log`) dan `Logs/nusantara_runtime.log` bila ada.
3. Lakukan "kompilasi mental": cek setiap `using`, setiap method yang dipanggil benar-benar ada, tanda tangan antar-file cocok dengan `SOTFContracts.cs`, tidak ada nama ganda dalam namespace yang sama.

### H13. Konten dewasa, budaya & legal (v3)
- Game horor dewasa **18+**: darah, luka, mutilasi, ragdoll, mayat diatur `GoreLevel` (Mati/Sedang/Penuh) persis sesuai `docs/gdd/14`. Layar peringatan 18+ wajib saat pertama dibuka.
- Batas mutlak (tidak boleh dibuat walau diminta di tempat lain): kekerasan seksual; menyiksa makhluk tak berdaya secara interaktif; gore pada sosok berwujud anak; pemain memakan manusia; mutilasi Andi/Deden/Ningsih; penodaan simbol agama nyata.
- Folklor boleh; agama nyata tidak boleh diolok. DILARANG mensintesis adzan, sholawat, atau bacaan kitab suci. Penanda Maghrib = langit merah + kentongan + angin. Makam Tua tidak bisa digali.
- Sekte, Ki Rogo, PT Bhumi Cakra fiktif. Jangan memakai nama, logo, model, atau teks "Sons of the Forest" di game, judul, atau metadata build.

### H14. Kontinuitas antar-sesi
- Awal setiap sesi: baca `docs/PROGRESS.md`. Akhir setiap sesi: perbarui `docs/PROGRESS.md` (tahap, file yang berubah, hasil cek kompilasi, hal yang belum lulus uji, keputusan teknis baru).
- Jangan menghapus fitur tahap sebelumnya. Jika harus mengubah kontrak di `SOTFContracts.cs`, perbarui SEMUA pemakainya dalam sesi yang sama dan catat di PROGRESS.

### H15. Shader tulisan tangan (AK Shader Pack)
- BOLEH & dianjurkan: file `.shader` + `.hlsl` HLSL URP buatan sendiri di `Assets/Shaders/` sesuai `docs/gdd/15` §2 (bukan paket eksternal). DILARANG Shader Graph (`.shadergraph`), Amplify, renderer feature kustom.
- Setiap shader: `Fallback "Universal Render Pipeline/Lit"`, pass ShadowCaster & DepthOnly, properti di `UnityPerMaterial` (SRP Batcher), lolos `ShaderUtil.ShaderHasError == false` di `NusantaraSelfTest`. Bila tidak didukung perangkat → `SafeMaterialFactory` memakai Lit setara.

### H16. Teror & gore dikendalikan sistem, bukan tersebar
- Semua kejadian seram lewat `TerrorDirector` (anggaran & fase `docs/gdd/13` §2) — tidak ada jump scare yang di-hardcode di tempat lain kecuali set-piece terdaftar di 13 §8.
- Semua darah/luka/mutilasi lewat `GoreSystem` yang membaca `GoreLevel`; tidak ada efek gore yang melewati pengaturan ini.

---

## BAGIAN 3 — INDEKS DOKUMEN (`docs/`)

| File | Isi | Wajib dibaca saat |
|---|---|---|
| `docs/gdd/01_VISI_PILAR_DAN_ARAH_SENI.md` | Visi, pilar desain, matriks paritas fitur SOTF→Nusantara, arah seni, palet warna, batas lingkup | Semua tahap (sekali per sesi) |
| `docs/gdd/02_KARAKTER.md` | Anatomi 20 sendi, ukuran, atlas warna, wajah, pakaian, animasi prosedural Andi/Deden/Ningsih/Ki Rogo/Sekte/Hantu | T4, T7, T8, T10 |
| `docs/gdd/03_DUNIA_DAN_LEVEL.md` | Rumus `SampleHeight`, sungai, 7 bioma + koordinat props, 3 kompleks bawah tanah (denah ruang), vegetasi, hewan | T2, T5, T10 |
| `docs/gdd/04_CERITA_MISI_DAN_LORE.md` | 5 babak, 13 misi + pemicu, grafik gerbang progresi, 12 dokumen lore (teks final), naskah cutscene & 2 ending | T10 |
| `docs/gdd/05_SURVIVAL_ITEM_DAN_CRAFTING.md` | Rumus vitals, cuaca/basah/dingin, daftar ItemID final, makanan & masak, 16 resep, zirah | T3, T5, T6 |
| `docs/gdd/06_TEMPUR_DAN_MUSUH.md` | Sistem tempur, tabel senjata, parry, stat & state machine Sekte/Hantu/Boss, spawner, agresi, raid, siege | T7, T10 |
| `docs/gdd/07_POHON_DAN_BANGUNAN.md` | Tebang 4 kuadran, log fisik, panggul bahu, 5 mode freeform, integritas struktur, 14 blueprint, zipline | T5, T6 |
| `docs/gdd/08_COMPANION.md` | AI Kang Deden (15 perintah Buku Catatan) & Ningsih (Trust, hadiah, senjata ganda, indra keenam) | T8 |
| `docs/gdd/09_UI_UX_MOBILE.md` | Tata letak HUD 1920×1080, ukuran tombol, warna hex, semua layar, ikon prosedural, pengaturan | T3, T5, T6, T8, T11 |
| `docs/gdd/10_AUDIO_PROSEDURAL.md` | Blok DSP, rumus semua SFX, ambien siang/malam, musik pelog/slendro, mixing | T1, T2, T5, T7 |
| `docs/gdd/13_HOROR_DAN_TEROR.md` | Sutradara Teror (Tension & 4 fase), 37 kejadian seram, folklor sebagai mekanik, audio binaural, kegelapan, getar | T7, T9, T10 |
| `docs/gdd/14_GORE_DAN_KEKERASAN.md` | Rating 18+, GoreLevel, darah & decal, luka, Pendarahan, mutilasi rigid-skinned, ragdoll, mayat, dekorasi | T4, T7, T9 |
| `docs/gdd/15_VISUAL_FIDELITY_DAN_SHADER.md` | AK Shader Pack, pipa tekstur kualitas tinggi, wajah 512, LUT, sinar tajuk, cuaca visual, animasi hidup, jalur CC0 | T2, T4, T6, T11 |
| `docs/gdd/16_DUNIA_HIDUP.md` | Ekosistem, lokasi baru, jadwal & sosial Sekte, 13 kejadian dunia, keseharian & catatan Deden, cerita lingkungan | T9 |
| `docs/gdd/17_EVALUASI_DAN_PENINGKATAN_V3.md` | Rapor v2, senjata/resep/item/blueprint/musuh baru (ID final), stealth, cerita, UI imersif, mode kustom | T6, T7, T8, T10, T11 |
| `docs/gdd/19_VISUAL_V4_ASET_CC0_DAN_MOCAP.md` | Tekstur/model/HDRI CC0, mocap CMU, Unity Terrain, sinematografi wajah, rubrik foto, 12 money shot (mengalahkan 15) | T0-C, T2, T4, T5, T6, G2 |
| `docs/gdd/20_V5_KUALITAS_MAKSIMAL.md` | V5: dunia 2×2 km, zona & bawah tanah baru, anggaran Ultra, visual & mekanik baru, pembagian tahap | SETIAP tahap |
| `docs/gdd/21_AUDIO_V5_HIDUP_DAN_MENAKUTKAN.md` | V5 audio: rekaman CC0, HRTF-lite, akustik, ambien hidup, suara makhluk, musik adaptif, Uji Dengar (G5) | SETIAP tahap |
| `docs/gdd/22_V5_KARAKTER_REALISTIS.md` | V5: karakter MakeHuman/MPFB2 dibuat Blender headless di CI, wajah 12 ekspresi + visem, kulit/rambut/mata HLSL, gore segmen | T1, T4, T7, T10 |
| `docs/gdd/23_V5_CERITA_DAN_SINEMATIK.md` | V5: sejarah perjanjian, 6 babak, misi M14–M18 & S05–S08, 3 ending, `CutsceneDirector`, 42 cutscene, dialog bercabang | T3, T4, T10 |
| `docs/tech/18_PIPELINE_CLOUD_OTONOM.md` | Codex Cloud + GameCI + Autopilot, T0-C, aturan kerja cloud, gerbang G1–G4, bila macet | SETIAP tugas |
| `docs/EVALUASI_V4.md` · `docs/UMPAN_BALIK.md` | Rapor jujur & prioritas · masukan pengguna dari HP | Awal setiap tugas |
| `docs/tech/11_ARSITEKTUR_TEKNIS.md` | Manifest file, namespace, kode kontrak, hierarki scene, Tag/Layer, aset Generated, save JSON, performa, menu Builder | SETIAP tahap |
| `docs/tech/12_TAHAPAN_DAN_PENGUJIAN.md` | Tahap T0–T12 (T12 opsional), prompt Codex per tahap, checklist uji non-coder, troubleshooting | SETIAP tahap |
| `docs/PROGRESS.md` | Catatan progres yang kamu perbarui | Awal & akhir sesi |

---

## BAGIAN 4 — MANIFEST FILE (RINGKAS; lengkap di `docs/tech/11` §1)

17 file inti dari spesifikasi v1 TETAP ADA dengan nama class sama:

| # | Path | Class utama |
|---|---|---|
| 1 | `Assets/Scripts/Core/SOTFContracts.cs` | enum, struct, interface bersama |
| 2 | `Assets/Scripts/Core/ProceduralAssetFactory.cs` | `ProceduralAssetFactory`, `SafeMaterialFactory`, `ProcTex`, `ProcMesh`, `CharacterMeshBuilder`, `IconPainter` |
| 3 | `Assets/Scripts/Core/ProceduralAudioSynth.cs` | `ProceduralAudioSynth`, `Dsp` |
| 4 | `Assets/Scripts/Core/URPAtmosphereAndWeather.cs` | `URPAtmosphereAndWeather` |
| 5 | `Assets/Scripts/Core/WorldCaveAndSaveManager.cs` | `WorldCaveAndSaveManager`, `QuestManager`, `LoreLibrary`, `SaveSystem` |
| 6 | `Assets/Scripts/Player/AndiFPSController.cs` | `AndiFPSController` |
| 7 | `Assets/Scripts/Player/AndiVitalsAndSanity.cs` | `AndiVitalsAndSanity` |
| 8 | `Assets/Scripts/Player/CombatAndLeftHandController.cs` | `CombatAndLeftHandController` |
| 9 | `Assets/Scripts/Player/MobileTouchHUDAndRadar.cs` | `MobileTouchHUDAndRadar` (HUD, radar, layar lore, toast, subtitle) |
| 10 | `Assets/Scripts/Inventory/TikarPandan3DInventory.cs` | `TikarPandan3DInventory`, `CraftingBook` |
| 11 | `Assets/Scripts/Building/DirectionalTreeAndForaging.cs` | `DirectionalTree`, `VegetationManager`, `ForagingNode`, `WildAnimalAI` |
| 12 | `Assets/Scripts/Building/ShoulderLogAndFreeformBuilder.cs` | `ShoulderLogAndFreeformBuilder`, `StructureIntegrity` |
| 13 | `Assets/Scripts/Building/GuidebookBlueprintsAndCooking.cs` | `GuidebookBlueprintsAndCooking`, `BlueprintGhost`, `CookingStation` |
| 14 | `Assets/Scripts/AI/KangDedenKelvinAI.cs` | `KangDedenKelvinAI` |
| 15 | `Assets/Scripts/AI/NingsihVirginiaAI.cs` | `NingsihVirginiaAI` |
| 16 | `Assets/Scripts/AI/NusantaraEnemyAndMutantAI.cs` | `NusantaraEnemyAndMutantAI`, `EnemyDirector` |
| 17 | `Assets/Editor/NusantaraMasterBuilder.cs` | `NusantaraMasterBuilder` |

File tambahan RESMI (boleh/wajib dibuat sesuai tahap): `Core/WorldTerrainGenerator.cs`, `Core/GameServices.cs`, `Core/GameEvents.cs`, `Core/GameBootstrap.cs`, `Core/PhysicsCompat.cs`, `Core/NusantaraDiagnostics.cs`, `Core/TimeOfDayClock.cs`, `AI/GridPathfinder.cs`, `AI/WaypointGraph.cs`, `AI/ProceduralAnimator.cs`, `UI/TouchControls.cs`, `UI/MenuScreens.cs`, `Editor/NusantaraSelfTest.cs`, `Editor/NusantaraAndroidBuild.cs`, `Editor/BuilderSteps.*.cs` (partial dari `NusantaraMasterBuilder`), serta file `partial` `NamaKelas.Bagian.cs`. Script shell: `Tools/cek_kompilasi.bat`, `Tools/cek_kompilasi.sh`.
Tambahan v3: `Core/TerrorDirector.cs`, `Core/WorldEventScheduler.cs`, `Core/GoreSystem.cs`, `AI/EcosystemDirector.cs`, `Editor/AKTexturePostprocessor.cs`, `Assets/Shaders/AK_Common.hlsl` + `Assets/Shaders/AK_*.shader` (daftar di `docs/gdd/15` §2.3).

---

## BAGIAN 5 — KONTRAK INTI (RINGKAS; kode lengkap di `docs/tech/11` §3)

- Namespace: `AlasKeramat.Core`, `.Player`, `.Inventory`, `.Building`, `.AI`, `.UI`, `.EditorTools`.
- `GameServices.Register<T>(obj)` / `GameServices.Get<T>()` — registri layanan; dilarang `Find*` di `Update`.
- `GameEvents` — event statis C# (`OnItemChanged`, `OnTreeFelled`, `OnEnemyKilled`, `OnQuestAdvanced`, `OnHourChanged`, `OnDayChanged`, `OnWeatherChanged`, `OnLoreFound`, `OnPlayerDamaged`, `OnStructureBuilt`, `OnMaghrib`, `OnMalamDarahStart/End`).
- Interface: `IDamageable { void TakeDamage(DamageInfo d); bool IsAlive { get; } }`, `IInteractable { string GetPromptText(GameObject actor); bool CanInteract(GameObject actor); void Interact(GameObject actor); float HoldDuration { get; } }`, `ISaveable { string SaveKey { get; } string CaptureJson(); void RestoreJson(string json); }`.
- ID item, resep, blueprint, musuh, misi, lore: NILAI INTEGER EKSPLISIT dan FINAL sesuai `docs/gdd/05`, `docs/gdd/17` (tambahan v3) & `docs/tech/11` (jangan diurutkan ulang — save file bergantung padanya).
- Satuan: 1 unit = 1 meter. Waktu game: 1 detik nyata = 1 menit game (1 hari = 24 menit nyata). Hari 1 jam 08:00 dengan skala waktu 0,65 sampai 18:00 (tutorial).
- Koordinat dunia final: `docs/gdd/03` dikali 2 (v5, `docs/gdd/20` §2). Permukaan X,Z ∈ [-1000, 1000]. Bawah tanah di Y = -80: UG1–UG6 sesuai `docs/gdd/20` §2.3. Masuk/keluar gua = fade hitam 1,2 detik + teleport.

---

## BAGIAN 6 — ALUR KERJA SETIAP SESI CODEX

1. Baca `docs/PROGRESS.md` → tentukan tahap aktif.
2. Baca bagian tahap itu di `docs/tech/12`, lalu SEMUA dokumen yang tercantum untuk tahap tersebut, plus `docs/tech/11`.
3. Tulis rencana singkat (daftar file + class + method publik baru) di awal jawabanmu.
4. Tulis kode per file. Setiap file lengkap & bisa dikompilasi. Pecah dengan `partial` bila > 900 baris.
5. Perbarui `NusantaraMasterBuilder` (dan `BuilderSteps.*.cs`) agar fitur baru ikut terbangun 1-klik, termasuk aset `Assets/Generated/`.
6. Perbarui `NusantaraSelfTest` dengan pemeriksaan otomatis fitur baru (lihat `docs/tech/12` per tahap).
7. Jalankan lingkaran verifikasi H12. Perbaiki sampai bersih.
8. Perbarui `docs/PROGRESS.md`. Tutup dengan pesan Bahasa Indonesia sederhana untuk pengguna: menu apa yang diklik + checklist uji tahap ini.

---

## BAGIAN 7 — DEFINITION OF DONE (SETIAP TAHAP)

- [ ] 0 `error CS` saat kompilasi; tidak ada error merah saat klik menu Builder dan saat Play 3 menit.
- [ ] Menu `Nusantara Builder` muncul; klik 2× berturut-turut tidak menggandakan objek/aset.
- [ ] Scene tersimpan otomatis di `Assets/Scenes/AlasKeramat_Main.unity`.
- [ ] Semua teks UI berbahasa Indonesia, terbaca jelas di resolusi 1920×1080 dan 2400×1080.
- [ ] Tidak ada material pink, tidak ada kotak kosong pengganti emoji.
- [ ] `NusantaraSelfTest` lulus (laporan di `Logs/nusantara_selftest.txt`).
- [ ] Checklist uji non-coder tahap tersebut di `docs/tech/12` bisa dijalankan dan lulus.
- [ ] `docs/PROGRESS.md` diperbarui.

---

## BAGIAN 8 — CARA TUGAS DIMULAI
Normalnya workflow **AK Autopilot** memposting perintah tahap ke issue `AUTOPILOT` (lihat `.github/workflows/ak-autopilot.yml`). Perintah manual setara:
> "@codex Kerjakan TAHAP T{n} proyek ini dari awal sampai lulus. Baca AGENTS.md, docs/PROGRESS.md, docs/UMPAN_BALIK.md, docs/tech/18, dan bagian T{n} di docs/tech/12 beserta semua dokumen yang diwajibkannya. Patuhi semua Hukum Besi. Perbarui Builder, SelfTest, NusantaraCI, PROGRESS (AUTOPILOT_TAHAP_BERIKUTNYA). Buka satu pull request."
Urutan: T0-C → T1 … T7 → G3 Irisan Vertikal → T8 … T11 → (T12 opsional) → tulis `AUTOPILOT_TAHAP_BERIKUTNYA: SELESAI`.

---

## BAGIAN 9 — CONTEKAN LARANGAN CEPAT

| Jangan | Pakai |
|---|---|
| `TMPro.TextMeshProUGUI` | `UnityEngine.UI.Text` + `GetSafeFont()` + `Outline` |
| `NavMeshAgent` | `CharacterController` + `GridPathfinder`/`WaypointGraph` |
| `Animator` | `ProceduralAnimator` (rotasi sendi via kode) |
| `Input.GetTouch` untuk gameplay | `TouchControls` berbasis uGUI pointer event |
| `StandaloneInputModule` tanpa cek | `AddCorrectInputModule()` (H3) |
| Emoji / ★ / ⌂ di teks | Sprite dari `IconPainter` |
| `Shader.Find` di sembarang tempat | `SafeMaterialFactory` + aset `.mat` di `Assets/Generated/` |
| `rb.velocity` langsung | `PhysicsCompat.GetVelocity/SetVelocity` |
| `FindObjectOfType` | `GameServices.Get<T>()` |
| Satu file 3000 baris | `partial class` per bagian |
| Material per bagian tubuh | 1 `SkinnedMeshRenderer` + atlas per karakter |
| GameObject per pohon jauh | `VegetationManager` + `Graphics.RenderMeshInstanced` |
| Adzan / teks kitab suci | Kentongan, gong jauh, angin, langit merah |
| Shader Graph / Decal Renderer Feature | Shader HLSL `AK/*` + decal quad (H15, 14 §3.2) |
| Jump scare ditulis langsung di skrip musuh | Kejadian terdaftar di `TerrorDirector` (H16) |
| Efek darah tanpa cek pengaturan | `GoreSystem` + `GoreLevel` (H16) |

# AKHIR AGENTS.md — detail lengkap ada di folder docs/
