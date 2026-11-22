# LAPORAN CI
- Commit: b05bc16a0d66aed0a850c80fa2a71abd7ba67d87
- Hasil aset visual: success
- Hasil langkah Unity: failure
- Waktu: 2026-11-22T03:13:30Z

[LULUS] pengaturan pemain
[GAGAL] builder: BuildAll melaporkan langkah GAGAL (lihat Logs/nusantara_build_report.txt)
[GAGAL] self-test: RunAll melaporkan gagal
[LEWAT] foto: dijalankan di job CI terpisah (FotoSaja)
[LULUS] build APK

## Baris error yang ditemukan
[Error] [AK][Builder] Langkah 3c. Vegetasi gagal: UnityEngine.MissingReferenceException: The object of type 'AlasKeramat.Building.VegetationData' has been destroyed but you are still trying to access it.
  at UnityEngine.Object+MarshalledUnityObject.TryThrowEditorNullExceptionObject (UnityEngine.Object unityObj, System.String parameterName) [0x00064] in /home/bokken/build/output/unity/unity/Runtime/Export/Scripting/UnityEngineObject.bindings.cs:870 
  at UnityEngine.Bindings.ThrowHelper.ThrowArgumentNullException (System.Object obj, System.String parameterName) [0x00010] in /home/bokken/build/output/unity/unity/Runtime/Export/Scripting/BindingsHelpers.cs:53 
[Error] [AK][GAGAL] builder: System.Exception: BuildAll melaporkan langkah GAGAL (lihat Logs/nusantara_build_report.txt)
[Error] [AK][Builder] Langkah 3c. Vegetasi gagal: UnityEngine.MissingReferenceException: The object of type 'AlasKeramat.Building.VegetationData' has been destroyed but you are still trying to access it.
  at UnityEngine.Object+MarshalledUnityObject.TryThrowEditorNullExceptionObject (UnityEngine.Object unityObj, System.String parameterName) [0x00064] in /home/bokken/build/output/unity/unity/Runtime/Export/Scripting/UnityEngineObject.bindings.cs:870 
  at UnityEngine.Bindings.ThrowHelper.ThrowArgumentNullException (System.Object obj, System.String parameterName) [0x00010] in /home/bokken/build/output/unity/unity/Runtime/Export/Scripting/BindingsHelpers.cs:53 
[Error] [AK][Builder] Langkah 3c. Vegetasi gagal: UnityEngine.MissingReferenceException: The object of type 'AlasKeramat.Building.VegetationData' has been destroyed but you are still trying to access it.
  at UnityEngine.Object+MarshalledUnityObject.TryThrowEditorNullExceptionObject (UnityEngine.Object unityObj, System.String parameterName) [0x00064] in /home/bokken/build/output/unity/unity/Runtime/Export/Scripting/UnityEngineObject.bindings.cs:870 
  at UnityEngine.Bindings.ThrowHelper.ThrowArgumentNullException (System.Object obj, System.String parameterName) [0x00010] in /home/bokken/build/output/unity/unity/Runtime/Export/Scripting/BindingsHelpers.cs:53 
[Error] [AK][GAGAL] self-test: [GAGAL] Builder pertama tanpa langkah gagal
[Error] [AK][GAGAL] self-test: [GAGAL] Builder kedua tanpa langkah gagal
[Error] [AK][GAGAL] self-test: [GAGAL] Pohon 8.000-45.000 (0)
[Error] [AK][GAGAL] self-test: [GAGAL] Semak/pakis >= 10.000 (0)
[Error] [AK][GAGAL] self-test: [GAGAL] VegetationManager & data terpasang
[Error] [AK][GAGAL] self-test: [GAGAL] Laju vital 60 s sesuai 05 (haus -4.34, kenyang -3.30, istirahat -2.10)
[Error] [AK][GAGAL] self-test: [GAGAL] Laron: tiap lampu minyak punya laron (mati awal, <= 20 partikel)
[Error] [AK][GAGAL] self-test: [GAGAL] Prop desa: >= 10 lampu minyak, cahaya mati awal, tanpa bayangan, jangkauan <= 6 m (33)
[Error] [AK][GAGAL] self-test: [GAGAL] Ekspansi SOTF: biota air (ikan tangkap & kepiting pantai)
[Error] [AK][GAGAL] self-test: [GAGAL] Fitur Baru SOTF: 4 Sarang Pemangsa & 8 Jejak Lacak
[Error] [AK][GAGAL] self-test: [GAGAL] G3b: pohon & pakis pemindaian tersambung ke instancing dengan LOD
[Error] [AK][GAGAL] self-test: [GAGAL] Sentuh: jari kedua tidak merebut kamera
[Error] [AK][GAGAL] self-test: System.Exception: RunAll melaporkan gagal
