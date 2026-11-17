using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Pool proxy pohon interaktif: tiap 0,5 s mencari instance vegetasi yang bisa ditebang/dipanen dalam 30 m dari kamera,
    // memasang collider kapsul + DirectionalTree (layer Tree) tanpa membuat GameObject per pohon jauh (H10).
    public class PohonInteraktif : MonoBehaviour
    {
        public const int MaksProxy = 32;
        public const float Radius = 30f, Interval = 0.5f, RadiusBatang = 0.45f;

        readonly VegetationManager.InfoPohon[] _buf = new VegetationManager.InfoPohon[256];
        DirectionalTree[] _pool;
        VegetationManager _vm;
        float _t;

        public int Aktif { get; private set; }

        void Awake()
        {
            GameServices.Register(this);
            _pool = new DirectionalTree[MaksProxy];
            for (int i = 0; i < MaksProxy; i++)
            {
                var go = new GameObject("ProxyPohon_" + i);
                go.transform.SetParent(transform, false);
                go.layer = GameLayers.Tree; go.tag = GameTags.TreeNode;
                var c = go.AddComponent<CapsuleCollider>();
                c.radius = RadiusBatang; c.height = 6f; c.center = new Vector3(0f, 3f, 0f);
                _pool[i] = go.AddComponent<DirectionalTree>();
                go.SetActive(false);
            }
        }

        public void Lepas(DirectionalTree t) { if (t != null) t.gameObject.SetActive(false); }

        void Update()
        {
            _t += Time.deltaTime;
            if (_t < Interval) return;
            _t = 0f;
            if (_vm == null) _vm = GameServices.Get<VegetationManager>();
            var cam = Camera.main;
            if (_vm == null || cam == null) return;
            int n = _vm.Cari(cam.transform.position, Radius, _buf);
            // Pertahankan proxy yang masih valid (takikan tidak hilang), isi slot kosong dengan pohon baru.
            for (int i = 0; i < MaksProxy; i++)
            {
                var p = _pool[i];
                if (!p.gameObject.activeSelf) continue;
                if (_vm.Tersembunyi(p.Info.id) || (p.Info.posisi - cam.transform.position).sqrMagnitude > Radius * Radius * 1.5f) p.gameObject.SetActive(false);
            }
            Aktif = 0;
            for (int k = 0; k < n; k++)
            {
                if (!DirectionalTree.Interaktif(_buf[k].jenis)) continue;
                if (SudahAda(_buf[k].id)) { Aktif++; continue; }
                var slot = Kosong();
                if (slot == null) break;
                slot.Info = _buf[k];
                for (int q = 0; q < 4; q++) slot.CutDepth[q] = 0f;
                slot.transform.position = _buf[k].posisi;
                var c = slot.GetComponent<CapsuleCollider>();
                bool bambu = _buf[k].jenis == ProcPohon.Jenis.BambuRumpun;
                c.radius = (bambu ? 0.8f : RadiusBatang) * Mathf.Clamp(_buf[k].skala, 0.6f, 2.5f);
                slot.gameObject.SetActive(true);
                Aktif++;
            }
        }

        bool SudahAda(int id) { for (int i = 0; i < MaksProxy; i++) if (_pool[i].gameObject.activeSelf && _pool[i].Info.id == id) return true; return false; }
        DirectionalTree Kosong() { for (int i = 0; i < MaksProxy; i++) if (!_pool[i].gameObject.activeSelf) return _pool[i]; return null; }
    }
}
