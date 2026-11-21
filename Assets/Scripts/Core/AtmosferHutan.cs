using UnityEngine;
namespace AlasKeramat.Core
{
    // Lapisan kabut lokal mengikuti kamera dengan pembaruan jarang; jumlah kartu tetap dan tanpa alokasi frame.
    public class AtmosferHutan : MonoBehaviour
    {
        public const int JumlahKartu=18;
        public const int JumlahSinar=14;
        public const float Jeda=0.35f;
        [SerializeField] Transform[] kartu=new Transform[0];
        [SerializeField] Renderer[] gambar=new Renderer[0];
        [SerializeField] Transform[] sinar=new Transform[0];
        [SerializeField] Renderer[] gambarSinar=new Renderer[0];
        Camera _kamera;
        URPAtmosphereAndWeather _cuaca;
        MaterialPropertyBlock _blok,_blokSinar;
        float _sisa;
        public void Pasang(Transform[] t,Renderer[] r) { kartu=t;gambar=r; }
        public void PasangSinar(Transform[] t,Renderer[] r) { sinar=t;gambarSinar=r; }
        void Awake() { GameServices.Register(this);_blok=new MaterialPropertyBlock();_blokSinar=new MaterialPropertyBlock(); }
        public static float BasahTarget(WeatherState w)
        { return w==WeatherState.HujanBadai?1f:w==WeatherState.Gerimis?0.55f:0f; }
        public static float KekuatanSinar(float jam,WeatherState w,bool diBawahTanah)
        {
            if(diBawahTanah || w==WeatherState.HujanBadai || jam<6.0f || jam>17.8f)return 0f;
            float k=jam<11.0f?Mathf.InverseLerp(6.0f,7.6f,jam)*Mathf.Lerp(1f,0.72f,Mathf.InverseLerp(7.6f,11.0f,jam))
                :jam<=15.0f?0.72f:Mathf.Lerp(0.72f,1f,Mathf.InverseLerp(15.0f,16.4f,jam))*(1f-Mathf.InverseLerp(16.6f,17.8f,jam));
            if(w==WeatherState.KabutPagi)k*=1.15f;
            else if(w==WeatherState.Gerimis)k*=0.45f;
            else if(w==WeatherState.Berawan)k*=0.55f;
            return Mathf.Clamp01(k);
        }
        public void Terapkan(Camera cam,float jam,WeatherState w)
        {
            if(cam==null)return;
            if(_blok==null)_blok=new MaterialPropertyBlock();
            if(_blokSinar==null)_blokSinar=new MaterialPropertyBlock();
            Vector3 p=cam.transform.position;
            bool bawah=WorldTerrainGenerator.DiBawahTanah(p.y);
            bool aktif=!bawah && p.y<WorldTerrainGenerator.SampleHeight(p.x,p.z)+18f;
            float malam=URPAtmosphereAndWeather.Malam01(jam);
            Shader.SetGlobalFloat("_AK_Basah",Mathf.Max(BasahTarget(w),malam*0.38f));
            bool maghrib=jam>=17.4f && jam<19.2f;
            var bioma=WorldTerrainGenerator.GetBiome(p.x,p.z);
            bool lembab=bioma==Biome.TepiSungai || bioma==WorldTerrainGenerator.RawaBakau;
            float tebal=w==WeatherState.KabutPagi?0.20f:w==WeatherState.HujanBadai?0.15f:maghrib?0.14f:malam>0.5f?0.16f:0.10f;
            if(lembab && (malam>0.25f || w==WeatherState.KabutPagi || (jam>=4.5f && jam<8f))) tebal=Mathf.Min(0.30f,tebal*1.45f);
            Color warna=maghrib?new Color(0.68f,0.34f,0.22f):Color.Lerp(new Color(0.58f,0.66f,0.62f),new Color(0.16f,0.25f,0.34f),malam);warna.a=tebal;
            _blok.SetColor("_BaseColor",warna);
            float gridX=Mathf.Floor(p.x/16f)*16f,gridZ=Mathf.Floor(p.z/16f)*16f;
            for(int i=0;i<kartu.Length;i++)
            {
                if(kartu[i]==null)continue;
                kartu[i].gameObject.SetActive(aktif);if(!aktif)continue;
                float a=(i*137.5f)*Mathf.Deg2Rad,r=10f+i*3.4f;
                Vector3 q=new Vector3(gridX+Mathf.Cos(a)*r,0f,gridZ+Mathf.Sin(a)*r);
                q.y=WorldTerrainGenerator.SampleHeight(q.x,q.z)+2.4f;
                kartu[i].position=q;
                Vector3 lihat=cam.transform.position-q;lihat.y=0f;
                if(lihat.sqrMagnitude>0.01f)kartu[i].rotation=Quaternion.LookRotation(lihat);
                if(i<gambar.Length && gambar[i]!=null)gambar[i].SetPropertyBlock(_blok);
            }
            float kuatSinar=KekuatanSinar(jam,w,bawah);
            bool sinarAktif=kuatSinar>0.02f;
            if(sinarAktif)
            {
                float emas=1f-Mathf.InverseLerp(7f,10.5f,jam)+Mathf.InverseLerp(14.8f,17f,jam);
                Color ws=Color.Lerp(new Color(1f,0.95f,0.82f),new Color(1f,0.82f,0.52f),Mathf.Clamp01(emas));
                ws.a=kuatSinar*0.34f;
                _blokSinar.SetColor("_BaseColor",ws);
            }
            float miring=Mathf.Lerp(36f,-36f,Mathf.InverseLerp(6f,18f,jam));
            for(int i=0;i<sinar.Length;i++)
            {
                if(sinar[i]==null)continue;
                sinar[i].gameObject.SetActive(sinarAktif);if(!sinarAktif)continue;
                float a=((i+0.5f)*137.5f)*Mathf.Deg2Rad,r=9f+i*2.9f;
                Vector3 q=new Vector3(gridX+Mathf.Cos(a)*r,0f,gridZ+Mathf.Sin(a)*r);
                q.y=WorldTerrainGenerator.SampleHeight(q.x,q.z)+9.2f;
                sinar[i].position=q;
                Vector3 lihat=cam.transform.position-q;lihat.y=0f;
                Quaternion hadap=lihat.sqrMagnitude>0.01f?Quaternion.LookRotation(lihat):Quaternion.identity;
                sinar[i].rotation=hadap*Quaternion.Euler(0f,0f,miring*(0.75f+(i%3)*0.12f));
                if(i<gambarSinar.Length && gambarSinar[i]!=null)gambarSinar[i].SetPropertyBlock(_blokSinar);
            }
        }
        void Update()
        {
            if(_cuaca==null)_cuaca=GameServices.Get<URPAtmosphereAndWeather>();
            var jam=GameServices.Get<TimeOfDayClock>();
            float malamF=URPAtmosphereAndWeather.Malam01(jam!=null?jam.Jam:9f);
            float basah=Shader.GetGlobalFloat("_AK_Basah");
            float basahTarget=Mathf.Max(BasahTarget(_cuaca!=null?_cuaca.Cuaca:WeatherState.Cerah),malamF*0.35f);
            Shader.SetGlobalFloat("_AK_Basah",Mathf.MoveTowards(basah,basahTarget,Time.deltaTime/20f));
            _sisa-=Time.deltaTime;if(_sisa>0f)return;_sisa=Jeda;
            if(_kamera==null)_kamera=Camera.main;
            Terapkan(_kamera,jam!=null?jam.Jam:9f,_cuaca!=null?_cuaca.Cuaca:WeatherState.Cerah);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetBasah() { Shader.SetGlobalFloat("_AK_Basah",0f); }
    }
}
