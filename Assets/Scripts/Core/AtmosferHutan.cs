using UnityEngine;
namespace AlasKeramat.Core
{
    // Lapisan kabut lokal mengikuti kamera dengan pembaruan jarang; jumlah kartu tetap dan tanpa alokasi frame.
    public class AtmosferHutan : MonoBehaviour
    {
        public const int JumlahKartu=12;
        public const float Jeda=0.35f;
        [SerializeField] Transform[] kartu=new Transform[0];
        [SerializeField] Renderer[] gambar=new Renderer[0];
        Camera _kamera;
        URPAtmosphereAndWeather _cuaca;
        MaterialPropertyBlock _blok;
        float _sisa;
        public void Pasang(Transform[] t,Renderer[] r) { kartu=t;gambar=r; }
        void Awake() { GameServices.Register(this);_blok=new MaterialPropertyBlock(); }
        public static float BasahTarget(WeatherState w)
        { return w==WeatherState.HujanBadai?1f:w==WeatherState.Gerimis?0.55f:0f; }
        public void Terapkan(Camera cam,float jam,WeatherState w)
        {
            if(cam==null)return;
            if(_blok==null)_blok=new MaterialPropertyBlock();
            Vector3 p=cam.transform.position;
            bool aktif=!WorldTerrainGenerator.DiBawahTanah(p.y) && p.y<WorldTerrainGenerator.SampleHeight(p.x,p.z)+18f;
            float malam=URPAtmosphereAndWeather.Malam01(jam);
            bool maghrib=jam>=17.4f && jam<19.2f;
            float tebal=w==WeatherState.KabutPagi?0.16f:w==WeatherState.HujanBadai?0.11f:maghrib?0.10f:malam>0.5f?0.095f:0.075f;
            Color warna=maghrib?new Color(0.68f,0.34f,0.22f):Color.Lerp(new Color(0.54f,0.62f,0.58f),new Color(0.14f,0.22f,0.29f),malam);warna.a=tebal;
            _blok.SetColor("_BaseColor",warna);
            float gridX=Mathf.Floor(p.x/16f)*16f,gridZ=Mathf.Floor(p.z/16f)*16f;
            for(int i=0;i<kartu.Length;i++)
            {
                if(kartu[i]==null)continue;
                kartu[i].gameObject.SetActive(aktif);if(!aktif)continue;
                float a=(i*137.5f)*Mathf.Deg2Rad,r=20f+i*4.5f;
                Vector3 q=new Vector3(gridX+Mathf.Cos(a)*r,0f,gridZ+Mathf.Sin(a)*r);
                q.y=WorldTerrainGenerator.SampleHeight(q.x,q.z)+2.2f;
                kartu[i].position=q;
                Vector3 lihat=cam.transform.position-q;lihat.y=0f;
                if(lihat.sqrMagnitude>0.01f)kartu[i].rotation=Quaternion.LookRotation(lihat);
                if(i<gambar.Length && gambar[i]!=null)gambar[i].SetPropertyBlock(_blok);
            }
        }
        void Update()
        {
            if(_cuaca==null)_cuaca=GameServices.Get<URPAtmosphereAndWeather>();
            float basah=Shader.GetGlobalFloat("_AK_Basah");
            Shader.SetGlobalFloat("_AK_Basah",Mathf.MoveTowards(basah,BasahTarget(_cuaca!=null?_cuaca.Cuaca:WeatherState.Cerah),Time.deltaTime/40f));
            _sisa-=Time.deltaTime;if(_sisa>0f)return;_sisa=Jeda;
            if(_kamera==null)_kamera=Camera.main;
            var jam=GameServices.Get<TimeOfDayClock>();
            Terapkan(_kamera,jam!=null?jam.Jam:9f,_cuaca!=null?_cuaca.Cuaca:WeatherState.Cerah);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetBasah() { Shader.SetGlobalFloat("_AK_Basah",0f); }
    }
}
