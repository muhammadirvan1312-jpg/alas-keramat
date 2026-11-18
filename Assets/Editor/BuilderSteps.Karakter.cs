using System.Collections.Generic;
using System.IO;
using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using J = AlasKeramat.Core.ResepKarakter.JenisKarakter;

namespace AlasKeramat.EditorTools
{
    // Langkah 7g (T4a): 12 humanoid prosedural (mesh + atlas + kerangka 24 sendi + ProceduralAnimator), Andi 2 renderer (badan terlihat, kepala-lengan bayangan saja),
    // Deden duduk terhuyung di ekor heli, Ningsih di Sarang, Galeri Karakter (nonaktif; menu Debug).
    public static partial class NusantaraMasterBuilder
    {
        public const string FolderKarakter = FolderGen + "/Characters";
        public static readonly Vector3 PosDeden = new Vector3(24f, 0f, -440f), PosNingsih = new Vector3(200f, 0f, -350f), PosGaleri = new Vector3(56f, 0f, -376f);
        public const float JarakGaleri = 2.4f;
        public const string NamaGaleri = "GaleriKarakter";
        static readonly Vector3 TahiLalatLokal = new Vector3(-0.036f, -0.014f, 0.081f);

        static readonly Dictionary<string, Mesh> _meshKarakter = new Dictionary<string, Mesh>();
        static readonly Dictionary<J, Material> _matKarakter = new Dictionary<J, Material>();

        static void LangkahKarakter()
        {
            EnsureFolder(FolderKarakter);
            _meshKarakter.Clear(); _matKarakter.Clear();SiapkanKarakterReal();
            var andi = Object.FindFirstObjectByType<AndiFPSController>();
            Transform kamAndi = andi != null ? andi.Kamera : null;

            if (andi != null)
            {
                var model = BuatKarakter(J.Andi, "ModelAndi", ProceduralAnimator.Mode.Normal, true);
                model.transform.SetParent(andi.transform, false);
                SetLayer(model, GameLayers.Player);
            }

            var npc = Anak(_root, "Karakter");
            var deden = BuatKarakter(J.Deden, "KangDeden", ProceduralAnimator.Mode.DudukTerhuyung, false);
            Tempatkan(deden, npc, PosDeden, Arah(PosDeden, SpawnAndi));
            deden.tag = GameTags.Companion; SetLayer(deden, GameLayers.Companion);
            deden.GetComponent<ProceduralAnimator>().TargetLihat = kamAndi;
            var ningsih = BuatKarakter(J.Ningsih, "Ningsih", ProceduralAnimator.Mode.Normal, false);
            Tempatkan(ningsih, npc, PosNingsih, 200f);
            ningsih.tag = GameTags.Companion; SetLayer(ningsih, GameLayers.Companion);
            ningsih.GetComponent<ProceduralAnimator>().TargetLihat = kamAndi;

            var galeri = Anak(_root, NamaGaleri);
            var semua = (J[])System.Enum.GetValues(typeof(J));
            for (int i = 0; i < semua.Length; i++)
            {
                var mode = semua[i] == J.Pocong ? ProceduralAnimator.Mode.Pocong : semua[i] == J.Kuntilanak ? ProceduralAnimator.Mode.Melayang : ProceduralAnimator.Mode.Galeri;
                var k = BuatKarakter(semua[i], "Galeri_" + semua[i], mode, false);
                Vector3 p = PosGaleri + new Vector3((i - (semua.Length - 1) * 0.5f) * JarakGaleri, 0f, 0f);
                Tempatkan(k, galeri, p, 180f);
            }
            galeri.SetActive(false);
            Catat("Karakter: Andi (2 renderer), Deden di heli, Ningsih di Sarang, galeri " + semua.Length + " humanoid");
        }

        static float Arah(Vector3 dari, Vector3 ke) { return Mathf.Atan2(ke.x - dari.x, ke.z - dari.z) * Mathf.Rad2Deg; }

        static void Tempatkan(GameObject k, GameObject induk, Vector3 xz, float yaw)
        {
            k.transform.SetParent(induk.transform, false);
            float h = Mathf.Max(0f, WorldTerrainGenerator.SampleHeight(xz.x, xz.z));
            k.transform.position = new Vector3(xz.x, h, xz.z);
            k.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        static void SetLayer(GameObject g, int layer) { foreach (var t in g.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = layer; }

        // Dibangun di titik asal (identitas) agar bindpose sama untuk semua instance satu jenis; lalu dipindah.
        public static GameObject BuatKarakter(J j, string nama, ProceduralAnimator.Mode mode, bool andi)
        {
            var d = ResepKarakter.Ambil(j);
            var root = new GameObject(nama);
            var tulang = Kerangka.Buat(root.transform, d.tinggi, d.lebar);
            var mat = MaterialKarakter(j);
            bool real=CobaKarakterReal(j,root,tulang,andi);
            if (!real && andi)
            {
                Renderer(root, "Andi_Body", MeshKarakter(j, ResepKarakter.Bagian.BadanSaja, root.transform, tulang), tulang, mat, ShadowCastingMode.On);
                Renderer(root, "Andi_HeadArms", MeshKarakter(j, ResepKarakter.Bagian.KepalaLengan, root.transform, tulang), tulang, mat, ShadowCastingMode.ShadowsOnly);
                var tl = new GameObject("TahiLalat").transform;
                tl.SetParent(tulang[Kerangka.Head], false);
                tl.position = tulang[Kerangka.Head].position + new Vector3(0f, 0.085f, 0.01f) + TahiLalatLokal;
            }
            else if(!real) Renderer(root, "Tubuh", MeshKarakter(j, ResepKarakter.Bagian.Semua, root.transform, tulang), tulang, mat, ShadowCastingMode.On);
            var anim = root.AddComponent<ProceduralAnimator>();
            anim.Pasang(tulang, mode, d.tinggi);
            EditorUtility.SetDirty(anim);
            return root;
        }

        static void Renderer(GameObject root, string nama, Mesh mesh, Transform[] tulang, Material mat, ShadowCastingMode bayangan)
        {
            var go = new GameObject(nama);
            go.transform.SetParent(root.transform, false);
            var smr = go.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = mesh; smr.bones = tulang; smr.rootBone = tulang[Kerangka.Pelvis];
            smr.sharedMaterial = mat; smr.shadowCastingMode = bayangan;
            smr.localBounds = new Bounds(new Vector3(0f, 0f, 0f), new Vector3(2.5f, 3.2f, 2.5f));
        }

        static Mesh MeshKarakter(J j, ResepKarakter.Bagian b, Transform root, Transform[] tulang)
        {
            string kunci = j + "_" + b;
            Mesh m;
            if (_meshKarakter.TryGetValue(kunci, out m) && m != null) return m;
            m = ResepKarakter.BangunMesh(j, b, root, tulang);
            string p = FolderKarakter + "/M_" + kunci + ".asset";
            AssetDatabase.DeleteAsset(p);
            AssetDatabase.CreateAsset(m, p);
            _meshKarakter[kunci] = m;
            return m;
        }

        static Material MaterialKarakter(J j)
        {
            Material m;
            if (_matKarakter.TryGetValue(j, out m) && m != null) return m;
            string pt = FolderKarakter + "/Atlas_" + j + ".png";
            var tex = CharacterMeshBuilder.Atlas(ResepKarakter.WarnaAtlas(j));
            File.WriteAllBytes(pt, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(pt);
            AturImpor(pt, false, false, CharacterMeshBuilder.UkuranAtlas);
            var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(pt);
            var normKain = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderTex + "/kain_n.png");
            Color? emisi = j == J.Pocong ? new Color(0.04f, 0.05f, 0.045f) : j == J.Kuntilanak ? new Color(0.06f, 0.02f, 0.02f) : j == J.Genderuwo ? new Color(0.12f, 0.02f, 0.01f) : (Color?)null;
            m = SafeMaterialFactory.CreatePBR("M_Karakter_" + j, Color.white, 0f, j == J.AbdiLumpur ? 0.55f : 0.28f, atlas, normKain, emisi);
            string pm = FolderKarakter + "/M_Karakter_" + j + ".mat";
            AssetDatabase.DeleteAsset(pm);
            AssetDatabase.CreateAsset(m, pm);
            m = AssetDatabase.LoadAssetAtPath<Material>(pm);
            _matKarakter[j] = m;
            return m;
        }

        [MenuItem("Nusantara Builder/Debug/Galeri Karakter (saat Play)")]
        static void MenuGaleri()
        {
            if (!EditorApplication.isPlaying) { EditorUtility.DisplayDialog("Galeri Karakter", "Tekan Play dulu, lalu klik menu ini lagi.", "OK"); return; }
            var root = GameObject.Find(NamaRoot);
            var g = root != null ? root.transform.Find(NamaGaleri) : null;
            var andi = Object.FindFirstObjectByType<AndiFPSController>();
            if (g == null || andi == null) { Debug.LogError("[AK][Builder] Galeri atau Andi tidak ada"); return; }
            g.gameObject.SetActive(true);
            var cc = andi.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            float h = Mathf.Max(0f, WorldTerrainGenerator.SampleHeight(PosGaleri.x, PosGaleri.z - 7f));
            andi.transform.SetPositionAndRotation(new Vector3(PosGaleri.x, h + 0.2f, PosGaleri.z - 7f), Quaternion.identity);
            if (cc != null) cc.enabled = true;
        }
    }
}
