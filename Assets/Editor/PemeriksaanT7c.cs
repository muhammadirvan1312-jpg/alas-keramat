using System;
using AlasKeramat.AI;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Z = AlasKeramat.AI.NusantaraEnemyAndMutantAI.ZonaKena;

namespace AlasKeramat.EditorTools
{
    // Self-test T7c (12 T7): putus anggota menyembunyikan bagian mesh yang benar & memunculkan prop (tiap varian), batas decal/ragdoll/mayat,
    // zona kena, aturan tingkat gore, pendarahan, ragdoll 11 tulang, kelengkapan scene.
    public static class PemeriksaanT7c
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            cek("Batas gore 14 §10: decal 64, luka 6, ragdoll 4, prop 12, mayat 12, partikel 300", GoreSystem.MaksDecal == 64 && GoreSystem.MaksLuka == 6 && GoreSystem.MaksRagdoll == 4
                && GoreSystem.MaksProp == 12 && GoreSystem.MaksMayat == 12 && GoreSystem.MaksPartikel == 300);
            cek("Tetes darah 6 + damage/4 (maks 40)", GoreSystem.JumlahTetes(0f) == 6 && GoreSystem.JumlahTetes(40f) == 16 && GoreSystem.JumlahTetes(500f) == 40);
            cek("Tingkat gore: Mati tanpa darah/mayat, Sedang tanpa mutilasi, Penuh semua", !GoreSystem.BolehDarah(GoreLevel.Mati) && !GoreSystem.BolehMayat(GoreLevel.Mati)
                && GoreSystem.BolehDarah(GoreLevel.Sedang) && !GoreSystem.BolehMutilasi(GoreLevel.Sedang) && GoreSystem.BolehMutilasi(GoreLevel.Penuh));
            bool zona = NusantaraEnemyAndMutantAI.Zona(new Vector3(0f, 1.6f, 0f), 1.75f) == Z.Kepala && NusantaraEnemyAndMutantAI.Zona(new Vector3(0.3f, 1.2f, 0f), 1.75f) == Z.LenganKanan
                && NusantaraEnemyAndMutantAI.Zona(new Vector3(-0.3f, 1.2f, 0f), 1.75f) == Z.LenganKiri && NusantaraEnemyAndMutantAI.Zona(new Vector3(-0.1f, 0.5f, 0f), 1.75f) == Z.KakiKiri
                && NusantaraEnemyAndMutantAI.Zona(new Vector3(0f, 1.1f, 0f), 1.75f) == Z.Badan
                && Mathf.Approximately(NusantaraEnemyAndMutantAI.PengaliZona(Z.Kepala, false), 1.8f) && Mathf.Approximately(NusantaraEnemyAndMutantAI.PengaliZona(Z.Kepala, true), 1.5f)
                && Mathf.Approximately(NusantaraEnemyAndMutantAI.PengaliZona(Z.KakiKanan, true), 0.7f) && NusantaraEnemyAndMutantAI.TulangPutus(Z.LenganKiri, 0.7f) == Kerangka.LUpper
                && NusantaraEnemyAndMutantAI.TulangPutus(Z.KakiKanan, 0.1f) == Kerangka.RCalf;
            cek("Zona kena 14 §4: kepala x1,8 (melee x1,5), anggota x0,7; tulang putus per zona", zona);
            var d = default(DamageInfo);
            cek("Pendarahan: hanya tebasan Sekte (peluang 25%)", !AlasKeramat.Player.ReaksiAndi.PicuPendarahan(d, 0f));

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NusantaraMasterBuilder.FolderPrefabMusuh + "/Musuh_AbdiSekte.prefab");
            if (prefab == null) { cek("Prefab Musuh_AbdiSekte untuk uji mutilasi", false); return; }
            bool semua = true; string ringkas = "";
            foreach (int akar in GoreSystem.VarianPutus)
            {
                var inst = (GameObject)Object.Instantiate(prefab);
                GameObject prop = null;
                try
                {
                    var smr = inst.GetComponentInChildren<SkinnedMeshRenderer>();
                    int seg;
                    prop = GoreSystem.Putus(smr, akar, Vector3.zero, null, out seg);
                    var bw = smr.sharedMesh.boneWeights; int milik = 0;
                    foreach (var w in bw) if (GoreSystem.DalamSubtree(w.boneIndex0, akar)) milik++;
                    bool tersembunyi = smr.bones[akar].localScale.x < 0.001f && smr.bones[Kerangka.Pelvis].localScale.x > 0.5f;
                    bool lainUtuh = akar == Kerangka.Head ? smr.bones[Kerangka.LUpper].localScale.x > 0.5f : smr.bones[Kerangka.Head].localScale.x > 0.5f;
                    var mf = prop != null ? prop.GetComponent<MeshFilter>() : null;
                    bool ok = prop != null && seg > 0 && milik > 0 && mf != null && mf.sharedMesh.vertexCount <= milik && prop.GetComponent<Rigidbody>() != null && tersembunyi && lainUtuh;
                    ringkas += Kerangka.Nama[akar] + ":" + seg + " ";
                    semua &= ok;
                }
                finally
                {
                    if (prop != null) { var mf = prop.GetComponent<MeshFilter>(); if (mf != null) Object.DestroyImmediate(mf.sharedMesh); Object.DestroyImmediate(prop); }
                    Object.DestroyImmediate(inst);
                }
            }
            cek("Mutilasi 5 varian: tulang tersembunyi, prop dari segitiga subtree, sisa utuh (" + ringkas.Trim() + ")", semua);

            var inst2 = (GameObject)Object.Instantiate(prefab);
            try
            {
                var smr = inst2.GetComponentInChildren<SkinnedMeshRenderer>();
                var r = GoreSystem.PasangRagdoll(smr.bones, Vector3.forward, GameLayers.Enemy);
                int rb = inst2.GetComponentsInChildren<Rigidbody>().Length, sendi = inst2.GetComponentsInChildren<CharacterJoint>().Length;
                cek("Ragdoll 11 tulang & 10 sendi (" + rb + "/" + sendi + ")", r != null && rb == 11 && sendi == 10);
            }
            finally { Object.DestroyImmediate(inst2); }

            var gore = Object.FindFirstObjectByType<GoreSystem>();
            bool sc = gore != null;
            if (gore != null)
            {
                var so = new SerializedObject(gore);
                sc &= so.FindProperty("partikel").objectReferenceValue != null && so.FindProperty("matGenangan").objectReferenceValue != null && so.FindProperty("matLuka").objectReferenceValue != null
                    && so.FindProperty("matTutup").objectReferenceValue != null && so.FindProperty("matPercikan").arraySize == 4 && so.FindProperty("meshLuka").arraySize == 8;
            }
            cek("GoreSystem + partikel + material percikan/genangan/luka/tutup", sc);
            cek("Layar peringatan 18+ & pilihan gore", Object.FindFirstObjectByType<AlasKeramat.UI.LayarPeringatan>(FindObjectsInactive.Include) != null);
        }
    }
}
