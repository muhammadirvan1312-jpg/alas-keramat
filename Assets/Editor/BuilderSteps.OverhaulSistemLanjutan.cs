using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.EditorTools
{
    // Langkah 7z13: Sistem Lanjutan SOTF - Kombo 3-serangan & Level Survival di Andi, 4 Stasiun Radio Darurat,
    // 3 Pohon Keramat & Batu Sesajen, dan Audio Ambien Dinamis Mega-Landmark.
    public static partial class NusantaraMasterBuilder
    {
        public static int JumlahRadioDarurat, JumlahPohonKeramat;

        static void LangkahSistemLanjutan()
        {
            JumlahRadioDarurat = 0; JumlahPohonKeramat = 0;
            var lm = GameObject.Find(NamaRoot + "/World/Landmark") ?? GameObject.Find(NamaRoot + "/Landmark") ?? _root;
            var induk = Anak(lm, "SistemLanjutanSOTF");

            var andi = Object.FindFirstObjectByType<AndiFPSController>(FindObjectsInactive.Include);
            if (andi != null)
            {
                if (andi.GetComponent<SistemKomboDanStagger>() == null) andi.gameObject.AddComponent<SistemKomboDanStagger>();
                if (andi.GetComponent<LevelSurvivalAndi>() == null) andi.gameObject.AddComponent<LevelSurvivalAndi>();
                EditorUtility.SetDirty(andi.gameObject);
            }

            // 4 Stasiun Radio Darurat (Heli, Desa Mati, Mercusuar, Pabrik Gula)
            var logam = AmbilMat("M_LogamGelap");
            var matIndikator = SimpanMat(SafeMaterialFactory.CreatePBR("M_LampuRadio", new Color(0.25f, 1f, 0.45f), 0f, 0.5f, null, null, new Color(0.6f, 3.2f, 1.1f)));
            Vector2[] posRadio = { new Vector2(10f, -444f), new Vector2(546f, 202f), new Vector2(-176f, -862f), new Vector2(660f, -494f) };
            for (int i = 0; i < posRadio.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(posRadio[i].x, posRadio[i].y);
                var go = Anak(induk, "RadioDarurat_" + i);
                go.transform.position = new Vector3(posRadio[i].x, y, posRadio[i].y);
                go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
                var bc = go.AddComponent<BoxCollider>(); bc.size = new Vector3(0.75f, 0.8f, 0.75f); bc.center = new Vector3(0f, 0.4f, 0f);
                Hias(go, PrimitiveType.Cube, "KotakRadio", new Vector3(0f, 0.25f, 0f), new Vector3(0.55f, 0.36f, 0.32f), logam, false);
                Hias(go, PrimitiveType.Cylinder, "Antena", new Vector3(0.22f, 0.78f, 0f), new Vector3(0.02f, 0.45f, 0.02f), logam, false);
                var ind = Hias(go, PrimitiveType.Sphere, "Indikator", new Vector3(-0.14f, 0.34f, 0.17f), Vector3.one * 0.07f, matIndikator, false);
                var imr = ind.GetComponent<MeshRenderer>(); imr.shadowCastingMode = ShadowCastingMode.Off; imr.enabled = false;
                var rd = go.AddComponent<RadioDaruratPulau>(); rd.Pasang(i, imr); EditorUtility.SetDirty(rd);
                JumlahRadioDarurat++;
            }

            // 3 Pohon Keramat & Batu Sesajen (Kain putih/poleng + batu altar kecil)
            var kulit = AmbilMat("M_KulitPohon") ?? AmbilMat("M_Kayu");
            var kainPutih = MatPolos("M_KainKeramat", "#E6E2D6");
            var batu = AmbilMat("M_Batu");
            Vector2[] posKeramat = { new Vector2(-145f, -310f), new Vector2(180f, 80f), new Vector2(-310f, -60f) };
            for (int i = 0; i < posKeramat.Length; i++)
            {
                float y = WorldTerrainGenerator.SampleHeight(posKeramat[i].x, posKeramat[i].y);
                var go = Anak(induk, "PohonKeramat_" + i);
                go.transform.position = new Vector3(posKeramat[i].x, y, posKeramat[i].y);
                go.layer = GameLayers.Interactable; go.tag = GameTags.Interactable;
                var bc = go.AddComponent<BoxCollider>(); bc.size = new Vector3(2.4f, 2.2f, 2.4f); bc.center = new Vector3(0f, 1.1f, 0f);
                Hias(go, PrimitiveType.Cylinder, "BatangKeramat", new Vector3(0f, 3.2f, 0f), new Vector3(1.4f, 3.2f, 1.4f), kulit, true);
                Hias(go, PrimitiveType.Cylinder, "KainLilit", new Vector3(0f, 1.45f, 0f), new Vector3(1.52f, 0.45f, 1.52f), kainPutih, false);
                Hias(go, PrimitiveType.Cube, "BatuSesajen", new Vector3(0f, 0.32f, -1.35f), new Vector3(1.1f, 0.64f, 0.85f), batu, true);
                go.AddComponent<PohonKeramatSesajen>();
                JumlahPohonKeramat++;
            }

            // Audio Ambien Spasial Mega-Landmark
            var audGo = Anak(induk, "AudioDinamisRimba");
            var aud = audGo.AddComponent<AudioDinamisRimba>();
            aud.Pasang(
                new Vector3(312f, WorldTerrainGenerator.SampleHeight(312f, -165f), -165f),
                new Vector3(-265f, WorldTerrainGenerator.SampleHeight(-265f, 115f), 115f),
                new Vector3(272f, WorldTerrainGenerator.SampleHeight(272f, -218f), -218f),
                new Vector3(-115f, WorldTerrainGenerator.SampleHeight(-115f, -865f), -865f));
            EditorUtility.SetDirty(aud);

            Catat("Sistem Lanjutan SOTF: Kombo 3-serangan, Level Survival 4 atribut, " + JumlahRadioDarurat + " radio darurat, "
                + JumlahPohonKeramat + " pohon keramat sesajen & audio ambien spasial mega-landmark");
        }
    }
}
