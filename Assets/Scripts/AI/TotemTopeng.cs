using System.Collections.Generic;
using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // 06 §7: 6 Totem Topeng (3 Desa, 3 Sawit). Dihancurkan (HP 60) = Kemarahan Sekte +5; status tersimpan di EnemyDirector.
    public class TotemTopeng : MonoBehaviour, IDamageable
    {
        public const float HPAwal = 60f;
        public static readonly List<TotemTopeng> Semua = new List<TotemTopeng>();

        [SerializeField] int indeks;
        float _hp = HPAwal;
        bool _hancur;

        public int Indeks { get { return indeks; } }
        public bool IsAlive { get { return !_hancur; } }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatis() { Semua.Clear(); }

        public void Pasang(int i) { indeks = i; }

        void OnEnable() { if (!Semua.Contains(this)) Semua.Add(this); }
        void OnDisable() { Semua.Remove(this); }

        void Start()
        {
            var dir = GameServices.Get<EnemyDirector>();
            if (dir != null && dir.TotemHancur(indeks)) Sinkron(true);
        }

        public void TakeDamage(DamageInfo info)
        {
            if (_hancur || info.amount <= 0f) return;
            _hp -= info.amount;
            if (_hp > 0f) return;
            Sinkron(true);
            var dir = GameServices.Get<EnemyDirector>();
            if (dir != null) dir.LaporTotem(this);
            MeterSuara.Pancar(transform.position, 30f);
        }

        public void Sinkron(bool hancur)
        {
            _hancur = hancur;
            if (!hancur) _hp = HPAwal;
            foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = !hancur;
            foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = !hancur;
        }
    }
}
