using AlasKeramat.Core;
using AlasKeramat.Inventory;
using AlasKeramat.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace AlasKeramat.UI
{
    public class PetaSurveiUI : MonoBehaviour, IPointerClickHandler
    {
        public GameObject Panel;
        public RectTransform Bidang, Andi, Tujuan;
        public Text Info, Kompas;
        public static readonly string[] NamaLokasi={"HELI","BERINGIN","DESA","GUA SUNGAI","GUA UTARA","BUNKER","PANTAI"};
        public static readonly Vector2[] Lokasi={new Vector2(0,-440),Vector2.zero,new Vector2(540,196),new Vector2(256,-276),new Vector2(0,694),new Vector2(-560,290),new Vector2(0,-840)};
        Vector2 _tujuan=Lokasi[0]; string _nama="HELI";
        AndiFPSController _pemain; TikarPandan3DInventory _tas;
        float _jeda,_waktu=1f; bool _terbuka;
        public static Vector2 KeUV(Vector2 dunia) { return (dunia+Vector2.one*WorldTerrainGenerator.BatasDunia)/(WorldTerrainGenerator.BatasDunia*2f); }
        public static Vector2 KeDunia(Vector2 uv) { return uv*(WorldTerrainGenerator.BatasDunia*2f)-Vector2.one*WorldTerrainGenerator.BatasDunia; }
        public static float Bearing(Vector2 delta) { return Mathf.Repeat(Mathf.Atan2(delta.x,delta.y)*Mathf.Rad2Deg,360f); }
        void Update()
        {
            _jeda-=Time.unscaledDeltaTime;if(_jeda>0f)return;_jeda=0.2f;
            if(_pemain==null)_pemain=GameServices.Get<AndiFPSController>();
            if(_tas==null)_tas=GameServices.Get<TikarPandan3DInventory>();
            if(_pemain==null)return;
            Vector3 pos=_pemain.transform.position;float yaw=_pemain.transform.eulerAngles.y;
            bool gps=_tas!=null && _tas.Punya(ItemID.GPSHandset);
            Vector2 delta=_tujuan-new Vector2(pos.x,pos.z);
            if(Kompas!=null)Kompas.text=gps?"ARAH "+yaw.ToString("000")+"  |  "+_nama+" "+delta.magnitude.ToString("0")+" m  |  TUJU "+Bearing(delta).ToString("000"):"ARAH "+yaw.ToString("000")+"  |  CARI GPS & PETA DI KOPER DARURAT";
            if(!_terbuka)return;
            if(Info!=null)Info.text="PETA SURVEI - UTARA DI ATAS | "+(gps?"MERAH: ANDI | KUNING: TUJUAN":"GPS BELUM DITEMUKAN")+"\nKetuk peta untuk menandai tujuan. Jarak lurus; perhatikan sungai dan tebing.";
            if(Andi!=null)
            {
                Andi.gameObject.SetActive(gps);
                Andi.anchorMin=Andi.anchorMax=KeUV(new Vector2(pos.x,pos.z));Andi.anchoredPosition=Vector2.zero;
                Andi.localRotation=Quaternion.Euler(0,0,-yaw);
            }
        }
        public void Toggle()
        {
            if(_terbuka){Tutup();return;}
            if(_tas==null)_tas=GameServices.Get<TikarPandan3DInventory>();
            if(_tas==null || !_tas.Punya(ItemID.PetaKertas)) { GameEvents.RaiseToast("PETA DAN GPS ADA DI KOPER DARURAT DEKAT HELI");return; }
            if(Panel==null)return;
            _terbuka=true;_waktu=Time.timeScale;Time.timeScale=0f;
            var kontrol=GameServices.Get<TouchControls>();if(kontrol!=null)kontrol.HentikanInput();
            Panel.SetActive(true);_jeda=0f;
        }
        public void Tutup() { if(!_terbuka)return;_terbuka=false;if(Panel!=null)Panel.SetActive(false);Time.timeScale=_waktu; }
        void OnDisable() { Tutup(); }
        public void OnPointerClick(PointerEventData e)
        {
            if(!_terbuka || Bidang==null)return;
            Vector2 p;if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(Bidang,e.position,e.pressEventCamera,out p) || !Bidang.rect.Contains(p))return;
            Vector2 uv=new Vector2((p.x-Bidang.rect.xMin)/Bidang.rect.width,(p.y-Bidang.rect.yMin)/Bidang.rect.height);
            _tujuan=KeDunia(uv);_nama="TANDA PETA";
            for(int i=0;i<Lokasi.Length;i++)if(Vector2.Distance(_tujuan,Lokasi[i])<60f){_tujuan=Lokasi[i];_nama=NamaLokasi[i];break;}
            if(Tujuan!=null){Tujuan.anchorMin=Tujuan.anchorMax=KeUV(_tujuan);Tujuan.anchoredPosition=Vector2.zero;}
            _jeda=0f;
        }
    }
}
