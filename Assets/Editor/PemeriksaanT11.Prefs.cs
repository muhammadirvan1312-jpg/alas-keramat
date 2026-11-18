using System;
using AlasKeramat.Core;
using AlasKeramat.UI;
using UnityEngine;
using Object=UnityEngine.Object;
namespace AlasKeramat.EditorTools
{
    public static partial class PemeriksaanT11
    {
        static void CekPengaturanT11(MenuScreens menu,Action<string,bool> cek)
        {
            string[] ki={MenuScreens.PrefPreset,GameBootstrap.PrefFps,MenuScreens.PrefBalikY,MenuScreens.PrefHeadBob,MenuScreens.PrefGetar,MenuScreens.PrefImersif,MenuScreens.PrefKesulitan,MenuScreens.PrefSlot,MenuScreens.PrefHoror,MenuScreens.PrefHeadset};
            string[] kf={MenuScreens.PrefSens,MenuScreens.PrefSkalaTombol,MenuScreens.PrefVolMaster,MenuScreens.PrefVolEfek,MenuScreens.PrefVolAmbien,MenuScreens.PrefVolMusik,MenuScreens.PrefFov};
            var adaI=new bool[ki.Length];var nilaiI=new int[ki.Length];var adaF=new bool[kf.Length];var nilaiF=new float[kf.Length];
            for(int i=0;i<ki.Length;i++) { adaI[i]=PlayerPrefs.HasKey(ki[i]);nilaiI[i]=PlayerPrefs.GetInt(ki[i]); }
            for(int i=0;i<kf.Length;i++) { adaF[i]=PlayerPrefs.HasKey(kf[i]);nilaiF[i]=PlayerPrefs.GetFloat(kf[i]); }
            var go=new GameObject("PembacaPengaturanT11");go.SetActive(false);
            try
            {
                var pembaca=go.AddComponent<MenuScreens>();
                menu.SetPreset((int)PresetGrafis.Tinggi);menu.SiklusSkalaTombol();
                float skala=menu.SkalaTombol;menu.SimpanPengaturan();
                pembaca.MuatPengaturan();
                bool valid=pembaca.Preset==PresetGrafis.Tinggi && Mathf.Approximately(pembaca.SkalaTombol,skala);
                // SetPreset memang otomatis menyimpan: pembaca baru harus melihat nilai terbaru.
                menu.SetPreset((int)PresetGrafis.Rendah);pembaca.MuatPengaturan();
                valid &= pembaca.Preset==PresetGrafis.Rendah && Mathf.Approximately(pembaca.SkalaTombol,skala);
                cek("T11: Pengaturan tersimpan & dimuat kembali dari PlayerPrefs",valid);
            }
            finally
            {
                Object.DestroyImmediate(go);
                for(int i=0;i<ki.Length;i++)if(adaI[i])PlayerPrefs.SetInt(ki[i],nilaiI[i]);else PlayerPrefs.DeleteKey(ki[i]);
                for(int i=0;i<kf.Length;i++)if(adaF[i])PlayerPrefs.SetFloat(kf[i],nilaiF[i]);else PlayerPrefs.DeleteKey(kf[i]);
                PlayerPrefs.Save();menu.MuatPengaturan();menu.TerapkanPengaturan();
            }
        }
    }
}
