using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
namespace AlasKeramat.EditorTools
{
    // Langkah 7v (T10d): statistik pemain, Helikopter SAR di Landasan (0,-840), layar sinema (intro, Pilihan Takdir, Ending A/B, statistik).
    public static partial class NusantaraMasterBuilder
    {
        public static readonly Vector3 PosLandasanSAR = new Vector3(0f, 0f, -840f);

        static void LangkahEnding()
        {
            Anak(_sistem, "StatistikPemain").AddComponent<StatistikPemain>();
            var root = Anak(_root, "HeliSAR");
            root.transform.position = Permukaan(PosLandasanSAR.x, PosLandasanSAR.z, 0f);
            var heli = GameObject.Find("HeliRusak");
            GameObject model;
            if (heli != null)
            {
                model = Object.Instantiate(heli, root.transform);
                model.name = "ModelHeli";
                model.transform.localPosition = Vector3.up * 1f; model.transform.localRotation = Quaternion.Euler(0f, 35f, 0f);
            }
            else model = Hias(root, PrimitiveType.Capsule, "ModelHeli", Vector3.up * 1.5f, new Vector3(2.5f, 3f, 2.5f), AmbilMat("M_CatOranye"), false);
            var h = root.AddComponent<HeliSAR>(); h.Pasang(model); EditorUtility.SetDirty(h);
            model.SetActive(false);
            BangunUISinema();
            Catat("Ending: Heli SAR, layar intro/Pilihan Takdir/Ending A-B/statistik, Mode Sandbox");
        }

        static void BangunUISinema()
        {
            var kanvas = _ui != null ? _ui.transform.Find("Canvas_HUD") : null;
            if (kanvas == null) { Debug.LogError("[AK][Builder] Canvas_HUD tidak ada, layar sinema dilewati"); return; }
            var font = ProceduralAssetFactory.GetSafeFont();
            Text tx;
            var akar = UIStretch(kanvas.gameObject, "LayarSinema", Vector2.zero, Vector2.one);
            var sinema = akar.AddComponent<LayarSinema>();
            var hitam = UIStretch(akar, "Hitam", Vector2.zero, Vector2.one).AddComponent<Image>();
            hitam.color = new Color(0f, 0f, 0f, 0f); hitam.enabled = false;

            var besar = BuatTeks(akar, "TeksBesar", "", font, 38, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100f, 100f));
            Regang(besar.GetComponent<RectTransform>(), new Vector2(0.12f, 0.25f), new Vector2(0.88f, 0.78f));
            besar.horizontalOverflow = HorizontalWrapMode.Wrap; besar.verticalOverflow = VerticalWrapMode.Overflow; besar.color = Color.white; besar.lineSpacing = 1.2f;
            var merah = BuatTeks(akar, "TeksMerah", "", font, 42, new Vector2(0.5f, 0.18f), Vector2.zero, new Vector2(1200f, 70f));
            merah.color = new Color(1f, 0.15f, 0.1f);

            var lewati = TombolPanel(akar, "TombolLewati", "LEWATI", font, Vector2.zero, new Vector2(240f, 80f), ProcTex.Hex("#3A3A3A"), out tx);
            var lrt = lewati.GetComponent<RectTransform>(); lrt.anchorMin = lrt.anchorMax = new Vector2(1f, 0f); lrt.anchoredPosition = new Vector2(-170f, 70f);
            UnityEventTools.AddPersistentListener(lewati.onClick, sinema.Lewati);

            var pilihan = UIStretch(akar, "PanelPilihan", new Vector2(0.15f, 0.25f), new Vector2(0.85f, 0.8f));
            var jp = BuatTeks(pilihan, "Judul", "PILIHAN TAKDIR", font, 54, new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(1200f, 80f));
            jp.color = ProcTex.Hex("#F2C14E"); jp.gameObject.AddComponent<Outline>();
            var a = TombolPanel(pilihan, "TombolPulang", "NAIK HELIKOPTER & PULANG", font, new Vector2(0f, 330f), new Vector2(1100f, 120f), ProcTex.Hex("#2F5D3A"), out tx);
            tx.fontSize = 36;
            UnityEventTools.AddPersistentListener(a.onClick, sinema.PilihA);
            var b = TombolPanel(pilihan, "TombolMenjaga", "AMBIL RANSEL & TETAP MENJAGA ALAS KERAMAT", font, new Vector2(0f, 150f), new Vector2(1100f, 120f), ProcTex.Hex("#6A2A18"), out tx);
            tx.fontSize = 34;
            UnityEventTools.AddPersistentListener(b.onClick, sinema.PilihB);
            pilihan.SetActive(false);

            var stat = UIStretch(akar, "PanelStatistik", new Vector2(0.27f, 0.1f), new Vector2(0.73f, 0.9f));
            stat.AddComponent<Image>().color = new Color(0.06f, 0.05f, 0.05f, 0.92f);
            var js = BuatTeks(stat, "Judul", "STATISTIK", font, 46, new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(800f, 70f));
            js.color = ProcTex.Hex("#F2C14E");
            var isi = BuatTeks(stat, "Isi", "", font, 32, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100f, 100f));
            Regang(isi.GetComponent<RectTransform>(), new Vector2(0.1f, 0.25f), new Vector2(0.9f, 0.85f));
            isi.alignment = TextAnchor.MiddleLeft; isi.color = Color.white; isi.lineSpacing = 1.25f;
            Text labelLanjut;
            var lanjut = TombolPanel(stat, "TombolLanjut", "LANJUT", font, new Vector2(-190f, 80f), new Vector2(340f, 90f), ProcTex.Hex("#2F5D3A"), out labelLanjut);
            UnityEventTools.AddPersistentListener(lanjut.onClick, sinema.Lanjut);
            var baru = TombolPanel(stat, "TombolMulaiBaru", "MULAI BARU", font, new Vector2(190f, 80f), new Vector2(340f, 90f), ProcTex.Hex("#7A1F1F"), out tx);
            UnityEventTools.AddPersistentListener(baru.onClick, sinema.MulaiBaru);
            stat.SetActive(false);

            sinema.Pasang(hitam, besar, merah, isi, lewati.gameObject, pilihan, stat, baru.gameObject, labelLanjut);
            lewati.gameObject.SetActive(false);
            EditorUtility.SetDirty(sinema);
            akar.transform.SetAsLastSibling();
            var gelapGua = kanvas.Find("LayarHitamGua"); if (gelapGua != null) gelapGua.SetAsLastSibling();
        }

        static void Regang(RectTransform rt, Vector2 min, Vector2 maks) { rt.anchorMin = min; rt.anchorMax = maks; rt.offsetMin = rt.offsetMax = Vector2.zero; }
    }
}
