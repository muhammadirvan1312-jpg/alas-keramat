using System.Collections;
using AlasKeramat.Core;
using AlasKeramat.Player;
using AlasKeramat.UI;
using UnityEngine;
namespace AlasKeramat.AI
{
    // Helikopter SAR (04 §7.6-7.9): turun di Landasan SAR setelah siege; Andi dalam 8 m dari pintu -> Pilihan Takdir;
    // Ending B: heli terbang pergi dan pulau menjadi sandbox (tanpa Malam Darah).
    public class HeliSAR : MonoBehaviour
    {
        public const float JarakPintu = 8f, TinggiDatang = 45f, LamaTurun = 8f, LajuPergi = 14f, LamaPergi = 9f;
        [SerializeField] GameObject model;
        Vector3 _landas;
        bool _turun, _ditawarkan, _pergi;
        AndiFPSController _andi;

        public void Pasang(GameObject m) { model = m; }
        void Awake() { GameServices.Register(this); _landas = transform.position; if (model != null) model.SetActive(false); }

        public static bool BolehDatang(QuestManager.ProgresMisiT6 p) { return p.siegeSelesai && p.ending == 0; }

        void Update()
        {
            var q = GameServices.Get<QuestManager>(); if (q == null || model == null) return;
            if (q.Progres.ending == 2) EnemyDirector.TanpaMalamDarah = true;
            if (_pergi) return;
            bool datang = BolehDatang(q.Progres);
            if (datang != model.activeSelf) model.SetActive(datang);
            if (!datang) return;
            if (!_turun) { _turun = true; StartCoroutine(Turun()); }
            if (_ditawarkan) return;
            if (_andi == null) { _andi = GameServices.Get<AndiFPSController>(); if (_andi == null) return; }
            if ((_andi.transform.position - transform.position).sqrMagnitude <= JarakPintu * JarakPintu)
            {
                var s = GameServices.Get<LayarSinema>();
                if (s != null) { _ditawarkan = true; s.TampilPilihan(); }
            }
        }

        IEnumerator Turun()
        {
            var a = GameServices.Get<ProceduralAudioSynth>();
            if (a != null) a.Play(ProceduralAudioSynth.KlipPesawat, _landas, AudioCategory.Efek, 1f, 0.9f, 8);
            for (float t = 0f; t < LamaTurun; t += Time.deltaTime) { transform.position = _landas + Vector3.up * TinggiDatang * (1f - Mathf.SmoothStep(0f, 1f, t / LamaTurun)); yield return null; }
            transform.position = _landas;
        }

        public void Terbang() { if (!_pergi && model != null && model.activeSelf) StartCoroutine(Pergi()); }

        IEnumerator Pergi()
        {
            _pergi = true;
            Vector3 awal = transform.position;
            for (float t = 0f; t < LamaPergi; t += Time.deltaTime) { transform.position = awal + new Vector3(0f, t * LajuPergi * 0.5f, -t * LajuPergi); yield return null; }
            model.SetActive(false);
        }
    }
}
