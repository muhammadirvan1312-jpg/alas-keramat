using System.Collections.Generic;
using AlasKeramat.Core;
using AlasKeramat.Inventory;
using UnityEngine;

namespace AlasKeramat.Player
{
    // 05 §3.7: zirah 10 segmen. Damage diserap segmen terluar 100% sampai durabilitasnya habis; sisa ke HP.
    // Daun rusak 2x saat hujan, bambu lemah api (x1,5), tulang kewarasan -1 saat dipakai pertama. Jatuh, tenggelam & gaib menembus zirah.
    public partial class AndiVitalsAndSanity
    {
        public const int MaksSegmen = 10;
        public struct Segmen { public ItemID jenis; public float dur, durMaks; }
        readonly List<Segmen> _zirah = new List<Segmen>();
        bool _tulangPernah;
        public bool DekatApi { get { return _dekatApi || Building.ApiUnggun.AdaApiDekat(transform.position, Building.ApiUnggun.JarakHangat); } }

        public int JumlahSegmen { get { return _zirah.Count; } }
        public Segmen AmbilSegmen(int i) { return _zirah[i]; }

        public static float Durabilitas(ItemID id)
        {
            switch (id) { case ItemID.ZirahDaun: return 8f; case ItemID.ZirahBambu: return 15f; case ItemID.ZirahTulang: return 25f; case ItemID.ZirahBaja: return 40f; case ItemID.ZirahEmas: return 70f; default: return 0f; }
        }

        public bool PakaiZirah(ItemID id, bool ambilDariTas = true)
        {
            if (Durabilitas(id) <= 0f || _zirah.Count >= MaksSegmen) return false;
            if (ambilDariTas)
            {
                var inv = GameServices.Get<TikarPandan3DInventory>();
                if (inv == null || !inv.Kurangi(id, 1)) return false;
            }
            _zirah.Add(new Segmen { jenis = id, dur = Durabilitas(id), durMaks = Durabilitas(id) });
            if (id == ItemID.ZirahTulang && !_tulangPernah) { _tulangPernah = true; UbahSanity(-1f); }
            return true;
        }

        // Mengembalikan damage yang tersisa untuk HP.
        public float Serap(DamageInfo d)
        {
            if (d.type == DamageType.Jatuh || d.type == DamageType.Tenggelam || d.type == DamageType.Gaib) return d.amount;
            float sisa = d.amount;
            bool hujan = _atmos != null && (_atmos.Cuaca == WeatherState.Gerimis || _atmos.Cuaca == WeatherState.HujanBadai);
            while (sisa > 0f && _zirah.Count > 0)
            {
                int i = _zirah.Count - 1;
                var s = _zirah[i];
                float aus = (s.jenis == ItemID.ZirahBambu && d.type == DamageType.Api) ? 1.5f : (s.jenis == ItemID.ZirahDaun && hujan) ? 2f : 1f;
                float serap = Mathf.Min(sisa, s.dur / aus);
                s.dur -= serap * aus; sisa -= serap;
                if (s.dur <= 0.001f) _zirah.RemoveAt(i); else _zirah[i] = s;
            }
            return sisa;
        }
    }
}
