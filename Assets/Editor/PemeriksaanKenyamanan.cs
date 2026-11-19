using System;
using AlasKeramat.Building;
using AlasKeramat.Core;
using AlasKeramat.Player;
using AlasKeramat.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlasKeramat.EditorTools
{
    public static class PemeriksaanKenyamanan
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            var a = new GameObject("UjiPemilikJedaA"); var b = new GameObject("UjiPemilikJedaB"); float skala = Time.timeScale;
            try
            {
                JedaDunia.Atur(a.GetInstanceID(), true); JedaDunia.Atur(b.GetInstanceID(), true);
                JedaDunia.Atur(a.GetInstanceID(), false);
                cek("Jeda: satu panel ditutup, panel lain tetap menjeda", JedaDunia.Dijeda && Time.timeScale == 0f);
                JedaDunia.HitStop(0.05f); JedaDunia.Atur(b.GetInstanceID(), false);
                cek("Jeda: hit-stop saat panel tidak meninggalkan slow motion", !JedaDunia.Dijeda && Time.timeScale == 1f);
            }
            finally { JedaDunia.Atur(a.GetInstanceID(), false); JedaDunia.Atur(b.GetInstanceID(), false); Time.timeScale = skala; Object.DestroyImmediate(a); Object.DestroyImmediate(b); }
            var rect = new Rect(0, 0, 864, 756);
            cek("Analog: basis tidak keluar zona di tepi layar", ZonaJoystick.BatasBasis(new Vector2(0, 0), rect, 150) == new Vector2(150, 150) && ZonaJoystick.BatasBasis(new Vector2(860, 750), rect, 150) == new Vector2(714, 606));
            cek("Langkah: jalan dan sprint tidak menjadi rentetan 5-7 bunyi per detik", AndiFPSController.JalanMaks / FoleyAndi.LangkahJalan < 3f && AndiFPSController.Sprint / FoleyAndi.LangkahLari < 4f);
            var m = Matrix4x4.TRS(new Vector3(4, 2, 8), Quaternion.Euler(0, 37, 0), new Vector3(2, 3, 1));
            var lokal = new Bounds(Vector3.up, new Vector3(4, 2, 6)); var dunia = VegetationManager.BatasInstance(lokal, m); bool meliputi = true;
            for (int x = -1; x <= 1; x += 2) for (int y = -1; y <= 1; y += 2) for (int z = -1; z <= 1; z += 2)
                meliputi &= dunia.Contains(m.MultiplyPoint3x4(lokal.center + Vector3.Scale(lokal.extents, new Vector3(x, y, z))));
            cek("Culling: bounds instance mencakup mesh yang diputar dan diskalakan", meliputi);
            bool lengan = false;
            foreach (var model in Object.FindObjectsByType<PenandaModelReal>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var renderer = model.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer == null || renderer.sharedMesh == null || renderer.sharedMesh.name != "Ningsih") continue;
                var copy = Object.Instantiate(model.gameObject); Mesh mesh = null;
                try
                {
                    copy.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity); copy.transform.localScale = Vector3.one;
                    var pose = copy.GetComponent<AlasKeramat.AI.ProceduralAnimator>();
                    if (pose == null) break;
                    pose.TerapkanPoseFoto(); var smr = copy.GetComponentInChildren<SkinnedMeshRenderer>();
                    mesh = new Mesh(); smr.BakeMesh(mesh); var vertices = mesh.vertices; var weights = smr.sharedMesh.boneWeights;
                    float kiri = 0f, kanan = 0f; int nk = 0, nn = 0;
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        if (weights[i].boneIndex0 == Kerangka.LFore && weights[i].weight0 > 0.5f) { kiri += vertices[i].x; nk++; }
                        if (weights[i].boneIndex0 == Kerangka.RFore && weights[i].weight0 > 0.5f) { kanan += vertices[i].x; nn++; }
                    }
                    float bahuK = smr.transform.InverseTransformPoint(smr.bones[Kerangka.LUpper].position).x;
                    float bahuN = smr.transform.InverseTransformPoint(smr.bones[Kerangka.RUpper].position).x;
                    lengan = nk > 100 && nn > 100 && kiri / nk < bahuK - 0.015f && kanan / nn > bahuN + 0.015f;
                }
                finally { if (mesh != null) Object.DestroyImmediate(mesh); Object.DestroyImmediate(copy); }
                break;
            }
            cek("Pose nyata: kedua lengan bawah Ningsih berada di luar bahu, bukan terlipat masuk torso", lengan);
        }
    }
}
