using System;
using System.IO;
using AlasKeramat.Building;
using UnityEngine;
namespace AlasKeramat.Core
{
    public class DiagnostikMain : MonoBehaviour
    {
        const int Kapasitas=300;
        readonly float[] _ms=new float[Kapasitas],_urut=new float[Kapasitas];
        int _jumlah,_pos;float _jeda;
        public string Commit;
        public string Ringkas { get; private set; }="Menunggu sampel main...";
        void Awake() { GameServices.Register(this); }
        void Update()
        {
            if(Time.timeScale<=0f || Time.realtimeSinceStartup<5f)return;
            _ms[_pos]=Time.unscaledDeltaTime*1000f;_pos=(_pos+1)%Kapasitas;_jumlah=Mathf.Min(Kapasitas,_jumlah+1);
            _jeda+=Time.unscaledDeltaTime;if(_jeda<1f)return;_jeda=0f;
            Array.Copy(_ms,_urut,_jumlah);Array.Sort(_urut,0,_jumlah);
            float total=0f;for(int i=0;i<_jumlah;i++)total+=_urut[i];
            float p95=_urut[Mathf.Clamp(Mathf.CeilToInt(_jumlah*0.95f)-1,0,_jumlah-1)];
            var veg=GameServices.Get<VegetationManager>();
            Ringkas="FPS "+(1000f/Mathf.Max(1f,total/_jumlah)).ToString("0.0")+" | P95 frame "+p95.ToString("0.0")+" ms | sampel "+_jumlah+
                "\nPohon terlihat "+(veg!=null?veg.InstanceTerlihat:0)+" | segitiga vegetasi "+(veg!=null?veg.SegitigaTerlihat:0);
        }
        public void SalinLaporan()
        {
            string data="ALAS KERAMAT "+Application.version+" / "+Commit+"\n"+SystemInfo.deviceModel+" | "+SystemInfo.graphicsDeviceName+
                " | RAM "+SystemInfo.systemMemorySize+" MB\n"+Screen.width+"x"+Screen.height+"\n"+Ringkas;
            GUIUtility.systemCopyBuffer=data;
            try { File.WriteAllText(Path.Combine(Application.persistentDataPath,"laporan_main.txt"),data); }
            catch(Exception e) { Debug.LogWarning("[AK][DiagnostikMain] Salin berkas gagal: "+e.Message); }
            GameEvents.RaiseToast("LAPORAN HP DISALIN");
        }
    }
}
