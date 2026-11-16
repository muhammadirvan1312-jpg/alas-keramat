# 11 — ARSITEKTUR TEKNIS: MANIFEST, KONTRAK, SCENE, ASET GENERATED, PERFORMA, SAVE, BUILDER, PATHFINDING
Dibaca di SETIAP tahap. Dokumen ini adalah sumber kebenaran untuk nama, ID integer, layer, tag, dan struktur folder.

---

## 1. Manifest File Lengkap
| Path | Tahap dibuat | Isi utama |
|---|---|---|
| `Assets/Scripts/Core/SOTFContracts.cs` | T1 | Semua enum (nilai integer eksplisit), struct data, interface (§3) |
| `Assets/Scripts/Core/GameServices.cs` | T1 | Registri layanan |
| `Assets/Scripts/Core/GameEvents.cs` | T1 | Event statis |
| `Assets/Scripts/Core/GameBootstrap.cs` | T1 | `[DefaultExecutionOrder(-1000)]`, registrasi, validasi scene, pemilihan preset kualitas, klon aset URP runtime |
| `Assets/Scripts/Core/PhysicsCompat.cs` | T1 | Wrapper Rigidbody 2022.3/Unity 6 |
| `Assets/Scripts/Core/NusantaraDiagnostics.cs` | T1 | Log runtime ke `Logs/nusantara_runtime.log` (Editor) + panel debug tersembunyi di HP (ketuk pojok kiri atas 5×) |
| `Assets/Scripts/Core/ProceduralAssetFactory.cs` (+ `.Textures.cs`, `.Meshes.cs`, `.Characters.cs`, `.Props.cs`) | T1, T2, T4 | `SafeMaterialFactory`, `ProcTex`, `ProcMesh`, `CharacterMeshBuilder`, `IconPainter`, cek `Resources/Override` |
| `Assets/Scripts/Core/ProceduralAudioSynth.cs` (+ `.Sfx.cs`, `.Ambience.cs`, `.Music.cs`) | T1, T2, T5, T7, T10 | `Dsp`, generator klip, pool suara, penjadwal musik |
| `Assets/Scripts/Core/TimeOfDayClock.cs` | T2 | Jam, hari, musim, event jam |
| `Assets/Scripts/Core/URPAtmosphereAndWeather.cs` | T2 | Matahari/bulan, ambient, fog, skybox, Volume, cuaca, partikel, petir, mode dalam ruangan |
| `Assets/Scripts/Core/WorldTerrainGenerator.cs` | T2 | `SampleHeight`, sungai, bioma, pad (03) |
| `Assets/Scripts/Core/WorldCaveAndSaveManager.cs` (+ `.Caves.cs`, `.Quests.cs`, `.Lore.cs`, `.Save.cs`, `.Cutscenes.cs`, `.Endings.cs`) | T2, T6, T10 | Zona, gua, teleport, `QuestManager`, `LoreLibrary`, `SaveSystem`, `CutsceneDirector`, ending |
| `Assets/Scripts/Player/AndiFPSController.cs` (+ `.Swim.cs`, `.Zipline.cs`, `.Camera.cs`) | T3 | Gerak, kamera, renang, zipline, interaksi raycast |
| `Assets/Scripts/Player/AndiVitalsAndSanity.cs` | T3 | Vitals 05 §3 |
| `Assets/Scripts/Player/CombatAndLeftHandController.cs` (+ `.Ranged.cs`) | T7 (dasar golok T3) | Senjata, parry, tangan kiri |
| `Assets/Scripts/Player/MobileTouchHUDAndRadar.cs` (+ `.Radar.cs`, `.Screens.cs`) | T3 | HUD, radar, layar lore, toast, subtitle |
| `Assets/Scripts/UI/TouchControls.cs` | T3 | Joystick, LookPad, ActionButton (ketuk/tahan/ketuk ganda) → `InputState` |
| `Assets/Scripts/UI/MenuScreens.cs` | T11 | Menu utama, jeda, pengaturan, peta, statistik |
| `Assets/Scripts/AI/ProceduralAnimator.cs` (+ `.Clips.cs`) | T4 | Lapisan animasi, data `PoseClip` |
| `Assets/Scripts/Inventory/TikarPandan3DInventory.cs` (+ `.Crafting.cs`) | T5 | Inventaris, panggung tikar, `CraftingBook`, `ItemDatabase` |
| `Assets/Scripts/Building/DirectionalTreeAndForaging.cs` (+ `.Vegetation.cs`, `.Animals.cs`) | T5 | Pohon, `VegetationManager`, foraging, hewan |
| `Assets/Scripts/Building/ShoulderLogAndFreeformBuilder.cs` (+ `.Sockets.cs`, `.Integrity.cs`) | T6 | Panggul, 5 mode, soket, integritas |
| `Assets/Scripts/Building/GuidebookBlueprintsAndCooking.cs` (+ `.Blueprints.cs`, `.Cooking.cs`) | T6 | Buku 3D, 14 blueprint, masak |
| `Assets/Scripts/AI/GridPathfinder.cs` | T7 | A* grid dipanggang |
| `Assets/Scripts/AI/WaypointGraph.cs` | T10 | Graf gua |
| `Assets/Scripts/AI/NusantaraEnemyAndMutantAI.cs` (+ `.Sekte.cs`, `.Ghosts.cs`, `.Boss.cs`, `.Director.cs`) | T7, T10 | AI musuh, `EnemyDirector`, agresi, raid, siege |
| `Assets/Scripts/AI/KangDedenKelvinAI.cs` (+ `.Orders.cs`) | T8 | 08 §2 |
| `Assets/Scripts/AI/NingsihVirginiaAI.cs` (+ `.Trust.cs`) | T8 | 08 §3 |
| `Assets/Editor/NusantaraMasterBuilder.cs` + `Assets/Editor/BuilderSteps.*.cs` | T1 → T11 | Menu & langkah build (§9) |
| `Assets/Editor/NusantaraSelfTest.cs` | T1 → T11 | Uji otomatis per tahap |
| `Assets/Editor/NusantaraAndroidBuild.cs` | T11 | Player Settings Android + Build APK |
| `Tools/cek_kompilasi.bat`, `Tools/cek_kompilasi.sh` | T1 | §14 |
| `Assets/Shaders/AK_Common.hlsl`, `AK_*.shader` | T2, T4, T7 | AK Shader Pack (15 §2) |
| `Assets/Scripts/Core/GoreSystem.cs` (+`.Blood.cs`, `.Dismember.cs`, `.Ragdoll.cs`, `.Corpses.cs`) | T7 | 14 |
| `Assets/Scripts/Core/TerrorDirector.cs` (+`.Events.cs`) | T9 | 13 |
| `Assets/Scripts/Core/WorldEventScheduler.cs` | T9 | 16 §5 |
| `Assets/Scripts/AI/EcosystemDirector.cs` (+`.Wildlife.cs`) | T9 | 16 §1 |
| `Assets/Editor/AKTexturePostprocessor.cs` | T0-C/T2 | Impor otomatis tekstur CC0 (19 §4) |
| `Assets/Editor/NusantaraCI.cs` | T0-C | `BuildAndTest()` untuk GameCI (18 §4) |
| `Assets/Editor/MocapImporter.cs` (+`.Retarget.cs`) | T4 | Parser ASF/AMC CMU → PoseClip (19 §5) |
| `Tools/ambil_aset.py`, `Tools/aset_cc0.lock.json`, `Tools/mocap.lock.json`, `Tools/cek_statis.py` | T0-C | 18 §5, 19 §4 |
| `.github/workflows/ak-ci.yml`, `ak-autopilot.yml` | disediakan, diuji T0-C | 18 |
Semua class runtime di namespace `AlasKeramat.*`; Editor di `AlasKeramat.EditorTools`.

## 2. Struktur Folder
```
Assets/
  Scripts/{Core,Player,UI,AI,Inventory,Building}/
  Editor/
  Scenes/AlasKeramat_Main.unity
  Shaders/              <- AK Shader Pack (H15)
  ThirdParty/CC0/, ThirdParty/Mocap/  <- diunduh Tools/ambil_aset.py, TIDAK dikomit (19)
  Generated/            <- HANYA ditulis Builder; boleh dihapus & dibuat ulang
    Textures/{World,Props,Characters,Vegetation,UI}/
    Materials/
    Meshes/{Terrain,Vegetation,Props,Characters,Underground}/
    Audio/{Sfx,Ambience,Music}/
    UI/Icons/
    Nav/SurfaceGrid.bytes
    Rendering/AK_URP_Mobile.asset, AK_VolumeProfile.asset, AK_Skybox.mat
  Resources/Override/   <- opsional milik pengguna (aset asli pengganti), tidak pernah dihapus Builder
Tools/  Logs/  docs/  AGENTS.md
```

## 3. Kontrak (`SOTFContracts.cs`) — nilai integer FINAL
```csharp
namespace AlasKeramat.Core {
public enum ItemCategory { Bahan = 0, Makanan = 1, Senjata = 2, Amunisi = 3, Alat = 4, Zirah = 5, Cerita = 6 }
public enum ItemID {
    None = 0,
    KayuGelondong = 1, Ranting = 2, Batu = 3, DaunPisang = 4, SeratIjuk = 5, TaliIjuk = 6, KainBatik = 7, Bambu = 8,
    PapanKayu = 9, Tulang = 10, KulitHewan = 11, Tengkorak = 12, Bulu = 13, Damar = 14, Kemenyan = 15, LogamBekas = 16,
    Baterai = 17, MinyakTanah = 18, Arang = 19, BotolKosong = 20, MesiuHitam = 21, KainKafan = 22,
    PotonganTubuh = 24, TopengSekte = 25, BatuBesar = 26,
    KelapaMuda = 50, Pisang = 51, Jamur = 52, JamurBeracun = 53, DaunSirih = 54, KunyitObat = 55, JaheHutan = 56,
    IkanMentah = 57, IkanMatang = 58, IkanKering = 59, DagingMentah = 60, DagingMatang = 61, DagingKering = 62,
    TelurAyamHutan = 63, RansumDarurat = 64, BotolAirBersih = 65, SupHutan = 66, JamuHerbal = 67, JamuEnergi = 68,
    MakananGosong = 69, DagingBusuk = 70, Kepiting = 71, PerbanKain = 72,
    GolokPusaka = 100, KapakBatu = 101, TombakBambu = 102, BusurBambu = 103, AnakPanah = 104, PanahApi = 105,
    Ketapel = 106, PistolSuar = 107, PelurSuar = 108, BedilLocok = 109, PeluruLocok = 110, KerisPusaka = 111,
    GolokApiPusaka = 112, Cangkul = 113, ZiplineGun = 114, TaliZipline = 115, BomBambuMinyak = 116, KapakBaja = 117,
    PakuBesiKuning = 118, PistolPT = 119, PeluruPistol = 120, SenapanLaras = 121, PeluruSenapan = 122, Mandau = 123,
    TombakLogam = 124, BusurTanduk = 125, PanahTulang = 126, PanahRacun = 127,
    OborBambu = 150, SenterTaktis = 151, GPSHandset = 152, BukuPanduan = 153, BukuCatatan = 154, KorekGas = 155,
    AlatSelamRebreather = 156, KartuAksesBunker = 157, PetaKertas = 158,
    ZirahDaun = 180, ZirahBambu = 181, ZirahTulang = 182, ZirahBaja = 183, ZirahEmas = 184,
    PakuKeramat1 = 190, PakuKeramat2 = 191, PakuKeramat3 = 192, PatungKayuDewi = 193, TandaPengenalRendra = 194
}
public enum BlueprintID { ApiUnggun = 1, SaungPanggung = 2, RakKayu = 3, KeretaDorongKayu = 4, PenampungHujan = 5,
    JemuranIkan = 6, PerangkapAyunBambu = 7, OborTengkorak = 8, TendaDarurat = 9, PanciMasak = 10,
    MenaraPengintai = 11, PintuBambu = 12, KentonganKemah = 13, RakitBambu = 14,
    MangkukKepiting = 15, PatungNgeri = 16, BatuAsah = 17, RumahPohon = 18, PerangkapPunji = 19, JeratHewan = 20,
    BubuIkan = 21, PetiPenyimpanan = 22, LampuMinyak = 23, JembatanGantung = 24, GerbangPagar = 25, KursiMeja = 26 }
public enum BuildPieceType { Tiang = 0, BalokDinding = 1, Atap = 2, Tangga = 3, Papan = 4, PagarRuncing = 5, BambuTanam = 6, BlueprintPart = 7, FondasiBatu = 8, DindingBatu = 9, JalanBatu = 10 }
public enum BuildSocketType { Tanah = 0, TiangAtas = 1, TiangBawah = 2, TiangSisi = 3, UjungBalok = 4, TengahBalok = 5, TepiPapan = 6, AnakTangga = 7 }
public enum CompanionOrder { Ikuti = 0, Tunggu = 1, Istirahat = 2, AmbilKayu = 3, AmbilRanting = 4, AmbilBatu = 5,
    IsiRakKayu = 6, SelesaikanBangunan = 7, JagaApi = 8, BersihkanKemah = 9, CariMakanan = 10, IsiAir = 11,
    PerbaikiBangunan = 12, PulangKeKemah = 13, BangunPagarKeliling = 14 }
public enum WeatherState { Cerah = 0, Berawan = 1, Gerimis = 2, HujanBadai = 3, KabutPagi = 4, KabutAsap = 5, KabutDarah = 6 }
public enum Season { Kemarau = 0, Penghujan = 1 }
public enum EnemyArchetype { AbdiSekte = 0, AbdiWanita = 1, AbdiLumpur = 2, AbdiButo = 3, PemimpinPatroli = 4, KiRogo = 5,
    PocongHutan = 6, PocongGua = 7, Kuntilanak = 8, Banaspati = 9, Genderuwo = 10, Jenglot = 11, ManusiaAkar = 12,
    Kuyang = 13, Tuyul = 14, GenderuwoHutan = 15, MutanGembung = 16, MutanJemari = 17, Penyerupa = 18, HantuAir = 19 }
public enum QuestID { M01 = 1, M02 = 2, M03 = 3, M04 = 4, M05 = 5, M06 = 6, M07 = 7, M08 = 8, M09 = 9, M10 = 10, M11 = 11, M12 = 12, M13 = 13,
    S01_KenanganDewi = 101, S02_MencariRendra = 102, S03_ArwahTersesat = 103, S04_Persembahan = 104 }
public enum LoreID { L01 = 1, L02 = 2, L03 = 3, L04 = 4, L05 = 5, L06 = 6, L07 = 7, L08 = 8, L09 = 9, L10 = 10, L11 = 11, L12 = 12 }
public enum Biome { Laut = 0, Pantai = 1, KawahBeringin = 2, TebingKapur = 3, SawitTerbakar = 4, Desa = 5, HutanBambu = 6, TepiSungai = 7, HutanJatiSelatan = 8, HutanCampuran = 9, BawahTanah = 10 }
public enum SurfaceType { Tanah = 0, DaunKering = 1, Pasir = 2, Air = 3, Kayu = 4, Beton = 5, BatuGua = 6 }
public enum DamageType { Tebas = 0, Tumpul = 1, Tusuk = 2, Proyektil = 3, Api = 4, Jatuh = 5, Tenggelam = 6, Gaib = 7, PohonTumbang = 8 }
public enum Difficulty { Damai = 0, Normal = 1, Sulit = 2 }
public enum TreeKind { Jati = 0, Mahoni = 1, Kelapa = 2, Aren = 3, SawitHangus = 4, BambuRumpun = 5, Pisang = 6, BeringinKecil = 7, BeringinAgung = 8 }
public enum UndergroundId { None = 0, UG1_GuaSungai = 1, UG2_Bunker = 2, UG3_KuilAkar = 3 }
public enum GoreLevel { Mati = 0, Sedang = 1, Penuh = 2 }
public enum HorrorIntensity { Ringan = 0, Normal = 1, Ekstrem = 2 }
public enum TerrorPhase { Tenang = 0, Meremang = 1, Mencekam = 2, Lega = 3 }
[System.Flags] public enum StatusEffect { None = 0, Pendarahan = 1, Racun = 2, KamuflaseLumpur = 4, SenjataDiasah = 8, Keracunan = 16 }

[System.Serializable] public struct ItemStack { public ItemID id; public int count; }
[System.Serializable] public struct DamageInfo { public float amount; public DamageType type; public UnityEngine.Vector3 point; public UnityEngine.Vector3 direction; public UnityEngine.GameObject source; public bool isHeavy; }
[System.Serializable] public struct CraftingRecipe { public int recipeId; public ItemStack[] inputs; public ItemStack output; public ItemID requiredTool; public bool advanced; }
[System.Serializable] public struct BlueprintDefinition { public BlueprintID id; public string displayName; public string category; public ItemStack[] cost; public UnityEngine.Vector2 footprint; public bool advanced; }
[System.Serializable] public struct ItemDefinition { public ItemID id; public string displayName; public string description; public ItemCategory category; public int maxCarry; public string iconName; public float fullness, hydration, health, stamina, sanity; public float spoilDays; public ItemID spoilsInto; }

public interface IDamageable { void TakeDamage(DamageInfo info); bool IsAlive { get; } }
public interface IInteractable { string GetPromptText(UnityEngine.GameObject actor); bool CanInteract(UnityEngine.GameObject actor); void Interact(UnityEngine.GameObject actor); float HoldDuration { get; } }
public interface ISaveable { string SaveKey { get; } string CaptureJson(); void RestoreJson(string json); }

public static class GameLayers { public const int Water = 4, UI = 5, Ground = 6, Player = 7, Companion = 8, Enemy = 9, Interactable = 10, Tree = 11, LogPhysics = 12, Structure = 13, BuildGhost = 14, InventoryStage = 15, Projectile = 16, Foliage = 17, TriggerZone = 18; }
public static class GameTags { public const string Player = "Player", Companion = "Companion", Enemy = "Enemy", TreeNode = "TreeNode", LogItem = "LogItem", StructurePiece = "StructurePiece", Interactable = "Interactable", CaveEntrance = "CaveEntrance", LoreDocument = "LoreDocument", Altar = "Altar", SaungBed = "SaungBed", Campfire = "Campfire", WaterSource = "WaterSource", Effigy = "Effigy", GeneratedRoot = "GeneratedRoot", ZiplineAnchor = "ZiplineAnchor"; }
}
```
`HoldDuration` = 0 untuk ketuk; > 0 untuk aksi tahan (gali 1,2 s, bongkar 1,0 s, dobrak 0,6 s).

## 4. Layanan Inti
- `GameServices`: `Register<T>(T s)`, `Get<T>()` (bila belum terdaftar dan T adalah `MonoBehaviour` → `FindFirstObjectByType<T>()` sekali, lalu cache), `Clear()` di `SubsystemRegistration`.
- `GameEvents` (semua `static event System.Action<...>`; method `Raise...` aman null): `OnItemChanged(ItemID,int)`, `OnTreeFelled(int treeId, TreeKind)`, `OnEnemyKilled(EnemyArchetype, bool spared)`, `OnQuestAdvanced(QuestID)`, `OnHourChanged(int)`, `OnDayChanged(int)`, `OnMaghrib()`, `OnMalamDarahStart()`, `OnMalamDarahEnd()`, `OnWeatherChanged(WeatherState)`, `OnLoreFound(LoreID)`, `OnPlayerDamaged(DamageInfo)`, `OnStructureBuilt(BlueprintID)`, `OnEnterUnderground(UndergroundId)`, `OnExitUnderground()`, `OnSubtitle(string)`, `OnToast(string)`, `OnTerrorPhaseChanged(TerrorPhase)`, `OnTerrorEvent(int eventId)`, `OnWorldEvent(int eventId)`, `OnLimbSevered(EnemyArchetype, int boneIndex)`, `OnStatusChanged(StatusEffect)`.
- `GameBootstrap` (`Awake` urutan −1000): pastikan `Canvas_HUD`, `EventSystem`, `Andi`, manajer ada; daftarkan ke `GameServices`; terapkan preset grafis dari `PlayerPrefs`; klon aset URP (§7.2); `Application.targetFrameRate`; `QualitySettings.vSyncCount = 0`; mulai cutscene intro bila game baru.

## 5. Scene, Layer, Tag
### 5.1 Hierarki (dibuat Builder)
```
[ALAS_KERAMAT_ROOT]  (tag GeneratedRoot)
 ├─ _Systems   GameBootstrap, TimeOfDayClock, URPAtmosphereAndWeather, ProceduralAudioSynth, WorldCaveAndSaveManager,
 │             EnemyDirector, VegetationManager, GridPathfinder, NusantaraDiagnostics
 ├─ _Lighting  Sun (Directional), Moon (Directional), GlobalVolume, FxVolume
 ├─ World      Terrain/Chunk_x_z (400), Water/{Laut, SungaiUtama, AnakSungai, Rawa, AirTerjun}, Zones/Zona1..7, Props
 ├─ Underground UG1_GuaSungai, UG2_Bunker, UG3_KuilAkar
 ├─ Characters  Andi (CharacterController + kamera), KangDeden, Ningsih
 ├─ Enemies     (pool kosong, diisi runtime)
 ├─ Interactables Koper×3, Lore×12, Pickups, CaveEntrance×3, Altar, ZiplineAnchor×4
 ├─ InventoryStage (0,500,0)
 └─ UI          Canvas_HUD, Canvas_Menus, EventSystem
```
### 5.2 Layer (didaftarkan via `SerializedObject` pada `TagManager.asset`, slot user 6–18; slot 4 "Water" & 5 "UI" bawaan)
Ground 6, Player 7, Companion 8, Enemy 9, Interactable 10, Tree 11, LogPhysics 12, Structure 13, BuildGhost 14, InventoryStage 15, Projectile 16, Foliage 17, TriggerZone 18.
Matriks tabrakan (`Physics.IgnoreLayerCollision` di Builder & Bootstrap): BuildGhost abaikan semua; InventoryStage abaikan semua; Foliage hanya trigger dengan Player/Enemy/Companion; Projectile abaikan Projectile & Foliage; TriggerZone hanya trigger.
### 5.3 Tag
Bawaan: `Player`, `MainCamera`. Tambahan: semua konstanta `GameTags` di §3.

## 6. Pipeline Aset Generated
1. `AssetDatabase.StartAssetEditing()` dalam `try`, `StopAssetEditing()` + `Refresh()` dalam `finally`.
2. Tekstur → `EncodeToPNG` → `File.WriteAllBytes` → `ImportAsset` → atur `TextureImporter`: normal map = `TextureImporterType.NormalMap`; sprite UI = `Sprite`; maxSize sesuai (World 2048, props 512, karakter 512, ikon 128); mipmap ON kecuali UI; override Android `ASTC_6x6` (UI `ASTC_4x4`).
3. Mesh → `AssetDatabase.CreateAsset(mesh, path)`; `mesh.RecalculateBounds()`; > 65.000 vertex → `IndexFormat.UInt32` (hindari; pecah mesh).
4. Material → `CreateAsset` (`.mat`) setelah tekstur diimpor ulang (referensi tekstur dari aset, bukan objek memori).
5. **VolumeProfile**: `ScriptableObject.CreateInstance<VolumeProfile>()` → `CreateAsset` → tiap `profile.Add<T>(true)` lalu **`AssetDatabase.AddObjectToAsset(component, profile)`** (tanpa ini komponen hilang saat proyek dibuka ulang).
6. Audio → WAV 16-bit + pengaturan impor (10 §1).
7. Idempoten: Builder menghapus isi `Assets/Generated/` (bukan folder lain) sebelum membuat ulang, KECUALI pengguna memilih "Bangun Ulang Cepat (pakai aset yang ada)".
### 6.4 Override Aset Asli (opsional)
`ProceduralAssetFactory.TryOverride<T>(string kategori, string nama)` → `Resources.Load<T>("Override/" + kategori + "/" + nama)`. Kategori: `Meshes`, `Textures`, `Audio`, `Prefabs`. Jika ada, pakai; jika tidak, prosedural. Dicatat di laporan build.

## 7. Performa
### 7.1 Anggaran (HP menengah, preset Sedang)
| Item | Batas |
|---|---|
| FPS | 30 stabil (Tinggi: 60 di HP kuat) |
| Batch/SetPass | ≤ 150 |
| Segitiga terlihat | ≤ 250 ribu |
| Musuh aktif | ≤ 12 (logika > 60 m: 4 Hz; animasi mati > 80 m) |
| Hewan aktif | ≤ 10 |
| Rigidbody kayu aktif | ≤ 40 |
| Light realtime per area | 1 directional (bayangan) + ≤ 4 titik dekat (tanpa bayangan) |
| Suara | ≤ 24 |
| Memori total | ≤ 1,2 GB; tekstur ≤ 250 MB; audio ≤ 40 MB |
| GC per frame | 0 B di gameplay normal |
### 7.2 Preset Grafis (diterapkan ke KLON aset URP saat runtime agar aset proyek tidak berubah)
| Setting | Rendah | Sedang | Tinggi |
|---|---|---|---|
| Render scale | 0,7 | 0,85 | 1,0 |
| Shadow distance | 0 (mati) | 35 | 60 |
| Cascade | — | 1 | 2 |
| MSAA | off | off | 2× |
| HDR | off | on | on |
| Bloom | off | on | on |
| Far clip kamera | 140 | 200 | 260 |
| Fog tambahan | +0,006 | +0,002 | 0 |
| Jarak vegetasi instanced | 90 | 140 | 200 |
| Jarak semak | 25 | 40 | 60 |
| Target FPS | 30 | 30 | 60 |
- Builder menyalin aset URP template ke `Assets/Generated/Rendering/AK_URP_Mobile.asset` (via `AssetDatabase.CopyAsset`), mengatur nilai dasar (SRP Batcher ON, depth/opaque texture OFF, soft shadow OFF, additional lights per-pixel maks 4 tanpa bayangan), lalu mengisinya ke setiap level Quality via `QualitySettings.renderPipeline` (loop `SetQualityLevel`). Properti tanpa setter publik → `SerializedObject` (mis. `m_MainLightShadowmapResolution` = 1024); bila nama properti tidak ditemukan, log peringatan & lanjut.
- Runtime: `GameBootstrap` membuat `Instantiate(aset)` lalu `QualitySettings.renderPipeline = klon` sebelum menerapkan preset.
Batas tambahan v3: 14 §10 (gore) dan 17 §9 (satwa, kejadian, perangkap).
### 7.3 Teknik
- Terrain chunk LOD0/LOD1 + `LODGroup`; collider hanya LOD0.
- **VegetationManager**: data per chunk (posisi, rotasi, skala, jenis, hidup). Dalam 45 m: GameObject pohon interaktif dari pool (collider + `DirectionalTree`); 45 m–jarak preset: `Graphics.RenderMeshInstanced` per jenis/LOD (array `Matrix4x4` dipakai ulang, maks 1023 per panggilan, tanpa bayangan); semak & pakis instanced ≤ jarak semak.
- Karakter 1 `SkinnedMeshRenderer` + atlas; GPU skinning ON (Player Settings).
- SRP Batcher ON; material berbagi shader; tekstur atlas untuk props kecil.
- Fog eksponensial selalu ON (menyembunyikan batas far clip).
- `Physics` fixed timestep 0,025; `Rigidbody.sleepThreshold` default; kayu diam > 3 s → `Sleep()`.
- UI: Canvas HUD dibagi 3 (statis, bar dinamis, radar) agar rebuild murah; `raycastTarget = false` untuk elemen non-interaktif.

## 8. Simpan/Muat (`SaveSystem`)
- File: `Application.persistentDataPath/alas_keramat_slot{1-3}.json`; tulis ke `.tmp` lalu `File.Replace`/`Move` (atomik); cadangan `.bak` versi sebelumnya.
- `JsonUtility` (tanpa Dictionary → pakai List). `version = 2`; fungsi migrasi bila versi lebih lama.
```csharp
[System.Serializable] public class WorldSaveData {
  public int version = 2; public string savedAtIso; public int difficulty; public bool sandboxMode; public int endingChosen;
  public int day; public float minuteOfDay; public int season; public int weather; public float aggression; public int captureCount;
  public UnityEngine.Vector3 playerPos; public float playerYaw; public int underground; public UnityEngine.Vector3 surfaceReturnPos;
  public float health, healthCeiling, stamina, fullness, hydration, rest, sanity, wetness, strengthXp;
  public System.Collections.Generic.List<ItemStack> inventory; public int[] quickSlots; public int leftHandItem;
  public int[] armorTypes; public float[] armorDurability;
  public System.Collections.Generic.List<int> felledTreeIds; public System.Collections.Generic.List<float> felledTreeDay;
  public System.Collections.Generic.List<string> structuresJson;   // per potongan: tipe, pos, rot, hp, koneksi
  public System.Collections.Generic.List<string> blueprintsJson;   // id, pos, rot, bahan terisi
  public System.Collections.Generic.List<string> looseLogsJson;    // bentuk, pos, rot
  public System.Collections.Generic.List<string> ziplinesJson;     // a, b
  public string dedenJson, ningsihJson;
  public System.Collections.Generic.List<int> questsDone, questsActive, loreFound;
  public System.Collections.Generic.List<string> flags;            // mis. "KOPER_1", "GALI_BUNKER=3", "PAKU_TERTANCAP_2"
  public System.Collections.Generic.List<string> pickedWorldItems; // ID pickup dunia yang sudah diambil
  public System.Collections.Generic.List<string> forageCooldowns;  // "nodeId:hariSiap"
  public string mapRevealBase64; public string statsJson;
  // v3
  public int goreLevel; public int statusFlags; public string trailMapBase64;
  public System.Collections.Generic.List<string> corpsesJson;       // archetype, pos, rot, bone yang putus, hari mati
  public System.Collections.Generic.List<int> terrorEventDays;      // indeks = eventId, nilai = hari terakhir terjadi
  public System.Collections.Generic.List<int> worldEventDays;
  public System.Collections.Generic.List<int> dedenNotesRead;
  public System.Collections.Generic.List<string> wildlifeStock;     // "gridId:stok"
  public int arwahDilepas, hantuDiikat, persembahanDiterima;
}
```
- Pemicu simpan: tidur, Tenda "SIMPAN", masuk/keluar gua, sebelum boss, sebelum ritual, keluar ke menu (opsional). Tidak bisa simpan saat musuh ≤ 30 m atau siege.
- Muat: bangun ulang dunia statis (sudah di scene) → terapkan pohon rebah → spawn struktur/kayu/tali → companion → vitals → posisi.

## 9. Builder (`NusantaraMasterBuilder` + `BuilderSteps.*.cs`)
### 9.1 Menu (tanpa emoji)
- `Nusantara Builder/1. BANGUN SELURUH GAME (1-KLIK)`
- `Nusantara Builder/2. Bangun Ulang Cepat (pakai aset yang ada)`
- `Nusantara Builder/Tahap/Bangun sampai T1 … T11` (kumulatif)
- `Nusantara Builder/Utilitas/Jalankan Self-Test`, `Buka Scene Utama`, `Tampilkan Laporan Build`, `Hapus Semua Hasil Generate`, `Set Input ke Both (opsional, perlu restart Unity)`
- `Nusantara Builder/Android/Atur Player Settings Android`, `Build APK Uji`
- `Nusantara Builder/Debug (saat Play)/Teleport ke Zona 1–7, UG1–3`, `Lompat +3 Jam`, `Set 17:55 (Maghrib)`, `Mulai Malam Darah`, `Beri Semua Item`, `Trust Ningsih 70`, `Agresi Sekte 80`, `Ambil 3 Paku`, `Isi Semua Vitals`, `Paksa Kejadian Teror E01–E37`, `Paksa Kejadian Dunia W01–W13`, `Set Gore Penuh`, `Foto Kualitas` (15 §8), `Galeri Karakter`
### 9.2 Urutan Langkah (bobot progress bar)
1. Proyek (5%): Tag/Layer, matriks tabrakan, Player Settings dasar (landscape, GPU skinning, warna ruang Linear), aset URP + Quality, Always Included Shaders.
2. Aset (25%): tekstur, ikon, material, mesh props/vegetasi/karakter, audio WAV, VolumeProfile, skybox.
3. Dunia (25%): terrain chunk, air, jalan, pad, zona & props, vegetasi (data `VegetationManager`), hewan (titik spawn).
4. Bawah tanah (10%): UG1–3 + pintu masuk/keluar + waypoint.
5. Karakter (10%): Andi (+ kamera, 3 renderer), Deden, Ningsih, prototipe musuh untuk pool.
6. Sistem (10%): semua manajer, pickup, lore, koper, altar, jangkar zipline.
7. UI (10%): Canvas, HUD, layar, EventSystem dengan `AddCorrectInputModule`.
8. Nav (5%): panggang `SurfaceGrid.bytes`.
9. Simpan scene + daftar Build Settings + laporan `Logs/nusantara_build_report.txt` (jumlah objek, segitiga, aset, durasi, peringatan) + dialog ringkasan.
Setiap langkah dibungkus try/catch: gagal di satu langkah → lanjut langkah lain, tandai GAGAL di laporan.

## 10. Pathfinding Tanpa NavMesh
- **SurfaceGrid**: sel 2 m di [−500,500]² = 500×500 byte. Bit: 0 = dapat dilalui, 1 = air dangkal (biaya ×3), 2 = air dalam (tidak), 3 = lereng 30–38° (biaya ×2), 4 = terhalang statis (props, batu besar: `Physics.CheckBox` saat panggang), 5 = semak (biaya ×1,2), 6 = terhalang dinamis (struktur pemain; diperbarui saat bangun/bongkar). Lereng > 38° = tidak bisa.
- A* dengan binary heap, heuristik oktil, maks 4.000 node per permintaan, maks 2 permintaan per frame (antrian), penghalusan jalur via garis pandang grid, cache jalur 2 s.
- Pengikut jalur: steering ke titik berikut + `SphereCast` 0,4 m menghindari halangan dinamis; **deteksi macet**: bergerak < 0,3 m dalam 2 s → lompat kecil / langkah samping / minta jalur baru; macet 3× → teleport kecil 2 m ke sel valid terdekat bila tidak terlihat kamera.
- **WaypointGraph** untuk UG1–3: simpul & sisi didefinisikan di kode (posisi lokal dari 03 §10), A* di graf.
- Ground snap: `Raycast` ke bawah (layer Ground|Structure) dari +1,5 m; di permukaan, fallback `SampleHeight` bila raycast gagal.

## 11. Arsitektur Input
- `TouchControls` menulis ke `InputState` (struct statis per frame): `Move (Vector2)`, `SprintHeld`, `LookDelta`, `AttackDown/Held/Up`, `ParryTap/Held/DoubleTap`, `InteractDown/Held/Up`, `ThrowTap`, `JumpTap`, `CrouchToggle`, `QuickSlot (1–4)`, `LeftHandCycle`, `OpenTikar`, `OpenBuku`, `OpenCatatan`, `Pause`, `BuildModeIndex`.
- Editor (`#if ENABLE_LEGACY_INPUT_MANAGER`): WASD, mouse kanan ditahan = lihat, Shift sprint, Space lompat, C jongkok, LMB serang, Q parry (ketuk ganda Q = holster), E interaksi, G lempar, Tab tikar, B buku, N catatan, 1–4 slot, F tangan kiri, M peta, Esc jeda.

## 12. Diagnostik & Self-Test
- `NusantaraDiagnostics`: `Application.logMessageReceived` → tulis Warning/Error/Exception ke `Logs/nusantara_runtime.log` (hanya Editor; di build: simpan 50 entri terakhir untuk panel debug).
- `NusantaraSelfTest.RunAll()` (menu & `-executeMethod`): menulis `Logs/nusantara_selftest.txt` berisi `[LULUS]`/`[GAGAL]` per pemeriksaan; `RunFromCommandLine()` keluar dengan kode 0/1. Pemeriksaan per tahap tercantum di `12_TAHAPAN_DAN_PENGUJIAN.md`.

## 13. Kompatibilitas Versi (rinci)
| Topik | Aturan |
|---|---|
| Bahasa C# | ≤ C# 9 |
| Cari objek | `Object.FindFirstObjectByType<T>()`, `FindObjectsByType<T>(FindObjectsSortMode.None)` |
| Rigidbody | Hanya via `PhysicsCompat` |
| Pipeline | Baca `GraphicsSettings.currentRenderPipeline`; tulis per-quality `QualitySettings.renderPipeline` |
| Font | `LegacyRuntime.ttf` |
| Render API | Hindari `ScriptableRendererFeature`, `RenderGraph` kustom, `CommandBuffer` kustom |
| Mesh API | `SetVertices/SetTriangles/SetUVs/SetNormals` + `boneWeights` (API lama yang ada di keduanya) |
| Editor | `EditorSceneManager`, `AssetDatabase`, `TextureImporter`, `PlayerSettings` dengan `NamedBuildTarget` |

## 14. Skrip Cek Kompilasi
`Tools/cek_kompilasi.bat`:
```bat
@echo off
setlocal
if "%UNITY_EXE%"=="" (echo [AK] Variabel UNITY_EXE belum diatur. & exit /b 2)
set PROJ=%~dp0..
if not exist "%PROJ%\Logs" mkdir "%PROJ%\Logs"
"%UNITY_EXE%" -batchmode -nographics -quit -projectPath "%PROJ%" -logFile "%PROJ%\Logs\cek_kompilasi.log" -executeMethod AlasKeramat.EditorTools.NusantaraSelfTest.RunFromCommandLine
set CODE=%ERRORLEVEL%
findstr /C:"error CS" "%PROJ%\Logs\cek_kompilasi.log"
if %ERRORLEVEL%==0 (echo [AK] ADA ERROR KOMPILASI & exit /b 1)
if not %CODE%==0 (echo [AK] Self-test gagal, lihat Logs\nusantara_selftest.txt & exit /b %CODE%)
echo [AK] BERSIH & exit /b 0
```
`Tools/cek_kompilasi.sh`: padanan bash (`"$UNITY_EXE" -batchmode ...`, `grep -n "error CS"`).
Catatan: Unity Editor harus TERTUTUP (proyek terkunci bila terbuka). Contoh `UNITY_EXE` Windows: `C:\Program Files\Unity\Hub\Editor\<versi>\Editor\Unity.exe`; macOS: `/Applications/Unity/Hub/Editor/<versi>/Unity.app/Contents/MacOS/Unity`.

## 15. Build Android (`NusantaraAndroidBuild`)
- Product "Alas Keramat", package `com.alaskeramat.game` (bisa diubah pengguna), landscape kiri/kanan, IL2CPP, ARM64 saja, Min API 26, Target API = tertinggi terpasang, Graphics API otomatis (Vulkan + GLES3), warna Linear, GPU skinning, stripping Low.
- `Build APK Uji`: cek `BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android)`; jika belum: dialog "Modul Android belum terpasang. Buka Unity Hub → Installs → (versi Unity) → Add modules → Android Build Support (centang OpenJDK & SDK/NDK)." Jika ada: build ke `Builds/AlasKeramat_uji.apk` dengan keystore debug, lalu buka folder.
- Reflection modul UI Input System hanya dijalankan di Editor (Builder); komponen tersimpan di scene sehingga tidak terhapus stripping.
