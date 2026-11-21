#!/usr/bin/env python3
"""Cache hasil panggang Blender, berdasarkan hash sumber dan skrip."""
import argparse, hashlib, json, shutil, subprocess, sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/ThirdParty/Prepared';STAMP=OUT/'versi.sha256'
SOURCES=['Tools/aset_cc0.lock.json','Tools/karakter.lock.json','Tools/karakter/resep.json','Tools/karakter/buat_karakter.py','Tools/visual/panggang_pohon.py','Tools/visual/pasang_blender.py']
EXPECTED=['Andi','Deden','Ningsih','KiRogo','Pocong','Kuntilanak','Genderuwo','SektePria','SekteWanita','SekteButo','Kuyang','Tuyul','HantuAir','Banaspati','PohonScan_LOD0','PohonScan_LOD1','PohonScanB_LOD0','PohonScanB_LOD1','PohonScanC_LOD0','PohonScanC_LOD1','PakisScan_LOD0','PakisScan_LOD1','RumputScanA','RumputScanB','KeladiScan','BatuBesarScan','BatuLumutA','BatuLumutB','KayuTumbangScan','AkarScan','TunggulScan','RantingScanA','RantingScanB','BatuLumutC','BatuLumutD','SemakScanA_LOD0','SemakScanA_LOD1','SemakScanB_LOD0','SemakScanB_LOD1','PohonScanD_LOD0','PohonScanD_LOD1','PetiKayuScan','DrumScan','BakulScan','EmberKayuScan','PeriukScan','DingklikScan','JerikenScan','BanBekasScan','PanciScan','TanggaScan','BangkuScan','KapakScan','LampuMinyakScan']
def digest():return hashlib.sha256(b''.join((ROOT/p).read_bytes() for p in SOURCES)).hexdigest()
def siap():return STAMP.exists() and STAMP.read_text()==digest() and all((OUT/(n+'.bytes')).exists() for n in EXPECTED)
def main():
    p=argparse.ArgumentParser();p.add_argument('--cek',action='store_true');args=p.parse_args()
    if args.cek:return 0 if siap() else 1
    if siap():print('[AK] Aset visual tersimpan di cache, panggang dilewati');return 0
    pinned=ROOT/'.tools/blender43/blender-4.3.2-linux-x64/blender'
    blender=str(pinned) if pinned.exists() else shutil.which('blender')
    if not blender:raise RuntimeError('Blender diperlukan untuk memanggang G3')
    for script in ['Tools/karakter/buat_karakter.py','Tools/visual/panggang_pohon.py','Tools/visual/pasang_blender.py']:
        subprocess.run([blender,'-b','-t','2','--python-exit-code','1','--python',str(ROOT/script)],cwd=ROOT,check=True)
    if not all((OUT/(n+'.bytes')).exists() for n in EXPECTED):raise RuntimeError('Hasil panggang visual tidak lengkap')
    STAMP.write_text(digest());print('[AK] Delapan mesh visual siap')
    return 0
if __name__=='__main__':sys.exit(main())
