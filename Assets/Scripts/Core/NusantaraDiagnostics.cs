using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AlasKeramat.Core
{
    // Menyalin Warning/Error/Exception ke Logs/nusantara_runtime.log (Editor) atau 50 entri terakhir (build).
    public class NusantaraDiagnostics : MonoBehaviour
    {
        public const int MaksEntri = 50;
        const string PathLog = "Logs/nusantara_runtime.log";
        readonly Queue<string> _terakhir = new Queue<string>(MaksEntri);

        public IEnumerable<string> EntriTerakhir { get { return _terakhir; } }

        void Awake() { GameServices.Register(this); }
        void OnEnable() { Application.logMessageReceived += Tangkap; }
        void OnDisable() { Application.logMessageReceived -= Tangkap; }

        void Tangkap(string pesan, string jejak, LogType tipe)
        {
            if (tipe == LogType.Log) return;
            string baris = "[" + tipe + "] " + pesan;
            if (_terakhir.Count >= MaksEntri) _terakhir.Dequeue();
            _terakhir.Enqueue(baris);
#if UNITY_EDITOR
            try
            {
                Directory.CreateDirectory("Logs");
                File.AppendAllText(PathLog, baris + (tipe == LogType.Warning ? "" : "\n" + jejak) + "\n");
            }
            catch (IOException) { }
#endif
        }
    }
}
