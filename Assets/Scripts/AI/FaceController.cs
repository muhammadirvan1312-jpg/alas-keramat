using AlasKeramat.Core;
using UnityEngine;
namespace AlasKeramat.AI
{
    public enum EkspresiWajahG3 { Netral,Takut,Ngeri,Marah,Sedih,Menangis,Lega,Senyum,Sakit,Lelah,Curiga,Teriak }
    public class FaceController : MonoBehaviour
    {
        [SerializeField] SkinnedMeshRenderer[] wajah=new SkinnedMeshRenderer[0];
        [SerializeField] string tokoh;
        readonly int[,] _index=new int[2,13];
        readonly float[] _bobot=new float[12];
        EkspresiWajahG3 _ekspresi;
        float _kuat=65f,_tKedip=3f,_kedip,_bicara;
        public void Pasang(SkinnedMeshRenderer[] renderers,string nama) { wajah=renderers;tokoh=nama; }
        public void SetEkspresi(EkspresiWajahG3 jenis,float intensitas=0.65f) { _ekspresi=jenis;_kuat=Mathf.Clamp01(intensitas)*100f; }
        void Awake()
        {
            for(int r=0;r<2;r++)for(int s=0;s<13;s++)_index[r,s]=-1;
            for(int r=0;r<wajah.Length && r<2;r++)
            {
                if(wajah[r]==null || wajah[r].sharedMesh==null)continue;
                var mesh=wajah[r].sharedMesh;
                for(int s=1;s<12;s++)_index[r,s-1]=mesh.GetBlendShapeIndex(((EkspresiWajahG3)s).ToString());
                _index[r,11]=mesh.GetBlendShapeIndex("Kedip");_index[r,12]=mesh.GetBlendShapeIndex("Bicara");
            }
        }
        void OnEnable() { GameEvents.OnSubtitle+=Subtitle; }
        void OnDisable() { GameEvents.OnSubtitle-=Subtitle; }
        void Subtitle(string teks)
        { if(!string.IsNullOrEmpty(teks) && !string.IsNullOrEmpty(tokoh) && teks.StartsWith(tokoh))_bicara=Mathf.Clamp(teks.Length/15f,1f,8f); }
        void LateUpdate()
        {
            float dt=Time.deltaTime;_tKedip-=dt;
            if(_tKedip<=0f) { _kedip=0.16f;_tKedip=Random.Range(2f,6f); }
            _kedip=Mathf.Max(0f,_kedip-dt);_bicara=Mathf.Max(0f,_bicara-dt);
            float blink=_kedip>0f?Mathf.Sin((1f-_kedip/0.16f)*Mathf.PI)*100f:0f;
            for(int i=0;i<11;i++)_bobot[i]=Mathf.MoveTowards(_bobot[i],(int)_ekspresi==i+1?_kuat:0f,dt*250f);
            for(int r=0;r<wajah.Length && r<2;r++)
            {
                if(wajah[r]==null)continue;
                for(int s=0;s<11;s++)if(_index[r,s]>=0)wajah[r].SetBlendShapeWeight(_index[r,s],_bobot[s]);
                if(_index[r,11]>=0)wajah[r].SetBlendShapeWeight(_index[r,11],blink);
                if(_index[r,12]>=0)wajah[r].SetBlendShapeWeight(_index[r,12],_bicara>0f?(0.5f+0.5f*Mathf.Sin(Time.time*23f))*85f:0f);
            }
        }
    }
}
