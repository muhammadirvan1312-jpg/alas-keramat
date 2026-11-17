using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Building
{
    // 07 §2: batang rebah fisik (Rigidbody 400 + HingeJoint di pangkal, torsi ke arah rebah), lepas sendi setelah 25°,
    // pecah jadi kayu gelondong saat menghantam tanah atau 4 s, plus ranting/kelapa sebagai benda ambil. Tunggul 0,5 m tertinggal.
    public class PohonRebah : MonoBehaviour
    {
        public const float Massa = 400f, Torsi = 2600f, SudutLepas = 25f, BatasWaktu = 4f, TinggiTunggul = 0.5f;

        VegetationManager.InfoPohon _info;
        VegetationData.JenisVeg _jenis;
        Rigidbody _rb;
        HingeJoint _sendi;
        Vector3 _arah;
        float _t;
        bool _pecah;

        public static PohonRebah Buat(VegetationManager.InfoPohon info, VegetationData.JenisVeg jenis, Vector3 arah)
        {
            var go = new GameObject("PohonRebah_" + info.id);
            go.layer = GameLayers.Tree;
            go.transform.SetPositionAndRotation(info.posisi + Vector3.up * TinggiTunggul, info.rotasi);
            go.transform.localScale = Vector3.one * info.skala;
            var mf = go.AddComponent<MeshFilter>(); mf.sharedMesh = jenis.lod0;
            var mr = go.AddComponent<MeshRenderer>(); mr.sharedMaterials = new[] { jenis.kayu, jenis.daun };
            float tinggi = jenis.lod0 != null ? jenis.lod0.bounds.size.y * info.skala : 12f;
            var col = go.AddComponent<CapsuleCollider>();
            col.direction = 1; col.radius = 0.3f; col.height = tinggi / Mathf.Max(info.skala, 0.01f); col.center = new Vector3(0f, col.height * 0.5f, 0f);
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = Massa; rb.interpolation = RigidbodyInterpolation.Interpolate;
            var hj = go.AddComponent<HingeJoint>();
            hj.anchor = Vector3.zero;
            hj.axis = go.transform.InverseTransformDirection(Vector3.Cross(Vector3.up, arah).normalized);
            var p = go.AddComponent<PohonRebah>();
            p._info = info; p._jenis = jenis; p._rb = rb; p._sendi = hj; p._arah = arah;
            rb.AddTorque(Vector3.Cross(Vector3.up, arah).normalized * Torsi, ForceMode.Impulse);
            Tunggul(info, jenis);
            return p;
        }

        static void Tunggul(VegetationManager.InfoPohon info, VegetationData.JenisVeg jenis)
        {
            var t = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            t.name = "Tunggul_" + info.id;
            t.layer = GameLayers.Tree;
            float r = 0.32f * info.skala;
            t.transform.SetPositionAndRotation(info.posisi + Vector3.up * TinggiTunggul * 0.5f, Quaternion.identity);
            t.transform.localScale = new Vector3(r * 2f, TinggiTunggul * 0.5f, r * 2f);
            t.GetComponent<MeshRenderer>().sharedMaterial = jenis.kayu;
        }

        void FixedUpdate()
        {
            if (_pecah) return;
            _t += Time.fixedDeltaTime;
            float miring = Vector3.Angle(Vector3.up, transform.up);
            if (_sendi != null && miring > SudutLepas) { Destroy(_sendi); _sendi = null; }
            if (_sendi != null) _rb.AddTorque(Vector3.Cross(Vector3.up, _arah).normalized * Torsi * 0.15f, ForceMode.Force);
            if (_t > BatasWaktu || (miring > 70f && _t > 1.2f)) Pecah();
        }

        void OnCollisionEnter(Collision c)
        {
            if (_pecah || _t < 0.8f) return;
            if (Vector3.Angle(Vector3.up, transform.up) > 55f) Pecah();
        }

        void Pecah()
        {
            _pecah = true;
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.PlayTreeCrackAndFall(transform.position);
            int n = DirectionalTree.JumlahKayu(_info.jenis);
            float tinggi = _jenis.lod0 != null ? _jenis.lod0.bounds.size.y * _info.skala : 12f;
            Vector3 sumbu = transform.up;
            for (int i = 0; i < n; i++)
            {
                Vector3 p = transform.position + sumbu * (KayuFisik.Panjang * (i + 0.5f)) + Vector3.up * 0.4f;
                if ((i + 0.5f) * KayuFisik.Panjang > tinggi) break;
                KayuFisik.Buat(p, sumbu, _jenis.kayu);
            }
            Vector3 tajuk = transform.position + sumbu * tinggi * 0.8f + Vector3.up * 0.5f;
            if (_info.jenis == ProcPohon.Jenis.Jati) KayuFisik.Benda("Ranting", tajuk, ItemID.Ranting, Random.Range(2, 4), "Ranting");
            if (_info.jenis == ProcPohon.Jenis.Mahoni) KayuFisik.Benda("Ranting", tajuk, ItemID.Ranting, 2, "Ranting");
            if (_info.jenis == ProcPohon.Jenis.Kelapa) KayuFisik.Benda("Kelapa", tajuk, ItemID.KelapaMuda, Random.Range(2, 4), "Kelapa Muda");
            GameEvents.RaiseTreeFelled(_info.id, (TreeKind)Mathf.Clamp((int)_info.jenis, 0, 6));
            Destroy(gameObject);
        }
    }
}
