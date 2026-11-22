using System;
using AlasKeramat.World;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlasKeramat.EditorTools
{
    // Self-test Overhaul Hero Camera, Senjata FP & Gua Bawah Tanah V11 (Langkah 7z24).
    public static class PemeriksaanHeroDanGuaV11
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            var matKulitFP = AssetDatabase.LoadAssetAtPath<Material>(NusantaraMasterBuilder.FolderMat + "/FP_Kulit.mat");
            var matEmasFP = AssetDatabase.LoadAssetAtPath<Material>(NusantaraMasterBuilder.FolderMat + "/FP_Emas.mat");
            cek("Hero & FP V11: material kulit lengan FP pakai KulitManusia & emas senjata metalik tinggi",
                matKulitFP != null && matKulitFP.shader != null && matKulitFP.shader.name == "AlasKeramat/KulitManusia"
                && matEmasFP != null && matEmasFP.HasProperty("_Metallic") && matEmasFP.GetFloat("_Metallic") > 0.85f);

            var gua = Object.FindFirstObjectByType<AtmosferGuaHidup>(FindObjectsInactive.Include);
            cek("Hero & Gua V11: AtmosferGuaHidup (>= 50 elemen bio/bunker/kubah boss/spora) & diorama hero terpasang",
                gua != null && gua.JumlahElemenGua >= 50
                && AtmosferGuaHidup.FaktorDenyutBio(0.8f, 0f) > 0.7f
                && AtmosferGuaHidup.FaktorKedipBunker(0.2f, 0f) > 0.1f
                && GameObject.Find("ApiRitualUtamaDesa") != null
                && GameObject.Find("JamurBioUG1_0") != null
                && GameObject.Find("TungkuKubahBoss_0") != null);
        }
    }
}
