using UnityEngine;
namespace AlasKeramat.UI
{
    public class RuangAmanKontrol : MonoBehaviour
    {
        Rect _lama; int _lebar,_tinggi;
        void OnEnable() { Terapkan(); }
        void Update() { if(_lebar!=Screen.width || _tinggi!=Screen.height || _lama!=Screen.safeArea)Terapkan(); }
        void Terapkan()
        {
            if(Screen.width<=0 || Screen.height<=0)return;
            _lebar=Screen.width;_tinggi=Screen.height;_lama=Screen.safeArea;
            var rt=transform as RectTransform;if(rt==null)return;
            rt.anchorMin=new Vector2(_lama.xMin/_lebar,_lama.yMin/_tinggi);
            rt.anchorMax=new Vector2(_lama.xMax/_lebar,_lama.yMax/_tinggi);
            rt.offsetMin=rt.offsetMax=Vector2.zero;
        }
    }
}
