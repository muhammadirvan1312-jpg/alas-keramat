using System;
using UnityEngine;

namespace AlasKeramat.Building
{
    // Data vegetasi yang dipanggang Builder (posisi dikelompokkan per sel <= 1023 instance).
    public class VegetationData : ScriptableObject
    {
        [Serializable]
        public class JenisVeg
        {
            public string nama;
            public Mesh lod0, lod1;
            public Material kayu, daun;
            public float jarakLod1 = 120f, jarakMaks = 700f;
            public bool bayangan = true;
        }

        [Serializable]
        public class Sel
        {
            public int jenis;
            public Vector3 pusat;
            public float radius;
            public Matrix4x4[] matriks;
            public int[] idGlobal;
        }

        public JenisVeg[] jenis = new JenisVeg[0];
        public Sel[] sel = new Sel[0];

        public int HitungInstance(Func<int, bool> filter)
        {
            int n = 0;
            for (int i = 0; i < sel.Length; i++) if (filter(sel[i].jenis)) n += sel[i].matriks.Length;
            return n;
        }
    }
}
