using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Membangun objek jadi dari blueprint dengan primitif & material bercache (runtime, H5). Lantai/tiang = layer Structure; titik interaksi = Interactable.
    public static partial class BangunanBlueprint
    {
        static readonly Color Kayu = new Color(0.45f, 0.32f, 0.2f), Batu = new Color(0.45f, 0.44f, 0.42f), Daun = new Color(0.36f, 0.42f, 0.2f),
            Bambu = new Color(0.62f, 0.66f, 0.36f), Kain = new Color(0.6f, 0.32f, 0.22f), Tulang = new Color(0.88f, 0.84f, 0.74f), Logam = new Color(0.25f, 0.25f, 0.26f);

        static GameObject P(Transform induk, PrimitiveType t, Vector3 pos, Vector3 skala, Color w, int layer, bool collider, Vector3 rot = default(Vector3))
        {
            var g = GameObject.CreatePrimitive(t);
            g.transform.SetParent(induk, false);
            g.transform.localPosition = pos; g.transform.localScale = skala; g.transform.localRotation = Quaternion.Euler(rot);
            g.layer = layer;
            if (!collider) { var c = g.GetComponent<Collider>(); if (Application.isPlaying) Object.Destroy(c); else Object.DestroyImmediate(c); }
            g.GetComponent<MeshRenderer>().sharedMaterial = SafeMaterialFactory.GetCached("BP_" + ColorUtility.ToHtmlStringRGB(w), w, 0f, 0.3f);
            return g;
        }

        static Light Lampu(Transform induk, Vector3 pos, float range, Color w)
        {
            var g = new GameObject("Cahaya"); g.transform.SetParent(induk, false); g.transform.localPosition = pos;
            var l = g.AddComponent<Light>(); l.type = LightType.Point; l.range = range; l.color = w; l.intensity = 2.2f; l.shadows = LightShadows.None; l.enabled = false;
            return l;
        }

        static FungsiBangunan Fungsi(Transform t, BlueprintID id, Transform[] vis = null, Light l = null, Transform pintu = null, Vector3 atas = default(Vector3))
        {
            t.gameObject.layer = GameLayers.Interactable; t.gameObject.tag = GameTags.Interactable;
            var f = t.gameObject.AddComponent<FungsiBangunan>();
            f.Pasang(id, vis ?? new Transform[0], l, pintu, atas);
            return f;
        }

        public static GameObject Bangun(BlueprintID id, Vector3 pos, Quaternion rot, float bentang = 6f)
        {
            var root = new GameObject("Bangunan_" + id);
            root.transform.SetPositionAndRotation(pos, rot);
            var identitas = root.AddComponent<IdentitasBangunan>(); identitas.Id = id; identitas.Bentang = bentang;
            var r = root.transform;
            if ((int)id >= 15) { BangunV3(root, id); return root; }
            int S = GameLayers.Structure, I = GameLayers.Interactable;
            switch (id)
            {
                case BlueprintID.ApiUnggun:
                {
                    var inti = P(r, PrimitiveType.Cylinder, new Vector3(0f, 0.08f, 0f), new Vector3(1.1f, 0.08f, 1.1f), new Color(0.12f, 0.1f, 0.08f), I, true);
                    for (int i = 0; i < 8; i++) { float a = i * Mathf.PI / 4f; P(r, PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.62f, 0.1f, Mathf.Sin(a) * 0.62f), new Vector3(0.24f, 0.18f, 0.22f), Batu, 0, false); }
                    for (int i = 0; i < 3; i++) P(r, PrimitiveType.Capsule, new Vector3(0f, 0.2f, 0f), new Vector3(0.1f, 0.4f, 0.1f), Kayu, 0, false, new Vector3(60f, i * 120f, 0f));
                    var slot = new Transform[ApiUnggun.Tusuk];
                    for (int i = 0; i < slot.Length; i++) { slot[i] = P(r, PrimitiveType.Sphere, new Vector3(-0.3f + i * 0.2f, 0.55f, 0.35f), Vector3.one * 0.12f, Color.white, 0, false).transform; slot[i].gameObject.SetActive(false); }
                    var api = inti.AddComponent<ApiUnggun>();
                    inti.tag = GameTags.Campfire;
                    api.Pasang(Lampu(r, new Vector3(0f, 0.7f, 0f), 9f, new Color(1f, 0.7f, 0.28f)), slot);
                    break;
                }
                case BlueprintID.SaungPanggung:
                {
                    for (int i = 0; i < 4; i++) P(r, PrimitiveType.Cylinder, new Vector3(i % 2 == 0 ? -1.6f : 1.6f, 1.1f, i < 2 ? -1.6f : 1.6f), new Vector3(0.2f, 1.1f, 0.2f), Kayu, S, true);
                    P(r, PrimitiveType.Cube, new Vector3(0f, 1.0f, 0f), new Vector3(3.6f, 0.12f, 3.6f), Kayu, S, true);
                    P(r, PrimitiveType.Cube, new Vector3(0f, 2.75f, -0.95f), new Vector3(3.9f, 0.08f, 2.2f), Daun, S, true, new Vector3(-28f, 0f, 0f));
                    P(r, PrimitiveType.Cube, new Vector3(0f, 2.75f, 0.95f), new Vector3(3.9f, 0.08f, 2.2f), Daun, S, true, new Vector3(28f, 0f, 0f));
                    P(r, PrimitiveType.Cube, new Vector3(0f, 0.5f, 2.2f), new Vector3(0.8f, 0.06f, 1.4f), Kayu, S, true, new Vector3(-35f, 0f, 0f));
                    var kasur = P(r, PrimitiveType.Cube, new Vector3(0.6f, 1.12f, -0.6f), new Vector3(1.0f, 0.12f, 2.0f), new Color(0.72f, 0.64f, 0.4f), I, true);
                    kasur.name = "KasurPandan"; kasur.tag = GameTags.SaungBed;
                    Fungsi(kasur.transform, id);
                    break;
                }
                case BlueprintID.RakKayu:
                case BlueprintID.KeretaDorongKayu:
                {
                    bool rak = id == BlueprintID.RakKayu;
                    var dasar = P(r, PrimitiveType.Cube, new Vector3(0f, rak ? 0.15f : 0.45f, 0f), rak ? new Vector3(3.6f, 0.1f, 1.2f) : new Vector3(3.6f, 0.1f, 1.2f), Kayu, I, true);
                    if (!rak) for (int i = 0; i < 2; i++) P(r, PrimitiveType.Cylinder, new Vector3(i == 0 ? -0.65f : 0.65f, 0.3f, 1.2f), new Vector3(0.6f, 0.05f, 0.6f), Kayu, 0, false, new Vector3(0f, 0f, 90f));
                    int kap = rak ? 12 : FungsiBangunan.KapKereta;
                    var vis = new Transform[kap];
                    for (int i = 0; i < kap; i++)
                    {
                        int kol = i % 4, baris = i / 4;
                        vis[i] = P(r, PrimitiveType.Capsule, new Vector3(0f, (rak ? 0.4f : 0.7f) + baris * 0.42f, -0.45f + kol * 0.3f), new Vector3(0.4f, 1.7f, 0.4f), Kayu, 0, false, new Vector3(0f, 0f, 90f)).transform;
                        vis[i].gameObject.SetActive(false);
                    }
                    Fungsi(dasar.transform, id, vis);
                    break;
                }
                case BlueprintID.PenampungHujan:
                {
                    for (int i = 0; i < 4; i++) P(r, PrimitiveType.Cylinder, new Vector3(i % 2 == 0 ? -0.6f : 0.6f, 0.6f, i < 2 ? -0.6f : 0.6f), new Vector3(0.08f, 0.6f, 0.08f), Bambu, S, true);
                    P(r, PrimitiveType.Cube, new Vector3(0f, 1.3f, 0f), new Vector3(1.6f, 0.04f, 1.6f), Daun, 0, false, new Vector3(0f, 45f, 0f));
                    var bak = P(r, PrimitiveType.Cylinder, new Vector3(0f, 0.25f, 0f), new Vector3(0.7f, 0.25f, 0.7f), Bambu, I, true);
                    Fungsi(bak.transform, id);
                    break;
                }
                case BlueprintID.JemuranIkan:
                {
                    P(r, PrimitiveType.Cylinder, new Vector3(-0.9f, 0.6f, 0f), new Vector3(0.06f, 0.6f, 0.06f), Kayu, S, true);
                    P(r, PrimitiveType.Cylinder, new Vector3(0.9f, 0.6f, 0f), new Vector3(0.06f, 0.6f, 0.06f), Kayu, S, true);
                    var palang = P(r, PrimitiveType.Cube, new Vector3(0f, 1.15f, 0f), new Vector3(2f, 0.06f, 0.3f), Kayu, I, true);
                    var vis = new Transform[FungsiBangunan.SlotJemur];
                    for (int i = 0; i < vis.Length; i++) { vis[i] = P(r, PrimitiveType.Capsule, new Vector3(-0.75f + i * 0.3f, 0.95f, 0f), new Vector3(0.08f, 0.15f, 0.04f), new Color(0.6f, 0.45f, 0.3f), 0, false).transform; vis[i].gameObject.SetActive(false); }
                    Fungsi(palang.transform, id, vis);
                    break;
                }
                case BlueprintID.PerangkapAyunBambu:
                {
                    P(r, PrimitiveType.Cylinder, new Vector3(-1.1f, 1.2f, 0f), new Vector3(0.1f, 1.2f, 0.1f), Bambu, S, true);
                    P(r, PrimitiveType.Cylinder, new Vector3(1.1f, 1.2f, 0f), new Vector3(0.1f, 1.2f, 0.1f), Bambu, S, true);
                    var poros = new GameObject("Ayunan").transform; poros.SetParent(r, false); poros.localPosition = new Vector3(0f, 2.4f, 0f);
                    P(poros, PrimitiveType.Capsule, new Vector3(0f, -1.2f, 0f), new Vector3(0.3f, 1f, 0.3f), Kayu, 0, false);
                    for (int i = 0; i < 6; i++) P(poros, PrimitiveType.Cylinder, new Vector3(0f, -1.6f + i * 0.15f, 0.25f), new Vector3(0.04f, 0.15f, 0.04f), Bambu, 0, false, new Vector3(90f, 0f, 0f));
                    var picu = P(r, PrimitiveType.Cube, new Vector3(0f, 0.6f, 0f), new Vector3(2.2f, 1.2f, 2.2f), Color.white, I, true);
                    picu.GetComponent<Collider>().isTrigger = true; picu.GetComponent<MeshRenderer>().enabled = false;
                    Fungsi(picu.transform, id, new[] { poros });
                    break;
                }
                case BlueprintID.OborTengkorak:
                {
                    var tiang = P(r, PrimitiveType.Cylinder, new Vector3(0f, 1.0f, 0f), new Vector3(0.08f, 1.0f, 0.08f), Kayu, I, true);
                    P(r, PrimitiveType.Sphere, new Vector3(0f, 2.1f, 0f), new Vector3(0.22f, 0.24f, 0.26f), Tulang, 0, false);
                    P(r, PrimitiveType.Cube, new Vector3(0f, 1.85f, 0f), new Vector3(0.2f, 0.12f, 0.2f), Kain, 0, false);
                    Fungsi(tiang.transform, id, null, Lampu(r, new Vector3(0f, 2.3f, 0f), 12f, new Color(1f, 0.6f, 0.25f)));
                    break;
                }
                case BlueprintID.TendaDarurat:
                {
                    P(r, PrimitiveType.Cube, new Vector3(-0.5f, 0.6f, 0f), new Vector3(0.04f, 1.4f, 1.6f), Kain, S, true, new Vector3(0f, 0f, -38f));
                    P(r, PrimitiveType.Cube, new Vector3(0.5f, 0.6f, 0f), new Vector3(0.04f, 1.4f, 1.6f), Kain, S, true, new Vector3(0f, 0f, 38f));
                    var alas = P(r, PrimitiveType.Cube, new Vector3(0f, 0.04f, 0f), new Vector3(1.4f, 0.06f, 1.7f), new Color(0.72f, 0.64f, 0.4f), I, true);
                    Fungsi(alas.transform, id);
                    break;
                }
                case BlueprintID.PanciMasak:
                {
                    var panci = P(r, PrimitiveType.Cylinder, new Vector3(0f, 0.55f, 0f), new Vector3(0.45f, 0.18f, 0.45f), Logam, I, true);
                    for (int i = 0; i < 2; i++) P(r, PrimitiveType.Cylinder, new Vector3(i == 0 ? -0.35f : 0.35f, 0.35f, 0f), new Vector3(0.05f, 0.35f, 0.05f), Kayu, 0, false);
                    panci.tag = GameTags.Interactable; panci.AddComponent<PanciMasak>();
                    break;
                }
                case BlueprintID.MenaraPengintai:
                {
                    for (int i = 0; i < 4; i++) P(r, PrimitiveType.Cylinder, new Vector3(i % 2 == 0 ? -1.6f : 1.6f, 3.2f, i < 2 ? -1.6f : 1.6f), new Vector3(0.24f, 3.2f, 0.24f), Kayu, S, true);
                    P(r, PrimitiveType.Cube, new Vector3(0f, 6f, 0f), new Vector3(3.6f, 0.14f, 3.6f), Kayu, S, true);
                    var tangga = P(r, PrimitiveType.Cube, new Vector3(0f, 3f, 2.0f), new Vector3(0.7f, 6.2f, 0.1f), Kayu, I, true, new Vector3(-12f, 0f, 0f));
                    Fungsi(tangga.transform, id, null, null, null, new Vector3(0f, 0.55f, -2.0f));
                    break;
                }
                case BlueprintID.PintuBambu:
                {
                    P(r, PrimitiveType.Cylinder, new Vector3(-0.8f, 1.1f, 0f), new Vector3(0.12f, 1.1f, 0.12f), Kayu, S, true);
                    P(r, PrimitiveType.Cylinder, new Vector3(0.8f, 1.1f, 0f), new Vector3(0.12f, 1.1f, 0.12f), Kayu, S, true);
                    var engsel = new GameObject("Engsel").transform; engsel.SetParent(r, false); engsel.localPosition = new Vector3(-0.7f, 0f, 0f);
                    var daun = P(engsel, PrimitiveType.Cube, new Vector3(0.7f, 1.0f, 0f), new Vector3(1.4f, 2.0f, 0.08f), Bambu, I, true);
                    Fungsi(daun.transform, id, null, null, engsel);
                    break;
                }
                case BlueprintID.KentonganKemah:
                {
                    P(r, PrimitiveType.Cylinder, new Vector3(0f, 0.8f, 0f), new Vector3(0.1f, 0.8f, 0.1f), Kayu, S, true);
                    var k = P(r, PrimitiveType.Cylinder, new Vector3(0f, 1.5f, 0.15f), new Vector3(0.18f, 0.35f, 0.18f), Bambu, I, true);
                    Fungsi(k.transform, id);
                    break;
                }
                default:
                {
                    var g = new GameObject("Rakit").transform; g.SetParent(r, false);
                    for (int i = 0; i < 8; i++) P(g, PrimitiveType.Cylinder, new Vector3(-1.05f + i * 0.3f, 0.1f, 0f), new Vector3(0.28f, 1.8f, 0.28f), Bambu, i == 0 ? I : S, true, new Vector3(90f, 0f, 0f));
                    Fungsi(g.GetChild(0), id);
                    break;
                }
            }
            return root;
        }
    }
}
