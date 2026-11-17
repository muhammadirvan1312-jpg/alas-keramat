using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Player;
using AlasKeramat.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlasKeramat.EditorTools
{
    // Langkah 7k (T6a): pemanggul kayu bahu kiri (2 gelondong / 4 papan terlihat saat menunduk), material hantu hijau/merah, tombol LEMPAR & MODE.
    public static partial class NusantaraMasterBuilder
    {
        static void LangkahBangun()
        {
            var andi = Object.FindFirstObjectByType<AndiFPSController>();
            if (andi == null || andi.Kamera == null) return;
            var pb = andi.gameObject.AddComponent<ShoulderLogAndFreeformBuilder>();
            var bahu = Anak(andi.Kamera.gameObject, "BahuKiri").transform;
            bahu.localPosition = new Vector3(-0.38f, -0.3f, 0.25f);
            bahu.localRotation = Quaternion.Euler(80f, 0f, 10f);
            var kulit = AmbilMat("M_KulitPohon"); var papan = AmbilMat("M_Kayu");
            for (int i = 0; i < ShoulderLogAndFreeformBuilder.MaksGelondong; i++) BahuVis(bahu, "Gelondong" + i, PrimitiveType.Capsule, new Vector3(0f, 0f, -i * 0.24f), new Vector3(0.3f, 1.6f, 0.3f), kulit);
            for (int i = 0; i < ShoulderLogAndFreeformBuilder.MaksPapan; i++) BahuVis(bahu, "Papan" + i, PrimitiveType.Cube, new Vector3(0f, 0f, -i * 0.09f), new Vector3(0.36f, 3.2f, 0.08f), papan);
            var sah = SafeMaterialFactory.CreatePBR("M_HantuSah", ProcTex.Hex("#3CFF7A59"), 0f, 0.2f, null, null, null, SurfaceMode.Transparent);
            var salah = SafeMaterialFactory.CreatePBR("M_HantuSalah", ProcTex.Hex("#FF3C3C59"), 0f, 0.2f, null, null, null, SurfaceMode.Transparent);
            AssetDatabase.DeleteAsset(FolderMat + "/M_HantuSah.mat"); AssetDatabase.CreateAsset(sah, FolderMat + "/M_HantuSah.mat");
            AssetDatabase.DeleteAsset(FolderMat + "/M_HantuSalah.mat"); AssetDatabase.CreateAsset(salah, FolderMat + "/M_HantuSalah.mat");
            pb.Pasang(bahu, AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_HantuSah.mat"), AssetDatabase.LoadAssetAtPath<Material>(FolderMat + "/M_HantuSalah.mat"));
            EditorUtility.SetDirty(pb);

            var kanvas = _ui != null ? _ui.transform.Find("Canvas_HUD") : null;
            var kontrol = kanvas != null ? kanvas.Find("Kontrol") : null;
            var tc = kontrol != null ? kontrol.GetComponent<TouchControls>() : null;
            if (tc == null) return;
            var font = ProceduralAssetFactory.GetSafeFont();
            var ling = AssetDatabase.LoadAssetAtPath<Sprite>(FolderIkon + "/ui_lingkaran.png");
            var bL = Tombol(kontrol.gameObject, "TombolLempar", "LEMPAR", ling, font, 110f, new Vector2(-470f, 360f), ProcTex.Hex("#6A4A2A"));
            UnityEventTools.AddPersistentListener(bL.onClick, tc.TekanLempar);
            var bM = Tombol(kontrol.gameObject, "TombolMode", "MODE", ling, font, 100f, new Vector2(-200f, 470f), ProcTex.Hex("#3A5A3A"));
            UnityEventTools.AddPersistentListener(bM.onClick, tc.TekanModeBangun);
            tc.PasangPanggul(bL.gameObject, bM.gameObject);
            EditorUtility.SetDirty(tc);
            Catat("Bangun bebas: pemanggul kayu, hantu, tombol LEMPAR & MODE");
        }

        static void BahuVis(Transform induk, string nama, PrimitiveType t, Vector3 pos, Vector3 skala, Material m)
        {
            var g = GameObject.CreatePrimitive(t);
            Object.DestroyImmediate(g.GetComponent<Collider>());
            g.name = nama; g.layer = GameLayers.Player;
            g.transform.SetParent(induk, false); g.transform.localPosition = pos; g.transform.localScale = skala;
            var mr = g.GetComponent<MeshRenderer>(); if (m != null) mr.sharedMaterial = m; mr.shadowCastingMode = ShadowCastingMode.On;
            g.SetActive(false);
        }
    }
}
