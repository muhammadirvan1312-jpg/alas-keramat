using System;
using System.Collections.Generic;
using UnityEngine;

namespace AlasKeramat.Core
{
    // Registri layanan. Dilarang Find* di Update: pakai GameServices.Get<T>().
    public static class GameServices
    {
        static readonly Dictionary<Type, object> _layanan = new Dictionary<Type, object>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatis() { Clear(); }

        public static void Register<T>(T s) where T : class
        {
            if (s == null) { Debug.LogWarning("[AK][GameServices] Mendaftarkan layanan kosong untuk " + typeof(T).Name); return; }
            _layanan[typeof(T)] = s;
        }

        public static T Get<T>() where T : class
        {
            object o;
            if (_layanan.TryGetValue(typeof(T), out o))
            {
                var uo = o as UnityEngine.Object;
                if (!(o is UnityEngine.Object) || uo != null) return (T)o;
                _layanan.Remove(typeof(T));
            }
            if (typeof(MonoBehaviour).IsAssignableFrom(typeof(T)))
            {
                var found = UnityEngine.Object.FindFirstObjectByType(typeof(T)) as T;
                if (found != null) _layanan[typeof(T)] = found;
                return found;
            }
            return null;
        }

        public static void Clear() { _layanan.Clear(); }
    }
}
