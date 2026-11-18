# LAPORAN CI
- Commit: c60875b26ea92fdfa710bbb492f228d200d484a8
- Hasil aset visual: failure
- Hasil langkah Unity: skipped
- Waktu: 2026-11-18T06:06:51Z

Ringkasan dari NusantaraCI tidak ada (kemungkinan error kompilasi sebelum kode berjalan).

## Baris error yang ditemukan
  File "/usr/lib/blender/scripts/modules/bpy/ops.py", line 109, in __call__
    ret = _op_call(self.idname_py(), kw)
RuntimeError: Error: Python: Traceback (most recent call last):
  File "/usr/lib/blender/scripts/addons_core/io_scene_fbx/__init__.py", line 228, in execute
    if self.files:
       ^^^^^^^^^^
  File "/usr/lib/blender/scripts/modules/_bpy_types.py", line 1024, in __getattribute__
    return super().__getattribute__(attr)
           ~~~~~~~~~~~~~~~~~~~~~~~~^^^^^^
AttributeError: 'ImportFBX' object has no attribute 'files'
Location: /usr/lib/blender/scripts/modules/bpy/ops.py:109

Error: Python: Traceback (most recent call last):
  File "/usr/lib/blender/scripts/addons_core/io_scene_fbx/__init__.py", line 228, in execute
    if self.files:
       ^^^^^^^^^^
  File "/usr/lib/blender/scripts/modules/_bpy_types.py", line 1024, in __getattribute__
    return super().__getattribute__(attr)
           ~~~~~~~~~~~~~~~~~~~~~~~~^^^^^^
AttributeError: 'ImportFBX' object has no attribute 'files'
Location: /usr/lib/blender/scripts/modules/bpy/ops.py:109

Error: script failed, file: '/home/runner/work/alas-keramat/alas-keramat/Tools/visual/panggang_pohon.py', exiting.
Blender 5.0.1

Blender quit
Traceback (most recent call last):
  File "/home/runner/work/alas-keramat/alas-keramat/Tools/visual/siapkan.py", line 22, in <module>
    if __name__=='__main__':sys.exit(main())
                                     ^^^^^^
  File "/home/runner/work/alas-keramat/alas-keramat/Tools/visual/siapkan.py", line 18, in main
    subprocess.run([blender,'-b','-t','2','--python-exit-code','1','--python',str(ROOT/script)],cwd=ROOT,check=True)
  File "/opt/hostedtoolcache/Python/3.12.16/x64/lib/python3.12/subprocess.py", line 571, in run
    raise CalledProcessError(retcode, process.args,
subprocess.CalledProcessError: Command '['/usr/bin/blender', '-b', '-t', '2', '--python-exit-code', '1', '--python', '/home/runner/work/alas-keramat/alas-keramat/Tools/visual/panggang_pohon.py']' returned non-zero exit status 1.
