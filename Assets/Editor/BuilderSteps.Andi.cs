using System.IO;
using AlasKeramat.Core;
using AlasKeramat.Player;
using AlasKeramat.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.EditorTools
{
    // Langkah 7b (T3a): Andi bisa berjalan — spawn dekat heli, kamera orang pertama, joystick mengambang, area geser, tombol, baris info HUD.
    public static partial class NusantaraMasterBuilder
    {
        // Spawn 12 §T3 (-2,-212) dikali 2 (v5 20 §2): di samping heli rusak.
        public static readonly Vector3 SpawnAndi = new Vector3(-4f, 0f, -424f);
        const float TinggiSpawn = 0.2f, FovAndi = 70f, NearAndi = 0.1f, FarAndi = 2400f;
        const int UkuranSpriteUI = 256;

        static void LangkahAndi()
        {
            var lingkaran = SpriteUI("ui_lingkaran", false);
            var cincin = SpriteUI("ui_cincin", true);
            var font = ProceduralAssetFactory.GetSafeFont();

            // Andi
            var andi = Anak(_root, "Andi");
            andi.tag = "Player";
            andi.layer = GameLayers.Player;
            float h = Mathf.Max(0f, WorldTerrainGenerator.SampleHeight(SpawnAndi.x, SpawnAndi.z));
            andi.transform.position = new Vector3(SpawnAndi.x, h + TinggiSpawn, SpawnAndi.z);
            andi.transform.rotation = Quaternion.LookRotation(new Vector3(8f - SpawnAndi.x, 0f, -448f - SpawnAndi.z));
            andi.AddComponent<CharacterController>();
            var ctrl = andi.AddComponent<AndiFPSController>();
            var kam = Anak(andi, "KameraAndi");
            kam.tag = "MainCamera";
            kam.transform.localPosition = new Vector3(0f, AndiFPSController.MataBerdiri, 0f);
            var c = kam.AddComponent<Camera>();
            c.fieldOfView = FovAndi; c.nearClipPlane = NearAndi; c.farClipPlane = FarAndi; c.clearFlags = CameraClearFlags.Skybox;
            kam.AddComponent<AudioListener>();
            ctrl.Pasang(kam.transform);
            EditorUtility.SetDirty(ctrl);

            // Tur pratinjau dimatikan bila Andi ada (12 §T3); kamera tur tetap ada untuk foto/menu.
            if (_kamera != null)
            {
                _kamera.tag = "Untagged";
                var ct = _kamera.GetComponent<Camera>(); if (ct != null) ct.enabled = false;
                var al = _kamera.GetComponent<AudioListener>(); if (al != null) al.enabled = false;
                var tur = _kamera.GetComponent<TurPratinjau>(); if (tur != null) tur.enabled = false;
            }

            var kanvas = _ui != null ? _ui.transform.Find("Canvas_HUD") : null;
            if (kanvas == null) { Debug.LogError("[AK][Builder] Canvas_HUD tidak ada, HUD Andi dilewati"); return; }
            var judul = kanvas.Find("Judul"); if (judul != null) judul.gameObject.SetActive(false);

            // Kontrol sentuh: area geser (belakang) -> zona joystick -> tombol (depan).
            var kontrol = UIStretch(kanvas.gameObject, "Kontrol", Vector2.zero, Vector2.one);
            kontrol.AddComponent<RuangAmanKontrol>();
            var geser = UIStretch(kontrol, "ZonaGeser", new Vector2(0.45f, 0f), Vector2.one);
            Transparan(geser);
            var zg = geser.AddComponent<ZonaGeser>();

            var zonaJoy = UIStretch(kontrol, "ZonaJoystick", Vector2.zero, new Vector2(0.45f, 0.7f));
            ((RectTransform)zonaJoy.transform).pivot = Vector2.zero;
            Transparan(zonaJoy);
            var basis = UIGambar(zonaJoy, "Basis", cincin, new Vector2(300f, 300f), Vector2.zero, new Vector2(260f, 260f));
            var kenop = UIGambar(basis, "Kenop", lingkaran, new Vector2(130f, 130f), new Vector2(0.5f, 0.5f), Vector2.zero);
            var lari = BuatTeks(basis, "LabelLari", "LARI", font, 30, new Vector2(0.5f, 1f), new Vector2(0f, 30f), new Vector2(200f, 40f));
            lari.color = ProcTex.Hex("#FF9A3C");
            lari.enabled = false;
            var zj = zonaJoy.AddComponent<ZonaJoystick>();
            zj.Pasang((RectTransform)basis.transform, (RectTransform)kenop.transform, basis.GetComponent<Image>(), lari);

            var tc = kontrol.AddComponent<TouchControls>();
            tc.Pasang(zj, zg);
            var bLompat = Tombol(kontrol, "TombolLompat", "LOMPAT", lingkaran, font, 160f, new Vector2(-150f, 170f), ProcTex.Hex("#4A6F8A"));
            UnityEventTools.AddPersistentListener(bLompat.onClick, tc.TekanLompat);
            var bJongkok = Tombol(kontrol, "TombolJongkok", "JONGKOK", lingkaran, font, 130f, new Vector2(-330f, 110f), ProcTex.Hex("#5B5A4E"));
            UnityEventTools.AddPersistentListener(bJongkok.onClick, tc.TekanJongkok);
            EditorUtility.SetDirty(zj); EditorUtility.SetDirty(tc);

            // Baris info kiri-atas & nama zona (pakai label tur yang sudah ada).
            var info = kanvas.Find("LabelJam") != null ? kanvas.Find("LabelJam").GetComponent<Text>() : null;
            var zona = kanvas.Find("LabelZona") != null ? kanvas.Find("LabelZona").GetComponent<Text>() : null;
            if (info != null)
            {
                var rt = info.rectTransform;
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
                rt.anchoredPosition = new Vector2(40f, -32f); rt.sizeDelta = new Vector2(1100f, 40f);
                info.fontSize = 26; info.alignment = TextAnchor.MiddleLeft; info.color = ProcTex.Hex("#F2EFE6");
            }
            if (zona != null)
            {
                var rt = zona.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(0f, -150f);
                zona.fontSize = 44;
            }
            var hud = kanvas.gameObject.AddComponent<HUDInfo>();
            hud.Pasang(info, zona);
            EditorUtility.SetDirty(hud);
            Catat("Andi di " + andi.transform.position + ", kontrol sentuh (joystick, geser, LOMPAT, JONGKOK), baris info HUD");
        }

        static GameObject UIStretch(GameObject induk, string nama, Vector2 min, Vector2 max)
        {
            var go = Anak(induk, nama);
            go.layer = GameLayers.UI;
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = rt.offsetMax = Vector2.zero;
            return go;
        }

        static void Transparan(GameObject go)
        {
            var img = go.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0f);
            img.raycastTarget = true;
        }

        static GameObject UIGambar(GameObject induk, string nama, Sprite s, Vector2 besar, Vector2 jangkar, Vector2 pos)
        {
            var go = Anak(induk, nama);
            go.layer = GameLayers.UI;
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = jangkar; rt.sizeDelta = besar; rt.anchoredPosition = pos;
            var img = go.AddComponent<Image>();
            img.sprite = s; img.raycastTarget = false;
            return go;
        }

        static Button Tombol(GameObject induk, string nama, string label, Sprite s, Font font, float d, Vector2 pos, Color warna)
        {
            var go = UIGambar(induk, nama, s, new Vector2(d, d), new Vector2(1f, 0f), pos);
            var img = go.GetComponent<Image>();
            img.color = new Color(warna.r, warna.g, warna.b, 0.75f); img.raycastTarget = true;
            var b = go.AddComponent<Button>();
            b.targetGraphic = img;
            var t = BuatTeks(go, "Label", label, font, 26, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(d, 40f));
            t.color = ProcTex.Hex("#F2EFE6"); t.raycastTarget = false;
            return b;
        }

        // Sprite UI lingkaran/cincin anti-alias, dipanggang ke Generated/Icons.
        static Sprite SpriteUI(string nama, bool cincin)
        {
            string p = FolderIkon + "/" + nama + ".png";
            var tex = new Texture2D(UkuranSpriteUI, UkuranSpriteUI, TextureFormat.RGBA32, false);
            float r = UkuranSpriteUI * 0.5f - 2f, tebal = UkuranSpriteUI * 0.06f;
            var px = new Color32[UkuranSpriteUI * UkuranSpriteUI];
            for (int y = 0; y < UkuranSpriteUI; y++)
                for (int x = 0; x < UkuranSpriteUI; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(UkuranSpriteUI * 0.5f, UkuranSpriteUI * 0.5f));
                    float a = Mathf.Clamp01(r - d + 0.5f);
                    if (cincin) a = Mathf.Max(a * Mathf.Clamp01(d - (r - tebal) + 0.5f), a * 0.18f);
                    px[y * UkuranSpriteUI + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            tex.SetPixels32(px);
            File.WriteAllBytes(p, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(p);
            AturImpor(p, false, true, UkuranSpriteUI);
            return AssetDatabase.LoadAssetAtPath<Sprite>(p);
        }
    }
}
