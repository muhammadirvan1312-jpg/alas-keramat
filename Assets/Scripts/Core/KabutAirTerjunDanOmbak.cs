using UnityEngine;

namespace AlasKeramat.Core
{
    // Efek visual atmosfer hidup: kabut semburan air terjun (zero-GC, culling 75 m), ombak menyapu garis pantai,
    // serta ayunan lembut lentera gantung mengikuti angin.
    public class KabutAirTerjunDanOmbak : MonoBehaviour
    {
        public const float JarakAktif = 75f;

        [SerializeField] Vector3 posisiAirTerjun;
        [SerializeField] Transform[] lenteraAyun = new Transform[0];
        [SerializeField] Transform[] ombakPantai = new Transform[0];

        Transform _cam;
        float _tKabut;
        Vector3[] _posAwalOmbak;
        GoreSystem _gore;

        public int JumlahLentera { get { return lenteraAyun != null ? lenteraAyun.Length : 0; } }
        public int JumlahOmbak { get { return ombakPantai != null ? ombakPantai.Length : 0; } }

        public void Pasang(Vector3 posAirTerjun, Transform[] lentera, Transform[] ombak)
        {
            posisiAirTerjun = posAirTerjun;
            lenteraAyun = lentera ?? new Transform[0];
            ombakPantai = ombak ?? new Transform[0];
        }

        public static float SudutAyunLentera(float waktu, float fase, float angin)
        {
            return Mathf.Sin(waktu * 1.7f + fase) * (5.5f + Mathf.Clamp01(angin) * 9.5f);
        }

        public static float GeserOmbak(float waktu, float fase)
        {
            return Mathf.Sin(waktu * 0.85f + fase) * 2.4f;
        }

        void Awake()
        {
            if (ombakPantai != null && ombakPantai.Length > 0)
            {
                _posAwalOmbak = new Vector3[ombakPantai.Length];
                for (int i = 0; i < ombakPantai.Length; i++)
                    if (ombakPantai[i] != null) _posAwalOmbak[i] = ombakPantai[i].position;
            }
        }

        void Update()
        {
            if (JedaDunia.Dijeda) return;
            if (_cam == null) { var c = Camera.main; if (c != null) _cam = c.transform; }
            if (_cam == null) return;
            Vector3 cp = _cam.position;
            float t = Time.time;
            float angin = Shader.GetGlobalFloat("_AK_AnginKuat");

            if (lenteraAyun != null)
            {
                for (int i = 0; i < lenteraAyun.Length; i++)
                {
                    var l = lenteraAyun[i];
                    if (l == null) continue;
                    if ((l.position - cp).sqrMagnitude > JarakAktif * JarakAktif) continue;
                    l.localRotation = Quaternion.Euler(0f, 0f, SudutAyunLentera(t, i * 1.3f, angin));
                }
            }

            if (ombakPantai != null && _posAwalOmbak != null)
            {
                for (int i = 0; i < ombakPantai.Length; i++)
                {
                    var o = ombakPantai[i];
                    if (o == null) continue;
                    if ((_posAwalOmbak[i] - cp).sqrMagnitude > 140f * 140f) continue;
                    float g = GeserOmbak(t, i * 0.9f);
                    o.position = _posAwalOmbak[i] + new Vector3(0f, 0f, g);
                }
            }

            if ((posisiAirTerjun - cp).sqrMagnitude <= JarakAktif * JarakAktif)
            {
                _tKabut -= Time.deltaTime;
                if (_tKabut <= 0f)
                {
                    _tKabut = 0.22f;
                    if (_gore == null) _gore = GameServices.Get<GoreSystem>();
                    if (_gore != null)
                        _gore.PercikPartikel(posisiAirTerjun + new Vector3(Random.Range(-2.5f, 2.5f), 0.5f, Random.Range(-1.5f, 1.5f)),
                            new Color(0.84f, 0.93f, 0.97f, 0.52f), 5, 0.55f, 1.6f, 1.3f);
                }
            }
        }
    }
}
