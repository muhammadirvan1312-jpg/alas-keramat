using AlasKeramat.Core;
using UnityEngine;

namespace AlasKeramat.AI
{
    // Lembar catatan Kang Deden di dekat api: "BACA CATATAN DEDEN" -> teks tampil sebagai subtitle panjang & tercatat sebagai terbaca.
    public class CatatanDeden : MonoBehaviour, IInteractable
    {
        public static readonly System.Collections.Generic.List<int> Terbaca = new System.Collections.Generic.List<int>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Reset() { Terbaca.Clear(); }

        int _indeks;

        public static void Taruh(Vector3 p, int indeks, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "CatatanDeden_" + indeks;
            Destroy(go.GetComponent<Collider>());
            go.transform.position = new Vector3(p.x, WorldTerrainGenerator.SampleHeight(p.x, p.z) + 0.03f, p.z);
            go.transform.localScale = new Vector3(0.21f, 0.01f, 0.3f);
            go.transform.rotation = Quaternion.Euler(0f, Random.value * 360f, 0f);
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            var t = new GameObject("Titik"); t.transform.SetParent(go.transform, false); t.layer = GameLayers.Interactable;
            var sc = t.AddComponent<SphereCollider>(); sc.isTrigger = true; sc.radius = 3f;
            var c = go.AddComponent<CatatanDeden>(); c._indeks = indeks;
            GameEvents.RaiseToast("Ada catatan dari Kang Deden di dekat api");
        }

        public float HoldDuration { get { return 0f; } }
        public string GetPromptText(GameObject actor) { return "BACA CATATAN DEDEN"; }
        public bool CanInteract(GameObject actor) { return true; }

        public void Interact(GameObject actor)
        {
            if (_indeks >= 0 && _indeks < KangDedenKelvinAI.Catatan.Length)
            {
                GameEvents.RaiseSubtitle("Catatan Kang Deden: \"" + KangDedenKelvinAI.Catatan[_indeks].teks + "\"");
                if (!Terbaca.Contains(_indeks)) Terbaca.Add(_indeks);
            }
            Destroy(gameObject);
        }
    }
}
