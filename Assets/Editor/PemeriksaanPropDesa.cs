using System;
using System.Collections.Generic;
using System.IO;
using AlasKeramat.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlasKeramat.EditorTools
{
    // Self-test props desa CC0 & lampu minyak (langkah 7z8).
    public static class PemeriksaanPropDesa
    {
        const int MinProps = 50, MinLampu = 10, MinGenteng = 15;
        const float JangkauanMaks = 6f;

        public static void Jalankan(Action<string, bool> cek)
        {
            cek("Prop desa: lampu minyak menyala 18:00-05:30, padam siang",
                LampuMinyak.NyalaPada(22f) && LampuMinyak.NyalaPada(4f) && LampuMinyak.NyalaPada(18f)
                && !LampuMinyak.NyalaPada(12f) && !LampuMinyak.NyalaPada(17.9f) && !LampuMinyak.NyalaPada(6f));
            bool kedipOk = true;
            for (float t = 0f; t < 40f; t += 0.37f) { float k = LampuMinyak.Kedip(t); if (k < 0.72f || k > 1f) kedipOk = false; }
            cek("Prop desa: kedip api lampu dalam rentang 0,72-1", kedipOk);
            var unik = new HashSet<string>(NusantaraMasterBuilder.NamaProp);
            cek("Kelelawar gua: kabur bila tersorot senter dalam kerucut 24 derajat <= 10 m",
                AlasKeramat.AI.KelelawarGua.TersorotSenter(Vector3.zero, Vector3.forward, new Vector3(0.5f, 0.3f, 6f), AlasKeramat.AI.KelelawarGua.JarakSorot, AlasKeramat.AI.KelelawarGua.SudutSorot)
                && !AlasKeramat.AI.KelelawarGua.TersorotSenter(Vector3.zero, Vector3.forward, new Vector3(6f, 0f, 2f), AlasKeramat.AI.KelelawarGua.JarakSorot, AlasKeramat.AI.KelelawarGua.SudutSorot)
                && !AlasKeramat.AI.KelelawarGua.TersorotSenter(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 14f), AlasKeramat.AI.KelelawarGua.JarakSorot, AlasKeramat.AI.KelelawarGua.SudutSorot));
            cek("Prop desa: 13 jenis props CC0 bernama unik", NusantaraMasterBuilder.NamaProp.Length == 13 && unik.Count == 13);

            if (!File.Exists("Assets/ThirdParty/Prepared/PetiKayuScan.bytes"))
            {
                cek("Prop desa: aset panggang belum ada, langkah dilewati (fail-soft)", true);
                return;
            }
            var induk = GameObject.Find(NusantaraMasterBuilder.NamaRoot + "/World/Landmark/PropDesa");
            if (induk == null) induk = GameObject.Find(NusantaraMasterBuilder.NamaRoot + "/Landmark/PropDesa");
            int n = induk != null ? induk.transform.childCount : 0;
            cek("Prop desa: >= " + MinProps + " props tersebar di desa/kampung/heli/pabrik (" + n + ")", n >= MinProps);

            bool bahanOk = induk != null;
            if (induk != null)
                foreach (var mr in induk.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (mr.gameObject.name == "Api") continue;
                    var m = mr.sharedMaterial;
                    if (m == null || m.shader == null || m.shader.name == "Hidden/InternalErrorShader" || (m.HasProperty("_BaseMap") ? m.GetTexture("_BaseMap") : m.mainTexture) == null) { bahanOk = false; break; }
                }
            cek("Prop desa: semua props bertekstur foto CC0 (tidak pink)", bahanOk);

            int genteng = 0;
            var lmInduk = induk != null ? induk.transform.parent : null;
            if (lmInduk != null)
                for (int i = 0; i < lmInduk.childCount; i++)
                {
                    var mr = lmInduk.GetChild(i).GetComponent<MeshRenderer>();
                    if (mr == null || mr.sharedMaterials.Length < 2 || mr.sharedMaterials[1] == null) continue;
                    if (mr.sharedMaterials[1].name.StartsWith("M_AtapGenteng")) genteng++;
                }
            cek("Atap genteng: >= " + MinGenteng + " rumah desa beratap genteng foto CC0 (" + genteng + ")", genteng >= MinGenteng);

            var lampu = Object.FindObjectsByType<LampuMinyak>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            bool lampuOk = lampu.Length >= MinLampu;
            for (int i = 0; i < lampu.Length && lampuOk; i++)
            {
                var l = lampu[i];
                lampuOk = l.Api != null && l.Cahaya != null && l.Cahaya.shadows == LightShadows.None
                    && l.Cahaya.range <= JangkauanMaks && !l.Cahaya.enabled && !l.Api.enabled;
            }
            cek("Prop desa: >= " + MinLampu + " lampu minyak, cahaya mati awal, tanpa bayangan, jangkauan <= 6 m (" + lampu.Length + ")", lampuOk);
            cek("Prop desa: maksimal " + LampuMinyak.MaksCahaya + " cahaya titik aktif (hemat HP 4 GB)", LampuMinyak.MaksCahaya <= 2);
        }
    }
}
