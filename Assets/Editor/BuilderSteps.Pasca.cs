using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AlasKeramat.EditorTools
{
    // Langkah 3f (T3c): Volume global pasca-proses (ACES, color grading, bloom, vignette, split-tone) + aktifkan pasca-proses di semua kamera.
    public static partial class NusantaraMasterBuilder
    {
        public const string PathVolume = FolderGen + "/Render/AK_Volume.asset";

        static void LangkahPasca()
        {
            AssetDatabase.DeleteAsset(PathVolume);
            var prof = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(prof, PathVolume);

            var tm = prof.Add<Tonemapping>(true); tm.mode.Override(TonemappingMode.ACES);
            var ca = prof.Add<ColorAdjustments>(true);
            ca.postExposure.Override(0.24f); ca.contrast.Override(10f); ca.saturation.Override(2f);
            ca.colorFilter.Override(Color.white);
            var wb = prof.Add<WhiteBalance>(true); wb.temperature.Override(2f); wb.tint.Override(1f);
            var bl = prof.Add<Bloom>(true); bl.threshold.Override(1.10f); bl.intensity.Override(0.32f); bl.scatter.Override(0.72f);
            var vg = prof.Add<Vignette>(true); vg.intensity.Override(0.18f); vg.smoothness.Override(0.48f); vg.color.Override(new Color(0.03f, 0.02f, 0.015f));
            var st = prof.Add<SplitToning>(true);
            st.shadows.Override(ProcTex.Hex("#647C8E")); st.highlights.Override(ProcTex.Hex("#B29C7E")); st.balance.Override(-6f);
            var lgg = prof.Add<LiftGammaGain>(true); lgg.lift.Override(new Vector4(1f, 1f, 1f, 0f));
            var chr = prof.Add<ChromaticAberration>(true); chr.intensity.Override(0.04f);
            var fg = prof.Add<FilmGrain>(true); fg.type.Override(FilmGrainLookup.Thin1); fg.intensity.Override(0.14f); fg.response.Override(0.8f);
            foreach (var k in prof.components) AssetDatabase.AddObjectToAsset(k, prof);
            EditorUtility.SetDirty(prof);
            AssetDatabase.SaveAssets();

            var lighting = GameObject.Find(NamaRoot + "/_Lighting");
            var go = Anak(lighting != null ? lighting : _root, "VolumeGlobal");
            var vol = go.AddComponent<Volume>();
            vol.isGlobal = true; vol.priority = 0f; vol.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(PathVolume);
            var pp = go.AddComponent<AKPascaProses>();
            pp.Pasang(vol);
            EditorUtility.SetDirty(pp);
            AktifkanPasca();
            Catat("Pasca-proses: ACES, color grading, bloom, vignette, split-tone + dinamis jam/cuaca");
        }

        // Dipanggil juga setelah kamera Andi dibuat.
        public static void AktifkanPasca()
        {
            foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var d = cam.GetUniversalAdditionalCameraData();
                if (d == null) continue;
                d.renderPostProcessing = true;
                d.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                EditorUtility.SetDirty(d);
            }
        }
    }
}
