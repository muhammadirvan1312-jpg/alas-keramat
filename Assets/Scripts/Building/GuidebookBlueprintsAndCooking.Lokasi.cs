using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;
namespace AlasKeramat.Building
{
    public partial class GuidebookBlueprintsAndCooking
    {
        float _bentang=6f;
        bool Khusus(RaycastHit hit, ref Vector3 posisi, ref Quaternion rotasi, bool sah)
        {
            if(_pilih.id==BlueprintID.PerangkapPunji)
            {
                var inv=GameServices.Get<TikarPandan3DInventory>(); return sah && inv!=null && inv.Punya(ItemID.Cangkul);
            }
            if(_pilih.id==BlueprintID.RumahPohon)
            {
                var pohon=hit.collider.GetComponentInParent<DirectionalTree>();
                if(pohon==null || (pohon.Info.jenis!=ProcPohon.Jenis.Jati && pohon.Info.jenis!=ProcPohon.Jenis.Mahoni))return false;
                var vm=GameServices.Get<VegetationManager>(); if(vm==null || vm.Tersembunyi(pohon.Info.id))return false;
                posisi=pohon.Info.posisi+rotasi*Vector3.forward*1.8f; return true;
            }
            if(_pilih.id==BlueprintID.JembatanGantung)
            {
                var awal=hit.collider.GetComponentInParent<BagianStruktur>();
                if(awal==null || !awal.BisaDisimpan)return false;
                Vector3 a=awal.Puncak; BagianStruktur akhir=null; float skor=16f;
                foreach(var s in BagianStruktur.Semua)
                {
                    if(s==awal || !s.BisaDisimpan || (s.Jenis!=BagianStruktur.Tipe.Tiang && s.Jenis!=BagianStruktur.Tipe.Balok))continue;
                    Vector3 d=s.Puncak-a; float n=d.magnitude;
                    if(n<2f || n>15f || Mathf.Abs(d.y)>n*0.5f || Vector3.Dot(d.normalized,rotasi*Vector3.forward)<0.5f)continue;
                    if(n<skor){skor=n; akhir=s;}
                }
                if(akhir==null)return false;
                posisi=a; _bentang=skor; rotasi=Quaternion.LookRotation(akhir.Puncak-a,Vector3.up); return true;
            }
            return sah;
        }
    }
}
