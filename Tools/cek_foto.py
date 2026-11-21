#!/usr/bin/env python3
"""Periksa bukti render, bukan memberi skor estetika. Hanya dipakai alat CI."""
import argparse, hashlib, json
from pathlib import Path
from PIL import Image, ImageStat
EXPECTED=('g3_01_deden_wajah','g3_02_ningsih_wajah','g3_03_hutan_malam','g3_04_lantai_hutan','t10_01_gua_lumut','t10_02_kubah_akar_boss')

def periksa_satu(folder,name,hashes,errors,rows):
    path=folder/(name+'.png')
    if not path.exists():errors.append('Foto belum tersedia: '+name);return
    try:
        digest=hashlib.sha256(path.read_bytes()).hexdigest()
        if digest in hashes:errors.append('Foto identik dengan shot lain: '+name)
        hashes.add(digest)
        with Image.open(path) as src:
            size=src.size
            if src.format!='PNG' or min(size)<480:errors.append('Format/resolusi foto tidak cukup: '+name)
            image=src.convert('RGB');image.thumbnail((240,135))
        pixels=list(image.getdata());lum=sorted((.2126*r+.7152*g+.0722*b)/255 for r,g,b in pixels)
        p95=lum[int((len(lum)-1)*.95)];spread=ImageStat.Stat(image.convert('L')).stddev[0]/255
        pink=sum(r>190 and b>190 and g<70 for r,g,b in pixels)/len(pixels)
        if p95<.02 or spread<.008:errors.append('Foto hampir kosong/gelap merata: '+name)
        if pink>.02:errors.append('Indikasi material pink: '+name)
        rows.append(dict(shot=name,width=size[0],height=size[1],p95=round(p95,4),variation=round(spread,4),pink=round(pink,4),sha256=digest))
    except (OSError,ValueError) as error:errors.append('Foto tidak dapat dibaca: '+name+' '+str(error))

def periksa(folder,commit):
    folder=Path(folder);errors=[];rows=[];hashes=set()
    provenance=folder/'sumber.txt'
    if not provenance.exists() or not provenance.read_text().splitlines() or provenance.read_text().splitlines()[0]!=commit:
        errors.append('Sumber foto tidak cocok dengan commit yang diuji')
    for name in EXPECTED:
        periksa_satu(folder,name,hashes,errors,rows)
    for extra in sorted(folder.glob('v1_*.png')):
        periksa_satu(folder,extra.stem,hashes,errors,rows)
    return dict(commit=commit,passed=not errors,photos=rows,errors=errors,note='Pemeriksaan teknis gambar; kualitas seni dan kesetaraan visual belum dinilai')

def main():
    p=argparse.ArgumentParser();p.add_argument('--folder',default='Logs/foto_kualitas');p.add_argument('--commit',required=True);p.add_argument('--report',default='Logs/ci/audit_foto.json');a=p.parse_args()
    result=periksa(a.folder,a.commit);report=Path(a.report);report.parent.mkdir(parents=True,exist_ok=True);report.write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n')
    for error in result['errors']:print('[AK][Foto] '+error)
    print('[AK][Foto] Pemeriksaan teknis:', 'LULUS' if result['passed'] else 'GAGAL')
    return 0 if result['passed'] else 1
if __name__=='__main__':raise SystemExit(main())
