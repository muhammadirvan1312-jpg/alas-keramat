using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;

namespace AlasKeramat.EditorTools
{
    // Langkah 3d (T2c): landmark zona (03 §9 dikali 2, 20 §2.2) — Beringin Agung, heli, desa, kampung, mercusuar, pabrik gula, gua, altar.
    public static partial class NusantaraMasterBuilder
    {
        public const string FolderLandmark = FolderGen + "/Landmark";
        public static int JumlahLandmark;

        static GameObject _induk;

        static void LangkahLandmark()
        {
            EnsureFolder(FolderLandmark);
            var dunia = GameObject.Find(NamaRoot + "/World");
            _induk = Anak(dunia != null ? dunia : _root, "Landmark");
            JumlahLandmark = 0;
            Material kayu = AmbilMat("M_Kayu"), seng = AmbilMat("M_LogamKarat"), batu = AmbilMat("M_Batu");
            var bata = SimpanMat(SafeMaterialFactory.CreatePBR("M_Bata", ProcTex.Hex("#B0583E"), 0f, 0.15f, Tex("beton"), Tex("beton_n")));
            var putih = SimpanMat(SafeMaterialFactory.CreatePBR("M_CatPutih", ProcTex.Hex("#E2DFD6"), 0.1f, 0.45f, Tex("beton")));
            var oranye = SimpanMat(SafeMaterialFactory.CreatePBR("M_CatOranye", ProcTex.Hex("#D9661C"), 0.1f, 0.45f));
            var merah = SimpanMat(SafeMaterialFactory.CreatePBR("M_CatMerahPudar", ProcTex.Hex("#8E3A30"), 0f, 0.3f, Tex("beton")));
            var gelap = SimpanMat(SafeMaterialFactory.CreatePBR("M_LogamGelap", ProcTex.Hex("#3A3B3D"), 0.7f, 0.35f));
            var hitam = SimpanMat(SafeMaterialFactory.CreatePBR("M_GelapGua", Color.black, 0f, 0f));
            var ijuk = SimpanMat(SafeMaterialFactory.CreatePBR("M_AtapIjuk", ProcTex.Hex("#3A2E22"), 0f, 0.1f, Tex("kulit_kayu")));

            // Zona 5: Beringin Agung (28 m) + altar.
            var beringin = ProcPohon.Buat(ProcPohon.Jenis.BeringinKecil, 7, false);
            Taruh("BeringinAgung", Simpan(beringin), new[] { AmbilMat("M_KulitPohon"), AmbilMat("M_DaunPohon") }, 0f, 0f, 0f, new Vector3(2.6f, 2.2f, 2.6f), false);
            Taruh("Altar", Simpan(ProcBangunan.Altar()), new[] { batu }, 0f, -18f, 180f, Vector3.one, true);

            // Zona 1: heli rusak (yaw 35, roll 18).
            var heli = Taruh("HeliRusak", Simpan(ProcBangunan.HeliRusak()), new[] { putih, oranye, gelap }, 8f, -448f, 35f, Vector3.one, true);
            heli.transform.position += Vector3.up * 1.0f;
            heli.transform.rotation = Quaternion.Euler(0f, 35f, 18f);

            // Zona 3: Desa Mati (6 rumah, dikali 2 dari 03).
            float[,] desa = { { 564, 224, 200, 0.3f }, { 524, 236, 180, 0.4f }, { 592, 176, 260, 0.3f }, { 544, 140, 330, 0.4f }, { 504, 200, 120, 0.85f }, { 580, 260, 210, 0.9f } };
            for (int i = 0; i < desa.GetLength(0); i++)
                Taruh("RumahDesa_" + (i + 1), Simpan(ProcBangunan.RumahPanggung(100 + i, i == 0 ? 8f : 7f, i == 0 ? 6f : 5.5f, 1.5f, desa[i, 3])), new[] { kayu, i % 2 == 0 ? seng : ijuk }, desa[i, 0], desa[i, 1], desa[i, 2], Vector3.one, true);

            // Zona 11: Kampung Sukamaju (22 rumah seragam, ditinggal mendadak).
            for (int i = 0; i < 22; i++)
            {
                int baris = i / 6, kolom = i % 6;
                float x = 560f + kolom * 26f + (baris % 2) * 8f, z = 560f + baris * 30f;
                Taruh("RumahKampung_" + (i + 1), Simpan(ProcBangunan.RumahPanggung(300 + i, 6.5f, 5f, 0.6f, 0.15f)), new[] { kayu, seng }, x, z, baris % 2 == 0 ? 180f : 0f, Vector3.one, true);
            }

            // Zona 7: Mercusuar.
            Taruh("Mercusuar", Simpan(ProcBangunan.Mercusuar()), new[] { putih, merah, gelap }, -190f, -876f, 0f, Vector3.one, true);

            // Zona 9: Pabrik Gula Sinar Tebu.
            Taruh("Cerobong", Simpan(ProcBangunan.GedungPabrik(9, 0, 0, 0, true)), new[] { bata, seng, kayu }, 700f, -540f, 0f, Vector3.one, true);
            Taruh("GedungGiling", Simpan(ProcBangunan.GedungPabrik(10, 30f, 16f, 7f, false)), new[] { bata, seng, kayu }, 640f, -520f, 10f, Vector3.one, true);
            Taruh("GudangGula", Simpan(ProcBangunan.GedungPabrik(11, 20f, 12f, 6f, false)), new[] { bata, seng, kayu }, 690f, -470f, 100f, Vector3.one, true);
            Taruh("KantorAdministratur", Simpan(ProcBangunan.GedungPabrik(12, 14f, 10f, 5f, false)), new[] { putih, seng, kayu }, 620f, -455f, 190f, Vector3.one, true);

            // Mulut gua: UG1 lembah bambu (menghadap timur ke sungai), UG3 celah kuil.
            Taruh("CaveEntrance_1", Simpan(ProcBangunan.MulutGua(1, 9f)), new[] { batu, hitam }, 256f, -276f, 90f, Vector3.one, true);
            Taruh("CaveEntrance_3", Simpan(ProcBangunan.MulutGua(3, 12f)), new[] { batu, hitam }, 0f, 694f, 180f, Vector3.one, true);
            Catat("Landmark: " + JumlahLandmark + " objek (Beringin Agung, heli, 6 rumah desa, 22 rumah kampung, mercusuar, pabrik gula, 2 mulut gua, altar)");
        }

        static Texture2D Tex(string nama) { return AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/" + nama + ".png"); }

        static Material AmbilMat(string nama)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/" + nama + ".mat");
            if (m == null) Debug.LogError("[AK][Builder] material tidak ditemukan: " + nama);
            return m;
        }

        static Mesh Simpan(Mesh m)
        {
            string p = FolderLandmark + "/" + m.name + ".asset";
            AssetDatabase.DeleteAsset(p);
            AssetDatabase.CreateAsset(m, p);
            return AssetDatabase.LoadAssetAtPath<Mesh>(p);
        }

        static GameObject Taruh(string nama, Mesh mesh, Material[] mats, float x, float z, float yaw, Vector3 skala, bool collider)
        {
            var go = Anak(_induk, nama);
            go.layer = GameLayers.Structure;
            go.tag = GameTags.StructurePiece;
            float y = WorldTerrainGenerator.SampleHeight(x, z);
            go.transform.position = new Vector3(x, y, z);
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            go.transform.localScale = skala;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterials = mats;
            if (collider) go.AddComponent<MeshCollider>().sharedMesh = mesh;
            JumlahLandmark++;
            return go;
        }
    }
}
