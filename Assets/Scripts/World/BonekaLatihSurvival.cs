using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.World
{
    // Boneka latih kayu interaktif di Pos Survival Awal: pemain bisa mencoba Kombo 3-Hit, parry,
    // & menaikkan Level Kekuatan langsung sejak menit pertama mendarat.
    public class BonekaLatihSurvival : MonoBehaviour, IDamageable
    {
        float _goyang;
        Vector3 _sumbuGoyang = Vector3.right;
        Quaternion _rotAsal;
        int _totalPukulan;

        public bool IsAlive { get { return true; } }
        public int TotalPukulan { get { return _totalPukulan; } }

        void Awake() { _rotAsal = transform.localRotation; }

        public void TakeDamage(DamageInfo d)
        {
            if (d.amount <= 0f) return;
            _totalPukulan++;
            _goyang = Mathf.Min(1f, _goyang + (d.isHeavy ? 0.9f : 0.55f));
            Vector3 arah = d.direction; arah.y = 0f;
            _sumbuGoyang = arah.sqrMagnitude > 0.01f ? Vector3.Cross(Vector3.up, arah.normalized) : Vector3.right;

            var gore = GameServices.Get<GoreSystem>();
            if (gore != null) gore.PercikPartikel(d.point, new Color(0.62f, 0.44f, 0.24f, 0.9f), d.isHeavy ? 10 : 5, 0.06f, 2.2f, 0.5f);
            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.Play(ProceduralAudioSynth.KlipTebang, transform.position, AudioCategory.Efek, 0.72f, d.isHeavy ? 0.85f : 1.05f, 1, 24f);

            var kombo = GameServices.Get<SistemKomboDanStagger>();
            int step = kombo != null ? kombo.LangkahKombo : 1;
            string tag = step >= 3 ? " [FINISHER x3!]" : (step == 2 ? " [Kombo x2]" : "");
            GameEvents.RaiseToast("Latihan: " + Mathf.RoundToInt(d.amount) + " DMG" + tag);
        }

        void Update()
        {
            if (_goyang <= 0.001f || JedaDunia.Dijeda) return;
            _goyang = Mathf.MoveTowards(_goyang, 0f, Time.deltaTime * 2.4f);
            float sudut = Mathf.Sin(_goyang * 22f) * _goyang * 18f;
            transform.localRotation = _rotAsal * Quaternion.AngleAxis(sudut, _sumbuGoyang);
        }
    }
}
