using System.Collections.Generic;
using UnityEngine;

namespace AlasKeramat.Core
{
    // 14 §5-6: mutilasi pada mesh rigid-skinned (skala tulang 0,0001 + prop dari segitiga milik subtree tulang + tutup potongan)
    // dan ragdoll ringan 11 tulang (Rigidbody + CapsuleCollider + CharacterJoint, beku setelah 6 s diam).
    public partial class GoreSystem
    {
        public const float SkalaPutus = 0.0001f, LamaDiamRagdoll = 6f, MassaProp = 3f;
        public static readonly int[] TulangRagdoll = { Kerangka.Pelvis, Kerangka.Chest, Kerangka.Head, Kerangka.LUpper, Kerangka.RUpper, Kerangka.LFore, Kerangka.RFore, Kerangka.LThigh, Kerangka.RThigh, Kerangka.LCalf, Kerangka.RCalf };
        // Varian prop anggota tubuh (14 §5): Kepala, Lengan Atas+Bawah+Tangan, Lengan Bawah+Tangan, Paha+Betis+Kaki, Betis+Kaki.
        public static readonly int[] VarianPutus = { Kerangka.Head, Kerangka.LUpper, Kerangka.RFore, Kerangka.LThigh, Kerangka.RCalf };

        readonly Queue<GameObject> _prop = new Queue<GameObject>();
        readonly List<RagdollDiam> _ragdoll = new List<RagdollDiam>();
        readonly List<GameObject> _mayat = new List<GameObject>();

        static void Hapus(Object o) { if (o == null) return; if (Application.isPlaying) Destroy(o); else DestroyImmediate(o); }

        public static bool DalamSubtree(int tulang, int akar)
        {
            for (int t = tulang; t >= 0; t = Kerangka.Induk[t]) if (t == akar) return true;
            return false;
        }

        // Memutus anggota: mengembalikan prop (null bila gagal). segitiga = jumlah segitiga prop (self-test).
        public static GameObject Putus(SkinnedMeshRenderer smr, int akar, Vector3 impuls, Material tutup, out int segitiga)
        {
            segitiga = 0;
            if (smr == null || smr.sharedMesh == null || smr.bones == null || akar < 0 || akar >= smr.bones.Length || smr.bones[akar] == null) return null;
            var sumber = smr.sharedMesh;
            var bw = sumber.boneWeights;
            if (bw == null || bw.Length != sumber.vertexCount) return null;
            var baked = new Mesh();
            smr.BakeMesh(baked);
            var v = baked.vertices; var n = baked.normals; var uv = sumber.uv; var tri = sumber.triangles;
            var peta = new Dictionary<int, int>();
            var vb = new List<Vector3>(); var nb = new List<Vector3>(); var ub = new List<Vector2>(); var tb = new List<int>();
            for (int i = 0; i < tri.Length; i += 3)
            {
                if (!DalamSubtree(bw[tri[i]].boneIndex0, akar) || !DalamSubtree(bw[tri[i + 1]].boneIndex0, akar) || !DalamSubtree(bw[tri[i + 2]].boneIndex0, akar)) continue;
                for (int k = 0; k < 3; k++)
                {
                    int s = tri[i + k], d;
                    if (!peta.TryGetValue(s, out d)) { d = vb.Count; peta[s] = d; vb.Add(v[s]); nb.Add(n.Length > s ? n[s] : Vector3.up); ub.Add(uv.Length > s ? uv[s] : Vector2.zero); }
                    tb.Add(d);
                }
            }
            Hapus(baked);
            segitiga = tb.Count / 3;
            if (segitiga == 0) return null;
            // Pusat massa ke titik asal mesh prop.
            Vector3 pusat = Vector3.zero;
            for (int i = 0; i < vb.Count; i++) pusat += vb[i];
            pusat /= vb.Count;
            for (int i = 0; i < vb.Count; i++) vb[i] -= pusat;
            var m = new Mesh { name = "Potongan_" + Kerangka.Nama[akar] };
            m.SetVertices(vb); m.SetNormals(nb); m.SetUVs(0, ub); m.SetTriangles(tb, 0); m.RecalculateBounds();

            var t = smr.transform;
            var prop = new GameObject("Potongan_" + Kerangka.Nama[akar]);
            prop.transform.SetPositionAndRotation(t.TransformPoint(pusat), t.rotation);
            prop.transform.localScale = t.lossyScale;
            prop.AddComponent<MeshFilter>().sharedMesh = m;
            prop.AddComponent<MeshRenderer>().sharedMaterial = smr.sharedMaterial;
            var bc = prop.AddComponent<BoxCollider>(); bc.center = m.bounds.center; bc.size = Vector3.Max(m.bounds.size, Vector3.one * 0.06f);
            var rb = prop.AddComponent<Rigidbody>(); rb.mass = MassaProp;
            PhysicsCompat.SetVelocity(rb, impuls);
            rb.angularVelocity = Random.insideUnitSphere * 6f;

            var tulang = smr.bones[akar];
            if (tutup != null)
            {
                var induk = smr.bones[Kerangka.Induk[akar] >= 0 ? Kerangka.Induk[akar] : akar];
                Tutup(induk, tulang.position, tulang.up, tutup);
                Tutup(prop.transform, tulang.position, -tulang.up, tutup);
            }
            tulang.localScale = Vector3.one * SkalaPutus;
            return prop;
        }

        static void Tutup(Transform induk, Vector3 posisi, Vector3 arah, Material mat)
        {
            var c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Hapus(c.GetComponent<Collider>());
            c.name = "TutupPotongan";
            c.transform.SetParent(induk, true);
            c.transform.position = posisi;
            c.transform.up = arah;
            c.transform.localScale = new Vector3(0.09f, 0.006f, 0.09f);
            c.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        // Instance: putus + semburan + batas prop 12 (tertua dihapus).
        public GameObject PutusAnggota(SkinnedMeshRenderer smr, int akar, Vector3 impuls, bool hitam)
        {
            if (!BolehMutilasi(Tingkat)) return null;
            int seg;
            var prop = Putus(smr, akar, impuls, matTutup, out seg);
            if (prop == null) return null;
            _prop.Enqueue(prop);
            while (_prop.Count > MaksProp) Hapus(_prop.Dequeue());
            Semburan(smr.bones[akar].position, impuls.normalized + Vector3.up, 40f, hitam, true);
            Genangan(smr.bones[akar].position);
            return prop;
        }

        // Ragdoll ringan pada kerangka 24 sendi (pakai 11). Mengembalikan komponen pengawas atau null bila gore Mati.
        public static RagdollDiam PasangRagdoll(Transform[] tulang, Vector3 impuls, int layer)
        {
            if (tulang == null || tulang.Length < Kerangka.Jumlah) return null;
            var rb = new Rigidbody[Kerangka.Jumlah];
            float tinggi = Vector3.Distance(tulang[Kerangka.Head].position, tulang[Kerangka.Pelvis].position) / 0.75f;
            for (int k = 0; k < TulangRagdoll.Length; k++)
            {
                int i = TulangRagdoll[k];
                var b = tulang[i];
                if (b == null) continue;
                b.gameObject.layer = layer;
                var r = b.gameObject.AddComponent<Rigidbody>();
                r.mass = i == Kerangka.Pelvis || i == Kerangka.Chest ? 12f : i == Kerangka.Head ? 5f : 3f;
                rb[i] = r;
                if (i == Kerangka.Head)
                {
                    var s = b.gameObject.AddComponent<SphereCollider>(); s.radius = 0.11f * tinggi / 1.75f; s.center = Vector3.up * 0.08f;
                }
                else
                {
                    var c = b.gameObject.AddComponent<CapsuleCollider>();
                    int anak = AnakUtama(i);
                    float pj = anak >= 0 && tulang[anak] != null ? Vector3.Distance(b.position, tulang[anak].position) : 0.3f;
                    c.direction = 1; c.height = Mathf.Max(0.15f, pj); c.center = b.InverseTransformPoint(anak >= 0 && tulang[anak] != null ? (b.position + tulang[anak].position) * 0.5f : b.position);
                    c.radius = (i == Kerangka.Pelvis || i == Kerangka.Chest ? 0.15f : 0.07f) * tinggi / 1.75f;
                }
            }
            for (int k = 1; k < TulangRagdoll.Length; k++)
            {
                int i = TulangRagdoll[k];
                if (rb[i] == null) continue;
                int p = Kerangka.Induk[i];
                while (p >= 0 && rb[p] == null) p = Kerangka.Induk[p];
                if (p < 0) continue;
                var j = rb[i].gameObject.AddComponent<CharacterJoint>();
                j.connectedBody = rb[p];
                float bawah, atas, ayun;
                BatasSendi(i, out bawah, out atas, out ayun);
                j.lowTwistLimit = new SoftJointLimit { limit = bawah }; j.highTwistLimit = new SoftJointLimit { limit = atas };
                j.swing1Limit = new SoftJointLimit { limit = ayun }; j.swing2Limit = new SoftJointLimit { limit = ayun * 0.5f };
                j.enableProjection = true;
            }
            if (rb[Kerangka.Chest] != null) PhysicsCompat.SetVelocity(rb[Kerangka.Chest], impuls);
            var diam = tulang[Kerangka.Pelvis].root.gameObject.AddComponent<RagdollDiam>();
            diam.Pasang(rb);
            return diam;
        }

        static int AnakUtama(int i)
        {
            if (i == Kerangka.Pelvis) return Kerangka.Chest;
            if (i == Kerangka.Chest) return Kerangka.Neck;
            if (i == Kerangka.LUpper) return Kerangka.LFore; if (i == Kerangka.RUpper) return Kerangka.RFore;
            if (i == Kerangka.LFore) return Kerangka.LHand; if (i == Kerangka.RFore) return Kerangka.RHand;
            if (i == Kerangka.LThigh) return Kerangka.LCalf; if (i == Kerangka.RThigh) return Kerangka.RCalf;
            if (i == Kerangka.LCalf) return Kerangka.LFoot; if (i == Kerangka.RCalf) return Kerangka.RFoot;
            return -1;
        }

        // Batas sendi 14 §6: lutut 0..-130, siku 0..140, leher +-45, bahu +-80, pinggul -30..+100.
        public static void BatasSendi(int i, out float bawah, out float atas, out float ayun)
        {
            if (i == Kerangka.LCalf || i == Kerangka.RCalf) { bawah = -130f; atas = 0f; ayun = 5f; return; }
            if (i == Kerangka.LFore || i == Kerangka.RFore) { bawah = -5f; atas = 140f; ayun = 10f; return; }
            if (i == Kerangka.Head) { bawah = -45f; atas = 45f; ayun = 45f; return; }
            if (i == Kerangka.LUpper || i == Kerangka.RUpper) { bawah = -80f; atas = 80f; ayun = 80f; return; }
            if (i == Kerangka.LThigh || i == Kerangka.RThigh) { bawah = -30f; atas = 100f; ayun = 40f; return; }
            bawah = -20f; atas = 20f; ayun = 20f;
        }

        // Instance: ragdoll dengan batas 4 aktif (tertua dibekukan) + daftar mayat maks 12.
        public void Ragdoll(Transform[] tulang, Vector3 impuls, int layer, GameObject mayat)
        {
            if (!BolehMayat(Tingkat)) return;
            var r = PasangRagdoll(tulang, impuls, layer);
            if (r != null)
            {
                _ragdoll.RemoveAll(x => x == null || x.Beku);
                _ragdoll.Add(r);
                while (_ragdoll.Count > MaksRagdoll) { _ragdoll[0].Bekukan(); _ragdoll.RemoveAt(0); }
            }
            DaftarMayat(mayat);
        }

        public void DaftarMayat(GameObject mayat)
        {
            if (mayat == null) return;
            _mayat.RemoveAll(x => x == null);
            _mayat.Add(mayat);
            if (_mayat.Count <= MaksMayat) return;
            var cam = Camera.main;
            for (int i = 0; i < _mayat.Count - 1; i++)
            {
                var m = _mayat[i];
                bool terlihat = cam != null && Vector3.Angle(cam.transform.forward, m.transform.position - cam.transform.position) < 60f && Vector3.Distance(cam.transform.position, m.transform.position) < 80f;
                if (terlihat) continue;
                _mayat.RemoveAt(i); Hapus(m);
                return;
            }
            var tertua = _mayat[0]; _mayat.RemoveAt(0); Hapus(tertua);
        }

        public int JumlahMayat { get { _mayat.RemoveAll(x => x == null); return _mayat.Count; } }
        public int JumlahProp { get { return _prop.Count; } }
        public int RagdollAktif { get { _ragdoll.RemoveAll(x => x == null || x.Beku); return _ragdoll.Count; } }
    }
}
