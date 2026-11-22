using AlasKeramat.AI;
using AlasKeramat.Core;
using AlasKeramat.Player;
using UnityEngine;

namespace AlasKeramat.World
{
    // Kentongan bambu/kayu Nusantara yang bisa dipukul: gema 120 m menenangkan batin Andi,
    // mengusir hantu dekat, serta memancing/mengalihkan perhatian sekte ke titik suara.
    public class KentonganDesaInteraktif : MonoBehaviour, IInteractable
    {
        public const float JedaPukul = 12f;

        [SerializeField] string namaPos = "Pos Desa";
        [SerializeField] Transform badanKentongan;

        float _siapPada = -99f, _goyang;

        public float HoldDuration { get { return 0f; } }

        public void Pasang(string nama, Transform badan)
        {
            namaPos = string.IsNullOrEmpty(nama) ? "Pos Desa" : nama;
            badanKentongan = badan;
        }

        public string GetPromptText(GameObject actor)
        {
            return Time.time < _siapPada ? "Gema kentongan masih terdengar..." : "PUKUL KENTONGAN (" + namaPos + ")";
        }

        public bool CanInteract(GameObject actor) { return Time.time >= _siapPada; }

        public void Interact(GameObject actor)
        {
            if (Time.time < _siapPada) return;
            _siapPada = Time.time + JedaPukul;
            _goyang = 1f;

            var synth = GameServices.Get<ProceduralAudioSynth>();
            if (synth != null) synth.Play(ProceduralAudioSynth.KlipKentongan, transform.position, AudioCategory.Efek, 1f, 0.96f, 2, 120f);
            MeterSuara.Pancar(transform.position, 95f);

            var vital = GameServices.Get<AndiVitalsAndSanity>();
            if (vital != null) vital.UbahSanity(8f);

            // Hantu dalam radius 24 m mundur saat kentongan berbunyi nyaring
            var hantus = Object.FindObjectsByType<HantuAI>(FindObjectsSortMode.None);
            int usir = 0;
            for (int i = 0; i < hantus.Length; i++)
            {
                if (hantus[i] == null || !hantus[i].gameObject.activeInHierarchy) continue;
                Vector3 d = hantus[i].transform.position - transform.position;
                if (d.sqrMagnitude <= 24f * 24f)
                {
                    hantus[i].transform.position += (d.sqrMagnitude > 0.1f ? d.normalized : Vector3.forward) * 14f;
                    usir++;
                }
            }
            GameEvents.RaiseToast(usir > 0
                ? "Kentongan " + namaPos + " menggema! " + usir + " sosok gaib mundur menjauh!"
                : "Kentongan " + namaPos + " menggema di seluruh lembah!");
        }

        void Update()
        {
            if (_goyang <= 0.001f || badanKentongan == null || JedaDunia.Dijeda) return;
            _goyang = Mathf.MoveTowards(_goyang, 0f, Time.deltaTime * 1.8f);
            badanKentongan.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(_goyang * 26f) * _goyang * 14f);
        }
    }
}
