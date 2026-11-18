using AlasKeramat.Core;
using UnityEngine;
namespace AlasKeramat.AI
{
    // T10: di bawah tanah musuh mengikuti WaypointGraph kompleks (H6), bukan grid permukaan.
    public partial class NusantaraEnemyAndMutantAI
    {
        float _tGua;
        void MenujuGua(Vector3 tujuan, float laju, float dt, float d)
        {
            var g = WaypointGraph.Untuk(transform.position);
            if (g == null || d < JarakLangsung) { Gerak(tujuan, laju, dt, true); return; }
            _tGua -= dt;
            if (_tGua <= 0f || Datar(tujuan - _tujuanJalur).sqrMagnitude > 25f)
            {
                _tGua = JedaJalur; _tujuanJalur = tujuan; _iJalur = 0;
                if (!g.Jalur(transform.position, tujuan, _jalur)) _jalur.Clear();
            }
            while (_iJalur < _jalur.Count && Datar(_jalur[_iJalur] - transform.position).magnitude < 1.6f) _iJalur++;
            Gerak(_iJalur < _jalur.Count ? _jalur[_iJalur] : tujuan, laju, dt, true);
        }
    }
}
