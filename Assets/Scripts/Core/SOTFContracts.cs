// Kontrak bersama Alas Keramat. Nilai integer FINAL (docs/tech/11 bagian 3) - jangan diurutkan ulang.
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
    BubuIkan = 21, PetiPenyimpanan = 22, LampuMinyak = 23, JembatanGantung = 24, GerbangPagar = 25, KursiMeja = 26,
    PondokKayu = 27, PagarKayu = 28, KebunTanam = 29, MejaKerja = 30, KomporBatu = 31 }
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
