using System;
using System.Collections.Generic;
using AlasKeramat.Core;
using AlasKeramat.Player;
using AlasKeramat.World;
using UnityEngine;
using Object=UnityEngine.Object;
namespace AlasKeramat.EditorTools
{
    public static class PemeriksaanT10a
    {
        public static void Jalankan(Action<string,bool> cek)
        {
            var graphs=Object.FindObjectsByType<WaypointGraph>(FindObjectsSortMode.None);
            var gates=Object.FindObjectsByType<GerbangGua>(FindObjectsSortMode.None);
            var path=new List<Vector3>();bool meshSah=graphs.Length==3,masuk=true,keluar=true,terhubung=true,lantai=true;
            Physics.SyncTransforms();
            for(int id=1;id<=3;id++)
            {
                bool adaMasuk=false,adaKeluar=false;
                foreach(var g in gates)
                {
                    if((g.Jenis==JenisGerbangGua.Masuk || g.Jenis==JenisGerbangGua.Gali) && (int)WorldCaveAndSaveManager.KompleksPada(g.Tujuan)==id)adaMasuk=true;
                    if(g.Jenis==JenisGerbangGua.Keluar && (int)WorldCaveAndSaveManager.KompleksPada(g.transform.position)==id && g.Tujuan.y>-40f)adaKeluar=true;
                }
                masuk &= adaMasuk;keluar &= adaKeluar;
            }
            foreach(var graph in graphs)
            {
                var filter=graph.GetComponent<MeshFilter>();var collider=graph.GetComponent<MeshCollider>();
                meshSah &= filter!=null && filter.sharedMesh!=null && collider!=null && collider.sharedMesh==filter.sharedMesh;
                if(filter!=null && filter.sharedMesh!=null)meshSah &= filter.sharedMesh.triangles.Length/3<=60000;
                for(int i=0;i<graph.Jumlah;i++)
                {
                    terhubung &= graph.Cari(0,i,path) && path.Count>0;
                    RaycastHit hit;
                    lantai &= Physics.Raycast(graph.Simpul[i]+Vector3.up*0.8f,Vector3.down,out hit,3f,1<<GameLayers.Ground,QueryTriggerInteraction.Ignore);
                }
            }
            cek("T10a: 3 kompleks mesh/collider <= 60000 segitiga",meshSah);
            cek("T10a: UG1-UG3 punya pintu masuk & keluar permukaan",masuk && keluar);
            cek("T10a: waypoint tiap ruang terhubung dan lantai raycast valid",terhubung && lantai);
            var manager=Object.FindFirstObjectByType<WorldCaveAndSaveManager>();
            cek("T10a: air hanya terowongan UG1, bukan permukaan/bunker",manager!=null && manager.AirPada(new Vector3(-524f,-83f,-628f)).HasValue && !manager.AirPada(new Vector3(0f,-80f,-600f)).HasValue && !manager.AirPada(Vector3.zero).HasValue);
            GameObject go=null,blok=null;
            try
            {
                go=new GameObject("UjiGerbangT10");blok=new GameObject("UjiBlokT10");var g=go.AddComponent<GerbangGua>();
                g.Pasang(JenisGerbangGua.Gali,Vector3.zero,"uji",blok);
                bool gali=!g.CobaBuka(false,true) && g.Pukulan==0;
                for(int i=0;i<3;i++)gali &= !g.CobaBuka(true,true);
                string tiga=g.CaptureJson();gali &= g.CobaBuka(true,true) && g.Pukulan==4 && !blok.activeSelf;
                g.RestoreJson(tiga);gali &= g.Pukulan==3 && !g.Terbuka && blok.activeSelf && g.CobaBuka(true,true);
                cek("T10a: Cangkul tepat 4 pukulan, save 3 lalu muat lanjut 4",gali);
                foreach(var jenis in new[]{JenisGerbangGua.Kartu,JenisGerbangGua.Selam,JenisGerbangGua.Reruntuhan})
                {
                    g.RestoreJson("{\"pukulan\":0,\"terbuka\":false}");g.Pasang(jenis,Vector3.zero,"uji",blok);
                    bool valid=!g.CobaBuka(false,true) && !g.CobaBuka(true,false) && blok.activeSelf && g.CobaBuka(true,true) && !blok.activeSelf;
                    string simpan=g.CaptureJson();g.RestoreJson("{\"terbuka\":false}");g.RestoreJson(simpan);
                    cek("T10a: gerbang "+jenis+" menolak tanpa alat/sisi, restore terbuka",valid && g.Terbuka && !blok.activeSelf);
                }
            }
            finally { if(go!=null)Object.DestroyImmediate(go);if(blok!=null)Object.DestroyImmediate(blok); }
            cek("T10a: zipline turun maksimum 16 dan naik 3 m/s",AndiFPSController.KecepatanTali(Vector3.down)==16f && AndiFPSController.KecepatanTali(Vector3.up)==3f && Object.FindObjectsByType<JangkarZipline>(FindObjectsSortMode.None).Length==2);
            cek("T10a: angin, reverb & ambien pada tiga kompleks",Object.FindFirstObjectByType<LorongAnginGua>()!=null && Object.FindObjectsByType<AudioGua>(FindObjectsSortMode.None).Length==3);
        }
    }
}
