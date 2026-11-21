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
            cek("Audio: musik jelajah siang/malam terpasang", synth != null && synth.GetComponent<AlasKeramat.AI.MusikJelajah>() != null
                && AlasKeramat.AI.MusikJelajah.PilihKlip(22f, 1) == AlasKeramat.AI.MusikJelajah.KlipMalam && AlasKeramat.AI.MusikJelajah.PilihKlip(10f, 1) == AlasKeramat.AI.MusikJelajah.KlipSiang
                && synth.AmbilRekaman(AlasKeramat.AI.MusikJelajah.KlipSiang) != null);
            cek("Audio: suara hujan ikut cuaca", AlasKeramat.Core.SuaraHujan.TargetUntuk(AlasKeramat.Core.WeatherState.HujanBadai) > AlasKeramat.Core.SuaraHujan.TargetUntuk(AlasKeramat.Core.WeatherState.Gerimis)
                && AlasKeramat.Core.SuaraHujan.TargetUntuk(AlasKeramat.Core.WeatherState.Cerah) == 0f && synth != null && synth.GetComponent<AlasKeramat.Core.SuaraHujan>() != null);
            cek("Mixamo: peta model karakter (Deden=Remy, Ningsih=Peasant_Girl, KiRogo=Peasant_Man, Genderuwo=Warrok)", NusantaraMasterBuilder.ModelUntuk("BossGenderuwo") == "Warrok"
                && NusantaraMasterBuilder.ModelUntuk("Deden") == "Remy" && NusantaraMasterBuilder.ModelUntuk("Ningsih") == "Peasant_Girl" && NusantaraMasterBuilder.ModelUntuk("KiRogo") == "Peasant_Man"
                && NusantaraMasterBuilder.ModelUntuk("Musuh_AbdiWanita") == "Zombiegirl" && NusantaraMasterBuilder.ModelUntuk("Musuh_AbdiSekte") == "Nightshade" && NusantaraMasterBuilder.ModelUntuk("Musuh_KiRogo") == null
                && AlasKeramat.AI.AnimasiMixamo.SkalaTinggi(1.8f, 1.8f) == 1f && AlasKeramat.AI.AnimasiMixamo.SkalaTinggi(3f, 1f) == 2.5f);
            cek("Hewan asli: peta model & pilihan klip", NusantaraMasterBuilder.ModelHewanUntuk(AlasKeramat.Core.ResepHewan.JenisHewan.Rusa) == "rusa"
                && NusantaraMasterBuilder.ModelHewanUntuk(AlasKeramat.Core.ResepHewan.JenisHewan.Harimau) == "harimau"
                && NusantaraMasterBuilder.ModelHewanUntuk(AlasKeramat.Core.ResepHewan.JenisHewan.BabiHutan) == "babi_hutan"
                && NusantaraMasterBuilder.ModelHewanUntuk(AlasKeramat.Core.ResepHewan.JenisHewan.Buaya) == "buaya"
                && NusantaraMasterBuilder.ModelHewanUntuk(AlasKeramat.Core.ResepHewan.JenisHewan.Biawak) == "biawak"
                && NusantaraMasterBuilder.ModelHewanUntuk(AlasKeramat.Core.ResepHewan.JenisHewan.Monyet) == "monyet"
                && NusantaraMasterBuilder.ModelHewanUntuk(AlasKeramat.Core.ResepHewan.JenisHewan.Anjing) == "anjing"
                && NusantaraMasterBuilder.ModelHewanUntuk(AlasKeramat.Core.ResepHewan.JenisHewan.Kucing) == "kucing"
                && NusantaraMasterBuilder.ModelHewanUntuk(AlasKeramat.Core.ResepHewan.JenisHewan.Tawon) == null
                && AlasKeramat.AI.ModelHewanAsli.PilihKlip(0f, 5f, "i", "w", "r") == "i" && AlasKeramat.AI.ModelHewanAsli.PilihKlip(1f, 5f, "i", "w", "r") == "w" && AlasKeramat.AI.ModelHewanAsli.PilihKlip(5f, 5f, "i", "w", "r") == "r"
                && AlasKeramat.AI.ModelHewanAsli.IndeksBahan("Eyes", "Head", null) == -1 && AlasKeramat.AI.ModelHewanAsli.IndeksBahan("Body", "head", new[] { "body", "head" }) == 1);
            cek("Gua & atmosfer: kelelawar gua terbang & god rays sinar tajuk",
                AlasKeramat.AI.KelelawarGua.HitungPosisi(Vector3.zero, 3f, 0.5f, 1.2f, 0f, 1.5f).magnitude > 0.5f
                && Object.FindObjectsByType<AlasKeramat.AI.KelelawarGua>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length >= 6
                && AlasKeramat.Core.AtmosferHutan.KekuatanSinar(8.5f, AlasKeramat.Core.WeatherState.Cerah, false) > 0.5f
                && AlasKeramat.Core.AtmosferHutan.KekuatanSinar(22f, AlasKeramat.Core.WeatherState.Cerah, false) == 0f
                && AlasKeramat.Core.AtmosferHutan.KekuatanSinar(9f, AlasKeramat.Core.WeatherState.Cerah, true) == 0f);
            float pSw;
            cek("Dunia & gameplay SOTF v2: kunang/spora, ayun senjata FP, suara hewan 3D & properti desa/pantai",
                AlasKeramat.Core.PartikelAlam.LajuKunang(21f, AlasKeramat.Core.WeatherState.Cerah, true, false) > 0f
                && AlasKeramat.Core.PartikelAlam.LajuKunang(12f, AlasKeramat.Core.WeatherState.Cerah, true, false) == 0f
                && AlasKeramat.Core.PartikelAlam.LajuKunang(12f, AlasKeramat.Core.WeatherState.Cerah, false, true) > 0f
                && AlasKeramat.Player.CombatAndLeftHandController.HitungAyunFP(1.5f, 4f, true, false) != Quaternion.identity
                && AlasKeramat.AI.WildAnimalAI.KlipSuaraHewan(AlasKeramat.Core.ResepHewan.JenisHewan.Harimau, out pSw) == AlasKeramat.Core.ProceduralAudioSynth.KlipTeriakSekte
                && GameObject.Find("SumurDesa_1") != null && GameObject.Find("PerahuNelayan_0") != null);
            cek("SOTF v3: suar mercusuar malam, tarik busur FP & mesin pabrik gula",
                AlasKeramat.Core.SuarMercusuar.AktifPadaJam(21f, AlasKeramat.Core.WeatherState.Cerah)
                && !AlasKeramat.Core.SuarMercusuar.AktifPadaJam(12f, AlasKeramat.Core.WeatherState.Cerah)
                && AlasKeramat.Player.CombatAndLeftHandController.TarikBusur01(true, 0.5f, 1.0f) == 0.5f
                && AlasKeramat.Player.CombatAndLeftHandController.TarikBusur01(false, 0.5f, 1.0f) == 0f
                && Object.FindFirstObjectByType<AlasKeramat.Core.SuarMercusuar>() != null
                && GameObject.Find("MesinPabrik_0") != null && GameObject.Find("LoriTebu_0") != null);
            if (AssetDatabase.IsValidFolder(ImporHewanAsli.Folder))
            {
                int nh = 0;
                foreach (var w in Object.FindObjectsByType<AlasKeramat.AI.WildAnimalAI>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (w.GetComponent<AlasKeramat.AI.ModelHewanAsli>() != null) nh++;
                cek("Hewan asli: templat memakai model FBX (" + nh + ")", nh >= 3);
            }
            if (AssetDatabase.IsValidFolder(ImporKarakterMixamo.Folder))
            {
                int pakai = 0;
                foreach (var g in AssetDatabase.FindAssets("t:Prefab", new[] { NusantaraMasterBuilder.FolderPrefabMusuh }))
                {
                    var pf = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g));
                    var am = pf != null ? pf.GetComponentInChildren<AlasKeramat.AI.AnimasiMixamo>(true) : null;
                    if (am != null && new SerializedObject(am).FindProperty("modelKarakter").objectReferenceValue != null) pakai++;
                }
                cek("Mixamo: prefab musuh memakai model karakter asli (" + pakai + ")", pakai >= 2);
            }
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
