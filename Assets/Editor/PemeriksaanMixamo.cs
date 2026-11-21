using System;
using AlasKeramat.AI;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using PT = AlasKeramat.AI.ProceduralAnimator.PoseTempur;
namespace AlasKeramat.EditorTools
{
    // Self-test animasi mocap Mixamo: pemetaan keadaan -> klip (murni), dan bila aset ada: bank, proksi, klip legacy, pemasangan.
    public static class PemeriksaanMixamo
    {
        public static void Jalankan(Action<string, bool> cek)
        {
            cek("Mixamo: diam/jalan/lari/serang/gertak memetakan klip yang benar",
                AnimasiMixamo.KlipUntuk(AnimasiMixamo.Peran.Sekte, 0f, PT.Tidak) == "Breathing_Idle"
                && AnimasiMixamo.KlipUntuk(AnimasiMixamo.Peran.Wanita, 1.2f, PT.Tidak) == "Female_Walk"
                && AnimasiMixamo.KlipUntuk(AnimasiMixamo.Peran.Sekte, 4f, PT.Tidak) == "Running"
                && AnimasiMixamo.KlipUntuk(AnimasiMixamo.Peran.Sekte, 0f, PT.Ayun) == "Standing_Melee_Attack_Horizontal"
                && AnimasiMixamo.KlipUntuk(AnimasiMixamo.Peran.Sekte, 0f, PT.Gertak) == "Zombie_Scream"
                && AnimasiMixamo.KlipUntuk(AnimasiMixamo.Peran.Sekte, 0f, PT.Tari) == "");
            cek("Mixamo: klip berulang vs sekali", ImporMixamo.Berulang("Walking") && ImporMixamo.Berulang("Zombie_Running") && !ImporMixamo.Berulang("Dying") && !ImporMixamo.Berulang("Hit_Reaction"));
            var synth = Object.FindFirstObjectByType<AlasKeramat.Core.ProceduralAudioSynth>();
            int rekam = 0;
            if (synth != null && synth.NamaPanggang != null)
                for (int i = 0; i < synth.NamaPanggang.Length && i < synth.KlipPanggang.Length; i++)
                    if (synth.KlipPanggang[i] != null && AssetDatabase.GetAssetPath(synth.KlipPanggang[i]).Contains("/kenney/")) rekam++;
            cek("Audio: rekaman CC0 Kenney terpasang (" + rekam + "/" + NusantaraMasterBuilder.PetaKenney.GetLength(0) + ")", rekam >= NusantaraMasterBuilder.PetaKenney.GetLength(0) - 1);
            int oga = 0;
            if (synth != null && synth.NamaPanggang != null)
                for (int i = 0; i < synth.NamaPanggang.Length && i < synth.KlipPanggang.Length; i++)
                    if (synth.KlipPanggang[i] != null && AssetDatabase.GetAssetPath(synth.KlipPanggang[i]).Contains("/oga/")) oga++;
            cek("Audio: rekaman CC0 OpenGameArt terpasang (" + oga + "/" + NusantaraMasterBuilder.PetaOGA.GetLength(0) + ")", oga >= NusantaraMasterBuilder.PetaOGA.GetLength(0) - 1);
            cek("Audio: suara hujan ikut cuaca", AlasKeramat.Core.SuaraHujan.TargetUntuk(AlasKeramat.Core.WeatherState.HujanBadai) > AlasKeramat.Core.SuaraHujan.TargetUntuk(AlasKeramat.Core.WeatherState.Gerimis)
                && AlasKeramat.Core.SuaraHujan.TargetUntuk(AlasKeramat.Core.WeatherState.Cerah) == 0f && synth != null && synth.GetComponent<AlasKeramat.Core.SuaraHujan>() != null);
            if (!AssetDatabase.IsValidFolder(ImporMixamo.Folder)) { Debug.Log("[AK][SelfTest] Mixamo: aset belum ada (repo perlu private), pemeriksaan aset dilewati"); return; }
            var bank = AssetDatabase.LoadAssetAtPath<BankAnimasiMixamo>(NusantaraMasterBuilder.PathBankMixamo);
            cek("Mixamo: bank animasi ada (>= 40 klip)", bank != null && bank.klip.Length >= 40);
            if (bank == null) return;
            bool legacy = true; foreach (var k in bank.klip) legacy &= k != null && k.legacy;
            cek("Mixamo: semua klip legacy (H1)", legacy);
            bool hips = false; if (bank.proksi != null) foreach (var t in bank.proksi.GetComponentsInChildren<Transform>(true)) hips |= t.name.EndsWith(":Hips");
            cek("Mixamo: proksi berkerangka mixamorig tanpa renderer", hips && bank.proksi.GetComponentsInChildren<Renderer>(true).Length == 0);
            string[] wajib = { "Breathing_Idle", "Walking", "Running", "Female_Walk", "Injured_Walk", "Standing_Melee_Attack_Horizontal", "Hit_Reaction", "Zombie_Scream", "Crouch_Walking", "Kneeling_Idle", "Crawling" };
            bool ada = true; string hilang = ""; foreach (var w in wajib) if (bank.Cari(w) == null) { ada = false; hilang += w + " "; }
            cek("Mixamo: klip wajib tersedia " + hilang, ada);
            cek("Mixamo: retarget terpasang di Deden/Ningsih", Object.FindObjectsByType<AnimasiMixamo>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length >= 2);
        }
    }
}
