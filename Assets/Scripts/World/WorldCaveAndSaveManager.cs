using System.Collections;
using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.World
{
    // T10a: transisi, batas air lokal, cahaya & status kompleks. Posisi Andi tetap milik save pemain.
    public class WorldCaveAndSaveManager : MonoBehaviour, ISaveable
    {
        public const float DurasiFade = 1.2f;
        [SerializeField] Image tirai;
        [SerializeField] Light[] lampu = new Light[0];
        [SerializeField] Bounds airGua;
        public UndergroundId Aktif { get; private set; }
        public bool Transisi { get; private set; }
        public Vector3 TitikPulang { get; private set; }
        public string SaveKey { get { return "gua"; } }
        [System.Serializable] class SimpanKompleksT10 { public Vector3 pulang; }
        AndiFPSController _andi;
        Camera _kamera;
        float _far;
        CameraClearFlags _clear;
        Color _background;
        readonly int[] _dekat = new int[4];
        public void Pasang(Image fade, Light[] lights, Bounds air) { tirai=fade; lampu=lights; airGua=air; }
        void Awake() { GameServices.Register(this); }
        public static UndergroundId KompleksPada(Vector3 p)
        {
            if(p.y > -40f) return UndergroundId.None;
            if(p.x < -300f) return UndergroundId.UG1_GuaSungai;
            return p.x < 300f ? UndergroundId.UG2_Bunker : UndergroundId.UG3_KuilAkar;
        }
        public float? AirPada(Vector3 p) { return airGua.Contains(p) ? (float?)airGua.max.y : null; }
        public string CaptureJson() { return JsonUtility.ToJson(new SimpanKompleksT10 { pulang=TitikPulang }); }
        public void RestoreJson(string json)
        {
            var d=JsonUtility.FromJson<SimpanKompleksT10>(json); if(d==null)return;
            BatalkanTransisi(); TitikPulang=d.pulang;
        }
        public void Pindah(AndiFPSController andi, Vector3 tujuan, bool masuk)
        {
            if(Transisi || andi==null)return;
            if(masuk) TitikPulang=andi.transform.position;
            StartCoroutine(Lintas(andi,tujuan));
        }
        IEnumerator Lintas(AndiFPSController andi, Vector3 tujuan)
        {
            Transisi=true; _andi=andi; andi.GerakDikunci=true;
            yield return Fade(0f,1f);
            andi.Teleport(tujuan); Sinkron(andi);
            yield return Fade(1f,0f);
            andi.GerakDikunci=false; Transisi=false;
        }
        IEnumerator Fade(float awal,float akhir)
        {
            float t=0f;
            if(tirai!=null) { tirai.enabled=true; tirai.transform.SetAsLastSibling(); }
            while(t<DurasiFade)
            {
                t+=Time.unscaledDeltaTime;
                if(tirai!=null)tirai.color=new Color(0f,0f,0f,Mathf.Lerp(awal,akhir,t/DurasiFade));
                yield return null;
            }
            if(tirai!=null && akhir==0f)tirai.enabled=false;
        }
        public void BatalkanTransisi()
        {
            StopAllCoroutines(); Transisi=false;
            if(_andi!=null)_andi.GerakDikunci=false;
            if(tirai!=null)tirai.enabled=false;
        }
        void OnDisable() { BatalkanTransisi(); }
        void LateUpdate()
        {
            if(_andi==null)_andi=GameServices.Get<AndiFPSController>();
            if(_andi==null)return;
            Sinkron(_andi);
            // Empat lampu terdekat; tanpa sorting/alokasi per frame.
            for(int k=0;k<4;k++)_dekat[k]=-1;
            for(int i=0;i<lampu.Length;i++)
            {
                if(lampu[i]==null)continue;
                lampu[i].enabled=false;
                if(Aktif==UndergroundId.None || KompleksPada(lampu[i].transform.position)!=Aktif)continue;
                float d=(lampu[i].transform.position-_andi.transform.position).sqrMagnitude;
                for(int k=0;k<4;k++)if(_dekat[k]<0 || d<(lampu[_dekat[k]].transform.position-_andi.transform.position).sqrMagnitude)
                { for(int j=3;j>k;j--)_dekat[j]=_dekat[j-1]; _dekat[k]=i; break; }
            }
            for(int k=0;k<4;k++)if(_dekat[k]>=0)lampu[_dekat[k]].enabled=true;
        }
        public void Sinkron(AndiFPSController andi)
        {
            var baru=KompleksPada(andi.transform.position);
            if(baru==Aktif)return;
            Aktif=baru;
            if(_kamera==null && andi.Kamera!=null)_kamera=andi.Kamera.GetComponent<Camera>();
            var atmosfer=GameServices.Get<URPAtmosphereAndWeather>();
            if(atmosfer!=null)atmosfer.SetDalamRuangan(baru!=UndergroundId.None);
            if(baru!=UndergroundId.None)
            {
                if(_kamera!=null) { _far=_kamera.farClipPlane; _clear=_kamera.clearFlags; _background=_kamera.backgroundColor; _kamera.farClipPlane=80f; _kamera.clearFlags=CameraClearFlags.SolidColor; _kamera.backgroundColor=Color.black; }
                GameEvents.RaiseEnterUnderground(baru);
            }
            else
            {
                if(_kamera!=null) { _kamera.farClipPlane=_far; _kamera.clearFlags=_clear; _kamera.backgroundColor=_background; }
                GameEvents.RaiseExitUnderground();
            }
        }
    }
}
