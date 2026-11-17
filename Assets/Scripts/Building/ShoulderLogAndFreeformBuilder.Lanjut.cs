using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.Building
{
    public partial class ShoulderLogAndFreeformBuilder
    {
        public const float SudutAtapMin = 20f, SudutAtapMaks = 45f, SudutTanggaMin = 25f, SudutTanggaMaks = 40f;
        public bool ModeCelah { get { return ModeAktif == ModeBangun.CelahJendela || ModeAktif == ModeBangun.CelahPintu; } }
        public void PanggulBatu()
        {
            var inv = GameServices.Get<TikarPandan3DInventory>();
            if (Jumlah != 0 || inv == null || !inv.Punya(ItemID.BatuBesar)) { GameEvents.RaiseToast("Butuh Batu Besar dan bahu kosong"); return; }
            if (!Panggul(Muatan.Batu, SafeMaterialFactory.GetCached("BatuBangun", Color.gray, 0f, 0.2f))) return;
            inv.Kurangi(ItemID.BatuBesar, 1); ModeAktif = ModeBangun.FondasiBatu;
        }
        public static bool Kotak(ModeBangun m) { return m == ModeBangun.Papan || m >= ModeBangun.Atap; }
        public static bool SudutSah(ModeBangun m, Vector3 a, Vector3 b)
        {
            Vector3 d = b - a;
            float sudut = Mathf.Atan2(Mathf.Abs(d.y), new Vector2(d.x, d.z).magnitude) * Mathf.Rad2Deg;
            return d.magnitude <= BentangMaks && sudut >= (m == ModeBangun.Atap ? SudutAtapMin : SudutTanggaMin) && sudut <= (m == ModeBangun.Atap ? SudutAtapMaks : SudutTanggaMaks);
        }
        public static Quaternion RotasiBidangMiring(Vector3 bawah, Vector3 atas)
        {
            Vector3 sumbu=(atas-bawah).normalized;
            Vector3 kanan=Vector3.Cross(sumbu,Vector3.up).normalized;
            if(kanan.sqrMagnitude<0.001f)kanan=Vector3.right;
            return Quaternion.LookRotation(Vector3.Cross(kanan,sumbu).normalized,sumbu);
        }
        static Rencana RencanaLanjut(RaycastHit hit, Vector3 arah, ModeBangun mode, BagianStruktur kena)
        {
            var r = new Rencana { mode = mode, rot = Quaternion.identity };
            if (mode >= ModeBangun.FondasiBatu && mode <= ModeBangun.JalanBatu)
            {
                Vector3 p = hit.point;
                p.x = Mathf.Round(p.x / 0.6f) * 0.6f; p.z = Mathf.Round(p.z / 0.6f) * 0.6f;
                float h = mode == ModeBangun.JalanBatu ? 0.12f : 0.4f;
                p.y = kena != null ? kena.transform.position.y + kena.transform.localScale.y * 0.5f : hit.point.y;
                r.pos = p + Vector3.up * h * 0.5f; r.panjang = h; r.a = kena; r.tertanam = kena == null;
                float? wl = WorldTerrainGenerator.GetWaterLevelAt(p.x, p.z);
                r.sah = (!wl.HasValue || wl.Value <= p.y) && Vector3.Angle(hit.normal, Vector3.up) <= LerengMaks && (kena == null || kena.Batu) && Tingkat(p) < StructureIntegrity.MaksTingkat;
                return r;
            }
            if (kena == null || (kena.Jenis != BagianStruktur.Tipe.Tiang && kena.Jenis != BagianStruktur.Tipe.Balok)) return r;
            Vector3 atas = kena.Jenis == BagianStruktur.Tipe.Tiang ? kena.Puncak : Proyeksi(kena, hit.point);
            Vector3 datar = new Vector3(arah.x, 0f, arah.z).normalized;
            Vector3 bawah = atas + datar * 2.7f;
            bawah.y = WorldTerrainGenerator.SampleHeight(bawah.x, bawah.z);
            BagianStruktur penopang = null;
            foreach (var s in BagianStruktur.Semua)
            {
                if (s == kena || s.Jenis != BagianStruktur.Tipe.Balok) continue;
                Vector3 q = Proyeksi(s, bawah);
                if (Vector2.Distance(new Vector2(q.x, q.z), new Vector2(bawah.x, bawah.z)) < 1.2f && q.y < atas.y) { bawah = q; penopang = s; break; }
            }
            float? air = WorldTerrainGenerator.GetWaterLevelAt(bawah.x, bawah.z);
            r.pos = (atas + bawah) * 0.5f; r.rot = RotasiBidangMiring(bawah, atas);
            r.panjang = Vector3.Distance(atas, bawah); r.a = kena; r.b = penopang; r.tertanam = penopang == null;
            r.sah = SudutSah(mode, atas, bawah) && (!air.HasValue || air.Value <= bawah.y);
            return r;
        }
        static Rencana AnakTangga(RaycastHit hit, BagianStruktur tangga)
        {
            Vector3 lokal = tangga.transform.InverseTransformPoint(hit.point);
            float jarak = Mathf.Round(lokal.y * tangga.Panjang / 0.3f) * 0.3f;
            Vector3 p = tangga.transform.position + tangga.transform.up * Mathf.Clamp(jarak, -tangga.Panjang * 0.5f, tangga.Panjang * 0.5f);
            return new Rencana { sah = true, mode = ModeBangun.Papan, panjang = 0.9f, pos = p + Vector3.up * 0.08f, rot = Quaternion.LookRotation(Vector3.up, tangga.transform.right), a = tangga, b = tangga };
        }
        static void Pengapit(BagianStruktur s)
        {
            // Dua pengapit; collider tengah dimatikan sehingga anak tangga perlu dipasang dengan papan.
            s.GetComponent<Renderer>().enabled = false; s.GetComponent<Collider>().enabled = false;
            for (int i = 0; i < 2; i++)
            {
                var g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = "Pengapit";
                g.transform.SetParent(s.transform, false); g.transform.localPosition = new Vector3(i == 0 ? -0.45f : 0.45f, 0f, 0f);
                g.transform.localScale = new Vector3(0.15f, 1f, 1f); g.layer = GameLayers.Structure;
                g.GetComponent<Renderer>().sharedMaterial = s.GetComponent<Renderer>().sharedMaterial;
            }
        }
    }
}
