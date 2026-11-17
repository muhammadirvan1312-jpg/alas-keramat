using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.Building
{
    // 07 §4-5: memanggul kayu di bahu kiri (maks 2 gelondong / 4 papan), LEMPAR, bangun bebas dengan pratinjau hantu hijau/merah:
    // TIANG (tanah / di atas tiang), BALOK (antar 2 tiang <= 3,6 m pada kelipatan 0,42 m, atau pondasi), PAPAN (di atas 2 balok sejajar).
    public class ShoulderLogAndFreeformBuilder : MonoBehaviour
    {
        public enum Muatan { Kosong, Gelondong, Papan }
        public enum ModeBangun { Tiang, Balok, Papan }
        public const int MaksGelondong = 2, MaksPapan = 4;
        public const float Jangkau = 3.5f, Benam = 0.2f, Tumpuk = 0.42f, BentangMaks = 3.6f, CariTiang = 1.6f, LerengMaks = 30f;
        public const float VLempar = 3f, VAtasLempar = 1f;

        [SerializeField] Transform bahu;
        [SerializeField] Material hantuSah, hantuSalah;

        public Muatan Bawa { get; private set; }
        public int Jumlah { get; private set; }
        public ModeBangun ModeAktif { get; private set; }
        public string Prompt { get; private set; } = "";

        readonly Material[] _mat = new Material[MaksPapan];
        AndiFPSController _ctrl;
        GameObject _hantu;
        MeshFilter _hantuMf;
        MeshRenderer _hantuMr;
        Mesh _kapsul, _kotak;
        Rencana _r;

        public struct Rencana
        {
            public bool sah; public ModeBangun mode; public Vector3 pos; public Quaternion rot; public float panjang; public bool tertanam;
            public BagianStruktur a, b;
        }

        public void Pasang(Transform b, Material sah, Material salah) { bahu = b; hantuSah = sah; hantuSalah = salah; }

        void Awake()
        {
            GameServices.Register(this);
            _ctrl = GetComponent<AndiFPSController>();
            var tmp = GameObject.CreatePrimitive(PrimitiveType.Capsule); _kapsul = tmp.GetComponent<MeshFilter>().sharedMesh; Destroy(tmp);
            tmp = GameObject.CreatePrimitive(PrimitiveType.Cube); _kotak = tmp.GetComponent<MeshFilter>().sharedMesh; Destroy(tmp);
            _hantu = new GameObject("HantuBangun");
            _hantuMf = _hantu.AddComponent<MeshFilter>(); _hantuMr = _hantu.AddComponent<MeshRenderer>();
            _hantuMr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _hantu.SetActive(false);
            Visual();
        }

        public bool Panggul(Muatan m, Material mat)
        {
            if (m == Muatan.Kosong) return false;
            if (Bawa != Muatan.Kosong && Bawa != m) { GameEvents.RaiseToast("Turunkan dulu yang sedang dipanggul"); return false; }
            int maks = m == Muatan.Gelondong ? MaksGelondong : MaksPapan;
            if (Jumlah >= maks) return false;
            _mat[Jumlah] = mat; Jumlah++; Bawa = m;
            ModeAktif = m == Muatan.Papan ? ModeBangun.Papan : (ModeAktif == ModeBangun.Papan ? ModeBangun.Tiang : ModeAktif);
            Visual();
            return true;
        }

        public void GantiMode()
        {
            if (Bawa != Muatan.Gelondong) return;
            ModeAktif = ModeAktif == ModeBangun.Tiang ? ModeBangun.Balok : ModeBangun.Tiang;
            GameEvents.RaiseToast("ModeBangun bangun: " + (ModeAktif == ModeBangun.Tiang ? "TIANG" : "BALOK"));
        }

        Material Ambil()
        {
            Jumlah--;
            var m = _mat[Jumlah];
            if (Jumlah <= 0) { Jumlah = 0; Bawa = Muatan.Kosong; }
            Visual();
            return m;
        }

        public void Lempar()
        {
            if (Jumlah <= 0) return;
            bool papan = Bawa == Muatan.Papan;
            var cam = _ctrl != null ? _ctrl.Kamera : transform;
            var k = KayuFisik.Buat(cam.position + cam.forward * 1.2f - Vector3.up * 0.2f, cam.right, Ambil(), papan);
            var rb = k.GetComponent<Rigidbody>();
            if (rb != null) PhysicsCompat.SetVelocity(rb, cam.forward * VLempar + Vector3.up * VAtasLempar);
        }

        void Visual()
        {
            if (bahu != null)
                for (int i = 0; i < bahu.childCount; i++)
                {
                    var c = bahu.GetChild(i);
                    bool gel = c.name.StartsWith("Gelondong"), aktif = (gel ? Bawa == Muatan.Gelondong : Bawa == Muatan.Papan) && (c.name[c.name.Length - 1] - '0') < Jumlah;
                    c.gameObject.SetActive(aktif);
                }
            if (_ctrl != null) _ctrl.PengaliKecepatan = Bawa == Muatan.Gelondong ? (Jumlah >= 2 ? 0.75f : 0.85f) : Bawa == Muatan.Papan ? 0.9f : 1f;
        }

        void Update()
        {
            if (_ctrl != null && _ctrl.Berenang && Jumlah > 0) { while (Jumlah > 0) Lempar(); GameEvents.RaiseToast("Kayu terlepas di air"); }
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.G)) Lempar();
            if (Input.GetKeyDown(KeyCode.R)) GantiMode();
#endif
            if (Jumlah <= 0 || _ctrl == null || _ctrl.Kamera == null) { if (_hantu.activeSelf) _hantu.SetActive(false); if (Prompt.Length > 0) Prompt = ""; return; }
            _r = Rencanakan(_ctrl.Kamera.position, _ctrl.Kamera.forward, ModeAktif, Bawa);
            Prompt = _r.sah ? (ModeAktif == ModeBangun.Tiang ? "PASANG TIANG" : ModeAktif == ModeBangun.Balok ? "PASANG BALOK" : "PASANG PAPAN") : "";
            bool tampil = _r.panjang > 0f;
            _hantu.SetActive(tampil);
            if (!tampil) return;
            _hantuMf.sharedMesh = ModeAktif == ModeBangun.Papan ? _kotak : _kapsul;
            _hantuMr.sharedMaterial = _r.sah ? hantuSah : hantuSalah;
            _hantu.transform.SetPositionAndRotation(_r.pos, _r.rot);
            _hantu.transform.localScale = Skala(ModeAktif, _r.panjang);
        }

        public static Vector3 Skala(ModeBangun m, float panjang)
        {
            return m == ModeBangun.Papan ? new Vector3(KayuFisik.UkuranPapan.x, panjang, KayuFisik.UkuranPapan.z) : new Vector3(KayuFisik.Radius * 2f, panjang * 0.5f, KayuFisik.Radius * 2f);
        }

        // Perencanaan penempatan (dipakai juga self-test). panjang <= 0 berarti tidak ada pratinjau.
        public static Rencana Rencanakan(Vector3 asal, Vector3 arah, ModeBangun mode, Muatan bawa)
        {
            var r = new Rencana { mode = mode };
            RaycastHit hit;
            int mask = (1 << GameLayers.Ground) | (1 << GameLayers.Structure) | 1;
            if (!Physics.Raycast(asal, arah, out hit, Jangkau, mask, QueryTriggerInteraction.Ignore)) return r;
            var kena = hit.collider.GetComponent<BagianStruktur>();
            if (mode == ModeBangun.Tiang)
            {
                r.panjang = KayuFisik.Panjang; r.rot = Quaternion.identity;
                if (kena != null && (kena.Jenis == BagianStruktur.Tipe.Tiang) && hit.point.y > kena.Puncak.y - 0.6f)
                {
                    r.pos = kena.Puncak + Vector3.up * KayuFisik.Panjang * 0.5f; r.a = kena;
                    r.sah = Tingkat(r.pos) < StructureIntegrity.MaksTingkat;
                    return r;
                }
                r.pos = hit.point + Vector3.up * (KayuFisik.Panjang * 0.5f - Benam);
                float? wl = WorldTerrainGenerator.GetWaterLevelAt(hit.point.x, hit.point.z);
                r.tertanam = true;
                r.sah = kena == null && Vector3.Angle(hit.normal, Vector3.up) <= LerengMaks && !(wl.HasValue && wl.Value > hit.point.y);
                return r;
            }
            if (mode == ModeBangun.Balok)
            {
                var a = TiangTerdekat(hit.point, null, CariTiang);
                var b = a != null ? TiangTerdekat(hit.point, a, BentangMaks + CariTiang) : null;
                if (a != null && b != null)
                {
                    Vector3 pa = a.transform.position, pb = b.transform.position; pa.y = 0f; pb.y = 0f;
                    float jarak = Vector3.Distance(pa, pb);
                    float dasar = Mathf.Max(a.Ujung(0).y, b.Ujung(0).y), atas = Mathf.Min(a.Puncak.y, b.Puncak.y);
                    float y = dasar + Mathf.Clamp(Mathf.Round((hit.point.y - dasar) / Tumpuk) * Tumpuk, Tumpuk, Mathf.Max(Tumpuk, atas - dasar));
                    r.pos = (pa + pb) * 0.5f + Vector3.up * y;
                    r.rot = Quaternion.FromToRotation(Vector3.up, (pb - pa).normalized);
                    r.panjang = Mathf.Min(jarak, BentangMaks); r.a = a; r.b = b;
                    r.sah = jarak <= BentangMaks && jarak > 0.5f;
                    return r;
                }
                if (kena == null)
                {
                    Vector3 datar = new Vector3(arah.x, 0f, arah.z).normalized;
                    r.pos = hit.point + Vector3.up * KayuFisik.Radius; r.rot = Quaternion.FromToRotation(Vector3.up, datar);
                    r.panjang = KayuFisik.Panjang; r.tertanam = true;
                    r.sah = Vector3.Angle(hit.normal, Vector3.up) <= LerengMaks;
                }
                return r;
            }
            // Papan: di atas balok yang dibidik + balok sejajar lain pada ketinggian sama.
            if (kena == null || kena.Jenis != BagianStruktur.Tipe.Balok) return r;
            var lain = BalokSejajar(kena);
            r.panjang = BentangMaks;
            r.pos = hit.point + Vector3.up * 0.1f; r.rot = Quaternion.FromToRotation(Vector3.up, Vector3.Cross(kena.transform.up, Vector3.up).normalized);
            if (lain == null) return r;
            Vector3 p1 = Proyeksi(kena, hit.point), p2 = Proyeksi(lain, hit.point);
            float bentang = Vector3.Distance(p1, p2);
            r.pos = (p1 + p2) * 0.5f + Vector3.up * (KayuFisik.Radius + KayuFisik.UkuranPapan.z * 0.5f);
            r.rot = Quaternion.FromToRotation(Vector3.up, (p2 - p1).normalized);
            r.panjang = bentang + 0.3f; r.a = kena; r.b = lain;
            r.sah = bentang <= BentangMaks;
            return r;
        }

        static float Tingkat(Vector3 p) { return (p.y - WorldTerrainGenerator.SampleHeight(p.x, p.z)) / StructureIntegrity.TinggiTingkat; }

        static BagianStruktur TiangTerdekat(Vector3 p, BagianStruktur kecuali, float maks)
        {
            BagianStruktur best = null; float bd = maks;
            foreach (var s in BagianStruktur.Semua)
            {
                if (s == kecuali || s.Jenis != BagianStruktur.Tipe.Tiang) continue;
                Vector3 d = s.transform.position - p; d.y = 0f;
                if (kecuali != null) { Vector3 e = s.transform.position - kecuali.transform.position; e.y = 0f; if (e.magnitude > BentangMaks) continue; }
                if (d.magnitude < bd) { bd = d.magnitude; best = s; }
            }
            return best;
        }

        static BagianStruktur BalokSejajar(BagianStruktur b)
        {
            BagianStruktur best = null; float bd = BentangMaks + 0.01f;
            foreach (var s in BagianStruktur.Semua)
            {
                if (s == b || s.Jenis != BagianStruktur.Tipe.Balok) continue;
                if (Mathf.Abs(Vector3.Dot(s.transform.up, b.transform.up)) < 0.95f || Mathf.Abs(s.transform.position.y - b.transform.position.y) > 0.15f) continue;
                Vector3 d = s.transform.position - b.transform.position; d.y = 0f;
                float tegak = Vector3.ProjectOnPlane(d, b.transform.up).magnitude;
                if (tegak > 0.3f && tegak < bd) { bd = tegak; best = s; }
            }
            return best;
        }

        static Vector3 Proyeksi(BagianStruktur b, Vector3 p)
        {
            Vector3 o = b.transform.position, u = b.transform.up;
            float t = Mathf.Clamp(Vector3.Dot(p - o, u), -b.Panjang * 0.5f, b.Panjang * 0.5f);
            return o + u * t;
        }

        // Dipanggil InteraksiAndi (tombol TANGAN) saat Prompt tidak kosong.
        public void PasangSekarang()
        {
            if (!_r.sah || Jumlah <= 0) return;
            var b = Buat(_r, Ambil());
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.PlayLogPickup(b.transform.position);
        }

        public static BagianStruktur Buat(Rencana r, Material mat)
        {
            bool papan = r.mode == ModeBangun.Papan;
            var go = GameObject.CreatePrimitive(papan ? PrimitiveType.Cube : PrimitiveType.Capsule);
            go.name = papan ? "Papan" : r.mode == ModeBangun.Tiang ? "Tiang" : "Balok";
            go.layer = GameLayers.Structure; go.tag = GameTags.StructurePiece;
            go.transform.SetPositionAndRotation(r.pos, r.rot);
            go.transform.localScale = Skala(r.mode, r.panjang);
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            var s = go.AddComponent<BagianStruktur>();
            s.Jenis = papan ? BagianStruktur.Tipe.Papan : r.mode == ModeBangun.Tiang ? BagianStruktur.Tipe.Tiang : BagianStruktur.Tipe.Balok;
            s.Tertanam = r.tertanam; s.Panjang = r.panjang; s.HP = BagianStruktur.HPAwal(s.Jenis);
            if (r.a != null) s.Hubungkan(r.a);
            if (r.b != null) s.Hubungkan(r.b);
            return s;
        }

        // 07 §5 mode 5: 2 pukulan di ujung atas tiang -> pagar runcing.
        public static void Serut(BagianStruktur s)
        {
            if (s == null || s.Jenis != BagianStruktur.Tipe.Tiang) return;
            if (++s.SerutKe < 2) { GameEvents.RaiseToast("SERUT RUNCING 1/2"); return; }
            s.Jenis = BagianStruktur.Tipe.PagarRuncing; s.HP = BagianStruktur.HPRuncing;
            var ujung = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(ujung.GetComponent<Collider>());
            ujung.name = "UjungRuncing";
            ujung.transform.SetParent(s.transform, false);
            ujung.transform.localPosition = new Vector3(0f, 1.05f, 0f); ujung.transform.localScale = new Vector3(0.6f, 0.08f, 0.6f);
            ujung.GetComponent<MeshRenderer>().sharedMaterial = s.GetComponent<MeshRenderer>().sharedMaterial;
            GameEvents.RaiseToast("Tiang jadi pagar runcing");
        }
    }
}
