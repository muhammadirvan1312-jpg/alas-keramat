using System.Collections.Generic;
using UnityEngine;
namespace AlasKeramat.World
{
    // Simpul dipanggang dari lantai gua; BFS dibatasi jumlah node, tidak menyentuh grid permukaan.
    public class WaypointGraph : MonoBehaviour
    {
        [SerializeField] Vector3[] simpul=new Vector3[0];
        [SerializeField] Vector2Int[] sambungan=new Vector2Int[0];
        public int Jumlah { get { return simpul.Length; } }
        public Vector3[] Simpul { get { return simpul; } }
        public void Pasang(Vector3[] titik,Vector2Int[] sisi) { simpul=titik;sambungan=sisi; }
        public bool Cari(int awal,int akhir,List<Vector3> hasil)
        {
            hasil.Clear(); if(awal<0 || akhir<0 || awal>=simpul.Length || akhir>=simpul.Length)return false;
            var sebelumnya=new int[simpul.Length]; for(int i=0;i<sebelumnya.Length;i++)sebelumnya[i]=-1;
            var antre=new Queue<int>();antre.Enqueue(awal);sebelumnya[awal]=awal;
            while(antre.Count>0)
            {
                int u=antre.Dequeue();if(u==akhir)break;
                foreach(var e in sambungan)
                {
                    int v=e.x==u?e.y:e.y==u?e.x:-1;
                    if(v<0 || v>=simpul.Length || sebelumnya[v]>=0)continue;
                    sebelumnya[v]=u;antre.Enqueue(v);
                }
            }
            if(sebelumnya[akhir]<0)return false;
            for(int u=akhir;;u=sebelumnya[u]) { hasil.Add(simpul[u]);if(u==awal)break; }
            hasil.Reverse();return true;
        }
    }
}
