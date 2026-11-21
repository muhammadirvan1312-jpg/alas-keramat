using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AlasKeramat.Core
{
    // Inisialisasi paling awal: layanan, matriks tabrakan, frame rate, EventSystem.
    [DefaultExecutionOrder(-1000)]
    public class GameBootstrap : MonoBehaviour
    {
        public const int TargetFpsDefault = 30;
        public const string PrefFps = "ak_target_fps";
        bool _kehilanganFokus, _latar;

        void Update() { JedaDunia.Tick(); }
        void OnApplicationFocus(bool fokus)
        {
#if UNITY_EDITOR
            if (Application.isBatchMode) return;
#endif
            _kehilanganFokus = !fokus; JedaDunia.Atur(GetInstanceID(), _kehilanganFokus || _latar);
        }
        void OnApplicationPause(bool jeda) { _latar = jeda; JedaDunia.Atur(GetInstanceID(), _kehilanganFokus || _latar); }
        void OnDisable() { JedaDunia.Atur(GetInstanceID(), false); }

        void Awake()
        {
            try
            {
                GameServices.Register(this);
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = PlayerPrefs.GetInt(PrefFps, TargetFpsDefault);
                ApplyCollisionMatrix();
                var synth = GetComponentInChildren<ProceduralAudioSynth>(true);
                if (synth != null) GameServices.Register(synth);
                if (FindFirstObjectByType<EventSystem>() == null)
                {
                    var es = new GameObject("EventSystem", typeof(EventSystem));
                    es.GetComponent<EventSystem>().pixelDragThreshold = 4;
                    AddCorrectInputModule(es);
                    Debug.LogWarning("[AK][GameBootstrap] EventSystem tidak ada di scene, dibuat ulang saat runtime.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[AK][GameBootstrap] Inisialisasi gagal: " + e);
            }
        }

        // Matriks tabrakan (docs/tech/11 bagian 5.2). Dipanggil Builder dan saat runtime.
        public static void ApplyCollisionMatrix()
        {
            for (int i = 0; i < 32; i++)
            {
                Physics.IgnoreLayerCollision(GameLayers.BuildGhost, i, true);
                Physics.IgnoreLayerCollision(GameLayers.InventoryStage, i, true);
            }
            Physics.IgnoreLayerCollision(GameLayers.Projectile, GameLayers.Projectile, true);
            Physics.IgnoreLayerCollision(GameLayers.Projectile, GameLayers.Foliage, true);
            Physics.IgnoreLayerCollision(GameLayers.Foliage, GameLayers.Foliage, true);
            Physics.IgnoreLayerCollision(GameLayers.Foliage, GameLayers.Ground, true);
            Physics.IgnoreLayerCollision(GameLayers.Foliage, GameLayers.Structure, true);
            Physics.IgnoreLayerCollision(GameLayers.TriggerZone, GameLayers.TriggerZone, true);
        }

        // H3: modul input UI yang benar tanpa referensi compile-time ke Input System.
        public static void AddCorrectInputModule(GameObject esGo)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var t = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (t != null)
            {
                var c = esGo.AddComponent(t);
                var m = t.GetMethod("AssignDefaultActions");
                if (m != null) m.Invoke(c, null);
                return;
            }
            Debug.LogError("[AK][Input] Proyek memakai Input System (New) tetapi modul UI tidak ditemukan.");
#else
            esGo.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
